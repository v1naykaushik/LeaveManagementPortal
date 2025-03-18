using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.Script.Serialization;
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

                CacheExistingLeaves();

                GenerateLeaveCalendar();

                // Set minimum date for calendar
                txtStartDate.Attributes["min"] = DateTime.Today.ToString("yyyy-MM-dd");
                txtEndDate.Attributes["min"] = DateTime.Today.ToString("yyyy-MM-dd");
            }
        }

        private List<LeaveInfo> CachedLeaves
        {
            get { return ViewState["CachedLeaves"] as List<LeaveInfo> ?? new List<LeaveInfo>(); }
            set { ViewState["CachedLeaves"] = value; }
        }

        // Class to store leave information
        [Serializable]
        private class LeaveInfo
        {
            public int LeaveID { get; set; }
            public string LeaveType { get; set; }
            public string Status { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }

        private void CacheExistingLeaves()
        {
            string userId = Session["UserID"]?.ToString();
            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        la.LeaveID,
                        lt.LeaveTypeName,
                        la.StartDate,
                        la.EndDate,
                        la.Status
                    FROM LeaveApplications la
                    INNER JOIN LeaveTypes lt ON la.LeaveTypeID = lt.LeaveTypeID
                    WHERE la.UserID = @UserID 
                    AND la.Status IN ('Approved', 'Pending')
                    ORDER BY la.StartDate", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    var leaveList = new List<LeaveInfo>();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            leaveList.Add(new LeaveInfo
                            {
                                LeaveID = Convert.ToInt32(reader["LeaveID"]),
                                LeaveType = reader["LeaveTypeName"].ToString(),
                                StartDate = Convert.ToDateTime(reader["StartDate"]),
                                EndDate = Convert.ToDateTime(reader["EndDate"]),
                                Status = reader["Status"].ToString()
                            });
                        }
                    }
                    // Store in ViewState
                    CachedLeaves = leaveList;
                    System.Diagnostics.Debug.WriteLine($"Cached {leaveList.Count} leaves");
                }
            }
        }

        private void GenerateLeaveCalendar()
        {
            // Get the current year
            int year = DateTime.Now.Year;

            // Get leave data
            List<LeaveInfo> leaveData = CachedLeaves;

            // Create a dictionary to store leave information by date
            Dictionary<DateTime, List<LeaveInfo>> leaveDates = new Dictionary<DateTime, List<LeaveInfo>>();

            // Process leave data
            foreach (LeaveInfo leave in leaveData)
            {
                DateTime startDate = leave.StartDate;
                DateTime endDate = leave.EndDate;

                // Loop through all days in the leave period
                for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    if (!leaveDates.ContainsKey(date))
                    {
                        leaveDates[date] = new List<LeaveInfo>();
                    }

                    leaveDates[date].Add(leave);
                }
            }

            // Generate HTML for the calendar
            StringBuilder calendarHtml = new StringBuilder();

            // Generate a calendar for each month
            for (int month = 1; month <= 12; month++)
            {
                calendarHtml.Append(GenerateMonthCalendar(year, month, leaveDates));
            }

            // Set the HTML
            yearCalendar.InnerHtml = calendarHtml.ToString();
        }

        private string GenerateMonthCalendar(int year, int month, Dictionary<DateTime, List<LeaveInfo>> leaveDates)
        {
            StringBuilder html = new StringBuilder();

            // Start month container
            html.Append("<div class='month-calendar'>");

            // Month header
            html.AppendFormat("<div class='month-header'>{0}</div>", new DateTime(year, month, 1).ToString("MMMM"));

            // Day headers
            html.Append("<div class='month-days'>");
            string[] dayNames = { "Su", "Mo", "Tu", "We", "Th", "Fr", "Sa" };
            foreach (string dayName in dayNames)
            {
                html.AppendFormat("<div class='day-header'>{0}</div>", dayName);
            }

            // Calculate first day of the month
            DateTime firstDay = new DateTime(year, month, 1);

            // Get the day of week for the first day (0 = Sunday, 6 = Saturday)
            int firstDayOfWeek = (int)firstDay.DayOfWeek;

            // Get the number of days in the month
            int daysInMonth = DateTime.DaysInMonth(year, month);

            // Calculate previous month days to display
            DateTime prevMonth = firstDay.AddMonths(-1);
            int daysInPrevMonth = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);

            // Generate calendar days
            int currentDay = 1;
            int nextMonthDay = 1;

            // Total cells needed (max 6 weeks × 7 days)
            for (int i = 0; i < 42; i++)
            {
                // Determine if it's a weekend
                bool isWeekend = i % 7 == 0 || i % 7 == 6; // Sunday or Saturday
                string weekendClass = isWeekend ? "weekend" : "";

                if (i < firstDayOfWeek)
                {
                    // Previous month
                    int prevMonthDay = daysInPrevMonth - firstDayOfWeek + i + 1;
                    html.AppendFormat("<div class='calendar-day {0} other-month'>{1}</div>", weekendClass, prevMonthDay);
                }
                else if (currentDay <= daysInMonth)
                {
                    // Current month
                    DateTime currentDate = new DateTime(year, month, currentDay);
                    string leaveClass = "";
                    string tooltipHtml = "";

                    if (leaveDates.ContainsKey(currentDate))
                    {
                        List<LeaveInfo> leaves = leaveDates[currentDate];
                        LeaveInfo leave = leaves[0]; // Just show the first leave if multiple exist

                        string leaveColor = GetLeaveTypeClass(leave.LeaveType);
                        string statusClass = leave.Status == "Pending" ? "pending-color" : leaveColor;

                        leaveClass = "leave-day " + statusClass;
                        tooltipHtml = $"<span class='leave-day-tooltip'>{leave.LeaveType} ({leave.Status})<br>{leave.StartDate.ToShortDateString()} - {leave.EndDate.ToShortDateString()}</span>";
                    }

                    html.AppendFormat("<div class='calendar-day {0} {1}'>{2}{3}</div>",
                        weekendClass, leaveClass, currentDay, tooltipHtml);
                    currentDay++;
                }
                else
                {
                    // Next month
                    html.AppendFormat("<div class='calendar-day {0} other-month'>{1}</div>", weekendClass, nextMonthDay);
                    nextMonthDay++;
                }

                // End after displaying all days of the current month and completing the row
                if (currentDay > daysInMonth && i % 7 == 6)
                {
                    break;
                }
            }

            html.Append("</div>"); // Close month-days
            html.Append("</div>"); // Close month-calendar

            return html.ToString();
        }

        private string GetLeaveTypeClass(string leaveType)
        {
            switch (leaveType)
            {
                case "CL": return "cl-color";
                case "EL": return "el-color";
                case "ML": return "ml-color";
                case "RL": return "rl-color";
                case "LOP": return "lop-color";
                default: return "";
            }
        }

        private void CheckManagerAssignment()
        {
            string userId = Session["UserID"]?.ToString();
            string userRole = Session["UserRole"]?.ToString();

            // Don't show warning for Director as they don't need a manager
            if (string.IsNullOrEmpty(userId) || userRole?.ToLower() == "director")
                return;

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
            System.Diagnostics.Debug.WriteLine("inside txtStartDate_TextChanged");
            if (string.IsNullOrEmpty(txtStartDate.Text)) return;

            ValidateDateRange();
            // Validate restricted holiday date if applicable
            if (ddlLeaveType.SelectedValue == "4")
            {
                ValidateRestrictedHolidayDate();
            }

            // If end date is empty or it's a restricted leave, set it to start date
            if (string.IsNullOrEmpty(txtEndDate.Text) || ddlLeaveType.SelectedValue == "4")
            {
                txtEndDate.Text = txtStartDate.Text;
            }
        }

        protected void txtEndDate_TextChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("inside txtEndDate_TextChanged");
            ValidateDateRange();
        }

        private void ValidateDateRange()
        {
            if (string.IsNullOrEmpty(txtStartDate.Text) || string.IsNullOrEmpty(txtEndDate.Text))
                return;

            DateTime startDate = DateTime.Parse(txtStartDate.Text);
            DateTime endDate = DateTime.Parse(txtEndDate.Text);

            // Check for leave overlap
            if (!ValidateLeaveOverlap(startDate, endDate))
            {
                lblError.Text = "Selected dates overlap with an existing leave application.";
                txtStartDate.Text = "";
                txtEndDate.Text = "";
                return;
            }

            // Validate dates are in same year
            if (startDate.Year != endDate.Year)
            {
                lblError.Text = "Leave dates must be within the same year.";
                txtEndDate.Text = "";
                return;
            }

            lblError.Text = "";
        }

        private bool ValidateLeaveOverlap(DateTime startDate, DateTime endDate)
        {
            // Use cached leaves instead of querying database again
            foreach (var leave in CachedLeaves)
            {
                if ((startDate >= leave.StartDate && startDate <= leave.EndDate) ||
                    (endDate >= leave.StartDate && endDate <= leave.EndDate) ||
                    (startDate <= leave.StartDate && endDate >= leave.StartDate) ||
                    (leave.StartDate <= startDate && leave.EndDate >= startDate))
                {
                    return false; // Overlap found
                }
            }
            return true; // No overlap
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
            if (leaveTypeId != "5" && leaveTypeId != "3") // Not LOP or Medical vinay note: check this medical rule here
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

            string userRole = Session["UserRole"]?.ToString();
            string initialStatus;
            string managerApprovalStatus;
            string directorApprovalStatus;

            switch (userRole?.ToLower())
            {
                case "director":
                    // Director's leaves are auto-approved at both levels
                    initialStatus = "Approved";
                    managerApprovalStatus = "Approved";
                    directorApprovalStatus = "Approved";
                    break;

                case "manager":
                    // Manager's leaves are auto-approved at manager level only
                    initialStatus = "Pending";
                    managerApprovalStatus = "Approved";
                    directorApprovalStatus = "Pending";
                    break;

                default:
                    // Regular employee leaves start as pending
                    initialStatus = "Pending";
                    managerApprovalStatus = "Pending";
                    directorApprovalStatus = "Pending";
                    break;
            }

            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int newLeaveId = 0;
                        // Insert leave application
                        using (SqlCommand cmd = new SqlCommand(@"
                            DECLARE @NewLeaveID int;

                            INSERT INTO LeaveApplications (
                                UserID, LeaveTypeID, StartDate, EndDate, 
                                Duration, IsHalfDay, Status, 
                                ManagerApprovalStatus, DirectorApprovalStatus, Reason
                            )
                            VALUES (
                                @UserID, @LeaveTypeID, @StartDate, @EndDate,
                                @Duration, @IsHalfDay, @Status,
                                @ManagerApprovalStatus, @DirectorApprovalStatus, @Reason
                            );
    
                            SET @NewLeaveID = SCOPE_IDENTITY();
    
                            -- Double verify this is the correct leave
                            SELECT @NewLeaveID WHERE EXISTS (
                                SELECT 1 FROM LeaveApplications 
                                WHERE LeaveID = @NewLeaveID 
                                AND UserID = @UserID 
                                AND StartDate = @StartDate
                                AND EndDate = @EndDate
                            );", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@UserID", userId);
                            cmd.Parameters.AddWithValue("@LeaveTypeID", leaveTypeId);
                            cmd.Parameters.AddWithValue("@StartDate", startDate);
                            cmd.Parameters.AddWithValue("@EndDate", endDate);
                            cmd.Parameters.AddWithValue("@Duration", duration);
                            cmd.Parameters.AddWithValue("@IsHalfDay", chkHalfDay.Checked);
                            cmd.Parameters.AddWithValue("@Status", initialStatus);
                            cmd.Parameters.AddWithValue("@ManagerApprovalStatus", managerApprovalStatus);
                            cmd.Parameters.AddWithValue("@DirectorApprovalStatus", directorApprovalStatus);
                            if (string.IsNullOrWhiteSpace(txtReason.Text))
                            {
                                cmd.Parameters.AddWithValue("@Reason", DBNull.Value);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@Reason", txtReason.Text.Trim());
                            }

                            var result = cmd.ExecuteScalar();
                            newLeaveId = result != null ? Convert.ToInt32(result) : 0;
                        }

                        // If it's a medical leave, create LOP entry
                        if (leaveTypeId == "3") // Medical Leave
                        {
                            // Calculate LOP duration as half of medical leave duration
                            decimal lopDuration = duration * 0.5m;
                            // LOP leave type is 5
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
                                cmd.Parameters.AddWithValue("@IsHalfDay", chkHalfDay.Checked);
                                cmd.Parameters.AddWithValue("@Status", initialStatus);
                                cmd.Parameters.AddWithValue("@ManagerApprovalStatus", managerApprovalStatus);
                                cmd.Parameters.AddWithValue("@DirectorApprovalStatus", directorApprovalStatus);
                                cmd.Parameters.AddWithValue("@Reason", "System generated LOP. Its Duration is half of Medical leaves applied in same date range.");

                                cmd.ExecuteNonQuery();
                            }

                        }

                        // Process sandwich rule for Director's auto-approved leaves
                        if (userRole?.ToLower() == "director" && newLeaveId != 0)
                        {
                            try
                            {
                                SandwichLeaveManager sandwichManager = new SandwichLeaveManager();
                                sandwichManager.ProcessSandwichRule(newLeaveId);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Sandwich rule processing error for Director's leave: {ex.Message}");
                                // Don't throw the exception as it's not critical to leave creation
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