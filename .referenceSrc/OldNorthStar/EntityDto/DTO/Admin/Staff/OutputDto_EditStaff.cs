using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NorthStar4.CrossPlatform.DTO.Admin.Staff
{
    public class OutputDto_EditStaff
    {
        public OutputDto_EditStaff()
        {
            StaffSchools = new List<Dto_StaffSchool>();
        }

        public int Id { get; set; }
        public List<Dto_StaffSchool> StaffSchools { get; set; }
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
        public string OriginalUserName { get; set; }
        public bool? IsActive { get; set; }
        public bool? CanLogIn { get; set; }
        public bool IsDistrictAdmin { get; set; }
        public bool? IsDistrictContact { get; set; }
    }
}
