using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;

namespace CustomAuthentication
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnAuth_Click(object sender, EventArgs e)
        {
            var baseUrl = Request.Url.GetLeftPart(UriPartial.Authority) + "/api";
            var client = new JsonServiceClient(baseUrl);
            var authResponse = client.Post<AuthResponse>("/auth", new Auth
                            {
                                UserName = tbUser.Text,
                                Password = tbPassword.Text
                            });
            var response = HttpContext.Current.Response.ToResponse();
            response.Cookies.AddSessionCookie(SessionFeature.SessionId, authResponse.SessionId);

            phAuthenticated.Visible = true;
        }
    }
}