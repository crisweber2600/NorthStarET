using EntityDto.DTO.Admin.InterventionToolkit;
using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.DTO
{
    public class VideoDto : BaseEntityNoTrack
    {
        public int PageId { get; set; }

        public string VideoName { get; set; }
        public int? ChapterStartTime { get; set; }
        public int? ParentVideoId { get; set; }
        public string EncodedVideoURL { get; set; }
        public string VideoFileName { get; set; }
        public string Description { get; set; }
        public string VideoLength { get; set; }
        public string FileExtension { get; set; }
        public string FileSize { get; set; }
        public DateTime? LastModified { get; set; }
        public int? SortOrder { get; set; }
        public string VideoStreamId { get; set; }
        public string ThumbnailURL { get; set; }
        public OutputDto_DropdownData_VzaarVideo UploadedVideoFile { get; set; }
    }
}
