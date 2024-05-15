<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="register.aspx.cs" Inherits="TechPractice.register" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>REGISTRATION</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
             <link rel="stylesheet" type="text/css"  href="style.css">
            <asp:Button ID="Button1" runat="server" Text="ReturnToFirstPage" onClick="ReturnToFirstPage" class="home-login button"/></br>
                    <section class="registrBlock ">

                <asp:Label ID="errorTxt" runat="server" Text=""></asp:Label></br>

                <asp:Label ID="l_login" runat="server" Text="username"></asp:Label></br>
                <asp:TextBox ID="tb_login" runat="server"></asp:TextBox></br>

                <asp:Label ID="l_password" runat="server" Text="password"></asp:Label></br>
                <asp:TextBox ID="tb_password" TextMode="Password" runat="server"></asp:TextBox>
                <button type="button" onclick="togglePassword('tb_password')" class="regButton">show password</button></br>

                <asp:Label ID="l_confirmPass" runat="server" Text="confirm password"></asp:Label></br>
                <asp:TextBox ID="tb_confirmPass"  TextMode="Password" runat="server"></asp:TextBox>
                <button type="button" onclick="togglePassword('tb_confirmPass')" class="regButton">show password</button></br>

                <asp:Button ID="b_register" runat="server" Text="submit" OnClick="registerUser" class="regButton"/></br>


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
                    </section>
        </div>
    </form>
</body>
</html>
