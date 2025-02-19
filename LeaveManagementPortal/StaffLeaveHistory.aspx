<%@ Page Title="Staff Leave History" Language="C#" MasterPageFile="~/LeaveManagementPortalMaster.Master" AutoEventWireup="true" CodeBehind="StaffLeaveHistory.aspx.cs" Inherits="LeaveManagementPortal.StaffLeaveHistory" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .page-title {
            color: #1a237e;
            font-size: 2rem;
            font-weight: 600;
            margin-top: 150px;
            margin-bottom: 2rem;
        }

        .filter-section {
            background: white;
            padding: 1.5rem;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            margin-bottom: 2rem;
        }

        .table-container {
            background: white;
            padding: 1.5rem;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            overflow-x: auto;
            margin-bottom: 50px;
        }

        .leave-table {
            width: 100%;
            border-collapse: collapse;
        }

        .leave-table th {
            background-color: #1a237e;
            color: white;
            padding: 12px;
            text-align: left;
            font-weight: 500;
        }

        .leave-table td {
            padding: 12px;
            border-bottom: 1px solid #e0e0e0;
        }

        .leave-table tr:hover {
            background-color: #f5f5f5;
        }

        .status-pill {
            padding: 4px 12px;
            border-radius: 12px;
            font-size: 0.875rem;
            font-weight: 500;
        }

        .status-approved {
            background-color: #e8f5e9;
            color: #2e7d32;
        }

        .status-pending {
            background-color: #fff3e0;
            color: #ef6c00;
        }

        .status-rejected {
            background-color: #ffebee;
            color: #c62828;
        }

        .status-cancelled {
            background-color: #f5f5f5;
            color: #616161;
        }

        .export-buttons {
            margin-bottom: 1rem;
        }

        .export-btn {
            padding: 8px 16px;
            margin-right: 10px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            font-weight: 500;
            transition: background-color 0.3s;
        }

        .btn-excel {
            margin-left: 28px;
            background-color: #57a14a;
            color: white;
        }

        .filter-row {
            display: flex;
            gap: 1rem;
            align-items: flex-end;
            flex-wrap: wrap;
        }

        .filter-group {
            flex: 1;
            min-width: 200px;
        }

        .filter-label {
            display: block;
            margin-bottom: 0.5rem;
            color: #1a237e;
            font-weight: 500;
        }

        .filter-control {
            width: 100%;
            padding: 0.5rem;
            border: 1px solid #ddd;
            border-radius: 4px;
        }

        .btn-filter {
            background-color: #1a237e;
            color: white;
            height: 38px;
            padding: 0 1.5rem;
        }

        .btn-clear {
            background-color: #616161;
            color: white;
            height: 38px;
            padding: 0 1.5rem;
        }

        @media (max-width: 768px) {
            .filter-group {
                flex: 100%;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <h2 class="page-title">Staff Leave History</h2>

        <!-- Filters Section -->
        <div class="filter-section">
            <div class="filter-row">
                <div class="filter-group">
                    <label class="filter-label">Employee</label>
                    <asp:DropDownList ID="ddlEmployee" runat="server" CssClass="filter-control">
                    </asp:DropDownList>
                </div>
                <div class="filter-group">
                    <label class="filter-label">Leave Type</label>
                    <asp:DropDownList ID="ddlLeaveType" runat="server" CssClass="filter-control">
                    </asp:DropDownList>
                </div>
                <div class="filter-group">
                    <label class="filter-label">Status</label>
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="filter-control">
                        <asp:ListItem Text="All" Value="" />
                        <asp:ListItem Text="Pending" Value="Pending" />
                        <asp:ListItem Text="Approved" Value="Approved" />
                        <asp:ListItem Text="Rejected" Value="Rejected" />
                        <asp:ListItem Text="Cancelled" Value="Cancelled" />
                    </asp:DropDownList>
                </div>
                <div class="filter-group">
                    <label class="filter-label">Date Range</label>
                    <asp:TextBox ID="txtDateRange" runat="server" CssClass="filter-control" TextMode="Month" />
                </div>
                <div class="filter-group" style="flex: 0 0 auto;">
                    <asp:Button ID="btnFilter" runat="server" Text="Apply Filters" 
                        CssClass="export-btn btn-filter" OnClick="btnFilter_Click" />
                    <asp:Button ID="btnClear" runat="server" Text="Clear" 
                        CssClass="export-btn btn-clear" OnClick="btnClear_Click" />
                </div>
            </div>
        </div>

        <!-- Export Buttons -->
        <div class="export-buttons">
            <asp:Button ID="btnExportExcel" runat="server" Text="Export to Excel" 
                CssClass="export-btn btn-excel" OnClick="btnExportExcel_Click" />
        </div>

        <!-- Leave History Table -->
        <div class="table-container">
            <asp:GridView ID="gvLeaveHistory" runat="server" CssClass="leave-table" 
                AutoGenerateColumns="false" AllowPaging="true" PageSize="10"
                OnPageIndexChanging="gvLeaveHistory_PageIndexChanging">
                <Columns>
                    <asp:BoundField DataField="EmployeeName" HeaderText="Employee" />
                    <asp:BoundField DataField="LeaveTypeName" HeaderText="Leave Type" />
                    <asp:BoundField DataField="StartDate" HeaderText="Start Date" 
                        DataFormatString="{0:dd MMM yyyy}" />
                    <asp:BoundField DataField="EndDate" HeaderText="End Date" 
                        DataFormatString="{0:dd MMM yyyy}" />
                    <asp:BoundField DataField="Duration" HeaderText="Duration" />
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate>
                            <span class="status-pill status-<%# Eval("Status").ToString().ToLower() %>">
                                <%# Eval("Status") %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="ManagerApprovalStatus" HeaderText="Manager Status" />
                    <asp:BoundField DataField="DirectorApprovalStatus" HeaderText="Director Status" />
                    <asp:BoundField DataField="Reason" HeaderText="Reason" />
                </Columns>
                <PagerSettings Mode="NumericFirstLast" FirstPageText="«" LastPageText="»" />
                <PagerStyle CssClass="pagination" />
            </asp:GridView>
        </div>
    </div>

    <script type="text/javascript">
        // Make date range field clickable
        document.addEventListener('DOMContentLoaded', function() {
            var dateField = document.getElementById('<%= txtDateRange.ClientID %>');
            if (dateField) {
                dateField.addEventListener('click', function() {
                    this.showPicker();
                });
            }
        });
    </script>
</asp:Content>

<%-- 'Half Day' : Math.round(duration) + (duration === '1.0' ? ' day' : ' days' --%>