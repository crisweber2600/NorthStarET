using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer3.Core;
using IdentityServer3.Core.Models;
using NorthStar.Core;

namespace NorthStar4.IdentityServer.Configuration
{
    public class Scopes
    {
        public static IEnumerable<Scope> Get()
        {
            return new[]
            {
                StandardScopes.OpenId,
                StandardScopes.Profile,
                StandardScopes.Email,
                StandardScopes.Address,
                StandardScopes.OfflineAccess,
                StandardScopes.RolesAlwaysInclude,
                StandardScopes.AllClaims,

                ////////////////////////
                // resource scopes
                ////////////////////////

                new Scope
                {
                    Name = "read",
                    DisplayName = "Read data",
                    Type = ScopeType.Resource,
                    Emphasize = false,
                },
                new Scope
                {
                    Name = "write",
                    DisplayName = "Write data",
                    Type = ScopeType.Resource,
                    Emphasize = true,
                },
                new Scope
                {
                    Name = "idmgr",
                    DisplayName = "IdentityManager",
                    Type = ScopeType.Resource,
                    Emphasize = true,
                    ShowInDiscoveryDocument = true,
                    IncludeAllClaimsForUser = true,

                    Claims = new List<ScopeClaim>
                    {
                        new ScopeClaim(Constants.ClaimTypes.Name),
                        new ScopeClaim(Constants.ClaimTypes.Role),
                        new ScopeClaim(Constants.ClaimTypes.Email),
                        new ScopeClaim(Constants.ClaimTypes.Id),
                        new ScopeClaim(Constants.ClaimTypes.Role),
                        new ScopeClaim(Constants.ClaimTypes.PreferredUserName),
                        new ScopeClaim(NSConstants.ClaimTypes.AuthenticatedAccount),
                        new ScopeClaim(NSConstants.ClaimTypes.DistrictId)
                    }
                }
            };
        }
    }
}
