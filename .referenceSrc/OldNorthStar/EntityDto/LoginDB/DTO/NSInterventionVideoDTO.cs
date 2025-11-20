using EntityDto.DTO.Admin.InterventionToolkit;
using NorthStar4.PCL.DTO;
using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.DTO
{
    public class NSInterventionVideoDto : BaseEntity
    {
        public NSInterventionVideoDto()
        {
            Districts = new List<OutputDto_DropdownData>();
            Grades = new List<OutputDto_DropdownData>();
            UploadedVideoFile = new OutputDto_DropdownData_VzaarVideo();
            Interventions = new List<OutputDto_DropdownData>();
        }

        public string VideoName { get; set; }
        public int? ChapterStartTime { get; set; }
        public int? ParentVideoId { get; set; }
        public string EncodedVideoURL { get; set; }
        public OutputDto_DropdownData_VzaarVideo UploadedVideoFile { get; set; }
        public string Description { get; set; }
        public string VideoLength { get; set; }
        public string FileExtension { get; set; }
        public string FileSize { get; set; }
        public DateTime? LastModified { get; set; }
        public int? SortOrder { get; set; }
        public string VideoStreamId { get; set; }
        public string ThumbnailURL { get; set; }

        public List<OutputDto_DropdownData> Districts { get; set; }
        public List<OutputDto_DropdownData> Grades { get; set; }
        public List<OutputDto_DropdownData> Interventions { get; set; }
    }
}
