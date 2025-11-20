using AutoMapper;
using DataAccess;
using EntityDto.DTO.Admin.Simple;
using EntityDto.DTO.ImportExport;
using EntityDto.LoginDB.Entity;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Northstar.Core;
using NorthStar.Core;
using NorthStar.Core.FileUpload;
using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NorthStar.EF6
{
    public class RosterRolloverDataService : NSBaseDataService
    {
        public RosterRolloverDataService(ClaimsIdentity user, string loginConnectionString) : base(user, loginConnectionString)
        {

        }

        #region Template Definitions
        public OutputDto_SimpleString GetFullRolloverTemplateCSV()
        {
            var result = new OutputDto_SimpleString();

            var inputTemplate = GetFullRolloverTemplate(_dbContext);

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

        public OutputDto_SimpleString GetStudentRolloverTemplateCSV()
        {
            var result = new OutputDto_SimpleString();

            var inputTemplate = GetStudentRolloverTemplate(_dbContext);

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

        public OutputDto_SimpleString GetTeacherRolloverTemplateCSV()
        {
            var result = new OutputDto_SimpleString();

            var inputTemplate = GetTeacherRolloverTemplate(_dbContext);

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

        public AssessmentTemplate GetFullRolloverTemplateWithContext()
        {
            return GetFullRolloverTemplate(_dbContext);
        }

        public AssessmentTemplate GetStudentRolloverTemplateWithContext()
        {
            return GetStudentRolloverTemplate(_dbContext);
        }

        public AssessmentTemplate GetTeacherRolloverTemplateWithContext()
        {
            return GetTeacherRolloverTemplate(_dbContext);
        }

        public static AssessmentTemplate GetStudentRolloverTemplate(DistrictContext dbContext)
        {
            var result = new AssessmentTemplate();


            // for state tests, we need to the following fields
            var studentId = new AssessmentFieldTemplate
            {
                FieldName = "Student ID",
                FieldType = "Text",
                Required = true,
                SortOrder = 1,
                ValidValues = "North Star Student Identifier",
                UniqueColumnName = "Student ID"
            };
            result.Fields.Add(studentId);

            var studentFirstName = new AssessmentFieldTemplate
            {
                FieldName = "Student First Name",
                FieldType = "Text",
                Required = true,
                SortOrder = 2,
                ValidValues = "Text less than 255 characters.",
                UniqueColumnName = "Student First Name"
            };
            result.Fields.Add(studentFirstName);

            var studentMiddleName = new AssessmentFieldTemplate
            {
                FieldName = "Student Middle Name",
                FieldType = "Text",
                Required = false,
                SortOrder = 3,
                ValidValues = "Text less than 255 characters.",
                UniqueColumnName = "Student Middle Name"
            };
            result.Fields.Add(studentMiddleName);

            var studentLastName = new AssessmentFieldTemplate
            {
                FieldName = "Student Last Name",
                FieldType = "Text",
                Required = true,
                SortOrder = 4,
                ValidValues = "Text less than 255 characters.",
                UniqueColumnName = "Student Last Name"
            };
            result.Fields.Add(studentLastName);

            //var grade = new AssessmentFieldTemplate
            //{
            //    FieldName = "Student Grade",
            //    FieldType = "Grade",
            //    Required = true,
            //    SortOrder = 5,
            //    ValidValues = "Any integer value in the range specified, P (pre-K) or K (kindergarten).",
            //    ValidRange = "P, K, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12",
            //    UniqueColumnName = "Student Grade"
            //};
            //result.Fields.Add(grade);

            var dob = new AssessmentFieldTemplate
            {
                FieldName = "Student DOB",
                FieldType = "Date",
                Required = true,
                SortOrder = 6,
                ValidValues = "A date in the format MM/DD/YYYY (month and day do not need leading zeroes)",
                ValidRange = "Any date.",
                UniqueColumnName = "Student DOB"
            };
            result.Fields.Add(dob);

            var gradYear = new AssessmentFieldTemplate
            {
                FieldName = "Student Graduation Year",
                FieldType = "Integer",
                Required = false,
                SortOrder = 7,
                ValidValues = "Any integer value in the range specified.",
                ValidRange = "1900 - 2050",
                UniqueColumnName = "Student Graduation Year"
            };
            result.Fields.Add(gradYear);



            var currentSortOrder = 8;
            // add studentAttributes
            var studentAttributes = dbContext.StudentAttributeTypes.OrderBy(p => p.AttributeName).ToList();
            studentAttributes.Each(p =>
            {
                // get values for this field
                var values = string.Join(",", dbContext.StudentAttributeLookupValues.Where(g => g.AttributeID == p.Id).OrderBy(g => g.LookupValue).Select(r => r.LookupValue).ToList());

                result.Fields.Add(new AssessmentFieldTemplate
                {
                    FieldName = p.AttributeName,
                    FieldType = "StudentAttribute",
                    Required = false,
                    SortOrder = currentSortOrder,
                    ValidValues = "Any value from the list specified or blank. (For Student Services, a comma-separated list is allowed.  Only a single value for other types)",
                    ValidRange = values,
                    UniqueColumnName = p.AttributeName
                });
                currentSortOrder++;
            });

            var studentActive = new AssessmentFieldTemplate
            {
                FieldName = "Student Is Active",
                FieldType = "Text",
                Required = false,
                SortOrder = 25,
                ValidValues = "Y, N or blank",
                ValidRange = "Y/N or leave blank",
                UniqueColumnName = "Student Is Active"
            };
            result.Fields.Add(studentActive);

            return result;
        }


        public static AssessmentTemplate GetTeacherRolloverTemplate(DistrictContext dbContext)
        {
            var result = new AssessmentTemplate();

            var currentSortOrder = 8;
        
            // teacher fields
            var teacherId = new AssessmentFieldTemplate
            {
                FieldName = "Teacher ID",
                FieldType = "Text",
                Required = true,
                SortOrder = currentSortOrder++,
                ValidValues = "North Star Teacher Identifier",
                UniqueColumnName = "Teacher ID"
            };
            result.Fields.Add(teacherId);

            var teacherFirstName = new AssessmentFieldTemplate
            {
                FieldName = "Teacher First Name",
                FieldType = "Text",
                Required = true,
                SortOrder = currentSortOrder++,
                ValidValues = "Text less than 255 characters.",
                UniqueColumnName = "Teacher First Name"
            };
            result.Fields.Add(teacherFirstName);

            var teacherMiddleName = new AssessmentFieldTemplate
            {
                FieldName = "Teacher Middle Name",
                FieldType = "Text",
                Required = false,
                SortOrder = currentSortOrder++,
                ValidValues = "Text less than 255 characters.",
                UniqueColumnName = "Teacher Middle Name"
            };
            result.Fields.Add(teacherMiddleName);

            var teacherLastName = new AssessmentFieldTemplate
            {
                FieldName = "Teacher Last Name",
                FieldType = "Text",
                Required = true,
                SortOrder = currentSortOrder++,
                ValidValues = "Text less than 255 characters.",
                UniqueColumnName = "Teacher Last Name"
            };
            result.Fields.Add(teacherLastName);

            var teacherEmail = new AssessmentFieldTemplate
            {
                FieldName = "Teacher Email",
                FieldType = "Text",
                Required = true,
                SortOrder = currentSortOrder++,
                ValidValues = "Any valid email address.",
                ValidRange = "An email address",
                UniqueColumnName = "Teacher Email"
            };
            result.Fields.Add(teacherEmail);

            var isInterventionist = new AssessmentFieldTemplate
            {
                FieldName = "Is Interventionist",
                FieldType = "Bool",
                Required = false,
                SortOrder = currentSortOrder++,
                ValidValues = "Y, N or blank",
                ValidRange = "Y/N or leave blank",
                UniqueColumnName = "Is Interventionist"
            };
            result.Fields.Add(isInterventionist);

            // eventually add permissions/role here

            var isDA = new AssessmentFieldTemplate
            {
                FieldName = "Is District Administrator",
                FieldType = "Bool",
                Required = false,
                SortOrder = currentSortOrder++,
                ValidValues = "Y, N or blank",
                ValidRange = "Y/N or leave blank",
                UniqueColumnName = "Is District Administrator"
            };
            result.Fields.Add(isDA);

            var isDC = new AssessmentFieldTemplate
            {
                FieldName = "Is District Contact",
                FieldType = "Bool",
                Required = false,
                SortOrder = currentSortOrder++,
                ValidValues = "Y, N or blank",
                ValidRange = "Y/N or leave blank",
                UniqueColumnName = "Is District Contact"
            };
            result.Fields.Add(isDC);

            var isEnabled = new AssessmentFieldTemplate
            {
                FieldName = "Is Enabled",
                FieldType = "Bool",
                Required = false,
                SortOrder = currentSortOrder++,
                ValidValues = "Y, N or blank",
                ValidRange = "Y/N or leave blank",
                UniqueColumnName = "Is Enabled"
            };
            result.Fields.Add(isEnabled);

            var canLogin = new AssessmentFieldTemplate
            {
                FieldName = "Can Login",
                FieldType = "Bool",
                Required = false,
                SortOrder = currentSortOrder++,
                ValidValues = "Y, N or blank",
                ValidRange = "Y/N or leave blank",
                UniqueColumnName = "Can Login"
            };
            result.Fields.Add(canLogin);

            var staffRole = new AssessmentFieldTemplate
            {
                FieldName = "Teacher Role",
                FieldType = "TeacherAttribute",
                Required = false,
                SortOrder = currentSortOrder++,
                ValidValues = "Teacher,Administrator,Teaching Assistant,Other",
                ValidRange = "Any value from the list specified or blank. ",
                UniqueColumnName = "Teacher Role"
            };
            result.Fields.Add(staffRole);

            return result;
        }

        public static AssessmentTemplate GetFullRolloverTemplate(DistrictContext dbContext)
        {
            var result = new AssessmentTemplate();


            // for state tests, we need to the following fields
            var studentId = new AssessmentFieldTemplate
            {
                FieldName = "Student ID",
                FieldType = "Text",
                Required = true,
                SortOrder = 1,
                ValidValues = "North Star Student Identifier",
                UniqueColumnName = "Student ID"
            };
            result.Fields.Add(studentId);

            var studentFirstName = new AssessmentFieldTemplate
            {
                FieldName = "Student First Name",
                FieldType = "Text",
                Required = true,
                SortOrder = 2,
                ValidValues = "Text less than 255 characters.",
                UniqueColumnName = "Student First Name"
            };
            result.Fields.Add(studentFirstName);

            var studentMiddleName = new AssessmentFieldTemplate
            {
                FieldName = "Student Middle Name",
                FieldType = "Text",
                Required = false,
                SortOrder = 3,
                ValidValues = "Text less than 255 characters.",
                UniqueColumnName = "Student Middle Name"
            };
            result.Fields.Add(studentMiddleName);

            var studentLastName = new AssessmentFieldTemplate
            {
                FieldName = "Student Last Name",
                FieldType = "Text",
                Required = true,
                SortOrder = 4,
                ValidValues = "Text less than 255 characters.",
                UniqueColumnName = "Student Last Name"
            };
            result.Fields.Add(studentLastName);

            var grade = new AssessmentFieldTemplate
            {
                FieldName = "Student Grade",
                FieldType = "Grade",
                Required = true,
                SortOrder = 5,
                ValidValues = "Any integer value in the range specified, P (pre-K) or K (kindergarten).",
                ValidRange = "P, K, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12",
                UniqueColumnName = "Student Grade"
            };
            result.Fields.Add(grade);

            var dob = new AssessmentFieldTemplate
            {
                FieldName = "Student DOB",
                FieldType = "Date",
                Required = true,
                SortOrder = 6,
                ValidValues = "A date in the format MM/DD/YYYY (month and day do not need leading zeroes)",
                ValidRange = "Any date.",
                UniqueColumnName = "Student DOB"
            };
            result.Fields.Add(dob);

            var gradYear = new AssessmentFieldTemplate
            {
                FieldName = "Student Graduation Year",
                FieldType = "Integer",
                Required = false,
                SortOrder = 7,
                ValidValues = "Any integer value in the range specified.",
                ValidRange = "1900 - 2050",
                UniqueColumnName = "Student Graduation Year"
            };
            result.Fields.Add(gradYear);

            var studentActive = new AssessmentFieldTemplate
            {
                FieldName = "Student Is Active",
                FieldType = "Text",
                Required = false,
                SortOrder = 8, // make this last
                ValidValues = "Y, N or blank",
                ValidRange = "Y/N or leave blank",
                UniqueColumnName = "Student Is Active"
            };
            result.Fields.Add(studentActive);

            var currentSortOrder = 8;
            // add studentAttributes
            var studentAttributes = dbContext.StudentAttributeTypes.OrderBy(p => p.AttributeName).ToList();
            studentAttributes.Each(p =>
            {
                // get values for this field
                var values = string.Join(",", dbContext.StudentAttributeLookupValues.Where(g => g.AttributeID == p.Id).OrderBy(g => g.LookupValue).Select(r => r.LookupValue).ToList());

                result.Fields.Add(new AssessmentFieldTemplate
                {
                    FieldName = p.AttributeName,
                    FieldType = "StudentAttribute",
                    Required = false,
                    SortOrder = currentSortOrder,
                    ValidValues = "Any value from the list specified or blank. (For Student Services, a comma-separated list is allowed.  Only a single value for other types)",
                    ValidRange = values,
                    UniqueColumnName = p.AttributeName
                });
                currentSortOrder++;
            });

            // teacher fields
            var teacherId = new AssessmentFieldTemplate
            {
                FieldName = "Teacher ID",
                FieldType = "Text",
                Required = true,
                SortOrder = currentSortOrder++,
                ValidValues = "North Star Teacher Identifier",
                UniqueColumnName = "Teacher ID"
            };
            result.Fields.Add(teacherId);

            var teacherFirstName = new AssessmentFieldTemplate
            {
                FieldName = "Teacher First Name",
                FieldType = "Text",
                Required = true,
                SortOrder = currentSortOrder++,
                ValidValues = "Text less than 255 characters.",
                UniqueColumnName = "Teacher First Name"
            };
            result.Fields.Add(teacherFirstName);

            var teacherMiddleName = new AssessmentFieldTemplate
            {
                FieldName = "Teacher Middle Name",
                FieldType = "Text",
                Required = false,
                SortOrder = currentSortOrder++,
                ValidValues = "Text less than 255 characters.",
                UniqueColumnName = "Teacher Middle Name"
            };
            result.Fields.Add(teacherMiddleName);

            var teacherLastName = new AssessmentFieldTemplate
            {
                FieldName = "Teacher Last Name",
                FieldType = "Text",
                Required = true,
                SortOrder = currentSortOrder++,
                ValidValues = "Text less than 255 characters.",
                UniqueColumnName = "Teacher Last Name"
            };
            result.Fields.Add(teacherLastName);

            var teacherEmail = new AssessmentFieldTemplate
            {
                FieldName = "Teacher Email",
                FieldType = "Text",
                Required = true,
                SortOrder = currentSortOrder++,
                ValidValues = "Any valid email address.",
                ValidRange = "An email address",
                UniqueColumnName = "Teacher Email"
            };
            result.Fields.Add(teacherEmail);

            var isInterventionist = new AssessmentFieldTemplate
            {
                FieldName = "Is Interventionist",
                FieldType = "Bool",
                Required = false,
                SortOrder = currentSortOrder++,
                ValidValues = "Y, N or blank",
                ValidRange = "Y/N or leave blank",
                UniqueColumnName = "Is Interventionist"
            };
            result.Fields.Add(isInterventionist);

            // class fields
            var classIdentifier = new AssessmentFieldTemplate
            {
                FieldName = "Class Identifier",
                FieldType = "Text",
                Required = true,
                SortOrder = currentSortOrder++,
                ValidValues = "A value to uniquely identify the class.  Simply enter 'A' if you do not track this value in your district.",
                ValidRange = "Any unique text or numeric value.",
                UniqueColumnName = "Class Identifier"
            };
            result.Fields.Add(classIdentifier);

            var schoolValues = string.Join(",", dbContext.Schools.OrderBy(p => p.Name).Select(p => p.Name).ToList());
            var schoolName = new AssessmentFieldTemplate
            {
                FieldName = "School Name",
                FieldType = "School",
                Required = true,
                SortOrder = currentSortOrder++,
                ValidValues = "Any school name from the list specified.",
                ValidRange = schoolValues,
                UniqueColumnName = "School Name"
            };
            result.Fields.Add(schoolName);

            return result;
        }
        #endregion

        #region History Log
        public OutputDto_SimpleString GetFullRolloverHistoryLog(InputDto_SimpleId input)
        {
            var result = new OutputDto_SimpleString();

            var log = _loginContext.JobFullRollovers.First(p => p.Id == input.Id).ImportLog;

            result.Result = log;
            return result;
        }
        public OutputDto_SimpleString GetStudentRolloverHistoryLog(InputDto_SimpleId input)
        {
            var result = new OutputDto_SimpleString();

            var log = _loginContext.JobStudentRollovers.First(p => p.Id == input.Id).ImportLog;

            result.Result = log;
            return result;
        }

        public OutputDto_SimpleString GetTeacherRolloverHistoryLog(InputDto_SimpleId input)
        {
            var result = new OutputDto_SimpleString();

            var log = _loginContext.JobTeacherRollovers.First(p => p.Id == input.Id).ImportLog;

            result.Result = log;
            return result;
        }

        public OutputDto_Base DeleteFullRolloverHistoryItem(InputDto_SimpleId input)
        {
            var result = new OutputDto_Base();
            var historyItem = _loginContext.JobFullRollovers.First(p => p.Id == input.Id);

            if (historyItem.StaffEmail != _currentUser.Email)
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

            _loginContext.JobFullRollovers.Remove(historyItem);
            _loginContext.SaveChanges();

            return result;
        }
        public OutputDto_Base DeleteStudentRolloverHistoryItem(InputDto_SimpleId input)
        {
            var result = new OutputDto_Base();
            var historyItem = _loginContext.JobStudentRollovers.First(p => p.Id == input.Id);

            if (historyItem.StaffEmail != _currentUser.Email)
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

            _loginContext.JobStudentRollovers.Remove(historyItem);
            _loginContext.SaveChanges();

            return result;
        }

        public OutputDto_Base DeleteTeacherRolloverHistoryItem(InputDto_SimpleId input)
        {
            var result = new OutputDto_Base();
            var historyItem = _loginContext.JobTeacherRollovers.First(p => p.Id == input.Id);

            if (historyItem.StaffEmail != _currentUser.Email)
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

            _loginContext.JobTeacherRollovers.Remove(historyItem);
            _loginContext.SaveChanges();

            return result;
        }

        public class OutputDto_FullRolloverTestLog : OutputDto_Base
        {
            public OutputDto_FullRolloverTestLog()
            {
                HistoryItems = new List<JobFullRolloverDto>();
            }

            public List<JobFullRolloverDto> HistoryItems { get; set; }
            public bool RolloverInProgress { get; set; }
        }
        public class OutputDto_StudentRolloverTestLog : OutputDto_Base
        {
            public OutputDto_StudentRolloverTestLog()
            {
                HistoryItems = new List<JobStudentRolloverDto>();
            }

            public List<JobStudentRolloverDto> HistoryItems { get; set; }
            public bool RolloverInProgress { get; set; }
        }

        public class OutputDto_TeacherRolloverTestLog : OutputDto_Base
        {
            public OutputDto_TeacherRolloverTestLog()
            {
                HistoryItems = new List<JobTeacherRolloverDto>();
            }

            public List<JobTeacherRolloverDto> HistoryItems { get; set; }
            public bool RolloverInProgress { get; set; }
        }

        public OutputDto_FullRolloverTestLog LoadFullRolloverImportHistory()
        {
            var result = new OutputDto_FullRolloverTestLog();

            var items = _loginContext.JobFullRollovers.Where(p => p.StaffEmail == _currentUser.Email).OrderByDescending(p => p.StartDate);
            var inProgressRollovers = _loginContext.JobFullRollovers.Where(p => p.Status != "Error" && p.Status != "Complete" && p.Status != "Cancelled").ToList();

            foreach(var item in items)
            {
                var mappedItem = Mapper.Map<JobFullRolloverDto>(item);
                if (!String.IsNullOrEmpty(item.PotentialIssuesLog))
                {
                    mappedItem.RolloverLogMessages = JsonConvert.DeserializeObject<List<RolloverLogMessage>>(item.PotentialIssuesLog);
                }

                result.HistoryItems.Add(mappedItem);
            }

            // now lets see if there is a rollover in Progress
            SqlParameter parm = new SqlParameter()
            {
                ParameterName = "@MyID",
                SqlDbType = SqlDbType.Bit,
                Direction = System.Data.ParameterDirection.Output
            };

            _dbContext.Database.ExecuteSqlCommand("exec @MyId = dbo.ns4_student_rollover_inprogress", parm);
            result.RolloverInProgress = (bool)parm.Value || (inProgressRollovers.Count > 0);

            return result;
        }
        public OutputDto_StudentRolloverTestLog LoadStudentRolloverImportHistory()
        {
            var result = new OutputDto_StudentRolloverTestLog();

            var items = _loginContext.JobStudentRollovers.Where(p => p.StaffEmail == _currentUser.Email).OrderByDescending(p => p.StartDate);
            var inProgressRollovers = _loginContext.JobStudentRollovers.Where(p => p.Status != "Error" && p.Status != "Complete" && p.Status != "Cancelled").ToList();

            foreach (var item in items)
            {
                var mappedItem = Mapper.Map<JobStudentRolloverDto>(item);
                if (!String.IsNullOrEmpty(item.PotentialIssuesLog))
                {
                    mappedItem.RolloverLogMessages = JsonConvert.DeserializeObject<List<RolloverLogMessage>>(item.PotentialIssuesLog);
                }

                result.HistoryItems.Add(mappedItem);
            }

            // now lets see if there is a rollover in Progress
            SqlParameter parm = new SqlParameter()
            {
                ParameterName = "@MyID",
                SqlDbType = SqlDbType.Bit,
                Direction = System.Data.ParameterDirection.Output
            };

            _dbContext.Database.ExecuteSqlCommand("exec @MyId = dbo.ns4_student_rollover_inprogress", parm);
            result.RolloverInProgress = (bool)parm.Value || (inProgressRollovers.Count > 0);

            return result;
        }
        public OutputDto_TeacherRolloverTestLog LoadTeacherRolloverImportHistory()
        {
            var result = new OutputDto_TeacherRolloverTestLog();

            var items = _loginContext.JobTeacherRollovers.Where(p => p.StaffEmail == _currentUser.Email).OrderByDescending(p => p.StartDate);
            var inProgressRollovers = _loginContext.JobTeacherRollovers.Where(p => p.Status != "Error" && p.Status != "Complete" && p.Status != "Cancelled").ToList();

            foreach (var item in items)
            {
                var mappedItem = Mapper.Map<JobTeacherRolloverDto>(item);
                if (!String.IsNullOrEmpty(item.PotentialIssuesLog))
                {
                    mappedItem.RolloverLogMessages = JsonConvert.DeserializeObject<List<RolloverLogMessage>>(item.PotentialIssuesLog);
                }

                result.HistoryItems.Add(mappedItem);
            }

            // now lets see if there is a rollover in Progress
            SqlParameter parm = new SqlParameter()
            {
                ParameterName = "@MyID",
                SqlDbType = SqlDbType.Bit,
                Direction = System.Data.ParameterDirection.Output
            };

            _dbContext.Database.ExecuteSqlCommand("exec @MyId = dbo.ns4_teacher_rollover_inprogress", parm);
            result.RolloverInProgress = (bool)parm.Value || (inProgressRollovers.Count > 0);

            return result;
        }
        #endregion

        #region Main Processing Functions
        public async Task<OutputDto_Log> ProcessFullRolloverUpload(ImportTestDataViewModel files, IPhotoManager manager)
        {
            var result = new OutputDto_Log();
            int schoolYear = Int32.Parse(files.FormData["SchoolYear"]);

            if (schoolYear == 0)
            {
                result.LogItems.Add("One or more of the required fields has not been selected.  Please select all options before uploading a file.");
                return result;
            }

            // get the fields that are supposed to be in the file
            var fields = GetFullRolloverTemplate(_dbContext).Fields;

            // get the file from Azure so that we can work with it
            var uploadedFile = files.Files.First();
            var textReaders = await manager.Download(uploadedFile.Url);
            var textReader = (textReaders.Count() == 1 ? textReaders.First() : null);
            
            // to import data
            // state test data has some fields that are required, same for benchmark... based on type, make sure that those fields are available first
            // create simple tryParse functions to validate


            if (textReader != null)
            {
                var read = DataAccess.DataTable.New.Read(textReader,',');
                // TODO: make sure the same column can't be used twice, see if it throws an exception, otherwise we need to check for it
                // get type
                // chosen and passed in: benchmarkdate, assessmentid
                // validate studentid
                var columns = read.ColumnNames.ToList();

                foreach (var expectedField in fields)
                {
                    if (!columns.Contains(expectedField.UniqueColumnName, StringComparer.OrdinalIgnoreCase))
                    {
                        result.LogItems.Add(String.Format("The column '{0}' is missing from the uploaded file.  Please ensure the column is included and labeled correctly.", expectedField.UniqueColumnName));
                    }
                }
            }

            // if no errors... add a log item and add message to queue
            if (result.LogItems.Count != 0)
            {
                return result;
            }

            var newJob = _loginContext.JobFullRollovers.Create();
            newJob.StaffId = _currentUser.Id;
            newJob.StaffEmail = _currentUser.Email;
            newJob.StartDate = DateTime.Now;
            newJob.Status = "Awaiting processing";
            newJob.UploadedFileName = uploadedFile.Name;
            newJob.UploadedFileUrl = uploadedFile.Url;
            newJob.SchoolStartYear = schoolYear;
            _loginContext.JobFullRollovers.Add(newJob);
            _loginContext.SaveChanges();

            Utility.AddNewImportQueueMessage(newJob.Id, NSConstants.Azure.JobType.FullRollover);

            return result;
        }

        public async Task<OutputDto_Log> ProcessStudentRolloverUpload(ImportTestDataViewModel files, IPhotoManager manager)
        {
            var result = new OutputDto_Log();
            var batchName = files.FormData["BatchName"];
            //int schoolYear = Int32.Parse(files.FormData["SchoolYear"]);

            //if (schoolYear == 0)
            //{
            //    result.LogItems.Add("One or more of the required fields has not been selected.  Please select all options before uploading a file.");
            //    return result;
            //}

            // get the fields that are supposed to be in the file
            var fields = GetStudentRolloverTemplate(_dbContext).Fields;

            // get the file from Azure so that we can work with it
            var uploadedFile = files.Files.First();
            var textReaders = await manager.Download(uploadedFile.Url);
            var textReader = (textReaders.Count() == 1 ? textReaders.First() : null);

            // to import data
            // state test data has some fields that are required, same for benchmark... based on type, make sure that those fields are available first
            // create simple tryParse functions to validate


            if (textReader != null)
            {
                var read = DataAccess.DataTable.New.Read(textReader, ',');
                // TODO: make sure the same column can't be used twice, see if it throws an exception, otherwise we need to check for it
                // get type
                // chosen and passed in: benchmarkdate, assessmentid
                // validate studentid
                var columns = read.ColumnNames.ToList();

                foreach (var expectedField in fields)
                {
                    if (!columns.Contains(expectedField.UniqueColumnName, StringComparer.OrdinalIgnoreCase))
                    {
                        result.LogItems.Add(String.Format("The column '{0}' is missing from the uploaded file.  Please ensure the column is included and labeled correctly.", expectedField.UniqueColumnName));
                    }
                }
            }

            // if no errors... add a log item and add message to queue
            if (result.LogItems.Count != 0)
            {
                return result;
            }

            var newJob = _loginContext.JobStudentRollovers.Create();
            newJob.StaffId = _currentUser.Id;
            newJob.StaffEmail = _currentUser.Email;
            newJob.StartDate = DateTime.Now;
            newJob.Status = "Awaiting processing";
            newJob.UploadedFileName = uploadedFile.Name;
            newJob.UploadedFileUrl = uploadedFile.Url;
            newJob.BatchName = batchName;
            //newJob.SchoolStartYear = schoolYear;
            _loginContext.JobStudentRollovers.Add(newJob);
            _loginContext.SaveChanges();

            Utility.AddNewImportQueueMessage(newJob.Id, NSConstants.Azure.JobType.StudentRollover);

            return result;
        }

        public async Task<OutputDto_Log> ProcessTeacherRolloverUpload(ImportTestDataViewModel files, IPhotoManager manager)
        {
            var result = new OutputDto_Log();
            var batchName = files.FormData["BatchName"];
          
            // get the fields that are supposed to be in the file
            var fields = GetTeacherRolloverTemplate(_dbContext).Fields;

            // get the file from Azure so that we can work with it
            var uploadedFile = files.Files.First();
            var textReaders = await manager.Download(uploadedFile.Url);
            var textReader = (textReaders.Count() == 1 ? textReaders.First() : null);

            // to import data
            // state test data has some fields that are required, same for benchmark... based on type, make sure that those fields are available first
            // create simple tryParse functions to validate


            if (textReader != null)
            {
                var read = DataAccess.DataTable.New.Read(textReader, ',');
                // TODO: make sure the same column can't be used twice, see if it throws an exception, otherwise we need to check for it
                // get type
                // chosen and passed in: benchmarkdate, assessmentid
                // validate studentid
                var columns = read.ColumnNames.ToList();

                foreach (var expectedField in fields)
                {
                    if (!columns.Contains(expectedField.UniqueColumnName, StringComparer.OrdinalIgnoreCase))
                    {
                        result.LogItems.Add(String.Format("The column '{0}' is missing from the uploaded file.  Please ensure the column is included and labeled correctly.", expectedField.UniqueColumnName));
                    }
                }
            }

            // if no errors... add a log item and add message to queue
            if (result.LogItems.Count != 0)
            {
                return result;
            }

            var newJob = _loginContext.JobTeacherRollovers.Create();
            newJob.StaffId = _currentUser.Id;
            newJob.StaffEmail = _currentUser.Email;
            newJob.StartDate = DateTime.Now;
            newJob.Status = "Awaiting processing";
            newJob.UploadedFileName = uploadedFile.Name;
            newJob.UploadedFileUrl = uploadedFile.Url;
            newJob.BatchName = batchName;
            //newJob.SchoolStartYear = schoolYear;
            _loginContext.JobTeacherRollovers.Add(newJob);
            _loginContext.SaveChanges();

            Utility.AddNewImportQueueMessage(newJob.Id, NSConstants.Azure.JobType.TeacherRollover);

            return result;
        }

        // user has validated fields, continue the rollover
        public OutputDto_Base ValidateStudentRollover(int id)
        {
            var result = new OutputDto_Base();

            var existingJob = _loginContext.JobStudentRollovers.FirstOrDefault(p => p.Id == id);
            existingJob.Status = "Continuing Processing";
            _loginContext.SaveChanges();

            Utility.AddNewImportQueueMessage(existingJob.Id, NSConstants.Azure.JobType.StudentRolloverValidation);

            return result;
        }

        public OutputDto_Base ValidateTeacherRollover(int id)
        {
            var result = new OutputDto_Base();

            var existingJob = _loginContext.JobTeacherRollovers.FirstOrDefault(p => p.Id == id);
            existingJob.Status = "Continuing Processing";
            _loginContext.SaveChanges();

            Utility.AddNewImportQueueMessage(existingJob.Id, NSConstants.Azure.JobType.TeacherRolloverValidation);

            return result;
        }

        public OutputDto_Base ValidateRollover(int id)
        {
            var result = new OutputDto_Base();

            var existingJob = _loginContext.JobFullRollovers.FirstOrDefault(p => p.Id == id);
            existingJob.Status = "Continuing Processing";
            _loginContext.SaveChanges();

            Utility.AddNewImportQueueMessage(existingJob.Id, NSConstants.Azure.JobType.RolloverValidation);

            return result;
        }

        public OutputDto_Base CancelRollover(int id)
        {
            var result = new OutputDto_Base();

            var existingJob = _loginContext.JobFullRollovers.FirstOrDefault(p => p.Id == id);
            existingJob.Status = "Cancelled";
            _loginContext.SaveChanges();

            _dbContext.Database.ExecuteSqlCommand("ns4_cancel_rollover");

            return result;
        }

        public OutputDto_Base CancelStudentRollover(int id)
        {
            var result = new OutputDto_Base();

            var existingJob = _loginContext.JobStudentRollovers.FirstOrDefault(p => p.Id == id);
            existingJob.Status = "Cancelled";
            _loginContext.SaveChanges();

            _dbContext.Database.ExecuteSqlCommand("ns4_cancel_student_rollover");

            return result;
        }

        public OutputDto_Base CancelTeacherRollover(int id)
        {
            var result = new OutputDto_Base();

            var existingJob = _loginContext.JobTeacherRollovers.FirstOrDefault(p => p.Id == id);
            existingJob.Status = "Cancelled";
            _loginContext.SaveChanges();

            _dbContext.Database.ExecuteSqlCommand("ns4_cancel_teacher_rollover");

            return result;
        }

        public OutputDto_Base FullRolloverReset()
        {
            var result = new OutputDto_Base();

            var existingJobs = _loginContext.JobFullRollovers.Where(p => p.Status == "Awaiting User Verification" || p.Status == "Awaiting processing");
            existingJobs.Each(p =>
            {
                p.Status = "Cancelled";
            });
            _loginContext.SaveChanges();

            _dbContext.Database.ExecuteSqlCommand("ns4_cancel_rollover");

            return result;
        }
        public OutputDto_Base StudentRolloverReset()
        {
            var result = new OutputDto_Base();

            var existingJobs = _loginContext.JobStudentRollovers.Where(p => p.Status == "Awaiting User Verification" || p.Status == "Awaiting processing");
            existingJobs.Each(p =>
            {
                p.Status = "Cancelled";
            });
            _loginContext.SaveChanges();

            _dbContext.Database.ExecuteSqlCommand("ns4_cancel_student_rollover");

            return result;
        }
        public OutputDto_Base TeacherRolloverReset()
        {
            var result = new OutputDto_Base();

            var existingJobs = _loginContext.JobTeacherRollovers.Where(p => p.Status == "Awaiting User Verification" || p.Status == "Awaiting processing");
            existingJobs.Each(p =>
            {
                p.Status = "Cancelled";
            });
            _loginContext.SaveChanges();

            _dbContext.Database.ExecuteSqlCommand("ns4_cancel_teacher_rollover");

            return result;
        }
        #endregion
    }
}
