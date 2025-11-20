using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.InterventionToolkit
{
    public class InputDto_InterventionSearch
    {
        public int TierId { get; set; }
        public int GradeId { get; set; }
        public int WorkshopId { get; set; }
        public int CategoryId { get; set; }
    }
}
