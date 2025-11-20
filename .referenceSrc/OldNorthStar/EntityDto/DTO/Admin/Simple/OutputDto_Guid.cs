using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Simple
{
    public class OutputDto_Guid : OutputDto_Base
    {
        public Guid Guid { get; set; }
    }

    public class OutputDto_Bool : OutputDto_Base
    {
        public bool? Value { get; set; }
    }
}
