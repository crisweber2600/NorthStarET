using EntityDto.DTO.Admin.Simple;
using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.District
{
    public class OutputDto_HFWList : OutputDto_Base
    {
        public List<AssessmentFieldGroupDto> Words { get; set; }
    }
}
