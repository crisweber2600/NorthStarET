using NorthStar.EF6;
using NorthStar4.API.Infrastructure;
using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace NorthStar4.API.api
{
    [RoutePrefix("api/Video")]
    [Authorize]
    public class VideoController : NSBaseController
    {

        [Route("GetVzaarVideoList")]
        [HttpPost]
        public IHttpActionResult GetVzaarVideoList([FromBody] InputDto_SimpleId input)  // pagenumber
        {
            var dataService = new VideoDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetVzaarVideos(input, VzaarSecret, VzaarToken);

            return ProcessResultStatus(result);
        }

        [Route("GetPagedVideoList")]
        [HttpPost]
        public IHttpActionResult GetPagedVideoList([FromBody] InputDto_SimpleId input)  // pagenumber
        {
            var dataService = new VideoDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetPagedVideoList(input, VzaarSecret, VzaarToken);

            return ProcessResultStatus(result);
        }

        [Route("GetDistrictVideos")]
        [HttpPost]
        public IHttpActionResult GetDistrictVideos([FromBody] InputDto_SimpleNullableId input)  // gradeid
        {
            var dataService = new VideoDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetDistrictVideos(input.Id);

            return ProcessResultStatus(result);
        }

        [Route("GetAllVideos")]
        [HttpGet]
        public IHttpActionResult GetAllVideos()  // gradeid
        {
            var dataService = new VideoDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetAllVideos();

            return ProcessResultStatus(result);
        }

        [Route("RemoveVideo")]
        [HttpPost]
        public IHttpActionResult RemoveVideo([FromBody]InputDto_SimpleId input)  // gradeid
        {
            var dataService = new VideoDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.RemoveVideo(input);

            return ProcessResultStatus(result);
        }
    }
}
