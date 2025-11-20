using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NorthStar.EF6
{
    public class ClassRosterDataService : NSBaseDataService
    {
        public ClassRosterDataService(ClaimsIdentity user, string loginConnectionString) : base(user, loginConnectionString)
        {

        }
    }
}
