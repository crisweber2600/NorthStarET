using NorthStar4.PCL.Entity;

namespace NorthStar4.CrossPlatform.Entity
{
    public class SchoolCalendar : BaseEntity
    {
        public int SchoolID { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public System.DateTime Start { get; set; }
        public System.DateTime End { get; set; }

        public virtual School School { get; set; }
    }
}