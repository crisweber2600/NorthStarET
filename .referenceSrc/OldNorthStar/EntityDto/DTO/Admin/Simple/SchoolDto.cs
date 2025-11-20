using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Simple
{
    public class SchoolDto : BaseEntityNoTrack
    {
        public bool? IsPreK { get; set; }
        public bool? IsK2 { get; set; }
        public bool? Is35 { get; set; }
        public bool? IsK5 { get; set; }
        public bool? IsK8 { get; set; }
        public bool? IsMS { get; set; }
        public bool? IsHS { get; set; }
        public bool? IsSS { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
