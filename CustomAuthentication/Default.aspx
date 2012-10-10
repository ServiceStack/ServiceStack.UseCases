<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="CustomAuthentication._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.7.1/jquery.min.js"></script>
</head>
<body>
    <div Visible="false" id="phAuthenticated" runat="server" enableviewstate=false style="color:green;">Authenticated.</div>
    <form id="form1" runat="server">
    <div>
        User: <asp:TextBox runat="server" ID="tbUser" CssClass="userName" Text="admin" /> (or "user") <br />
        Password: <asp:TextBox runat="server" ID="tbPassword" CssClass="password" Text="123" /> <br />

        <asp:Button runat="server" OnClick="btnAuth_Click" Text="ASP.NET Form Login" /> 
    </div>
    </form>
        <button onclick="ajaxLogin()">Ajax Login</button> 

    <div class="protectedServices">
        <button onclick="helloService()">Invoke Hello Service</button> 
    </div>

    <script>
        function ajaxLogin() {
            InvokeService('<%=ResolveUrl("~/api/auth")%>',
                { userName: userName = $('.userName').val(), password: $('.password').val() },
                function (data, textStatus, jqXHR) {
                    alert('Authenticated! Now you can run Hello Service.');
                }
            );

            return false;
        }

        function helloService() {
            InvokeService('<%=ResolveUrl("~/api/hello")%>',
                { name: userName = $('.userName').val() },
                function (data, textStatus, jqXHR) {
                    alert(data.Result);
                }
            );
        }

        function InvokeService(url, data, success) {
            $.ajax({
                type: "GET",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: data,
                url: url,
                success: success,
                error: function (xhr, textStatus, errorThrown) {
                    var data = $.parseJSON(xhr.responseText);
                    if (data === null)
                        alert(textStatus + " HttpCode:" + xhr.status);
                    else
                        alert("ERROR: " + data.ResponseStatus.Message + (data.ResponseStatus.StackTrace ? " \r\n Stack:" + data.ResponseStatus.StackTrace : ""));
                }
            });
        }
    </script>
</body>
</html>
