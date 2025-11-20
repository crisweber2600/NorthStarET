using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Simple
{
    public class InputDto_AssessmentSectionBenchmark
    {
        public int AssessmentId { get; set; }
        public int SectionId { get; set; }
        public int BenchmarkDateId { get; set; }
    }
}
