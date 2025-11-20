using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Northstar.Core.Identity
{
    public class NorthStarIdentityResult
    {
        public static readonly NorthStarIdentityResult Success = new NorthStarIdentityResult();

        public NorthStarIdentityResult()
        {
        }

        public NorthStarIdentityResult(params string[] errors)
        {
            this.Errors = errors;
        }

        public IEnumerable<string> Errors { get; private set; }

        public bool IsSuccess
        {
            get { return Errors == null || !Errors.Any(); }
        }
    }

    public class NorthStarIdentityResult<T> : NorthStarIdentityResult
    {
        public NorthStarIdentityResult(T result)
        {
            Result = result;
        }

        public NorthStarIdentityResult(params string[] errors)
            : base(errors)
        {
        }

        public T Result { get; private set; }
    }
}
