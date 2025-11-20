using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.Entity
{
    public class StaffDistrict : BaseEntityNoTrack
    {
        public string StaffEmail { get; set; }
        public int DistrictId { get; set; }
        public decimal? CurrentVersion { get; set; }
        public DateTime? VersionLastUpdated { get; set; }
        public bool? IsSA { get; set; }
        //public int PermissionLevel { get; set; }
    }
}
