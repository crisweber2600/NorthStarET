using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.Entity
{
	public class SchoolYear : BaseEntityNoId
	{
		public int SchoolStartYear { get; set; }
		public string YearVerbose { get; set; }
		public int SchoolEndYear { get; set; }
	}
}
