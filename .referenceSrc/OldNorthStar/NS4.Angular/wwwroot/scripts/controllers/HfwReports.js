(function () {
    'use strict';

    angular
        .module('hfwReportsModule', [])
        .controller('HfwStudentDetailReportController', ['$scope', 'InterventionGroup', '$q', '$http', 'nsPinesService', '$location', '$routeParams', 'hfwReportFactory','nsFilterOptionsService', '$timeout', '$bootbox', 'webApiBaseUrl', 'FileSaver', 'spinnerService',
            function ($scope, InterventionGroup, $q, $http, nsPinesService, $location, $routeParams, hfwReportFactory, nsFilterOptionsService, $timeout, $bootbox, webApiBaseUrl, FileSaver, spinnerService) {


                var LoadData = function () {

                    if ($location.absUrl().indexOf('printmode=') >= 0) {
                        // unencode Hfw Ranges URL Param... get student from routeparams
                    }

                    if ($scope.filterOptions.selectedSectionStudent && $scope.filterOptions.selectedHfwMultiRange && $scope.filterOptions.selectedHfwSortOrder) {
                        $timeout(function () {
                            spinnerService.show('tableSpinner');
                        });

                        $scope.factory.LoadReportDetailData($scope.filterOptions.selectedSectionStudent.id, $scope.filterOptions.selectedHfwMultiRange, $scope.filterOptions.selectedHfwSortOrder.id)
                            .then(function () { })
                            .finally(function () {
                                spinnerService.hide('tableSpinner');
                            });
                    }
                }
                
                $scope.processQuickSearchStudent = function () {
                    if (angular.isDefined($scope.filterOptions.quickSearchStudent)) {
                        spinnerService.show('tableSpinner');
                        $scope.factory.LoadReportDetailData($scope.filterOptions.quickSearchStudent.id, $scope.filterOptions.selectedHfwMultiRange, $scope.filterOptions.selectedHfwSortOrder.id).finally(function (response) {
                            spinnerService.hide('tableSpinner');
                        });
                    } else {
                        $bootbox.alert('Please select a Student first.');
                    }
                }

                $scope.factory = new hfwReportFactory();
                $scope.filterOptions = nsFilterOptionsService.options;
                $scope.settings = { printMode: false, printInProgress: false, batchPrint: false };

                if ($location.absUrl().indexOf('printmode=') >= 0) {
                    $scope.settings.printMode = true;
                }

                if ($location.absUrl().indexOf('batchprint=') >= 0) {
                    $scope.settings.batchPrint = true;
                }
          
                $scope.$watch('filterOptions.selectedSectionStudent.id', function (newVal, oldVal) {
                    if (newVal !== oldVal) {
                        LoadData();
                    }
                });

                $scope.setHfwBackgroundColor = function (wordRow) {
                    if (wordRow.Read && wordRow.Write) {
                        return 'yellowSelectedHfwRow';  // TODO: filterOptions.hfwBG
                    }
                }

                $scope.$watch('filterOptions.selectedHfwSortOrder', function (newVal, oldVal) {
                    if (newVal !== oldVal) {
                        LoadData();
                    }
                });

                $scope.$watch('filterOptions.selectedHfwMultiRange', function (newVal, oldVal) {
                    if (newVal !== oldVal) {
                        LoadData();
                    }
                });
             
            }])
          .controller('HfwStudentMissingWordsReportController', ['$scope', 'InterventionGroup', '$q', '$http', 'nsPinesService', '$location', '$routeParams', 'hfwReportFactory', 'nsFilterOptionsService','spinnerService',
            function ($scope, InterventionGroup, $q, $http, nsPinesService, $location, $routeParams, hfwReportFactory, nsFilterOptionsService, spinnerService) {

                $scope.settings = { batchPrint: false };
                $scope.factory = new hfwReportFactory();
                $scope.filterOptions = nsFilterOptionsService.options;

                if ($location.absUrl().indexOf('batchprint=') >= 0) {
                    $scope.settings.batchPrint = true;
                }

                $scope.processQuickSearchStudent = function () {
                    if (angular.isDefined($scope.filterOptions.quickSearchStudent)) {
                        spinnerService.show('tableSpinner');
                        $scope.factory.LoadMissingWordsReportData($scope.filterOptions.quickSearchStudent.id, $scope.filterOptions.selectedHfwMultiRange, $scope.filterOptions.selectedHfwSortOrder.id).finally(function (response) {
                            spinnerService.hide('tableSpinner');
                        });
                    } else {
                        $bootbox.alert('Please select a Student first.');
                    }
                }

                $scope.$on('NSInitialLoadComplete', function (event, data) {
                    if ($scope.filterOptions.selectedSectionStudent) {
                        spinnerService.show('tableSpinner');
                        $scope.factory.LoadMissingWordsReportData($scope.filterOptions.selectedSectionStudent.id, $scope.filterOptions.selectedHfwMultiRange, $scope.filterOptions.selectedHfwSortOrder.id).finally(function (response) {
                            spinnerService.hide('tableSpinner');
                        });
                    }
                });

                $scope.$on('NSHfwSortOrderOptionsUpdated', function (event, data) {
                    spinnerService.show('tableSpinner');
                    $scope.factory.LoadMissingWordsReportData($scope.filterOptions.selectedSectionStudent.id, $scope.filterOptions.selectedHfwMultiRange, $scope.filterOptions.selectedHfwSortOrder.id).finally(function (response) {
                        spinnerService.hide('tableSpinner');
                    });
                });

                $scope.$on('NSHfwMultiRangeOptionsUpdated', function (event, data) {
                    spinnerService.show('tableSpinner');
                    $scope.factory.LoadMissingWordsReportData($scope.filterOptions.selectedSectionStudent.id, $scope.filterOptions.selectedHfwMultiRange, $scope.filterOptions.selectedHfwSortOrder.id).finally(function (response) {
                        spinnerService.hide('tableSpinner');
                    });
                });
                // TODO: create watches after initial load
                $scope.$watch('filterOptions.selectedSectionStudent', function (newVal, oldVal) {
                    if ($scope.filterOptions.selectedSectionStudent) {
                        spinnerService.show('tableSpinner');
                        $scope.factory.LoadMissingWordsReportData($scope.filterOptions.selectedSectionStudent.id, $scope.filterOptions.selectedHfwMultiRange, $scope.filterOptions.selectedHfwSortOrder.id).finally(function (response) {
                            spinnerService.hide('tableSpinner');
                        });
                    }
                });
            }])
        .factory('hfwReportFactory', ['$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {

            var hfwReportFactory = function () {
                this.LoadReportDetailData = function (studentId, selectedRanges, sortOrder) {

                    // set default
                    if (selectedRanges == null) {
                        selectedRanges = [{ id: 1, text: "1-100" }];
                    }

                    var url = webApiBaseUrl + '/api/sectionreport/GetHFWDetailReport/';
                    var paramObj = { StudentId: studentId, SelectedRanges: selectedRanges, HfwSortOrder: sortOrder };
                    var promise = $http.post(url, paramObj);
                    var self = this;

                    return promise.then(function (response) {
                        angular.extend(self, response.data);

                    });
                }

                this.LoadMissingWordsReportData = function (studentId, selectedRanges, sortOrder) {

                    // set default
                    if (selectedRanges == null) {
                        selectedRanges = [{ id: 1, text: "1-100" }];
                    }

                    var url = webApiBaseUrl + '/api/sectionreport/GetHFWMissingWordsReport/';
                    var paramObj = { StudentId: studentId, SelectedRanges: selectedRanges, HfwSortOrder: sortOrder };
                    var promise = $http.post(url, paramObj);
                    var self = this;

                    return promise.then(function (response) {
                        angular.extend(self, response.data);

                    });
                }
            }
            return hfwReportFactory;
        }])
    ;

})();