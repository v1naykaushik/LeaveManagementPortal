using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;  // Keep this for ASP.NET controls
using iText.Kernel.Pdf;
using iText.Kernel.Colors;
using iText.Layout.Borders;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Layout.Properties;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;

namespace LeaveManagementPortal
{
    public partial class StaffLeaveHistory : Page
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
                        ddlEmployee.Items.Add(new ListItem("All Employees", ""));
                        while (reader.Read())
                        {
                            ddlEmployee.Items.Add(new ListItem(
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
                        ddlLeaveType.Items.Add(new ListItem("All Types", ""));
                        while (reader.Read())
                        {
                            ddlLeaveType.Items.Add(new ListItem(
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

        protected void btnExportPDF_Click(object sender, EventArgs e)
        {
            DataTable dt = GetLeaveHistoryData();
            using (MemoryStream ms = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdf = new PdfDocument(writer);
                iText.Layout.Document document = new iText.Layout.Document(pdf, iText.Kernel.Geom.PageSize.A4.Rotate());

                // Add title
                document.Add(new iText.Layout.Element.Paragraph("Staff Leave History Report")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFontSize(16)
                    .SetBold()
                    .SetMarginBottom(20));

                // Add filters info
                StringBuilder filters = new StringBuilder("Filters: ");
                if (!string.IsNullOrEmpty(ddlEmployee.SelectedValue))
                    filters.Append($"Employee: {ddlEmployee.SelectedItem.Text}, ");
                if (!string.IsNullOrEmpty(ddlLeaveType.SelectedValue))
                    filters.Append($"Leave Type: {ddlLeaveType.SelectedItem.Text}, ");
                if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
                    filters.Append($"Status: {ddlStatus.SelectedValue}, ");
                if (!string.IsNullOrEmpty(txtDateRange.Text))
                    filters.Append($"Date: {DateTime.ParseExact(txtDateRange.Text, "yyyy-MM", null).ToString("MMMM yyyy")}");

                document.Add(new iText.Layout.Element.Paragraph(filters.ToString().TrimEnd(',', ' '))
                    .SetFontSize(10)
                    .SetMarginBottom(20));

                // Create table
                iText.Layout.Element.Table table = new iText.Layout.Element.Table(dt.Columns.Count)
                    .UseAllAvailableWidth()
                    .SetFixedLayout();

                // Add headers
                foreach (DataColumn column in dt.Columns)
                {
                    iText.Layout.Element.Cell cell = new iText.Layout.Element.Cell()
                        .Add(new iText.Layout.Element.Paragraph(column.ColumnName))
                        .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetPadding(5)
                        .SetBold();
                    table.AddCell(cell);
                }

                // Add data rows
                foreach (DataRow row in dt.Rows)
                {
                    foreach (object item in row.ItemArray)
                    {
                        iText.Layout.Element.Cell cell = new iText.Layout.Element.Cell()
                            .Add(new iText.Layout.Element.Paragraph(item.ToString()))
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetPadding(5);
                        table.AddCell(cell);
                    }
                }

                document.Add(table);
                document.Close();

                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment;filename=LeaveHistory.pdf");
                Response.Buffer = true;
                Response.BinaryWrite(ms.ToArray());
                Response.End();
            }
        }

        protected void btnExportWord_Click(object sender, EventArgs e)
        {
            DataTable dt = GetLeaveHistoryData();
            using (MemoryStream ms = new MemoryStream())
            {
                using (WordprocessingDocument doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document))
                {
                    MainDocumentPart mainPart = doc.AddMainDocumentPart();
                    mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
                    Body body = mainPart.Document.AppendChild(new Body());

                    // Add title
                    DocumentFormat.OpenXml.Wordprocessing.Paragraph titlePara =
                        body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                            new Run(new DocumentFormat.OpenXml.Wordprocessing.Text("Staff Leave History Report"))));
                    titlePara.ParagraphProperties = new ParagraphProperties(
                        new Justification() { Val = JustificationValues.Center });

                    // Add filters info
                    StringBuilder filters = new StringBuilder("Filters: ");
                    if (!string.IsNullOrEmpty(ddlEmployee.SelectedValue))
                        filters.Append($"Employee: {ddlEmployee.SelectedItem.Text}, ");
                    if (!string.IsNullOrEmpty(ddlLeaveType.SelectedValue))
                        filters.Append($"Leave Type: {ddlLeaveType.SelectedItem.Text}, ");
                    if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
                        filters.Append($"Status: {ddlStatus.SelectedValue}, ");
                    if (!string.IsNullOrEmpty(txtDateRange.Text))
                        filters.Append($"Date: {DateTime.ParseExact(txtDateRange.Text, "yyyy-MM", null).ToString("MMMM yyyy")}");

                    body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                        new Run(new DocumentFormat.OpenXml.Wordprocessing.Text(filters.ToString().TrimEnd(',', ' ')))));

                    // Create table
                    DocumentFormat.OpenXml.Wordprocessing.Table table =
                        new DocumentFormat.OpenXml.Wordprocessing.Table();

                    // Add header row
                    DocumentFormat.OpenXml.Wordprocessing.TableRow headerRow =
                        new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                    foreach (DataColumn column in dt.Columns)
                    {
                        DocumentFormat.OpenXml.Wordprocessing.TableCell cell =
                            new DocumentFormat.OpenXml.Wordprocessing.TableCell(
                                new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                                    new Run(new DocumentFormat.OpenXml.Wordprocessing.Text(column.ColumnName))));
                        headerRow.Append(cell);
                    }
                    table.Append(headerRow);

                    // Add data rows
                    foreach (DataRow row in dt.Rows)
                    {
                        DocumentFormat.OpenXml.Wordprocessing.TableRow dataRow =
                            new DocumentFormat.OpenXml.Wordprocessing.TableRow();
                        foreach (object item in row.ItemArray)
                        {
                            DocumentFormat.OpenXml.Wordprocessing.TableCell cell =
                                new DocumentFormat.OpenXml.Wordprocessing.TableCell(
                                    new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                                        new Run(new DocumentFormat.OpenXml.Wordprocessing.Text(item.ToString()))));
                            dataRow.Append(cell);
                        }
                        table.Append(dataRow);
                    }

                    body.Append(table);
                }

                Response.Clear();
                Response.ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                Response.AddHeader("content-disposition", "attachment;filename=LeaveHistory.docx");
                Response.BinaryWrite(ms.ToArray());
                Response.End();
            }
        }
    }
}using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;  // Keep this for ASP.NET controls
using iText.Kernel.Pdf;
using iText.Kernel.Colors;
using iText.Layout.Borders;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Layout.Properties;

namespace LeaveManagementPortal
{
    public partial class StaffLeaveHistory : Page
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
                        ddlEmployee.Items.Add(new ListItem("All Employees", ""));
                        while (reader.Read())
                        {
                            ddlEmployee.Items.Add(new ListItem(
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
                        ddlLeaveType.Items.Add(new ListItem("All Types", ""));
                        while (reader.Read())
                        {
                            ddlLeaveType.Items.Add(new ListItem(
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

        protected void btnExportPDF_Click(object sender, EventArgs e)
        {
            DataTable dt = GetLeaveHistoryData();
            using (MemoryStream ms = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdf = new PdfDocument(writer);
                iText.Layout.Document document = new iText.Layout.Document(pdf, iText.Kernel.Geom.PageSize.A4.Rotate());

                // Add title
                document.Add(new iText.Layout.Element.Paragraph("Staff Leave History Report")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFontSize(16)
                    .SetBold()
                    .SetMarginBottom(20));

                // Add filters info
                StringBuilder filters = new StringBuilder("Filters: ");
                if (!string.IsNullOrEmpty(ddlEmployee.SelectedValue))
                    filters.Append($"Employee: {ddlEmployee.SelectedItem.Text}, ");
                if (!string.IsNullOrEmpty(ddlLeaveType.SelectedValue))
                    filters.Append($"Leave Type: {ddlLeaveType.SelectedItem.Text}, ");
                if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
                    filters.Append($"Status: {ddlStatus.SelectedValue}, ");
                if (!string.IsNullOrEmpty(txtDateRange.Text))
                    filters.Append($"Date: {DateTime.ParseExact(txtDateRange.Text, "yyyy-MM", null).ToString("MMMM yyyy")}");

                document.Add(new iText.Layout.Element.Paragraph(filters.ToString().TrimEnd(',', ' '))
                    .SetFontSize(10)
                    .SetMarginBottom(20));

                // Create table
                iText.Layout.Element.Table table = new iText.Layout.Element.Table(dt.Columns.Count)
                    .UseAllAvailableWidth()
                    .SetFixedLayout();

                // Add headers
                foreach (DataColumn column in dt.Columns)
                {
                    iText.Layout.Element.Cell cell = new iText.Layout.Element.Cell()
                        .Add(new iText.Layout.Element.Paragraph(column.ColumnName))
                        .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetPadding(5)
                        .SetBold();
                    table.AddCell(cell);
                }

                // Add data rows
                foreach (DataRow row in dt.Rows)
                {
                    foreach (object item in row.ItemArray)
                    {
                        iText.Layout.Element.Cell cell = new iText.Layout.Element.Cell()
                            .Add(new iText.Layout.Element.Paragraph(item.ToString()))
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                            .SetPadding(5);
                        table.AddCell(cell);
                    }
                }

                document.Add(table);
                document.Close();

                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment;filename=LeaveHistory.pdf");
                Response.Buffer = true;
                Response.BinaryWrite(ms.ToArray());
                Response.End();
            }
        }

        protected void btnExportWord_Click(object sender, EventArgs e)
        {
            DataTable dt = GetLeaveHistoryData();
            using (MemoryStream ms = new MemoryStream())
            {
                using (WordprocessingDocument doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document))
                {
                    MainDocumentPart mainPart = doc.AddMainDocumentPart();
                    mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
                    Body body = mainPart.Document.AppendChild(new Body());

                    // Add title
                    DocumentFormat.OpenXml.Wordprocessing.Paragraph titlePara =
                        body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                            new Run(new DocumentFormat.OpenXml.Wordprocessing.Text("Staff Leave History Report"))));
                    titlePara.ParagraphProperties = new ParagraphProperties(
                        new Justification() { Val = JustificationValues.Center });

                    // Add filters info
                    StringBuilder filters = new StringBuilder("Filters: ");
                    if (!string.IsNullOrEmpty(ddlEmployee.SelectedValue))
                        filters.Append($"Employee: {ddlEmployee.SelectedItem.Text}, ");
                    if (!string.IsNullOrEmpty(ddlLeaveType.SelectedValue))
                        filters.Append($"Leave Type: {ddlLeaveType.SelectedItem.Text}, ");
                    if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
                        filters.Append($"Status: {ddlStatus.SelectedValue}, ");
                    if (!string.IsNullOrEmpty(txtDateRange.Text))
                        filters.Append($"Date: {DateTime.ParseExact(txtDateRange.Text, "yyyy-MM", null).ToString("MMMM yyyy")}");

                    body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                        new Run(new DocumentFormat.OpenXml.Wordprocessing.Text(filters.ToString().TrimEnd(',', ' ')))));

                    // Create table
                    DocumentFormat.OpenXml.Wordprocessing.Table table =
                        new DocumentFormat.OpenXml.Wordprocessing.Table();

                    // Add header row
                    DocumentFormat.OpenXml.Wordprocessing.TableRow headerRow =
                        new DocumentFormat.OpenXml.Wordprocessing.TableRow();

                    foreach (DataColumn column in dt.Columns)
                    {
                        DocumentFormat.OpenXml.Wordprocessing.TableCell cell =
                            new DocumentFormat.OpenXml.Wordprocessing.TableCell(
                                new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                                    new Run(new DocumentFormat.OpenXml.Wordprocessing.Text(column.ColumnName))));
                        headerRow.Append(cell);
                    }
                    table.Append(headerRow);

                    // Add data rows
                    foreach (DataRow row in dt.Rows)
                    {
                        DocumentFormat.OpenXml.Wordprocessing.TableRow dataRow =
                            new DocumentFormat.OpenXml.Wordprocessing.TableRow();
                        foreach (object item in row.ItemArray)
                        {
                            DocumentFormat.OpenXml.Wordprocessing.TableCell cell =
                                new DocumentFormat.OpenXml.Wordprocessing.TableCell(
                                    new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                                        new Run(new DocumentFormat.OpenXml.Wordprocessing.Text(item.ToString()))));
                            dataRow.Append(cell);
                        }
                        table.Append(dataRow);
                    }

                    body.Append(table);
                }

                Response.Clear();
                Response.ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                Response.AddHeader("content-disposition", "attachment;filename=LeaveHistory.docx");
                Response.BinaryWrite(ms.ToArray());
                Response.End();
            }
        }
    }
}