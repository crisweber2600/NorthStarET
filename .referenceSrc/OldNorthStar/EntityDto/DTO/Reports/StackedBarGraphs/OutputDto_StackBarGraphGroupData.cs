using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Reports.StackedBarGraphs
{
    public class OutputDto_StackBarGraphGroupData
    {
        public DateTime? DueDate { get; set; }
        public int TestDueDateID { get; set; }
        public int ScoreGrouping { get; set; }
        public int NumberOfResults { get; set; }
    }
}
