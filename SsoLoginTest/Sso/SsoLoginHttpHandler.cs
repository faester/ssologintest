using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.RelyingParty;

namespace SsoLoginTest.Sso
{
    public class SsoLoginHttpHandler : IHttpHandler
    {
        private string _ssoEndpoint;
        private string _branding;

        public SsoLoginHttpHandler()
        {
            _ssoEndpoint = ConfigurationManager.AppSettings["sso.endpoint"];
            _branding = ConfigurationManager.AppSettings["sso.branding"];
            if (string.IsNullOrEmpty(_branding))
            {
                throw new ConfigurationErrorsException("no sso.branding in config. Fix it.");
            }
            if (string.IsNullOrEmpty(_ssoEndpoint))
            {
                throw new ConfigurationErrorsException("no sso.endpoint in config. Fix it.");
            }
        }

        public void HandleLogin(string returnUrl)
        {
            var rp = new OpenIdRelyingParty();
            var response = rp.GetResponse();

            if (response != null)
            {
                FormsAuthentication.SetAuthCookie(response.ClaimedIdentifier, false);
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Cookies["noautologin"] != null 
                && context.Request.Cookies["noautologin"].Value == "done")
            {
                return;
            } else if (context.Request.Cookies["noautologin"] != null)
            {
                CheckLogin();
                context.Response.Cookies.Add(new HttpCookie("noautologin", "done"));
                return;
            }

            var rp = new OpenIdRelyingParty();

            var request = rp.CreateRequest(_ssoEndpoint);

            request.AddExtension(new BrandingExtension() { BrandingIdentifier = _branding });

            request.Mode = AuthenticationRequestMode.Immediate;

            context.Response.Cookies.Add(new HttpCookie("noautologin", "active"));

            request.RedirectToProvider();
        }

        private void CheckLogin()
        {
            var rp = new OpenIdRelyingParty();
            var response = rp.GetResponse();
            if (response == null)
            {
                return;
            }
            if (response.Status == AuthenticationStatus.Authenticated)
            {
                FormsAuthentication.SetAuthCookie(response.ClaimedIdentifier, false);
            }
        }

        public bool IsReusable { get { return true; } }
    }
}