using System.Security.Claims;
using System.Web.Http;
using NorthStar4.CrossPlatform.DTO.Reports.StackedBarGraphs;
using NorthStar.EF6;
using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EntityDto.DTO.Reports.StackedBarGraphs;
using NorthStar4.API.Infrastructure;
using EntityDto.DTO;

namespace NorthStar4.api
{
    [RoutePrefix("api/StackedBarGraph")]
    [Authorize]
    public class StackedBarGraphController : NSBaseController
    {
        //private readonly DistrictContext _dbContext;



        //[HttpPost("GetStudentStackedBarGraph")]
        //public OutputDto_GetStackedBarGraphData GetStackedBarGraph([FromBody]InputDto_GetGroupedStackBarGraph input)
        //{
        //    var assessment = _dbContext.Assessments.First(p => p.Id == input.AssessmentID);

        //    var stackedBarGraphResults = _dbContext.GetGroupedStackBarGraphResults(input.SchoolStartYear, input.SectionID, input.SchoolID, input.GradeID, input.GroupingType, input.GroupingValue, assessment.Id, input.FieldToRetrieve, input.IsDecimalField, assessment.StorageTable);

        //    return new OutputDto_GetStackedBarGraphData()
        //    {
        //        Results = stackedBarGraphResults
        //    };
        //}
        [Route("GetStackedBarGraphGroupData")]
        [HttpPost]
        public IHttpActionResult GetStackedBarGraphGroupData([FromBody]InputDto_GetStackedBarGraphGroupingUpdatedOptions input)
        {
            // TODO: 1/10/2016 - fix this from List to a single DTO output
            var dataService = new StackedBarGraphReportService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetStackedBarGraphGroupData(input);

            return Ok(result); ;
        }
        [Route("GetStackedBarGraphComparisonData")]
        [HttpPost]
        public IHttpActionResult GetStackedBarGraphComparisonData([FromBody]InputDto_GetStackedBarGraphGroupingUpdatedOptions input)
        {
            var dataService = new StackedBarGraphReportService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetStackedBarGraphComparisonData(input);

            return ProcessResultStatus(result);
             
        }
        [Route("GetPLCPlanningReport")]
        [HttpPost]
        public IHttpActionResult GetPLCPlanningReport([FromBody]InputDto_GetFilterOptions filterInput)
        {
            var input = new InputDto_GetStackedBarGraphGroupingUpdatedOptions();
            var dataService = new StackedBarGraphReportService(((ClaimsIdentity)User.Identity), LoginConnectionString);

            input.AssessmentField = new PCL.Entity.AssessmentFieldDto() { AssessmentId = 1, DatabaseColumn = "FPValueID" };
            input.TestDueDateID = filterInput.BenchmarkDateId;
            input.SchoolStartYear = filterInput.SchoolYear;
            input.Schools.Add(new OutputDto_DropdownData() { id = filterInput.SchoolId });
            input.Grades.Add(new OutputDto_DropdownData() { id = filterInput.GradeId });
            var result = dataService.GetPLCPlanningReport(input);

            return ProcessResultStatus(result);

        }
        [Route("GetStackedBarGraphGroupSummary")]
        [HttpPost]
        public IHttpActionResult GetStackedBarGraphGroupSummary([FromBody]InputDto_GetStackedBarGraphGroupingSummaryUpdatedOptions input)
        {
            var dataService = new StackedBarGraphReportService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetStackedBarGraphGroupSummary(input);

            return ProcessResultStatus(result);
        }
        [Route("GetStackedBarGraphGroupHistoricalSummary")]
        [HttpPost]
        public IHttpActionResult GetStackedBarGraphGroupHistoricalSummary([FromBody]InputDto_GetStackedBarGraphGroupingSummaryUpdatedOptions input)
        {
            var dataService = new StackedBarGraphReportService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetStackedBarGraphGroupHistoricalSummary(input);

            return ProcessResultStatus(result);
        }

        [Route("GetStackedBarGraphGroupingUpdatedOptions")]
        [HttpPost]
        public IHttpActionResult GetStackedBarGraphGroupingUpdatedOptions([FromBody]InputDto_GetStackedBarGraphGroupingUpdatedOptions input)
        {
            var dataService = new StackedBarGraphReportService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetStackedBarGraphGroupingUpdatedOptions(input);

            return ProcessResultStatus(result);
            
        }
        [Route("getschoolsbygrade")]
        [HttpPost]
        public IHttpActionResult GetSchoolsByGrade([FromBody]InputDto_SchoolsByGrade input)
        {
            var dataService = new StackedBarGraphReportService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetSchoolsByGrade(input);

            return ProcessResultStatus(result);

        }
    }
}
