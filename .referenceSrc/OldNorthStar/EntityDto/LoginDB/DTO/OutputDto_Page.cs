using EntityDto.DTO.Admin.Simple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.InterventionToolkit
{
    public class OutputDto_Page : OutputDto_Base
    {
        public PageDto NSPage { get; set; }
    }

    public class OutputDto_PagesList : OutputDto_Base
    {
        public List<PageDto> Pages { get; set; }
    }
}
