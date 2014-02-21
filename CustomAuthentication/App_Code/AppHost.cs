using System.Collections.Generic;
using Funq;
using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.Configuration;

namespace CustomAuthentication.App_Code
{
    public class AppHost : AppHostBase
    {
        public AppHost() : base("Custom Authentication Example", typeof(AppHost).Assembly) { }

        public override void Configure(Container container)
        {
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