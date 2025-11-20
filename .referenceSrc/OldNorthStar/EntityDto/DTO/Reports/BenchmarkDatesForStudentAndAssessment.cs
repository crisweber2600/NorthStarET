using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NorthStar4.CrossPlatform.DTO.Reports
{
    public class BenchmarkDatesForStudentAndAssessment
    {
        public int GradeID { get; set; }
        public int GradeOrder { get; set; }
        public string GradeShortName { get; set; }
        public int TestLevelPeriodID { get; set; }
        public int? TestDueDateID { get; set; }
        public DateTime? DueDate { get; set; }
        public int Year { get; set; }
        public decimal? Meets { get; set; }
        public decimal? DoesNotMeet { get; set; }
        public decimal? Exceeds { get; set; }
        public decimal? Approaches { get; set; }
        public int? SectionID { get; set; }
        public string Hex { get; set; }
        public int TestNumber { get; set; }
    }
}
