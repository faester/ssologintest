using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.RelyingParty;
using JpPolitikensHus.UserServiceProxyNetCore;

namespace SsoLoginTest.Sso
{
    public static class StringExtensions
    {
        public static string Left(this string inputString, int characters)
        {
            var length = inputString.Length;
            if (length < characters)
            {
                throw new ArgumentException($"Requested leftmost {characters} og string with length {length}. Can't do: Substring length should be smaller than string length.");
            }
            return inputString.Substring(length - characters, characters);
        }
    }
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
                return StartLogin(rp, returnUrl);
            }
        }

        private ActionResult StartLogin(OpenIdRelyingParty rp, string returnUrl)
        {
            var request = rp.CreateRequest(_ssoEndpoint);

            request.AddExtension(new BrandingExtension() { BrandingIdentifier = _branding });

            request.Mode = AuthenticationRequestMode.Setup;

            return request.RedirectingResponse.AsActionResult();
        }

        private ActionResult HandleLogin(IAuthenticationResponse response)
        {
            if (response.Status == AuthenticationStatus.Authenticated)
            {
                var userUri = new Uri(response.ClaimedIdentifier);
                var userId = Guid.Parse(userUri.LocalPath.Left(36));
                var userServiceClient = UserServiceClientFactory
                    .StartBuilding()
                    .WithApplicationCredentials(
                    Guid.Parse(ConfigurationManager.AppSettings["userservice:appid"]),
                    ConfigurationManager.AppSettings["userservice:appkey"])
                    .WithEndpoint(new Uri(ConfigurationManager.AppSettings["userservice:endpoint"]))
                    .BuildClient();

                var user = userServiceClient.Get(userId);

                FormsAuthentication.SetAuthCookie(user.GetUsername(), false);
                return new RedirectResult("/");
            }
            else
            {
                throw new Exception("Unexpected status " + response.Status);
            }
        }

        public void ResetAutoLogin()
        {
            HttpContext.Current.Response.Cookies.Set(new HttpCookie("noautologin", null));
        }
    }
}