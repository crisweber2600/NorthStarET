using DataAccess;
using Microsoft.WindowsAzure.Storage;
using NorthStar.Core.FileUpload;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using EntityDto.DTO.Admin.Simple;
using NorthStar4.PCL.Entity;
using Northstar.Core.Extensions;
using System.Data;
using Serilog;
using System.Globalization;
using NorthStar4.PCL.DTO;
using EntityDto.DTO.ImportExport;
using EntityDto.Entity;
using Newtonsoft.Json;
using NorthStar.Core;
using Northstar.Core;
using Microsoft.WindowsAzure.Storage.Queue;
using EntityDto.LoginDB.Entity;
using AutoMapper;
using EntityDto.DTO.Admin.InterventionGroup;

namespace NorthStar.EF6
{
    public class ImportTestDataService : NSBaseDataService
    {
        public ImportTestDataService(ClaimsIdentity user, string loginConnectionString) : base(user, loginConnectionString)
        {

        }
      

        public System.Data.DataTable AssessmentDataToDataTable(Assessment assessment, int sectionId, int benchmarkDateId)
        {
            var results = _dbContext.GetAssessmentStudentResults(assessment, sectionId, benchmarkDateId, DateTime.Now, true);

            var fieldsToAdd = GetImportableColumns(assessment);
            
            System.Data.DataTable table = new System.Data.DataTable();
            table.Columns.Add("Student ID", typeof(string));
            table.Columns.Add("Student Last Name", typeof(string));
            table.Columns.Add("Student First Name", typeof(string));
            table.Columns.Add("Date Test Taken");
            ConvertFieldsToDataColumns(table.Columns, fieldsToAdd, false);

            // convert records to datarows
            foreach(var studentResult in results)
            {
                DataRow row = table.NewRow();
                row["Student ID"] = studentResult.StudentIdentifier;
                row["Student Last Name"] = studentResult.StudentName.Split(new string[] { "," }, StringSplitOptions.None)[0].Trim();
                row["Student First Name"] = studentResult.StudentName.Split(new string[] { "," }, StringSplitOptions.None)[1].Trim();
                row["Date Test Taken"] = studentResult.TestDate?.ToShortDateString();

                foreach (var fieldResult in studentResult.FieldResults)
                {
                    var field = GetAssessmentFieldForDbColumn(fieldResult.DbColumn, fieldsToAdd, assessment.Id);
                    if(field != null)
                    {
                        row[field.DisplayLabel] = GetValueForFieldResultByType(fieldResult, field, _dbContext);
                    }
                }

                table.Rows.Add(row);
            }

            return table;

        }

        public static System.Data.DataTable OSResultToDataTable(ObservationSummaryGroupResults results, List<Assessment> assessments, DistrictContext context)
        {
            var fieldsToAdd = new List<AssessmentField>();

            // add all fields to a single list
            assessments.Each(a =>
            {
                fieldsToAdd.AddRange(a.Fields.Where(p => !String.IsNullOrEmpty(p.DatabaseColumn)));
            });


            System.Data.DataTable table = new System.Data.DataTable();
            
            table.Columns.Add("Student Last Name", typeof(string));
            table.Columns.Add("Student First Name", typeof(string));
            table.Columns.Add("Student ID", typeof(string));
            table.Columns.Add("School", typeof(string));
            table.Columns.Add("Grade", typeof(string));
            table.Columns.Add("Teacher (Section)", typeof(string));
            ConvertFieldsToDataColumns(table.Columns, fieldsToAdd, true);

            // convert records to datarows
            foreach (var studentResult in results.StudentResults)
            {
                DataRow row = table.NewRow();
                
                row["Student Last Name"] = studentResult.StudentName.Split(new string[] { "," }, StringSplitOptions.None)[0].Trim();
                row["Student First Name"] = studentResult.StudentName.Split(new string[] { "," }, StringSplitOptions.None)[1].Trim();
                row["Student ID"] = studentResult.StudentIdentifier;
                row["School"] = studentResult.SchoolName;
                row["Grade"] = studentResult.GradeName;
                row["Teacher (Section)"] = studentResult.DelimitedTeacherSections;

                foreach (var fieldResult in studentResult.OSFieldResults)
                {
                    var field = GetAssessmentFieldForDbColumn(fieldResult.DbColumn, fieldsToAdd, fieldResult.AssessmentId);
                    if (field != null && !String.IsNullOrWhiteSpace(field.UniqueImportColumnName))
                    {
                        //var label = String.IsNullOrWhiteSpace(field.DisplayLabel) ? field.ObsSummaryLabel : field.DisplayLabel;
                        row[field.Assessment.AssessmentName + "_" + field.DisplayLabel] = GetValueForFieldResultByType(fieldResult, field, context);
                    }
                }

                table.Rows.Add(row);
            }

            return table;

        }

        public static System.Data.DataTable AllFieldsResultToDataTable(ObservationSummaryGroupResults results, List<Assessment> assessments, DistrictContext context)
        {
            var fieldsToAdd = new List<AssessmentField>();

            // add all fields to a single list
            assessments.Each(a =>
            {
                fieldsToAdd.AddRange(a.Fields.Where(p => !String.IsNullOrEmpty(p.DatabaseColumn)));
            });


            System.Data.DataTable table = new System.Data.DataTable();

            table.Columns.Add("Student Last Name", typeof(string));
            table.Columns.Add("Student First Name", typeof(string));
            table.Columns.Add("Student ID", typeof(string));
            table.Columns.Add("School", typeof(string));
            table.Columns.Add("Grade", typeof(string));
            table.Columns.Add("Teacher (Section)", typeof(string));
            ConvertAllFieldsToDataColumns(table.Columns, fieldsToAdd, true);

            // convert records to datarows
            foreach (var studentResult in results.StudentResults)
            {
                DataRow row = table.NewRow();

                row["Student Last Name"] = studentResult.StudentName.Split(new string[] { "," }, StringSplitOptions.None)[0].Trim();
                row["Student First Name"] = studentResult.StudentName.Split(new string[] { "," }, StringSplitOptions.None)[1].Trim();
                row["Student ID"] = studentResult.StudentIdentifier;
                row["School"] = studentResult.SchoolName;
                row["Grade"] = studentResult.GradeName;
                row["Teacher (Section)"] = studentResult.DelimitedTeacherSections;

                foreach (var fieldResult in studentResult.OSFieldResults)
                {
                    var field = GetAssessmentFieldForDbColumn(fieldResult.DbColumn, fieldsToAdd, fieldResult.AssessmentId);
                    if (field != null && !String.IsNullOrWhiteSpace(field.UniqueImportColumnName))
                    {
                        //var label = String.IsNullOrWhiteSpace(field.DisplayLabel) ? field.ObsSummaryLabel : field.DisplayLabel;
                        row[field.Assessment.AssessmentName + "_" + field.UniqueImportColumnName.Replace(",", "")] = GetValueForFieldResultByType(fieldResult, field, context);
                    }
                }

                table.Rows.Add(row);
            }

            return table;

        }



        public static System.Data.DataTable InterventionDataResultToDataTable(ObservationSummaryGroupResults results, List<Assessment> assessments, DistrictContext context)
        {
            var fieldsToAdd = new List<AssessmentField>();

            // add all fields to a single list
            assessments.Each(a =>
            {
                fieldsToAdd.AddRange(a.Fields.Where(p => !String.IsNullOrEmpty(p.DatabaseColumn)));
            });


            System.Data.DataTable table = new System.Data.DataTable();

            table.Columns.Add("Student Last Name", typeof(string));
            table.Columns.Add("Student First Name", typeof(string));
            table.Columns.Add("Student ID", typeof(string));
            table.Columns.Add("School", typeof(string));
            table.Columns.Add("Grade", typeof(string));
            table.Columns.Add("Teacher (Section)", typeof(string));
            table.Columns.Add("Interventionist", typeof(string));
            table.Columns.Add("Intervention Group Name", typeof(string));
            table.Columns.Add("ELL", typeof(string));
            table.Columns.Add("Special Ed Labels", typeof(string));
            table.Columns.Add("Student Services", typeof(string));
            table.Columns.Add("Class BAS Score", typeof(string));
            table.Columns.Add("Benchmark Date", typeof(string));
            table.Columns.Add("Date Test Taken", typeof(string));
            ConvertFieldsToDataColumns(table.Columns, fieldsToAdd, true);

            // convert records to datarows
            foreach (var studentResult in results.StudentResults)
            {
                DataRow row = table.NewRow();

                row["Student Last Name"] = studentResult.StudentName.Split(new string[] { "," }, StringSplitOptions.None)[0].Trim();
                row["Student First Name"] = studentResult.StudentName.Split(new string[] { "," }, StringSplitOptions.None)[1].Trim();
                row["Student ID"] = studentResult.StudentIdentifier;
                row["School"] = studentResult.SchoolName;
                row["Grade"] = studentResult.GradeName;
                row["Teacher (Section)"] = studentResult.DelimitedTeacherSections;
                row["Interventionist"] = studentResult.Interventionist;
                row["Intervention Group Name"] = studentResult.InterventionGroupName;
                row["ELL"] = studentResult.ELL;
                row["Special Ed Labels"] = studentResult.SPEDLables;
                row["Student Services"] = studentResult.StudentServices;
                row["Class BAS Score"] = studentResult.FPScore;
                row["Benchmark Date"] = studentResult.BenchmarkDate?.ToShortDateString();
                row["Date Test Taken"] = studentResult.DateTestTaken?.ToShortDateString();

                foreach (var fieldResult in studentResult.OSFieldResults)
                {
                    var field = GetAssessmentFieldForDbColumn(fieldResult.DbColumn, fieldsToAdd, fieldResult.AssessmentId);
                    if (field != null)
                    {
                        row[field.Assessment.AssessmentName + "_" + field.DisplayLabel] = GetValueForFieldResultByType(fieldResult, field, context);
                    }
                }

                table.Rows.Add(row);
            }

            return table;

        }

        public static System.Data.DataTable AttendanceResultsToDataTable(List<AttendanceExportInfo> results, DistrictContext context)
        {
            System.Data.DataTable table = new System.Data.DataTable();

            table.Columns.Add("Student Last Name", typeof(string));
            table.Columns.Add("Student First Name", typeof(string));
            table.Columns.Add("Student ID", typeof(string));
            table.Columns.Add("School", typeof(string));
            table.Columns.Add("Interventionist", typeof(string));
            table.Columns.Add("Intervention Group Name", typeof(string));
            table.Columns.Add("Intervention Start Date", typeof(string));
            table.Columns.Add("Intervention End Date", typeof(string));
            table.Columns.Add("Intervention Start Time", typeof(string));
            table.Columns.Add("Intervention End Time", typeof(string));
            table.Columns.Add("Attendance Date", typeof(string));
            table.Columns.Add("Attendance Reason", typeof(string));
            table.Columns.Add("Comment", typeof(string));
            table.Columns.Add("Intervention Type", typeof(string));
            TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");


            // convert records to datarows
            foreach (var result in results)
            {
                DataRow row = table.NewRow();

                DateTime cstStartTime = TimeZoneInfo.ConvertTimeFromUtc(result.GroupStartTime, cstZone);
                DateTime cstEndTime = TimeZoneInfo.ConvertTimeFromUtc(result.GroupEndTime, cstZone);

                row["Student Last Name"] = result.Student.Split(new string[] { "," }, StringSplitOptions.None)[0].Trim();
                row["Student First Name"] = result.Student.Split(new string[] { "," }, StringSplitOptions.None)[1].Trim();
                row["Student ID"] = result.StudentNumber;
                row["School"] = result.SchoolName;
                row["Interventionist"] = result.Interventionist;
                row["Intervention Group Name"] = result.GroupName;
                row["Intervention Start Date"] = result.InterventionStart.ToShortDateString();
                row["Intervention End Date"] = result.InterventionEnd == null ? "No End Date" : result.InterventionEnd.Value.ToShortDateString();
                row["Intervention Start Time"] = cstStartTime.ToShortTimeString() + " " + (cstZone.IsDaylightSavingTime(cstStartTime) ? cstZone.DaylightName : cstZone.StandardName);
                row["Intervention End Time"] = cstEndTime.ToShortTimeString() + " " + (cstZone.IsDaylightSavingTime(cstEndTime) ? cstZone.DaylightName : cstZone.StandardName);
                row["Attendance Date"] = result.AttendanceDate.Value.ToShortDateString();
                row["Attendance Reason"] = result.AttendanceReason;
                row["Comment"] = result.Comment;
                row["Intervention Type"] = result.InterventionType;

                table.Rows.Add(row);
            }

            return table;

        }

        public List<OutputDto_DropdownData> QuickSearchPageTypesToPrint(string search)
        {

            var pageTypes = new List<OutputDto_DropdownData>();

            pageTypes.Add(new OutputDto_DropdownData { id = 1, text = "Class Reports" });
            pageTypes.Add(new OutputDto_DropdownData { id = 2, text = "Student Line Graphs" });
            pageTypes.Add(new OutputDto_DropdownData { id = 3, text = "Student Dashboard" });
            pageTypes.Add(new OutputDto_DropdownData { id = 4, text = "Classroom Dashboard" });
            pageTypes.Add(new OutputDto_DropdownData { id = 5, text = "Student Data Entry Screens" });
            pageTypes.Add(new OutputDto_DropdownData { id = 6, text = "HFW Detailed Student Report" });
            pageTypes.Add(new OutputDto_DropdownData { id = 7, text = "HFW Missing Words Report" });

            return pageTypes;
        }

        public List<OutputDto_DropdownData> QuickSearchTextLevelZones(string search)
        {

            var zones = new List<OutputDto_DropdownData>();

            zones.Add(new OutputDto_DropdownData { id = 1, text = "Red Zone" });
            zones.Add(new OutputDto_DropdownData { id = 2, text = "Yellow Zone" });
            zones.Add(new OutputDto_DropdownData { id = 3, text = "Green Zone" });
            zones.Add(new OutputDto_DropdownData { id = 4, text = "Blue Zone" });

            return zones;
        }

        //public List<OutputDto_DropdownData> QuickSearchHfwStudentReports(string search)
        //{

        //    var zones = new List<OutputDto_DropdownData>();

        //    zones.Add(new OutputDto_DropdownData { id = 1, text = "Detailed Student Report" });
        //    zones.Add(new OutputDto_DropdownData { id = 2, text = "Missing Words Report" });

        //    return zones;
        //}

        public System.Data.DataTable InterventionDataToDataTable(Assessment assessment, int interventionGroupId, int studentId)
        {
            var results = _dbContext.GetIGAssessmentStudentResults(assessment, interventionGroupId, studentId);

            var fieldsToAdd = GetImportableColumns(assessment);

            System.Data.DataTable table = new System.Data.DataTable();
            table.Columns.Add("Student ID", typeof(string));
            table.Columns.Add("Student Last Name", typeof(string));
            table.Columns.Add("Student First Name", typeof(string));
            table.Columns.Add("Date Test Taken");
            ConvertFieldsToDataColumns(table.Columns, fieldsToAdd, false);

            // convert records to datarows
            foreach (var studentResult in results)
            {
                DataRow row = table.NewRow();
                row["Student ID"] = studentResult.StudentIdentifier;
                row["Student Last Name"] = studentResult.StudentName.Split(new string[] { "," }, StringSplitOptions.None)[0].Trim();
                row["Student First Name"] = studentResult.StudentName.Split(new string[] { "," }, StringSplitOptions.None)[1].Trim();
                row["Date Test Taken"] = studentResult.TestDate?.ToShortDateString();

                foreach (var fieldResult in studentResult.FieldResults)
                {
                    var field = GetAssessmentFieldForDbColumn(fieldResult.DbColumn, fieldsToAdd, assessment.Id);
                    if (field != null)
                    {
                        row[field.DisplayLabel] = GetValueForFieldResultByType(fieldResult, field, _dbContext);
                    }
                }

                table.Rows.Add(row);
            }

            return table;

        }

        public static object GetValueForFieldResultByType(IFieldResult fieldResult, AssessmentField field, DistrictContext context)
        {
            switch (field.FieldType)
            {
                case "Checkbox":
                    if (fieldResult.BoolValue == null) return DBNull.Value; else return fieldResult.BoolValue.ToString();
                case "CalculatedFieldDbBacked":
                    if (fieldResult.IntValue == null) return DBNull.Value; else return fieldResult.IntValue;
                case "Textfield":
                case "Textarea":                
                case "CalculatedFieldDbBackedString":
                case "CalculatedFieldDbOnly":
                    if (fieldResult.StringValue == null) return DBNull.Value; else return fieldResult.StringValue;
                case "DecimalRange":
                    if (fieldResult.DecimalValue == null) return DBNull.Value; else return fieldResult.DecimalValue;
                case "Date":
                case "DateCheckbox":
                    if (fieldResult.DateValue == null) return DBNull.Value; else return fieldResult.DateValue;
                case "DropdownFromDB":
                    if (fieldResult.IntValue == null)
                        return DBNull.Value;
                    else
                    {
                        var lookupValue = context.LookupFields.FirstOrDefault(p => p.FieldName == field.LookupFieldName && p.FieldSpecificId == fieldResult.IntValue);
                        return lookupValue.FieldValue;
                    }
                case "DropdownRange":
                    if (fieldResult.IntValue == null) return DBNull.Value; else return fieldResult.IntValue;
                case "checklist": //TODO: 5/17/2020 - knowingly Nerfing importing checklist data
                    if(!String.IsNullOrWhiteSpace(fieldResult.StringValue))
                    {
                        // paying up on 6/30/2020
                        var lookupValues = context.LookupFields.Where(p => p.FieldName == field.LookupFieldName);
                        var concatList = new List<string>();

                        foreach(var value in fieldResult.StringValue.Split(Char.Parse(",")))
                        {
                            var intValue = Int32.Parse(value);
                            var matchingValue = lookupValues.FirstOrDefault(p => p.FieldSpecificId == intValue);

                            if (matchingValue != null)
                            {
                                concatList.Add(matchingValue.FieldValue);
                            }
                        }
                        return string.Join(",", concatList);
                    }
                    return DBNull.Value;
            }

            Log.Error("Unknown Field Type exported: {0}", field.FieldType);
            return DBNull.Value;
        }
              

        public static AssessmentField GetAssessmentFieldForDbColumn(string dbColumn, List<AssessmentField> fields, int assessmentId)
        {
            foreach(var field in fields)
            {
                if(field.DatabaseColumn == dbColumn && field.AssessmentId == assessmentId)
                {
                    return field;
                }
            }

            return null;
            // should never get here
            // Update: We dont' add calculated fields, so we do get here now
            //Log.Error("Column not found for DB Column");
            //throw new Exception("Error loading columns");
        }

        public static void ConvertAllFieldsToDataColumns(DataColumnCollection columns, List<AssessmentField> fields, bool assessmentPrefix)
        {
            foreach(var field in fields)
            {
                var label = field.UniqueImportColumnName.Replace(",", "");
                if (String.IsNullOrWhiteSpace(field.UniqueImportColumnName))
                {
                    continue;
                }

                var columnName = assessmentPrefix ? field.Assessment.AssessmentName + "_" + label : label;
                switch (field.FieldType)
                {
                    case "Checkbox":
                        columns.Add(columnName, typeof(bool));
                        break;
                    case "Textfield":
                    case "Textarea":
                    case "CalculatedFieldDbBacked":
                    case "CalculatedFieldDbBackedString":
                    case "CalculatedFieldDbOnly":
                    case "checklist":
                        columns.Add(columnName, typeof(string));
                        break;
                    case "DecimalRange":
                        columns.Add(columnName, typeof(decimal));
                        break;
                    case "Date":
                    case "DateCheckbox":
                        columns.Add(columnName, typeof(DateTime));
                        break;
                    case "DropdownFromDB":
                        columns.Add(columnName, typeof(string));
                        break;
                    case "DropdownRange":
                        columns.Add(columnName, typeof(int));
                        break;
                }
            }
        }

        public static void ConvertFieldsToDataColumns(DataColumnCollection columns, List<AssessmentField> fields, bool assessmentPrefix)
        {
            foreach (var field in fields)
            {
                var columnName = assessmentPrefix ? field.Assessment.AssessmentName + "_" + field.DisplayLabel : field.DisplayLabel;
                switch (field.FieldType)
                {
                    case "Checkbox":
                        columns.Add(columnName, typeof(bool));
                        break;
                    case "Textfield":
                    case "Textarea":
                    case "CalculatedFieldDbBacked":
                    case "CalculatedFieldDbBackedString":
                    case "CalculatedFieldDbOnly":
                    case "checklist":
                        columns.Add(columnName, typeof(string));
                        break;
                    case "DecimalRange":
                        columns.Add(columnName, typeof(decimal));
                        break;
                    case "Date":
                    case "DateCheckbox":
                        columns.Add(columnName, typeof(DateTime));
                        break;
                    case "DropdownFromDB":
                        columns.Add(columnName, typeof(string));
                        break;
                    case "DropdownRange":
                        columns.Add(columnName, typeof(int));
                        break;
                }
            }
        }

        public static List<AssessmentField> GetImportableColumns(Assessment assessment)
        {
            var fields = assessment.Fields.Where(p => (p.FieldType == "DropdownRange" ||
                p.FieldType == "Textfield" ||
                p.FieldType == "Textarea" ||
                p.FieldType == "Checkbox" ||
                p.FieldType == "DecimalRange" ||
                p.FieldType == "DropdownFromDB" ||
                p.FieldType == "DateCheckbox" ||
                p.FieldType == "checklist" ||
                p.FieldType == "Date") &&
                !String.IsNullOrWhiteSpace(p.DatabaseColumn)).ToList();

            return fields;
        }

        public static List<AssessmentField> GetCalculatedColumns(Assessment assessment)
        {
            var computedFields = assessment.Fields.Where(p => (
                p.FieldType == "CalculatedFieldDbBacked" ||
                p.FieldType == "CalculatedFieldDbBackedString" ||
                p.FieldType == "CalculatedFieldDbOnly") &&
                !String.IsNullOrWhiteSpace(p.DatabaseColumn)).ToList();

            return computedFields;
        }

        public OutputDto_SimpleString GetBenchmarkExporTemplateWithData(InputDto_ExportRequest input)
        {
            var result = new OutputDto_SimpleString();

            var assessment = _dbContext.Assessments
                .Include(p => p.Fields)
                .Include(p => p.FieldGroups)
                .First(p => p.Id == input.AssessmentId);

            var dataTable = AssessmentDataToDataTable(assessment, input.SectionId, input.BenchmarkDateId);
            result.Result = Utility.DataTableToCSVString(dataTable);
            return result;
        }

        public OutputDto_SimpleString GetInterventionExporTemplateWithData(InputDto_InterventionExportRequest input)
        {
            var result = new OutputDto_SimpleString();

            var assessment = _dbContext.Assessments
                .Include(p => p.Fields)
                .Include(p => p.FieldGroups)
                .First(p => p.Id == input.AssessmentId);

            var dataTable = InterventionDataToDataTable(assessment, input.InterventionGroupId, input.StudentId);
            result.Result = Utility.DataTableToCSVString(dataTable);
            return result;
        }

        #region TestLog Classes
        public class OutputDto_StateTestLog : OutputDto_Base
        {
            public OutputDto_StateTestLog()
            {
                HistoryItems = new List<JobStateTestDataImportDto>();
            }

            public List<JobStateTestDataImportDto> HistoryItems { get; set; }
        }
        public class OutputDto_BenchmarkTestLog : OutputDto_Base
        {
            public OutputDto_BenchmarkTestLog()
            {
                HistoryItems = new List<JobBenchmarkDataImportDto>();
            }

            public List<JobBenchmarkDataImportDto> HistoryItems { get; set; }
        }
        public class OutputDto_InterventionTestLog : OutputDto_Base
        {
            public OutputDto_InterventionTestLog()
            {
                HistoryItems = new List<JobInterventionDataImportDto>();
            }

            public List<JobInterventionDataImportDto> HistoryItems { get; set; }
        }
        #endregion

        #region Load User's Import History
        public OutputDto_StateTestLog LoadStateTestImportHistory()
        {
            var result = new OutputDto_StateTestLog();

            var items = _loginContext.JobStateTestDataImports.Where(p => p.StaffId == _currentUser.Id);

            result.HistoryItems = Mapper.Map<List<JobStateTestDataImportDto>>(items.OrderByDescending(p => p.StartDate).ToList());

            result.HistoryItems.Each(p =>
            {
                p.AssessmentName = _dbContext.Assessments.FirstOrDefault(g => g.Id == p.AssessmentId)?.AssessmentName;
                p.SchoolYearVerbose = _dbContext.SchoolYears.FirstOrDefault(g => g.SchoolStartYear == p.SchoolStartYear)?.YearVerbose;
            });
            return result;
        }
        public OutputDto_BenchmarkTestLog LoadBenchmarkTestImportHistory()
        {
            var result = new OutputDto_BenchmarkTestLog();

            var items = _loginContext.JobBenchmarkDataImports.Where(p => p.StaffId == _currentUser.Id);

            result.HistoryItems = Mapper.Map<List<JobBenchmarkDataImportDto>>(items.OrderByDescending(p => p.StartDate).ToList());

            result.HistoryItems.Each(p =>
            {
                p.AssessmentName = _dbContext.Assessments.FirstOrDefault(g => g.Id == p.AssessmentId)?.AssessmentName;
                p.SchoolYearVerbose = _dbContext.SchoolYears.FirstOrDefault(g => g.SchoolStartYear == p.SchoolStartYear)?.YearVerbose;
                p.BenchmarkDate = _dbContext.TestDueDates.FirstOrDefault(g => g.Id == p.BenchmarkDateId)?.DueDate;
                p.RecorderName = _dbContext.Staffs.FirstOrDefault(g => g.Id == p.RecorderId)?.FullName;
            });
            return result;
        }
        public OutputDto_InterventionTestLog LoadInterventionTestImportHistory()
        {
            var result = new OutputDto_InterventionTestLog();

            var items = _loginContext.JobInterventionDataImports.Where(p => p.StaffId == _currentUser.Id);

            result.HistoryItems = Mapper.Map<List<JobInterventionDataImportDto>>(items.OrderByDescending(p => p.StartDate).ToList());

            result.HistoryItems.Each(p =>
            {
                p.AssessmentName = _dbContext.Assessments.FirstOrDefault(g => g.Id == p.AssessmentId)?.AssessmentName;
                p.SchoolYearVerbose = _dbContext.SchoolYears.FirstOrDefault(g => g.SchoolStartYear == p.SchoolStartYear)?.YearVerbose;
                p.InterventionGroupName = _dbContext.InterventionGroups.FirstOrDefault(g => g.Id == p.InterventionGroupId)?.Name;
                p.RecorderName = _dbContext.Staffs.FirstOrDefault(g => g.Id == p.RecorderId)?.FullName;
            });
            return result;
        }
        #endregion

        #region History Logs
        public OutputDto_SimpleString GetHistoryLog(InputDto_SimpleId input)
        {
            var result = new OutputDto_SimpleString();

            var log = _loginContext.JobStateTestDataImports.First(p => p.Id == input.Id).ImportLog;

            result.Result = log;
            return result;
        }
        public OutputDto_SimpleString GetBMHistoryLog(InputDto_SimpleId input)
        {
            var result = new OutputDto_SimpleString();

            var log = _loginContext.JobBenchmarkDataImports.First(p => p.Id == input.Id).ImportLog;

            result.Result = log;
            return result;
        }
        public OutputDto_SimpleString GetIntvHistoryLog(InputDto_SimpleId input)
        {
            var result = new OutputDto_SimpleString();

            var log = _loginContext.JobInterventionDataImports.First(p => p.Id == input.Id).ImportLog;

            result.Result = log;
            return result;
        }
        #endregion

        #region Delete History
        public OutputDto_Base DeleteHistoryItem(InputDto_SimpleId input)
        {
            var result = new OutputDto_Base();
            var historyItem = _loginContext.JobStateTestDataImports.First(p => p.Id == input.Id);

            if(historyItem.StaffId != _currentUser.Id)
            {
                result.Status.StatusCode = StatusCode.AccessDenied;
                result.Status.StatusMessage = "You do not have access to delete this item.";
                return result;
            }

            // delete file from Azure
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);
            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(NSConstants.Azure.AssessmentImportContainer);
            var blob = container.GetBlockBlobReference(historyItem.UploadedFileName);
            blob.DeleteIfExists();

            _loginContext.JobStateTestDataImports.Remove(historyItem);
            _loginContext.SaveChanges();

            return result;
        }

        public OutputDto_Base DeleteBMHistoryItem(InputDto_SimpleId input)
        {
            var result = new OutputDto_Base();
            var historyItem = _loginContext.JobBenchmarkDataImports.First(p => p.Id == input.Id);

            if (historyItem.StaffId != _currentUser.Id)
            {
                result.Status.StatusCode = StatusCode.AccessDenied;
                result.Status.StatusMessage = "You do not have access to delete this item.";
                return result;
            }

            // delete file from Azure
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);
            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(NSConstants.Azure.AssessmentImportContainer);
            var blob = container.GetBlockBlobReference(historyItem.UploadedFileName);
            blob.DeleteIfExists();

            _loginContext.JobBenchmarkDataImports.Remove(historyItem);
            _loginContext.SaveChanges();

            return result;
        }

        public OutputDto_Base DeleteIntvHistoryItem(InputDto_SimpleId input)
        {
            var result = new OutputDto_Base();
            var historyItem = _loginContext.JobInterventionDataImports.First(p => p.Id == input.Id);

            if (historyItem.StaffId != _currentUser.Id)
            {
                result.Status.StatusCode = StatusCode.AccessDenied;
                result.Status.StatusMessage = "You do not have access to delete this item.";
                return result;
            }

            // delete file from Azure
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);
            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(NSConstants.Azure.AssessmentImportContainer);
            var blob = container.GetBlockBlobReference(historyItem.UploadedFileName);
            blob.DeleteIfExists();

            _loginContext.JobInterventionDataImports.Remove(historyItem);
            _loginContext.SaveChanges();

            return result;
        }
        #endregion


        #region Get ExportTemplate CSV
        public OutputDto_SimpleString GetStateTestExportTemplate(InputDto_SimpleId input)
        {
            var result = new OutputDto_SimpleString();

            var inputTemplate = GetStateTestDataImportTemplate(input);

            StringBuilder sb = new StringBuilder();
            for(var i = 0; i < inputTemplate.Fields.Count; i++)
            {
                var field = inputTemplate.Fields[i];

                sb.Append(field.UniqueColumnName);

                if (i < inputTemplate.Fields.Count - 1)
                {
                    sb.Append(",");
                }
            }

            result.Result = sb.ToString();
            return result;
        }
        public OutputDto_SimpleString GetBenchmarkTestExportTemplate(InputDto_SimpleId input)
        {
            var result = new OutputDto_SimpleString();

            var inputTemplate = GetBenchmarkTestDataImportTemplate(input);

            StringBuilder sb = new StringBuilder();
            for (var i = 0; i < inputTemplate.Fields.Count; i++)
            {
                var field = inputTemplate.Fields[i];

                sb.Append(field.UniqueColumnName);

                if (i < inputTemplate.Fields.Count - 1)
                {
                    sb.Append(",");
                }
            }

            result.Result = sb.ToString();
            return result;
        }
        public OutputDto_SimpleString GetInterventionTestExportTemplate(InputDto_SimpleId input)
        {
            var result = new OutputDto_SimpleString();

            var inputTemplate = GetInterventionTestDataImportTemplate(input);

            StringBuilder sb = new StringBuilder();
            for (var i = 0; i < inputTemplate.Fields.Count; i++)
            {
                var field = inputTemplate.Fields[i];

                sb.Append(field.UniqueColumnName);

                if (i < inputTemplate.Fields.Count - 1)
                {
                    sb.Append(",");
                }
            }

            result.Result = sb.ToString();
            return result;
        }
        #endregion





        #region Get Template
        public AssessmentTemplate GetStateTestDataImportTemplate(InputDto_SimpleId input)
        {
            var result = new AssessmentTemplate();

            var assessment = _dbContext.Assessments
                .Include(p => p.Fields)
                .First(p => p.Id == input.Id);

            // for state tests, we need to the following fields
            var studentId = new AssessmentFieldTemplate
            {
                FieldName = "Student ID",
                FieldType = "Label",
                Required = true,
                SortOrder = 1,
                ValidValues = "North Star Student Identifier",
                UniqueColumnName = "Student ID"
            };
            var lastName = new AssessmentFieldTemplate
            {
                FieldName = "Student Last Name",
                FieldType = "Label",
                Required = true,
                SortOrder = 2,
                ValidValues = "Text less than 255 characters.",
                UniqueColumnName = "Student Last Name"
            };
            var firstName = new AssessmentFieldTemplate
            {
                FieldName = "Student First Name",
                FieldType = "Label",
                Required = true,
                SortOrder = 3,
                ValidValues = "Text less than 255 characters.",
                UniqueColumnName = "Student First Name"
            };
            var grade = new AssessmentFieldTemplate
            {
                FieldName = "Grade",
                FieldType = "Label",
                Required = true,
                SortOrder = 4,
                ValidValues = "Any grade value in the range specified.",
                ValidRange = "P, K, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12",
                UniqueColumnName = "Grade"
            };
            var testDate = new AssessmentFieldTemplate
            {
                FieldName = "TestDate",
                FieldType = "Label",
                Required = true,
                SortOrder = 5,
                ValidValues = "A date in the format MM/DD/YYYY (month and day do not need leading zeroes)",
                ValidRange = "Any date.",
                UniqueColumnName = "Date Test Taken"
            };

            var fields = assessment.Fields.Where(p => (p.FieldType == "DropdownRange" ||
                p.FieldType == "Textfield" ||
                p.FieldType == "Textarea" ||
                p.FieldType == "Checkbox" ||
                p.FieldType == "DecimalRange" ||
                p.FieldType == "DropdownFromDB" ||
                p.FieldType == "DateCheckbox" ||
                p.FieldType == "checklist" ||
                p.FieldType == "Date") &&
                !String.IsNullOrWhiteSpace(p.DatabaseColumn)).ToList();

            result.Fields.Add(studentId);
            result.Fields.Add(lastName);
            result.Fields.Add(firstName);
            result.Fields.Add(grade);
            result.Fields.Add(testDate);
            
            foreach(var field in fields)
            {
                result.Fields.Add(ConvertFieldToTemplate(field));
            }

            return result;
        }
        public AssessmentTemplate GetBenchmarkTestDataImportTemplate(InputDto_SimpleId input)
        {
            var result = new AssessmentTemplate();

            var assessment = _dbContext.Assessments
                .Include(p => p.Fields)
                .First(p => p.Id == input.Id);

            // for state tests, we need to the following fields
            var studentId = new AssessmentFieldTemplate
            {
                FieldName = "Student ID",
                FieldType = "Label",
                Required = true,
                SortOrder = 1,
                ValidValues = "North Star Student Identifier",
                UniqueColumnName = "Student ID"
            };
            var lastName = new AssessmentFieldTemplate
            {
                FieldName = "Student Last Name",
                FieldType = "Label",
                Required = true,
                SortOrder = 2,
                ValidValues = "Text less than 255 characters.",
                UniqueColumnName = "Student Last Name"
            };
            var firstName = new AssessmentFieldTemplate
            {
                FieldName = "Student First Name",
                FieldType = "Label",
                Required = true,
                SortOrder = 3,
                ValidValues = "Text less than 255 characters.",
                UniqueColumnName = "Student First Name"
            };
            var testDate = new AssessmentFieldTemplate
            {
                FieldName = "TestDate",
                FieldType = "Label",
                Required = true,
                SortOrder = 5,
                ValidValues = "A date in the format MM/DD/YYYY (month and day do not need leading zeroes)",
                ValidRange = "Any date.",
                UniqueColumnName = "Date Test Taken"
            };

            var fields = assessment.Fields.Where(p => (p.FieldType == "DropdownRange" ||
                p.FieldType == "Textfield" ||
                p.FieldType == "Textarea" ||
                p.FieldType == "Checkbox" ||
                p.FieldType == "DecimalRange" ||
                p.FieldType == "DropdownFromDB" ||
                p.FieldType == "DateCheckbox" ||
                p.FieldType == "checklist" ||
                p.FieldType == "Date") &&
                !String.IsNullOrWhiteSpace(p.DatabaseColumn)).ToList();

            result.Fields.Add(studentId);
            result.Fields.Add(lastName);
            result.Fields.Add(firstName);
            result.Fields.Add(testDate);

            foreach (var field in fields)
            {
                result.Fields.Add(ConvertFieldToTemplate(field));
            }

            return result;
        }
        public AssessmentTemplate GetInterventionTestDataImportTemplate(InputDto_SimpleId input)
        {
            var result = new AssessmentTemplate();

            var assessment = _dbContext.Assessments
                .Include(p => p.Fields)
                .First(p => p.Id == input.Id);

            // for state tests, we need to the following fields
            var studentId = new AssessmentFieldTemplate
            {
                FieldName = "Student ID",
                FieldType = "Label",
                Required = true,
                SortOrder = 1,
                ValidValues = "North Star Student Identifier",
                UniqueColumnName = "Student ID"
            };
            var lastName = new AssessmentFieldTemplate
            {
                FieldName = "Student Last Name",
                FieldType = "Label",
                Required = true,
                SortOrder = 2,
                ValidValues = "Text less than 255 characters.",
                UniqueColumnName = "Student Last Name"
            };
            var firstName = new AssessmentFieldTemplate
            {
                FieldName = "Student First Name",
                FieldType = "Label",
                Required = true,
                SortOrder = 3,
                ValidValues = "Text less than 255 characters.",
                UniqueColumnName = "Student First Name"
            };
            var testDate = new AssessmentFieldTemplate
            {
                FieldName = "TestDate",
                FieldType = "Label",
                Required = true,
                SortOrder = 5,
                ValidValues = "A date in the format MM/DD/YYYY (month and day do not need leading zeroes)",
                ValidRange = "Any date.",
                UniqueColumnName = "Date Test Taken"
            };

            var fields = assessment.Fields.Where(p => (p.FieldType == "DropdownRange" ||
                p.FieldType == "Textfield" ||
                p.FieldType == "Textarea" ||
                p.FieldType == "Checkbox" ||
                p.FieldType == "DecimalRange" ||
                p.FieldType == "DropdownFromDB" ||
                p.FieldType == "DateCheckbox" ||
                p.FieldType == "checklist" ||
                p.FieldType == "Date") &&
                !String.IsNullOrWhiteSpace(p.DatabaseColumn)).ToList();

            result.Fields.Add(studentId);
            result.Fields.Add(lastName);
            result.Fields.Add(firstName);
            result.Fields.Add(testDate);

            foreach (var field in fields)
            {
                result.Fields.Add(ConvertFieldToTemplate(field));
            }

            return result;
        }
        #endregion


        public AssessmentFieldTemplate ConvertFieldToTemplate(AssessmentField field)
        {
            var result = new AssessmentFieldTemplate();
            // assumes a max of 10 non-assessmentfield columns like studentid
            result.SortOrder = 10 + field.FieldOrder;
            result.Required = field.IsRequired;
            result.FieldType = field.FieldType;
            result.FieldName = field.DisplayLabel;
            result.UniqueColumnName = field.UniqueImportColumnName;

            switch (field.FieldType)
            {
                case "Textfield":
                case "Textarea":
                    result.ValidValues = "Text of any length.";
                    break;
                case "Checkbox":
                    result.ValidValues = "true or false";
                    break;
                case "DecimalRange":
                    result.ValidValues = "Any decimal value in the range specified.";
                    result.ValidRange = field.RangeLow + " - " + field.RangeHigh;
                    break;
                case "DateCheckbox":
                case "Date":
                    result.ValidValues = "Any valid date in the format 'DD-MMM-YYYY' or 'MM/DD/YYYY'";
                    break;
                case "DropdownFromDB":
                case "checklist":
                    result.ValidValues = "Any value from the list of available values.";
                    var vals = _dbContext.LookupFields.Where(p => p.FieldName == field.LookupFieldName).OrderBy(p => p.SortOrder).Select(p => p.FieldValue).ToList();
                    if(vals.Count > 0)
                    {
                        result.ValidRange = string.Join(", ", vals);
                    }
                    break;
                case "DropdownRange":
                    result.ValidValues = "Any integer value in the range specified.";
                    result.ValidRange = field.RangeLow + " - " + field.RangeHigh;
                    break;
            }

            return result;
        }


        #region Class and Intervention Group Import routines
        public async Task<OutputDto_Log> ProcessClassBenchmarkFile(ImportTestDataViewModel files, IPhotoManager manager)
        {
            var result = new OutputDto_Log();

            // eventually pass in assessment ID and benchmarkdate for benchmark assessments
            // get formdata
            int assessmentId = Int32.Parse(files.FormData["AssessmentId"]);
            int benchmarkDateId = Int32.Parse(files.FormData["BenchmarkDateId"]);
            int recorderId = _currentUser.Id;


            var assessment = _dbContext.Assessments
                .Include(p => p.Fields)
                .Include(p => p.FieldGroups)
                .First(p => p.Id == assessmentId);

            var fields = ImportTestDataService.GetImportableColumns(assessment);
            var computedFields = ImportTestDataService.GetCalculatedColumns(assessment);

            // to import data
            // state test data has some fields that are required, same for benchmark... based on type, make sure that those fields are available first
            // create simple tryParse functions to validate
            foreach (var file in files.Files) // should one be one
            {

                var textReaders = await manager.Download(file.Url);
                var textReader = (textReaders.Count() == 1 ? textReaders.First() : null);

                if (textReader != null)
                {
                    var read = DataAccess.DataTable.New.Read(textReader);
                    // TODO: make sure the same column can't be used twice, see if it throws an exception, otherwise we need to check for it

                    if (assessment.TestType == 1)
                    {
                        // chosen and passed in: benchmarkdate, assessmentid
                        // validate studentid
                        var columns = read.ColumnNames.ToList();
                        if (!columns.Contains("STUDENT ID", StringComparer.OrdinalIgnoreCase))
                        {
                            result.Status.StatusCode = StatusCode.UserDisplayableException;
                            result.Status.StatusMessage = "The StudentId column is missing.  Please ensure the column is labeled correctly.";
                        }
                        else
                        {
                            // get columns that are on the list
                            var validFields = new List<AssessmentField>();
                            foreach (var column in columns)
                            {
                                foreach (var field in fields)
                                {
                                    if (field.DisplayLabel.Equals(column, StringComparison.OrdinalIgnoreCase))
                                    {
                                        validFields.Add(field);
                                    }
                                }
                            }


                            // loop over each row and see which fields are part of the assessment and build an update/insert statement
                            foreach (var row in read.Rows)
                            {
                                var recordIsValid = true;
                                var studentCode = row["STUDENT ID"];

                                foreach (var column in columns)
                                {
                                    foreach (var field in fields)
                                    {
                                        if (field.DisplayLabel.Equals(column, StringComparison.OrdinalIgnoreCase))
                                        {
                                            if (row[column] == null && field.IsRequired)
                                            {
                                                result.LogItems.Add(String.Format("The following student's record did not have a value for the required field '{3}' and is being skipped: {0}, {1} - {2} \r\n", row["Student Last Name"], row["Student First Name"], studentCode, column));
                                                recordIsValid = false;
                                                break;
                                            }
                                        }
                                    }
                                }

                                // make sure required fields have values
                                if (!recordIsValid)
                                {
                                    continue;
                                }

                                // determine if this is an update or insert by running a query
                                // remove leading zeros
                                var student = _dbContext.Students.FirstOrDefault(p => p.StudentIdentifier == studentCode);

                                if (student == null)
                                {
                                    result.LogItems.Add(String.Format("The following student was not found in the database: {0}, {1} - {2} \r\n", row["Student Last Name"], row["Student First Name"], studentCode));
                                    continue; // skip this kid
                                }
                                else
                                {
                                    // now see if this kid already has a record in the table for this this tdd
                                    var isInsert = IsInsert(student.Id, benchmarkDateId, assessment.StorageTable, _dbContext);
                                    var insertUpdateSql = new StringBuilder();

                                    using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
                                    {
                                        try
                                        {
                                            _dbContext.Database.Connection.Open();
                                            command.CommandTimeout = command.Connection.ConnectionTimeout;

                                            // need to detect null on testdate
                                            // also need to update TestDueDateID and Recorder

                                            if (isInsert)
                                            {
                                                insertUpdateSql.AppendFormat("INSERT INTO {0} (StudentId, SectionId, RecorderId, TestDueDateId, DateTestTaken", assessment.StorageTable);

                                                // for each
                                                foreach (var field in computedFields)
                                                {
                                                    insertUpdateSql.AppendFormat(",{0}", field.DatabaseColumn);
                                                }

                                                // for each
                                                foreach (var field in validFields)
                                                {
                                                    insertUpdateSql.AppendFormat(",{0}", field.DatabaseColumn);
                                                }
                                                // TODO: 10/1/2015  add a LastModified date to the end, figure out where to get SectionID (DONT NEED), RecorderID can passIN
                                                insertUpdateSql.AppendFormat(") VALUES ({0}, {1}, {2}, {3},'{4}'", student.Id, -1, recorderId, benchmarkDateId, row["Date Test Taken"].ToNullableDate() ?? DateTime.Now);
                                                //for each
                                                foreach (var field in computedFields)
                                                {
                                                    insertUpdateSql.AppendFormat(",{0}", GetFieldInsertUpdateStringCalculatedFields(field, assessment, row, result, _dbContext)); // use the rest of the fields to calculate
                                                }

                                                foreach (var field in validFields)
                                                {

                                                    insertUpdateSql.AppendFormat(",{0}", GetFieldInsertUpdateString(field, row, result, _dbContext));

                                                }
                                                insertUpdateSql.AppendFormat(")");
                                            }
                                            else
                                            {
                                                insertUpdateSql.AppendFormat("UPDATE {0} SET ", assessment.StorageTable);

                                                // update recorder
                                                //insertUpdateSql.AppendFormat("{0} = {1},", "RecorderID", recorderId);
                                                insertUpdateSql.AppendFormat("{0} = '{1}',", "DateTestTaken", row["Date Test Taken"].ToNullableDate() ?? DateTime.Now);
                                                insertUpdateSql.AppendFormat("{0} = {1},", "IsCopied", 0);

                                                foreach (var field in computedFields)
                                                {
                                                    insertUpdateSql.AppendFormat("{0} = {1},", field.DatabaseColumn, GetFieldInsertUpdateStringCalculatedFields(field, assessment, row, result, _dbContext));
                                                }

                                                // don't include fields that we don't have fields for
                                                foreach (var field in validFields)
                                                {
                                                    // don't try to update fields that don't have a dbcolumn
                                                    //if (control.FieldType == "CalculatedFieldDbBacked" || control.FieldType == "CalculatedFieldDbOnly" || control.FieldType == "CalculatedFieldDbBackedString")
                                                    //{
                                                    //    insertUpdateSql.AppendFormat("{0} = {1},", field.DbColumn, GetFieldInsertUpdateStringCalculatedFields(assessment, field, control, input.StudentResult));
                                                    //}
                                                    insertUpdateSql.AppendFormat("{0} = {1},", field.DatabaseColumn, GetFieldInsertUpdateString(field, row, result, _dbContext));
                                                }
                                                // remove trailing comma
                                                insertUpdateSql.Remove(insertUpdateSql.Length - 1, 1);
                                                insertUpdateSql.AppendFormat(" WHERE StudentId = {0} AND TestDueDateId = {1}", student.Id, benchmarkDateId); // or testdate = {4}
                                            }

                                            command.CommandText = insertUpdateSql.ToString();
                                            command.ExecuteNonQuery();
                                        }
                                        catch (Exception ex)
                                        {
                                            //log
                                            throw ex;
                                        }
                                        finally
                                        {
                                            _dbContext.Database.Connection.Close();
                                            command.Parameters.Clear();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    

                }

            }

            return result;
        }

        public async Task<OutputDto_Log> ProcessInterventionGroupFile(ImportTestDataViewModel files, IPhotoManager manager)
        {
            var result = new OutputDto_Log();

            // eventually pass in assessment ID and benchmarkdate for benchmark assessments
            // get formdata
            int assessmentId = Int32.Parse(files.FormData["AssessmentId"]);
            int interventionGroupId = Int32.Parse(files.FormData["InterventionGroupId"]);
            int studentId = Int32.Parse(files.FormData["StudentId"]);
            int recorderId = _currentUser.Id;


            var assessment = _dbContext.Assessments
                .Include(p => p.Fields)
                .First(p => p.Id == assessmentId);

            var fields = ImportTestDataService.GetImportableColumns(assessment);
            var computedFields = ImportTestDataService.GetCalculatedColumns(assessment);

            // to import data
            // state test data has some fields that are required, same for benchmark... based on type, make sure that those fields are available first
            // create simple tryParse functions to validate
            foreach (var file in files.Files) // should one be one
            {

                var textReaders = await manager.Download(file.Url);
                var textReader = (textReaders.Count() == 1 ? textReaders.First() : null);

                if (textReader != null)
                {
                    var read = DataAccess.DataTable.New.Read(textReader);
                    // TODO: make sure the same column can't be used twice, see if it throws an exception, otherwise we need to check for it

                    if (assessment.TestType == 2)
                    {
                        // chosen and passed in: benchmarkdate, assessmentid
                        // validate studentid
                        var columns = read.ColumnNames.ToList();
                        if (!columns.Contains("STUDENT ID", StringComparer.OrdinalIgnoreCase))
                        {
                            result.Status.StatusCode = StatusCode.UserDisplayableException;
                            result.Status.StatusMessage = "The StudentId column is missing.  Please ensure the column is labeled correctly.";
                        }
                        else
                        {
                            // get columns that are on the list
                            var validFields = new List<AssessmentField>();
                            foreach (var column in columns)
                            {
                                foreach (var field in fields)
                                {
                                    if (field.DisplayLabel.Equals(column, StringComparison.OrdinalIgnoreCase))
                                    {
                                        validFields.Add(field);
                                    }
                                }
                            }


                            // loop over each row and see which fields are part of the assessment and build an update/insert statement
                            foreach (var row in read.Rows)
                            {
                                var recordIsValid = true;
                                var studentCode = row["STUDENT ID"];

                                foreach (var column in columns)
                                {
                                    foreach (var field in fields)
                                    {
                                        if (field.DisplayLabel.Equals(column, StringComparison.OrdinalIgnoreCase))
                                        {
                                            if (row[column] == null && field.IsRequired)
                                            {
                                                result.LogItems.Add(String.Format("The following student's record did not have a value for the required field '{3}' and is being skipped: {0}, {1} - {2} \r\n", row["Student Last Name"], row["Student First Name"], studentCode, column));
                                                recordIsValid = false;
                                                break;
                                            }
                                        }
                                    }
                                }

                                // make sure required fields have values
                                if (!recordIsValid)
                                {
                                    continue;
                                }

                                // determine if this is an update or insert by running a query
                                // remove leading zeros
                                var student = _dbContext.Students.FirstOrDefault(p => p.StudentIdentifier == studentCode);

                                if (student == null)
                                {
                                    result.LogItems.Add(String.Format("The following student was not found in the database: {0}, {1} - {2} \r\n", row["Student Last Name"], row["Student First Name"], studentCode));
                                    continue; // skip this kid
                                }
                                else
                                {
                                    // now see if this kid already has a record in the table for this this tdd
                                    var isInsert = IsInterventionInsert(student.Id, interventionGroupId, DateTime.Parse(row["Date Test Taken"]), assessment.StorageTable, _dbContext);
                                    var insertUpdateSql = new StringBuilder();

                                    using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
                                    {
                                        try
                                        {
                                            _dbContext.Database.Connection.Open();
                                            command.CommandTimeout = command.Connection.ConnectionTimeout;

                                            // need to detect null on testdate
                                            // also need to update TestDueDateID and Recorder

                                            if (isInsert)
                                            {
                                                insertUpdateSql.AppendFormat("INSERT INTO {0} (StudentId, InterventionGroupId, RecorderId, TestDueDate", assessment.StorageTable);

                                                // for each
                                                foreach (var field in computedFields)
                                                {
                                                    insertUpdateSql.AppendFormat(",{0}", field.DatabaseColumn);
                                                }

                                                // for each
                                                foreach (var field in validFields)
                                                {
                                                    insertUpdateSql.AppendFormat(",{0}", field.DatabaseColumn);
                                                }
                                                // TODO: 10/1/2015  add a LastModified date to the end, figure out where to get SectionID (DONT NEED), RecorderID can passIN
                                                insertUpdateSql.AppendFormat(") VALUES ({0},{1},{2},'{3}'",
                                                    student.Id,
                                                    interventionGroupId,
                                                    recorderId,
                                                    DateTime.Parse(row["Date Test Taken"]));
                                                //for each
                                                foreach (var field in computedFields)
                                                {
                                                    insertUpdateSql.AppendFormat(",{0}", GetFieldInsertUpdateStringCalculatedFields(field, assessment, row, result, _dbContext)); // use the rest of the fields to calculate
                                                }

                                                foreach (var field in validFields)
                                                {

                                                    insertUpdateSql.AppendFormat(",{0}", GetFieldInsertUpdateString(field, row, result, _dbContext));

                                                }
                                                insertUpdateSql.AppendFormat(")");
                                            }
                                            else
                                            {
                                                insertUpdateSql.AppendFormat("UPDATE {0} SET ", assessment.StorageTable);

                                                // update recorder
                                                insertUpdateSql.AppendFormat("{0} = '{1}',", "TestDueDate", DateTime.Parse(row["Date Test Taken"]));

                                                foreach (var field in computedFields)
                                                {
                                                    insertUpdateSql.AppendFormat("{0} = {1},", field.DatabaseColumn, GetFieldInsertUpdateStringCalculatedFields(field, assessment, row, result, _dbContext));
                                                }

                                                // don't include fields that we don't have fields for
                                                foreach (var field in validFields)
                                                {
                                                    // don't try to update fields that don't have a dbcolumn
                                                    //if (control.FieldType == "CalculatedFieldDbBacked" || control.FieldType == "CalculatedFieldDbOnly" || control.FieldType == "CalculatedFieldDbBackedString")
                                                    //{
                                                    //    insertUpdateSql.AppendFormat("{0} = {1},", field.DbColumn, GetFieldInsertUpdateStringCalculatedFields(assessment, field, control, input.StudentResult));
                                                    //}
                                                    insertUpdateSql.AppendFormat("{0} = {1},", field.DatabaseColumn, GetFieldInsertUpdateString(field, row, result, _dbContext));
                                                }
                                                // remove trailing comma
                                                insertUpdateSql.Remove(insertUpdateSql.Length - 1, 1);
                                                insertUpdateSql.AppendFormat(" WHERE StudentId = {0} AND InterventionGroupId = {1} and TestDueDate = '{2}'", student.Id, interventionGroupId, DateTime.Parse(row["Date Test Taken"]));
                                            }

                                            command.CommandText = insertUpdateSql.ToString();
                                            command.ExecuteNonQuery();
                                        }
                                        catch (Exception ex)
                                        {
                                            //log
                                            throw ex;
                                        }
                                        finally
                                        {
                                            _dbContext.Database.Connection.Close();
                                            command.Parameters.Clear();
                                        }
                                    }
                                }
                            }
                        }
                    }

                }

            }

            return result;
        }

        #endregion
        //public async Task<OutputDto_Log> ProcessMNImportedFiles(ImportTestDataViewModel files, IPhotoManager manager)
        //{
        //    var result = new OutputDto_Log();

        //    // get the MN prelim assessemnts
        //    // TODO:  pass in a flag that says prelim or not... hard code for now
        //    Assessment assessment = null;

        //    var readingAssessment = _dbContext.Assessments
        //        .Include(p => p.Fields)
        //        .First(p => p.AssessmentName == "MN Reading-Prelim");
        //    var readingFields = readingAssessment.Fields.Where(p => (p.FieldType == "DropdownRange" ||
        //        p.FieldType == "Textfield" ||
        //        p.FieldType == "Textarea" ||
        //        p.FieldType == "Checkbox" ||
        //        p.FieldType == "DecimalRange" ||
        //        p.FieldType == "DropdownFromDB" ||
        //        p.FieldType == "DateCheckbox" ||
        //        p.FieldType == "Date") &&
        //        !String.IsNullOrWhiteSpace(p.DatabaseColumn)).ToList();

        //    var mathAssessment = _dbContext.Assessments
        //        .Include(p => p.Fields)
        //        .First(p => p.AssessmentName == "MN Math-Prelim");
        //    var mathFields = mathAssessment.Fields.Where(p => (p.FieldType == "DropdownRange" ||
        //        p.FieldType == "Textfield" ||
        //        p.FieldType == "Textarea" ||
        //        p.FieldType == "Checkbox" ||
        //        p.FieldType == "DecimalRange" ||
        //        p.FieldType == "DropdownFromDB" ||
        //        p.FieldType == "DateCheckbox" ||
        //        p.FieldType == "Date") &&
        //        !String.IsNullOrWhiteSpace(p.DatabaseColumn)).ToList();

        //    var scienceAssessment = _dbContext.Assessments
        //        .Include(p => p.Fields)
        //        .First(p => p.AssessmentName == "MN Science-Prelim");
        //    var scienceFields = scienceAssessment.Fields.Where(p => (p.FieldType == "DropdownRange" ||
        //        p.FieldType == "Textfield" ||
        //        p.FieldType == "Textarea" ||
        //        p.FieldType == "Checkbox" ||
        //        p.FieldType == "DecimalRange" ||
        //        p.FieldType == "DropdownFromDB" ||
        //        p.FieldType == "DateCheckbox" ||
        //        p.FieldType == "Date") &&
        //        !String.IsNullOrWhiteSpace(p.DatabaseColumn)).ToList();

        //    List<AssessmentField> fields = null;

        //    // to import data
        //    // state test data has some fields that are required, same for benchmark... based on type, make sure that those fields are available first
        //    // create simple tryParse functions to validate
        //    foreach (var file in files.Files) // should one be one
        //    {

        //        var textReaders = await manager.Download(file.Url);
        //        var textReader = (textReaders.Count() == 1 ? textReaders.First() : null);

        //        if (textReader != null)
        //        {
        //            var read = DataAccess.DataTable.New.Read(textReader);
        //            // TODO: make sure the same column can't be used twice, see if it throws an exception, otherwise we need to check for it
        //            // get type
        //            // chosen and passed in: benchmarkdate, assessmentid
        //            // validate studentid
        //            var columns = read.ColumnNames.ToList();
        //            if (!columns.Contains("State Student ID", StringComparer.OrdinalIgnoreCase))
        //            {
        //                result.Status.StatusCode = StatusCode.UserDisplayableException;
        //                result.Status.StatusMessage = "The State Student ID column is missing.  Please ensure the column is labeled correctly.";
        //            }
        //            else if (!columns.Contains("MARSS", StringComparer.OrdinalIgnoreCase))
        //            {
        //                result.Status.StatusCode = StatusCode.UserDisplayableException;
        //                result.Status.StatusMessage = "The MARSS column is missing.  Please ensure the column is labeled correctly.";
        //            }
        //            else if (!columns.Contains("Grade", StringComparer.OrdinalIgnoreCase))
        //            {
        //                result.Status.StatusCode = StatusCode.UserDisplayableException;
        //                result.Status.StatusMessage = "The Grade column is missing.  Please ensure the column is labeled correctly.";
        //            }
        //            else if (!columns.Contains("TestDate", StringComparer.OrdinalIgnoreCase))
        //            {
        //                result.Status.StatusCode = StatusCode.UserDisplayableException;
        //                result.Status.StatusMessage = "The TestDate column is missing.  Please ensure the column is labeled correctly.";
        //            }
        //            else if (!columns.Contains("Subject", StringComparer.OrdinalIgnoreCase))
        //            {
        //                result.Status.StatusCode = StatusCode.UserDisplayableException;
        //                result.Status.StatusMessage = "The Subject column is missing.  Please ensure the column is labeled correctly.";
        //            }
        //            else
        //            {                        
        //                // loop over each row and see which fields are part of the assessment and build an update/insert statement
        //                foreach (var row in read.Rows)
        //                {
        //                    switch (row["Subject"])
        //                    {
        //                        case "Science":
        //                            assessment = scienceAssessment;
        //                            fields = scienceFields;
        //                            break;
        //                        case "Reading":
        //                            assessment = readingAssessment;
        //                            fields = readingFields;
        //                            break;
        //                        case "Math":
        //                            assessment = mathAssessment;
        //                            fields = mathFields;
        //                            break;
        //                    }


        //                    // get columns that are on the list
        //                    var validFields = new List<AssessmentField>();
        //                    foreach (var column in columns)
        //                    {
        //                        foreach (var field in fields)
        //                        {
        //                            if (field.DisplayLabel.Equals(column, StringComparison.OrdinalIgnoreCase))
        //                            {
        //                                validFields.Add(field);
        //                            }
        //                        }
        //                    }

        //                    // determine if this is an update or insert by running a query
        //                    var studentCode = row["State Student ID"]; // also check for staffID!!!
        //                    var student = _dbContext.Students.FirstOrDefault(p => p.StudentIdentifier == studentCode);

        //                    if (student == null)
        //                    {
        //                        result.LogItems.Add(String.Format("The following student was not found in the database: {0}", studentCode));
        //                        continue; // skip this kid
        //                    }
        //                    else
        //                    {
        //                        // now see if this kid already has a record in the table for this this tdd
        //                        var isInsert = IsStateInsert(student.Id, row["Grade"], assessment.StorageTable, _dbContext);
        //                        var insertUpdateSql = new StringBuilder();

        //                        using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
        //                        {
        //                            try
        //                            {
        //                                _dbContext.Database.Connection.Open();
        //                                command.CommandTimeout = command.Connection.ConnectionTimeout;

        //                                // need to detect null on testdate
        //                                // also need to update TestDueDateID and Recorder

        //                                if (isInsert)
        //                                {
        //                                    insertUpdateSql.AppendFormat("INSERT INTO {0} (TestDate", assessment.StorageTable);

        //                                    // for each
        //                                    foreach (var field in validFields)
        //                                    {
        //                                        insertUpdateSql.AppendFormat(",{0}", field.DatabaseColumn);
        //                                    }
        //                                    // TODO: 10/1/2015  add a LastModified date to the end, figure out where to get SectionID (DONT NEED), RecorderID can passIN
        //                                    insertUpdateSql.AppendFormat(") VALUES ('{0}'", DateTime.ParseExact(row["TestDate"], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None));

        //                                    foreach (var field in validFields)
        //                                    {
        //                                        insertUpdateSql.AppendFormat(",{0}", GetFieldInsertUpdateString(field, row, result, _dbContext));
        //                                    }
        //                                    insertUpdateSql.AppendFormat(")");
        //                                }
        //                                else
        //                                {
        //                                    insertUpdateSql.AppendFormat("UPDATE {0} SET ", assessment.StorageTable);

        //                                    // update test_date
        //                                    insertUpdateSql.AppendFormat("{0} = '{1}',", "TestDate", DateTime.ParseExact(row["TestDate"], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None));

        //                                    // don't include fields that we don't have fields for
        //                                    foreach (var field in validFields)
        //                                    {
        //                                        insertUpdateSql.AppendFormat("{0} = {1},", field.DatabaseColumn, GetFieldInsertUpdateString(field, row, result, _dbContext));
        //                                    }
        //                                    // remove trailing comma
        //                                    insertUpdateSql.Remove(insertUpdateSql.Length - 1, 1);
        //                                    insertUpdateSql.AppendFormat(" WHERE StateStudentID = '{0}' AND Grade = {1}", row["State Student ID"], row["Grade"]); 
        //                                }

        //                                command.CommandText = insertUpdateSql.ToString();
        //                                command.ExecuteNonQuery();
        //                            }
        //                            catch (Exception ex)
        //                            {
        //                                //log
        //                                throw ex;
        //                            }
        //                            finally
        //                            {
        //                                _dbContext.Database.Connection.Close();
        //                                command.Parameters.Clear();
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //    }

        //    return result;
        //}

        //public async Task<OutputDto_Log> ProcessMNFinalFiles(ImportTestDataViewModel files, IPhotoManager manager)
        //{
        //    var result = new OutputDto_Log();

        //    // get the MN prelim assessemnts
        //    // TODO:  pass in a flag that says prelim or not... hard code for now
        //    Assessment assessment = null;

        //    var readingAssessment = _dbContext.Assessments
        //        .Include(p => p.Fields)
        //        .First(p => p.AssessmentName == "MCA Reading");
        //    var readingFields = readingAssessment.Fields.Where(p => (p.FieldType == "DropdownRange" ||
        //        p.FieldType == "Textfield" ||
        //        p.FieldType == "Textarea" ||
        //        p.FieldType == "Checkbox" ||
        //        p.FieldType == "DecimalRange" ||
        //        p.FieldType == "DropdownFromDB" ||
        //        p.FieldType == "DateCheckbox" ||
        //        p.FieldType == "Date") &&
        //        !String.IsNullOrWhiteSpace(p.DatabaseColumn)).ToList();

        //    var mathAssessment = _dbContext.Assessments
        //        .Include(p => p.Fields)
        //        .First(p => p.AssessmentName == "MCA Math");
        //    var mathFields = mathAssessment.Fields.Where(p => (p.FieldType == "DropdownRange" ||
        //        p.FieldType == "Textfield" ||
        //        p.FieldType == "Textarea" ||
        //        p.FieldType == "Checkbox" ||
        //        p.FieldType == "DecimalRange" ||
        //        p.FieldType == "DropdownFromDB" ||
        //        p.FieldType == "DateCheckbox" ||
        //        p.FieldType == "Date") &&
        //        !String.IsNullOrWhiteSpace(p.DatabaseColumn)).ToList();

        //    var scienceAssessment = _dbContext.Assessments
        //        .Include(p => p.Fields)
        //        .First(p => p.AssessmentName == "MCA Science");
        //    var scienceFields = scienceAssessment.Fields.Where(p => (p.FieldType == "DropdownRange" ||
        //        p.FieldType == "Textfield" ||
        //        p.FieldType == "Textarea" ||
        //        p.FieldType == "Checkbox" ||
        //        p.FieldType == "DecimalRange" ||
        //        p.FieldType == "DropdownFromDB" ||
        //        p.FieldType == "DateCheckbox" ||
        //        p.FieldType == "Date") &&
        //        !String.IsNullOrWhiteSpace(p.DatabaseColumn)).ToList();

        //    List<AssessmentField> fields = null;

        //    // to import data
        //    // state test data has some fields that are required, same for benchmark... based on type, make sure that those fields are available first
        //    // create simple tryParse functions to validate
        //    foreach (var file in files.Files) // should one be one
        //    {

        //        var textReaders = await manager.Download(file.Url);
        //        var textReader = (textReaders.Count() == 1 ? textReaders.First() : null);

        //        if (textReader != null)
        //        {
        //            var read = DataAccess.DataTable.New.Read(textReader);
        //            // TODO: make sure the same column can't be used twice, see if it throws an exception, otherwise we need to check for it
        //            // get type
        //            // chosen and passed in: benchmarkdate, assessmentid
        //            // validate studentid
        //            var columns = read.ColumnNames.ToList();
        //            if (!columns.Contains("STATE STUDENT ID", StringComparer.OrdinalIgnoreCase))
        //            {
        //                result.Status.StatusCode = StatusCode.UserDisplayableException;
        //                result.Status.StatusMessage = "The State Student ID column is missing.  Please ensure the column is labeled correctly.";
        //            }
        //            else if (!columns.Contains("MARSS", StringComparer.OrdinalIgnoreCase))
        //            {
        //                result.Status.StatusCode = StatusCode.UserDisplayableException;
        //                result.Status.StatusMessage = "The MARSS column is missing.  Please ensure the column is labeled correctly.";
        //            }
        //            else if (!columns.Contains("GRADE", StringComparer.OrdinalIgnoreCase))
        //            {
        //                result.Status.StatusCode = StatusCode.UserDisplayableException;
        //                result.Status.StatusMessage = "The Grade column is missing.  Please ensure the column is labeled correctly.";
        //            }
        //            else if (!columns.Contains("LATEST_TEST_START_DATE", StringComparer.OrdinalIgnoreCase))
        //            {
        //                result.Status.StatusCode = StatusCode.UserDisplayableException;
        //                result.Status.StatusMessage = "The TestDate column is missing.  Please ensure the column is labeled correctly.";
        //            }
        //            else if (!columns.Contains("SUBJECT", StringComparer.OrdinalIgnoreCase))
        //            {
        //                result.Status.StatusCode = StatusCode.UserDisplayableException;
        //                result.Status.StatusMessage = "The Subject column is missing.  Please ensure the column is labeled correctly.";
        //            }
        //            else
        //            {
        //                // loop over each row and see which fields are part of the assessment and build an update/insert statement
        //                foreach (var row in read.Rows)
        //                {
        //                    switch (row["SUBJECT"])
        //                    {
        //                        case "Science":
        //                            assessment = scienceAssessment;
        //                            fields = scienceFields;
        //                            break;
        //                        case "Reading":
        //                            assessment = readingAssessment;
        //                            fields = readingFields;
        //                            break;
        //                        case "Mathematics":
        //                            assessment = mathAssessment;
        //                            fields = mathFields;
        //                            break;
        //                    }


        //                    // get columns that are on the list
        //                    var validFields = new List<AssessmentField>();
        //                    foreach (var column in columns)
        //                    {
        //                        foreach (var field in fields)
        //                        {
        //                            if (field.DisplayLabel.Equals(column, StringComparison.OrdinalIgnoreCase))
        //                            {
        //                                validFields.Add(field);
        //                            }
        //                        }
        //                    }

        //                    // determine if this is an update or insert by running a query
        //                    var studentCode = row["STATE STUDENT ID"]; // also check for staffID!!!
        //                    var student = _dbContext.Students.FirstOrDefault(p => p.StudentIdentifier == studentCode);

        //                    if (student == null)
        //                    {
        //                        result.LogItems.Add(String.Format("The following student was not found in the database: {0}", studentCode));
        //                        continue; // skip this kid
        //                    }
        //                    else
        //                    {
        //                        // now see if this kid already has a record in the table for this this tdd
        //                        var isInsert = IsStateInsert(student.Id, row["GRADE"], assessment.StorageTable, _dbContext);
        //                        var insertUpdateSql = new StringBuilder();

        //                        using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
        //                        {
        //                            try
        //                            {
        //                                _dbContext.Database.Connection.Open();
        //                                command.CommandTimeout = command.Connection.ConnectionTimeout;

        //                                // need to detect null on testdate
        //                                // also need to update TestDueDateID and Recorder

        //                                if (isInsert)
        //                                {
        //                                    insertUpdateSql.AppendFormat("INSERT INTO {0} (TestDate", assessment.StorageTable);

        //                                    // for each
        //                                    foreach (var field in validFields)
        //                                    {
        //                                        insertUpdateSql.AppendFormat(",{0}", field.DatabaseColumn);
        //                                    }
        //                                    // TODO: 10/1/2015  add a LastModified date to the end, figure out where to get SectionID (DONT NEED), RecorderID can passIN
        //                                    insertUpdateSql.AppendFormat(") VALUES ('{0}'", DateTime.ParseExact(row["LATEST_TEST_START_DATE"], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None));

        //                                    foreach (var field in validFields)
        //                                    {
        //                                        insertUpdateSql.AppendFormat(",{0}", GetFieldInsertUpdateString(field, row, result, _dbContext));
        //                                    }
        //                                    insertUpdateSql.AppendFormat(")");
        //                                }
        //                                else
        //                                {
        //                                    insertUpdateSql.AppendFormat("UPDATE {0} SET ", assessment.StorageTable);

        //                                    // update test_date
        //                                    insertUpdateSql.AppendFormat("{0} = '{1}',", "TestDate", DateTime.ParseExact(row["LATEST_TEST_START_DATE"], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None));

        //                                    // don't include fields that we don't have fields for
        //                                    foreach (var field in validFields)
        //                                    {
        //                                        insertUpdateSql.AppendFormat("{0} = {1},", field.DatabaseColumn, GetFieldInsertUpdateString(field, row, result, _dbContext));
        //                                    }
        //                                    // remove trailing comma
        //                                    insertUpdateSql.Remove(insertUpdateSql.Length - 1, 1);
        //                                    insertUpdateSql.AppendFormat(" WHERE StateStudentID = '{0}' AND Grade = {1}", row["STATE STUDENT ID"], row["GRADE"]);
        //                                }

        //                                command.CommandText = insertUpdateSql.ToString();
        //                                command.ExecuteNonQuery();
        //                            }
        //                            catch (Exception ex)
        //                            {
        //                                //log
        //                                throw ex;
        //                            }
        //                            finally
        //                            {
        //                                _dbContext.Database.Connection.Close();
        //                                command.Parameters.Clear();
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //    }

        //    return result;
        //}

        #region Add Job to Queue
        public async Task<OutputDto_Log> ProcessStateTestUpload(ImportTestDataViewModel files, IPhotoManager manager)
        {
            var result = new OutputDto_Log();
            int assessmentId = Int32.Parse(files.FormData["AssessmentId"]);
            int schoolYear = Int32.Parse(files.FormData["SchoolYear"]);

            if (assessmentId == 0 || schoolYear == 0)
            {
                result.LogItems.Add("One or more of the required fields has not been selected.  Please select all options before uploading a file.");
                return result;
            }

            // get the fields that are supposed to be in the file
            var fields = GetStateTestDataImportTemplate(new InputDto_SimpleId { Id = assessmentId }).Fields;

            // get the file from Azure so that we can work with it
            var uploadedFile = files.Files.First();
            var textReaders = await manager.Download(uploadedFile.Url);
            var textReader = (textReaders.Count() == 1 ? textReaders.First() : null);



            // to import data
            // state test data has some fields that are required, same for benchmark... based on type, make sure that those fields are available first
            // create simple tryParse functions to validate


            if (textReader != null)
            {
                var read = DataAccess.DataTable.New.Read(textReader);
                // TODO: make sure the same column can't be used twice, see if it throws an exception, otherwise we need to check for it
                // get type
                // chosen and passed in: benchmarkdate, assessmentid
                // validate studentid
                var columns = read.ColumnNames.ToList();

                foreach (var expectedField in fields)
                {
                    if (!columns.Contains(expectedField.UniqueColumnName, StringComparer.OrdinalIgnoreCase))
                    {
                        //result.Status.StatusCode = StatusCode.UserDisplayableException;
                        //result.Status.StatusMessage = "One or more columns are missing from the imported file. Review the log below for details.";

                        result.LogItems.Add(String.Format("The column '{0}' is missing from the uploaded file.  Please ensure the column is included and labeled correctly.", expectedField.UniqueColumnName));
                    }
                }
            }

            // if no errors... add a log item and add message to queue
            if(result.LogItems.Count != 0)
            {
                return result;
            }

            var newJob = _loginContext.JobStateTestDataImports.Create();
            newJob.AssessmentId = assessmentId;
            newJob.StaffId = _currentUser.Id;
            newJob.StaffEmail = _currentUser.Email;
            newJob.StartDate = DateTime.Now;
            newJob.Status = "Awaiting processing";
            newJob.UploadedFileName = uploadedFile.Name;
            newJob.UploadedFileUrl = uploadedFile.Url;
            newJob.SchoolStartYear = schoolYear;
            _loginContext.JobStateTestDataImports.Add(newJob);
            _loginContext.SaveChanges();

            Utility.AddNewImportQueueMessage(newJob.Id, NSConstants.Azure.JobType.StateTestImport);

            return result;
        }
        public async Task<OutputDto_Log> ProcessBenchmarkTestUpload(ImportTestDataViewModel files, IPhotoManager manager)
        {
            var result = new OutputDto_Log();
            int assessmentId = Int32.Parse(files.FormData["AssessmentId"]);
            int schoolYear = Int32.Parse(files.FormData["SchoolYear"]);
            int benchmarkDateId = Int32.Parse(files.FormData["BenchmarkDateId"]);
            int recorderId = Int32.Parse(files.FormData["RecorderId"]);

            // make sure the required fields were included
            if(assessmentId == 0 || schoolYear == 0 || benchmarkDateId == 0 || recorderId == 0)
            {
                result.LogItems.Add("One or more of the required fields has not been selected.  Please select all options before uploading a file.");
                return result;
            }


            // get the fields that are supposed to be in the file
            var fields = GetBenchmarkTestDataImportTemplate(new InputDto_SimpleId { Id = assessmentId }).Fields;

            // get the file from Azure so that we can work with it
            var uploadedFile = files.Files.First();
            var textReaders = await manager.Download(uploadedFile.Url);
            var textReader = (textReaders.Count() == 1 ? textReaders.First() : null);



            // to import data
            // state test data has some fields that are required, same for benchmark... based on type, make sure that those fields are available first
            // create simple tryParse functions to validate


            if (textReader != null)
            {
                var read = DataAccess.DataTable.New.Read(textReader);
                // TODO: make sure the same column can't be used twice, see if it throws an exception, otherwise we need to check for it
                // get type
                // chosen and passed in: benchmarkdate, assessmentid
                // validate studentid
                var columns = read.ColumnNames.ToList();

                foreach (var expectedField in fields)
                {
                    if (!columns.Contains(expectedField.UniqueColumnName, StringComparer.OrdinalIgnoreCase))
                    {
                        //result.Status.StatusCode = StatusCode.UserDisplayableException;
                        //result.Status.StatusMessage = "One or more columns are missing from the imported file. Review the log below for details.";

                        result.LogItems.Add(String.Format("The column '{0}' is missing from the uploaded file.  Please ensure the column is included and labeled correctly.", expectedField.UniqueColumnName));
                    }
                }
            }

            // if no errors... add a log item and add message to queue
            if (result.LogItems.Count != 0)
            {
                return result;
            }

            var newJob = _loginContext.JobBenchmarkDataImports.Create();
            newJob.AssessmentId = assessmentId;
            newJob.StaffId = _currentUser.Id;
            newJob.StaffEmail = _currentUser.Email;
            newJob.StartDate = DateTime.Now;
            newJob.Status = "Awaiting processing";
            newJob.UploadedFileName = uploadedFile.Name;
            newJob.UploadedFileUrl = uploadedFile.Url;
            newJob.SchoolStartYear = schoolYear;
            newJob.BenchmarkDateId = benchmarkDateId;
            newJob.RecorderId = recorderId;
            _loginContext.JobBenchmarkDataImports.Add(newJob);
            _loginContext.SaveChanges();

            Utility.AddNewImportQueueMessage(newJob.Id, NSConstants.Azure.JobType.BenchmarkTestImport);

            return result;
        }
        public async Task<OutputDto_Log> ProcessInterventionTestUpload(ImportTestDataViewModel files, IPhotoManager manager)
        {
            var result = new OutputDto_Log();
            int assessmentId = Int32.Parse(files.FormData["AssessmentId"]);
            int schoolYear = Int32.Parse(files.FormData["SchoolYear"]);
            int interventionGroupId = Int32.Parse(files.FormData["InterventionGroupId"]);
            int recorderId = Int32.Parse(files.FormData["RecorderId"]);

            if (assessmentId == 0 || schoolYear == 0 || interventionGroupId == 0 || recorderId == 0)
            {
                result.LogItems.Add("One or more of the required fields has not been selected.  Please select all options before uploading a file.");
                return result;
            }

            // get the fields that are supposed to be in the file
            var fields = GetInterventionTestDataImportTemplate(new InputDto_SimpleId { Id = assessmentId }).Fields;

            // get the file from Azure so that we can work with it
            var uploadedFile = files.Files.First();
            var textReaders = await manager.Download(uploadedFile.Url);
            var textReader = (textReaders.Count() == 1 ? textReaders.First() : null);



            // to import data
            // state test data has some fields that are required, same for benchmark... based on type, make sure that those fields are available first
            // create simple tryParse functions to validate


            if (textReader != null)
            {
                var read = DataAccess.DataTable.New.Read(textReader);
                // TODO: make sure the same column can't be used twice, see if it throws an exception, otherwise we need to check for it
                // get type
                // chosen and passed in: benchmarkdate, assessmentid
                // validate studentid
                var columns = read.ColumnNames.ToList();

                foreach (var expectedField in fields)
                {
                    if (!columns.Contains(expectedField.UniqueColumnName, StringComparer.OrdinalIgnoreCase))
                    {
                        //result.Status.StatusCode = StatusCode.UserDisplayableException;
                        //result.Status.StatusMessage = "One or more columns are missing from the imported file. Review the log below for details.";

                        result.LogItems.Add(String.Format("The column '{0}' is missing from the uploaded file.  Please ensure the column is included and labeled correctly.", expectedField.UniqueColumnName));
                    }
                }
            }

            // if no errors... add a log item and add message to queue
            if (result.LogItems.Count != 0)
            {
                return result;
            }

            var newJob = _loginContext.JobInterventionDataImports.Create();
            newJob.AssessmentId = assessmentId;
            newJob.StaffId = _currentUser.Id;
            newJob.StaffEmail = _currentUser.Email;
            newJob.StartDate = DateTime.Now;
            newJob.Status = "Awaiting processing";
            newJob.UploadedFileName = uploadedFile.Name;
            newJob.UploadedFileUrl = uploadedFile.Url;
            newJob.SchoolStartYear = schoolYear;
            newJob.RecorderId = recorderId;
            newJob.InterventionGroupId = interventionGroupId;
            _loginContext.JobInterventionDataImports.Add(newJob);
            _loginContext.SaveChanges();

            Utility.AddNewImportQueueMessage(newJob.Id, NSConstants.Azure.JobType.InterventionTestImport);

            return result;
        }
        #endregion

        //#region Azure Helper Function
        //private void AddNewImportQueueMessage(int jobId, NSConstants.Azure.JobType jobType)
        //{
        //    var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);
        //    CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

        //    // Retrieve a reference to a container.
        //    CloudQueue queue = queueClient.GetQueueReference(NSConstants.Azure.JobQueue);

        //    // Create the queue if it doesn't already exist
        //    queue.CreateIfNotExists();

        //    CloudQueueMessage message = new CloudQueueMessage(JsonConvert.SerializeObject(new NSAzureJob { JobId = jobId, JobType = jobType }));
        //    queue.AddMessage(message);
        //}
        //#endregion

        #region DB Helper Functions

        public static string GetDbColumnForColumn(string columnDisplayName, Assessment assessment)
        {
            return assessment.Fields.FirstOrDefault(p => p.DisplayLabel.Equals(columnDisplayName, StringComparison.OrdinalIgnoreCase))?.DatabaseColumn;
        }


        public static string GetFieldInsertUpdateStringCalculatedFields(AssessmentField field, Assessment assessment, Row row, OutputDto_Log result, DistrictContext _dbContext)
        {
            switch (field.FieldType)
            {
                case "CalculatedFieldDbBacked":
                case "CalculatedFieldDbBackedString":
                    int sum = 0;

                    if (field.CalculationFunction == "Sum")
                    {
                        var fieldsToSum = field.CalculationFields.Split(Char.Parse(","));

                        foreach (var currentColumn in row.ColumnNames)
                        {
                            foreach (var fieldNameToSum in fieldsToSum)
                            {
                                if (GetDbColumnForColumn(currentColumn, assessment) == fieldNameToSum)
                                {
                                    sum += row[currentColumn].ToNullableInt32() ?? 0;
                                }
                            }
                        }
                        return sum.ToString();
                    }
                    if (field.CalculationFunction == "SumBool")
                    {
                        var fieldsToSum = field.CalculationFields.Split(Char.Parse(","));

                        foreach (var currentColumn in row.ColumnNames)
                        {
                            foreach (var fieldNameToSum in fieldsToSum)
                            {
                                if (GetDbColumnForColumn(currentColumn, assessment) == fieldNameToSum)
                                {
                                    sum += row[currentColumn] == "1" ? 1 : 0;
                                }
                            }
                        }
                        return sum.ToString();
                    }
                    if (field.CalculationFunction == "SumBoolByGroup")
                    {
                        foreach (var group in assessment.FieldGroups)
                        {
                            var groupId = group.Id;

                            // i don't like having this reference to the field... need to figure out
                            // if it makes more sense to pass the additional data for each field
                            // or to just join them on the client
                            foreach (var currentColumn in row.ColumnNames)
                            {
                                // fix this later so that field properties are part of the fieldresults
                                // it is really getting stupid to keep looking this up
                                var currentField = assessment.Fields.First(p => p.DatabaseColumn == GetDbColumnForColumn(currentColumn, assessment));

                                if (currentField.GroupId == groupId && GetDbColumnForColumn(currentColumn, assessment).Substring(0, 3) == "chk")
                                {
                                    // only add each groupid once

                                    if (row[currentColumn] == "1")
                                    {
                                        sum++;
                                        break;
                                    }
                                }
                            }
                        }

                        return sum.ToString();
                    }
                    if (field.CalculationFunction == "ConcatenatedMissingLetters")
                    {
                        var unknownLetters = "";

                        foreach (var group in assessment.FieldGroups)
                        {
                            var groupId = group.Id;

                            var foundInGroup = false;

                            foreach (var currentColumn in row.ColumnNames)
                            {
                                var currentField = assessment.Fields.First(p => p.DatabaseColumn == GetDbColumnForColumn(currentColumn, assessment));

                                if (currentField.GroupId == groupId && GetDbColumnForColumn(currentColumn, assessment).Substring(0, 3) == "chk")
                                {
                                    if (row[currentColumn] == "1")
                                    {
                                        foundInGroup = true;
                                        break;
                                    }
                                }
                            }
                            if (!foundInGroup)
                            {
                                // how to get the letter? find the field with the same groupid and print its DisplayLabel
                                foreach (var currentField in assessment.Fields)
                                {

                                    if (currentField.GroupId == groupId && currentField.FieldType == "Label")
                                    {
                                        unknownLetters += currentField.DisplayLabel + ",";
                                        break;
                                    }
                                }
                            }
                        }
                        //remove trailing comma
                        if (unknownLetters.Length > 0)
                        {
                            unknownLetters = unknownLetters.Substring(0, unknownLetters.Length - 1);
                        }
                        else
                        {
                            unknownLetters = "none";
                        }
                        return String.Format("'{0}'", unknownLetters); ;
                    }
                    return "0";
                case "CalculatedFieldDbOnly":
                    string stringValue = String.Empty;
                    if (field.CalculationFunction == "BenchmarkLevel")
                    {
                        // calculate benchmark value and return
                        using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
                        {
                            try
                            {
                                // FPValueId, Accuracy, CompScore (may need to calculate, since it might have changed)
                                //Database.AsSqlServer().Connection.DbConnection.Open();
                                command.CommandText = "dbo.ns4_udf_CalculateBenchmarkLevel";
                                command.CommandType = CommandType.StoredProcedure;
                                command.CommandTimeout = command.Connection.ConnectionTimeout;
                                var outParameter = command.CreateParameter();
                                outParameter.Direction = ParameterDirection.ReturnValue;
                                outParameter.ParameterName = "@RETURN_VALUE";
                                outParameter.DbType = DbType.String;
                                outParameter.Size = 50;

                                var fpValueId = command.CreateParameter();
                                fpValueId.Direction = ParameterDirection.Input;
                                fpValueId.DbType = DbType.Int32;
                                fpValueId.ParameterName = "@FPValueID";
                                var rowValue = row["Level"];
                                var fpField = row.ColumnNames.Contains("Level") ? _dbContext.LookupFields.FirstOrDefault(p => p.FieldName == "FPScale" && p.FieldValue == rowValue)?.FieldSpecificId : null;// result.FieldResults.FirstOrDefault(p => p.DbColumn == "FPValueID");
                                fpValueId.Value = fpField == null ? 0 : fpField.Value;

                                var accuracy = command.CreateParameter();
                                accuracy.Direction = ParameterDirection.Input;
                                accuracy.DbType = DbType.Int32;
                                accuracy.ParameterName = "@Accuracy";
                                var accuracyField = row.ColumnNames.Contains("Accuracy") ? row["Accuracy"] : null;//result.FieldResults.FirstOrDefault(p => p.DbColumn == "Accuracy");
                                accuracy.Value = accuracyField == null ? 0 : accuracyField.ToNullableDecimal() ?? 0;

                                var compScore = command.CreateParameter();
                                compScore.Direction = ParameterDirection.Input;
                                compScore.DbType = DbType.Int32;
                                compScore.ParameterName = "@CompScore";
                                int newCompScoreSum = 0;
                                var aboutField = row.ColumnNames.Contains("About") ? row["About"] : null; //resul.FieldResults.FirstOrDefault(p => p.DbColumn == "About");
                                var withinField = row.ColumnNames.Contains("Within") ? row["Within"] : null; //  result.FieldResults.FirstOrDefault(p => p.DbColumn == "Within");
                                var beyondField = row.ColumnNames.Contains("Beyond") ? row["Beyond"] : null; // result.FieldResults.FirstOrDefault(p => p.DbColumn == "Beyond");
                                var extraPtField = row.ColumnNames.Contains("Extra Point") ? row["Extra Point"] : null; // result.FieldResults.FirstOrDefault(p => p.DbColumn == "ExtraPt");

                                compScore.Value = (aboutField == null ? 0 : aboutField.ToNullableInt32() ?? 0) +
                                    (withinField == null ? 0 : withinField.ToNullableInt32() ?? 0) +
                                    (beyondField == null ? 0 : beyondField.ToNullableInt32() ?? 0) +
                                    (extraPtField == null ? 0 : extraPtField.ToNullableInt32() ?? 0);

                                command.Parameters.Add(outParameter);
                                command.Parameters.Add(fpValueId);
                                command.Parameters.Add(accuracy);
                                command.Parameters.Add(compScore);
                                command.ExecuteNonQuery();
                                stringValue = ((System.Data.SqlClient.SqlParameter)(command.Parameters["@RETURN_VALUE"])).Value.ToString();
                            }
                            catch (Exception ex)
                            {
                                Log.Error("Error importing FP data and calculating Benchmark Level");
                            }
                        }
                    }
                    else if (field.CalculationFunction == "BenchmarkLevelV3")
                    {
                        // calculate benchmark value and return
                        using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
                        {
                            try
                            {
                                // FPValueId, Accuracy, CompScore (may need to calculate, since it might have changed)
                                //Database.AsSqlServer().Connection.DbConnection.Open();
                                command.CommandText = "dbo.ns4_udf_CalculateBenchmarkLevelV3";
                                command.CommandType = CommandType.StoredProcedure;
                                command.CommandTimeout = command.Connection.ConnectionTimeout;
                                var outParameter = command.CreateParameter();
                                outParameter.Direction = ParameterDirection.ReturnValue;
                                outParameter.ParameterName = "@RETURN_VALUE";
                                outParameter.DbType = DbType.String;
                                outParameter.Size = 50;

                                var fpValueId = command.CreateParameter();
                                fpValueId.Direction = ParameterDirection.Input;
                                fpValueId.DbType = DbType.Int32;
                                fpValueId.ParameterName = "@FPValueID";
                                var rowValue = row["Level"];
                                var fpField = row.ColumnNames.Contains("Level") ? _dbContext.LookupFields.FirstOrDefault(p => p.FieldName == "FPScale" && p.FieldValue == rowValue)?.FieldSpecificId : null;// result.FieldResults.FirstOrDefault(p => p.DbColumn == "FPValueID");
                                fpValueId.Value = fpField == null ? 0 : fpField.Value;

                                var accuracy = command.CreateParameter();
                                accuracy.Direction = ParameterDirection.Input;
                                accuracy.DbType = DbType.Int32;
                                accuracy.ParameterName = "@Accuracy";
                                var accuracyField = row.ColumnNames.Contains("Accuracy") ? row["Accuracy"] : null;//result.FieldResults.FirstOrDefault(p => p.DbColumn == "Accuracy");
                                accuracy.Value = accuracyField == null ? 0 : accuracyField.ToNullableDecimal() ?? 0;

                                var compScore = command.CreateParameter();
                                compScore.Direction = ParameterDirection.Input;
                                compScore.DbType = DbType.Int32;
                                compScore.ParameterName = "@CompScore";
                                int newCompScoreSum = 0;
                                var aboutField = row.ColumnNames.Contains("About") ? row["About"] : null; //resul.FieldResults.FirstOrDefault(p => p.DbColumn == "About");
                                var withinField = row.ColumnNames.Contains("Within") ? row["Within"] : null; //  result.FieldResults.FirstOrDefault(p => p.DbColumn == "Within");
                                var beyondField = row.ColumnNames.Contains("Beyond") ? row["Beyond"] : null; // result.FieldResults.FirstOrDefault(p => p.DbColumn == "Beyond");
                                var extraPtField = row.ColumnNames.Contains("Extra Point") ? row["Extra Point"] : null; // result.FieldResults.FirstOrDefault(p => p.DbColumn == "ExtraPt");

                                compScore.Value = (aboutField == null ? 0 : aboutField.ToNullableInt32() ?? 0) +
                                    (withinField == null ? 0 : withinField.ToNullableInt32() ?? 0) +
                                    (beyondField == null ? 0 : beyondField.ToNullableInt32() ?? 0) +
                                    (extraPtField == null ? 0 : extraPtField.ToNullableInt32() ?? 0);

                                command.Parameters.Add(outParameter);
                                command.Parameters.Add(fpValueId);
                                command.Parameters.Add(accuracy);
                                command.Parameters.Add(compScore);
                                command.ExecuteNonQuery();
                                stringValue = ((System.Data.SqlClient.SqlParameter)(command.Parameters["@RETURN_VALUE"])).Value.ToString();
                            }
                            catch (Exception ex)
                            {
                                Log.Error("Error importing FP data and calculating Benchmark Level");
                            }
                        }
                    }
                    return String.Format("'{0}'", stringValue);

            }

            return String.Empty;
        }

        public static string GetFieldInsertUpdateString(AssessmentField field, Row row, OutputDto_Log result, DistrictContext _dbContext)
        {
            switch (field.FieldType)
            {
                case "DropdownRange":
                    if (field.RangeLow != field.RangeHigh)
                    {
                        var ddrVal = row[field.DisplayLabel].ToNullableInt32();
                        if (ddrVal != null && (ddrVal < field.RangeLow || ddrVal > field.RangeHigh))
                        {
                            result.LogItems.Add(String.Format("The value '{0}' is outside of the range {2} - {3} for field '{1}' for Student {4}, {5}\r\n",
                                ddrVal,
                                field.DisplayLabel,
                                field.RangeLow,
                                field.RangeHigh,
                                row[NSConstants.BatchProcessing.StateTestStudentLastName],
                                row[NSConstants.BatchProcessing.StateTestStudentFirstName]));
                            return "null";
                        }
                    }
                    return row[field.DisplayLabel].ToNullableInt32()?.ToString() ?? "null";
                case "Textfield":
                case "Textarea":
                    return String.Format("'{0}'", row[field.DisplayLabel] == null ? String.Empty : row[field.DisplayLabel].Replace("'", "''"));
                case "Checkbox":
                    return String.Format("{0}", row[field.DisplayLabel] == "1" ? "1" : "0");
                case "DecimalRange":
                    if (field.RangeLow != field.RangeHigh)
                    {
                        var deciVal = row[field.DisplayLabel].ToNullableDecimal();
                        if (deciVal != null && (deciVal < field.RangeLow || deciVal > field.RangeHigh))
                        {
                            result.LogItems.Add(String.Format("The value '{0}' is outside of the range {2} - {3} for field '{1}' for Student {4}, {5}\r\n",
                                deciVal,
                                field.DisplayLabel,
                                field.RangeLow,
                                field.RangeHigh,
                                row[NSConstants.BatchProcessing.StateTestStudentLastName],
                                row[NSConstants.BatchProcessing.StateTestStudentFirstName]));
                            return "null";
                        }
                    }
                    return row[field.DisplayLabel].ToNullableDecimal()?.ToString() ?? "null";
                case "DropdownFromDB": // need to go lookup int value
                    var theValue = row[field.DisplayLabel];
                    var lookupValue = _dbContext.LookupFields.FirstOrDefault(p => p.FieldName == field.LookupFieldName && p.FieldValue == theValue);
                    if (lookupValue == null && theValue != null && theValue != "")
                    {
                        result.LogItems.Add(String.Format("The value '{0}' is not a valid value for field '{1}' for Student {2}, {3}\r\n",
                            theValue,
                            field.DisplayLabel,
                            row[NSConstants.BatchProcessing.StateTestStudentLastName],
                            row[NSConstants.BatchProcessing.StateTestStudentFirstName]));
                    }
                    return lookupValue?.FieldSpecificId.ToString() ?? "null";
                case "CalculatedFieldDbBacked":
                    return row[field.DisplayLabel].ToNullableInt32()?.ToString() ?? "null";
                case "CalculatedFieldDbBackedString":
                    return String.Format("'{0}'", row[field.DisplayLabel]?.Replace("'", "''") ?? "");
                case "CalculatedFieldDbOnly": // benchmark level... shouldn't really allow this, right
                    return String.Format("'{0}'", row[field.DisplayLabel]?.Replace("'", "''") ?? "");
                case "DateCheckbox":
                case "Date":
                    // if null, set null
                    return row[field.DisplayLabel].ToNullableDate()?.ToShortDateString() ?? "null";
                default: // TODO: ignoring checklist for now
                    return "";
            }
        }

        public static bool IsStateInsert(int studentId, string grade, string assessmentTable, DistrictContext _dbContext)
        {
            // first determine if there's already a result for the current student/class/date
            var resultExistSql = new StringBuilder();
            resultExistSql.AppendFormat("SELECT * FROM {0} WHERE StudentID = {1} and Grade = '{2}'", assessmentTable, studentId.ToString(), grade);

            var insertUpdateSql = new StringBuilder();
            bool isInsert = true;

            // replace this crap with just a check for the result ID > 0
            using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
            {
                try
                {
                    _dbContext.Database.Connection.Open();
                    command.CommandText = resultExistSql.ToString();
                    command.CommandTimeout = command.Connection.ConnectionTimeout;

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {

                        if (((System.Data.SqlClient.SqlDataReader)(reader)).HasRows)
                        {
                            isInsert = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    _dbContext.Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }

            return isInsert;
        }

        public static bool IsInsert(int studentId, int testDueDateId, string assessmentTable, DistrictContext _dbContext)
        {
            // first determine if there's already a result for the current student/class/date
            var resultExistSql = new StringBuilder();
            resultExistSql.AppendFormat("SELECT * FROM {0} WHERE StudentID = {1} and TestDueDateID = {2}", assessmentTable, studentId.ToString(), testDueDateId);

            var insertUpdateSql = new StringBuilder();
            bool isInsert = true;

            // replace this crap with just a check for the result ID > 0
            using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
            {
                try
                {
                    _dbContext.Database.Connection.Open();
                    command.CommandText = resultExistSql.ToString();
                    command.CommandTimeout = command.Connection.ConnectionTimeout;

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {

                        if (((System.Data.SqlClient.SqlDataReader)(reader)).HasRows)
                        {
                            isInsert = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Could not determine if an Insert was needed for student in the Job processor for student: " + studentId);
                    throw ex;
                }
                finally
                {
                    _dbContext.Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }

            return isInsert;
        }

        public static bool IsInterventionInsert(int studentId, int interventionGroupId, DateTime testDueDate, string assessmentTable, DistrictContext _dbContext)
        {
            // first determine if there's already a result for the current student/class/date
            var resultExistSql = new StringBuilder();
            resultExistSql.AppendFormat("SELECT * FROM {0} WHERE StudentID = {1} and InterventionGroupId = {2} and TestDueDate = '{3}'", assessmentTable, studentId.ToString(), interventionGroupId, testDueDate);

            var insertUpdateSql = new StringBuilder();
            bool isInsert = true;

            // replace this crap with just a check for the result ID > 0
            using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
            {
                try
                {
                    _dbContext.Database.Connection.Open();
                    command.CommandText = resultExistSql.ToString();
                    command.CommandTimeout = command.Connection.ConnectionTimeout;

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {

                        if (((System.Data.SqlClient.SqlDataReader)(reader)).HasRows)
                        {
                            isInsert = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Could not determine if an Insert was needed for student in the Job processor for student: " + studentId);
                    throw ex;
                }
                finally
                {
                    _dbContext.Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }

            return isInsert;
        }

        #endregion

    }
}
