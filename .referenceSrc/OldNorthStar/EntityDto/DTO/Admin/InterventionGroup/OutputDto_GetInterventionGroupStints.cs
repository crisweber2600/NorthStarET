using EntityDto.DTO.Admin.Simple;
using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.InterventionGroup
{
    public class OutputDto_GetInterventionGroupStints : OutputDto_Base
    {
        public List<InterventionGroupStudentDto> Stints { get; set; }
    }
}
