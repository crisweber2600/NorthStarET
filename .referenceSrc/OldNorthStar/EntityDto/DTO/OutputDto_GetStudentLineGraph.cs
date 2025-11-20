using EntityDto.DTO.Admin.Simple;
using EntityDto.DTO.Personal;
using NorthStar4.CrossPlatform.DTO.Reports;
using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NorthStar4.CrossPlatform.DTO
{
    public class OutputDto_GetStudentLineGraph : OutputDto_Base
    {
        public List<BenchmarkDatesForStudentAndAssessment> BenchmarkDates { get; set; }
        public List<BenchmarkedAssessmentResult> Results { get; set; }
        public List<AssessmentLookupField> VScale { get; set; }
        public List<AssessmentFieldDto> Fields { get; set; }
       // public OutputDto_GetAssessmentsAndFieldsForUser Fields { get; set; }
    }
}
