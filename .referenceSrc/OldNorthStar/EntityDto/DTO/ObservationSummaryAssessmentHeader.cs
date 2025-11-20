using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.DTO
{
	public class ObservationSummaryAssessmentHeader
	{
		public int FieldOrder { get; set; }
		public string FieldName { get; set; }
		public string AssessmentName { get; set; }
        public int AssessmentId { get; set; }
		public string LookupFieldName { get; set; }
		public string FieldType { get; set; }
		public string DatabaseColumn { get; set; }
        public int Id { get; set; }
        public string CalculationFunction { get; set; }
        public string CalculationFields { get; set; }
	}
}
