
using EntityDto.DTO.Admin.Simple;
using EntityDto.DTO.ImportExport;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Northstar.Core;
using NorthStar.Core;
using NorthStar.Core.FileUpload;
using NorthStar.EF6;
using NorthStar4.API.Infrastructure;
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
    [RoutePrefix("api/ImportStateTestData")]
    [Authorize]
    public class ImportStateTestDataController : NSBaseController
    {

        private IPhotoManager photoManager;

        public ImportStateTestDataController() 
        : this(new AzurePhotoManager(CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString), "assessmentimports")) 
    {
            HttpContext.Current.Server.ScriptTimeout = 30000;
        }

        public ImportStateTestDataController(IPhotoManager photoManager)
        {
            this.photoManager = photoManager;
        }

        public class InputDto_ImportAssessmentData
        {
            public int AssessmentId { get; set; }
            public int? BenchmarkDateId { get; set; }
        }

        [HttpPost]
        [Route("UploadBenchmarkDataCSV")]//[FromBody] InputDto_ImportAssessmentData input
        public async Task<IHttpActionResult> UploadBenchmarkDataCSV()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent("form-data"))
            {
                return BadRequest("Unsupported media type");
            }

            try
            {
                var files = await photoManager.Add(Request);

                // process uploaded files
                var dataService = new ImportTestDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
                var result = await dataService.ProcessClassBenchmarkFile(files, photoManager);
                return ProcessResultStatus(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.GetBaseException().Message);
            }
        }

        [Route("QuickSearchPageTypesToPrint")]
        [HttpGet]
        public List<OutputDto_DropdownData> QuickSearchPageTypesToPrint(string searchString)
        {
            var dataService = new ImportTestDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.QuickSearchPageTypesToPrint(searchString);
            return result;
        }

        [Route("QuickSearchTextLevelZones")]
        [HttpGet]
        public List<OutputDto_DropdownData> QuickSearchTextLevelZones(string searchString)
        {
            var dataService = new ImportTestDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.QuickSearchTextLevelZones(searchString);
            return result;
        }

        //[Route("QuickSearchHfwStudentReports")]
        //[HttpGet]
        //public List<OutputDto_DropdownData> QuickSearchHfwStudentReports(string searchString)
        //{
        //    var dataService = new ImportTestDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
        //    var result = dataService.QuickSearchHfwStudentReports(searchString);
        //    return result;
        //}

        [HttpPost]
        [Route("UploadInterventionDataCSV")]//[FromBody] InputDto_ImportAssessmentData input
        public async Task<IHttpActionResult> UploadInterventionDataCSV()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent("form-data"))
            {
                return BadRequest("Unsupported media type");
            }

            try
            {
                var files = await photoManager.Add(Request);

                // process uploaded files
                var dataService = new ImportTestDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
                var result = await dataService.ProcessInterventionGroupFile(files, photoManager);
                return ProcessResultStatus(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.GetBaseException().Message);
            }
        }


        #region Section And Intervention Group Export
        [HttpPost]
        [Route("GetBenchmarkExporTemplateWithData")]
        public IHttpActionResult GetBenchmarkExporTemplateWithData([FromBody]InputDto_ExportRequest input)
        {
            var dataService = new ImportTestDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetBenchmarkExporTemplateWithData(input);
            return ProcessResultStatus(result);
        }

        [HttpPost]
        [Route("GetInterventionExporTemplateWithData")]
        public IHttpActionResult GetInterventionExporTemplateWithData([FromBody]InputDto_InterventionExportRequest input)
        {
            var dataService = new ImportTestDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetInterventionExporTemplateWithData(input);
            return ProcessResultStatus(result);
        }
        #endregion

        #region Export Templates
        [HttpPost]
        [Route("getstatetestexporttemplate")]
        public IHttpActionResult GetStateTestExportTemplate([FromBody]InputDto_SimpleId input)
        {
            var dataService = new ImportTestDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetStateTestExportTemplate(input);
            return ProcessResultStatus(result);
        }

        [HttpPost]
        [Route("GetBenchmarkTestExportTemplate")]
        public IHttpActionResult GetBenchmarkTestExportTemplate([FromBody]InputDto_SimpleId input)
        {
            var dataService = new ImportTestDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetBenchmarkTestExportTemplate(input);
            return ProcessResultStatus(result);
        }

        [HttpPost]
        [Route("GetInterventionTestExportTemplate")]
        public IHttpActionResult GetInterventionTestExportTemplate([FromBody]InputDto_SimpleId input)
        {
            var dataService = new ImportTestDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetInterventionTestExportTemplate(input);
            return ProcessResultStatus(result);
        }
        #endregion

        #region Load User's Import History
        [HttpGet]
        [Route("LoadStateTestImportHistory")]
        public IHttpActionResult LoadStateTestImportHistory()
        {
            var dataService = new ImportTestDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.LoadStateTestImportHistory();
            return ProcessResultStatus(result);
        }
        [HttpGet]
        [Route("LoadBenchmarkTestImportHistory")]
        public IHttpActionResult LoadBenchmarkTestImportHistory()
        {
            var dataService = new ImportTestDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.LoadBenchmarkTestImportHistory();
            return ProcessResultStatus(result);
        }
        [HttpGet]
        [Route("LoadInterventionTestImportHistory")]
        public IHttpActionResult LoadInterventionTestImportHistory()
        {
            var dataService = new ImportTestDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.LoadInterventionTestImportHistory();
            return ProcessResultStatus(result);
        }
        #endregion

        #region Import Template Fields
        [HttpPost]
        [Route("GetStateTestDataImportTemplate")]
        public IHttpActionResult GetStateTestDataImportTemplate([FromBody]InputDto_SimpleId input)
        {
            var dataService = new ImportTestDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetStateTestDataImportTemplate(input);
            return ProcessResultStatus(result);
        }
        [HttpPost]
        [Route("GetBenchmarkTestDataImportTemplate")]
        public IHttpActionResult GetBenchmarkTestDataImportTemplate([FromBody]InputDto_SimpleId input)
        {
            var dataService = new ImportTestDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetBenchmarkTestDataImportTemplate(input);
            return ProcessResultStatus(result);
        }
        [HttpPost]
        [Route("GetInterventionTestDataImportTemplate")]
        public IHttpActionResult GetInterventionTestDataImportTemplate([FromBody]InputDto_SimpleId input)
        {
            var dataService = new ImportTestDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetInterventionTestDataImportTemplate(input);
            return ProcessResultStatus(result);
        }
        #endregion

        [HttpPost]
        [Route("updatecalculatedfields")]
        public IHttpActionResult UpdateCalculatedFields()
        {
                var dataService = new SectionDataEntryService(((ClaimsIdentity)User.Identity), LoginConnectionString);
                var result = dataService.UpdateCalculatedFields();
                return ProcessResultStatus(result);
        }

        #region History Logs
        [HttpPost]
        [Route("GetHistoryLog")]
        public IHttpActionResult GetHistoryLog([FromBody]InputDto_SimpleId input)
        {
            var dataService = new ImportTestDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetHistoryLog(input);
            return ProcessResultStatus(result);
        }
        [HttpPost]
        [Route("GetBMHistoryLog")]
        public IHttpActionResult GetBMHistoryLog([FromBody]InputDto_SimpleId input)
        {
            var dataService = new ImportTestDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetBMHistoryLog(input);
            return ProcessResultStatus(result);
        }
        [HttpPost]
        [Route("GetIntvHistoryLog")]
        public IHttpActionResult GetIntvHistoryLog([FromBody]InputDto_SimpleId input)
        {
            var dataService = new ImportTestDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetIntvHistoryLog(input);
            return ProcessResultStatus(result);
        }
        #endregion

        [Route("GetImportFile")]
        [HttpPost]
        public HttpResponseMessage GetImportFile([FromBody]InputDto_SimpleString input)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);

            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(NSConstants.Azure.AssessmentImportContainer);
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

        #region Delete History
        [HttpPost]
        [Route("DeleteHistoryItem")]
        public IHttpActionResult DeleteHistoryItem([FromBody]InputDto_SimpleId input)
        {
            var dataService = new ImportTestDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.DeleteHistoryItem(input);
            return ProcessResultStatus(result);
        }
        [HttpPost]
        [Route("DeleteBMHistoryItem")]
        public IHttpActionResult DeleteBMHistoryItem([FromBody]InputDto_SimpleId input)
        {
            var dataService = new ImportTestDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.DeleteBMHistoryItem(input);
            return ProcessResultStatus(result);
        }
        [HttpPost]
        [Route("DeleteIntvHistoryItem")]
        public IHttpActionResult DeleteIntvHistoryItem([FromBody]InputDto_SimpleId input)
        {
            var dataService = new ImportTestDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.DeleteIntvHistoryItem(input);
            return ProcessResultStatus(result);
        }
        #endregion

        //[HttpPost]
        //[Route("uploadmncsv")]//[FromBody] InputDto_ImportAssessmentData input
        //public async Task<IHttpActionResult> UploadMNCSV()
        //{
        //    // Check if the request contains multipart/form-data.
        //    if (!Request.Content.IsMimeMultipartContent("form-data"))
        //    {
        //        return BadRequest("Unsupported media type");
        //    }

        //    try
        //    {
        //        var files = await photoManager.Add(Request);

        //        // process uploaded files
        //        var dataService = new ImportTestDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
        //        var result = await dataService.ProcessMNImportedFiles(files, photoManager);
        //        return ProcessResultStatus(result);
        //        //return Ok(new { Successful = true, Message = "Files uploaded ok", Photos = files });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.GetBaseException().Message);
        //    }
        //}

        //[HttpPost]
        //[Route("uploadmnfinalcsv")]//[FromBody] InputDto_ImportAssessmentData input
        //public async Task<IHttpActionResult> UploadMNFinalCSV()
        //{
        //    // Check if the request contains multipart/form-data.
        //    if (!Request.Content.IsMimeMultipartContent("form-data"))
        //    {
        //        return BadRequest("Unsupported media type");
        //    }

        //    try
        //    {
        //        var files = await photoManager.Add(Request);

        //        // process uploaded files
        //        var dataService = new ImportTestDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
        //        var result = await dataService.ProcessMNFinalFiles(files, photoManager);
        //        return ProcessResultStatus(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.GetBaseException().Message);
        //    }
        //}

        #region New Queue Based Upload Routines
        [HttpPost]
        [Route("uploadstatetestcsv")]//[FromBody] InputDto_ImportAssessmentData input
        public async Task<IHttpActionResult> UploadStateTestCsv()
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
                photoManager.ContainerName = NSConstants.Azure.AssessmentImportContainer;
                var files = await photoManager.Add(Request);

                // process uploaded files
                var dataService = new ImportTestDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
                var result = await dataService.ProcessStateTestUpload(files, photoManager);
                return ProcessResultStatus(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.GetBaseException().Message);
            }
        }
        [HttpPost]
        [Route("UploadBenchmarkTestCsv")]//[FromBody] InputDto_ImportAssessmentData input
        public async Task<IHttpActionResult> UploadBenchmarkTestCsv()
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
                photoManager.ContainerName = NSConstants.Azure.AssessmentImportContainer;
                var files = await photoManager.Add(Request);

                // process uploaded files
                var dataService = new ImportTestDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
                var result = await dataService.ProcessBenchmarkTestUpload(files, photoManager);
                return ProcessResultStatus(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.GetBaseException().Message);
            }
        }
        [HttpPost]
        [Route("UploadInterventionTestCsv")]//[FromBody] InputDto_ImportAssessmentData input
        public async Task<IHttpActionResult> UploadInterventionTestCsv()
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
                photoManager.ContainerName = NSConstants.Azure.AssessmentImportContainer;
                var files = await photoManager.Add(Request);

                // process uploaded files
                var dataService = new ImportTestDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
                var result = await dataService.ProcessInterventionTestUpload(files, photoManager);
                return ProcessResultStatus(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.GetBaseException().Message);
            }
        }
        #endregion


        //public async Task<IEnumerable<int>> Add(HttpRequestMessage request)
        //{
        //    var stuff = new List<int>();
        //    string root = HttpContext.Current.Server.MapPath("~/App_Data");
        //    var provider = new MultipartFormDataStreamProvider(root);
        //    //var stream = await request.Content.ReadAsMultipartAsync();



        //    try {

        //        await Request.Content.ReadAsMultipartAsync(provider);
        //        // This illustrates how to get the file names.
        //        foreach (MultipartFileData file in provider.FileData)
        //        {
        //            var data = DataTable.New.ReadCsv(file.LocalFileName);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error("Error Saving file {message}", ex.Message);
        //    }

        //    //foreach (var fileContent in stream.Contents)
        //    //{

        //    //    var stream2 = await request.Content.ReadAsStreamAsync();
        //    //    TextReader tr = new StreamReader(stream2);
        //    //    var text = tr.ReadToEnd();
        //    //    try
        //    //    {
        //    //        var data = DataTable.New.ReadFromString(text);

        //    //        //CloudBlobClient blobClient = this.StorageAccount.CreateCloudBlobClient();
        //    //        //CloudBlobContainer photoContainer = blobClient.GetContainerReference(this.ContainerName);
        //    //        //var provider = new AzureBlobMultipartFormDataStreamProvider(photoContainer);

        //    //        //await request.Content.ReadAsMultipartAsync(provider);

        //    //        //var photos = new List<PhotoViewModel>();

        //    //        //foreach (var file in provider.FileData)
        //    //        //{
        //    //        //    //the LocalFileName is going to be the absolute Uri of the blob (see GetStream)
        //    //        //    //use it to get the blob info to return to the client
        //    //        //    var blob = await photoContainer.GetBlobReferenceFromServerAsync(file.LocalFileName);
        //    //        //    await blob.FetchAttributesAsync();
        //    //        //    blob.

        //    //        //    //photos.Add(new PhotoViewModel
        //    //        //    //{
        //    //        //    //    Name = blob.Name,
        //    //        //    //    Size = blob.Properties.Length / 1024,
        //    //        //    //    Created = blob.Metadata["Created"] == null ? DateTime.Now : DateTime.Parse(blob.Metadata["Created"]),
        //    //        //    //    Modified = ((DateTimeOffset)blob.Properties.LastModified).DateTime,
        //    //        //    //    Url = blob.Uri.AbsoluteUri
        //    //        //    //});
        //    //        // }
        //    //    }
        //    //    catch (Exception ex)
        //    //    {
        //    //        Log.Error("Error Saving file {message}", ex.Message);
        //    //    }
        //    //}
        //    return stuff;
        //}

        //private readonly DistrictContext _dbContext;
        //public IHostingEnvironment HostingEnvironment { get; set; }

        //public ImportStateTestDataController(DistrictContext dbContext, IHostingEnvironment hosting)
        //{
        //    _dbContext = dbContext;
        //    HostingEnvironment = hosting;
        //}

        //[HttpPost("ImportMNData")]
        //public IHttpActionResult ImportMNData(IList<IFormFile> files)
        //{

        //    var theFile = Request.Form.Files[0];
        //    var memStream = new MemoryStream();


        //    //foreach (var f in Request.Form.Files)
        //    //{
        //    //    await f.SaveAsAsync(Path.Combine(HostingEnvironment.WebRootPath, "test-file"));
        //    //}
        //    using (var stream = theFile.OpenReadStream())
        //    {
        //        //foreach (var file in files)
        //        //{
        //        //    var fileName = ContentDispositionHeaderValue
        //        //        .Parse(file.ContentDisposition)
        //        //        .FileName
        //        //        .Trim('"');// FileName returns "fileName.ext"(with double quotes) in beta 3

        //        //    if (fileName.EndsWith(".txt"))// Important for security if saving in webroot
        //        //    {
        //        //        var filePath = _hostingEnvironment.ApplicationBasePath + "\\wwwroot\\" + fileName;
        //        //        await file..SaveAsAsync(filePath);
        //        //    }
        //        //}



        //        SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");

        //        ExcelFile ef = ExcelFile.Load(stream, LoadOptions.CsvDefault);

        //        DataTable dataTable = new DataTable();

        //        // Depending on the format of the input file, you need to change this:
        //        dataTable.Columns.Add("Admin", typeof(string));
        //        dataTable.Columns.Add("TEST_NAME", typeof(string));
        //        dataTable.Columns.Add("SUBJECT", typeof(string));
        //        dataTable.Columns.Add("LATEST_TEST_START_DATE", typeof(string));
        //        dataTable.Columns.Add("FIRST_NAME", typeof(string));
        //        dataTable.Columns.Add("LAST_NAME", typeof(string));
        //        dataTable.Columns.Add("MARSS_NUMBER", typeof(string));
        //        dataTable.Columns.Add("STUDENT_ID", typeof(int));
        //        dataTable.Columns.Add("DATE_OF_BIRTH", typeof(string));
        //        dataTable.Columns.Add("GENDER", typeof(string));
        //        dataTable.Columns.Add("ETHNICITY_CODE", typeof(int));
        //        dataTable.Columns.Add("GRADE_CODE", typeof(int));
        //        dataTable.Columns.Add("DISTRICT_CODE", typeof(string));
        //        dataTable.Columns.Add("DISTRICT_ORG_TYPE", typeof(string));
        //        dataTable.Columns.Add("SCHOOL_CODE", typeof(string));
        //        dataTable.Columns.Add("ENROLLMENT_CODE", typeof(string));
        //        dataTable.Columns.Add("OPPORTUNITY", typeof(string));
        //        dataTable.Columns.Add("TEST_CODE", typeof(string));
        //        dataTable.Columns.Add("SCALE_SCORE", typeof(int));
        //        dataTable.Columns.Add("SEM", typeof(string));
        //        dataTable.Columns.Add("ACHIEVEMENT_LEVEL", typeof(string));
        //        dataTable.Columns.Add("STRAND_NAME_1", typeof(string));
        //        dataTable.Columns.Add("STRAND_SCORE_1", typeof(int));
        //        dataTable.Columns.Add("STRAND_NAME_2", typeof(string));
        //        dataTable.Columns.Add("STRAND_SCORE_2", typeof(int));
        //        dataTable.Columns.Add("STRAND_NAME_3", typeof(string));
        //        dataTable.Columns.Add("STRAND_SCORE_3", typeof(int));
        //        dataTable.Columns.Add("STRAND_NAME_4", typeof(string));
        //        dataTable.Columns.Add("STRAND_SCORE_4", typeof(int));

        //        // Select the first worksheet from the file.
        //        ExcelWorksheet ws = ef.Worksheets[0];

        //        ExtractToDataTableOptions options = new ExtractToDataTableOptions(1, 0, 150);
        //        options.ExtractDataOptions = ExtractDataOptions.StopAtFirstEmptyRow;

        //        options.ExcelCellToDataTableCellConverting += (sender, e) =>
        //        {
        //            if (!e.IsDataTableValueValid)
        //            {
        //            // GemBox.Spreadsheet doesn't automatically convert numbers to strings in ExtractToDataTable() method because of culture issues; 
        //            // someone would expect the number 12.4 as "12.4" and someone else as "12,4".
        //            e.DataTableValue = e.ExcelCell.Value == null ? null : e.ExcelCell.Value.ToString();
        //                e.Action = ExtractDataEventAction.Continue;
        //            }
        //        };

        //        // Extract the data from the worksheet to the DataTable.
        //        // Data is extracted starting at first row and first column for 10 rows or until the first empty row appears.
        //        ws.ExtractToDataTable(dataTable, options);

        //        // Write DataTable content
        //        StringBuilder sb = new StringBuilder();
        //        sb.AppendLine("DataTable content:");
        //        foreach (DataRow row in dataTable.Rows)
        //        {
        //            sb.AppendFormat("{0}    {1}", row[0], row[1]);
        //            sb.AppendLine();
        //        }

        //        Console.WriteLine(sb.ToString());

        //    }


        //    return new HttpStatusCodeResult(200);
        //}
    }
}
