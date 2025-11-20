using EntityDto.DTO.Admin.InterventionGroup;
using NorthStar.EF6.DataService;
using NorthStar4.API.Infrastructure;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http;

namespace NorthStar4.API.api
{
    [RoutePrefix("api/InterventionDashboard")]
    [Authorize]
    public class InterventionDashboardController : NSBaseController
    {
        


        [Route("GetStintAttendanceSummary")]
        [HttpPost]
        public IHttpActionResult GetStintAttendanceSummary([FromBody] InputDto_GetStintAttendanceSummary input)
        {
            var dataService = new InterventionDashboardDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetStintAttendanceSummary(input);


            return ProcessResultStatus(result);
        }
    }
}
