using NorthStar4.CrossPlatform.Entity;
using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.Entity
{
    public class StaffAssessmentFieldVisibility : BaseEntityNoTrack
    {
        public int StaffId { get; set; }
        public int AssessmentId { get; set; }
        public int AssessmentFieldId { get; set; }
        public bool? DisplayInObsSummary { get; set; }
        public bool? DisplayInEditResultList { get; set; }
        public bool? DisplayInLineGraphs { get; set; }

        public virtual Assessment Assessment { get; set; }
        public virtual AssessmentField AssessmentField { get; set; }
        public virtual Staff Staff { get; set; }
    }
}
