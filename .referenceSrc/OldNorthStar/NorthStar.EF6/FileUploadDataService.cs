using AutoMapper;
using EntityDto.DTO.Admin.InterventionGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Net.Http;
using NorthStar.Core.FileUpload;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;

namespace NorthStar.EF6.DataService
{
    public class FileUploadDataService : NSBaseDataService
    {
        private string _imageContainer = "images";
        private CloudStorageAccount _storageAccount;
        public FileUploadDataService(ClaimsIdentity user, string loginConnectionString) : base(user, loginConnectionString)
        {
            _storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);
            var district = _loginContext.Districts.First(p => p.Id == _currentUser.DistrictId);

            if(!String.IsNullOrEmpty(district.AzureContainerName))
            {
                _imageContainer = district.AzureContainerName;
            }
        }

       public string GetDistrictImage(string fileName)
        {
            var district = _loginContext.Districts.First(p => p.Id == _currentUser.DistrictId);

            if (!String.IsNullOrEmpty(district.AzureContainerName))
            {
                _imageContainer = district.AzureContainerName;
            }

            var client = _storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(_imageContainer);
            var blob = container.GetBlockBlobReference(fileName);

            var builder = new UriBuilder(blob.Uri);
            builder.Query = blob.GetSharedAccessSignature(
                new SharedAccessBlobPolicy
                {
                    Permissions = SharedAccessBlobPermissions.Read,
                    //SharedAccessStartTime = new DateTimeOffset(DateTime.UtcNow.AddMinutes(-5)),
                    SharedAccessExpiryTime = new DateTimeOffset(DateTime.UtcNow.AddMinutes(5))
                }
                ).TrimStart('?');

            return builder.Uri.ToString();
        }

        public async Task<AssessmentImportViewModel> UploadImageDragDrop(HttpRequestMessage Request, IPhotoManager photoManager)
        {
            photoManager.ContainerName = _imageContainer;
            var uploadedImages = await photoManager.Add(Request);

            var firstImage = uploadedImages.Files.First();

            return firstImage;
        }

        public async Task<string> UploadImageFileBrowser(HttpRequestMessage Request, IPhotoManager photoManager, string token)
        {
            photoManager.ContainerName = _imageContainer;
            var uploadedImages = await photoManager.Add(Request);

            var firstImage = uploadedImages.Files.First();
            var result = @"<script type='text/javascript'>";//azurewebsites.net
            result += @"function getUrlParam( paramName ) { var reParam = new RegExp( '(?:[\?&]|&)' + paramName + '=([^&]+)', 'i' );  var match = window.location.search.match( reParam );  return ( match && match.length > 1 ) ? match[1] : null; }";
            result += String.Format(@"document.domain = '{1}';var funcNum = getUrlParam( 'CKEditorFuncNum' ); window.parent.CKEDITOR.tools.callFunction(funcNum, '{0}', '');", ConfigurationManager.AppSettings["WebApiUrlBase"] + "/api/fileuploader/getdistrictimage?filename=" + firstImage.Name + "&access_token=" + token, ConfigurationManager.AppSettings["DocDomain"]);
            result += "</script>";
            return result;
        }
    }
}
