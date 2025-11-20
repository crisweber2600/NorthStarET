using EntityDto.DTO.Admin.Simple;
using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Student
{
    public class StudentNoteDto : BaseEntityNoTrack
    {
        public int StudentID { get; set; }

        public DateTime NoteDate { get; set; }

        public string Note { get; set; }

        public int StaffID { get; set; }

        public StaffDto Staff { get; set; }
    }
}
