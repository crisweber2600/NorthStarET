using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NorthStar4.PCL.Entity;

namespace NorthStar4.CrossPlatform.Entity
{
    public class InterventionCardinality : BaseEntityNoTrack
    {
        public InterventionCardinality()
        {
            this.InterventionTypes = new HashSet<Intervention>();
        }

        public string CardinalityName { get; set; }

        public virtual ICollection<Intervention> InterventionTypes { get; set; }
    }
}
