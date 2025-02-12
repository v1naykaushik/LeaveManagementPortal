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
            justify-content: space-between;
            align-items: center;
            /*            box-shadow: 0 -4px 15px rgba(0,0,0,0.1);*/
            z-index: 1000;
            border-top: 1px solid rgba(255,255,255,0.1); /* Subtle top border */
        }

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

        /* Modal Styles */
        .detail-card {
            position: relative;
            padding: 1rem 1.5rem;
            margin-bottom: 1rem;
            border-left: 3px solid #1a237e;
            background: #fff;
        }

        .detail-card-title {
            position: absolute;
            top: -10px;
            left: -10px;
            color: #1a237e;
            font-size: 0.9rem;
            font-weight: 600;
            background: #fff;
            padding: 0 10px;
            border-radius: 4px;
            box-shadow: 0 1px 3px rgba(0,0,0,0.1);
        }

        .detail-label {
            color: #666;
            font-size: 0.8rem;
            margin-bottom: 0.25rem;
        }

        .detail-value {
            color: #2c3e50;
            font-weight: 500;
            font-size: 0.95rem;
        }

        /*        .approval-status {
            padding: 1rem;
            background: white;
            border-radius: 4px;
            border: 1px solid #e9ecef;
        }*/

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
                <div class="select-all-container">
                    <input type="checkbox" id="selectAll" class="tile-checkbox" onclick="toggleAllCheckboxes()" />
                    <label for="selectAll" class="select-all-label">Select All</label>
                </div>
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
                                        <div class="leave-info"
                                            onclick="showLeaveDetails('<%# HttpUtility.JavaScriptStringEncode(Eval("EmployeeName").ToString()) %>', 
                                                                      '<%# HttpUtility.JavaScriptStringEncode(Eval("LeaveTypeName").ToString()) %>', 
                                                                      '<%# Eval("StartDate", "{0:dd MMM yyyy}") %>', 
                                                                      '<%# Eval("EndDate", "{0:dd MMM yyyy}") %>', 
                                                                      '<%# Eval("Duration") %>', 
                                                                      '<%# HttpUtility.JavaScriptStringEncode(Eval("EmployeeId").ToString()) %>', 
                                                                      '<%# HttpUtility.JavaScriptStringEncode((Eval("LeaveReason") ?? "").ToString()) %>', 
                                                                      <%# Eval("IsHalfDay").ToString().ToLower() %>, 
                                                                      '<%# Eval("ManagerApprovalStatus") %>', 
                                                                      '<%# Eval("DirectorApprovalStatus") %>',
                                                                      '<%# Eval("LeaveID") %>')"
                                            style="cursor: pointer;">
                                            <!-- Employee Info -->
                                            <div class="employee-info">
                                                <span class="employee-name"><%# Eval("EmployeeName") %></span>
                                                <span class="employee-id">ID: <%# Eval("EmployeeId") %></span>
                                            </div>

                                            <!-- Leave Type & Duration -->
                                            <div class="leave-details">
                                                <span class="leave-type"><%# Eval("LeaveTypeName") %></span>
                                                <span class="leave-duration"><%# FormatDuration(Eval("Duration"), Eval("IsHalfDay")) %></span>
                                                <span class="leave-id ms-2">Leave ID: <%# Eval("LeaveID") %></span>
                                                <%# Session["UserRole"].ToString() == "Director" ? 
                                                    "<span class='manager-status " + (Eval("ManagerApprovalStatus").ToString() == "Approved" ? "status-approved" : "status-pending") + "'>" +
                                                    "Manager: " + Eval("ManagerApprovalStatus") + "</span>" : "" %>
                                            </div>

                                            <!-- Dates -->
                                            <div class="leave-dates">
                                                <%# Eval("StartDate", "{0:dd MMM yyyy}") %>
                                                <%# Eval("StartDate").ToString() != Eval("EndDate").ToString() ? " - " + Eval("EndDate", "{0:dd MMM yyyy}") : "" %>
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
            <span class="selection-count">0 leaves selected</span>
            <asp:Button ID="btnSaveChanges" runat="server" Text="Save Changes"
                CssClass="btn-save" OnClientClick="return setHiddenField();" OnClick="btnSaveChanges_Click" Enabled="false" />
            <asp:Button ID="btnTestApprove" runat="server" Text="Test Approve Leave 1011"
                OnClick="btnTestApprove_Click" CssClass="btn btn-warning" />
        </div>

        <!-- Leave Details Modal -->
        <div class="modal fade" id="leaveDetailsModal" tabindex="-1">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header bg-primary text-white">
                        <h5 class="modal-title">
                            <i class="fas fa-calendar-alt me-2"></i>Leave Request Details
                        </h5>
                        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <div class="row g-4">
                            <!-- Employee Information -->
                            <div class="col-md-12">
                                <div class="detail-card">
                                    <h6 class="detail-card-title">
                                        <i class="fas fa-calendar-check me-2"></i>Employee Information
                                    </h6>
                                    <div class="row gy-2">
                                        <div class="col-6">
                                            <label class="detail-label">Name</label>
                                            <div class="detail-value" id="modalEmployeeName"></div>
                                        </div>
                                        <div class="col-6">
                                            <label class="detail-label">Employee ID</label>
                                            <div class="detail-value" id="modalEmployeeId"></div>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <!-- Leave Information -->
                            <div class="col-md-12">
                                <div class="detail-card">
                                    <h6 class="detail-card-title">
                                        <i class="fas fa-calendar-check me-2"></i>Leave Details
                                    </h6>
                                    <div class="row g-3">
                                        <div class="col-6">
                                            <label class="detail-label">Leave ID</label>
                                            <div class="detail-value" id="modalLeaveId"></div>
                                        </div>
                                        <div class="col-md-6">
                                            <label class="detail-label">Leave Type</label>
                                            <div class="detail-value" id="modalLeaveType"></div>
                                        </div>
                                        <div class="col-md-6">
                                            <label class="detail-label">Duration</label>
                                            <div class="detail-value" id="modalDuration"></div>
                                        </div>
                                        <div class="col-md-12">
                                            <label class="detail-label">Leave Period</label>
                                            <div class="detail-value" id="modalDates"></div>
                                        </div>
                                        <div class="col-md-12">
                                            <label class="detail-label">Reason for Leave</label>
                                            <div class="detail-value" id="modalReason"></div>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <!-- Approval Status -->
                            <div class="col-md-12">
                                <div class="detail-card">
                                    <h6 class="detail-card-title">
                                        <i class="fas fa-check-circle me-2"></i>Approval Status
                                    </h6>
                                    <div class="approval-status" id="modalApprovalStatus"></div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
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

        function toggleAllCheckboxes() {
            var checkboxes = document.querySelectorAll('.tile-checkbox');
            var selectAllChecked = document.getElementById('selectAll').checked;

            checkboxes.forEach(function (checkbox) {
                if (checkbox.dataset.leaveid) {
                    checkbox.checked = selectAllChecked;
                    if (selectAllChecked) {
                        selectedLeaves.add(checkbox.dataset.leaveid);
                    } else {
                        selectedLeaves.delete(checkbox.dataset.leaveid);
                        delete pendingActions[checkbox.dataset.leaveid];

                        // Reset button states
                        const approveBtn = document.querySelector(`button[onclick*="markForAction(${checkbox.dataset.leaveid}, 'approve')"]`);
                        const rejectBtn = document.querySelector(`button[onclick*="markForAction(${checkbox.dataset.leaveid}, 'reject')"]`);
                        if (approveBtn) approveBtn.classList.remove('faded');
                        if (rejectBtn) rejectBtn.classList.remove('faded');
                    }
                }
            });

            updateSelectedCount();
        }

        function updateSelectedCount() {
            var hasActions = Object.keys(pendingActions).length > 0;
            var countText = selectedLeaves.size + ' leaves selected';
            document.querySelector('.selection-count').textContent = countText;

            // Enable/disable save button
            var saveButton = document.getElementById('<%= btnSaveChanges.ClientID %>');
            if (saveButton) {
                saveButton.disabled = !hasActions;
            }
        }

        function markForAction(leaveId, action) {
            // Store action for this leave ID
            selectedActions[leaveId] = action;

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

        function showLeaveDetails(employeeName, leaveType, startDate, endDate, duration, employeeId, reason, isHalfDay, managerStatus, directorStatus, leaveId) {
            console.log("Function called with:", { employeeName, leaveType, startDate, endDate, duration, employeeId, reason, isHalfDay, managerStatus, directorStatus });

            try {
                document.getElementById('modalEmployeeName').textContent = employeeName;
                document.getElementById('modalEmployeeId').textContent = employeeId;
                document.getElementById('modalLeaveType').textContent = leaveType;
                document.getElementById('modalDuration').textContent = isHalfDay ? 'Half Day' : Math.round(duration) + (duration === '1.0' ? ' day' : ' days');
                document.getElementById('modalDates').textContent = startDate + (startDate !== endDate ? ' - ' + endDate : '');
                document.getElementById('modalReason').textContent = reason || 'No reason provided';
                document.getElementById('modalLeaveId').textContent = leaveId;

                // Format approval status
                let approvalStatus = 'Manager: ' + managerStatus;
                if (managerStatus === 'Approved') {
                    approvalStatus += ' | Director: ' + directorStatus;
                }
                document.getElementById('modalApprovalStatus').textContent = approvalStatus;

                // Create and show modal
                const modalElement = document.getElementById('leaveDetailsModal');
                const modal = new bootstrap.Modal(modalElement);
                modal.show();

                console.log("Modal should be visible now");
            } catch (error) {
                console.error("Error in showLeaveDetails:", error);
            }
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
            console.log('Setting hidden field value:', hiddenField.value);
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
