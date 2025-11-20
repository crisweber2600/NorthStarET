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

    public class AssessmentListDto : BaseEntityNoTrack
    {
        public AssessmentListDto()
        {
        }

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

    }
}
