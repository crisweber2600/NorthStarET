(function () {
    'use strict';

    angular
        .module('stateTestDataModule', [])
        //.controller('BenchmarkDataImportController', BenchmarkDataImportController)
        //.controller('MNStateTestDataImportFinalController', MNStateTestDataImportFinalController)
        //.controller('MNStateTestDataImportController', MNStateTestDataImportController)
        .factory('StateTestDataImportManager', [
        '$http', 'webApiBaseUrl', 'FileSaver', function ($http, webApiBaseUrl, FileSaver) {
            var StateTestDataImportManager = function () {
                var self = this;

                self.LoadAssessmentTemplate = function (id) {
                    var url = webApiBaseUrl + '/api/importstatetestdata/GetStateTestDataImportTemplate';
                    var paramObj = { id: id };
                    var promise = $http.post(url, paramObj);

                    return promise.then(function (response) {
                        self.Fields = response.data.Fields;
                    });
                };

                self.LoadImportHistory = function () {
                    var url = webApiBaseUrl + '/api/importstatetestdata/LoadStateTestImportHistory';
                    var promise = $http.get(url);

                    return promise.then(function (response) {
                        self.HistoryItems = response.data.HistoryItems;
                    });
                };
                
                self.deleteHistoryItem = function (item) {
                    var url = webApiBaseUrl + '/api/importstatetestdata/DeleteHistoryItem';
                    var paramObj = { id: item.Id };
                    var promise = $http.post(url, paramObj);

                    return promise;
                }

                self.downloadImportFile = function (item) {
                    var url = webApiBaseUrl + '/api/importstatetestdata/GetImportFile';
                    var paramObj = { value: item.UploadedFileName };
                    var promise = $http.post(url, paramObj, {
                        responseType: 'arraybuffer'
                        }).then(function (response) {
                            var data = new Blob([response.data]);
                            FileSaver.saveAs(data, "originalimportfile.csv");
                        });
                }

                self.downloadImportLog = function (item) {
                    var paramObj = { id: item.Id };

                    var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/GetHistoryLog', paramObj);
                    promise.then(function (response) {
                        var data = new Blob([response.data.Result], { type: 'text/csv;charset=ANSI' });
                        FileSaver.saveAs(data, 'importlog.txt');
                    });
                }
            };

            return (StateTestDataImportManager);
        }
        ])
        .controller('StateTestDataImportController', ['$scope', '$http', 'webApiBaseUrl', 'progressLoader', 'StateTestDataImportManager', 'nsFilterOptionsService', 'FileSaver', '$timeout', 'nsPinesService', '$interval','$bootbox',
            function ($scope, $http, webApiBaseUrl, progressLoader, StateTestDataImportManager, nsFilterOptionsService, FileSaver, $timeout, nsPinesService, $interval, $bootbox) {
            $scope.settings = { uploadComplete: false, hasFiles: false };
            $scope.dataMgr = new StateTestDataImportManager();
            $scope.filterOptions = nsFilterOptionsService.options;
            $scope.theFiles = [];

            // watch assessmentfield chooser and reload template when it changes

            // load template
            $scope.$watch('filterOptions.selectedStateTest', function (newValue, oldValue) {
                if (!angular.equals(newValue, oldValue) && newValue != null) {
                    progressLoader.start();
                    progressLoader.set(50);
                    $scope.dataMgr.LoadAssessmentTemplate($scope.filterOptions.selectedStateTest.id).then(function (response) {
                        progressLoader.end();
                    });
                }
            }, true);
                 
            $scope.downloadImportFile = function (item) {
                $scope.dataMgr.downloadImportFile(item);
            }

            $scope.deleteHistoryItem = function (item) {
                $bootbox.confirm('Are you sure you want to delete this job?  <b>Note:</b> If it has not yet been processed, it will be cancelled.',
                    function (response) {
                        if (response) {
                            progressLoader.start();
                            progressLoader.set(50);
                            $scope.dataMgr.deleteHistoryItem(item).then(function (response) {
                                nsPinesService.dataDeletedSuccessfully();
                                reloadHistoryTable();
                                progressLoader.end();
                            });
                        }
                    });
            }

            $scope.downloadImportLog = function (item) {
                $scope.dataMgr.downloadImportLog(item);
            }

            $scope.getStatusClass = function (item) {
                if (item.Status == 'Awaiting processing') {
                    return 'badge-default';
                } else if (item.Status == 'Complete') {
                    return 'badge-success';
                } else if (item.Status == 'Processing') {
                    return 'badge-primary';
                } else if (item.Status == 'Error') {
                    return 'badge-danger';
                } else {
                    return 'badge-warning';
                }
            }
            var reloadHistoryTable = function () {
                $scope.dataMgr.LoadImportHistory();
            };
            reloadHistoryTable();

                // reload table every 5 seconds
            var reloadInterval = $interval(reloadHistoryTable, 5000);
                // here is where the cleanup happens
            $scope.$on('$destroy', function () {
                $interval.cancel(reloadInterval);
            });


            $scope.getTemplate = function () {
                var paramObj = { id: $scope.filterOptions.selectedStateTest.id };

                var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/getstatetestexporttemplate', paramObj);
                promise.then(function (response) {
                    var data = new Blob([response.data.Result], { type: 'text/csv;charset=ANSI' });
                    FileSaver.saveAs(data, 'template.csv');
                });
            };
            
            $scope.upload = function (theFiles) {
                var formData = new FormData();
                formData.append("AssessmentId", $scope.filterOptions.selectedStateTest.id);
                formData.append("SchoolYear", $scope.filterOptions.selectedSchoolYear.id);

                angular.forEach(theFiles, function (file) {
                    formData.append(file.name, file);
                });
                var paramObj = {};
                // start loader
                progressLoader.start();
                progressLoader.set(50);
                var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/uploadstatetestcsv', formData, {
                    transformRequest: angular.identity,
                    headers: { 'Content-Type': undefined }
                }).then(function (response) {
                    // end loader
                    progressLoader.end();
                    //$scope.errors = [];
                    $scope.settings.LogItems = response.data.LogItems;
                    // show success
                    $timeout(function () {
                        $('#formReset').click();
                    }, 100);
                    //$scope.theFiles.length = 0;
                    //$scope.settings.hasFiles = false;
                    $scope.settings.uploadComplete = true;

                    if ($scope.settings.LogItems.length > 0) {
                        nsPinesService.buildMessage('Data Import Error', 'There were one or more errors in your import file.', 'error');
                    } else {
                        nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
                    }
                }, function(err) {
                    progressLoader.end();
                    $scope.settings.uploadComplete = true;
                });
            };
            }])
        .factory('BenchmarkDataImportManager', [
        '$http', 'webApiBaseUrl', 'FileSaver', function ($http, webApiBaseUrl, FileSaver) {
            var BenchmarkDataImportManager = function () {
                var self = this;

                self.LoadAssessmentTemplate = function (id) {
                    var url = webApiBaseUrl + '/api/importstatetestdata/GetBenchmarkTestDataImportTemplate';
                    var paramObj = { id: id };
                    var promise = $http.post(url, paramObj);

                    return promise.then(function (response) {
                        self.Fields = response.data.Fields;
                    });
                };

                self.LoadImportHistory = function () {
                    var url = webApiBaseUrl + '/api/importstatetestdata/LoadBenchmarkTestImportHistory';
                    var promise = $http.get(url);

                    return promise.then(function (response) {
                        self.HistoryItems = response.data.HistoryItems;
                    });
                };

                self.deleteHistoryItem = function (item) {
                    var url = webApiBaseUrl + '/api/importstatetestdata/DeleteBMHistoryItem';
                    var paramObj = { id: item.Id };
                    var promise = $http.post(url, paramObj);

                    return promise;
                }

                self.downloadImportFile = function (item) {
                    var url = webApiBaseUrl + '/api/importstatetestdata/GetImportFile';
                    var paramObj = { value: item.UploadedFileName };
                    var promise = $http.post(url, paramObj, {
                        responseType: 'arraybuffer'
                    }).then(function (response) {
                        var data = new Blob([response.data]);
                        FileSaver.saveAs(data, "originalimportfile.csv");
                    });
                }

                self.downloadImportLog = function (item) {
                    var paramObj = { id: item.Id };

                    var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/GetBMHistoryLog', paramObj);
                    promise.then(function (response) {
                        var data = new Blob([response.data.Result], { type: 'text/csv;charset=ANSI' });
                        FileSaver.saveAs(data, 'importlog.txt');
                    });
                }
            };

            return (BenchmarkDataImportManager);
        }
        ])
        .controller('BenchmarkDataImportController', ['$scope', '$http', 'webApiBaseUrl', 'progressLoader', 'BenchmarkDataImportManager', 'nsFilterOptionsService', 'FileSaver', '$timeout', 'nsPinesService', '$interval', '$bootbox',
            function ($scope, $http, webApiBaseUrl, progressLoader, BenchmarkDataImportManager, nsFilterOptionsService, FileSaver, $timeout, nsPinesService, $interval, $bootbox) {
                $scope.settings = { uploadComplete: false, hasFiles: false };
                $scope.dataMgr = new BenchmarkDataImportManager();
                $scope.filterOptions = nsFilterOptionsService.options;
                $scope.theFiles = [];

                // watch assessmentfield chooser and reload template when it changes

                // load template
                $scope.$watch('filterOptions.selectedBenchmarkTest', function (newValue, oldValue) {
                    if (!angular.equals(newValue, oldValue) && newValue != null) {
                        progressLoader.start();
                        progressLoader.set(50);
                        $scope.dataMgr.LoadAssessmentTemplate($scope.filterOptions.selectedBenchmarkTest.id).then(function (response) {
                            progressLoader.end();
                        });
                    }
                }, true);

                $scope.downloadImportFile = function (item) {
                    $scope.dataMgr.downloadImportFile(item);
                }

                $scope.deleteHistoryItem = function (item) {
                    $bootbox.confirm('Are you sure you want to delete this job?  <b>Note:</b> If it has not yet been processed, it will be cancelled.',
                        function (response) {
                            if (response) {
                                progressLoader.start();
                                progressLoader.set(50);
                                $scope.dataMgr.deleteHistoryItem(item).then(function (response) {
                                    nsPinesService.dataDeletedSuccessfully();
                                    reloadHistoryTable();
                                    progressLoader.end();
                                });
                            }
                        });
                }

                $scope.downloadImportLog = function (item) {
                    $scope.dataMgr.downloadImportLog(item);
                }

                $scope.getStatusClass = function (item) {
                    if (item.Status == 'Awaiting processing') {
                        return 'badge-default';
                    } else if (item.Status == 'Complete') {
                        return 'badge-success';
                    } else if (item.Status == 'Processing') {
                        return 'badge-primary';
                    } else if (item.Status == 'Error') {
                        return 'badge-danger';
                    } else {
                        return 'badge-warning';
                    }
                }
                var reloadHistoryTable = function () {
                    $scope.dataMgr.LoadImportHistory();
                };
                reloadHistoryTable();
                                
                // reload table every 5 seconds
                var reloadInterval = $interval(reloadHistoryTable, 5000);
                // here is where the cleanup happens
                $scope.$on('$destroy', function () {
                    $interval.cancel(reloadInterval);
                });




                $scope.getTemplate = function () {
                    var paramObj = { id: $scope.filterOptions.selectedBenchmarkTest.id };

                    var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/getbenchmarktestexporttemplate', paramObj);
                    promise.then(function (response) {
                        var data = new Blob([response.data.Result], { type: 'text/csv;charset=ANSI' });
                        FileSaver.saveAs(data, 'template.csv');
                    });
                };

                $scope.upload = function (theFiles) {
                    var formData = new FormData();
                    formData.append("AssessmentId", $scope.filterOptions.selectedBenchmarkTest.id);
                    formData.append("SchoolYear", $scope.filterOptions.selectedSchoolYear.id);
                    formData.append("BenchmarkDateId", $scope.filterOptions.selectedBenchmarkDate.id);
                    formData.append("RecorderId", $scope.filterOptions.quickSearchStaff.id);

                    angular.forEach(theFiles, function (file) {
                        formData.append(file.name, file);
                    });
                    var paramObj = {};
                    // start loader
                    progressLoader.start();
                    progressLoader.set(50);
                    var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/uploadbenchmarktestcsv', formData, {
                        transformRequest: angular.identity,
                        headers: { 'Content-Type': undefined }
                    }).then(function (response) {
                        // end loader
                        progressLoader.end();
                        //$scope.errors = [];
                        $scope.settings.LogItems = response.data.LogItems;
                        // show success
                        $timeout(function () {
                            $('#formReset').click();
                        }, 100);
                        //$scope.theFiles.length = 0;
                        //$scope.settings.hasFiles = false;
                        $scope.settings.uploadComplete = true;

                        if ($scope.settings.LogItems.length > 0) {
                            nsPinesService.buildMessage('Data Import Error', 'There were one or more errors in your import file.', 'error');
                        } else {
                            nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
                        }
                    }, function (err) {
                        progressLoader.end();
                        $scope.settings.uploadComplete = true;
                    });
                };
            }])
        .factory('InterventionDataImportManager', [
        '$http', 'webApiBaseUrl', 'FileSaver', function ($http, webApiBaseUrl, FileSaver) {
            var InterventionDataImportManager = function () {
                var self = this;

                self.LoadAssessmentTemplate = function (id) {
                    var url = webApiBaseUrl + '/api/importstatetestdata/GetInterventionTestDataImportTemplate';
                    var paramObj = { id: id };
                    var promise = $http.post(url, paramObj);

                    return promise.then(function (response) {
                        self.Fields = response.data.Fields;
                    });
                };

                self.LoadImportHistory = function () {
                    var url = webApiBaseUrl + '/api/importstatetestdata/LoadInterventionTestImportHistory';
                    var promise = $http.get(url);

                    return promise.then(function (response) {
                        self.HistoryItems = response.data.HistoryItems;
                    });
                };

                self.deleteHistoryItem = function (item) {
                    var url = webApiBaseUrl + '/api/importstatetestdata/DeleteIntvHistoryItem';
                    var paramObj = { id: item.Id };
                    var promise = $http.post(url, paramObj);

                    return promise;
                }

                self.downloadImportFile = function (item) {
                    var url = webApiBaseUrl + '/api/importstatetestdata/GetImportFile';
                    var paramObj = { value: item.UploadedFileName };
                    var promise = $http.post(url, paramObj, {
                        responseType: 'arraybuffer'
                    }).then(function (response) {
                        var data = new Blob([response.data]);
                        FileSaver.saveAs(data, "originalimportfile.csv");
                    });
                }

                self.downloadImportLog = function (item) {
                    var paramObj = { id: item.Id };

                    var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/GetIntvHistoryLog', paramObj);
                    promise.then(function (response) {
                        var data = new Blob([response.data.Result], { type: 'text/csv;charset=ANSI' });
                        FileSaver.saveAs(data, 'importlog.txt');
                    });
                }
            };

            return (InterventionDataImportManager);
        }
        ])
        .controller('InterventionDataImportController', ['$scope', '$http', 'webApiBaseUrl', 'progressLoader', 'InterventionDataImportManager', 'nsFilterOptionsService', 'FileSaver', '$timeout', 'nsPinesService', '$interval', '$bootbox',
            function ($scope, $http, webApiBaseUrl, progressLoader, InterventionDataImportManager, nsFilterOptionsService, FileSaver, $timeout, nsPinesService, $interval, $bootbox) {
                $scope.settings = { uploadComplete: false, hasFiles: false };
                $scope.dataMgr = new InterventionDataImportManager();
                $scope.filterOptions = nsFilterOptionsService.options;
                $scope.theFiles = [];

                // watch assessmentfield chooser and reload template when it changes

                // load template
                $scope.$watch('filterOptions.selectedInterventionTest', function (newValue, oldValue) {
                    if (!angular.equals(newValue, oldValue) && newValue != null) {
                        progressLoader.start();
                        progressLoader.set(50);
                        $scope.dataMgr.LoadAssessmentTemplate($scope.filterOptions.selectedInterventionTest.id).then(function (response) {
                            progressLoader.end();
                        });
                    }
                }, true);

                $scope.downloadImportFile = function (item) {
                    $scope.dataMgr.downloadImportFile(item);
                }

                $scope.deleteHistoryItem = function (item) {
                    $bootbox.confirm('Are you sure you want to delete this job?  <b>Note:</b> If it has not yet been processed, it will be cancelled.',
                        function (response) {
                            if (response) {
                                progressLoader.start();
                                progressLoader.set(50);
                                $scope.dataMgr.deleteHistoryItem(item).then(function (response) {
                                    nsPinesService.dataDeletedSuccessfully();
                                    reloadHistoryTable();
                                    progressLoader.end();
                                });
                            }
                        });
                }

                $scope.downloadImportLog = function (item) {
                    $scope.dataMgr.downloadImportLog(item);
                }

                $scope.getStatusClass = function (item) {
                    if (item.Status == 'Awaiting processing') {
                        return 'badge-default';
                    } else if (item.Status == 'Complete') {
                        return 'badge-success';
                    } else if (item.Status == 'Processing') {
                        return 'badge-primary';
                    } else if (item.Status == 'Error') {
                        return 'badge-danger';
                    } else {
                        return 'badge-warning';
                    }
                }
                var reloadHistoryTable = function () {
                    $scope.dataMgr.LoadImportHistory();
                };
                reloadHistoryTable();

                // reload table every 5 seconds
                var reloadInterval = $interval(reloadHistoryTable, 5000);
                // here is where the cleanup happens
                $scope.$on('$destroy', function () {
                    $interval.cancel(reloadInterval);
                });


                $scope.getTemplate = function () {
                    var paramObj = { id: $scope.filterOptions.selectedInterventionTest.id };

                    var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/getbenchmarktestexporttemplate', paramObj);
                    promise.then(function (response) {
                        var data = new Blob([response.data.Result], { type: 'text/csv;charset=ANSI' });
                        FileSaver.saveAs(data, 'template.csv');
                    });
                };

                $scope.upload = function (theFiles) {
                    var formData = new FormData();
                    formData.append("SchoolYear", $scope.filterOptions.selectedSchoolYear.id);
                    formData.append("AssessmentId", $scope.filterOptions.selectedInterventionTest.id);
                    formData.append("InterventionGroupId", $scope.filterOptions.selectedInterventionGroup.id);
                    formData.append("RecorderId", $scope.filterOptions.quickSearchStaff.id);

                    angular.forEach(theFiles, function (file) {
                        formData.append(file.name, file);
                    });
                    var paramObj = {};
                    // start loader
                    progressLoader.start();
                    progressLoader.set(50);
                    var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/uploadinterventiontestcsv', formData, {
                        transformRequest: angular.identity,
                        headers: { 'Content-Type': undefined }
                    }).then(function (response) {
                        // end loader
                        progressLoader.end();
                        //$scope.errors = [];
                        $scope.settings.LogItems = response.data.LogItems;
                        // show success
                        $timeout(function () {
                            $('#formReset').click();
                        }, 100);
                        //$scope.theFiles.length = 0;
                        //$scope.settings.hasFiles = false;
                        $scope.settings.uploadComplete = true;

                        if ($scope.settings.LogItems.length > 0) {
                            nsPinesService.buildMessage('Data Import Error', 'There were one or more errors in your import file.', 'error');
                        } else {
                            nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
                        }
                    }, function (err) {
                        progressLoader.end();
                        $scope.settings.uploadComplete = true;
                    });
                };
            }])
        .controller('UpdateCalculatedFieldsController', ['$scope', '$http', 'webApiBaseUrl', 'progressLoader', function ($scope, $http, webApiBaseUrl, progressLoader) {
            $scope.settings = { uploadComplete: false };

            $scope.run = function () {
                progressLoader.start();
                progressLoader.set(50);
                $http.post(webApiBaseUrl + '/api/importstatetestdata/updatecalculatedfields').then(function (response) {
                    $scope.settings.uploadComplete = true;
                    progressLoader.end();
                });
            }


        }])
        .controller('HRSUtilityController', ['$scope', '$http', 'webApiBaseUrl', 'progressLoader', function ($scope, $http, webApiBaseUrl, progressLoader) {
                $scope.settings = { uploadComplete: false };

                var paramObj = {
                    assessmentId: null,
                    formId: null,
                    suffix: '',
                    sentence:  ''
                };

                $scope.run = function () {
                    paramObj.assessmentId = $scope.settings.assessmentId;
                    paramObj.suffix = $scope.settings.suffix;
                    paramObj.sentence = $scope.settings.sentence;
                    paramObj.formId = $scope.settings.formId;

                    progressLoader.start();
                    progressLoader.set(50);
                    $http.post(webApiBaseUrl + '/api/assessment/createhrissentence', paramObj).then(function (response) {
                        $scope.settings.uploadComplete = true;
                        progressLoader.end();
                    });
                }


            }])
        
    ;


    //MNStateTestDataImportFinalController.$inject = ['$scope', '$q', '$http', 'nsPinesService', '$location', '$filter', 'webApiBaseUrl', 'nsFilterOptionsService', 'nsSelect2RemoteOptions', 'progressLoader', 'FileSaver', 'Blob', '$timeout'];
    //function MNStateTestDataImportFinalController($scope, $q, $http, nsPinesService, $location, $filter, webApiBaseUrl, nsFilterOptionsService, nsSelect2RemoteOptions, progressLoader, FileSaver, Blob, $timeout) {
    //    $scope.filterOptions = nsFilterOptionsService.options;
    //    $scope.getTeacherRemoteOptions = nsSelect2RemoteOptions.TeacherRemoteOptions;
    //    $scope.theFiles = [];
    //    $scope.settings = { hasFiles: false, uploadComplete: false };
    //    $scope.LogItems = [];

    //    // get scoped warnings and download
    //    $scope.downloadResult = function () {
    //        var text = '';

    //        for (var i = 0; i < $scope.LogItems.length; i++) {
    //            text += $scope.LogItems[i] + '\r\n';
    //        }

    //        var data = new Blob([text], { type: 'text/plain;charset=ANSI' });
    //        FileSaver.saveAs(data, 'results.txt');
    //        $scope.settings.uploadComplete = false;
    //    }


    //    $scope.upload = function (theFiles) {
    //        var formData = new FormData();

    //        angular.forEach(theFiles, function (file) {
    //            formData.append(file.name, file);
    //        });
    //        var paramObj = {};
    //        // start loader
    //        progressLoader.start();
    //        progressLoader.set(50);
    //        var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/uploadmnfinalcsv', formData, {
    //            transformRequest: angular.identity,
    //            headers: { 'Content-Type': undefined }
    //        }).then(function (response) {
    //            // end loader
    //            progressLoader.end();
    //            $scope.errors = [];
    //            $scope.LogItems = response.data.LogItems;
    //            // show success
    //            $timeout(function () {
    //                $('#formReset').click();
    //            }, 100);
    //            //$scope.theFiles.length = 0;
    //            //$scope.settings.hasFiles = false;
    //            $scope.settings.uploadComplete = true;
    //            nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
    //        });


    //    }
    //}

    //MNStateTestDataImportController.$inject = ['$scope', '$q', '$http', 'nsPinesService', '$location', '$filter', 'webApiBaseUrl', 'nsFilterOptionsService', 'nsSelect2RemoteOptions', 'progressLoader', 'FileSaver', 'Blob', '$timeout'];
    //function MNStateTestDataImportController($scope, $q, $http, nsPinesService, $location, $filter, webApiBaseUrl, nsFilterOptionsService, nsSelect2RemoteOptions, progressLoader, FileSaver, Blob, $timeout) {
    //    $scope.filterOptions = nsFilterOptionsService.options;
    //    $scope.getTeacherRemoteOptions = nsSelect2RemoteOptions.TeacherRemoteOptions;
    //    $scope.theFiles = [];
    //    $scope.settings = { hasFiles: false, uploadComplete: false };
    //    $scope.LogItems = [];

    //    // get scoped warnings and download
    //    $scope.downloadResult = function () {
    //        var text = '';

    //        for (var i = 0; i < $scope.LogItems.length; i++) {
    //            text += $scope.LogItems[i] + '\r\n';
    //        }

    //        var data = new Blob([text], { type: 'text/plain;charset=ANSI' });
    //        FileSaver.saveAs(data, 'results.txt');
    //        $scope.settings.uploadComplete = false;
    //    }


    //    $scope.upload = function (theFiles) {
    //        var formData = new FormData();

    //        angular.forEach(theFiles, function (file) {
    //            formData.append(file.name, file);
    //        });
    //        var paramObj = {};
    //        // start loader
    //        progressLoader.start();
    //        progressLoader.set(50);
    //        var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/uploadmncsv', formData, {
    //            transformRequest: angular.identity,
    //            headers: { 'Content-Type': undefined }
    //        }).then(function (response) {
    //            // end loader
    //            progressLoader.end();
    //            $scope.errors = [];
    //            $scope.LogItems = response.data.LogItems;
    //            // show success
    //            $timeout(function () {
    //                $('#formReset').click();
    //            }, 100);
    //            //$scope.theFiles.length = 0;
    //            //$scope.settings.hasFiles = false;
    //            $scope.settings.uploadComplete = true;
    //            nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
    //        });


    //    }
    //}

    //BenchmarkDataImportController.$inject = ['$scope', '$q', '$http', 'nsPinesService', '$location', '$filter', 'webApiBaseUrl', 'nsFilterOptionsService', 'nsSelect2RemoteOptions', 'progressLoader', 'FileSaver', 'Blob', '$timeout'];
    //function BenchmarkDataImportController($scope, $q, $http, nsPinesService, $location, $filter, webApiBaseUrl, nsFilterOptionsService, nsSelect2RemoteOptions, progressLoader, FileSaver, Blob, $timeout) {
    //    $scope.filterOptions = nsFilterOptionsService.options;
    //    $scope.getTeacherRemoteOptions = nsSelect2RemoteOptions.TeacherRemoteOptions;
    //    $scope.theFiles = [];
    //    $scope.settings = { hasFiles: false, uploadComplete: false };
    //    $scope.LogItems = [];

    //    // get scoped warnings and download
    //    $scope.downloadResult = function () {
    //        var text = '';

    //        for (var i = 0; i < $scope.LogItems.length; i++) {
    //            text += $scope.LogItems[i] + '\r\n';
    //        }

    //        var data = new Blob([text], { type: 'text/plain;charset=ANSI' });
    //        FileSaver.saveAs(data, 'results.txt');
    //        $scope.settings.uploadComplete = false;
    //    }


    //    $scope.upload = function (theFiles) {
    //        var formData = new FormData();
    //        formData.append("AssessmentId", 63);
    //        formData.append("BenchmarkDateId", $scope.filterOptions.selectedBenchmarkDate.id);
    //        formData.append("RecorderId", $scope.settings.Recorder.id);
    //        formData.append("SchoolYear", $scope.filterOptions.selectedSchoolYear.id);

    //        angular.forEach(theFiles, function (file) {
    //            formData.append(file.name, file);
    //        });
    //        var paramObj = {};
    //        // start loader
    //        progressLoader.start();
    //        progressLoader.set(50);
    //        var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/uploadcsv', formData, {
    //            transformRequest: angular.identity,
    //            headers: { 'Content-Type': undefined }
    //        }).then(function (response) {
    //            // end loader
    //            progressLoader.end();
    //            $scope.errors = [];
    //            $scope.LogItems = response.data.LogItems;
    //            // show success
    //            $timeout(function () {
    //                $('#formReset').click();
    //            }, 100);
    //            //$scope.theFiles.length = 0;
    //            //$scope.settings.hasFiles = false;
    //            $scope.settings.uploadComplete = true;
    //            nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
    //        });

            
    //    }
    //}



})();