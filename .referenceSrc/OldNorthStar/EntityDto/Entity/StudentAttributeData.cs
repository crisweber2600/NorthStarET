using NorthStar4.CrossPlatform.Entity;
using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.Entity
{
    public class StudentAttributeData : BaseEntityNoTrack
    {
        public int StudentID { get; set; }
        public int AttributeID { get; set; }
        public int AttributeValueID { get; set; }

        public virtual Student Student { get; set; }
        public virtual StudentAttributeType AttributeType { get; set; }
        public virtual StudentAttributeLookupValue LookupValue { get; set; }
    }
}
