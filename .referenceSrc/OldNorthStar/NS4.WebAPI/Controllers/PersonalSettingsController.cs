using EntityDto.DTO.Admin.Simple;
using EntityDto.DTO.Personal;
using System.Security.Claims;
using System.Web.Http;
using NorthStar.EF6;
using NorthStar4.API.Infrastructure;
using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NorthStar4.API.api
{
    [RoutePrefix("api/PersonalSettings")]
    [Authorize]
    public class PersonalSettingsController : NSBaseController
    {

        //private NorthStarDataService dataService = null;

        [Route("GetAssessmentsAndFieldsForUser")]
        [HttpPost]
        public OutputDto_GetAssessmentsAndFieldsForUser GetAssessmentsAndFieldsForUser()
        {
            var dataService = new StaffDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetAssessmentsAndFieldsForUser();

            return result;
        }
        [Route("GetAssessmentAndFieldsForUser")]
        [HttpPost]
        public OutputDto_GetAssessmentsAndFieldsForUser GetAssessmentAndFieldsForUser([FromBody]InputDto_SimpleId input)
        {
            var dataService = new StaffDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetAssessmentAndFieldsForUser(input.Id);

            return result;
        }
        [Route("UpdateFieldForUser")]
        [HttpPost]
        public OutputDto_SuccessAndStatus UpdateFieldForUser([FromBody]InputDto_UpdateFieldForUser input)
        {
            var dataService = new StaffDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.UpdateFieldForUser(input);

            return result;
        }
        [Route("GetUserCurrentVersion")]
        [HttpGet]
        public IHttpActionResult GetUserCurrentVersion([FromBody]InputDto_UpdateFieldForUser input)
        {
            var dataService = new StaffDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetUserCurrentVersion();

            return ProcessResultStatus(result);
        }
        [Route("StartVersionUpdate")]
        [HttpPost]
        public OutputDto_SuccessAndStatus StartVersionUpdate()
        {
            var dataService = new StaffDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.StartVersionUpdate();

            return result;
        }
        [Route("FinalizeVersionUpdate")]
        [HttpPost]
        public OutputDto_SuccessAndStatus FinalizeVersionUpdate()
        {
            var dataService = new StaffDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.FinalizeVersionUpdate();

            return result;
        }
    }
}
