<%@ Page Title="Add New Employee" Language="C#" MasterPageFile="~/LeaveManagementPortalMaster.Master" AutoEventWireup="true" CodeBehind="AddEmployee.aspx.cs" Inherits="LeaveManagementPortal.AddEmployee" %>


<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .form-container {
            max-width: 600px;
            margin: 2rem auto;
            padding: 2rem;
            background-color: white;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        .validation-error {
            color: #dc3545;
            font-size: 0.875rem;
            margin-top: 0.25rem;
        }
        .success-message {
            color: #28a745;
            padding: 1rem;
            margin-bottom: 1rem;
            border-radius: 4px;
            background-color: #d4edda;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <div class="form-container">
            <h2 class="mb-4">Add New Employee</h2

            <asp:Panel ID="pnlSuccess" runat="server" CssClass="success-message" Visible="false">
                Employee added successfully! The account details have been set up.<br />
                Generated Password: <asp:Label ID="lblGeneratedPassword" runat="server" Font-Bold="true"></asp:Label>
            </asp:Panel>

            <div class="mb-3">
                <label for="txtName" class="form-label">Full Name</label>
                <asp:TextBox ID="txtName" runat="server" CssClass="form-control" />
                <asp:RequiredFieldValidator ID="rfvName" runat="server" 
                    ControlToValidate="txtName"
                    CssClass="validation-error"
                    Display="Dynamic"
                    ErrorMessage="Name is required." />
            </div>

            <div class="mb-3">
                <label for="txtEmail" class="form-label">Email Address</label>
                <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" />
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

            <div class="mb-3">
                <label for="txtEmployeeId" class="form-label">Employee Office ID</label>
                <asp:TextBox ID="txtEmployeeId" runat="server" CssClass="form-control" />
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

            <div class="d-grid">
                <asp:Button ID="btnAddEmployee" runat="server" 
                    Text="Add Employee" 
                    CssClass="btn btn-primary"
                    OnClick="btnAddEmployee_Click" />
            </div>

            <asp:Label ID="lblError" runat="server" CssClass="validation-error mt-3 d-block" />
        </div>
    </div>
</asp:Content>