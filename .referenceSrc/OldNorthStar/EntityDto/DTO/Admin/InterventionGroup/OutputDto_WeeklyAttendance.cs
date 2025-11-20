using System.Collections.Generic;
using NorthStar4.CrossPlatform.Entity;
using EntityDto.DTO.Admin.Simple;

namespace NorthStar4.CrossPlatform.DTO.Admin.InterventionGroup
{
    public class OutputDto_WeeklyAttendance : OutputDto_Base
    {
         public List<WeeklyAttendanceResult> AttendanceData { get; set; }
         public List<AttendanceWeekdayDto> WeekDays { get; set; }
    }
}