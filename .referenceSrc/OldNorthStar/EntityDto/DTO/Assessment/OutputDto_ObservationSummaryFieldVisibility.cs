using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Assessment
{
    public class OutputDto_ObservationSummaryFieldVisibility
    {
        public int id { get; set; }
        public string text { get; set; }
        public bool Visible { get; set; }
    }

    public class InputDto_ObservationSummaryFieldVisibility
    {
        public List<OutputDto_ObservationSummaryFieldVisibility> AssessmentsOrFields { get; set; }
        public bool? OSSchoolColumnVisible { get; set; }
        public bool? OSGradeColumnVisible { get; set; }
        public bool? OSTeacherColumnVisible { get; set; }
    }
}
