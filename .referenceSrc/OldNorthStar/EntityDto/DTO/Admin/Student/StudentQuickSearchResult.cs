using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NorthStar4.PCL.Entity;

namespace NorthStar4.PCL.DTO
{
	public class StudentQuickSearchResult
	{
		public int id { get; set; }
		public string StudentIdentifier { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
        public string MiddleName { get; set; }
		public DateTime DOB { get; set; }
		public bool IsActive { get; set; }
        public bool disabled { get; set; }
	}
}
