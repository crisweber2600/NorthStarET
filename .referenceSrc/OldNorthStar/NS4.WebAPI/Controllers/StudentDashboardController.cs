using System.Security.Claims;
using System.Web.Http;
using NorthStar.EF6;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NorthStar4.API.Infrastructure;
using NorthStar4.PCL.DTO;
using NorthStar.EF6.DataService;
using EntityDto.DTO.Reports.StudentDashboard;

namespace NorthStar4.API.api
{
    [RoutePrefix("api/StudentDashboard")]
    [Authorize]
    public class StudentDashboardController : NSBaseController
    {
        //private readonly DistrictContext _dbContext;
        [Route("GetStudentObservationSummary")]
        [HttpPost]
        public IHttpActionResult GetStudentObservationSummary([FromBody]InputDto_GetStudentObservationSummary input)
        {
            var dataService = new StudentDashboardDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);

            var asmtDataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetStudentObservationSummary(input);
            //var lookupLists = asmtDataService.GetLookupFieldsForAssessments(input.AssessmentIds);

            //result.LookupLists = lookupLists;
            //return result;

            return ProcessResultStatus(result);
        }

       
    }
}
