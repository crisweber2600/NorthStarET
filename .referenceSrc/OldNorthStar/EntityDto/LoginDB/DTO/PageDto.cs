using EntityDto.LoginDB.DTO;
using NorthStar4.CrossPlatform.Entity;
using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.InterventionToolkit
{
    public class PageDto : BaseEntityNoTrack
    {
        public PageDto()
        {
            this.Tools = new List<ToolDto>();
            this.Presentations = new List<PresentationDto>();
            this.Videos = new List<VideoDto>();
        }
        public string Path { get; set; }
        public string BriefDescription { get; set; }
        public string Title { get; set; }

        public List<ToolDto> Tools { get; set; }
        public List<PresentationDto> Presentations { get; set; }
        public List<VideoDto> Videos { get; set; }
    }

}
