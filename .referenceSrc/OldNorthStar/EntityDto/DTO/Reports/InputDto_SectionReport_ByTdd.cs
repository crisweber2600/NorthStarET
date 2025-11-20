using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Reports
{
    public class InputDto_SectionReport_ByTdd
    {
        public int AssessmentId { get; set; }
        public int SectionId { get; set; }
        public int SchoolYear { get; set; }
        public int HRSFormId { get; set; }
        public int BenchmarkDateId { get; set; }
    }
}
