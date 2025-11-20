using EntityDto.DTO.Admin.Simple;
using EntityDto.DTO.Assessment;
//using Microsoft.AspNet.Authorization;
//using Microsoft.AspNet.Mvc;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.OptionsModel;
using NorthStar.EF6;
using NorthStar4.API.Infrastructure;
using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace NorthStar4.API.api
{
    [RoutePrefix("api/AssessmentAvailability")]
    [Authorize]
    public class AssessmentAvailabilityController : NSBaseController
    {
        //
        private AssessmentDataService dataService = null;

        public AssessmentAvailabilityController()
        {
          //  var user = User;
            //AppSettings = appSettings;
        }

        [Route("GetAssessmentList")]
        [HttpGet]
        public IHttpActionResult  GetAssessmentList()
        {

            dataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetAssessmentList();

            return ProcessResultStatus(result);
        }

        [Route("GetObservationSummaryAssessmentList")]
        [HttpGet]
        public IHttpActionResult GetObservationSummaryAssessmentList()
        {
            dataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetObservationSummaryAssessmentList();

            return ProcessResultStatus(result);
        }

        [Route("GetStudentAttributeList")]
        [HttpGet]
        public IHttpActionResult GetStudentAttributeList()
        {
            dataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetStudentAttributeList();

            return ProcessResultStatus(result);
        }

        [Route("GetObservationSummaryAssessmentFieldList")]
        [HttpPost]
        public IHttpActionResult GetObservationSummaryAssessmentFieldList([FromBody]InputDto_SimpleId input)
        {
            dataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetObservationSummaryAssessmentFieldList(input);

            return ProcessResultStatus(result);
        }

        [Route("UpdateObservationSummaryAssessmentVisibility")]
        [HttpPost]
        public IHttpActionResult UpdateObservationSummaryAssessmentVisibility([FromBody]InputDto_ObservationSummaryFieldVisibility input)
        {
            dataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.UpdateObservationSummaryAssessmentVisibility(input);

            return ProcessResultStatus(result);
        }

        [Route("UpdateStudentAttributeVisibility")]
        [HttpPost]
        public IHttpActionResult UpdateStudentAttributeVisibility([FromBody]OutputDto_StudentAttributes input)
        {
            dataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.UpdateStudentAttributeVisibility(input);

            return ProcessResultStatus(result);
        }

        [Route("UpdateObservationSummaryColumnVisibility")]
        [HttpPost]
        public IHttpActionResult UpdateObservationSummaryColumnVisibility([FromBody]InputDto_SimpleString input)
        {
            dataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.UpdateObservationSummaryColumnVisibility(input);

            return ProcessResultStatus(result);
        }

        [Route("UpdateObservationSummaryAssessmentFieldVisibility")]
        [HttpPost]
        public IHttpActionResult UpdateObservationSummaryAssessmentFieldVisibility([FromBody]InputDto_ObservationSummaryFieldVisibility input)
        {
            dataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.UpdateObservationSummaryAssessmentFieldVisibility(input);

            return ProcessResultStatus(result);
        }

        [Route("GetSchoolAssessments")]
        [HttpPost]
        public OutputDto_SchoolAssessments GetSchoolAssessments([FromBody]InputDto_SimpleId input)
        {
            dataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetSchoolAssessments(input);

            return result;
        }

        [Route("GetStaffAssessments")]
        [HttpPost]
        public OutputDto_StaffAssessments GetStaffAssessments()
        {
            dataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetStaffAssessments();

            return result;
        }

        [Route("UpdateAssessmentAvailability")]
        [HttpPost]
        public OutputDto_SuccessAndStatus UpdateAssessmentAvailability([FromBody]AssessmentDto input)
        {
            dataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.UpdateAssessmentAvailability(input);

            return result;
        }

        [Route("UpdateSchoolAssessmentAvailability")]
        [HttpPost]
        public OutputDto_SuccessAndStatus UpdateSchoolAssessmentAvailability([FromBody]SchoolAssessmentDto input)
        {
            dataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.UpdateSchoolAssessmentAvailability(input);

            return result;
        }

        [Route("UpdateStaffAssessmentAvailability")]
        [HttpPost]
        public OutputDto_SuccessAndStatus UpdateStaffAssessmentAvailability([FromBody]StaffAssessmentDto input)
        {
            dataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.UpdateStaffAssessmentAvailability(input);

            return result;
        }
    }
}
