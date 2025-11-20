using AutoMapper;
using com.vzaar.api;
using EntityDto.DTO.Admin.District;
using EntityDto.DTO.Admin.InterventionGroup;
using EntityDto.DTO.Admin.InterventionToolkit;
using EntityDto.DTO.Admin.Section;
using EntityDto.DTO.Admin.Simple;
using EntityDto.DTO.Admin.Student;
using EntityDto.DTO.Admin.TeamMeeting;
using EntityDto.DTO.Assessment;
using EntityDto.DTO.Assessment.Benchmarks;
using EntityDto.DTO.Calendars;
using EntityDto.DTO.Personal;
using EntityDto.Entity;
using EntityDto.LoginDB.DTO;
using EntityDto.LoginDB.Entity;
using Newtonsoft.Json;
using NorthStar4.CrossPlatform.DTO.Admin.Staff;
using NorthStar4.CrossPlatform.Entity;
using NorthStar4.PCL.DTO;
using NorthStar4.PCL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NorthStar.EF6.Infrastructure
{
    public static class DtoMappings
    {
        public static void Map()
        {

            Mapper.Initialize(cfg => {


                int districtId = 0;

                cfg.CreateMap<NSIntervention, InterventionDto>()
                .ForMember(dest => dest.TierColor, opts => opts.MapFrom(src => src.InterventionTierId.HasValue ? src.InterventionTier.TierColor : "#7dcc93"))
                .ForMember(dest => dest.TierLabel, opts => opts.MapFrom(src => src.InterventionTierId.HasValue ?  src.InterventionTier.TierLabel : "Tier 1"))
                .ForMember(dest => dest.AssessmentTools, opts => opts.MapFrom(src => src.InterventionToolInterventions.Where(p => p.InterventionTypeId == src.Id && p.InterventionTool.InterventionToolTypeId == 1).OrderBy(p => p.InterventionTool.ToolName)))
                .ForMember(dest => dest.InterventionTools, opts => opts.MapFrom(src => src.InterventionToolInterventions.Where(p => p.InterventionTypeId == src.Id && p.InterventionTool.InterventionToolTypeId == 2).OrderBy(p => p.InterventionTool.ToolName)))
                .ForMember(dest => dest.InterventionWorkshop, opts => opts.MapFrom(src => src.InterventionWorkshopId.HasValue ? new OutputDto_DropdownData { id = src.InterventionWorkshopId.Value, text = src.InterventionWorkshop.WorkshopName } : null))
                .ForMember(dest => dest.Videos, opts => opts.MapFrom(src => src.InterventionVideoInterventions.Where(p => p.InterventionVideo.InterventionVideoDistricts.Any(j => j.DistrictId == districtId))))
                .ForMember(dest => dest.InterventionGrades, opts => opts.MapFrom(src => src.InterventionGrades.OrderBy(p => p.Grade.GradeOrder).ToList()));

            });
            // data entry DTOs
            Mapper.CreateMap<AssessmentFieldGroup, DataEntryFieldGroupDto>()
                .ForMember(dest => dest.O, opts => opts.MapFrom(src => src.SortOrder))
                .ForMember(dest => dest.N, opts => opts.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id));

            Mapper.CreateMap<AssessmentLookupField, OutputDto_DropdownData>()
                .ForMember(dest => dest.id, opts => opts.MapFrom(src => src.FieldSpecificId))
                .ForMember(dest => dest.text, opts => opts.MapFrom(src => src.FieldValue));

            Mapper.CreateMap<AssessmentFieldCategory, DataEntryFieldCategoryDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.N, opts => opts.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.O, opts => opts.MapFrom(src => src.SortOrder));

            Mapper.CreateMap<Section, TMSectionDetailsDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.StaffID, opts => opts.MapFrom(src => src.StaffID))
                .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.Description))
                .ForMember(dest => dest.Grade, opts => opts.MapFrom(src => src.Grade.ShortName))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.SchoolID, opts => opts.MapFrom(src => src.SchoolID))
                .ForMember(dest => dest.SchoolStartYear, opts => opts.MapFrom(src => src.SchoolStartYear))
                .ForMember(dest => dest.StaffFullName, opts => opts.MapFrom(src => src.Staff.LastName + "," + src.Staff.FirstName))
                .ForMember(dest => dest.StaffLastName, opts => opts.MapFrom(src => src.Staff.LastName));

            Mapper.CreateMap<NSPage, PageDto>()
                .ForMember(dest => dest.Tools, opts => opts.MapFrom(src => src.PageTools))
                .ForMember(dest => dest.Presentations, opts => opts.MapFrom(src => src.PagePresentations))
                .ForMember(dest => dest.Videos, opts => opts.MapFrom(src => src.PageVideos))
                .ReverseMap();
            Mapper.CreateMap<PageTool, ToolDto>()
            .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.Tool.Description))
            .ForMember(dest => dest.FileExtension, opts => opts.MapFrom(src => src.Tool.FileExtension))
            .ForMember(dest => dest.FileSystemFileName, opts => opts.MapFrom(src => src.Tool.FileSystemFileName))
            .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Tool.Id))
            .ForMember(dest => dest.LastModified, opts => opts.MapFrom(src => src.Tool.LastModified))
            .ForMember(dest => dest.SortOrder, opts => opts.MapFrom(src => src.Tool.SortOrder))
            .ForMember(dest => dest.ToolName, opts => opts.MapFrom(src => src.Tool.ToolName))
            .ForMember(dest => dest.ToolFileName, opts => opts.MapFrom(src => src.Tool.ToolFileName));

            Mapper.CreateMap<PageVideo, VideoDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.VideoId))
                .ForMember(dest => dest.VideoName, opts => opts.MapFrom(src => src.Video.VideoName))
                .ForMember(dest => dest.ChapterStartTime, opts => opts.MapFrom(src => src.Video.ChapterStartTime))
                .ForMember(dest => dest.ParentVideoId, opts => opts.MapFrom(src => src.Video.ParentVideoId))
                .ForMember(dest => dest.EncodedVideoURL, opts => opts.MapFrom(src => src.Video.EncodedVideoURL))
                .ForMember(dest => dest.UploadedVideoFile, opts => opts.MapFrom(src => new OutputDto_DropdownData_VzaarVideo() { text = src.Video.VideoFileName, id = Int32.Parse(src.Video.VideoStreamId) }))
                .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.Video.Description))
                .ForMember(dest => dest.VideoLength, opts => opts.MapFrom(src => src.Video.VideoLength))
                .ForMember(dest => dest.FileExtension, opts => opts.MapFrom(src => src.Video.FileExtension))
                .ForMember(dest => dest.FileSize, opts => opts.MapFrom(src => src.Video.FileSize))
                .ForMember(dest => dest.LastModified, opts => opts.MapFrom(src => src.Video.LastModified))
                .ForMember(dest => dest.SortOrder, opts => opts.MapFrom(src => src.Video.SortOrder))
                .ForMember(dest => dest.VideoStreamId, opts => opts.MapFrom(src => src.Video.VideoStreamId))
                .ForMember(dest => dest.ThumbnailURL, opts => opts.MapFrom(src => src.Video.ThumbnailURL));
            //.ForMember(dest => dest.UploadedVideoFile, opts => opts.MapFrom(src => new OutputDto_DropdownData_VzaarVideo() { text = src.Video.VideoName, id = Int32.Parse(src.Video.VideoStreamId) }));

            Mapper.CreateMap<PagePresentation, PresentationDto>()
            .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.Presentation.Description))
            .ForMember(dest => dest.FileExtension, opts => opts.MapFrom(src => src.Presentation.FileExtension))
            .ForMember(dest => dest.FileSystemFileName, opts => opts.MapFrom(src => src.Presentation.FileSystemFileName))
            .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Presentation.Id))
            .ForMember(dest => dest.LastModified, opts => opts.MapFrom(src => src.Presentation.LastModified))
            .ForMember(dest => dest.SortOrder, opts => opts.MapFrom(src => src.Presentation.SortOrder))
            .ForMember(dest => dest.ToolName, opts => opts.MapFrom(src => src.Presentation.ToolName))
            .ForMember(dest => dest.ToolFileName, opts => opts.MapFrom(src => src.Presentation.ToolFileName));


            Mapper.CreateMap<com.vzaar.api.Video, OutputDto_DropdownData_VzaarVideo>()
            .ForMember(dest => dest.createdAt, opts => opts.MapFrom(src => src.createdAt))
            .ForMember(dest => dest.description, opts => opts.MapFrom(src => src.description))
            .ForMember(dest => dest.duration, opts => opts.MapFrom(src => src.duration))
            .ForMember(dest => dest.height, opts => opts.MapFrom(src => src.height))
            .ForMember(dest => dest.id, opts => opts.MapFrom(src => src.id))
            .ForMember(dest => dest.playCount, opts => opts.MapFrom(src => src.playCount))
            .ForMember(dest => dest.status, opts => opts.MapFrom(src => src.status))
            .ForMember(dest => dest.statusId, opts => opts.MapFrom(src => src.statusId))
            .ForMember(dest => dest.thumbnail, opts => opts.MapFrom(src => src.thumbnail))
            .ForMember(dest => dest.url, opts => opts.MapFrom(src => src.url))
            .ForMember(dest => dest.version, opts => opts.MapFrom(src => src.version))
                .ForMember(dest => dest.width, opts => opts.MapFrom(src => src.width))
                .ForMember(dest => dest.title, opts => opts.MapFrom(src => src.title))
                .ForMember(dest => dest.text, opts => opts.MapFrom(src => src.title));

            Mapper.CreateMap<VzaarApi.Video, OutputDto_DropdownData_VzaarVideo>()
.ForMember(dest => dest.createdAt, opts => opts.MapFrom(src => src["created_at"]))
.ForMember(dest => dest.description, opts => opts.MapFrom(src => src["description"]))
.ForMember(dest => dest.duration, opts => opts.MapFrom(src => src["duration"]))
//.ForMember(dest => dest.height, opts => opts.MapFrom(src => src.height))
.ForMember(dest => dest.id, opts => opts.MapFrom(src => src["id"]))
//.ForMember(dest => dest.playCount, opts => opts.MapFrom(src => src.playCount))
//.ForMember(dest => dest.status, opts => opts.MapFrom(src => src.status))
//.ForMember(dest => dest.statusId, opts => opts.MapFrom(src => src.statusId))
.ForMember(dest => dest.thumbnail, opts => opts.MapFrom(src => src["thumbnail_url"]))
.ForMember(dest => dest.url, opts => opts.MapFrom(src => src["url"]))
.ForMember(dest => dest.version, opts => opts.MapFrom(src => src["version"]))
    .ForMember(dest => dest.width, opts => opts.MapFrom(src => src["width"]))
    .ForMember(dest => dest.title, opts => opts.MapFrom(src => src["title"]))
    .ForMember(dest => dest.text, opts => opts.MapFrom(src => src["title"]));

            Mapper.CreateMap<NSInterventionVideoDistrict, OutputDto_DropdownData>()
                .ForMember(dest => dest.id, opts => opts.MapFrom(src => src.DistrictId))
                .ForMember(dest => dest.text, opts => opts.MapFrom(src => src.District.Name));

            Mapper.CreateMap<InterventionVideoGrade, OutputDto_DropdownData>()
                .ForMember(dest => dest.id, opts => opts.MapFrom(src => src.GradeId))
                .ForMember(dest => dest.text, opts => opts.MapFrom(src => src.Grade.ShortName));

            Mapper.CreateMap<NSInterventionVideoNSIntervention, OutputDto_DropdownData>()
                .ForMember(dest => dest.id, opts => opts.MapFrom(src => src.InterventionTypeId))
                .ForMember(dest => dest.text, opts => opts.MapFrom(src => src.InterventionType.Description + " (" + src.InterventionType.InterventionType + ")"));

            Mapper.CreateMap<NSIntervention, OutputDto_DropdownData>()
            .ForMember(dest => dest.id, opts => opts.MapFrom(src => src.Id))
            .ForMember(dest => dest.text, opts => opts.MapFrom(src => src.Description + " (" + src.InterventionType + ")"));

            Mapper.CreateMap<NSInterventionToolIntervention, NSInterventionToolDto>()
            .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.InterventionTool.Description))
            .ForMember(dest => dest.FileExtension, opts => opts.MapFrom(src => src.InterventionTool.FileExtension))
            .ForMember(dest => dest.InterventionToolTypeId, opts => opts.MapFrom(src => src.InterventionTool.InterventionToolTypeId))
            .ForMember(dest => dest.FileSystemFileName, opts => opts.MapFrom(src => src.InterventionTool.FileSystemFileName))
            .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.InterventionTool.Id))
            .ForMember(dest => dest.LastModified, opts => opts.MapFrom(src => src.InterventionTool.LastModified))
            .ForMember(dest => dest.SortOrder, opts => opts.MapFrom(src => src.InterventionTool.SortOrder))
            .ForMember(dest => dest.ToolName, opts => opts.MapFrom(src => src.InterventionTool.ToolName))
            .ForMember(dest => dest.ToolFileName, opts => opts.MapFrom(src => src.InterventionTool.ToolFileName));

            Mapper.CreateMap<NSInterventionVideo, NSInterventionVideoDto>()
                .ForMember(dest => dest.UploadedVideoFile, opts => opts.MapFrom(src => new OutputDto_DropdownData_VzaarVideo() { text = src.VideoFileName, id = Int32.Parse(src.VideoStreamId) }))
                .ForMember(dest => dest.Grades, opts => opts.MapFrom(src => src.InterventionVideoGrades))
                .ForMember(dest => dest.Interventions, opts => opts.MapFrom(src => src.InterventionVideoInterventions))
                .ForMember(dest => dest.Districts, opts => opts.MapFrom(src => src.InterventionVideoDistricts));

            Mapper.CreateMap<NSInterventionVideoNSIntervention, NSInterventionVideoDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.InterventionVideoId))
                .ForMember(dest => dest.VideoName, opts => opts.MapFrom(src => src.InterventionVideo.VideoName))
                .ForMember(dest => dest.ChapterStartTime, opts => opts.MapFrom(src => src.InterventionVideo.ChapterStartTime))
                .ForMember(dest => dest.ParentVideoId, opts => opts.MapFrom(src => src.InterventionVideo.ParentVideoId))
                .ForMember(dest => dest.EncodedVideoURL, opts => opts.MapFrom(src => src.InterventionVideo.EncodedVideoURL))
                .ForMember(dest => dest.UploadedVideoFile, opts => opts.MapFrom(src => new OutputDto_DropdownData_VzaarVideo() { text = src.InterventionVideo.VideoFileName, id = Int32.Parse(src.InterventionVideo.VideoStreamId) }))
                .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.InterventionVideo.Description))
                .ForMember(dest => dest.VideoLength, opts => opts.MapFrom(src => src.InterventionVideo.VideoLength))
                .ForMember(dest => dest.FileExtension, opts => opts.MapFrom(src => src.InterventionVideo.FileExtension))
                .ForMember(dest => dest.FileSize, opts => opts.MapFrom(src => src.InterventionVideo.FileSize))
                .ForMember(dest => dest.LastModified, opts => opts.MapFrom(src => src.InterventionVideo.LastModified))
                .ForMember(dest => dest.SortOrder, opts => opts.MapFrom(src => src.InterventionVideo.SortOrder))
                .ForMember(dest => dest.VideoStreamId, opts => opts.MapFrom(src => src.InterventionVideo.VideoStreamId))
                .ForMember(dest => dest.Districts, opts => opts.MapFrom(src => src.InterventionVideo.InterventionVideoDistricts))
                .ForMember(dest => dest.Grades, opts => opts.MapFrom(src => src.InterventionVideo.InterventionVideoGrades))
                .ForMember(dest => dest.Interventions, opts => opts.MapFrom(src => src.InterventionVideo.InterventionVideoInterventions))
                .ForMember(dest => dest.ThumbnailURL, opts => opts.MapFrom(src => src.InterventionVideo.ThumbnailURL));

            Mapper.CreateMap<District, DistrictDto>().ReverseMap();
            Mapper.CreateMap<District_Benchmark, DistrictBenchmarkDto>().ReverseMap();
            Mapper.CreateMap<District_YearlyAssessmentBenchmark, DistrictYearlyAssessmentBenchmarkDto>().ReverseMap();
            Mapper.CreateMap<Assessment_Benchmarks, AssessmentBenchmarkDto>().ReverseMap();
            Mapper.CreateMap<TestLevelPeriod, TestLevelPeriodDto>().ReverseMap();
            Mapper.CreateMap<AttendeeGroup, AttendeeGroupDto>().ReverseMap();
            Mapper.CreateMap<AttendeeGroupStaff, AttendeeGroupStaffDto>().ReverseMap();
            Mapper.CreateMap<SchoolAssessmentDto, SchoolAssessment>().ReverseMap();
            Mapper.CreateMap<StaffAssessment, StaffAssessmentDto>().ReverseMap();
            Mapper.CreateMap<StaffAssessmentDto, OutputDto_DropdownData>()
                .ForMember(dest => dest.id, opts => opts.MapFrom(src => src.AssessmentId))
                .ForMember(dest => dest.text, opts => opts.MapFrom(src => src.AssessmentName));
            Mapper.CreateMap<OutputDto_DropdownData, AssessmentDto>()
            .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.id))
            .ForMember(dest => dest.AssessmentName, opts => opts.MapFrom(src => src.text));
            Mapper.CreateMap<JobStateTestDataImport, JobStateTestDataImportDto>().ReverseMap();
            Mapper.CreateMap<JobInterventionDataExport, JobInterventionDataExportDto>().ReverseMap();
            Mapper.CreateMap<JobBenchmarkDataImport, JobBenchmarkDataImportDto>().ReverseMap();
            Mapper.CreateMap<JobInterventionDataImport, JobInterventionDataImportDto>().ReverseMap();
            Mapper.CreateMap<JobAttendanceExport, JobAttendanceExportDto>().ReverseMap();
            Mapper.CreateMap<JobStaffExport, JobStaffExportDto>().ReverseMap();
            Mapper.CreateMap<JobStudentExport, JobStudentExportDto>().ReverseMap();
            Mapper.CreateMap<JobPrintBatch, JobPrintBatchDto>().ReverseMap();
            Mapper.CreateMap<JobFullRollover, JobFullRolloverDto>();
            Mapper.CreateMap<JobStudentRollover, JobStudentRolloverDto>();
            Mapper.CreateMap<JobTeacherRollover, JobTeacherRolloverDto>();
            Mapper.CreateMap<JobAssessmentDataExport, JobAssessmentDataExportDto>();
            Mapper.CreateMap<JobAllFieldsAssessmentDataExport, JobAssessmentDataExportAllFieldsDto>();
            Mapper.CreateMap<InterventionDto, NSIntervention>()
                .ForMember(dest => dest.InterventionGrades, opts => opts.Ignore())
                .ForMember(dest => dest.InterventionWorkshop, opts => opts.Ignore())
                .ForMember(dest => dest.Id, opts => opts.Ignore())
                .ForMember(dest => dest.BriefDescription, opts => opts.MapFrom(src => src.BriefDescription))
                .ForMember(dest => dest.DetailedDescription, opts => opts.MapFrom(src => src.DetailedDescription))
                .ForMember(dest => dest.EntranceCriteria, opts => opts.MapFrom(src => src.EntranceCriteria))
                .ForMember(dest => dest.ExitCriteria, opts => opts.MapFrom(src => src.ExitCriteria))
                .ForMember(dest => dest.LearnerNeed, opts => opts.MapFrom(src => src.LearnerNeed))
                .ForMember(dest => dest.TimeOfYear, opts => opts.MapFrom(src => src.TimeOfYear))
                .ForMember(dest => dest.InterventionTierId, opts => opts.MapFrom(src => src.InterventionTierID))
                .ForMember(dest => dest.InterventionWorkshopId, opts => opts.MapFrom(src => src.InterventionWorkshop.id))                
                .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.Description));
            Mapper.CreateMap<NSInterventionToolTypeDto, NSInterventionToolType>().ReverseMap();
            Mapper.CreateMap<NSInterventionToolDto, NSInterventionTool>().ReverseMap();
            Mapper.CreateMap<NSInterventionCategory, InterventionCategoryDto>().ReverseMap();
            Mapper.CreateMap<NSInterventionWorkshop, InterventionWorkshopDto>().ReverseMap();
            Mapper.CreateMap<Assessment, AssessmentDto>().ReverseMap();
            Mapper.CreateMap<Assessment, AssessmentListDto>().ReverseMap();
            Mapper.CreateMap<AssessmentField, AssessmentFieldDto>()
                 .ForMember(dest => dest.AssessmentName, opts => opts.MapFrom(src => src.Assessment.AssessmentName))
                 .ForMember(dest => dest.FieldName, opts => opts.MapFrom(src => src.DatabaseColumn))
                .ReverseMap();
            Mapper.CreateMap<AssessmentFieldGroup, AssessmentFieldGroupDto>().ReverseMap();
            Mapper.CreateMap<AssessmentFieldGroupContainer, AssessmentFieldGroupContainerDto>().ReverseMap();
            Mapper.CreateMap<AssessmentFieldCategory, AssessmentFieldCategoryDto>().ReverseMap();
            Mapper.CreateMap<AssessmentFieldSubCategory, AssessmentFieldSubCategoryDto>().ReverseMap();
            Mapper.CreateMap<Student, StudentDto>().ReverseMap();
            Mapper.CreateMap<Staff, StaffDto>().ReverseMap();
            Mapper.CreateMap<Dto_StaffSchool, StaffSchool>().ReverseMap();
            Mapper.CreateMap<TestDueDate, TestDueDateDto>().ReverseMap();
            Mapper.CreateMap<School, SchoolDto>().ReverseMap();
            Mapper.CreateMap<SchoolYear, SchoolYearDto>().ReverseMap();
            Mapper.CreateMap<StaffAssessmentFieldVisibility, StaffAssessmentFieldVisibilityDto>().ReverseMap();
            Mapper.CreateMap<Section, SectionDto>().ReverseMap();
            Mapper.CreateMap<Grade, GradeDto>().ReverseMap();
            Mapper.CreateMap<NSGrade, GradeDto>().ReverseMap();
            Mapper.CreateMap<NSInterventionGrade, OutputDto_DropdownData>()
                .ForMember(dest => dest.id, opts => opts.MapFrom(src => src.GradeID))
                .ForMember(dest => dest.text, opts => opts.MapFrom(src => src.Grade.LongName));
            Mapper.CreateMap<NSInterventionTier, InterventionTierDto>()
                .ForMember(dest => dest.NumInterventions, opts => opts.MapFrom(src => src.InterventionTypes.Where(p => p.InterventionTierId == src.Id).Count()))
                .ReverseMap();
            Mapper.CreateMap<StudentSchool, StudentSchoolDto>()
                .ForMember(dest => dest.SchoolId, opts => opts.MapFrom(src => src.SchoolID))
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.StudentId, opts => opts.MapFrom(src => src.StudentID))
                .ForMember(dest => dest.SchoolYearLabel, opts => opts.MapFrom(src => src.SchoolYear.YearVerbose))
                .ForMember(dest => dest.SchoolName, opts => opts.MapFrom(src => src.School.Name))
                .ForMember(dest => dest.GradeId, opts => opts.MapFrom(src => src.GradeId))
                .ForMember(dest => dest.GradeName, opts => opts.MapFrom(src => src.Grade.ShortName))
                .ForMember(dest => dest.SchoolStartYear, opts => opts.MapFrom(src => src.SchoolStartYear)); 
            Mapper.CreateMap<Staff, OutputDto_EditStaff>().ReverseMap();
            Mapper.CreateMap<Assessment, OutputDto_ObservationSummaryFieldVisibility>()
                .ForMember(dest => dest.text, opts => opts.MapFrom(src => src.AssessmentName))
                .ForMember(dest => dest.id, opts => opts.MapFrom(src => src.Id));
            Mapper.CreateMap<AssessmentField, OutputDto_ObservationSummaryFieldVisibility>()
                .ForMember(dest => dest.text, opts => opts.MapFrom(src => src.DisplayLabel))
                .ForMember(dest => dest.id, opts => opts.MapFrom(src => src.Id));
            Mapper.CreateMap<StudentAttributeLookupValue, OutputDto_DropdownData>()
                .ForMember(dest => dest.text, opts => opts.MapFrom(src => src.LookupValue + (String.IsNullOrEmpty(src.Description) ? String.Empty : " (" + src.Description + ")")))
                .ForMember(dest => dest.id, opts => opts.MapFrom(src => src.Id));
            Mapper.CreateMap<Intervention, OutputDto_DropdownData>()
                .ForMember(dest => dest.text, opts => opts.MapFrom(src => src.InterventionType + " (" + src.Description + ")"))
                .ForMember(dest => dest.id, opts => opts.MapFrom(src => src.Id));
            Mapper.CreateMap<Intervention, DistrictInterventionDto>().ReverseMap();
            Mapper.CreateMap<StaffInterventionGroup, OutputDto_DropdownData>()
                .ForMember(dest => dest.text, opts => opts.MapFrom(src => src.Staff.LastName + ", " + src.Staff.FirstName))
                .ForMember(dest => dest.id, opts => opts.MapFrom(src => src.Staff.Id));
            Mapper.CreateMap<TeamMeetingAttendance, OutputDto_DropdownData>()
                .ForMember(dest => dest.text, opts => opts.MapFrom(src => src.Staff.LastName + ", " + src.Staff.FirstName))
                .ForMember(dest => dest.id, opts => opts.MapFrom(src => src.Staff.Id));
            Mapper.CreateMap<TeamMeetingManager, OutputDto_DropdownData>()
                .ForMember(dest => dest.text, opts => opts.MapFrom(src => src.Staff.LastName + ", " + src.Staff.FirstName))
                .ForMember(dest => dest.id, opts => opts.MapFrom(src => src.Staff.Id));
            Mapper.CreateMap<TeamMeeting, OutputDto_DropdownData>()
                .ForMember(dest => dest.text, opts => opts.MapFrom(src => "(" + src.MeetingTime.ToString("dd-MMM-yyyy hh:ss tt") + ") " + src.Title))
                .ForMember(dest => dest.id, opts => opts.MapFrom(src => src.ID));
            Mapper.CreateMap<SchoolYear, OutputDto_DropdownData>()
                .ForMember(dest => dest.text, opts => opts.MapFrom(src => src.YearVerbose))
                .ForMember(dest => dest.id, opts => opts.MapFrom(src => src.SchoolStartYear));
            Mapper.CreateMap<TestDueDate, OutputDto_DropdownData>()
                .ForMember(dest => dest.text, opts => opts.MapFrom(src => src.DisplayDate))
                .ForMember(dest => dest.id, opts => opts.MapFrom(src => src.Id));
            Mapper.CreateMap<Staff, OutputDto_DropdownData>()
                .ForMember(dest => dest.text, opts => opts.MapFrom(src => src.LastName + ", " + src.FirstName))
                .ForMember(dest => dest.id, opts => opts.MapFrom(src => src.Id));
            Mapper.CreateMap<School, OutputDto_DropdownData>()
                .ForMember(dest => dest.text, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.id, opts => opts.MapFrom(src => src.Id));
            Mapper.CreateMap<Grade, OutputDto_DropdownData>()
                .ForMember(dest => dest.text, opts => opts.MapFrom(src => src.ShortName))
                .ForMember(dest => dest.id, opts => opts.MapFrom(src => src.Id));
            Mapper.CreateMap<Section, OutputDto_DropdownData>()
                .ForMember(dest => dest.text, opts => opts.MapFrom(src => "(" + src.Staff.LastName + ") " + src.Name))
                .ForMember(dest => dest.id, opts => opts.MapFrom(src => src.Id));
            Mapper.CreateMap<InterventionGroup, OutputDto_DropdownData>()
                .ForMember(dest => dest.text, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.id, opts => opts.MapFrom(src => src.Id));
            Mapper.CreateMap<Student, OutputDto_DropdownData>()
                .ForMember(dest => dest.text, opts => opts.MapFrom(src => src.LastName + ", " + src.FirstName))
                .ForMember(dest => dest.id, opts => opts.MapFrom(src => src.Id));
            Mapper.CreateMap<TestDueDate, OutputDto_DropdownData_BenchmarkDate>()
                .ForMember(dest => dest.text, opts => opts.MapFrom(src => src.DisplayDate))
                .ForMember(dest => dest.id, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.testLevelPeriodId, opts => opts.MapFrom(src => src.TestLevelPeriodID))
                .ForMember(dest => dest.Hex, opts => opts.MapFrom(src => src.Hex));
            Mapper.CreateMap<StudentAttributeData, StudentAttributeDataDto>()
                .ForMember(dest => dest.AttributeID, opts => opts.MapFrom(src => src.AttributeID))
                .ForMember(dest => dest.AttributeValueID, opts => opts.MapFrom(src => src.AttributeValueID))
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.StudentID, opts => opts.MapFrom(src => src.StudentID));
            Mapper.CreateMap<StudentAttributeType, StudentAttributeTypeDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.AttributeName , opts => opts.MapFrom(src => src.AttributeName))
                .ForMember(dest => dest.LookupValues , opts => opts.MapFrom(src => src.LookupValues.OrderBy(p => p.LookupValue)));
            Mapper.CreateMap<StudentInterventionGroup, InterventionGroupStudentDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.StudentId, opts => opts.MapFrom(src => src.StudentID))
                .ForMember(dest => dest.InterventionGroupId, opts => opts.MapFrom(src => src.InterventionGroupId))
                .ForMember(dest => dest.StudentName, opts => opts.MapFrom(src => src.Student.LastName + ", " + src.Student.FirstName + " " + src.Student.MiddleName))
                .ForMember(dest => dest.InterventionGroupName, opts => opts.MapFrom(src => src.InterventionGroup.Name))
                .ForMember(dest => dest.InterventionType, opts => opts.MapFrom(src => src.InterventionGroup.InterventionType.InterventionType))
                .ForMember(dest => dest.InterventionTypeLong, opts => opts.MapFrom(src => src.InterventionGroup.InterventionType.Description))
                .ForMember(dest => dest.StartDate, opts => opts.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.StartDateText, opts => opts.MapFrom(src => src.StartDate.ToString("dd-MMM-yyyy")))
                .ForMember(dest => dest.EndDate, opts => opts.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.EndDateText, opts => opts.MapFrom(src => (src.EndDate.HasValue ? src.EndDate.Value.ToString("dd-MMM-yyyy") : "no end date")))
                .ForMember(dest => dest.InterventionistId, opts => opts.MapFrom(src => src.InterventionGroup.StaffID))
                .ForMember(dest => dest.SchoolYear, opts => opts.MapFrom(src => src.InterventionGroup.SchoolStartYear))
                .ForMember(dest => dest.SchoolId, opts => opts.MapFrom(src => src.InterventionGroup.SchoolID));
            Mapper.CreateMap<StudentInterventionGroup, OutputDto_DropdownData>()
                .ForMember(dest => dest.id, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.text, opts => opts.MapFrom(src => src.StartDate.ToString("dd-MMM-yyyy") + " --> " + (src.EndDate.HasValue ? src.EndDate.Value.ToString("dd-MMM-yyyy") : "no end date")));
            Mapper.CreateMap<StudentAttributeLookupValue, StudentAttributeLookupValueDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.IsSpecialEd, opts => opts.MapFrom(src => src.IsSpecialEd))
                .ForMember(dest => dest.AttributeId, opts => opts.MapFrom(src => src.AttributeID))
                .ForMember(dest => dest.LookupValue, opts => opts.MapFrom(src => src.LookupValue))
                .ForMember(dest => dest.LookupValueId, opts => opts.MapFrom(src => src.LookupValueID));
            Mapper.CreateMap<StudentAttributeLookupValue, OutputDto_DropdownData>()
                .ForMember(dest => dest.id, opts => opts.MapFrom(src => src.LookupValueID))
                .ForMember(dest => dest.text, opts => opts.MapFrom(src => src.LookupValue + (String.IsNullOrEmpty(src.Description) ? "" : " (" + src.Description + ")")));

            Mapper.CreateMap<InterventionAttendance, InterventionAttendanceDto>()
                .ForMember(dest => dest.AttendanceStatus, opts => opts.MapFrom(src => src.AttendanceReason.Reason))
                .ForMember(dest => dest.AttendanceDateString, opts => opts.MapFrom(src => src.AttendanceDate.ToString("dddd, MMMM d, yyyy")));
            Mapper.CreateMap<TeamMeeting, TeamMeetingDto>().ReverseMap();
            Mapper.CreateMap<TeamMeetingAttendance, TeamMeetingAttendanceDto>().ReverseMap();
            Mapper.CreateMap<TeamMeetingStudent, TeamMeetingStudentDto>().ReverseMap();
            Mapper.CreateMap<TeamMeetingStudentNote, TeamMeetingStudentNoteDto>().ReverseMap();
            Mapper.CreateMap<StudentNote, StudentNoteDto>().ReverseMap();
            Mapper.CreateMap<DistrictCalendar, DistrictCalendarDto>().ReverseMap();
            Mapper.CreateMap<SchoolCalendar, SchoolCalendarDto>().ReverseMap();
            Mapper.CreateMap<InterventionGroupDto, InterventionGroup>().ReverseMap();
            Mapper.CreateMap<TeamMeetingManager, TeamMeetingManagerDto>();
           // Mapper.CreateMap<OutputDto_ManageInterventionGroup, InterventionGroup>().ReverseMap();
        }
    }
}
