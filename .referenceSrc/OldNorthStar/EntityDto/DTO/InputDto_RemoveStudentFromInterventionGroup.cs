using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.DTO
{
	public class InputDto_RemoveStudentFromInterventionGroup
	{
		public int groupId { get; set; }
		public int studentId { get; set; }
		public int studentSectionId { get; set; }
    }
}
