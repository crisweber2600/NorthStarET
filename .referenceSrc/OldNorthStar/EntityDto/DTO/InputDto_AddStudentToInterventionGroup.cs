using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.DTO
{
	public class InputDto_AddStudentToInterventionGroup
	{
		public int groupId { get; set; }
		public DateTime? startDate { get; set; }
		public DateTime? endDate { get; set; }
		public int studentId { get; set; }
	}
}
