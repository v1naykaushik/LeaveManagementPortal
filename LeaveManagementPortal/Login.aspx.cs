﻿using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Security;
using WebGrease.Activities;

namespace LeaveManagementPortal
{
    /// <summary>
    /// Login page code-behind class that handles user authentication
    /// </summary>
    public partial class Login : System.Web.UI.Page
    {
        /// <summary>
        /// Page load event handler
        /// Redirects to dashboard if user is already authenticated
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            // Check if user is already logged in
            //if (User.Identity.IsAuthenticated)
            //{
            //    Response.Redirect("~/Dashboard.aspx");
            //}
        }

        /// <summary>
        /// Handles the login button click event
        /// 
        /// Validates user credentials against the database and creates authentication ticket
        /// </summary>
        protected void btnLogin_Click(object sender, EventArgs e)
        {
            // Clear any previous error messages
            lblError.Text = "";

            // Check if password is empty after trim
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                lblError.Text = "Password is required.";
                return;
            }

            if (Page.IsValid)
            {
                try
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        // ADD THIS NEW BLOCK: Check if account exists but is inactive
                        using (SqlCommand checkActiveCmd = new SqlCommand(@"
                    SELECT IsActive 
                    FROM Users 
                    WHERE Email = @Email", conn))
                        {
                            checkActiveCmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());

                            using (SqlDataReader reader = checkActiveCmd.ExecuteReader())
                            {
                                if (reader.Read() && !Convert.ToBoolean(reader["IsActive"]))
                                {
                                    lblError.Text = "This account is inactive. Please contact your admin.";
                                    return;
                                }
                            }
                        }
                        // END OF NEW BLOCK

                        // Existing credential check code remains exactly the same
                        using (SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        u.UserID, 
                        u.FirstName, 
                        u.MiddleName, 
                        u.LastName, 
                        u.Email, 
                        u.Role, 
                        u.ManagerID, 
                        d.Name AS DesignationName, 
                        t.Name AS TitleName 
                    FROM Users u
                    LEFT JOIN DesignationMaster d ON u.Designation = d.id
                    LEFT JOIN TitlesMaster t ON u.Title = t.id
                    WHERE u.Email = @Email AND u.Password = @Password AND u.IsActive = 1", conn))
                        {
                            // Add parameters to prevent SQL injection
                            cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                            cmd.Parameters.AddWithValue("@Password", txtPassword.Text.Trim());

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    Session["UserID"] = reader["UserID"].ToString();
                                    //Session["UserName"] = reader["Name"].ToString();
                                    Session["UserRole"] = reader["Role"].ToString();
                                    Session["ManagerID"] = reader["ManagerID"].ToString();
                                    // Concatenating FirstName, MiddleName, and LastName (ignoring null values)
                                    string fullName = reader["FirstName"].ToString();
                                    if (!string.IsNullOrEmpty(reader["MiddleName"].ToString()))
                                        fullName += " " + reader["MiddleName"].ToString();
                                    if (!string.IsNullOrEmpty(reader["LastName"].ToString()))
                                        fullName += " " + reader["LastName"].ToString();

                                    Session["UserName"] = fullName.Trim(); // Store formatted name in Session

                                    // Storing Designation and Title Names in Session
                                    Session["UserDesignation"] = reader["DesignationName"].ToString();
                                    Session["UserTitle"] = reader["TitleName"].ToString();
                                    FormsAuthentication.SetAuthCookie(reader["Email"].ToString(), false);

                                    string userRole = reader["Role"].ToString();
                                    switch (userRole.ToLower())
                                    {
                                        case "director":
                                        case "manager":
                                            Response.Redirect("~/AdminDashboard.aspx");
                                            break;
                                        default:
                                            Response.Redirect("~/Dashboard.aspx");
                                            break;
                                    }
                                }
                                else
                                {
                                    lblError.Text = "Invalid email or password. Please try again.";
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception (implement proper logging)
                    lblError.Text = "An error occurred. Please try again later.";
                    System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
                }
            }
        }
    }
}