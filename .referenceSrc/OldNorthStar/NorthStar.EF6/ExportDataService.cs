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
using NorthStar4.CrossPlatform.DTO.Reports.ObservationSummary;
using EntityDto.DTO.Assessment;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Data.SqlClient;
using NorthStar.EF6.Infrastructure;
using EntityDto.DTO.Admin.InterventionGroup;

namespace NorthStar.EF6
{
    public class ExportDataService : NSBaseDataService
    {
        public ExportDataService(ClaimsIdentity user, string loginConnectionString) : base(user, loginConnectionString)
        {

        }

    

        private string DataTableToCSVString(System.Data.DataTable table)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < table.Columns.Count; i++)
            {
                sb.Append(table.Columns[i]);
                if (i < table.Columns.Count - 1)
                {
                    sb.Append(",");
                }
            }
            sb.AppendLine();

            foreach (DataRow dr in table.Rows)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        string value = dr[i].ToString();
                        
                        if (value.Contains(','))
                        {
                            value = string.Format("\"{0}\"", value.Replace("\"", "\"\""));
                            sb.Append(value);
                        }
                        else
                        {
                            sb.Append(dr[i].ToString());
                        }
                    }
                    if (i < table.Columns.Count - 1)
                    {
                        sb.Append(",");
                    }
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        //public System.Data.DataTable AssessmentDataToDataTable(Assessment assessment, int sectionId, int benchmarkDateId)
        //{
        //    var results = _dbContext.GetAssessmentStudentResults(assessment, sectionId, benchmarkDateId, DateTime.Now);

        //    var fieldsToAdd = GetImportableColumns(assessment);
            
        //    System.Data.DataTable table = new System.Data.DataTable();
        //    table.Columns.Add("Student ID", typeof(string));
        //    table.Columns.Add("Student Last Name", typeof(string));
        //    table.Columns.Add("Student First Name", typeof(string));
        //    table.Columns.Add("Date Test Taken");
        //    ConvertFieldsToDataColumns(table.Columns, fieldsToAdd);

        //    // convert records to datarows
        //    foreach(var studentResult in results)
        //    {
        //        DataRow row = table.NewRow();
        //        row["Student ID"] = studentResult.StudentIdentifier;
        //        row["Student Last Name"] = studentResult.StudentName.Split(new string[] { "," }, StringSplitOptions.None)[0].Trim();
        //        row["Student First Name"] = studentResult.StudentName.Split(new string[] { "," }, StringSplitOptions.None)[1].Trim();
        //        row["Date Test Taken"] = studentResult.TestDate?.ToShortDateString();

        //        foreach (var fieldResult in studentResult.FieldResults)
        //        {
        //            var field = GetAssessmentFieldForDbColumn(fieldResult.DbColumn, fieldsToAdd);
        //            if(field != null)
        //            {
        //                row[field.DisplayLabel] = GetValueForFieldResultByType(fieldResult, field);
        //            }
        //        }

        //        table.Rows.Add(row);
        //    }

        //    return table;

        //}

       

        private object GetValueForFieldResultByType(AssessmentFieldResult fieldResult, AssessmentField field)
        {
            switch (field.FieldType)
            {
                case "Checkbox":
                    if (fieldResult.BoolValue == null) return DBNull.Value; else return fieldResult.BoolValue;
                case "Textfield":
                case "Textarea":
                case "CalculatedFieldDbBacked":
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
                        var lookupValue = _dbContext.LookupFields.FirstOrDefault(p => p.FieldName == field.LookupFieldName && p.FieldSpecificId == fieldResult.IntValue);
                        return lookupValue.FieldValue;
                    }
                case "checklist":
                    if (fieldResult.StringValue == null)
                        return DBNull.Value;
                    else
                    {
                        var arySplit = fieldResult.StringValue.Split(Char.Parse(","));

                        var toCombine = new List<string>();
                        foreach(var intString in arySplit)
                        {
                            toCombine.Add(_dbContext.LookupFields.FirstOrDefault(p => p.FieldName == field.LookupFieldName && p.FieldSpecificId == Int32.Parse(intString)).FieldValue);
                        }

                        return String.Join(",", toCombine);
                    }
                case "DropdownRange":
                    if (fieldResult.IntValue == null) return DBNull.Value; else return fieldResult.IntValue;
            }

            Log.Error("Unknown Field Type expored: {0}", field.FieldType);
            return DBNull.Value;
        }

      

        public void ConvertFieldsToDataColumns(DataColumnCollection columns, List<AssessmentField> fields)
        {
            foreach(var field in fields)
            {
                switch (field.FieldType)
                {
                    case "Checkbox":
                        columns.Add(field.DisplayLabel, typeof(bool));
                        break;
                    case "Textfield":
                    case "Textarea":
                    case "CalculatedFieldDbBacked":
                    case "CalculatedFieldDbBackedString":
                    case "CalculatedFieldDbOnly":
                    case "checklist":
                        columns.Add(field.DisplayLabel, typeof(string));
                        break;
                    case "DecimalRange":
                        columns.Add(field.DisplayLabel, typeof(decimal));
                        break;
                    case "Date":
                    case "DateCheckbox":
                        columns.Add(field.DisplayLabel, typeof(DateTime));
                        break;
                    case "DropdownFromDB":
                        columns.Add(field.DisplayLabel, typeof(string));
                        break;
                    case "DropdownRange":
                        columns.Add(field.DisplayLabel, typeof(int));
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
                p.FieldType == "checklist" ||
                p.FieldType == "DateCheckbox" ||
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


     

        #region TestLog Classes
        public class OutputDto_AssessmentDataExportLog : OutputDto_Base
        {
            public OutputDto_AssessmentDataExportLog()
            {
                HistoryItems = new List<JobAssessmentDataExportDto>();
            }

            public List<JobAssessmentDataExportDto> HistoryItems { get; set; }
        }

        public class OutputDto_AssessmentDataExportAllFieldsLog : OutputDto_Base
        {
            public OutputDto_AssessmentDataExportAllFieldsLog()
            {
                HistoryItems = new List<JobAssessmentDataExportAllFieldsDto>();
            }

            public List<JobAssessmentDataExportAllFieldsDto> HistoryItems { get; set; }
        }

        public class OutputDto_InterventionDataExportLog : OutputDto_Base
        {
            public OutputDto_InterventionDataExportLog()
            {
                HistoryItems = new List<JobInterventionDataExportDto>();
            }

            public List<JobInterventionDataExportDto> HistoryItems { get; set; }
        }


        public class OutputDto_BatchPrintExportLog : OutputDto_Base
        {
            public OutputDto_BatchPrintExportLog()
            {
                HistoryItems = new List<JobPrintBatchDto>();
            }

            public List<JobPrintBatchDto> HistoryItems { get; set; }
        }

        public class OutputDto_AttendanceDataExportLog : OutputDto_Base
        {
            public OutputDto_AttendanceDataExportLog()
            {
                HistoryItems = new List<JobAttendanceExportDto>();
            }

            public List<JobAttendanceExportDto> HistoryItems { get; set; }
        }
        public class OutputDto_StaffExportLog : OutputDto_Base
        {
            public OutputDto_StaffExportLog()
            {
                HistoryItems = new List<JobStaffExportDto>();
            }

            public List<JobStaffExportDto> HistoryItems { get; set; }
        }
        public class OutputDto_StudentExportLog : OutputDto_Base
        {
            public OutputDto_StudentExportLog()
            {
                HistoryItems = new List<JobStudentExportDto>();
            }

            public List<JobStudentExportDto> HistoryItems { get; set; }
        }
        #endregion

        #region Load User's Import History
        public OutputDto_AssessmentDataExportLog LoadAssessmentDataExportHistory()
        {
            var result = new OutputDto_AssessmentDataExportLog();

            var items = _loginContext.JobAssessmentDataExports.Where(p => p.StaffId == _currentUser.Id);

            result.HistoryItems = Mapper.Map<List<JobAssessmentDataExportDto>>(items.OrderByDescending(p => p.StartDate).ToList());

            result.HistoryItems.Each(p =>
            {
                var requestDetails = JsonConvert.DeserializeObject<InputDto_DataExportRequest>(p.SerializedRequest);
                var assessmentString = string.Join(", ", requestDetails.Assessments.Select(s => s.AssessmentName));

                p.Assessments = assessmentString;
                p.SchoolYearVerbose = _dbContext.SchoolYears.FirstOrDefault(g => g.SchoolStartYear == p.SchoolStartYear)?.YearVerbose;
                p.BenchmarkDate = _dbContext.TestDueDates.FirstOrDefault(g => g.Id == p.BenchmarkDateId)?.DueDate;
            });
            return result;
        }

        public OutputDto_AssessmentDataExportAllFieldsLog LoadAssessmentDataExportHistoryAllFields()
        {
            var result = new OutputDto_AssessmentDataExportAllFieldsLog();

            var items = _loginContext.JobAllFieldsAssessmentDataExports.Where(p => p.StaffId == _currentUser.Id);

            result.HistoryItems = Mapper.Map<List<JobAssessmentDataExportAllFieldsDto>>(items.OrderByDescending(p => p.StartDate).ToList());

            result.HistoryItems.Each(p =>
            {
                var requestDetails = JsonConvert.DeserializeObject<InputDto_DataExportRequest>(p.SerializedRequest);
                var assessmentString = string.Join(", ", requestDetails.Assessments.Select(s => s.AssessmentName));

                p.Assessments = assessmentString;
                p.SchoolYearVerbose = _dbContext.SchoolYears.FirstOrDefault(g => g.SchoolStartYear == p.SchoolStartYear)?.YearVerbose;
                p.BenchmarkDate = _dbContext.TestDueDates.FirstOrDefault(g => g.Id == p.BenchmarkDateId)?.DueDate;
            });
            return result;
        }

        public OutputDto_InterventionDataExportLog LoadInterventionDataExportHistory()
        {
            var result = new OutputDto_InterventionDataExportLog();

            var items = _loginContext.JobInterventionDataExports.Where(p => p.StaffId == _currentUser.Id);

            result.HistoryItems = Mapper.Map<List<JobInterventionDataExportDto>>(items.OrderByDescending(p => p.StartDate).ToList());

            result.HistoryItems.Each(p =>
            {
                var requestDetails = JsonConvert.DeserializeObject<InputDto_InterventionExportRequest>(p.SerializedRequest);
                p.BatchName = p.BatchName;
                p.Assessments = p.AssessmentName;
                p.SchoolYearVerbose = _dbContext.SchoolYears.FirstOrDefault(g => g.SchoolStartYear == p.SchoolStartYear)?.YearVerbose;
                //p.BenchmarkDate = _dbContext.TestDueDates.FirstOrDefault(g => g.Id == p.BenchmarkDateId)?.DueDate;
            });
            return result;
        }

        public OutputDto_BatchPrintExportLog LoadBatchPrintHistory()
        {
            var result = new OutputDto_BatchPrintExportLog();

            var items = _loginContext.JobPrintBatches.Where(p => p.StaffId == _currentUser.Id);

            result.HistoryItems = Mapper.Map<List<JobPrintBatchDto>>(items.OrderByDescending(p => p.StartDate).ToList());

            result.HistoryItems.Each(p =>
            {
                var requestDetails = JsonConvert.DeserializeObject<InputDto_BatchPrintRequest>(p.SerializedRequest);
                var assessmentString = string.Join(", ", requestDetails.Assessments.Select(s => s.AssessmentName));

                p.Assessments = assessmentString;
                p.SchoolYearVerbose = _dbContext.SchoolYears.FirstOrDefault(g => g.SchoolStartYear == p.SchoolStartYear)?.YearVerbose;
                p.BenchmarkDate = _dbContext.TestDueDates.FirstOrDefault(g => g.Id == p.BenchmarkDateId)?.DueDate;
                p.HfwPages = requestDetails.ReportOptions.HfwPages == null ? null : string.Join(", ", requestDetails.ReportOptions.HfwPages.Select(s => s.text));
                p.PageTypes = requestDetails.ReportOptions.PageTypes == null ? null : string.Join(", ", requestDetails.ReportOptions.PageTypes.Select(s => s.text));
                p.TextLevelZones = requestDetails.ReportOptions.TargetLevelZones == null ? null : string.Join(", ", requestDetails.ReportOptions.TargetLevelZones.Select(s => s.text));
            });
            return result;
        }


        public OutputDto_AttendanceDataExportLog LoadAttendanceDataExportHistory()
        {
            var result = new OutputDto_AttendanceDataExportLog();

            var items = _loginContext.JobAttendanceExports.Where(p => p.StaffId == _currentUser.Id);

            result.HistoryItems = Mapper.Map<List<JobAttendanceExportDto>>(items.OrderByDescending(p => p.StartDate).ToList());

            result.HistoryItems.Each(p =>
            {
                p.SchoolYearVerbose = _dbContext.SchoolYears.FirstOrDefault(g => g.SchoolStartYear == p.SchoolStartYear)?.YearVerbose;
                p.DownloadUrl = GetFileSharedAccessSigUrl(p.UploadedFileName);
            });
            return result;
        }
        public OutputDto_StaffExportLog LoadStaffExportHistory()
        {
            var result = new OutputDto_StaffExportLog();

            var items = _loginContext.JobStaffExports.Where(p => p.StaffId == _currentUser.Id);

            result.HistoryItems = Mapper.Map<List<JobStaffExportDto>>(items.OrderByDescending(p => p.StartDate).ToList());

            result.HistoryItems.Each(p =>
            {
                p.DownloadUrl = GetFileSharedAccessSigUrl(p.UploadedFileName);
            });
            return result;
        }
        public OutputDto_StudentExportLog LoadStudentExportHistory()
        {
            var result = new OutputDto_StudentExportLog();

            var items = _loginContext.JobStudentExports.Where(p => p.StaffId == _currentUser.Id);

            result.HistoryItems = Mapper.Map<List<JobStudentExportDto>>(items.OrderByDescending(p => p.StartDate).ToList());

            result.HistoryItems.Each(p =>
            {
                p.DownloadUrl = GetFileSharedAccessSigUrl(p.UploadedFileName);
            });
            return result;
        }
        #endregion

        public string GetFileSharedAccessSigUrl(string fileName)
        {
            if(String.IsNullOrEmpty(fileName))
            {
                return String.Empty;
            }

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);

            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(NSConstants.Azure.AssessmentDataExportContainer);
            var blob = container.GetBlockBlobReference(fileName);

            if (!blob.Exists())
            {
                return String.Empty;
            }
            var builder = new UriBuilder(blob.Uri);
            builder.Query = blob.GetSharedAccessSignature(
                new SharedAccessBlobPolicy
                {
                    Permissions = SharedAccessBlobPermissions.Read,
                    //SharedAccessStartTime = new DateTimeOffset(DateTime.UtcNow.AddMinutes(-5)),
                    SharedAccessExpiryTime = new DateTimeOffset(DateTime.UtcNow.AddMinutes(20))
                }
                ).TrimStart('?');

            return builder.Uri.ToString();
        }


        #region Delete History
        public OutputDto_Base DeleteHistoryItem(InputDto_SimpleId input)
        {
            var result = new OutputDto_Base();
            var historyItem = _loginContext.JobAssessmentDataExports.First(p => p.Id == input.Id);

            if(historyItem.StaffId != _currentUser.Id)
            {
                result.Status.StatusCode = StatusCode.AccessDenied;
                result.Status.StatusMessage = "You do not have access to delete this item.";
                return result;
            }

            // delete file from Azure
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);
            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(NSConstants.Azure.AssessmentDataExportContainer);

            if (!String.IsNullOrEmpty(historyItem.UploadedFileName))
            {
                var blob = container.GetBlockBlobReference(historyItem.UploadedFileName);
                blob.DeleteIfExists();
            }

            _loginContext.JobAssessmentDataExports.Remove(historyItem);
            _loginContext.SaveChanges();

            return result;
        }

        public OutputDto_Base DeleteAllFieldsHistoryItem(InputDto_SimpleId input)
        {
            var result = new OutputDto_Base();
            var historyItem = _loginContext.JobAllFieldsAssessmentDataExports.First(p => p.Id == input.Id);

            if (historyItem.StaffId != _currentUser.Id)
            {
                result.Status.StatusCode = StatusCode.AccessDenied;
                result.Status.StatusMessage = "You do not have access to delete this item.";
                return result;
            }

            // delete file from Azure
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);
            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(NSConstants.Azure.AssessmentDataExportContainer);

            if (!String.IsNullOrEmpty(historyItem.UploadedFileName))
            {
                var blob = container.GetBlockBlobReference(historyItem.UploadedFileName);
                blob.DeleteIfExists();
            }

            _loginContext.JobAllFieldsAssessmentDataExports.Remove(historyItem);
            _loginContext.SaveChanges();

            return result;
        }

        public OutputDto_Base DeleteInterventionDataHistoryItem(InputDto_SimpleId input)
        {
            var result = new OutputDto_Base();
            var historyItem = _loginContext.JobInterventionDataExports.First(p => p.Id == input.Id);

            if (historyItem.StaffId != _currentUser.Id)
            {
                result.Status.StatusCode = StatusCode.AccessDenied;
                result.Status.StatusMessage = "You do not have access to delete this item.";
                return result;
            }

            // delete file from Azure
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);
            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(NSConstants.Azure.AssessmentDataExportContainer);

            if (!String.IsNullOrEmpty(historyItem.UploadedFileName))
            {
                var blob = container.GetBlockBlobReference(historyItem.UploadedFileName);
                blob.DeleteIfExists();
            }

            _loginContext.JobInterventionDataExports.Remove(historyItem);
            _loginContext.SaveChanges();

            return result;
        }

        public OutputDto_Base DeleteAttendanceHistoryItem(InputDto_SimpleId input)
        {
            var result = new OutputDto_Base();
            var historyItem = _loginContext.JobAttendanceExports.First(p => p.Id == input.Id);

            if (historyItem.StaffId != _currentUser.Id)
            {
                result.Status.StatusCode = StatusCode.AccessDenied;
                result.Status.StatusMessage = "You do not have access to delete this item.";
                return result;
            }

            // delete file from Azure
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);
            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(NSConstants.Azure.AssessmentDataExportContainer);
            
            if(historyItem.UploadedFileName != null)
            {
                var blob = container.GetBlockBlobReference(historyItem.UploadedFileName);
                blob.DeleteIfExists();
            }

            _loginContext.JobAttendanceExports.Remove(historyItem);
            _loginContext.SaveChanges();

            return result;
        }
        public OutputDto_Base DeleteStudentHistoryItem(InputDto_SimpleId input)
        {
            var result = new OutputDto_Base();
            var historyItem = _loginContext.JobStudentExports.First(p => p.Id == input.Id);

            if (historyItem.StaffId != _currentUser.Id)
            {
                result.Status.StatusCode = StatusCode.AccessDenied;
                result.Status.StatusMessage = "You do not have access to delete this item.";
                return result;
            }

            // delete file from Azure
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);
            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(NSConstants.Azure.AssessmentDataExportContainer);

            if (historyItem.UploadedFileName != null)
            {
                var blob = container.GetBlockBlobReference(historyItem.UploadedFileName);
                blob.DeleteIfExists();
            }

            _loginContext.JobStudentExports.Remove(historyItem);
            _loginContext.SaveChanges();

            return result;
        }
        public OutputDto_Base DeleteStaffHistoryItem(InputDto_SimpleId input)
        {
            var result = new OutputDto_Base();
            var historyItem = _loginContext.JobStaffExports.First(p => p.Id == input.Id);

            if (historyItem.StaffId != _currentUser.Id)
            {
                result.Status.StatusCode = StatusCode.AccessDenied;
                result.Status.StatusMessage = "You do not have access to delete this item.";
                return result;
            }

            // delete file from Azure
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);
            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(NSConstants.Azure.AssessmentDataExportContainer);

            if (historyItem.UploadedFileName != null)
            {
                var blob = container.GetBlockBlobReference(historyItem.UploadedFileName);
                blob.DeleteIfExists();
            }

            _loginContext.JobStaffExports.Remove(historyItem);
            _loginContext.SaveChanges();

            return result;
        }

        public OutputDto_Base DeleteBatchPrintHistoryItem(InputDto_SimpleId input)
        {
            var result = new OutputDto_Base();
            var historyItem = _loginContext.JobPrintBatches.First(p => p.Id == input.Id);

            if (historyItem.StaffId != _currentUser.Id)
            {
                result.Status.StatusCode = StatusCode.AccessDenied;
                result.Status.StatusMessage = "You do not have access to delete this item.";
                return result;
            }

            // delete file from Azure
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);
            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(NSConstants.Azure.AssessmentDataExportContainer);

            if (historyItem.UploadedFileName != null)
            {
                var blob = container.GetBlockBlobReference(historyItem.UploadedFileName);
                blob.DeleteIfExists();
            }

            _loginContext.JobPrintBatches.Remove(historyItem);
            _loginContext.SaveChanges();

            return result;
        }
        #endregion





        #region Get Template
        //public AssessmentTemplate GetStateTestDataImportTemplate(InputDto_SimpleId input)
        //{
        //    var result = new AssessmentTemplate();

        //    var assessment = _dbContext.Assessments
        //        .Include(p => p.Fields)
        //        .First(p => p.Id == input.Id);

        //    // for state tests, we need to the following fields
        //    var studentId = new AssessmentFieldTemplate
        //    {
        //        FieldName = "Student ID",
        //        FieldType = "Label",
        //        Required = true,
        //        SortOrder = 1,
        //        ValidValues = "North Star Student Identifier",
        //        UniqueColumnName = "Student ID"
        //    };
        //    var lastName = new AssessmentFieldTemplate
        //    {
        //        FieldName = "Student Last Name",
        //        FieldType = "Label",
        //        Required = true,
        //        SortOrder = 2,
        //        ValidValues = "Text less than 255 characters.",
        //        UniqueColumnName = "Student Last Name"
        //    };
        //    var firstName = new AssessmentFieldTemplate
        //    {
        //        FieldName = "Student First Name",
        //        FieldType = "Label",
        //        Required = true,
        //        SortOrder = 3,
        //        ValidValues = "Text less than 255 characters.",
        //        UniqueColumnName = "Student First Name"
        //    };
        //    var grade = new AssessmentFieldTemplate
        //    {
        //        FieldName = "Grade",
        //        FieldType = "Label",
        //        Required = true,
        //        SortOrder = 4,
        //        ValidValues = "Any grade value in the range specified.",
        //        ValidRange = "P, K, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12",
        //        UniqueColumnName = "Grade"
        //    };
        //    var testDate = new AssessmentFieldTemplate
        //    {
        //        FieldName = "TestDate",
        //        FieldType = "Label",
        //        Required = true,
        //        SortOrder = 5,
        //        ValidValues = "A date in the format MM/DD/YYYY (month and day do not need leading zeroes)",
        //        ValidRange = "Any date.",
        //        UniqueColumnName = "Date Test Taken"
        //    };

        //    var fields = assessment.Fields.Where(p => (p.FieldType == "DropdownRange" ||
        //        p.FieldType == "Textfield" ||
        //        p.FieldType == "Textarea" ||
        //        p.FieldType == "Checkbox" ||
        //        p.FieldType == "DecimalRange" ||
        //        p.FieldType == "DropdownFromDB" ||
        //        p.FieldType == "DateCheckbox" ||
        //        p.FieldType == "Date") &&
        //        !String.IsNullOrWhiteSpace(p.DatabaseColumn)).ToList();

        //    result.Fields.Add(studentId);
        //    result.Fields.Add(lastName);
        //    result.Fields.Add(firstName);
        //    result.Fields.Add(grade);
        //    result.Fields.Add(testDate);

        //    foreach(var field in fields)
        //    {
        //        result.Fields.Add(ConvertFieldToTemplate(field));
        //    }

        //    return result;
        //}

        #endregion

        public bool CanAccessExportFile(string fileName)
        {
             return _loginContext.JobAssessmentDataExports.Any(p => p.UploadedFileName == fileName && p.StaffId == _currentUser.Id);
        }

        public bool CanAccessAllFieldsExportFile(string fileName)
        {
            return _loginContext.JobAllFieldsAssessmentDataExports.Any(p => p.UploadedFileName == fileName && p.StaffId == _currentUser.Id);
        }

        public bool CanAccessPrintBatchFile(string fileName)
        {
            return _loginContext.JobPrintBatches.Any(p => p.UploadedFileName == fileName && p.StaffId == _currentUser.Id);
        }


        public bool CanAccessAttendanceExportFile(string fileName)
        {
            return _loginContext.JobAttendanceExports.Any(p => p.UploadedFileName == fileName && p.StaffId == _currentUser.Id);
        }
        public bool CanAccessStudentExportFile(string fileName)
        {
            return _loginContext.JobStudentExports.Any(p => p.UploadedFileName == fileName && p.StaffId == _currentUser.Id);
        }

        public bool CanAccessStaffExportFile(string fileName)
        {
            return _loginContext.JobStaffExports.Any(p => p.UploadedFileName == fileName && p.StaffId == _currentUser.Id);
        }

        public bool CanAccessInterventionDataExportFile(string fileName)
        {
            return _loginContext.JobInterventionDataExports.Any(p => p.UploadedFileName == fileName && p.StaffId == _currentUser.Id);
        }

        #region Add Job to Queue
        public OutputDto_Base CreateAssessmentDataExportJob(InputDto_GetFilteredObservationSummaryOptions input)
        {
            var request = new InputDto_DataExportRequest();

            List<Assessment> assessmentsToInclude = _dbContext.GetObservationSummaryVisibleAssessments(_currentUser.Id);

            assessmentsToInclude.Each(p =>
            {
                p.Fields = _dbContext.GetViewableFields(p, "observationsummary", _currentUser.Id);
            });

            List<AssessmentDto> serializedAssessments = Mapper.Map<List<AssessmentDto>>(assessmentsToInclude);

            request.Assessments = serializedAssessments;
            request.ReportOptions = input;
            
            var newJob = _loginContext.JobAssessmentDataExports.Create();
            newJob.StaffId = _currentUser.Id;
            newJob.StaffEmail = _currentUser.Email;
            newJob.StartDate = DateTime.Now;
            newJob.Status = "Awaiting processing";
            newJob.SchoolStartYear = input.SchoolStartYear;
            newJob.BenchmarkDateId = input.TestDueDateID.Value;
            newJob.SerializedRequest = JsonConvert.SerializeObject(request);
            _loginContext.JobAssessmentDataExports.Add(newJob);
            _loginContext.SaveChanges();

            Utility.AddNewImportQueueMessage(newJob.Id, NSConstants.Azure.JobType.DataExport);

            return new OutputDto_Base();
        }

        public OutputDto_Base CreateAssessmentAllFieldsDataExportJob(InputDto_GetFilteredObservationSummaryOptions input)
        {
            var request = new InputDto_DataExportRequest();

            // MAP Id and text to assessmentdto
            List<AssessmentDto> serializedAssessments = Mapper.Map<List<AssessmentDto>>(input.Assessments);

            request.Assessments = serializedAssessments;
            request.ReportOptions = input;

            var newJob = _loginContext.JobAllFieldsAssessmentDataExports.Create();
            newJob.StaffId = _currentUser.Id;
            newJob.StaffEmail = _currentUser.Email;
            newJob.StartDate = DateTime.Now;
            newJob.Status = "Awaiting processing";
            newJob.SchoolStartYear = input.SchoolStartYear;
            newJob.BenchmarkDateId = input.TestDueDateID.Value;
            newJob.SerializedRequest = JsonConvert.SerializeObject(request);
            _loginContext.JobAllFieldsAssessmentDataExports.Add(newJob);
            _loginContext.SaveChanges();

            Utility.AddNewImportQueueMessage(newJob.Id, NSConstants.Azure.JobType.AllFieldsBenchmarkExport);

            return new OutputDto_Base();
        }



        //CreateInterventionGroupAssessmentDataExportJob
        public OutputDto_Base CreateInterventionGroupAssessmentDataExportJob(InputDto_GetFilteredObservationSummaryOptions input)
        {
            var request = new InputDto_DataExportRequest();

            // don't need to do this... just go with the assessment passed in.  LLI or RR
            //List<Assessment> assessmentsToInclude = _dbContext.GetObservationSummaryVisibleAssessments(_currentUser.Id);

            //assessmentsToInclude.Each(p =>
            //{
            //    p.Fields = _dbContext.GetViewableFields(p, "observationsummary", _currentUser.Id);
            //});

            //List<AssessmentDto> serializedAssessments = Mapper.Map<List<AssessmentDto>>(assessmentsToInclude);
            input.InterventionAssessment = input.InterventionAssessment;
            //request.Assessments = serializedAssessments;
            request.ReportOptions = input;  

            var newJob = _loginContext.JobInterventionDataExports.Create();
            newJob.StaffId = _currentUser.Id;
            newJob.StaffEmail = _currentUser.Email;
            newJob.StartDate = DateTime.Now;
            newJob.Status = "Awaiting processing";
            newJob.SchoolStartYear = input.SchoolStartYear;
            newJob.AssessmentName = input.InterventionAssessment.text;
            newJob.BatchName = input.BatchName;
            //newJob.BenchmarkDateId = input.TestDueDateID.Value;
            newJob.SerializedRequest = JsonConvert.SerializeObject(request);
            _loginContext.JobInterventionDataExports.Add(newJob);
            _loginContext.SaveChanges();

            Utility.AddNewImportQueueMessage(newJob.Id, NSConstants.Azure.JobType.InterventionDataExport);

            return new OutputDto_Base();
        }

        public OutputDto_Base CreatePrintBatchJob(InputDto_GetFilteredPrintBatchOptions input)
        {
            var request = new InputDto_BatchPrintRequest();

            var assessmentIds = input.Assessments.Select(p => p.id).ToList();
            List<Assessment> assessmentsToInclude = _dbContext.Assessments.Where(p => assessmentIds.Any(j => j == p.Id)).ToList();

            assessmentsToInclude.Each(p =>
            {
                p.Fields = _dbContext.GetViewableFields(p, "observationsummary", _currentUser.Id);
            });

            List<AssessmentDto> serializedAssessments = Mapper.Map<List<AssessmentDto>>(assessmentsToInclude);

            request.Assessments = serializedAssessments;
            request.ReportOptions = input;


            var newJob = _loginContext.JobPrintBatches.Create();
            newJob.StaffId = _currentUser.Id;
            newJob.StaffEmail = _currentUser.Email;
            newJob.StartDate = DateTime.Now;
            newJob.Status = "Awaiting processing";
            newJob.SchoolStartYear = input.SchoolStartYear;
            newJob.BenchmarkDateId = input.TestDueDateID.Value;
            newJob.SerializedRequest = JsonConvert.SerializeObject(request);
            newJob.BatchName = input.BatchName;
            _loginContext.JobPrintBatches.Add(newJob);
            _loginContext.SaveChanges();

            Utility.AddNewImportQueueMessage(newJob.Id, NSConstants.Azure.JobType.PrintBatch);

            return new OutputDto_Base();
        }

        
        public OutputDto_Base CreateAttendanceDataExportJob(InputDto_SimpleId input)
        {
            var request = new InputDto_AttendanceExportRequest();

            //var results = _dbContext.Database.SqlQuery<AttendanceExportInfo>("EXEC [ns4_GetAttendanceExportInfo] @InterventionGroupId",
            //    new SqlParameter("InterventionGroupId", 20134)).ToList();

            //var uniqueGroups = results.Select(p => p.InterventionGroupId).Distinct();

            //// speed... get all current records for all groups we care about
            //var existingAttendanceInfoForGroups = _dbContext.InterventionAttendances.Include(p => p.AttendanceReason).Where(p => uniqueGroups.Contains(p.SectionID)).ToList();


            //var aCalc = new AttendanceCalculator(_dbContext);
            //// now get attendance data for every day for each unique group
            //// get meeting days, etc
            //var detailedAttendance = aCalc.GetAttendanceDetail(results, existingAttendanceInfoForGroups);

            // convert results to DataTable and return
            var newJob = _loginContext.JobAttendanceExports.Create();
            newJob.StaffId = _currentUser.Id;
            newJob.StaffEmail = _currentUser.Email;
            newJob.StartDate = DateTime.Now;
            newJob.Status = "Awaiting processing";
            newJob.SchoolStartYear = input.Id;
            _loginContext.JobAttendanceExports.Add(newJob);
            _loginContext.SaveChanges();

            Utility.AddNewImportQueueMessage(newJob.Id, NSConstants.Azure.JobType.AttendanceExport);

            return new OutputDto_Base();
        }
        public OutputDto_Base CreateStaffExportJob(InputDto_SimpleString input)
        {

            // convert results to DataTable and return
            var newJob = _loginContext.JobStaffExports.Create();
            newJob.StaffId = _currentUser.Id;
            newJob.StaffEmail = _currentUser.Email;
            newJob.StartDate = DateTime.Now;
            newJob.Status = "Awaiting processing";
            newJob.BatchName = input.value;
            _loginContext.JobStaffExports.Add(newJob);
            _loginContext.SaveChanges();

            Utility.AddNewImportQueueMessage(newJob.Id, NSConstants.Azure.JobType.TeacherAttributeExport);

            return new OutputDto_Base();
        }
        public OutputDto_Base CreateStudentExportJob(InputDto_SimpleString input)
        {
            var request = new InputDto_AttendanceExportRequest();

            // convert results to DataTable and return
            var newJob = _loginContext.JobStudentExports.Create();
            newJob.StaffId = _currentUser.Id;
            newJob.StaffEmail = _currentUser.Email;
            newJob.StartDate = DateTime.Now;
            newJob.Status = "Awaiting processing";
            newJob.BatchName = input.value;
            _loginContext.JobStudentExports.Add(newJob);
            _loginContext.SaveChanges();

            Utility.AddNewImportQueueMessage(newJob.Id, NSConstants.Azure.JobType.StudentAttributeExport);

            return new OutputDto_Base();
        }
        #endregion

        #region DB Helper Functions

        public static string GetDbColumnForColumn(string columnDisplayName, Assessment assessment)
        {
            return assessment.Fields.FirstOrDefault(p => p.DisplayLabel.Equals(columnDisplayName, StringComparison.OrdinalIgnoreCase))?.DatabaseColumn;
        }


       
        #endregion

    }
}
