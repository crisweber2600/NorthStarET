using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.TeamMeeting
{
    public class AttendeeGroupStaffDto : BaseEntityNoTrack
    {
        public int StaffId { get; set; }
        public int AttendeeGroupId { get; set; }
    }
}
