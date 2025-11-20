using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Reports.HFW
{
    public class InputDto_GetHFWDetailReport
    {
        public int StudentId { get; set; }
        public int StudentResultId { get; set; } // may not need
        public string HfwSortOrder { get; set; }
        public List<OutputDto_DropdownData> SelectedRanges { get; set; }
    }
}
