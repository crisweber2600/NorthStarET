using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;
//using Thinktecture.IdentityModel.Client;

namespace Northstar.Core
{
    public class TokenManager
    {
        public static class Constants
        {
            // TODO: Move to APP SETTINGS... don't care for now since this only affects printing
            public const string BaseAddress = "http://localhost:16725";

            public const string AuthorizeEndpoint = BaseAddress + "/connect/authorize";
            public const string LogoutEndpoint = BaseAddress + "/connect/endsession";
            public const string TokenEndpoint = BaseAddress + "/connect/token";
            public const string UserInfoEndpoint = BaseAddress + "/connect/userinfo";
            public const string IdentityTokenValidationEndpoint = BaseAddress + "/connect/identitytokenvalidation";
            public const string TokenRevocationEndpoint = BaseAddress + "/connect/revocation";

        }

        //public Thinktecture.IdentityModel.Client.TokenResponse RequestToken()
        //{
        //    var client = new OAuth2Client(
        //        new Uri(Constants.TokenEndpoint),
        //        "roclient",
        //        "secret");

        //    // idsrv supports additional non-standard parameters 
        //    // that get passed through to the user service
        //    var optional = new Dictionary<string, string>
        //    {
        //        { "acr_values", "onbehalfof:kerri.town@isd196.org" }
        //    };

        //    return client.RequestResourceOwnerPasswordAsync("northstar.shannon@gmail.com", "dammit", "idmgr", optional).Result;
        //}

        //public string AccessToken
        //{
        //    get
        //    {
        //        return RequestToken().AccessToken;
        //    }
        //}
    }
}
