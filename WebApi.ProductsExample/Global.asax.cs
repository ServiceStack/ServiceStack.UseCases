using System;
using Funq;
using ServiceStack;
using ServiceStack.MiniProfiler;

namespace WebApi.ProductsExample
{
    //Product is used by both ServiceStack and WebApi Service Examples
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
    }

    public class AppHost : AppHostBase
    {
        public AppHost() : base("ServiceStack Messages vs WebApi RPC Demo", typeof(AppHost).Assembly) { }
        public override void Configure(Container container) { }
    }

    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            new AppHost().Init();
        }

        protected void Application_BeginRequest(object src, EventArgs e)
        {
            if (Request.IsLocal)
                Profiler.Start();
        }

        protected void Application_EndRequest(object src, EventArgs e)
        {
            Profiler.Stop();
        }
    }
}