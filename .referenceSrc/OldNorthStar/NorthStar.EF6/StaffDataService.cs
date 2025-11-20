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
using System.Collections.Generic;
using NorthStar4.PCL.Entity;
using EntityDto.DTO.Assessment;
using SendGrid;
using System.Net.Mail;
using System.Data.SqlClient;
using System;
using EntityDto.LoginDB.Entity;
using EntityDto.LoginDB.DTO;
using EntityDto.DTO.Admin.Staff;
using System.Configuration;

namespace NorthStar.EF6
{


    public class StaffDataService : NSBaseDataService
    {
        public StaffDataService(ClaimsIdentity user, string loginConnectionString) : base(user, loginConnectionString)
        {
        }

        public OutputDto_SuccessAndStatus ResetUsersPassword(InputDto_ResetUsersPassword input)
        {
            var status = new OutputDto_SuccessAndStatus();
            var canReset = false;
            var targetAccount = _dbContext.Staffs.FirstOrDefault(p => p.Email == input.UserName);
            // security checks
            if(targetAccount == null)
            {
                status.Status = new OutputDto_Status { StatusCode = StatusCode.UserDisplayableException, StatusMessage = "This user account does not exist in the system." };
                return status;
            }


            if (_currentUser.IsDistrictAdmin == true || _currentUser.IsPowerUser == true)
            {
                canReset = true;
            }
            else
            {
                // get schools current user is admin at
                List<StaffSchool> currentUserStaffSchools = _dbContext.StaffSchools.Where(p => p.StaffID == _currentUser.Id && p.StaffHierarchyPermissionID == 1).ToList();

                // get schools search result user has access to
                var targetUserStaffSchools = _dbContext.StaffSchools.Where(p => p.StaffID == targetAccount.Id).ToList();


                foreach (StaffSchool sc in currentUserStaffSchools)
                {
                    foreach (StaffSchool dto in targetUserStaffSchools)
                    {
                        if (dto.SchoolID == sc.SchoolID)
                        {

                            // they are a school admin... before we get carried away, make sure target user isn't district admin
                            if (dto.Staff.IsDistrictAdmin)
                            {
                                canReset = false;
                                status.Status = new OutputDto_Status { StatusCode = StatusCode.UserDisplayableException, StatusMessage = "You do not have access to reset this user's password." };
                                return status;
                            }
                            else
                            {
                                canReset = true;
                            }
                        }
                    }
                }               

            }

            if (canReset)
            {
                // first see if user exists, if not create account
                UserStoreManager mgr = new UserStoreManager(_loginConnectionString);
                var aspNetUser = mgr.GetUserByEmail(input.UserName);

                if (aspNetUser.Result == null)
                {
                    
                    // this is a new user, create a new user account
                    mgr.CreateUser(input.UserName, input.Password);
                    // now give user district access
                    StaffDistrict sd = new StaffDistrict { DistrictId = _currentUser.DistrictId, StaffEmail = input.UserName };
                    _loginContext.StaffDistricts.Add(sd);
                    _loginContext.SaveChanges();
                } else
                {
                    // make sure teacher has a record in StaffDistrictTable
                    var sd = _loginContext.StaffDistricts.FirstOrDefault(p => p.StaffEmail == input.UserName);

                    if(sd == null)
                    {
                        StaffDistrict sd1 = new StaffDistrict { DistrictId = _currentUser.DistrictId, StaffEmail = input.UserName };
                        _loginContext.StaffDistricts.Add(sd1);
                        _loginContext.SaveChanges();
                    }
                }

                var result = mgr.SetPassword(input.UserName, input.Password);
     
                if (result.IsSuccess)
                {
                    status.Status = new OutputDto_Status { StatusCode = StatusCode.Ok };
                }
                else
                {
                    status.Status = new OutputDto_Status { StatusCode = StatusCode.UserDisplayableException, StatusMessage = result.Errors.FirstOrDefault() };
                }
            }

            return status;
        }

        public OutputDto_SuccessAndStatus ValidateUsernameChange(InputDto_SimpleString input)
        {
            var result = new OutputDto_SuccessAndStatus();

            var email = input.value;

            if (_dbContext.Staffs.Any(p => p.Email.Equals(email, System.StringComparison.CurrentCultureIgnoreCase) && p.Email != _currentUser.Email))
            {
                // can get belongs to here if we want
                result.isValid = false;
                //var who = _dbContext.Staffs.FirstOrDefault(p => p.TeacherIdentifier.Equals(teachKey, System.StringComparison.CurrentCultureIgnoreCase) && p.Id != _currentUser.Id);
                //if(who != null)
                //{
                //    result.value = "This Teacher Key is already assigned to: " + who.FullName + " (" + who.Email + ")";
                //}
            }
            else
            {
                result.isValid = true;
            }

            return result;
        }

        public OutputDto_SuccessAndStatus StartVersionUpdate()
        {
            var currentNSVersion = _loginContext.NorthStarVersions.FirstOrDefault()?.Version;

            if(currentNSVersion != null)
            {
                var db_setting = _loginContext.StaffDistricts.Where(p => p.StaffEmail == _currentUser.Email).FirstOrDefault();

                if(db_setting != null)
                {
                    db_setting.CurrentVersion = currentNSVersion;
                    db_setting.VersionLastUpdated = null;
                    _loginContext.SaveChanges();
                } 
            }

            return new OutputDto_SuccessAndStatus();
        }

        public OutputDto_GetUserCurrentVersion GetUserCurrentVersion()
        {
            var result = new OutputDto_GetUserCurrentVersion();

            var currentNSVersion = _loginContext.NorthStarVersions.FirstOrDefault()?.Version;

            if (currentNSVersion != null)
            {
                result.CurrentNSVersion = currentNSVersion;
            }

            var db_setting = _loginContext.StaffDistricts.Where(p => p.StaffEmail == _currentUser.Email).FirstOrDefault();

            if (db_setting != null)
            {
                result.UserCurrentVersion = db_setting.CurrentVersion;
                result.VersionLastUpdated = db_setting.VersionLastUpdated;
            }

            return result;
        }

        public OutputDto_SuccessAndStatus FinalizeVersionUpdate()
        {

            var db_setting = _loginContext.StaffDistricts.Where(p => p.StaffEmail == _currentUser.Email).FirstOrDefault();

            if (db_setting != null)
            {
                db_setting.VersionLastUpdated = DateTime.Now;
                _loginContext.SaveChanges();
            }

            return new OutputDto_SuccessAndStatus();
        }

        public OutputDto_SuccessAndStatus ValidateUsernameChange(InputDto_StringAndUserId input)
        {
            var result = new OutputDto_SuccessAndStatus();

            var email = input.value;

            if (_dbContext.Staffs.Any(p => p.Email.Equals(email, System.StringComparison.CurrentCultureIgnoreCase) && p.Id != input.UserId))
            {
                // can get belongs to here if we want
                result.isValid = false;
                //var who = _dbContext.Staffs.FirstOrDefault(p => p.TeacherIdentifier.Equals(teachKey, System.StringComparison.CurrentCultureIgnoreCase) && p.Id != _currentUser.Id);
                //if(who != null)
                //{
                //    result.value = "This Teacher Key is already assigned to: " + who.FullName + " (" + who.Email + ")";
                //}
            }
            else
            {
                result.isValid = true;
            }

            return result;
        }

        public OutputDto_Base DeleteStaff(InputDto_SimpleId input)
        {
            var result = new OutputDto_SuccessAndStatus();
            var staffToChange = _dbContext.Staffs.First(p => p.Id == input.Id);
            var email = staffToChange.Email;

            // TODO: check security
            if (_dbContext.StaffSections.Any(p => p.StaffID == staffToChange.Id)) {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "This teacher is currently assigned to one or more classes and cannot be deleted";
                return result;
            }
            if (_dbContext.StaffInterventionGroups.Any(p => p.StaffID == staffToChange.Id)) {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "This teacher is currently assigned to one or more intervention groups and cannot be deleted";
                return result;
            }

            // do a bunch of checks for integrity violations so that we don't waste time doing deletes that we know will fail

            // attempt delete

            try
            {


                var staffSchoolGrades = _dbContext.StaffSchoolGrades.Where(p => p.StaffID == input.Id);
                var staffSchools = _dbContext.StaffSchools.Where(p => p.StaffID == input.Id);
                var staffAssessments = _dbContext.StaffAssessments.Where(p => p.StaffId == input.Id);
                _dbContext.StaffSchoolGrades.RemoveRange(staffSchoolGrades);
                _dbContext.StaffSchools.RemoveRange(staffSchools);
                _dbContext.StaffAssessments.RemoveRange(staffAssessments);
                _dbContext.Staffs.Remove(staffToChange);
                _dbContext.SaveChanges();

                var login = _loginContext.StaffDistricts.FirstOrDefault(p => p.StaffEmail.Equals(email, StringComparison.CurrentCultureIgnoreCase));
                if (login != null)
                {
                    _loginContext.StaffDistricts.Remove(login);
                    _loginContext.SaveChanges();
                }

                // remove account
                try
                {
                    UserStoreManager mgr = new UserStoreManager(_loginConnectionString);
                    mgr.DeleteUser(email);
                }
                catch (Exception ex)
                {
                    // LOG can't delete account
                }
            } catch(Exception ex)
            {
                // LOG ex
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "Unable to delete this user.  This user is still in use.";
            }

            return result;
        }

        public OutputDto_SuccessAndStatus ChangeUsername(InputDto_SimpleString input)
        {
            var result = new OutputDto_SuccessAndStatus();
            
            var staffToChange = _dbContext.Staffs.First(p => p.Id == _currentUser.Id);
            staffToChange.Email = input.value;
            staffToChange.LoweredUserName = input.value;
            _dbContext.SaveChanges();
            return result;
        }

        public OutputDto_SuccessAndStatus ValidateTeacherKeyChange(InputDto_SimpleString input)
        {
            var result = new OutputDto_SuccessAndStatus();

            var teachKey = input.value;

            if(_dbContext.Staffs.Any(p => p.TeacherIdentifier.Equals(teachKey, System.StringComparison.CurrentCultureIgnoreCase) && p.Id != _currentUser.Id))
            {
                // can get belongs to here if we want
                result.isValid = false;
                //var who = _dbContext.Staffs.FirstOrDefault(p => p.TeacherIdentifier.Equals(teachKey, System.StringComparison.CurrentCultureIgnoreCase) && p.Id != _currentUser.Id);
                //if(who != null)
                //{
                //    result.value = "This Teacher Key is already assigned to: " + who.FullName + " (" + who.Email + ")";
                //}
            }
            else
            {
                result.isValid = true;
            }

            return result;
        }

        public OutputDto_SuccessAndStatus ValidateTeacherKeyChange(InputDto_StringAndUserId input)
        {
            var result = new OutputDto_SuccessAndStatus();

            var teachKey = input.value;

            if (_dbContext.Staffs.Any(p => p.TeacherIdentifier.Equals(teachKey, System.StringComparison.CurrentCultureIgnoreCase) && p.Id != input.UserId))
            {
                // can get belongs to here if we want
                result.isValid = false;
                //var who = _dbContext.Staffs.FirstOrDefault(p => p.TeacherIdentifier.Equals(teachKey, System.StringComparison.CurrentCultureIgnoreCase) && p.Id != _currentUser.Id);
                //if(who != null)
                //{
                //    result.value = "This Teacher Key is already assigned to: " + who.FullName + " (" + who.Email + ")";
                //}
            }
            else
            {
                result.isValid = true;
            }

            return result;
        }

        public StaffDto MyInfo()
        {
            var districtName = _loginContext.Districts.First(p => p.Id == _currentUser.DistrictId).Name;

            var staff = Mapper.Map<StaffDto>(_dbContext.Staffs.First(p => p.Id == _currentUser.Id));
            staff.DistrictName = districtName;

            return staff;
        }

        public OutputDto_Base SaveUserProfileInfo(InputDto_EditProfile staff)
        {
            var result = new OutputDto_Base();

            Staff dbStaff = _dbContext.Staffs.First(m => m.Id == _currentUser.Id); ;

            dbStaff.FirstName = staff.FirstName;
            dbStaff.LastName = staff.LastName;
            dbStaff.MiddleName = staff.MiddleName;
            dbStaff.RoleID = staff.RoleID;
            dbStaff.TeacherIdentifier = staff.TeacherIdentifier;
            dbStaff.IsInterventionSpecialist = staff.IsInterventionSpecialist;

            _dbContext.SaveChanges();

            return result;
        }

        public List<StaffDto> GetStaffBySchool(int schoolId)
        {
            var staffSchools = _dbContext.StaffSchools.Where(p => p.SchoolID == schoolId).Where(p => p.Staff.IsSA != true).ToList();
            var staffOnly = new List<StaffDto>();
            //.Select(p => p.Staff).OrderBy(p => p.LastName).ThenBy(p => p.FirstName);
            foreach (var staffSchool in staffSchools)
            {
                var staff = _dbContext.Staffs.FirstOrDefault(p =>
                    p.Id == staffSchool.StaffID
                    );

                if (staff != null)
                {
                    var dtoStaff = Mapper.Map<StaffDto>(staff);
                    dtoStaff.IsSchoolAdmin = (staffSchool.StaffHierarchyPermissionID == 1);

                    if(dtoStaff.IsDistrictAdmin)
                    {
                        dtoStaff.AccessLevel = "District Administrator";
                    } else if (dtoStaff.IsSchoolAdmin)
                    {
                        dtoStaff.AccessLevel = "School Administrator";
                    } else if (_dbContext.StaffSchoolGrades.Any(p => p.SchoolID == schoolId && p.StaffID == dtoStaff.Id))
                    {
                        dtoStaff.AccessLevel = "Grade Access";
                    } else
                    {
                        dtoStaff.AccessLevel = "User";
                    }

                    dtoStaff.CanModify = CanModify(dtoStaff, schoolId);

                    staffOnly.Add(dtoStaff);                    
                }
            }

            staffOnly = staffOnly.Where(p => p.IsActive != false).OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToList();
            return staffOnly;
        }

        // TODO: don't forget to test this when saving and throw 401 if user trying to circumvent system
        private bool CanModify(StaffDto testUser, int schoolId)
        {
            if(IsDistrictAdmin)
            {
                return true;
            } else if (IsSchoolAdmin(schoolId))
            {
                // school admin trying to modify district admin, no dice
                if(testUser.IsDistrictAdmin)
                {
                    return false;
                } else // school admins can modify other school and grade admins
                {
                    return true;
                }
            } else
            {
                return false;
            }
        }

        public List<OutputDto_DropdownData> GetSchoolAdminstrators()
        {
            var schoolsUserCanAccess = _dbContext.StaffSchools.Where(p => p.StaffID == _currentUser.Id).Select(p => p.SchoolID).ToList();
            var admins = _dbContext.StaffSchools.Where(p => p.IsSchoolContact == true && p.Staff.IsActive != false && schoolsUserCanAccess.Contains(p.SchoolID)).Select(p => p.Staff).Distinct().OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToList();
            return Mapper.Map<List<OutputDto_DropdownData>>(admins);
        }

        public List<OutputDto_DropdownData> GetDistrictAdminstrators()
        {
            var admins = _dbContext.Staffs.Where(p => p.IsDistrictContact == true).OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToList();
            return Mapper.Map<List<OutputDto_DropdownData>>(admins);
        }

        public List<StaffQuickSearchResult> GetStaffQuickSearchResults(string strSearch)
        {
            // TODO: security check... maybe in PROC?
            bool accessDisabled = true;
            var result = new List<StaffQuickSearchResult>();

            if(_currentUser.IsDistrictAdmin == true || _currentUser.IsPowerUser == true)
            {
                accessDisabled = false;
                result = _dbContext.Staffs.Where(p => p.LastName.Contains(strSearch) || p.FirstName.Contains(strSearch) || p.Email.Contains(strSearch) || p.TeacherIdentifier.Contains(strSearch))
                .Where(p => p.IsSA != true) // remove system admins
                .Select(p => new StaffQuickSearchResult()
                {
                    id = p.Id,
                    LastName = p.LastName,
                    FirstName = p.FirstName,
                    IsActive = p.IsActive.HasValue ? p.IsActive.Value : true,
                    IsInterventionist = p.IsInterventionSpecialist,
                    TeacherKey = p.TeacherIdentifier,
                    Email = p.Email,
                    disabled = accessDisabled // TODO: this is just a hack... do a real check
                            }).OrderBy(p => p.LastName).ThenBy(p => p.FirstName).Take(50).ToList();
            } else
            {
                // get schools current user is admin at
                List<StaffSchool> currentUserStaffSchools = _dbContext.StaffSchools.Where(p => p.StaffID == _currentUser.Id).ToList();

                // get schools search result user has access to
                var targetUserStaffSchools = _dbContext.StaffSchools.Where(p => p.Staff.LastName.Contains(strSearch) || p.Staff.FirstName.Contains(strSearch) || p.Staff.Email.Contains(strSearch) || p.Staff.TeacherIdentifier.Contains(strSearch)).Where(p => p.Staff.IsSA != true).ToList();


                foreach (StaffSchool sc in currentUserStaffSchools)
                {
                    foreach (StaffSchool dto in targetUserStaffSchools)
                    {
                        if (dto.SchoolID == sc.SchoolID)
                        {
                            // if this user is a school admin
                            if (sc.StaffHierarchyPermissionID == 1)
                            {
                                // they are a school admin... before we get carried away, make sure target user isn't district admin
                                // don't let school admin disable a district admin
                                if (dto.Staff.IsDistrictAdmin)
                                {
                                    accessDisabled = true;
                                }
                                else
                                {
                                    accessDisabled = false;
                                }
                            } else
                            {
                                accessDisabled = true;
                            }

                            // distinct fix
                            // BUG! this finds the first school that they have in common... if it's one that this user
                            // isn't an admin at, it will mistakenly show "no access"
                            var alreadyOnList = result.FirstOrDefault(p => p.id == dto.Staff.Id);

                            if(alreadyOnList == null)
                            {
                                result.Add(new StaffQuickSearchResult()
                                {
                                    id = dto.Staff.Id,
                                    LastName = dto.Staff.LastName,
                                    FirstName = dto.Staff.FirstName,
                                    IsActive = dto.Staff.IsActive.HasValue ? dto.Staff.IsActive.Value : true,
                                    IsInterventionist = dto.Staff.IsInterventionSpecialist,
                                    TeacherKey = dto.Staff.TeacherIdentifier,
                                    Email = dto.Staff.Email,
                                    disabled = accessDisabled // TODO: this is just a hack... do a real check
                                });
                            } else
                            {
                                // undo that disabled if we currently have one for enabled - only takes one to enable it
                                if(alreadyOnList.disabled == true && accessDisabled == false)
                                {
                                    alreadyOnList.disabled = accessDisabled;
                                }
                            }
                        }
                    }
                }


                result = result.OrderBy(p => p.LastName).ThenBy(p => p.FirstName).Take(50).ToList();
            }

            return result;

        }

        public OutputDto_EditStaff GetStaffForEdit(int id)
        {
            // TODO: Put this in a filter somewhere
            UserStoreManager mgr = new UserStoreManager(_loginConnectionString);
            var schoolsUserIsAdminAt = new List<School>();
                
            if(!_currentUser.IsDistrictAdmin)
            {
                schoolsUserIsAdminAt = _dbContext.StaffSchools.Where(p => p.StaffID == _currentUser.Id && p.StaffHierarchyPermissionID == 1).Select(p => p.School).ToList();
            }
            else
            {
                schoolsUserIsAdminAt = _dbContext.Schools.ToList();
            }


            // get some additional stuff like security
            Staff staff = _dbContext.Staffs.FirstOrDefault(m => m.Id == id);
            
            // new user setup
            if(staff == null)
            {
                staff = new Staff();
                _dbContext.Staffs.Add(staff);
            }
            var identityUser = mgr.GetUserByEmail(staff.Email).Result;
            var mappedStaff = Mapper.Map<OutputDto_EditStaff>(staff);
            mappedStaff.OriginalUserName = staff.Email;
            mappedStaff.StaffSchools = new List<Dto_StaffSchool>();  // TODO: empty out the staffschool collection (perhaps modify the DtoMappings to not populate)

            if(identityUser == null)
            {
                mappedStaff.CanLogIn = true;
            }
            else
            {
                mappedStaff.CanLogIn = ((identityUser.LockoutEndDateUtc < DateTime.Now || identityUser.LockoutEndDateUtc == null) && identityUser.LockoutEnabled) || identityUser.LockoutEnabled == false;
            }
            // TODO: first, get the schools that the CURRENT USER has admin access to
            // then use that list to create the StaffSchools collection
            // when saving, only delete schools that were being "actively modified"... 
            // i.e. those that are already in the collection

            // get all schools in the district and combine with staffschools
            //var allSchools = _dbContext.Schools.ToList();
            foreach (var school in schoolsUserIsAdminAt)
            {
                var existingRights = _dbContext.StaffSchools.FirstOrDefault(p => p.StaffID == staff.Id && p.SchoolID == school.Id);
                var gradesForSchool = _dbContext.StaffSchoolGrades.Where(p => p.SchoolID == school.Id && p.StaffID == staff.Id).Select(p => new OutputDto_DropdownData { id = p.GradeID, text = p.Grade.ShortName }).ToList();

                if (existingRights != null)
                {
                    mappedStaff.StaffSchools.Add(new Dto_StaffSchool()
                    {
                        SchoolID = school.Id,
                        SchoolName = school.Name,
                        StaffID = staff.Id,
                        StaffHierarchyPermissionID = existingRights.StaffHierarchyPermissionID,
                        Grades = gradesForSchool,
                        IsSchoolContact = existingRights.IsSchoolContact
                    });
                }
                else
                {
                    mappedStaff.StaffSchools.Add(new Dto_StaffSchool()
                    {
                        SchoolID = school.Id,
                        SchoolName = school.Name,
                        StaffID = staff.Id,
                        StaffHierarchyPermissionID = 0, // no access
                        IsSchoolContact = false
                    });
                }
            }

            mappedStaff.StaffSchools = mappedStaff.StaffSchools.OrderBy(p => p.SchoolName).ToList();

            return mappedStaff;
        }

        public static List<OutputDto_EditStaff> GetAllStaff(DistrictContext dbContext)
        {
            List<OutputDto_EditStaff> allStaff = new List<OutputDto_EditStaff>();

            var allDbStaff = dbContext.Staffs.Where(p => p.IsSA != true).ToList();
            var schoolsUserIsAdminAt = new List<School>();
            schoolsUserIsAdminAt = dbContext.Schools.ToList();

            foreach (var staff in allDbStaff)
            {
                var mappedStaff = Mapper.Map<OutputDto_EditStaff>(staff);
                mappedStaff.OriginalUserName = staff.Email;
                mappedStaff.StaffSchools = new List<Dto_StaffSchool>();  // TODO: empty out the staffschool collection (perhaps modify the DtoMappings to not populate)

                // TODO: first, get the schools that the CURRENT USER has admin access to
                // then use that list to create the StaffSchools collection
                // when saving, only delete schools that were being "actively modified"... 
                // i.e. those that are already in the collection

                // get all schools in the district and combine with staffschools
                //var allSchools = _dbContext.Schools.ToList();
                foreach (var school in schoolsUserIsAdminAt)
                {
                    var existingRights = dbContext.StaffSchools.FirstOrDefault(p => p.StaffID == staff.Id && p.SchoolID == school.Id);
                    var gradesForSchool = dbContext.StaffSchoolGrades.Where(p => p.SchoolID == school.Id && p.StaffID == staff.Id).Select(p => new OutputDto_DropdownData { id = p.GradeID, text = p.Grade.ShortName }).ToList();

                    if (existingRights != null)
                    {
                        mappedStaff.StaffSchools.Add(new Dto_StaffSchool()
                        {
                            SchoolID = school.Id,
                            SchoolName = school.Name,
                            StaffID = staff.Id,
                            StaffHierarchyPermissionID = existingRights.StaffHierarchyPermissionID,
                            Grades = gradesForSchool,
                            IsSchoolContact = existingRights.IsSchoolContact
                        });
                    }
                    else
                    {
                        mappedStaff.StaffSchools.Add(new Dto_StaffSchool()
                        {
                            SchoolID = school.Id,
                            SchoolName = school.Name,
                            StaffID = staff.Id,
                            StaffHierarchyPermissionID = 0, // no access
                            IsSchoolContact = false
                        });
                    }
                }

                mappedStaff.StaffSchools = mappedStaff.StaffSchools.OrderBy(p => p.SchoolName).ToList();
                allStaff.Add(mappedStaff);
            }

            return allStaff;
        }

        public static DataTable ConvertStaffToDataTable(List<OutputDto_EditStaff> allStaff, DistrictContext dbContext)
        {
            UserStoreManager mgr = new UserStoreManager(ConfigurationManager.ConnectionStrings["LoginConnection"].ConnectionString);

            DataTable table = new DataTable();

            // add standard attributes
            table.Columns.Add("Last Name", typeof(string));
            table.Columns.Add("First Name", typeof(string));
            table.Columns.Add("Middle Name", typeof(string));
            table.Columns.Add("Email", typeof(string));
            table.Columns.Add("ID", typeof(string));
            table.Columns.Add("Notes", typeof(string));
            table.Columns.Add("Role", typeof(string));
            table.Columns.Add("Is Active", typeof(string));
            table.Columns.Add("Can Login", typeof(string));
            table.Columns.Add("Is District Administrator", typeof(string));
            table.Columns.Add("Is District Contact", typeof(string));
            table.Columns.Add("Is Intervention Specialist", typeof(string));

            // add column for adminstrator at each school
            foreach (var school in dbContext.Schools)
            {
                table.Columns.Add(school.Name + "-Administrator");
                table.Columns.Add(school.Name + "-School Contact");

                foreach (var grade in dbContext.Grades.Where(p => p.ShortName != "None").OrderBy(p => p.GradeOrder))
                {
                    table.Columns.Add(school.Name + "-" + grade.ShortName + "-Full Grade Access");
                }
            }


            // add a column for each grade at teach school

            foreach (var staff in allStaff)
            {
                var canLogin = "N";
                NorthStarUser identityUser = null;
                var email = "No email address found";
                if (!String.IsNullOrWhiteSpace(staff.LoweredUserName))
                {
                    email = staff.LoweredUserName;
                    identityUser = mgr.GetUserByEmail(email).Result;
                }
                else
                {
                    if (!String.IsNullOrWhiteSpace(staff.Email))
                    {
                        email = staff.Email;
                        identityUser = mgr.GetUserByEmail(email).Result;
                    }
                }


                if (identityUser != null)
                {
                    if (((identityUser.LockoutEndDateUtc < DateTime.Now || identityUser.LockoutEndDateUtc == null) && identityUser.LockoutEnabled) || identityUser.LockoutEnabled == false)
                    {
                        canLogin = "Y";
                    }
                    else
                    {
                        canLogin = "N";
                    }
                }

                DataRow row = table.NewRow();
                row["Last Name"] = staff.LastName;
                row["First Name"] = staff.FirstName;
                row["Middle Name"] = staff.MiddleName;
                row["Email"] = email;
                row["ID"] = staff.TeacherIdentifier;
                row["Notes"] = staff.Notes;
                row["Role"] = GetRole(staff.RoleID);
                row["Is Active"] = staff.IsActive.HasValue && staff.IsActive.Value == false ? "N" : "Y";
                row["Can Login"] = canLogin;
                row["Is District Administrator"] = staff.IsDistrictAdmin == false ? "N" : "Y";
                row["Is District Contact"] = staff.IsDistrictContact.HasValue && staff.IsDistrictContact.Value == true ? "Y" : "N";
                row["Is Intervention Specialist"] = staff.IsInterventionSpecialist == false ? "N" : "Y";

                foreach (var staffSchool in staff.StaffSchools)
                {
                    var school = staffSchool.SchoolName;
                    var isSchoolAdmin = staffSchool.StaffHierarchyPermissionID == 1 ? "Y" : "N";
                    var isSchoolContact = staffSchool.IsSchoolContact.HasValue && staffSchool.IsSchoolContact == true ? "Y" : "N";

                    row[school + "-Administrator"] = isSchoolAdmin;
                    row[school + "-School Contact"] = isSchoolContact;

                    foreach (var staffGrade in staffSchool.Grades.Where(p => p.text != "None"))
                    {
                        var gradeName = staffGrade.text;

                        row[school + "-" + gradeName + "-Full Grade Access"] = "Y";
                    }
                }

                table.Rows.Add(row);
            }

            return table;
        }

        private static string GetRole(int roleID)
        {
            if(roleID == 1)
            {
                return "Teacher";
            }
            else if (roleID == 2)
            {
                return "Teaching Assistant";
            }
            else if (roleID == 5)
            {
                return "Administrator";
            }
            else 
            {
                return "Other";
            }

        }

        public Staff GetStaffUnvalidated(int id)
        {
            return _dbContext.Staffs.First(p => p.Id == id);
        }

        public OutputDto_SuccessAndStatus ChangeUserLogin(string newUserName, string oldUsername)
        {
            using (System.Data.IDbCommand command = _loginContext.Database.Connection.CreateCommand())
            {
                _loginContext.Database.Connection.Open();
                command.CommandText = "changeUserName";
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.CommandTimeout = command.Connection.ConnectionTimeout;
                command.Parameters.Add(new SqlParameter("@newusername", newUserName));
                command.Parameters.Add(new SqlParameter("@oldusername", oldUsername));

                command.ExecuteNonQuery();

                return new OutputDto_SuccessAndStatus { isValid = true, Status = new OutputDto_Status { StatusCode = StatusCode.Ok } };
            }
        }



        public OutputDto_SuccessAndStatus ConsolidateStaff(InputDto_ConsolidateStaff input)
        {
            // consolidate user accounts, in case there is more than one being used
            using (System.Data.IDbCommand command = _loginContext.Database.Connection.CreateCommand())
            {
                _loginContext.Database.Connection.Open();
                command.CommandText = "ns4_ConsolidateAccounts";
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.CommandTimeout = command.Connection.ConnectionTimeout;
                command.Parameters.Add(new SqlParameter("@primaryStaffEmail", input.PrimaryStaffId.Email));
                command.Parameters.Add(new SqlParameter("@secondaryStaffEmail", input.SecondaryStaffId.Email));

                command.ExecuteNonQuery();

            }

            using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
            {
                _dbContext.Database.Connection.Open();
                command.CommandText = "ns4_ConsolidateStaff";
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.CommandTimeout = command.Connection.ConnectionTimeout;
                command.Parameters.Add(new SqlParameter("@primaryStaffId", input.PrimaryStaffId.id));
                command.Parameters.Add(new SqlParameter("@secondaryStaffId", input.SecondaryStaffId.id));

                command.ExecuteNonQuery();

            }

            return new OutputDto_SuccessAndStatus { isValid = true, Status = new OutputDto_Status { StatusCode = StatusCode.Ok } };

        }

        public string CreatePassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }

        public OutputDto_Base SaveStaff(OutputDto_EditStaff staff, out bool isNewUser, out bool userNameChanged, out string newPassword)
        {
            newPassword = "";
            isNewUser = false;
            userNameChanged = false;

            // TODO: SECURITY CHECKS!!
            var result = new OutputDto_Base();
            Staff dbStaff = null;

            // if this is a new staff
            if (staff.Id <= 0)
            {
                // make sure this isn't a resubmission
                dbStaff = _dbContext.Staffs.FirstOrDefault(m => m.Email.Equals(staff.Email, StringComparison.CurrentCultureIgnoreCase));

                if(dbStaff == null)
                {
                    isNewUser = true;
                    dbStaff = _dbContext.Staffs.Add(new Staff());
                    _dbContext.SaveChanges();
                }
            }
            else
            {
                dbStaff = _dbContext.Staffs.First(m => m.Id == staff.Id);
            }

            dbStaff.FirstName = staff.FirstName;
            dbStaff.LastName = staff.LastName;
            dbStaff.RoleID = staff.RoleID;
            dbStaff.TeacherIdentifier = staff.TeacherIdentifier;
            dbStaff.IsActive = staff.IsActive;
            dbStaff.IsInterventionSpecialist = staff.IsInterventionSpecialist;
            dbStaff.Notes = staff.Notes;
            dbStaff.IsDistrictAdmin = staff.IsDistrictAdmin;
            dbStaff.IsDistrictContact = staff.IsDistrictContact;
            dbStaff.ModifiedBy = _currentUser.Email;
            dbStaff.ModifiedDate = DateTime.Now;

            UserStoreManager mgr = new UserStoreManager(_loginConnectionString);

            // if username was changed for Existing User
            if (staff.OriginalUserName != staff.Email && !isNewUser)
            {
                var loginSuccess = ChangeUserLogin(staff.Email, staff.OriginalUserName);
                if (loginSuccess.Status.StatusCode == StatusCode.Ok)
                {
                    dbStaff.LoweredUserName = staff.Email;
                    dbStaff.Email = staff.Email;
                    _dbContext.SaveChanges();
                    userNameChanged = true;
                    // make sure to save login changes
                }
            } else if(isNewUser)
            {
                // this is a new user, create a new user account
                newPassword = CreatePassword(8);
                mgr.CreateUser(staff.Email, newPassword);
                // now give user district access
                StaffDistrict sd = new StaffDistrict { DistrictId = _currentUser.DistrictId, StaffEmail = staff.Email };
                _loginContext.StaffDistricts.Add(sd);
                _loginContext.SaveChanges();

                dbStaff.LoweredUserName = staff.Email;
                dbStaff.Email = staff.Email;
                _dbContext.SaveChanges();
            }

            // lock and unlock accounts
            // changed this on 10/4/20 to not work on IsActive
            if (staff.CanLogIn == false)
            {
                mgr.LockUserAccount(staff.Email);
            } else
            {
                mgr.UnLockUserAccount(staff.Email);
            }

            // security access... only update the schools current user has admin access to
            // TODO: Put this in a filter somewhere
            var adminStaffSchoolsForCurrentUser = _dbContext.StaffSchools.Where(p => p.StaffID == _currentUser.Id && p.StaffHierarchyPermissionID == 1);
            var schoolsUserIsAdminAt =  IsDistrictAdmin ? _dbContext.Schools.ToList() : _dbContext.Schools.Where(p => adminStaffSchoolsForCurrentUser.Any(j => j.SchoolID == p.Id)).ToList();

            foreach (var staffSchool in staff.StaffSchools)
            {
                // if current user has access to this school : Hack check
                if (schoolsUserIsAdminAt.Any(p => p.Id == staffSchool.SchoolID))
                {
                    // see if already in DB, and update if so, add if not
                    var existingStaffSchool = _dbContext.StaffSchools.FirstOrDefault(p => p.SchoolID == staffSchool.SchoolID && p.StaffID == staffSchool.StaffID);

                    if (existingStaffSchool == null)
                    {
                        existingStaffSchool = new StaffSchool { StaffID = dbStaff.Id, SchoolID = staffSchool.SchoolID, StaffHierarchyPermissionID = staffSchool.StaffHierarchyPermissionID, IsSchoolContact = staffSchool.IsSchoolContact };
                        _dbContext.StaffSchools.Add(existingStaffSchool);
                    }
                    else
                    {
                        existingStaffSchool.StaffHierarchyPermissionID = staffSchool.StaffHierarchyPermissionID;
                        existingStaffSchool.IsSchoolContact = staffSchool.IsSchoolContact;
                    }

                    // add grades
                    var existingStaffSchoolGrades = _dbContext.StaffSchoolGrades.Where(p => p.SchoolID == staffSchool.SchoolID && p.StaffID == dbStaff.Id).ToList();

                    if (existingStaffSchoolGrades.Count > 0)
                    {
                        _dbContext.StaffSchoolGrades.RemoveRange(existingStaffSchoolGrades);
                    }
                    if (staffSchool.StaffHierarchyPermissionID == 3) // grade level
                    {
                        foreach (var grade in staffSchool.Grades)
                        {
                            _dbContext.StaffSchoolGrades.Add(new StaffSchoolGrade() { GradeID = grade.id, SchoolID = staffSchool.SchoolID, StaffID = dbStaff.Id, StaffHierarchyPermissionID = 1 });
                        }
                    }
                }
            }

            _dbContext.SaveChanges();
            return result;
        }

        public OutputDto_GetAssessmentsAndFieldsForUser GetAssessmentsAndFieldsForUser()
        {
            var assessments = _dbContext.Assessments //.Where(p => p.Enabled)
                .Include(p => p.Fields);
            //

            // stupid hack b/c EF6 doesn't allow filtering on Included collections
            foreach (var assessment in assessments)
            {
                var fieldsToUpdate = _dbContext.StaffAssessmentFieldVisibilities.Where(p => p.StaffId == _currentUser.Id && p.AssessmentId == assessment.Id);
                assessment.Fields = assessment.Fields.Where(p => (p.DisplayInObsSummary.HasValue == true && p.DisplayInObsSummary.Value == true) ||
                (p.DisplayInEditResultList.HasValue == true && p.DisplayInEditResultList.Value == true) ||
                (p.DisplayInLineGraphs.HasValue == true && p.DisplayInLineGraphs.Value == true)).ToList();

                // loop over all the fields that "should" be included
                foreach (var field in assessment.Fields)
                {
                    var userField = fieldsToUpdate.FirstOrDefault(p => p.AssessmentFieldId == field.Id);

                    if (userField != null)
                    {
                        field.DisplayInObsSummary = userField.DisplayInObsSummary == null ? field.DisplayInObsSummary : userField.DisplayInObsSummary;
                        field.DisplayInLineGraphs = userField.DisplayInLineGraphs == null ? field.DisplayInLineGraphs : userField.DisplayInLineGraphs;
                        field.DisplayInEditResultList = userField.DisplayInEditResultList == null ? field.DisplayInEditResultList : userField.DisplayInEditResultList;
                    }
                }
            }


            return new OutputDto_GetAssessmentsAndFieldsForUser
            {
                Assessments = Mapper.Map<List<AssessmentDto>>(assessments)
            };
        }

        public OutputDto_GetAssessmentsAndFieldsForUser GetAssessmentAndFieldsForUser(int assessmentId)
        {
            var assessment = _dbContext.Assessments.Include(p => p.Fields).First(p => p.Id == assessmentId); //.Where(p => p.Enabled)

            //

            // stupid hack b/c EF6 doesn't allow filtering on Included collections
            var fieldsToUpdate = _dbContext.StaffAssessmentFieldVisibilities.Where(p => p.StaffId == _currentUser.Id && p.AssessmentId == assessment.Id);
            assessment.Fields = assessment.Fields.Where(p => (p.DisplayInObsSummary.HasValue == true && p.DisplayInObsSummary.Value == true) ||
            (p.DisplayInEditResultList.HasValue == true && p.DisplayInEditResultList.Value == true) ||
            (p.DisplayInLineGraphs.HasValue == true && p.DisplayInLineGraphs.Value == true)).ToList();

            // loop over all the fields that "should" be included
            foreach (var field in assessment.Fields)
            {
                var userField = fieldsToUpdate.FirstOrDefault(p => p.AssessmentFieldId == field.Id);

                if (userField != null)
                {
                    field.DisplayInObsSummary = userField.DisplayInObsSummary == null ? field.DisplayInObsSummary : userField.DisplayInObsSummary;
                    field.DisplayInLineGraphs = userField.DisplayInLineGraphs == null ? field.DisplayInLineGraphs : userField.DisplayInLineGraphs;
                    field.DisplayInEditResultList = userField.DisplayInEditResultList == null ? field.DisplayInEditResultList : userField.DisplayInEditResultList;
                }
            }


            return new OutputDto_GetAssessmentsAndFieldsForUser
            {
                Assessments = Mapper.Map<List<AssessmentDto>>(new List<Assessment>() { assessment })
            };
        }

        public OutputDto_SuccessAndStatus UpdateFieldForUser(InputDto_UpdateFieldForUser input)
        {
            var fieldToUpdate = _dbContext.StaffAssessmentFieldVisibilities.FirstOrDefault(p => p.StaffId == _currentUser.Id && p.AssessmentId == input.AssessmentId && p.AssessmentFieldId == input.FieldId);

            if (fieldToUpdate == null)
            {
                fieldToUpdate = new StaffAssessmentFieldVisibility()
                {
                    StaffId = _currentUser.Id,
                    AssessmentFieldId = input.FieldId,
                    AssessmentId = input.AssessmentId
                };
                _dbContext.StaffAssessmentFieldVisibilities.Add(fieldToUpdate);
            }

            switch (input.HideFieldFrom)
            {
                case "observationsummary":
                    fieldToUpdate.DisplayInObsSummary = input.HiddenStatus;
                    break;
                case "editresults":
                    fieldToUpdate.DisplayInEditResultList = input.HiddenStatus;
                    break;
                case "linegraphs":
                    fieldToUpdate.DisplayInLineGraphs = input.HiddenStatus;
                    break;
            }

            _dbContext.SaveChanges();
            return new OutputDto_SuccessAndStatus
            {
                isValid = true,
                value = null
            };
        }
    }
}
