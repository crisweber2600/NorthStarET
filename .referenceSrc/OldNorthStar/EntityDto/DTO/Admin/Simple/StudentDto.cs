using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Simple
{
    public class StudentDto : BaseEntityNoTrack
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateTime? DOB { get; set; }
        public int? GradYear { get; set; }
        public string StudentIdentifier { get; set; }
        public int? TitleOnetypeID { get; set; }
        public string Comment { get; set; }
        public int? EthnicityID { get; set; }
        public string Gender { get; set; }
        public bool? IsActive { get; set; }
        public int? GenderID { get; set; }
        public int? DistrictID { get; set; }
        public bool? ELL { get; set; }
        public bool? ADSIS { get; set; }
        public bool? Gifted { get; set; }
    }
}
