using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.TeamMeeting
{
    public class AttendeeGroupDto : BaseEntityNoTrack
    {
        public AttendeeGroupDto()
        {
            AttendeeGroupStaffs = new List<AttendeeGroupStaffDto>();
        }
        public string GroupName { get; set; }
        public int StaffId { get; set; }

        public List<AttendeeGroupStaffDto> AttendeeGroupStaffs { get; set; }
    }
}
