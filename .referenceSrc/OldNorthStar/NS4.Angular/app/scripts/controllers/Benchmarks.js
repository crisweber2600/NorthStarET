(function () {
    'use strict';

    angular
        .module('benchmarksModule', [])
    .controller('SystemBenchmarksController', SystemBenchmarksController)
    .factory('NSBenchmarksManager', [
        '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
            var NSBenchmarksManager = function () {
                var self = this;

                self.LoadData = function (assessmentId, fieldName, lookupFieldName) {
                    var paramObj = { AssessmentId: assessmentId, FieldName: fieldName, LookupFieldName: lookupFieldName };

                    var url = webApiBaseUrl + '/api/benchmark/GetSystemBenchmarks';
                    var benchmarksPromise = $http.post(url, paramObj);

                    return benchmarksPromise.then(function (response) {
                        self.benchmarks = response.data.Benchmarks;
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
                                RangeLow: v.RangeLow
                            })
                        })
                    })
                    return out;
                }

                self.SaveBenchmark = function (benchmarkRecord) {
                    var paramObj = { Benchmark: benchmarkRecord };

                    var url = webApiBaseUrl + '/api/benchmark/SaveSystemBenchmark';
                    var promise = $http.post(url, paramObj);

                    // temporary

                    return promise.then(function (response) {
                        //self.benchmarks = response.data.Benchmarks;
                    });
                }

                self.DeleteBenchmark = function (benchmarkRecord) {
                    var paramObj = { Benchmark: benchmarkRecord };

                    var url = webApiBaseUrl + '/api/benchmark/DeleteSystemBenchmark';
                    var promise = $http.post(url, paramObj);

                    // temporary

                    return promise.then(function (response) {
                        //self.benchmarks = response.data.Benchmarks;
                    });
                }

                //this.changeFieldStatus = function (assessmentId, fieldId, status, hideFieldFrom, successCallback, failureCallback) {
                //    var returnObject = { AssessmentId: assessmentId, FieldId: fieldId, HiddenStatus: status, HideFieldFrom: hideFieldFrom };
                //    var saveResponse = $http.post(webApiBaseUrl + "/api/personalsettings/UpdateFieldForUser", returnObject);
                //    saveResponse.then(successCallback, failureCallback);
                //};

            };

            return (NSBenchmarksManager);
        }
    ])
    .directive('nsBenchmarkField', [
			'Assessment', '$routeParams', '$compile', '$templateCache', '$http', 'nsLookupFieldService', function (Assessment, $routeParams, $compile, $templateCache, $http, nsLookupFieldService) {

			    var getTemplate = function (field, mode) {
			        var type = field.FieldType;
			        var template = '';

			        if (type === '' || type === null) {
			            type = 'textfield';
			        }
			        var templateName = 'templates/benchmark-' + type + '.html';
			        template = $templateCache.get(templateName.toLocaleLowerCase());
			        return template;
			    }

			    return {
			        restrict: 'E',
			        scope: {
			            result: '=',
			            eForm: '=',
			            field: '='
			        },
			        link: function (scope, element, attr) {
			            // get our own lookupFields
			            scope.lookupValues = [];
			            scope.lookupFieldsArray = nsLookupFieldService.LookupFieldsArray;

			            for (var i = 0; i < scope.lookupFieldsArray.length; i++) {
			                if (scope.lookupFieldsArray[i].LookupColumnName === scope.field.LookupFieldName) {
			                    scope.lookupValues = scope.lookupFieldsArray[i].LookupFields;
			                    break;
			                }
			            }

			            var templateText = getTemplate(scope.field, scope.mode);
			            var dataToAppend = templateText;
			            element.html(dataToAppend);
			            $compile(element.contents())(scope);
			        }
			    };
			}
    ]);

    SystemBenchmarksController.$inject = ['$scope', '$routeParams', '$location', 'nsSectionService', 'nsFilterOptionsService', 'nsPinesService', 'nsSelect2RemoteOptions', 'NSBenchmarksManager', '$bootbox'];

    function SystemBenchmarksController($scope, $routeParams, $location, nsSectionService, nsFilterOptionsService, nsPinesService, nsSelect2RemoteOptions, NSBenchmarksManager, $bootbox) {
        $scope.dataMgr = new NSBenchmarksManager();
        //$scope.assessments = $scope.fieldsManager.Assessments;
        $scope.errors = [];
        $scope.$on('NSHTTPError', function (event, data) {
            $scope.errors.push({ type: "danger", msg: data });
            $('html, body').animate({ scrollTop: 0 }, 'fast');
        });

        $scope.settings = {};

        $scope.saveAssessmentData = function (result) {
            $scope.dataMgr.SaveBenchmark(result).then(function (response) {
                nsPinesService.dataSavedSuccessfully();
                //$scope.dataMgr.LoadData(1, 'FPValueId', 'FPScale');
                $scope.dataMgr.LoadData($scope.settings.selectedAssessmentField.AssessmentId, $scope.settings.selectedAssessmentField.FieldName, $scope.settings.selectedAssessmentField.LookupFieldName);
            });
        }

        $scope.deleteAssessmentData = function (result) {

            $bootbox.confirm('Are you sure you want to delete this benchmark record?', function (response) {
                if (response) {
                    $scope.dataMgr.DeleteBenchmark(result).then(function (response) {
                        nsPinesService.dataDeletedSuccessfully();
                        $scope.dataMgr.LoadData($scope.settings.selectedAssessmentField.AssessmentId, $scope.settings.selectedAssessmentField.FieldName, $scope.settings.selectedAssessmentField.LookupFieldName);
                    });
                }
            }
            );
        }

        $scope.$watch('settings.selectedAssessmentField', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                $scope.dataMgr.LoadData($scope.settings.selectedAssessmentField.AssessmentId, $scope.settings.selectedAssessmentField.FieldName, $scope.settings.selectedAssessmentField.LookupFieldName);
            }
        }, true);



        $scope.dataMgr.LoadAssessmentFields();
        //$scope.dataMgr.LoadData(1, 'FPValueId', 'FPScale');
        // initial load
        //$scope.getCoTeacherRemoteOptions = nsSelect2RemoteOptions.CoTeacherRemoteOptions;
        //$scope.getStaffGroupRemoteOptions = nsSelect2RemoteOptions.StaffGroupRemoteOptions;
    }

})();