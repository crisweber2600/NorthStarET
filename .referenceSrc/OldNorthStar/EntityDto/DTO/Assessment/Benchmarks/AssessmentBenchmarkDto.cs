using NorthStar4.CrossPlatform.Entity;
using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Assessment.Benchmarks
{
    public class AssessmentBenchmarkDto : BaseEntityNoTrack
    {
        public AssessmentBenchmarkDto()
        {

        }
        public int AssessmentID { get; set; }
        public int GradeID { get; set; }
        public string GradeName { get; set; }
        public int TestLevelPeriodID { get; set; }
        public string TestLevelPeriodName { get; set; }
        public string AssessmentField { get; set; }
        public decimal? DoesNotMeet { get; set; }
        public string DoesNotMeetLabel { get; set; }
        public OutputDto_BenchmarkResult DoesNotMeetResult
        {
            get; set;
        }
        public OutputDto_BenchmarkResult ApproachesResult
        {
            get; set;
        }
        public OutputDto_BenchmarkResult MeetsResult
        {
            get; set;
        }
        public OutputDto_BenchmarkResult ExceedsResult
        {
            get; set;
        }

        public decimal? Approaches { get; set; }
        public string ApproachesLabel { get; set; }
        public decimal? Meets { get; set; }
        public string MeetsLabel { get; set; }
        public decimal? Exceeds { get; set; }
        public string ExceedsLabel { get; set; }
    }
}
