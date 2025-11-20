using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.Entity
{
	public class StaffSchool : BaseEntity
	{
		public int SchoolID { get; set; }
		public int StaffID { get; set; }
		public int StaffHierarchyPermissionID { get; set; }
		public virtual School School { get; set; }
		public virtual Staff Staff { get; set; }
        public bool? IsSchoolContact { get; set; }
    }
}
