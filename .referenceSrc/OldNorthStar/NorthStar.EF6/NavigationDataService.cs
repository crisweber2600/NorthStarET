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

namespace NorthStar.EF6
{
    public class NavigationDataService : NSBaseDataService
    {
        public NavigationDataService(ClaimsIdentity user, string loginConnectionString) : base(user, loginConnectionString)
        {

        }

        /// <summary>
        /// TODO: This design is stupid and wasteful... redo this when time permits... however at
        /// the beginning there will typically be only one or two of these calls per page.  spending
        /// xx hours making directives talk to get down to one call is not a good use of time right now... choose
        /// your battles
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public InputOutputDto_NSHelp GetHelp(InputOutputDto_NSHelp input)
        {
            var existingHelp = _loginContext.HelpPages.FirstOrDefault(p => p.Path == input.path);

            if(existingHelp == null)
            {
                return input;
            } else
            {
                switch (input.field)
                {
                    case "HelpPlaceHolder1":
                        input.text = existingHelp.HelpPlaceHolder1;
                        return input;
                    case "HelpPlaceHolder2":
                        input.text = existingHelp.HelpPlaceHolder2;
                        return input;
                    case "HelpPlaceHolder3":
                        input.text = existingHelp.HelpPlaceHolder3;
                        return input;
                    case "HelpPlaceHolder4":
                        input.text = existingHelp.HelpPlaceHolder4;
                        return input;
                    case "HelpPlaceHolder5":
                        input.text = existingHelp.HelpPlaceHolder5;
                        return input;
                    case "HelpPlaceHolder6":
                        input.text = existingHelp.HelpPlaceHolder6;
                        return input;
                    case "HelpPlaceHolder7":
                        input.text = existingHelp.HelpPlaceHolder7;
                        return input;
                    case "HelpPlaceHolder8":
                        input.text = existingHelp.HelpPlaceHolder8;
                        return input;
                    case "HelpPlaceHolder9":
                        input.text = existingHelp.HelpPlaceHolder9;
                        return input;
                    case "HelpPlaceHolder10":
                        input.text = existingHelp.HelpPlaceHolder10;
                        return input;
                    default:
                        input.text = existingHelp.HelpPlaceHolder1;
                        return input;
                }
            }
        }

        public InputOutputDto_NSHelp SaveHelp(InputOutputDto_NSHelp input)
        {
            var existingHelp = _loginContext.HelpPages.FirstOrDefault(p => p.Path == input.path);

            if (existingHelp == null)
            {
                existingHelp = new EntityDto.LoginDB.Entity.NSPageHelp() { Path = input.path };
                _loginContext.HelpPages.Add(existingHelp);
            }

            switch (input.field)
            {
                case "HelpPlaceHolder1":
                    existingHelp.HelpPlaceHolder1 = input.text;
                    break;
                case "HelpPlaceHolder2":
                    existingHelp.HelpPlaceHolder2 = input.text;
                    break;
                case "HelpPlaceHolder3":
                    existingHelp.HelpPlaceHolder3 = input.text;
                    break;
                case "HelpPlaceHolder4":
                    existingHelp.HelpPlaceHolder4 = input.text;
                    break;
                case "HelpPlaceHolder5":
                    existingHelp.HelpPlaceHolder5 = input.text;
                    break;
                case "HelpPlaceHolder6":
                    existingHelp.HelpPlaceHolder6 = input.text;
                    break;
                case "HelpPlaceHolder7":
                    existingHelp.HelpPlaceHolder7 = input.text;
                    break;
                case "HelpPlaceHolder8":
                    existingHelp.HelpPlaceHolder8 = input.text;
                    break;
                case "HelpPlaceHolder9":
                    existingHelp.HelpPlaceHolder9 = input.text;
                    break;
                case "HelpPlaceHolder10":
                    existingHelp.HelpPlaceHolder10 = input.text;
                    break;
                default:
                    existingHelp.HelpPlaceHolder1 = input.text;
                    break;
            }

            _loginContext.SaveChanges();
            return input;
        }


        public void AddClassDataEntry(NavigationNode parentNode)
        {
            // do some additional checking here, like for schools user has access to, district access, preference list
            var assessments = GetNavigationAssessments().Where(p => p.TestType == 1).ToList();

            foreach (var assessment in assessments)
            {
                if(assessment.DataEntryPages == null)
                {
                    parentNode.children.Add(
                    new NavigationNode
                    {
                        url = "#/section-assessment-resultlist/" + assessment.Id,
                        label = assessment.AssessmentName

                    });
                } else
                {
                    parentNode.children.Add(
                    new NavigationNode
                    {
                        url = "#/" + assessment.DataEntryPages + "/" + assessment.Id,
                        label = assessment.AssessmentName

                    });
                }

            }
        }

        public void BuildNavigation(OutputDto_Navigation rootNode)
        {
            var rootNodes = new List<NavigationNode>();

            // HOME
            var homeNode = new NavigationNode { html = "", iconClasses = "fa fa-home", label = "Home", url = "#/" };
            rootNodes.Add(homeNode);

            // User Settings
            var personalSettingsNode = new NavigationNode { html = "", iconClasses = "fa fa-user", label = "Personal Settings", url = "" };
            rootNodes.Add(personalSettingsNode);
            AddPersonalSettingsNodes(personalSettingsNode);

            var dashboardChildren = new List<NavigationNode>();
            var interventionDashboard = new NavigationNode { html = "", iconClasses = "", label = "Intervention Dashboard", url = "#/ig-dashboard" };
            var studentDashboardNode = new NavigationNode { html = "", iconClasses = "", label = "Student Dashboard", url = "#/student-dashboard" };

            var stateAssessments = GetNavigationAssessments().Where(p => p.TestType == 3).ToList();
            if (stateAssessments.Any(p => p.AssessmentName.StartsWith("MN") || p.AssessmentName.StartsWith("MCA")))
            {
                var schoolMCAAllDashboardNode = new NavigationNode { html = "", iconClasses = "", label = "School MCA Dashboard", url = "#/dashboard-school-mca" };
                var schoolMCAAllPrelimDashboardNode = new NavigationNode { html = "", iconClasses = "", label = "School MCA-Prelim Dashboard", url = "#/dashboard-school-mca-prelim" };
                dashboardChildren.Add(schoolMCAAllDashboardNode);
                dashboardChildren.Add(schoolMCAAllPrelimDashboardNode);
            }

            var observationSummaryNode = new NavigationNode { html = "",  label = "Classroom Dashboard", url = "#/observation-summary-class" };
            var observationSummary2Node = new NavigationNode { html = "", label = "Classroom Dashboard - Filters", url = "#/observation-summary-filtered" };
            var observationSummary3Node = new NavigationNode { html = "", label = "Classroom Dashboard - Multiple Benchmark Dates", url = "#/observation-summary-class-multiple" };
            var observationSummary4Node = new NavigationNode { html = "", label = "Classroom Dashboard - Multiple Benchmark Dates (Multi-Column)", url = "#/observation-summary-class-multiple-columns" };
            dashboardChildren.Add(observationSummary2Node);
            dashboardChildren.Add(observationSummaryNode);
            dashboardChildren.Add(observationSummary3Node);
            dashboardChildren.Add(observationSummary4Node);
            dashboardChildren.Add(studentDashboardNode);
            dashboardChildren.Add(interventionDashboard);



            var dashboardsNode = new NavigationNode { html = "", iconClasses = "fa fa-dashboard", label = "Dashboards", url = "", children = dashboardChildren };


            rootNodes.Add(dashboardsNode);

            // Reports
            var reportSecondLevel = new List<NavigationNode>();
            if (IsAnyGradeAdmin())
            {
                var plcNode = new NavigationNode { html = "", iconClasses = "", label = "PLC Intervention Planning", url = "#/plc-intervention-planning" };
                reportSecondLevel.Add(plcNode);
            }
            var stackedBarGraphGroups = new NavigationNode { html = "", iconClasses = "", label = "Stacked Bar Graphs - Group Detail", url = "#/stackedbargraph-groups" };
            reportSecondLevel.Add(stackedBarGraphGroups);
            var stackedBarGraphGroupDetail = new NavigationNode { html = "", iconClasses = "", label = "Stacked Bar Graphs - Compare Groups", url = "#/stackedbargraph-comparison" };
            reportSecondLevel.Add(stackedBarGraphGroupDetail);
            var reportClassRoomNode = new NavigationNode { html = "", iconClasses = "", label = "Classrooms", url = "" };
            reportSecondLevel.Add(reportClassRoomNode);
            var reportInterventionGroupNode = new NavigationNode { html = "", iconClasses = "", label = "Intervention Groups", url = "" };
            reportSecondLevel.Add(reportInterventionGroupNode);
            var reportNode = new NavigationNode { html = "", iconClasses = "fa fa-bar-chart-o", label = "Reports", url = "", children = reportSecondLevel };
            AddInterventionGroupReports(reportInterventionGroupNode);
            AddClassReports(reportClassRoomNode);
            rootNodes.Add(reportNode);

            // data entry
            var dataEntrySecondLevel = new List<NavigationNode>();
            var dataEntryClassRoomNode = new NavigationNode { html = "", iconClasses = "", label = "Classrooms", url = "" };
            dataEntrySecondLevel.Add(dataEntryClassRoomNode);
            AddClassDataEntry(dataEntryClassRoomNode);
            var dataEntryInterventionGroupNode = new NavigationNode { html = "", iconClasses = "", label = "Intervention Groups", url = "" };
            dataEntrySecondLevel.Add(dataEntryInterventionGroupNode);
            AddInterventionGroupDataEntry(dataEntryInterventionGroupNode);
            var dataEntryNode = new NavigationNode { html = "", iconClasses = "fa fa-pencil", label = "Data Entry", url = "", children = dataEntrySecondLevel };
            rootNodes.Add(dataEntryNode);

            // admin
            var adminNode = new NavigationNode { html = "", iconClasses = "fa fa-cog", label = "Administration", url = "" };
            AddAdminNodes(adminNode);
            rootNodes.Add(adminNode);

            // RTI
            var rtiNode = new NavigationNode { html = "", iconClasses = "fa fa-calendar", label = "RTI", url = "" };
            rtiNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Attend Team Meeting", url = "#/tm-attend-list" });
            rtiNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Manage Team Meetings", url = "#/tm-manage" });
            rootNodes.Add(rtiNode);

            // Intervention Toolkit
            var interventionToolkitNode = new NavigationNode { html = "", iconClasses = "fa fa-briefcase", label = "Intervention Toolkit", url = "" };
            interventionToolkitNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Browse Interventions", url = "#/toolkit-browse" });
            //interventionToolkitNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Manage Interventions", url = "#/toolit-manage-interventions" });
            //interventionToolkitNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Manage Tools", url = "#/toolkit-tools" });
            //interventionToolkitNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Manage Videos", url = "#/toolkit-videos" });
            //interventionToolkitNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Manage Tiers", url = "#/toolkit-tiers" });
            //interventionToolkitNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Manage Categories", url = "#/toolkit-categories" });
            //interventionToolkitNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Manage Units Of Study", url = "#/toolkit-unitsofstudy" });
            //interventionToolkitNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Manage Frameworks", url = "#/toolkit-frameworks" });
            //interventionToolkitNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Manage Workshops", url = "#/toolkit-workshops" });

            rootNodes.Add(interventionToolkitNode);

            // Videos
            var videoNode = new NavigationNode { html = "", iconClasses = "fa fa-video-camera", label = "Videos", url = "" };
            if (_currentUser.IsPowerUser == true)
            {
                videoNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Manage Videos", url = "#/videos-manage" });
            }
            videoNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "View Videos", url = "#/videos-browse" });
            rootNodes.Add(videoNode);

            // Utilities
            var utilityNode = new NavigationNode { html = "", iconClasses = "fa fa-wrench", label = "Utilities", url = "" };

            var exportNode = new NavigationNode { html = "", iconClasses = "fa fa-download", label = "Export", url = "" };
            var importNode = new NavigationNode { html = "", iconClasses = "fa fa-upload", label = "Import", url = "" };
            var batchPrintNode = new NavigationNode { html = "", iconClasses = "fa fa-print", label = "Batch Printing", url = "#/utility-batch-print" };
            utilityNode.children.Add(batchPrintNode);
            utilityNode.children.Add(exportNode);
            utilityNode.children.Add(importNode);

            exportNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Benchmark Assessment Data (All Fields)", url = "#/utility-data-export-all-fields" });
            exportNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Benchmark Assessment Data (Summary Fields)", url = "#/utility-data-export" });
            exportNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Intervention Assessment Data", url = "#/utility-interventiondata-export" });


            if (IsDistrictAdmin)
            {
                exportNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Attendance Data", url = "#/utility-attendance-export" });
                exportNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Student Attributes", url = "#/utility-student-export" });
                exportNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Staff Permissions", url = "#/utility-staff-export" });
            }
            //utilityNode.children.Add(new NavigationNode { html = "", iconClasses = "fa fa-print", label = "Batch Printing", url = "#/utility-batch-print" });
            if (IsDistrictAdmin)
            {
                importNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "State Test Data", url = "#/utility-import-state-test-data" });
            }
            importNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Benchmark Data", url = "#/utility-import-benchmark-data" });
            importNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Intervention Data", url = "#/utility-import-intervention-data" });


            // only admin can do rollover
            if (IsDistrictAdmin)
            {
                // Rollover
                var rolloverNode = new NavigationNode { html = "", iconClasses = "fa fa-rotate-right", label = "Rollover", url = "" };
                rolloverNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Full Roster Rollover", url = "#/utility-rollover" });
                rolloverNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Student Updates", url = "#/utility-student-rollover" });
                rolloverNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Staff Updates", url = "#/utility-teacher-rollover" });
                rootNodes.Add(rolloverNode);
            }

            rootNodes.Add(utilityNode);


            if (IsSysAdmin)
            {
                var saNode = new NavigationNode { html = "", iconClasses = "fa fa-lock", label = "System Administration", url = "" };
                saNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Manage Assessments", url = "#/assessment-list" });
                saNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Syndicate Assessments", url = "#/assessment-syndicate" });
                saNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Log In to Other Districts", url = "#/district-access" });
                rootNodes.Add(saNode);
            }


            // Add System Admin Nodes
            AddSysAdminNodes(rootNodes);

            rootNode.Nodes = rootNodes;
        }

        public List<Assessment> GetNavigationAssessments()
        {
            var response = new List<Assessment>();
            var allAssessmentsICanAccess = _dbContext.Assessments.Where(p => (p.AssessmentIsAvailable.HasValue && p.AssessmentIsAvailable.Value) || (p.AssessmentIsAvailable == null)).ToList();

            // remove any that are removed by the schools
            // get all of the schoolIds that I have access to
            var schoolIds = _dbContext.StaffSchools.Where(p => p.StaffID == _currentUser.Id).Select(p => p.SchoolID).ToList();
            var schoolAssessmentsICantAccess = new List<Assessment>();

            foreach (var districtAccesssibleAssessment in allAssessmentsICanAccess)
            {
                var schoolAssessments = _dbContext.SchoolAssessments.Where(p => schoolIds.Contains(p.SchoolId) && p.AssessmentId == districtAccesssibleAssessment.Id);
                if (schoolAssessments.Count() > 0)
                {
                    if (schoolAssessments.All(p => !p.AssessmentIsAvailable))
                    {
                        schoolAssessmentsICantAccess.Add(districtAccesssibleAssessment);
                    }
                }
            }

            // remove assessments that ALL schools have said are not available
            allAssessmentsICanAccess.RemoveAll(p => schoolAssessmentsICantAccess.Contains(p));

            // remove any that are hidden by the user
            var staffAssessmentsICantAccess = _dbContext.StaffAssessments.Where(p => p.StaffId == _currentUser.Id && !p.AssessmentIsAvailable).Select(p => p.Assessment).ToList();
            allAssessmentsICanAccess.RemoveAll(p => staffAssessmentsICantAccess.Contains(p));

            return allAssessmentsICanAccess;
        }


        private void AddClassReports(NavigationNode parentNode)
        {
            // do some additional checking here, like for schools user has access to, district access, preference list
            var assessments = GetNavigationAssessments().Where(p => p.TestType == 1).ToList();

            // add line graph
            parentNode.children.Add(
            new NavigationNode
            {
                url = "#/linegraph",
                label = "Student Line Graph"

            });

            foreach (var assessment in assessments)
            {
                var assessmentNode = new NavigationNode { label = assessment.AssessmentName };
                parentNode.children.Add(assessmentNode);


                var multiReportSplit = assessment.ClassReportPages == null ? new string[] { } : assessment.ClassReportPages.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);

                // these are just the class reports
                foreach (var splitString in multiReportSplit)
                {
                    // see if the report has a custom name or not
                    var customNameSplit = splitString.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

                    // if it has a custom name, it is the first item in the array
                    if (customNameSplit.Length == 2)
                    {
                        assessmentNode.children.Add(
                        new NavigationNode
                        {
                            url = "#/" + customNameSplit[1] + "/" + assessment.Id,
                            label = customNameSplit[0],
                            children = null
                        });
                    }
                    else
                    {
                        assessmentNode.children.Add(
                        new NavigationNode
                        {
                            url = "#/" + customNameSplit[0] + "/" + assessment.Id,
                            label = "Section Report"
                        });
                    }

                }
                //assessmentNode.html = "<span class='badge badge - blue'>" + assessmentNode.children.Count + "</span>";
            }
            //parentNode.html = "<span class='badge badge - blue'>" + parentNode.children.Count + "</span>";
        }

        private void AddInterventionGroupReports(NavigationNode parentNode)
        {
            // do some additional checking here, like for schools user has access to, district access, preference list
            var assessments = GetNavigationAssessments().Where(p => p.TestType == 2).ToList();
            parentNode.children.Add(
                new NavigationNode
                {
                    url = "#/ig-linegraph",
                    label = "Progress Monitoring Line Graph",
                    iconClasses = "fa fa-paw"

                });

            foreach (var assessment in assessments)
            {
                var assessmentNode = new NavigationNode { label = assessment.AssessmentName };

                // add line graph
                

                var multiReportSplit = assessment.ClassReportPages == null ? new string[] { } : assessment.ClassReportPages.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);

                // if has at least on report
                if(multiReportSplit.Length > 0)
                {
                    parentNode.children.Add(assessmentNode);
                }

                // these are just the class reports
                foreach (var splitString in multiReportSplit)
                {
                    // see if the report has a custom name or not
                    var customNameSplit = splitString.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

                    // if it has a custom name, it is the first item in the array
                    if (customNameSplit.Length == 2)
                    {
                        assessmentNode.children.Add(
                        new NavigationNode
                        {
                            url = "#/" + customNameSplit[1] + "/" + assessment.Id,
                            label = customNameSplit[0],
                            children = null
                        });
                    }
                    else
                    {
                        assessmentNode.children.Add(
                        new NavigationNode
                        {
                            url = "#/" + customNameSplit[0] + "/" + assessment.Id,
                            label = "Intervention Report"
                        });
                    }

                }
                //assessmentNode.html = "<span class='badge badge - blue'>" + assessmentNode.children.Count + "</span>";
            }
            //parentNode.html = "<span class='badge badge - blue'>" + parentNode.children.Count + "</span>";
        }

        private void AddInterventionGroupDataEntry(NavigationNode parentNode)
        {
            // do some additional checking here, like for schools user has access to, district access, preference list
            var assessments = GetNavigationAssessments().Where(p => p.TestType == 2).ToList();

            foreach (var assessment in assessments)
            {
                parentNode.children.Add(
                    new NavigationNode
                    {
                        url = "#/ig-assessment-resultlist/" + assessment.Id,
                        label = assessment.AssessmentName

                    });
            }
        }

        private void AddAdminNodes(NavigationNode adminNode)
        {
            if (IsDistrictAdmin)
            {
                var adminDistrict = new NavigationNode { html = "", iconClasses = "", label = "District", url = "" };
                adminNode.children.Add(adminDistrict);
                adminDistrict.children.Add(new NavigationNode { html = "", iconClasses = "", label = "District Holiday Calendar", url = "#/district-calendar" });
                adminDistrict.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Change Available Assessments", url = "#/district-assessments" });
                adminDistrict.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Set Benchmark Dates", url = "#/district-benchmark-dates" });
                adminDistrict.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Customize District Benchmarks", url = "#/district-benchmarks" });
                adminDistrict.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Customize State Test Benchmarks", url = "#/district-yearlyassessment-benchmarks" });
                adminDistrict.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Customize HFW List", url = "#/district-hfw-list" });
                adminDistrict.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Customize Interventions", url = "#/district-interventions" });
                adminDistrict.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Customize Student Attributes", url = "#/district-student-attributes" });
                adminDistrict.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Consolidate Student Services", url = "#/consolidate-student-services" });
            }

            if (IsAnySchoolAdmin() || IsDistrictAdmin)
            {
                var adminSchool = new NavigationNode { html = "", iconClasses = "", label = "School", url = "" };
                adminNode.children.Add(adminSchool);
                adminSchool.children.Add(new NavigationNode { html = "", iconClasses = "", label = "School Holiday Calendar", url = "#/school-calendar" });
                adminSchool.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Change Available Assessments", url = "#/school-assessments" });
            }

            if (IsAnySchoolAdmin() || IsDistrictAdmin)
            {
                var adminStaff = new NavigationNode { html = "", iconClasses = "", label = "Staff", url = "" };
                adminNode.children.Add(adminStaff);
                adminStaff.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Manage Staff", url = "#/staff-list" });
                adminStaff.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Reset User's Password", url = "#/staff-reset-users-password" });
            }

            var adminSections = new NavigationNode { html = "", iconClasses = "", label = "Sections", url = "" };
            adminNode.children.Add(adminSections);
            adminSections.children.Add(new NavigationNode { html = "", iconClasses = "fa fa-gears", label = "Manage Sections", url = "#/section-list" });

            if (IsAnySchoolAdmin() || IsDistrictAdmin)
            {
                var adminStudent = new NavigationNode { html = "", iconClasses = "", label = "Students", url = "" };
                adminNode.children.Add(adminStudent);
                adminStudent.children.Add(new NavigationNode { html = "", iconClasses = "fa fa-gears", label = "Manage Students", url = "#/student-list" });
                adminStudent.children.Add(new NavigationNode { html = "", iconClasses = "fa fa-truck", label = "Move Students", url = "#/student-move" });
            }


            var adminInterventionGroup = new NavigationNode { html = "", iconClasses = "", label = "Intervention Groups", url = "" };
            adminNode.children.Add(adminInterventionGroup);
            adminInterventionGroup.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Manage Intervention Groups", url = "#/ig-manage" });
            adminInterventionGroup.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Manage Intervention Attendance", url = "#/ig-attendance" });
            //adminInterventionGroup.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Change Student Intervention Start/End Date", url = "#/ig-startend" });
            //adminInterventionGroup.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Intervention Group Attendance Summary", url = "#/ig-attendance-summary" });
            //adminInterventionGroup.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Intervention Attendance Export", url = "#/ig-attendance-export" });
            adminInterventionGroup.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Intervention Group Dashboard", url = "#/ig-dashboard" });

            if (IsSysAdmin)
            {
                var rolloverHeading = new NavigationNode { html = "", iconClasses = "", label = "Rollover", url = "" };
                adminNode.children.Add(rolloverHeading);
                rolloverHeading.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Rollover Data", url = "#/rollover-data" });
                rolloverHeading.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Rollover Staff", url = "#/rollover-staff" });
            }
        }

        private void AddPersonalSettingsNodes(NavigationNode personalSettingsNode)
        {
            personalSettingsNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Change Password", url = "#/user-password" });
            personalSettingsNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Change Username", url = "#/user-username" });
            personalSettingsNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Change Profile Info", url = "#/user-profile" });
            personalSettingsNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Manage Available Assessments", url = "#/user-assessments" });
            //personalSettingsNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Manage Available Assessment Fields", url = "#/user-assessment-fields" });
            personalSettingsNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Contact District Administrator", url = "#/user-contact-district-admin" });
            personalSettingsNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Contact School Administrator", url = "#/user-contact-school-admin" });
            if (IsDistrictAdmin)
            {
                personalSettingsNode.children.Add(new NavigationNode { html = "", iconClasses = "", label = "Contact North Star Support", url = "#/user-contact-northstar-support" });
            }
        }

        private void AddSysAdminNodes(List<NavigationNode> rootNodes)
        {
            // if SysAdmin
        }
    }
}
