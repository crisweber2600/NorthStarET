using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.Entity
{
    public class NSPage : BaseEntityNoTrack
    {
        public NSPage()
        {
            this.PagePresentations = new HashSet<PagePresentation>();
            this.PageTools = new HashSet<PageTool>();
            this.PageVideos = new HashSet<PageVideo>();
        }
        public string Path { get; set; }
        public string BriefDescription { get; set; }
        public string Title { get; set; }
        public virtual ICollection<PageTool> PageTools { get; set; }
        public virtual ICollection<PagePresentation> PagePresentations { get; set; }
        public virtual ICollection<PageVideo> PageVideos { get; set; }

    }
}
