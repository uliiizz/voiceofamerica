<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CreateEventView.aspx.cs" Inherits="TechPractice.CreateEventView" %>

<html xmlns="http:
    
    
    
    
    
    www.w3.org/1999/xhtml">
<head runat="server">
    <title>Create Event</title>
        <link rel="stylesheet" href="./style.css" />
        <script src="Index.js"></script> 
</head>
<body>
    <form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <asp:UpdatePanel ID="updatePanel" runat="server">
        <ContentTemplate>
            <div>
                <h2>Create Event</h2>
            <div>
                <label for="txtName">Name:</label>
                <asp:TextBox ID="txtName" runat="server"></asp:TextBox>
            </div>
            <div>
                <label for="txtDate">Date:</label>
                <asp:TextBox ID="TextBoxDateTime" runat="server" type="datetime-local" OnTextChanged="TextBoxDateTime_TextChanged"></asp:TextBox>            <div>
            <div>
                    <label for="txtTTL">TTL:</label>
                    <asp:TextBox ID="txtTTL" runat="server" AutoPostBack="true" OnTextChanged="txtTTL_TextChanged"></asp:TextBox>
                <div>
                    <label for="ddlLocation">Location:</label>
                    <asp:DropDownList ID="ddlLocation" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlLocation_SelectedIndexChanged"></asp:DropDownList>
                </div>
                            <div>
                <asp:Panel ID="pnlSelectedLocations" runat="server"></asp:Panel>
            </div>
                <div>
                    <asp:Button ID="btnSubmit" runat="server" Text="Submit" OnClick="btnSubmit_Click" />
                </div>
            </div>

        </ContentTemplate>
        </asp:UpdatePanel>
    </form>
</body>
</html>
