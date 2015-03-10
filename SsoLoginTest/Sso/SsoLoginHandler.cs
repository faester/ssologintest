using System;
using System.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.RelyingParty;

namespace SsoLoginTest.Sso
{
    class SsoLoginHandler
    {
        private readonly string _ssoEndpoint;
        private readonly string _branding;

        public SsoLoginHandler()
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

        public ActionResult HandleLogin(string returnUrl)
        {
            var rp = new OpenIdRelyingParty();
            var response = rp.GetResponse();

            if (response != null)
            {
                return HandleLogin(response);
            }
            else
            {
                var request = rp.CreateRequest(_ssoEndpoint);

                request.AddExtension(new BrandingExtension() { BrandingIdentifier = _branding });

                request.Mode = AuthenticationRequestMode.Setup;

                return request.RedirectingResponse.AsActionResult();
            }
        }

        private ActionResult HandleLogin(IAuthenticationResponse response)
        {
            if (response.Status == AuthenticationStatus.Authenticated)
            {
                FormsAuthentication.SetAuthCookie(response.ClaimedIdentifier, false);
                return new RedirectResult("/");
            }
            else
            {
                throw new Exception("Unexpected status " + response.Status);
            }
        }
    }
}