using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NorthStar.Core.FileUpload
{
    public interface IPhotoManager
    {
        Task<IEnumerable<AssessmentImportViewModel>> Get();
        Task<PhotoActionResult> Delete(string fileName);
        Task<ImportTestDataViewModel> Add(HttpRequestMessage request);
        Task<IEnumerable<TextReader>> Download(string uri);
        Task<bool> FileExists(string fileName);
        CloudStorageAccount StorageAccount { get; }
        string ContainerName { get; set; }
    }
}
