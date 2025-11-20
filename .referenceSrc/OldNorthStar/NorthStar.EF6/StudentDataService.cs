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
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using Serilog;
using EntityDto.DTO.Admin.InterventionGroup;
using NorthStar.EF6.Infrastructure;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;
using static NorthStar.Core.NSConstants;
using Northstar.Core.Extensions;
using EntityDto.LoginDB.Entity;

namespace NorthStar.EF6
{
    public class StudentDataService : NSBaseDataService
    {
        private string _imageContainer = "images";
        private CloudBlobContainer _container = null;
        private CloudBlobClient _client = null;
        private CloudStorageAccount _storageAccount;
        private District _district;
        public StudentDataService(ClaimsIdentity user, string loginConnectionString) : base(user, loginConnectionString)
        {
            _storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);

            _district = _loginContext.Districts.First(p => p.Id == _currentUser.DistrictId);

            if (!String.IsNullOrEmpty(_district.AzureContainerName))
            {
                _imageContainer = _district.AzureContainerName;
                //photoManager.ContainerName = _imageContainer;

                _client = _storageAccount.CreateCloudBlobClient();
                _container = _client.GetContainerReference(_imageContainer);
            }
        }

        public OutputDto_Base DeleteStudent(int id)
        {
            var result = new OutputDto_Base();

            var db_student = _dbContext.Students.FirstOrDefault(p => p.Id == id);
            if (db_student == null)
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "This student no longer exists.";
                return result;
            }

            // call stored proc to see if data exists
            var dataCheck = false;
            if (dataCheck)
            {
                throw new UserDisplayableException("This student has data entered.  Please archive the student instead of deleting him or her.", null);
            }

            // check if student is in classes
            if (db_student.StudentSections.Count > 0)
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "This student is registered in one or more classes.  Please archive the student instead of deleting him or her.";
                return result;
            }

            // try deletion after we've checked for everything we can think of
            // you can only delete a student with no classes or data
            try
            {
                _dbContext.StudentSchools.RemoveRange(db_student.StudentSchools);
                _dbContext.Students.Remove(db_student);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Log.Warning("Someone is trying to delete the student {0}, {1} but is getting the following error: {2}", db_student.LastName, db_student.FirstName, ex.Message);
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "An error occurred while deleting this student.  The student likely has data or other refrences that prevent deletion.  Please archive the student instead of deleting him or her.";
                return result;
            }

            return result;
        }

        public OutputDto_Base MoveStudent(InputDto_MoveStudent input)
        {
            // TODO: security checks

            // SHOULD probably call a proc so that i can use transactions??

            var result = new OutputDto_Base();

            var currentStudentInfo = input.Student;
            var newSectionInfo = input.Section;
                        
            // first see if student is registered at target school, if not, add to the school
            if(!_dbContext.StudentSchools.Any(p => p.SchoolID == newSectionInfo.SchoolId && p.SchoolStartYear == newSectionInfo.SchoolStartYear && p.StudentID == currentStudentInfo.StudentId)) // no need to check for grade
            {
                var newStudentSchoolRecord = new StudentSchool { StudentID = currentStudentInfo.StudentId, SchoolStartYear = newSectionInfo.SchoolStartYear, SchoolID = newSectionInfo.SchoolId, GradeId =  newSectionInfo.GradeId};
                _dbContext.StudentSchools.Add(newStudentSchoolRecord);
            }

            // see if student is already in this class, if not add him
            if (!_dbContext.StudentSections.Any(p => p.ClassID == newSectionInfo.id && p.StudentID == currentStudentInfo.StudentId)) 
            {
                var newStudentSectionRecord = new StudentSection { StudentID = currentStudentInfo.StudentId, ClassID = newSectionInfo.id };
                _dbContext.StudentSections.Add(newStudentSectionRecord);
            }

            // remove student from section... save changes
            var sectionToRemove = _dbContext.StudentSections.FirstOrDefault(p => p.ClassID == currentStudentInfo.SectionId && p.StudentID == currentStudentInfo.StudentId);
            if(sectionToRemove != null)
            {
                _dbContext.StudentSections.Remove(sectionToRemove);
            }
            _dbContext.SaveChanges();

            //if no more sections at previous school, for that YEAR, remove school registration
            var anySectionsAtSchool = _dbContext.StudentSections.Any(p => p.StudentID == currentStudentInfo.StudentId && p.Section.SchoolID == currentStudentInfo.SchoolId && p.Section.SchoolStartYear == currentStudentInfo.SchoolStartYear);
            if(!anySectionsAtSchool)
            {
                // remove school registration
                var schoolRegistration = _dbContext.StudentSchools.FirstOrDefault(p => p.SchoolID == currentStudentInfo.SchoolId && p.SchoolStartYear == currentStudentInfo.SchoolStartYear && p.StudentID == currentStudentInfo.StudentId);
                if(schoolRegistration != null)
                {
                    _dbContext.StudentSchools.Remove(schoolRegistration);
                    _dbContext.SaveChanges();
                }
            }

            return result;
        }

        public List<StudentQuickSearchResult> GetStudentQuickSearchResults(string strSearch, bool disableInactiveStudents)
        {
            // TODO: security check... maybe in PROC?
            // how to do security check... need to do "if district admin, etc... later"

            if (String.IsNullOrEmpty(strSearch))
            {
                return new List<StudentQuickSearchResult>();
            }

            var results = _dbContext.Database.SqlQuery<StudentQuickSearchResult>("EXEC nset_StudentQuickSearch @StaffID, @SearchString, @IsAdmin, @disableInactiveStudents",
                new SqlParameter("StaffID", _currentUser.Id),
                new SqlParameter("SearchString", strSearch),
                new SqlParameter("IsAdmin", IsDistrictAdmin),
                new SqlParameter("disableInactiveStudents", disableInactiveStudents));

            return results.ToList();
        }

        public List<StudentDetailedQuickSearchResult> GetStudentDetailedQuickSearchResults(string strSearch, bool disableInactiveStudents)
        {
            List<StudentDetailedQuickSearchResult> sections = new List<StudentDetailedQuickSearchResult>();
            // TODO: security check... maybe in PROC?
            //var students = _dbContext.Students.Where(p => p.LastName.Contains(strSearch) || p.FirstName.Contains(strSearch) || p.MiddleName.Contains(strSearch) || p.StudentIdentifier.Contains(strSearch))
            //                .OrderBy(p => p.LastName).ThenBy(p => p.FirstName).Take(10).ToList();

            // get the kids... then get details
            var students = GetStudentQuickSearchResults(strSearch, disableInactiveStudents);

            students.Each(p =>
            {
                var newSections = _dbContext.StudentSections.Where(j => j.StudentID == p.id).Select(r => new StudentDetailedQuickSearchResult()
                {
                    id = p.id,
                    StudentId = p.id,
                    FirstName = p.FirstName,
                    MiddleName = p.MiddleName,
                    LastName = p.LastName,
                    DOB = p.DOB, // TODO: make this a datetime.  only Detroit lakes and pequot have any kids with no DOB...
                    IsActive = p.IsActive,
                    StudentIdentifier = p.StudentIdentifier,
                    disabled = p.disabled,
                    SchoolId = r.Section.SchoolID,
                    SchoolName = r.Section.School.Name,
                    SchoolStartYear = r.Section.SchoolStartYear,
                    SchoolYearVerbose = r.Section.SchoolYear.YearVerbose,
                    GradeId = r.Section.GradeID,
                    GradeName = r.Section.Grade.ShortName, // TODO: this should come from student, not section
                    SectionId = r.Section.Id,
                    SectionName = r.Section.Name,
                    StaffId = r.Section.StaffID,
                    StaffName = r.Section.Staff.LastName + ", " + r.Section.Staff.FirstName
                }).OrderByDescending(n => n.SchoolStartYear).ToList();

                sections.AddRange(newSections);
            });

            return sections;
        }//GetStudentDetailedById

        public List<StudentDetailedQuickSearchResult> GetStudentDetailedQuickSearchResultsCurrentYear(string strSearch, bool disableInactiveStudents)
        {
            List<StudentDetailedQuickSearchResult> sections = new List<StudentDetailedQuickSearchResult>();
            // TODO: security check... maybe in PROC?
            //var students = _dbContext.Students.Where(p => p.LastName.Contains(strSearch) || p.FirstName.Contains(strSearch) || p.MiddleName.Contains(strSearch) || p.StudentIdentifier.Contains(strSearch))
            //                .OrderBy(p => p.LastName).ThenBy(p => p.FirstName).Take(10).ToList();

            // get the kids... then get details
            var students = GetStudentQuickSearchResults(strSearch, disableInactiveStudents);
            var currentSchoolYear = GetDefaultYear();

            students.Each(p =>
            {
                var newSections = _dbContext.StudentSections.Where(j => j.StudentID == p.id && j.Section.SchoolStartYear == currentSchoolYear).Select(r => new StudentDetailedQuickSearchResult()
                {
                    id = p.id,
                    StudentId = p.id,
                    FirstName = p.FirstName,
                    MiddleName = p.MiddleName,
                    LastName = p.LastName,
                    DOB = p.DOB, // TODO: make this a datetime.  only Detroit lakes and pequot have any kids with no DOB...
                    IsActive = p.IsActive,
                    StudentIdentifier = p.StudentIdentifier,
                    disabled = p.disabled,
                    SchoolId = r.Section.SchoolID,
                    SchoolName = r.Section.School.Name,
                    SchoolStartYear = r.Section.SchoolStartYear,
                    SchoolYearVerbose = r.Section.SchoolYear.YearVerbose,
                    GradeId = r.Section.GradeID,
                    GradeName = r.Section.Grade.ShortName, // TODO: this should come from student, not section
                    SectionId = r.Section.Id,
                    SectionName = r.Section.Name,
                    StaffId = r.Section.StaffID,
                    StaffName = r.Section.Staff.LastName + ", " + r.Section.Staff.FirstName
                }).OrderByDescending(n => n.SchoolStartYear).ToList();

                sections.AddRange(newSections);
            });

            return sections;
        }//GetStudentDetailedById


        public StudentQuickSearchResult GetStudentById(InputDto_SimpleId input)
        {            
            // TODO: security check... maybe in PROC?
            var student = _dbContext.Students.Where(p => p.Id == input.Id)
                .Select(p => new StudentQuickSearchResult()
            {
                id = p.Id,
                LastName = p.LastName,
                FirstName = p.FirstName,
                MiddleName = p.MiddleName,
                DOB = p.DOB.Value, // TODO: make this a datetime.  only Detroit lakes and pequot have any kids with no DOB...
                IsActive = p.IsActive.HasValue ? p.IsActive.Value : true,
                StudentIdentifier = p.StudentIdentifier,
                disabled = (p.IsActive.HasValue && p.IsActive.Value == false)

            }).ToList();

            return student.FirstOrDefault();
        }

        public class OutputDto_StudentSection
        {
            public string SectionName { get; set; }
            public string TeacherName { get; set; }
            public int StudentCount { get; set; }
        }

        public class OutputDto_StudentSections : OutputDto_Base
        {
            public OutputDto_StudentSections()
            {
                Sections = new List<OutputDto_StudentSection>();
            }
            public List<OutputDto_StudentSection> Sections { get; set; }
        }

        public OutputDto_StudentSections GetSectionsForYear(InputDto_StudentSchoolDto input)
        {
            var result = new OutputDto_StudentSections();

            var sections = _dbContext.StudentSections.Where(p => p.Section.SchoolStartYear == input.StudentSchool.SchoolStartYear && p.StudentID == input.StudentSchool.StudentId).Select(p => p.Section);
            sections.Each(p =>
            {
                result.Sections.Add(new OutputDto_StudentSection()
                {
                    StudentCount = p.StudentSections.Count(),
                    TeacherName = p.Staff.FullName,
                    SectionName = p.Name
                });
            });
            result.Sections = result.Sections.OrderBy(p => p.SectionName).ToList();
            return result;
        }

        // what combinations can you use to get a list of students?
        // 
        public OutputDto_GetStudentList GetStudentList(InputDto_GetStudentList input)
        {
            OutputDto_GetStudentList outputList = new OutputDto_GetStudentList();

            // kids for a specific section
            if (input.SectionId > 0)
            {
                outputList.Students = _dbContext.Students.Where(p => p.StudentSections.Any(j => j.ClassID == input.SectionId && p.IsActive != false))
                    .Select(r => new StudentListDto()
                    {
                        DOB = r.DOB.Value,
                        FirstName = r.FirstName,
                        MiddleName = r.MiddleName,
                        LastName = r.LastName,
                        Id = r.Id,
                        RegisteredForCurrentYear = r.StudentSchools.Any(j => j.SchoolStartYear == input.SchoolYear),
                        StudentIdentifier = r.StudentIdentifier
                    })
                    .OrderBy(s => s.LastName)
                    .ThenBy(s => s.FirstName)
                    .ToList();
            }
            else if (input.TeacherId > 0 && input.SchoolYear > 0) // all the kids for a teacher and year, irrespective of section
            {
                var studentSectionsForTeacher = _dbContext.StaffSections.Include(p => p.Section.StudentSections.Select(j => j.Student)).Where(p => p.StaffID == input.TeacherId && p.Section.SchoolStartYear == input.SchoolYear)
                    .Select(p => p.Section.StudentSections.Where(n => n.Student.IsActive != false).Select(j => j.Student)).ToList();

                var studentsAcrossSections = new List<StudentListDto>();
                foreach (var studentSectionList in studentSectionsForTeacher)
                {
                    foreach (var studentSection in studentSectionList)
                    {
                        if (!studentsAcrossSections.Exists(p => p.Id == studentSection.Id))
                        {
                            studentsAcrossSections.Add(new StudentListDto()
                            {
                                Id = studentSection.Id,
                                DOB = studentSection.DOB.Value,
                                LastName = studentSection.LastName,
                                MiddleName = studentSection.MiddleName,
                                FirstName = studentSection.FirstName,
                                RegisteredForCurrentYear = studentSection.StudentSchools.Any(j => j.SchoolStartYear == input.SchoolYear),
                                StudentIdentifier = studentSection.StudentIdentifier
                            });
                        }
                    }
                }
                outputList.Students = studentsAcrossSections
                                        .OrderBy(s => s.LastName)
                                        .ThenBy(s => s.FirstName)
                                        .ToList();
            }
            else if (input.GradeId > 0 && input.SchoolYear > 0 && input.SchoolId > 0) // all the kids for a certain grade at a school for a year
            {
                var studentSectionsForSchool = _dbContext.Sections.Where(p => p.GradeID == input.GradeId && input.SchoolYear == p.SchoolStartYear && p.SchoolID == input.SchoolId)
                    .Select(p => p.StudentSections.Where(n => n.Student.IsActive != false).Select(j => j.Student)).ToList();

                var studentsAcrossSections = new List<StudentListDto>();
                foreach (var studentSectionList in studentSectionsForSchool)
                {
                    foreach (var studentSection in studentSectionList)
                    {
                        if (!studentsAcrossSections.Exists(p => p.Id == studentSection.Id))
                        {
                            studentsAcrossSections.Add(new StudentListDto()
                            {
                                Id = studentSection.Id,
                                DOB = studentSection.DOB.Value,
                                LastName = studentSection.LastName,
                                MiddleName = studentSection.MiddleName,
                                FirstName = studentSection.FirstName,
                                RegisteredForCurrentYear = studentSection.StudentSchools.Any(j => j.SchoolStartYear == input.SchoolYear),
                                StudentIdentifier = studentSection.StudentIdentifier
                            });
                        }
                    }
                }
                outputList.Students = studentsAcrossSections
                                        .OrderBy(s => s.LastName)
                                        .ThenBy(s => s.FirstName)
                                        .ToList();
            }
            else if (input.SchoolId > 0 && input.SchoolYear > 0) // all the kids for a given school for a year
            {
                outputList.Students = _dbContext.StudentSchools.Where(p => input.SchoolYear == p.SchoolStartYear && p.SchoolID == input.SchoolId && p.Student.IsActive != false)
                    .Select(p => p.Student)
                    .Select(r => new StudentListDto()
                    {
                        DOB = r.DOB.Value,
                        FirstName = r.FirstName,
                        MiddleName = r.MiddleName,
                        LastName = r.LastName,
                        Id = r.Id,
                        RegisteredForCurrentYear = r.StudentSchools.Any(j => j.SchoolStartYear == input.SchoolYear),
                        StudentIdentifier = r.StudentIdentifier
                    })
                    .OrderBy(s => s.LastName)
                    .ThenBy(s => s.FirstName)
                    .ToList();
            }

            outputList.Students.Each(p =>
            {
                p.DOBText = p.DOB.ToString("dd-MMM-yyyy");
            });

            return outputList;
        }

        public bool SaveStudent(OutputDto_ManageStudent student)
        {
            // determine if this is a new section or not
            var db_student = _dbContext.Students
                .Include(p => p.StudentSections)
                .FirstOrDefault(p => p.Id == student.Id);

            if (db_student == null)
            {
                db_student = new Student()
                {
                    FirstName = student.FirstName,
                    MiddleName = student.MiddleName,
                    LastName = student.LastName,
                    DOB = student.DOB,
                    StudentIdentifier = student.StudentIdentifier,
                    GradYear = student.GraduationYear,
                    IsActive = student.IsActive
                };

                _dbContext.Students.Add(db_student);
            }
            else
            {
                db_student.FirstName = student.FirstName;
                db_student.MiddleName = student.MiddleName;
                db_student.LastName = student.LastName;
                db_student.DOB = student.DOB;
                db_student.StudentIdentifier = student.StudentIdentifier;
                db_student.GradYear = student.GraduationYear;
                db_student.IsActive = student.IsActive;
            }
            _dbContext.SaveChanges();

            #region StudentSchools
            var studentSchoolsToDelete = new List<StudentSchool>();
            // remove deleted studentschools, studentSchoolsToDeletes that are attached to db_student but not in student

            // TODO: security check to make sure that the current user has access to delete these sections
            db_student.StudentSchools
                    .Where(d => !student.StudentSchools.Any(ct => ct.Id == d.Id))
                    .Each(deleted => studentSchoolsToDelete.Add(deleted));

            _dbContext.StudentSchools.RemoveRange(studentSchoolsToDelete);

            //update or add new studentschools
            student.StudentSchools.Each(ct =>
            {
                // check to see if this is an existing co-teacher record
                var studentSchool = db_student.StudentSchools.FirstOrDefault(d => d.Id == ct.Id);
                if (studentSchool == null)
                {
                    studentSchool = new StudentSchool();
                    db_student.StudentSchools.Add(studentSchool);
                }
                studentSchool.SchoolID = ct.SchoolId;
                studentSchool.StudentID = db_student.Id;
                studentSchool.SchoolStartYear = ct.SchoolStartYear;
                studentSchool.GradeId = ct.GradeId;
            });
            _dbContext.SaveChanges();
            #endregion

            #region SPEDLabels
            // remove deleted SPEDLabels, spedlabels that are attached to db_student but not in section
            var studentSpedLabelsToDelete = new List<StudentAttributeData>();
            db_student.StudentAttributeDatas
                    .Where(d => !student.SpecialEdLabels.Any(st => st.id == d.AttributeValueID && d.AttributeID == 4 && d.StudentID == db_student.Id))
                    .Each(deleted => studentSpedLabelsToDelete.Add(deleted));
            _dbContext.StudentAttributeDatas.RemoveRange(studentSpedLabelsToDelete);

            //update or add new staffsections
            student.SpecialEdLabels.Each(st =>
            {
                // check to see if this is an existing spedlabel record
                var spedlabel = db_student.StudentAttributeDatas.FirstOrDefault(d => d.StudentID == db_student.Id && d.AttributeID == 4 && d.AttributeValueID == st.id);
                if (spedlabel == null)
                {
                    spedlabel = new StudentAttributeData();
                    db_student.StudentAttributeDatas.Add(spedlabel);
                }
                spedlabel.StudentID = db_student.Id;
                spedlabel.AttributeID = 4;
                spedlabel.AttributeValueID = st.id;
            });
            _dbContext.SaveChanges();
            #endregion

            #region StudentAttributes
            // remove deleted SPEDLabels, spedlabels that are attached to db_student but not in section
            var studentAttributesToDelete = new List<StudentAttributeData>();
            var properties = student.StudentAttributes.Properties();
            var attributeList = new List<StudentAttributeData>();

            /// Convert JObject Attribute list to normal attributedata list for ease of comparisons
            foreach (var prop in properties)
            {
                var attributeId = Int32.Parse(prop.Name);
                var attributeValueId = ((JValue)prop.Value).Value == null ? -1 : Int32.Parse(((JValue)prop.Value).Value.ToString());

                attributeList.Add(new StudentAttributeData() { AttributeValueID = attributeValueId, AttributeID = attributeId, StudentID = db_student.Id });
            }

            // remove attributes that have been removed client side
            db_student.StudentAttributeDatas
                    .Where(d => !attributeList.Any(st => (st.AttributeValueID == -1 && d.AttributeID == st.AttributeID && d.StudentID == db_student.Id)) && d.AttributeID != 4)
                    .Each(deleted => studentAttributesToDelete.Add(deleted));
            _dbContext.StudentAttributeDatas.RemoveRange(studentAttributesToDelete);

            //update or add new attributes
            attributeList.Each(st =>
            {
                // check to see if this is an existing attribute record
                var studentAttribute = db_student.StudentAttributeDatas.FirstOrDefault(d => d.StudentID == db_student.Id && d.AttributeID == st.AttributeID && d.AttributeValueID == st.AttributeValueID);
                if (studentAttribute == null)
                {
                    studentAttribute = new StudentAttributeData();
                    db_student.StudentAttributeDatas.Add(studentAttribute);
                }
                studentAttribute.StudentID = student.Id;
                studentAttribute.AttributeID = st.AttributeID;
                studentAttribute.AttributeValueID = st.AttributeValueID;
            });
            _dbContext.SaveChanges();
            #endregion
            return true;
        }

        public OutputDto_StudentAttributeLookups GetStudentAttributeLookups()
        {
            OutputDto_StudentAttributeLookups output = new OutputDto_StudentAttributeLookups();

            var attributeTypes = _dbContext.StudentAttributeTypes
                                    .Include(x => x.LookupValues)
                                    .Where(j => j.Id != 4)
                                    .OrderBy(j => j.AttributeName)
                                    .ToList();

            output.AllAttributes = Mapper.Map<List<StudentAttributeTypeDto>>(attributeTypes);

            return output;
        }

        public static OutputDto_StudentAttributeLookups GetStudentAttributeLookups(DistrictContext dbContext)
        {
            OutputDto_StudentAttributeLookups output = new OutputDto_StudentAttributeLookups();

            var attributeTypes = dbContext.StudentAttributeTypes
                                    .Include(x => x.LookupValues)
                                    .Where(j => j.Id != 4)
                                    .OrderBy(j => j.AttributeName)
                                    .ToList();

            output.AllAttributes = Mapper.Map<List<StudentAttributeTypeDto>>(attributeTypes);

            return output;
        }

        public List<OutputDto_DropdownData> GetStudentSpedLabelLookups(string searchString)
        {
            List<OutputDto_DropdownData> output = new List<OutputDto_DropdownData>();

            var lookupValues = _dbContext.StudentAttributeLookupValues
                        .Where(j => j.AttributeID == 4 && (j.LookupValue.Contains(searchString) || j.Description.Contains(searchString) || String.IsNullOrEmpty(searchString)))
                        .OrderBy(j => j.LookupValue)
                        .ToList();

            output = Mapper.Map<List<OutputDto_DropdownData>>(lookupValues);

            return output;
        }



        public List<StudentServiceDto> GetStudentServices(string searchString)
        {
            List<StudentServiceDto> output = new List<StudentServiceDto>();

            var lookupValues = _dbContext.StudentAttributeLookupValues
                        .Where(j => j.AttributeID == 4 && (j.LookupValue.Contains(searchString) || j.Description.Contains(searchString) || String.IsNullOrEmpty(searchString)))
                        .OrderBy(j => j.LookupValue)
                        .ToList();

            lookupValues.Each(p =>
            {
                output.Add(new StudentServiceDto
                {
                    id = p.LookupValueID.Value,
                    text = p.Description + " (" + p.LookupValue + ")",
                    Name = p.LookupValue,
                    Description = p.Description,
                    IsSpecEdLabel = p.IsSpecialEd
                });
            });

            return output;
        }

        public OutputDto_SuccessAndStatus IsStudentIDUnique(InputDto_StudentIdAndIdentifier input)
        {
            OutputDto_SuccessAndStatus output = new OutputDto_SuccessAndStatus();
            // TODO: TRIM studentidentifiers in the DB and tlowerthem when comparing

            // if it is the current student's ID or doesn't belong to anyone
            var studentWithId = _dbContext.Students.FirstOrDefault(p => p.StudentIdentifier == input.StudentIdentifier);
            if (studentWithId == null || studentWithId.Id == input.StudentId)
            {
                output.isValid = true;
            }
            else
            {
                output.isValid = false;
                if (studentWithId != null)
                {
                    output.value = "The following student is already using this ID: " + studentWithId.LastName + ", " + studentWithId.FirstName;
                }
                else
                {
                    output.value = "This ID is already in use."; // TODO: should never see this message, handle better, at least log it
                }
            }

            return output;
        }

        public OutputDto_SuccessAndStatus ConsolidateStudentServices(InputDto_ConsolidateStudentServices input)
        {
            var targetServiceId = input.PrimaryService.id;
            // get all students that have this services, and see if they already have the primary service...
            

            // get all students currently using any of the target services
            foreach (var svc in input.SecondaryServices)
            {
                var studentAttributeDataToDelete = new List<StudentAttributeData>();
                var studentsWhoHavePrimaryServiceAlready = _dbContext.StudentAttributeDatas.Where(p => p.AttributeID == 4 && p.AttributeValueID == targetServiceId).Select(p => p.StudentID);

                // these are the students that have this secondary service already
                var studentsWhoAlreadyHaveSecondaryService = _dbContext.StudentAttributeDatas.Where(p => p.AttributeID == 4 && p.AttributeValueID == svc.id);

                // for the ones that are already on the primary list, delete their secondary reference
                var studentsWithSecondaryAndPrimary = studentsWhoAlreadyHaveSecondaryService.Where(p => studentsWhoHavePrimaryServiceAlready.Contains(p.StudentID));
                studentsWithSecondaryAndPrimary.Each(p =>
                {
                    studentAttributeDataToDelete.Add(p);
                });
                _dbContext.StudentAttributeDatas.RemoveRange(studentAttributeDataToDelete);

                // for the ones that don't have the secondary, update their secondary lookupvaluid to match targetserviceid
                var studentsWithSecondaryOnly = studentsWhoAlreadyHaveSecondaryService.Where(p => !studentsWhoHavePrimaryServiceAlready.Contains(p.StudentID));
                studentsWithSecondaryOnly.Each(p =>
                {
                    p.AttributeValueID = targetServiceId;
                });

                var studentAttributeToRemove = _dbContext.StudentAttributeLookupValues.First(p => p.AttributeID == 4 && p.LookupValueID == svc.id);
                _dbContext.StudentAttributeLookupValues.Remove(studentAttributeToRemove);
                _dbContext.SaveChanges();
            }



            return new OutputDto_SuccessAndStatus { isValid = true, Status = new OutputDto_Status { StatusCode = StatusCode.Ok } };

        }


        public OutputDto_SuccessAndStatus ConsolidateStudent(InputDto_ConsolidateStudent input)
        {
            using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
            {
                _dbContext.Database.Connection.Open();
                command.CommandText = "ns4_ConsolidateStudent";
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.CommandTimeout = command.Connection.ConnectionTimeout;
                command.Parameters.Add(new SqlParameter("@primaryStudentId", input.PrimaryStudent.id));
                command.Parameters.Add(new SqlParameter("@secondaryStudentId", input.SecondaryStudent.id));

                command.ExecuteNonQuery();

            }

            return new OutputDto_SuccessAndStatus { isValid = true, Status = new OutputDto_Status { StatusCode = StatusCode.Ok } };

        }

        public OutputDto_Success CanRemoveStudentSchool(InputDto_CanRemoveStudentSchool input)
        {
            var output = new OutputDto_Success();
            // what types of things do we need to check?
            // 1. check and make sure user has access to this school

            // 2. check student isn't in classes at this school
            output.Success = !_dbContext.Students.Include(x => x.StudentSections.Select(y => y.Section)).First(p => p.Id == input.StudentId).StudentSections.Any(p => p.Section.SchoolID == input.SchoolId && p.Section.SchoolStartYear == input.SchoolStartYear);

            return output;
        }

        private async Task<string> GetStudentImage(string studentId, int schoolYear)
        {

            var district = _loginContext.Districts.First(p => p.Id == _currentUser.DistrictId);

            var fileName = schoolYear.ToString() + "/" + district.ProfilePicturePrefix + studentId.ToString() + district.ProfilePictureExtension;

            var blob = _container.GetBlockBlobReference(fileName);
            // check if file exists first
            if (!await blob.ExistsAsync())
            {
                return String.Empty;
            }
            //   var blob = container.GetBlockBlobReference(fileName);

            var builder = new UriBuilder(blob.Uri);
            builder.Query = blob.GetSharedAccessSignature(
                new SharedAccessBlobPolicy
                {
                    Permissions = SharedAccessBlobPermissions.Read,
                    SharedAccessExpiryTime = new DateTimeOffset(DateTime.UtcNow.AddMinutes(60))
                }
                ).TrimStart('?');

            return builder.Uri.ToString();
        }
        public async Task<OutputDto_ManageStudent> GetStudent(int id)
        {
            OutputDto_ManageStudent output = new OutputDto_ManageStudent();

            var student = _dbContext.Students
                .Include(x => x.StudentAttributeDatas)
                .Include(x => x.StudentSchools)
                .FirstOrDefault(p => p.Id == id);

            if (student != null)
            {
                output.Id = student.Id;
                output.FirstName = student.FirstName;
                output.LastName = student.LastName;
                output.MiddleName = student.MiddleName;
                output.DOB = student.DOB.HasValue ? student.DOB.Value : DateTime.MaxValue;  // TODO: this check should not be necessary.  DOB is REQUIRED only 162 students in Detroit and Pequot don't have DOBs... set them to something
                output.DOBText = student.DOB.Value.ToString("dd-MMM-yyyy");
                output.GraduationYear = student.GradYear;
                output.EnrollmentYear = student.StudentSchools.Count > 0 ? student.StudentSchools.OrderBy(p => p.SchoolStartYear).First().SchoolStartYear.ToString() : "Not enrolled in any schools yet.";
                output.Notes = student.Comment;
                output.StudentIdentifier = student.StudentIdentifier;
                output.IsActive = student.IsActive;
                //output.Pronunciation = student.Pronunciation;

                // get imageUrl only if district has images
                if (!String.IsNullOrWhiteSpace(_district.ProfilePictureExtension))
                {

                    var existingSchoolYear = _dbContext.StaffSettings.FirstOrDefault(p => p.StaffId == _currentUser.Id && p.Attribute == StaffSettingTypes.SchoolYear);
                    var schoolYear = GetDefaultYear();

                    if (existingSchoolYear != null)
                    {
                        // no setting found
                        schoolYear = (short)(existingSchoolYear.SelectedValueId.ToNullableInt32() ?? schoolYear);
                    } 
                
                    output.ImageUrl = await GetStudentImage(student.StudentIdentifier, schoolYear);
                }

                foreach (var studentAttributeData in student.StudentAttributeDatas.Where(p => p.AttributeID != 4)) // SPEDLabels are separate
                {
                    output.StudentAttributes.Add(studentAttributeData.AttributeID.ToString(), JToken.FromObject(studentAttributeData.AttributeValueID));
                }

                var studentSpedLabels = new List<StudentAttributeLookupValue>();
                var spedLabelStudentData = student.StudentAttributeDatas.Where(p => p.AttributeID == 4);
                foreach (var dataItem in spedLabelStudentData)
                {
                    var lookupValue = _dbContext.StudentAttributeLookupValues.FirstOrDefault(p => p.AttributeID == 4 && p.LookupValueID == dataItem.AttributeValueID);
                    if (lookupValue != null)
                    {
                        studentSpedLabels.Add(lookupValue);
                    }
                }

                output.SpecialEdLabels = Mapper.Map<List<OutputDto_DropdownData>>(studentSpedLabels);
                output.StudentSchools = Mapper.Map<List<StudentSchoolDto>>(student.StudentSchools.OrderByDescending(p => p.SchoolStartYear).ToList());
            }

            return output;
        }

        public static List<OutputDto_ManageStudent> GetAllStudents(DistrictContext dbContext)
        {
            List<OutputDto_ManageStudent> allStudents = new List<OutputDto_ManageStudent>();

            var students = dbContext.Students
                .Include(x => x.StudentAttributeDatas).ToList();

            foreach (var student in students)
            {
                OutputDto_ManageStudent output = new OutputDto_ManageStudent();

                output.Id = student.Id;
                output.FirstName = student.FirstName;
                output.LastName = student.LastName;
                output.MiddleName = student.MiddleName;
                output.DOB = student.DOB.HasValue ? student.DOB.Value : DateTime.MaxValue;  // TODO: this check should not be necessary.  DOB is REQUIRED only 162 students in Detroit and Pequot don't have DOBs... set them to something
                output.DOBText = student.DOB.Value.ToString("dd-MMM-yyyy");
                output.GraduationYear = student.GradYear;
                output.EnrollmentYear = student.StudentSchools.Count > 0 ? student.StudentSchools.OrderBy(p => p.SchoolStartYear).First().SchoolStartYear.ToString() : "Not enrolled in any schools yet.";
                output.Notes = student.Comment;
                output.StudentIdentifier = student.StudentIdentifier;
                output.IsActive = student.IsActive;

                foreach (var studentAttributeData in student.StudentAttributeDatas.Where(p => p.AttributeID != 4)) // SPEDLabels are separate
                {
                    output.StudentAttributes.Add(studentAttributeData.AttributeID.ToString(), JToken.FromObject(studentAttributeData.AttributeValueID));
                }

                var studentSpedLabels = new List<StudentAttributeLookupValue>();
                var spedLabelStudentData = student.StudentAttributeDatas.Where(p => p.AttributeID == 4);
                foreach (var dataItem in spedLabelStudentData)
                {
                    var lookupValue = dbContext.StudentAttributeLookupValues.FirstOrDefault(p => p.AttributeID == 4 && p.LookupValueID == dataItem.AttributeValueID);
                    if (lookupValue != null)
                    {
                        studentSpedLabels.Add(lookupValue);
                    }
                }

                output.SpecialEdLabels = Mapper.Map<List<OutputDto_DropdownData>>(studentSpedLabels);
                allStudents.Add(output);
            }

            return allStudents;
        }

        public static DataTable ConvertStudentsToDataTable(List<OutputDto_ManageStudent> allStudents, OutputDto_StudentAttributeLookups studentAttributes)
        {
            DataTable table = new DataTable();

            // add standard attributes
            table.Columns.Add("Last Name", typeof(string));
            table.Columns.Add("First Name", typeof(string));
            table.Columns.Add("ID", typeof(string));
            table.Columns.Add("DOB", typeof(string));
            table.Columns.Add("Graduation Year", typeof(string));
            table.Columns.Add("Enrollment Year", typeof(string));
            table.Columns.Add("Notes", typeof(string));
            table.Columns.Add("Is Active", typeof(string));
            table.Columns.Add("Student Services", typeof(string));

            // add dynamic attributes
            foreach(var attribute in studentAttributes.AllAttributes)
            {
                table.Columns.Add(attribute.AttributeName, typeof(string));
            }

            // loop over each student and add them as a new row and match up their attributes to the right column
            foreach(var student in allStudents)
            {
                DataRow row = table.NewRow();
                row["Last Name"] = student.LastName;
                row["First Name"] = student.FirstName;
                row["ID"] = student.StudentIdentifier;
                row["DOB"] = student.DOBText;
                row["Graduation Year"] = student.GraduationYear;
                row["Enrollment Year"] = student.EnrollmentYear;
                row["Notes"] = student.Notes;
                row["Is Active"] = student.IsActive.HasValue && student.IsActive.Value == false ? "N" : "Y";
                row["Student Services"] = string.Join(",", student.SpecialEdLabels.Select(j => j.text).ToList()); // need to just get "text"

                foreach(var attributeValue in student.StudentAttributes)
                {
                    var attributeID = Int32.Parse(attributeValue.Key);
                    var attribute = studentAttributes.AllAttributes.First(p => p.Id == attributeID);
                    var attributeValueID = attributeValue.Value.Value<int>();
                    // find attributeID and attributevalue for the key and values we have
                    var attributeName = attribute.AttributeName;

                    // see if there's a value for this, if so, add it
                    var attributeDisplayValue = attribute.LookupValues.FirstOrDefault(p => p.LookupValueId == attributeValueID);

                    if (attributeDisplayValue != null)
                    {
                        row[attributeName] = attributeDisplayValue.LookupValue;
                    }
                }

                table.Rows.Add(row);
            }

            return table;
        }

        public List<ReportInterventionResult> GetStudentInterventionsForReport(int studentId)
        {
            //var results = ReportInterventionResults.FromSql<ReportInterventionResult>("EXEC [_ns4_GetInterventionsForStudent] @StudentID",
            //    new SqlParameter("StudentID", studentId));

            //return results.ToList();
            var list = new List<ReportInterventionResult>();

            using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
            {
                try
                {
                    _dbContext.Database.Connection.Open();
                    command.CommandText = String.Format("EXEC dbo._ns4_GetInterventionsForStudent @StudentId={0}", studentId);
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = command.Connection.ConnectionTimeout;

                    var attendanceCalculator = new AttendanceCalculator(_dbContext);

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        // load datatable
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            ReportInterventionResult studentResult = new ReportInterventionResult();
                            list.Add(studentResult);
                            studentResult.InterventionGroupId = (dt.Rows[i]["InterventionGroupId"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["InterventionGroupId"].ToString()) : -1;
                            studentResult.SchoolYear = (dt.Rows[i]["SchoolYear"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["SchoolYear"].ToString()) : -1;
                            studentResult.SchoolId = (dt.Rows[i]["SchoolId"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["SchoolId"].ToString()) : -1;
                            studentResult.EndTDDID = (dt.Rows[i]["EndTDDID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["EndTDDID"].ToString()) : (int?)null;
                            studentResult.Description = (dt.Rows[i]["Description"] != DBNull.Value) ? dt.Rows[i]["Description"].ToString() : String.Empty;
                            studentResult.EndOfIntervention = (dt.Rows[i]["EndOfIntervention"] != DBNull.Value) ? DateTime.Parse(dt.Rows[i]["EndOfIntervention"].ToString()) : (DateTime?)null;
                            studentResult.EndOfInterventionString = studentResult.EndOfIntervention?.ToString("dd-MMM-yyyy") ?? "N/A";
                            studentResult.StaffInitials = (dt.Rows[i]["StaffInitials"] != DBNull.Value) ? dt.Rows[i]["StaffInitials"].ToString() : String.Empty;
                            studentResult.InterventionType = (dt.Rows[i]["InterventionType"] != DBNull.Value) ? dt.Rows[i]["InterventionType"].ToString() : String.Empty;
                            studentResult.NumLessons = 0;
                            studentResult.InterventionistId = (dt.Rows[i]["InterventionistId"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["InterventionistId"].ToString()) : -1;
                            studentResult.StartOfIntervention = DateTime.Parse(dt.Rows[i]["StartOfIntervention"].ToString());
                            studentResult.StartOfInterventionString = studentResult.StartOfIntervention.ToString("dd-MMM-yyyy");
                            studentResult.StartTDDID = (dt.Rows[i]["StartTDDID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["StartTDDID"].ToString()) : -1;
                            studentResult.StudentID = (dt.Rows[i]["StudentID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["StudentID"].ToString()) : -1;
                            studentResult.Tier = (dt.Rows[i]["Tier"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["Tier"].ToString()) : -1;
                            studentResult.Id = (dt.Rows[i]["Id"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["Id"].ToString()) : -1;


                            var stintStartEnd = _dbContext.StudentInterventionGroups.FirstOrDefault(p => p.Id == studentResult.Id);
                            var start = DateTime.Now.AddYears(-50);
                            var end = DateTime.Now.AddYears(100);
                            if (stintStartEnd != null)
                            {
                                start = stintStartEnd.StartDate;
                                end = stintStartEnd.EndDate ?? end;
                            }

                            var attendanceData = _dbContext.InterventionAttendances
                            .Include(p => p.AttendanceReason)
                            .Where(p => p.ClassStartEndID == studentResult.Id && p.AttendanceDate >= start && p.AttendanceDate <= end).OrderByDescending(p => p.AttendanceDate).ToList();

                            var distinctAttendanceReasons = _dbContext.AttendanceReasons.Where(p => p.Reason == "Make-Up Lesson" || p.Reason == "Intervention Delivered").ToList();
                            var distinctAttendanceReasonSummaries = distinctAttendanceReasons.Select(p => new AttendanceStatusSummary { Count = 0, StatusLabel = p.Reason }).ToList();

                            attendanceCalculator.SetAttendanceStatuses(distinctAttendanceReasons, distinctAttendanceReasonSummaries, attendanceData, studentResult.StudentID, studentResult.Id);

                            studentResult.NumLessons = distinctAttendanceReasonSummaries.First(j => j.StatusLabel == "Intervention Delivered").Count + distinctAttendanceReasonSummaries.First(j => j.StatusLabel == "Make-Up Lesson").Count;
                        }
                    }

                }
                catch (Exception ex)
                {
                    Log.Error("Error in Student Getting Interventions: {0}", ex.Message);
                }
                finally
                {
                    _dbContext.Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }
            return list;
        }

        public OutputDto_StudentNotes GetNotesForStudent(InputDto_SimpleId input, string token)
        {
            var studentNotes = _dbContext.StudentNotes
                .Where(p => p.StudentID == input.Id).ToList();

            var result = new OutputDto_StudentNotes();
            var student = Mapper.Map<StudentDto>(_dbContext.Students.First(p => p.Id == input.Id));

            result.Student = student;
            result.Notes = Mapper.Map<List<StudentNoteDto>>(studentNotes.OrderByDescending(p => p.NoteDate).ToList());

            foreach(var note in result.Notes)
            {
                note.Note = Regex.Replace(note.Note, @"\?filename=[^&""]+", delegate (Match match)
                {
                    return match.ToString() + "&access_token=" + token;
                });
            }

            return result;
        }

        public OutputDto_Success SaveNote(InputDto_SaveStudentNote input)
        {
            var existingNote = _dbContext.StudentNotes.FirstOrDefault(p =>
                p.Id == input.NoteId);

            // strip access token
            input.NoteHtml = Regex.Replace(input.NoteHtml, "&amp;access_token=([^\"]+)", "");
            
            if (existingNote == null)
            {
                existingNote = new StudentNote()
                {
                    StudentID = input.StudentId,
                    StaffID = _currentUser.Id,
                    Note = input.NoteHtml,
                    NoteDate = DateTime.Now
                };
                _dbContext.StudentNotes.Add(existingNote);
            }
            else
            {
                existingNote.Note = input.NoteHtml;
                existingNote.NoteDate = DateTime.Now;
            }
            _dbContext.SaveChanges();

            return new OutputDto_Success { Success = true };
        }

        public OutputDto_Success DeleteNote(InputDto_SimpleId input)
        {
            var existingNote = _dbContext.StudentNotes.FirstOrDefault(p =>
                p.Id == input.Id);

            if (existingNote == null)
            {
                return new OutputDto_Success { Success = false };
            }

            _dbContext.StudentNotes.Remove(existingNote);
            _dbContext.SaveChanges();

            return new OutputDto_Success { Success = true };
        }
    }
}
