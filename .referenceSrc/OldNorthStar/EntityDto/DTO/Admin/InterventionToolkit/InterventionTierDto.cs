using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.InterventionToolkit
{
    public class InterventionTierDto : BaseEntityNoTrack
    {

        public int TierValue { get; set; }
        public string Description { get; set; }
        public string TierName { get; set; }
        public string TierLabel { get; set; }
        public string TierColor { get; set; }
        public int NumInterventions { get; set; }
    }
}
