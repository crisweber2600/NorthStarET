using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.TeamMeeting
{
    public class InterventionsByStudent
    {
        public InterventionsByStudent()
        {
            InterventionsBySchoolYear = new List<InterventionsBySchoolYear>();
        }

        public int StudentId { get; set; }
        public List<InterventionsBySchoolYear> InterventionsBySchoolYear { get; set; }
    }
}
