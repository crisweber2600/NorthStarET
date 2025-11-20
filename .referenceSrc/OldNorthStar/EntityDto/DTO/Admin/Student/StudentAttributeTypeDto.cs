using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Student
{
    public class StudentAttributeTypeDto
    {
        public StudentAttributeTypeDto()
        {
            LookupValues = new List<StudentAttributeLookupValueDto>();
        }

        public int Id { get; set; }
        public string AttributeName { get; set; }
        public List<StudentAttributeLookupValueDto> LookupValues { get; set; }
    }
}
