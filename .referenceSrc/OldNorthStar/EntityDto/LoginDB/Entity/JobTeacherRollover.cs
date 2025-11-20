using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.Entity
{
    public class JobTeacherRollover : BaseEntity, INorthStarJob
    {
        public string UploadedFileName { get; set; }
        public string UploadedFileUrl { get; set; }
        public string ImportLog { get; set; }
        public string PotentialIssuesLog { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public int StaffId { get; set; }
        public string StaffEmail { get; set; }
        public int RecordsProcessed { get; set; }
        public int RecordsSkipped { get; set; }
        public string BatchName { get; set; }
    }

    public class JobTeacherRolloverDto : BaseEntity, INorthStarJob
    {
        public JobTeacherRolloverDto()
        {
            RolloverLogMessages = new List<RolloverLogMessage>();
        }

        public string UploadedFileName { get; set; }
        public string UploadedFileUrl { get; set; }
        public string ImportLog { get; set; }
        public string PotentialIssuesLog { get; set; }
        public string SchoolYearVerbose { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public int StaffId { get; set; }
        public string StaffEmail { get; set; }
        public int RecordsProcessed { get; set; }
        public int RecordsSkipped { get; set; }
        public List<RolloverLogMessage> RolloverLogMessages { get; set; }
        public string BatchName { get; set; }
    }
}
