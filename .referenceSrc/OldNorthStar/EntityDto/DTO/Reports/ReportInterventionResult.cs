using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NorthStar4.CrossPlatform.DTO.Reports
{
    public class ReportInterventionResult
    {
        public int Id { get; set; }
        public int SchoolYear { get; set; }
        public int Tier { get; set; }
        public int InterventionGroupId { get; set; }
        public int InterventionistId { get; set; }
        public string InterventionType { get; set; }
        public int NumLessons { get; set; }
        public DateTime StartOfIntervention { get; set; }
        public string StartOfInterventionString { get; set; }
        public DateTime? EndOfIntervention { get; set; }
        public string EndOfInterventionString { get; set; }
        public string StaffInitials { get; set; }
        public string Description { get; set; }
        public int SchoolId { get; set; }
        public int StudentID { get; set; }
        public int? StartTDDID { get; set; }
        public int? EndTDDID { get; set; }

    }
}
