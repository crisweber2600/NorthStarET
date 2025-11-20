using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Student
{
    public class InputDto_StudentQuickSearch
    {
        public string SearchString { get; set; }
        public bool SearchPreviousYears { get; set; }
        public int SchoolYear { get; set; }
    }
}
