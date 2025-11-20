using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Reports.StackedBarGraphs
{
    public class SummaryResultByTDD
    {
        public SummaryResultByTDD()
        {
            FieldResults = new List<AssessmentFieldResult>();
        }

        public int TDDID { get; set; }
        public int PeriodId { get; set; }

        public List<AssessmentFieldResult> FieldResults { get; set; }
    }
}
