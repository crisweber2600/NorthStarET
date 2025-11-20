using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace NorthStar.Core.FileUpload
{
    public class AzureBlobMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
    {
        private CloudBlobContainer BlobContainer { get; set; }
        private readonly Collection<bool> _isFormData = new Collection<bool>();
        private readonly NameValueCollection _formData = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Stream> _fileStreams = new Dictionary<string, Stream>();


        public AzureBlobMultipartFormDataStreamProvider(CloudBlobContainer blobContainer): base("azure")
        {
            this.BlobContainer = blobContainer;
        }

        public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
        {
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }
            if (headers == null)
            {
                throw new ArgumentNullException("headers");
            }
            

            var fileName = this.GetLocalFileName(headers);;
            var contentDisposition = headers.ContentDisposition;
            if (contentDisposition == null)
            {
                throw new InvalidOperationException("Did not find required 'Content-Disposition' header field in MIME multipart body part.");
            }

            _isFormData.Add(String.IsNullOrEmpty(contentDisposition.FileName));
            if (String.IsNullOrEmpty(contentDisposition.FileName))
            {
                return base.GetStream(parent, headers);
            }

            CloudBlockBlob blob = this.BlobContainer.GetBlockBlobReference(fileName);
            blob.Metadata["Created"] = DateTime.Now.ToString();

            if (headers.ContentType != null)
            {
                headers.ContentDisposition.FileName = fileName;
                blob.Properties.ContentType = headers.ContentType.MediaType;
            }

            this.FileData.Add(new MultipartFileData(headers, blob.Name));

            return blob.OpenWrite();            
        }

        private static string UnquoteToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return token;
            }

            if (token.StartsWith("\"", StringComparison.Ordinal) && token.EndsWith("\"", StringComparison.Ordinal) && token.Length > 1)
            {
                return token.Substring(1, token.Length - 2);
            }

            return token;
        }

        public Dictionary<string, Stream> FileStreams
        {
            get { return _fileStreams; }
        }


        public override async Task ExecutePostProcessingAsync()
        {

            for (var index = 0; index < Contents.Count; index++)
            {
                HttpContent formContent = Contents[index];
                if (_isFormData[index])
                {
                    // Field
                    string formFieldName = UnquoteToken(formContent.Headers.ContentDisposition.Name) ?? string.Empty;
                    string formFieldValue = await formContent.ReadAsStringAsync();
                    FormData.Add(formFieldName, formFieldValue);
                }
                else
                {
                    // File
                    string fileName = UnquoteToken(formContent.Headers.ContentDisposition.FileName);
                    Stream stream = await formContent.ReadAsStreamAsync();
                    FileStreams.Add(fileName, stream);
                }
            }
        }

        public override string GetLocalFileName(System.Net.Http.Headers.HttpContentHeaders headers)
        {
            //Make the file name URL safe and then use it & is the only disallowed url character allowed in a windows filename            
            var name = !string.IsNullOrWhiteSpace(headers.ContentDisposition.FileName) ? headers.ContentDisposition.FileName : "NoName";

            name = Guid.NewGuid().ToString() + "_" + name.Trim(new char[] { '"' })
                        .Replace("&", "and");

            //IE sets the full path as the file name 
            name =  Path.GetFileName(name);

            return name;
        }
    }
}