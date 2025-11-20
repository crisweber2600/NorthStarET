using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.DTO
{
	public class InputDto_SaveHFWAssessmentResult
    {
		public int AssessmentId { get; set; }

		public AssessmentHFWStudentResult StudentResult { get; set; }
	}
}
