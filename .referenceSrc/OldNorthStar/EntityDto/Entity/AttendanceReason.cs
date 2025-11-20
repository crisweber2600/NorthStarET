using NorthStar4.PCL.Entity;

namespace NorthStar4.CrossPlatform.Entity
{
    public class AttendanceReason : BaseEntity
    {
        public string Reason { get; set; }
        public bool ValidForScheduledDays { get; set; }
        public bool CountsAsAbsense { get; set; }
        public bool ValidForNonScheduledDays { get; set; }
        public bool CountsAsBonusDay { get; set; }
        public int Order { get; set; }
    }
}