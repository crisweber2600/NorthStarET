using EntityDto.DTO.Admin.InterventionToolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.DTO
{
    public class InputDto_AssociateVideoToIntervention
    {
        public OutputDto_DropdownData_VzaarVideo Video { get; set; }
        public int InterventionId { get; set; }
    }

    public class InputDto_RemoveVideoFromIntervention
    {
        public int VideoId { get; set; }
        public int InterventionId { get; set; }
    }

    public class InputDto_AssociateVideoToPage
    {
        public OutputDto_DropdownData_VzaarVideo Video { get; set; }
        public int PageId { get; set; }
    }

    public class InputDto_RemoveVideoFromPage
    {
        public int VideoId { get; set; }
        public int PageId { get; set; }
    }
}
