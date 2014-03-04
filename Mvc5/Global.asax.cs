using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

using ServiceStack;
using ServiceStack.Text;
using ServiceStack.Mvc;

namespace Mvc5
{
    public class AppHost : AppHostBase
    {
        public AppHost() : base("MVC5", typeof(HelloServices).Assembly) { }

        public override void Configure(Funq.Container container)
        {
            SetConfig(new HostConfig { HandlerFactoryPath = "api" });

            //Set MVC to use the same Funq IOC as ServiceStack
            ControllerBuilder.Current.SetControllerFactory(new FunqControllerFactory(container));
        }
    }

    [ServiceStack.Route("/hello")]
    public class Hello 
    {
        public string Name { get; set; }
    }
    public class HelloResponse
    {
        public string Result { get; set; }
    }

    public class HelloServices : Service
    {
        public object Any(Hello request)
        {
            return new HelloResponse { Result = "Hello, {0}!".Fmt(request.Name) };
        }
    }

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            new AppHost().Init();
        }
    }
}
