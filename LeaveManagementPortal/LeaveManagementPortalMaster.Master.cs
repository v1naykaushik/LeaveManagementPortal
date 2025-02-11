using System;
using System.Web.UI;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace LeaveManagementPortal
{
    public partial class LeaveManagementPortalMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Check authentication
                if (!Request.IsAuthenticated)
                {
                    Response.Redirect("~/Login.aspx");
                    return;
                }

                // Get user information from session
                string userRole = Session["UserRole"]?.ToString();
                string userName = Session["UserName"]?.ToString();

                if (string.IsNullOrEmpty(userRole) || string.IsNullOrEmpty(userName))
                {
                    // If session is expired or invalid, redirect to login
                    FormsAuthentication.SignOut();
                    Response.Redirect("~/Login.aspx");
                    return;
                }

                // Set user information in the UI
                litUserName.Text = userName;
                litUserRole.Text = userRole;

                // Configure menu visibility based on user role
                ConfigureMenuVisibility(userRole);

                // Set active menu item based on current page
                SetActiveMenuItem();
            }
        }

        private void ConfigureMenuVisibility(string userRole)
        {
            // Reset all menus to hidden by default
            EmployeeMenu.Visible = false;
            AdminMenu.Visible = false;

            // Show appropriate menus based on role
            switch (userRole.ToLower())
            {
                case "director":
                case "manager":
                    AdminMenu.Visible = true;
                    EmployeeMenu.Visible = true;
                    lnkDashboard.NavigateUrl = "~/AdminDashboard.aspx";
                    break;

                case "employee":
                    EmployeeMenu.Visible = true;
                    lnkDashboard.NavigateUrl = "~/Dashboard.aspx";
                    break;

                default:
                    // Invalid role, redirect to login
                    FormsAuthentication.SignOut();
                    Response.Redirect("~/Login.aspx");
                    break;
            }
        }

        private void SetActiveMenuItem()
        {
            // Get the current page URL
            string currentUrl = Request.Url.AbsolutePath.ToLower();

            // Remove active class from all links
            foreach (Control panel in new Control[] { EmployeeMenu, AdminMenu})
            {
                if (panel is Panel)
                {
                    foreach (Control control in panel.Controls)
                    {
                        if (control is HyperLink link)
                        {
                            link.CssClass = link.CssClass.Replace(" active", "");
                        }
                    }
                }
            }

            // Find and mark the active link
            foreach (Control panel in new Control[] { EmployeeMenu, AdminMenu})
            {
                if (panel is Panel)
                {
                    foreach (Control control in panel.Controls)
                    {
                        if (control is HyperLink link)
                        {
                            string linkUrl = ResolveUrl(link.NavigateUrl).ToLower();
                            if (currentUrl.EndsWith(linkUrl) ||
                                (currentUrl.Contains(linkUrl) && !linkUrl.EndsWith("default.aspx")))
                            {
                                link.CssClass = link.CssClass + " active";
                            }
                        }
                    }
                }
            }
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            try
            {
                // Clear all session variables
                Session.Clear();
                Session.Abandon();

                // Clear authentication cookie
                FormsAuthentication.SignOut();

                // Redirect to login page
                Response.Redirect("~/Login.aspx", true);
            }
            catch (Exception ex)
            {
                // Log the error (implement proper logging)
                System.Diagnostics.Debug.WriteLine($"Logout error: {ex.Message}");

                // Ensure user is redirected to login even if there's an error
                Response.Redirect("~/Login.aspx", true);
            }
        }

        protected void lnkChangePassword_Click(object sender, EventArgs e)
        {
            // Redirect to change password page
            Response.Redirect("~/ChangePassword.aspx");
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Ensure the current page title is set
            if (Page.Header != null)
            {
                string pageTitle = Page.Title;
                if (string.IsNullOrEmpty(pageTitle))
                {
                    Page.Title = "Leave Management Portal";
                }
                else
                {
                    Page.Title = $"{pageTitle} - Leave Management Portal";
                }
            }
        }

        protected string GetUserDisplayName()
        {
            string userName = Session["UserName"]?.ToString();
            return string.IsNullOrEmpty(userName) ? "User" : userName;
        }

        protected string GetCurrentPageName()
        {
            string path = Request.Url.AbsolutePath;
            string pageName = System.IO.Path.GetFileNameWithoutExtension(path);
            return char.ToUpper(pageName[0]) + pageName.Substring(1);
        }
    }
}