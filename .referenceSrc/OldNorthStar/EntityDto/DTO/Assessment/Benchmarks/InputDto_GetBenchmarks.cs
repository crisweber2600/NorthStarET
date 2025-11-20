using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Assessment.Benchmarks
{
    public class InputDto_GetBenchmarks
    {
        public int AssessmentId { get; set; }
        public string FieldName { get; set; }
        public string LookupFieldName { get; set; }
    }
}
