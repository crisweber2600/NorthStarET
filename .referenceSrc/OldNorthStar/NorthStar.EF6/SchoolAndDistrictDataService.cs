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

namespace NorthStar.EF6
{
    public class SchoolAndDistrictDataService : NSBaseDataService
    {
        public SchoolAndDistrictDataService(ClaimsIdentity user, string loginConnectionString) : base(user, loginConnectionString)
        {

        }
        public OutputDto_DistrictCalendarList GetDistrictCalendar()
        {
            var response = new OutputDto_DistrictCalendarList();
            var calendars = _dbContext.DistrictCalendars.ToList();
            response.CalendarItems = Mapper.Map<List<DistrictCalendarDto>>(calendars);

            return response;
        }

        public OutputDto_SuccessAndNewId SaveDistrictCalendarEvent(DistrictCalendarDto item)
        {
            var response = new OutputDto_SuccessAndNewId();
            var existingEvent = _dbContext.DistrictCalendars.FirstOrDefault(p => p.Id == item.Id);

            if(existingEvent != null)
            {
                existingEvent.Subject = item.Subject;
            }
            else
            {
                existingEvent = _dbContext.DistrictCalendars.Create();
                existingEvent.Subject = item.Subject;
                existingEvent.Start = item.Start;
                existingEvent.End = item.End;
                _dbContext.DistrictCalendars.Add(existingEvent);
            }

            _dbContext.SaveChanges();
            response.id = existingEvent.Id;
            return response;
        }

        public OutputDto_SuccessAndNewId SaveSchoolCalendarEvent(SchoolCalendarDto item)
        {
            var check = SchoolAdminSecurityCheck(item.SchoolID);
            if (check.Status.StatusCode != StatusCode.Ok)
            {
                return (OutputDto_SuccessAndNewId)check;
            }
            var response = new OutputDto_SuccessAndNewId();
            var existingEvent = _dbContext.SchoolCalendars.FirstOrDefault(p => p.Id == item.Id);

            if (existingEvent != null)
            {
                existingEvent.Subject = item.Subject;
            }
            else
            {
                existingEvent = _dbContext.SchoolCalendars.Create();
                existingEvent.SchoolID = item.SchoolID;
                existingEvent.Subject = item.Subject;
                existingEvent.Start = item.Start;
                existingEvent.End = item.End;
                _dbContext.SchoolCalendars.Add(existingEvent);
            }

            _dbContext.SaveChanges();
            response.id = existingEvent.Id;
            return response;
        }

        public OutputDto_Success DeleteDistrictCalendarEvent(DistrictCalendarDto item)
        {
            var response = new OutputDto_Success();
            var existingEvent = _dbContext.DistrictCalendars.FirstOrDefault(p => p.Id == item.Id);

            if (existingEvent != null)
            {
                _dbContext.DistrictCalendars.Remove(existingEvent);
            }

            _dbContext.SaveChanges();
            return response;
        }

        public OutputDto_Success DeleteSchoolCalendarEvent(SchoolCalendarDto item)
        {
            var response = new OutputDto_Success();
            var check = SchoolAdminSecurityCheck(item.SchoolID);
            if(check.Status.StatusCode != StatusCode.Ok)
            {
                return (OutputDto_Success)check;
            }
            
            var existingEvent = _dbContext.SchoolCalendars.FirstOrDefault(p => p.Id == item.Id);

            if (existingEvent != null)
            {
                _dbContext.SchoolCalendars.Remove(existingEvent);
            }

            _dbContext.SaveChanges();
            return response;
        }

        public OutputDto_SchoolCalendarList GetSchoolCalendar(InputDto_SimpleId input)
        {
            var response = new OutputDto_SchoolCalendarList();
            var calendars = _dbContext.SchoolCalendars.Where(p => p.SchoolID == input.Id).ToList();
            response.CalendarItems = Mapper.Map<List<SchoolCalendarDto>>(calendars);

            return response;
        }
    }
}
