using EntityDto.DTO.Reports.StackedBarGraphs;
using NorthStar4.PCL.DTO;
using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NorthStar4.CrossPlatform.DTO.Reports.StackedBarGraphs
{
    public class InputDto_GetStackedBarGraphGroupingSummaryUpdatedOptions
    {
        public InputDto_GetStackedBarGraphGroupingSummaryUpdatedOptions()
        {
            DropdownDataList = new List<NamedDropdownData>();
        }

        public AssessmentFieldDto AssessmentField { get; set; }
        public List<NamedDropdownData> DropdownDataList { get; set; }
        public int SchoolStartYear { get; set; }
        public List<OutputDto_DropdownData> Schools { get; set; }
        public List<OutputDto_DropdownData> Grades { get; set; }
        public List<OutputDto_DropdownData> Teachers { get; set; }
        public List<OutputDto_DropdownData> Sections { get; set; }
        public List<OutputDto_DropdownData> EducationLabels { get; set; }
        public List<OutputDto_DropdownData> InterventionTypes { get; set; }
        public List<OutputDto_DropdownData> TitleOneTypes { get; set; }
        public List<OutputDto_DropdownData> Ethnicities { get; set; }
        public OutputDto_DropdownData SpecialEd { get; set; }
        public bool ADSIS { get; set; }
        public bool ELL { get; set; }
        public bool Gifted { get; set; }
        public string Gender { get; set; }
        public string ChangeType { get; set; }
        public int ScoreGrouping { get; set; }
        public DateTime? TestDueDate { get; set; }
        public int AssessmentId { get; set; }
        public string FieldName { get; set; }
    }
}
