using EntityDto.DTO.Reports.StackedBarGraphs;
using NorthStar4.PCL.DTO;
using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NorthStar4.CrossPlatform.DTO.Reports.StackedBarGraphs
{
    public class InputDto_GetStackedBarGraphGroupingUpdatedOptions
    {
        public InputDto_GetStackedBarGraphGroupingUpdatedOptions()
        {
            DropdownDataList = new List<NamedDropdownData>();
            Schools = new List<OutputDto_DropdownData>();
            Grades = new List<OutputDto_DropdownData>();
            Teachers = new List<OutputDto_DropdownData>();
            Sections = new List<OutputDto_DropdownData>();
            Students = new List<OutputDto_DropdownData>();
            InterventionTypes = new List<OutputDto_DropdownData>();
            Interventionists = new List<OutputDto_DropdownData>();
            InterventionGroups = new List<OutputDto_DropdownData>();
            InterventionStints = new List<OutputDto_DropdownData>();
        }
        public int SchoolStartYear { get; set; }
        public List<NamedDropdownData> DropdownDataList { get; set; }
        public List<OutputDto_DropdownData> Schools { get; set; }
        public List<OutputDto_DropdownData> Grades { get; set; }
        public List<OutputDto_DropdownData> Teachers { get; set; }
        public List<OutputDto_DropdownData> Sections { get; set; }
        public List<OutputDto_DropdownData> Students { get; set; }
        public List<OutputDto_DropdownData> InterventionStudents { get; set; }
        public OutputDto_DropdownData InterventionAssessment { get; set; }
        public List<OutputDto_DropdownData> Interventionists { get; set; }
        public List<OutputDto_DropdownData> InterventionGroups { get; set; }
        public List<OutputDto_DropdownData> InterventionStints { get; set; }
        public List<OutputDto_DropdownData> EducationLabels { get; set; }
        public List<OutputDto_DropdownData> InterventionTypes { get; set; }
        public AssessmentFieldDto AssessmentField { get; set; }
        public OutputDto_DropdownData SpecialEd { get; set; }
        public string ChangeType { get; set; }
        public int? TestDueDateID { get; set; }
        public string GroupName { get; set; }
    }
}
