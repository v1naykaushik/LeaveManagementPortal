using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LeaveManagementPortal
{
    public partial class EmployeeLeaveHistory : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PopulateYearDropdown();
                LoadLeaveHistory();
            }
        }

        private void PopulateYearDropdown()
        {
            int currentYear = DateTime.Now.Year;
            ddlYear.Items.Add(new ListItem("All Years", ""));

            // Add last 5 years and next 2 years
            for (int year = currentYear - 5; year <= currentYear + 2; year++)
            {
                ddlYear.Items.Add(new ListItem(year.ToString(), year.ToString()));
            }

            // Set current year as default
            ddlYear.SelectedValue = currentYear.ToString();
        }

        private void LoadLeaveHistory()
        {
            string userId = Session["UserID"]?.ToString();
            if (string.IsNullOrEmpty(userId))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Build the query with filters
                string query = @"
                    SELECT 
                        la.LeaveID,
                        lt.LeaveTypeName,
                        la.StartDate,
                        la.EndDate,
                        la.Duration,
                        la.IsHalfDay,
                        la.Status,
                        la.ManagerApprovalStatus,
                        la.ManagerApprovalDate,
                        la.DirectorApprovalStatus,
                        la.DirectorApprovalDate,
                        la.Reason,
                        la.CreatedDate
                    FROM LeaveApplications la
                    INNER JOIN LeaveTypes lt ON la.LeaveTypeID = lt.LeaveTypeID
                    WHERE la.UserID = @UserID";

                // Add Status filter
                if (!string.IsNullOrEmpty(ddlStatus.SelectedValue) && ddlStatus.SelectedValue != "All")
                {
                    query += " AND la.Status = @Status";
                }

                // Add Month filter
                if (!string.IsNullOrEmpty(ddlMonth.SelectedValue))
                {
                    query += " AND MONTH(la.StartDate) = @Month";
                }

                // Add Year filter
                if (!string.IsNullOrEmpty(ddlYear.SelectedValue))
                {
                    query += " AND YEAR(la.StartDate) = @Year";
                }

                // Order by date descending
                query += " ORDER BY la.StartDate DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);

                    if (!string.IsNullOrEmpty(ddlStatus.SelectedValue) && ddlStatus.SelectedValue != "All")
                    {
                        cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue);
                    }

                    if (!string.IsNullOrEmpty(ddlMonth.SelectedValue))
                    {
                        cmd.Parameters.AddWithValue("@Month", Convert.ToInt32(ddlMonth.SelectedValue));
                    }

                    if (!string.IsNullOrEmpty(ddlYear.SelectedValue))
                    {
                        cmd.Parameters.AddWithValue("@Year", Convert.ToInt32(ddlYear.SelectedValue));
                    }

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dtLeaves = new DataTable();
                        adapter.Fill(dtLeaves);

                        rptLeaveHistory.Visible = true;  // Always keep the repeater visible
                        rptLeaveHistory.DataSource = dtLeaves;
                        rptLeaveHistory.DataBind();

                        // Show/hide the no leaves message based on results
                        pnlNoLeaves.Visible = (dtLeaves.Rows.Count == 0);
                    }
                }
            }
        }

        protected void btnApplyFilters_Click(object sender, EventArgs e)
        {
            LoadLeaveHistory();
        }

        protected void btnClearFilters_Click(object sender, EventArgs e)
        {
            ddlStatus.SelectedValue = "All";
            ddlMonth.SelectedValue = "";
            ddlYear.SelectedValue = DateTime.Now.Year.ToString();
            LoadLeaveHistory();
        }

        protected void rptLeaveHistory_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "CancelLeave")
            {
                int leaveId = Convert.ToInt32(e.CommandArgument);
                CancelLeave(leaveId);
            }
        }

        private void CancelLeave(int leaveId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Update leave status to Cancelled
                        using (SqlCommand cmd = new SqlCommand(@"
                            UPDATE LeaveApplications 
                            SET Status = 'Cancelled',
                                LastModifiedDate = GETDATE()
                            WHERE LeaveID = @LeaveID", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@LeaveID", leaveId);
                            cmd.ExecuteNonQuery();
                        }

                        // If this was an approved leave, need to handle sandwich leaves
                        using (SqlCommand cmd = new SqlCommand(@"
                            SELECT Status 
                            FROM LeaveApplications 
                            WHERE LeaveID = @LeaveID", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@LeaveID", leaveId);
                            string status = cmd.ExecuteScalar()?.ToString();

                            if (status == "Approved")
                            {
                                // Cancel any sandwich leaves that reference this leave
                                using (SqlCommand cancelCmd = new SqlCommand(@"
                                    UPDATE LeaveApplications 
                                    SET Status = 'Cancelled',
                                        LastModifiedDate = GETDATE()
                                    WHERE Reason LIKE '%check leave ID ' + CAST(@LeaveID as varchar) + '%'
                                    AND Status = 'Approved'", conn, transaction))
                                {
                                    cancelCmd.Parameters.AddWithValue("@LeaveID", leaveId);
                                    cancelCmd.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                        LoadLeaveHistory(); // Refresh the list
                        ScriptManager.RegisterStartupScript(this, GetType(), "LeaveCancel",
                            "alert('Leave has been cancelled successfully.');", true);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ScriptManager.RegisterStartupScript(this, GetType(), "LeaveCancel",
                            "alert('Error cancelling leave. Please try again.');", true);
                        System.Diagnostics.Debug.WriteLine($"Leave cancellation error: {ex.Message}");
                    }
                }
            }
        }

        protected string FormatDateRange(object startDate, object endDate)
        {
            if (startDate == null || endDate == null) return string.Empty;

            DateTime start = Convert.ToDateTime(startDate);
            DateTime end = Convert.ToDateTime(endDate);

            if (start == end)
            {
                return start.ToString("dd MMM yyyy");
            }
            else
            {
                return $"{start.ToString("dd MMM yyyy")} - {end.ToString("dd MMM yyyy")}";
            }
        }

        protected string FormatDuration(object durationObj, object isHalfDayObj)
        {
            if (durationObj == null) return string.Empty;

            decimal duration = Convert.ToDecimal(durationObj);
            bool isHalfDay = Convert.ToBoolean(isHalfDayObj);

            if (isHalfDay)
            {
                return "Half Day";
            }
            else
            {
                int wholeDays = (int)duration;
                return $"{wholeDays} {(wholeDays == 1 ? "day" : "days")}";
            }
        }

        protected bool CanCancelLeave(string status, object startDateObj)
        {
            if (startDateObj == null) return false;

            // Can only cancel pending or approved leaves
            if (status != "Pending" && status != "Approved")
                return false;

            DateTime startDate = Convert.ToDateTime(startDateObj);

            // Can only cancel future leaves
            return startDate > DateTime.Today;
        }
    }
}