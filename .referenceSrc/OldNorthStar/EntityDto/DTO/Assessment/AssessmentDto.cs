using EntityDto.DTO.Admin.Simple;
using NorthStar4.CrossPlatform.Entity;
using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Assessment
{

    public class AssessmentDto : BaseEntityNoTrack
    {
        public AssessmentDto()
        {
            Fields = new List<AssessmentFieldDto>();
            FieldCategories = new List<AssessmentFieldCategoryDto>();
            FieldSubCategories = new List<AssessmentFieldSubCategoryDto>();
            FieldGroups = new List<AssessmentFieldGroupDto>();
            LookupFields = new List<IndexedLookupListDto>();
            FieldGroupContainers = new List<AssessmentFieldGroupContainerDto>();
        }

        public bool? CanImport { get; set; }
        public bool? AssessmentIsAvailable { get; set; }
        public string StorageTable { get; set; }
        public string SecondaryStorageTable { get; set; }
        public string TertiaryStorageTable { get; set; }
        public bool? IsStateTest { get; set; }
        public int? TestType { get; set; }
        public NSAssessmentBaseType BaseType { get; set; }
        public bool? IsHFW { get; set; }
        public bool? IsProgressMonitoring { get; set; }
        public string AssessmentName { get; set; }
        public string AssessmentDescription { get; set; }
        public string DefaultDataEntryPage { get; set; }
        public string DataEntryPages { get; set; }
        public string DefaultClassReportPage { get; set; }
        public string ClassReportPages { get; set; }
        public int? SortOrder { get; set; }
        public virtual ICollection<AssessmentFieldDto> Fields { get; set; }
        public virtual ICollection<AssessmentFieldCategoryDto> FieldCategories { get; set; }
        public virtual ICollection<AssessmentFieldSubCategoryDto> FieldSubCategories { get; set; }
        public virtual ICollection<AssessmentFieldGroupDto> FieldGroups { get; set; }
        public virtual ICollection<AssessmentFieldGroupContainerDto> FieldGroupContainers { get; set; }
        public List<IndexedLookupListDto> LookupFields { get; set; }
    }
}
