using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Simple
{
    public class OutputDto_Status
    {
        // status message will be false for an exception or access denied
        public StatusCode StatusCode { get; set; }

        // assume that if a status message is present, it is user-displayable. otherwise, it will just be logged
        // status message may be "access denied", "blank", or "some message".
        public string StatusMessage { get; set; }
    }

    public enum StatusCode { Ok, AccessDenied, UserDisplayableException, UnhandledException };

}
