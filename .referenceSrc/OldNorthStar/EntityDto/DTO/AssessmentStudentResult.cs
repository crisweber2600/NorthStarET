using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NorthStar4.PCL.Entity;

namespace NorthStar4.PCL.DTO
{
	public class AssessmentStudentResult
	{
		public AssessmentStudentResult()
		{
			FieldResults = new List<AssessmentFieldResult>();
            Recorder = new OutputDto_DropdownData();
		}
        public string ImageUrl { get; set; }
        public string Pronunciation { get; set; }
		public int StudentId { get; set; }
        public string StudentIdentifier { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string MiddleName { get; set; }
        public string StudentName { get; set; }
        public bool IsCopied { get; set; }
        public string FPText { get; set; }
        public int? FPValueID { get; set; }
        public int? GuidedReadingGroupId { get; set; }
        public int? ResultId { get; set; }
		public List<AssessmentFieldResult> FieldResults { get; set; }
		public int? TestDueDateId { get; set; }
		public DateTime? TestDate { get; set; }
        public string TestDateDisplay { get
            {
                return TestDate?.ToString("dd-MMM-yyy") ?? string.Empty;
            }
            set { } }
		public int? StaffId { get; set; }
		public int? ClassId { get; set; }
        public int? GradeId { get; set; }
        public OutputDto_DropdownData Recorder { get; set; }
	}
}
