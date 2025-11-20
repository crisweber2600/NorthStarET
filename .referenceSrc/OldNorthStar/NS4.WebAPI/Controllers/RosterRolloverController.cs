using EntityDto.DTO.Admin.Simple;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Northstar.Core;
using NorthStar.Core;
using NorthStar.Core.FileUpload;
using NorthStar.EF6;
using NorthStar4.API.Infrastructure;
using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
namespace NS4.WebAPI.Controllers
{
    [RoutePrefix("api/RosterRollover")]
    [Authorize]
    public class RosterRolloverController : NSBaseController
    {
        private IPhotoManager photoManager;

        public RosterRolloverController() 
        : this(new AzurePhotoManager(CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString), NSConstants.Azure.RolloverContainer)) 
    {
            HttpContext.Current.Server.ScriptTimeout = 30000;
        }

        public RosterRolloverController(IPhotoManager photoManager)
        {
            this.photoManager = photoManager;
        }

        [HttpPost]
        [Route("uploadfullrollovercsv")]//[FromBody] InputDto_ImportAssessmentData input
        public async Task<IHttpActionResult> UploadFullRolloverCsv()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent("form-data"))
            {
                return BadRequest("Unsupported media type");
            }

            // also check to make sure it is CSV, XLS or XLSX
            try
            {
                // upload the file to the container
                photoManager.ContainerName = NSConstants.Azure.RolloverContainer;
                var files = await photoManager.Add(Request);

                // process uploaded files
                var dataService = new RosterRolloverDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
                var result = await dataService.ProcessFullRolloverUpload(files, photoManager);
                return ProcessResultStatus(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.GetBaseException().Message);
            }
        }

        [HttpPost]
        [Route("uploadstudentrollovercsv")]//[FromBody] InputDto_ImportAssessmentData input
        public async Task<IHttpActionResult> UploadStudentRolloverCsv()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent("form-data"))
            {
                return BadRequest("Unsupported media type");
            }

            // also check to make sure it is CSV, XLS or XLSX
            try
            {
                // upload the file to the container
                photoManager.ContainerName = NSConstants.Azure.RolloverContainer;
                var files = await photoManager.Add(Request);

                // process uploaded files
                var dataService = new RosterRolloverDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
                var result = await dataService.ProcessStudentRolloverUpload(files, photoManager);
                return ProcessResultStatus(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.GetBaseException().Message);
            }
        }
        [HttpPost]
        [Route("UploadTeacherRolloverCsv")]//[FromBody] InputDto_ImportAssessmentData input
        public async Task<IHttpActionResult> UploadTeacherRolloverCsv()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent("form-data"))
            {
                return BadRequest("Unsupported media type");
            }

            // also check to make sure it is CSV, XLS or XLSX
            try
            {
                // upload the file to the container
                photoManager.ContainerName = NSConstants.Azure.RolloverContainer;
                var files = await photoManager.Add(Request);

                // process uploaded files
                var dataService = new RosterRolloverDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
                var result = await dataService.ProcessTeacherRolloverUpload(files, photoManager);
                return ProcessResultStatus(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.GetBaseException().Message);
            }
        }


        [HttpPost]
        [Route("validateStudentRollover")]//[FromBody] InputDto_ImportAssessmentData input
        public IHttpActionResult validateStudentRollover([FromBody]InputDto_SimpleId input)
        {
            var dataService = new RosterRolloverDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.ValidateStudentRollover(input.Id);
            return ProcessResultStatus(result);
        }
        [HttpPost]
        [Route("validateTeacherRollover")]//[FromBody] InputDto_ImportAssessmentData input
        public IHttpActionResult validateTeacherRollover([FromBody]InputDto_SimpleId input)
        {
            var dataService = new RosterRolloverDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.ValidateTeacherRollover(input.Id);
            return ProcessResultStatus(result);
        }
        [HttpPost]
        [Route("validaterollover")]//[FromBody] InputDto_ImportAssessmentData input
        public IHttpActionResult ValidateRollover([FromBody]InputDto_SimpleId input)
        {
            var dataService = new RosterRolloverDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.ValidateRollover(input.Id);
            return ProcessResultStatus(result);
        }

        [HttpPost]
        [Route("CancelRollover")]//[FromBody] InputDto_ImportAssessmentData input
        public IHttpActionResult CancelRollover([FromBody]InputDto_SimpleId input)
        {
            var dataService = new RosterRolloverDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.CancelRollover(input.Id);
            return ProcessResultStatus(result);
        }
        [HttpPost]
        [Route("CancelStudentRollover")]//[FromBody] InputDto_ImportAssessmentData input
        public IHttpActionResult CancelStudentRollover([FromBody]InputDto_SimpleId input)
        {
            var dataService = new RosterRolloverDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.CancelStudentRollover(input.Id);
            return ProcessResultStatus(result);
        }
        [HttpPost]
        [Route("CancelTeacherRollover")]//[FromBody] InputDto_ImportAssessmentData input
        public IHttpActionResult CancelTeacherRollover([FromBody]InputDto_SimpleId input)
        {
            var dataService = new RosterRolloverDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.CancelTeacherRollover(input.Id);
            return ProcessResultStatus(result);
        }



        [HttpPost]
        [Route("FullRolloverReset")]//[FromBody] InputDto_ImportAssessmentData input
        public async Task<IHttpActionResult> FullRolloverReset()
        {
            var dataService = new RosterRolloverDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.FullRolloverReset();
            return ProcessResultStatus(result);
        }
        [HttpPost]
        [Route("StudentRolloverReset")]//[FromBody] InputDto_ImportAssessmentData input
        public async Task<IHttpActionResult> StudentRolloverReset()
        {
            var dataService = new RosterRolloverDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.StudentRolloverReset();
            return ProcessResultStatus(result);
        }
        [HttpPost]
        [Route("TeacherRolloverReset")]//[FromBody] InputDto_ImportAssessmentData input
        public async Task<IHttpActionResult> TeacherRolloverReset()
        {
            var dataService = new RosterRolloverDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.TeacherRolloverReset();
            return ProcessResultStatus(result);
        }

        [HttpGet]
        [Route("GetFullRolloverImportTemplate")]
        public IHttpActionResult GetFullRolloverImportTemplate()
        {
            var dataService = new RosterRolloverDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetFullRolloverTemplateWithContext();
            return ProcessResultStatus(result);
        }
        [HttpGet]
        [Route("GetStudentRolloverImportTemplate")]
        public IHttpActionResult GetStudentRolloverImportTemplate()
        {
            var dataService = new RosterRolloverDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetStudentRolloverTemplateWithContext();
            return ProcessResultStatus(result);
        }
        [HttpGet]
        [Route("GetTeacherRolloverImportTemplate")]
        public IHttpActionResult GetTeacherRolloverImportTemplate()
        {
            var dataService = new RosterRolloverDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetTeacherRolloverTemplateWithContext();
            return ProcessResultStatus(result);
        }

        [HttpGet]
        [Route("GetFullRolloverTemplateCSV")]
        public IHttpActionResult GetFullRolloverTemplateCSV()
        {
            var dataService = new RosterRolloverDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetFullRolloverTemplateCSV();
            return ProcessResultStatus(result);
        }
        [HttpGet]
        [Route("GetStudentRolloverTemplateCSV")]
        public IHttpActionResult GetStudentRolloverTemplateCSV()
        {
            var dataService = new RosterRolloverDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetStudentRolloverTemplateCSV();
            return ProcessResultStatus(result);
        }
        [HttpGet]
        [Route("GetTeacherRolloverTemplateCSV")]
        public IHttpActionResult GetTeacherRolloverTemplateCSV()
        {
            var dataService = new RosterRolloverDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetTeacherRolloverTemplateCSV();
            return ProcessResultStatus(result);
        }


        [HttpGet]
        [Route("LoadFullRolloverImportHistory")]
        public IHttpActionResult LoadFullRolloverImportHistory()
        {
            var dataService = new RosterRolloverDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.LoadFullRolloverImportHistory();
            return ProcessResultStatus(result);
        }
        [HttpGet]
        [Route("LoadStudentRolloverImportHistory")]
        public IHttpActionResult LoadStudentRolloverImportHistory()
        {
            var dataService = new RosterRolloverDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.LoadStudentRolloverImportHistory();
            return ProcessResultStatus(result);
        }
        [HttpGet]
        [Route("LoadTeacherRolloverImportHistory")]
        public IHttpActionResult LoadTeacherRolloverImportHistory()
        {
            var dataService = new RosterRolloverDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.LoadTeacherRolloverImportHistory();
            return ProcessResultStatus(result);
        }

        [HttpPost]
        [Route("DeleteFullRolloverHistoryItem")]
        public IHttpActionResult DeleteFullRolloverHistoryItem([FromBody]InputDto_SimpleId input)
        {
            var dataService = new RosterRolloverDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.DeleteFullRolloverHistoryItem(input);
            return ProcessResultStatus(result);
        }
        [HttpPost]
        [Route("DeleteStudentRolloverHistoryItem")]
        public IHttpActionResult DeleteStudentRolloverHistoryItem([FromBody]InputDto_SimpleId input)
        {
            var dataService = new RosterRolloverDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.DeleteStudentRolloverHistoryItem(input);
            return ProcessResultStatus(result);
        }
        [HttpPost]
        [Route("DeleteTeacherRolloverHistoryItem")]
        public IHttpActionResult DeleteTeacherRolloverHistoryItem([FromBody]InputDto_SimpleId input)
        {
            var dataService = new RosterRolloverDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.DeleteTeacherRolloverHistoryItem(input);
            return ProcessResultStatus(result);
        }

        [HttpPost]
        [Route("GetFullRolloverHistoryLog")]
        public IHttpActionResult GetFullRolloverHistoryLog([FromBody]InputDto_SimpleId input)
        {
            var dataService = new RosterRolloverDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetFullRolloverHistoryLog(input);
            return ProcessResultStatus(result);
        }
        [HttpPost]
        [Route("GetStudentRolloverHistoryLog")]
        public IHttpActionResult GetStudentRolloverHistoryLog([FromBody]InputDto_SimpleId input)
        {
            var dataService = new RosterRolloverDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetStudentRolloverHistoryLog(input);
            return ProcessResultStatus(result);
        }
        [HttpPost]
        [Route("GetTeacherRolloverHistoryLog")]
        public IHttpActionResult GetTeacherRolloverHistoryLog([FromBody]InputDto_SimpleId input)
        {
            var dataService = new RosterRolloverDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetTeacherRolloverHistoryLog(input);
            return ProcessResultStatus(result);
        }

        [Route("GetImportFile")]
        [HttpPost]
        public HttpResponseMessage GetImportFile([FromBody]InputDto_SimpleString input)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);

            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(NSConstants.Azure.RolloverContainer);
            var blob = container.GetBlockBlobReference(input.value);

            var builder = new UriBuilder(blob.Uri);
            builder.Query = blob.GetSharedAccessSignature(
                new SharedAccessBlobPolicy
                {
                    Permissions = SharedAccessBlobPermissions.Read,
                    //SharedAccessStartTime = new DateTimeOffset(DateTime.UtcNow.AddMinutes(-5)),
                    SharedAccessExpiryTime = new DateTimeOffset(DateTime.UtcNow.AddMinutes(5))
                }
                ,
                new SharedAccessBlobHeaders
                {
                    ContentDisposition = "attachment; filename=orginalimportfile.csv",
                    ContentType = MimeTypeMap.List.MimeTypeMap.GetMimeType(Path.GetExtension(input.value)).FirstOrDefault()
                }
                ).TrimStart('?');

            CloudBlockBlob downloadBlobReference = new CloudBlockBlob(builder.Uri);
            var stream = new MemoryStream();
            downloadBlobReference.DownloadToStream(stream);
            // processing the stream.

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(Utility.ReadFully(stream))
            };
            result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
            {
                FileName = "orginalimportfile.csv"
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return result;
        }
    }
}
