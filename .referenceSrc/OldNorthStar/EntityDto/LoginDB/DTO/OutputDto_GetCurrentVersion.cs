using EntityDto.DTO.Admin.Simple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.DTO
{
    public class OutputDto_GetUserCurrentVersion : OutputDto_Base
    {
        public DateTime? VersionLastUpdated { get; set; }
        public decimal? CurrentNSVersion { get; set; }
        public decimal? UserCurrentVersion { get; set; }
    }
}
