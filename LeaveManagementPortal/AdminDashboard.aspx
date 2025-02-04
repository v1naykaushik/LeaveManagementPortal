<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/LeaveManagementPortalMaster.Master" AutoEventWireup="true" CodeBehind="AdminDashboard.aspx.cs" Inherits="LeaveManagementPortal.AdminDashboard" %>


<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .dashboard-card {
            background: white;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            padding: 20px;
            margin-bottom: 20px;
        }
        .card-title {
            color: #1a237e;
            font-size: 1.2rem;
            margin-bottom: 15px;
            font-weight: 600;
        }
        .employee-list {
            list-style: none;
            padding: 0;
        }
        .employee-item {
            padding: 8px 0;
            border-bottom: 1px solid #eee;
        }
        .employee-item:last-child {
            border-bottom: none;
        }
        .no-leaves {
            color: #666;
            font-style: italic;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid">
        <h2 class="mb-4">Dashboard</h2>
        
        <div class="row">
            <!-- Today's Leaves -->
            <div class="col-md-6">
                <div class="dashboard-card">
                    <h3 class="card-title">Employees on Leave Today</h3>
                    <asp:Repeater ID="rptTodayLeaves" runat="server">
                        <HeaderTemplate>
                            <ul class="employee-list">
                        </HeaderTemplate>
                        <ItemTemplate>
                            <li class="employee-item">
                                <%# Eval("Name") %> - <%# Eval("LeaveTypeName") %>
                            </li>
                        </ItemTemplate>
                        <FooterTemplate>
                            </ul>
                        </FooterTemplate>
                    </asp:Repeater>
                    <asp:Label ID="lblNoTodayLeaves" runat="server" CssClass="no-leaves" 
                        Text="No employees on leave today" Visible="false" />
                </div>
            </div>

            <!-- Tomorrow's Leaves -->
            <div class="col-md-6">
                <div class="dashboard-card">
                    <h3 class="card-title">Employees on Leave Tomorrow</h3>
                    <asp:Repeater ID="rptTomorrowLeaves" runat="server">
                        <HeaderTemplate>
                            <ul class="employee-list">
                        </HeaderTemplate>
                        <ItemTemplate>
                            <li class="employee-item">
                                <%# Eval("Name") %> - <%# Eval("LeaveTypeName") %>
                            </li>
                        </ItemTemplate>
                        <FooterTemplate>
                            </ul>
                        </FooterTemplate>
                    </asp:Repeater>
                    <asp:Label ID="lblNoTomorrowLeaves" runat="server" CssClass="no-leaves" 
                        Text="No employees on leave tomorrow" Visible="false" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>