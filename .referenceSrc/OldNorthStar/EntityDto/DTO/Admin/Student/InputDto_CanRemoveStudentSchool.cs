using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Student
{
    public class InputDto_CanRemoveStudentSchool
    {
        public int StudentId { get; set; }
        public int SchoolId { get; set; }
        public int SchoolStartYear { get; set; }
    }
}
