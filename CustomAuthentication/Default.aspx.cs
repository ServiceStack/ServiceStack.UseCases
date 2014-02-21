using System;
using System.Web;
using ServiceStack;
using ServiceStack.Web;

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
            var authResponse = client.Post<AuthenticateResponse>("/auth", new Authenticate
                {
                    UserName = tbUser.Text,
                    Password = tbPassword.Text
                });

            var requestContxt = HttpContext.Current.ToRequest();
            ((IHttpResponse)requestContxt.Response).Cookies.AddSessionCookie(SessionFeature.SessionId, authResponse.SessionId);

            phAuthenticated.Visible = true;
        }
    }
}