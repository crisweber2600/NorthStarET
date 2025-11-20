using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.Entity
{
	public class AssessmentFieldGroupDto : BaseEntityNoTrack
    {
        public int AssessmentId { get; set; }
        public int SortOrder { get; set; }
        public int? AltOrder { get; set; }
        public string DisplayName { get; set; }
        public string AltDisplayLabel { get; set; }
        public string Description { get; set; }
        public int? PreviousId { get; set; }
        public bool IsFlaggedForDelete { get; set; }
        public bool IsKdg { get; set; }
        public bool? Flag1 { get; set; }
        public bool? Flag2 { get; set; }
        public bool? Flag3 { get; set; }
        public bool? Flag4 { get; set; }
        public bool? Flag5 { get; set; }
        public int? AssessmentFieldGroupContainerId { get; set; }
        public AssessmentFieldGroupContainerDto Container { get; set; }
    }

    public class AssessmentFieldGroupContainerDto : BaseEntityNoTrack
    {
        public AssessmentFieldGroupContainerDto()
        {
            FieldGroups = new List<AssessmentFieldGroupDto>();
        }

        public int AssessmentId { get; set; }
        public int SortOrder { get; set; }
        public int? AltOrder { get; set; }
        public string DisplayName { get; set; }
        public string AltDisplayLabel { get; set; }
        public string Description { get; set; }
        public bool IsFlaggedForDelete { get; set; }

        public bool? Flag1 { get; set; }
        public bool? Flag2 { get; set; }
        public bool? Flag3 { get; set; }
        public bool? Flag4 { get; set; }
        public bool? Flag5 { get; set; }
        public virtual ICollection<AssessmentFieldGroupDto> FieldGroups { get; set; }
    }
}
