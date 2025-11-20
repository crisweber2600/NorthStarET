using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NorthStar4.CrossPlatform.Entity
{
    public class InterventionToolType :BaseEntityNoTrack
    {
        public InterventionToolType()
        {
            this.InterventionTools = new HashSet<InterventionTool>();
        }

        public string Name { get; set; }

        public virtual ICollection<InterventionTool> InterventionTools { get; set; }
    }
}
