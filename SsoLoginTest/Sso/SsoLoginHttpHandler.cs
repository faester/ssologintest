using System;
using System.Configuration;
using System.Web;
using System.Web.Security;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.RelyingParty;
using JpPolitikensHus.UserServiceProxyNetCore;

namespace SsoLoginTest.Sso
{
    /// <summary>
    /// This class is injected into the ASPX stack in begin-request. 
    /// <para>
    /// It will check a local cookie ot see if a login has been attempted 
    /// against the SSO.
    /// </para>
    /// <para>
    /// If no prior login attempts has been made against sso, an openid 
    /// immediate request is performed. If a user context exist a user 
    /// session wil be established. 
    /// </para>
    /// </summary>
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
            if (context.Request.Cookies["noautologin"] == null)
            {
                // Cookie does not exist, so prepare the request, set the cookie and redirect to provider

                var rp = new OpenIdRelyingParty();

                var request = rp.CreateRequest(_ssoEndpoint);

                request.AddExtension(new BrandingExtension() { BrandingIdentifier = _branding });

                request.Mode = AuthenticationRequestMode.Immediate;

                context.Response.Cookies.Add(new HttpCookie("noautologin", "active"));

                request.RedirectToProvider();
            }
            else if (context.Request.Cookies["noautologin"].Value != "done")
            {
                // Implied that if cookie value == "done" then nothing happens
                CheckLogin(context);
                context.Response.Cookies.Add(new HttpCookie("noautologin", "done"));
            }
        }

        private void CheckLogin(HttpContext context)
        {
            var rp = new OpenIdRelyingParty();
            var response = rp.GetResponse();
            if (response?.Status == AuthenticationStatus.Authenticated)
            {
                SsoLoginHandler.PerformLogin(response);
                var redirTo = _queryStringSanitizer.RemoveDotNetOpenAuthParametersFromQuerystring(context);
                context.Response.Redirect(redirTo, true);
            }
        }
        
        public bool IsReusable { get { return true; } }
    }
}