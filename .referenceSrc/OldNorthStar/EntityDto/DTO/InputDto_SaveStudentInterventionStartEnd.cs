using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.DTO
{
	public class InputDto_SaveStudentInterventionStartEnd
	{
		public string dataType { get; set; }
		public DateTime? date { get; set; }
		public int Id { get; set; }
    }
}
