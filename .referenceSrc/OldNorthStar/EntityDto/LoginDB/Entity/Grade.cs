using EntityDto.Entity;
using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntityDto.LoginDB.Entity
{
	public class NSGrade : BaseEntityNoTrack
	{
        public NSGrade()
        {

        }
        public string ShortName { get; set; }
		public string LongName { get; set; }
		public int GradeOrder { get; set; }

    }
}
