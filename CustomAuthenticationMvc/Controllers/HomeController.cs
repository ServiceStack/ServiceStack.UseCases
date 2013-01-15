using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CustomAuthenticationMvc.App_Start;
using ServiceStack.CacheAccess;
using ServiceStack.Common.ServiceClient.Web;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.WebHost.Endpoints;

namespace CustomAuthenticationMvc.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult RunHelloService()
        {
            var helloService = AppHostBase.Resolve<HelloService>();
            helloService.RequestContext = System.Web.HttpContext.Current.ToRequestContext();
            var response = (HelloResponse)helloService.Any(new HelloRequest { Name = User.Identity.Name });
            
            ViewBag.Response = response.Result;
            ViewBag.Counter = ServiceStackSession.Get<int>(HelloService.HelloServiceCounterKey);
            return View("Index");
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
