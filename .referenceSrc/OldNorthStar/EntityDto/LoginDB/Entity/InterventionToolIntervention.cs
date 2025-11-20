using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NorthStar4.PCL.Entity;
using NorthStar4.CrossPlatform.Entity;

namespace EntityDto.LoginDB.Entity
{
    public class NSInterventionToolIntervention : BaseEntityNoTrack
    {
        public int InterventionTypeId { get; set; }
        public int InterventionToolId { get; set; }
        public Nullable<int> SortOrder { get; set; }

        public virtual NSInterventionTool InterventionTool { get; set; }
        public virtual NSIntervention InterventionType { get; set; }
    }
}
