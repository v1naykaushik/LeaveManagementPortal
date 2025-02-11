<%@ Page Title="" Language="C#" MasterPageFile="~/LeaveManagementPortalMaster.Master" AutoEventWireup="true" CodeBehind="ApplyLeave.aspx.cs" Inherits="LeaveManagementPortal.ApplyLeave" %>


<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .form-container {
            max-width: 800px;
            margin-top: 150px;
            padding: 2rem;
            background-color: white;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        .page-title {
            color: #1a237e;
            font-size: 2rem;
            font-weight: 600;
            margin-bottom: 2rem;
        }
        .leave-type-balance {
            font-size: 0.9rem;
            color: #666;
            margin-left: 8px;
        }
        .alert {
            padding: 0.75rem 1.25rem;
            margin-bottom: 1rem;
            border: 1px solid transparent;
            border-radius: 0.25rem;
        }
        .alert-warning {
            color: #856404;
            background-color: #fff3cd;
            border-color: #ffeeba;
        }
        .alert-info {
            color: #0c5460;
            background-color: #d1ecf1;
            border-color: #bee5eb;
        }
        .validation-error {
            color: #dc3545;
            font-size: 0.875rem;
            margin-top: 0.25rem;
        }
        .calendar-info {
            font-size: 0.875rem;
            color: #666;
            margin-top: 0.25rem;
        }
        .date-field {
            cursor: pointer;
            background-color: #fff;
        }
    
        .date-field::-webkit-calendar-picker-indicator {
            position: absolute;
            right: 10px;
            top: 50%;
            transform: translateY(-50%);
            cursor: pointer;
        }

        .date-field-container {
            position: relative;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <div class="form-container">
            <h2 class="page-title">Apply Leave</h2>

            <%-- Manager Warning Alert --%>
            <asp:Panel ID="pnlNoManager" runat="server" CssClass="alert alert-warning" Visible="false">
                No reporting manager is assigned to you. Your leave application will still be submitted but might face delays in approval.
            </asp:Panel>

            <%-- Medical Leave Info Alert --%>
            <asp:Panel ID="pnlMedicalLeaveInfo" runat="server" CssClass="alert alert-info" Visible="false">
                Note: Medical Leave will be considered as half day LOP.
            </asp:Panel>

            <%-- Leave Type Selection --%>
            <div class="mb-3">
                <label for="ddlLeaveType" class="form-label">Leave Type</label>
                <asp:DropDownList ID="ddlLeaveType" runat="server" CssClass="form-select" AutoPostBack="true" 
                    OnSelectedIndexChanged="ddlLeaveType_SelectedIndexChanged">
                </asp:DropDownList>
                <asp:RequiredFieldValidator ID="rfvLeaveType" runat="server" 
                    ControlToValidate="ddlLeaveType"
                    InitialValue=""
                    CssClass="validation-error"
                    Display="Dynamic"
                    ErrorMessage="Please select a leave type." />
            </div>

            <%-- Date Selection --%>
            <div class="row mb-3">
                <div class="col-md-6">
                    <label for="txtStartDate" class="form-label">Start Date</label>
                    <div class="date-field-container">
                        <asp:TextBox ID="txtStartDate" runat="server" 
                            CssClass="form-control date-field" 
                            TextMode="Date" 
                            AutoPostBack="true" 
                            onclick="this.showPicker()"
                            OnTextChanged="txtStartDate_TextChanged" />
                        <asp:RequiredFieldValidator ID="rfvStartDate" runat="server" 
                            ControlToValidate="txtStartDate"
                            CssClass="validation-error"
                            Display="Dynamic"
                            ErrorMessage="Start date is required." />
                        <asp:CustomValidator ID="cvStartDate" runat="server" 
                            ControlToValidate="txtStartDate"
                            CssClass="validation-error"
                            Display="Dynamic"
                            OnServerValidate="cvStartDate_ServerValidate"
                            ErrorMessage="Start date cannot be in the past." />
                    </div>
                </div>
                <div class="col-md-6">
                    <label for="txtEndDate" class="form-label">End Date</label>
                    <div class="date-field-container">
                        <asp:TextBox ID="txtEndDate" runat="server" 
                            CssClass="form-control date-field" 
                            TextMode="Date" 
                            AutoPostBack="true"
                            onclick="this.showPicker()"
                            OnTextChanged="txtEndDate_TextChanged" />
                        <asp:CustomValidator ID="cvEndDate" runat="server" 
                            ControlToValidate="txtEndDate"
                            CssClass="validation-error"
                            Display="Dynamic"
                            OnServerValidate="cvEndDate_ServerValidate"
                            ErrorMessage="End date must be after Start date and in the same year as Start date." />
                    </div>
                </div>
            </div>

            <%-- Half Day Option --%>
            <div class="mb-4">
                <asp:CheckBox ID="chkHalfDay" runat="server" Text="  Half Day Leave" 
                    AutoPostBack="true" OnCheckedChanged="chkHalfDay_CheckedChanged" />
            </div>

            <%-- Restricted Leave Calendar Link --%>
            <asp:Panel ID="pnlRestrictedLeave" runat="server" CssClass="mb-4" Visible="false">
                <asp:HyperLink ID="lnkViewHolidays" runat="server" NavigateUrl="~/ViewRestrictedHolidays.aspx" 
                    Target="_blank" CssClass="btn btn-outline-primary btn-sm">
                    View Restricted Holidays Calendar
                </asp:HyperLink>
            </asp:Panel>

            <%-- Leave Reason --%>
            <div class="mb-4">
                <label for="txtReason" class="form-label">Reason</label>
                <asp:TextBox ID="txtReason" runat="server" CssClass="form-control" 
                    TextMode="MultiLine" Rows="3" MaxLength="150" />
                <div class="calendar-info">
                    Maximum 150 characters allowed
                </div>
            </div>

            <%-- Submit Button --%>
            <div class="d-grid">
                <asp:Button ID="btnApplyLeave" runat="server" Text="Apply Leave" 
                    CssClass="btn btn-primary"
                    OnClick="btnApplyLeave_Click" />
            </div>

            <%-- Error Messages --%>
            <asp:Label ID="lblError" runat="server" CssClass="validation-error mt-3 d-block" />
        </div>
    </div>
</asp:Content>