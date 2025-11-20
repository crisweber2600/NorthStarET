using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.Entity
{
	public class AssessmentLookupField : BaseEntity
	{
		public string FieldName { get; set; }
		public string FieldValue { get; set; }
		public int SortOrder { get; set; }
		public int? FieldSpecificId { get; set; }
		public string Description { get; set; }
	}
}
