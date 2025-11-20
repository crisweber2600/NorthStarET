(function () {
    'use strict';

    angular
        .module('districtSettingsModule', [])
    .controller('BenchmarkDatesController', ['NSBenchmarkDatesManager', '$scope', 'nsFilterOptionsService', function (NSBenchmarkDatesManager, $scope, nsFilterOptionsService) {
        $scope.mgr = new NSBenchmarkDatesManager();
        $scope.filterOptions = nsFilterOptionsService.options;
        
        $scope.$watch('filterOptions.selectedSchoolYear.id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                LoadData();
            }
        }); 

        var LoadData = function () {
            if ($scope.filterOptions.selectedSchoolYear != null) {
                $scope.mgr.LoadData($scope.filterOptions.selectedSchoolYear.id).then(function (response) {
                    $scope.benchmarkdates = response.data.DueDates;
                });
            }
        }
    }])
    .factory('NSBenchmarkDatesManager', [
        '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
            var NSBenchmarkDatesManager = function () {
                var self = this;

                self.LoadData = function (schoolYear) {
                    var paramObj = { Id: schoolYear };

                    var url = webApiBaseUrl + '/api/districtsettings/GetBenchmarkDatesForSchoolYear';
                    var benchmarksPromise = $http.post(url, paramObj);

                    return benchmarksPromise;
                }
            };

            return (NSBenchmarkDatesManager);
        }
    ])
    
})();