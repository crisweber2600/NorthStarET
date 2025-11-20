(function () {
    'use strict';

    angular
        .module('studentDashboardModule', [])
        .controller('StudentDashboardController', [
            '$http', 'nsFilterOptionsService', '$scope', 'NSAssesssmentLineGraphManager', 'NSStudentInterventionManager', 'NSStudentTMNotesManager', 'nsPinesService', 'webApiBaseUrl','$bootbox', function (
                $http, nsFilterOptionsService, $scope, NSAssesssmentLineGraphManager, NSStudentInterventionManager, NSStudentTMNotesManager, nsPinesService, webApiBaseUrl, $bootbox) {
            $scope.filterOptions = nsFilterOptionsService.options;

            // create array of assessmentfields for testing, eventually this will be from some kind of picker
            $scope.fields = [];
            $scope.fields.push({ AssessmentName: 'F&P Text Leveling', FieldName: 'FPValueID', FieldDisplayLabel: 'Level', AssessmentId: 1, LookupFieldName: 'FPScale', FieldType: 'DropDownFromDb' });
            $scope.fields.push({ AssessmentName: 'Writing Vocab', FieldName: 'WordsCorrect', FieldDisplayLabel: 'Words Correct', AssessmentId: 3, LookupFieldName: null, FieldType: 'DropDownRange' });

            $scope.studentTMNotesManager = new NSStudentTMNotesManager();
            $scope.studentDataManager = new NSStudentInterventionManager();
            $scope.ClassLineGraphDataManagers = [];
            $scope.options = {
                language: 'en',
                allowedContent: true,
                entities: false
            };

            // TODO: move current user crap to service... PLEASE!!!
            $scope.currentUser = {};
            $scope.loadInfo = function () {
                return $http.get(webApiBaseUrl + "/api/staff/myinfo").then(function (response) {
                    angular.extend($scope.currentUser, response.data);
                });
            }
            $scope.loadInfo();

            $scope.onReady = function () {
                
            };

            $scope.saveNote = function (noteId, noteHtml, teamMeetingId, studentId) {

                if (noteHtml === '' || noteHtml == null) {
                    alert('Please enter a note');
                    return;
                }
                $scope.studentTMNotesManager.saveNote(noteId, noteHtml, teamMeetingId, studentId)
                    .then(function (response) {
                        nsPinesService.dataSavedSuccessfully();
                        // reload the list
                        $scope.studentTMNotesManager.LoadData($scope.filterOptions.selectedStudent.id);
                });
            }

            $scope.deleteNote = function (noteId) {
                $bootbox.confirm('Are you sure you want to delete this note?', function (response) {
                    if (response) {

                        $scope.studentTMNotesManager.deleteNote(noteId)
                            .then(function (response) {
                                nsPinesService.dataDeletedSuccessfully();
                                $scope.studentTMNotesManager.LoadData($scope.filterOptions.selectedStudent.id);
                            });
                    }
                });
            };

            $scope.$watch('filterOptions.selectedStudent.id', function (newVal, oldVal) {
                if (newVal !== oldVal) {
                    LoadLineGraphs();
                    $scope.studentTMNotesManager.LoadData(newVal);
                }
            });

            // scope.watch assessment and call function to load
            function LoadLineGraphs() {
                $scope.ClassLineGraphDataManagers = [];

                $scope.studentDataManager.LoadData($scope.filterOptions.selectedStudent.id).then(function (response) {
                    angular.forEach($scope.fields, function (f) {
                        var dataMgr = new NSAssesssmentLineGraphManager();
                        dataMgr.LoadData(f.AssessmentId, f.FieldName, f.LookupFieldName, f.FieldType, $scope.filterOptions.selectedStudent.id, f.FieldDisplayLabel, $scope.filterOptions.selectedSection.id, f.AssessmentName, $scope.studentDataManager.Interventions).then(function (response) {
                            $scope.ClassLineGraphDataManagers.push(dataMgr);
                        });
                    });
                });
            }
        }])
        .factory('NSStudentDashboardManager', ['$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {

            var NSStudentDashboardManager = function () {
                this.LoadData = function (studentId, assessmentIds) {
                    var url = webApiBaseUrl + '/api/studentdashboard/GetStudentObservationSummary/';
                    var paramObj = { StudentId: studentId, AssessmentIds: assessmentIds }
                    var promise = $http.post(url, paramObj);
                    var self = this;

                    self.LookupLists = [];
                    self.Scores = [];
                    self.BenchmarksByGrade = [];
                    

                    return promise.then(function (response) {
                        angular.extend(self, response.data);
                        if (self.LookupLists === null) self.LookupLists = [];
                        if (self.Scores === null) self.Scores = [];
                        if (self.BenchmarksByGrade === null) self.BenchmarksByGrade = [];

                    });
                }
            }
            return NSStudentDashboardManager;
        }])
        .factory('NSStudentTMNotesManager', ['$http', 'webApiBaseUrl','$filter', function ($http, webApiBaseUrl, $filter) {

            var NSStudentTMNotesManager = function () {

                this.saveNote = function (noteId, noteHtml, teamMeetingId, studentId) {
                    var url = webApiBaseUrl + '/api/teammeeting/savenote';
                    var paramObj = { TeamMeetingId: teamMeetingId, NoteId: noteId, NoteHtml: noteHtml, StudentId: studentId };
                    var promise = $http.post(url, paramObj);

                    return promise;
                }

                this.deleteNote = function (noteId) {
                    var url = webApiBaseUrl + '/api/teammeeting/deletenote';
                    var paramObj = { Id: noteId };
                    var promise = $http.post(url, paramObj);

                    return promise;
                }

                this.LoadData = function (studentId) {
                    var url = webApiBaseUrl + '/api/teammeeting/getnotesforstudentteammeetings/';
                    var paramObj = { id: studentId }
                    var promise = $http.post(url, paramObj);
                    var self = this;
                    self.Meetings = [];

                    // SAVE THIS LOGIC for Regular student notes
                    function groupByYear(data) {
                        if (data === null) {
                            return [];
                        }
                        var groupedNotes = [];
                        
                        angular.forEach(data.Meetings, function (meeting) {
                            var foundYear = $filter('filter')(groupedNotes, { SchoolYear: meeting.SchoolYear });
                            // see if category already exists, if not, add it
                            if (!foundYear.length) {
                                var newGroupedNote = { SchoolYear: meeting.SchoolYear, Meetings: [] };
                                newGroupedNote.Meetings.push(meeting);
                                groupedNotes.push(newGroupedNote);
                            } else {
                                foundYear[0].Meetings.push(meeting);
                            }
                        });

                        return groupedNotes;
                    }

                    return promise.then(function (response) {
                        self.Meetings = response.data.Meetings;
                    });
                }
            }
            return NSStudentTMNotesManager;
        }])
        .directive('nsObservationSummaryStudent', [
            '$routeParams', '$compile', '$templateCache', '$http', 'nsFilterOptionsService', '$filter', 'NSStudentDashboardManager', 'NSSortManager', 'nsLookupFieldService',
            function ($routeParams, $compile, $templateCache, $http, nsFilterOptionsService, $filter, NSStudentDashboardManager, NSSortManager, nsLookupFieldService) {

                return {
                    restrict: 'E',
                    templateUrl: 'templates/observation-summary-student.html',
                    scope: {
                        selectedStudentId: '=',
                        selectedAssessmentIds: '='
                    },
                    link: function (scope, element, attr) {
                        scope.observationSummaryManager = new NSStudentDashboardManager();
                        scope.filterOptions = nsFilterOptionsService.options;
                        scope.manualSortHeaders = {};
                        scope.manualSortHeaders.firstNameHeaderClass = "fa";
                        scope.manualSortHeaders.lastNameHeaderClass = "fa";
                        scope.sortArray = [];
                        scope.headerClassArray = [];
                        scope.allSelected = false;
                        scope.sortMgr = new NSSortManager();
                        scope.lookupFieldsArray = nsLookupFieldService.LookupFieldsArray;

                        scope.$watch('selectedStudentId', function (newVal, oldVal) {
                            if (newVal !== oldVal) {
                                scope.observationSummaryManager.LoadData(scope.selectedStudentId, "1,3").then(function (response) { attachFieldsCallback(); });
                            }
                        });
                                                        

                        var attachFieldsCallback = function () {
                            // initialize the sort manager now that the data has been loaded
                            scope.sortMgr.initialize(scope.manualSortHeaders, scope.sortArray, scope.headerClassArray, 'OSFieldResults', scope.observationSummaryManager.Scores);

                            for (var j = 0; j < scope.observationSummaryManager.Scores.StudentResults.length; j++) {
                                for (var k = 0; k < scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults.length; k++) {
                                    for (var i = 0; i < scope.observationSummaryManager.Scores.Fields.length; i++) {
                                        if (scope.observationSummaryManager.Scores.Fields[i].DatabaseColumn == scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].DbColumn) {
                                            scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].Field = angular.copy(scope.observationSummaryManager.Scores.Fields[i]);

                                            // set display value
                                            if (scope.observationSummaryManager.Scores.Fields[i].FieldType === "DropdownFromDB") {
                                                for (var p = 0; p < scope.lookupFieldsArray.length; p++) {
                                                    if (scope.lookupFieldsArray[p].LookupColumnName === scope.observationSummaryManager.Scores.Fields[i].LookupFieldName) {
                                                        // now find the specifc value that matches
                                                        for (var y = 0; y < scope.lookupFieldsArray[p].LookupFields.length; y++) {
                                                            if (scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].IntValue === scope.lookupFieldsArray[p].LookupFields[y].FieldSpecificId) {
                                                                scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].DisplayValue = scope.lookupFieldsArray[p].LookupFields[y].FieldValue;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                // now select the students that should be selected from the team meeeting
                                if (scope.teamMeetingStudents) {
                                    for (var n = 0; n < scope.teamMeetingStudents.length; n++) {
                                        if (scope.teamMeetingStudents[n].StudentID === scope.observationSummaryManager.Scores.StudentResults[j].StudentId) {
                                            // increment numstudents too
                                            scope.observationSummaryManager.Scores.StudentResults[j].selected = true;
                                        }
                                    }
                                }
                            }


                        }
                        scope.observationSummaryManager.LoadData(scope.selectedStudentId, "1,3").then(function (response) { attachFieldsCallback(); });

                        // delegate sorting to the sort manager
                        scope.sort = function (column) {
                            scope.sortMgr.sort(column);
                        };

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
                                                //	return 'obsPerfect';
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
                                    return getIntColor(gradeId, studentFieldScore);
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
        ]);
})();