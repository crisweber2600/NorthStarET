using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Section
{
    public class InputDto_GetSectionList
    {
        public int SchoolId { get; set; }
        public int GradeId { get; set; }
        public int TeacherId { get; set; }
        public int SchoolYear { get; set; }
    }
}
