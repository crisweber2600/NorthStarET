using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Reports.FP
{
    public class StudentPreviousGradeReportRecord
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? PreviousFPID { get; set; }
        public string PreviousFP { get; set; }
        public int? PreviousFPOrder { get; set; }
        public string PreviousGradeLabel { get; set; }
    }
}
