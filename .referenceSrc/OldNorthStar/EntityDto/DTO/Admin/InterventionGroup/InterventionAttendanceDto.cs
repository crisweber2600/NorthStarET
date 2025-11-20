using EntityDto.DTO.Admin.Simple;
using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.InterventionGroup
{
    public class InterventionAttendanceDto : BaseEntity
    {
        public int SectionID { get; set; }
        public string Notes { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string AttendanceDateString { get; set; }
        public int AttendanceReasonID { get; set; }
        public int StudentID { get; set; }
        public int RecorderID { get; set; }
        public bool? Contact { get; set; }
        public int? ClassStartEndID { get; set; }
        public StaffDto Recorder { get; set; }
        public string AttendanceStatus { get; set; }
    }

    public class AttendanceExportInfo
    {
        public string YearVerbose { get; set; }
        public int InterventionGroupId { get; set; }
        public string GroupName { get; set; }
        public int StartEndDateID { get; set; }
        public DateTime InterventionStart { get; set; }
        public DateTime? InterventionEnd { get; set; }
        public DateTime GroupStartTime { get; set; }
        public DateTime GroupEndTime { get; set; }
        public int StudentID { get; set; }
        public int SchoolID { get; set; }
        public string SchoolName { get; set; }
        public string StudentNumber { get; set; }
        public string Student { get; set; }
        public string Interventionist { get; set; }

        public DateTime? AttendanceDate { get; set; }
        public string AttendanceReason { get; set; }
        public string Comment { get; set; }
        public string InterventionType { get; set; }
    }

}
