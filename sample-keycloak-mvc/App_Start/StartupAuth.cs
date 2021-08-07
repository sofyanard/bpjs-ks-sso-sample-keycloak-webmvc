using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security.Notifications;
using Owin;
using System;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(sample_keycloak_mvc.App_Start.StartupAuth))]

namespace sample_keycloak_mvc.App_Start
{
    public class StartupAuth
    {
        // The Client ID is used by the application to uniquely identify itself to Microsoft identity platform.
        string _clientId = System.Configuration.ConfigurationManager.AppSettings["clientId"];

        string _clientSecret = System.Configuration.ConfigurationManager.AppSettings["clientSecret"];

        // RedirectUri is the URL where the user will be redirected to after they sign in.
        string _redirectUri = System.Configuration.ConfigurationManager.AppSettings["redirectUri"];

        // Tenant is the tenant ID (e.g. contoso.onmicrosoft.com, or 'common' for multi-tenant)
        static string _tenant = System.Configuration.ConfigurationManager.AppSettings["tenant"];

        // Authority is the URL for authority, composed of the Microsoft identity platform and the tenant name (e.g. https://login.microsoftonline.com/contoso.onmicrosoft.com/v2.0)
        static string _authority = String.Format(System.Globalization.CultureInfo.InvariantCulture, System.Configuration.ConfigurationManager.AppSettings["authority"], _tenant);

        static string _metadataAddress = _authority + "/.well-known/openid-configuration";

        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888

            // https://developer.okta.com/blog/2018/08/29/secure-webforms-with-openidconnect-okta
            // https://docs.microsoft.com/en-us/azure/active-directory/develop/tutorial-v2-asp-webapp

            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                // Sets the ClientId, authority, RedirectUri as obtained from web.config
                ClientId = _clientId,
                ClientSecret = _clientSecret,
                Authority = _authority,
                RedirectUri = _redirectUri,
                // PostLogoutRedirectUri is the page that users will be redirected to after sign-out. In this case, it is using the home page
                PostLogoutRedirectUri = _redirectUri,
                Scope = OpenIdConnectScope.OpenIdProfile,
                // ResponseType is set to request the code id_token - which contains basic information about the signed-in user
                ResponseType = OpenIdConnectResponseType.CodeIdToken,
                // ValidateIssuer set to false to allow personal and work accounts from any organization to sign in to your application
                // To only allow users from a single organizations, set ValidateIssuer to true and 'tenant' setting in web.config to the tenant name
                // To allow users from only a list of specific organizations, set ValidateIssuer to true and use ValidIssuers parameter
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false // This is a simplification
                    // NameClaimType = "name"
                },
                // OpenIdConnectAuthenticationNotifications configures OWIN to send notification of failed authentications to OnAuthenticationFailed method
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    /*
                    AuthorizationCodeReceived = async n =>
                    {
                        // Exchange code for access and ID tokens
                        var tokenClient = new TokenClient($"{_authority}/token", _clientId, _clientSecret);

                        var tokenResponse = await tokenClient.RequestAuthorizationCodeAsync(n.Code, _redirectUri);
                        if (tokenResponse.IsError)
                        {
                            throw new Exception(tokenResponse.Error);
                        }

                        var userInfoClient = new UserInfoClient($"{_authority}/userinfo");
                        var userInfoResponse = await userInfoClient.GetAsync(tokenResponse.AccessToken);

                        var claims = new List<Claim>(userInfoResponse.Claims)
                        {
                            new Claim("id_token", tokenResponse.IdentityToken),
                            new Claim("access_token", tokenResponse.AccessToken)
                        };

                        n.AuthenticationTicket.Identity.AddClaims(claims);
                    },
                    */

                    AuthenticationFailed = OnAuthenticationFailed
                },
                // Disable Https for Development
                RequireHttpsMetadata = false,
                MetadataAddress = _metadataAddress
            });
        }

        // Handle failed authentication requests by redirecting the user to the home page with an error in the query string
        private Task OnAuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> context)
        {
            context.HandleResponse();
            context.Response.Redirect("/?errormessage=" + context.Exception.Message);
            return Task.FromResult(0);
        }
    }
}
