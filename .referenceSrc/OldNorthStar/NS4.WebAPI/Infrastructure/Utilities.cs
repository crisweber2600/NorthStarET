using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NorthStar4.API.Infrastructure
{
    public static class Utilities
    {
        public static string GetUserEmail(ClaimsIdentity user)
        {
            return user.Claims.Single(x => x.Type == "preferred_username").Value;
        }
    }
}
