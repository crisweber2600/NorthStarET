using NorthStar4.CrossPlatform.Entity;
using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.Entity
{
    public class District_Benchmark : BaseEntityNoTrack
    {
        public int AssessmentID { get; set; }
        public int GradeID {get;set;}
        public int TestLevelPeriodID { get; set; }
        public string AssessmentField { get; set; }
        public decimal? DoesNotMeet { get; set; }
        public decimal? Approaches { get; set; }
        public decimal? Meets { get; set; }
        public decimal? Exceeds { get; set; }
        public Assessment Assessment { get; set; }
        public Grade Grade { get; set; }
        public TestLevelPeriod TestLevelPeriod { get; set; }
    }
}
