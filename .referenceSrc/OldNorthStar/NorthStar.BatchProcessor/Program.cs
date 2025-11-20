using EntityDto.LoginDB.Entity;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using Northstar.Core;
using NorthStar.Core;
using NorthStar.EF6;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NorthStar.BatchProcessor
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var storageConnectionString = ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString;
            NorthStar.EF6.Infrastructure.DtoMappings.Map();
            Console.WriteLine("Got connection string: " + storageConnectionString);
            var config = new JobHostConfiguration
            {
                StorageConnectionString = storageConnectionString,
                DashboardConnectionString = storageConnectionString
            };
            config.Queues.BatchSize = 2;

            //Console.WriteLine("Got login connection string:" + ConfigurationManager.ConnectionStrings["LoginConnection"].ConnectionString);
            //Console.WriteLine("Got Identity Server:" + ConfigurationManager.AppSettings["IdentityServer"]);
            //Console.WriteLine("Got Site URL Base:" + ConfigurationManager.AppSettings["SiteUrlBase"]);
            //Console.WriteLine("Got PDF Server Port:" + ConfigurationManager.AppSettings["PdfServerPort"]);
            //Console.WriteLine("Got PDF Server IP:" + ConfigurationManager.AppSettings["PdfServerIp"]);
            Console.WriteLine("About to start run and block");
            var host = new JobHost(config);
            host.RunAndBlock();



        }

        private static async Task ProcessJob(NSAzureJob job, LoginContext loginContext, TextWriter output)
        {
            // check queue for jobs manually and process
            // see if anything in queue, deque and delete, then process

            INorthStarJob jobDetail = null;
            DistrictContext dbContext = null;
            JobProcessor jobProcessor = null;
            try
            {

                var districtId = 0;

                    switch (job.JobType)
                    {
                        case NSConstants.Azure.JobType.StateTestImport:
                            jobDetail = loginContext.JobStateTestDataImports.FirstOrDefault(p => p.Id == job.JobId);
                            if (jobDetail != null)
                            {
                                dbContext = GetDbContextReference(job, loginContext, jobDetail);
                                if (dbContext != null)
                                {
                                    jobProcessor = new JobProcessor(dbContext, loginContext);
                                    output.WriteLine("Starting Processing of State Test Job " + job.JobId);
                                    await jobProcessor.ProcessStateTestDataJob(jobDetail as JobStateTestDataImport, output);
                                    output.WriteLine("Completed Processing of State Test Job " + job.JobId);
                                }
                            }
                            break;
                        case NSConstants.Azure.JobType.BenchmarkTestImport:
                            jobDetail = loginContext.JobBenchmarkDataImports.FirstOrDefault(p => p.Id == job.JobId);
                            if (jobDetail != null)
                            {
                                dbContext = GetDbContextReference(job, loginContext, jobDetail);
                                if (dbContext != null)
                                {
                                    jobProcessor = new JobProcessor(dbContext, loginContext);
                                    output.WriteLine("Starting Processing of Benchmark Test Job " + job.JobId);
                                    await jobProcessor.ProcessBenchmarkTestDataJob(jobDetail as JobBenchmarkDataImport, output);
                                output.WriteLine("Completed Processing of Benchmark Test Job " + job.JobId);
                                }
                            }
                            break;
                        case NSConstants.Azure.JobType.InterventionTestImport:
                            jobDetail = loginContext.JobInterventionDataImports.FirstOrDefault(p => p.Id == job.JobId);
                            if (jobDetail != null)
                            {
                                dbContext = GetDbContextReference(job, loginContext, jobDetail);
                                if (dbContext != null)
                                {
                                    jobProcessor = new JobProcessor(dbContext, loginContext);
                                output.WriteLine("Starting Processing of Intervention Test Job " + job.JobId);
                                    await jobProcessor.ProcessInterventionTestDataJob(jobDetail as JobInterventionDataImport, output);
                                output.WriteLine("Completed Processing of Intervention Test Job " + job.JobId);
                                }
                            }
                            break;
                        case NSConstants.Azure.JobType.AttendanceExport:
                        jobDetail = loginContext.JobAttendanceExports.FirstOrDefault(p => p.Id == job.JobId);
                        if (jobDetail != null)
                        {
                            dbContext = GetDbContextReference(job, loginContext, jobDetail);
                            if (dbContext != null)
                            {
                                jobProcessor = new JobProcessor(dbContext, loginContext);
                                output.WriteLine("Starting Processing of Attendance Data Export Job " + job.JobId);
                                jobProcessor.ProcessAttendanceDataExportJob(jobDetail as JobAttendanceExport, output);
                                output.WriteLine("Completed Processing of Attendance Data Export Job " + job.JobId);
                            }
                        }
                        break;
                    case NSConstants.Azure.JobType.StudentAttributeExport:
                        jobDetail = loginContext.JobStudentExports.FirstOrDefault(p => p.Id == job.JobId);
                        if (jobDetail != null)
                        {
                            dbContext = GetDbContextReference(job, loginContext, jobDetail);
                            if (dbContext != null)
                            {
                                jobProcessor = new JobProcessor(dbContext, loginContext);
                                output.WriteLine("Starting Processing of Student Data Export Job " + job.JobId);
                                jobProcessor.ProcessStudentExportJob(jobDetail as JobStudentExport, output);
                                output.WriteLine("Completed Processing of Student Data Export Job " + job.JobId);
                            }
                        }
                        break;
                    case NSConstants.Azure.JobType.TeacherAttributeExport:
                        jobDetail = loginContext.JobStaffExports.FirstOrDefault(p => p.Id == job.JobId);
                        if (jobDetail != null)
                        {
                            dbContext = GetDbContextReference(job, loginContext, jobDetail);
                            if (dbContext != null)
                            {
                                jobProcessor = new JobProcessor(dbContext, loginContext);
                                output.WriteLine("Starting Processing of Staff Data Export Job " + job.JobId);
                                jobProcessor.ProcessStaffExportJob(jobDetail as JobStaffExport, output);
                                output.WriteLine("Completed Processing of Staff Data Export Job " + job.JobId);
                            }
                        }
                        break;
                    case NSConstants.Azure.JobType.PrintBatch:
                        jobDetail = loginContext.JobPrintBatches.FirstOrDefault(p => p.Id == job.JobId);
                        if (jobDetail != null)
                        {
                            dbContext = GetDbContextReference(job, loginContext, jobDetail);
                            if (dbContext != null)
                            {
                                jobProcessor = new JobProcessor(dbContext, loginContext);
                                Console.WriteLine("Starting Processing of Print Batch Job " + job.JobId);
                                jobProcessor.ProcessPrintBatchJob(jobDetail as JobPrintBatch, output);
                                Console.WriteLine("Completed Processing of Print Batch Job " + job.JobId);
                            }
                        }
                        break;
                    case NSConstants.Azure.JobType.DataExport:
                        jobDetail = loginContext.JobAssessmentDataExports.FirstOrDefault(p => p.Id == job.JobId);
                        if (jobDetail != null)
                        {
                            dbContext = GetDbContextReference(job, loginContext, jobDetail);
                            if (dbContext != null)
                            {
                                jobProcessor = new JobProcessor(dbContext, loginContext);
                                output.WriteLine("Starting Processing of Assessment Data Export Job " + job.JobId);
                                jobProcessor.ProcessAssessmentDataExportJob(jobDetail as JobAssessmentDataExport, output);
                                output.WriteLine("Completed Processing of Assessment Data Export Job " + job.JobId);
                            }
                        }
                        break;
                    case NSConstants.Azure.JobType.AllFieldsBenchmarkExport:
                        jobDetail = loginContext.JobAllFieldsAssessmentDataExports.FirstOrDefault(p => p.Id == job.JobId);
                        if (jobDetail != null)
                        {
                            dbContext = GetDbContextReference(job, loginContext, jobDetail);
                            if (dbContext != null)
                            {
                                jobProcessor = new JobProcessor(dbContext, loginContext);
                                output.WriteLine("Starting Processing of Assessment All Fields Data Export Job " + job.JobId);
                                jobProcessor.ProcessAllFieldsAssessmentDataExportJob(jobDetail as JobAllFieldsAssessmentDataExport, output);
                                output.WriteLine("Completed Processing of Assessment All Fields Data Export Job " + job.JobId);
                            }
                        }
                        break;
                    case NSConstants.Azure.JobType.InterventionDataExport:
                        jobDetail = loginContext.JobInterventionDataExports.FirstOrDefault(p => p.Id == job.JobId);
                        if (jobDetail != null)
                        {
                            dbContext = GetDbContextReference(job, loginContext, jobDetail);
                            if (dbContext != null)
                            {
                                jobProcessor = new JobProcessor(dbContext, loginContext);
                                output.WriteLine("Starting Processing of Intervention Data Export Job " + job.JobId);
                                jobProcessor.ProcessInterventionDataExportJob(jobDetail as JobInterventionDataExport, output);
                                output.WriteLine("Completed Processing of Intervention Data Export Job " + job.JobId);
                            }
                        }
                        break;
                    case NSConstants.Azure.JobType.FullRollover:
                        jobDetail = loginContext.JobFullRollovers.FirstOrDefault(p => p.Id == job.JobId);
                        if (jobDetail != null && jobDetail.Status != "Cancelled")
                        {
                            dbContext = GetDbContextReference(job, loginContext, jobDetail);
                            if (dbContext != null)
                            {
                                jobProcessor = new JobProcessor(dbContext, loginContext);
                                output.WriteLine("Starting Processing of Full Rollover Job " + job.JobId);
                                districtId = GetDistrictId(loginContext, jobDetail);
                                await jobProcessor.ProccessFullRolloverJob(jobDetail as JobFullRollover, output, districtId);
                                output.WriteLine("Completed Processing of Full Rollover Job " + job.JobId);
                            }
                        }
                        break;
                        case NSConstants.Azure.JobType.RolloverValidation:
                            jobDetail = loginContext.JobFullRollovers.FirstOrDefault(p => p.Id == job.JobId);
                            if (jobDetail != null && jobDetail.Status != "Cancelled")
                            {
                                dbContext = GetDbContextReference(job, loginContext, jobDetail);
                                if (dbContext != null)
                                {
                                    jobProcessor = new JobProcessor(dbContext, loginContext);
                                    output.WriteLine("Resuming Processing of Full Rollover Job " + job.JobId);
                                    districtId = GetDistrictId(loginContext, jobDetail);
                                    jobProcessor.FinalizeFullRolloverJob(jobDetail as JobFullRollover, output, null, districtId);
                                    output.WriteLine("Completed Processing of Full Rollover Job " + job.JobId);
                                }
                            }
                            break;
                    case NSConstants.Azure.JobType.StudentRollover:
                        jobDetail = loginContext.JobStudentRollovers.FirstOrDefault(p => p.Id == job.JobId);
                        if (jobDetail != null && jobDetail.Status != "Cancelled")
                        {
                            dbContext = GetDbContextReference(job, loginContext, jobDetail);
                            if (dbContext != null)
                            {
                                jobProcessor = new JobProcessor(dbContext, loginContext);
                                output.WriteLine("Starting Processing of Student Rollover Job " + job.JobId);
                                districtId = GetDistrictId(loginContext, jobDetail);
                                await jobProcessor.ProccessStudentRolloverJob(jobDetail as JobStudentRollover, output, districtId);
                                output.WriteLine("Completed Processing of Student Rollover Job " + job.JobId);
                            }
                        }
                        break;
                    case NSConstants.Azure.JobType.StudentRolloverValidation:
                        jobDetail = loginContext.JobStudentRollovers.FirstOrDefault(p => p.Id == job.JobId);
                        if (jobDetail != null && jobDetail.Status != "Cancelled")
                        {
                            dbContext = GetDbContextReference(job, loginContext, jobDetail);
                            if (dbContext != null)
                            {
                                jobProcessor = new JobProcessor(dbContext, loginContext);
                                output.WriteLine("Resuming Processing of Student Rollover Job " + job.JobId);
                                districtId = GetDistrictId(loginContext, jobDetail);
                                jobProcessor.FinalizeStudentRolloverJob(jobDetail as JobStudentRollover, output, null, districtId);
                                output.WriteLine("Completed Processing of Student Rollover Job " + job.JobId);
                            }
                        }
                        break;
                    case NSConstants.Azure.JobType.TeacherRollover:
                        jobDetail = loginContext.JobTeacherRollovers.FirstOrDefault(p => p.Id == job.JobId);
                        if (jobDetail != null && jobDetail.Status != "Cancelled")
                        {
                            dbContext = GetDbContextReference(job, loginContext, jobDetail);
                            if (dbContext != null)
                            {
                                jobProcessor = new JobProcessor(dbContext, loginContext);
                                output.WriteLine("Starting Processing of Teacher Rollover Job " + job.JobId);
                                districtId = GetDistrictId(loginContext, jobDetail);
                                await jobProcessor.ProccessTeacherRolloverJob(jobDetail as JobTeacherRollover, output, districtId);
                                output.WriteLine("Completed Processing of Teacher Rollover Job " + job.JobId);
                            }
                        }
                        break;
                    case NSConstants.Azure.JobType.TeacherRolloverValidation:
                        jobDetail = loginContext.JobTeacherRollovers.FirstOrDefault(p => p.Id == job.JobId);
                        if (jobDetail != null && jobDetail.Status != "Cancelled")
                        {
                            dbContext = GetDbContextReference(job, loginContext, jobDetail);
                            if (dbContext != null)
                            {
                                jobProcessor = new JobProcessor(dbContext, loginContext);
                                output.WriteLine("Resuming Processing of Teacher Rollover Job " + job.JobId);
                                districtId = GetDistrictId(loginContext, jobDetail);
                                jobProcessor.FinalizeTeacherRolloverJob(jobDetail as JobTeacherRollover, output, null, districtId);
                                output.WriteLine("Completed Processing of Teacher Rollover Job " + job.JobId);
                            }
                        }
                        break;
                }

            }
            catch (Exception ex)
            {
                if(jobDetail != null)
                {
                    jobDetail.Status = "Error";
                    loginContext.SaveChanges();
                }
                Console.WriteLine(String.Format("An error occurred during processing a job that caused the whole process to quit.  Exception is: {0}, Job Number and Type are: {1} - {2}", ex.Message, job.JobId, job.JobType));
            }
        }

        public static void ProcessLocal()
        {
            var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);
            var loginContext = new LoginContext(ConfigurationManager.ConnectionStrings["LoginConnection"].ConnectionString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a container.
            CloudQueue queue = queueClient.GetQueueReference(NSConstants.Azure.JobQueue);
            while (true)
            {
                Thread.Sleep(1000);

                // check queue for jobs manually and process
                // see if anything in queue, deque and delete, then process
                var message = queue.GetMessage();

                NSAzureJob job = null;
                try
                {
                    if (message != null)
                    {
                        job = JsonConvert.DeserializeObject<NSAzureJob>(message.AsString);

                        ProcessJob(job, loginContext, Console.Out);
                    }
                }
                catch (Exception ex)
                {
                    Console.Write(String.Format("An error occurred during processing a job that cause the whole process to quit.  Exception is: {0}, Queue Message is: {1}", ex.Message, message.AsString));
                }
            }
        }

        private static DistrictContext GetDbContextReference(NSAzureJob job, LoginContext loginContext, INorthStarJob jobDetail)
        {
            DistrictContext _dbContext = null;
            // get the connection string and process the job
            if (jobDetail != null)
            {
                var staffEmail = jobDetail.StaffEmail;
                var districtId = loginContext.StaffDistricts.FirstOrDefault(p => p.StaffEmail == staffEmail);

                if (districtId != null)
                {
                    var cnxString = loginContext.DistrictDbs.FirstOrDefault(p => p.DistrictId == districtId.DistrictId);

                    if (cnxString != null)
                    {
                        _dbContext = new DistrictContext(cnxString.DbName);

                        // now that we have dbcontext... can process stuff

                    }
                    else
                    {
                        // Log extremely strange case of database not existing
                    }
                }
                else
                {
                    // TODO: Log that this funky, user does not have a district... should never happen

                }

            }
            else
            {
                // LOG strange state... multiple job processors or job deleted? Not a big deal, but good to know
            }

            return _dbContext;
        }

        private static int GetDistrictId(LoginContext loginContext, INorthStarJob jobDetail)
        {
            DistrictContext _dbContext = null;
            var districtId = 0;
            // get the connection string and process the job
            if (jobDetail != null)
            {
                var staffEmail = jobDetail.StaffEmail;
                districtId = loginContext.StaffDistricts.FirstOrDefault(p => p.StaffEmail == staffEmail).DistrictId;
            }
            else
            {
                // LOG strange state... multiple job processors or job deleted? Not a big deal, but good to know
            }

            return districtId;
        }

        public static async Task ProcessQueueMessage([QueueTrigger(NSConstants.Azure.JobQueue)] NSAzureJob job, TextWriter logger)
        {
            logger.WriteLine("Received job");
            var loginContext = new LoginContext(ConfigurationManager.ConnectionStrings["LoginConnection"].ConnectionString);
            await ProcessJob(job, loginContext, logger);
            logger.WriteLine("Completed job");
        }
    }
}
