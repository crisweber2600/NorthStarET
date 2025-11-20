using System;

namespace NorthStar4.CrossPlatform.DTO.Admin.InterventionGroup
{
    public class InputDto_SaveSingleAttendance
    {
        public int StartEndDateId { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public DateTime Date { get; set; } 
        public int SectionId { get; set; }
        public int StudentId { get; set; }
    }
}