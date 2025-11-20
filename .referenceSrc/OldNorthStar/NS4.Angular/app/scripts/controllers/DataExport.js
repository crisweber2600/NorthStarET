(function () {
    'use strict';

    angular
        .module('dataExportModule', [])
        .factory('AssessmentDataExportManager', [
        '$http', 'webApiBaseUrl', 'FileSaver', function ($http, webApiBaseUrl, FileSaver) {
            var AssessmentDataExportManager = function () {
                var self = this;

                self.LoadAssessmentDataExportHistory = function () {
                     var url = webApiBaseUrl + '/api/exportdata/LoadAssessmentDataExportHistory';
                    var promise = $http.get(url);

                    return promise.then(function (response) {
                        self.HistoryItems = response.data.HistoryItems;
                    });
                };

                self.LoadAssessmentDataAllFieldsExportHistory = function () {
                    var url = webApiBaseUrl + '/api/exportdata/LoadAssessmentDataExportHistoryAllFields';
                    var promise = $http.get(url);

                    return promise.then(function (response) {
                        self.HistoryItems = response.data.HistoryItems;
                    });
                };

                self.deleteHistoryItem = function (item) {
                    var url = webApiBaseUrl + '/api/exportdata/DeleteHistoryItem';
                    var paramObj = { id: item.Id };
                    var promise = $http.post(url, paramObj);

                    return promise;
                }

                self.deleteAllFieldsHistoryItem = function (item) {
                    var url = webApiBaseUrl + '/api/exportdata/DeleteAllFieldsHistoryItem';
                    var paramObj = { id: item.Id };
                    var promise = $http.post(url, paramObj);

                    return promise;
                }

                self.downloadExportFile = function (item) {
                    var url = webApiBaseUrl + '/api/exportdata/DownloadExportFile';
                    var paramObj = { value: item.UploadedFileName };
                    var promise = $http.post(url, paramObj, {
                        responseType: 'arraybuffer'
                    }).then(function (response) {
                        var data = new Blob([response.data]);
                        FileSaver.saveAs(data, "assessmentdataexport.csv");
                    });
                }

                self.downloadAllFieldsExportFile = function (item) {
                    var url = webApiBaseUrl + '/api/exportdata/DownloadAllFieldsExportFile';
                    var paramObj = { value: item.UploadedFileName };
                    var promise = $http.post(url, paramObj, {
                        responseType: 'arraybuffer'
                    }).then(function (response) {
                        var data = new Blob([response.data]);
                        FileSaver.saveAs(data, "assessmentdataexport.csv");
                    });
                }
            };

            return (AssessmentDataExportManager);
        }
        ])
        .factory('InterventionDataExportManager', [
        '$http', 'webApiBaseUrl', 'FileSaver', function ($http, webApiBaseUrl, FileSaver) {
            var InterventionDataExportManager = function () {
                var self = this;

                self.LoadInterventionDataExportHistory = function () {
                    var url = webApiBaseUrl + '/api/exportdata/LoadInterventionDataExportHistory';
                    var promise = $http.get(url);

                    return promise.then(function (response) {
                        self.HistoryItems = response.data.HistoryItems;
                    });
                };

                self.deleteHistoryItem = function (item) {
                    var url = webApiBaseUrl + '/api/exportdata/DeleteInterventionDataHistoryItem';
                    var paramObj = { id: item.Id };
                    var promise = $http.post(url, paramObj);

                    return promise;
                }
                 
                self.downloadExportFile = function (item) {
                    var url = webApiBaseUrl + '/api/exportdata/DownloadInterventionDataExportFile';
                    var paramObj = { value: item.UploadedFileName };
                    var promise = $http.post(url, paramObj, {
                        responseType: 'arraybuffer'
                    }).then(function (response) {
                        var data = new Blob([response.data]);
                        FileSaver.saveAs(data, "interventiondataexport.csv");
                    });
                }
            };  

            return (InterventionDataExportManager);
        }
                ])
        .controller('AssessmentDataExportController', ['$scope', '$http', 'webApiBaseUrl', 'progressLoader', 'nsFilterOptionsService', 'FileSaver', '$timeout', 'nsPinesService', '$interval', '$bootbox', 'nsStackedBarGraphOptionsFactory', 'AssessmentDataExportManager',
            function ($scope, $http, webApiBaseUrl, progressLoader, nsFilterOptionsService, FileSaver, $timeout, nsPinesService, $interval, $bootbox, nsStackedBarGraphOptionsFactory, AssessmentDataExportManager) {
                $scope.settings = { uploadComplete: false, hasFiles: false };
                $scope.dataMgr = new AssessmentDataExportManager();
                $scope.groupsFactory = new nsStackedBarGraphOptionsFactory('Get Assessment Export Data', false);
                $scope.filterOptions = $scope.groupsFactory.options;

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

                $scope.downloadExportFile = function (item) {
                    $scope.dataMgr.downloadExportFile(item);
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
                    $scope.dataMgr.LoadAssessmentDataExportHistory();
                };
                reloadHistoryTable();

                 //reload table every 5 seconds
                var reloadInterval = $interval(reloadHistoryTable, 5000);
                 //here is where the cleanup happens
                $scope.$on('$destroy', function () {
                    $interval.cancel(reloadInterval);
                });

                 
                $scope.getExportData = function () {
                    $scope.groupsFactory.loadOSExportData();
                    nsPinesService.buildMessage('Export Requested', 'Your data export has been requested', 'info');
                }

            }])
            .controller('AssessmentDataExportAllFieldsController', ['$scope', '$http', 'webApiBaseUrl', 'progressLoader', 'nsFilterOptionsService', 'FileSaver', '$timeout', 'nsPinesService', '$interval', '$bootbox', 'nsStackedBarGraphOptionsFactory', 'AssessmentDataExportManager',
            function ($scope, $http, webApiBaseUrl, progressLoader, nsFilterOptionsService, FileSaver, $timeout, nsPinesService, $interval, $bootbox, nsStackedBarGraphOptionsFactory, AssessmentDataExportManager) {
                $scope.settings = { uploadComplete: false, hasFiles: false };
                $scope.dataMgr = new AssessmentDataExportManager();
                $scope.groupsFactory = new nsStackedBarGraphOptionsFactory('Get Assessment Export Data', false);
                $scope.filterOptions = $scope.groupsFactory.options;

                $scope.deleteHistoryItem = function (item) {
                    $bootbox.confirm('Are you sure you want to delete this job?  <b>Note:</b> If it has not yet been processed, it will be cancelled.',
                        function (response) {
                            if (response) {
                                progressLoader.start();
                                progressLoader.set(50);
                                $scope.dataMgr.deleteAllFieldsHistoryItem(item).then(function (response) {
                                    nsPinesService.dataDeletedSuccessfully();
                                    reloadHistoryTable();
                                    progressLoader.end();
                                });
                            }
                        });
                }

                $scope.downloadExportFile = function (item) {
                    $scope.dataMgr.downloadAllFieldsExportFile(item);
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
                    $scope.dataMgr.LoadAssessmentDataAllFieldsExportHistory();
                };
                reloadHistoryTable();

                //reload table every 5 seconds
                var reloadInterval = $interval(reloadHistoryTable, 5000);
                //here is where the cleanup happens
                $scope.$on('$destroy', function () {
                    $interval.cancel(reloadInterval);
                });


                $scope.getExportData = function () {
                    $scope.groupsFactory.loadAllFieldsExportData();
                    nsPinesService.buildMessage('Export Requested', 'Your data export has been requested', 'info');
                }

            }])
        .controller('InterventionGroupDataExportController', ['$scope', '$http', 'webApiBaseUrl', 'progressLoader', 'nsFilterOptionsService', 'FileSaver', '$timeout', 'nsPinesService', '$interval', '$bootbox', 'nsStackedBarGraphOptionsFactory', 'InterventionDataExportManager',
            function ($scope, $http, webApiBaseUrl, progressLoader, nsFilterOptionsService, FileSaver, $timeout, nsPinesService, $interval, $bootbox, nsStackedBarGraphOptionsFactory, InterventionDataExportManager) {
                $scope.settings = { uploadComplete: false, hasFiles: false };
                $scope.dataMgr = new InterventionDataExportManager();
                $scope.groupsFactory = new nsStackedBarGraphOptionsFactory('Get Assessment Export Data', false);
                $scope.filterOptions = $scope.groupsFactory.options;

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

                $scope.downloadExportFile = function (item) {
                    $scope.dataMgr.downloadExportFile(item);
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
                    $scope.dataMgr.LoadInterventionDataExportHistory();
                };
                reloadHistoryTable();

                //reload table every 5 seconds
                var reloadInterval = $interval(reloadHistoryTable, 5000);
                //here is where the cleanup happens
                $scope.$on('$destroy', function () {
                    $interval.cancel(reloadInterval);
                });


                $scope.getExportData = function () {
                    if ($scope.groupsFactory.options.selectedInterventionAssessment == null) {
                        $bootbox.alert("Please select an Assessment Type to export.");
                        return;
                    }
                      
                    $bootbox.prompt('Please enter a name for this batch.', function (response) {
                        if (!response) {
                            $bootbox.alert('Batch cannot be created without a name.');
                        } else {
                            $scope.groupsFactory.loadIGExportData(response);
                            nsPinesService.buildMessage('Export Requested', 'Your data export has been requested', 'info');
                        }
                    })

                }

            }])
    .factory('AttendanceDataExportManager', [
        '$http', 'webApiBaseUrl', 'FileSaver', function ($http, webApiBaseUrl, FileSaver) {
            var AttendanceDataExportManager = function () {
                var self = this;

                self.LoadHistory = function () {
                    var url = webApiBaseUrl + '/api/exportdata/LoadAttendanceDataExportHistory';
                    var promise = $http.get(url);

                    return promise.then(function (response) {
                        self.HistoryItems = response.data.HistoryItems;
                    });
                };

                self.deleteHistoryItem = function (item) {
                    var url = webApiBaseUrl + '/api/exportdata/DeleteAttendanceHistoryItem';
                    var paramObj = { Id: item.Id };
                    var promise = $http.post(url, paramObj);

                    return promise;
                }

                self.getExportData = function (year) {

                    var returnObject = {
                        id: year,
                    };

                    // doesn't actually return the data, just creates a job
                    return $http.post(webApiBaseUrl + "/api/exportdata/CreateAttendanceDataExportJob", returnObject);
                }

                self.downloadExportFile = function (item) {
                    var url = webApiBaseUrl + '/api/exportdata/DownloadAttendanceExportFile';
                    var paramObj = { value: item.UploadedFileName };
                    var promise = $http.post(url, paramObj, {
                        responseType: 'arraybuffer'
                    }).then(function (response) {
                        var data = new Blob([response.data]);
                        FileSaver.saveAs(data, "attendancedataexport.csv");
                    });
                }
            };

            return (AttendanceDataExportManager);
        }
    ])
        .controller('AttendanceDataExportController', ['$scope', '$http', 'webApiBaseUrl', 'progressLoader', 'nsFilterOptionsService', 'FileSaver', '$timeout', 'nsPinesService', '$interval', '$bootbox', 'AttendanceDataExportManager',
            function ($scope, $http, webApiBaseUrl, progressLoader, nsFilterOptionsService, FileSaver, $timeout, nsPinesService, $interval, $bootbox, AttendanceDataExportManager) {
                $scope.settings = { uploadComplete: false, hasFiles: false };
                $scope.dataMgr = new AttendanceDataExportManager();
                $scope.filterOptions = nsFilterOptionsService.options;

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

                $scope.downloadExportFile = function (item) {
                    $scope.dataMgr.downloadExportFile(item);
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
                    $scope.dataMgr.LoadHistory();
                };
                reloadHistoryTable();

                //reload table every 5 seconds
                var reloadInterval = $interval(reloadHistoryTable, 5000);
                //here is where the cleanup happens
                $scope.$on('$destroy', function () {
                    $interval.cancel(reloadInterval);
                });


                $scope.getExportData = function () {
                    if ($scope.filterOptions.selectedSchoolYear != null) {
                        $scope.dataMgr.getExportData($scope.filterOptions.selectedSchoolYear.id);
                        nsPinesService.buildMessage('Export Requested', 'Your data export has been requested', 'info');
                    }
                }

            }])
         .factory('StudentExportManager', [
        '$http', 'webApiBaseUrl', 'FileSaver', '$bootbox', function ($http, webApiBaseUrl, FileSaver, $bootbox) {
            var StudentExportManager = function () {
                var self = this;

                self.LoadHistory = function () {
                    var url = webApiBaseUrl + '/api/exportdata/LoadStudentExportHistory';
                    var promise = $http.get(url);

                    return promise.then(function (response) {
                        self.HistoryItems = response.data.HistoryItems;
                    });
                };

                self.deleteHistoryItem = function (item) {
                    var url = webApiBaseUrl + '/api/exportdata/DeleteStudentHistoryItem';
                    var paramObj = { Id: item.Id };
                    var promise = $http.post(url, paramObj);

                    return promise;
                }

                self.getExportData = function () {
                    $bootbox.prompt('Please enter a name for this batch.', function (response) {
                        if (!response) {
                            $bootbox.alert('Batch cannot be created without a name.');
                            return;
                        } else {
                            var returnObject = {
                                value: response
                            };

                            // doesn't actually return the data, just creates a job
                            return $http.post(webApiBaseUrl + "/api/exportdata/CreateStudentExportJob", returnObject);
                        }
                    });
                }

                self.downloadExportFile = function (item) {
                    var url = webApiBaseUrl + '/api/exportdata/DownloadStudentExportFile';
                    var paramObj = { value: item.UploadedFileName };
                    var promise = $http.post(url, paramObj, {
                        responseType: 'arraybuffer'
                    }).then(function (response) {
                        var data = new Blob([response.data]);
                        FileSaver.saveAs(data, "studentexport.csv");
                    });
                }
            };

            return (StudentExportManager);
        }
         ])
        .controller('StudentExportController', ['$scope', '$http', 'webApiBaseUrl', 'progressLoader', 'nsFilterOptionsService', 'FileSaver', '$timeout', 'nsPinesService', '$interval', '$bootbox', 'StudentExportManager',
            function ($scope, $http, webApiBaseUrl, progressLoader, nsFilterOptionsService, FileSaver, $timeout, nsPinesService, $interval, $bootbox, StudentExportManager) {
                $scope.settings = { uploadComplete: false, hasFiles: false };
                $scope.dataMgr = new StudentExportManager();
                $scope.filterOptions = nsFilterOptionsService.options;

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

                $scope.downloadExportFile = function (item) {
                    $scope.dataMgr.downloadExportFile(item);
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
                    $scope.dataMgr.LoadHistory();
                };
                reloadHistoryTable();

                //reload table every 5 seconds
                var reloadInterval = $interval(reloadHistoryTable, 5000);
                //here is where the cleanup happens
                $scope.$on('$destroy', function () {
                    $interval.cancel(reloadInterval);
                });


                $scope.getExportData = function () {
                    // get name of export
                    $scope.dataMgr.getExportData();
                    nsPinesService.buildMessage('Export Requested', 'Your data export has been requested', 'info');
                }

            }])
         .factory('StaffExportManager', [
        '$http', 'webApiBaseUrl', 'FileSaver', '$bootbox', function ($http, webApiBaseUrl, FileSaver, $bootbox) {
            var StaffExportManager = function () {
                var self = this;

                self.LoadHistory = function () {
                    var url = webApiBaseUrl + '/api/exportdata/LoadStaffExportHistory';
                    var promise = $http.get(url);

                    return promise.then(function (response) {
                        self.HistoryItems = response.data.HistoryItems;
                    });
                };

                self.deleteHistoryItem = function (item) {
                    var url = webApiBaseUrl + '/api/exportdata/DeleteStaffHistoryItem';
                    var paramObj = { Id: item.Id };
                    var promise = $http.post(url, paramObj);

                    return promise;
                }

                self.getExportData = function () {
                    $bootbox.prompt('Please enter a name for this batch.', function (response) {
                        if (!response) {
                            $bootbox.alert('Batch cannot be created without a name.');
                            return;
                        } else {
                            var returnObject = {
                                value: response
                            };

                            // doesn't actually return the data, just creates a job
                            return $http.post(webApiBaseUrl + "/api/exportdata/CreateStaffExportJob", returnObject);
                        }
                    });
                }

                self.downloadExportFile = function (item) {
                    var url = webApiBaseUrl + '/api/exportdata/DownloadStaffExportFile';
                    var paramObj = { value: item.UploadedFileName };
                    var promise = $http.post(url, paramObj, {
                        responseType: 'arraybuffer'
                    }).then(function (response) {
                        var data = new Blob([response.data]);
                        FileSaver.saveAs(data, "staffexport.csv");
                    });
                }
            };

            return (StaffExportManager);
        }
         ])
        .controller('StaffExportController', ['$scope', '$http', 'webApiBaseUrl', 'progressLoader', 'nsFilterOptionsService', 'FileSaver', '$timeout', 'nsPinesService', '$interval', '$bootbox', 'StaffExportManager',
            function ($scope, $http, webApiBaseUrl, progressLoader, nsFilterOptionsService, FileSaver, $timeout, nsPinesService, $interval, $bootbox, StaffExportManager) {
                $scope.settings = { uploadComplete: false, hasFiles: false };
                $scope.dataMgr = new StaffExportManager();
                $scope.filterOptions = nsFilterOptionsService.options;

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

                $scope.downloadExportFile = function (item) {
                    $scope.dataMgr.downloadExportFile(item);
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
                    $scope.dataMgr.LoadHistory();
                };
                reloadHistoryTable();

                //reload table every 5 seconds
                var reloadInterval = $interval(reloadHistoryTable, 5000);
                //here is where the cleanup happens
                $scope.$on('$destroy', function () {
                    $interval.cancel(reloadInterval);
                });


                $scope.getExportData = function () {
                    // get name of export
                    $scope.dataMgr.getExportData();
                    nsPinesService.buildMessage('Export Requested', 'Your data export has been requested', 'info');
                }

            }])
      .factory('PrintBatchManager', [
        '$http', 'webApiBaseUrl', 'FileSaver', function ($http, webApiBaseUrl, FileSaver) {
            var PrintBatchManager = function () {
                var self = this;

                self.LoadHistory = function () {
                    var url = webApiBaseUrl + '/api/exportdata/LoadBatchPrintHistory';
                    var promise = $http.get(url);

                    return promise.then(function (response) {
                        self.HistoryItems = response.data.HistoryItems;
                    });
                };

                self.deleteHistoryItem = function (item) {
                    var url = webApiBaseUrl + '/api/exportdata/DeleteBatchPrintHistoryItem';
                    var paramObj = { Id: item.Id };
                    var promise = $http.post(url, paramObj);

                    return promise;
                }

                self.downloadExportFile = function (item) {
                    var url = webApiBaseUrl + '/api/exportdata/DownloadBatchPrintFile';
                    var paramObj = { value: item.UploadedFileName };
                    var promise = $http.post(url, paramObj, {
                        responseType: 'arraybuffer'
                    }).then(function (response) {
                        var data = new Blob([response.data]);
                        FileSaver.saveAs(data, "batchprint.pdf");
                    });
                }
            };

            return (PrintBatchManager);
        }
      ])
        .controller('PrintBatchController', ['$scope', '$http', 'webApiBaseUrl', 'progressLoader', 'nsFilterOptionsService', 'FileSaver', '$timeout', 'nsPinesService', '$interval', '$bootbox', 'PrintBatchManager', 'nsStackedBarGraphOptionsFactory',
            function ($scope, $http, webApiBaseUrl, progressLoader, nsFilterOptionsService, FileSaver, $timeout, nsPinesService, $interval, $bootbox, PrintBatchManager, nsStackedBarGraphOptionsFactory) {
                $scope.settings = { uploadComplete: false, hasFiles: false };
                $scope.dataMgr = new PrintBatchManager();
                $scope.groupsFactory = new nsStackedBarGraphOptionsFactory('Get Assessment Export Data', false);
                $scope.filterOptions = nsFilterOptionsService.options;

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

                $scope.downloadExportFile = function (item) {
                    $scope.dataMgr.downloadExportFile(item);
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
                    $scope.dataMgr.LoadHistory();
                };
                reloadHistoryTable();

                //reload table every 5 seconds
                var reloadInterval = $interval(reloadHistoryTable, 5000);
                //here is where the cleanup happens
                $scope.$on('$destroy', function () {
                    $interval.cancel(reloadInterval);
                });


                $scope.getExportData = function () {
                    // make sure at least
                    if ($scope.groupsFactory.options.selectedPageTypes.length == 0) {
                        $bootbox.alert("Please select at least one Page Type to Print.");
                        return;
                    }

                    if ($scope.groupsFactory.options.selectedSchools.length == 0) {
                        $bootbox.alert("Please select at least one school to create a print batch.");
                        return;
                    }

                    if ($scope.groupsFactory.options.selectedTestDueDate == null) {
                        $bootbox.alert("Please ask your District Administrator to set your district's benchmark dates before using this tool.");
                        return;
                    }

                    $bootbox.prompt('Please enter a name for this batch.', function (response) {
                        if (!response) {
                            $bootbox.alert('Batch cannot be created without a name.');
                        } else {
                            $scope.groupsFactory.createPrintBatch(response);
                            nsPinesService.buildMessage('Export Requested', 'Your data export has been requested', 'info');
                        }
                    })

                }

            }]);



})();