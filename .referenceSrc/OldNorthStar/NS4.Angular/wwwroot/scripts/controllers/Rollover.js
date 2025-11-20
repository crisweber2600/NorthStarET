(function () {
    'use strict';

    angular
        .module('rolloverModule', [])
        .factory('FullRolloverManager', [
        '$http', 'webApiBaseUrl', 'FileSaver', function ($http, webApiBaseUrl, FileSaver) {
            var FullRolloverManager = function () {
                var self = this;

                self.LoadTemplate = function () {
                    var url = webApiBaseUrl + '/api/RosterRollover/GetFullRolloverImportTemplate';
                    var promise = $http.get(url);

                    return promise.then(function (response) {
                        self.Fields = response.data.Fields;
                    });
                };

                self.LoadImportHistory = function () {
                    var url = webApiBaseUrl + '/api/RosterRollover/LoadFullRolloverImportHistory';
                    var promise = $http.get(url);

                    return promise.then(function (response) {
                        angular.extend(self, response.data);
                        //self.RolloverInProgress = response.data.RolloverInProgress;
                        //if (self.HistoryItems) {
                        //    while(self.HistoryItems.length)
                        //        self.HistoryItems.pop();
                        //}
                        //self.HistoryItems = response.data.HistoryItems;
                    });
                };

                self.FullRolloverReset = function (item) {
                    var url = webApiBaseUrl + '/api/RosterRollover/FullRolloverReset';
                    var promise = $http.post(url);

                    return promise;
                }

                self.deleteHistoryItem = function (item) {
                    var url = webApiBaseUrl + '/api/RosterRollover/DeleteFullRolloverHistoryItem';
                    var paramObj = { id: item.Id };
                    var promise = $http.post(url, paramObj);

                    return promise;
                }

                self.CancelRollover = function (item) {
                    var url = webApiBaseUrl + '/api/RosterRollover/CancelRollover';
                    var paramObj = { id: item.Id };
                    var promise = $http.post(url, paramObj);

                    return promise;
                }

                self.validateRollover = function (item) {
                    var url = webApiBaseUrl + '/api/RosterRollover/validateRollover';
                    var paramObj = { id: item.Id };
                    var promise = $http.post(url, paramObj);

                    return promise;
                }

                self.downloadImportFile = function (item) {
                    var url = webApiBaseUrl + '/api/RosterRollover/GetImportFile';
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

                    var promise = $http.post(webApiBaseUrl + '/api/RosterRollover/GetFullRolloverHistoryLog', paramObj);
                    promise.then(function (response) {
                        var data = new Blob([response.data.Result], { type: 'text/csv;charset=ANSI' });
                        FileSaver.saveAs(data, 'importlog.txt');
                    });
                }
            };

            return (FullRolloverManager);
        }
        ])
        .controller('FullRolloverController', ['$scope', '$http', 'webApiBaseUrl', 'progressLoader', 'FullRolloverManager', 'nsFilterOptionsService', 'FileSaver', '$timeout', 'nsPinesService', '$interval', '$bootbox', '$uibModal',
            function ($scope, $http, webApiBaseUrl, progressLoader, FullRolloverManager, nsFilterOptionsService, FileSaver, $timeout, nsPinesService, $interval, $bootbox, $uibModal) {
                $scope.settings = { uploadComplete: false, hasFiles: false };
                $scope.dataMgr = new FullRolloverManager();
                $scope.filterOptions = nsFilterOptionsService.options;
                $scope.theFiles = [];

                $scope.downloadImportFile = function (item) {
                    $scope.dataMgr.downloadImportFile(item);
                };

                $scope.checkNewYear = function () {
                    if (moment().month() > 6 && $scope.filterOptions.selectedSchoolYear.id < moment().year()) {
                        return false;
                    }

                    return true;
                }

                $scope.verificationDialog = function (job) {

                    var modalInstance = $uibModal.open({
                        templateUrl: 'rolloverIssueValidation.html',
                        scope: $scope,
                        controller: function ($scope, $uibModalInstance) {
                            $scope.selectedJob = job;
                            $scope.issues = job.RolloverLogMessages;

                            $scope.ensureAllSelected = function () {
                                for (var i = 0; i < $scope.issues.length; i++) {
                                    if ($scope.issues[i].Validate != true) {
                                        return true;
                                    }
                                }

                                return false;
                            }

                            $scope.validateRollover = function (job) {
                                $bootbox.confirm('Are you sure you want to ignore all potential issues listed for this rollover? <BR><BR> <b>Note:</b> You may end up with duplicate students and/or teachers if the issues are not investigated thoroughly.',
                                  function (response) {
                                      if (response) {
                                          progressLoader.start();
                                          progressLoader.set(50);
                                          $scope.dataMgr.validateRollover(job).then(function (response) {
                                              progressLoader.end();
                                              $uibModalInstance.dismiss('cancel');
                                          });
                                      }
                                  });
                            }

                            $scope.cancel = function () {
                                $uibModalInstance.dismiss('cancel');
                            };
                        },
                        size: 'md',
                    });
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
                $scope.cancelRollover = function (job) {
                    $bootbox.confirm('Are you sure you want to cancel this rollover? <BR><BR> <b>Note:</b> You can resolve any issues and resubmit the rollover file to try again.',
                      function (response) {
                          if (response) {
                              progressLoader.start();
                              progressLoader.set(50);
                              $scope.dataMgr.CancelRollover(job).then(function (response) {
                                  progressLoader.end();
                              });
                          }
                      });
                };

                $scope.FullRolloverReset = function () {
                    $bootbox.confirm('Are you sure you want to cancel any rollover that may be in progress? <BR><BR> <b>Note:</b> You typically only need to do this if a rollover is in a "hung" state or cannot be completed for some reason.',
                      function (response) {
                          if (response) {
                              progressLoader.start();
                              progressLoader.set(50);
                              $scope.dataMgr.FullRolloverReset().then(function (response) {
                                  progressLoader.end();
                              });
                          }
                      });
                };

                reloadHistoryTable();

                // reload table every 5 seconds
                var reloadInterval = $interval(reloadHistoryTable, 5000);
                // here is where the cleanup happens
                $scope.$on('$destroy', function () {
                    $interval.cancel(reloadInterval);
                });


                $scope.getTemplate = function () {

                    var promise = $http.get(webApiBaseUrl + '/api/RosterRollover/GetFullRolloverTemplateCSV');
                    promise.then(function (response) {
                        var data = new Blob([response.data.Result], { type: 'text/csv;charset=ANSI' });
                        FileSaver.saveAs(data, 'template.csv');
                    });
                };

                $scope.upload = function (theFiles) {
                    var formData = new FormData();
                    formData.append("SchoolYear", $scope.filterOptions.selectedSchoolYear.id);

                    angular.forEach(theFiles, function (file) {
                        formData.append(file.name, file);
                    });
                    var paramObj = {};
                    // start loader
                    progressLoader.start();
                    progressLoader.set(50);
                    var promise = $http.post(webApiBaseUrl + '/api/RosterRollover/uploadfullrollovercsv', formData, {
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

                progressLoader.start();
                progressLoader.set(50);
                $scope.dataMgr.LoadTemplate().then(function (response) {
                    progressLoader.end();
                });
            }])
    .factory('StudentRolloverManager', [
        '$http', 'webApiBaseUrl', 'FileSaver', function ($http, webApiBaseUrl, FileSaver) {
            var StudentRolloverManager = function () {
                var self = this;

                self.LoadTemplate = function () {
                    var url = webApiBaseUrl + '/api/RosterRollover/GetStudentRolloverImportTemplate';
                    var promise = $http.get(url);

                    return promise.then(function (response) {
                        self.Fields = response.data.Fields;
                    });
                };

                self.LoadImportHistory = function () {
                    var url = webApiBaseUrl + '/api/RosterRollover/LoadStudentRolloverImportHistory';
                    var promise = $http.get(url);

                    return promise.then(function (response) {
                        angular.extend(self, response.data);
                    });
                };

                self.FullRolloverReset = function (item) {
                    var url = webApiBaseUrl + '/api/RosterRollover/StudentRolloverReset';
                    var promise = $http.post(url);

                    return promise;
                }

                self.deleteHistoryItem = function (item) {
                    var url = webApiBaseUrl + '/api/RosterRollover/DeleteStudentRolloverHistoryItem';
                    var paramObj = { id: item.Id };
                    var promise = $http.post(url, paramObj);

                    return promise;
                }

                self.CancelRollover = function (item) {
                    var url = webApiBaseUrl + '/api/RosterRollover/CancelStudentRollover';
                    var paramObj = { id: item.Id };
                    var promise = $http.post(url, paramObj);

                    return promise;
                }

                self.validateRollover = function (item) {
                    var url = webApiBaseUrl + '/api/RosterRollover/validateStudentRollover';
                    var paramObj = { id: item.Id };
                    var promise = $http.post(url, paramObj);

                    return promise;
                }

                self.downloadImportFile = function (item) {
                    var url = webApiBaseUrl + '/api/RosterRollover/GetImportFile';
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

                    var promise = $http.post(webApiBaseUrl + '/api/RosterRollover/GetStudentRolloverHistoryLog', paramObj);
                    promise.then(function (response) {
                        var data = new Blob([response.data.Result], { type: 'text/csv;charset=ANSI' });
                        FileSaver.saveAs(data, 'importlog.txt');
                    });
                }
            };

            return (StudentRolloverManager);
        }
    ])
        .controller('StudentRolloverController', ['$scope', '$http', 'webApiBaseUrl', 'progressLoader', 'StudentRolloverManager', 'nsFilterOptionsService', 'FileSaver', '$timeout', 'nsPinesService', '$interval', '$bootbox', '$uibModal',
            function ($scope, $http, webApiBaseUrl, progressLoader, StudentRolloverManager, nsFilterOptionsService, FileSaver, $timeout, nsPinesService, $interval, $bootbox, $uibModal) {
                $scope.settings = { uploadComplete: false, hasFiles: false };
                $scope.dataMgr = new StudentRolloverManager();
                $scope.filterOptions = nsFilterOptionsService.options;
                $scope.theFiles = [];

                $scope.downloadImportFile = function (item) {
                    $scope.dataMgr.downloadImportFile(item);
                };

                $scope.verificationDialog = function (job) {

                    var modalInstance = $uibModal.open({
                        templateUrl: 'studentRolloverIssueValidation.html',
                        scope: $scope,
                        controller: function ($scope, $uibModalInstance) {
                            $scope.selectedJob = job;
                            $scope.issues = job.RolloverLogMessages;

                            $scope.ensureAllSelected = function () {
                                for (var i = 0; i < $scope.issues.length; i++) {
                                    if ($scope.issues[i].Validate != true) {
                                        return true;
                                    }
                                }

                                return false;
                            }

                            $scope.validateRollover = function (job) {
                                $bootbox.confirm('Are you sure you want to ignore all potential issues listed for this rollover? <BR><BR> <b>Note:</b> You may end up with duplicate students if the issues are not investigated thoroughly.',
                                  function (response) {
                                      if (response) {
                                          progressLoader.start();
                                          progressLoader.set(50);
                                          $scope.dataMgr.validateRollover(job).then(function (response) {
                                              progressLoader.end();
                                              $uibModalInstance.dismiss('cancel');
                                          });
                                      }
                                  });
                            }

                            $scope.cancel = function () {
                                $uibModalInstance.dismiss('cancel');
                            };
                        },
                        size: 'md',
                    });
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
                $scope.cancelRollover = function (job) {
                    $bootbox.confirm('Are you sure you want to cancel this rollover? <BR><BR> <b>Note:</b> You can resolve any issues and resubmit the rollover file to try again.',
                      function (response) {
                          if (response) {
                              progressLoader.start();
                              progressLoader.set(50);
                              $scope.dataMgr.CancelRollover(job).then(function (response) {
                                  progressLoader.end();
                              });
                          }
                      });
                };

                $scope.FullRolloverReset = function () {
                    $bootbox.confirm('Are you sure you want to cancel any rollover that may be in progress? <BR><BR> <b>Note:</b> You typically only need to do this if a rollover is in a "hung" state or cannot be completed for some reason.',
                      function (response) {
                          if (response) {
                              progressLoader.start();
                              progressLoader.set(50);
                              $scope.dataMgr.FullRolloverReset().then(function (response) {
                                  progressLoader.end();
                              });
                          }
                      });
                };

                reloadHistoryTable();

                // reload table every 5 seconds
                var reloadInterval = $interval(reloadHistoryTable, 5000);
                // here is where the cleanup happens
                $scope.$on('$destroy', function () {
                    $interval.cancel(reloadInterval);
                });


                $scope.getTemplate = function () {

                    var promise = $http.get(webApiBaseUrl + '/api/RosterRollover/GetStudentRolloverTemplateCSV');
                    promise.then(function (response) {
                        var data = new Blob([response.data.Result], { type: 'text/csv;charset=ANSI' });
                        FileSaver.saveAs(data, 'template.csv');
                    });
                };

                $scope.upload = function (theFiles) {
                    var formData = new FormData();

                    $bootbox.prompt('Please enter a name for this batch.', function (response) {
                        if (!response) {
                            $bootbox.alert('Batch cannot be created without a name.');
                            return;
                        } else {
                            formData.append("BatchName", response);

                            angular.forEach(theFiles, function (file) {
                                formData.append(file.name, file);
                            });
                            var paramObj = {};
                            // start loader
                            progressLoader.start();
                            progressLoader.set(50);
                            var promise = $http.post(webApiBaseUrl + '/api/RosterRollover/uploadstudentrollovercsv', formData, {
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
                        }
                    })
                };

                progressLoader.start();
                progressLoader.set(50);
                $scope.dataMgr.LoadTemplate().then(function (response) {
                    progressLoader.end();
                });
            }])
    .factory('TeacherRolloverManager', [
        '$http', 'webApiBaseUrl', 'FileSaver', function ($http, webApiBaseUrl, FileSaver) {
            var StudentRolloverManager = function () {
                var self = this;

                self.LoadTemplate = function () {
                    var url = webApiBaseUrl + '/api/RosterRollover/GetTeacherRolloverImportTemplate';
                    var promise = $http.get(url);

                    return promise.then(function (response) {
                        self.Fields = response.data.Fields;
                    });
                };

                self.LoadImportHistory = function () {
                    var url = webApiBaseUrl + '/api/RosterRollover/LoadTeacherRolloverImportHistory';
                    var promise = $http.get(url);

                    return promise.then(function (response) {
                        angular.extend(self, response.data);
                    });
                };

                self.FullRolloverReset = function (item) {
                    var url = webApiBaseUrl + '/api/RosterRollover/TeacherRolloverReset';
                    var promise = $http.post(url);

                    return promise;
                }

                self.deleteHistoryItem = function (item) {
                    var url = webApiBaseUrl + '/api/RosterRollover/DeleteTeacherRolloverHistoryItem';
                    var paramObj = { id: item.Id };
                    var promise = $http.post(url, paramObj);

                    return promise;
                }

                self.CancelRollover = function (item) {
                    var url = webApiBaseUrl + '/api/RosterRollover/CancelTeacherRollover';
                    var paramObj = { id: item.Id };
                    var promise = $http.post(url, paramObj);

                    return promise;
                }

                self.validateRollover = function (item) {
                    var url = webApiBaseUrl + '/api/RosterRollover/validateTeacherRollover';
                    var paramObj = { id: item.Id };
                    var promise = $http.post(url, paramObj);

                    return promise;
                }

                self.downloadImportFile = function (item) {
                    var url = webApiBaseUrl + '/api/RosterRollover/GetImportFile';
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

                    var promise = $http.post(webApiBaseUrl + '/api/RosterRollover/GetTeacherRolloverHistoryLog', paramObj);
                    promise.then(function (response) {
                        var data = new Blob([response.data.Result], { type: 'text/csv;charset=ANSI' });
                        FileSaver.saveAs(data, 'importlog.txt');
                    });
                }
            };

            return (StudentRolloverManager);
        }
    ])
        .controller('TeacherRolloverController', ['$scope', '$http', 'webApiBaseUrl', 'progressLoader', 'TeacherRolloverManager', 'nsFilterOptionsService', 'FileSaver', '$timeout', 'nsPinesService', '$interval', '$bootbox', '$uibModal',
            function ($scope, $http, webApiBaseUrl, progressLoader, TeacherRolloverManager, nsFilterOptionsService, FileSaver, $timeout, nsPinesService, $interval, $bootbox, $uibModal) {
                $scope.settings = { uploadComplete: false, hasFiles: false };
                $scope.dataMgr = new TeacherRolloverManager();
                $scope.filterOptions = nsFilterOptionsService.options;
                $scope.theFiles = [];

                $scope.downloadImportFile = function (item) {
                    $scope.dataMgr.downloadImportFile(item);
                };

                $scope.verificationDialog = function (job) {

                    var modalInstance = $uibModal.open({
                        templateUrl: 'teacherRolloverIssueValidation.html',
                        scope: $scope,
                        controller: function ($scope, $uibModalInstance) {
                            $scope.selectedJob = job;
                            $scope.issues = job.RolloverLogMessages;

                            $scope.ensureAllSelected = function () {
                                for (var i = 0; i < $scope.issues.length; i++) {
                                    if ($scope.issues[i].Validate != true) {
                                        return true;
                                    }
                                }

                                return false;
                            }

                            $scope.validateRollover = function (job) {
                                $bootbox.confirm('Are you sure you want to ignore all potential issues listed for this rollover? <BR><BR> <b>Note:</b> You may end up with duplicate teachers if the issues are not investigated thoroughly.',
                                  function (response) {
                                      if (response) {
                                          progressLoader.start();
                                          progressLoader.set(50);
                                          $scope.dataMgr.validateRollover(job).then(function (response) {
                                              progressLoader.end();
                                              $uibModalInstance.dismiss('cancel');
                                          });
                                      }
                                  });
                            }

                            $scope.cancel = function () {
                                $uibModalInstance.dismiss('cancel');
                            };
                        },
                        size: 'md',
                    });
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
                $scope.cancelRollover = function (job) {
                    $bootbox.confirm('Are you sure you want to cancel this rollover? <BR><BR> <b>Note:</b> You can resolve any issues and resubmit the rollover file to try again.',
                      function (response) {
                          if (response) {
                              progressLoader.start();
                              progressLoader.set(50);
                              $scope.dataMgr.CancelRollover(job).then(function (response) {
                                  progressLoader.end();
                              });
                          }
                      });
                };

                $scope.FullRolloverReset = function () {
                    $bootbox.confirm('Are you sure you want to cancel any rollover that may be in progress? <BR><BR> <b>Note:</b> You typically only need to do this if a rollover is in a "hung" state or cannot be completed for some reason.',
                      function (response) {
                          if (response) {
                              progressLoader.start();
                              progressLoader.set(50);
                              $scope.dataMgr.FullRolloverReset().then(function (response) {
                                  progressLoader.end();
                              });
                          }
                      });
                };

                reloadHistoryTable();

                // reload table every 5 seconds
                var reloadInterval = $interval(reloadHistoryTable, 5000);
                // here is where the cleanup happens
                $scope.$on('$destroy', function () {
                    $interval.cancel(reloadInterval);
                });


                $scope.getTemplate = function () {

                    var promise = $http.get(webApiBaseUrl + '/api/RosterRollover/GetTeacherRolloverTemplateCSV');
                    promise.then(function (response) {
                        var data = new Blob([response.data.Result], { type: 'text/csv;charset=ANSI' });
                        FileSaver.saveAs(data, 'template.csv');
                    });
                };

                $scope.upload = function (theFiles) {
                    var formData = new FormData();

                    $bootbox.prompt('Please enter a name for this batch.', function (response) {
                        if (!response) {
                            $bootbox.alert('Batch cannot be created without a name.');
                            return;
                        } else {
                            formData.append("BatchName", response);

                            angular.forEach(theFiles, function (file) {
                                formData.append(file.name, file);
                            });
                            var paramObj = {};
                            // start loader
                            progressLoader.start();
                            progressLoader.set(50);
                            var promise = $http.post(webApiBaseUrl + '/api/RosterRollover/uploadTeacherrollovercsv', formData, {
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
                        }
                    })
                };

                progressLoader.start();
                progressLoader.set(50);
                $scope.dataMgr.LoadTemplate().then(function (response) {
                    progressLoader.end();
                });
            }]);



})();