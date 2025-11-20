using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Section
{
    public class InputDto_GetStudentAssessmentResult
    {
        public int AssessmentId { get; set; }
        public int SectionId { get; set; }
        public int BenchmarkDateId { get; set; }
        public int StudentResultId { get; set; }
        public int StudentId { get; set; }
    }

    public class InputDto_GetStudentProgressMonResult
    {
        public int AssessmentId { get; set; }
        public int InterventionGroupId { get; set; }
        public int StudentResultId { get; set; }
        public int StudentId { get; set; }
        public DateTime? TestDate { get; set; }
    }
}
