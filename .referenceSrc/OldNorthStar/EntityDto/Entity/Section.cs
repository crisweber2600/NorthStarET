using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.Entity
{
	public class Section : BaseEntity
	{
        public Section()
        {
            StudentSections = new HashSet<StudentSection>();
            StaffSections = new HashSet<StaffSection>();
        }

        public string Name { get; set; }
		public string Description { get; set; }
		public int StaffID { get; set; }
		public int SchoolStartYear { get; set; }
		public int SchoolID { get; set; }
		public int GradeID { get; set; }
		public int SectionDataTypeID { get; set; }
		public int? InterventionTypeID { get; set; }
		public int? InterventionTierID { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public int? StaffTimeSlotID { get; set; }
		public bool IsInterventionGroup { get; set; }
		public int? InterventionDistrictID { get; set; }
		public bool? MondayMeet { get; set; }
		public bool? TuesdayMeet { get; set; }
		public bool? WednesdayMeet { get; set; }
		public bool? ThursdayMeet { get; set; }
		public bool? FridayMeet { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }
		public virtual SchoolYear SchoolYear { get; set; }
		public virtual School School { get; set; }
		public virtual Grade Grade { get; set; }
		public virtual ICollection<StudentSection> StudentSections { get; set; }
		public virtual ICollection<StaffSection> StaffSections { get; set; } 
		public virtual Staff Staff { get; set; }
        public virtual Intervention InterventionType { get; set; }
    }
}
