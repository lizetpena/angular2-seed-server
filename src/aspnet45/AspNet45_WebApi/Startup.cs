using Microsoft.Owin;
using Microsoft.Owin.Security.ActiveDirectory;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(AspNet45_WebApi.Startup))]
namespace AspNet45_WebApi
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseWindowsAzureActiveDirectoryBearerAuthentication(
                new WindowsAzureActiveDirectoryBearerAuthenticationOptions
                {
                    Tenant = ConfigurationManager.AppSettings["ida:Tenant"],
                    TokenValidationParameters = new TokenValidationParameters
                    {
                        // Load all acceptable audiences
                        ValidAudiences = new List<string>
                        {
                            ConfigurationManager.AppSettings["ida:ClientID"]
                        },
                    },
                    // Adds WWW-Authenticate to auth endpoint in response headers.
                    Provider = new OAuthBearerAuthenticationProvider
                    {
                        OnApplyChallenge = (context) =>
                        {
                            context.OwinContext.Response.Headers.AppendValues(
                           "WWW-Authenticate",
                          $"Bearer authorization_uri=\"{ConfigurationManager.AppSettings["ida:AuthorizationUri"]}\", {context.Challenge}");
                            return Task.FromResult(0);
                        },
                        OnValidateIdentity = (OAuthValidateIdentityContext context) =>
                        {
                            return Task.FromResult(0);
                        },
                        OnRequestToken = (OAuthRequestTokenContext context) =>
                        {
                            return Task.FromResult(0);
                        }
                    },
                });
        }
    }
}
