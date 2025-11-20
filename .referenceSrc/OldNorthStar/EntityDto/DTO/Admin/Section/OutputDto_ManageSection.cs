using EntityDto.DTO.Admin.Simple;
using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Section
{
    public class OutputDto_ManageSection : OutputDto_Base
    {
        public int SchoolYear { get; set; }
        public int SchoolId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int GradeId { get; set; }
        public OutputDto_DropdownData PrimaryTeacher { get; set; }
        public List<OutputDto_DropdownData> CoTeachers { get; set; }
        public List<OutputDto_DropdownData> Students { get; set; }
    }
}
