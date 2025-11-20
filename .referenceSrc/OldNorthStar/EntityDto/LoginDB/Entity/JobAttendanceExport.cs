using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.Entity
{
    public class JobAttendanceExport : BaseEntity, INorthStarJob
    {
        public string UploadedFileName { get; set; }
        public int SchoolStartYear { get; set; }
        public string UploadedFileUrl { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public int StaffId { get; set; }
        public string StaffEmail { get; set; }
        public int RecordsProcessed { get; set; }
        public string BatchName { get; set; }
    }

    public class JobAttendanceExportDto : BaseEntity, INorthStarJob
    {
        public string UploadedFileName { get; set; }
        public string DownloadUrl { get; set; }
        public int SchoolStartYear { get; set; }
        public string SchoolYearVerbose { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public int StaffId { get; set; }
        public string StaffEmail { get; set; }
        public int RecordsProcessed { get; set; }
        public string BatchName { get; set; }
    }

    public class JobStudentExport : BaseEntity, INorthStarJob
    {
        public string UploadedFileName { get; set; }
        public int SchoolStartYear { get; set; }
        public string UploadedFileUrl { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public int StaffId { get; set; }
        public string StaffEmail { get; set; }
        public int RecordsProcessed { get; set; }
        public string BatchName { get; set; }
    }

    public class JobStudentExportDto : BaseEntity, INorthStarJob
    {
        public string UploadedFileName { get; set; }
        public string DownloadUrl { get; set; }
        public int SchoolStartYear { get; set; }
        public string SchoolYearVerbose { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public int StaffId { get; set; }
        public string StaffEmail { get; set; }
        public int RecordsProcessed { get; set; }
        public string BatchName { get; set; }
    }

    public class JobStaffExport : BaseEntity, INorthStarJob
    {
        public string UploadedFileName { get; set; }
        public int SchoolStartYear { get; set; }
        public string UploadedFileUrl { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public int StaffId { get; set; }
        public string StaffEmail { get; set; }
        public int RecordsProcessed { get; set; }
        public string BatchName { get; set; }
    }

    public class JobStaffExportDto : BaseEntity, INorthStarJob
    {
        public string UploadedFileName { get; set; }
        public string DownloadUrl { get; set; }
        public int SchoolStartYear { get; set; }
        public string SchoolYearVerbose { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public int StaffId { get; set; }
        public string StaffEmail { get; set; }
        public int RecordsProcessed { get; set; }
        public string BatchName { get; set; }
    }
}
