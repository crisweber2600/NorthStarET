using EntityDto.DTO.Admin.Simple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.DTO
{
	public class InputDto_CopyStudentAssessmentResult : OutputDto_Base
	{
        public int AssessmentId { get; set; }
        public OutputDto_DropdownData TargetBenchmarkDate { get; set; }
        public OutputDto_DropdownData SelectedBenchMarkDate { get; set; }
		public int StudentId { get; set; }
        public OutputDto_DropdownData Section { get; set; }
    }

    public class InputDto_CopyFromStudentAssessmentResult : OutputDto_Base
    {
        public int AssessmentId { get; set; }
        public OutputDto_DropdownData SourceBenchmarkDate { get; set; }
        public OutputDto_DropdownData SelectedBenchMarkDate { get; set; }
        public int StudentId { get; set; }
        public OutputDto_DropdownData Section { get; set; }
    }
}
