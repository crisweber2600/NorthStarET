using System.Security.Claims;
using System.Web.Http;
using NorthStar.EF6;
using NorthStar4.PCL.Entity;
using System.Collections.Generic;
using System.Linq;
using EntityDto.DTO.Admin.Section;
using NorthStar4.PCL.DTO;
using System;
using NorthStar4.API.Infrastructure;
using Northstar.Core;
using System.Net.Http;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace NorthStar4.api
{
    [RoutePrefix("api/Section")]
    [Authorize]
    public class SectionController : NSBaseController
    {



        [Route("GetSectionList")]
        [HttpPost]
        public OutputDto_GetSectionList GetSectionList([FromBody] InputDto_GetSectionList input)
        {
            var dataService = new SectionDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetSectionList(input);

            return result;
        }
        [Route("GetSection/{Id:int}")]
        [HttpGet]
        public IHttpActionResult GetSection(int Id)
        {
            var dataService = new SectionDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.GetSection(Id);

            return ProcessResultStatus(result);
        }

        [Route("getcoteachersfordropdown")]
        [HttpGet]
        public List<OutputDto_DropdownData> GetCoTeachersForDropdown(int pageNo, string searchString)
        {
            var dataService = new SectionDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.QuickSearchTeachers(searchString);

            return result;
            //return ProcessResultStatus(result);

        }

        [Route("deletesection")]
        [HttpPost]
        public IHttpActionResult DeleteSection([FromBody]InputDto_SimpleId input)
        {
            if(input == null)
            {
                return NotFound(); 
            }

            try
            {
                var dataService = new SectionDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
                var result = dataService.DeleteSection(input.Id);

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

        //[HttpGet("getstudentquicksearch")]
        //public IHttpActionResult GetStudentQuickSearchResults(string strSearch, bool expandSearch)
        //{
        //    var dataService = new SectionDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
        //    var result = dataService.GetStudentQuickSearchResults(strSearch, expandSearch);

        //    return ProcessResultStatus(result);
        //}
        [Route("savesection")]
        [HttpPost]
        public IHttpActionResult SaveSection([FromBody]OutputDto_ManageSection section)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var dataService = new SectionDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
                    var result = dataService.SaveSection(section);

                    if (result.Status.StatusCode == EntityDto.DTO.Admin.Simple.StatusCode.AccessDenied)
                    {
                        return Unauthorized();
                    }
                    else if (result.Status.StatusCode == EntityDto.DTO.Admin.Simple.StatusCode.UserDisplayableException)
                    {
                        return BadRequest(result.Status.StatusMessage);
                    }

                    return Ok(section);
                }
                catch(Exception ex)
                {
                    throw new UserDisplayableException("There was an error while saving the Section.  Support has been notified.  Please try again later.", ex);
                }
            }
           throw new UserDisplayableException("There was an error while saving the Section.  Support has been notified.  Please try again later.", null);
        }
        [Route("QuickSearchSections")]
        [HttpGet]
        public List<OutputDto_OptGroupDropdownData> QuickSearchSections(int schoolYear, string searchString)
        {
            var dataService = new SectionDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.QuickSearchSections(searchString, schoolYear);

            return result;

        }

        [Route("QuickSearchSectionsCurrentYear")]
        [HttpGet]
        public List<OutputDto_OptGroupDropdownData> QuickSearchSectionsCurrentYear(string searchString)
        {
            var dataService = new SectionDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var schoolYear = GetDefaultYear();

            var result = dataService.QuickSearchSections(searchString, schoolYear);

            return result;

        }
        [Route("QuickSearchSectionsAllSchoolYears")]
        [HttpGet]
        public List<OutputDto_OptGroupDropdownData> QuickSearchSectionsAllSchoolYears(string searchString)
        {
            var dataService = new SectionDataService(((ClaimsIdentity)User.Identity), LoginConnectionString);
            var result = dataService.QuickSearchSectionsAllSchoolYears(searchString);

            return result;

        }

        public short GetDefaultYear()
        {
            if (DateTime.Now.Month > 7)
            {
                return (short)DateTime.Now.Year;
            }
            else
            {
                return (short)(DateTime.Now.Year - 1);
            }
        }
    }
}
