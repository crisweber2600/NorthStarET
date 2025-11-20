using EntityDto.DTO.Admin.Simple;
using EntityDto.DTO.Video;
using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using VzaarApi;
using System.Diagnostics;
using EntityDto.DTO.Admin.InterventionToolkit;
using AutoMapper;
using EntityDto.LoginDB.DTO;
using System.Data;
using System.Data.Entity;
using com.vzaar.api;
using System.Net;

namespace NorthStar.EF6
{
    public class VideoDataService : NSBaseDataService
    {
        public VideoDataService(ClaimsIdentity user, string loginConnectionString) : base(user, loginConnectionString)
        {

        }

        public async Task<List<OutputDto_DropdownData_VzaarVideo>> QuickSearchVideos(string searchString, string secret, string token)
        {

            Client.client_id = "fluff-idea-vale";
            Client.auth_token = "UGuApKT_bWUiUsyrx5ks";
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Client.url = "https://api.vzaar.com/api/";


            //Client.version = "v2";
            //Client.urlAuth = true;
            //Client client = new Client() { CfgClientId = "fluff-idea-vale", CfgAuthToken = "UGuApKT_bWUiUsyrx5ks" };
            Dictionary<string, string> query = new Dictionary<string, string>() {
                    {"sort","title"},
                    {"order", "asc"},
                    {"per_page", "100"},
                    {"q", "title:" + searchString }
                };

            var videosList = new List<OutputDto_DropdownData_VzaarVideo>();

            foreach (var item in VideosList.EachItem(query))
            {
                videosList.Add(Mapper.Map<OutputDto_DropdownData_VzaarVideo>(item));
            }
            //var api = new Vzaar(secret, token);
            //var query = new VideoListQuery();
            //query.count = 100;
            //query.page = 1;
            //query.sort = VideoListSorting.ASCENDING;
            //query.title = searchString;
            //var list = api.getVideoList(query);
            return videosList;
        }


        public OutputDto_VzaarVideoList GetVzaarVideos(InputDto_SimpleId input, string secret, string token)
        {
            var api = new Vzaar(secret, token);

            Debug.WriteLine(api.whoAmI());

            var query = new VideoListQuery();
            query.count = 10;
            query.page = 1;
            var list = api.getVideoList(query);
            //Video b;
            return new OutputDto_VzaarVideoList
            {
                Videos = list
            };
        }

        public OutputDto_VzaarVideoList GetPagedVideoList(InputDto_SimpleId input, string secret, string token)
        {
            var api = new Vzaar(secret, token);

            Debug.WriteLine(api.whoAmI());

            var details = api.getUserDetails(secret);

            var numVideos = details.videoCount;

            var query = new VideoListQuery();
            query.count = 10;
            query.page = 1;
            //query.title
            var list = api.getVideoList(query);
            var vid = list.First();
            //vid.
            return new OutputDto_VzaarVideoList
            {
                Videos = list
            };
        }

        public OutputDto_NSVideoList GetDistrictVideos(int? gradeId)
        {
            // removing district restriction per beth 7/20/2019
            //var videos = _loginContext.NSInterventionVideoDistricts.Where(p => p.DistrictId == _currentUser.DistrictId && (p.InterventionVideo.InterventionVideoGrades.Any(j => j.GradeId == gradeId) || gradeId == null)).Select(p => p.InterventionVideo).Include(p => p.InterventionVideoGrades);
            var videos = _loginContext.NSInterventionVideos.Where(p => (p.InterventionVideoGrades.Any(j => j.GradeId == gradeId) || gradeId == null)).Include(p => p.InterventionVideoGrades);
            return new OutputDto_NSVideoList { Videos = Mapper.Map<List<NSInterventionVideoDto>>(videos) };
        }

        // TODO: SECURITY CHECKS --> NS Admin ONLY
        public OutputDto_NSVideoList GetAllVideos()
        {
            var videos = _loginContext.NSInterventionVideos.ToList().OrderBy(p => p.VideoName);

            var result = new OutputDto_NSVideoList { Videos = Mapper.Map<List<NSInterventionVideoDto>>(videos) };

            // only return district list if a sysadmin is logged in
            if (IsSysAdmin)
            {
                return result;
            } else
            {
                var district = _loginContext.Districts.First(p => p.Id == _currentUser.DistrictId);
                foreach(var video in result.Videos)
                {
                    video.Districts = new List<OutputDto_DropdownData>() { new OutputDto_DropdownData { id = district.Id, text = district.Name } };
                }
                return result;
            }
        }

        public OutputDto_Base RemoveVideo(InputDto_SimpleId input)
        {
            var result = new OutputDto_Base();

            // remove districts
            var videoDistrictsToRemove = _loginContext.NSInterventionVideoDistricts.Where(p => p.InterventionVideoId == input.Id);
            _loginContext.NSInterventionVideoDistricts.RemoveRange(videoDistrictsToRemove);

            // remove grades
            var videoGradesToRemove = _loginContext.NSInterventionVideoGrades.Where(p => p.InterventionVideoId == input.Id);
            _loginContext.NSInterventionVideoGrades.RemoveRange(videoGradesToRemove);


            // remove intervention references
            var interventionReferencesToRemove = _loginContext.NSInterventionVideoNSInterventions.Where(p => p.InterventionVideoId == input.Id);
            _loginContext.NSInterventionVideoNSInterventions.RemoveRange(interventionReferencesToRemove);

            var videoToRemove = _loginContext.NSInterventionVideos.First(p => p.Id == input.Id);
            _loginContext.NSInterventionVideos.Remove(videoToRemove);

            _loginContext.SaveChanges();

            return result;
        }
    }
}
