using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Student
{
    public class InputDto_MoveStudent
    {
        public StudentDetailedQuickSearchResult Student { get; set; }
        public OutputDto_OptGroupDropdownDataSection Section { get; set; }
    }
}
