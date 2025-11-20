using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.DTO
{
	public class LIDSummaryFieldResult
    {
		public int GroupId { get; set; }
		public DateTime? CellColorDate { get; set; }
        public DateTime? XColorDate { get; set; }
		public string DbColumn { get; set; }
	}
}
