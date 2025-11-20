using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.DTO
{
	public class InputDto_MoveStudentToInterventionGroup
	{
		public int oldGroupId { get; set; }
		public int newGroupId { get; set; }
		public int studentId { get; set; }
		public int studentSectionId { get; set; }
    }
}
