using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NorthStar4.PCL.Entity;

namespace NorthStar4.PCL.DTO
{
	public class StudentDetailedQuickSearchResult
	{

        public int id { get; set; }
        public int StudentId { get; set; }
        public string StudentIdentifier { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public DateTime DOB { get; set; }
        public bool IsActive { get; set; }
        public bool disabled { get; set; }
        public int SectionId { get; set; }
        public int SchoolId { get; set; }
        public string SectionName { get; set; }
        public string SchoolName { get; set; }
        public int SchoolStartYear { get; set; }
        public string SchoolYearVerbose { get; set; }
        public int StaffId { get; set; }
        public string StaffName { get; set; }
        public int GradeId { get; set; }
        public string GradeName { get; set; }
        public string text { get { return LastName; } }
 
    }

    public class StudentSectionRegistration
    {
        public int SectionId { get; set; }
        public int SchoolId { get; set; }
        public string SectionName { get; set; }
        public string SchoolName { get; set; }
        public int SchoolStartYear { get; set; }
        public string SchoolYearVerbose { get; set; }
        public int StaffId { get; set; }
        public string StaffName { get; set; }
        public int GradeId { get; set; }
        public string GradeName { get; set; }
    }
}
