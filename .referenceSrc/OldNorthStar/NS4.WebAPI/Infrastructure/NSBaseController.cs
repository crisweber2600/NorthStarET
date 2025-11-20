using EntityDto.DTO.Admin.Simple;
//using Microsoft.AspNet.Mvc;
//using Microsoft.Extensions.Logging;
using Northstar.Core;
using NorthStar.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace NorthStar4.API.Infrastructure
{
    public class NSBaseController : ApiController
    {
        public string LoginConnectionString {
            get
            {
                return ConfigurationManager.ConnectionStrings["LoginConnection"].ConnectionString;
            }
            set { }
        }
        public string VzaarSecret
        {
            get
            {
                return ConfigurationManager.AppSettings["VzaarSecret"];
            }
            set { }
        }
        
        public string SiteUrlBase
        {
            get
            {
                return ConfigurationManager.AppSettings["SiteUrlBase"];
            }
            set { }
        }
        public string VzaarToken
        {
            get
            {
                return ConfigurationManager.AppSettings["VzaarToken"];
            }
            set { }
        }
        public string IdentityServer
        {
            get
            {
                return ConfigurationManager.AppSettings["IdentityServer"];
            }
            set { }
        }
        //static protected ILogger<NSBaseController> logger;
        //public class NSWebApiExceptionResponseAttribute : ExceptionFilterAttribute
        //{
        //    //private readonly ILogger logger;

        //    public override void OnException(ExceptionContext context)
        //    {
        //        if (context.Exception is UserDisplayableException)
        //        {
        //            //var j = new HttpError
        //            //var response = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
        //            //response.Content = new HttpContent(). context.Exception.Message;
        //            //var bytes = System.Text.Encoding.ASCII.GetBytes(context.Exception.Message);
        //            context.HttpContext.Response.StatusCode = 501;
        //            context.Result = new JsonResult(((UserDisplayableException)context.Exception).EndUserMessage);

        //            // TODO: write some specific stuff about the exception
        //            logger.LogError("NEW ERROR MESSAGE:" + context.Exception.Message);
        //            //Serilog.Log.loLog.Write(LogEventLevel.Error, context.Exception.Message, null);
        //        }
        //        else
        //        {
        //            //Serilog.Log.Write(LogEventLevel.Error, context.Exception.Message, null);
        //            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        //            context.Result = new JsonResult("An unexepected Error has occurred.  Support has been notified.  Please try again later.");
        //        }

        //       // base.OnException(context);
        //       // throw context.Exception;//log it
        //    }
        //}
        //public NSBaseController(ILogger<NSBaseController> _logger)
        //{
        //    logger = _logger;
        //}

        protected string GetEmail(ClaimsIdentity user)
        {
            return user.FindFirst(NSConstants.ClaimTypes.AuthenticatedAccount) != null ? user.FindFirst(NSConstants.ClaimTypes.AuthenticatedAccount).Value : user.Claims.First(x => x.Type == "preferred_username").Value;
        }

        protected bool IsSA(ClaimsIdentity user)
        {
            var email = GetEmail(user);
            if (email != "northstar.shannon@gmail.com" && email != "northstar.beth@gmail.com" && email != "northstar.diane@gmail.com")
            {
                return false;
            }

            return true;
        }

        protected IHttpActionResult ProcessResultStatus(OutputDto_Base result)
        {
            if (result.Status.StatusCode == EntityDto.DTO.Admin.Simple.StatusCode.AccessDenied)
            {
                return StatusCode(HttpStatusCode.Unauthorized);
            }
            else if (result.Status.StatusCode == EntityDto.DTO.Admin.Simple.StatusCode.UserDisplayableException)
            {
                return BadRequest(result.Status.StatusMessage);
            }

            return Ok(result);
        }
    }
}
