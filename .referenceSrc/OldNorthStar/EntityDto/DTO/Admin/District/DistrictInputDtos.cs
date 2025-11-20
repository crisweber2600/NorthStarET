using EntityDto.DTO.Admin.Student;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.District
{
    public class InputDto_StudentAttribute
    {
        public StudentAttributeTypeDto Attribute { get; set; }
    }

    public class InputDto_StudentAttributeValue
    {
        public StudentAttributeLookupValueDto AttributeValue { get; set; }
    }
}
