using Microsoft.WindowsAzure.Storage;
using NorthStar.Core.FileUpload;
using NorthStar.EF6;
using NorthStar.EF6.DataService;
using NorthStar4.API.Infrastructure;
using NorthStar4.Infrastructure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace NS4.WebAPI.Controllers
{
    [RoutePrefix("api/FileUploader")]
    public class FileUploaderController : NSBaseController
    {
        private IPhotoManager photoManager;
        private string imageContainer = "images";

        public FileUploaderController() 
        : this(new AzurePhotoManager(CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString), "interventiontools")) 
    {
        }


        public FileUploaderController(IPhotoManager photoManager)
        {
            this.photoManager = photoManager;
        }

        // GET: api/Photo
        [Authorize]
        public async Task<IHttpActionResult> Get()
        {
            var results = await photoManager.Get();
            return Ok(new { photos = results });
        }

        public class CKEditorUploadResponse : EntityDto.DTO.Admin.Simple.OutputDto_Base
        {
            public int uploaded { get; set; }
            public string fileName { get; set; }
            public string url { get; set; }
            public CKEditorError error { get; set; }
        }

        public class CKEditorError
        {
            public string message { get; set; }
        }

        [Route("uploadimages")]
        [HttpPost]
        [Authorize]
        public async Task<IHttpActionResult> uploadimages()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent("form-data"))
            {
                return BadRequest("Unsupported media type");
            }

            try
            {
                var dataService = new FileUploadDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
                var result = await dataService.UploadImageDragDrop(Request, photoManager);

                var token = Request.GetQueryString("access_token");

                return Ok( new CKEditorUploadResponse { fileName = result.Name, url = ConfigurationManager.AppSettings["WebApiUrlBase"] + "/api/fileuploader/getdistrictimage?filename=" + result.Name + "&access_token=" + token, uploaded = 1 });
                //return Ok(new { Successful = true, Message = "Photos uploaded ok", Photos = photos });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.GetBaseException().Message);
            }
        }

        [Route("uploadimagesadmin")]
        [HttpPost]
        public async Task<IHttpActionResult> uploadimagesadmin()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent("form-data"))
            {
                return BadRequest("Unsupported media type");
            }

            try
            {
                photoManager.ContainerName = imageContainer;
                var uploadedImages = await photoManager.Add(Request);

                var firstImage = uploadedImages.Files.First();

                return Ok(new CKEditorUploadResponse { fileName = firstImage.Name, url = ConfigurationManager.AppSettings["WebApiUrlBase"] + "/api/fileuploader/getnorthstarimage?filename=" + firstImage.Name, uploaded = 1 });
                //return Ok(new { Successful = true, Message = "Photos uploaded ok", Photos = photos });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.GetBaseException().Message);
            }
        }

        [HttpGet]
        [Route("getdistrictimage")]
        [Authorize]
        public IHttpActionResult GetDistrictImage(string filename)
        {
            var dataService = new FileUploadDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetDistrictImage(filename);

            return Redirect(result);
        }

        [HttpGet]
        [Route("getnorthstarimage")]
        public IHttpActionResult GetNorthStarImage(string filename)
        {
            CloudStorageAccount _storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);
            var client = _storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(imageContainer);
            var blob = container.GetBlockBlobReference(filename);

            return Redirect(blob.Uri.ToString());
        }


        [Route("uploadimagesjsResponse")]
        [HttpPost]
        [Authorize]
        public async Task<HttpResponseMessage> uploadimagesjsResponse()
        {

            var dataService = new FileUploadDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var token = Request.GetQueryString("access_token");
            var result = await dataService.UploadImageFileBrowser(Request, photoManager, token);

       
            var response =  Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(result, Encoding.UTF8, "text/html");
            return response;
        }

        [Route("uploadimagesjsResponseAdmin")]
        [HttpPost]
        public async Task<HttpResponseMessage> uploadimagesjsResponseAdmin()
        {

            photoManager.ContainerName = imageContainer;
            var uploadedImages = await photoManager.Add(Request);

            var firstImage = uploadedImages.Files.First();
            var result = @"<script type='text/javascript'>";//azurewebsites.net
            result += @"function getUrlParam( paramName ) { var reParam = new RegExp( '(?:[\?&]|&)' + paramName + '=([^&]+)', 'i' );  var match = window.location.search.match( reParam );  return ( match && match.length > 1 ) ? match[1] : null; }";
            result += String.Format(@"document.domain = '{1}';var funcNum = getUrlParam( 'CKEditorFuncNum' ); window.parent.CKEDITOR.tools.callFunction(funcNum, '{0}', '');", ConfigurationManager.AppSettings["WebApiUrlBase"] +  "/api/fileuploader/getnorthstarimage?filename=" + firstImage.Name, ConfigurationManager.AppSettings["DocDomain"]);
            result += "</script>";

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(result, Encoding.UTF8, "text/html");
            return response;
        }



        [Authorize]
        public async Task<IHttpActionResult> Post()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent("form-data"))
            {
                return BadRequest("Unsupported media type");
            }

            try
            {
                var photos = await photoManager.Add(Request);
                return Ok(new { Successful = true, Message = "Photos uploaded ok", Photos = photos });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.GetBaseException().Message);
            }
        }

        [Authorize]
        [HttpDelete]
        public async Task<IHttpActionResult> Delete(string fileName)
        {
            if (!await this.photoManager.FileExists(fileName))
            {
                return NotFound();
            }

            var result = await this.photoManager.Delete(fileName);

            if (result.Successful)
            {
                return Ok(result.Message);
            }
            else
            {
                return BadRequest(result.Message);
            }
        }
    }
}
