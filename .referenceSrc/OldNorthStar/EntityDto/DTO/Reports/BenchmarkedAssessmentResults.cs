using NorthStar4.PCL.DTO;
using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NorthStar4.CrossPlatform.DTO.Reports
{
    public class BenchmarkedAssessmentResult
    {
        public BenchmarkedAssessmentResult()
        {
            FieldResults = new List<AssessmentFieldResultDisplayOnly>();
        }

        public List<AssessmentFieldResultDisplayOnly> FieldResults { get; set; }

        public int StudentId { get; set; }
        public int TestDueDateID { get; set; }
        public decimal? FieldValueID { get; set; }
        public string FieldDisplayValue { get; set; }
        public int? FieldSortOrder { get; set; }
        public string Comments { get; set; }
        public bool IsCopied { get; set; }
    }
}
