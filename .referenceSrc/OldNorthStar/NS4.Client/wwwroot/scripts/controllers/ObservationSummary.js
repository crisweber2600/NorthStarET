(function () {
	'use strict'

	angular
	.module('observationSummaryModule', [])
	.controller('ObservationSummaryClassController', ObservationSummaryClassController)
    .controller('ObservationSummaryGradeController', ObservationSummaryGradeController)
        .factory('NSObservationSummarySectionManager', [
        '$http', 'webApiBaseUrl', 'nsLookupFieldService', function ($http, webApiBaseUrl, nsLookupFieldService) {
            var NSObservationSummarySectionManager = function () {

                var self = this;
                self.initialize = function () {

                }

                self.LoadData = function (sectionId, tddId) {
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
                    });
                }
            };

            return (NSObservationSummarySectionManager);
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
    .directive('nsObservationSummarySection', [
			'$routeParams', '$compile', '$templateCache', '$http', 'nsFilterOptionsService', '$filter', 'NSObservationSummarySectionManager', 'NSSortManager', 'observationSummaryAssessmentFieldChooserSvc',
            function ($routeParams, $compile, $templateCache, $http, nsFilterOptionsService, $filter, NSObservationSummarySectionManager, NSSortManager, observationSummaryAssessmentFieldChooserSvc) {

                return {
                    restrict: 'E',
                    templateUrl: 'templates/observation-summary-section.html',
                    scope: {
                        selectedSectionId: '=',
                        selectedStaffId: '=',
                        selectedBenchmarkDateId: '=',
                        selectedAssessmentIds: '=',
                        showCheckboxes: '=',
                        teamMeetingStudents: '=',
                        teamMeetingSections: '='
                    },
                    link: function (scope, element, attr) {
                        scope.observationSummaryManager = new NSObservationSummarySectionManager();
                        scope.filterOptions = nsFilterOptionsService.options;
                        scope.manualSortHeaders = {};
                        scope.manualSortHeaders.firstNameHeaderClass = "fa";
                        scope.manualSortHeaders.lastNameHeaderClass = "fa";
                        scope.sortArray = [];
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
                        scope.observationSummaryManager.LoadData(scope.selectedSectionId, scope.selectedBenchmarkDateId).then(function (response) {
                            attachFieldsCallback();
                        });

                        // delegate sorting to the sort manager
                        scope.sort = function (column) {
                            scope.sortMgr.sort(column);
                        };

                        //function getDecimalColor(gradeId, studentFieldScore) {
                        //    var benchmarkArray = null;
                        //    for (var i = 0; i < scope.observationSummaryManager.BenchmarksByGrade.length; i++) {

                        //        if (scope.observationSummaryManager.BenchmarksByGrade[i].GradeId == gradeId) {
                        //            benchmarkArray = scope.observationSummaryManager.BenchmarksByGrade[i];
                        //        }

                        //        if (benchmarkArray != null) {
                        //            for (var j = 0; j < benchmarkArray.Benchmarks.length; j++) {
                        //                if (benchmarkArray.Benchmarks[j].DbColumn === studentFieldScore.DbColumn && benchmarkArray.Benchmarks[j].AssessmentId === studentFieldScore.AssessmentId) {
                        //                    if (studentFieldScore.DecimalValue != null) {
                        //                        // not defined yet
                        //                        //if (studentFieldScore.DecimalValue === $scope.Benchmarks[i].MaxScore) {
                        //                        //	return 'obsGreen';
                        //                        //}
                        //                        if (studentFieldScore.DecimalValue >= benchmarkArray.Benchmarks[j].Decimal80) {
                        //                            return 'obsBlue';
                        //                        }
                        //                        if (studentFieldScore.DecimalValue >= benchmarkArray.Benchmarks[j].DecimalMean) {
                        //                            return '';
                        //                        }
                        //                        if (studentFieldScore.DecimalValue >= benchmarkArray.Benchmarks[j].Decimal20) {
                        //                            return 'obsYellow';
                        //                        }
                        //                        if (studentFieldScore.DecimalValue <= benchmarkArray.Benchmarks[j].Decimal20) {
                        //                            return 'obsRed';
                        //                        }
                        //                    }
                        //                }
                        //            }
                        //        }
                        //    }
                        //    return '';
                        //}

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

	/* Movies List Controller  */
	ObservationSummaryClassController.$inject = ['$scope', 'Admin', '$http', 'nsFilterOptionsService', '$routeParams', '$location'];

	function ObservationSummaryClassController($scope, Admin, $http, nsFilterOptionsService, $routeParams, $location) {
	    $scope.selectedOptions = {};
		//$scope.lookupFieldsArray = [];
		//$scope.studentResults = [];
		//$scope.benchmarks = [];
		//$scope.sortArray = [];
		//$scope.headerClassArray = [];
		//$scope.firstNameHeaderClass = "fa";
		//$scope.lastNameHeaderClass = "fa";
		//$scope.options = {};
		$scope.filterOptions = nsFilterOptionsService.options;
		//$scope.filterOptions.selectedBenchmarkDate = (typeof $routeParams.benchmarkDateId !== 'undefined') ? nsFilterOptionsService.getBenchmarkDateById($routeParams.benchmarkDateId) : $scope.filterOptions.selectedBenchmarkDate;
		//$scope.selectedOptions.benchmarkDateId = $scope.filterOptions.selectedBenchmarkDate.id;
		//$scope.selectedOptions.selectedSectionId = $scope.filterOptions.selectedSection.id;

		//$scope.$watch('filterOptions.selectedSection', function () {
		//    $scope.selectedOptions.selectedSectionId = $scope.filterOptions.selectedSection.id;
		//});
		//$scope.$watch('filterOptions.selectedBenchmarkDate', function () {
		//    $scope.selectedOptions.selectedBenchmarkDateId = $scope.filterOptions.selectedBenchmarkDate.id;
		//});

		$scope.navigateToTdd = function (tddid) {
		    $scope.filterOptions.selectedBenchmarkDate = nsFilterOptionsService.getBenchmarkDateById(tddid);
		}

		
	}

	/* Movies Create Controller */
	ObservationSummaryGradeController.$inject = ['$scope', '$location', 'Admin'];

	function ObservationSummaryGradeController($scope, $location, Admin) {
		//$scope.staff = new Admin();
		//$scope.add = function () {
		//	$scope.staff.$save(
        //        // success
        //        function () {
        //        	$location.path('/');
        //        },
        //        // error
        //        function (error) {
        //        	_showValidationErrors($scope, error);
        //        }
        //    );
		//};

	}

})();