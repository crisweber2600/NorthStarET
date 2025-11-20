using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NorthStar4.PCL.Entity;
using NorthStar4.CrossPlatform.DTO.Reports;

namespace NorthStar4.PCL.DTO
{
	public class StudentSectionCAPReportResult
    {
		public StudentSectionCAPReportResult()
		{
			FieldResultsByTestDueDate = new List<CAPFieldResultByTDD>();
            //FieldTotalResultsByTestDueDate = new List<CAPTotalFieldResultByTDDID>();
        }

		public int StudentId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string MiddleName { get; set; }
		public List<CAPFieldResultByTDD> FieldResultsByTestDueDate { get; set; }
       // public List<CAPTotalFieldResultByTDDID> FieldTotalResultsByTestDueDate { get; set; }
        public List<CAPSummaryFieldResult> SummaryFieldResults { get; set; }
        //public CAPTotalFieldResult CurrentTotalScoreField { get; set; }
		public int? StaffId { get; set; }
		public int? ClassId { get; set; }
	}

    public class StudentSectionAVMRSingleDateReportResult
    {
        public StudentSectionAVMRSingleDateReportResult()
        {
            FieldResults = new List<CAPFieldResult>();
            //FieldTotalResultsByTestDueDate = new List<CAPTotalFieldResultByTDDID>();
        }

        public int StudentId { get; set; }
        public string FullName { get; set; }
        public List<CAPFieldResult> FieldResults { get; set; }
        // public List<CAPTotalFieldResultByTDDID> FieldTotalResultsByTestDueDate { get; set; }
        public List<CAPSummaryFieldResult> SummaryFieldResults { get; set; }
        //public CAPTotalFieldResult CurrentTotalScoreField { get; set; }
        public int? StaffId { get; set; }
        public int? ClassId { get; set; }
    }

    //public class StudentSectionAVMRSingleDateDetailReportResult
    //{
    //    public StudentSectionAVMRSingleDateDetailReportResult()
    //    {
    //        FieldResults = new List<CAPFieldResult>();
    //        //FieldTotalResultsByTestDueDate = new List<CAPTotalFieldResultByTDDID>();
    //    }

    //    public int StudentId { get; set; }
    //    public string FullName { get; set; }
    //    public List<CAPFieldResult> FieldResults { get; set; }
    //    // public List<CAPTotalFieldResultByTDDID> FieldTotalResultsByTestDueDate { get; set; }

    //    public int? StaffId { get; set; }
    //    public int? ClassId { get; set; }
    //}
}
