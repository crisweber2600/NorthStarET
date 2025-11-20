using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.District
{
    public class InputDto_CopyAssessmentToDistrict
    {
        public int DistrictId { get; set; }
        public int AssessmentId { get; set; }
    }
}
