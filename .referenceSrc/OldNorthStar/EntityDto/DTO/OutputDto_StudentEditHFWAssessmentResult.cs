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
    public class OutputDto_StudentEditHFWAssessmentResult : OutputDto_Base
    {
        public AssessmentHFWStudentResult StudentResult { get; set; }
        public AssessmentDto Assessment { get; set; }
        public List<TestDueDateDto> TestDueDates { get; set; }
        public int WordCount { get; set; }
    }
}
