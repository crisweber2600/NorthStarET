using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NorthStar4.PCL.Entity;

namespace NorthStar4.PCL.DTO
{
	public class ObservationSummaryStudentResult
	{
		public ObservationSummaryStudentResult()
		{
			//OSFieldResults = new List<ObservationSummaryFieldScore>();
		}

		public int StudentId { get; set; }
		public string StudentName { get; set; }
        public string StudentIdentifier { get; set; }
        public string SchoolName { get; set; }
        public string GradeName { get; set; }
        public string DelimitedTeacherSections { get; set; }
        public string DelimitedTeachers { get; set; }
        public int? ResultId { get; set; }
		public List<ObservationSummaryFieldScore> OSFieldResults { get; set; }
		public int? TestDueDateId { get; set; }
		public DateTime? TestDate { get; set; }
		public int? StaffId { get; set; }
		public int? ClassId { get; set; }
        public int GradeId { get; set; }
        public int GradeOrder { get; set; }
        public int? NoteCount { get; set; }
        public int? TestLevelPeriodId { get; set; }

        // intervention data-specific
        public string Interventionist { get; set; }
        public string InterventionGroupName { get; set; }
        public string SPEDLables { get; set; }
        public string StudentServices { get; set; }
        public string ELL { get; set; }
        public string FPScore { get; set; }
        public DateTime? BenchmarkDate { get; set; }
        public DateTime? DateTestTaken { get; set; }

        public string SpecialED { get; set; }
        public string Services { get; set; }
        public string Att1 { get; set; }
        public string Att2 { get; set; }
        public string Att3 { get; set; }
        public string Att4 { get; set; }
        public string Att5 { get; set; }
        public string Att6 { get; set; }
        public string Att7 { get; set; }
        public string Att8 { get; set; }
        public string Att9 { get; set; }
    }
}
