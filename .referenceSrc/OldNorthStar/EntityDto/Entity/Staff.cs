using EntityDto.Entity;
using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.Entity
{
	public class Staff : BaseEntity
	{
        public Staff()
        {
            //InterventionAttendances = new HashSet<InterventionAttendance>();
            Sections = new HashSet<Section>();
            //StaffCalendars = new HashSet<StaffCalendar>();
            StaffSections = new HashSet<StaffSection>();
            StaffSchools = new HashSet<StaffSchool>();
            AttendeeGroups = new HashSet<AttendeeGroup>();
            StaffSchoolGrades = new HashSet<StaffSchoolGrade>();
            StaffObservationSummaryAssessments = new List<StaffObservationSummaryAssessment>();
            StaffObservationSummaryAssessmentFields = new List<StaffObservationSummaryAssessmentField>();
            //StaffTests = new HashSet<StaffTest>();
            //StaffTimeSlots = new HashSet<StaffTimeSlot>();
            //TeamMeetings = new HashSet<TeamMeeting>();
            //TeamMeetingAttendances = new HashSet<TeamMeetingAttendance>();
            //TeamMeetingStudents = new HashSet<TeamMeetingStudent>();
            //TeamMeetingStudentNotes = new HashSet<TeamMeetingStudentNote>();
        }
        public int DistrictId { get; set; }
        public string TeacherIdentifier { get; set; }
		public string LoweredUserName { get; set; }
		public string FirstName { get; set; }
		public string MiddleName { get; set; }
		public string LastName { get; set; }
		public int? NorthStarUserTypeID { get; set; }
		public string Notes { get; set; }
		public int RoleID { get; set; }
		public string Email { get; set; }
		public bool IsInterventionSpecialist { get; set; }
		public string NavigationFavorites { get; set; }
		public DateTime? RolesLastUpdated { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsSA { get; set; }
        public bool? IsPowerUser { get; set; }

        public string FullName { get { return this.LastName + ", " + this.FirstName; } }

        //public virtual ICollection<InterventionAttendance> InterventionAttendances { get; set; }

        //public virtual NorthStarUserType NorthStarUserType { get; set; }

        public virtual ICollection<Section> Sections { get; set; }

        public bool IsDistrictAdmin { get; set; }
        public bool? IsDistrictContact { get; set; }

        //public virtual StaffRole StaffRole { get; set; }

        //public virtual ICollection<StaffCalendar> StaffCalendars { get; set; }

        public virtual ICollection<StaffSection> StaffSections { get; set; }

        public virtual ICollection<StaffSchool> StaffSchools { get; set; }

        public virtual ICollection<StaffSchoolGrade> StaffSchoolGrades { get; set; }

        public virtual ICollection<AttendeeGroup> AttendeeGroups { get; set; }

        public virtual ICollection<InterventionAttendance> InterventionAttendances { get; set; }

        public virtual ICollection<StaffObservationSummaryAssessment> StaffObservationSummaryAssessments { get; set; }
        public virtual ICollection<StaffObservationSummaryAssessmentField> StaffObservationSummaryAssessmentFields { get; set; }

        //public virtual ICollection<StaffTest> StaffTests { get; set; }

        //public virtual ICollection<StaffTimeSlot> StaffTimeSlots { get; set; }

        //public virtual ICollection<TeamMeeting> TeamMeetings { get; set; }

        //public virtual ICollection<TeamMeetingAttendance> TeamMeetingAttendances { get; set; }

        //public virtual ICollection<TeamMeetingStudent> TeamMeetingStudents { get; set; }

        //public virtual ICollection<TeamMeetingStudentNote> TeamMeetingStudentNotes { get; set; }
    }
}
