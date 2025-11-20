using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NorthStar4.PCL.Entity;
using EntityDto.DTO.Admin.Simple;
using EntityDto.DTO.Assessment;

namespace NorthStar4.PCL.DTO
{
	public class OutputDto_StudentEditAssessmentResult : OutputDto_Base
    {
		public AssessmentStudentResult StudentResult { get; set; }
		public AssessmentDto Assessment { get; set; }
        public List<TestDueDateDto> TestDueDates { get; set; }
	}
}
