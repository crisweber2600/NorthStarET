using EntityDto.DTO.Assessment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.Entity
{
	public class InputDto_Assessment
	{
        public InputDto_Assessment()
        {
            Fields = new List<AssessmentFieldDto>();
            FieldCategories = new List<AssessmentFieldCategoryDto>();
            FieldSubCategories = new List<AssessmentFieldSubCategoryDto>();
            FieldGroups = new List<AssessmentFieldGroupDto>();
            FieldGroupContainers = new List<AssessmentFieldGroupContainerDto>();
        }
        public bool? CanImport { get; set; }
        public int? SelectedGroup { get; set; }
        public int? SelectedPage { get; set; }
        public int? SelectedCategory { get; set; }
        public int? SelectedSubcategory { get; set; }
        public int Id { get; set; }
		public string StorageTable { get; set; }
        public bool? IsStateTest { get; set; }
		public string AssessmentName { get; set; }
		public string AssessmentDescription { get; set; }
		public string DefaultDataEntryPage { get; set; }
		public string DataEntryPages { get; set; }
		public string DefaultClassReportPage { get; set; }
		public string ClassReportPages { get; set; }
        public int? TestType { get; set; }
        public NSAssessmentBaseType? BaseType { get; set; }
        public virtual ICollection<AssessmentFieldDto> Fields { get; set; }
		public virtual ICollection<AssessmentFieldCategoryDto> FieldCategories { get; set; }
		public virtual ICollection<AssessmentFieldSubCategoryDto> FieldSubCategories { get; set; }
		public virtual ICollection<AssessmentFieldGroupDto> FieldGroups { get; set; }
        public virtual ICollection<AssessmentFieldGroupContainerDto> FieldGroupContainers { get; set; }
    }
}
