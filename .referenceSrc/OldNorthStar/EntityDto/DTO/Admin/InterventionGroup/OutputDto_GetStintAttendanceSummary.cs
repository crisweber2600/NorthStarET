using EntityDto.DTO.Admin.Simple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.InterventionGroup
{
    public class OutputDto_GetStintAttendanceSummary : OutputDto_Base
    {
        public List<InterventionAttendanceDto> Notes { get; set; }
        //public List<AttendanceNotesByType> NotesGroupedByType { get; set; }
        public List<AttendanceStatusSummary> AttendanceSummary {get;set;}
    }

    public class AttendanceNotesByType
    {
        public AttendanceNotesByType()
        {
            StintNotes = new List<InterventionAttendanceDto>();
        }

        public string AttendanceStatus { get; set; }
        public List<InterventionAttendanceDto> StintNotes { get; set; }
    }

    public class AttendanceStatusSummary
    {
        //public int StatusId { get; set; }
        public int Count { get; set; }
        public string StatusLabel { get; set; }
    }
}
