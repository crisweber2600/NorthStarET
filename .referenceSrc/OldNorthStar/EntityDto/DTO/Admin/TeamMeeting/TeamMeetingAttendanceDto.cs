using EntityDto.DTO.Admin.Simple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.TeamMeeting
{
    public class TeamMeetingAttendanceDto
    {
        public int ID { get; set; }

        public int TeamMeetingID { get; set; }

        public int StaffID { get; set; }

        public DateTime? NoticeSent { get; set; }

        public bool Attended { get; set; }

        public int? SchoolID { get; set; }

        public bool IncludeAllStudents { get; set; }

        public virtual StaffDto Staff { get; set; }

    }
}
