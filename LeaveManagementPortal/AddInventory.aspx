<%@ Page Title="Add Inventory" Language="C#" MasterPageFile="~/LeaveManagementPortalMaster.Master" AutoEventWireup="true" CodeBehind="AddInventory.aspx.cs" Inherits="LeaveManagementPortal.AddInventory" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .inventory-container {
            max-width: 800px;
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

        .form-section {
            margin-bottom: 2rem;
            padding: 1.5rem;
            border: 1px solid #e2e8f0;
            border-radius: 8px;
            background-color: #f8f9fa;
        }

        .section-title {
            color: #1a237e;
            font-size: 1.25rem;
            font-weight: 500;
            margin-bottom: 1.5rem;
            padding-bottom: 0.5rem;
            border-bottom: 2px solid #1a237e;
        }

        .validation-error {
            color: #dc3545;
            font-size: 0.875rem;
            margin-top: 0.25rem;
        }

        .success-message {
            color: #198754;
            font-size: 0.875rem;
            margin-top: 0.25rem;
        }

        .btn-primary {
            background-color: #1a237e;
            color: white;
            padding: 0.5rem 1.5rem;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            transition: background-color 0.3s;
        }

        .btn-primary:hover {
            background-color: #151c5e;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <div class="inventory-container">
            <h2 class="page-title">Add Inventory Item</h2>

            <div class="form-section">
                <h3 class="section-title">Item Details</h3>
                
                <div class="form-group mb-3">
                    <label for="txtName" class="form-label">Name <span class="text-danger">*</span></label>
                    <asp:TextBox ID="txtName" runat="server" CssClass="form-control" />
                    <asp:RequiredFieldValidator ID="rfvName" runat="server" 
                        ControlToValidate="txtName"
                        CssClass="validation-error"
                        Display="Dynamic"
                        ErrorMessage="Name is required." />
                </div>
                
                <div class="form-group mb-3">
                    <label for="ddlCategory" class="form-label">Category</label>
                    <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-control">
                    </asp:DropDownList>
                    <asp:RequiredFieldValidator ID="rfvCategory" runat="server" 
                        ControlToValidate="ddlCategory"
                        CssClass="validation-error"
                        Display="Dynamic"
                        InitialValue="0"
                        ErrorMessage="Category is required." />
                </div>
                
                <div class="form-group mb-3">
                    <label for="txtQuantity" class="form-label">Quantity <span class="text-danger">*</span></label>
                    <asp:TextBox ID="txtQuantity" runat="server" CssClass="form-control" TextMode="Number" />
                    <asp:RequiredFieldValidator ID="rfvQuantity" runat="server" 
                        ControlToValidate="txtQuantity"
                        CssClass="validation-error"
                        Display="Dynamic"
                        ErrorMessage="Quantity is required." />
                    <asp:RangeValidator ID="rvQuantity" runat="server"
                        ControlToValidate="txtQuantity"
                        CssClass="validation-error"
                        Display="Dynamic"
                        MinimumValue="0"
                        MaximumValue="10000"
                        Type="Integer"
                        ErrorMessage="Quantity must be between 0 and 10000." />
                </div>
                
                <div class="form-group mb-3">
                    <label for="txtPrice" class="form-label">Price</label>
                    <asp:TextBox ID="txtPrice" runat="server" CssClass="form-control" />
                    <%--<asp:RequiredFieldValidator ID="rfvPrice" runat="server" 
                        ControlToValidate="txtPrice"
                        CssClass="validation-error"
                        Display="Dynamic"
                        ErrorMessage="Price is required." />--%>
                    <asp:RegularExpressionValidator ID="revPrice" runat="server"
                        ControlToValidate="txtPrice"
                        CssClass="validation-error"
                        Display="Dynamic"
                        ValidationExpression="^\d+(\.\d{1,2})?$"
                        ErrorMessage="Price must be a valid decimal (e.g., 123.45)." />
                </div>
                
                <div class="form-group mb-3">
                    <label for="fuPhoto" class="form-label">Photo </label>
                    <asp:FileUpload ID="fuPhoto" runat="server" CssClass="form-control" />
                </div>

                <div class="form-group mb-3">
                    <label for="txtDescription" class="form-label">Description</label>
                    <asp:TextBox ID="TextBox1" runat="server" CssClass="form-control" />
                </div>
                
                <div class="mt-4">
                    <asp:Button ID="btnSave" runat="server" Text="Save Item" 
                        CssClass="btn btn-primary" OnClick="btnSave_Click" />
                </div>
                
                <asp:Label ID="lblMessage" runat="server" CssClass="mt-3 d-block"></asp:Label>
            </div>
        </div>
    </div>
</asp:Content>