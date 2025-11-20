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
using EntityDto.DTO.Reports.StackedBarGraphs;
using NorthStar4.CrossPlatform.DTO.Reports.StackedBarGraphs;
using static NorthStar.Core.NSConstants;
using Northstar.Core.Extensions;
using Serilog;
using Newtonsoft.Json;

namespace NorthStar.EF6
{
    public class StackedBarGraphReportService : NSBaseDataService
    {
        public StackedBarGraphReportService(ClaimsIdentity user, string loginConnectionString) : base(user, loginConnectionString)
        {

        }

        public List<OutputDto_StackBarGraphGroupData> GetStackedBarGraphGroupData(InputDto_GetStackedBarGraphGroupingUpdatedOptions input)
        {
            var result = _dbContext.GetGroupedStackBarGraphResults(input);
            return result;
        }

        public OutputDto_StackedBarGraphComparisonData GetStackedBarGraphComparisonData(InputDto_GetStackedBarGraphGroupingUpdatedOptions input)
        {
            var list = _dbContext.GetGroupedStackBarGraphResults(input);
            return new OutputDto_StackedBarGraphComparisonData { Results = list, GroupName = input.GroupName };
        }

        public OutputDto_GetPLCPlanningReport GetPLCPlanningReport(InputDto_GetStackedBarGraphGroupingUpdatedOptions input)
        {
            var output = new OutputDto_GetPLCPlanningReport();

            var serializedInput = JsonConvert.SerializeObject(input);
            var schoolId = input.Schools.First().id;
            var gradeId = input.Grades.First().id;

            var sectionsForGradeSchool = _dbContext.Sections.Include(j => j.StudentSections).Where(p => p.SchoolStartYear == input.SchoolStartYear && p.SchoolID == schoolId && p.GradeID == gradeId);

            // loop over this and get every class for this grade level
            foreach (var section in sectionsForGradeSchool)
            {
                var deserializedInput = JsonConvert.DeserializeObject<InputDto_GetStackedBarGraphGroupingUpdatedOptions>(serializedInput);
                deserializedInput.Sections.Add(new OutputDto_DropdownData() { id = section.Id, text = section.Name });
                
                var studentCount = section.StudentSections.ToList().Count;
                // SH TODO - remove inactive students from count
                var updatedSerializedInput = JsonConvert.SerializeObject(deserializedInput);

                var genEdClone = JsonConvert.DeserializeObject<InputDto_GetStackedBarGraphGroupingUpdatedOptions>(updatedSerializedInput);
                var spedClone = JsonConvert.DeserializeObject<InputDto_GetStackedBarGraphGroupingUpdatedOptions>(updatedSerializedInput);
                var elClone = JsonConvert.DeserializeObject<InputDto_GetStackedBarGraphGroupingUpdatedOptions>(updatedSerializedInput); ;
                var gtClone = JsonConvert.DeserializeObject<InputDto_GetStackedBarGraphGroupingUpdatedOptions>(updatedSerializedInput);
                var t1Clone = JsonConvert.DeserializeObject<InputDto_GetStackedBarGraphGroupingUpdatedOptions>(updatedSerializedInput);

                genEdClone.SpecialEd = new OutputDto_DropdownData() { id = 0, text = "Non-Special Ed (General Population Student)" };
                spedClone.SpecialEd = new OutputDto_DropdownData() { id = 1, text = "Special Ed (Special Education Student)" };

                var elAttribute = _dbContext.StudentAttributeTypes.First(p => p.AttributeName == "ELL");
                elClone.DropdownDataList.Add(new NamedDropdownData() { AttributeTypeId = elAttribute.Id, Name = elAttribute.AttributeName, DropDownData = new List<OutputDto_DropdownData>() { new OutputDto_DropdownData() { id = 1, text = "ELL" } } });

                var gtAttr = _dbContext.StudentAttributeTypes.First(p => p.AttributeName == "Gifted");
                gtClone.DropdownDataList.Add(new NamedDropdownData() { AttributeTypeId = gtAttr.Id, Name = gtAttr.AttributeName, DropDownData = new List<OutputDto_DropdownData>() { new OutputDto_DropdownData() { id = 1, text = "Gifted" } } });

                var t1Attr = _dbContext.StudentAttributeTypes.First(p => p.AttributeName == "Title I");
                t1Clone.DropdownDataList.Add(new NamedDropdownData() { AttributeTypeId = t1Attr.Id, Name = t1Attr.AttributeName, DropDownData = new List<OutputDto_DropdownData>() { new OutputDto_DropdownData() { id = 1, text = "T-m" }, new OutputDto_DropdownData() { id = 2, text = "T-R" }, new OutputDto_DropdownData() { id = 3, text = "T-rm" }, new OutputDto_DropdownData() { id = 4, text = "T" } } });

                var genEdList = _dbContext.GetGroupedStackBarGraphResults(genEdClone);
                var spEdList = _dbContext.GetGroupedStackBarGraphResults(spedClone);
                var elList = _dbContext.GetGroupedStackBarGraphResults(elClone);
                var gtList = _dbContext.GetGroupedStackBarGraphResults(gtClone);
                var t1List = _dbContext.GetGroupedStackBarGraphResults(t1Clone);

                var currentSectionInfo = new PLCSectionInfo { TeacherName = section.Staff.FullName, ClassName = section.Name, StudentCount = studentCount, GenEdResults = genEdList, SpEdResults = spEdList, ELResults = elList, GTResults = gtList, T1Results = t1List, StaffId = section.StaffID, SectionId = section.Id };

                output.PLCSectionInfoList.Add(currentSectionInfo);
            }
            return output;
        }

        public OutputDto_StackBarGraphSummaryData GetStackedBarGraphGroupSummary(InputDto_GetStackedBarGraphGroupingSummaryUpdatedOptions input)
        {
            // TODO: Move this to data service
            OutputDto_StackBarGraphSummaryData result = null;

            try {
                result = _dbContext.GetStackedBarGraphGroupSummary(input, _currentUser.Id);
            } catch(Exception ex)
            {
                Log.Error("Error getting stacked bar graph group summary.  Likely a timeout.  Error is: {0} - INPUT IS: {1}", ex.Message, JsonConvert.SerializeObject(input));
                throw ex;
            }
            return result;
        }

        public OutputDto_StackBarGraphHistoricalSummaryData GetStackedBarGraphGroupHistoricalSummary(InputDto_GetStackedBarGraphGroupingSummaryUpdatedOptions input)
        {
            // TODO: Move this to data service
            var result = _dbContext.GetStackedBarGraphGroupHistoricalSummary(input, _currentUser.Id);
            return result;
        }

        private List<School> LoadSchools()
        {
            if (IsDistrictAdmin)
            {
                return _dbContext.Schools.OrderBy(p => p.Name).ToList();
            }
            else
            {
                return _dbContext.StaffSchools.Where(p =>
                    p.StaffID == _currentUser.Id && p.StaffHierarchyPermissionID >= 1)
                .OrderBy(g => g.School.Name).Select(p => p.School).ToList();
            }
        }

        private List<Staff> LoadTeachers(InputDto_GetStackedBarGraphGroupingUpdatedOptions inputOptions)
        {
                if (inputOptions.Schools.Count == 0)
                {
                    return new List<Staff>();
                }

            var combinedStaffList = new List<Staff>();
            var gradeIdList = inputOptions.Grades != null ? inputOptions.Grades.Select(p => p.id).ToList() : new List<int>();

            foreach (var school in inputOptions.Schools)
            {
                if (IsDistrictAdmin || IsSchoolAdmin(school.id))
                {
                    if (inputOptions.Grades.Count == 0)
                    {
                        combinedStaffList =  _dbContext.StaffSchools.Where(p => ((p.Staff.IsActive.HasValue && p.Staff.IsActive.Value) || !p.Staff.IsActive.HasValue) && p.SchoolID == school.id)
                            .Select(p => p.Staff).ToList()
                            .Union(combinedStaffList).ToList();
                    }
                    else
                    {
                        combinedStaffList = _dbContext.StaffSections.Where(p => ((p.Staff.IsActive.HasValue && p.Staff.IsActive.Value) || !p.Staff.IsActive.HasValue)
                            && p.Section.SchoolID == school.id
                            && gradeIdList.Contains(p.Section.GradeID)
                            && p.Section.SchoolStartYear == inputOptions.SchoolStartYear)
                            .Select(p => p.Staff).ToList()
                            .Union(combinedStaffList).ToList();
                    }
                }
                else
                {
                    if (inputOptions.Grades.Count == 0)
                    {
                        combinedStaffList = _dbContext.Staffs.Where(p => p.Id == _currentUser.Id).Union(combinedStaffList).ToList();
                    }
                    else
                    {
                        // if not a school or district admin, get teachers for grades i have access to
                        var staffWithExplicitAccess = _dbContext.StaffSections.Where(p =>
                            p.StaffID == _currentUser.Id &&
                            p.Section.SchoolID == school.id &&
                            gradeIdList.Contains(p.Section.GradeID) &&
                            p.Section.IsInterventionGroup == false &&
                            p.Section.SchoolStartYear == inputOptions.SchoolStartYear)
                            .Select(p => p.Staff)
                            .ToList();

                        var gradesCanAccessAtSchool = _dbContext.StaffSchoolGrades.Where(p =>
                            p.StaffID == _currentUser.Id &&
                            p.SchoolID == school.id)
                            .Select(p => p.GradeID).ToList();

                        //var combinedGradeList = gradesCanAccessAtSchool.Union(gradeIdList).Distinct().ToList();

                        var staffCanAccessByGrade = _dbContext.StaffSections.Where(p =>
                            p.Section.SchoolID == school.id &&
                            gradesCanAccessAtSchool.Contains(p.Section.GradeID) &&
                            p.Section.IsInterventionGroup == false &&
                            p.Section.SchoolStartYear == inputOptions.SchoolStartYear)
                            .Select(p => p.Staff)
                            .ToList();

                        combinedStaffList = staffWithExplicitAccess.Union(staffCanAccessByGrade).Union(combinedStaffList)
                            .ToList();
                    }
                }
            }


            return combinedStaffList 
                    .OrderBy(p => p.LastName)
                    .ThenBy(p => p.FirstName)
                    .Distinct().ToList();
        }

        private List<Staff> LoadInterventionists(InputDto_GetStackedBarGraphGroupingUpdatedOptions inputOptions)
        {
            foreach (var school in inputOptions.Schools)
            {
                // there's a school selected, but we only want to return the list of teachers for a school if you are an admin there... otherwise, you just get yourself
                if (IsSchoolAdmin(school.id) || IsDistrictAdmin)
                {
                    return _dbContext.StaffInterventionGroups.Where(p => ((p.Staff.IsActive.HasValue && p.Staff.IsActive.Value) || !p.Staff.IsActive.HasValue) 
                        && p.InterventionGroup.SchoolID == school.id && p.InterventionGroup.SchoolStartYear == inputOptions.SchoolStartYear)
                        .Select(p => p.Staff)
                        .Distinct()
                        .OrderBy(p => p.LastName)
                        .ThenBy(p => p.FirstName)
                        .ToList();
                }
                else // just get yourselff
                {
                    return _dbContext.Staffs.Where(p => p.Id == _currentUser.Id)
                        .ToList();
                }
            }

            return new List<Staff>();
        }

        private List<Section> LoadSections(InputDto_GetStackedBarGraphGroupingUpdatedOptions inputOptions)
        {
            // if no school or staff selected, just return empty list
            if (inputOptions.Schools.Count == 0 || inputOptions.Teachers.Count == 0)
            {
                return new List<Section>();
            }

            var combinedSectionList = new List<Section>();
            var gradeIdList = inputOptions.Grades != null ? inputOptions.Grades.Select(p => p.id).ToList() : new List<int>();

            foreach (var teacher in inputOptions.Teachers)
            {
                combinedSectionList = _dbContext.StaffSections.Where(p =>
                    p.StaffID == teacher.id &&
                    p.Section.SchoolStartYear == inputOptions.SchoolStartYear &&
                    (gradeIdList.Contains(p.Section.GradeID) || inputOptions.Grades.Count == 0)).Select(p => p.Section).ToList().Union(combinedSectionList).ToList();
            }

            return combinedSectionList.Distinct().ToList();
        }

        private List<InterventionGroup> LoadInterventionGroups(InputDto_GetStackedBarGraphGroupingUpdatedOptions inputOptions)
        {
            // if no school or staff selected, just return empty list
            if (inputOptions.Schools.Count == 0 || inputOptions.Interventionists.Count == 0)
            {
                return new List<InterventionGroup>();
            }

            var groups = new List<InterventionGroup>();

            foreach (var interventionist in inputOptions.Interventionists)
            {
                groups = _dbContext.StaffInterventionGroups.Where(p =>
                    p.StaffID == interventionist.id &&
                    p.InterventionGroup.SchoolStartYear == inputOptions.SchoolStartYear).Select(p => p.InterventionGroup).ToList().Union(groups).ToList();
            }

            return groups.Distinct().ToList();
        }

        private List<Student> LoadInterventionGroupStudents(InputDto_GetStackedBarGraphGroupingUpdatedOptions inputOptions)
        {
            if (inputOptions.InterventionGroups.Count == 0)
            {
                return new List<Student>();
            }

            var students = new List<Student>();

            foreach (var group in inputOptions.InterventionGroups)
            {
                students = _dbContext.StudentInterventionGroups.Where(p =>
                    p.InterventionGroupId == group.id &&
                    p.InterventionGroup.SchoolStartYear == inputOptions.SchoolStartYear).Select(p => p.Student).ToList().Union(students).ToList();
            }

            return students.Distinct().ToList();
        }

        private List<StudentInterventionGroup> LoadInterventionStints(InputDto_GetStackedBarGraphGroupingUpdatedOptions inputOptions)
        {
            // if no school or staff selected, just return empty list
            if (inputOptions.InterventionGroups.Count == 0 || inputOptions.Students.Count == 0)
            {
                return new List<StudentInterventionGroup>();
            }

            var stints = new List<StudentInterventionGroup>();
            var studentIds = inputOptions.Students.Select(p => p.id).ToList();

            foreach (var group in inputOptions.InterventionGroups)
            {
                stints = _dbContext.StudentInterventionGroups.Where(p =>
                    p.InterventionGroupId == group.id && 
                    studentIds.Contains(p.StudentID) &&
                    p.InterventionGroup.SchoolStartYear == inputOptions.SchoolStartYear).ToList().Union(stints).ToList();
            }

            return stints.Distinct().ToList();
        }

        private List<Grade> LoadGrades(InputDto_GetStackedBarGraphGroupingUpdatedOptions inputOptions)
        {
            if (inputOptions.Schools.Count == 0)
            {
                return new List<Grade>();
            }

            var combinedGradeList = new List<Grade>();

            foreach(var school in inputOptions.Schools)
            {
                if (IsSchoolAdmin(school.id) || IsDistrictAdmin)
                {
                    combinedGradeList = _dbContext.StaffSections.Where(p =>
                        p.Section.SchoolID == school.id &&
                        p.Section.SchoolStartYear == inputOptions.SchoolStartYear)
                        .OrderBy(g => g.Section.Grade.GradeOrder).Select(p => p.Section.Grade).Distinct().ToList().Union(combinedGradeList).ToList();
                }
                else
                {
                    // also ensure users with grade level access are accounted for
                    var gradesGrantedAccess = _dbContext.StaffSchoolGrades.Where(p =>
                        p.SchoolID == school.id &&
                        p.StaffID == _currentUser.Id).Select(p => p.Grade).Distinct().ToList();

                    var explicitGradesFromSections = _dbContext.StaffSections.Where(p =>
                       p.StaffID == _currentUser.Id &&
                       p.Section.SchoolID == school.id &&
                       p.Section.SchoolStartYear == inputOptions.SchoolStartYear &&
                       p.Section.IsInterventionGroup == false)
                    .OrderBy(g => g.Section.Grade.GradeOrder).Select(p => p.Section.Grade).Distinct().ToList();

                    // combine the two lists
                    combinedGradeList = gradesGrantedAccess.Union(explicitGradesFromSections).Union(combinedGradeList).OrderBy(p => p.GradeOrder).ToList();
                }
            }
            return combinedGradeList.Distinct().ToList();
        }

        public OutputDto_GetStackedBarGraphGroupingUpdatedOptions GetSchoolsByGrade(InputDto_SchoolsByGrade input)
        {
            // get all schools user has access to
            var schools = LoadSchools();

            // now filter them by input parameter
            switch (input.Grades)
            {
                case "prek":
                    schools = schools.Where(p => p.IsPreK == true).ToList();
                    break;
                case "k2":
                    schools = schools.Where(p => p.IsK2 == true).ToList();
                    break;
                case "elem":
                    schools = schools.Where(p => p.Is35 == true).ToList();
                    break;
                case "k5":
                    schools = schools.Where(p => p.IsK5 == true).ToList();
                    break;
                case "k8":
                    schools = schools.Where(p => p.IsK8 == true).ToList();
                    break;
                case "ms":
                    schools = schools.Where(p => p.IsMS == true).ToList();
                    break;
                case "hs":
                    schools = schools.Where(p => p.IsHS == true).ToList();
                    break;
                case "ss":
                    schools = schools.Where(p => p.IsSS == true).ToList();
                    break;
                case "all":
                    break;
            }

            return new OutputDto_GetStackedBarGraphGroupingUpdatedOptions { Schools = Mapper.Map<List<OutputDto_DropdownData>>(schools) };
        }

        public OutputDto_GetStackedBarGraphGroupingUpdatedOptions GetStackedBarGraphGroupingUpdatedOptions(InputDto_GetStackedBarGraphGroupingUpdatedOptions input)
        {
            var currentStaffId = _currentUser.Id;

            OutputDto_GetStackedBarGraphGroupingUpdatedOptions options = new OutputDto_GetStackedBarGraphGroupingUpdatedOptions();
            var schools = new List<OutputDto_DropdownData>();
            var grades = new List<OutputDto_DropdownData>();
            var teachers = new List<OutputDto_DropdownData>();
            var sections = new List<OutputDto_DropdownData>();
            var schoolYears = new List<OutputDto_DropdownData>();
            var interventionists = new List<OutputDto_DropdownData>();
            var interventionGroups = new List<OutputDto_DropdownData>();
            var interventionStints = new List<OutputDto_DropdownData>();
            var students = new List<OutputDto_DropdownData>();
            var interventionStudents = new List<OutputDto_DropdownData>();

            var schoolIdList = input.Schools != null ? input.Schools.Select(p => p.id) : new List<int>();
            var gradeIdList = input.Grades != null ? input.Grades.Select(p => p.id) : new List<int>();
            var teacherIdList = input.Teachers != null ? input.Teachers.Select(p => p.id) : new List<int>();
            var interventionistIdList = input.Interventionists != null ? input.Interventionists.Select(p => p.id) : new List<int>();
            var schoolYearLow = (DateTime.Now.AddYears(-20).Year);
            var schoolYearHigh = (DateTime.Now.AddYears(2).Year);

            switch (input.ChangeType)
            {
                case "initial":
                    schools = Mapper.Map<List<OutputDto_DropdownData>>(LoadSchools());
                    options.InterventionAssessments = Mapper.Map<List<OutputDto_ObservationSummaryFieldVisibility>>(_dbContext.Assessments.Where(p => p.TestType == 2));
                    if (schools.Count == 1)
                    {
                        schools.First().locked = true;
                        options.SelectedSchool = schools.First().id;
                        input.Schools.Add(schools.First());
                    }
                    var schoolYearsWithTestDueDates = _dbContext.SchoolYears.Where(p => p.SchoolStartYear >= schoolYearLow && p.SchoolStartYear <= schoolYearHigh).ToList();
                    var testDueDateYears = _dbContext.TestDueDates.Select(p => p.SchoolStartYear).ToList();
                    if(testDueDateYears.Count == 0)
                    {
                        Log.Warning("WARNING: A district does not have any test due dates set.  This will be a problem.");
                        return options;
                    }
                    else
                    {
                        schoolYearsWithTestDueDates = schoolYearsWithTestDueDates.Where(p => testDueDateYears.Contains(p.SchoolStartYear)).ToList();
                    }

                    options.Schools = schools;
                    options.SchoolYears = Mapper.Map<List<OutputDto_DropdownData>>(schoolYearsWithTestDueDates);

                    var existingSetting = _dbContext.StaffSettings.FirstOrDefault(p => p.StaffId == _currentUser.Id && p.Attribute == StaffSettingTypes.SchoolYear);

                    if (existingSetting == null)
                    {
                        options.SelectedSchoolYear = GetDefaultYear();
                        input.SchoolStartYear = options.SelectedSchoolYear.Value;

                    } else
                    {
                        options.SelectedSchoolYear = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                        input.SchoolStartYear = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    }

                    options.TestDueDates = Mapper.Map<List<OutputDto_DropdownData>>(_dbContext.TestDueDates.Where(p => p.SchoolStartYear == options.SelectedSchoolYear)).ToList();
                    options.SelectedTestDueDateId = GetCurrentBenchmarkDate(input.SchoolStartYear, DateTime.Now)?.Id;

                    grades = Mapper.Map<List<OutputDto_DropdownData>>(LoadGrades(input));
                    if (grades.Count == 1)
                    {
                        grades.First().locked = true;
                        options.SelectedGrade = grades.First().id;
                    }
                    options.Grades = grades;
                    input.Grades = grades;

                    teachers = Mapper.Map<List<OutputDto_DropdownData>>(LoadTeachers(input));
                    if (teachers.Count == 1)
                    {
                        teachers.First().locked = true;
                        options.SelectedTeacher = teachers.First().id;
                    }
                    input.Teachers = teachers;
                    options.Teachers = teachers;

                    interventionists = Mapper.Map<List<OutputDto_DropdownData>>(LoadInterventionists(input));
                    if (interventionists.Count == 1)
                    {
                        interventionists.First().locked = true;
                    }
                    input.Interventionists = interventionists;
                    options.Interventionists = interventionists;

                    interventionGroups = Mapper.Map<List<OutputDto_DropdownData>>(LoadInterventionGroups(input));
                    if (interventionGroups.Count == 1)
                    {
                        interventionGroups.First().locked = true;
                    }
                    input.InterventionGroups = interventionGroups;
                    options.InterventionGroups = interventionGroups;

                    interventionStudents = Mapper.Map<List<OutputDto_DropdownData>>(LoadInterventionGroupStudents(input));
                    if (interventionStudents.Count == 1)
                    {
                        interventionStudents.First().locked = true;
                    }
                    input.InterventionStudents = interventionStudents;
                    options.InterventionStudents = interventionStudents;

                    //interventionStints = Mapper.Map<List<OutputDto_DropdownData>>(LoadInterventionStints(input));
                    //if (interventionStints.Count == 1)
                    //{
                    //    interventionStints.First().locked = true;
                    //}
                    //input.InterventionStints = interventionStints;
                    //options.InterventionStints = interventionStints;

                    sections = Mapper.Map<List<OutputDto_DropdownData>>(LoadSections(input));
                    if (sections.Count == 1)
                    {
                        sections.First().locked = true;
                        options.SelectedSection = sections.First().id;
                    }
                    options.Sections = sections;
                    input.Sections = sections;

                    // only load students for a single section
                    if (options.SelectedSection != null)
                    {
                        var initialSectionStudents = Mapper.Map<List<OutputDto_DropdownData>>(LoadStudentsForSection(options.SelectedSection.Value));

                        options.Students = initialSectionStudents;
                    }

                    // get attributes and add them
                    foreach (var attributeType in _dbContext.StudentAttributeTypes)
                    {
                        var attributeOptions = _dbContext.StudentAttributeLookupValues.Where(p => p.AttributeID == attributeType.Id);
                        options.DropdownDataList.Add(new NamedDropdownData { Name = attributeType.AttributeName, AttributeTypeId = attributeType.Id, DropDownData = Mapper.Map<List<OutputDto_DropdownData>>(attributeOptions.ToList())});
                    }

                    options.InterventionTypes = Mapper.Map<List<OutputDto_DropdownData>>(_dbContext.InterventionGroups.Where(p => p.SchoolStartYear == options.SelectedSchoolYear).Select(p => p.InterventionType).Distinct().OrderBy(p => p.InterventionType).ToList());
                    break;
                case "school":
                    // TODO: make sure to include security logic streamlining
                    // first get all the sections you have access to at each school
                    // check if you are school or district admin
                    // get either all sections at the school for the current year OR just the ones from staffsection
                    // let's do ALL for now
                    var schoolSectionsGrades = _dbContext.Sections.Where(p => p.SchoolStartYear == input.SchoolStartYear && schoolIdList.Contains(p.SchoolID)).Select(p => p.Grade).Distinct().OrderBy(p => p.GradeOrder).ToList();
                    var schoolStaff = _dbContext.Sections.Where(p => p.SchoolStartYear == input.SchoolStartYear && schoolIdList.Contains(p.SchoolID) && (gradeIdList.Count() == 0 || gradeIdList.Contains(p.GradeID))).Select(p => p.Staff).Distinct().OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToList();
                    grades = Mapper.Map<List<OutputDto_DropdownData>>(schoolSectionsGrades);
                    teachers = Mapper.Map<List<OutputDto_DropdownData>>(schoolStaff);

                    interventionists = Mapper.Map<List<OutputDto_DropdownData>>(LoadInterventionists(input));
                    if (interventionists.Count == 1)
                    {
                        interventionists.First().locked = true;
                    }
                    input.Interventionists = interventionists;
                    options.Interventionists = interventionists;
                    options.Teachers = teachers;
                    options.Grades = grades;
                    break;
                case "schoolyear":
                    UpdateSetting(StaffSettingTypes.SchoolYear, input.SchoolStartYear.ToString());
                    options.TestDueDates = Mapper.Map<List<OutputDto_DropdownData>>(_dbContext.TestDueDates.Where(p => p.SchoolStartYear == input.SchoolStartYear)).ToList();
                    options.InterventionTypes = Mapper.Map<List<OutputDto_DropdownData>>(_dbContext.InterventionGroups.Where(p => p.SchoolStartYear == input.SchoolStartYear).Select(p => p.InterventionType).Distinct().OrderBy(p => p.InterventionType).ToList());

                    //options.TestDueDates = Mapper.Map<List<OutputDto_DropdownData>>(_dbContext.TestDueDates.Where(p => p.SchoolStartYear == options.SelectedSchoolYear)).ToList();
                    options.SelectedTestDueDateId = GetCurrentBenchmarkDate(input.SchoolStartYear, DateTime.Now).Id;

                    grades = Mapper.Map<List<OutputDto_DropdownData>>(LoadGrades(input));
                    if (grades.Count == 1)
                    {
                        grades.First().locked = true;
                        options.SelectedGrade = grades.First().id;
                    }
                    options.Grades = grades;
                    input.Grades = grades;

                    teachers = Mapper.Map<List<OutputDto_DropdownData>>(LoadTeachers(input));
                    if (teachers.Count == 1)
                    {
                        teachers.First().locked = true;
                        options.SelectedTeacher = teachers.First().id;
                    }
                    input.Teachers = teachers;
                    options.Teachers = teachers;

                    interventionists = Mapper.Map<List<OutputDto_DropdownData>>(LoadInterventionists(input));
                    if (interventionists.Count == 1)
                    {
                        interventionists.First().locked = true;
                    }
                    input.Interventionists = interventionists;
                    options.Interventionists = interventionists;

                    sections = Mapper.Map<List<OutputDto_DropdownData>>(LoadSections(input));
                    if (sections.Count == 1)
                    {
                        sections.First().locked = true;
                        options.SelectedSection = sections.First().id;
                    }
                    options.Sections = sections;
                    input.Sections = sections;
                    break;
                case "grade":
                    // TODO: make sure to include security logic streamlining
                    // first get all the sections you have access to at each school
                    // check if you are school or district admin
                    // get either all sections at the school for the current year OR just the ones from staffsection
                    // let's do ALL for now
                    var staff = _dbContext.Sections.Where(p => p.SchoolStartYear == input.SchoolStartYear && schoolIdList.Contains(p.SchoolID) && gradeIdList.Contains(p.GradeID)).Select(p => p.Staff).Distinct().OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToList();

                    teachers = Mapper.Map<List<OutputDto_DropdownData>>(staff);
                    options.Teachers = teachers;
                    break;
                case "teacher":
                    // TODO: make sure to include security logic streamlining
                    // first get all the sections you have access to at each school
                    // check if you are school or district admin
                    // get either all sections at the school for the current year OR just the ones from staffsection
                    // let's do ALL for now
                    var dbSections = _dbContext.Sections.Where(p => p.SchoolStartYear == input.SchoolStartYear && schoolIdList.Contains(p.SchoolID) && gradeIdList.Contains(p.GradeID) && teacherIdList.Contains(p.StaffID)).Distinct().OrderBy(p => p.Name).ToList();

                    sections = Mapper.Map<List<OutputDto_DropdownData>>(dbSections);
                    options.Sections = sections;
                    break;
                case "interventionist":
                    var dbInterventionGroups = _dbContext.InterventionGroups.Where(p => p.SchoolStartYear == input.SchoolStartYear && schoolIdList.Contains(p.SchoolID) && interventionistIdList.Contains(p.StaffID)).Distinct().OrderBy(p => p.Name).ToList();

                    interventionGroups = Mapper.Map<List<OutputDto_DropdownData>>(dbInterventionGroups);
                    if (interventionGroups.Count == 1)
                    {
                        interventionGroups.First().locked = true;
                    }
                    input.InterventionGroups = interventionGroups;
                    options.InterventionGroups = interventionGroups;

                    interventionStudents = Mapper.Map<List<OutputDto_DropdownData>>(LoadInterventionGroupStudents(input));
                    if (interventionStudents.Count == 1)
                    {
                        interventionStudents.First().locked = true;
                    }
                    input.InterventionStudents = interventionStudents;
                    options.InterventionStudents = interventionStudents;

                    //interventionStints = Mapper.Map<List<OutputDto_DropdownData>>(LoadInterventionStints(input));
                    //if (interventionStints.Count == 1)
                    //{
                    //    interventionStints.First().locked = true;
                    //}
                    //input.InterventionStints = interventionStints;
                    //options.InterventionStints = interventionStints;
                    break;
                case "interventiongroup":
                    interventionStudents = Mapper.Map<List<OutputDto_DropdownData>>(LoadInterventionGroupStudents(input));
                    if (interventionStudents.Count == 1)
                    {
                        interventionStudents.First().locked = true;
                    }
                    input.InterventionStudents = interventionStudents;
                    options.InterventionStudents = interventionStudents;

                    break;
                case "interventionstudent":
                    //interventionStints = Mapper.Map<List<OutputDto_DropdownData>>(LoadInterventionStints(input));
                    //if (interventionStints.Count == 1)
                    //{
                    //    interventionStints.First().locked = true;
                    //}
                    //input.InterventionStints = interventionStints;
                    //options.InterventionStints = interventionStints;
                    //break;
                case "section":
                    // only load students for a single section
                    if (input.Sections.Count == 1)
                    {
                        var sectionStudents = Mapper.Map<List<OutputDto_DropdownData>>(LoadStudentsForSection(input.Sections.First().id));

                        options.Students = sectionStudents;
                    }
                    break;
            }

            return options;
        }

        private List<Student> LoadStudentsForSection(int? selectedSectionId)
        {
            // if no school or staff selected, just return empty list
            if (selectedSectionId == null || selectedSectionId == 0)
            {
                return new List<Student>();
            }

            return _dbContext.StudentSections.Where(j =>
                       j.ClassID == selectedSectionId &&
                       !(j.Student.IsActive.HasValue && j.Student.IsActive.Value == false))
                    .Select(j => j.Student).Distinct()
                    .OrderBy(c => c.LastName).ToList();

        }

        private void UpdateSetting(string settingName, string settingValue)
        {
            var existingSetting = _dbContext.StaffSettings.FirstOrDefault(p => p.StaffId == _currentUser.Id && p.Attribute == settingName);

            if (existingSetting == null)
            {
                existingSetting = new StaffSetting { Attribute = settingName, SelectedValueId = settingValue, StaffId = _currentUser.Id, ModifiedBy = _currentUser.Email, ModifiedDate = DateTime.Now };
                _dbContext.StaffSettings.Add(existingSetting);
            }
            else
            {
                existingSetting.SelectedValueId = settingValue;
            }

            var settingsToRemove = new List<StaffSetting>();
            // remove lower settings
            switch (settingName)
            {
                case StaffSettingTypes.SchoolYear:
                    settingsToRemove = _dbContext.StaffSettings.Where(p => p.StaffId == _currentUser.Id &&
                    (p.Attribute == StaffSettingTypes.School ||
                    p.Attribute == StaffSettingTypes.Grade ||
                    p.Attribute == StaffSettingTypes.Teacher ||
                    p.Attribute == StaffSettingTypes.Section ||
                    p.Attribute == StaffSettingTypes.SectionStudent ||
                    p.Attribute == StaffSettingTypes.Interventionist ||
                    p.Attribute == StaffSettingTypes.InterventionGroup ||
                    p.Attribute == StaffSettingTypes.InterventionStudent ||
                    p.Attribute == StaffSettingTypes.Stint)).ToList();
                    _dbContext.StaffSettings.RemoveRange(settingsToRemove);
                    break;               
            }

            _dbContext.SaveChanges();
        }
    }
}
