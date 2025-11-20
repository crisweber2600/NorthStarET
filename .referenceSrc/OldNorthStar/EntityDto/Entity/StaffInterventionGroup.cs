using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.Entity
{
    public class StaffInterventionGroup : BaseEntity
    {
        public int StaffID { get; set; }
        public int InterventionGroupId { get; set; }
        public int StaffHierarchyPermissionID { get; set; }

        public virtual InterventionGroup InterventionGroup { get; set; }
        public virtual Staff Staff { get; set; }
    }
}
