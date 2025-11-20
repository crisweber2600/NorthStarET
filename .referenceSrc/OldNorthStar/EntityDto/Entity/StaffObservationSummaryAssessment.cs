using NorthStar4.CrossPlatform.Entity;
using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.Entity
{
    public class StaffObservationSummaryAssessment : BaseEntityNoTrack
    {
        public int AssessmentId { get; set; }
        public int StaffId { get; set; }
        public bool Hidden { get; set; } // This really isn't necessary, since just being on this list means its hidden, but this is just in case
        public Assessment Assessment { get; set; }
        public Staff Staff { get; set; }
    }

    public class StaffStudentAttribute : BaseEntityNoTrack
    {
        public int AttributeId { get; set; }
        public int StaffId { get; set; }
        public bool Visible { get; set; } // This really isn't necessary, since just being on this list means its hidden, but this is just in case
        public StudentAttributeType Attribute { get; set; }
        public Staff Staff { get; set; }
    }

    public class StaffObservationSummaryAssessmentField : BaseEntityNoTrack
    {
        public int AssessmentId { get; set; }
        public int AssessmentFieldId { get; set; }
        public int StaffId { get; set; }
        public bool Hidden { get; set; } // This really isn't necessary, since just being on this list means its hidden, but this is just in case

        public Assessment Assessment { get; set; }
        public AssessmentField AssessmentField { get; set; }
        public Staff Staff { get; set; }

    }
}
