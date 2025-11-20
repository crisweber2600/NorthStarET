
using EntityDto.DTO.Admin.Simple;
using EntityDto.DTO.ImportExport;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Northstar.Core;
using NorthStar.Core;
using NorthStar.Core.FileUpload;
using NorthStar.EF6;
using NorthStar4.API.Infrastructure;
using NorthStar4.CrossPlatform.DTO.Reports.ObservationSummary;
using NorthStar4.PCL.DTO;
using Serilog;
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

namespace NorthStar4.api
{
    [RoutePrefix("api/ExportData")]
    [Authorize]
    public class ExportDataController : NSBaseController
    {

        private IPhotoManager photoManager;

        public ExportDataController() 
        : this(new AzurePhotoManager(CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString), "assessmentimports")) 
    {
            HttpContext.Current.Server.ScriptTimeout = 30000;
        }

        public ExportDataController(IPhotoManager photoManager)
        {
            this.photoManager = photoManager;
        }

        #region Attendance and Assessment Data Export
        [HttpPost]
        [Route("CreateAssessmentDataExportJob")]
        public IHttpActionResult CreateAssessmentDataExportJob([FromBody]InputDto_GetFilteredObservationSummaryOptions input)
        {
            var dataService = new ExportDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.CreateAssessmentDataExportJob(input);
            return ProcessResultStatus(result);
        }

        [HttpPost]
        [Route("CreateAssessmentAllFieldsDataExportJob")]
        public IHttpActionResult CreateAssessmentAllFieldsDataExportJob([FromBody]InputDto_GetFilteredObservationSummaryOptions input)
        {
            var dataService = new ExportDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.CreateAssessmentAllFieldsDataExportJob(input);
            return ProcessResultStatus(result);
        }


        [HttpPost]
        [Route("CreateInterventionGroupAssessmentDataExportJob")]
        public IHttpActionResult CreateInterventionGroupAssessmentDataExportJob([FromBody]InputDto_GetFilteredObservationSummaryOptions input)
        {
            var dataService = new ExportDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.CreateInterventionGroupAssessmentDataExportJob(input);
            return ProcessResultStatus(result);
        }

        //[HttpPost]
        //[Route("CreateAttendanceDataExportJob")]
        //public IHttpActionResult CreateAttendanceDataExportJob([FromBody]InputDto_SimpleId input)
        //{
        //    var dataService = new ExportDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
        //    var result = dataService.CreateAttendanceDataExportJob(input);
        //    return ProcessResultStatus(result);
        //}

        [HttpPost]
        [Route("CreateAttendanceDataExportJob")]
        public IHttpActionResult CreateAttendanceDataExportJob([FromBody]InputDto_SimpleId input)
        {
            var dataService = new ExportDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.CreateAttendanceDataExportJob(input);
            return ProcessResultStatus(result);
        }
        [HttpPost]
        [Route("CreateStudentExportJob")]
        public IHttpActionResult CreateStudentExportJob([FromBody]InputDto_SimpleString input)
        {
            var dataService = new ExportDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.CreateStudentExportJob(input);
            return ProcessResultStatus(result);
        }
        [HttpPost]
        [Route("CreateStaffExportJob")]
        public IHttpActionResult CreateStaffExportJob([FromBody]InputDto_SimpleString input)
        {
            var dataService = new ExportDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.CreateStaffExportJob(input);
            return ProcessResultStatus(result);
        }

        [HttpPost]
        [Route("CreatePrintBatchJob")]
        public IHttpActionResult CreatePrintBatchJob([FromBody]InputDto_GetFilteredPrintBatchOptions input)
        {
            var dataService = new ExportDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.CreatePrintBatchJob(input);
            return ProcessResultStatus(result);
        }
        #endregion

        #region Load User's Import History
        [HttpGet]
        [Route("LoadAssessmentDataExportHistory")]
        public IHttpActionResult LoadAssessmentDataExportHistory()
        {
            var dataService = new ExportDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.LoadAssessmentDataExportHistory();
            return ProcessResultStatus(result);
        }

       
        [HttpGet]
        [Route("LoadAssessmentDataExportHistoryAllFields")]
        public IHttpActionResult LoadAssessmentDataExportHistoryAllFields()
        {
            var dataService = new ExportDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.LoadAssessmentDataExportHistoryAllFields();
            return ProcessResultStatus(result);
        }
        [HttpGet]
        [Route("LoadInterventionDataExportHistory")]
        public IHttpActionResult LoadInterventionDataExportHistory()
        {
            var dataService = new ExportDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.LoadInterventionDataExportHistory();
            return ProcessResultStatus(result);
        }

        [HttpGet]
        [Route("LoadBatchPrintHistory")]
        public IHttpActionResult LoadBatchPrintHistory()
        {
            var dataService = new ExportDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.LoadBatchPrintHistory();
            return ProcessResultStatus(result);
        }

        [HttpGet]
        [Route("LoadAttendanceDataExportHistory")]
        public IHttpActionResult LoadAttendanceDataExportHistory()
        {
            var dataService = new ExportDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.LoadAttendanceDataExportHistory();
            return ProcessResultStatus(result);
        }
        [HttpGet]
        [Route("LoadStaffExportHistory")]
        public IHttpActionResult LoadStaffExportHistory()
        {
            var dataService = new ExportDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.LoadStaffExportHistory();
            return ProcessResultStatus(result);
        }
        [HttpGet]
        [Route("LoadStudentExportHistory")]
        public IHttpActionResult LoadStudentExportHistory()
        {
            var dataService = new ExportDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.LoadStudentExportHistory();
            return ProcessResultStatus(result);
        }


        #endregion


        [Route("DownloadExportFile")]
        [HttpPost]
        public HttpResponseMessage DownloadExportFile([FromBody]InputDto_SimpleString input)
        {
            // TODO: Security... need to also pass in the Job ID and make sure the current user has access to it... they could make a fake request if they knew the file name
            // and could just request the file... need to make sure they created the job, it is unlikely... but need to be careful
            // maybe we just search the jobs table for a file with this name and make sure their name is on it
            var dataService = new ExportDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            if (!dataService.CanAccessExportFile(input.value))
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);

            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(NSConstants.Azure.AssessmentDataExportContainer);
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
                    ContentDisposition = "attachment; filename=assessmentdataexport.csv",
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
                FileName = "assessmentdataexport.csv"
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return result;
        }

        [Route("DownloadAllFieldsExportFile")]
        [HttpPost]
        public HttpResponseMessage DownloadAllFieldsExportFile([FromBody]InputDto_SimpleString input)
        {
            // TODO: Security... need to also pass in the Job ID and make sure the current user has access to it... they could make a fake request if they knew the file name
            // and could just request the file... need to make sure they created the job, it is unlikely... but need to be careful
            // maybe we just search the jobs table for a file with this name and make sure their name is on it
            var dataService = new ExportDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            if (!dataService.CanAccessAllFieldsExportFile(input.value))
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);

            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(NSConstants.Azure.AssessmentDataExportContainer);
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
                    ContentDisposition = "attachment; filename=assessmentdataexport.csv",
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
                FileName = "assessmentdataexport.csv"
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return result;
        }

        [Route("DownloadBatchPrintFile")]
        [HttpPost]
        public HttpResponseMessage DownloadBatchPrintFile([FromBody]InputDto_SimpleString input)
        {
            // TODO: Security... need to also pass in the Job ID and make sure the current user has access to it... they could make a fake request if they knew the file name
            // and could just request the file... need to make sure they created the job, it is unlikely... but need to be careful
            // maybe we just search the jobs table for a file with this name and make sure their name is on it
            var dataService = new ExportDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            if (!dataService.CanAccessPrintBatchFile(input.value))
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);

            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(NSConstants.Azure.AssessmentDataExportContainer);
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
                    ContentDisposition = "attachment; filename=printbatch.pdf",
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
                FileName = "printbatch.pdf"
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return result;
        }

        [Route("DownloadAttendanceExportFile")]
        [HttpPost]
        public HttpResponseMessage DownloadAttendanceExportFile([FromBody]InputDto_SimpleString input)
        {
            // TODO: Security... need to also pass in the Job ID and make sure the current user has access to it... they could make a fake request if they knew the file name
            // and could just request the file... need to make sure they created the job, it is unlikely... but need to be careful
            // maybe we just search the jobs table for a file with this name and make sure their name is on it
            var dataService = new ExportDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            if (!dataService.CanAccessAttendanceExportFile(input.value))
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);

            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(NSConstants.Azure.AssessmentDataExportContainer);
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
                    ContentDisposition = "attachment; filename=attendancedataexport.csv",
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
                FileName = "attendancedataexport.csv"
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return result;
        }
        [Route("DownloadStudentExportFile")]
        [HttpPost]
        public HttpResponseMessage DownloadStudentExportFile([FromBody]InputDto_SimpleString input)
        {
            // TODO: Security... need to also pass in the Job ID and make sure the current user has access to it... they could make a fake request if they knew the file name
            // and could just request the file... need to make sure they created the job, it is unlikely... but need to be careful
            // maybe we just search the jobs table for a file with this name and make sure their name is on it
            var dataService = new ExportDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            if (!dataService.CanAccessStudentExportFile(input.value))
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);

            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(NSConstants.Azure.AssessmentDataExportContainer);
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
                    ContentDisposition = "attachment; filename=studentexport.csv",
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
                FileName = "studentexport.csv"
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return result;
        }

        [Route("DownloadStaffExportFile")]
        [HttpPost]
        public HttpResponseMessage DownloadStaffExportFile([FromBody]InputDto_SimpleString input)
        {
            // TODO: Security... need to also pass in the Job ID and make sure the current user has access to it... they could make a fake request if they knew the file name
            // and could just request the file... need to make sure they created the job, it is unlikely... but need to be careful
            // maybe we just search the jobs table for a file with this name and make sure their name is on it
            var dataService = new ExportDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            if (!dataService.CanAccessStaffExportFile(input.value))
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);

            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(NSConstants.Azure.AssessmentDataExportContainer);
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
                    ContentDisposition = "attachment; filename=staffexport.csv",
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
                FileName = "staffexport.csv"
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return result;
        }

        [Route("DownloadInterventionDataExportFile")]
        [HttpPost]
        public HttpResponseMessage DownloadInterventionDataExportFile([FromBody]InputDto_SimpleString input)
        {
            // TODO: Security... need to also pass in the Job ID and make sure the current user has access to it... they could make a fake request if they knew the file name
            // and could just request the file... need to make sure they created the job, it is unlikely... but need to be careful
            // maybe we just search the jobs table for a file with this name and make sure their name is on it
            var dataService = new ExportDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            if (!dataService.CanAccessInterventionDataExportFile(input.value))
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);

            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(NSConstants.Azure.AssessmentDataExportContainer);
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
                    ContentDisposition = "attachment; filename=interventiondataexport.csv",
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
                FileName = "attendancedataexport.csv"
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return result;
        }

        #region Delete History
        [HttpPost]
        [Route("DeleteHistoryItem")]
        public IHttpActionResult DeleteHistoryItem([FromBody]InputDto_SimpleId input)
        {
            var dataService = new ExportDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.DeleteHistoryItem(input);
            return ProcessResultStatus(result);
        }

        [HttpPost]
        [Route("DeleteAllFieldsHistoryItem")]
        public IHttpActionResult DeleteAllFieldsHistoryItem([FromBody]InputDto_SimpleId input)
        {
            var dataService = new ExportDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.DeleteAllFieldsHistoryItem(input);
            return ProcessResultStatus(result);
        }

        [HttpPost]
        [Route("DeleteInterventionDataHistoryItem")]
        public IHttpActionResult DeleteInterventionDataHistoryItem([FromBody]InputDto_SimpleId input)
        {
            var dataService = new ExportDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.DeleteInterventionDataHistoryItem(input);
            return ProcessResultStatus(result);
        }


        [HttpPost]
        [Route("DeleteBatchPrintHistoryItem")]
        public IHttpActionResult DeleteBatchPrintHistoryItem([FromBody]InputDto_SimpleId input)
        {
            var dataService = new ExportDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.DeleteBatchPrintHistoryItem(input);
            return ProcessResultStatus(result);
        }

        [HttpPost]
        [Route("DeleteAttendanceHistoryItem")]
        public IHttpActionResult DeleteAttendanceHistoryItem([FromBody]InputDto_SimpleId input)
        {
            var dataService = new ExportDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.DeleteAttendanceHistoryItem(input);
            return ProcessResultStatus(result);
        }
        [HttpPost]
        [Route("DeleteStaffHistoryItem")]
        public IHttpActionResult DeleteStaffHistoryItem([FromBody]InputDto_SimpleId input)
        {
            var dataService = new ExportDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.DeleteStaffHistoryItem(input);
            return ProcessResultStatus(result);
        }
        [HttpPost]
        [Route("DeleteStudentHistoryItem")]
        public IHttpActionResult DeleteStudentHistoryItem([FromBody]InputDto_SimpleId input)
        {
            var dataService = new ExportDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.DeleteStudentHistoryItem(input);
            return ProcessResultStatus(result);
        }
        #endregion


    }
}
