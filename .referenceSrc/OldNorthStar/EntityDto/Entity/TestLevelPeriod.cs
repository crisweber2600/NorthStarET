using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.Entity
{
    public class TestLevelPeriod: BaseEntityNoTrack
    {
        public TestLevelPeriod()
        {
            AssessmentBenchmarks = new List<Assessment_Benchmarks>();
        }

        public string Title { get; set; }
        public int PeriodOrder { get; set; }
        public string Notes { get; set; }

        public List<Assessment_Benchmarks> AssessmentBenchmarks { get; set; }
    }
}
