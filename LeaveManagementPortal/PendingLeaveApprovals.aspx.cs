using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Linq;

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
            lblDebugHidden.Text = "Hidden field value: " + hdnPendingActions.Value;
        }

        protected void btnTestApprove_Click(object sender, EventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // Modified SQL to handle multiple leave IDs using IN clause
                using (SqlCommand cmd = new SqlCommand(@"
            UPDATE LeaveApplications 
            SET ManagerApprovalStatus = 'Approved',
                LastModifiedDate = GETDATE()
            WHERE LeaveID IN (1, 2, 3)", conn))
                {
                    try
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"Rows affected: {rowsAffected}");

                        LoadPendingLeaves();

                        // Updated message to reflect multiple leaves
                        ScriptManager.RegisterStartupScript(this, GetType(), "UpdateSuccess",
                            "alert('Leave IDs 1, 2, and 3 have been approved successfully.');", true);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error updating leaves: {ex.Message}");
                        ScriptManager.RegisterStartupScript(this, GetType(), "UpdateError",
                            "alert('Error updating leave statuses.');", true);
                    }
                }
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            LoadPendingLeaves(txtSearch.Value.Trim());
        }

        protected void btnClearSearch_Click(object sender, EventArgs e)
        {
            txtSearch.Value = "";
            LoadPendingLeaves();
        }

        private void LoadPendingLeaves(string searchTerm = "")
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
                la.DirectorApprovalStatus,
                la.Status
            FROM LeaveApplications la
            INNER JOIN Users u ON la.UserID = u.UserID
            INNER JOIN LeaveTypes lt ON la.LeaveTypeID = lt.LeaveTypeID
            WHERE 1=1 ";

                // For Manager: Show leaves where
                // 1. Employee reports to this manager
                // 2. Leave is in Pending status
                // 3. Manager approval is still Pending
                // 4. Director hasn't already approved/rejected it

                // Add search condition if search term exists
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query += @" AND (
                        u.Name LIKE @SearchTerm 
                        OR lt.LeaveTypeName LIKE @SearchTerm
                        OR lt.LeaveTypeID LIKE @SearchTerm
                    )";
                }

                if (userRole == "Manager")
                {
                    query += @"
                        AND u.ManagerID = @UserID 
                        AND la.Status = 'Pending'
                        AND la.ManagerApprovalStatus = 'Pending'
                        AND la.DirectorApprovalStatus = 'Pending'";
                }
                // For Director: Show leaves where
                // 1. Leave is in Pending status
                // 2. Either:
                //    a. Manager has approved and Director approval is pending
                //    b. Both approvals are pending (Director can act first)
                else if (userRole == "Director")
                {
                    query += @"
                AND la.Status = 'Pending'
                AND (
                    (la.ManagerApprovalStatus = 'Approved' AND la.DirectorApprovalStatus = 'Pending')
                    OR (la.DirectorApprovalStatus = 'Pending' AND la.ManagerApprovalStatus = 'Pending')
                )";
                }

                query += " ORDER BY la.StartDate ASC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Add search parameter if needed
                    if (!string.IsNullOrWhiteSpace(searchTerm))
                    {
                        cmd.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
                    }

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
                            rptLeaveRequests.Visible = true;
                            pnlNoLeaves.Visible = false;
                        }
                        else
                        {
                            rptLeaveRequests.Visible = false;
                            pnlNoLeaves.Visible = true;
                        }
                    }
                }

                // Log the current state for debugging
                using (SqlCommand debugCmd = new SqlCommand(@"
            SELECT 
                la.LeaveID,
                u.Name,
                la.Status,
                la.ManagerApprovalStatus,
                la.DirectorApprovalStatus
            FROM LeaveApplications la
            INNER JOIN Users u ON la.UserID = u.UserID
            WHERE la.Status = 'Pending'", conn))
                {
                    using (SqlDataReader reader = debugCmd.ExecuteReader())
                    {
                        System.Diagnostics.Debug.WriteLine("\n=== Current Pending Leaves ===");
                        while (reader.Read())
                        {
                            System.Diagnostics.Debug.WriteLine(
                                $"LeaveID: {reader["LeaveID"]}, " +
                                $"Employee: {reader["Name"]}, " +
                                $"Status: {reader["Status"]}, " +
                                $"Manager: {reader["ManagerApprovalStatus"]}, " +
                                $"Director: {reader["DirectorApprovalStatus"]}");
                        }
                    }
                }
            }
        }

        private void LogLeaveStatus(int leaveId, SqlConnection conn, SqlTransaction transaction)
        {
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT 
                    la.LeaveID,
                    la.Status,
                    la.ManagerApprovalStatus,
                    la.DirectorApprovalStatus,
                    u.Name AS EmployeeName,
                    lt.LeaveTypeName,
                    la.StartDate,
                    la.EndDate,
                    la.Duration,
                    la.IsHalfDay,
                    la.Reason
                FROM LeaveApplications la
                INNER JOIN Users u 
                    ON la.UserID = u.UserID
                INNER JOIN LeaveTypes lt 
                    ON la.LeaveTypeID = lt.LeaveTypeID
                WHERE la.LeaveID = @LeaveID", conn, transaction))
            {
                cmd.Parameters.AddWithValue("@LeaveID", leaveId);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        System.Diagnostics.Debug.WriteLine($@"
                        === Leave Status for ID {leaveId} ===
                        Employee: {reader["EmployeeName"]}
                        Leave Type: {reader["LeaveTypeName"]}
                        Status: {reader["Status"]}
                        Manager Approval: {reader["ManagerApprovalStatus"]}
                        Director Approval: {reader["DirectorApprovalStatus"]}
                        Duration: {reader["Duration"]} day(s)
                        Start Date: {Convert.ToDateTime(reader["StartDate"]).ToString("dd-MMM-yyyy")}
                        End Date: {Convert.ToDateTime(reader["EndDate"]).ToString("dd-MMM-yyyy")}
                        Half Day: {reader["IsHalfDay"]}
                        Reason: {reader["Reason"]}
                        ============================");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"No leave found with ID {leaveId}");
                    }
                }
            }
        }

        protected void btnSaveChanges_Click(object sender, EventArgs e)
        {
            string actionsJson = hdnPendingActions.Value;
            string userRole = Session["UserRole"]?.ToString();

            if (string.IsNullOrEmpty(actionsJson))
            {
                System.Diagnostics.Debug.WriteLine("No actions received");
                return;
            }

            try
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                var data = serializer.Deserialize<Dictionary<string, object>>(actionsJson);
                var actions = data["actions"] as Dictionary<string, object>;

                string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            foreach (var action in actions)
                            {
                                // First check if leave is already rejected
                                bool isAlreadyRejected = false;
                                using (SqlCommand checkCmd = new SqlCommand(@"
                            SELECT Status, ManagerApprovalStatus, DirectorApprovalStatus 
                            FROM LeaveApplications 
                            WHERE LeaveID = @LeaveID", conn, transaction))
                                {
                                    checkCmd.Parameters.AddWithValue("@LeaveID", Convert.ToInt32(action.Key));
                                    using (SqlDataReader reader = checkCmd.ExecuteReader())
                                    {
                                        if (reader.Read())
                                        {
                                            isAlreadyRejected =
                                                reader["Status"].ToString() == "Rejected" ||
                                                reader["ManagerApprovalStatus"].ToString() == "Rejected" ||
                                                reader["DirectorApprovalStatus"].ToString() == "Rejected";
                                        }
                                    }
                                }

                                // Skip if already rejected
                                if (isAlreadyRejected)
                                {
                                    continue;
                                }
                                System.Diagnostics.Debug.WriteLine($"Processing action - Role: {userRole}, Action: {action.Value}, LeaveID: {action.Key}");
                                using (SqlCommand cmd = new SqlCommand(@"
                                    UPDATE LeaveApplications 
                                    SET LastModifiedDate = GETDATE(),
                                        Status = CASE 
                                            WHEN @Action = 'reject' THEN 'Rejected'
                                            WHEN @UserRole = 'Director' AND @Action = 'approve' THEN 'Approved'
                                            WHEN @UserRole = 'Manager' AND @Action = 'approve' AND DirectorApprovalStatus = 'Approved' THEN 'Approved'
                                            WHEN @UserRole = 'Manager' AND @Action = 'approve' THEN 'Pending'
                                            ELSE Status
                                        END,
                                        DirectorApprovalStatus = CASE 
                                            WHEN @UserRole = 'Director' THEN 
                                                CASE @Action 
                                                    WHEN 'approve' THEN 'Approved'
                                                    WHEN 'reject' THEN 'Rejected'
                                                    ELSE DirectorApprovalStatus 
                                                END
                                            WHEN @UserRole = 'Manager' AND @Action = 'reject' THEN 'Pending'
                                            ELSE DirectorApprovalStatus 
                                        END,
                                        ManagerApprovalStatus = CASE 
                                            WHEN @UserRole = 'Manager' THEN 
                                                CASE @Action 
                                                    WHEN 'approve' THEN 'Approved'
                                                    WHEN 'reject' THEN 'Rejected'
                                                    ELSE ManagerApprovalStatus 
                                                END
                                            WHEN @UserRole = 'Director' AND ManagerApprovalStatus = 'Pending' THEN 'Pending'
                                            ELSE ManagerApprovalStatus 
                                        END,
                                        -- Insert the date updates here, after all status updates
                                        DirectorApprovalDate = CASE 
                                            WHEN @UserRole = 'Director' AND (@Action = 'approve' OR @Action = 'reject') THEN GETDATE()
                                            ELSE DirectorApprovalDate 
                                        END,
                                        ManagerApprovalDate = CASE 
                                            WHEN @UserRole = 'Manager' AND (@Action = 'approve' OR @Action = 'reject') THEN GETDATE()
                                            ELSE ManagerApprovalDate 
                                        END
                                    WHERE LeaveID = @LeaveID", conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@Action", action.Value.ToString());
                                    cmd.Parameters.AddWithValue("@LeaveID", Convert.ToInt32(action.Key));
                                    cmd.Parameters.AddWithValue("@UserRole", userRole);

                                    int rowsAffected = cmd.ExecuteNonQuery();
                                    using (SqlCommand verifyCmd = new SqlCommand(@"
                                        SELECT ManagerApprovalDate, DirectorApprovalDate 
                                        FROM LeaveApplications 
                                        WHERE LeaveID = @LeaveID", conn, transaction))
                                    {
                                        verifyCmd.Parameters.AddWithValue("@LeaveID", Convert.ToInt32(action.Key));
                                        using (SqlDataReader reader = verifyCmd.ExecuteReader())
                                        {
                                            if (reader.Read())
                                            {
                                                System.Diagnostics.Debug.WriteLine($"After update - ManagerApprovalDate: {reader["ManagerApprovalDate"]}, DirectorApprovalDate: {reader["DirectorApprovalDate"]}");
                                            }
                                        }
                                    }
                                    System.Diagnostics.Debug.WriteLine($"Leave {action.Key} updated. Rows affected: {rowsAffected}");

                                    // Check for sandwich rule only if it's an approval and action succeeded
                                    if (rowsAffected > 0 && action.Value.ToString() == "approve")
                                    {
                                        CheckAndProcessSandwichRule(Convert.ToInt32(action.Key), conn, transaction);
                                    }
                                }
                            }

                            transaction.Commit();

                            // Process any pending sandwich leaves after transaction commits
                            if (System.Web.HttpContext.Current.Session["PendingSandwichLeaveId"] != null)
                            {
                                try
                                {
                                    int sandwichLeaveId = (int)System.Web.HttpContext.Current.Session["PendingSandwichLeaveId"];
                                    SandwichLeaveManager sandwichManager = new SandwichLeaveManager();
                                    sandwichManager.ProcessSandwichRule(sandwichLeaveId);
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Sandwich rule processing error: {ex.Message}");
                                }
                                finally
                                {
                                    System.Web.HttpContext.Current.Session.Remove("PendingSandwichLeaveId");
                                }
                            }

                            LoadPendingLeaves();
                            ScriptManager.RegisterStartupScript(this, GetType(), "UpdateSuccess",
                                "alert('Leave status updated successfully.'); clearSelected();", true);
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            System.Diagnostics.Debug.WriteLine($"Transaction error: {ex.Message}");
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                ScriptManager.RegisterStartupScript(this, GetType(), "UpdateError",
                    "alert('Error updating leave statuses.');", true);
            }
        }

        private void CheckAndProcessSandwichRule(int leaveId, SqlConnection conn, SqlTransaction transaction)
        {
            // First check if the leave is fully approved using the current transaction
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT 
                    LA.Status,
                    LA.ManagerApprovalStatus,
                    LA.DirectorApprovalStatus,
                    LA.UserID,
                    LA.StartDate,
                    LA.EndDate
                FROM LeaveApplications LA
                WHERE LA.LeaveID = @LeaveID
                AND LA.Status = 'Approved'
                AND LA.ManagerApprovalStatus = 'Approved'
                AND LA.DirectorApprovalStatus = 'Approved'", conn, transaction))
            {
                cmd.Parameters.AddWithValue("@LeaveID", leaveId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // If we found a fully approved leave, process it after the current transaction completes
                        var userId = Convert.ToInt32(reader["UserID"]);
                        var startDate = Convert.ToDateTime(reader["StartDate"]);
                        var endDate = Convert.ToDateTime(reader["EndDate"]);

                        // Store these values to process after transaction commits
                        System.Web.HttpContext.Current.Session["PendingSandwichLeaveId"] = leaveId;
                    }
                }
            }
        }

        private void UpdateLeaveStatus(int leaveId, string action, string userRole,
        SqlConnection conn, SqlTransaction transaction)
        {
            System.Diagnostics.Debug.WriteLine($"\nUpdating Leave {leaveId}");
            System.Diagnostics.Debug.WriteLine($"Action: {action}, UserRole: {userRole}");

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
                    if (!reader.Read())
                    {
                        System.Diagnostics.Debug.WriteLine("Leave not found!");
                        return;
                    }
                    currentStatus = reader["Status"].ToString();
                    managerStatus = reader["ManagerApprovalStatus"].ToString();
                    directorStatus = reader["DirectorApprovalStatus"].ToString();

                    System.Diagnostics.Debug.WriteLine($"Current Status: {currentStatus}");
                    System.Diagnostics.Debug.WriteLine($"Current Manager Status: {managerStatus}");
                    System.Diagnostics.Debug.WriteLine($"Current Director Status: {directorStatus}");
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
                else if (action == "approve" && directorStatus == "Approved")
                {
                    updateQuery += @", Status = 'Approved'";
                    isFullyApproved = true;
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
            System.Diagnostics.Debug.WriteLine($"Update Query: {updateQuery}");

            using (SqlCommand cmd = new SqlCommand(updateQuery, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@LeaveID", leaveId);
                cmd.Parameters.AddWithValue("@ActionStatus",
                    action == "approve" ? "Approved" : "Rejected");

                int rowsAffected = cmd.ExecuteNonQuery();
                System.Diagnostics.Debug.WriteLine($"Rows affected: {rowsAffected}");
            }

            // Verify the update
            using (SqlCommand verifyCmd = new SqlCommand(
                @"SELECT Status, ManagerApprovalStatus, DirectorApprovalStatus 
              FROM LeaveApplications 
              WHERE LeaveID = @LeaveID", conn, transaction))
            {
                verifyCmd.Parameters.AddWithValue("@LeaveID", leaveId);
                using (SqlDataReader reader = verifyCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        System.Diagnostics.Debug.WriteLine("\nAfter Update:");
                        System.Diagnostics.Debug.WriteLine($"Status: {reader["Status"]}");
                        System.Diagnostics.Debug.WriteLine($"Manager Status: {reader["ManagerApprovalStatus"]}");
                        System.Diagnostics.Debug.WriteLine($"Director Status: {reader["DirectorApprovalStatus"]}");
                    }
                }
            }

            // If leave is fully approved, process sandwich rule
            if (isFullyApproved)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("Processing sandwich rule...");
                    SandwichLeaveManager sandwichManager = new SandwichLeaveManager();
                    sandwichManager.ProcessSandwichRule(leaveId);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Sandwich rule processing error: {ex.Message}");
                    throw;
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