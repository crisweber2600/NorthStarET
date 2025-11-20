using EntityDto.DTO.Admin.Simple;
using EntityDto.DTO.Assessment;
using EntityDto.DTO.Reports.StackedBarGraphs;
using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NorthStar4.CrossPlatform.DTO.Reports.StackedBarGraphs
{


    public class OutputDto_GetStackedBarGraphGroupingUpdatedOptions :  OutputDto_Base
    {
        public OutputDto_GetStackedBarGraphGroupingUpdatedOptions()
        {
            DropdownDataList = new List<NamedDropdownData>();
        }

        public List<NamedDropdownData> DropdownDataList { get; set; }
        public List<OutputDto_DropdownData> Schools { get; set; }
        public int? SelectedSchool { get; set; }
        public List<OutputDto_DropdownData> Grades { get; set; }
        public int? SelectedGrade { get; set; }
        public List<OutputDto_DropdownData> Teachers { get; set; }
        public int? SelectedTeacher { get; set; }
        public List<OutputDto_DropdownData> Sections { get; set; }
        public List<OutputDto_DropdownData> Students { get; set; }
        public List<OutputDto_DropdownData> InterventionStudents { get; set; }
        public List<OutputDto_ObservationSummaryFieldVisibility> InterventionAssessments { get; set; }
        public List<OutputDto_DropdownData> Interventionists { get; set; }
        public List<OutputDto_DropdownData> InterventionGroups { get; set; }
        public List<OutputDto_DropdownData> InterventionStints { get; set; }
        public int? SelectedSection { get; set; }
        public List<OutputDto_DropdownData> SchoolYears { get; set; }
        public int? SelectedSchoolYear { get; set; }
        public List<OutputDto_DropdownData> EducationLabels { get; set; }
        public List<OutputDto_DropdownData> InterventionTypes { get; set; }
        public List<OutputDto_DropdownData> TitleOneTypes { get; set; }
        public List<OutputDto_DropdownData> Ethnicities { get; set; }
        public List<OutputDto_DropdownData> Genders { get; set; }
        public List<OutputDto_DropdownData> TestDueDates { get; set; }
        public int? SelectedTestDueDateId { get; set; }
    }
}
