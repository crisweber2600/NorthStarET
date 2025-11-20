using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.Entity
{
    public class Tool : BaseEntityNoTrack
    {
        public Tool()
        {
            this.PageTools = new HashSet<PageTool>();
        }
        public string ToolName { get; set; }
        public string ToolFileName { get; set; }
        public string Description { get; set; }
        public int? SortOrder { get; set; }
        public string FileSystemFileName { get; set; }
        public DateTime? LastModified { get; set; }
        public int? FileSize { get; set; }
        public string FileExtension { get; set; }

        public virtual ICollection<PageTool> PageTools { get; set; }
    }
}
