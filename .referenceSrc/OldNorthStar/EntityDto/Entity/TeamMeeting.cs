using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.Entity
{
    public class TeamMeeting
    {
        public TeamMeeting()
        {
            TeamMeetingAttendances = new HashSet<TeamMeetingAttendance>();
            TeamMeetingStudents = new HashSet<TeamMeetingStudent>();
            TeamMeetingStudentNotes = new HashSet<TeamMeetingStudentNote>();
            TeamMeetingManagers = new HashSet<TeamMeetingManager>();
        }

        public int ID { get; set; }
        public int? TestDueDateId { get; set; }

        public string Title { get; set; }

        public string Comments { get; set; }

        public DateTime MeetingTime { get; set; }

        public int StaffID { get; set; }

        public int SchoolYear { get; set; }

        public virtual Staff Staff { get; set; }

        public virtual ICollection<TeamMeetingAttendance> TeamMeetingAttendances { get; set; }

        public virtual ICollection<TeamMeetingStudent> TeamMeetingStudents { get; set; }
        public virtual ICollection<TeamMeetingStudentNote> TeamMeetingStudentNotes { get; set; }

        public virtual ICollection<TeamMeetingManager> TeamMeetingManagers { get; set; }
    }
}
