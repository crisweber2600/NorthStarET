using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.DTO
{
	public class ObservationSummaryGroupResults
	{
		public ObservationSummaryGroupResults()
		{
			HeaderGroups = new List<ObservationSummaryAssessmentHeaderGroup>();
			Fields = new List<ObservationSummaryAssessmentHeader>();
		}
		public List<ObservationSummaryAssessmentHeaderGroup> HeaderGroups { get; set; }
		public List<ObservationSummaryAssessmentHeader> Fields { get; set; }
		public List<ObservationSummaryStudentResult> StudentResults { get; set; }

        public string Att1Header { get; set; }
        public string Att2Header { get; set; }
        public string Att3Header { get; set; }
        public string Att4Header { get; set; }
        public string Att5Header { get; set; }
        public string Att6Header { get; set; }
        public string Att7Header { get; set; }
        public string Att8Header { get; set; }
        public string Att9Header { get; set; }

        public bool Att1Visible { get; set; }
        public bool Att2Visible { get; set; }
        public bool Att3Visible { get; set; }
        public bool Att4Visible { get; set; }
        public bool Att5Visible { get; set; }
        public bool Att6Visible { get; set; }
        public bool Att7Visible { get; set; }
        public bool Att8Visible { get; set; }
        public bool Att9Visible { get; set; }
    }
}
