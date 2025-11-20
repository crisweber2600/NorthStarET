using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.Entity
{
	public class TestDueDate : BaseEntity
	{
		public int? SchoolStartYear { get; set; }
		public DateTime? DueDate { get; set; }
		public int? TestLevelPeriodID { get; set; }
		public  string Notes { get; set; }
		public string Hex { get; set; }
		public bool? IsSupplemental { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
        public string DisplayDate { get { return DueDate.HasValue ? DueDate.Value.ToString("dd-MMM-yyyy") : "N/A"; }  }
        public string PeriodName
        {
            get
            {
                switch (TestLevelPeriodID)
                {
                    case 1:
                        return "Beginning of Year";
                    case 2:
                        return "1st Interval Assessment";
                    case 3:
                        return "2nd Interval Assessment";
                    case 4:
                        return "End of Year";
                    default:
                        return "Supplemental";
                }
            }
            set { }
        }
    }
}
