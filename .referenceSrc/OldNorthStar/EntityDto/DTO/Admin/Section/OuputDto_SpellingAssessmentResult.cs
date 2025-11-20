using NorthStar4.PCL.DTO;
using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Section
{
    public class OuputDto_SpellingAssessmentResult
    {
        public int AssessmentId { get; set; }
        public string AssessmentName { get; set;}
        public List<DataEntryFieldGroupDto> FieldGroups { get; set; }
        public AssessmentStudentResult StudentResult { get; set; }
        public List<DataEntryFieldCategoryDto> Categories { get; set; }
        public List<AssessmentField> AllFields { get; set; }
    }
}
