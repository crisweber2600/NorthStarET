using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.TeamMeeting
{
    public class InputDto_TeamMeetingAttendance
    {
        public TeamMeetingAttendanceDto TeamMeetingAttendance { get; set; }
    }

    public class InputDto_TeamMeetingAttendances
    {
        public List<TeamMeetingAttendanceDto> TeamMeetingAttendances { get; set; }
    }
}
