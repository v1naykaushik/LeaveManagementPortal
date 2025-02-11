<%-- Employee Dashboard--%>

<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/LeaveManagementPortalMaster.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="LeaveManagementPortal.Dashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .dashboard-card {
            background: white;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            padding: 20px;
            margin-bottom: 20px;
            margin-top: 100px;
        }
        .page-title {
            color: #1a237e;
            font-size: 2rem;
            font-weight: 600;
            margin-bottom: 2rem;
        }
        .card-title {
            color: #1a237e;
            font-size: 1.2rem;
            margin-bottom: 15px;
            font-weight: 600;
        }
        .leave-status {
            padding: 4px 8px;
            border-radius: 4px;
            font-size: 0.875rem;
            font-weight: 500;
        }
        .status-pending {
            background-color: #fff3cd;
            color: #856404;
        }
        .status-approved {
            background-color: #d4edda;
            color: #155724;
        }
        .status-rejected {
            background-color: #f8d7da;
            color: #721c24;
        }
        .leave-item {
            padding: 12px;
            border-bottom: 1px solid #eee;
        }
        .leave-item:last-child {
            border-bottom: none;
        }
        .leave-date {
            color: #666;
            font-size: 0.9rem;
        }
        .no-leaves {
            color: #666;
            font-style: italic;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <h2 class="page-title">My Dashboard</h2>
        
        <div class="row">
            <div class="col-md-6">
                <div class="dashboard-card">
                    <h3 class="card-title">Recent Leave Applications</h3>
                    <asp:Repeater ID="rptRecentLeaves" runat="server">
                        <ItemTemplate>
                            <div class="leave-item">
                                <div class="d-flex justify-content-between align-items-center mb-2">
                                    <strong>
                                        <%# Eval("LeaveTypeName") %> 
                                        (<%# FormatDuration(Eval("Duration")) %>)
                                    </strong>
                                    <span class="leave-status status-<%# Eval("Status").ToString().ToLower() %>">
                                        <%# Eval("Status") %>
                                    </span>
                                </div>
                                <div class="leave-date">
                                    <%# Convert.ToDateTime(Eval("StartDate")).ToString("MMM dd, yyyy") %>
                                    <%# Convert.ToDateTime(Eval("StartDate")).Date != Convert.ToDateTime(Eval("EndDate")).Date 
                                        ? " - " + Convert.ToDateTime(Eval("EndDate")).ToString("MMM dd, yyyy") 
                                        : "" %>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    <asp:Label ID="lblNoLeaves" runat="server" CssClass="no-leaves" 
                        Text="No recent leave applications" Visible="false" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>