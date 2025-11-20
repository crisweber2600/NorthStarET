using AutoMapper;
using DataAccess;
using EntityDto.DTO.Admin.Simple;
using EntityDto.DTO.Assessment;
using EntityDto.DTO.ImportExport;
using EntityDto.LoginDB.Entity;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Northstar.Core;
using Northstar.Core.Extensions;
using NorthStar.Core;
using NorthStar.Core.FileUpload;
using NorthStar.Core.Identity;
using NorthStar.EF6;
using NorthStar4.API.Infrastructure;
using NorthStar4.CrossPlatform.DTO.Reports.ObservationSummary;
using NorthStar4.PCL.Entity;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Client;
using Thinktecture.IdentityModel.Extensions;
using Winnovative;
using Winnovative.HtmlToPdfClient;

namespace NorthStar.BatchProcessor
{
    public class StudentDataEntryResultInfo
    {
        public int SectionID { get; set; }
        public int TestDueDateID { get; set; }
        public int ID { get; set; }
        public int StudentID { get; set; }
    }
    public class JobProcessor
    {
        LoginContext _loginContext;
        DistrictContext _dbContext;
        readonly string _loginConnectionString;
        int DistrictId = 0;
        
        private PrintType printType = new PrintType();
        public JobProcessor(DistrictContext dbContext, LoginContext loginContext)
        {
            _loginContext = loginContext;
            _dbContext = dbContext;
            _loginConnectionString = ConfigurationManager.ConnectionStrings["LoginConnection"].ConnectionString;
        }

        public string SiteUrlBase
        {
            get
            {
                return ConfigurationManager.AppSettings["SiteUrlBase"];
            }
            set { }
        }

        public string IdentityServerUrl
        {
            get
            {
                return ConfigurationManager.AppSettings["IdentityServer"];
            }
            set { }
        }

        public class StateStudentDetails
        {
            public int? StudentId { get; set; }
            public int? GradeId { get; set; }
            public int? TestDueDateId { get; set; }
            public int? GradeOrder { get; set; }
        }
        public async Task ProcessStateTestDataJob(JobStateTestDataImport job, TextWriter output)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            // now that we have dbcontext... can process stuff
            var result = new OutputDto_Log();
            job.Status = "Processing";
            _loginContext.SaveChanges();
            output.WriteLine("Starting processing of job.");
            // get the MN prelim assessemnts
            // TODO:  pass in a flag that says prelim or not... hard code for now
            Assessment assessment = _dbContext.Assessments
                .Include(p => p.Fields)
                .First(p => p.Id == job.AssessmentId);

            var fields = ImportTestDataService.GetImportableColumns(assessment);

            // download file from azure for processing
            var manager = new AzurePhotoManager(CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString), NSConstants.Azure.AssessmentImportContainer);

            var textReaders = await manager.Download(job.UploadedFileUrl);
            var textReader = (textReaders.Count() == 1 ? textReaders.First() : null);


            var read = DataAccess.DataTable.New.Read(textReader);
            var columns = read.ColumnNames.ToList();

            foreach (var row in read.Rows)
            {
                try
                {
                    var recordIsValid = true;
                    var studentCode = row["STUDENT ID"];


                    // get columns that are on the list
                    var validFields = new List<AssessmentField>();
                    foreach (var column in columns)
                    {
                        foreach (var field in fields)
                        {
                            if (field.DisplayLabel.Equals(column, StringComparison.OrdinalIgnoreCase))
                            {
                                validFields.Add(field);

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
                    var student = _dbContext.Students.FirstOrDefault(p => p.StudentIdentifier == studentCode);

                    if (student == null)
                    {
                        result.LogItems.Add(String.Format("The following student was not found in the database: {0}, {1} - {2} \r\n", row["Student Last Name"], row["Student First Name"], studentCode));
                        continue; // skip this kid
                    }
                    else
                    {
                        var dateTest = DateTime.MinValue;
                        if(!DateTime.TryParseExact(row["Date Test Taken"], "yyyyMMdd", provider, DateTimeStyles.None, out dateTest))
                        {
                            dateTest = DateTime.Parse(row["Date Test Taken"]);
                        }

                        var results = _dbContext.Database.SqlQuery<StateStudentDetails>(@"EXEC [ns4_GetStateTestStudentInfo] @dateTestTaken, 
                        @studentCode, @grade",
                    new SqlParameter("dateTestTaken", SqlDbType.DateTime) { Value = dateTest },
                    new SqlParameter("studentCode", row["Student ID"]),
                    new SqlParameter("grade", row["Grade"]));

                        if(!"P,1,2,3,4,5,6,7,8,9,10,11,12,P,K".Contains(row["Grade"]))
                        {
                            result.LogItems.Add(String.Format("You have assigned an invalid Grade Code of '{3}' and the record has been skipped for for: {0}, {1} - {2} \r\n", row["Student Last Name"], row["Student First Name"], studentCode, row["Grade"]));
                            continue;
                        }

                        var studentDetails = results.FirstOrDefault();
                        if (studentDetails == null || studentDetails.GradeId == 0 || studentDetails.GradeId == null)
                        {
                            result.LogItems.Add(String.Format("Data for the following student could not be added (You may be using an invalid Grade Code): {0}, {1} - {2} \r\n", row["Student Last Name"], row["Student First Name"], studentCode));
                            Log.Information("Could not import data for an assessment for record: {0}", row);
                            continue;
                        }

                        // now see if this kid already has a record in the table for this this tdd
                        var isInsert = ImportTestDataService.IsStateInsert(student.Id, row["GRADE"], assessment.StorageTable, _dbContext);
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
                                    insertUpdateSql.AppendFormat("INSERT INTO {0} (TestDate, TestDueDateId, Grade, GradeId, SchoolStartYear, StateStudentID, StudentId, LastName, FirstName, GradeOrder", assessment.StorageTable);

                                    // for each
                                    foreach (var field in validFields)
                                    {
                                        insertUpdateSql.AppendFormat(",{0}", field.DatabaseColumn);
                                    }
                                    // TODO: 10/1/2015  add a LastModified date to the end, figure out where to get SectionID (DONT NEED), RecorderID can passIN
                                    // validate the required fields to ensure that they contain a value before adding to the script
                                    insertUpdateSql.AppendFormat(") VALUES ('{0}',{1},'{2}',{3},{4},{5},{6},'{7}','{8}',{9}",
                                        dateTest,
                                        studentDetails.TestDueDateId,
                                        row["Grade"],
                                        studentDetails.GradeId,
                                        job.SchoolStartYear,
                                        row["Student ID"],
                                        studentDetails.StudentId,
                                        row["Student Last Name"].Replace("'", "''"),
                                        row["Student First Name"].Replace("'", "''"),
                                        studentDetails.GradeOrder);

                                    foreach (var field in validFields)
                                    {
                                        insertUpdateSql.AppendFormat(",{0}", ImportTestDataService.GetFieldInsertUpdateString(field, row, result, _dbContext));
                                    }
                                    insertUpdateSql.AppendFormat(")");
                                }
                                else
                                {
                                    insertUpdateSql.AppendFormat("UPDATE {0} SET ", assessment.StorageTable);

                                    // update test_date
                                    insertUpdateSql.AppendFormat("{0} = '{1}',", "TestDate", dateTest);

                                    // don't include fields that we don't have fields for
                                    foreach (var field in validFields)
                                    {
                                        insertUpdateSql.AppendFormat("{0} = {1},", field.DatabaseColumn, ImportTestDataService.GetFieldInsertUpdateString(field, row, result, _dbContext));
                                    }
                                    // remove trailing comma
                                    insertUpdateSql.Remove(insertUpdateSql.Length - 1, 1);
                                    insertUpdateSql.AppendFormat(" WHERE StateStudentID = '{0}' AND Grade = {1}", row["STUDENT ID"], row["GRADE"]);
                                }

                                command.CommandText = insertUpdateSql.ToString();
                                command.ExecuteNonQuery();

                                // update test due dates and gradeIds


                            }
                            catch (Exception ex)
                            {
                                //log
                                output.WriteLine("Error while processing State Test Data batch: {0}", ex.Message);
                                job.Status = "Error";
                                job.EndDate = DateTime.Now;
                                job.ImportLog = ConvertLogToString(result);
                                _loginContext.SaveChanges();
                            }
                            finally
                            {
                                _dbContext.Database.Connection.Close();
                                command.Parameters.Clear();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    output.WriteLine("Outer Exception while processing State Test Data batch: {0}", ex.Message);
                }
            }


            job.Status = "Complete";
            job.EndDate = DateTime.Now;
            job.ImportLog = ConvertLogToString(result);
            _loginContext.SaveChanges();
        }

        public async Task ProcessBenchmarkTestDataJob(JobBenchmarkDataImport job, TextWriter output)
        {
            // now that we have dbcontext... can process stuff
            var result = new OutputDto_Log();
            job.Status = "Processing";
            _loginContext.SaveChanges();

            // get the MN prelim assessemnts
            // TODO:  pass in a flag that says prelim or not... hard code for now
            Assessment assessment = _dbContext.Assessments
                .Include(p => p.Fields)
                .First(p => p.Id == job.AssessmentId);

            var fields = ImportTestDataService.GetImportableColumns(assessment);
            var computedFields = ImportTestDataService.GetCalculatedColumns(assessment);

            // download file from azure for processing
            var manager = new AzurePhotoManager(CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString), NSConstants.Azure.AssessmentImportContainer);

            var textReaders = await manager.Download(job.UploadedFileUrl);
            var textReader = (textReaders.Count() == 1 ? textReaders.First() : null);


            var read = DataAccess.DataTable.New.Read(textReader);
            var columns = read.ColumnNames.ToList();

            foreach (var row in read.Rows)
            {
                try
                {
                    var recordIsValid = true;
                    var studentCode = row["STUDENT ID"];


                    // get columns that are on the list
                    var validFields = new List<AssessmentField>();
                    foreach (var column in columns)
                    {
                        foreach (var field in fields)
                        {
                            if (field.DisplayLabel.Equals(column, StringComparison.OrdinalIgnoreCase))
                            {
                                validFields.Add(field);

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
                    var student = _dbContext.Students.FirstOrDefault(p => p.StudentIdentifier == studentCode);

                    // TODO: check to see if user can access this student.. make a function of some sort in DB

                    if (student == null)
                    {
                        result.LogItems.Add(String.Format("The following student was not found in the database: {0}, {1} - {2} \r\n", row["Student Last Name"], row["Student First Name"], studentCode));
                        continue; // skip this kid
                    }
                    else
                    {
                        // now see if this kid already has a record in the table for this this tdd
                        var isInsert = ImportTestDataService.IsInsert(student.Id, job.BenchmarkDateId, assessment.StorageTable, _dbContext);
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
                                    // validate the required fields to ensure that they contain a value before adding to the script
                                    insertUpdateSql.AppendFormat(") VALUES ({0},-1,{1},{2},'{3}'",
                                        student.Id,
                                        job.RecorderId,
                                        job.BenchmarkDateId,
                                        DateTime.Parse(row["Date Test Taken"]));

                                    //for each
                                    foreach (var field in computedFields)
                                    {
                                        insertUpdateSql.AppendFormat(",{0}", ImportTestDataService.GetFieldInsertUpdateStringCalculatedFields(field, assessment, row, result, _dbContext)); // use the rest of the fields to calculate
                                    }

                                    foreach (var field in validFields)
                                    {
                                        insertUpdateSql.AppendFormat(",{0}", ImportTestDataService.GetFieldInsertUpdateString(field, row, result, _dbContext));
                                    }
                                    insertUpdateSql.AppendFormat(")");
                                }
                                else
                                {
                                    insertUpdateSql.AppendFormat("UPDATE {0} SET ", assessment.StorageTable);

                                    // update test_date
                                    insertUpdateSql.AppendFormat("{0} = '{1}',", "DateTestTaken", DateTime.Parse(row["Date Test Taken"]));

                                    foreach (var field in computedFields)
                                    {
                                        insertUpdateSql.AppendFormat("{0} = {1},", field.DatabaseColumn, ImportTestDataService.GetFieldInsertUpdateStringCalculatedFields(field, assessment, row, result, _dbContext));
                                    }

                                    // don't include fields that we don't have fields for
                                    foreach (var field in validFields)
                                    {
                                        insertUpdateSql.AppendFormat("{0} = {1},", field.DatabaseColumn, ImportTestDataService.GetFieldInsertUpdateString(field, row, result, _dbContext));
                                    }
                                    // remove trailing comma
                                    insertUpdateSql.Remove(insertUpdateSql.Length - 1, 1);
                                    insertUpdateSql.AppendFormat(" WHERE StudentId = {0} AND TestDueDateId = {1}", student.Id, job.BenchmarkDateId);
                                }

                                command.CommandText = insertUpdateSql.ToString();
                                command.ExecuteNonQuery();

                                // update test due dates and gradeIds


                            }
                            catch (Exception ex)
                            {
                                //log
                                output.WriteLine("Error while processing Benchmark Test Data batch: {0}", ex.Message);
                                job.Status = "Error";
                                job.EndDate = DateTime.Now;
                                job.ImportLog = ConvertLogToString(result);
                                _loginContext.SaveChanges();
                            }
                            finally
                            {
                                _dbContext.Database.Connection.Close();
                                command.Parameters.Clear();
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    output.WriteLine("Outer Exception while processing Benchmark Data batch: {0}", ex.Message);
                }
            }

            job.Status = "Complete";
            job.EndDate = DateTime.Now;
            job.ImportLog = ConvertLogToString(result);
            _loginContext.SaveChanges();
        }

        public class StaffAccountChange
        {
            public string NewUserName { get; set; }
            public string OldUsername { get; set; }
        }

        public class CanLoginStatusChange
        {
            public string Email { get; set; }
            public string CanLogin { get; set; }
        }

        private string CreatePassword(int length)
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

        public void FinalizeFullRolloverJob(JobFullRollover job, TextWriter output, OutputDto_Log result, int districtId)
        {
            if(result == null)
            {
                result = new OutputDto_Log();
            }

            try
            {
                _dbContext.Database.ExecuteSqlCommand("EXEC ns4_process_fullRollover @SchoolStartYear", new SqlParameter("@SchoolStartYear", job.SchoolStartYear));

                // now update user accounts that are chagned or new
                var newAccounts = _dbContext.Database.SqlQuery<StaffAccountChange>("SELECT Email as NewUserName, Loweredusername as OldUserName FROM Staff WHERE ModifiedBy = 'rollover' and LoweredUserName IS NULL").ToList();
                var changedAccounts = _dbContext.Database.SqlQuery<StaffAccountChange>("SELECT Email as NewUserName, Loweredusername as OldUserName FROM Staff WHERE ModifiedBy = 'rollover' and LoweredUserName != Email").ToList();


                UserStoreManager mgr = new UserStoreManager(_loginConnectionString);
                newAccounts.Each(p =>
                {
                    try
                    {

                        // make sure these accounts don't already exist (this fixes mistakes)
                        var existingStaffDistict = _loginContext.StaffDistricts.FirstOrDefault(sd => sd.StaffEmail == p.NewUserName);

                        // should be null
                        if (existingStaffDistict == null)
                        {
                            // create new account and add to staffdistrict
                            var newPassword = CreatePassword(8);
                            mgr.CreateUser(p.NewUserName, newPassword);
                            StaffDistrict sd = new StaffDistrict { DistrictId = districtId, StaffEmail = p.NewUserName };
                            _loginContext.StaffDistricts.Add(sd);
                            _loginContext.SaveChanges();

                            var staff = _dbContext.Staffs.First(s => s.Email == p.NewUserName);
                            staff.ModifiedBy = "useraccountcreation";
                            staff.ModifiedDate = DateTime.Now;
                            _dbContext.SaveChanges();

                            // send email
                            //EmailHandler.SendUserPasswordEmail(newPassword, p.NewUserName, p.NewUserName, "support@northstaret.net", p.NewUserName, SiteUrlBase);
                        }
                        else
                        {
                            output.WriteLine("STRANGE CASE: Attempting to create a new user, but the user already has a record in the staffdistrict table: {0}", p.NewUserName);
                        }
                    }
                    catch (Exception ex)
                    {
                        output.WriteLine("Error creating new user account for user: {0}.  Error is: {1}", p.NewUserName, ex.Message);
                    }
                });

                changedAccounts.Each(p =>
                {
                    try
                    {

                        using (System.Data.IDbCommand command = _loginContext.Database.Connection.CreateCommand())
                        {
                            try
                            {
                                _loginContext.Database.Connection.Open();
                                command.CommandText = "changeUserName";
                                command.CommandType = System.Data.CommandType.StoredProcedure;
                                command.CommandTimeout = command.Connection.ConnectionTimeout;
                                command.Parameters.Add(new SqlParameter("@newusername", p.NewUserName));
                                command.Parameters.Add(new SqlParameter("@oldusername", p.OldUsername));

                                command.ExecuteNonQuery();
                                // send email
                                //EmailHandler.SendNewUsernameEmail(p.NewUserName, p.NewUserName, SiteUrlBase);
                            }
                            finally
                            {
                                _loginContext.Database.Connection.Close();
                                command.Parameters.Clear();
                            }
                        }

                        var staff = _dbContext.Staffs.First(s => s.Email == p.NewUserName);
                        staff.ModifiedBy = "useraccountchanged";
                        staff.ModifiedDate = DateTime.Now;
                        _dbContext.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        output.WriteLine("Error changing username after rollover for user: {0}.  Error is: {1}", p.NewUserName, ex.Message);
                    }
                });

                result.LogItems.Add("Rollover completed successfully\r\n");
                job.Status = "Complete";
                job.EndDate = DateTime.Now;
                job.ImportLog = ConvertLogToString(result);
                _loginContext.SaveChanges();
            }
            catch (Exception ex)
            {
                // send email to support... i want to know when this happens
                result.LogItems.Add("Rollover was not processed successfully.  An error occurred that prevented successful completion.  Please review the log for details.\r\n");
                output.WriteLine("Error while processing Full Rollover batch: {0}", ex.Message);
                job.Status = "Error";
                job.EndDate = DateTime.Now;
                job.ImportLog = ConvertLogToString(result);
                _loginContext.SaveChanges();
            }
        }

        public void FinalizeTeacherRolloverJob(JobTeacherRollover job, TextWriter output, OutputDto_Log result, int districtId)
        {
            if (result == null)
            {
                result = new OutputDto_Log();
            }

            try
            {
                UserStoreManager mgr = new UserStoreManager(_loginConnectionString);

                _dbContext.Database.ExecuteSqlCommand("EXEC ns4_process_teacherRollover");

                // now update user accounts that are chagned or new
                var newAccounts = _dbContext.Database.SqlQuery<StaffAccountChange>("SELECT Email as NewUserName, Loweredusername as OldUserName FROM Staff WHERE ModifiedBy = 'rollover' and LoweredUserName IS NULL").ToList();
                var changedAccounts = _dbContext.Database.SqlQuery<StaffAccountChange>("SELECT Email as NewUserName, Loweredusername as OldUserName FROM Staff WHERE ModifiedBy = 'rollover' and LoweredUserName != Email").ToList();



                newAccounts.Each(p =>
                {
                    try
                    {

                        // make sure these accounts don't already exist (this fixes mistakes)
                        var existingStaffDistict = _loginContext.StaffDistricts.FirstOrDefault(sd => sd.StaffEmail == p.NewUserName);

                        // should be null
                        if (existingStaffDistict == null)
                        {
                            // create new account and add to staffdistrict
                            var newPassword = CreatePassword(8);
                            mgr.CreateUser(p.NewUserName, newPassword);
                            StaffDistrict sd = new StaffDistrict { DistrictId = districtId, StaffEmail = p.NewUserName };
                            _loginContext.StaffDistricts.Add(sd);
                            _loginContext.SaveChanges();

                            var staff = _dbContext.Staffs.First(s => s.Email == p.NewUserName);
                            staff.ModifiedBy = "useraccountcreation";
                            staff.ModifiedDate = DateTime.Now;
                            _dbContext.SaveChanges();

                            // send email
                            EmailHandler.SendUserPasswordEmail(newPassword, p.NewUserName, p.NewUserName, "support@northstaret.net", p.NewUserName, SiteUrlBase);
                        }
                        else
                        {
                            output.WriteLine("STRANGE CASE: Attempting to create a new user, but the user already has a record in the staffdistrict table: {0}", p.NewUserName);
                        }
                    }
                    catch (Exception ex)
                    {
                        output.WriteLine("Error creating new user account for user: {0}.  Error is: {1}", p.NewUserName, ex.Message);
                    }
                });

                changedAccounts.Each(p =>
                {
                    try
                    {

                        using (System.Data.IDbCommand command = _loginContext.Database.Connection.CreateCommand())
                        {
                            try
                            {
                                _loginContext.Database.Connection.Open();
                                command.CommandText = "changeUserName";
                                command.CommandType = System.Data.CommandType.StoredProcedure;
                                command.CommandTimeout = command.Connection.ConnectionTimeout;
                                command.Parameters.Add(new SqlParameter("@newusername", p.NewUserName));
                                command.Parameters.Add(new SqlParameter("@oldusername", p.OldUsername));

                                command.ExecuteNonQuery();
                                // send email
                                EmailHandler.SendNewUsernameEmail(p.NewUserName, p.NewUserName, SiteUrlBase);
                            }
                            finally
                            {
                                _loginContext.Database.Connection.Close();
                                command.Parameters.Clear();
                            }
                        }

                        var staff = _dbContext.Staffs.First(s => s.Email == p.NewUserName);
                        staff.ModifiedBy = "useraccountchanged";
                        staff.ModifiedDate = DateTime.Now;
                        _dbContext.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        output.WriteLine("Error changing username after rollover for user: {0}.  Error is: {1}", p.NewUserName, ex.Message);
                    }
                });

                // set canLogin as well
                var canLoginStatus = _dbContext.Database.SqlQuery<CanLoginStatusChange>("SELECT [Teacher Email] as Email, [Can Login] as CanLogin  FROM TeacherRollover").ToList();
                canLoginStatus.Each(p =>
                {
                    if (p.CanLogin == "N")
                    {
                        mgr.LockUserAccount(p.Email);
                    }
                    else
                    {
                        mgr.UnLockUserAccount(p.Email);
                    }
                });

                // now clear the rollover table
                _dbContext.Database.ExecuteSqlCommand("EXEC ns4_cancel_teacher_rollover");

                result.LogItems.Add("Rollover completed successfully\r\n");
                job.Status = "Complete";
                job.EndDate = DateTime.Now;
                job.ImportLog = ConvertLogToString(result);
                _loginContext.SaveChanges();
            }
            catch (Exception ex)
            {
                // send email to support... i want to know when this happens
                result.LogItems.Add("Rollover was not processed successfully.  An error occurred that prevented successful completion.  Please review the log for details.\r\n");
                output.WriteLine("Error while processing Full Rollover batch: {0}", ex.Message);
                job.Status = "Error";
                job.EndDate = DateTime.Now;
                job.ImportLog = ConvertLogToString(result);
                _loginContext.SaveChanges();
            }
        }

        public void FinalizeStudentRolloverJob(JobStudentRollover job, TextWriter output, OutputDto_Log result, int districtId)
        {
            if (result == null)
            {
                result = new OutputDto_Log();
            }

            try
            {
                _dbContext.Database.ExecuteSqlCommand("EXEC ns4_process_studentRollover");

                result.LogItems.Add("Student Rollover completed successfully\r\n");
                job.Status = "Complete";
                job.EndDate = DateTime.Now;
                job.ImportLog = ConvertLogToString(result);
                _loginContext.SaveChanges();
            }
            catch (Exception ex)
            {
                // send email to support... i want to know when this happens
                result.LogItems.Add("Student Rollover was not processed successfully.  An error occurred that prevented successful completion.  Please review the log for details.\r\n");
                output.WriteLine("Error while processing Full Rollover batch: {0}", ex.Message);
                job.Status = "Error";
                job.EndDate = DateTime.Now;
                job.ImportLog = ConvertLogToString(result);
                _loginContext.SaveChanges();
            }
        }

        public async Task ProccessFullRolloverJob(JobFullRollover job, TextWriter output, int districtId)
        {
            DistrictId = districtId;
            // clear any existing rollover crap
            _dbContext.Database.ExecuteSqlCommand("EXEC dbo.[ns4_cancel_rollover]");
            //now that we have dbcontext...can process stuff
            var invalidRecordCount = 0;
            var validRecordCount = 0;
            var result = new OutputDto_Log();
            job.Status = "Processing";
            _loginContext.SaveChanges();

            var fields = RosterRolloverDataService.GetFullRolloverTemplate(_dbContext).Fields;

            // download file from azure for processing
            var manager = new AzurePhotoManager(CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString), NSConstants.Azure.RolloverContainer);

            var textReaders = await manager.Download(job.UploadedFileUrl);
            var textReader = (textReaders.Count() == 1 ? textReaders.First() : null);


            var read = DataAccess.DataTable.New.Read(textReader);
            var columns = read.ColumnNames.ToList();

            foreach (var row in read.Rows)
            {
                try
                {
                    var recordIsValid = true;
                    var studentCode = row["STUDENT ID"];

                    // get columns that are on the list
                    var validFields = new List<AssessmentField>();
                    foreach (var column in columns)
                    {
                        foreach (var field in fields)
                        {
                            if (field.UniqueColumnName.Equals(column, StringComparison.OrdinalIgnoreCase))
                            {

                                if (row[column] == null && field.Required)
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
                        invalidRecordCount++;
                        continue;
                    }

                    // we've now at least validated that all required fields are there
                    // let's now validate that all fields in the record have valid values
                    foreach (var field in fields)
                    {
                        if (!ValidateRolloverField(field, row, result, _dbContext))
                        {
                            //result.LogItems.Add(String.Format("The following student's record had the invalid value '{0}' for the field '{1}' and is being skipped (could also be an invalid student attribute value): {2}, {3} - {4} \r\n", row[field.FieldName], field.FieldName, row["Student Last Name"], row["Student First Name"], studentCode));
                            recordIsValid = false;
                        }
                    }

                    if (!recordIsValid)
                    {
                        invalidRecordCount++;
                        continue;
                    }

                    // now the fields have valid values, lets insert the records into the db and run some queries and check results
                    


                    var insertUpdateSql = new StringBuilder();
                    var insertColumns = new StringBuilder();

                    if (invalidRecordCount == 0)
                    {
                        using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
                        {
                            try
                            {


                                // need to detect null on testdate
                                // also need to update TestDueDateID and Recorder

                                insertColumns.AppendFormat(@"INSERT INTO FullRollover (");

                                var attributeFieldCount = 1;
                                // for each
                                foreach (var field in fields)
                                {

                                    // last field
                                    if (field.FieldName == "School Name")
                                    {
                                        insertColumns.AppendFormat(@"[{0}]) Values (", field.FieldName);
                                        insertUpdateSql.AppendFormat("{0})", GetRolloverFieldInsertString(field, row, result, _dbContext));
                                    }
                                    else if (field.FieldType == "StudentAttribute")
                                    {
                                        insertColumns.AppendFormat(@"[StudentAttributeName{0}],", attributeFieldCount);
                                        insertColumns.AppendFormat(@"[StudentAttributeValue{0}],", attributeFieldCount);
                                        insertUpdateSql.AppendFormat("'{0}',", field.FieldName.Replace("'", "''"));
                                        insertUpdateSql.AppendFormat("'{0}',", row[field.FieldName]?.Replace("'", "''"));
                                        attributeFieldCount++;
                                    }
                                    else
                                    {
                                        insertColumns.AppendFormat(@"[{0}],", field.FieldName);
                                        // all other fields
                                        insertUpdateSql.AppendFormat("{0},", GetRolloverFieldInsertString(field, row, result, _dbContext));
                                    }
                                }

                                _dbContext.Database.Connection.Open();
                                command.CommandTimeout = command.Connection.ConnectionTimeout;
                                command.CommandText = insertColumns.ToString() + insertUpdateSql.ToString();
                                command.ExecuteNonQuery();
                                validRecordCount++;

                            }
                            catch (Exception ex)
                            {
                                //log
                                output.WriteLine("Error while processing Full Rollover batch: {0}", ex.Message);
                                result.LogItems.Add("An unknown error occurred while processing a record for student: " + studentCode + "\r\n");
                                job.Status = "Error";
                                job.EndDate = DateTime.Now;
                                job.ImportLog = ConvertLogToString(result);
                                invalidRecordCount++;
                                job.RecordsProcessed = validRecordCount;
                                job.RecordsSkipped = invalidRecordCount;
                                _loginContext.SaveChanges();
                                return;
                            }
                            finally
                            {
                                _dbContext.Database.Connection.Close();
                                command.Parameters.Clear();
                                _loginContext.SaveChanges();
                            }
                        }
                    } 
                }
                catch (Exception ex)
                {
                    output.WriteLine("Outer Exception while processing Rollover Data batch: {0}", ex.Message);
                }
            }

            // if there are any invalid records, kill the rollover
            if(invalidRecordCount > 0)
            {
                _dbContext.Database.ExecuteSqlCommand("EXEC dbo.[ns4_cancel_rollover]");
                job.Status = "Error";
                job.ImportLog = ConvertLogToString(result);
                job.RecordsProcessed = validRecordCount;
                job.RecordsSkipped = invalidRecordCount;
                _loginContext.SaveChanges();
                return;
            }


            // all records are inserted... time for the fun part
            var integrityResults = _dbContext.Database.SqlQuery<RolloverLogMessage>("EXEC dbo.ns4_rollover_integritycheck @SchoolStartYear", new SqlParameter("@SchoolStartYear", job.SchoolStartYear)).ToList();

            if(integrityResults.Count == 0)
            {
                var duplicationResults = _dbContext.Database.SqlQuery<RolloverLogMessage>("EXEC dbo.ns4_rollover_duplicationcheck").ToList();

                if(duplicationResults.Count != 0)
                {
                    job.PotentialIssuesLog = JsonConvert.SerializeObject(duplicationResults);
                    job.Status = "Awaiting User Verification";
                    job.RecordsProcessed = validRecordCount;
                    job.RecordsSkipped = invalidRecordCount;
                    _loginContext.SaveChanges();
                    return;
                }
            } else
            {
                job.ImportLog = ConvertLogToString(integrityResults);
                job.Status = "Error";
                job.RecordsProcessed = validRecordCount;
                job.RecordsSkipped = invalidRecordCount;
                _loginContext.SaveChanges();
                return;
            }

            job.RecordsProcessed = validRecordCount;
            job.RecordsSkipped = invalidRecordCount;
            // now that everything's been validated... do the rollover
            FinalizeFullRolloverJob(job, output, result, districtId);

        }

        public async Task ProccessTeacherRolloverJob(JobTeacherRollover job, TextWriter output, int districtId)
        {
            // clear any existing rollover crap
            _dbContext.Database.ExecuteSqlCommand("EXEC dbo.[ns4_cancel_teacher_rollover]");
            //now that we have dbcontext...can process stuff
            var invalidRecordCount = 0;
            var validRecordCount = 0;
            var result = new OutputDto_Log();
            job.Status = "Processing";
            _loginContext.SaveChanges();

            var fields = RosterRolloverDataService.GetTeacherRolloverTemplate(_dbContext).Fields;

            // download file from azure for processing
            var manager = new AzurePhotoManager(CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString), NSConstants.Azure.RolloverContainer);

            var textReaders = await manager.Download(job.UploadedFileUrl);
            var textReader = (textReaders.Count() == 1 ? textReaders.First() : null);


            var read = DataAccess.DataTable.New.Read(textReader);
            var columns = read.ColumnNames.ToList();

            foreach (var row in read.Rows)
            {
                try
                {
                    var recordIsValid = true;
                    var teacherCode = row["TEACHER ID"];

                    // get columns that are on the list
                    var validFields = new List<AssessmentField>();
                    foreach (var column in columns)
                    {
                        foreach (var field in fields)
                        {
                            if (field.UniqueColumnName.Equals(column, StringComparison.OrdinalIgnoreCase))
                            {

                                if (row[column] == null && field.Required)
                                {
                                    result.LogItems.Add(String.Format("The following teacher's record did not have a value for the required field '{3}' and is being skipped: {0}, {1} - {2} \r\n", row["Teacher Last Name"], row["Teacher First Name"], teacherCode, column));
                                    recordIsValid = false;
                                    break;
                                }
                            }
                        }
                    }

                    // make sure required fields have values
                    if (!recordIsValid)
                    {
                        invalidRecordCount++;
                        continue;
                    }

                    // we've now at least validated that all required fields are there
                    // let's now validate that all fields in the record have valid values
                    foreach (var field in fields)
                    {
                        if (!ValidateRolloverField(field, row, result, _dbContext))
                        {
                            //result.LogItems.Add(String.Format("The following student's record had the invalid value '{0}' for the field '{1}' and is being skipped (could also be an invalid student attribute value): {2}, {3} - {4} \r\n", row[field.FieldName], field.FieldName, row["Student Last Name"], row["Student First Name"], studentCode));
                            recordIsValid = false;
                        }
                    }

                    if (!recordIsValid)
                    {
                        invalidRecordCount++;
                        continue;
                    }

                    // now the fields have valid values, lets insert the records into the db and run some queries and check results



                    var insertUpdateSql = new StringBuilder();
                    var insertColumns = new StringBuilder();

                    if (invalidRecordCount == 0)
                    {
                        using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
                        {
                            try
                            {


                                // need to detect null on testdate
                                // also need to update TestDueDateID and Recorder

                                insertColumns.AppendFormat(@"INSERT INTO TeacherRollover (");

                                // for each
                                foreach (var field in fields)
                                {

                                    // last field
                                    if (field.FieldName == "Teacher Role")
                                    {
                                        insertColumns.AppendFormat(@"[{0}]) Values (", field.FieldName);
                                        insertUpdateSql.AppendFormat("{0})", GetRolloverFieldInsertString(field, row, result, _dbContext));
                                    }
                                    else
                                    {
                                        insertColumns.AppendFormat(@"[{0}],", field.FieldName);
                                        // all other fields
                                        insertUpdateSql.AppendFormat("{0},", GetRolloverFieldInsertString(field, row, result, _dbContext));
                                    }
                                }

                                _dbContext.Database.Connection.Open();
                                command.CommandTimeout = command.Connection.ConnectionTimeout;
                                command.CommandText = insertColumns.ToString() + insertUpdateSql.ToString();
                                command.ExecuteNonQuery();
                                validRecordCount++;

                            }
                            catch (Exception ex)
                            {
                                //log
                                output.WriteLine("Error while processing Student Rollover batch: {0}", ex.Message);
                                result.LogItems.Add("An unknown error occurred while processing a record for teacher: " + teacherCode + "\r\n");
                                job.Status = "Error";
                                job.EndDate = DateTime.Now;
                                job.ImportLog = ConvertLogToString(result);
                                invalidRecordCount++;
                                job.RecordsProcessed = validRecordCount;
                                job.RecordsSkipped = invalidRecordCount;
                                _loginContext.SaveChanges();
                                return;
                            }
                            finally
                            {
                                _dbContext.Database.Connection.Close();
                                command.Parameters.Clear();
                                _loginContext.SaveChanges();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    output.WriteLine("Outer Exception while processing Teacher Rollover Data batch: {0}", ex.Message);
                }
            }

            // if there are any invalid records, kill the rollover
            if (invalidRecordCount > 0)
            {
                _dbContext.Database.ExecuteSqlCommand("EXEC dbo.[ns4_cancel_teacher_rollover]");
                job.Status = "Error";
                job.ImportLog = ConvertLogToString(result);
                job.RecordsProcessed = validRecordCount;
                job.RecordsSkipped = invalidRecordCount;
                _loginContext.SaveChanges();
                return;
            }


            // all records are inserted... time for the fun part
            var integrityResults = _dbContext.Database.SqlQuery<RolloverLogMessage>("EXEC dbo.ns4_teacher_rollover_integritycheck").ToList();

            if (integrityResults.Count == 0)
            {
                var duplicationResults = _dbContext.Database.SqlQuery<RolloverLogMessage>("EXEC dbo.ns4_teacher_rollover_duplicationcheck").ToList();

                if (duplicationResults.Count != 0)
                {
                    job.PotentialIssuesLog = JsonConvert.SerializeObject(duplicationResults);
                    job.Status = "Awaiting User Verification";
                    job.RecordsProcessed = validRecordCount;
                    job.RecordsSkipped = invalidRecordCount;
                    _loginContext.SaveChanges();
                    return;
                }
            }
            else
            {
                job.ImportLog = ConvertLogToString(integrityResults);
                job.Status = "Error";
                job.RecordsProcessed = validRecordCount;
                job.RecordsSkipped = invalidRecordCount;
                _loginContext.SaveChanges();
                return;
            }

            job.RecordsProcessed = validRecordCount;
            job.RecordsSkipped = invalidRecordCount;
            // now that everything's been validated... do the rollover
            FinalizeTeacherRolloverJob(job, output, result, districtId);

        }

        public async Task ProccessStudentRolloverJob(JobStudentRollover job, TextWriter output, int districtId)
        {
            // clear any existing rollover crap
            _dbContext.Database.ExecuteSqlCommand("EXEC dbo.[ns4_cancel_student_rollover]");
            //now that we have dbcontext...can process stuff
            var invalidRecordCount = 0;
            var validRecordCount = 0;
            var result = new OutputDto_Log();
            job.Status = "Processing";
            _loginContext.SaveChanges();

            var fields = RosterRolloverDataService.GetStudentRolloverTemplate(_dbContext).Fields;

            // download file from azure for processing
            var manager = new AzurePhotoManager(CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString), NSConstants.Azure.RolloverContainer);

            var textReaders = await manager.Download(job.UploadedFileUrl);
            var textReader = (textReaders.Count() == 1 ? textReaders.First() : null);


            var read = DataAccess.DataTable.New.Read(textReader);
            var columns = read.ColumnNames.ToList();

            foreach (var row in read.Rows)
            {
                try
                {
                    var recordIsValid = true;
                    var studentCode = row["STUDENT ID"];

                    // get columns that are on the list
                    var validFields = new List<AssessmentField>();
                    foreach (var column in columns)
                    {
                        foreach (var field in fields)
                        {
                            if (field.UniqueColumnName.Equals(column, StringComparison.OrdinalIgnoreCase))
                            {

                                if (row[column] == null && field.Required)
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
                        invalidRecordCount++;
                        continue;
                    }

                    // we've now at least validated that all required fields are there
                    // let's now validate that all fields in the record have valid values
                    foreach (var field in fields)
                    {
                        if (!ValidateRolloverField(field, row, result, _dbContext))
                        {
                            //result.LogItems.Add(String.Format("The following student's record had the invalid value '{0}' for the field '{1}' and is being skipped (could also be an invalid student attribute value): {2}, {3} - {4} \r\n", row[field.FieldName], field.FieldName, row["Student Last Name"], row["Student First Name"], studentCode));
                            recordIsValid = false;
                        }
                    }

                    if (!recordIsValid)
                    {
                        invalidRecordCount++;
                        continue;
                    }

                    // now the fields have valid values, lets insert the records into the db and run some queries and check results



                    var insertUpdateSql = new StringBuilder();
                    var insertColumns = new StringBuilder();

                    if (invalidRecordCount == 0)
                    {
                        using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
                        {
                            try
                            {


                                // need to detect null on testdate
                                // also need to update TestDueDateID and Recorder

                                insertColumns.AppendFormat(@"INSERT INTO StudentRollover (");

                                var attributeFieldCount = 1;
                                // for each
                                foreach (var field in fields)
                                {

                                    // last field
                                    if (field.FieldName == "Student Is Active")
                                    {
                                        insertColumns.AppendFormat(@"[{0}]) Values (", field.FieldName);
                                        insertUpdateSql.AppendFormat("{0})", GetRolloverFieldInsertString(field, row, result, _dbContext));
                                    }
                                    else if (field.FieldType == "StudentAttribute")
                                    {
                                        insertColumns.AppendFormat(@"[StudentAttributeName{0}],", attributeFieldCount);
                                        insertColumns.AppendFormat(@"[StudentAttributeValue{0}],", attributeFieldCount);
                                        insertUpdateSql.AppendFormat("'{0}',", field.FieldName.Replace("'", "''"));
                                        insertUpdateSql.AppendFormat("'{0}',", row[field.FieldName]?.Replace("'", "''"));
                                        attributeFieldCount++;
                                    }
                                    else
                                    {
                                        insertColumns.AppendFormat(@"[{0}],", field.FieldName);
                                        // all other fields
                                        insertUpdateSql.AppendFormat("{0},", GetRolloverFieldInsertString(field, row, result, _dbContext));
                                    }
                                }

                                _dbContext.Database.Connection.Open();
                                command.CommandTimeout = command.Connection.ConnectionTimeout;
                                command.CommandText = insertColumns.ToString() + insertUpdateSql.ToString();
                                command.ExecuteNonQuery();
                                validRecordCount++;

                            }
                            catch (Exception ex)
                            {
                                //log
                                output.WriteLine("Error while processing Student Rollover batch: {0}", ex.Message);
                                result.LogItems.Add("An unknown error occurred while processing a record for student: " + studentCode + "\r\n");
                                job.Status = "Error";
                                job.EndDate = DateTime.Now;
                                job.ImportLog = ConvertLogToString(result);
                                invalidRecordCount++;
                                job.RecordsProcessed = validRecordCount;
                                job.RecordsSkipped = invalidRecordCount;
                                _loginContext.SaveChanges();
                                return;
                            }
                            finally
                            {
                                _dbContext.Database.Connection.Close();
                                command.Parameters.Clear();
                                _loginContext.SaveChanges();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    output.WriteLine("Outer Exception while processing Rollover Data batch: {0}", ex.Message);
                }
            }

            // if there are any invalid records, kill the rollover
            if (invalidRecordCount > 0)
            {
                _dbContext.Database.ExecuteSqlCommand("EXEC dbo.[ns4_cancel_student_rollover]");
                job.Status = "Error";
                job.ImportLog = ConvertLogToString(result);
                job.RecordsProcessed = validRecordCount;
                job.RecordsSkipped = invalidRecordCount;
                _loginContext.SaveChanges();
                return;
            }


            // all records are inserted... time for the fun part
            var integrityResults = _dbContext.Database.SqlQuery<RolloverLogMessage>("EXEC dbo.ns4_student_rollover_integritycheck").ToList();

            if (integrityResults.Count == 0)
            {
                var duplicationResults = _dbContext.Database.SqlQuery<RolloverLogMessage>("EXEC dbo.ns4_student_rollover_duplicationcheck").ToList();

                if (duplicationResults.Count != 0)
                {
                    job.PotentialIssuesLog = JsonConvert.SerializeObject(duplicationResults);
                    job.Status = "Awaiting User Verification";
                    job.RecordsProcessed = validRecordCount;
                    job.RecordsSkipped = invalidRecordCount;
                    _loginContext.SaveChanges();
                    return;
                }
            }
            else
            {
                job.ImportLog = ConvertLogToString(integrityResults);
                job.Status = "Error";
                job.RecordsProcessed = validRecordCount;
                job.RecordsSkipped = invalidRecordCount;
                _loginContext.SaveChanges();
                return;
            }

            job.RecordsProcessed = validRecordCount;
            job.RecordsSkipped = invalidRecordCount;
            // now that everything's been validated... do the rollover
            FinalizeStudentRolloverJob(job, output, result, districtId);

        }

        // no need to validate if a field is null and required, this has been done already
        public bool ValidateRolloverField(AssessmentFieldTemplate field, Row row, OutputDto_Log result, DistrictContext _dbContext)
        {
            switch (field.FieldType)
            {
                case "Grade":
                    if (!"P,1,2,3,4,5,6,7,8,9,10,11,12,P,K".Contains(row[field.FieldName]))
                    {
                        result.LogItems.Add(String.Format("You have assigned an invalid Grade Code of '{3}' and the record has been skipped for for: {0}, {1} - {2} \r\n", row["Student Last Name"], row["Student First Name"], row["Student Id"], row[field.FieldName]));
                        return false;
                    }
                    break;
                case "TeacherAttribute":
                    if (!String.IsNullOrEmpty(row[field.FieldName]) && !"Teacher,Administrator,Teaching Assistant,Other".Contains(row[field.FieldName]))
                    {
                        result.LogItems.Add(String.Format("You have assigned an invalid Staff Role of '{3}' and the record has been skipped for for: {0}, {1} - {2} \r\n", row["Teacher Last Name"], row["Teacher First Name"], row["Teacher Id"], row[field.FieldName]));
                        return false;
                    }
                    break;
                case "School":
                    var schoolName = row[field.FieldName];
                    var schoolExists = _dbContext.Schools.FirstOrDefault(p => p.Name.Equals(schoolName, StringComparison.OrdinalIgnoreCase));
                    if (schoolExists == null)
                    {
                        result.LogItems.Add(String.Format("The following student's record did not have a vaild School and is being skipped.  The school in the file was '{3}' for student: {0}, {1} - {2} \r\n", row["Student Last Name"], row["Student First Name"], row["Student Id"], row[field.FieldName]));
                        return false;
                    }
                    break;
                case "Bool":
                    if (!String.IsNullOrEmpty(row[field.FieldName]) && !"Y,N".Contains(row[field.FieldName]))
                    {
                        result.LogItems.Add(String.Format("You have assigned an invalid Is Interventionist Value of '{3}' and the record has been skipped for: {0}, {1} - {2} \r\n", row["Student Last Name"], row["Student First Name"], row["Student Id"], row[field.FieldName]));
                        return false;
                    }
                    break;
                case "Integer":
                    int i;
                    if (!String.IsNullOrEmpty(row[field.FieldName]) && !Int32.TryParse(row[field.FieldName], out i))
                    {
                        result.LogItems.Add(String.Format("You have assigned an invalid value for Graduation Year of '{3}' and the record has been skipped for: {0}, {1} - {2} \r\n", row["Student Last Name"], row["Student First Name"], row["Student Id"], row[field.FieldName]));
                        return false;
                    }
                    break;
                case "Date":
                    DateTime j;
                    if(DistrictId == 1)
                    {
                        if (!DateTime.TryParseExact(row[field.FieldName], "dd-MM-yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out j)) {
                            result.LogItems.Add(String.Format("You have assigned an invalid value for Student DOB of '{3}' and the record has been skipped for: {0}, {1} - {2} \r\n", row["Student Last Name"], row["Student First Name"], row["Student Id"], row[field.FieldName]));
                            return false;
                        }
                    }
                    else if(!DateTime.TryParse(row[field.FieldName], out j))
                    {
                        result.LogItems.Add(String.Format("You have assigned an invalid value for Student DOB of '{3}' and the record has been skipped for: {0}, {1} - {2} \r\n", row["Student Last Name"], row["Student First Name"], row["Student Id"], row[field.FieldName]));
                        return false;
                    }
                    break;
                case "StudentAttribute":
                    // get the lookup values for this field and make sure that they're all on the valid list
                    var attribute = field.FieldName;

                    // make sure this is a valid attribute
                    var db_attribute = _dbContext.StudentAttributeTypes.FirstOrDefault(p => p.AttributeName.Equals(attribute, StringComparison.OrdinalIgnoreCase));
                    var lookedUpValues = _dbContext.StudentAttributeLookupValues.Where(p => p.AttributeID == db_attribute.Id).Select(p => p.LookupValue).ToList();

                    // this attribute doesn't exist
                    if (db_attribute == null)
                    {
                        result.LogItems.Add(String.Format("You have tried to assign values for the student attribute '{3}' which does not exist and the record has been skipped for: {0}, {1} - {2} \r\n", row["Student Last Name"], row["Student First Name"], row["Student Id"], row[field.FieldName]));
                        return false;
                    } else
                    {
                        var attValues = row[field.FieldName];

                        // only student services can have more than one value
                        var splitAttributes = attValues.Split(Char.Parse(","));

                        if(splitAttributes.Length > 1 && !string.Equals("Student Services", attribute, StringComparison.OrdinalIgnoreCase))
                        {
                            result.LogItems.Add(String.Format("You have tried to assign multiple values for the student attribute '{3}' which only accepts a single value and the record has been skipped for: {0}, {1} - {2} \r\n", row["Student Last Name"], row["Student First Name"], row["Student Id"], row[field.FieldName]));
                            return false;
                        }
                        else if(string.Equals("Student Services", attribute, StringComparison.InvariantCultureIgnoreCase))
                        {
                            foreach(var att in splitAttributes)
                            {
                                if (!String.IsNullOrEmpty(att) && !lookedUpValues.Contains(att, StringComparer.OrdinalIgnoreCase))
                                {
                                    result.LogItems.Add(String.Format("You have tried to assign an invalid value of '{4}' for the student attribute '{3}' and the record has been skipped for: {0}, {1} - {2} \r\n", row["Student Last Name"], row["Student First Name"], row["Student Id"], field.FieldName, att));
                                    return false;
                                }
                            }
                        } else
                        {
                            // single valued attribute
                            if(!String.IsNullOrEmpty(attValues) && !lookedUpValues.Contains(attValues, StringComparer.OrdinalIgnoreCase))
                            {
                                result.LogItems.Add(String.Format("You have tried to assign an invalid value of '{4}' for the student attribute '{3}' and the record has been skipped for: {0}, {1} - {2} \r\n", row["Student Last Name"], row["Student First Name"], row["Student Id"], field.FieldName, attValues));
                                return false;
                            }
                        }
                    }
                    break;
            }
            return true;
        }

        public string GetRolloverFieldInsertString(AssessmentFieldTemplate field, Row row, OutputDto_Log result, DistrictContext _dbContext)
        {
            switch (field.FieldType)
            {
                case "TeacherAttribute":
                case "Text":
                case "Grade":
                case "School":
                    return String.Format("'{0}'", row[field.FieldName] == null ? String.Empty : row[field.FieldName].Replace("'", "''"));
                case "Bool":
                    return String.Format("{0}", row[field.FieldName] == "Y" ? "'Y'" : "'N'");
                case "Integer":
                    var currentGradYear = row[field.FieldName].ToNullableInt32();

                    if(currentGradYear != null)
                    {
                        return currentGradYear.ToString();
                    } else
                    {
                        // compute it based on grade
                        var gradeFactor = 0;
                        if (row.ColumnNames.FirstOrDefault(p => p == "Student Grade") != null)
                        {
                            if (row["Student Grade"] == "P")
                            {
                                gradeFactor = 13;
                            }
                            else if (row["Student Grade"] == "K")
                            {
                                gradeFactor = 12;
                            }
                            else
                            {
                                gradeFactor = 12 - (Int32.Parse(row["Student Grade"]));
                            }

                            currentGradYear = DateTime.Now.Month > 7 ? DateTime.Now.Year + gradeFactor + 1 : DateTime.Now.Year + gradeFactor;
                            return currentGradYear.ToString();
                        } else
                        {
                            return DateTime.Now.Year.ToString();
                        }
                    }
                case "Date":
                    if (DistrictId == 1)//ISD181 hack
                    {
                        return String.Format("'{0}'", row[field.FieldName].ToNullableDateExplictFormat("dd-MM-yyyy")?.ToShortDateString());
                    }
                    return String.Format("'{0}'", row[field.FieldName].ToNullableDate()?.ToShortDateString());
                default:
                    return "";
            }
        }

        public async Task ProcessInterventionTestDataJob(JobInterventionDataImport job, TextWriter output)
        {
            // now that we have dbcontext... can process stuff
            var result = new OutputDto_Log();
            job.Status = "Processing";
            _loginContext.SaveChanges();

            // get the MN prelim assessemnts
            // TODO:  pass in a flag that says prelim or not... hard code for now
            Assessment assessment = _dbContext.Assessments
                .Include(p => p.Fields)
                .First(p => p.Id == job.AssessmentId);

            var fields = ImportTestDataService.GetImportableColumns(assessment);
            var computedFields = ImportTestDataService.GetCalculatedColumns(assessment);

            // download file from azure for processing
            var manager = new AzurePhotoManager(CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString), NSConstants.Azure.AssessmentImportContainer);

            var textReaders = await manager.Download(job.UploadedFileUrl);
            var textReader = (textReaders.Count() == 1 ? textReaders.First() : null);


            var read = DataAccess.DataTable.New.Read(textReader);
            var columns = read.ColumnNames.ToList();

            foreach (var row in read.Rows)
            {

                try
                {

                    var recordIsValid = true;
                    var studentCode = row["STUDENT ID"];


                    // get columns that are on the list
                    var validFields = new List<AssessmentField>();
                    foreach (var column in columns)
                    {
                        foreach (var field in fields)
                        {
                            if (field.DisplayLabel.Equals(column, StringComparison.OrdinalIgnoreCase))
                            {
                                validFields.Add(field);

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
                    var student = _dbContext.Students.FirstOrDefault(p => p.StudentIdentifier == studentCode);

                    // TODO: check to see if user can access this student.. make a function of some sort in DB

                    // make sure that this student is in the target intervention group
                    var inGroup = _dbContext.StudentInterventionGroups.FirstOrDefault(p => p.InterventionGroupId == job.InterventionGroupId && p.StudentID == student.Id);

                    if (student == null)
                    {
                        result.LogItems.Add(String.Format("The following student was not found in the database: {0}, {1} - {2} \r\n", row["Student Last Name"], row["Student First Name"], studentCode));
                        continue; // skip this kid
                    }
                    else if (inGroup == null)
                    {
                        result.LogItems.Add(String.Format("The following student was not found in the intervention group you selected: {0}, {1} - {2} \r\n", row["Student Last Name"], row["Student First Name"], studentCode));
                        continue; // skip this kid
                    }
                    else
                    {
                        // now see if this kid already has a record in the table for this this tdd
                        var isInsert = ImportTestDataService.IsInterventionInsert(student.Id, job.InterventionGroupId, DateTime.Parse(row["Date Test Taken"]), assessment.StorageTable, _dbContext);
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
                                    // validate the required fields to ensure that they contain a value before adding to the script
                                    insertUpdateSql.AppendFormat(") VALUES ({0},{1},{2},'{3}'",
                                        student.Id,
                                        job.InterventionGroupId,
                                        job.RecorderId,
                                        DateTime.Parse(row["Date Test Taken"]));

                                    //for each
                                    foreach (var field in computedFields)
                                    {
                                        insertUpdateSql.AppendFormat(",{0}", ImportTestDataService.GetFieldInsertUpdateStringCalculatedFields(field, assessment, row, result, _dbContext)); // use the rest of the fields to calculate
                                    }

                                    foreach (var field in validFields)
                                    {
                                        insertUpdateSql.AppendFormat(",{0}", ImportTestDataService.GetFieldInsertUpdateString(field, row, result, _dbContext));
                                    }
                                    insertUpdateSql.AppendFormat(")");
                                }
                                else
                                {
                                    insertUpdateSql.AppendFormat("UPDATE {0} SET ", assessment.StorageTable);

                                    // update test_date
                                    insertUpdateSql.AppendFormat("{0} = '{1}',", "TestDueDate", DateTime.Parse(row["Date Test Taken"]));

                                    foreach (var field in computedFields)
                                    {
                                        insertUpdateSql.AppendFormat("{0} = {1},", field.DatabaseColumn, ImportTestDataService.GetFieldInsertUpdateStringCalculatedFields(field, assessment, row, result, _dbContext));
                                    }

                                    // don't include fields that we don't have fields for
                                    foreach (var field in validFields)
                                    {
                                        insertUpdateSql.AppendFormat("{0} = {1},", field.DatabaseColumn, ImportTestDataService.GetFieldInsertUpdateString(field, row, result, _dbContext));
                                    }
                                    // remove trailing comma
                                    insertUpdateSql.Remove(insertUpdateSql.Length - 1, 1);
                                    insertUpdateSql.AppendFormat(" WHERE StudentId = {0} AND InterventionGroupId = {1} and TestDueDate = '{2}'", student.Id, job.InterventionGroupId, DateTime.Parse(row["Date Test Taken"]));
                                }

                                command.CommandText = insertUpdateSql.ToString();
                                command.ExecuteNonQuery();

                                // update test due dates and gradeIds


                            }
                            catch (Exception ex)
                            {
                                //log
                                output.WriteLine("Error while processing Intervention Test Data batch: {0}", ex.Message);
                                job.Status = "Error";
                                job.EndDate = DateTime.Now;
                                job.ImportLog = ConvertLogToString(result);
                                _loginContext.SaveChanges();
                            }
                            finally
                            {
                                _dbContext.Database.Connection.Close();
                                command.Parameters.Clear();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    output.WriteLine("Outer Exception while processing Rollover Data batch: {0}", ex.Message);
                }
            }

            job.Status = "Complete";
            job.EndDate = DateTime.Now;
            job.ImportLog = ConvertLogToString(result);
            _loginContext.SaveChanges();
        }

        public class PrintType
        {
            public bool IsLocal { get; set; }
            public Winnovative.Document finalDocLocal { get; set; }
            public Winnovative.HtmlToPdfClient.Document finalDoc { get; set; }

            public Winnovative.HtmlToPdfClient.HtmlToPdfConverter pdfConverter { get; set; }
            public PdfConverter pdfConverterLocal { get; set; }
        }


        public void ProcessAssessmentDataExportJob(JobAssessmentDataExport job, TextWriter output)
        {
            // now that we have dbcontext... can process stuff
            var result = new OutputDto_Log();
            job.Status = "Processing";
            _loginContext.SaveChanges();

            var requestDetails = JsonConvert.DeserializeObject<InputDto_DataExportRequest>(job.SerializedRequest);

            //var rehydratedAssessments = Mapper.Map<List<Assessment>>(requestDetails.Assessments);

            var rehydratedAssessments = new List<Assessment>();



            requestDetails.Assessments.Each(p =>
            {
                // new
               // var dbAssessmentithFields = _dbContext.Assessments.First(z => z.Id == p.Id);

                var newAssessment = new Assessment();
                newAssessment.Id = p.Id;
                newAssessment.AssessmentName = p.AssessmentName;
                newAssessment.SortOrder = p.SortOrder;
                newAssessment.TestType = p.TestType;
                newAssessment.StorageTable = p.StorageTable;

                rehydratedAssessments.Add(newAssessment);

                var fields = new List<AssessmentField>();

                newAssessment.Fields = fields;

                // to include all fields
                //var allFields = assessmentWithFields.Fields.Where(g => !String.IsNullOrWhiteSpace(g.UniqueImportColumnName));

                p.Fields.Each(f =>
                {
                    var nf = new AssessmentField()
                    {
                        Id = f.Id,
                        Assessment = newAssessment,
                        AssessmentId = f.AssessmentId,
                        AltDisplayLabel = f.AltDisplayLabel,
                        FieldOrder = f.FieldOrder,
                        DisplayInEditResultList = f.DisplayInEditResultList,
                        DisplayInLineGraphs = f.DisplayInLineGraphs,
                        DisplayInLineGraphSummaryTable = f.DisplayInLineGraphSummaryTable,
                        DisplayInObsSummary = f.DisplayInObsSummary,
                        DatabaseColumn = f.DatabaseColumn,
                        CalculationFields = f.CalculationFields,
                        CategoryId = f.CategoryId,
                        SubcategoryId = f.SubcategoryId,
                        FieldType = f.FieldType,
                        LookupFieldName = f.LookupFieldName,
                        RangeHigh = f.RangeHigh,
                        RangeLow = f.RangeLow,
                        CalculationFunction = f.CalculationFunction,
                        GroupId = f.GroupId,
                        DisplayLabel = f.DisplayLabel,
                        DisplayInStackedBarGraphSummary = f.DisplayInStackedBarGraphSummary,
                        Description = f.Description,
                        IsRequired = f.IsRequired,
                        ImportColumnName = f.ImportColumnName,
                        ObsSummaryLabel = f.ObsSummaryLabel,
                        OutOfHowMany = f.OutOfHowMany,
                        Page = f.Page,
                        StorageTable = f.StorageTable,
                        UniqueImportColumnName = f.UniqueImportColumnName
                    };

                    newAssessment.Fields.Add(nf);
                });
            });

            // call and get OS results
            var scores = _dbContext.GetFilteredObservationSummaryData(rehydratedAssessments, requestDetails.ReportOptions, job.StaffId, true);

            var dataTable = ImportTestDataService.OSResultToDataTable(scores, rehydratedAssessments, _dbContext);
            var fileContents = Utility.DataTableToCSVString(dataTable);

            // upload to azure
            var manager = new AzurePhotoManager(CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString), NSConstants.Azure.AssessmentDataExportContainer);
            var fileName = "Staff_" + job.StaffId + "_" + Guid.NewGuid().ToString() + ".csv";
            var uri = manager.UploadTextToBlob(fileName, fileContents);

            job.UploadedFileName = fileName;
            job.UploadedFileUrl = uri.ToString();
            job.Status = "Complete";
            job.EndDate = DateTime.Now;
            job.RecordsProcessed = dataTable.Rows.Count;
            _loginContext.SaveChanges();
        }
        public void ProcessAllFieldsAssessmentDataExportJob(JobAllFieldsAssessmentDataExport job, TextWriter output)
        {
            // now that we have dbcontext... can process stuff
            var result = new OutputDto_Log();
            job.Status = "Processing";
            _loginContext.SaveChanges();

            var requestDetails = JsonConvert.DeserializeObject<InputDto_DataExportRequest>(job.SerializedRequest);

            //var rehydratedAssessments = Mapper.Map<List<Assessment>>(requestDetails.Assessments);

            var rehydratedAssessments = new List<Assessment>();



            requestDetails.Assessments.Each(p =>
            {
                // new
                var assessmentWithFields = _dbContext.Assessments.Include(n => n.Fields).First(z => z.Id == p.Id);

                var newAssessment = new Assessment();
                newAssessment.Id = p.Id;
                newAssessment.AssessmentName = p.AssessmentName;
                newAssessment.SortOrder = assessmentWithFields.SortOrder;
                newAssessment.TestType = assessmentWithFields.TestType;
                newAssessment.StorageTable = assessmentWithFields.StorageTable;

                rehydratedAssessments.Add(newAssessment);

                var fields = new List<AssessmentField>();

                newAssessment.Fields = fields;

                // to include all fields
                var allFields = assessmentWithFields.Fields.Where(g => !String.IsNullOrWhiteSpace(g.UniqueImportColumnName));

                allFields.Each(f =>
                {
                    var nf = new AssessmentField()
                    {
                        Id = f.Id,
                        Assessment = newAssessment,
                        AssessmentId = f.AssessmentId,
                        AltDisplayLabel = f.AltDisplayLabel,
                        FieldOrder = f.FieldOrder,
                        DisplayInEditResultList = f.DisplayInEditResultList,
                        DisplayInLineGraphs = f.DisplayInLineGraphs,
                        DisplayInLineGraphSummaryTable = f.DisplayInLineGraphSummaryTable,
                        DisplayInObsSummary = f.DisplayInObsSummary,
                        DatabaseColumn = f.DatabaseColumn,
                        CalculationFields = f.CalculationFields,
                        CategoryId = f.CategoryId,
                        SubcategoryId = f.SubcategoryId,
                        FieldType = f.FieldType,
                        LookupFieldName = f.LookupFieldName,
                        RangeHigh = f.RangeHigh,
                        RangeLow = f.RangeLow,
                        CalculationFunction = f.CalculationFunction,
                        GroupId = f.GroupId,
                        DisplayLabel = f.DisplayLabel,
                        DisplayInStackedBarGraphSummary = f.DisplayInStackedBarGraphSummary,
                        Description = f.Description,
                        IsRequired = f.IsRequired,
                        ImportColumnName = f.ImportColumnName,
                        ObsSummaryLabel = f.ObsSummaryLabel,
                        OutOfHowMany = f.OutOfHowMany,
                        Page = f.Page,
                        StorageTable = f.StorageTable,
                        UniqueImportColumnName = f.UniqueImportColumnName                    
                    };

                    newAssessment.Fields.Add(nf);
                });
            });

            // call and get OS results
            var scores = _dbContext.GetFilteredDataExportData(rehydratedAssessments, requestDetails.ReportOptions, job.StaffId, true);

            var dataTable = ImportTestDataService.AllFieldsResultToDataTable(scores, rehydratedAssessments, _dbContext);
            var fileContents = Utility.DataTableToCSVString(dataTable);

            // upload to azure
            var manager = new AzurePhotoManager(CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString), NSConstants.Azure.AssessmentDataExportContainer);
            var fileName = "Staff_" + job.StaffId + "_" + Guid.NewGuid().ToString() + ".csv";
            var uri = manager.UploadTextToBlob(fileName, fileContents);

            job.UploadedFileName = fileName;
            job.UploadedFileUrl = uri.ToString();
            job.Status = "Complete";
            job.EndDate = DateTime.Now;
            job.RecordsProcessed = dataTable.Rows.Count;
            _loginContext.SaveChanges();
        }

        public void ProcessInterventionDataExportJob(JobInterventionDataExport job, TextWriter output)
        {
            // now that we have dbcontext... can process stuff
            var result = new OutputDto_Log();
            job.Status = "Processing";
            _loginContext.SaveChanges();

            var requestDetails = JsonConvert.DeserializeObject<InputDto_DataExportRequest>(job.SerializedRequest);

            //var rehydratedAssessments = Mapper.Map<List<Assessment>>(requestDetails.Assessments);

            var rehydratedAssessments = _dbContext.Assessments.Where(p => p.Id == requestDetails.ReportOptions.InterventionAssessment.id).ToList();
            
            //requestDetails.Assessments.Each(p =>
            //{
            //    var newAssessment = new Assessment();
            //    newAssessment.Id = p.Id;
            //    newAssessment.AssessmentName = p.AssessmentName;
            //    newAssessment.SortOrder = p.SortOrder;
            //    newAssessment.TestType = p.TestType;
            //    newAssessment.StorageTable = p.StorageTable;

            //    rehydratedAssessments.Add(newAssessment);

            //    var fields = new List<AssessmentField>();

            //    newAssessment.Fields = fields;

            //    p.Fields.Each(f =>
            //    {
            //        var nf = new AssessmentField()
            //        {
            //            Id = f.Id,
            //            Assessment = newAssessment,
            //            AssessmentId = f.AssessmentId,
            //            AltDisplayLabel = f.AltDisplayLabel,
            //            FieldOrder = f.FieldOrder,
            //            DisplayInEditResultList = f.DisplayInEditResultList,
            //            DisplayInLineGraphs = f.DisplayInLineGraphs,
            //            DisplayInLineGraphSummaryTable = f.DisplayInLineGraphSummaryTable,
            //            DisplayInObsSummary = f.DisplayInObsSummary,
            //            DatabaseColumn = f.DatabaseColumn,
            //            CalculationFields = f.CalculationFields,
            //            CategoryId = f.CategoryId,
            //            SubcategoryId = f.SubcategoryId,
            //            FieldType = f.FieldType,
            //            LookupFieldName = f.LookupFieldName,
            //            RangeHigh = f.RangeHigh,
            //            RangeLow = f.RangeLow,
            //            CalculationFunction = f.CalculationFunction,
            //            GroupId = f.GroupId,
            //            DisplayLabel = f.DisplayLabel,
            //            DisplayInStackedBarGraphSummary = f.DisplayInStackedBarGraphSummary,
            //            Description = f.Description,
            //            IsRequired = f.IsRequired,
            //            ImportColumnName = f.ImportColumnName,
            //            ObsSummaryLabel = f.ObsSummaryLabel,
            //            OutOfHowMany = f.OutOfHowMany,
            //            Page = f.Page,
            //            StorageTable = f.StorageTable
            //        };

            //        newAssessment.Fields.Add(nf);
            //    });
            //});

            // call and get OS results
            var scores = _dbContext.GetFilteredInterventionSummaryData(rehydratedAssessments, requestDetails.ReportOptions, job.StaffId);

            var dataTable = ImportTestDataService.InterventionDataResultToDataTable(scores, rehydratedAssessments, _dbContext);
            var fileContents = Utility.DataTableToCSVString(dataTable);

            // upload to azure
            var manager = new AzurePhotoManager(CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString), NSConstants.Azure.AssessmentDataExportContainer);
            var fileName = "Staff_" + job.StaffId + "_" + Guid.NewGuid().ToString() + ".csv";
            var uri = manager.UploadTextToBlob(fileName, fileContents);

            job.UploadedFileName = fileName;
            job.UploadedFileUrl = uri.ToString();
            job.Status = "Complete";
            job.EndDate = DateTime.Now;
            job.RecordsProcessed = dataTable.Rows.Count;
            _loginContext.SaveChanges();
        }

        public void ProcessPrintBatchJob(JobPrintBatch job, TextWriter output)
        {
            
            if (SiteUrlBase.StartsWith("http://192.168.") || SiteUrlBase.StartsWith("http://localhost"))
            {
                printType.IsLocal = true;
                printType.pdfConverterLocal = new PdfConverter();
            } else
            {
                printType.IsLocal = false;
                uint ServerPort = uint.Parse(ConfigurationManager.AppSettings["PdfServerPort"]);
                printType.pdfConverter = new Winnovative.HtmlToPdfClient.HtmlToPdfConverter(ConfigurationManager.AppSettings["PdfServerIp"], ServerPort);
                Console.WriteLine("Writing Out Print Server IP: " + printType.pdfConverter.Server);
                Console.WriteLine("Creating new PDF Converter at: " + ConfigurationManager.AppSettings["PdfServerIp"]);
            }

            // now that we have dbcontext... can process stuff
            var result = new OutputDto_Log();
            job.Status = "Processing";
            _loginContext.SaveChanges();

            var requestDetails = JsonConvert.DeserializeObject<InputDto_BatchPrintRequest>(job.SerializedRequest);

            //var rehydratedAssessments = Mapper.Map<List<Assessment>>(requestDetails.Assessments);

            var rehydratedAssessments = new List<AssessmentDto>();

            var response = RequestToken(job.StaffEmail);
            //ShowResponse(response);

            if (printType.IsLocal)
            {
                printType.pdfConverterLocal.HttpRequestCookies.Add("Authorization", "Bearer " + response.AccessToken);
                printType.pdfConverterLocal.JavaScriptEnabled = true;
                printType.pdfConverterLocal.LicenseKey = "jgAQARABExEVARAVDxEBEhAPEBMPGBgYGAER";
            } else
            {
                printType.pdfConverter.HttpRequestCookies.Add("Authorization", "Bearer " + response.AccessToken);
                printType.pdfConverter.JavaScriptEnabled = true;
                printType.pdfConverter.LicenseKey = "jgAQARABExEVARAVDxEBEhAPEBMPGBgYGAER";
                printType.pdfConverter.NavigationTimeout = 300;
            }

            // Get list of sections based on schools, grades, teachers, sections
            var sections = GetSectionsToProcess(requestDetails.ReportOptions);

            // get students based on schools, grades, teachers, sections, attributes, zones (new!)
            var students = GetStudentsToProcess(requestDetails.Assessments, requestDetails.ReportOptions);
            var currentUrl = string.Empty;
            var benchmarkDate = _dbContext.TestDueDates.First(p => p.Id == requestDetails.ReportOptions.TestDueDateID);
            var schoolYear = benchmarkDate.SchoolStartYear.HasValue ? benchmarkDate.SchoolStartYear.Value : DateTime.Now.Year; // should never happen... ever
            var encodedHfwRanges = Uri.EscapeUriString(JsonConvert.SerializeObject(requestDetails.ReportOptions.HfwPages));

            QueryStringHelper qHelper = null;
            foreach (var classReport in requestDetails.ReportOptions.PageTypes)
            {
                // process any class reports if there are any specified by page types
                foreach (var sectionId in sections)
                {
                    // get section details
                    var section = _dbContext.Sections.First(p => p.Id == sectionId);
                    var encodedSchoolYear = Uri.EscapeUriString(JsonConvert.SerializeObject(new { id = section.SchoolStartYear, text = section.SchoolYear.YearVerbose }));
                    var encodedSchool = Uri.EscapeUriString(JsonConvert.SerializeObject(new { id = section.SchoolID, text = section.School.Name }));
                    var encodedTeacher = Uri.EscapeUriString(JsonConvert.SerializeObject(new { id = section.StaffID, text = section.Staff.FullName }));
                    var encodedGrade = Uri.EscapeUriString(JsonConvert.SerializeObject(new { id = section.GradeID, text = section.Grade.ShortName }));
                    var encodedSection = Uri.EscapeUriString(JsonConvert.SerializeObject(new { id = sectionId, text = section.Name }));
                    var encodedBenchmarkDate = Uri.EscapeUriString(JsonConvert.SerializeObject(new { id = benchmarkDate.Id, text = benchmarkDate.DisplayDate }));

                    switch (classReport.text)
                    {
                        case "Class Reports":
                            foreach (var assessment in requestDetails.Assessments)
                            {
                                // don't do HFW
                                if (assessment.BaseType != NSAssessmentBaseType.HighFrequencyWords)
                                {
                                    var reportUrl = assessment.ClassReportPages;
                                    // get the first page, if there is one
                                    var multiReportSplit = assessment.ClassReportPages == null ? new string[] { } : assessment.ClassReportPages.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);

                                    // these are just the class reports
                                    foreach (var splitString in multiReportSplit)
                                    {
                                        // see if the report has a custom name or not
                                        var customNameSplit = splitString.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

                                        // if it has a custom name, it is the first item in the array
                                        if (customNameSplit.Length == 2)
                                        {
                                            currentUrl = SiteUrlBase + "/#/" + customNameSplit[1] + "/" + assessment.Id;
                                            qHelper = new QueryStringHelper(currentUrl);
                                            qHelper.SetParameter("SchoolYear", encodedSchoolYear);
                                            qHelper.SetParameter("School", encodedSchool);
                                            qHelper.SetParameter("Teacher", encodedTeacher);
                                            qHelper.SetParameter("Grade", encodedGrade);
                                            qHelper.SetParameter("Section", encodedSection);
                                            qHelper.SetParameter("BenchmarkDate", encodedBenchmarkDate);
                                            qHelper.SetParameter("printmode", "1");
                                            qHelper.SetParameter("batchprint", "1");

                                            // go print this URL with these section settings
                                            PDFGen(qHelper.All);
                                        }
                                        else if (customNameSplit.Length == 1)
                                        {
                                            currentUrl = SiteUrlBase + "/#/" + customNameSplit[0] + "/" + assessment.Id;
                                            qHelper = new QueryStringHelper(currentUrl);
                                            qHelper.SetParameter("SchoolYear", encodedSchoolYear);
                                            qHelper.SetParameter("School", encodedSchool);
                                            qHelper.SetParameter("Teacher", encodedTeacher);
                                            qHelper.SetParameter("Grade", encodedGrade);
                                            qHelper.SetParameter("Section", encodedSection);
                                            qHelper.SetParameter("BenchmarkDate", encodedBenchmarkDate);
                                            qHelper.SetParameter("printmode", "1");
                                            qHelper.SetParameter("batchprint", "1");

                                            // go print this URL with these section settings
                                            PDFGen(qHelper.All);
                                        }
                                    }
                                }

                            }
                            break;
                        case "Classroom Dashboard":
                            currentUrl = SiteUrlBase + "/#/observation-summary-class";
                            qHelper = new QueryStringHelper(currentUrl);
                            qHelper.SetParameter("SchoolYear", encodedSchoolYear);
                            qHelper.SetParameter("School", encodedSchool);
                            qHelper.SetParameter("Teacher", encodedTeacher);
                            qHelper.SetParameter("Grade", encodedGrade);
                            qHelper.SetParameter("Section", encodedSection);
                            qHelper.SetParameter("BenchmarkDate", encodedBenchmarkDate);
                            qHelper.SetParameter("printmode", "1");
                            qHelper.SetParameter("batchprint", "1");

                            // go print this URL with these section settings
                            PDFGen(qHelper.All);
                            break;
                    }
                }


                // 11/11/2020
                // if we don't have any student selected, see if we have any sections and get the kids from those sections
                if(students.Count == 0)
                {
                    //var tempStudentList = new List<int>();
                    if(sections.Count > 0)
                    {
                        foreach(var section in sections)
                        {
                            students.AddRange(_dbContext.StudentSections.Where(p => p.ClassID == section).Select(j => j.StudentID).Distinct().ToList());
                        }
                    }
                }

                foreach (var studentId in students)
                {
                    // get student details
                    var student = _dbContext.Students.First(p => p.Id == studentId);
                    var encodedStudent = Uri.EscapeUriString(JsonConvert.SerializeObject(new { id = studentId, text = student.LastName + ", " + student.FirstName }));

                    switch (classReport.text)
                    {
                        case "Student Data Entry Screens":
                            foreach (var assessment in requestDetails.Assessments)
                            {
                                var dataEntryPageUrl = assessment.DefaultDataEntryPage ?? "section-dataentry-generic";

                                // don't do HFW the same way
                                if (assessment.BaseType != NSAssessmentBaseType.HighFrequencyWords)
                                {
                                    var assessmentResult = _dbContext.Database.SqlQuery<StudentDataEntryResultInfo>(String.Format("SELECT TOP 1 * FROM {0} WHERE StudentId = {1} and TestDueDateID = {2}", assessment.StorageTable, studentId, benchmarkDate.Id)).FirstOrDefault();

                                    // don't print kids with no results
                                    if (assessmentResult != null)
                                    {
                                        currentUrl = SiteUrlBase + "/#/" + dataEntryPageUrl + "/" + assessment.Id + "/" + assessmentResult.SectionID + "/" + benchmarkDate.Id + "/" + studentId + "/" + assessmentResult.ID;
                                        qHelper = new QueryStringHelper(currentUrl);
                                        qHelper.SetParameter("printmode", "1");
                                        qHelper.SetParameter("batchprint", "1");

                                        // go print this URL with these section settings
                                        PDFGen(qHelper.All);
                                    }
                                }
                                else
                                {
                                    // print all the page ranges for HFW
                                    var assessmentResult = _dbContext.Database.SqlQuery<StudentDataEntryResultInfo>(String.Format("SELECT TOP 1 * FROM {0} WHERE StudentId = {1}", assessment.StorageTable, studentId)).FirstOrDefault();

                                    foreach (var wordRange in requestDetails.ReportOptions.HfwPages)
                                    {

                                        // don't print kids with no results
                                        if (assessmentResult != null)
                                        {
                                            currentUrl = SiteUrlBase + "/#/" + dataEntryPageUrl + "/" + assessment.Id + "/" + assessmentResult.SectionID + "/" + benchmarkDate.Id + "/" + studentId + "/" + assessmentResult.ID + "/" + wordRange.text;
                                            qHelper = new QueryStringHelper(currentUrl);
                                            qHelper.SetParameter("printmode", "1");
                                            qHelper.SetParameter("batchprint", "1");

                                            // go print this URL with these section settings
                                            PDFGen(qHelper.All);
                                        }
                                    }
                                }

                            }
                            break;
                        case "Student Dashboard":
                            currentUrl = SiteUrlBase + "/#/student-dashboard-printall/" + studentId;
                            qHelper = new QueryStringHelper(currentUrl);
                            qHelper.SetParameter("printmode", "1");
                            qHelper.SetParameter("batchprint", "1");

                            // go print this URL with these section settings
                            PDFGen(qHelper.All);
                            break;
                        case "HFW Detailed Student Report":
                            var studentSection = _dbContext.StudentSections
                                .Include(p => p.Section)
                                .Include(p => p.Section.Grade)
                                .Include(p => p.Section.Staff)
                                .Include(p => p.Section.School)
                                .FirstOrDefault(p => p.StudentID == studentId && p.Section.SchoolStartYear == schoolYear);

                            currentUrl = SiteUrlBase + "/#/hfw-detail/";
                            qHelper = new QueryStringHelper(currentUrl);
                            qHelper.SetParameter("printmode", "1");
                            qHelper.SetParameter("HfwRanges", encodedHfwRanges);

                            if (studentSection != null)
                            {
                                qHelper.SetParameter("Teacher", Uri.EscapeUriString(JsonConvert.SerializeObject(new { id = studentSection.Section.Staff.Id, text = studentSection.Section.Staff.FullName })));
                                qHelper.SetParameter("Grade", Uri.EscapeUriString(JsonConvert.SerializeObject(new { id = studentSection.Section.Grade.Id, text = studentSection.Section.Grade.ShortName })));
                                qHelper.SetParameter("Section", Uri.EscapeUriString(JsonConvert.SerializeObject(new { id = studentSection.Section.Id, text = studentSection.Section.Name })));
                                qHelper.SetParameter("School", Uri.EscapeUriString(JsonConvert.SerializeObject(new { id = studentSection.Section.School.Id, text = studentSection.Section.School.Name })));
                                qHelper.SetParameter("SchoolYear", Uri.EscapeUriString(JsonConvert.SerializeObject(new { id = studentSection.Section.SchoolStartYear, text = studentSection.Section.SchoolYear.YearVerbose })));
                            }

                          
                            qHelper.SetParameter("Student", encodedStudent);
                            qHelper.SetParameter("batchprint", "1");

                            // go print this URL with these section settings
                            PDFGen(qHelper.All);
                            break;
                        case "HFW Missing Words Report":
                            currentUrl = SiteUrlBase + "/#/hfw-missing-words/";
                            qHelper = new QueryStringHelper(currentUrl);
                            qHelper.SetParameter("printmode", "1");
                            qHelper.SetParameter("batchprint", "1");
                            qHelper.SetParameter("HfwRanges", encodedHfwRanges);
                            qHelper.SetParameter("Student", encodedStudent);

                            // go print this URL with these section settings
                            PDFGen(qHelper.All);
                            break;
                        case "Student Line Graphs":
                            // for each assessment, get all of the benchmarked fields that this student has data for and draw
                            var finalList = new List<AssessmentDto>();
                            var assessmentFields = new List<AssessmentField>();
                            foreach (var assessment in requestDetails.Assessments)
                            {
                                using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
                                {
                                    try
                                    {
                                        // see if the student has data for this assessment
                                        command.CommandText = String.Format("Select * FROM {0} WHERE StudentID = {1}", assessment.StorageTable, studentId.ToString());
                                        command.CommandTimeout = command.Connection.ConnectionTimeout;
                                        _dbContext.Database.Connection.Open();


                                        using (System.Data.IDataReader reader = command.ExecuteReader())
                                        {

                                            if (((System.Data.SqlClient.SqlDataReader)(reader)).HasRows)
                                            {
                                                finalList.Add(assessment);
                                            }
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Error("Exception encountered while getting student line graph fields for Print Batch: {0}", ex.Message);
                                    }
                                    finally
                                    {
                                        _dbContext.Database.Connection.Close();
                                        command.Parameters.Clear();
                                    }
                                }
                            }
                            // now find which fields that can be displayed in line graphs are benchmarked
                            foreach (var asmnt in finalList)
                            {
                                var benchmarkedFieldNames = _dbContext.DistrictBenchmarks.Where(p => p.AssessmentID == asmnt.Id).Select(p => p.AssessmentField).ToList();
                                assessmentFields.AddRange(_dbContext.AssessmentFields.Include(p => p.Assessment).Where(p => p.AssessmentId == asmnt.Id && p.DisplayInLineGraphs == true && benchmarkedFieldNames.Contains(p.DatabaseColumn)).ToList());
                            }

                            List<AssessmentFieldDto> dtoFields = Mapper.Map<List<AssessmentFieldDto>>(assessmentFields);

                            foreach (var field in dtoFields)
                            {
                                var encodedField = Uri.EscapeUriString(JsonConvert.SerializeObject(field));

                                currentUrl = SiteUrlBase + "/#/linegraph";
                                qHelper = new QueryStringHelper(currentUrl);
                                qHelper.SetParameter("printmode", "1");
                                qHelper.SetParameter("batchprint", "1");
                                qHelper.SetParameter("Student", encodedStudent);
                                qHelper.SetParameter("AssessmentField", encodedField);

                                // go print this URL with these section settings
                                PDFGen(qHelper.All);

                            }

                            break;
                    }
                }
            }
            // process any student-level reports if specified... get data entry URL for based on assessment



            //finalDoc.Save((string.Format("{0}.pdf", ConfigurationManager.AppSettings["pdfPath"] + "\\batch_" + printbatch.batchguid));
            byte[] savedBytes = null;

            if (printType.IsLocal)
            {
                savedBytes = printType.finalDocLocal.Save();
                printType.finalDocLocal.DetachStream();
                printType.finalDocLocal.Close();
            } else
            {
                savedBytes = printType.finalDoc.Save();
            }

            var streamOfBytes = new MemoryStream(savedBytes);
            // where to get individual page print settings?  a.  make a table with URL that has all the print settings... eventually, update directive to retrieve these

            // for assessments that don't have a dedicated data entry screen... make a separate page that has all the fields listed vertically... WV and FP... comments listed

            // upload to azure
            var manager = new AzurePhotoManager(CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString), NSConstants.Azure.AssessmentDataExportContainer);
            var fileName = "PrintBatch_" + job.StaffId + "_" + Guid.NewGuid().ToString() + ".pdf";
            var uri = manager.UploadBinaryDataToBlob(fileName, streamOfBytes);

            job.UploadedFileName = fileName;
            job.UploadedFileUrl = uri.ToString();
            job.Status = "Complete";
            job.EndDate = DateTime.Now;
            _loginContext.SaveChanges();
        }

        private void PDFGen(string url)
        {
            var input = _loginContext.PrintSettings.FirstOrDefault(p => url.Contains(p.Url));

            if (printType.IsLocal)
            {
                if (input != null)
                {
                    printType.pdfConverterLocal.PdfDocumentOptions.SinglePage = !(input.PrintMultiPage ?? false);
                    printType.pdfConverterLocal.ConversionDelay = input.ConversionDelay ?? 5;

                    if (input.PrintMultiPage != true)
                    {
                        printType.pdfConverterLocal.PdfDocumentOptions.FitWidth = input.FitWidth ?? true;
                        printType.pdfConverterLocal.PdfDocumentOptions.FitHeight = input.FitHeight ?? true;
                        printType.pdfConverterLocal.PdfDocumentOptions.StretchToFit = input.StretchToFit ?? true;
                    }
                    else
                    {
                        printType.pdfConverterLocal.PdfDocumentOptions.FitWidth = input.FitWidth ?? false;
                        printType.pdfConverterLocal.PdfDocumentOptions.FitHeight = input.FitHeight ?? false;
                        printType.pdfConverterLocal.PdfDocumentOptions.StretchToFit = input.StretchToFit ?? false;
                    }

                    if (input.PrintLandscape == true)
                    {
                        printType.pdfConverterLocal.HtmlViewerWidth = input.HtmlViewerWidth ?? 1950;
                    }
                    else
                    {
                        printType.pdfConverterLocal.HtmlViewerWidth = input.HtmlViewerWidth ?? 1300;
                        if (input.HtmlViewerHeight != null)
                        {
                            printType.pdfConverterLocal.HtmlViewerHeight = input.HtmlViewerHeight.Value;
                        }
                    }

                    if (input.ForcePortraitPageSize == true)
                    {
                        printType.pdfConverterLocal.PdfDocumentOptions.PdfPageSize = new Winnovative.PdfPageSize(612, 1350);
                    }

                    printType.pdfConverterLocal.PdfDocumentOptions.PdfPageOrientation = input.PrintLandscape == true ? Winnovative.PdfPageOrientation.Landscape : Winnovative.PdfPageOrientation.Portrait;
                }
                else
                {
                    // default settings if none specified
                    printType.pdfConverterLocal.PdfDocumentOptions.PdfPageOrientation = Winnovative.PdfPageOrientation.Landscape;
                    printType.pdfConverterLocal.HtmlViewerWidth = 1950;
                    printType.pdfConverterLocal.ConversionDelay = 5;
                }

                printType.pdfConverterLocal.PdfDocumentOptions.ShowHeader = false;
                printType.pdfConverterLocal.PdfDocumentOptions.ShowFooter = false;
                printType.pdfConverterLocal.PdfDocumentOptions.LeftMargin = 20;
                printType.pdfConverterLocal.PdfDocumentOptions.RightMargin = 20;
                printType.pdfConverterLocal.PdfDocumentOptions.TopMargin = 20;
                printType.pdfConverterLocal.PdfDocumentOptions.BottomMargin = 20;
                printType.pdfConverterLocal.PdfDocumentOptions.JpegCompressionEnabled = false;
                printType.pdfConverterLocal.PdfDocumentOptions.JpegCompressionLevel = 0;
                Winnovative.Document pdfDocument = printType.pdfConverterLocal.GetPdfDocumentObjectFromUrl(url);

                // first document becomes doc
                if (printType.finalDocLocal == null)
                {
                    printType.finalDocLocal = pdfDocument;
                }
                else
                {
                    printType.finalDocLocal.AppendDocument(pdfDocument);
                }
            } else
            {
                if (input != null)
                {
                    printType.pdfConverter.PdfDocumentOptions.SinglePage = !(input.PrintMultiPage ?? false);
                    printType.pdfConverter.ConversionDelay = input.ConversionDelay ?? 5;

                    if (input.PrintMultiPage != true)
                    {
                        printType.pdfConverter.PdfDocumentOptions.FitWidth = input.FitWidth ?? true;
                        printType.pdfConverter.PdfDocumentOptions.FitHeight = input.FitHeight ?? true;
                        printType.pdfConverter.PdfDocumentOptions.StretchToFit = input.StretchToFit ?? true;
                    }
                    else
                    {
                        printType.pdfConverter.PdfDocumentOptions.FitWidth = input.FitWidth ?? false;
                        printType.pdfConverter.PdfDocumentOptions.FitHeight = input.FitHeight ?? false;
                        printType.pdfConverter.PdfDocumentOptions.StretchToFit = input.StretchToFit ?? false;
                    }

                    if (input.PrintLandscape == true)
                    {
                        printType.pdfConverter.HtmlViewerWidth = input.HtmlViewerWidth ?? 1950;
                    }
                    else
                    {
                        printType.pdfConverter.HtmlViewerWidth = input.HtmlViewerWidth ?? 1300;
                        if (input.HtmlViewerHeight != null)
                        {
                            printType.pdfConverter.HtmlViewerHeight = input.HtmlViewerHeight.Value;
                        }
                    }

                    if (input.ForcePortraitPageSize == true)
                    {
                        printType.pdfConverter.PdfDocumentOptions.PdfPageSize = new Winnovative.HtmlToPdfClient.PdfPageSize(612, 1350);
                    }

                    printType.pdfConverter.PdfDocumentOptions.PdfPageOrientation = input.PrintLandscape == true ? Winnovative.HtmlToPdfClient.PdfPageOrientation.Landscape : Winnovative.HtmlToPdfClient.PdfPageOrientation.Portrait;
                }
                else
                {
                    // default settings if none specified
                    printType.pdfConverter.PdfDocumentOptions.PdfPageOrientation = Winnovative.HtmlToPdfClient.PdfPageOrientation.Landscape;
                    printType.pdfConverter.HtmlViewerWidth = 1950;
                    printType.pdfConverter.ConversionDelay = 5;
                }

                printType.pdfConverter.PdfDocumentOptions.ShowHeader = false;
                printType.pdfConverter.PdfDocumentOptions.ShowFooter = false;
                printType.pdfConverter.PdfDocumentOptions.LeftMargin = 20;
                printType.pdfConverter.PdfDocumentOptions.RightMargin = 20;
                printType.pdfConverter.PdfDocumentOptions.TopMargin = 20;
                printType.pdfConverter.PdfDocumentOptions.BottomMargin = 20;
                printType.pdfConverter.PdfDocumentOptions.JpegCompressionEnabled = false;
                printType.pdfConverter.PdfDocumentOptions.JpegCompressionLevel = 0;
                Console.WriteLine("About to call ConvertURL on Address:" + url);
                Console.WriteLine("Writing Out Print Server IP: " + printType.pdfConverter.Server);
                byte[] pdfBytes = printType.pdfConverter.ConvertUrl(url);
                Console.WriteLine("Didn't Die after calling ConvertURL");
                // first document becomes doc
                if (printType.finalDoc == null)
                {
                    Console.WriteLine("Final Doc is NULL and creating new Final Doc");
                    printType.finalDoc = new Winnovative.HtmlToPdfClient.Document(ConfigurationManager.AppSettings["PdfServerIp"], uint.Parse(ConfigurationManager.AppSettings["PdfServerPort"]), pdfBytes);
                    printType.finalDoc.LicenseKey = "jgAQARABExEVARAVDxEBEhAPEBMPGBgYGAER";
                    Console.WriteLine("New Final Doc Created");
                }
                else
                {
                    Console.WriteLine("Final Doc is NOT Null and Appending");
                    printType.finalDoc.AppendDocument(pdfBytes);
                    Console.WriteLine("Appended To FInal DOc");
                }
            }
        }

        

        private List<int> GetStudentsToProcess(List<AssessmentDto> assessments, InputDto_GetFilteredPrintBatchOptions input)
        {
            var studentIds = new List<int>();

            string commaTerminatedAssessments = string.Empty;
            commaTerminatedAssessments = string.Join(",", assessments.Select(p => p.StorageTable).ToArray()) + ",";

            // now intersect current list of student ids from db with explicitly-add student IDs... this shouldn't
            // happen very often, but we at least need to handle it
            var explictStudentIds = input.Students.Select(p => p.id).ToList();

            if(explictStudentIds.Count > 0)
            {
                return explictStudentIds;
            }

//            studentIds = studentIds.Intersect(explictStudentIds).Distinct().ToList();

            // calculate benchmark value and return
            using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
            {
                command.CommandTimeout = 45;
                try
                {
                    
                    Dictionary<int, List<int>> attributeDictionary = new Dictionary<int, List<int>>();
                    var attributeParameterArray = new SqlParameter[10];
                    var attributeIdParameterArray = new SqlParameter[10];

                    // hackfix, empty params
                    for (var i = 0; i < 10; i++)
                    {
                        attributeParameterArray[i] = new SqlParameter("StudentAttributeValues" + i, DBNull.Value);
                        attributeIdParameterArray[i] = new SqlParameter("StudentAttributeIdValue" + i, DBNull.Value);
                    }

                    // combine the student attributevalues into a single list
                    List<string> attributeValues = new List<string>();
                    foreach (var attributeType in input.DropdownDataList)
                    {
                        // hackfix, and yes, I did regret it... thanks jerkface
                        attributeParameterArray[attributeDictionary.Count].Value = string.Join<int>(",", attributeType.DropDownData.Select(p => p.id).ToList());
                        attributeIdParameterArray[attributeDictionary.Count].Value = attributeType.AttributeTypeId;

                        attributeDictionary.Add(attributeType.AttributeTypeId, attributeType.DropDownData.Select(p => p.id).ToList());
                    }

                    // call stored procedure and pass parameters
                    _dbContext.Database.Connection.Open();
                    command.CommandText = @"EXEC dbo.ns4_ReturnFilteredStudents @TableNames, @TestDueDateID, @SchoolStartYear, 
                        @Schools, @Grades, @Teachers, @Sections,
                        @StudentAttributeIdValue0, @StudentAttributeValues0,
                        @StudentAttributeIdValue1, @StudentAttributeValues1,
                        @StudentAttributeIdValue2, @StudentAttributeValues2,
                        @StudentAttributeIdValue3, @StudentAttributeValues3,
                        @StudentAttributeIdValue4, @StudentAttributeValues4,
                        @StudentAttributeIdValue5, @StudentAttributeValues5,
                        @StudentAttributeIdValue6, @StudentAttributeValues6,
                        @StudentAttributeIdValue7, @StudentAttributeValues7,
                        @StudentAttributeIdValue8, @StudentAttributeValues8,
                        @StudentAttributeIdValue9, @StudentAttributeValues9,
                         @InterventionTypes,@SpecialEd,@TeamMeeting,@TextLevelZones";//, commaTerminatedAssessments, DateTime.Now.ToShortDateString(), benchmarkDateId, classId);
                    command.Parameters.Add(new SqlParameter("TableNames", commaTerminatedAssessments));
                    command.Parameters.Add(new SqlParameter("TestDueDateID", SqlDbType.Int) { Value = (object)input.TestDueDateID ?? DBNull.Value });
                    command.Parameters.Add(new SqlParameter("SchoolStartYear", input.SchoolStartYear));
                    command.Parameters.Add(new SqlParameter("Schools", string.Join<int>(",", input.Schools.Select(p => p.id).ToList())));
                    command.Parameters.Add(new SqlParameter("Sections", string.Join<int>(",", input.Sections.Select(p => p.id).ToList())));
                    command.Parameters.Add(new SqlParameter("Grades", string.Join<int>(",", input.Grades.Select(p => p.id).ToList())));
                    command.Parameters.Add(new SqlParameter("Teachers", string.Join<int>(",", input.Teachers.Select(p => p.id).ToList())));
                    command.Parameters.Add(attributeParameterArray[0]);
                    command.Parameters.Add(attributeIdParameterArray[0]);
                    command.Parameters.Add(attributeParameterArray[1]);
                    command.Parameters.Add(attributeIdParameterArray[1]);
                    command.Parameters.Add(attributeParameterArray[2]);
                    command.Parameters.Add(attributeIdParameterArray[2]);
                    command.Parameters.Add(attributeParameterArray[3]);
                    command.Parameters.Add(attributeIdParameterArray[3]);
                    command.Parameters.Add(attributeParameterArray[4]);
                    command.Parameters.Add(attributeIdParameterArray[4]);
                    command.Parameters.Add(attributeParameterArray[5]);
                    command.Parameters.Add(attributeIdParameterArray[5]);
                    command.Parameters.Add(attributeParameterArray[6]);
                    command.Parameters.Add(attributeIdParameterArray[6]);
                    command.Parameters.Add(attributeParameterArray[7]);
                    command.Parameters.Add(attributeIdParameterArray[7]);
                    command.Parameters.Add(attributeParameterArray[8]);
                    command.Parameters.Add(attributeIdParameterArray[8]);
                    command.Parameters.Add(attributeParameterArray[9]);
                    command.Parameters.Add(attributeIdParameterArray[9]);
                    command.Parameters.Add(new SqlParameter("@InterventionTypes", string.Join<int>(",", input.InterventionTypes.Select(p => p.id).ToList())));
                    command.Parameters.Add(new SqlParameter("SpecialEd", SqlDbType.VarChar) { Value = (object)input.SpecialEd?.id ?? DBNull.Value });
                    command.Parameters.Add(new SqlParameter("TeamMeeting", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("TextLevelZones", string.Join<int>(",", input.TargetLevelZones.Select(p => p.id).ToList())));
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 45;

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        // load datatable
                        System.Data.DataTable dt = new System.Data.DataTable();
                        dt.Load(reader);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            studentIds.Add(Int32.Parse(dt.Rows[i]["StudentId"].ToString()));
                        }
                    }

                }
                catch (Exception ex)
                {
                    Log.Error("Error loading students for printing: {0}", ex.Message);
                }
                finally
                {
                    _dbContext.Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }



            return studentIds;
        }

        private List<int> GetSectionsToProcess(InputDto_GetFilteredPrintBatchOptions input)
        {
            var sectionIds = new List<int>();

            // get sections
            if(input.Sections != null && input.Sections.Count() > 0)
            {
                sectionIds = input.Sections.Select(p => p.id).ToList();
            } else if(input.Teachers != null && input.Teachers.Count() > 0) // teachers
            {
                var teacherIds = input.Teachers.Select(p => p.id).ToList();
                sectionIds = (from t1 in _dbContext.StaffSections
                              join t2 in _dbContext.Sections on t1.ClassID equals t2.Id
                              where teacherIds.Any(teacher => t1.StaffID == teacher) && t2.SchoolStartYear == input.SchoolStartYear
                              select t2.Id).Distinct().ToList();
            } else if(input.Grades != null && input.Grades.Count() > 0) // grades
            {
                var gradeIds = input.Grades.Select(p => p.id).ToList();
                var schoolIds = input.Schools.Select(p => p.id).ToList();
                sectionIds = (from t1 in _dbContext.Sections
                              where gradeIds.Any(grade => t1.GradeID == grade) 
                              && schoolIds.Any(school => t1.SchoolID == school) 
                              && t1.SchoolStartYear == input.SchoolStartYear
                              select t1.Id).Distinct().ToList();

            }
            else if(input.Schools != null && input.Schools.Count() > 0) // schools
            {
                sectionIds = (from t1 in _dbContext.Sections
                              where input.Schools.Any(school => t1.SchoolID == school.id && t1.SchoolStartYear == input.SchoolStartYear)
                              select t1.Id).Distinct().ToList();
            }

            return sectionIds;
        }

        private TokenResponse RequestToken(string behalfOf)
        {
            var client = new OAuth2Client(
                new Uri(IdentityServerUrl + "/connect/token"),
                "roclient",
                "secret");

            // idsrv supports additional non-standard parameters 
            // that get passed through to the user service
            var optional = new Dictionary<string, string>
            {
                { "acr_values", behalfOf }
            };

            return client.RequestResourceOwnerPasswordAsync("northstar.shannon@gmail.com", "dammit", "idmgr", optional).Result;
        }

        private void ShowResponse(TokenResponse response)
        {
            if (!response.IsError)
            {
                "Token response:".ConsoleGreen();
                Console.WriteLine(response.Json);

                if (response.AccessToken.Contains("."))
                {
                    "\nAccess Token (decoded):".ConsoleGreen();

                    var parts = response.AccessToken.Split('.');
                    var header = parts[0];
                    var claims = parts[1];

                    Console.WriteLine(JObject.Parse(Encoding.UTF8.GetString(Base64Url.Decode(header))));
                    Console.WriteLine(JObject.Parse(Encoding.UTF8.GetString(Base64Url.Decode(claims))));
                }
            }
            else
            {
                if (response.IsHttpError)
                {
                    "HTTP error: ".ConsoleGreen();
                    Console.WriteLine(response.HttpErrorStatusCode);
                    "HTTP error reason: ".ConsoleGreen();
                    Console.WriteLine(response.HttpErrorReason);
                }
                else
                {
                    "Protocol error response:".ConsoleGreen();
                    Console.WriteLine(response.Json);
                }
            }
        }


        public void ProcessAttendanceDataExportJob(JobAttendanceExport job, TextWriter output)
        {
            // now that we have dbcontext... can process stuff
            var result = new OutputDto_Log();
            job.Status = "Processing";
            _loginContext.SaveChanges();

            // call and get OS results
            var dataTable = _dbContext.GetDistrictAttendance(job.SchoolStartYear);
            var fileContents = Utility.DataTableToCSVString(dataTable);

            // upload to azure
            var manager = new AzurePhotoManager(CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString), NSConstants.Azure.AssessmentDataExportContainer);
            var fileName = "Staff_Attendance_" + job.StaffId + "_" + Guid.NewGuid().ToString() + ".csv";
            var uri = manager.UploadTextToBlob(fileName, fileContents);

            job.UploadedFileName = fileName;
            job.UploadedFileUrl = uri.ToString();
            job.Status = "Complete";
            job.EndDate = DateTime.Now;
            job.RecordsProcessed = dataTable.Rows.Count;
            _loginContext.SaveChanges();
        }

        public void ProcessStudentExportJob(JobStudentExport job, TextWriter output)
        {
            // now that we have dbcontext... can process stuff
            var result = new OutputDto_Log();
            job.Status = "Processing";
            _loginContext.SaveChanges();

            // call and get OS results
            var dataTable = _dbContext.GetStudentExportInfo();
            var fileContents = Utility.DataTableToCSVString(dataTable);

            // upload to azure
            var manager = new AzurePhotoManager(CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString), NSConstants.Azure.AssessmentDataExportContainer);
            var fileName = "StudentExport_" + job.StaffId + "_" + Guid.NewGuid().ToString() + ".csv";
            var uri = manager.UploadTextToBlob(fileName, fileContents);

            job.UploadedFileName = fileName;
            job.UploadedFileUrl = uri.ToString();
            job.Status = "Complete";
            job.EndDate = DateTime.Now;
            job.RecordsProcessed = dataTable.Rows.Count;
            _loginContext.SaveChanges();
        }


        public void ProcessStaffExportJob(JobStaffExport job, TextWriter output)
        {
            // now that we have dbcontext... can process stuff
            var result = new OutputDto_Log();
            job.Status = "Processing";
            _loginContext.SaveChanges();

            // call and get OS results
            var dataTable = _dbContext.GetStaffExportInfo();
            var fileContents = Utility.DataTableToCSVString(dataTable);

            // upload to azure
            var manager = new AzurePhotoManager(CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString), NSConstants.Azure.AssessmentDataExportContainer);
            var fileName = "StaffExport" + job.StaffId + "_" + Guid.NewGuid().ToString() + ".csv";
            var uri = manager.UploadTextToBlob(fileName, fileContents);

            job.UploadedFileName = fileName;
            job.UploadedFileUrl = uri.ToString();
            job.Status = "Complete";
            job.EndDate = DateTime.Now;
            job.RecordsProcessed = dataTable.Rows.Count;
            _loginContext.SaveChanges();
        }



        public string ConvertLogToString(OutputDto_Log log)
        {
            var sb = new StringBuilder();

            log.LogItems.Each(p =>
            {
                sb.Append(p);
            });

            return sb.ToString();
        }

        public string ConvertLogToString(List<RolloverLogMessage> logs)
        {
            var sb = new StringBuilder();

            logs.Each(p =>
            {
                sb.Append(p.Issue + "\r\n");
            });

            return sb.ToString();
        }
    }


}
