using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.Entity
{
    public class PageTool : BaseEntityNoTrack
    {
        public int PageId { get; set; }
        public int ToolId { get; set; }

        public virtual NSPage Page { get; set; }
        public virtual Tool Tool { get; set; }
    }
}
