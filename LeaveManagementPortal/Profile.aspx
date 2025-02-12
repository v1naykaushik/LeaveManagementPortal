<%@ Page Title="My Profile" Language="C#" MasterPageFile="~/LeaveManagementPortalMaster.Master" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="LeaveManagementPortal.Profile" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .profile-container {
            max-width: 800px;
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

        .profile-section {
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

        .info-row {
            display: flex;
            margin-bottom: 1rem;
        }

        .info-label {
            width: 150px;
            color: #666;
            font-weight: 500;
        }

        .info-value {
            flex: 1;
            color: #333;
        }

        .password-section {
            margin-top: 2rem;
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

        .btn-update {
            background-color: #1a237e;
            color: white;
            padding: 0.5rem 1.5rem;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            transition: background-color 0.3s;
        }

        .btn-update:hover {
            background-color: #151c5e;
        }

        .password-requirements {
            font-size: 0.875rem;
            color: #666;
            margin-top: 1rem;
            padding: 1rem;
            background-color: #fff;
            border-radius: 4px;
            border: 1px solid #e2e8f0;
        }

        .requirement-list {
            list-style-type: none;
            padding: 0;
            margin: 0.5rem 0 0 0;
        }

        .requirement-list li {
            margin-bottom: 0.25rem;
            padding-left: 1.5rem;
            position: relative;
        }

        .requirement-list li::before {
            content: "•";
            position: absolute;
            left: 0.5rem;
            color: #1a237e;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <div class="profile-container">
            <h2 class="page-title">My Profile</h2>

            <!-- Personal Information Section -->
            <div class="profile-section">
                <h3 class="section-title">Personal Information</h3>
                <div class="info-row">
                    <div class="info-label">Name:</div>
                    <div class="info-value">
                        <asp:Label ID="lblName" runat="server"></asp:Label>
                    </div>
                </div>
                <div class="info-row">
                    <div class="info-label">Email:</div>
                    <div class="info-value">
                        <asp:Label ID="lblEmail" runat="server"></asp:Label>
                    </div>
                </div>
                <div class="info-row">
                    <div class="info-label">Employee ID:</div>
                    <div class="info-value">
                        <asp:Label ID="lblEmployeeId" runat="server"></asp:Label>
                    </div>
                </div>
                <div class="info-row">
                    <div class="info-label">Role:</div>
                    <div class="info-value">
                        <asp:Label ID="lblRole" runat="server"></asp:Label>
                    </div>
                </div>
                <div class="info-row">
                    <div class="info-label">Reporting Manager:</div>
                    <div class="info-value">
                        <asp:Label ID="lblManager" runat="server"></asp:Label>
                    </div>
                </div>
            </div>

            <!-- Change Password Section -->
            <div class="profile-section">
                <h3 class="section-title">Change Password</h3>
                <div class="form-group mb-3">
                    <label for="txtCurrentPassword" class="form-label">Current Password</label>
                    <asp:TextBox ID="txtCurrentPassword" runat="server" CssClass="form-control" 
                        TextMode="Password" />
                    <asp:RequiredFieldValidator ID="rfvCurrentPassword" runat="server" 
                        ControlToValidate="txtCurrentPassword"
                        CssClass="validation-error"
                        Display="Dynamic"
                        ErrorMessage="Current password is required." />
                </div>

                <div class="form-group mb-3">
                    <label for="txtNewPassword" class="form-label">New Password</label>
                    <asp:TextBox ID="txtNewPassword" runat="server" CssClass="form-control" 
                        TextMode="Password" />
                    <asp:RequiredFieldValidator ID="rfvNewPassword" runat="server" 
                        ControlToValidate="txtNewPassword"
                        CssClass="validation-error"
                        Display="Dynamic"
                        ErrorMessage="New password is required." />
                    <asp:RegularExpressionValidator ID="revNewPassword" runat="server"
                        ControlToValidate="txtNewPassword"
                        CssClass="validation-error"
                        Display="Dynamic"
                        ValidationExpression="^(?=.*[A-Za-z])(?=.*\d)(?=.*[@#$%&])[A-Za-z\d@#$%&]{6,10}$"
                        ErrorMessage="Password does not meet the requirements." />
                </div>

                <div class="form-group mb-4">
                    <label for="txtConfirmPassword" class="form-label">Confirm New Password</label>
                    <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="form-control" 
                        TextMode="Password" />
                    <asp:RequiredFieldValidator ID="rfvConfirmPassword" runat="server" 
                        ControlToValidate="txtConfirmPassword"
                        CssClass="validation-error"
                        Display="Dynamic"
                        ErrorMessage="Please confirm your new password." />
                    <asp:CompareValidator ID="cvConfirmPassword" runat="server" 
                        ControlToCompare="txtNewPassword"
                        ControlToValidate="txtConfirmPassword"
                        CssClass="validation-error"
                        Display="Dynamic"
                        ErrorMessage="The passwords do not match." />
                </div>

                <div class="password-requirements">
                    <strong>Password Requirements:</strong>
                    <ul class="requirement-list">
                        <li>6 to 10 characters in length</li>
                        <li>At least one letter</li>
                        <li>At least one number</li>
                        <li>At least one special character (@#$%&)</li>
                    </ul>
                </div>

                <div class="mt-4">
                    <asp:Button ID="btnChangePassword" runat="server" Text="Update Password" 
                        CssClass="btn-update"
                        OnClick="btnChangePassword_Click" />
                </div>

                <asp:Label ID="lblMessage" runat="server" CssClass="mt-3 d-block"></asp:Label>
            </div>
        </div>
    </div>
</asp:Content>