using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NorthStar4.PCL.Entity;

namespace NorthStar4.PCL.DTO
{
	public class AssessmentHFWStudentResult
    {
		public AssessmentHFWStudentResult()
		{
            TotalFieldResults = new List<AssessmentFieldResult>();
            ReadFieldResults = new List<AssessmentFieldResult>();
            WriteFieldResults = new List<AssessmentFieldResult>();
        }

		public int StudentId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string MiddleName { get; set; }
        public string FPText { get; set; }
        public int? FPValueID { get; set; }
		public int? ResultId { get; set; }
		public List<AssessmentFieldResult> TotalFieldResults { get; set; }
        public List<AssessmentFieldResult> ReadFieldResults { get; set; }
        public List<AssessmentFieldResult> WriteFieldResults { get; set; }
        public int? TestDueDateId { get; set; }
		public DateTime? TestDate { get; set; }
		public int? StaffId { get; set; }
		public int? ClassId { get; set; }
	}
}
