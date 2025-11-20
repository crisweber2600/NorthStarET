using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.DTO
{
	public class InputDto_GetAssessmentResults
	{
		public int AssessmentId { get; set; }
		public int ClassId { get; set; }
		public int BenchmarkDateId { get; set; }
		public DateTime TestDate { get; set; }
	}
}
