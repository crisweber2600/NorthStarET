using EntityDto.LoginDB.DTO;
using NorthStar4.CrossPlatform.Entity;
using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.InterventionToolkit
{
    public class InterventionDto : BaseEntityNoTrack
    {
        public InterventionDto()
        {
            this.InterventionGrades = new HashSet<OutputDto_DropdownData>();
            this.InterventionTools = new List<NSInterventionToolDto>();
            this.AssessmentTools = new List<NSInterventionToolDto>();
            this.Videos = new List<NSInterventionVideoDto>();
        }

        public string InterventionType { get; set; }
        public bool bDisplay { get; set; }
        public string Description { get; set; }
        public int? DefaultTextLevelType { get; set; }
        public int? InterventionCardinalityID { get; set; }
        public string ExitCriteria { get; set; }
        public string EntranceCriteria { get; set; }
        public string LearnerNeed { get; set; }
        public string DetailedDescription { get; set; }
        public string TimeOfYear { get; set; }
        public int? InterventionTierID { get; set; }
        public int? CategoryID { get; set; }
        public string BriefDescription { get; set; }
        public int? FrameworkID { get; set; }
        public int? UnitOfStudyID { get; set; }
        public int? WorkshopID { get; set; }
        public string TierColor { get; set; }
        public string TierLabel { get; set; }

        public virtual ICollection<OutputDto_DropdownData> InterventionGrades { get; set; }
        public virtual OutputDto_DropdownData InterventionWorkshop { get; set; }
        public List<NSInterventionToolDto> InterventionTools { get; set; }
        public List<NSInterventionToolDto> AssessmentTools { get; set; }
        public List<NSInterventionVideoDto> Videos { get; set; }
    }
}
