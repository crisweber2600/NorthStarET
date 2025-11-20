using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.Entity
{
    public class PagePresentation : BaseEntityNoTrack
    {
        public int PageId { get; set; }
        public int PresentationId { get; set; }

        public virtual NSPage Page { get; set; }
        public virtual Presentation Presentation { get; set; }
    }
}
