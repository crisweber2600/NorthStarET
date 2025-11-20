using EntityDto.DTO.Personal;
using NorthStar4.CrossPlatform.DTO.Reports;
using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NorthStar4.CrossPlatform.DTO
{
    public class OutputDto_GetStudentInterventionGroupLineGraph
    {
        public List<BenchmarkDatesForStudentAndAssessment> BenchmarkDates { get; set; }
        public List<InterventionGroupAssessmentResult> Results { get; set; }
        public List<ReportInterventionResult> Interventions { get; set; }
        public List<AssessmentLookupField> VScale { get; set; }
        public List<AssessmentFieldDto> Fields { get; set; }
    }
}
