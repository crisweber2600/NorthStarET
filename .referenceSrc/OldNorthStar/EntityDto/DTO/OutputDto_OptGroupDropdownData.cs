using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.DTO
{
	public class OutputDto_OptGroupDropdownData
    {
        public OutputDto_OptGroupDropdownData()
        {
            children = new List<OutputDto_OptGroupDropdownDataSection>();
        }

		public string text { get; set; }


        public List<OutputDto_OptGroupDropdownDataSection> children { get; set; }
	}

    public class OutputDto_OptGroupDropdownDataSection
    {
        public string text { get; set; }
        public int id { get; set; }
        public int SchoolId { get; set; }
        public string SectionName { get; set; }
        public string SchoolName { get; set; }
        public int SchoolStartYear { get; set; }
        public string SchoolYearVerbose { get; set; }
        public int StaffId { get; set; }
        public string StaffName { get; set; }
        public int GradeId { get; set; }
        public string GradeName { get; set; }
        public bool disabled { get; set; }
    }
}
