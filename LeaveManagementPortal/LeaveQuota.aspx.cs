using DocumentFormat.OpenXml.Math;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static System.Data.Entity.Infrastructure.Design.Executor;

namespace LeaveManagementPortal
{
    public partial class AdminLeaveBalances : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            // Check if user is authorized (Director or Manager)
            string userRole = Session["UserRole"]?.ToString();
            if (userRole != "Director" && userRole != "Manager")
            {
                Response.Redirect("~/Dashboard.aspx");
                return;
            }

            if (!IsPostBack)
            {
                PopulateEmployeeDropdown();
                PopulateLeaveTypeDropdown();
                LoadLeaveBalances();
                //LoadLeaveTypeSummary();
            }
        }

        private void PopulateEmployeeDropdown()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT UserID, Name, EmployeeOfficeID 
                    FROM Users 
                    WHERE IsActive = 1
                    ORDER BY Name", conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string displayText = $"{reader["Name"]} ({reader["EmployeeOfficeID"]})";
                            ListItem item = new ListItem(displayText, reader["UserID"].ToString());
                            ddlEmployee.Items.Add(item);
                        }
                    }
                }
            }
        }

        private void PopulateLeaveTypeDropdown()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT LeaveTypeID, LeaveTypeName 
                    FROM LeaveTypes 
                    ORDER BY LeaveTypeName", conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ListItem item = new ListItem(reader["LeaveTypeName"].ToString(), reader["LeaveTypeID"].ToString());
                            ddlLeaveType.Items.Add(item);
                        }
                    }
                }
            }
        }


        private void LoadLeaveBalances()
        {
            string employeeFilter = string.Empty;

            if (!string.IsNullOrEmpty(ddlEmployee.SelectedValue))
            {
                employeeFilter = $"WHERE u.UserID = {ddlEmployee.SelectedValue}";
            }
            else
            {
                employeeFilter = "WHERE u.IsActive = 1";
            }

            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // First, we need to get the raw data
                using (SqlCommand cmd = new SqlCommand($@"
                    SELECT 
                        u.UserID,
                        u.EmployeeOfficeID,
                        u.Name AS EmployeeName,
                        lt.LeaveTypeName,
                        lb.PresentYearBalance
                    FROM Users u
                    LEFT JOIN LeaveBalances lb ON u.UserID = lb.UserID
                    LEFT JOIN LeaveTypes lt ON lb.LeaveTypeID = lt.LeaveTypeID
                    {employeeFilter}
                    ORDER BY u.EmployeeOfficeID, u.Name", conn))
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable rawData = new DataTable();
                    adapter.Fill(rawData);

                    // Now create a pivot table with employees as rows and leave types as columns
                    DataTable pivotTable = new DataTable();
                    pivotTable.Columns.Add("UserID", typeof(int));
                    pivotTable.Columns.Add("EmployeeOfficeID", typeof(string));
                    pivotTable.Columns.Add("EmployeeName", typeof(string));

                    // Add columns for each leave type
                    pivotTable.Columns.Add("CL", typeof(decimal));
                    pivotTable.Columns.Add("EL", typeof(decimal));
                    pivotTable.Columns.Add("ML", typeof(decimal));
                    pivotTable.Columns.Add("RL", typeof(decimal));

                    // Group the data by employee and pivot
                    var employeeGroups = rawData.AsEnumerable()
                        .GroupBy(row => new {
                            UserID = row.Field<int>("UserID"),
                            EmployeeOfficeID = row.Field<string>("EmployeeOfficeID"),
                            EmployeeName = row.Field<string>("EmployeeName")
                        });

                    foreach (var group in employeeGroups)
                    {
                        DataRow newRow = pivotTable.NewRow();
                        newRow["UserID"] = group.Key.UserID;
                        newRow["EmployeeOfficeID"] = group.Key.EmployeeOfficeID;
                        newRow["EmployeeName"] = group.Key.EmployeeName;

                        // Set default values of 0 for all leave types
                        newRow["CL"] = 0m;
                        newRow["EL"] = 0m;
                        newRow["ML"] = 0m;
                        newRow["RL"] = 0m;

                        // Update values for each leave type that exists for this employee
                        foreach (var row in group)
                        {
                            string leaveType = row.Field<string>("LeaveTypeName");
                            if (!string.IsNullOrEmpty(leaveType) && pivotTable.Columns.Contains(leaveType))
                            {
                                object balanceValue = row["PresentYearBalance"];
                                if (balanceValue != DBNull.Value)
                                {
                                    newRow[leaveType] = Convert.ToDecimal(balanceValue);
                                }
                            }
                        }

                        pivotTable.Rows.Add(newRow);
                    }

                    // Bind the pivot table to the grid
                    gvLeaveBalances.DataSource = pivotTable;
                    gvLeaveBalances.DataBind();
                }
            }
        }

        //private void LoadLeaveTypeSummary()
        //{
        //    string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
        //    using (SqlConnection conn = new SqlConnection(connectionString))
        //    {
        //        using (SqlCommand cmd = new SqlCommand(@"
        //            SELECT 
        //                LeaveTypeID,
        //                LeaveTypeName,
        //                LeaveDescription,
        //                InitialAllocation,
        //                MidYearAllocation,
        //                CASE WHEN LapsesAtYearEnd = 1 THEN 'Yes' ELSE 'No' END AS LapsesAtYearEnd
        //            FROM LeaveTypes
        //            ORDER BY LeaveTypeName", conn))
        //        {
        //            conn.Open();
        //            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
        //            DataTable dt = new DataTable();
        //            adapter.Fill(dt);

        //            gvLeaveTypeSummary.DataSource = dt;
        //            gvLeaveTypeSummary.DataBind();
        //        }
        //    }
        //}

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            LoadLeaveBalances();
        }

        protected void gvLeaveBalances_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvLeaveBalances.PageIndex = e.NewPageIndex;
            LoadLeaveBalances();
        }

        //protected void lnkEdit_Command(object sender, CommandEventArgs e)
        //{
        //    // Extract UserID and LeaveTypeID from the command argument
        //    string[] args = e.CommandArgument.ToString().Split(',');
        //    string userId = args[0];
        //    string leaveTypeId = args[1];

        //    // Store these values for later use when saving changes
        //    hdnUserID.Value = userId;
        //    hdnLeaveTypeID.Value = leaveTypeId;

        //    // Get the current balances to populate the edit fields
        //    string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
        //    using (SqlConnection conn = new SqlConnection(connectionString))
        //    {
        //        using (SqlCommand cmd = new SqlCommand(@"
        //            SELECT PresentYearBalance, NewYearBalance
        //            FROM LeaveBalances
        //            WHERE UserID = @UserID AND LeaveTypeID = @LeaveTypeID", conn))
        //        {
        //            cmd.Parameters.AddWithValue("@UserID", userId);
        //            cmd.Parameters.AddWithValue("@LeaveTypeID", leaveTypeId);

        //            conn.Open();
        //            using (SqlDataReader reader = cmd.ExecuteReader())
        //            {
        //                if (reader.Read())
        //                {
        //                    txtCurrentBalance.Text = reader["PresentYearBalance"].ToString();
        //                    txtNextYearBalance.Text = reader["NewYearBalance"].ToString();

        //                    // Use JavaScript to show the modal
        //                    ScriptManager.RegisterStartupScript(this, GetType(), "showModal", "showEditModal();", true);
        //                }
        //            }
        //        }
        //    }
        //}

        //protected void btnSaveBalance_Click(object sender, EventArgs e)
        //{
        //    if (!Page.IsValid)
        //        return;

        //    string userId = hdnUserID.Value;
        //    string leaveTypeId = hdnLeaveTypeID.Value;
        //    decimal currentBalance = decimal.Parse(txtCurrentBalance.Text);
        //    decimal nextYearBalance = decimal.Parse(txtNextYearBalance.Text);

        //    string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
        //    using (SqlConnection conn = new SqlConnection(connectionString))
        //    {
        //        using (SqlCommand cmd = new SqlCommand(@"
        //            UPDATE LeaveBalances
        //            SET PresentYearBalance = @CurrentBalance,
        //                NewYearBalance = @NextYearBalance,
        //                LastUpdatedBy = @UpdatedBy,
        //                LastUpdatedDate = GETDATE()
        //            WHERE UserID = @UserID AND LeaveTypeID = @LeaveTypeID", conn))
        //        {
        //            cmd.Parameters.AddWithValue("@CurrentBalance", currentBalance);
        //            cmd.Parameters.AddWithValue("@NextYearBalance", nextYearBalance);
        //            cmd.Parameters.AddWithValue("@UpdatedBy", Session["UserID"]);
        //            cmd.Parameters.AddWithValue("@UserID", userId);
        //            cmd.Parameters.AddWithValue("@LeaveTypeID", leaveTypeId);

        //            conn.Open();
        //            int result = cmd.ExecuteNonQuery();

        //            if (result > 0)
        //            {
        //                // Log the update in an audit table if needed
        //                LogLeaveBalanceUpdate(conn, userId, leaveTypeId, currentBalance, nextYearBalance);

        //                // Show success message and reload the grid
        //                ScriptManager.RegisterStartupScript(this, GetType(), "closeModal", "$('#editModal').modal('hide');", true);
        //                LoadLeaveBalances();
        //            }
        //        }
        //    }
        //}

        //private void LogLeaveBalanceUpdate(SqlConnection conn, string userId, string leaveTypeId, decimal currentBalance, decimal nextYearBalance)
        //{
        //    // Optional: Add code to log the update to an audit table
        //    // This is good practice for administrative changes
        //    using (SqlCommand cmd = new SqlCommand(@"
        //        INSERT INTO LeaveBalanceAuditLog (
        //            UserID, LeaveTypeID, OldPresentYearBalance, NewPresentYearBalance, 
        //            OldNewYearBalance, NewNewYearBalance, UpdatedBy, UpdatedDate
        //        ) VALUES (
        //            @UserID, @LeaveTypeID, 
        //            (SELECT PresentYearBalance FROM LeaveBalances WHERE UserID = @UserID AND LeaveTypeID = @LeaveTypeID),
        //            @CurrentBalance,
        //            (SELECT NewYearBalance FROM LeaveBalances WHERE UserID = @UserID AND LeaveTypeID = @LeaveTypeID),
        //            @NextYearBalance,
        //            @UpdatedBy, GETDATE()
        //        )", conn))
        //    {
        //        cmd.Parameters.AddWithValue("@UserID", userId);
        //        cmd.Parameters.AddWithValue("@LeaveTypeID", leaveTypeId);
        //        cmd.Parameters.AddWithValue("@CurrentBalance", currentBalance);
        //        cmd.Parameters.AddWithValue("@NextYearBalance", nextYearBalance);
        //        cmd.Parameters.AddWithValue("@UpdatedBy", Session["UserID"]);

        //        // Note: This might throw an exception if the audit table doesn't exist
        //        // You would need to handle this appropriately or create the table first
        //        try
        //        {
        //            cmd.ExecuteNonQuery();
        //        }
        //        catch (Exception ex)
        //        {
        //            // Log or handle the exception
        //            // For example, create the audit table if it doesn't exist
        //        }
        //    }
        //}

        

        protected void btnExport_Click(object sender, EventArgs e)
        {
            string employeeFilter = string.Empty;
            if (!string.IsNullOrEmpty(ddlEmployee.SelectedValue))
            {
                employeeFilter = $"WHERE u.UserID = {ddlEmployee.SelectedValue}";
            }
            else
            {
                employeeFilter = "WHERE u.IsActive = 1";
            }

            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand($@"
            SELECT 
                u.UserID,
                u.EmployeeOfficeID,
                u.Name AS EmployeeName,
                lt.LeaveTypeName,
                lb.PresentYearBalance
            FROM Users u
            LEFT JOIN LeaveBalances lb ON u.UserID = lb.UserID
            LEFT JOIN LeaveTypes lt ON lb.LeaveTypeID = lt.LeaveTypeID
            {employeeFilter}
            ORDER BY u.EmployeeOfficeID, u.Name", conn))
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable rawData = new DataTable();
                    adapter.Fill(rawData);

                    // Pivot the data using C#
                    DataTable pivotTable = new DataTable();
                    pivotTable.Columns.Add("UserID", typeof(int));
                    pivotTable.Columns.Add("EmployeeOfficeID", typeof(string));
                    pivotTable.Columns.Add("EmployeeName", typeof(string));
                    pivotTable.Columns.Add("CL", typeof(decimal));
                    pivotTable.Columns.Add("EL", typeof(decimal));
                    pivotTable.Columns.Add("ML", typeof(decimal));
                    pivotTable.Columns.Add("RL", typeof(decimal));

                    var employeeGroups = rawData.AsEnumerable()
                        .GroupBy(row => new {
                            UserID = row.Field<int>("UserID"),
                            EmployeeOfficeID = row.Field<string>("EmployeeOfficeID"),
                            EmployeeName = row.Field<string>("EmployeeName")
                        });

                    foreach (var group in employeeGroups)
                    {
                        DataRow newRow = pivotTable.NewRow();
                        newRow["UserID"] = group.Key.UserID;
                        newRow["EmployeeOfficeID"] = group.Key.EmployeeOfficeID;
                        newRow["EmployeeName"] = group.Key.EmployeeName;

                        // Initialize leave balances to 0
                        newRow["CL"] = 0m;
                        newRow["EL"] = 0m;
                        newRow["ML"] = 0m;
                        newRow["RL"] = 0m;

                        foreach (var row in group)
                        {
                            string leaveType = row.Field<string>("LeaveTypeName");
                            if (!string.IsNullOrEmpty(leaveType) && pivotTable.Columns.Contains(leaveType))
                            {
                                object balanceValue = row["PresentYearBalance"];
                                if (balanceValue != DBNull.Value)
                                {
                                    newRow[leaveType] = Convert.ToDecimal(balanceValue);
                                }
                            }
                        }

                        pivotTable.Rows.Add(newRow);
                    }

                    // Export to CSV
                    Response.Clear();
                    Response.Buffer = true;
                    Response.AddHeader("content-disposition", "attachment;filename=LeaveBalances_" + DateTime.Now.ToString("yyyyMMdd") + ".csv");
                    Response.Charset = "";
                    Response.ContentType = "application/text";

                    StringBuilder sb = new StringBuilder();
                    sb.Append("Emp ID,Employee Name,CL,EL,ML,RL");
                    sb.Append(Environment.NewLine);

                    foreach (DataRow row in pivotTable.Rows)
                    {
                        sb.Append(EscapeCsvValue(row["EmployeeOfficeID"].ToString()) + ",");
                        sb.Append(EscapeCsvValue(row["EmployeeName"].ToString()) + ",");
                        sb.Append(EscapeCsvValue(row["CL"].ToString()) + ",");
                        sb.Append(EscapeCsvValue(row["EL"].ToString()) + ",");
                        sb.Append(EscapeCsvValue(row["ML"].ToString()) + ",");
                        sb.Append(EscapeCsvValue(row["RL"].ToString()));
                        sb.Append(Environment.NewLine);
                    }

                    Response.Output.Write(sb.ToString());
                    Response.Flush();
                    Response.End();
                }
            }
        }


        private string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            bool containsComma = value.Contains(",");
            bool containsQuote = value.Contains("\"");
            bool containsNewline = value.Contains("\n") || value.Contains("\r");

            if (containsComma || containsQuote || containsNewline)
            {
                // Escape quotes by doubling them
                if (containsQuote)
                    value = value.Replace("\"", "\"\"");

                // Wrap in quotes
                return "\"" + value + "\"";
            }

            return value;
        }
    }
}
