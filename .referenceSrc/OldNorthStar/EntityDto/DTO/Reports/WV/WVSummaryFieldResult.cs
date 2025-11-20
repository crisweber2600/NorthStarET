using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.DTO
{
	public class WVSummaryFieldResult
    {
        public WVSummaryFieldResult()
        {
            XColorDates = new List<DateTime?>();
        }

		public DateTime? CellColorDate { get; set; }
        public List<DateTime?> XColorDates { get; set; }
		public int WordsCorrect { get; set; }
	}
}
