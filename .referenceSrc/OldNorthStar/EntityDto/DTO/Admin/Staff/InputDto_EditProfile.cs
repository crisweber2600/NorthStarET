using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NorthStar4.CrossPlatform.DTO.Admin.Staff
{
    public class InputDto_EditProfile
    {
        public string TeacherIdentifier { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public int RoleID { get; set; }
        public bool IsInterventionSpecialist { get; set; }
    }

    public class InputDto_SaveLoginCookie
    {
        public string UserName { get; set; }
        public bool SaveCookie { get; set; }
    }
}
