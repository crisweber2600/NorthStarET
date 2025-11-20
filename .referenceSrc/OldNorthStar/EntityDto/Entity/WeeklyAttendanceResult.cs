using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NorthStar4.CrossPlatform.Entity
{
    public class WeeklyAttendanceResult
    {
        public string RecordKey { get; set; }
        public string YearVerbose { get; set; }
        public int SectionID { get; set; }
        public string SectionName { get; set; }
        public int StartEndDateID { get; set; }
        public string InterventionStart { get; set; }
        public string InterventionEnd { get; set; }
        public int StudentID { get; set; }
        public int? SchoolID { get; set; }
        public string Student { get; set; }
        public string StudentNumber { get; set; }
        public string MondayStatus { get; set; }
        public string MondayNotes { get; set; }
        public string TuesdayStatus { get; set; }
        public string TuesdayNotes { get; set; }
        public string WednesdayStatus { get; set; }
        public string WednesdayNotes { get; set; }
        public string ThursdayStatus { get; set; }
        public string ThursdayNotes { get; set; }
        public string FridayStatus { get; set; }
        public string FridayNotes { get; set; }
        public bool MondayCanEdit { get; set; }
        public bool TuesdayCanEdit { get; set; }
        public bool WednesdayCanEdit { get; set; }
        public bool ThursdayCanEdit { get; set; }
        public bool FridayCanEdit { get; set; }
    }
}
