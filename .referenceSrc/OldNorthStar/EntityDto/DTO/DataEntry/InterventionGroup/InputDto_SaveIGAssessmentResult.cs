using EntityDto.DTO.Admin.Simple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.DTO
{
	public class InputDto_SaveIGAssessmentResult : OutputDto_Base
	{
		public int AssessmentId { get; set; }
        public DateTime DateTestTaken { get; set; }
		public AssessmentStudentResult StudentResult { get; set; }
	}
}
