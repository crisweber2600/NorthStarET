using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Assessment
{
    public class SchoolAssessmentDto : BaseEntityNoTrack
    {
        public int AssessmentId { get; set; }
        public int SchoolId { get; set; }
        public bool AssessmentIsAvailable { get; set; }
        public bool IsDisabled { get; set; }
        public string AssessmentName { get; set; }
        public int TestType { get; set; }
        public string AssessmentDescription { get; set; }
    }
}
