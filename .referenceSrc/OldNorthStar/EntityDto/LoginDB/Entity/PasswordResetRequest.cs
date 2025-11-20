using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.Entity
{
    public class PasswordResetRequest : BaseEntityNoTrack
    {
        public DateTime ResetRequestDateStamp { get; set; }
        public string UserName { get; set; }
        public Guid UID { get; set; }
    }
}
