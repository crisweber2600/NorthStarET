using EntityDto.DTO.Admin.Simple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Navigation
{
    public class InputOutputDto_NSHelp : OutputDto_Base
    {
        public string path { get; set; }
        public string field { get; set; }
        public string text { get; set; }
    }
}
