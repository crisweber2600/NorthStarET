using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Northstar.Core
{
    public class UserDisplayableException : Exception
    {
        public string EndUserMessage { get; set; }
        public UserDisplayableException(string message, Exception ex) : base(message, ex)
        {
            EndUserMessage = message;
        }
    }
}
