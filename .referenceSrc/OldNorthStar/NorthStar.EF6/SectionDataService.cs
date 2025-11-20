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

namespace NorthStar.EF6
{
    public class SectionDataService : NSBaseDataService
    {
        public SectionDataService(ClaimsIdentity user, string loginConnectionString) : base(user, loginConnectionString)
        {
        }

        public OutputDto_GetSectionList GetSectionList(InputDto_GetSectionList input)
        {
            OutputDto_GetSectionList outputList = new OutputDto_GetSectionList();

            // if you are a district admin, give the user the classes they want based on input criteria
            if (IsDistrictAdmin || IsSchoolAdmin(input.SchoolId))
            {

                outputList.Sections = _dbContext.Sections.Where(p =>
                                                                p.SchoolStartYear == input.SchoolYear &&
                                                                p.SchoolID == input.SchoolId &&
                                                                (p.GradeID == input.GradeId || input.GradeId == -1) &&
                                                                (p.StaffSections.Any(j => j.StaffID == input.TeacherId) || input.TeacherId == -1))
                                                          .Select(p =>
                                                            new SectionListDto
                                                            {
                                                                Description = p.Description,
                                                                Name = p.Name,
                                                                Id = p.Id,
                                                                PrimaryTeacher = p.Staff.LastName + ", " + p.Staff.FirstName,
                                                                NumStudents = p.StudentSections.Count,
                                                                Grade = p.Grade.ShortName,
                                                                CoTeachers = p.StaffSections.Where(s => s.StaffHierarchyPermissionID != 1).Select(r => r.Staff.LastName + ", " + r.Staff.FirstName).ToList()
                                                            }).OrderBy(p => p.Grade).ThenBy(p => p.PrimaryTeacher).ToList(); // TODO: This gets inactive students in the count
            }
            // if the user has grade level access, give them all the sections for the grade that they have access to
            else if(IsGradeAdmin(input.SchoolId, input.GradeId)){
                outputList.Sections = _dbContext.Sections.Where(p =>
                                                p.SchoolStartYear == input.SchoolYear &&
                                                p.SchoolID == input.SchoolId &&
                                                (p.GradeID == input.GradeId) &&
                                                (p.StaffSections.Any(j => j.StaffID == input.TeacherId) || input.TeacherId == -1))
                                          .Select(p =>
                                            new SectionListDto
                                            {
                                                Description = p.Description,
                                                Name = p.Name,
                                                Id = p.Id,
                                                PrimaryTeacher = p.Staff.LastName + ", " + p.Staff.FirstName,
                                                NumStudents = p.StudentSections.Count,
                                                Grade = p.Grade.ShortName,
                                                CoTeachers = p.StaffSections.Where(s => s.StaffHierarchyPermissionID != 1).Select(r => r.Staff.LastName + ", " + r.Staff.FirstName).ToList()
                                            }).OrderBy(p => p.Grade).ThenBy(p => p.PrimaryTeacher).ToList(); 
            }
            // if the user is just a teacher, only give them sections that they are explicitly added to
            else
            {
                outputList.Sections = _dbContext.Sections.Where(p =>
                                p.SchoolStartYear == input.SchoolYear &&
                                p.SchoolID == input.SchoolId &&
                                (p.GradeID == input.GradeId || input.GradeId == -1) &&
                                (p.StaffSections.Any(j => j.StaffID == input.TeacherId)))
                          .Select(p =>
                            new SectionListDto
                            {
                                Description = p.Description,
                                Name = p.Name,
                                Id = p.Id,
                                PrimaryTeacher = p.Staff.LastName + ", " + p.Staff.FirstName,
                                NumStudents = p.StudentSections.Count,
                                Grade = p.Grade.ShortName,
                                CoTeachers = p.StaffSections.Where(s => s.StaffHierarchyPermissionID != 1).Select(r => r.Staff.LastName + ", " + r.Staff.FirstName).ToList()
                            }).OrderBy(p => p.Grade).ThenBy(p => p.PrimaryTeacher).ToList();
            }

            return outputList;
        }

        public OutputDto_ManageSection GetSection(int Id)
        {
            OutputDto_ManageSection output = new OutputDto_ManageSection();


            // do security check
            var status = SecurityCheck(Id);
            output.Status = status;
            if (status.StatusCode != StatusCode.Ok)
            {
                return output;
            }

            // will throw exception if not a real section
            var section = _dbContext.Sections
                .Include(x => x.StudentSections.Select(j => j.Student))
                .Include(x => x.StaffSections.Select(j => j.Staff))
                .FirstOrDefault(p => p.Id == Id);

            if(section == null)
            {
                return output;
            }

            output.Id = section.Id;
            output.Name = section.Name;
            output.Description = section.Description;
            output.SchoolId = section.SchoolID;
            output.GradeId = section.Grade.Id;  // = new OutputDto_DropdownData() { id = section.Grade.Id, text = section.Grade.ShortName };
            output.SchoolYear = section.SchoolStartYear;

            // all this type of crap should be fixed when EF7 is done
            output.PrimaryTeacher = new OutputDto_DropdownData() { id = section.Staff.Id, text = section.Staff.LastName + ", " + section.Staff.FirstName };

            var lstStaffSections = new List<OutputDto_DropdownData>();
            // get the details for each staffsection
            foreach (var staffSection in section.StaffSections.Where(p => p.StaffHierarchyPermissionID == 2))
            {
                lstStaffSections.Add(new OutputDto_DropdownData() { id = staffSection.Staff.Id, text = staffSection.Staff.LastName + ", " + staffSection.Staff.FirstName });
            }
            output.CoTeachers = lstStaffSections;

            var lstStudentSections = new List<OutputDto_DropdownData>();
            // get the details for each staffsection
            foreach (var studentSection in section.StudentSections.Where(p => p.Student.IsActive != false))
            {
                lstStudentSections.Add(new OutputDto_DropdownData() { id = studentSection.Student.Id, text = studentSection.Student.LastName + ", " + studentSection.Student.FirstName + " " + studentSection.Student.MiddleName });
            }
            output.Students = lstStudentSections.OrderBy(p => p.text).ToList();

            return output;
        }
        public bool DeleteSection(int id)
        {

            var db_section = _dbContext.Sections.FirstOrDefault(p => p.Id == id);

            if (db_section == null)
            {
                return true;
            }

            // do some checks to make sure that you can delete this section
            // is it referenced in any team meetings?
            //if(_dbContext.TeamMeetings.Where(prop => prop.SectionID == id))
            //{
            //    throw new UserDisplayableException("This section is referenced by Team Meetings.  Unable to delete at this time.");
            //}

            // remove all the staffsections
            _dbContext.StaffSections.RemoveRange(db_section.StaffSections);

            // remove all the studentsections
            _dbContext.StudentSections.RemoveRange(db_section.StudentSections);

            // delete the section
            _dbContext.Sections.Remove(db_section);

            _dbContext.SaveChanges();
            return true;

        }
        public OutputDto_Base SaveSection(OutputDto_ManageSection section)
        {
            var output = new OutputDto_Base();

            // do security check
            var status = SecurityCheck(section.Id);
            output.Status = status;
            if (status.StatusCode != StatusCode.Ok)
            {
                return output;
            }

            // determine if this is a new section or not
            var db_section = _dbContext.Sections
                    .Include(p => p.StaffSections)
                    .Include(p => p.StudentSections)
                    .FirstOrDefault(p => p.Id == section.Id);

            if (db_section == null)
            {
                db_section = new Section()
                {
                    GradeID = section.GradeId,
                    Description = section.Description,
                    StaffID = section.PrimaryTeacher.id,
                    IsInterventionGroup = false,
                    Name = section.Name,
                    SchoolID = section.SchoolId,
                    SchoolStartYear = section.SchoolYear
                };

                _dbContext.Sections.Add(db_section);
            }
            else
            {
                db_section.Name = section.Name;
                db_section.GradeID = section.GradeId;
                db_section.Description = section.Description;
                db_section.StaffID = section.PrimaryTeacher.id;
                db_section.IsInterventionGroup = false;
                db_section.SchoolID = section.SchoolId;
                db_section.SchoolStartYear = section.SchoolYear;
            }
            _dbContext.SaveChanges();

            #region StaffSections
            var staffSectionsToDelete = new List<StaffSection>();
            // remove deleted staffsections, staffsections that are attached to db_section but not in section

            db_section.StaffSections
                    .Where(d => d.StaffHierarchyPermissionID == 2 && !section.CoTeachers.Any(ct => ct.id == d.StaffID))
                    .Each(deleted => staffSectionsToDelete.Add(deleted));

            _dbContext.StaffSections.RemoveRange(staffSectionsToDelete);

            //update or add new staffsections
            section.CoTeachers.Each(ct =>
            {
                // check to see if this is an existing co-teacher record
                var coTeacher = db_section.StaffSections.FirstOrDefault(d => d.StaffID == ct.id);
                if (coTeacher == null)
                {
                    coTeacher = new StaffSection();
                    db_section.StaffSections.Add(coTeacher);
                }
                coTeacher.StaffID = ct.id;
                coTeacher.ClassID = section.Id;
                coTeacher.StaffHierarchyPermissionID = 2;
            });

            // add or update primary teacher
            var db_primaryTeacher = db_section.StaffSections.FirstOrDefault(p => p.StaffHierarchyPermissionID == 1);

            // fringe case, no primary teacher, just create a new one
            if (db_primaryTeacher == null)
            {
                db_primaryTeacher = new StaffSection() { StaffHierarchyPermissionID = 1, ClassID = db_section.Id, StaffID = section.PrimaryTeacher.id };
                _dbContext.StaffSections.Add(db_primaryTeacher);
            }// new teacher, get rid of old and add new
            else if (db_primaryTeacher.StaffID != section.PrimaryTeacher.id)
            {
                _dbContext.StaffSections.Remove(db_primaryTeacher);
                db_primaryTeacher = new StaffSection() { StaffHierarchyPermissionID = 1, ClassID = db_section.Id, StaffID = section.PrimaryTeacher.id };
                _dbContext.StaffSections.Add(db_primaryTeacher);
            }
            _dbContext.SaveChanges();
            #endregion

            #region StudentSections
            // remove deleted staffsections, staffsections that are attached to db_section but not in section
            var studentSectionsToDelete = new List<StudentSection>();
            db_section.StudentSections
                    .Where(d => !section.Students.Any(st => st.id == d.StudentID))
                    .Each(deleted => studentSectionsToDelete.Add(deleted));
            _dbContext.StudentSections.RemoveRange(studentSectionsToDelete);

            //update or add new staffsections
            section.Students.Each(st =>
            {
                // check to see if this is an existing co-teacher record
                var student = db_section.StudentSections.FirstOrDefault(d => d.StudentID == st.id);
                if (student == null)
                {
                    student = new StudentSection();
                    db_section.StudentSections.Add(student);
                }
                student.StudentID = st.id;
                student.ClassID = db_section.Id;
            });
            _dbContext.SaveChanges();
            #endregion

            output.Status.StatusCode = StatusCode.Ok;
            return output;
        }

        public List<OutputDto_OptGroupDropdownData> QuickSearchSections(string searchString, int schoolYear)
        {
            // TODO: filter this by the staff you have access to
            var teachers = _dbContext.Staffs.Where(p => (p.FirstName.StartsWith(searchString) || p.LastName.StartsWith(searchString))).OrderBy(p => p.LastName).ThenBy(p => p.FirstName).Take(10);

            if (teachers.Any())
            {
                var results = new List<OutputDto_OptGroupDropdownData>();
                // TODO: global check for unassigned staff
                foreach (var teacher in teachers)
                {
                    // get all the sections for the teacher
                    var sectionList = new List<OutputDto_OptGroupDropdownDataSection>();
                    var sections = _dbContext.StaffSections.Where(p => p.StaffID == teacher.Id && p.Section.SchoolStartYear == schoolYear).Select(p => p.Section).OrderByDescending(p => p.SchoolStartYear);

                    foreach (var section in sections)
                    {
                        sectionList.Add(new OutputDto_OptGroupDropdownDataSection()
                        {
                            id = section.Id,
                            GradeId = section.GradeID,
                            GradeName = section.Grade.ShortName,
                            SchoolYearVerbose = section.SchoolYear.YearVerbose,
                            SchoolId = section.SchoolID,
                            SchoolName = section.School.Name,
                            SchoolStartYear = section.SchoolStartYear,
                            StaffName = section.Staff.LastName + ", " + section.Staff.FirstName,
                            SectionName = section.Name,
                            StaffId = section.StaffID,
                            text = "School Year: <b>" + section.SchoolYear.YearVerbose + "</b> | " + section.School.Name + " <br>Grade: <b>" + section.Grade.ShortName + "</b><br>Name: <b>" + section.Name + "</b>"
                        });
                    }

                    // only add teachers that have a matching class
                    if (sectionList.Count > 0)
                    {
                        results.Add(new OutputDto_OptGroupDropdownData()
                        {
                            text = "<div class='select2OptGroupHeader'>" + teacher.LastName + ", " + teacher.FirstName + "</div>",
                            children = sectionList
                        });
                    }
                }
                return results;
            }
            else
            {
                return new List<OutputDto_OptGroupDropdownData>();
            }
        }

        public List<OutputDto_OptGroupDropdownData> QuickSearchSectionsAllSchoolYears(string searchString)
        {
            // TODO: filter this by the staff you have access to
            var teachers = _dbContext.Staffs.Where(p => (p.FirstName.StartsWith(searchString) || p.LastName.StartsWith(searchString))).OrderBy(p => p.LastName).ThenBy(p => p.FirstName).Take(10);

            if (teachers.Any())
            {
                var results = new List<OutputDto_OptGroupDropdownData>();
                // TODO: global check for unassigned staff
                foreach (var teacher in teachers)
                {
                    // get all the sections for the teacher
                    var sectionList = new List<OutputDto_OptGroupDropdownDataSection>();
                    var sections = _dbContext.StaffSections.Where(p => p.StaffID == teacher.Id).Select(p => p.Section).OrderByDescending(p => p.SchoolStartYear);

                    foreach (var section in sections)
                    {
                        sectionList.Add(new OutputDto_OptGroupDropdownDataSection()
                        {
                            id = section.Id,
                            GradeId = section.GradeID,
                            GradeName = section.Grade.ShortName,
                            SchoolYearVerbose = section.SchoolYear.YearVerbose,
                            SchoolId = section.SchoolID,
                            SchoolName = section.School.Name,
                            SchoolStartYear = section.SchoolStartYear,
                            StaffName = section.Staff.LastName + ", " + section.Staff.FirstName,
                            SectionName = section.Name,
                            StaffId = section.StaffID,
                            text = "School Year: <b>" + section.SchoolYear.YearVerbose + "</b> | " + section.School.Name + " <br>Grade: <b>" + section.Grade.ShortName + "</b><br>Name: <b>" + section.Name + "</b>"
                        });
                    }

                    // only add teachers that have a matching class
                    if (sectionList.Count > 0)
                    {
                        results.Add(new OutputDto_OptGroupDropdownData()
                        {
                            text = "<div class='select2OptGroupHeader'>" + teacher.LastName + ", " + teacher.FirstName + "</div>",
                            children = sectionList
                        });
                    }
                }
                return results;
            }
            else
            {
                return new List<OutputDto_OptGroupDropdownData>();
            }
        }
    }
}
