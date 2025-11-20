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
using Serilog;
using EntityDto.DTO.Reports;

namespace NorthStar.EF6
{
    public class LineGraphReportService : NSBaseDataService
    {
        public LineGraphReportService(ClaimsIdentity user, string loginConnectionString) : base(user, loginConnectionString)
        {

        }

        public List<AssessmentLookupField> GetVScale(string lookupFieldName)
        {
            return _dbContext.LookupFields.Where(p => p.FieldName == lookupFieldName).OrderBy(p => p.SortOrder).ToList();
        }

        public List<BenchmarkDatesForStudentAndAssessment> GetBenchmarkDatesForIG(int assessmentId, InputDto_GetStudentInterventionGroupLineGraph input)
        {
            var assessment = _dbContext.Assessments.First(p => p.Id == assessmentId);

            //return results.ToList();
            var list = new List<BenchmarkDatesForStudentAndAssessment>();

            using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
            {
                try
                {
                    _dbContext.Database.Connection.Open();
                    command.CommandText = String.Format("EXEC _ns4_GetBenchmarkDatesForInterventionGroupAndAssessment @StudentId={0}, @AssessmentId={1}, @AssessmentField='{2}', @InterventionGroupID={3}, @SchoolStartYear={4}, @AssessmentTable='{5}'", input.StudentId, assessment.Id, input.FieldToRetrieve, input.InterventionGroupId, input.SchoolStartYear, assessment.StorageTable);// TODO: make separate scale for LLI and use it
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = command.Connection.ConnectionTimeout;

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        // load datatable
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            BenchmarkDatesForStudentAndAssessment studentResult = new BenchmarkDatesForStudentAndAssessment();
                            list.Add(studentResult);
                            studentResult.DueDate = (dt.Rows[i]["DueDate"] != DBNull.Value) ? DateTime.Parse(dt.Rows[i]["DueDate"].ToString()) : (DateTime?)null;
                            studentResult.GradeID = (dt.Rows[i]["GradeID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["GradeID"].ToString()) : -1;
                            //studentResult.GradeOrder = (dt.Rows[i]["GradeOrder"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["GradeOrder"].ToString()) : -1;
                            //studentResult.GradeShortName = (dt.Rows[i]["GradeShortName"] != DBNull.Value) ? dt.Rows[i]["GradeShortName"].ToString() : String.Empty;
                            studentResult.Hex = (dt.Rows[i]["Hex"] != DBNull.Value) ? dt.Rows[i]["Hex"].ToString() : String.Empty;
                            //studentResult.SectionID = (dt.Rows[i]["SectionID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["SectionID"].ToString()) : -1;
                            studentResult.TestDueDateID = (dt.Rows[i]["TestDueDateID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["TestDueDateID"].ToString()) : -1;
                            studentResult.TestLevelPeriodID = (dt.Rows[i]["TestLevelPeriodID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["TestLevelPeriodID"].ToString()) : 0;
                            studentResult.TestNumber = (dt.Rows[i]["TestNumber"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["TestNumber"].ToString()) : 0;
                            studentResult.Exceeds = (dt.Rows[i]["Exceeds"] != DBNull.Value) ? Decimal.Parse(dt.Rows[i]["Exceeds"].ToString()) : (decimal?)null;
                            studentResult.Meets = (dt.Rows[i]["Meets"] != DBNull.Value) ? Decimal.Parse(dt.Rows[i]["Meets"].ToString()) : (decimal?)null;
                            studentResult.DoesNotMeet = (dt.Rows[i]["DoesNotMeet"] != DBNull.Value) ? Decimal.Parse(dt.Rows[i]["DoesNotMeet"].ToString()) : (decimal?)null;
                            studentResult.Approaches = (dt.Rows[i]["Approaches"] != DBNull.Value) ? Decimal.Parse(dt.Rows[i]["Approaches"].ToString()) : (decimal?)null;
                        }
                    }

                }
                finally
                {
                    _dbContext.Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }
            return list;
        }
        public List<BenchmarkDatesForStudentAndAssessment> GetBenchmarkDatesForStudentAndAssessment(int assessmentId, int studentId, string fieldName)
        {
            var assessment = _dbContext.Assessments.First(p => p.Id == assessmentId);


            //return results.ToList();
            var list = new List<BenchmarkDatesForStudentAndAssessment>();

            using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
            {
                try
                {
                    _dbContext.Database.Connection.Open();
                    command.CommandText = String.Format("EXEC _ns4_GetBenchmarkDatesForStudentAndAssessment @StudentId={0}, @AssessmentId={1}, @AssessmentField='{2}'", studentId, assessment.Id, fieldName);
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = command.Connection.ConnectionTimeout;

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        // load datatable
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            BenchmarkDatesForStudentAndAssessment studentResult = new BenchmarkDatesForStudentAndAssessment();
                            list.Add(studentResult);
                            studentResult.DueDate = (dt.Rows[i]["DueDate"] != DBNull.Value) ? DateTime.Parse(dt.Rows[i]["DueDate"].ToString()) : (DateTime?)null;
                            studentResult.Exceeds = (dt.Rows[i]["Exceeds"] != DBNull.Value) ? Decimal.Parse(dt.Rows[i]["Exceeds"].ToString()) : (decimal?)null;
                            studentResult.GradeID = (dt.Rows[i]["GradeID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["GradeID"].ToString()) : 0;
                            studentResult.GradeOrder = (dt.Rows[i]["GradeOrder"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["GradeOrder"].ToString()) : 0;
                            studentResult.GradeShortName = (dt.Rows[i]["GradeShortName"] != DBNull.Value) ? dt.Rows[i]["GradeShortName"].ToString() : String.Empty;
                            studentResult.Hex = (dt.Rows[i]["Hex"] != DBNull.Value) ? dt.Rows[i]["Hex"].ToString() : String.Empty;
                            studentResult.Meets = (dt.Rows[i]["Meets"] != DBNull.Value) ? Decimal.Parse(dt.Rows[i]["Meets"].ToString()) : (decimal?)null;
                            studentResult.SectionID = (dt.Rows[i]["SectionID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["SectionID"].ToString()) : (int?)null;
                            studentResult.TestDueDateID = (dt.Rows[i]["TestDueDateID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["TestDueDateID"].ToString()) : -1;
                            studentResult.TestLevelPeriodID = (dt.Rows[i]["TestLevelPeriodID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["TestLevelPeriodID"].ToString()) : 0;
                            studentResult.TestNumber = (dt.Rows[i]["TestNumber"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["TestNumber"].ToString()) : 0;
                            studentResult.DoesNotMeet = (dt.Rows[i]["DoesNotMeet"] != DBNull.Value) ? Decimal.Parse(dt.Rows[i]["DoesNotMeet"].ToString()) : (decimal?)null;
                            studentResult.Approaches = (dt.Rows[i]["Approaches"] != DBNull.Value) ? Decimal.Parse(dt.Rows[i]["Approaches"].ToString()) : (decimal?)null;
                            studentResult.Year = (dt.Rows[i]["Year"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["Year"].ToString()) : 0;
                        }
                    }

                }
                catch (Exception ex)
                {
                    Log.Error("Error getting line graph benchmarks: {0}", ex.Message);
                }
                finally
                {
                    _dbContext.Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }
            return list.OrderBy(p => p.DueDate).ToList();
        }

        public OutputDto_GetAssessmentFields GetStudentLineGraphFields(InputDto_GetStudentLineGraphFields input)
        {
            var assessmentsOfType = _dbContext.Assessments.Where(p => p.TestType == input.AssessmentTypeId).ToList();
            var finalList = new List<Assessment>();
            var assessmentFields = new List<AssessmentField>();

            // this is probably the most inefficient way in history to do this, but I have deadlines and no peer reviewer :)  sue me...
            foreach(var assessment in assessmentsOfType)
            {
                using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
                {
                    try
                    {
                        _dbContext.Database.Connection.Open();

                        // intervention groups are not for all time like benchmark
                        if(input.AssessmentTypeId == 2)
                        {
                            command.CommandText = String.Format("Select * FROM {0} WHERE StudentID = {1} and interventiongroupid = {2}", assessment.StorageTable, input.StudentId.ToString(), input.InterventionGroupId);

                        }
                        else
                        {
                            command.CommandText = String.Format("Select * FROM {0} WHERE StudentID = {1}", assessment.StorageTable, input.StudentId.ToString());

                        }
                        command.CommandTimeout = command.Connection.ConnectionTimeout;

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
                        Log.Error("Exception encountered while getting student line graph fields: {0}", ex.Message);
                    }
                    finally
                    {
                        _dbContext.Database.Connection.Close();
                        command.Parameters.Clear();
                    }
                }

            }

            // now find which fields that can be displayed in line graphs are benchmarked
            // UPdated on 2/15/2018 to show fields that aren't benchmarked as well
            foreach(var assessment in finalList)
            {
                //var benchmarkedFieldNames = _dbContext.DistrictBenchmarks.Where(p => p.AssessmentID == assessment.Id).Select(p => p.AssessmentField).ToList();
                assessmentFields.AddRange(_dbContext.AssessmentFields.Where(p => p.AssessmentId == assessment.Id && p.DisplayInLineGraphs == true).ToList());
            }

            return new OutputDto_GetAssessmentFields() { Fields = Mapper.Map<List<AssessmentFieldDto>>(assessmentFields) };
        }

        public OutputDto_LineGraphResults GetBenchmarkedAssessmentResults(int assessmentId, string fieldToRetrieve, bool isLookupColumn, int studentId, string lookupFieldName)
        {
            var assessment = _dbContext.Assessments.First(p => p.Id == assessmentId);
            IEnumerable<AssessmentField> fieldsToRetrieve = null;
            fieldsToRetrieve = assessment.Fields.Where(p => p.DisplayInLineGraphSummaryTable == true && p.DatabaseColumn != fieldToRetrieve);

            var list = new List<BenchmarkedAssessmentResult>();

            using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
            {
                try
                {
                    _dbContext.Database.Connection.Open();
                    command.CommandText = String.Format("EXEC dbo._ns4_GetBenchmarkedAssessmentResults @TestDbTable='{0}',@FieldToRetrieve='{1}',@StudentId={2},@IsLookupColumn={3}, @LookupFieldName='{4}'", assessment.StorageTable,
                fieldToRetrieve,
                studentId,
                isLookupColumn,
                lookupFieldName);
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = command.Connection.ConnectionTimeout;

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        // load datatable
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            BenchmarkedAssessmentResult studentResult = new BenchmarkedAssessmentResult();
                            list.Add(studentResult);
                            studentResult.StudentId = (dt.Rows[i]["StudentId"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["StudentId"].ToString()) : -1;
                            studentResult.TestDueDateID = (dt.Rows[i]["TestDueDateID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["TestDueDateID"].ToString()) : -1;
                            studentResult.FieldValueID = (dt.Rows[i]["Unique_FieldValueID"] != DBNull.Value) ? Decimal.Parse(dt.Rows[i]["Unique_FieldValueID"].ToString()) : (decimal?)null;
                            studentResult.FieldDisplayValue = (dt.Rows[i]["FieldDisplayValue"] != DBNull.Value) ? dt.Rows[i]["FieldDisplayValue"].ToString() : String.Empty;
                            studentResult.FieldSortOrder = (dt.Rows[i]["FieldSortOrder"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["FieldSortOrder"].ToString()) : -1;
                            studentResult.IsCopied = (dt.Rows[i]["IsCopied"] != DBNull.Value) ? Boolean.Parse(dt.Rows[i]["IsCopied"].ToString()) : false;

                            // add FieldResults
                            foreach (var field in fieldsToRetrieve.OrderBy(p => p.FieldOrder))
                            {
                                if (!String.IsNullOrEmpty(field.DatabaseColumn))
                                {
                                    AssessmentFieldResultDisplayOnly fieldResult = new AssessmentFieldResultDisplayOnly();
                                    studentResult.FieldResults.Add(fieldResult);
                                    fieldResult.DbColumn = field.DatabaseColumn;
                                    _dbContext.SetFieldDisplayValueBasedOnType(fieldResult, field, dt.Rows[i]);
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Log.Error("Error getting line graph data: {0}", ex.Message);
                    throw;
                }
                finally
                {
                    _dbContext.Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }
            return new OutputDto_LineGraphResults { StudentResults = list, Fields = Mapper.Map<List<AssessmentFieldDto>>(fieldsToRetrieve) };
        }

        public OutputDto_IGLineGraphResults GetInterventionGroupAssessmentResults(int assessmentId, string fieldToRetrieve, bool isLookupColumn, int studentId, string lookupFieldName, int interventionGroupId)
        {
            var assessment = _dbContext.Assessments.First(p => p.Id == assessmentId);
            IEnumerable<AssessmentField> fieldsToRetrieve = null;
            fieldsToRetrieve = assessment.Fields.Where(p => p.DisplayInLineGraphSummaryTable == true && p.DatabaseColumn != fieldToRetrieve);
            var list = new List<InterventionGroupAssessmentResult>();

            using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
            {
                try
                {
                    _dbContext.Database.Connection.Open();
                    command.CommandText = String.Format("EXEC dbo._ns4_GetInterventionGroupAssessmentResults @TestDbTable='{0}',@FieldToRetrieve='{1}',@StudentId={2},@IsLookupColumn={3}, @LookupFieldName='{4}', @InterventionGroupId='{5}'", assessment.StorageTable,
                fieldToRetrieve,
                studentId,
                isLookupColumn,
                lookupFieldName,
                interventionGroupId);
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = command.Connection.ConnectionTimeout;

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        // load datatable
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            InterventionGroupAssessmentResult studentResult = new InterventionGroupAssessmentResult();
                            list.Add(studentResult);
                            studentResult.StudentId = (dt.Rows[i]["StudentId"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["StudentId"].ToString()) : -1;
                            studentResult.TestDueDate = DateTime.Parse(dt.Rows[i]["TestDueDate"].ToString());
                            studentResult.TestDueDateID = (dt.Rows[i]["TestDueDateID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["TestDueDateID"].ToString()) : -1;
                            studentResult.TestNumber = (dt.Rows[i]["TestNumber"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["TestNumber"].ToString()) : -1;
                            studentResult.FieldValueID = (dt.Rows[i]["Unique_FieldValueID"] != DBNull.Value) ? Decimal.Parse(dt.Rows[i]["Unique_FieldValueID"].ToString()) : (decimal?)null;
                            studentResult.FieldDisplayValue = (dt.Rows[i]["FieldDisplayValue"] != DBNull.Value) ? dt.Rows[i]["FieldDisplayValue"].ToString() : String.Empty;
                            studentResult.FieldSortOrder = (dt.Rows[i]["FieldSortOrder"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["FieldSortOrder"].ToString()) : -1;

                            // add FieldResults
                            foreach (var field in fieldsToRetrieve.OrderBy(p => p.FieldOrder))
                            {
                                if (!String.IsNullOrEmpty(field.DatabaseColumn))
                                {
                                    AssessmentFieldResultDisplayOnly fieldResult = new AssessmentFieldResultDisplayOnly();
                                    studentResult.FieldResults.Add(fieldResult);
                                    fieldResult.DbColumn = field.DatabaseColumn;
                                    _dbContext.SetFieldDisplayValueBasedOnType(fieldResult, field, dt.Rows[i]);
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Log.Error("Error getting line graph data: {0}", ex.Message);
                    throw;
                }
                finally
                {
                    _dbContext.Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }
            return new OutputDto_IGLineGraphResults { StudentResults = list, Fields = Mapper.Map<List<AssessmentFieldDto>>(fieldsToRetrieve) };
        }

        
    }
}
