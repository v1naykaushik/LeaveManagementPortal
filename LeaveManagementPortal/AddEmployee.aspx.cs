using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Linq;
using WebGrease.Activities;

namespace LeaveManagementPortal
{
    public partial class AddEmployee : Page
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
                LoadManagers();
            }
        }

        private void LoadManagers()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT UserID, Name 
                    FROM Users 
                    WHERE Role = 'Manager' AND IsActive = 1 
                    ORDER BY Name", conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        ddlManager.Items.Clear();
                        while (reader.Read())
                        {
                            ddlManager.Items.Add(new System.Web.UI.WebControls.ListItem(
                                reader["Name"].ToString(),
                                reader["UserID"].ToString()
                            ));
                        }
                        if (ddlManager.Items.Count > 0)
                        {
                            ddlManager.SelectedIndex = 0;
                        }
                    }
                }
            }
        }

        protected void btnAddEmployee_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
                System.Diagnostics.Debug.WriteLine("\n=== Starting New Employee Addition ===");
                System.Diagnostics.Debug.WriteLine($"Attempting to add employee with email: {txtEmail.Text.Trim()}");

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        System.Diagnostics.Debug.WriteLine("Database connection opened successfully");

                        // Let's see ALL emails in the database
                        using (SqlCommand listCmd = new SqlCommand(
                            "SELECT UserID, Email FROM Users", conn))
                        {
                            using (SqlDataReader reader = listCmd.ExecuteReader())
                            {
                                System.Diagnostics.Debug.WriteLine("\nCurrent emails in database:");
                                while (reader.Read())
                                {
                                    System.Diagnostics.Debug.WriteLine($"ID: {reader["UserID"]}, Email: {reader["Email"]}");
                                }
                            }
                        }

                        // Now check for our specific email
                        using (SqlCommand checkCmd = new SqlCommand(
                            "SELECT COUNT(*) FROM Users WHERE Email = @Email", conn))
                        {
                            string emailToAdd = txtEmail.Text.Trim();
                            checkCmd.Parameters.AddWithValue("@Email", emailToAdd);
                            int existingCount = (int)checkCmd.ExecuteScalar();
                            System.Diagnostics.Debug.WriteLine($"\nFound {existingCount} users with email: {emailToAdd}");

                            if (existingCount > 0)
                            {
                                lblError.Text = "An employee with this email already exists.";
                                pnlSuccess.Visible = false;
                                return;
                            }
                        }

                        // Now try to insert the new user
                        using (SqlCommand insertCmd = new SqlCommand(@"
                    INSERT INTO Users (Name, Email, Password, Role, ManagerID, IsActive, EmployeeOfficeID)
                    VALUES (@Name, @Email, @Password, 'Employee', @ManagerID, 1, @EmployeeOfficeID)", conn))
                        {
                            string email = txtEmail.Text.Trim();
                            string firstThree = email.Length >= 3 ? email.Substring(0, 3).ToLower() : email.ToLower();
                            int asciiSum = firstThree.ToCharArray().Sum(c => (int)c);
                            string password = $"{firstThree}@{asciiSum}";

                            insertCmd.Parameters.AddWithValue("@Name", txtName.Text.Trim());
                            insertCmd.Parameters.AddWithValue("@Email", email);
                            insertCmd.Parameters.AddWithValue("@Password", password);
                            insertCmd.Parameters.AddWithValue("@ManagerID", ddlManager.SelectedValue);
                            insertCmd.Parameters.AddWithValue("@EmployeeOfficeID", txtEmployeeId.Text.Trim());

                            System.Diagnostics.Debug.WriteLine("\nExecuting INSERT command with values:");
                            System.Diagnostics.Debug.WriteLine($"Name: {txtName.Text.Trim()}");
                            System.Diagnostics.Debug.WriteLine($"Email: {email}");
                            System.Diagnostics.Debug.WriteLine($"ManagerID: {ddlManager.SelectedValue}");
                            System.Diagnostics.Debug.WriteLine($"EmployeeOfficeID: {txtEmployeeId.Text.Trim()}");

                            int rowsAffected = insertCmd.ExecuteNonQuery();
                            System.Diagnostics.Debug.WriteLine($"\nINSERT affected {rowsAffected} rows");

                            if (rowsAffected > 0)
                            {
                                // Store the generated password to show in success message
                                lblGeneratedPassword.Text = password;

                                // Clear form and show success message
                                txtName.Text = "";
                                txtEmail.Text = "";
                                txtEmployeeId.Text = "";
                                ddlManager.SelectedIndex = 0;
                                lblError.Text = "";
                                pnlSuccess.Visible = true;
                            }
                            else
                            {
                                lblError.Text = "Failed to insert new employee record.";
                                pnlSuccess.Visible = false;
                            }
                        }

                        // Verify the insert worked by checking again
                        using (SqlCommand verifyCmd = new SqlCommand(
                            "SELECT UserID, Email FROM Users WHERE Email = @Email", conn))
                        {
                            verifyCmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                            using (SqlDataReader reader = verifyCmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    System.Diagnostics.Debug.WriteLine($"\nVerification - Found user after insert: ID={reader["UserID"]}, Email={reader["Email"]}");
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine("\nVerification FAILED - User not found after insert!");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"\nERROR: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                        lblError.Text = $"Database error: {ex.Message}";
                        pnlSuccess.Visible = false;
                    }
                }
            }
        }
    }
}
