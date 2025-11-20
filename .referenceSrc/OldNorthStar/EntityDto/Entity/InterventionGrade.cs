using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NorthStar4.PCL.Entity;

namespace NorthStar4.CrossPlatform.Entity
{
    public class InterventionGrade : BaseEntityNoTrack
    {
        public int InterventionID { get; set; }
        public int GradeID { get; set; }

        public virtual Grade Grade { get; set; }
        public virtual Intervention InterventionType { get; set; }
    }
}
