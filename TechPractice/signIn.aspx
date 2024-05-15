<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="signIn.aspx.cs" Inherits="TechPractice.signIn" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
                <link rel="stylesheet" type="text/css"  href="style.css">

                <asp:Button ID="Button1" runat="server" Text="ReturnToFirstPage" onClick="ReturnToFirstPage" class="home-login button"/></br>
                     <section class="registrBlock ">
                <asp:Label ID="errorTxt" runat="server" Text=""></asp:Label></br>
                <asp:Label ID="l_login" runat="server">username</asp:Label></br>
                <asp:TextBox ID="tb_login" runat="server"></asp:TextBox></br>

                <asp:Label ID="l_password" runat="server">password</asp:Label></br>
                <asp:TextBox ID="tb_password" textMode="Password" runat="server"></asp:TextBox>
                <button type="button" onclick="togglePassword('tb_password')" class="regButton">show password</button></br>
                <script>
                    function togglePassword(textBoxID) {
                        var passwordField = document.getElementById(textBoxID);

                        if (passwordField.type === "password") {
                            passwordField.type = "text";
                        } else {
                            passwordField.type = "password";
                        }
                    }
                </script>
                <asp:Button ID="b_signIn" runat="server" Text="Sign In" OnClick="btnLogin_Click" class="regButton"/>
                <asp:Button ID="b_google" runat="server" Text="GOOGLE" onClick="b_googleClick" class="regButton"/>
                <asp:Button ID="b_facebook" runat="server" Text="FACEBOOK" onClick=" b_FacebookClick" class="regButton"/></br>
                         </section>
        </div>
    </form>
</body>
</html>
