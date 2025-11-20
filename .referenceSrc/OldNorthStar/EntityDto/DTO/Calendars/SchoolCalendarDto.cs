using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Calendars
{
    public class SchoolCalendarDto : BaseEntity
    {
        public int SchoolID { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public System.DateTime Start { get; set; }
        public System.DateTime End { get; set; }

    }
}
