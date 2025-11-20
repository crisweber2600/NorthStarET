using EntityDto.DTO.Admin.Simple;
using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Student
{
    public class OutputDto_StaffQuickSearch : OutputDto_Base
    {
        public List<StaffQuickSearchResult> Results { get; set; }
    }
}
