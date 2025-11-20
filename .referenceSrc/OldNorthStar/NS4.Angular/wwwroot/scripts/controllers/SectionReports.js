(function () {
    'use strict';

    angular
        .module('sectionReportsModule', [])
            .factory('LIDReportManager', [
        '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
            var LIDReportManager = function () {
                var self = this;
                self.assessment = {};
                self.studentSectionReportResults = [];
                self.tdds = [];
                self.HeaderFields = [];
                self.headerClassArray = [];
                self.sortArray = [];

                self.LoadData = function (assessmentId, sectionId, reportType, schoolYear) {
                    var paramObj = { AssessmentId: assessmentId, SectionId: sectionId, ReportType: reportType, SchoolYear: schoolYear };

                    return $http.post(webApiBaseUrl + '/api/sectionreport/GetLIDSectionReport', paramObj)
                        .then(
                            function (response) {
                                self.assessment = response.data.Assessment;
                                self.studentSectionReportResults = response.data.StudentSectionReportResults;
                                self.tdds = response.data.TestDueDates;

                                self.HeaderFields = response.data.HeaderFields;

                                var currentCssClass = '';
                                var currentSubCatId = 0;
                                for (var i = 0; i < self.HeaderFields.length; i++) {
                                    if (self.HeaderFields[i].SubcategoryId != currentSubCatId) {
                                        self.HeaderFields[i].cssClass = 'leftDoubleBorder';
                                    }
                                    else if (i === self.HeaderFields.length - 1) {
                                        self.HeaderFields[i].cssClass = 'rightDoubleBorder';
                                    }
                                    currentSubCatId = self.HeaderFields[i].SubcategoryId;
                                }

                                for (var r = 0; r < self.HeaderFields.length; r++) {
                                    self.headerClassArray[r] = 'fa';
                                }
                            }
                    );
                };
                self.sort = function (column) {

                    var columnIndex = -1;
                    // if this is not a first or lastname column
                    if (!isNaN(parseInt(column))) {
                        columnIndex = column;

                        column = 'SummaryFieldResults[' + column + '].CellColorDate';

                    }
                    var bFound = false;
                    for (var j = 0; j < self.sortArray.length; j++) {
                        // if it is already on the list, reverse the sort
                        if (self.sortArray[j].indexOf(column) >= 0) {
                            bFound = true;

                            // is it already negative? if so, remove it
                            if (self.sortArray[j].indexOf("-") === 0) {
                                if (columnIndex > -1) {
                                    self.headerClassArray[columnIndex] = "fa";
                                } else if (column === 'LastName') {
                                    self.nameHeaderClass = "fa";
                                }
                                self.sortArray.splice(j, 1);
                            } else {
                                if (columnIndex > -1) {
                                    self.headerClassArray[columnIndex] = "fa fa-chevron-down";
                                } else if (column === 'LastName') {
                                    self.nameHeaderClass = "fa fa-chevron-down";
                                }
                                self.sortArray[j] = "-" + self.sortArray[j];
                            }
                            break;
                        }
                    }
                    if (!bFound) {
                        self.sortArray.push(column);

                        if (columnIndex > -1) {
                            self.headerClassArray[columnIndex] = "fa fa-chevron-up";
                        } else if (column === 'LastName') {
                            self.nameHeaderClass = "fa fa-chevron-up";
                        }
                    }
                };



                self.subCategoryColSpan = function (subCategoryId) {
                    var colSpan = 0;

                    for (var i = 0; i < self.HeaderFields.length; i++) {
                        if (self.HeaderFields[i].SubcategoryId == subCategoryId) {
                            colSpan++;
                        }
                    }

                    return colSpan;
                };
            };

            return (LIDReportManager);
        }
            ])
                .factory('FPReportManager', [
        '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
            var FPReportManager = function () {
                var self = this;
                self.assessment = {};
                self.studentSectionReportResults = [];
                self.tdds = [];
                self.scale = [];

                self.LoadData = function (assessmentId, sectionId, schoolYear) {
                    var paramObj = { AssessmentId: assessmentId, SectionId: sectionId, SchoolYear: schoolYear };

                    return $http.post(webApiBaseUrl + '/api/sectionreport/GetFPSectionReport', paramObj)
                        .then(
                            function (response) {
                                self.assessment = response.data.Assessment;
                                self.studentSectionReportResults = response.data.StudentSectionReportResults;
                                self.tdds = response.data.TestDueDates;
                                self.scale = response.data.Scale;
                                self.eoyBenchmark = response.data.EndOfYearBenchmark;
                                self.soyBenchmark = response.data.StartOfYearBenchmark;
                                self.targetZone = response.data.TargetZone;
                                self.interventionRecords = response.data.InterventionRecords;
                                self.previousGradeScores = response.data.PreviousGradeScores;
                                self.studentServices = response.data.StudentServices;
                                self.benchmarksByGrade = response.data.BenchmarksByGrade;
                            }
                    );
                };


            };

            return (FPReportManager);
        }
                ])
         .factory('KNTCReportManager', [
        '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
            var KNTCReportManager = function () {
                var self = this;
                self.assessment = {};
                self.studentSectionReportResults = [];
                self.tdds = [];
                self.scale = [];

                self.LoadData = function (assessmentId, sectionId, schoolYear, benchmarkDateId) {
                    var paramObj = { AssessmentId: assessmentId, SectionId: sectionId, SchoolYear: schoolYear, BenchmarkDateId: benchmarkDateId };

                    return $http.post(webApiBaseUrl + '/api/sectionreport/GetKNTCSectionReport', paramObj)
                        .then(
                            function (response) {
                                self.assessment = response.data.Assessment;
                                self.studentSectionReportResults = response.data.StudentSectionReportResults;
                                self.studentFPSectionReportResults = response.data.StudentFPSectionReportResults;
                                self.interventionRecords = response.data.InterventionRecords;
                                self.studentServices = response.data.StudentServices;
                                self.benchmarksByGrade = response.data.BenchmarksByGrade;
                                self.HeaderFields = response.data.HeaderFields;
                                self.StudentAttributes = response.data.StudentAttributes;
                                self.MCAResults = response.data.MCAResults;
                            }
                    );
                };


            };

            return (KNTCReportManager);
        }
         ])
        .factory('AVMRDetailReportManager', [
        '$http', 'webApiBaseUrl', '$filter', function ($http, webApiBaseUrl, $filter) {
            var KNTCReportManager = function () {
                var self = this;
                self.detailAssessment = {};
                self.studentDetailResults = [];
                self.detailHeaderFields = [];
                self.altDisplayLabel = '';
                self.groupContainer = {};
                self.loaded = false;
                self.categories = [];

                function getArrayRef(ary, item) {
                    if (ary == null || item == null) {
                        return null;
                    }
                    for (var i = 0; i < ary.length; i++) {
                        if (ary[i].Id == item.Id) {
                            return ary[i];
                        }
                    }
                    return null;
                }

                function getArrayItemById(ary, id) {
                    if (ary == null || id == null) {
                        return null;
                    }
                    for (var i = 0; i < ary.length; i++) {
                        if (ary[i].Id == id) {
                            return ary[i];
                        }
                    }
                    return null;
                }

                self.LoadData = function (assessmentId, selectedSectionId, selectedBenchmarkDateId) {
                    var postObject = { AssessmentId: assessmentId, SectionId: selectedSectionId, BenchmarkDateId: selectedBenchmarkDateId };
                    var url = webApiBaseUrl + '/api/sectionreport/GetAVMRSingleDateSectionReportDetail';
                    return $http.post(url, postObject)
                        .then(
                            function (response) {
                                self.detailAssessment = response.data.Assessment;
                                self.studentDetailResults = response.data.StudentSectionReportResults;
                                self.detailHeaderFields = response.data.HeaderFields;
                                self.loaded = true;
                            }
                    );
                };
                self.FilterData = function (groupId, altDisplayLabel) {
                    self.categories = [];
                    // filter out and only get the categories for the group we care about
                    var fieldGroup = getArrayItemById(self.detailAssessment.FieldGroups, groupId);

                    var fieldsInGroup = $filter('filter')(self.detailAssessment.Fields, { GroupId: fieldGroup.Id, FieldType: '!label' });

                    self.altDisplayLabel = altDisplayLabel;
                    self.subCategory = getArrayItemById(self.detailAssessment.FieldSubCategories, fieldsInGroup[0].SubcategoryId);
                    self.groupContainer = fieldGroup.Container;

                    // get the unique categories we need for this fieldgroup
                    for (var b = 0; b < fieldsInGroup.length; b++) {
                        var currentField = fieldsInGroup[b];

                        var categoryForField = getArrayItemById(self.detailAssessment.FieldCategories, currentField.CategoryId);
                        var categoryRef = getArrayRef(self.categories, categoryForField);
                        if (categoryRef == null) {
                            categoryRef = angular.copy(categoryForField);
                            self.categories.push(categoryRef);
                        }
                    }

                    // get only the fieldresults that apply to this fieldgroup's fields
                    for (var i = 0; i < self.studentDetailResults.length; i++) {
                        var currentStudent = self.studentDetailResults[i];

                        // get only the results that apply to this group
                        currentStudent.GroupFieldResults = $filter('filter')(currentStudent.FieldResults, { GroupId: fieldGroup.Id });
                        // attach field to each result so that we can filter by category
                        // TODO: probably would be easier to just add categoryid to the fieldresult
                        for (var k = 0; k < currentStudent.GroupFieldResults.length; k++) {
                            currentStudent.GroupFieldResults[k].Field = getArrayItemById(self.detailAssessment.Fields, currentStudent.GroupFieldResults[k].FieldId);
                        }
                    }
                }
            };

            return (KNTCReportManager);
        }
                 ])
        .factory('SpellReportManager', [
        '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
            var SpellReportManager = function () {
                var self = this;
  

                self.LoadData = function (assessmentId, sectionId, benchmarkDateId) {
                    var paramObj = {
                        AssessmentId: assessmentId,
                        SectionId: sectionId,
                        BenchmarkDateId: benchmarkDateId
                    }

                    return $http.post(webApiBaseUrl + '/api/sectionreport/GetSpellingInventorySectionReport', paramObj)
                        .then(function (response) {
                            self.assessment = response.data.Assessment;
                            self.studentResults = response.data.StudentResults;
                            self.HeaderFields = response.data.HeaderFields;

                        // assign fields to fieldResults... for the love of god, optimize this
                            for (var j = 0; j < self.studentResults.length; j++) {
                                for (var k = 0; k < self.studentResults[j].FieldResults.length; k++) {
                                    for (var i = 0; i < self.HeaderFields.length; i++) {
                                        if (self.HeaderFields[i].DatabaseColumn === self.studentResults[j].FieldResults[k].DbColumn) {
                                            self.studentResults[j].FieldResults[k].Field = self.HeaderFields[i];
                                            self.studentResults[j].FieldResults[k].Field.OutOfHowMany = self.HeaderFields[i].OutOfHowMany;
                                    }
                                }
                            }
                        }

                    });

                };


            };

            return (SpellReportManager);
        }
                        ])
                    .factory('HRSIWReportManager', [
        '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
            var HRSIWReportManager = function () {
                var self = this;
                self.assessment = {};
                self.studentSectionReportResults = [];
                self.tdds = [];
                self.HeaderFields = [];
                self.headerClassArray = [];
                self.sortArray = [];

                self.LoadData = function (assessmentId, sectionId, reportType, schoolYear, hrsFormId) {
                    var paramObj = { AssessmentId: assessmentId, SectionId: sectionId, ReportType: reportType, SchoolYear: schoolYear, HRSFormId: hrsFormId };

                    return $http.post(webApiBaseUrl + '/api/sectionreport/GetHRSIWSectionReport', paramObj)
                        .then(
                            function (response) {
                                self.assessment = response.data.Assessment;
                                self.studentSectionReportResults = response.data.StudentSectionReportResults;
                                self.tdds = response.data.TestDueDates;

                                self.HeaderFields = response.data.HeaderFields;

                                var currentCssClass = '';
                                var currentSubCatId = 0;
                             
                                for (var r = 0; r < self.HeaderFields.length; r++) {
                                    self.headerClassArray[r] = 'fa';
                                }
                            }
                    );
                };
                self.sort = function (column) {

                    var columnIndex = -1;
                    // if this is not a first or lastname column
                    if (!isNaN(parseInt(column))) {
                        columnIndex = column;

                        column = 'SummaryFieldResults[' + column + '].CellColorDate';

                    }
                    var bFound = false;
                    for (var j = 0; j < self.sortArray.length; j++) {
                        // if it is already on the list, reverse the sort
                        if (self.sortArray[j].indexOf(column) >= 0) {
                            bFound = true;

                            // is it already negative? if so, remove it
                            if (self.sortArray[j].indexOf("-") === 0) {
                                if (columnIndex > -1) {
                                    self.headerClassArray[columnIndex] = "fa";
                                } else if (column === 'LastName') {
                                    self.nameHeaderClass = "fa";
                                }
                                self.sortArray.splice(j, 1);
                            } else {
                                if (columnIndex > -1) {
                                    self.headerClassArray[columnIndex] = "fa fa-chevron-down";
                                } else if (column === 'LastName') {
                                    self.nameHeaderClass = "fa fa-chevron-down";
                                }
                                self.sortArray[j] = "-" + self.sortArray[j];
                            }
                            break;
                        }
                    }
                    if (!bFound) {
                        self.sortArray.push(column);

                        if (columnIndex > -1) {
                            self.headerClassArray[columnIndex] = "fa fa-chevron-up";
                        } else if (column === 'LastName') {
                            self.nameHeaderClass = "fa fa-chevron-up";
                        }
                    }
                };

                self.getFormForClass = function (sectionId) {
                    var paramObj = { Id: sectionId };
                    return $http.post(webApiBaseUrl + '/api/sectionreport/GetHRSIWFormForClass', paramObj);
                }

                self.subCategoryColSpan = function (subCategoryId) {
                    var colSpan = 0;

                    for (var i = 0; i < self.HeaderFields.length; i++) {
                        if (self.HeaderFields[i].SubcategoryId == subCategoryId) {
                            colSpan++;
                        }
                    }

                    return colSpan;
                };
            };

            return (HRSIWReportManager);
        }
                    ])
        .factory('HRSIW2ReportManager', [
        '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
            var HRSIW2ReportManager = function () {
                var self = this;
                self.assessment = {};
                self.studentSectionReportResults = [];
                self.tdds = [];
                self.HeaderFields = [];
                self.headerClassArray = [];
                self.sortArray = [];

                self.LoadData = function (assessmentId, sectionId, reportType, schoolYear, hrsFormId) {
                    var paramObj = { AssessmentId: assessmentId, SectionId: sectionId, ReportType: reportType, SchoolYear: schoolYear, HRSFormId2: hrsFormId };

                    return $http.post(webApiBaseUrl + '/api/sectionreport/GetHRSIW2SectionReport', paramObj)
                        .then(
                            function (response) {
                                self.assessment = response.data.Assessment;
                                self.studentSectionReportResults = response.data.StudentSectionReportResults;
                                self.tdds = response.data.TestDueDates;

                                self.HeaderFields = response.data.HeaderFields;

                                var currentCssClass = '';
                                var currentSubCatId = 0;

                                for (var r = 0; r < self.HeaderFields.length; r++) {
                                    self.headerClassArray[r] = 'fa';
                                }
                            }
                    );
                };
                self.sort = function (column) {

                    var columnIndex = -1;
                    // if this is not a first or lastname column
                    if (!isNaN(parseInt(column))) {
                        columnIndex = column;

                        column = 'SummaryFieldResults[' + column + '].CellColorDate';

                    }
                    var bFound = false;
                    for (var j = 0; j < self.sortArray.length; j++) {
                        // if it is already on the list, reverse the sort
                        if (self.sortArray[j].indexOf(column) >= 0) {
                            bFound = true;

                            // is it already negative? if so, remove it
                            if (self.sortArray[j].indexOf("-") === 0) {
                                if (columnIndex > -1) {
                                    self.headerClassArray[columnIndex] = "fa";
                                } else if (column === 'LastName') {
                                    self.nameHeaderClass = "fa";
                                }
                                self.sortArray.splice(j, 1);
                            } else {
                                if (columnIndex > -1) {
                                    self.headerClassArray[columnIndex] = "fa fa-chevron-down";
                                } else if (column === 'LastName') {
                                    self.nameHeaderClass = "fa fa-chevron-down";
                                }
                                self.sortArray[j] = "-" + self.sortArray[j];
                            }
                            break;
                        }
                    }
                    if (!bFound) {
                        self.sortArray.push(column);

                        if (columnIndex > -1) {
                            self.headerClassArray[columnIndex] = "fa fa-chevron-up";
                        } else if (column === 'LastName') {
                            self.nameHeaderClass = "fa fa-chevron-up";
                        }
                    }
                };

                self.getFormForClass = function (sectionId) {
                    var paramObj = { Id: sectionId };
                    return $http.post(webApiBaseUrl + '/api/sectionreport/GetHRSIW2FormForClass', paramObj);
                }

                self.subCategoryColSpan = function (subCategoryId) {
                    var colSpan = 0;

                    for (var i = 0; i < self.HeaderFields.length; i++) {
                        if (self.HeaderFields[i].SubcategoryId == subCategoryId) {
                            colSpan++;
                        }
                    }

                    return colSpan;
                };
            };

            return (HRSIW2ReportManager);
        }
        ])
        .factory('HRSIW3ReportManager', [
        '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
            var HRSIW3ReportManager = function () {
                var self = this;
                self.assessment = {};
                self.studentSectionReportResults = [];
                self.tdds = [];
                self.HeaderFields = [];
                self.headerClassArray = [];
                self.sortArray = [];

                self.LoadData = function (assessmentId, sectionId, reportType, schoolYear, hrsFormId) {
                    var paramObj = { AssessmentId: assessmentId, SectionId: sectionId, ReportType: reportType, SchoolYear: schoolYear, HRSFormId3: hrsFormId };

                    return $http.post(webApiBaseUrl + '/api/sectionreport/GetHRSIW3SectionReport', paramObj)
                        .then(
                            function (response) {
                                self.assessment = response.data.Assessment;
                                self.studentSectionReportResults = response.data.StudentSectionReportResults;
                                self.tdds = response.data.TestDueDates;

                                self.HeaderFields = response.data.HeaderFields;

                                var currentCssClass = '';
                                var currentSubCatId = 0;

                                for (var r = 0; r < self.HeaderFields.length; r++) {
                                    self.headerClassArray[r] = 'fa';
                                }
                            }
                    );
                };
                self.sort = function (column) {

                    var columnIndex = -1;
                    // if this is not a first or lastname column
                    if (!isNaN(parseInt(column))) {
                        columnIndex = column;

                        column = 'SummaryFieldResults[' + column + '].CellColorDate';

                    }
                    var bFound = false;
                    for (var j = 0; j < self.sortArray.length; j++) {
                        // if it is already on the list, reverse the sort
                        if (self.sortArray[j].indexOf(column) >= 0) {
                            bFound = true;

                            // is it already negative? if so, remove it
                            if (self.sortArray[j].indexOf("-") === 0) {
                                if (columnIndex > -1) {
                                    self.headerClassArray[columnIndex] = "fa";
                                } else if (column === 'LastName') {
                                    self.nameHeaderClass = "fa";
                                }
                                self.sortArray.splice(j, 1);
                            } else {
                                if (columnIndex > -1) {
                                    self.headerClassArray[columnIndex] = "fa fa-chevron-down";
                                } else if (column === 'LastName') {
                                    self.nameHeaderClass = "fa fa-chevron-down";
                                }
                                self.sortArray[j] = "-" + self.sortArray[j];
                            }
                            break;
                        }
                    }
                    if (!bFound) {
                        self.sortArray.push(column);

                        if (columnIndex > -1) {
                            self.headerClassArray[columnIndex] = "fa fa-chevron-up";
                        } else if (column === 'LastName') {
                            self.nameHeaderClass = "fa fa-chevron-up";
                        }
                    }
                };

                self.getFormForClass = function (sectionId) {
                    var paramObj = { Id: sectionId };
                    return $http.post(webApiBaseUrl + '/api/sectionreport/GetHRSIW3FormForClass', paramObj);
                }

                self.subCategoryColSpan = function (subCategoryId) {
                    var colSpan = 0;

                    for (var i = 0; i < self.HeaderFields.length; i++) {
                        if (self.HeaderFields[i].SubcategoryId == subCategoryId) {
                            colSpan++;
                        }
                    }

                    return colSpan;
                };
            };

            return (HRSIW3ReportManager);
        }
        ])
        .factory('WVReportManager', [
        '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
            var WVReportManager = function () {
                var self = this;
                self.assessment = {};
                self.studentSectionReportResults = [];
                self.tdds = [];
                self.scale = [];

                self.LoadData = function (assessmentId, sectionId, schoolYear) {
                    var paramObj = { AssessmentId: assessmentId, SectionId: sectionId, SchoolYear: schoolYear };

                    return $http.post(webApiBaseUrl + '/api/sectionreport/GetWVSectionReport', paramObj)
                        .then(
                            function (response) {
                                self.assessment = response.data.Assessment;
                                self.studentSectionReportResults = response.data.StudentSectionReportResults;
                                self.tdds = response.data.TestDueDates;
                                self.scale = response.data.Scale;
                            }
                    );
                };


            };

            return (WVReportManager);
        }
        ])
           .directive('interventionString', [
			'$routeParams', '$compile', '$templateCache', '$http', function ($routeParams, $compile, $templateCache, $http) {
			    return {
			        restrict: 'A',
			        scope: {
			            studentId: '=',
			            tier: '=',
                        interventionRecords: '='
			        },
			        link: function (scope, element, attr) {
			            var recordHtml = '';

			            var interventionString = "";

			            for (var i = 0; i < scope.interventionRecords.length; i++) {
			                if (scope.interventionRecords[i].Tier == scope.tier && scope.interventionRecords[i].StudentId == scope.studentId) {

			                    interventionString += "<a class='badge badge-danger' href='#/ig-dashboard/" +
                                    scope.interventionRecords[i].SchoolStartYear + "/" +
                                    scope.interventionRecords[i].SchoolId + "/" +
                                    scope.interventionRecords[i].InterventionistId + "/" +
                                    scope.interventionRecords[i].InterventionGroupId + "/" +
                                    scope.interventionRecords[i].StudentId + "/" +
                                    scope.interventionRecords[i].StintId +
                                    "'>" + 
                                    scope.interventionRecords[i].InterventionType +
                                    //"/" + 
                                    //scope.interventionRecords[i].StaffInitials +
                                    "-" +
                                    scope.interventionRecords[i].NumberOfLessons +
			                        "<a/><br/>";
			                }
			            }
			            element.html(interventionString);
			            $compile(element.contents())(scope);
			        }
			    };
			}
           ])
        .directive('avmrDetail', [
			'AVMRDetailReportManager', function (AVMRDetailReportManager) {
			    return {
			        restrict: 'AE',
                    templateUrl: 'templates/avmr-detail.html',
			        scope: {
			            detailReportMgr: '=',
                        displayTextAreas: '='
			        }
			    };
			}
        ])
        .directive('activeInterventionString', [
			'$routeParams', '$compile', '$templateCache', '$http', function ($routeParams, $compile, $templateCache, $http) {
			    return {
			        restrict: 'A',
			        templateUrl: 'templates/active-intervention-dashboard-link.html',
			        scope: {
			            studentId: '=',
			            interventionRecords: '='
			        },
			        link: function (scope, element, attr) {
			            scope.intervention = {};

			            var recordHtml = '';

			            var interventionString = "";

			            for (var i = 0; i < scope.interventionRecords.length; i++) {
			                if (scope.interventionRecords[i].StudentId == scope.studentId) {

			                    interventionString += "<div active-intervention-dashboard-link school-year='" + scope.interventionRecords[i].SchoolStartYear +
                                    "' intervention-id='" + scope.interventionRecords[i].StintId +
                                    "' group-id='" + scope.interventionRecords[i].InterventionGroupId +
                                    "' school-id='" + scope.interventionRecords[i].SchoolId +
                                    "' interventionist-id='" + scope.interventionRecords[i].InterventionistId +
                                    "' staff-initials='\"" + scope.interventionRecords[i].StaffInitials +
                                    "\"' student-id='" + scope.interventionRecords[i].StudentId +
                                    "' intervention-type='\"" + scope.interventionRecords[i].InterventionType + "\"'></div>";
                                    //scope.interventionRecords[i].SchoolId + "/" +
                                    //scope.interventionRecords[i].InterventionistId + "/" +
                                    //scope.interventionRecords[i].InterventionGroupId + "/" +
                                    //scope.interventionRecords[i].StudentId + "/" +
                                    //scope.interventionRecords[i].StintId +
                                    //"'>" +
                                    //scope.interventionRecords[i].InterventionType +
                                    ////"/" + 
                                    ////scope.interventionRecords[i].StaffInitials +
                                    //" - " +
                                    //scope.interventionRecords[i].StaffInitials +
			                        //"<a/><br/>";
			                }
			            }

			            if (interventionString != '') {
			                interventionString = '<h3><strong>Interventions</strong></h3>' + interventionString;
			            }

			            element.html(interventionString);
			            $compile(element.contents())(scope);
			        }
			    };
			}
        ])
            .directive('wvScoreByTdd', [
			'$routeParams', '$compile', '$templateCache', '$http', function ($routeParams, $compile, $templateCache, $http) {
			    return {
			        restrict: 'A',
			        scope: {
			            studentResult: '=',
			            tdd: '='
			        },
			        link: function (scope, element, attr) {
			            var score = '';

			            for (var i = 0; i < scope.studentResult.FieldResultsByTestDueDate.length; i++) {

			                if (scope.studentResult.FieldResultsByTestDueDate[i].TDDID === scope.tdd.Id) {
			                    for (var k = 0; k < scope.studentResult.FieldResultsByTestDueDate[i].FieldResults.length; k++) {
			                        // find the right field, then check to see if it should be colored in
			                        if (scope.studentResult.FieldResultsByTestDueDate[i].FieldResults[k].WordsCorrect != null) {
			                            score = scope.studentResult.FieldResultsByTestDueDate[i].FieldResults[k].WordsCorrect;
			                            break;
			                        }
			                    }
			                }
			            }

			            element.html(score);
			            $compile(element.contents())(scope);
			        }
			    };
			}
                    ])
            .directive('studentServices', [
			'$routeParams', '$compile', '$templateCache', '$http', function ($routeParams, $compile, $templateCache, $http) {
			    return {
			        restrict: 'A',
			        scope: {
			            studentId: '=',
			            allStudentServices: '=',
			            showHeader: '=',
                        longFormat: '='
			        },
			        link: function (scope, element, attr) {
			            var recordHtml = '';

			            var studentServiceString = '';

			            for (var i = 0; i < scope.allStudentServices.length; i++) {
			                if (scope.allStudentServices[i].StudentId == scope.studentId) {

			                    if (scope.longFormat) {
			                        studentServiceString += '<span title="' + scope.allStudentServices[i].Label + '">' + scope.allStudentServices[i].Description + '</span><br />';
			                    } else {
			                        studentServiceString += '<span title="' + scope.allStudentServices[i].Description + '">' + scope.allStudentServices[i].Label +
                                        '</span><br />';
			                    }
			                }
			            }

			            if (scope.showHeader && studentServiceString != '') {
			                studentServiceString = '<h3><strong>Student Services</strong></h3>' + studentServiceString;
			            }

			            element.html(studentServiceString);
			            $compile(element.contents())(scope);
			        }
			    };
			}
            ])
            .directive('basData', [
			'$routeParams', '$compile', '$templateCache', '$http', function ($routeParams, $compile, $templateCache, $http) {
			    return {
			        restrict: 'A',
			        scope: {
			            studentId: '=',
			            allStudentResults: '='
			        },
			        link: function (scope, element, attr) {
			            var recordHtml = '';

			            var studentResultString = '';

			            for (var i = 0; i < scope.allStudentResults.length; i++) {
			                if (scope.allStudentResults[i].StudentId == scope.studentId) {
			                    for (var j = 0; j < scope.allStudentResults[i].FieldResults.length; j++) {
			                        if (scope.allStudentResults[i].FieldResults[j].DbColumn == 'Accuracy') {
			                            studentResultString += '<small>Accuracy: </small>' + (scope.allStudentResults[i].FieldResults[j].DecimalValue == null ? '' : scope.allStudentResults[i].FieldResults[j].DecimalValue + '%') + '<br />';
			                        }
			                        if (scope.allStudentResults[i].FieldResults[j].DbColumn == 'Within') {
			                            studentResultString += '<small>Within: </small>' + (scope.allStudentResults[i].FieldResults[j].IntValue == null ? '' : scope.allStudentResults[i].FieldResults[j].IntValue) + '<br />';
			                        }
			                        if (scope.allStudentResults[i].FieldResults[j].DbColumn == 'Beyond') {
			                            studentResultString += '<small>Beyond: </small>' + (scope.allStudentResults[i].FieldResults[j].IntValue == null ? '' : scope.allStudentResults[i].FieldResults[j].IntValue) + '<br />';
			                        }
			                        if (scope.allStudentResults[i].FieldResults[j].DbColumn == 'About') {
			                            studentResultString += '<small>About: </small>' + (scope.allStudentResults[i].FieldResults[j].IntValue == null ? '' : scope.allStudentResults[i].FieldResults[j].IntValue) + '<br />';
			                        }
			                        if (scope.allStudentResults[i].FieldResults[j].DbColumn == 'Fluency') {
			                            studentResultString += '<small>Fluency: </small>' + (scope.allStudentResults[i].FieldResults[j].IntValue == null ? '' : scope.allStudentResults[i].FieldResults[j].IntValue) + '<br />';
			                        }
			                        if (scope.allStudentResults[i].FieldResults[j].DbColumn == 'SelfCorrection') {
			                            studentResultString += '<small>SC: </small>' + (scope.allStudentResults[i].FieldResults[j].IntValue == null ? '' : scope.allStudentResults[i].FieldResults[j].IntValue)  + '<br />';
			                        }
			                    }
			                }
			            }

			            if (studentResultString != '') {
			                studentResultString = '<small class="boldSmall">BAS Data</small>' + studentResultString;
			            }

			            element.html(studentResultString);
			            $compile(element.contents())(scope);
			        }
			    };
			}
            ])
        .directive('verticalAssessment', [
			'$routeParams', '$compile', '$templateCache', '$http', function ($routeParams, $compile, $templateCache, $http) {
			    return {
			        restrict: 'A',
			        scope: {
			            studentId: '=',
			            allStudentResults: '=',
			            assessmentId: '=',
			            assessmentName: '=',
			            fields: '=',
                        benchmarksByGrade: '='
			        },
			        link: function (scope, element, attr) {
			            var recordHtml = '';

			            var studentResultString = '';

			            function getIntColor(gradeId, studentFieldScore, fieldValue) {
			                var benchmarkArray = null;
			                for (var i = 0; i < scope.benchmarksByGrade.length; i++) {

			                    // if this is a state test, use the statetestgrade instead of the one for the overall result
			                    if (studentFieldScore.TestTypeId == 3) {
			                        gradeId = studentFieldScore.StateGradeId;
			                    }

			                    if (scope.benchmarksByGrade[i].GradeId == gradeId) {
			                        benchmarkArray = scope.benchmarksByGrade[i];
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

			            var foundAtLeastOneValue = false;
			            for (var i = 0; i < scope.allStudentResults.length; i++) {
			                if (scope.allStudentResults[i].StudentId == scope.studentId) {
			                    // found the result for this student
			                    for (var f = 0; f < scope.fields.length; f++) {
			                        // loop over every field

			                        for (var j = 0; j < scope.allStudentResults[i].OSFieldResults.length; j++) {

			                            // find all results for this assessment
			                            if (scope.allStudentResults[i].OSFieldResults[j].AssessmentId == scope.assessmentId && scope.fields[f].DatabaseColumn == scope.allStudentResults[i].OSFieldResults[j].DbColumn) {

			                                // only fields for this assessment
			                                if (scope.fields[f].AssessmentId == scope.assessmentId) {
			                                    switch (scope.fields[f].FieldType) {
			                                        case "DropdownFromDB":
			                                            studentResultString += '<tr><td class="verticalOSCell vosLeft"><nobr>' + scope.fields[f].FieldName + '</nobr></td><td ng-class="::getBackgroundClass(allStudentResults[' + i + '].GradeId, allStudentResults[' + i + '].OSFieldResults[' + j + '])" class="verticalOSCell">' + "<ns-assessment-field mode=\"'readonly'\" result='allStudentResults[" + i + "].OSFieldResults[" + j + "]' all-results='studentResult.OSFieldResults'></ns-assessment-field>" + '</td></tr>';
			                                                if (scope.allStudentResults[i].OSFieldResults[j].IntValue != null) {
			                                                    foundAtLeastOneValue = true;
			                                                }
			                                            break;
			                                        case "CalculatedFieldDbBacked":
			                                            studentResultString += '<tr><td class="verticalOSCell vosLeft"><nobr>' + scope.fields[f].FieldName + '</nobr></td><td ng-class="::getBackgroundClass(allStudentResults[' + i + '].GradeId, allStudentResults[' + i + '].OSFieldResults[' + j + '])" class="verticalOSCell">' + "<ns-assessment-field mode=\"'readonly'\" result='allStudentResults[" + i + "].OSFieldResults[" + j + "]' all-results='studentResult.OSFieldResults'></ns-assessment-field>" + '</td></tr>';
			                                                if (scope.allStudentResults[i].OSFieldResults[j].IntValue != null) {
			                                                    foundAtLeastOneValue = true;
			                                                }
			                                                break;
			                                        case "DropdownRange":
			                                            studentResultString += '<tr><td class="verticalOSCell vosLeft"><nobr>' + scope.fields[f].FieldName + '</nobr></td><td ng-class="::getBackgroundClass(allStudentResults[' + i + '].GradeId, allStudentResults[' + i + '].OSFieldResults[' + j + '])" class="verticalOSCell">' + "<ns-assessment-field mode=\"'readonly'\" result='allStudentResults[" + i + "].OSFieldResults[" + j + "]' all-results='studentResult.OSFieldResults'></ns-assessment-field>" + '</td></tr>';
			                                            if (scope.allStudentResults[i].OSFieldResults[j].IntValue != null) {
			                                                foundAtLeastOneValue = true;
			                                            }
			                                            break;
			                                        case "DecimalRange":
			                                            studentResultString += '<tr><td class="verticalOSCell vosLeft"><nobr>' + scope.fields[f].FieldName + '</nobr></td><td ng-class="::getBackgroundClass(allStudentResults[' + i + '].GradeId, allStudentResults[' + i + '].OSFieldResults[' + j + '])" class="verticalOSCell">' + "<ns-assessment-field mode=\"'readonly'\" result='allStudentResults[" + i + "].OSFieldResults[" + j + "]' all-results='studentResult.OSFieldResults'></ns-assessment-field>" + '</td></tr>';
			                                                if (scope.allStudentResults[i].OSFieldResults[j].DecimalValue != null) {
			                                                    foundAtLeastOneValue = true;
			                                                }
			                                            break;
			                                    }
			                                }
			                            }
			                        }
			                    }
			                }
			            }

			            if (studentResultString != '' && foundAtLeastOneValue) {
			                studentResultString = '<h3><strong>' + scope.assessmentName + '</strong></h3><table class="table table-condensed osVTable"><tbody>' + studentResultString + '</tbody></table>';
			                element.html(studentResultString);
			                $compile(element.contents())(scope);
			            }

			        }
			    };
			}
        ])
        .directive('mcaData', [
			'$routeParams', '$compile', '$templateCache', '$http', function ($routeParams, $compile, $templateCache, $http) {
			    return {
			        restrict: 'A',
			        scope: {
			            studentId: '=',
			            allStudentResults: '=',
			            assessmentId: '=',
			            assessmentName: '=',
                        fields: '='
			        },
			        link: function (scope, element, attr) {
			            var recordHtml = '';

			            var studentResultString = '';

			            function getLevelHtml(lookupId) {
			                var levelName = '';

			                switch(lookupId){
			                    case 1:
			                        return '<span class="obsRed">Does Not Meet</span>';
			                    case 2:
			                        return '<span class="obsYellow">Partially Meets</span>';
			                    case 3:
			                        return '<span class="obsGreen">Meets</span>';
			                    case 4:
			                        return '<span class="obsBlue">Exceeds</span>';
			                }

			                return levelName;
			            }

			            var foundAtLeastOneValue = false;
			            for (var i = 0; i < scope.allStudentResults.length; i++) {
			                if (scope.allStudentResults[i].StudentId == scope.studentId) {
                                // found the result for this student
			                    for (var f = 0; f < scope.fields.length; f++) {
			                        // loop over every field

			                        for (var j = 0; j < scope.allStudentResults[i].OSFieldResults.length; j++) {

			                        // find all results for this assessment
			                            if (scope.allStudentResults[i].OSFieldResults[j].AssessmentId == scope.assessmentId && scope.fields[f].DatabaseColumn == scope.allStudentResults[i].OSFieldResults[j].DbColumn) {
			                          
			                                // only fields for this assessment
			                                if (scope.fields[f].AssessmentId == scope.assessmentId) {
			                                    switch (scope.fields[f].FieldType) {
			                                        case "DropdownFromDB":
			                                            if (scope.assessmentId != 63) {
			                                                studentResultString += '<small>' + scope.fields[f].FieldName + ': </small>' + (scope.allStudentResults[i].OSFieldResults[j].IntValue == null ? '' : getLevelHtml(scope.allStudentResults[i].OSFieldResults[j].IntValue)) + '<br />';
			                                                if (scope.allStudentResults[i].OSFieldResults[j].IntValue != null) {
			                                                    foundAtLeastOneValue = true;
			                                                }
			                                            }
			                                            break;
			                                        case "DropdownRange":
			                                            if (scope.assessmentId != 63) {
			                                                studentResultString += '<small>' + scope.fields[f].FieldName + ': </small>' + (scope.allStudentResults[i].OSFieldResults[j].IntValue == null ? '' : scope.allStudentResults[i].OSFieldResults[j].IntValue) + '<br />';
			                                                if (scope.allStudentResults[i].OSFieldResults[j].IntValue != null) {
			                                                    foundAtLeastOneValue = true;
			                                                }
			                                            }
			                                            break;
			                                        case "DecimalRange":
			                                            if (scope.assessmentId == 63) {
			                                                if (scope.allStudentResults[i].OSFieldResults[j].DbColumn.indexOf('Proficiency') >= 0) {
			                                                    studentResultString += '<small>' + scope.fields[f].FieldName + ': </small>' + (scope.allStudentResults[i].OSFieldResults[j].DecimalValue == null ? '' : scope.allStudentResults[i].OSFieldResults[j].DecimalValue) + '<br />';
			                                                    if (scope.allStudentResults[i].OSFieldResults[j].DecimalValue != null) {
			                                                        foundAtLeastOneValue = true;
			                                                    }
			                                                }
			                                            } else {
			                                                studentResultString += '<small>' + scope.fields[f].FieldName + ': </small>' + (scope.allStudentResults[i].OSFieldResults[j].DecimalValue == null ? '' : scope.allStudentResults[i].OSFieldResults[j].DecimalValue) + '<br />';
			                                                if (scope.allStudentResults[i].OSFieldResults[j].DecimalValue != null) {
			                                                    foundAtLeastOneValue = true;
			                                                }
			                                            }
			                                            break;
			                                    }
			                                }
			                            }
			                        }
			                    }
			                }
			            }

			            if (studentResultString != '' && foundAtLeastOneValue) {
			                studentResultString = '<small class="boldSmall">' + scope.assessmentName + '</small>' + studentResultString;
			                element.html(studentResultString);
			                $compile(element.contents())(scope);
			            }

			        }
			    };
			}
        ])
            .directive('fpTestScore', [
			'$routeParams', '$compile', '$templateCache', '$http', function ($routeParams, $compile, $templateCache, $http) {
			    return {
			        restrict: 'A',
			        scope: {
			            scale: '=',
			            studentResult: '=',
                        tdd: '='
			        },
			        link: function (scope, element, attr) {
			            var cellHtml = '';

			            var currentTDDID = scope.tdd.Id;

			            for (var i = 0; i < scope.studentResult.FieldResultsByTestDueDate.length; i++) {
			                if (scope.studentResult.FieldResultsByTestDueDate[i].TDDID === currentTDDID) {
			                    if (scope.studentResult.FieldResultsByTestDueDate[i].FieldResults.length > 0) { // if there is a value
			                        for (var p = 0; p < scope.scale.length; p++) {
			                            if (scope.studentResult.FieldResultsByTestDueDate[i].FieldResults[0].FPValueId == scope.scale[p].FPID) {
			                                cellHtml = scope.scale[p].FPs;
			                                break;
			                            }
			                        }
			                    }
			                }
			            }
			            
			            element.html(cellHtml);
			            $compile(element.contents())(scope);
			        }
			    };
			}
                    ])
         .directive('fpCommentsCell', [
			'$routeParams', '$compile', '$templateCache', '$http', '$uibModal', function ($routeParams, $compile, $templateCache, $http, $uibModal) {


			    return {
			        restrict: 'A',
			        scope: {
			            scaleRow: '=',
			            studentresult: '=',
			            tdds: '=',
			            targetZone: '=',
			            eoyBenchmark: '=',
			            soyBenchmark: '=',
			            scale: '='
			        },
			        link: function (scope, element, attr) {

			            var currentColor = '';
			            //var previousColor = '';
			            var currentCellLeftHtml = '';
			            var currentCellRightHtml = '</td>';
			            var rowHtml = '';
			            var currentInnerText = '';
			            var currentNoteText = '';
			            var noteLeftTemplate = "";
			            var noteRightTemplate = '';
			            var currentCategory = 0;
			            var cssBorderClass = '';
			            scope.settings = { comments: 'empty' };

			            scope.toolTipFunction = function (tddId, date, duedateIndex, resultIndex,
                                  tddId2, date2, duedateIndex2, resultIndex2,
                                  tddId3, date3, duedateIndex3, resultIndex3,
                                  tddId4, date4, duedateIndex4, resultIndex4) {
			                var returnString = '';


			                returnString = '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex].FieldResults[resultIndex].Comment + '</span></div>';

			                if (angular.isDefined(tddId2)) {
			                    returnString += '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId2 + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date2 + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex2].FieldResults[resultIndex2].Comment + '</span></div>';
			                }
			                if (angular.isDefined(tddId3)) {
			                    returnString += '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId3 + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date3 + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex3].FieldResults[resultIndex3].Comment + '</span></div>';
			                }
			                if (angular.isDefined(tddId4)) {
			                    returnString += '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId4 + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date4 + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex4].FieldResults[resultIndex4].Comment + '</span></div>';
			                }

			                var modalInstance = $uibModal.open({
			                    templateUrl: 'commentsModal.html',
			                    scope: scope,
			                    controller: function ($scope, $uibModalInstance) {
			                        $scope.settings.comments = returnString;
			                        $scope.cancel = function () {
			                            $uibModalInstance.dismiss('cancel');
			                        };
			                    },
			                    size: 'lg',
			                });

			                //return returnString;
			            }

			                var studentresult = scope.studentresult;
			                currentCellLeftHtml = '';
			                currentInnerText = '';
			                currentNoteText = '';
			                noteLeftTemplate = "";
			                noteRightTemplate = "";
			                cssBorderClass = '';

			                for (var j = 0; j < scope.tdds.length; j++) {
			                    var currentTDDID = scope.tdds[j].Id;

			                    //TODO, need to do a foreach of the headerfields and output them
			                    for (var i = 0; i < studentresult.FieldResultsByTestDueDate.length; i++) {

			                        if (studentresult.FieldResultsByTestDueDate[i].TDDID === currentTDDID) {
			                            // we've found the color for this one, do we color the cell or not?
			                            for (var k = 0; k < studentresult.FieldResultsByTestDueDate[i].FieldResults.length; k++) {
			                                // do the comment here for each tdd, only add one icon
			                                if (studentresult.FieldResultsByTestDueDate[i].FieldResults[k].Comment != null && studentresult.FieldResultsByTestDueDate[i].FieldResults[k].Comment != '') {
			                                    if (currentNoteText === '') {
			                                        noteLeftTemplate = '<i class="fa fa-comments" style="margin-left:5px;cursor:pointer" ng-click="toolTipFunction(';
			                                        noteRightTemplate = ');"></i>';
			                                    }
			                                    var d = new Date(Date.parse(scope.tdds[j].DueDate));
			                                    currentNoteText += "'" + scope.tdds[j].Id + "','" + (d.getMonth() + 1) + "/" + d.getFullYear() + "'," + i + "," + k + ",";
			                                }
			                            }
			                        }
			                    }
			                }

			                // remove trailing comma on currentNotText
			                if (currentNoteText.length > 0) {
			                    currentNoteText = currentNoteText.substring(0, currentNoteText.length - 1);
			                }


			                //currentCategory = scope.headerfields[p].SubcategoryId;
			                // add an empty cell if none of the test due dates are checked
			                if (currentCellLeftHtml === '') {

			                    if (noteLeftTemplate === '') {
			                        rowHtml += "<td></td>";
			                    }
			                    else {
			                        rowHtml += "<td>" + noteLeftTemplate + currentNoteText + noteRightTemplate + "</td>";
			                    }
			                }
			                else {
			                    if (noteLeftTemplate === '') {
			                        rowHtml += currentCellLeftHtml + currentInnerText + currentCellRightHtml;
			                    }
			                    else {
			                        rowHtml += currentCellLeftHtml + currentInnerText + noteLeftTemplate + currentNoteText + noteRightTemplate + currentCellRightHtml;
			                    }

			                }

			            //element.html("cheese");
			            element.html(rowHtml);
			            $compile(element.contents())(scope);
			        }
			    };
			}
         ])
         .directive('fpSectionReportTableRow', [
			'$routeParams', '$compile', '$templateCache', '$http', function ($routeParams, $compile, $templateCache, $http) {


			    return {
			        restrict: 'A',
			        scope: {
			            scaleRow: '=',
			            studentresults: '=',
			            tdds: '=',
			            targetZone: '=',
			            eoyBenchmark: '=',
			            soyBenchmark: '=',
                        benchmarksByGrade: '=',
                        previousGradeScores: '='
			        },
			        link: function (scope, element, attr) {

			            var currentColor = '';
			            //var previousColor = '';
			            var currentCellLeftHtml = '';
			            var currentCellRightHtml = '</td>';
			            var rowHtml = '';
			            var currentInnerText = '';
			            var currentNoteText = '';
			            var noteLeftTemplate = "";
			            var noteRightTemplate = '';
			            var currentCategory = 0;
			            var cssBorderClass = '';
			            var scaleRowFPValueID = scope.scaleRow.FPID;

			            function getCellStyle() {
			                var cellStyle = '';

                            // use the grade level for the section for the leftmost columns
			                // target zone
			                if (scope.scaleRow.FPID >= scope.targetZone.Meets && scope.scaleRow.FPID < scope.targetZone.Exceeds) {
			                    cellStyle = 'targetZone';
			                }
			                // eoy
			                if (scope.scaleRow.FPID == scope.eoyBenchmark.Exceeds - 1) {
			                    cellStyle += ' eoyBenchmark';
			                }
			                // soy
			                if (scope.scaleRow.FPID == scope.soyBenchmark.Meets || (scope.soyBenchmark.Meets == 1 && scope.scaleRow.FPID == 1)) {
			                    cellStyle += ' soyBenchmark';
			                }

			                return cellStyle;
			            }

			            function getCellStyleForStudentResult(studentResult) {
			                var cellStyle = '';
                                // use the benchmarks from student's current grade
			                    var currentTargetZone = scope.targetZone;
			                    var currentEoyBenchmark = scope.eoyBenchmark;
			                    var currentSoyBenchmark = scope.soyBenchmark;

			                    for (var i = 0; i < scope.benchmarksByGrade.length; i++) {
			                        if (scope.benchmarksByGrade[i].GradeId == studentResult.GradeId) {
			                            currentTargetZone = scope.benchmarksByGrade[i].TargetZone;
			                            currentEoyBenchmark = scope.benchmarksByGrade[i].EndOfYearBenchmark;
			                            currentSoyBenchmark = scope.benchmarksByGrade[i].StartOfYearBenchmark;

			                            // target zone
			                            if (scope.scaleRow.FPID >= currentTargetZone.Meets && scope.scaleRow.FPID < currentTargetZone.Exceeds) {
			                                cellStyle = 'targetZone';
			                            }
			                            // eoy
			                            if (scope.scaleRow.FPID == currentEoyBenchmark.Exceeds - 1) {
			                                cellStyle += ' eoyBenchmark';
			                            }
			                            // soy
			                            if (scope.scaleRow.FPID == currentSoyBenchmark.Meets || (currentSoyBenchmark.Meets == 1 && scope.scaleRow.FPID == 1)) {
			                                cellStyle += ' soyBenchmark';
			                            }
			                        }
			                    }

			                return cellStyle;
			            }

                        // left side of grid, TODO: css class for upper and lower and benchmark gray
			            rowHtml += "<td class='" + getCellStyle() + "'>" + scope.scaleRow.Grades + "</td>";
			            rowHtml += "<td class='" + getCellStyle() + "'>" + scope.scaleRow.DRAs + "</td>";
			            rowHtml += "<td class='" + getCellStyle() + "'>" + (scope.scaleRow.RR === null ? "" : scope.scaleRow.RR) + "</td>";
			            rowHtml += "<td class='" + getCellStyle() + "'>" + scope.scaleRow.FPs + "</td>";
			            rowHtml += "<td class='" + getCellStyle() + "'>" + scope.scaleRow.Lexiles + "</td>";
			            // loop over the tdds for each studentresult and find the latest one, then get the hex for it?

			            for (var p = 0; p < scope.studentresults.length; p++) {
			                var studentresult = scope.studentresults[p];
			                currentCellLeftHtml = '';
			                currentInnerText = '';
			                currentNoteText = '';
			                noteLeftTemplate = "";
			                noteRightTemplate = "";
			                cssBorderClass = '';

			                // previous grade score
			                for (var n = 0; n < scope.previousGradeScores.length; n++) {
			                    if (scope.previousGradeScores[n].Id === studentresult.StudentId && scope.previousGradeScores[n].PreviousFPID === scope.scaleRow.FPID) {
			                        currentInnerText += "<span class='badge' style='background-color:black'>" + scope.previousGradeScores[n].PreviousGradeLabel + "</span>";
			                        break;
			                    }
			                }

			                for (var n = 0; n < studentresult.SummaryFieldResults.length; n++) {
			                    // find the right field, then check to see if it should be colored in
			                    if (studentresult.SummaryFieldResults[n].FPValueId === scaleRowFPValueID) {
			                        // NEW
			                        if (studentresult.SummaryFieldResults[n].XColorDates.length > 0) {
			                            var hexColor = '';
			                            for (var r = 0; r < studentresult.SummaryFieldResults[n].XColorDates.length; r++) {
			                                for (var t = 0; t < scope.tdds.length; t++) {
			                                    if (scope.tdds[t].DueDate == studentresult.SummaryFieldResults[n].XColorDates[r]) {
			                                        hexColor = scope.tdds[t].Hex;
			                                        currentInnerText += "<span class='badge' style='background-color:black;color:" + hexColor + "'>X</span>";
			                                        break;
			                                    }
			                                }
			                            }

			                        }

			                        if (studentresult.SummaryFieldResults[n].CellColorDate != null) {
			                            var hexColor = '';
			                            for (var t = 0; t < scope.tdds.length; t++) {
			                                if (scope.tdds[t].DueDate == studentresult.SummaryFieldResults[n].CellColorDate) {
			                                    hexColor = scope.tdds[t].Hex;
			                                }
			                            }

			                            currentCellLeftHtml = "<td class='" + getCellStyleForStudentResult(studentresult) + "' style='font-weight:bold;text-align:center;background-color:" + hexColor + "'>";
			                        }
			                    }
			                }


			                //currentCategory = scope.headerfields[p].SubcategoryId;
			                // add an empty cell if none of the test due dates are checked
			                if (currentCellLeftHtml === '') {

			                    if (noteLeftTemplate === '') {
			                        if (currentInnerText == '')
			                        {
			                            rowHtml += "<td class='" + getCellStyleForStudentResult(studentresult) + "' ></td>";

			                        } else {
			                            rowHtml += "<td class='" + getCellStyleForStudentResult(studentresult) + "' style='font-weight:bold;text-align:center'>" + currentInnerText + "</td>";

			                        }
			                    }
			                    else {
			                        rowHtml += "<td class='" + getCellStyleForStudentResult(studentresult) + "' >" + noteLeftTemplate + currentNoteText + noteRightTemplate + "</td>";
			                    }
			                }
			                else {
			                    if (noteLeftTemplate === '') {
			                        rowHtml += currentCellLeftHtml + currentInnerText + currentCellRightHtml;
			                    }
			                    else {
			                        rowHtml += currentCellLeftHtml + currentInnerText + noteLeftTemplate + currentNoteText + noteRightTemplate + currentCellRightHtml;
			                    }

			                }
			            }

			            //element.html("cheese");
			            element.html(rowHtml);
			            $compile(element.contents())(scope);
			        }
			    };
			}
         ])
        .directive('wvSectionReportTableRow', [
			'$routeParams', '$compile', '$templateCache', '$http', function ($routeParams, $compile, $templateCache, $http) {
			    return {
			        restrict: 'A',
			        scope: {
			            scaleRow: '=',
			            studentresults: '=',
			            tdds: '='
			        },
			        link: function (scope, element, attr) {

			            var currentColor = '';
			            //var previousColor = '';
			            var currentCellLeftHtml = '';
			            var currentCellRightHtml = '</td>';
			            var rowHtml = '';
			            var currentInnerText = '';
			            var currentNoteText = '';
			            var noteLeftTemplate = "";
			            var noteRightTemplate = '';
			            var currentCategory = 0;
			            var cssBorderClass = '';
			            var scaleRowScore = scope.scaleRow.id;


			            // left side of grid, TODO: css class for upper and lower and benchmark gray
			            rowHtml += "<td>" + scope.scaleRow.text + "</td>";
			            // loop over the tdds for each studentresult and find the latest one, then get the hex for it?

			            for (var p = 0; p < scope.studentresults.length; p++) {
			                var studentresult = scope.studentresults[p];
			                currentCellLeftHtml = '';
			                currentInnerText = '';
			                currentNoteText = '';
			                noteLeftTemplate = "";
			                noteRightTemplate = "";
			                cssBorderClass = '';

			                for (var n = 0; n < studentresult.SummaryFieldResults.length; n++) {
			                    // find the right field, then check to see if it should be colored in
			                    if (studentresult.SummaryFieldResults[n].WordsCorrect === scaleRowScore) {
			                        // NEW
			                        if (studentresult.SummaryFieldResults[n].XColorDates.length > 0) {
			                            var hexColor = '';
			                            for (var r = 0; r < studentresult.SummaryFieldResults[n].XColorDates.length; r++) {
			                                for (var t = 0; t < scope.tdds.length; t++) {
			                                    if (scope.tdds[t].DueDate == studentresult.SummaryFieldResults[n].XColorDates[r]) {
			                                        hexColor = scope.tdds[t].Hex;
			                                        currentInnerText += "<span class='badge' style='background-color:black;color:" + hexColor + "'>X</span>";
			                                        break;
			                                    }
			                                }
			                            }

			                        }

			                        if (studentresult.SummaryFieldResults[n].CellColorDate != null) {
			                            var hexColor = '';
			                            for (var t = 0; t < scope.tdds.length; t++) {
			                                if (scope.tdds[t].DueDate == studentresult.SummaryFieldResults[n].CellColorDate) {
			                                    hexColor = scope.tdds[t].Hex;
			                                }
			                            }

			                            currentCellLeftHtml = "<td style='font-weight:bold;text-align:center;background-color:" + hexColor + "'>";
			                        }
			                    }
			                }


			                //currentCategory = scope.headerfields[p].SubcategoryId;
			                // add an empty cell if none of the test due dates are checked
			                if (currentCellLeftHtml === '') {

			                    if (noteLeftTemplate === '') {
			                        rowHtml += "<td></td>";
			                    }
			                    else {
			                        rowHtml += "<td>" + noteLeftTemplate + currentNoteText + noteRightTemplate + "</td>";
			                    }
			                }
			                else {
			                    if (noteLeftTemplate === '') {
			                        rowHtml += currentCellLeftHtml + currentInnerText + currentCellRightHtml;
			                    }
			                    else {
			                        rowHtml += currentCellLeftHtml + currentInnerText + noteLeftTemplate + currentNoteText + noteRightTemplate + currentCellRightHtml;
			                    }

			                }
			            }

			            //element.html("cheese");
			            element.html(rowHtml);
			            $compile(element.contents())(scope);
			        }
			    };
			}
        ])
        .directive('avmrSectionReportTableRow', [
			'$routeParams', '$compile', '$templateCache', '$http', function ($routeParams, $compile, $templateCache, $http) {


			    return {
			        restrict: 'A',
			        scope: {
			            headerfields: '=',
			            studentresult: '=',
                        tdd: '='
			            
			        },
			        link: function (scope, element, attr) {

			            var currentColor = scope.tdd.Hex;
			            var currentTDDID = scope.tdd.Id;
			            //var previousColor = '';
			            var currentCellLeftHtml = '';
			            var currentCellRightHtml = '</td>';
			            var rowHtml = '';
			            var currentInnerText = '';
			            var currentNoteText = '';
			            var noteLeftTemplate = "";
			            var noteRightTemplate = '';
			            var currentCategory = 0;
			            var cssBorderClass = '';

                        // TODO: make this more generic in V5 so that number of dates doesn't matter
			            scope.toolTipFunction = function (tddId, date, resultIndex) {
			                var returnString = '';


			                returnString = '<div class="commentSpacing"><span class="commentCellBox selectedDateBg">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResults[resultIndex].Comment + '</span></div>';

			                return returnString;
			            }

			            rowHtml += "<td><span student-dashboard-link student-id=\"" + scope.studentresult.StudentId + "\" student-name=\"'" + scope.studentresult.FullName.replace(/'/g, "\\'") + "'\"></span></td>";
			            // loop over the tdds for each studentresult and find the latest one, then get the hex for it?

			            for (var p = 0; p < scope.headerfields.length; p++) {
			                var field = scope.headerfields[p];
			                currentCellLeftHtml = '';
			                currentInnerText = '';
			                currentNoteText = '';
			                noteLeftTemplate = "";
			                noteRightTemplate = "";
			                cssBorderClass = '';

			                if (currentCategory != scope.headerfields[p].GroupContainerId) {
			                    cssBorderClass = 'leftDoubleBorder';

			                    if (p == scope.headerfields.length - 1) {
			                        cssBorderClass = 'leftDoubleBorder rightDoubleBorder';
			                    }
			                }
			                else if (p == scope.headerfields.length - 1) {
			                    cssBorderClass = 'rightDoubleBorder';
			                }



			                    //TODO, need to do a foreach of the headerfields and output them
			                        // we've found the color for this one, do we color the cell or not?
			                for (var k = 0; k < scope.studentresult.FieldResults.length; k++) {
			                    // find the right field, then check to see if it should be colored in
			                    if (scope.studentresult.FieldResults[k].GroupId === field.GroupId) {

			                        // do the comment here for each tdd, only add one icon
			                        if (scope.studentresult.FieldResults[k].Comment != null && scope.studentresult.FieldResults[k].Comment != '') {
			                            if (currentNoteText === '') {
			                                noteLeftTemplate = '<i popover-trigger="mouseenter" class="fa fa-comments" style="margin-left:5px;cursor:pointer" uib-popover-html="toolTipFunction(';
			                                noteRightTemplate = ');" popover-title="Comments"></i>';
			                            }
			                            var d = new Date(Date.parse(scope.tdd.text));
			                            currentNoteText += "'" + scope.tdd.Id + "','" + (d.getMonth() + 1) + "/" + d.getFullYear() + "'," + k + ",";
			                        }


			                        if (scope.studentresult.FieldResults[k].Checked == true) {
			                            var hexColor = '';
			                            //for (var t = 0; t < scope.tdds.length; t++) {
			                            //    if (scope.tdds[t].DueDate == scope.studentresult.SummaryFieldResults[n].CellColorDate) {
			                            hexColor = scope.tdd.Hex;
			                            //    }
			                            //}

			                            currentCellLeftHtml = "<td class='" + cssBorderClass + "' style='font-weight:bold;text-align:center;background-color:" + hexColor + "'>";
			                        }
			                    }
			                }


			          

			                // remove trailing comma on currentNotText
			                if (currentNoteText.length > 0) {
			                    currentNoteText = currentNoteText.substring(0, currentNoteText.length - 1);
			                }

			                currentCategory = scope.headerfields[p].GroupContainerId;
			                // add an empty cell if none of the test due dates are checked
			                if (currentCellLeftHtml === '') {

			                    if (noteLeftTemplate === '') {
			                        rowHtml += "<td class='" + cssBorderClass + "' ></td>";
			                    }
			                    else {
			                        rowHtml += "<td class='" + cssBorderClass + "' >" + noteLeftTemplate + currentNoteText + noteRightTemplate + "</td>";
			                    }
			                }
			                else {
			                    if (noteLeftTemplate === '') {
			                        rowHtml += currentCellLeftHtml + currentInnerText + currentCellRightHtml;
			                    }
			                    else {
			                        rowHtml += currentCellLeftHtml + currentInnerText + noteLeftTemplate + currentNoteText + noteRightTemplate + currentCellRightHtml;
			                    }

			                }
			            }

			            //element.html("cheese");
			            element.html(rowHtml);
			            $compile(element.contents())(scope);
			        }
			    };
			}
        ])
        .directive('avmrSingleDateSectionReportTableRow', [
			'$routeParams', '$compile', '$templateCache', '$http', function ($routeParams, $compile, $templateCache, $http) {


			    return {
			        restrict: 'A',
			        scope: {
			            headerfields: '=',
			            studentresult: '=',
			            tdds: '='
			        },
			        link: function (scope, element, attr) {

			            var currentColor = '';
			            //var previousColor = '';
			            var currentCellLeftHtml = '';
			            var currentCellRightHtml = '</td>';
			            var rowHtml = '';
			            var currentInnerText = '';
			            var currentNoteText = '';
			            var noteLeftTemplate = "";
			            var noteRightTemplate = '';
			            var currentCategory = 0;
			            var cssBorderClass = '';

			            scope.toolTipFunction = function (tddId, date, duedateIndex, resultIndex,
                                                          tddId2, date2, duedateIndex2, resultIndex2,
                                                          tddId3, date3, duedateIndex3, resultIndex3,
                                                          tddId4, date4, duedateIndex4, resultIndex4) {
			                var returnString = '';


			                returnString = '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex].FieldResults[resultIndex].Comment + '</span></div>';

			                if (angular.isDefined(tddId2)) {
			                    returnString += '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId2 + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date2 + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex2].FieldResults[resultIndex2].Comment + '</span></div>';
			                }
			                if (angular.isDefined(tddId3)) {
			                    returnString += '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId3 + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date3 + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex3].FieldResults[resultIndex3].Comment + '</span></div>';
			                }
			                if (angular.isDefined(tddId4)) {
			                    returnString += '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId4 + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date4 + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex4].FieldResults[resultIndex4].Comment + '</span></div>';
			                }

			                return returnString;
			            }

			            rowHtml += "<td><span student-dashboard-link student-id=\"" + scope.studentresult.StudentId + "\" student-name=\"'" + scope.studentresult.LastName.replace(/'/g, "\\'") + ", " + scope.studentresult.FirstName.replace(/'/g, "\\'") + "'\"></span></td>";
			            // loop over the tdds for each studentresult and find the latest one, then get the hex for it?

			            for (var p = 0; p < scope.headerfields.length; p++) {
			                var field = scope.headerfields[p];
			                currentCellLeftHtml = '';
			                currentInnerText = '';
			                currentNoteText = '';
			                noteLeftTemplate = "";
			                noteRightTemplate = "";
			                cssBorderClass = '';

			                if (currentCategory != scope.headerfields[p].GroupContainerId) {
			                    cssBorderClass = 'leftDoubleBorder';

			                    if (p == scope.headerfields.length - 1) {
			                        cssBorderClass = 'leftDoubleBorder rightDoubleBorder';
			                    }
			                }
			                else if (p == scope.headerfields.length - 1) {
			                    cssBorderClass = 'rightDoubleBorder';
			                }

			                for (var j = 0; j < scope.tdds.length; j++) {
			                    currentColor = scope.tdds[j].Hex;
			                    var currentTDDID = scope.tdds[j].Id;

			                    //TODO, need to do a foreach of the headerfields and output them

			                    for (var i = 0; i < scope.studentresult.FieldResultsByTestDueDate.length; i++) {

			                        if (scope.studentresult.FieldResultsByTestDueDate[i].TDDID === currentTDDID) {
			                            // we've found the color for this one, do we color the cell or not?
			                            for (var k = 0; k < scope.studentresult.FieldResultsByTestDueDate[i].FieldResults.length; k++) {
			                                // find the right field, then check to see if it should be colored in
			                                if (scope.studentresult.FieldResultsByTestDueDate[i].FieldResults[k].GroupId === field.GroupId) {

			                                    // do the comment here for each tdd, only add one icon
			                                    if (scope.studentresult.FieldResultsByTestDueDate[i].FieldResults[k].Comment != null && scope.studentresult.FieldResultsByTestDueDate[i].FieldResults[k].Comment != '') {
			                                        if (currentNoteText === '') {
			                                            noteLeftTemplate = '<i popover-trigger="mouseenter" class="fa fa-comments" style="margin-left:5px;cursor:pointer" uib-popover-html="toolTipFunction(';
			                                            noteRightTemplate = ');" popover-title="Comments"></i>';
			                                        }
			                                        var d = new Date(Date.parse(scope.tdds[j].DueDate));
			                                        currentNoteText += "'" + scope.tdds[j].Id + "','" + (d.getMonth() + 1) + "/" + d.getFullYear() + "'," + i + "," + k + ",";
			                                    }

			                                }
			                            }
			                        }
			                    }



			                    for (var n = 0; n < scope.studentresult.SummaryFieldResults.length; n++) {
			                        // find the right field, then check to see if it should be colored in
			                        if (scope.studentresult.SummaryFieldResults[n].GroupId === field.GroupId) {
			                            // NEW
			                            if (scope.studentresult.SummaryFieldResults[n].XColorDate != null) {
			                                var hexColor = '';
			                                for (var t = 0; t < scope.tdds.length; t++) {
			                                    if (scope.tdds[t].DueDate == scope.studentresult.SummaryFieldResults[n].XColorDate) {
			                                        hexColor = scope.tdds[t].Hex;
			                                    }
			                                }

			                                currentInnerText = "<span style='color:" + hexColor + "'>X</span>";
			                            }

			                            if (scope.studentresult.SummaryFieldResults[n].CellColorDate != null) {
			                                var hexColor = '';
			                                for (var t = 0; t < scope.tdds.length; t++) {
			                                    if (scope.tdds[t].DueDate == scope.studentresult.SummaryFieldResults[n].CellColorDate) {
			                                        hexColor = scope.tdds[t].Hex;
			                                    }
			                                }

			                                currentCellLeftHtml = "<td class='" + cssBorderClass + "' style='font-weight:bold;text-align:center;background-color:" + hexColor + "'>";
			                            }
			                        }
			                    }
			                }

			                // remove trailing comma on currentNotText
			                if (currentNoteText.length > 0) {
			                    currentNoteText = currentNoteText.substring(0, currentNoteText.length - 1);
			                }

			                currentCategory = scope.headerfields[p].GroupContainerId;
			                // add an empty cell if none of the test due dates are checked
			                if (currentCellLeftHtml === '') {

			                    if (noteLeftTemplate === '') {
			                        rowHtml += "<td class='" + cssBorderClass + "' ></td>";
			                    }
			                    else {
			                        rowHtml += "<td class='" + cssBorderClass + "' >" + noteLeftTemplate + currentNoteText + noteRightTemplate + "</td>";
			                    }
			                }
			                else {
			                    if (noteLeftTemplate === '') {
			                        rowHtml += currentCellLeftHtml + currentInnerText + currentCellRightHtml;
			                    }
			                    else {
			                        rowHtml += currentCellLeftHtml + currentInnerText + noteLeftTemplate + currentNoteText + noteRightTemplate + currentCellRightHtml;
			                    }

			                }
			            }

			            //element.html("cheese");
			            element.html(rowHtml);
			            $compile(element.contents())(scope);
			        }
			    };
			}
        ])
         .directive('capSectionReportTableRow', [
			'$routeParams', '$compile', '$templateCache', '$http', function ($routeParams, $compile, $templateCache, $http) {


			    return {
			        restrict: 'A',
			        scope: {
			            headerfields: '=',
			            studentresult: '=',
			            tdds: '='
			        },
			        link: function (scope, element, attr) {

			            var currentColor = '';
			            //var previousColor = '';
			            var currentCellLeftHtml = '';
			            var currentCellRightHtml = '</td>';
			            var rowHtml = '';
			            var currentInnerText = '';
			            var currentNoteText = '';
			            var noteLeftTemplate = "";
			            var noteRightTemplate = '';
			            var currentCategory = 0;
			            var cssBorderClass = '';

			            scope.toolTipFunction = function (tddId, date, duedateIndex, resultIndex,
                                                          tddId2, date2, duedateIndex2, resultIndex2,
                                                          tddId3, date3, duedateIndex3, resultIndex3,
                                                          tddId4, date4, duedateIndex4, resultIndex4) {
			                var returnString = '';

                            
			                returnString = '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex].FieldResults[resultIndex].Comment + '</span></div>';

			                if (angular.isDefined(tddId2)) {
			                    returnString += '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId2 + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date2 + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex2].FieldResults[resultIndex2].Comment + '</span></div>';
			                }
			                if (angular.isDefined(tddId3)) {
			                    returnString += '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId3 + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date3 + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex3].FieldResults[resultIndex3].Comment + '</span></div>';
			                }
			                if (angular.isDefined(tddId4)) {
			                    returnString += '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId4 + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date4 + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex4].FieldResults[resultIndex4].Comment + '</span></div>';
			                }

			                return returnString;
			            }

			            rowHtml += "<td><span student-dashboard-link student-id=\"" + scope.studentresult.StudentId + "\" student-name=\"'" + scope.studentresult.LastName.replace(/'/g, "\\'") + ", " + scope.studentresult.FirstName.replace(/'/g, "\\'") + "'\"></span></td>";
			            // loop over the tdds for each studentresult and find the latest one, then get the hex for it?

			            for (var p = 0; p < scope.headerfields.length; p++) {
			                var field = scope.headerfields[p];
			                currentCellLeftHtml = '';
			                currentInnerText = '';
			                currentNoteText = '';
			                noteLeftTemplate = "";
			                noteRightTemplate = "";
			                cssBorderClass = '';

			                if (currentCategory != scope.headerfields[p].SubcategoryId) {
			                    cssBorderClass = 'leftDoubleBorder';
			                }
			                else if (p == scope.headerfields.length - 1) {
			                    cssBorderClass = 'rightDoubleBorder';
			                }

			                for (var j = 0; j < scope.tdds.length; j++) {
			                    currentColor = scope.tdds[j].Hex;
			                    var currentTDDID = scope.tdds[j].Id;

			                    //TODO, need to do a foreach of the headerfields and output them

			                    for (var i = 0; i < scope.studentresult.FieldResultsByTestDueDate.length; i++) {

			                        if (scope.studentresult.FieldResultsByTestDueDate[i].TDDID === currentTDDID) {
			                            // we've found the color for this one, do we color the cell or not?
			                            for (var k = 0; k < scope.studentresult.FieldResultsByTestDueDate[i].FieldResults.length; k++) {
			                                // find the right field, then check to see if it should be colored in
			                                if (scope.studentresult.FieldResultsByTestDueDate[i].FieldResults[k].GroupId === field.GroupId) {

			                                    // do the comment here for each tdd, only add one icon
			                                    if (scope.studentresult.FieldResultsByTestDueDate[i].FieldResults[k].Comment != null && scope.studentresult.FieldResultsByTestDueDate[i].FieldResults[k].Comment != '') {
			                                        if (currentNoteText === '') {
			                                            noteLeftTemplate = '<i popover-trigger="mouseenter" class="fa fa-comments" style="margin-left:5px;cursor:pointer" uib-popover-html="toolTipFunction(';
			                                            noteRightTemplate = ');" popover-title="Comments"></i>';
			                                        }
			                                        var d = new Date(Date.parse(scope.tdds[j].DueDate));
			                                        currentNoteText += "'" + scope.tdds[j].Id + "','" + (d.getMonth() + 1) + "/" + d.getFullYear() + "'," + i + "," + k + ","; 
			                                    }

			                                }
			                            }
			                        }
			                    }



			                    for (var n = 0; n < scope.studentresult.SummaryFieldResults.length; n++) {
			                        // find the right field, then check to see if it should be colored in
			                        if (scope.studentresult.SummaryFieldResults[n].GroupId === field.GroupId) {
			                            // NEW
			                            if (scope.studentresult.SummaryFieldResults[n].XColorDate != null) {
			                                var hexColor = '';
			                                for (var t = 0; t < scope.tdds.length; t++) {
			                                    if (scope.tdds[t].DueDate == scope.studentresult.SummaryFieldResults[n].XColorDate) {
			                                        hexColor = scope.tdds[t].Hex;
			                                    }
			                                }

			                                currentInnerText = "<span style='color:" + hexColor + "'>X</span>";
			                            }

			                            if (scope.studentresult.SummaryFieldResults[n].CellColorDate != null) {
			                                var hexColor = '';
			                                for (var t = 0; t < scope.tdds.length; t++) {
			                                    if (scope.tdds[t].DueDate == scope.studentresult.SummaryFieldResults[n].CellColorDate) {
			                                        hexColor = scope.tdds[t].Hex;
			                                    }
			                                }

			                                currentCellLeftHtml = "<td class='" + cssBorderClass + "' style='font-weight:bold;text-align:center;background-color:" + hexColor + "'>";
			                            }
			                        }
			                    }
			                }

			                // remove trailing comma on currentNotText
			                if (currentNoteText.length > 0) {
			                    currentNoteText = currentNoteText.substring(0, currentNoteText.length - 1);
			                }

			                currentCategory = scope.headerfields[p].SubcategoryId;
			                // add an empty cell if none of the test due dates are checked
			                if (currentCellLeftHtml === '') {

			                    if (noteLeftTemplate === '') {
			                        rowHtml += "<td class='" + cssBorderClass + "' ></td>";
			                    }
			                    else {
			                        rowHtml += "<td class='" + cssBorderClass + "' >" + noteLeftTemplate + currentNoteText + noteRightTemplate + "</td>";
			                    }
			                }
			                else {
			                    if (noteLeftTemplate === '') {
			                        rowHtml += currentCellLeftHtml + currentInnerText + currentCellRightHtml;
			                    }
			                    else {
			                        rowHtml += currentCellLeftHtml + currentInnerText + noteLeftTemplate + currentNoteText + noteRightTemplate + currentCellRightHtml;
			                    }

			                }
			            }

			            //element.html("cheese");
			            element.html(rowHtml);
			            $compile(element.contents())(scope);
			        }
			    };
			}
         ])
        .directive('hrsiwSectionReportTableRow', [
			'$routeParams', '$compile', '$templateCache', '$http', function ($routeParams, $compile, $templateCache, $http) {


			    return {
			        restrict: 'A',
			        scope: {
			            headerfields: '=',
			            studentresult: '=',
			            tdds: '='
			        },
			        link: function (scope, element, attr) {

			            var currentColor = '';
			            //var previousColor = '';
			            var currentCellLeftHtml = '';
			            var currentCellRightHtml = '</td>';
			            var rowHtml = '';
			            var currentInnerText = '';
			            var currentNoteText = '';
			            var noteLeftTemplate = "";
			            var noteRightTemplate = '';
			            var currentCategory = 0;
			            var cssBorderClass = '';

			            scope.toolTipFunction = function (tddId, date, duedateIndex,
                                  tddId2, date2, duedateIndex2,
                                  tddId3, date3, duedateIndex3,
                                  tddId4, date4, duedateIndex4) {
			                var returnString = '';


			                returnString = '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex].Comments + '</span></div>';

			                if (angular.isDefined(tddId2)) {
			                    returnString += '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId2 + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date2 + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex2].Comments + '</span></div>';
			                }
			                if (angular.isDefined(tddId3)) {
			                    returnString += '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId3 + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date3 + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex3].Comments + '</span></div>';
			                }
			                if (angular.isDefined(tddId4)) {
			                    returnString += '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId4 + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date4 + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex4].Comments + '</span></div>';
			                }

			                return returnString;
			            }

			            rowHtml += "<td>" + scope.studentresult.LastName + "," + scope.studentresult.FirstName + "</td>";

			            // add the comments column
			            for (var j = 0; j < scope.tdds.length; j++) {
			                currentColor = scope.tdds[j].Hex;
			                var currentTDDID = scope.tdds[j].Id;
			                for (var i = 0; i < scope.studentresult.FieldResultsByTestDueDate.length; i++) {

			                    if (scope.studentresult.FieldResultsByTestDueDate[i].TDDID === currentTDDID) {

			                        // do the comment here for each tdd, only add one icon
			                        if (scope.studentresult.FieldResultsByTestDueDate[i].Comments != null && scope.studentresult.FieldResultsByTestDueDate[i].Comments != '') {
			                            if (currentNoteText === '') {
			                                //currentNoteText += '<i class="fa fa-paperclip" style="cursor:pointer" popover-template="&apos;commentCell_' + scope.studentresult.StudentId + '_' + field.Id + '&apos;" popover-title="Comments"></i>';
			                                noteLeftTemplate = '<i popover-trigger="mouseenter" class="fa fa-comments" style="margin-left:5px;cursor:pointer" uib-popover-html="toolTipFunction(';
			                                noteRightTemplate = ');" popover-title="Comments"></i>';
			                            }
			                            var d = new Date(Date.parse(scope.tdds[j].DueDate));
			                            currentNoteText += "'" + scope.tdds[j].Id + "','" + (d.getMonth() + 1) + "/" + d.getFullYear() + "'," + i + ","; //"<div><span class='orange' style='border:1px solid #666666;display:inline-block;height:10px;width:25px;Background-color:" + scope.tdds[j].Hex + ";'></span><span>&nbsp;" + (d.getMonth() + 1) + "/" + d.getFullYear() + "</span>&nbsp;&nbsp;--&nbsp;&nbsp;<span style='font-weight:bold'>" + scope.studentresult.FieldResultsByTestDueDate[i].FieldResults[k].Comment + "</span></div>";
			                            //noteLeftTemplate += "busted"; 
			                        }
			                    }
			                }
			            }

			            // remove trailing comma on currentNotText
			            if (currentNoteText.length > 0) {
			                currentNoteText = currentNoteText.substring(0, currentNoteText.length - 1);
			            }

			            rowHtml += "<td class='nsCenterAlignedText'>" + noteLeftTemplate + currentNoteText + noteRightTemplate + "</td>";

			            // loop over the tdds for each studentresult and find the latest one, then get the hex for it?

			            for (var p = 0; p < scope.headerfields.length; p++) {
			                var field = scope.headerfields[p];
			                currentCellLeftHtml = '';
			                currentInnerText = '';
			                currentNoteText = '';
			                noteLeftTemplate = "";
			                noteRightTemplate = "";

			                for (var j = 0; j < scope.tdds.length; j++) {
			                    currentColor = scope.tdds[j].Hex;
			                    var currentTDDID = scope.tdds[j].Id;

			                    //TODO, need to do a foreach of the headerfields and output them



			                    for (var n = 0; n < scope.studentresult.SummaryFieldResults.length; n++) {
			                        // find the right field, then check to see if it should be colored in
			                        if (scope.studentresult.SummaryFieldResults[n].DbColumn === field.DatabaseColumn) {
			                            // NEW
			                            if (scope.studentresult.SummaryFieldResults[n].XColorDate != null) {
			                                var hexColor = '';
			                                for (var t = 0; t < scope.tdds.length; t++) {
			                                    if (scope.tdds[t].DueDate == scope.studentresult.SummaryFieldResults[n].XColorDate) {
			                                        hexColor = scope.tdds[t].Hex;
			                                    }
			                                }

			                                currentInnerText = "<span class='badge nsBadgeBackground' style='color:" + hexColor + "'>X</span>";
			                            }

			                            if (scope.studentresult.SummaryFieldResults[n].CellColorDate != null) {
			                                var hexColor = '';
			                                for (var t = 0; t < scope.tdds.length; t++) {
			                                    if (scope.tdds[t].DueDate == scope.studentresult.SummaryFieldResults[n].CellColorDate) {
			                                        hexColor = scope.tdds[t].Hex;
			                                    }
			                                }

			                                currentCellLeftHtml = "<td class='" + cssBorderClass + "' style='font-weight:bold;text-align:center;background-color:" + hexColor + "'>";
			                            }
			                        }
			                    }
			                }

			                currentCategory = scope.headerfields[p].SubcategoryId;
			                // add an empty cell if none of the test due dates are checked
			                if (currentCellLeftHtml === '') {
                                rowHtml += "<td></td>";
			                }
			                else {
    	                        rowHtml += currentCellLeftHtml + currentInnerText + currentCellRightHtml;
			                }
			            }

			            //element.html("cheese");
			            element.html(rowHtml);
			            $compile(element.contents())(scope);
			        }
			    };
			}
        ])
      .directive('lidSectionReportTableRow', [
			'$routeParams', '$compile', '$templateCache', '$http', function ($routeParams, $compile, $templateCache, $http) {


			    return {
			        restrict: 'A',
			        scope: {
			            headerfields: '=',
			            studentresult: '=',
			            tdds: '='
			        },
			        link: function (scope, element, attr) {

			            var currentColor = '';
			            //var previousColor = '';
			            var currentCellLeftHtml = '';
			            var currentCellRightHtml = '</td>';
			            var rowHtml = '';
			            var currentInnerText = '';
			            var currentNoteText = '';
			            var noteLeftTemplate = "";
			            var noteRightTemplate = '';
			            var currentCategory = 0;
			            var cssBorderClass = '';

			            scope.toolTipFunction = function (tddId, date, duedateIndex, resultIndex,
                                                          tddId2, date2, duedateIndex2, resultIndex2,
                                                          tddId3, date3, duedateIndex3, resultIndex3,
                                                          tddId4, date4, duedateIndex4, resultIndex4) {
			                var returnString = '';


			                returnString = '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex].FieldResults[resultIndex].Comment + '</span></div>';

			                if (angular.isDefined(tddId2)) {
			                    returnString += '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId2 + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date2 + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex2].FieldResults[resultIndex2].Comment + '</span></div>';
			                }
			                if (angular.isDefined(tddId3)) {
			                    returnString += '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId3 + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date3 + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex3].FieldResults[resultIndex3].Comment + '</span></div>';
			                }
			                if (angular.isDefined(tddId4)) {
			                    returnString += '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId4 + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date4 + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex4].FieldResults[resultIndex4].Comment + '</span></div>';
			                }

			                return returnString;
			            }

			            rowHtml += "<td><span student-dashboard-link student-id=\"" + scope.studentresult.StudentId + "\" student-name=\"'" + scope.studentresult.LastName.replace(/'/g, "\\'") + ", " + scope.studentresult.FirstName.replace(/'/g, "\\'") + "'\"></span></td>";
			            //rowHtml += "<td>" + scope.studentresult.LastName + "," + scope.studentresult.FirstName + "</td>";
			            // loop over the tdds for each studentresult and find the latest one, then get the hex for it?

			            for (var p = 0; p < scope.headerfields.length; p++) {
			                var field = scope.headerfields[p];
			                currentCellLeftHtml = '';
			                currentInnerText = '';
			                currentNoteText = '';
			                noteLeftTemplate = "";
			                noteRightTemplate = "";
			                cssBorderClass = '';

			                if (currentCategory != scope.headerfields[p].SubcategoryId) {
			                    cssBorderClass = 'leftDoubleBorder';
			                }
			                else if (p == scope.headerfields.length - 1) {
			                    cssBorderClass = 'rightDoubleBorder';
			                }

			                for (var j = 0; j < scope.tdds.length; j++) {
			                    currentColor = scope.tdds[j].Hex;
			                    var currentTDDID = scope.tdds[j].Id;

			                    //TODO, need to do a foreach of the headerfields and output them

			                    for (var i = 0; i < scope.studentresult.FieldResultsByTestDueDate.length; i++) {

			                        if (scope.studentresult.FieldResultsByTestDueDate[i].TDDID === currentTDDID) {
			                            // we've found the color for this one, do we color the cell or not?
			                            for (var k = 0; k < scope.studentresult.FieldResultsByTestDueDate[i].FieldResults.length; k++) {
			                                // find the right field, then check to see if it should be colored in
			                                if (scope.studentresult.FieldResultsByTestDueDate[i].FieldResults[k].GroupId === field.GroupId) {

			                                    // do the comment here for each tdd, only add one icon
			                                    if (scope.studentresult.FieldResultsByTestDueDate[i].FieldResults[k].Comment != null && scope.studentresult.FieldResultsByTestDueDate[i].FieldResults[k].Comment != '') {
			                                        if (currentNoteText === '') {
			                                            noteLeftTemplate = '<i popover-trigger="mouseenter" class="fa fa-comments" style="margin-left:5px;cursor:pointer" uib-popover-html="toolTipFunction(';
			                                            noteRightTemplate = ');" popover-title="Comments"></i>';
			                                        }
			                                        var d = new Date(Date.parse(scope.tdds[j].DueDate));
			                                        currentNoteText += "'" + scope.tdds[j].Id + "','" + (d.getMonth() + 1) + "/" + d.getFullYear() + "'," + i + "," + k + ",";
			                                    }

			                                }
			                            }
			                        }
			                    }

			                    for (var n = 0; n < scope.studentresult.SummaryFieldResults.length; n++) {
			                        // find the right field, then check to see if it should be colored in
			                        if (scope.studentresult.SummaryFieldResults[n].GroupId === field.GroupId) {
			                            // NEW
			                            if (scope.studentresult.SummaryFieldResults[n].XColorDate != null) {
			                                var hexColor = '';
			                                for (var t = 0; t < scope.tdds.length; t++) {
			                                    if (scope.tdds[t].DueDate == scope.studentresult.SummaryFieldResults[n].XColorDate) {
			                                        hexColor = scope.tdds[t].Hex;
			                                    }
			                                }

			                                currentInnerText = "<span style='color:" + hexColor + "'>X</span>";
			                            }

			                            if (scope.studentresult.SummaryFieldResults[n].CellColorDate != null) {
			                                var hexColor = '';
			                                for (var t = 0; t < scope.tdds.length; t++) {
			                                    if (scope.tdds[t].DueDate == scope.studentresult.SummaryFieldResults[n].CellColorDate) {
			                                        hexColor = scope.tdds[t].Hex;
			                                    }
			                                }

			                                currentCellLeftHtml = "<td class='" + cssBorderClass + "' style='font-weight:bold;text-align:center;background-color:" + hexColor + "'>";
			                            }
			                        }
			                    }
			                }

			                // remove trailing comma on currentNotText
			                if (currentNoteText.length > 0) {
			                    currentNoteText = currentNoteText.substring(0, currentNoteText.length - 1);
			                }

			                currentCategory = scope.headerfields[p].SubcategoryId;
			                // add an empty cell if none of the test due dates are checked
			                if (currentCellLeftHtml === '') {

			                    if (noteLeftTemplate === '') {
			                        rowHtml += "<td class='" + cssBorderClass + "' ></td>";
			                    }
			                    else {
			                        rowHtml += "<td class='" + cssBorderClass + "' >" + noteLeftTemplate + currentNoteText + noteRightTemplate + "</td>";
			                    }
			                }
			                else {
			                    if (noteLeftTemplate === '') {
			                        rowHtml += currentCellLeftHtml + currentInnerText + currentCellRightHtml;
			                    }
			                    else {
			                        rowHtml += currentCellLeftHtml + currentInnerText + noteLeftTemplate + currentNoteText + noteRightTemplate + currentCellRightHtml;
			                    }

			                }
			            }

			            //element.html("cheese");
			            element.html(rowHtml);
			            $compile(element.contents())(scope);
			        }
			    };
			}
      ])
        .controller('WVSectionReportController', WVSectionReportController)
        .controller('FPSectionReportController', FPSectionReportController)
        .controller('CAPSectionReportController', CAPSectionReportController)
        .controller('AVMRSectionReportController', AVMRSectionReportController)
        .controller('AVMRSingleDateSectionReportController', AVMRSingleDateSectionReportController)
        .controller('AVMRSingleDateDetailSectionReportController', AVMRSingleDateDetailSectionReportController)
        .controller('HRSIWSectionReportController', HRSIWSectionReportController)
        .controller('HRSIW2SectionReportController', HRSIW2SectionReportController)
        .controller('HRSIW3SectionReportController', HRSIW3SectionReportController)
        .controller('LIDSectionAlphaReportController', LIDSectionAlphaReportController)
        .controller('LIDSectionSoundReportController', LIDSectionSoundReportController)
        .controller('LIDSectionOverallReportController', LIDSectionOverallReportController)
        .controller('KNTCSectionReportController', KNTCSectionReportController)
        .controller('SpellingInventorySectionReportController', SpellingInventorySectionReportController);

    WVSectionReportController.$inject = ['$scope', '$q', '$http', 'nsPinesService', '$location', '$filter', '$routeParams', 'nsFilterOptionsService', 'WVReportManager', 'webApiBaseUrl', 'spinnerService', '$timeout', 'FileSaver', '$bootbox'];
    FPSectionReportController.$inject = ['$scope', '$q', '$http', 'nsPinesService', '$location', '$filter', '$routeParams', 'nsFilterOptionsService', 'FPReportManager', 'webApiBaseUrl', 'spinnerService', '$timeout', 'FileSaver', '$bootbox'];
    HRSIWSectionReportController.$inject = ['$scope', '$q', '$http', 'pinesNotifications', '$location', '$filter', '$routeParams', 'nsFilterOptionsService', 'HRSIWReportManager'];
    HRSIW2SectionReportController.$inject = ['$scope', '$q', '$http', 'pinesNotifications', '$location', '$filter', '$routeParams', 'nsFilterOptionsService', 'HRSIW2ReportManager'];
    HRSIW3SectionReportController.$inject = ['$scope', '$q', '$http', 'pinesNotifications', '$location', '$filter', '$routeParams', 'nsFilterOptionsService', 'HRSIW3ReportManager'];
    CAPSectionReportController.$inject = ['$scope', '$q', '$http', 'pinesNotifications', '$location', '$filter', '$routeParams', 'nsFilterOptionsService', 'webApiBaseUrl'];
    AVMRSectionReportController.$inject = ['$scope', '$q', '$http', 'pinesNotifications', '$location', '$filter', '$routeParams', 'nsFilterOptionsService', 'webApiBaseUrl'];
    AVMRSingleDateSectionReportController.$inject = ['$scope', '$q', '$http', 'pinesNotifications', '$location', '$filter', '$routeParams', 'nsFilterOptionsService', 'webApiBaseUrl', '$uibModal', '$timeout', 'AVMRDetailReportManager','spinnerService'];
    AVMRSingleDateDetailSectionReportController.$inject = ['$scope', '$q', '$http', 'pinesNotifications', '$location', '$filter', '$routeParams', 'nsFilterOptionsService', 'webApiBaseUrl', '$uibModal', '$timeout', 'AVMRDetailReportManager'];
    LIDSectionAlphaReportController.$inject = ['$scope', '$q', '$http', 'pinesNotifications', '$location', '$filter', '$routeParams', 'nsFilterOptionsService', 'LIDReportManager'];
    LIDSectionSoundReportController.$inject = ['$scope', '$q', '$http', 'pinesNotifications', '$location', '$filter', '$routeParams', 'nsFilterOptionsService', 'LIDReportManager'];
    LIDSectionOverallReportController.$inject = ['$scope', '$q', '$http', 'pinesNotifications', '$location', '$filter', '$routeParams', 'nsFilterOptionsService', 'LIDReportManager'];
    KNTCSectionReportController.$inject = ['$scope', '$q', '$http', 'pinesNotifications', '$location', '$filter', '$routeParams', 'nsFilterOptionsService', 'webApiBaseUrl', 'KNTCReportManager', 'NSSortManager', 'NSStudentAttributeLookups', 'spinnerService', '$timeout', 'NSObservationSummarySectionManager','$uibModal'];
    SpellingInventorySectionReportController.$inject = ['$scope', '$q', '$http', 'pinesNotifications', '$location', '$filter', '$routeParams','nsFilterOptionsService', 'webApiBaseUrl', 'SpellReportManager', 'NSSortManager'];

    function WVSectionReportController($scope, $q, $http, nsPinesService, $location, $filter, $routeParams, nsFilterOptionsService, WVReportManager, webApiBaseUrl, spinnerService, $timeout, FileSaver, $bootbox) {
        $scope.filterOptions = nsFilterOptionsService.options;
        $scope.dataMgr = new WVReportManager();
        

        $scope.$watch('filterOptions.selectedSection.id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                LoadData();
            }
        });

        var LoadData = function () {
            if ($scope.filterOptions.selectedSection != null) {
                $scope.dataMgr.LoadData($routeParams.id, $scope.filterOptions.selectedSection.id, $scope.filterOptions.selectedSchoolYear.id);
            }
        }

        // intial load if section already selected
        LoadData();
    }


    function FPSectionReportController($scope, $q, $http, nsPinesService, $location, $filter, $routeParams, nsFilterOptionsService, FPReportManager, webApiBaseUrl, spinnerService, $timeout, FileSaver, $bootbox) {
        $scope.filterOptions = nsFilterOptionsService.options;
        $scope.dataMgr = new FPReportManager();

        $scope.$watch('filterOptions.selectedSection.id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue) || $location.absUrl().indexOf('printmode=') >= 0) {
                LoadData();
            } 
        });

        $scope.lowBenchmark = function () {
            for (var p = 0; p < $scope.dataMgr.scale.length; p++) {
                if ($scope.dataMgr.soyBenchmark.Meets == $scope.dataMgr.scale[p].FPID) {
                    return $scope.dataMgr.scale[p].FPs;
                }
            }
        }

        $scope.highBenchmark = function () {
            for (var p = 0; p < $scope.dataMgr.scale.length; p++) {
                if ($scope.dataMgr.eoyBenchmark.Meets == $scope.dataMgr.scale[p].FPID) {
                    return $scope.dataMgr.scale[p].FPs;
                }
            }
        }

        $scope.targetZone = function () {
            var start = '', end = '';
            for (var p = 0; p < $scope.dataMgr.scale.length; p++) {
                if ($scope.dataMgr.targetZone.Meets == $scope.dataMgr.scale[p].FPID) {
                    start = $scope.dataMgr.scale[p].FPs;
                    break;
                }
            }

            for (var p = 0; p < $scope.dataMgr.scale.length; p++) {
                if ($scope.dataMgr.targetZone.Exceeds == $scope.dataMgr.scale[p].FPID) {
                    end = $scope.dataMgr.scale[p].FPs;
                    break;
                }
            }
            return start + '-' + end;
            //scope.scaleRow.FPID >= scope.targetZone.Meets && scope.scaleRow.FPID < scope.targetZone.Exceeds
        }
        var LoadData = function () {
            if ($scope.filterOptions.selectedSection != null) {
                $timeout(function () {
                    spinnerService.show('tableSpinner');
                });
                $scope.dataMgr.LoadData($routeParams.id, $scope.filterOptions.selectedSection.id, $scope.filterOptions.selectedSchoolYear.id)
                    .finally(function () {
                        spinnerService.hide('tableSpinner');
                    });
            }
        }

        // intial load if section already selected
        LoadData();
    }

    function HRSIWSectionReportController($scope, $q, $http, pinesNotifications, $location, $filter, $routeParams, nsFilterOptionsService, HRSIWReportManager) {
        $scope.reportType = "Alphabet Response";
        $scope.filterOptions = nsFilterOptionsService.options;
        $scope.dataMgr = new HRSIWReportManager();
        $scope.nameHeaderClass = "fa";

        $scope.showForm = function () {
            return $scope.filterOptions.selectedSection != null && $scope.filterOptions.selectedHrsForm != null;
        }

        $scope.$watch('filterOptions.selectedSection.id', function (newValue, oldValue) {
            if ((!angular.equals(newValue, oldValue) && newValue != null) || $location.absUrl().indexOf('printmode=') >= 0) {
                LoadData();
            }
        });

        $scope.$watch('filterOptions.selectedHrsForm.id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                LoadData();
            }
        });
         
        var LoadData = function () {
            // if this is a batchprint, go get formid
            var formId = $scope.filterOptions.selectedHrsForm == null ? 1 : $scope.filterOptions.selectedHrsForm.id;

            if ($location.absUrl().indexOf('batchprint') >= 0 && $scope.filterOptions.selectedSection != null) {
                $scope.dataMgr.getFormForClass($scope.filterOptions.selectedSection.id).then(function (response) {
                    formId = response.data.id;
                    $scope.filterOptions.selectedHrsForm = { id: formId, text: formId };
                    $scope.dataMgr.LoadData($routeParams.id, $scope.filterOptions.selectedSection.id, $scope.reportType, $scope.filterOptions.selectedSchoolYear.id, formId);
                });
            } else {
                if ($scope.filterOptions.selectedSection != null && $scope.filterOptions.selectedHrsForm != null) { //Todo: CHECK FORMID(assessmentId, sectionId, reportType, schoolYear)
                    $scope.dataMgr.LoadData($routeParams.id, $scope.filterOptions.selectedSection.id, $scope.reportType, $scope.filterOptions.selectedSchoolYear.id, formId);
                }
            }

        }

        // intial load if section already selected
        LoadData();
    }

    function HRSIW2SectionReportController($scope, $q, $http, pinesNotifications, $location, $filter, $routeParams, nsFilterOptionsService, HRSIW2ReportManager) {
        $scope.reportType = "Alphabet Response";
        $scope.filterOptions = nsFilterOptionsService.options;
        $scope.dataMgr = new HRSIW2ReportManager();
        $scope.nameHeaderClass = "fa";

        $scope.showForm = function () {
            return $scope.filterOptions.selectedSection != null;
        }

        $scope.$watch('filterOptions.selectedSection.id', function (newValue, oldValue) {
            if ((!angular.equals(newValue, oldValue) && newValue != null) || $location.absUrl().indexOf('printmode=') >= 0) {
                LoadData();
            }
        });

        $scope.$watch('filterOptions.selectedHrsForm2.id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                LoadData();
            }
        });

        var LoadData = function () {
            // if this is a batchprint, go get formid
            var formId = $scope.filterOptions.selectedHrsForm2 == null ? 1 : $scope.filterOptions.selectedHrsForm2.id;

            if ($location.absUrl().indexOf('batchprint') >= 0 && $scope.filterOptions.selectedSection != null) {
                $scope.dataMgr.getFormForClass($scope.filterOptions.selectedSection.id).then(function (response) {
                    formId = response.data.id;
                    $scope.filterOptions.selectedHrsForm2 = { id: formId, text: formId };
                    $scope.dataMgr.LoadData($routeParams.id, $scope.filterOptions.selectedSection.id, $scope.reportType, $scope.filterOptions.selectedSchoolYear.id, formId);
                });
            } else {
                if ($scope.filterOptions.selectedSection != null && $scope.filterOptions.selectedHrsForm2 != null) { //Todo: CHECK FORMID(assessmentId, sectionId, reportType, schoolYear)
                    $scope.dataMgr.LoadData($routeParams.id, $scope.filterOptions.selectedSection.id, $scope.reportType, $scope.filterOptions.selectedSchoolYear.id, formId);
                }
            }

        }

        // intial load if section already selected
        LoadData();
    }

    function HRSIW3SectionReportController($scope, $q, $http, pinesNotifications, $location, $filter, $routeParams, nsFilterOptionsService, HRSIW3ReportManager) {
        $scope.reportType = "Alphabet Response";
        $scope.filterOptions = nsFilterOptionsService.options;
        $scope.dataMgr = new HRSIW3ReportManager();
        $scope.nameHeaderClass = "fa";

        $scope.showForm = function () {
            return $scope.filterOptions.selectedSection != null;
        }

        $scope.$watch('filterOptions.selectedSection.id', function (newValue, oldValue) {
            if ((!angular.equals(newValue, oldValue) && newValue != null) || $location.absUrl().indexOf('printmode=') >= 0) {
                LoadData();
            }
        });

        $scope.$watch('filterOptions.selectedHrsForm3.id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                LoadData();
            }
        });

        var LoadData = function () {
            // if this is a batchprint, go get formid
            var formId = $scope.filterOptions.selectedHrsForm3 == null ? 1 : $scope.filterOptions.selectedHrsForm3.id;

            if ($location.absUrl().indexOf('batchprint') >= 0 && $scope.filterOptions.selectedSection != null) {
                $scope.dataMgr.getFormForClass($scope.filterOptions.selectedSection.id).then(function (response) {
                    formId = response.data.id;
                    $scope.filterOptions.selectedHrsForm3 = { id: formId, text: formId };
                    $scope.dataMgr.LoadData($routeParams.id, $scope.filterOptions.selectedSection.id, $scope.reportType, $scope.filterOptions.selectedSchoolYear.id, formId);
                });
            } else {
                if ($scope.filterOptions.selectedSection != null && $scope.filterOptions.selectedHrsForm3 != null) { //Todo: CHECK FORMID(assessmentId, sectionId, reportType, schoolYear)
                    $scope.dataMgr.LoadData($routeParams.id, $scope.filterOptions.selectedSection.id, $scope.reportType, $scope.filterOptions.selectedSchoolYear.id, formId);
                }
            }

        }

        // intial load if section already selected
        LoadData();
    }


    function LIDSectionAlphaReportController($scope, $q, $http, pinesNotifications, $location, $filter, $routeParams, nsFilterOptionsService, LIDReportManager) {
        $scope.reportType = "Alphabet Response";
        $scope.filterOptions = nsFilterOptionsService.options;
        $scope.dataMgr = new LIDReportManager();
        $scope.nameHeaderClass = "fa";

        $scope.$watch('filterOptions.selectedSection.id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue) || $location.absUrl().indexOf('printmode=') >= 0) {
                //var b = $httpParamSerializer($scope.filterOptions);
                LoadData();
            }
        });

        var LoadData = function () {          
            if ($scope.filterOptions.selectedSection != null) { //(assessmentId, sectionId, reportType, schoolYear)
                $scope.dataMgr.LoadData($routeParams.id, $scope.filterOptions.selectedSection.id, $scope.reportType, $scope.filterOptions.selectedSchoolYear.id);
            }
        }

        // intial load if section already selected
        LoadData();
    }

    function LIDSectionSoundReportController($scope, $q, $http, pinesNotifications, $location, $filter, $routeParams, nsFilterOptionsService, LIDReportManager) {
        $scope.reportType = "Sound Response";
        $scope.filterOptions = nsFilterOptionsService.options;
        $scope.dataMgr = new LIDReportManager();
        $scope.nameHeaderClass = "fa";

        $scope.$watch('filterOptions.selectedSection.id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue) || $location.absUrl().indexOf('printmode=') >= 0) {
                //var b = $httpParamSerializer($scope.filterOptions);
                LoadData();
            }
        });

        var LoadData = function () {
            if ($scope.filterOptions.selectedSection != null) { //(assessmentId, sectionId, reportType, schoolYear)
                $scope.dataMgr.LoadData($routeParams.id, $scope.filterOptions.selectedSection.id, $scope.reportType, $scope.filterOptions.selectedSchoolYear.id);
            }
        }

        // intial load if section already selected
        LoadData();
    }

    function LIDSectionOverallReportController($scope, $q, $http, pinesNotifications, $location, $filter, $routeParams, nsFilterOptionsService, LIDReportManager) {
        $scope.reportType = "Overall Response";
        $scope.filterOptions = nsFilterOptionsService.options;
        $scope.dataMgr = new LIDReportManager();
        $scope.nameHeaderClass = "fa";

        $scope.$watch('filterOptions.selectedSection.id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue) || $location.absUrl().indexOf('printmode=') >= 0) {
                //var b = $httpParamSerializer($scope.filterOptions);
                LoadData();
            }
        });

        var LoadData = function () {
            if ($scope.filterOptions.selectedSection != null) { //(assessmentId, sectionId, reportType, schoolYear)
                $scope.dataMgr.LoadData($routeParams.id, $scope.filterOptions.selectedSection.id, $scope.reportType, $scope.filterOptions.selectedSchoolYear.id);
            }
        }

        // intial load if section already selected
        LoadData();
    }

    function CAPSectionReportController($scope, $q, $http, pinesNotifications, $location, $filter, $routeParams, nsFilterOptionsService, webApiBaseUrl) {

        $scope.assessment = {};
        $scope.studentSectionReportResults = [];
        $scope.tdds = [];
        $scope.HeaderFields = [];
        $scope.filterOptions = nsFilterOptionsService.options;

        $scope.subCategoryColSpan = function (subCategoryId) {
            var colSpan = 0;

            for (var i = 0; i < $scope.HeaderFields.length; i++) {
                if ($scope.HeaderFields[i].SubcategoryId == subCategoryId) {
                    colSpan++;
                }
            }

            return colSpan;
        };

        var LoadData = function () {
            if ($scope.filterOptions.selectedSection != null) {
                var paramObj = { AssessmentId: $routeParams.id, SchoolYear: $scope.filterOptions.selectedSchoolYear.id, SectionId: $scope.filterOptions.selectedSection.id };

                $http.post(webApiBaseUrl + '/api/sectionreport/GetCAPSectionReport', paramObj).success(function (data) {
                    $scope.assessment = data.Assessment;
                    $scope.studentSectionReportResults = data.StudentSectionReportResults;
                    $scope.tdds = data.TestDueDates;

                    $scope.HeaderFields = data.HeaderFields;

                    var currentCssClass = '';
                    var currentSubCatId = 0;
                    for (var i = 0; i < $scope.HeaderFields.length; i++) {
                        if ($scope.HeaderFields[i].SubcategoryId != currentSubCatId) {
                            $scope.HeaderFields[i].cssClass = 'leftDoubleBorder';
                        }
                        else if (i === $scope.HeaderFields.length - 1) {
                            $scope.HeaderFields[i].cssClass = 'rightDoubleBorder';
                        }
                        currentSubCatId = $scope.HeaderFields[i].SubcategoryId;
                    }

                });
            }
        };

        $scope.$watch('filterOptions.selectedSection.id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue) || $location.absUrl().indexOf('printmode=') >= 0) {
                //var b = $httpParamSerializer($scope.filterOptions);
                LoadData();
            }
        });
        $scope.$watch('filterOptions.selectedBenchmarkDate.id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                //var b = $httpParamSerializer($scope.filterOptions);
                LoadData();
            }
        });
        

        $scope.sortArray = [];
        $scope.headerClassArray = [];
        $scope.nameHeaderClass = "fa";
        $scope.sort = function (column) {

            var columnIndex = -1; 
            // if this is not a first or lastname column
            if (!isNaN(parseInt(column))) {
                columnIndex = column;

                column = 'SummaryFieldResults[' + column + '].CellColorDate';

            }
            var bFound = false;
            for (var j = 0; j < $scope.sortArray.length; j++) {
                // if it is already on the list, reverse the sort
                if ($scope.sortArray[j].indexOf(column) >= 0) {
                    bFound = true;

                    // is it already negative? if so, remove it
                    if ($scope.sortArray[j].indexOf("-") === 0) {
                        if (columnIndex > -1) {
                            $scope.headerClassArray[columnIndex] = "fa";
                        } else if (column === 'LastName') {
                            $scope.nameHeaderClass = "fa";
                        } 
                        $scope.sortArray.splice(j, 1);
                    } else {
                        if (columnIndex > -1) {
                            $scope.headerClassArray[columnIndex] = "fa fa-chevron-down";
                        } else if (column === 'LastName') {
                            $scope.nameHeaderClass = "fa fa-chevron-down";
                        } 
                        $scope.sortArray[j] = "-" + $scope.sortArray[j];
                    }
                    break;
                }
            }
            if (!bFound) {
                $scope.sortArray.push(column);

                if (columnIndex > -1) {
                    $scope.headerClassArray[columnIndex] = "fa fa-chevron-up";
                } else if (column === 'LastName') {
                    $scope.nameHeaderClass = "fa fa-chevron-up";
                } 
            }
        };

        for (var r = 0; r < $scope.HeaderFields.length; r++) {
            $scope.headerClassArray[r] = 'fa';
        }

        LoadData();
    }

    function AVMRSingleDateSectionReportController($scope, $q, $http, pinesNotifications, $location, $filter, $routeParams, nsFilterOptionsService, webApiBaseUrl, $uibModal, $timeout, AVMRDetailReportManager, spinnerService) {

        $scope.assessment = {};
        $scope.studentSectionReportResults = [];
        $scope.HeaderFields = [];
        $scope.filterOptions = nsFilterOptionsService.options;
        $scope.detailReportMgr = new AVMRDetailReportManager();

        $scope.subCategoryColSpan = function (subCategoryId) {
            var colSpan = 0;

            for (var i = 0; i < $scope.HeaderFields.length; i++) {
                if ($scope.HeaderFields[i].SubcategoryId == subCategoryId) {
                    colSpan++;
                }
            }

            return colSpan;
        };

        $scope.detailPopup = function (groupId, altDisplayLabel) {
            var modalInstance = $uibModal.open({
                templateUrl: 'avmrDetailDialog.html',
                scope: $scope,
                controller: function ($scope, $uibModalInstance) {

                    $timeout(function () {
                        $('div.modal-dialog').addClass('modal-dialog-max');
                        $('div.modal-content').addClass('modal-content-max');
                    }, 250);

                    $scope.detailReportMgr.FilterData(groupId, altDisplayLabel);

                    $scope.cancel = function () {
                        $uibModalInstance.dismiss('cancel');
                    };

                    $scope.avmrDetailPrintUrl = function () {
                        var port = $location.port() == "443" || $location.port() == "80" ? "" : ":" + $location.port();

                        return $location.protocol() + '://' + $location.host() + port + '/#/section-report-avmr-structured-detail/' + $routeParams.id + '/' + $scope.filterOptions.selectedSection.id + '/' + $scope.filterOptions.selectedBenchmarkDate.id + '/' + groupId + '/' + altDisplayLabel;
                    }

                },
                size: 'md',
            });
        }

        var getGroupContainerId = function (groupId) {
            for (var i = 0; i < $scope.assessment.FieldGroups.length; i++) {
                if ($scope.assessment.FieldGroups[i].Id == groupId) {
                    return $scope.assessment.FieldGroups[i].AssessmentFieldGroupContainerId;
                }
            }
        }


        $scope.fieldGroupContainerColSpan = function (containerId) {
            var colSpan = 0;

            var groupsForThisContainer = [];
            for (var i = 0; i < $scope.assessment.FieldGroups.length; i++) {
                if ($scope.assessment.FieldGroups[i].AssessmentFieldGroupContainerId == containerId) {
                    groupsForThisContainer.push($scope.assessment.FieldGroups[i]);
                }
            }
            // we now hav an array of all of the groups for this container... loop over them and gets all headerfiels with any of these groupIDs
            for (var k = 0; k < groupsForThisContainer.length; k++) {
                for (var i = 0; i < $scope.HeaderFields.length; i++) {
                    if ($scope.HeaderFields[i].GroupId == groupsForThisContainer[k].Id) {
                        colSpan++;
                    }
                }
            }

            return colSpan;
        }

        $scope.navigateToTdd = function (tddid) {
            $scope.filterOptions.selectedBenchmarkDate = nsFilterOptionsService.getBenchmarkDateById(tddid);
            nsFilterOptionsService.changeBenchmarkDate();
        }

        var LoadData = function () {
            if ($scope.filterOptions.selectedSection != null && $scope.filterOptions.selectedBenchmarkDate != null) {
                var paramObj = { AssessmentId: $routeParams.id, SectionId: $scope.filterOptions.selectedSection.id, BenchmarkDateId: $scope.filterOptions.selectedBenchmarkDate.id };

                spinnerService.show('tableSpinner');

                $http.post(webApiBaseUrl + '/api/sectionreport/GetAVMRSingleDateSectionReport', paramObj)
                    .then(function (response) {
                        $scope.assessment = response.data.Assessment;
                        $scope.studentSectionReportResults = response.data.StudentSectionReportResults;
                        $scope.tdds = response.data.TestDueDates;

                        $scope.HeaderFields = response.data.HeaderFields;

                        var currentCssClass = '';
                        var lastGroupContainerId = 0;
                        var numberInContainer = 0;
                        for (var i = 0; i < $scope.HeaderFields.length; i++) {
                            var currentGroupContainerId = getGroupContainerId($scope.HeaderFields[i].GroupId);
                            $scope.HeaderFields[i].GroupContainerId = currentGroupContainerId;

                            if (currentGroupContainerId != lastGroupContainerId) {
                                $scope.HeaderFields[i].cssClass = 'leftDoubleBorder';
                                numberInContainer = 1;

                                if (i == $scope.HeaderFields.length - 1) {
                                    $scope.HeaderFields[i].cssClass = 'leftDoubleBorder rightDoubleBorder';
                                }
                            }
                            else if (i == $scope.HeaderFields.length -1) {
                                if (numberInContainer == 1) {
                                    $scope.HeaderFields[i].cssClass = 'leftDoubleBorder rightDoubleBorder';
                                } else {
                                    $scope.HeaderFields[i].cssClass = 'rightDoubleBorder';
                                }
                            }
                            numberInContainer++;
                            lastGroupContainerId = getGroupContainerId($scope.HeaderFields[i].GroupId);
                        }

                        var currentSortOrder = 0;
                        for (var i = 0; i < $scope.assessment.FieldGroupContainers.length; i++) {
                            if ($scope.assessment.FieldGroupContainers[i].SortOrder >= currentSortOrder) {
                                $scope.assessment.FieldGroupContainers[i].cssClass = 'leftDoubleBorder';
                            }
                            if (i === $scope.assessment.FieldGroupContainers.length - 1) {
                                $scope.assessment.FieldGroupContainers[i].cssClass = 'leftDoubleBorder rightDoubleBorder';
                            }
                            currentSortOrder = $scope.assessment.FieldGroupContainers[i].SortOrder;
                        }
                    }).then(function () {
                        // load detail data
                        $scope.detailReportMgr.LoadData($routeParams.id, $scope.filterOptions.selectedSection.id, $scope.filterOptions.selectedBenchmarkDate.id);
                    }).finally( function () {
                        spinnerService.hide('tableSpinner');
                    });
            }
        };

        $scope.$watchGroup(['filterOptions.selectedSection.id', 'filterOptions.selectedBenchmarkDate.id'], function (newValue, oldValue, scope) {
            if (angular.isDefined(newValue[0]) && angular.isDefined(newValue[1])) {
                if (newValue[0] != oldValue[0] || newValue[1] != oldValue[1]) {
                    LoadData();
                }
            }
        });
        //$scope.$watch('filterOptions.selectedSection.id', function (newValue, oldValue) {
        //    if (!angular.equals(newValue, oldValue) || $location.absUrl().indexOf('printmode=') >= 0) {
        //        //var b = $httpParamSerializer($scope.filterOptions);
        //        LoadData();
        //    }
        //});
        //$scope.$watch('filterOptions.selectedBenchmarkDate.id', function (newValue, oldValue) {
        //    if (!angular.equals(newValue, oldValue)) {
        //        //var b = $httpParamSerializer($scope.filterOptions);
        //        LoadData();
        //    }
        //});


        $scope.sortArray = [];
        $scope.headerClassArray = [];
        $scope.nameHeaderClass = "fa";
        $scope.sort = function (column) {

            var columnIndex = -1;
            // if this is not a first or lastname column
            if (!isNaN(parseInt(column))) {
                columnIndex = column;

                column = 'SummaryFieldResults[' + column + '].CellColorDate';

            }
            var bFound = false;
            for (var j = 0; j < $scope.sortArray.length; j++) {
                // if it is already on the list, reverse the sort
                if ($scope.sortArray[j].indexOf(column) >= 0) {
                    bFound = true;

                    // is it already negative? if so, remove it
                    if ($scope.sortArray[j].indexOf("-") === 0) {
                        if (columnIndex > -1) {
                            $scope.headerClassArray[columnIndex] = "fa";
                        } else if (column === 'LastName') {
                            $scope.nameHeaderClass = "fa";
                        }
                        $scope.sortArray.splice(j, 1);
                    } else {
                        if (columnIndex > -1) {
                            $scope.headerClassArray[columnIndex] = "fa fa-chevron-down";
                        } else if (column === 'LastName') {
                            $scope.nameHeaderClass = "fa fa-chevron-down";
                        }
                        $scope.sortArray[j] = "-" + $scope.sortArray[j];
                    }
                    break;
                }
            }
            if (!bFound) {
                $scope.sortArray.push(column);

                if (columnIndex > -1) {
                    $scope.headerClassArray[columnIndex] = "fa fa-chevron-up";
                } else if (column === 'LastName') {
                    $scope.nameHeaderClass = "fa fa-chevron-up";
                }
            }
        };

        for (var r = 0; r < $scope.HeaderFields.length; r++) {
            $scope.headerClassArray[r] = 'fa';
        }

        LoadData();
    }


    function AVMRSingleDateDetailSectionReportController($scope, $q, $http, pinesNotifications, $location, $filter, $routeParams, nsFilterOptionsService, webApiBaseUrl, $uibModal, $timeout, AVMRDetailReportManager) {

      
        $scope.detailReportMgr = new AVMRDetailReportManager();
        $scope.filterOptions = nsFilterOptionsService.options;
      

        $scope.$watch('filterOptions.selectedBenchmarkDate.id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                LoadData();
            }
        });

        $scope.navigateToTdd = function (tddid) {
            $scope.filterOptions.selectedBenchmarkDate = nsFilterOptionsService.getBenchmarkDateById(tddid);
            nsFilterOptionsService.changeBenchmarkDate();
        }

        var LoadData = function () {
            if ($scope.filterOptions.selectedSection != null && $scope.filterOptions.selectedBenchmarkDate != null) {
                $scope.detailReportMgr.LoadData($routeParams.assessmentId, $scope.filterOptions.selectedSection.id, $scope.filterOptions.selectedBenchmarkDate.id).then(function () {
                    $scope.detailReportMgr.FilterData($routeParams.groupId, $routeParams.altDisplayLabel);
                })
            }
        };

        $scope.$watch('filterOptions.selectedSection.id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue) || $location.absUrl().indexOf('printmode=') >= 0) {
                LoadData();
            }
        });
        $scope.$watch('filterOptions.selectedBenchmarkDate.id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                LoadData();
            }
        });
      
        LoadData();
    }

    function AVMRSectionReportController($scope, $q, $http, pinesNotifications, $location, $filter, $routeParams, nsFilterOptionsService, webApiBaseUrl) {

        $scope.assessment = {};
        $scope.studentSectionReportResults = [];
        $scope.tdds = [];
        $scope.HeaderFields = [];
        $scope.filterOptions = nsFilterOptionsService.options;

        $scope.subCategoryColSpan = function (subCategoryId) {
            var colSpan = 0;

            for (var i = 0; i < $scope.HeaderFields.length; i++) {
                if ($scope.HeaderFields[i].SubcategoryId == subCategoryId) {
                    colSpan++;
                }
            }

            return colSpan;
        };

        var getGroupContainerId = function (groupId) {
            for(var i = 0; i < $scope.assessment.FieldGroups.length; i++) {
                if ($scope.assessment.FieldGroups[i].Id == groupId) {
                    return $scope.assessment.FieldGroups[i].AssessmentFieldGroupContainerId;
                }
            }
        }

        $scope.fieldGroupContainerColSpan = function (containerId) {
            var colSpan = 0;

            var groupsForThisContainer = [];
            for (var i = 0; i < $scope.assessment.FieldGroups.length; i++) {
                if ($scope.assessment.FieldGroups[i].AssessmentFieldGroupContainerId == containerId) {
                    groupsForThisContainer.push($scope.assessment.FieldGroups[i]);
                }
            }
            // we now hav an array of all of the groups for this container... loop over them and gets all headerfiels with any of these groupIDs
            for (var k = 0; k < groupsForThisContainer.length; k++) {
                for (var i = 0; i < $scope.HeaderFields.length; i++) {
                    if ($scope.HeaderFields[i].GroupId == groupsForThisContainer[k].Id) {
                        colSpan++;
                    }
                }
            }

            return colSpan;
        }

        var LoadData = function () {
            if ($scope.filterOptions.selectedSection != null) {
                var paramObj = { AssessmentId: $routeParams.id, SchoolYear: $scope.filterOptions.selectedSchoolYear.id, SectionId: $scope.filterOptions.selectedSection.id };

                $http.post(webApiBaseUrl + '/api/sectionreport/GetAVMRSectionReport', paramObj).success(function (data) {
                    $scope.assessment = data.Assessment;
                    $scope.studentSectionReportResults = data.StudentSectionReportResults;
                    $scope.tdds = data.TestDueDates;

                    $scope.HeaderFields = data.HeaderFields;

                    var currentCssClass = '';
                    var lastGroupContainerId = 0;
                    var numberInContainer = 0;
                    for (var i = 0; i < $scope.HeaderFields.length; i++) {
                        var currentGroupContainerId = getGroupContainerId($scope.HeaderFields[i].GroupId);

                        $scope.HeaderFields[i].GroupContainerId = currentGroupContainerId;

                        if (currentGroupContainerId != lastGroupContainerId) {
                            $scope.HeaderFields[i].cssClass = 'leftDoubleBorder';
                            numberInContainer = 1;

                            if (i == $scope.HeaderFields.length - 1) {
                                $scope.HeaderFields[i].cssClass = 'leftDoubleBorder rightDoubleBorder';
                            }
                        }
                        else if (i == $scope.HeaderFields.length - 1) {
                            if (numberInContainer == 1) {
                                $scope.HeaderFields[i].cssClass = 'leftDoubleBorder rightDoubleBorder';
                            } else {
                                $scope.HeaderFields[i].cssClass = 'rightDoubleBorder';
                            }
                        }
                        numberInContainer++;
                        lastGroupContainerId = getGroupContainerId($scope.HeaderFields[i].GroupId);
                    }

                    var currentSortOrder = 0;
                        for (var i = 0; i < $scope.assessment.FieldGroupContainers.length; i++) {
                            if ($scope.assessment.FieldGroupContainers[i].SortOrder >= currentSortOrder) {
                                $scope.assessment.FieldGroupContainers[i].cssClass = 'leftDoubleBorder';
                            }
                            if (i === $scope.assessment.FieldGroupContainers.length -1) {
                                $scope.assessment.FieldGroupContainers[i].cssClass = 'leftDoubleBorder rightDoubleBorder';
                            }
                            currentSortOrder = $scope.assessment.FieldGroupContainers[i].SortOrder;
        }

                });
            }
        };

        $scope.$watch('filterOptions.selectedSection.id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue) || $location.absUrl().indexOf('printmode=') >= 0) {
                //var b = $httpParamSerializer($scope.filterOptions);
                LoadData();
            }
        });
        $scope.$watch('filterOptions.selectedBenchmarkDate.id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                //var b = $httpParamSerializer($scope.filterOptions);
                LoadData();
            }
        });


        $scope.sortArray = [];
        $scope.headerClassArray = [];
        $scope.nameHeaderClass = "fa";
        $scope.sort = function (column) {

            var columnIndex = -1;
            // if this is not a first or lastname column
            if (!isNaN(parseInt(column))) {
                columnIndex = column;

                column = 'SummaryFieldResults[' + column + '].CellColorDate';

            }
            var bFound = false;
            for (var j = 0; j < $scope.sortArray.length; j++) {
                // if it is already on the list, reverse the sort
                if ($scope.sortArray[j].indexOf(column) >= 0) {
                    bFound = true;

                    // is it already negative? if so, remove it
                    if ($scope.sortArray[j].indexOf("-") === 0) {
                        if (columnIndex > -1) {
                            $scope.headerClassArray[columnIndex] = "fa";
                        } else if (column === 'LastName') {
                            $scope.nameHeaderClass = "fa";
                        }
                        $scope.sortArray.splice(j, 1);
                    } else {
                        if (columnIndex > -1) {
                            $scope.headerClassArray[columnIndex] = "fa fa-chevron-down";
                        } else if (column === 'LastName') {
                            $scope.nameHeaderClass = "fa fa-chevron-down";
                        }
                        $scope.sortArray[j] = "-" + $scope.sortArray[j];
                    }
                    break;
                }
            }
            if (!bFound) {
                $scope.sortArray.push(column);

                if (columnIndex > -1) {
                    $scope.headerClassArray[columnIndex] = "fa fa-chevron-up";
                } else if (column === 'LastName') {
                    $scope.nameHeaderClass = "fa fa-chevron-up";
                }
            }
        };

        for (var r = 0; r < $scope.HeaderFields.length; r++) {
            $scope.headerClassArray[r] = 'fa';
        }

        LoadData();
    }


    function SpellingInventorySectionReportController($scope, $q, $http, pinesNotifications, $location, $filter, $routeParams, nsFilterOptionsService, webApiBaseUrl, SpellReportManager, NSSortManager) {
        $scope.filterOptions = nsFilterOptionsService.options;
        $scope.dataMgr = new SpellReportManager();

        $scope.manualSortHeaders = {};
        $scope.manualSortHeaders.studentNameHeaderClass = "fa";
        $scope.manualSortHeaders.fpValueIDHeaderClass = "fa";
        $scope.manualSortHeaders.totalScoreHeaderClass = "fa";
        $scope.sortArray = [];
        $scope.headerClassArray = [];
        $scope.allSelected = false;
        $scope.sortMgr = new NSSortManager();

        $scope.$watch('filterOptions.selectedSection.id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue) || $location.absUrl().indexOf('printmode=') >= 0) {
                LoadData();
            }
        });

        $scope.spellingCellBgColor = function (fieldResult) {
            if (fieldResult.IntValue != null) {
                if (fieldResult.Field.OutOfHowMany - fieldResult.IntValue <= 1) {
                    return 'obsGreen';
                } else if (fieldResult.Field.OutOfHowMany - fieldResult.IntValue <= 2) {
                    return 'obsYellow';
                } else {
                    return '';
                }
            }
        };

        $scope.$watch('filterOptions.selectedBenchmarkDate.id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                LoadData();
            }
        });

        $scope.navigateToTdd = function (tddid) {
            $scope.filterOptions.selectedBenchmarkDate = nsFilterOptionsService.getBenchmarkDateById(tddid);
            nsFilterOptionsService.changeBenchmarkDate();
        }

        var LoadData = function () {
            if ($scope.filterOptions.selectedSection != null && $scope.filterOptions.selectedBenchmarkDate != null) {
                $scope.dataMgr.LoadData($routeParams.id, $scope.filterOptions.selectedSection.id, $scope.filterOptions.selectedBenchmarkDate.id).then(function (response) {
                    $scope.sortMgr.initialize($scope.manualSortHeaders, $scope.sortArray, $scope.headerClassArray, 'FieldResults', $scope.dataMgr.assessment);
                });
            }
        }

        $scope.totalFPAndWords =  function(studentResult) {
            var totalScore = '0';
            var totalWSC = '0';
            var totalFP = '0';
            // do some error checking, etc to make sure IntValue is populated, etc
            for(var i=0;i<studentResult.FieldResults.length;i++)
            {
                if(studentResult.FieldResults[i].DbColumn === 'totalScore') {
                    totalScore = (studentResult.FieldResults[i].IntValue === null ? '0' : studentResult.FieldResults[i].IntValue);
                }
                else if(studentResult.FieldResults[i].DbColumn === 'totalWSC') {
                    totalWSC = (studentResult.FieldResults[i].IntValue === null ? '0' : studentResult.FieldResults[i].IntValue);
                }
                else if(studentResult.FieldResults[i].DbColumn === 'totalFP') {
                    totalFP = (studentResult.FieldResults[i].IntValue === null ? '0' : studentResult.FieldResults[i].IntValue);
                }
            }

            return totalScore + ' (' + totalFP + '/' + totalWSC + ')';
        }

       
        // intial load if section already selected
        LoadData();

        $scope.sort = function (column) {
            if (!isNaN(column)) {
                $scope.sortMgr.funkySort(column, column + 1);
            }
            else {
                $scope.sortMgr.sort(column);
            }
        };
    }


    function KNTCSectionReportController($scope, $q, $http, pinesNotifications, $location, $filter, $routeParams, nsFilterOptionsService, webApiBaseUrl, KNTCReportManager, NSSortManager, NSStudentAttributeLookups, spinnerService, $timeout, NSObservationSummarySectionManager, $uibModal) {
        $scope.filterOptions = nsFilterOptionsService.options;
        $scope.dataMgr = new KNTCReportManager();

        $scope.manualSortHeaders = {};
        $scope.manualSortHeaders.studentNameHeaderClass = "fa";
        $scope.manualSortHeaders.fpValueIDHeaderClass = "fa";
        $scope.manualSortHeaders.totalScoreHeaderClass = "fa";
        $scope.manualSortHeaders.guidedReadingGroupClass = "fa";
        $scope.sortArray = [];
        $scope.headerClassArray = [];
        $scope.allSelected = false;
        $scope.sortMgr = new NSSortManager();
        $scope.allAttributes = new NSStudentAttributeLookups();
        $scope.observationSummaryManager = new NSObservationSummarySectionManager();
        $scope.sourceTddSettings = { };
         // set default

        $scope.$watch('filterOptions.selectedSection.id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue) || $location.absUrl().indexOf('printmode=') >= 0) {
                LoadData();
            }
        });

        var attachFieldsCallback = function () {
            for (var j = 0; j < $scope.observationSummaryManager.Scores.StudentResults.length; j++) {
                for (var k = 0; k < $scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults.length; k++) {
                    for (var i = 0; i < $scope.observationSummaryManager.Scores.Fields.length; i++) {
                        if ($scope.observationSummaryManager.Scores.Fields[i].DatabaseColumn == $scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].DbColumn) {
                            $scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].Field = angular.copy($scope.observationSummaryManager.Scores.Fields[i]);
                        }
                    }
                }
            }
        }

        $scope.openSourceTddDialog = function () {
            // set settings
            //$scope.copySettings.copyAll = copyAll;
            //$scope.copySettings.targetResult = studentResult;

            var modalInstance = $uibModal.open({
                templateUrl: 'sourceTddChooser.html',
                scope: $scope,
                controller: function ($scope, $uibModalInstance) {
                    $scope.setSourceDate = function () {

                        // validate that a date is selected and it isn't the current date
                        if (!$scope.sourceTddSettings.newSourceDate) {
                            alert("Please select a valid source benchmark date.");
                            return;
                        }

                        $scope.sourceTddSettings.sourceDate = $scope.sourceTddSettings.newSourceDate;
                        $uibModalInstance.dismiss('cancel');
                        LoadData();
                    };
                    $scope.cancel = function () {
                        $uibModalInstance.dismiss('cancel');
                    };
                },
                size: 'md',
            });
        }

        $scope.guidedReadingColor = function (fieldValue) {
            switch (fieldValue) {
                case 1:
                    return 'grRed';
                case 2:
                    return 'grOrange';
                case 3:
                    return 'grYellow';
                case 4:
                    return 'grGreen';
                case 5:
                    return 'grBlue';
                case 6:
                    return 'grPurple';
                default:
                    return '';
            }
        }

        $scope.guidedReadingText = function (fieldValue) {
            switch (fieldValue) {
                case 1:
                    return 'Group 1';
                case 2:
                    return 'Group 2';
                case 3:
                    return 'Group 3';
                case 4:
                    return 'Group 4';
                case 5:
                    return 'Group 5';
                case 6:
                    return 'Group 6';
                default:
                    return 'N/A';
            }
        }

        $scope.getAttributeLookupValue = function (studentId, attributeId) {
            var attributeIndex = -1;
            for (var p = 0; p < $scope.allAttributes.AllAttributes.length; p++) {
                if (attributeId == $scope.allAttributes.AllAttributes[p].Id) {
                    attributeIndex = p;
                    break;
                }
            }

            // if this attribute doesn't exist (fringe case)
            if (p == -1) {
                return '';
            }

            for (var i = 0; i < $scope.dataMgr.StudentAttributes.length; i++) {
                if ($scope.dataMgr.StudentAttributes[i].StudentId == studentId) {
                    // now have student selected
                    var currentAttribute = $scope.allAttributes.AllAttributes[attributeIndex];
                    // if student even has this attribute
                    if (typeof ($scope.dataMgr.StudentAttributes[i][attributeId]) != 'undefined') {
                        var currentStudentLookupId = $scope.dataMgr.StudentAttributes[i][attributeId];
                        return currentStudentLookupId;
                    }
                }
            }
            return '';
        }

        $scope.getAttributeValue = function (studentId, attributeId) {
            var attributeIndex = -1;
            for (var p = 0; p < $scope.allAttributes.AllAttributes.length; p++){
                if (attributeId == $scope.allAttributes.AllAttributes[p].Id) {
                    attributeIndex = p;
                    break;
                }
            }

            // if this attribute doesn't exist (fringe case)
            if (p == -1) {
                return '';
            }

            for (var i = 0; i < $scope.dataMgr.StudentAttributes.length; i++) {
                if ($scope.dataMgr.StudentAttributes[i].StudentId == studentId) {
                    // now have student selected
                    var currentAttribute = $scope.allAttributes.AllAttributes[attributeIndex];
                    // if student even has this attribute
                    if (typeof ($scope.dataMgr.StudentAttributes[i][attributeId]) != 'undefined') {
                        var currentStudentLookupId = $scope.dataMgr.StudentAttributes[i][attributeId];

                        for (var m = 0; m < $scope.allAttributes.AllAttributes[attributeIndex].LookupValues.length; m++) {
                            if ($scope.allAttributes.AllAttributes[attributeIndex].LookupValues[m].LookupValueId == currentStudentLookupId) {
                                return $scope.allAttributes.AllAttributes[attributeIndex].LookupValues[m].LookupValue;
                            }
                        }
                    }
                }
            }
            return '';
        }


        $scope.$watch('filterOptions.selectedBenchmarkDate.id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                LoadData();
            }
        });

        $scope.navigateToTdd = function (tddid) {
            $scope.filterOptions.selectedBenchmarkDate = nsFilterOptionsService.getBenchmarkDateById(tddid);
            nsFilterOptionsService.changeBenchmarkDate();
        }

        $scope.$on('NSFieldsUpdated', function (event, data) {
            LoadData();
        });

        var LoadData = function () {
            // get sourcedate object from url if exists
            if ($location.$$search.SourceBenchmarkDate) {
                $scope.sourceTddSettings.sourceDate = JSON.parse(decodeURIComponent($location.$$search.SourceBenchmarkDate));
            }

            if ($scope.filterOptions.selectedSection != null && $scope.filterOptions.selectedBenchmarkDate != null) {
                if (!$scope.sourceTddSettings.sourceDate) {
                    $scope.sourceTddSettings.sourceDate = $scope.filterOptions.selectedBenchmarkDate; // set default source date
                }

                $timeout(function () {
                    spinnerService.show('tableSpinner');
                });
                $scope.dataMgr.LoadData($routeParams.id, $scope.filterOptions.selectedSection.id, $scope.filterOptions.selectedSchoolYear.id, $scope.filterOptions.selectedBenchmarkDate.id).then(function (response) {
                    $scope.sortMgr.initialize($scope.manualSortHeaders, $scope.sortArray, $scope.headerClassArray, 'FieldResults', $scope.dataMgr.assessment);
                }).finally(function () {
                    $scope.observationSummaryManager.LoadData($scope.filterOptions.selectedSection.id, $scope.sourceTddSettings.sourceDate.id).then(function (response) {
                        attachFieldsCallback();
                        spinnerService.hide('tableSpinner');
                    });
                });
            }
        }

  


        // intial load if section already selected
        LoadData();

        $scope.sort = function (column) {
            if (!isNaN(column)) {
                $scope.sortMgr.funkySort(column, column + 1);
            }
            else {
                $scope.sortMgr.sort(column);
            }
        };
    }


})();