//using Microsoft.AspNet.Authorization;
//using Microsoft.AspNet.Mvc;
//using Microsoft.Extensions.Logging;
using NorthStar.EF6;
using NorthStar4.API.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EntityDto.DTO.Navigation;
using System.Configuration;
//using Microsoft.Extensions.OptionsModel;
using System.Web.Http;
using System.Security.Claims;
using EntityDto.DTO.Admin.Simple;

namespace NorthStar4.API.api
{
    
    [Authorize]
    [RoutePrefix("api/Navigation")]
    public class NavigationController : NSBaseController
    {
        //
       // private NorthStarDataService dataService = null;

        // sample data
        public NavigationController()
        {
            var user = User;
            // AppSettings = appSettings;
            //dataService = new NorthStarDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
        }

        [Route("NavigationNodes")]
        [HttpGet]
        public OutputDto_Navigation NavigationNodes()
        {
            var dataService = new NavigationDataService(((ClaimsIdentity)User.Identity), ConfigurationManager.ConnectionStrings["LoginConnection"].ConnectionString);
            var rootObj = new OutputDto_Navigation();
            dataService.BuildNavigation(rootObj);
            
            return rootObj;
        }



        [Route("GetHelp")]
        [HttpPost]
        public IHttpActionResult GetHelp([FromBody]InputOutputDto_NSHelp input)
        {
            var dataService = new NavigationDataService(((ClaimsIdentity)User.Identity), ConfigurationManager.ConnectionStrings["LoginConnection"].ConnectionString);
            var returnValue = dataService.GetHelp(input);

            return ProcessResultStatus(returnValue);
        }

        [Route("SaveHelp")]
        [HttpPost]
        public IHttpActionResult SaveHelp([FromBody]InputOutputDto_NSHelp input)
        {
            var dataService = new NavigationDataService(((ClaimsIdentity)User.Identity), ConfigurationManager.ConnectionStrings["LoginConnection"].ConnectionString);
            var returnValue = dataService.SaveHelp(input);

            return ProcessResultStatus(returnValue);
        }
    }
}
