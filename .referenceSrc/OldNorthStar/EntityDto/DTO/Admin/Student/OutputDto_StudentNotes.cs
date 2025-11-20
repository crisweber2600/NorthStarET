using EntityDto.DTO.Admin.Simple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Student
{
    public class OutputDto_StudentNotes
    {
        public StudentDto Student { get; set; }
        public List<StudentNoteDto> Notes { get; set; }
    }
}
