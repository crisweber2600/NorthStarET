(function () {
    'use strict';

    angular
        .module('districtSettingsModule', [])
        .controller('DistrictBenchmarksController', ['$scope', '$routeParams', '$location', 'nsSectionService', 'nsFilterOptionsService', 'nsPinesService', 'nsSelect2RemoteOptions', 'DistrictBenchmarksManager', '$bootbox', 'progressLoader',
            function ($scope, $routeParams, $location, nsSectionService, nsFilterOptionsService, nsPinesService, nsSelect2RemoteOptions, DistrictBenchmarksManager, $bootbox, progressLoader) {
                $scope.dataMgr = new DistrictBenchmarksManager();
                //$scope.assessments = $scope.fieldsManager.Assessments;
                $scope.errors = [];
                $scope.$on('NSHTTPError', function (event, data) {
                    $scope.errors.push({ type: "danger", msg: data });
                    $('html, body').animate({ scrollTop: 0 }, 'fast');
                });

                $scope.settings = {};
                $scope.notImplemented = function () {
                    $bootbox.alert("Note: This feature is not implemented yet.");
                }

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
                            progressLoader.start();
                            progressLoader.set(50);
                            $scope.dataMgr.DeleteBenchmark(result).then(function (response) {
                                nsPinesService.dataDeletedSuccessfully();
                                $scope.dataMgr.LoadData($scope.settings.selectedAssessmentField.AssessmentId, $scope.settings.selectedAssessmentField.FieldName, $scope.settings.selectedAssessmentField.LookupFieldName).then(function (response) {
                                    progressLoader.end();
                                });
                            });
                        }
                    }
                    );
                }

                $scope.$watch('settings.selectedAssessmentField', function (newValue, oldValue) {
                    if (!angular.equals(newValue, oldValue) && newValue !== null) {
                        progressLoader.start();
                        progressLoader.set(50);
                        $scope.dataMgr.LoadData($scope.settings.selectedAssessmentField.AssessmentId, $scope.settings.selectedAssessmentField.FieldName, $scope.settings.selectedAssessmentField.LookupFieldName).then(function (response) {
                            progressLoader.end();
                        });
                    }
                }, true);



                $scope.dataMgr.LoadAssessmentFields();
            }

        ])
        .controller('StudentAttributeController', ['$scope', '$routeParams', '$location', 'nsSectionService', 'nsFilterOptionsService', 'nsPinesService', 'nsSelect2RemoteOptions', 'DistrictStudentAttributesManager', '$bootbox', 'progressLoader', '$uibModal',
            function ($scope, $routeParams, $location, nsSectionService, nsFilterOptionsService, nsPinesService, nsSelect2RemoteOptions, DistrictStudentAttributesManager, $bootbox, progressLoader, $uibModal) {
                $scope.dataMgr = new DistrictStudentAttributesManager();

                $scope.settings = { newAttribute: {} };

                $scope.showNewAttributeModal = function () {

                    var modalInstance = $uibModal.open({
                        templateUrl: 'newAttribute.html',
                        scope: $scope,
                        controller: function ($scope, $uibModalInstance) {

                            $scope.saveNewAttribute = function (att) {
                                if (!att.AttributeName || att.AttributeName == '') {
                                    alert('The Attribute must have a name.');
                                    return;
                                }
                                $scope.saveAttribute(att);
                                $uibModalInstance.dismiss('cancel');
                            };
                            $scope.cancel = function () {
                                $uibModalInstance.dismiss('cancel');
                            };
                        },
                        size: 'md',
                    });
                }

                $scope.saveAttribute = function (att) {
                    $scope.dataMgr.SaveAttribute(att).then(function (response) {
                        nsPinesService.dataSavedSuccessfully();
                        $scope.dataMgr.LoadStudentAttributes();
                        $scope.settings.newAttribute = {};
                    }, function (err) {
                        $scope.dataMgr.LoadStudentAttributes();
                    });
                }
                $scope.saveAttributeValue = function (att) {
                    $scope.dataMgr.SaveAttributeValue(att).then(function (response) {
                        nsPinesService.dataSavedSuccessfully();
                        $scope.dataMgr.LoadStudentAttributes();
                    }, function (err) {
                        $scope.dataMgr.LoadStudentAttributes();
                    });
                }

                $scope.saveNewAttributeValue = function (attribute) {
                    if (attribute.addAttributeLookupValue == '' || !attribute.addAttributeLookupValue) {
                        $bootbox.alert('New Attribute Value must have a name.');
                        return;
                    }

                    var addAttributeValue = { AttributeId: attribute.Id, LookupValue: attribute.addAttributeLookupValue, Description: attribute.addAttributeDescription };

                    $scope.dataMgr.SaveAttributeValue(addAttributeValue).then(function (response) {
                        nsPinesService.dataSavedSuccessfully();
                        attribute.addAttributeLookupValue = '';
                        attribute.addAttributeDescription = '';
                        $scope.dataMgr.LoadStudentAttributes();
                    }, function (err) {
                        $scope.dataMgr.LoadStudentAttributes();
                    });
                }

                $scope.startEdit = function (rowform) {
                    rowform.$show();
                }
                $scope.before = function (rowform) {
                    rowform.$setSubmitted();

                    if (rowform.$valid) {
                        return;
                    } else return 'At least one required field is not filled out.';
                }

                $scope.deleteAttribute = function (attribute) {

                    $bootbox.confirm('Are you sure you want to delete the Attribute "' + attribute.AttributeName + '"?<br><br><span style="color:red;font-weight:bold">NOTE: This will delete any attribute data associated with students and is IRREVERSIBLE!</span>', function (response) {
                        if (response) {
                            progressLoader.start();
                            progressLoader.set(50);
                            $scope.dataMgr.DeleteAttribute(attribute).then(function (response) {
                                nsPinesService.dataDeletedSuccessfully();
                                $scope.dataMgr.LoadStudentAttributes().then(function (response) {
                                    progressLoader.end();
                                });
                            });
                        }
                    }
                    );
                }
                $scope.deleteAttributeValue = function (attribute) {

                    $bootbox.confirm('Are you sure you want to delete this Attribute Value "' + attribute.LookupValue + '"?<br><br><span style="color:red;font-weight:bold">NOTE: This will delete any attribute value data associated with students and is IRREVERSIBLE!</span>', function (response) {
                        if (response) {
                            progressLoader.start();
                            progressLoader.set(50);
                            $scope.dataMgr.DeleteAttributeValue(attribute).then(function (response) {
                                nsPinesService.dataDeletedSuccessfully();
                                $scope.dataMgr.LoadStudentAttributes().then(function (response) {
                                    progressLoader.end();
                                });
                            });
                        }
                    }
                    );
                }



                $scope.dataMgr.LoadStudentAttributes();
            }

        ])
         .controller('DistrictYearlyAssessmentBenchmarksController', ['$scope', '$routeParams', '$location', 'nsSectionService', 'nsFilterOptionsService', 'nsPinesService', 'nsSelect2RemoteOptions', 'DistrictYearlyAssessmentBenchmarksManager', '$bootbox', 'progressLoader',
            function ($scope, $routeParams, $location, nsSectionService, nsFilterOptionsService, nsPinesService, nsSelect2RemoteOptions, DistrictYearlyAssessmentBenchmarksManager, $bootbox, progressLoader) {
                $scope.dataMgr = new DistrictYearlyAssessmentBenchmarksManager();
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
                        $scope.dataMgr.LoadData($scope.settings.selectedAssessmentField.AssessmentId, $scope.settings.selectedAssessmentField.FieldName, $scope.settings.selectedAssessmentField.LookupFieldName);
                    });
                }

                $scope.deleteAssessmentData = function (result) {

                    $bootbox.confirm('Are you sure you want to delete this benchmark record?', function (response) {
                        if (response) {
                            progressLoader.start();
                            progressLoader.set(50);
                            $scope.dataMgr.DeleteBenchmark(result).then(function (response) {
                                nsPinesService.dataDeletedSuccessfully();
                                $scope.dataMgr.LoadData($scope.settings.selectedAssessmentField.AssessmentId, $scope.settings.selectedAssessmentField.FieldName, $scope.settings.selectedAssessmentField.LookupFieldName).then(function (response) {
                                    progressLoader.end();
                                });
                            });
                        }
                    }
                    );
                }

                $scope.$watch('settings.selectedAssessmentField', function (newValue, oldValue) {
                    if (!angular.equals(newValue, oldValue)) {
                        progressLoader.start();
                        progressLoader.set(50);
                        $scope.dataMgr.LoadData($scope.settings.selectedAssessmentField.AssessmentId, $scope.settings.selectedAssessmentField.FieldName, $scope.settings.selectedAssessmentField.LookupFieldName).then(function (response) {
                            progressLoader.end();
                        });
                    }
                }, true);



                $scope.dataMgr.LoadAssessmentFields();
            }

         ])

         .controller('DistrictAccessController', ['$scope', '$routeParams', '$location', 'nsSectionService', 'nsFilterOptionsService', 'nsPinesService', '$bootbox', 'progressLoader','$uibModal', '$http','webApiBaseUrl','$timeout',
            function ($scope, $routeParams, $location, nsSectionService, nsFilterOptionsService, nsPinesService, $bootbox, progressLoader, $uibModal, $http, webApiBaseUrl, $timeout) {

                var GetDistricts = function () {
                    $http.get(webApiBaseUrl + "/api/Assessment/GetDistrictList").then(function (response) {
                        $scope.allDistricts = response.data.Districts;
                    });
                }

                var reload = function () {
                    location.reload(true);
                }

                $scope.logInToDistrict = function (district) {
                    $http.post(webApiBaseUrl + '/api/DistrictSettings/LogIn', { Id: district.Id }).then(function (response) {
                        nsPinesService.buildMessage('Log In Successful', 'You are now logged into <b>' + district.Name + '</b>', 'success');
                        $timeout(reload, 1000);
                    })
                }

                

                GetDistricts();
            }
         ])
    .factory('DistrictBenchmarksManager', [
        '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
            var NSBenchmarksManager = function () {
                var self = this;

                self.LoadData = function (assessmentId, fieldName, lookupFieldName) {
                    var paramObj = { AssessmentId: assessmentId, FieldName: fieldName, LookupFieldName: lookupFieldName };

                    var url = webApiBaseUrl + '/api/benchmark/GetDistrictBenchmarks';
                    var benchmarksPromise = $http.post(url, paramObj);

                    return benchmarksPromise.then(function (response) {
                        self.benchmarks = response.data.Benchmarks;
                    });
                }

                self.LoadAssessmentFields = function () {

                    var url = webApiBaseUrl + '/api/benchmark/GetDistrictAssessmentsAndFields';
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

                    var url = webApiBaseUrl + '/api/benchmark/SaveDistrictBenchmark';
                    var promise = $http.post(url, paramObj);

                    // temporary

                    return promise.then(function (response) {
                        //self.benchmarks = response.data.Benchmarks;
                    });
                }

                self.DeleteBenchmark = function (benchmarkRecord) {
                    var paramObj = { Benchmark: benchmarkRecord };

                    var url = webApiBaseUrl + '/api/benchmark/DeleteDistrictBenchmark';
                    var promise = $http.post(url, paramObj);

                    // temporary

                    return promise.then(function (response) {
                        //self.benchmarks = response.data.Benchmarks;
                    });
                }
            };

            return (NSBenchmarksManager);
        }
    ])
         .factory('DistrictStudentAttributesManager', [
        '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
            var DistrictStudentAttributesManager = function () {
                var self = this;

                self.LoadStudentAttributes = function () {
                    var url = webApiBaseUrl + '/api/districtsettings/GetStudentAttributes';
                    var promise = $http.get(url);

                    return promise.then(function (response) {
                        self.attributes = response.data.Attributes;
                    });
                }


                self.SaveAttribute = function (attribute) {
                    var paramObj = { Attribute: attribute };

                    var url = webApiBaseUrl + '/api/districtsettings/SaveAttribute';
                    var promise = $http.post(url, paramObj);

                    return promise;
                };

                self.SaveAttributeValue = function (val) {
                    var paramObj = { AttributeValue: val };

                    var url = webApiBaseUrl + '/api/districtsettings/SaveAttributeValue';
                    var promise = $http.post(url, paramObj);

                    return promise;
                }

                self.DeleteAttribute = function (attribute) {
                    var paramObj = { Attribute: attribute };

                    var url = webApiBaseUrl + '/api/districtsettings/DeleteAttribute';
                    var promise = $http.post(url, paramObj);

                    return promise;
                }

                self.DeleteAttributeValue = function (val) {
                    var paramObj = { AttributeValue: val };

                    var url = webApiBaseUrl + '/api/districtsettings/DeleteAttributeValue';
                    var promise = $http.post(url, paramObj);

                    return promise;
                }
            };

            return (DistrictStudentAttributesManager);
        }
         ])
         .factory('DistrictYearlyAssessmentBenchmarksManager', [
        '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
            var NSYearlyAssessmentBenchmarksManager = function () {
                var self = this;

                self.LoadData = function (assessmentId, fieldName, lookupFieldName) {
                    var paramObj = { AssessmentId: assessmentId, FieldName: fieldName, LookupFieldName: lookupFieldName };

                    var url = webApiBaseUrl + '/api/benchmark/GetDistrictYearlyAssessmentBenchmarks';
                    var benchmarksPromise = $http.post(url, paramObj);

                    return benchmarksPromise.then(function (response) {
                        self.benchmarks = response.data.Benchmarks;
                    });
                }

                self.LoadAssessmentFields = function () {

                    var url = webApiBaseUrl + '/api/benchmark/GetDistrictYearlyAssessmentsAndFields';
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

                    var url = webApiBaseUrl + '/api/benchmark/SaveDistrictYearlyAssessmentBenchmark';
                    var promise = $http.post(url, paramObj);

                    // temporary

                    return promise.then(function (response) {
                        //self.benchmarks = response.data.Benchmarks;
                    });
                }

                self.DeleteBenchmark = function (benchmarkRecord) {
                    var paramObj = { Benchmark: benchmarkRecord };

                    var url = webApiBaseUrl + '/api/benchmark/DeleteDistrictYearlyAssessmentBenchmark';
                    var promise = $http.post(url, paramObj);

                    // temporary

                    return promise.then(function (response) {
                        //self.benchmarks = response.data.Benchmarks;
                    });
                }
            };

            return (NSYearlyAssessmentBenchmarksManager);
        }
         ])
      
            .factory('NSHFWListManager', [
        '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
            var NSHFWListManager = function () {
                var self = this;

                self.LoadData = function (wordList, orderObj) {
                    var isAlphaOrder = true;

                    if (orderObj && orderObj.id == 'alphabetic') {
                        isAlphaOrder = true;
                    } else {
                        isAlphaOrder = false;
                    }

                    var paramObj = { WordList: wordList, IsAlphaOrder: isAlphaOrder };

                    var url = webApiBaseUrl + '/api/districtsettings/GetHFWList';
                    var promise = $http.post(url, paramObj);
                     
                    return promise;
                }

                self.saveHfw = function (word) {
                    var paramObj = { Word: word };

                    var url = webApiBaseUrl + '/api/districtsettings/SaveHFW';
                    var promise = $http.post(url, paramObj);
                    return promise;
                }

            };

            return (NSHFWListManager);
        }
            ])
        .factory('NSInterventionListManager', [
        '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
            var NSInterventionListManager = function () {
                var self = this;

                self.LoadData = function () {
                    var url = webApiBaseUrl + '/api/districtsettings/GetInterventionList';
                    var promise = $http.get(url);

                    return promise;
                }

                self.saveIntervention = function (intervention) {
                    var paramObj = { Intervention: intervention };

                    var url = webApiBaseUrl + '/api/districtsettings/SaveIntervention';
                    var promise = $http.post(url, paramObj);
                    return promise;
                }

                self.deleteIntervention = function (intervention) {
                    var paramObj = { Intervention: intervention };

                    var url = webApiBaseUrl + '/api/districtsettings/DeleteIntervention';
                    var promise = $http.post(url, paramObj);
                    return promise;
                }

            };

            return (NSInterventionListManager);
        }
                    ])
                   .controller('ManageInterventionTypesController', ['NSInterventionListManager', '$scope', 'nsFilterOptionsService', 'progressLoader', 'nsPinesService', '$rootScope', '$bootbox','$uibModal',
        function (NSInterventionListManager, $scope, nsFilterOptionsService, progressLoader, nsPinesService, $rootScope, $bootbox, $uibModal) {
            $scope.mgr = new NSInterventionListManager();
            $scope.settings = { newIntervention: {} };
            
            $scope.startEdit = function (rowform) {
                rowform.$show();
            }

            $scope.before = function (rowform) {
                rowform.$setSubmitted();

                if (rowform.$valid) {
                    return;
                } else return 'At least one required field is not filled out.';
            }

            $scope.deleteIntervention = function (intervention) {
                $bootbox.confirm('Are you sure you want to delete this Intervention?  <br><br><b>NOTE:</b> You will not be able to delete it if it is in use.', function (response) {
                    if (response) {
                        progressLoader.start();
                        progressLoader.set(50);
                        $scope.mgr.deleteIntervention(intervention).then(function (response) {
                            progressLoader.end();
                            nsPinesService.dataDeletedSuccessfully();
                            $rootScope.$broadcast("NSHTTPClear", 'Saved Successfully');
                            LoadData();
                        }, function (error) {
                            nsPinesService.generalLoadingError();
                        });
                    }
                });
            }

            $scope.saveIntervention = function (intervention) {

                progressLoader.start();
                progressLoader.set(50);
                $scope.mgr.saveIntervention(intervention).then(function (response) {
                    $scope.settings.newIntervention = {};
                    progressLoader.end();
                    nsPinesService.dataSavedSuccessfully();
                    $rootScope.$broadcast("NSHTTPClear", 'Saved Successfully');
                    LoadData();
                }, function (error) {
                    nsPinesService.dataError();
                    LoadData();
                });
            }

            var LoadData = function () {
                progressLoader.start();
                progressLoader.set(50);
                    $scope.mgr.LoadData().then(function (response) {
                        progressLoader.end();
                        $scope.interventions = response.data.Interventions;
                    });
            }

            $scope.showNewInterventionModal = function () {

                var modalInstance = $uibModal.open({
                    templateUrl: 'newIntervention.html',
                    scope: $scope,
                    controller: function ($scope, $uibModalInstance) {

                        $scope.saveNewIntervention = function (intervention) {
                            if(!intervention.InterventionType || intervention.InterventionType == ''){
                                alert('The Intervention must have a name.');
                                return;
                            }
                            $scope.saveIntervention(intervention);
                            $uibModalInstance.dismiss('cancel');
                        };
                        $scope.cancel = function () {
                            $uibModalInstance.dismiss('cancel');
                        };
                    },
                    size: 'md',
                });
            }

            LoadData();
        }])

           .controller('HFWListController', ['NSHFWListManager', '$scope', 'nsFilterOptionsService', 'progressLoader', 'nsPinesService', '$rootScope', '$bootbox',
        function (NSHFWListManager, $scope, nsFilterOptionsService, progressLoader, nsPinesService, $rootScope, $bootbox) {
            $scope.mgr = new NSHFWListManager();
            $scope.filterOptions = nsFilterOptionsService.options;

            $scope.startEdit = function (rowform, word) {
                word.DisplayNameTemp = word.DisplayName;
                word.IsKdgTemp = word.IsKdg;
                word.SortOrderTemp = word.SortOrder;
                word.AltOrderTemp = word.AltOrder;

                rowform.$show();
            }

            $scope.before = function (rowform) {
                rowform.$setSubmitted();

                if (rowform.$valid) {
                    return;
                } else return 'At least one required field is not filled out.';
            }

            $scope.saveWord = function (word) {
                word.DisplayName = word.DisplayNameTemp;
                word.IsKdg = word.IsKdgTemp;
                word.SortOrder = word.SortOrderTemp;
                word.AltOrder = word.AltOrderTemp;

                progressLoader.start();
                progressLoader.set(50);
                $scope.mgr.saveHfw(word).then(function (response) {
                    progressLoader.end();
                    nsPinesService.dataSavedSuccessfully();
                    $rootScope.$broadcast("NSHTTPClear", 'Saved Successfully');
                    LoadData();
                }, function (error) {
                    nsPinesService.dataError();
                    LoadData();
                });
            }

            $scope.$watch('filterOptions.selectedHfwRange', function (newValue, oldValue) {
                if (newValue != null && newValue != '') {
                    if (!angular.equals(newValue, oldValue)) {
                        LoadData();
                    }
                } else {
                    $scope.words = [];
                }
            });

            $scope.$watch('filterOptions.selectedHfwSortOrder', function (newValue, oldValue) {
                if (!angular.equals(newValue, oldValue)) {
                    LoadData();
                }
            });

            var LoadData = function () {
                progressLoader.start();
                progressLoader.set(50);
                if ($scope.filterOptions.selectedHfwRange != null) {
                    $scope.mgr.LoadData($scope.filterOptions.selectedHfwRange, $scope.filterOptions.selectedHfwSortOrder).then(function (response) {
                        progressLoader.end();
                        $scope.words = response.data.Words;
                    });
                }
            }
        }])
    .controller('BenchmarkDatesController', ['NSBenchmarkDatesManager', '$scope', 'nsFilterOptionsService', 'progressLoader', 'nsPinesService', '$rootScope', '$bootbox',
        function (NSBenchmarkDatesManager, $scope, nsFilterOptionsService, progressLoader, nsPinesService, $rootScope, $bootbox) {
        $scope.mgr = new NSBenchmarkDatesManager();
        $scope.filterOptions = nsFilterOptionsService.options;
        $scope.benchmarkdates = [];

        $scope.settings = {
            newItem: null
        };

        $scope.addNew = function () {
            $scope.settings.newItem = {
                TestLevelPeriodIDTemp: null,
                DueDateTemp: moment().toDate(),
                StartDateTemp: moment().toDate(),
                SchoolStartYear: $scope.filterOptions.selectedSchoolYear.id
            };
        }
        
        $scope.startEdit = function (rowform, period) {
           period.TestLevelPeriodIDTemp = period.TestLevelPeriodID;
           period.StartDateTemp = moment(period.StartDate).toDate();
           period.DueDateTemp = moment(period.DueDate).toDate();
           period.HexTemp = period.Hex;
           period.NotesTemp = period.Notes;
           period.IsSupplementalTemp = period.IsSupplemental;

           rowform.$show();
        }

        $scope.before = function (rowform) {
            rowform.$setSubmitted();

            if (rowform.$valid) {
                return;
            } else return 'At least one required field is not filled out.';
        }

        $scope.deleteBenchmark = function (benchmark) {
            $bootbox.confirm('Are you sure you want to delete this benchmark date?  You will not be able to delete it if it is in use.', function (response) {
                if (response) {
                    progressLoader.start();
                    progressLoader.set(50);
                    $scope.mgr.deleteBenchmark(benchmark).then(function (response) {
                        progressLoader.end();
                        nsPinesService.dataDeletedSuccessfully();
                        $rootScope.$broadcast("NSHTTPClear", 'Saved Successfully');
                        LoadData();
                    }, function (error) {
                        nsPinesService.generalLoadingError();
                    });
                }
            });
        }

        $scope.saveBenchmarkPeriod = function (period) {
            period.TestLevelPeriodID = period.TestLevelPeriodIDTemp;
            period.StartDate = moment(period.StartDateTemp).toDate();
            period.DueDate = moment(period.DueDateTemp).toDate();
            period.Hex = period.HexTemp;
            period.Notes = period.NotesTemp;
            period.IsSupplemental = period.IsSupplementalTemp;

            progressLoader.start();
            progressLoader.set(50);
            $scope.mgr.saveBenchmarkPeriod(period).then(function (response) {
                progressLoader.end();
                nsPinesService.dataSavedSuccessfully();
                $scope.settings.newItem = null;
                $rootScope.$broadcast("NSHTTPClear", 'Saved Successfully');
                LoadData();
            }, function (error) {
                nsPinesService.dataError();
                LoadData();
            });
        }

        $scope.$watch('filterOptions.selectedSchoolYear.id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                LoadData();
            }
        }); 

        var LoadData = function () {
            if ($scope.filterOptions.selectedSchoolYear != null) {
                $scope.mgr.LoadData($scope.filterOptions.selectedSchoolYear.id).then(function (response) {
                    $scope.benchmarkdates = response.data.DueDates;

                    for (var i = 0; i < $scope.benchmarkdates.length; i++) {
                        var cb = $scope.benchmarkdates[i];

                        var dueDate = moment(cb.DueDate.substring(0, cb.DueDate.indexOf('T')));
                        var startDate = moment(cb.StartDate.substring(0, cb.StartDate.indexOf('T')));

                        $scope.benchmarkdates[i].TestLevelPeriodID = $scope.benchmarkdates[i].TestLevelPeriodID + '';
                        $scope.benchmarkdates[i].StartDate = startDate.toDate();
                        $scope.benchmarkdates[i].DueDate = dueDate.toDate();
                    }
                });
            }
        }

        LoadData();
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

                self.saveBenchmarkPeriod = function (period) {
                    var paramObj = { Tdd: period };

                    var url = webApiBaseUrl + '/api/districtsettings/SaveTestDueDate';
                    var promise = $http.post(url, paramObj);
                    return promise;
                }

                self.deleteBenchmark = function (period) {
                    var paramObj = { Tdd: period };

                    var url = webApiBaseUrl + '/api/districtsettings/DeleteBenchmarkDate';
                    var promise = $http.post(url, paramObj);
                    return promise;
                }
            };

            return (NSBenchmarkDatesManager);
        }
    ])
    
})();