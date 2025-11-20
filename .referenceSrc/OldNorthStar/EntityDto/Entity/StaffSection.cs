using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.Entity
{
	public class StaffSection : BaseEntity
	{
		public int StaffID { get; set; }
		public int ClassID { get; set; }
		public int StaffHierarchyPermissionID { get; set; }

		public virtual Section Section { get; set; }
		public virtual Staff Staff { get; set; }
	}
}
