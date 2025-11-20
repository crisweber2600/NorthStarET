using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.DTO
{
    public class InputDto_AssociateToolToIntervention
    {
        public int InterventionToolId { get; set; }
        public int InterventionId { get; set; }
    }

    public class InputDto_AssociateToolToPage
    {
        public int ToolId { get; set; }
        public int PageId { get; set; }
    }

    public class InputDto_AssociatePresentationToPage
    {
        public int PresentationId { get; set; }
        public int PageId { get; set; }
    }
}
