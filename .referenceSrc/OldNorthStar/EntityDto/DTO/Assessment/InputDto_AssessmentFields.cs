using EntityDto.DTO.Admin.Simple;
using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Assessment
{
    public class InputDto_GetAssessmentFields
    {
        public int assessmentId { get; set; }
        public int groupId { get; set; }
        public int categoryId { get; set; }
        public int subCategoryId { get; set; }
        public int page { get; set; }
        public string dbTable { get; set; }
    }

    public class InputDto_GetAssessmentFieldGroups
    {
        public int assessmentId { get; set; }
        public int startOrder { get; set; }
        public int endOrder { get; set; }
    }

    public class OutputDto_LoadAssessmentFields : OutputDto_Base
    {
        public List<AssessmentFieldDto> Fields { get; set; }
    }

    public class OutputDto_LoadAssessmentFieldGroups : OutputDto_Base
    {
        public List<AssessmentFieldGroupDto> Groups { get; set; }
    }
}
