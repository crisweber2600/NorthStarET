using System;

namespace NorthStar4.CrossPlatform.DTO.Admin.InterventionGroup
{
    public class InputDto_ApplyStatusNotes
    {
        public int StaffId { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public DateTime Date { get; set; }
        public int SectionId { get; set; }
        public int SchoolStartYear { get; set; }
    }
}