using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthStar.EF6
{
    public static class LoginContextFactory
    {
        public static LoginContext GetContext(string cnx)
        {
            //var cnx = ConfigurationManager.ConnectionStrings["LoginConnectionString"].ConnectionString;
            return new LoginContext(cnx);
        }
    }
}
