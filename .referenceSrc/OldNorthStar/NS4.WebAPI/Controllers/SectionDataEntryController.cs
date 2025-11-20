using System.Security.Claims;
using System.Web.Http;
using NorthStar.EF6;
using NorthStar4.PCL.DTO;
using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EntityDto.DTO.Assessment;
//using AutoMapper;
using EntityDto.DTO.Admin.Section;
using EntityDto.DTO.Admin.Simple;
using NorthStar4.API.Infrastructure;

namespace NorthStar4.api
{
    [RoutePrefix("api/SectionDataEntry")]
    [Authorize]
    public class SectionDataEntryController : NSBaseController
    {
        ////private readonly DistrictContext _dbContext;

        ////private NorthStarDataService dataService = null;


        //[HttpGet("GetAssessmentResults/{assessmentId:int}/{classId:int}/{benchmarkDateId:int}")]
        //public OutputDto_StudentAssessmentResults GetAssessmentResults(int assessmentId, int classId, int benchmarkDateId)
        //{

        //    var assessment = _dbContext.Assessments.FirstOrDefault(m => m.Id == assessmentId);

        //    // get only the fields that we want
        //    assessment.Fields = assessment.Fields.Where(p =>p.DisplayInEditResultList != null && p.DisplayInEditResultList == true).OrderBy(p => p.FieldOrder).ToList();
        //    //assessment.Fields = assessment.Fields.OrderBy(p => p.FieldOrder).ToList();

        //    // now have the assessment and controls loaded, need to get the test results from the table
        //    // specified in the assessment and turn each result into a StudentResult DTO
        //    var studentResults = _dbContext.GetAssessmentStudentResults(assessment, classId, benchmarkDateId, DateTime.MaxValue);

        //    // return JObject with the proper format

        //    // TODO:  don't use EF Assessment... reduce to DTOs... get rid of field categoreies, subcats, groups, etc
        //    return new OutputDto_StudentAssessmentResults()
        //    {
        //        StudentResults = studentResults,
        //        Assessment = Mapper.Map<AssessmentDto>(assessment)
        //    };
        //}

        //[HttpGet("GetSingleAssessmentResult/{assessmentId:int}/{classId:int}/{benchmarkDateId:int}/{studentResultId:int}")]
        //public OutputDto_StudentEditAssessmentResult GetSingleAssessmentResult(int assessmentId, int classId, int benchmarkDateId, int studentResultId)
        //{
        //    var assessment = _dbContext.Assessments.Include("FieldGroups")
        //        .Include("FieldCategories")
        //        .Include("FieldSubCategories")
        //        .Include("Fields")
        //        .FirstOrDefault(m => m.Id == assessmentId);

        //    // now have the assessment and controls loaded, need to get the test results from the table
        //    // specified in the assessment and turn each result into a StudentResult DTO
        //    var studentResults = _dbContext.GetAssessmentStudentResults(assessment, classId, benchmarkDateId, DateTime.MaxValue);
        //    var studentResult = studentResults.FirstOrDefault(p => p.ResultId == studentResultId);
        //    // change this to just a single student query
        //    // return JObject with the proper format

        //    var assessmentDto = Mapper.Map<AssessmentDto>(assessment);

        //    // TODO: temporarily convert to new structure
        //    foreach (var category in assessmentDto.FieldCategories)
        //    {
        //        // find all the fields in this category and add them to the category's list of fields
        //        category.Fields = assessmentDto.Fields.Where(p => p.CategoryId == category.Id).OrderBy(p => p.FieldOrder).ToList();

        //        // now match up each student result
        //        foreach (var field in category.Fields)
        //        {
        //            field.StudentFieldResult = studentResult.FieldResults.FirstOrDefault(p => p.DbColumn == field.DatabaseColumn);
        //        }
        //    }
        //    // trim some fat
        //    //assessmentDto.Fields = null;
        //    //assessmentDto.FieldGroups = null;
        //    //studentResult.FieldResults = null;

        //    return new OutputDto_StudentEditAssessmentResult()
        //    {
        //        StudentResult = studentResult,
        //        Assessment = assessmentDto //Mapper.Map<AssessmentDto>(assessment)
        //    };
        //}
        [Route("GetStudentAssessmentResult")]
        [HttpPost]
        public IHttpActionResult GetStudentAssessmentResult([FromBody]InputDto_GetStudentAssessmentResult input)
        {
            var dataService = new SectionDataEntryService(((ClaimsIdentity)User.Identity), LoginConnectionString);

            var result = dataService.GetStudentAssessmentResult(input);

            return ProcessResultStatus(result);
        }

        [Route("GetStudentProgressMonResult")]
        [HttpPost]
        public IHttpActionResult GetStudentProgressMonResult([FromBody]InputDto_GetStudentProgressMonResult input)
        {
            var dataService = new SectionDataEntryService(((ClaimsIdentity)User.Identity), LoginConnectionString);

            var result = dataService.GetStudentProgressMonResult(input);

            return ProcessResultStatus(result);
        }

        //[HttpGet("GetHFWSingleAssessmentResult/{assessmentId:int}/{classId:int}/{benchmarkDateId:int}/{studentId:int}/{lowWordOrder:int}/{highWordOrder:int}")]
        //public OutputDto_StudentEditHFWAssessmentResult GetHFWSingleAssessmentResult(int assessmentId, int classId, int benchmarkDateId, int studentId, int lowWordOrder, int highWordOrder)
        //{
        //    var assessment = _dbContext.Assessments
        //        .Include("FieldGroups")
        //        .Include("FieldCategories")
        //        .Include("FieldSubCategories")
        //        .Include("Fields")
        //        .FirstOrDefault(m => m.Id == assessmentId);

        //    // remove unneeded fields
        //    assessment.Fields = assessment.Fields.Where(j => string.IsNullOrEmpty(j.StorageTable) || (j.Group.SortOrder <= highWordOrder && j.Group.SortOrder >= lowWordOrder)).ToList();
        //    assessment.FieldGroups = assessment.FieldGroups.Where(j => j.SortOrder <= highWordOrder && j.SortOrder >= lowWordOrder).ToList();

        //    // now have the assessment and controls loaded, need to get the test results from the table
        //    // specified in the assessment and turn each result into a StudentResult DTO
        //    var studentResult = _dbContext.GetHFWAssessmentStudentResults(assessment, classId, benchmarkDateId, DateTime.MaxValue, studentId);
        //    // var studentResult = studentResults.FirstOrDefault(p => p.ResultId == studentResultId);
        //    // change this to just a single student query
        //    // return JObject with the proper format


        //    return new OutputDto_StudentEditHFWAssessmentResult()
        //    {
        //        StudentResult = studentResult,
        //        Assessment = Mapper.Map<AssessmentDto>(assessment)
        //    };
        //}

        //[HttpPost("SaveHFWAssessmentResult")]
        //public IHttpActionResult SaveHFWAssessmentResult([FromBody]InputDto_SaveHFWAssessmentResult studentResult)
        //{
        //    if (ModelState.IsValid)
        //    {

        //        _dbContext.SaveHFWStudentData(studentResult.StudentResult, studentResult.AssessmentId);

        //        // loop over fields and save data
        //        return Ok();
        //    }
        //    else
        //    {
        //        return BadRequest(ModelState);
        //    }
        //}
    }
}
