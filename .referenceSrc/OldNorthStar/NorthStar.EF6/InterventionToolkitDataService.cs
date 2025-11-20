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
using EntityDto.DTO.InterventionToolkit;
using NorthStar4.CrossPlatform.DTO.Admin.InterventionToolkit;
using EntityDto.DTO.Admin.InterventionToolkit;
using EntityDto.LoginDB.Entity;
using System.Data.SqlClient;
using System.Threading.Tasks;
using NorthStar.Core.FileUpload;
using EntityDto.LoginDB.DTO;
using AutoMapper.QueryableExtensions;
using Serilog;

namespace NorthStar.EF6
{
    public class InterventionToolkitDataService : NSBaseDataService
    {
        public InterventionToolkitDataService(ClaimsIdentity user, string loginConnectionString) : base(user, loginConnectionString)
        {

        }

        public OutputDto_Base AssociateUploadedInterventionTool(ImportTestDataViewModel files, int interventionId, int interventionToolTypeId)
        {
            var result = new OutputDto_Base();
            var file = files.Files.First();

            var intervention = _loginContext.NSInterventions.First(p => p.Id == interventionId);
            var newTool = _loginContext.NSInterventionTools.Create();

            // set up new tool
            newTool.InterventionToolTypeId = interventionToolTypeId;
            newTool.SortOrder = 0;
            newTool.ToolFileName = file.Name;
            newTool.FileSystemFileName = file.Url;
            _loginContext.NSInterventionTools.Add(newTool);

            var newInterventionToolInterventions = _loginContext.NSInterventionToolInterventions.Create();
            newInterventionToolInterventions.InterventionTool = newTool;
            newInterventionToolInterventions.InterventionType = intervention;
            _loginContext.NSInterventionToolInterventions.Add(newInterventionToolInterventions);

            _loginContext.SaveChanges();

            return result;
        }

        public OutputDto_Base AssociateUploadedPageTool(ImportTestDataViewModel files, int pageId)
        {
            var result = new OutputDto_Base();
            var file = files.Files.First();

            var page = _loginContext.NSPages.First(p => p.Id == pageId);
            var newTool = _loginContext.Tools.Create();

            // set up new tool
            newTool.SortOrder = 0;
            newTool.ToolFileName = file.Name;
            newTool.FileSystemFileName = file.Url;
            _loginContext.Tools.Add(newTool);

            var newPageTool = _loginContext.PageTools.Create();
            newPageTool.Tool = newTool;
            newPageTool.Page = page;
            _loginContext.PageTools.Add(newPageTool);

            _loginContext.SaveChanges();

            return result;
        }

        public OutputDto_Base AssociateUploadedPresention(ImportTestDataViewModel files, int pageId)
        {
            var result = new OutputDto_Base();
            var file = files.Files.First();

            var page = _loginContext.NSPages.First(p => p.Id == pageId);
            var newPresentation = _loginContext.Presentations.Create();

            // set up new tool
            newPresentation.SortOrder = 0;
            newPresentation.ToolFileName = file.Name;
            newPresentation.FileSystemFileName = file.Url;
            _loginContext.Presentations.Add(newPresentation);

            var newPagePresentation = _loginContext.PagePresentations.Create();
            newPagePresentation.Presentation = newPresentation;
            newPagePresentation.Page = page;
            _loginContext.PagePresentations.Add(newPagePresentation);

            _loginContext.SaveChanges();

            return result;
        }



        public OutputDto_GetTiers GetInterventionTiers()
        {
            var tiers = _loginContext.NSInterventionTiers.Where(p => p.TierValue < 4).OrderByDescending(p => p.TierValue).ToList();

            return new OutputDto_GetTiers
            {
                Tiers = Mapper.Map<List<InterventionTierDto>>(tiers)
            };
        }

        public OutputDto_Intervention GetInterventionById(InputDto_SimpleId input)
        {
            var intervention = _loginContext.NSInterventions.FirstOrDefault(p => p.Id == input.Id);

            if(intervention == null)
            {
                intervention = new NSIntervention();
            }

            var filterIntervention = new[] { intervention }.AsQueryable().ProjectTo<InterventionDto>(new { districtId = _currentUser.DistrictId }).SingleOrDefault();

            // remove districts current user isn't supposed to see

            // only return district list if a sysadmin is logged in
            if (IsSysAdmin)
            {
                return new OutputDto_Intervention
                {
                    Intervention = filterIntervention //Mapper.Map<InterventionDto>(intervention)
                };
            }
            else
            {
                var district = _loginContext.Districts.First(p => p.Id == _currentUser.DistrictId);
                foreach (var video in filterIntervention.Videos)
                {
                    video.Districts = new List<OutputDto_DropdownData>() { new OutputDto_DropdownData { id = district.Id, text = district.Name } };
                }
                return new OutputDto_Intervention
                {
                    Intervention = filterIntervention //Mapper.Map<InterventionDto>(intervention)
                };
            }
        }


        public OutputDto_PagesList GetPages()
        {
            var pages = _loginContext.NSPages.ToList();

            return new OutputDto_PagesList
            {
                Pages = Mapper.Map<List<PageDto>>(pages)
            };
        }
        public OutputDto_Page GetPageById(InputDto_SimpleId input)
        {
            var page = _loginContext.NSPages
                .Include(p => p.PageTools)
                .Include(p => p.PageVideos)
                .Include(p => p.PagePresentations)
                .FirstOrDefault(p => p.Id == input.Id);

            if (page == null)
            {
                page = new NSPage();
            }
            
            return new OutputDto_Page
            {
                NSPage = Mapper.Map<PageDto>(page) 
            };
        }

        public OutputDto_SuccessAndStatus DeleteIntervention(int id)
        {
            var result = new OutputDto_SuccessAndStatus();

            var intervention = _loginContext.NSInterventions.FirstOrDefault(p => p.Id == id);

            if(intervention != null)
            {
                _loginContext.NSInterventions.Remove(intervention);
                _loginContext.SaveChanges();
            }
            else
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "This intervention does not exist and cannot be deleted.";
            }

            return result;
        }

        public OutputDto_SuccessAndStatus DeletePage(int id)
        {
            var result = new OutputDto_SuccessAndStatus();

            var page = _loginContext.NSPages.FirstOrDefault(p => p.Id == id);

            if (page != null)
            {
                var presentations = _loginContext.PagePresentations.Where(p => p.PageId == page.Id);
                var tools = _loginContext.PageTools.Where(p => p.PageId == page.Id);
                var videos = _loginContext.PageVideos.Where(p => p.PageId == page.Id);
                _loginContext.PagePresentations.RemoveRange(presentations);
                _loginContext.PageTools.RemoveRange(tools);
                _loginContext.PageVideos.RemoveRange(videos);

                _loginContext.NSPages.Remove(page);
                _loginContext.SaveChanges();
            }
            else
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "This page does not exist and cannot be deleted.";
            }

            return result;
        }
        public OutputDto_SuccessAndNewId SavePage(PageDto input)
        {
            var result = new OutputDto_SuccessAndNewId();

            if (String.IsNullOrEmpty(input.Title))
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "Page Title is required.";
                return result;
            }

            var db_page = _loginContext.NSPages.FirstOrDefault(p => p.Id == input.Id);


            if (db_page == null)
            {
                db_page = new NSPage();
                _loginContext.NSPages.Add(db_page);
                _loginContext.SaveChanges();
                result.id = db_page.Id;
            }

            // copy stuff from dto to entity
            input.Id = db_page.Id;
            var useless = Mapper.Map<PageDto, NSPage>(input, db_page);

            _loginContext.SaveChanges();

            return result;
        }

        public OutputDto_SuccessAndNewId SaveIntervention(InterventionDto input)
        {
            var result = new OutputDto_SuccessAndNewId();

            // validate input
            if (input.InterventionTierID == 0 || input.InterventionTierID == null)
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "You must select a Tier";
                return result;
            }
            if (String.IsNullOrEmpty(input.InterventionType))
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "Intervention Abbreviation is required.";
                return result;
            }
            if (String.IsNullOrEmpty(input.Description))
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "Intervention Title is required.";
                return result;
            }

            var db_intervention = _loginContext.NSInterventions.FirstOrDefault(p => p.Id == input.Id);


            if (db_intervention == null)
            {
                db_intervention = new NSIntervention();
                _loginContext.NSInterventions.Add(db_intervention);
                _loginContext.SaveChanges();
                result.id = db_intervention.Id;
            }

            // copy stuff from dto to entity
            var useless = Mapper.Map<InterventionDto, NSIntervention>(input, db_intervention);


            // now do collections
            var interventionGradesToDelete = new List<NSInterventionGrade>();

            db_intervention.InterventionGrades
                    .Where(d => !input.InterventionGrades.Any(ct => ct.id == d.GradeID && db_intervention.Id == d.InterventionTypeId))
                    .Each(deleted => interventionGradesToDelete.Add(deleted));

            if(interventionGradesToDelete.Count > 0)
            {
                _loginContext.NSInterventionGrades.RemoveRange(interventionGradesToDelete);

            }

            //update or add new staffsections
            input.InterventionGrades.Each(ct =>
            {
                // check to see if this is an existing co-teacher record
                var interventionGrade = db_intervention.InterventionGrades.FirstOrDefault(d => d.GradeID == ct.id && d.InterventionTypeId == db_intervention.Id);
                if (interventionGrade == null)
                {
                    interventionGrade = new NSInterventionGrade();
                    db_intervention.InterventionGrades.Add(interventionGrade);
                }
                interventionGrade.GradeID = ct.id;
                interventionGrade.InterventionTypeId = db_intervention.Id;
            });

            _loginContext.SaveChanges();

            return result;
        }

        public List<OutputDto_DropdownData> QuickSearchAssessmentTools(string searchString)
        {
            var tools = _loginContext.NSInterventionTools.Where(p => (p.ToolName.Contains(searchString) || p.Description.Contains(searchString)) && p.InterventionToolTypeId == 1).OrderBy(p => p.ToolName);

            if (tools.Any())
            {
                var results = new List<OutputDto_DropdownData>();

                foreach (var tool in tools)
                {
                    results.Add(new OutputDto_DropdownData()
                    {
                        id = tool.Id,
                        text = tool.ToolName + " (" + tool.Description + ")"
                    });
                }


                return results;
            }
            else
            {
                return new List<OutputDto_DropdownData>();
            }
        }

        public List<OutputDto_DropdownData> QuickSearchInterventionTools(string searchString)
        {
            var tools = _loginContext.NSInterventionTools.Where(p => (p.ToolName.Contains(searchString) || p.Description.Contains(searchString)) && p.InterventionToolTypeId == 2).OrderBy(p => p.ToolName);

            if (tools.Any())
            {
                var results = new List<OutputDto_DropdownData>();

                foreach (var tool in tools)
                {
                    results.Add(new OutputDto_DropdownData()
                    {
                        id = tool.Id,
                        text = tool.ToolName + " (" + tool.Description + ")"
                    });
                }


                return results;
            }
            else
            {
                return new List<OutputDto_DropdownData>();
            }
        }

        public List<OutputDto_DropdownData> QuickSearchPageTools(string searchString)
        {
            var tools = _loginContext.Tools.Where(p => (p.ToolName.Contains(searchString) || p.Description.Contains(searchString))).OrderBy(p => p.ToolName);

            if (tools.Any())
            {
                var results = new List<OutputDto_DropdownData>();

                foreach (var tool in tools)
                {
                    results.Add(new OutputDto_DropdownData()
                    {
                        id = tool.Id,
                        text = tool.ToolName + " (" + tool.Description + ")"
                    });
                }


                return results;
            }
            else
            {
                return new List<OutputDto_DropdownData>();
            }
        }


        public List<OutputDto_DropdownData> QuickSearchPresentations(string searchString)
        {
            var tools = _loginContext.Presentations.Where(p => (p.ToolName.Contains(searchString) || p.Description.Contains(searchString))).OrderBy(p => p.ToolName);

            if (tools.Any())
            {
                var results = new List<OutputDto_DropdownData>();

                foreach (var tool in tools)
                {
                    results.Add(new OutputDto_DropdownData()
                    {
                        id = tool.Id,
                        text = tool.ToolName + " (" + tool.Description + ")"
                    });
                }


                return results;
            }
            else
            {
                return new List<OutputDto_DropdownData>();
            }
        }


        public List<OutputDto_DropdownData> GetInterventions(string searchString)
        {
            var interventions = _loginContext.NSInterventions.Where(p => (p.Description.Contains(searchString) || p.InterventionType.Contains(searchString)) || String.IsNullOrEmpty(searchString)).OrderBy(p => p.Description);

            if (interventions.Any())
            {
                var results = Mapper.Map<List<OutputDto_DropdownData>>(interventions);
                return results;
            }
            else
            {
                return new List<OutputDto_DropdownData>();
            }
        }

        // associate the ID of an existing tool to this intervention
        public OutputDto_Base AssociateTool(InputDto_AssociateToolToIntervention input)
        {
            var result = new OutputDto_Base();

            // see if this tool is already associated, if so, skip it
            var db_interventionToolIntervention = _loginContext.NSInterventionToolInterventions.FirstOrDefault(p => p.InterventionToolId == input.InterventionToolId && p.InterventionTypeId == input.InterventionId);


            if (db_interventionToolIntervention != null)
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "This tool is already associated with this intervention";
                return result;
            }

            // copy stuff from dto to entity
            db_interventionToolIntervention = _loginContext.NSInterventionToolInterventions.Create();
            db_interventionToolIntervention.InterventionToolId = input.InterventionToolId;
            db_interventionToolIntervention.InterventionTypeId = input.InterventionId;
            _loginContext.NSInterventionToolInterventions.Add(db_interventionToolIntervention);

            _loginContext.SaveChanges();

            return result;
        }

        public OutputDto_Base AssociatePageTool(InputDto_AssociateToolToPage input)
        {
            var result = new OutputDto_Base();

            // see if this tool is already associated, if so, skip it
            var db_pageTool = _loginContext.PageTools.FirstOrDefault(p => p.ToolId == input.ToolId && p.PageId == input.PageId);


            if (db_pageTool != null)
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "This tool is already associated with this page";
                return result;
            }

            // copy stuff from dto to entity
            db_pageTool = _loginContext.PageTools.Create();
            db_pageTool.ToolId = input.ToolId;
            db_pageTool.PageId = input.PageId;
            _loginContext.PageTools.Add(db_pageTool);

            _loginContext.SaveChanges();

            return result;
        }

        public OutputDto_Base AssociatePresentation(InputDto_AssociatePresentationToPage input)
        {
            var result = new OutputDto_Base();

            // see if this tool is already associated, if so, skip it
            var db_pagePresentation = _loginContext.PagePresentations.FirstOrDefault(p => p.PresentationId == input.PresentationId && p.PageId == input.PageId);


            if (db_pagePresentation != null)
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "This presentation is already associated with this page";
                return result;
            }

            // copy stuff from dto to entity
            db_pagePresentation = _loginContext.PagePresentations.Create();
            db_pagePresentation.PresentationId = input.PresentationId;
            db_pagePresentation.PageId = input.PageId;
            _loginContext.PagePresentations.Add(db_pagePresentation);

            _loginContext.SaveChanges();

            return result;
        }

        public OutputDto_Base AssociatePageVideo(InputDto_AssociateVideoToPage input)
        {
            var result = new OutputDto_Base();
            var videoStreamId = input.Video.id.ToString();


            // first make sure this video is in the database
            var db_video = _loginContext.Videos.FirstOrDefault(p => p.VideoStreamId == videoStreamId);
            if (db_video == null)
            {
                db_video = _loginContext.Videos.Create();
                db_video.VideoLength = input.Video.duration.ToString();
                //db_video.ModifiedDate = input.Video.createdAt;
                db_video.ThumbnailURL = input.Video.thumbnail;
                db_video.VideoStreamId = videoStreamId;
                db_video.VideoFileName = input.Video.title;
                db_video.VideoName = input.Video.title;
                _loginContext.Videos.Add(db_video);
                _loginContext.SaveChanges();
            }

            // see if this tool is already associated, if so, skip it
            var db_pageVideo = _loginContext.PageVideos.FirstOrDefault(p => p.VideoId == db_video.Id && p.PageId == input.PageId);

            if (db_pageVideo != null)
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "This video is already associated with this page";
                return result;
            }

            // copy stuff from dto to entity
            db_pageVideo = _loginContext.PageVideos.Create();
            db_pageVideo.VideoId = db_video.Id;
            db_pageVideo.PageId = input.PageId;
            _loginContext.PageVideos.Add(db_pageVideo);


            _loginContext.SaveChanges();

            return result;
        }

        public OutputDto_Base AssociateVideo(InputDto_AssociateVideoToIntervention input)
        {
            var result = new OutputDto_Base();
            var videoStreamId = input.Video.id.ToString();


            // first make sure this video is in the database
            var db_video = _loginContext.NSInterventionVideos.FirstOrDefault(p => p.VideoStreamId == videoStreamId);
            if(db_video == null)
            {
                db_video = _loginContext.NSInterventionVideos.Create();
                db_video.VideoLength = input.Video.duration.ToString();
                db_video.ModifiedDate = input.Video.createdAt;
                db_video.ThumbnailURL = input.Video.thumbnail;
                db_video.VideoStreamId = videoStreamId;
                db_video.VideoFileName = input.Video.title;
                db_video.VideoName = input.Video.title;
                _loginContext.NSInterventionVideos.Add(db_video);
                _loginContext.SaveChanges();
            }

            // see if this tool is already associated, if so, skip it
            var db_interventionVideoIntervention = _loginContext.NSInterventionVideoNSInterventions.FirstOrDefault(p => p.InterventionVideoId == db_video.Id && p.InterventionTypeId == input.InterventionId);
            
            if (db_interventionVideoIntervention != null)
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "This video is already associated with this intervention";
                return result;
            }

            // copy stuff from dto to entity
            db_interventionVideoIntervention = _loginContext.NSInterventionVideoNSInterventions.Create();
            db_interventionVideoIntervention.InterventionVideoId = db_video.Id;
            db_interventionVideoIntervention.InterventionTypeId = input.InterventionId;
            _loginContext.NSInterventionVideoNSInterventions.Add(db_interventionVideoIntervention);

            // also make sure at least this district has access so that it shows up to be further modified
            var db_interventionVideoDistrict = _loginContext.NSInterventionVideoDistricts.FirstOrDefault(p => p.InterventionVideoId == db_video.Id && p.DistrictId == _currentUser.DistrictId);

            if(db_interventionVideoDistrict == null)
            {
                db_interventionVideoDistrict = _loginContext.NSInterventionVideoDistricts.Create();
                db_interventionVideoDistrict.InterventionVideoId = db_video.Id;
                db_interventionVideoDistrict.DistrictId = _currentUser.DistrictId;
                _loginContext.NSInterventionVideoDistricts.Add(db_interventionVideoDistrict);
            }

            _loginContext.SaveChanges();

            return result;
        }

        public OutputDto_Base RemoveTool(InputDto_AssociateToolToIntervention input)
        {
            var result = new OutputDto_Base();

            // see if this tool is already associated, if so, skip it
            var db_interventionToolIntervention = _loginContext.NSInterventionToolInterventions.FirstOrDefault(p => p.InterventionToolId == input.InterventionToolId && p.InterventionTypeId == input.InterventionId);


            if (db_interventionToolIntervention == null)
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "This tool has already been removed";
                return result;
            }

            _loginContext.NSInterventionToolInterventions.Remove(db_interventionToolIntervention);

            _loginContext.SaveChanges();

            return result;
        }

        public OutputDto_Base RemovePresentation(InputDto_AssociatePresentationToPage input)
        {
            var result = new OutputDto_Base();

            // see if this tool is already associated, if so, skip it
            var db_pagePresentation = _loginContext.PagePresentations.FirstOrDefault(p => p.PresentationId == input.PresentationId && p.PageId == input.PageId);


            if (db_pagePresentation == null)
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "This presentation has already been removed";
                return result;
            }

            _loginContext.PagePresentations.Remove(db_pagePresentation);

            _loginContext.SaveChanges();

            return result;
        }

        public OutputDto_Base RemovePageTool(InputDto_AssociateToolToPage input)
        {
            var result = new OutputDto_Base();

            // see if this tool is already associated, if so, skip it
            var db_pageTool = _loginContext.PageTools.FirstOrDefault(p => p.ToolId == input.ToolId && p.PageId == input.PageId);


            if (db_pageTool == null)
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "This tool has already been removed";
                return result;
            }

            _loginContext.PageTools.Remove(db_pageTool);

            _loginContext.SaveChanges();

            return result;
        }

        public OutputDto_Base RemoveVideo(InputDto_RemoveVideoFromIntervention input)
        {
            var result = new OutputDto_Base();

            // see if this tool is already associated, if so, skip it
            var db_interventionVideoIntervention = _loginContext.NSInterventionVideoNSInterventions.FirstOrDefault(p => p.InterventionVideoId == input.VideoId && p.InterventionTypeId == input.InterventionId);


            if (db_interventionVideoIntervention == null)
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "This video has already been removed";
                return result;
            }

            _loginContext.NSInterventionVideoNSInterventions.Remove(db_interventionVideoIntervention);

            _loginContext.SaveChanges();

            return result;
        }

        public OutputDto_Base RemovePageVideo(InputDto_RemoveVideoFromPage input)
        {
            var result = new OutputDto_Base();

            // see if this tool is already associated, if so, skip it
            var db_pageVideo = _loginContext.PageVideos.FirstOrDefault(p => p.VideoId == input.VideoId && p.PageId == input.PageId);


            if (db_pageVideo == null)
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "This video has already been removed";
                return result;
            }

            _loginContext.PageVideos.Remove(db_pageVideo);

            _loginContext.SaveChanges();

            return result;
        }

        public OutputDto_Base SaveTool(NSInterventionToolDto input)
        {
            var result = new OutputDto_Base();

            // validate input
            if (String.IsNullOrEmpty(input.ToolName))
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "Tool must have a name";
                return result;
            }

            // if this tool doesn't have an ID, throw an exception
            var db_interventionTool = _loginContext.NSInterventionTools.FirstOrDefault(p => p.Id == input.Id);


            if (db_interventionTool == null)
            {
                result.Status.StatusCode = StatusCode.UnhandledException;
                result.Status.StatusMessage = "This tool does not exist";
                return result;
            }

            // copy stuff from dto to entity
            db_interventionTool.ToolName = input.ToolName;
            db_interventionTool.Description = input.Description;

            _loginContext.SaveChanges();

            return result;
        }

        public OutputDto_Base SavePageTool(ToolDto input)
        {
            var result = new OutputDto_Base();

            // validate input
            if (String.IsNullOrEmpty(input.ToolName))
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "Tool must have a name";
                return result;
            }

            // if this tool doesn't have an ID, throw an exception
            var db_pageTool = _loginContext.Tools.FirstOrDefault(p => p.Id == input.Id);


            if (db_pageTool == null)
            {
                result.Status.StatusCode = StatusCode.UnhandledException;
                result.Status.StatusMessage = "This tool does not exist";
                return result;
            }

            // copy stuff from dto to entity
            db_pageTool.ToolName = input.ToolName;
            db_pageTool.Description = input.Description;

            _loginContext.SaveChanges();

            return result;
        }

        public OutputDto_Base SavePagePresentation(PresentationDto input)
        {
            var result = new OutputDto_Base();

            // validate input
            if (String.IsNullOrEmpty(input.ToolName))
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "Presentation must have a name";
                return result;
            }

            // if this tool doesn't have an ID, throw an exception
            var db_pagePresentation = _loginContext.Presentations.FirstOrDefault(p => p.Id == input.Id);


            if (db_pagePresentation == null)
            {
                result.Status.StatusCode = StatusCode.UnhandledException;
                result.Status.StatusMessage = "This presentation does not exist";
                return result;
            }

            // copy stuff from dto to entity
            db_pagePresentation.ToolName = input.ToolName;
            db_pagePresentation.Description = input.Description;

            _loginContext.SaveChanges();

            return result;
        }

        public OutputDto_Base SavePageVideo(VideoDto input)
        {
            // TODO: I think we need to save the page here too...?

            var result = new OutputDto_Base();

            // validate input
            if (String.IsNullOrEmpty(input.VideoName))
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "Video must have a title";
                return result;
            }

            if (String.IsNullOrEmpty(input.UploadedVideoFile.text))
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "Video must be associated with an uploaded video.";
                return result;
            }

            var db_video = _loginContext.Videos.FirstOrDefault(p => p.Id == input.Id);

            if (db_video == null)
            {
                db_video = _loginContext.Videos.Create();
                _loginContext.Videos.Add(db_video);
                _loginContext.SaveChanges();
            }

            // copy stuff from dto to entity
            db_video.VideoName = input.VideoName;
            db_video.Description = input.Description;
            db_video.VideoFileName = input.UploadedVideoFile.text;
            db_video.VideoStreamId = input.UploadedVideoFile.id.ToString();
                    

            // TODO: security check to make sure that the current user has access to delete these sections
            //var interventionsToDelete = new List<NSInterventionVideoNSIntervention>();

            //// TODO: security check to make sure that the current user has access to delete these sections
            //db_video.InterventionVideoInterventions
            //        .Where(d => !input.Interventions.Any(ct => ct.id == d.Id))
            //        .Each(deleted => interventionsToDelete.Add(deleted));

            //_loginContext.NSInterventionVideoNSInterventions.RemoveRange(interventionsToDelete);

            ////update or add new interventions
            //input.Interventions.Each(ct =>
            //{
            //    // check to see if this is an existing co-teacher record
            //    var interventionVideo = db_video.InterventionVideoInterventions.FirstOrDefault(d => d.Id == ct.id);
            //    if (interventionVideo == null)
            //    {
            //        interventionVideo = new NSInterventionVideoNSIntervention();
            //        interventionVideo.InterventionVideoId = input.Id;
            //        interventionVideo.InterventionTypeId = ct.id;
            //        db_video.InterventionVideoInterventions.Add(interventionVideo);
            //    }
            //});

            _loginContext.SaveChanges();

            return result;
        }


        public OutputDto_Base SaveVideo(NSInterventionVideoDto input)
        {
            var result = new OutputDto_Base();

            // validate input
            if (String.IsNullOrEmpty(input.VideoName))
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "Video must have a title";
                return result;
            }

            if (String.IsNullOrEmpty(input.UploadedVideoFile.text))
            {
                result.Status.StatusCode = StatusCode.UserDisplayableException;
                result.Status.StatusMessage = "Video must be associated with an uploaded video.";
                return result;
            }

            var db_video = _loginContext.NSInterventionVideos.FirstOrDefault(p => p.Id == input.Id);

            if (db_video == null)
            {
                db_video = _loginContext.NSInterventionVideos.Create();
                _loginContext.NSInterventionVideos.Add(db_video);
                _loginContext.SaveChanges();
            }

            // copy stuff from dto to entity
            db_video.VideoName = input.VideoName;
            db_video.Description = input.Description;
            db_video.VideoFileName = input.UploadedVideoFile.text;
            db_video.VideoStreamId = input.UploadedVideoFile.id.ToString();

            // districts and grades
            var videoDistrictsToDelete = new List<NSInterventionVideoDistrict>();

            // TODO: security check to make sure that the current user has access to delete these sections
            if (IsSysAdmin)
            {
                db_video.InterventionVideoDistricts
                    .Where(d => !input.Districts.Any(ct => ct.id == d.Id))
                    .Each(deleted => videoDistrictsToDelete.Add(deleted));
            } else
            {
                db_video.InterventionVideoDistricts
                .Where(dist => dist.DistrictId == Currentuser.DistrictId) // only th district user has access to
                .Where(d => !input.Districts.Any(ct => ct.id == d.Id))
                .Each(deleted => videoDistrictsToDelete.Add(deleted));
            }


            _loginContext.NSInterventionVideoDistricts.RemoveRange(videoDistrictsToDelete);

            //update or add new videodistricts
            input.Districts.Each(ct =>
            {
                // check to see if this is an existing co-teacher record
                var videoDistrict = db_video.InterventionVideoDistricts.FirstOrDefault(d => d.Id == ct.id);
                if (videoDistrict == null)
                {
                    videoDistrict = new NSInterventionVideoDistrict();
                    videoDistrict.InterventionVideoId = input.Id;
                    videoDistrict.DistrictId = ct.id;
                    db_video.InterventionVideoDistricts.Add(videoDistrict);
                }
            });

            // interventions
            var interventionsToDelete = new List<NSInterventionVideoNSIntervention>();

            // TODO: security check to make sure that the current user has access to delete these sections
            db_video.InterventionVideoInterventions
                    .Where(d => !input.Interventions.Any(ct => ct.id == d.Id))
                    .Each(deleted => interventionsToDelete.Add(deleted));

            _loginContext.NSInterventionVideoNSInterventions.RemoveRange(interventionsToDelete);

            //update or add new interventions
            input.Interventions.Each(ct =>
            {
                // check to see if this is an existing co-teacher record
                var interventionVideo = db_video.InterventionVideoInterventions.FirstOrDefault(d => d.Id == ct.id);
                if (interventionVideo == null)
                {
                    interventionVideo = new NSInterventionVideoNSIntervention();
                    interventionVideo.InterventionVideoId = input.Id;
                    interventionVideo.InterventionTypeId = ct.id;
                    db_video.InterventionVideoInterventions.Add(interventionVideo);
                }
            });

            var videoGradesToDelete = new List<InterventionVideoGrade>();

            // TODO: security check to make sure that the current user has access to delete these sections
            db_video.InterventionVideoGrades
                    .Where(d => !input.Grades.Any(ct => ct.id == d.Id))
                    .Each(deleted => videoGradesToDelete.Add(deleted));

            _loginContext.NSInterventionVideoGrades.RemoveRange(videoGradesToDelete);

            //update or add new videodistricts
            input.Grades.Each(ct =>
            {
                // check to see if this is an existing co-teacher record
                var videoGrade = db_video.InterventionVideoGrades.FirstOrDefault(d => d.Id == ct.id);
                if (videoGrade == null)
                {
                    videoGrade = new InterventionVideoGrade();
                    videoGrade.InterventionVideoId = input.Id;
                    videoGrade.GradeId = ct.id;
                    db_video.InterventionVideoGrades.Add(videoGrade);
                }
            });

            _loginContext.SaveChanges();

            return result;
        }


        public List<OutputDto_DropdownData> QuickSearchInterventionGrades(string search)
        {

            //var results = _loginContext.Database.SqlQuery<InterventionGradeDto>(@"EXEC [ns4_QuickSearchInterventionGrades] @searchString, 
            //            @interventionId",
            //    new SqlParameter("searchString", String.IsNullOrEmpty(search) ? DBNull.Value : (object)search),
            //    new SqlParameter("interventionId", interventionId));

            //return results.ToList();

            var grades = _loginContext.NSGrades.Where(p => (p.LongName.Contains(search) || p.ShortName.Contains(search) || String.IsNullOrEmpty(search))).OrderBy(p => p.GradeOrder)
                .Select(p => new OutputDto_DropdownData { id = p.Id, text = p.LongName }).ToList();

            return grades.ToList();
        }

        public List<OutputDto_DropdownData> QuickSearchDistricts(string search)
        {
            var districts = new List<OutputDto_DropdownData>();

            if (IsSysAdmin)
            {
                districts = _loginContext.Districts.Where(p => (p.Name.Contains(search) || String.IsNullOrEmpty(search))).OrderBy(p => p.Name)
                   .Select(p => new OutputDto_DropdownData { id = p.Id, text = p.Name }).ToList();
            } else
            {
                districts = _loginContext.Districts.Where(p => p.Id == _currentUser.DistrictId)
                    .Select(p => new OutputDto_DropdownData { id = p.Id, text = p.Name }).ToList();
            }
            return districts;
        }

        public List<OutputDto_DropdownData> QuickSearchWorkshops(string search)
        {

            var grades = _loginContext.NSInterventionWorkshops.Where(p => (p.WorkshopName.Contains(search) || p.WorkshopDescription.Contains(search) || String.IsNullOrEmpty(search))).OrderBy(p => p.WorkshopName)
                .Select(p => new OutputDto_DropdownData { id = p.Id, text = p.WorkshopName }).ToList();

            return grades.ToList();
        }

        public OutputDto_GetInterventionsByTier GetInterventionsByTier(InputDto_InterventionSearch input)
        {
            var matchingInterventions = _loginContext.NSInterventions.Include("InterventionGrades").Where(p => p.InterventionTierId == input.TierId
            && (p.InterventionGrades.Any(j => j.GradeID == input.GradeId) || input.GradeId == -1)
            && (p.InterventionCategoryId == input.CategoryId || input.CategoryId == -1)
            && (p.InterventionWorkshopId == input.WorkshopId || input.WorkshopId == -1)).OrderBy(p => p.Description).ToList();
            // get all grades
            var grades = _loginContext.NSGrades.OrderBy(p => p.GradeOrder).ToList();

            // get all categories
            var categories = _loginContext.NSInterventionCategories.OrderBy(p => p.CategoryName).ToList();

            // get all groupsizes
            var groupSizes = _loginContext.NSInterventionCardinalities.OrderBy(p => p.CardinalityName).ToList();

            // get all workshops
            var workshops = _loginContext.NSInterventionWorkshops.OrderBy(p => p.WorkshopName).ToList();

            // get all frameworks
            var frameworks = _loginContext.NSInterventionFrameworks.OrderBy(p => p.FrameworkName).ToList();

            // get all units of study
            var units = _loginContext.NSInterventionUnitsOfStudy.OrderBy(p => p.UnitName).ToList();

            var tier = _loginContext.NSInterventionTiers.FirstOrDefault(p => p.Id == input.TierId);

            return new OutputDto_GetInterventionsByTier
            {
                Interventions = Mapper.Map<List<InterventionDto>>(matchingInterventions),
                Grades = Mapper.Map<List<GradeDto>>(grades), // TODO
                Categories = Mapper.Map<List<InterventionCategoryDto>>(categories),
                Workshops = Mapper.Map<List<InterventionWorkshopDto>>(workshops),
                Tier = Mapper.Map<InterventionTierDto>(tier)
            };
        }
    }
}
