using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CustomAuthenticationMvc.Helper;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;

[assembly: PreApplicationStartMethod(typeof(SuppressFormsAuthenticationRedirectModule), "Register")]

namespace CustomAuthenticationMvc.Helper
{
    /// <summary>
    /// Prevents Forms Authentication Login Page Redirect for AJAX requests:
    /// http://haacked.com/archive/2011/10/04/prevent-forms-authentication-login-page-redirect-when-you-donrsquot-want.aspx
    /// </summary>
    public class SuppressFormsAuthenticationRedirectModule : IHttpModule
    {
        private static readonly object SuppressAuthenticationKey = new Object();

        public static void SuppressAuthenticationRedirect(HttpContext context)
        {
            context.Items[SuppressAuthenticationKey] = true;
        }

        public static void SuppressAuthenticationRedirect(HttpContextBase context)
        {
            context.Items[SuppressAuthenticationKey] = true;
        }

        public void Init(HttpApplication context)
        {
            context.PostReleaseRequestState += OnPostReleaseRequestState;
            context.EndRequest += OnEndRequest;
        }

        private void OnPostReleaseRequestState(object source, EventArgs args)
        {
            var context = (HttpApplication)source;
            var response = context.Response;
            var request = context.Request;

            if (response.StatusCode == 401 && (request.Headers["X-Requested-With"] == "XMLHttpRequest" 
                || (request.AcceptTypes != null && request.AcceptTypes.Any(t => string.Compare(t, "application/json", true) == 0)))
            )
            {
                SuppressAuthenticationRedirect(context.Context);
            }
        }

        private void OnEndRequest(object source, EventArgs args)
        {
            var context = (HttpApplication)source;
            var response = context.Response;

            if (context.Context.Items.Contains(SuppressAuthenticationKey))
            {
                response.TrySkipIisCustomErrors = true;
                response.ClearContent();
                response.StatusCode = 401;
                response.RedirectLocation = null;
            }
        }

        public void Dispose()
        {
        }

        public static void Register()
        {
            DynamicModuleUtility.RegisterModule(
              typeof(SuppressFormsAuthenticationRedirectModule));
        }
    }
}