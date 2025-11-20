using EntityDto.DTO.Admin.Simple;
using EntityDto.Entity;
using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.TeamMeeting
{
    public class TeamMeetingDto
    {
        public TeamMeetingDto()
        {
            TeamMeetingAttendances = new HashSet<TeamMeetingAttendanceDto>();
            TeamMeetingStudents = new HashSet<TeamMeetingStudentDto>();
            TeamMeetingStudentNotes = new HashSet<TeamMeetingStudentNoteDto>();
            TeamMeetingManagers = new HashSet<TeamMeetingManagerDto>();
        }
        public int Id { get; set; }
        public int? TestDueDateId { get; set; }
        public string Title { get; set; }
        public string Comments { get; set; }
        public DateTime MeetingTime { get; set; }
        public int NumStudents { get; set; }
        public int NumStaffInvited { get; set; }
        public int NumStaffAttended { get; set; }
        public StaffDto Staff { get; set; }
        public int SchoolYear { get; set; }
        public string Creator { get; set; }
        public ICollection<TeamMeetingAttendanceDto> TeamMeetingAttendances { get; set; }
        public ICollection<TeamMeetingStudentDto> TeamMeetingStudents { get; set; }
        public ICollection<TeamMeetingStudentNoteDto> TeamMeetingStudentNotes { get; set; }
        public ICollection<TeamMeetingManagerDto> TeamMeetingManagers { get; set; }
    }
}
