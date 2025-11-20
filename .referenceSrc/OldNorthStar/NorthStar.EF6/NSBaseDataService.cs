using EntityDto.DTO.Assessment;
using NorthStar4.CrossPlatform.DTO;
using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using NorthStar.Core;
using NorthStar4.PCL.DTO;
using EntityDto.DTO;
using EntityDto.DTO.Admin.Simple;
using System.Security.Claims;
using EntityDto.DTO.Admin.Section;
using System.Data.Entity;
using Northstar.Core;
using EntityDto.DTO.Admin.Student;
using Newtonsoft.Json.Linq;
using EntityDto.Entity;
using System.Data;
using EntityDto.DTO.Admin.TeamMeeting;
using EntityDto.DTO.Personal;
using EntityDto.DTO.Misc;
using NorthStar4.CrossPlatform.DTO.Reports;
using EntityDto.DTO.Reports.ObservationSummary;
using EntityDto.DTO.Navigation;
using EntityDto.DTO.Calendars;
using NorthStar.Core.Identity;
using Northstar.Core.Identity;
using NorthStar4.CrossPlatform.DTO.Admin.Staff;
using System.Configuration;
using Serilog;

namespace NorthStar.EF6
{
    public class NSBaseDataService
    {
        protected readonly DistrictContext _dbContext;
        protected readonly LoginContext _loginContext;
        protected readonly string _loginConnectionString;
        protected Staff _currentUser;
        protected int _pageSize = 50;

        public Staff Currentuser
        {
            get { return _currentUser; }
        }

        //public string SendGridUser
        //{
        //    get
        //    {
        //        return ConfigurationManager.AppSettings["SendGridUser"];
        //    }
        //}

        //public string SendGridPassword
        //{
        //    get
        //    {
        //        return ConfigurationManager.AppSettings["SendGridPassword"];
        //    }
        //}

        public string SendGridApiKey
        {
            get
            {
                return ConfigurationManager.AppSettings["SendGridApiKey"];
            }
        }


        public NSBaseDataService(ClaimsIdentity user, string loginConnectionString)
        {
            // TODO: First we check the claims to see if there is a district ID available, if not we 
            // just pick the first one

            _loginContext = new LoginContext(loginConnectionString);
            _loginConnectionString = loginConnectionString;

            var username = user.FindFirst(NSConstants.ClaimTypes.AuthenticatedAccount) != null ? user.FindFirst(NSConstants.ClaimTypes.AuthenticatedAccount).Value : user.Claims.First(x => x.Type == "preferred_username").Value;  //user.Claims.First(x => x.Type == "preferred_username").Value;

            int districtId = -1;
            var staffDistrictUser = _loginContext.StaffDistricts.FirstOrDefault(p => p.StaffEmail == username);

            if (staffDistrictUser != null && staffDistrictUser.IsSA == true)
            {
                districtId = staffDistrictUser.DistrictId;
            }
            else
            {
                districtId = Int32.Parse(user.FindFirst(NSConstants.ClaimTypes.DistrictId).Value);
            }


            if (districtId != -1)
            {
                var districtContextDbString = _loginContext.DistrictDbs.FirstOrDefault(p => p.DistrictId == districtId);

                // TODO: potentially get this from Azure settings
                if (districtContextDbString != null)
                {
                    _dbContext = new DistrictContext(districtContextDbString.DbName);
                    _currentUser = _dbContext.Staffs.First(p => p.Email != null && p.Email.ToLower().Equals(username.ToLower()));
                    _currentUser.DistrictId = districtId;

                    if(_currentUser.IsActive == false)
                    {
                        Log.Information("Disabled User attempting login: " + _currentUser.Email);
                        throw new Exception("Disabled User attempting login: " + _currentUser.Email);
                    }
                }
                else
                {
                    // log exception that no database exists for this district ID and throw exception
                    throw new Exception("NorthStarDataService: No connection string available for database: " + districtId.ToString());
                }
            }
            else
            {
                // log exception that user doesn't have access to any DB and throw exception
                throw new Exception("NorthStarDataService: User " + username + " does not have access to any districts");
            }
        }

        public List<OutputDto_DropdownData> QuickSearchGrades(string search)
        {
            var grades = _dbContext.Grades.Where(p => p.LongName.Contains(search) || p.ShortName.Contains(search) || String.IsNullOrEmpty(search)).OrderBy(p => p.GradeOrder).ToList();

            return Mapper.Map<List<OutputDto_DropdownData>>(grades);
        }

        public TestDueDate GetCurrentBenchmarkDate(int schoolStartYear, DateTime today)
        {
            var benchMarkWeeksForDistrict = _dbContext.TestDueDates.Where(p => p.SchoolStartYear == schoolStartYear).OrderBy(p => p.DueDate).ToList();

            // if benchmark dates aren't set
            if (benchMarkWeeksForDistrict.Count == 0)
            {
                return null;
            }

            TestDueDate defaultTdd = null;

            var smallestDate = benchMarkWeeksForDistrict.OrderBy(p => p.DueDate).FirstOrDefault();
            var largestDate = benchMarkWeeksForDistrict.OrderByDescending(p => p.DueDate).FirstOrDefault();

            defaultTdd = smallestDate;
            if (today <= smallestDate.DueDate.Value)
            {
                defaultTdd = smallestDate;
            }
            else if (today >= largestDate.DueDate.Value)
            {
                defaultTdd = largestDate;
            }
            else
            {
                var selectedDate =
                    benchMarkWeeksForDistrict.Where(
                        p =>
                        (p.StartDate.HasValue && p.StartDate.Value <= today)).OrderByDescending(p => p.DueDate).
                        FirstOrDefault();

                if (selectedDate != null)
                {
                    defaultTdd = selectedDate;
                }
            }

            return defaultTdd;
        }

        public bool IsSysAdmin
        {
            get
            {
                if (_currentUser.IsSA == true)
                    return true;
                else return false;

                //var admin = _loginContext.SysAdmins.FirstOrDefault(p => p.StaffEmail.ToLower() == _currentUser.Email.ToLower());

                //if (admin != null)
                //{
                //    return true;
                //}
                //else
                //{
                //    return false;
                //}
            }
        }

        public bool IsDistrictAdmin
        {
            get
            {
                var admin = _dbContext.Staffs.FirstOrDefault(p => p.Email.ToLower() == _currentUser.Email.ToLower() && p.IsDistrictAdmin);

                if (admin != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        public bool IsSchoolAdmin(int schoolId)
        {
            var admin = _dbContext.StaffSchools.FirstOrDefault(p => p.StaffID == _currentUser.Id && p.StaffHierarchyPermissionID == 1 && p.SchoolID == schoolId);

            if (admin != null)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool IsGradeAdmin(int schoolId, int gradeId)
        {
            var admin = _dbContext.StaffSchoolGrades.FirstOrDefault(p => p.StaffID == _currentUser.Id && p.StaffHierarchyPermissionID == 1 && p.SchoolID == schoolId && p.GradeID == gradeId);

            if (admin != null)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool IsAnyGradeAdmin()
        {
            if (IsAnySchoolAdmin() || IsDistrictAdmin)
            {
                return true;
            }

            var admin = _dbContext.StaffSchoolGrades.FirstOrDefault(p => p.StaffID == _currentUser.Id && p.StaffHierarchyPermissionID == 1);

            if (admin != null)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool IsAnySchoolAdmin()
        {
            var admin = _dbContext.StaffSchools.FirstOrDefault(p => p.StaffID == _currentUser.Id && p.StaffHierarchyPermissionID == 1);

            if (admin != null)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool IsSectionAdmin(int sectionId)
        {
            var admin = _dbContext.StaffSections.FirstOrDefault(p => p.StaffID == _currentUser.Id && p.ClassID == sectionId);

            if (admin != null)
            {
                return true;
            }
            else
            {
                return false;
            }

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

        public OutputDto_Status SecurityCheck(int sectionId)
        {
            OutputDto_Status status = new OutputDto_Status();
            var section = _dbContext.Sections.FirstOrDefault(p => p.Id == sectionId);

            //check if sys admin
            if (IsSysAdmin || section == null)
            {
                status.StatusCode = StatusCode.Ok;
            }
            // check if district admin
            else if (IsDistrictAdmin)
            {
                status.StatusCode = StatusCode.Ok;
            }
            // get the school for the section and see if this user is an admin
            else if (IsSchoolAdmin(section.SchoolID))
            {
                status.StatusCode = StatusCode.Ok;
            }
            // check the grade/staff table and see if has access

            // check the staffsection table and see if has access
            else if (IsSectionAdmin(section.Id))
            {
                status.StatusCode = StatusCode.Ok;
            }
            // if none of those checks pass, access denied
            else
            {
                status.StatusCode = StatusCode.AccessDenied;
                status.StatusMessage = "You do not have access to the section '" + section.Name + "'";
            }

            return status;
        }

        public OutputDto_Base SchoolAdminSecurityCheck(int schoolId)
        {
            OutputDto_Base status = new OutputDto_Base();

            //check if sys admin
            if (IsSysAdmin)
            {
                status.Status.StatusCode = StatusCode.Ok;
            }
            // check if district admin
            else if (IsDistrictAdmin)
            {
                status.Status.StatusCode = StatusCode.Ok;
            }
            // get the school for the section and see if this user is an admin
            else if (IsSchoolAdmin(schoolId))
            {
                status.Status.StatusCode = StatusCode.Ok;
            }
            // if none of those checks pass, access denied
            else
            {
                status.Status.StatusCode = StatusCode.AccessDenied;
                status.Status.StatusMessage = "You do not have administrative access to this school.";
            }

            return status;
        }

        private Section GetCurrentSection(int StudentId, int SchoolYear)
        {
            var currentSection = _dbContext.StudentSections.FirstOrDefault(p =>
                p.StudentID == StudentId &&
                p.Section.SchoolStartYear == SchoolYear &&
                p.Section.IsInterventionGroup == false);

            if (currentSection == null)
            {
                return null;
            }
            else
            {
                return currentSection.Section;
            }
        }


        public List<OutputDto_DropdownData> QuickSearchTeachers(string searchString)
        {
            // TODO: security check

            var teachers = _dbContext.Staffs.Where(p => (p.FirstName.StartsWith(searchString) || p.LastName.StartsWith(searchString))).OrderBy(p => p.LastName).ThenBy(p => p.FirstName).Take(50);

            if (teachers.Any())
            {
                var results = new List<OutputDto_DropdownData>();

                foreach (var teacher in teachers)
                {
                    results.Add(new OutputDto_DropdownData()
                    {
                        id = teacher.Id,
                        text = teacher.LastName + ", " + teacher.FirstName
                    });
                }


                return results;
            }
            else
            {
                return new List<OutputDto_DropdownData>();
            }
        }

        public List<OutputDto_DropdownData> QuickSearchStaffGroups(string searchString)
        {
            // TODO: Implement StaffGroups
            var teachers = _dbContext.Staffs.Where(p => (p.FirstName.StartsWith(searchString) || p.LastName.StartsWith(searchString)) && p.IsInterventionSpecialist == false).OrderBy(p => p.LastName).ThenBy(p => p.FirstName).Take(10);

            if (teachers.Any())
            {
                var results = new List<OutputDto_DropdownData>();

                foreach (var teacher in teachers)
                {
                    results.Add(new OutputDto_DropdownData()
                    {
                        id = teacher.Id,
                        text = teacher.LastName + ", " + teacher.FirstName
                    });
                }


                return results;
            }
            else
            {
                return new List<OutputDto_DropdownData>();
            }
        }



        // TODO: Delete this crap... it is useless
        //public OutputDto_StudentQuickSearch GetStudentQuickSearchResults(string strSearch, bool expandSearch)
        //{
        //    var result = _dbContext.Students.Where(p => p.LastName.StartsWith(strSearch))
        //                    .Select(p => new StudentQuickSearchResult()
        //                    {
        //                        StudentId = p.Id,
        //                        LastName = p.LastName,
        //                        FirstName = p.FirstName,
        //                        CurrentSectionName = "test",
        //                        CurrentGradeName = "One",
        //                        CurrentlyRegistered = true,
        //                        CurrentSchoolName = "East Bay",
        //                        CurrentTeacherName = "Engles",
        //                        StudentIdentifier = p.StudentIdentifier
        //                    }).Take(10).ToList();

        //    return new OutputDto_StudentQuickSearch { Results = result };
        //}


    }
}
