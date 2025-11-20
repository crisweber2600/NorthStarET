using EntityDto.DTO.Admin.Simple;
using EntityDto.DTO.Admin.TeamMeeting;
using EntityDto.Entity;
using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Section
{
    public class OutputDto_ManageTeamMeeting
    {
        public OutputDto_ManageTeamMeeting()
        {
            Sections = new List<TMSectionDetailsDto>();
            Attendees = new List<OutputDto_DropdownData>();
            Managers = new List<OutputDto_DropdownData>();
            StaffGroups = new List<OutputDto_DropdownData>();
            TeamMeetingAttendances = new List<TeamMeetingAttendanceDto>();
            TeamMeetingStudents = new List<TeamMeetingStudentDto>();
            TeamMeetingStudentNotes = new List<TeamMeetingStudentNoteDto>();
            TeamMeetingManagers = new List<TeamMeetingManagerDto>();
            AttendeeGroups = new List<AttendeeGroupDto>();
        }
        public int ID { get; set; }
        public int? TestDueDateId { get; set; }

        public string Title { get; set; }

        public string Comments { get; set; }

        public DateTime MeetingTime { get; set; }

        public int SchoolYear { get; set; }

        // this represents the list of sections that we have students for, dynamically generated from TeamMeetingStudents
        public List<TMSectionDetailsDto> Sections { get; set; }
        public List<OutputDto_DropdownData> Attendees { get; set; }
        public List<OutputDto_DropdownData> Managers { get; set; }
        public List<OutputDto_DropdownData> StaffGroups { get; set; }
        public List<AttendeeGroupDto> AttendeeGroups { get; set; }

        // need these because they will contain additional data when this dto is used on the invitation screen
        public List<TeamMeetingAttendanceDto> TeamMeetingAttendances { get; set; }

        public List<TeamMeetingStudentDto> TeamMeetingStudents { get; set; }
        // a link/button will be available on the manage team meetings page to view all of the notes from the team meeting?
        public List<TeamMeetingStudentNoteDto> TeamMeetingStudentNotes { get; set; }

        public List<TeamMeetingManagerDto> TeamMeetingManagers { get; set; }

    }
}
