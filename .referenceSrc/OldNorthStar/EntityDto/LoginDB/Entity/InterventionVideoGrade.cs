using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.Entity
{
    public class InterventionVideoGrade : BaseEntityNoTrack
    {
        public int InterventionVideoId { get; set; }
        public int GradeId { get; set; }

        public virtual NSGrade Grade { get; set; }
        public virtual NSInterventionVideo InterventionVideo { get; set; }
    }
}
