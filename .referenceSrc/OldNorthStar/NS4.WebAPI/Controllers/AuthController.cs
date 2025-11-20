using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MimeTypeMap;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using System.Net.Http.Headers;
using NS4.WebAPI.Infrastructure;
using NorthStar.EF6;
using NorthStar4.API.Infrastructure;
using System.Security.Claims;
using NorthStar.EF6.DataService;
using Northstar.Core;
using IdentityModel.Client;
using System.Web.Security;

namespace NS4.WebAPI.Controllers
{
    [RoutePrefix("api/auth")]
    public class AuthController : NSBaseController
    {

        [Route("getrefreshtoken")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IHttpActionResult> RefreshToken(string refreshToken)
        {
            var tokenClient = new TokenClient(IdentityServer + "/connect/token", "my-application-id", "my-application-secret");
            var response = await tokenClient.RequestRefreshTokenAsync(StringEncryptor.Decrypt(refreshToken));
            var newAccessToken = response.AccessToken;

            return Json(new { newAccessToken });
        }

        public static class StringEncryptor
        {
            public static string Encrypt(string plaintextValue)
            {
                var plaintextBytes = plaintextValue.Select(c => (byte)c).ToArray();
                var encryptedBytes = MachineKey.Protect(plaintextBytes);
                return Convert.ToBase64String(encryptedBytes);
            }

            public static string Decrypt(string encryptedValue)
            {
                try
                {
                    var encryptedBytes = Convert.FromBase64String(encryptedValue);
                    var decryptedBytes = MachineKey.Unprotect(encryptedBytes);
                    return new string(decryptedBytes.Select(b => (char)b).ToArray());
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
