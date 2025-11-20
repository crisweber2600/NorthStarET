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
using System.Collections.Generic;
using System.Linq;
using EntityDto.DTO.Assessment;
using EntityDto.DTO.Assessment.Benchmarks;
using System.Data.SqlClient;
using System;
using NorthStar4.PCL.Entity;

namespace NorthStar.EF6
{
    public class BenchmarkDataService : NSBaseDataService
    {
        public BenchmarkDataService(ClaimsIdentity user, string loginConnectionString) : base(user, loginConnectionString)
        {

        }
        public OutputDto_Benchmarks GetSystemBenchmarks(InputDto_GetBenchmarks input)
        {
            var response = new OutputDto_Benchmarks();


            // call stored procedure and pass parameters
            var results = _dbContext.Database.SqlQuery<AssessmentBenchmarkDto>("EXEC [ns4_GetDefaultBenchmarks] @assessmentId, @fieldName, @lookupFieldName",
                new SqlParameter("assessmentId", input.AssessmentId),
                new SqlParameter("fieldName", input.FieldName),
                new SqlParameter("lookupFieldName", string.IsNullOrEmpty(input.LookupFieldName) ? (object)DBNull.Value : input.LookupFieldName));


            response.Benchmarks = results.ToList();
            response.Benchmarks.Each(p => {
                p.DoesNotMeetResult = new OutputDto_BenchmarkResult { Score = p.DoesNotMeet, ScoreLabel = p.DoesNotMeetLabel };
                p.ApproachesResult = new OutputDto_BenchmarkResult { Score = p.Approaches, ScoreLabel = p.ApproachesLabel };
                p.MeetsResult = new OutputDto_BenchmarkResult { Score = p.Meets, ScoreLabel = p.MeetsLabel };
                p.ExceedsResult = new OutputDto_BenchmarkResult { Score = p.Exceeds, ScoreLabel = p.ExceedsLabel };
            });
            return response;
        }

        public OutputDto_Base SaveSystemBenchmark(AssessmentBenchmarkDto input)
        {
            var response = new OutputDto_Base();

            // see if exists yet
            var existing = _dbContext.AssessmentBenchmarks.FirstOrDefault(p => p.AssessmentID == input.AssessmentID && p.AssessmentField == input.AssessmentField && p.GradeID == input.GradeID && p.TestLevelPeriodID == input.TestLevelPeriodID);

            try
            {


                if (existing != null)
                {
                    existing.DoesNotMeet = input.DoesNotMeetResult.Score;
                    existing.Approaches = input.ApproachesResult.Score;
                    existing.Meets = input.MeetsResult.Score;
                    existing.Exceeds = input.ExceedsResult.Score;
                }
                else
                {
                    existing = _dbContext.AssessmentBenchmarks.Create();
                    existing.AssessmentID = input.AssessmentID;
                    existing.AssessmentField = input.AssessmentField;
                    existing.GradeID = input.GradeID;
                    existing.TestLevelPeriodID = input.TestLevelPeriodID;
                    existing.DoesNotMeet = input.DoesNotMeetResult.Score;
                    existing.Approaches = input.ApproachesResult.Score;
                    existing.Meets = input.MeetsResult.Score;
                    existing.Exceeds = input.ExceedsResult.Score;
                    _dbContext.AssessmentBenchmarks.Add(existing);
                }
                _dbContext.SaveChanges();
            }
            catch(Exception ex)
            {
                response.Status.StatusCode = StatusCode.UserDisplayableException;
                response.Status.StatusMessage = "There was an error saving this record.  Please try again later.";

                // TODO: LOG IT!!
            }

            return response;
        }

        public OutputDto_Base DeleteSystemBenchmark(AssessmentBenchmarkDto input)
        {
            var response = new OutputDto_Base();

            // see if exists yet
            var existing = _dbContext.AssessmentBenchmarks.FirstOrDefault(p => p.AssessmentID == input.AssessmentID && p.AssessmentField == input.AssessmentField && p.GradeID == input.GradeID && p.TestLevelPeriodID == input.TestLevelPeriodID);

            try
            {


                if (existing != null)
                {
                    _dbContext.AssessmentBenchmarks.Remove(existing);
                    _dbContext.SaveChanges();

                }
            }
            catch (Exception ex)
            {
                response.Status.StatusCode = StatusCode.UserDisplayableException;
                response.Status.StatusMessage = "There was an error deleting this record.  Please try again later.";

                // TODO: LOG IT!!
            }

            return response;
        }

        public OutputDto_GetAssessmentsAndFieldsForUser GetAssessmentsAndFields()
        {
            var allAssessmentsICanAccess = _dbContext.Assessments //.Where(p => p.Enabled)
                .Where(p => (p.AssessmentIsAvailable.HasValue && p.AssessmentIsAvailable.Value) || (p.AssessmentIsAvailable == null))
                .Include(p => p.Fields)
                .Where(p => p.TestType == 1 || p.TestType == 3).ToList();

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

            // stupid hack b/c EF6 doesn't allow filtering on Included collections
            foreach (var assessment in allAssessmentsICanAccess)
            {
                assessment.Fields = assessment.Fields.Where(p => ((p.DisplayInObsSummary.HasValue == true && p.DisplayInObsSummary.Value == true) ||
                (p.DisplayInEditResultList.HasValue == true && p.DisplayInEditResultList.Value == true) ||
                (p.DisplayInLineGraphs.HasValue == true && p.DisplayInLineGraphs.Value == true)) && (p.FieldType == "DropdownFromDB" || p.FieldType == "checklist" || p.FieldType == "DropdownRange" || p.FieldType == "DecimalRange" || p.FieldType == "CalculatedFieldDbBacked")).ToList();

                var benchmarkedFields = new List<AssessmentField>();
                foreach (var field in assessment.Fields)
                {
                    if(_dbContext.DistrictBenchmarks.Any(p => p.AssessmentID == assessment.Id && p.AssessmentField.Equals(field.DatabaseColumn, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        benchmarkedFields.Add(field);
                    }
                    else if (_dbContext.DistrictYearlyAssessmentBenchmarks.Any(p => p.AssessmentID == assessment.Id && p.AssessmentField.Equals(field.DatabaseColumn, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        benchmarkedFields.Add(field);
                    }
                }
                assessment.Fields = benchmarkedFields;

            }


            return new OutputDto_GetAssessmentsAndFieldsForUser
            {
                Assessments = Mapper.Map<List<AssessmentDto>>(allAssessmentsICanAccess)
            };
        }

        public OutputDto_GetAssessmentsAndFieldsForUser GetInterventionAssessmentsAndFields()
        {
            var assessments = _dbContext.Assessments //.Where(p => p.Enabled)
                .Include(p => p.Fields)
                .Where(p => p.TestType == 2);
            //

            // stupid hack b/c EF6 doesn't allow filtering on Included collections
            foreach (var assessment in assessments)
            {
                assessment.Fields = assessment.Fields.Where(p => ((p.DisplayInObsSummary.HasValue == true && p.DisplayInObsSummary.Value == true) ||
                (p.DisplayInEditResultList.HasValue == true && p.DisplayInEditResultList.Value == true) ||
                (p.DisplayInLineGraphs.HasValue == true && p.DisplayInLineGraphs.Value == true)) && (p.FieldType == "DropdownFromDB" || p.FieldType == "checklist" || p.FieldType == "DropdownRange" || p.FieldType == "DecimalRange" || p.FieldType == "CalculatedFieldDbBacked")).ToList();
            }


            return new OutputDto_GetAssessmentsAndFieldsForUser
            {
                Assessments = Mapper.Map<List<AssessmentDto>>(assessments)
            };
        }

        #region District Benchmarks
        public OutputDto_DistrictYearlyAssessmentBenchmarks GetDistrictYearlyAssessmentBenchmarks(InputDto_GetBenchmarks input)
        {
            var response = new OutputDto_DistrictYearlyAssessmentBenchmarks();


            // call stored procedure and pass parameters
            var results = _dbContext.Database.SqlQuery<DistrictYearlyAssessmentBenchmarkDto>("EXEC [ns4_GetDistrictYearlyAssessmentBenchmarks] @assessmentId, @fieldName, @lookupFieldName",
                new SqlParameter("assessmentId", input.AssessmentId),
                new SqlParameter("fieldName", input.FieldName),
                new SqlParameter("lookupFieldName", string.IsNullOrEmpty(input.LookupFieldName) ? (object)DBNull.Value : input.LookupFieldName));


            response.Benchmarks = results.ToList();
            response.Benchmarks.Each(p => {
                p.DoesNotMeetResult = new OutputDto_BenchmarkResult { Score = p.DoesNotMeet, ScoreLabel = p.DoesNotMeetLabel };
                p.ApproachesResult = new OutputDto_BenchmarkResult { Score = p.Approaches, ScoreLabel = p.ApproachesLabel };
                p.MeetsResult = new OutputDto_BenchmarkResult { Score = p.Meets, ScoreLabel = p.MeetsLabel };
                p.ExceedsResult = new OutputDto_BenchmarkResult { Score = p.Exceeds, ScoreLabel = p.ExceedsLabel };
            });
            return response;
        }

        public OutputDto_Base SaveDistrictYearlyAssessmentBenchmark(DistrictYearlyAssessmentBenchmarkDto input)
        {
            var response = new OutputDto_Base();

            // see if exists yet
            var existing = _dbContext.DistrictYearlyAssessmentBenchmarks.FirstOrDefault(p => p.AssessmentID == input.AssessmentID && p.AssessmentField == input.AssessmentField && p.GradeID == input.GradeID);

            try
            {
                if (existing != null)
                {
                    existing.DoesNotMeet = input.DoesNotMeetResult.Score;
                    existing.Approaches = input.ApproachesResult.Score;
                    existing.Meets = input.MeetsResult.Score;
                    existing.Exceeds = input.ExceedsResult.Score;
                }
                else
                {
                    existing = _dbContext.DistrictYearlyAssessmentBenchmarks.Create();
                    existing.AssessmentID = input.AssessmentID;
                    existing.AssessmentField = input.AssessmentField;
                    existing.GradeID = input.GradeID;                    
                    existing.DoesNotMeet = input.DoesNotMeetResult.Score;
                    existing.Approaches = input.ApproachesResult.Score;
                    existing.Meets = input.MeetsResult.Score;
                    existing.Exceeds = input.ExceedsResult.Score;
                    _dbContext.DistrictYearlyAssessmentBenchmarks.Add(existing);
                }
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                response.Status.StatusCode = StatusCode.UserDisplayableException;
                response.Status.StatusMessage = "There was an error saving this record.  Please try again later.";

                // TODO: LOG IT!!
            }

            return response;
        }

        public OutputDto_Base DeleteDistrictYearlyAssessmentBenchmark(DistrictYearlyAssessmentBenchmarkDto input)
        {
            var response = new OutputDto_Base();

            // see if exists yet
            var existing = _dbContext.DistrictYearlyAssessmentBenchmarks.FirstOrDefault(p => p.AssessmentID == input.AssessmentID && p.AssessmentField == input.AssessmentField && p.GradeID == input.GradeID);

            try
            {                
                if (existing != null)
                {
                    _dbContext.DistrictYearlyAssessmentBenchmarks.Remove(existing);
                    _dbContext.SaveChanges();

                }
            }
            catch (Exception ex)
            {
                response.Status.StatusCode = StatusCode.UserDisplayableException;
                response.Status.StatusMessage = "There was an error deleting this record.  Please try again later.";

                // TODO: LOG IT!!
            }

            return response;
        }



        public OutputDto_DistrictBenchmarks GetDistrictBenchmarks(InputDto_GetBenchmarks input)
        {
            var response = new OutputDto_DistrictBenchmarks();


            // call stored procedure and pass parameters
            var results = _dbContext.Database.SqlQuery<DistrictBenchmarkDto>("EXEC [ns4_GetDistrictBenchmarks] @assessmentId, @fieldName, @lookupFieldName",
                new SqlParameter("assessmentId", input.AssessmentId),
                new SqlParameter("fieldName", input.FieldName),
                new SqlParameter("lookupFieldName", string.IsNullOrEmpty(input.LookupFieldName) ? (object)DBNull.Value : input.LookupFieldName));


            response.Benchmarks = results.ToList();
            response.Benchmarks.Each(p => {
                p.DoesNotMeetResult = new OutputDto_BenchmarkResult { Score = p.DoesNotMeet, ScoreLabel = p.DoesNotMeetLabel };
                p.ApproachesResult = new OutputDto_BenchmarkResult { Score = p.Approaches, ScoreLabel = p.ApproachesLabel };
                p.MeetsResult = new OutputDto_BenchmarkResult { Score = p.Meets, ScoreLabel = p.MeetsLabel };
                p.ExceedsResult = new OutputDto_BenchmarkResult { Score = p.Exceeds, ScoreLabel = p.ExceedsLabel };
            });
            return response;
        }

        public OutputDto_Base SaveDistrictBenchmark(DistrictBenchmarkDto input)
        {
            var response = new OutputDto_Base();

            // see if exists yet
            var existing = _dbContext.DistrictBenchmarks.FirstOrDefault(p => p.AssessmentID == input.AssessmentID && p.AssessmentField == input.AssessmentField && p.GradeID == input.GradeID && p.TestLevelPeriodID == input.TestLevelPeriodID);

            try
            {


                if (existing != null)
                {
                    existing.DoesNotMeet = input.DoesNotMeetResult.Score;
                    existing.Approaches = input.ApproachesResult.Score;
                    existing.Meets = input.MeetsResult.Score;
                    existing.Exceeds = input.ExceedsResult.Score;
                }
                else
                {
                    existing = _dbContext.DistrictBenchmarks.Create();
                    existing.AssessmentID = input.AssessmentID;
                    existing.AssessmentField = input.AssessmentField;
                    existing.GradeID = input.GradeID;
                    existing.TestLevelPeriodID = input.TestLevelPeriodID;
                    existing.DoesNotMeet = input.DoesNotMeetResult.Score;
                    existing.Approaches = input.ApproachesResult.Score;
                    existing.Meets = input.MeetsResult.Score;
                    existing.Exceeds = input.ExceedsResult.Score;
                    _dbContext.DistrictBenchmarks.Add(existing);
                }
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                response.Status.StatusCode = StatusCode.UserDisplayableException;
                response.Status.StatusMessage = "There was an error saving this record.  Please try again later.";

                // TODO: LOG IT!!
            }

            return response;
        }

        public OutputDto_Base DeleteDistrictBenchmark(DistrictBenchmarkDto input)
        {
            var response = new OutputDto_Base();

            // see if exists yet
            var existing = _dbContext.DistrictBenchmarks.FirstOrDefault(p => p.AssessmentID == input.AssessmentID && p.AssessmentField == input.AssessmentField && p.GradeID == input.GradeID && p.TestLevelPeriodID == input.TestLevelPeriodID);

            try
            {


                if (existing != null)
                {
                    _dbContext.DistrictBenchmarks.Remove(existing);
                    _dbContext.SaveChanges();

                }
            }
            catch (Exception ex)
            {
                response.Status.StatusCode = StatusCode.UserDisplayableException;
                response.Status.StatusMessage = "There was an error deleting this record.  Please try again later.";

                // TODO: LOG IT!!
            }

            return response;
        }

        public OutputDto_GetAssessmentsAndFieldsForUser GetDistrictAssessmentsAndFields()
        {
            var assessments = _dbContext.Assessments.Where(p => p.TestType == 1 || p.TestType == 2) //.Where(p => p.Enabled)
                .Include(p => p.Fields);
            //

            // stupid hack b/c EF6 doesn't allow filtering on Included collections
            foreach (var assessment in assessments)
            {
                assessment.Fields = assessment.Fields.Where(p => ((p.DisplayInObsSummary.HasValue == true && p.DisplayInObsSummary.Value == true) ||
                (p.DisplayInEditResultList.HasValue == true && p.DisplayInEditResultList.Value == true) ||
                (p.DisplayInLineGraphs.HasValue == true && p.DisplayInLineGraphs.Value == true)) && (p.FieldType == "DropdownFromDB" || p.FieldType == "checklist" || p.FieldType == "DropdownRange" || p.FieldType == "DecimalRange" || p.FieldType == "CalculatedFieldDbBacked")).OrderBy(p => p.DisplayLabel).ToList();
            }


            return new OutputDto_GetAssessmentsAndFieldsForUser
            {
                Assessments = Mapper.Map<List<AssessmentDto>>(assessments)
            };
        }

        public OutputDto_GetAssessmentsAndFieldsForUser GetDistrictYearlyAssessmentsAndFields()
        {
            var assessments = _dbContext.Assessments.Where(p => p.TestType == 3) //.Where(p => p.Enabled)
                .Include(p => p.Fields);
            //

            // stupid hack b/c EF6 doesn't allow filtering on Included collections
            foreach (var assessment in assessments)
            {
                assessment.Fields = assessment.Fields.Where(p => ((p.DisplayInObsSummary.HasValue == true && p.DisplayInObsSummary.Value == true) ||
                (p.DisplayInEditResultList.HasValue == true && p.DisplayInEditResultList.Value == true) ||
                (p.DisplayInLineGraphs.HasValue == true && p.DisplayInLineGraphs.Value == true)) && (p.FieldType == "DropdownFromDB" || p.FieldType == "checklist" || p.FieldType == "DropdownRange" || p.FieldType == "DecimalRange" || p.FieldType == "CalculatedFieldDbBacked")).OrderBy(p => p.DisplayLabel).ToList();
            }


            return new OutputDto_GetAssessmentsAndFieldsForUser
            {
                Assessments = Mapper.Map<List<AssessmentDto>>(assessments)
            };
        }
        #endregion
    }
}
