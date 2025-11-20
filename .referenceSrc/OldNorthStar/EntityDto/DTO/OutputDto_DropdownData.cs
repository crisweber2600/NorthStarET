using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.DTO
{
	public class OutputDto_DropdownData
	{
        public OutputDto_DropdownData()
        {
            locked = false;
        }

		public int id { get; set; }
		public string text { get; set; }

        public bool locked { get; set; }
     
	}
}
