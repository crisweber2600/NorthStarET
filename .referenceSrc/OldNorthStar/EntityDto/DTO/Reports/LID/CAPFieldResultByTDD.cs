using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NorthStar4.CrossPlatform.DTO.Reports
{
    public class LIDFieldResultByTDD
    {
        public int TDDID { get; set; }
        public List<LIDFieldResult> FieldResults { get; set; }
    }
}
