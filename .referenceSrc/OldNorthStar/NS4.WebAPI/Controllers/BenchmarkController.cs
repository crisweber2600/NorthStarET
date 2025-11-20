using EntityDto.DTO.Assessment.Benchmarks;
using EntityDto.DTO.Personal;
using NorthStar.EF6;
using NorthStar4.API.Infrastructure;
using System.Security.Claims;
using System.Web.Http;

namespace NorthStar4.API.api
{
    [RoutePrefix("api/Benchmark")]
    [Authorize]
    public class BenchmarkController : NSBaseController
    {
    

        [Route("GetSystemBenchmarks")]
        [HttpPost]
        public IHttpActionResult GetSystemBenchmarks([FromBody]InputDto_GetBenchmarks input)
        {
            var dataService = new BenchmarkDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetSystemBenchmarks(input);

            return ProcessResultStatus(result);
        }

        [Route("SaveSystemBenchmark")]
        [HttpPost]
        public IHttpActionResult SaveSystemBenchmark([FromBody]InputDto_AssessmentBenchmarkDto input)
        {
            var dataService = new BenchmarkDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.SaveSystemBenchmark(input.Benchmark);

            return ProcessResultStatus(result);
        }

        [Route("DeleteSystemBenchmark")]
        [HttpPost]
        public IHttpActionResult DeleteSystemBenchmark([FromBody]InputDto_AssessmentBenchmarkDto input)
        {
            var dataService = new BenchmarkDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.DeleteSystemBenchmark(input.Benchmark);

            return ProcessResultStatus(result);
        }
        [Route("GetAssessmentsAndFields")]
        [HttpGet]
        public OutputDto_GetAssessmentsAndFieldsForUser GetAssessmentsAndFields()
        {
            var dataService = new BenchmarkDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetAssessmentsAndFields();

            return result;
        }

        [Route("GetInterventionAssessmentsAndFields")]
        [HttpGet]
        public OutputDto_GetAssessmentsAndFieldsForUser GetInterventionAssessmentsAndFields()
        {
            var dataService = new BenchmarkDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetInterventionAssessmentsAndFields();

            return result;
        }

        #region District Benchmarks
        [Route("GetDistrictYearlyAssessmentBenchmarks")]
        [HttpPost]
        public IHttpActionResult GetDistrictYearlyAssessmentBenchmarks([FromBody]InputDto_GetBenchmarks input)
        {
            var dataService = new BenchmarkDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetDistrictYearlyAssessmentBenchmarks(input);

            return ProcessResultStatus(result);
        }

        [Route("SaveDistrictYearlyAssessmentBenchmark")]
        [HttpPost]
        public IHttpActionResult SaveDistrictYearlyAssessmentBenchmark([FromBody]InputDto_DistrictYearlyAssessmentBenchmarkDto input)
        {
            var dataService = new BenchmarkDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.SaveDistrictYearlyAssessmentBenchmark(input.Benchmark);

            return ProcessResultStatus(result);
        }

        [Route("DeleteDistrictYearlyAssessmentBenchmark")]
        [HttpPost]
        public IHttpActionResult DeleteDistrictYearlyAssessmentBenchmark([FromBody]InputDto_DistrictYearlyAssessmentBenchmarkDto input)
        {
            var dataService = new BenchmarkDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.DeleteDistrictYearlyAssessmentBenchmark(input.Benchmark);

            return ProcessResultStatus(result);
        }
        [Route("GetDistrictYearlyAssessmentsAndFields")]
        [HttpGet]
        public OutputDto_GetAssessmentsAndFieldsForUser GetDistrictYearlyAssessmentsAndFields()
        {
            var dataService = new BenchmarkDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetDistrictYearlyAssessmentsAndFields();

            return result;
        }

        [Route("GetDistrictBenchmarks")]
        [HttpPost]
        public IHttpActionResult GetDistrictBenchmarks([FromBody]InputDto_GetBenchmarks input)
        {
            var dataService = new BenchmarkDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetDistrictBenchmarks(input);

            return ProcessResultStatus(result);
        }

        [Route("SaveDistrictBenchmark")]
        [HttpPost]
        public IHttpActionResult SaveDistrictBenchmark([FromBody]InputDto_DistrictBenchmarkDto input)
        {
            var dataService = new BenchmarkDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.SaveDistrictBenchmark(input.Benchmark);

            return ProcessResultStatus(result);
        }

        [Route("DeleteDistrictBenchmark")]
        [HttpPost]
        public IHttpActionResult DeleteDistrictBenchmark([FromBody]InputDto_DistrictBenchmarkDto input)
        {
            var dataService = new BenchmarkDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.DeleteDistrictBenchmark(input.Benchmark);

            return ProcessResultStatus(result);
        }
        [Route("GetDistrictAssessmentsAndFields")]
        [HttpGet]
        public OutputDto_GetAssessmentsAndFieldsForUser GetDistrictAssessmentsAndFields()
        {
            var dataService = new BenchmarkDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetDistrictAssessmentsAndFields();

            return result;
        }
        #endregion
    }
}
