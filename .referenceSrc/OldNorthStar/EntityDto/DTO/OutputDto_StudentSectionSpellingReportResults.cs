using EntityDto.DTO.Admin.Simple;
using EntityDto.DTO.Assessment;
using NorthStar4.PCL.DTO;
using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NorthStar4.CrossPlatform.DTO
{
    public class OutputDto_StudentSectionSpellingReportResults : OutputDto_Base
    {
        public OutputDto_StudentSectionSpellingReportResults()
        {
            StudentResults = new List<AssessmentStudentResult>();
        }

        public List<AssessmentStudentResult> StudentResults { get; set; }
        public AssessmentDto Assessment { get; set; }
        public List<AssessmentFieldDto> HeaderFields { get; set; }
    }
}
