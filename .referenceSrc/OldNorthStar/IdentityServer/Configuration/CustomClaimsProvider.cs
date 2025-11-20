using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using IdentityServer3.Core.Validation;
using NorthStar.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NorthStar4.IdentityServer.Configuration
{
    public class CustomClaimsProvider : DefaultClaimsProvider
    {
        public CustomClaimsProvider(IUserService users) : base(users)
        { }

        public override async Task<IEnumerable<Claim>> GetAccessTokenClaimsAsync(ClaimsPrincipal subject, Client client, IEnumerable<Scope> scopes, ValidatedRequest request)
        {
            var claims = await base.GetAccessTokenClaimsAsync(subject, client, scopes, request);

            var newClaims = claims.ToList();
            newClaims.Add(subject.FindFirst(NSConstants.ClaimTypes.DistrictId));
            newClaims.Add(subject.FindFirst(NSConstants.ClaimTypes.AuthenticatedAccount));
            //newClaims.Add(subject.FindFirst("district_admin"));

            return newClaims;
        }
    }
}
