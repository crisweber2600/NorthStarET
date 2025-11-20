using System.Security.Claims;
using System.Web.Http;
using NorthStar4.CrossPlatform.DTO;
using NorthStar4.CrossPlatform.DTO.Reports;
using NorthStar.EF6;
using NorthStar4.API.Infrastructure;

using EntityDto.DTO.Reports;
using EntityDto.DTO.Reports.LID;
using NorthStar4.PCL.DTO;
using System.Collections.Generic;
using EntityDto.DTO.Reports.HFW;
using NorthStar.EF6.DataService;
using NorthStar.Core.FileUpload;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;
using System.Threading.Tasks;

namespace NorthStar4.api
{
    [RoutePrefix("api/SectionReport")]
    [Authorize]
    public class SectionReportController : NSBaseController
    {
        private IPhotoManager photoManager;
        private string imageContainer = "images";

        public SectionReportController() 
        : this(new AzurePhotoManager(CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString), "interventiontools")) 
    {
        }


        public SectionReportController(IPhotoManager photoManager)
        {
            this.photoManager = photoManager;
        }

        //private NorthStarDataService dataService = null;
        [Route("GetSpellingInventorySectionReport")]
        [HttpPost]
        public IHttpActionResult GetSpellingInventorySectionReport([FromBody] InputDto_SectionReport_ByTdd input)
        {
            //var currentUserName = Utilities.GetUserEmail(User);// User.Claims.Single(x => x.Type == "preferred_username").Value;
            // Send 
            var dataService = new SectionReportService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetSpellingInventorySectionReport(input);



            return ProcessResultStatus(result);
        }

        [Route("GetHFWDetailReport")]
        [HttpPost]
        public IHttpActionResult GetHFWDetailReport([FromBody] InputDto_GetHFWDetailReport input)
        {
            //var currentUserName = Utilities.GetUserEmail(User);// User.Claims.Single(x => x.Type == "preferred_username").Value;
            // Send 
            var dataService = new SectionReportService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetHFWDetailReport(input);
            
            return ProcessResultStatus(result);
        }

        [Route("GetHFWMissingWordsReport")]
        [HttpPost]
        public IHttpActionResult GetHFWMissingWordsReport([FromBody] InputDto_GetHFWDetailReport input)
        {
            //var currentUserName = Utilities.GetUserEmail(User);// User.Claims.Single(x => x.Type == "preferred_username").Value;
            // Send 
            var dataService = new SectionReportService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetHFWMissingWordsReport(input);



            return ProcessResultStatus(result);
        }

        [Route("GetFPSectionReport")]
        [HttpPost]
        public IHttpActionResult GetFPSectionReport([FromBody] InputDto_BAS_SectionReport input)
        {
            var dataService = new SectionReportService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetFPSectionReport(input);

            return ProcessResultStatus(result);
        }
        [Route("GetKNTCSectionReport")]
        [HttpPost]
        public async Task<IHttpActionResult> GetKNTCSectionReport([FromBody] InputDto_SectionReport_ByTdd input)
        {
            var dataService = new SectionReportService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            //var fileService = new FileUploadDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = await dataService.GetKNTCSectionReport(input);

            return ProcessResultStatus(result);
        }
        [Route("GetWVSectionReport")]
        [HttpPost]
        public IHttpActionResult GetWVSectionReport([FromBody] InputDto_BAS_SectionReport input)
        {
            var dataService = new SectionReportService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetWVSectionReport(input);

            return ProcessResultStatus(result);
        }
        [Route("GetHRSIWSectionReport")]
        [HttpPost]
        public IHttpActionResult GetHRSIWSectionReport([FromBody] InputDto_BAS_SectionReport input)
        {
            var dataService = new SectionReportService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetHRSIWSectionReport(input);

            return ProcessResultStatus(result);
        }
        [Route("GetHRSIW2SectionReport")]
        [HttpPost]
        public IHttpActionResult GetHRSIW2SectionReport([FromBody] InputDto_BAS_SectionReport input)
        {
            var dataService = new SectionReportService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetHRSIW2SectionReport(input);

            return ProcessResultStatus(result);
        }
        [Route("GetHRSIW3SectionReport")]
        [HttpPost]
        public IHttpActionResult GetHRSIW3SectionReport([FromBody] InputDto_BAS_SectionReport input)
        {
            var dataService = new SectionReportService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetHRSIW3SectionReport(input);

            return ProcessResultStatus(result);
        }
        [Route("GetHRSIWFormForClass")]
        [HttpPost]
        public IHttpActionResult GetHRSIWFormForClass([FromBody] InputDto_SimpleId input)
        {
            var dataService = new SectionReportService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetHRSIWFormForClass(input);

            return ProcessResultStatus(result);
        }
        [Route("GetHRSIW2FormForClass")]
        [HttpPost]
        public IHttpActionResult GetHRSIW2FormForClass([FromBody] InputDto_SimpleId input)
        {
            var dataService = new SectionReportService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetHRSIW2FormForClass(input);

            return ProcessResultStatus(result);
        }
        [Route("GetHRSIW3FormForClass")]
        [HttpPost]
        public IHttpActionResult GetHRSIW3FormForClass([FromBody] InputDto_SimpleId input)
        {
            var dataService = new SectionReportService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetHRSIW3FormForClass(input);

            return ProcessResultStatus(result);
        }

        [Route("QuickSearchHFWRanges")]
        [HttpGet]
        public List<OutputDto_DropdownData> QuickSearchHFWRanges(string searchString)
        {
            var dataService = new SectionReportService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.QuickSearchHfwRanges(searchString);
            return result;
        }
        [Route("GetCAPSectionReport")]
        [HttpPost]
        public IHttpActionResult GetCAPSectionReport([FromBody] InputDto_BAS_SectionReport input)
        {
            var dataService = new SectionReportService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetCAPSectionReport(input);

            return ProcessResultStatus(result);
        }
        [Route("GetAVMRSectionReport")]
        [HttpPost]
        public IHttpActionResult GetAVMRSectionReport([FromBody] InputDto_BAS_SectionReport input)
        {
            var dataService = new SectionReportService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetAVMRSectionReport(input);

            return ProcessResultStatus(result);
        }

        [Route("GetAVMRSingleDateSectionReport")]
        [HttpPost]
        public IHttpActionResult GetAVMRSingleDateSectionReport([FromBody] InputDto_SectionReport_ByTdd input)
        {
            var dataService = new SectionReportService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetAVMRSingleDateSectionReport(input);

            return ProcessResultStatus(result);
        }

        [Route("GetAVMRSingleDateSectionReportDetail")]
        [HttpPost]
        public IHttpActionResult GetAVMRSingleDateSectionReportDetail([FromBody] InputDto_SectionReport_ByTdd input)
        {
            var dataService = new SectionReportService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetAVMRSingleDateSectionReportDetail(input);

            return ProcessResultStatus(result);
        }

        [Route("GetLIDSectionReport")]
        [HttpPost]
        public IHttpActionResult GetLIDSectionReport([FromBody] InputDto_LetterIDReport input)
        {
            var dataService = new SectionReportService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetLIDSectionReport(input);

            return ProcessResultStatus(result);
        }
    }
}
