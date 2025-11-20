using EntityDto.DTO.Admin.Simple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.DTO
{
	public class OutputDto_SaveAssessmentResult : OutputDto_Base
	{
		public AssessmentStudentResult StudentResult { get; set; }
	}
}
