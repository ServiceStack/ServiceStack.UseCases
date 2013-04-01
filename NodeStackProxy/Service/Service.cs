using System;
using System.ServiceProcess;
using ServiceStack.Configuration;
using Backbone.Todos;

namespace Service
{
    partial class Service : ServiceBase
    {
        private ToDoAppHost appHost;

        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            appHost = new ToDoAppHost();
            appHost.Init();

            var appSettings = new AppSettings();
            var url = appSettings.GetString("Api.Url");
            Console.WriteLine("Listening: " + url);

            appHost.Start(url);
        }

        protected override void OnStop()
        {
            Console.WriteLine("Stopping...");
            appHost.Stop();
            appHost.Dispose();
            Console.WriteLine("Stopped");
        }

        public static void Main(string[] args)
        {
            var service = new Service();
#if !__MonoCS__
			if (Environment.UserInteractive)
#else
			if (AppDomain.CurrentDomain.FriendlyName != "service")
#endif
			{
                service.OnStart(args);
                Console.WriteLine("Press any key to stop program");
                Console.Read();
                service.OnStop();
            }
            else
                Run(service);
        }
    }
}
