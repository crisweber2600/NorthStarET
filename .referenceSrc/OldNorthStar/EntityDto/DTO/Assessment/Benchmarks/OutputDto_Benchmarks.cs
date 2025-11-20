using EntityDto.DTO.Admin.Simple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Assessment.Benchmarks
{
    public class OutputDto_Benchmarks : OutputDto_Base
    {
        public List<AssessmentBenchmarkDto> Benchmarks { get; set; }
        public List<AssessmentDto> Assessments { get; set; }
    }
}
