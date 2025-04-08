<%@ Page Title="Pending Leave Approvals" Language="C#" MasterPageFile="~/LeaveManagementPortalMaster.Master" AutoEventWireup="true" CodeBehind="PendingLeaveApprovals.aspx.cs" Inherits="LeaveManagementPortal.PendingLeaveApprovals" %>

<%@ Import Namespace="System.Web" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .leave-tile {
            background: white;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            padding: 15px;
            margin-bottom: 15px;
            transition: transform 0.2s;
        }

        .page-title {
            color: #1a237e;
            font-size: 2rem;
            font-weight: 600;
            margin-bottom: 2rem;
            margin-top: 150px;
        }

        .leave-content {
            display: flex;
            justify-content: space-between;
            align-items: center;
        }

        .leave-info {
            flex-grow: 1;
            margin-left: 15px;
        }

        .checkbox-container {
            display: flex;
            align-items: center;
            margin-right: 15px;
        }

        .tile-checkbox {
            width: 20px;
            height: 20px;
            cursor: pointer;
        }

        .employee-info {
            display: flex;
            align-items: center;
            gap: 15px;
            margin-bottom: 8px;
        }

        .employee-name {
            font-size: 1.1rem;
            font-weight: 600;
            color: #1a237e;
        }

        .employee-id {
            color: #666;
            font-size: 0.9rem;
            padding: 2px 6px;
            background-color: #f5f5f5;
            border-radius: 4px;
        }

        .leave-details {
            display: flex;
            align-items: center;
            gap: 15px;
            margin-bottom: 8px;
        }

        .leave-type {
            background-color: #e8f0fe;
            color: #1a237e;
            padding: 4px 8px;
            border-radius: 4px;
            font-size: 0.9rem;
        }

        .leave-duration {
            color: #333;
            font-size: 0.9rem;
            font-weight: 500;
        }

        .leave-dates {
            color: #666;
            font-size: 0.9rem;
        }

        .action-buttons {
            display: flex;
            gap: 8px;
            margin-left: 20px;
        }

        .manager-status {
            padding: 4px 8px;
            border-radius: 4px;
            font-size: 0.85rem;
            font-weight: 500;
            margin-left: 15px;
        }

        .status-approved {
            background-color: #e8f5e9;
            color: #2e7d32;
        }

        .status-pending {
            background-color: #fff3e0;
            color: #ef6c00;
        }

        .btn-approve {
            background-color: #4caf50;
            color: white;
            border: none;
            padding: 6px 12px;
            border-radius: 4px;
            cursor: pointer;
            transition: opacity 0.3s ease;
        }

        .btn-reject {
            background-color: #f44336;
            color: white;
            border: none;
            padding: 6px 12px;
            border-radius: 4px;
            cursor: pointer;
            transition: opacity 0.3s ease;
        }

        .btn-approve.faded, .btn-reject.faded {
            opacity: 0.5;
            cursor: not-allowed;
        }

        .batch-actions {
            background: #f8f9fa;
            padding: 15px;
            border-radius: 8px;
            margin-top: 10px;
        }

        .save-button-container {
            position: fixed;
            bottom: 80px; /* Adjusted to not overlap with footer */
            left: 250px;
            right: 0;
            background: rgba(26, 35, 126, 0.7); /* Translucent background */
            backdrop-filter: blur(5px); /* Adds frosted glass effect */
            padding: 15px 30px; /* Increased horizontal padding */
            display: flex;
            justify-content: center;
            align-items: center;
            /*            box-shadow: 0 -4px 15px rgba(0,0,0,0.1);*/
            z-index: 1000;
            border-top: 1px solid rgba(255,255,255,0.1); /* Subtle top border */
        }

        /*.search-container .input-group {
            min-width: 300px;
        }

        .search-container .btn {
            min-width: 70px;
        }*/

        .selection-count {
            color: white;
            font-size: 0.9rem;
        }

        .btn-save {
            background-color: #4CAF50;
            color: white;
            border: none;
            padding: 6px 20px;
            border-radius: 4px;
            cursor: pointer;
        }

        .btn-save:disabled {
            background-color: #cccccc;
            cursor: not-allowed;
        }

        /* Add padding to main content to prevent overlap with fixed footer */
        .container-fluid {
            padding-bottom: 60px;
        }

        .action-count {
            margin-right: auto;
            color: #666;
            font-size: 0.9rem;
            padding: 8px 0;
        }

        /* Add space at bottom to prevent content from being hidden behind fixed footer */
        .container-fluid {
            padding-bottom: 80px;
        }

        .select-all-container {
            margin-bottom: 15px;
            padding: 10px;
            background-color: #f8f9fa;
            border-radius: 8px;
            display: flex;
            align-items: center;
        }

        .select-all-label {
            margin-left: 10px;
            color: #666;
            font-size: 0.9rem;
        }

        .modal-header {
            background: #1a237e;
            padding: 0.75rem 1rem;
        }

        .modal-title {
            color: white;
            font-size: 1.1rem;
            font-weight: 500;
        }

        .modal-body {
            padding: 1.5rem;
        }

        .row.g-4 {
            gap: 1rem;
        }

        .approval-status {
            padding: 0.5rem 0;
            font-weight: 500;
        }

        .leave-id {
            color: #666;
            font-size: 0.9rem;
            background-color: #f8f9fa;
            padding: 2px 8px;
            border-radius: 4px;
            margin-left: 8px;
        }

        /*.pagination-container {
            margin: 20px 0;
        }

        .page-link {
            padding: 8px 16px;
            margin: 0 4px;
            border-radius: 4px;
            color: #1a237e;
            background: white;
            border: 1px solid #dee2e6;
        }

        .page-link.active {
            background: #1a237e;
            color: white;
            border-color: #1a237e;
        }*/
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager runat="server"></asp:ScriptManager>

    <div class="container-fluid">
        <h2 class="page-title">Leave Approvals</h2>

        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnSaveChanges" EventName="Click" />
            </Triggers>
            <ContentTemplate>
                <!-- Select All Checkbox -->
                <%--<div class="select-all-container">
                    <input type="checkbox" id="selectAll" class="tile-checkbox" onclick="toggleAllCheckboxes()" />
                    <label for="selectAll" class="select-all-label">Select All</label>
                </div>--%>
                <div class="batch-actions mb-3">
                    <button type="button" class="btn btn-success me-2" onclick="approveSelected()">
                        <i class="fas fa-check me-1"></i>Approve Selected
                    </button>
                    <button type="button" class="btn btn-danger me-2" onclick="rejectSelected()">
                        <i class="fas fa-times me-1"></i>Reject Selected
                    </button>
                    <button type="button" class="btn btn-secondary" onclick="clearSelected()">
                        <i class="fas fa-undo me-1"></i>Clear Selected
                    </button>
                </div>


                <!-- Pending Leaves Grid -->
                <div class="row">
                    <asp:Repeater ID="rptLeaveRequests" runat="server">
                        <ItemTemplate>
                            <div class="col-12">
                                <div class="leave-tile">
                                    <div class="leave-content">
                                        <!-- Checkbox -->
                                        <div class="checkbox-container">
                                            <input type="checkbox" class="tile-checkbox"
                                                data-leaveid='<%# Eval("LeaveId") %>'
                                                onclick="updateSelectedCount()" />
                                        </div>

                                        <!-- Leave Info -->
                                        <div class="leave-info">
                                            <!-- Employee Info -->
                                            <div class="employee-info">
                                                <span class="employee-name"><%# Eval("EmployeeName") %></span>
                                                <span class="employee-id">ID: <%# Eval("EmployeeId") %></span>
                                            </div>

                                            <!-- Leave Type & Duration -->
                                            <div class="leave-details">
                                                <span class="leave-type"><%# Eval("LeaveTypeName") %></span>
                                                <span class="leave-duration"><%# FormatDuration(Eval("Duration"), Eval("IsHalfDay")) %></span>
                                                <%--<span class="leave-id ms-2">Leave ID: <%# Eval("LeaveID") %></span>--%>
                                                <span class="leave-dates">
                                                    <%# Eval("StartDate", "{0:dd MMM yyyy}") %>
                                                    <%# Eval("StartDate").ToString() != Eval("EndDate").ToString() ? " - " + Eval("EndDate", "{0:dd MMM yyyy}") : "" %>
                                                </span>
                                                <%# Session["UserRole"].ToString() == "Director" ? 
                                                    "<span class='manager-status " + (Eval("ManagerApprovalStatus").ToString() == "Approved" ? "status-approved" : "status-pending") + "'>" +
                                                    "Manager: " + Eval("ManagerApprovalStatus") + "</span>" : "" %>
                                            </div>

                                            <!-- Dates -->
                                            <div>
                                                 <span class="leave-duration">Reason:</span> 
                                                <span class="leave-dates">
                                                    <%# string.IsNullOrEmpty(Eval("LeaveReason").ToString()) ? "No reason provided" : Eval("LeaveReason") %>
                                                </span>
                                            </div>
                                        </div>

                                        <!-- Action Buttons -->
                                        <div class="action-buttons">
                                            <button type="button" class="btn-approve"
                                                onclick="markForAction(<%# Eval("LeaveId") %>, 'approve')">
                                                Approve
                                            </button>
                                            <button type="button" class="btn-reject"
                                                onclick="markForAction(<%# Eval("LeaveId") %>, 'reject')">
                                                Reject
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>

                    <asp:Panel ID="pnlNoLeaves" runat="server" CssClass="col-12" Visible="false">
                        <div class="alert alert-info">
                            No pending leave requests to display.
                        </div>
                    </asp:Panel>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>

        <!-- Save Button Container -->
        <div class="save-button-container">
            <span class="selection-count"></span>
            <asp:Button ID="btnSaveChanges" runat="server" Text="Save Changes"
                CssClass="btn-save" OnClientClick="return setHiddenField();" OnClick="btnSaveChanges_Click" Enabled="false" />
            <%--<asp:Button ID="btnTestApprove" runat="server" Text="Test Approve Leave 1011"
                OnClick="btnTestApprove_Click" CssClass="btn btn-warning" />--%>
        </div>

        
    </div>

    <script type="text/javascript">
        var pendingActions = {};
        var selectedLeaves = new Set();
        var selectedActions = {}; // Will store leaveId -> action mappings

        function approveSelected() {
            document.querySelectorAll('.tile-checkbox[data-leaveid]:checked').forEach(function (checkbox) {
                markForAction(checkbox.dataset.leaveid, 'approve');
            });
        }

        function rejectSelected() {
            document.querySelectorAll('.tile-checkbox[data-leaveid]:checked').forEach(function (checkbox) {
                markForAction(checkbox.dataset.leaveid, 'reject');
            });
        }

        function updateSelectedCount() {
            console.log("here in updateSelectedCount");
            var hasActions = Object.keys(pendingActions).length > 0;
            var countText = selectedLeaves.size + ' leaves selected';
            console.log("Debug: count of selected", countText);
            document.querySelector('.selection-count').textContent = "";
            console.log("Selected Leaves Size:", selectedLeaves.size, selectedLeaves);

            // Enable/disable save button
            var saveButton = document.getElementById('<%= btnSaveChanges.ClientID %>');
            if (saveButton) {
                saveButton.disabled = !hasActions;
            }
        }

        function markForAction(leaveId, action) {
            // Store action for this leave ID
            selectedActions[leaveId] = action;

            // code for button fading
            const approveBtn = document.querySelector(`button[onclick*="markForAction(${leaveId}, 'approve')"]`);
            const rejectBtn = document.querySelector(`button[onclick*="markForAction(${leaveId}, 'reject')"]`);
            if (action === 'approve') {
                approveBtn.classList.remove('faded');
                rejectBtn.classList.add('faded');
            } else {
                approveBtn.classList.add('faded');
                rejectBtn.classList.remove('faded');
            }

            // Find and check the checkbox
            const checkbox = document.querySelector(`.tile-checkbox[data-leaveid="${leaveId}"]`);
            if (checkbox) {
                checkbox.checked = true;
            }

            // Collect all leave IDs and their actions
            var data = {
                leaveIds: Object.keys(selectedActions),
                actions: selectedActions
            };

            // Set the hidden field with complete data
            var hiddenField = document.getElementById('<%= hdnPendingActions.ClientID %>');
            hiddenField.value = JSON.stringify(data);

            // Enable save button
            var saveButton = document.getElementById('<%= btnSaveChanges.ClientID %>');
            if (saveButton) {
                saveButton.disabled = false;
            }

            console.log('Current selected actions:', selectedActions);
        }

        function clearSelected() {
            document.querySelectorAll('.tile-checkbox[data-leaveid]:checked').forEach(function (checkbox) {
                const leaveId = checkbox.dataset.leaveid;

                // Remove from pending actions
                delete pendingActions[leaveId];

                // Reset button states
                const approveBtn = document.querySelector(`button[onclick*="markForAction(${leaveId}, 'approve')"]`);
                const rejectBtn = document.querySelector(`button[onclick*="markForAction(${leaveId}, 'reject')"]`);
                if (approveBtn) approveBtn.classList.remove('faded');
                if (rejectBtn) rejectBtn.classList.remove('faded');

                // Uncheck the checkbox
                checkbox.checked = false;
                selectedLeaves.delete(leaveId);
            });

            // Update UI
            updateSelectedCount();

            // Uncheck "Select All" if it's checked
            document.getElementById('selectAll').checked = false;
        }

        function cancelPendingActions() {
            pendingActions = {};
            selectedLeaves.clear();

            // Reset all checkboxes and button states
            document.querySelectorAll('.tile-checkbox').forEach(function (checkbox) {
                checkbox.checked = false;
            });

            document.querySelectorAll('.btn-approve, .btn-reject').forEach(function (btn) {
                btn.style.opacity = '1';
            });

            // Hide the footer actions
            document.getElementById('footerActions').style.display = 'none';

            // Reset select all checkbox
            document.getElementById('selectAll').checked = false;

            updateSelectedCount();
        }

        

        // Function to prepare data for submission
        function prepareSubmissionData() {
            return JSON.stringify({
                pendingActions: pendingActions,
                selectedLeaves: Array.from(selectedLeaves)
            });
        }

        // Just before form submission
        function setHiddenField() {
            var hiddenField = document.getElementById('<%= hdnPendingActions.ClientID %>');
            if (!hiddenField) {
                console.error('Hidden field not found');
                return false;
            }

            if (Object.keys(pendingActions).length === 0) {
                console.error('No pending actions found');
                return false;
            }

            var data = {
                pendingActions: pendingActions,
                selectedLeaves: Array.from(selectedLeaves)
            };

            hiddenField.value = JSON.stringify(data);
            //console.log('Setting hidden field value:', hiddenField.value);
            if (window.location.hostname === 'localhost') {
                console.log('Setting hidden field value:', hiddenField.value);
            }
            return true;
        }
    </script>

    <!-- Hidden field to store pending actions -->
    <asp:HiddenField ID="hdnPendingActions" runat="server" />
    <asp:Label ID="lblDebugHidden" runat="server" />
    <!-- Bootstrap Bundle with Popper -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js"
        integrity="sha384-ka7Sk0Gln4gmtz2MlQnikT1wXgYsOg+OMhuP+IlRH9sENBO0LRn5q+8nbTov4+1p"
        crossorigin="anonymous"></script>

    <!-- For debugging -->
    <script>
        console.log("Bootstrap version:", typeof bootstrap !== 'undefined' ? 'Loaded' : 'Not loaded');
    </script>
</asp:Content>
