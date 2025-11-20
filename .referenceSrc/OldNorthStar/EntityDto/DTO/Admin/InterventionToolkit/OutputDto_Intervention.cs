using EntityDto.DTO.Admin.Simple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.InterventionToolkit
{
    public class OutputDto_Intervention : OutputDto_Base
    {
        public InterventionDto Intervention { get; set; }
    }
}
