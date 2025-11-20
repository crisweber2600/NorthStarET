using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.Entity
{
    public class NSInterventionToolType :BaseEntityNoTrack
    {
        public NSInterventionToolType()
        {
            this.InterventionTools = new HashSet<NSInterventionTool>();
        }

        public string Name { get; set; }

        public virtual ICollection<NSInterventionTool> InterventionTools { get; set; }
    }
}
