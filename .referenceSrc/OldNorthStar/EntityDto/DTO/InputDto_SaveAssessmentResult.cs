using EntityDto.DTO.Admin.Simple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.DTO
{
	public class InputDto_SaveAssessmentResult : OutputDto_Base
	{
		public int AssessmentId { get; set; }
        public int BenchmarkDateId { get; set; }
		public AssessmentStudentResult StudentResult { get; set; }
	}

    public class InputDto_SaveProgMonResult : OutputDto_Base
    {
        public int AssessmentId { get; set; }
        public int InterventionGroupId { get; set; }
        public AssessmentStudentResult StudentResult { get; set; }
    }
}
