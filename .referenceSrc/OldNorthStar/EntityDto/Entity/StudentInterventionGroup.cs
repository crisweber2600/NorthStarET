using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.Entity
{
    public class StudentInterventionGroup : BaseEntity
    {
        public StudentInterventionGroup()
        {
            //InterventionAttendances = new HashSet<InterventionAttendance>();
        }

        public int StudentID { get; set; }
        public int InterventionGroupId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? LastAssociatedTDDID { get; set; }
        public string Notes { get; set; }
        public DateTime? LastAssociatedTDD { get; set; }
        public Student Student { get; set; }
        public InterventionGroup InterventionGroup { get; set; }

        //public virtual ICollection<InterventionAttendance> InterventionAttendances { get; set; }
    }
}
