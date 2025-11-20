using EntityDto.DTO.Admin.Simple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Assessment
{
    public class OutputDto_AssessmentList : OutputDto_Base
    {
        public List<AssessmentListDto> Assessments { get; set; }
    }
}
