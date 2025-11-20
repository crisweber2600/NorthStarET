using NorthStar4.CrossPlatform.Entity;
using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.Entity
{
    public class StaffSetting : BaseEntity
    {
        public int StaffId { get; set; }
        public string SelectedValueId { get; set; }
        public string Attribute { get; set; }
        public Staff Staff { get; set; }
    }
}
