using System.Web.Mvc;
using CustomAuthenticationMvc.App_Start;
using ServiceStack;

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
            using (var helloService = HostContext.ResolveService<HelloService>(base.HttpContext))
            {
                var response = (HelloResponse)helloService.Any(new HelloRequest { Name = User.Identity.Name });

                ViewBag.Response = response.Result;
                ViewBag.Counter = ServiceStackSession.Get<int>(HelloService.HelloServiceCounterKey);
                return View("Index");
            }
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
