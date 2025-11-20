using EntityDto.DTO.Admin.Simple;
using NorthStar4.CrossPlatform.DTO.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Student
{
    public class OutputDto_GetStudentInterventions : OutputDto_Base
    {
        public List<ReportInterventionResult> Interventions { get; set; }
    }
}
