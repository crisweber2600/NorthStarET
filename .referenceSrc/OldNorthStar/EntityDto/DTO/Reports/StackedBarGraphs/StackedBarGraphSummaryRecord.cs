using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Reports.StackedBarGraphs
{
    public class StackedBarGraphSummaryRecord
    {
        public StackedBarGraphSummaryRecord()
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
        public string SchoolsAndSections { get; set; }
        public string School { get; set; }
        public string Grade { get; set; }
        public int GradeOrder { get; set; }
        public string Teacher { get; set; }
        public string HomeLanguage { get; set; }
        public int TestDueDateID { get; set; }
        public List<string> SchoolsSectionsList
        {
            get
            {
                if(_SchoolsSectionsList == null)
                { 
                    _SchoolsSectionsList = new List<string>();
                    _SchoolsSectionsList.AddRange(SchoolsAndSections.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries));
                }

                return _SchoolsSectionsList;
            }
        }
        public int ScoreGrouping { get; set; }
        public int FieldValueID { get; set; }

        public List<SummaryResultByTDD> ResultsByTDD { get; set; }

    }
}
