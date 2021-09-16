using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace sample_keycloak_mvc.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public void Login()
        {
            // redirect page after authenticated
            string authenticatedUri = System.Configuration.ConfigurationManager.AppSettings["authenticatedUri"];

            if (!Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(
                // new AuthenticationProperties { RedirectUri = "/" },
                new AuthenticationProperties { RedirectUri = authenticatedUri },
                OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
        }

        public ActionResult Logout()
        {
            HttpContext.GetOwinContext().Authentication.SignOut(
                OpenIdConnectAuthenticationDefaults.AuthenticationType,
                CookieAuthenticationDefaults.AuthenticationType);

            return RedirectToAction("Index", "Home");
        }
    }
}