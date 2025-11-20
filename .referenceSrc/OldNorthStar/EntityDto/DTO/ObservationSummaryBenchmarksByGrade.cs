using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NorthStar4.PCL.Entity;

namespace NorthStar4.PCL.DTO
{
	public class ObservationSummaryBenchmarksByGrade
	{
        public ObservationSummaryBenchmarksByGrade()
        {
            Benchmarks = new List<ObservationSummaryBenchmark>();
        }

		public int GradeId { get; set; }
        public List<ObservationSummaryBenchmark> Benchmarks { get; set; }
    }
}
