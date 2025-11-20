using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NorthStar4.PCL.Entity;
using EntityDto.DTO.Assessment;
using EntityDto.Entity;
using EntityDto.DTO.Admin.Simple;
using EntityDto.DTO.Reports.FP;
using EntityDto.DTO.Assessment.Benchmarks;
using Newtonsoft.Json.Linq;

namespace NorthStar4.PCL.DTO
{
	public class OutputDto_StudentSectionFPReportResults : OutputDto_Base
    {
		public OutputDto_StudentSectionFPReportResults()
		{
            StudentSectionReportResults = new List<StudentSectionFPReportResult>();
            InterventionRecords = new List<StudentInterventionReportRecord>();
            PreviousGradeScores = new List<StudentPreviousGradeReportRecord>();
		}

        public List<StudentSectionFPReportResult> StudentSectionReportResults { get; set; }
		//public List<AssessmentStudentResult> StudentResults { get; set; }
		public AssessmentDto Assessment { get; set; }
        public List<TestDueDateDto> TestDueDates { get; set; }
        public List<AssessmentFieldDto> HeaderFields { get; set; }
        public List<FPComparison> Scale { get; set; }
        public DistrictBenchmarkDto StartOfYearBenchmark { get; set; }
        public DistrictBenchmarkDto TargetZone { get; set; }
        public DistrictBenchmarkDto EndOfYearBenchmark { get; set; }
        public List<BenchmarksByGrade> BenchmarksByGrade { get; set; }
        public List<StudentInterventionReportRecord> InterventionRecords { get; set; }
        public List<StudentPreviousGradeReportRecord> PreviousGradeScores { get; set; }
        public List<StudentSPEDLabel> StudentServices { get; set; }
    }

    public class BenchmarksByGrade
    {
        public DistrictBenchmarkDto StartOfYearBenchmark { get; set; }
        public DistrictBenchmarkDto TargetZone { get; set; }
        public DistrictBenchmarkDto EndOfYearBenchmark { get; set; }
        public int GradeId { get; set; }
    }

    public class OutputDto_ClassRosterReportResults : OutputDto_Base
    {
        public OutputDto_ClassRosterReportResults()
        {
            StudentSectionReportResults = new List<AssessmentStudentResult>();
            InterventionRecords = new List<StudentInterventionReportRecord>();
            StudentAttributes = new List<JObject>();
        }

        public List<AssessmentStudentResult> StudentSectionReportResults { get; set; }
        public AssessmentDto Assessment { get; set; }
        public List<AssessmentFieldDto> HeaderFields { get; set; }
        public List<StudentInterventionReportRecord> InterventionRecords { get; set; }        
        public List<StudentSPEDLabel> StudentServices { get; set; }
        public List<JObject> StudentAttributes { get; set; }
        public ObservationSummaryGroupResults StudentACCESSReportResults { get; set; }
    }
}
