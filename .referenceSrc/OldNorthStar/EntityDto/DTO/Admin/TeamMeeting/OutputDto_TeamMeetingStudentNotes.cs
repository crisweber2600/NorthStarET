using EntityDto.DTO.Admin.Simple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.TeamMeeting
{
    public class OutputDto_TeamMeetingStudentNotes
    {
        public StudentDto Student { get; set; }
        public List<TeamMeetingStudentNoteDto> Notes { get; set; }
    }
}
