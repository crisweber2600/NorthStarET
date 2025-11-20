using EntityDto.DTO.Admin.Simple;
using NorthStar4.CrossPlatform.DTO.Reports;
using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Reports
{
    public class OutputDto_LineGraphResults : OutputDto_Base
    {
        public List<BenchmarkedAssessmentResult> StudentResults { get; set; }
        public List<AssessmentFieldDto> Fields { get; set; }
    }
}
