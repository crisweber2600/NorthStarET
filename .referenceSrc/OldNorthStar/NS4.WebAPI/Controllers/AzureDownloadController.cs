using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MimeTypeMap;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using System.Net.Http.Headers;
using NS4.WebAPI.Infrastructure;
using NorthStar.EF6;
using NorthStar4.API.Infrastructure;
using System.Security.Claims;
using NorthStar.EF6.DataService;
using Northstar.Core;

namespace NS4.WebAPI.Controllers
{
    [RoutePrefix("api/azuredownload")]
    [Authorize]
    public class AzureDownloadController : NSBaseController
    {

        [Route("downloadnorthstarfile")]
        [HttpGet]
        public HttpResponseMessage DownloadNorthstarFile(string fileName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);

            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference("interventiontools");
            var blob = container.GetBlockBlobReference(fileName);
                      

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
                    ContentDisposition = "attachment; filename=" + fileName,
                    ContentType = MimeTypeMap.List.MimeTypeMap.GetMimeType(Path.GetExtension(fileName)).FirstOrDefault()
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
                FileName = fileName
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return result;

            //Response
            //HttpResponseMessage response = new HttpResponseMessage();
            //response.Headers.Add();
            ////var signedBlobUrl = builder.Uri;

            
            //// Redirect
            ////return Redirect(signedBlobUrl);
        }

        public class InputDto_FileNames
        {
            public InputDto_FileNames()
            {
                FileNames = new List<string>();
            }

            public List<string> FileNames { get; set; }
        }

        [Route("downloadzippedtools")]
        [HttpPost]
        public HttpResponseMessage DownloadZippedTools([FromBody]InputDto_FileNames input)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);

            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference("interventiontools");

            using (MemoryStream outputMemoryStream = new MemoryStream())
            {
                using (ZipFileBuilder zBuilder = new ZipFileBuilder(outputMemoryStream))
                {
                    foreach (var fileName in input.FileNames)
                    {
                        var blob = container.GetBlockBlobReference(fileName);


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
                                ContentDisposition = "attachment; filename=" + fileName,
                                ContentType = MimeTypeMap.List.MimeTypeMap.GetMimeType(Path.GetExtension(fileName)).FirstOrDefault()
                            }
                            ).TrimStart('?');

                        CloudBlockBlob downloadBlobReference = new CloudBlockBlob(builder.Uri);

                        using (var stream = new MemoryStream())
                        {
                            downloadBlobReference.DownloadToStream(stream);
                            zBuilder.Add(fileName, stream);
                        }
                    }

                    zBuilder.Finish();
                    var result = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(Utility.ReadFully(outputMemoryStream))
                    };
                    result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                    {
                        FileName = "NorthStarTools_" + DateTime.Now.ToFileTime().ToString() + ".zip"
                    };
                    result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    return result;
                }
            }




            //Response
            //HttpResponseMessage response = new HttpResponseMessage();
            //response.Headers.Add();
            ////var signedBlobUrl = builder.Uri;


            //// Redirect
            ////return Redirect(signedBlobUrl);
        }

        //public static byte[] ReadFully(Stream input)
        //{
        //    byte[] buffer = new byte[16 * 1024];

        //    input.Position = 0; // Add this line to set the input stream position to 0

        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        int read;
        //        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        //        {
        //            ms.Write(buffer, 0, read);
        //        }
        //        return ms.ToArray();
        //    }
        //}
        //moved to CORE utility



        //[Route("getblobsettings")]
        //[HttpGet]
        //public void AddCorsRuleStorageClientLibrary()
        //{

        //    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);
        //    //Add a new rule.
        //    var corsRule = new CorsRule()
        //    {
        //        AllowedHeaders = new List<string> { "x-ms-*", "content-type", "accept" },
        //        AllowedMethods = CorsHttpMethods.Get,//Since we'll only be calling Put Blob, let's just allow PUT verb
        //        AllowedOrigins = new List<string> { "http://localhost:48254" },//This is the URL of our application.
        //        MaxAgeInSeconds = 1 * 60 * 60,//Let the browswer cache it for an hour
        //    };

        //    //First get the service properties from storage to ensure we're not adding the same CORS rule again.
        //    var client = storageAccount.CreateCloudBlobClient();
        //    var serviceProperties = client.GetServiceProperties();
        //    var corsSettings = serviceProperties.Cors;

        //    corsSettings.CorsRules.Add(corsRule);
        //    //Save the rule
        //    client.SetServiceProperties(serviceProperties);
        //}

        //[Route("getblobsettings")]
        //[HttpGet]
        //public void GetCorsRulesStorageClientLibrary()
        //{
        //    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);

        //    var blobClient = storageAccount.CreateCloudBlobClient();
        //    var serviceProperties = blobClient.GetServiceProperties();
        //    var corsSettings = serviceProperties.Cors;
        //    foreach (var corsRule in corsSettings.CorsRules)
        //    {
        //        StringBuilder allowedOrigins = new StringBuilder();
        //        foreach (var allowedOrigin in corsRule.AllowedOrigins)
        //        {
        //            allowedOrigins.AppendFormat("{0}, ", allowedOrigin);
        //        }
        //        StringBuilder allowedMethods = new StringBuilder();
        //        foreach (var type in Enum.GetValues(typeof(CorsHttpMethods)))
        //        {
        //            if ((CorsHttpMethods)type != CorsHttpMethods.None)
        //            {
        //                if (corsRule.AllowedMethods.HasFlag((CorsHttpMethods)type))
        //                {
        //                    allowedMethods.AppendFormat("{0}, ", (CorsHttpMethods)type);
        //                }
        //            }
        //        }
        //        StringBuilder allowedHeaders = new StringBuilder();
        //        foreach (var allowedHeader in corsRule.AllowedHeaders)
        //        {
        //            allowedHeaders.AppendFormat("{0}, ", allowedHeader);
        //        }
        //        int maxAgeInSeconds = corsRule.MaxAgeInSeconds;
        //        StringBuilder exposedHeaders = new StringBuilder();
        //        foreach (var exposedHeader in corsRule.ExposedHeaders)
        //        {
        //            exposedHeaders.AppendFormat("{0}, ", exposedHeader);
        //        }
        //        Console.WriteLine(string.Format("Allowed Origins:  {0}", allowedOrigins.ToString()));
        //        Console.WriteLine(string.Format("Allowed Methods:  {0}", allowedMethods.ToString()));
        //        Console.WriteLine(string.Format("Allowed Headers:  {0}", allowedHeaders.ToString()));
        //        Console.WriteLine(string.Format("Max Age (Seconds): {0}", maxAgeInSeconds));
        //        Console.WriteLine(string.Format("Exposed Headers:  {0}", exposedHeaders.ToString()));
        //        Console.WriteLine("==============================================================================");
        //    }
        //}
    }
}
