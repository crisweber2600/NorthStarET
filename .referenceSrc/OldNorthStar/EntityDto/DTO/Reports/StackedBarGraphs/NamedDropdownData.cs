using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Reports.StackedBarGraphs
{
    public class NamedDropdownData
    {
        public NamedDropdownData()
        {
            DropDownData = new List<OutputDto_DropdownData>();
        }

        public string Name { get; set; }
        public int AttributeTypeId { get; set; }
        public List<OutputDto_DropdownData> DropDownData { get; set; }
    }
}
