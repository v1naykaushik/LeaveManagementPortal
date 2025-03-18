using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LeaveManagementPortal
{
    public partial class Settings : System.Web.UI.Page
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
                LoadHolidays();
            }
        }

        private void LoadHolidays()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Get all holidays grouped by year
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT YEAR(HolidayDate) as Year, 
                           HolidayID, HolidayDate, HolidayName, isRestricted 
                    FROM RestrictedHolidays
                    WHERE YEAR(HolidayDate) >= YEAR(GETDATE())
                          AND YEAR(HolidayDate) <= YEAR(GETDATE()) + 1
                    ORDER BY HolidayDate", conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        var holidays = new Dictionary<int, List<dynamic>>();

                        while (reader.Read())
                        {
                            int year = Convert.ToInt32(reader["Year"]);
                            if (!holidays.ContainsKey(year))
                            {
                                holidays[year] = new List<dynamic>();
                            }

                            holidays[year].Add(new
                            {
                                HolidayID = reader["HolidayID"],
                                HolidayDate = reader["HolidayDate"],
                                HolidayName = reader["HolidayName"],
                                isRestricted = reader["isRestricted"]
                            });
                        }

                        // Convert to format suitable for repeater
                        var yearData = holidays.Select(kv => new
                        {
                            Year = kv.Key,
                            Holidays = kv.Value
                        }).OrderBy(y => y.Year).ToList();

                        // Ensure current and next year are always present
                        int currentYear = DateTime.Now.Year;
                        if (!yearData.Any(y => y.Year == currentYear))
                        {
                            yearData.Add(new { Year = currentYear, Holidays = new List<dynamic>() });
                        }
                        if (!yearData.Any(y => y.Year == currentYear + 1))
                        {
                            yearData.Add(new { Year = currentYear + 1, Holidays = new List<dynamic>() });
                        }

                        yearData = yearData.OrderBy(y => y.Year).ToList();
                        rptYears.DataSource = yearData;
                        rptYears.DataBind();
                    }
                }
            }
        }

        protected void rptYears_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var yearData = (dynamic)e.Item.DataItem;
                var rptHolidays = (Repeater)e.Item.FindControl("rptHolidays");
                rptHolidays.DataSource = yearData.Holidays;
                rptHolidays.DataBind();

                // Handle empty state visibility in the repeater's OnItemDataBound event
                if (rptHolidays.Items.Count == 0)
                {
                    Panel pnlNoHolidays = (Panel)rptHolidays.Controls[rptHolidays.Controls.Count - 1].FindControl("pnlNoHolidays");
                    if (pnlNoHolidays != null)
                    {
                        pnlNoHolidays.Visible = true;
                    }
                }
            }
        }

        protected void btnAddHoliday_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                DateTime holidayDate = DateTime.Parse(txtHolidayDate.Text);
                string holidayName = txtHolidayName.Text.Trim();

                string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Check if date already exists
                    using (SqlCommand checkCmd = new SqlCommand(
                        "SELECT COUNT(*) FROM RestrictedHolidays WHERE HolidayDate = @Date", conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Date", holidayDate);
                        int existingCount = (int)checkCmd.ExecuteScalar();

                        if (existingCount > 0)
                        {
                            ScriptManager.RegisterStartupScript(this, GetType(), "Error",
                                "alert('A holiday already exists for this date.');", true);
                            return;
                        }
                    }

                    // Insert new holiday
                    using (SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO RestrictedHolidays (HolidayDate, HolidayName, isRestricted)
                        VALUES (@HolidayDate, @HolidayName, @IsRestricted)", conn))
                    {
                        cmd.Parameters.AddWithValue("@HolidayDate", holidayDate);
                        cmd.Parameters.AddWithValue("@HolidayName", holidayName);
                        cmd.Parameters.AddWithValue("@IsRestricted", Convert.ToBoolean(rblHolidayType.SelectedValue));

                        try
                        {
                            cmd.ExecuteNonQuery();

                            // Clear input fields
                            txtHolidayDate.Text = "";
                            txtHolidayName.Text = "";

                            // Refresh holiday list
                            LoadHolidays();

                            // Show success message
                            ScriptManager.RegisterStartupScript(this, GetType(), "Success",
                                "showSuccessMessage();", true);
                        }
                        catch (Exception ex)
                        {
                            ScriptManager.RegisterStartupScript(this, GetType(), "Error",
                                $"alert('Error adding holiday: {ex.Message}');", true);
                        }
                    }
                }
            }
        }

        protected void rptHolidays_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Delete")
            {
                int holidayId = Convert.ToInt32(e.CommandArgument);
                DeleteHoliday(holidayId);
            }
        }

        private void DeleteHoliday(int holidayId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // First check if any leaves are approved for this holiday
                        using (SqlCommand checkCmd = new SqlCommand(@"
                            SELECT COUNT(*)
                            FROM LeaveApplications la
                            INNER JOIN RestrictedHolidays rh ON la.StartDate = rh.HolidayDate
                            WHERE rh.HolidayID = @HolidayID
                            AND la.LeaveTypeID = '4'  -- RL
                            AND la.Status = 'Approved'", conn, transaction))
                        {
                            checkCmd.Parameters.AddWithValue("@HolidayID", holidayId);
                            int approvedLeaves = (int)checkCmd.ExecuteScalar();

                            if (approvedLeaves > 0)
                            {
                                ScriptManager.RegisterStartupScript(this, GetType(), "Error",
                                    "alert('Cannot delete this holiday as there are approved leaves for this date.');", true);
                                return;
                            }
                        }

                        // If no approved leaves, first cancel any pending leaves
                        using (SqlCommand updateCmd = new SqlCommand(@"
                            UPDATE LeaveApplications
                            SET Status = 'Cancelled',
                                LastModifiedDate = GETDATE()
                            FROM LeaveApplications la
                            INNER JOIN RestrictedHolidays rh ON la.StartDate = rh.HolidayDate
                            WHERE rh.HolidayID = @HolidayID
                            AND la.LeaveTypeID = '4'  -- RL
                            AND la.Status = 'Pending'", conn, transaction))
                        {
                            updateCmd.Parameters.AddWithValue("@HolidayID", holidayId);
                            updateCmd.ExecuteNonQuery();
                        }

                        // Then delete the holiday
                        using (SqlCommand deleteCmd = new SqlCommand(
                            "DELETE FROM RestrictedHolidays WHERE HolidayID = @HolidayID", conn, transaction))
                        {
                            deleteCmd.Parameters.AddWithValue("@HolidayID", holidayId);
                            deleteCmd.ExecuteNonQuery();
                        }

                        transaction.Commit();

                        // Refresh the holiday list
                        LoadHolidays();

                        // Show success message
                        ScriptManager.RegisterStartupScript(this, GetType(), "Success",
                            "showSuccessMessage();", true);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ScriptManager.RegisterStartupScript(this, GetType(), "Error",
                            $"alert('Error deleting holiday: {ex.Message}');", true);
                    }
                }
            }
        }
    }
}