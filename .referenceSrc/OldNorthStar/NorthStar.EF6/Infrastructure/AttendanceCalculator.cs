using EntityDto.Entity;
using NorthStar.EF6;
using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Threading.Tasks;
using EntityDto.DTO.Admin.InterventionGroup;

namespace NorthStar.EF6.Infrastructure
{
    public class AttendanceCalculator
    {
        private DistrictContext repository = null;
        private List<DistrictCalendar> DistrictCalendars = new List<DistrictCalendar>();
        private Dictionary<int, List<SchoolCalendar>> SchoolCalendars = new Dictionary<int, List<SchoolCalendar>>();
        bool districtHolidaysAdded = false;


        public AttendanceCalculator(DistrictContext repo)
        {
            this.repository = repo;
        }

        public int GetMinutesDelivered(int sessionsMet, DateTime startTime, DateTime endTime)
        {
            // normalize the dates
            var currentDate = DateTime.Now;
            var currentStartDate = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, startTime.Hour, startTime.Minute, 0);
            var currentEndDate = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, endTime.Hour, endTime.Minute, 0);


            var minutesBetween = currentEndDate.Subtract(currentStartDate);
            //var sessionsMet = GetSessionsMet(sectionId, studentId, schoolId, districtId);

            return (int)minutesBetween.TotalMinutes * sessionsMet;
        }

        public int GetSessionsPossible(int sectionId, int studentId, int schoolId, int districtId)
        {
            AddHolidays(schoolId);

            StudentInterventionGroup studentClass =
                repository.StudentInterventionGroups.First(p => p.StudentID == studentId && p.InterventionGroupId == sectionId);

            InterventionGroup section = studentClass.InterventionGroup;

            DateTime startDate = studentClass.StartDate;
            DateTime endDate = studentClass.EndDate.HasValue ? (studentClass.EndDate.Value < DateTime.Now ? studentClass.EndDate.Value : DateTime.Now) : DateTime.Now;

            List<DateTime> datesToProcess = CreateDateTimeList(startDate, endDate);

            int runningTotal = 0;
            List<InterventionAttendance> interventionAttendance =
                repository.InterventionAttendances.Where(
                    p => p.SectionID == sectionId && p.StudentID == studentId).ToList();


            foreach (var dateTime in datesToProcess)
            {
                // this variable ensures we don't "double count" a day we are supposed to meet and an inadvertent bonus day (Make Up Lesson)
                bool alreadyUpdated = false;
                bool alreadyRemoved = false;

                if (dateTime.DayOfWeek != DayOfWeek.Saturday && dateTime.DayOfWeek != DayOfWeek.Sunday)
                {
                    // check the daysmet
                    switch (dateTime.DayOfWeek)
                    {
                        case DayOfWeek.Monday:
                            if (section.MondayMeet.HasValue && section.MondayMeet.Value == true)
                            {
                                runningTotal++;
                                alreadyUpdated = true;
                            }
                            break;
                        case DayOfWeek.Tuesday:
                            if (section.TuesdayMeet.HasValue && section.TuesdayMeet.Value == true)
                            {
                                runningTotal++;
                                alreadyUpdated = true;
                            }
                            break;
                        case DayOfWeek.Wednesday:
                            if (section.WednesdayMeet.HasValue && section.WednesdayMeet.Value == true)
                            {
                                runningTotal++;
                                alreadyUpdated = true;
                            }
                            break;
                        case DayOfWeek.Thursday:
                            if (section.ThursdayMeet.HasValue && section.ThursdayMeet.Value == true)
                            {
                                runningTotal++;
                                alreadyUpdated = true;
                            }
                            break;
                        case DayOfWeek.Friday:
                            if (section.FridayMeet.HasValue && section.FridayMeet.Value == true)
                            {
                                runningTotal++;
                                alreadyUpdated = true;
                            }
                            break;
                    }

                    if (!alreadyUpdated)
                    {
                        var attendanceBonusDay =
                            interventionAttendance.FirstOrDefault(
                                p => p.AttendanceDate == dateTime && p.AttendanceReason.CountsAsBonusDay);

                        if (attendanceBonusDay != null)
                        {
                            runningTotal++;
                        }
                    }

                    // only subtract holidays if > 0 already and stop at zero
                    // see if this date falls on a district holiday
                    DistrictCalendars.ForEach(p =>
                    {
                        if (dateTime >= p.Start && dateTime <= p.End && !alreadyRemoved)
                        {
                            if (runningTotal > 0)
                            {
                                runningTotal--;
                                alreadyRemoved = true;
                            }
                        }
                    });
                    // see if this date falls on a school holiday
                    SchoolCalendars[schoolId].ForEach(p =>
                    {
                        if (dateTime >= p.Start && dateTime <= p.End && !alreadyRemoved)
                        {
                            if (runningTotal > 0)
                            {
                                runningTotal--;
                            }
                        }
                    });
                }
            }

            return runningTotal;
        }

        public int GetSessionsMissed(int stintId, int studentId)
        {

            StudentInterventionGroup studentClass =
                repository.StudentInterventionGroups.First(p => p.StudentID == studentId && p.Id == stintId);

            InterventionGroup section = studentClass.InterventionGroup;

            var schoolId = section.SchoolID;
            AddHolidays(schoolId);


            DateTime startDate = studentClass.StartDate;
            DateTime endDate = studentClass.EndDate.HasValue ? (studentClass.EndDate.Value < DateTime.Now ? studentClass.EndDate.Value : DateTime.Now) : DateTime.Now;

            List<DateTime> datesToProcess = CreateDateTimeList(startDate, endDate);

            int runningTotal = 0;
            List<InterventionAttendance> interventionAttendance =
                repository.InterventionAttendances
                .Include(p => p.AttendanceReason)
                .Where(p => p.SectionID == section.Id && p.StudentID == studentId).ToList();


            foreach (var dateTime in datesToProcess)
            {
                // this variable ensures we don't "double count" a day we are supposed to meet and an inadvertent bonus day (Make Up Lesson)
                bool alreadyUpdated = false;
                bool alreadyRemoved = false;

                if (dateTime.DayOfWeek != DayOfWeek.Saturday && dateTime.DayOfWeek != DayOfWeek.Sunday)
                {
                    // check the daysmet
                    switch (dateTime.DayOfWeek)
                    {
                        case DayOfWeek.Monday:
                            if (section.MondayMeet.HasValue && section.MondayMeet.Value == true)
                            {
                                var standardAttendance =
                            interventionAttendance.FirstOrDefault(
                                p => p.AttendanceDate == dateTime);

                                // if this is a standard meeting date with no status, an intervention delivered day or make up lesson day
                                if (standardAttendance != null && standardAttendance.AttendanceReason.CountsAsAbsense)
                                {
                                    runningTotal++;
                                    alreadyUpdated = true;
                                }
                            }
                            break;
                        case DayOfWeek.Tuesday:
                            if (section.TuesdayMeet.HasValue && section.TuesdayMeet.Value == true)
                            {
                                var standardAttendance =
                            interventionAttendance.FirstOrDefault(
                                p => p.AttendanceDate == dateTime);

                                // if this is a standard meeting date with no status, an intervention delivered day or make up lesson day
                                if (standardAttendance != null && standardAttendance.AttendanceReason.CountsAsAbsense)
                                {
                                    runningTotal++;
                                    alreadyUpdated = true;
                                }
                            }
                            break;
                        case DayOfWeek.Wednesday:
                            if (section.WednesdayMeet.HasValue && section.WednesdayMeet.Value == true)
                            {
                                var standardAttendance =
                            interventionAttendance.FirstOrDefault(
                                p => p.AttendanceDate == dateTime);

                                // if this is a standard meeting date with no status, an intervention delivered day or make up lesson day
                                if (standardAttendance != null && standardAttendance.AttendanceReason.CountsAsAbsense)
                                {
                                    runningTotal++;
                                    alreadyUpdated = true;
                                }
                            }
                            break;
                        case DayOfWeek.Thursday:
                            if (section.ThursdayMeet.HasValue && section.ThursdayMeet.Value == true)
                            {
                                var standardAttendance =
                            interventionAttendance.FirstOrDefault(
                                p => p.AttendanceDate == dateTime);

                                // if this is a standard meeting date with no status, an intervention delivered day or make up lesson day
                                if (standardAttendance != null && standardAttendance.AttendanceReason.CountsAsAbsense)
                                {
                                    runningTotal++;
                                    alreadyUpdated = true;
                                }
                            }
                            break;
                        case DayOfWeek.Friday:
                            if (section.FridayMeet.HasValue && section.FridayMeet.Value == true)
                            {
                                var standardAttendance =
                            interventionAttendance.FirstOrDefault(
                                p => p.AttendanceDate == dateTime);

                                // if this is a standard meeting date with no status, an intervention delivered day or make up lesson day
                                if (standardAttendance != null && standardAttendance.AttendanceReason.CountsAsAbsense)
                                {
                                    runningTotal++;
                                    alreadyUpdated = true;
                                }
                            }
                            break;
                    }

                    if (!alreadyUpdated)
                    {
                        var attendanceBonusDay =
                            interventionAttendance.FirstOrDefault(
                                p => p.AttendanceDate == dateTime && p.AttendanceReason.CountsAsAbsense);

                        if (attendanceBonusDay != null)
                        {
                            runningTotal++;
                        }
                    }


                    // only subtract holidays if > 0 already and stop at zero
                    // see if this date falls on a district holiday
                    DistrictCalendars.ForEach(p =>
                    {
                        if (dateTime >= p.Start && dateTime <= p.End && !alreadyRemoved)
                        {
                            // make SURE that this was a date that was on the list!! BUG FIX on 11/18/2013
                            var dateThatCountedButShouldBeRemoved = interventionAttendance.FirstOrDefault(
                                j => j.AttendanceDate == dateTime && j.AttendanceReason.CountsAsAbsense);

                            if (dateThatCountedButShouldBeRemoved != null && runningTotal > 0)
                            {
                                runningTotal--;
                                alreadyRemoved = true;
                            }
                        }
                    });
                    // see if this date falls on a school holiday
                    SchoolCalendars[schoolId].ForEach(p =>
                    {
                        if (dateTime >= p.Start && dateTime <= p.End && !alreadyRemoved)
                        {
                            // make SURE that this was a date that was on the list!! BUG FIX on 11/18/2013
                            var dateThatCountedButShouldBeRemoved = interventionAttendance.FirstOrDefault(
                                j => j.AttendanceDate == dateTime && j.AttendanceReason.CountsAsAbsense);

                            if (dateThatCountedButShouldBeRemoved != null && runningTotal > 0)
                            {
                                runningTotal--;
                            }
                        }
                    });
                }
            }

            return runningTotal;
        }

        public AttendanceExportInfo CreateNewAttendanceRecord(AttendanceExportInfo studentGroup, InterventionAttendance source)
        {
            var newRecord = new AttendanceExportInfo();
            newRecord.AttendanceDate = source.AttendanceDate;
            newRecord.AttendanceReason = source.AttendanceReason.Reason;
            newRecord.GroupEndTime = studentGroup.GroupEndTime;
            newRecord.GroupName = studentGroup.GroupName;
            newRecord.GroupStartTime = studentGroup.GroupStartTime;
            newRecord.InterventionEnd = studentGroup.InterventionEnd;
            newRecord.InterventionGroupId = studentGroup.InterventionGroupId;
            newRecord.Interventionist = studentGroup.Interventionist;
            newRecord.InterventionStart = studentGroup.InterventionStart;
            newRecord.SchoolID = studentGroup.SchoolID;
            newRecord.SchoolName = studentGroup.SchoolName;
            newRecord.StartEndDateID = studentGroup.StartEndDateID;
            newRecord.Student = studentGroup.Student;
            newRecord.StudentNumber = studentGroup.StudentNumber;
            newRecord.StudentID = studentGroup.StudentID;
            newRecord.YearVerbose = studentGroup.YearVerbose;
            newRecord.InterventionType = studentGroup.InterventionType;
            newRecord.Comment = source.Notes;

            return newRecord;
        }

        public AttendanceExportInfo CreateNewInterventionDeliveredRecord(AttendanceExportInfo studentGroup, DateTime date)
        {
            var newRecord = new AttendanceExportInfo();
            newRecord.AttendanceDate = date;
            newRecord.AttendanceReason = "Intervention Delivered";
            newRecord.GroupEndTime = studentGroup.GroupEndTime;
            newRecord.GroupName = studentGroup.GroupName;
            newRecord.GroupStartTime = studentGroup.GroupStartTime;
            newRecord.InterventionEnd = studentGroup.InterventionEnd;
            newRecord.InterventionGroupId = studentGroup.InterventionGroupId;
            newRecord.Interventionist = studentGroup.Interventionist;
            newRecord.InterventionStart = studentGroup.InterventionStart;
            newRecord.SchoolID = studentGroup.SchoolID;
            newRecord.SchoolName = studentGroup.SchoolName;
            newRecord.StartEndDateID = studentGroup.StartEndDateID;
            newRecord.Student = studentGroup.Student;
            newRecord.StudentNumber = studentGroup.StudentNumber;
            newRecord.StudentID = studentGroup.StudentID;
            newRecord.YearVerbose = studentGroup.YearVerbose;
            newRecord.InterventionType = studentGroup.InterventionType;

            return newRecord;
        }


        public List<AttendanceExportInfo> GetAttendanceDetail(List<AttendanceExportInfo> distinctStudentGroups, List<InterventionAttendance> existingAttendanceCache)
        {
            var finalExportList = new List<AttendanceExportInfo>();

            foreach(var studentGroup in distinctStudentGroups)
            {
                List<DateTime> datesToProcess = CreateDateTimeList(studentGroup.InterventionStart, studentGroup.InterventionEnd.HasValue ? (studentGroup.InterventionEnd.Value < DateTime.Now ? studentGroup.InterventionEnd.Value : DateTime.Now) : DateTime.Now);
                var group = repository.InterventionGroups.FirstOrDefault(p => p.Id == studentGroup.InterventionGroupId); // all groups must exist

                if(group == null)
                {
                    // LOG null group
                    continue;
                }

                foreach (var dateTime in datesToProcess)
                {
                    var existingAttendance = existingAttendanceCache.FirstOrDefault(p => p.ClassStartEndID == studentGroup.StartEndDateID && p.AttendanceDate.Date == dateTime.Date);
                    // check and see if this date is already explictly in the  existing attendance, if so, we don't need to do anything
                    if (existingAttendance != null)
                    {
                        finalExportList.Add(CreateNewAttendanceRecord(studentGroup, existingAttendance));
                        continue;
                    }


                    // we only need to add InterventionDelivered

                    bool alreadyProcessed = false;
                    if (dateTime.DayOfWeek != DayOfWeek.Saturday && dateTime.DayOfWeek != DayOfWeek.Sunday)
                    {
                        // check the daysmet
                        switch (dateTime.DayOfWeek)
                        {
                            case DayOfWeek.Monday:
                                if (group.MondayMeet.HasValue && group.MondayMeet.Value == true)
                                {
                                    finalExportList.Add(CreateNewInterventionDeliveredRecord(studentGroup, dateTime));
                                }
                                break;
                            case DayOfWeek.Tuesday:
                                if (group.TuesdayMeet.HasValue && group.TuesdayMeet.Value == true)
                                {
                                    finalExportList.Add(CreateNewInterventionDeliveredRecord(studentGroup, dateTime));
                                }
                                break;
                            case DayOfWeek.Wednesday:
                                if (group.WednesdayMeet.HasValue && group.WednesdayMeet.Value == true)
                                {
                                    finalExportList.Add(CreateNewInterventionDeliveredRecord(studentGroup, dateTime));
                                }
                                break;
                            case DayOfWeek.Thursday:
                                if (group.ThursdayMeet.HasValue && group.ThursdayMeet.Value == true)
                                {
                                    finalExportList.Add(CreateNewInterventionDeliveredRecord(studentGroup, dateTime));
                                }
                                break;
                            case DayOfWeek.Friday:
                                if (group.FridayMeet.HasValue && group.FridayMeet.Value == true)
                                {
                                    finalExportList.Add(CreateNewInterventionDeliveredRecord(studentGroup, dateTime));
                                }
                                break;
                        }
                    }
                }
            }

            return finalExportList;
        }

        public void SetAttendanceStatuses(List<AttendanceReason> reasons, List<AttendanceStatusSummary> statusSummaries, List<InterventionAttendance> attendanceData, int studentId, int stintId)
        {
            StudentInterventionGroup studentClass =
                   repository.StudentInterventionGroups.Include(p => p.InterventionGroup).First(p => p.StudentID == studentId && p.Id == stintId);

            InterventionGroup section = studentClass.InterventionGroup;
            var schoolId = section.SchoolID;

            AddHolidays(schoolId);

            // only count attendance through today... no extra days in the future
            DateTime startDate = studentClass.StartDate;
            DateTime endDate = studentClass.EndDate.HasValue ? (studentClass.EndDate.Value < DateTime.Now ? studentClass.EndDate.Value : DateTime.Now) : DateTime.Now;

            List<DateTime> datesToProcess = CreateDateTimeList(startDate, endDate);

            foreach (var reason in reasons)
            {
                // get the corresponding reason and use that
                var currentStatusSummary = statusSummaries.First(p => p.StatusLabel == reason.Reason);

                var totalDays = 0;

                foreach (var dateTime in datesToProcess)
                {
                    bool alreadyProcessed = false;
                    if (dateTime.DayOfWeek != DayOfWeek.Saturday && dateTime.DayOfWeek != DayOfWeek.Sunday)
                    {
                        // check the daysmet
                        switch (dateTime.DayOfWeek)
                        {
                            case DayOfWeek.Monday:
                                if (section.MondayMeet.HasValue && section.MondayMeet.Value == true)
                                {
                                    ProcessMeetingDate(dateTime, ref totalDays, schoolId, attendanceData, reason);
                                    alreadyProcessed = true;
                                }
                                break;
                            case DayOfWeek.Tuesday:
                                if (section.TuesdayMeet.HasValue && section.TuesdayMeet.Value == true)
                                {
                                    ProcessMeetingDate(dateTime, ref totalDays, schoolId, attendanceData, reason);
                                    alreadyProcessed = true;
                                }
                                break;
                            case DayOfWeek.Wednesday:
                                if (section.WednesdayMeet.HasValue && section.WednesdayMeet.Value == true)
                                {
                                    ProcessMeetingDate(dateTime, ref totalDays, schoolId, attendanceData, reason);
                                    alreadyProcessed = true;
                                }
                                break;
                            case DayOfWeek.Thursday:
                                if (section.ThursdayMeet.HasValue && section.ThursdayMeet.Value == true)
                                {
                                    ProcessMeetingDate(dateTime, ref totalDays, schoolId, attendanceData, reason);
                                    alreadyProcessed = true;
                                }
                                break;
                            case DayOfWeek.Friday:
                                if (section.FridayMeet.HasValue && section.FridayMeet.Value == true)
                                {
                                    ProcessMeetingDate(dateTime, ref totalDays, schoolId, attendanceData, reason);
                                    alreadyProcessed = true;
                                }
                                break;
                        }

                        if (!alreadyProcessed)
                        {
                            ProcessNonMeetingDate(dateTime, ref totalDays, schoolId, attendanceData, reason);
                        }
                    }

                }
                currentStatusSummary.Count = totalDays;

            }
        }

        private void ProcessNonMeetingDate(DateTime dateTime, ref int totalDays, int schoolId, List<InterventionAttendance> attendanceData, AttendanceReason reason)
        {
            // check if this date is a school or district holiday, if so, skip it altogether
            foreach (var districtCalendar in DistrictCalendars)
            {
                if (dateTime >= districtCalendar.Start && dateTime < districtCalendar.End)
                {
                    // supposed to meet this day, but it's a holiday... don't care what else may be stored
                    return;
                }
            }


            // see if this date falls on a school holiday
            foreach (var schoolCalendar in SchoolCalendars[schoolId])
            {

                if (dateTime >= schoolCalendar.Start && dateTime < schoolCalendar.End)
                {
                    // supposed to meet this day, but it's a holiday
                    return;
                }
            }

            // also check to see if this is a non-cycle day
            var nonCycleCheck = attendanceData.FirstOrDefault(p => p.AttendanceDate == dateTime && p.AttendanceReason.Reason == "Non-Cycle Day");
            if (nonCycleCheck != null)
            {
                // if we are on a non-cycle day for this date, skip it in all calculations
                return;
            }

            var currentStatusDate = attendanceData.FirstOrDefault(p => p.AttendanceDate == dateTime);

            switch (reason.Reason)
            {
                case "Intervention Delivered":
                case "Make-Up Lesson":
                    if (currentStatusDate != null && currentStatusDate.AttendanceReason.Reason == reason.Reason)
                    {
                        totalDays++;
                    }
                    break;
            }

        }

        private void ProcessMeetingDate(DateTime dateTime, ref int totalDays, int schoolId, List<InterventionAttendance> attendanceData, AttendanceReason reason)
        {
            // check if this date is a school or district holiday, if so, skip it altogether
            foreach (var districtCalendar in DistrictCalendars)
            {
                if (dateTime >= districtCalendar.Start && dateTime < districtCalendar.End)
                {
                    // supposed to meet this day, but it's a holiday... don't care what else may be stored
                    return;
                }
            }


            // see if this date falls on a school holiday
            foreach (var schoolCalendar in SchoolCalendars[schoolId])
            {

                if (dateTime >= schoolCalendar.Start && dateTime < schoolCalendar.End)
                {
                    // supposed to meet this day, but it's a holiday
                    return;
                }
            }

            // also check to see if this is a non-cycle day
            var nonCycleCheck = attendanceData.FirstOrDefault(p => p.AttendanceDate == dateTime && p.AttendanceReason.Reason == "Non-Cycle Day");
            if(nonCycleCheck != null)
            {
                // if we are on a non-cycle day for this date, skip it in all calculations
                return;
            }

            var currentStatusDate = attendanceData.FirstOrDefault(p => p.AttendanceDate == dateTime);

            switch (reason.Reason)
            {
                case "Intervention Delivered":
                    // if nothing is recored, by default, add an auto day
                    //if (currentStatusDate == null)
                    //{
                    // IF there isn't already something else on that day
                    if (currentStatusDate == null || (currentStatusDate != null && currentStatusDate.AttendanceReason.Reason == "Intervention Delivered"))
                        totalDays++;
                    //}
                    //else if(currentStatusDate.AttendanceReason.CountsAsBonusDay)
                    //{
                    //    // otherwise, add a day if any date that counts as bonus day is set
                    //    totalDays++;
                    //}
                    break;
                case "Make-Up Lesson":
                case "Child Absent":
                case "Teacher Absent":
                case "Teacher Unavailable":
                case "Child Unavailable":
                case "No School":
                    if(currentStatusDate != null && currentStatusDate.AttendanceReason.Reason == reason.Reason)
                    {
                        totalDays++;
                    }
                    break;
            }

        }
        //public void UpdateDefaultSessionsMet(int districtId, short schoolYear)
        //{
        //    // get all intervention groups in the district for this school year
        //    var sections =
        //        repository.Sections.Where(
        //            p => p.SchoolStartYear == schoolYear && p.IsInterventionGroup && p.School.DistrictID == districtId).ToList();

        //    for (int i = 0; i < sections.Count; i++)
        //    {
        //        var section = sections[i];
        //        var schoolId = section.SchoolID;

        //        AddHolidays(schoolId, districtId);

        //        var studentClasses = section.StudentClasses.ToList();

        //        // skip the group if its one of the ones where they dumped the whole school to the group
        //        if (studentClasses.Count < 20)
        //        {
        //            for (int j = 0; j < studentClasses.Count; j++)
        //            {
        //                var studentClass = studentClasses[j];
        //                DateTime startDate = studentClass.StartDate.Value;
        //                DateTime endDate = studentClass.EndDate.HasValue
        //                    ? (studentClass.EndDate.Value < DateTime.Now ? studentClass.EndDate.Value : DateTime.Now)
        //                    : DateTime.Now;

        //                // get a list of all the individual dates between the start and end date... won't need all of them
        //                List<DateTime> datesToProcess = CreateDateTimeList(startDate, endDate);
        //                List<InterventionAttendance> interventionAttendance =
        //                    repository.Context.InterventionAttendances.Where(
        //                        p => p.ClassStartEndID == studentClass.ID).ToList();

        //                foreach (var dateTime in datesToProcess)
        //                {
        //                    // this variable ensures we don't "double count" a day we are supposed to meet and an inadvertent bonus day (Make Up Lesson)
        //                    bool dateIsNotHoliday = true;

        //                    // these are the dates WE HAVE ACTUALLY RECORDED
        //                    // don't process if this date is a school or district holiday
        //                    // only subtract holidays if > 0 already and stop at zero
        //                    // see if this date falls on a district holiday
        //                    DistrictCalendars.ForEach(p =>
        //                    {
        //                        if (dateTime >= p.Start && dateTime <= p.End)
        //                        {
        //                            dateIsNotHoliday = false;
        //                        }
        //                    });
        //                    // see if this date falls on a school holiday
        //                    SchoolCalendars[schoolId].ForEach(p =>
        //                    {
        //                        if (dateTime >= p.Start && dateTime <= p.End)
        //                        {
        //                            dateIsNotHoliday = false;
        //                        }
        //                    });

        //                    if (dateTime.DayOfWeek != DayOfWeek.Saturday && dateTime.DayOfWeek != DayOfWeek.Sunday && dateIsNotHoliday)
        //                    {

        //                        var standardAttendance =
        //                            interventionAttendance.FirstOrDefault(
        //                                p => p.AttendanceDate == dateTime);

        //                        if (standardAttendance == null)
        //                        {
        //                            // check the daysmet
        //                            switch (dateTime.DayOfWeek)
        //                            {
        //                                // if we are supposed to meet on this day, there should be some kind of status, if there isn't, add it
        //                                case DayOfWeek.Monday:
        //                                    if (section.MondayMeet)
        //                                    {
        //                                        // if this is a standard meeting date with no status, add a status
        //                                        repository.Context.bulk_insert_InterventionAttendance(dateTime, 10,
        //                                            studentClass.ID, "Auto Added", section.ID, section.StaffID,
        //                                            studentClass.StudentID);
        //                                        //repository.Context.InterventionAttendances.Add(
        //                                        //    new InterventionAttendance()
        //                                        //    {
        //                                        //        AttendanceDate = dateTime,
        //                                        //        AttendanceReasonID = 10,
        //                                        //        ClassStartEndID = studentClass.ID,
        //                                        //        Notes = "Auto Added",
        //                                        //        SectionID = section.ID,
        //                                        //        RecorderID = section.StaffID,
        //                                        //        StudentID = studentClass.StudentID
        //                                        //    });
        //                                    }
        //                                    break;
        //                                case DayOfWeek.Tuesday:
        //                                    if (section.TuesdayMeet)
        //                                    {
        //                                        // if this is a standard meeting date with no status, add a status
        //                                        repository.Context.bulk_insert_InterventionAttendance(dateTime, 10,
        //                                            studentClass.ID, "Auto Added", section.ID, section.StaffID,
        //                                            studentClass.StudentID);
        //                                        //repository.Context.InterventionAttendances.Add(
        //                                        //    new InterventionAttendance()
        //                                        //    {
        //                                        //        AttendanceDate = dateTime,
        //                                        //        AttendanceReasonID = 10,
        //                                        //        ClassStartEndID = studentClass.ID,
        //                                        //        Notes = "Auto Added",
        //                                        //        SectionID = section.ID,
        //                                        //        RecorderID = section.StaffID,
        //                                        //        StudentID = studentClass.StudentID
        //                                        //    });
        //                                    }
        //                                    break;
        //                                case DayOfWeek.Wednesday:
        //                                    if (section.WednesdayMeet)
        //                                    {
        //                                        // if this is a standard meeting date with no status, add a status
        //                                        repository.Context.bulk_insert_InterventionAttendance(dateTime, 10,
        //                                            studentClass.ID, "Auto Added", section.ID, section.StaffID,
        //                                            studentClass.StudentID);
        //                                        //repository.Context.InterventionAttendances.Add(
        //                                        //    new InterventionAttendance()
        //                                        //    {
        //                                        //        AttendanceDate = dateTime,
        //                                        //        AttendanceReasonID = 10,
        //                                        //        ClassStartEndID = studentClass.ID,
        //                                        //        Notes = "Auto Added",
        //                                        //        SectionID = section.ID,
        //                                        //        RecorderID = section.StaffID,
        //                                        //        StudentID = studentClass.StudentID
        //                                        //    });
        //                                    }
        //                                    break;
        //                                case DayOfWeek.Thursday:
        //                                    if (section.ThursdayMeet)
        //                                    {
        //                                        // if this is a standard meeting date with no status, add a status
        //                                        repository.Context.bulk_insert_InterventionAttendance(dateTime, 10,
        //                                            studentClass.ID, "Auto Added", section.ID, section.StaffID,
        //                                            studentClass.StudentID);
        //                                        //repository.Context.InterventionAttendances.Add(
        //                                        //    new InterventionAttendance()
        //                                        //    {
        //                                        //        AttendanceDate = dateTime,
        //                                        //        AttendanceReasonID = 10,
        //                                        //        ClassStartEndID = studentClass.ID,
        //                                        //        Notes = "Auto Added",
        //                                        //        SectionID = section.ID,
        //                                        //        RecorderID = section.StaffID,
        //                                        //        StudentID = studentClass.StudentID
        //                                        //    });
        //                                    }
        //                                    break;
        //                                case DayOfWeek.Friday:
        //                                    if (section.FridayMeet)
        //                                    {
        //                                        // if this is a standard meeting date with no status, add a status
        //                                        repository.Context.bulk_insert_InterventionAttendance(dateTime, 10,
        //                                            studentClass.ID, "Auto Added", section.ID, section.StaffID,
        //                                            studentClass.StudentID);
        //                                        //repository.Context.InterventionAttendances.Add(
        //                                        //    new InterventionAttendance()
        //                                        //    {
        //                                        //        AttendanceDate = dateTime,
        //                                        //        AttendanceReasonID = 10,
        //                                        //        ClassStartEndID = studentClass.ID,
        //                                        //        Notes = "Auto Added",
        //                                        //        SectionID = section.ID,
        //                                        //        RecorderID = section.StaffID,
        //                                        //        StudentID = studentClass.StudentID
        //                                        //    });
        //                                    }
        //                                    break;
        //                            }
        //                        }
        //                    }
        //                }
        //                // repository.Context.SaveChanges();
        //            }
        //        }

        //    }



        //}

        public int GetSessionsMet(int stintId, int studentId)
        {

            StudentInterventionGroup studentClass =
                repository.StudentInterventionGroups.Include(p => p.InterventionGroup).First(p => p.StudentID == studentId && p.Id == stintId);

            InterventionGroup section = studentClass.InterventionGroup;
            var schoolId = section.SchoolID;

            AddHolidays(schoolId);


            DateTime startDate = studentClass.StartDate;
            DateTime endDate = studentClass.EndDate.HasValue ? (studentClass.EndDate.Value < DateTime.Now ? studentClass.EndDate.Value : DateTime.Now) : DateTime.Now;

            int teacherAbsent = 0;
            int teacherUnavailable = 0;
            int childAbsent = 0;
            int childUnavailable = 0;
            int daysMet = 0;

            List<DateTime> datesToProcess = CreateDateTimeList(startDate, endDate);

            int runningTotal = 0;
            List<InterventionAttendance> interventionAttendance =
                repository.InterventionAttendances
                .Include(p => p.AttendanceReason)
                .Where(p => p.SectionID == section.Id && p.StudentID == studentId).ToList();

            foreach (var dateTime in datesToProcess)
            {
                bool alreadyProcessed = false;
                if (dateTime.DayOfWeek != DayOfWeek.Saturday && dateTime.DayOfWeek != DayOfWeek.Sunday)
                {
                    // check the daysmet
                    switch (dateTime.DayOfWeek)
                    {
                        case DayOfWeek.Monday:
                            if (section.MondayMeet.HasValue && section.MondayMeet.Value == true)
                            {
                                ProcessMeetingDate(dateTime, ref daysMet, ref teacherAbsent, ref teacherUnavailable,
                                    ref childAbsent, ref childUnavailable, interventionAttendance, schoolId);
                                alreadyProcessed = true;
                            }
                            break;
                        case DayOfWeek.Tuesday:
                            if (section.TuesdayMeet.HasValue && section.TuesdayMeet.Value == true)
                            {
                                ProcessMeetingDate(dateTime, ref daysMet, ref teacherAbsent, ref teacherUnavailable,
                                    ref childAbsent, ref childUnavailable, interventionAttendance, schoolId);
                                alreadyProcessed = true;
                            }
                            break;
                        case DayOfWeek.Wednesday:
                            if (section.WednesdayMeet.HasValue && section.WednesdayMeet.Value == true)
                            {
                                ProcessMeetingDate(dateTime, ref daysMet, ref teacherAbsent, ref teacherUnavailable,
                                    ref childAbsent, ref childUnavailable, interventionAttendance, schoolId);
                                alreadyProcessed = true;
                            }
                            break;
                        case DayOfWeek.Thursday:
                            if (section.ThursdayMeet.HasValue && section.ThursdayMeet.Value == true)
                            {
                                ProcessMeetingDate(dateTime, ref daysMet, ref teacherAbsent, ref teacherUnavailable,
                                    ref childAbsent, ref childUnavailable, interventionAttendance, schoolId);
                                alreadyProcessed = true;
                            }
                            break;
                        case DayOfWeek.Friday:
                            if (section.FridayMeet.HasValue && section.FridayMeet.Value == true)
                            {
                                ProcessMeetingDate(dateTime, ref daysMet, ref teacherAbsent, ref teacherUnavailable,
                                    ref childAbsent, ref childUnavailable, interventionAttendance, schoolId);
                                alreadyProcessed = true;
                            }
                            break;
                    }

                    if (!alreadyProcessed)
                    {
                        ProcessNonMeetingDate(dateTime, ref daysMet, ref teacherAbsent, ref teacherUnavailable,
                            ref childAbsent, ref childUnavailable, interventionAttendance, schoolId);
                    }



                }
            }
          

            return daysMet;
        }

        //public string GetPieChart(int sectionId, int studentId, int schoolId, int districtId)
        //{
        //    int teacherAbsent = 0;
        //    int teacherUnavailable = 0;
        //    int childAbsent = 0;
        //    int childUnavailable = 0;
        //    int daysMet = 0;

        //    chart piechart = new chart()
        //    {
        //        caption = "Attendance Chart",
        //        bgcolor = "ffffff,ffffff",
        //        showBorder = "0",
        //        showLabels = "0",
        //        showLegend = "1"
        //    };

        //    AddHolidays(schoolId, districtId);

        //    StudentClass studentClass =
        //        repository.Context.StudentClasses.First(p => p.StudentID == studentId && p.ClassID == sectionId);

        //    Section section = studentClass.Section;

        //    DateTime startDate = studentClass.StartDate.Value;
        //    DateTime endDate = studentClass.EndDate.HasValue ? (studentClass.EndDate.Value < DateTime.Now ? studentClass.EndDate.Value : DateTime.Now) : DateTime.Now;

        //    List<DateTime> datesToProcess = CreateDateTimeList(startDate, endDate);

        //    List<InterventionAttendance> interventionAttendance =
        //        repository.Context.InterventionAttendances.Where(
        //            p => p.SectionID == sectionId && p.StudentID == studentId).ToList();


        //    foreach (var dateTime in datesToProcess)
        //    {
        //        bool alreadyProcessed = false;
        //        if (dateTime.DayOfWeek != DayOfWeek.Saturday && dateTime.DayOfWeek != DayOfWeek.Sunday)
        //        {
        //            // check the daysmet
        //            switch (dateTime.DayOfWeek)
        //            {
        //                case DayOfWeek.Monday:
        //                    if (section.MondayMeet)
        //                    {
        //                        ProcessMeetingDate(dateTime, ref daysMet, ref teacherAbsent, ref teacherUnavailable,
        //                                    ref childAbsent, ref childUnavailable, interventionAttendance, districtId, schoolId);
        //                        alreadyProcessed = true;
        //                    }
        //                    break;
        //                case DayOfWeek.Tuesday:
        //                    if (section.TuesdayMeet)
        //                    {
        //                        ProcessMeetingDate(dateTime, ref daysMet, ref teacherAbsent, ref teacherUnavailable,
        //                                    ref childAbsent, ref childUnavailable, interventionAttendance, districtId, schoolId);
        //                        alreadyProcessed = true;
        //                    }
        //                    break;
        //                case DayOfWeek.Wednesday:
        //                    if (section.WednesdayMeet)
        //                    {
        //                        ProcessMeetingDate(dateTime, ref daysMet, ref teacherAbsent, ref teacherUnavailable,
        //                                    ref childAbsent, ref childUnavailable, interventionAttendance, districtId, schoolId);
        //                        alreadyProcessed = true;
        //                    }
        //                    break;
        //                case DayOfWeek.Thursday:
        //                    if (section.ThursdayMeet)
        //                    {
        //                        ProcessMeetingDate(dateTime, ref daysMet, ref teacherAbsent, ref teacherUnavailable,
        //                                    ref childAbsent, ref childUnavailable, interventionAttendance, districtId, schoolId);
        //                        alreadyProcessed = true;
        //                    }
        //                    break;
        //                case DayOfWeek.Friday:
        //                    if (section.FridayMeet)
        //                    {
        //                        ProcessMeetingDate(dateTime, ref daysMet, ref teacherAbsent, ref teacherUnavailable,
        //                                    ref childAbsent, ref childUnavailable, interventionAttendance, districtId, schoolId);
        //                        alreadyProcessed = true;
        //                    }
        //                    break;
        //            }

        //            if (!alreadyProcessed)
        //            {
        //                ProcessNonMeetingDate(dateTime, ref daysMet, ref teacherAbsent, ref teacherUnavailable,
        //    ref childAbsent, ref childUnavailable, interventionAttendance, districtId, schoolId);
        //            }



        //        }
        //    }


        //    #region refput chart
        //    if (teacherAbsent != 0)
        //        piechart.sets.Add(new set() { value = teacherAbsent.ToString(), label = "Teacher Absent", color = "#FF0048" });

        //    if (teacherUnavailable != 0)
        //        piechart.sets.Add(new set() { value = teacherUnavailable.ToString(), label = "Teacher Unavailable", color = "#FFAA48" });

        //    if (childAbsent != 0)
        //        piechart.sets.Add(new set() { value = childAbsent.ToString(), label = "Child Absent", color = "#FF47E0" });

        //    if (childUnavailable != 0)
        //        piechart.sets.Add(new set() { value = childUnavailable.ToString(), label = "Child Unavailable", color = "#B71CFF" });

        //    if (daysMet != 0)
        //        piechart.sets.Add(new set() { value = daysMet.ToString(), label = "Intervention Delivered", color = "#00FF00" });

        //    var utf8 = new UTF8Encoding(false);
        //    using (var ms = new MemoryStream())
        //    {
        //        using (var x = new XmlTextWriter(ms, utf8) { Formatting = Formatting.None })
        //        {
        //            var xs = new XmlSerializer(piechart.GetType());
        //            xs.Serialize(x, piechart);
        //            //Response.Write(utf8.GetString(ms.GetBuffer(), 0, (int)ms.Length));

        //            if (!String.IsNullOrEmpty(HttpContext.Current.Request["printmode"]))
        //            {
        //                FusionCharts.SetRenderer("javascript");
        //            }

        //            return FusionCharts.RenderChart("/Content/FusionCharts/Pie2D.swf", "", HttpUtility.JavaScriptStringEncode(utf8.GetString(ms.GetBuffer(), 0, (int)ms.Length)), Guid.NewGuid().ToString().Replace('-', '_'), "400", "300", false, true);
        //        }
        //    }
        //    #endregion
        //}

        private void ProcessMeetingDate(DateTime dateTime, ref int daysMet, ref int teacherAbsent, ref int teacherUnavailable, ref int childAbsent, ref int childUnavailable, List<InterventionAttendance> interventionAttendance, int schoolId)
        {
            bool alreadyRemoved = false;
            var standardAttendance =
    interventionAttendance.FirstOrDefault(
        p => p.AttendanceDate == dateTime);

            // if this is a standard meeting date with no status, an intervention delivered day or make up lesson day
            if (standardAttendance == null ||
                standardAttendance.AttendanceReason.Reason == "Intervention Delivered" ||
                standardAttendance.AttendanceReason.CountsAsBonusDay)
            {
                daysMet++;

                // only subtract holidays if > 0 already and stop at zero
                // see if this date falls on a district holiday
                foreach (var districtCalendar in DistrictCalendars)
                {

                    if (dateTime >= districtCalendar.Start && dateTime <= districtCalendar.End)
                    {
                        if (daysMet > 0)
                        {
                            daysMet--;
                            alreadyRemoved = true;
                            break;
                        }
                    }
                }


                // see if this date falls on a school holiday
                foreach (var schoolCalendar in SchoolCalendars[schoolId])
                {

                    if (dateTime >= schoolCalendar.Start && dateTime <= schoolCalendar.End && !alreadyRemoved)
                    {
                        if (daysMet > 0)
                        {
                            daysMet--;
                        }
                    }
                }
            }
            else if (standardAttendance != null &&
                     standardAttendance.AttendanceReason.Reason == "Teacher Absent")
            {
                teacherAbsent++;

                // only subtract holidays if > 0 already and stop at zero
                // see if this date falls on a district holiday
                foreach (var districtCalendar in DistrictCalendars)
                {

                    if (dateTime >= districtCalendar.Start && dateTime <= districtCalendar.End)
                    {
                        if (teacherAbsent > 0)
                        {
                            teacherAbsent--;
                            alreadyRemoved = true;
                            break;
                        }
                    }
                }


                // see if this date falls on a school holiday
                foreach (var schoolCalendar in SchoolCalendars[schoolId])
                {

                    if (dateTime >= schoolCalendar.Start && dateTime <= schoolCalendar.End && !alreadyRemoved)
                    {
                        if (teacherAbsent > 0)
                        {
                            teacherAbsent--;
                        }
                    }
                }
            }
            else if (standardAttendance != null &&
                     standardAttendance.AttendanceReason.Reason == "Teacher Unavailable")
            {
                teacherUnavailable++;

                // only subtract holidays if > 0 already and stop at zero
                // see if this date falls on a district holiday
                foreach (var districtCalendar in DistrictCalendars)
                {

                    if (dateTime >= districtCalendar.Start && dateTime <= districtCalendar.End)
                    {
                        if (teacherUnavailable > 0)
                        {
                            teacherUnavailable--;
                            alreadyRemoved = true;
                            break;
                        }
                    }
                }


                // see if this date falls on a school holiday
                foreach (var schoolCalendar in SchoolCalendars[schoolId])
                {

                    if (dateTime >= schoolCalendar.Start && dateTime <= schoolCalendar.End && !alreadyRemoved)
                    {
                        if (teacherUnavailable > 0)
                        {
                            teacherUnavailable--;
                        }
                    }
                }
            }
            else if (standardAttendance != null &&
                     standardAttendance.AttendanceReason.Reason == "Child Absent")
            {
                childAbsent++;

                // only subtract holidays if > 0 already and stop at zero
                // see if this date falls on a district holiday
                foreach (var districtCalendar in DistrictCalendars)
                {

                    if (dateTime >= districtCalendar.Start && dateTime <= districtCalendar.End)
                    {
                        if (childAbsent > 0)
                        {
                            childAbsent--;
                            alreadyRemoved = true;
                            break;
                        }
                    }
                }


                // see if this date falls on a school holiday
                foreach (var schoolCalendar in SchoolCalendars[schoolId])
                {

                    if (dateTime >= schoolCalendar.Start && dateTime <= schoolCalendar.End && !alreadyRemoved)
                    {
                        if (childAbsent > 0)
                        {
                            childAbsent--;
                        }
                    }
                }
            }
            else if (standardAttendance != null &&
                     standardAttendance.AttendanceReason.Reason == "Child Unavailable")
            {
                childUnavailable++;

                // only subtract holidays if > 0 already and stop at zero
                // see if this date falls on a district holiday
                foreach (var districtCalendar in DistrictCalendars)
                {

                    if (dateTime >= districtCalendar.Start && dateTime <= districtCalendar.End)
                    {
                        if (childUnavailable > 0)
                        {
                            childUnavailable--;
                            alreadyRemoved = true;
                            break;
                        }
                    }
                }


                // see if this date falls on a school holiday
                foreach (var schoolCalendar in SchoolCalendars[schoolId])
                {

                    if (dateTime >= schoolCalendar.Start && dateTime <= schoolCalendar.End && !alreadyRemoved)
                    {
                        if (childUnavailable > 0)
                        {
                            childUnavailable--;
                        }
                    }
                }
            }
        }

        private void ProcessNonMeetingDate(DateTime dateTime, ref int daysMet, ref int teacherAbsent, ref int teacherUnavailable, ref int childAbsent, ref int childUnavailable, List<InterventionAttendance> interventionAttendance, int schoolId)
        {
            bool alreadyRemoved = false;
            var standardAttendance =
    interventionAttendance.FirstOrDefault(
        p => p.AttendanceDate == dateTime);

            // if this is an intervention delivered day or make up lesson day
            if (standardAttendance != null && (
                standardAttendance.AttendanceReason.Reason == "Intervention Delivered" ||
                standardAttendance.AttendanceReason.CountsAsBonusDay))
            {
                daysMet++;

                // only subtract holidays if > 0 already and stop at zero
                // see if this date falls on a district holiday
                foreach (var districtCalendar in DistrictCalendars)
                {

                    if (dateTime >= districtCalendar.Start && dateTime <= districtCalendar.End)
                    {
                        if (daysMet > 0)
                        {
                            daysMet--;
                            alreadyRemoved = true;
                            break;
                        }
                    }
                }


                // see if this date falls on a school holiday
                foreach (var schoolCalendar in SchoolCalendars[schoolId])
                {

                    if (dateTime >= schoolCalendar.Start && dateTime <= schoolCalendar.End && !alreadyRemoved)
                    {
                        if (daysMet > 0)
                        {
                            daysMet--;
                        }
                    }
                }
            }
            else if (standardAttendance != null &&
                     standardAttendance.AttendanceReason.Reason == "Teacher Absent")
            {
                teacherAbsent++;

                // only subtract holidays if > 0 already and stop at zero
                // see if this date falls on a district holiday
                foreach (var districtCalendar in DistrictCalendars)
                {

                    if (dateTime >= districtCalendar.Start && dateTime <= districtCalendar.End)
                    {
                        if (teacherAbsent > 0)
                        {
                            teacherAbsent--;
                            alreadyRemoved = true;
                            break;
                        }
                    }
                }


                // see if this date falls on a school holiday
                foreach (var schoolCalendar in SchoolCalendars[schoolId])
                {

                    if (dateTime >= schoolCalendar.Start && dateTime <= schoolCalendar.End && !alreadyRemoved)
                    {
                        if (teacherAbsent > 0)
                        {
                            teacherAbsent--;
                        }
                    }
                }
            }
            else if (standardAttendance != null &&
                     standardAttendance.AttendanceReason.Reason == "Teacher Unavailable")
            {
                teacherUnavailable++;

                // only subtract holidays if > 0 already and stop at zero
                // see if this date falls on a district holiday
                foreach (var districtCalendar in DistrictCalendars)
                {

                    if (dateTime >= districtCalendar.Start && dateTime <= districtCalendar.End)
                    {
                        if (teacherUnavailable > 0)
                        {
                            teacherUnavailable--;
                            alreadyRemoved = true;
                            break;
                        }
                    }
                }


                // see if this date falls on a school holiday
                foreach (var schoolCalendar in SchoolCalendars[schoolId])
                {

                    if (dateTime >= schoolCalendar.Start && dateTime <= schoolCalendar.End && !alreadyRemoved)
                    {
                        if (teacherUnavailable > 0)
                        {
                            teacherUnavailable--;
                        }
                    }
                }
            }
            else if (standardAttendance != null &&
                     standardAttendance.AttendanceReason.Reason == "Child Absent")
            {
                childAbsent++;

                // only subtract holidays if > 0 already and stop at zero
                // see if this date falls on a district holiday
                foreach (var districtCalendar in DistrictCalendars)
                {

                    if (dateTime >= districtCalendar.Start && dateTime <= districtCalendar.End)
                    {
                        if (childAbsent > 0)
                        {
                            childAbsent--;
                            alreadyRemoved = true;
                            break;
                        }
                    }
                }


                // see if this date falls on a school holiday
                foreach (var schoolCalendar in SchoolCalendars[schoolId])
                {

                    if (dateTime >= schoolCalendar.Start && dateTime <= schoolCalendar.End && !alreadyRemoved)
                    {
                        if (childAbsent > 0)
                        {
                            childAbsent--;
                        }
                    }
                }
            }
            else if (standardAttendance != null &&
                     standardAttendance.AttendanceReason.Reason == "Child Unavailable")
            {
                childUnavailable++;

                // only subtract holidays if > 0 already and stop at zero
                // see if this date falls on a district holiday
                foreach (var districtCalendar in DistrictCalendars)
                {

                    if (dateTime >= districtCalendar.Start && dateTime <= districtCalendar.End)
                    {
                        if (childUnavailable > 0)
                        {
                            childUnavailable--;
                            alreadyRemoved = true;
                            break;
                        }
                    }
                }


                // see if this date falls on a school holiday
                foreach (var schoolCalendar in SchoolCalendars[schoolId])
                {

                    if (dateTime >= schoolCalendar.Start && dateTime <= schoolCalendar.End && !alreadyRemoved)
                    {
                        if (childUnavailable > 0)
                        {
                            childUnavailable--;
                        }
                    }
                }
            }
        }

        private void AddHolidays(int schoolId)
        {
            if (!districtHolidaysAdded)
            {
                AddDistrictHolidays();
            }
            if (!SchoolCalendars.ContainsKey(schoolId))
            {
                AddSchoolHolidays(schoolId);
            }
        }

        private List<DateTime> CreateDateTimeList(DateTime start, DateTime end)
        {
            DateTime normalizedStart = new DateTime(start.Year, start.Month, start.Day);
            DateTime normalizedEnd = new DateTime(end.Year, end.Month, end.Day);

            List<DateTime> dateList = new List<DateTime>();

            for (int i = 0; i <= (normalizedEnd - normalizedStart).TotalDays; i++)
            {
                dateList.Add(normalizedStart.AddDays(i));
            }

            return dateList;
        }

        private void AddDistrictHolidays()
        {
            List<DistrictCalendar> dc = repository.DistrictCalendars.ToList();

            DistrictCalendars.AddRange(dc);
        }

        private void AddSchoolHolidays(int schoolId)
        {
            List<SchoolCalendar> sc = repository.SchoolCalendars.Where(
                            p =>
                            p.SchoolID == schoolId).ToList();

            SchoolCalendars.Add(schoolId, sc);
        }
    }
}
