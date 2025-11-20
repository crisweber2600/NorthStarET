using NorthStar4.CrossPlatform.Entity;
using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.Entity
{
    public class StudentNote : BaseEntityNoTrack
    {
        public int StudentID { get; set; }

        public DateTime NoteDate { get; set; }

        public string Note { get; set; }

        public int StaffID { get; set; }

        public virtual Staff Staff { get; set; }

        public virtual Student Student { get; set; }

    }
}
