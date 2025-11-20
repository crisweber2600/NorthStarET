using NorthStar4.CrossPlatform.Entity;
using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.Entity
{
    public class AttendeeGroupStaff : BaseEntityNoTrack
    {
        public int StaffId { get; set; }
        public int AttendeeGroupId { get; set; }

        public virtual Staff Staff { get; set; }

        public virtual AttendeeGroup AttendeeGroup { get; set; }
    }
}
