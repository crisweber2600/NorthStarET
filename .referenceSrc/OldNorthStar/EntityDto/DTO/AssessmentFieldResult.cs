using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.DTO
{
	public class AssessmentFieldResult : IFieldResult
    {
        public AssessmentFieldResult()
        {
            ChecklistValues = new List<int>();
        }
        public List<int> ChecklistValues { get; set; }

		public string StringValue { get; set; }
		public int? IntValue { get; set; }
		public decimal? DecimalValue { get; set; }
		public DateTime? DateValue { get; set; }
		public bool? BoolValue { get; set; }
		public string DbColumn { get; set; }
        public bool IsModified { get; set; }
        public int FieldIndex { get; set; }
        public int? GroupId { get; set; }
        public string FieldType { get; set; }
        public int FieldId { get; set; }
        public bool? FF1 { get; set; }
        public bool? FF2 { get; set; }
        public bool? FF3 { get; set; }
        public bool? FF4 { get; set; }
        public bool? FF5 { get; set; }

	}

    public interface IFieldResult
    {
        string StringValue { get; set; }
        int? IntValue { get; set; }
        decimal? DecimalValue { get; set; }
        DateTime? DateValue { get; set; }
        bool? BoolValue { get; set; }
        string DbColumn { get; set; }
    }

    public class AssessmentFieldResultDisplayOnly
    {
        public string StringValue { get; set; }
        public string DbColumn { get; set; }
        public int FieldIndex { get; set; }
    }
}
