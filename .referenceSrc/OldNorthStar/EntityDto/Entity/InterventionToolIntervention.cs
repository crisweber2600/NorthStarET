using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NorthStar4.PCL.Entity;

namespace NorthStar4.CrossPlatform.Entity
{
    public class InterventionToolIntervention : BaseEntityNoTrack
    {
        public int InterventionID { get; set; }
        public int InterventionToolID { get; set; }
        public Nullable<int> SortOrder { get; set; }

        public virtual InterventionTool InterventionTool { get; set; }
        public virtual Intervention InterventionType { get; set; }
    }
}
