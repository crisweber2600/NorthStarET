using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NorthStar4.PCL.Entity;
using NorthStar4.CrossPlatform.Entity;

namespace EntityDto.LoginDB.Entity
{
    public class NSInterventionCategory : BaseEntityNoTrack
    {
        public NSInterventionCategory()
        {
            this.InterventionTypes = new HashSet<NSIntervention>();
        }

        public string CategoryName { get; set; }
        public string CategoryDescription { get; set; }
        public int? SortOrder { get; set; }

        public virtual ICollection<NSIntervention> InterventionTypes { get; set; }
    }
}
