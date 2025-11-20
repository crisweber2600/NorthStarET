using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Simple
{
    public class StudentSchoolDto : BaseEntityNoTrack
    {
        public int SchoolId { get; set; }
        public int StudentId { get; set; }
        public int SchoolStartYear { get; set; }
        public string SchoolName { get; set; }
        public string SchoolYearLabel { get; set; }
        public int? GradeId { get; set; }
        public string GradeName { get; set; }
    }
    public class InputDto_StudentSchoolDto
    {
        public StudentSchoolDto StudentSchool { get; set; }
    }
}
