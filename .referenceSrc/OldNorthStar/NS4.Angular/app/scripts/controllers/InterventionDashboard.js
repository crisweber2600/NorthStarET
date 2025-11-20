(function () {
    'use strict';

    angular
        .module('interventionDashboardModule', [])
        .directive('interventionDashboard', [
            '$compile', '$templateCache','NSInterventionDashboardManager', 'NSPieChartManager', 'NSStudentAssessmentFieldManager', 'NSAssesssmentIGLineGraphManager', 'spinnerService', '$q',
            function ($compile, $templateCache, NSInterventionDashboardManager, NSPieChartManager, NSStudentAssessmentFieldManager, NSAssesssmentIGLineGraphManager, spinnerService, $q) {
                return {
                    restrict: 'EA',
                    templateUrl: 'templates/intervention-dashboard-directive.html',
                    scope: {
                        interventionGroup: '=',
                        interventionStudent: '=',
                        interventionStint: '=',
                        schoolYear: '='
                    },
                    link: function(scope, element, attr){
                        scope.studentAssessmentFieldsManager = new NSStudentAssessmentFieldManager();
                        scope.dashMgr = new NSInterventionDashboardManager();
                        scope.pieMgr = new NSPieChartManager();
                        scope.settings = { selectedInterventionGroup: {}, selectedInterventionStudent: {}, selectedStint: {}, selectedSchoolYear: {} };
                        scope.ClassLineGraphDataManagers = [];



                        scope.attendanceNoteColor = function (type) {
                            switch (type.AttendanceStatus) {
                                case 'None':
                                    return 'timeline-inverse';
                                case 'No School':
                                    return 'timeline-inverse';
                                case 'Intervention Delivered':
                                    return 'timeline-green';
                                case 'Make-Up Lesson':
                                    return 'timeline-warning';
                                default:
                                    return 'timeline-danger';
                            }
                        }

                        scope.selectPieSlice = function (status) {
                            scope.pieMgr.selectPieSlice(status);
                        }

                        scope.attendanceBadgeClass = function (AttendanceStatus) {
                            switch (AttendanceStatus) {
                                case 'None':
                                    return 'badge-inverse';
                                case 'No School':
                                    return 'badge-inverse';
                                case 'Intervention Delivered':
                                    return 'badge-success';
                                case 'Make-Up Lesson':
                                    return 'badge-warning';
                                default:
                                    return 'badge-danger';
                            }
                        }

                        scope.$watch('interventionStint.id', function (newVal, oldVal) {
                            if (angular.isDefined(newVal) && newVal !== null) {
                                scope.settings.selectedStint = scope.interventionStint;
                                scope.settings.selectedInterventionStudent = scope.interventionStudent;
                                scope.settings.selectedInterventionGroup = scope.interventionGroup;
                                scope.settings.selectedSchoolYear = scope.schoolYear;

                                scope.dashMgr.LoadAttendanceSummary(scope.settings.selectedInterventionStudent.id, scope.settings.selectedStint.id).then(function (response) {
                                    scope.pieMgr.SetData(scope.dashMgr.AttendanceSummary)
                                });
                            }
                        });

                        scope.$watch('interventionStudent.id', function (newVal, oldVal) {
                            if (angular.isDefined(newVal) && newVal !== null) {
                                scope.settings.selectedInterventionStudent = scope.interventionStudent;
                                scope.settings.selectedInterventionGroup = scope.interventionGroup;
                                scope.settings.selectedSchoolYear = scope.schoolYear;

                                LoadLineGraphs();
                            }
                        });

                        function LoadLineGraphs() {
                            spinnerService.show('tableSpinner');
                            scope.ClassLineGraphDataManagers = [];

                            scope.studentAssessmentFieldsManager.LoadData(2, scope.settings.selectedInterventionStudent.id, scope.settings.selectedInterventionGroup.id).then(function (response) {
                                var promiseCollection = [];

                                angular.forEach(scope.studentAssessmentFieldsManager.Fields, function (f) {
                                    var dataMgr = new NSAssesssmentIGLineGraphManager();
                                    promiseCollection.push(dataMgr.LoadData(f.AssessmentId, f.DatabaseColumn, f.LookupFieldName, f.FieldType, scope.settings.selectedInterventionStudent.id, f.DisplayLabel, scope.settings.selectedInterventionGroup.id, f.AssessmentName, null, scope.settings.selectedSchoolYear.id));
                                    scope.ClassLineGraphDataManagers.push(dataMgr);
                                });

                                $q.all(promiseCollection).then(function (response) {
                                    spinnerService.hide('tableSpinner');
                                })
                            });
                        }
                    }
                }
            }
        ])
        .controller('InterventionDashboardController', [
            'nsFilterOptionsService', 'NSInterventionDashboardManager','$scope', 'NSPieChartManager', 'NSStudentAssessmentFieldManager', 'NSAssesssmentIGLineGraphManager', 'spinnerService', '$q',
            function (nsFilterOptionsService, NSInterventionDashboardManager, $scope, NSPieChartManager, NSStudentAssessmentFieldManager, NSAssesssmentIGLineGraphManager, spinnerService, $q) {
                var origin = location.protocol + '//' + location.host + '/#/';
                $scope.targetPages = [{ url: origin + 'ig-dashboard-printall', label: 'Print Dashboard' }];
                $scope.filterOptions = nsFilterOptionsService.options;              
            }])
         .controller('InterventionDashboardControllerPrint', [
            'nsFilterOptionsService', 'NSInterventionDashboardManager', '$scope', 'NSPieChartManager', 'NSStudentAssessmentFieldManager', 'NSAssesssmentIGLineGraphManager', 'spinnerService', '$q',
            function (nsFilterOptionsService, NSInterventionDashboardManager, $scope, NSPieChartManager, NSStudentAssessmentFieldManager, NSAssesssmentIGLineGraphManager, spinnerService, $q) {
                var origin = location.protocol + '//' + location.host + '/#/';
                $scope.targetPages = [{ url: origin + 'ig-dashboard-printall', label: 'Print Dashboard' }];
                $scope.filterOptions = nsFilterOptionsService.options;
                $scope.studentAssessmentFieldsManager = new NSStudentAssessmentFieldManager();
                $scope.dashMgr = new NSInterventionDashboardManager();
                $scope.pieMgr = new NSPieChartManager();
                $scope.settings = {};
                $scope.ClassLineGraphDataManagers = [];



                $scope.attendanceNoteColor = function (type) {
                    switch (type.AttendanceStatus) {
                        case 'None':
                            return 'timeline-inverse';
                        case 'No School':
                            return 'timeline-inverse';
                        case 'Intervention Delivered':
                            return 'timeline-green';
                        case 'Make-Up Lesson':
                            return 'timeline-warning';
                        default:
                            return 'timeline-danger';
                    }
                }

                $scope.selectPieSlice = function (status) {
                    $scope.pieMgr.selectPieSlice(status);
                }

                $scope.attendanceBadgeClass = function (AttendanceStatus) {
                    switch (AttendanceStatus) {
                        case 'None':
                            return 'badge-inverse';
                        case 'No School':
                            return 'badge-inverse';
                        case 'Intervention Delivered':
                            return 'badge-success';
                        case 'Make-Up Lesson':
                            return 'badge-warning';
                        default:
                            return 'badge-danger';
                    }
                }

                $scope.$watch('filterOptions.selectedStint.id', function (newVal, oldVal) {
                    if (newVal !== oldVal) {
                        if (angular.isDefined(newVal) && newVal !== null)
                            $scope.dashMgr.LoadAttendanceSummary($scope.filterOptions.selectedInterventionStudent.id, $scope.filterOptions.selectedStint.id).then(function (response) {
                                $scope.pieMgr.SetData($scope.dashMgr.AttendanceSummary)
                            });
                    }
                });

                $scope.$watch('filterOptions.selectedInterventionStudent.id', function (newVal, oldVal) {
                    if (newVal !== oldVal) {
                        if (angular.isDefined(newVal) && newVal !== null) {
                            LoadLineGraphs();
                        }
                    }
                });

                function LoadLineGraphs() {
                    spinnerService.show('tableSpinner');
                    $scope.ClassLineGraphDataManagers = [];

                    $scope.studentAssessmentFieldsManager.LoadData(2, $scope.filterOptions.selectedInterventionStudent.id, $scope.filterOptions.selectedInterventionGroup.id).then(function (response) {
                        var promiseCollection = [];

                        angular.forEach($scope.studentAssessmentFieldsManager.Fields, function (f) {
                            var dataMgr = new NSAssesssmentIGLineGraphManager();
                            promiseCollection.push(dataMgr.LoadData(f.AssessmentId, f.DatabaseColumn, f.LookupFieldName, f.FieldType, $scope.filterOptions.selectedInterventionStudent.id, f.DisplayLabel, $scope.filterOptions.selectedInterventionGroup.id, f.AssessmentName, null, $scope.filterOptions.selectedSchoolYear.id));
                            $scope.ClassLineGraphDataManagers.push(dataMgr);
                        });

                        $q.all(promiseCollection).then(function (response) {
                            spinnerService.hide('tableSpinner');
                        })
                    });
                }
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
                        if (self.Notes === null) self.Notes = [];
                        if (self.AttendanceSummary === null) self.AttendanceSummary = [];

                        self.TotalDays = 0;
                        angular.forEach(self.AttendanceSummary, function (item) {
                            if (item.StatusLabel == "Intervention Delivered" || item.StatusLabel == "Make-Up Lesson")
                            self.TotalDays += item.Count;
                        });
                        //if (self.NotesGroupedByType === null) self.NotesGroupedByType = [];
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

                self.selectPieSlice = function (slice) {
                    if (self.settings.selectedStatus == slice || slice == null) {
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
                        'Teacher Absent': '#870000',
                        'Teacher Unavailable': '#D03737',
                        'Child Absent': '#AC1313',
                        'Child Unavailable': '#F45B5B',
                        'No School': '#434348',
                        'Intervention Delivered': '#90ED7D',
                        'Make-Up Lesson': '#E4D354 ',
                        'Non-Cycle Day': '#4697ce',
                        'None': '#cccccc',
                    }

                    var highchartsNgConfig = {};



                    // set up series
                    //color: attendanceColor[self.AttendanceSummary[i].StatusLabel]
                    for (var i = 0; i < self.AttendanceSummary.length; i++) {
                        data.push({ name: self.AttendanceSummary[i].StatusLabel, y: self.AttendanceSummary[i].Count, color: attendanceColor[self.AttendanceSummary[i].StatusLabel] });
                   
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
                            credits: {enabled: false},
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
                                                self.selectPieSlice(this.name);
                                            }
                                        }
                                    },
                                    dataLabels: {
                                        enabled: true,
                                        format: '{point.name} ({point.percentage:.1f}%)',
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
                            text: ''
                        },
                        //Boolean to control showng loading status on chart (optional)
                        //Could be a string if you want to show specific loading text.
                        loading: false,
                        //size: {
                        //    height: 600,
                        //    width: 600
                        //},
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