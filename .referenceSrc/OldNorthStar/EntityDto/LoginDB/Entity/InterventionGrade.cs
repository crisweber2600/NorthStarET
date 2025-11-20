using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NorthStar4.PCL.Entity;
using NorthStar4.CrossPlatform.Entity;

namespace EntityDto.LoginDB.Entity
{
    public class NSInterventionGrade : BaseEntityNoTrack
    {
        public int InterventionTypeId { get; set; }
        public int GradeID { get; set; }

        public virtual NSGrade Grade { get; set; }
        public virtual NSIntervention InterventionType { get; set; }
    }
}
