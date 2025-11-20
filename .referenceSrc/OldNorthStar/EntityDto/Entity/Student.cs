using EntityDto.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.Entity
{
	public class Student : BaseEntity
	{
        public Student()
        {
            //InterventionAttendances = new HashSet<InterventionAttendance>();
            StudentSections = new HashSet<StudentSection>();
            //StudentEducationServices = new HashSet<StudentEducationService>();
            StudentSchools = new HashSet<StudentSchool>();
            StudentNotes = new HashSet<StudentNote>();

            StudentAttributeDatas = new HashSet<StudentAttributeData>();
            //StudentSpecialEducationLabels = new HashSet<StudentSpecialEducationLabel>();
            //TeamMeetingStudents = new HashSet<TeamMeetingStudent>();
            //TeamMeetingStudentNotes = new HashSet<TeamMeetingStudentNote>();
        }
        public string FirstName { get; set; }
		public string MiddleName { get; set; }
		public string LastName { get; set; }
		public DateTime? DOB { get; set; }
		public int? GradYear { get; set; }
		public string StudentIdentifier { get; set; }
		public int? TitleOnetypeID { get; set; }
		public string Comment { get; set; }
        //public string Pronunciation { get; set; }
        public int? EthnicityID { get; set; }
		public string Gender { get; set; }
		public bool? IsActive { get; set; }
		public int? GenderID { get; set; }
		public int? DistrictID { get; set; }
		public bool? ELL { get; set; }
		public bool? ADSIS { get; set; }
		public bool? Gifted { get; set; }
		public virtual ICollection<StudentSection> StudentSections { get; set; }
        public virtual ICollection<StudentAttributeData> StudentAttributeDatas { get; set; }
        //public virtual ICollection<InterventionAttendance> InterventionAttendances { get; set; }

        //public virtual ICollection<StudentEducationService> StudentEducationServices { get; set; }

        public virtual ICollection<StudentSchool> StudentSchools { get; set; }
        public virtual ICollection<StudentNote> StudentNotes { get; set; }

        //public virtual ICollection<StudentSpecialEducationLabel> StudentSpecialEducationLabels { get; set; }

        //public virtual ICollection<TeamMeetingStudent> TeamMeetingStudents { get; set; }

        //public virtual ICollection<TeamMeetingStudentNote> TeamMeetingStudentNotes { get; set; }

    }
}
