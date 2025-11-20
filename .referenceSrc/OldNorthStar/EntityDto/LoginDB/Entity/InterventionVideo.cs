using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.Entity
{
    public class NSInterventionVideo : BaseEntity
    {
        public NSInterventionVideo()
        {
            this.InterventionVideoInterventions = new HashSet<NSInterventionVideoNSIntervention>();
            this.ChildVideos = new HashSet<NSInterventionVideo>();
            this.InterventionVideoDistricts = new HashSet<NSInterventionVideoDistrict>();
            this.InterventionVideoGrades = new HashSet<InterventionVideoGrade>();
        }

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

        public virtual ICollection<NSInterventionVideoNSIntervention> InterventionVideoInterventions { get; set; }
        public virtual ICollection<NSInterventionVideo> ChildVideos { get; set; }
        public virtual NSInterventionVideo ParentVideo { get; set; }
        public virtual ICollection<NSInterventionVideoDistrict> InterventionVideoDistricts { get; set; }
        public virtual ICollection<InterventionVideoGrade> InterventionVideoGrades { get; set; }
    }
}
