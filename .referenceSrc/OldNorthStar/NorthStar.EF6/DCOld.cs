//using NorthStar4.PCL.Entity;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Runtime.Remoting.Contexts;
//using System.Security.Permissions;
//using System.Text;
//using Microsoft.Data.Entity;
//using Microsoft.Data.Entity.Metadata;
//using Microsoft.Data.Entity.Query;
//using NorthStar4.PCL.DTO;
//using NorthStar4.CrossPlatform.DTO.Reports;
//using NorthStar4.CrossPlatform.Entity;

//namespace NorthStar4.EntityFramework
//{
//    public class DistrictContext : DbContext
//    {
//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            base.OnModelCreating(modelBuilder);

//            #region Assessment

//            modelBuilder.Entity<Assessment>()
//                .ForRelational(t => t.Table("Assessment"));

//            modelBuilder.Entity<Assessment>().Property(t => t.Id).GenerateValueOnAdd(true).Required(true).Annotation("SqlServer:ValueGeneration", "Identity").UseStoreDefault(false);
//            modelBuilder.Entity<Assessment>().Property(t => t.AssessmentName).Required(true);
//            modelBuilder.Entity<Assessment>().Property(t => t.ClassReportPages);
//            modelBuilder.Entity<AssessmentField>().Property(p => p.Id).GenerateValueOnAdd(true).Annotation("SqlServer:ValueGeneration", "Identity").UseStoreDefault(false);


//            modelBuilder.Entity<AssessmentFieldGroup>()
//                .ForRelational(t => t.Table("AssessmentFieldGroup"));
//            modelBuilder.Entity<AssessmentFieldGroup>().Property(t => t.Id).GenerateValueOnAdd(true).Annotation("SqlServer:ValueGeneration", "Identity").UseStoreDefault(false);
//            modelBuilder.Entity<AssessmentFieldGroup>().Property(t => t.AssessmentId).Required(true);
//            modelBuilder.Entity<AssessmentFieldGroup>().Property(t => t.DisplayName).Required(true);
//            modelBuilder.Entity<AssessmentFieldGroup>().Property(t => t.ModifiedDate).UseStoreDefault(true);

//            modelBuilder.Entity<AssessmentFieldCategory>()
//                .ForRelational(t => t.Table("AssessmentFieldCategory"));
//            modelBuilder.Entity<AssessmentFieldCategory>().Property(t => t.Id).GenerateValueOnAdd(true).Annotation("SqlServer:ValueGeneration", "Identity").UseStoreDefault(false);
//            modelBuilder.Entity<AssessmentFieldCategory>().Property(t => t.AssessmentId).Required(true);
//            modelBuilder.Entity<AssessmentFieldCategory>().Property(t => t.DisplayName).Required(true);
//            modelBuilder.Entity<AssessmentFieldCategory>().Property(t => t.ModifiedDate).UseStoreDefault(true);

//            modelBuilder.Entity<AssessmentFieldSubCategory>()
//                .ForRelational(t => t.Table("AssessmentFieldSubCategory"));
//            modelBuilder.Entity<AssessmentFieldSubCategory>().Property(t => t.Id).GenerateValueOnAdd(true).Annotation("SqlServer:ValueGeneration", "Identity").UseStoreDefault(false);
//            modelBuilder.Entity<AssessmentFieldSubCategory>().Property(t => t.AssessmentId).Required(true);
//            modelBuilder.Entity<AssessmentFieldSubCategory>().Property(t => t.DisplayName).Required(true);
//            modelBuilder.Entity<AssessmentFieldSubCategory>().Property(t => t.ModifiedDate).UseStoreDefault(true);

//            #endregion

//            #region SchoolStartYear
//            modelBuilder.Entity<SchoolYear>()
//                .ForRelational(t => t.Table("SchoolYears"));
//            modelBuilder.Entity<SchoolYear>().Key("SchoolStartYear");

//            #endregion

//            #region Section

//            modelBuilder.Entity<StaffSection>().Property(t => t.Id).GenerateValueOnAdd(true).Annotation("SqlServer:ValueGeneration", "Identity").UseStoreDefault(false);
//            modelBuilder.Entity<StaffSection>()
//                .ForRelational(t => t.Table("StaffSection")).Reference(p => p.Section).InverseCollection(p => p.StaffSections).ForeignKey(p => p.ClassID);
//            modelBuilder.Entity<Section>().Property(p => p.Id).GenerateValueOnAdd(true).Annotation("SqlServer:ValueGeneration", "Identity").UseStoreDefault(false);
//            modelBuilder.Entity<Section>()
//                .ForRelational(t => t.Table("Section")).Collection<StaffSection>(p => p.StaffSections).InverseReference(p => p.Section).ForeignKey(p => p.ClassID);

//            #endregion

//            #region Staff
//            modelBuilder.Entity<StaffSchool>().Property(t => t.Id).GenerateValueOnAdd(true).Annotation("SqlServer:ValueGeneration", "Identity").UseStoreDefault(false);

//            #endregion

//            modelBuilder.Entity<Intervention>()
//                .ForRelational(t => t.Table("InterventionType"))
//                .Reference(p => p.InterventionFramework)
//                .InverseCollection(p => p.InterventionTypes)
//                .ForeignKey(p => p.FrameworkID);
//            modelBuilder.Entity<Intervention>()
//                .ForRelational(t => t.Table("InterventionType"))
//                .Reference(p => p.InterventionCategory)
//                .InverseCollection(p => p.InterventionTypes)
//                .ForeignKey(p => p.CategoryID);
//            modelBuilder.Entity<Intervention>()
//                .ForRelational(t => t.Table("InterventionType"))
//                .Reference(p => p.InterventionUnitOfStudy)
//                .InverseCollection(p => p.InterventionTypes)
//                .ForeignKey(p => p.UnitOfStudyID);
//            modelBuilder.Entity<Intervention>()
//                .ForRelational(t => t.Table("InterventionType"))
//                .Reference(p => p.InterventionWorkshop)
//                .InverseCollection(p => p.InterventionTypes)
//                .ForeignKey(p => p.WorkshopID);
//            modelBuilder.Entity<WeeklyAttendanceResult>().Key("RecordKey");
//            modelBuilder.Entity<InterventionAttendance>().Property(t => t.Id).GenerateValueOnAdd(true).Annotation("SqlServer:ValueGeneration", "Identity").UseStoreDefault(false);
//        }

//        public DbSet<Staff> Staffs { get; set; }
//        public DbSet<WeeklyAttendanceResult> WeeklyAttendanceResults { get; set; }

//        public DbSet<Assessment> Assessments { get; set; }
//        public DbSet<AssessmentLookupField> LookupFields { get; set; }
//        public DbSet<AssessmentField> AssessmentFields { get; set; }
//        public DbSet<AssessmentFieldGroup> AssessmentFieldGroups { get; set; }
//        public DbSet<AssessmentFieldCategory> AssessmentFieldCategories { get; set; }
//        public DbSet<AssessmentFieldSubCategory> AssessmentFieldSubCategories { get; set; }
//        public DbSet<Student> Students { get; set; }
//        public DbSet<Section> Sections { get; set; }
//        public DbSet<StudentSection> StudentSections { get; set; }
//        public DbSet<Grade> Grades { get; set; }
//        public DbSet<TestDueDate> TestDueDates { get; set; }
//        public DbSet<School> Schools { get; set; }
//        public DbSet<Intervention> Interventions { get; set; }
//        public DbSet<InterventionTool> InterventionTools { get; set; }
//        public DbSet<InterventionCardinality> InterventionCardinalities { get; set; }
//        public DbSet<InterventionUnitOfStudy> InterventionUnitOfStudies { get; set; }
//        public DbSet<InterventionCategory> InterventionCategories { get; set; }
//        public DbSet<InterventionFramework> InterventionFrameworks { get; set; }
//        public DbSet<InterventionToolIntervention> InterventionToolInterventions { get; set; }
//        public DbSet<InterventionGrade> InterventionGrades { get; set; }
//        public DbSet<InterventionWorkshop> InterventionWorkshops { get; set; }
//        public DbSet<InterventionTier> InterventionTiers { get; set; }
//        public DbSet<SchoolYear> SchoolYears { get; set; }
//        public DbSet<StaffSchool> StaffSchools { get; set; }
//        public DbSet<StaffSection> StaffSections { get; set; }
//        public DbSet<StudentSchool> StudentSchools { get; set; }
//        public DbSet<SchoolCalendar> SchoolCalendars { get; set; }
//        public DbSet<DistrictCalendar> DistrictCalendars { get; set; }
//        public DbSet<InterventionAttendance> InterventionAttendances { get; set; }
//        public DbSet<AttendanceReason> AttendanceReasons { get; set; }

//        /// <summary>
//        /// This is used for data entry screens and returns the data for a single TDD
//        /// </summary>
//        /// <param name="assessment"></param>
//        /// <param name="classId"></param>
//        /// <param name="benchmarkDateId"></param>
//        /// <param name="testDate"></param>
//        /// <returns></returns>
//        public List<AssessmentStudentResult> GetAssessmentStudentResults(Assessment assessment, int classId,
//            int benchmarkDateId, DateTime testDate)
//        {
//            List<AssessmentStudentResult> lstStudentData = new List<AssessmentStudentResult>();
//            try
//            {
//                var results = Database.DynamicSqlQuery(String.Format("SELECT s.ID as StudentID, s.FirstName, s.LastName, s.MiddleName, ISNULL(fp.FPText, 'N/A') as FPText, ISNULL(fp.FPValueID, 0) as FPValueID, t.*, {1} as InputSectionId FROM {0} t RIGHT OUTER JOIN Student s on s.ID = t.StudentID AND t.sectionid = {1} and t.testduedateid = {2} RIGHT OUTER JOIN dbo.nset_udf_GetStudentFPText(NULL, {1}, {2}, NULL) fp ON fp.StudentID = s.ID WHERE s.ID IN (SELECT Studentid FROM  dbo.nset_udf_GetStudentIDsForSummaryPages({1}, '{3}', {2}))", assessment.StorageTable, classId, benchmarkDateId, DateTime.Now), assessment, this);
//                lstStudentData = results;
//            }
//            catch (Exception ex)
//            {
//                //TODO:LOG
//            }

//            return lstStudentData;
//        }

//        /// <summary>
//        /// This is used for data entry screens and returns the data for a single TDD
//        /// </summary>
//        /// <param name="assessment"></param>
//        /// <param name="classId"></param>
//        /// <param name="benchmarkDateId"></param>
//        /// <param name="testDate"></param>
//        /// <returns></returns>
//        public AssessmentHFWStudentResult GetHFWAssessmentStudentResults(Assessment assessment, int classId,
//            int benchmarkDateId, DateTime testDate, int studentId)
//        {
//            AssessmentHFWStudentResult studentData = null;
//            try
//            {
//                studentData = Database.HFWStudentDataEntryResults(String.Format("SELECT s.ID as StudentID, s.FirstName, s.LastName, s.MiddleName, ISNULL(fp.FPText, 'N/A') as FPText, ISNULL(fp.FPValueID, 0) as FPValueID, t.*, r.*, w.*, {1} as InputSectionId FROM {0} t RIGHT OUTER JOIN Student s on s.ID = t.StudentID AND t.testduedateid = {2} RIGHT OUTER JOIN dbo.nset_udf_GetStudentFPText({4}, {1}, {2}, NULL) fp ON fp.StudentID = s.ID and s.ID = {4} LEFT JOIN {5} r on r.StudentID = {4} LEFT JOIN {6} w on w.StudentID = {4}", assessment.StorageTable, classId, benchmarkDateId, DateTime.Now, studentId, assessment.SecondaryStorageTable, assessment.TertiaryStorageTable), assessment, this);
//            }
//            catch (Exception ex)
//            {
//                //TODO:LOG
//            }

//            return studentData;
//        }

//        /// <summary>
//        /// This method doesn't care about test due dates and returns data for ALL test due dates.  This is used for section reports.
//        /// </summary>
//        /// <param name="assessment"></param>
//        /// <param name="classId"></param>
//        /// <returns></returns>
//        public List<AssessmentStudentResult> GetBASAssessmentStudentResults(Assessment assessment, int classId)
//        {
//            List<AssessmentStudentResult> lstStudentData = new List<AssessmentStudentResult>();
//            try
//            {
//                var results = Database.GetBASClassReportData(String.Format("SELECT s.ID as StudentID, s.FirstName, s.LastName, s.MiddleName, t.*, {1} as InputSectionId FROM {0} t RIGHT OUTER JOIN Student s on s.ID = t.StudentID AND t.sectionid = {1} WHERE s.ID IN (SELECT Studentid FROM  dbo.nset_udf_GetStudentIDsForSummaryPages({1}, '{2}', null))", assessment.StorageTable, classId, DateTime.Now), assessment, this);
//                lstStudentData = results;
//            }
//            catch (Exception ex)
//            {
//                //TODO:LOG
//            }

//            return lstStudentData;
//        }

//        public List<OutputDto_StudentQuickSearch> GetStudentQuickSearchResults(InputDto_StudentQuickSearch input)
//        {
//            List<OutputDto_StudentQuickSearch> results = new List<OutputDto_StudentQuickSearch>();

//            using (System.Data.IDbCommand command = Database.AsSqlServer().Connection.DbConnection.CreateCommand())
//            {
//                try
//                {
//                    //Database.AsSqlServer().sq
//                    //Database.AsRelational().s
//                    Database.AsSqlServer().Connection.DbConnection.Open();
//                    command.CommandText =
//                        String.Format(
//                            "EXEC dbo.nset_StudentQuickSearch @StaffID='163',@SearchString='{0}',@IsSysAdmin=1,@RequiresSchool=0,@RequiresClass=0,@SchoolYear=2014,@IsInterventionGroup=0",
//                            input.searchString);
//                    command.CommandType = CommandType.Text;
//                    command.CommandTimeout = command.Connection.ConnectionTimeout;

//                    using (System.Data.IDataReader reader = command.ExecuteReader())
//                    {
//                        // load datatable
//                        DataTable dt = new DataTable();
//                        dt.Load(reader);

//                        for (int i = 0; i < dt.Rows.Count; i++)
//                        {
//                            OutputDto_StudentQuickSearch result = new OutputDto_StudentQuickSearch();
//                            result.StudentId = Int32.Parse(dt.Rows[i]["ID"].ToString());
//                            result.FirstName = dt.Rows[i]["FirstName"].ToString();
//                            result.LastName = dt.Rows[i]["LastName"].ToString();
//                            result.StudentIdentifier = dt.Rows[i]["StudentIdentifier"].ToString();
//                            result.CurrentSchoolName = dt.Rows[i]["SchoolName"].ToString();
//                            result.CurrentTeacherName = dt.Rows[i]["TeacherName"].ToString();
//                            result.CurrentSectionName = dt.Rows[i]["ClassName"].ToString();
//                            results.Add(result);
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {

//                }
//                finally
//                {
//                    Database.AsSqlServer().Connection.DbConnection.Close();
//                    command.Parameters.Clear();
//                }
//            }

//            return results;
//        }

//        public
//            ObservationSummaryGroupResults GetClassObservationSummaryData(List<Assessment> assessments, int classId,
//        int benchmarkDateId, DateTime testDate)
//        {
//            ObservationSummaryGroupResults groupResults = new ObservationSummaryGroupResults();
//            groupResults.StudentResults = new List<ObservationSummaryStudentResult>();
//            int gradeId = 0;
//            int testLevelPeriodId = 0;
//            string commaTerminatedAssessments = string.Empty;
//            commaTerminatedAssessments = string.Join(",", assessments.Select(p => p.StorageTable).ToArray()) + ",";

//            // calculate benchmark value and return
//            using (System.Data.IDbCommand command = Database.AsSqlServer().Connection.DbConnection.CreateCommand())
//            {
//                try
//                {
//                    // set up headers
//                    foreach (Assessment assessment in assessments)
//                    {
//                        // TODO: Add a sort order for Assessments
//                        var currentHeaderGroup = new ObservationSummaryAssessmentHeaderGroup()
//                        {
//                            AssessmentId = assessment.Id,
//                            AssessmentName =
//                                                         assessment.AssessmentName,
//                            AssessmentOrder = 5
//                        };
//                        groupResults.HeaderGroups.Add(currentHeaderGroup);
//                        foreach (var currentField in assessment.Fields)
//                        {
//                            var currentHeader = new ObservationSummaryAssessmentHeader()
//                            {
//                                AssessmentName = currentHeaderGroup.AssessmentName,
//                                FieldName = currentField.DisplayLabel,
//                                FieldOrder = currentField.FieldOrder,
//                                LookupFieldName = currentField.LookupFieldName,
//                                DatabaseColumn = currentField.DatabaseColumn,
//                                FieldType = currentField.FieldType
//                            };
//                            groupResults.Fields.Add(currentHeader);
//                            currentHeaderGroup.FieldCount++;
//                        }
//                    }


//                    Database.AsSqlServer().Connection.DbConnection.Open();
//                    command.CommandText = String.Format("EXEC dbo.ns4_GetObservationSummaryScores @TableNames='{0}',@TestDueDate='{1}',@TestDueDateID={2},@SectionID={3}", commaTerminatedAssessments, DateTime.Now.ToShortDateString(), benchmarkDateId, classId);
//                    command.CommandType = CommandType.Text;
//                    command.CommandTimeout = command.Connection.ConnectionTimeout;

//                    using (System.Data.IDataReader reader = command.ExecuteReader())
//                    {
//                        // load datatable
//                        DataTable dt = new DataTable();
//                        dt.Load(reader);

//                        for (int i = 0; i < dt.Rows.Count; i++)
//                        {
//                            ObservationSummaryStudentResult studentResult = new ObservationSummaryStudentResult();
//                            groupResults.StudentResults.Add(studentResult);
//                            studentResult.StudentId = Int32.Parse(dt.Rows[i]["RealStudentID"].ToString());
//                            studentResult.FirstName = dt.Rows[i]["FirstName"].ToString();
//                            studentResult.MiddleName = dt.Rows[i]["MiddleName"].ToString();
//                            studentResult.LastName = dt.Rows[i]["LastName"].ToString();
//                            studentResult.TestDueDateId = (dt.Rows[i]["TestDueDateID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["TestDueDateID"].ToString()) : -1;//result.GetPropValue<int>("TestDueDateID");

//                            // now create the fields that hold the scores for each assessment
//                            List<ObservationSummaryFieldScore> fieldScores = new List<ObservationSummaryFieldScore>();
//                            studentResult.OSFieldResults = fieldScores;

//                            // not right now, but think through the case of assessment with the same field names... like accuracy in two assessments
//                            // need to prefix the field names
//                            foreach (Assessment assessment in assessments)
//                            {
//                                foreach (var currentField in assessment.Fields)
//                                {
//                                    var currentFieldScore = new ObservationSummaryFieldScore();
//                                    fieldScores.Add(currentFieldScore);
//                                    currentFieldScore.LookupFieldName = currentField.LookupFieldName;
//                                    currentFieldScore.AssessmentId = assessment.Id;
//                                    currentFieldScore.DbColumn = currentField.DatabaseColumn;
//                                    currentFieldScore.ColumnType = currentField.FieldType;
//                                    currentFieldScore.FieldOrder = currentField.FieldOrder;
//                                    switch (currentField.FieldType)
//                                    {
//                                        case "Textfield":
//                                            if (dt.Rows[i][currentField.DatabaseColumn] != DBNull.Value)
//                                            {
//                                                currentFieldScore.StringValue = dt.Rows[i][currentField.DatabaseColumn].ToString();
//                                            }
//                                            break;
//                                        case "DecimalRange":
//                                            if (dt.Rows[i][currentField.DatabaseColumn] != DBNull.Value)
//                                            {
//                                                currentFieldScore.DecimalValue = Decimal.Parse(dt.Rows[i][currentField.DatabaseColumn].ToString());
//                                            }
//                                            break;
//                                        case "DropdownRange":
//                                            if (dt.Rows[i][currentField.DatabaseColumn] != DBNull.Value)
//                                            {
//                                                currentFieldScore.IntValue = Int32.Parse(dt.Rows[i][currentField.DatabaseColumn].ToString());
//                                            }
//                                            break;
//                                        case "DropdownFromDB":
//                                            if (dt.Rows[i][currentField.DatabaseColumn] != DBNull.Value)
//                                            {
//                                                currentFieldScore.IntValue = Int32.Parse(dt.Rows[i][currentField.DatabaseColumn].ToString());
//                                            }
//                                            break;
//                                        case "CalculatedFieldDbBacked":
//                                            if (dt.Rows[i][currentField.DatabaseColumn] != DBNull.Value)
//                                            {
//                                                currentFieldScore.IntValue = Int32.Parse(dt.Rows[i][currentField.DatabaseColumn].ToString());
//                                            }
//                                            break;
//                                        case "CalculatedFieldDbBackedString":
//                                            if (dt.Rows[i][currentField.DatabaseColumn] != DBNull.Value)
//                                            {
//                                                currentFieldScore.StringValue = dt.Rows[i][currentField.DatabaseColumn].ToString();
//                                            }
//                                            break;
//                                        case "CalculatedFieldDbOnly":
//                                            if (dt.Rows[i][currentField.DatabaseColumn] != DBNull.Value)
//                                            {
//                                                currentFieldScore.StringValue = dt.Rows[i][currentField.DatabaseColumn].ToString();
//                                            }
//                                            break;
//                                        case "CalculatedFieldClientOnly":
//                                            // no-op
//                                            break;
//                                        default:
//                                            if (dt.Rows[i][currentField.DatabaseColumn] != DBNull.Value)
//                                            {
//                                                currentFieldScore.StringValue = dt.Rows[i][currentField.DatabaseColumn].ToString();
//                                            }
//                                            break;
//                                    }
//                                }
//                            }
//                        }
//                    }

//                }
//                catch (Exception ex)
//                {

//                }
//                finally
//                {
//                    Database.AsSqlServer().Connection.DbConnection.Close();
//                    command.Parameters.Clear();
//                }
//            }

//            return groupResults;
//        }

//        public List<ObservationSummaryBenchmark> GetClassObservationSummaryBenchmarks(List<Assessment> assessments, int benchmarkDateId, int classId)
//        {
//            // get grade, testlevelperiod and delimited string of assessementId|fieldName,

//            List<ObservationSummaryBenchmark> lstBenchmarkData = new List<ObservationSummaryBenchmark>();
//            int gradeId = 13; // get later
//            int testLevelPeriodId = 3; // get later
//            StringBuilder delimitedAssessmentFields = new StringBuilder();
//            foreach (var assessment in assessments)
//            {
//                foreach (var field in assessment.Fields)
//                {
//                    delimitedAssessmentFields.AppendFormat("{0}|{1},", assessment.Id, field.DatabaseColumn);
//                }
//            }

//            // calculate benchmark value and return
//            using (System.Data.IDbCommand command = Database.AsSqlServer().Connection.DbConnection.CreateCommand())
//            {
//                try
//                {
//                    Database.AsSqlServer().Connection.DbConnection.Open();
//                    command.CommandText = String.Format("EXEC dbo.ns4_GetObservationSummaryBenchmarks @FieldsWithAssessments='{0}',@GradeID={1},@TestLevelPeriodID={2}", delimitedAssessmentFields.ToString(), gradeId, testLevelPeriodId);
//                    command.CommandType = CommandType.Text;
//                    command.CommandTimeout = command.Connection.ConnectionTimeout;

//                    using (System.Data.IDataReader reader = command.ExecuteReader())
//                    {
//                        // load datatable
//                        DataTable dt = new DataTable();
//                        dt.Load(reader);

//                        for (int i = 0; i < dt.Rows.Count; i++)
//                        {
//                            ObservationSummaryBenchmark result = new ObservationSummaryBenchmark();
//                            result.AssessmentId = Int32.Parse(dt.Rows[i]["AssessmentID"].ToString());
//                            result.DbColumn = dt.Rows[i]["FieldName"].ToString();
//                            result.Int20 = dt.Rows[i]["Int20"] as int?;
//                            result.IntMean = dt.Rows[i]["IntMean"] as int?;
//                            result.Int80 = dt.Rows[i]["Int80"] as int?;
//                            result.Decimal20 = dt.Rows[i]["Decimal20"] as decimal?;
//                            result.DecimalMean = dt.Rows[i]["DecimalMean"] as decimal?;
//                            result.Decimal80 = dt.Rows[i]["Decimal80"] as decimal?;
//                            lstBenchmarkData.Add(result);
//                        }
//                    }

//                }
//                catch (Exception ex)
//                {

//                }
//                finally
//                {
//                    Database.AsSqlServer().Connection.DbConnection.Close();
//                    command.Parameters.Clear();
//                }
//            }

//            return lstBenchmarkData;
//        }

//        public void DeleteStudentData(AssessmentStudentResult result, int assessmentId)
//        {
//            var assessment = Assessments.First(p => p.Id == assessmentId);

//            // first determine if there's already a result for the current student/class/date
//            var deleteSql = new StringBuilder();
//            deleteSql.AppendFormat(
//                "DELETE FROM {0} WHERE ID = {1}",
//                assessment.StorageTable, result.ResultId.ToString());

//            using (System.Data.IDbCommand command = Database.AsSqlServer().Connection.DbConnection.CreateCommand())
//            {
//                try
//                {
//                    Database.AsSqlServer().Connection.DbConnection.Open();
//                    command.CommandText = deleteSql.ToString();
//                    command.CommandTimeout = command.Connection.ConnectionTimeout;
//                    command.ExecuteNonQuery();
//                }
//                catch (Exception ex)
//                {
//                    throw ex;
//                }
//                finally
//                {
//                    Database.AsSqlServer().Connection.DbConnection.Close();
//                    command.Parameters.Clear();
//                }
//            }
//        }

//        public AssessmentStudentResult SaveStudentData(AssessmentStudentResult result, int assessmentId)
//        {
//            var testduedateid = 374;
//            var recorderid = 1570;

//            var assessment = Assessments.Include(p => p.Fields).Include(p => p.FieldGroups).Include(p => p.FieldCategories).Include(p => p.FieldSubCategories).First(p => p.Id == assessmentId);

//            // first determine if there's already a result for the current student/class/date
//            var resultExistSql = new StringBuilder();
//            resultExistSql.AppendFormat("SELECT * FROM {0} WHERE StudentID = {1} and SectionID = {2} and (TestDueDateID = {3})", assessment.StorageTable, result.StudentId.ToString(), result.ClassId.ToString(), testduedateid, result.TestDate.HasValue ? result.TestDate.Value.ToShortDateString() : null);

//            var insertUpdateSql = new StringBuilder();
//            bool isInsert = true;

//            // replace this crap with just a check for the result ID > 0
//            using (System.Data.IDbCommand command = Database.AsSqlServer().Connection.DbConnection.CreateCommand())
//            {
//                try
//                {
//                    Database.AsSqlServer().Connection.DbConnection.Open();
//                    command.CommandText = resultExistSql.ToString();
//                    command.CommandTimeout = command.Connection.ConnectionTimeout;

//                    using (System.Data.IDataReader reader = command.ExecuteReader())
//                    {

//                        if (((System.Data.SqlClient.SqlDataReader)(reader)).HasRows)
//                        {
//                            isInsert = false;
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {
//                    throw ex;
//                }
//                finally
//                {
//                    Database.AsSqlServer().Connection.DbConnection.Close();
//                    command.Parameters.Clear();
//                }
//            }

//            using (System.Data.IDbCommand command = Database.AsSqlServer().Connection.DbConnection.CreateCommand())
//            {
//                try
//                {
//                    Database.AsSqlServer().Connection.DbConnection.Open();
//                    command.CommandText = resultExistSql.ToString();
//                    command.CommandTimeout = command.Connection.ConnectionTimeout;

//                    // need to detect null on testdate
//                    // also need to update TestDueDateID and Recorder

//                    if (isInsert)
//                    {
//                        insertUpdateSql.AppendFormat("INSERT INTO {0} (StudentId, SectionId, RecorderId, TestDueDateId, TestDueDate", assessment.StorageTable);
//                        // for each
//                        foreach (var field in result.FieldResults)
//                        {
//                            var control = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn);
//                            if (control.FieldType != "CalculatedFieldClientOnly")
//                            {
//                                insertUpdateSql.AppendFormat(",{0}", field.DbColumn);
//                            }
//                        }
//                        insertUpdateSql.AppendFormat(") VALUES ({0}, {1}, {2}, {3}, '{4}'", result.StudentId, result.ClassId, recorderid, testduedateid, result.TestDate);
//                        //for each
//                        foreach (var field in result.FieldResults)
//                        {
//                            var control = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn);
//                            if (control.FieldType != "CalculatedFieldClientOnly")
//                            {
//                                if (control.FieldType == "CalculatedFieldDbBacked" || control.FieldType == "CalculatedFieldDbOnly" || control.FieldType == "CalculatedFieldDbBackedString")
//                                {
//                                    insertUpdateSql.AppendFormat(",{0}", GetFieldInsertUpdateStringCalculatedFields(assessment, field, control, result));

//                                }
//                                else
//                                {
//                                    insertUpdateSql.AppendFormat(",{0}", GetFieldInsertUpdateString(field, control.FieldType));

//                                }
//                            }
//                        }
//                        insertUpdateSql.AppendFormat(")");
//                    }
//                    else
//                    {
//                        insertUpdateSql.AppendFormat("UPDATE {0} SET ", assessment.StorageTable);
//                        // don't include fields that we don't have fields for
//                        foreach (var field in result.FieldResults)
//                        {
//                            // don't try to update fields that don't have a dbcolumn
//                            var control = assessment.Fields.FirstOrDefault(p => p.DatabaseColumn == field.DbColumn);
//                            if (control != null && control.FieldType != "CalculatedFieldClientOnly")
//                            {
//                                if (control.FieldType == "CalculatedFieldDbBacked" || control.FieldType == "CalculatedFieldDbOnly" || control.FieldType == "CalculatedFieldDbBackedString")
//                                {
//                                    insertUpdateSql.AppendFormat("{0} = {1},", field.DbColumn, GetFieldInsertUpdateStringCalculatedFields(assessment, field, control, result));
//                                }
//                                else
//                                {
//                                    insertUpdateSql.AppendFormat("{0} = {1},", field.DbColumn, GetFieldInsertUpdateString(field, control.FieldType));
//                                }
//                            }
//                        }
//                        // remove trailing comma
//                        insertUpdateSql.Remove(insertUpdateSql.Length - 1, 1);
//                        insertUpdateSql.AppendFormat(" WHERE StudentId = {0} AND SectionID = {1} and TestDueDateId = {2}", result.StudentId, result.ClassId, testduedateid); // or testdate = {4}
//                    }

//                    command.CommandText = insertUpdateSql.ToString();
//                    command.ExecuteNonQuery();
//                }
//                catch (Exception ex)
//                {
//                    //log
//                    throw ex;
//                }
//                finally
//                {
//                    Database.AsSqlServer().Connection.DbConnection.Close();
//                    command.Parameters.Clear();
//                }
//            }

//            return result;

//        }

//        public AssessmentHFWStudentResult SaveHFWStudentData(AssessmentHFWStudentResult result, int assessmentId)
//        {
//            var testduedateid = 374;
//            var recorderid = 1570;

//            var assessment = Assessments.Include(p => p.Fields).Include(p => p.FieldGroups).Include(p => p.FieldCategories).Include(p => p.FieldSubCategories).First(p => p.Id == assessmentId);

//            // first determine if there's already a result for the current student/class/date
//            var readResultExistSql = new StringBuilder();
//            readResultExistSql.AppendFormat("SELECT * FROM {0} WHERE StudentID = {1} ", assessment.SecondaryStorageTable, result.StudentId.ToString());

//            var readInsertUpdateSql = new StringBuilder();
//            bool isReadInsert = true;
//            bool doReadChange = false;

//            // should not need to do this
//            //var writeResultExistSql = new StringBuilder();
//            //writeResultExistSql.AppendFormat("SELECT * FROM {0} WHERE StudentID = {1} ", assessment.TertiaryStorageTable, result.StudentId.ToString());

//            var writeInsertUpdateSql = new StringBuilder();
//            bool doWriteChange = false;
//            //bool isWriteInsert = true;

//            // replace this crap with just a check for the result ID > 0
//            using (System.Data.IDbCommand command = Database.AsSqlServer().Connection.DbConnection.CreateCommand())
//            {
//                try
//                {
//                    Database.AsSqlServer().Connection.DbConnection.Open();
//                    command.CommandText = readResultExistSql.ToString();
//                    command.CommandTimeout = command.Connection.ConnectionTimeout;

//                    using (System.Data.IDataReader reader = command.ExecuteReader())
//                    {

//                        if (((System.Data.SqlClient.SqlDataReader)(reader)).HasRows)
//                        {
//                            isReadInsert = false;
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {
//                    throw ex;
//                }
//                finally
//                {
//                    Database.AsSqlServer().Connection.DbConnection.Close();
//                    command.Parameters.Clear();
//                }
//            }

//            using (System.Data.IDbCommand command = Database.AsSqlServer().Connection.DbConnection.CreateCommand())
//            {
//                try
//                {
//                    Database.AsSqlServer().Connection.DbConnection.Open();
//                    command.CommandText = readResultExistSql.ToString();
//                    command.CommandTimeout = command.Connection.ConnectionTimeout;

//                    // need to detect null on testdate
//                    // also need to update TestDueDateID and Recorder

//                    if (isReadInsert)
//                    {
//                        readInsertUpdateSql.AppendFormat("INSERT INTO {0} (StudentId,  SectionId, RecorderId, ", assessment.SecondaryStorageTable);
//                        // for each
//                        foreach (var field in result.ReadFieldResults.Where(p => p.IsModified))
//                        {
//                            doReadChange = true;
//                            var control = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn);

//                            readInsertUpdateSql.AppendFormat(",{0}", field.DbColumn);
//                        }
//                        readInsertUpdateSql.AppendFormat(") VALUES ({0}, {1}, {2}, {3}, '{4}'", result.StudentId, result.ClassId, recorderid);
//                        //for each
//                        foreach (var field in result.ReadFieldResults.Where(p => p.IsModified))
//                        {
//                            var control = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn);

//                            readInsertUpdateSql.AppendFormat(",{0}", GetFieldInsertUpdateString(field, control.FieldType));

//                        }
//                        readInsertUpdateSql.AppendFormat(")");

//                        writeInsertUpdateSql.AppendFormat("INSERT INTO {0} (StudentId, SectionId, RecorderId, ", assessment.TertiaryStorageTable);
//                        // for each
//                        foreach (var field in result.WriteFieldResults.Where(p => p.IsModified))
//                        {
//                            doWriteChange = true;
//                            var control = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn);

//                            writeInsertUpdateSql.AppendFormat(",{0}", field.DbColumn);
//                        }
//                        writeInsertUpdateSql.AppendFormat(") VALUES ({0}, {1}, {2}, {3}, '{4}'", result.StudentId, result.ClassId, recorderid);
//                        //for each
//                        foreach (var field in result.WriteFieldResults.Where(p => p.IsModified))
//                        {
//                            var control = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn);

//                            writeInsertUpdateSql.AppendFormat(",{0}", GetFieldInsertUpdateString(field, control.FieldType));

//                        }
//                        writeInsertUpdateSql.AppendFormat(")");
//                    }
//                    else
//                    {
//                        readInsertUpdateSql.AppendFormat("UPDATE {0} SET ", assessment.SecondaryStorageTable);
//                        // don't include fields that we don't have fields for
//                        foreach (var field in result.ReadFieldResults.Where(p => p.IsModified))
//                        {
//                            doReadChange = true;
//                            // don't try to update fields that don't have a dbcolumn
//                            var control = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn);
//                            readInsertUpdateSql.AppendFormat("{0} = {1},", field.DbColumn, GetFieldInsertUpdateString(field, control.FieldType));
//                        }
//                        // remove trailing comma
//                        readInsertUpdateSql.Remove(readInsertUpdateSql.Length - 1, 1);
//                        readInsertUpdateSql.AppendFormat(" WHERE StudentId = {0}", result.StudentId); // or testdate = {4}

//                        writeInsertUpdateSql.AppendFormat("UPDATE {0} SET ", assessment.TertiaryStorageTable);
//                        // don't include fields that we don't have fields for
//                        foreach (var field in result.WriteFieldResults.Where(p => p.IsModified))
//                        {
//                            doWriteChange = true;
//                            // don't try to update fields that don't have a dbcolumn
//                            var control = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn);
//                            writeInsertUpdateSql.AppendFormat("{0} = {1},", field.DbColumn, GetFieldInsertUpdateString(field, control.FieldType));
//                        }
//                        // remove trailing comma
//                        writeInsertUpdateSql.Remove(writeInsertUpdateSql.Length - 1, 1);
//                        writeInsertUpdateSql.AppendFormat(" WHERE StudentId = {0}", result.StudentId); // or testdate = {4}
//                    }

//                    // TODO: Make this a transaction
//                    if (doReadChange)
//                    {
//                        command.CommandText = readInsertUpdateSql.ToString();
//                        command.ExecuteNonQuery();
//                    }
//                    if (doWriteChange)
//                    {
//                        command.CommandText = writeInsertUpdateSql.ToString();
//                        command.ExecuteNonQuery();
//                    }
//                }
//                catch (Exception ex)
//                {
//                    //log
//                    throw ex;
//                }
//                finally
//                {
//                    Database.AsSqlServer().Connection.DbConnection.Close();
//                    command.Parameters.Clear();
//                }
//            }

//            return result;

//        }

//        public string GetFieldInsertUpdateString(AssessmentFieldResult field, string controlType)
//        {
//            switch (controlType)
//            {
//                case "DropdownRange":
//                    return field.IntValue?.ToString() ?? "null";
//                case "Textfield":
//                case "Textarea":
//                    return String.Format("'{0}'", field.StringValue == null ? String.Empty : field.StringValue.Replace("'", "''"));
//                case "Checkbox":
//                    return String.Format("{0}", field.BoolValue.HasValue ? (field.BoolValue.Value == true ? 1 : 0) : 0);
//                case "DecimalRange":
//                    return field.DecimalValue?.ToString() ?? "null";
//                case "DropdownFromDB":
//                    return field.IntValue?.ToString() ?? "null";
//                case "CalculatedFieldDbBacked":
//                    return field.IntValue?.ToString() ?? "null";
//                case "CalculatedFieldDbBackedString":
//                    return String.Format("'{0}'", field.StringValue.Replace("'", "''"));
//                case "CalculatedFieldDbOnly":
//                    return String.Format("'{0}'", field.StringValue.Replace("'", "''"));
//                case "DateCheckbox":
//                case "Date":
//                    // if null, set null
//                    return String.Format("{0}", field.DateValue.HasValue ? "'" + field.DateValue.Value.ToShortDateString() + "'" : "null");
//                default:
//                    return String.Format("'{0}'", field.StringValue.Replace("'", "''"));
//            }
//        }

//        public string GetFieldInsertUpdateStringCalculatedFields(Assessment assessment, AssessmentFieldResult field, AssessmentField asmntField, AssessmentStudentResult result)
//        {
//            switch (asmntField.FieldType)
//            {
//                case "CalculatedFieldDbBacked":
//                case "CalculatedFieldDbBackedString":
//                    int sum = 0;

//                    if (asmntField.CalculationFunction == "Sum")
//                    {
//                        var fieldsToSum = asmntField.CalculationFields.Split(Char.Parse(","));

//                        foreach (var currentResult in result.FieldResults)
//                        {
//                            foreach (var fieldNameToSum in fieldsToSum)
//                            {
//                                if (currentResult.DbColumn.Trim().ToLower() == fieldNameToSum.Trim().ToLower())
//                                {
//                                    sum += currentResult.IntValue ?? 0;
//                                }
//                            }
//                        }
//                        field.IntValue = sum;
//                        return sum.ToString();
//                    }
//                    if (asmntField.CalculationFunction == "SumBool")
//                    {
//                        var fieldsToSum = asmntField.CalculationFields.Split(Char.Parse(","));

//                        foreach (var currentResult in result.FieldResults)
//                        {
//                            foreach (var fieldNameToSum in fieldsToSum)
//                            {
//                                if (currentResult.DbColumn.Trim().ToLower() == fieldNameToSum.Trim().ToLower())
//                                {
//                                    sum += currentResult.BoolValue.HasValue ? (currentResult.BoolValue.Value ? 1 : 0) : 0;
//                                }
//                            }
//                        }
//                        field.IntValue = sum;
//                        return sum.ToString();
//                    }
//                    if (asmntField.CalculationFunction == "SumBoolByGroup")
//                    {
//                        foreach (var group in assessment.FieldGroups)
//                        {
//                            var groupId = group.Id;

//                            // i don't like having this reference to the field... need to figure out
//                            // if it makes more sense to pass the additional data for each field
//                            // or to just join them on the client
//                            foreach (var currentResult in result.FieldResults)
//                            {
//                                // fix this later so that field properties are part of the fieldresults
//                                // it is really getting stupid to keep looking this up
//                                var currentField = assessment.Fields.First(p => p.DatabaseColumn == currentResult.DbColumn);

//                                if (currentField.GroupId == groupId && currentResult.DbColumn.Substring(0, 3) == "chk")
//                                {
//                                    // only add each groupid once

//                                    if (currentResult.BoolValue.Value)
//                                    {
//                                        sum++;
//                                        break;
//                                    }
//                                }
//                            }
//                        }


//                        field.IntValue = sum;
//                        return sum.ToString();
//                    }
//                    if (asmntField.CalculationFunction == "ConcatenatedMissingLetters")
//                    {
//                        var unknownLetters = "";

//                        foreach (var group in assessment.FieldGroups)
//                        {
//                            var groupId = group.Id;

//                            var foundInGroup = false;

//                            foreach (var currentResult in result.FieldResults)
//                            {
//                                var currentField = assessment.Fields.First(p => p.DatabaseColumn == currentResult.DbColumn);

//                                if (currentField.GroupId == groupId && currentResult.DbColumn.Substring(0, 3) == "chk")
//                                {
//                                    if (currentResult.BoolValue.Value)
//                                    {
//                                        foundInGroup = true;
//                                        break;
//                                    }
//                                }
//                            }
//                            if (!foundInGroup)
//                            {
//                                // how to get the letter? find the field with the same groupid and print its DisplayLabel
//                                foreach (var currentField in assessment.Fields)
//                                {

//                                    if (currentField.GroupId == groupId && currentField.FieldType == "Label")
//                                    {
//                                        unknownLetters += currentField.DisplayLabel + ",";
//                                        break;
//                                    }
//                                }
//                            }
//                        }
//                        //remove trailing comma
//                        if (unknownLetters.Length > 0)
//                        {
//                            unknownLetters = unknownLetters.Substring(0, unknownLetters.Length - 1);
//                        }
//                        else
//                        {
//                            unknownLetters = "none";
//                        }
//                        field.StringValue = unknownLetters;
//                        return String.Format("'{0}'", unknownLetters); ;
//                    }
//                    return "0";
//                case "CalculatedFieldDbOnly":
//                    string stringValue = String.Empty;
//                    if (asmntField.CalculationFunction == "BenchmarkLevel")
//                    {
//                        // calculate benchmark value and return
//                        using (System.Data.IDbCommand command = Database.AsSqlServer().Connection.DbConnection.CreateCommand())
//                        {
//                            try
//                            {
//                                // FPValueId, Accuracy, CompScore (may need to calculate, since it might have changed)
//                                //Database.AsSqlServer().Connection.DbConnection.Open();
//                                command.CommandText = "dbo.ns4_udf_CalculateBenchmarkLevel";
//                                command.CommandType = CommandType.StoredProcedure;
//                                command.CommandTimeout = command.Connection.ConnectionTimeout;
//                                var outParameter = command.CreateParameter();
//                                outParameter.Direction = ParameterDirection.ReturnValue;
//                                outParameter.ParameterName = "@RETURN_VALUE";
//                                outParameter.DbType = DbType.String;
//                                outParameter.Size = 50;

//                                var fpValueId = command.CreateParameter();
//                                fpValueId.Direction = ParameterDirection.Input;
//                                fpValueId.DbType = DbType.Int32;
//                                fpValueId.ParameterName = "@FPValueID";
//                                var fpField = result.FieldResults.FirstOrDefault(p => p.DbColumn == "FPValueID");
//                                fpValueId.Value = fpField == null ? 0 : fpField.IntValue ?? 0;

//                                var accuracy = command.CreateParameter();
//                                accuracy.Direction = ParameterDirection.Input;
//                                accuracy.DbType = DbType.Int32;
//                                accuracy.ParameterName = "@Accuracy";
//                                var accuracyField = result.FieldResults.FirstOrDefault(p => p.DbColumn == "Accuracy");
//                                accuracy.Value = accuracyField == null ? 0 : accuracyField.DecimalValue ?? 0;

//                                var compScore = command.CreateParameter();
//                                compScore.Direction = ParameterDirection.Input;
//                                compScore.DbType = DbType.Int32;
//                                compScore.ParameterName = "@CompScore";
//                                int newCompScoreSum = 0;
//                                var aboutField = result.FieldResults.FirstOrDefault(p => p.DbColumn == "About");
//                                var withinField = result.FieldResults.FirstOrDefault(p => p.DbColumn == "Within");
//                                var beyondField = result.FieldResults.FirstOrDefault(p => p.DbColumn == "Beyond");
//                                var extraPtField = result.FieldResults.FirstOrDefault(p => p.DbColumn == "ExtraPt");

//                                compScore.Value = (aboutField == null ? 0 : aboutField.IntValue ?? 0) +
//                                    (withinField == null ? 0 : withinField.IntValue ?? 0) +
//                                    (beyondField == null ? 0 : beyondField.IntValue ?? 0) +
//                                    (extraPtField == null ? 0 : extraPtField.IntValue ?? 0);

//                                command.Parameters.Add(outParameter);
//                                command.Parameters.Add(fpValueId);
//                                command.Parameters.Add(accuracy);
//                                command.Parameters.Add(compScore);
//                                command.ExecuteNonQuery();
//                                stringValue = ((System.Data.SqlClient.SqlParameter)(command.Parameters["@RETURN_VALUE"])).Value.ToString();
//                            }
//                            catch (Exception ex)
//                            {

//                            }
//                        }
//                        field.StringValue = stringValue;
//                    }
//                    return String.Format("'{0}'", stringValue);

//            }

//            return String.Empty;
//        }

//        private SqlParameter CreateNullableVarCharSqlParameter(string paramValue, string paramName)
//        {
//            SqlParameter newParam = new SqlParameter();
//            newParam.SqlDbType = SqlDbType.VarChar;
//            newParam.ParameterName = paramName;

//            if (paramValue == null)
//            {
//                newParam.Value = DBNull.Value;
//            }
//            else
//            {
//                newParam.Value = paramValue;
//            }

//            return newParam;
//        }
//        private SqlParameter CreateNullableIntSqlParameter(int? paramValue, string paramName)
//        {
//            SqlParameter newParam = new SqlParameter();
//            newParam.SqlDbType = SqlDbType.Int;
//            newParam.ParameterName = paramName;

//            if (paramValue == null)
//            {
//                newParam.Value = DBNull.Value;
//            }
//            else
//            {
//                newParam.Value = paramValue;
//            }

//            return newParam;
//        }

//        #region Reports
//        public List<BenchmarkedAssessmentResult> GetBenchmarkedAssessmentResults(Assessment assessment, string fieldToRetrieve, bool isLookupColumn, int studentId, string lookupFieldName)
//        {
//            var list = new List<BenchmarkedAssessmentResult>();

//            using (System.Data.IDbCommand command = Database.AsSqlServer().Connection.DbConnection.CreateCommand())
//            {
//                try
//                {
//                    Database.AsSqlServer().Connection.DbConnection.Open();
//                    command.CommandText = String.Format("EXEC dbo._ns4_GetBenchmarkedAssessmentResults @TestDbTable='{0}',@FieldToRetrieve='{1}',@StudentId={2},@IsLookupColumn={3}, @LookupFieldName='{4}'", assessment.StorageTable,
//                fieldToRetrieve,
//                studentId,
//                isLookupColumn,
//                lookupFieldName);
//                    command.CommandType = CommandType.Text;
//                    command.CommandTimeout = command.Connection.ConnectionTimeout;

//                    using (System.Data.IDataReader reader = command.ExecuteReader())
//                    {
//                        // load datatable
//                        DataTable dt = new DataTable();
//                        dt.Load(reader);

//                        for (int i = 0; i < dt.Rows.Count; i++)
//                        {
//                            BenchmarkedAssessmentResult studentResult = new BenchmarkedAssessmentResult();
//                            list.Add(studentResult);
//                            studentResult.StudentId = (dt.Rows[i]["StudentId"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["StudentId"].ToString()) : -1;
//                            studentResult.TestDueDateID = (dt.Rows[i]["TestDueDateID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["TestDueDateID"].ToString()) : -1;
//                            studentResult.FieldValueID = (dt.Rows[i]["FieldValueID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["FieldValueID"].ToString()) : -1;
//                            studentResult.FieldDisplayValue = (dt.Rows[i]["FieldDisplayValue"] != DBNull.Value) ? dt.Rows[i]["FieldDisplayValue"].ToString() : String.Empty;
//                            studentResult.FieldSortOrder = (dt.Rows[i]["FieldSortOrder"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["FieldSortOrder"].ToString()) : -1;
//                        }
//                    }

//                }
//                catch (Exception ex)
//                {

//                }
//                finally
//                {
//                    Database.AsSqlServer().Connection.DbConnection.Close();
//                    command.Parameters.Clear();
//                }
//            }
//            return list;
//        }
//        public List<ReportInterventionResult> GetStudentInterventionsForReport(int studentId)
//        {
//            //var results = ReportInterventionResults.FromSql<ReportInterventionResult>("EXEC [_ns4_GetInterventionsForStudent] @StudentID",
//            //    new SqlParameter("StudentID", studentId));

//            //return results.ToList();
//            var list = new List<ReportInterventionResult>();

//            using (System.Data.IDbCommand command = Database.AsSqlServer().Connection.DbConnection.CreateCommand())
//            {
//                try
//                {
//                    Database.AsSqlServer().Connection.DbConnection.Open();
//                    command.CommandText = String.Format("EXEC dbo._ns4_GetInterventionsForStudent @StudentId={0}", studentId);
//                    command.CommandType = CommandType.Text;
//                    command.CommandTimeout = command.Connection.ConnectionTimeout;

//                    using (System.Data.IDataReader reader = command.ExecuteReader())
//                    {
//                        // load datatable
//                        DataTable dt = new DataTable();
//                        dt.Load(reader);

//                        for (int i = 0; i < dt.Rows.Count; i++)
//                        {
//                            ReportInterventionResult studentResult = new ReportInterventionResult();
//                            list.Add(studentResult);
//                            studentResult.ClassID = (dt.Rows[i]["StudentId"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["StudentId"].ToString()) : -1;
//                            studentResult.CurrentSchoolID = (dt.Rows[i]["CurrentSchoolID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["CurrentSchoolID"].ToString()) : -1;
//                            studentResult.EndTDDID = (dt.Rows[i]["EndTDDID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["EndTDDID"].ToString()) : (int?)null;
//                            studentResult.Description = (dt.Rows[i]["Description"] != DBNull.Value) ? dt.Rows[i]["Description"].ToString() : String.Empty;
//                            studentResult.EndOfIntervention = (dt.Rows[i]["EndOfIntervention"] != DBNull.Value) ? DateTime.Parse(dt.Rows[i]["EndOfIntervention"].ToString()) : (DateTime?)null; ;
//                            studentResult.InterventionType = (dt.Rows[i]["InterventionType"] != DBNull.Value) ? dt.Rows[i]["InterventionType"].ToString() : String.Empty;
//                            studentResult.NumLessons = (dt.Rows[i]["NumLessons"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["NumLessons"].ToString()) : -1;
//                            studentResult.StaffID = (dt.Rows[i]["StaffID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["StaffID"].ToString()) : -1;
//                            studentResult.StartOfIntervention = DateTime.Parse(dt.Rows[i]["StartOfIntervention"].ToString());
//                            studentResult.StartTDDID = (dt.Rows[i]["StartTDDID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["StartTDDID"].ToString()) : -1;
//                            studentResult.StudentID = (dt.Rows[i]["StudentID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["StudentID"].ToString()) : -1;
//                            studentResult.Tier = (dt.Rows[i]["Tier"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["Tier"].ToString()) : -1;
//                        }
//                    }

//                }
//                catch (Exception ex)
//                {

//                }
//                finally
//                {
//                    Database.AsSqlServer().Connection.DbConnection.Close();
//                    command.Parameters.Clear();
//                }
//            }
//            return list;
//        }
//        public List<BenchmarkDatesForStudentAndAssessment> GetBenchmarkDatesForStudentAndAssessment(Assessment assessment, int studentId, string fieldName)
//        {


//            //return results.ToList();
//            var list = new List<BenchmarkDatesForStudentAndAssessment>();

//            using (System.Data.IDbCommand command = Database.AsSqlServer().Connection.DbConnection.CreateCommand())
//            {
//                try
//                {
//                    Database.AsSqlServer().Connection.DbConnection.Open();
//                    command.CommandText = String.Format("EXEC _ns4_GetBenchmarkDatesForStudentAndAssessment @StudentId={0}, @AssessmentId={1}, @AssessmentField='{2}'", studentId, assessment.Id, fieldName);
//                    command.CommandType = CommandType.Text;
//                    command.CommandTimeout = command.Connection.ConnectionTimeout;

//                    using (System.Data.IDataReader reader = command.ExecuteReader())
//                    {
//                        // load datatable
//                        DataTable dt = new DataTable();
//                        dt.Load(reader);

//                        for (int i = 0; i < dt.Rows.Count; i++)
//                        {
//                            BenchmarkDatesForStudentAndAssessment studentResult = new BenchmarkDatesForStudentAndAssessment();
//                            list.Add(studentResult);
//                            studentResult.DueDate = (dt.Rows[i]["DueDate"] != DBNull.Value) ? DateTime.Parse(dt.Rows[i]["DueDate"].ToString()) : (DateTime?)null;
//                            studentResult.EightiethPercentileID = (dt.Rows[i]["EightiethPercentileID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["EightiethPercentileID"].ToString()) : -1;
//                            studentResult.GradeID = (dt.Rows[i]["GradeID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["GradeID"].ToString()) : -1;
//                            studentResult.GradeOrder = (dt.Rows[i]["GradeOrder"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["GradeOrder"].ToString()) : -1;
//                            studentResult.Hex = (dt.Rows[i]["Hex"] != DBNull.Value) ? dt.Rows[i]["Hex"].ToString() : String.Empty;
//                            studentResult.MeanID = (dt.Rows[i]["MeanID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["MeanID"].ToString()) : -1;
//                            studentResult.SectionID = (dt.Rows[i]["SectionID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["SectionID"].ToString()) : -1;
//                            studentResult.TestDueDateID = (dt.Rows[i]["TestDueDateID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["TestDueDateID"].ToString()) : -1;
//                            studentResult.TestLevelPeriodID = (dt.Rows[i]["TestLevelPeriodID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["TestLevelPeriodID"].ToString()) : -1;
//                            studentResult.TestNumber = (dt.Rows[i]["TestNumber"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["TestNumber"].ToString()) : -1;
//                            studentResult.TwentiethPercentileID = (dt.Rows[i]["TwentiethPercentileID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["TwentiethPercentileID"].ToString()) : -1;
//                            studentResult.Year = (dt.Rows[i]["Year"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["Year"].ToString()) : -1;
//                        }
//                    }

//                }
//                catch (Exception ex)
//                {

//                }
//                finally
//                {
//                    Database.AsSqlServer().Connection.DbConnection.Close();
//                    command.Parameters.Clear();
//                }
//            }
//            return list;
//        }

//        public List<StackedBarGraphResult> GetGroupedStackBarGraphResults(int schoolStartYear, int? sectionId, int? schoolId, int? gradeId, int groupingType, string groupingValue, int assessmentId, string fieldToRetrieve, bool isDecimalField, string testDbTable)
//        {



//            //return results.ToList();
//            var list = new List<StackedBarGraphResult>();

//            using (System.Data.IDbCommand command = Database.AsSqlServer().Connection.DbConnection.CreateCommand())
//            {
//                try
//                {
//                    Database.AsSqlServer().Connection.DbConnection.Open();
//                    command.CommandText = String.Format("EXEC [_ns4_report_StackedGroupedBarGraph] @SchoolStartYear = {0}, @SectionId={1}, @SchoolID={2}, @GradeID={3}, @GroupingType={4}, @GroupingValue={5}, @AssessmentID={6}, @FieldToRetrieve='{7}', @IsDecimalField={8}, @TestDbTable='{9}'", schoolStartYear,
//                        sectionId.HasValue ? sectionId.ToString() : "null",
//                        schoolId.HasValue ? schoolId.ToString() : "null",
//                        gradeId.HasValue ? gradeId.ToString() : "null",
//                        groupingType,
//                        String.IsNullOrEmpty(groupingValue) ? "null" : "'" + groupingValue + "'",
//                        assessmentId,
//                        fieldToRetrieve,
//                        isDecimalField,
//                        testDbTable);


//                    command.CommandType = CommandType.Text;
//                    command.CommandTimeout = command.Connection.ConnectionTimeout;

//                    using (System.Data.IDataReader reader = command.ExecuteReader())
//                    {
//                        // load datatable
//                        DataTable dt = new DataTable();
//                        dt.Load(reader);

//                        for (int i = 0; i < dt.Rows.Count; i++)
//                        {
//                            StackedBarGraphResult studentResult = new StackedBarGraphResult();
//                            list.Add(studentResult);
//                            studentResult.DueDate = DateTime.Parse(dt.Rows[i]["DueDate"].ToString());
//                            studentResult.TestDueDateID = Int32.Parse(dt.Rows[i]["TestDueDateID"].ToString());
//                            studentResult.ScoreGrouping = (dt.Rows[i]["ScoreGrouping"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["ScoreGrouping"].ToString()) : -1;
//                            studentResult.NumberOfResults = (dt.Rows[i]["NumberOfResults"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["NumberOfResults"].ToString()) : 0;
//                            studentResult.GroupingValue = (dt.Rows[i]["GroupingValue"] != DBNull.Value) ? dt.Rows[i]["GroupingValue"].ToString() : String.Empty;
//                        }
//                    }

//                }
//                catch (Exception ex)
//                {

//                }
//                finally
//                {
//                    Database.AsSqlServer().Connection.DbConnection.Close();
//                    command.Parameters.Clear();
//                }
//            }
//            return list;
//        }
//        #endregion

       
//    }
//}
