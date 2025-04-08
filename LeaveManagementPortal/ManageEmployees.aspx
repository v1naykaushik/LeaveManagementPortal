<%@ Page Title="Manage Employees" Language="C#" MasterPageFile="~/LeaveManagementPortalMaster.Master" AutoEventWireup="true" CodeBehind="ManageEmployees.aspx.cs" Inherits="LeaveManagementPortal.ManageEmployees" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .employee-grid {
            margin-top: 150px;
            background: white;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            padding: 20px;
        }

        .page-title {
            color: #1a237e;
            font-size: 2rem;
            font-weight: 600;
            margin-bottom: 2rem;
        }

        .grid-table {
            width: 100%;
            border-collapse: separate;
            border-spacing: 0;
            margin-bottom: 50px;
        }

        .grid-header {
            background-color: #f8f9fa;
            font-weight: 600;
            color: #1a237e;
        }

        .grid-header th {
            padding: 12px;
            border-bottom: 2px solid #dee2e6;
            text-align: left;
        }

        .grid-row td {
            padding: 12px;
            border-bottom: 1px solid #dee2e6;
            vertical-align: middle;
        }

        .grid-row:hover {
            background-color: #f8f9fa;
        }

        .status-badge {
            padding: 6px 12px;
            border-radius: 4px;
            font-size: 0.875rem;
            font-weight: 500;
        }

        .status-active {
            background-color: #d4edda;
            color: #155724;
        }

        .status-inactive {
            background-color: #f8d7da;
            color: #721c24;
        }

        .toggle-button {
            padding: 6px 12px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            font-size: 0.875rem;
            transition: background-color 0.3s;
        }

        .btn-activate {
            background-color: #28a745;
            color: white;
        }

        .btn-deactivate {
            background-color: #dc3545;
            color: white;
        }

        .toggle-button:hover {
            opacity: 0.9;
        }

        .employee-id {
            color: #666;
            font-size: 0.9rem;
            padding: 2px 6px;
            background-color: #f5f5f5;
            border-radius: 4px;
        }

        .role-badge {
            background-color: #e8f0fe;
            color: #1a237e;
            padding: 4px 8px;
            border-radius: 4px;
            font-size: 0.875rem;
        }

        .success-message {
            background-color: #d4edda;
            color: #155724;
            padding: 12px;
            border-radius: 4px;
            margin-bottom: 1rem;
            display: none;
        }

        /* Modal styles */
        .modal-backdrop {
            opacity: 0.5;
        }

        .password-modal {
            display: none;
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background-color: rgba(0,0,0,0.5);
            z-index: 1050;
        }

        .modal-dialog {
            position: relative;
            width: auto;
            margin: 1.75rem auto;
            max-width: 500px;
        }

        .modal-content {
            position: relative;
            background-color: #fff;
            border-radius: 0.3rem;
            box-shadow: 0 0.5rem 1rem rgba(0,0,0,0.15);
        }

        .modal-header {
            padding: 1rem;
            border-bottom: 1px solid #dee2e6;
            background-color: #1a237e;
            color: white;
            border-top-left-radius: 0.3rem;
            border-top-right-radius: 0.3rem;
        }

        .modal-body {
            padding: 1rem;
        }

        .modal-footer {
            padding: 1rem;
            border-top: 1px solid #dee2e6;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager runat="server"></asp:ScriptManager>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <div class="container">
                <div class="employee-grid">
                    <h2 class="page-title">Manage Employees</h2>
                    
                    <div id="successMessage" class="success-message">
                        Employee status updated successfully.
                    </div>

                    <asp:GridView ID="gvEmployees" runat="server" AutoGenerateColumns="false" 
                        CssClass="grid-table" OnRowCommand="gvEmployees_RowCommand"
                        DataKeyNames="UserID">
                        <HeaderStyle CssClass="grid-header" />
                        <RowStyle CssClass="grid-row" />
                        <Columns>
                            <asp:BoundField DataField="EmployeeOfficeID" HeaderText="Employee ID" 
                                ItemStyle-CssClass="employee-id" />
                            <asp:BoundField DataField="FullName" HeaderText="Name" />
                            <asp:BoundField DataField="Email" HeaderText="Email" />
                            <asp:TemplateField HeaderText="Designation">
                                <ItemTemplate>
                                    <span class="role-badge"><%# Eval("Designation") %></span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Status">
                                <ItemTemplate>
                                    <span class='status-badge <%# Convert.ToBoolean(Eval("IsActive")) ? "status-active" : "status-inactive" %>'>
                                        <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Action">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnToggle" runat="server" 
                                        CssClass='<%# Convert.ToBoolean(Eval("IsActive")) ? "toggle-button btn-deactivate" : "toggle-button btn-activate" %>'
                                        CommandName="ToggleStatus" 
                                        CommandArgument='<%# Eval("UserID") %>'>
                                        <%# Convert.ToBoolean(Eval("IsActive")) ? "Deactivate" : "Activate" %>
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>

                    <!-- Password Verification Panel -->
                    <asp:Panel ID="pnlPassword" runat="server" CssClass="password-modal" DefaultButton="btnVerifyPassword">
                        <div class="modal-dialog">
                            <div class="modal-content">
                                <div class="modal-header">
                                    <h5 class="modal-title">Verify Director Password</h5>
                                    <asp:Button ID="btnCloseModal" runat="server" Text="×" CssClass="btn-close" OnClick="btnCloseModal_Click" />
                                </div>
                                <div class="modal-body">
                                    <div class="mb-3">
                                        <label for="<%= txtDirectorPassword.ClientID %>" class="form-label">Enter Director Password</label>
                                        <asp:TextBox ID="txtDirectorPassword" runat="server" CssClass="form-control" TextMode="Password" />
                                        <asp:HiddenField ID="hdnSelectedUserId" runat="server" />
                                    </div>
                                    <asp:Label ID="lblPasswordError" runat="server" CssClass="text-danger" Visible="false">
                                        Invalid password. Please try again.
                                    </asp:Label>
                                </div>
                                <div class="modal-footer">
                                    <asp:Button ID="btnCancelPassword" runat="server" Text="Cancel" CssClass="btn btn-secondary" OnClick="btnCloseModal_Click" />
                                    <asp:Button ID="btnVerifyPassword" runat="server" Text="Verify" CssClass="btn btn-primary" OnClick="btnVerifyPassword_Click" />
                                </div>
                            </div>
                        </div>
                    </asp:Panel>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

    <script type="text/javascript">
        function showSuccessMessage() {
            var message = document.getElementById('successMessage');
            message.style.display = 'block';
            setTimeout(function() {
                message.style.display = 'none';
            }, 3000);
        }

        function showPasswordModal() {
            document.getElementById('<%= pnlPassword.ClientID %>').style.display = 'block';
        }

        function hidePasswordModal() {
            document.getElementById('<%= pnlPassword.ClientID %>').style.display = 'none';
            document.getElementById('<%= txtDirectorPassword.ClientID %>').value = '';
        }
    </script>
</asp:Content>