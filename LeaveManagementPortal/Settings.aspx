<%@ Page Title="Settings" Language="C#" MasterPageFile="~/LeaveManagementPortalMaster.Master" AutoEventWireup="true" CodeBehind="Settings.aspx.cs" Inherits="LeaveManagementPortal.Settings" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .settings-container {
            max-width: 1200px;
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

        .settings-section {
            margin-bottom: 2rem;
        }

        .section-title {
            color: #1a237e;
            font-size: 1.25rem;
            font-weight: 500;
            margin-bottom: 1rem;
            padding-bottom: 0.5rem;
            border-bottom: 2px solid #1a237e;
        }

        .json-input {
            font-family: monospace;
            min-height: 200px;
        }

        .json-format-example {
            background-color: #f8f9fa;
            padding: 1rem;
            border-radius: 4px;
            margin-bottom: 1rem;
            font-family: monospace;
            font-size: 0.875rem;
        }

        .validation-error {
            color: #dc3545;
            margin-top: 0.5rem;
        }

        .accordion-button:not(.collapsed) {
            background-color: #e8f0fe;
            color: #1a237e;
        }

        .holiday-item {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 0.75rem;
            border-bottom: 1px solid #e9ecef;
        }

        .holiday-item:last-child {
            border-bottom: none;
        }

        .holiday-date {
            font-weight: 500;
            color: #1a237e;
        }

        .holiday-name {
            flex-grow: 1;
            margin: 0 1rem;
        }

        .delete-btn {
            color: #dc3545;
            background: none;
            border: none;
            cursor: pointer;
            padding: 0.25rem 0.5rem;
            transition: color 0.2s;
        }

        .delete-btn:hover {
            color: #bd2130;
        }

        .holiday-type {
            font-size: 0.75rem;
            padding: 2px 8px;
            border-radius: 12px;
            margin-left: 8px;
        }

        .holiday-type.restricted {
            background-color: #e8f0fe;
            color: #1a237e;
        }

        .holiday-type.gazetted {
            background-color: #fef3c7;
            color: #92400e;
        }

        /* Style for radio button list */
        #rblHolidayType {
            display: flex;
            gap: 1.5rem;
        }

        #rblHolidayType input[type="radio"] {
            margin-right: 0.5rem;
        }

        .add-holiday {
            background-color: #ffffff;
            padding: 1rem;
            border: 1px solid #e9ecef;
            border-radius: 4px;
            margin-bottom: 1rem;
        }

        .success-message {
            color: #198754;
            background-color: #d1e7dd;
            padding: 1rem;
            border-radius: 4px;
            margin-bottom: 1rem;
            display: none;
        }

        .year-header {
            background-color: #f8f9fa;
            padding: 1rem;
            margin-bottom: 1rem;
            border-radius: 4px;
            font-weight: 500;
        }

        .radio-inline {
            display: flex;
            gap: 1.5rem;
            margin-top: 0.25rem;
        }

        .radio-inline input[type="radio"] {
            margin-right: 0.5rem;
            vertical-align: middle;
            position: relative;
            top: -1px;
        }

        .radio-inline label {
            font-weight: normal;
            color: #495057;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager runat="server"></asp:ScriptManager>
    <div class="container">
        <div class="settings-container">
            <h2 class="page-title">Settings</h2>

            <!-- Success Message -->
            <div id="successMessage" class="success-message">
                Changes saved successfully!
            </div>

            <!-- Holiday Management Section -->
            <div class="settings-section">
                <h3 class="section-title">Restricted Holiday Calendar</h3>

                <!-- Add New Holiday -->
                <div class="add-holiday">
                    <div class="row">
                        <div class="col-md-4">
                            <div class="mb-3">
                                <label for="txtHolidayDate" class="form-label">Date</label>
                                <asp:TextBox ID="txtHolidayDate" runat="server" CssClass="form-control" 
                                    TextMode="Date" />
                                <asp:RequiredFieldValidator ID="rfvDate" runat="server" 
                                    ControlToValidate="txtHolidayDate"
                                    ValidationGroup="AddHoliday"
                                    CssClass="validation-error"
                                    Display="Dynamic"
                                    ErrorMessage="Date is required." />
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="mb-3">
                                <label for="txtHolidayName" class="form-label">Holiday Name</label>
                                <asp:TextBox ID="txtHolidayName" runat="server" CssClass="form-control" />
                                <asp:RequiredFieldValidator ID="rfvName" runat="server" 
                                    ControlToValidate="txtHolidayName"
                                    ValidationGroup="AddHoliday"
                                    CssClass="validation-error"
                                    Display="Dynamic"
                                    ErrorMessage="Holiday name is required." />
                            </div>
                            <div class="mb-3">
                                <label class="form-label d-block">Holiday Type</label>
                                <asp:RadioButtonList ID="rblHolidayType" runat="server" RepeatDirection="Horizontal" CssClass="radio-inline">
                                    <asp:ListItem Text="Restricted" Value="True" Selected="True" />
                                    <asp:ListItem Text="Gazetted" Value="False" />
                                </asp:RadioButtonList>
                            </div>
                        </div>
                        <div class="col-md-2">
                            <div class="mb-3">
                                <label class="form-label">&nbsp;</label>
                                <asp:Button ID="btnAddHoliday" runat="server" Text="Add Holiday" 
                                    CssClass="btn btn-primary w-100"
                                    ValidationGroup="AddHoliday"
                                    OnClick="btnAddHoliday_Click" />
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Holiday List -->
                <asp:UpdatePanel ID="upHolidays" runat="server">
                    <ContentTemplate>
                        <div class="accordion" id="holidayAccordion">
                            <asp:Repeater ID="rptYears" runat="server" OnItemDataBound="rptYears_ItemDataBound">
                                <ItemTemplate>
                                    <div class="accordion-item">
                                        <h2 class="accordion-header">
                                            <button class="accordion-button <%# Container.ItemIndex == 0 ? "" : "collapsed" %>" 
                                                    type="button" data-bs-toggle="collapse" 
                                                    data-bs-target="#collapse<%# Container.ItemIndex %>">
                                                <%# Eval("Year") %> Holidays
                                            </button>
                                        </h2>
                                        <div id="collapse<%# Container.ItemIndex %>" 
                                             class="accordion-collapse collapse <%# Container.ItemIndex == 0 ? "show" : "" %>">
                                            <div class="accordion-body">
                                                <asp:Repeater ID="rptHolidays" runat="server" OnItemCommand="rptHolidays_ItemCommand">
                                                    <HeaderTemplate>
                                                        <div class="holidays-list">
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <div class="holiday-item">
                                                            <span class="holiday-date">
                                                                <%# Convert.ToDateTime(Eval("HolidayDate")).ToString("dd MMM yyyy") %>
                                                            </span>
                                                            <span class="holiday-name">
                                                <%# Eval("HolidayName") %>
                                                <span class='holiday-type <%# Convert.ToBoolean(Eval("isRestricted")) ? "restricted" : "gazetted" %>'>
                                                    <%# Convert.ToBoolean(Eval("isRestricted")) ? "Restricted" : "Gazetted" %>
                                                </span>
                                            </span>
                                                            <asp:LinkButton ID="btnDelete" runat="server" 
                                                                CssClass="delete-btn"
                                                                CommandName="Delete" 
                                                                CommandArgument='<%# Eval("HolidayID") %>'
                                                                OnClientClick="return confirm('Are you sure you want to delete this holiday?');">
                                                                <i class="fas fa-trash"></i> Delete
                                                            </asp:LinkButton>
                                                        </div>
                                                    </ItemTemplate>
                                                    <%--<FooterTemplate>
                                                        </div>
                                                        <asp:Panel ID="pnlNoHolidays" runat="server" CssClass="text-muted text-center py-3">
                                                            No holidays added for this year
                                                        </asp:Panel>
                                                    </FooterTemplate>--%>
                                                </asp:Repeater>
                                            </div>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        function showSuccessMessage() {
            var message = document.getElementById('successMessage');
            message.style.display = 'block';
            setTimeout(function () {
                message.style.display = 'none';
            }, 3000);
        }
    </script>
</asp:Content>