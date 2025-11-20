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
			            studentEnabled: '=',
			            benchmarkDateEnabled: '=',
			            verticalMode: '=',
			            schoolYearRequired: '=',
                        benchmarkDateRequired: '=',
			            schoolRequired: '=',
			            hrsFormEnabled: '=',
			            staffQuickSearchEnabled: '=',
			            studentQuickSearchEnabled: '=',
                        studentDetailedQuickSearchEnabled: '=',
                        quickSearchCallback: '=',
                        stintEnabled: '=',
                        teamMeetingEnabled: '=',
                        teamMeetingStaffEnabled: '='
			        },
			        link: function (scope, element, attr) {
			            scope.filterOptions = nsFilterOptionsService.options;
			            nsFilterOptionsService.options.schoolYearEnabled = scope.schoolYearEnabled;
			            nsFilterOptionsService.options.benchmarkDateEnabled = scope.benchmarkDateEnabled;
			            nsFilterOptionsService.options.schoolEnabled = scope.schoolEnabled;
			            nsFilterOptionsService.options.gradeEnabled = scope.gradeEnabled;
			            nsFilterOptionsService.options.teacherEnabled = scope.teacherEnabled;
			            nsFilterOptionsService.options.interventionistEnabled = scope.interventionistEnabled;
			            nsFilterOptionsService.options.sectionEnabled = scope.sectionEnabled;
			            nsFilterOptionsService.options.interventionGroupEnabled = scope.interventionGroupEnabled;
			            nsFilterOptionsService.options.studentEnabled = scope.studentEnabled;
			            nsFilterOptionsService.options.hrsFormEnabled = scope.hrsFormEnabled;
			            nsFilterOptionsService.options.staffQuickSearchEnabled = scope.staffQuickSearchEnabled;
			            nsFilterOptionsService.options.studentQuickSearchEnabled = scope.studentQuickSearchEnabled;
			            nsFilterOptionsService.options.studentDetailedQuickSearchEnabled = scope.studentDetailedQuickSearchEnabled;
			            nsFilterOptionsService.options.stintEnabled = scope.stintEnabled;
			            nsFilterOptionsService.options.teamMeetingEnabled = scope.teamMeetingEnabled;
			            nsFilterOptionsService.options.teamMeetingStaffEnabled = scope.teamMeetingStaffEnabled;

                        // determine which method to call
			            if (angular.isDefined($routeParams.stint)) {
			                nsFilterOptionsService.stintOverride();
			            } else {
			                nsFilterOptionsService.loadOptions();

			            }

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
			            scope.changeStudent = function () {
			                nsFilterOptionsService.changeStudent();
			            };
			            scope.changeTeamMeeting = function () {
			                nsFilterOptionsService.changeTeamMeeting();
			            };
			            scope.changeInterventionGroup = function () {
			                nsFilterOptionsService.changeInterventionGroup();
			            };
			        }
			    };
			}
	])
    .service('nsFilterOptionsService', [
			'$http', '$location', '$q', '$routeParams', 'webApiBaseUrl', '$rootScope', function ($http, $location, $q, $routeParams, webApiBaseUrl, $rootScope) {
			    var self = this;
			    self.initialLoadComplete = false;

			    self.normalizeParameter = function (parameter) {
			        if (parameter !== null) {
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
			            StudentId: self.normalizeParameter(options.selectedStudent),
			            StudentEnabled: options.studentEnabled,
			            SchoolYear: self.normalizeParameter(options.selectedSchoolYear),// options.selectedSchoolYear != null ? options.selectedSchoolYear.SchoolStartYear : -1,
			            SchoolYearEnabled: options.schoolYearEnabled,
			            BenchmarkDateId: self.normalizeParameter(options.selectedBenchmarkDate),
			            BenchmarkDateEnabled: options.benchmarkDateEnabled,
			            HRSFormEnabled: options.hrsFormEnabled,
			            StintId: self.normalizeParameter(options.selectedStint),
			            StintEnabled: options.stintEnabled,
			            TeamMeetingId: self.normalizeParameter(options.selectedTeamMeeting),
			            TeamMeetingEnabled: options.teamMeetingEnabled,
			            TeamMeetingStaffId: self.normalizeParameter(options.selectedTeamMeetingStaff),
			            TeamMeetingStaffEnabled: options.teamMeetingStaffEnabled
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
			    self.options.students = [];
			    self.options.schoolEnabled = false;
			    self.options.schools = [];
			    self.options.schoolYearEnabled = false;
			    self.options.schoolYears = [];
			    self.options.benchmarkDateEnabled = false;
			    self.options.benchmarkDates = [];
			    self.options.interventionGroupEnabled = false;
			    self.options.interventionGroups = [];
			    self.options.interventionistEnabled = false;
			    self.options.interventionists = [];
			    self.options.hrsFormEnabled = false;
			    self.options.hrsForms = [];
			    self.options.stints = [];
			    self.options.stintEnabled = false;
			    self.options.teamMeetings = [];
			    self.options.teamMeetingEnabled = false;
			    self.options.teamMeetingStaffs = [];
			    self.options.teamMeetingStaffEnabled = false;

			    self.options.selectedGrade = null;
			    self.options.selectedTeacher = null;
			    self.options.selectedInterventionist = null;
			    self.options.selectedSection = null;
			    self.options.selectedInterventionGroup = null;
			    self.options.selectedStudent = null;
			    self.options.selectedSchool = null;
			    self.options.selectedSchoolYear = null;
			    self.options.selectedBenchmarkDate = null;
			    self.options.selectedHrsForm = null;
			    self.options.selectedStint = null
			    self.options.selectedTeamMeeting = null;
			    self.options.selectedTeamMeetingStaff = null;

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

			    self.loadOptions = function () {
                    // only do this once
			        if (self.initialLoadComplete) {
			            return;
			        }
			        var returnObject = self.createReturnObject(self.options);
			        returnObject.ChangeType = "initial";
			        returnObject.SchoolEnabled = true;
			        returnObject.SchoolYearEnabled = true;
			        returnObject.BenchmarkDateEnabled = true;

			        return $http.post(webApiBaseUrl + "/api/assessment/GetUpdatedFilterOptions", returnObject).then(function (response) {
			            reloadSchools(response);
			            reloadSchoolYears(response);
			            reloadBenchmarkDates(response);
			            reloadTeamMeetings(response);
			            loadHrsForms(response);

                        // TODO: reload all them
			            self.initialLoadComplete = true;

			            // Read any passed in options from RouteParams and set them
			            //self.setUrlOverrides();
			        });
			    }

			    var loadHrsForms = function (response) {
			        if (self.options.hrsFormEnabled) {
			            self.options.hrsForms.splice(0, self.options.hrsForms.length);
			            self.options.hrsForms.push.apply(self.options.hrsForms, response.data.HRSForms);
			        }
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
			            StudentId: $routeParams.student,
			            StudentEnabled: true,
			            SchoolYear: $routeParams.schoolyear,
			            SchoolYearEnabled: true,
			            BenchmarkDateId: -1,
			            BenchmarkDateEnabled: false,
			            HRSFormEnabled: false,
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
			            reloadStudents(response);
			            reloadStints(response);

			            // TODO: reload all them
			            self.initialLoadComplete = true;

			        });
			    }

			    //self.schoolYearOverride = function () {
			    //    //schoolyear
			    //    if (angular.isDefined($location.$$search.SchoolYear) && !isNaN($location.$$search.SchoolYear)) {
			    //        for (var i = 0; i < self.options.schoolYears.length; i++) {
			    //            if (self.options.schoolYears[i].SchoolStartYear == $location.$$search.SchoolYear) {
			    //                self.options.selectedSchoolYear = self.options.schoolYears[i];
			    //                return self.changeSchoolYear().then(function (response) { self.schoolOverride() });
			    //            }
			    //        }
			    //    }
			    //    return $q.resolve(1);
			    //};
			    //self.schoolOverride = function () {
			    //    //school
			    //    if (angular.isDefined($location.$$search.SchoolId) && !isNaN($location.$$search.SchoolId)) {
			    //        for (var i = 0; i < self.options.schools.length; i++) {
			    //            if (self.options.schools[i].id == $location.$$search.SchoolId) {
			    //                self.options.selectedSchool = self.options.schools[i];
			    //                return self.changeSchool().then(function (response) { self.gradeOverride() });
			    //            }
			    //        }
			    //    }
			    //    return $q.resolve(1);
			    //};
			    //self.gradeOverride = function () {
			    //    //grade
			    //    if (angular.isDefined($location.$$search.GradeId) && !isNaN($location.$$search.GradeId)) {
			    //        for (var i = 0; i < self.options.grades.length; i++) {
			    //            if (self.options.grades[i].id == $location.$$search.GradeId) {
			    //                self.options.selectedGrade = self.options.grades[i];
			    //                return self.changeGrade().then(function (response) { self.teacherOverride() });
			    //            }
			    //        }
			    //    }
			    //    return $q.resolve(1);
			    //};
			    //self.teacherOverride = function () {
			    //    //teacher
			    //    if (angular.isDefined($location.$$search.TeacherId) && !isNaN($location.$$search.TeacherId)) {
			    //        for (var i = 0; i < self.options.teachers.length; i++) {
			    //            if (self.options.teachers[i].id == $location.$$search.TeacherId) {
			    //                self.options.selectedTeacher = self.options.teachers[i];
			    //                return self.changeTeacher().then(function (response) { self.sectionOverride() });
			    //            }
			    //        }
			    //    }
			    //    return $q.resolve(1);
			    //};
			    //self.sectionOverride = function () {
			    //    //section
			    //    if (angular.isDefined($location.$$search.SectionId) && !isNaN($location.$$search.SectionId)) {
			    //        for (var i = 0; i < self.options.sections.length; i++) {
			    //            if (self.options.sections[i].id == $location.$$search.SectionId) {
			    //                self.options.selectedSection = self.options.sections[i];
			    //                return self.changeSection().then(function (response) { self.studentOverride() });
			    //            }
			    //        }
			    //    }
			    //    return $q.resolve(1);
			    //};
			    //self.studentOverride = function () {
			    //    //student
			    //    if (angular.isDefined($location.$$search.StudentId) && !isNaN($location.$$search.StudentId)) {
			    //        for (var i = 0; i < self.options.students.length; i++) {
			    //            if (self.options.students[i].id == $location.$$search.StudentId) {
			    //                self.options.selectedStudent = self.options.students[i];
			    //                return $q.resolve();
			    //            }
			    //        }
			    //    }
			    //    return $q.resolve(1);
			    //};

			    //self.setUrlOverrides = function () {
			    //    // start with the chained ones
			    //    self.schoolYearOverride().then(
                //        function () { },
                //        function (error) { alert('something bad'); });
			    //    // now do the unchained ones
			    //    // list of tests, etc
			    //}

			    //self.loadOptions(self.options);



			    self.loadSchoolChange = function (options) {
			        var returnObject = self.createReturnObject(options);
			        returnObject.ChangeType = "school";

			        return $http.post(webApiBaseUrl + "/api/assessment/GetUpdatedFilterOptions", returnObject).then(function (response) {
			            reloadGrades(response);
			            reloadTeachers(response);
			            reloadInterventionists(response);
			            reloadSections(response);
			            reloadInterventionGroups(response);
			            reloadStudents(response);

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
			            reloadStudents(response);

			            $rootScope.$broadcast("NSGradeOptionsUpdated", true);

			            return response.data;
			        });
			    }

			    self.loadTeacherChange = function (options) {
			        var returnObject = self.createReturnObject(options);
			        returnObject.ChangeType = "teacher";

			        return $http.post(webApiBaseUrl + "/api/assessment/GetUpdatedFilterOptions", returnObject).then(function (response) {
			            reloadSections(response);
			            reloadStudents(response);

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
			            reloadStudents(response);

			            $rootScope.$broadcast("NSInterventionistOptionsUpdated", true);

			            return response.data;
			        });
			    }

			    self.loadSectionChange = function (options) {
			        var returnObject = self.createReturnObject(options);
			        returnObject.ChangeType = "section";

			        return $http.post(webApiBaseUrl + "/api/assessment/GetUpdatedFilterOptions", returnObject).then(function (response) {
			            reloadStudents(response);

			            $rootScope.$broadcast("NSSectionOptionsUpdated", true);

			            return response.data;
			        });
			    }

			    self.loadStudentChange = function (options) {
                    // only do anything for studentchange if loading stints
			        if (options.stintEnabled) {
			            var returnObject = self.createReturnObject(options);
			            returnObject.ChangeType = "student";

			            return $http.post(webApiBaseUrl + "/api/assessment/GetUpdatedFilterOptions", returnObject).then(function (response) {
			                reloadStints(response);
			                $rootScope.$broadcast("NSStudentOptionsUpdated", true);

			                return response.data;
			            });
			        }
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

			    self.loadInterventionGroupChange = function (options) {
			        var returnObject = self.createReturnObject(options);
			        returnObject.ChangeType = "interventiongroup";

			        return $http.post(webApiBaseUrl + "/api/assessment/GetUpdatedFilterOptions", returnObject).then(function (response) {
			            reloadStudents(response);

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
			            reloadSections(response)
			            reloadStudents(response);

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
			        self.options.selectedBenchmarkDate.active = true;
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

			    function reloadStudents(response) {
			        self.options.students.splice(0, self.options.students.length);
			        self.options.students.push.apply(self.options.students, response.data.Students);
			        self.options.selectedStudent = getSelectedItemFromCollection(response.data.SelectedStudent, self.options.students);
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

			    self.changeSchool = function () {
			        return self.loadSchoolChange(self.options);
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
			    self.changeStudent = function () {
			        return self.loadStudentChange(self.options);
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

			}]
	)


})();