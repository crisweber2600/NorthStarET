using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace NorthStar.Core.FileUpload
{
    public class AzurePhotoManager : IPhotoManager
    {
        public CloudStorageAccount StorageAccount {get; set;}
        public CloudBlobContainer BlobContainer { get; set; }
        public string ContainerName { get; set; }
        
        public AzurePhotoManager(CloudStorageAccount storageAccount, string containerName)
        {
            this.StorageAccount = storageAccount;
            this.ContainerName = containerName;
        }

        public Uri UploadTextToBlob(string fileName, string content)
        {
            CloudBlobClient blobClient = this.StorageAccount.CreateCloudBlobClient();
            CloudBlobContainer photoContainer = blobClient.GetContainerReference(this.ContainerName);

            var blob = photoContainer.GetBlockBlobReference(fileName);
            blob.UploadText(content);

            return blob.Uri;
        }

        public Uri UploadBinaryDataToBlob(string fileName, Stream content)
        {
            CloudBlobClient blobClient = this.StorageAccount.CreateCloudBlobClient();
            CloudBlobContainer photoContainer = blobClient.GetContainerReference(this.ContainerName);

            var blob = photoContainer.GetBlockBlobReference(fileName);
            blob.UploadFromStream(content);

            return blob.Uri;
        }

        public async Task<IEnumerable<AssessmentImportViewModel>> Get()
        {
            //note the browser will get the actual images directly from the container we are not passing actual files back just references
            CloudBlobClient blobClient = this.StorageAccount.CreateCloudBlobClient();
            CloudBlobContainer photoContainer = blobClient.GetContainerReference(this.ContainerName);
            

            if (! await photoContainer.ExistsAsync())
            {
                await photoContainer.CreateAsync(BlobContainerPublicAccessType.Blob, null, null);                
            }

            var photos = new List<AssessmentImportViewModel>();
            var blobItems = photoContainer.ListBlobs();

            foreach (CloudBlockBlob blobItem in blobItems.Where(bi => bi.GetType() == typeof(CloudBlockBlob)))
            {
                await blobItem.FetchAttributesAsync();

                photos.Add(new AssessmentImportViewModel
                {
                    Name = blobItem.Name,
                    Size = blobItem.Properties.Length / 1024,
                    Created = blobItem.Metadata["Created"] == null ? DateTime.Now : DateTime.Parse(blobItem.Metadata["Created"]),
                    Modified = ((DateTimeOffset)blobItem.Properties.LastModified).DateTime,
                    Url = blobItem.Uri.AbsoluteUri
                });                    
            }
     
            return photos;
        }

        public async Task<IEnumerable<TextReader>> Download(string uri)
        {
            //note the browser will get the actual images directly from the container we are not passing actual files back just references
            CloudBlobClient blobClient = this.StorageAccount.CreateCloudBlobClient();
            CloudBlobContainer photoContainer = blobClient.GetContainerReference(this.ContainerName);


            //if (!await photoContainer.ExistsAsync())
            //{
            //    await photoContainer.CreateAsync(BlobContainerPublicAccessType.Blob, null, null);
            //}

            var photos = new List<TextReader>();
            var blobItems = photoContainer.ListBlobs();

            foreach (CloudBlockBlob blobItem in blobItems.Where(bi => bi.GetType() == typeof(CloudBlockBlob) && bi.Uri.AbsoluteUri == uri))
            {
                var stream = new MemoryStream();
                blobItem.DownloadToStream(stream);
                stream.Seek(0, SeekOrigin.Begin);

                TextReader textReader = new StreamReader(stream);

                photos.Add(textReader);
            }

            return photos;
        }

        public async Task<PhotoActionResult> Delete(string fileName)
        {
            CloudBlobClient blobClient = this.StorageAccount.CreateCloudBlobClient();
            CloudBlobContainer photoContainer = blobClient.GetContainerReference(this.ContainerName);

            try
            {
                var blob = await photoContainer.GetBlobReferenceFromServerAsync(fileName);
                await blob.DeleteAsync();

                return new PhotoActionResult { Successful = true, Message = fileName + "deleted successfully" };
            }
            catch(Exception ex)
            {
                return new PhotoActionResult { Successful = false, Message = "error deleting fileName " + ex.GetBaseException().Message };
            }
        }

        public async Task<ImportTestDataViewModel> Add(HttpRequestMessage request)
        {
            CloudBlobClient blobClient = this.StorageAccount.CreateCloudBlobClient();
            CloudBlobContainer photoContainer = blobClient.GetContainerReference(this.ContainerName);

            var provider = new AzureBlobMultipartFormDataStreamProvider(photoContainer);

            //var schoolYear = provider.FormData["SchoolYear"];

            await request.Content.ReadAsMultipartAsync(provider);

            var photos = new List<AssessmentImportViewModel>();
            
            foreach (var file in provider.FileStreams)
            {
                //the LocalFileName is going to be the absolute Uri of the blob (see GetStream)
                //use it to get the blob info to return to the client
                var blob = await photoContainer.GetBlobReferenceFromServerAsync(file.Key);
                await blob.FetchAttributesAsync();

                photos.Add(new AssessmentImportViewModel
                {
                    Name = blob.Name,
                    Size = blob.Properties.Length / 1024,
                    Created = blob.Metadata["Created"] == null ? DateTime.Now : DateTime.Parse(blob.Metadata["Created"]),
                    Modified = ((DateTimeOffset)blob.Properties.LastModified).DateTime,
                    Url = blob.Uri.AbsoluteUri
                });
            }

            return new ImportTestDataViewModel
            {
                Files = photos,
                FormData = provider.FormData
            };      
        }

        public async Task<bool> FileExists(string fileName)
        {
            CloudBlobClient blobClient = this.StorageAccount.CreateCloudBlobClient();
            CloudBlobContainer photoContainer = blobClient.GetContainerReference(this.ContainerName);

            var blob = await photoContainer.GetBlobReferenceFromServerAsync(fileName);
            return await blob.ExistsAsync();
        }
    }
}