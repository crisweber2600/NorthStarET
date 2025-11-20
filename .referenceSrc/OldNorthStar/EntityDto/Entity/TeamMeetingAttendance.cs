using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.Entity
{
    public class TeamMeetingAttendance
    {
        public int ID { get; set; }

        public int TeamMeetingID { get; set; }

        public int StaffID { get; set; }

        public DateTime? NoticeSent { get; set; }

        public bool Attended { get; set; }

        public int? SchoolID { get; set; }

        public bool IncludeAllStudents { get; set; }

        public virtual School School { get; set; }

        public virtual Staff Staff { get; set; }

        public virtual TeamMeeting TeamMeeting { get; set; }
    }
}
