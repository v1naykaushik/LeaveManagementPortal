using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace LeaveManagementPortal
{
    public partial class Dashboard : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadRecentLeaves();
            }
        }

        private void LoadRecentLeaves()
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

                // Query for recent leaves
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT TOP 5 
                        la.StartDate, 
                        la.EndDate, 
                        la.Status, 
                        la.Duration,
                        lt.LeaveTypeName
                    FROM LeaveApplications la
                    INNER JOIN LeaveTypes lt ON la.LeaveTypeID = lt.LeaveTypeID
                    WHERE la.UserID = @UserID
                    ORDER BY la.CreatedDate DESC", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dtLeaves = new DataTable();
                        adapter.Fill(dtLeaves);

                        if (dtLeaves.Rows.Count > 0)
                        {
                            rptRecentLeaves.DataSource = dtLeaves;
                            rptRecentLeaves.DataBind();
                        }
                        else
                        {
                            lblNoLeaves.Visible = true;
                        }
                    }
                }
            }
        }

        protected string FormatDuration(object durationObj)
        {
            if (durationObj != null)
            {
                decimal duration = Convert.ToDecimal(durationObj);

                if (duration == 0.5m)
                {
                    return "half day";
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