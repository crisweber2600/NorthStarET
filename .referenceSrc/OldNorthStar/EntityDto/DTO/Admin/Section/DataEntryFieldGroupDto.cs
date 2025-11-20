using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Section
{
    public class DataEntryFieldGroupDto : BaseEntityNoTrack
    {
        public DataEntryFieldGroupDto()
        {
            Categories = new List<DataEntryFieldCategoryDto>();
        }
        public string N { get; set; }
        public int O { get; set; }
        public List<DataEntryFieldCategoryDto> Categories {get;set;}
    }
}
