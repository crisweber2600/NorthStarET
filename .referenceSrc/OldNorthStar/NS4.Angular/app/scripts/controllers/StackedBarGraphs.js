(function () {
    'use strict';

    angular
        .module('stackedBarGraphModule', [])
        .controller('StackedBarGraphComparisonController', StackedBarGraphComparisonController)
        .controller('StackedBarGraphGroupsController', StackedBarGraphGroupsController)
        .controller('PLCInterventionPlanningController', PLCInterventionPlanningController)
        .directive('nsSummaryFieldHeaders', ['$compile', 'NSSummarySortManager', function ($compile, NSSummarySortManager) {
            function Controller($scope, $element) {
                var outputHtml = '';
                var sortText = '';
                    //$scope.manualSortHeaders = {};
                    //$scope.manualSortHeaders.firstNameHeaderClass = "fa";
                    //$scope.manualSortHeaders.lastNameHeaderClass = "fa";
                    //$scope.headerClassArray = [];

                    //for (var i = 0; i < $scope.tdds.length; i++) {
                    //    $scope.headerClassArray[i] = [];
                    //}
                    //$scope.sortMgr = new NSSummarySortManager();

                    $scope.sort = function (tddIndex, fieldIndex) {
                        $scope.sortMgr.sort(tddIndex, fieldIndex);
                    };

                    // display the headers once for each TDD
                    for (var i = 0; i < $scope.tdds.length; i++) {
                        for (var j = 0; j < $scope.fields.length; j++) {
                            if ($scope.sortable) {
                                sortText = '<div style="cursor: pointer;" ng-click="sort(' + i + ',' + j + ')">' + $scope.fields[j].DisplayLabel + ' <i class="{{headerClassArray[' + i + '][' + j + ']}}"></i></div>';
                            } else {
                                sortText = $scope.fields[j].DisplayLabel;
                            }


                            if (j == 0) {
                                outputHtml += '<th class="text-center" style="border-left:3px solid #e9ecf0;border-right:1px solid #e9ecf0;">' + sortText + '</th>';
                            }
                            else if (j == $scope.fields.length - 1) {
                                outputHtml += '<th class="text-center" style="border-right:3px solid #e9ecf0;border-left:1px solid #e9ecf0;">' + sortText + '</th>';
                            } else {
                                outputHtml += '<th class="text-center" style="border-left:1px solid #e9ecf0;border-right:1px solid #e9ecf0;">' + sortText + '</th>';
                            }
                        }
                    }

                    $element.html(outputHtml);
                    $compile($element.contents())($scope);

                    //$scope.sortMgr.initialize($scope.manualSortHeaders, $scope.sortArray, $scope.headerClassArray, $scope.fields);
            }



            return {
                restrict: 'AE',
                scope: {
                    tdds: '=',
                    fields: '=',
                    dataRows: '=',
                    sortable: '=',
                    sortArray: '=',
                    sortMgr: '=',
                    headerClassArray: '='
                },
                controller: Controller
            }
        }])
          .factory('NSSummarySortManager', [
        '$http','$location', function ($http, $location) {
            var NSSummarySortManager = function () {
                var self = this;
                self.manualSortHeaders = {};
                self.sortArray = [];
                self.headerClassArray = [];
                self.fieldResultName = '';
                self.fields = [];
                self.initializedOnce = false;

                self.initialize = function (manualSortHeaders, sortArray, headerClassArray, fields) {
                    self.manualSortHeaders = manualSortHeaders;
                    self.sortArray = sortArray;
                    self.headerClassArray = headerClassArray;
                    self.fields = fields;

                    // set sort settings if printing
                    if ($location.absUrl().indexOf('SortParam=') > 0 && self.initializedOnce == false && self.sortArray) {
                        self.initializedOnce = true;
                        var sortParamArray = $location.search().SortParam.split(',');

                        for (var i = 0; i < sortParamArray.length; i++) {
                            self.sortArray.push(sortParamArray[i]);
                        }
                    }
                };

                self.sort = function (tddIndex, fieldIndex) {
                    var column = '';
                    var columnIndex = -1;
                    // if this is not a first or lastname column
                    if (!isNaN(parseInt(fieldIndex))) {
                        switch (self.fields[fieldIndex].FieldType) {
                            case 'Textfield':
                                column = 'ResultsByTDD[' + tddIndex + '].FieldResults[' + fieldIndex + '].StringValue';
                                break;
                            case 'DecimalRange':
                                column = 'ResultsByTDD[' + tddIndex + '].FieldResults[' + fieldIndex + '].DecimalValue';
                                break;
                            case 'DropdownRange':
                                column = 'ResultsByTDD[' + tddIndex + '].FieldResults[' + fieldIndex + '].IntValue';
                                break;
                            case 'DropdownFromDB':
                                column = 'ResultsByTDD[' + tddIndex + '].FieldResults[' + fieldIndex + '].IntValue';
                                break;
                            case 'CalculatedFieldDbOnly':
                                column = 'ResultsByTDD[' + tddIndex + '].FieldResults[' + fieldIndex + '].StringValue';
                                break;
                            case 'CalculatedFieldDbBacked':
                                column = 'ResultsByTDD[' + tddIndex + '].FieldResults[' + fieldIndex + '].IntValue';
                                break;
                            case 'CalculatedFieldDbBackedString':
                                column = 'ResultsByTDD[' + tddIndex + '].FieldResults[' + fieldIndex + '].StringValue';
                                break;
                            case 'CalculatedFieldClientOnly':
                                column = 'ResultsByTDD[' + tddIndex + '].FieldResults[' + fieldIndex + '].StringValue'; //shouldnt even be used in sorting
                                break;
                            default:
                                column = 'ResultsByTDD[' + tddIndex + '].FieldResults[' + fieldIndex + '].IntValue';
                                break;
                        }
                    } else {
                        column = tddIndex;
                    }
                    var bFound = false;
                    for (var j = 0; j < self.sortArray.length; j++) {
                        // if it is already on the list, reverse the sort
                        if (self.sortArray[j].indexOf(column) >= 0) {
                            bFound = true;

                            // is it already negative? if so, remove it
                            if (self.sortArray[j].indexOf("-") === 0) {
                                if (tddIndex > -1) {
                                    self.headerClassArray[tddIndex][fieldIndex] = "fa";
                                }
                                else {
                                    self.manualSortHeaders[column] = "fa";
                                }

                                //else if (column === 'Student') {
                                //    self.manualSortHeaders.studentNameHeaderClass = "fa";
                                //} else if (column === 'LastName') {
                                //    self.manualSortHeaders.lastNameHeaderClass = "fa";
                                //}
                                self.sortArray.splice(j, 1);
                            } else {
                                if (tddIndex > -1) {
                                    self.headerClassArray[tddIndex][fieldIndex] = "fa fa-chevron-down";
                                } else {
                                    self.manualSortHeaders[column] = "fa fa-chevron-down";
                                }


                                //else if (column === 'Student') {
                                //    self.manualSortHeaders.studentNameHeaderClass = "fa fa-chevron-down";
                                //} else if (column === 'LastName') {
                                //    self.manualSortHeaders.lastNameHeaderClass = "fa fa-chevron-down";
                                //}
                                self.sortArray[j] = "-" + self.sortArray[j];
                            }
                            break;
                        }
                    }
                    if (!bFound) {
                        self.sortArray.push(column);

                        if (tddIndex > -1) {
                            self.headerClassArray[tddIndex][fieldIndex] = "fa fa-chevron-up";
                        } else {
                            self.manualSortHeaders[column] = "fa fa-chevron-up";
                        }

                        //else if (column === 'Student') {
                        //    self.manualSortHeaders.studentNameHeaderClass = "fa fa-chevron-up";
                        //} else if (column === 'LastName') {
                        //    self.manualSortHeaders.lastNameHeaderClass = "fa fa-chevron-up";
                        //}
                    }
                };
            };

            return NSSummarySortManager;
        }])

        .directive('nsSummaryField', ['$compile','$filter', function ($compile, $filter) {
            return {
                restrict: 'AE',
                //scope: {
                //    tdds: '=',
                //    fields: '=',
                //    studentResult: '='
                //},
                scope: true,
                link: function (scope, element, attr) {
                    var outputHtml = '';

                    var removeit = scope.$watch(function () {
                        if (attr.tdds && attr.fields && attr.studentResult) {
                            return 'loaded';
                        } else {
                            return '';
                        }
                    }, function (newVal) {
                        if (newVal == 'loaded') {
                            scope.tdds = scope.$eval(attr.tdds);
                            scope.fields = scope.$eval(attr.fields);
                            scope.studentResult = scope.$eval(attr.studentResult);

                            // display the headers once for each TDD
                            for (var i = 0; i < scope.tdds.length; i++) {
                                var currentTdd = scope.tdds[i];
                                for (var j = 0; j < scope.fields.length; j++) {
                                    var currentField = scope.fields[j];
                                    var foundTdd = false;

                                    for (var k = 0; k < scope.studentResult.ResultsByTDD.length; k++) {
                                        var currentResultByTDD = scope.studentResult.ResultsByTDD[k];
                                        // make sure we have the right test due date
                                        if (currentResultByTDD.PeriodId == currentTdd.TestLevelPeriodID) {


                                            for (var v = 0; v < currentResultByTDD.FieldResults.length; v++) {
                                                //var resultField = $filter('filter')(currentResultByTDD.FieldResults, { DbColumn: currentField.DatabaseColumn }, true);
                                                var resultField = currentResultByTDD.FieldResults[v];
                                                if (resultField.DbColumn === currentField.DatabaseColumn) {
                                                    foundTdd = true;
                                                    // now output the value for this specific field, directive ideally
                                                    if (j == 0) {
                                                        outputHtml += "<td class='text-center' style='border-left:3px solid #e9ecf0;border-right:1px solid #e9ecf0;'><ns-assessment-field result='studentResult.ResultsByTDD[" + k + "].FieldResults[" + v + "]' all-results='studentResult.ResultsByTDD[" + k + "].FieldResults' /></td>";
                                                    }
                                                    else if (j == scope.fields.length - 1) {
                                                        outputHtml += "<td class='text-center' style='border-right:3px solid #e9ecf0;border-left:1px solid #e9ecf0;'><ns-assessment-field result='studentResult.ResultsByTDD[" + k + "].FieldResults[" + v + "]' all-results='studentResult.ResultsByTDD[" + k + "].FieldResults' /></td>";
                                                    } else {
                                                        outputHtml += "<td class='text-center' style='border-left:1px solid #e9ecf0;border-right:1px solid #e9ecf0;'><ns-assessment-field result='studentResult.ResultsByTDD[" + k + "].FieldResults[" + v + "]' all-results='studentResult.ResultsByTDD[" + k + "].FieldResults' /></td>";
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    // if there's no data for this tdd
                                    if (!foundTdd) {
                                        if (j == 0) {
                                            outputHtml += "<td style='border-left:3px solid #e9ecf0;border-right:1px solid #e9ecf0;'></td>";
                                        }
                                        else if (j == scope.fields.length - 1) {
                                            outputHtml += "<td style='border-right:3px solid #e9ecf0;border-left:1px solid #e9ecf0;'></td>";
                                        } else {
                                            outputHtml += "<td style='border-left:1px solid #e9ecf0;border-right:1px solid #e9ecf0;'></td>";
                                        }
                                        //outputHtml += "<td></td>";
                                    }
                                }
                            }

                            var linkFn = $compile(outputHtml);
                            var content = linkFn(scope);
                            element.append(content);
                            removeit();
                        }
                    }, true);

                    //var linkFn = $compile(outputHtml);
                    //var content = linkFn(scope);
                    //element.append(content);
                        //element.append(outputHtml);
                        //$compile(element.contents())(scope);
                    

                    //init();
                }
            }
        }])
         .directive('nsStackedBarGraphLegend', [
		'$routeParams', '$compile', '$templateCache', '$http', '$filter', function ($routeParams, $compile, $templateCache, $http, $filter) {

		    return {
		        scope: {
		            options: '=',
		            zone: '=',
		            category: '=',
		            summaryMode: '=',
                    title: '='
		        },
		        restrict: 'E',
		        templateUrl: 'templates/stacked-bar-graph-legend.html',
		        link: function (scope, element, attr) {

		            scope.schoolYear = function () {
		                return scope.options.selectedSchoolYear.text;
		            }

		            scope.whichStack = function () {
		                return scope.category;
		            }

		            scope.scoreGrouping = function () {
		                switch (scope.zone) {
		                    case 1: return 'Exceeds Expectations';
		                    case 2: return 'Meets Expectations';
		                    case 3: return 'Approaches Expectations';
		                    case 4: return 'Does Not Meet Expectations';
		                }
		            }

		            scope.schools = function () {
		                var result = '';

		                if (scope.options.selectedSchoolGrouping != null && scope.options.selectedSchoolGrouping != '') {
		                    return scope.options.selectedSchoolGrouping;
		                }

		                if (scope.options.selectedSchools.length > 0) {
		                    for (var i = 0; i < scope.options.selectedSchools.length; i++) {
		                        result += scope.options.selectedSchools[i].text + ', ';
		                    }
		                    result = result.substring(0, result.length - 2);
		                } else {
		                    result = 'All Schools';
		                }

		                return result;
		            }
		            scope.grades = function () {
		                var result = '';

		                if (scope.options.selectedGrades.length > 0) {
		                    for (var i = 0; i < scope.options.selectedGrades.length; i++) {
		                        result += scope.options.selectedGrades[i].text + ', ';
		                    }
		                    result = result.substring(0, result.length - 2);
		                } else {
		                    result = 'All Grades';
		                }

		                return result;
		            }
		            scope.teachers = function () {
		                var result = '';

		                if (scope.options.selectedTeachers.length > 0) {
		                    for (var i = 0; i < scope.options.selectedTeachers.length; i++) {
		                        result += scope.options.selectedTeachers[i].text + ', ';
		                    }
		                    result = result.substring(0, result.length - 2);
		                } else {
		                    result = 'All Teachers';
		                }

		                return result;
		            }
		            scope.sections = function () {
		                var result = '';

		                if (scope.options.selectedSections.length > 0) {
		                    for (var i = 0; i < scope.options.selectedSections.length; i++) {
		                        result += scope.options.selectedSections[i].text + ', ';
		                    }
		                    result = result.substring(0, result.length - 2);
		                } else {
		                    result = 'All Sections';
		                }

		                return result;
		            }
		            scope.interventionTypes = function () {
		                var result = '';

		                if (scope.options.selectedInterventionTypes.length > 0) {
		                    for (var i = 0; i < scope.options.selectedInterventionTypes.length; i++) {
		                        result += scope.options.selectedInterventionTypes[i].text + ', ';
		                    }
		                    result = result.substring(0, result.length - 2);
		                } else {
		                    result = 'All Intervention Types';
		                }

		                return result;
		            }
		            scope.attributeValue = function (attributeName, selectedData) {
		                var result = '';

		                if (selectedData && selectedData.length > 0) {
		                    for (var i = 0; i < selectedData.length; i++) {
		                        result += selectedData[i].text + ', ';
		                    }
		                    result = result.substring(0, result.length - 2);
		                } else {
		                    result = 'All ' + attributeName;
		                }

		                return result;
		            }
		            scope.specialEd = function () {

		                if (scope.options.selectedSpedTypes == null) {
		                    return 'All Students';
		                } else {
		                    return scope.options.selectedSpedTypes.text;
		                }
		            }
		            scope.assessmentField = function () {

		                if (!angular.isDefined(scope.options.selectedAssessmentField.AssessmentName)) {
		                    return 'Field not yet chosen';
		                } else {
		                    return scope.options.selectedAssessmentField.AssessmentName + " / " + scope.options.selectedAssessmentField.DisplayLabel;
		                }
		            }
		        }
		    };
		}
         ])
        .directive('nsStackedBarGraphCustomGroupOptions', [
		'$routeParams', '$compile', '$templateCache', '$http', '$filter', 'nsSelect2RemoteOptions', function ($routeParams, $compile, $templateCache, $http, $filter, nsSelect2RemoteOptions) {

		    function Controller($scope) {
		        $scope.filterOptions = $scope.groupFactory.options;


		        $scope.changeSchools = function (newVals, oldVals) {
		            $scope.groupFactory.loadSchoolChange(newVals, oldVals);
		        };
		        $scope.loadSchools = function (grades, oldVals, displayLabel) {
		            $scope.groupFactory.loadSchoolsByGrade(grades, oldVals, displayLabel);
		        };
		        $scope.clearSchools = function (oldVals) {
		            $scope.groupFactory.clearSchools(oldVals);
		        };
		        $scope.changeSchoolYears = function (newVals, oldVals) {
		            $scope.groupFactory.loadSchoolYearChange(newVals, oldVals);
		        };
		        $scope.changeGrades = function (newVals, oldVals) {
		            $scope.groupFactory.loadGradeChange(newVals, oldVals);
		        };
		        $scope.changeTeachers = function (newVals, oldVals) {
		            $scope.groupFactory.loadTeacherChange(newVals, oldVals);
		        };
		        $scope.changeInterventionists = function (newVals, oldVals) {
		            $scope.groupFactory.loadInterventionistChange(newVals, oldVals);
		        };
		        $scope.changeInterventionGroups = function (newVals, oldVals) {
		            $scope.groupFactory.loadInterventionGroupChange(newVals, oldVals);
		        };
		        $scope.changeInterventionStudents = function (newVals, oldVals) {
		            $scope.groupFactory.loadInterventionStudentChange(newVals, oldVals);
		        };
		        $scope.changeSections = function (newVals, oldVals) {
		            $scope.groupFactory.loadSectionChange(newVals, oldVals);
		        };
                 
		        $scope.select2SchoolOptions = {
		            minimumInputLength: 0,
		            data: $scope.filterOptions.schools,
		            multiple: true,
		            width: 'resolve',
		        };

		        //$scope.HfwMultiStudentReportRemoteOptions = nsSelect2RemoteOptions.HfwMultiStudentReportRemoteOptions;
		        $scope.HfwMultiRangeRemoteOptions = nsSelect2RemoteOptions.HfwMultiRangeRemoteOptions;
		        $scope.QuickSearchTextLevelZones = nsSelect2RemoteOptions.QuickSearchTextLevelZones;
		        $scope.QuickSearchPageTypesToPrint = nsSelect2RemoteOptions.QuickSearchPageTypesToPrint;
                 
		        $scope.select2SchoolYearOptions = {
		            minimumInputLength: 0,
		            data: $scope.filterOptions.schoolYears,
		            multiple: false,
		            width: 'resolve',
		        };

		        $scope.select2BenchmarkDateOptions = {
		            minimumInputLength: 0,
		            data: $scope.filterOptions.testDueDates,
		            multiple: false,
		            width: 'resolve',
		        };

		        $scope.select2GradeOptions = {
		            minimumInputLength: 0,
		            data: $scope.filterOptions.grades,
		            multiple: true,
		            width: 'resolve',
		        };

		        $scope.select2TeacherOptions = {
		            minimumInputLength: 0,
		            data: $scope.filterOptions.teachers,
		            multiple: true,
		            width: 'resolve',
		        };

		        $scope.select2InterventionistOptions = {
		            minimumInputLength: 0,
		            data: $scope.filterOptions.interventionists,
		            multiple: true,
		            width: 'resolve',
		        };

		        $scope.select2InterventionGroupOptions = {
		            minimumInputLength: 0,
		            data: $scope.filterOptions.interventionGroups,
		            multiple: true,
		            width: 'resolve',
		        };

		        $scope.select2InterventionStudentOptions = {
		            minimumInputLength: 0,
		            data: $scope.filterOptions.interventionStudents,
		            multiple: true,
		            width: 'resolve',
		        };

		        $scope.select2SectionStudentOptions = {
		            minimumInputLength: 0,
		            data: $scope.filterOptions.students,
		            multiple: true,
		            width: 'resolve',
		        };

		        $scope.select2SectionOptions = {
		            minimumInputLength: 0,
		            data: $scope.filterOptions.sections,
		            multiple: true,
		            width: 'resolve',
		        };

		        $scope.select2MultiExportableAssessmentOptions = {
		            minimumInputLength: 0,
		            data: $scope.filterOptions.exportableAssessments,
		            multiple: true,
		            width: 'resolve',
		        };

		        $scope.select2MultiAssessmentOptions = {
		            minimumInputLength: 0,
		            data: $scope.filterOptions.benchmarkAssessments,
		            multiple: true,
		            width: 'resolve',
		        };


		        $scope.select2InterventionAssessmentOptions = {
		            minimumInputLength: 0,
		            data: $scope.filterOptions.interventionAssessments,
		            multiple: false,
		            width: 'resolve',
		        };

		        $scope.select2InterventionTypeOptions = {
		            minimumInputLength: 0,
		            data: $scope.filterOptions.interventionTypes,
		            multiple: true,
		            width: 'resolve',
		        };

		        $scope.select2SpedOptions = {
		            minimumInputLength: 0,
		            data: [{ id: 1, text: 'Special Ed (Special Education Student)' }, { id: 0, text: 'Non-Special Ed (General Population Student)' }],
		            multiple: false,
		            width: 'resolve',
		        };
		    }

		    return {
		        scope: {
		            groupFactory: '=',
		            tddEnabled: '=',
                    assessmentFieldEnabled: '=',
                    groupName: '=',
                    batchPrintFieldsEnabled: '=',
                    studentsEnabled: '=',
                    interventionMode: '=',
                    assessmentExportEnabled: '='
		        },
			    restrict: 'E',
			    templateUrl: 'templates/stacked-bar-graph-group-options.html',
                controller: Controller
			};
		}
        ])
        .factory('nsStackedBarGraphOptionsFactory', [
			'$http', '$routeParams', '$filter', 'nsLookupFieldService', 'webApiBaseUrl', 'spinnerService', 'progressLoader', 'observationSummaryAssessmentFieldChooserSvc',
            function ($http, $routeParams, $filter, nsLookupFieldService, webApiBaseUrl, spinnerService, progressLoader, observationSummaryAssessmentFieldChooserSvc) {
			    var nsStackedBarGraphOptionsFactory = function (name, tddEnabled, skipAssessmentFieldLoad, explicitReturnLoadPromise) {

			        var self = this;
			        self.tddEnabled = tddEnabled;
			        self.name = name;
			        self.LookupFieldsArray = nsLookupFieldService.LookupFieldsArray;

			        self.options = {};
			        self.groupResults = {};
			        self.Scores = [];
			        self.BenchmarksByGrade = [];
			        self.TestDueDates = [];
			        self.StudentRecords = [];

			        self.options.selectedSchoolGrouping = null;
			        self.options.benchmarkAssessments = observationSummaryAssessmentFieldChooserSvc.BenchmarkAssessments;
			        self.options.interventionAssessments = [];
			        self.options.selectedAssessmentField = {};
			        self.options.selectedInterventionAssessment = null;
			        self.options.attributeTypes = [];
			        self.options.teachers = [];
			        self.options.interventionists = [];
			        self.options.interventionGroups = [];
			        self.options.interventionStudents = [];
			        self.options.grades = [];
			        self.options.sections = [];
			        self.options.students = [];
			        self.options.schools = [];
			        self.options.schoolYears = [];
			        self.options.ethnicities = [];
			        self.options.genders = [];
			        self.options.interventionTypes = [];
			        self.options.titleOneTypes = [];
			        self.options.educationLabels = [];
			        self.options.testDueDates = [];
			        self.options.exportableAssessments = [];

			        self.options.selectedExportableAssessments = [];
			        self.options.selectedGrades = [];
			        self.options.selectedTeachers = [];
			        self.options.selectedInterventionists = [];
			        self.options.selectedInterventionGroups = [];
			        self.options.selectedInterventionStudents = [];
			        self.options.selectedSections = [];
			        self.options.selectedSchools = [];
			        self.options.selectedEthnicities = [];
			        self.options.selectedInterventionTypes = [];
			        self.options.selectedSpedTypes = null;
			        self.options.selectedTitleOneTypes = [];
			        self.options.selectedEducationLabels = [];
			        self.options.selectedSchoolYear = {}; // TODO: GetDefaultYear
			        self.options.selectedTestDueDate = {};
			        self.options.selectedADSIS = false;
			        self.options.selectedELL = false;
			        self.options.selectedGifted = false;
			        self.options.selectedGender = {};
			        self.PageSize = 20;
			        self.options.selectedAssessments = [];
			        self.options.selectedHfwPages = [];
			        self.options.selectedHfwStudentReportPages = [];
			        self.options.selectedTextLevelZones = [];
			        self.options.selectedPageTypes = [];

			        //this.options.selected

			        self.LoadAssessmentFields = function () {

			            var url = webApiBaseUrl + '/api/benchmark/GetAssessmentsAndFields';
			            var promise = $http.get(url);

			            return promise.then(function (response) {
			                self.options.assessments = self.flatten(response.data.Assessments);
			                self.options.exportableAssessments.push.apply(self.options.exportableAssessments, self.getExportableAssessments(response.data.Assessments));
			            });
			        }

			        self.LoadInterventionAssessments = function () {
			            var url = webApiBaseUrl + '/api/benchmark/GetInterventionAssessmentsAndFields';
			            var promise = $http.get(url);

			            return promise.then(function (response) {
			                self.interventionAssessments = self.formatAssessment(response.data.Assessments);
			            });
			        }

			        self.getExportableAssessments = function (data) {
			            var out = [];
			            angular.forEach(data, function (d) {
			                if(d.CanImport && d.TestType == 1){
			                    out.push({
			                        id: d.Id,
			                        text: d.AssessmentName,
			                    });
			                }
			            });
			            return out;
			        }

			        self.formatAssessment = function (data) {
			            var out = [];
			            angular.forEach(data, function (d) {
			                out.push({
			                    id: d.Id,
			                    text: d.AssessmentName,
			                })
			            });
			            return out;
			        }

			        self.flatten = function (data) {
			            var out = [];
			            angular.forEach(data, function (d) {
			                angular.forEach(d.Fields, function (v) {
			                    out.push({
			                        AssessmentName: d.AssessmentName,
			                        TestType: d.TestType,
			                        DisplayLabel: v.DisplayLabel,
			                        FieldName: v.DatabaseColumn,
			                        LookupFieldName: v.LookupFieldName,
			                        AssessmentId: d.Id,
			                        FieldType: v.FieldType,
			                        RangeHigh: v.RangeHigh,
			                        RangeLow: v.RangeLow,
			                        DisplayInLineGraphs: v.DisplayInLineGraphs,
			                        DatabaseColumn: v.DatabaseColumn
			                    })
			                })
			            })
			            return out;
			        }

			        self.loadOSData = function () { 
			            spinnerService.show('tableSpinner');
			            var selectedAttributes = [];

			            for (var i = 0; i < self.options.attributeTypes.length; i++) {
			                var currentType = self.options.attributeTypes[i];
			                selectedAttributes.push({ AttributeTypeId: currentType.AttributeTypeId, Name: currentType.Name, DropDownData: currentType.selectedData });
			            }

			            var returnObject = {
			                Schools: self.options.selectedSchools,
			                Grades: self.options.selectedGrades,
			                Teachers: self.options.selectedTeachers,
			                Sections: self.options.selectedSections,
			                InterventionTypes: self.options.selectedInterventionTypes,
			                SpecialEd: self.options.selectedSpedTypes,
			                SchoolStartYear: self.options.selectedSchoolYear.id,
			                DropdownDataList: selectedAttributes,
			                AssessmentField: self.options.selectedAssessmentField,
			                TestDueDateID: self.options.selectedTestDueDate.id,
			                GroupName: self.name
			            };

			            return $http.post(webApiBaseUrl + "/api/assessment/GetFilteredObservationSummary", returnObject).then(function (response) {
			                angular.extend(self, response.data);
			                self.LookupLists = nsLookupFieldService.LookupFieldsArray;
			                //if (response.data.Scores != null) {
			                //    self.AllStudentResults = response.data.Scores.StudentResults;
			                //    self.Scores.StudentResults = self.AllStudentResults.slice(0, self.PageSize);
			                //    self.Scores.Fields = response.data.Scores.Fields;
			                //    self.Scores.HeaderGroups = response.data.Scores.HeaderGroups;
			                //}
			                if (self.Scores === null) self.Scores = { StudentResults: []};

                            // benchmarks by grade
			                //self.BenchmarksByGrade = response.data.BenchmarksByGrade;
			                if (response.data.BenchmarksByGrade === null) self.BenchmarksByGrade = [];
			                // reset infinity scroll
			                self.timesScrolled = 1;
			                self.maxRecords = self.PageSize;

			                return;
			            }).finally(function (response) {
			                spinnerService.hide('tableSpinner');
			            });
			        }

			        self.loadOSExportData = function () {
			            var selectedAttributes = [];

			            for (var i = 0; i < self.options.attributeTypes.length; i++) {
			                var currentType = self.options.attributeTypes[i];
			                selectedAttributes.push({ AttributeTypeId: currentType.AttributeTypeId, Name: currentType.Name, DropDownData: currentType.selectedData });
			            }

			            var returnObject = {
			                Schools: self.options.selectedSchools,
			                Grades: self.options.selectedGrades,
			                Teachers: self.options.selectedTeachers,
			                Sections: self.options.selectedSections,
			                InterventionTypes: self.options.selectedInterventionTypes,
			                SpecialEd: self.options.selectedSpedTypes,
			                SchoolStartYear: self.options.selectedSchoolYear.id,
			                DropdownDataList: selectedAttributes,
			                AssessmentField: self.options.selectedAssessmentField,
			                TestDueDateID: self.options.selectedTestDueDate.id,
			                GroupName: self.name
			            };

                        // doesn't actually return the data, just creates a job
			            return $http.post(webApiBaseUrl + "/api/exportdata/CreateAssessmentDataExportJob", returnObject);
			        }

			        self.loadAllFieldsExportData = function () {
			            var selectedAttributes = [];

			            for (var i = 0; i < self.options.attributeTypes.length; i++) {
			                var currentType = self.options.attributeTypes[i];
			                selectedAttributes.push({ AttributeTypeId: currentType.AttributeTypeId, Name: currentType.Name, DropDownData: currentType.selectedData });
			            }

			            var returnObject = {
			                Schools: self.options.selectedSchools,
			                Grades: self.options.selectedGrades,
			                Teachers: self.options.selectedTeachers,
			                Sections: self.options.selectedSections,
			                InterventionTypes: self.options.selectedInterventionTypes,
			                SpecialEd: self.options.selectedSpedTypes,
			                SchoolStartYear: self.options.selectedSchoolYear.id,
			                DropdownDataList: selectedAttributes,
			                AssessmentField: self.options.selectedAssessmentField,
			                TestDueDateID: self.options.selectedTestDueDate.id,
			                GroupName: self.name,
                            Assessments: self.options.selectedExportableAssessments
			            };

			            // doesn't actually return the data, just creates a job
			            return $http.post(webApiBaseUrl + "/api/exportdata/CreateAssessmentAllFieldsDataExportJob", returnObject);
			        }

			        self.loadIGExportData = function (batchName) {
			            var selectedAttributes = [];

			            for (var i = 0; i < self.options.attributeTypes.length; i++) {
			                var currentType = self.options.attributeTypes[i];
			                selectedAttributes.push({ AttributeTypeId: currentType.AttributeTypeId, Name: currentType.Name, DropDownData: currentType.selectedData });
			            }

			            var returnObject = {
			                Schools: self.options.selectedSchools,
			                InterventionGroups: self.options.selectedInterventionGroups,
			                Interventionists: self.options.selectedInterventionists,
			                InterventionStudents: self.options.selectedInterventionStudents,
			                InterventionTypes: self.options.selectedInterventionTypes,
			                SpecialEd: self.options.selectedSpedTypes,
			                SchoolStartYear: self.options.selectedSchoolYear.id,
			                DropdownDataList: selectedAttributes,
			                AssessmentField: self.options.selectedAssessmentField,
			                TestDueDateID: self.options.selectedTestDueDate.id,
			                GroupName: self.name,
			                InterventionAssessment: self.options.selectedInterventionAssessment,
                            BatchName: batchName
			            };

			            // doesn't actually return the data, just creates a job
			            return $http.post(webApiBaseUrl + "/api/exportdata/CreateInterventionGroupAssessmentDataExportJob", returnObject);
			        }

			        self.createPrintBatch = function (batchName) {
			            var selectedAttributes = [];

			            for (var i = 0; i < self.options.attributeTypes.length; i++) {
			                var currentType = self.options.attributeTypes[i];
			                selectedAttributes.push({ AttributeTypeId: currentType.AttributeTypeId, Name: currentType.Name, DropDownData: currentType.selectedData });
			            }
                         
			            var returnObject = {
                            BatchName: batchName,
			                Schools: self.options.selectedSchools,
			                Grades: self.options.selectedGrades,
			                Teachers: self.options.selectedTeachers,
			                Sections: self.options.selectedSections,
			                InterventionTypes: self.options.selectedInterventionTypes,
			                SpecialEd: self.options.selectedSpedTypes,
			                SchoolStartYear: self.options.selectedSchoolYear.id,
			                DropdownDataList: selectedAttributes,
			                AssessmentField: self.options.selectedAssessmentField,
			                TestDueDateID: self.options.selectedTestDueDate.id,
			                GroupName: self.name,
			                Students: self.options.selectedStudents,
			                PageTypes: self.options.selectedPageTypes,
			                TargetLevelZones: self.options.selectedTextLevelZones,
			                HfwPages: self.options.selectedHfwPages,
                            Assessments : self.options.selectedAssessments,
			                HfwStudentReports: self.options.selectedHfwStudentReportPages
			            };

			            // doesn't actually return the data, just creates a job
			            return $http.post(webApiBaseUrl + "/api/exportdata/CreatePrintBatchJob", returnObject);
			        }

			        self.timesScrolled = 1;
			        self.maxRecords = self.PageSize;
                    // every time we scroll, add more records
			        self.loadOSInfinityRecords = function () {
			            if (self.Scores.StudentResults) {
			                var scrollStart = self.timesScrolled * self.PageSize;

                            // don't keep calling this forever
			                if (self.Scores.StudentResults.length > scrollStart) {
			                    progressLoader.start();
			                    progressLoader.set(50);
			                    //self.Scores.StudentResults.push.apply(self.Scores.StudentResults, self.AllStudentResults.slice(scrollStart, scrollStart + self.PageSize));
			                    self.timesScrolled++;
			                    self.maxRecords = scrollStart + self.PageSize;
			                    progressLoader.end();
			                }
			            }
			        }


			        self.loadGroupData = function (comparison) {
			            var selectedAttributes = [];

			            for (var i = 0; i < self.options.attributeTypes.length; i++) {
			                var currentType = self.options.attributeTypes[i];
			                selectedAttributes.push({ AttributeTypeId: currentType.AttributeTypeId, Name: currentType.Name, DropDownData: currentType.selectedData });
			            }


			            var returnObject = {
			                Schools: self.options.selectedSchools,
			                Grades: self.options.selectedGrades,
			                Teachers: self.options.selectedTeachers,
			                Sections: self.options.selectedSections,
			                //EducationLabels: self.options.selectedEducationLabels,
			                InterventionTypes: self.options.selectedInterventionTypes,
			                SpecialEd: self.options.selectedSpedTypes,
			                //TitleOneTypes: self.options.selectedTitleOneTypes,
			                //Ethnicities: self.options.selectedEthnicities,
			                SchoolStartYear: self.options.selectedSchoolYear.id,
			                DropdownDataList: selectedAttributes,
			                AssessmentField: self.options.selectedAssessmentField,
			                TestDueDateID: self.tddEnabled ? self.options.selectedTestDueDate.id : null, 
			                GroupName: self.name
			            };

			            if (comparison) {
			                return $http.post(webApiBaseUrl + "/api/stackedbargraph/GetStackedBarGraphComparisonData", returnObject);
			            } else {
			                return $http.post(webApiBaseUrl + "/api/stackedbargraph/GetStackedBarGraphGroupData", returnObject);
			            }
			                //    .then(function (response) {
			                //    self.groupResults.data = response.data;
			            //});
			        }

			        self.loadSummaryData = function (scoreGrouping, tdd, comparisionTdd, isHistorical) {
			            var selectedAttributes = [];

			            for (var i = 0; i < self.options.attributeTypes.length; i++) {
			                var currentType = self.options.attributeTypes[i];
			                selectedAttributes.push({ AttributeTypeId: currentType.AttributeTypeId, Name: currentType.Name, DropDownData: currentType.selectedData });
			            }

			            var returnObject = {
			                Schools: self.options.selectedSchools,
			                Grades: self.options.selectedGrades,
			                Teachers: self.options.selectedTeachers,
			                Sections: self.options.selectedSections,
			                InterventionTypes: self.options.selectedInterventionTypes,
                            SpecialEd: self.options.selectedSpedTypes,
			                SchoolStartYear: self.options.selectedSchoolYear.id,
			                ScoreGrouping: angular.isDefined(scoreGrouping) ? scoreGrouping : $routeParams.scoreGrouping,
			                TestDueDate: (angular.isDefined(comparisionTdd) && comparisionTdd != null) ? moment(comparisionTdd).format('MM/DD/YYYY') : tdd,
			                AssessmentField: self.options.selectedAssessmentField,
			                DropdownDataList: selectedAttributes
			            };

			            if (isHistorical) {
			                return $http.post(webApiBaseUrl + "/api/stackedbargraph/GetStackedBarGraphGroupHistoricalSummary", returnObject).then(function (response) {
			                    self.Att1Header = response.data.Att1Header;
			                    self.Att2Header = response.data.Att2Header;
			                    self.Att3Header = response.data.Att3Header;
			                    self.Att4Header = response.data.Att4Header;
			                    self.Att5Header = response.data.Att5Header;
			                    self.Att6Header = response.data.Att6Header;
			                    self.Att7Header = response.data.Att7Header;
			                    self.Att8Header = response.data.Att8Header;
			                    self.Att9Header = response.data.Att9Header;
			                    self.Att1Visible = response.data.Att1Visible;
			                    self.Att2Visible = response.data.Att2Visible;
			                    self.Att3Visible = response.data.Att3Visible;
			                    self.Att4Visible = response.data.Att4Visible;
			                    self.Att5Visible = response.data.Att5Visible;
			                    self.Att6Visible = response.data.Att6Visible;
			                    self.Att7Visible = response.data.Att7Visible;
			                    self.Att8Visible = response.data.Att8Visible;
			                    self.Att9Visible = response.data.Att9Visible;
			                    self.TestDueDates = response.data.TestDueDates;
			                    self.HistoricalSummaryRecords = response.data.SummaryRecords;
			                    self.Fields = response.data.Fields;
			                    self.HistoricalSummaryDataPostProcess();
			                    fixDates(self.TestDueDates);
			                });
			            } else {
			                return $http.post(webApiBaseUrl + "/api/stackedbargraph/GetStackedBarGraphGroupSummary", returnObject).then(function (response) {
			                    self.Att1Header = response.data.Att1Header;
			                    self.Att2Header = response.data.Att2Header;
			                    self.Att3Header = response.data.Att3Header;
			                    self.Att4Header = response.data.Att4Header;
			                    self.Att5Header = response.data.Att5Header;
			                    self.Att6Header = response.data.Att6Header;
			                    self.Att7Header = response.data.Att7Header;
			                    self.Att8Header = response.data.Att8Header;
			                    self.Att9Header = response.data.Att9Header;
			                    self.Att1Visible = response.data.Att1Visible;
			                    self.Att2Visible = response.data.Att2Visible;
			                    self.Att3Visible = response.data.Att3Visible;
			                    self.Att4Visible = response.data.Att4Visible;
			                    self.Att5Visible = response.data.Att5Visible;
			                    self.Att6Visible = response.data.Att6Visible;
			                    self.Att7Visible = response.data.Att7Visible;
			                    self.Att8Visible = response.data.Att8Visible;
			                    self.Att9Visible = response.data.Att9Visible;
			                    self.TestDueDates = response.data.TestDueDates;
			                    self.SummaryRecords = response.data.SummaryRecords;
			                    self.Fields = response.data.Fields;
			                    self.SummaryDataPostProcess();
			                });
			            }
			        }

			        function fixDates(aryDates) {
			            for (var i = 0; i < aryDates.length; i++) {
			                aryDates[i].DisplayDate = moment(aryDates[i].DisplayDate.substring(0, aryDates[i].DisplayDate.indexOf('T')));
			            }
			        }

			        self.SummaryDataPostProcess = function () {
			            for (var j = 0; j < self.SummaryRecords.length; j++) {
			                for (var i = 0; i < self.SummaryRecords[j].ResultsByTDD.length; i++) {
			                    for (var k = 0; k < self.SummaryRecords[j].ResultsByTDD[i].FieldResults.length; k++) {
			                        for (var r = 0; r < self.Fields.length; r++) {
			                            if (self.Fields[r].DatabaseColumn == self.SummaryRecords[j].ResultsByTDD[i].FieldResults[k].DbColumn) {
			                                self.SummaryRecords[j].ResultsByTDD[i].FieldResults[k].Field = angular.copy(self.Fields[r]);
			                            }
			                        }
			                    }
			                }
			            }
			        }

			        function uniqueClassCount(summaryRecords, studentId) {
			            var uniqueCounter = 0;
			            var currentSection = '';

			            var recordsForStudentId = $filter('filter')(summaryRecords, { StudentID: studentId });

			            for (var i = 0; i < recordsForStudentId.length; i++) {
			                if (currentSection !== recordsForStudentId[i].Section) {
			                    currentSection = recordsForStudentId[i].Section;
			                    uniqueCounter++;
			                }
			            }

			            return uniqueCounter;
			        }

			        function uniqueSchoolForClassCount(summaryRecords, studentId, schoolName) {

			            var recordsForStudentIdSchool = $filter('filter')(summaryRecords, { StudentID: studentId, SchoolName: schoolName });

			            return recordsForStudentIdSchool.length;
			        }

			        self.HistoricalSummaryDataPostProcess = function () {
			            var currentStudentId = -1;
			            var currentSchool = '';

			            for (var j = 0; j < self.HistoricalSummaryRecords.length; j++) {
			                if (currentStudentId != self.HistoricalSummaryRecords[j].StudentID) {
			                    currentStudentId = self.HistoricalSummaryRecords[j].StudentID;
			                    self.HistoricalSummaryRecords[j].FirstRecord = true;
			                    self.HistoricalSummaryRecords[j].RowSpan = uniqueClassCount(self.HistoricalSummaryRecords, currentStudentId);

			                    currentSchool = self.HistoricalSummaryRecords[j].SchoolName;
			                    self.HistoricalSummaryRecords[j].SchoolRowSpan = uniqueSchoolForClassCount(self.HistoricalSummaryRecords, currentStudentId, currentSchool);
			                    self.HistoricalSummaryRecords[j].FirstSchoolRecord = true;
			                } else {
			                    self.HistoricalSummaryRecords[j].FirstRecord = false;
			                    self.HistoricalSummarcurrentSchool = self.HistoricalSummaryRecords[j].SchoolName;

			                    if (currentSchool != self.HistoricalSummaryRecords[j].SchoolName) {
			                        currentSchool = self.HistoricalSummaryRecords[j].SchoolName;
			                        self.HistoricalSummaryRecords[j].SchoolRowSpan = uniqueSchoolForClassCount(self.HistoricalSummaryRecords, currentStudentId, currentSchool);
			                        self.HistoricalSummaryRecords[j].FirstSchoolRecord = true;
			                    }
			                    else {
			                        self.HistoricalSummaryRecords[j].SchoolRowSpan = 0;
			                        self.HistoricalSummaryRecords[j].FirstSchoolRecord = false
			                    }
			                }

                            // get unique class count, which will give me the rowspan for 

			                for (var i = 0; i < self.HistoricalSummaryRecords[j].ResultsByTDD.length; i++) {
			                    for (var k = 0; k < self.HistoricalSummaryRecords[j].ResultsByTDD[i].FieldResults.length; k++) {
			                        for (var r = 0; r < self.Fields.length; r++) {
			                            if (self.Fields[r].DatabaseColumn == self.HistoricalSummaryRecords[j].ResultsByTDD[i].FieldResults[k].DbColumn) {
			                                self.HistoricalSummaryRecords[j].ResultsByTDD[i].FieldResults[k].Field = angular.copy(self.Fields[r]);
			                            }
			                        }
			                    }
			                }
			            }
			        }

			        self.loadOptions = function () {
			            var returnObject = { ChangeType: 'initial' };


			            return $http.post(webApiBaseUrl + "/api/stackedbargraph/GetStackedBarGraphGroupingUpdatedOptions", returnObject).then(function (response) {
			                //self.options.schools.splice(1, 0);
			                self.options.schools.push.apply(self.options.schools, response.data.Schools);
			                self.options.grades.push.apply(self.options.grades, response.data.Grades);
			                self.options.teachers.push.apply(self.options.teachers, response.data.Teachers);
			                self.options.interventionists.push.apply(self.options.interventionists, response.data.Interventionists);
			                self.options.interventionGroups.push.apply(self.options.interventionGroups, response.data.InterventionGroups);
			                self.options.interventionStudents.push.apply(self.options.interventionStudents, response.data.InterventionStudents);
			                self.options.sections.push.apply(self.options.sections, response.data.Sections);
			                self.options.students.push.apply(self.options.students, response.data.Students);
			                self.options.schoolYears.push.apply(self.options.schoolYears, response.data.SchoolYears);
			                self.options.educationLabels.push.apply(self.options.educationLabels, response.data.EducationLabels);
			                self.options.titleOneTypes.push.apply(self.options.titleOneTypes, response.data.TitleOneTypes);
			                self.options.interventionTypes.push.apply(self.options.interventionTypes, response.data.InterventionTypes);
			                self.options.ethnicities.push.apply(self.options.ethnicities, response.data.Ethnicities);
			                self.options.genders.push.apply(self.options.genders, response.data.Genders);
			                self.options.attributeTypes.push.apply(self.options.attributeTypes, response.data.DropdownDataList);
			                self.options.testDueDates.push.apply(self.options.testDueDates, response.data.TestDueDates);
			                self.options.interventionAssessments.push.apply(self.options.interventionAssessments, response.data.InterventionAssessments);

			                // set defaults
			                if (response.data.SelectedSchoolYear != null) {
			                    for (var i = 0; i < self.options.schoolYears.length; i++) {
			                        if (self.options.schoolYears[i].id == response.data.SelectedSchoolYear) {
			                            self.options.selectedSchoolYear = self.options.schoolYears[i];
			                        }
			                    }
			                }

			                if (response.data.SelectedSchool != null) {
			                    for (var i = 0; i < self.options.schools.length; i++) {
			                        if (self.options.schools[i].id == response.data.SelectedSchool) {
			                            self.options.selectedSchools.splice(0, 0, self.options.schools[i]);
			                        }
			                    }
			                }

			                if (response.data.SelectedGrade != null) {
			                    for (var i = 0; i < self.options.grades.length; i++) {
			                        if (self.options.grades[i].id == response.data.SelectedGrade) {
			                            self.options.selectedGrades.splice(0, 0, self.options.grades[i]);
			                        }
			                    }
			                }

			                if (response.data.SelectedTeacher != null) {
			                    for (var i = 0; i < self.options.teachers.length; i++) {
			                        if (self.options.teachers[i].id == response.data.SelectedTeacher) {
			                            self.options.selectedTeachers.splice(0, 0, self.options.teachers[i]);
			                        }
			                    }
			                }

			                if (response.data.SelectedInterventionist != null) {
			                    for (var i = 0; i < self.options.interventionists.length; i++) {
			                        if (self.options.interventionists[i].id == response.data.SelectedInterventionist) {
			                            self.options.selectedInterventionists.splice(0, 0, self.options.interventionists[i]);
			                        }
			                    }
			                }

			                if (response.data.SelectedInterventionGroup != null) {
			                    for (var i = 0; i < self.options.interventionGroups.length; i++) {
			                        if (self.options.interventionGroups[i].id == response.data.SelectedInterventionGroup) {
			                            self.options.selectedInterventionGroups.splice(0, 0, self.options.interventionGroups[i]);
			                        }
			                    }
			                }

			                if (response.data.SelectedInterventionStudent != null) {
			                    for (var i = 0; i < self.options.interventionStudents.length; i++) {
			                        if (self.options.interventionStudents[i].id == response.data.SelectedInterventionStudent) {
			                            self.options.selectedInterventionStudents.splice(0, 0, self.options.interventionStudents[i]);
			                        }
			                    }
			                }

			                if (response.data.SelectedSection != null) {
			                    for (var i = 0; i < self.options.sections.length; i++) {
			                        if (self.options.sections[i].id == response.data.SelectedSection) {
			                            self.options.selectedSections.splice(0, 0, self.options.sections[i]);
			                        }
			                    }
			                }

                            // TODO: set a reasonable default TDD (should come from response.data.SelectedTDD!!!)
                            //if (self.options.testDueDates.length > 0 && self.tddEnabled) {
			                //    self.options.selectedTestDueDate = self.options.testDueDates[0];
			                //}

                            if (response.data.SelectedTestDueDateId != null) {
                                for (var i = 0; i < self.options.testDueDates.length; i++) {
                                    if (self.options.testDueDates[i].id == response.data.SelectedTestDueDateId) {
                                        self.options.selectedTestDueDate = self.options.testDueDates[i];
                                    }
                                }
                            }

			                //options.schoolYears.splice(1, 0, data.SchoolYears);
			                //options.schoolYears.push.apply(options.schoolYears, data.SchoolYears);
			                //options.benchmarkDates.splice(1, 0, data.TestDueDates);
			                //tions.benchmarkDates.push.apply(options.benchmarkDates, data.TestDueDates);
			            });
			        }

			        if (!explicitReturnLoadPromise) {
			            self.loadOptions();
			        }

			        if (!skipAssessmentFieldLoad) {
			            self.LoadAssessmentFields();
			        }

			        // load intervention assessments
			        self.LoadInterventionAssessments();

			        //this.initialLoad = function()
			        //{
			        //    this.loadOptions(this.options);
			        //}

			        self.loadSchoolChange = function (newSelections, oldSelections, forceLoad, dontResetSchoolGrouping) {

			            if (angular.equals(newSelections, oldSelections) && !angular.isDefined(forceLoad)) {
			                return;
			            }
			            // reset any grouping
			            if (!dontResetSchoolGrouping) {
			                self.options.selectedSchoolGrouping = null;
			            }
			            var returnObject = { ChangeType: 'school', Schools: self.options.selectedSchools, SchoolStartYear: self.options.selectedSchoolYear.id };

			            $http.post(webApiBaseUrl + "/api/stackedbargraph/GetStackedBarGraphGroupingUpdatedOptions", returnObject).success(function (data) {
			                self.options.grades.length = 0;
			                self.options.grades.push.apply(self.options.grades, data.Grades);
			                self.options.teachers.length = 0;;
			                self.options.teachers.push.apply(self.options.teachers, data.Teachers);
			                self.options.interventionists.length = 0;;
			                self.options.interventionists.push.apply(self.options.interventionists, data.Interventionists);
			                self.options.interventionGroups.length = 0;
			                self.options.interventionStudents.length = 0;
			                self.options.sections.length = 0;
			                self.options.students.length = 0;
			                self.options.selectedGrades = [];
			                self.options.selectedSections = [];
			                self.options.selectedStudents = [];
			                self.options.selectedTeachers = [];
			                self.options.selectedInterventionists = [];
			                self.options.selectedInterventionGroups = [];
			                self.options.selectedInterventionStudents = [];
			            });
			        }

			        self.loadSchoolsByGrade = function (grades, oldSelections, displayLabel) {
			            self.options.selectedSchoolGrouping = displayLabel;

			            // get schools, then call loadschoolchange with the schools
			            var paramObj = { Grades: grades };
			            $http.post(webApiBaseUrl + '/api/stackedbargraph/getschoolsbygrade', paramObj)
                            .then(function (response) {
                                self.options.selectedSchools.length = 0;
                                self.options.selectedSchools.push.apply(self.options.selectedSchools, response.data.Schools);

                                self.loadSchoolChange(self.options.selectedSchools, oldSelections, true, true);
                            });
			        };

			        self.clearSchools = function (oldSelections) {
			            self.options.selectedSchools.length = 0;
			            self.loadSchoolChange(self.options.selectedSchools, oldSelections, true);
			        };
			        self.loadGradeChange = function (newSelections, oldSelections) {

			            if (angular.equals(newSelections, oldSelections)) {
			                return;
			            }

			            var returnObject = { ChangeType: 'grade', Schools: self.options.selectedSchools, Grades: self.options.selectedGrades, SchoolStartYear: self.options.selectedSchoolYear.id };

			            $http.post(webApiBaseUrl + "/api/stackedbargraph/GetStackedBarGraphGroupingUpdatedOptions", returnObject).success(function (data) {
			                self.options.teachers.length = 0;
			                self.options.teachers.push.apply(self.options.teachers, data.Teachers);
			                self.options.sections.length = 0;
			                self.options.students.length = 0;
			                self.options.selectedSections = [];
			                self.options.selectedStudents = [];
			                self.options.selectedTeachers = [];
			            });
			        }
			        self.loadTeacherChange = function (newSelections, oldSelections) {

			            if (angular.equals(newSelections, oldSelections)) {
			                return;
			            }

			            var returnObject = { ChangeType: 'teacher', Schools: self.options.selectedSchools, Grades: self.options.selectedGrades, Teachers: self.options.selectedTeachers, SchoolStartYear: self.options.selectedSchoolYear.id };

			            $http.post(webApiBaseUrl + "/api/stackedbargraph/GetStackedBarGraphGroupingUpdatedOptions", returnObject).success(function (data) {
			                //self.options.sections = data.Sections;
			                //self.options.sections.splice(1, self.options.sections.length)
			                self.options.sections.length = 0;;
			                self.options.sections.push.apply(self.options.sections, data.Sections);
			                //if (data.Sections.length === 1) {
			                //    self.options.selectedSections = self.options.sections[1];
			                //}
			                //else {
			                //    self.options.selectedSections = self.options.sections[0];
			                //}
			                // load students
			            });
			        }

			        self.loadInterventionistChange = function (newSelections, oldSelections) {

			            if (angular.equals(newSelections, oldSelections)) {
			                return;
			            }

			            var returnObject = { ChangeType: 'interventionist', Schools: self.options.selectedSchools, Interventionists: self.options.selectedInterventionists, SchoolStartYear: self.options.selectedSchoolYear.id };

			            $http.post(webApiBaseUrl + "/api/stackedbargraph/GetStackedBarGraphGroupingUpdatedOptions", returnObject).success(function (data) {
			                self.options.interventionGroups.length = 0;;
			                self.options.interventionGroups.push.apply(self.options.interventionGroups, data.InterventionGroups);

                            // do i need to load students???
			            });
			        }

			        self.loadInterventionGroupChange = function (newSelections, oldSelections) {

			            if (angular.equals(newSelections, oldSelections)) {
			                return;
			            }

			            var returnObject = { ChangeType: 'interventiongroup', Schools: self.options.selectedSchools, Interventionists: self.options.selectedInterventionists, InterventionGroups: self.options.selectedInterventionGroups, SchoolStartYear: self.options.selectedSchoolYear.id };

			            $http.post(webApiBaseUrl + "/api/stackedbargraph/GetStackedBarGraphGroupingUpdatedOptions", returnObject).success(function (data) {
			                self.options.interventionStudents.length = 0;;
			                self.options.interventionStudents.push.apply(self.options.interventionStudents, data.InterventionStudents);
			            });
			        }

			        self.loadSectionChange = function (newSelections, oldSelections) {

			            if (angular.equals(newSelections, oldSelections)) {
			                return;
			            }

			            var returnObject = { ChangeType: 'section', Schools: self.options.selectedSchools, Grades: self.options.selectedGrades, Teachers: self.options.selectedTeachers, SchoolStartYear: self.options.selectedSchoolYear.id, Sections: self.options.selectedSections };

			            $http.post(webApiBaseUrl + "/api/stackedbargraph/GetStackedBarGraphGroupingUpdatedOptions", returnObject).success(function (data) {
			                self.options.students.length = 0;;
			                self.options.students.push.apply(self.options.students, data.Students);
			            });
			        }
			        self.loadSchoolYearChange = function () {
			            var returnObject = { ChangeType: 'schoolyear', SchoolStartYear: self.options.selectedSchoolYear.id, Schools: self.options.selectedSchools, Grades: self.options.selectedGrades, Teachers: self.options.selectedTeachers };

			            $http.post(webApiBaseUrl + "/api/stackedbargraph/GetStackedBarGraphGroupingUpdatedOptions", returnObject).then(function (response) {
			                self.options.testDueDates.length = 0;
			                self.options.testDueDates.push.apply(self.options.testDueDates, response.data.TestDueDates);

                            // TODO: find "current" TDD and select it from array
			                if (self.options.testDueDates.length > 0) {
			                    self.options.selectedTestDueDate = self.options.testDueDates[0];
			                }

			                self.options.interventionTypes.length = 0;
			                self.options.selectedInterventionTypes = [];
			                self.options.interventionTypes.push.apply(self.options.interventionTypes, response.data.InterventionTypes);

			                self.options.grades.length = 0;
			                self.options.grades.push.apply(self.options.grades, response.data.Grades);
			                self.options.selectedGrades = [];

			                self.options.sections.length = 0;
			                self.options.sections.push.apply(self.options.sections, response.data.Sections);
			                self.options.selectedSections = [];

			                self.options.teachers.length = 0;
			                self.options.teachers.push.apply(self.options.teachers, response.data.Teachers);
			                self.options.selectedTeachers = [];

			                if (response.data.SelectedGrade != null) {
			                    for (var i = 0; i < self.options.grades.length; i++) {
			                        if (self.options.grades[i].id == response.data.SelectedGrade) {
			                            self.options.selectedGrades.splice(0, 0, self.options.grades[i]);
			                        }
			                    }
			                }

			                if (response.data.SelectedTeacher != null) {
			                    for (var i = 0; i < self.options.teachers.length; i++) {
			                        if (self.options.teachers[i].id == response.data.SelectedTeacher) {
			                            self.options.selectedTeachers.splice(0, 0, self.options.teachers[i]);
			                        }
			                    }
			                }

			                if (response.data.SelectedSection != null) {
			                    for (var i = 0; i < self.options.sections.length; i++) {
			                        if (self.options.sections[i].id == response.data.SelectedSection) {
			                            self.options.selectedSections.splice(0, 0, self.options.sections[i]);
			                        }
			                    }
			                }
			            });
			        }
			    }
			    return nsStackedBarGraphOptionsFactory;
			

			}]
	)
        ;

    StackedBarGraphComparisonController.$inject = ['$scope', '$q', '$http', 'pinesNotifications', '$location', '$filter', 'nsStackedBarGraphOptionsFactory', '$routeParams', 'spinnerService','NSSummarySortManager'];


    function StackedBarGraphComparisonController($scope, $q, $http, pinesNotifications, $location, $filter, nsStackedBarGraphOptionsFactory, $routeParams, spinnerService, NSSummarySortManager) {
        $scope.settings = { chartHeader : ''};
        $scope.settings.summaryMode = false;
        $scope.settings.summaryView = 'Current Year Only (Sortable)';
        $scope.settings.summaryCategory = '';
        $scope.settings.summaryScoreGrouping = 1;
        $scope.settings.stacking = 'normal';
        $scope.settings.stackingDescription = 'Number of Students';
        $scope.settings.graphGenerated = false;
        $scope.groupCounter = 1;

        $scope.sortArray = [];
        $scope.attributeHeaders = {};
        $scope.manualSortHeaders = {};
        $scope.manualSortHeaders['Student'] = "fa";
        $scope.manualSortHeaders['StudentIdentifier'] = "fa";
        $scope.manualSortHeaders['SpecialED'] = "fa";
        $scope.manualSortHeaders['Services'] = "fa";
        $scope.manualSortHeaders['School'] = "fa";
        $scope.manualSortHeaders['Grade'] = "fa";
        $scope.manualSortHeaders['Teacher'] = "fa";
        $scope.manualSortHeaders['HomeLanguage'] = "fa";
        $scope.headerClassArray = [];
        $scope.sortMgr = new NSSummarySortManager();

        $scope.schoolYearSame = true;
        $scope.datesSame = true;
        $scope.schoolsSame = true;
        $scope.gradesSame = true;
        $scope.teachersSame = true;
        $scope.sectionsSame = true;
        $scope.interventionTypesSame = true;
        $scope.specialEdSame = true;
        $scope.assessmentFieldSame = true;
        $scope.attributesSameArrary = [];

        var highchartsNgConfig = {};
        $scope.groupsFactory = {};

        $scope.comparisonGroups = [];
        var firstGroup = new nsStackedBarGraphOptionsFactory("Group 1", true, false, true);
        $scope.attributeTypesArray = [];
        // load attributesSameArray
        firstGroup.loadOptions().then(function (response) {
            $scope.attributeTypesArray = firstGroup.options.attributeTypes;

            for (var i = 0; i < $scope.attributeTypesArray.length; i++) {
                $scope.attributesSameArrary[$scope.attributeTypesArray[i].Name] = true;
            }
        });
        firstGroup.DisplayName = "Group 1";
        firstGroup.AccordionHeaderDisplayName = "Group 1";
        $scope.comparisonGroups.push(firstGroup);
        $scope.filterOptions = $scope.comparisonGroups[0].options;
        //$scope.groupResults = nsStackedBarGraphGroupsOptionsService.groupResults;

        $scope.getSummaryHeader = function (displayDate) {
            if ($scope.groupsFactory.TestDueDates.length == 1) {
                return $scope.groupsFactory.options.selectedSchoolYear.text;
            } else {
                return displayDate;
            }
        }

        $scope.sort = function (fieldName) {
            $scope.sortMgr.sort(fieldName);
        };
        
        $scope.generateChartHeader = function () {
            $scope.schoolYearSame = true;
            $scope.datesSame = true;
            $scope.schoolsSame = true;
            $scope.gradesSame = true;
            $scope.teachersSame = true;
            $scope.sectionsSame = true;
            $scope.interventionTypesSame = true;
            $scope.specialEdSame = true;
            $scope.assessmentFieldSame = true;
            for (var i = 0; i < $scope.attributeTypesArray.length; i++) {
                $scope.attributesSameArrary[$scope.attributeTypesArray[i].Name] = true;
            }

            // no special header for 1 or fewere groups
            if ($scope.comparisonGroups.length <= 1) {
                $scope.settings.chartHeader = '';
                return '';
            }

            var headerText = ' - ';


            // loop over all the comparison groups and compare them to eachother
            for (var i = 0; i < $scope.comparisonGroups.length; i++) {


                // don't process the last one
                if (i !== $scope.comparisonGroups.length - 1) {
                    if ($scope.comparisonGroups[i].options.selectedSchoolYear.text != $scope.comparisonGroups[i + 1].options.selectedSchoolYear.text) {
                        $scope.schoolYearSame = false;
                    }

                    if ($scope.comparisonGroups[i].options.selectedTestDueDate.text != $scope.comparisonGroups[i + 1].options.selectedTestDueDate.text) {
                        $scope.datesSame = false;
                    }

                    if (!angular.equals($scope.comparisonGroups[i].options.selectedSchools, $scope.comparisonGroups[i + 1].options.selectedSchools)) {
                        $scope.schoolsSame = false;
                    }

                    if (!angular.equals($scope.comparisonGroups[i].options.selectedGrades, $scope.comparisonGroups[i + 1].options.selectedGrades)) {
                        $scope.gradesSame = false;
                    }

                    if (!angular.equals($scope.comparisonGroups[i].options.selectedTeachers, $scope.comparisonGroups[i + 1].options.selectedTeachers)) {
                        $scope.teachersSame = false;
                    }

                    if (!angular.equals($scope.comparisonGroups[i].options.selectedSections, $scope.comparisonGroups[i + 1].options.selectedSections)) {
                        $scope.sectionsSame = false;
                    }
                    if (!angular.equals($scope.comparisonGroups[i].options.selectedInterventionTypes, $scope.comparisonGroups[i + 1].options.selectedInterventionTypes)) {
                        $scope.interventionTypesSame = false;
                    }
                    if (($scope.comparisonGroups[i].options.selectedSpedTypes != null && $scope.comparisonGroups[i + 1].options.selectedSpedTypes != null && $scope.comparisonGroups[i].options.selectedSpedTypes.text != $scope.comparisonGroups[i + 1].options.selectedSpedTypes.text) ||
                        ($scope.comparisonGroups[i].options.selectedSpedTypes != null && $scope.comparisonGroups[i + 1].options.selectedSpedTypes == null) ||
                        ($scope.comparisonGroups[i].options.selectedSpedTypes == null && $scope.comparisonGroups[i + 1].options.selectedSpedTypes != null)) {
                        $scope.specialEdSame = false;
                    }
                    if (($scope.comparisonGroups[i].options.selectedAssessmentField != null && $scope.comparisonGroups[i + 1].options.selectedAssessmentField != null && ($scope.comparisonGroups[i].options.selectedAssessmentField.AssessmentName + '/' + $scope.comparisonGroups[i].options.selectedAssessmentField.DisplayLabel) != $scope.comparisonGroups[i + 1].options.selectedAssessmentField.AssessmentName + '/' + $scope.comparisonGroups[i + 1].options.selectedAssessmentField.DisplayLabel) ||
                        ($scope.comparisonGroups[i].options.selectedAssessmentField == null && $scope.comparisonGroups[i + 1].options.selectedAssessmentField != null) ||
                        ($scope.comparisonGroups[i].options.selectedAssessmentField != null && $scope.comparisonGroups[i + 1].options.selectedAssessmentField == null)) {
                        $scope.assessmentFieldSame = false;
                    }

                    // Attributes
                    for (var j = 0; j < $scope.comparisonGroups[i].options.attributeTypes.length; j++) {
                        var att = $scope.comparisonGroups[i].options.attributeTypes[j];
                        var att2 = $scope.comparisonGroups[i + 1].options.attributeTypes[j];
                        if (!angular.equals(att.selectedData, att2.selectedData)) {
                            $scope.attributesSameArrary[att.Name] = false;
                        }
                    }
                }
            }

            var delimiter = ' | ';

            // now add the ones that are the same across the board
            if ($scope.schoolYearSame) {
                headerText += $scope.comparisonGroups[0].options.selectedSchoolYear.text + delimiter;
            }
            if ($scope.datesSame) {
                headerText += $scope.comparisonGroups[0].options.selectedTestDueDate.text + delimiter;
            }
            if ($scope.schoolsSame) {
                if ($scope.comparisonGroups[0].options.selectedSchoolGrouping != null) {
                    headerText += $scope.comparisonGroups[0].options.selectedSchoolGrouping + delimiter;
                } else {
                    headerText += getMultiDropdownString($scope.comparisonGroups[0].options.selectedSchools, delimiter);
                }
            }
            if ($scope.gradesSame) {
                headerText += getMultiDropdownString($scope.comparisonGroups[0].options.selectedGrades, delimiter);
            }
            if ($scope.teachersSame) {
                headerText += getMultiDropdownString($scope.comparisonGroups[0].options.selectedTeachers, delimiter);
            }
            if ($scope.sectionsSame) {
                headerText += getMultiDropdownString($scope.comparisonGroups[0].options.selectedSections, delimiter);
            }
            if ($scope.interventionTypesSame) {
                headerText += getMultiDropdownString($scope.comparisonGroups[0].options.selectedInterventionTypes, delimiter);
            }
            if ($scope.specialEdSame && $scope.comparisonGroups[0].options.selectedSpedTypes != null) {
                headerText += $scope.comparisonGroups[0].options.selectedSpedTypes.text + delimiter
            }
            if ($scope.assessmentFieldSame && $scope.comparisonGroups[0].options.selectedAssessmentField != null) {
                headerText += $scope.comparisonGroups[0].options.selectedAssessmentField.AssessmentName + '/' + $scope.comparisonGroups[0].options.selectedAssessmentField.DisplayLabel + delimiter
            }

            for (var j = 0; j < $scope.comparisonGroups[0].options.attributeTypes.length; j++) {
                if ($scope.attributesSameArrary[$scope.comparisonGroups[0].options.attributeTypes[j].Name]) {
                    headerText += getMultiDropdownString($scope.comparisonGroups[0].options.attributeTypes[j].selectedData, delimiter);
                }
            }

            // if every field is differnt, make it a part of the group names and not the header
            $scope.settings.chartHeader =  headerText == ' - ' ? '' : headerText.substr(0, headerText.length - delimiter.length);
        }



        $scope.generateGroupName = function (groupName) {
            var group = null;

            for (var i = 0; i < $scope.comparisonGroups.length; i++) {
                if ($scope.comparisonGroups[i].name == groupName) {
                    group = $scope.comparisonGroups[i];
                }
            }
            var delimiter = ' <br/> ';

            if (group == null) {
                return groupName;
            }

            var originalName = group.name;
            var customName = '';

            if (!$scope.schoolYearSame) {
                customName += group.options.selectedSchoolYear.text + delimiter;
            }


            if (!$scope.datesSame) {
                customName += group.options.selectedTestDueDate.text + delimiter;
            }

            if (!$scope.schoolsSame && group.options.selectedSchools.length > 0) {
                if (group.options.selectedSchoolGrouping != null) {
                    customName += group.options.selectedSchoolGrouping + delimiter;
                } else {
                    customName += getMultiDropdownString(group.options.selectedSchools, delimiter);
                }
            }

            if (!$scope.gradesSame && group.options.selectedGrades.length > 0) {
                customName += getMultiDropdownString(group.options.selectedGrades, delimiter);
            }

            if (!$scope.teachersSame && group.options.selectedTeachers.length > 0) {
                customName += getMultiDropdownString(group.options.selectedTeachers, delimiter);
            }

            if (!$scope.sectionsSame && group.options.selectedSections.length > 0) {
                customName += getMultiDropdownString(group.options.selectedSections, delimiter);
            }
            if (!$scope.interventionTypesSame && group.options.selectedInterventionTypes.length > 0) {
                customName += getMultiDropdownString(group.options.selectedInterventionTypes, delimiter);
            }
            if (!$scope.specialEdSame && group.options.selectedSpedTypes != null) {
                customName += group.options.selectedSpedTypes.text + delimiter;
            }
            if (!$scope.assessmentFieldSame && group.options.selectedAssessmentField != null) {
                customName += group.options.selectedAssessmentField.AssessmentName + '/' + group.options.selectedAssessmentField.DisplayLabel + delimiter;
            }

            for (var j = 0; j < group.options.attributeTypes.length; j++) {
                if (!$scope.attributesSameArrary[group.options.attributeTypes[j].Name] && group.options.attributeTypes[j].selectedData && group.options.attributeTypes[j].selectedData.length > 0) {
                    customName += getMultiDropdownString(group.options.attributeTypes[j].selectedData, delimiter);
                }
            }

            // return default name unless name has changed
            return customName == '' ? originalName : customName.substr(0, customName.length - delimiter.length);
        }

        function getMultiDropdownString(aryOptions, delimiter) {
            var result = '';

            if (aryOptions && aryOptions.length > 0) {
                for (var i = 0; i < aryOptions.length; i++) {
                    result += aryOptions[i].text + ', ';
                }
                result = result.substring(0, result.length - 2) + delimiter;
            } else {
                result = '';
            }

            return result;
        }
        $scope.addNewGroup = function () {
            $scope.groupCounter++;
            var newGroup = new nsStackedBarGraphOptionsFactory("Group " + ($scope.groupCounter), true);
            newGroup.DisplayName = "Group " + ($scope.groupCounter);
            newGroup.AccordionHeaderDisplayName = "Group " + ($scope.groupCounter);
            $scope.comparisonGroups.push(newGroup);
        }

        $scope.removeGroup = function (name, $event) {
            $event.preventDefault();
            $event.stopPropagation();

            if ($scope.comparisonGroups.length == 1) {
                alert('You must keep at least one group.');
                return;
            }
            for (var i = 0; i < $scope.comparisonGroups.length; i++) {
                if ($scope.comparisonGroups[i].name === name) {
                    $scope.comparisonGroups.splice(i, 1);

                    return;
                }
            }
        }

        $scope.generateGraph = function (skipSpinner) {
            $scope.settings.anyResults = false;
            if(!skipSpinner)
                spinnerService.show('tableSpinner');

            var promiseCollection = [];
            // loop over each group and get data... build graph once they've all returned
            for (var i = 0; i < $scope.comparisonGroups.length; i++) {
                promiseCollection.push($scope.comparisonGroups[i].loadGroupData(true));
            }

            var studentResultsCollection = [];
            //var groupNameCollection = [];
            $q.all(promiseCollection).then(function (response) {
                for (var j = 0; j < response.length; j++) {
                    $scope.comparisonGroups[j].graphGenerated = true;
                    $scope.settings.anyResults = true;
                    studentResultsCollection.push(response[j].data);
                    //groupNameCollection.push()
                }                              

                updateDataFromServiceChange(studentResultsCollection);
            })
            .finally(function () {
                if(!skipSpinner)
                    spinnerService.hide('tableSpinner');
            });

            $scope.settings.graphGenerated = true;
        };

        $scope.changeSummaryMode = function() {
                changeToSummaryMode($scope.settings.summaryCategory, $scope.settings.summaryScoreGrouping);
        };

        $scope.$on('NSStudentAttributesUpdated', function (event, data) {
            if ($scope.settings.summaryMode == true) {
                $scope.changeSummaryMode();
            }
        });

        $scope.$watch('settings.stacking', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                if (newValue == 'normal') {
                    $scope.settings.stackingDescription = 'Number of Students';
                } else {
                    $scope.settings.stackingDescription = 'Percentage of Students';
                }

                $scope.generateGraph();
            }
        });

        var changeToSummaryMode = function (category, scoreGrouping, skipSpinner) {
            if(!skipSpinner)
                spinnerService.show('tableSpinner');
            $scope.settings.summaryCategory = category;
            $scope.settings.summaryScoreGrouping = scoreGrouping;

            // find proper factory based on category (groupName)
            for (var i = 0; i < $scope.comparisonGroups.length;i++) {
                if ($scope.comparisonGroups[i].DisplayName == category) {
                    $scope.groupsFactory = $scope.comparisonGroups[i];
                    $scope.groupsFactory.loadSummaryData(scoreGrouping, category, $scope.groupsFactory.options.selectedTestDueDate.text, ($scope.settings.summaryView !== 'Current Year Only (Sortable)'))
                        .then(function (response) {
                            $scope.settings.summaryMode = true;
                            $scope.settings.graphGenerated = true;

                            // set attribute headers (hidden ones here too)
                            $scope.attributeHeaders['Att1'] = $scope.groupsFactory.Att1Header;
                            $scope.attributeHeaders['Att2'] = $scope.groupsFactory.Att2Header;
                            $scope.attributeHeaders['Att3'] = $scope.groupsFactory.Att3Header;
                            $scope.attributeHeaders['Att4'] = $scope.groupsFactory.Att4Header;
                            $scope.attributeHeaders['Att5'] = $scope.groupsFactory.Att5Header;
                            $scope.attributeHeaders['Att6'] = $scope.groupsFactory.Att6Header;
                            $scope.attributeHeaders['Att7'] = $scope.groupsFactory.Att7Header;
                            $scope.attributeHeaders['Att8'] = $scope.groupsFactory.Att8Header;
                            $scope.attributeHeaders['Att9'] = $scope.groupsFactory.Att9Header;

                            // initialize sorting
                            for (var i = 0; i < $scope.groupsFactory.TestDueDates.length; i++) {
                                $scope.headerClassArray[i] = [];
                            }
                            $scope.sortMgr.initialize($scope.manualSortHeaders, $scope.sortArray, $scope.headerClassArray, $scope.groupsFactory.Fields);
                        })
                        .finally(function () {
                            if(!skipSpinner)
                                spinnerService.hide('tableSpinner');
                        });
                    break;
                }
            }
        }

        $scope.changeToChartMode = function () {
            $scope.settings.summaryView = 'Current Year Only (Sortable)';
            $scope.settings.summaryMode = false;
            $scope.settings.summaryCategory = '';
        }

        // generate graph if printmode
        if ($location.absUrl().indexOf('printmode=') >= 0) {
            var groupsParamArray = JSON.parse(decodeURIComponent($location.search().GroupsArrayParam));

            var summaryDataParam = JSON.parse(decodeURIComponent($location.search().SummaryDataParam));
            $scope.settings.summaryMode = summaryDataParam.summaryMode;
            $scope.settings.summaryView = summaryDataParam.summaryView;
            $scope.settings.summaryCategory = summaryDataParam.summaryCategory;
            $scope.settings.summaryScoreGrouping = summaryDataParam.summaryScoreGrouping;
            $scope.settings.stacking = summaryDataParam.stacking;
            $scope.settings.stackingDescription = summaryDataParam.stackingDescription;

            for (var i = 0; i < groupsParamArray.length; i++) {
                // TODO: COMPARISON GROUPS...LOAD DIFFERENTLY
                // there's one added by default
                if (i > 0) {
                    $scope.addNewGroup();
                }
                var newGroupsFactory = $scope.comparisonGroups[$scope.comparisonGroups.length - 1];
                //newGroupsFactory.options.attributeTypes = [];
                // set selected items from deserialed groups and generate
                newGroupsFactory.DisplayName = groupsParamArray[i].DisplayName;
                newGroupsFactory.AccordionHeaderDisplayName = groupsParamArray[i].AccordionHeaderDisplayName;
                //newGroupsFactory.Fields = groupsParamArray[i].Fields;
                newGroupsFactory.options.selectedAssessmentField = groupsParamArray[i].selectedAssessmentField;
                newGroupsFactory.options.selectedEducationLabels = groupsParamArray[i].selectedEducationLabels;
                newGroupsFactory.options.selectedSchoolYear = groupsParamArray[i].selectedSchoolYear;
                newGroupsFactory.options.selectedSchools = groupsParamArray[i].selectedSchools;
                newGroupsFactory.options.selectedTeachers = groupsParamArray[i].selectedTeachers;
                newGroupsFactory.options.selectedSections = groupsParamArray[i].selectedSections;
                newGroupsFactory.options.selectedStudents = groupsParamArray[i].selectedStudents;
                newGroupsFactory.options.selectedInterventionTypes = groupsParamArray[i].selectedInterventionTypes;
                newGroupsFactory.options.selectedGrades = groupsParamArray[i].selectedGrades;
                newGroupsFactory.options.selectedTestDueDate = groupsParamArray[i].selectedTestDueDate;
                newGroupsFactory.options.attributeTypes = groupsParamArray[i].attributeTypes;
                newGroupsFactory.graphGenerated = true;

            }
            if ($scope.settings.summaryMode) {
                changeToSummaryMode($scope.settings.summaryCategory, $scope.settings.summaryScoreGrouping, true);                
                $scope.settings.anyResults = true;
            } else {
                $scope.generateGraph(true);
            }
        }

        function getTitle() {
            var title = '';

            title += '<b>Schools: </b>'
            if ($scope.filterOptions.selectedSchools.length > 0) {
                for (var i = 0; i < $scope.filterOptions.selectedSchools.length; i++) {
                    title += $scope.filterOptions.selectedSchools[i].text + ',';
                }
            } else {
                title += ' All '
            }

            title += '<b>Grades: </b>'
            if($scope.filterOptions.selectedGrades.length > 0) {
                for (var i = 0; i < $scope.filterOptions.selectedGrades.length; i++) {
                    title += $scope.filterOptions.selectedGrades[i].text + ',';
                }
            } else {
                title += ' All '
            }

            return title;
        }

 
        function updateDataFromServiceChange(studentResultsCollection) {
            // might need this
            //angular.copy(data.results, $scope.studentResults);
            //$scope.studentResults = nsStackedBarGraphGroupsOptionsService.groupResults.data;
            var re = /<br\/>/g;
            //return;
            var seriesArray = [];
            var categoriesArray = [];
            $scope.generateChartHeader();
            
            // set up series
            for (var i = 0; i < studentResultsCollection.length; i++) {
                var currentResult = studentResultsCollection[i];
                var groupName = $scope.generateGroupName(currentResult.GroupName);
                
                // update group display names
                for (var n = 0; n < $scope.comparisonGroups.length; n++) {
                    if (currentResult.GroupName == $scope.comparisonGroups[n].name) {
                        
                        $scope.comparisonGroups[n].DisplayName = groupName;
                        $scope.comparisonGroups[n].AccordionHeaderDisplayName = groupName.replace(re," | ");
                    }
                }

                var foundCategory = $filter('filter')(categoriesArray, groupName, true);
                // see if category already exists, if not, add it
                if (!foundCategory.length) {
                    categoriesArray.push(groupName);//[categoriesArray.length] = { name: currentResult.DueDate, categories: [currentResult.DueDate] };
                }

                for (var j = 0; j < currentResult.Results.length; j++) {
                    var currentScore = currentResult.Results[j];
                    //labels: {rotation: -90}
                    // create a data array for each scoregrouping
                    // FIX THIS... need to be able to create an array of arrays with the index being the scoregrouping

                    var groupingName = "";
                    var groupingColor = "";

                    if (currentScore.ScoreGrouping == 1) {
                        groupingName = "Exceeds Expectations";
                        groupingColor = "#4697ce";
                    }
                    if (currentScore.ScoreGrouping == 2) {
                        groupingName = "Meets Expectations";
                        groupingColor = "#90ED7D";
                    }
                    if (currentScore.ScoreGrouping == 3) {
                        groupingName = "Approaches Expectations";
                        groupingColor = "#E4D354";
                    }
                    if (currentScore.ScoreGrouping == 4) {
                        groupingName = "Does Not Meet Expectations";
                        groupingColor = "#BF453D";
                    }

                    if (seriesArray[currentScore.ScoreGrouping] == null) {
                        seriesArray[currentScore.ScoreGrouping] = { name: groupingName, color: groupingColor, data: [currentScore.NumberOfResults], id: currentScore.ScoreGrouping }
                    }
                    else {
                        seriesArray[currentScore.ScoreGrouping].data.push(currentScore.NumberOfResults);
                    }
                }
            }

            highchartsNgConfig = {
                //This is not a highcharts object. It just looks a little like one!
                options: {
                    //This is the Main Highcharts chart config. Any Highchart options are valid here.
                    //will be ovverriden by values specified below.
                    chart: {
                        type: 'column',
                        //options3d: {
                        //    enabled: true,
                        //    alpha: 15,
                        //    beta: 15,
                        //    viewDistance: 25,
                        //    depth: 40
                        //},
                        //marginTop: 80,
                        //marginRight: 40
                    },
                    tooltip: {
                        pointFormat: '<span style="color:{series.color}">\u25CF</span>  <span style="color:#666666">{series.name}</span>: <b>{point.y} Students</b> ({point.percentage:.0f}%)<br/>',
                        style: {
                            padding: 10,
                            fontWeight: 'bold'
                        }, useHTML: true
                    },
                    plotOptions: {
                        series: {
                            cursor: 'pointer',
                            point: {
                                events: {
                                    click: function (event) {
                                        var category = this.category;
                                        var scoreGrouping = this.series.userOptions.id;

                                        changeToSummaryMode(category, scoreGrouping);
                                    }
                                }
                            }
                        },
                        column: {
                            stacking: $scope.settings.stacking,
                            dataLabels: {
                                enabled: true,
                                color: (Highcharts.theme && Highcharts.theme.dataLabelsColor) || 'white',
                                style: {
                                    textShadow: '0 0 3px black'
                                },
                                formatter: function () {
                                    if (this.y > 0 && $scope.settings.stacking === 'normal')
                                        return this.y;
                                    if (this.y > 0 && $scope.settings.stacking === 'percent')
                                        return this.percentage.toFixed(0) + '%';
                                }
                            }
                        }
                    }
                },

                yAxis: {
                    allowDecimals: false,
                    min: 0,
                    title: {
                        text: $scope.settings.stackingDescription
                    },
                    stackLabels: {
                        enabled: true,
                        style: {
                            fontWeight: 'bold',
                            color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                        }
                    }
                },

                //The below properties are watched separately for changes.

                //Series object (optional) - a list of series using normal highcharts series options.
                series: seriesArray,
                //Title configuration (optional)
                title: {
                    text: 'Compare Student Groups ' + $scope.settings.chartHeader
                },
                //Boolean to control showng loading status on chart (optional)
                //Could be a string if you want to show specific loading text.
                loading: false,
                //Configuration for the xAxis (optional). Currently only one x axis can be dynamically controlled.
                //properties currentMin and currentMax provied 2-way binding to the chart's maximimum and minimum
                xAxis: {
                    categories: categoriesArray

                },
                //Whether to use HighStocks instead of HighCharts (optional). Defaults to false.
                useHighStocks: false,
                //size (optional) if left out the chart will default to size of the div or something sensible.
                //                size: {
                //                  width: 1000,
                //                height: 600
                //           },
                //function (optional)
                func: function (chart) {
                    //setup some logic for the chart
                }

            };

            $scope.chartConfig = highchartsNgConfig;
        }
    }


    
    PLCInterventionPlanningController.$inject = ['$scope', '$q', '$http', 'pinesNotifications', '$location', '$filter', 'nsFilterOptionsService', '$routeParams', 'spinnerService', 'NSSummarySortManager', 'webApiBaseUrl','$uibModal', '$timeout'];


    function PLCInterventionPlanningController($scope, $q, $http, pinesNotifications, $location, $filter, nsFilterOptionsService, $routeParams, spinnerService, NSSummarySortManager, webApiBaseUrl, $uibModal, $timeout) {
        $scope.settings = {};
        $scope.settings.summaryMode = false;
        $scope.settings.summaryView = 'Current Year Only (Sortable)';
        $scope.settings.summaryCategory = '';
        $scope.settings.summaryScoreGrouping = 1;
        $scope.settings.stacking = 'normal';
        $scope.settings.stackingDescription = 'Number of Students';
        $scope.settings.graphGenerated = false;

        $scope.headerClassArray = [];
        $scope.groupsFactory = {};
        $scope.groupsFactory.SummaryRecords = [];
        $scope.groupsFactory.TestDueDates = [];

        $scope.filterOptions = nsFilterOptionsService.options;

        $scope.groupsFactory.TestDueDates = $scope.filterOptions.benchmarkDates;
        $scope.report = {};
        $scope.badgeClass = function (grouping) {
            switch (grouping) {
                case 1:
                    return 'badge-primary';
                case 2:
                    return 'badge-success';
                case 3:
                    return 'badge-warning';
                case 4:
                    return 'badge-danger';
            }

            return '';
        }

        $scope.navigateToTdd = function (tddid) {
            $scope.filterOptions.selectedBenchmarkDate = nsFilterOptionsService.getBenchmarkDateById(tddid);
            nsFilterOptionsService.changeBenchmarkDate();
        }

        var loadPLCInterventionPlanningData = function() {


            var returnObject = {
                GradeId: $scope.filterOptions.selectedGrade.id,
                SchoolYear: $scope.filterOptions.selectedSchoolYear.id,
                BenchmarkDateId: $scope.filterOptions.selectedBenchmarkDate.id,
                SchoolId: $scope.filterOptions.selectedSchool.id
            };

            return $http.post(webApiBaseUrl + "/api/stackedbargraph/GetPLCPlanningReport", returnObject);
        }

        $scope.summaryPopup = function (scoreGrouping, tdd, dataType, isHistorical, sectionId, staffId) {
            var modalInstance = $uibModal.open({
                templateUrl: 'stackedBarGraphSummary.html',
                scope: $scope,
                controller: function ($scope, $uibModalInstance) {

                    $timeout(function () {
                        $('div.modal-dialog').addClass('modal-dialog-max');
                        $('div.modal-content').addClass('modal-content-max');
                    }, 250);

                    $scope.headerClassArray = [];
                    $scope.settings = {};
                    $scope.settings.summaryMode = false;
                    $scope.settings.summaryView = 'Current Year Only (Sortable)';
                    $scope.settings.summaryCategory = '';
                    $scope.settings.summaryScoreGrouping = 1;
                    $scope.settings.stacking = 'normal';
                    $scope.settings.stackingDescription = 'Number of Students';

                    $scope.sortArray = [];
                    $scope.manualSortHeaders = {};
                    $scope.attributeHeaders = {};
                    $scope.manualSortHeaders['Student'] = "fa";
                    $scope.manualSortHeaders['StudentIdentifier'] = "fa";
                    $scope.manualSortHeaders['SpecialED'] = "fa";
                    $scope.manualSortHeaders['Services'] = "fa";
                    $scope.manualSortHeaders['Att1'] = "fa";
                    $scope.manualSortHeaders['Att2'] = "fa";
                    $scope.manualSortHeaders['Att3'] = "fa";
                    $scope.manualSortHeaders['Att4'] = "fa";
                    $scope.manualSortHeaders['Att5'] = "fa";
                    $scope.manualSortHeaders['Att6'] = "fa";
                    $scope.manualSortHeaders['Att7'] = "fa";
                    $scope.manualSortHeaders['Att8'] = "fa";
                    $scope.manualSortHeaders['Att9'] = "fa";
                    $scope.manualSortHeaders['School'] = "fa";
                    $scope.manualSortHeaders['Grade'] = "fa";
                    $scope.manualSortHeaders['Teacher'] = "fa";
                    $scope.manualSortHeaders['HomeLanguage'] = "fa";
                    //$scope.manualSortHeaders.studentIdHeaderClass = "fa";
                    //$scope.manualSortHeaders.studentServicesHeaderClass = "fa";
                    //$scope.manualSortHeaders.studentSpedLabelsHeaderClass = "fa";
                    //$scope.manualSortHeaders.studentSchoolHeaderClass = "fa";
                    //$scope.manualSortHeaders.studentGradeHeaderClass = "fa";
                    //$scope.manualSortHeaders.studentTeacherHeaderClass = "fa";
                    //$scope.manualSortHeaders.studentHomeLanguageHeaderClass = "fa";
                    $scope.headerClassArray = [];
                    $scope.sortMgr = new NSSummarySortManager();
                    
                    //loadSummaryData(scoreGrouping, tdd, dataType, false, sectionId, staffId);
                    //
                    $scope.changeSummaryMode = function () {
                        spinnerService.show('tableSpinner');
                        changeToSummaryMode().then(
                            function () {
                                spinnerService.hide('tableSpinner');
                            });
                    };

                    var changeToSummaryMode = function () {

                        

                        return loadSummaryData(scoreGrouping, tdd, dataType, ($scope.settings.summaryView !== 'Current Year Only (Sortable)'), sectionId, staffId)
                            .then(function (response) {
                                $scope.settings.summaryMode = true;

                                // set attribute headers (hidden ones here too)
                                //$scope.attributeHeaders['Att1'] = $scope.groupsFactory.Att1Header;
                                //$scope.attributeHeaders['Att2'] = $scope.groupsFactory.Att2Header;
                                //$scope.attributeHeaders['Att3'] = $scope.groupsFactory.Att3Header;
                                //$scope.attributeHeaders['Att4'] = $scope.groupsFactory.Att4Header;
                                //$scope.attributeHeaders['Att5'] = $scope.groupsFactory.Att5Header;
                                //$scope.attributeHeaders['Att6'] = $scope.groupsFactory.Att6Header;
                                //$scope.attributeHeaders['Att7'] = $scope.groupsFactory.Att7Header;
                                //$scope.attributeHeaders['Att8'] = $scope.groupsFactory.Att8Header;
                                //$scope.attributeHeaders['Att9'] = $scope.groupsFactory.Att9Header;


                            });
                    }

                    $scope.sort = function (fieldName) {
                        $scope.sortMgr.sort(fieldName);
                    };

                    var loadSummaryData = function (scoreGrouping, tdd, dataType, isHistorical, sectionId, staffId) {
                        var selectedAttributes = [];
                        // initialize sorting
                        for (var i = 0; i < $scope.groupsFactory.TestDueDates.length; i++) {
                            $scope.headerClassArray[i] = [];
                        }
                        $scope.sortMgr.initialize($scope.manualSortHeaders, $scope.sortArray, $scope.headerClassArray, $scope.groupsFactory.Fields);
                        //for (var i = 0; i < self.options.attributeTypes.length; i++) {
                        //    var currentType = self.options.attributeTypes[i];
                        //    selectedAttributes.push({ AttributeTypeId: currentType.AttributeTypeId, Name: currentType.Name, DropDownData: currentType.selectedData });
                        //}

                        var edType = null;
                        var title1 = null;
                        var el = null;
                        var gt = null;
                        var ddlList = [];

                        switch (dataType) {
                            case 'GenEd':
                                edType = { id: 0, text: 'irrelevant' };
                                break;
                            case 'SpEd':
                                edType = { id: 1, text: 'irrelevant' };
                                break;
                            case 'Title':
                                title1 = { AttributeTypeId: 3, DropDownData: [{ id: 1 }, { id: 2 }, { id: 3 }, { id: 4 }] };
                                ddlList.push(title1);
                                break;
                            case 'EL':
                                el = { AttributeTypeId: 6, DropDownData: [{ id: 1 }] };
                                ddlList.push(el);
                                break;
                            case 'GT':
                                gt = { AttributeTypeId: 7, DropDownData: [{ id: 1 }] };
                                ddlList.push(gt);
                                break;
                        }
                        
                        var selectedSchools = [ {id : $scope.filterOptions.selectedSchool.id, text : 'irrelevant'}];
                        var selectedGrades = [ {id : $scope.filterOptions.selectedGrade.id, text :'irrelevant'}];

                        var returnObject = {
                            Schools: selectedSchools,
                            Grades: selectedGrades,
                            Teachers: [{ id: staffId }],
                            Sections: [{ id: sectionId }],
                            SpecialEd: edType,
                            SchoolStartYear: $scope.filterOptions.selectedSchoolYear.id,
                            ScoreGrouping: scoreGrouping,
                            TestDueDate: tdd,
                            AssessmentField:  { AssessmentId: 1,  DatabaseColumn: 'FPValueID'},
                            DropdownDataList: ddlList,
                            InterventionTypes: []
                        };

                        if (isHistorical) {
                            return $http.post(webApiBaseUrl + "/api/stackedbargraph/GetStackedBarGraphGroupHistoricalSummary", returnObject).then(function (response) {
               
                                $scope.groupsFactory.TestDueDates = response.data.TestDueDates;
                                $scope.groupsFactory.HistoricalSummaryRecords = response.data.SummaryRecords;
                                $scope.groupsFactory.Fields = response.data.Fields;
                                HistoricalSummaryDataPostProcess();
                                fixDates($scope.groupsFactory.TestDueDates);
                            });
                        } else {
                            return $http.post(webApiBaseUrl + "/api/stackedbargraph/GetStackedBarGraphGroupSummary", returnObject).then(function (response) {

                                $scope.groupsFactory.TestDueDates = response.data.TestDueDates;
                                $scope.groupsFactory.SummaryRecords = response.data.SummaryRecords;
                                $scope.groupsFactory.Fields = response.data.Fields;
                                SummaryDataPostProcess();
                            });
                        }
                    }

                    function fixDates(aryDates) {
                        for (var i = 0; i < aryDates.length; i++) {
                            aryDates[i].DisplayDate = moment(aryDates[i].DisplayDate.substring(0, aryDates[i].DisplayDate.indexOf('T')));
                        }
                    }


                    $scope.getSummaryHeader = function (displayDate) {
                        if ($scope.groupsFactory.TestDueDates.length == 1) {
                            return $scope.groupsFactory.options.selectedSchoolYear.text;
                        } else {
                            return displayDate;
                        }
                    }

                    var SummaryDataPostProcess = function () {
                        for (var j = 0; j < $scope.groupsFactory.SummaryRecords.length; j++) {
                            for (var i = 0; i < $scope.groupsFactory.SummaryRecords[j].ResultsByTDD.length; i++) {
                                for (var k = 0; k < $scope.groupsFactory.SummaryRecords[j].ResultsByTDD[i].FieldResults.length; k++) {
                                    for (var r = 0; r < $scope.groupsFactory.Fields.length; r++) {
                                        if ($scope.groupsFactory.Fields[r].DatabaseColumn == $scope.groupsFactory.SummaryRecords[j].ResultsByTDD[i].FieldResults[k].DbColumn) {
                                            $scope.groupsFactory.SummaryRecords[j].ResultsByTDD[i].FieldResults[k].Field = $scope.groupsFactory.Fields[r];
                                        }
                                    }
                                }
                            }
                        }
                    }

                    function uniqueClassCount(summaryRecords, studentId) {
                        var uniqueCounter = 0;
                        var currentSection = '';

                        var recordsForStudentId = $filter('filter')(summaryRecords, { StudentID: studentId });

                        for (var i = 0; i < recordsForStudentId.length; i++) {
                            if (currentSection !== recordsForStudentId[i].Section) {
                                currentSection = recordsForStudentId[i].Section;
                                uniqueCounter++;
                            }
                        }

                        return uniqueCounter;
                    }

                    function uniqueSchoolForClassCount(summaryRecords, studentId, schoolName) {

                        var recordsForStudentIdSchool = $filter('filter')(summaryRecords, { StudentID: studentId, SchoolName: schoolName });

                        return recordsForStudentIdSchool.length;
                    }
                    var HistoricalSummaryDataPostProcess = function () {
                        var currentStudentId = -1;
                        var currentSchool = '';

                        for (var j = 0; j < $scope.groupsFactory.HistoricalSummaryRecords.length; j++) {
                            if (currentStudentId != $scope.groupsFactory.HistoricalSummaryRecords[j].StudentID) {
                                currentStudentId = $scope.groupsFactory.HistoricalSummaryRecords[j].StudentID;
                                $scope.groupsFactory.HistoricalSummaryRecords[j].FirstRecord = true;
                                $scope.groupsFactory.HistoricalSummaryRecords[j].RowSpan = uniqueClassCount($scope.groupsFactory.HistoricalSummaryRecords, currentStudentId);

                                currentSchool = $scope.groupsFactory.HistoricalSummaryRecords[j].SchoolName;
                                $scope.groupsFactory.HistoricalSummaryRecords[j].SchoolRowSpan = uniqueSchoolForClassCount($scope.groupsFactory.HistoricalSummaryRecords, currentStudentId, currentSchool);
                                $scope.groupsFactory.HistoricalSummaryRecords[j].FirstSchoolRecord = true;
                            } else {
                                $scope.groupsFactory.HistoricalSummaryRecords[j].FirstRecord = false;
                                $scope.groupsFactory.HistoricalSummarcurrentSchool = $scope.groupsFactory.HistoricalSummaryRecords[j].SchoolName;

                                if (currentSchool != $scope.groupsFactory.HistoricalSummaryRecords[j].SchoolName) {
                                    currentSchool = $scope.groupsFactory.HistoricalSummaryRecords[j].SchoolName;
                                    $scope.groupsFactory.HistoricalSummaryRecords[j].SchoolRowSpan = uniqueSchoolForClassCount($scope.groupsFactory.HistoricalSummaryRecords, currentStudentId, currentSchool);
                                    $scope.groupsFactory.HistoricalSummaryRecords[j].FirstSchoolRecord = true;
                                }
                                else {
                                    $scope.groupsFactory.HistoricalSummaryRecords[j].SchoolRowSpan = 0;
                                    $scope.groupsFactory.HistoricalSummaryRecords[j].FirstSchoolRecord = false
                                }
                            }

                            // get unique class count, which will give me the rowspan for 

                            for (var i = 0; i < $scope.groupsFactory.HistoricalSummaryRecords[j].ResultsByTDD.length; i++) {
                                for (var k = 0; k < $scope.groupsFactory.HistoricalSummaryRecords[j].ResultsByTDD[i].FieldResults.length; k++) {
                                    for (var r = 0; r < $scope.groupsFactory.Fields.length; r++) {
                                        if ($scope.groupsFactory.Fields[r].DatabaseColumn == $scope.groupsFactory.HistoricalSummaryRecords[j].ResultsByTDD[i].FieldResults[k].DbColumn) {
                                            $scope.groupsFactory.HistoricalSummaryRecords[j].ResultsByTDD[i].FieldResults[k].Field = $scope.groupsFactory.Fields[r];
                                        }
                                    }
                                }
                            }
                        }
                    }

                    loadSummaryData(scoreGrouping, tdd, dataType, false, sectionId, staffId);

                    $scope.cancel = function () {
                        $uibModalInstance.dismiss('cancel');
                    };
                },
                size: 'md',
            });
        }

        $scope.$watchGroup(['filterOptions.selectedGrade.id', 'filterOptions.selectedBenchmarkDate.id', 'filterOptions.selectedSchool.id'], function (newValue, oldValue, scope) {
            if (angular.isDefined(newValue[0]) && angular.isDefined(newValue[1]) && angular.isDefined(newValue[2])) {
                if (newValue[0] != oldValue[0] || newValue[1] != oldValue[1] || newValue[2] != oldValue[2]) {
                    $scope.loadReport();
                }
            }
        });

        $scope.getResultCount = function (grouping) {
            var totalScore = 0;

            for(var i = 0; i < grouping.length; i++)
            {
                totalScore += grouping[i].NumberOfResults;
            }

            return totalScore;
        } 

        $scope.loadReport = function () {
            if ($scope.filterOptions.selectedGrade.id != -1 && $scope.filterOptions.selectedSchoolYear != -1 && $scope.filterOptions.selectedBenchmarkDate != -1) {
                spinnerService.show('tableSpinner');
                loadPLCInterventionPlanningData()
                .then(function (response) {
                    $scope.report = response.data;
                })
                .finally(function () {
                    spinnerService.hide('tableSpinner');
                });
                $scope.settings.graphGenerated = true;
            }
        }

        //var changeToSummaryMode = function (category, scoreGrouping) {

        //    $scope.settings.summaryCategory = category;
        //    $scope.settings.summaryScoreGrouping = scoreGrouping;

        //    return $scope.loadSummaryData(scoreGrouping, category, null, ($scope.settings.summaryView !== 'Current Year Only (Sortable)'))
        //        .then(function (response) {
        //            $scope.settings.summaryMode = true;

        //            // set attribute headers (hidden ones here too)
        //            $scope.attributeHeaders['Att1'] = $scope.groupsFactory.Att1Header;
        //            $scope.attributeHeaders['Att2'] = $scope.groupsFactory.Att2Header;
        //            $scope.attributeHeaders['Att3'] = $scope.groupsFactory.Att3Header;
        //            $scope.attributeHeaders['Att4'] = $scope.groupsFactory.Att4Header;
        //            $scope.attributeHeaders['Att5'] = $scope.groupsFactory.Att5Header;
        //            $scope.attributeHeaders['Att6'] = $scope.groupsFactory.Att6Header;
        //            $scope.attributeHeaders['Att7'] = $scope.groupsFactory.Att7Header;
        //            $scope.attributeHeaders['Att8'] = $scope.groupsFactory.Att8Header;
        //            $scope.attributeHeaders['Att9'] = $scope.groupsFactory.Att9Header;

        //            // initialize sorting
        //            for (var i = 0; i < $scope.groupsFactory.TestDueDates.length; i++) {
        //                $scope.headerClassArray[i] = [];
        //            }
        //            $scope.sortMgr.initialize($scope.manualSortHeaders, $scope.sortArray, $scope.headerClassArray, $scope.groupsFactory.Fields);
        //        });
        //}

        
        
    }

    StackedBarGraphGroupsController.$inject = ['$scope', '$q', '$http', 'pinesNotifications', '$location', '$filter', 'nsStackedBarGraphOptionsFactory', '$routeParams', 'spinnerService', 'NSSummarySortManager'];


    function StackedBarGraphGroupsController($scope, $q, $http, pinesNotifications, $location, $filter, nsStackedBarGraphOptionsFactory, $routeParams, spinnerService, NSSummarySortManager) {
        $scope.settings = {};
        $scope.settings.summaryMode = false;
        $scope.settings.summaryView = 'Current Year Only (Sortable)';
        $scope.settings.summaryCategory = '';
        $scope.settings.summaryScoreGrouping = 1;
        $scope.settings.stacking = 'normal';
        $scope.settings.stackingDescription = 'Number of Students';
        $scope.settings.graphGenerated = false;

        // sort initialization
        $scope.sortArray = [];
        $scope.manualSortHeaders = {};
        $scope.attributeHeaders = {};
        $scope.manualSortHeaders['Student'] = "fa";
        $scope.manualSortHeaders['StudentIdentifier'] = "fa";
        $scope.manualSortHeaders['SpecialED'] = "fa";
        $scope.manualSortHeaders['Services'] = "fa";
        $scope.manualSortHeaders['Att1'] = "fa";
        $scope.manualSortHeaders['Att2'] = "fa";
        $scope.manualSortHeaders['Att3'] = "fa";
        $scope.manualSortHeaders['Att4'] = "fa";
        $scope.manualSortHeaders['Att5'] = "fa";
        $scope.manualSortHeaders['Att6'] = "fa";
        $scope.manualSortHeaders['Att7'] = "fa";
        $scope.manualSortHeaders['Att8'] = "fa";
        $scope.manualSortHeaders['Att9'] = "fa";
        $scope.manualSortHeaders['School'] = "fa";
        $scope.manualSortHeaders['Grade'] = "fa";
        $scope.manualSortHeaders['Teacher'] = "fa";
        $scope.manualSortHeaders['HomeLanguage'] = "fa";
        //$scope.manualSortHeaders.studentIdHeaderClass = "fa";
        //$scope.manualSortHeaders.studentServicesHeaderClass = "fa";
        //$scope.manualSortHeaders.studentSpedLabelsHeaderClass = "fa";
        //$scope.manualSortHeaders.studentSchoolHeaderClass = "fa";
        //$scope.manualSortHeaders.studentGradeHeaderClass = "fa";
        //$scope.manualSortHeaders.studentTeacherHeaderClass = "fa";
        //$scope.manualSortHeaders.studentHomeLanguageHeaderClass = "fa";
        $scope.headerClassArray = [];
        $scope.sortMgr = new NSSummarySortManager();

        var highchartsNgConfig = {};
        $scope.groupsFactory = new nsStackedBarGraphOptionsFactory('Compare Group Across Benchmark Dates', false);
        $scope.filterOptions = $scope.groupsFactory.options;
        $scope.groupResults = $scope.groupsFactory.groupResults;

        $scope.generateGraph = function () {
            spinnerService.show('tableSpinner');
            $scope.groupsFactory.loadGroupData()
                .then(function (response) {
                    $scope.groupsFactory.groupResults.data = response.data;
                    updateDataFromServiceChange();
                })
                .finally(function () {
                    spinnerService.hide('tableSpinner');
                });
            $scope.settings.graphGenerated = true;
        };

        $scope.sort = function (fieldName) {
            $scope.sortMgr.sort(fieldName);
        };

        $scope.getSummaryHeader = function (displayDate) {
            if ($scope.groupsFactory.TestDueDates.length == 1) {
                return $scope.groupsFactory.options.selectedSchoolYear.text;
            } else {
                return displayDate;
            }
        }

        $scope.changeSummaryMode = function () {
            spinnerService.show('tableSpinner');
            changeToSummaryMode($scope.settings.summaryCategory, $scope.settings.summaryScoreGrouping).then(
                function () {
                    spinnerService.hide('tableSpinner');
                });
        };

        $scope.$on('NSStudentAttributesUpdated', function (event, data) {
            if ($scope.settings.summaryMode == true) {
                $scope.changeSummaryMode();
            }
        });


        $scope.$watch('settings.stacking', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                if (newValue == 'normal') {
                    $scope.settings.stackingDescription = 'Number of Students';
                } else {
                    $scope.settings.stackingDescription = 'Percentage of Students';
                }

                updateDataFromServiceChange();
            }
        });

        var changeToSummaryMode = function (category, scoreGrouping) {
            
            $scope.settings.summaryCategory = category;
            $scope.settings.summaryScoreGrouping = scoreGrouping;

            return $scope.groupsFactory.loadSummaryData(scoreGrouping, category, null, ($scope.settings.summaryView !== 'Current Year Only (Sortable)'))
                .then(function (response) {
                    $scope.settings.summaryMode = true;

                    // set attribute headers (hidden ones here too)
                    $scope.attributeHeaders['Att1'] = $scope.groupsFactory.Att1Header;
                    $scope.attributeHeaders['Att2'] = $scope.groupsFactory.Att2Header;
                    $scope.attributeHeaders['Att3'] = $scope.groupsFactory.Att3Header;
                    $scope.attributeHeaders['Att4'] = $scope.groupsFactory.Att4Header;
                    $scope.attributeHeaders['Att5'] = $scope.groupsFactory.Att5Header;
                    $scope.attributeHeaders['Att6'] = $scope.groupsFactory.Att6Header;
                    $scope.attributeHeaders['Att7'] = $scope.groupsFactory.Att7Header;
                    $scope.attributeHeaders['Att8'] = $scope.groupsFactory.Att8Header;
                    $scope.attributeHeaders['Att9'] = $scope.groupsFactory.Att9Header;

                    // initialize sorting
                    for (var i = 0; i < $scope.groupsFactory.TestDueDates.length; i++) {
                        $scope.headerClassArray[i] = [];
                    }
                    $scope.sortMgr.initialize($scope.manualSortHeaders, $scope.sortArray, $scope.headerClassArray, $scope.groupsFactory.Fields);
                });
        }

        $scope.changeToChartMode = function () {
            $scope.settings.summaryView = 'Current Year Only (Sortable)';
            $scope.settings.summaryMode = false;
        }

        // generate graph if printmode
        if ($location.absUrl().indexOf('printmode=') >= 0) {
            var groupsParam = JSON.parse(decodeURIComponent($location.search().GroupsParam));

            var summaryDataParam = JSON.parse(decodeURIComponent($location.search().SummaryDataParam));
            $scope.settings.summaryMode = summaryDataParam.summaryMode;
            $scope.settings.summaryView = summaryDataParam.summaryView;
            $scope.settings.summaryCategory = summaryDataParam.summaryCategory;
            $scope.settings.summaryScoreGrouping = summaryDataParam.summaryScoreGrouping;
            $scope.settings.stacking = summaryDataParam.stacking;
            $scope.settings.stackingDescription = summaryDataParam.stackingDescription;

            // set selected items from deserialed groups and generate
            $scope.groupsFactory.Fields = groupsParam.Fields;
            $scope.groupsFactory.options.selectedAssessmentField = groupsParam.selectedAssessmentField;
            $scope.groupsFactory.options.selectedEducationLabels = groupsParam.selectedEducationLabels;
            $scope.groupsFactory.options.selectedSchoolYear = groupsParam.selectedSchoolYear;
            $scope.groupsFactory.options.selectedSchools = groupsParam.selectedSchools;
            $scope.groupsFactory.options.selectedTeachers = groupsParam.selectedTeachers;
            $scope.groupsFactory.options.selectedSections = groupsParam.selectedSections;
            $scope.groupsFactory.options.selectedStudents = groupsParam.selectedStudents;
            $scope.groupsFactory.options.selectedInterventionTypes = groupsParam.selectedInterventionTypes;
            $scope.groupsFactory.options.selectedGrades = groupsParam.selectedGrades;
            $scope.groupsFactory.options.selectedTestDueDate = groupsParam.selectedTestDueDate;
            $scope.groupsFactory.options.attributeTypes = groupsParam.attributeTypes;

            if ($scope.settings.summaryMode) {
                changeToSummaryMode($scope.settings.summaryCategory, $scope.settings.summaryScoreGrouping);
                $scope.settings.graphGenerated = true;
                $scope.settings.anyResults = true;
            } else {
                $scope.groupsFactory.loadGroupData()
                               .then(function (response) {
                                   $scope.groupsFactory.groupResults.data = response.data;
                                   updateDataFromServiceChange();
                                   $scope.settings.graphGenerated = true;
                               });
            }
        }

        function getTitle() {
            var title = '';

            title += '<b>Schools: </b>'
            if ($scope.filterOptions.selectedSchools.length > 0) {
                for (var i = 0; i < $scope.filterOptions.selectedSchools.length; i++) {
                    title += $scope.filterOptions.selectedSchools[i].text + ',';
                }
            } else {
                title += ' All '
            }

            title += '<b>Grades: </b>'
            if ($scope.filterOptions.selectedGrades.length > 0) {
                for (var i = 0; i < $scope.filterOptions.selectedGrades.length; i++) {
                    title += $scope.filterOptions.selectedGrades[i].text + ',';
                }
            } else {
                title += ' All '
            }

            return title;
        }

        function updateDataFromServiceChange() {
            $scope.settings.anyResults = false;
            // might need this
            $scope.studentResults = $scope.groupsFactory.groupResults.data;

            //return;
            var seriesArray = [];
            var categoriesArray = [];

            // set up series
            for (var i = 0; i < $scope.studentResults.length; i++) {
                $scope.settings.anyResults = true;
                var currentResult = $scope.studentResults[i];
                //var formattedDate = currentResult.DueDate == null ? $scope.groupsFactory.options.selectedSchoolYear.text : moment(currentResult.DueDate).format("DD-MMM-YYYY");
                //moment(cb.DueDate.substring(0, cb.DueDate.indexOf('T')));
                var formattedDate = currentResult.DueDate == null ? $scope.groupsFactory.options.selectedSchoolYear.text : moment(currentResult.DueDate.substring(0, currentResult.DueDate.indexOf('T'))).format("DD-MMM-YYYY");
                var foundCategory = $filter('filter')(categoriesArray, formattedDate, true);
                // see if category already exists, if not, add it
                if (!foundCategory.length) {
                    categoriesArray.push(formattedDate);//[categoriesArray.length] = { name: currentResult.DueDate, categories: [currentResult.DueDate] };
                }
                //labels: {rotation: -90}
                // create a data array for each scoregrouping
                // FIX THIS... need to be able to create an array of arrays with the index being the scoregrouping

                var groupingName = "";
                var groupingColor = "";

                if (currentResult.ScoreGrouping == 1) {
                    groupingName = "Exceeds Expectations";
                    groupingColor = "#4697ce";
                }
                if (currentResult.ScoreGrouping == 2) {
                    groupingName = "Meets Expectations";
                    groupingColor = "#90ED7D";
                }
                if (currentResult.ScoreGrouping == 3) {
                    groupingName = "Approaches Expectations";
                    groupingColor = "#E4D354";
                }
                if (currentResult.ScoreGrouping == 4) {
                    groupingName = "Does Not Meet Expectations";
                    groupingColor = "#BF453D";
                }

                if (seriesArray[currentResult.ScoreGrouping] == null) {
                    seriesArray[currentResult.ScoreGrouping] = { name: groupingName, color: groupingColor, data: [currentResult.NumberOfResults], id: currentResult.ScoreGrouping }
                }
                else {
                    seriesArray[currentResult.ScoreGrouping].data.push(currentResult.NumberOfResults);
                }
            }

            highchartsNgConfig = {
                //This is not a highcharts object. It just looks a little like one!
                options: {
                    //This is the Main Highcharts chart config. Any Highchart options are valid here.
                    //will be ovverriden by values specified below.
                    chart: {
                        type: 'column',
                        //options3d: {
                        //    enabled: true,
                        //    alpha: 15,
                        //    beta: 15,
                        //    viewDistance: 25,
                        //    depth: 40
                        //},
                        //marginTop: 80,
                        //marginRight: 40
                    },
                    tooltip: {
                        pointFormat: '<span style="color:{series.color}">\u25CF</span>  <span style="color:#666666">{series.name}</span>: <b>{point.y} Students</b> ({point.percentage:.0f}%)<br/>',
                        style: {
                            padding: 10,
                            fontWeight: 'bold'
                        }, useHTML: true
                    },
                    plotOptions: {
                        series: {
                            cursor: 'pointer',
                            point: {
                                events: {
                                    click: function (event) {
                                        var category = this.category;
                                        var scoreGrouping = this.series.userOptions.id;

                                        changeToSummaryMode(category, scoreGrouping);
                                    }
                                }
                            }
                        },
                        column: {
                            stacking: $scope.settings.stacking,
                            dataLabels: {
                                enabled: true,
                                color: (Highcharts.theme && Highcharts.theme.dataLabelsColor) || 'white',
                                style: {
                                    textShadow: '0 0 3px black'
                                },
                                formatter: function () {
                                    if (this.y > 0 && $scope.settings.stacking === 'normal')
                                        return this.y;
                                    if (this.y > 0 && $scope.settings.stacking === 'percent')
                                        return this.percentage.toFixed(0) + '%';
                                }
                            }
                        }
                    }
                },

                yAxis: {
                    allowDecimals: false,
                    min: 0,
                    title: {
                        text: $scope.settings.stackingDescription
                    },
                    stackLabels: {
                        enabled: true,
                        style: {
                            fontWeight: 'bold',
                            color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                        }
                    }
                },

                //The below properties are watched separately for changes.

                //Series object (optional) - a list of series using normal highcharts series options.
                series: seriesArray,
                //Title configuration (optional)
                title: {
                    text: 'Single Group Of Students Across Multiple Benchmark Dates'
                },
                //Boolean to control showng loading status on chart (optional)
                //Could be a string if you want to show specific loading text.
                loading: false,
                //Configuration for the xAxis (optional). Currently only one x axis can be dynamically controlled.
                //properties currentMin and currentMax provied 2-way binding to the chart's maximimum and minimum
                xAxis: {
                    categories: categoriesArray

                },
                //Whether to use HighStocks instead of HighCharts (optional). Defaults to false.
                useHighStocks: false,
                //size (optional) if left out the chart will default to size of the div or something sensible.
                //                size: {
                //                  width: 1000,
                //                height: 600
                //           },
                //function (optional)
                func: function (chart) {
                    //setup some logic for the chart
                }

            };

            $scope.chartConfig = highchartsNgConfig;
    }
                
    }



})();