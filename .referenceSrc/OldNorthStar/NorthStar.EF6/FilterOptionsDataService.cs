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
using static NorthStar.Core.NSConstants;
using Newtonsoft.Json;
using Northstar.Core.Extensions;
using Serilog;

namespace NorthStar.EF6
{
    public class InputDto_HfwSetting
    {
        public string SettingName { get; set; }
        public string SettingValue { get; set; }
    }

    public class FilterOptionsDataService : NSBaseDataService
    {
        public FilterOptionsDataService(ClaimsIdentity user, string loginConnectionString) : base(user, loginConnectionString)
        {

        }

        public List<OutputDto_DropdownData> LoadGrades()
        {
            var grades = _dbContext.Grades.OrderBy(p => p.GradeOrder).ToList();

            return Mapper.Map<List<OutputDto_DropdownData>>(grades);
        }

        public class HfwSettings : OutputDto_Base
        {
            public string WordOrder { get; set; }
            public string WordRange { get; set; }
        }



        public HfwSettings LoadHfwSettings()
        {
            var wordOrder = _dbContext.StaffSettings.FirstOrDefault(p => p.StaffId == _currentUser.Id && p.Attribute == StaffSettingTypes.HFWSortOrder)?.SelectedValueId;
            var wordRange = _dbContext.StaffSettings.FirstOrDefault(p => p.StaffId == _currentUser.Id && p.Attribute == StaffSettingTypes.HFWSingleRange)?.SelectedValueId;

            return new HfwSettings { WordOrder = wordOrder, WordRange = wordRange };
        }

        public OutputDto_Success UpdateHfwSetting(string settingName, string settingValue)
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

            _dbContext.SaveChanges();

            return new OutputDto_Success();
        }

        public List<OutputDto_DropdownData> GetAllSchoolYears()
        {
            var dbSchoolYears = _dbContext.SchoolYears;
            var maxYear = GetDefaultYear();
            var result = Mapper.Map<List<OutputDto_DropdownData>>(dbSchoolYears.Where(p => p.SchoolEndYear <= maxYear + 2).OrderByDescending(p => p.SchoolStartYear).ToList());

            return result;
        }

        public List<OutputDto_DropdownData> GetUserAccessibleSchools()
        {
            var dbSchools = LoadSchools(new InputDto_GetFilterOptions() { SchoolEnabled = true }); // get only the schools the user has access to

            var result = Mapper.Map<List<OutputDto_DropdownData>>(dbSchools.OrderBy(p => p.Name).ToList());

            return result;

  
        }

        public int? GetDefaultBenchmarkDate(int schoolYear, bool yearChange)
        {
            int? tddId = -1;
            var existingBenchmark = _dbContext.StaffSettings.FirstOrDefault(p => p.StaffId == _currentUser.Id && p.Attribute == StaffSettingTypes.BenchmarkDate);
            // after 6 hours default, SH on 9/13/2017 - if yearchange, get new benchmark date
            if ((existingBenchmark != null && DateTime.Now.Subtract(existingBenchmark.ModifiedDate.Value).TotalHours < 8) && !yearChange) // DateTime.Now.Subtract(existingBenchmark.ModifiedDate.Value).Hours < 6
            {
                tddId = existingBenchmark.SelectedValueId.ToNullableInt32();
            }
            else
            {
                tddId = GetCurrentBenchmarkDate(schoolYear, DateTime.Now)?.Id;

                if (existingBenchmark != null)
                {
                    // also set the date
                    // should set it now in case we need to print something
                    if (DateTime.Now.Subtract(existingBenchmark.ModifiedDate.Value).TotalHours >= 8)
                    {
                        UpdateSetting(NSConstants.StaffSettingTypes.BenchmarkDate, tddId.ToString());
                    }
                }
            }

            return tddId;
        }

        /// <summary>
        /// Add or update the setting, remove "lower" settings
        /// </summary>
        /// <param name="settingName"></param>
        /// <param name="settingValue"></param>
        public OutputDto_Success UpdateSetting(string settingName, string settingValue)
        {
            var existingSetting = _dbContext.StaffSettings.FirstOrDefault(p => p.StaffId == _currentUser.Id && p.Attribute == settingName);

            if(existingSetting == null)
            {
                existingSetting = new StaffSetting { Attribute = settingName, SelectedValueId = settingValue, StaffId = _currentUser.Id, ModifiedBy = _currentUser.Email, ModifiedDate = DateTime.Now };
                _dbContext.StaffSettings.Add(existingSetting);
            } else
            {
                existingSetting.SelectedValueId = settingValue;
                existingSetting.ModifiedDate = DateTime.Now;
            }

            var settingsToRemove = new List<StaffSetting>();
            // remove lower settings
            switch (settingName)
            {
                case StaffSettingTypes.Section:
                    settingsToRemove = _dbContext.StaffSettings.Where(p => p.StaffId == _currentUser.Id && p.Attribute == StaffSettingTypes.SectionStudent).ToList();
                    _dbContext.StaffSettings.RemoveRange(settingsToRemove);
                    break;
                case StaffSettingTypes.SectionStudent:
                    // no-op
                    break;
                case StaffSettingTypes.School:
                    settingsToRemove = _dbContext.StaffSettings.Where(p => p.StaffId == _currentUser.Id && 
                    (p.Attribute == StaffSettingTypes.Grade ||
                    p.Attribute == StaffSettingTypes.Teacher ||
                    p.Attribute == StaffSettingTypes.Section ||
                    p.Attribute == StaffSettingTypes.SectionStudent ||
                    p.Attribute == StaffSettingTypes.Interventionist ||
                    p.Attribute == StaffSettingTypes.InterventionGroup ||
                    p.Attribute == StaffSettingTypes.InterventionStudent ||
                    p.Attribute == StaffSettingTypes.Stint)).ToList();
                    _dbContext.StaffSettings.RemoveRange(settingsToRemove);
                    break;
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
                case StaffSettingTypes.Grade:
                    settingsToRemove = _dbContext.StaffSettings.Where(p => p.StaffId == _currentUser.Id &&
                    (p.Attribute == StaffSettingTypes.Teacher ||
                    p.Attribute == StaffSettingTypes.Section ||
                    p.Attribute == StaffSettingTypes.SectionStudent)).ToList();
                    _dbContext.StaffSettings.RemoveRange(settingsToRemove);
                    break;
                case StaffSettingTypes.Teacher:
                    settingsToRemove = _dbContext.StaffSettings.Where(p => p.StaffId == _currentUser.Id &&
                    (p.Attribute == StaffSettingTypes.Section ||
                    p.Attribute == StaffSettingTypes.SectionStudent)).ToList();
                    _dbContext.StaffSettings.RemoveRange(settingsToRemove);
                    break;
                case StaffSettingTypes.Interventionist:
                    settingsToRemove = _dbContext.StaffSettings.Where(p => p.StaffId == _currentUser.Id &&
                    (p.Attribute == StaffSettingTypes.InterventionGroup ||
                    p.Attribute == StaffSettingTypes.InterventionStudent ||
                    p.Attribute == StaffSettingTypes.Stint)).ToList();
                    _dbContext.StaffSettings.RemoveRange(settingsToRemove);
                    break;
                case StaffSettingTypes.InterventionGroup:
                    settingsToRemove = _dbContext.StaffSettings.Where(p => p.StaffId == _currentUser.Id &&
                    (p.Attribute == StaffSettingTypes.InterventionStudent ||
                    p.Attribute == StaffSettingTypes.Stint)).ToList();
                    _dbContext.StaffSettings.RemoveRange(settingsToRemove);
                    break;
                case StaffSettingTypes.InterventionStudent:
                    settingsToRemove = _dbContext.StaffSettings.Where(p => p.StaffId == _currentUser.Id && p.Attribute == StaffSettingTypes.Stint).ToList();
                    _dbContext.StaffSettings.RemoveRange(settingsToRemove);
                    break;
                case StaffSettingTypes.TeamMeeting:
                    settingsToRemove = _dbContext.StaffSettings.Where(p => p.StaffId == _currentUser.Id &&
                    (p.Attribute == StaffSettingTypes.TeamMeetingStaff)).ToList();
                    _dbContext.StaffSettings.RemoveRange(settingsToRemove);
                    break;
                case StaffSettingTypes.TurnOffCommentBubbles:
                    break;
                case StaffSettingTypes.TddMultiRange:
                    // no-op
                    break;
                case StaffSettingTypes.HFWMultiRange:
                    // no-op
                    break;
                case StaffSettingTypes.TeamMeetingStaff:
                    // no-op
                    break;
                case StaffSettingTypes.Stint:
                    // no-op
                    break;
            }

            _dbContext.SaveChanges();

            return new OutputDto_Success();
        }

        public OutputDto_Bool GetExistingBoolSetting(string settingName)
        {
            var result = new OutputDto_Bool();

            var existingSetting = _dbContext.StaffSettings.FirstOrDefault(p => p.StaffId == _currentUser.Id && p.Attribute == settingName);

            if (existingSetting == null)
            {
                // no setting found
                result.Value = null;
            } else
            {
                result.Value = existingSetting.SelectedValueId.ToNullableBool() ?? false;
            }

            return result;
        }

        private void GetExistingSetting(string settingName, OutputDto_FilterOptions outputOptions, InputDto_GetFilterOptions inputOptions)
        {
            var existingSetting = _dbContext.StaffSettings.FirstOrDefault(p => p.StaffId == _currentUser.Id && p.Attribute == settingName);

            if (existingSetting == null)
            {
                // no setting found
                return;
            }

            // remove lower settings
            switch (settingName)
            {
                case StaffSettingTypes.Section:
                    outputOptions.SelectedSection = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    inputOptions.SectionId = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    break;
                case StaffSettingTypes.SectionStudent:
                    outputOptions.SelectedSectionStudent = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    inputOptions.SectionStudentId = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    break;
                case StaffSettingTypes.School:
                    outputOptions.SelectedSchool = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    inputOptions.SchoolId = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    break;
                case StaffSettingTypes.SchoolYear:
                    outputOptions.SelectedSchoolStartYear = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    inputOptions.SchoolYear = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    break;
                case StaffSettingTypes.Grade:
                    outputOptions.SelectedGrade = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    inputOptions.GradeId = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    break;
                case StaffSettingTypes.ClassroomAssessmentField:
                    outputOptions.SelectedClassroomAssessmentField = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    inputOptions.ClassroomAssessmentFieldId = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    break;
                case StaffSettingTypes.InterventionGroupAssessmentField:
                    outputOptions.SelectedInterventionGroupAssessmentField = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    inputOptions.InterventionGroupAssessmentFieldId = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    break;
                case StaffSettingTypes.Teacher:
                    outputOptions.SelectedTeacher = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    inputOptions.TeacherId = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    break;
                case StaffSettingTypes.Interventionist:
                    outputOptions.SelectedInterventionist = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    inputOptions.InterventionistId = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    break;
                case StaffSettingTypes.InterventionGroup:
                    outputOptions.SelectedInterventionGroup = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    inputOptions.InterventionGroupId = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    break;
                case StaffSettingTypes.InterventionStudent:
                    outputOptions.SelectedInterventionStudent = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    inputOptions.InterventionStudentId = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    break;
                case StaffSettingTypes.TeamMeeting:
                    outputOptions.SelectedTeamMeeting = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    inputOptions.TeamMeetingId = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    break;
                case StaffSettingTypes.TeamMeetingStaff:
                    outputOptions.SelectedTeamMeetingStaff = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    inputOptions.TeamMeetingStaffId = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    break;
                case StaffSettingTypes.Stint:
                    outputOptions.SelectedStint = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    inputOptions.StintId = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    break;
                case StaffSettingTypes.HFWSortOrder:
                    outputOptions.SelectedHFWSortOrder = existingSetting.SelectedValueId;
                    inputOptions.HFWSortOrder = existingSetting.SelectedValueId;
                    break;
                case StaffSettingTypes.HRSForm:
                    outputOptions.SelectedHRSForm = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    inputOptions.HRSFormId = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    break;
                case StaffSettingTypes.HRSForm2:
                    outputOptions.SelectedHRSForm2 = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    inputOptions.HRSForm2Id = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    break;
                case StaffSettingTypes.HRSForm3:
                    outputOptions.SelectedHRSForm3 = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    inputOptions.HRSForm3Id = existingSetting.SelectedValueId.ToNullableInt32() ?? 0;
                    break;
                case StaffSettingTypes.HFWMultiRange:
                    outputOptions.SelectedHFWMultiRange = JsonConvert.DeserializeObject<List<OutputDto_DropdownData>>(existingSetting.SelectedValueId);// existingSetting.SelectedValueId;
                    inputOptions.HFWMultiRange = JsonConvert.DeserializeObject<List<OutputDto_DropdownData>>(existingSetting.SelectedValueId);
                    break;
                case StaffSettingTypes.TddMultiRange:
                    outputOptions.SelectedTestDueDates = JsonConvert.DeserializeObject<List<OutputDto_DropdownData_BenchmarkDate>>(existingSetting.SelectedValueId);// existingSetting.SelectedValueId;
                    inputOptions.MultiBenchmarkDates = JsonConvert.DeserializeObject<List<OutputDto_DropdownData_BenchmarkDate>>(existingSetting.SelectedValueId);
                    break;
            }
        }

        public OutputDto_FilterOptions GetUpdatedFilterOptions(InputDto_GetFilterOptions options, List<StaffAssessmentDto> assessments)
        {
            OutputDto_FilterOptions outputOptions = new OutputDto_FilterOptions();

            // get hrsForms regardless of changeType
            if(options.HRSFormEnabled)
            {
                var lookupFields = _dbContext.LookupFields.Where(p => p.FieldName == "HRSIWForm").OrderBy(p => p.SortOrder).ToList();
                outputOptions.HRSForms = Mapper.Map<List<OutputDto_DropdownData>>(lookupFields);
            }
            if (options.HRSForm2Enabled)
            {
                var lookupFields = _dbContext.LookupFields.Where(p => p.FieldName == "HRSIWForm2").OrderBy(p => p.SortOrder).ToList();
                outputOptions.HRSForms2 = Mapper.Map<List<OutputDto_DropdownData>>(lookupFields);
            }
            if (options.HRSForm3Enabled)
            {
                var lookupFields = _dbContext.LookupFields.Where(p => p.FieldName == "HRSIWForm3").OrderBy(p => p.SortOrder).ToList();
                outputOptions.HRSForms3 = Mapper.Map<List<OutputDto_DropdownData>>(lookupFields);
            }
            if (options.StateTestEnabled)
            {
                outputOptions.StateTests = Mapper.Map<List<OutputDto_DropdownData>>(assessments.Where(p => p.TestType == 3 && p.CanImport == true));
            }
            if (options.InterventionTestEnabled)
            {
                outputOptions.InterventionTests = Mapper.Map<List<OutputDto_DropdownData>>(assessments.Where(p => p.TestType == 2 && p.CanImport == true));
            }
            if (options.BenchmarkTestEnabled)
            {
                outputOptions.BenchmarkTests = Mapper.Map<List<OutputDto_DropdownData>>(assessments.Where(p => p.TestType == 1 && p.CanImport == true));
            }
            if (options.ClassroomAssessmentFieldEnabled)
            {
                outputOptions.ClassroomAssessmentFields = LoadClassroomAssessments();
            }
            if (options.InterventionGroupAssessmentFieldEnabled)
            {
                outputOptions.InterventionGroupAssessmentFields = LoadInterventionGroupAssessments();
            }

            switch (options.ChangeType)
            {
                case "stintOverride":
                    var stintSchoolYears = _dbContext.SchoolYears;
                    outputOptions.SchoolYears = Mapper.Map<List<OutputDto_DropdownData>>(stintSchoolYears.OrderBy(p => p.SchoolStartYear).ToList());
                    outputOptions.SelectedSchoolStartYear = options.SchoolYear;

                    var stintSchools = LoadSchools(options);
                    outputOptions.Schools = Mapper.Map<List<OutputDto_DropdownData>>(stintSchools.OrderBy(p => p.Name).ToList());
                    outputOptions.SelectedSchool = options.SchoolId;

                    var stintInterventionists = LoadInterventionists(options);
                    outputOptions.Interventionists = Mapper.Map<List<OutputDto_DropdownData>>(stintInterventionists);
                    outputOptions.SelectedInterventionist = options.InterventionistId;

                    var stintInterventionGroups = LoadInterventionGroups(options);
                    outputOptions.InterventionGroups = Mapper.Map<List<OutputDto_DropdownData>>(stintInterventionGroups);
                    outputOptions.SelectedInterventionGroup = options.InterventionGroupId;

                    var stintStudents = LoadStudentsForInterventionGroup(options);
                    outputOptions.InterventionStudents = Mapper.Map<List<OutputDto_DropdownData>>(stintStudents);
                    outputOptions.SelectedInterventionStudent = options.InterventionStudentId;

                    var stints = LoadStintsForStudentInterventionGroup(options);
                    outputOptions.Stints = Mapper.Map<List<OutputDto_DropdownData>>(stints);
                    outputOptions.SelectedStint = options.StintId;

                    break;
                case "initial":
                    var defaultYear = GetDefaultYear();
                    var schools = LoadSchools(options); // get only the schools the user has access to
                    var schoolYears = _dbContext.SchoolYears;

                    if (options.HFWMultiRangeEnabled)
                    {
                        GetExistingSetting(StaffSettingTypes.HFWMultiRange, outputOptions, options);
                    }

                    if (options.MultiBenchmarkDatesEnabled || 1 == 1)
                    {
                        GetExistingSetting(StaffSettingTypes.TddMultiRange, outputOptions, options);
                    }

                    if (options.ClassroomAssessmentFieldEnabled)
                    {
                        GetExistingSetting(StaffSettingTypes.ClassroomAssessmentField, outputOptions, options);
                    }

                    if (options.InterventionGroupAssessmentFieldEnabled)
                    {
                        GetExistingSetting(StaffSettingTypes.InterventionGroupAssessmentField, outputOptions, options);
                    }

                    if (options.HFWSortOrderEnabled)
                    {
                        GetExistingSetting(StaffSettingTypes.HFWSortOrder, outputOptions, options);
                    }

                    if (options.HRSFormEnabled)
                    {
                        GetExistingSetting(StaffSettingTypes.HRSForm, outputOptions, options);
                    }

                    if (options.HRSForm2Enabled)
                    {
                        GetExistingSetting(StaffSettingTypes.HRSForm2, outputOptions, options);
                    }
                    if (options.HRSForm3Enabled)
                    {
                        GetExistingSetting(StaffSettingTypes.HRSForm3, outputOptions, options);
                    }

                    outputOptions.Schools = Mapper.Map<List<OutputDto_DropdownData>>(schools.OrderBy(p => p.Name).ToList());

                    if (schools.Count == 1)
                    {
                        outputOptions.SelectedSchool = schools.First().Id;
                        options.SchoolId = schools.First().Id;
                    } else
                    {
                        options.SchoolId = -1;
                        GetExistingSetting(StaffSettingTypes.School, outputOptions, options);
                    }

                    var schoolYearsWithTestDueDates = _dbContext.SchoolYears.ToList();
                    var testDueDateYears = _dbContext.TestDueDates.Select(p => p.SchoolStartYear).ToList();
                    if (testDueDateYears.Count == 0)
                    {
                        Log.Warning("WARNING: A district does not have any test due dates set.  This will be a problem.");
                        return outputOptions;
                    }
                    else
                    {
                        schoolYearsWithTestDueDates = schoolYearsWithTestDueDates.ToList();
                    }

                    outputOptions.SchoolYears = Mapper.Map<List<OutputDto_DropdownData>>(schoolYearsWithTestDueDates.OrderBy(p => p.SchoolStartYear).ToList());

                    GetExistingSetting(StaffSettingTypes.SchoolYear, outputOptions, options);
                    if (options.SchoolYear == -1 || outputOptions.SelectedSchoolStartYear == null)
                    {
                        outputOptions.SelectedSchoolStartYear = defaultYear;
                        options.SchoolYear = defaultYear;
                    }

                    var tdds = _dbContext.TestDueDates.Where(p => p.SchoolStartYear == outputOptions.SelectedSchoolStartYear).ToList();
                    outputOptions.TestDueDates = Mapper.Map<List<OutputDto_DropdownData_BenchmarkDate>>(tdds.OrderBy(p => p.DueDate).ToList());

                    var mytdds = _dbContext.TestDueDates.Where(p => p.SchoolStartYear == outputOptions.SelectedSchoolStartYear || p.SchoolStartYear == outputOptions.SelectedSchoolStartYear - 1).ToList();
                    outputOptions.MultiYearTestDueDates = Mapper.Map<List<OutputDto_DropdownData_BenchmarkDate>>(mytdds.OrderBy(p => p.DueDate).ToList());
                    // after 6 hours default
                    outputOptions.SelectedTDD = GetDefaultBenchmarkDate(outputOptions.SelectedSchoolStartYear.Value, false);


                    var initialSchoolGrades = LoadGrades(options);
                    if (initialSchoolGrades.Count == 1)
                    {
                        outputOptions.SelectedGrade = initialSchoolGrades.First().Id;
                        options.GradeId = initialSchoolGrades.First().Id;
                    }
                    else
                    {
                        options.GradeId = -1;
                        GetExistingSetting(StaffSettingTypes.Grade, outputOptions, options);
                    }

                    var initialSchoolTeachers = LoadTeachers(options);
                    if (initialSchoolTeachers.Count == 1)
                    {
                        outputOptions.SelectedTeacher = initialSchoolTeachers.First().Id;
                        options.TeacherId = initialSchoolTeachers.First().Id;
                    }
                    else
                    {
                        options.TeacherId = -1;
                        GetExistingSetting(StaffSettingTypes.Teacher, outputOptions, options);
                    }

                    var initialSchoolInterventionists = LoadInterventionists(options);
                    if (initialSchoolInterventionists.Count == 1)
                    {
                        outputOptions.SelectedInterventionist = initialSchoolInterventionists.First().Id;
                        options.InterventionistId = initialSchoolInterventionists.First().Id;
                    }
                    else
                    {
                        options.InterventionistId = -1;
                        GetExistingSetting(StaffSettingTypes.Interventionist, outputOptions, options);
                    }

                    var initialInterventionGroups = LoadInterventionGroups(options);
                    if (initialInterventionGroups.Count == 1)
                    {
                        outputOptions.SelectedInterventionGroup = initialInterventionGroups.First().Id;
                        options.InterventionGroupId = initialInterventionGroups.First().Id;
                    }
                    else
                    {
                        options.InterventionGroupId = -1;
                        GetExistingSetting(StaffSettingTypes.InterventionGroup, outputOptions, options);
                    }

                    var initialInterventionStudents = LoadStudentsForInterventionGroup(options);
                    if (initialInterventionStudents.Count == 1)
                    {
                        outputOptions.SelectedInterventionStudent = initialInterventionStudents.First().Id;
                        options.InterventionStudentId = initialInterventionStudents.First().Id;
                    }
                    else
                    {
                        options.InterventionStudentId = -1;
                        GetExistingSetting(StaffSettingTypes.InterventionStudent, outputOptions, options);
                    }

                    var initialSchoolSections = LoadSections(options);
                    if (initialSchoolSections.Count == 1)
                    {
                        outputOptions.SelectedSection = initialSchoolSections.First().Id;
                        options.SectionId = initialSchoolSections.First().Id;
                    }
                    else
                    {
                        options.SectionId = -1;
                        GetExistingSetting(StaffSettingTypes.Section, outputOptions, options);
                    }

                    var initialSchoolStudents = LoadStudentsForSection(options);
                    if (initialSchoolStudents.Count == 1)
                    {
                        outputOptions.SelectedSectionStudent = initialSchoolStudents.First().Id;
                        options.SectionStudentId = initialSchoolStudents.First().Id;
                    } else
                    {
                        options.SectionStudentId = -1;
                        GetExistingSetting(StaffSettingTypes.SectionStudent, outputOptions, options);
                    }

                    var initialStints = LoadStintsForStudentInterventionGroup(options);
                    if (initialStints.Count == 1)
                    {
                        outputOptions.SelectedStint = initialStints.First().Id;
                    }else
                    {
                        GetExistingSetting(StaffSettingTypes.Stint, outputOptions, options);
                    }

                    var initialTeamMeetings = LoadTeamMeetingsThisYear(options);
                    if (initialTeamMeetings.Count == 1)
                    {
                        outputOptions.SelectedTeamMeeting = initialTeamMeetings.First().ID;
                        options.TeamMeetingId = initialTeamMeetings.First().ID;
                    } else
                    {
                        options.TeamMeetingId = -1;
                        GetExistingSetting(StaffSettingTypes.TeamMeeting, outputOptions, options);
                    }

                    var initialTeamMeetingStaffs = LoadStaffForTeamMeeting(options);
                    if (initialTeamMeetingStaffs.Count == 1)
                    {
                        outputOptions.SelectedTeamMeetingStaff = initialTeamMeetingStaffs.First().Id;
                    }else
                    {
                        GetExistingSetting(StaffSettingTypes.TeamMeetingStaff, outputOptions, options);
                    }


                    // he have the grade list now
                    outputOptions.Grades = Mapper.Map<List<OutputDto_DropdownData>>(initialSchoolGrades);
                    outputOptions.Teachers = Mapper.Map<List<OutputDto_DropdownData>>(initialSchoolTeachers);
                    outputOptions.Sections = Mapper.Map<List<OutputDto_DropdownData>>(initialSchoolSections);
                    outputOptions.InterventionGroups = Mapper.Map<List<OutputDto_DropdownData>>(initialInterventionGroups);
                    outputOptions.SectionStudents = Mapper.Map<List<OutputDto_DropdownData>>(initialSchoolStudents);
                    outputOptions.InterventionStudents = Mapper.Map<List<OutputDto_DropdownData>>(initialInterventionStudents);
                    outputOptions.Interventionists = Mapper.Map<List<OutputDto_DropdownData>>(initialSchoolInterventionists);
                    outputOptions.TeamMeetings = Mapper.Map<List<OutputDto_DropdownData>>(initialTeamMeetings);
                    outputOptions.TeamMeetingStaffs = Mapper.Map<List<OutputDto_DropdownData>>(initialTeamMeetingStaffs);
                    outputOptions.Stints = Mapper.Map<List<OutputDto_DropdownData>>(initialStints);
                    break;
                case StaffSettingTypes.HFWMultiRange:
                    UpdateSetting(StaffSettingTypes.HFWMultiRange, JsonConvert.SerializeObject(options.HFWMultiRange));
                    break;
                case StaffSettingTypes.HFWSortOrder:
                    UpdateSetting(StaffSettingTypes.HFWSortOrder, options.HFWSortOrder);
                    break;
                case StaffSettingTypes.InterventionGroupAssessmentField:
                    UpdateSetting(StaffSettingTypes.InterventionGroupAssessmentField, options.InterventionGroupAssessmentFieldId.ToString());
                    break;
                case StaffSettingTypes.ClassroomAssessmentField:
                    UpdateSetting(StaffSettingTypes.ClassroomAssessmentField, options.ClassroomAssessmentFieldId.ToString());
                    break;
                case StaffSettingTypes.HRSForm:
                    UpdateSetting(StaffSettingTypes.HRSForm, options.HRSFormId.ToString());
                    break;
                case StaffSettingTypes.HRSForm2:
                    UpdateSetting(StaffSettingTypes.HRSForm2, options.HRSForm2Id.ToString());
                    break;
                case StaffSettingTypes.HRSForm3:
                    UpdateSetting(StaffSettingTypes.HRSForm3, options.HRSForm3Id.ToString());
                    break;
                case StaffSettingTypes.BenchmarkDate:
                    UpdateSetting(StaffSettingTypes.BenchmarkDate, options.BenchmarkDateId.ToString());
                    break;
                case StaffSettingTypes.TddMultiRange:
                    UpdateSetting(StaffSettingTypes.TddMultiRange, JsonConvert.SerializeObject(options.MultiBenchmarkDates));
                    break;
                case StaffSettingTypes.Section:
                    var students = LoadStudentsForSection(options);
                    UpdateSetting(StaffSettingTypes.Section, options.SectionId.ToString());

                    if (students.Count == 1)
                    {
                        outputOptions.SelectedSectionStudent = students.First().Id;
                        UpdateSetting(StaffSettingTypes.SectionStudent, outputOptions.SelectedSectionStudent.Value.ToString());
                    }

                    outputOptions.SectionStudents = Mapper.Map<List<OutputDto_DropdownData>>(students);
                    break;
                case StaffSettingTypes.TeamMeeting:
                    var attendees = LoadStaffForTeamMeeting(options);
                    UpdateSetting(StaffSettingTypes.Section, options.TeamMeetingId.ToString());

                    if (attendees.Count == 1)
                    {
                        outputOptions.SelectedTeamMeetingStaff = attendees.First().Id;
                        UpdateSetting(StaffSettingTypes.TeamMeetingStaff, outputOptions.SelectedTeamMeetingStaff.Value.ToString());
                    }

                    outputOptions.TeamMeetingStaffs = Mapper.Map<List<OutputDto_DropdownData>>(attendees);
                    break;
                case StaffSettingTypes.InterventionStudent:
                    var studentStints = LoadStintsForStudentInterventionGroup(options);
                    UpdateSetting(StaffSettingTypes.InterventionStudent, options.InterventionStudentId.ToString());

                    if (studentStints.Count == 1)
                    {
                        outputOptions.SelectedStint = studentStints.First().Id;
                        UpdateSetting(StaffSettingTypes.Stint, outputOptions.SelectedStint.Value.ToString());
                    }

                    outputOptions.Stints = Mapper.Map<List<OutputDto_DropdownData>>(studentStints);
                    break;
                case StaffSettingTypes.SectionStudent:
                    UpdateSetting(StaffSettingTypes.SectionStudent, options.SectionStudentId.ToString());
                    break;
                case StaffSettingTypes.InterventionGroup:
                    var igStudents = LoadStudentsForInterventionGroup(options);
                    UpdateSetting(StaffSettingTypes.InterventionGroup, options.InterventionGroupId.ToString());

                    if (igStudents.Count == 1)
                    {
                        outputOptions.SelectedInterventionStudent = igStudents.First().Id;
                        options.InterventionStudentId = igStudents.First().Id;
                        UpdateSetting(StaffSettingTypes.InterventionStudent, outputOptions.SelectedInterventionStudent.Value.ToString());
                    }

                    outputOptions.InterventionStudents = Mapper.Map<List<OutputDto_DropdownData>>(igStudents);

                    var igStints = LoadStintsForStudentInterventionGroup(options);

                    if (igStints.Count == 1)
                    {
                        outputOptions.SelectedStint = igStints.First().Id;
                        UpdateSetting(StaffSettingTypes.Stint, outputOptions.SelectedStint.Value.ToString());
                    }

                    outputOptions.Stints = Mapper.Map<List<OutputDto_DropdownData>>(igStints);
                    break;
                case StaffSettingTypes.Teacher:
                    var teacherSections = LoadSections(options);
                    UpdateSetting(StaffSettingTypes.Teacher, options.TeacherId.ToString());

                    // if there's only a single section, set the selected grade to that grade
                    if (teacherSections.Count == 1)
                    {
                        outputOptions.SelectedGrade = teacherSections.First().GradeID;
                        options.GradeId = teacherSections.First().GradeID;
                       // UpdateSetting(StaffSettingTypes.Grade, outputOptions.SelectedGrade.Value);
                        outputOptions.SelectedSection = teacherSections.First().Id;
                        options.SectionId = teacherSections.First().Id;
                        UpdateSetting(StaffSettingTypes.Section, outputOptions.SelectedSection.Value.ToString());
                    }
                    else
                    {
                        // grade should only be set to -1 if the sections for the teacher selected are at multiple grade levels AND sectionEnabled
                        if(options.SectionEnabled)
                        {
                            var distinctGradeCountForTeacher = teacherSections.Select(p => p.GradeID).Distinct();
                            if (distinctGradeCountForTeacher.Count() > 1)
                            {
                                options.GradeId = -1;
                            } else
                            {
                                outputOptions.SelectedGrade = options.GradeId;
                            }
                        } else
                        {
                            // pass the same gradeid back since it will be reselected client side
                            outputOptions.SelectedGrade = options.GradeId;
                        }
                        options.SectionId = -1;
                    }

                    var teacherStudents = LoadStudentsForSection(options);
                    if (teacherStudents.Count == 1)
                    {
                        outputOptions.SelectedSectionStudent = teacherStudents.First().Id;
                        UpdateSetting(StaffSettingTypes.SectionStudent, outputOptions.SelectedSectionStudent.Value.ToString());
                    }

                    outputOptions.Sections = Mapper.Map<List<OutputDto_DropdownData>>(teacherSections);
                    outputOptions.SectionStudents = Mapper.Map<List<OutputDto_DropdownData>>(teacherStudents);
                    break;
                case StaffSettingTypes.Interventionist:
                    var interventionistGroups = LoadInterventionGroups(options);
                    UpdateSetting(StaffSettingTypes.Interventionist, options.InterventionistId.ToString());
                    // if there's only a single section, set the selected grade to that grade
                    if (interventionistGroups.Count == 1)
                    {
                        outputOptions.SelectedInterventionGroup = interventionistGroups.First().Id;
                        options.InterventionGroupId = interventionistGroups.First().Id;
                        UpdateSetting(StaffSettingTypes.InterventionGroup, outputOptions.SelectedInterventionGroup.Value.ToString());
                    }
                    else
                    {
                        options.InterventionGroupId = -1;
                    }

                    var interventionistStudents = LoadStudentsForInterventionGroup(options);
                    if (interventionistStudents.Count == 1)
                    {
                        outputOptions.SelectedInterventionStudent = interventionistStudents.First().Id;
                        UpdateSetting(StaffSettingTypes.InterventionStudent, outputOptions.SelectedInterventionStudent.Value.ToString());
                    }

                    outputOptions.InterventionGroups = Mapper.Map<List<OutputDto_DropdownData>>(interventionistGroups);
                    outputOptions.InterventionStudents = Mapper.Map<List<OutputDto_DropdownData>>(interventionistStudents);
                    break;
                case StaffSettingTypes.Grade:
                    var gradeTeachers = LoadTeachers(options);
                    UpdateSetting(StaffSettingTypes.Grade, options.GradeId.ToString());
                    if (gradeTeachers.Count == 1)
                    {
                        outputOptions.SelectedTeacher = gradeTeachers.First().Id;
                        options.TeacherId = gradeTeachers.First().Id;
                        UpdateSetting(StaffSettingTypes.Teacher, outputOptions.SelectedTeacher.Value.ToString());
                    }
                    else
                    {
                        options.TeacherId = -1;
                    }

                    var gradeSections = LoadSections(options);
                    if (gradeSections.Count == 1)
                    {
                        outputOptions.SelectedSection = gradeSections.First().Id;
                        options.SectionId = gradeSections.First().Id;
                        UpdateSetting(StaffSettingTypes.Section, outputOptions.SelectedSection.Value.ToString());
                    }
                    else
                    {
                        options.SectionId = -1;
                    }

                    var gradeStudents = LoadStudentsForSection(options);
                    if (gradeStudents.Count == 1)
                    {
                        outputOptions.SelectedSectionStudent = gradeStudents.First().Id;
                        UpdateSetting(StaffSettingTypes.SectionStudent, outputOptions.SelectedSectionStudent.Value.ToString());
                    }

                    outputOptions.Teachers = Mapper.Map<List<OutputDto_DropdownData>>(gradeTeachers);
                    outputOptions.Sections = Mapper.Map<List<OutputDto_DropdownData>>(gradeSections);
                    outputOptions.SectionStudents = Mapper.Map<List<OutputDto_DropdownData>>(gradeStudents);
                    break;
                case StaffSettingTypes.SchoolYear:
                    var benchdates = _dbContext.TestDueDates.Where(p =>
                        p.SchoolStartYear == options.SchoolYear
                        ).ToList();
                    UpdateSetting(StaffSettingTypes.SchoolYear, options.SchoolYear.ToString());
                    outputOptions.TestDueDates = Mapper.Map<List<OutputDto_DropdownData_BenchmarkDate>>(benchdates.OrderBy(p => p.DueDate).ToList());
                    outputOptions.SelectedTDD = GetDefaultBenchmarkDate(options.SchoolYear, true);
                    var mybenchdates = _dbContext.TestDueDates.Where(p => p.SchoolStartYear == options.SchoolYear || p.SchoolStartYear == options.SchoolYear - 1).ToList();
                    outputOptions.MultiYearTestDueDates = Mapper.Map<List<OutputDto_DropdownData_BenchmarkDate>>(mybenchdates.OrderBy(p => p.DueDate).ToList());

                    var schoolYearGrades = LoadGrades(options);
                    if (schoolYearGrades.Count == 1)
                    {
                        outputOptions.SelectedGrade = schoolYearGrades.First().Id;
                        options.GradeId = schoolYearGrades.First().Id;
                        UpdateSetting(StaffSettingTypes.Grade, outputOptions.SelectedGrade.Value.ToString());
                    }
                    else
                    {
                        options.GradeId = -1;
                    }

                    var schoolYearTeachers = LoadTeachers(options);
                    if (schoolYearTeachers.Count == 1)
                    {
                        outputOptions.SelectedTeacher = schoolYearTeachers.First().Id;
                        options.TeacherId = schoolYearTeachers.First().Id;
                        UpdateSetting(StaffSettingTypes.Teacher, outputOptions.SelectedTeacher.Value.ToString());
                    }
                    else
                    {
                        options.TeacherId = -1;
                    }

                    var schoolYearInterventionists = LoadInterventionists(options);
                    if (schoolYearInterventionists.Count == 1)
                    {
                        outputOptions.SelectedInterventionist = schoolYearInterventionists.First().Id;
                        options.InterventionistId = schoolYearInterventionists.First().Id;
                        UpdateSetting(StaffSettingTypes.Interventionist, outputOptions.SelectedInterventionist.Value.ToString());
                    }
                    else
                    {
                        options.InterventionistId = -1;
                    }

                    var schoolYearSections = LoadSections(options);
                    if (schoolYearSections.Count == 1)
                    {
                        outputOptions.SelectedSection = schoolYearSections.First().Id;
                        options.SectionId = schoolYearSections.First().Id;
                        UpdateSetting(StaffSettingTypes.Section, outputOptions.SelectedSection.Value.ToString());
                    }
                    else
                    {
                        options.SectionId = -1;
                    }

                    var schoolYearStudents = LoadStudentsForSection(options);
                    if (schoolYearStudents.Count == 1)
                    {
                        outputOptions.SelectedSectionStudent = schoolYearStudents.First().Id;
                        UpdateSetting(StaffSettingTypes.SectionStudent, outputOptions.SelectedSectionStudent.Value.ToString());
                    }

                    // he have the grade list now
                    outputOptions.Grades = Mapper.Map<List<OutputDto_DropdownData>>(schoolYearGrades);
                    outputOptions.Teachers = Mapper.Map<List<OutputDto_DropdownData>>(schoolYearTeachers);
                    outputOptions.Sections = Mapper.Map<List<OutputDto_DropdownData>>(schoolYearSections);
                    outputOptions.SectionStudents = Mapper.Map<List<OutputDto_DropdownData>>(schoolYearStudents);
                    outputOptions.Interventionists = Mapper.Map<List<OutputDto_DropdownData>>(schoolYearInterventionists);

                    break;
                case StaffSettingTypes.School:
                    UpdateSetting(StaffSettingTypes.School, options.SchoolId.ToString());

                    // school change should create new list of grades, interventionists, staff, sections, interventiongroups
                    var schoolGrades = LoadGrades(options);
                    if (schoolGrades.Count == 1)
                    {
                        outputOptions.SelectedGrade = schoolGrades.First().Id;
                        options.GradeId = schoolGrades.First().Id;
                        UpdateSetting(StaffSettingTypes.Grade, outputOptions.SelectedGrade.Value.ToString());
                    }
                    else
                    {
                        options.GradeId = -1;
                    }

                    var schoolTeachers = LoadTeachers(options);
                    if (schoolTeachers.Count == 1)
                    {
                        outputOptions.SelectedTeacher = schoolTeachers.First().Id;
                        options.TeacherId = schoolTeachers.First().Id;
                        UpdateSetting(StaffSettingTypes.Teacher, outputOptions.SelectedTeacher.Value.ToString());
                    }
                    else
                    {
                        options.TeacherId = -1;
                    }

                    var schoolInterventionists = LoadInterventionists(options);
                    if (schoolInterventionists.Count == 1)
                    {
                        outputOptions.SelectedInterventionist = schoolInterventionists.First().Id;
                        options.InterventionistId = schoolInterventionists.First().Id;
                        UpdateSetting(StaffSettingTypes.Interventionist, outputOptions.SelectedInterventionist.Value.ToString());
                    }
                    else
                    {
                        options.InterventionistId = -1;
                    }

                    var schoolSections = LoadSections(options);
                    if (schoolSections.Count == 1)
                    {
                        outputOptions.SelectedSection = schoolSections.First().Id;
                        options.SectionId = schoolSections.First().Id;
                        UpdateSetting(StaffSettingTypes.Section, outputOptions.SelectedSection.Value.ToString());
                    }
                    else
                    {
                        options.SectionId = -1;
                    }

                    var schoolStudents = LoadStudentsForSection(options);
                    if (schoolStudents.Count == 1)
                    {
                        outputOptions.SelectedSectionStudent = schoolStudents.First().Id;
                        UpdateSetting(StaffSettingTypes.SectionStudent, outputOptions.SelectedSectionStudent.Value.ToString());
                    }

                    // he have the grade list now
                    outputOptions.Grades = Mapper.Map<List<OutputDto_DropdownData>>(schoolGrades);
                    outputOptions.Teachers = Mapper.Map<List<OutputDto_DropdownData>>(schoolTeachers);
                    outputOptions.Sections = Mapper.Map<List<OutputDto_DropdownData>>(schoolSections);
                    outputOptions.SectionStudents = Mapper.Map<List<OutputDto_DropdownData>>(schoolStudents);
                    outputOptions.Interventionists = Mapper.Map<List<OutputDto_DropdownData>>(schoolInterventionists);
                    // lets get the list of teachers for these classes
                    break;
            }



            return outputOptions;
        }

        
        public TestDueDate GetNextBenchmarkDate(int schoolStartYear, TestDueDate currentDate)
        {
            if (currentDate == null)
            {
                return null;
            }
            else
            {
                return
                    _dbContext.TestDueDates.Where(
                        p =>
                        p.SchoolStartYear == schoolStartYear &&
                        p.DueDate > currentDate.DueDate).ToList().OrderBy(u => u.DueDate).FirstOrDefault();
            }
        }

        private List<School> LoadSchools(InputDto_GetFilterOptions inputOptions)
        {
            if (!inputOptions.SchoolEnabled)
            {
                return new List<School>();
            }

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




        private List<Grade> LoadGrades(InputDto_GetFilterOptions inputOptions)
        {
            if (!inputOptions.GradeEnabled || inputOptions.SchoolId == -1)
            {
                return new List<Grade>();
            }

            if (IsSchoolAdmin(inputOptions.SchoolId) || IsDistrictAdmin)
            {
                return _dbContext.StaffSections.Where(p =>
                    p.Section.SchoolID == inputOptions.SchoolId &&
                    p.Section.SchoolStartYear == inputOptions.SchoolYear &&
                    p.Section.IsInterventionGroup == false).OrderBy(g => g.Section.Grade.GradeOrder).Select(p => p.Section.Grade).Distinct().ToList();
            }
            else
            {
                // also ensure users with grade level access are accounted for
                var gradesGrantedAccess = _dbContext.StaffSchoolGrades.Where(p =>
                    p.SchoolID == inputOptions.SchoolId &&
                    p.StaffID == _currentUser.Id).Select(p => p.Grade).Distinct().ToList();

                var explicitGradesFromSections = _dbContext.StaffSections.Where(p =>
                   p.StaffID == _currentUser.Id &&
                   p.Section.SchoolID == inputOptions.SchoolId &&
                   p.Section.SchoolStartYear == inputOptions.SchoolYear &&
                   p.Section.IsInterventionGroup == false)
                .OrderBy(g => g.Section.Grade.GradeOrder).Select(p => p.Section.Grade).Distinct().ToList();

                // combine the two lists
                var combinedList = gradesGrantedAccess.Union(explicitGradesFromSections).OrderBy(p => p.GradeOrder).ToList();
                return combinedList;
            }
        }

        // LoadTeamMeetingsYouAreCoManagerOfThisYear
        private List<TeamMeeting> LoadTeamMeetingsThisYear(InputDto_GetFilterOptions inputOptions)
        {
            // if no school or staff selected, just return empty list
            if (!inputOptions.TeamMeetingEnabled || inputOptions.SchoolYear == -1)
            {
                return new List<TeamMeeting>();
            }

            // get meetings where you are an attendee or a manager
            var attendeeMeetings = _dbContext.TeamMeetingAttendances.Where(p =>
                p.StaffID == _currentUser.Id &&
                p.TeamMeeting.SchoolYear == inputOptions.SchoolYear).Select(p => p.TeamMeeting).ToList();

            var managerMeetings = _dbContext.TeamMeetingManagers.Where(p =>
                p.StaffID == _currentUser.Id &&
                p.TeamMeeting.SchoolYear == inputOptions.SchoolYear).Select(p => p.TeamMeeting).ToList();

            var result = attendeeMeetings.Union(managerMeetings).Distinct().ToList();
            return result;
        }

        private List<Staff> LoadStaffForTeamMeeting(InputDto_GetFilterOptions inputOptions)
        {
            // if no school or staff selected, just return empty list
            if (!inputOptions.TeamMeetingStaffEnabled || inputOptions.TeamMeetingId == -1)
            {
                return new List<Staff>();
            }

            var isManager = false;

            // if primary owner
            isManager = _dbContext.TeamMeetings.Any(p => p.ID == inputOptions.TeamMeetingId && p.StaffID == _currentUser.Id);

            if (!isManager)
            {
                // determine if you are a manager for this meeting
                isManager = _dbContext.TeamMeetingManagers.Any(p =>
                   p.StaffID == _currentUser.Id && p.TeamMeeting.ID == inputOptions.TeamMeetingId);
            }

            // get meetings where you are an attendee or a manager
            var attendees = _dbContext.TeamMeetingAttendances.Where(p =>
                (p.StaffID == _currentUser.Id || isManager)  && p.TeamMeeting.ID == inputOptions.TeamMeetingId).Select(p => p.Staff).OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToList();

            return attendees;
        }

        private List<Section> LoadSections(InputDto_GetFilterOptions inputOptions)
        {
            // if no school or staff selected, just return empty list
            if (!inputOptions.SectionEnabled || inputOptions.SchoolId == -1 || inputOptions.TeacherId == -1)
            {
                return new List<Section>();
            }

            return _dbContext.StaffSections.Where(p =>
                p.StaffID == inputOptions.TeacherId &&
                p.Section.SchoolID == inputOptions.SchoolId &&
                p.Section.SchoolStartYear == inputOptions.SchoolYear
                 &&
                (p.Section.GradeID == inputOptions.GradeId || inputOptions.GradeId == -1)).Select(p => p.Section).ToList();

        }

        private List<InterventionGroup> LoadInterventionGroups(InputDto_GetFilterOptions inputOptions)
        {
            // if no school or staff selected, just return empty list
            if (!inputOptions.InterventionGroupEnabled || inputOptions.SchoolId == -1 || inputOptions.InterventionistId == -1)
            {
                return new List<InterventionGroup>();
            }

            return _dbContext.StaffInterventionGroups.Where(p => p.StaffID == inputOptions.InterventionistId && p.InterventionGroup.SchoolID == inputOptions.SchoolId && p.InterventionGroup.SchoolStartYear == inputOptions.SchoolYear).Select(p => p.InterventionGroup).Distinct().OrderBy(p => p.Name).ToList();

        }

        private List<Student> LoadStudentsForSection(InputDto_GetFilterOptions inputOptions)
        {
            // if no school or staff selected, just return empty list
            if (!inputOptions.SectionStudentEnabled || inputOptions.SectionId == -1)
            {
                return new List<Student>();
            }

            return _dbContext.StudentSections.Where(j =>
                       j.ClassID == inputOptions.SectionId &&
                       !(j.Student.IsActive.HasValue && j.Student.IsActive.Value == false))
                    .Select(j => j.Student).Distinct()
                    .OrderBy(c => c.LastName).ToList();

        }

        private List<Student> LoadStudentsForInterventionGroup(InputDto_GetFilterOptions inputOptions)
        {
            // if no school or staff selected, just return empty list
            if (!inputOptions.InterventionStudentEnabled || inputOptions.InterventionGroupId == -1)
            {
                return new List<Student>();
            }

            return _dbContext.StudentInterventionGroups.Where(j =>
                       j.InterventionGroupId == inputOptions.InterventionGroupId &&
                       !(j.Student.IsActive.HasValue && j.Student.IsActive.Value == false))
                    .Select(j => j.Student).Distinct()
                    .OrderBy(c => c.LastName).ToList();

        }

        private List<StudentInterventionGroup> LoadStintsForStudentInterventionGroup(InputDto_GetFilterOptions inputOptions)
        {
            // if no school or staff selected, just return empty list
            if (!inputOptions.StintEnabled || inputOptions.InterventionStudentId == -1 || inputOptions.InterventionGroupId == -1)
            {
                return new List<StudentInterventionGroup>();
            }

            return _dbContext.StudentInterventionGroups.Where(j =>
                       j.InterventionGroupId == inputOptions.InterventionGroupId &&
                       j.StudentID == inputOptions.InterventionStudentId &&
                       !(j.Student.IsActive.HasValue && j.Student.IsActive.Value == false))
                    .OrderByDescending(c => c.StartDate).ToList();

        }

        private List<AssessmentDto> LoadInterventionGroupAssessments()
        {


            var assessments = Mapper.Map<List<AssessmentDto>>(_dbContext.Assessments //.Where(p => p.Enabled)
                        .Include(p => p.Fields)
                        .Where(p => p.TestType == 2).ToList());
            //

            // stupid hack b/c EF6 doesn't allow filtering on Included collections
            foreach (var assessment in assessments)
            {
                assessment.Fields = assessment.Fields.Where(p => ((p.DisplayInObsSummary.HasValue == true && p.DisplayInObsSummary.Value == true) ||
                (p.DisplayInEditResultList.HasValue == true && p.DisplayInEditResultList.Value == true) ||
                (p.DisplayInLineGraphs.HasValue == true && p.DisplayInLineGraphs.Value == true)) && (p.FieldType == "DropdownFromDB" || p.FieldType == "checklist" || p.FieldType == "checklist" || p.FieldType == "DropdownRange" || p.FieldType == "DecimalRange" || p.FieldType == "CalculatedFieldDbBacked")).ToList();
            }

            return  assessments;
        }

        private List<AssessmentDto> LoadClassroomAssessments()
        {
            
            var connectedAssessments = _dbContext.Assessments //.Where(p => p.Enabled)
                            .Where(p => (p.AssessmentIsAvailable.HasValue && p.AssessmentIsAvailable.Value) || (p.AssessmentIsAvailable == null))
                            .Include(p => p.Fields)
                            .Where(p => p.TestType == 1 || p.TestType == 3).ToList();

            // create disconnected dto so we don't have savechanges issues later
            var allAssessmentsICanAccess = Mapper.Map<List<AssessmentDto>>(connectedAssessments);

            // remove any that are removed by the schools
            // get all of the schoolIds that I have access to
            var schoolIds = _dbContext.StaffSchools.Where(p => p.StaffID == _currentUser.Id).Select(p => p.SchoolID).ToList();
            var schoolAssessmentsICantAccess = new List<AssessmentDto>();

            foreach (var districtAccesssibleAssessment in allAssessmentsICanAccess)
            {
                var schoolAssessments = _dbContext.SchoolAssessments.Where(p => schoolIds.Contains(p.SchoolId) && p.AssessmentId == districtAccesssibleAssessment.Id);
                if (schoolAssessments.Count() > 0)
                {
                    if (schoolAssessments.All(p => !p.AssessmentIsAvailable))
                    {
                        schoolAssessmentsICantAccess.Add(districtAccesssibleAssessment);
                    }
                }
            }

            // remove assessments that ALL schools have said are not available
            allAssessmentsICanAccess.RemoveAll(p => schoolAssessmentsICantAccess.Contains(p));

            // remove any that are hidden by the user
            var staffAssessmentsICantAccess = Mapper.Map<List<AssessmentDto>>(_dbContext.StaffAssessments.Where(p => p.StaffId == _currentUser.Id && !p.AssessmentIsAvailable).Select(p => p.Assessment).ToList());
            allAssessmentsICanAccess.RemoveAll(p => staffAssessmentsICantAccess.Contains(p));

            // stupid hack b/c EF6 doesn't allow filtering on Included collections
            foreach (var assessment in allAssessmentsICanAccess)
            {
                assessment.Fields = assessment.Fields.Where(p => ((p.DisplayInObsSummary.HasValue == true && p.DisplayInObsSummary.Value == true) ||
                (p.DisplayInEditResultList.HasValue == true && p.DisplayInEditResultList.Value == true) ||
                (p.DisplayInLineGraphs.HasValue == true && p.DisplayInLineGraphs.Value == true)) && (p.FieldType == "DropdownFromDB" || p.FieldType == "DropdownRange" || p.FieldType == "DecimalRange" || p.FieldType == "CalculatedFieldDbBacked")).ToList();

                var benchmarkedFields = new List<AssessmentFieldDto>();
                foreach (var field in assessment.Fields)
                {
                    if (_dbContext.DistrictBenchmarks.Any(p => p.AssessmentID == assessment.Id && p.AssessmentField.Equals(field.DatabaseColumn, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        benchmarkedFields.Add(field);
                    }
                    else if (_dbContext.DistrictYearlyAssessmentBenchmarks.Any(p => p.AssessmentID == assessment.Id && p.AssessmentField.Equals(field.DatabaseColumn, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        benchmarkedFields.Add(field);
                    }
                }
                assessment.Fields = benchmarkedFields;

            }

            return allAssessmentsICanAccess;
        }
        //private List<OutputDto_StudentQuickSearch> GetStudentQuickSearchResults(InputDto_StudentQuickSearch input)
        //{
        //    // if no school or staff selected, just return empty list
        //    if (!inputOptions.StudentEnabled || inputOptions.SectionId == -1)
        //    {
        //        return new List<Student>();
        //    }

        //    return _dbContext.StudentSections.Where(j =>
        //               j.ClassID == inputOptions.SectionId &&
        //               !(j.Student.IsActive.HasValue && j.Student.IsActive.Value == false))
        //            .Select(j => j.Student).Distinct()
        //            .OrderBy(c => c.LastName).ToList();

        //}


        private List<Staff> LoadTeachers(InputDto_GetFilterOptions inputOptions)
        {
            if (!inputOptions.TeacherEnabled)
            {
                return new List<Staff>();
            }

            // this is only when using the manage staff page, this means we're not looking at teachers for a particular school year
            if (!inputOptions.SchoolYearEnabled)
            {
                // TODO: 50 at a time --- if school is not selected, we load all the staff from the district, but only 50 at a time
                if (inputOptions.SchoolId == -1)
                {
                    if (IsDistrictAdmin)
                    {
                        return _dbContext.Staffs.Where(p => (p.IsActive.HasValue && p.IsActive.Value) || !p.IsActive.HasValue)
                            .OrderBy(p => p.LastName)
                            .ThenBy(p => p.FirstName)
                            .ToList();
                    }
                    else // if you're not a district admin and you haven't selected a school... don't send anything
                    {
                        return new List<Staff>();
                    }
                }
                else
                {
                    // there's a school selected, but we only want to return the list of teachers for a school if you are an admin there... otherwise, you just get yourself
                    if (IsSchoolAdmin(inputOptions.SchoolId))
                    {
                        return _dbContext.StaffSchools.Where(p => ((p.Staff.IsActive.HasValue && p.Staff.IsActive.Value) || !p.Staff.IsActive.HasValue) && p.SchoolID == inputOptions.SchoolId)
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
            }
            else // this is the typical case where we have to care about grade and school
            {
                if (inputOptions.SchoolId == -1)
                {
                    return new List<Staff>();
                }
                if (IsDistrictAdmin || IsSchoolAdmin(inputOptions.SchoolId))
                {
                    if (inputOptions.GradeId == -1)
                    {
                        return _dbContext.StaffSchools.Where(p => ((p.Staff.IsActive.HasValue && p.Staff.IsActive.Value) || !p.Staff.IsActive.HasValue) && p.SchoolID == inputOptions.SchoolId)
                            .Select(p => p.Staff)
                            .Distinct()
                            .OrderBy(p => p.LastName)
                            .ThenBy(p => p.FirstName)
                            .ToList();
                    }
                    else
                    {
                        return _dbContext.StaffSections.Where(p => ((p.Staff.IsActive.HasValue && p.Staff.IsActive.Value) || !p.Staff.IsActive.HasValue)
                            && p.Section.SchoolID == inputOptions.SchoolId
                            && p.Section.GradeID == inputOptions.GradeId
                            && p.Section.IsInterventionGroup == false
                            && p.Section.SchoolStartYear == inputOptions.SchoolYear)
                            .Select(p => p.Staff)
                            .Distinct()
                            .OrderBy(p => p.LastName)
                            .ThenBy(p => p.FirstName)
                            .ToList();
                    }
                }
                else
                {
                    if (inputOptions.GradeId == -1)
                    {
                        return _dbContext.Staffs.Where(p => p.Id == _currentUser.Id).ToList();
                    }
                    else
                    {
                        // if not a school or district admin, get teachers for grades i have access to
                        var staffWithExplicitAccess = _dbContext.StaffSections.Where(p =>
                            p.StaffID == _currentUser.Id &&
                            p.Section.SchoolID == inputOptions.SchoolId &&
                            p.Section.GradeID == inputOptions.GradeId &&
                            p.Section.IsInterventionGroup == false &&
                            p.Section.SchoolStartYear == inputOptions.SchoolYear)
                            .Select(p => p.Staff)
                            .ToList();

                        var gradesCanAccessAtSchool = _dbContext.StaffSchoolGrades.Where(p =>
                            p.StaffID == _currentUser.Id &&
                            p.SchoolID == inputOptions.SchoolId)
                            .Select(p => p.GradeID).ToList();

                        var staffCanAccessByGrade = _dbContext.StaffSections.Where(p =>
                            p.Section.SchoolID == inputOptions.SchoolId &&
                            p.Section.GradeID == inputOptions.GradeId &&
                            gradesCanAccessAtSchool.Contains(inputOptions.GradeId) &&
                            p.Section.IsInterventionGroup == false &&
                            p.Section.SchoolStartYear == inputOptions.SchoolYear)
                            .Select(p => p.Staff)
                            .ToList();

                        return staffWithExplicitAccess.Union(staffCanAccessByGrade)
                            .Distinct()
                            .OrderBy(p => p.LastName)
                            .ThenBy(p => p.FirstName)
                            .ToList();
                    }
                }
            }
        }

        private List<Staff> LoadInterventionists(InputDto_GetFilterOptions inputOptions)
        {
            if (!inputOptions.InterventionistEnabled)
            {
                return new List<Staff>();
            }

            // this is only when using the manage staff page, this means we're not looking at teachers for a particular school year
            if (!inputOptions.SchoolYearEnabled)
            {
                // if school is not selected, we load all the staff from the district, but only 50 at a time
                if (inputOptions.SchoolId == -1)
                {
                    if (IsDistrictAdmin)
                    {
                        return _dbContext.Staffs.Where(p => ((p.IsActive.HasValue && p.IsActive.Value) || !p.IsActive.HasValue) && p.IsInterventionSpecialist)
                            .OrderBy(p => p.LastName)
                            .ThenBy(p => p.FirstName)
                            .ToList();
                    }
                    else // if you're not a district admin and you haven't selected a school... don't send anything
                    {
                        return new List<Staff>();
                    }
                }
                else
                {
                    // there's a school selected, but we only want to return the list of teachers for a school if you are an admin there... otherwise, you just get yourself
                    if (IsSchoolAdmin(inputOptions.SchoolId))
                    {
                        return _dbContext.StaffSchools.Where(p => ((p.Staff.IsActive.HasValue && p.Staff.IsActive.Value) || !p.Staff.IsActive.HasValue) && p.SchoolID == inputOptions.SchoolId && p.Staff.IsInterventionSpecialist)
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
            }
            else // this is the typical case where we have to care about grade and school
            {
                if (inputOptions.SchoolId == -1)
                {
                    return new List<Staff>();
                }
                if (IsDistrictAdmin || IsSchoolAdmin(inputOptions.SchoolId))
                {
                    return _dbContext.StaffSchools.Where(p => ((p.Staff.IsActive.HasValue && p.Staff.IsActive.Value) || !p.Staff.IsActive.HasValue) && p.SchoolID == inputOptions.SchoolId && p.Staff.IsInterventionSpecialist)
                        .Select(p => p.Staff)
                        .Distinct()
                        .OrderBy(p => p.LastName)
                        .ThenBy(p => p.FirstName)
                        .ToList();

                }
                else
                {
                    return _dbContext.Staffs.Where(p => p.Id == _currentUser.Id && p.IsInterventionSpecialist).ToList();
                }
            }
        }


    }
}
