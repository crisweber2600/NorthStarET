using System.Collections.Generic;
using NorthStar4.PCL.Entity;
using NorthStar4.CrossPlatform.Entity;

namespace EntityDto.LoginDB.Entity
{
    public class NSInterventionTier : BaseEntityNoTrack
    {
        public NSInterventionTier()
        {
            this.InterventionTypes = new HashSet<NSIntervention>();
        }

        public int TierValue { get; set; }
        public string Description { get; set; }
        public string TierName { get; set; }
        public string TierLabel { get; set; }
        public string TierColor { get; set; }

        public virtual ICollection<NSIntervention> InterventionTypes { get; set; }
    }
}