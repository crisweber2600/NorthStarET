using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NorthStar4.PCL.Entity;

namespace NorthStar4.CrossPlatform.Entity
{
    public class InterventionFramework : BaseEntityNoTrack
    {
        public InterventionFramework()
        {
            this.InterventionTypes = new HashSet<Intervention>();
        }

        public string FrameworkName { get; set; }
        public string FreameworkDescription { get; set; }

        public virtual ICollection<Intervention> InterventionTypes { get; set; }
    }
}
