using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Student
{
    public class InputDto_SaveStudentNote
    {
        public int NoteId { get; set; }
        public string NoteHtml { get; set; }
        public int StudentId { get; set; }
    }
}
