using AutoMapper;
using EntityDto.DTO.Admin.District;
using EntityDto.DTO.Admin.InterventionToolkit;
using EntityDto.DTO.Admin.Simple;
using EntityDto.DTO.Admin.Student;
using NorthStar4.PCL.DTO;
using NorthStar4.PCL.Entity;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NorthStar.EF6
{
    public class DistrictSettingsDataService : NSBaseDataService
    {
        public DistrictSettingsDataService(ClaimsIdentity user, string loginConnectionString) : base(user, loginConnectionString)
        {

        }

        public class OutputDto_TestDueDates : OutputDto_Base
        {
            public List<TestDueDateDto> DueDates { get; set; }
        }

        public class OutputDto_StudentAttributes : OutputDto_Base
        {
            public List<StudentAttributeTypeDto> Attributes { get; set; }
        }

        public class OutputDto_Interventions : OutputDto_Base
        {
            public List<DistrictInterventionDto> Interventions { get; set; }
        }

        public OutputDto_TestDueDates GetBenchmarkDatesForSchoolYear(InputDto_SimpleId input)
        {
            var result = new OutputDto_TestDueDates();
            result.DueDates = Mapper.Map<List<TestDueDateDto>>(_dbContext.TestDueDates.Where(p => p.SchoolStartYear == input.Id).ToList());
            return result;
        }

        public OutputDto_StudentAttributes GetStudentAttributes()
        {
            var result = new OutputDto_StudentAttributes();
            result.Attributes = Mapper.Map<List<StudentAttributeTypeDto>>(_dbContext.StudentAttributeTypes.OrderBy(p => p.AttributeName).ToList());
            return result;
        }





        public OutputDto_Interventions GetInterventionList()
        {
            var result = new OutputDto_Interventions();
            result.Interventions = Mapper.Map<List<DistrictInterventionDto>>(_dbContext.Interventions.OrderBy(p => p.InterventionType).ToList());
            return result;
        }

        public OutputDto_SuccessAndStatus LogIn(InputDto_SimpleId input)
        {
            var result = new OutputDto_SuccessAndStatus();

            var cxnString = _loginContext.DistrictDbs.First(p => p.DistrictId == input.Id);
            var targetDistrictContext = new DistrictContext(cxnString.DbName);

            // see if we are already in the staff table
            var existingStaff = targetDistrictContext.Staffs.FirstOrDefault(p => p.Email == _currentUser.Email);
            if(existingStaff == null)
            {
                var staff = targetDistrictContext.Staffs.Add(new Staff
                {
                    Email = _currentUser.Email,
                    FirstName = _currentUser.FirstName,
                    IsInterventionSpecialist = false,
                    LastName = _currentUser.LastName,
                    LoweredUserName = _currentUser.LoweredUserName,
                    MiddleName = _currentUser.MiddleName,
                    NorthStarUserTypeID = _currentUser.NorthStarUserTypeID,
                    RoleID = _currentUser.RoleID,
                    TeacherIdentifier = _currentUser.TeacherIdentifier,
                    IsActive = true,
                    IsDistrictAdmin = true,
                    IsSA = true,
                    IsPowerUser = true
                });
                targetDistrictContext.SaveChanges();
                var staffDistrict = _loginContext.StaffDistricts.First(p => p.StaffEmail == _currentUser.Email);
                staffDistrict.DistrictId = input.Id;
                _loginContext.SaveChanges();
            } else
            {
                var staffDistrict = _loginContext.StaffDistricts.First(p => p.StaffEmail == _currentUser.Email);
                staffDistrict.DistrictId = input.Id;
                _loginContext.SaveChanges();
            }

            return result;
        }

        public OutputDto_HFWList GetHFWList(InputDto_HFWList input)
        {
            var result = new OutputDto_HFWList();

            var hfwAssessment = _dbContext.Assessments.FirstOrDefault(p => p.BaseType == NSAssessmentBaseType.HighFrequencyWords);
            if(hfwAssessment == null)
            {
                Log.Fatal("NOTICE!!! Someone has deleted the High Frequency Words Assessment!!");
                return result;
            }

            if(input.WordList == "kdg")
            {
                // TODO: eventually add ISKdg to grouping, for now, just bring 1-26
                result.Words = Mapper.Map<List<AssessmentFieldGroupDto>>(_dbContext.AssessmentFieldGroups.Where(p => p.IsKdg && p.AssessmentId == hfwAssessment.Id).ToList());

            } else
            {
                var startEnd = input.WordList.Split(Char.Parse("-"));
                var start = Int32.Parse(startEnd[0]);
                var end = Int32.Parse(startEnd[1]);

                if (input.IsAlphaOrder)
                {
                    result.Words = Mapper.Map<List<AssessmentFieldGroupDto>>(_dbContext.AssessmentFieldGroups.Where(p => p.SortOrder >= start && p.SortOrder <= end && p.AssessmentId == hfwAssessment.Id).OrderBy(p => p.SortOrder).ToList());
                } else
                {
                    result.Words = Mapper.Map<List<AssessmentFieldGroupDto>>(_dbContext.AssessmentFieldGroups.Where(p => p.AltOrder >= start && p.AltOrder <= end && p.AssessmentId == hfwAssessment.Id).OrderBy(p => p.AltOrder).ToList());
                }
            }

            return result;
        }

        public OutputDto_SuccessAndStatus SaveHfw(AssessmentFieldGroupDto input)
        {
            var result = new OutputDto_SuccessAndStatus();

            var db_word = _dbContext.AssessmentFieldGroups.FirstOrDefault(p => p.Id == input.Id);

            if(db_word != null)
            {
                db_word.DisplayName = input.DisplayName;
                db_word.SortOrder = input.SortOrder;
                db_word.AltOrder = input.AltOrder;
                db_word.IsKdg = input.IsKdg;
                _dbContext.SaveChanges();

            }
            else
            {
                Log.Error("Attempting to save a HFW and the ID does not exist!!  Should never happen.");
                result.Status.StatusCode = StatusCode.UnhandledException;
                result.Status.StatusMessage = "An error occurred while saving this word.";
            }

            return result;
        }

        public OutputDto_SuccessAndStatus SaveIntervention(DistrictInterventionDto input)
        {
            var result = new OutputDto_SuccessAndStatus();

            var db_intervention = _dbContext.Interventions.FirstOrDefault(p => p.Id == input.Id);

            // make sure we're not re-using an already existing name
            var nameExists = _dbContext.Interventions.FirstOrDefault(p => p.InterventionType == input.InterventionType && p.Id != input.Id);
            if (nameExists != null)
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = String.Format("The Intervention '{0}' is already in use.  Please choose a different name.", input.InterventionType);
                return result;
            }

            if (db_intervention != null)
            {
                db_intervention.InterventionType = input.InterventionType;
                db_intervention.Description = input.Description;
                db_intervention.bDisplay = input.bDisplay;
            }
            else
            {
                db_intervention = _dbContext.Interventions.Create();
                db_intervention.InterventionType = input.InterventionType;
                db_intervention.Description = input.Description;
                db_intervention.bDisplay = input.bDisplay;
                _dbContext.Interventions.Add(db_intervention);
            }

            _dbContext.SaveChanges();

            return result;
        }

        public OutputDto_SuccessAndStatus SaveAttribute(StudentAttributeTypeDto input)
        {
            var result = new OutputDto_SuccessAndStatus();

            var db_attribute = _dbContext.StudentAttributeTypes.FirstOrDefault(p => p.Id == input.Id);

            // make sure we're not re-using an already existing name
            var nameExists = _dbContext.StudentAttributeTypes.FirstOrDefault(p => p.AttributeName == input.AttributeName && p.Id != input.Id);
            if (nameExists != null)
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = String.Format("The Attribute '{0}' has already been created.  Please choose a different name.", input.AttributeName);
                return result;
            }

            if (db_attribute != null)
            {
                db_attribute.AttributeName = input.AttributeName;
            }
            else
            {
                // make sure attribute count is less than = 10
                var attCount = _dbContext.StudentAttributeTypes.ToList().Count;

                if(attCount >= 10)
                {
                    result.Status.StatusCode = StatusCode.UserDisplayableException;
                    result.Status.StatusMessage = "The current version of North Star only supports a maximum of 10 attributes.  You must delete an attribute before adding another.";
                    return result;
                }

                // when creating a new attribute, we must create an attribute value called "unspecified", set is value to 0 and assign it to all existing students
                db_attribute = _dbContext.StudentAttributeTypes.Create();
                db_attribute.AttributeName = input.AttributeName;
                _dbContext.StudentAttributeTypes.Add(db_attribute);
                _dbContext.SaveChanges();

                var unspecifiedLookupValue = _dbContext.StudentAttributeLookupValues.Create();
                unspecifiedLookupValue.LookupValueID = 0;
                unspecifiedLookupValue.AttributeID = db_attribute.Id;
                unspecifiedLookupValue.LookupValue = "Unspecified";
                _dbContext.StudentAttributeLookupValues.Add(unspecifiedLookupValue);
                _dbContext.SaveChanges();

                // add an 'uspecified' record for all students so that stacked bar graphs are accurate
                using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
                {
                    try
                    {
                        _dbContext.Database.Connection.Open();
                        command.CommandTimeout = command.Connection.ConnectionTimeout;
                        command.CommandText = String.Format("_ns4_prime_new_student_attribute {0}", db_attribute.Id);
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error thrown while Adding new Attribute: {0}, Message is: {1}", db_attribute.AttributeName, ex.Message);
                    }
                    finally
                    {
                        _dbContext.Database.Connection.Close();
                        command.Parameters.Clear();
                    }
                }
                //foreach (var student in _dbContext.Students)
                //{
                //    var newAttributeRecord = _dbContext.StudentAttributeDatas.Create();
                //    newAttributeRecord.AttributeID = db_attribute.Id;
                //    newAttributeRecord.AttributeValueID = 0;
                //    newAttributeRecord.StudentID = student.Id;
                //    _dbContext.StudentAttributeDatas.Add(newAttributeRecord);
                //}
            }

            _dbContext.SaveChanges();

            return result;
        }

        public OutputDto_SuccessAndStatus DeleteAttribute(StudentAttributeTypeDto input)
        {
            // check if it is in use... may need to a proc to search the assessment tables... will just depend on foreign key exception for now
            var result = new OutputDto_SuccessAndStatus();

            var db_attribute = _dbContext.StudentAttributeTypes.FirstOrDefault(p => p.Id == input.Id);

            // can't delete special Ed
            if (input.Id == 4)
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "You cannot delete the Student Services attribute.  It is used in multiple places throughout the system.";
                return result;
            }

            if (db_attribute != null)
            {
                try
                {
                    //var studentAttributeDataToDelete = _dbContext.StudentAttributeDatas.Where(p => p.AttributeID == db_attribute.Id);
                    //_dbContext.StudentAttributeDatas.RemoveRange(studentAttributeDataToDelete);


                    using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
                    {
                        try
                        {
                            _dbContext.Database.Connection.Open();
                            command.CommandTimeout = command.Connection.ConnectionTimeout;
                            command.CommandText = String.Format("_ns4_delete_student_attribute {0}", db_attribute.Id);
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Error thrown while Deleting Attribute: {0}, Message is: {1}", db_attribute.AttributeName, ex.Message);
                        }
                        finally
                        {
                            _dbContext.Database.Connection.Close();
                            command.Parameters.Clear();
                        }
                    }

                    var lookupValuesToDelete = _dbContext.StudentAttributeLookupValues.Where(p => p.AttributeID == db_attribute.Id);
                    _dbContext.StudentAttributeLookupValues.RemoveRange(lookupValuesToDelete);

                    _dbContext.StudentAttributeTypes.Remove(db_attribute);
                    _dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    Log.Error("Exception thrown while deleting a student attribute: {0}", ex.Message);
                    result.Status.StatusCode = StatusCode.UserDisplayableException;
                    result.Status.StatusMessage = "Unable to delete this student attribute.  Please contact North Star support.";
                }
            }

            return result;
        }

        public OutputDto_SuccessAndStatus SaveAttributeValue(StudentAttributeLookupValueDto input)
        {
            var result = new OutputDto_SuccessAndStatus();

            var db_attributeVal = _dbContext.StudentAttributeLookupValues.FirstOrDefault(p => p.Id == input.Id);

            // make sure we're not re-using an already existing name
            var nameExists = _dbContext.StudentAttributeLookupValues.FirstOrDefault(p => p.LookupValue == input.LookupValue && p.Id != input.Id && p.AttributeID == input.AttributeId);
            if (nameExists != null)
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = String.Format("The Attribute Value '{0}' has already been created for this attribute.  Please choose a different name.", input.LookupValue);
                return result;
            }

            if (db_attributeVal != null)
            {
                db_attributeVal.LookupValue = input.LookupValue; 
                db_attributeVal.Description = input.Description;
                db_attributeVal.IsSpecialEd = input.IsSpecialEd;
            }
            else
            {
                var newLookupValue = _dbContext.StudentAttributeLookupValues.Where(p => p.AttributeID == input.AttributeId).Max(p => p.LookupValueID);
                if (newLookupValue.HasValue)
                {
                    newLookupValue++;
                }
                else
                {
                    newLookupValue = 1;
                }
                // when creating a new attribute, we must create an attribute value called "unspecified", set is value to 0 and assign it to all existing students
                db_attributeVal = _dbContext.StudentAttributeLookupValues.Create();
                db_attributeVal.Description = input.Description;
                db_attributeVal.LookupValue = input.LookupValue;
                db_attributeVal.LookupValueID = newLookupValue.Value;
                db_attributeVal.AttributeID = input.AttributeId;
                db_attributeVal.IsSpecialEd = input.IsSpecialEd;
                _dbContext.StudentAttributeLookupValues.Add(db_attributeVal);
            }

            _dbContext.SaveChanges();

            return result;
        }

        public OutputDto_SuccessAndStatus DeleteAttributeValue(StudentAttributeLookupValueDto input)
        {
            // check if it is in use... may need to a proc to search the assessment tables... will just depend on foreign key exception for now
            var result = new OutputDto_SuccessAndStatus();

            var db_attributeVal = _dbContext.StudentAttributeLookupValues.FirstOrDefault(p => p.Id == input.Id);

            var attCount = _dbContext.StudentAttributeLookupValues.Where(p => p.AttributeID == input.AttributeId).ToList().Count;

            if (attCount <= 1)
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "At least one attribute value must exist for each attribute.  You cannot delete the last attribute value.";
                return result;
            }

            if (db_attributeVal != null)
            {
                try
                {
                    var studentAttributeValDataToDelete = _dbContext.StudentAttributeDatas.Where(p => p.AttributeValueID == db_attributeVal.LookupValueID && p.AttributeID == db_attributeVal.AttributeID);
                    _dbContext.StudentAttributeDatas.RemoveRange(studentAttributeValDataToDelete);

                    _dbContext.StudentAttributeLookupValues.Remove(db_attributeVal);
                    _dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    Log.Error("Exception thrown while deleting a student attribute value: {0}", ex.Message);
                    result.Status.StatusCode = StatusCode.UserDisplayableException;
                    result.Status.StatusMessage = "Unable to delete this student attribute value.  Please contact North Star support.";
                }
            }

            return result;
        }

        public OutputDto_SuccessAndStatus DeleteIntervention(DistrictInterventionDto input)
        {
            // check if it is in use... may need to a proc to search the assessment tables... will just depend on foreign key exception for now
            var result = new OutputDto_SuccessAndStatus();

            var db_intervention = _dbContext.Interventions.FirstOrDefault(p => p.Id == input.Id);

            if (db_intervention != null)
            {
                try
                {
                    // intervention can't be deleted if there are any interventions groups that reference it... that's it
                    var inUse = _dbContext.InterventionGroups.FirstOrDefault(p => p.InterventionTypeID == db_intervention.Id);

                    if(inUse != null)
                    {
                        result.Status.StatusCode = StatusCode.UserDisplayableException;
                        result.Status.StatusMessage = "This Intervention is in use and cannot be deleted.";
                        return result;
                    }

                    _dbContext.Interventions.Remove(db_intervention);
                    _dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    Log.Error("Exception thrown while deleting an intervention: {0}", ex.Message);
                    result.Status.StatusCode = StatusCode.UserDisplayableException;
                    result.Status.StatusMessage = "This Intervention is in use and cannot be deleted.";
                }
            }


            return result;
        }

        public OutputDto_SuccessAndStatus DeleteBenchmarkDate(TestDueDateDto input)
        {
            // check if it is in use... may need to a proc to search the assessment tables... will just depend on foreign key exception for now
            var result = new OutputDto_SuccessAndStatus();

            var db_tdd = _dbContext.TestDueDates.FirstOrDefault(p => p.Id == input.Id);

            if (db_tdd != null)
            {
                try
                {
                    // check assessment tables first
                    var results = _dbContext.Database.SqlQuery<int>(@"EXEC [_ns4_tdd_delete_check] @tddid", new SqlParameter("tddid", input.Id)).ToList();

                    if(results.Count > 0)
                    {
                        result.Status.StatusCode = StatusCode.UserDisplayableException;
                        result.Status.StatusMessage = "This test due date is in use and cannot be deleted.";
                        return result;
                    }

                   _dbContext.TestDueDates.Remove(db_tdd);
                    _dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    result.Status.StatusCode = StatusCode.UserDisplayableException;
                    result.Status.StatusMessage = "This test due date is in use and cannot be deleted.";
                }
            }


            return result;
        }

        public OutputDto_SuccessAndStatus SaveTestDueDate(TestDueDateDto input)
        {
            var result = new OutputDto_SuccessAndStatus();

            // validate tdd
            var db_tdd = _dbContext.TestDueDates.FirstOrDefault(p => p.Id == input.Id);

            // if this is a new test due date
            if(db_tdd == null)
            {
                db_tdd = new TestDueDate();
                _dbContext.TestDueDates.Add(db_tdd);
            }
            
            // rules
            // 1. only one benchmark period of each type unless it is supplemental
            if (_dbContext.TestDueDates.FirstOrDefault(p => p.TestLevelPeriodID == input.TestLevelPeriodID && (p.IsSupplemental != true && input.IsSupplemental != true) && p.SchoolStartYear == input.SchoolStartYear && p.Id != input.Id) != null) {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "Only one non-supplemental test due date can exist for each benchmark period.";
                return result;
            }
            // 2. default to this date on must be before the due date
            if (input.StartDate > input.DueDate)
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "You cannot default to a benchmark date after the start of the benchmark period.";
                return result;
            }
            // 3. color should be unique
            if (_dbContext.TestDueDates.FirstOrDefault(p => p.Hex == input.Hex && p.SchoolStartYear == input.SchoolStartYear && p.Id != input.Id) != null)
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "The color for each benchmark date must be unique.";
                return result;
            }

            Mapper.Map<TestDueDateDto, TestDueDate>(input, db_tdd);


            _dbContext.SaveChanges();




            return result;
        }
    }
}
