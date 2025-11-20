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
using EntityDto.DTO.DataEntry;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;

namespace NorthStar.EF6
{
    public class SectionDataEntryService :NSBaseDataService
    {
        private string _imageContainer = "images";
        private CloudBlobContainer _container = null;
        private CloudBlobClient _client = null;
        private CloudStorageAccount _storageAccount;
        public SectionDataEntryService(ClaimsIdentity user, string loginConnectionString) : base(user, loginConnectionString)
        {
            _storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);

            var district = _loginContext.Districts.First(p => p.Id == _currentUser.DistrictId);

            if (!String.IsNullOrEmpty(district.AzureContainerName))
            {
                _imageContainer = district.AzureContainerName;
                //photoManager.ContainerName = _imageContainer;

                _client = _storageAccount.CreateCloudBlobClient();
                _container = _client.GetContainerReference(_imageContainer);
            }
        }

        public async Task<OutputDto_StudentAssessmentResults> GetAssessmentResults(InputDto_AssessmentSectionBenchmark input)
        {
            var assessment = _dbContext.Assessments.Include("FieldGroups")
                .Include("FieldCategories")
                .Include("FieldSubCategories")
                .Include("Fields")
                .FirstOrDefault(m => m.Id == input.AssessmentId);

            // now have the assessment and controls loaded, need to get the test results from the table
            // specified in the assessment and turn each result into a StudentResult DTO
            var studentResults = _dbContext.GetAssessmentStudentResults(assessment, input.SectionId, input.BenchmarkDateId, DateTime.MaxValue, true);

            // return JObject with the proper format
            assessment.Fields = assessment.Fields.Where(p => p.DisplayInEditResultList == true).OrderBy(p => p.FieldOrder).ToList();

            var fields = _dbContext.AssessmentFields.Where(p => p.AssessmentId == input.AssessmentId && (p.FieldType == "DropdownFromDB" || p.FieldType == "checklist")).ToList();

            List<IndexedLookupListDto> lookupFields = new List<IndexedLookupListDto>();
            foreach (var field in fields)
            {
                if (!lookupFields.Any(p => p.LookupColumnName == field.LookupFieldName))
                {
                    var fieldValues = _dbContext.LookupFields.Where(p => p.FieldName == field.LookupFieldName).ToList();
                    lookupFields.Add(new IndexedLookupListDto() { LookupColumnName = field.LookupFieldName, LookupFields = fieldValues });
                }
            }

            var assessmentDto = Mapper.Map<AssessmentDto>(assessment);
            assessmentDto.LookupFields = lookupFields;
            var tdd = _dbContext.TestDueDates.First(p => p.Id == input.BenchmarkDateId);

            // add imageUrl for students
            foreach (var studentResult in studentResults)
            {
                studentResult.ImageUrl = await GetStudentImage(studentResult.StudentIdentifier, tdd.SchoolStartYear.Value);
                foreach (var result in studentResult.FieldResults)
                {
                    if (result.DbColumn == "guidedreadinggroup")
                    {
                        studentResult.GuidedReadingGroupId = result.IntValue;
                    }
                }
            }

            return new OutputDto_StudentAssessmentResults()
            {
                StudentResults = studentResults,
                Assessment = assessmentDto
            };
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

        public OutputDto_StudentEditAssessmentResult GetStudentAssessmentResult(InputDto_GetStudentAssessmentResult input)
        {
            var assessment = _dbContext.Assessments.Include("FieldGroups")
                .Include("FieldCategories")
                .Include("FieldSubCategories")
                .Include("Fields")
                .FirstOrDefault(m => m.Id == input.AssessmentId);

            // now have the assessment and controls loaded, need to get the test results from the table
            // specified in the assessment and turn each result into a StudentResult DTO
            var studentResult = _dbContext.GetStudentAssesmentResult(assessment, input.SectionId, input.BenchmarkDateId, DateTime.MaxValue, input.StudentId, false);
            var assessmentDto = Mapper.Map<AssessmentDto>(assessment);
            assessmentDto.Fields = assessmentDto.Fields.OrderBy(p => p.FieldOrder).ToList();
            assessmentDto.FieldGroups = assessmentDto.FieldGroups.OrderBy(p => p.SortOrder).ToList();
            assessmentDto.FieldCategories = assessmentDto.FieldCategories.OrderBy(p => p.SortOrder).ToList();

            List<IndexedLookupListDto> lookupFields = new List<IndexedLookupListDto>();
            foreach (var field in assessmentDto.Fields)
            {
                if (!lookupFields.Any(p => p.LookupColumnName == field.LookupFieldName))
                {
                    var fieldValues = _dbContext.LookupFields.Where(p => p.FieldName == field.LookupFieldName).ToList();
                    lookupFields.Add(new IndexedLookupListDto() { LookupColumnName = field.LookupFieldName, LookupFields = fieldValues });
                }
            }

            assessmentDto.LookupFields = lookupFields;

            return new OutputDto_StudentEditAssessmentResult()
            {
                StudentResult = studentResult,
                Assessment = assessmentDto
            };
        }

        public OutputDto_StudentEditAssessmentResult GetStudentProgressMonResult(InputDto_GetStudentProgressMonResult input)
        {
            var assessment = _dbContext.Assessments.Include("FieldGroups")
                .Include("FieldCategories")
                .Include("FieldSubCategories")
                .Include("Fields")
                .FirstOrDefault(m => m.Id == input.AssessmentId);

            // now have the assessment and controls loaded, need to get the test results from the table
            // specified in the assessment and turn each result into a StudentResult DTO
            var studentResult = _dbContext.GetStudentProgressMonResult(assessment, input.InterventionGroupId, input.StudentResultId, input.StudentId);
            var assessmentDto = Mapper.Map<AssessmentDto>(assessment);
            assessmentDto.Fields = assessmentDto.Fields.OrderBy(p => p.FieldOrder).ToList();
            assessmentDto.FieldGroups = assessmentDto.FieldGroups.OrderBy(p => p.SortOrder).ToList();
            assessmentDto.FieldCategories = assessmentDto.FieldCategories.OrderBy(p => p.SortOrder).ToList();

            List<IndexedLookupListDto> lookupFields = new List<IndexedLookupListDto>();
            foreach (var field in assessmentDto.Fields)
            {
                if (!lookupFields.Any(p => p.LookupColumnName == field.LookupFieldName))
                {
                    var fieldValues = _dbContext.LookupFields.Where(p => p.FieldName == field.LookupFieldName).ToList();
                    lookupFields.Add(new IndexedLookupListDto() { LookupColumnName = field.LookupFieldName, LookupFields = fieldValues });
                }
            }

            assessmentDto.LookupFields = lookupFields;

            return new OutputDto_StudentEditAssessmentResult()
            {
                StudentResult = studentResult,
                Assessment = assessmentDto
            };
        }

        public OutputDto_SaveAssessmentResult DeleteAssessmentResult(InputDto_SaveAssessmentResult input)
        {
            var result = new OutputDto_SaveAssessmentResult();
            var assessment = _dbContext.Assessments.First(p => p.Id == input.AssessmentId);

            // first determine if there's already a result for the current student/class/date
            var deleteSql = new StringBuilder();
            deleteSql.AppendFormat(
                "DELETE FROM {0} WHERE ID = {1}",
                assessment.StorageTable, input.StudentResult.ResultId.ToString());

            using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
            {
                try
                {
                    _dbContext.Database.Connection.Open();
                    command.CommandText = deleteSql.ToString();
                    command.CommandTimeout = command.Connection.ConnectionTimeout;
                    command.ExecuteNonQuery();
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

            // get updated result and return
            result.StudentResult = _dbContext.GetStudentAssesmentResult(assessment, input.StudentResult.ClassId.Value, input.BenchmarkDateId, DateTime.MaxValue, input.StudentResult.StudentId, true);

            return result;
        }

        private void ValidateSaveAssessment(InputDto_SaveAssessmentResult input)
        {
            if (input.BenchmarkDateId == 0)
            {
                input.Status.StatusCode = StatusCode.UserDisplayableException;
                input.Status.StatusMessage = "Please select a Benchmark Date";
            }
            if (input.StudentResult.Recorder.id == 0 || input.StudentResult.Recorder.id == -1)
            {
                input.Status.StatusCode = StatusCode.UserDisplayableException;
                input.Status.StatusMessage = "Please select a Recorder";
            }
            if (input.StudentResult.TestDate == null)
            {
                input.Status.StatusCode = StatusCode.UserDisplayableException;
                input.Status.StatusMessage = "Please select the Date Test Taken";
            }
        }

        private void ValidateSaveProgMonData(InputDto_SaveProgMonResult input)
        {
            if (input.InterventionGroupId == 0)
            {
                input.Status.StatusCode = StatusCode.UserDisplayableException;
                input.Status.StatusMessage = "Please select an Intervention Group";
            }
            if (input.StudentResult.Recorder.id == 0 || input.StudentResult.Recorder.id == -1)
            {
                input.Status.StatusCode = StatusCode.UserDisplayableException;
                input.Status.StatusMessage = "Please select a Recorder";
            }
            if (input.StudentResult.TestDate == null)
            {
                input.Status.StatusCode = StatusCode.UserDisplayableException;
                input.Status.StatusMessage = "Please select the Date Test Taken";
            }
        }

        private void ValidateRequiredFields(InputDto_SaveAssessmentResult input, Assessment assessment)
        {
            foreach(var field in assessment.Fields)
            {
                // only validate the required fields
                if(field.IsRequired)
                {
                    // get corresponding field result
                    foreach(var result in input.StudentResult.FieldResults)
                    {
                        if(result.DbColumn == field.DatabaseColumn)
                        {
                            switch (field.FieldType)
                            {
                                case "DropdownFromDB":
                                case "DropdownRange":
                                    if (result.IntValue.HasValue == false)
                                    {
                                        input.Status.StatusCode = StatusCode.UserDisplayableException;
                                        input.Status.StatusMessage = "The field '" + field.DisplayLabel + "' is required.";
                                        return;
                                    }
                                    break;
                                case "Textfield":
                                case "Textarea":
                                    if (String.IsNullOrEmpty(result.StringValue))
                                    {
                                        input.Status.StatusCode = StatusCode.UserDisplayableException;
                                        input.Status.StatusMessage = "The field '" + field.DisplayLabel + "' is required.";
                                        return;
                                    }
                                    break;
                                case "DecimalRange":
                                    if (result.DecimalValue.HasValue == false)
                                    {
                                        input.Status.StatusCode = StatusCode.UserDisplayableException;
                                        input.Status.StatusMessage = "The field '" + field.DisplayLabel + "' is required.";
                                        return;
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private void ValidateRequiredFields(InputDto_SaveProgMonResult input, Assessment assessment)
        {
            foreach (var field in assessment.Fields)
            {
                // only validate the required fields
                if (field.IsRequired)
                {
                    // get corresponding field result
                    foreach (var result in input.StudentResult.FieldResults)
                    {
                        if (result.DbColumn == field.DatabaseColumn)
                        {
                            switch (field.FieldType)
                            {
                                case "DropdownFromDB":
                                case "DropdownRange":
                                    if (result.IntValue.HasValue == false)
                                    {
                                        input.Status.StatusCode = StatusCode.UserDisplayableException;
                                        input.Status.StatusMessage = "The field '" + field.DisplayLabel + "' is required.";
                                        return;
                                    }
                                    break;
                                case "Textfield":
                                case "Textarea":
                                    if (String.IsNullOrEmpty(result.StringValue))
                                    {
                                        input.Status.StatusCode = StatusCode.UserDisplayableException;
                                        input.Status.StatusMessage = "The field '" + field.DisplayLabel + "' is required.";
                                        return;
                                    }
                                    break;
                                case "DecimalRange":
                                    if (result.DecimalValue.HasValue == false)
                                    {
                                        input.Status.StatusCode = StatusCode.UserDisplayableException;
                                        input.Status.StatusMessage = "The field '" + field.DisplayLabel + "' is required.";
                                        return;
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
        }

        public OutputDto_StudentEditHFWAssessmentResult GetHFWSingleAssessmentResult(InputDto_StudentEditHFWAssessmentResult input)
        {
            var result = new OutputDto_StudentEditHFWAssessmentResult();

            var assessment = _dbContext.Assessments
                .Include("FieldGroups")
                .Include("FieldCategories")
                .Include("FieldSubCategories")
                .Include("Fields")
                .FirstOrDefault(m => m.Id == input.AssessmentId);

            // remove unneeded fields
            if (input.IsKdg)
            {
                assessment.Fields = assessment.Fields.Where(j => string.IsNullOrEmpty(j.StorageTable) || j.Group.IsKdg).ToList();
                assessment.FieldGroups = assessment.FieldGroups.Where(j => j.IsKdg).ToList();
                result.WordCount = assessment.FieldGroups.Count;
            }
            else
            {
                result.WordCount = 100;
                if (input.WordOrder == "Alphabetic" || input.WordOrder == "alphabetic")
                {
                    assessment.Fields = assessment.Fields.Where(j => string.IsNullOrEmpty(j.StorageTable) || (j.Group.SortOrder <= input.HighWordOrder && j.Group.SortOrder >= input.LowWordOrder)).ToList();
                    assessment.FieldGroups = assessment.FieldGroups.Where(j => j.SortOrder <= input.HighWordOrder && j.SortOrder >= input.LowWordOrder).ToList();

                }
                else
                {
                    assessment.Fields = assessment.Fields.Where(j => string.IsNullOrEmpty(j.StorageTable) || (j.Group.AltOrder <= input.HighWordOrder && j.Group.AltOrder >= input.LowWordOrder)).ToList();
                    assessment.FieldGroups = assessment.FieldGroups.Where(j => j.AltOrder <= input.HighWordOrder && j.AltOrder >= input.LowWordOrder).ToList();

                }
            }

            // now have the assessment and controls loaded, need to get the test results from the table
            // specified in the assessment and turn each result into a StudentResult DTO
            var studentResult = _dbContext.GetHFWAssessmentStudentResults(assessment, input.SectionId, input.BenchmarkDateId, DateTime.MaxValue, input.StudentId);
            // var studentResult = studentResults.FirstOrDefault(p => p.ResultId == studentResultId);
            // change this to just a single student query
            // return JObject with the proper format

            result.StudentResult = studentResult;
            result.Assessment = Mapper.Map<AssessmentDto>(assessment);
            return result;
        }

        public OutputDto_Base SaveHFWAssessmentResult(InputDto_SaveHFWAssessmentResult studentResult)
        {
            //ValidateSaveAssessment(input);
            //if (input.Status.StatusCode == StatusCode.UserDisplayableException)
            //{
            //    return input;
            //}

            //var assessment = _dbContext.Assessments.Include(p => p.Fields).Include(p => p.FieldGroups).Include(p => p.FieldCategories).Include(p => p.FieldSubCategories).First(p => p.Id == input.AssessmentId);
            //ValidateRequiredFields(input, assessment);
            //if (input.Status.StatusCode == StatusCode.UserDisplayableException)
            //{
            //    return input;
            //}

            _dbContext.SaveHFWStudentData(studentResult.StudentResult, studentResult.AssessmentId, _currentUser.Id);
            return null;

        }

        public OutputDto_Base UpdateCalculatedFields()
        {
            var result = new OutputDto_Base();

            var assessment = _dbContext.Assessments
                .Include("FieldGroups")
                .Include("FieldCategories")
                .Include("FieldSubCategories")
                .Include("Fields").First(p => p.BaseType == NSAssessmentBaseType.LetterID);

            _dbContext.GetAllStudentsAssesmentResultsForLetterIdUpdate(assessment);

            //foreach (var testResult in allResults)
            //{
            //    SaveLIDBulkAssessmentResult(assessment, testResult);
            //}

            return result;
        }

        public OutputDto_Base CopyStudentAssessmentData(InputDto_CopyStudentAssessmentResult input)
        {
            var result = new OutputDto_Base();

            var assessment = _dbContext.Assessments.Include(p => p.Fields).Include(p => p.FieldGroups).Include(p => p.FieldCategories).Include(p => p.FieldSubCategories).First(p => p.Id == input.AssessmentId);
                        
            // first determine if there's already a result for the current student/class/date
            var resultExistSql = new StringBuilder();
            // only do inserts, if data already exists, skip
            resultExistSql.AppendFormat("SELECT * FROM {0} WHERE StudentID = {1} and (TestDueDateID = {2})", assessment.StorageTable, input.StudentId.ToString(), input.TargetBenchmarkDate.id);

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

            if (!isInsert)
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "This user already has data entered for the target benchmark date.  You cannot overwrite existing data using copy.  You must delete any existing data first.";
                return result;
            }

            var studentResult = _dbContext.GetStudentAssesmentResult(assessment, input.Section.id, input.SelectedBenchMarkDate.id, DateTime.MaxValue, input.StudentId, false);

            // changed by SH on 5/12/2017 -- don't try to copy empty results
            if (studentResult.TestDueDateId > 0)
            {
                using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
                {
                    try
                    {
                        _dbContext.Database.Connection.Open();
                        command.CommandText = resultExistSql.ToString();
                        command.CommandTimeout = command.Connection.ConnectionTimeout;

                        // need to detect null on testdate
                        // also need to update TestDueDateID and Recorder


                        insertUpdateSql.AppendFormat("INSERT INTO {0} (IsCopied, StudentId, SectionId, RecorderId, TestDueDateId, DateTestTaken", assessment.StorageTable);
                        // for each
                        foreach (var field in studentResult.FieldResults)
                        {
                            var control = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn);
                            if (control.FieldType != "CalculatedFieldClientOnly")
                            {
                                insertUpdateSql.AppendFormat(",{0}", field.DbColumn);
                            }
                        }
                        // TODO: 10/1/2015  add a LastModified date to the end
                        insertUpdateSql.AppendFormat(") VALUES (1, {0}, {1}, {2}, {3},'{4}'", studentResult.StudentId, studentResult.ClassId, studentResult.Recorder.id, input.TargetBenchmarkDate.id, studentResult.TestDate);
                        //for each
                        foreach (var field in studentResult.FieldResults)
                        {
                            var control = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn);
                            if (control.FieldType != "CalculatedFieldClientOnly")
                            {
                                if (control.FieldType == "CalculatedFieldDbBacked" || control.FieldType == "CalculatedFieldDbOnly" || control.FieldType == "CalculatedFieldDbBackedString")
                                {
                                    insertUpdateSql.AppendFormat(",{0}", GetFieldInsertUpdateStringCalculatedFields(assessment, field, control, studentResult));

                                }
                                else
                                {
                                    insertUpdateSql.AppendFormat(",{0}", GetFieldInsertUpdateString(field, control.FieldType));

                                }
                            }
                        }
                        insertUpdateSql.AppendFormat(")");

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

            return result;
        }

        public OutputDto_Base CopySectionAssessmentData(InputDto_CopySectionAssessmentResult input)
        {
            var result = new OutputDto_Base();

            var assessment = _dbContext.Assessments.Include(p => p.Fields).Include(p => p.FieldGroups).Include(p => p.FieldCategories).Include(p => p.FieldSubCategories).First(p => p.Id == input.AssessmentId);
            // TODO: should call ValidateRequidedFields

            var studentResults = _dbContext.GetAssessmentStudentResults(assessment, input.Section.id, input.SelectedBenchMarkDate.id, DateTime.MaxValue, false);

            foreach (var studentResult in studentResults)
            {
                // changed by SH on 5/12/2017 -- don't try to copy empty results
                if (studentResult.TestDueDateId > 0)
                {
                    // first determine if there's already a result for the current student/class/date
                    var resultExistSql = new StringBuilder();
                    // only do inserts, if data already exists, skip
                    resultExistSql.AppendFormat("SELECT * FROM {0} WHERE StudentID = {1} and (TestDueDateID = {2})", assessment.StorageTable, studentResult.StudentId.ToString(), input.TargetBenchmarkDate.id);

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

                    if (!isInsert)
                    {
                        continue;
                    }

                    using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
                    {
                        try
                        {
                            _dbContext.Database.Connection.Open();
                            command.CommandText = resultExistSql.ToString();
                            command.CommandTimeout = command.Connection.ConnectionTimeout;

                            // need to detect null on testdate
                            // also need to update TestDueDateID and Recorder


                            insertUpdateSql.AppendFormat("INSERT INTO {0} (IsCopied, StudentId, SectionId, RecorderId, TestDueDateId, DateTestTaken", assessment.StorageTable);
                            // for each
                            foreach (var field in studentResult.FieldResults)
                            {
                                var control = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn);
                                if (control.FieldType != "CalculatedFieldClientOnly")
                                {
                                    insertUpdateSql.AppendFormat(",{0}", field.DbColumn);
                                }
                            }
                            // TODO: 10/1/2015  add a LastModified date to the end
                            insertUpdateSql.AppendFormat(") VALUES (1, {0}, {1}, {2}, {3},'{4}'", studentResult.StudentId, studentResult.ClassId, studentResult.Recorder.id, input.TargetBenchmarkDate.id, studentResult.TestDate);
                            //for each
                            foreach (var field in studentResult.FieldResults)
                            {
                                var control = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn);
                                if (control.FieldType != "CalculatedFieldClientOnly")
                                {
                                    if (control.FieldType == "CalculatedFieldDbBacked" || control.FieldType == "CalculatedFieldDbOnly" || control.FieldType == "CalculatedFieldDbBackedString")
                                    {
                                        insertUpdateSql.AppendFormat(",{0}", GetFieldInsertUpdateStringCalculatedFields(assessment, field, control, studentResult));

                                    }
                                    else
                                    {
                                        insertUpdateSql.AppendFormat(",{0}", GetFieldInsertUpdateString(field, control.FieldType));

                                    }
                                }
                            }
                            insertUpdateSql.AppendFormat(")");

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
            return result;
        }

        public OutputDto_Base CopyFromStudentAssessmentData(InputDto_CopyFromStudentAssessmentResult input)
        {
            var result = new OutputDto_Base();

            var assessment = _dbContext.Assessments.Include(p => p.Fields).Include(p => p.FieldGroups).Include(p => p.FieldCategories).Include(p => p.FieldSubCategories).First(p => p.Id == input.AssessmentId);

            // first determine if there's already a result for the current student/class/date
            var resultExistSql = new StringBuilder();
            // only do inserts, if data already exists, skip
            resultExistSql.AppendFormat("SELECT * FROM {0} WHERE StudentID = {1} and (TestDueDateID = {2})", assessment.StorageTable, input.StudentId.ToString(), input.SelectedBenchMarkDate.id);

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

            if (!isInsert)
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "This user already has data entered for the target benchmark date.  You cannot overwrite existing data using copy.  You must delete any existing data first.";
                return result;
            }

            var studentResult = _dbContext.GetStudentAssesmentResult(assessment, input.Section.id, input.SourceBenchmarkDate.id, DateTime.MaxValue, input.StudentId, false);

            // changed by SH on 5/12/2017 -- don't try to copy empty results
            if (studentResult.TestDueDateId > 0)
            {
                using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
                {
                    try
                    {
                        _dbContext.Database.Connection.Open();
                        command.CommandText = resultExistSql.ToString();
                        command.CommandTimeout = command.Connection.ConnectionTimeout;

                        // need to detect null on testdate
                        // also need to update TestDueDateID and Recorder


                        insertUpdateSql.AppendFormat("INSERT INTO {0} (IsCopied, StudentId, SectionId, RecorderId, TestDueDateId, DateTestTaken", assessment.StorageTable);
                        // for each
                        foreach (var field in studentResult.FieldResults)
                        {
                            var control = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn);
                            if (control.FieldType != "CalculatedFieldClientOnly")
                            {
                                insertUpdateSql.AppendFormat(",{0}", field.DbColumn);
                            }
                        }
                        // TODO: 10/1/2015  add a LastModified date to the end
                        insertUpdateSql.AppendFormat(") VALUES (1, {0}, {1}, {2}, {3},'{4}'", studentResult.StudentId, studentResult.ClassId, studentResult.Recorder.id, input.SelectedBenchMarkDate.id, studentResult.TestDate);
                        //for each
                        foreach (var field in studentResult.FieldResults)
                        {
                            var control = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn);
                            if (control.FieldType != "CalculatedFieldClientOnly")
                            {
                                if (control.FieldType == "CalculatedFieldDbBacked" || control.FieldType == "CalculatedFieldDbOnly" || control.FieldType == "CalculatedFieldDbBackedString")
                                {
                                    insertUpdateSql.AppendFormat(",{0}", GetFieldInsertUpdateStringCalculatedFields(assessment, field, control, studentResult));

                                }
                                else
                                {
                                    insertUpdateSql.AppendFormat(",{0}", GetFieldInsertUpdateString(field, control.FieldType));

                                }
                            }
                        }
                        insertUpdateSql.AppendFormat(")");

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

            return result;
        }

        public OutputDto_Base CopyFromSectionAssessmentData(InputDto_CopyFromSectionAssessmentResult input)
        {
            var result = new OutputDto_Base();

            var assessment = _dbContext.Assessments.Include(p => p.Fields).Include(p => p.FieldGroups).Include(p => p.FieldCategories).Include(p => p.FieldSubCategories).First(p => p.Id == input.AssessmentId);
            // TODO: should call ValidateRequidedFields

            var studentResults = _dbContext.GetAssessmentStudentResults(assessment, input.Section.id, input.SourceBenchmarkDate.id, DateTime.MaxValue, false);

            foreach (var studentResult in studentResults)
            {
                // changed by SH on 5/12/2017 -- don't try to copy empty results
                if (studentResult.TestDueDateId > 0)
                {
                    // first determine if there's already a result for the current student/class/date
                    var resultExistSql = new StringBuilder();
                    // only do inserts, if data already exists, skip
                    resultExistSql.AppendFormat("SELECT * FROM {0} WHERE StudentID = {1} and (TestDueDateID = {2})", assessment.StorageTable, studentResult.StudentId.ToString(), input.SelectedBenchMarkDate.id);

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

                    if (!isInsert)
                    {
                        continue;
                    }

                    using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
                    {
                        try
                        {
                            _dbContext.Database.Connection.Open();
                            command.CommandText = resultExistSql.ToString();
                            command.CommandTimeout = command.Connection.ConnectionTimeout;

                            // need to detect null on testdate
                            // also need to update TestDueDateID and Recorder


                            insertUpdateSql.AppendFormat("INSERT INTO {0} (IsCopied, StudentId, SectionId, RecorderId, TestDueDateId, DateTestTaken", assessment.StorageTable);
                            // for each
                            foreach (var field in studentResult.FieldResults)
                            {
                                var control = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn);
                                if (control.FieldType != "CalculatedFieldClientOnly")
                                {
                                    insertUpdateSql.AppendFormat(",{0}", field.DbColumn);
                                }
                            }
                            // TODO: 10/1/2015  add a LastModified date to the end
                            insertUpdateSql.AppendFormat(") VALUES (1, {0}, {1}, {2}, {3},'{4}'", studentResult.StudentId, studentResult.ClassId, studentResult.Recorder.id, input.SelectedBenchMarkDate.id, studentResult.TestDate);
                            //for each
                            foreach (var field in studentResult.FieldResults)
                            {
                                var control = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn);
                                if (control.FieldType != "CalculatedFieldClientOnly")
                                {
                                    if (control.FieldType == "CalculatedFieldDbBacked" || control.FieldType == "CalculatedFieldDbOnly" || control.FieldType == "CalculatedFieldDbBackedString")
                                    {
                                        insertUpdateSql.AppendFormat(",{0}", GetFieldInsertUpdateStringCalculatedFields(assessment, field, control, studentResult));

                                    }
                                    else
                                    {
                                        insertUpdateSql.AppendFormat(",{0}", GetFieldInsertUpdateString(field, control.FieldType));

                                    }
                                }
                            }
                            insertUpdateSql.AppendFormat(")");

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
            return result;
        }

        public async Task<OutputDto_SaveAssessmentResult> SaveAssessmentResult(InputDto_SaveAssessmentResult input)
        {
            var result = new OutputDto_SaveAssessmentResult();

            //var testduedateid = 374;
            // var recorderid = 1570;
            ValidateSaveAssessment(input);
            if(input.Status.StatusCode == StatusCode.UserDisplayableException)
            {
                result.StudentResult = input.StudentResult;
                return result;
            }

            var assessment = _dbContext.Assessments.Include(p => p.Fields).Include(p => p.FieldGroups).Include(p => p.FieldCategories).Include(p => p.FieldSubCategories).First(p => p.Id == input.AssessmentId);
            ValidateRequiredFields(input, assessment);
            if (input.Status.StatusCode == StatusCode.UserDisplayableException)
            {
                result.StudentResult = input.StudentResult;
                return result;
            }

            // first determine if there's already a result for the current student/class/date
            var resultExistSql = new StringBuilder();
            resultExistSql.AppendFormat("SELECT * FROM {0} WHERE StudentID = {1} and SectionID = {2} and (TestDueDateID = {3})", assessment.StorageTable, input.StudentResult.StudentId.ToString(), input.StudentResult.ClassId.ToString(), input.BenchmarkDateId, input.StudentResult.TestDate.HasValue ? input.StudentResult.TestDate.Value.ToShortDateString() : null);

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

            using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
            {
                try
                {
                    _dbContext.Database.Connection.Open();
                    command.CommandText = resultExistSql.ToString();
                    command.CommandTimeout = command.Connection.ConnectionTimeout;

                    // need to detect null on testdate
                    // also need to update TestDueDateID and Recorder

                    if (isInsert)
                    {
                        insertUpdateSql.AppendFormat("INSERT INTO {0} (StudentId, SectionId, RecorderId, TestDueDateId, DateTestTaken", assessment.StorageTable);
                        // for each
                        foreach (var field in input.StudentResult.FieldResults)
                        {
                            var control = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn);
                            if (control.FieldType != "CalculatedFieldClientOnly")
                            {
                                insertUpdateSql.AppendFormat(",{0}", field.DbColumn);
                            }
                        }
                        // TODO: 10/1/2015  add a LastModified date to the end
                        insertUpdateSql.AppendFormat(") VALUES ({0}, {1}, {2}, {3},'{4}'", input.StudentResult.StudentId, input.StudentResult.ClassId, input.StudentResult.Recorder.id, input.BenchmarkDateId, input.StudentResult.TestDate);
                        //for each
                        foreach (var field in input.StudentResult.FieldResults)
                        {
                            var control = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn);
                            if (control.FieldType != "CalculatedFieldClientOnly")
                            {
                                if (control.FieldType == "CalculatedFieldDbBacked" || control.FieldType == "CalculatedFieldDbOnly" || control.FieldType == "CalculatedFieldDbBackedString")
                                {
                                    insertUpdateSql.AppendFormat(",{0}", GetFieldInsertUpdateStringCalculatedFields(assessment, field, control, input.StudentResult));

                                }
                                else
                                {
                                    insertUpdateSql.AppendFormat(",{0}", GetFieldInsertUpdateString(field, control.FieldType));

                                }
                            }
                        }
                        insertUpdateSql.AppendFormat(")");
                    }
                    else
                    {
                        insertUpdateSql.AppendFormat("UPDATE {0} SET ", assessment.StorageTable);

                        // update recorder
                        insertUpdateSql.AppendFormat("{0} = {1},", "RecorderID", input.StudentResult.Recorder.id);
                        insertUpdateSql.AppendFormat("{0} = '{1}',", "DateTestTaken", input.StudentResult.TestDate);
                        insertUpdateSql.AppendFormat("{0} = 0,", "IsCopied");

                        // don't include fields that we don't have fields for
                        foreach (var field in input.StudentResult.FieldResults)
                        {
                            // don't try to update fields that don't have a dbcolumn
                            var control = assessment.Fields.FirstOrDefault(p => p.DatabaseColumn == field.DbColumn);
                            if (control != null && control.FieldType != "CalculatedFieldClientOnly")
                            {
                                if (control.FieldType == "CalculatedFieldDbBacked" || control.FieldType == "CalculatedFieldDbOnly" || control.FieldType == "CalculatedFieldDbBackedString")
                                {
                                    insertUpdateSql.AppendFormat("{0} = {1},", field.DbColumn, GetFieldInsertUpdateStringCalculatedFields(assessment, field, control, input.StudentResult));
                                }
                                else
                                {
                                    insertUpdateSql.AppendFormat("{0} = {1},", field.DbColumn, GetFieldInsertUpdateString(field, control.FieldType));
                                }
                            }
                        }
                        // remove trailing comma
                        insertUpdateSql.Remove(insertUpdateSql.Length - 1, 1);
                        insertUpdateSql.AppendFormat(" WHERE StudentId = {0} AND SectionID = {1} and TestDueDateId = {2}", input.StudentResult.StudentId, input.StudentResult.ClassId, input.BenchmarkDateId); // or testdate = {4}
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

            // get updated result and return
            result.StudentResult = _dbContext.GetStudentAssesmentResult(assessment, input.StudentResult.ClassId.Value, input.BenchmarkDateId, DateTime.MaxValue, input.StudentResult.StudentId, false);
            var tdd = _dbContext.TestDueDates.First(p => p.Id == input.BenchmarkDateId);
            if (!String.IsNullOrEmpty(input.StudentResult.StudentIdentifier))
            {
                result.StudentResult.ImageUrl = await GetStudentImage(input.StudentResult.StudentIdentifier, tdd.SchoolStartYear.Value);
            }

            return result;
        }

        public OutputDto_SaveAssessmentResult SaveProgMonResult(InputDto_SaveProgMonResult input)
        {
            var result = new OutputDto_SaveAssessmentResult();

            //var testduedateid = 374;
            // var recorderid = 1570;
            ValidateSaveProgMonData(input);
            if (input.Status.StatusCode == StatusCode.UserDisplayableException)
            {
                result.StudentResult = input.StudentResult;
                return result;
            }

            var assessment = _dbContext.Assessments.Include(p => p.Fields).Include(p => p.FieldGroups).Include(p => p.FieldCategories).Include(p => p.FieldSubCategories).First(p => p.Id == input.AssessmentId);
            ValidateRequiredFields(input, assessment);
            if (input.Status.StatusCode == StatusCode.UserDisplayableException)
            {
                result.StudentResult = input.StudentResult;
                return result;
            }

            // first determine if there's already a result for the current student/class/date
            var resultExistSql = new StringBuilder();
            resultExistSql.AppendFormat("SELECT * FROM {0} WHERE StudentID = {1} and InterventionGroupId = {2} and (ID = {3})", assessment.StorageTable, input.StudentResult.StudentId.ToString(), input.StudentResult.ClassId.ToString(), input.StudentResult.ResultId);

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

            using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
            {
                try
                {
                    _dbContext.Database.Connection.Open();
                    command.CommandText = resultExistSql.ToString();
                    command.CommandTimeout = command.Connection.ConnectionTimeout;

                    // need to detect null on testdate
                    // also need to update TestDueDateID and Recorder

                    if (isInsert)
                    {
                        insertUpdateSql.AppendFormat("INSERT INTO {0} (StudentId, InterventionGroupId, RecorderId, TestDueDate ", assessment.StorageTable);
                        // for each
                        foreach (var field in input.StudentResult.FieldResults)
                        {
                            var control = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn);
                            if (control.FieldType != "CalculatedFieldClientOnly")
                            {
                                insertUpdateSql.AppendFormat(",{0}", field.DbColumn);
                            }
                        }
                        // TODO: 10/1/2015  add a LastModified date to the end
                        insertUpdateSql.AppendFormat(") VALUES ({0}, {1}, {2}, '{3}'", input.StudentResult.StudentId, input.StudentResult.ClassId, input.StudentResult.Recorder.id, input.StudentResult.TestDate);
                        //for each
                        foreach (var field in input.StudentResult.FieldResults)
                        {
                            var control = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn);
                            if (control.FieldType != "CalculatedFieldClientOnly")
                            {
                                if (control.FieldType == "CalculatedFieldDbBacked" || control.FieldType == "CalculatedFieldDbOnly" || control.FieldType == "CalculatedFieldDbBackedString")
                                {
                                    insertUpdateSql.AppendFormat(",{0}", GetFieldInsertUpdateStringCalculatedFields(assessment, field, control, input.StudentResult));

                                }
                                else
                                {
                                    insertUpdateSql.AppendFormat(",{0}", GetFieldInsertUpdateString(field, control.FieldType));

                                }
                            }
                        }
                        insertUpdateSql.AppendFormat(")");
                    }
                    else
                    {
                        insertUpdateSql.AppendFormat("UPDATE {0} SET ", assessment.StorageTable);

                        // update recorder
                        insertUpdateSql.AppendFormat("{0} = {1},", "RecorderID", input.StudentResult.Recorder.id);
                        insertUpdateSql.AppendFormat("{0} = '{1}',", "TestDueDate", input.StudentResult.TestDate);
                        // don't include fields that we don't have fields for
                        foreach (var field in input.StudentResult.FieldResults)
                        {
                            // don't try to update fields that don't have a dbcolumn
                            var control = assessment.Fields.FirstOrDefault(p => p.DatabaseColumn == field.DbColumn);
                            if (control != null && control.FieldType != "CalculatedFieldClientOnly")
                            {
                                if (control.FieldType == "CalculatedFieldDbBacked" || control.FieldType == "CalculatedFieldDbOnly" || control.FieldType == "CalculatedFieldDbBackedString")
                                {
                                    insertUpdateSql.AppendFormat("{0} = {1},", field.DbColumn, GetFieldInsertUpdateStringCalculatedFields(assessment, field, control, input.StudentResult));
                                }
                                else
                                {
                                    insertUpdateSql.AppendFormat("{0} = {1},", field.DbColumn, GetFieldInsertUpdateString(field, control.FieldType));
                                }
                            }
                        }
                        // remove trailing comma
                        insertUpdateSql.Remove(insertUpdateSql.Length - 1, 1);
                        insertUpdateSql.AppendFormat(" WHERE StudentId = {0} AND InterventionGroupID = {1} and ID = {2}", input.StudentResult.StudentId, input.StudentResult.ClassId, input.StudentResult.ResultId); // or testdate = {4}
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

            return result;
        }

        internal string GetFieldInsertUpdateStringCalculatedFields(Assessment assessment, AssessmentFieldResult field, AssessmentField asmntField, AssessmentStudentResult result)
        {
            switch (asmntField.FieldType)
            {
                case "CalculatedFieldDbBacked":
                case "CalculatedFieldDbBackedString":
                    int sum = 0;

                    if (asmntField.CalculationFunction == "Sum")
                    {
                        var fieldsToSum = asmntField.CalculationFields.Split(Char.Parse(","));

                        foreach (var currentResult in result.FieldResults)
                        {
                            foreach (var fieldNameToSum in fieldsToSum)
                            {
                                if (currentResult.DbColumn.Trim().ToLower() == fieldNameToSum.Trim().ToLower())
                                {
                                    sum += currentResult.IntValue ?? 0;
                                }
                            }
                        }
                        field.IntValue = sum;
                        return sum.ToString();
                    }
                    if (asmntField.CalculationFunction == "SumBool")
                    {
                        var fieldsToSum = asmntField.CalculationFields.Split(Char.Parse(","));

                        foreach (var currentResult in result.FieldResults)
                        {
                            foreach (var fieldNameToSum in fieldsToSum)
                            {
                                if (currentResult.DbColumn.Trim().ToLower() == fieldNameToSum.Trim().ToLower())
                                {
                                    sum += currentResult.BoolValue.HasValue ? (currentResult.BoolValue.Value ? 1 : 0) : 0;
                                }
                            }
                        }
                        field.IntValue = sum;
                        return sum.ToString();
                    }
                    if (asmntField.CalculationFunction == "SumBoolByGroup")
                    {
                        foreach (var group in assessment.FieldGroups)
                        {
                            var groupId = group.Id;

                            // i don't like having this reference to the field... need to figure out
                            // if it makes more sense to pass the additional data for each field
                            // or to just join them on the client
                            foreach (var currentResult in result.FieldResults)
                            {
                                // fix this later so that field properties are part of the fieldresults
                                // it is really getting stupid to keep looking this up
                                var currentField = assessment.Fields.First(p => p.DatabaseColumn == currentResult.DbColumn);

                                if (currentField.GroupId == groupId && currentResult.DbColumn.Substring(0, 3) == "chk")
                                {
                                    // only add each groupid once

                                    if (currentResult.BoolValue.HasValue && currentResult.BoolValue.Value)
                                    {
                                        sum++;
                                        break;
                                    }
                                }
                            }
                        }


                        field.IntValue = sum;
                        return sum.ToString();
                    }
                    if (asmntField.CalculationFunction == "ConcatenatedMissingLetters")
                    {
                        var unknownLetters = "";
                        var unknownLettersList = new List<string>();

                        foreach (var group in assessment.FieldGroups)
                        {
                            var groupId = group.Id;

                            var foundInGroup = false;

                            foreach (var currentResult in result.FieldResults)
                            {
                                var currentField = assessment.Fields.First(p => p.DatabaseColumn == currentResult.DbColumn);

                                if (currentField.GroupId == groupId && currentResult.DbColumn.Substring(0, 3) == "chk")
                                {
                                    if (currentResult.BoolValue.HasValue && currentResult.BoolValue.Value)
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
                                        unknownLettersList.Add(currentField.DisplayLabel);
                                        //unknownLetters += currentField.DisplayLabel + ",";
                                        break;
                                    }
                                }
                            }
                        }
                        //remove trailing comma
                        if (unknownLettersList.Count > 0)
                        {
                            unknownLettersList.Sort();
                            unknownLetters = string.Join(",", unknownLettersList);
                            //unknownLetters = unknownLetters.Substring(0, unknownLetters.Length - 1);
                        }
                        else
                        {
                            unknownLetters = "none";
                        }
                        field.StringValue = unknownLetters;
                        return String.Format("'{0}'", unknownLetters); ;
                    }
                    return "0";
                case "CalculatedFieldDbOnly":
                    string stringValue = String.Empty;
                    if (asmntField.CalculationFunction == "BenchmarkLevel")
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
                                var fpField = result.FieldResults.FirstOrDefault(p => p.DbColumn == "FPValueID");
                                fpValueId.Value = fpField == null ? 0 : fpField.IntValue ?? 0;

                                var accuracy = command.CreateParameter();
                                accuracy.Direction = ParameterDirection.Input;
                                accuracy.DbType = DbType.Int32;
                                accuracy.ParameterName = "@Accuracy";
                                var accuracyField = result.FieldResults.FirstOrDefault(p => p.DbColumn == "Accuracy");
                                accuracy.Value = accuracyField == null ? 0 : accuracyField.DecimalValue ?? 0;

                                var compScore = command.CreateParameter();
                                compScore.Direction = ParameterDirection.Input;
                                compScore.DbType = DbType.Int32;
                                compScore.ParameterName = "@CompScore";
                                int newCompScoreSum = 0;
                                var aboutField = result.FieldResults.FirstOrDefault(p => p.DbColumn == "About");
                                var withinField = result.FieldResults.FirstOrDefault(p => p.DbColumn == "Within");
                                var beyondField = result.FieldResults.FirstOrDefault(p => p.DbColumn == "Beyond");
                                var extraPtField = result.FieldResults.FirstOrDefault(p => p.DbColumn == "ExtraPt");

                                compScore.Value = (aboutField == null ? 0 : aboutField.IntValue ?? 0) +
                                    (withinField == null ? 0 : withinField.IntValue ?? 0) +
                                    (beyondField == null ? 0 : beyondField.IntValue ?? 0) +
                                    (extraPtField == null ? 0 : extraPtField.IntValue ?? 0);

                                command.Parameters.Add(outParameter);
                                command.Parameters.Add(fpValueId);
                                command.Parameters.Add(accuracy);
                                command.Parameters.Add(compScore);
                                command.ExecuteNonQuery();
                                stringValue = ((System.Data.SqlClient.SqlParameter)(command.Parameters["@RETURN_VALUE"])).Value.ToString();
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        field.StringValue = stringValue;
                    }
                    else if (asmntField.CalculationFunction == "BenchmarkLevelV3")
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
                                var fpField = result.FieldResults.FirstOrDefault(p => p.DbColumn == "FPValueID");
                                fpValueId.Value = fpField == null ? 0 : fpField.IntValue ?? 0;

                                var accuracy = command.CreateParameter();
                                accuracy.Direction = ParameterDirection.Input;
                                accuracy.DbType = DbType.Int32;
                                accuracy.ParameterName = "@Accuracy";
                                var accuracyField = result.FieldResults.FirstOrDefault(p => p.DbColumn == "Accuracy");
                                accuracy.Value = accuracyField == null ? 0 : accuracyField.DecimalValue ?? 0;

                                var compScore = command.CreateParameter();
                                compScore.Direction = ParameterDirection.Input;
                                compScore.DbType = DbType.Int32;
                                compScore.ParameterName = "@CompScore";
                                int newCompScoreSum = 0;
                                var aboutField = result.FieldResults.FirstOrDefault(p => p.DbColumn == "About");
                                var withinField = result.FieldResults.FirstOrDefault(p => p.DbColumn == "Within");
                                var beyondField = result.FieldResults.FirstOrDefault(p => p.DbColumn == "Beyond");
                                var extraPtField = result.FieldResults.FirstOrDefault(p => p.DbColumn == "ExtraPt");

                                compScore.Value = (aboutField == null ? 0 : aboutField.IntValue ?? 0) +
                                    (withinField == null ? 0 : withinField.IntValue ?? 0) +
                                    (beyondField == null ? 0 : beyondField.IntValue ?? 0) +
                                    (extraPtField == null ? 0 : extraPtField.IntValue ?? 0);

                                command.Parameters.Add(outParameter);
                                command.Parameters.Add(fpValueId);
                                command.Parameters.Add(accuracy);
                                command.Parameters.Add(compScore);
                                command.ExecuteNonQuery();
                                stringValue = ((System.Data.SqlClient.SqlParameter)(command.Parameters["@RETURN_VALUE"])).Value.ToString();
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        field.StringValue = stringValue;
                    }
                    return String.Format("'{0}'", stringValue);

            }

            return String.Empty;
        }

        internal string GetFieldInsertUpdateString(AssessmentFieldResult field, string controlType)
        {
            switch (controlType)
            {
                case "checklist":
                    return field.ChecklistValues.Count == 0 ? "null" : String.Format("'{0}'", string.Join(",", field.ChecklistValues));
                case "DropdownRange":
                    return field.IntValue?.ToString() ?? "null";
                case "Textfield":
                case "Textarea":                
                    return String.Format("'{0}'", field.StringValue == null ? String.Empty : field.StringValue.Replace("'", "''"));
                case "Checkbox":
                    return String.Format("{0}", field.BoolValue.HasValue ? (field.BoolValue.Value == true ? 1 : 0) : 0);
                case "DecimalRange":
                    return field.DecimalValue?.ToString() ?? "null";
                case "DropdownFromDB":
                    return field.IntValue?.ToString() ?? "null";
                case "CalculatedFieldDbBacked":
                    return field.IntValue?.ToString() ?? "null";
                case "CalculatedFieldDbBackedString":
                    return String.Format("'{0}'", field.StringValue.Replace("'", "''"));
                case "CalculatedFieldDbOnly":
                    return String.Format("'{0}'", field.StringValue.Replace("'", "''"));
                case "DateCheckbox":
                case "Date":
                    // if null, set null
                    return String.Format("{0}", field.DateValue.HasValue ? "'" + field.DateValue.Value.ToShortDateString() + "'" : "null");
                default:
                    return String.Format("'{0}'", field.StringValue.Replace("'", "''"));
            }
        }
    }
}
