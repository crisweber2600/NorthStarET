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
using System;
using NorthStar4.PCL.Entity;
using NorthStar4.CrossPlatform.DTO.Reports.ObservationSummary;
using EntityDto.DTO.Reports.FP;
using NorthStar.EF6.Infrastructure;
using EntityDto.DTO.Admin.InterventionGroup;

namespace NorthStar.EF6
{
    public class ObservationSummaryDataService :NSBaseDataService
    {
        public ObservationSummaryDataService(ClaimsIdentity user, string loginConnectionString) : base(user, loginConnectionString)
        {

        }

        public OutputDto_ObservationSummaryClass GetFilteredObservationSummary(InputDto_GetFilteredObservationSummaryOptions input)
        {
            // bad data check
            if (input.TestDueDateID == -1 || input.TestDueDateID == 0 || input.TestDueDateID == null)
            {
                return new OutputDto_ObservationSummaryClass();
            }

            // 1. Get list of assessments from Db
            List<Assessment> assessmentsToInclude = _dbContext.GetObservationSummaryVisibleAssessments(_currentUser.Id);

            // 2. Create table of concatenated fields from all the tables
            var scores = _dbContext.GetFilteredObservationSummaryData(assessmentsToInclude, input, _currentUser.Id, false);

            //var distinctTdds = scores.StudentResults.Select(p => new { TddId = p.TestDueDateId, TestPeriodLevelId = p.TestLevelPeriodId, GradeId = p.GradeId }).Distinct().ToList();

            // 3. Get Benchmarks for all of the fields
            // get an an array of benchmarks BY GRADE so that we can use this for other forms of the Obs Summary data
            // TODO: loop over all grades and get benchmarks
            //var benchmarks = new List<ObservationSummaryBenchmark>();


            //var benchmarkList = _dbContext.GetAllObservationSummaryBenchmarks(assessmentsToInclude);
            //var benchmarkArray = benchmarkList.Distinct().ToList();
            //var benchmarksByGrade = new List<ObservationSummaryBenchmarksByGrade>();

            //foreach (var benchmark in benchmarkArray)
            //{
            //    var gradeBenchmark = benchmarksByGrade.FirstOrDefault(p => p.GradeId == benchmark.GradeId);

            //    if (gradeBenchmark == null)
            //    {
            //        gradeBenchmark = new ObservationSummaryBenchmarksByGrade { GradeId = benchmark.GradeId };
            //        benchmarksByGrade.Add(gradeBenchmark);
            //    }
            //    gradeBenchmark.Benchmarks.Add(benchmark);
            //}
            var distinctGrades = _dbContext.Grades.Select(p => p.Id).ToList();

            var benchmarksByGrade = new List<ObservationSummaryBenchmarksByGrade>();

            foreach (var grade in distinctGrades)
            {
                var benchmarks = _dbContext.GetClassObservationSummaryBenchmarks(assessmentsToInclude, input.TestDueDateID.Value, grade);
                var benchmarksForCurrentGrade = new ObservationSummaryBenchmarksByGrade() { GradeId = grade, Benchmarks = benchmarks };
                benchmarksByGrade.Add(benchmarksForCurrentGrade);
            }

            // 4. Combine all data into and object I can send to Angular
            return new OutputDto_ObservationSummaryClass()
            {
                Scores = scores,
                BenchmarksByGrade = benchmarksByGrade
            };
        }

        public OutputDto_ObservationSummaryClass GetClassObservationSummary(int classId, int testduedateId)
        {
            // bad data check
            if (classId == -1 || classId == 0 || testduedateId == -1 || testduedateId == 0)
            {
                return new OutputDto_ObservationSummaryClass();
            }

            // turn list of string to list of Ints
            //string[] aryAssessmentIds = assessmentIds.Split(Char.Parse(","));
            //List<int> intAssessmentIds = new List<int>();
            var gradeId = _dbContext.Sections.FirstOrDefault(p => p.Id == classId).GradeID;

            //foreach (var assessmentId in aryAssessmentIds)
            //{
            //    intAssessmentIds.Add(Int32.Parse(assessmentId));
            //}

            // 1. Get list of assessments from Db
            List<Assessment> assessmentsToInclude = _dbContext.GetObservationSummaryVisibleAssessments(_currentUser.Id);

            // 2. Create table of concatenated fields from all the tables
            var scores = _dbContext.GetClassObservationSummaryData(assessmentsToInclude, classId, testduedateId, DateTime.Now, _currentUser.Id);

            // 3. Get Benchmarks for all of the fields
            // get an an array of benchmarks BY GRADE so that we can use this for other forms of the Obs Summary data
            //var benchmarks = _dbContext.GetAllObservationSummaryBenchmarks(assessmentsToInclude);
            var benchmarkArray = new List<ObservationSummaryBenchmark>();

            var distinctGrades = _dbContext.Grades.Select(p => p.Id).ToList();

            var benchmarksByGrade = new List<ObservationSummaryBenchmarksByGrade>();

            foreach (var grade in distinctGrades)
            {
                var benchmarks = _dbContext.GetClassObservationSummaryBenchmarks(assessmentsToInclude, testduedateId, grade);
                var benchmarksForCurrentGrade = new ObservationSummaryBenchmarksByGrade() { GradeId = grade, Benchmarks = benchmarks };
                benchmarksByGrade.Add(benchmarksForCurrentGrade);
            }

            var studentAttributes = new List<JObject>();
            var tdd = _dbContext.TestDueDates.First(p => p.Id == testduedateId);

            var students = _dbContext.StudentSections.Where(p => p.Section.Id == classId && p.Student.IsActive != false)
            .Include(x => x.Student.StudentAttributeDatas).Select(p => p.Student);

            foreach (var student in students)
            {
                var newStudentAttributes = new JObject();
                newStudentAttributes.Add("StudentId", student.Id);
                foreach (var studentAttributeData in student.StudentAttributeDatas.Where(p => p.AttributeID != 4)) // SPEDLabels are separate
                {
                    newStudentAttributes.Add(studentAttributeData.AttributeID.ToString(), JToken.FromObject(studentAttributeData.AttributeValueID));
                }
                studentAttributes.Add(newStudentAttributes);
            }

            var interventionRecords = GetActiveStudentInterventionsBySectionId(classId, tdd.DueDate.Value);
            var studentServices = GetStudentSPEDLabelsBySectionId(classId);

            // 4. Combine all data into and object I can send to Angular
            return new OutputDto_ObservationSummaryClass()
            {
                Scores = scores,
                BenchmarksByGrade = benchmarksByGrade,
                StudentAttributes = studentAttributes,
                InterventionRecords = interventionRecords,
                StudentServices = studentServices
            };
        }

        public OutputDto_ObservationSummaryClassMultiple GetClassObservationSummaryMultiple(InputDto_GetSectionMultipleObservationSummary input)
        {
            // bad data check
            if (input.SectionId == -1 || input.SectionId == 0 || input.TestDueDates == null)
            {
                return new OutputDto_ObservationSummaryClassMultiple();
            }

            // turn list of string to list of Ints
            //string[] aryAssessmentIds = assessmentIds.Split(Char.Parse(","));
            //List<int> intAssessmentIds = new List<int>();
            var gradeId = _dbContext.Sections.FirstOrDefault(p => p.Id == input.SectionId).GradeID;

            //foreach (var assessmentId in aryAssessmentIds)
            //{
            //    intAssessmentIds.Add(Int32.Parse(assessmentId));
            //}

            // 1. Get list of assessments from Db
            List<Assessment> assessmentsToInclude = _dbContext.GetObservationSummaryVisibleAssessments(_currentUser.Id);

            // 2. Create table of concatenated fields from all the tables
            var scores = _dbContext.GetClassObservationSummaryMultipleData(assessmentsToInclude, input.SectionId, input.TestDueDates, _currentUser.Id, input.IsMultiColumn);

            // 3. Get Benchmarks for all of the fields
            // get an an array of benchmarks BY GRADE so that we can use this for other forms of the Obs Summary data
            //var benchmarks = _dbContext.GetAllObservationSummaryBenchmarks(assessmentsToInclude);
            var benchmarkArray = new List<ObservationSummaryBenchmark>();

            var distinctGrades = _dbContext.Grades.Select(p => p.Id).ToList();

            var benchmarksByGrade = new List<ObservationSummaryBenchmarksByGrade>();

            foreach (var grade in distinctGrades)
            {
                var benchmarks = _dbContext.GetClassObservationSummaryBenchmarks(assessmentsToInclude, 0, grade, null);
                var benchmarksForCurrentGrade = new ObservationSummaryBenchmarksByGrade() { GradeId = grade, Benchmarks = benchmarks };
                benchmarksByGrade.Add(benchmarksForCurrentGrade);
            }

            //var studentAttributes = new List<JObject>();
            //var tdd = _dbContext.TestDueDates.First(p => p.Id == benchmarkd);

            //var students = _dbContext.StudentSections.Where(p => p.Section.Id == classId && p.Student.IsActive != false)
            //.Include(x => x.Student.StudentAttributeDatas).Select(p => p.Student);

            //foreach (var student in students)
            //{
            //    var newStudentAttributes = new JObject();
            //    newStudentAttributes.Add("StudentId", student.Id);
            //    foreach (var studentAttributeData in student.StudentAttributeDatas.Where(p => p.AttributeID != 4)) // SPEDLabels are separate
            //    {
            //        newStudentAttributes.Add(studentAttributeData.AttributeID.ToString(), JToken.FromObject(studentAttributeData.AttributeValueID));
            //    }
            //    studentAttributes.Add(newStudentAttributes);
            //}

            // QUESTION: WHY DO I NEED THESE?  SURELY THERE WAS A POINT??
            //var interventionRecords = GetActiveStudentInterventionsBySectionId(input.SectionId, tdd.DueDate.Value);
            //var studentServices = GetStudentSPEDLabelsBySectionId(classId);

            // 4. Combine all data into and object I can send to Angular
            return new OutputDto_ObservationSummaryClassMultiple()
            {
                Scores = scores,
                BenchmarksByGrade = benchmarksByGrade,
                //StudentAttributes = studentAttributes,
                //InterventionRecords = interventionRecords,
                //StudentServices = studentServices
            };
        }

        public List<StudentSPEDLabel> GetStudentSPEDLabelsBySectionId(int sectionId)
        {
            var records = _dbContext.Database.SqlQuery<StudentSPEDLabel>(string.Format("dbo.[nset_sp_GetStudentSPEDLabelsBySectionID] {0}", sectionId));

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


        public OutputDto_ObservationSummaryClass GetTeamMeetingObservationSummary(InputDto_ObservationSummaryTeamMeeting input)
        {
            List<Assessment> assessmentsToInclude = _dbContext.GetObservationSummaryVisibleAssessments(_currentUser.Id);
            //var assessmentIds = "1,3,8,9";
            // TODO: get TDD if not defined
            var tm = _dbContext.TeamMeetings.First(p => p.ID == input.TeamMeetingId);
            if (!tm.TestDueDateId.HasValue)
            { 
                var date = GetCurrentBenchmarkDate(tm.SchoolYear, tm.MeetingTime);
                tm.TestDueDateId = date.Id; // TODO: Get nearest
            }

            // bad data check
            if (input.TeamMeetingId == -1 || input.TeamMeetingId == 0)
            {
                return new OutputDto_ObservationSummaryClass();
            }

            // turn list of string to list of Ints
            //string[] aryAssessmentIds = assessmentIds.Split(Char.Parse(","));
            //List<int> intAssessmentIds = new List<int>();

            //foreach (var assessmentId in aryAssessmentIds)
            //{
            //    intAssessmentIds.Add(Int32.Parse(assessmentId));
            //}

            //// 1. Get list of assessments from Db
            //List<Assessment> assessmentsToInclude = _dbContext.Assessments
            //    .Include(p => p.FieldGroups)
            //        .Include(p => p.FieldCategories)
            //        .Include(p => p.FieldSubCategories)
            //        .Include(p => p.Fields)
            //        .Where(p => intAssessmentIds.Contains(p.Id)).ToList();

            // 2. Create table of concatenated fields from all the tables
            var scores = _dbContext.GetTeamMeetingObservationSummaryData(assessmentsToInclude, input.TeamMeetingId, tm.TestDueDateId.Value, input.StaffId, _currentUser.Id);

            // need to get the distinct list of grades for everyone at the team meeting
            var distinctGrades = _dbContext.Grades.Select(p => p.Id).ToList();

            // 3. Get Benchmarks for all of the fields
            // get an an array of benchmarks BY GRADE so that we can use this for other forms of the Obs Summary data
            var benchmarkArray = new List<ObservationSummaryBenchmark>();
            var benchmarksByGrade = new List<ObservationSummaryBenchmarksByGrade>();
            foreach (var grade in distinctGrades)
            {
                var benchmarks = _dbContext.GetClassObservationSummaryBenchmarks(assessmentsToInclude, tm.TestDueDateId.Value, grade);
                var benchmarksForCurrentGrade = new ObservationSummaryBenchmarksByGrade() { GradeId = grade, Benchmarks = benchmarks };
                benchmarksByGrade.Add(benchmarksForCurrentGrade);
            }

            // 4. Combine all data into and object I can send to Angular
            return new OutputDto_ObservationSummaryClass()
            {
                Scores = scores,
                BenchmarksByGrade = benchmarksByGrade
            };
        }

        public List<InterventionsByStudent> GetInterventionGroupsForTeamMeetingStudents(InputDto_ObservationSummaryTeamMeeting input)
        {
            // get the list of students
            var tmStudents = _dbContext.TeamMeetingStudents.Where(p => p.TeamMeetingID == input.TeamMeetingId); // TODO: only get ones for staff
            var studentInterventionList = new List<InterventionsByStudent>();

            foreach (var tmStudent in tmStudents)
            {
                // get this student's interventions
                var currentStudentsInterventions = _dbContext.StudentInterventionGroups
                    .Include(p => p.InterventionGroup)
                    .Include(p => p.InterventionGroup.InterventionType)
                    .Where(p =>
                    p.StudentID == tmStudent.StudentID)
                    .OrderByDescending(p => p.StartDate);

                // does this student have any interventions?
                if (currentStudentsInterventions.Any())
                {
                    var interventionsBySchoolYear = new List<InterventionsBySchoolYear>();
                    // get the unique schoolyears
                    foreach (var ig in currentStudentsInterventions)
                    {
                        // add to existing year or create new one
                        if (interventionsBySchoolYear.Any(p => p.SchoolYear == ig.InterventionGroup.SchoolStartYear))
                        {
                            interventionsBySchoolYear.First(p =>
                            p.SchoolYear == ig.InterventionGroup.SchoolStartYear)
                                .InterventionList.Add(Mapper.Map<InterventionGroupStudentDto>(ig));
                        }
                        else
                        {
                            var newBySchoolyear = new InterventionsBySchoolYear() { SchoolYear = ig.InterventionGroup.SchoolStartYear };
                            newBySchoolyear.InterventionList.Add(Mapper.Map<InterventionGroupStudentDto>(ig));
                            interventionsBySchoolYear.Add(newBySchoolyear);
                        }
                    }

                    var studentIntervention = new InterventionsByStudent()
                    {
                        StudentId = tmStudent.StudentID,
                        InterventionsBySchoolYear = interventionsBySchoolYear.OrderByDescending(p => p.SchoolYear).ToList()
                    };
                    studentInterventionList.Add(studentIntervention);
                }

            }
            return studentInterventionList;
        }
    }
}
