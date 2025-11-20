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
using EntityDto.DTO.Admin.District;
using NorthStar4.CrossPlatform.DTO.Reports.ObservationSummary;

namespace NorthStar.EF6
{
    public class AssessmentDataService : NSBaseDataService
    {
        public AssessmentDataService(ClaimsIdentity user, string loginConnectionString) : base(user, loginConnectionString)
        {
        }

        public OutputDto_SuccessAndStatus CreateHRISSentence(InputDto_CreateHRISWSentence input)
        {
            var existingAssessment = _dbContext.Assessments
                .First(p => p.Id == input.AssessmentId);

            if (existingAssessment == null)
                throw new Exception("No assessment exists with this ID");

            var fields = input.Sentence.Split(Char.Parse("|"));

            var index = 0;
            foreach(var field in fields)
            {
                index++;
                // for running multiple times
                AssessmentField newField = _dbContext.AssessmentFields.FirstOrDefault(p => p.AssessmentId == input.AssessmentId && p.DatabaseColumn == "col_" + input.FormId + "_" + index);

                if(newField == null)
                {
                    newField = new AssessmentField();
                    newField.AssessmentId = input.AssessmentId;
                    existingAssessment.Fields.Add(newField);
                }

                // add suffix as groupid
                newField.GroupId = Int32.Parse(input.Suffix);

                // will have delete blank labels manually - no easy way to avoid replication
                if(field == " ")
                {
                    newField.Page = input.FormId;
                    newField.FieldType = "Label";
                    newField.DisplayLabel = "";
                } else
                {
                    newField.FieldType = "Checkbox";
                    newField.Page = input.FormId;
                    newField.DisplayLabel = field;
                    newField.ObsSummaryLabel = field;
                    newField.DatabaseColumn = "col_" + input.FormId + "_" + index;
                }
                newField.FieldOrder = index;
                
            }

            _dbContext.SaveChanges();
            // Update the db table
            using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
            {
                try
                {
                    _dbContext.Database.Connection.Open();
                    command.CommandTimeout = command.Connection.ConnectionTimeout;
                    command.CommandText = String.Format("ns4_UpdateAssessmentTable {0}", input.AssessmentId);
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Log.Error("Error thrown while update Assessment Table: {0}, Message is: {1}", existingAssessment.StorageTable, ex.Message);
                }
                finally
                {
                    _dbContext.Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }
            return new OutputDto_SuccessAndStatus { isValid = true, value = "" };
        }

        // TODO: eventually, this will create a new table, get rid of the TDDID column and add a Date column instead
        public OutputDto_SuccessAndStatus CopyAsInterventionTest(InputDto_SimpleId input)
        {
            var existingAssessment = _dbContext.Assessments
                .First(p => p.Id == input.Id);

            // create a copy of the assessment
            Assessment copy = _dbContext.Assessments.Add(existingAssessment);
            copy.AssessmentName = copy.AssessmentName + "_INTERVENTION";
            copy.StorageTable = copy.StorageTable + "_INTERVENTION";
            copy.TestType = 2; // means intervention
            _dbContext.SaveChanges();

            // now get the fields
            var existingAssessmentFields = _dbContext.AssessmentFields.Where(p => p.AssessmentId == input.Id).ToList();

            // now copy the fields
            foreach (var currentField in existingAssessmentFields)
            {
                var originalField = (AssessmentField)currentField.Clone();
                AssessmentField newField = _dbContext.AssessmentFields.Add(originalField);
                newField.PreviousId = currentField.Id;
                newField.AssessmentId = copy.Id;

                // now check for it's field group and make sure it has been added
                if (currentField.GroupId.HasValue)
                {
                    // first check to see if this fieldgroup has already been added by another field
                    var alreadyCopiedFg = _dbContext.AssessmentFieldGroups.FirstOrDefault(p => p.PreviousId == currentField.GroupId && p.AssessmentId == copy.Id);

                    // if it's already been copied, we are all set
                    if (alreadyCopiedFg != null)
                    {
                        newField.GroupId = alreadyCopiedFg.Id;
                    }
                    else
                    {
                        // make a copy of the corresponding field group
                        var originalFg = _dbContext.AssessmentFieldGroups.First(p => p.Id == currentField.GroupId);
                        var originalFgCopy = (AssessmentFieldGroup)originalFg.Clone();
                        var newFg = _dbContext.AssessmentFieldGroups.Add(originalFgCopy);
                        newFg.PreviousId = originalFg.Id;
                        newFg.AssessmentId = copy.Id;
                        newField.GroupId = newFg.Id;
                    }
                }
                // now check for it's field category and make sure it has been added
                if (currentField.CategoryId.HasValue)
                {
                    // first check to see if this cat has already been added by another field
                    var alreadyCopiedCat = _dbContext.AssessmentFieldCategories.FirstOrDefault(p => p.PreviousId == currentField.CategoryId && p.AssessmentId == copy.Id);

                    // if it's already been copied, we are all set
                    if (alreadyCopiedCat != null)
                    {
                        newField.CategoryId = alreadyCopiedCat.Id;
                    }
                    else
                    {
                        // make a copy of the corresponding cat
                        var originalCat = _dbContext.AssessmentFieldCategories.First(p => p.Id == currentField.CategoryId);
                        var originalCatCopy = (AssessmentFieldCategory)originalCat.Clone();
                        var newCat = _dbContext.AssessmentFieldCategories.Add(originalCatCopy);
                        newCat.PreviousId = originalCat.Id;
                        newCat.AssessmentId = copy.Id;
                        newField.CategoryId = newCat.Id;
                    }
                }
                // now check for it's field subcategory and make sure it has been added
                if (currentField.SubcategoryId.HasValue)
                {
                    // first check to see if this subcategory has already been added by another field
                    var alreadyCopiedSC = _dbContext.AssessmentFieldSubCategories.FirstOrDefault(p => p.PreviousId == currentField.SubcategoryId && p.AssessmentId == copy.Id);

                    // if it's already been copied, we are all set
                    if (alreadyCopiedSC != null)
                    {
                        newField.SubcategoryId = alreadyCopiedSC.Id;
                    }
                    else
                    {
                        // make a copy of the corresponding subcategory
                        var originalSC = _dbContext.AssessmentFieldSubCategories.First(p => p.Id == currentField.SubcategoryId);
                        var originalSCCopy = (AssessmentFieldSubCategory)originalSC.Clone();
                        var newSC = _dbContext.AssessmentFieldSubCategories.Add(originalSCCopy);
                        newSC.PreviousId = originalSC.Id;
                        newSC.AssessmentId = copy.Id;
                        newField.SubcategoryId = newSC.Id;
                    }
                }
                _dbContext.SaveChanges();
            }

            return new OutputDto_SuccessAndStatus { isValid = true, value = "" };
        }

        public OutputDto_SuccessAndStatus SimpleCopy(InputDto_SimpleId input)
        {

            var existingAssessment = _dbContext.Assessments.First(p => p.Id == input.Id);

            // create a copy of the assessment
            Assessment copy = _dbContext.Assessments.Add(existingAssessment);
            copy.AssessmentName += "_COPY";
            copy.StorageTable += "_COPY";
            _dbContext.SaveChanges();

            var existingAssessmentFields = _dbContext.AssessmentFields.Where(p => p.AssessmentId == input.Id).ToList();

            // now copy the fields
            foreach (var currentField in existingAssessmentFields)
            {
                var originalField = (AssessmentField)currentField.Clone();
                AssessmentField newField = _dbContext.AssessmentFields.Add(originalField);
                newField.PreviousId = originalField.Id;
                newField.AssessmentId = copy.Id;

                // now check for it's field group and make sure it has been added
                if (currentField.GroupId.HasValue)
                {
                    // first check to see if this fieldgroup has already been added by another field
                    var alreadyCopiedFg = _dbContext.AssessmentFieldGroups.FirstOrDefault(p => p.PreviousId == currentField.GroupId && p.AssessmentId == copy.Id);

                    // if it's already been copied, we are all set
                    if (alreadyCopiedFg != null)
                    {
                        newField.GroupId = alreadyCopiedFg.Id;
                    }
                    else
                    {
                        // make a copy of the corresponding field group
                        var originalFg = _dbContext.AssessmentFieldGroups.First(p => p.Id == currentField.GroupId);
                        var originalFgCopy = (AssessmentFieldGroup)originalFg.Clone();
                        var newFg = _dbContext.AssessmentFieldGroups.Add(originalFgCopy);
                        newFg.PreviousId = originalFg.Id;
                        newFg.AssessmentId = copy.Id;
                        newField.GroupId = newFg.Id;
                    }
                }
                // now check for it's field category and make sure it has been added
                if (currentField.CategoryId.HasValue)
                {
                    // first check to see if this cat has already been added by another field
                    var alreadyCopiedCat = _dbContext.AssessmentFieldCategories.FirstOrDefault(p => p.PreviousId == currentField.CategoryId && p.AssessmentId == copy.Id);

                    // if it's already been copied, we are all set
                    if (alreadyCopiedCat != null)
                    {
                        newField.CategoryId = alreadyCopiedCat.Id;
                    }
                    else
                    {
                        // make a copy of the corresponding cat
                        var originalCat = _dbContext.AssessmentFieldCategories.First(p => p.Id == currentField.CategoryId);
                        var originalCatCopy = (AssessmentFieldCategory)originalCat.Clone();
                        var newCat = _dbContext.AssessmentFieldCategories.Add(originalCatCopy);
                        newCat.PreviousId = originalCat.Id;
                        newCat.AssessmentId = copy.Id;
                        newField.CategoryId = newCat.Id;
                    }
                }
                // now check for it's field subcategory and make sure it has been added
                if (currentField.SubcategoryId.HasValue)
                {
                    // first check to see if this subcategory has already been added by another field
                    var alreadyCopiedSC = _dbContext.AssessmentFieldSubCategories.FirstOrDefault(p => p.PreviousId == currentField.SubcategoryId && p.AssessmentId == copy.Id);

                    // if it's already been copied, we are all set
                    if (alreadyCopiedSC != null)
                    {
                        newField.SubcategoryId = alreadyCopiedSC.Id;
                    }
                    else
                    {
                        // make a copy of the corresponding subcategory
                        var originalSC = _dbContext.AssessmentFieldSubCategories.First(p => p.Id == currentField.SubcategoryId);
                        var originalSCCopy = (AssessmentFieldSubCategory)originalSC.Clone();
                        var newSC = _dbContext.AssessmentFieldSubCategories.Add(originalSCCopy);
                        newSC.PreviousId = originalSC.Id;
                        newSC.AssessmentId = copy.Id;
                        newField.SubcategoryId = newSC.Id;
                    }
                }
                _dbContext.SaveChanges();
            }

            // TODO: call service that creates the table

            return new OutputDto_SuccessAndStatus { isValid = true, value = "" };
        }

        public AssessmentDto GetAssessmentById(int id)
        {
            var assessment = _dbContext.Assessments
                                .Include(p => p.FieldCategories)
                                .Include(p => p.FieldSubCategories)
                                .Include(p => p.FieldGroupContainers)
                                .FirstOrDefault(m => m.Id == id);
            if (assessment == null)
            {
                return new AssessmentDto();
            }
            else
            {
                assessment.Fields = new List<AssessmentField>();
                assessment.FieldCategories = assessment.FieldCategories.OrderBy(p => p.SortOrder).ToList();
                assessment.FieldSubCategories = assessment.FieldSubCategories.OrderBy(p => p.SortOrder).ToList();
                assessment.FieldGroupContainers = assessment.FieldGroupContainers.OrderBy(p => p.SortOrder).ToList();
                assessment.FieldGroups = new List<AssessmentFieldGroup>();
                return Mapper.Map<AssessmentDto>(assessment);
            }
        }

        public OutputDto_LoadAssessmentFields GetFieldsForAssessment(InputDto_GetAssessmentFields input)
        {
            var result = new OutputDto_LoadAssessmentFields();

            //throw new InvalidOperationException("Ghost in the machine!");
            var assessment = _dbContext.Assessments.Include(p => p.Fields).First(p => p.Id == input.assessmentId);

            var fields = assessment.Fields.Where(p =>
           (input.groupId == 0 || input.groupId == p.GroupId)
           && (input.categoryId == 0 || input.categoryId == p.CategoryId)
           && (input.subCategoryId == 0 || input.subCategoryId == p.SubcategoryId)
           && (input.page == 0 || input.page == p.Page)
           && (input.dbTable == "primary" || p.StorageTable == input.dbTable)
            ).ToList();

            result.Fields =  Mapper.Map<List<AssessmentFieldDto>>(fields);
            return result;
        }

        public OutputDto_District GetDistrictList()
        {
            var result = new OutputDto_District();

            var districts = _loginContext.Districts.ToList();
            result.Districts =  Mapper.Map<List<DistrictDto>>(districts);

            return result;
        }

        public OutputDto_LoadAssessmentFieldGroups GetGroupsForAssessment(InputDto_GetAssessmentFieldGroups input)
        {
            var result = new OutputDto_LoadAssessmentFieldGroups(); 

            var assessment = _dbContext.Assessments.Include(p => p.FieldGroups).Include(p => p.FieldGroupContainers).First(p => p.Id == input.assessmentId);

            var fields = assessment.FieldGroups.Where(p =>
           p.SortOrder >= input.startOrder && p.SortOrder <= input.endOrder).ToList();

            result.Groups = Mapper.Map<List<AssessmentFieldGroupDto>>(fields);
            return result;
        }

        public OutputDto_SuccessAndStatus CopyAssessmentToNewDistrict(InputDto_CopyAssessmentToDistrict input)
        {
            try
            {

                // get district connection string
                var cxnString = _loginContext.DistrictDbs.First(p => p.DistrictId == input.DistrictId);
                var targetDistrictContext = new DistrictContext(cxnString.DbName);

                var sourceAssessment = _dbContext.Assessments                    
                    .Include(p => p.FieldGroups)
                    .Include(p => p.FieldCategories)
                    .Include(p => p.FieldGroupContainers)
                    .Include(p => p.FieldSubCategories)
                    .Include(p => p.Fields)
                    .FirstOrDefault(p => p.Id == input.AssessmentId);

                // existing fieldgroup
                //var existingAssessmentFieldGroupContainers = _dbContext.AssessmentFieldGroupContainers.Where(p => p.AssessmentId == input.AssessmentId).ToList();
                //var existingAssessmentFields = _dbContext.AssessmentFields.Where(p => p.AssessmentId == input.AssessmentId).ToList();
                
                // get the new assessmentid
                var assessmentCopy = targetDistrictContext.Assessments.Add(sourceAssessment.Clone() as Assessment);

                // now copy the categories
                foreach (var category in sourceAssessment.FieldCategories)
                {
                    AssessmentFieldCategory newCategory = targetDistrictContext.AssessmentFieldCategories.Add(category.Clone() as AssessmentFieldCategory);
                    newCategory.PreviousId = category.Id;
                    newCategory.AssessmentId = assessmentCopy.Id;
                }

                // now copy the subcategories
                foreach (var subcat in sourceAssessment.FieldSubCategories)
                {
                    AssessmentFieldSubCategory newSubCat = targetDistrictContext.AssessmentFieldSubCategories.Add(subcat.Clone() as AssessmentFieldSubCategory);
                    newSubCat.PreviousId = subcat.Id;
                    newSubCat.AssessmentId = assessmentCopy.Id;
                }
                // now copy the groupcontainers
                foreach (var container in sourceAssessment.FieldGroupContainers)
                {
                    AssessmentFieldGroupContainer newContainer = targetDistrictContext.AssessmentFieldGroupContainers.Add(container.Clone() as AssessmentFieldGroupContainer);
                    newContainer.PreviousId = container.Id;
                    newContainer.AssessmentId = assessmentCopy.Id;
                }
                // now copy the fieldgroups
                foreach (var group in sourceAssessment.FieldGroups)
                {
                    AssessmentFieldGroup newGroup = targetDistrictContext.AssessmentFieldGroups.Add(group.Clone() as AssessmentFieldGroup);
                    newGroup.PreviousId = group.Id;
                    newGroup.AssessmentId = assessmentCopy.Id;

                    // now check for it's field category and make sure it has been added
                    if (group.AssessmentFieldGroupContainerId.HasValue)
                    {
                        // first check to see if this cat has already been added by another field, using First since that indicates a referential integrity problem if its not there
                        var alreadyCopiedContainer = targetDistrictContext.AssessmentFieldGroupContainers.Local.First(p => p.PreviousId == group.AssessmentFieldGroupContainerId && p.AssessmentId == assessmentCopy.Id);
                        newGroup.Container = alreadyCopiedContainer;
                    }
                }
                // now copy the fields
                foreach (var currentField in sourceAssessment.Fields)
                {
                    AssessmentField newField = targetDistrictContext.AssessmentFields.Add(currentField.Clone() as AssessmentField);
                    newField.PreviousId = currentField.Id;
                    newField.AssessmentId = assessmentCopy.Id;

                    // now check for it's field category and make sure it has been added
                    if (currentField.CategoryId.HasValue)
                    {
                        // first check to see if this cat has already been added by another field, using First since that indicates a referential integrity problem if its not there
                        var alreadyCopiedCat = targetDistrictContext.AssessmentFieldCategories.Local.First(p => p.PreviousId == currentField.CategoryId && p.AssessmentId == assessmentCopy.Id);
                        newField.Category = alreadyCopiedCat;
                    }

                    // now check for it's field subcategory and make sure it has been added
                    if (currentField.SubcategoryId.HasValue)
                    {
                        // first check to see if this subcategory has already been added by another field
                        var alreadyCopiedSC = targetDistrictContext.AssessmentFieldSubCategories.Local.First(p => p.PreviousId == currentField.SubcategoryId && p.AssessmentId == assessmentCopy.Id);
                        newField.SubCategory = alreadyCopiedSC;
                    }
                    if (currentField.GroupId.HasValue)
                    {
                        // first check to see if this fieldgroup has already been added by another field
                        var alreadyCopiedFg = targetDistrictContext.AssessmentFieldGroups.Local.FirstOrDefault(p => p.PreviousId == currentField.GroupId && p.AssessmentId == assessmentCopy.Id);
                        newField.Group = alreadyCopiedFg;
                    }
                }

                // bring over any benchmarks
                var existingBenchmarks = _dbContext.AssessmentBenchmarks.Where(p => p.AssessmentID == sourceAssessment.Id);
                foreach (var benchmark in existingBenchmarks)
                {
                    Assessment_Benchmarks newBenchmark = targetDistrictContext.AssessmentBenchmarks.Add(benchmark.Clone() as Assessment_Benchmarks);
                    newBenchmark.Assessment = assessmentCopy;
                }


                // last step is to copy the db table... add a guid to the table name
                var tableSuffix = Guid.NewGuid().ToString().Replace("-","_").Substring(0,4);
                var newTableName = sourceAssessment.StorageTable + "_" + tableSuffix;
                assessmentCopy.StorageTable = newTableName;

                var tableSql = String.Empty;

                // copy the assessment
                // get the sql for the current table
                using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
                {
                    try
                    {
                        if (_dbContext.Database.Connection.State != ConnectionState.Open) _dbContext.Database.Connection.Open();
                        command.CommandText = String.Format("EXEC dbo.ns4_ScriptExistingTable @currentTableName='{0}',@tableSuffix='{1}'", sourceAssessment.StorageTable, tableSuffix);
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = command.Connection.ConnectionTimeout;

                        tableSql = command.ExecuteScalar().ToString();                        

                    }
                    finally
                    {
                        _dbContext.Database.Connection.Close();
                    }
                }

                using (System.Data.IDbCommand command = targetDistrictContext.Database.Connection.CreateCommand())
                {
                    try
                    {
                        if (targetDistrictContext.Database.Connection.State != ConnectionState.Open) targetDistrictContext.Database.Connection.Open();
                        command.CommandText = String.Format("{0}", tableSql);
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = command.Connection.ConnectionTimeout;

                       command.ExecuteNonQuery();

                    }
                    finally
                    {
                        targetDistrictContext.Database.Connection.Close();
                    }
                }

                targetDistrictContext.SaveChanges();
                // we should now have the new assessmentId



                // add a new record to districtassessments to make it avaialble and show as available

                // what about benchmarks???  should bring those too
                // Need to make sure that the fields are unique!!!
            } catch(Exception ex)
            {
                Log.Error("Error while copying assessment to new district: {0}", ex.Message);
                return new OutputDto_SuccessAndStatus() { isValid = false, value = "Error copying assessment.  See log." };
            }
            // it would be great to do this stuff in a transaction?????
            return new OutputDto_SuccessAndStatus() { isValid = true };
        } 

        public bool SaveAssessment(InputDto_Assessment assessment)
        {

            var db_assessment = _dbContext.Assessments
                .Include(p => p.FieldGroups)
                .Include(p => p.FieldCategories)
                .Include(p => p.FieldGroupContainers)
                .Include(p => p.FieldSubCategories)
                .Include(p => p.Fields)
                .FirstOrDefault(p => p.Id == assessment.Id);

            // if this is a new assessment
            if (db_assessment == null)
            {
                var newAssessment = new Assessment()
                {
                    AssessmentName = assessment.AssessmentName,
                    AssessmentDescription = assessment.AssessmentDescription,
                    StorageTable = assessment.StorageTable,
                    ClassReportPages = assessment.ClassReportPages,
                    DefaultDataEntryPage = assessment.DefaultDataEntryPage,
                    TestType = assessment.TestType,
                    BaseType = assessment.BaseType,
                    CanImport = assessment.CanImport
                };

                db_assessment = _dbContext.Assessments.Add(newAssessment);
                _dbContext.SaveChanges();
                //var newid = saved.Id;

                //db_assessment = _dbContext.Assessments
                //    .First(p => p.Id == newid);
            } else
            {
                db_assessment.AssessmentName = assessment.AssessmentName;
                db_assessment.AssessmentDescription = assessment.AssessmentDescription;
                db_assessment.ClassReportPages = assessment.ClassReportPages;
                db_assessment.DefaultDataEntryPage = assessment.DefaultDataEntryPage;
                db_assessment.TestType = assessment.TestType;
                db_assessment.BaseType = assessment.BaseType;
                db_assessment.CanImport = assessment.CanImport;
            }

            #region FieldGroupContainers
            // remove deleted fieldgroupContainers
            var groupContainersToRemove = new List<AssessmentFieldGroupContainer>();
            db_assessment.FieldGroupContainers
                    .Where(d => assessment.FieldGroupContainers.Any(fg => fg.Id == d.Id && fg.IsFlaggedForDelete))
                    .Each(deleted => groupContainersToRemove.Add(deleted));
            foreach (var container in groupContainersToRemove)
            {
                // remove from the database and the in-memory collection
                _dbContext.AssessmentFieldGroups.Where(p => p.AssessmentFieldGroupContainerId == container.Id).Each(p => p.AssessmentFieldGroupContainerId = null);
                assessment.FieldGroups.Where(p => p.AssessmentFieldGroupContainerId == container.Id).Each(p => p.AssessmentFieldGroupContainerId = null);
            }
            _dbContext.AssessmentFieldGroupContainers.RemoveRange(groupContainersToRemove);

            //update or add details
            assessment.FieldGroupContainers.Where(p => !p.IsFlaggedForDelete).Each(fg =>
            {
                // determine if we are adding a new one, or updating an existing one
                var fieldGroupContainer = db_assessment.FieldGroupContainers.FirstOrDefault(d => d.Id == fg.Id);
                if (fieldGroupContainer == null)
                {
                    fieldGroupContainer = new AssessmentFieldGroupContainer();
                    var newFieldGroupContainer = _dbContext.AssessmentFieldGroupContainers.Add(fieldGroupContainer);
                    fieldGroupContainer.DisplayName = fg.DisplayName;
                    fieldGroupContainer.Description = fg.Description;
                    fieldGroupContainer.SortOrder = fg.SortOrder;
                    fieldGroupContainer.ModifiedDate = DateTime.Now;
                    fieldGroupContainer.AssessmentId = db_assessment.Id;
                    fieldGroupContainer.Flag1 = fg.Flag1;
                    fieldGroupContainer.Flag2 = fg.Flag2;
                    fieldGroupContainer.Flag3 = fg.Flag3;
                    fieldGroupContainer.Flag4 = fg.Flag4;
                    fieldGroupContainer.Flag5 = fg.Flag5;

                    _dbContext.SaveChanges();
                    fieldGroupContainer.Id = newFieldGroupContainer.Id;

                    // if we don't have this field group container in the DB already, we need to update any fields that may reference it
                    assessment.FieldGroups
                      .Where(d => d.AssessmentFieldGroupContainerId == fg.Id) // fg.id is the client side ID
                      .Each(p => p.AssessmentFieldGroupContainerId = newFieldGroupContainer.Id);
                }
                else
                {
                    fieldGroupContainer.DisplayName = fg.DisplayName;
                    fieldGroupContainer.Description = fg.Description;
                    fieldGroupContainer.SortOrder = fg.SortOrder;
                    fieldGroupContainer.ModifiedDate = DateTime.Now;
                    fieldGroupContainer.Flag1 = fg.Flag1;
                    fieldGroupContainer.Flag2 = fg.Flag2;
                    fieldGroupContainer.Flag3 = fg.Flag3;
                    fieldGroupContainer.Flag4 = fg.Flag4;
                    fieldGroupContainer.Flag5 = fg.Flag5;
                }
            });
            #endregion

            #region FieldGroups
            // remove deleted fieldgroups
            var groupsToRemove = new List<AssessmentFieldGroup>();
            db_assessment.FieldGroups
                    .Where(d => assessment.FieldGroups.Any(fg => fg.Id == d.Id && fg.IsFlaggedForDelete))
                    .Each(deleted => groupsToRemove.Add(deleted));
            foreach (var group in groupsToRemove)
            {
                _dbContext.AssessmentFields.Where(p => p.GroupId == group.Id).Each(p => p.GroupId = null);
                assessment.Fields.Where(p => p.GroupId == group.Id).Each(p => p.GroupId = null);
            }
            _dbContext.AssessmentFieldGroups.RemoveRange(groupsToRemove);

            //update or add details
            assessment.FieldGroups.Where(p => !p.IsFlaggedForDelete).Each(fg =>
            {
                var fieldGroup = db_assessment.FieldGroups.FirstOrDefault(d => d.Id == fg.Id);
                if (fieldGroup == null)
                {
                    fieldGroup = new AssessmentFieldGroup();
                    var newFieldGroup = _dbContext.AssessmentFieldGroups.Add(fieldGroup);
                    fieldGroup.DisplayName = fg.DisplayName;
                    fieldGroup.Description = fg.Description;
                    fieldGroup.SortOrder = fg.SortOrder;
                    fieldGroup.ModifiedDate = DateTime.Now;
                    fieldGroup.AssessmentId = db_assessment.Id;
                    fieldGroup.AssessmentFieldGroupContainerId = fg.AssessmentFieldGroupContainerId;
                    fieldGroup.IsKdg = fg.IsKdg;
                    fieldGroup.Flag1 = fg.Flag1;
                    fieldGroup.Flag2 = fg.Flag2;
                    fieldGroup.Flag3 = fg.Flag3;
                    fieldGroup.Flag4 = fg.Flag4;
                    fieldGroup.Flag5 = fg.Flag5;

                    _dbContext.SaveChanges();
                    fieldGroup.Id = newFieldGroup.Id;

                    // if we don't have this field group already, we need to update any fields that may reference it
                    assessment.Fields
              .Where(d => d.GroupId == fg.Id)
              .Each(p => p.GroupId = newFieldGroup.Id);
                }
                else
                {
                    fieldGroup.AssessmentFieldGroupContainerId = fg.AssessmentFieldGroupContainerId;
                    fieldGroup.DisplayName = fg.DisplayName;
                    fieldGroup.Description = fg.Description;
                    fieldGroup.SortOrder = fg.SortOrder;
                    fieldGroup.ModifiedDate = DateTime.Now;                    
                    fieldGroup.IsKdg = fg.IsKdg;
                    fieldGroup.Flag1 = fg.Flag1;
                    fieldGroup.Flag2 = fg.Flag2;
                    fieldGroup.Flag3 = fg.Flag3;
                    fieldGroup.Flag4 = fg.Flag4;
                    fieldGroup.Flag5 = fg.Flag5;
                }


            });
            #endregion

            #region Categories
            // remove deleted categories
            var catsToRemove = new List<AssessmentFieldCategory>();
            db_assessment.FieldCategories
                    .Where(d => assessment.FieldCategories.All(fg => fg.Id != d.Id))
                    .Each(deleted => catsToRemove.Add(deleted));

            foreach (var cat in catsToRemove)
            {
                _dbContext.AssessmentFields.Where(p => p.CategoryId == cat.Id).Each(p => p.CategoryId = null);
                assessment.Fields.Where(p => p.CategoryId == cat.Id).Each(p => p.CategoryId = null);
            }
            _dbContext.AssessmentFieldCategories.RemoveRange(catsToRemove);
            //update or add details
            assessment.FieldCategories.Each(fg =>
            {
                var cat = db_assessment.FieldCategories.FirstOrDefault(d => d.Id == fg.Id);
                if (cat == null)
                {
                    cat = new AssessmentFieldCategory();
                    var newCategory = _dbContext.AssessmentFieldCategories.Add(cat);
                    cat.DisplayName = fg.DisplayName;
                    cat.AltDisplayLabel = fg.AltDisplayLabel;
                    cat.Description = fg.Description;
                    cat.SortOrder = fg.SortOrder;
                    cat.ModifiedDate = DateTime.Now;
                    cat.AssessmentId = db_assessment.Id;

                    cat.Flag1 = fg.Flag1;
                    cat.Flag2 = fg.Flag2;
                    cat.Flag3 = fg.Flag3;
                    cat.Flag4 = fg.Flag4;
                    cat.Flag5 = fg.Flag5;

                    _dbContext.SaveChanges();
                    cat.Id = newCategory.Id;

                    // if we don't have this field group already, we need to update any fields that may reference it
                    assessment.Fields
              .Where(d => d.CategoryId == fg.Id)
              .Each(p => p.CategoryId = newCategory.Id);
                }
                else
                {
                    cat.DisplayName = fg.DisplayName;
                    cat.AltDisplayLabel = fg.AltDisplayLabel;
                    cat.Description = fg.Description;
                    cat.SortOrder = fg.SortOrder;
                    cat.ModifiedDate = DateTime.Now;

                    cat.Flag1 = fg.Flag1;
                    cat.Flag2 = fg.Flag2;
                    cat.Flag3 = fg.Flag3;
                    cat.Flag4 = fg.Flag4;
                    cat.Flag5 = fg.Flag5;
                }
            });
            #endregion

            #region Sub-Categories
            // remove deleted subcategories
            var subCatsToRemove = new List<AssessmentFieldSubCategory>();
            db_assessment.FieldSubCategories
                .Where(d => assessment.FieldSubCategories.All(fg => fg.Id != d.Id))
                .Each(deleted => subCatsToRemove.Add(deleted));

            // remove this reference from any fields that are using it
            foreach (var sc in subCatsToRemove)
            {
                _dbContext.AssessmentFields.Where(p => p.SubcategoryId == sc.Id).Each(p => p.SubcategoryId = null);
                assessment.Fields.Where(p => p.SubcategoryId == sc.Id).Each(p => p.SubcategoryId = null);
            }
            _dbContext.AssessmentFieldSubCategories.RemoveRange(subCatsToRemove);

            //update or add details
            assessment.FieldSubCategories.Each(fg =>
            {
                var cat = db_assessment.FieldSubCategories.FirstOrDefault(d => d.Id == fg.Id);
                if (cat == null)
                {
                    cat = new AssessmentFieldSubCategory();
                    var newSubCategory = _dbContext.AssessmentFieldSubCategories.Add(cat);
                    cat.DisplayName = fg.DisplayName;
                    cat.Description = fg.Description;
                    cat.SortOrder = fg.SortOrder;
                    cat.ModifiedDate = DateTime.Now;
                    cat.AssessmentId = db_assessment.Id;

                    cat.Flag1 = fg.Flag1;
                    cat.Flag2 = fg.Flag2;
                    cat.Flag3 = fg.Flag3;
                    cat.Flag4 = fg.Flag4;
                    cat.Flag5 = fg.Flag5;

                    _dbContext.SaveChanges();
                    cat.Id = newSubCategory.Id;

                    // if we don't have this field group already, we need to update any fields that may reference it
                    assessment.Fields
              .Where(d => d.SubcategoryId == fg.Id)
              .Each(p => p.SubcategoryId = newSubCategory.Id);
                }
                else
                {
                    cat.DisplayName = fg.DisplayName;
                    cat.Description = fg.Description;
                    cat.SortOrder = fg.SortOrder;
                    cat.ModifiedDate = DateTime.Now;

                    cat.Flag1 = fg.Flag1;
                    cat.Flag2 = fg.Flag2;
                    cat.Flag3 = fg.Flag3;
                    cat.Flag4 = fg.Flag4;
                    cat.Flag5 = fg.Flag5;
                }
            });
            #endregion

            #region Fields
            // remove deleted fields
            //db_assessment.Fields
            //    .Where(d => assessment.Fields.All(fg => fg.Id != d.Id) 
            //    && (assessment.SelectedCategory == null || assessment.SelectedCategory == 0 || assessment.SelectedCategory == d.CategoryId) 
            //    && (assessment.SelectedSubcategory == null || assessment.SelectedSubcategory == null || assessment.SelectedSubcategory == d.SubcategoryId)
            //    && (assessment.SelectedGroup == null || assessment.SelectedGroup == null || assessment.SelectedGroup == d.GroupId)
            //    && (assessment.SelectedPage == null || assessment.SelectedPage == null || assessment.SelectedPage == d.Page))
            //    .Each(deleted => _dbContext.AssessmentFields.Remove(deleted));
            // instead of this approach, only remove ones that are FlaggedForDeletion
            var fieldsToRemove = new List<AssessmentField>();
            db_assessment.Fields
                    .Where(d => assessment.Fields.Any(fg => fg.Id == d.Id && fg.IsFlaggedForDelete))
                    .Each(deleted => fieldsToRemove.Add(deleted));
            _dbContext.AssessmentFields.RemoveRange(fieldsToRemove);

            //update or add details
            assessment.Fields.Where(p => !p.IsFlaggedForDelete).Each(fg =>
            {
                var field = db_assessment.Fields.FirstOrDefault(d => d.Id == fg.Id);
                if (field == null)
                {
                    field = new AssessmentField();
                    db_assessment.Fields.Add(field);
                }
                field.DatabaseColumn = fg.DatabaseColumn;
                field.DisplayLabel = fg.DisplayLabel;
                field.Description = fg.Description;
                field.GroupId = fg.GroupId;
                field.CategoryId = fg.CategoryId;
                field.Page = fg.Page;
                field.AltDisplayLabel = fg.AltDisplayLabel;
                field.UniqueImportColumnName = fg.UniqueImportColumnName;
                field.SubcategoryId = fg.SubcategoryId;
                field.CalculationFunction = fg.CalculationFunction;
                field.CalculationFields = fg.CalculationFields;
                field.DisplayInEditResultList = fg.DisplayInEditResultList;
                field.DisplayInLineGraphSummaryTable = fg.DisplayInLineGraphSummaryTable;
                field.DisplayInObsSummary = fg.DisplayInObsSummary;
                field.FieldOrder = fg.FieldOrder;
                field.RangeHigh = fg.RangeHigh;
                field.RangeLow = fg.RangeLow;
                field.FieldType = fg.FieldType;
                field.ModifiedDate = DateTime.Now;
                field.LookupFieldName = fg.LookupFieldName;
                field.ObsSummaryLabel = fg.ObsSummaryLabel;
                field.DisplayInStackedBarGraphSummary = fg.DisplayInStackedBarGraphSummary;
                field.Flag1 = fg.Flag1;
                field.Flag2 = fg.Flag2;
                field.Flag3 = fg.Flag3;
                field.Flag4 = fg.Flag4;
                field.Flag5 = fg.Flag5;
            });
            #endregion

            _dbContext.SaveChanges();

            // last thing... update the database table
            using (System.Data.IDbCommand command = _dbContext.Database.Connection.CreateCommand())
            {
                try
                {
                    _dbContext.Database.Connection.Open();
                    command.CommandTimeout = command.Connection.ConnectionTimeout;
                    command.CommandText = String.Format("ns4_UpdateAssessmentTable {0}", db_assessment.Id);
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Log.Error("Error thrown while update Assessment Table: {0}, Message is: {1}", db_assessment.StorageTable, ex.Message);
                }
                finally
                {
                    _dbContext.Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }
            return true;

        }

        public OutputDto_Base DeleteAssessment(int id)
        {
            var result = new OutputDto_Base();

            var assessment = _dbContext.Assessments.Include(p => p.FieldGroups)
                                .Include(p => p.FieldCategories)
                                .Include(p => p.FieldSubCategories)
                                .Include(p => p.FieldGroupContainers)
                                .Include(p => p.Fields).FirstOrDefault(m => m.Id == id);

            // prevent accidental deletions
            if(assessment.BaseType != NSAssessmentBaseType.Other && assessment.BaseType != null && assessment.BaseType != 0)
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "Cannot delete this type of Assessment.";

                return result;
            }

            _dbContext.AssessmentFields.RemoveRange(assessment.Fields);
            _dbContext.AssessmentFieldGroupContainers.RemoveRange(assessment.FieldGroupContainers);
            _dbContext.AssessmentFieldGroups.RemoveRange(assessment.FieldGroups);
            _dbContext.AssessmentFieldCategories.RemoveRange(assessment.FieldCategories);
            _dbContext.AssessmentFieldSubCategories.RemoveRange(assessment.FieldSubCategories);
            _dbContext.Assessments.Remove(assessment);
            _dbContext.SaveChanges();

            return result;
        }

        public OutputDto_StudentAttributes GetStudentAttributeList()
        {
            // get all attributes
            var allAttributes = _dbContext.StudentAttributeTypes.Where(p => p.Id != 4).ToList();

            // set visibility based on settings from DB
            var visibleAttributes = _dbContext.StaffStudentAttributes.Where(p => p.StaffId == _currentUser.Id).Select(j => j.AttributeId);

            var attributeList = new List<StudentAttributeVisibilityDto>();

            foreach(var att in allAttributes)
            {
                var newAtt = new StudentAttributeVisibilityDto() { id = att.Id, Name = att.AttributeName };
                // see if in visible list
                if (visibleAttributes.Contains(att.Id))
                {
                    newAtt.Visible = true;
                }
                else
                {
                    newAtt.Visible = false;
                }

                attributeList.Add(newAtt);
            }

            OutputDto_StudentAttributes result = new OutputDto_StudentAttributes();
            result.Attributes = attributeList;

            return result;
        }

        public OutputDto_ObservationSummaryAssessments GetObservationSummaryAssessmentList()
        {
            var response = new OutputDto_ObservationSummaryAssessments();
            //response.BenchmarkAssessments = Mapper.Map<List<OutputDto_ObservationSummaryFieldVisibility>>(_dbContext.Assessments.Where(p => p.TestType == 1 && ((p.AssessmentIsAvailable.HasValue && p.AssessmentIsAvailable.Value) || (p.AssessmentIsAvailable == null))).ToList());
            var allAssessmentsICanAccess = _dbContext.Assessments.Where(p => (p.AssessmentIsAvailable.HasValue && p.AssessmentIsAvailable.Value) || (p.AssessmentIsAvailable == null)).ToList();

            // remove any that are removed by the schools
            // get all of the schoolIds that I have access to
            var schoolIds = _dbContext.StaffSchools.Where(p => p.StaffID == _currentUser.Id).Select(p => p.SchoolID).ToList();
            var schoolAssessmentsICantAccess = new List<Assessment>();

            foreach (var districtAccesssibleAssessment in allAssessmentsICanAccess)
            {
                var schoolAssessments = _dbContext.SchoolAssessments.Where(p => schoolIds.Contains(p.SchoolId) && p.AssessmentId == districtAccesssibleAssessment.Id);
                if (schoolAssessments.Count() > 0)
                {
                    if (schoolAssessments.All(p => !p.AssessmentIsAvailable))
                    {
                        schoolAssessmentsICantAccess.Add(districtAccesssibleAssessment);
                    }
                }
            }

            // remove assessments that ALL schools have said are not available
            allAssessmentsICanAccess.RemoveAll(p => schoolAssessmentsICantAccess.Contains(p));

            // remove any that are hidden by the user
            var staffAssessmentsICantAccess = _dbContext.StaffAssessments.Where(p => p.StaffId == _currentUser.Id && !p.AssessmentIsAvailable).Select(p => p.Assessment).ToList();
            allAssessmentsICanAccess.RemoveAll(p => staffAssessmentsICantAccess.Contains(p));

            // set to hidden any that are hidden
            response.BenchmarkAssessments = Mapper.Map<List<OutputDto_ObservationSummaryFieldVisibility>>(allAssessmentsICanAccess.Where(p => p.TestType == 1).ToList());
            response.StateTests = Mapper.Map<List<OutputDto_ObservationSummaryFieldVisibility>>(allAssessmentsICanAccess.Where(p => p.TestType == 3).ToList());

            response.BenchmarkAssessments.Each(p =>
            {
                p.Visible = _dbContext.StaffObservationSummaryAssessments.FirstOrDefault(g => g.AssessmentId == p.id && g.StaffId == _currentUser.Id) == null ? true : false;
            });

            response.StateTests.Each(p =>
            {
                p.Visible = _dbContext.StaffObservationSummaryAssessments.FirstOrDefault(g => g.AssessmentId == p.id && g.StaffId == _currentUser.Id) == null ? true : false;
            });

            // get settings to determine if OS Columns visible or not
            var osSchoolDb = _dbContext.StaffSettings.FirstOrDefault(p => p.StaffId == _currentUser.Id && p.Attribute == NSConstants.StaffSettingTypes.OSSchoolColumn);
            response.OSSchoolVisible = osSchoolDb == null ? true : Boolean.Parse(osSchoolDb.SelectedValueId);

            var osGradeDb = _dbContext.StaffSettings.FirstOrDefault(p => p.StaffId == _currentUser.Id && p.Attribute == NSConstants.StaffSettingTypes.OSGradeColumn);
            response.OSGradeVisible = osGradeDb == null ? true : Boolean.Parse(osGradeDb.SelectedValueId);

            var osTeacherDb = _dbContext.StaffSettings.FirstOrDefault(p => p.StaffId == _currentUser.Id && p.Attribute == NSConstants.StaffSettingTypes.OSTeacherColumn);
            response.OSTeacherVisible = osTeacherDb == null ? true : Boolean.Parse(osTeacherDb.SelectedValueId);

            return response;
        }

        public OutputDto_ObservationSummaryAssessmentsFields GetObservationSummaryAssessmentFieldList(InputDto_SimpleId input)
        {
            var response = new OutputDto_ObservationSummaryAssessmentsFields();
            //response.BenchmarkAssessments = Mapper.Map<List<OutputDto_ObservationSummaryFieldVisibility>>(_dbContext.Assessments.Where(p => p.TestType == 1 && ((p.AssessmentIsAvailable.HasValue && p.AssessmentIsAvailable.Value) || (p.AssessmentIsAvailable == null))).ToList());
            var allFields = _dbContext.AssessmentFields.Where(p => p.AssessmentId == input.Id && p.DisplayInObsSummary == true).ToList();
            response.Fields = Mapper.Map<List<OutputDto_ObservationSummaryFieldVisibility>>(allFields);

            response.Fields.Each(p =>
            {
                p.Visible = _dbContext.StaffObservationSummaryAssessmentFields.FirstOrDefault(g => g.AssessmentFieldId == p.id && g.StaffId == _currentUser.Id) == null ? true : false;
            });

            return response;
        }

        public OutputDto_Base UpdateObservationSummaryAssessmentVisibility(InputDto_ObservationSummaryFieldVisibility data)
        {
            var response = new OutputDto_Base();

            foreach(var input in data.AssessmentsOrFields)
            { 
                var assessment = _dbContext.StaffObservationSummaryAssessments.FirstOrDefault(p => p.AssessmentId == input.id && p.StaffId == _currentUser.Id);

                if (assessment == null)
                {
                    if (!input.Visible)
                    {
                        assessment = new StaffObservationSummaryAssessment { AssessmentId = input.id, StaffId = _currentUser.Id, Hidden = true };
                        _dbContext.StaffObservationSummaryAssessments.Add(assessment);
                    }
                }
                else
                {
                    if (input.Visible)
                    {
                        _dbContext.StaffObservationSummaryAssessments.Remove(assessment);
                    }
                }
            }


            // update OS columns
            var osSchoolDb = _dbContext.StaffSettings.FirstOrDefault(p => p.StaffId == _currentUser.Id && p.Attribute == NSConstants.StaffSettingTypes.OSSchoolColumn);
            if (osSchoolDb == null)
            {
                osSchoolDb = _dbContext.StaffSettings.Create();
                osSchoolDb.StaffId = _currentUser.Id;
                osSchoolDb.Attribute = NSConstants.StaffSettingTypes.OSSchoolColumn;
                osSchoolDb.SelectedValueId = data.OSSchoolColumnVisible?.ToString() ?? "false";
                _dbContext.StaffSettings.Add(osSchoolDb);
            } else
            {
                osSchoolDb.SelectedValueId = data.OSSchoolColumnVisible?.ToString() ?? "false";
            }

            var osGradeDb = _dbContext.StaffSettings.FirstOrDefault(p => p.StaffId == _currentUser.Id && p.Attribute == NSConstants.StaffSettingTypes.OSGradeColumn);
            if (osGradeDb == null)
            {
                osGradeDb = _dbContext.StaffSettings.Create();
                osGradeDb.StaffId = _currentUser.Id;
                osGradeDb.Attribute = NSConstants.StaffSettingTypes.OSGradeColumn;
                osGradeDb.SelectedValueId = data.OSGradeColumnVisible?.ToString() ?? "false";
                _dbContext.StaffSettings.Add(osGradeDb);
            } else
            {
                osGradeDb.SelectedValueId = data.OSGradeColumnVisible?.ToString() ?? "false";
            }

            var osTeacherDb = _dbContext.StaffSettings.FirstOrDefault(p => p.StaffId == _currentUser.Id && p.Attribute == NSConstants.StaffSettingTypes.OSTeacherColumn);
            if (osTeacherDb == null)
            {
                osTeacherDb = _dbContext.StaffSettings.Create();
                osTeacherDb.StaffId = _currentUser.Id;
                osTeacherDb.Attribute = NSConstants.StaffSettingTypes.OSTeacherColumn;
                osTeacherDb.SelectedValueId = data.OSTeacherColumnVisible?.ToString() ?? "false";
                _dbContext.StaffSettings.Add(osTeacherDb);
            } else
            {
                osTeacherDb.SelectedValueId = data.OSTeacherColumnVisible?.ToString() ?? "false";
            }

            _dbContext.SaveChanges();

            return response;
        }

        public OutputDto_Base UpdateStudentAttributeVisibility(OutputDto_StudentAttributes data)
        {
            var response = new OutputDto_Base();

            foreach (var input in data.Attributes)
            {
                var att = _dbContext.StaffStudentAttributes.FirstOrDefault(p => p.AttributeId == input.id && p.StaffId == _currentUser.Id);

                if (att == null)
                {
                    if (input.Visible)
                    {
                        att = new StaffStudentAttribute { AttributeId = input.id, StaffId = _currentUser.Id, Visible = true };
                        _dbContext.StaffStudentAttributes.Add(att);
                    }
                }
                else
                {
                    if (!input.Visible)
                    {
                        _dbContext.StaffStudentAttributes.Remove(att);
                    }
                }
            }

            _dbContext.SaveChanges();

            return response;
        }

        public OutputDto_Base UpdateObservationSummaryColumnVisibility(InputDto_SimpleString data)
        {
            var response = new OutputDto_Base();

            switch (data.value)
            {
                case "school":
                    var osSchoolDb = _dbContext.StaffSettings.FirstOrDefault(p => p.StaffId == _currentUser.Id && p.Attribute == NSConstants.StaffSettingTypes.OSSchoolColumn);
                    if (osSchoolDb == null)
                    {
                        osSchoolDb = _dbContext.StaffSettings.Create();
                        osSchoolDb.StaffId = _currentUser.Id;
                        osSchoolDb.Attribute = NSConstants.StaffSettingTypes.OSSchoolColumn;
                        osSchoolDb.SelectedValueId = "false";
                        _dbContext.StaffSettings.Add(osSchoolDb);
                    }
                    else
                    {
                        osSchoolDb.SelectedValueId = "false";
                    }
                    break;
                case "grade":
                    var osGradeDb = _dbContext.StaffSettings.FirstOrDefault(p => p.StaffId == _currentUser.Id && p.Attribute == NSConstants.StaffSettingTypes.OSGradeColumn);
                    if (osGradeDb == null)
                    {
                        osGradeDb = _dbContext.StaffSettings.Create();
                        osGradeDb.StaffId = _currentUser.Id;
                        osGradeDb.Attribute = NSConstants.StaffSettingTypes.OSGradeColumn;
                        osGradeDb.SelectedValueId = "false";
                        _dbContext.StaffSettings.Add(osGradeDb);
                    }
                    else
                    {
                        osGradeDb.SelectedValueId = "false";
                    }
                    break;
                case "teacher":
                    var osTeacherDb = _dbContext.StaffSettings.FirstOrDefault(p => p.StaffId == _currentUser.Id && p.Attribute == NSConstants.StaffSettingTypes.OSTeacherColumn);
                    if (osTeacherDb == null)
                    {
                        osTeacherDb = _dbContext.StaffSettings.Create();
                        osTeacherDb.StaffId = _currentUser.Id;
                        osTeacherDb.Attribute = NSConstants.StaffSettingTypes.OSTeacherColumn;
                        osTeacherDb.SelectedValueId = "false";
                        _dbContext.StaffSettings.Add(osTeacherDb);
                    } else
                    {
                        osTeacherDb.SelectedValueId = "false";
                    }
                    break;
            }



            _dbContext.SaveChanges();

            return response;
        }

        public OutputDto_Base UpdateObservationSummaryAssessmentFieldVisibility(InputDto_ObservationSummaryFieldVisibility data)
        {
            var response = new OutputDto_Base();

            foreach (var input in data.AssessmentsOrFields)
            {
                var fieldAssessmentId = _dbContext.AssessmentFields.First(p => p.Id == input.id).AssessmentId; // if we don't find it... throw an exception
                var field = _dbContext.StaffObservationSummaryAssessmentFields.FirstOrDefault(p => p.AssessmentId == fieldAssessmentId && p.AssessmentFieldId == input.id && p.StaffId == _currentUser.Id);

                if (field == null)
                {
                    if (!input.Visible)
                    {
                        field = new StaffObservationSummaryAssessmentField { AssessmentId = fieldAssessmentId, AssessmentFieldId = input.id, StaffId = _currentUser.Id, Hidden = true };
                        _dbContext.StaffObservationSummaryAssessmentFields.Add(field);
                    }
                }
                else
                {
                    if (input.Visible)
                    {
                        _dbContext.StaffObservationSummaryAssessmentFields.Remove(field);
                    }
                }
            }
            _dbContext.SaveChanges();

            return response;
        }

        public OutputDto_ObservationSummaryAssessmentsFields GetObservationSummaryAssessmentFields(InputDto_SimpleId input)
        {
            var response = new OutputDto_ObservationSummaryAssessmentsFields();
            response.Fields = Mapper.Map<List<OutputDto_ObservationSummaryFieldVisibility>>(_dbContext.AssessmentFields
                .Where(p => p.DisplayInObsSummary == true).ToList());

            return response;
        }

        public OutputDto_AssessmentList GetAssessmentList()
        {
            var response = new OutputDto_AssessmentList();
            var assessments = _dbContext.Assessments.Include(p => p.FieldSubCategories).ToList();
            response.Assessments = Mapper.Map<List<AssessmentListDto>>(assessments);

            return response;
        }
        public OutputDto_SchoolAssessments GetSchoolAssessments(InputDto_SimpleId input)
        {
            var response = new OutputDto_SchoolAssessments();
            var assessments = _dbContext.Assessments.ToList();
            var schoolAssessments = _dbContext.SchoolAssessments.Where(p => p.SchoolId == p.SchoolId).ToList();
            var assessmentReturnList = new List<SchoolAssessmentDto>();

            foreach (var assessment in assessments)
            {
                var newSchoolAssessment = new SchoolAssessmentDto()
                {
                    IsDisabled = (!assessment.AssessmentIsAvailable.HasValue ? false : !assessment.AssessmentIsAvailable.Value),
                    AssessmentId = assessment.Id,
                    SchoolId = input.Id,
                    AssessmentDescription = assessment.AssessmentDescription,
                    AssessmentName = assessment.AssessmentName,
                    TestType = assessment.TestType.Value,
                    AssessmentIsAvailable = (!assessment.AssessmentIsAvailable.HasValue ? true : assessment.AssessmentIsAvailable.Value)
                };
                assessmentReturnList.Add(newSchoolAssessment);

                // if we've set a status for this school/assessemnt before
                var isAvailable = schoolAssessments.FirstOrDefault(p => p.AssessmentId == assessment.Id && p.SchoolId == input.Id);
                if (isAvailable != null)
                {
                    newSchoolAssessment.AssessmentIsAvailable = isAvailable.AssessmentIsAvailable;
                }
            }
            response.SchoolAssessments = assessmentReturnList;
            return response;
        }
        public OutputDto_StaffAssessments GetStaffAssessments()
        {
            var userId = _currentUser.Id;
            var response = new OutputDto_StaffAssessments();
            var staffAssessements = _dbContext.StaffAssessments.Where(p => p.StaffId == userId).ToList();

            // first get all assessments
            var assessments = _dbContext.Assessments.ToList();

            // next, get all schools where this teacher has access
            var staffAccess = _dbContext.StaffSchools.Where(p => p.StaffID == userId).Select(p => p.SchoolID).ToList();
            var schoolAssessments = _dbContext.SchoolAssessments.Where(p => staffAccess.Any(j => j == p.SchoolId)).ToList();
            var assessmentReturnList = new List<StaffAssessmentDto>();

            foreach (var assessment in assessments)
            {
                // first get all the assessments and disable the ones that are disabled at the district level
                var newStaffAssessment = new StaffAssessmentDto()
                {
                    IsDisabled = (!assessment.AssessmentIsAvailable.HasValue ? false : !assessment.AssessmentIsAvailable.Value),
                    AssessmentId = assessment.Id,
                    StaffId = userId,
                    AssessmentDescription = assessment.AssessmentDescription,
                    AssessmentName = assessment.AssessmentName,
                    TestType = assessment.TestType.Value,
                    CanImport = assessment.CanImport,
                    AssessmentIsAvailable = (!assessment.AssessmentIsAvailable.HasValue ? true : assessment.AssessmentIsAvailable.Value)
                };
                assessmentReturnList.Add(newStaffAssessment);


                // disable at ths school level from our adminschools, we only disable it if ALL the schools say false
                if (schoolAssessments.Count > 0)
                {
                    var schoolAssessmentsForAssessment = schoolAssessments.Where(p => p.AssessmentId == assessment.Id).ToList();

                    // have to go school by school
                    bool allFalse = true;
                    foreach (var accessSchool in staffAccess)
                    {
                        var isAvailable = schoolAssessmentsForAssessment.FirstOrDefault(p => p.SchoolId == accessSchool && p.AssessmentIsAvailable == false);
                        if (isAvailable == null)
                        {
                            allFalse = false;
                            break;
                        }
                    }

                    if (allFalse)
                    {
                        newStaffAssessment.IsDisabled = true;
                        newStaffAssessment.AssessmentIsAvailable = false;
                    }
                }

                // now we check the personally disabled list
                if (staffAssessements.Count > 0)
                {
                    var isHidden = staffAssessements.FirstOrDefault(p => p.AssessmentId == assessment.Id && p.AssessmentIsAvailable == false);
                    if (isHidden != null)
                    {
                        newStaffAssessment.AssessmentIsAvailable = false;
                    }
                }
            }
            response.StaffAssessments = assessmentReturnList;
            return response;
        }
        public OutputDto_SuccessAndStatus UpdateAssessmentAvailability(AssessmentDto input)
        {
            var assessment = _dbContext.Assessments.First(p => p.Id == input.Id);
            assessment.AssessmentIsAvailable = input.AssessmentIsAvailable;
            _dbContext.SaveChanges();

            return new OutputDto_SuccessAndStatus
            {
                isValid = true,
                value = ""
            };
        }
        public OutputDto_SuccessAndStatus UpdateSchoolAssessmentAvailability(SchoolAssessmentDto input)
        {
            var schoolAssessment = _dbContext.SchoolAssessments.FirstOrDefault(p => p.AssessmentId == input.AssessmentId && p.SchoolId == input.SchoolId);
            if (schoolAssessment != null)
            {
                schoolAssessment.AssessmentIsAvailable = input.AssessmentIsAvailable;
            }
            else
            {
                schoolAssessment = new SchoolAssessment() { SchoolId = input.SchoolId, AssessmentId = input.AssessmentId, AssessmentIsAvailable = input.AssessmentIsAvailable };
                _dbContext.SchoolAssessments.Add(schoolAssessment);
            }

            _dbContext.SaveChanges();


            return new OutputDto_SuccessAndStatus
            {
                isValid = true,
                value = ""
            };
        }

        public List<IndexedLookupList> GetLookupFieldsForAssessments(string assessmentIds)
        {
            List<int> aryAssessments = assessmentIds.Split(',').Select(int.Parse).ToList();

            var assessments = _dbContext.Assessments
                .Include(p => p.Fields)
                .Where(p => aryAssessments.Contains(p.Id));

            List<IndexedLookupList> lookupFields = new List<IndexedLookupList>();
            foreach (var assessment in assessments)
            {
                foreach (var field in assessment.Fields)
                {
                    if (field.FieldType == "DropdownFromDB" || field.FieldType == "checklist")
                    {
                        if (!lookupFields.Any(p => p.LookupColumnName == field.LookupFieldName))
                        {
                            var fieldValues = _dbContext.LookupFields.Where(p => p.FieldName == field.LookupFieldName).ToList();
                            lookupFields.Add(new IndexedLookupList() { LookupColumnName = field.LookupFieldName, LookupFields = fieldValues });
                        }
                    }
                }
            }
            return lookupFields;
        }

        public List<IndexedLookupList> GetLookupFieldsForAllAssessments()
        {
            var assessments = _dbContext.Assessments
                .Include(p => p.Fields);

            List<IndexedLookupList> lookupFields = new List<IndexedLookupList>();
            foreach (var assessment in assessments)
            {
                foreach (var field in assessment.Fields)
                {
                    if (field.FieldType == "DropdownFromDB" || field.FieldType == "checklist")
                    {
                        if (!lookupFields.Any(p => p.LookupColumnName == field.LookupFieldName))
                        {
                            var fieldValues = _dbContext.LookupFields.Where(p => p.FieldName == field.LookupFieldName).OrderBy(p => p.SortOrder).ToList();
                            lookupFields.Add(new IndexedLookupList() { LookupColumnName = field.LookupFieldName, LookupFields = fieldValues });
                        }
                    }
                }
            }
            return lookupFields;
        }

        public OutputDto_SuccessAndStatus UpdateStaffAssessmentAvailability(StaffAssessmentDto input)
        {
            var userId = _currentUser.Id;
            var staffAssessment = _dbContext.StaffAssessments.FirstOrDefault(p => p.AssessmentId == input.AssessmentId && p.StaffId == userId);
            if (staffAssessment != null)
            {
                staffAssessment.AssessmentIsAvailable = input.AssessmentIsAvailable;
            }
            else
            {
                staffAssessment = new StaffAssessment() { StaffId = userId, AssessmentId = input.AssessmentId, AssessmentIsAvailable = input.AssessmentIsAvailable };
                _dbContext.StaffAssessments.Add(staffAssessment);
            }

            _dbContext.SaveChanges();

            return new OutputDto_SuccessAndStatus
            {
                isValid = true,
                value = ""
            };
        }
    }
}
