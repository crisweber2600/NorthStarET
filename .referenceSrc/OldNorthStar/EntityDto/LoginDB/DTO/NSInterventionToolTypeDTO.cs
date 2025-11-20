using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.Entity
{
    public class NSInterventionToolTypeDto :BaseEntityNoTrack
    {
        public NSInterventionToolTypeDto()
        {
        }

        public string Name { get; set; }

    }
}
