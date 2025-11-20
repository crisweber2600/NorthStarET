using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NorthStar4.PCL.Entity;
using NorthStar4.CrossPlatform.Entity;

namespace EntityDto.LoginDB.Entity
{
    public class NSInterventionWorkshop : BaseEntityNoTrack
    {
        public NSInterventionWorkshop()
        {
            this.InterventionTypes = new HashSet<NSIntervention>();
        }

        public string WorkshopName { get; set; }
        public string WorkshopDescription { get; set; }

        public virtual ICollection<NSIntervention> InterventionTypes { get; set; }
    }
}
