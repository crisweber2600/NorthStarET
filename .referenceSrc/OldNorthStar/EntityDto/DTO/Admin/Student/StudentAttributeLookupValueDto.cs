using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Student
{
    public class StudentAttributeLookupValueDto
    {
        public int Id { get; set; }
        public int AttributeId { get; set; }
        public int LookupValueId { get; set; }
        public string LookupValue { get; set; }
        public bool? IsSpecialEd { get; set; }
        public string Description { get; set; }
    }
}
