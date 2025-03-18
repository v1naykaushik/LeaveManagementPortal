<%@ Page Title="Leave Balances" Language="C#" MasterPageFile="~/LeaveManagementPortalMaster.Master" AutoEventWireup="true" CodeBehind="LeaveQuota.aspx.cs" Inherits="LeaveManagementPortal.AdminLeaveBalances" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .balances-container {
            max-width: 1200px;
            margin-top: 150px;
            margin-bottom: 100px;
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

        .filters-section {
            margin-bottom: 2rem;
            padding: 1.5rem;
            border: 1px solid #e2e8f0;
            border-radius: 8px;
            background-color: #f8f9fa;
        }

        .filter-row {
            display: flex;
            align-items: center;
            gap: 1rem;
            margin-bottom: 1rem;
        }

        .filter-label {
            width: 100px;
            color: #666;
            font-weight: 500;
        }

        .filter-control {
            flex: 1;
        }

        .btn-filter {
            background-color: #1a237e;
            color: white;
            padding: 0.5rem 1.5rem;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            transition: background-color 0.3s;
        }

        .btn-filter:hover {
            background-color: #151c5e;
        }

        .btn-export {
            background-color: #388e3c;
            color: white;
            padding: 0.5rem 1.5rem;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            transition: background-color 0.3s;
            margin-left: 10px;
        }

        .btn-export:hover {
            background-color: #2e7d32;
        }

        .grid-container {
            margin-top: 2rem;
            overflow-x: auto;
        }

        .leave-grid {
            width: 100%;
            border-collapse: collapse;
        }

        .leave-grid th {
            background-color: #1a237e;
            color: white;
            padding: 0.75rem;
            text-align: left;
        }

        .leave-grid td {
            padding: 0.75rem;
            border-bottom: 1px solid #e2e8f0;
        }

        .leave-grid tr:nth-child(even) {
            background-color: #f8f9fa;
        }

        .leave-grid tr:hover {
            background-color: #e2e8f0;
        }

        .summary-section {
            margin-top: 2rem;
            padding: 1.5rem;
            border: 1px solid #e2e8f0;
            border-radius: 8px;
            background-color: #f8f9fa;
        }

        .summary-title {
            color: #1a237e;
            font-size: 1.25rem;
            font-weight: 500;
            margin-bottom: 1rem;
        }

        .summary-grid {
            width: 100%;
            border-collapse: collapse;
        }

        .summary-grid th {
            background-color: #1a237e;
            color: white;
            padding: 0.5rem;
            text-align: left;
        }

        .summary-grid td {
            padding: 0.5rem;
            border-bottom: 1px solid #e2e8f0;
        }

        .no-data {
            text-align: center;
            padding: 2rem;
            color: #666;
            font-style: italic;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <div class="balances-container">
            <h2 class="page-title">Employee Leave Balances</h2>

            <!-- Filters Section -->
            <div class="filters-section">
                <div class="filter-row">
                    <div class="filter-label">Employee:</div>
                    <div class="filter-control">
                        <asp:DropDownList ID="ddlEmployee" runat="server" CssClass="form-control">
                            <asp:ListItem Value="" Text="-- All Employees --" />
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="filter-row">
                    <div class="filter-label">Leave Type:</div>
                    <div class="filter-control">
                        <asp:DropDownList ID="ddlLeaveType" runat="server" CssClass="form-control">
                            <asp:ListItem Value="" Text="-- All Leave Types --" />
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="filter-row">
                    <asp:Button ID="btnFilter" runat="server" Text="Apply Filters" CssClass="btn-filter" OnClick="btnFilter_Click" />
                    <asp:Button ID="btnExport" runat="server" Text="Export to Excel" CssClass="btn-export" OnClick="btnExport_Click" />
                </div>
            </div>

            <!-- Leave Balances Grid -->
            <%--<div class="grid-container">
                <asp:GridView ID="gvLeaveBalances" runat="server" CssClass="leave-grid" AutoGenerateColumns="false"
                    EmptyDataText="No leave balance records found." AllowPaging="true" PageSize="20"
                    OnPageIndexChanging="gvLeaveBalances_PageIndexChanging">
                    <Columns>
                        <asp:BoundField DataField="EmployeeID" HeaderText="Emp ID" />
                        <asp:BoundField DataField="EmployeeName" HeaderText="Employee Name" />
                        <asp:BoundField DataField="LeaveTypeName" HeaderText="Leave Type" />
                        <asp:BoundField DataField="PresentYearBalance" HeaderText="Current Balance" DataFormatString="{0:F1}" />
                        <%--<asp:BoundField DataField="NewYearBalance" HeaderText="Next Year Balance" DataFormatString="{0:F1}" />--%>
                        <%--<asp:TemplateField HeaderText="Actions">
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkEdit" runat="server" Text="Edit" CommandName="EditBalance" 
                                    CommandArgument='<%# $"{Eval("UserID")},{Eval("LeaveTypeID")}" %>'
                                    OnCommand="lnkEdit_Command" CssClass="btn btn-sm btn-primary" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>--%>

             <div class="grid-container">
                <asp:GridView ID="gvLeaveBalances" runat="server" CssClass="leave-grid" AutoGenerateColumns="false"
                    EmptyDataText="No leave balance records found." AllowPaging="true" PageSize="20"
                    OnPageIndexChanging="gvLeaveBalances_PageIndexChanging">
                    <Columns>
                        <asp:BoundField DataField="EmployeeOfficeID" HeaderText="Emp ID" />
                        <asp:BoundField DataField="EmployeeName" HeaderText="Employee Name" />
                        <asp:BoundField DataField="CL" HeaderText="CL" DataFormatString="{0:F1}" />
                        <asp:BoundField DataField="EL" HeaderText="EL" DataFormatString="{0:F1}" />
                        <asp:BoundField DataField="ML" HeaderText="ML" DataFormatString="{0:F1}" />
                        <asp:BoundField DataField="RL" HeaderText="RL" DataFormatString="{0:F1}" />
                    </Columns>
                </asp:GridView>
            </div>

            <%--<!-- Leave Summary Section -->
            <div class="summary-section">
                <h3 class="summary-title">Leave Types Summary</h3>
                <asp:GridView ID="gvLeaveTypeSummary" runat="server" CssClass="summary-grid" AutoGenerateColumns="false"
                    EmptyDataText="No leave type data available.">
                    <Columns>
                        <asp:BoundField DataField="LeaveTypeID" HeaderText="ID" />
                        <asp:BoundField DataField="LeaveTypeName" HeaderText="Leave Type" />
                        <asp:BoundField DataField="LeaveDescription" HeaderText="Description" />
                        <asp:BoundField DataField="InitialAllocation" HeaderText="Default Allocation" DataFormatString="{0:F1}" />
                        <asp:BoundField DataField="LapsesAtYearEnd" HeaderText="Lapses" />
                    </Columns>
                </asp:GridView>
            </div>--%>
        </div>
    </div>

    <!-- Modal for Editing Leave Balance -->
    <%--<div class="modal fade" id="editModal" tabindex="-1" aria-labelledby="editModalLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="editModalLabel">Edit Leave Balance</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <asp:HiddenField ID="hdnUserID" runat="server" />
                    <asp:HiddenField ID="hdnLeaveTypeID" runat="server" />
                    <div class="mb-3">
                        <label for="txtCurrentBalance" class="form-label">Current Year Balance</label>
                        <asp:TextBox ID="txtCurrentBalance" runat="server" CssClass="form-control" TextMode="Number" Step="0.5"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvCurrentBalance" runat="server" 
                            ControlToValidate="txtCurrentBalance" Display="Dynamic"
                            ErrorMessage="Current balance is required."
                            ValidationGroup="EditBalance" CssClass="text-danger" />
                    </div>
                    <div class="mb-3">
                        <label for="txtNextYearBalance" class="form-label">Next Year Balance</label>
                        <asp:TextBox ID="txtNextYearBalance" runat="server" CssClass="form-control" TextMode="Number" Step="0.5"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvNextYearBalance" runat="server" 
                            ControlToValidate="txtNextYearBalance" Display="Dynamic"
                            ErrorMessage="Next year balance is required."
                            ValidationGroup="EditBalance" CssClass="text-danger" />
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                    <asp:Button ID="btnSaveBalance" runat="server" Text="Save Changes" CssClass="btn btn-primary"
                        ValidationGroup="EditBalance" OnClick="btnSaveBalance_Click" />
                </div>
            </div>
        </div>
    </div>--%>

    <!-- Include Bootstrap JS for modal functionality -->
    <%--<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js"></script>
    <script type="text/javascript">
        function showEditModal() {
            var editModal = new bootstrap.Modal(document.getElementById('editModal'));
            editModal.show();
        }
    </script>--%>
</asp:Content>