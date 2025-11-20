using NorthStar.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;

namespace NS4.WebAPI.Infrastructure
{
    public class UnhandledExceptionLogger : ExceptionLogger
    {
        public override void Log(ExceptionLoggerContext context)
        {
            var log = context.Exception.ToString();
            var user = (ClaimsIdentity)context.RequestContext.Principal.Identity;

            var username = string.Empty;
            if (user != null)
            {
                username = user.FindFirst(NSConstants.ClaimTypes.AuthenticatedAccount) != null ? user.FindFirst(NSConstants.ClaimTypes.AuthenticatedAccount).Value : user.Claims.First(x => x.Type == "preferred_username").Value;
            }
            Serilog.Log.Error("NorthStar Unhandled Exception: {log} at Url: {url}, Username: {2}", log, context.Request.RequestUri.AbsolutePath, username);
        }
    }
}
