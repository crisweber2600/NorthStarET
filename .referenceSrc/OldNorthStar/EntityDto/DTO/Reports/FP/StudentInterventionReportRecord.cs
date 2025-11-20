using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Reports.FP
{
    public class StudentInterventionReportRecord
    {
        public int StudentId { get; set; }
        public string InterventionType { get; set; }
        public int Tier { get; set; }
        public int NumberOfLessons { get; set; }
        public string StaffInitials { get; set; }
        public int InterventionGroupId { get; set; }
        public int EndOrder { get; set; }
        public DateTime? EndDate { get; set; }
        public int StintId { get; set; }
        public int InterventionistId { get; set; }
        public int SchoolStartYear { get; set; }
        public int SchoolId { get; set; }
    }
}
