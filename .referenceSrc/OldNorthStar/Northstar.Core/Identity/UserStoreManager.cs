using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Northstar.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthStar.Core.Identity
{
    public class UserStoreManager
    {
        protected Microsoft.AspNet.Identity.UserManager<NorthStarUser> userManager;
        public UserStoreManager(string connectionString)
        {
            userManager = new UserManager<NorthStarUser>(new UserStore<NorthStarUser>(new NSAppContext(connectionString)));
            userManager.UserTokenProvider = new TokenProvider<NorthStarUser, string>();
        }

        public NorthStarIdentityResult CreateUser(string email, string password)
        {
            var user = new NorthStarUser();
            user.Email = email;
            user.LoweredEmail = email;
            user.UserName = email;
            user.TwoFactorEnabled = true; // muy importante!!!
            user.LockoutEnabled = true; // 9/28/2020 new security feature

            //userManager.DefaultAccountLockoutTimeSpan = TimeSpan.FromHours(365 * 200 * 24);
            //userManager.MaxFailedAccessAttemptsBeforeLockout = 10;
            //userManager.UserLockoutEnabledByDefault = true;
            
            userManager.UserValidator = new UserValidator<NorthStarUser>(userManager)
            {
                AllowOnlyAlphanumericUserNames = false
            };
            var result = userManager.Create(user, password);

            if (!result.Succeeded)
            {
                return new NorthStarIdentityResult(result.Errors.First());
            }
            return NorthStarIdentityResult.Success;
        }

        public NorthStarIdentityResult DeleteUser(string email)
        {
            var user = GetUserByEmail(email);

            var result = userManager.Delete(user.Result);

            if (!result.Succeeded)
            {
                return new NorthStarIdentityResult(result.Errors.First());
            }
            return NorthStarIdentityResult.Success;
        }

        public virtual NorthStarIdentityResult SetPassword(string userName, string password)
        {
            var user = GetUserByEmail(userName);
            userManager.UserValidator = new UserValidator<NorthStarUser>(userManager)
            {
                AllowOnlyAlphanumericUserNames = false
            };
            var token = this.userManager.GeneratePasswordResetToken(user.Result.Id);
            var result = this.userManager.ResetPassword(user.Result.Id, token, password);

            if (!result.Succeeded)
            {
                return new NorthStarIdentityResult(result.Errors.First());
            }
            return NorthStarIdentityResult.Success;
        }

        public virtual NorthStarIdentityResult LockUserAccount(string userName)
        {
            var user = GetUserByEmail(userName);
            userManager.UserValidator = new UserValidator<NorthStarUser>(userManager)
            {
                AllowOnlyAlphanumericUserNames = false
            };
            var result = this.userManager.SetLockoutEnabled(user.Result.Id, true);
            if (result.Succeeded)
            {
                result = this.userManager.SetLockoutEndDate(user.Result.Id, DateTimeOffset.MaxValue);
            }

            if (!result.Succeeded)
            {
                return new NorthStarIdentityResult(result.Errors.First());
            }
            return NorthStarIdentityResult.Success;
        }

        public virtual NorthStarIdentityResult UnLockUserAccount(string userName)
        {
            var user = GetUserByEmail(userName);
            userManager.UserValidator = new UserValidator<NorthStarUser>(userManager)
            {
                AllowOnlyAlphanumericUserNames = false
            };
            var result = this.userManager.SetLockoutEndDate(user.Result.Id, DateTimeOffset.MinValue); //this.userManager.SetLockoutEnabled(user.Result.Id, false);

            if (!result.Succeeded)
            {
                return new NorthStarIdentityResult(result.Errors.First());
            }
            return NorthStarIdentityResult.Success;
        }

        public virtual NorthStarIdentityResult<NorthStarUser> GetUserByEmail(string subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                return new NorthStarIdentityResult<NorthStarUser>((NorthStarUser)null);
            }

            var user = this.userManager.FindByEmail(subject);

            if (user == null)
            {
                return new NorthStarIdentityResult<NorthStarUser>((NorthStarUser)null);
            }

            return new NorthStarIdentityResult<NorthStarUser>(user);
        }
    }
}
