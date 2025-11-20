using EntityDto.DTO.Admin.Simple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.TeamMeeting
{
    public class OutputDto_TeamMeetingStudentAllNotes : OutputDto_Base
    {
        public OutputDto_TeamMeetingStudentAllNotes()
        {
            Meetings = new List<TeamMeetingDto>();
        }
        public StudentDto Student { get; set; }
        public List<TeamMeetingDto> Meetings {get;set;}       
    }
}
