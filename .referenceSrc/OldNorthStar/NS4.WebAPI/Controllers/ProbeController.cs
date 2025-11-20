using NorthStar4.API.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace NS4.WebAPI.Controllers
{
    [RoutePrefix("api/Probe")]
    public class ProbeController : NSBaseController
    {
        [Route("getStatus")]
        [HttpGet]
        public IHttpActionResult ProbeResult()
        {
            return StatusCode(HttpStatusCode.OK);
        }

    }
}
