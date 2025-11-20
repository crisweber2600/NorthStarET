using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Student
{
    public class StudentServiceDto
    {
        public int id { get; set; }
        public string text { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool? IsSpecEdLabel { get; set; }
    }
}
