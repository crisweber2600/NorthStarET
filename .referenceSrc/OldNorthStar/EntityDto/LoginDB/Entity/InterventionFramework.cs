using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NorthStar4.PCL.Entity;
using NorthStar4.CrossPlatform.Entity;

namespace EntityDto.LoginDB.Entity
{
    public class NSInterventionFramework : BaseEntityNoTrack
    {
        public NSInterventionFramework()
        {
            this.InterventionTypes = new HashSet<NSIntervention>();
        }

        public string FrameworkName { get; set; }
        public string FreameworkDescription { get; set; }

        public virtual ICollection<NSIntervention> InterventionTypes { get; set; }
    }
}
