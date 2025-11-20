using EntityDto.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.Entity
{
    public enum NSAssessmentBaseType
    {
        LetterID = 1,
        OhioWordTest = 2,
        ConceptsAboutPrint = 3,
        WritingVocabulary = 4,
        HearingAndRecording = 5,
        DRA = 6,
        HighFrequencyWords = 7,
        FPTextLevel = 8,
        SpellingInventoryV3P = 9,
        SpellingInventoryV3E = 10,
        SpellingInventoryV3I = 11,
        SpellingInventoryV4P = 12,
        SpellingInventoryV4E = 13,
        SpellingInventoryV4I = 14,
        ReadingRecovery = 15,
        HighFrequencyWordsReading = 16,
        HighFrequencyWordsWriting = 17,
        Other = 100
    }

    public class Assessment : BaseEntity, ICloneable
	{
        public Assessment()
        {
            Fields = new List<AssessmentField>();
            FieldCategories = new List<AssessmentFieldCategory>();
            FieldSubCategories = new List<AssessmentFieldSubCategory>();
            FieldGroups = new List<AssessmentFieldGroup>();
            AssessmentBenchmarks = new List<Assessment_Benchmarks>();
            StaffObservationSummaryAssessments = new List<StaffObservationSummaryAssessment>();
            StaffObservationSummaryAssessmentFields = new List<StaffObservationSummaryAssessmentField>();
            FieldGroupContainers = new List<AssessmentFieldGroupContainer>();
        }

        public object Clone()
        {
            var p = new Assessment();
            p.StorageTable = this.StorageTable;
            p.SecondaryStorageTable = this.SecondaryStorageTable;
            p.TertiaryStorageTable = this.TertiaryStorageTable;
            p.IsStateTest = this.IsStateTest;
            p.IsHFW = this.IsHFW;
            p.IsProgressMonitoring = this.IsProgressMonitoring;
            p.AssessmentName = this.AssessmentName;
            p.AssessmentDescription = this.AssessmentDescription;
            p.DefaultDataEntryPage = this.DefaultDataEntryPage;
            p.DataEntryPages = this.DataEntryPages;
            p.DefaultClassReportPage = this.DefaultClassReportPage;
            p.ClassReportPages = this.ClassReportPages;
            p.TestType = this.TestType;
            p.AssessmentIsAvailable = this.AssessmentIsAvailable;
            p.CanImport = this.CanImport;
            p.SortOrder = this.SortOrder;
            p.BaseType = this.BaseType;

            return p;
        }

        public bool? CanImport { get; set; }
        public bool? AssessmentIsAvailable { get; set; }
        public string StorageTable { get; set; }
        public string SecondaryStorageTable { get; set; }
        public string TertiaryStorageTable { get; set; }
        public bool? IsStateTest { get; set; }
        public bool? IsHFW { get; set; }
        public bool? IsProgressMonitoring { get; set; }
		public string AssessmentName { get; set; }
		public string AssessmentDescription { get; set; }
		public string DefaultDataEntryPage { get; set; }
		public string DataEntryPages { get; set; }
		public string DefaultClassReportPage { get; set; }
		public string ClassReportPages { get; set; }
        public int? TestType { get; set; }
        public int? SortOrder { get; set; }
        public NSAssessmentBaseType? BaseType { get; set; }
        public virtual ICollection<AssessmentField> Fields { get; set; }
		public virtual ICollection<AssessmentFieldCategory> FieldCategories { get; set; }
		public virtual ICollection<AssessmentFieldSubCategory> FieldSubCategories { get; set; }
		public virtual ICollection<AssessmentFieldGroup> FieldGroups { get; set; }
        public virtual ICollection<AssessmentFieldGroupContainer> FieldGroupContainers { get; set; }
        public virtual ICollection<Assessment_Benchmarks> AssessmentBenchmarks { get; set; }
        public virtual ICollection<StaffObservationSummaryAssessment> StaffObservationSummaryAssessments { get; set; }
        public virtual ICollection<StaffObservationSummaryAssessmentField> StaffObservationSummaryAssessmentFields { get; set; }
    }
}
