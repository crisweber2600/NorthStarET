using EntityDto.DTO.Admin.Simple;
using EntityDto.LoginDB.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Video
{
    public class OutputDto_VzaarVideoList : OutputDto_Base
    {
        public List<com.vzaar.api.Video> Videos { get; set; }
    }

    public class OutputDto_NSVideoList : OutputDto_Base
    {
        public List<NSInterventionVideoDto> Videos { get; set; }
    }

}
