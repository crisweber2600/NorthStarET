using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Student
{
    public class InputDto_GetStudentList
    {
        public int SchoolId { get; set; }
        public int GradeId { get; set; }
        public int TeacherId { get; set; }
        public int SchoolYear { get; set; }
        public int SectionId { get; set; }
        public int StudentId { get; set; }
    }
}
