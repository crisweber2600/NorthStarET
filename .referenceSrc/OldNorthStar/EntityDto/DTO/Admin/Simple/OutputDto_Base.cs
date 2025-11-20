using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Simple
{
    public class OutputDto_Base
    {
        public OutputDto_Base()
        {
            Status = new OutputDto_Status();
            Status.StatusCode = StatusCode.Ok; // OK until proven otherwise
        }

        public OutputDto_Status Status { get; set; }

    }
}
