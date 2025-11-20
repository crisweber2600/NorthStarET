using NorthStar4.CrossPlatform.Entity;
using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NorthStar4.CrossPlatform.DTO.Admin.Staff
{
    public class Dto_StaffSchool : BaseEntityNoTrack
    {
        public Dto_StaffSchool()
        {
            Grades = new List<OutputDto_DropdownData>();
        }

        public int SchoolID { get; set; }
        public int StaffID { get; set; }
        public string SchoolName { get; set; }
        public int StaffHierarchyPermissionID { get; set; }
        public bool? IsSchoolContact { get; set; }

        public List<OutputDto_DropdownData> Grades { get; set; }
    }
}
