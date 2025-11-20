using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Reports.StackedBarGraphs
{
    public class StackedBarGraphHistoricalSummaryRecord
    {
        public StackedBarGraphHistoricalSummaryRecord()
        {
            ResultsByTDD = new List<SummaryResultByTDD>();
        }

        private List<string> _SchoolsSectionsList;
        public string Student { get; set; }
        public int StudentID { get; set; }
        public string StudentIdentifier { get; set; }
        public string SpecialED { get; set; }
        public string Services { get; set; }
        public string Att1 { get; set; }
        public string Att2 { get; set; }
        public string Att3 { get; set; }
        public string Att4 { get; set; }
        public string Att5 { get; set; }
        public string Att6 { get; set; }
        public string Att7 { get; set; }
        public string Att8 { get; set; }
        public string Att9 { get; set; }
        public string Section { get; set; }

        public string SchoolName { get; set; }
        public int TestDueDateID { get; set; }
        public int ScoreGrouping { get; set; }
        public int FieldValueID { get; set; }

        public List<SummaryResultByTDD> ResultsByTDD { get; set; }

    }
}
