using EntityDto.DTO.Admin.Simple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Reports.StackedBarGraphs
{
    public class OutputDto_StackedBarGraphComparisonData : OutputDto_Base
    {
        public string GroupName { get; set; }
        public List<OutputDto_StackBarGraphGroupData> Results { get; set; }
    }

    public class OutputDto_GetPLCPlanningReport : OutputDto_Base
    {
        public OutputDto_GetPLCPlanningReport()
        {
            PLCSectionInfoList = new List<PLCSectionInfo>();
        }

        public List<PLCSectionInfo> PLCSectionInfoList { get; set; }

    }

    public class PLCSectionInfo
    {
        public string TeacherName { get; set; }
        public int StudentCount { get; set; }
        public string ClassName { get; set; }
        public int SectionId { get; set; }
        public int StaffId { get; set; }

        public List<OutputDto_StackBarGraphGroupData> GenEdResults { get; set; }
        public List<OutputDto_StackBarGraphGroupData> SpEdResults { get; set; }
        public List<OutputDto_StackBarGraphGroupData> ELResults { get; set; }
        public List<OutputDto_StackBarGraphGroupData> GTResults { get; set; }

        public List<OutputDto_StackBarGraphGroupData> T1Results { get; set; }
    }
}
