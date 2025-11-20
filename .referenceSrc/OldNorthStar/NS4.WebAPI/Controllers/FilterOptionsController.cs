using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MimeTypeMap;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using System.Net.Http.Headers;
using NS4.WebAPI.Infrastructure;
using NorthStar.EF6;
using NorthStar4.API.Infrastructure;
using System.Security.Claims;
using NorthStar.EF6.DataService;
using EntityDto.DTO.Admin.Simple;

namespace NS4.WebAPI.Controllers
{
    [RoutePrefix("api/filteroptions")]
    [Authorize]
    public class FilterOptionsController : NSBaseController
    {

        [Route("LoadAllSchoolsForUser")]
        [HttpGet]
        public IHttpActionResult LoadAllSchoolsForUser()
        {
            var dataService = new FilterOptionsDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetUserAccessibleSchools();

            return Ok(result);
        }

        [Route("LoadAllSchoolYears")]
        [HttpGet]
        public IHttpActionResult LoadAllSchoolYears()
        {
            var dataService = new FilterOptionsDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetAllSchoolYears();

            return Ok(result);
        }

        [Route("LoadAllGrades")]
        [HttpGet]
        public IHttpActionResult LoadGrades()
        {

            var dataService = new FilterOptionsDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.LoadGrades();

            return Ok(result);
        }

        //InputDto_HfwSetting
        [Route("UpdateHfwSetting")]
        [HttpPost]
        public IHttpActionResult UpdateHfwSetting(InputDto_HfwSetting input)
        {

            var dataService = new FilterOptionsDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.UpdateHfwSetting(input.SettingName, input.SettingValue);

            return Ok(result);
        }

        [Route("UpdateStaffSetting")]
        [HttpPost]
        public IHttpActionResult UpdateStaffSetting(InputDto_HfwSetting input)
        {

            var dataService = new FilterOptionsDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.UpdateSetting(input.SettingName, input.SettingValue);

            return Ok(result);
        }

        [Route("GetExistingBoolSetting")]
        [HttpPost]
        public IHttpActionResult GetExistingBoolSetting(InputDto_SimpleString input)
        {

            var dataService = new FilterOptionsDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetExistingBoolSetting(input.value);

            return Ok(result);
        }



        [Route("LoadHfwSettings")]
        [HttpGet]
        public IHttpActionResult LoadHfwSettings()
        {

            var dataService = new FilterOptionsDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.LoadHfwSettings();

            return ProcessResultStatus(result);
        }


        [Route("LoadStateTests")]
        [HttpGet]
        public IHttpActionResult LoadStateTests()
        {


            var dataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetStaffAssessments();
            result.StaffAssessments = result.StaffAssessments.Where(p => p.TestType == 3).ToList();

            return Ok(result);
        }

        [Route("LoadBenchmarkAssessments")]
        [HttpGet]
        public IHttpActionResult LoadBenchmarkAssessments()
        {

            var dataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetStaffAssessments();
            result.StaffAssessments = result.StaffAssessments.Where(p => p.TestType == 1).ToList();

            return Ok(result);
        }

        [Route("LoadInterventionAssessments")]
        [HttpGet]
        public IHttpActionResult LoadInterventionAssessments()
        {


            var dataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetStaffAssessments();
            result.StaffAssessments = result.StaffAssessments.Where(p => p.TestType == 2).ToList();

            return Ok(result);
        }

    }
}
