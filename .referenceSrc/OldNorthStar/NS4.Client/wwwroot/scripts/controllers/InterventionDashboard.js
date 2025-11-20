(function () {
    'use strict';

    angular
        .module('interventionDashboardModule', [])
        .controller('InterventionDashboardController', [
            'nsFilterOptionsService', 'NSInterventionDashboardManager','$scope', 'NSPieChartManager',
            function (nsFilterOptionsService, NSInterventionDashboardManager, $scope, NSPieChartManager) {
                $scope.filterOptions = nsFilterOptionsService.options;
                $scope.dashMgr = new NSInterventionDashboardManager();
                $scope.pieMgr = new NSPieChartManager();
                $scope.settings = {};

                //$scope.$watch('filterOptions.selectedStudent.id', function (newVal, oldVal) {
                //    if (newVal !== oldVal) {
                //        $scope.dashMgr.LoadStints(newVal, $scope.filterOptions.selectedInterventionGroup.id);
                //    }
                //});

                $scope.$watch('filterOptions.selectedStint.id', function (newVal, oldVal) {
                    if (newVal !== oldVal) {
                        if(angular.isDefined(newVal) && newVal !== null)
                            $scope.dashMgr.LoadAttendanceSummary($scope.filterOptions.selectedStudent.id, $scope.filterOptions.selectedStint.id).then(function(response) {
                                $scope.pieMgr.SetData($scope.dashMgr.AttendanceSummary)
                            });
                    }
                });
        }])
        .factory('NSInterventionDashboardManager', ['$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
            var NSInterventionDashboardManager = function () {

                var self = this;

                //self.LoadStints = function (studentId, interventionGroupId) {
                //    var url = webApiBaseUrl + '/api/interventiongroup/GetInterventionGroupStints';
                //    var paramObj = { StudentId: studentId, InterventionGroupId: interventionGroupId }
                //    var promise = $http.post(url, paramObj);

                //    return promise.then(function (response) {
                //        angular.extend(self, response.data);
                //        if (self.Stints === null) self.Stints = [];
                //    });
                //}

                self.LoadAttendanceSummary = function (studentId, stintId) {
                    var url = webApiBaseUrl + '/api/interventiondashboard/GetStintAttendanceSummary';
                    var paramObj = { StudentId: studentId, StintId: stintId }
                    var promise = $http.post(url, paramObj);

                    return promise.then(function (response) {
                        angular.extend(self, response.data);
                        if (self.AttendanceSummary === null) self.AttendanceSummary = [];
                        if (self.NotesGroupedByType === null) self.NotesGroupedByType = [];
                    });
                }
            }
            return NSInterventionDashboardManager;
        }])
        .directive('nsInterventionDashboardRow', ['$http', function ($http) {

        }])
    .factory('NSPieChartManager', [
        '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
            var NSPieChartManager = function () {
                var self = this;
                self.chartConfig = {};
                self.settings = { selectedStatus: undefined };

                self.SetData = function (attendanceSummary) {
                    if (attendanceSummary != null) {
                        self.AttendanceSummary = attendanceSummary;
                        self.postDataLoadSetup();
                    }
                }

                var selectPieSlice = function (slice) {
                    if (self.settings.selectedStatus == slice) {
                        self.settings.selectedStatus = undefined;
                    } else {
                        self.settings.selectedStatus = slice;
                    }
                }
             
                self.postDataLoadSetup = function () {
                    var self = this;
                    var currentMax = 1;
                    var data = [];
                    var attendanceColor = {
                        'Teacher Absent': '#FFC000',
                        'Teacher Unavailable': '#BF453D',
                        'Child Absent': '#EEC033',
                        'Child Unavailable': '#FF0000',
                        'No School': '#777777',
                        'Intervention Delivered': '#90ED7D',
                        'Make-Up Lesson': '#E4D354 ',
                        'Non-Cycle Day': '#4697ce',
                        'None': 'green',
                    }

                    var highchartsNgConfig = {};



                    // set up series
                    //color: attendanceColor[self.AttendanceSummary[i].StatusLabel]
                    for (var i = 0; i < self.AttendanceSummary.length; i++) {
                        data.push({ name: self.AttendanceSummary[i].StatusLabel, y: self.AttendanceSummary[i].Count });
                   
                    }

                    highchartsNgConfig = {
                        //This is not a highcharts object. It just looks a little like one!
                        options: {
                            //This is the Main Highcharts chart config. Any Highchart options are valid here.
                            //will be ovverriden by values specified below.
                            chart: {
                                type: 'pie',
                                plotShadow: false,
                                plotBorderWidth: null,
                                plotBackgroundColor: null
                            },
                            tooltip: {
                                formatter: function () {
                                    if (this.y > 0)
                                        return this.y + ' (' + this.percentage.toFixed(0) + '%)';
                                }
                            },
                            plotOptions: {
                                pie: {
                                    allowPointSelect: true,
                                    cursor: 'pointer',
                                    point: {
                                        events: {
                                            click: function (event) {
                                                selectPieSlice(this.name);
                                            }
                                        }
                                    },
                                    dataLabels: {
                                        enabled: true,
                                        format: '{point.y} ({point.percentage:.1f}%)',
                                        style: {
                                            color: (Highcharts.theme && Highcharts.theme.contrastTextColor) || 'black'
                                        }
                                    },
                                    showInLegend: true
                                }
                            }
                        },

                        //The below properties are watched separately for changes.

                        //Series object (optional) - a list of series using normal highcharts series options.
                        series: [
                            {
                                name: 'Attendance',
                                colorByPoint: true,
                                data: data
                            }
                        ],
                        //Title configuration (optional)
                        title: {
                            text: 'Attendance Summary'
                        },
                        //Boolean to control showng loading status on chart (optional)
                        //Could be a string if you want to show specific loading text.
                        loading: false,
                        size: {
                            height: 600,
                            width: 600
                        },
                        //function (optional)
                        func: function (chart) {
                            //setup some logic for the chart
                        }

                    };

                    self.chartConfig = highchartsNgConfig;
                }
            };

            return (NSPieChartManager);
        }
    ]);
})();