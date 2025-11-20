(function () {
    'use strict';

    angular
        .module('stackedBarGraphModule', [])
        .controller('StackedBarGraphComparisonController', StackedBarGraphComparisonController)
        .controller('StackedBarGraphGroupsController', StackedBarGraphGroupsController)
        .directive('nsSummaryFieldHeaders', ['$compile', 'NSSummarySortManager', function ($compile, NSSummarySortManager) {
            function Controller($scope, $element) {
                var outputHtml = '';
                var sortText = '';
                    $scope.manualSortHeaders = {};
                    $scope.manualSortHeaders.firstNameHeaderClass = "fa";
                    $scope.manualSortHeaders.lastNameHeaderClass = "fa";
                    $scope.headerClassArray = [];

                    for (var i = 0; i < $scope.tdds.length; i++) {
                        $scope.headerClassArray[i] = [];
                    }
                    $scope.sortMgr = new NSSummarySortManager();

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

                    $scope.sortMgr.initialize($scope.manualSortHeaders, $scope.sortArray, $scope.headerClassArray, $scope.fields);
            }



            return {
                restrict: 'AE',
                scope: {
                    tdds: '=',
                    fields: '=',
                    dataRows: '=',
                    sortable: '=',
                    sortArray: '='
                },
                controller: Controller
            }
        }])
          .factory('NSSummarySortManager', [
        '$http', function ($http) {
            var NSSummarySortManager = function () {
                var self = this;
                self.manualSortHeaders = {};
                self.sortArray = [];
                self.headerClassArray = [];
                self.fieldResultName = '';
                self.fields = [];

                self.initialize = function (manualSortHeaders, sortArray, headerClassArray, fields) {
                    self.manualSortHeaders = manualSortHeaders;
                    self.sortArray = sortArray;
                    self.headerClassArray = headerClassArray;
                    self.fields = fields;

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
                                } else if (column === 'FirstName') {
                                    self.manualSortHeaders.firstNameHeaderClass = "fa";
                                } else if (column === 'LastName') {
                                    self.manualSortHeaders.lastNameHeaderClass = "fa";
                                }
                                self.sortArray.splice(j, 1);
                            } else {
                                if (tddIndex > -1) {
                                    self.headerClassArray[tddIndex][fieldIndex] = "fa fa-chevron-down";
                                } else if (column === 'FirstName') {
                                    self.manualSortHeaders.firstNameHeaderClass = "fa fa-chevron-down";
                                } else if (column === 'LastName') {
                                    self.manualSortHeaders.lastNameHeaderClass = "fa fa-chevron-down";
                                }
                                self.sortArray[j] = "-" + self.sortArray[j];
                            }
                            break;
                        }
                    }
                    if (!bFound) {
                        self.sortArray.push(column);

                        if (tddIndex > -1) {
                            self.headerClassArray[tddIndex][fieldIndex] = "fa fa-chevron-up";
                        } else if (column === 'FirstName') {
                            self.manualSortHeaders.firstNameHeaderClass = "fa fa-chevron-up";
                        } else if (column === 'LastName') {
                            self.manualSortHeaders.lastNameHeaderClass = "fa fa-chevron-up";
                        }
                    }
                };
            };

            return NSSummarySortManager;
        }])

        .directive('nsSummaryField', ['$compile','$filter', function ($compile, $filter) {
            return {
                restrict: 'AE',
                scope: {
                    tdds: '=',
                    fields: '=',
                    studentResult: '='
                },
                link: function (scope, element, attr) {
                    var outputHtml = '';

                    function init() {
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
                        //element.append(outputHtml);
                        //$compile(element.contents())(scope);
                    }

                    init();
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

		                if (selectedData.length > 0) {
		                    for (var i = 0; i < selectedData.length; i++) {
		                        result += selectedData[i].text + ', ';
		                    }
		                    result = result.substring(0, result.length - 2);
		                } else {
		                    result = 'All ' + attributeName;
		                }

		                return result;
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
		'$routeParams', '$compile', '$templateCache', '$http', '$filter', function ($routeParams, $compile, $templateCache, $http, $filter) {

		    function Controller($scope) {
		        $scope.filterOptions = $scope.groupFactory.options;


		        $scope.changeSchools = function (newVals, oldVals) {
		            $scope.groupFactory.loadSchoolChange(newVals, oldVals);
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
		        $scope.changeSections = function () {
		            $scope.groupFactory.loadSectionChange();
		        };
                 
		        $scope.select2SchoolOptions = {
		            minimumInputLength: 0,
		            data: $scope.filterOptions.schools,
		            multiple: true,
		            width: 'resolve',
		        };

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

		        $scope.select2SectionOptions = {
		            minimumInputLength: 0,
		            data: $scope.filterOptions.sections,
		            multiple: true,
		            width: 'resolve',
		        };

		        $scope.select2InterventionTypeOptions = {
		            minimumInputLength: 0,
		            data: $scope.filterOptions.interventionTypes,
		            multiple: true,
		            width: 'resolve',
		        };
		    }

		    return {
		        scope: {
		            groupFactory: '=',
		            tddEnabled: '=',
                    groupName: '='
		        },
			    restrict: 'E',
			    templateUrl: 'templates/stacked-bar-graph-group-options.html',
                controller: Controller
			};
		}
        ])
        .factory('nsStackedBarGraphOptionsFactory', [
			'$http', '$routeParams', '$filter', 'nsLookupFieldService', 'webApiBaseUrl', function ($http, $routeParams, $filter, nsLookupFieldService, webApiBaseUrl) {
			    var nsStackedBarGraphOptionsFactory = function (name, tddEnabled) {

			        var self = this;
			        self.tddEnabled = tddEnabled;
			        self.name = name;
			        self.LookupFieldsArray = nsLookupFieldService.LookupFieldsArray;

			        self.options = {};
			        self.groupResults = {};
			        self.Scores = [];
			        self.TestDueDates = [];
			        self.StudentRecords = [];

			        self.options.selectedAssessmentField = {};
			        self.options.attributeTypes = [];
			        self.options.teachers = [];
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

			        self.options.selectedGrades = [];
			        self.options.selectedTeachers = [];
			        self.options.selectedSections = [];
			        self.options.selectedSchools = [];
			        self.options.selectedEthnicities = [];
			        self.options.selectedInterventionTypes = [];
			        self.options.selectedTitleOneTypes = [];
			        self.options.selectedEducationLabels = [];
			        self.options.selectedSchoolYear = {}; // TODO: GetDefaultYear
			        self.options.selectedTestDueDate = {};
			        self.options.selectedADSIS = false;
			        self.options.selectedELL = false;
			        self.options.selectedGifted = false;
			        self.options.selectedGender = {};
			        //this.options.selected

			        self.LoadAssessmentFields = function () {

			            var url = webApiBaseUrl + '/api/benchmark/GetAssessmentsAndFields';
			            var promise = $http.get(url);

			            return promise.then(function (response) {
			                self.options.assessments = self.flatten(response.data.Assessments);
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
			                        DatabaseColumn: v.DatabaseColumn
			                    })
			                })
			            })
			            return out;
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
			                //TitleOneTypes: self.options.selectedTitleOneTypes,
			                //Ethnicities: self.options.selectedEthnicities,
			                SchoolStartYear: self.options.selectedSchoolYear.id,
			                DropdownDataList: selectedAttributes,
			                AssessmentField: self.options.selectedAssessmentField,
			                TestDueDateID: self.options.selectedTestDueDate.id,
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
			                SchoolStartYear: self.options.selectedSchoolYear.id,
			                ScoreGrouping: angular.isDefined(scoreGrouping) ? scoreGrouping : $routeParams.scoreGrouping,
			                TestDueDate: (angular.isDefined(comparisionTdd) && comparisionTdd != null) ? moment(comparisionTdd).format('MM/DD/YYYY') : tdd,
			                AssessmentField: self.options.selectedAssessmentField,
			                DropdownDataList: selectedAttributes
			            };

			            if (isHistorical) {
			                return $http.post(webApiBaseUrl + "/api/stackedbargraph/GetStackedBarGraphGroupHistoricalSummary", returnObject).then(function (response) {
			                    self.TestDueDates = response.data.TestDueDates;
			                    self.HistoricalSummaryRecords = response.data.SummaryRecords;
			                    self.Fields = response.data.Fields;
			                    self.HistoricalSummaryDataPostProcess();
			                });
			            } else {
			                return $http.post(webApiBaseUrl + "/api/stackedbargraph/GetStackedBarGraphGroupSummary", returnObject).then(function (response) {
			                    self.TestDueDates = response.data.TestDueDates;
			                    self.SummaryRecords = response.data.SummaryRecords;
			                    self.Fields = response.data.Fields;
			                    self.SummaryDataPostProcess();
			                });
			            }
			        }

			        self.SummaryDataPostProcess = function () {
			            for (var j = 0; j < self.SummaryRecords.length; j++) {
			                for (var i = 0; i < self.SummaryRecords[j].ResultsByTDD.length; i++) {
			                    for (var k = 0; k < self.SummaryRecords[j].ResultsByTDD[i].FieldResults.length; k++) {
			                        for (var r = 0; r < self.Fields.length; r++) {
			                            if (self.Fields[r].DatabaseColumn == self.SummaryRecords[j].ResultsByTDD[i].FieldResults[k].DbColumn) {
			                                self.SummaryRecords[j].ResultsByTDD[i].FieldResults[k].Field = angular.copy(self.Fields[r]);

			                                if (self.Fields[r].FieldType === "DropdownFromDB") {
			                                    for (var p = 0; p < self.LookupFieldsArray.length; p++) {
			                                        if (self.LookupFieldsArray[p].LookupColumnName === self.Fields[r].LookupFieldName) {
			                                            // now find the specifc value that matches
			                                            for (var y = 0; y < self.LookupFieldsArray[p].LookupFields.length; y++) {
			                                                if (self.SummaryRecords[j].ResultsByTDD[i].FieldResults[k].IntValue === self.LookupFieldsArray[p].LookupFields[y].FieldSpecificId) {
			                                                    self.SummaryRecords[j].ResultsByTDD[i].FieldResults[k].DisplayValue = self.LookupFieldsArray[p].LookupFields[y].FieldValue;
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

			                                if (self.Fields[r].FieldType === "DropdownFromDB") {
			                                    for (var p = 0; p < self.LookupFieldsArray.length; p++) {
			                                        if (self.LookupFieldsArray[p].LookupColumnName === self.Fields[r].LookupFieldName) {
			                                            // now find the specifc value that matches
			                                            for (var y = 0; y < self.LookupFieldsArray[p].LookupFields.length; y++) {
			                                                if (self.HistoricalSummaryRecords[j].ResultsByTDD[i].FieldResults[k].IntValue === self.LookupFieldsArray[p].LookupFields[y].FieldSpecificId) {
			                                                    self.HistoricalSummaryRecords[j].ResultsByTDD[i].FieldResults[k].DisplayValue = self.LookupFieldsArray[p].LookupFields[y].FieldValue;
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
			        }

			        self.loadOptions = function () {
			            var returnObject = { ChangeType: 'initial' };


			            $http.post(webApiBaseUrl + "/api/stackedbargraph/GetStackedBarGraphGroupingUpdatedOptions", returnObject).then(function (response) {
			                //self.options.schools.splice(1, 0);
			                self.options.schools.push.apply(self.options.schools, response.data.Schools);
			                self.options.schoolYears.push.apply(self.options.schoolYears, response.data.SchoolYears);
			                self.options.educationLabels.push.apply(self.options.educationLabels, response.data.EducationLabels);
			                self.options.titleOneTypes.push.apply(self.options.titleOneTypes, response.data.TitleOneTypes);
			                self.options.interventionTypes.push.apply(self.options.interventionTypes, response.data.InterventionTypes);
			                self.options.ethnicities.push.apply(self.options.ethnicities, response.data.Ethnicities);
			                self.options.genders.push.apply(self.options.genders, response.data.Genders);
			                self.options.attributeTypes.push.apply(self.options.attributeTypes, response.data.DropdownDataList);
			                self.options.testDueDates.push.apply(self.options.testDueDates, response.data.TestDueDates);

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

                            // TODO: set a reasonable default TDD (should come from response.data.SelectedTDD!!!)
                            if (self.options.testDueDates.length > 0 && self.tddEnabled) {
			                    self.options.selectedTestDueDate = self.options.testDueDates[0];
			                }


			                //options.schoolYears.splice(1, 0, data.SchoolYears);
			                //options.schoolYears.push.apply(options.schoolYears, data.SchoolYears);
			                //options.benchmarkDates.splice(1, 0, data.TestDueDates);
			                //tions.benchmarkDates.push.apply(options.benchmarkDates, data.TestDueDates);
			            });
			        }
			        self.loadOptions();
			        self.LoadAssessmentFields();

			        //this.initialLoad = function()
			        //{
			        //    this.loadOptions(this.options);
			        //}

			        self.loadSchoolChange = function (newSelections, oldSelections) {

			            if (angular.equals(newSelections, oldSelections)) {
			                return;
			            }

			            var returnObject = { ChangeType: 'school', Schools: self.options.selectedSchools, SchoolStartYear: self.options.selectedSchoolYear.id };

			            $http.post(webApiBaseUrl + "/api/stackedbargraph/GetStackedBarGraphGroupingUpdatedOptions", returnObject).success(function (data) {
			                self.options.grades.length = 0;
			                self.options.grades.push.apply(self.options.grades, data.Grades);
			                self.options.teachers.length = 0;;
			                self.options.teachers.push.apply(self.options.teachers, data.Teachers);
			                self.options.sections.length = 0;
			                self.options.students.length = 0;
			                self.options.selectedGrades = [];
			                self.options.selectedSections = [];
			                self.options.selectedStudents = [];
			                self.options.selectedTeachers = [];
			            });
			        }
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
			                self.options.selectedGrades = [];

			                self.options.sections.length = 0;
			                self.options.selectedSections = [];

			                self.options.teachers.length = 0;
			                self.options.selectedTeachers = [];
			            });
			        }
			    }
			    return nsStackedBarGraphOptionsFactory;
			

			}]
	)
        ;

    StackedBarGraphComparisonController.$inject = ['$scope', '$q', '$http', 'pinesNotifications', '$location', '$filter', 'nsStackedBarGraphOptionsFactory', '$routeParams'];


    function StackedBarGraphComparisonController($scope, $q, $http, pinesNotifications, $location, $filter, nsStackedBarGraphOptionsFactory, $routeParams) {
        $scope.settings = {};
        $scope.settings.summaryMode = false;
        $scope.settings.summaryView = 'Current Year Only (Sortable)';
        $scope.settings.summaryCategory = '';
        $scope.settings.summaryScoreGrouping = 1;
        $scope.settings.stacking = 'normal';
        $scope.settings.stackingDescription = 'Number of Students';
        $scope.settings.graphGenerated = false;

        $scope.sortArray = [];

        var highchartsNgConfig = {};
        $scope.groupsFactory = {};

        $scope.comparisonGroups = [];
        $scope.comparisonGroups.push(new nsStackedBarGraphOptionsFactory("Group 1", true))
        $scope.filterOptions = $scope.comparisonGroups[0].options;
        //$scope.groupResults = nsStackedBarGraphGroupsOptionsService.groupResults;

        $scope.addNewGroup = function () {
            $scope.comparisonGroups.push(new nsStackedBarGraphOptionsFactory("Group " + ($scope.comparisonGroups.length + 1), true));
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

        $scope.generateGraph = function () {

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
                    studentResultsCollection.push(response[j].data);
                    //groupNameCollection.push()
                }

                updateDataFromServiceChange(studentResultsCollection);
            });

            $scope.settings.graphGenerated = true;
        };

        $scope.$watch('settings.summaryView', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                changeToSummaryMode($scope.settings.summaryCategory, $scope.settings.summaryScoreGrouping);
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

        var changeToSummaryMode = function (category, scoreGrouping) {
            $scope.settings.summaryCategory = category;
            $scope.settings.summaryScoreGrouping = scoreGrouping;

            // find proper factory based on category (groupName)
            for (var i = 0; i < $scope.comparisonGroups.length;i++) {
                if ($scope.comparisonGroups[i].name == category) {
                    $scope.groupsFactory = $scope.comparisonGroups[i];
                    $scope.groupsFactory.loadSummaryData(scoreGrouping, category, $scope.groupsFactory.options.selectedTestDueDate.text, ($scope.settings.summaryView !== 'Current Year Only (Sortable)')).then(function (response) {
                        $scope.settings.summaryMode = true;
                    });
                    break;
                }
            }
        }

        $scope.changeToChartMode = function () {
            $scope.settings.summaryMode = false;
            $scope.settings.summaryCategory = '';
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

            //return;
            var seriesArray = [];
            var categoriesArray = [];

            // set up series
            for (var i = 0; i < studentResultsCollection.length; i++) {
                var currentResult = studentResultsCollection[i];

                var foundCategory = $filter('filter')(categoriesArray, currentResult.GroupName, true);
                // see if category already exists, if not, add it
                if (!foundCategory.length) {
                    categoriesArray.push(currentResult.GroupName);//[categoriesArray.length] = { name: currentResult.DueDate, categories: [currentResult.DueDate] };
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
                        }
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
                    text: 'Compare Student Groups'
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

    StackedBarGraphGroupsController.$inject = ['$scope', '$q', '$http', 'pinesNotifications', '$location', '$filter', 'nsStackedBarGraphOptionsFactory', '$routeParams'];


    function StackedBarGraphGroupsController($scope, $q, $http, pinesNotifications, $location, $filter, nsStackedBarGraphOptionsFactory, $routeParams) {
        $scope.settings = {};
        $scope.settings.summaryMode = false;
        $scope.settings.summaryView = 'Current Year Only (Sortable)';
        $scope.settings.summaryCategory = '';
        $scope.settings.summaryScoreGrouping = 1;
        $scope.settings.stacking = 'normal';
        $scope.settings.stackingDescription = 'Number of Students';
        $scope.settings.graphGenerated = false;

        $scope.sortArray = [];

        var highchartsNgConfig = {};
        $scope.groupsFactory = new nsStackedBarGraphOptionsFactory('Compare Group Across Benchmark Dates', false);
        $scope.filterOptions = $scope.groupsFactory.options;
        $scope.groupResults = $scope.groupsFactory.groupResults;

        $scope.generateGraph = function () {
            $scope.groupsFactory.loadGroupData().then(function (response) {
                $scope.groupsFactory.groupResults.data = response.data;
                updateDataFromServiceChange();
            });
            $scope.settings.graphGenerated = true;
        }; 

        $scope.$watch('settings.summaryView', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                changeToSummaryMode($scope.settings.summaryCategory, $scope.settings.summaryScoreGrouping);
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

            $scope.groupsFactory.loadSummaryData(scoreGrouping, category, null, ($scope.settings.summaryView !== 'Current Year Only (Sortable)')).then(function (response) {
                $scope.settings.summaryMode = true;
            });            
        }

        $scope.changeToChartMode = function () {
            $scope.settings.summaryMode = false;
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
            // might need this
            $scope.studentResults = $scope.groupsFactory.groupResults.data;

            //return;
            var seriesArray = [];
            var categoriesArray = [];

            // set up series
            for (var i = 0; i < $scope.studentResults.length; i++) {
                var currentResult = $scope.studentResults[i];
                var formattedDate = moment(currentResult.DueDate).format("DD-MMM-YYYY")

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
                        }
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