using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Reports.ObservationSummary
{
    public class InputDto_ObservationSummaryTeamMeeting
    {
        public int TeamMeetingId { get; set; }
        public int? StaffId { get; set; }
        public int TestDueDateId { get; set; }
    }
}
