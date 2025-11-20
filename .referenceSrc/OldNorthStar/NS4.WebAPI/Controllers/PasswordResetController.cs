using EntityDto.DTO.Admin.Simple;
using EntityDto.LoginDB.DTO;
using System.Security.Claims;
using System.Web.Http;
using NorthStar.EF6;
using NorthStar4.API.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NorthStar4.API.api
{
    [RoutePrefix("api/PasswordReset")]
    public class PasswordResetController : NSBaseController
    {
        

        [Route("RequestPasswordReset")]
        [HttpPost]
        public IHttpActionResult RequestPasswordReset([FromBody] InputDto_RequestPasswordReset input)
        {
            var dataService = new LoginDbDataService(LoginConnectionString);
            var result = dataService.RequestPasswordReset(input.UserName);

            if (result.Status.StatusCode == EntityDto.DTO.Admin.Simple.StatusCode.Ok)
            {
                // send email from here
                EmailHandler.PasswordResetLinkEmail(input.UserName, input.UserName, result.Guid.ToString(), SiteUrlBase);
            }

            return ProcessResultStatus(result);
        }
        [Route("ResetPasswordFromEmail")]
        [HttpPost]
        public IHttpActionResult ResetPasswordFromEmail([FromBody] InputDto_PasswordReset input)
        {
            // first validate UID
            var dataService = new LoginDbDataService(LoginConnectionString);
            var result = dataService.ValidateUID(input);

            if (result.Status.StatusCode == EntityDto.DTO.Admin.Simple.StatusCode.Ok)
            {
                // now change the password
                result = dataService.ResetUsersPasswordFromEmail(input.UID, input.Password);
            }

            return ProcessResultStatus(result);
        }
        [Route("ResetUsersPassword")]
        [HttpPost]
        public IHttpActionResult ResetUsersPassword([FromBody] InputDto_ResetUsersPassword input)
        {
            var dataService = new LoginDbDataService(LoginConnectionString);
            var result = dataService.ResetUsersPassword(input.UserName, input.Password);

            return ProcessResultStatus(result);
        }
        [Route("ChangeUsername")]
        [HttpPost]
        public IHttpActionResult ChangeUsername([FromBody] InputDto_SimpleString input)
        {
            var loginDataService = new LoginDbDataService(LoginConnectionString);
            var result = loginDataService.ChangeLogin(input.value, ((ClaimsIdentity)User.Identity));

            if (result.Status.StatusCode == EntityDto.DTO.Admin.Simple.StatusCode.Ok)
            {
                var staffDataService = new StaffDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
                var result2 = staffDataService.ChangeUsername(input);
            }

            return ProcessResultStatus(result);
        }
        [Route("ValidateUID")]
        [HttpPost]
        public IHttpActionResult ValidateUID([FromBody] InputDto_PasswordReset input)
        {
            var dataService = new LoginDbDataService(LoginConnectionString);
            var result = dataService.ValidateUID(input);

            return ProcessResultStatus(result);
        }
    }
}
