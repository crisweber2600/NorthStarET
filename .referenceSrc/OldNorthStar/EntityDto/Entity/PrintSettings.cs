using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.Entity
{
    public class PrintSetting : BaseEntityNoTrack
    {
        public string Url { get; set; }
        public bool? PrintLandscape { get; set; }
        public bool? PrintMultiPage { get; set; }
        public bool? FitHeight { get; set; }
        public bool? FitWidth { get; set; }
        public bool? StretchToFit { get; set; }
        public int? HtmlViewerWidth { get; set; }
        public int? HtmlViewerHeight { get; set; }
        public bool? ForcePortraitPageSize { get; set; }
        public int? ConversionDelay { get; set; }
    }
}
