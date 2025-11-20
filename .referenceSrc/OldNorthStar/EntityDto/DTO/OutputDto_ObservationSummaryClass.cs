using EntityDto.DTO.Admin.Simple;
using EntityDto.DTO.Admin.TeamMeeting;
using EntityDto.DTO.Misc;
using EntityDto.DTO.Reports.FP;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.DTO
{
	public class OutputDto_ObservationSummaryClass : OutputDto_Base
	{
        public OutputDto_ObservationSummaryClass()
        {
            InterventionRecords = new List<StudentInterventionReportRecord>();
            StudentAttributes = new List<JObject>();
        }
        public ObservationSummaryGroupResults Scores { get; set; }
        public List<ObservationSummaryBenchmarksByGrade> BenchmarksByGrade { get; set; }		
        public List<IndexedLookupList> LookupLists { get; set; }
        public List<InterventionsByStudent> InterventionsByStudent { get; set; }
        public List<StudentInterventionReportRecord> InterventionRecords { get; set; }
        public List<StudentSPEDLabel> StudentServices { get; set; }
        public List<JObject> StudentAttributes { get; set; }
    }
    public class OutputDto_ObservationSummaryClassMultiple : OutputDto_Base
    {
        public OutputDto_ObservationSummaryClassMultiple()
        {
            InterventionRecords = new List<StudentInterventionReportRecord>();
            StudentAttributes = new List<JObject>();
        }
        public ObservationSummaryGroupResults Scores { get; set; }
        public List<ObservationSummaryBenchmarksByGrade> BenchmarksByGrade { get; set; }
        public List<IndexedLookupList> LookupLists { get; set; }
        public List<InterventionsByStudent> InterventionsByStudent { get; set; }
        public List<StudentInterventionReportRecord> InterventionRecords { get; set; }
        public List<StudentSPEDLabel> StudentServices { get; set; }
        public List<JObject> StudentAttributes { get; set; }
    }
}
