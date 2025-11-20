using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NorthStar4.PCL.Entity;
using NorthStar4.CrossPlatform.DTO.Reports;

namespace NorthStar4.PCL.DTO
{
	public class StudentSectionWVReportResult
    {
		public StudentSectionWVReportResult()
		{
			FieldResultsByTestDueDate = new List<WVFieldResultByTDD>();
            //FieldTotalResultsByTestDueDate = new List<CAPTotalFieldResultByTDDID>();
        }

		public int StudentId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string MiddleName { get; set; }
		public List<WVFieldResultByTDD> FieldResultsByTestDueDate { get; set; }
       // public List<CAPTotalFieldResultByTDDID> FieldTotalResultsByTestDueDate { get; set; }
        public List<WVSummaryFieldResult> SummaryFieldResults { get; set; }
        //public CAPTotalFieldResult CurrentTotalScoreField { get; set; }
		public int? StaffId { get; set; }
		public int? ClassId { get; set; }
	}
}
