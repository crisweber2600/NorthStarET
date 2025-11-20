using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Simple
{
    public class TestDueDateDto : BaseEntityNoTrack
    {
        public int? SchoolStartYear { get; set; }
        public DateTime? DueDate { get; set; }
        public int? TestLevelPeriodID { get; set; }
        public string Notes { get; set; }
        public string Hex { get; set; }
        public bool? IsSupplemental { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string DisplayDate { get { return DueDate.HasValue ? DueDate.Value.ToString("dd-MMM-yyyy") : "N/A"; } }
        public string PeriodName { get; set; }
    }
}
