using System.Web.Mvc;
using ServiceStack;
using ServiceStack.Caching;

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
                return _serviceStackSession ?? (_serviceStackSession = HostContext.Resolve<ISessionFactory>().GetOrCreateSession());
            }
        }
    }
}
