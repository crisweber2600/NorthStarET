using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.Entity
{
    public class JobPrintBatch : BaseEntity, INorthStarJob
    {
        public string SerializedRequest { get; set; }
        public string UploadedFileName { get; set; }
        public int SchoolStartYear { get; set; }
        public string UploadedFileUrl { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public int StaffId { get; set; }
        public string StaffEmail { get; set; }
        public int BenchmarkDateId { get; set; }
        public int RecordsProcessed { get; set; }
        public string BatchName { get; set; }
    }

    public class JobPrintBatchDto : BaseEntity, INorthStarJob
    {
        public string SerializedRequest { get; set; }
        public string UploadedFileName { get; set; }
        public int SchoolStartYear { get; set; }
        public string SchoolYearVerbose { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public int StaffId { get; set; }
        public string StaffEmail { get; set; }
        public int BenchmarkDateId { get; set; }
        public DateTime? BenchmarkDate { get; set; }
        public int RecordsProcessed { get; set; }
        public string Assessments { get; set; }
        public string HfwPages { get; set; }
        public string TextLevelZones { get; set; }
        public string PageTypes { get; set; }
        public string BatchName { get; set; }
    }
}
