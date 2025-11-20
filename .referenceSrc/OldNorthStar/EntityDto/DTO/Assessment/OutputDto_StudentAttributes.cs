using EntityDto.DTO.Admin.InterventionToolkit;
using EntityDto.DTO.Admin.Simple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Assessment
{
    public class OutputDto_StudentAttributes : OutputDto_Base
    {
        public OutputDto_StudentAttributes()
        {
            Attributes = new List<StudentAttributeVisibilityDto>();
        }
        public List<StudentAttributeVisibilityDto> Attributes { get; set; }
    }

    public class StudentAttributeVisibilityDto : DtoBase
    {
        public string Name { get; set; }
        public bool Visible { get; set; }
    }
}
