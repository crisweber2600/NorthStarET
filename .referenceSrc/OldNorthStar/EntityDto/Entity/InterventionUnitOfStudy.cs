using System.Collections.Generic;
using NorthStar4.PCL.Entity;

namespace NorthStar4.CrossPlatform.Entity
{
    public class InterventionUnitOfStudy :BaseEntityNoTrack
    {
        public InterventionUnitOfStudy()
        {
            this.InterventionTypes = new HashSet<Intervention>();
        }
        public string UnitName { get; set; }
        public string UnitDescription { get; set; }

        public virtual ICollection<Intervention> InterventionTypes { get; set; }
    }
}