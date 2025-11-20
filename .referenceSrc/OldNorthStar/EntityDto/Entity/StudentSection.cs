using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.Entity
{
	public class StudentSection : BaseEntity
	{
        public StudentSection()
        {
            //InterventionAttendances = new HashSet<InterventionAttendance>();
        }

        public int StudentID { get; set; }
		public int ClassID { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public int? LastAssociatedTDDID { get; set; }
		public string Notes { get; set; }
		public DateTime? LastAssociatedTDD { get; set; }
		public Student Student { get; set; }
		public Section Section { get; set; }

        //public virtual ICollection<InterventionAttendance> InterventionAttendances { get; set; }
    }
}
