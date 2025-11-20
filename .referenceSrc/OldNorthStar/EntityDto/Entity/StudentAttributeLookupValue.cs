using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.Entity
{
    public class StudentAttributeLookupValue : BaseEntityNoTrack
    {
        public StudentAttributeLookupValue()
        {
            StudentAttributeDatas = new HashSet<StudentAttributeData>();
        }
        public int AttributeID { get; set; }
        public int? LookupValueID { get; set; }
        public string LookupValue { get; set; }
        public string Description { get; set; }
        public bool? IsSpecialEd { get; set; }
        public bool? IsDefaultOption { get; set; }
        public bool? NotGenEd { get; set; }

        public virtual StudentAttributeType AttributeType { get; set; }
        public virtual ICollection<StudentAttributeData> StudentAttributeDatas { get; set; }
    }
}
