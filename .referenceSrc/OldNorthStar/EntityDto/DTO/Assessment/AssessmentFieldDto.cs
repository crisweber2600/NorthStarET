using NorthStar4.CrossPlatform.Entity;
using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.Entity
{
	public class AssessmentFieldDto : BaseEntityNoTrack
    {
        public AssessmentFieldResult StudentFieldResult { get; set; }
        public string StorageTable { get; set; }
        public string DisplayLabel { get; set; }
        public string ObsSummaryLabel { get; set; }
        public bool? DisplayInStackedBarGraphSummary { get; set; }
        public string AltDisplayLabel { get; set; }
		public string FieldType { get; set; }
		public string DefaultValue { get; set; }
		public bool IsRequired { get; set; }
		public int? CategoryId { get; set; }
		public int? SubcategoryId { get; set; }
		public int? GroupId { get; set; }
		public int Page { get; set; }
		public int FieldOrder { get; set; }
		public string Description { get; set; }
		public string LookupFieldName { get; set; }
		public int RangeHigh { get; set; }
		public int RangeLow { get; set; }
		public string DatabaseColumn { get; set; }
        public string UniqueImportColumnName { get; set; }
        public string CalculationFunction { get; set; }
		public string CalculationFields { get; set; }
		public int AssessmentId { get; set; }
        public int? PreviousId { get; set; }
        public bool? IsPrimaryFieldForAssessment { get; set; }
        public bool? DisplayInObsSummary { get; set; }
        public bool? DisplayInEditResultList { get; set; }
        public bool? DisplayInLineGraphSummaryTable { get; set; }
        public bool? DisplayInLineGraphs { get; set; }
        public bool IsFlaggedForDelete { get; set; }
        public int? OutOfHowMany { get; set; }
        public string ImportColumnName { get; set; }

        public string AssessmentName { get; set; }
        public string FieldName { get; set; }

        public bool? Flag1 { get; set; }
        public bool? Flag2 { get; set; }
        public bool? Flag3 { get; set; }
        public bool? Flag4 { get; set; }
        public bool? Flag5 { get; set; }

        //public Assessment ParentAssessment { get; set; }
        //public AssessmentFieldCategory Category { get; set; }
        //public AssessmentFieldGroup Group { get; set; }
        //public AssessmentFieldSubCategory SubCategory { get; set; }
    }
}
