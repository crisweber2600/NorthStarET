using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Misc
{
    public class InputDto_PrintSettings
    {
        public bool ForcePortraitPageSize { get; set; }
        public string Url { get; set; }
        public int SchoolYear { get; set; }
        public int SchoolId { get; set; }
        public int GradeId { get; set; }
        public int TeacherId { get; set; }
        public int SectionId { get; set; }
        public int StudentId { get; set; }
        public int InterventionistId { get; set; }
        public int InterventionGroupId { get; set; }
        public bool SchoolYearEnabled { get; set; }
        public bool SchoolEnabled { get; set; }
        public bool GradeEnabled { get; set; }
        public bool TeacherEnabled { get; set; }
        public bool SectionEnabled { get; set; }
        public bool StudentEnabled { get; set; }
        public bool InterventionistEnabled { get; set; }
        public bool InterventionGroupEnabled { get; set; }
        public int StaffRecordStart { get; set; }
        public int StudentRecordStart { get; set; }
        public bool? PrintLandscape { get; set; }
        public bool? PrintMultiPage { get; set; }
        public bool? FitHeight { get; set; }
        public bool? FitWidth { get; set; }
        public bool? StretchToFit { get; set; }
        public int? HtmlViewerWidth { get; set; }
        public int? HtmlViewerHeight { get; set; }
        public string SourceBenchmarkDate { get; set; }
        public string SortParam { get; set; }
        public string GroupsParam { get; set; }
        public string GroupsArrayParam { get; set; }
        public string ObjParam { get; set; }
        public bool ReportByNumberOfStudents { get; set; }
        public bool IsSummaryMode { get; set; }
        public string SummaryDataParam { get; set; }
    }
}
