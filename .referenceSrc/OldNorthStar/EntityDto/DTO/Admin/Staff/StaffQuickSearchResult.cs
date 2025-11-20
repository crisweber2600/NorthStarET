using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NorthStar4.PCL.Entity;

namespace NorthStar4.PCL.DTO
{
	public class StaffQuickSearchResult
	{
		public int id { get; set; }
		public string TeacherKey { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public bool IsActive { get; set; }
		public bool IsInterventionist { get; set; }
        public bool disabled { get; set; }
        public string text { get
            {
                return FirstName + " " + LastName;
            } }
	}
}
