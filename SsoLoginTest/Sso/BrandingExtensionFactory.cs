using System.Collections.Generic;
using DotNetOpenAuth.OpenId.ChannelElements;

namespace SsoLoginTest.Sso
{
    public class BrandingExtensionFactory : IOpenIdExtensionFactory
    {
        public const string TypeUri = "http://openid.net/srv/ax/1.0";

        public DotNetOpenAuth.OpenId.Messages.IOpenIdMessageExtension Create(string typeUri, IDictionary<string, string> data, DotNetOpenAuth.Messaging.IProtocolMessageWithExtensions baseMessage, bool isProviderRole)
        {
            if (typeUri == TypeUri && isProviderRole)
            {
                return new BrandingExtension();
            }

            return null;
        }
    }
}