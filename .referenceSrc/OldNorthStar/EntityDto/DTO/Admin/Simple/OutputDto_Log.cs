using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Simple
{
    public class OutputDto_Log : OutputDto_Base
    {
        public OutputDto_Log()
        {
            LogItems = new List<string>();
        }

        public List<string> LogItems { get; set; }

    }

    public class AutomaticRolloverOutputDto_Log : OutputDto_Base
    {
        public AutomaticRolloverOutputDto_Log()
        {
            IntegrityErrors = new List<string>();
            HardStopErrors = new List<string>();
            DuplicationErrors = new List<string>();
            UserAccountErrors = new List<string>();
        }

        public List<string> IntegrityErrors { get; set; }
        public List<string> HardStopErrors { get; set; }
        public List<string> DuplicationErrors { get; set; }
        public List<string> UserAccountErrors { get; set; }
        public string OverallStatusMessage { get; set; }

    }
}
