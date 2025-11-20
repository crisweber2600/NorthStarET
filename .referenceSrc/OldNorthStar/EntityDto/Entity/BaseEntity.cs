using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.Entity
{
	public abstract class BaseEntity
	{
		public int Id { get; set; }
		public DateTime? ModifiedDate { get; set; }
		public string Ip { get; set; }
		public string ModifiedBy { get; set; }
	}
}
