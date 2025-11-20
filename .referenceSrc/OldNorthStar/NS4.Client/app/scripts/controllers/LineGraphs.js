(function () {
    'use strict';

    angular
        .module('lineGraphModule', [])
        .directive('lineGraphDetail', ['$http', function ($http) {
            return {
                restrict: 'E',
                templateUrl: 'templates/linegraph-detail.html',
                scope: {
                    dataManager: '='
                }
            }
        }])
        .directive('studentInterventions', ['$http', function ($http) {
            return {
                restrict: 'E',
                templateUrl: 'templates/student-interventions.html',
                scope: {
                    dataManager: '='
                }
            }
        }])
        .controller('LineGraphController', LineGraphController)
        .controller('LineGraphIGController', LineGraphIGController)
        .controller('LineGraphMNStateController', LineGraphMNStateController)
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
        .factory('NSAssesssmentLineGraphManager', [
        '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
            var NSAssesssmentLineGraphManager = function () {
                var self = this;

                self.LoadData = function (assessmentId, fieldToRetrieve, lookupFieldName, fieldType, studentId, fieldDisplayName, sectionId, assessmentName, interventions) {
                   
                    self.assessmentName = assessmentName;
                    self.fieldToRetrieve = fieldDisplayName;
                    self.status = {};
                    self.status.isLookup = (lookupFieldName != null && lookupFieldName != '');
                    self.status.isDecimalField = false; // TODO: check field type
                    var returnObject = { AssessmentId: assessmentId, FieldToRetrieve: fieldToRetrieve, LookupFieldName: lookupFieldName, IsLookupColumn: self.status.isLookup, StudentId: studentId };

                    var url = webApiBaseUrl + '/api/lineGraph/GetStudentLineGraph';
                    var returnData = $http.post(url, returnObject);

                    self.Results = [];
                    self.BenchmarkDates = [];
                    self.Interventions = (interventions == null) ? [] : interventions;
                    self.VScale = [];
                   // self.Fields = {};
                    self.chartConfig = {};

                    return returnData.then(function (response) {
                        angular.extend(self, response.data);
                        if (self.Results === null) self.Results = [];
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
                    }, function (response) {
                        // error callback function

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

                        // set up interventions
                        // TODO: create a formula for the Y value.  something like +30 for every time through
                        for (var j = 0; j < self.Interventions.length; j++) {
                            if (self.BenchmarkDates[i].TestDueDateID == self.Interventions[j].StartTDDID) {
                                plotlines[plotLinesLoopCounter] = {
                                    color: '#2244BB', dashStyle: 'solid', value: currentMax, width: '2',
                                    label: { text: 'Start ' + self.Interventions[j].InterventionType, align: 'left', y: 20, x: 3, rotation: 0, style: { fontFamily: 'Arial' } },
                                    zIndex: 25
                                }
                                plotLinesLoopCounter++;
                            }
                            if (self.BenchmarkDates[i].TestDueDateID == self.Interventions[j].EndTDDID) {
                                plotlines[plotLinesLoopCounter] = {
                                    color: '#2244BB', dashStyle: 'solid', value: currentMax, width: '2',
                                    label: { text: 'End ' + self.Interventions[j].InterventionType, align: 'left', y: 45, x: 3, rotation: 0, style: { fontFamily: 'Arial' } },
                                    zIndex: 25
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

                    highchartsNgConfig = {
                        //This is not a highcharts object. It just looks a little like one!
                        options: {
                            //This is the Main Highcharts chart config. Any Highchart options are valid here.
                            //will be ovverriden by values specified below.
                            chart: {
                                type: 'line'
                            },
                            tooltip: {
                                formatter: function () {
                                    var value = null;
                                    var comments = '';

                                    for (var j = 0; j < self.Results.length; j++) {
                                        if (self.Results[j].x == this.x) {
                                            comments = self.Results[j].Comments;
                                        }
                                    }

                                    for (var i = 0; i < self.VScale.length; i++) {
                                        if (self.VScale[i].FieldSpecificId == this.y) {
                                            if (comments != '' && comments != null && angular.isDefined(comments)) {
                                                return self.VScale[i].FieldValue + "<br> <b>Comments:</b> " + comments;
                                            } else return self.VScale[i].FieldValue;
                                        }
                                    }
                                    return this.y;
                                }
                            }
                        },

                        //The below properties are watched separately for changes.

                        //Series object (optional) - a list of series using normal highcharts series options.
                        series: [
                            {
                                name: self.fieldToRetrieve,
                                color: "#2244BB",
                                data: data,
                                lineWidth: 3,
                                shadow: { offsetX: 3, offsetY: 2, opacity: 0.12, width: 6 },
                                zIndex: 100,
                                marker: {
                                    fillColor: '#FFFFFF',
                                    lineWidth: 2,
                                    lineColor: null // inherit from series
                                }
                            },
                            {
                                name: "Does Not Meet",
                                color: "#BF453D",
                                data: doesnotmeetData,
                                lineWidth: 3,
                                shadow: false,
                                zIndex: 1,
                                dashStyle: 'ShortDash',
                                marker: { enabled: false }
                            },
                            {
                                name: "Approaches",
                                color: "#E4D354",
                                data: approachesData,
                                lineWidth: 3,
                                shadow: false,
                                zIndex: 3,
                                dashStyle: 'ShortDash',
                                marker: { enabled: false }
                            },
                            {
                                name: "Meets",
                                color: "#ABF09C",
                                data: meetsData,
                                lineWidth: 3,
                                shadow: false,
                                zIndex: 2,
                                dashStyle: 'ShortDash',
                                marker: { enabled: false }
                            },
                            {
                                name: "Exceeds",
                                color: "#4697ce",
                                data: exceedsData,
                                lineWidth: 3,
                                shadow: false,
                                zIndex: 2,
                                dashStyle: 'ShortDash',
                                marker: { enabled: false }
                            }
                        ],
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
                                if (studentFieldScore.FieldValueID >= self.BenchmarkDates[i].Exceeds) {
                                    return 'obsBlue';
                                }
                                else if (studentFieldScore.FieldValueID >= self.BenchmarkDates[i].Meets) {
                                    return '';
                                }
                                else if (studentFieldScore.FieldValueID >= self.BenchmarkDates[i].Approaches || studentFieldScore.FieldValueID >= self.BenchmarkDates[i].DoesNotMeet) {
                                    return 'obsYellow';
                                }
                                else if (studentFieldScore.FieldValueID <= self.BenchmarkDates[i].DoesNotMeet) {
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
                    self.Interventions = [];
                    self.VScale = [];
                    //self.Fields = {};
                    self.chartConfig = {};

                    returnData.then(function (response) {
                        angular.extend(self, response.data);
                        if (self.Results === null) self.Results = [];
                        if (self.BenchmarkDates === null) self.BenchmarkDates = [];
                        //if (self.Interventions === null) self.Interventions = [];
                        //if (self.VScale === null) self.VScale = [];
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

                    highchartsNgConfig = {
                        //This is not a highcharts object. It just looks a little like one!
                        options: {
                            //This is the Main Highcharts chart config. Any Highchart options are valid here.
                            //will be ovverriden by values specified below.
                            chart: {
                                type: 'line'
                            },
                            tooltip: {
                                formatter: function () {
                                    var value = null;
                                    var comments = '';

                                    for (var j = 0; j < self.Results.length; j++) {
                                        if (self.Results[j].x == this.x) {
                                            comments = self.Results[j].Comments;
                                        }
                                    }

                                    for (var i = 0; i < self.VScale.length; i++) {
                                        if (self.VScale[i].FieldSpecificId == this.y) {
                                            return self.VScale[i].FieldValue + " comments: " + comments;
                                        }
                                    }
                                    return this.y;
                                }
                            }
                        },

                        //The below properties are watched separately for changes.

                        //Series object (optional) - a list of series using normal highcharts series options.
                        series: [
                            {
                                name: self.fieldToRetrieve,
                                color: "#2244BB",
                                data: data,
                                lineWidth: 3,
                                shadow: { offsetX: 3, offsetY: 2, opacity: 0.12, width: 6},
                                zIndex: 100,
                                marker: {
                                    fillColor: '#FFFFFF',
                                    lineWidth: 2,
                                    lineColor: null // inherit from series
                                    }
                            },
                            {
                                name: "Does Not Meet",
                                color: "#BF453D",
                                data: doesnotmeetData,
                                lineWidth: 3,
                                shadow: false,
                                zIndex: 1,
                                dashStyle: 'ShortDash',
                                marker: {
                                    enabled: false
                                }
                            },
                                {
                                    name: "Approaches",
                                    color: "#E4D354",
                                    data: approachesData,
                                    lineWidth: 3,
                                    shadow: false,
                                    zIndex: 3,
                                    dashStyle: 'ShortDash',
                                    marker: {
                                    enabled: false
                                }
                            },
                                {
                                    name: "Meets",
                                    color: "#ABF09C",
                                    data: meetsData,
                                    lineWidth: 3,
                                    shadow: false,
                                    zIndex: 2,
                                    dashStyle: 'ShortDash',
                                        marker: {
                                    enabled: false
                                }
                            },
                        {
                            name: "Exceeds",
                                        color: "#4697ce",
                                        data: exceedsData,
                            lineWidth: 3,
                            shadow: false,
                            zIndex: 2,
                            dashStyle: 'ShortDash',
                            marker: {
                            enabled: false
                        }
                        }
                    ],
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
                        return moment(inDate).format('DD-MMM-YY');
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
                        if (self.BenchmarkDates[i].TestDueDateID === tddId) {
                            if (studentFieldScore.FieldValueID != null) {
                                // not defined yet
                                //if (studentFieldScore.IntValue === $scope.Benchmarks[i].MaxScore) {
                                //	return 'obsGreen';
                                //}
                                if (studentFieldScore.FieldValueID >= self.BenchmarkDates[i].Exceeds) {
                                    return 'obsBlue';
                                }
                                if (studentFieldScore.FieldValueID >= self.BenchmarkDates[i].Meets) {
                                    return '';
                                }
                                else if (studentFieldScore.FieldValueID >= self.BenchmarkDates[i].Approaches || studentFieldScore.FieldValueID >= self.BenchmarkDates[i].DoesNotMeet) {
                                    return 'obsYellow';
                                }
                                if (studentFieldScore.FieldValueID <= self.BenchmarkDates[i].DoesNotMeet) {
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
    LineGraphController.$inject = ['$scope', 'InterventionGroup', '$q', '$http', 'pinesNotifications', '$location', '$routeParams', 'NSAssesssmentLineGraphManager', 'nsFilterOptionsService', 'NSStudentInterventionManager'];
    function LineGraphController($scope, InterventionGroup, $q, $http, pinesNotifications, $location, $routeParams, NSAssesssmentLineGraphManager, nsFilterOptionsService, NSStudentInterventionManager) {
        var vm = this;
        vm.dataManager = new NSAssesssmentLineGraphManager();
        vm.studentDataManager = new NSStudentInterventionManager();
        vm.filterOptions = nsFilterOptionsService.options;
        vm.settings = { selectedAssessmentField: {}};

        vm.dataManager.LoadAssessmentFields();

        // watch selectedField
        $scope.$watch('vm.settings.selectedAssessmentField', function (newVal, oldVal) {
            if (newVal !== oldVal) {
                if (vm.filterOptions.selectedStudent != null && angular.isDefined(vm.settings.selectedAssessmentField.FieldName)) {
                    vm.dataManager.LoadData(vm.settings.selectedAssessmentField.AssessmentId, vm.settings.selectedAssessmentField.FieldName, vm.settings.selectedAssessmentField.LookupFieldName, vm.settings.selectedAssessmentField.FieldType, vm.filterOptions.selectedStudent.id, vm.settings.selectedAssessmentField.DisplayLabel, vm.filterOptions.selectedSection.id, vm.settings.selectedAssessmentField.AssessmentName, vm.studentDataManager.Interventions);
                }
            }
        }, true);

        $scope.$watch('vm.filterOptions.selectedStudent', function (newVal, oldVal) {
            if (newVal !== oldVal) {
                if (vm.filterOptions.selectedStudent != null) {
                    vm.studentDataManager.LoadData(vm.filterOptions.selectedStudent.id).then(function (response) {
                        if (angular.isDefined(vm.settings.selectedAssessmentField.FieldName)) {
                            vm.dataManager.LoadData(vm.settings.selectedAssessmentField.AssessmentId, vm.settings.selectedAssessmentField.FieldName, vm.settings.selectedAssessmentField.LookupFieldName, vm.settings.selectedAssessmentField.FieldType, vm.filterOptions.selectedStudent.id, vm.settings.selectedAssessmentField.DisplayLabel, vm.filterOptions.selectedSection.id, vm.settings.selectedAssessmentField.AssessmentName, vm.studentDataManager.Interventions);
                        }
                    });
                }
            }
        }, true);
    }

    LineGraphIGController.$inject = ['$scope', 'InterventionGroup', '$q', '$http', 'pinesNotifications', '$location', '$routeParams', 'NSAssesssmentIGLineGraphManager','nsFilterOptionsService','NSStudentInterventionManager'];
    function LineGraphIGController($scope, InterventionGroup, $q, $http, pinesNotifications, $location, $routeParams, NSAssesssmentIGLineGraphManager, nsFilterOptionsService, NSStudentInterventionManager) {
        var vm = this;
        vm.dataManager = new NSAssesssmentIGLineGraphManager();
        vm.studentDataManager = new NSStudentInterventionManager();
        vm.filterOptions = nsFilterOptionsService.options;
        vm.settings = { selectedAssessmentField: {} };

        vm.dataManager.LoadAssessmentFields();

        // watch selectedField
        $scope.$watch('vm.settings.selectedAssessmentField', function (newVal, oldVal) {
            if (newVal !== oldVal) {
                if (vm.filterOptions.selectedStudent != null && angular.isDefined(vm.settings.selectedAssessmentField.FieldName)) {
                    vm.dataManager.LoadData(vm.settings.selectedAssessmentField.AssessmentId, vm.settings.selectedAssessmentField.FieldName, vm.settings.selectedAssessmentField.LookupFieldName, vm.settings.selectedAssessmentField.FieldType, vm.filterOptions.selectedStudent.id, vm.settings.selectedAssessmentField.DisplayLabel, vm.filterOptions.selectedInterventionGroup.id, vm.settings.selectedAssessmentField.AssessmentName, vm.studentDataManager.Interventions, vm.filterOptions.selectedSchoolYear.id);
                }
            }
        }, true);

        $scope.$watch('vm.filterOptions.selectedStudent', function (newVal, oldVal) {
            if (newVal !== oldVal) {
                if (vm.filterOptions.selectedStudent != null) {
                    vm.studentDataManager.LoadData(vm.filterOptions.selectedStudent.id).then(function (response) {
                        if (angular.isDefined(vm.settings.selectedAssessmentField.FieldName)) {
                            vm.dataManager.LoadData(vm.settings.selectedAssessmentField.AssessmentId, vm.settings.selectedAssessmentField.FieldName, vm.settings.selectedAssessmentField.LookupFieldName, vm.settings.selectedAssessmentField.FieldType, vm.filterOptions.selectedStudent.id, vm.settings.selectedAssessmentField.DisplayLabel, vm.filterOptions.selectedInterventionGroup.id, vm.settings.selectedAssessmentField.AssessmentName, vm.studentDataManager.Interventions, vm.filterOptions.selectedSchoolYear.id);
                        }
                    });
                }
            }
        }, true);
    }

/* Movies List Controller  */
    LineGraphMNStateController.$inject = ['$scope', 'InterventionGroup', '$q', '$http', 'pinesNotifications', '$location'];


    function LineGraphMNStateController($scope, InterventionGroup, $q, $http, pinesNotifications, $location) {
        $scope.studentResults = {};
        $scope.benchmarkDates = {};
        $scope.chartConfig = {};

        var highchartsNgConfig = {};
        
        /// TODO: Go back and change this to a GET resource query instead of a POST... will need this for Printing
        $http.post('http://localhost:16726/api/lineGraph/GetStudentLineGraph',{ AssessmentId: 1, FieldToRetrieve: 'FPValueID', LookupFieldName: 'FPScale', IsLookupColumn: true, StudentId: 26303 }).success(function (data) {
            $scope.studentResults = data.Results;
            $scope.benchmarkDates = data.BenchmarkDates;
            $scope.interventions = data.Interventions;

            var currentMax = 1;
            var data = [];
            var lowData = [];
            var meanData = [];
            var highData = [];
            var plotlines = [];
            var loopCounter = 0;
            var plotLinesLoopCounter = 0;

            // set up series
            for (var i = 0; i < $scope.benchmarkDates.length; i++) {
                currentMax = $scope.benchmarkDates[i].TestNumber;

                // set benchmark data series
                lowData[i] = [i, $scope.benchmarkDates[i].TwentiethPercentileID];
                meanData[i] = [i, $scope.benchmarkDates[i].MeanID];
                highData[i] = [i, $scope.benchmarkDates[i].EightiethPercentileID];

                for (var j = 0; j < $scope.studentResults.length; j++) {
                    if ($scope.benchmarkDates[i].TestDueDateID == $scope.studentResults[j].TestDueDateID) {
                        data[loopCounter] = [i, $scope.studentResults[j].FieldValueID];
                        loopCounter++;
                    }
                }

                // set up interventions
                for (var j = 0; j < $scope.interventions.length; j++) {
                    if ($scope.benchmarkDates[i].TestDueDateID == $scope.interventions[j].StartTDDID) {
                        plotlines[plotLinesLoopCounter] = {
                            color: '#2244BB', dashStyle: 'solid', value: currentMax, width: '2',
                            label: { text: 'Start ' + $scope.interventions[j].InterventionType, align: 'left' }
                        }
                        plotLinesLoopCounter++;
                    }
                    if ($scope.benchmarkDates[i].TestDueDateID == $scope.interventions[j].EndTDDID) {
                        plotlines[plotLinesLoopCounter] = {
                            color: '#2244BB', dashStyle: 'solid', value: currentMax, width: '2',
                            label: { text: 'End ' + $scope.interventions[j].InterventionType, align: 'left' }
                        }
                        plotLinesLoopCounter++;
                    }
                }
            }




            highchartsNgConfig = {
                //This is not a highcharts object. It just looks a little like one!
                options: {
                    //This is the Main Highcharts chart config. Any Highchart options are valid here.
                    //will be ovverriden by values specified below.
                    chart: {
                        type: 'line'
                    },
                    tooltip: {
                        style: {
                            padding: 10,
                            fontWeight: 'bold'
                        }
                    }
                },

                //The below properties are watched separately for changes.

                //Series object (optional) - a list of series using normal highcharts series options.
                series: [
                    {
                        name: "FP Score",
                        color: "#7cb5ec",
                        data: data,
                        lineWidth: 3,
                        shadow: true
                    },
                    {
                        name: "20th Percentile",
                        color: "#BF453D",
                        data: lowData,
                        lineWidth: 3,
                        shadow: true
                    },
                    {
                        name: "Mean",
                        color: "#E4D354",
                        data: meanData,
                        lineWidth: 3,
                        shadow: true
                    },
                    {
                        name: "80th Percentile",
                        color: "#ABF09C",
                        data: highData,
                        lineWidth: 3,
                        shadow: true
                    }
                ],
                //Title configuration (optional)
                title: {
                    text: 'FP Text Leveling'
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
                yAxis: {
                    allowDecimals: false,
                    min: 0,
                    title: { text: 'Text Level' }
                },
                //Whether to use HighStocks instead of HighCharts (optional). Defaults to false.
                useHighStocks: false,
                //size (optional) if left out the chart will default to size of the div or something sensible.
                //size: {
                //    width: 700,
                //    height: 600
                //},
                //function (optional)
                func: function (chart) {
                    //setup some logic for the chart
                }

            };

            $scope.chartConfig = highchartsNgConfig;
        }).error(function (error) {
            alert('there was an error loading the line graph data');
        });
    }



})();