using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NorthStar4.PCL.Entity;
using EntityDto.DTO.Assessment;
using EntityDto.Entity;
using EntityDto.DTO.Admin.Simple;
using EntityDto.DTO.Reports.FP;

namespace NorthStar4.PCL.DTO
{
	public class OutputDto_StudentSectionWVReportResults : OutputDto_Base
    {
		public OutputDto_StudentSectionWVReportResults()
		{
            StudentSectionReportResults = new List<StudentSectionWVReportResult>();
            
		}

        public List<StudentSectionWVReportResult> StudentSectionReportResults { get; set; }
		//public List<AssessmentStudentResult> StudentResults { get; set; }
		public AssessmentDto Assessment { get; set; }
        public List<TestDueDateDto> TestDueDates { get; set; }
        public List<AssessmentFieldDto> HeaderFields { get; set; }
        public List<OutputDto_DropdownData> Scale
        {
            get
            {
                var list = new List<OutputDto_DropdownData>();
                for (var i = 0; i < 57; i++)
                {
                    list.Add(new OutputDto_DropdownData { id = i, text = i.ToString("D2") });
                }

                return list.OrderByDescending(p => p.id).ToList();
            }
            set { }
        }
    }
}
