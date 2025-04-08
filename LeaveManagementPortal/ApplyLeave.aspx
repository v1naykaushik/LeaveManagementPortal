<%@ Page Title="" Language="C#" MasterPageFile="~/LeaveManagementPortalMaster.Master" AutoEventWireup="true" CodeBehind="ApplyLeave.aspx.cs" Inherits="LeaveManagementPortal.ApplyLeave" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .form-container {
            max-width: 1200px;
            margin-top: 150px;
            margin-bottom: 100px;
            padding: 2rem;
            background-color: white;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }

        .btn-submit {
            background-color: #1a237e;
            color: white;
            padding: 1rem 2rem;
            border-radius: 6px;
            border: none;
            font-weight: 500;
            transition: all 0.3s ease;
        }

            .btn-submit:hover {
                background-color: #151c5e;
                transform: translateY(-1px);
            }

            .btn-submit:active {
                transform: translateY(0);
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

        .calendar-container {
            margin-bottom: 25px;
            font-family: Arial, sans-serif;
        }

        .calendar-legend {
            display: flex;
            flex-wrap: wrap;
            margin-bottom: 10px;
            gap: 15px;
        }

        .legend-item {
            display: flex;
            align-items: center;
            font-size: 12px;
        }

        .legend-color {
            display: inline-block;
            width: 15px;
            height: 15px;
            margin-right: 5px;
            border: 1px solid #ccc;
        }

        .cl-color {
            background-color: #4CAF50;
        }

        .el-color {
            background-color: #2196F3;
        }

        .ml-color {
            background-color: #FF9800;
        }

        .rl-color {
            background-color: #9C27B0;
        }

        .lop-color {
            background-color: #F44336;
        }

        .pending-color {
            background: repeating-linear-gradient( 45deg, #ccc, #ccc 5px, #eee 5px, #eee 10px );
        }

        .year-calendar {
            display: grid;
            grid-template-columns: repeat(6, 1fr);
            gap: 15px;
        }

        .month-calendar {
            border: 1px solid #ddd;
            border-radius: 5px;
        }

        .month-header {
            background-color: #f0f0f0;
            padding: 5px;
            text-align: center;
            font-weight: bold;
            border-bottom: 1px solid #ddd;
        }

        .month-days {
            display: grid;
            grid-template-columns: repeat(7, 1fr);
        }

        .day-header {
            text-align: center;
            padding: 3px;
            font-size: 11px;
            font-weight: bold;
            border-bottom: 1px solid #eee;
        }

        .calendar-day {
            height: 22px;
            font-size: 11px;
            text-align: center;
            border: 1px solid transparent;
            display: flex;
            justify-content: center;
            align-items: center;
            position: relative;
        }

        .weekend {
            background-color: #f9f9f9;
        }

        .other-month {
            color: #ccc;
        }

        .leave-day {
            cursor: default;
            position: relative;
        }

        .leave-day-tooltip {
            visibility: hidden;
            background-color: black;
            color: white;
            text-align: center;
            padding: 5px;
            border-radius: 6px;
            position: absolute;
            z-index: 1;
            bottom: 125%;
            left: 50%;
            margin-left: -60px;
            opacity: 0;
            transition: opacity 0.3s;
            font-size: 10px;
            width: 120px;
        }

        .leave-day:hover .leave-day-tooltip {
            visibility: visible;
            opacity: 1;
        }

        .restricted-holiday {
            background-color: #ff9999; /* Light red for restricted holidays */
            font-weight: bold;
        }

        .gazetted-holiday {
            background-color: #99ccff; /* Light blue for gazetted holidays */
            font-weight: bold;
        }

        .holiday-tooltip {
            display: none;
            position: absolute;
            background-color: #333;
            color: #fff;
            padding: 5px;
            border-radius: 3px;
            z-index: 100;
            width: 150px;
            text-align: center;
        }

        .calendar-day:hover .holiday-tooltip {
            display: block;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <div class="form-container">
            <h2 class="page-title">Apply Leave</h2>

            <div class="calendar-container">
                <h3>Your Leave Calendar</h3>
                <div class="calendar-legend">
                    <div class="legend-item"><span class="legend-color cl-color"></span>CL - Casual Leave</div>
                    <div class="legend-item"><span class="legend-color el-color"></span>EL - Earned Leave</div>
                    <div class="legend-item"><span class="legend-color ml-color"></span>ML - Medical Leave</div>
                    <div class="legend-item"><span class="legend-color rl-color"></span>RL - Restricted Leave</div>
                    <div class="legend-item"><span class="legend-color lop-color"></span>LOP - Loss of Pay</div>
                    <div class="legend-item"><span class="legend-color pending-color"></span>Pending</div>
                    <span class="legend-item"><span class="legend-color restricted-holiday"></span> Restricted Holiday</span>
                    <span class="legend-item"><span class="legend-color gazetted-holiday"></span> Gazetted Holiday</span>
                </div>
                <div id="yearCalendar" runat="server" class="year-calendar"></div>
            </div>

            <%-- Manager Warning Alert --%>
            <asp:Panel ID="pnlNoManager" runat="server" CssClass="alert alert-warning" Visible="false">
                No reporting manager is assigned to you. Your leave application will still be submitted but might face delays in approval.
            </asp:Panel>

            <%-- Medical Leave Info Alert --%>
            <asp:Panel ID="pnlMedicalLeaveInfo" runat="server" CssClass="alert alert-info" Visible="false">
                Note: Medical Leave will be considered as half day LOP.
            </asp:Panel>

            <%-- Leave Type Selection, Date Selection, --%>
            <div class="row mb-6">
                <div class="col-md-3">
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

                <div class="col-md-2">
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
                <div class="col-md-2">
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
                <%-- Leave Reason --%>
                <div class="col-md-5">
                    <label for="txtReason" class="form-label">Reason</label>
                    <asp:TextBox ID="txtReason" runat="server" CssClass="form-control"
                        TextMode="SingleLine" Rows="1" MaxLength="150" onkeyup="updateCharCount(this);" />
                    <div class="calendar-info">
                        <span id="charCount">150</span> characters remaining
                    </div>
                </div>
            </div>

            <%-- Hald day leave checkbox --%>
            <div class="mb-4">
                <asp:CheckBox ID="chkHalfDay" runat="server" Text="  Half Day Leave"
                    AutoPostBack="true" OnCheckedChanged="chkHalfDay_CheckedChanged" />
            </div>

            <%-- Restricted Leave Calendar Link --%>
            <%--<asp:Panel ID="pnlRestrictedLeave" runat="server" CssClass="mb-4" Visible="false">
                <asp:HyperLink ID="lnkViewHolidays" runat="server" NavigateUrl="~/ViewRestrictedHolidays.aspx"
                    Target="_blank" CssClass="btn btn-outline-primary btn-sm">
                    View Restricted Holidays Calendar
                </asp:HyperLink>
            </asp:Panel>--%>

            <%-- Submit Button --%>
            <div class="form-section mb-3">
                <asp:Button ID="btnApplyLeave" runat="server" Text="Apply"
                    CssClass="btn-submit"
                    OnClick="btnApplyLeave_Click" />
            </div>

            <%-- Error Messages --%>
            <asp:Label ID="lblError" runat="server" CssClass="validation-error mt-3 d-block" />
        </div>
    </div>

    <script type="text/javascript">
        function updateCharCount(textBox) {
            var maxLength = 150;
            var currentLength = textBox.value.length;
            var remaining = maxLength - currentLength;
            document.getElementById('charCount').innerHTML = remaining;
        }

        // Initialize the counter when the page loads
        window.onload = function () {
            updateCharCount(document.getElementById('<%= txtReason.ClientID %>'));
        };
    </script>
</asp:Content>
