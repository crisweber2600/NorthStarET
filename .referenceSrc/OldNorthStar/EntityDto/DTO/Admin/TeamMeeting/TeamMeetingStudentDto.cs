using EntityDto.DTO.Admin.Simple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.TeamMeeting
{
    public class TeamMeetingStudentDto
    {
        public int ID { get; set; }

        public int TeamMeetingID { get; set; }

        public int SchoolID { get; set; }

        public int StaffID { get; set; }

        public int SectionID { get; set; }

        public int StudentID { get; set; }

        public string Notes { get; set; }

    }
}
