using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace CustomAuthenticationMvc
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // we are going to use ServiceStack for /api/*

            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);
        }
    }
}
