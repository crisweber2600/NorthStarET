using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.DTO
{
	public class LIDFieldResult
    {
		public string Comment { get; set; }
		public int GroupId { get; set; }
		public bool? Checked { get; set; }
		public string DbColumn { get; set; }
	}
}
