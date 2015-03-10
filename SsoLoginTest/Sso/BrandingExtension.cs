using System;
using System.Collections.Generic;
using DotNetOpenAuth.OpenId.Messages;

namespace SsoLoginTest.Sso
{
    [Serializable]
    public class BrandingExtension : IOpenIdMessageExtension
    {
        static readonly Version version = new Version(1, 0);
        readonly static string[] NoAdditionalSupportedTypeUris = new string[0];
        readonly IDictionary<string, string> _extraData;
        public const string INACTIVE_LINK_PARAMETER_NAME = "inactivelink";
        public const string FORGOT_PASSWORD_LINK_PARAMETER_NAME = "forgot";
        public const string CREATE_USER_LINK_PARAMETER_NAME = "create";


        internal BrandingExtension(IDictionary<string, string> data)
        {
            _extraData = data;
        }

        public BrandingExtension()
        {
            _extraData = new Dictionary<string, string>();
        }

        public IEnumerable<string> AdditionalSupportedTypeUris
        {
            get { return NoAdditionalSupportedTypeUris; }
        }

        public bool IsSignedByRemoteParty { get; set; }

        public string TypeUri
        {
            get { return BrandingExtensionFactory.TypeUri; }
        }

        public void EnsureValidMessage()
        {
            // no op
        }

        public IDictionary<string, string> ExtraData
        {
            get { return _extraData; }
        }

        public Version Version
        {
            get { return version; }
        }

        public string BrandingIdentifier
        {
            get
            {
                return _extraData["brand"];
            }
            set
            {
                _extraData["brand"] = value;
            }
        }
    }
}