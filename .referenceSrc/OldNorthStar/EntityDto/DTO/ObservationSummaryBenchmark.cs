using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NorthStar4.PCL.Entity;

namespace NorthStar4.PCL.DTO
{
	public class ObservationSummaryBenchmark
	{
		public int AssessmentId { get; set; }
		public int FieldOrder { get; set; }
		public string DbColumn { get; set; }
		public decimal? Exceeds { get; set; }
		public decimal? Meets { get; set; }
		public decimal? Approaches { get; set; }
        public decimal? DoesNotMeet { get; set; }
        public int GradeId { get; set; }
        public int TestLevelPeriodId { get; set; }
	}
}
