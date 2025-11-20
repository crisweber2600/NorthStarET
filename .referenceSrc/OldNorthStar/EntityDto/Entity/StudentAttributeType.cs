using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.Entity
{
    public class StudentAttributeType : BaseEntityNoTrack
    {
        public StudentAttributeType()
        {
            StudentAttributeDatas = new HashSet<StudentAttributeData>();
            LookupValues = new HashSet<StudentAttributeLookupValue>();
        }
        public string AttributeName { get; set; }
        public virtual ICollection<StudentAttributeData> StudentAttributeDatas { get; set; }
        public virtual ICollection<StudentAttributeLookupValue> LookupValues { get; set; }
    }
}
