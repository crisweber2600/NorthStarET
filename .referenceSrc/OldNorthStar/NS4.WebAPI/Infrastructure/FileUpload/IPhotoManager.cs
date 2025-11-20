using EntityDto.DTO.AssessmentDataImport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebApiAngularJsAzureUploader.Models;

namespace WebApiAngularJsAzureUploader.Photo
{
    public interface IPhotoManager
    {
        Task<IEnumerable<AssessmentImportViewModel>> Get();
        Task<PhotoActionResult> Delete(string fileName);
        Task<IEnumerable<AssessmentImportViewModel>> Add(HttpRequestMessage request);
        Task<bool> FileExists(string fileName);
    }
}
