using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LeaveManagementPortal
{
    public partial class ItemAllotment : System.Web.UI.Page
    {
        private int itemID;
        private int availableQuantity;
        //private DateTime allotmentDate = new DateTime(2025, 1, 1);

        protected void Page_Load(object sender, EventArgs e)
        {
            // Get the ItemID from query string
            if (!int.TryParse(Request.QueryString["ItemID"], out itemID))
            {
                // Invalid ItemID
                pnlItemDetails.Visible = false;
                pnlItemNotFound.Visible = true;
                System.Diagnostics.Debug.WriteLine("itemID in page load is : " + itemID);
                return;
            }

            System.Diagnostics.Debug.WriteLine("itemID in page load is2 : " + itemID);

            if (!IsPostBack)
            {
                LoadItemDetails();
                LoadAllotments();
            }
        }

        private void LoadItemDetails()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Simplified query that only uses the Inventory table
                string query = @"
            SELECT ID, Name AS ItemName, (InitialQuantity - AllotedQuantity) AS ItemQuantity, price, Photo
            FROM Inventory
            WHERE ID = @ItemID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ItemID", itemID);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        // Populate item details with correct column names
                        lblItemID.Text = reader["ID"].ToString();
                        lblItemName.Text = reader["ItemName"].ToString();
                        // Category field is removed
                        availableQuantity = Convert.ToInt32(reader["ItemQuantity"]);
                        lblQuantity.Text = availableQuantity.ToString();
                        ViewState["AvailableQuantity"] = availableQuantity;
                        lblPrice.Text = reader["price"] == DBNull.Value || reader["price"] == null || string.IsNullOrEmpty(reader["price"].ToString())
                            ? "N/A"
                            : "₹ " + String.Format("{0:N0}", Convert.ToDecimal(reader["price"]));
                        // Set item image
                        string photoUrl = reader["Photo"].ToString();
                        if (!string.IsNullOrEmpty(photoUrl))
                        {
                            imgItem.ImageUrl = photoUrl;
                        }
                        else
                        {
                            imgItem.ImageUrl = "~/Images/no-image.png";
                        }
                    }
                    else
                    {
                        // Item not found
                        pnlItemDetails.Visible = false;
                        pnlItemNotFound.Visible = true;
                    }
                    reader.Close();
                }
            }
        }

        private void LoadAllotments()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT ID, Name, Organization, Quantity, AllotmentDate
                    FROM InventoryAllotment
                    WHERE ItemID = @ItemID
                    ORDER BY AllotmentDate DESC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ItemID", itemID);

                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dtAllotments = new DataTable();

                    adapter.Fill(dtAllotments);
                    gvAllotments.DataSource = dtAllotments;
                    gvAllotments.DataBind();
                }
            }
        }


        protected void cvQuantity_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (!int.TryParse(args.Value, out int requestedQuantity))
            {
                args.IsValid = false;
                return;
            }

            // Debugging Output
            System.Diagnostics.Debug.WriteLine("Requested Quantity: " + requestedQuantity);
            System.Diagnostics.Debug.WriteLine("ViewState Available Quantity: " + (ViewState["AvailableQuantity"] ?? "NULL"));

            int storedAvailableQuantity = ViewState["AvailableQuantity"] != null ? (int)ViewState["AvailableQuantity"] : 0;
            System.Diagnostics.Debug.WriteLine("Stored Available Quantity: " + storedAvailableQuantity);

            args.IsValid = (requestedQuantity <= storedAvailableQuantity);
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;

            string personName = txtPersonName.Text.Trim();
            string organization = string.IsNullOrWhiteSpace(txtOrganization.Text) ? "N/A" : txtOrganization.Text.Trim();
            int quantity = Convert.ToInt32(txtQuantity.Text);
            string remarks = string.IsNullOrWhiteSpace(txtRemarks.Text) ? "No remarks" : txtRemarks.Text.Trim();

            DateTime allotmentDate;
            if (!DateTime.TryParse(txtAllotmentDate.Text, out allotmentDate))
            {
                lblSuccess.Text = "Invalid allotment date.";
                lblSuccess.CssClass = "validation-message";
                return;
            }

            System.Diagnostics.Debug.WriteLine("itemID in btnSave_Click is : " + itemID);

            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    System.Diagnostics.Debug.WriteLine("allotmentDate from btnSave_Click: " + allotmentDate);
                    // 1. Insert new allotment record
                    string insertQuery = @"
                        INSERT INTO InventoryAllotment (ItemID, Name, Organization, Quantity, Remarks, AllotmentDate)
                        VALUES (@ItemID, @Name, @Organization, @Quantity, @Remarks, @AllotmentDate);
                        SELECT SCOPE_IDENTITY();";

                    using (SqlCommand command = new SqlCommand(insertQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@ItemID", itemID);
                        command.Parameters.AddWithValue("@Name", personName);
                        command.Parameters.AddWithValue("@Organization", (object)organization ?? "N/A");
                        command.Parameters.AddWithValue("@Quantity", quantity);
                        command.Parameters.AddWithValue("@Remarks", (object)remarks ?? "No remarks");
                        command.Parameters.AddWithValue("@AllotmentDate", allotmentDate);

                        int allotmentID = Convert.ToInt32(command.ExecuteScalar());
                    }

                    string updateInventoryQuery = @"
                        UPDATE Inventory 
                        SET AllotedQuantity = ISNULL(AllotedQuantity, 0) + @Quantity,
                        ModifiedDate = GETDATE()
                        WHERE ID = @ItemID";
                    using (SqlCommand updateCommand = new SqlCommand(updateInventoryQuery, connection, transaction))
                    {
                        updateCommand.Parameters.AddWithValue("@ItemID", itemID);
                        updateCommand.Parameters.AddWithValue("@Quantity", quantity);
                        updateCommand.ExecuteNonQuery();
                    }

                    // Commit transaction
                    transaction.Commit();

                    // Show success message
                    pnlSuccess.Visible = true;
                    lblSuccess.Text = $"Item successfully allotted to {personName} ({quantity} units).";

                    // Clear form
                    txtPersonName.Text = "";
                    txtOrganization.Text = "";
                    txtQuantity.Text = "1";
                    txtRemarks.Text = "";
                    txtAllotmentDate.Text = "";

                    // Refresh data
                    LoadItemDetails();
                    LoadAllotments();
                }
                catch (Exception ex)
                {
                    // Rollback transaction on error
                    transaction.Rollback();

                    // Show error message
                    pnlSuccess.Visible = true;
                    lblSuccess.Text = $"Error: {ex.Message}";
                    lblSuccess.CssClass = "validation-message";
                }
            }
        }

        protected void gvAllotments_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvAllotments.PageIndex = e.NewPageIndex;
            LoadAllotments();
        }
    }
}