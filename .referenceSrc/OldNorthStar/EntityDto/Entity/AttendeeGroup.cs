using NorthStar4.CrossPlatform.Entity;
using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.Entity
{
    public class AttendeeGroup : BaseEntityNoTrack
    {

        public AttendeeGroup()
        {
            AttendeeGroupStaffs = new List<AttendeeGroupStaff>();
        }
        public string GroupName { get; set; }
        public int StaffId { get; set; }

        public List<AttendeeGroupStaff> AttendeeGroupStaffs { get; set; }
        public virtual Staff Staff { get; set; }
    }
}
