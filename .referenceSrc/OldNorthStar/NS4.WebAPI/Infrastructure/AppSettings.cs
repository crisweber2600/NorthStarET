using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NorthStar4.API.Infrastructure
{
    public class AppSettings
    {
        public string CORSOrigin { get; set; }
        public string IdentityServer { get; set; }
        public string SiteUrlBase { get; set; }
        public string LoginConnection { get; set; }
        public string VzaarToken { get; set; }
        public string VzaarSecret { get; set; }
    }
}
