using EntityDto.DTO.Assessment;
using EntityDto.DTO.Calendars;
using NorthStar.EF6;
using NorthStar4.API.Infrastructure;
using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace NorthStar4.API.api
{
    [RoutePrefix("api/Calendar")]
    [Authorize]
    public class CalendarController : NSBaseController
    {

        [Route("GetDistrictCalendar")]
        [HttpGet]
        public OutputDto_DistrictCalendarList GetDistrictCalendar()
        {
            var dataService = new SchoolAndDistrictDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetDistrictCalendar();

            return result;
        }

        [Route("SaveDistrictCalendarEvent")]
        [HttpPost]
        public IHttpActionResult SaveDistrictCalendarEvent([FromBody]DistrictCalendarDto item)
        {
            var dataService = new SchoolAndDistrictDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.SaveDistrictCalendarEvent(item);

            return ProcessResultStatus(result);
        }

        [Route("SaveSchoolCalendarEvent")]
        [HttpPost]
        public IHttpActionResult SaveSchoolCalendarEvent([FromBody]SchoolCalendarDto item)
        {
            var dataService = new SchoolAndDistrictDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.SaveSchoolCalendarEvent(item);

            return ProcessResultStatus(result);
        }

        [Route("DeleteDistrictCalendarEvent")]
        [HttpPost]
        public IHttpActionResult DeleteDistrictCalendarEvent([FromBody]DistrictCalendarDto item)
        {
            var dataService = new SchoolAndDistrictDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.DeleteDistrictCalendarEvent(item);

            return ProcessResultStatus(result);
        }
        [Route("DeleteSchoolCalendarEvent")]
        [HttpPost]
        public IHttpActionResult DeleteSchoolCalendarEvent([FromBody]SchoolCalendarDto item)
        {
            var dataService = new SchoolAndDistrictDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.DeleteSchoolCalendarEvent(item);

            return ProcessResultStatus(result);
        }

        [Route("GetSchoolCalendar")]
        [HttpPost]
        public OutputDto_SchoolCalendarList GetSchoolCalendar([FromBody]InputDto_SimpleId input)
        {
            var dataService = new SchoolAndDistrictDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetSchoolCalendar(input);

            return result;
        }


    }
}
