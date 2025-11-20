using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.DTO
{
	public class InterventionGroupStudentDto
	{
		public int Id { get; set; }
		public int StudentId { get; set; }
        public int InterventionistId { get; set; }
        public int SchoolId { get; set; }
        public int SchoolYear { get; set; }
		public string StudentName { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime? EndDate { get; set; }
        public string StartDateText { get; set; }
        public string EndDateText { get; set; }
        public int InterventionGroupId { get; set; }
        public bool CanBeDeleted { get; set; }
        public string InterventionGroupName { get; set; }
        public string InterventionType { get; set; }
        public string InterventionTypeLong { get; set; }
        public string StintName { get { return StartDate.ToString("dd-MMM-yyyy") + " --> " + (EndDate.HasValue ? EndDate.Value.ToString("dd-MMM-yyyy") : "No End Date"); } }
	}
}
