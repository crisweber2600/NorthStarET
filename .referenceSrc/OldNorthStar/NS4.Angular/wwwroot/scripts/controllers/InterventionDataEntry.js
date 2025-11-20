(function () {
    'use strict';

    angular
        .module('interventionGroupDataEntryModule', [])

        .controller('IGDataEntryController', IGDataEntryController)
        .factory('NSInterventionGroupAssessmentDataEntryManager', [
        '$http', 'webApiBaseUrl', 'nsLookupFieldService', function ($http, webApiBaseUrl, nsLookupFieldService) {
            var self = this;


            var NSInterventionGroupAssessmentDataEntryManager = function (lookupFieldsArray) {
                self.LookupFieldsArray = lookupFieldsArray;

                this.initialize = function () {

                }

                this.loadAssessmentResultData = function (assessmentId, nsFilterOptionsService) {
                    var postObject = { AssessmentId: assessmentId, InterventionGroupId: nsFilterOptionsService.selectedInterventionGroup.id, StudentId: nsFilterOptionsService.selectedInterventionStudent.id };
                    var url = webApiBaseUrl + '/api/interventiongroupdataentry/GetAssessmentResults';
                    return $http.post(url, postObject);
                }

                this.makeDatesPopupCompatible = function (studentResultsArray) {
                    for (var j = 0; j < studentResultsArray.length; j++) {
                        var result = studentResultsArray[j];

                        if (result.TestDate.indexOf('T') < 0) {
                            result.TestDate = moment().format('DD-MMM-YYYY');
                        } else {
                            result.TestDate = moment(result.TestDate.substring(0, result.TestDate.indexOf('T'))).format('DD-MMM-YYYY');
                        }
                    }
                }

                this.attachFieldsToResults = function (studentResultsArray, fieldsArray) {
                    console.time("Start attach fields");
                    for (var j = 0; j < studentResultsArray.length; j++) {
                        for (var k = 0; k < studentResultsArray[j].FieldResults.length; k++) {
                            for (var r = 0; r < fieldsArray.length; r++) {
                                if (fieldsArray[r].DatabaseColumn == studentResultsArray[j].FieldResults[k].DbColumn) {
                                    studentResultsArray[j].FieldResults[k].Field = angular.copy(fieldsArray[r]);

                                    // set display value
                                    if (fieldsArray[r].FieldType === "DropdownFromDB") {
                                        for (var p = 0; p < self.LookupFieldsArray.length; p++) {
                                            if (self.LookupFieldsArray[p].LookupColumnName === fieldsArray[r].LookupFieldName) {
                                                // now find the specifc value that matches
                                                for (var y = 0; y < self.LookupFieldsArray[p].LookupFields.length; y++) {
                                                    if (studentResultsArray[j].FieldResults[k].IntValue === self.LookupFieldsArray[p].LookupFields[y].FieldSpecificId) {
                                                        studentResultsArray[j].FieldResults[k].DisplayValue = self.LookupFieldsArray[p].LookupFields[y].FieldValue;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    console.timeEnd("Start attach fields");
                    // set initial display values
                };

                this.initializeHeaderClassArray = function (fields, headerClassArray) {
                    for (var r = 0; r < fields.length; r++) {
                        headerClassArray[r] = 'fa';
                    }
                }

                this.doSort = function (column, staticColumnsObj, fields, headerClassArray, sortArray) {
                    var columnIndex = -1;
                    // if this is not a first or lastname column
                    if (!isNaN(parseInt(column))) {
                        columnIndex = column;
                        switch (fields[column].FieldType) {
                            case 'DateCheckbox':
                                column = 'FieldResults[' + column + '].DateValue';
                                break;
                            case 'Textfield':
                                column = 'FieldResults[' + column + '].StringValue';
                                break;
                            case 'DecimalRange':
                                column = 'FieldResults[' + column + '].DecimalValue';
                                break;
                            case 'DropdownRange':
                                column = 'FieldResults[' + column + '].IntValue';
                                break;
                            case 'DropdownFromDB':
                                column = 'FieldResults[' + column + '].IntValue';
                                break;
                            case 'CalculatedFieldDbOnly':
                                column = 'FieldResults[' + column + '].StringValue';
                                break;
                            case 'CalculatedFieldDbBacked':
                                column = 'FieldResults[' + column + '].IntValue';
                                break;
                            case 'CalculatedFieldDbBackedString':
                                column = 'FieldResults[' + column + '].StringValue';
                                break;
                            case 'CalculatedFieldClientOnly':
                                column = 'FieldResults[' + column + '].StringValue';//shouldnt even be used in sorting
                                break;
                            default:
                                column = 'FieldResults[' + column + '].IntValue';
                                break;
                        }
                    }


                    var bFound = false;
                    for (var j = 0; j < sortArray.length; j++) {
                        // if it is already on the list, reverse the sort
                        if (sortArray[j].indexOf(column) >= 0) {
                            bFound = true;

                            // is it already negative? if so, remove it
                            if (sortArray[j].indexOf("-") === 0) {
                                if (columnIndex > -1) {
                                    headerClassArray[columnIndex] = "fa";
                                }
                                else if (column === 'StudentName') {
                                    staticColumnsObj.studentNameHeaderClass = "fa";
                                }
                                sortArray.splice(j, 1);
                            } else {
                                if (columnIndex > -1) {
                                    headerClassArray[columnIndex] = "fa fa-chevron-down";
                                }
                                else if (column === 'StudentName') {
                                    staticColumnsObj.studentNameHeaderClass = "fa fa-chevron-down";
                                }
                                sortArray[j] = "-" + sortArray[j];
                            }
                            break;
                        }
                    }
                    if (!bFound) {
                        sortArray.push(column);

                        if (columnIndex > -1) {
                            headerClassArray[columnIndex] = "fa fa-chevron-up";
                        }
                        else if (column === 'StudentName') {
                            staticColumnsObj.studentNameHeaderClass = "fa fa-chevron-up";
                        }
                    }
                };

                this.saveAssessmentResult = function (assessmentId, studentResult) {

                    var returnObject = {
                        StudentResult: studentResult,
                        AssessmentId: assessmentId
                    }

                    return $http.post(webApiBaseUrl + "/api/interventiongroupdataentry/SaveAssessmentResult", returnObject);
                };

                this.deleteStudentTestResult = function (assessmentId, studentResult) {

                    var returnObject = {
                        StudentResult: studentResult,
                        AssessmentId: assessmentId
                    }

                    return $http.post(webApiBaseUrl + "/api/interventiongroupdataentry/DeleteAssessmentResult", returnObject);
                };

                this.cleanupAfterDelete = function () {
                    // TODO: update local model, delete all field values
                    // loop over each field and delete the values... also delete anything else relevant
                    for (var k = 0; k < studentResult.FieldResults.length; k++) {
                        studentResult.FieldResults[k].IntValue = null;
                        studentResult.FieldResults[k].DecimalValue = null;
                        studentResult.FieldResults[k].DateValue = null;
                        studentResult.FieldResults[k].StringValue = null;
                        studentResult.FieldResults[k].DisplayValue = null;
                        studentResult.FieldResults[k].BoolValue = null;
                    }
                };

                this.initialize();

            };

            return (NSInterventionGroupAssessmentDataEntryManager);
        }]);

    IGDataEntryController.$inject = ['$httpParamSerializer', '$scope', '$q', '$http', 'nsPinesService', '$location', '$filter', '$routeParams', 'nsSectionDataEntryService', 'nsFilterOptionsService', 'NSInterventionGroupAssessmentDataEntryManager', 'nsLookupFieldService', 'nsSelect2RemoteOptions', '$bootbox', '$uibModal', 'progressLoader', 'spinnerService', 'FileSaver', 'webApiBaseUrl', '$timeout', 'authService', 'nsGlobalSettings', 'ckEditorSettings' ];

    function IGDataEntryController($httpParamSerializer, $scope, $q, $http, nsPinesService, $location, $filter, $routeParams, nsSectionDataEntryService, nsFilterOptionsService, NSInterventionGroupAssessmentDataEntryManager, nsLookupFieldService, nsSelect2RemoteOptions, $bootbox, $uibModal, progressLoader, spinnerService, FileSaver, webApiBaseUrl, $timeout, authService, nsGlobalSettings, ckEditorSettings) {
        $scope.sortArray = [];
        $scope.errors = [];
        $scope.headerClassArray = [];
        $scope.staticColumnsObj = {};
        $scope.staticColumnsObj.studentNameHeaderClass = "fa";
        $scope.filterOptions = nsFilterOptionsService.options;
        $scope.uploadSettings = { LogItems: [] };
        $scope.ckeditorOptions = {
            language: 'en',
            allowedContent: true,
            entities: false,
            width: 450,
            height: 90,
            uploadUrl: ckEditorSettings.uploadurl + '?access_token=' + authService.token(),
            imageUploadUrl: ckEditorSettings.imageUploadUrl + '?access_token=' + authService.token(),
            filebrowserImageUploadUrl: ckEditorSettings.filebrowserImageUploadUrl + '?access_token=' + authService.token(),
            toolbarGroups: nsGlobalSettings.ckEditorDefaultConfig.toolbarGroups 
        };
        $scope.GroupResults = new NSInterventionGroupAssessmentDataEntryManager(nsLookupFieldService.LookupFieldsArray);
        $scope.getTeacherRemoteOptions = nsSelect2RemoteOptions.TeacherRemoteOptions;
        $scope.validateRecorder = function (studentResult) {
            if (angular.isDefined(studentResult) && studentResult.Recorder.id > 0) {
                return true;
            }

            return false;
        }


        $scope.downloadResult = function () {
            var text = '';

            for (var i = 0; i < $scope.uploadSettings.LogItems.length; i++) {
                text += $scope.uploadSettings.LogItems[i] + '\r\n';
            }

            var data = new Blob([text], { type: 'text/plain;charset=ANSI' });
            FileSaver.saveAs(data, 'results.txt');
            $scope.uploadSettings.uploadComplete = false;
        }

        $scope.getTemplate = function () {
            if ($scope.filterOptions.selectedInterventionStudent == null) {
                $bootbox.alert('You must select a student in order to generate a template.');
                return;
            }

            var paramObj = { AssessmentId: $routeParams.assessmentId, InterventionGroupId: $scope.filterOptions.selectedInterventionGroup.id, StudentId: $scope.filterOptions.selectedInterventionStudent.id };

            var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/GetInterventionExporTemplateWithData', paramObj);
            promise.then(function (response) {
                var data = new Blob([response.data.Result], { type: 'text/plain;charset=ANSI' });
                FileSaver.saveAs(data, 'export.csv');
            });
        }

        //$scope.openStudentDashboardDialog = function (studentId, studentName) {

        //    var modalInstance = $uibModal.open({
        //        templateUrl: 'studentDashboardViewer.html',
        //        scope: $scope,
        //        controller: function ($scope, $uibModalInstance) {
        //            $scope.settings = { selectedStudent: { id: studentId, text: studentName } };
        //            $scope.cancel = function () {
        //                $uibModalInstance.dismiss('cancel');
        //            };
        //        },
        //        size: 'md',
        //    });
        //}

        $scope.openImportDialog = function () {

            var modalInstance = $uibModal.open({
                templateUrl: 'importBenchmarkData.html',
                scope: $scope,
                controller: function ($scope, $uibModalInstance) {
                    $scope.theFiles = [];

                    $scope.upload = function (theFiles) {

                        var formData = new FormData();
                        formData.append("AssessmentId", $routeParams.assessmentId);
                        formData.append("InterventionGroupId", $scope.filterOptions.selectedInterventionGroup.id);
                        formData.append("StudentId", $scope.filterOptions.selectedInterventionStudent.id);

                        angular.forEach(theFiles, function (file) {
                            formData.append(file.name, file);
                        });
                        var paramObj = {};
                        // start loader
                        progressLoader.start();
                        progressLoader.set(50);
                        var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/UploadInterventionDataCSV', formData, {
                            transformRequest: angular.identity,
                            headers: { 'Content-Type': undefined }
                        }).then(function (response) {
                            // end loader
                            progressLoader.end();
                            $scope.errors = [];
                            $scope.uploadSettings.LogItems = response.data.LogItems;
                            // show success
                            $('#formReset').click();
                            $scope.uploadSettings.hasFiles = false;
                            //$scope.theFiles.length = 0;
                            //$scope.settings.hasFiles = false;
                            $scope.uploadSettings.uploadComplete = true;

                            LoadData();
                            nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
                        },
                        function (err) {
                            progressLoader.end();
                            $('#formReset').click();
                            $scope.uploadSettings.hasFiles = false;
                        });

                        $uibModalInstance.dismiss('cancel');
                    };
                    $scope.cancel = function () {
                        $uibModalInstance.dismiss('cancel');
                    };
                },
                size: 'md',
            });
        }

        $scope.addNewRow = function () {
            // if this assessment has a defaultdataentrypage, go there instead
            var dataEntryPage = $scope.assessment.DefaultDataEntryPage;
            // default editing
            if (dataEntryPage !== null && dataEntryPage !== '') {
                $location.path(dataEntryPage + "/" + $routeParams.assessmentId + "/" + $scope.filterOptions.selectedInterventionGroup.id + "/" + $scope.filterOptions.selectedInterventionStudent.id + "/-1");
                return;
            }


            for (var i = 0; i < $scope.studentResults.length; i++) {
                if ($scope.studentResults[i].ResultId == -1) {
                    alert('Only one new record can be added a time.');
                    return;
                }
            }
            var newResult = {};
            newResult.StudentId = $scope.filterOptions.selectedInterventionStudent.id;
            newResult.StudentName = $scope.filterOptions.selectedInterventionStudent.text;
            newResult.ResultId = -1;
            newResult.TestDate = new moment().format('DD-MMM-YYYY');
            newResult.Recorder = { id: $scope.filterOptions.selectedInterventionist.id, text: $scope.filterOptions.selectedInterventionist.text }; // TODO: eventually switch this to use the CURRENTUSER service
            newResult.ClassId = $scope.filterOptions.selectedInterventionGroup.id;
            newResult.StaffId = $scope.filterOptions.selectedInterventionist.id;  // don't think we even need this
            newResult.FieldResults = [];

            // now turn fields into fieldResults
            for (var i = 0; i < $scope.assessment.Fields.length; i++) {
                var field = $scope.assessment.Fields[i];

                if (field.DatabaseColumn == null || field.DatabaseColumn == '') {
                    continue;
                } else {
                    newResult.FieldResults.push({
                        StringValue: null,
                        IntValue: null,
                        DecimalValue: null,
                        BoolValue: null,
                        DbColumn: field.DatabaseColumn,
                        IsModified: true,
                        FieldIndex: 0, // not need anymore i think
                        Field: angular.copy(field)
                    });
                }
            }

            // now need some way to put form in edit mode
            $scope.addNewResult = newResult;
           // $scope.addNewForm.$visible = true;
            $scope.studentResults.push($scope.addNewResult);
        }

        $scope.before = function (rowform) {
            rowform.$setSubmitted();

            if (rowform.$valid) {
                return;
            } else return 'At least one required field is not filled out.';
        }

        $scope.formats = ['dd-MMM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
        $scope.format = $scope.formats[0];


        $scope.cancelEdit = function (studentResult) {
            if (studentResult.ResultId == -1) {
                // delete record
                for (var i = 0; i < $scope.studentResults.length; i++) {
                    if ($scope.studentResults[i].ResultId == -1) {
                        $scope.studentResults.splice(i, 1);
                        break;
                    }
                }
            } else {
                studentResult.rowform.$cancel();
            }
        }

        $scope.popup1 = {
            opened: false
        };

        $scope.open1 = function () {
            $scope.popup1.opened = true;
        };

        //studentResults

        $scope.$watch('filterOptions.selectedInterventionStudent.id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                LoadData(); 
            }
        });

        //$scope.$watch('addNewResult', function (newValue, oldValue) {
        //    if (angular.isDefined($scope.addNewForm) && $scope.addNewForm != null) {
        //        $scope.addNewForm.$visible = true;
        //    }
        //});


        $scope.sort = function (column) {
            $scope.GroupResults.doSort(column, $scope.staticColumnsObj, $scope.fields, $scope.headerClassArray, $scope.sortArray);
        };



        $scope.deleteAssessmentData = function (studentResult) {

            $bootbox.confirm('Are you sure you want to delete this record?', function (response) {
                if (response) {
                    $scope.GroupResults.deleteStudentTestResult($scope.assessment.Id, studentResult)
                    .then(
                        function (data) {
                            LoadData();
                            nsPinesService.dataDeletedSuccessfully();
                        }
                    );
                }
            });

        };

        $scope.saveAssessmentData = function (studentResult) {
            $scope.GroupResults.saveAssessmentResult($scope.assessment.Id, studentResult)
                .then(
                    function (data) {
                        LoadData();
                        nsPinesService.dataSavedSuccessfully();
                    }
                );
        };

        $scope.defaultEditAction = function (studentResult, rowform) {
            var dataEntryPage = $scope.assessment.DefaultDataEntryPage;
            // default editing
            if (dataEntryPage === null || dataEntryPage === '')
            {
                rowform.$show();
            }
            else
            {
                $location.path(dataEntryPage + "/" + $routeParams.assessmentId + "/" + $scope.filterOptions.selectedInterventionGroup.id + "/" + studentResult.StudentId + "/" + studentResult.ResultId);
            }
        };

        var LoadData = function()
        {

            if ($scope.filterOptions.selectedInterventionStudent != null) {
                $timeout(function () {
                    spinnerService.show('tableSpinner');
                });
                $scope.GroupResults.loadAssessmentResultData($routeParams.assessmentId, nsFilterOptionsService.options)
                    .then(
                        function (data) {
                            //$scope.lookupFieldsArray = data.data.Assessment.LookupFields;
                            $scope.fields = data.data.Assessment.Fields;
                            $scope.assessment = data.data.Assessment;
                            $scope.studentResults = data.data.StudentResults;
                            $scope.GroupResults.attachFieldsToResults($scope.studentResults, $scope.fields, $scope.lookupFieldsArray);
                            $scope.GroupResults.makeDatesPopupCompatible($scope.studentResults);
                        }
                    )
                    .finally(function () {
                        $timeout(function () {
                            spinnerService.hide('tableSpinner');
                        });
                    });;
            }
        }

        // initial load
        LoadData();
    }

    


})();