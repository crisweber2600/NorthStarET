using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NorthStar4.PCL.Entity;

namespace NorthStar4.CrossPlatform.Entity
{
    public class InterventionCategory : BaseEntityNoTrack
    {
        public InterventionCategory()
        {
            this.InterventionTypes = new HashSet<Intervention>();
        }

        public string CategoryName { get; set; }
        public string CategoryDescription { get; set; }
        public Nullable<int> SortOrder { get; set; }

        public virtual ICollection<Intervention> InterventionTypes { get; set; }
    }
}
