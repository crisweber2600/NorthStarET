using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using System.Net;
using System.Configuration;
using System.IO;
using Rebex.Net;
using DataAccess;
using NorthStar.EF6;
using NorthStar4.PCL.Entity;
using EntityDto.DTO.ImportExport;
using Northstar.Core.Extensions;
using EntityDto.DTO.Admin.Simple;
using EntityDto.LoginDB.Entity;
using NorthStar.Core;
using System.Data.SqlClient;
using NorthStar.Core.Identity;
using NorthStar4.API.Infrastructure;
using Newtonsoft.Json;

namespace NorthStar.AutomatedRollover
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {

        static LoginContext _loginContext = null;
        static DistrictContext dbContext = null;
        static string _loginConnectionString;
        static string ftpFileName = "";
        static int fileYear = 0;
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            //Rebex.Licensing.Key = ConfigurationManager.AppSettings["RebexKey"];
            _loginContext = new LoginContext(ConfigurationManager.ConnectionStrings["LoginConnection"].ConnectionString);
            _loginConnectionString = ConfigurationManager.ConnectionStrings["LoginConnection"].ConnectionString;

            // SH on 8/2/2020
            // Get all Jobs and loop
            var jobs = _loginContext.AutomatedRolloverDetails.Where(p => p.IsActive == true).ToList();

            foreach (var job in jobs)
            {
                dbContext = GetDbContextReference(job);
                var textReader = downloadFile(job);
                // see if there is actually a file
                if (String.IsNullOrEmpty(ftpFileName))
                {
                    return;
                }
                try
                {
                    Console.WriteLine("About to process file");
                    ProcessRollover(textReader, job);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error processing file:" + ex.Message);
                    var result = new AutomaticRolloverOutputDto_Log();
                    result.OverallStatusMessage = "Rollover was not processed successfully.  Errors occurred that prevented successful completion.  Please review the log for details.";
                    result.HardStopErrors.Add("An unexpected error has prevented the rollover from being processed.  Please correct this error before attempting a subsequent rollover.  The error is: " + ex.Message);
                    ReportRolloverStatus(result, job);
                }

            }
            //var host = new JobHost();
            // The following code ensures that the WebJob will be running continuously
            //host.RunAndBlock();

            // Connect to FTP site and look for first file
        }

        private static DistrictContext GetDbContextReference(AutomatedRolloverDetail job)
        {
            DistrictContext _dbContext = null;
            // get the connection string and process the job
            //var staffEmail = jobDetail.StaffEmail;
            var districtId = job.DistrictId; //ONLY 196 is using this //loginContext.StaffDistricts.FirstOrDefault(p => p.StaffEmail == staffEmail);

            var cnxString = _loginContext.DistrictDbs.FirstOrDefault(p => p.DistrictId == districtId);

            if (cnxString != null)
            {
                _dbContext = new DistrictContext(cnxString.DbName);
                // now that we have dbcontext... can process stuff
            }
            else
            {
                // Log extremely strange case of database not existing
            }

            return _dbContext;
        }

        private static TextReader downloadFile(AutomatedRolloverDetail job)
        {
            TextReader sr = null;
            var ftpSite = job.FtpSite;
            var ftpUser = job.FtpUsername;
            var ftpPassword = job.FtpPassword;

            //var serverUri = new Uri(ftpSite);
            var relativeUri = job.RelativeUri;
            //var fullUri = new Uri(serverUri, relativeUri);

            Ftp ftp = new Ftp();
            // Connect securely using explicit SSL.
            ftp.Connect(ftpSite, SslMode.Implicit);

            // Connection is protected now, we can log in safely.
            ftp.Login(ftpUser, ftpPassword);

            // Specifies whether protection of transferred data is required.
            // Default value of this property is "true", so this is not required.
            ftp.SecureTransfers = true;

            ftp.ChangeDirectory(relativeUri);
            string currentDir = ftp.GetCurrentDirectory();
            Console.WriteLine("Current directory changed to: {0}", currentDir);

            // get items within the current directory
            FtpItemCollection currentItems = ftp.GetList();

            // get names of items within "/MyData
            string[] dataItems = ftp.GetNameList();

            // get the file
            if (dataItems.Length > 0)
            {
                var stream = new MemoryStream();
                
                    long bytes = ftp.GetFile(dataItems[0], stream);
                    ftpFileName = dataItems[0].ToString();
                    fileYear = Int32.Parse(ftpFileName.Substring(0, 4));
                    byte[] byteArray = stream.ToArray();

                    stream.Position = 0;
                    sr = new StreamReader(stream);

                    //var read = DataAccess.DataTable.New.Read(sr);
                    //var columns = read.ColumnNames.ToList();



                    //var contents = sr.ReadToEnd();
                    //Console.WriteLine(contents);

                    // got string
                    // need to read it line by line and pass to CSVReader... or see if i can do it all at once

                    // first just process it like normal

                    // then add all the special error handling


                    //Clean up the memory stream

                
            }

            return sr;
            //FtpWebRequest ftpReq = (FtpWebRequest)WebRequest.Create(fullUri);
            //ftpReq.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            //ftpReq.Credentials = new NetworkCredential(ftpUser, ftpPassword);
            //ftpReq.EnableSsl = true;
            //ftpReq.UsePassive = false;

            //FtpWebResponse response = (FtpWebResponse)ftpReq.GetResponse();
            ////response.
            //Stream responseStream = response.GetResponseStream();
            //StreamReader reader = new StreamReader(responseStream);

            //Console.WriteLine(reader.ReadToEnd());
        }

        private static void ReportRolloverStatus(AutomaticRolloverOutputDto_Log log, AutomatedRolloverDetail job)
        {
            // report status and move file to process folder
            EmailHandler.SendAutomatedRolloverEmail(null,  ConfigurationManager.AppSettings["SiteUrlBase"], log);

            // move file
            var ftpSite = job.FtpSite;
            var ftpUser = job.FtpUsername;
            var ftpPassword = job.FtpPassword;

            //var serverUri = new Uri(ftpSite);
            var relativeUri = job.RelativeUri;
            //var fullUri = new Uri(serverUri, relativeUri);

            Ftp ftp = new Ftp();
            // Connect securely using explicit SSL.
            ftp.Connect(ftpSite, SslMode.Implicit);

            // Connection is protected now, we can log in safely.
            ftp.Login(ftpUser, ftpPassword);

            // Specifies whether protection of transferred data is required.
            // Default value of this property is "true", so this is not required.
            ftp.SecureTransfers = true;

            ftp.ChangeDirectory(relativeUri);
            string currentDir = ftp.GetCurrentDirectory();

            // check if file exists in directory already, if so, delete it
            if(ftp.FileExists("/processed/" + ftpFileName))
            {
                ftp.DeleteFile("/processed/" + ftpFileName);
            }
            ftp.Rename(relativeUri + ftpFileName, "/processed/" + ftpFileName);
        }
        private static void ProcessRollover(TextReader textReader, AutomatedRolloverDetail job)
        {
            var forceLoad = job.ForceLoad;
            var hardStopCount = forceLoad ? 1000 : 20;
            var duplicateCount = forceLoad ? 1000 : 100;
            var integrityCount = forceLoad ? 1000 : 50;
            Console.WriteLine("Cancelling previous rollover");
            // clear any existing rollover crap
            dbContext.Database.ExecuteSqlCommand("EXEC dbo.[ns4_cancel_rollover]");
            //now that we have dbcontext...can process stuff
            var invalidRecordCount = 0;
            var validRecordCount = 0;
            var result = new AutomaticRolloverOutputDto_Log();
            //job.Status = "Processing";
            //_loginContext.SaveChanges();

            var fields = RosterRolloverDataService.GetFullRolloverTemplate(dbContext).Fields;

            // download file from azure for processing
            //var manager = new AzurePhotoManager(CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString), NSConstants.Azure.RolloverContainer);

            //var textReaders = await manager.Download(job.UploadedFileUrl);
           // var textReader = (textReaders.Count() == 1 ? textReaders.First() : null);


            var read = DataAccess.DataTable.New.Read(textReader);
            //textReader.Flush();
            textReader.Close();
            var columns = read.ColumnNames.ToList();

            foreach (var row in read.Rows)
            {
                try
                {
                    var recordIsValid = true;
                    var studentCode = row["STUDENT ID"].Replace("\"", String.Empty);

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
                                    result.IntegrityErrors.Add(String.Format("The following student's record did not have a value for the required field '{3}' and is being skipped: {0}, {1} - {2} ", row["Student Last Name"], row["Student First Name"], studentCode, column));
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
                        if (!ValidateRolloverField(field, row, result))
                        {
                            //result.LogItems.Add(String.Format("The following student's record had the invalid value '{0}' for the field '{1}' and is being skipped (could also be an invalid student attribute value): {2}, {3} - {4} ", row[field.FieldName], field.FieldName, row["Student Last Name"], row["Student First Name"], studentCode));
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

                    // stop processing if we've had more than 10 integrity issues
                    if (invalidRecordCount < hardStopCount)
                    {
                        using (System.Data.IDbCommand command = dbContext.Database.Connection.CreateCommand())
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
                                        insertUpdateSql.AppendFormat("{0})", GetRolloverFieldInsertString(field, row, result));
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
                                        insertUpdateSql.AppendFormat("{0},", GetRolloverFieldInsertString(field, row, result));
                                    }
                                }

                                dbContext.Database.Connection.Open();
                                command.CommandTimeout = command.Connection.ConnectionTimeout;
                                command.CommandText = insertColumns.ToString() + insertUpdateSql.ToString();
                                command.ExecuteNonQuery();
                                validRecordCount++;

                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("There was an error processing the rollover: " + ex.Message);
                                //log
                                //output.WriteLine("Error while processing Full Rollover batch: {0}", ex.Message);
                                result.OverallStatusMessage = "Rollover was not processed successfully.  Errors occurred that prevented successful completion.  Please review the log for details.";
                                result.HardStopErrors.Add("An unknown error occurred while processing a record for student: " + studentCode + ".  The error is: " + ex.Message);
                                //job.Status = "Error";
                                //job.EndDate = DateTime.Now;
                                //job.ImportLog = ConvertLogToString(result);
                                invalidRecordCount++;
                                //job.RecordsProcessed = validRecordCount;
                                //job.RecordsSkipped = invalidRecordCount;
                                //_loginContext.SaveChanges();
                                ReportRolloverStatus(result, job);                                
                                return;
                            }
                            finally
                            {
                                dbContext.Database.Connection.Close();
                                command.Parameters.Clear();
                                //_loginContext.SaveChanges();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Outer loop error processing rollover: " + ex.Message);
                    result.OverallStatusMessage = "Rollover was not processed successfully.  Errors occurred that prevented successful completion.  Please review the log for details.";
                    result.HardStopErrors.Add("An unexpected error has prevented the rollover from being processed.  Please correct this error before attempting a subsequent rollover.  The error is: " + ex.Message);
                    ReportRolloverStatus(result, job);
                    //EmailHandler.SendAutomatedRolloverEmail(null, null, null, result);
                    return;
                    //output.WriteLine("Outer Exception while processing Rollover Data batch: {0}", ex.Message);
                }
            }

            // if there are any invalid records, kill the rollover
            if(result.IntegrityErrors.Count > integrityCount || result.HardStopErrors.Count > 0)
            {
                Console.WriteLine("Rollover cancelled due to too many errors. ");
                dbContext.Database.ExecuteSqlCommand("EXEC dbo.[ns4_cancel_rollover]");
                result.OverallStatusMessage = "Rollover was not processed successfully.  Errors occurred that prevented successful completion.  Please review the log for details.";
                ReportRolloverStatus(result, job);
                //job.Status = "Error";
                //job.ImportLog = ConvertLogToString(result);
                //job.RecordsProcessed = validRecordCount;
                //job.RecordsSkipped = invalidRecordCount;
                //_loginContext.SaveChanges();
                return;
            }

            Console.WriteLine("Starting integrity check");
            // all records are inserted... time for the fun part
            var integrityResults = dbContext.Database.SqlQuery<RolloverLogMessage>("EXEC dbo.ns4_rollover_integritycheck @SchoolStartYear", new SqlParameter("@SchoolStartYear", fileYear)).ToList();

            Console.WriteLine("Integrity results count: " + integrityResults.Count);
            if(integrityResults.Count == 0)
            {
                Console.WriteLine("Starting duplication check");
                var duplicationResults = dbContext.Database.SqlQuery<RolloverLogMessage>("EXEC dbo.ns4_rollover_duplicationcheck").ToList();

                if (duplicationResults.Count != 0)
                {
                    Console.WriteLine("There are duplicates... how many?: " + duplicationResults.Count);
                    ConvertDupLogToString(duplicationResults, result);
                    //job.PotentialIssuesLog = JsonConvert.SerializeObject(duplicationResults);
                    //job.Status = "Awaiting User Verification";
                    //job.RecordsProcessed = validRecordCount;
                    //job.RecordsSkipped = invalidRecordCount;
                    //_loginContext.SaveChanges();
                    //return;
                }
            } else
            {
                Console.WriteLine("There are integrity issues, see email for details");
                ConvertLogToString(integrityResults, result);
                //job.Status = "Error";
                //job.RecordsProcessed = validRecordCount;
                //job.RecordsSkipped = invalidRecordCount;
                //_loginContext.SaveChanges();
                //return;
            }

            // TODO: SENDSUMMARYEMAIL
            //job.RecordsProcessed = validRecordCount;
            //job.RecordsSkipped = invalidRecordCount;
            // now that everything's been validated... do the rollover
            // if too many errors, send email and quit

            if (result.IntegrityErrors.Count > integrityCount || result.DuplicationErrors.Count > duplicateCount || result.HardStopErrors.Count > 0)
            {
                Console.WriteLine("Too many errors, rollover is being killed.");
                result.OverallStatusMessage = "Rollover was not processed successfully.  Errors occurred that prevented successful completion.  Please review the log for details.";
                ReportRolloverStatus(result, job);
                return;
            }

            Console.WriteLine("Looks good, proceeding to finalize");
            FinalizeFullRolloverJob(null, result, job.DistrictId, job);
        }
        public class StaffAccountChange
        {
            public string NewUserName { get; set; }
            public string OldUsername { get; set; }
        }

        private static string CreatePassword(int length)
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
        private static void FinalizeFullRolloverJob(TextWriter output, AutomaticRolloverOutputDto_Log result, int districtId, AutomatedRolloverDetail job)
        {
            if (result == null)
            {
                result = new AutomaticRolloverOutputDto_Log();
            }

            try
            {
                Console.WriteLine("Processing full rollover with fileYear: " + fileYear);
                dbContext.Database.ExecuteSqlCommand("EXEC ns4_process_fullRollover @SchoolStartYear", new SqlParameter("@SchoolStartYear", fileYear));

                // now update user accounts that are chagned or new
                var newAccounts = dbContext.Database.SqlQuery<StaffAccountChange>("SELECT Email as NewUserName, Loweredusername as OldUserName FROM Staff WHERE ModifiedBy = 'rollover' and LoweredUserName IS NULL").ToList();
                var changedAccounts = dbContext.Database.SqlQuery<StaffAccountChange>("SELECT Email as NewUserName, Loweredusername as OldUserName FROM Staff WHERE ModifiedBy = 'rollover' and LoweredUserName != Email").ToList();

                Console.WriteLine("How many new accounts: " + newAccounts.Count);
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

                            var staff = dbContext.Staffs.First(s => s.Email == p.NewUserName);
                            staff.ModifiedBy = "useraccountcreation";
                            staff.ModifiedDate = DateTime.Now;
                            dbContext.SaveChanges();

                            // send email
                            EmailHandler.SendUserPasswordEmail(newPassword, p.NewUserName, p.NewUserName, "support@northstaret.net", p.NewUserName, SiteUrlBase);
                        }
                        else
                        {
                            result.UserAccountErrors.Add("Attempting to create a new user, but the user already has a record in the staffdistrict table: " + p.NewUserName);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.UserAccountErrors.Add(String.Format("Error creating new user account for user: {0}.  Error is: {1}", p.NewUserName, ex.Message));
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

                        var staff = dbContext.Staffs.First(s => s.Email == p.NewUserName);
                        staff.ModifiedBy = "useraccountchanged";
                        staff.ModifiedDate = DateTime.Now;
                        dbContext.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error changing usernames: " + ex.Message);
                        result.UserAccountErrors.Add(String.Format("Error changing username after rollover for user: {0}.  Error is: {1}", p.NewUserName, ex.Message));
                    }
                });

                result.OverallStatusMessage = "Rollover completed successfully";
                Console.WriteLine("Rollover complete, sending email to: " + ConfigurationManager.AppSettings["RolloverEmail"] + " and " + ConfigurationManager.AppSettings["AdminEmail"]);
                ReportRolloverStatus(result, job);
                //job.Status = "Complete";
                //job.EndDate = DateTime.Now;
                //job.ImportLog = ConvertLogToString(result);
                //_loginContext.SaveChanges();
            }
            catch (Exception ex)
            {
                // send email to support... i want to know when this happens
                Console.WriteLine("Rollover was not processed successfully because of: " + ex.Message);
                result.OverallStatusMessage = "Rollover was not processed successfully.  Errors occurred that prevented successful completion.  Please review the log for details.";
                //output.WriteLine("Error while processing Full Rollover batch: {0}", ex.Message);
                ReportRolloverStatus(result, job);
                //job.Status = "Error";
                //job.EndDate = DateTime.Now;
                //job.ImportLog = ConvertLogToString(result);
                //_loginContext.SaveChanges();
            }
        }


        // no need to validate if a field is null and required, this has been done already
        public static bool ValidateRolloverField(AssessmentFieldTemplate field, Row row, AutomaticRolloverOutputDto_Log result)
        {
            switch (field.FieldType)
            {
                case "Grade":
                    if (!"P,1,2,3,4,5,6,7,8,9,10,11,12,P,K".Contains(row[field.FieldName]))
                    {
                        result.IntegrityErrors.Add(String.Format("You have assigned an invalid Grade Code of '{3}' and the record has been skipped for for: {0}, {1} - {2} ", row["Student Last Name"], row["Student First Name"], row["Student Id"], row[field.FieldName]));
                        return false;
                    }
                    break;
                case "TeacherAttribute":
                    if (!String.IsNullOrEmpty(row[field.FieldName]) && !"Teacher,Administrator,Teaching Assistant,Other".Contains(row[field.FieldName]))
                    {
                        result.IntegrityErrors.Add(String.Format("You have assigned an invalid Staff Role of '{3}' and the record has been skipped for for: {0}, {1} - {2} ", row["Teacher Last Name"], row["Teacher First Name"], row["Teacher Id"], row[field.FieldName]));
                        return false;
                    }
                    break;
                case "School":
                    var schoolName = row[field.FieldName];
                    var schoolExists = dbContext.Schools.FirstOrDefault(p => p.Name.Equals(schoolName, StringComparison.OrdinalIgnoreCase));
                    if (schoolExists == null)
                    {
                        result.IntegrityErrors.Add(String.Format("The following student's record did not have a vaild School and is being skipped.  The school in the file was '{3}' for student: {0}, {1} - {2} ", row["Student Last Name"], row["Student First Name"], row["Student Id"], row[field.FieldName]));
                        return false;
                    }
                    break;
                case "Bool":
                    if (!String.IsNullOrEmpty(row[field.FieldName]) && !"Y,N".Contains(row[field.FieldName]))
                    {
                        result.IntegrityErrors.Add(String.Format("You have assigned an invalid Is Interventionist Value of '{3}' and the record has been skipped for: {0}, {1} - {2} ", row["Student Last Name"], row["Student First Name"], row["Student Id"], row[field.FieldName]));
                        return false;
                    }
                    break;
                case "Integer":
                    int i;
                    if (!String.IsNullOrEmpty(row[field.FieldName]) && !Int32.TryParse(row[field.FieldName], out i))
                    {
                        result.IntegrityErrors.Add(String.Format("You have assigned an invalid value for Graduation Year of '{3}' and the record has been skipped for: {0}, {1} - {2} ", row["Student Last Name"], row["Student First Name"], row["Student Id"], row[field.FieldName]));
                        return false;
                    }
                    break;
                case "Date":
                    DateTime j;
                    if (!DateTime.TryParse(row[field.FieldName], out j))
                    {
                        result.IntegrityErrors.Add(String.Format("You have assigned an invalid value for Student DOB of '{3}' and the record has been skipped for: {0}, {1} - {2} ", row["Student Last Name"], row["Student First Name"], row["Student Id"], row[field.FieldName]));
                        return false;
                    }
                    break;
                case "StudentAttribute":
                    // get the lookup values for this field and make sure that they're all on the valid list
                    var attribute = field.FieldName;

                    // make sure this is a valid attribute
                    var db_attribute = dbContext.StudentAttributeTypes.FirstOrDefault(p => p.AttributeName.Equals(attribute, StringComparison.OrdinalIgnoreCase));
                    var lookedUpValues = dbContext.StudentAttributeLookupValues.Where(p => p.AttributeID == db_attribute.Id).Select(p => p.LookupValue).ToList();

                    // this attribute doesn't exist
                    if (db_attribute == null)
                    {
                        result.IntegrityErrors.Add(String.Format("You have tried to assign values for the student attribute '{3}' which does not exist and the record has been skipped for: {0}, {1} - {2} ", row["Student Last Name"], row["Student First Name"], row["Student Id"], row[field.FieldName]));
                        return false;
                    }
                    else
                    {
                        var attValues = row[field.FieldName];

                        // only student services can have more than one value
                        var splitAttributes = attValues.Split(Char.Parse(","));

                        if (splitAttributes.Length > 1 && !string.Equals("Student Services", attribute, StringComparison.OrdinalIgnoreCase))
                        {
                            result.IntegrityErrors.Add(String.Format("You have tried to assign multiple values for the student attribute '{3}' which only accepts a single value and the record has been skipped for: {0}, {1} - {2} ", row["Student Last Name"], row["Student First Name"], row["Student Id"], row[field.FieldName]));
                            return false;
                        }
                        else if (string.Equals("Student Services", attribute, StringComparison.InvariantCultureIgnoreCase))
                        {
                            foreach (var att in splitAttributes)
                            {
                                if (!String.IsNullOrEmpty(att) && !lookedUpValues.Contains(att, StringComparer.OrdinalIgnoreCase))
                                {
                                    result.IntegrityErrors.Add(String.Format("You have tried to assign an invalid value of '{4}' for the student attribute '{3}' and the record has been skipped for: {0}, {1} - {2} ", row["Student Last Name"], row["Student First Name"], row["Student Id"], field.FieldName, att));
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            // single valued attribute
                            if (!String.IsNullOrEmpty(attValues) && !lookedUpValues.Contains(attValues, StringComparer.OrdinalIgnoreCase))
                            {
                                result.IntegrityErrors.Add(String.Format("You have tried to assign an invalid value of '{4}' for the student attribute '{3}' and the record has been skipped for: {0}, {1} - {2} ", row["Student Last Name"], row["Student First Name"], row["Student Id"], field.FieldName, attValues));
                                return false;
                            }
                        }
                    }
                    break;
            }
            return true;
        }

        public static string GetRolloverFieldInsertString(AssessmentFieldTemplate field, Row row, AutomaticRolloverOutputDto_Log result)
        {
            switch (field.FieldType)
            {
                case "TeacherAttribute":
                case "Text":
                case "Grade":
                case "School":
                    return String.Format("'{0}'", row[field.FieldName] == null ? String.Empty : row[field.FieldName].Replace("'", "''").Replace("\"", String.Empty));
                case "Bool":
                    return String.Format("{0}", row[field.FieldName] == "Y" ? "'Y'" : "'N'");
                case "Integer":
                    var currentGradYear = row[field.FieldName].ToNullableInt32();

                    if (currentGradYear != null)
                    {
                        return currentGradYear.ToString();
                    }
                    else
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
                        }
                        else
                        {
                            return DateTime.Now.Year.ToString();
                        }
                    }
                case "Date":
                    return String.Format("'{0}'", row[field.FieldName].ToNullableDate()?.ToShortDateString());
                default:
                    return "";
            }
        }

        //public static string ConvertLogToString(AutomaticRolloverOutputDto_Log log)
        //{
        //    var sb = new StringBuilder();

        //    log.LogItems.Each(p =>
        //    {
        //        sb.Append(p);
        //    });

        //    return sb.ToString();
        //}

        public static void ConvertLogToString(List<RolloverLogMessage> logs, AutomaticRolloverOutputDto_Log log)
        {
            //var sb = new StringBuilder();

            logs.Each(p =>
            {
                log.HardStopErrors.Add(p.Issue);
            });

            //return sb.ToString();
        }

        public static void ConvertDupLogToString(List<RolloverLogMessage> logs, AutomaticRolloverOutputDto_Log log)
        {
            //var sb = new StringBuilder();

            logs.Each(p =>
            {
                log.DuplicationErrors.Add(p.Issue);
            });

            //return sb.ToString();
        }

        public static string SiteUrlBase
        {
            get
            {
                return ConfigurationManager.AppSettings["SiteUrlBase"];
            }
            set { }
        }
    }
}
