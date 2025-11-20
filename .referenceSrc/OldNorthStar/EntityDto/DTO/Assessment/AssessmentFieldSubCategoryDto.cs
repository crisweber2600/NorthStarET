using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Assessment
{
    public class AssessmentFieldSubCategoryDto : BaseEntityNoTrack
    {
        public int AssessmentId { get; set; }
        public int SortOrder { get; set; }
        public int? AltOrder { get; set; }
        public string DisplayName { get; set; }
        public string AltDisplayLabel { get; set; }
        public string Description { get; set; }
        public int? PreviousId { get; set; }
        public bool? Flag1 { get; set; }
        public bool? Flag2 { get; set; }
        public bool? Flag3 { get; set; }
        public bool? Flag4 { get; set; }
        public bool? Flag5 { get; set; }

    }
}
