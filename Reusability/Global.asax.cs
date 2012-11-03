using System;
using Funq;
using ServiceStack.Common.Utils;
using ServiceStack.Configuration;
using ServiceStack.DataAnnotations;
using ServiceStack.Messaging;
using ServiceStack.MiniProfiler;
using ServiceStack.MiniProfiler.Data;
using ServiceStack.OrmLite;
using ServiceStack.Razor;
using ServiceStack.Redis;
using ServiceStack.Redis.Messaging;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Admin;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.WebHost.Endpoints;

namespace Reusability
{
    public class CustomSession : AuthUserSession {}

    public class EmailRegistration
    {
        [PrimaryKey]
        public string Email { get; set; }
    }

    public class AppHost : AppHostBase
    {
        public AppHost() : base("Reusability demo", typeof(SMessage).Assembly) {}

        public override void Configure(Container container)
        {
            container.RegisterAutoWired<EmailProvider>();
            container.RegisterAutoWired<FacebookGateway>();
            container.RegisterAutoWired<TwitterGateway>();
            
            Plugins.Add(new RazorFormat());
            Plugins.Add(new RequestLogsFeature());

            var appSettings = new AppSettings();
            Plugins.Add(new AuthFeature(() => new CustomSession(), 
                new IAuthProvider[] {
                    new CredentialsAuthProvider(appSettings), 
                    new TwitterAuthProvider(appSettings),
                    new FacebookAuthProvider(appSettings), 
                }));

            container.Register<IRedisClientsManager>(new PooledRedisClientManager("localhost:6379"));
            container.Register(c => c.Resolve<IRedisClientsManager>().GetCacheClient());

            container.Register<IDbConnectionFactory>(
                new OrmLiteConnectionFactory("~/App_Data/db.sqlite".MapHostAbsolutePath(), 
                    SqliteDialect.Provider) {
                        ConnectionFilter = x => new ProfiledDbConnection(x, Profiler.Current)
                    });

            //Store User Data into above OrmLite database
            container.Register<IUserAuthRepository>(c => 
                new OrmLiteAuthRepository(c.Resolve<IDbConnectionFactory>()));

            //If using and RDBMS to persist UserAuth, we must create required tables
            var authRepo = (OrmLiteAuthRepository)container.Resolve<IUserAuthRepository>();
            authRepo.CreateMissingTables();

            //Register MQ Service
            var mqService = new RedisMqServer(container.Resolve<IRedisClientsManager>());
            container.Register<IMessageService>(mqService);
            container.Register(mqService.MessageFactory);

            mqService.RegisterHandler<SMessage>(ServiceController.ExecuteMessage);
            mqService.RegisterHandler<CallFacebook>(ServiceController.ExecuteMessage);
            mqService.RegisterHandler<EmailMessage>(ServiceController.ExecuteMessage);
            mqService.RegisterHandler<PostStatusTwitter>(ServiceController.ExecuteMessage);

            mqService.Start();

            if (appSettings.Get("ResetAllOnStartUp", false))
            {
                ResetAll(container, authRepo);
            }
        }

        private static void ResetAll(Container container, OrmLiteAuthRepository authRepo)
        {
            authRepo.DropAndReCreateTables();
            container.Resolve<IDbConnectionFactory>().Run(db => {
                db.DropAndCreateTable<EmailRegistration>();
                db.DropAndCreateTable<SMessageReceipt>();
            });
            container.Resolve<IRedisClientsManager>().Exec(r => r.FlushAll());
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