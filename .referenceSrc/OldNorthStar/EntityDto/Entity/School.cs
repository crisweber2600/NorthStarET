using EntityDto.Entity;
using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.Entity
{
	public class School : BaseEntity
	{
        public School()
        {
            Sections = new HashSet<Section>();
            InterventionGroups = new HashSet<InterventionGroup>();
            SchoolCalendars = new HashSet<SchoolCalendar>();
            //SchoolTests = new HashSet<SchoolTest>();
            StaffSchools = new HashSet<StaffSchool>();
            StudentSchools = new HashSet<StudentSchool>();

            //TeamMeetingAttendances = new HashSet<TeamMeetingAttendance>();
            //TeamMeetingStudents = new HashSet<TeamMeetingStudent>();
        }

        public bool? IsPreK { get; set; }
        public bool? IsK2 { get; set; }
        public bool? Is35 { get; set; }
        public bool? IsK5 { get; set; }
        public bool? IsK8 { get; set; }
        public bool? IsMS { get; set; }
        public bool? IsHS { get; set; }
        public bool? IsSS { get; set; }

        public string Name { get; set; }
		public string Description { get; set; }

        public virtual ICollection<Section> Sections { get; set; }
        public virtual ICollection<InterventionGroup> InterventionGroups { get; set; }

        public virtual ICollection<SchoolCalendar> SchoolCalendars { get; set; }

        //public virtual ICollection<SchoolTest> SchoolTests { get; set; }

        public virtual ICollection<StaffSchool> StaffSchools { get; set; }

        public virtual ICollection<StudentSchool> StudentSchools { get; set; }

        //public virtual ICollection<TeamMeetingAttendance> TeamMeetingAttendances { get; set; }

        //public virtual ICollection<TeamMeetingStudent> TeamMeetingStudents { get; set; }
    }
}
