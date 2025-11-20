using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.Entity
{
    public class NSInterventionVideoDistrict : BaseEntityNoTrack
    {
        public int InterventionVideoId { get; set; }
        public int DistrictId { get; set; }

        public virtual District District { get; set; }
        public virtual NSInterventionVideo InterventionVideo { get; set; }
    }
}
