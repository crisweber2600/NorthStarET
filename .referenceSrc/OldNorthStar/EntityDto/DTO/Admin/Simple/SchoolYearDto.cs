using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Simple
{
    public class SchoolYearDto
    {
        public int SchoolStartYear { get; set; }
        public string YearVerbose { get; set; }
        public int SchoolEndYear { get; set; }
    }
}
