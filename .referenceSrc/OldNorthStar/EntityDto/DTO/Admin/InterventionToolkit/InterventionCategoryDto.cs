using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.InterventionToolkit
{
    public class InterventionCategoryDto : BaseEntityNoTrack
    {
        public InterventionCategoryDto()
        {
        }

        public string CategoryName { get; set; }
        public string CategoryDescription { get; set; }
        public Nullable<int> SortOrder { get; set; }

    }
}
