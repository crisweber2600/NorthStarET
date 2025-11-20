using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Simple
{
    public class OutputDto_SuccessAndNewId : OutputDto_Base
    {
        public bool isValid { get; set; }
        public string value { get; set; }
        public int id { get; set; }
    }
}
