using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NorthStar4.PCL.Entity;
using NorthStar4.CrossPlatform.DTO.Reports;

namespace NorthStar4.PCL.DTO
{
	public class StudentSectionLIDReportResult
    {
		public StudentSectionLIDReportResult()
		{
			FieldResultsByTestDueDate = new List<LIDFieldResultByTDD>();
            //FieldTotalResultsByTestDueDate = new List<CAPTotalFieldResultByTDDID>();
        }

		public int StudentId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string MiddleName { get; set; }
		public List<LIDFieldResultByTDD> FieldResultsByTestDueDate { get; set; }
       // public List<CAPTotalFieldResultByTDDID> FieldTotalResultsByTestDueDate { get; set; }
        public List<LIDSummaryFieldResult> SummaryFieldResults { get; set; }
        //public CAPTotalFieldResult CurrentTotalScoreField { get; set; }
		public int? StaffId { get; set; }
		public int? ClassId { get; set; }
	}
}
