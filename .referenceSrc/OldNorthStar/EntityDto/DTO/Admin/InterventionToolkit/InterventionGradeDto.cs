using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.InterventionToolkit
{
    public class InterventionGradeDto : DtoBase
    {
        public int InterventionTypeId { get; set; }
        public int GradeID { get; set; }
        public string GradeName { get; set; }
    }

    public class DtoBase
    {
        public int id { get; set; }
    }
}
