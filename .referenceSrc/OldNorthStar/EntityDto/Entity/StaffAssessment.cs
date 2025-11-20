using NorthStar4.CrossPlatform.Entity;
using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.Entity
{
    public class StaffAssessment : BaseEntityNoTrack
    {
        public int AssessmentId { get; set; }
        public int StaffId { get; set; }
        public bool AssessmentIsAvailable { get; set; }
        public virtual Assessment Assessment { get; set; }
    }
}
