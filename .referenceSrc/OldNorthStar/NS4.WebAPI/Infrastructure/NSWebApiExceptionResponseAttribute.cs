//using Microsoft.AspNet.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using Microsoft.Extensions.Internal;
using System.Net.Http;
using System.Net;
//using Serilog.Events;
//using Microsoft.Extensions.Logging;
using Northstar.Core;

namespace NorthStar4.API.Infrastructure
{
    public class NSWebApiExceptionResponseAttribute //: ExceptionFilterAttribute
    {
        //private readonly ILogger logger;

        //public NSWebApiExceptionResponseAttribute()
        //{
        //}
   
        //public override void OnException(ExceptionContext context)
        //{
        //    if(context.Exception is UserDisplayableException)
        //    {
        //        //var j = new HttpError
        //        //var response = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
        //        //response.Content = new HttpContent(). context.Exception.Message;
        //        //var bytes = System.Text.Encoding.ASCII.GetBytes(context.Exception.Message);
        //        context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        //        context.Result = new JsonResult(((UserDisplayableException)context.Exception).EndUserMessage);

        //        // TODO: write some specific stuff about the exception
                
        //        //Serilog.Log.loLog.Write(LogEventLevel.Error, context.Exception.Message, null);
        //    }
        //    else
        //    {
        //        //Serilog.Log.Write(LogEventLevel.Error, context.Exception.Message, null);
        //        context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        //        context.Result = new JsonResult("An unexepected Error has occurred.  Support has been notified.  Please try again later.");
        //    }

        //    base.OnException(context);
        //    throw context.Exception;//log it
        //}
    }
}
