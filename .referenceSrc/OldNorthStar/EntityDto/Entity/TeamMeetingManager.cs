using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.Entity
{
    public class TeamMeetingManager
    {
        public int ID { get; set; }
        public int TeamMeetingID { get; set; }
        public int StaffID { get; set; }
        public virtual Staff Staff { get; set; }
        public virtual TeamMeeting TeamMeeting { get; set; }
    }
}
