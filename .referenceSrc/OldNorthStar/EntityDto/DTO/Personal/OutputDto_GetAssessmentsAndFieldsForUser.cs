using EntityDto.DTO.Assessment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Personal
{
    public class OutputDto_GetAssessmentsAndFieldsForUser
    {
        public List<AssessmentDto> Assessments { get; set; }
    }
}
