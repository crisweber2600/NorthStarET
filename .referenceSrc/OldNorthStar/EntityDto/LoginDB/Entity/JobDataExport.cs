using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.Entity
{
    public class JobAssessmentDataExport : BaseEntity, INorthStarJob
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

    public class JobAllFieldsAssessmentDataExport : BaseEntity, INorthStarJob
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

    public class JobInterventionDataExport : BaseEntity, INorthStarJob
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
        public string AssessmentName { get; set; }
    }

    public class JobAssessmentDataExportDto : BaseEntity, INorthStarJob
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
        public string BatchName { get; set; }
        public string AssessmentName { get; set; }
    }

    public class JobAssessmentDataExportAllFieldsDto : BaseEntity, INorthStarJob
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
        public string BatchName { get; set; }
        public string AssessmentName { get; set; }
    }

    public class JobInterventionDataExportDto : BaseEntity, INorthStarJob
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
        public string BatchName { get; set; }
        public string AssessmentName { get; set; }
    }
}
