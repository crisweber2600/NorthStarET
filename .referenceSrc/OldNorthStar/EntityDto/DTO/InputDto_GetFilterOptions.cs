using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO
{
    public class InputDto_GetFilterOptions
    {
        public string ChangeType { get; set; }
        public int SchoolYear { get; set; }
        public int BenchmarkDateId { get; set; }
        public int SchoolId { get; set; }
        public int GradeId { get; set; }
        public int TeacherId { get; set; }
        public int SectionId { get; set; }
        public int SectionStudentId { get; set; }
        public int InterventionStudentId { get; set; }
        public int InterventionistId { get; set; }
        public int InterventionGroupId { get; set; }
        public int InterventionGroupAssessmentFieldId { get; set; }
        public int ClassroomAssessmentFieldId { get; set; }
        public int StintId { get; set; }
        public int TeamMeetingId { get; set; }
        public int TeamMeetingStaffId { get; set; }
        public bool SchoolYearEnabled { get; set; }
        public bool ClassroomAssessmentFieldEnabled { get; set; }
        public bool InterventionGroupAssessmentFieldEnabled { get; set; }
        public bool SchoolEnabled { get; set; }
        public bool GradeEnabled { get; set; }
        public bool TeacherEnabled {get;set;}
        public bool SectionEnabled { get; set; }
        public bool SectionStudentEnabled { get; set; }
        public bool InterventionStudentEnabled { get; set; }
        public bool InterventionistEnabled { get; set; }
        public bool InterventionGroupEnabled { get; set; }
        public bool StintEnabled { get; set; }
        public bool TeamMeetingEnabled { get; set; }
        public bool TeamMeetingStaffEnabled { get; set; }
        public bool MultiBenchmarkDatesEnabled { get; set; }
        public int StaffRecordStart { get; set; }
        public int StudentRecordStart { get; set; }
        public int HRSFormId { get; set; }
        public int HRSForm2Id { get; set; }
        public int HRSForm3Id { get; set; }
        public bool HRSFormEnabled { get; set; }
        public bool HRSForm2Enabled { get; set; }
        public bool HRSForm3Enabled { get; set; }
        public bool HFWRangeEnabled { get; set; }
        public bool HFWSortOrderEnabled { get; set; }
        public string HFWSortOrder { get; set; }
        public bool HFWMultiRangeEnabled { get; set; }
        public bool StateTestEnabled { get; set; }
        public bool InterventionTestEnabled { get; set; }
        public bool BenchmarkTestEnabled { get; set; }
        public  List<OutputDto_DropdownData_BenchmarkDate> MultiBenchmarkDates { get; set; }
        public List<OutputDto_DropdownData> HFWMultiRange { get; set; }
    }
}
