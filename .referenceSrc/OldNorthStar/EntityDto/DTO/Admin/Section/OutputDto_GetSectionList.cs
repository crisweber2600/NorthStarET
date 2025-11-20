using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Section
{
    public class OutputDto_GetSectionList
    {
        public OutputDto_GetSectionList()
        {
        }
        public List<SectionListDto> Sections { get; set; }
    }
}
