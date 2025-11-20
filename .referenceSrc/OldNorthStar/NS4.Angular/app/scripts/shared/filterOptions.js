(function () {
    'use strict'

    angular
	.module('filterOptionsModule', [])
	.directive('nsFilterOptionsContainerDirective', [
			'$routeParams', '$compile', '$templateCache', '$http', 'nsFilterOptionsService', '$filter', 'nsSelect2RemoteOptions', function ($routeParams, $compile, $templateCache, $http, nsFilterOptionsService, $filter, nsSelect2RemoteOptions) {


			    return {
			        restrict: 'E',
			        templateUrl: 'templates/global-filter-options.html',
			        scope: {
			            schoolYearEnabled: '=',
			            schoolEnabled: '=',
			            gradeEnabled: '=',
			            teacherEnabled: '=',
                        interventionistEnabled: '=',
			            sectionEnabled: '=',
                        interventionGroupEnabled: '=',
                        sectionStudentEnabled: '=',
                        interventionStudentEnabled: '=',
			            benchmarkDateEnabled: '=',
			            verticalMode: '=',
			            schoolYearRequired: '=',
                        benchmarkDateRequired: '=',
			            schoolRequired: '=',
			            hrsFormEnabled: '=',
			            hrsForm2Enabled: '=',
			            hrsForm3Enabled: '=',
			            staffQuickSearchEnabled: '=',
                        showHr: '=',
			            studentQuickSearchEnabled: '=',
                        studentDetailedQuickSearchEnabled: '=',
                        quickSearchCallback: '=',
                        stintEnabled: '=',
                        teamMeetingEnabled: '=',
                        teamMeetingStaffEnabled: '=',
                        hfwRangeEnabled: '=',
                        hfwSortOrderEnabled: '=',
                        hfwMultiRangeEnabled: '=',
                        stateTestEnabled: '=',
                        benchmarkTestEnabled: '=',
                        interventionTestEnabled: '=',
                        staffSearchCustomLabel: '=',
                        classroomAssessmentFieldEnabled: '=',
                        interventionGroupAssessmentFieldEnabled: '=',
			            multiBenchmarkDateEnabled: '='
			        },
			        link: function (scope, element, attr) {
			            scope.filterOptions = nsFilterOptionsService.options;
			            nsFilterOptionsService.options.multiBenchmarkDateEnabled = scope.multiBenchmarkDateEnabled;
			            nsFilterOptionsService.options.schoolYearEnabled = scope.schoolYearEnabled;
			            nsFilterOptionsService.options.benchmarkDateEnabled = scope.benchmarkDateEnabled;
			            nsFilterOptionsService.options.schoolEnabled = scope.schoolEnabled;
			            nsFilterOptionsService.options.gradeEnabled = scope.gradeEnabled;
			            nsFilterOptionsService.options.teacherEnabled = scope.teacherEnabled;
			            nsFilterOptionsService.options.interventionistEnabled = scope.interventionistEnabled;
			            nsFilterOptionsService.options.sectionEnabled = scope.sectionEnabled;
			            nsFilterOptionsService.options.interventionGroupEnabled = scope.interventionGroupEnabled;
			            nsFilterOptionsService.options.sectionStudentEnabled = scope.sectionStudentEnabled;
			            nsFilterOptionsService.options.interventionStudentEnabled = scope.interventionStudentEnabled;
			            nsFilterOptionsService.options.hrsFormEnabled = scope.hrsFormEnabled;
			            nsFilterOptionsService.options.hrsForm2Enabled = scope.hrsForm2Enabled;
			            nsFilterOptionsService.options.hrsForm3Enabled = scope.hrsForm3Enabled;
			            nsFilterOptionsService.options.staffQuickSearchEnabled = scope.staffQuickSearchEnabled;
			            nsFilterOptionsService.options.showHr = scope.showHr;
			            nsFilterOptionsService.options.studentQuickSearchEnabled = scope.studentQuickSearchEnabled;
			            nsFilterOptionsService.options.studentDetailedQuickSearchEnabled = scope.studentDetailedQuickSearchEnabled;
			            nsFilterOptionsService.options.stintEnabled = scope.stintEnabled;
			            nsFilterOptionsService.options.teamMeetingEnabled = scope.teamMeetingEnabled;
			            nsFilterOptionsService.options.teamMeetingStaffEnabled = scope.teamMeetingStaffEnabled;
			            nsFilterOptionsService.options.hfwRangeEnabled = scope.hfwRangeEnabled;
			            nsFilterOptionsService.options.hfwSortOrderEnabled = scope.hfwSortOrderEnabled;
			            nsFilterOptionsService.options.hfwMultiRangeEnabled = scope.hfwMultiRangeEnabled;
			            nsFilterOptionsService.options.stateTestEnabled = scope.stateTestEnabled;
			            nsFilterOptionsService.options.benchmarkTestEnabled = scope.benchmarkTestEnabled;
			            nsFilterOptionsService.options.interventionTestEnabled = scope.interventionTestEnabled;
			            nsFilterOptionsService.options.classroomAssessmentFieldEnabled = scope.classroomAssessmentFieldEnabled;
			            nsFilterOptionsService.options.interventionGroupAssessmentFieldEnabled = scope.interventionGroupAssessmentFieldEnabled;

                        // determine which method to call
			            if (angular.isDefined($routeParams.stint)) {
			                nsFilterOptionsService.stintOverride();
			            } else if (angular.isDefined($routeParams.teamMeeting)) {
			                nsFilterOptionsService.teamMeetingOverride();
			            } else {
			                nsFilterOptionsService.loadOptions();

			            }

			            scope.HfwMultiRangeRemoteOptions = nsSelect2RemoteOptions.HfwMultiRangeRemoteOptions;
			            scope.StaffQuickSearchRemoteOptions = nsSelect2RemoteOptions.StaffQuickSearchRemoteOptions;
			            scope.StudentQuickSearchRemoteOptions = nsSelect2RemoteOptions.StudentQuickSearchRemoteOptions;
			            scope.StudentDetailedQuickSearchRemoteOptions = nsSelect2RemoteOptions.StudentDetailedQuickSearchRemoteOptions;

			            scope.qsCallBack = function () {
			                if (angular.isDefined(scope.quickSearchCallback)) {
			                    scope.quickSearchCallback();
			                } else {
			                    return;
			                }
			            }

			            scope.clearQsStudent = function () {
			                nsFilterOptionsService.options.quickSearchStudent = null;
			                //scope.quickSearchCallback();
			            }

			            scope.clearQsStaff = function () {
			                nsFilterOptionsService.options.quickSearchStaff = null;
			                //scope.quickSearchCallback();
			            }


			            scope.changeClassroomAssessmentField = function () {
			                nsFilterOptionsService.changeClassroomAssessmentField();
			            };
			            scope.changeInterventionGroupAssessmentField = function () {
			                nsFilterOptionsService.changeInterventionGroupAssessmentField();
			            };
			            scope.changeSchool = function () {
			                nsFilterOptionsService.changeSchool();
			            };
			            scope.changeSchoolYear = function () {
			                nsFilterOptionsService.changeSchoolYear();
			            };
			            scope.changeGrade = function () {
			                nsFilterOptionsService.changeGrade();
			            };
			            scope.changeTeacher = function () {
			                nsFilterOptionsService.changeTeacher();
			            };
			            scope.changeInterventionist = function () {
			                nsFilterOptionsService.changeInterventionist();
			            };
			            scope.changeTeacher = function () {
			                nsFilterOptionsService.changeTeacher();
			            };
			            scope.changeSection = function () {
			                nsFilterOptionsService.changeSection();
			            };
			            scope.changeSectionStudent = function () {
			                nsFilterOptionsService.changeSectionStudent();
			            };
			            scope.changeInterventionStudent = function () {
			                nsFilterOptionsService.changeInterventionStudent();
			            };
			            scope.changeHfwMultiRange = function () {
			                nsFilterOptionsService.changeHfwMultiRange();
			            };
			            scope.changeHfwSortOrder = function () {
			                nsFilterOptionsService.changeHfwSortOrder();
			            };
			            scope.changeTeamMeeting = function () {
			                nsFilterOptionsService.changeTeamMeeting();
			            };
			            scope.changeInterventionGroup = function () {
			                nsFilterOptionsService.changeInterventionGroup();
			            };
			            scope.changeHrsForm = function () {
			                nsFilterOptionsService.changeHrsForm();
			            };
			            scope.changeHrsForm2 = function () {
			                nsFilterOptionsService.changeHrsForm2();
			            };
			            scope.changeHrsForm3 = function () {
			                nsFilterOptionsService.changeHrsForm3();
			            };

			            scope.changeMultiBenchmarkDates = function (oldval, newval) {
			                if (JSON.stringify(oldval) != JSON.stringify(newval)) {
			                    nsFilterOptionsService.changeMultiBenchmarkDates();
			                }
			            }
			        }
			    };
			}
	])
    .service('nsFilterOptionsService', [
			'$http', '$location', '$q', '$routeParams', 'webApiBaseUrl', '$rootScope', function ($http, $location, $q, $routeParams, webApiBaseUrl, $rootScope) {
			    var self = this;
			    self.initialLoadComplete = false;

			    self.normalizeParameter = function (parameter) {
			        if (parameter !== null && angular.isDefined(parameter)) {
			            return parameter.id;
			        }
			        else {
			            return -1;
			        }
			    }

			    self.createReturnObject = function (options) {
			        var returnObject = {
			            SchoolId: self.normalizeParameter(options.selectedSchool),
			            SchoolEnabled: options.schoolEnabled,
			            GradeId: self.normalizeParameter(options.selectedGrade),
			            GradeEnabled: options.gradeEnabled,
			            TeacherId: self.normalizeParameter(options.selectedTeacher),
			            TeacherEnabled: options.teacherEnabled,
			            InterventionistId: self.normalizeParameter(options.selectedInterventionist),
			            InterventionistEnabled: options.interventionistEnabled,
			            SectionId: self.normalizeParameter(options.selectedSection),
			            SectionEnabled: options.sectionEnabled,
			            InterventionGroupId: self.normalizeParameter(options.selectedInterventionGroup),
			            InterventionGroupEnabled: options.interventionGroupEnabled,
			            SectionStudentId: self.normalizeParameter(options.selectedSectionStudent),
			            SectionStudentEnabled: options.sectionStudentEnabled,
			            InterventionStudentId: self.normalizeParameter(options.selectedInterventionStudent),
			            ClassroomAssessmentFieldId: self.normalizeParameter(options.selectedClassroomAssessmentField),
			            InterventionGroupAssessmentFieldId: self.normalizeParameter(options.selectedInterventionGroupAssessmentField),
			            InterventionStudentEnabled: options.interventionStudentEnabled,
			            SchoolYear: self.normalizeParameter(options.selectedSchoolYear),// options.selectedSchoolYear != null ? options.selectedSchoolYear.SchoolStartYear : -1,
			            SchoolYearEnabled: options.schoolYearEnabled,
			            BenchmarkDateId: self.normalizeParameter(options.selectedBenchmarkDate),
			            BenchmarkDateEnabled: options.benchmarkDateEnabled,
			            HRSFormEnabled: options.hrsFormEnabled,
			            HRSForm2Enabled: options.hrsForm2Enabled,
			            HRSForm3Enabled: options.hrsForm3Enabled,
			            HRSFormId: self.normalizeParameter(options.selectedHrsForm),
			            HRSForm2Id: self.normalizeParameter(options.selectedHrsForm2),
			            HRSForm3Id: self.normalizeParameter(options.selectedHrsForm3),
			            StintId: self.normalizeParameter(options.selectedStint),
			            StintEnabled: options.stintEnabled,
			            TeamMeetingId: self.normalizeParameter(options.selectedTeamMeeting),
			            TeamMeetingEnabled: options.teamMeetingEnabled,
			            TeamMeetingStaffId: self.normalizeParameter(options.selectedTeamMeetingStaff),
			            TeamMeetingStaffEnabled: options.teamMeetingStaffEnabled,
			            HFWRangeEnabled: options.hfwRangeEnabled,
			            HFWRange: options.selectedHfwRange, // string
			            HFWSortOrderEnabled: options.hfwSortOrderEnabled,
			            HFWMultiRangeEnabled: options.hfwMultiRangeEnabled,
			            HFWSortOrder: options.selectedHfwSortOrder != null ? options.selectedHfwSortOrder.id : 'alphabetic',
			            HFWMultiRange: options.selectedHfwMultiRange,
			            MultiBenchmarkDates: options.selectedBenchmarkDates,
			            StateTestEnabled: options.stateTestEnabled,
			            BenchmarkTestEnabled: options.benchmarkTestEnabled,
			            InterventionTestEnabled: options.interventionTestEnabled,
			            ClassroomAssessmentFieldEnabled: options.classroomAssessmentFieldEnabled,
			            InterventionGroupAssessmentFieldEnabled: options.interventionGroupAssessmentFieldEnabled
			        };

			        return returnObject;
			    }

			    self.options = {};

			    self.options.teacherEnabled = false;
			    self.options.teachers = [];
			    self.options.gradeEnabled = false;
			    self.options.grades = [];
			    self.options.sectionEnabled = false;
			    self.options.sections = [];
			    self.options.studentEnabled = false;
			    self.options.sectionStudents = [];
			    self.options.interventionStudents = [];
			    self.options.schoolEnabled = false;
			    self.options.schools = [];
			    self.options.schoolYearEnabled = false;
			    self.options.schoolYears = [];
			    self.options.benchmarkDateEnabled = false;
			    self.options.benchmarkDates = [];
			    self.options.multiYearBenchmarkDates = [];
			    self.options.interventionGroupEnabled = false;
			    self.options.interventionGroups = [];
			    self.options.interventionistEnabled = false;
			    self.options.interventionists = [];
			    self.options.classroomAssessmentFields = [];
			    self.options.interventionGroupAssessmentFields = [];
			    self.options.hrsFormEnabled = false;
			    self.options.hrsForm2Enabled = false;
			    self.options.hrsForm3Enabled = false;
			    self.options.hrsForms = [];
			    self.options.hrsForms2 = [];
			    self.options.hrsForms3 = [];
			    self.options.hfwOrders = [{ id: "alphabetic", text: "Alphabetic Order" }, { id: "teaching", text: "Teaching Order" }];
			    self.options.stints = [];
			    self.options.stateTests = [];
			    self.options.benchmarkTests = [];
			    self.options.interventionTests = [];
			    self.options.stintEnabled = false;
			    self.options.teamMeetings = [];
			    self.options.teamMeetingEnabled = false;
			    self.options.teamMeetingStaffs = [];
			    self.options.teamMeetingStaffEnabled = false;
			    self.options.hfwRangeEnabled = false;
			    self.options.hfwSortOrderEnabled = false;
			    self.options.stateTestEnabled = false;
			    self.options.benchmarkTestEnabled = false;
			    self.options.interventionTestEnabled = false;
			    self.options.classroomAssessmentFieldEnabled = false;
			    self.options.interventionGroupAssessmentFieldEnabled = false;
               

			    self.options.selectedGrade = null;
			    self.options.selectedTeacher = null;
			    self.options.selectedInterventionist = null;
			    self.options.selectedSection = null;
			    self.options.selectedInterventionGroup = null;
			    self.options.selectedSectionStudent = null;
			    self.options.selectedInterventionStudent = null;
			    self.options.selectedSchool = null;
			    self.options.selectedSchoolYear = null;
			    self.options.selectedBenchmarkDate = null;
			    self.options.selectedBenchmarkDates = [];
			    self.options.selectedHrsForm = null;
			    self.options.selectedHrsForm2 = null;
			    self.options.selectedHrsForm3 = null;
			    self.options.selectedStint = null
			    self.options.selectedTeamMeeting = null;
			    self.options.selectedTeamMeetingStaff = null;
			    self.options.selectedHfwRange = null;
			    self.options.selectedHfwMultiRange = null;
			    self.options.selectedHfwSortOrder = null;
			    self.options.selectedStateTest = null;
			    self.options.selectedBenchmarkTest = null;
			    self.options.selectedInterventionTest = null;
			    self.options.selectedClassroomAssessmentField = null;
			    self.options.selectedInterventionGroupAssessmentField = null;

		

			    self.getBenchmarkDateById = function (id) {
			        var theBenchmarkDate = null;
			        for (var i = 0; i < self.options.benchmarkDates.length; i++) {
			            if (self.options.benchmarkDates[i].id == id) {
			                self.options.benchmarkDates[i].active = true;
			                theBenchmarkDate = self.options.benchmarkDates[i];
			            }
			            else {
			                self.options.benchmarkDates[i].active = false;
			            }
			        }
			        return theBenchmarkDate;
			    }

			    self.getMultiYearBenchmarkDateById = function (id) {
			        var theBenchmarkDate = null;
			        for (var i = 0; i < self.options.multiYearBenchmarkDates.length; i++) {
			            if (self.options.multiYearBenchmarkDates[i].id == id) {
			                self.options.multiYearBenchmarkDates[i].active = true;
			                theBenchmarkDate = self.options.multiYearBenchmarkDates[i];
			            }
			            else {
			                self.options.multiYearBenchmarkDates[i].active = false;
			            }
			        }
			        return theBenchmarkDate;
			    }

			    self.loadOptions = function () {

			        var returnObject = self.createReturnObject(self.options);
			        returnObject.ChangeType = "initial";
			        returnObject.SchoolEnabled = true;
			        returnObject.SchoolYearEnabled = true;
			        returnObject.BenchmarkDateEnabled = true;

			        return $http.post(webApiBaseUrl + "/api/assessment/GetUpdatedFilterOptions", returnObject).then(function (response) {
			            loadClassroomAssessmentFields(response);
			            loadInterventionGroupAssessmentFields(response);
			            reloadSchoolYears(response);
			            reloadSchools(response);
			            reloadGrades(response);
			            reloadTeachers(response);
			            reloadSections(response);
			            reloadSectionStudents(response);
			            reloadInterventionists(response);
			            reloadInterventionGroups(response);
			            reloadInterventionStudents(response);
			            reloadStints(response);
			            reloadBenchmarkDates(response);
			            reloadTeamMeetings(response);
			            reloadTeamMeetingStaffs(response);
			            loadHrsForms(response);
			            loadHrsForms2(response);
			            loadHrsForms3(response);
			            loadHfwRanges(response);
			            loadHfwSortOrder(response);
			            loadStateTests(response);
			            loadInterventionTests(response);
			            loadBenchmarkTests(response);
			            loadMultiTdds(response);


			            // now override what was loaded with any URL params
			            // if there is a printmode url param... skip all this
			            if ($location.absUrl().indexOf('printmode=') >= 0) {
			                if (angular.isDefined($location.$$search.Teacher)) {
			                    self.options.selectedTeacher = JSON.parse(decodeURIComponent($location.$$search.Teacher));
			                }
			                if (angular.isDefined($location.$$search.SchoolYear)) {
			                    self.options.selectedSchoolYear = JSON.parse(decodeURIComponent($location.$$search.SchoolYear));
			                }
			                if (angular.isDefined($location.$$search.Section)) {
			                    self.options.selectedSection = JSON.parse(decodeURIComponent($location.$$search.Section));
			                }
			                if (angular.isDefined($location.$$search.Student)) {
			                    self.options.selectedSectionStudent = JSON.parse(decodeURIComponent($location.$$search.Student));
			                }
			                if (angular.isDefined($location.$$search.Grade)) {
			                    self.options.selectedGrade = JSON.parse(decodeURIComponent($location.$$search.Grade));
			                }
			                if (angular.isDefined($location.$$search.School)) {
			                    self.options.selectedSchool = JSON.parse(decodeURIComponent($location.$$search.School));
			                }
			                if (angular.isDefined($location.$$search.BenchmarkDate)) {
			                    var searchDate = JSON.parse(decodeURIComponent($location.$$search.BenchmarkDate));
			                    self.options.selectedBenchmarkDate = getSelectedItemFromCollection(searchDate.id, self.options.benchmarkDates);
			                    if (self.options.selectedBenchmarkDate != null && angular.isDefined(self.options.selectedBenchmarkDate)) {
			                        self.options.selectedBenchmarkDate.active = true;
			                    }
			                }
			                if (angular.isDefined($location.$$search.HfwRanges)) {
			                    self.options.selectedHfwMultiRange = JSON.parse(decodeURIComponent($location.$$search.HfwRanges));
			                }
			            }

			            // bencmarkdates
			            self.options.select2MultiBenchmarkDateOptions = {
			                minimumInputLength: 0,
			                data: self.options.multiYearBenchmarkDates,
			                multiple: true,
			                width: 'resolve',
			            };

                        // TODO: reload all them
			            //self.initialLoadComplete = true;
			            $rootScope.$broadcast("NSInitialLoadComplete", true);
			        });
			    }

			    var loadHrsForms = function (response) {
			        if (self.options.hrsFormEnabled) {
			            self.options.hrsForms.splice(0, self.options.hrsForms.length);
			            self.options.hrsForms.push.apply(self.options.hrsForms, response.data.HRSForms);
			            if (response.data.SelectedHRSForm != 0) {
			                self.options.selectedHrsForm = getSelectedItemFromCollection(response.data.SelectedHRSForm, self.options.hrsForms);
			            }
			        }
			    }

			    var loadHrsForms2 = function (response) {
			        if (self.options.hrsForm2Enabled) {
			            self.options.hrsForms2.splice(0, self.options.hrsForms2.length);
			            self.options.hrsForms2.push.apply(self.options.hrsForms2, response.data.HRSForms2);
			            if (response.data.SelectedHRSForm2 != 0) {
			                self.options.selectedHrsForm2 = getSelectedItemFromCollection(response.data.SelectedHRSForm2, self.options.hrsForms2);
			            }
			        }
			    }

			    var loadHrsForms3 = function (response) {
			        if (self.options.hrsForm3Enabled) {
			            self.options.hrsForms3.splice(0, self.options.hrsForms3.length);
			            self.options.hrsForms3.push.apply(self.options.hrsForms3, response.data.HRSForms3);
			            if (response.data.SelectedHRSForm3 != 0) {
			                self.options.selectedHrsForm3 = getSelectedItemFromCollection(response.data.SelectedHRSForm3, self.options.hrsForms3);
			            }
			        }
			    }

			    var loadMultiTdds = function (response) {
			        if (1 == 1) {
			            self.options.selectedBenchmarkDates.splice(0, self.options.selectedBenchmarkDates.length);
			            self.options.selectedBenchmarkDates.push.apply(self.options.selectedBenchmarkDates, response.data.SelectedTestDueDates);
			        }
			    }

			    var loadStateTests = function (response) {
			        if (self.options.stateTestEnabled) {
			            self.options.stateTests.splice(0, self.options.stateTests.length);
			            self.options.stateTests.push.apply(self.options.stateTests, response.data.StateTests);
			        }
			    }
			    var loadBenchmarkTests = function (response) {
			        if (self.options.benchmarkTestEnabled) {
			            self.options.benchmarkTests.splice(0, self.options.benchmarkTests.length);
			            self.options.benchmarkTests.push.apply(self.options.benchmarkTests, response.data.BenchmarkTests);
			        }
			    }
			    var loadInterventionTests = function (response) {
			        if (self.options.interventionTestEnabled) {
			            self.options.interventionTests.splice(0, self.options.interventionTests.length);
			            self.options.interventionTests.push.apply(self.options.interventionTests, response.data.InterventionTests);
			        }
			    }


			    var loadHfwRanges = function (response) {
			        if (self.options.hfwMultiRangeEnabled) {
			            self.options.selectedHfwMultiRange = response.data.SelectedHFWMultiRange;
			        }
			    }

			    var loadHfwSortOrder = function (response) {
			        if (self.options.hfwSortOrderEnabled) {
			            if (response.data.SelectedHFWSortOrder == null) {
			                self.options.selectedHfwSortOrder = self.options.hfwOrders[0];
			            } else {
			                self.options.selectedHfwSortOrder = getSelectedItemFromCollection(response.data.SelectedHFWSortOrder, self.options.hfwOrders);
			            }
			        }
			    }

                // pick up here!!!  figure out field vs assessment and FLATTENING
			    var loadClassroomAssessmentFields = function (response) {
			        if (self.options.classroomAssessmentFieldEnabled) {
			            // flatten response data
			            self.options.classroomAssessmentFields.splice(0, self.options.classroomAssessmentFields.length);
			            self.options.classroomAssessmentFields.push.apply(self.options.classroomAssessmentFields, self.flatten(response.data.ClassroomAssessmentFields));

			            if (response.data.SelectedClassroomAssessmentField != null) {
			                self.options.selectedClassroomAssessmentField = getSelectedItemFromCollection(response.data.SelectedClassroomAssessmentField, self.options.classroomAssessmentFields);
			            } 
			        }
			    }

			    var loadInterventionGroupAssessmentFields = function (response) {
			        if (self.options.interventionGroupAssessmentFieldEnabled) {
			            self.options.interventionGroupAssessmentFields.splice(0, self.options.interventionGroupAssessmentFields.length);
			            self.options.interventionGroupAssessmentFields.push.apply(self.options.interventionGroupAssessmentFields, self.flatten(response.data.InterventionGroupAssessmentFields));

			            if (response.data.SelectedInterventionGroupAssessmentField != null) {
			                self.options.selectedInterventionGroupAssessmentField = getSelectedItemFromCollection(response.data.SelectedInterventionGroupAssessmentField, self.options.interventionGroupAssessmentFields);
			            }
			        }
			    }

			    self.studentOverride = function () {

			    }

			    self.stintOverride = function () {
			        // if this is a stint override, create returnobject based on URL params
			        var paramObj = {
			            SchoolId: $routeParams.school,
			            SchoolEnabled: true,
			            GradeId: -1,
			            GradeEnabled: false,
			            TeacherId: -1,
			            TeacherEnabled: false,
			            InterventionistId: $routeParams.interventionist,
			            InterventionistEnabled: true,
			            SectionId: -1,
			            SectionEnabled: false,
			            InterventionGroupId: $routeParams.interventiongroup,
			            InterventionGroupEnabled:true,
			            InterventionStudentId: $routeParams.student,
			            InterventionStudentEnabled: true,
			            SectionStudentId: -1,
			            SectionStudentEnabled: false,
			            SchoolYear: $routeParams.schoolyear,
			            SchoolYearEnabled: true,
			            BenchmarkDateId: -1,
			            BenchmarkDateEnabled: false,
			            HRSFormEnabled: false,
			            HRSForm2Enabled: false,
			            HRSForm3Enabled: false,
			            StintId: $routeParams.stint,
			            StintEnabled: true,
			            TeamMeetingId: -1,
			            TeamMeetingEnabled: false,
			            TeamMeetingStaffId: -1,
                        TeamMeetingStaffEnabled: false
			        }
			        paramObj.ChangeType = "stintOverride";

			        return $http.post(webApiBaseUrl + "/api/assessment/GetUpdatedFilterOptions", paramObj).then(function (response) {
			            reloadSchoolYears(response);
			            reloadSchools(response);
			            reloadInterventionists(response);
			            reloadInterventionGroups(response);
			            reloadInterventionStudents(response);
			            reloadStints(response);

			            // TODO: reload all them
			            self.initialLoadComplete = true;

			        });
			    }

			    self.teamMeetingOverride = function () {
			        // if this is a teammeeting override, create returnobject based on URL params
			        var paramObj = {
			            SchoolId: -1,
			            SchoolEnabled: true,
			            GradeId: -1,
			            GradeEnabled: false,
			            TeacherId: -1,
			            TeacherEnabled: false,
			            InterventionistId: -1,
			            InterventionistEnabled: true,
			            SectionId: -1,
			            SectionEnabled: false,
			            InterventionGroupId: -1,
			            InterventionGroupEnabled: true,
			            InterventionStudentId: -1,
			            InterventionStudentEnabled: true,
			            SectionStudentId: -1,
			            SectionStudentEnabled: false,
			            SchoolYear: $routeParams.schoolyear,
			            SchoolYearEnabled: true,
			            BenchmarkDateId: -1,
			            BenchmarkDateEnabled: false,
			            HRSFormEnabled: false,
			            HRSForm2Enabled: false,
			            StintId: $routeParams.stint,
			            StintEnabled: true,
			            TeamMeetingId: $routeParams.teamMeeting,
			            TeamMeetingEnabled: true,
			            TeamMeetingStaffId: -1,
			            TeamMeetingStaffEnabled: true
			        }
			        paramObj.ChangeType = "teammeeting";

			        return $http.post(webApiBaseUrl + "/api/assessment/GetUpdatedFilterOptions", paramObj).then(function (response) {
			            reloadTeamMeetingStaffs(response);

			            // TODO: reload all them
			            self.initialLoadComplete = true;

			        });
			    }

			    

			    self.loadClassroomAssessmentFieldChange = function (options) {
			        var returnObject = self.createReturnObject(options);
			        returnObject.ChangeType = "classroomassessmentfield";

			        return $http.post(webApiBaseUrl + "/api/assessment/GetUpdatedFilterOptions", returnObject).then(function (response) {
			            $rootScope.$broadcast("NSClassroomAssessmentFieldOptionsUpdated", true);

			            return response.data;
			        });
			    }

			    self.loadInterventionGroupAssessmentFieldChange = function (options) {
			        var returnObject = self.createReturnObject(options);
			        returnObject.ChangeType = "interventiongroupassessmentfield";

			        return $http.post(webApiBaseUrl + "/api/assessment/GetUpdatedFilterOptions", returnObject).then(function (response) {
			            $rootScope.$broadcast("NSInterventionGroupAssessmentFieldOptionsUpdated", true);

			            return response.data;
			        });
			    }

			    self.flatten = function (data) {
			        var out = [];
			        angular.forEach(data, function (d) {
			            angular.forEach(d.Fields, function (v) {
			                out.push({
			                    AssessmentName: d.AssessmentName,
			                    DisplayLabel: v.DisplayLabel,
			                    FieldName: v.DatabaseColumn,
			                    LookupFieldName: v.LookupFieldName,
			                    AssessmentId: d.Id,
			                    FieldType: v.FieldType,
			                    RangeHigh: v.RangeHigh,
			                    RangeLow: v.RangeLow,
			                    DisplayInLineGraphs: v.DisplayInLineGraphs,
                                id: v.Id
			                })
			            })
			        })
			        return out;
			    }

			    self.loadSchoolChange = function (options) {
			        var returnObject = self.createReturnObject(options);
			        returnObject.ChangeType = "school";

			        return $http.post(webApiBaseUrl + "/api/assessment/GetUpdatedFilterOptions", returnObject).then(function (response) {
			            reloadGrades(response);
			            reloadTeachers(response);
			            reloadInterventionists(response);
			            reloadSections(response);
			            reloadInterventionGroups(response);
			            reloadSectionStudents(response);
			            reloadInterventionStudents(response);

			            $rootScope.$broadcast("NSSchoolOptionsUpdated", true);

			            return response.data;
			        });
			    }
			    self.loadGradeChange = function (options) {
			        var returnObject = self.createReturnObject(options);
			        returnObject.ChangeType = "grade";

			        return $http.post(webApiBaseUrl + "/api/assessment/GetUpdatedFilterOptions", returnObject).then(function (response) {
			            reloadTeachers(response);
			            reloadSections(response);
			            reloadSectionStudents(response);

			            $rootScope.$broadcast("NSGradeOptionsUpdated", true);

			            return response.data;
			        });
			    }

			    self.loadTeacherChange = function (options) {
			        var returnObject = self.createReturnObject(options);
			        returnObject.ChangeType = "teacher";

			        return $http.post(webApiBaseUrl + "/api/assessment/GetUpdatedFilterOptions", returnObject).then(function (response) {
			            reloadSections(response);
			            reloadSectionStudents(response);

                        // TODO: determine if this is necessary, and correct
			            options.selectedGrade = getSelectedItemFromCollection(response.data.SelectedGrade, options.grades);

			            $rootScope.$broadcast("NSTeacherOptionsUpdated", true);

			            return response.data;
			        });
			    }

			    self.loadInterventionistChange = function (options) {
			        var returnObject = self.createReturnObject(options);
			        returnObject.ChangeType = "interventionist";

			        return $http.post(webApiBaseUrl + "/api/assessment/GetUpdatedFilterOptions", returnObject).then(function (response) {
			            reloadInterventionGroups(response);
			            reloadInterventionStudents(response);

			            $rootScope.$broadcast("NSInterventionistOptionsUpdated", true);

			            return response.data;
			        });
			    }

			    self.loadSectionChange = function (options) {
			        var returnObject = self.createReturnObject(options);
			        returnObject.ChangeType = "section";

			        return $http.post(webApiBaseUrl + "/api/assessment/GetUpdatedFilterOptions", returnObject).then(function (response) {
			            reloadSectionStudents(response);

			            $rootScope.$broadcast("NSSectionOptionsUpdated", true);

			            return response.data;
			        });
			    }

			    self.loadSectionStudentChange = function (options) {
			            var returnObject = self.createReturnObject(options);
			            returnObject.ChangeType = "sectionstudent";

			            return $http.post(webApiBaseUrl + "/api/assessment/GetUpdatedFilterOptions", returnObject).then(function (response) {
			                $rootScope.$broadcast("NSSectionStudentOptionsUpdated", true);

			                return response.data;
			            });
			    }

			    self.loadChangeHfwMultiRange = function (options) {
			        var returnObject = self.createReturnObject(options);
			        returnObject.ChangeType = "hfwmultirange";

			        return $http.post(webApiBaseUrl + "/api/assessment/GetUpdatedFilterOptions", returnObject).then(function (response) {
			            $rootScope.$broadcast("NSHfwMultiRangeOptionsUpdated", true);

			            return response.data;
			        });
			    }

			    self.loadChangeMultiBenchmarkDates = function (options) {
			        var returnObject = self.createReturnObject(options);
			        returnObject.ChangeType = "tddmultirange";

			        return $http.post(webApiBaseUrl + "/api/assessment/GetUpdatedFilterOptions", returnObject).then(function (response) {
			            $rootScope.$broadcast("NSTddMultiRangeOptionsUpdated", true);

			            return response.data;
			        });
			    }

			    self.loadChangeHfwSortOrder = function (options) {
			        var returnObject = self.createReturnObject(options);
			        returnObject.ChangeType = "hfwsortorder";

			        return $http.post(webApiBaseUrl + "/api/assessment/GetUpdatedFilterOptions", returnObject).then(function (response) {
			            $rootScope.$broadcast("NSHfwSortOrderOptionsUpdated", true);

			            return response.data;
			        });
			    }

			    self.loadInterventionStudentChange = function (options) {
			        // only do anything for studentchange if loading stints
			        var returnObject = self.createReturnObject(options);
			        returnObject.ChangeType = "interventionstudent";

			        return $http.post(webApiBaseUrl + "/api/assessment/GetUpdatedFilterOptions", returnObject).then(function (response) {
			            reloadStints(response);
			            $rootScope.$broadcast("NSInterventionStudentOptionsUpdated", true);

			            return response.data;
			        });
			    }

			    self.loadTeamMeetingChange = function (options) {
			        // only do anything for studentchange if loading stints
			        var returnObject = self.createReturnObject(options);
			        returnObject.ChangeType = "teammeeting";

			        return $http.post(webApiBaseUrl + "/api/assessment/GetUpdatedFilterOptions", returnObject).then(function (response) {
			            reloadTeamMeetingStaffs(response);
			            $rootScope.$broadcast("NSTeamMeetingOptionsUpdated", true);

			            return response.data;
			        });
			    }

			    self.loadHrsFormChange = function (options) {
			        // only do anything for studentchange if loading stints
			        var returnObject = self.createReturnObject(options);
			        returnObject.ChangeType = "hrsform";

			        return $http.post(webApiBaseUrl + "/api/assessment/GetUpdatedFilterOptions", returnObject).then(function (response) {
			            $rootScope.$broadcast("NSHrsFormOptionsUpdated", true);

			            return response.data;
			        });
			    }

			    self.loadHrsForm2Change = function (options) {
			        // only do anything for studentchange if loading stints
			        var returnObject = self.createReturnObject(options);
			        returnObject.ChangeType = "hrsform2";

			        return $http.post(webApiBaseUrl + "/api/assessment/GetUpdatedFilterOptions", returnObject).then(function (response) {
			            $rootScope.$broadcast("NSHrsForm2OptionsUpdated", true);

			            return response.data;
			        });
			    }

			    self.loadHrsForm3Change = function (options) {
			        // only do anything for studentchange if loading stints
			        var returnObject = self.createReturnObject(options);
			        returnObject.ChangeType = "hrsform3";

			        return $http.post(webApiBaseUrl + "/api/assessment/GetUpdatedFilterOptions", returnObject).then(function (response) {
			            $rootScope.$broadcast("NSHrsForm3OptionsUpdated", true);

			            return response.data;
			        });
			    }

			    self.loadBenchmarkDateChange = function (options) {
			        var returnObject = self.createReturnObject(options);
			        returnObject.ChangeType = "benchmarkdate";

                    // nothing to update, this is just to save the setting
			        return $http.post(webApiBaseUrl + "/api/assessment/GetUpdatedFilterOptions", returnObject).then(function (response) {
			            $rootScope.$broadcast("NSBenchmarkDateOptionsUpdated", true);
			            return response.data;
			        });
			    }

			    self.loadInterventionGroupChange = function (options) {
			        var returnObject = self.createReturnObject(options);
			        returnObject.ChangeType = "interventiongroup";

			        return $http.post(webApiBaseUrl + "/api/assessment/GetUpdatedFilterOptions", returnObject).then(function (response) {
			            reloadInterventionStudents(response);
			            reloadStints(response);

			            $rootScope.$broadcast("NSInterventionGroupOptionsUpdated", true);

			            return response.data;
			        });
			    }
			    self.loadSchoolYearChange = function (options) {
			        var returnObject = self.createReturnObject(options);
			        returnObject.ChangeType = "schoolyear";

			        return $http.post(webApiBaseUrl + "/api/assessment/GetUpdatedFilterOptions", returnObject).then(function (response) {
			            reloadGrades(response);
			            reloadBenchmarkDates(response);
			            reloadTeachers(response);
			            reloadInterventionists(response);
			            reloadInterventionGroups(response);
			            reloadInterventionStudents(response);
			            reloadStints(response);
			            reloadSections(response)
			            reloadSectionStudents(response);
			            reloadTeamMeetings(response);
			            reloadTeamMeetingStaffs(response);

			            $rootScope.$broadcast("NSSchoolYearOptionsUpdated", true);
			            // check for routeparam with benchmarkdateid
			            if (typeof $routeParams.benchmarkDateId !== 'undefined' && self.options.selectedBenchmarkDate.id !== null && self.options.selectedBenchmarkDate.id !== -1) {
			                var index = $location.path().lastIndexOf('/');
			                $location.path($location.path().substring(0, index) + '/' + self.options.selectedBenchmarkDate.id);
			            }

			            return response.data;
			        });
			    }

			    function reloadBenchmarkDates(response) {
			        self.options.benchmarkDates.splice(0, self.options.benchmarkDates.length);
			        self.options.benchmarkDates.push.apply(self.options.benchmarkDates, response.data.TestDueDates);
			        self.options.selectedBenchmarkDate = getSelectedItemFromCollection(response.data.SelectedTDD, self.options.benchmarkDates);
			        if (self.options.selectedBenchmarkDate != null && angular.isDefined(self.options.selectedBenchmarkDate)) {
			            self.options.selectedBenchmarkDate.active = true;
			        }

			        self.options.multiYearBenchmarkDates.splice(0, self.options.multiYearBenchmarkDates.length);
			        self.options.multiYearBenchmarkDates.push.apply(self.options.multiYearBenchmarkDates, response.data.MultiYearTestDueDates);
			    }

			    function reloadSchoolYears(response) {
			        self.options.schoolYears.splice(0, self.options.schoolYears.length);
			        self.options.schoolYears.push.apply(self.options.schoolYears, response.data.SchoolYears);
			        self.options.selectedSchoolYear = getSelectedItemFromCollection(response.data.SelectedSchoolStartYear, self.options.schoolYears);
			    }

			    function reloadSchools(response) {
			        self.options.schools.splice(0, self.options.schools.length);
			        self.options.schools.push.apply(self.options.schools, response.data.Schools);
			        self.options.selectedSchool = getSelectedItemFromCollection(response.data.SelectedSchool, self.options.schools);
			    }

			    function reloadGrades(response) {
			        self.options.grades.splice(0, self.options.grades.length);
			        self.options.grades.push.apply(self.options.grades, response.data.Grades);
			        self.options.selectedGrade = getSelectedItemFromCollection(response.data.SelectedGrade, self.options.grades);
			    }

			    function reloadTeachers(response) {
			        self.options.teachers.splice(0, self.options.teachers.length);
			        self.options.teachers.push.apply(self.options.teachers, response.data.Teachers);
			        self.options.selectedTeacher = getSelectedItemFromCollection(response.data.SelectedTeacher, self.options.teachers);
			    }

			    function reloadSections(response) {
			        self.options.sections.splice(0, self.options.sections.length);
			        self.options.sections.push.apply(self.options.sections, response.data.Sections);
			        self.options.selectedSection = getSelectedItemFromCollection(response.data.SelectedSection, self.options.sections);
			    }

			    function reloadInterventionGroups(response) {
			        self.options.interventionGroups.splice(0, self.options.interventionGroups.length)
			        self.options.interventionGroups.push.apply(self.options.interventionGroups, response.data.InterventionGroups);
			        self.options.selectedInterventionGroup = getSelectedItemFromCollection(response.data.SelectedInterventionGroup, self.options.interventionGroups);
			    }

			    function reloadInterventionists(response) {
			        self.options.interventionists.splice(0, self.options.interventionists.length);
			        self.options.interventionists.push.apply(self.options.interventionists, response.data.Interventionists);
			        self.options.selectedInterventionist = getSelectedItemFromCollection(response.data.SelectedInterventionist, self.options.interventionists);
			    }

			    function reloadInterventionStudents(response) {
			        self.options.interventionStudents.splice(0, self.options.interventionStudents.length);
			        self.options.interventionStudents.push.apply(self.options.interventionStudents, response.data.InterventionStudents);
			        self.options.selectedInterventionStudent = getSelectedItemFromCollection(response.data.SelectedInterventionStudent, self.options.interventionStudents);
			    }
			    function reloadSectionStudents(response) {
			        self.options.sectionStudents.splice(0, self.options.sectionStudents.length);
			        self.options.sectionStudents.push.apply(self.options.sectionStudents, response.data.SectionStudents);
			        self.options.selectedSectionStudent = getSelectedItemFromCollection(response.data.SelectedSectionStudent, self.options.sectionStudents);
			    }

			    function reloadStints(response) {
			        self.options.stints.splice(0, self.options.stints.length);
			        self.options.stints.push.apply(self.options.stints, response.data.Stints);
			        self.options.selectedStint = getSelectedItemFromCollection(response.data.SelectedStint, self.options.stints);
			    }

			    function reloadTeamMeetings(response) {
			        self.options.teamMeetings.splice(0, self.options.teamMeetings.length);
			        self.options.teamMeetings.push.apply(self.options.teamMeetings, response.data.TeamMeetings);
			        self.options.selectedTeamMeeting = getSelectedItemFromCollection(response.data.SelectedTeamMeeting, self.options.teamMeetings);
			    }

			    function reloadTeamMeetingStaffs(response) {
			        self.options.teamMeetingStaffs.splice(0, self.options.teamMeetingStaffs.length);
			        self.options.teamMeetingStaffs.push.apply(self.options.teamMeetingStaffs, response.data.TeamMeetingStaffs);
			        self.options.selectedTeamMeetingStaff = getSelectedItemFromCollection(response.data.SelectedTeamMeetingStaff, self.options.teamMeetingStaffs);
			    }

			    function getSelectedItemFromCollection(id, collection) {
			        for (var i = 0; i < collection.length; i++) {
			            if (collection[i].id == id) {
			                return collection[i];
			            }
			        }
			        return null;
			    }

			    function getSelectedStringFromCollection(item, collection) {
			        for (var i = 0; i < collection.length; i++) {
			            if (collection[i] == item) {
			                return collection[i];
			            }
			        }
			        return null;
			    }

			    self.changeSchool = function () {
			        return self.loadSchoolChange(self.options);
			    }
			    self.changeClassroomAssessmentField = function () {
			        return self.loadClassroomAssessmentFieldChange(self.options);
			    }
			    self.changeInterventionGroupAssessmentField = function () {
			        return self.loadInterventionGroupAssessmentFieldChange(self.options);
			    }
			    self.changeGrade = function () {
			        return self.loadGradeChange(self.options);
			    }
			    self.changeTeacher = function () {
			        return self.loadTeacherChange(self.options);
			    }
			    self.changeInterventionist = function () {
			        return self.loadInterventionistChange(self.options);
			    }
			    self.changeSection = function () {
			        return self.loadSectionChange(self.options);
			    }
			    self.changeSectionStudent = function () {
			        return self.loadSectionStudentChange(self.options);
			    }
			    self.changeInterventionStudent = function () {
			        return self.loadInterventionStudentChange(self.options);
			    }
			    self.changeHfwMultiRange = function () {
			        return self.loadChangeHfwMultiRange(self.options);
			    }
			    self.changeMultiBenchmarkDates = function () {
			        return self.loadChangeMultiBenchmarkDates(self.options);
			    }
			    self.changeHfwSortOrder = function () {
			        return self.loadChangeHfwSortOrder(self.options);
			    }
			    self.changeTeamMeeting = function () {
			        return self.loadTeamMeetingChange(self.options);
			    }
			    self.changeInterventionGroup = function () {
			        return self.loadInterventionGroupChange(self.options);
			    }
			    self.changeSchoolYear = function () {
			        return self.loadSchoolYearChange(self.options);
			    }
			    self.changeBenchmarkDate = function () {
			        return self.loadBenchmarkDateChange(self.options);
			    }
			    self.changeHrsForm = function () {
			        return self.loadHrsFormChange(self.options);
			    }
			    self.changeHrsForm2 = function () {
			        return self.loadHrsForm2Change(self.options);
			    }
			    self.changeHrsForm3 = function () {
			        return self.loadHrsForm3Change(self.options);
			    }

			}]
	)


})();