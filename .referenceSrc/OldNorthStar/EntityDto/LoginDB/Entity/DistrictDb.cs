using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.Entity
{
    public class DistrictDb : BaseEntityNoTrack
    {
        public int DistrictId { get; set; }
        public string DbName { get; set; }
    }
}
