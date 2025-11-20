using Microsoft.AspNet.Identity;
using NorthStar.Core;
using NorthStar.EF6;
//using NorthStar4.IdentityServer.AspNetIdentity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
//using System.IdentityModel.Claims;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using IdentityServer3.AspNetIdentity;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using static IdentityServer3.Core.Constants;

namespace NorthStar4.IdentityServer.Configuration
{
    public static class UserServiceExtensions
    {
        public static void ConfigureUserService(this IdentityServerServiceFactory factory, string connString, string corsOrigin)
        {
            factory.UserService = new Registration<IUserService, UserService>();
            factory.Register(new Registration<UserManager>());
            factory.Register(new Registration<UserStore>());
            factory.Register(new Registration<NSAppContext>(resolver => new NSAppContext(connString)));

            ConnectionString = connString;
            CORSOrigin = corsOrigin;
        }

        public static string ConnectionString { get; set; }
        public static string CORSOrigin { get; set; }
    }

    public class UserService : AspNetIdentityUserService<NorthStarUser, string>
    {
        private readonly LoginContext _dbContext;

        public UserService(UserManager userMgr)
            : base(userMgr)
        {
            _dbContext = new LoginContext(UserServiceExtensions.ConnectionString);
        }

        //public override async Task PreAuthenticateAsync(PreAuthenticationContext context)
        //{
        //    //if we are requesting impersonationg, I pass it in acr values as you will see later on (Impersonate:UserName)
        //    string impersonatedUserName = context.AuthenticateResult.a.SignInMessage.AcrValues.FirstOrDefault(x => x.Split(':')[0] == "Impersonate");

        //    if (!string.IsNullOrWhiteSpace(impersonatedUserName))
        //    {
        //        impersonatedUserName = impersonatedUserName.Split(':')[1];

        //        //verify eligibility to impersonate user, you may need to modify this to siut your needs
        //        PortalUser currentUser = await userManager.FindByIdAsync(int.Parse(_ctx.Authentication.User.FindFirst(Constants.ClaimTypes.Subject).Value));
        //        bool isUserAdmin = currentUser.Claims.Any(x => x.ClaimType == "UserAdmin");

        //        //lookup the user to be impersonated
        //        PortalUser impersonatedUser = await userManager.Users.FirstOrDefaultAsync(x => x.UserName == impersonatedUserName && isUserAdmin);
        //        if (impersonatedUser != null)
        //        {
        //            context.AuthenticateResult = new AuthenticateResult(impersonatedUser.Id.ToString(), impersonatedUser.UserName, impersonationClaims);
        //        }
        //        else
        //        {
        //            context.AuthenticateResult = new AuthenticateResult("Invalid attempt to impersonate user");
        //        }
        //    }
        //    else
        //    {
        //        await base.PreAuthenticateAsync(context);
        //    }
        //}

        protected override async Task<AuthenticateResult> PostAuthenticateLocalAsync(NorthStarUser user, SignInMessage message)
        {
            if (base.userManager.SupportsUserTwoFactor)
            {
                var impersonatedUserAccount = message.AcrValues;
                var id = user.Id;
                var email = user.Email;

                if (await userManager.GetTwoFactorEnabledAsync(id))
                {
                    var code = await this.userManager.GenerateTwoFactorTokenAsync(id, "sms");
                    var result = await userManager.NotifyTwoFactorTokenAsync(id, "sms", code);
                    if (!result.Succeeded)
                    {
                        return new AuthenticateResult(result.Errors.First());
                    }

                    // i am the only admin
                    if(email == "northstar.shannon@gmail.com")
                    {
                        Trace.TraceInformation("Impersonating User: " + impersonatedUserAccount);
                        if(impersonatedUserAccount.Count() == 1)
                        {
                            if (impersonatedUserAccount.First() != "undefined")
                            {
                                email = impersonatedUserAccount.First();
                            }
                        }
                    }

                    Trace.TraceInformation("Email of user is: " + email);
                    var staffDistrict = _dbContext.StaffDistricts.FirstOrDefault(p => p.StaffEmail == email);
                    var districtId = staffDistrict == null ? -1 : staffDistrict.DistrictId;
                    Trace.TraceInformation("District ID is:" + districtId);

                    var name = await GetDisplayNameForAccountAsync(id);
                    Trace.TraceInformation("Got name:" + name);
                    return new AuthenticateResult(user.Id,
                        email,
                        new List<System.Security.Claims.Claim> {
                            new System.Security.Claims.Claim(ClaimTypes.Email, email),
                            new System.Security.Claims.Claim(NSConstants.ClaimTypes.DistrictId, districtId.ToString()),
                            new System.Security.Claims.Claim(NSConstants.ClaimTypes.AuthenticatedAccount, email),
                            new System.Security.Claims.Claim(ClaimTypes.PreferredUserName, email)
                        },
                        BuiltInIdentityProvider,
                        null);
                }
            }

            return null;
        }
    }
}
