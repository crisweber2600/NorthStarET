using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.Entity
{
	public class StaffSchoolGrade : BaseEntity
	{
		public int SchoolID { get; set; }
		public int StaffID { get; set; }
		public int StaffHierarchyPermissionID { get; set; }
        public int GradeID { get; set; }
		public virtual School School { get; set; }
		public virtual Staff Staff { get; set; }
        public virtual Grade Grade { get; set; }
	}
}
