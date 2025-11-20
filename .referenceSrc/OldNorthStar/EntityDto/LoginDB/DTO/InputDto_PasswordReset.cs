using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.DTO
{
    public class InputDto_PasswordReset
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public Guid UID { get; set; }
    }
}
