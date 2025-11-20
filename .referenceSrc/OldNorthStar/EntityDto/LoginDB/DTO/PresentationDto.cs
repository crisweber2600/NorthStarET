using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.DTO
{
    public class PresentationDto : BaseEntityNoTrack
    {
        public string ToolName { get; set; }
        public string ToolFileName { get; set; }
        public string Description { get; set; }
        public int? SortOrder { get; set; }
        public string FileSystemFileName { get; set; }
        public DateTime? LastModified { get; set; }
        public int? FileSize { get; set; }
        public string FileExtension { get; set; }
    }
}
