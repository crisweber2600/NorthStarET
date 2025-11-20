using System;
using NorthStar4.PCL.Entity;
using EntityDto.DTO.Admin.Simple;

namespace NorthStar4.CrossPlatform.Entity
{
    public class InterventionAttendance : BaseEntity
    {
        public int SectionID { get; set; }
        public string Notes { get; set; }
        public DateTime AttendanceDate { get; set; }
        public int AttendanceReasonID { get; set; }
        public int StudentID { get; set; }
        public int RecorderID { get; set; }
        public bool? Contact { get; set; }
        public int? ClassStartEndID { get; set; }
        public AttendanceReason AttendanceReason { get; set; }
        public Staff Recorder { get; set; }
    }
}