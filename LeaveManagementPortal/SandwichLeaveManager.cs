using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace LeaveManagementPortal
{
    public class SandwichLeaveManager
    {
        private readonly string connectionString;

        public SandwichLeaveManager()
        {
            connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
        }

        public void ProcessSandwichRule(int leaveId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            var leaveDetails = GetLeaveDetails(leaveId, conn, transaction);
                            if (leaveDetails == null) return;

                            // If leave is cancelled, handle sandwich leave cancellation
                            if (leaveDetails.Status == "Cancelled")
                            {
                                CancelRelatedSandwichLeaves(leaveDetails, conn, transaction);
                                transaction.Commit();
                                return;
                            }

                            // Only proceed if leave is fully approved
                            if (!IsLeaveFullyApproved(leaveDetails)) return;

                            var complementaryLeaves = FindComplementaryLeaves(leaveDetails, conn, transaction);
                            if (!complementaryLeaves.Any()) return;

                            var balances = GetAvailableBalances(leaveDetails.UserId, conn, transaction);

                            foreach (var sandwichPeriod in complementaryLeaves)
                            {
                                CreateSandwichLeavesForPeriod(
                                    leaveDetails.UserId,
                                    sandwichPeriod,
                                    leaveDetails.LeaveId,
                                    sandwichPeriod.Leave.LeaveId,
                                    conn,
                                    transaction
                                );
                            }

                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            string errorMessage = $"Error processing sandwich leave for LeaveID {leaveId}. " +
                                                 $"Details: {ex.Message}";
                            System.Diagnostics.Debug.WriteLine(errorMessage);
                            throw new Exception(errorMessage, ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Sandwich leave error: {ex.Message}");
                throw;
            }
        }

        private LeaveDetails GetLeaveDetails(int leaveId, SqlConnection conn, SqlTransaction transaction)
        {
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT UserID, StartDate, EndDate, LeaveTypeID, Status,
                       ManagerApprovalStatus, DirectorApprovalStatus
                FROM LeaveApplications 
                WHERE LeaveID = @LeaveID", conn, transaction))
            {
                cmd.Parameters.AddWithValue("@LeaveID", leaveId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new LeaveDetails
                        {
                            LeaveId = leaveId,
                            UserId = Convert.ToInt32(reader["UserID"]),
                            StartDate = Convert.ToDateTime(reader["StartDate"]),
                            EndDate = Convert.ToDateTime(reader["EndDate"]),
                            LeaveTypeId = reader["LeaveTypeID"].ToString(),
                            Status = reader["Status"].ToString(),
                            ManagerApprovalStatus = reader["ManagerApprovalStatus"].ToString(),
                            DirectorApprovalStatus = reader["DirectorApprovalStatus"].ToString()
                        };
                    }
                }
            }
            return null;
        }

        private bool IsLeaveFullyApproved(LeaveDetails leave)
        {
            return leave.Status == "Approved" &&
                   leave.ManagerApprovalStatus == "Approved" &&
                   leave.DirectorApprovalStatus == "Approved";
        }

        private List<(LeaveDetails Leave, DateTime SandwichStart, DateTime SandwichEnd)> FindComplementaryLeaves(
            LeaveDetails currentLeave, SqlConnection conn, SqlTransaction transaction)
        {
            var results = new List<(LeaveDetails Leave, DateTime SandwichStart, DateTime SandwichEnd)>();

            // Check for Friday at the start of leave range
            if (currentLeave.StartDate.DayOfWeek == DayOfWeek.Monday)
            {
                var previousFridayLeave = FindLeaveOnDate(
                    currentLeave.UserId,
                    currentLeave.StartDate.AddDays(-3), // Previous Friday
                    true, // isEndDate
                    conn,
                    transaction
                );

                if (previousFridayLeave != null)
                {
                    results.Add((
                        previousFridayLeave,
                        currentLeave.StartDate.AddDays(-2), // Saturday
                        currentLeave.StartDate.AddDays(-1)  // Sunday
                    ));
                }
            }

            // Check for Monday after the end of leave range
            if (currentLeave.EndDate.DayOfWeek == DayOfWeek.Friday)
            {
                var nextMondayLeave = FindLeaveOnDate(
                    currentLeave.UserId,
                    currentLeave.EndDate.AddDays(3), // Next Monday
                    false, // isStartDate
                    conn,
                    transaction
                );

                if (nextMondayLeave != null)
                {
                    results.Add((
                        nextMondayLeave,
                        currentLeave.EndDate.AddDays(1), // Saturday
                        currentLeave.EndDate.AddDays(2)  // Sunday
                    ));
                }
            }

            return results;
        }

        private LeaveDetails FindLeaveOnDate(
            int userId,
            DateTime dateToCheck,
            bool isEndDate,
            SqlConnection conn,
            SqlTransaction transaction)
        {
            string dateCondition = isEndDate ? "EndDate = @DateToCheck" : "StartDate = @DateToCheck";

            using (SqlCommand cmd = new SqlCommand($@"
                SELECT LeaveID, UserID, StartDate, EndDate, LeaveTypeID,
                       Status, ManagerApprovalStatus, DirectorApprovalStatus
                FROM LeaveApplications
                WHERE UserID = @UserID
                AND {dateCondition}
                AND Status = 'Approved'
                AND ManagerApprovalStatus = 'Approved'
                AND DirectorApprovalStatus = 'Approved'", conn, transaction))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@DateToCheck", dateToCheck);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new LeaveDetails
                        {
                            LeaveId = Convert.ToInt32(reader["LeaveID"]),
                            UserId = Convert.ToInt32(reader["UserID"]),
                            StartDate = Convert.ToDateTime(reader["StartDate"]),
                            EndDate = Convert.ToDateTime(reader["EndDate"]),
                            LeaveTypeId = reader["LeaveTypeID"].ToString(),
                            Status = reader["Status"].ToString(),
                            ManagerApprovalStatus = reader["ManagerApprovalStatus"].ToString(),
                            DirectorApprovalStatus = reader["DirectorApprovalStatus"].ToString()
                        };
                    }
                }
            }
            return null;
        }

        private void CancelRelatedSandwichLeaves(LeaveDetails cancelledLeave, SqlConnection conn, SqlTransaction transaction)
        {
            // Find sandwich leaves that reference this leave ID
            using (SqlCommand cmd = new SqlCommand(@"
                UPDATE LeaveApplications 
                SET Status = 'Cancelled',
                    LastModifiedDate = GETDATE()
                WHERE Reason LIKE '%check leave ID ' + CAST(@LeaveID as varchar) + '%'
                AND Status = 'Approved'", conn, transaction))
            {
                cmd.Parameters.AddWithValue("@LeaveID", cancelledLeave.LeaveId);
                try
                {
                    int rowsAffected = cmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine(
                        $"Cancelled {rowsAffected} sandwich leaves related to leave ID {cancelledLeave.LeaveId}");
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        $"Failed to cancel sandwich leaves for leave ID {cancelledLeave.LeaveId}", ex);
                }
            }
        }

        private bool IsHoliday(DateTime date, SqlConnection conn, SqlTransaction transaction)
        {
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT COUNT(1) 
                FROM Holidays 
                WHERE HolidayDate = @Date", conn, transaction))
            {
                cmd.Parameters.AddWithValue("@Date", date);
                return ((int)cmd.ExecuteScalar()) > 0;
            }
        }

        private LeaveBalances GetAvailableBalances(int userId, SqlConnection conn, SqlTransaction transaction)
        {
            var balances = new LeaveBalances();

            using (SqlCommand cmd = new SqlCommand(@"
                WITH LeaveUsage AS (
                    SELECT LeaveTypeID, SUM(Duration) as UsedDuration
                    FROM LeaveApplications
                    WHERE UserID = @UserID
                    AND Status IN ('Approved', 'Pending')
                    AND YEAR(StartDate) = YEAR(GETDATE())
                    GROUP BY LeaveTypeID
                )
                SELECT 
                    lb.LeaveTypeID,
                    lb.PresentYearBalance - ISNULL(lu.UsedDuration, 0) as AvailableBalance
                FROM LeaveBalances lb
                LEFT JOIN LeaveUsage lu ON lb.LeaveTypeID = lu.LeaveTypeID
                WHERE lb.UserID = @UserID", conn, transaction))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string leaveTypeId = reader["LeaveTypeID"].ToString();
                        decimal balance = Convert.ToDecimal(reader["AvailableBalance"]);

                        switch (leaveTypeId)
                        {
                            case "1": // CL
                                balances.CasualLeaveBalance = balance;
                                break;
                            case "2": // EL
                                balances.EarnedLeaveBalance = balance;
                                break;
                        }
                    }
                }
            }

            return balances;
        }

        private string DetermineLeaveType(LeaveBalances balances, bool isFirstDay)
        {
            if (isFirstDay)
            {
                if (balances.CasualLeaveBalance >= 1) return "1"; // CL
                if (balances.EarnedLeaveBalance >= 1) return "2"; // EL
                return "5"; // LOP
            }
            else
            {
                // For second day, prefer CL only if we didn't use it for first day and it's available
                if (balances.CasualLeaveBalance >= 1) return "1"; // CL
                if (balances.EarnedLeaveBalance >= 1) return "2"; // EL
                return "5"; // LOP
            }
        }

        private void CreateSandwichLeavesForPeriod(
            int userId,
            (LeaveDetails Leave, DateTime SandwichStart, DateTime SandwichEnd) sandwichPeriod,
            int leaveId1,
            int leaveId2,
            SqlConnection conn,
            SqlTransaction transaction)
        {
            // Check if Saturday is a holiday
            bool isSaturdayHoliday = IsHoliday(sandwichPeriod.SandwichStart, conn, transaction);
            bool isSundayHoliday = IsHoliday(sandwichPeriod.SandwichEnd, conn, transaction);

            // Skip entire process if both days are holidays
            if (isSaturdayHoliday && isSundayHoliday) return;

            string reason = $"Sandwich leave: check leave ID {leaveId1} and {leaveId2}";

            // Create leave for Saturday if it's not a holiday
            if (!isSaturdayHoliday)
            {
                var currentBalances = GetAvailableBalances(userId, conn, transaction);
                string saturdayLeaveType = DetermineLeaveType(currentBalances, true);
                CreateSingleDayLeave(
                    userId,
                    sandwichPeriod.SandwichStart,
                    saturdayLeaveType,
                    reason,
                    conn,
                    transaction
                );

            }

            // Create leave for Sunday if it's not a holiday
            if (!isSundayHoliday)
            {
                var currentBalances = GetAvailableBalances(userId, conn, transaction);
                string sundayLeaveType = DetermineLeaveType(currentBalances, false);
                CreateSingleDayLeave(
                    userId,
                    sandwichPeriod.SandwichEnd,
                    sundayLeaveType,
                    reason,
                    conn,
                    transaction
                );
            }
        }

        private void CreateSingleDayLeave(
            int userId,
            DateTime leaveDate,
            string leaveTypeId,
            string reason,
            SqlConnection conn,
            SqlTransaction transaction)
        {
            using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO LeaveApplications (
                    UserID, LeaveTypeID, StartDate, EndDate, Duration,
                    IsHalfDay, Status, ManagerApprovalStatus, DirectorApprovalStatus,
                    Reason, CreatedDate, LastModifiedDate
                )
                VALUES (
                    @UserID, @LeaveTypeID, @LeaveDate, @LeaveDate, 1,
                    0, 'Approved', 'Approved', 'Approved',
                    @Reason, GETDATE(), GETDATE()
                )", conn, transaction))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@LeaveTypeID", leaveTypeId);
                cmd.Parameters.AddWithValue("@LeaveDate", leaveDate);
                cmd.Parameters.AddWithValue("@Reason", reason);

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        $"Failed to create sandwich leave for date {leaveDate:yyyy-MM-dd} " +
                        $"using leave type {leaveTypeId}. User ID: {userId}", ex);
                }
            }
        }

        private class LeaveDetails
        {
            public int LeaveId { get; set; }
            public int UserId { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string LeaveTypeId { get; set; }
            public string Status { get; set; }
            public string ManagerApprovalStatus { get; set; }
            public string DirectorApprovalStatus { get; set; }
        }

        private class LeaveBalances
        {
            public decimal CasualLeaveBalance { get; set; }
            public decimal EarnedLeaveBalance { get; set; }
        }
    }
}