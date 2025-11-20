using NorthStar4.CrossPlatform.DTO;
using NorthStar.EF6;
using System.Security.Claims;
using System.Web.Http;
using NorthStar4.API.Infrastructure;
using NorthStar4.PCL.DTO;
using EntityDto.DTO.Reports;

namespace NorthStar4.api
{
    [RoutePrefix("api/LineGraph")]
    [Authorize]
    public class LineGraphController : NSBaseController
    {
        //private readonly DistrictContext _dbContext;
        
        private LineGraphReportService dataService = null;

        [Route("GetStudentLineGraph")]
        [HttpPost]
        public IHttpActionResult GetLineGraph([FromBody]InputDto_GetStudentLineGraph input)
        {
            var dataService = new LineGraphReportService(((ClaimsIdentity)User.Identity), LoginConnectionString);

            var benchmarkedAssessmentResults = dataService.GetBenchmarkedAssessmentResults(input.AssessmentId, input.FieldToRetrieve, input.IsLookupColumn, input.StudentId, input.LookupFieldName);
            var benchmarkDatesForStudentAndAssessment = dataService.GetBenchmarkDatesForStudentAndAssessment(input.AssessmentId, input.StudentId, input.FieldToRetrieve);
            //var interventions = dataService.GetStudentInterventionsForReport(input.StudentId);
            var vScale = dataService.GetVScale(input.LookupFieldName);

            var result =  new OutputDto_GetStudentLineGraph()
            {
                BenchmarkDates = benchmarkDatesForStudentAndAssessment,
                Results = benchmarkedAssessmentResults.StudentResults,
                Fields = benchmarkedAssessmentResults.Fields,
                //Interventions = interventions,
                VScale = vScale
                //Fields = dataService.GetAssessmentAndFieldsForUser(input.AssessmentId)
            };

            return ProcessResultStatus(result);
        }

        [Route("GetStudentLineGraphFields")]
        [HttpPost]
        public IHttpActionResult GetStudentLineGraphFields([FromBody]InputDto_GetStudentLineGraphFields input)
        {
            var dataService = new LineGraphReportService(((ClaimsIdentity)User.Identity), LoginConnectionString);

            var result = dataService.GetStudentLineGraphFields(input);

            return ProcessResultStatus(result);
        }

        [Route("GetStudentIGLineGraph")]
        [HttpPost]
        public OutputDto_GetStudentInterventionGroupLineGraph GetStudentIGLineGraph([FromBody]InputDto_GetStudentInterventionGroupLineGraph input)
        {
            var dataService = new LineGraphReportService(((ClaimsIdentity)User.Identity), LoginConnectionString);

            var benchmarkedAssessmentResults = dataService.GetInterventionGroupAssessmentResults(input.AssessmentId, input.FieldToRetrieve, input.IsLookupColumn, input.StudentId, input.LookupFieldName, input.InterventionGroupId);
            var benchmarkDatesForStudentAndAssessment = dataService.GetBenchmarkDatesForIG(input.AssessmentId, input);
            //var interventions = dataService.GetStudentInterventionsForReport(input.StudentId);
            var vScale = dataService.GetVScale(input.LookupFieldName);

            return new OutputDto_GetStudentInterventionGroupLineGraph()
            {
                BenchmarkDates = benchmarkDatesForStudentAndAssessment,
                Results = benchmarkedAssessmentResults.StudentResults,
                Fields = benchmarkedAssessmentResults.Fields,
               // Interventions = interventions,
                VScale = vScale
            };
        }
    }
}
