using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace LeaveManagementPortal
{
    public partial class AdminDashboard : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Check if user is Director or Manager
                string userRole = Session["UserRole"]?.ToString();
                if (userRole != "Director" && userRole != "Manager")
                {
                    Response.Redirect("~/AdminDashboard.aspx");
                }

                LoadLeaveData();
            }
        }

        private void LoadLeaveData()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Query for today's leaves
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        (COALESCE(u.FirstName, '') +
                            CASE WHEN u.MiddleName IS NOT NULL AND u.MiddleName<> '' THEN ' ' + u.MiddleName ELSE '' END +
                            CASE WHEN u.LastName IS NOT NULL AND u.LastName<> '' THEN ' ' + u.LastName ELSE '' END)
                            AS Name,
                        lt.LeaveTypeName
                    FROM LeaveApplications la
                    INNER JOIN Users u ON la.UserID = u.UserID
                    INNER JOIN LeaveTypes lt ON la.LeaveTypeID = lt.LeaveTypeID
                    WHERE @Today BETWEEN la.StartDate AND la.EndDate
                    AND la.Status = 'Approved'
                    ORDER BY Name", conn))
                {
                    cmd.Parameters.AddWithValue("@Today", DateTime.Today);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dtToday = new DataTable();
                        adapter.Fill(dtToday);

                        if (dtToday.Rows.Count > 0)
                        {
                            rptTodayLeaves.DataSource = dtToday;
                            rptTodayLeaves.DataBind();
                        }
                        else
                        {
                            lblNoTodayLeaves.Visible = true;
                        }
                    }
                }

                // Query for tomorrow's leaves
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT
                        (COALESCE(u.FirstName, '') +
                            CASE WHEN u.MiddleName IS NOT NULL AND u.MiddleName<> '' THEN ' ' + u.MiddleName ELSE '' END +
                            CASE WHEN u.LastName IS NOT NULL AND u.LastName<> '' THEN ' ' + u.LastName ELSE '' END)
                            AS Name,
                        lt.LeaveTypeName
                    FROM LeaveApplications la
                    INNER JOIN Users u ON la.UserID = u.UserID
                    INNER JOIN LeaveTypes lt ON la.LeaveTypeID = lt.LeaveTypeID
                    WHERE @Tomorrow BETWEEN la.StartDate AND la.EndDate
                    AND la.Status = 'Approved'
                    ORDER BY Name", conn))
                {
                    cmd.Parameters.AddWithValue("@Tomorrow", DateTime.Today.AddDays(1));

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dtTomorrow = new DataTable();
                        adapter.Fill(dtTomorrow);

                        if (dtTomorrow.Rows.Count > 0)
                        {
                            rptTomorrowLeaves.DataSource = dtTomorrow;
                            rptTomorrowLeaves.DataBind();
                        }
                        else
                        {
                            lblNoTomorrowLeaves.Visible = true;
                        }
                    }
                }
            }
        }
    }
}