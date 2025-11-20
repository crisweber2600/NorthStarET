using NorthStar.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Northstar.Core
{
    public class NSAzureJob
    {
        public NSConstants.Azure.JobType JobType { get; set; }
        public int JobId { get; set; }
    }
}
