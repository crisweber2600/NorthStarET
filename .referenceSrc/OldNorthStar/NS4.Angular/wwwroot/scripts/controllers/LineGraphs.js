(function () {
    'use strict';

    angular
        .module('lineGraphModule', [])
        .directive('lineGraphDetail', ['$http', '$uibModal', function ($http, $uibModal) {
            return {
                restrict: 'E',
                templateUrl: 'templates/linegraph-detail.html',
                scope: {
                    dataManager: '='
                },
                link: function (scope) {
                    scope.openCommentModal = function (text) {
                        var returnString = '';

                        var modalInstance = $uibModal.open({
                            templateUrl: 'genericModal.html',
                            scope: scope,
                            controller: function ($scope, $uibModalInstance) {
                                $scope.heading = 'Comments';
                                $scope.body = text;
                                $scope.cancel = function () {
                                    $uibModalInstance.dismiss('cancel');
                                };
                            },
                            size: 'lg',
                        });

                    }
                }
            }
        }])
                .directive('lineGraphDetailIg', ['$http', '$uibModal', function ($http, $uibModal) {
                    return {
                        restrict: 'E',
                        templateUrl: 'templates/linegraph-detail-ig.html',
                        scope: {
                            dataManager: '='
                        },
                        link: function (scope) {
                            scope.openCommentModal = function (text) {
                                var returnString = '';

                                var modalInstance = $uibModal.open({
                                    templateUrl: 'genericModal.html',
                                    scope: scope,
                                    controller: function ($scope, $uibModalInstance) {
                                        $scope.heading = 'Comments';
                                        $scope.body = text;
                                        $scope.cancel = function () {
                                            $uibModalInstance.dismiss('cancel');
                                        };
                                    },
                                    size: 'lg',
                                });

                            }
                        }
                    }
                }])
        .directive('studentInterventions', ['$http', '$location', function ($http, $location) {
            return {
                restrict: 'E',
                templateUrl: 'templates/student-interventions.html',
                scope: {
                    dataManager: '='
                },
                link: function (scope, element, attr) {
                    scope.goToDashboard = function (schoolYear, school, interventionist, interventionGroup, studentId, stint) {
                        $location.path('ig-dashboard/' + schoolYear + '/' + school + '/' + interventionist + '/' + interventionGroup + '/' + studentId + '/' + stint);
                    }
                }
            }
        }])
        .directive('lineGraphCommentsCell', [
			'$routeParams', '$compile', '$templateCache', '$http', '$uibModal','$global', function ($routeParams, $compile, $templateCache, $http, $uibModal, $global) {


			    return {
			        restrict: 'EA',
			        scope: {
			            comments: '='
			        },
			        link: function (scope, element, attr) {

			       
			            scope.settings = { comments: 'empty' };

			            scope.toolTipFunction = function () {
			                var returnString = '';


			                returnString = '<div class="commentSpacing"><span class="normalCommentText">' + scope.comments + '</span></div>';

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

		
			            var htmlLink = '';
			            if (angular.isDefined($global.get('turnOffCommentBubbles')) && $global.get('turnOffCommentBubbles')) {
			                htmlLink = scope.comments;
			            } else {
			                if (scope.comments != null && scope.comments != '' && scope.comments != 'N/A') {
			                    htmlLink = '<i class="fa fa-comments" style="margin-left:5px;cursor:pointer" ng-click="toolTipFunction();"></i>';
			                }
			            }

			            //element.html("cheese");
			            element.html(htmlLink);
			            $compile(element.contents())(scope);

			        }
			    };
			}
        ])
        .controller('LineGraphController', LineGraphController)
        .controller('LineGraphIGController', LineGraphIGController)
        .factory('NSStudentInterventionManager', [
        '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
            var NSStudentInterventionManager = function () {
                var self = this;

                self.formatDate2 = function (inDate) {
                    if (inDate != null) {
                        return moment(inDate).format('DD-MMM-YYYY');
                    }
                    else {
                        return 'N/A';
                    }
                }

                self.LoadData = function (studentId) {
                    var returnObject = { id: studentId };

                    var url = webApiBaseUrl + '/api/student/GetStudentInterventions';
                    var promise = $http.post(url, returnObject);

                    return promise.then(function (response) {
                        angular.extend(self, response.data);
                        if (self.Interventions === null) self.Interventions = [];
                    });
                }
            };

            return NSStudentInterventionManager;
        }])
        .factory('NSStudentAssessmentFieldManager', [
        '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
            var NSStudentAssessmentFieldManager = function () {
                var self = this;

                self.Fields = [];

                self.LoadData = function (assessmentTypeId, studentId, interventionGroupId) {
                    var returnObject = { StudentId: studentId, AssessmentTypeId: assessmentTypeId, InterventionGroupId: interventionGroupId };

                    var url = webApiBaseUrl + '/api/linegraph/GetStudentLineGraphFields';
                    var promise = $http.post(url, returnObject);

                    return promise.then(function (response) {
                        angular.extend(self, response.data);
                        if (self.Fields === null) self.Fields = [];
                    });
                }
            };

            return NSStudentAssessmentFieldManager;
        }])
        .factory('NSAssesssmentLineGraphManager', [
        '$http', 'webApiBaseUrl', '$location', function ($http, webApiBaseUrl, $location) {
            var NSAssesssmentLineGraphManager = function () {
                var self = this;

                self.LoadData = function (assessmentId, fieldToRetrieve, lookupFieldName, fieldType, studentId, fieldDisplayName, sectionId, assessmentName, interventions) {
                    self.bUseExceeds = false;
                    self.bUseMeets = false;
                    self.bUseApproaches = false;
                    self.bUseDoesNotMeet = false;
                    self.assessmentName = assessmentName;
                    self.fieldToRetrieve = fieldDisplayName;
                    self.status = {};
                    self.status.isLookup = (lookupFieldName != null && lookupFieldName != '');
                    self.status.isDecimalField = false; // TODO: check field type
                    var returnObject = { AssessmentId: assessmentId, FieldToRetrieve: fieldToRetrieve, LookupFieldName: lookupFieldName, IsLookupColumn: self.status.isLookup, StudentId: studentId };

                    var url = webApiBaseUrl + '/api/lineGraph/GetStudentLineGraph';
                    var returnData = $http.post(url, returnObject);

                    self.Results = [];
                    self.Fields = [];
                    self.BenchmarkDates = [];
                    self.Interventions = (interventions == null) ? [] : interventions;
                    self.VScale = [];
                   // self.Fields = {};
                    self.chartConfig = {};

                    return returnData.then(function (response) {
                        angular.extend(self, response.data);
                        if (self.Results === null) self.Results = [];
                        if (self.Fields === null) self.Fields = [];
                        if (self.BenchmarkDates === null) self.BenchmarkDates = [];
                        //if (self.Interventions === null) self.Interventions = [];
                        if (self.VScale === null) self.VScale = [];
                        //if (self.Fields === null) {
                        //    self.Fields = {};
                        //}
                        //else {
                        //    self.Fields = self.Fields.Assessments[0].Fields;
                        //}

                        self.postDataLoadSetup();
                    });
                }

                self.LoadAssessmentFields = function () {

                    var url = webApiBaseUrl + '/api/benchmark/GetAssessmentsAndFields';
                    var promise = $http.get(url);

                    return promise.then(function (response) {
                        self.assessments = self.flatten(response.data.Assessments);
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
                                DisplayInLineGraphs: v.DisplayInLineGraphs
                            })
                        })
                    })
                    return out;
                }

                var linesPrinted = 1;
                function dynamicY() {
                    var yValue = 0;
                    yValue = (linesPrinted % 10) * 20;
                    linesPrinted++;
                    return yValue;
                }

                self.postDataLoadSetup = function () {
                    var self = this;
                    var currentMax = 1;
                    var data = [];
                    var doesnotmeetData = [];
                    var approachesData = [];
                    var meetsData = [];
                    var exceedsData = [];
                    var plotlines = [];
                    var loopCounter = 0;
                    var plotLinesLoopCounter = 0;
                    var highchartsNgConfig = {};

                    // determine which benchmarks we want to use
                    
                    for (var d = 0; d < self.BenchmarkDates.length; d++) {
                        if (self.BenchmarkDates[d].Exceeds !== null || self.bUseExceeds == true) {
                            self.bUseExceeds = true;
                        }
                        if (self.BenchmarkDates[d].Meets !== null || self.bUseMeets == true) {
                            self.bUseMeets = true;
                        }
                        if (self.BenchmarkDates[d].Approaches !== null || self.bUseApproaches == true) {
                            self.bUseApproaches = true;
                        }
                        if (self.BenchmarkDates[d].DoesNotMeet !== null || self.bUseDoesNotMeet == true) {
                            self.bUseDoesNotMeet = true;
                        }
                    }

                    // set up fields
                    for (var r = 0; r < self.Fields.length; r++) {
                        self.Fields[r].BenchmarkDates = angular.copy(self.BenchmarkDates);

                        for (var i = 0; i < self.Fields[r].BenchmarkDates.length; i++) {
                            self.Fields[r].BenchmarkDates[i].Result = {};
                            self.Fields[r].BenchmarkDates[i].Result.StringValue = "N/A"; // magic number

                            for (var j = 0; j < self.Results.length; j++) {
                                if (self.Fields[r].BenchmarkDates[i].TestDueDateID == self.Results[j].TestDueDateID) {

                                    // attach result to the proper benchmark date
                                    //self.Fields[r].BenchmarkDates[i].Result = self.Results[j];
                                    // now get the proper result for this field

                                    for (var p = 0; p < self.Results[j].FieldResults.length; p++) {
                                        if (self.Results[j].FieldResults[p].DbColumn === self.Fields[r].DatabaseColumn) {
                                            self.Fields[r].BenchmarkDates[i].Result.StringValue = self.Results[j].FieldResults[p].StringValue;
                                            break;
                                        }
                                    }

                                }
                            }
                        }
                    }

                    // set up series
                    for (var i = 0; i < self.BenchmarkDates.length; i++) {
                        self.BenchmarkDates[i].Result = {};
                        self.BenchmarkDates[i].Result.FieldValueID = null; // magic number
                        self.BenchmarkDates[i].Result.FieldDisplayValue = "N/A"; // magic number
                        currentMax = self.BenchmarkDates[i].TestNumber;

                        // set benchmark data series
                        doesnotmeetData[i] = [i + 1, self.BenchmarkDates[i].DoesNotMeet];
                        approachesData[i] = [i + 1, self.BenchmarkDates[i].Approaches];
                        meetsData[i] = [i + 1, self.BenchmarkDates[i].Meets];
                        exceedsData[i] = [i + 1, self.BenchmarkDates[i].Exceeds];

                        for (var j = 0; j < self.Results.length; j++) {
                            if (self.BenchmarkDates[i].TestDueDateID == self.Results[j].TestDueDateID) {

                                // attach result to the proper benchmark date
                                self.BenchmarkDates[i].Result = self.Results[j];

                                // process the result, set colors, etc

                                if (self.Results[j].FieldDisplayValue != null && self.Results[j].FieldDisplayValue != '') {
                                    self.Results[j].x = i + 1;
                                    var fillColor = self.Results[j].IsCopied ? '#FFFFFF' : '#2244BB';

                                    data[loopCounter] = {
                                        marker: { fillColor: fillColor},
                                        x: i + 1,
                                        y: self.Results[j].FieldValueID, name: self.Results[j].FieldDisplayValue, dataLabels: {
                                            enabled: true,                                            crop: false,                                            style: {
                                                fontWeight: 'bold'
                                            },                                            formatter: function () {
                                                return this.point.name;
                                            }
                                        }
                                    };
                                }
                                else {
                                    self.Results[j].x = i + 1;
                                    data[loopCounter] = [i + 1, self.Results[j].FieldValueID];
                                }
                                loopCounter++;
                            }
                        }

                        // set up interventions
                        // TODO: create a formula for the Y value.  something like +30 for every time through                        
                        for (var j = 0; j < self.Interventions.length; j++) {
                            if (self.BenchmarkDates[i].TestDueDateID == self.Interventions[j].StartTDDID) {
                                plotlines[plotLinesLoopCounter] = {
                                    color: '#2244BB', dashStyle: 'solid', value: currentMax, width: '2',
                                    label: { text: 'Start ' + self.Interventions[j].InterventionType, align: 'left', y: dynamicY(), x: 3, rotation: 0, style: { fontFamily: 'Arial' } },
                                   // zIndex: 0
                                }
                                plotLinesLoopCounter++;
                            }
                            if (self.BenchmarkDates[i].TestDueDateID == self.Interventions[j].EndTDDID) {
                                plotlines[plotLinesLoopCounter] = {
                                    color: '#2244BB', dashStyle: 'solid', value: currentMax, width: '2',
                                    label: { text: 'End ' + self.Interventions[j].InterventionType, align: 'left', y: dynamicY(), x: 3, rotation: 0, style: { fontFamily: 'Arial' } },
                                    zIndex: 0
                                }
                                plotLinesLoopCounter++;
                            }
                        }
                    }


                    var yAxis = null;
                    if (self.status.isLookup) {
                        yAxis = {
                            allowDecimals: false,
                            min: 0,
                            title: { text: self.fieldToRetrieve },
                            labels: {
                                formatter: function () {
                                    var value = null;

                                    for (var i = 0; i < self.VScale.length; i++) {
                                        if (self.VScale[i].FieldSpecificId == this.value) {
                                            return self.VScale[i].FieldValue;
                                        }
                                    }
                                    return null;
                                }
                            }
                        }
                    }
                    else {
                        yAxis = {
                            allowDecimals: false,
                            min: 0,
                            title: { text: self.fieldToRetrieve }
                        }
                    }

                    var safePrint =  $location.absUrl().indexOf('printmode=') >= 0 ? false : true;

                    // TODO:  dynamically push series depending on if they have values or not
                    var series = [];
                    series.push({
                        enableMouseTracking: safePrint,
                        animation: safePrint,
                        name: self.fieldToRetrieve,
                        color: "#2244BB",
                        data: data,
                        lineWidth: 3,
                        shadow: { offsetX: 3, offsetY: 2, opacity: 0.12, width: 6 },
                        zIndex: 100,
                        marker: {
                            //fillColor: this.x > 1 ? 'red': '#FFFFFF',
                            lineWidth: 2,
                            lineColor: null // inherit from series
                        },
                        tooltip: {
                            headerFormat: '',
                            pointFormatter: function (point) {

                                //getDisplayValue(date.Result.FieldValueID)
                                var responseString = '';
                                // header score ROW
                                for (var j = 0; j < self.BenchmarkDates.length; j++) {
                                    var currentBenchmarkDate = self.BenchmarkDates[j];

                                    // if we are on the right benchmark date
                                    if (j + 1 == this.x) {
                                        responseString = '<span style="font-weight:bold;color:' + this.color + '">' + self.formatDate(currentBenchmarkDate.DueDate) + '</span><br/>';
                                        responseString += '<span style="color:' + this.color + '">\u25CF</span> ' + 'Score: ';
                                        responseString += '<b>' + self.getDisplayValue(currentBenchmarkDate.Result.FieldValueID) + '</b><br/>';
                                        break;
                                    }
                                }

                                for (var i = 0; i < self.Fields.length; i++) {
                                    var field = self.Fields[i];

                                    // don't display text fields
                                    if (field.FieldType == 'Textarea') {
                                        continue;
                                    }

                                    responseString += field.DisplayLabel + ' : ';
                                    for (var j = 0; j < field.BenchmarkDates.length; j++) {
                                        var currentBenchmarkDate = field.BenchmarkDates[j];

                                        // if we are on the right benchmark date
                                        if (j + 1 == this.x) {
                                            if (currentBenchmarkDate.Result.StringValue == null) {
                                                responseString += '<br/>';
                                            } else {
                                                responseString += '<b>' + (field.AltDisplayLabel ? field.AltDisplayLabel : '') + currentBenchmarkDate.Result.StringValue + '</b><br/>';

                                            }
                                            break;
                                        }
                                    }
                                }

                                return responseString;
                            }
                        }
                    });

                    if (self.bUseExceeds) {
                        series.push({
                            enableMouseTracking: safePrint,
                            animation: safePrint,
                            name: "Exceeds",
                            color: "#4697ce",
                            data: exceedsData,
                            lineWidth: 3,
                            shadow: false,
                            zIndex: 2,
                            dashStyle: 'ShortDash',
                            marker: { enabled: false },
                            tooltip: {
                            headerFormat: '<span style="font-size: 10px">Test {point.key}</span><br/>',
                                pointFormatter: function () {
                                    for (var i = 0; i < self.VScale.length; i++) {
                                        if (self.VScale[i].FieldSpecificId == this.y) {
                                            return '<span style="color:' + this.color + '">\u25CF</span> ' + this.series.name + ': <b>' + self.VScale[i].FieldValue + '</b><br/>';
                                        }
                                    }
                                    return '<span style="color:' + this.color + '">\u25CF</span> ' + this.series.name + ': <b>' + this.y + '</b><br/>';
                                }
                            }
                        });
                    }
                    if (self.bUseMeets) {
                        series.push({
                            enableMouseTracking: safePrint,
                            animation: safePrint,
                            name: "Meets",
                            color: "#ABF09C",
                            data: meetsData,
                            lineWidth: 3,
                            shadow: false,
                            zIndex: 2,
                            dashStyle: 'ShortDash',
                            marker: { enabled: false },
                            tooltip: {
                                headerFormat: '<span style="font-size: 10px">Test {point.key}</span><br/>',
                                pointFormatter: function () {
                                    for (var i = 0; i < self.VScale.length; i++) {
                                        if (self.VScale[i].FieldSpecificId == this.y) {
                                            return '<span style="color:' + this.color + '">\u25CF</span> ' + this.series.name + ': <b>' + self.VScale[i].FieldValue + '</b><br/>';
                                        }
                                    }
                                    return '<span style="color:' + this.color + '">\u25CF</span> ' + this.series.name + ': <b>' + this.y + '</b><br/>';
                                }
                            }
                        });
                    }
                    if (self.bUseApproaches) {
                        series.push({
                            enableMouseTracking: safePrint,
                            animation: safePrint,
                            name: "Approaches",
                            color: "#E4D354",
                            data: approachesData,
                            lineWidth: 3,
                            shadow: false,
                            zIndex: 3,
                            dashStyle: 'ShortDash',
                            marker: { enabled: false },
                            tooltip: {
                                headerFormat: '<span style="font-size: 10px">Test {point.key}</span><br/>',
                                pointFormatter: function () {
                                    for (var i = 0; i < self.VScale.length; i++) {
                                        if (self.VScale[i].FieldSpecificId == this.y) {
                                            return '<span style="color:' + this.color + '">\u25CF</span> ' + this.series.name + ': <b>' + self.VScale[i].FieldValue + '</b><br/>';
                                        }
                                    }
                                    return '<span style="color:' + this.color + '">\u25CF</span> ' + this.series.name + ': <b>' + this.y + '</b><br/>';
                                }
                            }
                        });
                    }
                    if (self.bUseDoesNotMeet) {
                        series.push({
                            enableMouseTracking: safePrint,
                            animation: safePrint,
                            name: "Does Not Meet",
                            color: "#BF453D",
                            data: doesnotmeetData,
                            lineWidth: 3,
                            shadow: false,
                            zIndex: 1,
                            dashStyle: 'ShortDash',
                            marker: { enabled: false },
                            tooltip: {
                                headerFormat: '<span style="font-size: 10px">Test {point.key}</span><br/>',
                                pointFormatter: function () {
                                    for (var i = 0; i < self.VScale.length; i++) {
                                        if (self.VScale[i].FieldSpecificId == this.y) {
                                            return '<span style="color:' + this.color + '">\u25CF</span> ' + this.series.name + ': <b>' + self.VScale[i].FieldValue + '</b><br/>';
                                        }
                                    }
                                    return '<span style="color:' + this.color + '">\u25CF</span> ' + this.series.name + ': <b>' + this.y + '</b><br/>';
                                }
                            }
                        });
                    }
                    highchartsNgConfig = {
                        //This is not a highcharts object. It just looks a little like one!
                        options: {
                            credits: { enabled: false },
                            //This is the Main Highcharts chart config. Any Highchart options are valid here.
                            //will be ovverriden by values specified below.
                            chart: {
                                type: 'line'
                            },

                            //,
                            //tooltip: {
                            //    formatter: function () {
                            //        var value = null;
                            //        var comments = '';

                            //        for (var j = 0; j < self.Results.length; j++) {
                            //            if (self.Results[j].x == this.x) {
                            //                comments = self.Results[j].Comments;
                            //            }
                            //        }

                            //        for (var i = 0; i < self.VScale.length; i++) {
                            //            if (self.VScale[i].FieldSpecificId == this.y) {
                            //                return '<span style="color:' + this.point.color + '">\u25CF</span> ' + this.series.name + ': <b>' + self.VScale[i].FieldValue + '</b><br/>';
                            //                //if (comments != '' && comments != null && angular.isDefined(comments)) {
                            //                //    return self.VScale[i].FieldValue + "<br> <b>Comments:</b> " + comments;
                            //                //} else return self.VScale[i].FieldValue;
                            //            }
                            //        }
                            //        return this.y;
                            //    }
                            //}
                        },

                        //The below properties are watched separately for changes.

                        //Series object (optional) - a list of series using normal highcharts series options.
                        series: series,
                        //Title configuration (optional)
                        title: {
                            text: self.assessmentName + ' ' + self.fieldToRetrieve
                        },
                        //Boolean to control showng loading status on chart (optional)
                        //Could be a string if you want to show specific loading text.
                        loading: false,
                        //Configuration for the xAxis (optional). Currently only one x axis can be dynamically controlled.
                        //properties currentMin and currentMax provied 2-way binding to the chart's maximimum and minimum
                        xAxis: {
                            currentMin: 1,
                            currentMax: currentMax,
                            title: { text: 'Test Number' },
                            plotLines: plotlines,
                            allowDecimals: false
                        },
                        yAxis: yAxis,
                        //Whether to use HighStocks instead of HighCharts (optional). Defaults to false.
                        useHighStocks: false,
                        //size (optional) if left out the chart will default to size of the div or something sensible.
                        size: {
                            height: 600
                        },
                        //function (optional)
                        func: function (chart) {
                            //setup some logic for the chart
                        }

                    };

                    self.chartConfig = highchartsNgConfig;

                    Highcharts.wrap(Highcharts.Axis.prototype, 'getPlotLinePath', function (proceed) {
                        var path = proceed.apply(this, Array.prototype.slice.call(arguments, 1));
                        if (path) {
                            path.flat = false;
                        }
                        return path;
                    });
                }

                self.getBackgroundClass = function (studentFieldScore, tddId) {
                    var self = this;
                    var bgClass = '';


                    bgClass = self.getIntColor(studentFieldScore, tddId);

                    // see if this is an intervention date
                    bgClass += ' ' + self.getTddClass(tddId);

                    return bgClass;
                };

                self.formatDate = function (inDate) {
                    if (inDate != null) {
                        return moment(inDate).format('MMM YYYY');
                    }
                    else {
                        return 'N/A';
                    }
                }
                self.formatDate2 = function (inDate) {
                    if (inDate != null) {
                        return moment(inDate).format('DD-MMM-YYYY');
                    }
                    else {
                        return 'N/A';
                    }
                }

                self.getDisplayValue = function (id) {
                    var self = this;
                    var lookupList = self.VScale;

                    if (!self.status.isLookup) {
                        return id;
                    }

                    for (var i = 0; i < lookupList.length; i++) {
                        if (lookupList[i].FieldSpecificId == id) {
                            return lookupList[i].FieldValue;
                        }
                    }

                    return 'N/A';
                };

                /* Private Functions */
                self.getTddClass = function (tddId) {
                    var self = this;
                    for (var i = 0; i < self.Interventions.length; i++) {
                        if (tddId === self.Interventions[i].StartTDDID ||
                            tddId === self.Interventions[i].EndTDDID) {
                            return 'interventionCell';
                        }
                    }
                    return '';
                }

                self.getIntColor = function (studentFieldScore, tddId) {
                    var self = this;
                    // TODO: dont forget to add a check for NULL Benchmarks, what happens?
                    for (var i = 0; i < self.BenchmarkDates.length; i++) {
                        if (self.BenchmarkDates[i].TestDueDateID === tddId) {
                            if (studentFieldScore.FieldValueID != null) {
                                // not defined yet
                                //if (studentFieldScore.IntValue === $scope.Benchmarks[i].MaxScore) {
                                //	return 'obsGreen';
                                //}
                                if (studentFieldScore.FieldValueID >= self.BenchmarkDates[i].Exceeds && self.BenchmarkDates[i].Exceeds != null) {
                                    return 'obsBlue';
                                }
                                else if (studentFieldScore.FieldValueID >= self.BenchmarkDates[i].Meets && self.BenchmarkDates[i].Meets != null) {
                                    return '';
                                }
                                else if (studentFieldScore.FieldValueID >= self.BenchmarkDates[i].Approaches && self.BenchmarkDates[i].Approaches != null) {
                                    return 'obsYellow';
                                }
                                else if (studentFieldScore.FieldValueID < self.BenchmarkDates[i].Approaches && self.BenchmarkDates[i].Approaches != null) {
                                    return 'obsRed';
                                }
                            }
                        }
                    }
                    return '';
                }
                /* End Private Functions */

            };

            return (NSAssesssmentLineGraphManager);
        }
        ])
    .factory('NSAssesssmentIGLineGraphManager', [
        '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
            var NSAssesssmentIGLineGraphManager = function () {

                var self = this;

                self.LoadData = function (assessmentId, fieldToRetrieve, lookupFieldName, fieldType, studentId, fieldDisplayName, interventionGroupId, assessmentName, interventions, schoolStartYear) {
                    self.bUseExceeds = false;
                    self.bUseMeets = false;
                    self.bUseApproaches = false;
                    self.bUseDoesNotMeet = false;
                    self.assessmentName = assessmentName;
                    self.fieldToRetrieve = fieldDisplayName;
                    self.status = {};
                    self.status.isLookup = (lookupFieldName != null && lookupFieldName != '');
                    self.status.isDecimalField = false; // TODO: check field type
                    var returnObject = { AssessmentId: assessmentId, FieldToRetrieve: fieldToRetrieve, LookupFieldName: lookupFieldName, IsLookupColumn: self.status.isLookup, StudentId: studentId, InterventionGroupId: interventionGroupId, SchoolStartYear: schoolStartYear };

                    var url = webApiBaseUrl + '/api/lineGraph/GetStudentIGLineGraph';
                    var returnData = $http.post(url, returnObject);

                    self.Results = [];
                    self.BenchmarkDates = [];
                    self.Interventions = (interventions == null) ? [] : interventions;
                    self.VScale = [];
                    //self.Fields = {};
                    self.chartConfig = {};

                    return returnData.then(function (response) {
                        angular.extend(self, response.data);
                        if (self.Results === null) self.Results = [];
                        if (self.Fields === null) self.Fields = [];
                        if (self.BenchmarkDates === null) self.BenchmarkDates = [];
                        //if (self.Interventions === null) self.Interventions = [];
                        if (self.VScale === null) self.VScale = [];
                        //if (self.Fields === null) {
                        //    self.Fields = {};
                        //}
                        //else {
                        //    self.Fields = self.Fields.Assessments[0].Fields;
                        //}

                        self.postDataLoadSetup();
                    });
                }

                self.LoadAssessmentFields = function () {

                    var url = webApiBaseUrl + '/api/benchmark/GetInterventionAssessmentsAndFields';
                    var promise = $http.get(url);

                    return promise.then(function (response) {
                        self.assessments = self.flatten(response.data.Assessments);
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
                                DisplayInLineGraphs: v.DisplayInLineGraphs
                            })
                        })
                    })
                    return out;
                }

                self.postDataLoadSetup = function () {
                    var self = this;
                    var currentMax = 1;
                    var data = [];
                    var doesnotmeetData = [];
                    var approachesData = [];
                    var meetsData = [];
                    var exceedsData = [];
                    var plotlines = [];
                    var loopCounter = 0;
                    var plotLinesLoopCounter = 0;
                    var highchartsNgConfig = {};

                    // determine which benchmarks we want to use

                    for (var d = 0; d < self.BenchmarkDates.length; d++) {
                        if (self.BenchmarkDates[d].Exceeds !== null || self.bUseExceeds == true) {
                            self.bUseExceeds = true;
                        }
                        if (self.BenchmarkDates[d].Meets !== null || self.bUseMeets == true) {
                            self.bUseMeets = true;
                        }
                        if (self.BenchmarkDates[d].Approaches !== null || self.bUseApproaches == true) {
                            self.bUseApproaches = true;
                        }
                        if (self.BenchmarkDates[d].DoesNotMeet !== null || self.bUseDoesNotMeet == true) {
                            self.bUseDoesNotMeet = true;
                        }
                    }


                    // set up fields
                    for (var r = 0; r < self.Fields.length; r++) {
                        self.Fields[r].BenchmarkDates = angular.copy(self.BenchmarkDates);

                        for (var i = 0; i < self.Fields[r].BenchmarkDates.length; i++) {
                            self.Fields[r].BenchmarkDates[i].Result = {};
                            self.Fields[r].BenchmarkDates[i].Result.StringValue = "N/A"; // magic number

                            for (var j = 0; j < self.Results.length; j++) {
                                if (self.Fields[r].BenchmarkDates[i].TestNumber == self.Results[j].TestNumber) {

                                    // attach result to the proper benchmark date
                                    //self.Fields[r].BenchmarkDates[i].Result = self.Results[j];
                                    // now get the proper result for this field

                                    for (var p = 0; p < self.Results[j].FieldResults.length; p++) {
                                        if (self.Results[j].FieldResults[p].DbColumn === self.Fields[r].DatabaseColumn) {
                                            self.Fields[r].BenchmarkDates[i].Result.StringValue = self.Results[j].FieldResults[p].StringValue;
                                            break;
                                        }
                                    }

                                }
                            }
                        }
                    }

                    // set up series
                    for (var i = 0; i < self.BenchmarkDates.length; i++) {
                        self.BenchmarkDates[i].Result = {};
                        self.BenchmarkDates[i].Result.FieldValueID = null; // magic number
                        self.BenchmarkDates[i].Result.FieldDisplayValue = "N/A"; // magic number
                        currentMax = self.BenchmarkDates[i].TestNumber;

                        // set benchmark data series
                        doesnotmeetData[i] = [i + 1, self.BenchmarkDates[i].DoesNotMeet];
                        approachesData[i] = [i + 1, self.BenchmarkDates[i].Approaches];
                        meetsData[i] = [i + 1, self.BenchmarkDates[i].Meets];
                        exceedsData[i] = [i + 1, self.BenchmarkDates[i].Exceeds];

                        for (var j = 0; j < self.Results.length; j++) {
                            if (self.BenchmarkDates[i].TestNumber == self.Results[j].TestNumber) {

                                // attach result to the proper benchmark date
                                self.BenchmarkDates[i].Result = self.Results[j];

                                // process the result, set colors, etc

                                if (self.Results[j].FieldDisplayValue != null && self.Results[j].FieldDisplayValue != '') {
                                    self.Results[j].x = i + 1;
                                    data[loopCounter] = {
                                        x: i + 1, y: self.Results[j].FieldValueID, name: self.Results[j].FieldDisplayValue, dataLabels: {
                                            enabled: true,                                            crop: false,                                            style: {
                                                fontWeight: 'bold'
                                            },                                            formatter: function () {
                                                return this.point.name;
                                            }
                                        }
                                    };
                                }
                                else {
                                    self.Results[j].x = i + 1;
                                    data[loopCounter] = [i + 1, self.Results[j].FieldValueID];
                                }
                                loopCounter++;
                            }
                        }
                    }


                    var yAxis = null;
                    if (self.status.isLookup) {
                        yAxis = {
                            allowDecimals: false,
                            min: 0,
                            title: { text: self.fieldToRetrieve },
                            labels: {
                                formatter: function () {
                                    var value = null;

                                    for (var i = 0; i < self.VScale.length; i++) {
                                        if (self.VScale[i].FieldSpecificId == this.value) {
                                            return self.VScale[i].FieldValue;
                                        }
                                    }
                                    return null;
                                }
                            }
                        }
                    }
                    else {
                        yAxis = {
                            allowDecimals: false,
                            min: 0,
                            title: { text: self.fieldToRetrieve }
                        }
                    }

                    // TODO:  dynamically push series depending on if they have values or not
                    var series = [];
                    series.push({
                        name: self.fieldToRetrieve,
                        color: "#2244BB",
                        data: data,
                        lineWidth: 3,
                        shadow: { offsetX: 3, offsetY: 2, opacity: 0.12, width: 6 },
                        zIndex: 100,
                        marker: {
                            //fillColor: '#FFFFFF',
                            lineWidth: 2,
                            lineColor: null // inherit from series
                        },
                        tooltip: {
                            headerFormat: '',
                            pointFormatter: function (point) {

                                //getDisplayValue(date.Result.FieldValueID)
                                var responseString = '';
                                // header score ROW
                                for (var j = 0; j < self.BenchmarkDates.length; j++) {
                                    var currentBenchmarkDate = self.BenchmarkDates[j];

                                    // if we are on the right benchmark date
                                    if (j + 1 == this.x) {
                                        responseString = '<span style="font-weight:bold;color:' + this.color + '">' + self.formatDate(currentBenchmarkDate.Result.TestDueDate) + '</span><br/>';
;                                       responseString += '<span style="color:' + this.color + '">\u25CF</span> ' + 'Score: ';
                                        responseString += '<b>' + self.getDisplayValue(currentBenchmarkDate.Result.FieldValueID) + '</b><br/>';
                                        break;
                                    }
                                }

                                for (var i = 0; i < self.Fields.length; i++) {
                                    var field = self.Fields[i];

                                    // don't display text fields
                                    if (field.FieldType == 'Textarea') {
                                        continue;
                                    }

                                    responseString += field.DisplayLabel + ' : ';
                                    for (var j = 0; j < field.BenchmarkDates.length; j++) {
                                        var currentBenchmarkDate = field.BenchmarkDates[j];

                                        // if we are on the right benchmark date
                                        if (j + 1 == this.x) {
                                            if (currentBenchmarkDate.Result.StringValue == null) {
                                                responseString += '<br/>';
                                            } else {
                                                responseString += '<b>' + (field.AltDisplayLabel ? field.AltDisplayLabel : '') +  currentBenchmarkDate.Result.StringValue + '</b><br/>';

                                            }
                                            break;
                                        }
                                    }
                                }

                                return responseString;
                            }
                        }
                    });

                    if (self.bUseExceeds) {
                        series.push({
                            name: "Exceeds",
                            color: "#4697ce",
                            data: exceedsData,
                            lineWidth: 3,
                            shadow: false,
                            zIndex: 2,
                            dashStyle: 'ShortDash',
                            marker: { enabled: false },
                            tooltip: {
                                headerFormat: '<span style="font-size: 10px">Test {point.key}</span><br/>',
                                pointFormatter: function () {
                                    for (var i = 0; i < self.VScale.length; i++) {
                                        if (self.VScale[i].FieldSpecificId == this.y) {
                                            return '<span style="color:' + this.color + '">\u25CF</span> ' + this.series.name + ': <b>' + self.VScale[i].FieldValue + '</b><br/>';
                                        }
                                    }
                                    return '<span style="color:' + this.color + '">\u25CF</span> ' + this.series.name + ': <b>' + this.y + '</b><br/>';
                                }
                            }
                        });
                    }
                    if (self.bUseMeets) {
                        series.push({
                            name: "Meets",
                            color: "#ABF09C",
                            data: meetsData,
                            lineWidth: 3,
                            shadow: false,
                            zIndex: 2,
                            dashStyle: 'ShortDash',
                            marker: { enabled: false },
                            tooltip: {
                                headerFormat: '<span style="font-size: 10px">Test {point.key}</span><br/>',
                                pointFormatter: function () {
                                    for (var i = 0; i < self.VScale.length; i++) {
                                        if (self.VScale[i].FieldSpecificId == this.y) {
                                            return '<span style="color:' + this.color + '">\u25CF</span> ' + this.series.name + ': <b>' + self.VScale[i].FieldValue + '</b><br/>';
                                        }
                                    }
                                    return '<span style="color:' + this.color + '">\u25CF</span> ' + this.series.name + ': <b>' + this.y + '</b><br/>';
                                }
                            }
                        });
                    }
                    if (self.bUseApproaches) {
                        series.push({
                            name: "Approaches",
                            color: "#E4D354",
                            data: approachesData,
                            lineWidth: 3,
                            shadow: false,
                            zIndex: 3,
                            dashStyle: 'ShortDash',
                            marker: { enabled: false },
                            tooltip: {
                                headerFormat: '<span style="font-size: 10px">Test {point.key}</span><br/>',
                                pointFormatter: function () {
                                    for (var i = 0; i < self.VScale.length; i++) {
                                        if (self.VScale[i].FieldSpecificId == this.y) {
                                            return '<span style="color:' + this.color + '">\u25CF</span> ' + this.series.name + ': <b>' + self.VScale[i].FieldValue + '</b><br/>';
                                        }
                                    }
                                    return '<span style="color:' + this.color + '">\u25CF</span> ' + this.series.name + ': <b>' + this.y + '</b><br/>';
                                }
                            }
                        });
                    }
                    if (self.bUseDoesNotMeet) {
                        series.push({
                            name: "Does Not Meet",
                            color: "#BF453D",
                            data: doesnotmeetData,
                            lineWidth: 3,
                            shadow: false,
                            zIndex: 1,
                            dashStyle: 'ShortDash',
                            marker: { enabled: false },
                            tooltip: {
                                headerFormat: '<span style="font-size: 10px">Test {point.key}</span><br/>',
                                pointFormatter: function () {
                                    for (var i = 0; i < self.VScale.length; i++) {
                                        if (self.VScale[i].FieldSpecificId == this.y) {
                                            return '<span style="color:' + this.color + '">\u25CF</span> ' + this.series.name + ': <b>' + self.VScale[i].FieldValue + '</b><br/>';
                                        }
                                    }
                                    return '<span style="color:' + this.color + '">\u25CF</span> ' + this.series.name + ': <b>' + this.y + '</b><br/>';
                                }
                            }
                        });
                    }

                    highchartsNgConfig = {
                        //This is not a highcharts object. It just looks a little like one!
                        options: {
                            credits: { enabled: false },
                            //This is the Main Highcharts chart config. Any Highchart options are valid here.
                            //will be ovverriden by values specified below.
                            chart: {
                                type: 'line'
                            },
                            //tooltip: {
                            //    formatter: function () {
                            //        var value = null;
                            //        var comments = '';

                            //        for (var j = 0; j < self.Results.length; j++) {
                            //            if (self.Results[j].x == this.x) {
                            //                comments = self.Results[j].Comments;
                            //            }
                            //        }

                            //        for (var i = 0; i < self.VScale.length; i++) {
                            //            if (self.VScale[i].FieldSpecificId == this.y) {
                            //                return self.VScale[i].FieldValue + " comments: " + comments;
                            //            }
                            //        }
                            //        return this.y;
                            //    }
                            //}
                        },

                        //The below properties are watched separately for changes.

                        //Series object (optional) - a list of series using normal highcharts series options.
                        series: series,
                    //    [
                    //        {
                    //            name: self.fieldToRetrieve,
                    //            color: "#2244BB",
                    //            data: data,
                    //            lineWidth: 3,
                    //            shadow: { offsetX: 3, offsetY: 2, opacity: 0.12, width: 6},
                    //            zIndex: 100,
                    //            marker: {
                    //                fillColor: '#FFFFFF',
                    //                lineWidth: 2,
                    //                lineColor: null // inherit from series
                    //                }
                    //        },
                    //        {
                    //            name: "Does Not Meet",
                    //            color: "#BF453D",
                    //            data: doesnotmeetData,
                    //            lineWidth: 3,
                    //            shadow: false,
                    //            zIndex: 1,
                    //            dashStyle: 'ShortDash',
                    //            marker: {
                    //                enabled: false
                    //            }
                    //        },
                    //            {
                    //                name: "Approaches",
                    //                color: "#E4D354",
                    //                data: approachesData,
                    //                lineWidth: 3,
                    //                shadow: false,
                    //                zIndex: 3,
                    //                dashStyle: 'ShortDash',
                    //                marker: {
                    //                enabled: false
                    //            }
                    //        },
                    //            {
                    //                name: "Meets",
                    //                color: "#ABF09C",
                    //                data: meetsData,
                    //                lineWidth: 3,
                    //                shadow: false,
                    //                zIndex: 2,
                    //                dashStyle: 'ShortDash',
                    //                    marker: {
                    //                enabled: false
                    //            }
                    //        },
                    //    {
                    //        name: "Exceeds",
                    //                    color: "#4697ce",
                    //                    data: exceedsData,
                    //        lineWidth: 3,
                    //        shadow: false,
                    //        zIndex: 2,
                    //        dashStyle: 'ShortDash',
                    //        marker: {
                    //        enabled: false
                    //    }
                    //    }
                    //],
                        //Title configuration (optional)
                        title: {
                            text: self.assessmentName + ' ' + self.fieldToRetrieve
                        },
                        //Boolean to control showng loading status on chart (optional)
                        //Could be a string if you want to show specific loading text.
                        loading: false,
                        //Configuration for the xAxis (optional). Currently only one x axis can be dynamically controlled.
                        //properties currentMin and currentMax provied 2-way binding to the chart's maximimum and minimum
                        xAxis: {
                            currentMin: 1,
                            currentMax: currentMax,
                            title: { text: 'Test Number' },
                            //plotLines: plotlines,
                            allowDecimals: false
                        },
                        yAxis: yAxis,
                        //Whether to use HighStocks instead of HighCharts (optional). Defaults to false.
                        useHighStocks: false,
                        //size (optional) if left out the chart will default to size of the div or something sensible.
                        size: {
                            height: 600
                        },
                        //function (optional)
                        func: function (chart) {
                            //setup some logic for the chart
                        }

                    };

                    self.chartConfig = highchartsNgConfig;

                    Highcharts.wrap(Highcharts.Axis.prototype, 'getPlotLinePath', function (proceed) {
                        var path = proceed.apply(this, Array.prototype.slice.call(arguments, 1));
                        if (path) {
                            path.flat = false;
                        }
                        return path;
                    });
                }

                self.getBackgroundClass = function (studentFieldScore, testNumber) {
                    var self = this;
                    var bgClass = '';

                        bgClass = self.getIntColor(studentFieldScore, testNumber);

                    // see if this is an intervention date
                    //bgClass += ' ' + self.getTddClass(tddId);

                    return bgClass;
                };

                self.formatDate = function (inDate) {
                    if (inDate != null) {
                        if (inDate.indexOf('T') >= 0) {
                            return moment(inDate.substring(0, inDate.indexOf('T'))).format('DD-MMM-YY');
                        } else {
                            return moment(inDate).format('DD-MMM-YY');
                        }
                    }
                    else {
                        return 'N/A';
                    }
                }
                self.formatDate2 = function (inDate) {
                    if (inDate != null) {
                        return moment(inDate).format('DD-MMM-YYYY');
                    }
                    else {
                        return 'N/A';
                    }
                }

                self.getDisplayValue = function (id) {
                    var self = this;
                    var lookupList = self.VScale;

                    if (!self.status.isLookup) {
                        return id;
                    }

                    for (var i = 0; i < lookupList.length; i++) {
                        if (lookupList[i].FieldSpecificId == id) {
                            return lookupList[i].FieldValue;
                        }
                    }

                    return 'N/A';
                };

                /* Private Functions */
                //this.getTddClass = function (tddId) {
                //    var self = this;
                //    for (var i = 0; i < self.Interventions.length; i++) {
                //        if (tddId === self.Interventions[i].StartTDDID ||
                //            tddId === self.Interventions[i].EndTDDID) {
                //            return 'interventionCell';
                //        }
                //    }
                //    return '';
                //}


                self.getIntColor = function (studentFieldScore, tddId) {
                    var self = this;
                    // TODO: dont forget to add a check for NULL Benchmarks, what happens?
                    for (var i = 0; i < self.BenchmarkDates.length; i++) {
                        if (self.BenchmarkDates[i].TestNumber === tddId) {
                            if (studentFieldScore.FieldValueID != null) {
                                // not defined yet
                                //if (studentFieldScore.IntValue === $scope.Benchmarks[i].MaxScore) {
                                //	return 'obsGreen';
                                //}
                                if (studentFieldScore.FieldValueID >= self.BenchmarkDates[i].Exceeds && self.BenchmarkDates[i].Exceeds != null) {
                                    return 'obsBlue';
                                }
                                else if (studentFieldScore.FieldValueID >= self.BenchmarkDates[i].Meets && self.BenchmarkDates[i].Meets != null) {
                                    return '';
                                }
                                else if (studentFieldScore.FieldValueID >= self.BenchmarkDates[i].Approaches && self.BenchmarkDates[i].Approaches != null) {
                                    return 'obsYellow';
                                }
                                else if (studentFieldScore.FieldValueID < self.BenchmarkDates[i].Approaches && self.BenchmarkDates[i].Approaches != null) {
                                    return 'obsRed';
                                }
                            }
                        }
                    }
                    return '';
                }
                /* End Private Functions */

            };

            return (NSAssesssmentIGLineGraphManager);
        }
    ]);

    /* Movies List Controller  */
    LineGraphController.$inject = ['$scope', 'InterventionGroup', '$q', '$http', 'pinesNotifications', '$location', '$routeParams', 'NSAssesssmentLineGraphManager', 'nsFilterOptionsService', 'NSStudentInterventionManager', 'spinnerService'];
    function LineGraphController($scope, InterventionGroup, $q, $http, pinesNotifications, $location, $routeParams, NSAssesssmentLineGraphManager, nsFilterOptionsService, NSStudentInterventionManager, spinnerService) {
        var vm = this;
        vm.dataManager = new NSAssesssmentLineGraphManager();
        vm.studentDataManager = new NSStudentInterventionManager();
        vm.filterOptions = nsFilterOptionsService.options;
        vm.settings = { selectedAssessmentField: null, batchPrint: false};

        vm.dataManager.LoadAssessmentFields();

        if ($location.absUrl().indexOf('printmode=') >= 0) {
            // unencode Hfw Ranges URL Param... get student from routeparams
            if (angular.isDefined($location.$$search.AssessmentField)) {
                vm.settings.selectedAssessmentField = JSON.parse(decodeURIComponent($location.$$search.AssessmentField));
            }
        }

        if ($location.absUrl().indexOf('batchprint=') >= 0) {
            vm.settings.batchPrint = true;
        }

        function LoadData() {
            if (vm.filterOptions.selectedSectionStudent != null && vm.filterOptions.selectedClassroomAssessmentField != null && angular.isDefined(vm.filterOptions.selectedClassroomAssessmentField.FieldName)) {
                spinnerService.show('tableSpinner');
                vm.studentDataManager.LoadData(vm.filterOptions.selectedSectionStudent.id).then(function (response) {
                    vm.dataManager.LoadData(vm.filterOptions.selectedClassroomAssessmentField.AssessmentId, vm.filterOptions.selectedClassroomAssessmentField.FieldName, vm.filterOptions.selectedClassroomAssessmentField.LookupFieldName, vm.filterOptions.selectedClassroomAssessmentField.FieldType, vm.filterOptions.selectedSectionStudent.id, vm.filterOptions.selectedClassroomAssessmentField.DisplayLabel, vm.filterOptions.selectedSection.id, vm.filterOptions.selectedClassroomAssessmentField.AssessmentName, vm.studentDataManager.Interventions)
                    .finally(function (response) {
                        spinnerService.hide('tableSpinner');
                    });
                    //  }
                });
            }
        }

        $scope.$on('NSSectionStudentOptionsUpdated', function (event, data) {
            LoadData();
        });

        $scope.$on('NSClassroomAssessmentFieldOptionsUpdated', function (event, data) {
            LoadData();
        });

        $scope.$on('NSInitialLoadComplete', function (event, data) {
            LoadData();
        });


        // watch selectedField
        //$scope.$watch('vm.filterOptions.selectedClassroomAssessmentField', function (newVal, oldVal) {
        //    if (newVal !== oldVal && newVal != null) {
        //        if (vm.filterOptions.selectedSectionStudent != null && angular.isDefined(vm.filterOptions.selectedClassroomAssessmentField.FieldName)) {
        //            spinnerService.show('tableSpinner');
        //            vm.dataManager.LoadData(vm.filterOptions.selectedClassroomAssessmentField.AssessmentId, vm.filterOptions.selectedClassroomAssessmentField.FieldName, vm.filterOptions.selectedClassroomAssessmentField.LookupFieldName, vm.filterOptions.selectedClassroomAssessmentField.FieldType, vm.filterOptions.selectedSectionStudent.id, vm.filterOptions.selectedClassroomAssessmentField.DisplayLabel, vm.filterOptions.selectedSection.id, vm.filterOptions.selectedClassroomAssessmentField.AssessmentName, vm.studentDataManager.Interventions)
        //            .finally(function (response) {
        //                spinnerService.hide('tableSpinner');
        //            });
        //        }
        //    }
        //}, true);

        //$scope.$watch('vm.filterOptions.selectedSectionStudent', function (newVal, oldVal) {
        //    if (newVal !== oldVal && newVal != null) {
        //        if (vm.filterOptions.selectedSectionStudent != null) {
        //            vm.studentDataManager.LoadData(vm.filterOptions.selectedSectionStudent.id).then(function (response) {
        //                if (vm.filterOptions.selectedClassroomAssessmentField != null && angular.isDefined(vm.filterOptions.selectedClassroomAssessmentField.FieldName)) {
        //                    spinnerService.show('tableSpinner');
        //                    vm.dataManager.LoadData(vm.filterOptions.selectedClassroomAssessmentField.AssessmentId, vm.filterOptions.selectedClassroomAssessmentField.FieldName, vm.filterOptions.selectedClassroomAssessmentField.LookupFieldName, vm.filterOptions.selectedClassroomAssessmentField.FieldType, vm.filterOptions.selectedSectionStudent.id, vm.filterOptions.selectedClassroomAssessmentField.DisplayLabel, vm.filterOptions.selectedSection.id, vm.filterOptions.selectedClassroomAssessmentField.AssessmentName, vm.studentDataManager.Interventions)
        //                    .finally(function (response) {
        //                        spinnerService.hide('tableSpinner');
        //                    });
        //                }
        //            });

        //        }
        //    }
        //}, true);
    }

    LineGraphIGController.$inject = ['$scope', 'InterventionGroup', '$q', '$http', 'pinesNotifications', '$location', '$routeParams', 'NSAssesssmentIGLineGraphManager','nsFilterOptionsService','NSStudentInterventionManager'];
    function LineGraphIGController($scope, InterventionGroup, $q, $http, pinesNotifications, $location, $routeParams, NSAssesssmentIGLineGraphManager, nsFilterOptionsService, NSStudentInterventionManager) {
        var vm = this;
        vm.dataManager = new NSAssesssmentIGLineGraphManager();
        vm.studentDataManager = new NSStudentInterventionManager();
        vm.filterOptions = nsFilterOptionsService.options;
        vm.loadedViaStartup = false;
        //vm.settings = { selectedAssessmentField: {} };

        vm.dataManager.LoadAssessmentFields();

        function LoadData(fromAttributeChange) {
            //if(fromAttributeChange == true || (fromAttributeChange == false))
            if (vm.filterOptions.selectedInterventionGroupAssessmentField != null && angular.isDefined(vm.filterOptions.selectedInterventionGroupAssessmentField.FieldName) && vm.filterOptions.selectedInterventionStudent != null) {
                vm.dataManager.LoadData(vm.filterOptions.selectedInterventionGroupAssessmentField.AssessmentId, vm.filterOptions.selectedInterventionGroupAssessmentField.FieldName, vm.filterOptions.selectedInterventionGroupAssessmentField.LookupFieldName, vm.filterOptions.selectedInterventionGroupAssessmentField.FieldType, vm.filterOptions.selectedInterventionStudent.id, vm.filterOptions.selectedInterventionGroupAssessmentField.DisplayLabel, vm.filterOptions.selectedInterventionGroup.id, vm.filterOptions.selectedInterventionGroupAssessmentField.AssessmentName, vm.studentDataManager.Interventions, vm.filterOptions.selectedSchoolYear.id);
                vm.dataManager.fieldName = vm.filterOptions.selectedInterventionGroupAssessmentField.DisplayLabel;
            }
        }

        //$scope.$watchGroup(['vm.filterOptions.selectedInterventionGroupAssessmentField', 'vm.filterOptions.selectedInterventionStudent'], function (newValue, oldValue, scope) {
        //    if (angular.isDefined(newValue[0]) && angular.isDefined(newValue[1])) {
        //        if (newValue[0] != oldValue[0] || newValue[1] != oldValue[1]) {
        //            LoadData(true);
        //        }
        //    }
        //});



        // watch selectedField
        $scope.$watch('vm.filterOptions.selectedInterventionGroupAssessmentField', function (newVal, oldVal) {
            if (newVal !== oldVal) {
                if (vm.filterOptions.selectedInterventionStudent != null && angular.isDefined(vm.filterOptions.selectedInterventionGroupAssessmentField.FieldName)) {
                    //vm.dataManager.LoadData(vm.filterOptions.selectedInterventionGroupAssessmentField.AssessmentId, vm.filterOptions.selectedInterventionGroupAssessmentField.FieldName, vm.filterOptions.selectedInterventionGroupAssessmentField.LookupFieldName, vm.filterOptions.selectedInterventionGroupAssessmentField.FieldType, vm.filterOptions.selectedInterventionStudent.id, vm.filterOptions.selectedInterventionGroupAssessmentField.DisplayLabel, vm.filterOptions.selectedInterventionGroup.id, vm.filterOptions.selectedInterventionGroupAssessmentField.AssessmentName, vm.studentDataManager.Interventions, vm.filterOptions.selectedSchoolYear.id);
                    //vm.dataManager.fieldName = vm.filterOptions.selectedInterventionGroupAssessmentField.DisplayLabel;
                    LoadData(true);
                }
            }
        }, true);

        $scope.$watch('vm.filterOptions.selectedInterventionStudent', function (newVal, oldVal) {
            if (newVal !== oldVal) {
                if (vm.filterOptions.selectedInterventionStudent != null) {
                    vm.studentDataManager.LoadData(vm.filterOptions.selectedInterventionStudent.id).then(function (response) {
                        if (vm.filterOptions.selectedInterventionGroupAssessmentField != null && angular.isDefined(vm.filterOptions.selectedInterventionGroupAssessmentField.FieldName)) {
                            //vm.dataManager.LoadData(vm.filterOptions.selectedInterventionGroupAssessmentField.AssessmentId, vm.filterOptions.selectedInterventionGroupAssessmentField.FieldName, vm.filterOptions.selectedInterventionGroupAssessmentField.LookupFieldName, vm.filterOptions.selectedInterventionGroupAssessmentField.FieldType, vm.filterOptions.selectedInterventionStudent.id, vm.filterOptions.selectedInterventionGroupAssessmentField.DisplayLabel, vm.filterOptions.selectedInterventionGroup.id, vm.filterOptions.selectedInterventionGroupAssessmentField.AssessmentName, vm.studentDataManager.Interventions, vm.filterOptions.selectedSchoolYear.id);
                            //vm.dataManager.fieldName = vm.filterOptions.selectedInterventionGroupAssessmentField.DisplayLabel;
                            LoadData(true);
                        }
                    });
                }
            }
        }, true);

        LoadData(false);
    }



})();