using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Simple
{
    public class InputDto_SendMail
    {
        public int ToId { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }
}
