using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NorthStar4.PCL.Entity;

namespace NorthStar4.PCL.DTO
{
	public class ObservationSummaryFieldScore : IFieldResult
    {
		public int AssessmentId { get; set; }
        public bool IsCopied { get; set; }
		public int FieldOrder { get; set; }
		public string DbColumn { get; set; }
		public int? IntValue { get; set; }
		public decimal? DecimalValue { get; set; }
		public string StringValue { get; set; }
		public string ColumnType { get; set; }
		public string LookupFieldName { get; set; }
        public string StateGradeId { get; set; }
        public int TestTypeId { get; set; }
        public bool? BoolValue { get; set; }
        public DateTime? DateValue { get; set; }
        public int? ResultGradeId { get; set; }
    }
}
