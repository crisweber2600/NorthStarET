using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NorthStar4.PCL.Entity;
using EntityDto.DTO.Admin.Simple;

namespace NorthStar4.PCL.DTO
{
	public class OuputDto_ManageInterventionGroup : OutputDto_Base
    {
		public OuputDto_ManageInterventionGroup()
		{
			CoTeachers = new List<OutputDto_DropdownData>();
            StartTime = DateTime.Now;
            EndTime = DateTime.Now;
            StudentInterventionGroups = new List<InterventionGroupStudentDto>();
		}

        public int? Tier { get; set; }
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public int PrimaryTeacherStaffID { get; set; }
		public int SchoolStartYear { get; set; }
		public int SchoolID { get; set; }
		public int? InterventionTypeID { get; set; }
		public bool IsInterventionGroup { get; set; }
		public bool? MondayMeet { get; set; }
		public bool? TuesdayMeet { get; set; }
		public bool? WednesdayMeet { get; set; }
		public bool? ThursdayMeet { get; set; }
		public bool? FridayMeet { get; set; }
        public int NumStudents { get; set; }
        public int ActiveStudents { get; set; }
        public string DaysMet
        {
            get
            {
                List<string> arr = new List<string>();

                if(MondayMeet.HasValue && MondayMeet.Value) 
                    arr.Add("M");
                if (TuesdayMeet.HasValue && TuesdayMeet.Value)
                    arr.Add("Tu");
                if (WednesdayMeet.HasValue && WednesdayMeet.Value)
                    arr.Add("W");
                if (ThursdayMeet.HasValue && ThursdayMeet.Value)
                    arr.Add("Th");
                if (FridayMeet.HasValue && FridayMeet.Value)
                    arr.Add("F");

                return String.Join(", ", arr);
            }
        }

		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }
		public SchoolYear SchoolYear { get; set; }
		public School School { get; set; }
		public OutputDto_DropdownData Intervention { get; set; }
		public OutputDto_DropdownData PrimaryTeacher { get; set; }
		public List<OutputDto_DropdownData> CoTeachers { get; set; }
        public List<InterventionGroupStudentDto> StudentInterventionGroups { get; set; }
    }
}
