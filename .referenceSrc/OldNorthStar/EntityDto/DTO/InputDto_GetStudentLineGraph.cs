using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NorthStar4.CrossPlatform.DTO
{
    public class InputDto_GetStudentLineGraph
    {
        public int AssessmentId { get; set; }
        public string FieldToRetrieve { get; set; }
        public string LookupFieldName { get; set; }
        public bool IsLookupColumn { get; set; }
        public int StudentId { get; set; }
    }
}
