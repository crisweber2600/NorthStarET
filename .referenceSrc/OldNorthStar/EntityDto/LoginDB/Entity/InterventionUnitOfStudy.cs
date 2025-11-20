using System.Collections.Generic;
using NorthStar4.PCL.Entity;
using NorthStar4.CrossPlatform.Entity;

namespace EntityDto.LoginDB.Entity
{
    public class NSInterventionUnitOfStudy :BaseEntityNoTrack
    {
        public NSInterventionUnitOfStudy()
        {
            this.InterventionTypes = new HashSet<NSIntervention>();
        }
        public string UnitName { get; set; }
        public string UnitDescription { get; set; }

        public virtual ICollection<NSIntervention> InterventionTypes { get; set; }
    }
}