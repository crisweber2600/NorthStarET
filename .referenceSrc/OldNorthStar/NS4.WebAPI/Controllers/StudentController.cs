using System.Security.Claims;
using System.Web.Http;
using NorthStar.EF6;
using NorthStar4.PCL.Entity;
using System.Collections.Generic;
using System.Linq;
using NorthStar4.API.Infrastructure;
using NorthStar4.PCL.DTO;
using EntityDto.DTO.Admin.Student;
using Northstar.Core;
using EntityDto.DTO.Admin.Simple;
using System.Threading.Tasks;
using System.Web.Http.Cors;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace NorthStar4.api
{
    [RoutePrefix("api/Student")]
    [Authorize]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class StudentController : NSBaseController
    {
        //private readonly DistrictContext _dbContext;

        [Route("GetStudent/{Id:int}")]
        [HttpGet]
        public async Task<OutputDto_ManageStudent> GetStudent(int Id)
        {
            var dataService = new StudentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = await dataService.GetStudent(Id);

            return result;
        }

        [Route("GetStudentList")]
        [HttpPost]
        public OutputDto_GetStudentList GetStudentList([FromBody] InputDto_GetStudentList input)
        {
            var dataService = new StudentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetStudentList(input);

            return result;
        }

        [Route("GetSectionsForYear")]
        [HttpPost]
        public IHttpActionResult GetStudentList([FromBody] InputDto_StudentSchoolDto input)
        {
            var dataService = new StudentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetSectionsForYear(input);

            return ProcessResultStatus(result);
        }

        [Route("ConsolidateStudent")]
        [HttpPost]
        public IHttpActionResult ConsolidateStudent([FromBody]InputDto_ConsolidateStudent input)
        {
            var dataService = new StudentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.ConsolidateStudent(input);

            return ProcessResultStatus(result);
        }

        [Route("ConsolidateStudentServices")]
        [HttpPost]
        public IHttpActionResult ConsolidateStudentServices([FromBody]InputDto_ConsolidateStudentServices input)
        {
            var dataService = new StudentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.ConsolidateStudentServices(input);

            return ProcessResultStatus(result);
        }

        [Route("MoveStudent")]
        [HttpPost]
        public IHttpActionResult MoveStudent([FromBody] InputDto_MoveStudent input)
        {
            var dataService = new StudentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.MoveStudent(input);

            return ProcessResultStatus(result);
        }
        [Route("quicksearchstudent")]
        [HttpGet]
        public List<StudentQuickSearchResult> QuickSearchStudent(string searchString, bool disableInactiveStudents)
        {
            var dataService = new StudentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetStudentQuickSearchResults(searchString, disableInactiveStudents);

            return result;
        }
        [Route("quicksearchstudentdetailed")]
        [HttpGet]
        public List<StudentDetailedQuickSearchResult> GetStudentDetailedQuickSearchResults(string searchString, bool disableInactiveStudents)
        {
            var dataService = new StudentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetStudentDetailedQuickSearchResults(searchString, disableInactiveStudents);

            return result;
        }

        [Route("quicksearchstudentdetailedcurrentyear")]
        [HttpGet]
        public List<StudentDetailedQuickSearchResult> GetStudentDetailedQuickSearchResultsCurrentYear(string searchString, bool disableInactiveStudents)
        {
            var dataService = new StudentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetStudentDetailedQuickSearchResultsCurrentYear(searchString, disableInactiveStudents);

            return result;
        }

        [Route("getstudentbyid")]
        [HttpPost]
        public StudentQuickSearchResult GetStudentById([FromBody]InputDto_SimpleId input)
        {
            var dataService = new StudentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetStudentById(input);

            return result;
        }

        [Route("savestudent")]
        [HttpPost]
        public IHttpActionResult SaveStudent([FromBody]OutputDto_ManageStudent student)
        {
            //if (ModelState.IsValid)
            //{
                var dataService = new StudentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
                var result = dataService.SaveStudent(student);

                return Ok(student);
            //}
            //throw new UserDisplayableException("There was an error while saving the Student.  Support has been notified.  Please try again later.", null);
        }
        [Route("GetStudentAttributeLookups")]
        [HttpGet]
        public OutputDto_StudentAttributeLookups GetStudentAttributeLookups()
        {
            var dataService = new StudentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetStudentAttributeLookups();

            return result;
        }
        [Route("GetStudentSpedLabelLookups")]
        [HttpGet]
        public List<OutputDto_DropdownData> GetStudentSpedLabelLookups(string searchString)
        {
            var dataService = new StudentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetStudentSpedLabelLookups(searchString);

            return result;
        }
        [Route("GetStudentServices")]
        [HttpGet]
        public List<StudentServiceDto> GetStudentServices(string searchString)
        {
            var dataService = new StudentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetStudentServices(searchString);

            return result;
        }

        [Route("deletestudent")]
        [HttpPost]
        public IHttpActionResult DeleteStudent([FromBody]InputDto_SimpleId input)
        {
            var dataService = new StudentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.DeleteStudent(input.Id);

            return ProcessResultStatus(result);
        }
        [Route("CanRemoveStudentSchool")]
        [HttpPost]
        public OutputDto_Success CanRemoveStudentSchool([FromBody] InputDto_CanRemoveStudentSchool input)
        {
            var dataService = new StudentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.CanRemoveStudentSchool(input);

            return result;
        }
        [Route("IsStudentIDUnique")]
        [HttpPost]
        public OutputDto_SuccessAndStatus IsStudentIDUnique([FromBody]InputDto_StudentIdAndIdentifier input)
        {
            var dataService = new StudentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.IsStudentIDUnique(input);

            return result;
        }
        [Route("GetStudentInterventions")]
        [HttpPost]
        public IHttpActionResult GetStudentInterventions([FromBody]InputDto_SimpleId input)
        {
            var dataService = new StudentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);

            var interventions = dataService.GetStudentInterventionsForReport(input.Id);

            var result = new OutputDto_GetStudentInterventions { Interventions = interventions };

            return ProcessResultStatus(result);
        }

        [Route("savenote")]
        [HttpPost]
        public OutputDto_Success SaveNote([FromBody] InputDto_SaveStudentNote input)
        {
            var dataService = new StudentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.SaveNote(input);

            return result;

        }
        [Route("deletenote")]
        [HttpPost]
        public OutputDto_Success DeleteNote([FromBody] InputDto_SimpleId input)
        {
            var dataService = new StudentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.DeleteNote(input);

            return result;

        }
        [Route("getnotesforstudent")]
        [HttpPost]
        public OutputDto_StudentNotes GetNotesForStudent([FromBody] InputDto_SimpleId input)
        {
            var dataService = new StudentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetNotesForStudent(input, Request.Headers.Authorization.Parameter);

            return result;

        }

    }
}
