(function () {
    'use strict';

    angular
        .module('sectionReportsModule', [])
    //    .run(["$templateCache", function ($templateCache) {
    //$templateCache.put("template/popover/popover.html",
    //  "<div class=\"popover {{placement}}\" ng-class=\"{ in: isOpen(), fade: animation() }\">\n" +
    //  "  <div class=\"arrow\"></div>\n" +
    //  "\n" +
    //  "  <div class=\"popover-inner\">\n" +
    //  "      <h3 class=\"popover-title\" ng-bind-html=\"title | unsafe\" ng-show=\"title\"></h3>\n" +
    //  "      <div class=\"popover-content\"ng-bind-html=\"content | unsafe\"></div>\n" +
    //  "  </div>\n" +
    //  "</div>\n" +
    //  "");
    //    }])
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
                            }
                    );
                };


            };

            return (FPReportManager);
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
                                //for (var i = 0; i < self.HeaderFields.length; i++) {
                                //    if (self.HeaderFields[i].SubcategoryId != currentSubCatId) {
                                //        self.HeaderFields[i].cssClass = 'leftDoubleBorder';
                                //    }
                                //    else if (i === self.HeaderFields.length - 1) {
                                //        self.HeaderFields[i].cssClass = 'rightDoubleBorder';
                                //    }
                                //    currentSubCatId = self.HeaderFields[i].SubcategoryId;
                                //}

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

            return (HRSIWReportManager);
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
                                    "/" + 
                                    scope.interventionRecords[i].StaffInitials +
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
			            allStudentServices: '='
			        },
			        link: function (scope, element, attr) {
			            var recordHtml = '';

			            var studentServiceString = '';

			            for (var i = 0; i < scope.allStudentServices.length; i++) {
			                if (scope.allStudentServices[i].StudentId == scope.studentId) {
			                    studentServiceString += '<span title="' + scope.allStudentServices[i].Description + '">' + scope.allStudentServices[i].Label +
                                    '</span><br />';
                                    
			                }
			            }
			            element.html(studentServiceString);
			            $compile(element.contents())(scope);
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
			'$routeParams', '$compile', '$templateCache', '$http', function ($routeParams, $compile, $templateCache, $http) {


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

			                // target zone
			                if (scope.scaleRow.FPID >= scope.targetZone.Meets && scope.scaleRow.FPID < scope.targetZone.Exceeds) {
			                        cellStyle = 'targetZone';
			                }
			                // eoy
			                if (scope.scaleRow.FPID == scope.eoyBenchmark.Meets + 1) {
			                    cellStyle += ' eoyBenchmark';
			                }
			                // soy
			                if (scope.scaleRow.FPID == scope.soyBenchmark.Meets - 1) {
			                    cellStyle += ' soyBenchmark';
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

			                //for (var j = 0; j < scope.tdds.length; j++) {
			                //    var currentTDDID = scope.tdds[j].Id;

			                //    //TODO, need to do a foreach of the headerfields and output them
			                //    for (var i = 0; i < studentresult.FieldResultsByTestDueDate.length; i++) {

			                //        if (studentresult.FieldResultsByTestDueDate[i].TDDID === currentTDDID) {
			                //            // we've found the color for this one, do we color the cell or not?
			                //            for (var k = 0; k < studentresult.FieldResultsByTestDueDate[i].FieldResults.length; k++) {
			                //                // find the right field, then check to see if it should be colored in
			                //                if (studentresult.FieldResultsByTestDueDate[i].FieldResults[k].FPValueId === scaleRowFPValueID) {

			                //                    // do the comment here for each tdd, only add one icon
			                //                    if (studentresult.FieldResultsByTestDueDate[i].FieldResults[k].Comment != null && studentresult.FieldResultsByTestDueDate[i].FieldResults[k].Comment != '') {
			                //                        if (currentNoteText === '') {
			                //                            //currentNoteText += '<i class="fa fa-paperclip" style="cursor:pointer" popover-template="&apos;commentCell_' + scope.studentresult.StudentId + '_' + field.Id + '&apos;" popover-title="Comments"></i>';
			                //                            noteLeftTemplate = '<i popover-trigger="mouseenter" class="fa fa-comments" style="margin-left:5px;cursor:pointer" popover="';
			                //                            noteRightTemplate = '" popover-title="Comments"></i>';
			                //                        }
			                //                        var d = new Date(Date.parse(scope.tdds[j].DueDate));
			                //                        currentNoteText += "<div><span class='orange' style='border:1px solid #666666;display:inline-block;height:10px;width:25px;Background-color:" + scope.tdds[j].Hex + ";'></span><span>&nbsp;" + (d.getMonth() + 1) + "/" + d.getFullYear() + "</span>&nbsp;&nbsp;--&nbsp;&nbsp;<span style='font-weight:bold'>" + studentresult.FieldResultsByTestDueDate[i].FieldResults[k].Comment + "</span></div>";
			                //                        //noteLeftTemplate += "busted";
			                //                    }

			                //                }
			                //            }
			                //        }
			                //    }
			                //}

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

			                            currentCellLeftHtml = "<td class='" + getCellStyle() + "' style='font-weight:bold;text-align:center;background-color:" + hexColor + "'>";
			                        }
			                    }
			                }


			                //currentCategory = scope.headerfields[p].SubcategoryId;
			                // add an empty cell if none of the test due dates are checked
			                if (currentCellLeftHtml === '') {

			                    if (noteLeftTemplate === '') {
			                        rowHtml += "<td class='" + getCellStyle() + "' ></td>";
			                    }
			                    else {
			                        rowHtml += "<td class='" + getCellStyle() + "' >" + noteLeftTemplate + currentNoteText + noteRightTemplate + "</td>";
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

			                for (var j = 0; j < scope.tdds.length; j++) {
			                    var currentTDDID = scope.tdds[j].Id;

			                    //TODO, need to do a foreach of the headerfields and output them
			                    for (var i = 0; i < studentresult.FieldResultsByTestDueDate.length; i++) {

			                        if (studentresult.FieldResultsByTestDueDate[i].TDDID === currentTDDID) {
			                            // we've found the color for this one, do we color the cell or not?
			                            for (var k = 0; k < studentresult.FieldResultsByTestDueDate[i].FieldResults.length; k++) {
			                                // find the right field, then check to see if it should be colored in
			                                if (studentresult.FieldResultsByTestDueDate[i].FieldResults[k].WordsCorrect === scaleRowScore || (studentresult.FieldResultsByTestDueDate[i].FieldResults[k].WordsCorrect >= 56 && scaleRowScore == 56)) {

			                                    // do the comment here for each tdd, only add one icon
			                                    if (studentresult.FieldResultsByTestDueDate[i].FieldResults[k].Comment != null && studentresult.FieldResultsByTestDueDate[i].FieldResults[k].Comment != '') {
			                                        if (currentNoteText === '') {
			                                            //currentNoteText += '<i class="fa fa-paperclip" style="cursor:pointer" popover-template="&apos;commentCell_' + scope.studentresult.StudentId + '_' + field.Id + '&apos;" popover-title="Comments"></i>';
			                                            noteLeftTemplate = '<i popover-trigger="mouseenter" class="fa fa-comments" style="margin-left:5px;cursor:pointer" popover="';
			                                            noteRightTemplate = '" popover-title="Comments"></i>';
			                                        }
			                                        var d = new Date(Date.parse(scope.tdds[j].DueDate));
			                                        currentNoteText += "<div><span class='orange' style='border:1px solid #666666;display:inline-block;height:10px;width:25px;Background-color:" + scope.tdds[j].Hex + ";'></span><span>&nbsp;" + (d.getMonth() + 1) + "/" + d.getFullYear() + "</span>&nbsp;&nbsp;--&nbsp;&nbsp;<span style='font-weight:bold'>" + studentresult.FieldResultsByTestDueDate[i].FieldResults[k].Comment + "</span></div>";
			                                        //noteLeftTemplate += "busted";
			                                    }

			                                }
			                            }
			                        }
			                    }
			                }


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

			            rowHtml += "<td>" + scope.studentresult.LastName + "," + scope.studentresult.FirstName + "</td>";
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

			            rowHtml += "<td>" + scope.studentresult.LastName + "," + scope.studentresult.FirstName + "</td>";
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
			                                            //currentNoteText += '<i class="fa fa-paperclip" style="cursor:pointer" popover-template="&apos;commentCell_' + scope.studentresult.StudentId + '_' + field.Id + '&apos;" popover-title="Comments"></i>';
			                                            noteLeftTemplate = '<i popover-trigger="mouseenter" class="fa fa-comments" style="margin-left:5px;cursor:pointer" popover="';
			                                            noteRightTemplate = '" popover-title="Comments"></i>';
			                                        }
			                                        var d = new Date(Date.parse(scope.tdds[j].DueDate));
			                                        currentNoteText += "<div><span class='orange' style='border:1px solid #666666;display:inline-block;height:10px;width:25px;Background-color:" + scope.tdds[j].Hex + ";'></span><span>&nbsp;" + (d.getMonth() + 1) + "/" + d.getFullYear() + "</span>&nbsp;&nbsp;--&nbsp;&nbsp;<span style='font-weight:bold'>" + scope.studentresult.FieldResultsByTestDueDate[i].FieldResults[k].Comment + "</span></div>";
			                                        //noteLeftTemplate += "busted";
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
        .controller('HRSIWSectionReportController', HRSIWSectionReportController)
        .controller('LIDSectionAlphaReportController', LIDSectionAlphaReportController)
        .controller('LIDSectionSoundReportController', LIDSectionSoundReportController)
        .controller('LIDSectionOverallReportController', LIDSectionOverallReportController)
        .controller('SpellingInventorySectionReportController', SpellingInventorySectionReportController);

    WVSectionReportController.$inject = ['$scope', '$q', '$http', 'pinesNotifications', '$location', '$filter', '$routeParams', 'nsFilterOptionsService', 'WVReportManager'];
    FPSectionReportController.$inject = ['$scope', '$q', '$http', 'pinesNotifications', '$location', '$filter', '$routeParams', 'nsFilterOptionsService', 'FPReportManager'];
    HRSIWSectionReportController.$inject = ['$scope', '$q', '$http', 'pinesNotifications', '$location', '$filter', '$routeParams', 'nsFilterOptionsService', 'HRSIWReportManager'];
    CAPSectionReportController.$inject = ['$scope', '$q', '$http', 'pinesNotifications', '$location', '$filter', '$routeParams', 'nsFilterOptionsService', 'webApiBaseUrl'];
    LIDSectionAlphaReportController.$inject = ['$scope', '$q', '$http', 'pinesNotifications', '$location', '$filter', '$routeParams', 'nsFilterOptionsService', 'LIDReportManager'];
    LIDSectionSoundReportController.$inject = ['$scope', '$q', '$http', 'pinesNotifications', '$location', '$filter', '$routeParams', 'nsFilterOptionsService', 'LIDReportManager'];
    LIDSectionOverallReportController.$inject = ['$scope', '$q', '$http', 'pinesNotifications', '$location', '$filter', '$routeParams', 'nsFilterOptionsService', 'LIDReportManager'];
    SpellingInventorySectionReportController.$inject = ['$scope', '$q', '$http', 'pinesNotifications', '$location', '$filter', '$routeParams','nsFilterOptionsService', 'webApiBaseUrl', 'SpellReportManager'];

    function WVSectionReportController($scope, $q, $http, pinesNotifications, $location, $filter, $routeParams, nsFilterOptionsService, WVReportManager) {
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


    function FPSectionReportController($scope, $q, $http, pinesNotifications, $location, $filter, $routeParams, nsFilterOptionsService, FPReportManager) {
        $scope.filterOptions = nsFilterOptionsService.options;
        $scope.dataMgr = new FPReportManager();

        $scope.$watch('filterOptions.selectedSection.id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
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
                $scope.dataMgr.LoadData($routeParams.id, $scope.filterOptions.selectedSection.id, $scope.filterOptions.selectedSchoolYear.id);
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
            if (!angular.equals(newValue, oldValue)) {
                LoadData();
            }
        });

        $scope.$watch('filterOptions.selectedHrsForm.id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                LoadData();
            }
        });

        var LoadData = function () {
            if ($scope.filterOptions.selectedSection != null && $scope.filterOptions.selectedHrsForm != null) { //Todo: CHECK FORMID(assessmentId, sectionId, reportType, schoolYear)
                $scope.dataMgr.LoadData($routeParams.id, $scope.filterOptions.selectedSection.id, $scope.reportType, $scope.filterOptions.selectedSchoolYear.id, $scope.filterOptions.selectedHrsForm.id);
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
            if (!angular.equals(newValue, oldValue)) {
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
            if (!angular.equals(newValue, oldValue)) {
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
            if (!angular.equals(newValue, oldValue)) {
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
            if (!angular.equals(newValue, oldValue)) {
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

    function SpellingInventorySectionReportController($scope, $q, $http, pinesNotifications, $location, $filter, $routeParams, nsFilterOptionsService, webApiBaseUrl, SpellReportManager) {
        $scope.filterOptions = nsFilterOptionsService.options;
        $scope.dataMgr = new SpellReportManager();

        $scope.$watch('filterOptions.selectedSection.id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                LoadData();
            }
        });

        $scope.$watch('filterOptions.selectedBenchmarkDate.id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                LoadData();
            }
        });

        $scope.navigateToTdd = function (tddid) {
            $scope.filterOptions.selectedBenchmarkDate = nsFilterOptionsService.getBenchmarkDateById(tddid);
        }

        var LoadData = function () {
            if ($scope.filterOptions.selectedSection != null && $scope.filterOptions.selectedBenchmarkDate != null) {
                $scope.dataMgr.LoadData($routeParams.id, $scope.filterOptions.selectedSection.id, $scope.filterOptions.selectedBenchmarkDate.id);
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
        // default sort
        //$scope.sortArray.push("LastName");
        //$scope.sortArray.push("FirstName");
        //$scope.hasBeenSorted = false;

        //$scope.sort = function(column) {
        //    if(!$scope.hasBeenSorted) {
        //        $scope.sortArray = [];
        //        $scope.hasBeenSorted = true;
        //    }

		//	var columnIndex = -1;
		//	// if this is not a first or lastname column
		//	if (!isNaN(parseInt(column))) {
		//		columnIndex = column;
		//		switch ($scope.HeaderFields[column].FieldType) {
        //        case 'DateCheckbox':
		//			column = 'FieldResults[' + column + '].DateValue';
		//			break;
		//		case 'Textfield':
		//			column = 'FieldResults[' + column + '].StringValue';
		//			break;
		//		case 'DecimalRange':
		//			column = 'FieldResults[' + column + '].DecimalValue';
		//			break;
		//		case 'DropdownRange':
		//			column = 'FieldResults[' + column + '].IntValue';
		//			break;
		//		case 'DropdownFromDB':
		//			column = 'FieldResults[' + column + '].IntValue';
		//			break;
		//		case 'CalculatedFieldDbOnly':
		//			column = 'FieldResults[' + column + '].StringValue';
		//			break;
		//		case 'CalculatedFieldDbBacked':
		//			column = 'FieldResults[' + column + '].IntValue';
		//			break;
		//		case 'CalculatedFieldDbBackedString':
		//			column = 'FieldResults[' + column + '].StringValue';
		//			break;
		//		case 'CalculatedFieldClientOnly':
		//			column = 'FieldResults[' + column + '].StringValue'; //shouldnt even be used in sorting
		//			break;
		//		default:
		//			column = 'FieldResults[' + column + '].IntValue';
		//			break;
		//		}
		//	}

            
		//	var bFound = false;
		//	for (var j = 0; j < $scope.sortArray.length; j++) {
		//		// if it is already on the list, reverse the sort
		//		if ($scope.sortArray[j].indexOf(column) >= 0) {
		//			bFound = true;

		//			// is it already negative? if so, remove it
		//			if ($scope.sortArray[j].indexOf("-") === 0) {
		//				if (columnIndex > -1) {
		//					$scope.headerClassArray[columnIndex] = "fa";
		//				} else if (column === 'FirstName') {
		//					$scope.firstNameHeaderClass = "fa";
		//				} else if (column === 'LastName') {
		//					$scope.lastNameHeaderClass = "fa";
		//				} else if (column === 'FPValueID') {
		//					$scope.fpValueIDHeaderClass = "fa";
		//				} else if (column === 'TotalScore') {
		//					$scope.totalScoreHeaderClass = "fa";
		//				}
		//				$scope.sortArray.splice(j, 1);

        //                if($scope.sortArray.length === 0) {
        //                     $scope.sortArray.push("LastName");
        //                     $scope.sortArray.push("FirstName");
        //                     $scope.hasBeenSorted = false;
        //                }
		//			} else {
		//				if (columnIndex > -1) {
		//					$scope.headerClassArray[columnIndex] = "fa fa-chevron-down";
		//				} else if (column === 'FirstName') {
		//					$scope.firstNameHeaderClass = "fa fa-chevron-down";
		//				} else if (column === 'LastName') {
		//					$scope.lastNameHeaderClass = "fa fa-chevron-down";
		//				} else if (column === 'FPValueID') {
		//					$scope.fpValueIDHeaderClass = "fa fa-chevron-down";
		//				} else if (column === 'TotalScore') {
		//					$scope.totalScoreHeaderClass = "fa fa-chevron-down";
		//				}
		//				$scope.sortArray[j] = "-" + $scope.sortArray[j];
		//			}
		//			break;
		//		}
		//	}
		//	if (!bFound) {
		//		$scope.sortArray.push(column);

		//		if (columnIndex > -1) {
		//			$scope.headerClassArray[columnIndex] = "fa fa-chevron-up";
		//		} else if (column === 'FirstName') {
		//			$scope.firstNameHeaderClass = "fa fa-chevron-up";
		//		} else if (column === 'LastName') {
		//			$scope.lastNameHeaderClass = "fa fa-chevron-up";
		//		} else if (column === 'FPValueID') {
		//		    $scope.fpValueIDHeaderClass = "fa fa-chevron-up";
		//	    } else if (column === 'TotalScore') {
		//		    $scope.totalScoreHeaderClass = "fa fa-chevron-up";
		//	    }
		//	}
		//};

        //for (var r = 0; r < $scope.HeaderFields.length; r++) {
        //    $scope.headerClassArray[r] = 'fa';
        //}
    }

    /* Utility Functions */



})();