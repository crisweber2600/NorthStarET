using AutoMapper;
using EntityDto.DTO.Admin.InterventionGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using NorthStar.EF6.Infrastructure;

namespace NorthStar.EF6.DataService
{
    public class InterventionDashboardDataService : NSBaseDataService
    {
        public InterventionDashboardDataService(ClaimsIdentity user, string loginConnectionString) : base(user, loginConnectionString)
        {

        }

        public OutputDto_GetStintAttendanceSummary GetStintAttendanceSummary(InputDto_GetStintAttendanceSummary input)
        {
            var stintStartEnd = _dbContext.StudentInterventionGroups.FirstOrDefault(p => p.Id == input.StintId);
            var start = DateTime.Now.AddYears(-50);
            var end = DateTime.Now.AddYears(100);
            if (stintStartEnd != null)
            {
                start = stintStartEnd.StartDate;
                end = stintStartEnd.EndDate ?? end;
            }

            // security scheck StudentId
            var notes = _dbContext.InterventionAttendances
                .Include(p => p.AttendanceReason)
                .Include(p => p.Recorder)
                .Where(p => p.ClassStartEndID == input.StintId && !String.IsNullOrEmpty(p.Notes) && p.Notes != "Auto Added" && p.AttendanceDate >= start && p.AttendanceDate <= end).OrderByDescending(p => p.AttendanceDate).ToList();


            var attendanceData = _dbContext.InterventionAttendances
                .Include(p => p.AttendanceReason)
                .Where(p => p.ClassStartEndID == input.StintId && p.AttendanceDate >= start && p.AttendanceDate <= end).OrderByDescending(p => p.AttendanceDate).ToList();

            var attendanceCalculator = new AttendanceCalculator(_dbContext);
            //var sessionsMissed = attendanceCalculator.GetSessionsMet(input.i)

            //var groupedNotes = new List<AttendanceNotesByType>();
            //foreach(var note in notes)
            //{
            //    var noteGroup = groupedNotes.FirstOrDefault(p => p.AttendanceStatus == note.AttendanceReason.Reason);

            //    if(noteGroup == null)
            //    {
            //        noteGroup = new AttendanceNotesByType { AttendanceStatus = note.AttendanceReason.Reason };
            //        noteGroup.StintNotes.Add(Mapper.Map<InterventionAttendanceDto>(note));
            //        groupedNotes.Add(noteGroup);
            //    }
            //    else
            //    {
            //        noteGroup.StintNotes.Add(Mapper.Map<InterventionAttendanceDto>(note));
            //    }
            //}
            //var deliveredCount = attendanceCalculator.GetSessionsMet(input.StintId, input.StudentId);
            //var missedCount = attendanceCalculator.GetSessionsMissed(input.StintId, input.StudentId);

            // don't comput for non-cycle days and "none" status... irrelevant
            var distinctAttendanceReasons = _dbContext.AttendanceReasons.Where(p => p.Reason != "Non-Cycle Day" && p.Reason != "None").ToList();
            var distinctAttendanceReasonSummaries = distinctAttendanceReasons.Select(p => new AttendanceStatusSummary { Count = 0, StatusLabel = p.Reason }).ToList();

            attendanceCalculator.SetAttendanceStatuses(distinctAttendanceReasons, distinctAttendanceReasonSummaries, attendanceData, input.StudentId, input.StintId);

            //var summary = from attendance in _dbContext.InterventionAttendances
            //              where attendance.ClassStartEndID == input.StintId && attendance.StudentID == input.StudentId && attendance.AttendanceDate >= start && attendance.AttendanceDate <= end
            //              group attendance by attendance.AttendanceReason.Reason into grouping
            //              select new AttendanceStatusSummary { Count = grouping.Count(), StatusLabel = grouping.Key };

            return new OutputDto_GetStintAttendanceSummary { Notes = Mapper.Map<List<InterventionAttendanceDto>>(notes), AttendanceSummary = distinctAttendanceReasonSummaries };
        }
    }
}
