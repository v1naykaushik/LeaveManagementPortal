<%@ Page Title="Add New Employee" Language="C#" MasterPageFile="~/LeaveManagementPortalMaster.Master" AutoEventWireup="true" CodeBehind="AddEmployee.aspx.cs" Inherits="LeaveManagementPortal.AddEmployee" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .form-container {
            max-width: 800px;
            margin-top: 150px;
            padding: 2.5rem;
            background-color: white;
            border-radius: 12px;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }

        .page-title {
            color: #1a237e;
            font-size: 2rem;
            font-weight: 600;
            margin-bottom: 2rem;
        }

        .form-section {
            margin-bottom: 2rem;
        }

        .form-label {
            font-weight: 500;
            color: #2c3e50;
            margin-bottom: 0.5rem;
        }

        .form-control {
            border: 1px solid #e2e8f0;
            border-radius: 6px;
            padding: 0.75rem;
            transition: all 0.3s ease;
        }

        .form-control:focus {
            border-color: #1a237e;
            box-shadow: 0 0 0 2px rgba(26, 35, 126, 0.2);
        }

        .form-select {
            border: 1px solid #e2e8f0;
            border-radius: 6px;
            padding: 0.75rem;
            transition: all 0.3s ease;
        }

        .form-select:focus {
            border-color: #1a237e;
            box-shadow: 0 0 0 2px rgba(26, 35, 126, 0.2);
        }

        .validation-error {
            color: #dc3545;
            font-size: 0.875rem;
            margin-top: 0.5rem;
            display: block;
        }

        .success-message {
            background-color: #d4edda;
            color: #155724;
            padding: 1rem;
            border-radius: 8px;
            margin-bottom: 2rem;
            border: 1px solid #c3e6cb;
        }

        .success-message .password-display {
            background-color: #ffffff;
            padding: 0.5rem 1rem;
            border-radius: 4px;
            margin-top: 0.5rem;
            font-family: monospace;
            border: 1px solid #c3e6cb;
        }

        .btn-submit {
            background-color: #1a237e;
            color: white;
            padding: 1rem 2rem;
            border-radius: 6px;
            border: none;
            font-weight: 500;
            width: 100%;
            transition: all 0.3s ease;
        }

        .btn-submit:hover {
            background-color: #151c5e;
            transform: translateY(-1px);
        }

        .btn-submit:active {
            transform: translateY(0);
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <div class="form-container">
            <h2 class="page-title">Add New Employee</h2>

            <!-- Success Message Panel -->
            <asp:Panel ID="pnlSuccess" runat="server" CssClass="success-message" Visible="false">
                <div><i class="fas fa-check-circle me-2"></i>Employee added successfully! The account details have been set up.</div>
                <div class="password-display">
                    Generated Password: <asp:Label ID="lblGeneratedPassword" runat="server" Font-Bold="true"></asp:Label>
                </div>
            </asp:Panel>

            <!-- Employee Details Form -->
            <div class="form-section">
                <div class="mb-4">
                    <label for="txtName" class="form-label">Full Name</label>
                    <asp:TextBox ID="txtName" runat="server" CssClass="form-control" placeholder="Enter employee's full name" />
                    <asp:RequiredFieldValidator ID="rfvName" runat="server" 
                        ControlToValidate="txtName"
                        CssClass="validation-error"
                        Display="Dynamic"
                        ErrorMessage="Name is required." />
                </div>

                <div class="mb-4">
                    <label for="txtEmail" class="form-label">Email Address</label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" placeholder="Enter employee's email address" />
                    <asp:RequiredFieldValidator ID="rfvEmail" runat="server" 
                        ControlToValidate="txtEmail"
                        CssClass="validation-error"
                        Display="Dynamic"
                        ErrorMessage="Email is required." />
                    <asp:RegularExpressionValidator ID="revEmail" runat="server" 
                        ControlToValidate="txtEmail"
                        CssClass="validation-error"
                        Display="Dynamic"
                        ValidationExpression="^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"
                        ErrorMessage="Please enter a valid email address." />
                </div>

                <div class="mb-4">
                    <label for="txtEmployeeId" class="form-label">Employee Office ID</label>
                    <asp:TextBox ID="txtEmployeeId" runat="server" CssClass="form-control" placeholder="Enter employee's office ID" />
                    <asp:RequiredFieldValidator ID="rfvEmployeeId" runat="server" 
                        ControlToValidate="txtEmployeeId"
                        CssClass="validation-error"
                        Display="Dynamic"
                        ErrorMessage="Employee ID is required." />
                </div>

                <div class="mb-4">
                    <label for="ddlManager" class="form-label">Reporting Manager</label>
                    <asp:DropDownList ID="ddlManager" runat="server" CssClass="form-select">
                    </asp:DropDownList>
                    <asp:RequiredFieldValidator ID="rfvManager" runat="server" 
                        ControlToValidate="ddlManager"
                        CssClass="validation-error"
                        Display="Dynamic"
                        ErrorMessage="Please select a manager." 
                        InitialValue="" />
                </div>
            </div>

            <!-- Submit Button -->
            <div class="form-section mb-3">
                <asp:Button ID="btnAddEmployee" runat="server" 
                    Text="Add Employee" 
                    CssClass="btn-submit"
                    OnClick="btnAddEmployee_Click" />
            </div>

            <!-- Error Message -->
            <asp:Label ID="lblError" runat="server" CssClass="validation-error text-center d-block" />
        </div>
    </div>
</asp:Content>