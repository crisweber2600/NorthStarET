using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.InterventionGroup
{
    public class InputDto_GetInterventionGroups
    {
        public int SchoolYear { get; set; }
        public int SchoolId { get; set; }
        public int StaffId { get; set; }
    }
}
