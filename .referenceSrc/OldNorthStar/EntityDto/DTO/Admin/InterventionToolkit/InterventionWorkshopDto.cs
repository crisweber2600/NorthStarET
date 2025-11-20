using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.InterventionToolkit
{
    public class InterventionWorkshopDto : BaseEntityNoTrack
    {
        public InterventionWorkshopDto()
        {
        }

        public string WorkshopName { get; set; }
        public string WorkshopDescription { get; set; }

    }
}
