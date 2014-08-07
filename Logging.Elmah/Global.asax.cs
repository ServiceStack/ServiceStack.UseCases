using System;
using Funq;
using ServiceStack;
using ServiceStack.Logging;
using ServiceStack.Logging.Elmah;

namespace Logging.Elmah
{
    public class AppHost : AppHostBase
    {
        public AppHost()
            : base("Logging.Elmah", typeof (MyServices).Assembly) {}

        public override void Configure(Container container)
        {
        }
    }

    [Route("/logerror/{Text}")]
    public class LogError
    {
        public string Text { get; set; }
    }

    public class MyServices : Service
    {
        public object Any(LogError request)
        {
            throw new Exception(request.Text);
        }
    }

    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            //View Elmah Error Log at: /elmah.axd
            var debugMessagesLog = new ConsoleLogFactory();
            LogManager.LogFactory = new ElmahLogFactory(debugMessagesLog, this);
            new AppHost().Init();
        }
    }
}