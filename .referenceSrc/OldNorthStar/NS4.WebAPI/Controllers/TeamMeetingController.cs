using EntityDto.DTO.Admin.Section;
using EntityDto.DTO.Admin.Simple;
using EntityDto.DTO.Admin.TeamMeeting;
using EntityDto.Entity;
using System.Security.Claims;
using System.Web.Http;
using Northstar.Core;
using NorthStar.EF6;
using NorthStar4.API.Infrastructure;
using NorthStar4.PCL.DTO;
using SendGrid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Serilog;

namespace NorthStar4.API.api
{
    [RoutePrefix("api/TeamMeeting")]
    [Authorize]
    public class TeamMeetingController : NSBaseController
    {

        [Route("GetTeamMeetingList")]
        [HttpPost]
        public OutputDto_GetTeamMeetingList GetTeamMeetingList([FromBody] InputDto_GetTeamMeetingList input)
        {
            var dataService = new TeamMeetingDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetTeamMeetingList(input);

            return result;
        }

        [Route("GetAttendTeamMeetingList")]
        [HttpPost]
        public OutputDto_GetTeamMeetingList GetAttendTeamMeetingList([FromBody] InputDto_GetTeamMeetingList input)
        {
            var dataService = new TeamMeetingDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetAttendTeamMeetingList(input);

            return result;
        }


        [Route("getstaffgroupsfordropdown")]
        [HttpGet]
        public List<OutputDto_DropdownData> GetStaffGroupsForDropdown(int pageNo, string searchString)
        {
            var dataService = new TeamMeetingDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.QuickSearchTeachers(searchString);

            return result;

        }


        [Route("savenote")]
        [HttpPost]
        public OutputDto_Success SaveNote([FromBody] InputDto_SaveNote input)
        {
            var dataService = new TeamMeetingDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.SaveNote(input);

            return result;

        }
        [Route("deletenote")]
        [HttpPost]
        public OutputDto_Success DeleteNote([FromBody] InputDto_SimpleId input)
        {
            var dataService = new TeamMeetingDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.DeleteNote(input);

            return result;

        }
        [Route("getnotesforstudentteammeeting")]
        [HttpPost]
        public OutputDto_TeamMeetingStudentNotes GetNotesForStudentTeamMeeting([FromBody] InputDto_TeamMeetingStudent input)
        {
            var dataService = new TeamMeetingDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetNotesForStudentTeamMeeting(input);

            return result;

        }
        [Route("getnotesforstudentteammeetings")]
        [HttpPost]
        public IHttpActionResult GetNotesForStudentTeamMeetings([FromBody] InputDto_SimpleId input)
        {
            var dataService = new TeamMeetingDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetNotesForStudentTeamMeetings(input);

            return ProcessResultStatus(result);

        }

        public class InputDto_SendInvitation
        {
            public string TableHtml { get; set; }
        }
        [Route("sendinvitations")]
        [HttpPost]
        public async Task<IOutputDto_DataSaveStatus> SendInvitation([FromBody] InputDto_SendInvitation input)
        {
            
            // send the freaking email
            var html = input.TableHtml;

            var j = 1;

            //var message = new SendGridMessage();
            //message.AddTo("northstar.shannon@gmail.com"); // TODO: NEED TO PASS TARGET!!!!
            ////message.AddTo("northstar.shannon@gmail.com");
            //message.From = new MailAddress("support@northstaret.net");
            //message.Html = html;
            //message.Subject = "Your North Star Invitation";


            //var credentials = new System.Net.NetworkCredential("Hayaku77", "dammit77");
            //var transportWeb = new Web(credentials);
            //await transportWeb.DeliverAsync(message);
          

            return new IOutputDto_DataSaveStatus { msg = "yah", status = "cool" };

        }
        [Route("getstaffforattendeegroup/{Id:int}")]
        [HttpGet]
        public List<OutputDto_DropdownData> GetStaffForAttendeeGroup(int Id)
        {
            var dataService = new TeamMeetingDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetStaffForAttendeeGroup(Id);

            return result;

        }

        //[HttpPost("sendtminvitations")]
        //public IHttpActionResult SendTMInvitations(InputDto_SendTMInvitations input)
        //{
        //    var asmtDataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
        //    var lookupLists = asmtDataService.GetLookupFieldsForAssessments("1,3,8,9");
        //    var dataService = new TeamMeetingDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
        //    var result = dataService.SendTMInvitations(input, lookupLists);

        //    return ProcessResultStatus(result);
        //}
        [Route("sendtminvitation")]
        [HttpPost]
        public IHttpActionResult SendTMInvitations([FromBody]InputDto_SendTMInvitation input)
        {
            var asmtDataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var lookupLists = asmtDataService.GetLookupFieldsForAllAssessments();
            var dataService = new TeamMeetingDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.SendTMInvitation(input, lookupLists);

            return ProcessResultStatus(result);
        }
        [Route("sendtmconcluded")]
        [HttpPost]
        public IHttpActionResult SendTMConcluded([FromBody]InputDto_SendTMInvitation input)
        {
            //Log.Information("Inside TM Concluded CONTROLLER");
            var dataService = new TeamMeetingDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            //Log.Information("Created Data Service");
            var result = dataService.SendTMConcluded(input);
            //Log.Information("Got Result");
            return ProcessResultStatus(result);
        }
        [Route("gettminvitationemailpreview")]
        [HttpPost]
        public string GetTMInvationEmailPreview([FromBody]InputDto_SendTMInvitation input)
        {
            var asmtDataService = new AssessmentDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var lookupLists = asmtDataService.GetLookupFieldsForAllAssessments();
            var dataService = new TeamMeetingDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GenerateTMInvitationHtml(input, lookupLists);

            return result;
        }
        [Route("GetTeamMeeting")]
        [HttpPost]
        public OutputDto_ManageTeamMeeting GetTeamMeeting([FromBody] InputDto_SimpleIdAndBool input)
        {
            var dataService = new TeamMeetingDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetTeamMeeting(input);

            return result;
        }

        //[HttpGet("getsectionsfordropdown")]
        //public List<OutputDto_OptGroupDropdownData> GetSectionsForDropdown(int pageNo, int schoolYear, string searchString)
        //{
        //    var dataService = new TeamMeetingDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
        //    var result = dataService.QuickSearchSections(searchString, schoolYear);

        //    return result;

        //}
        [Route("getsectiondetails/{sectionId:int}")]
        [HttpGet]
        public TMSectionDetailsDto GetSectionDetails(int sectionId)
        {
            var dataService = new TeamMeetingDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetSectionDetails(sectionId);

            return result;

        }
        [Route("saveattendeegroup")]
        [HttpPost]
        public AttendeeGroupDto SaveAttendeeGroup([FromBody]InputDto_SaveAttendeeGroup input)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var dataService = new TeamMeetingDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
                    var result = dataService.SaveAttendeeGroup(input);

                    return result;
                }
                catch (Exception ex)
                {
                    throw new UserDisplayableException("There was an error while saving the Attendee Group.  Support has been notified.  Please try again later.", ex);
                }
            }
            throw new UserDisplayableException("There was an error while saving the Attendee Group.  Support has been notified.  Please try again later.", null);
        }
        [Route("saveteammeeting")]
        [HttpPost]
        public IHttpActionResult SaveTeamMeeting([FromBody]OutputDto_ManageTeamMeeting meeting)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var dataService = new TeamMeetingDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
                    var result = dataService.SaveTeamMeeting(meeting);

                    return Ok(meeting);
                }
                catch (Exception ex)
                {
                    throw new UserDisplayableException("There was an error while saving the Team Meeting.  Support has been notified.  Please try again later.", ex);
                }
            }
            throw new UserDisplayableException("There was an error while saving the Team Meeting.  Support has been notified.  Please try again later.", null);
        }


        [Route("savesingleteammeetingattendance")]
        [HttpPost]
        public IHttpActionResult SaveSingleTeamMeetingAttendance([FromBody]InputDto_TeamMeetingAttendance input)
        {
            var dataService = new TeamMeetingDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.SaveSingleTeamMeetingAttendance(input.TeamMeetingAttendance);

            return ProcessResultStatus(result);
        }
        [Route("saveallteammeetingattendances")]
        [HttpPost]
        public IHttpActionResult SaveAllTeamMeetingAttendances([FromBody]InputDto_TeamMeetingAttendances input)
        {
            var dataService = new TeamMeetingDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.SaveAllTeamMeetingAttendances(input.TeamMeetingAttendances);

            return ProcessResultStatus(result);
        }
        [Route("deleteteammeeting")]
        [HttpPost]
        public IHttpActionResult DeleteTeamMeeting([FromBody]InputDto_SimpleId input)
        {
            if (input == null)
            {
                return NotFound();
            }

            try
            {
                var dataService = new TeamMeetingDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
                var result = dataService.DeleteTeamMeeting(input.Id);

                return Ok();
            }
            catch (UserDisplayableException uex)
            {
                return BadRequest(uex.Message + " Please try again.");
            }
            catch (Exception ex)
            {
                return BadRequest("Bad things happened");
            }
        }
        [Route("deleteattendeegroup")]
        [HttpPost]
        public IHttpActionResult DeleteAttendeeGroup([FromBody]InputDto_SimpleId input)
        {
            if (input == null)
            {
                return NotFound();
            }

            try
            {
                var dataService = new TeamMeetingDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
                var result = dataService.DeleteAttendeeGroup(input.Id);

                return Ok();
            }
            catch (UserDisplayableException uex)
            {
                return BadRequest(uex.Message + " Please try again.");
            }
            catch (Exception ex)
            {
                return BadRequest("Bad things happened");
            }
        }
    }
}
