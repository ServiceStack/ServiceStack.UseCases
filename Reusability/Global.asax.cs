using System;
using Funq;
using ServiceStack.Redis;
using ServiceStack.WebHost.Endpoints;

namespace Reusability
{
    public class AppHost : AppHostBase
    {
        public AppHost() : base("Reusability demo", typeof(SMessage).Assembly) {}

        public override void Configure(Container container)
        {
            container.Register<IRedisClientsManager>(
                new PooledRedisClientManager("localhost:6379"));


        }
    }

    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            new AppHost().Init();
        }
    }
}