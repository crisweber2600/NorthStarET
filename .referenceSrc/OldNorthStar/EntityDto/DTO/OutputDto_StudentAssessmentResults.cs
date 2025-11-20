using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NorthStar4.PCL.Entity;
using EntityDto.DTO.Assessment;
using EntityDto.DTO.Admin.Simple;

namespace NorthStar4.PCL.DTO
{
	public class OutputDto_StudentAssessmentResults
	{
		public OutputDto_StudentAssessmentResults()
		{
			StudentResults = new List<AssessmentStudentResult>();
		}
       
		public List<AssessmentStudentResult> StudentResults { get; set; }
		public AssessmentDto Assessment { get; set; }
        public List<TestDueDateDto> TestDueDates { get; set; }
	}
}
