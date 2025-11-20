using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.Entity
{
    public class NorthStarVersion : BaseEntityNoTrack
    {
        public decimal Version { get; set; }
    }
}
