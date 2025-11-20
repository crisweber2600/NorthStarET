using System;
using NorthStar4.PCL.Entity;
using System.Collections.Generic;
using System.Linq;
using NorthStar4.PCL.DTO;
using NorthStar.EF6;
using EntityDto.DTO.Admin.Simple;
using EntityDto.DTO.Assessment;
using NorthStar4.API.Infrastructure;
using EntityDto.DTO;
using EntityDto.DTO.Misc;
using EntityDto.DTO.Reports.ObservationSummary;
using System.Diagnostics;
using System.Web.Http;
using System.Security.Claims;
using NorthStar4.CrossPlatform.DTO.Reports.ObservationSummary;
using EntityDto.DTO.Admin.District;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace NorthStar4.api
{
    [RoutePrefix("api/Assessment")]
    [Authorize]
    public class AssessmentController : NSBaseController
    {
        //private readonly DistrictContext _dbContext;
        //private AssessmentDataService dataService = null;


        //[HttpGet("lookupfields")]
        //public List<string> GetLookupFields()
        //{
        //	//throw new InvalidOperationException("Ghost in the machine!");
        //	return _dbContext.LookupFields.Select(p => p.FieldName).Distinct().ToList();
        //}

        [Route("GetDistrictList")]
        [HttpGet]
        public OutputDto_District GetDistrictList()
        {
            // add security check so that only I can get this
            if (!IsSA(((ClaimsIdentity)User.Identity)))
            {
                return new OutputDto_District() { Status = new OutputDto_Status() { StatusCode = EntityDto.DTO.Admin.Simple.StatusCode.AccessDenied, StatusMessage = "Sorry, but you do not have access to this data" }  };
            }

            var dataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);

            var result = dataService.GetDistrictList();

            return result;
        }

        [Route("CopyAssessmentToNewDistrict")]
        [HttpPost]
        public OutputDto_SuccessAndStatus CopyAssessmentToNewDistrict([FromBody]InputDto_CopyAssessmentToDistrict input)
        {
            // add security check so that only I can get this
            if (!IsSA(((ClaimsIdentity)User.Identity)))
            {
                return new OutputDto_SuccessAndStatus() { Status = new OutputDto_Status() { StatusCode = EntityDto.DTO.Admin.Simple.StatusCode.AccessDenied, StatusMessage = "Sorry, but you do not have access to this data" } };
            }

            var dataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);

            var result = dataService.CopyAssessmentToNewDistrict(input);

            return result;
        }

        [Route("GetLookupFieldsForAssessment/{assessmentId:int}")]
        [HttpGet]
		public List<IndexedLookupList> GetLookupFieldsForAssessment(int assessmentId)
		{
            var dataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);

            var result = dataService.GetLookupFieldsForAssessments(assessmentId.ToString());

            return result;
            //         var fields = _dbContext.AssessmentFields.Where(p => p.AssessmentId == assessmentId && p.FieldType == "DropdownFromDB").ToList();

            //         List <IndexedLookupList> lookupFields = new List<IndexedLookupList>();
            //foreach (var field in fields)
            //{
            //	if (!lookupFields.Any(p => p.LookupColumnName == field.LookupFieldName))
            //	{
            //		var fieldValues = _dbContext.LookupFields.Where(p => p.FieldName == field.LookupFieldName).ToList();
            //                 lookupFields.Add(new IndexedLookupList() { LookupColumnName = field.LookupFieldName, LookupFields = fieldValues});
            //	}
            //}
            //return lookupFields;
        }
        [Route("GetLookupFieldsForAssessments/{assessmentIds}")]
        [HttpGet]
		public List<IndexedLookupList> GetLookupFieldsForAssessments(string assessmentIds)
		{

            var dataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);

            var result = dataService.GetLookupFieldsForAssessments(assessmentIds);

            return result;
        }

        //public class IndexedLookupList
        //{
        //	public IndexedLookupList()
        //	{
        //		LookupFields = new List<AssessmentLookupField>();
        //	}
        //	public string LookupColumnName { get; set; }
        //	public List<AssessmentLookupField> LookupFields { get; set; } 
        //}

        // TODO: Uncomment this later.  is used by the assesmentField directive
        //[HttpGet("GetLookupField/{fieldName}")]
        //public IEnumerable<AssessmentLookupField> GetLookupField(string fieldName)
        //{
        //	//throw new InvalidOperationException("Ghost in the machine!");
        //	return _dbContext.LookupFields.Where(p => p.FieldName == fieldName).OrderBy(p => p.SortOrder);
        //}

        [Route("GetFieldsForAssessment")]
        [HttpPost]
        public IHttpActionResult GetFieldsForAssessment([FromBody]InputDto_GetAssessmentFields input)
        {
            var dataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetFieldsForAssessment(input);

            return ProcessResultStatus(result);
        }

        [Route("GetGroupsForAssessment")]
        [HttpPost]
        public IHttpActionResult GetGroupsForAssessment([FromBody]InputDto_GetAssessmentFieldGroups input)
        {
            var dataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetGroupsForAssessment(input);

            return ProcessResultStatus(result);
        }

        [Route("GetAssessmentResults/{assessmentId:int}/{classId:int}/{benchmarkDateId:int}")]
        [HttpGet]
        public OutputDto_StudentAssessmentResults GetAssessmentResults(int assessmentId, int classId, int benchmarkDateId)
        {
            var dataService = new SectionReportService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetAssessmentResults(assessmentId,classId, benchmarkDateId);

            return result;
        }

        [Route("GetBenchmarkLevel/{fpvalueId:int}/{accuracy:int}/{compscore:int}")]
        [HttpGet]
		public string GetBenchmarkLevel(int fpvalueId=0, int accuracy=0, int compscore=0)
		{


			return "Hard";
		}

        [Route("{id:int}")]
        [HttpGet]
		public AssessmentDto Get(int id)
        {
            if (!IsSA(((ClaimsIdentity)User.Identity)))
            {
                throw new UnauthorizedAccessException();
            }

            var dataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetAssessmentById(id);

            return result;
        }


        [HttpPost]
        public IHttpActionResult Post([FromBody]InputDto_Assessment assessment)
        {
            if (!IsSA(((ClaimsIdentity)User.Identity)))
            {
                return Unauthorized();
            }

            var dataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            if (ModelState.IsValid)
            {
                var result = dataService.SaveAssessment(assessment);

                return  Ok(); ;
            }
            return BadRequest(ModelState);
        }

        [Route("Delete")]
        [HttpPost]
        public IHttpActionResult Delete([FromBody]InputDto_SimpleId input)
        {
            if (!IsSA(((ClaimsIdentity)User.Identity)))
            {
                return Unauthorized();
            }

            var dataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.DeleteAssessment(input.Id);

            return ProcessResultStatus(result);
        }

        [Route("GetFilteredObservationSummary")]
        [HttpPost]
        public OutputDto_ObservationSummaryClass GetFilteredObservationSummary([FromBody]InputDto_GetFilteredObservationSummaryOptions input)
        {
            var obsDataService = new ObservationSummaryDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = obsDataService.GetFilteredObservationSummary(input);

            return result;
        }

        [Route("GetClassObservationSummaryMultiple")]
        [HttpPost]
        public OutputDto_ObservationSummaryClassMultiple GetClassObservationSummaryMultiple([FromBody]InputDto_GetSectionMultipleObservationSummary input)
        {
            var obsDataService = new ObservationSummaryDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = obsDataService.GetClassObservationSummaryMultiple(input);

            return result;
        }

        [Route("GetClassObservationSummaryMultipleColumns")]
        [HttpPost]
        public OutputDto_ObservationSummaryClassMultiple GetClassObservationSummaryMultipleColumns([FromBody]InputDto_GetSectionMultipleObservationSummary input)
        {
            var obsDataService = new ObservationSummaryDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = obsDataService.GetClassObservationSummaryMultiple(input);

            return result;
        }

        [Route("CreateHRISSentence")]
        [HttpPost]
        public OutputDto_SuccessAndStatus CreateHRISSentence([FromBody]InputDto_CreateHRISWSentence input)
        {

            // add security check so that only I can get this
            if (!IsSA(((ClaimsIdentity)User.Identity)))
            {
                return new OutputDto_SuccessAndStatus() { Status = new OutputDto_Status() { StatusCode = EntityDto.DTO.Admin.Simple.StatusCode.AccessDenied, StatusMessage = "Sorry, but you do not have access to this data" } };
            }
            var dataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.CreateHRISSentence(input);

            return result;
        }

        [Route("GetClassObservationSummary/{classId:int}/{testduedateId:int}")]
        [HttpGet]
        public OutputDto_ObservationSummaryClass GetClassObservationSummary(int classId, int testduedateId)
        {
            var asmtDataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var obsDataService = new ObservationSummaryDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = obsDataService.GetClassObservationSummary(classId, testduedateId);
            //var lookupLists = asmtDataService.GetLookupFieldsForAssessments(assessmentIds);

            //result.LookupLists = lookupLists;
            return result;
        }
        [Route("GetAllLookupFields")]
        [HttpGet]
        public List<IndexedLookupList> GetAllLookupFields()
        {
            Console.WriteLine("Arrived in GetAllLookupFields");
            Trace.WriteLine("TRACED: GetAllLookupFields");
            
            try {
                var dataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
                var lookupLists = dataService.GetLookupFieldsForAllAssessments();

                return lookupLists;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception thrown in GetAllLookupFields: " + ex.Message);
                Console.WriteLine("Stack Trace: " + ex.StackTrace);

                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                }
                Trace.Write("TRACED: Exception thrown in GetAllLookupFields: " + ex.Message);
                return null;
            }
        }
        [Route("GetTeamMeetingObservationSummary")]
        [HttpPost]
        public OutputDto_ObservationSummaryClass GetTeamMeetingObservationSummary([FromBody] InputDto_ObservationSummaryTeamMeeting input)
        {
            var asmtDataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var obsDataService = new ObservationSummaryDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            // get assessmentids from personalsettings... dont need to pass this in

            var result = obsDataService.GetTeamMeetingObservationSummary(input);
            var lookupLists = asmtDataService.GetLookupFieldsForAssessments("1,3,8,9");

            result.LookupLists = lookupLists;
            return result;
        }
        [Route("GetTeamMeetingAttendObservationSummary")]
        [HttpPost]
        public OutputDto_ObservationSummaryClass GetTeamMeetingAttendObservationSummary([FromBody] InputDto_ObservationSummaryTeamMeeting input)
        {
            // get assessmentids from personalsettings... dont need to pass this in
            var asmtDataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var obsDataService = new ObservationSummaryDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);

            var result = obsDataService.GetTeamMeetingObservationSummary(input);
         //   var lookupLists = asmtDataService.GetLookupFieldsForAssessments("1,3,8,9");
            var interventionGroupsForStudents = obsDataService.GetInterventionGroupsForTeamMeetingStudents(input);

           // result.LookupLists = lookupLists;
            result.InterventionsByStudent = interventionGroupsForStudents;
            return result;
        }
        [Route("simplecopy")]
        [HttpPost]
        public OutputDto_SuccessAndStatus SimpleCopy([FromBody] InputDto_SimpleId input)
        {
            var dataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.SimpleCopy(input);
            return result;
        }
        [Route("copyasinterventiontest")]
        [HttpPost]
        public OutputDto_SuccessAndStatus CopyAsInterventionTest([FromBody] InputDto_SimpleId input)
        {
            var dataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.CopyAsInterventionTest(input);
            return result;
        }

        //      [HttpGet("GetFilterOptions")]
        //public OutputDto_FilterOptions GetFilterOptions()
        //{
        //          // get default school year
        //          var defaultYear = GetDefaultYear();

        //          // get values from Cookie for preselected options


        //	var schools = _dbContext.Schools; // get only the schools the user has access to
        //	var tdds = _dbContext.TestDueDates.Where(p => p.SchoolStartYear == defaultYear).ToList();
        //	var schoolYears = _dbContext.SchoolYears;

        //	var options =  new OutputDto_FilterOptions()
        //	       {
        //				Schools = Mapper.Map<List<SchoolDto>>(schools.OrderBy(p => p.Name).ToList()),
        //				 TestDueDates = Mapper.Map<List<TestDueDateDto>>(tdds.OrderBy(p => p.DueDate).ToList()),
        //				 SchoolYears = Mapper.Map<List<SchoolYearDto>>(schoolYears.OrderBy(p => p.SchoolStartYear).ToList())
        //	       };

        //          return options;
        //}

        // TODO: move to helper library

        //      [HttpGet("GetUpdatedFilterOptions/{option}/{schoolYear:int}/{schoolId:int}/{gradeId:int}/{teacherId:int}/{classId:int}/{studentId:int}")]
        //public OutputDto_FilterOptions GetUpdatedFilterOptions(string option, int schoolYear, int schoolId, int gradeId, int teacherId, int classId, int studentId)
        //{
        // //         var currentUserName = Utilities.GetUserEmail(User);// User.Claims.Single(x => x.Type == "preferred_username").Value;

        // //         // Send 
        // //         var dataService = new NorthStarDataService(currentUserName);
        // //         var result = dataService.GetSpellingInventorySectionReport(assessmentId, classId, benchmarkDateId);


        // //         OutputDto_FilterOptions options = new OutputDto_FilterOptions();

        //	//switch (option)
        //	//{
        //	//	case "section":
        //	//		var sectionStudents =
        //	//			_dbContext.StudentSections
        //	//				.Where(p => p.ClassID == classId)
        //	//				.ToList();
        //	//		var studentsOnly = new List<Student>();
        //	//		foreach (var studentSection in sectionStudents)
        //	//		{
        //	//			var student = _dbContext.Students.FirstOrDefault(p =>
        //	//				p.Id == studentSection.StudentID && p.IsActive != false
        //	//				);

        //	//			if (student != null)
        //	//			{
        //	//				studentsOnly.Add(student);
        //	//			}
        //	//		}

        //	//		options.Students = Mapper.Map<List<StudentDto>>(studentsOnly.OrderBy(p => p.LastName).ToList());
        //	//		break;
        //	//	case "teacher":
        //	//		var teacherSections =
        //	//			_dbContext.StaffSections
        //	//				.Where(
        //	//					p =>
        //	//						p.StaffID == teacherId)
        //	//				.ToList();
        //	//		var sectionsOnly = new List<Section>();
        //	//		foreach (var staffSection in teacherSections)
        //	//		{
        //	//			var section = _dbContext.Sections.FirstOrDefault(p =>
        //	//				p.Id == staffSection.ClassID &&
        //	//				p.SchoolStartYear == schoolYear &&
        //	//				p.SchoolID == schoolId &&
        //	//				p.IsInterventionGroup == false &&
        //	//				(p.GradeID == gradeId || gradeId == 0 || gradeId == -1));

        //	//			if (section != null)
        //	//			{
        //	//				sectionsOnly.Add(section);
        //	//			}
        //	//		}

        //	//		options.Sections = Mapper.Map<List<SectionDto>>(sectionsOnly.OrderBy(p => p.Name).ToList());
        //	//		break;
        //	//	case "grade":

        //	//		var sectionsThatMatch = _dbContext.Sections
        //	//			.Where(
        //	//				p =>
        //	//					p.SchoolStartYear == schoolYear &&
        //	//					p.IsInterventionGroup == false &&
        //	//					p.SchoolID == schoolId &&
        //	//					p.GradeID == gradeId).Select(p => p.Id).ToList();

        //	//		// now get the staffsections for these
        // //                 var gradeSections =
        //	//			_dbContext.StaffSections
        //	//				.Where(
        //	//					p =>
        //	//						sectionsThatMatch.Contains(p.ClassID))
        //	//				.ToList();

        //	//		// now get the staff
        //	//		var staffList1 = new List<Staff>();
        //	//		foreach (var staffSection in gradeSections)
        //	//		{
        //	//			var teacherMatch = _dbContext.Staffs.FirstOrDefault(p => p.Id == staffSection.StaffID);

        //	//			if (teacherMatch != null)
        //	//			{
        //	//				staffList1.Add(teacherMatch);
        //	//			}
        //	//		}
        //	//		options.Teachers = Mapper.Map<List<StaffDto>>(staffList1.OrderBy(p => p.LastName).ToList());
        //	//		break;
        //	//	case "schoolyear":
        // //                 var benchdates = _dbContext.TestDueDates.Where(p =>
        // //                     p.SchoolStartYear == schoolYear
        // //                     ).ToList();

        // //                 options.TestDueDates = Mapper.Map<List<TestDueDateDto>>(benchdates);
        // //                 break;
        // //             case "school":
        //	//		//options.Grades = _dbContext.Sections.Where(p => 
        //	//		//p.SchoolID == schoolId && 
        //	//		//p.SchoolStartYear == schoolYear &&
        //	//		//p.IsInterventionGroup == false
        //	//		//).Select(p => p.Grade).Distinct().ToList();
        //	//		var sections = _dbContext.Sections.Where(p =>
        //	//			p.SchoolID == schoolId &&
        //	//			p.SchoolStartYear == schoolYear &&
        //	//			p.IsInterventionGroup == false
        //	//			).ToList();

        //	//		// he have the grade list now
        //	//		var gradeList = new List<Grade>();
        //	//		var staffList = new List<Staff>();
        //	//		foreach (var section in sections)
        //	//		{
        //	//			gradeList.Add(_dbContext.Grades.First(p => p.Id == section.GradeID));
        //	//			staffList.Add(_dbContext.Staffs.First(p => p.Id == section.StaffID));
        //	//		}
        //	//		options.Grades = Mapper.Map<List<GradeDto>>(gradeList.Distinct().OrderBy(p => p.GradeOrder).ToList());
        //	//		options.Teachers = Mapper.Map<List<StaffDto>>(staffList.OrderBy(p => p.LastName).Distinct().ToList());

        //	//		// lets get the list of teachers for these classes
        //	//		break;
        //	//}


        //	return null;
        //}

        [Route("GetUpdatedFilterOptions")]
        [HttpPost]
        public OutputDto_FilterOptions GetUpdatedFilterOptions([FromBody] InputDto_GetFilterOptions options)
        {
            var dataService = new FilterOptionsDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var assessmentDataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var currentUserName = Utilities.GetUserEmail(((ClaimsIdentity)User.Identity));

            var staffAssessments = assessmentDataService.GetStaffAssessments().StaffAssessments;
            // Send 
            var result = dataService.GetUpdatedFilterOptions(options, staffAssessments);

            return result;
        }
    }
}
