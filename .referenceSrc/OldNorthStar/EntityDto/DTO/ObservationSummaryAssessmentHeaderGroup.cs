using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.DTO
{
	public class ObservationSummaryAssessmentHeaderGroup
	{
		public ObservationSummaryAssessmentHeaderGroup()
		{
			//Headers = new List<ObservationSummaryAssessmentHeader>();
		}
		public int AssessmentId { get; set; }
		public string AssessmentName { get; set; }
		public int AssessmentOrder { get; set; }
		public int FieldCount { get; set; }

		public List<ObservationSummaryAssessmentHeader> Headers { get; set; } 
	}
}
