using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.DTO
{
	public class OutputDto_DropdownData_BenchmarkDate
	{
        public OutputDto_DropdownData_BenchmarkDate()
        {

        }

		public int id { get; set; }
		public string text { get; set; }
        public string Hex { get; set; }
        public string testLevelPeriodId { get; set; }

    }
}
