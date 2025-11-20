using EntityDto.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.Entity
{
	public class Grade : BaseEntity
	{
        public Grade()
        {
            Sections = new HashSet<Section>();
            AssessmentBenchmarks = new List<Assessment_Benchmarks>();
        }
        public string ShortName { get; set; }
		public string LongName { get; set; }
		public int GradeOrder { get; set; }
        public string StateGradeNumber { get; set; }

        public virtual ICollection<Section> Sections { get; set; }
        public virtual ICollection<Assessment_Benchmarks> AssessmentBenchmarks { get; set; }
    }
}
