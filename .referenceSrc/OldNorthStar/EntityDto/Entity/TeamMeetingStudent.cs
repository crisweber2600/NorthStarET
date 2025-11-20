using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.Entity
{
    public class TeamMeetingStudent
    {
        public int ID { get; set; }

        public int TeamMeetingID { get; set; }

        public int StaffID { get; set; }

        public int SectionID { get; set; }

        public int StudentID { get; set; }

        public string Notes { get; set; }

        public virtual Section Section { get; set; }

        public virtual Staff Staff { get; set; }

        public virtual Student Student { get; set; }

        public virtual TeamMeeting TeamMeeting { get; set; }
    }
}
