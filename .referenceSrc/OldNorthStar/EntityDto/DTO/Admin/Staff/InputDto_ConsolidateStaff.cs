using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Staff
{
    public class InputDto_ConsolidateStaff
    {
        public StaffQuickSearchResult PrimaryStaffId { get; set; }
        public StaffQuickSearchResult SecondaryStaffId { get; set; }
    }
}
