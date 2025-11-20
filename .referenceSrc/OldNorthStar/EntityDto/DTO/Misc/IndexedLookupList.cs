using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Misc
{
    public class IndexedLookupList
    {
        public IndexedLookupList()
        {
            LookupFields = new List<AssessmentLookupField>();
        }
        public string LookupColumnName { get; set; }
        public List<AssessmentLookupField> LookupFields { get; set; }
    }
}
