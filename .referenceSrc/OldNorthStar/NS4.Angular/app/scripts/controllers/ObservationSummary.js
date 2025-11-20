(function () {
	'use strict'

	angular
	.module('observationSummaryModule', [])
	.controller('ObservationSummaryClassController', ObservationSummaryClassController)
    .controller('ObservationSummaryClassMultipleController', ObservationSummaryClassMultipleController)
        .controller('ObservationSummaryClassMultipleColumnController', ObservationSummaryClassMultipleColumnController)
        .controller('ObservationSummaryFilteredController', ObservationSummaryFilteredController)
        .factory('NSObservationSummarySectionManager', [
        '$http', 'webApiBaseUrl', 'nsLookupFieldService', 'spinnerService', function ($http, webApiBaseUrl, nsLookupFieldService, spinnerService) {
            var NSObservationSummarySectionManager = function () {

                var self = this;
                self.initialize = function () {

                }

                self.LoadData = function (sectionId, tddId) {
                    spinnerService.show('tableSpinner');

                    var url = webApiBaseUrl + '/api/assessment/GetClassObservationSummary/';
                    //var paramObj = {}
                    var summaryData = $http.get(url + sectionId + '/' + tddId);

                    self.LookupLists = [];
                    self.Scores = [];
                    self.BenchmarksByGrade = [];

                    return summaryData.then(function (response) {
                        angular.extend(self, response.data);
                        self.LookupLists = nsLookupFieldService.LookupFieldsArray;
                        //if (self.LookupLists === null) self.LookupLists = [];
                        if (self.Scores === null) self.Scores = [];
                        if (self.BenchmarksByGrade === null) self.BenchmarksByGrade = [];
                        return;
                    }).finally(function(response) {
                        spinnerService.hide('tableSpinner');
                    });
                }
            };

            return (NSObservationSummarySectionManager);
        }
        ])
        .factory('NSObservationSummarySectionMultipleManager', [
        '$http', 'webApiBaseUrl', 'nsLookupFieldService', 'spinnerService', function ($http, webApiBaseUrl, nsLookupFieldService, spinnerService) {
            var NSObservationSummarySectionMultipleManager = function (isMultiColumn) {

                var self = this;
                self.initialize = function () {

                }

                self.LoadData = function (sectionId, tdds) {
                    spinnerService.show('tableSpinner');

                    // temporary
                    
                    var paramObj = { SectionId: sectionId, TestDueDates: tdds, IsMultiColumn: isMultiColumn };

                    var url = webApiBaseUrl + (isMultiColumn ? '/api/assessment/GetClassObservationSummaryMultipleColumns' : '/api/assessment/GetClassObservationSummaryMultiple');
                    //var paramObj = {}
                    var summaryData = $http.post(url, paramObj);

                    self.LookupLists = [];
                    self.Scores = [];
                    self.BenchmarksByGrade = [];
                    self.FieldsWithDates = [];
                    self.StudentResultsByDates = [];

                    return summaryData.then(function (response) {
                        angular.extend(self, response.data);
                        self.LookupLists = nsLookupFieldService.LookupFieldsArray;
                        //if (self.LookupLists === null) self.LookupLists = [];
                        if (self.Scores === null) self.Scores = [];
                        if (self.BenchmarksByGrade === null) self.BenchmarksByGrade = [];
                        return;
                    }).finally(function (response) {
                        spinnerService.hide('tableSpinner');
                    });
                }
            };

            return (NSObservationSummarySectionMultipleManager);
        }
                ])
        .directive('nsObservationSummaryTm', [
			'$routeParams', '$compile', '$templateCache', '$http', 'nsFilterOptionsService', '$filter', 'NSObservationSummaryTeamMeetingManager', 'NSSortManager', '$timeout',
            function ($routeParams, $compile, $templateCache, $http, nsFilterOptionsService, $filter, NSObservationSummaryTeamMeetingManager, NSSortManager, $timeout) {

                return {
                    restrict: 'E',
                    templateUrl: 'templates/observation-summary-tm.html',
                    scope: {
                        selectedTeamMeetingId: '=',
                        selectedStaffId: '=',
                        selectedBenchmarkDateId: '='
                    },
                    link: function (scope, element, attr) {
                        scope.observationSummaryManager = new NSObservationSummaryTeamMeetingManager();
                        scope.filterOptions = nsFilterOptionsService.options;
                        scope.manualSortHeaders = {};
                        scope.manualSortHeaders.firstNameHeaderClass = "fa";
                        scope.manualSortHeaders.lastNameHeaderClass = "fa";
                        scope.sortArray = [];
                        scope.headerClassArray = [];
                        scope.allSelected = false;
                        scope.sortMgr = new NSSortManager();


                        scope.$watch('selectedStaffId', function (newVal, oldVal) {
                            if (newVal !== oldVal) {
                                scope.observationSummaryManager.LoadData(scope.selectedTeamMeetingId, scope.selectedBenchmarkDateId, scope.selectedStaffId).then(function () { attachFieldsCallback() });
                            }
                        });

                        scope.$on('NSFieldsUpdated', function (event, data) {
                            scope.observationSummaryManager.LoadData(scope.selectedTeamMeetingId, scope.selectedBenchmarkDateId, scope.selectedStaffId).then(function () { attachFieldsCallback() });
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

                            // if there are no records, broadcast it
                            if (!angular.isDefined(scope.observationSummaryManager.Scores.StudentResults) || scope.observationSummaryManager.Scores.StudentResults.length === 0) {
                                $timeout(function () {
                                    scope.$emit('nsNoRecords');
                                }, 750);
                            }
                        }
                        scope.observationSummaryManager.LoadData(scope.selectedTeamMeetingId, scope.selectedBenchmarkDateId, scope.selectedStaffId).then(function () { attachFieldsCallback() });

                        // delegate sorting to the sort manager
                        scope.sort = function (column) {
                            scope.sortMgr.sort(column);
                        };


                        function getIntColor(gradeId, studentFieldScore, fieldValue) {
                            var benchmarkArray = null;
                            for (var i = 0; i < scope.observationSummaryManager.BenchmarksByGrade.length; i++) {

                                if (scope.observationSummaryManager.BenchmarksByGrade[i].GradeId == gradeId) {
                                    benchmarkArray = scope.observationSummaryManager.BenchmarksByGrade[i];
                                }

                                if (benchmarkArray != null) {
                                    for (var j = 0; j < benchmarkArray.Benchmarks.length; j++) {
                                        if (benchmarkArray.Benchmarks[j].DbColumn === studentFieldScore.DbColumn && benchmarkArray.Benchmarks[j].AssessmentId === studentFieldScore.AssessmentId) {
                                            if (fieldValue != null) {
                                                // not defined yet
                                                //if (studentFieldScore.DecimalValue === $scope.Benchmarks[i].MaxScore) {
                                                //	return 'obsGreen';
                                                //}
                                                if (fieldValue >= benchmarkArray.Benchmarks[j].Exceeds && benchmarkArray.Benchmarks[j].Exceeds != null) {
                                                    return 'obsBlue';
                                                }
                                                if (fieldValue >= benchmarkArray.Benchmarks[j].Meets && benchmarkArray.Benchmarks[j].Meets != null) {
                                                    return 'obsGreen';
                                                }
                                                if (fieldValue >= benchmarkArray.Benchmarks[j].Approaches && benchmarkArray.Benchmarks[j].Approaches != null) {
                                                    return 'obsYellow';
                                                }
                                                if (fieldValue < benchmarkArray.Benchmarks[j].Approaches && benchmarkArray.Benchmarks[j].Approaches != null) {
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
                                    return getDecimalColor(gradeId, studentFieldScore, studentFieldScore.DecimalValue);
                                    break;
                                case 'DropdownRange':
                                    return getIntColor(gradeId, studentFieldScore, studentFieldScore.IntValue);
                                    break;
                                case 'DropdownFromDB':
                                    return getIntColor(gradeId, studentFieldScore, studentFieldScore.IntValue);
                                    break;
                                case 'CalculatedFieldClientOnly':
                                    return '';
                                    break;
                                case 'CalculatedFieldDbBacked':
                                    return getIntColor(gradeId, studentFieldScore, studentFieldScore.IntValue);
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
          .directive('nsObservationSummaryFiltered', [
			'$routeParams', '$compile', '$templateCache', '$http', 'nsFilterOptionsService', '$filter', 'NSObservationSummarySectionManager', 'NSSortManager', 'observationSummaryAssessmentFieldChooserSvc', 'nsPinesService','$timeout','$location',
            function ($routeParams, $compile, $templateCache, $http, nsFilterOptionsService, $filter, NSObservationSummarySectionManager, NSSortManager, observationSummaryAssessmentFieldChooserSvc, nsPinesService, $timeout, $location) {

                return {
                    restrict: 'E',
                    template: '<div ng-include="templateUrl"></div>',
                    //templateUrl: 'templates/observation-summary-filtered-button.html',
                    scope: {
                        groupFactory: '=',
                        sortArray: '='
                    },
                    link: function (scope, element, attr) {
                        scope.observationSummaryManager = scope.groupFactory;
                        scope.filterOptions = scope.groupFactory.options;
                        scope.manualSortHeaders = {};
                        scope.manualSortHeaders.studentNameHeaderClass = "fa";
                        scope.manualSortHeaders.gradeNameHeaderClass = "fa";
                        scope.manualSortHeaders.schoolNameHeaderClass = "fa";
                        scope.manualSortHeaders.teacherNameHeaderClass = "fa";
                        scope.manualSortHeaders['SpecialED'] = "fa";
                        scope.manualSortHeaders['Services'] = "fa";
                        scope.manualSortHeaders['Att1'] = "fa";
                        scope.manualSortHeaders['Att2'] = "fa";
                        scope.manualSortHeaders['Att3'] = "fa";
                        scope.manualSortHeaders['Att4'] = "fa";
                        scope.manualSortHeaders['Att5'] = "fa";
                        scope.manualSortHeaders['Att6'] = "fa";
                        scope.manualSortHeaders['Att7'] = "fa";
                        scope.manualSortHeaders['Att8'] = "fa";
                        scope.manualSortHeaders['Att9'] = "fa";
                        scope.headerClassArray = [];
                        scope.allSelected = false;
                        scope.sortMgr = new NSSortManager();
                        scope.fieldChooser = observationSummaryAssessmentFieldChooserSvc;
                        scope.settings = { graphGenerated: false };

                        // infinite scroll function call
                        scope.loadMoreRecords = function () {
                            $timeout(function () {

                                scope.observationSummaryManager.busy = true;
                                scope.observationSummaryManager.loadOSInfinityRecords();
                               // attachFieldsCallback();
                                scope.observationSummaryManager.busy = false;
                            }, 100);
                        };

                        
                        scope.generateGraph = function () {
                            scope.observationSummaryManager.loadOSData().then(function (response) {
                                attachFieldsCallback();
                                scope.settings.graphGenerated = true;
                            });
                        }

                        scope.hideField = function (field) {
                            observationSummaryAssessmentFieldChooserSvc.hideField(field).then(function (response) {
                                scope.observationSummaryManager.loadOSData().then(function (response) {
                                    attachFieldsCallback();
                                });
                            });
                        }

                        scope.hideOSColumn = function (column) {
                            observationSummaryAssessmentFieldChooserSvc.hideOSColumn(column).then(function (response) {
                                scope.observationSummaryManager.loadOSData().then(function (response) {
                                    attachFieldsCallback();
                                });
                            });
                        }

                        scope.hideAssessment = function (assessment) {
                            observationSummaryAssessmentFieldChooserSvc.hideAssessment(assessment).then(function (response) {
                                scope.observationSummaryManager.loadOSData().then(function (response) {
                                    attachFieldsCallback();
                                });
                            });
                        }

                        scope.$on('NSFieldsUpdated', function (event, data) {
                            scope.observationSummaryManager.loadOSData().then(function (response) {
                                attachFieldsCallback();
                            });
                        });

                        scope.$on('NSStudentAttributesUpdated', function (event, data) {
                            scope.observationSummaryManager.loadOSData().then(function (response) {
                                attachFieldsCallback();
                            });
                        });
         
                        scope.changeTeamMeetingStudentSelection = function (studentResult) {
                            if (studentResult.selected) {
                                // add student to the collection
                                scope.teamMeetingStudents.push({ ID: -1, TeamMeetingID: -1, SchoolID: -1, StaffID: scope.selectedStaffId, SectionID: scope.selectedSectionId, StudentID: studentResult.StudentId, Notes: null });
                                for (var i = 0; i < scope.teamMeetingSections.length; i++) {
                                    if (scope.teamMeetingSections[i].Id == scope.selectedSectionId) {
                                        scope.teamMeetingSections[i].NumStudents++;
                                    }
                                }
                            }
                            else {
                                // remove student from the collection
                                // TODO: Note, if a student is in more than one class, he will be removed from ALL classes with this logic.  Its probably fine, 
                                // but we can do something else if need be
                                for (var n = 0; n < scope.teamMeetingStudents.length; n++) {
                                    if (scope.teamMeetingStudents[n].StudentID === studentResult.StudentId) {
                                        scope.teamMeetingStudents.splice(n, 1);
                                    }
                                }

                                for (var i = 0; i < scope.teamMeetingSections.length; i++) {
                                    if (scope.teamMeetingSections[i].Id == scope.selectedSectionId) {
                                        scope.teamMeetingSections[i].NumStudents--;
                                    }
                                }
                            }
                        };

                        scope.printPages = [];
                        // printing setup
                        if ($location.absUrl().indexOf('printmode=') >= 0) {
                            scope.templateUrl = 'templates/observation-summary-filtered-button-print.html';

                            var groupsParam = JSON.parse(decodeURIComponent($location.search().GroupsParam));

                            // set selected items from deserialed groups and generate
                            //scope.groupFactory.options.selectedAssessmentField = groupsParam.selectedAssessmentField;
                            scope.groupFactory.options.selectedEducationLabels = groupsParam.selectedEducationLabels;
                            scope.groupFactory.options.selectedSchoolYear = groupsParam.selectedSchoolYear;
                            scope.groupFactory.options.selectedSchools = groupsParam.selectedSchools;
                            scope.groupFactory.options.selectedTeachers = groupsParam.selectedTeachers;
                            scope.groupFactory.options.selectedSections = groupsParam.selectedSections;
                            scope.groupFactory.options.selectedStudents = groupsParam.selectedStudents;
                            scope.groupFactory.options.selectedInterventionTypes = groupsParam.selectedInterventionTypes;
                            scope.groupFactory.options.selectedGrades = groupsParam.selectedGrades;
                            scope.groupFactory.options.selectedTestDueDate = groupsParam.selectedTestDueDate;
                            scope.groupFactory.options.attributeTypes = groupsParam.attributeTypes;
                            
                            scope.observationSummaryManager.loadOSData().then(function (response) {
                                attachFieldsCallback();
                                scope.settings.graphGenerated = true;
                            });
                        } else {
                            scope.templateUrl = 'templates/observation-summary-filtered-button.html';
                        }


                        var attachFieldsCallback = function () {
                            // initialize the sort manager now that the data has been loaded
                            scope.sortMgr.initialize(scope.manualSortHeaders, scope.sortArray, scope.headerClassArray, 'OSFieldResults', scope.observationSummaryManager.Scores);
                            
                            if (!angular.isDefined(scope.observationSummaryManager.Scores.StudentResults))
                            { return;}


                            if ($location.absUrl().indexOf('printmode=') >= 0) {

                                // add curent page to array
                                var currentPageNo = 1;
                                var currentHeaderCount = 0;
                                //scope.printPages.push(currentPage);

                                // set page for headers
                                var currentPage = { page: currentPageNo, assessments: [], currentHeaderCount: 0 };
                                scope.printPages.push(currentPage);
                                for (var p = 0; p < scope.observationSummaryManager.Scores.HeaderGroups.length; p++) {
                                    currentHeaderCount += scope.observationSummaryManager.Scores.HeaderGroups[p].FieldCount;


                                    // if less than 15, add to current page
                                    if (currentHeaderCount <= 15) {
                                        scope.observationSummaryManager.Scores.HeaderGroups[p].page = currentPageNo;
                                        currentPage.assessments.push(scope.observationSummaryManager.Scores.HeaderGroups[p].AssessmentId);
                                        currentPage.currentHeaderCount = currentHeaderCount;
                                    } else {
                                        // the header groups create the pages in the page array
                                        currentPageNo++;
                                        currentPage = { page: currentPageNo, assessments: [], currentHeaderCount: scope.observationSummaryManager.Scores.HeaderGroups[p].FieldCount };
                                        currentPage.assessments.push(scope.observationSummaryManager.Scores.HeaderGroups[p].AssessmentId);
                                        scope.printPages.push(currentPage);
                                        scope.observationSummaryManager.Scores.HeaderGroups[p].page = currentPageNo;
                                        currentHeaderCount = scope.observationSummaryManager.Scores.HeaderGroups[p].FieldCount;
                                    }
                                }

                                // set page for fields
                                for (var p = 0; p < scope.observationSummaryManager.Scores.Fields.length; p++) {
                                    for (var x = 0; x < scope.printPages.length; x++) {
                                        for (var y = 0; y < scope.printPages[x].assessments.length; y++) {
                                            if (scope.printPages[x].assessments[y] == scope.observationSummaryManager.Scores.Fields[p].AssessmentId) {
                                                scope.observationSummaryManager.Scores.Fields[p].page = scope.printPages[x].page;
                                            }
                                        }
                                    }
                                }

                                // set page for OSFieldREsults
                                for (var j = 0; j < scope.observationSummaryManager.Scores.StudentResults.length; j++) {
                                    for (var k = 0; k < scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults.length; k++) {
                                        for (var x = 0; x < scope.printPages.length; x++) {
                                            for (var y = 0; y < scope.printPages[x].assessments.length; y++) {
                                                if (scope.printPages[x].assessments[y] == scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].AssessmentId) {
                                                    scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].page = scope.printPages[x].page;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            for (var j = 0; j < scope.observationSummaryManager.Scores.StudentResults.length; j++) {
                                for (var k = 0; k < scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults.length; k++) {
                                    for (var i = 0; i < scope.observationSummaryManager.Scores.Fields.length; i++) {
                                        if (scope.observationSummaryManager.Scores.Fields[i].DatabaseColumn == scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].DbColumn) {
                                            scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].Field = angular.copy(scope.observationSummaryManager.Scores.Fields[i]);

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

                            // sort if part of url param
                            //if ($location.absUrl().indexOf('SortParam=') > 0) {
                            //    var sortParamArray = $location.search().SortParam.split(',');

                            //    for (var i = 0; i < sortParamArray.length; i++) {
                            //        var currentParam = decodeURIComponent(sortParamArray[i]);

                            //        var matches = currentParam.match(/\[(.*?)\]/);
                            //        var doubleUp = currentParam.indexOf('-') > -1;

                            //        // if we are able to get a number
                            //        if (matches) {
                            //            var index = matches[1];
                            //            scope.sort(index);
                            //            if (doubleUp) {
                            //                scope.sort(index);
                            //            }
                            //        } else {
                            //            scope.sort(currentParam);
                            //            if (doubleUp) {
                            //                scope.sort(currentParam);
                            //            }
                            //        }
                            //    }
                            //}
                        }
                        //scope.observationSummaryManager.loadOSData().then(function (response) {
                        //    attachFieldsCallback();
                        //});

                        // delegate sorting to the sort manager
                        scope.sort = function (column) {
                            scope.sortMgr.sort(column);
                        };
                                               
                        function getIntColor(gradeId, studentFieldScore, fieldValue) {
                            var benchmarkArray = null;
                            for (var i = 0; i < scope.observationSummaryManager.BenchmarksByGrade.length; i++) {

                                // if this is a state test, use the statetestgrade instead of the one for the overall result
                                if (studentFieldScore.TestTypeId == 3) {
                                    gradeId = studentFieldScore.StateGradeId;
                                }

                                if (scope.observationSummaryManager.BenchmarksByGrade[i].GradeId == gradeId) {
                                    benchmarkArray = scope.observationSummaryManager.BenchmarksByGrade[i];
                                }

                                if (benchmarkArray != null) {
                                    for (var j = 0; j < benchmarkArray.Benchmarks.length; j++) {
                                        if (benchmarkArray.Benchmarks[j].DbColumn === studentFieldScore.DbColumn && benchmarkArray.Benchmarks[j].AssessmentId === studentFieldScore.AssessmentId) {
                                            if (fieldValue != null) {
                                                // not defined yet
                                                //if (studentFieldScore.DecimalValue === $scope.Benchmarks[i].MaxScore) {
                                                //	return 'obsPerfect';
                                                //}
                                                if (fieldValue >= benchmarkArray.Benchmarks[j].Exceeds && benchmarkArray.Benchmarks[j].Exceeds != null) {
                                                    return 'obsBlue';
                                                }
                                                if (fieldValue >= benchmarkArray.Benchmarks[j].Meets && benchmarkArray.Benchmarks[j].Meets != null) {
                                                    return 'obsGreen';
                                                }
                                                if (fieldValue >= benchmarkArray.Benchmarks[j].Approaches && benchmarkArray.Benchmarks[j].Approaches != null) {
                                                    return 'obsYellow';
                                                }
                                                if (fieldValue < benchmarkArray.Benchmarks[j].Approaches && benchmarkArray.Benchmarks[j].Approaches != null) {
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
                                    return getIntColor(gradeId, studentFieldScore, studentFieldScore.DecimalValue);
                                    break;
                                case 'DropdownRange':
                                    return getIntColor(gradeId, studentFieldScore, studentFieldScore.IntValue);
                                    break;
                                case 'DropdownFromDB':
                                    return getIntColor(gradeId, studentFieldScore, studentFieldScore.IntValue);
                                    break;
                                case 'CalculatedFieldClientOnly':
                                    return '';
                                    break;
                                case 'CalculatedFieldDbBacked':
                                    return getIntColor(gradeId, studentFieldScore, studentFieldScore.IntValue);
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
    .directive('nsObservationSummarySection', [
			'$routeParams', '$compile', '$templateCache', '$http', 'nsFilterOptionsService', '$filter', 'NSObservationSummarySectionManager', 'NSSortManager', 'observationSummaryAssessmentFieldChooserSvc', '$location',
            function ($routeParams, $compile, $templateCache, $http, nsFilterOptionsService, $filter, NSObservationSummarySectionManager, NSSortManager, observationSummaryAssessmentFieldChooserSvc, $location) {

                return {
                    restrict: 'E',
                    template: '<div ng-include="templateUrl"></div>',
                    //templateUrl: 'templates/observation-summary-section-print.html',
                    scope: {
                        selectedSectionId: '=',
                        selectedStaffId: '=',
                        selectedBenchmarkDateId: '=',
                        selectedAssessmentIds: '=',
                        showCheckboxes: '=',
                        teamMeetingStudents: '=',
                        teamMeetingSections: '=',
                        sortArray: '='
                    },
                    link: function (scope, element, attr) {
                        scope.observationSummaryManager = new NSObservationSummarySectionManager();
                        scope.filterOptions = nsFilterOptionsService.options;
                        scope.manualSortHeaders = {};
                        scope.manualSortHeaders.studentNameHeaderClass = "fa";
                                                
                        scope.headerClassArray = [];
                        scope.allSelected = false;
                        scope.sortMgr = new NSSortManager();
                        scope.fieldChooser = observationSummaryAssessmentFieldChooserSvc;

                        scope.hideField = function (field) {
                            observationSummaryAssessmentFieldChooserSvc.hideField(field).then(function (response) {
                                scope.observationSummaryManager.LoadData(scope.selectedSectionId, scope.selectedBenchmarkDateId).then(function (response) {
                                    attachFieldsCallback();
                                });
                            });
                        }

                        scope.hideAssessment = function (assessment) {
                            observationSummaryAssessmentFieldChooserSvc.hideAssessment(assessment).then(function (response) {
                                scope.observationSummaryManager.LoadData(scope.selectedSectionId, scope.selectedBenchmarkDateId).then(function (response) {
                                    attachFieldsCallback();
                                });
                            });
                        }

                        scope.$on('NSFieldsUpdated', function (event, data) {
                            scope.observationSummaryManager.LoadData(scope.selectedSectionId, scope.selectedBenchmarkDateId).then(function (response) {
                                attachFieldsCallback();
                            });
                        });

                        scope.$on('NSStudentAttributesUpdated', function (event, data) {
                            scope.observationSummaryManager.LoadData(scope.selectedSectionId, scope.selectedBenchmarkDateId).then(function (response) {
                                attachFieldsCallback();
                            });
                        });

                        scope.$watch('selectedSectionId', function (newValue, oldValue) {
                            if (newValue != oldValue && newValue != null) {

                            scope.observationSummaryManager.LoadData(scope.selectedSectionId, scope.selectedBenchmarkDateId).then(function (response) {
                                attachFieldsCallback();
                            });
                            }

                        });

                        scope.selectAllStudents = function (selected) {
                            if (selected) {
                                // add student to the collection
                                // see if the student is already in the collection, add, if not
                                // loop over all students in the section
                                
                                for (var j = 0; j < scope.observationSummaryManager.Scores.StudentResults.length; j++) {
                                    // get current student
                                    var currentStudent = scope.observationSummaryManager.Scores.StudentResults[j];

                                    var studentAlreadyOnList = false;
                                    for (var n = 0; n < scope.teamMeetingStudents.length; n++) {
                                        if (scope.teamMeetingStudents[n].StudentID === currentStudent.StudentId) {
                                            // student is alrady on list
                                            studentAlreadyOnList = true;
                                            break;
                                        }
                                    }

                                    if (!studentAlreadyOnList) {
                                        currentStudent.selected = true;
                                        scope.teamMeetingStudents.push({ ID: -1, TeamMeetingID: -1, SchoolID: -1, StaffID: scope.selectedStaffId, SectionID: scope.selectedSectionId, StudentID: currentStudent.StudentId, Notes: null });
                                        for (var i = 0; i < scope.teamMeetingSections.length; i++) {
                                            if (scope.teamMeetingSections[i].Id == scope.selectedSectionId) {
                                                scope.teamMeetingSections[i].NumStudents++;
                                            }
                                        }
                                    }
                                }

                            }
                            else {
                                for (var j = 0; j < scope.observationSummaryManager.Scores.StudentResults.length; j++) {
                                    // get current student
                                    var currentStudent = scope.observationSummaryManager.Scores.StudentResults[j];
                                    // remove student from the collection
                                    // TODO: Note, if a student is in more than one class, he will be removed from ALL classes with this logic.  Its probably fine, 
                                    // but we can do something else if need be
                                    var studentRemoved = false;
                                    for (var n = 0; n < scope.teamMeetingStudents.length; n++) {
                                        if (scope.teamMeetingStudents[n].StudentID === currentStudent.StudentId) {
                                            scope.teamMeetingStudents.splice(n, 1);
                                            studentRemoved = true;
                                            currentStudent.selected = false;
                                        }
                                    }

                                    if (studentRemoved) {
                                        for (var i = 0; i < scope.teamMeetingSections.length; i++) {
                                            if (scope.teamMeetingSections[i].Id == scope.selectedSectionId) {
                                                scope.teamMeetingSections[i].NumStudents--;
                                            }
                                        }
                                    }
                                }
                            }
                        }


                        scope.changeTeamMeetingStudentSelection = function (studentResult) {
                            if (studentResult.selected) {
                                // add student to the collection
                                scope.teamMeetingStudents.push({ ID: -1, TeamMeetingID: -1, SchoolID: -1, StaffID: scope.selectedStaffId, SectionID: scope.selectedSectionId, StudentID: studentResult.StudentId, Notes: null });
                                for (var i = 0; i < scope.teamMeetingSections.length; i++) {
                                    if (scope.teamMeetingSections[i].Id == scope.selectedSectionId) {
                                        scope.teamMeetingSections[i].NumStudents++;
                                    }
                                }
                            }
                            else {
                                // remove student from the collection
                                // TODO: Note, if a student is in more than one class, he will be removed from ALL classes with this logic.  Its probably fine, 
                                // but we can do something else if need be
                                for (var n = 0; n < scope.teamMeetingStudents.length; n++) {
                                    if (scope.teamMeetingStudents[n].StudentID === studentResult.StudentId) {
                                        // make sure no notes are recorded for this student
                                        if (scope.teamMeetingStudents[n].Notes != null) {
                                            // this student has notes recorded
                                            $
                                        }
                                        scope.teamMeetingStudents.splice(n, 1);
                                    }
                                }

                                for (var i = 0; i < scope.teamMeetingSections.length; i++) {
                                    if (scope.teamMeetingSections[i].Id == scope.selectedSectionId) {
                                        scope.teamMeetingSections[i].NumStudents--;
                                    }
                                }
                            }
                        };

                        scope.printPages = [];
                        // printing setup
                        if ($location.absUrl().indexOf('printmode=') >= 0) {
                            scope.templateUrl = 'templates/observation-summary-section-print.html';
                        } else {
                            scope.templateUrl = 'templates/observation-summary-section.html';
                        }

                        var attachFieldsCallback = function () {
                            // initialize the sort manager now that the data has been loaded
                            scope.sortMgr.initialize(scope.manualSortHeaders, scope.sortArray, scope.headerClassArray, 'OSFieldResults', scope.observationSummaryManager.Scores);
                            
                            if ($location.absUrl().indexOf('printmode=') >= 0) {

                                // add curent page to array
                                var currentPageNo = 1;
                                var currentHeaderCount = 0;
                                //scope.printPages.push(currentPage);

                                // set page for headers
                                var currentPage = { page: currentPageNo, assessments: [], currentHeaderCount: 0 };
                                scope.printPages.push(currentPage);
                                for (var p = 0; p < scope.observationSummaryManager.Scores.HeaderGroups.length; p++) {
                                    currentHeaderCount += scope.observationSummaryManager.Scores.HeaderGroups[p].FieldCount;
                                                                        
               
                                    // if less than 15, add to current page
                                    if (currentHeaderCount <= 15) {
                                        scope.observationSummaryManager.Scores.HeaderGroups[p].page = currentPageNo;
                                        currentPage.assessments.push(scope.observationSummaryManager.Scores.HeaderGroups[p].AssessmentId);
                                        currentPage.currentHeaderCount = currentHeaderCount;
                                    } else {
                                        // the header groups create the pages in the page array
                                        currentPageNo++;
                                        currentPage = { page: currentPageNo, assessments: [], currentHeaderCount: scope.observationSummaryManager.Scores.HeaderGroups[p].FieldCount };
                                        currentPage.assessments.push(scope.observationSummaryManager.Scores.HeaderGroups[p].AssessmentId);
                                        scope.printPages.push(currentPage);
                                        scope.observationSummaryManager.Scores.HeaderGroups[p].page = currentPageNo;
                                        currentHeaderCount = scope.observationSummaryManager.Scores.HeaderGroups[p].FieldCount;
                                    }
                                }
                                
                                // set page for fields
                                for (var p = 0; p < scope.observationSummaryManager.Scores.Fields.length; p++) {
                                    for (var x = 0; x < scope.printPages.length; x++) {
                                        for (var y = 0; y < scope.printPages[x].assessments.length; y++) {
                                            if (scope.printPages[x].assessments[y] == scope.observationSummaryManager.Scores.Fields[p].AssessmentId) {
                                                scope.observationSummaryManager.Scores.Fields[p].page = scope.printPages[x].page;
                                            }
                                        }
                                    }
                                }

                                // set page for OSFieldREsults
                                for (var j = 0; j < scope.observationSummaryManager.Scores.StudentResults.length; j++) {
                                    for (var k = 0; k < scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults.length; k++) {
                                        for (var x = 0; x < scope.printPages.length; x++) {
                                            for (var y = 0; y < scope.printPages[x].assessments.length; y++) {
                                                if (scope.printPages[x].assessments[y] == scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].AssessmentId) {
                                                    scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].page = scope.printPages[x].page;
                                                }
                                            }
                                        }
                                    }
                                }
                            }


                            for (var j = 0; j < scope.observationSummaryManager.Scores.StudentResults.length; j++) {
                                for (var k = 0; k < scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults.length; k++) {
                                    for (var i = 0; i < scope.observationSummaryManager.Scores.Fields.length; i++) {
                                        if (scope.observationSummaryManager.Scores.Fields[i].DatabaseColumn == scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].DbColumn) {
                                            scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].Field = angular.copy(scope.observationSummaryManager.Scores.Fields[i]);
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

                            // sort if part of url param
                            //if ($location.absUrl().indexOf('SortParam=') > 0) {
                            //    var sortParamArray = $location.search().SortParam.split(',');

                            //    for (var i = 0; i < sortParamArray.length; i++) {
                            //        var currentParam = decodeURIComponent(sortParamArray[i]);

                            //        var matches = currentParam.match(/\[(.*?)\]/);
                            //        var doubleUp = currentParam.indexOf('-') > -1;

                            //        // if we are able to get a number
                            //        if (matches) {
                            //            var index = matches[1];
                            //            scope.sort(index);
                            //            if (doubleUp) {
                            //                scope.sort(index);
                            //            }
                            //        } else {
                            //            scope.sort(currentParam);
                            //            if (doubleUp) {
                            //                scope.sort(currentParam);
                            //            }
                            //        }
                            //    }
                            //}

                        }
                        scope.observationSummaryManager.LoadData(scope.selectedSectionId, scope.selectedBenchmarkDateId).then(function (response) {
                            attachFieldsCallback();
                        });

                        // delegate sorting to the sort manager
                        scope.sort = function (column) {
                            scope.sortMgr.sort(column);
                        };

                        // sort function for multiple page printing
                        scope.sortMultiPage = function (column, page) {
                            if (page == 1) {
                                scope.sortMgr.sort(column);
                            } else {
                                var trueIndex = 0;
                                for (var i = 0; i < scope.printPages.length; i++) {
                                    if (scope.printPages[i].page == page) {
                                        trueIndex += column;
                                        break; // quit when you get to the right page
                                    } else {
                                        trueIndex += scope.printPages[i].currentHeaderCount;
                                    }
                                }
                                scope.sortMgr.sort(trueIndex);
                            }
                        };
       

                        function getIntColor(gradeId, studentFieldScore, fieldValue) {
                            var benchmarkArray = null;
                            for (var i = 0; i < scope.observationSummaryManager.BenchmarksByGrade.length; i++) {

                                // if this is a state test, use the statetestgrade instead of the one for the overall result
                                if (studentFieldScore.TestTypeId == 3) {
                                    gradeId = studentFieldScore.StateGradeId;
                                }

                                if (scope.observationSummaryManager.BenchmarksByGrade[i].GradeId == gradeId) {
                                    benchmarkArray = scope.observationSummaryManager.BenchmarksByGrade[i];
                                }

                                if (benchmarkArray != null) {
                                    for (var j = 0; j < benchmarkArray.Benchmarks.length; j++) {
                                        if (benchmarkArray.Benchmarks[j].DbColumn === studentFieldScore.DbColumn && benchmarkArray.Benchmarks[j].AssessmentId === studentFieldScore.AssessmentId) {
                                            if (fieldValue != null) {
                                                // not defined yet
                                                //if (studentFieldScore.DecimalValue === $scope.Benchmarks[i].MaxScore) {
                                                //	return 'obsPerfect';
                                                //}
                                                if (fieldValue >= benchmarkArray.Benchmarks[j].Exceeds && benchmarkArray.Benchmarks[j].Exceeds != null) {
                                                    return 'obsBlue';
                                                }
                                                if (fieldValue >= benchmarkArray.Benchmarks[j].Meets && benchmarkArray.Benchmarks[j].Meets != null) {
                                                    return 'obsGreen';
                                                }
                                                if (fieldValue >= benchmarkArray.Benchmarks[j].Approaches && benchmarkArray.Benchmarks[j].Approaches != null) {
                                                    return 'obsYellow';
                                                }
                                                if (fieldValue < benchmarkArray.Benchmarks[j].Approaches && benchmarkArray.Benchmarks[j].Approaches != null) {
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
                                    return getIntColor(gradeId, studentFieldScore, studentFieldScore.DecimalValue);
                                    break;
                                case 'DropdownRange':
                                    return getIntColor(gradeId, studentFieldScore, studentFieldScore.IntValue);
                                    break;
                                case 'DropdownFromDB':
                                    return getIntColor(gradeId, studentFieldScore, studentFieldScore.IntValue);
                                    break;
                                case 'CalculatedFieldClientOnly':
                                    return '';
                                    break;
                                case 'CalculatedFieldDbBacked':
                                    return getIntColor(gradeId, studentFieldScore, studentFieldScore.IntValue);
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
    .directive('nsObservationSummarySectionMultiple', [
			'$routeParams', '$compile', '$templateCache', '$http', 'nsFilterOptionsService', '$filter', 'NSObservationSummarySectionMultipleManager', 'NSSortManager', 'observationSummaryAssessmentFieldChooserSvc', '$location',
            function ($routeParams, $compile, $templateCache, $http, nsFilterOptionsService, $filter, NSObservationSummarySectionMultipleManager, NSSortManager, observationSummaryAssessmentFieldChooserSvc, $location) {

                return {
                    restrict: 'E',
                    template: '<div ng-include="templateUrl"></div>',
                    //templateUrl: 'templates/observation-summary-section-print.html',
                    scope: {
                        selectedSectionId: '=',
                        selectedStaffId: '=',
                        selectedBenchmarkDateId: '=',
                        selectedAssessmentIds: '=',
                        showCheckboxes: '=',
                        teamMeetingStudents: '=',
                        teamMeetingSections: '=',
                        sortArray: '='
                    },
                    link: function (scope, element, attr) {
                        scope.observationSummaryManager = new NSObservationSummarySectionMultipleManager(false);
                        scope.filterOptions = nsFilterOptionsService.options;
                        scope.manualSortHeaders = {};
                        scope.manualSortHeaders.studentNameHeaderClass = "fa";
                        scope.initiallyLoaded = false;

                        scope.headerClassArray = [];
                        scope.allSelected = false;
                        scope.sortMgr = new NSSortManager();
                        scope.fieldChooser = observationSummaryAssessmentFieldChooserSvc;

                        scope.hideAssessment = function (assessment) {
                            observationSummaryAssessmentFieldChooserSvc.hideAssessment(assessment).then(function (response) {
                                scope.observationSummaryManager.LoadData(scope.selectedSectionId, scope.filterOptions.selectedBenchmarkDates).then(function (response) {
                                    attachFieldsCallback();
                                });
                            });
                        }

                        // if benchmark dates change, update page
                        scope.$on('NSTddMultiRangeOptionsUpdated', function (event, data) {
                            // don't run this again after the inital load
                            //if (scope.initiallyLoaded) {
                            //    scope.initiallyLoaded = false;
                            //    return;
                            //}
                            scope.observationSummaryManager.LoadData(scope.selectedSectionId, scope.filterOptions.selectedBenchmarkDates).then(function (response) {
                                attachFieldsCallback();
                            });
                        });

                        scope.$on('NSFieldsUpdated', function (event, data) {
                            scope.observationSummaryManager.LoadData(scope.selectedSectionId, scope.filterOptions.selectedBenchmarkDates).then(function (response) {
                                attachFieldsCallback();
                            });
                        });

                        scope.$on('NSStudentAttributesUpdated', function (event, data) {
                            scope.observationSummaryManager.LoadData(scope.selectedSectionId, scope.filterOptions.selectedBenchmarkDates).then(function (response) {
                                attachFieldsCallback();
                            });
                        });

                        scope.$watch('selectedSectionId', function (newValue, oldValue) {
                            if (newValue != oldValue && newValue != null) {

                                scope.observationSummaryManager.LoadData(scope.selectedSectionId, scope.filterOptions.selectedBenchmarkDates).then(function (response) {
                                    attachFieldsCallback();
                                });
                            }

                        });

                        scope.divider = function (index, parentIndex, maxRows) {
                            var maxCount = scope.observationSummaryManager.FieldsWithDates.length;
                            if (parentIndex) {
                                // if this is the last row
                                if (parentIndex == maxRows - 1) {
                                    if (index == maxCount - 1) {
                                        // special case for 1 selected date
                                        if (scope.filterOptions.selectedBenchmarkDates.length == 1) {
                                            return { 'border-right': '3px solid black', 'border-left': '3px solid black', 'border-bottom': '3px solid black' };
                                        } else {
                                            return { 'border-right': '3px solid black', 'border-bottom': '3px solid black' };
                                        }
                                    } else if (index % scope.filterOptions.selectedBenchmarkDates.length == 0 || index == 0) {
                                        return { 'border-left': '3px solid black', 'border-bottom': '3px solid black' };
                                    } else {
                                        return { 'border-bottom': '3px solid black' };
                                    }
                                } else {
                                    if (index == maxCount - 1) {
                                        // special case for 1 selected date
                                        if (scope.filterOptions.selectedBenchmarkDates.length == 1) {
                                            return { 'border-right': '3px solid black', 'border-left': '3px solid black' };
                                        } else {
                                            return { 'border-right': '3px solid black' };
                                        }
                                    } else if (index % scope.filterOptions.selectedBenchmarkDates.length == 0 || index == 0) {
                                        return { 'border-left': '3px solid black' };
                                    }
                                }
                            }
                            else {
                                if (index == maxCount - 1) {
                                    // special case for 1 selected date
                                    if (scope.filterOptions.selectedBenchmarkDates.length == 1) {
                                        return { 'border-right': '3px solid black', 'border-left': '3px solid black' };
                                    } else {
                                        return { 'border-right': '3px solid black' };
                                    }
                                } else if (index % scope.filterOptions.selectedBenchmarkDates.length == 0 || index == 0) {
                                    return { 'border-left': '3px solid black' };
                                } 
                            }
                        }

                        scope.printPages = [];
                        // printing setup
                        if ($location.absUrl().indexOf('printmode=') >= 0) {
                            scope.templateUrl = 'templates/observation-summary-section-multiple.html';
                        } else {
                            scope.templateUrl = 'templates/observation-summary-section-multiple.html';
                        }

                        var attachFieldsCallback = function () {
                            // initialize the sort manager now that the data has been loaded
                            //scope.sortMgr.initialize(scope.manualSortHeaders, scope.sortArray, scope.headerClassArray, 'OSFieldResults', scope.observationSummaryManager.Scores);

                            if ($location.absUrl().indexOf('printmode=') >= 0) {

                                // add curent page to array
                                var currentPageNo = 1;
                                var currentHeaderCount = 0;
                                //scope.printPages.push(currentPage);

                                // set page for headers
                                var currentPage = { page: currentPageNo, assessments: [], currentHeaderCount: 0 };
                                scope.printPages.push(currentPage);
                                for (var p = 0; p < scope.observationSummaryManager.Scores.HeaderGroups.length; p++) {
                                    currentHeaderCount += scope.observationSummaryManager.Scores.HeaderGroups[p].FieldCount;


                                    // if less than 15, add to current page
                                    if (currentHeaderCount <= 15) {
                                        scope.observationSummaryManager.Scores.HeaderGroups[p].page = currentPageNo;
                                        currentPage.assessments.push(scope.observationSummaryManager.Scores.HeaderGroups[p].AssessmentId);
                                        currentPage.currentHeaderCount = currentHeaderCount;
                                    } else {
                                        // the header groups create the pages in the page array
                                        currentPageNo++;
                                        currentPage = { page: currentPageNo, assessments: [], currentHeaderCount: scope.observationSummaryManager.Scores.HeaderGroups[p].FieldCount };
                                        currentPage.assessments.push(scope.observationSummaryManager.Scores.HeaderGroups[p].AssessmentId);
                                        scope.printPages.push(currentPage);
                                        scope.observationSummaryManager.Scores.HeaderGroups[p].page = currentPageNo;
                                        currentHeaderCount = scope.observationSummaryManager.Scores.HeaderGroups[p].FieldCount;
                                    }
                                }

                                // set page for fields
                                for (var p = 0; p < scope.observationSummaryManager.Scores.Fields.length; p++) {
                                    for (var x = 0; x < scope.printPages.length; x++) {
                                        for (var y = 0; y < scope.printPages[x].assessments.length; y++) {
                                            if (scope.printPages[x].assessments[y] == scope.observationSummaryManager.Scores.Fields[p].AssessmentId) {
                                                scope.observationSummaryManager.Scores.Fields[p].page = scope.printPages[x].page;
                                            }
                                        }
                                    }
                                }

                                // set page for OSFieldREsults
                                for (var j = 0; j < scope.observationSummaryManager.Scores.StudentResults.length; j++) {
                                    for (var k = 0; k < scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults.length; k++) {
                                        for (var x = 0; x < scope.printPages.length; x++) {
                                            for (var y = 0; y < scope.printPages[x].assessments.length; y++) {
                                                if (scope.printPages[x].assessments[y] == scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].AssessmentId) {
                                                    scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].page = scope.printPages[x].page;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            // set up dateFields
                            scope.observationSummaryManager.FieldsWithDates = [];
                            scope.observationSummaryManager.StudentResultsByDates = [];

                            var currentAssessment = 0;
                            for (var i = 0; i < scope.observationSummaryManager.Scores.Fields.length; i++) {
                                // only use the first field for each eassessment
                                if (currentAssessment != scope.observationSummaryManager.Scores.Fields[i].AssessmentId) {
                                    currentAssessment = scope.observationSummaryManager.Scores.Fields[i].AssessmentId;
                                    for (var p = 0; p < scope.filterOptions.selectedBenchmarkDates.length; p++) {
                                        // add this benchmark date and a reference to the first field for this assessment
                                        scope.observationSummaryManager.FieldsWithDates.push({ benchmarkDate: scope.filterOptions.selectedBenchmarkDates[p], field: scope.observationSummaryManager.Scores.Fields[i] });
                                    }
                                }
                            }

                            var currentStudentId = -1;
                            var newStudentResult = {};

                            // add a record for each student
                            for (var j = 0; j < scope.observationSummaryManager.Scores.StudentResults.length; j++) {
                                var csr = scope.observationSummaryManager.Scores.StudentResults[j];

                                // only add each student once
                                if (currentStudentId != csr.StudentId) {
                                    currentStudentId = csr.StudentId;
                                    newStudentResult = {
                                        StudentName: csr.StudentName,
                                        StudentId: csr.StudentId,
                                        GradeId: csr.GradeId,
                                        Att1: csr.Att1,
                                        Att2: csr.Att2,
                                        Att3: csr.Att3,
                                        Att4: csr.Att4,
                                        Att5: csr.Att5,
                                        Att6: csr.Att6,
                                        Att7: csr.Att7,
                                        Att8: csr.Att8,
                                        Att9: csr.Att9,
                                        OSFieldResults: []
                                    };
                                    scope.observationSummaryManager.StudentResultsByDates.push(newStudentResult);
                                }
                            }

                            // create new structure for student results by date
                            //for (var j = 0; j < scope.observationSummaryManager.Scores.StudentResults.length; j++) {
                            //    var csr = scope.observationSummaryManager.Scores.StudentResults[j];

                            //    // only add each student once
                            //    if (currentStudentId != csr.StudentId) {
                            //        newStudentResult = { StudentName: csr.StudentName, StudentId: csr.StudentId, GradeId: csr.GradeId, OSFieldResults: [] };
                            //    } 


                            // loop over fields by benchmarkdate
                            for (var n = 0; n < scope.observationSummaryManager.FieldsWithDates.length; n++) {
                                var fieldDate = scope.observationSummaryManager.FieldsWithDates[n];
                                for (var j = 0; j < scope.observationSummaryManager.Scores.StudentResults.length; j++) {
                                    // get the csr for the right student and date!!
                                    var csr = scope.observationSummaryManager.Scores.StudentResults[j];

                                    // get a reference to the studentresultbydate for this student
                                    var csrByDate = null;
                                    for (var r = 0; r < scope.observationSummaryManager.StudentResultsByDates.length; r++) {
                                        if (csr.StudentId == scope.observationSummaryManager.StudentResultsByDates[r].StudentId && csr.TestDueDateId == fieldDate.benchmarkDate.id) {
                                            csrByDate = scope.observationSummaryManager.StudentResultsByDates[r];

                                            for (var k = 0; k < csr.OSFieldResults.length; k++) {
                                                if (scope.observationSummaryManager.FieldsWithDates[n].field.DatabaseColumn == csr.OSFieldResults[k].DbColumn && 
                                                    csr.OSFieldResults[k].AssessmentId == scope.observationSummaryManager.FieldsWithDates[n].field.AssessmentId) {
                                                    var newOSFieldResult = angular.copy(csr.OSFieldResults[k]);
                                                    newOSFieldResult.TestLevelPeriodId = fieldDate.benchmarkDate.testLevelPeriodId;
                                                    newOSFieldResult.Field = angular.copy(fieldDate.field);
                                                    csrByDate.OSFieldResults.push(newOSFieldResult);
                                                    break; 
                                                }
                                            }
                                            break;
                                        }
                                    }


                                }

                                //scope.observationSummaryManager.StudentResultsByDates.push(newStudentResult);
                            }

                            //}

                            //for (var j = 0; j < scope.observationSummaryManager.Scores.StudentResults.length; j++) {
                            //    for (var k = 0; k < scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults.length; k++) {
                            //        for (var i = 0; i < scope.observationSummaryManager.Scores.Fields.length; i++) {
                            //            if (scope.observationSummaryManager.Scores.Fields[i].DatabaseColumn == scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].DbColumn) {
                            //                scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].Field = angular.copy(scope.observationSummaryManager.Scores.Fields[i]);
                            //            }
                            //        }
                            //    }
                            //}
                        }

                        if (scope.selectedSectionId != null && scope.selectedSectionId != -1 && scope.filterOptions.selectedBenchmarkDates.length > 0) {
                            //scope.initiallyLoaded = true;
                            scope.observationSummaryManager.LoadData(scope.selectedSectionId, scope.filterOptions.selectedBenchmarkDates).then(function (response) {
                                attachFieldsCallback();
                            });
                        }

                        // delegate sorting to the sort manager
                        scope.sort = function (column) {
                            scope.sortMgr.sort(column);
                        };

                        // sort function for multiple page printing
                        scope.sortMultiPage = function (column, page) {
                            if (page == 1) {
                                scope.sortMgr.sort(column);
                            } else {
                                var trueIndex = 0;
                                for (var i = 0; i < scope.printPages.length; i++) {
                                    if (scope.printPages[i].page == page) {
                                        trueIndex += column;
                                        break; // quit when you get to the right page
                                    } else {
                                        trueIndex += scope.printPages[i].currentHeaderCount;
                                    }
                                }
                                scope.sortMgr.sort(trueIndex);
                            }
                        };


                        function getIntColor(gradeId, studentFieldScore, fieldValue, testLevelPeriodId) {
                            var benchmarkArray = null;
                            for (var i = 0; i < scope.observationSummaryManager.BenchmarksByGrade.length; i++) {

                                // if this is a state test, use the statetestgrade instead of the one for the overall result
                                if (studentFieldScore.TestTypeId == 3) {
                                    gradeId = studentFieldScore.StateGradeId;
                                }

                                if (scope.observationSummaryManager.BenchmarksByGrade[i].GradeId == gradeId) {
                                    benchmarkArray = scope.observationSummaryManager.BenchmarksByGrade[i];
                                }

                                if (benchmarkArray != null) {
                                    for (var j = 0; j < benchmarkArray.Benchmarks.length; j++) {
                                        if (benchmarkArray.Benchmarks[j].DbColumn === studentFieldScore.DbColumn && benchmarkArray.Benchmarks[j].AssessmentId === studentFieldScore.AssessmentId && benchmarkArray.Benchmarks[j].TestLevelPeriodId == testLevelPeriodId) {
                                            if (fieldValue != null) {
                                                // not defined yet
                                                //if (studentFieldScore.DecimalValue === $scope.Benchmarks[i].MaxScore) {
                                                //	return 'obsPerfect';
                                                //}
                                                if (fieldValue >= benchmarkArray.Benchmarks[j].Exceeds && benchmarkArray.Benchmarks[j].Exceeds != null) {
                                                    return 'obsBlue';
                                                }
                                                if (fieldValue >= benchmarkArray.Benchmarks[j].Meets && benchmarkArray.Benchmarks[j].Meets != null) {
                                                    return 'obsGreen';
                                                }
                                                if (fieldValue >= benchmarkArray.Benchmarks[j].Approaches && benchmarkArray.Benchmarks[j].Approaches != null) {
                                                    return 'obsYellow';
                                                }
                                                if (fieldValue < benchmarkArray.Benchmarks[j].Approaches && benchmarkArray.Benchmarks[j].Approaches != null) {
                                                    return 'obsRed';
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            return '';
                        }

                        scope.getBackgroundClass = function (gradeId, studentFieldScore, tddId) {
                            switch (studentFieldScore.ColumnType) {
                                case 'Textfield':
                                    return '';
                                    break;
                                case 'DecimalRange':
                                    return getIntColor(gradeId, studentFieldScore, studentFieldScore.DecimalValue, tddId);
                                    break;
                                case 'DropdownRange':
                                    return getIntColor(gradeId, studentFieldScore, studentFieldScore.IntValue, tddId);
                                    break;
                                case 'DropdownFromDB':
                                    return getIntColor(gradeId, studentFieldScore, studentFieldScore.IntValue, tddId);
                                    break;
                                case 'CalculatedFieldClientOnly':
                                    return '';
                                    break;
                                case 'CalculatedFieldDbBacked':
                                    return getIntColor(gradeId, studentFieldScore, studentFieldScore.IntValue, tddId);
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
    ]).directive('nsObservationSummarySectionMultipleColumns', [
			'$routeParams', '$compile', '$templateCache', '$http', 'nsFilterOptionsService', '$filter', 'NSObservationSummarySectionMultipleManager', 'NSSortManager', 'observationSummaryAssessmentFieldChooserSvc', '$location',
            function ($routeParams, $compile, $templateCache, $http, nsFilterOptionsService, $filter, NSObservationSummarySectionMultipleManager, NSSortManager, observationSummaryAssessmentFieldChooserSvc, $location) {

                return {
                    restrict: 'E',
                    template: '<div ng-include="templateUrl"></div>',
                    //templateUrl: 'templates/observation-summary-section-print.html',
                    scope: {
                        selectedSectionId: '=',
                        selectedStaffId: '=',
                        selectedBenchmarkDateId: '=',
                        selectedAssessmentIds: '=',
                        showCheckboxes: '=',
                        teamMeetingStudents: '=',
                        teamMeetingSections: '=',
                        sortArray: '='
                    },
                    link: function (scope, element, attr) {
                        scope.observationSummaryManager = new NSObservationSummarySectionMultipleManager(true);
                        scope.filterOptions = nsFilterOptionsService.options;
                        scope.manualSortHeaders = {};
                        scope.manualSortHeaders.studentNameHeaderClass = "fa";
                        scope.initiallyLoaded = false;

                        scope.headerClassArray = [];
                        scope.allSelected = false;
                        scope.sortMgr = new NSSortManager();
                        scope.fieldChooser = observationSummaryAssessmentFieldChooserSvc;

                        scope.hideAssessment = function (assessment) {
                            observationSummaryAssessmentFieldChooserSvc.hideAssessment(assessment).then(function (response) {
                                scope.observationSummaryManager.LoadData(scope.selectedSectionId, scope.filterOptions.selectedBenchmarkDates).then(function (response) {
                                    attachFieldsCallback();
                                });
                            });
                        }

                        // if benchmark dates change, update page
                        scope.$on('NSTddMultiRangeOptionsUpdated', function (event, data) {
                            // don't run this again after the inital load
                            //if (scope.initiallyLoaded) {
                            //    scope.initiallyLoaded = false;
                            //    return;
                            //}
                            scope.observationSummaryManager.LoadData(scope.selectedSectionId, scope.filterOptions.selectedBenchmarkDates).then(function (response) {
                                attachFieldsCallback();
                            });
                        });

                        scope.$on('NSFieldsUpdated', function (event, data) {
                            scope.observationSummaryManager.LoadData(scope.selectedSectionId, scope.filterOptions.selectedBenchmarkDates).then(function (response) {
                                attachFieldsCallback();
                            });
                        });

                        scope.$on('NSStudentAttributesUpdated', function (event, data) {
                            scope.observationSummaryManager.LoadData(scope.selectedSectionId, scope.filterOptions.selectedBenchmarkDates).then(function (response) {
                                attachFieldsCallback();
                            });
                        });

                        scope.$watch('selectedSectionId', function (newValue, oldValue) {
                            if (newValue != oldValue && newValue != null) {

                                scope.observationSummaryManager.LoadData(scope.selectedSectionId, scope.filterOptions.selectedBenchmarkDates).then(function (response) {
                                    attachFieldsCallback();
                                });
                            }

                        });

                        scope.divider = function (index, parentIndex, maxRows, isAssessmentBorder) {
                            var maxCount = scope.observationSummaryManager.FieldsWithDates.length;
                            if (parentIndex) {
                                // if this is the last row
                                if (parentIndex == maxRows - 1) {
                                    if (index == maxCount - 1) {
                                        // special case for 1 selected date
                                        if (scope.filterOptions.selectedBenchmarkDates.length == 1) {
                                            return { 'border-right': '3px solid black', 'border-left': '3px solid black', 'border-bottom': '3px solid black' };
                                        } else {
                                            return { 'border-right': '3px solid black', 'border-bottom': '3px solid black' };
                                        }
                                    } else if (index % scope.filterOptions.selectedBenchmarkDates.length == 0 || index == 0) {
                                        return { 'border-left': '3px solid black', 'border-bottom': '3px solid black' };
                                    } else {
                                        return { 'border-bottom': '3px solid black' };
                                    }
                                } else {
                                    if (index == maxCount - 1) {
                                        // special case for 1 selected date
                                        if (scope.filterOptions.selectedBenchmarkDates.length == 1) {
                                            return { 'border-right': '3px solid black', 'border-left': '3px solid black' };
                                        } else {
                                            return { 'border-right': '3px solid black' };
                                        }
                                    } else if (index % scope.filterOptions.selectedBenchmarkDates.length == 0 || index == 0) {
                                        return { 'border-left': '3px solid black' };
                                    }
                                }
                            }
                            else {
                                if (index == maxCount - 1) {
                                    // special case for 1 selected assessment
                                    if ((scope.observationSummaryManager.Scores.HeaderGroups.length == 1 && isAssessmentBorder)) {
                                        return { 'border-right': '3px solid black', 'border-left': '3px solid black' };
                                    } else if(isAssessmentBorder) {
                                        return { 'border-right': '3px solid black' };
                                    }
                                } else if (isAssessmentBorder || index == 0) {
                                    return { 'border-left': '3px solid black' };
                                }
                            }
                        }

                        scope.assessmentDivider = function (index, isAssessmentBorder) {
                            var maxCount = scope.observationSummaryManager.Scores.Fields.length;
                                if (index == maxCount - 1) {
                                    // special case for 1 selected assessment
                                    if (scope.observationSummaryManager.Scores.HeaderGroups.length == 1) {
                                        return { 'border-right': '3px solid black', 'border-left': '3px solid black' };
                                    } else  {
                                        return { 'border-right': '3px solid black' };
                                    }
                                } else if (isAssessmentBorder) {
                                    return { 'border-right': '3px solid black' };
                                } if (index == 0) {
                                    return { 'border-left': '3px solid black' };
                                }
                        }


                        scope.dateDivider = function (index, isAssessmentBorder, isLastCellOfAssessment) {
                            var maxCount = scope.observationSummaryManager.FieldsWithDates.length;
                            if (index == maxCount - 1) {
                                // special case for 1 selected assessment
                                if (scope.observationSummaryManager.Scores.HeaderGroups.length == 1 && scope.filterOptions.selectedBenchmarkDates.length == 1) {
                                    return { 'border-right': '3px solid black', 'border-left': '3px solid black' };
                                } else {
                                    return { 'border-right': '3px solid black' };
                                }
                            } else if (isAssessmentBorder && isLastCellOfAssessment) {
                                return { 'border-right': '3px solid black' };
                            } else if (isLastCellOfAssessment) {
                                return { 'border-right': '1px solid black' };
                            } else if (index == 0) {
                                return { 'border-left': '3px solid black' };
                            }
                        }

                        scope.bodyDivider = function (index, parentIndex, isLastDateForField, isLastCellOfAssessment) {
                            var maxRows = scope.observationSummaryManager.StudentResultsByDates.length;
                            var numberOfDateFieldColumns = scope.observationSummaryManager.FieldsWithDates.length;
                            if (parentIndex == maxRows - 1) {
                                // last column case
                                if (index == numberOfDateFieldColumns - 1) {
                                    // special case for 1 selected date and 1 field
                                    if (scope.observationSummaryManager.FieldsWithDates.length == 1) {
                                        return { 'border-right': '3px solid black', 'border-left': '3px solid black', 'border-bottom': '3px solid black' };
                                    } else  {
                                        return { 'border-right': '3px solid black', 'border-bottom': '3px solid black' };
                                    }
                                } else if (index == 0) {
                                    return { 'border-left': '3px solid black', 'border-bottom': '3px solid black' };
                                } else if(isLastCellOfAssessment) {
                                    return { 'border-bottom': '3px solid black', 'border-right' : '3px solid black' };
                                }
                                else if (isLastDateForField) {
                                    return { 'border-right': '1px solid black', 'border-bottom' : '3px solid black' };
                                } else {
                                    return { 'border-bottom': '3px solid black' };
                                }
                            } else if (index == numberOfDateFieldColumns - 1) {
                                // special case for 1 selected assessment
                                if (scope.observationSummaryManager.Scores.HeaderGroups.length == 1 && scope.filterOptions.selectedBenchmarkDates.length == 1) {
                                    return { 'border-right': '3px solid black', 'border-left': '3px solid black' };
                                } else {
                                    return { 'border-right': '3px solid black' };
                                }
                            } else if (isLastCellOfAssessment) {
                                return { 'border-right': '3px solid black' };
                            } else if (index == 0) {
                                return { 'border-left': '3px solid black' };
                            } else if (isLastDateForField) {
                                return { 'border-right': '1px solid black' };
                            }
                        }


                        scope.printPages = [];
                        // printing setup
                        if ($location.absUrl().indexOf('printmode=') >= 0) {
                            scope.templateUrl = 'templates/observation-summary-section-multiple-columns.html';
                        } else {
                            scope.templateUrl = 'templates/observation-summary-section-multiple-columns.html';
                        }

                        var attachFieldsCallback = function () {
                            // initialize the sort manager now that the data has been loaded
                            //scope.sortMgr.initialize(scope.manualSortHeaders, scope.sortArray, scope.headerClassArray, 'OSFieldResults', scope.observationSummaryManager.Scores);

                            if ($location.absUrl().indexOf('printmode=') >= 0) {

                                // add curent page to array
                                var currentPageNo = 1;
                                var currentHeaderCount = 0;
                                //scope.printPages.push(currentPage);

                                // set page for headers
                                var currentPage = { page: currentPageNo, assessments: [], currentHeaderCount: 0 };
                                scope.printPages.push(currentPage);
                                for (var p = 0; p < scope.observationSummaryManager.Scores.HeaderGroups.length; p++) {
                                    currentHeaderCount += scope.observationSummaryManager.Scores.HeaderGroups[p].FieldCount;


                                    // if less than 15, add to current page
                                    if (currentHeaderCount <= 15) {
                                        scope.observationSummaryManager.Scores.HeaderGroups[p].page = currentPageNo;
                                        currentPage.assessments.push(scope.observationSummaryManager.Scores.HeaderGroups[p].AssessmentId);
                                        currentPage.currentHeaderCount = currentHeaderCount;
                                    } else {
                                        // the header groups create the pages in the page array
                                        currentPageNo++;
                                        currentPage = { page: currentPageNo, assessments: [], currentHeaderCount: scope.observationSummaryManager.Scores.HeaderGroups[p].FieldCount };
                                        currentPage.assessments.push(scope.observationSummaryManager.Scores.HeaderGroups[p].AssessmentId);
                                        scope.printPages.push(currentPage);
                                        scope.observationSummaryManager.Scores.HeaderGroups[p].page = currentPageNo;
                                        currentHeaderCount = scope.observationSummaryManager.Scores.HeaderGroups[p].FieldCount;
                                    }
                                }

                                // set page for fields
                                for (var p = 0; p < scope.observationSummaryManager.Scores.Fields.length; p++) {
                                    for (var x = 0; x < scope.printPages.length; x++) {
                                        for (var y = 0; y < scope.printPages[x].assessments.length; y++) {
                                            if (scope.printPages[x].assessments[y] == scope.observationSummaryManager.Scores.Fields[p].AssessmentId) {
                                                scope.observationSummaryManager.Scores.Fields[p].page = scope.printPages[x].page;
                                            }
                                        }
                                    }
                                }

                                // set page for OSFieldREsults
                                for (var j = 0; j < scope.observationSummaryManager.Scores.StudentResults.length; j++) {
                                    for (var k = 0; k < scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults.length; k++) {
                                        for (var x = 0; x < scope.printPages.length; x++) {
                                            for (var y = 0; y < scope.printPages[x].assessments.length; y++) {
                                                if (scope.printPages[x].assessments[y] == scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].AssessmentId) {
                                                    scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].page = scope.printPages[x].page;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            // set up dateFields
                            scope.observationSummaryManager.FieldsWithDates = [];
                            scope.observationSummaryManager.StudentResultsByDates = [];

                            var currentAssessment = 0;
                            // this isassessmentborder is used for the headers
                            for (var i = 0; i < scope.observationSummaryManager.Scores.Fields.length; i++) {
                                if (currentAssessment != scope.observationSummaryManager.Scores.Fields[i].AssessmentId) {
                                    if (currentAssessment != 0) {
                                        scope.observationSummaryManager.Scores.Fields[i - 1].IsAssessmentBorder = true;
                                    }
                                    currentAssessment = scope.observationSummaryManager.Scores.Fields[i].AssessmentId;
                                    scope.observationSummaryManager.Scores.Fields[i].IsAssessmentBorder = false;
                                } else {
                                    scope.observationSummaryManager.Scores.Fields[i].IsAssessmentBorder = false;
                                }
                                for (var p = 0; p < scope.filterOptions.selectedBenchmarkDates.length; p++) {
                                    // add this benchmark date and a reference to the first field for this assessment
                                    var latestDate = { benchmarkDate: scope.filterOptions.selectedBenchmarkDates[p], field: scope.observationSummaryManager.Scores.Fields[i] };
                                    if (p == scope.filterOptions.selectedBenchmarkDates.length - 1) {
                                        latestDate.IsBorderCell = true;
                                    } else {
                                        latestDate.IsBorderCell = false;
                                    } // this isbordercell is used by the date headers to mark the end of a FIELD
                                    scope.observationSummaryManager.FieldsWithDates.push(latestDate);
                                }
                            }

                            var currentStudentId = -1;
                            var newStudentResult = {};

                            // add a record for each student
                            for (var j = 0; j < scope.observationSummaryManager.Scores.StudentResults.length; j++) {
                                var csr = scope.observationSummaryManager.Scores.StudentResults[j];

                                // only add each student once
                                if (currentStudentId != csr.StudentId) {
                                    currentStudentId = csr.StudentId;
                                    newStudentResult = {
                                        StudentName: csr.StudentName,
                                        StudentId: csr.StudentId,
                                        GradeId: csr.GradeId,
                                        Att1: csr.Att1,
                                        Att2: csr.Att2,
                                        Att3: csr.Att3,
                                        Att4: csr.Att4,
                                        Att5: csr.Att5,
                                        Att6: csr.Att6,
                                        Att7: csr.Att7,
                                        Att8: csr.Att8,
                                        Att9: csr.Att9,
                                        OSFieldResults: []
                                    };
                                    scope.observationSummaryManager.StudentResultsByDates.push(newStudentResult);
                                }
                            }

                            // create new structure for student results by date
                            //for (var j = 0; j < scope.observationSummaryManager.Scores.StudentResults.length; j++) {
                            //    var csr = scope.observationSummaryManager.Scores.StudentResults[j];

                            //    // only add each student once
                            //    if (currentStudentId != csr.StudentId) {
                            //        newStudentResult = { StudentName: csr.StudentName, StudentId: csr.StudentId, GradeId: csr.GradeId, OSFieldResults: [] };
                            //    } 


                            // loop over fields by benchmarkdate
                            for (var n = 0; n < scope.observationSummaryManager.FieldsWithDates.length; n++) {
                                var fieldDate = scope.observationSummaryManager.FieldsWithDates[n];
                                currentAssessment = fieldDate.field.AssessmentId
                                for (var j = 0; j < scope.observationSummaryManager.Scores.StudentResults.length; j++) {
                                    // get the csr for the right student and date!!
                                    var csr = scope.observationSummaryManager.Scores.StudentResults[j];
                                    
                                    // get a reference to the studentresultbydate for this student
                                    var csrByDate = null;
                                    for (var r = 0; r < scope.observationSummaryManager.StudentResultsByDates.length; r++) {
                                        // TODO: need to flag when the assessmentID changes!!!!!!!!!!!!!!!
                                        if (csr.StudentId == scope.observationSummaryManager.StudentResultsByDates[r].StudentId && csr.TestDueDateId == fieldDate.benchmarkDate.id) {
                                            csrByDate = scope.observationSummaryManager.StudentResultsByDates[r];

                                            for (var k = 0; k < csr.OSFieldResults.length; k++) {
 
                                                if (scope.observationSummaryManager.FieldsWithDates[n].field.DatabaseColumn == csr.OSFieldResults[k].DbColumn &&
                                                    csr.OSFieldResults[k].AssessmentId == fieldDate.field.AssessmentId) {


                                                    var newOSFieldResult = angular.copy(csr.OSFieldResults[k]);
                                                    newOSFieldResult.TestLevelPeriodId = fieldDate.benchmarkDate.testLevelPeriodId;
                                                    newOSFieldResult.Field = angular.copy(fieldDate.field);
                                                    csrByDate.OSFieldResults.push(newOSFieldResult);


                                                    if (scope.observationSummaryManager.FieldsWithDates[n].IsBorderCell) {
                                                        newOSFieldResult.IsLastDateForField = true;
                                                    } else {
                                                        newOSFieldResult.IsLastDateForField = false;
                                                    }
                                                    break;
                                                }
                                            }
                                            break;
                                        }
                                    }


                                }

                                //scope.observationSummaryManager.StudentResultsByDates.push(newStudentResult);
                            }

                            // set locations for borders
                            currentAssessment = 0;

                            var csrByDate = null;
                            for (var r = 0; r < scope.observationSummaryManager.StudentResultsByDates.length; r++) {
                                csrByDate = scope.observationSummaryManager.StudentResultsByDates[r];
                                for (var k = 0; k < csrByDate.OSFieldResults.length; k++) {

                                    if (currentAssessment != csrByDate.OSFieldResults[k].AssessmentId) {
                                        if (currentAssessment != 0 && k > 0) {
                                            csrByDate.OSFieldResults[k - 1].IsAssessmentBorder = true;
                                            currentAssessment = csrByDate.OSFieldResults[k].AssessmentId;
                                            continue;
                                        }                                        
                                    } 
                                    csrByDate.OSFieldResults[k].IsAssessmentBorder = false;
                                    currentAssessment = csrByDate.OSFieldResults[k].AssessmentId;
                                }
                            }

                            //}

                            //for (var j = 0; j < scope.observationSummaryManager.Scores.StudentResults.length; j++) {
                            //    for (var k = 0; k < scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults.length; k++) {
                            //        for (var i = 0; i < scope.observationSummaryManager.Scores.Fields.length; i++) {
                            //            if (scope.observationSummaryManager.Scores.Fields[i].DatabaseColumn == scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].DbColumn) {
                            //                scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].Field = angular.copy(scope.observationSummaryManager.Scores.Fields[i]);
                            //            }
                            //        }
                            //    }
                            //}
                        }

                        if (scope.selectedSectionId != null && scope.selectedSectionId != -1 && scope.filterOptions.selectedBenchmarkDates.length > 0) {
                            //scope.initiallyLoaded = true;
                            scope.observationSummaryManager.LoadData(scope.selectedSectionId, scope.filterOptions.selectedBenchmarkDates).then(function (response) {
                                attachFieldsCallback();
                            });
                        }


                        function getIntColor(gradeId, studentFieldScore, fieldValue, testLevelPeriodId) {
                            var benchmarkArray = null;
                            for (var i = 0; i < scope.observationSummaryManager.BenchmarksByGrade.length; i++) {

                                // if this is a state test, use the statetestgrade instead of the one for the overall result
                                if (studentFieldScore.TestTypeId == 3) {
                                    gradeId = studentFieldScore.StateGradeId;
                                }

                                if (scope.observationSummaryManager.BenchmarksByGrade[i].GradeId == gradeId) {
                                    benchmarkArray = scope.observationSummaryManager.BenchmarksByGrade[i];
                                }

                                if (benchmarkArray != null) {
                                    for (var j = 0; j < benchmarkArray.Benchmarks.length; j++) {
                                        if (benchmarkArray.Benchmarks[j].DbColumn === studentFieldScore.DbColumn && benchmarkArray.Benchmarks[j].AssessmentId === studentFieldScore.AssessmentId && benchmarkArray.Benchmarks[j].TestLevelPeriodId == testLevelPeriodId) {
                                            if (fieldValue != null) {
                                                // not defined yet
                                                //if (studentFieldScore.DecimalValue === $scope.Benchmarks[i].MaxScore) {
                                                //	return 'obsPerfect';
                                                //}
                                                if (fieldValue >= benchmarkArray.Benchmarks[j].Exceeds && benchmarkArray.Benchmarks[j].Exceeds != null) {
                                                    return 'obsBlue';
                                                }
                                                if (fieldValue >= benchmarkArray.Benchmarks[j].Meets && benchmarkArray.Benchmarks[j].Meets != null) {
                                                    return 'obsGreen';
                                                }
                                                if (fieldValue >= benchmarkArray.Benchmarks[j].Approaches && benchmarkArray.Benchmarks[j].Approaches != null) {
                                                    return 'obsYellow';
                                                }
                                                if (fieldValue < benchmarkArray.Benchmarks[j].Approaches && benchmarkArray.Benchmarks[j].Approaches != null) {
                                                    return 'obsRed';
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            return '';
                        }

                        scope.getBackgroundClass = function (gradeId, studentFieldScore, tddId) {
                            switch (studentFieldScore.ColumnType) {
                                case 'Textfield':
                                    return '';
                                    break;
                                case 'DecimalRange':
                                    return getIntColor(gradeId, studentFieldScore, studentFieldScore.DecimalValue, tddId);
                                    break;
                                case 'DropdownRange':
                                    return getIntColor(gradeId, studentFieldScore, studentFieldScore.IntValue, tddId);
                                    break;
                                case 'DropdownFromDB':
                                    return getIntColor(gradeId, studentFieldScore, studentFieldScore.IntValue, tddId);
                                    break;
                                case 'CalculatedFieldClientOnly':
                                    return '';
                                    break;
                                case 'CalculatedFieldDbBacked':
                                    return getIntColor(gradeId, studentFieldScore, studentFieldScore.IntValue, tddId);
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

	ObservationSummaryClassController.$inject = ['$scope', 'Admin', '$http', 'nsFilterOptionsService', '$routeParams', '$location'];

	function ObservationSummaryClassController($scope, Admin, $http, nsFilterOptionsService, $routeParams, $location) {
	    $scope.selectedOptions = {};
		
		$scope.filterOptions = nsFilterOptionsService.options;
		
		$scope.sortArray = [];

		$scope.navigateToTdd = function (tddid) {
		    $scope.filterOptions.selectedBenchmarkDate = nsFilterOptionsService.getBenchmarkDateById(tddid);
		    nsFilterOptionsService.changeBenchmarkDate();
		}

		
	}

	ObservationSummaryClassMultipleController.$inject = ['$scope', 'Admin', '$http', 'nsFilterOptionsService', '$routeParams', '$location'];

	function ObservationSummaryClassMultipleController($scope, Admin, $http, nsFilterOptionsService, $routeParams, $location) {
	    $scope.selectedOptions = {};

	    $scope.filterOptions = nsFilterOptionsService.options;

	    $scope.sortArray = [];

	    //$scope.navigateToTdd = function (tddid) {
	    //    $scope.filterOptions.selectedBenchmarkDate = nsFilterOptionsService.getBenchmarkDateById(tddid);
	    //    nsFilterOptionsService.changeBenchmarkDate();
	    //}


	}

	ObservationSummaryClassMultipleColumnController.$inject = ['$scope', 'Admin', '$http', 'nsFilterOptionsService', '$routeParams', '$location'];

	function ObservationSummaryClassMultipleColumnController($scope, Admin, $http, nsFilterOptionsService, $routeParams, $location) {
	    $scope.selectedOptions = {};

	    $scope.filterOptions = nsFilterOptionsService.options;

	    $scope.sortArray = [];

	    //$scope.navigateToTdd = function (tddid) {
	    //    $scope.filterOptions.selectedBenchmarkDate = nsFilterOptionsService.getBenchmarkDateById(tddid);
	    //    nsFilterOptionsService.changeBenchmarkDate();
	    //}


	}

	ObservationSummaryFilteredController.$inject = ['$scope', 'Admin', '$http', 'nsFilterOptionsService', '$routeParams', '$location', 'nsStackedBarGraphOptionsFactory'];

	function ObservationSummaryFilteredController($scope, Admin, $http, nsFilterOptionsService, $routeParams, $location, nsStackedBarGraphOptionsFactory) {
	    $scope.selectedOptions = {};
	    $scope.sortArray = [];

	    $scope.groupsFactory = new nsStackedBarGraphOptionsFactory('Compare Group Across Benchmark Dates', false);
	    $scope.filterOptions = $scope.groupsFactory.options;
	}

})();