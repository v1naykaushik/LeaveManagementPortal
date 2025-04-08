using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

namespace LeaveManagementPortal
{
    public partial class Profile : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadUserProfile();
            }
        }

        private void LoadUserProfile()
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
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        u.Email,
                        u.EmployeeOfficeID,
                        m.Name as ManagerName
                    FROM Users u
                    LEFT JOIN Users m ON u.ManagerID = m.UserID
                    WHERE u.UserID = @UserID", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);

                    /////////
                    //@"
                    //SELECT 
                    //    u.UserID, 
                    //    u.FirstName, 
                    //    u.MiddleName, 
                    //    u.LastName, 
                    //    u.Email, 
                    //    u.Role, 
                    //    u.ManagerID, 
                    //    d.Name AS DesignationName, 
                    //    t.Name AS TitleName 
                    //FROM Users u
                    //LEFT JOIN DesignationMaster d ON u.Designation = d.id
                    //LEFT JOIN TitlesMaster t ON u.Title = t.id
                    //WHERE u.Email = @Email AND u.Password = @Password AND u.IsActive = 1"
                    ///

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            //lblName.Text = reader["Name"].ToString();
                            lblName.Text = Session["UserTitle"]?.ToString() +" "+ Session["UserName"]?.ToString();
                            lblDesignation.Text = Session["UserDesignation"]?.ToString();
                            lblEmail.Text = reader["Email"].ToString();
                            //lblRole.Text = reader["Role"].ToString();
                            lblRole.Text = Session["UserRole"]?.ToString();
                            lblEmployeeId.Text = reader["EmployeeOfficeID"].ToString();
                            lblManager.Text = reader["ManagerName"] != DBNull.Value
                                ? reader["ManagerName"].ToString()
                                : "Not Assigned";
                        }
                    }
                }
            }
        }

        protected void btnChangePassword_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;

            string userId = Session["UserID"]?.ToString();
            if (string.IsNullOrEmpty(userId))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string currentPassword = txtCurrentPassword.Text;
            string newPassword = txtNewPassword.Text;

            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // First verify current password
                using (SqlCommand verifyCmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Users WHERE UserID = @UserID AND Password = @CurrentPassword",
                    conn))
                {
                    verifyCmd.Parameters.AddWithValue("@UserID", userId);
                    verifyCmd.Parameters.AddWithValue("@CurrentPassword", currentPassword);

                    int matchCount = (int)verifyCmd.ExecuteScalar();
                    if (matchCount == 0)
                    {
                        lblMessage.Text = "Current password is incorrect.";
                        lblMessage.CssClass = "validation-error mt-3 d-block";
                        return;
                    }
                }

                // Update password
                using (SqlCommand updateCmd = new SqlCommand(
                    "UPDATE Users SET Password = @NewPassword WHERE UserID = @UserID",
                    conn))
                {
                    updateCmd.Parameters.AddWithValue("@UserID", userId);
                    updateCmd.Parameters.AddWithValue("@NewPassword", newPassword);

                    try
                    {
                        updateCmd.ExecuteNonQuery();
                        lblMessage.Text = "Password updated successfully.";
                        lblMessage.CssClass = "success-message mt-3 d-block";

                        // Clear password fields
                        txtCurrentPassword.Text = "";
                        txtNewPassword.Text = "";
                        txtConfirmPassword.Text = "";
                    }
                    catch (Exception ex)
                    {
                        lblMessage.Text = "An error occurred while updating the password.";
                        lblMessage.CssClass = "validation-error mt-3 d-block";
                        System.Diagnostics.Debug.WriteLine($"Password update error: {ex.Message}");
                    }
                }
            }
        }
    }
}