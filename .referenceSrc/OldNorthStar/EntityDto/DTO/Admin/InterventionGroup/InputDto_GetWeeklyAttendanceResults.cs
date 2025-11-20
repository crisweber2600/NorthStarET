using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.InterventionGroup
{
    public class InputDto_GetWeeklyAttendanceResults
    {
        public int InterventionGroupId { get; set; }
        public int StaffId { get; set; }
        public int SchoolStartYear { get; set; }
        public DateTime MondayDate { get; set; }
        public int SchoolId { get; set; }
    }
}
