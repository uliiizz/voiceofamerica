<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DetailedView.aspx.cs" Inherits="TechPractice.DetailedView" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link rel="stylesheet" href="./style.css" />
    <script src="Index.js"></script> 
    <style>
        wrapper,
.wrapper * {
    display: flex;
}

.wrapper {
    padding: 15px;
    flex-wrap: wrap;
}

.card {
    width: 220px;
    flex-direction: column;
    background: #fff;
    border-radius: 8px;
    box-shadow: 0 0 10px 5px rgba(128, 128, 128, .3);
    padding: 25px;
    position: relative;
    align-items: center;
    margin: 0 10px 20px;
    transform: rotate(0deg);
    transform-origin: 50% 50%;
    transition: all .25s;
}

    .card:hover {
        transform: rotate(5deg);
    }

.person_name {
    font-size: 15px;
    font-weight: 600;
    color: #444;
    letter-spacing: 1px;
    margin-bottom: 7px;
}

.person_desg {
    font-size: 11px;
    color: #ee3ab7;
    margin-bottom: 30px;
}

.hire_btn {
    width: 80px;
    height: 45px;
    background: #ee3ab7;
    border: 2px solid #ee3ab7;
    outline: 0;
    color: #fff;
    justify-content: center;
    text-transform: uppercase;
    font-size: 10px;
    font-weight: 700;
    border-radius: 50px;
    letter-spacing: 1px;
    text-shadow: 1px 1px 1px rgba(0,0,0,.3);
    box-shadow: 0 0 5px 5px rgba(238,58,183,.2);
    cursor: pointer;
    transition: all .25s;
}

    .hire_btn:hover {
        background: transparent;
        color: #ee3ab7;
        box-shadow: none;
    }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Panel ID="PanelEventDetails" runat="server" Enabled="false">
                    <div>
                        <h1>Event Details</h1>
                        <ul>
                            <li>Name: <asp:TextBox ID="txtName" runat="server" ReadOnly="true" /></li>
                            <li>TTL: <asp:TextBox ID="txtTTL" runat="server" ReadOnly="true" /></li>
                            <li>Time Zones: 
                                <asp:DropDownList ID="ddlTimeZones" runat="server" OnSelectedIndexChanged="ddlTimeZones_SelectedIndexChanged"></asp:DropDownList>
                            </li>
                            <li>Date and Time: <input type="datetime-local" id="txtDateTime" runat="server" disabled="true"/></li>
                            <li>Public Link: <asp:Label ID="LinkLabel" runat="server" Text="Copy Link"/></li>
                        </ul>
                    </div>

                </asp:Panel>
                                <asp:Panel ID="pnlSelectedLocations" runat="server"></asp:Panel>

                <asp:Button ID="btnToggleEdit" runat="server" Text="Edit" OnClick="btnToggleEdit_Click" />
                <asp:Button ID="btnSave" runat="server" Text="Save" OnClick="btnSave_Click" Visible="false"/>
                <asp:Panel ID="PanelRelatedLocations" runat="server">
                <div>
                    <br />
                    <asp:Label ID="UsersLocalEventTimeLabel" runat="server"/>
                    <br />
                <h1>Related Locations</h1>
                <ul>
                    <li>Related Locations:
                        <asp:DropDownList ID="ddlRelatedLocations" runat="server" OnSelectedIndexChanged="ddlRelatedLocations_SelectedIndexChanged" AutoPostBack="True"></asp:DropDownList>
                    </li>
                </ul>
                <asp:Panel ID="RelatedLocationsButtonsPanel" runat="server" CssClass="wrapper">  

                </asp:Panel>
                </div>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
    </form>
</body>
</html>
