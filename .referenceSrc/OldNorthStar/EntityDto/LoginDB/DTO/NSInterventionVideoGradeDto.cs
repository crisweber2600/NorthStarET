using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.DTO
{
    public class NSInterventionVideoGradeDto : BaseEntityNoTrack
    {
        public int InterventionVideoId { get; set; }
        public int GradeId { get; set; }
    }
}
