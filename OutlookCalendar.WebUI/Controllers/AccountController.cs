using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using OutlookCalendar.WebUI.TokenStorage;
using System.Security.Claims;
using OutlookCalendar.Domain.Abstract;

namespace OutlookCalendar.WebUI.Controllers
{
    public class AccountController : Controller
    {
        private IAppointmentRepository repository;

        public AccountController(IAppointmentRepository appointmentRepository)
        {
            repository = appointmentRepository;
        }

        public void SignIn()
        {
            if (!Request.IsAuthenticated)
            {
                repository.ClearAppointments();
                // Signal OWIN to send an authorization request to Azure.
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties { RedirectUri = "/" },
                    OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
        }

        public void SignOut()
        {
            if (Request.IsAuthenticated)
            {
                repository.ClearAppointments();
                // Get the user's token cache and clear it.
                string userObjectId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;

                SessionTokenCache tokenCache = new SessionTokenCache(userObjectId, HttpContext);
                tokenCache.Clear(userObjectId);
            }

            // Send an OpenID Connect sign-out request. 
            HttpContext.GetOwinContext().Authentication.SignOut(
                CookieAuthenticationDefaults.AuthenticationType);
            Response.Redirect("/");
        }
    }
}