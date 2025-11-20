using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NorthStar4.CrossPlatform.DTO.Admin.Staff
{
    public class Dto_StaffSchoolGrade : BaseEntityNoTrack
    {
        public int SchoolID { get; set; }
        public int StaffID { get; set; }
        public int GradeID { get; set; }
        public int StaffHierarchyPermissionID { get; set; }
    }
}
