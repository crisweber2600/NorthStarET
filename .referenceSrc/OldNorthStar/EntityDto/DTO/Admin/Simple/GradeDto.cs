using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Simple
{
    public class GradeDto : BaseEntityNoTrack
    {
        public string ShortName { get; set; }
        public string LongName { get; set; }
        public int GradeOrder { get; set; }
        public string StateGradeNumber { get; set; }
    }
}
