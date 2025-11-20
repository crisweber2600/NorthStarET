using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NorthStar4.PCL.Entity;
using NorthStar4.CrossPlatform.DTO.Reports;

namespace NorthStar4.PCL.DTO
{
	public class StudentSectionHRSIWReportResult
    {
		public StudentSectionHRSIWReportResult()
		{
			FieldResultsByTestDueDate = new List<HRSIWFieldResultByTDD>();
        }

		public int StudentId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string MiddleName { get; set; }
		public List<HRSIWFieldResultByTDD> FieldResultsByTestDueDate { get; set; }
        public List<HRSIWSummaryFieldResult> SummaryFieldResults { get; set; }
		public int? StaffId { get; set; }
		public int? ClassId { get; set; }
	}
}
