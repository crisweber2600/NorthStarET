using EntityDto.DTO.Admin.Simple;
using EntityDto.DTO.DataEntry;
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
    [RoutePrefix("api/InterventionGroupDataEntry")]
    [Authorize]
    public class InterventionGroupDataEntryController : NSBaseController
    {

        [Route("SaveAssessmentResult")]
        [HttpPost]
        public IHttpActionResult SaveAssessmentResult([FromBody]InputDto_SaveAssessmentResult studentResult)
        {
            var dataService = new InterventionGroupDataEntryService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            if (ModelState.IsValid)
            {
                var result = dataService.SaveAssessmentResult(studentResult);

                return ProcessResultStatus(result);
            }
            else
            {
                return BadRequest("There are invalid field values, please correct them before submitting.");
            }
        }

        // TODO: Figure out what to do for these cases.  I think it is return a 200 or just return generic bad error, probably
        // should throw it so that it is logged
        [Route("DeleteAssessmentResult")]
        [HttpPost]
        public IHttpActionResult DeleteAssessmentResult([FromBody]InputDto_SaveAssessmentResult studentResult)
        {
            var dataService = new InterventionGroupDataEntryService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            if (ModelState.IsValid)
            {
                dataService.DeleteAssessmentResult(studentResult);

                // loop over fields and save data
                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [Route("GetAssessmentResults")]
        [HttpPost]
        public OutputDto_StudentAssessmentResults GetAssessmentResults([FromBody]InputDto_AssessmentInterventionGroupStudent input)
        {
            var dataService = new InterventionGroupDataEntryService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetIGAssessmentResults(input);

            return result;
        }

       
    }
}
