using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CustomAuthenticationMvc.App_Start;
using ServiceStack.CacheAccess;
using ServiceStack.ServiceClient.Web;
using ServiceStack.WebHost.Endpoints;

namespace CustomAuthenticationMvc.Controllers
{
    public class BaseController : Controller
    {
        /// <summary>
        /// ServiceStack Session Bag
        /// </summary>
        private ISession _serviceStackSession;
        protected ISession ServiceStackSession
        {
            get
            {
                return _serviceStackSession ?? (_serviceStackSession = AppHostBase.Instance.Container.Resolve<ISessionFactory>().GetOrCreateSession());
            }
        }

        protected JsonServiceClient CreateJsonServiceClient()
        {
            var baseUrl = Request.Url.GetLeftPart(UriPartial.Authority) + "/api";
            var client = new JsonServiceClient(baseUrl);
            System.Web.HttpContext.Current.Request.ToCookiesContainer(client.CookieContainer);
            return client;
        }
    }
}
