using Microsoft.Identity.Client;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using OutlookCalendar.WebUI.TokenStorage;
using System;
using System.Configuration;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace OutlookCalendar.WebUI.Helpers
{
    public sealed class AuthProvider : IAuthProvider
    {
        private string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];
        private string appId = ConfigurationManager.AppSettings["ida:AppId"];
        private string appSecret = ConfigurationManager.AppSettings["ida:AppSecret"];
        private string scopes = ConfigurationManager.AppSettings["ida:GraphScopes"];
        private SessionTokenCache tokenCache { get; set; }

        private static readonly AuthProvider instance = new AuthProvider();
        private AuthProvider() { }

        public static AuthProvider Instance()
        {
            return instance;
        }

        public async Task<string> GetUserAccessTokenAsync()
        {
            string signedInUserID = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            tokenCache = new SessionTokenCache(
                signedInUserID,
                HttpContext.Current.GetOwinContext().Environment["System.Web.HttpContextBase"] as HttpContextBase);

            ConfidentialClientApplication cca = new ConfidentialClientApplication(
                appId,
                redirectUri,
                new ClientCredential(appSecret),
                tokenCache);

            try
            {
                AuthenticationResult result = await cca.AcquireTokenSilentAsync(scopes.Split(new char[] { ' ' }));
                return result.Token;
            }

            // Unable to retrieve the access token silently.
            catch (MsalSilentTokenAcquisitionException)
            {
                HttpContext.Current.Request.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties() { RedirectUri = "/" },
                    OpenIdConnectAuthenticationDefaults.AuthenticationType);

                throw new Exception("Caller needs to authenticate.");
            }
        }
    }
}