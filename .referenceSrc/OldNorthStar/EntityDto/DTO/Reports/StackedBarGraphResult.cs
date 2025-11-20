using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NorthStar4.CrossPlatform.DTO.Reports
{
    public class StackedBarGraphResult
    {
        public DateTime DueDate { get; set; }
        public int TestDueDateID { get; set; }
        public string GroupingValue { get; set; }
        public int ScoreGrouping { get; set; }
        public int NumberOfResults { get; set; }
        public string DisplayDate { get { return DueDate.ToString("MMM-yy"); } }
    }
}
