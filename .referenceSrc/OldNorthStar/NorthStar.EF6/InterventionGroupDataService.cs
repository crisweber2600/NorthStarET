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
using EntityDto.DTO.Admin.InterventionGroup;
using NorthStar4.CrossPlatform.DTO.Admin.InterventionGroup;
using NorthStar4.CrossPlatform.Entity;

namespace NorthStar.EF6
{
    public class InterventionGroupDataService : NSBaseDataService
    {
        public InterventionGroupDataService(ClaimsIdentity user, string loginConnectionString) : base(user, loginConnectionString)
        {

        }

        public OutputDto_Base SaveDate(InputDto_SaveStudentInterventionStartEnd startEnd)
        {
            var studentDataId = _dbContext.StudentSections.First(p => p.Id == startEnd.Id);
            var resultValue = new OutputDto_Base();

            if (studentDataId != null)
            {

                if (startEnd.dataType == "start")
                {
                    if (startEnd.date.HasValue)
                    {
                        studentDataId.StartDate = startEnd.date;
                        _dbContext.SaveChanges();
                    }
                    else
                    {
                        resultValue.Status.StatusCode = StatusCode.UserDisplayableException;
                        resultValue.Status.StatusMessage = "Cannot save empty start date.";
                    }
                }
                else
                {
                    if (startEnd.date.HasValue)
                    {
                        studentDataId.EndDate = startEnd.date;
                        _dbContext.SaveChanges();
                    }
                    else
                    {
                        studentDataId.EndDate = null;
                        _dbContext.SaveChanges();
                    }
                }
            }
            else
            {
                //log exception
                resultValue.Status.StatusCode = StatusCode.UserDisplayableException;
                resultValue.Status.StatusMessage = "There was an error while saving data.";
            }
            return resultValue;
        }

        public OutputDto_Base AddStudentToGroup(InputDto_AddStudentToInterventionGroup input)
        {
            var result = new OutputDto_Base();

                _dbContext.StudentSections.Add(new StudentSection()
                {
                    ClassID = input.groupId,
                    StudentID = input.studentId,
                    StartDate = input.startDate,
                    EndDate = input.endDate
                });
                _dbContext.SaveChanges();

            return result;
        }

        public OutputDto_Base MoveStudentToGroup(InputDto_MoveStudentToInterventionGroup input)
        {
            var result = new OutputDto_Base();

            //TODO: Check attendance to see if this is an issue
            var studentSection = _dbContext.StaffInterventionGroups.First(p => p.Id == input.studentSectionId);

            studentSection.InterventionGroupId = input.newGroupId;
            _dbContext.SaveChanges();

            return result;
        }

        public OutputDto_Base RemoveStudentFromGroup(InputDto_RemoveStudentFromInterventionGroup input)
        {
            var result = new OutputDto_Base();
            
            _dbContext.StudentSections.Remove(new StudentSection()
            {
                Id = input.studentSectionId
            });
            _dbContext.SaveChanges();

            return result;
        }

        private int GetAttendanceReasonID(string strReason)
        {
            var reason = _dbContext.AttendanceReasons.FirstOrDefault(p => p.Reason == strReason);

            if (reason != null)
            {
                return reason.Id;
            }
            else
            {
                //TODO: Log errant state
                return -1;
            }
        }

        public OutputDto_Base SaveSingleAttendance(InputDto_SaveSingleAttendance input)
        {
            var result = new OutputDto_Base();

                var existing =
                    _dbContext.InterventionAttendances.FirstOrDefault(
                        p => p.ClassStartEndID == input.StartEndDateId && p.AttendanceDate == input.Date);

                if (existing != null)
                {
                    existing.Notes = input.Notes;
                    existing.RecorderID = _currentUser.Id;
                    existing.AttendanceReasonID = GetAttendanceReasonID(input.Status);
                }
                else
                {
                    existing = new InterventionAttendance()
                    {
                        Notes = input.Notes,
                        RecorderID = _currentUser.Id,
                        AttendanceReasonID = GetAttendanceReasonID(input.Status),
                        AttendanceDate = input.Date,
                        ClassStartEndID = input.StartEndDateId,
                        SectionID = input.SectionId,
                        StudentID = input.StudentId
                    };
                    _dbContext.InterventionAttendances.Add(existing);
                }
                _dbContext.SaveChanges();

            return result;
        }

        public OutputDto_Base ApplyStatusNotes(InputDto_ApplyStatusNotes input)
        {
            var result = new OutputDto_Base();

            var statusId = GetAttendanceReasonID(input.Status);
            var mondayDate = GetMonday(input.Date);
            var fridayDate = mondayDate.AddDays(4);

            // for whole teacher
            if (input.SectionId == -1)
            {
                var studentsForTeacher = (from t1 in _dbContext.StudentInterventionGroups
                                          join t2 in _dbContext.Students on t1.StudentID equals t2.Id
                                          join t3 in _dbContext.InterventionGroups on t1.InterventionGroupId equals t3.Id
                                          join t4 in _dbContext.StaffInterventionGroups on t3.Id equals t4.InterventionGroupId
                                          where
                                              (t2.IsActive == true || t2.IsActive == null) 
                                              && t4.StaffID == input.StaffId
                                              && t3.SchoolStartYear == input.SchoolStartYear
                                              && (t1.StartDate <= input.Date && (t1.EndDate >= input.Date || t1.EndDate == null))// TODO: test. this is ensures we don't update kids whose group has started or ended already
                                          select t1).Distinct();

                foreach (StudentInterventionGroup studentClass in studentsForTeacher)
                {
                    int studentId = studentClass.StudentID;
                    int classId = studentClass.InterventionGroupId;
                    int StartEndDateID = studentClass.Id;

                    // get group and see what days it meets on
                    var group = _dbContext.InterventionGroups.First(p => p.Id == classId);
                    // if the group that this gid is in doesn't meet on the day we are applying, skip the kid
                    switch (input.Date.DayOfWeek)
                    {
                        case DayOfWeek.Monday:
                            if (group.MondayMeet != true)
                                continue;
                            break;
                        case DayOfWeek.Tuesday:
                            if (group.TuesdayMeet != true)
                                continue;
                            break;
                        case DayOfWeek.Wednesday:
                            if (group.WednesdayMeet != true)
                                continue;
                            break;
                        case DayOfWeek.Thursday:
                            if (group.ThursdayMeet != true)
                                continue;
                            break;
                        case DayOfWeek.Friday:
                            if (group.FridayMeet != true)
                                continue;
                            break;
                    }

                    var record =
                        _dbContext.InterventionAttendances.FirstOrDefault(
                            p => p.ClassStartEndID == StartEndDateID && p.AttendanceDate == input.Date);

                    if (record != null)
                    {
                        record.Notes = input.Notes;
                        record.AttendanceReasonID = statusId;
                        record.RecorderID = _currentUser.Id;
                    }
                    else
                    {
                        record = new InterventionAttendance()
                        {
                            Notes = input.Notes,
                            RecorderID = _currentUser.Id,
                            AttendanceReasonID = statusId,
                            AttendanceDate = input.Date,
                            ClassStartEndID = StartEndDateID,
                            SectionID = classId,
                            StudentID = studentId
                        };
                        _dbContext.InterventionAttendances.Add(record);
                    }
                }
            }
            else
            {
                int classId = input.SectionId;
                // get group and see what days it meets on
                var group = _dbContext.InterventionGroups.First(p => p.Id == classId);
                // if the group that this gid is in doesn't meet on the day we are applying, skip the kid
                switch (input.Date.DayOfWeek)
                {
                    case DayOfWeek.Monday:
                        if (group.MondayMeet != true)
                            return result;
                        break;
                    case DayOfWeek.Tuesday:
                        if (group.TuesdayMeet != true)
                            return result;
                        break;
                    case DayOfWeek.Wednesday:
                        if (group.WednesdayMeet != true)
                            return result;
                        break;
                    case DayOfWeek.Thursday:
                        if (group.ThursdayMeet != true)
                            return result;
                        break;
                    case DayOfWeek.Friday:
                        if (group.FridayMeet != true)
                            return result;
                        break;
                }


                var studentsForTeacher = (from t1 in _dbContext.StudentInterventionGroups
                                          join t2 in _dbContext.Students on t1.StudentID equals t2.Id
                                          join t3 in _dbContext.InterventionGroups on t1.InterventionGroupId equals t3.Id
                                          where
                                              (t2.IsActive == true || t2.IsActive == null)
                                              && t3.Id == classId
                                              && (t1.StartDate <= input.Date && (t1.EndDate >= input.Date || t1.EndDate == null))// TODO: test. this is ensures we don't update kids whose group has started or ended already
                                          select t1).Distinct();

                foreach (StudentInterventionGroup studentClass in studentsForTeacher)
                {
                    int studentId = studentClass.StudentID;
                    int StartEndDateID = studentClass.Id;

                    var record =
                        _dbContext.InterventionAttendances.FirstOrDefault(
                            p => p.ClassStartEndID == StartEndDateID && p.AttendanceDate == input.Date);

                    if (record != null)
                    {
                        record.Notes = input.Notes;
                        record.AttendanceReasonID = statusId;
                        record.RecorderID = _currentUser.Id;
                    }
                    else
                    {
                        record = new InterventionAttendance()
                        {
                            Notes = input.Notes,
                            RecorderID = _currentUser.Id,
                            AttendanceReasonID = statusId,
                            AttendanceDate = input.Date,
                            ClassStartEndID = StartEndDateID,
                            SectionID = classId,
                            StudentID = studentId
                        };
                        _dbContext.InterventionAttendances.Add(record);
                    }
                }
            }

            _dbContext.SaveChanges();



            return result;
        }

        private DateTime GetMonday(DateTime mondayDate)
        {
            //TODO: move this logic to a utility class
            switch (mondayDate.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    //no-op
                    break;
                case DayOfWeek.Tuesday:
                    mondayDate = mondayDate.AddDays(-1);
                    break;
                case DayOfWeek.Wednesday:
                    mondayDate = mondayDate.AddDays(-2);
                    break;
                case DayOfWeek.Thursday:
                    mondayDate = mondayDate.AddDays(-3);
                    break;
                case DayOfWeek.Friday:
                    mondayDate = mondayDate.AddDays(-4);
                    break;
                case DayOfWeek.Saturday:
                    mondayDate = mondayDate.AddDays(-5);
                    break;
                case DayOfWeek.Sunday:
                    mondayDate = mondayDate.AddDays(1); // week starts on Monday
                    break;
            }

            return mondayDate;
        }

        public OutputDto_WeeklyAttendance GetWeeklyAttendanceResults(InputDto_GetWeeklyAttendanceResults input)
        {
            var weekDayList = new List<AttendanceWeekdayDto>();

            var mondayDate = GetMonday(input.MondayDate);


            var fridayDate = mondayDate.AddDays(4);

            for (var i = 0; i < 5; i++)
            {
                var newDate = mondayDate.AddDays(i);
                weekDayList.Add(new AttendanceWeekdayDto() { DayOfWeek = newDate.DayOfWeek.ToString(), LongDate = newDate.ToString("MMM dd, yyyy"), Date = newDate });
            }

            var data = _dbContext.Database.SqlQuery<WeeklyAttendanceResult>(string.Format("SELECT * FROM dbo.[ns4_udf_sp_GetWeeklyAttendance]({0}, '{1}', {2}, {3}, {4})", input.InterventionGroupId, mondayDate, input.StaffId, input.SchoolStartYear, input.SchoolId)).ToList();
            var districtHolidays = GetHolidaysForDistrict(mondayDate, fridayDate);

            foreach (var attendanceResult in data)
            {
                //  add current school's holidays to Dictionary
                if (attendanceResult.SchoolID.HasValue && !SchoolHolidays.ContainsKey(attendanceResult.SchoolID.Value))
                {
                    SchoolHolidays.Add(attendanceResult.SchoolID.Value, GetHolidaysForSchool(attendanceResult.SchoolID.Value, mondayDate, fridayDate));
                }

                SetCellProperties(attendanceResult, attendanceResult.SchoolID.HasValue ? SchoolHolidays[attendanceResult.SchoolID.Value] : null, mondayDate, districtHolidays);
            }

            return new OutputDto_WeeklyAttendance()
            {
                AttendanceData = data,
                WeekDays = weekDayList
            };
        }

        #region Temporary Attendance Stuff
        private Dictionary<int, List<SchoolCalendar>> m_SchoolHolidays;
        private Dictionary<int, List<SchoolCalendar>> SchoolHolidays
        {
            get
            {
                if (m_SchoolHolidays != null)
                {
                    return m_SchoolHolidays;
                }
                else
                {
                    m_SchoolHolidays = new Dictionary<int, List<SchoolCalendar>>();
                    return m_SchoolHolidays;
                }
            }
        }

        private Dictionary<int, InterventionGroup> m_Sections;
        private Dictionary<int, InterventionGroup> Sections
        {
            get
            {
                if (m_Sections != null)
                {
                    return m_Sections;
                }
                else
                {
                    m_Sections = new Dictionary<int, InterventionGroup>();
                    return m_Sections;
                }
            }
        }


        private List<DistrictCalendar> GetHolidaysForDistrict(DateTime selectedMonday, DateTime selectedFriday)
        {
            return
                _dbContext.DistrictCalendars.Where(
                    p =>
                        p.Start >= selectedMonday &&
                        p.Start <= selectedFriday).ToList();
        }

        private void SetCellProperties(WeeklyAttendanceResult row, List<SchoolCalendar> holidaysForSchool, DateTime mondayDate, List<DistrictCalendar> districtHolidays)
        {
            var tuesdayDate = mondayDate.AddDays(1);
            var wednesdayDate = mondayDate.AddDays(2);
            var thursdayDate = mondayDate.AddDays(3);
            var fridayDate = mondayDate.AddDays(4);


            // add the current section to the dictionary
            InterventionGroup currentSection;
            if (Sections.ContainsKey(row.SectionID))
            {
                currentSection = Sections[row.SectionID];
            }
            else
            {
                currentSection = _dbContext.InterventionGroups.First(p => p.Id == row.SectionID);
                Sections.Add(currentSection.Id, currentSection);
            }


            // if the current date is AFTER the end of the intervention
            var proposedStatus = string.Empty;
            var canEdit = true;
            var proposedNotes = string.Empty;

            //   Monday
            GetStatusAndEdit(row, ref canEdit, ref proposedStatus, ref proposedNotes, currentSection, mondayDate, districtHolidays, holidaysForSchool);
            row.MondayStatus = !string.IsNullOrEmpty(proposedStatus) ? proposedStatus : row.MondayStatus;
            row.MondayNotes = !string.IsNullOrEmpty(proposedNotes) ? proposedNotes : row.MondayNotes;
            row.MondayCanEdit = canEdit;
            proposedNotes = string.Empty;
            proposedStatus = string.Empty;
            canEdit = true;

            GetStatusAndEdit(row, ref canEdit, ref proposedStatus, ref proposedNotes, currentSection, tuesdayDate, districtHolidays, holidaysForSchool);
            row.TuesdayStatus = !string.IsNullOrEmpty(proposedStatus) ? proposedStatus : row.TuesdayStatus;
            row.TuesdayNotes = !string.IsNullOrEmpty(proposedNotes) ? proposedNotes : row.TuesdayNotes;
            row.TuesdayCanEdit = canEdit;
            proposedNotes = string.Empty;
            proposedStatus = string.Empty;
            canEdit = true;

            GetStatusAndEdit(row, ref canEdit, ref proposedStatus, ref proposedNotes, currentSection, wednesdayDate, districtHolidays, holidaysForSchool);
            row.WednesdayStatus = !string.IsNullOrEmpty(proposedStatus) ? proposedStatus : row.WednesdayStatus;
            row.WednesdayNotes = !string.IsNullOrEmpty(proposedNotes) ? proposedNotes : row.WednesdayNotes;
            row.WednesdayCanEdit = canEdit;
            proposedNotes = string.Empty;
            proposedStatus = string.Empty;
            canEdit = true;

            GetStatusAndEdit(row, ref canEdit, ref proposedStatus, ref proposedNotes, currentSection, thursdayDate, districtHolidays, holidaysForSchool);
            row.ThursdayStatus = !string.IsNullOrEmpty(proposedStatus) ? proposedStatus : row.ThursdayStatus;
            row.ThursdayNotes = !string.IsNullOrEmpty(proposedNotes) ? proposedNotes : row.ThursdayNotes;
            row.ThursdayCanEdit = canEdit;
            proposedNotes = string.Empty;
            proposedStatus = string.Empty;
            canEdit = true;

            GetStatusAndEdit(row, ref canEdit, ref proposedStatus, ref proposedNotes, currentSection, fridayDate, districtHolidays, holidaysForSchool);
            row.FridayStatus = !string.IsNullOrEmpty(proposedStatus) ? proposedStatus : row.FridayStatus;
            row.FridayNotes = !string.IsNullOrEmpty(proposedNotes) ? proposedNotes : row.FridayNotes;
            row.FridayCanEdit = canEdit;
        }

        private void GetStatusAndEdit(WeeklyAttendanceResult row, ref bool canEdit, ref string proposedStatus, ref string proposedNotes, InterventionGroup currentSection, DateTime cellDate, List<DistrictCalendar> districtHolidays, List<SchoolCalendar> holidaysForSchool)
        {
            DateTime endDate;

            if ((DateTime.TryParse(row.InterventionEnd, out endDate) && endDate < cellDate))
            {
                proposedStatus = "Intervention Ended";
                canEdit = false;
            }
            // if the current date is BEFORE the start of the intervention
            else if (DateTime.Parse(row.InterventionStart) > cellDate)
            {
                proposedStatus = "Intervention Not Started";
                canEdit = false;
            }
            // determine if this is a district holiday
            else if (districtHolidays.Any(p => cellDate.Date >= p.Start.Date && cellDate.Date < p.End.Date))
            {
                proposedStatus = "No School";
                proposedNotes = districtHolidays.First((p => cellDate.Date >= p.Start.Date && cellDate.Date < p.End.Date)).Subject;
                canEdit = false;
            }
            // determine if this is a school holiday
            else if (holidaysForSchool != null && holidaysForSchool.Any((p => cellDate.Date >= p.Start.Date && cellDate.Date < p.End.Date)))
            {
                proposedStatus = "No School";
                proposedNotes = holidaysForSchool.First((p => cellDate.Date >= p.Start.Date && cellDate.Date < p.End.Date)).Subject;
                canEdit = false;
            }
            // determine if this is a scheduled to meet day
            else
            {
                switch (cellDate.DayOfWeek)
                {
                    case DayOfWeek.Monday:
                        if (currentSection.MondayMeet != null && currentSection.MondayMeet.Value)
                        {
                            if (String.IsNullOrEmpty(row.MondayStatus))
                            {
                                if (cellDate.Date > DateTime.Now)
                                {
                                    proposedStatus = "Scheduled To Meet";
                                }
                                else
                                {
                                    proposedStatus = "Intervention Delivered";
                                }
                            }
                            canEdit = true;
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(row.MondayStatus))
                            {
                                proposedStatus = "None";
                            }
                            canEdit = true;
                        }
                        break;
                    case DayOfWeek.Tuesday:
                        if (currentSection.TuesdayMeet != null && currentSection.TuesdayMeet.Value)
                        {
                            if (String.IsNullOrEmpty(row.TuesdayStatus))
                            {
                                if (cellDate.Date > DateTime.Now)
                                {
                                    proposedStatus = "Scheduled To Meet";
                                }
                                else
                                {
                                    proposedStatus = "Intervention Delivered";
                                }
                            }
                            canEdit = true;
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(row.TuesdayStatus))
                            {
                                proposedStatus = "None";
                            }
                            canEdit = true;
                        }
                        break;
                    case DayOfWeek.Wednesday:
                        if (currentSection.WednesdayMeet != null && currentSection.WednesdayMeet.Value)
                        {
                            if (String.IsNullOrEmpty(row.WednesdayStatus))
                            {
                                if (cellDate.Date > DateTime.Now)
                                {
                                    proposedStatus = "Scheduled To Meet";
                                }
                                else
                                {
                                    proposedStatus = "Intervention Delivered";
                                }
                            }
                            canEdit = true;
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(row.WednesdayStatus))
                            {
                                proposedStatus = "None";
                            }
                            canEdit = true;
                        }
                        break;
                    case DayOfWeek.Thursday:
                        if (currentSection.ThursdayMeet != null && currentSection.ThursdayMeet.Value)
                        {
                            if (String.IsNullOrEmpty(row.ThursdayStatus))
                            {
                                if (cellDate.Date > DateTime.Now)
                                {
                                    proposedStatus = "Scheduled To Meet";
                                }
                                else
                                {
                                    proposedStatus = "Intervention Delivered";
                                }
                            }
                            canEdit = true;
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(row.ThursdayStatus))
                            {
                                proposedStatus = "None";
                            }
                            canEdit = true;
                        }
                        break;
                    case DayOfWeek.Friday:
                        if (currentSection.FridayMeet != null && currentSection.FridayMeet.Value)
                        {
                            if (String.IsNullOrEmpty(row.FridayStatus))
                            {
                                if (cellDate.Date > DateTime.Now)
                                {
                                    proposedStatus = "Scheduled To Meet";
                                }
                                else
                                {
                                    proposedStatus = "Intervention Delivered";
                                }
                            }
                            canEdit = true;
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(row.FridayStatus))
                            {
                                proposedStatus = "None";
                            }
                            canEdit = true;
                        }
                        break;

                }
            }
        }

        private List<SchoolCalendar> GetHolidaysForSchool(int schoolID, DateTime selectedMonday, DateTime selectedFriday)
        {
            return _dbContext.SchoolCalendars.Where(
                            p =>
                            p.SchoolID == schoolID && p.Start >= selectedMonday &&
                            p.Start <= selectedFriday).ToList();
        }


        #endregion


        
        public OutputDto_StudentQuickSearch StudentQuickSearch(InputDto_StudentQuickSearch input)
        {
            // eventually, this will be a stored proc
            //var results = _dbContext.GetStudentQuickSearchResults(input);

            //return results;
            // TODO: FIX
            return null; 
        }

        //public OutputDto_StudentQuickSearch StudentQuickSearch(int id)
        //{

        //    // eventually, this will be a stored proc
        //    var result = _dbContext.Students.First(p => p.Id == id);

        //    return new OutputDto_StudentQuickSearch()
        //    {
        //        StudentId = result.Id,
        //        FirstName = result.FirstName,
        //        LastName = result.LastName
        //    };
        //}



        public OutputDto_GetInterventionGroups GetGroupsByYearSchoolStaff(InputDto_GetInterventionGroups input)
        {
            var movie = _dbContext.InterventionGroups
                .Include(p => p.InterventionType)
                .Where(p => p.SchoolStartYear == input.SchoolYear &&
                            p.SchoolID == input.SchoolId &&
                            p.StaffID == input.StaffId); // change this to StaffSections
            var groups = new List<OuputDto_ManageInterventionGroup>();

            if (!movie.Any())
            {
                return new OutputDto_GetInterventionGroups();
            }
            else
            {
                // convert sections to InterventionGroupDto, Automapper, where are you?

                movie.Each(group => groups.Add(new OuputDto_ManageInterventionGroup()
                {
                    Id = group.Id,
                    Name = group.Name,
                    Description = group.Description,
                    MondayMeet = group.MondayMeet,
                    TuesdayMeet = group.TuesdayMeet,
                    WednesdayMeet = group.WednesdayMeet,
                    ThursdayMeet = group.ThursdayMeet,
                    FridayMeet = group.FridayMeet,
                    StartTime = group.StartTime,
                    EndTime = group.EndTime,
                    Tier = group.InterventionTierID ?? 1,
                    Intervention = new OutputDto_DropdownData()
                    {
                        id = group.InterventionType.Id,
                        text = group.InterventionType.Description + " (" + group.InterventionType.InterventionType + ")"
                    },
                    NumStudents = _dbContext.StudentInterventionGroups.Where(p => p.InterventionGroupId == group.Id).Select(p => p.StudentID).Distinct().Count(),
                    ActiveStudents = _dbContext.StudentInterventionGroups.Where(j => j.InterventionGroupId == group.Id && (j.EndDate == null || j.EndDate > DateTime.Now)).ToList().Count
                }));

                // loop over sections and get studentsections
                //foreach (var section in groups)
                //{
                //    // get the student details for each studentsection
                //    var studentSections = _dbContext.StudentInterventionGroups.Where(p => p.InterventionGroupId == section.Id);

                //    foreach (var studentSection in studentSections)
                //    {
                //        var student = _dbContext.Students.FirstOrDefault(p => p.Id == studentSection.StudentID);

                //        if (student != null)
                //        {
                //            section.StudentInterventionGroups.Add(new InterventionGroupStudentDto()
                //            {
                //                Id = studentSection.Id,
                //                StudentId = student.Id,
                //                FirstName = student.FirstName,
                //                MiddleName = student.MiddleName,
                //                LastName = student.LastName,
                //                StartDate = studentSection.StartDate?.ToString("yyyy-MM-dd") ?? String.Empty,
                //                EndDate = studentSection.EndDate?.ToString("yyyy-MM-dd") ?? String.Empty
                //            });
                //        }
                //        section.StudentInterventionGroups.OrderBy(p => p.LastName).ThenBy(p => p.FirstName);
                //    }
                //}


                groups = groups.OrderBy(p => p.Name).ToList();
            }

            return new OutputDto_GetInterventionGroups()
            {
                 Groups = groups
            };
        }

        public OuputDto_ManageInterventionGroup GetGroup(InputDto_SimpleId input)
        {

            var group = _dbContext.InterventionGroups.FirstOrDefault(p => p.Id == input.Id);

            if (group != null)
            {
                var result = new OuputDto_ManageInterventionGroup()
                {
                    Id = group.Id,
                    Name = group.Name,
                    Description = group.Description,
                    PrimaryTeacherStaffID = group.StaffID,
                    SchoolStartYear = group.SchoolStartYear,
                    SchoolID = group.SchoolID,
                    InterventionTypeID = group.InterventionTypeID,
                    MondayMeet = group.MondayMeet,
                    TuesdayMeet = group.TuesdayMeet,
                    WednesdayMeet = group.WednesdayMeet,
                    ThursdayMeet = group.ThursdayMeet,
                    FridayMeet = group.FridayMeet,
                    StartTime = group.StartTime,
                    EndTime = group.EndTime,
                    Tier = group.InterventionTierID,
                };

                //   all this type of crap should be fixed when EF7 is done
                var primaryTeacher = _dbContext.Staffs.First(p => p.Id == result.PrimaryTeacherStaffID);
                result.PrimaryTeacher = new OutputDto_DropdownData() { id = primaryTeacher.Id, text = primaryTeacher.LastName + ", " + primaryTeacher.FirstName };

                var lstStaffSections = new List<OutputDto_DropdownData>();
                var intervention = _dbContext.Interventions.First(p => p.Id == group.InterventionTypeID);
                result.Intervention = new OutputDto_DropdownData()
                {
                    id = intervention.Id,
                    text =
                                              intervention.Description + " (" + intervention.InterventionType +
                                              ")"
                };

                // get the student details for each studentsection
                var staffSections = _dbContext.StaffInterventionGroups.Include(p => p.Staff).Where(p => p.InterventionGroupId == group.Id && p.Staff.Id != primaryTeacher.Id).ToList();


                result.CoTeachers = Mapper.Map<List<OutputDto_DropdownData>>(staffSections);


                var studentSections = _dbContext.StudentInterventionGroups.Include(p => p.Student).Where(p => p.InterventionGroupId == input.Id);
                result.StudentInterventionGroups = Mapper.Map<List<InterventionGroupStudentDto>>(studentSections);

                return result;
            }
            else
            {
                return  new OuputDto_ManageInterventionGroup(); // set some defaults in constructor???
                                                               // log exception
            }
        }

        public OutputDto_DropdownData GetStaffForDropdown(int id)
        {

            //  eventually, this will be a stored proc
            var result = _dbContext.Staffs.FirstOrDefault(p => p.Id == id);

            if (result == null)
            {
                return new OutputDto_DropdownData();
            }
            else
            {
                return new OutputDto_DropdownData()
                {
                    id = result.Id,
                    text = result.LastName + ", " + result.FirstName
                };
            }
        }

        public OutputDto_DropdownData GetInterventionbyId(int id)
        {

            //  eventually, this will be a stored proc
            var result = _dbContext.Interventions.First(p => p.Id == id);

            return new OutputDto_DropdownData()
            {
                id = result.Id,
                text = result.Description + " (" + result.InterventionType + ")"
            };
        }

        public List<OutputDto_DropdownData> GetInterventionistsForDropdown(int Id, string searchString)
        {

            var teachers = _dbContext.Staffs.Where(p => (p.FirstName.StartsWith(searchString) || p.LastName.StartsWith(searchString)) && p.IsInterventionSpecialist == true).Take(10);

            if (teachers.Any())
            {
                var results = new List<OutputDto_DropdownData>();

                foreach (var teacher in teachers)
                {
                    results.Add(new OutputDto_DropdownData()
                    {
                        id = teacher.Id,
                        text = teacher.LastName + ", " + teacher.FirstName
                    });
                }


                return results;
            }
            else
            {
                return new List<OutputDto_DropdownData>();
                //  log exception
            }
        }

        public List<OutputDto_DropdownData> GetInterventionsForDropdown(string searchString)
        {
            //_dbContext.Interventions.Where(p => p.InterventionType.StartsWith(searchString) || p.Description.StartsWith(searchString));
            var interventions = _dbContext.Interventions.Where(p => p.bDisplay == true);

            if (interventions.Any())
            {
                var results = new List<OutputDto_DropdownData>();

                foreach (var intervention in interventions)
                {
                    results.Add(new OutputDto_DropdownData()
                    {
                        id = intervention.Id,
                        text = intervention.Description + " (" + intervention.InterventionType + ")"
                    });
                }

                return results.OrderBy(p => p.text).ToList();
            }
            else
            {
                return new List<OutputDto_DropdownData>();
                //  log exception
            }
        }

        public List<OutputDto_DropdownData> GetCoInterventionistsForGroup(List<int> ids)
        {

            var staffs = _dbContext.Staffs.Where(p => ids.Contains(p.Id));
            var results = new List<OutputDto_DropdownData>();

            foreach (var staff in staffs)
            {
                if (staff != null)
                {
                    results.Add(new OutputDto_DropdownData()
                    {
                        id = staff.Id,
                        text = staff.LastName + ", " + staff.FirstName
                    });
                }
            }
            return results;
        }

        public IEnumerable<InterventionGroupStudentDto> GetStudentSections(int Id)
        {

            var lstStudentSections = new List<InterventionGroupStudentDto>();

            // get the student details for each studentsection
            var studentSections = _dbContext.StudentInterventionGroups.Where(p => p.InterventionGroupId == Id);
            lstStudentSections = Mapper.Map<List<InterventionGroupStudentDto>>(studentSections);


            return lstStudentSections.OrderBy(p => p.StudentName);
        }

        public OutputDto_GetInterventionGroupStints GetInterventionGroupStints(InputDto_GetInterventionGroupStints input)
        {

            var results = _dbContext.StudentInterventionGroups.Where(p => p.InterventionGroupId == input.InterventionGroupId && p.StudentID == input.StudentId);


            return new OutputDto_GetInterventionGroupStints { Stints = Mapper.Map<List<InterventionGroupStudentDto>>(results.ToList()) };
        }


        public OutputDto_ManageInterventionGroup GetInterventionGroup(InputDto_SimpleId input)
        {
            var group = _dbContext.InterventionGroups
				.Include(p => p.StudentInterventionGroups)
                .First(m => m.Id == input.Id);

            return new OutputDto_ManageInterventionGroup { Group = Mapper.Map<InterventionGroupDto>(group) };
        }


        public OutputDto_Base SaveInterventionGroup(OuputDto_ManageInterventionGroup interventionGroup)
        {
            var result = new OutputDto_Base();

            var db_interventionGroup = _dbContext.InterventionGroups
                .Include(p => p.StaffInterventionGroups)
                .FirstOrDefault(p => p.Id == interventionGroup.Id);
            if (db_interventionGroup == null)
            {
                db_interventionGroup = new InterventionGroup()
                {
                    Description = interventionGroup.Description,
                    StaffID = interventionGroup.PrimaryTeacher.id,
                    FridayMeet = interventionGroup.FridayMeet,
                    ThursdayMeet = interventionGroup.ThursdayMeet,
                    WednesdayMeet = interventionGroup.WednesdayMeet,
                    TuesdayMeet = interventionGroup.TuesdayMeet,
                    MondayMeet = interventionGroup.MondayMeet,
                    StartTime = interventionGroup.StartTime.HasValue ? (interventionGroup.StartTime) : null,
                    EndTime = interventionGroup.EndTime.HasValue ? (interventionGroup.EndTime) : null,
                    IsInterventionGroup = true,
                    Name = interventionGroup.Name,
                    SchoolID = interventionGroup.SchoolID,
                    SchoolStartYear = interventionGroup.SchoolStartYear,
                    InterventionTypeID = interventionGroup.Intervention.id,
                    InterventionTierID = interventionGroup.Tier

                };

                _dbContext.InterventionGroups.Add(db_interventionGroup);
                _dbContext.SaveChanges();
            }
            else
            {
                db_interventionGroup.Name = interventionGroup.Name;
                db_interventionGroup.Description = interventionGroup.Description;
                db_interventionGroup.StaffID = interventionGroup.PrimaryTeacher.id;
                db_interventionGroup.FridayMeet = interventionGroup.FridayMeet;
                db_interventionGroup.ThursdayMeet = interventionGroup.ThursdayMeet;
                db_interventionGroup.WednesdayMeet = interventionGroup.WednesdayMeet;
                db_interventionGroup.TuesdayMeet = interventionGroup.TuesdayMeet;
                db_interventionGroup.MondayMeet = interventionGroup.MondayMeet;
                db_interventionGroup.StartTime = interventionGroup.StartTime.HasValue ? (interventionGroup.StartTime) : null;
                db_interventionGroup.EndTime = interventionGroup.EndTime.HasValue ? (interventionGroup.EndTime) : null;
                db_interventionGroup.IsInterventionGroup = true;
                db_interventionGroup.SchoolID = interventionGroup.SchoolID;
                db_interventionGroup.SchoolStartYear = interventionGroup.SchoolStartYear;
                db_interventionGroup.InterventionTypeID = interventionGroup.Intervention.id;
                db_interventionGroup.InterventionTierID = interventionGroup.Tier;
            }
            #region StaffSections
            var staffInterventionGroupsToDelete = new List<StaffInterventionGroup>();
            db_interventionGroup.StaffInterventionGroups
            .Where(d => d.StaffHierarchyPermissionID == 2 && !interventionGroup.CoTeachers.Any(ct => ct.id == d.StaffID))
            .Each(deleted => staffInterventionGroupsToDelete.Add(deleted));

            _dbContext.StaffInterventionGroups.RemoveRange(staffInterventionGroupsToDelete);


            //update or add staffsections
            interventionGroup.CoTeachers.Each(ct =>
            {
                    // check to see if this is an existing co-teacher record
                    var coTeacher = db_interventionGroup.StaffInterventionGroups.FirstOrDefault(d => d.StaffID == ct.id && d.InterventionGroupId == interventionGroup.Id);
                if (coTeacher == null)
                {
                    coTeacher = new StaffInterventionGroup();
                    db_interventionGroup.StaffInterventionGroups.Add(coTeacher);
                }
                coTeacher.StaffID = ct.id;
                coTeacher.InterventionGroupId = interventionGroup.Id;
                coTeacher.StaffHierarchyPermissionID = 2;
            });
            #endregion

            var db_primaryTeacher = db_interventionGroup.StaffInterventionGroups.FirstOrDefault(p => p.StaffHierarchyPermissionID == 1);
            // fringe case, no primary teacher, just create a new one
            if (db_primaryTeacher == null)
            {
                db_primaryTeacher = new StaffInterventionGroup() { StaffHierarchyPermissionID = 1, InterventionGroupId = db_interventionGroup.Id, StaffID = interventionGroup.PrimaryTeacher.id };
                _dbContext.StaffInterventionGroups.Add(db_primaryTeacher);
            }// new teacher, get rid of old and add new
            else if (db_primaryTeacher.StaffID != interventionGroup.PrimaryTeacher.id)
            {
                _dbContext.StaffInterventionGroups.Remove(db_primaryTeacher);
                db_primaryTeacher = new StaffInterventionGroup() { StaffHierarchyPermissionID = 1, InterventionGroupId = db_interventionGroup.Id, StaffID = interventionGroup.PrimaryTeacher.id };
                _dbContext.StaffInterventionGroups.Add(db_primaryTeacher);
            }
            _dbContext.SaveChanges();


            #region StudentInterventionGroups
            var studentInterventionGroupsToDelete = new List<StudentInterventionGroup>();
            db_interventionGroup.StudentInterventionGroups
            .Where(d => !interventionGroup.StudentInterventionGroups.Any(ct => ct.Id == d.Id))
            .Each(deleted => studentInterventionGroupsToDelete.Add(deleted));

            var startEndDatesIdsToDelete = studentInterventionGroupsToDelete.Select(p => p.Id).ToList();

            // make sure these are cool to delete
            //if (_dbContext.InterventionAttendances.Any(p => startEndDatesIdsToDelete.Contains(p.ClassStartEndID.HasValue ? p.ClassStartEndID.Value : -1)))
            //{
            //    result.Status.StatusCode = StatusCode.UserDisplayableException;
            //    result.Status.StatusMessage = "A stint you are trying to remove has attendance information assigned to it and cannot be deleted.  Please refresh the page to reload this intervention group.";
            //    return result;
            //}

            // remove any attendance associated with these stints
            var attendanceToDelete = _dbContext.InterventionAttendances.Where(p => startEndDatesIdsToDelete.Contains(p.ClassStartEndID.HasValue ? p.ClassStartEndID.Value : -1));
            _dbContext.InterventionAttendances.RemoveRange(attendanceToDelete);

            _dbContext.StudentInterventionGroups.RemoveRange(studentInterventionGroupsToDelete);

            // validate date overlapping
            var uniqueStudentIds = interventionGroup.StudentInterventionGroups.Select(p => p.StudentId).Distinct();
            string validationMessage = null;

            foreach (var studentId in uniqueStudentIds)
            {
                // also check updated stints
                foreach (var updatedStint in interventionGroup.StudentInterventionGroups.Where(p => p.StudentId == studentId))
                {
                    // check this one against all others
                    validationMessage = validateUpdatedStint(interventionGroup.StudentInterventionGroups.Where(p => p != updatedStint && p.StudentId == studentId).ToList(), updatedStint);
                    if (validationMessage != null)
                    {
                        result.Status.StatusCode = StatusCode.UserDisplayableException;
                        result.Status.StatusMessage = validationMessage;
                        return result;
                    }
                }
            }

            // now that we've validated that we're good to go, let's save any new stints
            interventionGroup.StudentInterventionGroups.Where(p => p.Id == -1)
                .Each(newstint => _dbContext.StudentInterventionGroups.Add(new StudentInterventionGroup() { InterventionGroupId = db_interventionGroup.Id, StartDate = newstint.StartDate, EndDate = newstint.EndDate, StudentID = newstint.StudentId }));

            // now let's update any existing groups
            interventionGroup.StudentInterventionGroups.Where(p => p.Id != -1)
               .Each(updated => UpdateStint(updated));
            #endregion

            _dbContext.SaveChanges();

            return result;
        }

        public OutputDto_SuccessAndStatus CanStintBeDeleted(int stintId)
        {
            var result = new OutputDto_SuccessAndStatus();
            result.isValid = true;

            // make sure these are cool to delete
            if (_dbContext.InterventionAttendances.Any(p => stintId == (p.ClassStartEndID.HasValue ? p.ClassStartEndID.Value : -9999)))
            {
                result.isValid = false;
                return result;
            }
            return result;
        }

        private void UpdateStint(InterventionGroupStudentDto updatedStint)
        {
            var db_updated = _dbContext.StudentInterventionGroups.FirstOrDefault(p => p.Id == updatedStint.Id);

            if(db_updated != null)
            {
                db_updated.StartDate = updatedStint.StartDate;
                db_updated.EndDate = updatedStint.EndDate;
            }
            // TODO: if it's not there, do we care? should throw exception?
        }

        private string validateUpdatedStint(List<InterventionGroupStudentDto> existingStudentStints, InterventionGroupStudentDto newStint) {
            string validationMessage = null;

            // make sure only one open ended stint
            if (newStint.EndDate == null) {
                for (var i = 0; i < existingStudentStints.Count; i++) {
                    if (existingStudentStints[i].EndDate == null) {
                        validationMessage = "Only one stint can be open ended.";
                        break;
                    }

                    // make sure start date is later than last end date
                    if (newStint.StartDate < existingStudentStints[i].EndDate) {
                        validationMessage = "Start date of an open ended stint must be after the end date of all other stints.";
                    }
                }
            } else {
                // both start and end are filled in, make sure start is either before any ohter start date and ends before or is after any other end date and ends after
                for (var i = 0; i < existingStudentStints.Count; i++) {
                    if (newStint.StartDate <= existingStudentStints[i].StartDate && newStint.EndDate >= existingStudentStints[i].StartDate) {
                        validationMessage = "New stint must end before the start of any other stints.";
                        break;
                    } else if (newStint.StartDate <= existingStudentStints[i].EndDate && newStint.EndDate >= existingStudentStints[i].EndDate) {
                        validationMessage = "Cannot start a new stint during another stint.";
                        break;
                    } else if (newStint.StartDate >= existingStudentStints[i].StartDate && newStint.EndDate <= existingStudentStints[i].EndDate) {
                        validationMessage = "A new stint cannot take place during an existing stint.";
                        break;
                    } else if (newStint.StartDate <= existingStudentStints[i].StartDate && newStint.EndDate >= existingStudentStints[i].EndDate) {
                        validationMessage = "A new stint cannot overlap an existing stint.";
                        break;
                    }
                }
            }

            return validationMessage;
        }

        public OutputDto_Base Delete(int id)
        {
            var result = new OutputDto_Base();

            try
            {
                // TODO: check intervention attendance and DB tables

                var group = _dbContext.InterventionGroups.FirstOrDefault(m => m.Id == id);

                // remove staff
                var staffIgs = _dbContext.StaffInterventionGroups.Where(p => p.InterventionGroupId == id);
                _dbContext.StaffInterventionGroups.RemoveRange(staffIgs);

                // remove students
                var studentIgs = _dbContext.StudentInterventionGroups.Where(p => p.InterventionGroupId == id);
                _dbContext.StudentInterventionGroups.RemoveRange(studentIgs);

                _dbContext.InterventionGroups.Remove(group);
                _dbContext.SaveChanges();
            }
            catch(Exception ex)
            {
                // TODO: log this
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "This intervention group is in use and cannot be deleted.";
            }
            return result;
        }
    }
}
