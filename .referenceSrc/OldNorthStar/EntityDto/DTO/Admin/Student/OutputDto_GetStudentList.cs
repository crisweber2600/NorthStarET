using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Student
{
    public class OutputDto_GetStudentList
    {
        public OutputDto_GetStudentList()
        {

        }

        public List<StudentListDto> Students { get; set; }
    }
}
