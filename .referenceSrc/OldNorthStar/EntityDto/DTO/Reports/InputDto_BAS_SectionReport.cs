using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Reports
{
    public class InputDto_BAS_SectionReport
    {
        public int AssessmentId { get; set; }
        public int SectionId { get; set; }
        public int SchoolYear { get; set; }
        public int HRSFormId { get; set; }
        public int HRSFormId2 { get; set; }
        public int HRSFormId3 { get; set; }
    }
}
