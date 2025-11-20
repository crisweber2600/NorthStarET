using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Simple
{
    public class StaffDto : BaseEntityNoTrack
    {
        public string TeacherIdentifier { get; set; }
        public string LoweredUserName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public int? NorthStarUserTypeID { get; set; }
        public string Notes { get; set; }
        public int RoleID { get; set; }
        public int DistrictId { get; set; }
        public string DistrictName { get; set; }
        public string Email { get; set; }
        public bool IsInterventionSpecialist { get; set; }
        public string NavigationFavorites { get; set; }
        public DateTime? RolesLastUpdated { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsSA { get; set; }
        public bool? IsPowerUser { get; set; }
        public string FullName { get; set; }
        public bool IsDistrictAdmin {get;set;}
        public bool? IsDistrictContact { get; set; }
        public bool IsSchoolAdmin { get; set; }
        public string AccessLevel { get; set; }
        public bool CanModify { get; set; }
    }
}
