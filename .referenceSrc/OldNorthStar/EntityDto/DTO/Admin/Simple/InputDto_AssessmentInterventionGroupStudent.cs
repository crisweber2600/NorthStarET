using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Simple
{
    public class InputDto_AssessmentInterventionGroupStudent
    {
        public int AssessmentId { get; set; }
        public int InterventionGroupId { get; set; }
        public int StudentId { get; set; }
    }
}
