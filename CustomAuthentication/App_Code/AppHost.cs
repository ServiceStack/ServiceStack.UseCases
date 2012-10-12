using System;
using System.Collections.Generic;
using System.Net;
using Funq;
using ServiceStack.CacheAccess;
using ServiceStack.CacheAccess.Providers;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.WebHost.Endpoints;

namespace CustomAuthentication.App_Code
{
    public class AppHost : AppHostBase
    {
        public AppHost() : base("Custom Authentication Example", typeof(AppHost).Assembly) { }

        public override void Configure(Container container)
        {
            // register storage for user sessions 
            container.Register<ICacheClient>(new MemoryCacheClient());

            // Register AuthFeature with custom user session and custom auth provider
            Plugins.Add(new AuthFeature(
                () => new CustomUserSession(), 
                new[] { new CustomCredentialsAuthProvider() }
            ));
        }
    }

    public class CustomUserSession : AuthUserSession
    {
        public string CompanyName { get; set; }
    }

    public class CustomCredentialsAuthProvider : CredentialsAuthProvider
    {
        public override bool TryAuthenticate(IServiceBase authService, string userName, string password)
        {
            if (!CheckInDB(userName, password)) return false;

            var session = (CustomUserSession)authService.GetSession(false);
            session.CompanyName = "Company from DB";
            session.UserAuthId = userName;
            session.IsAuthenticated = true;

            // add roles 
            session.Roles = new List<string>();
            if (session.UserAuthId == "admin") session.Roles.Add(RoleNames.Admin);
            session.Roles.Add("User");

            return true;
        }

        private bool CheckInDB(string userName, string password)
        {
            if (userName != "admin" && userName != "user") return false;
            return password == "123";
        }
    }

    [Authenticate]
    [Route("/hello")]
    public class HelloRequest : IReturn<HelloResponse>
    {
        public string Name { get; set; }
    }

    public class HelloResponse
    {
        public string Result { get; set; }
    }

    public class HelloService : Service
    {
        public object Any(HelloRequest request)
        {
            var userSession = SessionAs<CustomUserSession>();
            var roles = string.Join(", ", userSession.Roles.ToArray());
            return new HelloResponse { Result = "Hello, " + request.Name + ", your role(s): " + roles};
        }
    }


}