(function () {
    'use strict';

    angular
        .module('teamMeetingModule', [])
    .controller('TeamMeetingListController', TeamMeetingListController)
    .controller('TeamMeetingEditController', TeamMeetingEditController)
    .controller('TeamMeetingInvitationController', TeamMeetingInvitationController)
    .controller('TeamMeetingAttendController', TeamMeetingAttendController)
    .controller('TeamMeetingAttendListController', TeamMeetingAttendListController)
    .controller('TeamMeetingNotesController', TeamMeetingNotesController)
    .controller('TeamMeetingEmailInviteController', TeamMeetingEmailInviteController)
    .service('nsTMAssignStudentToIG', ['$http', function ($http) {
        var self = this;
        self.display = false;
        self.meetingName = '';
        self.studentName = '';
        self.meetingId = '';

        self.initialize = function (meetingId, meetingName, studentName) {
            self.meetingId = meetingId;
            self.meetingName = meetingName;
            self.studentName = studentName;
            self.display = true;
        };
    }])
          .directive('nsDisplayAssigningStudent', [
			'nsTMAssignStudentToIG','$location',
            function (nsTMAssignStudentToIG, $location) {

                return {
                    restrict: 'E',
                    templateUrl: 'templates/teammeeting-assign-student-to-ig.html',
                    link: function (scope, element, attr) {
                        scope.display = nsTMAssignStudentToIG.display;
                        scope.teamMeetingName = nsTMAssignStudentToIG.meetingName;
                        scope.teamMeetingId = nsTMAssignStudentToIG.meetingId;
                        scope.studentName = nsTMAssignStudentToIG.studentName;

                        scope.backToTeamMeeting = function () {
                            nsTMAssignStudentToIG.display = false;
                            $location.path('tm-attend/' + scope.teamMeetingId);
                        }
                    }
                }
            }])
         .directive('nsObservationSummaryTmAttend', [
			'$routeParams', '$compile', '$templateCache', '$http', 'nsFilterOptionsService', 'NSObservationSummaryTeamMeetingAttendManager', 'NSSortManager', '$uibModal','nsTMAssignStudentToIG','$location',
            function ($routeParams, $compile, $templateCache, $http, nsFilterOptionsService, NSObservationSummaryTeamMeetingAttendManager, NSSortManager, $uibModal, nsTMAssignStudentToIG, $location) {

                return {
                    restrict: 'E',
                    templateUrl: 'templates/observation-summary-tm-attend.html',
                    scope: {
                        selectedTeamMeetingId: '=',
                        selectedStaffId: '=',
                        selectedBenchmarkDateId: '='
                    },
                    link: function (scope, element, attr) {
                        scope.observationSummaryManager = new NSObservationSummaryTeamMeetingAttendManager();
                        scope.filterOptions = nsFilterOptionsService.options;
                        scope.manualSortHeaders = {};
                        scope.manualSortHeaders.firstNameHeaderClass = "fa";
                        scope.manualSortHeaders.lastNameHeaderClass = "fa";
                        scope.sortArray = [];
                        scope.headerClassArray = [];
                        scope.allSelected = false;
                        scope.sortMgr = new NSSortManager();
                        scope.selectedStudentResult = {};

                        scope.goToDashboard = function (schoolYear, school, interventionist, interventionGroup, studentId, stint) {
                            $location.path('ig-dashboard/' + schoolYear + '/' + school + '/' + interventionist + '/' + interventionGroup + '/' + studentId + '/' + stint);
                        }

                        scope.$on('NSFieldsUpdated', function (event, data) {
                            scope.observationSummaryManager.LoadData(scope.selectedTeamMeetingId, scope.selectedBenchmarkDateId, scope.selectedStaffId).then(function () { attachFieldsCallback() });
                        });

                        scope.openInterventionPopup = function (studentResult) {
                            scope.selectedStudentResult = studentResult;

                            var modalInstance = $uibModal.open({
                                templateUrl: 'interventionList.html',
                                scope: scope,
                                controller: function ($scope, $uibModalInstance) {
                                    $scope.cancel = function () {
                                        $uibModalInstance.dismiss('cancel');
                                    };
                                },
                                size: 'lg',
                            });
                        }

                        scope.assignStudentToIntervention = function (studentResult) {
                            nsTMAssignStudentToIG.initialize(scope.selectedTeamMeetingId, 'temporary', studentResult.LastName + ', ' + studentResult.FirstName);
                            $location.path('ig-manage');
                        }

                        scope.openNotesModal = function (studentId, teamMeetingId) {


                            // show modal after data is loaded
                            scope.observationSummaryManager.LoadNotes(studentId, teamMeetingId).then(function () {

                                var modalInstance = $uibModal.open({
                                    templateUrl: 'studentTMNotes.html',
                                    scope: scope,

                                    size: 'lg',
                                });
                            });
                        }

                        scope.$watch('selectedTeamMeetingId', function (newVal, oldVal) {
                            if (newVal !== oldVal) {
                                if(scope.selectedTeamMeetingId != null)
                                scope.observationSummaryManager.LoadData(scope.selectedTeamMeetingId, scope.selectedBenchmarkDateId, scope.selectedStaffId).then(function () { attachFieldsCallback() });
                            }
                        });

                        scope.$watch('selectedStaffId', function (newVal, oldVal) {
                            if (newVal !== oldVal) {
                                if (scope.selectedTeamMeetingId != null)
                                    scope.observationSummaryManager.LoadData(scope.selectedTeamMeetingId, scope.selectedBenchmarkDateId, scope.selectedStaffId).then(function () { attachFieldsCallback() });
                            }
                        });

                        var attachFieldsCallback = function () {


                            // initialize the sort manager now that the data has been loaded
                            scope.sortMgr.initialize(scope.manualSortHeaders, scope.sortArray, scope.headerClassArray, 'OSFieldResults', scope.observationSummaryManager.Scores);

                            for (var j = 0; j < scope.observationSummaryManager.Scores.StudentResults.length; j++) {
                                for (var k = 0; k < scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults.length; k++) {
                                    for (var i = 0; i < scope.observationSummaryManager.Scores.Fields.length; i++) {
                                        if (scope.observationSummaryManager.Scores.Fields[i].DatabaseColumn == scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].DbColumn) {
                                            //scope.observationSummaryManager.Scores[j].FieldResults[k].Field = $scope.fields[i];

                                            // set display value
                                            if (scope.observationSummaryManager.Scores.Fields[i].FieldType === "DropdownFromDB") {
                                                for (var p = 0; p < scope.observationSummaryManager.LookupLists.length; p++) {
                                                    if (scope.observationSummaryManager.LookupLists[p].LookupColumnName === scope.observationSummaryManager.Scores.Fields[i].LookupFieldName) {
                                                        // now find the specifc value that matches
                                                        for (var y = 0; y < scope.observationSummaryManager.LookupLists[p].LookupFields.length; y++) {
                                                            if (scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].IntValue === scope.observationSummaryManager.LookupLists[p].LookupFields[y].FieldSpecificId) {
                                                                scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].DisplayValue = scope.observationSummaryManager.LookupLists[p].LookupFields[y].FieldValue;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //scope.observationSummaryManager.LoadData(scope.selectedTeamMeetingId, scope.selectedBenchmarkDateId, scope.selectedStaffId).then(function () { attachFieldsCallback() });

                        // delegate sorting to the sort manager
                        scope.sort = function (column) {
                            scope.sortMgr.sort(column);
                        };

                        function getDecimalColor(gradeId, studentFieldScore) {
                            var benchmarkArray = null;
                            for (var i = 0; i < scope.observationSummaryManager.BenchmarksByGrade.length; i++) {

                                if (scope.observationSummaryManager.BenchmarksByGrade[i].GradeId == gradeId) {
                                    benchmarkArray = scope.observationSummaryManager.BenchmarksByGrade[i];
                                }

                                if (benchmarkArray != null) {
                                    for (var j = 0; j < benchmarkArray.Benchmarks.length; j++) {
                                        if (benchmarkArray.Benchmarks[j].DbColumn === studentFieldScore.DbColumn && benchmarkArray.Benchmarks[j].AssessmentId === studentFieldScore.AssessmentId) {
                                            if (studentFieldScore.DecimalValue != null) {
                                                // not defined yet
                                                //if (studentFieldScore.DecimalValue === $scope.Benchmarks[i].MaxScore) {
                                                //	return 'obsGreen';
                                                //}
                                                if (studentFieldScore.DecimalValue >= benchmarkArray.Benchmarks[j].Decimal80) {
                                                    return 'obsBlue';
                                                }
                                                if (studentFieldScore.DecimalValue >= benchmarkArray.Benchmarks[j].DecimalMean) {
                                                    return '';
                                                }
                                                if (studentFieldScore.DecimalValue >= benchmarkArray.Benchmarks[j].Decimal20) {
                                                    return 'obsYellow';
                                                }
                                                if (studentFieldScore.DecimalValue <= benchmarkArray.Benchmarks[j].Decimal20) {
                                                    return 'obsRed';
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            return '';
                        }

                        function getIntColor(gradeId, studentFieldScore) {
                            var benchmarkArray = null;
                            for (var i = 0; i < scope.observationSummaryManager.BenchmarksByGrade.length; i++) {

                                if (scope.observationSummaryManager.BenchmarksByGrade[i].GradeId == gradeId) {
                                    benchmarkArray = scope.observationSummaryManager.BenchmarksByGrade[i];
                                }

                                if (benchmarkArray != null) {
                                    for (var j = 0; j < benchmarkArray.Benchmarks.length; j++) {
                                        if (benchmarkArray.Benchmarks[j].DbColumn === studentFieldScore.DbColumn && benchmarkArray.Benchmarks[j].AssessmentId === studentFieldScore.AssessmentId) {
                                            if (studentFieldScore.IntValue != null) {
                                                // not defined yet
                                                //if (studentFieldScore.DecimalValue === $scope.Benchmarks[i].MaxScore) {
                                                //	return 'obsGreen';
                                                //}
                                                if (studentFieldScore.IntValue >= benchmarkArray.Benchmarks[j].Exceeds) {
                                                    return 'obsBlue';
                                                }
                                                if (studentFieldScore.IntValue >= benchmarkArray.Benchmarks[j].Meets) {
                                                    return 'obsGreen';
                                                }
                                                if (studentFieldScore.IntValue >= benchmarkArray.Benchmarks[j].Approaches) {
                                                    return 'obsYellow';
                                                }
                                                if (studentFieldScore.IntValue < benchmarkArray.Benchmarks[j].Approaches) {
                                                    return 'obsRed';
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            return '';
                        }

                        scope.getBackgroundClass = function (gradeId, studentFieldScore) {
                            switch (studentFieldScore.ColumnType) {
                                case 'Textfield':
                                    return '';
                                    break;
                                case 'DecimalRange':
                                    return getDecimalColor(gradeId, studentFieldScore);
                                    break;
                                case 'DropdownRange':
                                    return getIntColor(gradeId, studentFieldScore);
                                    break;
                                case 'DropdownFromDB':
                                    return getIntColor(gradeId, studentFieldScore);
                                    break;
                                case 'CalculatedFieldClientOnly':
                                    return '';
                                    break;
                                case 'CalculatedFieldDbBacked':
                                    return getIntColor(gradeId, studentFieldScore);
                                    break;
                                case 'CalculatedFieldDbOnly':
                                    return '';
                                    break;
                                default:
                                    return '';
                                    break;
                            }

                            return '';
                        };


                    }
                };
            }
         ])
        .factory('NSObservationSummaryTeamMeetingAttendManager', [
    '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
        var NSObservationSummaryTeamMeetingAttendManager = function () {
            this.initialize = function () {

            }

            this.LoadData = function (teamMeetingId, tddId, staffId) {
                var paramObj = { TeamMeetingId: teamMeetingId, TestDueDateId: 0, StaffId: staffId };
                var url = webApiBaseUrl + '/api/assessment/GetTeamMeetingAttendObservationSummary/';
                //var paramObj = {}
                var summaryData = $http.post(url, paramObj);
                var self = this;

                self.LookupLists = [];
                self.Scores = [];
                self.BenchmarksByGrade = [];
                self.InterventionsByStudent = [];
                // add a "NoteCount" to each studentresultdata

                return summaryData.then(function (response) {
                    angular.extend(self, response.data);
                    if (self.LookupLists === null) self.LookupLists = [];
                    if (self.Scores === null) self.Scores = [];
                    if (self.BenchmarksByGrade === null) self.BenchmarksByGrade = [];
                    if (self.InterventionsByStudent === null) self.InterventionsByStudent = [];

                    // hook up interventions to studentresults
                    for (var i = 0; i < self.Scores.StudentResults.length; i++) {
                        var currentStudent = self.Scores.StudentResults[i];

                        for (var j = 0; j < self.InterventionsByStudent.length; j++) {
                            if (self.InterventionsByStudent[j].StudentId === currentStudent.StudentId) {
                                currentStudent.Interventions = self.InterventionsByStudent[j];
                            }
                        }
                    }
                }, function (response) {
                    // error callback function

                });
            }
        };

        return (NSObservationSummaryTeamMeetingAttendManager);
    }
        ])
     .factory('NSTeamMeetingManager', [
        '$http', '$bootbox', 'nsFilterOptionsService', 'webApiBaseUrl', function ($http, $bootbox, nsFilterOptionsService, webApiBaseUrl) {
            var NSTeamMeetingManager = function () {
                this.initialize = function () {
                }

                this.getTeamMeetingList = function (schoolYear) {
                    var returnObject = {
                        SchoolYear: nsFilterOptionsService.normalizeParameter(schoolYear)
                    };

                    return $http.post(webApiBaseUrl + "/api/teammeeting/GetTeamMeetingList", returnObject);
                };

                this.createNewAttendeeGroup = function (attendees, groupName) {
                    var returnObject = {
                        GroupName: groupName,
                        Attendees: attendees
                    };

                    var promise = $http.post(webApiBaseUrl + "/api/teammeeting/saveattendeegroup", returnObject);
                    return promise;
                }

                this.loadStudentsForSection = function (sectionId, successCallback, failureCallback) {
                    var loadResponse = $http.get(webApiBaseUrl + "/api/teammeeting/getsectiondetails/" + sectionId);
                    loadResponse.then(successCallback, failureCallback);
                }

                this.save = function (teammeeting, successCallback, failureCallback) {
                    var saveResponse = $http.post(webApiBaseUrl + "/api/teammeeting/saveteammeeting", teammeeting);
                    saveResponse.then(successCallback, failureCallback);
                };

                this.delete = function (id, successCallback, failureCallback) {
                    $bootbox.confirm("Are you sure you want to delete this team meeting?  You will not be able to delete a meeting that has Notes stored.", function (result) {
                        if (result === true) {
                            var returnObject = { Id: id };
                            var deleteResponse = $http.post(webApiBaseUrl + "/api/teammeeting/deleteteammeeting", returnObject);

                            deleteResponse.then(successCallback, failureCallback);
                        }
                    });
                };
                this.initialize();
            };

            return (NSTeamMeetingManager);
        }
     ])
     .factory('NSTeamMeeting', [
        'nsPinesService', '$http', 'webApiBaseUrl', function (nsPinesService, $http, webApiBaseUrl) {
            var NSTeamMeeting = function (id, flag) {
                var self = this;
                self.initialize = function () {
                    var paramObj = { Id: id, flag: flag };
                    var url = webApiBaseUrl + '/api/teammeeting/getteammeeting';
                    var meetingData = $http.post(url, paramObj);
                    
                    self.Sections = [];
                    self.Attendees = [];
                    self.StaffGroups = [];
                    self.TeamMeetingAttendances = [];
                    self.TeamMeetingStudents = [];
                    self.TeamMeetingStudentNotes = [];

                    meetingData.then(function (response) {
                        angular.extend(self, response.data);
                        if (self.Sections === null) self.Sections = [];
                        if (self.Attendees === null) self.Attendees = [];
                        if (self.StaffGroups === null) self.StaffGroups = [];
                        if (self.TeamMeetingAttendances === null) self.TeamMeetingAttendances = [];
                        if (self.TeamMeetingStudents === null) self.TeamMeetingStudents = [];
                        if (self.TeamMeetingStudentNotes === null) self.TeamMeetingStudentNotes = [];
                        self.MeetingTime = moment(self.MeetingTime).toDate();
                    }, function (response) {
                        // error callback function

                    });
                }

                self.LoadNotes = function (studentId, teamMeetingId) {
                    var paramObj = { StudentId: studentId, TeamMeetingId: teamMeetingId };
                    var url = webApiBaseUrl + '/api/teammeeting/getnotesforstudentteammeeting';
                    //var paramObj = {}
                    var promise = $http.post(url, paramObj);

                    return promise.then(function (response) {
                        self.CurrentNotes = response.data.Notes;
                        self.CurrentStudent = response.data.Student;
                    });
                }

                self.saveSingleTeamMeetingAttendance = function (attendance) {
                    var paramObj = { TeamMeetingAttendance: attendance };
                    var url = webApiBaseUrl + '/api/teammeeting/savesingleteammeetingattendance';

                    var promise = $http.post(url, paramObj);

                    return promise;
                }

                self.saveAllTeamMeetingAttendances = function (attendances) {
                    var paramObj = { TeamMeetingAttendances: attendances };
                    var url = webApiBaseUrl + '/api/teammeeting/saveallteammeetingattendances';

                    var promise = $http.post(url, paramObj);

                    return promise;
                }

                self.getInvitationPreview = function (staffId) {
                    var paramObj = { TeamMeetingId: self.ID, StaffId: staffId };
                    var url = webApiBaseUrl + '/api/teammeeting/gettminvitationemailpreview';

                    var promise = $http.post(url, paramObj);

                    return promise;
                }

                self.SaveNote = function (note) {
                    var paramObj = { StudentId: note.StudentID, TeamMeetingId: note.TeamMeetingID, NoteId: note.ID, NoteHtml: note.Note };
                    var url = webApiBaseUrl + '/api/teammeeting/SaveNote';

                    var promise = $http.post(url, paramObj);

                    return promise;
                }

                self.DeleteNote = function (note) {
                    var paramObj = { Id: note.ID };
                    var url = webApiBaseUrl + '/api/teammeeting/DeleteNote';

                    var promise = $http.post(url, paramObj);

                    return promise;
                }

                self.removeGroup = function (groupId) {
                    var self = this;
                    var response = $http.get(webApiBaseUrl + "/api/teammeeting/getstaffforattendeegroup/" + groupId);
                    response.then(function (data) {
                        for (var i = self.Attendees.length - 1; i >= 0; i--) {
                            for (var j = 0; j < data.data.length; j++) {
                                if (self.Attendees[i].id === data.data[j].id) {
                                    // if this is one of the ones we need to remove, split it
                                    self.Attendees.splice(i, 1);
                                    break;
                                }
                            }
                        }
                    }, genericFailure());
                };


                self.deleteGroup = function (groupId) {
                    var responseObject = { Id: groupId };
                    var response = $http.post(webApiBaseUrl + "/api/teammeeting/deleteattendeegroup/", responseObject);
                    response.then(function (data) {
                        for (var i = self.AttendeeGroups.length - 1; i >= 0; i--) {
                            if (self.AttendeeGroups[i].Id === groupId) {
                                // if this is one of the ones we need to remove, split it
                                nsPinesService.dataDeletedSuccessfully();

                                self.AttendeeGroups.splice(i, 1);
                                break;
                            }
                        }
                    }, genericFailure());
                };

                self.addGroup = function (groupId) {
                    var response = $http.get(webApiBaseUrl + "/api/teammeeting/getstaffforattendeegroup/" + groupId);
                    response.then(function (data) {
                        for (var i = 0; i < data.data.length; i++) {
                            var found = false;
                            for (var j = 0; j < self.Attendees.length; j++) {

                                if (self.Attendees[j].id === data.data[i].id) {
                                    // if this is one of the ones we need to remove, split it
                                    found = true;
                                    break;
                                }
                            }

                            if (!found) {
                                self.Attendees.push(data.data[i]);
                            }
                        }
                    }, genericFailure());
                };

                var genericFailure = function (error) {
                    // TODO: call common generic function
                };

                self.initialize();
            };

            return (NSTeamMeeting);
        }
     ]);

    TeamMeetingEditController.$inject = ['$scope', '$routeParams', '$location', 'nsSectionService', 'nsFilterOptionsService', 'nsPinesService', 'nsSelect2RemoteOptions', 'NSTeamMeeting', 'NSTeamMeetingManager', '$bootbox'];

    function TeamMeetingEditController($scope, $routeParams, $location, nsSectionService, nsFilterOptionsService, nsPinesService, nsSelect2RemoteOptions, NSTeamMeeting, NSTeamMeetingManager, $bootbox) {
        $scope.meeting = new NSTeamMeeting($routeParams.id, false);
        $scope.meeting.Sections = [];
        $scope.meeting.Attendees = [];
        $scope.meeting.StaffGroups = [];
        $scope.meeting.TeamMeetingAttendances = [];
        $scope.meeting.TeamMeetingStudents = [];
        $scope.meeting.TeamMeetingStudentNotes = [];
        $scope.meetingManager = new NSTeamMeetingManager();
        $scope.filterOptions = nsFilterOptionsService.options;
        $scope.errors = [];
        $scope.selectedSection = null;
        $scope.selectedStudentSections = [];

        $scope.loadSection = function(section)
        {
            $scope.selectedSection = section;
        }
        // we don't care about these changes unless this is a new section
        //$scope.$watch('filterOptions', function () {
        //    if ($routeParams.id === "-1") {
        //        $scope.section.SchoolId = nsFilterOptionsService.normalizeParameter($scope.filterOptions.selectedSchool);
        //        $scope.section.SchoolYear = nsFilterOptionsService.tempNormalizeParameter($scope.filterOptions.selectedSchoolYear);
        //    }
        //}, true);

        $scope.removeGoup = function (groupId) {
            $scope.meeting.removeGroup(groupId);
        };


        $scope.deleteGroup = function (groupId) {
            $bootbox.confirm("Are You sure you want to delete this Attendee Group?", function (result) {
                if (result) {
                    $scope.meeting.deleteGroup(groupId);
                }
            });
        };

        $scope.addGroup = function (groupId) {
            $scope.meeting.addGroup(groupId);
        };

        $scope.loadStudentsForSection = function (selectedSection) {
            if (selectedSection == null)
            {
                alert('Please select a section first.');
                return;
            }
            // go get section details and add to Sections[]
            $scope.meetingManager.loadStudentsForSection(selectedSection.id, loadStudentsCallback);
            
        };

        $scope.createNewAttendeeGroup = function (attendees) {
            $bootbox.prompt("What do you want to call this group?", function (result) {
                if (result === null) {
                    nsPinesService.cancelled();
                } else {
                    // call meeting manager to create new group and then add to the attendee group list
                    $scope.meetingManager.createNewAttendeeGroup(attendees, result).then(function (response) {
                        nsPinesService.buildMessage('Group Created', 'Your group was saved successfully', 'success');
                        $scope.meeting.AttendeeGroups.push(response.data);
                    });
                }
            });
        };

        var loadStudentsCallback = function (data) {
            // set the dynamic section to null to reset
            $scope.meeting.DynamicSection = null;
            $scope.meeting.Sections.push(data.data);
            $scope.loadSection(data.data);
        }

        // TODO: make this generic
        //var failureCallback = function (error) {
        //    alert('remove me and use generic');
        //}

        $scope.addStudentToSection = function () {
            if ($scope.section.addStudentToSection($scope.section.addStudent)) {
                $scope.section.addStudent = null;
            }
        };

        $scope.saveTeamMeeting = function () {
            // TODO: ensure all fields are valid
            $scope.meetingManager.save($scope.meeting,
                function () {
                    nsPinesService.dataSavedSuccessfully();
                    $location.path('tm-manage');
                }, function (msg) {
                    $scope.errors.push({ msg: '<strong>An Error Has Occurred</strong> ' + msg.data, type: 'danger' });
                    $('html, body').animate({ scrollTop: 0 }, 'fast')
                });
        };

        $scope.closeAlert = function (index) {
            $scope.errors.splice(index, 1);
        };

        // initial load
        $scope.getCoTeacherRemoteOptions = nsSelect2RemoteOptions.CoTeacherRemoteOptions;
        $scope.getStaffGroupRemoteOptions = nsSelect2RemoteOptions.StaffGroupRemoteOptions;
        $scope.chooseSectionRemoteOptions = nsSelect2RemoteOptions.quickSearchSectionsRemoteOptions;
    }

    TeamMeetingInvitationController.$inject = ['$scope', '$q', '$http', '$uibModal', 'nsPinesService', '$location', '$filter', '$routeParams', 'NSTeamMeeting', 'progressLoader'];

    function TeamMeetingInvitationController($scope, $q, $http, $uibModal, nsPinesService, $location, $filter, $routeParams, NSTeamMeeting, progressLoader) {
        //var self = this;
        $scope.status = {};
        $scope.status.sendEmailMode = false;
        $scope.meeting = new NSTeamMeeting($routeParams.id, true);
        $scope.meeting.selectedStaffId = null;

        $scope.saveSingleTeamMeetingAttendance = function (attendance) {
            progressLoader.start();
            progressLoader.set(50);
            $scope.meeting.saveSingleTeamMeetingAttendance(attendance).then(function (response) {
                progressLoader.end();
            });
        }

        $scope.toggleIncludeAllStudents = function () {
            for (var i = 0; i < $scope.meeting.TeamMeetingAttendances.length; i++) {
                var tma = $scope.meeting.TeamMeetingAttendances[i];
                tma.IncludeAllStudents = $scope.meeting.AllInclude;
            }

            progressLoader.start();
            progressLoader.set(50);
            $scope.meeting.saveAllTeamMeetingAttendances($scope.meeting.TeamMeetingAttendances).then(function (response) {
                progressLoader.end();
            });
        }

        $scope.toggleAllSelected = function () {
            for (var i = 0; i < $scope.meeting.TeamMeetingAttendances.length; i++) {
                var tma = $scope.meeting.TeamMeetingAttendances[i];
                tma.Selected = $scope.meeting.SelectAll;
            }
        }

        $scope.toggleAllAttended = function () {
            for (var i = 0; i < $scope.meeting.TeamMeetingAttendances.length; i++) {
                var tma = $scope.meeting.TeamMeetingAttendances[i];
                tma.Attended = $scope.meeting.AllAttended;
            }

            progressLoader.start();
            progressLoader.set(50);
            $scope.meeting.saveAllTeamMeetingAttendances($scope.meeting.TeamMeetingAttendances).then(function (response) {
                progressLoader.end();
            });
        }

        $scope.sendSelectedInvites = function () {
            var noticesToSend = [];

            // get the selected ones
            for (var i = 0; i < $scope.meeting.TeamMeetingAttendances.length; i++) {
                var tma = $scope.meeting.TeamMeetingAttendances[i];

                if (tma.Selected) {
                    noticesToSend.push(tma);
                }
            }

            if (noticesToSend.length == 0) {
                nsPinesService.buildMessage('Nothing to send', 'You have not selected any staff to send an invitation to.', 'info');
                return;
            }

            nsPinesService.setIntervalMessage(noticesToSend.length);
            // only process the selected ones
            for (var i = 0; i < noticesToSend.length; i++) {
                var tma = noticesToSend[i];
                var postObject = { TeamMeetingId: $scope.meeting.ID, StaffId: tma.StaffID };
                $http.post('http://localhost:16726/api/teammeeting/sendtminvitation', postObject).then(
                    function () {
                        nsPinesService.incrementInterval();
                    }
                );
            }
        }

        $scope.sendSelectedConcluded = function () {
            var noticesToSend = [];

            // get the selected ones
            for (var i = 0; i < $scope.meeting.TeamMeetingAttendances.length; i++) {
                var tma = $scope.meeting.TeamMeetingAttendances[i];

                if (tma.Selected) {
                    noticesToSend.push(tma);
                }
            }

            if (noticesToSend.length == 0) {
                nsPinesService.buildMessage('Nothing to send', 'You have not selected any staff to send a concluded notice to.', 'info');
                return;
            }

            nsPinesService.setIntervalMessage(noticesToSend.length);
            // only process the selected ones
            for (var i = 0; i < noticesToSend.length; i++) {
                var tma = noticesToSend[i];
                var postObject = { TeamMeetingId: $scope.meeting.ID, StaffId: tma.StaffID };
                $http.post('http://localhost:16726/api/teammeeting/sendtmconcluded', postObject).then(
                    function () {
                        nsPinesService.incrementInterval();
                    }
                );
            }
        }

        $scope.sendInvite = function (staffId) {
            var postObject = { TeamMeetingId: $scope.meeting.ID, StaffId: staffId };
            $http.post('http://localhost:16726/api/teammeeting/sendtminvitation', postObject).then(
                function () {
                    nsPinesService.emailSentSuccessfully();
                }
                );
        }

        $scope.sendConcluded = function (staffId) {
            var postObject = { TeamMeetingId: $scope.meeting.ID, StaffId: staffId };
            $http.post('http://localhost:16726/api/teammeeting/sendtmconcluded', postObject).then(
                function () {
                    nsPinesService.emailSentSuccessfully();
                }
                );
        }


        $scope.openDemoModal = function (staffId) {
            var paramStaffId = staffId;
            $scope.meeting.selectedStaffId = paramStaffId;

            $scope.meeting.getInvitationPreview(paramStaffId).then(function (response) {
                $scope.meeting.previewHtml = response.data;
                var modalInstance = $uibModal.open({
                    templateUrl: 'demoModalContent.html',
                    scope: $scope,
                    controller: function ($scope, $uibModalInstance) {
                        $scope.cancel = function () {
                            $uibModalInstance.dismiss('cancel');
                        };
                    },
                    size: 'lg',
                });
            });
        }

    }

    TeamMeetingAttendController.$inject = ['$scope', '$q', '$http', '$uibModal', 'nsPinesService', '$location', '$filter', '$routeParams', 'NSTeamMeeting','nsFilterOptionsService'];

    function TeamMeetingAttendController($scope, $q, $http, $uibModal, nsPinesService, $location, $filter, $routeParams, NSTeamMeeting, nsFilterOptionsService) {
        $scope.filterOptions = nsFilterOptionsService.options;
        $scope.status = {};
        $scope.meeting = new NSTeamMeeting($routeParams.id, true);
        $scope.meeting.selectedStaffId = null;
    }

    TeamMeetingNotesController.$inject = ['$scope', '$q', '$http', '$uibModal', 'nsPinesService', '$location', '$filter', '$routeParams', 'NSTeamMeeting', '$bootbox'];

    function TeamMeetingNotesController($scope, $q, $http, $uibModal, nsPinesService, $location, $filter, $routeParams, NSTeamMeeting, $bootbox) {
        // TODO: add current staff ID to a common service
        //var self = this;
        $scope.status = {};
        $scope.meeting = new NSTeamMeeting($routeParams.teamMeetingId, true);
        $scope.meeting.LoadNotes($routeParams.studentId, $routeParams.teamMeetingId);
        $scope.NewNote = {};
        $scope.NewNote.ID = -1;
        $scope.NewNote.TeamMeetingID = $routeParams.teamMeetingId;
        $scope.NewNote.StudentID = $routeParams.studentId;

        $scope.meeting.selectedStaffId = null;
        $scope.teamMeetingId = $routeParams.teamMeetingId;
        $scope.editingId = -1;

        $scope.getNoteClass = function (staffId, index) {
            var classArray = ["chat-success", "chat-midnightblue", "chat-primary", "chat-indigo", "chat-danger", "chat-orange"];
            var randomIndex = Math.floor((index / classArray.length) * classArray.length);
            var randomClass = classArray[randomIndex];

            if (staffId == 1191) {
                return "me";
            } else {
                return randomClass;
            }
        }

        $scope.cancel = function () {
            $scope.editingId = -1;
        }

        $scope.cancelAndReturn = function () {
            $scope.editingId = -1;
            $location.path("tm-attend/" + $routeParams.teamMeetingId);
        };

        $scope.editNote = function (noteId) {
            $scope.editingId = noteId;
        }

        $scope.saveExistingNote = function (note) {

            // get note
            var editor = CKEDITOR.instances.editableNote;
            var udpatedText = editor.getData();

            note.Note = udpatedText;
            $scope.meeting.SaveNote(note).then(function () {
                nsPinesService.dataSavedSuccessfully();
                $scope.editingId = -1;
            });
        };

        $scope.deleteNote = function (note) {
            $bootbox.confirm("Are You sure you want to delete this Note?", function (result) {
                if (result) {
                    $scope.meeting.DeleteNote(note).then(function () {
                        nsPinesService.dataDeletedSuccessfully();
                        // reload notes
                        $scope.meeting.LoadNotes($routeParams.studentId, $routeParams.teamMeetingId);
                    });
                }
            });
        };

        $scope.saveNewNote = function () {
            var editor = CKEDITOR.instances.mainEditor;
            var udpatedText = editor.getData();

            $scope.NewNote.Note = udpatedText;

            $scope.meeting.SaveNote($scope.NewNote).then(function () {
                nsPinesService.dataSavedSuccessfully();
                editor.setData('');
                $scope.NewNote.Note = '';
                // reload notes
                $scope.meeting.LoadNotes($routeParams.studentId, $routeParams.teamMeetingId);
            });
        };

        $scope.isEditing = function(noteId)
        {
            if ($scope.editingId === noteId) {
                return true;
            } else {
                return false;
            }
        }

        $scope.canEditNote = function(noteStaffId)
        {
            if(noteStaffId == 1191)
            {
                return true;
            } else {
                return false;
            }
        }
    }


    TeamMeetingEmailInviteController.$inject = ['$scope', '$q', '$http', '$uibModal', 'nsPinesService', '$location', '$filter', '$routeParams', 'NSTeamMeeting'];

    function TeamMeetingEmailInviteController($scope, $q, $http, $uibModal, nsPinesService, $location, $filter, $routeParams, NSTeamMeeting) {
        //var self = this;

        $scope.invite = {};
        $scope.invite.testDueDateId = $routeParams.tddid;
        $scope.invite.meetingId = $routeParams.teamMeetingId;
        $scope.invite.staffId = $routeParams.staffId;

    }

    TeamMeetingAttendListController.$inject = ['$scope', '$q', '$http', 'nsPinesService', '$location', '$filter', '$routeParams', 'nsSectionDataEntryService', 'nsFilterOptionsService', 'NSTeamMeetingManager'];

    function TeamMeetingAttendListController($scope, $q, $http, nsPinesService, $location, $filter, $routeParams, nsSectionDataEntryService, nsFilterOptionsService, NSTeamMeetingManager) {
        $scope.sortArray = [];
        $scope.headerClassArray = [];
        $scope.staticColumnsObj = {};
        $scope.staticColumnsObj.studentNameHeaderClass = "fa";
        $scope.filterOptions = nsFilterOptionsService.options;

        $scope.TeamMeetingManager = new NSTeamMeetingManager();

        $scope.$watch('filterOptions.selectedSchoolYear', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                LoadData();
            }
        }, true);

        $scope.sort = function (column) {
            $scope.TeamMeetingManager.doSort(column, $scope.staticColumnsObj, $scope.fields, $scope.headerClassArray, $scope.sortArray);
        };

        $scope.defaultEditAction = function (studentResult, rowform) {
            var dataEntryPage = $scope.assessment.DefaultDataEntryPage;
            // default editing
            if (dataEntryPage === null || dataEntryPage === '') {
                rowform.$show();
            }
            else {
                $location.path(dataEntryPage + "/" + $routeParams.assessmentid + "/" + $scope.filterOptions.selectedSection.Id + "/" + $scope.filterOptions.selectedBenchmarkDate.Id + "/" + studentResult.StudentId + "/" + studentResult.ResultId);
            }
        };

        var LoadData = function () {

            if ($scope.filterOptions.selectedSchoolYear != null) {
                $scope.TeamMeetingManager.getTeamMeetingList($scope.filterOptions.selectedSchoolYear)
                    .then(
                        function (data) {
                            $scope.teamMeetings = data.data.TeamMeetings;
                        },
                        function (msg) {
                            alert('error loading results');
                        }
                    );
            }
        }

        // initial load
        LoadData();
        // $scope.sort('StudentName');
    }



    TeamMeetingListController.$inject = ['$scope', '$q', '$http', 'nsPinesService', '$location', '$filter', '$routeParams', 'nsSectionDataEntryService', 'nsFilterOptionsService', 'NSTeamMeetingManager'];

    function TeamMeetingListController($scope, $q, $http, nsPinesService, $location, $filter, $routeParams, nsSectionDataEntryService, nsFilterOptionsService, NSTeamMeetingManager) {
        $scope.sortArray = [];
        $scope.headerClassArray = [];
        $scope.staticColumnsObj = {};
        $scope.staticColumnsObj.studentNameHeaderClass = "fa";
        $scope.filterOptions = nsFilterOptionsService.options;

        $scope.TeamMeetingManager = new NSTeamMeetingManager();

        $scope.$watch('filterOptions.selectedSchoolYear', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                LoadData();
            }
        }, true);

        $scope.sort = function (column) {
            $scope.TeamMeetingManager.doSort(column, $scope.staticColumnsObj, $scope.fields, $scope.headerClassArray, $scope.sortArray);
        };

        $scope.defaultEditAction = function (studentResult, rowform) {
            var dataEntryPage = $scope.assessment.DefaultDataEntryPage;
            // default editing
            if (dataEntryPage === null || dataEntryPage === '') {
                rowform.$show();
            }
            else {
                $location.path(dataEntryPage + "/" + $routeParams.assessmentid + "/" + $scope.filterOptions.selectedSection.Id + "/" + $scope.filterOptions.selectedBenchmarkDate.Id + "/" + studentResult.StudentId + "/" + studentResult.ResultId);
            }
        };

        var LoadData = function () {

            if ($scope.filterOptions.selectedSchoolYear != null) {
                $scope.TeamMeetingManager.getTeamMeetingList($scope.filterOptions.selectedSchoolYear)
                    .then(
                        function (data) {
                            $scope.teamMeetings = data.data.TeamMeetings;
                        },
                        function (msg) {
                            alert('error loading results');
                        }
                    );
            }
        }

        // initial load
        LoadData();
       // $scope.sort('StudentName');
    }



})();