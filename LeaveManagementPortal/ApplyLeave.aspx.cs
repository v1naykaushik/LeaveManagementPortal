using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LeaveManagementPortal
{
    public partial class ApplyLeave : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Check if user has a manager
                CheckManagerAssignment();

                // Load leave types with balances
                LoadLeaveTypes();

                // Set minimum date for calendar
                txtStartDate.Attributes["min"] = DateTime.Today.ToString("yyyy-MM-dd");
                txtEndDate.Attributes["min"] = DateTime.Today.ToString("yyyy-MM-dd");
            }
        }

        private void CheckManagerAssignment()
        {
            string userId = Session["UserID"]?.ToString();
            if (string.IsNullOrEmpty(userId)) return;

            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT ManagerID FROM Users WHERE UserID = @UserID", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    object result = cmd.ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                    {
                        pnlNoManager.Visible = true;
                    }
                }
            }
        }

        private void LoadLeaveTypes()
        {
            string userId = Session["UserID"]?.ToString();
            if (string.IsNullOrEmpty(userId)) return;

            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
                    WITH LeaveUsage AS (
                        SELECT 
                            LeaveTypeID,
                            SUM(Duration) as UsedDuration
                        FROM LeaveApplications
                        WHERE UserID = @UserID
                        AND Status IN ('Approved', 'Pending')
                        AND YEAR(StartDate) = YEAR(GETDATE())
                        GROUP BY LeaveTypeID
                    )
                    SELECT 
                        lt.LeaveTypeID, 
                        lt.LeaveTypeName, 
                        lb.PresentYearBalance as TotalBalance,
                        COALESCE(lu.UsedDuration, 0) as UsedLeaves,
                        lb.PresentYearBalance - COALESCE(lu.UsedDuration, 0) as AvailableBalance
                    FROM LeaveTypes lt
                    LEFT JOIN LeaveBalances lb ON lt.LeaveTypeID = lb.LeaveTypeID
                    LEFT JOIN LeaveUsage lu ON lt.LeaveTypeID = lu.LeaveTypeID
                    WHERE lb.UserID = @UserID
                    ORDER BY lt.LeaveTypeName", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);

                    ddlLeaveType.Items.Clear();
                    ddlLeaveType.Items.Add(new ListItem("Select Leave Type", ""));

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string leaveTypeId = reader["LeaveTypeID"].ToString();
                            string leaveTypeName = reader["LeaveTypeName"].ToString();
                            decimal totalBalance = Convert.ToDecimal(reader["TotalBalance"]);
                            // totalBalance is the balance that you see in current year balance in db
                            decimal availableBalance = Convert.ToDecimal(reader["AvailableBalance"]);
                            // availableBalance is the balance that you get after reducing pending leaves from totalBalance

                            ListItem item = new ListItem(
                                $"{leaveTypeName} (Available: {availableBalance})",
                                leaveTypeId
                            );

                            if (leaveTypeId == "5")          /* LOP leaveTypeId is 5 */
                            {
                                item = new ListItem(
                                $"{leaveTypeName} ",
                                leaveTypeId);
                            }

                                if (availableBalance <= 0 && leaveTypeId != "5") // Allow LOP even with 0 balance
                            {
                                item.Enabled = false;
                            }

                            ddlLeaveType.Items.Add(item);
                        }
                    }
                }
            }
        }

        protected void ddlLeaveType_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedLeaveType = ddlLeaveType.SelectedValue;
            lblError.Text = ""; // Clear any existing error messages

            // Show/hide half day option for CL only
            chkHalfDay.Visible = selectedLeaveType == "1"; // CL
            if (!chkHalfDay.Visible)
            {
                chkHalfDay.Checked = false;
            }

            // Show/hide medical leave info
            pnlMedicalLeaveInfo.Visible = selectedLeaveType == "3"; // Medical Leave

            // Show/hide restricted leave calendar link and manage end date
            pnlRestrictedLeave.Visible = selectedLeaveType == "4"; // RL
            if (selectedLeaveType == "4") // RL
            {
                txtEndDate.Text = txtStartDate.Text;
                txtEndDate.Enabled = false;
            }
            else
            {
                txtEndDate.Enabled = true;
            }

            // Validate date if it's a restricted leave
            if (selectedLeaveType == "4" && !string.IsNullOrEmpty(txtStartDate.Text))
            {
                ValidateRestrictedHolidayDate();
            }
        }

        private void ValidateRestrictedHolidayDate()
        {
            if (ddlLeaveType.SelectedValue == "4" && !string.IsNullOrEmpty(txtStartDate.Text))
            {
                DateTime startDate = DateTime.Parse(txtStartDate.Text);
                
                if (!IsRestrictedHoliday(startDate))
                {
                    lblError.Text = $"{startDate.ToString("dd-MMMM-yyyy")} is not a restricted holiday.";
                    txtStartDate.Text = "";
                }
                else
                {
                    lblError.Text = "";
                }
            }
        }

        protected void txtStartDate_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtStartDate.Text)) return;

            // Validate restricted holiday date if applicable
            if (ddlLeaveType.SelectedValue == "4")
            {
                ValidateRestrictedHolidayDate();
            }
            else
            {
                ValidateDateRange();
            }

            // If end date is empty or it's a restricted leave, set it to start date
            if (string.IsNullOrEmpty(txtEndDate.Text) || ddlLeaveType.SelectedValue == "4")
            {
                txtEndDate.Text = txtStartDate.Text;
            }
        }

        protected void txtEndDate_TextChanged(object sender, EventArgs e)
        {
            ValidateDateRange();
        }

        private void ValidateDateRange()
        {
            if (string.IsNullOrEmpty(txtStartDate.Text) || string.IsNullOrEmpty(txtEndDate.Text))
                return;

            DateTime startDate = DateTime.Parse(txtStartDate.Text);
            DateTime endDate = DateTime.Parse(txtEndDate.Text);

            // Validate dates are in same year
            if (startDate.Year != endDate.Year)
            {
                lblError.Text = "Leave dates must be within the same year.";
                txtEndDate.Text = "";
                return;
            }

            // Check for leave overlap
            if (!ValidateLeaveOverlap(startDate, endDate))
            {
                lblError.Text = "Selected dates overlap with an existing leave application.";
                txtEndDate.Text = "";
                return;
            }

            lblError.Text = "";
        }

        private bool ValidateLeaveOverlap(DateTime startDate, DateTime endDate)
        {
            string userId = Session["UserID"]?.ToString();
            if (string.IsNullOrEmpty(userId)) return false;

            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT COUNT(*)
                    FROM LeaveApplications
                    WHERE UserID = @UserID
                    AND Status NOT IN ('Cancelled', 'Rejected')
                    AND (
                        (@StartDate BETWEEN StartDate AND EndDate)
                        OR (@EndDate BETWEEN StartDate AND EndDate)
                        OR (StartDate BETWEEN @StartDate AND @EndDate)
                        OR (EndDate BETWEEN @StartDate AND @EndDate)
                    )", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);

                    int overlapCount = (int)cmd.ExecuteScalar();
                    return overlapCount == 0;
                }
            }
        }

        protected void chkHalfDay_CheckedChanged(object sender, EventArgs e)
        {
            if (chkHalfDay.Checked)
            {
                // For half day, end date must be same as start date
                txtEndDate.Text = txtStartDate.Text;
                txtEndDate.Enabled = false;
            }
            else
            {
                txtEndDate.Enabled = true;
            }
        }

        protected void cvStartDate_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (DateTime.TryParse(args.Value, out DateTime startDate))
            {
                args.IsValid = startDate >= DateTime.Today;
            }
            else
            {
                args.IsValid = false;
            }
        }

        protected void cvEndDate_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (string.IsNullOrEmpty(txtStartDate.Text)) return;

            if (DateTime.TryParse(args.Value, out DateTime endDate) &&
                DateTime.TryParse(txtStartDate.Text, out DateTime startDate))
            {
                args.IsValid = endDate.Year == startDate.Year && endDate >= startDate;
            }
            else
            {
                args.IsValid = false;
            }
        }

        protected void btnApplyLeave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            string userId = Session["UserID"]?.ToString();
            if (string.IsNullOrEmpty(userId))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            DateTime startDate = DateTime.Parse(txtStartDate.Text);
            DateTime endDate = DateTime.Parse(txtEndDate.Text);
            string leaveTypeId = ddlLeaveType.SelectedValue;
            decimal duration;

            // Calculate duration
            if (chkHalfDay.Checked)
            {
                duration = 0.5m;
            }
            else
            {
                duration = (decimal)(endDate - startDate).TotalDays + 1;
            }

            // For Medical Leave, we don't need to check LOP balance as it's handled automatically
            if (leaveTypeId != "5" && leaveTypeId != "3") // Not LOP or Medical vinay. check this medical rule here
            {
                // Check if user has sufficient balance
                if (!HasSufficientBalance(userId, leaveTypeId, duration))
                {
                    lblError.Text = "Insufficient leave balance.";
                    return;
                }
            }

            // For Restricted Leave, validate if selected date is a restricted holiday
            if (leaveTypeId == "4") // RL
            {
                if (!IsRestrictedHoliday(startDate))
                {
                    lblError.Text = "Selected date is not a restricted holiday.";
                    return;
                }
            }   

            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Insert leave application
                        using (SqlCommand cmd = new SqlCommand(@"
                            INSERT INTO LeaveApplications (
                                UserID, LeaveTypeID, StartDate, EndDate, 
                                Duration, IsHalfDay, Status, 
                                ManagerApprovalStatus, DirectorApprovalStatus, Reason
                            )
                            VALUES (
                                @UserID, @LeaveTypeID, @StartDate, @EndDate,
                                @Duration, @IsHalfDay, 'Pending',
                                'Pending', 'Pending', @Reason
                            )", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@UserID", userId);
                            cmd.Parameters.AddWithValue("@LeaveTypeID", leaveTypeId);
                            cmd.Parameters.AddWithValue("@StartDate", startDate);
                            cmd.Parameters.AddWithValue("@EndDate", endDate);
                            cmd.Parameters.AddWithValue("@Duration", duration);
                            cmd.Parameters.AddWithValue("@IsHalfDay", chkHalfDay.Checked);
                            if (string.IsNullOrWhiteSpace(txtReason.Text))
                            {
                                cmd.Parameters.AddWithValue("@Reason", DBNull.Value);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@Reason", txtReason.Text.Trim());
                            }

                            cmd.ExecuteNonQuery();
                        }

                        // If it's a medical leave, create LOP entry
                        if (leaveTypeId == "3") // Medical Leave
                        {
                            // Calculate LOP duration as half of medical leave duration
                            decimal lopDuration = duration * 0.5m;

                            using (SqlCommand cmd = new SqlCommand(@"
                                INSERT INTO LeaveApplications (
                                    UserID, LeaveTypeID, StartDate, EndDate,
                                    Duration, IsHalfDay, Status,
                                    ManagerApprovalStatus, DirectorApprovalStatus , Reason
                                )
                                VALUES (
                                    @UserID, '5', @StartDate, @EndDate,
                                    @LopDuration, 0, 'Pending',
                                    'Pending', 'Pending', @Reason
                                )", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@UserID", userId);
                                cmd.Parameters.AddWithValue("@StartDate", startDate);
                                cmd.Parameters.AddWithValue("@EndDate", endDate);
                                cmd.Parameters.AddWithValue("@LopDuration", lopDuration);
                                cmd.Parameters.AddWithValue("@Reason", "Auto generated LOP. Its Duration is half of Medical leaves applied in same date range.");

                                cmd.ExecuteNonQuery();
                            }

                        }
                            transaction.Commit();

                        // Clear form and show success message
                        ddlLeaveType.SelectedIndex = 0;
                        txtStartDate.Text = "";
                        txtEndDate.Text = "";
                        chkHalfDay.Checked = false;
                        lblError.Text = "";
                        txtReason.Text = "";

                        // Refresh leave balances
                        LoadLeaveTypes();

                        ScriptManager.RegisterStartupScript(this, GetType(), "LeaveApplied",
                            "alert('Leave application submitted successfully.');", true);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        lblError.Text = "An error occurred while submitting your leave application.";
                        System.Diagnostics.Debug.WriteLine($"Leave application error: {ex.Message}");
                    }
                }
            }
        }
        
        private bool HasSufficientBalance(string userId, string leaveTypeId, decimal duration)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
                    WITH LeaveUsage AS (
                        SELECT SUM(Duration) as UsedDuration
                        FROM LeaveApplications
                        WHERE UserID = @UserID
                        AND LeaveTypeID = @LeaveTypeID
                        AND Status IN ('Approved', 'Pending')
                        AND YEAR(StartDate) = YEAR(GETDATE())
                    )
                    SELECT 
                        lb.PresentYearBalance - COALESCE(lu.UsedDuration, 0) as AvailableBalance
                    FROM LeaveBalances lb
                    LEFT JOIN LeaveUsage lu ON 1=1
                    WHERE lb.UserID = @UserID 
                    AND lb.LeaveTypeID = @LeaveTypeID", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@LeaveTypeID", leaveTypeId);

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        decimal availableBalance = Convert.ToDecimal(result);
                        return availableBalance >= duration;
                    }
                }
            }
            return false;
        }

        private bool IsRestrictedHoliday(DateTime date)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT COUNT(*)
                    FROM RestrictedHolidays
                    WHERE HolidayDate = @Date", conn))
                {
                    cmd.Parameters.AddWithValue("@Date", date.Date);
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }
    }
}