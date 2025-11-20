//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Reflection;
//using System.Reflection.Emit;
//using Microsoft.Data.Entity;
//using Microsoft.Data.Entity.Infrastructure;
//using NorthStar4.PCL.DTO;
//using NorthStar4.PCL.Entity;
//using PropertyAttributes = System.Reflection.PropertyAttributes;

//namespace NorthStar4.EntityFramework
//{
//    public static class Extensions
//    {
//        public static List<AssessmentStudentResult> DynamicSqlQuery(this Database database, string sql, Assessment assessment, DistrictContext districtContext)
//        {

//            List<AssessmentStudentResult> studentResults = new List<AssessmentStudentResult>();

//            using (System.Data.IDbCommand command = database.AsSqlServer().Connection.DbConnection.CreateCommand())
//            {
//                SqlDataAdapter da = new SqlDataAdapter();

//                try
//                {
//                    database.AsSqlServer().Connection.DbConnection.Open();
//                    command.CommandText = sql;
//                    command.CommandTimeout = command.Connection.ConnectionTimeout;
//                    //foreach (var param in parameters)
//                    //{
//                    //    command.Parameters.Add(param);
//                    //}

//                    using (System.Data.IDataReader reader = command.ExecuteReader())
//                    {
//                        // load datatable
//                        DataTable dt = new DataTable();
//                        dt.Load(reader);

//                        for (int i = 0; i < dt.Rows.Count; i++)
//                        {
//                            AssessmentStudentResult studentResult = new AssessmentStudentResult();
//                            int studentId = Int32.Parse(dt.Rows[i]["StudentID"].ToString());
//                            studentResult.StudentId = Int32.Parse(dt.Rows[i]["StudentID"].ToString());
//                            studentResult.ResultId = (dt.Rows[i]["ID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["ID"].ToString()) : -1;
//                            studentResult.FirstName = dt.Rows[i]["FirstName"].ToString();
//                            studentResult.MiddleName = dt.Rows[i]["MiddleName"].ToString();
//                            studentResult.LastName = dt.Rows[i]["LastName"].ToString();
//                            studentResult.FPText = dt.Rows[i]["FPText"].ToString();
//                            studentResult.FPValueID = (dt.Rows[i]["FPValueID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["FPValueID"].ToString()) : 0;
//                            studentResult.StaffId = (dt.Rows[i]["RecorderID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["RecorderID"].ToString()) : -1;
//                            studentResult.ClassId = Int32.Parse(dt.Rows[i]["InputSectionId"].ToString()); //result.GetPropValue<int>("SectionID");
//                            studentResult.TestDueDateId = (dt.Rows[i]["TestDueDateID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["TestDueDateID"].ToString()) : -1;//result.GetPropValue<int>("TestDueDateID");
//                                                                                                                                                                   //studentResult.TestDate = result.GetPropValue<DateTime>("TestDueDate");
//                            studentResults.Add(studentResult);
//                            int fieldIndex = 0;
//                            foreach (var field in assessment.Fields.OrderBy(p => p.FieldOrder))
//                            {
//                                if (!String.IsNullOrEmpty(field.DatabaseColumn))
//                                {
//                                    AssessmentFieldResult fieldResult = new AssessmentFieldResult();
//                                    studentResult.FieldResults.Add(fieldResult);
//                                    fieldResult.DbColumn = field.DatabaseColumn;
//                                    fieldResult.FieldIndex = fieldIndex;
//                                    switch (field.FieldType)
//                                    {
//                                        case "Textfield":
//                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
//                                            {
//                                                fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
//                                            }
//                                            break;
//                                        case "DecimalRange":
//                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
//                                            {
//                                                fieldResult.DecimalValue = Decimal.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
//                                            }
//                                            break;
//                                        case "DropdownRange":
//                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
//                                            {
//                                                fieldResult.IntValue = Int32.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
//                                            }
//                                            break;
//                                        case "DropdownFromDB":
//                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
//                                            {
//                                                fieldResult.IntValue = Int32.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
//                                            }
//                                            break;
//                                        case "CalculatedFieldDbBacked":
//                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
//                                            {
//                                                fieldResult.IntValue = Int32.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
//                                            }
//                                            break;
//                                        case "CalculatedFieldDbBackedString":
//                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
//                                            {
//                                                fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
//                                            }
//                                            break;
//                                        case "Checkbox":
//                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
//                                            {
//                                                fieldResult.BoolValue = Boolean.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
//                                            }
//                                            break;
//                                        case "CalculatedFieldDbOnly":
//                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
//                                            {
//                                                fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
//                                            }
//                                            break;
//                                        case "CalculatedFieldClientOnly":
//                                            // no-op
//                                            break;
//                                        default:
//                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
//                                            {
//                                                fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
//                                            }
//                                            break;
//                                    }
//                                }
//                                fieldIndex++;
//                            }
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {

//                }
//                finally
//                {
//                    database.AsSqlServer().Connection.DbConnection.Close();
//                    command.Parameters.Clear();
//                }
//            }

//            //Type resultType = builder.CreateType();

//            // turn this into a datatable

//            return studentResults; //database.AsSqlServer().SqlQuery(resultType, sql);
//        }

//        public static AssessmentHFWStudentResult HFWStudentDataEntryResults(this Database database, string sql, Assessment assessment, DistrictContext districtContext)
//        {
//            AssessmentHFWStudentResult studentResult = new AssessmentHFWStudentResult();
//            // List<AssessmentStudentResult> studentResults = new List<AssessmentStudentResult>();

//            using (System.Data.IDbCommand command = database.AsSqlServer().Connection.DbConnection.CreateCommand())
//            {
//                SqlDataAdapter da = new SqlDataAdapter();

//                try
//                {
//                    database.AsSqlServer().Connection.DbConnection.Open();
//                    command.CommandText = sql;
//                    command.CommandTimeout = command.Connection.ConnectionTimeout;
//                    //foreach (var param in parameters)
//                    //{
//                    //    command.Parameters.Add(param);
//                    //}

//                    using (System.Data.IDataReader reader = command.ExecuteReader())
//                    {
//                        // load datatable
//                        DataTable dt = new DataTable();
//                        dt.Load(reader);

//                        for (int i = 0; i < dt.Rows.Count; i++)
//                        {

//                            int studentId = Int32.Parse(dt.Rows[i]["StudentID"].ToString());
//                            studentResult.StudentId = Int32.Parse(dt.Rows[i]["StudentID"].ToString());
//                            studentResult.ResultId = (dt.Rows[i]["ID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["ID"].ToString()) : -1;
//                            studentResult.FirstName = dt.Rows[i]["FirstName"].ToString();
//                            studentResult.MiddleName = dt.Rows[i]["MiddleName"].ToString();
//                            studentResult.LastName = dt.Rows[i]["LastName"].ToString();
//                            studentResult.FPText = dt.Rows[i]["FPText"].ToString();
//                            studentResult.FPValueID = (dt.Rows[i]["FPValueID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["FPValueID"].ToString()) : 0;
//                            studentResult.StaffId = (dt.Rows[i]["RecorderID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["RecorderID"].ToString()) : -1;
//                            studentResult.ClassId = Int32.Parse(dt.Rows[i]["InputSectionId"].ToString()); //result.GetPropValue<int>("SectionID");
//                            studentResult.TestDueDateId = (dt.Rows[i]["TestDueDateID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["TestDueDateID"].ToString()) : -1;//result.GetPropValue<int>("TestDueDateID");
//                                                                                                                                                                   //studentResult.TestDate = result.GetPropValue<DateTime>("TestDueDate");
//                                                                                                                                                                   //studentResults.Add(studentResult);
//                            foreach (var field in assessment.Fields.OrderBy(p => p.FieldOrder))
//                            {
//                                if (!String.IsNullOrEmpty(field.DatabaseColumn))
//                                {
//                                    AssessmentFieldResult fieldResult = new AssessmentFieldResult();

//                                    // determine which collection to add the fieldresult to
//                                    if (String.IsNullOrEmpty(field.StorageTable))
//                                    {
//                                        studentResult.TotalFieldResults.Add(fieldResult);
//                                    }
//                                    else if (field.StorageTable.Contains("Read"))
//                                    {
//                                        studentResult.ReadFieldResults.Add(fieldResult);
//                                    }
//                                    else
//                                    {
//                                        studentResult.WriteFieldResults.Add(fieldResult);
//                                    }
//                                    //studentResult.FieldResults.Add(fieldResult);
//                                    fieldResult.DbColumn = field.DatabaseColumn;
//                                    switch (field.FieldType)
//                                    {
//                                        case "Textfield":
//                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
//                                            {
//                                                fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
//                                            }
//                                            break;
//                                        case "DecimalRange":
//                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
//                                            {
//                                                fieldResult.DecimalValue = Decimal.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
//                                            }
//                                            break;
//                                        case "DropdownRange":
//                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
//                                            {
//                                                fieldResult.IntValue = Int32.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
//                                            }
//                                            break;
//                                        case "DropdownFromDB":
//                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
//                                            {
//                                                fieldResult.IntValue = Int32.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
//                                            }
//                                            break;
//                                        case "CalculatedFieldDbBacked":
//                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
//                                            {
//                                                fieldResult.IntValue = Int32.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
//                                            }
//                                            break;
//                                        case "CalculatedFieldDbBackedString":
//                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
//                                            {
//                                                fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
//                                            }
//                                            break;
//                                        case "Checkbox":
//                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
//                                            {
//                                                fieldResult.BoolValue = Boolean.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
//                                            }
//                                            break;
//                                        case "DateCheckbox":
//                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
//                                            {
//                                                fieldResult.DateValue = DateTime.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
//                                                fieldResult.BoolValue = true;
//                                            }
//                                            else
//                                            {
//                                                fieldResult.BoolValue = false;
//                                            }
//                                            break;
//                                        case "CalculatedFieldDbOnly":
//                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
//                                            {
//                                                fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
//                                            }
//                                            break;
//                                        case "CalculatedFieldClientOnly":
//                                            // no-op
//                                            break;
//                                        default:
//                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
//                                            {
//                                                fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
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
//                    database.AsSqlServer().Connection.DbConnection.Close();
//                    command.Parameters.Clear();
//                }
//            }

//            //Type resultType = builder.CreateType();

//            // turn this into a datatable

//            return studentResult; //database.AsSqlServer().SqlQuery(resultType, sql);
//        }

//        public static List<AssessmentStudentResult> GetBASClassReportData(this Database database, string sql, Assessment assessment, DistrictContext districtContext)
//        {

//            List<AssessmentStudentResult> studentResults = new List<AssessmentStudentResult>();

//            using (System.Data.IDbCommand command = database.AsSqlServer().Connection.DbConnection.CreateCommand())
//            {
//                SqlDataAdapter da = new SqlDataAdapter();

//                try
//                {
//                    database.AsSqlServer().Connection.DbConnection.Open();
//                    command.CommandText = sql;
//                    command.CommandTimeout = command.Connection.ConnectionTimeout;
//                    //foreach (var param in parameters)
//                    //{
//                    //    command.Parameters.Add(param);
//                    //}

//                    using (System.Data.IDataReader reader = command.ExecuteReader())
//                    {
//                        // load datatable
//                        DataTable dt = new DataTable();
//                        dt.Load(reader);

//                        for (int i = 0; i < dt.Rows.Count; i++)
//                        {
//                            AssessmentStudentResult studentResult = new AssessmentStudentResult();
//                            int studentId = Int32.Parse(dt.Rows[i]["StudentID"].ToString());
//                            studentResult.StudentId = Int32.Parse(dt.Rows[i]["StudentID"].ToString());
//                            studentResult.ResultId = (dt.Rows[i]["ID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["ID"].ToString()) : -1;
//                            studentResult.FirstName = dt.Rows[i]["FirstName"].ToString();
//                            studentResult.MiddleName = dt.Rows[i]["MiddleName"].ToString();
//                            studentResult.LastName = dt.Rows[i]["LastName"].ToString();
//                            studentResult.StaffId = (dt.Rows[i]["RecorderID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["RecorderID"].ToString()) : -1;
//                            studentResult.ClassId = Int32.Parse(dt.Rows[i]["InputSectionId"].ToString()); //result.GetPropValue<int>("SectionID");
//                            studentResult.TestDueDateId = (dt.Rows[i]["TestDueDateID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["TestDueDateID"].ToString()) : -1;//result.GetPropValue<int>("TestDueDateID");
//                                                                                                                                                                   //studentResult.TestDate = result.GetPropValue<DateTime>("TestDueDate");
//                            studentResults.Add(studentResult);
//                            // change this check to something like "display on report" or something
//                            foreach (var field in assessment.Fields.Where(p => p.FieldType != "Label"))
//                            {
//                                AssessmentFieldResult fieldResult = new AssessmentFieldResult();
//                                studentResult.FieldResults.Add(fieldResult);
//                                fieldResult.DbColumn = field.DatabaseColumn;

//                                switch (field.FieldType)
//                                {
//                                    case "Textarea":
//                                    case "Textfield":
//                                    case "CalculatedFieldDbBackedString":
//                                        if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
//                                        {
//                                            fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
//                                        }
//                                        break;
//                                    case "DecimalRange":
//                                        if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
//                                        {
//                                            fieldResult.DecimalValue = Decimal.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
//                                        }
//                                        break;
//                                    case "CalculatedFieldDbBacked":
//                                    case "DropdownFromDB":
//                                    case "DropdownRange":
//                                        if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
//                                        {
//                                            fieldResult.IntValue = Int32.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
//                                        }
//                                        break;
//                                    case "Checkbox":
//                                        if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
//                                        {
//                                            fieldResult.BoolValue = bool.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
//                                        }
//                                        break;
//                                    case "DateCheckbox":
//                                        if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
//                                        {
//                                            fieldResult.DateValue = DateTime.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
//                                        }
//                                        break;
//                                    case "CalculatedFieldDbOnly":
//                                        if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
//                                        {
//                                            fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
//                                        }
//                                        break;
//                                    case "Label":
//                                    case "CalculatedFieldClientOnly":
//                                        // no-op
//                                        break;
//                                    default:
//                                        if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
//                                        {
//                                            fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
//                                        }
//                                        break;
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
//                    database.AsSqlServer().Connection.DbConnection.Close();
//                    command.Parameters.Clear();
//                }
//            }

//            //Type resultType = builder.CreateType();

//            // turn this into a datatable

//            return studentResults; //database.AsSqlServer().SqlQuery(resultType, sql);
//        }

//    }
//}