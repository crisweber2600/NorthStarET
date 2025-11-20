using EntityDto.DTO.Admin.Simple;
using EntityDto.DTO.DataEntry;

using NorthStar.EF6;
using NorthStar4.API.Infrastructure;
using NorthStar4.PCL.DTO;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace NorthStar4.API.api
{
    [RoutePrefix("api/dataentry")]
    [Authorize]
    public class DataEntryController : NSBaseController
    {
        //private readonly DistrictContext _dbContext;

        //private NorthStarDataService dataService = null;


        [Route("SaveAssessmentResult")]
        [HttpPost]
        public async Task<IHttpActionResult> SaveAssessmentResult([FromBody]InputDto_SaveAssessmentResult studentResult)
        {
            var dataService = new SectionDataEntryService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            if (ModelState.IsValid)
            {
                var result = await dataService.SaveAssessmentResult(studentResult);

                return ProcessResultStatus(result);
            }
            else
            {
                return BadRequest("There are invalid field values, please correct them before submitting.");
            }
        }

        [Route("SaveProgMonResult")]
        [HttpPost]
        public IHttpActionResult SaveProgMonResult([FromBody]InputDto_SaveProgMonResult studentResult)
        {
            var dataService = new SectionDataEntryService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            if (ModelState.IsValid)
            {
                var result = dataService.SaveProgMonResult(studentResult);

                return ProcessResultStatus(result);
            }
            else
            {
                return BadRequest("There are invalid field values, please correct them before submitting.");
            }
        }

        [Route("CopyStudentAssessmentData")]
        [HttpPost]
        public IHttpActionResult CopyStudentAssessmentData([FromBody]InputDto_CopyStudentAssessmentResult studentResult)
        {
            var dataService = new SectionDataEntryService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            if (ModelState.IsValid)
            {
                var result = dataService.CopyStudentAssessmentData(studentResult);

                return ProcessResultStatus(result);
            }
            else
            {
                return BadRequest("There are invalid field values, please correct them before submitting.");
            }
        }

        [Route("CopySectionAssessmentData")]
        [HttpPost]
        public IHttpActionResult CopySectionAssessmentData([FromBody]InputDto_CopySectionAssessmentResult studentResult)
        {
            var dataService = new SectionDataEntryService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            if (ModelState.IsValid)
            {
                var result = dataService.CopySectionAssessmentData(studentResult);

                return ProcessResultStatus(result);
            }
            else
            {
                return BadRequest("There are invalid field values, please correct them before submitting.");
            }
        }

        [Route("CopyFromStudentAssessmentData")]
        [HttpPost]
        public IHttpActionResult CopyFromStudentAssessmentData([FromBody]InputDto_CopyFromStudentAssessmentResult studentResult)
        {
            var dataService = new SectionDataEntryService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            if (ModelState.IsValid)
            {
                var result = dataService.CopyFromStudentAssessmentData(studentResult);

                return ProcessResultStatus(result);
            }
            else
            {
                return BadRequest("There are invalid field values, please correct them before submitting.");
            }
        }

        [Route("CopyFromSectionAssessmentData")]
        [HttpPost]
        public IHttpActionResult CopyFromSectionAssessmentData([FromBody]InputDto_CopyFromSectionAssessmentResult studentResult)
        {
            var dataService = new SectionDataEntryService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            if (ModelState.IsValid)
            {
                var result = dataService.CopyFromSectionAssessmentData(studentResult);

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
            var dataService = new SectionDataEntryService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            if (ModelState.IsValid)
            {
                var result = dataService.DeleteAssessmentResult(studentResult);

                return ProcessResultStatus(result);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
        [Route("GetAssessmentResults")]
        [HttpPost]
        public async Task<OutputDto_StudentAssessmentResults> GetAssessmentResults([FromBody]InputDto_AssessmentSectionBenchmark input)
        {
            var dataService = new SectionDataEntryService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            if (input.AssessmentId == 0)
            {
                var user = ((ClaimsIdentity)User.Identity).Claims.FirstOrDefault(x => x.Type == "preferred_username").Value;
                Log.Error("Received an Assesment ID of 0 in GetAssessmentResults from user: {0}", user);

            }
            var result = await dataService.GetAssessmentResults(input);

            return result;
        }
        [Route("GetHFWSingleAssessmentResult")]
        [HttpPost]
        public IHttpActionResult GetHFWSingleAssessmentResult([FromBody]InputDto_StudentEditHFWAssessmentResult input)
        {
            var dataService = new SectionDataEntryService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetHFWSingleAssessmentResult(input);

            return this.ProcessResultStatus(result);
        }
        [Route("SaveHFWAssessmentResult")]
        [HttpPost]
        public IHttpActionResult SaveHFWAssessmentResult([FromBody]InputDto_SaveHFWAssessmentResult studentResult)
        {
            var dataService = new SectionDataEntryService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            if (ModelState.IsValid)
            {
                dataService.SaveHFWAssessmentResult(studentResult);

                // loop over fields and save data
                return Ok();
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
    }
}
