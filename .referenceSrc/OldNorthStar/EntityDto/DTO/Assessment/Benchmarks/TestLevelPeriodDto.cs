using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Assessment.Benchmarks
{
    public class TestLevelPeriodDto : BaseEntityNoTrack
    {
        public string Title { get; set; }
        public int PeriodOrder { get; set; }
        public string Notes { get; set; }
    }
}
