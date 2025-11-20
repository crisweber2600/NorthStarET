using EntityDto.DTO.Admin.Simple;
using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Reports.StackedBarGraphs
{
    public class OutputDto_StackBarGraphHistoricalSummaryData : OutputDto_Base
    {
        public OutputDto_StackBarGraphHistoricalSummaryData()
        {

        }

        public List<TestDueDateDto> TestDueDates { get; set; }
        public List<StackedBarGraphHistoricalSummaryRecord> SummaryRecords { get; set; }
        public List<AssessmentFieldDto> Fields { get; set; }

        public string Att1Header { get; set; }
        public string Att2Header { get; set; }
        public string Att3Header { get; set; }
        public string Att4Header { get; set; }
        public string Att5Header { get; set; }
        public string Att6Header { get; set; }
        public string Att7Header { get; set; }
        public string Att8Header { get; set; }
        public string Att9Header { get; set; }

        public bool Att1Visible { get; set; }
        public bool Att2Visible { get; set; }
        public bool Att3Visible { get; set; }
        public bool Att4Visible { get; set; }
        public bool Att5Visible { get; set; }
        public bool Att6Visible { get; set; }
        public bool Att7Visible { get; set; }
        public bool Att8Visible { get; set; }
        public bool Att9Visible { get; set; }
    }
}
