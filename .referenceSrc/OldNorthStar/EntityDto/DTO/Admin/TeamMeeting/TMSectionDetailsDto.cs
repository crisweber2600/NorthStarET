using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.TeamMeeting
{
    public class TMSectionDetailsDto : BaseEntityNoTrack
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int StaffID { get; set; }
        public string StaffFullName { get; set; }
        public string StaffLastName { get; set; }
        public int SchoolStartYear { get; set; }
        public int SchoolID { get; set; }
        public int GradeID { get; set; }
        public string Grade { get; set; }
        public int NumStudents { get; set; }
    }
}
