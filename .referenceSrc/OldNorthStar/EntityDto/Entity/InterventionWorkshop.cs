using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NorthStar4.PCL.Entity;

namespace NorthStar4.CrossPlatform.Entity
{
    public class InterventionWorkshop : BaseEntityNoTrack
    {
        public InterventionWorkshop()
        {
            this.InterventionTypes = new HashSet<Intervention>();
        }

        public string WorkshopName { get; set; }
        public string WorkshopDescription { get; set; }

        public virtual ICollection<Intervention> InterventionTypes { get; set; }
    }
}
