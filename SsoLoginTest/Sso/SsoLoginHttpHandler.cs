using System;
using System.Configuration;
using System.Web;
using System.Web.Security;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.RelyingParty;

namespace SsoLoginTest.Sso
{
    public class SsoLoginHttpHandler : IHttpHandler
    {
        private readonly QueryStringSanitizer _queryStringSanitizer = new QueryStringSanitizer();
        private readonly string _ssoEndpoint;
        private readonly string _branding;

        public SsoLoginHttpHandler(string branding, Uri ssoEndpoint)
        {
            if (branding == null) { throw new ArgumentNullException("branding"); }
            if (ssoEndpoint == null) { throw new ArgumentNullException("ssoEndpoint"); }
            _ssoEndpoint = ssoEndpoint.ToString();
            _branding = branding;
        }

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

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Cookies["noautologin"] != null 
                && context.Request.Cookies["noautologin"].Value == "done")
            {
                return;
            } else if (context.Request.Cookies["noautologin"] != null)
            {
                CheckLogin(context);
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

        private void CheckLogin(HttpContext context)
        {
            var rp = new OpenIdRelyingParty();
            var response = rp.GetResponse();
            if (response == null)
            {
                return;
            }
            if (response.Status == AuthenticationStatus.Authenticated)
            {
                DoLogin(response);
                var redirTo = _queryStringSanitizer.RemoveDotNetOpenAuthParametersFromQuerystring(context);
                context.Response.Redirect(redirTo, true);
            }
        }

        /// <summary>
        /// Performs the actual login. Should/could use other approaches than 
        /// FormsAuthentication.
        /// </summary>
        /// <param name="response"></param>
        protected virtual void DoLogin(IAuthenticationResponse response)
        {
            FormsAuthentication.SetAuthCookie(response.ClaimedIdentifier, false);
        }

        public bool IsReusable { get { return true; } }
    }
}