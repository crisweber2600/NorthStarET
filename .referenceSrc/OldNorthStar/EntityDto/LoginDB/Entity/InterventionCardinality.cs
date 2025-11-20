using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NorthStar4.PCL.Entity;
using NorthStar4.CrossPlatform.Entity;

namespace EntityDto.LoginDB.Entity
{
    public class NSInterventionCardinality : BaseEntityNoTrack
    {
        public NSInterventionCardinality()
        {
            this.InterventionTypes = new HashSet<NSIntervention>();
        }

        public string CardinalityName { get; set; }

        public virtual ICollection<NSIntervention> InterventionTypes { get; set; }
    }
}
