using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NorthStar4.CrossPlatform.DTO
{
    public class InputDto_GetGroupedStackBarGraph
    {
        public short SchoolStartYear { get; set; }
        public int? SectionID { get; set; }
        public int? SchoolID { get; set; }
        public int? GradeID { get; set; }
        public int GroupingType { get; set; }
        public string GroupingValue { get; set; }
        public int AssessmentID { get; set; }
        public bool IsDecimalField { get; set; }
        public string FieldToRetrieve { get; set; }
    }
}
