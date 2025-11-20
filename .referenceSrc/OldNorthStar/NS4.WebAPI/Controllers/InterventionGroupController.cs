using System;
using NorthStar4.PCL.Entity;
using System.Collections.Generic;
using System.Linq;
using NorthStar4.CrossPlatform.DTO.Admin.InterventionGroup;
using NorthStar4.CrossPlatform.Entity;
using NorthStar4.Infrastructure;
using NorthStar4.PCL.DTO;
using NorthStar.EF6;
using System.Security.Claims;
using System.Web.Http;
using NorthStar4.API.Infrastructure;
using EntityDto.DTO.Admin.InterventionGroup;
using EntityDto.DTO.Admin.Student;

//For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace NorthStar4.api
{
    [RoutePrefix("api/InterventionGroup")]
    [Authorize]
    public class InterventionGroupController : NSBaseController
    {

        //private NorthStarDataService dataService = null;


        [Route("saveDate")]
        [HttpPost]
        public IHttpActionResult SaveDate([FromBody]InputDto_SaveStudentInterventionStartEnd startEnd)
        {
            var dataService = new InterventionGroupDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.SaveDate(startEnd);

            return ProcessResultStatus(result);
        }
        [Route("addStudentToGroup")]
        [HttpPost]
        public IHttpActionResult AddStudentToGroup([FromBody] InputDto_AddStudentToInterventionGroup input)
        {
            var dataService = new InterventionGroupDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.AddStudentToGroup(input);

            return ProcessResultStatus(result);
        }
        [Route("moveStudentToGroup")]
        [HttpPost]
        public IHttpActionResult MoveStudentToGroup([FromBody] InputDto_MoveStudentToInterventionGroup input)
        {
            var dataService = new InterventionGroupDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.MoveStudentToGroup(input);

            return ProcessResultStatus(result);
        }
        [Route("removeStudentFromGroup")]
        [HttpPost]
        public IHttpActionResult RemoveStudentFromGroup([FromBody] InputDto_RemoveStudentFromInterventionGroup input)
        {
            var dataService = new InterventionGroupDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.RemoveStudentFromGroup(input);

            return ProcessResultStatus(result);
        }
        [Route("saveSingleAttendance")]
        [HttpPost]
        public IHttpActionResult SaveSingleAttendance([FromBody] InputDto_SaveSingleAttendance input)
        {
            var dataService = new InterventionGroupDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.SaveSingleAttendance(input);

            return ProcessResultStatus(result);
        }
        [Route("applyStatusNotes")]
        [HttpPost]
        public IHttpActionResult ApplyStatusNotes([FromBody] InputDto_ApplyStatusNotes input)
        {
            var dataService = new InterventionGroupDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.ApplyStatusNotes(input);

            return ProcessResultStatus(result);
        }
        [Route("getweeklyattendance")]
        [HttpPost]
        public IHttpActionResult GetWeeklyAttendanceResults([FromBody]InputDto_GetWeeklyAttendanceResults input)
        {
            var dataService = new InterventionGroupDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetWeeklyAttendanceResults(input);

            return ProcessResultStatus(result);
        }
        [Route("studentquicksearch")]
        [HttpPost]
        public IHttpActionResult StudentQuickSearch([FromBody]InputDto_StudentQuickSearch input)
        {
            var dataService = new InterventionGroupDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.StudentQuickSearch(input);

            return ProcessResultStatus(result);
        }

        //[HttpGet("getstudentfordropdown")]
        //public OutputDto_StudentQuickSearch StudentQuickSearch(int id)
        //{

        //   // eventually, this will be a stored proc
        //   var result = _dbContext.Students.First(p => p.Id == id);

        //    return new OutputDto_StudentQuickSearch()
        //    {
        //        StudentId = result.Id,
        //        FirstName = result.FirstName,
        //        LastName = result.LastName
        //    };
        //}


        [Route("getbyyearschoolstaff")]
        [HttpPost]
        public IHttpActionResult GetGroupsByYearSchoolStaff([FromBody] InputDto_GetInterventionGroups input)
        {

            var dataService = new InterventionGroupDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetGroupsByYearSchoolStaff(input);

            return ProcessResultStatus(result);
        }
        [Route("getgroup")]
        [HttpPost]
        public IHttpActionResult GetGroup([FromBody]InputDto_SimpleId input)
        {
            var dataService = new InterventionGroupDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetGroup(input);

            return ProcessResultStatus(result);
        }
        [Route("getstafffordropdown")]
        [HttpGet]
        public OutputDto_DropdownData GetStaffForDropdown(InputDto_SimpleId input)
        {
            var dataService = new InterventionGroupDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetStaffForDropdown(input.Id);

            return result;
       
        }
        [Route("getinterventionbyid")]
        [HttpGet]
        public OutputDto_DropdownData GetInterventionbyId(InputDto_SimpleId input)
        {
            var dataService = new InterventionGroupDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetInterventionbyId(input.Id);

            return result;
        }
        [Route("getinterventionistsfordropdown")]
        [HttpGet]
        public List<OutputDto_DropdownData> GetInterventionistsForDropdown(int pageNo, string searchString)
        {
            var dataService = new InterventionGroupDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetInterventionistsForDropdown(pageNo, searchString);

            return result;
        }
        [Route("getinterventionsfordropdown")]
        [HttpGet]
        public List<OutputDto_DropdownData> GetInterventionsForDropdown(string searchString)
        {
            var dataService = new InterventionGroupDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetInterventionsForDropdown(searchString);

            return result;
        }
        [Route("getcointerventionistsforgroup")]
        [HttpGet]
        public List<OutputDto_DropdownData> GetCoInterventionistsForGroup(List<int> ids)
        {
            var dataService = new InterventionGroupDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetCoInterventionistsForGroup(ids);

            return result;
        }
        [Route("getstudentsections/{Id:int}")]
        [HttpGet]
        public IEnumerable<InterventionGroupStudentDto> GetStudentSections(int Id)
        {
            var dataService = new InterventionGroupDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetStudentSections(Id);

            return result;
        }
        [Route("GetInterventionGroupStints")]
        [HttpPost]
        public IHttpActionResult GetInterventionGroupStints([FromBody] InputDto_GetInterventionGroupStints input)
        {
            var dataService = new InterventionGroupDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetInterventionGroupStints(input);
            return ProcessResultStatus(result);
        }
        [Route("GetInterventionGroup")]
        [HttpPost]
        public IHttpActionResult GetInterventionGroup([FromBody]InputDto_SimpleId input)
        {
                    var dataService = new InterventionGroupDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
                    var result = dataService.GetInterventionGroup(input);
                    return ProcessResultStatus(result);
        }
        [Route("CanStintBeDeleted")]
        [HttpPost]
        public IHttpActionResult CanStintBeDeleted([FromBody]InputDto_SimpleId input)
        {
            var dataService = new InterventionGroupDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.CanStintBeDeleted(input.Id);
            return ProcessResultStatus(result);
        }
        [Route("SaveInterventionGroup")]
        [HttpPost]
        public IHttpActionResult SaveInterventionGroup([FromBody]OuputDto_ManageInterventionGroup interventionGroup)
        {
            var dataService = new InterventionGroupDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.SaveInterventionGroup(interventionGroup);
            return ProcessResultStatus(result);
        }

        [Route("DeleteIntervention")]
        [HttpPost]
        public IHttpActionResult Delete([FromBody]InputDto_SimpleId input)
        {
            var dataService = new InterventionGroupDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.Delete(input.Id);
            return ProcessResultStatus(result);
        }
	
	}
}
