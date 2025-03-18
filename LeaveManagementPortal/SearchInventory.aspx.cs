using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LeaveManagementPortal
{
    public partial class SearchInventory : Page
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

                // Load all inventory items by default
                SearchInventoryItems();
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

                    ddlSearchCategory.DataSource = dt;
                    ddlSearchCategory.DataTextField = "category_name";
                    ddlSearchCategory.DataValueField = "category_id";
                    ddlSearchCategory.DataBind();

                    // Add default "All Categories" item
                    ddlSearchCategory.Items.Insert(0, new ListItem("-- All Categories --", "0"));
                }
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            SearchInventoryItems();
        }

        private void SearchInventoryItems()
        {
            string searchName = txtSearchName.Text.Trim();
            string categoryId = ddlSearchCategory.SelectedValue;
            string stockStatus = ddlStockStatus.SelectedValue;

            string connectionString = ConfigurationManager.ConnectionStrings["LeaveManagementDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT i.ID, i.Name, ic.category_name AS CategoryName, 
                            i.InitialQuantity, i.AllotedQuantity, i.price, i.Photo, 
                           i.CreatedDate, i.ModifiedDate
                    FROM Inventory i
                    INNER JOIN InventoryCategory ic ON i.CategoryID = ic.category_id
                    WHERE 1=1";

                // Add search conditions
                if (!string.IsNullOrEmpty(searchName))
                {
                    query += " AND i.Name LIKE @Name";
                }

                if (categoryId != "0") // If specific category selected
                {
                    query += " AND i.CategoryID = @CategoryID";
                }

                switch (stockStatus)
                {
                    case "instock":
                        query += " AND (i.InitialQuantity - i.AllotedQuantity) > 0";
                        break;
                    case "outofstock":
                        query += " AND (i.InitialQuantity - i.AllotedQuantity) = 0";
                        break;
                    case "lowstock":
                        query += " AND (i.InitialQuantity - i.AllotedQuantity) > 0 AND (i.InitialQuantity - i.AllotedQuantity) < 10";
                        break;
                }

                query += " ORDER BY i.Name";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Add parameters
                    if (!string.IsNullOrEmpty(searchName))
                    {
                        cmd.Parameters.AddWithValue("@Name", "%" + searchName + "%");
                    }

                    if (categoryId != "0")
                    {
                        cmd.Parameters.AddWithValue("@CategoryID", categoryId);
                    }

                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // Compute ItemQuantity in C# and add it as a new column
                    dt.Columns.Add("ItemQuantity", typeof(int));
                    foreach (DataRow row in dt.Rows)
                    {
                        int initialQuantity = row["InitialQuantity"] != DBNull.Value ? Convert.ToInt32(row["InitialQuantity"]) : 0;
                        int allotedQuantity = row["AllotedQuantity"] != DBNull.Value ? Convert.ToInt32(row["AllotedQuantity"]) : 0;
                        row["ItemQuantity"] = initialQuantity - allotedQuantity;
                    }

                    gvInventory.DataSource = dt;
                    gvInventory.DataBind();
                }
            }
        }

        protected void gvInventory_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvInventory.PageIndex = e.NewPageIndex;
            SearchInventoryItems();
        }
    }
}