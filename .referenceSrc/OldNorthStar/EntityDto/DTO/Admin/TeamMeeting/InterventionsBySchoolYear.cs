using EntityDto.DTO.Admin.Simple;
using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.TeamMeeting
{
    public class InterventionsBySchoolYear
    {
        public InterventionsBySchoolYear()
        {
            InterventionList = new List<InterventionGroupStudentDto>();
        }

        public int SchoolYear { get; set; }
        public List<InterventionGroupStudentDto> InterventionList { get; set; }
    }
}
