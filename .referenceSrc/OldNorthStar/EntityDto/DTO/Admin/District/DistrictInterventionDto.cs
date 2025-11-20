using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.District
{
    public class DistrictInterventionDto : BaseEntity
    {
        public DistrictInterventionDto()
        {
        }

        public string InterventionType { get; set; }
        public bool bDisplay { get; set; }
        public string Description { get; set; }
      
    }
}
