<%@ Page Title="Leave History" Language="C#" MasterPageFile="~/LeaveManagementPortalMaster.Master" AutoEventWireup="true" CodeBehind="EmployeeLeaveHistory.aspx.cs" Inherits="LeaveManagementPortal.EmployeeLeaveHistory" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .page-title {
            color: #1a237e;
            font-size: 2rem;
            font-weight: 600;
            margin-top: 150px;
            margin-bottom: 2rem;
        }

        .filters-section {
            background: white;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            margin-bottom: 20px;
        }

        .leave-tile {
            background: white;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            padding: 20px;
            margin-bottom: 20px;
            transition: transform 0.2s;
            position: relative;
            overflow: hidden;
        }

        .leave-tile:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 8px rgba(0,0,0,0.15);
        }

        .leave-status-strip {
            position: absolute;
            top: 0;
            left: 0;
            width: 4px;
            height: 100%;
        }

        .status-approved { background-color: #4caf50; }
        .status-pending { background-color: #ff9800; }
        .status-rejected { background-color: #f44336; }
        .status-cancelled { background-color: #9e9e9e; }

        .leave-type {
            font-size: 1.1rem;
            font-weight: 600;
            color: #1a237e;
            margin-bottom: 10px;
        }

        .leave-dates {
            color: #666;
            margin-bottom: 8px;
        }

        .leave-duration {
            background-color: #e8f0fe;
            color: #1a237e;
            padding: 4px 8px;
            border-radius: 4px;
            display: inline-block;
            font-size: 0.9rem;
            margin-bottom: 8px;
        }

        .leave-status {
            padding: 4px 8px;
            border-radius: 4px;
            font-size: 0.9rem;
            font-weight: 500;
        }

        .status-label-approved {
            background-color: #e8f5e9;
            color: #2e7d32;
        }

        .status-label-pending {
            background-color: #fff3e0;
            color: #ef6c00;
        }

        .status-label-rejected {
            background-color: #ffebee;
            color: #c62828;
        }

        .status-label-cancelled {
            background-color: #f5f5f5;
            color: #616161;
        }

        .leave-reason {
            color: #666;
            font-size: 0.9rem;
            margin-top: 10px;
            padding-top: 10px;
            border-top: 1px solid #eee;
        }

        .no-leaves {
            text-align: center;
            padding: 40px;
            background: white;
            border-radius: 8px;
            color: #666;
        }

        .approval-info {
            font-size: 0.85rem;
            color: #666;
            margin-top: 8px;
        }

        .cancel-button {
            position: absolute;
            top: 20px;
            right: 20px;
        }

        .btn-cancel {
            background-color: #f44336;
            color: white;
            border: none;
            padding: 4px 12px;
            border-radius: 4px;
            font-size: 0.9rem;
            cursor: pointer;
            transition: background-color 0.3s;
        }

        .btn-cancel:hover {
            background-color: #d32f2f;
        }

        .btn-cancel:disabled {
            background-color: #ccc;
            cursor: not-allowed;
        }

        /* Filter Styles */
        .filter-group {
            margin-bottom: 15px;
        }

        .filter-label {
            font-weight: 500;
            color: #1a237e;
            margin-bottom: 5px;
        }

        .filter-control {
            width: 100%;
            padding: 8px;
            border: 1px solid #ddd;
            border-radius: 4px;
            color: #333;
        }

        .filter-control:focus {
            border-color: #1a237e;
            outline: none;
            box-shadow: 0 0 0 2px rgba(26,35,126,0.1);
        }

        .apply-filters {
            background-color: #1a237e;
            color: white;
            border: none;
            padding: 8px 20px;
            border-radius: 4px;
            cursor: pointer;
            transition: background-color 0.3s;
        }

        .apply-filters:hover {
            background-color: #151c5e;
        }

        .clear-filters {
            background-color: #f5f5f5;
            color: #333;
            border: 1px solid #ddd;
            padding: 8px 20px;
            border-radius: 4px;
            cursor: pointer;
            transition: background-color 0.3s;
        }

        .clear-filters:hover {
            background-color: #e0e0e0;
        }
        .container-fluid {
            margin-bottom: 100px; /* Provides extra space for fixed footer */
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager runat="server"></asp:ScriptManager>
    <div class="container-fluid pb-5">
        <h2 class="page-title">Leave History</h2>

        <!-- Filters Section -->
        <div class="filters-section">
            <div class="row">
                <div class="col-md-3">
                    <div class="filter-group">
                        <label class="filter-label">Leave Status</label>
                        <asp:DropDownList ID="ddlStatus" runat="server" CssClass="filter-control">
                            <asp:ListItem Text="All Status" Value="All" />
                            <asp:ListItem Text="Pending" Value="Pending" />
                            <asp:ListItem Text="Approved" Value="Approved" />
                            <asp:ListItem Text="Rejected" Value="Rejected" />
                            <asp:ListItem Text="Cancelled" Value="Cancelled" />
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="filter-group">
                        <label class="filter-label">Month</label>
                        <asp:DropDownList ID="ddlMonth" runat="server" CssClass="filter-control">
                            <asp:ListItem Text="All Months" Value="" />
                            <asp:ListItem Text="January" Value="1" />
                            <asp:ListItem Text="February" Value="2" />
                            <asp:ListItem Text="March" Value="3" />
                            <asp:ListItem Text="April" Value="4" />
                            <asp:ListItem Text="May" Value="5" />
                            <asp:ListItem Text="June" Value="6" />
                            <asp:ListItem Text="July" Value="7" />
                            <asp:ListItem Text="August" Value="8" />
                            <asp:ListItem Text="September" Value="9" />
                            <asp:ListItem Text="October" Value="10" />
                            <asp:ListItem Text="November" Value="11" />
                            <asp:ListItem Text="December" Value="12" />
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="filter-group">
                        <label class="filter-label">Year</label>
                        <asp:DropDownList ID="ddlYear" runat="server" CssClass="filter-control">
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="filter-group">
                        <label class="filter-label">&nbsp;</label>
                        <div>
                            <asp:Button ID="btnApplyFilters" runat="server" Text="Apply Filters" 
                                CssClass="apply-filters" OnClick="btnApplyFilters_Click" />
                            <asp:Button ID="btnClearFilters" runat="server" Text="Clear" 
                                CssClass="clear-filters" OnClick="btnClearFilters_Click" />
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Leave History -->
        <asp:UpdatePanel ID="upLeaveHistory" runat="server">
            <ContentTemplate>
                <div class="row">
                    <asp:Repeater ID="rptLeaveHistory" runat="server" OnItemCommand="rptLeaveHistory_ItemCommand">
                        <ItemTemplate>
                            <div class="col-md-6">
                                <div class="leave-tile">
                                    <!-- Status Strip -->
                                    <div class="leave-status-strip status-<%# Eval("Status").ToString().ToLower() %>"></div>

                                    <!-- Leave Type -->
                                    <div class="leave-type">
                                        <%# Eval("LeaveTypeName") %>
                                        <span class="leave-status status-label-<%# Eval("Status").ToString().ToLower() %>">
                                            <%# Eval("Status") %>
                                        </span>
                                    </div>

                                    <!-- Leave Duration -->
                                    <div class="leave-duration">
                                        <%# FormatDuration(Eval("Duration"), Eval("IsHalfDay")) %>
                                    </div>

                                    <!-- Leave Dates -->
                                    <div class="leave-dates">
                                        <%# FormatDateRange(Eval("StartDate"), Eval("EndDate")) %>
                                    </div>

                                    <!-- Approval Info -->
                                    <div class="approval-info">
                                        Manager: <%# Eval("ManagerApprovalStatus") %>
                                        <%# Eval("ManagerApprovalDate") != DBNull.Value ? 
                                            " (" + Convert.ToDateTime(Eval("ManagerApprovalDate")).ToString("dd MMM yyyy") + ")" : "" %>
                                        <br />
                                        Director: <%# Eval("DirectorApprovalStatus") %>
                                        <%# Eval("DirectorApprovalDate") != DBNull.Value ? 
                                            " (" + Convert.ToDateTime(Eval("DirectorApprovalDate")).ToString("dd MMM yyyy") + ")" : "" %>
                                    </div>

                                    <!-- Leave Reason -->
                                    <%--<%# !string.IsNullOrEmpty(Eval("Reason").ToString()) ? 
                                        "<div class='leave-reason'>Reason: " + Eval("Reason") + "</div>" : "" %> --%>

                                    <!-- Cancel Button -->
                                    <asp:Panel ID="pnlCancelButton" runat="server" CssClass="cancel-button" 
                                        Visible='<%# CanCancelLeave(Eval("Status").ToString(), Eval("StartDate")) %>'>
                                        <asp:Button ID="btnCancel" runat="server" Text="Cancel Leave" 
                                            CssClass="btn-cancel"
                                            CommandName="CancelLeave" 
                                            CommandArgument='<%# Eval("LeaveID") %>'
                                            OnClientClick="return confirm('Are you sure you want to cancel this leave?');" />
                                    </asp:Panel>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>

                    <!-- No Leaves Message -->
                    <asp:Panel ID="pnlNoLeaves" runat="server" CssClass="col-12" Visible="false">
                        <div class="no-leaves">
                            <i class="fas fa-calendar-times mb-3" style="font-size: 48px; color: #ccc;"></i>
                            <h4>No leave records found</h4>
                            <p class="text-muted">Adjust your filters or clear them to see more results</p>
                        </div>
                    </asp:Panel>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>