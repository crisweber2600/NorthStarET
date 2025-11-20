using EntityDto.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.Entity
{
	public class AssessmentField : BaseEntity, ICloneable
	{
        public AssessmentField()
        {
            StaffObservationSummaryAssessmentFields = new List<StaffObservationSummaryAssessmentField>();
            StaffAssessmentFieldVisibilities = new List<StaffAssessmentFieldVisibility>();
        }

        public object Clone()
        {
            var p = new AssessmentField();
            p.StorageTable = this.StorageTable;
            p.DisplayLabel = this.DisplayLabel;
            p.AltDisplayLabel = this.AltDisplayLabel;
            p.FieldType = this.FieldType;
            p.DefaultValue = this.DefaultValue;
            p.IsRequired = this.IsRequired;
            p.Page = this.Page;
            p.FieldOrder = this.FieldOrder;
            p.AltOrder = this.AltOrder;
            p.Description = this.Description;
            p.LookupFieldName = this.LookupFieldName;
            p.RangeHigh = this.RangeHigh;
            p.RangeLow = this.RangeLow;
            p.DatabaseColumn = this.DatabaseColumn;
            p.UniqueImportColumnName = this.UniqueImportColumnName;
            p.CalculationFunction = this.CalculationFunction;
            p.CalculationFields = this.CalculationFields;
            p.DisplayInEditResultList = this.DisplayInEditResultList;
            p.DisplayInLineGraphSummaryTable = this.DisplayInLineGraphSummaryTable;
            p.DisplayInLineGraphs = this.DisplayInLineGraphs;
            p.DisplayInObsSummary = this.DisplayInObsSummary;
            p.OutOfHowMany = this.OutOfHowMany;
            p.ImportColumnName = this.ImportColumnName;
            p.DisplayInStackedBarGraphSummary = this.DisplayInStackedBarGraphSummary;
            p.ObsSummaryLabel = this.ObsSummaryLabel;
            p.Flag1 = this.Flag1;
            p.Flag2 = this.Flag2;
            p.Flag3 = this.Flag3;
            p.Flag4 = this.Flag4;
            p.Flag5 = this.Flag5;

            return p;
        }

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
        public int? AltOrder { get; set; }
		public string Description { get; set; }
		public string LookupFieldName { get; set; }
		public int RangeHigh { get; set; }
		public int RangeLow { get; set; }
		public string DatabaseColumn { get; set; }
        public string UniqueImportColumnName { get; set; }
        public string CalculationFunction { get; set; }
		public string CalculationFields { get; set; }
		public int AssessmentId { get; set; }
        public bool? DisplayInObsSummary { get; set; }
        public bool? DisplayInEditResultList { get; set; }
        public bool? DisplayInLineGraphSummaryTable { get; set; }
        public bool? DisplayInLineGraphs { get; set; }
        public int? OutOfHowMany { get; set; }
        public int? PreviousId { get; set; }
        public bool? IsPrimaryFieldForAssessment { get; set; }
        public string ImportColumnName { get; set; }

        public bool? Flag1 { get; set; }
        public bool? Flag2 { get; set; }
        public bool? Flag3 { get; set; }
        public bool? Flag4 { get; set; }
        public bool? Flag5 { get; set; }
        public Assessment Assessment { get; set; }
        public AssessmentFieldCategory Category { get; set; }
		public AssessmentFieldGroup Group { get; set; }
		public AssessmentFieldSubCategory SubCategory { get; set; }
        public virtual ICollection<StaffObservationSummaryAssessmentField> StaffObservationSummaryAssessmentFields { get; set; }
        public virtual ICollection<StaffAssessmentFieldVisibility> StaffAssessmentFieldVisibilities { get; set; }
    }
}
