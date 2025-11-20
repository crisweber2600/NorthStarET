using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NorthStar4.PCL.Entity;
using EntityDto.DTO.Admin.Simple;
using EntityDto.DTO.Assessment;

namespace NorthStar4.PCL.DTO
{
	public class OutputDto_FilterOptions
	{
		public int? SelectedSchoolStartYear { get; set; }
		public List<OutputDto_DropdownData> SchoolYears { get; set; }
		public int? SelectedSchool { get; set; } 
		public List<OutputDto_DropdownData> Schools { get; set; }
		public int? SelectedGrade { get; set; }
		public List<OutputDto_DropdownData> Grades { get; set; }
		public int? SelectedTeacher { get; set; }
		public List<OutputDto_DropdownData> Teachers { get; set; }
        public int? SelectedInterventionist { get; set; }
        public List<OutputDto_DropdownData> Interventionists { get; set; }
        public int? SelectedSection { get; set; }
		public List<OutputDto_DropdownData> Sections { get; set; }
        public int? SelectedInterventionGroup { get; set; }
        public List<OutputDto_DropdownData> InterventionGroups { get; set; }
        public int? SelectedSectionStudent { get; set; }
		public List<OutputDto_DropdownData> SectionStudents { get; set; }
        public int? SelectedInterventionStudent { get; set; }
        public List<OutputDto_DropdownData> InterventionStudents { get; set; }
        public int? SelectedStint { get; set; }
        public List<OutputDto_DropdownData> Stints { get; set; }
        public int? SelectedTeamMeeting { get; set; }
        public List<OutputDto_DropdownData> TeamMeetings { get; set; }
        public int? SelectedTeamMeetingStaff { get; set; }
        public List<OutputDto_DropdownData> TeamMeetingStaffs { get; set; }
        public int? SelectedTDD { get; set; }
		public List<OutputDto_DropdownData_BenchmarkDate> TestDueDates { get; set; }
        public List<OutputDto_DropdownData_BenchmarkDate> SelectedTestDueDates { get; set; }
        public List<OutputDto_DropdownData_BenchmarkDate> MultiYearTestDueDates { get; set; }
        public List<OutputDto_DropdownData> HRSForms { get; set; }
        public List<OutputDto_DropdownData> HRSForms2 { get; set; }
        public List<OutputDto_DropdownData> HRSForms3 { get; set; }
        public int? SelectedHRSForm { get; set; }
        public int? SelectedHRSForm2 { get; set; }
        public int? SelectedHRSForm3 { get; set; }
        public List<AssessmentDto> ClassroomAssessmentFields { get; set; }
        public int? SelectedClassroomAssessmentField { get; set; }
        public List<AssessmentDto> InterventionGroupAssessmentFields { get; set; }
        public int? SelectedInterventionGroupAssessmentField { get; set; }
        public List<OutputDto_DropdownData> SelectedHFWMultiRange { get; set; }
        public List<OutputDto_DropdownData> BenchmarkTests { get; set; }
        public List<OutputDto_DropdownData> StateTests { get; set; }
        public List<OutputDto_DropdownData> InterventionTests { get; set; }
        public string SelectedHFWSortOrder { get; set; }
    }
}
