using System.Collections.Generic;
using NorthStar4.PCL.Entity;

namespace NorthStar4.CrossPlatform.Entity
{
    public class InterventionTier : BaseEntityNoTrack
    {
        public InterventionTier()
        {
            //this.InterventionTypes = new HashSet<Intervention>();
        }

        public int TierValue { get; set; }
        public string Description { get; set; }
        public string TierName { get; set; }
        public string TierLabel { get; set; }
        public string TierColor { get; set; }

        //public virtual ICollection<Intervention> InterventionTypes { get; set; }
    }
}