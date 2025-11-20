using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web.Http;
using NorthStar.EF6;
using NorthStar4.PCL.Entity;
using NorthStar4.API.Infrastructure;
using EntityDto.DTO.Admin.InterventionToolkit;
using NorthStar4.PCL.DTO;
using Serilog;
using System.Threading.Tasks;
using System.Net.Http;
using NorthStar.Core.FileUpload;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using EntityDto.LoginDB.DTO;

namespace NorthStar4.API.api
{
    [RoutePrefix("api/InterventionToolkit")]
    [Authorize]
    public class InterventionToolkitController : NSBaseController
    {
        [HttpPost]
        [Route("uploadinterventiontool")]//[FromBody] InputDto_ImportAssessmentData input
        public async Task<IHttpActionResult> UploadInterventionTool()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent("form-data"))
            {
                return BadRequest("Unsupported media type");
            }

            try
            {
                var photoManager = new AzurePhotoManager(CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString), "interventiontools");

                // upload File
                var files = await photoManager.Add(Request);

                // get intervention Id
                var interventionId = Int32.Parse(files.FormData["InterventionId"]);

                // process uploaded files
                var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
                var result = dataService.AssociateUploadedInterventionTool(files, interventionId, 2);
                return ProcessResultStatus(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.GetBaseException().Message);
            }
        }

        [HttpPost]
        [Route("uploadassessmenttool")]//[FromBody] InputDto_ImportAssessmentData input
        public async Task<IHttpActionResult> UploadAssessmentTool()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent("form-data"))
            {
                return BadRequest("Unsupported media type");
            }

            try
            {
                var photoManager = new AzurePhotoManager(CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString), "interventiontools");

                // upload File
                var files = await photoManager.Add(Request);

                // get intervention Id
                var interventionId = Int32.Parse(files.FormData["InterventionId"]);

                // process uploaded files
                var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
                var result = dataService.AssociateUploadedInterventionTool(files, interventionId, 1);
                return ProcessResultStatus(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.GetBaseException().Message);
            }
        }

        [HttpPost]
        [Route("uploadpresentation")]//[FromBody] InputDto_ImportAssessmentData input
        public async Task<IHttpActionResult> UploadPresentation()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent("form-data"))
            {
                return BadRequest("Unsupported media type");
            }

            try
            {
                var photoManager = new AzurePhotoManager(CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString), "interventiontools");

                // upload File
                var files = await photoManager.Add(Request);

                // get intervention Id
                var pageId = Int32.Parse(files.FormData["PageId"]);

                // process uploaded files
                var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
                var result = dataService.AssociateUploadedPresention(files, pageId);
                return ProcessResultStatus(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.GetBaseException().Message);
            }
        }

        [HttpPost]
        [Route("uploadpagetool")]//[FromBody] InputDto_ImportAssessmentData input
        public async Task<IHttpActionResult> UploadPageTool()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent("form-data"))
            {
                return BadRequest("Unsupported media type");
            }

            try
            {
                var photoManager = new AzurePhotoManager(CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString), "interventiontools");

                // upload File
                var files = await photoManager.Add(Request);

                // get intervention Id
                var pageId = Int32.Parse(files.FormData["PageId"]);

                // process uploaded files
                var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
                var result = dataService.AssociateUploadedPageTool(files, pageId);
                return ProcessResultStatus(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.GetBaseException().Message);
            }
        }

        [Route("GetInterventionTiers")]
        [HttpGet]
        public IHttpActionResult GetInterventionTiers()
        {
            
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var results = dataService.GetInterventionTiers();

            return ProcessResultStatus(results);
        }

        [Route("GetPages")]
        [HttpGet]
        public IHttpActionResult GetPages()
        {

            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var results = dataService.GetPages();

            return ProcessResultStatus(results);
        }

        [Route("GetInterventionsByTier")]
        [HttpPost]
        public IHttpActionResult GetInterventionsByTier([FromBody]InputDto_InterventionSearch input)
        {
 
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var results = dataService.GetInterventionsByTier(input);

            return ProcessResultStatus(results);
        }
        [Route("GetInterventionById")]
        [HttpPost]
        public IHttpActionResult GetInterventionById([FromBody]InputDto_SimpleId input)
        {
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var results = dataService.GetInterventionById(input);

            return ProcessResultStatus(results);
        }
        [Route("GetPageById")]
        [HttpPost]
        public IHttpActionResult GetPageById([FromBody]InputDto_SimpleId input)
        {
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var results = dataService.GetPageById(input);

            return ProcessResultStatus(results);
        }
        [Route("SaveIntervention")]
        [HttpPost]
        public IHttpActionResult SaveIntervention([FromBody]OutputDto_Intervention input)
        {
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var results = dataService.SaveIntervention(input.Intervention);

            return ProcessResultStatus(results);
        }
        [Route("SavePage")]
        [HttpPost]
        public IHttpActionResult SavePage([FromBody]OutputDto_Page input)
        {
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var results = dataService.SavePage(input.NSPage);

            return ProcessResultStatus(results);
        }
        [Route("AssociateTool")]
        [HttpPost]
        public IHttpActionResult AssociateTool([FromBody]InputDto_AssociateToolToIntervention input)
        {
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var results = dataService.AssociateTool(input);

            return ProcessResultStatus(results);
        }
        [Route("AssociatePageTool")]
        [HttpPost]
        public IHttpActionResult AssociatePageTool([FromBody]InputDto_AssociateToolToPage input)
        {
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var results = dataService.AssociatePageTool(input);

            return ProcessResultStatus(results);
        }
        [Route("AssociatePresentation")]
        [HttpPost]
        public IHttpActionResult AssociatePresentation([FromBody]InputDto_AssociatePresentationToPage input)
        {
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var results = dataService.AssociatePresentation(input);

            return ProcessResultStatus(results);
        }
        [Route("AssociateVideo")]
        [HttpPost]
        public IHttpActionResult AssociateVideo([FromBody]InputDto_AssociateVideoToIntervention input)
        {
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var results = dataService.AssociateVideo(input);

            return ProcessResultStatus(results);
        }
        [Route("AssociatePageVideo")]
        [HttpPost]
        public IHttpActionResult AssociatePageVideo([FromBody]InputDto_AssociateVideoToPage input)
        {
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var results = dataService.AssociatePageVideo(input);

            return ProcessResultStatus(results);
        }

        [Route("RemoveTool")]
        [HttpPost]
        public IHttpActionResult RemoveTool([FromBody]InputDto_AssociateToolToIntervention input)
        {
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var results = dataService.RemoveTool(input);

            return ProcessResultStatus(results);
        }
        [Route("RemovePageTool")]
        [HttpPost]
        public IHttpActionResult RemovePageTool([FromBody]InputDto_AssociateToolToPage input)
        {
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var results = dataService.RemovePageTool(input);

            return ProcessResultStatus(results);
        }
        [Route("RemovePresentation")]
        [HttpPost]
        public IHttpActionResult RemovePresentation([FromBody]InputDto_AssociatePresentationToPage input)
        {
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var results = dataService.RemovePresentation(input);

            return ProcessResultStatus(results);
        }
        [Route("RemoveVideo")]
        [HttpPost]
        public IHttpActionResult RemoveVideo([FromBody]InputDto_RemoveVideoFromIntervention input)
        {
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var results = dataService.RemoveVideo(input);

            return ProcessResultStatus(results);
        }
        [Route("RemovePageVideo")]
        [HttpPost]
        public IHttpActionResult RemovePageVideo([FromBody]InputDto_RemoveVideoFromPage input)
        {
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var results = dataService.RemovePageVideo(input);

            return ProcessResultStatus(results);
        }
        [Route("quicksearchassessmenttools")]
        [HttpGet]
        public List<OutputDto_DropdownData> QuickSearchAssessmentTools(string searchString)
        {
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.QuickSearchAssessmentTools(searchString);

            return result;
        }
        [Route("getinterventions")]
        [HttpGet]
        public List<OutputDto_DropdownData> GetInterventions(string searchString)
        {
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetInterventions(searchString);

            return result;
        }
        [Route("QuickSearchVideos")]
        [HttpGet]
        public async Task<List<OutputDto_DropdownData_VzaarVideo>> QuickSearchVideos(string searchString)
        {
            var dataService = new VideoDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = await dataService.QuickSearchVideos(searchString, VzaarSecret, VzaarToken);

            return result;
        }

        [Route("quicksearchinterventiontools")]
        [HttpGet]
        public List<OutputDto_DropdownData> QuickSearchInterventionTools(string searchString)
        {
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.QuickSearchInterventionTools(searchString);

            return result;
        }

        [Route("quicksearchpagetools")]
        [HttpGet]
        public List<OutputDto_DropdownData> QuickSearchPageTools(string searchString)
        {
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.QuickSearchPageTools(searchString);

            return result;
        }

        [Route("quicksearchpresentations")]
        [HttpGet]
        public List<OutputDto_DropdownData> QuickSearchPresentations(string searchString)
        {
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.QuickSearchPresentations(searchString);

            return result;
        }

        [Route("SaveTool")]
        [HttpPost]
        public IHttpActionResult SaveTool([FromBody]InputDto_InterventionTool input)
        {
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var results = dataService.SaveTool(input.Tool);

            return ProcessResultStatus(results);
        }
        [Route("SavePageTool")]
        [HttpPost]
        public IHttpActionResult SavePageTool([FromBody]InputDto_PageTool input)
        {
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var results = dataService.SavePageTool(input.Tool);

            return ProcessResultStatus(results);
        }
        [Route("SavePagePresentation")]
        [HttpPost]
        public IHttpActionResult SavePagePresentation([FromBody]InputDto_PagePresentation input)
        {
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var results = dataService.SavePagePresentation(input.Presentation);

            return ProcessResultStatus(results);
        }
        [Route("SavePageVideo")]
        [HttpPost]
        public IHttpActionResult SavePageVideo([FromBody]InputDto_PageVideo input)
        {
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var results = dataService.SavePageVideo(input.Video);

            return ProcessResultStatus(results);
        }
        [Route("SaveVideo")]
        [HttpPost]
        public IHttpActionResult SaveVideo([FromBody]InputDto_InterventionVideo input)
        {
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var results = dataService.SaveVideo(input.Video);

            return ProcessResultStatus(results);
        }
        [Route("DeleteIntervention")]
        [HttpPost]
        public IHttpActionResult DeleteIntervention([FromBody]InputDto_SimpleId input)
        {
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var results = dataService.DeleteIntervention(input.Id);

            return ProcessResultStatus(results);
        }

        [Route("DeletePage")]
        [HttpPost]
        public IHttpActionResult DeletePage([FromBody]InputDto_SimpleId input)
        {
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var results = dataService.DeletePage(input.Id);

            return ProcessResultStatus(results);
        }
        [Route("QuickSearchGrades")]
        [HttpGet]
        public List<OutputDto_DropdownData> QuickSearchGrades(string searchString)
        {
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.QuickSearchInterventionGrades(searchString);
            return result;
        }
        [Route("QuickSearchDistricts")]
        [HttpGet]
        public List<OutputDto_DropdownData> QuickSearchDistricts(string searchString)
        {
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.QuickSearchDistricts(searchString);
            return result;
        }
        [Route("Workshops")]
        [HttpGet]
        public List<OutputDto_DropdownData> Workshops(string searchString)
        {
            var dataService = new InterventionToolkitDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.QuickSearchWorkshops(searchString);
            return result;
        }
    }
}
