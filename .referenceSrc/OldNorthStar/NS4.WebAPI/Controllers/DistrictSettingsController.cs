using EntityDto.DTO.Admin.District;
using EntityDto.DTO.Admin.Simple;
using NorthStar.EF6;
using NorthStar4.API.Infrastructure;
using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace NorthStar4.API.api
{
    [RoutePrefix("api/DistrictSettings")]
    [Authorize]
    public class DistrictSettingsController : NSBaseController
    {
        

        [Route("GetStudentAttributes")]
        [HttpGet]
        public IHttpActionResult GetStudentAttributes()
        {
            var dataService = new DistrictSettingsDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetStudentAttributes();
            return ProcessResultStatus(result);
        }

        [Route("SaveAttribute")]
        [HttpPost]
        public IHttpActionResult SaveAttribute([FromBody]InputDto_StudentAttribute input)
        {
            var dataService = new DistrictSettingsDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.SaveAttribute(input.Attribute);
            return ProcessResultStatus(result);
        }

        [Route("LogIn")]
        [HttpPost]
        public IHttpActionResult LogIn([FromBody]InputDto_SimpleId input)
        {
            
            if (!IsSA(((ClaimsIdentity)User.Identity)))
            {
                return Unauthorized();
            }

            var dataService = new DistrictSettingsDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.LogIn(input);
            return ProcessResultStatus(result);
        }

        [Route("SaveAttributeValue")]
        [HttpPost]
        public IHttpActionResult SaveAttributeValue([FromBody]InputDto_StudentAttributeValue input)
        {
            var dataService = new DistrictSettingsDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.SaveAttributeValue(input.AttributeValue);
            return ProcessResultStatus(result);
        }

        [Route("DeleteAttribute")]
        [HttpPost]
        public IHttpActionResult DeleteAttribute([FromBody]InputDto_StudentAttribute input)
        {
            var dataService = new DistrictSettingsDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.DeleteAttribute(input.Attribute);
            return ProcessResultStatus(result);
        }

        [Route("DeleteAttributeValue")]
        [HttpPost]
        public IHttpActionResult DeleteAttributeValue([FromBody]InputDto_StudentAttributeValue input)
        {
            var dataService = new DistrictSettingsDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.DeleteAttributeValue(input.AttributeValue);
            return ProcessResultStatus(result);
        }

        [Route("GetInterventionList")]
        [HttpGet]
        public IHttpActionResult GetInterventionList()
        {
            var dataService = new DistrictSettingsDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetInterventionList();
            return ProcessResultStatus(result);
        }

        [Route("GetBenchmarkDatesForSchoolYear")]
        [HttpPost]
        public IHttpActionResult GetBenchmarkDatesForSchoolYear([FromBody]InputDto_SimpleId input)
        {
            var dataService = new DistrictSettingsDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetBenchmarkDatesForSchoolYear(input);
            return ProcessResultStatus(result);
        }

        [Route("GetHFWList")]
        [HttpPost]
        public IHttpActionResult GetHFWList([FromBody]InputDto_HFWList input)
        {
            var dataService = new DistrictSettingsDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetHFWList(input);
            return ProcessResultStatus(result);
        }

        [Route("SaveHfw")]
        [HttpPost]
        public IHttpActionResult SaveHfw([FromBody]InputDto_HFW input)
        {
            var dataService = new DistrictSettingsDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.SaveHfw(input.Word);
            return ProcessResultStatus(result);
        }

        [Route("SaveIntervention")]
        [HttpPost]
        public IHttpActionResult SaveIntervention([FromBody]InputDto_SaveIntervention input)
        {
            var dataService = new DistrictSettingsDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.SaveIntervention(input.Intervention);
            return ProcessResultStatus(result);
        }
        
        [Route("DeleteIntervention")]
        [HttpPost]
        public IHttpActionResult DeleteIntervention([FromBody]InputDto_SaveIntervention input)
        {
            var dataService = new DistrictSettingsDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.DeleteIntervention(input.Intervention);
            return ProcessResultStatus(result);
        }

        [Route("SaveTestDueDate")]
        [HttpPost]
        public IHttpActionResult SaveTestDueDate([FromBody]InputDto_SaveTestDueDate input)
        {
            var dataService = new DistrictSettingsDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.SaveTestDueDate(input.Tdd);
            return ProcessResultStatus(result);
        }

        [Route("DeleteBenchmarkDate")]
        [HttpPost]
        public IHttpActionResult DeleteBenchmarkDate([FromBody]InputDto_SaveTestDueDate input)
        {
            var dataService = new DistrictSettingsDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.DeleteBenchmarkDate(input.Tdd);
            return ProcessResultStatus(result);
        }
    }
}
