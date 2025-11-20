using System.Security.Claims;
using System.Web.Http;
using NorthStar4.CrossPlatform.DTO.Admin.Staff;
using NorthStar.EF6;
using NorthStar4.PCL.Entity;
using System.Collections.Generic;
using System.Linq;
using Northstar.Core.Identity;
using EntityDto.DTO.Admin.Simple;
using NorthStar4.API.Infrastructure;
using NorthStar4.PCL.DTO;
using EntityDto.LoginDB.DTO;
using System.Net.Http;
using System.Net.Http.Headers;
using System;
using EntityDto.DTO.Admin.Staff;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace NorthStar4.api
{
	[RoutePrefix("api/Staff")]
	[Authorize]
	public class StaffController : NSBaseController
	{
		//private readonly DistrictContext _dbContext;

		//private NorthStarDataService dataService = null;

		//[HttpGet]
		//public IEnumerable<Staff> Get()
		//{
		//	//return _dbContext.Staffs;
		//}
		[Route("MyInfo")]
		[HttpGet]
		public StaffDto  MyInfo()
		{
			var dataService = new StaffDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
			var result = dataService.MyInfo();

			return result;
		}

		[Route("UpdateUserProfile")]
		[HttpPost]
		public OutputDto_Base UpdateUserProfile([FromBody]InputDto_EditProfile input)
		{
			var dataService = new StaffDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
			var result = dataService.SaveUserProfileInfo(input);

			return result;
		}

		//[Route("SaveLoginCookie")]
		//[HttpPost]
		//public HttpResponseMessage SaveLoginCookie([FromBody]InputDto_SaveLoginCookie input)
		//{
		//    var resp = new HttpResponseMessage();

		//    var cookie = new CookieHeaderValue("NSLogin", input.SaveCookie ? input.UserName : string.Empty);
		//    cookie.Expires = DateTimeOffset.Now.AddDays(30);
		//    cookie.Domain = Request.RequestUri.Host;
		//    cookie.Path = "/";

		//    resp.Headers.AddCookies(new CookieHeaderValue[] { cookie });
		//    return resp;
		//}

		[Route("ValidateTeacherKeyChange")]
		[HttpPost]
		public IHttpActionResult ValidateTeacherKeyChange([FromBody]InputDto_SimpleString input)
		{
			var dataService = new StaffDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);

			var result = dataService.ValidateTeacherKeyChange(input);

			// return simple success or failure
			return ProcessResultStatus(result);
		}
		[Route("ValidateTeacherKeyChangeOtherUser")]
		[HttpPost]
		public IHttpActionResult ValidateTeacherKeyChangeOtherUser([FromBody]InputDto_StringAndUserId input)
		{
			var dataService = new StaffDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);

			var result = dataService.ValidateTeacherKeyChange(input);

			// return simple success or failure
			return ProcessResultStatus(result);
		}
		[Route("ValidateEmailFormat")]
		[HttpPost]
		public IHttpActionResult ValidateEmailFormat([FromBody]InputDto_SimpleString input)
		{
			var util = new RegexUtilities();
			var result = new OutputDto_SuccessAndStatus();
			if(util.IsValidEmail(input.value))
			{
				result.isValid = true;
			} else
			{
				result.isValid = false;
			}

			// return simple success or failure
			return ProcessResultStatus(result);
		}
		[Route("ValidateUsernameChange")]
		[HttpPost]
		public IHttpActionResult ValidateUsernameChange([FromBody]InputDto_SimpleString input)
		{
			var dataService = new StaffDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);

			var result = dataService.ValidateUsernameChange(input);

			// return simple success or failure
			return ProcessResultStatus(result);
		}
		[Route("ValidateUsernameChangeOtherUser")]
		[HttpPost]
		public IHttpActionResult ValidateUsernameChangeOtherUser([FromBody]InputDto_StringAndUserId input)
		{
			var dataService = new StaffDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);

			var result = dataService.ValidateUsernameChange(input);

			// return simple success or failure
			return ProcessResultStatus(result);
		}

		[Route("GetStaffById/{id:int}")]
		[HttpGet]
		public OutputDto_EditStaff Get(int id)
		{
			var dataService = new StaffDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
			var result = dataService.GetStaffForEdit(id);
			return result;            
		}
		[Route("GetStaffBySchool/{schoolId:int}")]
		[HttpGet]
		public List<StaffDto> GetStaffBySchool(int schoolId)
		{
			var dataService = new StaffDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
			var result = dataService.GetStaffBySchool(schoolId);
		 
			return result;
		}
		[Route("GetSchoolAdminstrators")]
		[HttpGet]
		public List<OutputDto_DropdownData> GetSchoolAdminstrators()
		{
			var dataService = new StaffDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
			var result = dataService.GetSchoolAdminstrators();

			return result;
		}
		[Route("GetDistrictAdminstrators")]
		[HttpGet]
		public List<OutputDto_DropdownData> GetDistrictAdminstrators()
		{
			var dataService = new StaffDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
			var result = dataService.GetDistrictAdminstrators();

			return result;
		}

		[Route("ConsolidateStaff")]
		[HttpPost]
		public IHttpActionResult ConsolidateStaff([FromBody]InputDto_ConsolidateStaff input)
		{
			var dataService = new StaffDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
			var result = dataService.ConsolidateStaff(input);

			return ProcessResultStatus(result);
		}


		[Route("SendMail")]
		[HttpPost]
		public IHttpActionResult SendMail([FromBody]InputDto_SendMail input)
		{
			var dataService = new StaffDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
			var targetUser = dataService.GetStaffUnvalidated(input.ToId);
			var currentUser = dataService.Currentuser;

			EmailHandler.SendGenericEmail(targetUser.Email, targetUser.FullName, "support@northstaret.net", "Question From North Star User " + currentUser.FullName, input.Subject, input.Message, SiteUrlBase);
			return ProcessResultStatus(new OutputDto_Base());
		}
		[Route("SendSupportMail")]
		[HttpPost]
		public IHttpActionResult SendSupportMail([FromBody]InputDto_SendMail input)
		{
			var dataService = new StaffDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
			var currentUser = dataService.Currentuser;

			EmailHandler.SendGenericEmail("northstar.shannon@gmail.com", "support@northstaret.net", "support@northstaret.net", "Support Question From North Star District Admin " + currentUser.FullName, input.Subject, input.Message, SiteUrlBase);
			return ProcessResultStatus(new OutputDto_Base());
		}
		[Route("quicksearchstaff")]
		[HttpGet]
		public List<StaffQuickSearchResult> QuickSearchStaff(string searchString)
		{
			var dataService = new StaffDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
			var result = dataService.GetStaffQuickSearchResults(searchString);

			return result;
		}
		[Route("ResetUsersPassword")]
		[HttpPost]
		public IHttpActionResult ResetUsersPassword([FromBody]InputDto_ResetUsersPassword input)
		{
			var dataService = new StaffDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
			var result = dataService.ResetUsersPassword(input);

			return ProcessResultStatus(result);
		}
		[Route("SaveStaff")]
		[HttpPost]
		public IHttpActionResult SaveStaff([FromBody]OutputDto_EditStaff staff)
		{
			var dataService = new StaffDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
			var currentUser = dataService.Currentuser;
			bool isNewUser = false;
			bool userNameChanged = false;
			string newPassword;

			var result = dataService.SaveStaff(staff, out isNewUser, out userNameChanged, out newPassword);

			// send new user email
			if (isNewUser)
			{
				EmailHandler.SendUserPasswordEmail(newPassword, staff.Email, staff.Email, currentUser.Email , staff.LastName + "," + staff.FirstName, SiteUrlBase);
			}
			else if (userNameChanged)
			{
				EmailHandler.SendNewUsernameEmail(staff.Email, staff.LastName + ", " + staff.FirstName, SiteUrlBase);
			}

			// return simple success or failure
			return ProcessResultStatus(result);
		}

		[Route("DeleteStaff")]
		[HttpPost]
		public IHttpActionResult DeleteStaff([FromBody]InputDto_SimpleId input)
		{
			var dataService = new StaffDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
			var result = dataService.DeleteStaff(input);

			return ProcessResultStatus(result);
		}
	}
}
