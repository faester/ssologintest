using System;
using System.Linq;
using System.Web;

namespace SsoLoginTest.Sso
{
    internal class QueryStringSanitizer
    {
        public string RemoveDotNetOpenAuthParametersFromQuerystring(HttpContext context)
        {
            var badQuerystringParametersr =
                context.Request.QueryString.AllKeys.Where(x => x.StartsWith("dnoa") || x.StartsWith("openid"))
                    .ToList();

            var querystring = HttpUtility.ParseQueryString(context.Request.Url.Query);

            badQuerystringParametersr.ForEach(querystring.Remove);

            // this gets the page path from root without QueryString
            var pagePathWithoutQueryString = context.Request.Url.GetLeftPart(UriPartial.Path);

            var redirTo = querystring.Count > 0
                ? String.Format("{0}?{1}", pagePathWithoutQueryString, querystring)
                : pagePathWithoutQueryString;
            return redirTo;
        }


    }
}