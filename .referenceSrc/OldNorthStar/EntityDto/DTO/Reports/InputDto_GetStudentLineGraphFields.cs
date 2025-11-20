using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Reports
{
    public class InputDto_GetStudentLineGraphFields
    {
        public int StudentId { get; set; }
        public int AssessmentTypeId { get; set; }
        public int InterventionGroupId { get; set; }
    }
}
