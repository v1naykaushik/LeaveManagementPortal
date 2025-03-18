<%@ Page Title="Search Inventory" Language="C#" MasterPageFile="~/LeaveManagementPortalMaster.Master" AutoEventWireup="true" CodeBehind="SearchInventory.aspx.cs" Inherits="LeaveManagementPortal.SearchInventory" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .inventory-container {
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

        .search-section {
            margin-bottom: 2rem;
            padding: 1.5rem;
            border: 1px solid #e2e8f0;
            border-radius: 8px;
            background-color: #f8f9fa;
        }

        .results-section {
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

        .btn-search {
            background-color: #1a237e;
            color: white;
            padding: 0.5rem 1.5rem;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            transition: background-color 0.3s;
        }

            .btn-search:hover {
                background-color: #151c5e;
                transform: translateY(-1px);
                color: white !important;
            }

        .grid-container {
            overflow-x: auto;
        }

        .grid-inventory {
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

        .thumbnail {
            width: 50px;
            height: 50px;
            object-fit: cover;
            border-radius: 4px;
        }

        .no-results {
            padding: 2rem;
            text-align: center;
            color: #666;
            font-style: italic;
        }

        .animate-image {
            animation: fadeIn 0.3s ease-in-out;
        }

        @keyframes fadeIn {
            from {
                opacity: 0;
                transform: scale(0.9);
            }

            to {
                opacity: 1;
                transform: scale(1);
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <div class="inventory-container">
            <h2 class="page-title">Search Inventory</h2>

            <div class="search-section">
                <h3 class="section-title">Search Criteria</h3>

                <div class="row mb-3">
                    <div class="col-md-4">
                        <div class="form-group">
                            <label for="txtSearchName" class="form-label">Item Name</label>
                            <asp:TextBox ID="txtSearchName" runat="server" CssClass="form-control" />
                        </div>
                    </div>

                    <div class="col-md-4">
                        <div class="form-group">
                            <label for="ddlSearchCategory" class="form-label">Category</label>
                            <asp:DropDownList ID="ddlSearchCategory" runat="server" CssClass="form-control">
                            </asp:DropDownList>
                        </div>
                    </div>

                    <div class="col-md-4">
                        <div class="form-group">
                            <label for="ddlStockStatus" class="form-label">Stock Status</label>
                            <asp:DropDownList ID="ddlStockStatus" runat="server" CssClass="form-control">
                                <asp:ListItem Text="-- Any --" Value="" />
                                <asp:ListItem Text="In Stock" Value="instock" />
                                <asp:ListItem Text="Out of Stock" Value="outofstock" />
                                <asp:ListItem Text="Low Stock (< 10)" Value="lowstock" />
                            </asp:DropDownList>
                        </div>
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-12 text-end">
                        <asp:Button ID="btnSearch" runat="server" Text="Search"
                            CssClass="btn btn-search" OnClick="btnSearch_Click" />
                    </div>
                </div>
            </div>

            <div class="results-section">
                <h3 class="section-title">Search Results</h3>

                <div class="grid-container">
                    <asp:GridView ID="gvInventory" runat="server" AutoGenerateColumns="False"
                        CssClass="grid-inventory" HeaderStyle-CssClass="grid-header" RowStyle-CssClass="grid-item"
                        EmptyDataText="No items found matching your search criteria." EmptyDataRowStyle-CssClass="no-results"
                        AllowPaging="true" PageSize="10" OnPageIndexChanging="gvInventory_PageIndexChanging">
                        <Columns>
                            <asp:BoundField DataField="ID" HeaderText="ID" />
                            <asp:TemplateField HeaderText="Name">
                                <ItemTemplate>
                                    <asp:HyperLink ID="hlItemName" runat="server" 
                                        NavigateUrl='<%# "ItemAllotment.aspx?ItemID=" + Eval("ID") %>'
                                        Text='<%# Eval("Name") %>'>
                                    </asp:HyperLink>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="CategoryName" HeaderText="Category" />
                            <asp:BoundField DataField="ItemQuantity" HeaderText="Quantity" />
                            <asp:TemplateField HeaderText="Price">
                                <ItemTemplate>
                                    <%# Eval("Price") == DBNull.Value || Eval("Price") == null || Eval("Price").ToString() == "" 
                                        ? "N/A" 
                                        : "₹ " + string.Format("{0:N0}", Eval("Price")) %>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Photo">
                                <ItemTemplate>
                                    <asp:Image ID="imgItem" runat="server" CssClass='<%# string.IsNullOrEmpty(Eval("Photo") as string) ? "thumbnail rounded no-photo" : "thumbnail rounded photo-click" %>'
                                        Width="50px" Height="50px"
                                        ImageUrl='<%# string.IsNullOrEmpty(Eval("Photo") as string) ? ResolveUrl("~/Images/no-image.png") : ResolveUrl(Eval("Photo").ToString()) %>'
                                        data-photo='<%# string.IsNullOrEmpty(Eval("Photo") as string) ? "" : ResolveUrl(Eval("Photo").ToString()) %>'
                                        Style='<%# string.IsNullOrEmpty(Eval("Photo") as string) ? "cursor: default;" : "cursor: pointer;" %>' />
                                </ItemTemplate>
                            </asp:TemplateField>


                            <%--<asp:TemplateField HeaderText="Photo">
                                <ItemTemplate>
                                    <asp:Image ID="imgItem" runat="server" CssClass="thumbnail rounded"
                                        Width="50px" Height="50px" Style="cursor: pointer;"
                                        ImageUrl='<%# !string.IsNullOrEmpty(Eval("Photo") as string) ? ResolveUrl(Eval("Photo") as string) : ResolveUrl("~/Images/no-image.png") %>'
                                        onclick='<%# !string.IsNullOrEmpty(Eval("Photo") as string) ? "openPhotoModal(\"" + ResolveUrl(Eval("Photo") as string) + "\")" : "" %>' />
                                </ItemTemplate>
                            </asp:TemplateField>--%>
                            <%--<asp:TemplateField HeaderText="Photo">
                            <ItemTemplate>
                                <asp:ImageButton ID="imgItem" runat="server" CssClass="thumbnail rounded"
                                    Width="50px" Height="50px" OnClientClick='<%# !string.IsNullOrEmpty(Eval("Photo") as string) ? "openPhotoModal(\"" + ResolveUrl(Eval("Photo") as string) + "\"); return false;" : "return false;" %>'
                                    ImageUrl='<%# !string.IsNullOrEmpty(Eval("Photo") as string) ? ResolveUrl(Eval("Photo") as string) : ResolveUrl("~/Images/no-image.png") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>--%>
                            <%--<asp:TemplateField HeaderText="Photo">
                                <ItemTemplate>
                                    <asp:Panel ID="pnlImage" runat="server">
                                        <%# !string.IsNullOrEmpty(Eval("Photo") as string) ? 
                                        "<img src='" + ResolveUrl(Eval("Photo") as string) + "' class='thumbnail rounded' style='width: 50px; height: 50px; cursor: pointer;' onclick='openPhotoModal(\"" + ResolveUrl(Eval("Photo") as string) + "\")' />" : 
                                        "<img src='" + ResolveUrl("~/Images/no-image.png") + "' class='thumbnail rounded' style='width: 50px; height: 50px;' />" 
                                        %>
                                    </asp:Panel>
                                </ItemTemplate>
                            </asp:TemplateField>--%>
                            <asp:BoundField DataField="CreatedDate" HeaderText="Created Date" DataFormatString="{0:dd/MMM/yyyy}" />
                            <asp:BoundField DataField="ModifiedDate" HeaderText="Last Modified" DataFormatString="{0:dd/MMM/yyyy}" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>

    <!-- Photo Modal -->
    <div class="modal fade" id="photoModal" tabindex="-1" role="dialog" aria-labelledby="photoModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-lg">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="photoModalLabel">Photo View</h5>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
                <div class="modal-body text-center">
                    <img id="modalImage" src="" style="max-width: 100%;" />
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal">Close</button>
                </div>
            </div>
        </div>
    </div>
</div>
<script src="https://code.jquery.com/jquery-3.6.0.min.js"></script> <!-- Ensure jQuery is loaded -->
<script>
    $(document).ready(function () {
        // Ensure click event is attached dynamically
        $(document).on("click", ".photo-click", function () {
            var photoUrl = $(this).attr("data-photo");

            if (photoUrl) {
                $("#modalImage").attr("src", photoUrl);
                $("#photoModal").modal("show");
            }
        });
    });
</script>



    <%--<script type="text/javascript">
        function openPhotoModal(photoUrl) {
            document.getElementById('modalImage').src = photoUrl;
            $('#photoModal').modal('show'); // If using Bootstrap modal
        }
    </script>--%>
</asp:Content>
