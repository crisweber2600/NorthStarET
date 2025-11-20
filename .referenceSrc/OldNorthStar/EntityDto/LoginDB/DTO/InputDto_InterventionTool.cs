using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.DTO
{
    public class InputDto_InterventionTool
    {
        public NSInterventionToolDto Tool { get; set; }
    }

    public class InputDto_InterventionVideo
    {
        public NSInterventionVideoDto Video { get; set; }
    }

    public class InputDto_PageVideo
    {
        public VideoDto Video { get; set; }
    }

    public class InputDto_PageTool
    {
        public ToolDto Tool { get; set; }
    }

    public class InputDto_PagePresentation
    {
        public PresentationDto Presentation { get; set; }
    }
}
