using EntityDto.DTO.Admin.InterventionToolkit;
using EntityDto.DTO.Admin.Simple;
using EntityDto.LoginDB.Entity;
using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.InterventionToolkit
{
    public class OutputDto_GetTiers : OutputDto_Base
    {
        public List<InterventionTierDto> Tiers { get; set; }
    }
}
