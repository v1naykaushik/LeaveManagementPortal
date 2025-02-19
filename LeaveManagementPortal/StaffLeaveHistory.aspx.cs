using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace LeaveManagementPortal
{
    public partial class StaffLeaveHistory : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            if (!IsPostBack)
            {
                // Verify user role
                string userRole = Session["UserRole"]?.ToString();
                if (userRole != "Director" && userRole != "Manager")
                {
                    Response.Redirect("~/Dashboard.aspx");
                    return;
                }

                LoadEmployees();
                LoadLeaveTypes();
                LoadLeaveHistory();
            }
        }

        private void LoadEmployees()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT UserID, Name 
                    FROM Users 
                    WHERE Role = 'Employee' AND IsActive = 1 
                    ORDER BY Name", conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        ddlEmployee.Items.Clear();
                        ddlEmployee.Items.Add(new System.Web.UI.WebControls.ListItem("All Employees", ""));
                        while (reader.Read())
                        {
                            ddlEmployee.Items.Add(new System.Web.UI.WebControls.ListItem(
                                reader["Name"].ToString(),
                                reader["UserID"].ToString()
                            ));
                        }
                    }
                }
            }
        }

        private void LoadLeaveTypes()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT LeaveTypeID, LeaveTypeName 
                    FROM LeaveTypes 
                    ORDER BY LeaveTypeName", conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        ddlLeaveType.Items.Clear();
                        ddlLeaveType.Items.Add(new System.Web.UI.WebControls.ListItem("All Types", ""));
                        while (reader.Read())
                        {
                            ddlLeaveType.Items.Add(new System.Web.UI.WebControls.ListItem(
                                reader["LeaveTypeName"].ToString(),
                                reader["LeaveTypeID"].ToString()
                            ));
                        }
                    }
                }
            }
        }

        private DataTable GetLeaveHistoryData()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT 
                        u.Name as EmployeeName,
                        lt.LeaveTypeName,
                        la.StartDate,
                        la.EndDate,
                        CASE 
                            WHEN la.IsHalfDay = 1 THEN 'Half Day'
                            ELSE CAST(la.Duration as varchar) + ' day(s)'
                        END as Duration,
                        la.Status,
                        la.ManagerApprovalStatus,
                        la.DirectorApprovalStatus,
                        ISNULL(la.Reason, '') as Reason
                    FROM LeaveApplications la
                    INNER JOIN Users u ON la.UserID = u.UserID
                    INNER JOIN LeaveTypes lt ON la.LeaveTypeID = lt.LeaveTypeID
                    WHERE 1=1";

                if (!string.IsNullOrEmpty(ddlEmployee.SelectedValue))
                    query += " AND la.UserID = @UserID";
                if (!string.IsNullOrEmpty(ddlLeaveType.SelectedValue))
                    query += " AND la.LeaveTypeID = @LeaveTypeID";
                if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
                    query += " AND la.Status = @Status";
                if (!string.IsNullOrEmpty(txtDateRange.Text))
                    query += " AND YEAR(la.StartDate) = @Year AND MONTH(la.StartDate) = @Month";

                query += " ORDER BY la.StartDate DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(ddlEmployee.SelectedValue))
                        cmd.Parameters.AddWithValue("@UserID", ddlEmployee.SelectedValue);
                    if (!string.IsNullOrEmpty(ddlLeaveType.SelectedValue))
                        cmd.Parameters.AddWithValue("@LeaveTypeID", ddlLeaveType.SelectedValue);
                    if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
                        cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue);
                    if (!string.IsNullOrEmpty(txtDateRange.Text))
                    {
                        DateTime selectedDate = DateTime.ParseExact(txtDateRange.Text, "yyyy-MM", null);
                        cmd.Parameters.AddWithValue("@Year", selectedDate.Year);
                        cmd.Parameters.AddWithValue("@Month", selectedDate.Month);
                    }

                    DataTable dt = new DataTable();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        private void LoadLeaveHistory()
        {
            gvLeaveHistory.DataSource = GetLeaveHistoryData();
            gvLeaveHistory.DataBind();
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            LoadLeaveHistory();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ddlEmployee.SelectedIndex = 0;
            ddlLeaveType.SelectedIndex = 0;
            ddlStatus.SelectedIndex = 0;
            txtDateRange.Text = "";
            LoadLeaveHistory();
        }

        protected void gvLeaveHistory_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvLeaveHistory.PageIndex = e.NewPageIndex;
            LoadLeaveHistory();
        }

        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            // Get the data
            DataTable dt = GetLeaveHistoryData();

            // Create new Excel package
            using (ExcelPackage excel = new ExcelPackage())
            {
                // Add a new worksheet to the empty workbook
                ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add("Leave History");

                // Add the title (merged cells)
                worksheet.Cells[1, 1].Value = "Staff Leave History Report";
                worksheet.Cells[1, 1, 1, dt.Columns.Count].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Size = 16;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                // Add filters info in row 2
                StringBuilder filters = new StringBuilder("Filters: ");
                if (!string.IsNullOrEmpty(ddlEmployee.SelectedValue))
                    filters.Append($"Employee: {ddlEmployee.SelectedItem.Text}, ");
                if (!string.IsNullOrEmpty(ddlLeaveType.SelectedValue))
                    filters.Append($"Leave Type: {ddlLeaveType.SelectedItem.Text}, ");
                if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
                    filters.Append($"Status: {ddlStatus.SelectedValue}, ");
                if (!string.IsNullOrEmpty(txtDateRange.Text))
                    filters.Append($"Date: {DateTime.ParseExact(txtDateRange.Text, "yyyy-MM", null).ToString("MMMM yyyy")}");

                worksheet.Cells[2, 1].Value = filters.ToString().TrimEnd(',', ' ');
                worksheet.Cells[2, 1, 2, dt.Columns.Count].Merge = true;
                worksheet.Cells[2, 1].Style.Font.Italic = true;

                // Add headers in row 4
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    worksheet.Cells[4, i + 1].Value = dt.Columns[i].ColumnName;
                    worksheet.Cells[4, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[4, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[4, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Add data starting from row 5
                for (int row = 0; row < dt.Rows.Count; row++)
                {
                    for (int col = 0; col < dt.Columns.Count; col++)
                    {
                        worksheet.Cells[row + 5, col + 1].Value = dt.Rows[row][col].ToString();
                    }
                }

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Add borders to all cells
                var dataRange = worksheet.Cells[4, 1, dt.Rows.Count + 4, dt.Columns.Count];
                dataRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                dataRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                dataRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                dataRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                // Set content alignment
                dataRange.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                dataRange.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                // Convert to byte array and send to browser
                byte[] excelData = excel.GetAsByteArray();

                Response.Clear();
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename=LeaveHistory.xlsx");
                Response.BinaryWrite(excelData);
                Response.End();
            }
        }
    }
}