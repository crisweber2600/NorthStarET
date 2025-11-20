using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.Entity
{


    public class JobInterventionDataImport : BaseEntity, INorthStarJob
    {
        public int AssessmentId { get; set; }
        public string UploadedFileName { get; set; }
        public int SchoolStartYear { get; set; }
        public string UploadedFileUrl { get; set; }
        public string ImportLog { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public int StaffId { get; set; }
        public int RecorderId { get; set; }
        public string StaffEmail { get; set; }
        public int InterventionGroupId { get; set; }
        public int StintId { get; set; }
        public int RecordsProcessed { get; set; }
        public int RecordsSkipped { get; set; }
        public string BatchName { get; set; }
    }

    public class JobInterventionDataImportDto : BaseEntity, INorthStarJob
    {
        public int AssessmentId { get; set; }
        public string AssessmentName { get; set; }
        public string UploadedFileName { get; set; }
        public int SchoolStartYear { get; set; }
        public string SchoolYearVerbose { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public int StaffId { get; set; }
        public int RecorderId { get; set; }
        public string RecorderName { get; set; }
        public string StaffEmail { get; set; }
        public int InterventionGroupId { get; set; }
        public int StintId { get; set; }
        public string InterventionGroupName { get; set; }
        public string StintName { get; set; }
        public int RecordsProcessed { get; set; }
        public int RecordsSkipped { get; set; }
        public string BatchName { get; set; }
    }
}
