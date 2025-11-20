using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NorthStar4.PCL.Entity;
using EntityDto.DTO.Assessment;
using EntityDto.DTO.Admin.Simple;

namespace NorthStar4.PCL.DTO
{
	public class OutputDto_StudentSectionLIDReportResults : OutputDto_Base
    {
		public OutputDto_StudentSectionLIDReportResults()
		{
            StudentSectionReportResults = new List<StudentSectionLIDReportResult>();
		}

        public List<StudentSectionLIDReportResult> StudentSectionReportResults { get; set; }
		//public List<AssessmentStudentResult> StudentResults { get; set; }
		public AssessmentDto Assessment { get; set; }
        public List<TestDueDate> TestDueDates { get; set; }
        public List<AssessmentFieldDto> HeaderFields { get; set; }
	}
}
