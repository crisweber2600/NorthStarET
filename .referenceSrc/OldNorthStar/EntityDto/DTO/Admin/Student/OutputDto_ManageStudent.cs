using EntityDto.DTO.Admin.Simple;
using Newtonsoft.Json.Linq;
using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Student
{
    public class OutputDto_ManageStudent
    {
        public OutputDto_ManageStudent()
        {
            StudentAttributes = new JObject();
            StudentSchools = new List<StudentSchoolDto>();
        }
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateTime DOB { get; set; }
        public string DOBText { get; set; }
        public int? GraduationYear { get; set; }
        public string Pronunciation { get; set; }
        public string ImageUrl { get; set; }
        public string NewImageUrl { get; set; }
        public string EnrollmentYear { get; set; }
        public string StudentIdentifier { get; set; }
        public string Notes { get; set; } // TODO: make this a list
        public bool? IsActive { get; set; }
        public List<OutputDto_DropdownData> SpecialEdLabels { get; set; } // Attribute ID 4
        public JObject StudentAttributes { get; set; }

        public List<StudentSchoolDto> StudentSchools {get;set;}

    }
}
