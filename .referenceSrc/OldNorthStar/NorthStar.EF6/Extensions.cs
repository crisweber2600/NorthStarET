using NorthStar4.PCL.DTO;
using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthStar.EF6
{
    public static class Extensions
    {
        public static string GetGeneratedQuery(this IDbCommand dbCommand)
        {
            var query = dbCommand.CommandText;
            foreach (SqlParameter parameter in dbCommand.Parameters)
            {
                query = query.Replace(parameter.ParameterName, parameter.Value.ToString());
            }

            return query;
        }
        public static List<AssessmentStudentResult> DynamicSqlQuery(this Database database, string sql, Assessment assessment, DistrictContext districtContext, bool summaryFieldsOnly, bool isInterventionGroup)
        {

            List<AssessmentStudentResult> studentResults = new List<AssessmentStudentResult>();

            using (System.Data.IDbCommand command = database.Connection.CreateCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();

                try
                {
                    database.Connection.Open();
                    command.CommandText = sql;
                    command.CommandTimeout = command.Connection.ConnectionTimeout;
                    //foreach (var param in parameters)
                    //{
                    //    command.Parameters.Add(param);
                    //}

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        // load datatable
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            AssessmentStudentResult studentResult = new AssessmentStudentResult();
                            int studentId = Int32.Parse(dt.Rows[i]["StudentID"].ToString());
                            studentResult.StudentId = Int32.Parse(dt.Rows[i]["StudentID"].ToString());
                            studentResult.ResultId = (dt.Rows[i]["ID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["ID"].ToString()) : -1;
                            studentResult.StudentIdentifier = dt.Rows[i]["StudentIdentifier"].ToString();

                            if (!isInterventionGroup)
                            {
                                studentResult.IsCopied = (dt.Rows[i]["IsCopied"] != DBNull.Value) ? Boolean.Parse(dt.Rows[i]["IsCopied"].ToString()) : false;
                                studentResult.FPText = dt.Rows[i]["FPText"].ToString();
                                studentResult.FPValueID = (dt.Rows[i]["FPValueSortOrder"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["FPValueSortOrder"].ToString()) : 0;
                                studentResult.TestDueDateId = (dt.Rows[i]["TestDueDateID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["TestDueDateID"].ToString()) : -1;//result.GetPropValue<int>("TestDueDateID");
                                studentResult.TestDate = (dt.Rows[i]["DateTestTaken"] != DBNull.Value) ? DateTime.Parse(dt.Rows[i]["DateTestTaken"].ToString()) : (DateTime?)null;
                            }
                            else
                            {
                                studentResult.TestDate = (dt.Rows[i]["TestDueDate"] != DBNull.Value) ? DateTime.Parse(dt.Rows[i]["TestDueDate"].ToString()) : (DateTime?)null;
                            }

                            studentResult.StudentName = dt.Rows[i]["LastName"].ToString() + ", " + dt.Rows[i]["FirstName"].ToString();

                            studentResult.StaffId = (dt.Rows[i]["RecorderID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["RecorderID"].ToString()) : -1;
                            studentResult.ClassId = Int32.Parse(dt.Rows[i]["InputSectionId"].ToString());
                            studentResult.Recorder.id = studentResult.StaffId.HasValue ? studentResult.StaffId.Value : -1;
                            studentResult.Recorder.text = districtContext.Staffs.FirstOrDefault(p => p.Id == studentResult.Recorder.id)?.FullName ?? String.Empty;
                            studentResults.Add(studentResult);
                            int fieldIndex = 0;
                            IEnumerable<AssessmentField> fieldsToRetrieve = null;
                            if(summaryFieldsOnly)
                            {
                                fieldsToRetrieve = assessment.Fields.Where(p => p.DisplayInEditResultList == true);
                            }
                            else
                            {
                                fieldsToRetrieve = assessment.Fields;
                            }

                            foreach (var field in fieldsToRetrieve.OrderBy(p => p.FieldOrder))
                            {
                                if (!String.IsNullOrEmpty(field.DatabaseColumn))
                                {
                                    AssessmentFieldResult fieldResult = new AssessmentFieldResult();
                                    studentResult.FieldResults.Add(fieldResult);
                                    fieldResult.DbColumn = field.DatabaseColumn;
                                    fieldResult.FieldIndex = fieldIndex;
                                    fieldResult.FieldType = field.FieldType;
                                    fieldResult.GroupId = field.GroupId;

                                    // add new fieldResult details by SH on 7/13/2018 - carry over to NS5
                                    fieldResult.FieldId = field.Id;
                                    fieldResult.FF1 = field.Flag1;
                                    fieldResult.FF2 = field.Flag2;
                                    fieldResult.FF3 = field.Flag3;
                                    fieldResult.FF4 = field.Flag4;
                                    fieldResult.FF5 = field.Flag5;

                                    switch (field.FieldType)
                                    {
                                        case "Textfield":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
                                            }
                                            break;
                                        case "DecimalRange":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.DecimalValue = Decimal.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
                                            }
                                            break;
                                        case "DropdownRange":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.IntValue = Int32.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
                                            }
                                            break;
                                        case "DropdownFromDB":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.IntValue = Int32.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
                                            }
                                            break;
                                        case "checklist":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();

                                                // convert list of strings to array of ints
                                                fieldResult.ChecklistValues = fieldResult.StringValue.Split(',').Select(int.Parse).ToList();
                                            }
                                            break;
                                        case "CalculatedFieldDbBacked":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.IntValue = Int32.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
                                            }
                                            break;
                                        case "CalculatedFieldDbBackedString":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
                                            }
                                            break;
                                        case "Checkbox":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.BoolValue = Boolean.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
                                            }
                                            break;
                                        case "CalculatedFieldDbOnly":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
                                            }
                                            break;
                                        case "CalculatedFieldClientOnly":
                                            // no-op
                                            break;
                                        default:
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
                                            }
                                            break;
                                    }
                                }
                                fieldIndex++;
                            }
                        }
                    }
                }
                finally
                {
                    database.Connection.Close();
                    command.Parameters.Clear();
                }
            }

            //Type resultType = builder.CreateType();

            // turn this into a datatable

            return studentResults; //database.AsSqlServer().SqlQuery(resultType, sql);
        }

        public static List<AssessmentStudentResult> GetBASClassReportData(this Database database, string sql, Assessment assessment, DistrictContext districtContext)
        {

            List<AssessmentStudentResult> studentResults = new List<AssessmentStudentResult>();

            using (System.Data.IDbCommand command = database.Connection.CreateCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();

                try
                {
                    database.Connection.Open();
                    command.CommandText = sql;
                    command.CommandTimeout = command.Connection.ConnectionTimeout;
                    //foreach (var param in parameters)
                    //{
                    //    command.Parameters.Add(param);
                    //}

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        // load datatable
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            AssessmentStudentResult studentResult = new AssessmentStudentResult();
                            int studentId = Int32.Parse(dt.Rows[i]["StudentID"].ToString());
                            studentResult.StudentId = Int32.Parse(dt.Rows[i]["StudentID"].ToString());
                            studentResult.ResultId = (dt.Rows[i]["ID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["ID"].ToString()) : -1;
                            studentResult.FirstName = dt.Rows[i]["FirstName"].ToString();
                            studentResult.MiddleName = dt.Rows[i]["MiddleName"].ToString();
                            studentResult.LastName = dt.Rows[i]["LastName"].ToString();
                            studentResult.StaffId = (dt.Rows[i]["RecorderID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["RecorderID"].ToString()) : -1;
                            studentResult.GradeId = (dt.Rows[i]["GradeId"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["GradeId"].ToString()) : -1;
                            studentResult.ClassId = Int32.Parse(dt.Rows[i]["InputSectionId"].ToString()); //result.GetPropValue<int>("SectionID");
                            studentResult.TestDueDateId = (dt.Rows[i]["TestDueDateID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["TestDueDateID"].ToString()) : -1;//result.GetPropValue<int>("TestDueDateID");
                            studentResult.TestDate = (dt.Rows[i]["DateTestTaken"] != DBNull.Value) ? DateTime.Parse(dt.Rows[i]["DateTestTaken"].ToString()) : (DateTime?)null;//studentResult.TestDate = result.GetPropValue<DateTime>("TestDueDate");
                            studentResult.Recorder.id = studentResult.StaffId.HasValue ? studentResult.StaffId.Value : -1;
                            studentResult.Recorder.text = districtContext.Staffs.FirstOrDefault(p => p.Id == studentResult.Recorder.id)?.FullName ?? String.Empty;

                            studentResults.Add(studentResult);
                            // change this check to something like "display on report" or something
                            foreach (var field in assessment.Fields.Where(p => p.FieldType != "Label"))
                            {
                                AssessmentFieldResult fieldResult = new AssessmentFieldResult();
                                studentResult.FieldResults.Add(fieldResult);
                                fieldResult.DbColumn = field.DatabaseColumn;
                                fieldResult.GroupId = field.GroupId;
                                fieldResult.FieldType = field.FieldType;

                                switch (field.FieldType)
                                {
                                    case "Textarea":
                                    case "Textfield":
                                    case "checkist":
                                    case "CalculatedFieldDbBackedString":
                                        if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                        {
                                            fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
                                        }
                                        break;
                                    case "DecimalRange":
                                        if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                        {
                                            fieldResult.DecimalValue = Decimal.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
                                        }
                                        break;
                                    case "CalculatedFieldDbBacked":
                                    case "DropdownFromDB":
                                    case "DropdownRange":
                                        if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                        {
                                            fieldResult.IntValue = Int32.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
                                        }
                                        break;
                                    case "Checkbox":
                                        if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                        {
                                            fieldResult.BoolValue = bool.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
                                        }
                                        break;
                                    case "DateCheckbox":
                                        if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                        {
                                            fieldResult.DateValue = DateTime.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
                                        }
                                        break;
                                    case "CalculatedFieldDbOnly":
                                        if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                        {
                                            fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
                                        }
                                        break;
                                    case "Label":
                                    case "CalculatedFieldClientOnly":
                                        // no-op
                                        break;
                                    default:
                                        if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                        {
                                            fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
                                        }
                                        break;
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
                    database.Connection.Close();
                    command.Parameters.Clear();
                }
            }

            //Type resultType = builder.CreateType();

            // turn this into a datatable

            return studentResults; //database.AsSqlServer().SqlQuery(resultType, sql);
        }


        public static AssessmentHFWStudentResult HFWStudentDataEntryResults(this Database database, string sql, Assessment assessment, DistrictContext districtContext)
        {
            AssessmentHFWStudentResult studentResult = new AssessmentHFWStudentResult();
            // List<AssessmentStudentResult> studentResults = new List<AssessmentStudentResult>();

            using (System.Data.IDbCommand command = database.Connection.CreateCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();

                try
                {
                    database.Connection.Open();
                    command.CommandText = sql;
                    command.CommandTimeout = command.Connection.ConnectionTimeout;
                    //foreach (var param in parameters)
                    //{
                    //    command.Parameters.Add(param);
                    //}

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        // load datatable
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {

                            int studentId = Int32.Parse(dt.Rows[i]["StudentID"].ToString());
                            studentResult.StudentId = Int32.Parse(dt.Rows[i]["StudentID"].ToString());
                            studentResult.ResultId = (dt.Rows[i]["ID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["ID"].ToString()) : -1;
                            studentResult.FirstName = dt.Rows[i]["FirstName"].ToString();
                            studentResult.MiddleName = dt.Rows[i]["MiddleName"].ToString();
                            studentResult.LastName = dt.Rows[i]["LastName"].ToString();
                      //      studentResult.FPText = dt.Rows[i]["FPText"].ToString();
                        //    studentResult.FPValueID = (dt.Rows[i]["FPValueID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["FPValueID"].ToString()) : 0;
                            studentResult.StaffId = (dt.Rows[i]["RecorderID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["RecorderID"].ToString()) : -1;
                            studentResult.ClassId = Int32.Parse(dt.Rows[i]["InputSectionId"].ToString()); //result.GetPropValue<int>("SectionID");
                            //studentResult.TestDueDateId = (dt.Rows[i]["TestDueDateID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["TestDueDateID"].ToString()) : -1;//result.GetPropValue<int>("TestDueDateID");
                                                                                                                                                                   //studentResult.TestDate = result.GetPropValue<DateTime>("TestDueDate");
                                                                                                                                                                   //studentResults.Add(studentResult);
                            foreach (var field in assessment.Fields.OrderBy(p => p.FieldOrder))
                            {
                                if (!String.IsNullOrEmpty(field.DatabaseColumn))
                                {
                                    AssessmentFieldResult fieldResult = new AssessmentFieldResult();

                                    // determine which collection to add the fieldresult to
                                    if (String.IsNullOrEmpty(field.StorageTable))
                                    {
                                        studentResult.TotalFieldResults.Add(fieldResult);
                                    }
                                    else if (field.StorageTable.Contains("Read"))
                                    {
                                        studentResult.ReadFieldResults.Add(fieldResult);
                                    }
                                    else
                                    {
                                        studentResult.WriteFieldResults.Add(fieldResult);
                                    }
                                    //studentResult.FieldResults.Add(fieldResult);
                                    fieldResult.DbColumn = field.DatabaseColumn;
                                    switch (field.FieldType)
                                    {
                                        case "Textfield":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
                                            }
                                            break;
                                        case "DecimalRange":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.DecimalValue = Decimal.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
                                            }
                                            break;
                                        case "DropdownRange":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.IntValue = Int32.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
                                            }
                                            break;
                                        case "DropdownFromDB":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.IntValue = Int32.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
                                            }
                                            break;
                                        case "checklist":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
                                            }
                                            break;
                                        case "CalculatedFieldDbBacked":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.IntValue = Int32.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
                                            }
                                            break;
                                        case "CalculatedFieldDbBackedString":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
                                            }
                                            break;
                                        case "Checkbox":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.BoolValue = Boolean.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
                                            }
                                            break;
                                        case "DateCheckbox":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.DateValue = DateTime.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
                                                fieldResult.BoolValue = true;
                                            }
                                            else
                                            {
                                                fieldResult.BoolValue = false;
                                            }
                                            break;
                                        case "CalculatedFieldDbOnly":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
                                            }
                                            break;
                                        case "CalculatedFieldClientOnly":
                                            // no-op
                                            break;
                                        default:
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
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
                    // log
                    throw ex;
                }
                finally
                {
                    database.Connection.Close();
                    command.Parameters.Clear();
                }
            }

            //Type resultType = builder.CreateType();

            // turn this into a datatable

            return studentResult; //database.AsSqlServer().SqlQuery(resultType, sql);
        }

    }
}
