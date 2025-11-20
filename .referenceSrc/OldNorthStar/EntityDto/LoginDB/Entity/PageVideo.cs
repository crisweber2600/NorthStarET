using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.Entity
{
    public class PageVideo : BaseEntityNoTrack
    {
        public int PageId { get; set; }
        public int VideoId { get; set; }

        public virtual NSPage Page { get; set; }
        public virtual Video Video { get; set; }
    }
}
