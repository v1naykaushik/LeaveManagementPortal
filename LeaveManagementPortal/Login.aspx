<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="LeaveManagementPortal.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Leave Management System - Login</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css" />
    
    <style>
        /* Header Styles */
        .main-header {
            background: #BDD7EE;
            padding: 10px 0;
            border-bottom: 1px solid #ccc;
        }

        .header-left-logo {
            height: 80px;
        }

        .header-flags-container {
            text-align: center;
            margin-bottom: 5px;
        }

        .header-flag {
            height: 30px;
            margin: 0 5px;
        }

        .header-org-name {
            font-size: 24px;
            color: navy;
            text-align: center;
        }

        .header-right-logo {
            height: 80px;
            width: 130px;
        }

        /* Login form styles */
        .login-container {
            background: white;
            padding: 2rem;
            border-radius: 10px;
            box-shadow: 0 0 20px rgba(0,0,0,0.1);
            width: 100%;
            max-width: 400px;
            margin: 50px auto;
        }

        .form-control:focus {
            border-color: #667eea;
            box-shadow: 0 0 0 0.25rem rgba(102, 126, 234, 0.25);
        }

        /* Add to the existing style section */
        .password-container {
            position: relative;
        }

        .password-toggle {
            position: absolute;
            right: 10px;
            top: 50%;
            transform: translateY(-50%);
            background: none;
            border: none;
            cursor: pointer;
            color: #666;
        }

        .password-toggle:hover {
            color: #333;
        }

        .input-group .btn-outline-secondary {
            border-color: #ced4da;
            color: #6c757d;
        }

        .input-group .btn-outline-secondary:hover {
            background-color: #f8f9fa;
            color: #495057;
        }

        .input-group .form-control:focus {
            z-index: 1;
        }

        .btn-login {
            background: #667eea;
            border: none;
            padding: 0.75rem;
            font-size: 1rem;
            font-weight: 500;
            width: 100%;
            color: white;
            border-radius: 5px;
            transition: background-color 0.3s;
        }

        .btn-login:hover {
            background: #764ba2;
        }

        .validation-message {
            color: #dc3545;footer
            font-size: 0.875rem;
            margin-top: 0.25rem;
        }

        /* Footer Styles */
        .main-footer {
            background: #1a237e;
            color: white;
            padding: 10px 0;
            position: fixed;
            bottom: 0;
            width: 100%;
        }

        .footer-logo {
            height: 60px;
            margin: 0 10px;
        }

        .footer-text {
            font-size: 14px;
        }

        /* Main content area */
        .main-content {
            min-height: calc(100vh - 180px); /* Adjust based on header/footer height */
            display: flex;
            align-items: center;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        }
    </style>
</head>
<body>
    <form id="formLogin" runat="server">
        <!-- Header -->
        <header class="main-header">
            <div class="container-fluid">
                <div class="row align-items-center">
                    <div class="col-md-2">
                        <img src="Images/dstLogo.png" alt="Organization Logo" class="header-left-logo" />
                    </div>
                    <div class="col-md-8">
                        <div class="header-flags-container">
                            <img src="Images/indiaFlag.png" alt="Flag 1" class="header-flag" />
                            <img src="Images/cefipraLogo.png" alt="Flag 2" class="header-flag" />
                            <img src="Images/franceFlag.png" alt="Flag 3" class="header-flag" />
                        </div>
                        <div class="header-org-name">
                            Indo-French Centre for the Promotion of Advanced Research (IFCPAR/CEFIPRA)
                        </div>
                        <div class="header-org-name">
                            Leave Management System
                        </div>
                    </div>
                    <div class="col-md-2 text-end">
                        <img src="Images/meaeLogo.png" alt="Right Logo" class="header-right-logo" />
                    </div>
                </div>
            </div>
        </header>

        <!-- Main Content -->
        <div class="main-content">
            <div class="login-container">
                <h2 class="text-center mb-4">Sign In</h2>
                
                <!-- Email input group -->
                <div class="mb-3">
                    <label for="txtEmail" class="form-label">Email address</label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" placeholder="Enter your email" />
                    <asp:RequiredFieldValidator ID="rfvEmail" runat="server" 
                        ControlToValidate="txtEmail"
                        Display="Dynamic"
                        CssClass="validation-message"
                        ErrorMessage="Email is required." />
                    <asp:RegularExpressionValidator ID="revEmail" runat="server" 
                        ControlToValidate="txtEmail"
                        Display="Dynamic"
                        CssClass="validation-message"
                        ValidationExpression="^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"
                        ErrorMessage="Please enter a valid email address." />
                </div>

                <!-- Password input group -->
                <div class="mb-4">
                    <label for="txtPassword" class="form-label">Password</label>
                    <div class="input-group">
                        <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" 
                            CssClass="form-control" placeholder="Enter your password" 
                            autocomplete="current-password" />
                        <button class="btn password-toggle-btn" type="button" id="btnTogglePassword">
                            <i class="fa fa-eye" id="toggleIcon"></i>
                        </button>
                    </div>
                    <asp:RequiredFieldValidator ID="rfvPassword" runat="server" 
                        ControlToValidate="txtPassword"
                        Display="Dynamic"
                        CssClass="validation-message"
                        EnableClientScript="true"
                        ValidateEmptyText="true"
                        ErrorMessage="Password is required." />
                </div>

                <!-- Login button -->
                <asp:Button ID="btnLogin" runat="server" Text="Sign In" 
                    CssClass="btn-login" OnClick="btnLogin_Click" />

                <!-- Error message display -->
                <asp:Label ID="lblError" runat="server" CssClass="validation-message d-block text-center mt-3" />
            </div>
        </div>

        <!-- Footer -->
        <footer class="main-footer">
            <div class="container-fluid">
                <div class="row align-items-center">
                    <div class="col-md-4 text-start">
                        <img src="Images/azadiLogo.png" alt="Footer Left Logo" class="footer-logo" />
                    </div>
                    <div class="col-md-4 text-center footer-text">
                        Designed, Developed and Maintained by: CEFIPRA IT Cell<br />
                    </div>
                    <div class="col-md-4 text-end">
                        <img src="Images/digitalIndiaLogo.png" alt="Footer Right Logo" class="footer-logo" />
                    </div>
                </div>
            </div>
        </footer>
    </form>

    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script>
        $(document).ready(function() {
            // Handle browser autofill
            if (navigator.userAgent.indexOf("Chrome") !== -1) {
                setTimeout(function () {
                    var passwordField = $("#<%= txtPassword.ClientID %>");
                    if (passwordField.val()) {
                        passwordField.trigger('change');
                    }
                }, 200);
            }

            // Password toggle functionality
            $("#btnTogglePassword").on("click", function(e) {
                e.preventDefault();
                var passwordField = $("#<%= txtPassword.ClientID %>");
                var toggleIcon = $("#toggleIcon");
            
                if (passwordField.attr("type") === "password") {
                    passwordField.attr("type", "text");
                    toggleIcon.removeClass("fa-eye").addClass("fa-eye-slash");
                } else {
                    passwordField.attr("type", "password");
                    toggleIcon.removeClass("fa-eye-slash").addClass("fa-eye");
                }
            });
        });
    </script>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>