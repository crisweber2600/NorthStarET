using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.Entity
{
    public class NSInterventionVideoNSIntervention : BaseEntity
    {
        public int InterventionTypeId { get; set; }
        public int InterventionVideoId { get; set; }
        public int? SortOrder { get; set; }

        public virtual NSIntervention InterventionType { get; set; }
        public virtual NSInterventionVideo InterventionVideo { get; set; }
    }
}
