using EntityDto.DTO.Admin.Simple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.TeamMeeting
{
    public class TeamMeetingStudentNoteDto
    {
        public int ID { get; set; }

        public int TeamMeetingID { get; set; }

        public int StudentID { get; set; }

        public DateTime NoteDate { get; set; }

        public string Note { get; set; }

        public int StaffID { get; set; }

        public StaffDto Staff { get; set; }

        //public StudentDto Student { get; set; }

    }
}
