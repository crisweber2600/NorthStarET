using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.DataEntry
{
    public class InputDto_StudentEditHFWAssessmentResult
    {
        public int AssessmentId { get; set; }
        public int SectionId { get; set; }
        public int BenchmarkDateId { get; set; }
        public int StudentResultId { get; set; }
        public int StudentId { get; set; }
        public int LowWordOrder { get; set; }
        public int HighWordOrder { get; set; }
        public string WordOrder { get; set; }
        public bool IsKdg { get; set; }
    }
}
