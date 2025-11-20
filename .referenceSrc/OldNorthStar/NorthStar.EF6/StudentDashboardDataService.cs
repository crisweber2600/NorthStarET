using EntityDto.DTO.Reports.StudentDashboard;
using NorthStar4.PCL.DTO;
using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace NorthStar.EF6.DataService
{
    public class StudentDashboardDataService : NSBaseDataService
    {
        public StudentDashboardDataService(ClaimsIdentity user, string loginConnectionString) : base(user, loginConnectionString)
        {

        }

        public OutputDto_ObservationSummaryClass GetStudentObservationSummary(InputDto_GetStudentObservationSummary input)
        {
            // bad data check
            if (input.StudentId == -1 || input.StudentId == 0)
            {
                return new OutputDto_ObservationSummaryClass();
            }

            // turn list of string to list of Ints
            //string[] aryAssessmentIds = input.AssessmentIds.Split(Char.Parse(","));
            //List<int> intAssessmentIds = new List<int>();

            //foreach (var assessmentId in aryAssessmentIds)
            //{
            //    intAssessmentIds.Add(Int32.Parse(assessmentId));
            //}

            // 1. Get list of assessments from Db
            List<Assessment> assessmentsToInclude = _dbContext.GetObservationSummaryVisibleAssessments(_currentUser.Id);

            // 2. Create table of concatenated fields from all the tables
            var scores = GetStudentObservationSummaryData(assessmentsToInclude, input.StudentId, _currentUser.Id);

            //var distinctTdds = scores.StudentResults.Select(p => new { TddId = p.TestDueDateId, TestPeriodLevelId = p.TestLevelPeriodId, GradeId = p.GradeId }).Distinct().ToList();

            // 3. Get Benchmarks for all of the fields
            // get an an array of benchmarks BY GRADE so that we can use this for other forms of the Obs Summary data
            // TODO: loop over all grades and get benchmarks
            var benchmarks = new List<ObservationSummaryBenchmark>();

            //foreach (var distinctTdd in distinctTdds)
            //{
                var benchmarkList = _dbContext.GetAllObservationSummaryBenchmarks(assessmentsToInclude);
                //benchmarks.AddRange(benchmarkList);
            //}
            var benchmarkArray = benchmarkList.Distinct().ToList();
            var benchmarksByGrade = new List<ObservationSummaryBenchmarksByGrade>();

            foreach(var benchmark in benchmarkArray)
            {
                var gradeBenchmark = benchmarksByGrade.FirstOrDefault(p => p.GradeId == benchmark.GradeId);

                if(gradeBenchmark == null)
                {
                    gradeBenchmark = new ObservationSummaryBenchmarksByGrade { GradeId = benchmark.GradeId };
                    benchmarksByGrade.Add(gradeBenchmark);
                }
                gradeBenchmark.Benchmarks.Add(benchmark);
            }

            // 4. Combine all data into and object I can send to Angular
            return new OutputDto_ObservationSummaryClass()
            {
                Scores = scores,
                BenchmarksByGrade = benchmarksByGrade
            };
        }

        public ObservationSummaryGroupResults GetStudentObservationSummaryData(List<Assessment> assessments, int studentId, int staffId)
        {
            ObservationSummaryGroupResults groupResults = new ObservationSummaryGroupResults();
            groupResults.StudentResults = new List<ObservationSummaryStudentResult>();
            int gradeId = 0;
            int testLevelPeriodId = 0;
            string commaTerminatedAssessments = string.Empty;
            commaTerminatedAssessments = string.Join(",", assessments.Select(p => p.StorageTable).ToArray()) + ",";

            // calculate benchmark value and return
            using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
            {
                try
                {
                    // set up headers
                    foreach (Assessment assessment in assessments)
                    {
                        // remove fields that shouldn't be shown in this view
                        assessment.Fields = _dbContext.GetViewableFields(assessment, "observationsummary", staffId); // TODO: Do this client-side, send this collection back
                        // TODO: Add a sort order for Assessments
                        var currentHeaderGroup = new ObservationSummaryAssessmentHeaderGroup()
                        {
                            AssessmentId = assessment.Id,
                            AssessmentName =
                                                         assessment.AssessmentName,
                            AssessmentOrder = 5
                        };
                        groupResults.HeaderGroups.Add(currentHeaderGroup);
                        foreach (var currentField in assessment.Fields.OrderBy(p => p.FieldOrder))
                        {
                            var currentHeader = new ObservationSummaryAssessmentHeader()
                            {
                                AssessmentName = currentHeaderGroup.AssessmentName,
                                FieldName = currentField.ObsSummaryLabel,
                                FieldOrder = currentField.FieldOrder,
                                LookupFieldName = currentField.LookupFieldName,
                                DatabaseColumn = currentField.DatabaseColumn,
                                FieldType = currentField.FieldType,
                                Id = currentField.Id,
                                AssessmentId = currentHeaderGroup.AssessmentId
                            };
                            groupResults.Fields.Add(currentHeader);
                            currentHeaderGroup.FieldCount++;
                        }
                    }


                    _dbContext.Database.Connection.Open();
                    command.CommandText = String.Format("EXEC dbo.ns4_GetObservationSummaryScores_ForStudent @TableNames='{0}',@StudentId={1}", commaTerminatedAssessments, studentId);
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = command.Connection.ConnectionTimeout;

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        // load datatable
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            ObservationSummaryStudentResult studentResult = new ObservationSummaryStudentResult();
                            groupResults.StudentResults.Add(studentResult);
                            studentResult.StudentId = Int32.Parse(dt.Rows[i]["RealStudentID"].ToString());
                            studentResult.GradeId = Int32.Parse(dt.Rows[i]["GradeID"].ToString());
                            studentResult.GradeOrder = Int32.Parse(dt.Rows[i]["GradeOrder"].ToString());
                            studentResult.TestDueDateId = Int32.Parse(dt.Rows[i]["TddId"].ToString());
                            studentResult.TestLevelPeriodId = Int32.Parse(dt.Rows[i]["TestLevelPeriodId"].ToString());
                            studentResult.StudentName = dt.Rows[i]["LastName"].ToString() + ", " + dt.Rows[i]["FirstName"].ToString();
                            studentResult.DelimitedTeachers = dt.Rows[i]["Teachers"].ToString();
                            studentResult.GradeName = dt.Rows[i]["GradeName"].ToString();
                            //studentResult.MiddleName = dt.Rows[i]["MiddleName"].ToString();
                            //studentResult.LastName = dt.Rows[i]["LastName"].ToString();
                            studentResult.TestDate = dt.Columns.Contains("DueDate") ? DateTime.Parse(dt.Rows[i]["DueDate"].ToString()) : (DateTime?)null;

                            // now create the fields that hold the scores for each assessment
                            List<ObservationSummaryFieldScore> fieldScores = new List<ObservationSummaryFieldScore>();
                            studentResult.OSFieldResults = fieldScores;

                            // not right now, but think through the case of assessment with the same field names... like accuracy in two assessments
                            // need to prefix the field names
                            foreach (Assessment assessment in assessments)
                            {
                                foreach (var currentField in assessment.Fields.OrderBy(p => p.FieldOrder))
                                {
                                    var currentFieldScore = new ObservationSummaryFieldScore();
                                    fieldScores.Add(currentFieldScore);
                                    currentFieldScore.LookupFieldName = currentField.LookupFieldName;
                                    currentFieldScore.AssessmentId = assessment.Id;
                                    currentFieldScore.DbColumn = currentField.DatabaseColumn;
                                    currentFieldScore.ColumnType = currentField.FieldType;
                                    currentFieldScore.FieldOrder = currentField.FieldOrder;
                                    var currentColumn = assessment.StorageTable + "_" + currentField.DatabaseColumn;
                                    switch (currentField.FieldType)
                                    {
                                        case "Textfield":
                                        case "checkllist":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                        case "DecimalRange":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.DecimalValue = Decimal.Parse(dt.Rows[i][currentColumn].ToString());
                                            }
                                            break;
                                        case "DropdownRange":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.IntValue = Int32.Parse(dt.Rows[i][currentColumn].ToString());
                                            }
                                            break;
                                        case "DropdownFromDB":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.IntValue = Int32.Parse(dt.Rows[i][currentColumn].ToString());
                                            }
                                            break;
                                        case "CalculatedFieldDbBacked":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.IntValue = Int32.Parse(dt.Rows[i][currentColumn].ToString());
                                            }
                                            break;
                                        case "CalculatedFieldDbBackedString":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                        case "CalculatedFieldDbOnly":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                        case "CalculatedFieldClientOnly":
                                            // no-op
                                            break;
                                        default:
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {

                }
                finally
                {
                    _dbContext.Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }

            return groupResults;
        }
    }
}
