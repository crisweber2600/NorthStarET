using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.Entity
{
	public class StudentSchool : BaseEntity
	{
		public int StudentID { get; set; }
		public int SchoolID { get; set; }
		public int SchoolStartYear { get; set; }
        public int? GradeId { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public virtual School School { get; set; }
		public virtual Student Student { get; set; }
        public virtual SchoolYear SchoolYear { get; set; }
        public virtual Grade Grade { get; set; }
	}
}
