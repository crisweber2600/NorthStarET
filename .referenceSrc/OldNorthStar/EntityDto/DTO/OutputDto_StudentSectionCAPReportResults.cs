using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NorthStar4.PCL.Entity;
using EntityDto.DTO.Assessment;
using EntityDto.DTO.Admin.Simple;

namespace NorthStar4.PCL.DTO
{
	public class OutputDto_StudentSectionCAPReportResults : OutputDto_Base
    {
		public OutputDto_StudentSectionCAPReportResults()
		{
            StudentSectionReportResults = new List<StudentSectionCAPReportResult>();
		}

        public List<StudentSectionCAPReportResult> StudentSectionReportResults { get; set; }
		//public List<AssessmentStudentResult> StudentResults { get; set; }
		public AssessmentDto Assessment { get; set; }
        public List<TestDueDateDto> TestDueDates { get; set; }
        public List<AssessmentFieldDto> HeaderFields { get; set; }
	}

    public class OutputDto_StudentSectionAVMRSingleDateReportResults : OutputDto_Base
    {
        public OutputDto_StudentSectionAVMRSingleDateReportResults()
        {
            StudentSectionReportResults = new List<StudentSectionAVMRSingleDateReportResult>();
        }

        public List<StudentSectionAVMRSingleDateReportResult> StudentSectionReportResults { get; set; }
        //public List<AssessmentStudentResult> StudentResults { get; set; }
        public AssessmentDto Assessment { get; set; }
        public List<AssessmentFieldDto> HeaderFields { get; set; }
    }

    public class OutputDto_StudentSectionAVMRSingleDateDetailReportResults : OutputDto_Base
    {
        public OutputDto_StudentSectionAVMRSingleDateDetailReportResults()
        {
            StudentSectionReportResults = new List<AssessmentStudentResult>();
        }

        public List<AssessmentStudentResult> StudentSectionReportResults { get; set; }
        //public List<AssessmentStudentResult> StudentResults { get; set; }
        public AssessmentDto Assessment { get; set; }
        public List<AssessmentFieldDto> HeaderFields { get; set; }
    }

    public class OutputDto_StudentSectionAVMRReportResults : OutputDto_Base
    {
        public OutputDto_StudentSectionAVMRReportResults()
        {
            StudentSectionReportResults = new List<StudentSectionCAPReportResult>();
        }

        public List<StudentSectionCAPReportResult> StudentSectionReportResults { get; set; }
        //public List<AssessmentStudentResult> StudentResults { get; set; }
        public AssessmentDto Assessment { get; set; }
        public List<TestDueDateDto> TestDueDates { get; set; }
        public List<AssessmentFieldDto> HeaderFields { get; set; }

        public List<AssessmentFieldGroupContainerDto> HeaderFieldGroups { get; set; }
    }
}
