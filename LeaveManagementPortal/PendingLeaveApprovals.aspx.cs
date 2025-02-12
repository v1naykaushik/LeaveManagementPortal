﻿using System;
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
            System.Diagnostics.Debug.WriteLine($"Received JSON: {actionsJson}");

            if (string.IsNullOrEmpty(actionsJson))
            {
                System.Diagnostics.Debug.WriteLine("No actions received");
                return;
            }

            try
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                var data = serializer.Deserialize<Dictionary<string, object>>(actionsJson);

                // Directly cast the actions object instead of trying to deserialize it again
                var actions = data["actions"] as Dictionary<string, object>;
                System.Diagnostics.Debug.WriteLine($"Actions dictionary: {string.Join(", ", actions.Select(a => $"{a.Key}:{a.Value}"))}");

                string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    foreach (var action in actions)
                    {
                        using (SqlCommand cmd = new SqlCommand(@"
                            UPDATE LeaveApplications 
                            SET ManagerApprovalStatus = @Status,
                                LastModifiedDate = GETDATE()
                            WHERE LeaveID = @LeaveID", conn))
                        {
                            cmd.Parameters.AddWithValue("@Status", action.Value.ToString() == "approve" ? "Approved" : "Rejected");
                            cmd.Parameters.AddWithValue("@LeaveID", Convert.ToInt32(action.Key));

                            int rowsAffected = cmd.ExecuteNonQuery();
                            System.Diagnostics.Debug.WriteLine($"Leave {action.Key} status set to {action.Value}, rows affected: {rowsAffected}");
                        }
                    }

                    LoadPendingLeaves();
                    ScriptManager.RegisterStartupScript(this, GetType(), "UpdateSuccess",
                        "alert('Selected leaves have been updated successfully.');", true);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                ScriptManager.RegisterStartupScript(this, GetType(), "UpdateError",
                    "alert('Error updating leave statuses.');", true);
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