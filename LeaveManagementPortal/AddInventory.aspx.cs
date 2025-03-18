using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LeaveManagementPortal
{
    public partial class AddInventory : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Check if user is logged in
                if (Session["UserID"] == null)
                {
                    Response.Redirect("~/Login.aspx");
                    return;
                }

                // Load categories into dropdown
                LoadCategories();
            }
        }

        private void LoadCategories()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT category_id, category_name FROM InventoryCategory ORDER BY category_name", conn))
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    ddlCategory.DataSource = dt;
                    ddlCategory.DataTextField = "category_name";
                    ddlCategory.DataValueField = "category_id";
                    ddlCategory.DataBind();

                    // Add default "Select Category" item
                    ddlCategory.Items.Insert(0, new ListItem("-- Select Category --", "0"));
                }
            }
        }

        private string SaveUploadedImage(FileUpload fileUpload)
        {
            if (!fileUpload.HasFile)
                return null;

            string fileExtension = Path.GetExtension(fileUpload.FileName).ToLower();
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };

            if (Array.IndexOf(allowedExtensions, fileExtension) < 0)
                throw new Exception("Invalid file type. Please upload a .jpg, .jpeg or .png file.");

            // Ensure upload directory exists
            string relativePath = "~/Uploads/Inventory/";
            string physicalPath = Server.MapPath(relativePath);

            System.Diagnostics.Debug.WriteLine("Relative path: " + relativePath);
            System.Diagnostics.Debug.WriteLine("Physical path: " + physicalPath);
            System.Diagnostics.Debug.WriteLine("Directory exists: " + Directory.Exists(physicalPath));

            if (!Directory.Exists(physicalPath))
                Directory.CreateDirectory(physicalPath);

            System.Diagnostics.Debug.WriteLine("After creation, directory exists: " + Directory.Exists(physicalPath));

            // Create a unique filename to prevent collisions
            string uniqueFileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" +
                               Path.GetFileNameWithoutExtension(Guid.NewGuid().ToString()) +
                               fileExtension;

            // Save the file to the server
            string filePath = Path.Combine(physicalPath, uniqueFileName);

            System.Diagnostics.Debug.WriteLine("Saving file to: " + filePath);
            try
            {
                // Wrap the actual file save in a try-catch for better error reporting
                fileUpload.SaveAs(filePath);
                System.Diagnostics.Debug.WriteLine("File saved successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error saving file: " + ex.Message);
                throw; // Re-throw to maintain original error handling
            }

            // Return the relative URL that will be stored in the database
            return relativePath + uniqueFileName;
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;

            string userId = Session["UserID"]?.ToString();
            if (string.IsNullOrEmpty(userId))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string name = txtName.Text.Trim();
            int categoryId = Convert.ToInt32(ddlCategory.SelectedValue);
            int quantity = Convert.ToInt32(txtQuantity.Text.Trim());
            //decimal price = Convert.ToDecimal(txtPrice.Text.Trim());
            decimal? price = string.IsNullOrWhiteSpace(txtPrice.Text)
                ? (decimal?)null
                : Convert.ToDecimal(txtPrice.Text.Trim());
            string photoUrl = null;

            try
            {
                // Save image and get URL to store in database
                if (fuPhoto.HasFile)
                {
                    photoUrl = SaveUploadedImage(fuPhoto);
                    System.Diagnostics.Debug.WriteLine("Photo URL: " + photoUrl);
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error uploading file: " + ex.Message;
                lblMessage.CssClass = "validation-error mt-3 d-block";
                return;
            }

            // Save inventory to database
            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    INSERT INTO Inventory (Name, InitialQuantity, price, CategoryID, Photo, CreatedBy, ModifiedDate)
                    VALUES (@Name, @InitialQuantity, @price, @CategoryID, @Photo, @CreatedBy, @ModifiedDate)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@InitialQuantity", quantity);
                    cmd.Parameters.AddWithValue("@price", (object)price ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CategoryID", categoryId);
                    cmd.Parameters.AddWithValue("@Photo", string.IsNullOrEmpty(photoUrl) ? DBNull.Value : (object)photoUrl);
                    cmd.Parameters.AddWithValue("@CreatedBy", Convert.ToInt32(userId));
                    //cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);

                    try
                    {
                        conn.Open();
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            // Clear form after successful save
                            txtName.Text = "";
                            ddlCategory.SelectedIndex = 0;
                            txtQuantity.Text = "";
                            txtPrice.Text = "";

                            lblMessage.Text = "Inventory item added successfully!";
                            lblMessage.CssClass = "success-message mt-3 d-block";
                        }
                        else
                        {
                            lblMessage.Text = "Failed to add inventory item.";
                            lblMessage.CssClass = "validation-error mt-3 d-block";
                        }
                    }
                    catch (Exception ex)
                    {
                        lblMessage.Text = "Error: " + ex.Message;
                        lblMessage.CssClass = "validation-error mt-3 d-block";
                    }
                }
            }
        }
    }
}