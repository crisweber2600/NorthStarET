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
using EntityDto.DTO.Reports;
using EntityDto.DTO.Reports.LID;
using System.Data.SqlClient;
using EntityDto.DTO.Reports.FP;
using EntityDto.DTO.Reports.HFW;
using Serilog;
using EntityDto.DTO.Assessment.Benchmarks;
using NorthStar.EF6.Infrastructure;
using EntityDto.DTO.Admin.InterventionGroup;
using NorthStar.EF6.DataService;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using NorthStar.Core.FileUpload;
using System.Configuration;

namespace NorthStar.EF6
{
    public class SectionReportService : NSBaseDataService
    {
        private string _imageContainer = "images";
        private CloudBlobContainer _container = null;
        private CloudBlobClient _client = null;
        private CloudStorageAccount _storageAccount;
        public SectionReportService(ClaimsIdentity user, string loginConnectionString) : base(user, loginConnectionString)
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

        public OutputDto_HfwStudentDetailReportResult GetHFWDetailReport(InputDto_GetHFWDetailReport input)
        {
            var defaultOrder = input.HfwSortOrder == "alphabetic" ? true :false;
            var result = new OutputDto_HfwStudentDetailReportResult();
            // TODO: need to get default section ID... should be passed in via quick search

            var hfwAssessment = _dbContext.Assessments.Include(p => p.FieldGroups).Include(p => p.Fields).FirstOrDefault(p => p.BaseType == NSAssessmentBaseType.HighFrequencyWords);
            var originalFieldsList = hfwAssessment.Fields.ToList();
            var originalFieldGroupsList = hfwAssessment.FieldGroups.ToList();

            if (hfwAssessment == null)
            {
                Log.Fatal("NOTICE!!! Someone has deleted the High Frequency Words Assessment!!");
                return result;
            }


            var dataRows = GetHFWResultDataRowsForStudent(String.Format("SELECT TOP 1 {0} StudentID, t.*, r.*, w.* FROM  {1} t  INNER JOIN  {2} r on r.StudentID = {0} INNER JOIN {3} w on w.StudentID = {0} WHERE t.StudentId = {0}", input.StudentId, hfwAssessment.StorageTable,  hfwAssessment.SecondaryStorageTable, hfwAssessment.TertiaryStorageTable), hfwAssessment);
            foreach (var wordRange in input.SelectedRanges)
            {
                var currentRange = new WordListSection();
                result.Sections.Add(currentRange);
                if (wordRange.text == "Kindergarten")
                {
                    // TODO: eventually add ISKdg to grouping, for now, just bring 1-26
                    hfwAssessment.Fields = originalFieldsList.Where(j => j.FieldType == "DateCheckbox" && j.Group.IsKdg).ToList();
                    hfwAssessment.FieldGroups = originalFieldGroupsList.Where(j => j.IsKdg).ToList();
                    currentRange.IsKdg = true;
                    currentRange.WordCount = hfwAssessment.FieldGroups.Count;
                }
                else
                {
                    var startEnd = wordRange.text.Split(Char.Parse("-"));
                    var start = Int32.Parse(startEnd[0]);
                    var end = Int32.Parse(startEnd[1]);
                    currentRange.Start = start;
                    currentRange.End = end;
                    currentRange.WordCount = 100;

                    // get the fields and fieldgroups that will be used create the structre to return client side
                    if (defaultOrder) // todo: pass in sort order
                    {
                        hfwAssessment.Fields = originalFieldsList.Where(j => j.FieldType == "DateCheckbox" && j.Group.SortOrder <= end && j.Group.SortOrder >= start).ToList();
                        hfwAssessment.FieldGroups = originalFieldGroupsList.Where(j => j.SortOrder <= end && j.SortOrder >= start).ToList();

                    }
                    else
                    {
                        hfwAssessment.Fields = originalFieldsList.Where(j => j.FieldType == "DateCheckbox" && j.Group.AltOrder <= end && j.Group.AltOrder >= start).ToList();
                        hfwAssessment.FieldGroups = originalFieldGroupsList.Where(j => j.AltOrder <= end && j.AltOrder >= start).ToList();

                    }

                }
                // add result to collection 
                HFWStudentDataForRange(dataRows, hfwAssessment, currentRange, defaultOrder);

            }

            result.Sections = result.Sections.OrderBy(p => p.Start).ToList();
            return result;
        }

        public OutputDto_HfwStudentMissingWordsReportResult GetHFWMissingWordsReport(InputDto_GetHFWDetailReport input)
        {
            var defaultOrder = input.HfwSortOrder == "alphabetic" ? true : false;
            var result = new OutputDto_HfwStudentMissingWordsReportResult();
            // TODO: need to get default section ID... should be passed in via quick search

            var hfwAssessment = _dbContext.Assessments.Include(p => p.FieldGroups).Include(p => p.Fields).FirstOrDefault(p => p.BaseType == NSAssessmentBaseType.HighFrequencyWords);
            var originalFieldsList = hfwAssessment.Fields.ToList();
            var originalFieldGroupsList = hfwAssessment.FieldGroups.ToList();

            if (hfwAssessment == null)
            {
                Log.Fatal("NOTICE!!! Someone has deleted the High Frequency Words Assessment!!");
                return result;
            }

            var missingRowSections = new List<MissingWordsSection>();
            var dataRows = GetHFWResultDataRowsForStudent(String.Format("SELECT TOP 1 {0} StudentID, t.*, r.*, w.* FROM  {1} t  INNER JOIN  {2} r on r.StudentID = {0} INNER JOIN {3} w on w.StudentID = {0} WHERE t.StudentId = {0}", input.StudentId, hfwAssessment.StorageTable, hfwAssessment.SecondaryStorageTable, hfwAssessment.TertiaryStorageTable), hfwAssessment);
            //var dataRows = GetHFWResultDataRowsForStudent(String.Format("SELECT TOP 1 s.ID as StudentID, s.FirstName, s.LastName, s.MiddleName, ISNULL(fp.FPText, 'N/A') as FPText, ISNULL(fp.FPValueID, 0) as FPValueID, t.*, r.*, w.*, {1} as InputSectionId FROM dbo.nset_udf_GetStudentIDsForSummaryPages({1}, '{3}', {2}) a LEFT OUTER JOIN Student s on s.ID = a.StudentID LEFT OUTER JOIN {0} t ON a.StudentID = t.StudentID and t.testduedateid = {2} LEFT OUTER JOIN dbo.nset_udf_GetStudentFPText(NULL, {1}, {2}, NULL) fp ON fp.StudentID = s.ID LEFT JOIN {5} r on r.StudentID = {4} LEFT JOIN {6} w on w.StudentID = {4} WHERE s.id = {4} ORDER BY s.LastName, s.FirstName", hfwAssessment.StorageTable, 14954, 400, DateTime.Now, 52849, hfwAssessment.SecondaryStorageTable, hfwAssessment.TertiaryStorageTable), hfwAssessment);
            foreach (var wordRange in input.SelectedRanges)
            {
                var currentRange = new MissingWordsSection();
                missingRowSections.Add(currentRange);
                if (wordRange.text == "Kindergarten")
                {
                    // TODO: eventually add ISKdg to grouping, for now, just bring 1-26
                    hfwAssessment.Fields = originalFieldsList.Where(j => j.FieldType == "DateCheckbox" && j.Group.IsKdg).ToList();
                    hfwAssessment.FieldGroups = originalFieldGroupsList.Where(j => j.IsKdg).ToList();
                    currentRange.IsKdg = true;
                }
                else
                {
                    var startEnd = wordRange.text.Split(Char.Parse("-"));
                    var start = Int32.Parse(startEnd[0]);
                    var end = Int32.Parse(startEnd[1]);
                    currentRange.Start = start;
                    currentRange.End = end;

                    // get the fields and fieldgroups that will be used create the structre to return client side
                    if (defaultOrder) // todo: pass in sort order
                    {
                        hfwAssessment.Fields = originalFieldsList.Where(j => j.FieldType == "DateCheckbox" && j.Group.SortOrder <= end && j.Group.SortOrder >= start).ToList();
                        hfwAssessment.FieldGroups = originalFieldGroupsList.Where(j => j.SortOrder <= end && j.SortOrder >= start).ToList();

                    }
                    else
                    {
                        hfwAssessment.Fields = originalFieldsList.Where(j => j.FieldType == "DateCheckbox" && j.Group.AltOrder <= end && j.Group.AltOrder >= start).ToList();
                        hfwAssessment.FieldGroups = originalFieldGroupsList.Where(j => j.AltOrder <= end && j.AltOrder >= start).ToList();

                    }

                }
                // add result to collection 
                HFWStudentMissingWordsForRange(dataRows, hfwAssessment, currentRange, defaultOrder);
                result.WordsNotReadAndWritten.AddRange(currentRange.Words);
            }

            result.WordsNotReadAndWritten = result.WordsNotReadAndWritten.Distinct().OrderBy(p => p.Word).ToList();
            return result;
        }

        public void HFWStudentMissingWordsForRange(DataRowCollection rows, Assessment assessment, MissingWordsSection range, bool defaultOrder)
        {
            //AssessmentHFWStudentResult studentResult = new AssessmentHFWStudentResult();
            HfwWordRow currentRow = null;

            for (int i = 0; i < rows.Count; i++)
            {
                //range.StudentName = rows[i]["LastName"].ToString() + ", " + rows[i]["FirstName"].ToString();
                //range.TeacherName = "Get Teacher Later";
                //range.SchoolName = "Get School";

                foreach (var field in assessment.Fields.OrderBy(p => p.FieldOrder))
                {
                    if (!defaultOrder) // sortOrder
                    {
                            currentRow = range.Words.FirstOrDefault(p => p.WordId == field.Group.Id);
                            if (currentRow == null)
                            {
                                currentRow = new HfwWordRow { WordId = field.GroupId.Value, Order = field.Group.AltOrder.Value, Word = field.Group.DisplayName };
                                range.Words.Add(currentRow);
                            }
                    }
                    else
                    {
                            currentRow = range.Words.FirstOrDefault(p => p.WordId == field.Group.Id);
                            if (currentRow == null)
                            {
                                currentRow = new HfwWordRow { WordId = field.GroupId.Value, Order = field.Group.SortOrder, Word = field.Group.DisplayName };
                                range.Words.Add(currentRow);
                            }

                    }

                    if (!String.IsNullOrEmpty(field.DatabaseColumn))
                    {
                        //HfwWordRow hfwRow = new HfwWordRow();
                        // how to determine bucket, left or right, it should go in
                        AssessmentFieldResult fieldResult = new AssessmentFieldResult();

                        // determine which collection to add the fieldresult to
                        if (String.IsNullOrEmpty(field.StorageTable))
                        {
                            // TODO: how to get total?
                            //studentResult.TotalFieldResults.Add(fieldResult); no-op
                        }
                        else if (field.StorageTable.Contains("Read"))
                        {
                            currentRow.Read = rows[i][field.DatabaseColumn] == DBNull.Value ? (DateTime?)null : DateTime.Parse(rows[i][field.DatabaseColumn].ToString());
                            currentRow.RA = true;
                        }
                        else
                        {
                            currentRow.Write = rows[i][field.DatabaseColumn] == DBNull.Value ? (DateTime?)null : DateTime.Parse(rows[i][field.DatabaseColumn].ToString());
                            currentRow.WA = true;
                        }
                    }
                }
            }
            range.Words = range.Words.OrderBy(p => p.Word).ToList();
        }

        public void HFWStudentDataForRange(DataRowCollection rows, Assessment assessment, WordListSection range, bool defaultOrder)
        {
            //AssessmentHFWStudentResult studentResult = new AssessmentHFWStudentResult();
            HfwWordRow currentRow = null;

            for (int i = 0; i < rows.Count; i++)
            {
                //range.StudentName = rows[i]["LastName"].ToString() + ", " + rows[i]["FirstName"].ToString();
                //range.TeacherName = "Get Teacher Later";
                //range.SchoolName = "Get School";
                //int studentId = Int32.Parse(rows[i]["StudentID"].ToString());
                //studentResult.StudentId = Int32.Parse(rows[i]["StudentID"].ToString());
                //studentResult.ResultId = (rows[i]["ID"] != DBNull.Value) ? Int32.Parse(rows[i]["ID"].ToString()) : -1;
                //studentResult.FirstName = rows[i]["FirstName"].ToString();
                //studentResult.MiddleName = rows[i]["MiddleName"].ToString();
                //studentResult.LastName = rows[i]["LastName"].ToString();
                //studentResult.FPText = rows[i]["FPText"].ToString();
                //studentResult.FPValueID = (rows[i]["FPValueID"] != DBNull.Value) ? Int32.Parse(rows[i]["FPValueID"].ToString()) : 0;
                //studentResult.StaffId = (rows[i]["RecorderID"] != DBNull.Value) ? Int32.Parse(rows[i]["RecorderID"].ToString()) : -1;
                //studentResult.ClassId = Int32.Parse(rows[i]["InputSectionId"].ToString()); //result.GetPropValue<int>("SectionID");
                //studentResult.TestDueDateId = (rows[i]["TestDueDateID"] != DBNull.Value) ? Int32.Parse(rows[i]["TestDueDateID"].ToString()) : -1;//result.GetPropValue<int>("TestDueDateID");
                //                                                                                                                                 //studentResult.TestDate = result.GetPropValue<DateTime>("TestDueDate");
                                                                                                                                                 //studentResults.Add(studentResult);
                foreach (var field in assessment.Fields.OrderBy(p => p.FieldOrder))
                {
                    if (!defaultOrder) // sortOrder
                    { 
                        if (field.Group.AltOrder <= range.Start + 49 || range.IsKdg) // left side
                        {
                            currentRow = range.LowerSection.Rows.FirstOrDefault(p => p.WordId == field.Group.Id);
                            if(currentRow == null)
                            {
                                currentRow = new HfwWordRow { WordId = field.GroupId.Value, Order = field.Group.AltOrder.Value, Word = field.Group.DisplayName };
                                range.LowerSection.Rows.Add(currentRow);
                            }
                        }
                        else // right side
                        {
                            currentRow = range.UpperSection.Rows.FirstOrDefault(p => p.WordId == field.Group.Id);
                            if (currentRow == null)
                            {
                                currentRow = new HfwWordRow { WordId = field.GroupId.Value, Order = field.Group.AltOrder.Value, Word = field.Group.DisplayName };
                                range.UpperSection.Rows.Add(currentRow);
                            }
                        }
                    } else
                    {
                        if (field.Group.SortOrder <= range.Start + 49 || range.IsKdg) // left side
                        {
                            currentRow = range.LowerSection.Rows.FirstOrDefault(p => p.WordId == field.Group.Id);
                            if (currentRow == null)
                            {
                                currentRow = new HfwWordRow { WordId = field.GroupId.Value, Order = field.Group.SortOrder, Word = field.Group.DisplayName };
                                range.LowerSection.Rows.Add(currentRow);
                            }
                        }
                        else // right side
                        {
                            currentRow = range.UpperSection.Rows.FirstOrDefault(p => p.WordId == field.Group.Id);
                            if (currentRow == null)
                            {
                                currentRow = new HfwWordRow { WordId = field.GroupId.Value, Order = field.Group.SortOrder, Word = field.Group.DisplayName };
                                range.UpperSection.Rows.Add(currentRow);
                            }
                        }
                    }

                    if (!String.IsNullOrEmpty(field.DatabaseColumn))
                    {
                        //HfwWordRow hfwRow = new HfwWordRow();
                        // how to determine bucket, left or right, it should go in
                        AssessmentFieldResult fieldResult = new AssessmentFieldResult();

                        // determine which collection to add the fieldresult to
                        if (String.IsNullOrEmpty(field.StorageTable))
                        {
                            // TODO: how to get total?
                            //studentResult.TotalFieldResults.Add(fieldResult); no-op
                        }
                        else if (field.StorageTable.Contains("Read"))
                        {
                            currentRow.Read = rows[i][field.DatabaseColumn] == DBNull.Value ? (DateTime?)null :  DateTime.Parse(rows[i][field.DatabaseColumn].ToString());
                            currentRow.RA = true;

                            if (currentRow.Read != null)
                            {
                                range.TotalRead++;
                            }

                            if(currentRow.RA && currentRow.WA && currentRow.Read != null && currentRow.Write != null) // TODO: add WriteAssigned and ReadAssigned
                            {
                                range.TotalScore++;
                            }
                        }
                        else
                        {
                            currentRow.Write = rows[i][field.DatabaseColumn] == DBNull.Value ? (DateTime?)null : DateTime.Parse(rows[i][field.DatabaseColumn].ToString());
                            currentRow.WA = true;

                            if (currentRow.Write != null)
                            {
                                range.TotalWritten++;
                            }

                            if (currentRow.RA && currentRow.WA && currentRow.Read != null && currentRow.Write != null) // TODO: add WriteAssigned and ReadAssigned
                            {
                                range.TotalScore++;
                            }
                        }
                       
                    }
                }
            }
            range.LowerSection.Rows = range.LowerSection.Rows.OrderBy(p => p.Order).ToList();
            range.UpperSection.Rows = range.UpperSection.Rows.OrderBy(p => p.Order).ToList();
        }

        public DataRowCollection GetHFWResultDataRowsForStudent(string sql, Assessment assessment)
        {
            DataRowCollection dataRowCollection = null;

            using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();

                try
                {
                    _dbContext.Database.Connection.Open();
                    command.CommandText = sql;
                    command.CommandTimeout = command.Connection.ConnectionTimeout;

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        // load datatable
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        dataRowCollection = dt.Rows;
                    }
                }
                catch (Exception ex)
                {
                    // log
                    Log.Error("Error getting HFW Results: {0}", ex.Message);
                    throw ex;
                }
                finally
                {
                    _dbContext.Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }

            return dataRowCollection; 
        }

        public List<OutputDto_DropdownData> QuickSearchHfwRanges(string search)
        {

            var hfwWordRanges = new List<OutputDto_DropdownData>();

            hfwWordRanges.Add(new OutputDto_DropdownData { id = 1, text = "Kindergarten" });
            hfwWordRanges.Add(new OutputDto_DropdownData { id = 2, text = "1-100" });
            hfwWordRanges.Add(new OutputDto_DropdownData { id = 3, text = "101-200" });
            hfwWordRanges.Add(new OutputDto_DropdownData { id = 4, text = "201-300" });
            hfwWordRanges.Add(new OutputDto_DropdownData { id = 5, text = "301-400" });
            hfwWordRanges.Add(new OutputDto_DropdownData { id = 6, text = "401-500" });
            hfwWordRanges.Add(new OutputDto_DropdownData { id = 7, text = "501-600" });
            hfwWordRanges.Add(new OutputDto_DropdownData { id = 8, text = "601-700" });
            hfwWordRanges.Add(new OutputDto_DropdownData { id = 9, text = "701-800" });
            hfwWordRanges.Add(new OutputDto_DropdownData { id = 10, text = "801-900" });
            hfwWordRanges.Add(new OutputDto_DropdownData { id = 11, text = "901-1000" });

            return hfwWordRanges;
        }



        public OutputDto_StudentSectionSpellingReportResults GetSpellingInventorySectionReport(InputDto_SectionReport_ByTdd input)
        {

            // TODO: Change report type magic number to the name of the group, like alpha, word, sound
            var assessment = _dbContext.Assessments
                .Include("FieldCategories")
                .Include("FieldSubcategories")
                .Include("FieldGroups")
                .FirstOrDefault(m => m.Id == input.AssessmentId);

            // now have the assessment and controls loaded, need to get the test results from the table
            // specified in the assessment and turn each result into a StudentResult DTO
            var studentResults = _dbContext.GetAssessmentStudentResults(assessment, input.SectionId, input.BenchmarkDateId, DateTime.Now, true);

            List<AssessmentField> headerFields = new List<AssessmentField>();
            // set up properly ordered header fields
            foreach (var field in assessment.Fields)
            {
                if (field.SubCategory != null && field.SubCategory.DisplayName == "Report Headers")
                {
                    headerFields.Add(field);
                }
            }

            // fill in the readinglevel and totalfeaturepointsandwords results
            foreach (var studentResult in studentResults)
            {
                studentResult.FieldResults.ToList()
                    .Where(p => !headerFields.Any(hf => hf.DatabaseColumn == p.DbColumn) && p.DbColumn != "totalScore" && p.DbColumn != "totalWSC" && p.DbColumn != "totalFP")
                    .Each(deleted => studentResult.FieldResults.Remove(deleted));
            }

            headerFields = headerFields.OrderBy(j => j.FieldOrder).ToList();


            studentResults = studentResults.OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToList();

            // get rid of useless fields
            assessment.Fields = headerFields;
            return new OutputDto_StudentSectionSpellingReportResults()
            {
                StudentResults = studentResults,
                Assessment = Mapper.Map<AssessmentDto>(assessment),
                HeaderFields = Mapper.Map<List<AssessmentFieldDto>>(headerFields)
            };
        }

        public OutputDto_StudentAssessmentResults GetAssessmentResults(int assessmentId, int classId, int benchmarkDateId)
        {
            var assessment = _dbContext.Assessments.Include("FieldGroups")
                .Include("FieldCategories")
                .Include("FieldSubCategories")
                .Include("Fields")
                .FirstOrDefault(m => m.Id == assessmentId);

            // now have the assessment and controls loaded, need to get the test results from the table
            // specified in the assessment and turn each result into a StudentResult DTO
            var studentResults = _dbContext.GetAssessmentStudentResults(assessment, classId, benchmarkDateId, DateTime.MaxValue, true);

            // return JObject with the proper format
            assessment.Fields = assessment.Fields.OrderBy(p => p.FieldOrder).ToList();

            return new OutputDto_StudentAssessmentResults()
            {
                StudentResults = studentResults,
                Assessment = Mapper.Map<AssessmentDto>(assessment)
            };
        }

        public OutputDto_StudentSectionCAPReportResults GetCAPSectionReport(InputDto_BAS_SectionReport input)
        {
            // TODO: Change report type magic number to the name of the group, like alpha, word, sound
            var assessment = _dbContext.Assessments
                .Include("FieldCategories")
                .Include("FieldSubcategories")
                .Include("FieldGroups")
                .FirstOrDefault(m => m.Id == input.AssessmentId);

            // now have the assessment and controls loaded, need to get the test results from the table
            // specified in the assessment and turn each result into a StudentResult DTO
            var studentResults = _dbContext.GetBASAssessmentStudentResults(assessment, input.SectionId, input.SchoolYear);
            var testDueDates = _dbContext.TestDueDates.Where(p => p.SchoolStartYear == input.SchoolYear).OrderBy(p => p.DueDate).ToList();

            int currentStudentId = 0;
            StudentSectionCAPReportResult currentStudentRecord = null;
            var studentSectionReportResults = new List<StudentSectionCAPReportResult>();

            // group results by Test Due Date ID
            foreach (var studentResult in studentResults)
            {
                // create a new student record
                if (currentStudentId != studentResult.StudentId)
                {
                    currentStudentId = studentResult.StudentId;
                    currentStudentRecord = new StudentSectionCAPReportResult()
                    {
                        ClassId = studentResult.ClassId,
                        StaffId = studentResult.StaffId,
                        StudentId = studentResult.StudentId,
                        FirstName = studentResult.FirstName,
                        LastName = studentResult.LastName,
                        MiddleName = studentResult.MiddleName
                    };

                    studentSectionReportResults.Add(currentStudentRecord);
                }

                // convert assessmentfieldresults into CAPFieldResults... combine comment and checkbox into a single field
                List<CAPFieldResult> capFieldResults = new List<CAPFieldResult>();
                // the TOTAL field is now in fieldresults, need to filter it out by DBColumn... not sure if i like this hard-coded "magic value"
                foreach (var field in studentResult.FieldResults.Where(p => p.DbColumn != "SumTotal" && p.DbColumn != "totalOverallResponse" && p.DbColumn != "totalOverallResponseO"))
                {
                    var fieldSuffix = field.DbColumn.Substring(3);
                    var fieldPrefix = field.DbColumn.Substring(0, 3);

                    // if this field has already been added
                    var currentFieldResult = capFieldResults.FirstOrDefault(p => p.DbColumn == fieldSuffix);
                    if (currentFieldResult == null)
                    {
                        var groupId = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn).GroupId;
                        currentFieldResult = new CAPFieldResult()
                        {
                            DbColumn = fieldSuffix,
                            GroupId = groupId.Value
                        };
                        capFieldResults.Add(currentFieldResult);
                    }

                    if (fieldPrefix.ToLower() == "chk")
                    {
                        currentFieldResult.Checked = field.BoolValue;
                    }
                    else if (fieldPrefix.ToLower() == "txt")
                    {
                        currentFieldResult.Comment = String.IsNullOrWhiteSpace(field.StringValue) ? "" : field.StringValue.Replace("'", "&apos;").Replace("\"", "&quot;");
                    }
                }

                // I think what I was doing before was trying to add the total field in there somehow
                //List<CAPTotalFieldResult> capTotalFieldResults = new List<CAPTotalFieldResult>();
                //// the TOTAL field is now in fieldresults, need to filter it out by DBColumn... not sure if i like this hard-coded "magic value"
                //foreach (var field in studentResult.FieldResults.Where(p => p.DbColumn == "SumTotal"))
                //{
                //    // if this field has already been added

                //    var groupId = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn).GroupId;
                //    var currentFieldResult = new CAPTotalFieldResult()
                //        {
                //            DbColumn = field.DbColumn,
                //            GroupId = -1,
                //            Score = field.IntValue
                //        };
                //        capTotalFieldResults.Add(currentFieldResult);
                //}

                // add the summary fields

                // now add total field
                //CAPTotalFieldResult totalField = new CAPTotalFieldResult();

                //// add total score to collection
                //currentStudentRecord.FieldTotalResultsByTestDueDate.Add(new CAPTotalFieldResultByTDDID()
                //{
                //    TDDID = studentResult.TestDueDateId.Value,
                //    FieldResults = capTotalFieldResults
                //});

                // just add the field results to the proper collection
                currentStudentRecord.FieldResultsByTestDueDate.Add(new CAPFieldResultByTDD()
                {
                    TDDID = studentResult.TestDueDateId.Value,
                    FieldResults = capFieldResults
                });
            }

            List<AssessmentField> headerFields = new List<AssessmentField>();
            // set up properly ordered header fields
            foreach (var field in assessment.Fields)
            {
                if (field.Category != null && (field.Category.DisplayName == "Item" || field.Category.DisplayName == "Word" || field.Category.DisplayName == "Sheet 1" || field.Category.DisplayName == "Sheet 2" || field.Category.DisplayName == "Sheet"))
                {
                    headerFields.Add(field);
                }
            }

            
            headerFields = headerFields.OrderBy(p => p.SubCategory.SortOrder).ThenBy(j => j.FieldOrder).ToList();

            // now add summarized record with "date" and X markers and set currenttotalscore
            // loop over the fields to get the group ID only once
            foreach (var studentReportResult in studentSectionReportResults)
            {
                // create the summary list for each student
                var summaryResults = new List<CAPSummaryFieldResult>();
                studentReportResult.SummaryFieldResults = summaryResults;

                foreach (var field in headerFields)
                {
                    // every field will have a summary result
                    var newSummaryResult = new CAPSummaryFieldResult() { GroupId = field.GroupId.Value, DbColumn = field.DatabaseColumn };
                    summaryResults.Add(newSummaryResult);

                    foreach (var tdd in testDueDates)
                    {
                        var fieldResultsForTDD = studentReportResult.FieldResultsByTestDueDate.FirstOrDefault(p => p.TDDID == tdd.Id);

                        // if we have results for this TDD for this student
                        if (fieldResultsForTDD != null)
                        {

                            // get the field result for the current field we are looking at
                            var fieldResultForCurrentField = fieldResultsForTDD.FieldResults.FirstOrDefault(p => p.GroupId == field.GroupId);

                            // check to see if we've already set a date for this fieldResult
                            var existingResult = summaryResults.FirstOrDefault(p => p.CellColorDate != null && p.GroupId == field.GroupId);

                            // if the current result for the current due date and field is checked
                            if (fieldResultForCurrentField.Checked.HasValue && fieldResultForCurrentField.Checked.Value)
                            {
                                // if this result was set before, dont change it just set the new color
                                if (existingResult == null)
                                {
                                    newSummaryResult.CellColorDate = tdd.DueDate.Value;
                                }
                                else // fringe case!! if it was colored before and ther is an X, we've just gotten rid of it
                                {
                                    if (existingResult.XColorDate.HasValue)
                                    {
                                        existingResult.CellColorDate = tdd.DueDate.Value;
                                        existingResult.XColorDate = null;
                                    }
                                }
                            }
                            else
                            {
                                // current TDD for this field is NOT checked... we need to set the "X" and thus the previouscheck if it WAS set before
                                if (existingResult != null && existingResult.CellColorDate.HasValue)
                                {
                                    existingResult.XColorDate = tdd.DueDate;
                                }
                            }
                        }
                    }
                }
            }

            headerFields = headerFields.OrderBy(p => p.SubCategory.SortOrder).ThenBy(j => j.FieldOrder).ToList();
            studentSectionReportResults = studentSectionReportResults.OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToList();

            return new OutputDto_StudentSectionCAPReportResults()
            {
                StudentSectionReportResults = studentSectionReportResults,
                Assessment = Mapper.Map<AssessmentDto>(assessment),
                TestDueDates = Mapper.Map<List<TestDueDateDto>>(testDueDates),
                HeaderFields = Mapper.Map<List<AssessmentFieldDto>>(headerFields)
            };
        }

        public OutputDto_StudentSectionAVMRSingleDateReportResults GetAVMRSingleDateSectionReport(InputDto_SectionReport_ByTdd input)
        {
            // TODO: Change report type magic number to the name of the group, like alpha, word, sound
            var assessment = _dbContext.Assessments
                .Include("FieldCategories")
                .Include("FieldSubcategories")
                .Include("FieldGroups")
                .Include("FieldGroupContainers")
                .FirstOrDefault(m => m.Id == input.AssessmentId);

            // now have the assessment and controls loaded, need to get the test results from the table
            // specified in the assessment and turn each result into a StudentResult DTO
            var studentResults = _dbContext.GetAssessmentStudentResults(assessment, input.SectionId, input.BenchmarkDateId, DateTime.Now, false);


            int currentStudentId = 0;
            StudentSectionAVMRSingleDateReportResult currentStudentRecord = null;
            var studentSectionReportResults = new List<StudentSectionAVMRSingleDateReportResult>();

            // group results by Test Due Date ID
            foreach (var studentResult in studentResults)
            {
                // create a new student record
                if (currentStudentId != studentResult.StudentId)
                {
                    currentStudentId = studentResult.StudentId;
                    currentStudentRecord = new StudentSectionAVMRSingleDateReportResult()
                    {
                        ClassId = studentResult.ClassId,
                        StaffId = studentResult.StaffId,
                        StudentId = studentResult.StudentId,
                        FullName = studentResult.StudentName
                    };

                    studentSectionReportResults.Add(currentStudentRecord);
                }

                // convert assessmentfieldresults into CAPFieldResults... combine comment and checkbox into a single field
                List<CAPFieldResult> capFieldResults = new List<CAPFieldResult>();
                // the TOTAL field is now in fieldresults, need to filter it out by DBColumn... not sure if i like this hard-coded "magic value"
                foreach (var field in studentResult.FieldResults.Where(p => p.FF1 == true || p.FF2 == true || p.FF3 == true))
                {
                    //var fieldSuffix = field.DbColumn.Substring(3);
                    //var fieldPrefix = field.DbColumn.Substring(0, 3);

                    // if this field has already been added, same group and same flag
                    var currentFieldResult = capFieldResults.FirstOrDefault(p => p.GroupId == field.GroupId && (
                        (field.FF1 == true && field.FF1 == p.FF1) || (field.FF2 == true && field.FF2 == p.FF2) || (field.FF3 == true && field.FF3 == p.FF3)
                        )
                    );
                    if (currentFieldResult == null)
                    {
                        // hack to skip items that don't have a group
                        var groupId = assessment.Fields.First(p => p.Id == field.FieldId).GroupId;
                        if (groupId == null)
                        {
                            continue;
                        }
                        currentFieldResult = new CAPFieldResult()
                        {
                            DbColumn = field.DbColumn,
                            GroupId = groupId.Value,
                            FieldId = field.FieldId,
                            FF1 = field.FF1,
                            FF2 = field.FF2,
                            FF3 = field.FF3
                        };
                        capFieldResults.Add(currentFieldResult);
                    }

                    if (field.FieldType.ToLower() == "checkbox")
                    {
                        currentFieldResult.Checked = field.BoolValue;
                    }
                    else if (field.FieldType.ToLower() == "textarea")
                    {
                        currentFieldResult.Comment = String.IsNullOrWhiteSpace(field.StringValue) ? "" : field.StringValue.Replace("'", "&apos;").Replace("\"", "&quot;");
                    }
                }


                // just add the field results to the proper collection
                currentStudentRecord.FieldResults = capFieldResults;
            }

            List<AssessmentField> headerFields = new List<AssessmentField>();
            //List<AssessmentFieldGroupContainer> headerFieldGroupContainers = new List<AssessmentFieldGroupContainer>();
            // set up properly ordered header fields
            foreach (var field in assessment.Fields)
            {
                if (field.Category != null && field.SubCategory != null && field.FieldType.ToLower() == "checkbox" && (field.Flag1 == true || field.Flag2 == true || field.Flag3 == true))
                {
                    // see if we've added this field's

                    headerFields.Add(field);
                }
            }


            //headerFields = headerFields.OrderBy(p => p.Group.Container.SortOrder).ThenBy(j => j.FieldOrder).ToList();


            headerFields = headerFields.OrderBy(p => p.Group.Container.SortOrder).ThenBy(k => k.Group.SortOrder).ThenBy(j => j.FieldOrder).ToList();
            //headerFields = headerFields.OrderBy(p => p.SubCategory.SortOrder).ThenBy(j => j.FieldOrder).ToList();
            studentSectionReportResults = studentSectionReportResults.OrderBy(p => p.FullName).ToList();

            return new OutputDto_StudentSectionAVMRSingleDateReportResults()
            {
                StudentSectionReportResults = studentSectionReportResults,
                Assessment = Mapper.Map<AssessmentDto>(assessment),
                HeaderFields = Mapper.Map<List<AssessmentFieldDto>>(headerFields)
            };
        }

        public OutputDto_StudentSectionAVMRSingleDateDetailReportResults GetAVMRSingleDateSectionReportDetail(InputDto_SectionReport_ByTdd input)
        {
            // TODO: Change report type magic number to the name of the group, like alpha, word, sound
            var assessment = _dbContext.Assessments
                .Include("FieldCategories")
                .Include("FieldSubcategories")
                .Include("FieldGroups")
                .Include("FieldGroupContainers")
                .FirstOrDefault(m => m.Id == input.AssessmentId);

            // now have the assessment and controls loaded, need to get the test results from the table
            // specified in the assessment and turn each result into a StudentResult DTO
            var studentResults = _dbContext.GetAssessmentStudentResults(assessment, input.SectionId, input.BenchmarkDateId, DateTime.Now, false);
            var headerFields = assessment.Fields.Where(p => p.SubCategory != null).ToList();
            headerFields = headerFields.OrderBy(p => p.SubCategory.SortOrder).ThenBy(j => j.FieldOrder).ToList();
            studentResults = studentResults.OrderBy(p => p.StudentName).ToList();

            return new OutputDto_StudentSectionAVMRSingleDateDetailReportResults()
            {
                StudentSectionReportResults = studentResults,
                Assessment = Mapper.Map<AssessmentDto>(assessment),
                HeaderFields = Mapper.Map<List<AssessmentFieldDto>>(headerFields)
            };
        }

        public OutputDto_StudentSectionCAPReportResults GetAVMRSectionReport(InputDto_BAS_SectionReport input)
        {
            // TODO: Change report type magic number to the name of the group, like alpha, word, sound
            var assessment = _dbContext.Assessments
                .Include("FieldCategories")
                .Include("FieldSubcategories")
                .Include("FieldGroups")
                .Include("FieldGroupContainers")
                .FirstOrDefault(m => m.Id == input.AssessmentId);

            // now have the assessment and controls loaded, need to get the test results from the table
            // specified in the assessment and turn each result into a StudentResult DTO
            var studentResults = _dbContext.GetBASAssessmentStudentResults(assessment, input.SectionId, input.SchoolYear);
            var testDueDates = _dbContext.TestDueDates.Where(p => p.SchoolStartYear == input.SchoolYear).OrderBy(p => p.DueDate).ToList();

            int currentStudentId = 0;
            StudentSectionCAPReportResult currentStudentRecord = null;
            var studentSectionReportResults = new List<StudentSectionCAPReportResult>();

            // group results by Test Due Date ID
            foreach (var studentResult in studentResults)
            {
                // create a new student record
                if (currentStudentId != studentResult.StudentId)
                {
                    currentStudentId = studentResult.StudentId;
                    currentStudentRecord = new StudentSectionCAPReportResult()
                    {
                        ClassId = studentResult.ClassId,
                        StaffId = studentResult.StaffId,
                        StudentId = studentResult.StudentId,
                        FirstName = studentResult.FirstName,
                        LastName = studentResult.LastName,
                        MiddleName = studentResult.MiddleName
                    };

                    studentSectionReportResults.Add(currentStudentRecord);
                }

                // convert assessmentfieldresults into CAPFieldResults... combine comment and checkbox into a single field
                List<CAPFieldResult> capFieldResults = new List<CAPFieldResult>();
                // the TOTAL field is now in fieldresults, need to filter it out by DBColumn... not sure if i like this hard-coded "magic value"
                foreach (var field in studentResult.FieldResults.Where(p => p.DbColumn != "fnwslevel" && p.DbColumn != "fnwslevelcomments" 
                && p.DbColumn != "nidlevel" &&  p.DbColumn != "nidlevelcomments"
                 && p.DbColumn != "bnwslevel" && p.DbColumn != "bnwslevelcomments"
                  && p.DbColumn != "overallcomments"))
                {
                    //var fieldSuffix = field.DbColumn.Substring(3);
                    //var fieldPrefix = field.DbColumn.Substring(0, 3);

                    // if this field has already been added
                    var currentFieldResult = capFieldResults.FirstOrDefault(p => p.GroupId == field.GroupId);
                    if (currentFieldResult == null)
                    {
                        var groupId = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn).GroupId;
                        currentFieldResult = new CAPFieldResult()
                        {
                            DbColumn = field.DbColumn,
                            GroupId = groupId.Value
                        };
                        capFieldResults.Add(currentFieldResult);
                    }

                    if (field.FieldType.ToLower() == "checkbox")
                    {
                        currentFieldResult.Checked = field.BoolValue;
                    }
                    else if (field.FieldType.ToLower() == "textarea")
                    {
                        currentFieldResult.Comment = String.IsNullOrWhiteSpace(field.StringValue) ? "" : field.StringValue.Replace("'", "&apos;").Replace("\"", "&quot;");
                    }
                }

                
                // just add the field results to the proper collection
                currentStudentRecord.FieldResultsByTestDueDate.Add(new CAPFieldResultByTDD()
                {
                    TDDID = studentResult.TestDueDateId.Value,
                    FieldResults = capFieldResults
                });
            }

            List<AssessmentField> headerFields = new List<AssessmentField>();
            //List<AssessmentFieldGroupContainer> headerFieldGroupContainers = new List<AssessmentFieldGroupContainer>();
            // set up properly ordered header fields
            foreach (var field in assessment.Fields)
            {
                if (field.Category != null && field.SubCategory != null && ((field.Category.DisplayName == "Correct Response") || (field.SubCategory.DisplayName == "Task Group 6" && field.FieldType == "Textarea")))
                {
                    // see if we've added this field's

                    headerFields.Add(field);
                }
            }


            headerFields = headerFields.OrderBy(p => p.Group.Container.SortOrder).ThenBy(k => k.Group.SortOrder).ThenBy(j => j.FieldOrder).ToList();

            // now add summarized record with "date" and X markers and set currenttotalscore
            // loop over the fields to get the group ID only once
            foreach (var studentReportResult in studentSectionReportResults)
            {
                // create the summary list for each student
                var summaryResults = new List<CAPSummaryFieldResult>();
                studentReportResult.SummaryFieldResults = summaryResults;

                foreach (var field in headerFields)
                {
                    // every field will have a summary result
                    var newSummaryResult = new CAPSummaryFieldResult() { GroupId = field.GroupId.Value, DbColumn = field.DatabaseColumn };
                    summaryResults.Add(newSummaryResult);

                    foreach (var tdd in testDueDates)
                    {
                        var fieldResultsForTDD = studentReportResult.FieldResultsByTestDueDate.FirstOrDefault(p => p.TDDID == tdd.Id);

                        // if we have results for this TDD for this student
                        if (fieldResultsForTDD != null)
                        {

                            // get the field result for the current field we are looking at
                            var fieldResultForCurrentField = fieldResultsForTDD.FieldResults.FirstOrDefault(p => p.GroupId == field.GroupId);

                            // check to see if we've already set a date for this fieldResult
                            var existingResult = summaryResults.FirstOrDefault(p => p.CellColorDate != null && p.GroupId == field.GroupId);

                            // if the current result for the current due date and field is checked
                            if (fieldResultForCurrentField.Checked.HasValue && fieldResultForCurrentField.Checked.Value)
                            {
                                // if this result was set before, dont change it just set the new color
                                if (existingResult == null)
                                {
                                    newSummaryResult.CellColorDate = tdd.DueDate.Value;
                                }
                                else // fringe case!! if it was colored before and ther is an X, we've just gotten rid of it
                                {
                                    if (existingResult.XColorDate.HasValue)
                                    {
                                        existingResult.CellColorDate = tdd.DueDate.Value;
                                        existingResult.XColorDate = null;
                                    }
                                }
                            }
                            else
                            {
                                // current TDD for this field is NOT checked... we need to set the "X" and thus the previouscheck if it WAS set before
                                if (existingResult != null && existingResult.CellColorDate.HasValue)
                                {
                                    existingResult.XColorDate = tdd.DueDate;
                                }
                            }
                        }
                    }
                }
            }

            headerFields = headerFields.OrderBy(p => p.Group.Container.SortOrder).ThenBy(k => k.Group.SortOrder).ThenBy(j => j.FieldOrder).ToList();
            studentSectionReportResults = studentSectionReportResults.OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToList();

            return new OutputDto_StudentSectionCAPReportResults()
            {
                StudentSectionReportResults = studentSectionReportResults,
                Assessment = Mapper.Map<AssessmentDto>(assessment),
                TestDueDates = Mapper.Map<List<TestDueDateDto>>(testDueDates),
                HeaderFields = Mapper.Map<List<AssessmentFieldDto>>(headerFields)
            };
        }

        public OutputDto_SuccessAndNewId GetHRSIWFormForClass(InputDto_SimpleId input)
        {
            string sql = "select TOP 1 FormId from data_HearingAndRecordingSounds WHERE SectionId = " + input.Id;
            var formId = 1;
            // Define the ADO.NET Objects
            using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
            {
                try
                {

                    _dbContext.Database.Connection.Open();
                    command.CommandText = sql;
                    formId = (int?)command.ExecuteScalar() ?? 1;
                }
                finally
                {
                    _dbContext.Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }

            return new OutputDto_SuccessAndNewId { id = formId };
        }

        public OutputDto_SuccessAndNewId GetHRSIW2FormForClass(InputDto_SimpleId input)
        {
            string sql = "select TOP 1 FormId from data_HRS_Adv_Dictation WHERE SectionId = " + input.Id;
            var formId = 1;
            // Define the ADO.NET Objects
            using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
            {
                try
                {

                    _dbContext.Database.Connection.Open();
                    command.CommandText = sql;
                    formId = (int?)command.ExecuteScalar() ?? 1;
                }
                finally
                {
                    _dbContext.Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }

            return new OutputDto_SuccessAndNewId { id = formId };
        }

        public OutputDto_SuccessAndNewId GetHRSIW3FormForClass(InputDto_SimpleId input)
        {
            string sql = "select TOP 1 FormId from data_HRS_V2 WHERE SectionId = " + input.Id;
            var formId = 1;
            // Define the ADO.NET Objects
            using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
            {
                try
                {

                    _dbContext.Database.Connection.Open();
                    command.CommandText = sql;
                    formId = (int?)command.ExecuteScalar() ?? 1;
                }
                finally
                {
                    _dbContext.Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }

            return new OutputDto_SuccessAndNewId { id = formId };
        }

        public OutputDto_StudentSectionHRSIWReportResults GetHRSIWSectionReport(InputDto_BAS_SectionReport input)
        {
            // TODO: Change report type magic number to the name of the group, like alpha, word, sound
            var assessment = _dbContext.Assessments
                .Include("FieldCategories")
                .Include("FieldSubcategories")
                .Include("FieldGroups")
                .FirstOrDefault(m => m.Id == input.AssessmentId);

            // now have the assessment and controls loaded, need to get the test results from the table
            // specified in the assessment and turn each result into a StudentResult DTO
            var studentResults = _dbContext.GetBASAssessmentStudentResults(assessment, input.SectionId, input.SchoolYear);
            var testDueDates = _dbContext.TestDueDates.Where(p => p.SchoolStartYear == input.SchoolYear).OrderBy(p => p.DueDate).ToList();

            int currentStudentId = 0;
            StudentSectionHRSIWReportResult currentStudentRecord = null;
            var studentSectionReportResults = new List<StudentSectionHRSIWReportResult>();

            // group results by Test Due Date ID
            foreach (var studentResult in studentResults)
            {
                // create a new student record
                if (currentStudentId != studentResult.StudentId)
                {
                    currentStudentId = studentResult.StudentId;
                    currentStudentRecord = new StudentSectionHRSIWReportResult()
                    {
                        ClassId = studentResult.ClassId,
                        StaffId = studentResult.StaffId,
                        StudentId = studentResult.StudentId,
                        FirstName = studentResult.FirstName,
                        LastName = studentResult.LastName,
                        MiddleName = studentResult.MiddleName
                    };

                    studentSectionReportResults.Add(currentStudentRecord);
                }

                // convert assessmentfieldresults into CAPFieldResults... combine comment and checkbox into a single field
                List<HRSIWFieldResult> hrsiwFieldResults = new List<HRSIWFieldResult>();
                // the TOTAL field is now in fieldresults, need to filter it out by DBColumn... not sure if i like this hard-coded "magic value"
                foreach (var field in studentResult.FieldResults.Where(p => p.DbColumn != null && p.DbColumn.Substring(0, 6) == "Letter"))
                {

                        var currentFieldResult = new HRSIWFieldResult()
                        {
                            DbColumn = field.DbColumn,
                            Checked = field.BoolValue
                        };
                        hrsiwFieldResults.Add(currentFieldResult);
                }

                var commentResult = studentResult.FieldResults.First(p => p.DbColumn == "comments");

                // just add the field results to the proper collection
                currentStudentRecord.FieldResultsByTestDueDate.Add(new HRSIWFieldResultByTDD()
                {
                    TDDID = studentResult.TestDueDateId.Value,
                    FieldResults = hrsiwFieldResults,
                    Comments = commentResult.StringValue
                });
            }

            List<AssessmentField> headerFields = new List<AssessmentField>();
            // set up properly ordered header fields
            foreach (var field in assessment.Fields)
            {
                if (field.DatabaseColumn != null && field.DatabaseColumn.Substring(0, 6) == "Letter" && field.Page == input.HRSFormId)
                {
                    headerFields.Add(field);
                }
            }

            headerFields = headerFields.OrderBy(p => p.FieldOrder).ToList();

            // now add summarized record with "date" and X markers and set currenttotalscore
            // loop over the fields to get the group ID only once
            foreach (var studentReportResult in studentSectionReportResults)
            {
                // create the summary list for each student
                var summaryResults = new List<HRSIWSummaryFieldResult>();
                studentReportResult.SummaryFieldResults = summaryResults;

                foreach (var field in headerFields)
                {
                    // every field will have a summary result
                    var newSummaryResult = new HRSIWSummaryFieldResult() {  DbColumn = field.DatabaseColumn };
                    summaryResults.Add(newSummaryResult);

                    foreach (var tdd in testDueDates)
                    {
                        var fieldResultsForTDD = studentReportResult.FieldResultsByTestDueDate.FirstOrDefault(p => p.TDDID == tdd.Id);

                        // if we have results for this TDD for this student
                        if (fieldResultsForTDD != null)
                        {

                            // get the field result for the current field we are looking at
                            var fieldResultForCurrentField = fieldResultsForTDD.FieldResults.FirstOrDefault(p => p.DbColumn == field.DatabaseColumn);

                            // check to see if we've already set a date for this fieldResult
                            var existingResult = summaryResults.FirstOrDefault(p => p.CellColorDate != null && p.DbColumn == field.DatabaseColumn);

                            // if the current result for the current due date and field is checked
                            if (fieldResultForCurrentField.Checked.HasValue && fieldResultForCurrentField.Checked.Value)
                            {
                                // if this result was set before, dont change it just set the new color
                                if (existingResult == null)
                                {
                                    newSummaryResult.CellColorDate = tdd.DueDate.Value;
                                }
                                else // fringe case!! if it was colored before and ther is an X, we've just gotten rid of it
                                {
                                    if (existingResult.XColorDate.HasValue)
                                    {
                                        existingResult.CellColorDate = tdd.DueDate.Value;
                                        existingResult.XColorDate = null;
                                    }
                                }
                            }
                            else
                            {
                                // current TDD for this field is NOT checked... we need to set the "X" and thus the previouscheck if it WAS set before
                                if (existingResult != null && existingResult.CellColorDate.HasValue)
                                {
                                    existingResult.XColorDate = tdd.DueDate;
                                }
                            }
                        }
                    }
                }
            }

            studentSectionReportResults = studentSectionReportResults.OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToList();

            return new OutputDto_StudentSectionHRSIWReportResults()
            {
                StudentSectionReportResults = studentSectionReportResults,
                Assessment = Mapper.Map<AssessmentDto>(assessment),
                TestDueDates = Mapper.Map<List<TestDueDateDto>>(testDueDates),
                HeaderFields = Mapper.Map<List<AssessmentFieldDto>>(headerFields)
            };
        }

        public OutputDto_StudentSectionHRSIWReportResults GetHRSIW2SectionReport(InputDto_BAS_SectionReport input)
        {
            // TODO: Change report type magic number to the name of the group, like alpha, word, sound
            var assessment = _dbContext.Assessments
                .Include("FieldCategories")
                .Include("FieldSubcategories")
                .Include("FieldGroups")
                .FirstOrDefault(m => m.Id == input.AssessmentId);

            // now have the assessment and controls loaded, need to get the test results from the table
            // specified in the assessment and turn each result into a StudentResult DTO
            var studentResults = _dbContext.GetBASAssessmentStudentResults(assessment, input.SectionId, input.SchoolYear);
            var testDueDates = _dbContext.TestDueDates.Where(p => p.SchoolStartYear == input.SchoolYear).OrderBy(p => p.DueDate).ToList();

            int currentStudentId = 0;
            StudentSectionHRSIWReportResult currentStudentRecord = null;
            var studentSectionReportResults = new List<StudentSectionHRSIWReportResult>();

            // group results by Test Due Date ID
            foreach (var studentResult in studentResults)
            {
                // create a new student record
                if (currentStudentId != studentResult.StudentId)
                {
                    currentStudentId = studentResult.StudentId;
                    currentStudentRecord = new StudentSectionHRSIWReportResult()
                    {
                        ClassId = studentResult.ClassId,
                        StaffId = studentResult.StaffId,
                        StudentId = studentResult.StudentId,
                        FirstName = studentResult.FirstName,
                        LastName = studentResult.LastName,
                        MiddleName = studentResult.MiddleName
                    };

                    studentSectionReportResults.Add(currentStudentRecord);
                }

                // convert assessmentfieldresults into CAPFieldResults... combine comment and checkbox into a single field
                List<HRSIWFieldResult> hrsiwFieldResults = new List<HRSIWFieldResult>();
                // the TOTAL field is now in fieldresults, need to filter it out by DBColumn... not sure if i like this hard-coded "magic value"
                foreach (var field in studentResult.FieldResults.Where(p => p.DbColumn != null && p.DbColumn.Substring(0, 6) == "column"))
                {

                    var currentFieldResult = new HRSIWFieldResult()
                    {
                        DbColumn = field.DbColumn,
                        Checked = field.BoolValue
                    };
                    hrsiwFieldResults.Add(currentFieldResult);
                }

                var commentResult = studentResult.FieldResults.First(p => p.DbColumn == "comments");

                // just add the field results to the proper collection
                currentStudentRecord.FieldResultsByTestDueDate.Add(new HRSIWFieldResultByTDD()
                {
                    TDDID = studentResult.TestDueDateId.Value,
                    FieldResults = hrsiwFieldResults,
                    Comments = commentResult.StringValue
                });
            }

            List<AssessmentField> headerFields = new List<AssessmentField>();
            // set up properly ordered header fields
            foreach (var field in assessment.Fields)
            {
                // hack for Blends and Digraphs assessment
                if(assessment.AssessmentName == "Blends and Digraphs")
                {
                    // don't care about page
                    if (field.DatabaseColumn != null && field.DatabaseColumn.Substring(0, 6) == "column")
                    {
                        headerFields.Add(field);
                    }
                } else
                {
                    if (field.DatabaseColumn != null && field.DatabaseColumn.Substring(0, 6) == "column" && field.Page == input.HRSFormId2)
                    {
                        headerFields.Add(field);
                    }
                }
            }

            headerFields = headerFields.OrderBy(p => p.FieldOrder).ToList();

            // now add summarized record with "date" and X markers and set currenttotalscore
            // loop over the fields to get the group ID only once
            foreach (var studentReportResult in studentSectionReportResults)
            {
                // create the summary list for each student
                var summaryResults = new List<HRSIWSummaryFieldResult>();
                studentReportResult.SummaryFieldResults = summaryResults;

                foreach (var field in headerFields)
                {
                    // every field will have a summary result
                    var newSummaryResult = new HRSIWSummaryFieldResult() { DbColumn = field.DatabaseColumn };
                    summaryResults.Add(newSummaryResult);

                    foreach (var tdd in testDueDates)
                    {
                        var fieldResultsForTDD = studentReportResult.FieldResultsByTestDueDate.FirstOrDefault(p => p.TDDID == tdd.Id);

                        // if we have results for this TDD for this student
                        if (fieldResultsForTDD != null)
                        {

                            // get the field result for the current field we are looking at
                            var fieldResultForCurrentField = fieldResultsForTDD.FieldResults.FirstOrDefault(p => p.DbColumn == field.DatabaseColumn);

                            // check to see if we've already set a date for this fieldResult
                            var existingResult = summaryResults.FirstOrDefault(p => p.CellColorDate != null && p.DbColumn == field.DatabaseColumn);

                            // if the current result for the current due date and field is checked
                            if (fieldResultForCurrentField.Checked.HasValue && fieldResultForCurrentField.Checked.Value)
                            {
                                // if this result was set before, dont change it just set the new color
                                if (existingResult == null)
                                {
                                    newSummaryResult.CellColorDate = tdd.DueDate.Value;
                                }
                                else // fringe case!! if it was colored before and ther is an X, we've just gotten rid of it
                                {
                                    if (existingResult.XColorDate.HasValue)
                                    {
                                        existingResult.CellColorDate = tdd.DueDate.Value;
                                        existingResult.XColorDate = null;
                                    }
                                }
                            }
                            else
                            {
                                // current TDD for this field is NOT checked... we need to set the "X" and thus the previouscheck if it WAS set before
                                if (existingResult != null && existingResult.CellColorDate.HasValue)
                                {
                                    existingResult.XColorDate = tdd.DueDate;
                                }
                            }
                        }
                    }
                }
            }

            studentSectionReportResults = studentSectionReportResults.OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToList();

            return new OutputDto_StudentSectionHRSIWReportResults()
            {
                StudentSectionReportResults = studentSectionReportResults,
                Assessment = Mapper.Map<AssessmentDto>(assessment),
                TestDueDates = Mapper.Map<List<TestDueDateDto>>(testDueDates),
                HeaderFields = Mapper.Map<List<AssessmentFieldDto>>(headerFields)
            };
        }

        public OutputDto_StudentSectionHRSIWReportResults GetHRSIW3SectionReport(InputDto_BAS_SectionReport input)
        {
            // TODO: Change report type magic number to the name of the group, like alpha, word, sound
            var assessment = _dbContext.Assessments
                .Include("FieldCategories")
                .Include("FieldSubcategories")
                .Include("FieldGroups")
                .FirstOrDefault(m => m.Id == input.AssessmentId);

            // now have the assessment and controls loaded, need to get the test results from the table
            // specified in the assessment and turn each result into a StudentResult DTO
            var studentResults = _dbContext.GetBASAssessmentStudentResults(assessment, input.SectionId, input.SchoolYear);
            var testDueDates = _dbContext.TestDueDates.Where(p => p.SchoolStartYear == input.SchoolYear).OrderBy(p => p.DueDate).ToList();

            int currentStudentId = 0;
            StudentSectionHRSIWReportResult currentStudentRecord = null;
            var studentSectionReportResults = new List<StudentSectionHRSIWReportResult>();

            // group results by Test Due Date ID
            foreach (var studentResult in studentResults)
            {
                // create a new student record
                if (currentStudentId != studentResult.StudentId)
                {
                    currentStudentId = studentResult.StudentId;
                    currentStudentRecord = new StudentSectionHRSIWReportResult()
                    {
                        ClassId = studentResult.ClassId,
                        StaffId = studentResult.StaffId,
                        StudentId = studentResult.StudentId,
                        FirstName = studentResult.FirstName,
                        LastName = studentResult.LastName,
                        MiddleName = studentResult.MiddleName
                    };

                    studentSectionReportResults.Add(currentStudentRecord);
                }

                // convert assessmentfieldresults into CAPFieldResults... combine comment and checkbox into a single field
                List<HRSIWFieldResult> hrsiwFieldResults = new List<HRSIWFieldResult>();
                // the TOTAL field is now in fieldresults, need to filter it out by DBColumn... not sure if i like this hard-coded "magic value"
                foreach (var field in studentResult.FieldResults.Where(p => p.DbColumn != null && p.DbColumn.Substring(0, 3) == "col"))
                {

                    var currentFieldResult = new HRSIWFieldResult()
                    {
                        DbColumn = field.DbColumn,
                        Checked = field.BoolValue
                    };
                    hrsiwFieldResults.Add(currentFieldResult);
                }

                var commentResult = studentResult.FieldResults.First(p => p.DbColumn == "comments");

                // just add the field results to the proper collection
                currentStudentRecord.FieldResultsByTestDueDate.Add(new HRSIWFieldResultByTDD()
                {
                    TDDID = studentResult.TestDueDateId.Value,
                    FieldResults = hrsiwFieldResults,
                    Comments = commentResult.StringValue
                });
            }

            List<AssessmentField> headerFields = new List<AssessmentField>();
            // set up properly ordered header fields
            foreach (var field in assessment.Fields)
            {

                if (field.DatabaseColumn != null && field.DatabaseColumn.Substring(0, 3) == "col" && field.Page == input.HRSFormId3)
                {
                    headerFields.Add(field);
                }
            }

            headerFields = headerFields.OrderBy(p => p.FieldOrder).ToList();

            // now add summarized record with "date" and X markers and set currenttotalscore
            // loop over the fields to get the group ID only once
            foreach (var studentReportResult in studentSectionReportResults)
            {
                // create the summary list for each student
                var summaryResults = new List<HRSIWSummaryFieldResult>();
                studentReportResult.SummaryFieldResults = summaryResults;

                foreach (var field in headerFields)
                {
                    // every field will have a summary result
                    var newSummaryResult = new HRSIWSummaryFieldResult() { DbColumn = field.DatabaseColumn };
                    summaryResults.Add(newSummaryResult);

                    foreach (var tdd in testDueDates)
                    {
                        var fieldResultsForTDD = studentReportResult.FieldResultsByTestDueDate.FirstOrDefault(p => p.TDDID == tdd.Id);

                        // if we have results for this TDD for this student
                        if (fieldResultsForTDD != null)
                        {

                            // get the field result for the current field we are looking at
                            var fieldResultForCurrentField = fieldResultsForTDD.FieldResults.FirstOrDefault(p => p.DbColumn == field.DatabaseColumn);

                            // check to see if we've already set a date for this fieldResult
                            var existingResult = summaryResults.FirstOrDefault(p => p.CellColorDate != null && p.DbColumn == field.DatabaseColumn);

                            // if the current result for the current due date and field is checked
                            if (fieldResultForCurrentField.Checked.HasValue && fieldResultForCurrentField.Checked.Value)
                            {
                                // if this result was set before, dont change it just set the new color
                                if (existingResult == null)
                                {
                                    newSummaryResult.CellColorDate = tdd.DueDate.Value;
                                }
                                else // fringe case!! if it was colored before and ther is an X, we've just gotten rid of it
                                {
                                    if (existingResult.XColorDate.HasValue)
                                    {
                                        existingResult.CellColorDate = tdd.DueDate.Value;
                                        existingResult.XColorDate = null;
                                    }
                                }
                            }
                            else
                            {
                                // current TDD for this field is NOT checked... we need to set the "X" and thus the previouscheck if it WAS set before
                                if (existingResult != null && existingResult.CellColorDate.HasValue)
                                {
                                    existingResult.XColorDate = tdd.DueDate;
                                }
                            }
                        }
                    }
                }
            }

            studentSectionReportResults = studentSectionReportResults.OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToList();

            return new OutputDto_StudentSectionHRSIWReportResults()
            {
                StudentSectionReportResults = studentSectionReportResults,
                Assessment = Mapper.Map<AssessmentDto>(assessment),
                TestDueDates = Mapper.Map<List<TestDueDateDto>>(testDueDates),
                HeaderFields = Mapper.Map<List<AssessmentFieldDto>>(headerFields)
            };
        }

        private async Task<string> GetStudentImage(string studentId, int schoolYear)
        {

            var district = _loginContext.Districts.First(p => p.Id == _currentUser.DistrictId);

            var fileName = schoolYear.ToString() + "/" + district.ProfilePicturePrefix + studentId.ToString() + district.ProfilePictureExtension;

            var blob = _container.GetBlockBlobReference(fileName);
            // check if file exists first
            if (! await blob.ExistsAsync())
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

        public async Task<OutputDto_ClassRosterReportResults> GetKNTCSectionReport(InputDto_SectionReport_ByTdd input)
        {            
            // TODO: Change report type magic number to the name of the group, like alpha, word, sound
            var assessment = _dbContext.Assessments
                .Include("FieldCategories")
                .Include("FieldSubcategories")
                .Include("FieldGroups")
                .FirstOrDefault(m => m.Id == input.AssessmentId);

            var studentAttributes = new List<JObject>();
            var tdd = _dbContext.TestDueDates.First(p => p.Id == input.BenchmarkDateId);

            var students = _dbContext.StudentSections.Where(p => p.Section.Id == input.SectionId && p.Student.IsActive != false)
            .Include(x => x.Student.StudentAttributeDatas).Select(p => p.Student);

            foreach(var student in students)
            {
                var newStudentAttributes = new JObject();
                newStudentAttributes.Add("StudentId", student.Id);
                foreach (var studentAttributeData in student.StudentAttributeDatas.Where(p => p.AttributeID != 4)) // SPEDLabels are separate
                {
                    newStudentAttributes.Add(studentAttributeData.AttributeID.ToString(), JToken.FromObject(studentAttributeData.AttributeValueID));
                }
                studentAttributes.Add(newStudentAttributes);
            }

            // now have the assessment and controls loaded, need to get the test results from the table
            // specified in the assessment and turn each result into a StudentResult DTO
            var studentResults = _dbContext.GetAssessmentStudentResults(assessment, input.SectionId, input.BenchmarkDateId, DateTime.Now, true);
            
            // add imageUrl for students
            foreach(var studentResult in studentResults)
            {
                studentResult.ImageUrl = await GetStudentImage(studentResult.StudentIdentifier, input.SchoolYear);
                foreach(var result in studentResult.FieldResults)
                {
                    if(result.DbColumn == "guidedreadinggroup")
                    {
                        studentResult.GuidedReadingGroupId = result.IntValue;
                    }
                }
            }

      
            // get interventions

            // headerFields = headerFields.OrderBy(p => p.SubCategory.SortOrder).ThenBy(j => j.FieldOrder).ToList();
            var interventionRecords = GetActiveStudentInterventionsBySectionId(input.SectionId, tdd.DueDate.Value);
            var studentServices = GetStudentSPEDLabelsBySectionId(input.SectionId);

            return new OutputDto_ClassRosterReportResults()
            {
                StudentSectionReportResults = studentResults,
                Assessment = Mapper.Map<AssessmentDto>(assessment),
                InterventionRecords = interventionRecords,
                StudentServices = studentServices,
                HeaderFields = Mapper.Map<List<AssessmentFieldDto>>(assessment.Fields),
                StudentAttributes = studentAttributes

            };
        }

        public OutputDto_StudentSectionFPReportResults GetFPSectionReport(InputDto_BAS_SectionReport input)
        {
            // TODO: Change report type magic number to the name of the group, like alpha, word, sound
            var assessment = _dbContext.Assessments
                .Include("FieldCategories")
                .Include("FieldSubcategories")
                .Include("FieldGroups")
                .FirstOrDefault(m => m.Id == input.AssessmentId);

            // get section so that we know which grade
            var gradeForSection = _dbContext.Sections.First(p => p.Id == input.SectionId).GradeID;

            // get current TDD
            var currentTddPeriod = GetCurrentBenchmarkDate(input.SchoolYear, DateTime.Now)?.TestLevelPeriodID;

            // get ALL benchmarks
            var benchmarksForYear = _dbContext.DistrictBenchmarks.Where(p =>
                p.AssessmentID == assessment.Id &&
                p.AssessmentField == "FPValueID").ToList();

            var distinctGradesFromBenchmarks = benchmarksForYear.Select(p => p.GradeID).Distinct();

            var benchmarksByGrade = new List<BenchmarksByGrade>();
            foreach (var uniqueGrade in distinctGradesFromBenchmarks)
            {
                // set the grade
                var currentBenchmarkByGrade = new BenchmarksByGrade { GradeId = uniqueGrade };

                currentBenchmarkByGrade.EndOfYearBenchmark = Mapper.Map<DistrictBenchmarkDto>(benchmarksForYear.Where(p => p.GradeID == uniqueGrade).OrderByDescending(p => p.TestLevelPeriodID).FirstOrDefault());
                currentBenchmarkByGrade.StartOfYearBenchmark = Mapper.Map<DistrictBenchmarkDto>(benchmarksForYear.Where(p => p.GradeID == uniqueGrade).OrderBy(p => p.TestLevelPeriodID).FirstOrDefault());
                currentBenchmarkByGrade.TargetZone = Mapper.Map<DistrictBenchmarkDto>(benchmarksForYear.Where(p => p.GradeID == uniqueGrade).FirstOrDefault(p => p.TestLevelPeriodID == currentTddPeriod));

                benchmarksByGrade.Add(currentBenchmarkByGrade);
            }
            var topBenchmark = benchmarksForYear.OrderByDescending(p => p.TestLevelPeriodID).FirstOrDefault(p => p.GradeID == gradeForSection);
            var bottomBenchmark = benchmarksForYear.OrderBy(p => p.TestLevelPeriodID).FirstOrDefault(p => p.GradeID == gradeForSection);
            var currentBenchmark = benchmarksForYear.FirstOrDefault(p => p.TestLevelPeriodID == currentTddPeriod && p.GradeID == gradeForSection);

            // now have the assessment and controls loaded, need to get the test results from the table
            // specified in the assessment and turn each result into a StudentResult DTO
            var studentResults = _dbContext.GetBASAssessmentStudentResults(assessment, input.SectionId, input.SchoolYear);
            var testDueDates = _dbContext.TestDueDates.Where(p => p.SchoolStartYear == input.SchoolYear).OrderBy(p => p.DueDate).ToList(); // TODO: have this passed in

            int currentStudentId = 0;
            StudentSectionFPReportResult currentStudentRecord = null;
            var studentSectionReportResults = new List<StudentSectionFPReportResult>();

            // group results by Test Due Date ID
            foreach (var studentResult in studentResults)
            {
                // create a new student record
                if (currentStudentId != studentResult.StudentId)
                {
                    currentStudentId = studentResult.StudentId;
                    currentStudentRecord = new StudentSectionFPReportResult()
                    {
                        ClassId = studentResult.ClassId, // TODO: do we really need classID?
                        StaffId = studentResult.StaffId,
                        StudentId = studentResult.StudentId,
                        FirstName = studentResult.FirstName,
                        LastName = studentResult.LastName,
                        MiddleName = studentResult.MiddleName,
                        GradeId = studentResult.GradeId
                    };

                    studentSectionReportResults.Add(currentStudentRecord);
                }

                // convert assessmentfieldresults into FPFieldResults... combine comment and checkbox into a single field
                List<FPFieldResult> fpFieldResults = new List<FPFieldResult>();
                var comment = studentResult.FieldResults.FirstOrDefault(p => p.DbColumn == "Comments");
                var fpValueId = studentResult.FieldResults.FirstOrDefault(p => p.DbColumn == "FPValueID");
                //  only need the FPValueID column and comment column
                fpFieldResults.Add(new FPFieldResult()
                {
                    DbColumn = "FPValueID",
                    Comment = comment == null ? String.Empty : comment.StringValue,
                    FPValueId = fpValueId.IntValue // TODO: Ensure FPValueID is ALWAYS set... check in DB
                });

                // just add the field results to the proper collection
                currentStudentRecord.FieldResultsByTestDueDate.Add(new FPFieldResultByTDD()
                {
                    TDDID = studentResult.TestDueDateId.Value,
                    FieldResults = fpFieldResults
                });
            }

            // get interventions

            var fpComparisonScale = _dbContext.FPComparisons.OrderByDescending(p => p.FPOrder).ToList();

            // now add summarized record with "date" and X markers and set currenttotalscore
            // loop over the fields to get the group ID only once
            foreach (var studentReportResult in studentSectionReportResults)
            {
                // create the summary list for each student
                var summaryResults = new List<FPSummaryFieldResult>();
                studentReportResult.SummaryFieldResults = summaryResults;

                foreach (var fp in fpComparisonScale)
                {
                    // every fpscaleitem will have a summary result for each student
                    var newSummaryResult = new FPSummaryFieldResult() { FPValueId = fp.FPID };
                    summaryResults.Add(newSummaryResult);

                    foreach (var tdd in testDueDates)
                    {
                        // see if there is any data recorded for this student/testduedate
                        var fpScaleResultsForTDD = studentReportResult.FieldResultsByTestDueDate.FirstOrDefault(p => p.TDDID == tdd.Id);

                        // if we have results for this TDD for this student
                        if (fpScaleResultsForTDD != null)
                        {
                            // get the field result for the current FPID we are looking at
                            var fieldResultForCurrentFPScale = fpScaleResultsForTDD.FieldResults.FirstOrDefault(p => p.FPValueId >= fp.FPID);

                            // if this CELL should be colored (TDD/Scale/Student)
                            if (fieldResultForCurrentFPScale != null)
                            {
                                // this is the max value for current studentresult/tdd
                                var maxValueForFieldResult = fpScaleResultsForTDD.FieldResults.OrderByDescending(p => p.FPValueId).First().FPValueId;

                                // check to see if we've already set a date for this fieldResult
                                var existingResult = summaryResults.FirstOrDefault(p => p.CellColorDate != null && p.FPValueId == fp.FPID);

                                // if this result was set before, dont change it just set the new color
                                if (existingResult == null)
                                {
                                    newSummaryResult.CellColorDate = tdd.DueDate.Value;
                                }
                                if(existingResult != null && existingResult.CellColorDate.HasValue && existingResult.FPValueId == maxValueForFieldResult)
                                {
                                    existingResult.XColorDates.Add(tdd.DueDate);
                                }
                            }
                        }
                    }
                }
            }

            // headerFields = headerFields.OrderBy(p => p.SubCategory.SortOrder).ThenBy(j => j.FieldOrder).ToList();
            studentSectionReportResults = studentSectionReportResults.OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToList();
            var interventionRecords = GetStudentInterventionsBySectionId(input.SectionId);
            var previousGradeScores = GetStudentPreviousGradeFPScoreBySectionId(input.SectionId);
            var studentServices = GetStudentSPEDLabelsBySectionId(input.SectionId);

            return new OutputDto_StudentSectionFPReportResults()
            {
               StudentSectionReportResults = studentSectionReportResults,
                Assessment = Mapper.Map<AssessmentDto>(assessment),
                TestDueDates = Mapper.Map<List<TestDueDateDto>>(testDueDates),
                Scale = fpComparisonScale,
                EndOfYearBenchmark = Mapper.Map<DistrictBenchmarkDto>(topBenchmark),
                StartOfYearBenchmark = Mapper.Map<DistrictBenchmarkDto>(bottomBenchmark),
                TargetZone = Mapper.Map<DistrictBenchmarkDto>(currentBenchmark),
                InterventionRecords = interventionRecords,
                PreviousGradeScores = previousGradeScores,
                StudentServices = studentServices,
                BenchmarksByGrade = benchmarksByGrade
            };
        }

        public OutputDto_StudentSectionWVReportResults GetWVSectionReport(InputDto_BAS_SectionReport input)
        {

            var result = new OutputDto_StudentSectionWVReportResults();
            // TODO: Change report type magic number to the name of the group, like alpha, word, sound
            var assessment = _dbContext.Assessments
                .Include("FieldCategories")
                .Include("FieldSubcategories")
                .Include("FieldGroups")
                .FirstOrDefault(m => m.Id == input.AssessmentId);

            // get section so that we know which grade
            var gradeForSection = _dbContext.Sections.First(p => p.Id == input.SectionId).GradeID;

            // get current TDD
            var currentTddPeriod = GetCurrentBenchmarkDate(input.SchoolYear, DateTime.Now)?.TestLevelPeriodID;


            // now have the assessment and controls loaded, need to get the test results from the table
            // specified in the assessment and turn each result into a StudentResult DTO
            var studentResults = _dbContext.GetBASAssessmentStudentResults(assessment, input.SectionId, input.SchoolYear);
            var testDueDates = _dbContext.TestDueDates.Where(p => p.SchoolStartYear == input.SchoolYear).OrderBy(p => p.DueDate).ToList(); // TODO: have this passed in

            int currentStudentId = 0;
            StudentSectionWVReportResult currentStudentRecord = null;
            var studentSectionReportResults = new List<StudentSectionWVReportResult>();

            // group results by Test Due Date ID
            foreach (var studentResult in studentResults)
            {
                // create a new student record
                if (currentStudentId != studentResult.StudentId)
                {
                    currentStudentId = studentResult.StudentId;
                    currentStudentRecord = new StudentSectionWVReportResult()
                    {
                        ClassId = studentResult.ClassId, // TODO: do we really need classID?
                        StaffId = studentResult.StaffId,
                        StudentId = studentResult.StudentId,
                        FirstName = studentResult.FirstName,
                        LastName = studentResult.LastName,
                        MiddleName = studentResult.MiddleName
                    };

                    studentSectionReportResults.Add(currentStudentRecord);
                }

                // convert assessmentfieldresults into WVFieldResults... combine comment and checkbox into a single field
                List<WVFieldResult> wvFieldResults = new List<WVFieldResult>();
                var comment = studentResult.FieldResults.FirstOrDefault(p => p.DbColumn == "Comments");
                var wordsCorrect = studentResult.FieldResults.FirstOrDefault(p => p.DbColumn == "WordsCorrect");
                //  only need the FPValueID column and comment column
                wvFieldResults.Add(new WVFieldResult()
                {
                    DbColumn = "WordsCorrect",
                    Comment = comment == null ? String.Empty : comment.StringValue,
                    WordsCorrect = wordsCorrect.IntValue // TODO: Ensure FPValueID is ALWAYS set... check in DB
                });

                // just add the field results to the proper collection
                currentStudentRecord.FieldResultsByTestDueDate.Add(new WVFieldResultByTDD()
                {
                    TDDID = studentResult.TestDueDateId.Value,
                    FieldResults = wvFieldResults
                });
            }

            // now add summarized record with "date" and X markers and set currenttotalscore
            // loop over the fields to get the group ID only once
            foreach (var studentReportResult in studentSectionReportResults)
            {
                // create the summary list for each student
                var summaryResults = new List<WVSummaryFieldResult>();
                studentReportResult.SummaryFieldResults = summaryResults;

                foreach (var scaleValue in result.Scale)
                {
                    // every fpscaleitem will have a summary result for each student
                    var newSummaryResult = new WVSummaryFieldResult() { WordsCorrect = scaleValue.id };
                    summaryResults.Add(newSummaryResult);

                    foreach (var tdd in testDueDates)
                    {
                        // see if there is any data recorded for this student/testduedate
                        var fpScaleResultsForTDD = studentReportResult.FieldResultsByTestDueDate.FirstOrDefault(p => p.TDDID == tdd.Id);

                        // if we have results for this TDD for this student
                        if (fpScaleResultsForTDD != null)
                        {
                            // get the field result for the current FPID we are looking at
                            var fieldResultForCurrentFPScale = fpScaleResultsForTDD.FieldResults.FirstOrDefault(p => p.WordsCorrect >= scaleValue.id);

                            // if this CELL should be colored (TDD/Scale/Student)
                            if (fieldResultForCurrentFPScale != null)
                            {
                                // this is the max value for current studentresult/tdd
                                var maxValueForFieldResult = fpScaleResultsForTDD.FieldResults.OrderByDescending(p => p.WordsCorrect).First().WordsCorrect;

                                // check to see if we've already set a date for this fieldResult
                                var existingResult = summaryResults.FirstOrDefault(p => p.CellColorDate != null && p.WordsCorrect == scaleValue.id);

                                // if this result was set before, dont change it just set the new color
                                if (existingResult == null)
                                {
                                    newSummaryResult.CellColorDate = tdd.DueDate.Value;
                                }
                                if (existingResult != null && existingResult.CellColorDate.HasValue && existingResult.WordsCorrect == maxValueForFieldResult)
                                {
                                    existingResult.XColorDates.Add(tdd.DueDate);
                                }
                            }
                        }
                    }
                }
            }

            // headerFields = headerFields.OrderBy(p => p.SubCategory.SortOrder).ThenBy(j => j.FieldOrder).ToList();
            studentSectionReportResults = studentSectionReportResults.OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToList();

            result.StudentSectionReportResults = studentSectionReportResults;
            result.Assessment = Mapper.Map<AssessmentDto>(assessment);
            result.TestDueDates = Mapper.Map<List<TestDueDateDto>>(testDueDates);

            return result;
        }

        public List<StudentSPEDLabel> GetStudentSPEDLabelsBySectionId(int sectionId)
        {
            var records = _dbContext.Database.SqlQuery<StudentSPEDLabel>(string.Format("dbo.[nset_sp_GetStudentSPEDLabelsBySectionID] {0}", sectionId));

            return records.ToList();
        }

        public List<StudentInterventionReportRecord> GetStudentInterventionsBySectionId(int sectionId)
        {
            var records = _dbContext.Database.SqlQuery<StudentInterventionReportRecord>(string.Format("dbo.[nset_sp_GetStudentInterventionsBySectionID] {0}", sectionId)).ToList();

            var attendanceCalculator = new AttendanceCalculator(_dbContext);
            // don't comput for non-cycle days and "none" status... irrelevant
            var distinctAttendanceReasons = _dbContext.AttendanceReasons.Where(p => p.Reason == "Intervention Delivered" || p.Reason == "Make-Up Lesson").ToList();
            var distinctAttendanceReasonSummaries = distinctAttendanceReasons.Select(p => new AttendanceStatusSummary { Count = 0, StatusLabel = p.Reason }).ToList();

            records.Each(p =>
            {
                var stintStartEnd = _dbContext.StudentInterventionGroups.FirstOrDefault(g => g.Id == p.StintId);
                var start = DateTime.Now.AddYears(-50);
                var end = DateTime.Now.AddYears(100);
                if (stintStartEnd != null)
                {
                    start = stintStartEnd.StartDate;
                    end = stintStartEnd.EndDate ?? end;
                }
                var attendanceData = _dbContext.InterventionAttendances
                .Include(g => g.AttendanceReason)
                .Where(g => g.ClassStartEndID == p.StintId && g.AttendanceDate >= start && g.AttendanceDate <= end).OrderByDescending(g => g.AttendanceDate).ToList();
                attendanceCalculator.SetAttendanceStatuses(distinctAttendanceReasons, distinctAttendanceReasonSummaries, attendanceData, p.StudentId, p.StintId);

                // add total for met and made up
                p.NumberOfLessons = distinctAttendanceReasonSummaries.First(j => j.StatusLabel == "Intervention Delivered").Count + distinctAttendanceReasonSummaries.First(j => j.StatusLabel == "Make-Up Lesson").Count;
            });

            return records.ToList();
        }

        public List<StudentInterventionReportRecord> GetActiveStudentInterventionsBySectionId(int sectionId, DateTime dateInQuestion)
        {
            var records = _dbContext.Database.SqlQuery<StudentInterventionReportRecord>(string.Format("dbo.[nset_sp_GetActiveStudentInterventionsBySectionID] {0}, '{1}'", sectionId, dateInQuestion.ToShortDateString())).ToList();

            var attendanceCalculator = new AttendanceCalculator(_dbContext);
            // don't comput for non-cycle days and "none" status... irrelevant
            var distinctAttendanceReasons = _dbContext.AttendanceReasons.Where(p => p.Reason == "Intervention Delivered" || p.Reason == "Make-Up Lesson").ToList();
            var distinctAttendanceReasonSummaries = distinctAttendanceReasons.Select(p => new AttendanceStatusSummary { Count = 0, StatusLabel = p.Reason }).ToList();

            records.Each(p =>
            {
                var stintStartEnd = _dbContext.StudentInterventionGroups.FirstOrDefault(g => g.Id == p.StintId);
                var start = DateTime.Now.AddYears(-50);
                var end = DateTime.Now.AddYears(100);
                if (stintStartEnd != null)
                {
                    start = stintStartEnd.StartDate;
                    end = stintStartEnd.EndDate ?? end;
                }
                var attendanceData = _dbContext.InterventionAttendances
                .Include(g => g.AttendanceReason)
                .Where(g => g.ClassStartEndID == p.StintId && g.AttendanceDate >= start && g.AttendanceDate <= end).OrderByDescending(g => g.AttendanceDate).ToList();
                attendanceCalculator.SetAttendanceStatuses(distinctAttendanceReasons, distinctAttendanceReasonSummaries, attendanceData, p.StudentId, p.StintId);

                // add total for met and made up
                p.NumberOfLessons = distinctAttendanceReasonSummaries.First(j => j.StatusLabel == "Intervention Delivered").Count + distinctAttendanceReasonSummaries.First(j => j.StatusLabel == "Make-Up Lesson").Count;
            });

            return records.ToList();
        }

        public List<StudentPreviousGradeReportRecord> GetStudentPreviousGradeFPScoreBySectionId(int sectionId)
        {
            var records = _dbContext.Database.SqlQuery<StudentPreviousGradeReportRecord>(string.Format("dbo.[rpt_GetPreviousFPScoreForSection] {0}", sectionId));

            return records.ToList();
        }

        public OutputDto_StudentSectionLIDReportResults GetLIDSectionReport(InputDto_LetterIDReport input)
        {
            // TODO: Change report type magic number to the name of the group, like alpha, word, sound
            var assessment = _dbContext.Assessments
                .Include("FieldCategories")
                .Include("FieldSubcategories")
                .Include("FieldGroups")
                .FirstOrDefault(m => m.Id == input.AssessmentId);

            var categoryIdForReportType = _dbContext.AssessmentFieldCategories.FirstOrDefault(p => p.DisplayName == input.ReportType && p.AssessmentId == input.AssessmentId);
            List<string> dbColumnsForCategory = null;
            int subStringCount = 3;

            if (categoryIdForReportType == null)
            {
                subStringCount = 7;
                dbColumnsForCategory = _dbContext.AssessmentFields.Where(p => p.AssessmentId == input.AssessmentId && p.CategoryId != null && p.DatabaseColumn != null).Select(j => j.DatabaseColumn.Substring(subStringCount)).Distinct().ToList();
            }
            else
            {
                dbColumnsForCategory = _dbContext.AssessmentFields.Where(p => p.AssessmentId == input.AssessmentId && p.CategoryId == categoryIdForReportType.Id).Select(j => j.DatabaseColumn.Substring(subStringCount)).Distinct().ToList();
            }


            // now have the assessment and controls loaded, need to get the test results from the table
            // specified in the assessment and turn each result into a StudentResult DTO
            var studentResults = _dbContext.GetBASAssessmentStudentResults(assessment, input.SectionId, input.SchoolYear);
            var testDueDates = _dbContext.TestDueDates.Where(p => p.SchoolStartYear == input.SchoolYear).OrderBy(p => p.DueDate).ToList();

            int currentStudentId = 0;
            StudentSectionLIDReportResult currentStudentRecord = null;
            var studentSectionReportResults = new List<StudentSectionLIDReportResult>();

            // group results by Test Due Date ID
            foreach (var studentResult in studentResults)
            {
                // create a new student record
                if (currentStudentId != studentResult.StudentId)
                {
                    currentStudentId = studentResult.StudentId;
                    currentStudentRecord = new StudentSectionLIDReportResult()
                    {
                        ClassId = studentResult.ClassId,
                        StaffId = studentResult.StaffId,
                        StudentId = studentResult.StudentId,
                        FirstName = studentResult.FirstName,
                        LastName = studentResult.LastName,
                        MiddleName = studentResult.MiddleName
                    };

                    studentSectionReportResults.Add(currentStudentRecord);
                }

                // convert assessmentfieldresults into CAPFieldResults... combine comment and checkbox into a single field
                List<LIDFieldResult> lidFieldResults = new List<LIDFieldResult>();
                // TODO: the TOTAL field is now in fieldresults, need to filter it out by DBColumn... not sure if i like this hard-coded "magic value"
                // TODO: do this better...
                foreach (var field in studentResult.FieldResults.Where(p => p.DbColumn != "totalAlphabetResponse" && p.DbColumn != "totalSoundResponse" && p.DbColumn != "totalWordResponse" && p.DbColumn != "totalOverallResponse" && p.DbColumn != "unknownLetters" && p.DbColumn != "comments" && dbColumnsForCategory.Contains(p.DbColumn.Substring(subStringCount))))
                {
                    var dbField = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn);

                    var fieldSuffix = field.DbColumn.Substring(subStringCount);
                    var fieldPrefix = field.DbColumn.Substring(0, 3);

                    // if this field has already been added
                    var currentFieldResult = lidFieldResults.FirstOrDefault(p => p.DbColumn == fieldSuffix);
                    if (currentFieldResult == null)
                    {
                        var groupId = dbField.GroupId;
                        currentFieldResult = new LIDFieldResult()
                        {
                            DbColumn = fieldSuffix,
                            GroupId = groupId.Value
                        };
                        lidFieldResults.Add(currentFieldResult);
                    }

                    if (fieldPrefix.ToLower() == "chk")
                    {
                        if (input.ReportType == "Overall Response")
                        {
                            // if its not already checked, set it to whatever's there, if it is checked, leave it checked
                            if (currentFieldResult.Checked == null)
                            {
                                currentFieldResult.Checked = field.BoolValue;
                            }
                            else if (currentFieldResult.Checked.HasValue)
                            {
                                if (!currentFieldResult.Checked.Value)
                                {
                                    currentFieldResult.Checked = field.BoolValue;
                                }
                            }
                        }
                        else
                        {
                            currentFieldResult.Checked = field.BoolValue;
                        }
                    }
                    else if (fieldPrefix.ToLower() == "txt")
                    {
                        if (input.ReportType == "Overall Response")
                        {
                            if (String.IsNullOrWhiteSpace(currentFieldResult.Comment))
                            {
                                currentFieldResult.Comment = String.IsNullOrWhiteSpace(field.StringValue) ? "" : dbField.Category.DisplayName + " : " + field.StringValue.Replace("'", "&apos;").Replace("\"", "&quot;");

                            }
                            else
                            {
                                currentFieldResult.Comment += String.IsNullOrWhiteSpace(field.StringValue) ? "" : dbField.Category.DisplayName + " : " + field.StringValue.Replace("'", "&apos;").Replace("\"", "&quot;");

                            }
                        }
                        else
                        {
                            currentFieldResult.Comment = String.IsNullOrWhiteSpace(field.StringValue) ? "" : field.StringValue.Replace("'", "&apos;").Replace("\"", "&quot;");
                        }
                    }
                }


                // just add the field results to the proper collection
                currentStudentRecord.FieldResultsByTestDueDate.Add(new LIDFieldResultByTDD()
                {
                    TDDID = studentResult.TestDueDateId.Value,
                    FieldResults = lidFieldResults
                });
            }

            List<AssessmentField> headerFields = new List<AssessmentField>();
            // set up properly ordered header fields
            foreach (var field in assessment.Fields)
            {
                if (field.Category != null && field.FieldType == "Label")
                {
                    headerFields.Add(field);
                }
            }



            headerFields = headerFields.OrderBy(p => p.SubCategory.SortOrder).ThenBy(j => j.AltOrder).ToList();

            // now add summarized record with "date" and X markers and set currenttotalscore
            // loop over the fields to get the group ID only once
            foreach (var studentReportResult in studentSectionReportResults)
            {
                // create the summary list for each student
                var summaryResults = new List<LIDSummaryFieldResult>();
                studentReportResult.SummaryFieldResults = summaryResults;

                foreach (var field in headerFields)
                {
                    // every field will have a summary result
                    var newSummaryResult = new LIDSummaryFieldResult() { GroupId = field.GroupId.Value, DbColumn = field.DatabaseColumn };
                    summaryResults.Add(newSummaryResult);

                    foreach (var tdd in testDueDates)
                    {
                        var fieldResultsForTDD = studentReportResult.FieldResultsByTestDueDate.FirstOrDefault(p => p.TDDID == tdd.Id);

                        // if we have results for this TDD for this student
                        if (fieldResultsForTDD != null)
                        {

                            // get the field result for the current field we are looking at
                            // only get the fields for the current category (alpha, word, sound)
                            var fieldResultForCurrentField = fieldResultsForTDD.FieldResults.FirstOrDefault(p => p.GroupId == field.GroupId && dbColumnsForCategory.Contains(p.DbColumn));

                            // check to see if we've already set a date for this fieldResult
                            var existingResult = summaryResults.FirstOrDefault(p => p.CellColorDate != null && p.GroupId == field.GroupId);

                            // if the current result for the current due date and field is checked
                            if (fieldResultForCurrentField.Checked.HasValue && fieldResultForCurrentField.Checked.Value)
                            {
                                // if this result was set before, dont change it just set the new color
                                if (existingResult == null)
                                {
                                    newSummaryResult.CellColorDate = tdd.DueDate.Value;
                                }
                                else // fringe case!! if it was colored before and ther is an X, we've just gotten rid of it
                                {
                                    if (existingResult.XColorDate.HasValue)
                                    {
                                        existingResult.CellColorDate = tdd.DueDate.Value;
                                        existingResult.XColorDate = null;
                                    }
                                }
                            }
                            else
                            {
                                // current TDD for this field is NOT checked... we need to set the "X" and thus the previouscheck if it WAS set before
                                if (existingResult != null && existingResult.CellColorDate.HasValue)
                                {
                                    existingResult.XColorDate = tdd.DueDate;
                                }
                            }
                        }
                    }
                }
            }

            //headerFields = headerFields.OrderBy(p => p.SubCategory.SortOrder).ThenBy(j => j.FieldOrder).ToList();
            studentSectionReportResults = studentSectionReportResults.OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToList();

            return new OutputDto_StudentSectionLIDReportResults()
            {
                StudentSectionReportResults = studentSectionReportResults,
                Assessment = Mapper.Map<AssessmentDto>(assessment),
                TestDueDates = testDueDates,
                HeaderFields = Mapper.Map<List<AssessmentFieldDto>>(headerFields)
            };
        }
    }
}
