using EntityDto.DTO.Admin.Simple;
using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Assessment
{
    public class OutputDto_ObservationSummaryAssessments : OutputDto_Base
    {
        public List<OutputDto_ObservationSummaryFieldVisibility> BenchmarkAssessments { get; set; }
        public List<OutputDto_ObservationSummaryFieldVisibility> StateTests { get; set; }

        public bool OSSchoolVisible { get; set; }
        public bool OSGradeVisible { get; set; }
        public bool OSTeacherVisible { get; set; }
    }

    public class OutputDto_ObservationSummaryAssessmentsFields : OutputDto_Base
    {
        public List<OutputDto_ObservationSummaryFieldVisibility> Fields { get; set; }
    }
}
