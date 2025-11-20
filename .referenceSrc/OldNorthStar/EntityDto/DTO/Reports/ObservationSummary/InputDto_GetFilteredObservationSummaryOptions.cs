using EntityDto.DTO.Reports.StackedBarGraphs;
using NorthStar4.PCL.DTO;
using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NorthStar4.CrossPlatform.DTO.Reports.ObservationSummary
{
    public class InputDto_GetFilteredObservationSummaryOptions
    {
        public InputDto_GetFilteredObservationSummaryOptions()
        {
            DropdownDataList = new List<NamedDropdownData>();
        }
        public int SchoolStartYear { get; set; }
        public List<NamedDropdownData> DropdownDataList { get; set; }
        public List<OutputDto_DropdownData> Schools { get; set; }
        public List<OutputDto_DropdownData> Grades { get; set; }
        public List<OutputDto_DropdownData> Teachers { get; set; }
        public List<OutputDto_DropdownData> Interventionists { get; set; }
        public List<OutputDto_DropdownData> InterventionGroups { get; set; }
        public List<OutputDto_DropdownData> InterventionStudents { get; set; }
        public List<OutputDto_DropdownData> Sections { get; set; }
        public List<OutputDto_DropdownData> EducationLabels { get; set; }
        public List<OutputDto_DropdownData> InterventionTypes { get; set; }
        public OutputDto_DropdownData SpecialEd { get; set; }
        public List<OutputDto_DropdownData> TitleOneTypes { get; set; }
        public List<OutputDto_DropdownData> Ethnicities { get; set; }
        public List<OutputDto_DropdownData> Assessments { get; set; }
        public AssessmentFieldDto AssessmentField { get; set; }
        public int? TestDueDateID { get; set; }
        public string BatchName { get; set; }
        public OutputDto_DropdownData InterventionAssessment { get; set; }
    }

    public class InputDto_GetSectionMultipleObservationSummary
    {
        public InputDto_GetSectionMultipleObservationSummary()
        {
           
        }
        public int SectionId { get; set; }
        public List<OutputDto_DropdownData> TestDueDates { get; set; }
        public bool IsMultiColumn { get; set; }
    }

    public class InputDto_CreateHRISWSentence
    {
        public InputDto_CreateHRISWSentence()
        {

        }
        public int AssessmentId { get; set; }
        public int FormId { get; set; }
        public string Suffix { get; set; }
        public string Sentence { get; set; }
    }

    public class InputDto_GetFilteredPrintBatchOptions
    {
        public InputDto_GetFilteredPrintBatchOptions()
        {
            DropdownDataList = new List<NamedDropdownData>();
        }
        public int SchoolStartYear { get; set; }
        public List<NamedDropdownData> DropdownDataList { get; set; }
        public List<OutputDto_DropdownData> Schools { get; set; }
        public List<OutputDto_DropdownData> Grades { get; set; }
        public List<OutputDto_DropdownData> Teachers { get; set; }
        public List<OutputDto_DropdownData> Sections { get; set; }
        public List<OutputDto_DropdownData> EducationLabels { get; set; }
        public List<OutputDto_DropdownData> InterventionTypes { get; set; }
        public OutputDto_DropdownData SpecialEd { get; set; }
        public List<OutputDto_DropdownData> TitleOneTypes { get; set; }
        public List<OutputDto_DropdownData> Ethnicities { get; set; }
        public List<OutputDto_DropdownData> Assessments { get; set; }
        public List<OutputDto_DropdownData> Students { get; set; }
        public List<OutputDto_DropdownData> HfwPages { get; set; }
        public List<OutputDto_DropdownData> HfwStudentReports { get; set; }
        public List<OutputDto_DropdownData> PageTypes { get; set; }
        public List<OutputDto_DropdownData> TargetLevelZones { get; set; }
        public int? TestDueDateID { get; set; }
        public string BatchName { get; set; }
    }
}
