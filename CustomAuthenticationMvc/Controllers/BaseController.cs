using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CustomAuthenticationMvc.App_Start;
using ServiceStack.CacheAccess;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
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
                return _serviceStackSession ?? (_serviceStackSession = AppHostBase.Resolve<ISessionFactory>().GetOrCreateSession());
            }
        }
    }
}
