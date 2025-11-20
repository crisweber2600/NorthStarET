using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.TeamMeeting
{
    public class InputDto_SaveAttendeeGroup
    {
        public InputDto_SaveAttendeeGroup()
        {
            Attendees = new List<OutputDto_DropdownData>();
        }
        public string GroupName { get; set; }
        public List<OutputDto_DropdownData> Attendees { get; set; }
    }
}
