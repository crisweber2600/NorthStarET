using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.TeamMeeting
{
    public class InputDto_SendTMInvitation
    {
        public int TeamMeetingId { get; set; }
        public int StaffId { get; set; }
    }
}
