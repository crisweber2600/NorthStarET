using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.TeamMeeting
{
    public class OutputDto_GetTeamMeetingList
    {
        public OutputDto_GetTeamMeetingList()
        {
            TeamMeetings = new List<TeamMeetingDto>();
        }
        public List<TeamMeetingDto> TeamMeetings { get; set; }
    }
}
