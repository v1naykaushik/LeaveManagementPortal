<%@ Page Title="Item Allotment" Language="C#" MasterPageFile="~/LeaveManagementPortalMaster.Master" AutoEventWireup="true" CodeBehind="ItemAllotment.aspx.cs" Inherits="LeaveManagementPortal.ItemAllotment" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .allotment-container {
            max-width: 1000px;
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

        .item-details-section {
            margin-bottom: 2rem;
            padding: 1.5rem;
            border: 1px solid #e2e8f0;
            border-radius: 8px;
            background-color: #f8f9fa;
        }

        .allotment-form-section {
            margin-bottom: 2rem;
            padding: 1.5rem;
            border: 1px solid #e2e8f0;
            border-radius: 8px;
            background-color: #f8f9fa;
        }

        .allotments-section {
            margin-bottom: 2rem;
        }

        .section-title {
            color: #1a237e;
            font-size: 1.25rem;
            font-weight: 500;
            margin-bottom: 1.5rem;
            padding-bottom: 0.5rem;
            border-bottom: 2px solid #1a237e;
        }

        .btn-save {
            background-color: #1a237e;
            color: white;
            padding: 0.5rem 1.5rem;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            transition: background-color 0.3s;
        }

            .btn-save:hover {
                background-color: #151c5e;
                transform: translateY(-1px);
                color: white !important;
            }

        .grid-container {
            overflow-x: auto;
        }

        .grid-allotments {
            width: 100%;
            border-collapse: collapse;
        }

        .grid-header {
            background-color: #1a237e;
            color: white;
            font-weight: 500;
        }

            .grid-header th {
                padding: 0.75rem;
                text-align: left;
            }

        .grid-item {
            background-color: white;
        }

            .grid-item:nth-child(even) {
                background-color: #f8f9fa;
            }

            .grid-item td {
                padding: 0.75rem;
                border-bottom: 1px solid #e2e8f0;
            }

        .no-results {
            padding: 2rem;
            text-align: center;
            color: #666;
            font-style: italic;
        }

        .thumbnail {
            width: 80px;
            height: 80px;
            object-fit: cover;
            border-radius: 4px;
            margin-right: 20px;
        }

        .detail-label {
            font-weight: 600;
            color: #555;
            min-width: 150px;
            display: inline-block;
            text-align: right;
            margin-right: 10px;
        }

        .detail-value {
            font-weight: 400;
        }

        .validation-message {
            color: #d32f2f;
            font-size: 0.875rem;
            margin-top: 0.25rem;
        }

        .success-message {
            color: #388e3c;
            font-size: 0.875rem;
            margin-top: 0.25rem;
            padding: 0.5rem;
            background-color: #e8f5e9;
            border-radius: 4px;
        }

        .back-link {
            color: #1a237e;
            text-decoration: none;
            display: inline-flex;
            align-items: center;
            margin-bottom: 1rem;
        }

            .back-link:hover {
                text-decoration: underline;
            }

            .back-link i {
                margin-right: 0.5rem;
            }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <div class="allotment-container">
            <a href="SearchInventory.aspx" class="back-link">
                <i class="fas fa-arrow-left"></i> Back to Inventory
            </a>

            <h2 class="page-title">Item Allotment</h2>

            <asp:Panel ID="pnlItemNotFound" runat="server" Visible="false">
                <div class="alert alert-danger">
                    Item not found. Please return to the <a href="SearchInventory.aspx">inventory page</a> and try again.
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlItemDetails" runat="server">
                <div class="item-details-section">
                    <h3 class="section-title">Item Details</h3>
                    <div class="row">
                        <div class="col-md-3">
                            <asp:Image ID="imgItem" runat="server" CssClass="thumbnail" />
                        </div>
                        <div class="col-md-9">
                            <div class="mb-2">
                                <span class="detail-label">Name:</span>
                                <span class="detail-value">
                                    <asp:Label ID="lblItemName" runat="server"></asp:Label>
                                </span>
                            </div>
                            <div class="mb-2">
                                <span class="detail-label">ID:</span>
                                <span class="detail-value">
                                    <asp:Label ID="lblItemID" runat="server"></asp:Label>
                                </span>
                            </div>
                            <div class="mb-2">
                                <span class="detail-label">Available Quantity:</span>
                                <span class="detail-value">
                                    <asp:Label ID="lblQuantity" runat="server"></asp:Label>
                                </span>
                            </div>
                            <div class="mb-2">
                                <span class="detail-label">Price:</span>
                                <span class="detail-value">
                                    <asp:Label ID="lblPrice" runat="server"></asp:Label>
                                </span>
                            </div>
                        </div>
                    </div>
                </div>
                <%--New allotment form--%>
                <div class="allotment-form-section">
                    <h3 class="section-title">New Allotment</h3>
                    
                    <asp:Panel ID="pnlSuccess" runat="server" CssClass="success-message mb-3" Visible="false">
                        <asp:Label ID="lblSuccess" runat="server"></asp:Label>
                    </asp:Panel>

                    <div class="row mb-3">
                        <div class="col-md-4">
                            <div class="form-group">
                                <label for="txtPersonName" class="form-label">Person Name <span class="text-danger">*</span></label>
                                <asp:TextBox ID="txtPersonName" runat="server" CssClass="form-control" />
                                <asp:RequiredFieldValidator ID="rfvPersonName" runat="server" 
                                    ControlToValidate="txtPersonName" 
                                    ErrorMessage="Person name is required." 
                                    CssClass="validation-message" 
                                    Display="Dynamic" />
                            </div>
                        </div>

                        <div class="col-md-4">
                            <div class="form-group">
                                <label for="txtOrganization" class="form-label">Organization </label>
                                <asp:TextBox ID="txtOrganization" runat="server" CssClass="form-control" />
                            </div>
                        </div>

                        <div class="col-md-4">
                            <div class="form-group">
                                <label for="txtQuantity" class="form-label">Quantity <span class="text-danger">*</span></label>
                                <asp:TextBox ID="txtQuantity" runat="server" CssClass="form-control" Text="1" TextMode="Number" min="1" />
                                <asp:RequiredFieldValidator ID="rfvQuantity" runat="server" 
                                    ControlToValidate="txtQuantity" 
                                    ErrorMessage="Quantity is required." 
                                    CssClass="validation-message" 
                                    Display="Dynamic" />
                                <asp:RangeValidator ID="rvQuantity" runat="server" 
                                    ControlToValidate="txtQuantity" 
                                    Type="Integer" 
                                    MinimumValue="1" 
                                    MaximumValue="9999" 
                                    ErrorMessage="Quantity must be at least 1." 
                                    CssClass="validation-message" 
                                    Display="Dynamic" />
                                <asp:CustomValidator ID="cvQuantity" runat="server" 
                                    ControlToValidate="txtQuantity" 
                                    OnServerValidate="cvQuantity_ServerValidate" 
                                    ErrorMessage="Quantity exceeds available stock." 
                                    CssClass="validation-message" 
                                    Display="Dynamic" />
                            </div>
                        </div>

                        <div class="col-md-4">
                            <div class="mt-4">
                            <label for="txtAllotmentDate" class="form-label">Allotment Date <span class="text-danger">*</span></label>
                            <div class="date-field-container">
                                <asp:TextBox ID="txtAllotmentDate" runat="server" 
                                    CssClass="form-control date-field" 
                                    TextMode="Date" 
                                    onclick="this.showPicker()" />
                                <asp:RequiredFieldValidator ID="rfvAllotmentDate" runat="server" 
                                    ControlToValidate="txtAllotmentDate"
                                    Display="Dynamic"
                                    CssClass="validation-message"
                                    ErrorMessage="Allotment date is required." />
                                </div>
                            </div>
                        </div>

                        <div class="col-md-6">
                                <div class="mt-4">
                                <label for="txtRemarks" class="form-label">Remarks </label>
                                <asp:TextBox ID="txtRemarks" runat="server" CssClass="form-control" 
                                    MaxLength="200" onkeyup="updateCharCount(this);"/>
                                    <div class="calendar-info">
                                        <span id="charCount">200</span> characters remaining
                                    </div>
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12 text-end">
                            <asp:Button ID="btnSave" runat="server" Text="Save Allotment"
                                CssClass="btn btn-save" OnClick="btnSave_Click" />
                        </div>
                    </div>
                </div>
                <%--Previous allotment section--%>
                <div class="allotments-section">
                    <h3 class="section-title">Previous Allotments</h3>

                    <div class="grid-container">
                        <asp:GridView ID="gvAllotments" runat="server" AutoGenerateColumns="False"
                            CssClass="grid-allotments" HeaderStyle-CssClass="grid-header" RowStyle-CssClass="grid-item"
                            EmptyDataText="No previous allotments found for this item." EmptyDataRowStyle-CssClass="no-results"
                            AllowPaging="true" PageSize="10" OnPageIndexChanging="gvAllotments_PageIndexChanging">
                            <Columns>
                                <asp:BoundField DataField="ID" HeaderText="Allotment ID" />
                                <asp:BoundField DataField="Name" HeaderText="Person Name" />
                                <asp:BoundField DataField="Organization" HeaderText="Organization" />
                                <asp:BoundField DataField="Quantity" HeaderText="Quantity" />
                                <asp:BoundField DataField="AllotmentDate" HeaderText="Allotment Date" DataFormatString="{0:dd/MMM/yyyy}" />
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
            </asp:Panel>
        </div>
    </div>

    <script type="text/javascript">
        function updateCharCount(textBox) {
            var maxLength = 200;
            var currentLength = textBox.value.length;
            var remaining = maxLength - currentLength;
            document.getElementById('charCount').innerHTML = remaining;
        }

        // Initialize the counter when the page loads
        window.onload = function() {
            updateCharCount(document.getElementById('<%= txtRemarks.ClientID %>'));
        };
    </script>
</asp:Content>