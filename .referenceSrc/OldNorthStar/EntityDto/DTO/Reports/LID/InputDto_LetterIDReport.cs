using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Reports.LID
{
    public class InputDto_LetterIDReport
    {
        public int AssessmentId { get; set; }
        public int SectionId { get; set; }
        public int SchoolYear { get; set; }
        public string ReportType { get; set; }
    }
}
