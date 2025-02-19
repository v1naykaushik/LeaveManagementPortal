using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LeaveManagementPortal
{
    public partial class ManageEmployees : Page
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
                LoadEmployees();
            }
        }

        private void LoadEmployees()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT UserID, Name, Email, Role, IsActive, EmployeeOfficeID
                    FROM Users 
                    ORDER BY EmployeeOfficeID", conn))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        gvEmployees.DataSource = dt;
                        gvEmployees.DataBind();
                    }
                }
            }
        }

        protected void gvEmployees_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ToggleStatus")
            {
                int userId = Convert.ToInt32(e.CommandArgument);

                // Check if the target user is a Director
                string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT Role, IsActive FROM Users WHERE UserID = @UserID", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        string role = string.Empty;
                        bool isActive = false;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                role = reader["Role"].ToString();
                                isActive = Convert.ToBoolean(reader["IsActive"]);
                            }
                        }

                        if (role == "Director" && isActive)
                        {
                            // Check if this is the only director
                            using (SqlCommand countCmd = new SqlCommand(
                                "SELECT COUNT(*) FROM Users WHERE Role = 'Director'", conn))
                            {
                                int directorCount = (int)countCmd.ExecuteScalar();
                                if (directorCount == 1)
                                {
                                    ScriptManager.RegisterStartupScript(this, GetType(), "Alert",
                                        "alert('Warning: This is the only director in system.');", true);
                                }
                            }

                            // Store the UserID and show password modal
                            hdnSelectedUserId.Value = userId.ToString();
                            pnlPassword.Visible = true;
                            txtDirectorPassword.Focus();
                            ScriptManager.RegisterStartupScript(this, GetType(), "ShowModal",
                                "showPasswordModal();", true);
                        }
                        else if (role == "Director") // For activating a director
                        {
                            // Store the UserID and show password modal
                            hdnSelectedUserId.Value = userId.ToString();
                            pnlPassword.Visible = true;
                            txtDirectorPassword.Focus();
                            ScriptManager.RegisterStartupScript(this, GetType(), "ShowModal",
                                "showPasswordModal();", true);
                        }
                        else
                        {
                            // For non-directors, proceed with status toggle
                            ToggleEmployeeStatus(userId);
                        }
                    }
                }
            }
        }

        protected void btnVerifyPassword_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Password verification clicked");
            System.Diagnostics.Debug.WriteLine($"UserID: {hdnSelectedUserId.Value}");
            System.Diagnostics.Debug.WriteLine($"Password length: {txtDirectorPassword.Text?.Length ?? 0}");

            int userId = Convert.ToInt32(hdnSelectedUserId.Value);
            string enteredPassword = txtDirectorPassword.Text;

            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT COUNT(1) FROM Users WHERE UserID = @UserID AND Password = @Password", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@Password", enteredPassword);

                    int result = (int)cmd.ExecuteScalar();
                    System.Diagnostics.Debug.WriteLine($"Password verification result: {result}");

                    if (result > 0)
                    {
                        // Password verified, proceed with status toggle
                        ToggleEmployeeStatus(userId);

                        // Hide modal and clear password
                        pnlPassword.Visible = false;
                        txtDirectorPassword.Text = "";
                        lblPasswordError.Visible = false;

                        ScriptManager.RegisterStartupScript(this, GetType(), "HideModal",
                            "hidePasswordModal(); showSuccessMessage();", true);
                    }
                    else
                    {
                        // Show error message
                        lblPasswordError.Visible = true;
                        txtDirectorPassword.Text = "";
                        System.Diagnostics.Debug.WriteLine("Password verification failed");
                    }
                }
            }
        }

        protected void btnCloseModal_Click(object sender, EventArgs e)
        {
            pnlPassword.Visible = false;
            txtDirectorPassword.Text = "";
            lblPasswordError.Visible = false;
            ScriptManager.RegisterStartupScript(this, GetType(), "HideModal",
                "hidePasswordModal();", true);
        }

        private void ToggleEmployeeStatus(int userId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE Users 
                    SET IsActive = ~IsActive  -- Toggles between 1 and 0
                    WHERE UserID = @UserID", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine($"Toggle status affected {rowsAffected} rows");
                }
            }

            // Refresh the grid
            LoadEmployees();

            // Show success message
            ScriptManager.RegisterStartupScript(this, GetType(), "UpdateSuccess",
                "showSuccessMessage();", true);
        }
    }
}