<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="TechPractice.WebForm1" %>

    <%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>  

<asp:Content ContentPlaceHolderId="CPH1" runat="server">
        <div>
                    <div class="home-container2">
            <div id="chartdiv"></div>
            <div class="container-container1">
                <ul class="container-ul list" runat="server" id="container">
             
                </ul>
            <asp:Label ID="registerPrompt" runat="server" text="Please sign-in or register to view and create events."/>
            <asp:Button ID="btnCreateEvent" runat="server" CssClass="container-button button" Text="Create Event"/>
            <asp:Button ID="Button3" runat="server" CssClass="container-button button" Text="View Random Event" OnClick="rnd_Click"/>
            <asp:Button ID="Button1" runat="server" CssClass="container-button button" Text="View Random Expiring Event" OnClick="rndExpClick"/>
            <asp:Label ID="totalEventCount" runat="server" />
            <asp:Label ID="validEventCount" runat="server" />
            <asp:Label ID="hourEventCount" runat="server" />
            </div>               
        </div>
        </div>
    <cc1:ModalPopupExtender ID="mp1" runat="server" PopupControlID="Panl1" TargetControlID="btnCreateEvent"  
    CancelControlID="Button2" BackgroundCssClass="Background">  
</cc1:ModalPopupExtender>  
<asp:Panel ID="Panl1" runat="server" CssClass="Popup" align="center" style = "display:none">  
    <iframe style=" width: 350px; height: 300px;" id="irm1" src="CreateEventView.aspx" runat="server"></iframe>  
   <br/>  
<asp:Button ID="Button2" runat="server" Text="Close" OnClick="btnClose_Click"/>

</asp:Panel> 
</asp:Content>
