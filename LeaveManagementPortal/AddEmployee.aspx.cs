using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Linq;
using WebGrease.Activities;
using System.Web.UI.WebControls;


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
                //LoadManagers();
                LoadDropdowns();
            }
        }

        private void LoadDropdowns()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
            -- Get Managers
            SELECT 
                UserID, 
                (COALESCE(FirstName, '') +
                            CASE WHEN MiddleName IS NOT NULL AND MiddleName<> '' THEN ' ' + MiddleName ELSE '' END +
                            CASE WHEN LastName IS NOT NULL AND LastName<> '' THEN ' ' + LastName ELSE '' END)
                            AS Name
            FROM Users WHERE Role IN ('Manager', 'Director') AND IsActive = 1 ORDER BY Name;

            -- Get Titles
            SELECT ID, Name FROM TitlesMaster ORDER BY Name;

            -- Get Designations
            SELECT ID, Name FROM DesignationMaster ORDER BY Name;
        ", conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // Load Managers
                        ddlManager.Items.Clear();
                        while (reader.Read())
                        {
                            ddlManager.Items.Add(new ListItem(reader["Name"].ToString(), reader["UserID"].ToString()));
                        }
                        if (ddlManager.Items.Count > 0) ddlManager.SelectedIndex = 0;

                        // Move to Titles result set
                        if (reader.NextResult())
                        {
                            ddlTitle.Items.Clear();
                            while (reader.Read())
                            {
                                ddlTitle.Items.Add(new ListItem(reader["Name"].ToString(), reader["ID"].ToString()));
                            }
                            if (ddlTitle.Items.Count > 0) ddlTitle.SelectedIndex = 0;
                        }

                        // Move to Designations result set
                        if (reader.NextResult())
                        {
                            ddlDesignation.Items.Clear();
                            while (reader.Read())
                            {
                                ddlDesignation.Items.Add(new ListItem(reader["Name"].ToString(), reader["ID"].ToString()));
                            }
                            if (ddlDesignation.Items.Count > 0) ddlDesignation.SelectedIndex = 0;
                        }
                    }
                }
            }
        }


        //protected void btnAddEmployee_Click(object sender, EventArgs e)
        //{
        //    if (Page.IsValid)
        //    {
        //        string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
        //        System.Diagnostics.Debug.WriteLine("\n=== Starting New Employee Addition ===");
        //        System.Diagnostics.Debug.WriteLine($"Attempting to add employee with email: {txtEmail.Text.Trim()}");

        //        using (SqlConnection conn = new SqlConnection(connectionString))
        //        {
        //            try
        //            {
        //                conn.Open();
        //                System.Diagnostics.Debug.WriteLine("Database connection opened successfully");

        //                // Let's see ALL emails in the database
        //                using (SqlCommand listCmd = new SqlCommand(
        //                    "SELECT UserID, Email FROM Users", conn))
        //                {
        //                    using (SqlDataReader reader = listCmd.ExecuteReader())
        //                    {
        //                        System.Diagnostics.Debug.WriteLine("\nCurrent emails in database:");
        //                        while (reader.Read())
        //                        {
        //                            System.Diagnostics.Debug.WriteLine($"ID: {reader["UserID"]}, Email: {reader["Email"]}");
        //                        }
        //                    }
        //                }

        //                // Now check for our specific email
        //                using (SqlCommand checkCmd = new SqlCommand(
        //                    "SELECT COUNT(*) FROM Users WHERE Email = @Email", conn))
        //                {
        //                    string emailToAdd = txtEmail.Text.Trim();
        //                    checkCmd.Parameters.AddWithValue("@Email", emailToAdd);
        //                    int existingCount = (int)checkCmd.ExecuteScalar();
        //                    System.Diagnostics.Debug.WriteLine($"\nFound {existingCount} users with email: {emailToAdd}");

        //                    if (existingCount > 0)
        //                    {
        //                        lblError.Text = "An employee with this email already exists.";
        //                        pnlSuccess.Visible = false;
        //                        return;
        //                    }
        //                }

        //                // Now try to insert the new user
        //                using (SqlCommand insertCmd = new SqlCommand(@"
        //            INSERT INTO Users (Name, Email, Password, Role, ManagerID, IsActive, EmployeeOfficeID)
        //            VALUES (@Name, @Email, @Password, 'Employee', @ManagerID, 1, @EmployeeOfficeID)", conn))
        //                {
        //                    string email = txtEmail.Text.Trim();
        //                    string firstThree = email.Length >= 3 ? email.Substring(0, 3).ToLower() : email.ToLower();
        //                    int asciiSum = firstThree.ToCharArray().Sum(c => (int)c);
        //                    string password = $"{firstThree}@{asciiSum}";

        //                    insertCmd.Parameters.AddWithValue("@Name", txtName.Text.Trim());
        //                    insertCmd.Parameters.AddWithValue("@Email", email);
        //                    insertCmd.Parameters.AddWithValue("@Password", password);
        //                    insertCmd.Parameters.AddWithValue("@ManagerID", ddlManager.SelectedValue);
        //                    insertCmd.Parameters.AddWithValue("@EmployeeOfficeID", txtEmployeeId.Text.Trim());

        //                    System.Diagnostics.Debug.WriteLine("\nExecuting INSERT command with values:");
        //                    System.Diagnostics.Debug.WriteLine($"Name: {txtName.Text.Trim()}");
        //                    System.Diagnostics.Debug.WriteLine($"Email: {email}");
        //                    System.Diagnostics.Debug.WriteLine($"ManagerID: {ddlManager.SelectedValue}");
        //                    System.Diagnostics.Debug.WriteLine($"EmployeeOfficeID: {txtEmployeeId.Text.Trim()}");

        //                    int rowsAffected = insertCmd.ExecuteNonQuery();
        //                    System.Diagnostics.Debug.WriteLine($"\nINSERT affected {rowsAffected} rows");

        //                    if (rowsAffected > 0)
        //                    {
        //                        // Store the generated password to show in success message
        //                        lblGeneratedPassword.Text = password;

        //                        // Clear form and show success message
        //                        txtName.Text = "";
        //                        txtEmail.Text = "";
        //                        txtEmployeeId.Text = "";
        //                        ddlManager.SelectedIndex = 0;
        //                        lblError.Text = "";
        //                        pnlSuccess.Visible = true;
        //                    }
        //                    else
        //                    {
        //                        lblError.Text = "Failed to insert new employee record.";
        //                        pnlSuccess.Visible = false;
        //                    }
        //                }

        //                // Verify the insert worked by checking again
        //                using (SqlCommand verifyCmd = new SqlCommand(
        //                    "SELECT UserID, Email FROM Users WHERE Email = @Email", conn))
        //                {
        //                    verifyCmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
        //                    using (SqlDataReader reader = verifyCmd.ExecuteReader())
        //                    {
        //                        if (reader.Read())
        //                        {
        //                            System.Diagnostics.Debug.WriteLine($"\nVerification - Found user after insert: ID={reader["UserID"]}, Email={reader["Email"]}");
        //                        }
        //                        else
        //                        {
        //                            System.Diagnostics.Debug.WriteLine("\nVerification FAILED - User not found after insert!");
        //                        }
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                System.Diagnostics.Debug.WriteLine($"\nERROR: {ex.Message}");
        //                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        //                lblError.Text = $"Database error: {ex.Message}";
        //                pnlSuccess.Visible = false;
        //            }
        //        }
        //    }
        //}

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

                        // Begin a transaction to ensure both user and leave balances are created
                        SqlTransaction transaction = conn.BeginTransaction();
                        try
                        {
                            int newUserID = 0;

                            // Now try to insert the new user and get the new UserID
                            using (SqlCommand insertCmd = new SqlCommand(@"
                        INSERT INTO Users (Name, FirstName, MiddleName, LastName, Email, Password, Role, ManagerID, IsActive, EmployeeOfficeID, Title, Designation)
                        VALUES (@Name, @FirstName, @MiddleName, @LastName, @Email, @Password, @Role, @ManagerID, 1, @EmployeeOfficeID, @ddlTitle ,@ddlDesignation);
                        SELECT SCOPE_IDENTITY();", conn, transaction))
                            {
                                string email = txtEmail.Text.Trim();
                                string firstThree = email.Length >= 3 ? email.Substring(0, 3).ToLower() : email.ToLower();
                                int asciiSum = firstThree.ToCharArray().Sum(c => (int)c);
                                string password = $"{firstThree}@{asciiSum}";
                                string role;

                                if (ddlDesignation.SelectedValue == "1")
                                {
                                    role = "Director";
                                }
                                else if (ddlDesignation.SelectedValue == "2")
                                {
                                    role = "Manager";
                                }
                                else
                                {
                                    role = "Employee";
                                }

                                insertCmd.Parameters.AddWithValue("@Name", "AfterBurn");
                                insertCmd.Parameters.AddWithValue("@FirstName", txtFirstName.Text.Trim());
                                insertCmd.Parameters.AddWithValue("@MiddleName", txtMiddleName.Text.Trim());
                                insertCmd.Parameters.AddWithValue("@LastName", txtLastName.Text.Trim());
                                insertCmd.Parameters.AddWithValue("@Email", email);
                                insertCmd.Parameters.AddWithValue("@Password", password);
                                insertCmd.Parameters.AddWithValue("@Role", role);
                                insertCmd.Parameters.AddWithValue("@ManagerID", ddlManager.SelectedValue);
                                insertCmd.Parameters.AddWithValue("@EmployeeOfficeID", txtEmployeeId.Text.Trim());
                                insertCmd.Parameters.AddWithValue("@ddlTitle", ddlTitle.SelectedValue);
                                insertCmd.Parameters.AddWithValue("@ddlDesignation", ddlDesignation.SelectedValue);


                                System.Diagnostics.Debug.WriteLine("\nExecuting INSERT command with values:");
                                System.Diagnostics.Debug.WriteLine($"Full Name: {txtFirstName.Text.Trim()} + {txtMiddleName.Text.Trim()} + {txtLastName.Text.Trim()}");
                                System.Diagnostics.Debug.WriteLine($"Email: {email}");
                                System.Diagnostics.Debug.WriteLine($"ManagerID: {ddlManager.SelectedValue}");
                                System.Diagnostics.Debug.WriteLine($"EmployeeOfficeID: {txtEmployeeId.Text.Trim()}");

                                // Get the newly generated UserID
                                newUserID = Convert.ToInt32(insertCmd.ExecuteScalar());
                                System.Diagnostics.Debug.WriteLine($"\nNew user inserted with UserID: {newUserID}");

                                // Now insert default leave balances for this new user
                                if (newUserID > 0)
                                {
                                    // Insert default leave balance for LeaveTypeID 1
                                    InsertDefaultLeaveBalance(newUserID, 1, 8.0, 8.0, conn, transaction);

                                    // Insert default leave balance for LeaveTypeID 2
                                    InsertDefaultLeaveBalance(newUserID, 2, 15.0, 15.0, conn, transaction);

                                    // Insert default leave balance for LeaveTypeID 3
                                    InsertDefaultLeaveBalance(newUserID, 3, 10.0, 10.0, conn, transaction);

                                    // Insert default leave balance for LeaveTypeID 4
                                    InsertDefaultLeaveBalance(newUserID, 4, 2.0, 2.0, conn, transaction);

                                    // Insert default leave balance for LeaveTypeID 5
                                    InsertDefaultLeaveBalance(newUserID, 5, 360.0, 360.0, conn, transaction);

                                    System.Diagnostics.Debug.WriteLine("\nInserted default leave balances for the new user");

                                    // Store the generated password to show in success message
                                    lblGeneratedPassword.Text = password;

                                    // Clear form and show success message
                                    ddlTitle.SelectedIndex = 0;
                                    txtFirstName.Text = "";
                                    txtMiddleName.Text = "";
                                    txtLastName.Text = "";
                                    txtEmail.Text = "";
                                    txtEmployeeId.Text = "";
                                    ddlDesignation.SelectedIndex = 0;
                                    ddlManager.SelectedIndex = 0;
                                    lblError.Text = "";
                                    pnlSuccess.Visible = true;

                                    // Commit the transaction
                                    transaction.Commit();
                                    System.Diagnostics.Debug.WriteLine("\nTransaction committed successfully");
                                }
                                else
                                {
                                    transaction.Rollback();
                                    lblError.Text = "Failed to insert new employee record.";
                                    pnlSuccess.Visible = false;
                                    System.Diagnostics.Debug.WriteLine("\nTransaction rolled back - failed to get new UserID");
                                }
                            }

                            // Verify the user and leave balances were created correctly
                            VerifyUserAndLeaveBalances(newUserID, conn);
                        }
                        catch (Exception ex)
                        {
                            // Roll back the transaction if there's an error
                            transaction.Rollback();
                            System.Diagnostics.Debug.WriteLine($"\nTransaction rolled back due to ERROR: {ex.Message}");
                            throw; // Re-throw to be caught by the outer catch block
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

        // Helper method to insert a default leave balance record
        private void InsertDefaultLeaveBalance(int userID, int leaveTypeID, double presentYearBalance,
            double newYearBalance, SqlConnection conn, SqlTransaction transaction)
        {
            using (SqlCommand insertLeaveBalanceCmd = new SqlCommand(@"
        INSERT INTO LeaveBalances (UserID, LeaveTypeID, PresentYearBalance, NewYearBalance)
        VALUES (@UserID, @LeaveTypeID, @PresentYearBalance, @NewYearBalance)", conn, transaction))
            {
                insertLeaveBalanceCmd.Parameters.AddWithValue("@UserID", userID);
                insertLeaveBalanceCmd.Parameters.AddWithValue("@LeaveTypeID", leaveTypeID);
                insertLeaveBalanceCmd.Parameters.AddWithValue("@PresentYearBalance", presentYearBalance);
                insertLeaveBalanceCmd.Parameters.AddWithValue("@NewYearBalance", newYearBalance);

                int rowsAffected = insertLeaveBalanceCmd.ExecuteNonQuery();
                System.Diagnostics.Debug.WriteLine($"Inserted leave balance for LeaveTypeID {leaveTypeID}, rows affected: {rowsAffected}");
            }
        }

        // Helper method to verify the user and leave balances were created correctly
        private void VerifyUserAndLeaveBalances(int userID, SqlConnection conn)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();

            // Verify user was created
            using (SqlCommand verifyUserCmd = new SqlCommand(
                "SELECT UserID, Email FROM Users WHERE UserID = @UserID", conn))
            {
                verifyUserCmd.Parameters.AddWithValue("@UserID", userID);
                using (SqlDataReader reader = verifyUserCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        System.Diagnostics.Debug.WriteLine($"\nVerification - Found user: ID={reader["UserID"]}, Email={reader["Email"]}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("\nVerification FAILED - User not found!");
                    }
                }
            }

            // Verify leave balances were created
            using (SqlCommand verifyLeaveCmd = new SqlCommand(
                "SELECT LeaveTypeID, PresentYearBalance, NewYearBalance FROM LeaveBalances WHERE UserID = @UserID", conn))
            {
                verifyLeaveCmd.Parameters.AddWithValue("@UserID", userID);
                using (SqlDataReader reader = verifyLeaveCmd.ExecuteReader())
                {
                    System.Diagnostics.Debug.WriteLine("\nVerification - Leave balances for new user:");
                    int count = 0;
                    while (reader.Read())
                    {
                        count++;
                        System.Diagnostics.Debug.WriteLine($"LeaveTypeID: {reader["LeaveTypeID"]}, " +
                            $"PresentYearBalance: {reader["PresentYearBalance"]}, " +
                            $"NewYearBalance: {reader["NewYearBalance"]}");
                    }

                    if (count == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("\nVerification FAILED - No leave balances found for new user!");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"\nVerification SUCCESS - Found {count} leave balance records for new user");
                    }
                }
            }
        }
    }
}