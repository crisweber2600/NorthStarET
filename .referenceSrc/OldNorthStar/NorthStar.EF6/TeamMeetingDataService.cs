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
using SendGrid;
using System.Net.Mail;
using Serilog;
using SendGrid.Helpers.Mail;

namespace NorthStar.EF6
{
    public class TeamMeetingDataService : NSBaseDataService
    {
        public TeamMeetingDataService(ClaimsIdentity user, string loginConnectionString) : base(user, loginConnectionString)
        {
        }

        public List<OutputDto_DropdownData> GetStaffForAttendeeGroup(int Id)
        {
            // TODO: filter this by the staff you have access to
            var staff = _dbContext.AttendeeGroupStaffs.Where(p => p.AttendeeGroupId == Id).Select(p => p.Staff).ToList();

            return Mapper.Map<List<OutputDto_DropdownData>>(staff);
        }

        public string GenerateTMInvitationHtml(InputDto_SendTMInvitation input, List<IndexedLookupList> lookupLists)
        {
            var tmHtml = String.Empty;

            var teamMeeting = _dbContext.TeamMeetings.First(p => p.ID == input.TeamMeetingId);

            var tdd = teamMeeting.TestDueDateId.HasValue ? teamMeeting.TestDueDateId.Value : 399; // TODO: Getnearest TDDID for teammeeting date

            // get observation summary for staff
            var observationSummaryManager = GetTeamMeetingObservationSummary(new InputDto_ObservationSummaryTeamMeeting { StaffId = input.StaffId, TeamMeetingId = input.TeamMeetingId, TestDueDateId = tdd });
            observationSummaryManager.LookupLists = lookupLists;

            // set display values for lookup fields
            for (var j = 0; j < observationSummaryManager.Scores.StudentResults.Count; j++)
            {
                for (var k = 0; k < observationSummaryManager.Scores.StudentResults[j].OSFieldResults.Count; k++)
                {
                    for (var i = 0; i < observationSummaryManager.Scores.Fields.Count; i++)
                    {
                        if (observationSummaryManager.Scores.Fields[i].DatabaseColumn == observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].DbColumn)
                        {
                            //scope.observationSummaryManager.Scores[j].FieldResults[k].Field = $scope.fields[i];

                            // set display value
                            if (observationSummaryManager.Scores.Fields[i].FieldType == "DropdownFromDB")
                            {
                                for (var p = 0; p < observationSummaryManager.LookupLists.Count; p++)
                                {
                                    if (observationSummaryManager.LookupLists[p].LookupColumnName == observationSummaryManager.Scores.Fields[i].LookupFieldName)
                                    {
                                        // now find the specifc value that matches
                                        for (var y = 0; y < observationSummaryManager.LookupLists[p].LookupFields.Count; y++)
                                        {
                                            if (observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].IntValue == observationSummaryManager.LookupLists[p].LookupFields[y].FieldSpecificId)
                                            {
                                                observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].StringValue = observationSummaryManager.LookupLists[p].LookupFields[y].FieldValue;
                                            }
                                        }
                                    }
                                }
                            }

                            if (observationSummaryManager.Scores.Fields[i].FieldType == "checklist")
                            {
                                for (var p = 0; p < observationSummaryManager.LookupLists.Count; p++)
                                {
                                    if (observationSummaryManager.LookupLists[p].LookupColumnName == observationSummaryManager.Scores.Fields[i].LookupFieldName)
                                    {
                                        // now find the specifc value that matches
                                        for (var y = 0; y < observationSummaryManager.LookupLists[p].LookupFields.Count; y++)
                                        {
                                            var sourceIdAry = observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].StringValue.Split(Char.Parse(","));
                                            var finalList = new List<string>();
                                            
                                            foreach(var intString in sourceIdAry)
                                            {
                                                if (Int32.Parse(intString) == observationSummaryManager.LookupLists[p].LookupFields[y].FieldSpecificId)
                                                {
                                                    finalList.Add(observationSummaryManager.LookupLists[p].LookupFields[y].FieldValue);
                                                }
                                            }
                                            observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].StringValue = String.Join(",", finalList);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var staff = _dbContext.Staffs.FirstOrDefault(p => p.Id == input.StaffId);
            var staffName = staff != null ? staff.FullName : "User";

            TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            DateTime cstTime = TimeZoneInfo.ConvertTimeFromUtc(teamMeeting.MeetingTime, cstZone);
            Console.WriteLine("The date and time are {0} {1}.",
                              cstTime,
                              cstZone.IsDaylightSavingTime(cstTime) ?
                                      cstZone.DaylightName : cstZone.StandardName);


            // build the email
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(@"<img src=""https://app.northstaret.net/assets/img/NorthStar_Logo.png"">");
            sb.AppendFormat(@"<div style=""font-size: 16px; margin-top: 10px; margin-bottom: 5px; font-weight: bold;color:black;"">Team Meeting Invitation</div>");
            sb.AppendFormat(@"<div style=""font-size: 13px; margin-top: 10px; margin-bottom: 10px;color:black;"">Dear {0},</div>", staffName);
            sb.AppendFormat(@"<div style=""font-size: 12px;margin-bottom: 10px;color:black;"">You have been invited to the Team Meeting <b>{0}</b> at this date and time: <b><i style='color:red'>{1} {3}</i></b><br><br><b>Comments: </b>{2}<br></div>", teamMeeting.Title, cstTime.ToString("dddd, MMMM dd, yyyy h:mm tt"), teamMeeting.Comments, cstZone.IsDaylightSavingTime(cstTime) ? cstZone.DaylightName : cstZone.StandardName);
            sb.AppendFormat(@"<table style=""border-collapse: collapse;border-spacing: 0;"">");
            sb.AppendFormat(@"    <thead>");
            sb.AppendFormat(@"        <tr>");
            sb.AppendFormat(@"            <th rowspan=""2"" style=""color: black; font-weight: bold; border-top: 1px solid #e9ecf0 !important; border-bottom: 2px solid #e9ecf0; border-left: 1px solid #e9ecf0; border-right: 1px solid #e9ecf0;padding:5px 5px;"">Student Name</div></th>");
            //sb.AppendFormat(@"            <th rowspan=""2"" style=""color: black; font-weight: bold; border-top: 1px solid #e9ecf0 !important; border-bottom: 2px solid #e9ecf0; border-left: 1px solid #e9ecf0; border-right: 1px solid #e9ecf0;padding:5px 5px;"">First Name</div></th>");
            foreach (var headerGroup in observationSummaryManager.Scores.HeaderGroups)
            {
                sb.AppendFormat(@"<th style=""color: black; font-weight: bold; border-top: 1px solid #e9ecf0 !important; border-bottom: 2px solid #e9ecf0; border-left: 1px solid #e9ecf0; border-right: 1px solid #e9ecf0;padding:5px 5px;"" colspan=""{0}"">{1}</th>", headerGroup.FieldCount, headerGroup.AssessmentName);
            }
            sb.AppendFormat(@"        </tr>");
            sb.AppendFormat(@"        <tr>");
            foreach (var field in observationSummaryManager.Scores.Fields)
            {
                sb.AppendFormat(@"<th style=""color: black; font-weight: bold; border-top: 1px solid #e9ecf0 !important; border-bottom: 2px solid #e9ecf0; border-left: 1px solid #e9ecf0; border-right: 1px solid #e9ecf0;padding:5px 5px;"">{0}</th>", field.FieldName);
            }
            sb.AppendFormat(@"        </tr>");
            sb.AppendFormat(@"    </thead>");
            sb.AppendFormat(@"    <tbody>");
            if (observationSummaryManager.Scores.StudentResults.Count == 0)
            {
                sb.AppendFormat(@"<tr><td style=""padding:3px 5px;border: 1px solid #e9ecf0;color:black;"" colspan=""100"">No students</td></tr>");
            }
            for (var i = 0; i < observationSummaryManager.Scores.StudentResults.Count; i++)
            {
                var studentResult = observationSummaryManager.Scores.StudentResults[i];

                sb.AppendFormat(@"        <tr>");
                sb.AppendFormat(@"            <td align=""left"" style=""padding:3px 5px;border: 1px solid #e9ecf0;color:black;background-color:{1}"">{0}</td>", studentResult.StudentName, (i % 2 == 0 ? "#fbfbfb" : "#ffffff"));
                //sb.AppendFormat(@"            <td align=""right"" style=""padding:3px 5px;border: 1px solid #e9ecf0;color:black;background-color:{1}"">{0}</td>", studentResult.FirstName, (i % 2 == 0 ? "#fbfbfb" : "#ffffff"));
                foreach (var fieldResult in studentResult.OSFieldResults)
                {
                    sb.AppendFormat(@"        <td style=""padding:3px 5px;border: 1px solid #e9ecf0;{0}"">{1}</td>", GetBackgroundClass(studentResult.GradeId, fieldResult, observationSummaryManager, i), GetViewFieldHtml(observationSummaryManager.LookupLists, fieldResult, studentResult.OSFieldResults));
                }
                sb.AppendFormat(@"        </tr>");
            }
            sb.AppendFormat(@"    </tbody>");
            sb.AppendFormat(@"</table>");
            sb.AppendFormat(@"<div style='margin-top:10px;margin-bottom:10px;'>
                                <table>
                                    <tbody>
                                        <tr>
                                            <td style='padding-right:5px;'>
                                                <span style='background-color:green;border:1px solid black;display:inline-block;height:20px;width:25px;'></span>
                                            </td>
                                            <td style='padding-right:10px;color:black;'>
                                                <strong>Perfect Score</strong>
                                            </td>
                                            <td style='padding-right:5px;'>
                                                <span class='obsBlue' style='background-color:#4697ce ;border:1px solid black;display:inline-block;height:20px;width:25px;'></span>
                                            </td>
                                            <td style='padding-right:10px;color:black;'>
                                                <strong>Exceeds Expectations</strong>
                                            </td>
                                            <td style='padding-right:5px;'>
                                                <span class='obsGreen' style='background-color:#90ED7D;border:1px solid black;display:inline-block;height:20px;width:25px;'></span>
                                            </td>
                                            <td style='padding-right:10px;color:black;'>
                                                <strong>Meets Expectations</strong>
                                            </td>
                                            <td style='padding-right:5px;'>
                                                <span class='obsYellow' style='background-color:#E4D354;border:1px solid black;display:inline-block;height:20px;width:25px;'></span>
                                            </td>
                                            <td style='padding-right:10px;color:black;'>
                                                <strong>Approaches Expectations</strong>
                                            </td>
                                            <td style='padding-right:5px;'>
                                                <span class='obsRed' style='background-color:#BF453D;border:1px solid black;display:inline-block;height:20px;width:25px;'></span>
                                            </td>
                                            <td style='padding-right:10px;color:black;'>
                                                <strong>Does Not Meet Expectations</strong>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>");

            return sb.ToString();
        }

        public OutputDto_Base SendTMInvitation(InputDto_SendTMInvitation input, List<IndexedLookupList> lookupLists)
        {
            var html = GenerateTMInvitationHtml(input, lookupLists);
            var returnValue = new OutputDto_Base();

            var staff = _dbContext.Staffs.FirstOrDefault(p => p.Id == input.StaffId);
            var staffName = staff != null ? staff.FullName : "User";

            // email it
            //var message = new SendGridMessage();
            //message.AddTo(staff.Email); // TODO: staff.Email
            //var b = staff.Email;
            //message.EnableBcc("support@northstaret.net");
            //message.From = new MailAddress("support@northstaret.net");
            //message.Html = html;
            //message.Subject = "Team Meeting Invitation";

            //var credentials = new System.Net.NetworkCredential(SendGridUser, SendGridPassword);
            //var transportWeb = new Web(credentials);
            //transportWeb.Deliver(message);

            // new sendgrid API
            var client = new SendGridAPIClient(SendGridApiKey);
            Email sendTo = new Email(staff.Email);
            Email from = new Email("support@northstaret.net");
            Content mailContent = new Content("text/html", html);
            var message = new Mail(from, "Team Meeting Invitation", sendTo, mailContent);

            dynamic response = client.client.mail.send.post(requestBody: message.Get()).GetAwaiter().GetResult();
            if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                var responseMsg = response.Body.ReadAsStringAsync().Result;
                Log.Error($"Unable to send email: {responseMsg}");
            }

            return returnValue;
        }

        public OutputDto_Base SendTMConcluded(InputDto_SendTMInvitation input)
        {
            //Log.Information("Inside TM Concluded");
            var returnValue = new OutputDto_Base();

            var staff = _dbContext.Staffs.FirstOrDefault(p => p.Id == input.StaffId);
            var staffName = staff != null ? staff.FullName : "User";

            var teamMeeting = _dbContext.TeamMeetings.First(p => p.ID == input.TeamMeetingId);

            TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            DateTime cstTime = TimeZoneInfo.ConvertTimeFromUtc(teamMeeting.MeetingTime, cstZone);
            Console.WriteLine("The date and time are {0} {1}.",
                              cstTime,
                              cstZone.IsDaylightSavingTime(cstTime) ?
                                      cstZone.DaylightName : cstZone.StandardName);

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(@"<div style='margin-left: auto; margin-right: auto;' class=''>
                                <img src='https://app.northstaret.net/assets/img/NorthStar_Logo.png' class=''>

                                <div style='font-size: 16px; margin-top: 10px; margin-bottom: 5px; font-weight: bold;' class=''>Team Meeting Concluded</div>
                                <div style='font-size: 13px; margin-top: 10px; margin-bottom: 10px;' class=''>Dear {0},</div>
                                <div style='font-size: 12px;'>
                                FYI, the Team Meeting <b>{1}</b> at this date and time: <b><i style='color:red'>{2} {5}</i></b> has <i>concluded</i>.
                                </div>
        
                                <br class=''><br class=''>You can access this Team Meeting <a href='{3}' target='_blank' class=''>HERE</a> to view any notes that were taken during the meeting.        <div style='font-size: 10px; color: rgb(102, 102, 102); margin-top: 15px; margin-bottom: 10px;' class=''>© {4} North Star Educational Tools, LLC</div>
                            </div>", staff.FullName, teamMeeting.Title, cstTime.ToString("dddd, MMMM dd, yyyy h:mm tt"), "https://app.northstaret.net/#tm-attend/" + teamMeeting.ID.ToString(), DateTime.Now.Year.ToString(), cstZone.IsDaylightSavingTime(cstTime) ? cstZone.DaylightName : cstZone.StandardName);
            //Log.Information("Creating new Message");
            // email it
            //var message = new SendGridMessage();
            //message.AddTo(staff.Email); // TODO: staff.Email
            //message.EnableBcc("support@northstaret.net");
            //message.From = new MailAddress("support@northstaret.net");
            //message.Html = sb.ToString();
            //message.Subject = "Team Meeting Concluded";

            //Log.Information("Getting Credentials");
            //var credentials = new System.Net.NetworkCredential(SendGridUser, SendGridPassword);
            //var transportWeb = new Web(credentials);
            ////Log.Information("About to deliver");
            //transportWeb.Deliver(message);


            // new sendgrid API
            var client = new SendGridAPIClient(SendGridApiKey);
            Email sendTo = new Email(staff.Email);
            Email from = new Email("support@northstaret.net");
            Content mailContent = new Content("text/html", sb.ToString());
            var message = new Mail(from, "Team Meeting Concluded", sendTo, mailContent);

            dynamic response = client.client.mail.send.post(requestBody: message.Get()).GetAwaiter().GetResult();
            if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                var responseMsg = response.Body.ReadAsStringAsync().Result;
                Log.Error($"Unable to send email: {responseMsg}");
            }


            return returnValue;
        }

        private string GetViewFieldHtml(List<IndexedLookupList> lookupLists, ObservationSummaryFieldScore fieldResult, List<ObservationSummaryFieldScore> allResults)
        {
            var result = "";

            switch(fieldResult.ColumnType)
            {
                case "Textfield":
                    result = fieldResult.StringValue;
                    break;
                case "DecimalRange":
                    result = fieldResult.DecimalValue.HasValue ? fieldResult.DecimalValue.Value.ToString() : "";
                    break;
                case "DropdownRange":
                    result = fieldResult.IntValue.HasValue ? fieldResult.IntValue.Value.ToString() : "";
                    break;
                case "DropdownFromDB":
                case "checklist":
                    result = fieldResult.StringValue;
                    break;
                case "CalculatedFieldClientOnly":
                    result = fieldResult.StringValue;
                    break;
                case "CalculatedFieldDbBacked":
                    result = fieldResult.IntValue.HasValue ? fieldResult.IntValue.Value.ToString() : "";
                    break;
                case "CalculatedFieldDbBackedString":
                    result = fieldResult.StringValue;
                    break;
                case "CalculatedFieldDbOnly":
                    result = fieldResult.StringValue;
                    break;
                default:
                    result = fieldResult.StringValue;
                    break;
            }

            return result;
        }

        private string GetBackgroundClass(int gradeId, ObservationSummaryFieldScore studentFieldScore, OutputDto_ObservationSummaryClass observationSummaryManager, int i)
        {
            switch (studentFieldScore.ColumnType)
            {
                case "Textfield":
                case "checklist":
                    return "color:black;background-color:" + (i % 2 == 0 ? "#fbfbfb" : "#ffffff");
                    break;
                case "DecimalRange":
                    return GetIntColor(gradeId, studentFieldScore, observationSummaryManager, i);
                    break;
                case "DropdownRange":
                    return GetIntColor(gradeId, studentFieldScore, observationSummaryManager,i);
                    break;
                case "DropdownFromDB":
                    return GetIntColor(gradeId, studentFieldScore, observationSummaryManager, i);
                    break;
                case "CalculatedFieldClientOnly":
                    return "color:black;background-color:" + (i % 2 == 0 ? "#fbfbfb" : "#ffffff");
                    break;
                case "CalculatedFieldDbBacked":
                    return GetIntColor(gradeId, studentFieldScore, observationSummaryManager, i);
                    break;
                case "CalculatedFieldDbOnly":
                    return "color:black;background-color:" + (i % 2 == 0 ? "#fbfbfb" : "#ffffff");
                    break;
                default:
                    return "color:black;background-color:" + (i % 2 == 0 ? "#fbfbfb" : "#ffffff");
                    break;
            }

            return (i % 2 == 0 ? "#fbfbfb" : "#ffffff");
        }

        private string GetIntColor(int gradeId, ObservationSummaryFieldScore studentFieldScore, OutputDto_ObservationSummaryClass observationSummaryManager, int k)
        {
            ObservationSummaryBenchmarksByGrade benchmarkArray = null;
            for (var i = 0; i < observationSummaryManager.BenchmarksByGrade.Count; i++)
            {

                if (observationSummaryManager.BenchmarksByGrade[i].GradeId == gradeId)
                {
                    benchmarkArray = observationSummaryManager.BenchmarksByGrade[i];
                }

                if (benchmarkArray != null)
                {
                    for (var j = 0; j < benchmarkArray.Benchmarks.Count; j++)
                    {
                        if (benchmarkArray.Benchmarks[j].DbColumn == studentFieldScore.DbColumn && benchmarkArray.Benchmarks[j].AssessmentId == studentFieldScore.AssessmentId)
                        {
                            if (studentFieldScore.IntValue != null)
                            {
                                // not defined yet
                                //if (studentFieldScore.DecimalValue === $scope.Benchmarks[i].MaxScore) {
                                //	return "obsGreen";
                                //}
                                if (studentFieldScore.IntValue >= benchmarkArray.Benchmarks[j].Exceeds)
                                {
                                    return "background-color: #4697ce;color:black;";
                                }
                                if (studentFieldScore.IntValue >= benchmarkArray.Benchmarks[j].Meets)
                                {
                                    return "background-color: #90ED7D;color:black;";
                                }
                                if (studentFieldScore.IntValue >= benchmarkArray.Benchmarks[j].Approaches)
                                {                                    
                                    return "background-color: #E4D354;color:black;";
                                }
                                if (studentFieldScore.IntValue < benchmarkArray.Benchmarks[j].Approaches)
                                {
                                    return "background-color: #BF453D;color:white;";
                                }
                            }
                        }
                    }
                }
            }
            return "color:black;background-color:" + (k % 2 == 0 ? "#fbfbfb" : "#ffffff");
        }

        public OutputDto_ObservationSummaryClass GetTeamMeetingObservationSummary(InputDto_ObservationSummaryTeamMeeting input)
        {
            //var assessmentIds = "1,3,8,9";

            // bad data check
            if (input.TeamMeetingId == -1 || input.TeamMeetingId == 0 || input.TestDueDateId == -1 || input.TestDueDateId == 0)
            {
                return new OutputDto_ObservationSummaryClass();
            }

            // turn list of string to list of Ints
            //string[] aryAssessmentIds = assessmentIds.Split(Char.Parse(","));
            //List<int> intAssessmentIds = new List<int>();
            List<Assessment> assessmentsToInclude = _dbContext.GetObservationSummaryVisibleAssessments(_currentUser.Id);
            //foreach (var assessmentId in assessmentsToInclude)
            //{
            //    intAssessmentIds.Add(Int32.Parse(assessmentId.Id));
            //}

            //// 1. Get list of assessments from Db
            //List<Assessment> assessmentsToInclude = _dbContext.Assessments
            //    .Include(p => p.FieldGroups)
            //        .Include(p => p.FieldCategories)
            //        .Include(p => p.FieldSubCategories)
            //        .Include(p => p.Fields)
            //        .Where(p => intAssessmentIds.Contains(p.Id)).ToList();

            // get team meeting attendance so that we know if all studednts should be included.  if so, we wipe out staffID befoere getting data
            var tmAttendance = _dbContext.TeamMeetingAttendances.Where(p => p.StaffID == input.StaffId && p.TeamMeetingID == input.TeamMeetingId).FirstOrDefault();
            if(tmAttendance != null && tmAttendance.IncludeAllStudents)
            {
                input.StaffId = null;
            }

            // 2. Create table of concatenated fields from all the tables
            var scores = _dbContext.GetTeamMeetingObservationSummaryData(assessmentsToInclude, input.TeamMeetingId, input.TestDueDateId, input.StaffId, _currentUser.Id);

            // need to get the distinct list of grades for everyone at the team meeting
            var distinctGrades = _dbContext.Grades.Select(p => p.Id).ToList();

            // 3. Get Benchmarks for all of the fields
            // get an an array of benchmarks BY GRADE so that we can use this for other forms of the Obs Summary data
            var benchmarkArray = new List<ObservationSummaryBenchmark>();
            var benchmarksByGrade = new List<ObservationSummaryBenchmarksByGrade>();
            foreach (var grade in distinctGrades)
            {
                var benchmarks = _dbContext.GetClassObservationSummaryBenchmarks(assessmentsToInclude, input.TestDueDateId, grade);
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


        //public List<OutputDto_OptGroupDropdownData> QuickSearchSections(string searchString, int schoolYear)
        //{
        //    // TODO: filter this by the staff you have access to
        //    var teachers = _dbContext.Staffs.Where(p => (p.FirstName.StartsWith(searchString) || p.LastName.StartsWith(searchString))).OrderBy(p => p.LastName).ThenBy(p => p.FirstName).Take(10);

        //    if (teachers.Any())
        //    {
        //        var results = new List<OutputDto_OptGroupDropdownData>();

        //        foreach (var teacher in teachers)
        //        {
        //            // get all the sections for the teacher
        //            var sectionList = new List<OutputDto_DropdownData>();
        //            var sections = _dbContext.StaffSections.Where(p => p.StaffID == teacher.Id && p.Section.SchoolStartYear == schoolYear && p.Section.IsInterventionGroup == false).Select(p => p.Section);

        //            foreach (var section in sections)
        //            {
        //                sectionList.Add(new OutputDto_DropdownData()
        //                {
        //                    id = section.Id,
        //                    text = section.Name + " (" + section.Grade.ShortName + ")"
        //                });
        //            }

        //            results.Add(new OutputDto_OptGroupDropdownData()
        //            {
        //                text = teacher.LastName + ", " + teacher.FirstName,
        //                children = sectionList
        //            });
        //        }


        //        return results;
        //    }
        //    else
        //    {
        //        return new List<OutputDto_OptGroupDropdownData>();
        //    }
        //}

        public TMSectionDetailsDto GetSectionDetails(int sectionId)
        {
            var output = new TMSectionDetailsDto();

            //List<TeamMeetingDto> teamMeetings = new List<TeamMeetingDto>();
            var section = _dbContext.Sections.FirstOrDefault(p => p.Id == sectionId);

            if (section != null)
            {
                output = Mapper.Map<TMSectionDetailsDto>(section);
            }
            return output;
        }

        public OutputDto_GetTeamMeetingList GetTeamMeetingList(InputDto_GetTeamMeetingList input)
        {
            var output = new OutputDto_GetTeamMeetingList();

            //List<TeamMeetingDto> teamMeetings = new List<TeamMeetingDto>();
            var teamMeetings = _dbContext.TeamMeetings.Where(p => p.SchoolYear == input.SchoolYear && (p.StaffID == _currentUser.Id || p.TeamMeetingManagers.Any(j => j.StaffID == _currentUser.Id)))
                                .Select(p => new TeamMeetingDto()
                                {
                                    Comments = p.Comments,
                                    Id = p.ID,
                                    Creator = p.Staff.LastName + ", " + p.Staff.FirstName,
                                    MeetingTime = p.MeetingTime,
                                    NumStaffAttended = p.TeamMeetingAttendances.Where(j => j.Attended).Count(),
                                    NumStaffInvited = p.TeamMeetingAttendances.Count,
                                    NumStudents = p.TeamMeetingStudents.Count,
                                    Title = p.Title
                                }).ToList();

            output.TeamMeetings = teamMeetings;
            return output;
        }

        public OutputDto_GetTeamMeetingList GetAttendTeamMeetingList(InputDto_GetTeamMeetingList input)
        {
            var output = new OutputDto_GetTeamMeetingList();

            //List<TeamMeetingDto> teamMeetings = new List<TeamMeetingDto>();
            var teamMeetings = _dbContext.TeamMeetings.Where(p => p.SchoolYear == input.SchoolYear && (p.StaffID == _currentUser.Id || p.TeamMeetingManagers.Any(j => j.StaffID == _currentUser.Id) || p.TeamMeetingAttendances.Any(j => j.StaffID == _currentUser.Id)))
                                .Select(p => new TeamMeetingDto()
                                {
                                    Comments = p.Comments,
                                    Id = p.ID,
                                    Creator = p.Staff.LastName + ", " + p.Staff.FirstName,
                                    MeetingTime = p.MeetingTime,
                                    NumStaffAttended = p.TeamMeetingAttendances.Where(j => j.Attended).Count(),
                                    NumStaffInvited = p.TeamMeetingAttendances.Count,
                                    NumStudents = p.TeamMeetingStudents.Count,
                                    Title = p.Title
                                }).ToList();

            output.TeamMeetings = teamMeetings;
            return output;
        }

        public OutputDto_ManageTeamMeeting GetTeamMeeting(InputDto_SimpleIdAndBool input)
        {
            OutputDto_ManageTeamMeeting output = new OutputDto_ManageTeamMeeting();

            var meeting = _dbContext.TeamMeetings
                .Include(x => x.TeamMeetingAttendances.Select(j => j.Staff))
                .Include(x => x.TeamMeetingManagers.Select(j => j.Staff))
                .Include(x => x.TeamMeetingStudents.Select(j => j.Student))
                .Include(x => x.TeamMeetingStudentNotes)
                .FirstOrDefault(p => p.ID == input.Id);

            if (meeting != null)
            {
                output.ID = meeting.ID;
                output.Title = meeting.Title;
                output.Comments = meeting.Comments;
                output.MeetingTime = meeting.MeetingTime;
                output.TestDueDateId = meeting.TestDueDateId;
                output.Attendees = Mapper.Map<List<OutputDto_DropdownData>>(meeting.TeamMeetingAttendances.ToList());
                output.Managers = Mapper.Map<List<OutputDto_DropdownData>>(meeting.TeamMeetingManagers.ToList());
                output.SchoolYear = meeting.SchoolYear;
                output.TeamMeetingStudents = Mapper.Map<List<TeamMeetingStudentDto>>(meeting.TeamMeetingStudents.ToList());

                // extended properties request?
                if (input.flag)
                {
                    output.TeamMeetingAttendances = Mapper.Map<List<TeamMeetingAttendanceDto>>(meeting.TeamMeetingAttendances.ToList());
                }

                // now add the details for each section
                var sections = new List<TMSectionDetailsDto>();
                foreach (var studentMeeting in meeting.TeamMeetingStudents.ToList())
                {
                    // if we haven't added this section yet, add it
                    if (!sections.Any(s => s.Id == studentMeeting.SectionID))
                    {
                        var section = new TMSectionDetailsDto();
                        section.Id = studentMeeting.Section.Id;
                        section.StaffID = studentMeeting.Section.StaffID;
                        section.Description = studentMeeting.Section.Description;
                        section.Grade = studentMeeting.Section.Grade.ShortName;
                        section.Name = studentMeeting.Section.Name;
                        section.SchoolID = studentMeeting.Section.SchoolID;
                        section.SchoolStartYear = studentMeeting.Section.SchoolStartYear;
                        section.StaffFullName = studentMeeting.Section.Staff.LastName + "," + studentMeeting.Section.Staff.FirstName;
                        section.StaffLastName = studentMeeting.Section.Staff.LastName;
                        sections.Add(section);
                    }
                }
                //Mapper.Map(sections, output.Sections, typeof(List<Section>), typeof(List<TMSectionDetailsDto>));
                //  sections = sections.ToList();
                output.Sections = sections; //Mapper.Map<List<TMSectionDetailsDto>>(sections);

                // set NumStudents
                foreach (var section in output.Sections)
                {
                    // get the number of students
                    section.NumStudents = output.TeamMeetingStudents.Count(p => p.SectionID == section.Id);
                }

            }

            // get the attendeegroups for the current user
            output.AttendeeGroups = Mapper.Map<List<AttendeeGroupDto>>(_dbContext.AttendeeGroups.Where(p => p.StaffId == _currentUser.Id).ToList());

            return output;
        }

        public bool DeleteTeamMeeting(int id)
        {

            var db_meeting = _dbContext.TeamMeetings.FirstOrDefault(p => p.ID == id);

            if (db_meeting == null)
            {
                return true;
            }

            //// make sure no notes or don't allow meeting to be deleted
            //var existingNote = _dbContext.TeamMeetingStudentNotes.FirstOrDefault(p => p.TeamMeetingID == id);

            //if (existingNote != null)
            //{
            //    throw new UserDisplayableException("This meeting currently has notes saved.  Please either delete the notes first, or do not delete this meeting.", null);
            //}

            // remove all the team meeting student notes
            _dbContext.TeamMeetingStudentNotes.RemoveRange(db_meeting.TeamMeetingStudentNotes);
            
            // remove all the team meeting attendances
            _dbContext.TeamMeetingAttendances.RemoveRange(db_meeting.TeamMeetingAttendances);

            // remove all the team meeting managers
            _dbContext.TeamMeetingManagers.RemoveRange(db_meeting.TeamMeetingManagers);

            // remove all students
            _dbContext.TeamMeetingStudents.RemoveRange(db_meeting.TeamMeetingStudents);

            // delete the meeting
            _dbContext.TeamMeetings.Remove(db_meeting);

            _dbContext.SaveChanges();
            return true;

        }

        public bool DeleteAttendeeGroup(int id)
        {

            var db_group = _dbContext.AttendeeGroups.FirstOrDefault(p => p.Id == id);

            if (db_group == null)
            {
                return true;
            }

            // remove all the groupstaffs
            _dbContext.AttendeeGroupStaffs.RemoveRange(db_group.AttendeeGroupStaffs);

            // delete the meeting
            _dbContext.AttendeeGroups.Remove(db_group);

            _dbContext.SaveChanges();
            return true;

        }

        public OutputDto_Base SaveSingleTeamMeetingAttendance(TeamMeetingAttendanceDto teammeetingattendance)
        {
            var result = new OutputDto_Base();

            var tma = _dbContext.TeamMeetingAttendances.FirstOrDefault(p => p.ID == teammeetingattendance.ID);
            if(tma != null)
            {
                tma.Attended = teammeetingattendance.Attended;
                tma.IncludeAllStudents = teammeetingattendance.IncludeAllStudents;
                _dbContext.SaveChanges();
            }
            return result;
        }

        public OutputDto_Base SaveAllTeamMeetingAttendances(List<TeamMeetingAttendanceDto> teammeetingattendances)
        {
            var result = new OutputDto_Base();

            foreach (var tma in teammeetingattendances)
            {
                var db_tma = _dbContext.TeamMeetingAttendances.FirstOrDefault(p => p.ID == tma.ID);
                if (db_tma != null)
                {
                    db_tma.Attended = tma.Attended;
                    db_tma.IncludeAllStudents = tma.IncludeAllStudents;
                }
            }
            _dbContext.SaveChanges();
            return result;
        }

        public AttendeeGroupDto SaveAttendeeGroup(InputDto_SaveAttendeeGroup input)
        {
            // determine if this is a new section or not
            var db_group = _dbContext.AttendeeGroups
                .FirstOrDefault(p => p.GroupName == input.GroupName && p.StaffId == _currentUser.Id);

            if (db_group != null)
            {
                // wipe out any existing staff
                _dbContext.AttendeeGroupStaffs.RemoveRange(db_group.AttendeeGroupStaffs);
            }
            else
            {
                db_group = new AttendeeGroup();
                db_group.StaffId = _currentUser.Id;
                db_group.GroupName = input.GroupName;
                _dbContext.AttendeeGroups.Add(db_group);
            }
            _dbContext.SaveChanges();

            input.Attendees.Each(p => db_group.AttendeeGroupStaffs.Add(new AttendeeGroupStaff()
            {
                StaffId = p.id,
                AttendeeGroupId = db_group.Id
            }));
            _dbContext.SaveChanges();

            return Mapper.Map<AttendeeGroupDto>(db_group);
        }

        public bool SaveTeamMeeting(OutputDto_ManageTeamMeeting meeting)
        {
            // determine if this is a new section or not
            var db_meeting = _dbContext.TeamMeetings
                .Include(p => p.TeamMeetingStudents)
                .Include(p => p.TeamMeetingManagers)
                .Include(p => p.TeamMeetingAttendances)
                .FirstOrDefault(p => p.ID == meeting.ID);

            if (db_meeting == null)
            {
                db_meeting = new TeamMeeting()
                {
                    Title = meeting.Title,
                    SchoolYear = meeting.SchoolYear,
                    StaffID = _currentUser.Id,
                    MeetingTime = meeting.MeetingTime,
                    Comments = meeting.Comments,
                    TestDueDateId = meeting.TestDueDateId
                };

                _dbContext.TeamMeetings.Add(db_meeting);
            }
            else
            {
                db_meeting.Title = meeting.Title;
                db_meeting.SchoolYear = meeting.SchoolYear;
                db_meeting.MeetingTime = meeting.MeetingTime;
                db_meeting.Comments = meeting.Comments;
                db_meeting.TestDueDateId = meeting.TestDueDateId;
            }
            _dbContext.SaveChanges();

            #region MeetingManagers
            var meetingManagersToDelete = new List<TeamMeetingManager>();
            // remove deleted staffsections, staffsections that are attached to db_section but not in section
            db_meeting.TeamMeetingManagers
                    .Where(d => !meeting.Managers.Any(ct => ct.id == d.StaffID))
                    .Each(deleted => meetingManagersToDelete.Add(deleted));

            _dbContext.TeamMeetingManagers.RemoveRange(meetingManagersToDelete);

            //update or add new teammeetingmanagers
            meeting.Managers.Each(ct =>
            {
                // check to see if this is an existing co-teacher record
                var manager = db_meeting.TeamMeetingManagers.FirstOrDefault(d => d.StaffID == ct.id);
                if (manager == null)
                {
                    manager = new TeamMeetingManager();
                    db_meeting.TeamMeetingManagers.Add(manager);
                }
                manager.StaffID = ct.id;
                manager.TeamMeetingID = meeting.ID;
            });
            #endregion

            #region MeetingStudents
            var meetingStudentsToDelete = new List<TeamMeetingStudent>();
            // remove deleted staffsections, staffsections that are attached to db_section but not in section
            db_meeting.TeamMeetingStudents
                    .Where(d => !meeting.TeamMeetingStudents.Any(ct => ct.StudentID == d.StudentID))
                    .Each(deleted => meetingStudentsToDelete.Add(deleted));

            _dbContext.TeamMeetingStudents.RemoveRange(meetingStudentsToDelete);

            //update or add new teammeetingstudents
            //this requires some additional work
            meeting.TeamMeetingStudents.Each(ct =>
            {
                // check to see if this is an existing co-teacher record
                var student = db_meeting.TeamMeetingStudents.FirstOrDefault(d => d.StudentID == ct.StudentID);
                if (student == null)
                {
                    student = new TeamMeetingStudent();
                    db_meeting.TeamMeetingStudents.Add(student);
                }
                student.StaffID = ct.StaffID; // how to get staff id???
                student.TeamMeetingID = meeting.ID;
                student.StudentID = ct.StudentID;
                student.SectionID = ct.SectionID;
            });
            #endregion

            #region MeetingAttendees
            var meetingAttendancesToDelete = new List<TeamMeetingAttendance>();
            // remove deleted staffsections, staffsections that are attached to db_section but not in section
            db_meeting.TeamMeetingAttendances
                    .Where(d => !meeting.Attendees.Any(ct => ct.id == d.StaffID))
                    .Each(deleted => meetingAttendancesToDelete.Add(deleted));

            _dbContext.TeamMeetingAttendances.RemoveRange(meetingAttendancesToDelete);

            //update or add new teammeetingmanagers
            meeting.Attendees.Each(ct =>
            {
                // check to see if this is an existing co-teacher record
                var attendance = db_meeting.TeamMeetingAttendances.FirstOrDefault(d => d.StaffID == ct.id);
                if (attendance == null)
                {
                    attendance = new TeamMeetingAttendance();
                    db_meeting.TeamMeetingAttendances.Add(attendance);
                }
                attendance.StaffID = ct.id;
                attendance.TeamMeetingID = meeting.ID;
            });
            #endregion

            _dbContext.SaveChanges();
            return true;
        }

        public OutputDto_TeamMeetingStudentNotes GetNotesForStudentTeamMeeting(InputDto_TeamMeetingStudent input)
        {
            var notes = _dbContext.TeamMeetingStudentNotes.Include(path => path.Staff)
                .Where(p => p.StudentID == input.StudentId && p.TeamMeetingID == input.TeamMeetingId).ToList();

            var student = _dbContext.Students.First(p => p.Id == input.StudentId);

            return new OutputDto_TeamMeetingStudentNotes
            {
                Student = Mapper.Map<StudentDto>(student),
                Notes = Mapper.Map<List<TeamMeetingStudentNoteDto>>(notes)
            };
        }

        public OutputDto_TeamMeetingStudentAllNotes GetNotesForStudentTeamMeetings(InputDto_SimpleId input)
        {
            var tmStudents = _dbContext.TeamMeetingStudents
                .Where(p => p.StudentID == input.Id).GroupBy(p => p.TeamMeeting).ToList();

            var result = new OutputDto_TeamMeetingStudentAllNotes();
            var student = Mapper.Map<StudentDto>(_dbContext.Students.First(p => p.Id == input.Id));
            result.Student = student;

            foreach (var grouping in tmStudents)
            {
                var meetingNotes = Mapper.Map<List<TeamMeetingStudentNoteDto>>(grouping.Key.TeamMeetingStudentNotes.Where(p => p.StudentID == input.Id).ToList());
                var teamMeetingDto = Mapper.Map<TeamMeetingDto>(grouping.Key);
                teamMeetingDto.TeamMeetingStudentNotes = meetingNotes.OrderByDescending(p => p.NoteDate).ToList();
                teamMeetingDto.TeamMeetingStudents = null;
                teamMeetingDto.TeamMeetingManagers = null;
                teamMeetingDto.TeamMeetingAttendances = null;
                result.Meetings.Add(teamMeetingDto);
            }

            result.Meetings = result.Meetings.OrderByDescending(p => p.MeetingTime).ToList();

            return result;
        }

        public OutputDto_Success SaveNote(InputDto_SaveNote input)
        {
            var existingNote = _dbContext.TeamMeetingStudentNotes.FirstOrDefault(p =>
                p.ID == input.NoteId);

            if (existingNote == null)
            {
                existingNote = new TeamMeetingStudentNote()
                {
                    StudentID = input.StudentId,
                    StaffID = _currentUser.Id,
                    TeamMeetingID = input.TeamMeetingId,
                    Note = input.NoteHtml,
                    NoteDate = DateTime.Now
                };
                _dbContext.TeamMeetingStudentNotes.Add(existingNote);
            }
            else
            {
                existingNote.Note = input.NoteHtml;
                existingNote.NoteDate = DateTime.Now;
            }
            _dbContext.SaveChanges();

            return new OutputDto_Success { Success = true };
        }

        public OutputDto_Success DeleteNote(InputDto_SimpleId input)
        {
            var existingNote = _dbContext.TeamMeetingStudentNotes.FirstOrDefault(p =>
                p.ID == input.Id);

            if (existingNote == null)
            {
                return new OutputDto_Success { Success = false };
            }

            _dbContext.TeamMeetingStudentNotes.Remove(existingNote);
            _dbContext.SaveChanges();

            return new OutputDto_Success { Success = true };
        }
    }
}
