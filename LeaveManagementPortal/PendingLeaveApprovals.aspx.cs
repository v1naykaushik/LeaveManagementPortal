using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using System.Collections.Generic;

namespace LeaveManagementPortal
{
    public partial class PendingLeaveApprovals : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Verify user role
                string userRole = Session["UserRole"]?.ToString();
                if (userRole != "Director" && userRole != "Manager")
                {
                    Response.Redirect("~/Dashboard.aspx");
                    return;
                }

                LoadPendingLeaves();
            }
        }

        private void LoadPendingLeaves()
        {
            string userRole = Session["UserRole"]?.ToString();
            string userId = Session["UserID"]?.ToString();

            if (string.IsNullOrEmpty(userRole) || string.IsNullOrEmpty(userId))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT 
                        la.LeaveID,
                        u.Name as EmployeeName,
                        u.EmployeeOfficeID as EmployeeId,
                        lt.LeaveTypeName,
                        la.StartDate,
                        la.EndDate,
                        la.Duration,
                        la.IsHalfDay,
                        la.Reason as LeaveReason,
                        la.ManagerApprovalStatus,
                        la.DirectorApprovalStatus
                    FROM LeaveApplications la
                    INNER JOIN Users u ON la.UserID = u.UserID
                    INNER JOIN LeaveTypes lt ON la.LeaveTypeID = lt.LeaveTypeID
                    WHERE 1=1 ";

                if (userRole == "Manager")
                {
                    query += @"AND u.ManagerID = @UserID 
                             AND la.Status = 'Pending'
                             AND la.ManagerApprovalStatus = 'Pending'";
                }
                else // Director
                {
                    query += @"AND la.Status = 'Pending'
                             AND ((la.ManagerApprovalStatus = 'Approved' AND la.DirectorApprovalStatus = 'Pending')
                                  OR (la.DirectorApprovalStatus = 'Pending' AND la.ManagerApprovalStatus = 'Pending'))";
                }

                query += " ORDER BY la.StartDate ASC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (userRole == "Manager")
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                    }

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dtLeaves = new DataTable();
                        adapter.Fill(dtLeaves);

                        if (dtLeaves.Rows.Count > 0)
                        {
                            rptLeaveRequests.DataSource = dtLeaves;
                            rptLeaveRequests.DataBind();
                            pnlNoLeaves.Visible = false;
                        }
                        else
                        {
                            rptLeaveRequests.Visible = false;
                            pnlNoLeaves.Visible = true;
                        }
                    }
                }
            }
        }

        protected void btnSaveChanges_Click(object sender, EventArgs e)
        {
            string actionsJson = hdnPendingActions.Value;
            if (string.IsNullOrEmpty(actionsJson)) return;

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            var data = serializer.Deserialize<Dictionary<string, object>>(actionsJson);

            var pendingActions = serializer.Deserialize<Dictionary<string, string>>(data["pendingActions"].ToString());
            var selectedLeaves = serializer.Deserialize<List<string>>(data["selectedLeaves"].ToString());

            string userRole = Session["UserRole"]?.ToString();
            if (string.IsNullOrEmpty(userRole)) return;

            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (var leaveId in selectedLeaves)
                        {
                            if (pendingActions.ContainsKey(leaveId))
                            {
                                string action = pendingActions[leaveId];
                                UpdateLeaveStatus(int.Parse(leaveId), action, userRole, conn, transaction);
                            }
                        }

                        transaction.Commit();
                        LoadPendingLeaves(); // Refresh the list

                        ScriptManager.RegisterStartupScript(this, GetType(), "SaveSuccess",
                            "alert('Changes saved successfully.'); cancelPendingActions();", true);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ScriptManager.RegisterStartupScript(this, GetType(), "SaveError",
                            "alert('Error saving changes. Please try again.');", true);
                        System.Diagnostics.Debug.WriteLine($"Batch approval error: {ex.Message}");
                    }
                }
            }
        }

        private void UpdateLeaveStatus(int leaveId, string action, string userRole,
            SqlConnection conn, SqlTransaction transaction)
        {
            // Get current leave status
            string currentStatus;
            string managerStatus;
            string directorStatus;
            bool isFullyApproved = false;

            using (SqlCommand cmd = new SqlCommand(
                @"SELECT Status, ManagerApprovalStatus, DirectorApprovalStatus 
                  FROM LeaveApplications 
                  WHERE LeaveID = @LeaveID", conn, transaction))
            {
                cmd.Parameters.AddWithValue("@LeaveID", leaveId);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) return;
                    currentStatus = reader["Status"].ToString();
                    managerStatus = reader["ManagerApprovalStatus"].ToString();
                    directorStatus = reader["DirectorApprovalStatus"].ToString();
                }
            }

            string updateQuery = @"
                UPDATE LeaveApplications 
                SET LastModifiedDate = GETDATE()";

            if (userRole == "Manager")
            {
                updateQuery += @", ManagerApprovalStatus = @ActionStatus";
                if (action == "reject")
                {
                    updateQuery += @", Status = 'Rejected'";
                }
            }
            else if (userRole == "Director")
            {
                updateQuery += @", DirectorApprovalStatus = @ActionStatus";
                if (action == "reject")
                {
                    updateQuery += @", Status = 'Rejected'";
                }
                else if (action == "approve")
                {
                    if (managerStatus == "Approved" || managerStatus == "Pending")
                    {
                        updateQuery += @", Status = 'Approved'";
                        isFullyApproved = true;
                    }
                }
            }

            updateQuery += " WHERE LeaveID = @LeaveID";

            using (SqlCommand cmd = new SqlCommand(updateQuery, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@LeaveID", leaveId);
                cmd.Parameters.AddWithValue("@ActionStatus",
                    action == "approve" ? "Approved" : "Rejected");
                cmd.ExecuteNonQuery();
            }

            // If leave is fully approved, process sandwich rule
            if (isFullyApproved)
            {
                try
                {
                    SandwichLeaveManager sandwichManager = new SandwichLeaveManager();
                    sandwichManager.ProcessSandwichRule(leaveId);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Sandwich rule processing error: {ex.Message}");
                }
            }
        }

        public static string FormatDuration(object durationObj, object isHalfDayObj)
        {
            if (durationObj != null && isHalfDayObj != null)
            {
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
            return string.Empty;
        }
    }
}