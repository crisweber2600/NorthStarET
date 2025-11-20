(function () {
    'use strict';

    angular
        .module('sectionDataEntryModule', [])
        .controller('NSStudentSectionDataEntryBaseController',
        ['$scope', '$q', '$http', 'nsPinesService', '$location', '$filter', '$routeParams', 'NSStudentSectionResultDataEntry', 'nsFilterOptionsService','nsLookupFieldService', 'nsSelect2RemoteOptions','NSUserInfoService',
    function ($scope, $q, $http, nsPinesService, $location, $filter, $routeParams, NSStudentSectionResultDataEntry, nsFilterOptionsService, nsLookupFieldService, nsSelect2RemoteOptions, NSUserInfoService) {
                $scope.StudentSectionResults = new NSStudentSectionResultDataEntry();
                $scope.filterOptions = nsFilterOptionsService.options;
                //$scope.errors = [];
                $scope.isInterventionMode = (typeof $routeParams.interventionGroupId !== 'undefined');

                $scope.saveAssessmentData = function (studentResult) {
                    if ($scope.isInterventionMode) {
                        $scope.StudentSectionResults.saveProgMonResult($routeParams.assessmentId, studentResult, $routeParams.interventionGroupId)
                        .then(
                            function (data) {
                                $location.path("ig-assessment-resultlist/" + $routeParams.assessmentId);
                                nsPinesService.dataSavedSuccessfully();
                            }
                        );
                    } else {
                        $scope.StudentSectionResults.saveAssessmentResult($routeParams.assessmentId, studentResult, $routeParams.benchmarkDateId)
                            .then(
                                function (data) {
                                    if (angular.isDefined($scope.shouldClose) && !$scope.shouldClose) {
                                        // if route resultid is -1, set it in the URL
                                        if (studentResult.ResultId == -1) {
                                            $location.path($location.path().replace('-1', data.data.StudentResult.ResultId));
                                        }
                                        nsPinesService.dataSavedSuccessfully();
                                    } else {
                                        $location.path("section-assessment-resultlist/" + $routeParams.assessmentId);
                                        nsPinesService.dataSavedSuccessfully();
                                    }
                                }
                            );
                    }
                };
                $scope.cancel = function () {
                    $location.path("section-assessment-resultlist/" + $routeParams.assessmentId);
                }

                $scope.LoadData = function () {
                    // if this is progress monitoring... load that data instead
                    if ($scope.isInterventionMode) {
                        $scope.LoadProgMonData();
                        return;
                    }

                    console.time("Start load assessmentdata");
                    $scope.filterOptions.selectedBenchmarkDate = (typeof $routeParams.benchmarkDateId !== 'undefined') ? nsFilterOptionsService.getBenchmarkDateById($routeParams.benchmarkDateId) : $scope.filterOptions.selectedBenchmarkDate;


                    $scope.StudentSectionResults.loadAssessmentStudentResultData($routeParams.assessmentId, $routeParams.classId, $routeParams.benchmarkDateId, $routeParams.studentId, $routeParams.studentResultId)
                            .then(
                                function (data) {
                                    //$scope.lookupFieldsArray = data.data.Assessment.LookupFields;
                                    $scope.fields = data.data.Assessment.Fields;
                                    $scope.assessment = data.data.Assessment;
                                    $scope.studentResult = data.data.StudentResult;
                                    $scope.StudentSectionResults.attachFieldsToResults($scope.studentResult, $scope.fields, nsLookupFieldService.LookupFieldsArray);

                                    // set default recorder
                                    if ($scope.studentResult.Recorder == null || $scope.studentResult.Recorder.id == -1) {
                                        // add current user
                                        $scope.studentResult.Recorder = { id: NSUserInfoService.currentUser.Id, text: NSUserInfoService.currentUser.FullName };
                                    }


                                    if ($scope.studentResult.TestDate == null) {
                                        $scope.studentResult.TestDate = moment().format('DD-MMM-YYYY');
                                    } else {
                                        //var momentizedDate = moment($scope.studentResult.TestDate);
                                        $scope.studentResult.TestDate = moment($scope.studentResult.TestDate, "YYYY-MM-DD").format("DD-MMM-YYYY");
                                    }
                                    $scope.setDisplayStructure();
                                    console.timeEnd("Start load assessmentdata");
                                }
                                
                            );
                }

                $scope.LoadProgMonData = function () {
                    console.time("Start load assessmentdata");
                    //$scope.filterOptions.selectedBenchmarkDate = (typeof $routeParams.benchmarkDateId !== 'undefined') ? nsFilterOptionsService.getBenchmarkDateById($routeParams.benchmarkDateId) : $scope.filterOptions.selectedBenchmarkDate;


                    $scope.StudentSectionResults.loadProgMonStudentResultData($routeParams.assessmentId, $routeParams.interventionGroupId, $routeParams.studentId, $routeParams.studentResultId)
                            .then(
                                function (data) {
                                    //$scope.lookupFieldsArray = data.data.Assessment.LookupFields;
                                    $scope.fields = data.data.Assessment.Fields;
                                    $scope.assessment = data.data.Assessment;
                                    $scope.studentResult = data.data.StudentResult;
                                    $scope.StudentSectionResults.attachFieldsToResults($scope.studentResult, $scope.fields, nsLookupFieldService.LookupFieldsArray);

                                    // set default recorder
                                    if ($scope.studentResult.Recorder == null || $scope.studentResult.Recorder.id == -1) {
                                        // add current user
                                        $scope.studentResult.Recorder = { id: NSUserInfoService.currentUser.Id, text: NSUserInfoService.currentUser.FullName };
                                    }


                                    if ($scope.studentResult.TestDate == null) {
                                        $scope.studentResult.TestDate = moment().format('DD-MMM-YYYY');
                                    } else {
                                        //var momentizedDate = moment($scope.studentResult.TestDate);
                                        $scope.studentResult.TestDate = moment($scope.studentResult.TestDate, "YYYY-MM-DD").format("DD-MMM-YYYY");
                                    }
                                    $scope.setDisplayStructure();
                                    console.timeEnd("Start load assessmentdata");
                                }

                            );
                }


                $scope.formats = ['dd-MMM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
                $scope.format = $scope.formats[0];

                // move this to a simple directive
                //$scope.$on('NSHTTPError', function (event, data) {
                //    $scope.errors.push({ type: "danger", msg: data });
                //    $('html, body').animate({ scrollTop: 0 }, 'fast');
                //});

                $scope.validateRecorder = function () {
                    if (angular.isDefined($scope.studentResult) && $scope.studentResult.Recorder.id > 0) {
                        return true;
                    }

                    return false;
                }

                $scope.popup1 = {
                    opened: false
                };

                $scope.open1 = function () {
                    $scope.popup1.opened = true;
                };

                $scope.getTeacherRemoteOptions = nsSelect2RemoteOptions.TeacherRemoteOptions;

            }])
        .controller('SectionDataEntryController', SectionDataEntryController)
        .controller('SectionDataEntryKNTCController', SectionDataEntryKNTCController)
        .controller('SectionStudentResultHFWDataEntryController', SectionStudentResultHFWDataEntryController)
        .controller('SectionStudentResultSpellingDataEntryController', SectionStudentResultSpellingDataEntryController)
    .controller('SectionStudentResultLetterIDDataEntryController', SectionStudentResultLetterIDDataEntryController)
        .controller('SectionStudentResultHRSIWDataEntryController', SectionStudentResultHRSIWDataEntryController)
        .controller('SectionStudentResultHRSIW2DataEntryController', SectionStudentResultHRSIW2DataEntryController)
        .controller('SectionStudentResultHRSIW3DataEntryController', SectionStudentResultHRSIW3DataEntryController)
    .controller('SectionStudentResultCAPDataEntryController', SectionStudentResultCAPDataEntryController)
    .controller('SectionStudentResultAVMRDataEntryController', SectionStudentResultAVMRDataEntryController)
    .controller('SectionStudentResultGenericDataEntryController', SectionStudentResultGenericDataEntryController);;

    SectionDataEntryController.$inject = ['$httpParamSerializer', '$scope', '$q', '$http', 'nsPinesService', '$location', '$filter', '$routeParams', 'nsSectionDataEntryService', 'nsFilterOptionsService', 'NSSectionAssessmentDataEntryManager', 'nsLookupFieldService', 'nsSelect2RemoteOptions', '$bootbox', '$uibModal', 'progressLoader', 'NSUserInfoService', 'FileSaver', 'webApiBaseUrl', '$timeout', 'authService', 'nsGlobalSettings', 'ckEditorSettings', 'spinnerService'];
    SectionDataEntryKNTCController.$inject = ['$httpParamSerializer', '$scope', '$q', '$http', 'nsPinesService', '$location', '$filter', '$routeParams', 'nsSectionDataEntryService', 'nsFilterOptionsService', 'NSSectionAssessmentDataEntryManager', 'nsLookupFieldService', 'nsSelect2RemoteOptions', '$bootbox', '$uibModal', 'progressLoader', 'NSUserInfoService', 'FileSaver', 'webApiBaseUrl', '$timeout', 'authService', 'nsGlobalSettings', 'ckEditorSettings', 'spinnerService','NSObservationSummarySectionManager', 'NSStudentAttributeLookups'];
    SectionStudentResultHFWDataEntryController.$inject = ['$scope', '$q', '$http', 'pinesNotifications', '$location', '$filter', '$routeParams', '$bootbox', 'webApiBaseUrl', '$timeout', 'spinnerService', 'FileSaver', 'nsFilterOptionsService'];
    SectionStudentResultSpellingDataEntryController.$inject = ['$scope', '$q', '$http', 'nsPinesService', '$location', '$filter', '$routeParams', 'NSStudentSectionResultDataEntry', 'nsFilterOptionsService','$controller', 'nsSelect2RemoteOptions'];
    SectionStudentResultLetterIDDataEntryController.$inject = ['$scope', '$q', '$http', 'nsPinesService', '$location', '$filter', '$routeParams', 'NSStudentSectionResultDataEntry', 'nsFilterOptionsService', '$controller', 'nsSelect2RemoteOptions'];
    SectionStudentResultHRSIWDataEntryController.$inject = ['$scope', '$q', '$http', 'nsPinesService', '$location', '$filter', '$routeParams', 'NSStudentSectionResultDataEntry', 'nsFilterOptionsService', '$controller', 'nsSelect2RemoteOptions'];
    SectionStudentResultHRSIW2DataEntryController.$inject = ['$scope', '$q', '$http', 'nsPinesService', '$location', '$filter', '$routeParams', 'NSStudentSectionResultDataEntry', 'nsFilterOptionsService', '$controller', 'nsSelect2RemoteOptions'];
    SectionStudentResultHRSIW3DataEntryController.$inject = ['$scope', '$q', '$http', 'nsPinesService', '$location', '$filter', '$routeParams', 'NSStudentSectionResultDataEntry', 'nsFilterOptionsService', '$controller', 'nsSelect2RemoteOptions'];
    SectionStudentResultAVMRDataEntryController.$inject = ['$scope', '$q', '$http', 'nsPinesService', '$location', '$filter', '$routeParams', 'NSStudentSectionResultDataEntry', 'nsFilterOptionsService', '$controller', 'nsSelect2RemoteOptions','orderByFilter'];
    SectionStudentResultCAPDataEntryController.$inject = ['$scope', '$q', '$http', 'nsPinesService', '$location', '$filter', '$routeParams', 'NSStudentSectionResultDataEntry', 'nsFilterOptionsService', '$controller', 'nsSelect2RemoteOptions'];
    SectionStudentResultGenericDataEntryController.$inject = ['$scope', '$q', '$http', 'nsPinesService', '$location', '$filter', '$routeParams', 'NSStudentSectionResultDataEntry', 'nsFilterOptionsService', '$controller', 'nsSelect2RemoteOptions'];

    function SectionStudentResultLetterIDDataEntryController($scope, $q, $http, nsPinesService, $location, $filter, $routeParams, NSStudentSectionResultDataEntry, nsFilterOptionsService, $controller, nsSelect2RemoteOptions) {

        $controller('NSStudentSectionDataEntryBaseController', { $scope: $scope });
        $scope.totalFields = [];
        $scope.commentFields = [];
        $scope.page1FieldGroups = [];
        $scope.page2FieldGroups = [];
        $scope.fieldGroups = [];

        $scope.headerColspan = function (index) {
            if (index == 0) {
                return '1';
            } else {
                return '2';
            }
        }

        $scope.toggleCategory = function (category, page, isChecked) {
            if (isChecked) {
                for (var i = 0; i < $scope.studentResult.FieldResults.length; i++) {
                    if ($scope.studentResult.FieldResults[i].Field.CategoryId == category.Id && $scope.studentResult.FieldResults[i].Field.Page == page) {
                        $scope.studentResult.FieldResults[i].BoolValue = true;
                    }
                }
            } else {
                for (var i = 0; i < $scope.studentResult.FieldResults.length; i++) {
                    if ($scope.studentResult.FieldResults[i].Field.CategoryId == category.Id && $scope.studentResult.FieldResults[i].Field.Page == page) {
                        $scope.studentResult.FieldResults[i].BoolValue = false;
                    }
                }
            }
        }

        var getFieldGroupById = function (id) {
            for (var i = 0; i < $scope.fieldGroups.length; i++) {
                if(id == $scope.fieldGroups[i].Id){
                    return $scope.fieldGroups[i];
                }
            }

            return null;
        }

        $scope.setDisplayStructure = function () {
            // create scope fields house specific totals
            $scope.fieldGroups = $scope.assessment.FieldGroups;

            // set total fields
            for (var f = 0; f < $scope.fields.length; f++)
            {
                if($scope.fields[f].DatabaseColumn === 'totalAlphabetResponse')
                {
                    $scope.totalFields.push($scope.fields[f]);
                }
                else if ($scope.fields[f].DatabaseColumn === 'totalSoundResponse')
                {
                    $scope.totalFields.push($scope.fields[f]);
                }
                else if ($scope.fields[f].DatabaseColumn === 'totalWordResponse')
                {
                    $scope.totalFields.push($scope.fields[f]);
                }
                else if ($scope.fields[f].DatabaseColumn === 'totalOverallResponse')
                {
                    $scope.totalFields.push($scope.fields[f]);
                }
                else if ($scope.fields[f].DatabaseColumn === 'totalOverallResponseO') {
                    $scope.totalFields.push($scope.fields[f]);
                }
                else if ($scope.fields[f].DatabaseColumn === 'unknownLetters') {
                    $scope.commentFields.push($scope.fields[f]);
                }
                else if ($scope.fields[f].DatabaseColumn === 'comments') {
                    $scope.commentFields.push($scope.fields[f]);
                }
            }

            for (var i = 0; i < $scope.fields.length; i++) {
                var field = $scope.fields[i];
                var currentFieldGroup = getFieldGroupById(field.GroupId);
                var currentFieldGroupId = field.GroupId;
                var currentPage = field.Page;

                // check if this groupid is already added, if not add it
                var p1GroupIndex = 0;
                var p2GroupIndex = 0;
                if (currentPage == 1 && !isNaN(currentFieldGroupId)) {
                    var boolFoundOnP1 = false;
                    for (var j = 0; j < $scope.page1FieldGroups.length; j++) {
                        if($scope.page1FieldGroups[j].Id === currentFieldGroupId) {
                            boolFoundOnP1 = true;
                            p1GroupIndex = j;
                            break;
                        }
                    }

                    // if we didn't find it, add this group

                    if (!boolFoundOnP1) {
                        p1GroupIndex = $scope.page1FieldGroups.push(currentFieldGroup) - 1;
                        $scope.page1FieldGroups[p1GroupIndex].Fields = [];
                    }

                    // add the field to the fields array
                    $scope.page1FieldGroups[p1GroupIndex].Fields.push(angular.copy(field));
                }
                else if(currentPage == 2 && !isNaN(currentFieldGroupId)) {
                    var boolFoundOnP2 = false;
                    for (var j = 0; j < $scope.page2FieldGroups.length; j++) {
                        if ($scope.page2FieldGroups[j].Id === currentFieldGroupId) {
                            boolFoundOnP2 = true;
                            p2GroupIndex = j;
                            break;
                        }
                    }

                    // if we didn't find it, add this group
                    if (!boolFoundOnP2) {
                        p2GroupIndex = $scope.page2FieldGroups.push(currentFieldGroup) - 1;
                        $scope.page2FieldGroups[p2GroupIndex].Fields = [];
                    }

                    // add the field to the fields array
                    $scope.page2FieldGroups[p2GroupIndex].Fields.push(angular.copy(field));
                }
            }
        }
        // end spelling controller specific

        // initial load
        $scope.LoadData();
    }

    function SectionStudentResultHRSIWDataEntryController($scope, $q, $http, nsPinesService, $location, $filter, $routeParams, NSStudentSectionResultDataEntry, nsFilterOptionsService, $controller, nsSelect2RemoteOptions) {

        $controller('NSStudentSectionDataEntryBaseController', { $scope: $scope });
        $scope.totalFields = [];
        $scope.commentFields = [];
        $scope.formFields = [];
        $scope.row1Fields = [];
        $scope.row2Fields = [];
        $scope.rowSize = 30;
        $scope.formId = null;
        $scope.formFieldResult = {};
        $scope.isNew = $routeParams.studentResultId == "-1" ? true : false;
        $scope.checkAll = { checked: false };
        $scope.formSettings = {};

        $scope.toggleAll = function () {
            if ($scope.checkAll.checked) {
                for (var i = 0; i < $scope.studentResult.FieldResults.length; i++) {
                    if ($scope.studentResult.FieldResults[i].Field.Page == $scope.formId) {
                        $scope.studentResult.FieldResults[i].BoolValue = true;
                    } 
                }
            } else {
                for (var i = 0; i < $scope.studentResult.FieldResults.length; i++) {
                    if ($scope.studentResult.FieldResults[i].Field.Page == $scope.formId) {
                        $scope.studentResult.FieldResults[i].BoolValue = false;
                    }
                }
            }
        }


 
        var setFormId = function () {
            for (var i = 0; i < $scope.studentResult.FieldResults.length; i++) {
                if ($scope.studentResult.FieldResults[i].DbColumn == 'FormId') {
                    $scope.formId = $scope.studentResult.FieldResults[i].IntValue;
                    $scope.formFieldResult = $scope.studentResult.FieldResults[i];
                }
            }
        }

        $scope.headerColspan = function (index) {
            if (index == 0) {
                return '1';
            } else {
                return '2';
            }
        }

        $scope.checkDisabledForm = function () {
            var firstCheck =  ($scope.formFieldResult.IntValue != null && !$scope.isNew);
            
            // simple check
            if(firstCheck) {
                return firstCheck;
            } else {
                // otherwise loop through all checkbox fields to see if any are checked
                for(var i = 0; i < $scope.studentResult.FieldResults.length; i++) {
                    if($scope.studentResult.FieldResults[i].Field.FieldType == 'Checkbox') {
                        if($scope.studentResult.FieldResults[i].BoolValue == true) {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        $scope.setDisplayStructure = function (reload) {
            setFormId();

            // create scope fields house specific totals
            // set total fields
            $scope.row1Fields = [];
            $scope.row2Fields = [];

            for (var f = 0; f < $scope.fields.length; f++) {
                if ($scope.fields[f].DatabaseColumn === 'totalScore') {
                    if ($scope.totalFields.length == 0) {
                        $scope.totalFields.push($scope.fields[f]);
                    }
                }
                else if ($scope.fields[f].DatabaseColumn === 'comments') {
                    if ($scope.commentFields.length == 0) {
                        $scope.commentFields.push($scope.fields[f]);
                    }
                }
                else if ($scope.fields[f].DatabaseColumn === "FormId") {
                    if ($scope.formFields.length == 0) {
                        $scope.formFields.push($scope.fields[f]);
                    }
                }
            }

            var fieldsAdded = 0;
            for (var i = 0; i < $scope.fields.length; i++) {
                var field = $scope.fields[i];
                
                if (fieldsAdded < $scope.rowSize && field.Page == $scope.formId) {
                    $scope.row1Fields.push(angular.copy(field));
                    fieldsAdded++;
                } else if (field.Page == $scope.formId) {
                    $scope.row2Fields.push(angular.copy(field));
                    fieldsAdded++;
                }
            }
        }
        // end spelling controller specific

        // initial load
        $scope.LoadData();
        

        $scope.$watch('formFieldResult.IntValue', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                $scope.setDisplayStructure(true);
            }
        });
    }
    function SectionStudentResultHRSIW2DataEntryController($scope, $q, $http, nsPinesService, $location, $filter, $routeParams, NSStudentSectionResultDataEntry, nsFilterOptionsService, $controller, nsSelect2RemoteOptions) {

        $controller('NSStudentSectionDataEntryBaseController', { $scope: $scope });
        $scope.totalFields = [];
        $scope.commentFields = [];
        $scope.formFields = [];
        //$scope.row1Fields = [];
        //$scope.row2Fields = [];
        $scope.rowSize = 30;


        $scope.formId = null;
        $scope.formFieldResult = {};
        $scope.isNew = $routeParams.studentResultId == "-1" ? true : false;
        $scope.checkAll = { checked: false };
        $scope.formSettings = {};

        $scope.toggleAll = function () {
            if ($scope.checkAll.checked) {
                for (var i = 0; i < $scope.studentResult.FieldResults.length; i++) {
                    if ($scope.studentResult.FieldResults[i].Field.Page == $scope.formId &&
                        $scope.studentResult.FieldResults[i].Field.FieldType == 'Checkbox') {
                        $scope.studentResult.FieldResults[i].BoolValue = true;
                    }
                }
            } else {
                for (var i = 0; i < $scope.studentResult.FieldResults.length; i++) {
                    if ($scope.studentResult.FieldResults[i].Field.Page == $scope.formId &&
                        $scope.studentResult.FieldResults[i].Field.FieldType == 'Checkbox') {
                        $scope.studentResult.FieldResults[i].BoolValue = false;
                    }
                }
            }
        }



        var setFormId = function () {
            for (var i = 0; i < $scope.studentResult.FieldResults.length; i++) {
                if ($scope.studentResult.FieldResults[i].DbColumn == 'FormId') {
                    $scope.formId = $scope.studentResult.FieldResults[i].IntValue;
                    $scope.formFieldResult = $scope.studentResult.FieldResults[i];
                }
            }
        }

        $scope.headerColspan = function (index) {
            if (index == 0) {
                return '1';
            } else {
                return '2';
            }
        }

        $scope.checkDisabledForm = function () {
            var firstCheck = ($scope.formFieldResult.IntValue != null && !$scope.isNew);

            // simple check
            if (firstCheck) {
                return firstCheck;
            } else {
                // otherwise loop through all checkbox fields to see if any are checked
                for (var i = 0; i < $scope.studentResult.FieldResults.length; i++) {
                    if ($scope.studentResult.FieldResults[i].Field.FieldType == 'Checkbox') {
                        if ($scope.studentResult.FieldResults[i].BoolValue == true) {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        $scope.setDisplayStructure = function (reload) {
            setFormId();
                     

            // create scope fields house specific totals
            // set total fields
            $scope.rows = [];

            for (var f = 0; f < $scope.fields.length; f++) {
                if ($scope.fields[f].DatabaseColumn &&
                    $scope.fields[f].DatabaseColumn.substring(0, 10) == 'totalScore' &&
                    $scope.fields[f].Page == $scope.formId) {
                    if ($scope.totalFields.length == 0 || reload) {
                        $scope.totalFields = [];
                        $scope.totalFields.push($scope.fields[f]);
                    }
                }
                else if ($scope.fields[f].DatabaseColumn === 'comments') {
                    if ($scope.commentFields.length == 0) {
                        $scope.commentFields.push($scope.fields[f]);
                    }
                }
                else if ($scope.fields[f].DatabaseColumn === "FormId") {
                    if ($scope.formFields.length == 0) {
                        $scope.formFields.push($scope.fields[f]);
                    }
                }
            }

            var fieldsAdded = 0;
            var rowNum = 0;
            for (var i = 0; i < $scope.fields.length; i++) {
                var field = $scope.fields[i];
                
                // field.Page is used to indicate the formId that the field applies to
                // this is a hack, should be category or something, FIX IN V5
                if (fieldsAdded < $scope.rowSize && field.Page == $scope.formId &&
                    (field.FieldType == 'Checkbox' || field.FieldType == 'Label')) {
                    // if no row yet
                    if (!angular.isDefined($scope.rows[rowNum])) {
                        $scope.rows[rowNum] = [];
                        fieldsAdded = 0;
                    }
                    $scope.rows[rowNum].push(angular.copy(field));
                    fieldsAdded++;
                } else if (field.Page == $scope.formId &&
                    (field.FieldType == 'Checkbox' || field.FieldType == 'Label')) {
                    rowNum++;
                    $scope.rows[rowNum] = [];
                    $scope.rows[rowNum].push(angular.copy(field));
                    fieldsAdded = 1;
                }
            }
        }
        // end spelling controller specific

        // initial load
        $scope.LoadData();


        $scope.$watch('formFieldResult.IntValue', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                $scope.setDisplayStructure(true);
            }
        });
    }
    function SectionStudentResultHRSIW3DataEntryController($scope, $q, $http, nsPinesService, $location, $filter, $routeParams, NSStudentSectionResultDataEntry, nsFilterOptionsService, $controller, nsSelect2RemoteOptions) {

        $controller('NSStudentSectionDataEntryBaseController', { $scope: $scope });
        $scope.totalFields = [];
        $scope.commentFields = [];
        $scope.formFields = [];
        $scope.row1Fields = [];
        $scope.row2Fields = [];
        $scope.rowSize = 30;
        $scope.formId = null;
        $scope.formFieldResult = {};
        $scope.isNew = $routeParams.studentResultId == "-1" ? true : false;
        $scope.checkAll = { checked: false };
        $scope.formSettings = {};

        $scope.toggleAll = function () {
            if ($scope.checkAll.checked) {
                for (var i = 0; i < $scope.studentResult.FieldResults.length; i++) {
                       $scope.studentResult.FieldResults[i].BoolValue = true;
                }
            } else {
                for (var i = 0; i < $scope.studentResult.FieldResults.length; i++) {
                    $scope.studentResult.FieldResults[i].BoolValue = false;
                }
            }
        }

        $scope.headerColspan = function (index) {
            if (index == 0) {
                return '1';
            } else {
                return '2';
            }
        }

        $scope.checkDisabledForm = function () {
            var firstCheck = ($scope.formFieldResult.IntValue != null && !$scope.isNew);

            // simple check
            if (firstCheck) {
                return firstCheck;
            } else {
                // otherwise loop through all checkbox fields to see if any are checked
                for (var i = 0; i < $scope.studentResult.FieldResults.length; i++) {
                    if ($scope.studentResult.FieldResults[i].Field.FieldType == 'Checkbox') {
                        if ($scope.studentResult.FieldResults[i].BoolValue == true) {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        $scope.setDisplayStructure = function (reload) {
            //setFormId();

            // create scope fields house specific totals
            // set total fields
            $scope.row1Fields = [];
            $scope.row2Fields = [];
            $scope.row3Fields = [];
            $scope.row4Fields = [];
            $scope.row5Fields = [];
            $scope.row6Fields = [];
            $scope.row7Fields = [];

            for (var f = 0; f < $scope.fields.length; f++) {
                if ($scope.fields[f].DatabaseColumn && $scope.fields[f].DatabaseColumn.substring(0, 10) == 'totalScore') {
                    if ($scope.totalFields.length == 0) {
                        $scope.totalFields.push($scope.fields[f]);
                    }
                }
                else if ($scope.fields[f].DatabaseColumn === 'comments') {
                    if ($scope.commentFields.length == 0) {
                        $scope.commentFields.push($scope.fields[f]);
                    }
                }
                //else if ($scope.fields[f].DatabaseColumn === "FormId") {
                //    if ($scope.formFields.length == 0) {
                //        $scope.formFields.push($scope.fields[f]);
                //    }
                //}
            }

            for (var i = 0; i < $scope.fields.length; i++) {
                var field = $scope.fields[i];

                if (field.Page == 1) {
                    $scope.row1Fields.push(angular.copy(field));
                } else if (field.Page == 2) {
                    $scope.row2Fields.push(angular.copy(field));
                } else if (field.Page == 3) {
                    $scope.row3Fields.push(angular.copy(field));
                } else if (field.Page == 4) {
                    $scope.row4Fields.push(angular.copy(field));
                } else if (field.Page == 5) {
                    $scope.row5Fields.push(angular.copy(field));
                } else if (field.Page == 6) {
                    $scope.row6Fields.push(angular.copy(field));
                } else if (field.Page == 7) {
                    $scope.row7Fields.push(angular.copy(field));
                }
            }
        }
        // end spelling controller specific

        // initial load
        $scope.LoadData();


        //$scope.$watch('formFieldResult.IntValue', function (newValue, oldValue) {
        //    if (!angular.equals(newValue, oldValue)) {
        //        $scope.setDisplayStructure(true);
        //    }
        //});
    }
    function SectionStudentResultGenericDataEntryController($scope, $q, $http, nsPinesService, $location, $filter, $routeParams, NSStudentSectionResultDataEntry, nsFilterOptionsService, $controller, nsSelect2RemoteOptions) {
        $controller('NSStudentSectionDataEntryBaseController', { $scope: $scope });

        $scope.setDisplayStructure = function () {
            for (var k = 0; k < $scope.studentResult.FieldResults.length; k++) {
                for (var i = 0; i < $scope.assessment.Fields.length; i++) {
                    if ($scope.assessment.Fields[i].DatabaseColumn == $scope.studentResult.FieldResults[k].DbColumn) {
                        $scope.studentResult.FieldResults[k].Field = $scope.assessment.Fields[i];
                    }
                }
            }
        }


        // initial load
        $scope.LoadData();
    }

    function SectionStudentResultCAPDataEntryController($scope, $q, $http, nsPinesService, $location, $filter, $routeParams, NSStudentSectionResultDataEntry, nsFilterOptionsService, $controller, nsSelect2RemoteOptions) {
        $controller('NSStudentSectionDataEntryBaseController', { $scope: $scope });
     
        $scope.setDisplayStructure = function () {
            for (var i = 0; i < $scope.assessment.FieldGroups.length; i++) {
                $scope.assessment.FieldGroups[i].Fields = [];
                    for (var k = 0; k < $scope.fields.length; k++) {
                        if ($scope.fields[k].GroupId == $scope.assessment.FieldGroups[i].Id) {                           
                            $scope.assessment.FieldGroups[i].Fields.push(angular.copy($scope.fields[k]));
                        }
                    }
                
            }
        }

        $scope.checkAll = { checked: false };


        $scope.toggleAll = function () {
            if ($scope.checkAll.checked) {
                for (var i = 0; i < $scope.studentResult.FieldResults.length; i++) {
                    if ($scope.studentResult.FieldResults[i].Field.FieldType == 'Checkbox') {
                        $scope.studentResult.FieldResults[i].BoolValue = true;
                    }
                }
            } else {
                for (var i = 0; i < $scope.studentResult.FieldResults.length; i++) {
                    if ($scope.studentResult.FieldResults[i].Field.FieldType == 'Checkbox') {
                        $scope.studentResult.FieldResults[i].BoolValue = false;
                    }
                }
            }
        }

        // initial load
        $scope.LoadData();
    }

    function SectionStudentResultAVMRDataEntryController($scope, $q, $http, nsPinesService, $location, $filter, $routeParams, NSStudentSectionResultDataEntry, nsFilterOptionsService, $controller, nsSelect2RemoteOptions, orderByFilter) {
        $controller('NSStudentSectionDataEntryBaseController', { $scope: $scope });

        $scope.shouldClose = true;


        $scope.settings = { printMode: false, printInProgress: false };
        if ($location.absUrl().indexOf('printmode=') >= 0) {
            $scope.settings.printMode = true;
        }

        $scope.setDisplayStructure = function () {
            // structure is FieldSubCategories --> fieldgroupcontainers --> FieldGroups --> Fields

            // loop over each field, then add it to the right field group, then loop over the fieldgroups and add them to the proper container... then need to add containers to subcat
            // how to know which container belongs to which subcategory?  first add fields to the right sub category, then
            for (var i = 0; i < $scope.assessment.FieldSubCategories.length; i++) {
                $scope.assessment.FieldSubCategories[i].Fields = [];
                $scope.assessment.FieldSubCategories[i].checked = false;
                for (var k = 0; k < $scope.fields.length; k++) {
                    if ($scope.fields[k].SubcategoryId == $scope.assessment.FieldSubCategories[i].Id) {
                        $scope.assessment.FieldSubCategories[i].Fields.push(angular.copy($scope.fields[k]));
                    }
                }
            }

            // fields are now assigned to the proper subcategory (tab/task group), now add the proper container and group
            for (var i = 0; i < $scope.assessment.FieldSubCategories.length; i++) {
                $scope.assessment.FieldSubCategories[i].Containers = [];
                for (var j = 0; j < $scope.assessment.FieldSubCategories[i].Fields.length; j++) {
                    // find the group for this field
                    var currentField = $scope.assessment.FieldSubCategories[i].Fields[j];
                    var currentGroup = null;
                    for (var k = 0; k < $scope.assessment.FieldGroups.length; k++) {
                        if (currentField.GroupId == $scope.assessment.FieldGroups[k].Id) {
                            currentGroup = $scope.assessment.FieldGroups[k];
                            break;
                        }
                    }

                    // now we have the current group, get the container
                    
                    var currentContainer = null;
                    for (var m = 0; m < $scope.assessment.FieldGroupContainers.length; m++) {
                        if (currentGroup.AssessmentFieldGroupContainerId == $scope.assessment.FieldGroupContainers[m].Id) {
                            currentContainer = $scope.assessment.FieldGroupContainers[m];
                            break;
                        }
                    }

                    // now see if this container is already part of the this subcategory's containers
                    var containerRef = getArrayRef($scope.assessment.FieldSubCategories[i].Containers, currentContainer);
                    if (containerRef == null) {
                        containerRef = angular.copy(currentContainer);
                        containerRef.FieldGroups = [];
                        $scope.assessment.FieldSubCategories[i].Containers.push(containerRef);
                    }

                    // we now have the container, see if we have this group added already or not
                    var groupRef = getArrayRef(containerRef.FieldGroups, currentGroup);
                    if (groupRef == null) {
                        groupRef = angular.copy(currentGroup);
                        groupRef.FieldCell = {};
                        //groupRef.FieldCell.CategoryId = currentField.CategoryId;
                        groupRef.FieldCell.Fields = [];
                        groupRef.FieldCell.Fields.push(currentField);
                        containerRef.FieldGroups.push(groupRef);
                    } else {
                        groupRef.FieldCell.Fields.push(currentField);
                    }
                }
                // TODO: sort the containers
                $scope.assessment.FieldSubCategories[i].Containers = orderByFilter($scope.assessment.FieldSubCategories[i].Containers, 'SortOrder', false);
            }

            //var e = $scope.assessment.FieldSubCategories[2];
            //var extras = [];
            //for (var g = 0; g < e.Containers.length; g++) {
            //    var cont = e.Containers[g];
            //    for (var n = 0; n < cont.FieldGroups.length; n++) {
            //        var elGroup = cont.FieldGroups[n];
            //        if (elGroup.Id == 1560) {
            //            for (var c = 0; c < elGroup.Fields.length; c++) {
            //                if (elGroup.Fields[c].CategoryId == 105 && cont.DisplayName == 'Combinations to 5 With Materials') {
            //                    extras.push(elGroup.Fields[c]);
            //                }
            //            }
            //        }
            //    }
            //}

            //alert(extras.length);

            // add fields to the fieldgroups... or the fieldgroupgroups... make new superstructure
            //for (var i = 0; i < $scope.assessment.FieldGroups.length; i++) {
            //    $scope.assessment.FieldGroups[i].Fields = [];
            //    for (var k = 0; k < $scope.fields.length; k++) {
            //        if ($scope.fields[k].GroupId == $scope.assessment.FieldGroups[i].Id) {
            //            $scope.assessment.FieldGroups[i].Fields.push(angular.copy($scope.fields[k]));
            //        }
            //    }
            //}

            // find the categories that apply to this container
            for (var c = 0; c < $scope.assessment.FieldSubCategories.length; c++) {
                var currentSubCategory = $scope.assessment.FieldSubCategories[c];
                for (var x = 0; x < currentSubCategory.Containers.length; x++) {
                    var currentContainer = currentSubCategory.Containers[x];
                    currentContainer.Categories = []; // set new categories array

                    for (var y = 0; y < currentContainer.FieldGroups.length; y++) {
                        var currentFieldGroup = currentContainer.FieldGroups[y];
                        for (var b = 0; b < currentFieldGroup.FieldCell.Fields.length; b++) {
                            var currentField = currentFieldGroup.FieldCell.Fields[b];

                            var categoryForField = getArrayItemById($scope.assessment.FieldCategories, currentField.CategoryId);
                            var categoryRef = getArrayRef(currentContainer.Categories, categoryForField);
                            if (categoryRef == null) {
                                categoryRef = angular.copy(categoryForField);
                                currentContainer.Categories.push(categoryRef);
                            }
                            if (currentField.FieldType == 'Checkbox') {
                                categoryRef.ContainsCheckboxes = true;
                            }
                        }
                    }
                }
                // initialize categories for this container
            }

            function getArrayRef(ary, item) {
                if (ary == null || item == null) {
                    return null;
                }
                for (var i = 0; i < ary.length; i++) {
                    if (ary[i].Id == item.Id) {
                        return ary[i];
                    }
                }
                return null;
            }

            function getArrayItemById(ary, id) {
                if (ary == null || id == null) {
                    return null;
                }
                for (var i = 0; i < ary.length; i++) {
                    if (ary[i].Id == id) {
                        return ary[i];
                    }
                }
                return null;
            }
        }



        $scope.toggleAll = function (section) {
            if (section.checked) {
                for (var i = 0; i < $scope.studentResult.FieldResults.length; i++) {
                    if ($scope.studentResult.FieldResults[i].Field.FieldType == 'Checkbox' && $scope.studentResult.FieldResults[i].Field.SubcategoryId == section.Id && $scope.studentResult.FieldResults[i].Field.Flag1 == true) {
                        $scope.studentResult.FieldResults[i].BoolValue = true;
                    }
                }
            } else {
                for (var i = 0; i < $scope.studentResult.FieldResults.length; i++) {
                    if ($scope.studentResult.FieldResults[i].Field.FieldType == 'Checkbox' && $scope.studentResult.FieldResults[i].Field.SubcategoryId == section.Id && $scope.studentResult.FieldResults[i].Field.Flag1 == true) {
                        $scope.studentResult.FieldResults[i].BoolValue = false;
                    }
                }
            }
        }

        $scope.toggleContainer = function (container) {
            var fieldsInContainer = getFieldsInContainer(container);
            // need to go through all fieldgroups in the container and find the fields that match this categoryid
            if (container.checked) {
                for (var i = 0; i < $scope.studentResult.FieldResults.length; i++) {
                    if ($scope.studentResult.FieldResults[i].Field.FieldType == 'Checkbox' && isFieldInArray($scope.studentResult.FieldResults[i].Field, fieldsInContainer) && $scope.studentResult.FieldResults[i].Field.Flag1 == true) {
                        $scope.studentResult.FieldResults[i].BoolValue = true;
                    }
                }
            } else {
                for (var i = 0; i < $scope.studentResult.FieldResults.length; i++) {
                    if ($scope.studentResult.FieldResults[i].Field.FieldType == 'Checkbox' && isFieldInArray($scope.studentResult.FieldResults[i].Field, fieldsInContainer) && $scope.studentResult.FieldResults[i].Field.Flag1 == true) {
                        $scope.studentResult.FieldResults[i].BoolValue = false;
                    }
                }
            }
        }

        $scope.toggleCategory = function (category, container) {

            var fieldsInContainer = getFieldsInContainer(container);
            // need to go through all fieldgroups in the container and find the fields that match this categoryid
            if (category.checked) {
                for (var i = 0; i < $scope.studentResult.FieldResults.length; i++) {
                    if ($scope.studentResult.FieldResults[i].Field.FieldType == 'Checkbox' && $scope.studentResult.FieldResults[i].Field.CategoryId == category.Id && isFieldInArray($scope.studentResult.FieldResults[i].Field, fieldsInContainer)) {
                        $scope.studentResult.FieldResults[i].BoolValue = true;
                    }
                }
            } else {
                for (var i = 0; i < $scope.studentResult.FieldResults.length; i++) {
                    if ($scope.studentResult.FieldResults[i].Field.FieldType == 'Checkbox' && $scope.studentResult.FieldResults[i].Field.CategoryId == category.Id && isFieldInArray($scope.studentResult.FieldResults[i].Field, fieldsInContainer)) {
                        $scope.studentResult.FieldResults[i].BoolValue = false;
                    }
                }
            }
        }

        // TODO: Test this and make sure it works... also test for categoryid if we need to

        function getFieldsInContainer(container) {
            var fields = [];
            for (var i = 0; i < container.FieldGroups.length; i++) {
                for (var j = 0; j < container.FieldGroups[i].FieldCell.Fields.length; j++) {
                    fields.push(container.FieldGroups[i].FieldCell.Fields[j]);
                }
            }
            return fields;
        }

        function isFieldInArray(field, fieldArray) {
            for (var i = 0; i < fieldArray.length; i++) {
                if (field.Id == fieldArray[i].Id) {
                    return true;
                }
            }
            return false;
        }

        // initial load
        $scope.LoadData();
    }

    function SectionStudentResultSpellingDataEntryController($scope, $q, $http, nsPinesService, $location, $filter, $routeParams, NSStudentSectionResultDataEntry, nsFilterOptionsService, $controller, nsSelect2RemoteOptions) {
        $scope.wscToggle = {};
        $scope.wscToggle.toggleWordsSpelledCorrectly = false;

        $controller('NSStudentSectionDataEntryBaseController', { $scope: $scope });

        // spelling controller specific
        $scope.switchAllWordsSpelledCorrectly = function() {
            for (var j = 0; j < $scope.studentResult.FieldResults.length; j++) {
			    if ($scope.studentResult.FieldResults[j].Field.FieldType === 'Checkbox') {
                    $scope.studentResult.FieldResults[j].BoolValue = $scope.wscToggle.toggleWordsSpelledCorrectly;
			    }
			}
        }
        $scope.footerCellBgColor = function(category) {
            var categoryFieldCount = 0;
            var categoryFieldCheckedCount = 0;
                
            if(category.DisplayName === 'Words Spelled Correctly' || category.DisplayName === 'Feature Points') {
                return '';
            }
                
            for (var j = 0; j < $scope.studentResult.FieldResults.length; j++) {
			    if ($scope.studentResult.FieldResults[j].Field.CategoryId && $scope.studentResult.FieldResults[j].Field.SubcategoryId === null && $scope.studentResult.FieldResults[j].Field.CategoryId === category.Id) {
                    categoryFieldCount++;
                        
                    if($scope.studentResult.FieldResults[j].BoolValue === true) {
                            categoryFieldCheckedCount++;
                    }
			    }
			}

            if(categoryFieldCount - categoryFieldCheckedCount <= 1) {
                return 'obsGreen';
            } else if(categoryFieldCount - categoryFieldCheckedCount == 2){
                return 'obsYellow';
            } else { 
                return 'obsRed';
            }

        }
        $scope.setDisplayStructure = function () {

            for (var i = 0; i < $scope.assessment.FieldGroups.length; i++) {
                $scope.assessment.FieldGroups[i].Categories = [];
                for (var j = 0; j < $scope.assessment.FieldCategories.length; j++) {
                    $scope.assessment.FieldGroups[i].Categories.push(angular.copy($scope.assessment.FieldCategories[j]));
                    $scope.assessment.FieldGroups[i].Categories[j].Fields = [];

                    for (var k = 0; k < $scope.fields.length; k++) {
                        if ($scope.fields[k].GroupId == $scope.assessment.FieldGroups[i].Id &&
                            $scope.fields[k].CategoryId == $scope.assessment.FieldCategories[j].Id) {
                            $scope.assessment.FieldGroups[i].Categories[j].Fields.push(angular.copy($scope.fields[k]));
                        }
                    }
                }
            }
        }
        // end spelling controller specific

        // initial load
        $scope.LoadData();

    }

    function SectionDataEntryController($httpParamSerializer, $scope, $q, $http, nsPinesService, $location, $filter, $routeParams, nsSectionDataEntryService, nsFilterOptionsService, NSSectionAssessmentDataEntryManager, nsLookupFieldService, nsSelect2RemoteOptions, $bootbox, $uibModal, progressLoader, NSUserInfoService, FileSaver, webApiBaseUrl, $timeout, authService, nsGlobalSettings, ckEditorSettings, spinnerService) {
        $scope.sortArray = [];
        $scope.headerClassArray = [];
        $scope.staticColumnsObj = {};
        $scope.staticColumnsObj.studentNameHeaderClass = "fa";
        $scope.filterOptions = nsFilterOptionsService.options;
        $scope.copySettings = { copyAll: false, sourceTddId: 0, targetTddId: 0, studentId: 0, sectionId: 0 };
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

        $scope.settings = { printMode: false, printInProgress: false };

        if ($location.absUrl().indexOf('printmode=') >= 0) {
            $scope.settings.printMode = true;
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
            if ($scope.filterOptions.selectedSection == null || $scope.filterOptions.selectedBenchmarkDate == null) {
                $bootbox.alert('You must select a section and benchmark date in order to generate a template.');
                return;
            }

            var paramObj = { AssessmentId: $routeParams.assessmentId, SectionId: $scope.filterOptions.selectedSection.id, BenchmarkDateId: $scope.filterOptions.selectedBenchmarkDate.id };

            var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/GetBenchmarkExporTemplateWithData', paramObj);
            promise.then(function (response) {
                var data = new Blob([response.data.Result], { type: 'text/plain;charset=ANSI' });
                FileSaver.saveAs(data, 'export.csv');
            });
        }

        //var b = $httpParamSerializer($scope.filterOptions);
        $scope.SectionResults = new NSSectionAssessmentDataEntryManager(nsLookupFieldService.LookupFieldsArray);
        $scope.getTeacherRemoteOptions = nsSelect2RemoteOptions.TeacherRemoteOptions;
        $scope.validateRecorder = function (studentResult) {
            if (angular.isDefined(studentResult) && studentResult.Recorder.id > 0) {
                return true;
            }

            return false;
        }

        $scope.popupHeader = function () {
            if ($scope.copySettings.copyAll) {
                return 'All Student ';
            } else {
                return $scope.copySettings.targetResult.StudentName + "'s ";
            }
        }

        $scope.openImportDialog = function () {

            var modalInstance = $uibModal.open({
                templateUrl: 'importBenchmarkData.html',
                scope: $scope,
                controller: function ($scope, $uibModalInstance) {
                    $scope.theFiles = [];

                    $scope.upload = function (theFiles) {

                        var formData = new FormData();
                        formData.append("AssessmentId", $routeParams.assessmentId);
                        formData.append("BenchmarkDateId", $scope.filterOptions.selectedBenchmarkDate.id);

                        angular.forEach(theFiles, function (file) {
                            formData.append(file.name, file);
                        });
                        var paramObj = {};
                        // start loader
                        progressLoader.start();
                        progressLoader.set(50);
                        var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/UploadBenchmarkDataCSV', formData, {
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

        $scope.openCopyDialog = function (studentResult, copyAll) {
            // set settings
            $scope.copySettings.copyAll = copyAll;
            $scope.copySettings.targetResult = studentResult;

            var modalInstance = $uibModal.open({
                templateUrl: 'copyFieldTddChooser.html',
                scope: $scope,
                controller: function ($scope, $uibModalInstance) {
                    $scope.copyData = function () {

                        // validate that a date is selected and it isn't the current date
                        if (!$scope.copySettings.targetCopyDate || $scope.copySettings.targetCopyDate.id == $scope.filterOptions.selectedBenchmarkDate.id) {
                            alert("Please select a valid target benchmark date.");
                            return;
                        }

                        $uibModalInstance.dismiss('cancel');

                        progressLoader.start();
                        progressLoader.set(50);
                        if (copyAll) {
                            $scope.SectionResults.copySectionAssessmentData($routeParams.assessmentId, $scope.filterOptions.selectedBenchmarkDate, $scope.copySettings.targetCopyDate, $scope.filterOptions.selectedSection).then(function (response) {
                                nsPinesService.dataSavedSuccessfully();
                                progressLoader.end()
                            });
                        } else {
                            $scope.SectionResults.copyStudentAssessmentData($routeParams.assessmentId, $scope.filterOptions.selectedBenchmarkDate, $scope.copySettings.targetCopyDate, $scope.filterOptions.selectedSection, studentResult.StudentId).then(function (response) {
                                nsPinesService.dataSavedSuccessfully();
                                progressLoader.end()
                            });
                        }

                    };
                    $scope.cancel = function () {
                        $uibModalInstance.dismiss('cancel');
                    };
                },
                size: 'md',
            });
        }

        $scope.openCopyFromDialog = function (studentResult, copyAll) {
            // set settings
            $scope.copySettings.copyAll = copyAll;
            $scope.copySettings.targetResult = studentResult;

            var modalInstance = $uibModal.open({
                templateUrl: 'copyFromFieldTddChooser.html',
                scope: $scope,
                controller: function ($scope, $uibModalInstance) {
                    $scope.copyData = function () {

                        // validate that a date is selected and it isn't the current date
                        if (!$scope.copySettings.sourceCopyDate || $scope.copySettings.sourceCopyDate.id == $scope.filterOptions.selectedBenchmarkDate.id) {
                            alert("Please select a valid source benchmark date.");
                            return;
                        }

                        $uibModalInstance.dismiss('cancel');

                        progressLoader.start();
                        progressLoader.set(50);
                        if (copyAll) {
                            $scope.SectionResults.copyFromSectionAssessmentData($routeParams.assessmentId, $scope.filterOptions.selectedBenchmarkDate, $scope.copySettings.sourceCopyDate, $scope.filterOptions.selectedSection).then(function (response) {
                                nsPinesService.dataSavedSuccessfully();
                                LoadData();
                                progressLoader.end()
                            });
                        } else {
                            $scope.SectionResults.copyFromStudentAssessmentData($routeParams.assessmentId, $scope.filterOptions.selectedBenchmarkDate, $scope.copySettings.sourceCopyDate, $scope.filterOptions.selectedSection, studentResult.StudentId).then(function (response) {
                                nsPinesService.dataSavedSuccessfully();
                                LoadData();
                                progressLoader.end()
                            });
                        }

                    };
                    $scope.cancel = function () {
                        $uibModalInstance.dismiss('cancel');
                    };
                },
                size: 'md',
            });
        }

        $scope.before = function (rowform) {
            rowform.$setSubmitted();

            if (rowform.$valid) {
                return;
            } else return 'At least one required field is not filled out.';
        }

        $scope.formats = ['dd-MMM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
        $scope.format = $scope.formats[0];

        // move this to a simple directive
        $scope.$on('NSHTTPError', function (event, data) {
            $scope.errors.push({ type: "danger", msg: data });
            $('html, body').animate({ scrollTop: 0 }, 'fast');
        });


        $scope.popup1 = {
            opened: false
        };

        $scope.open1 = function () {
            $scope.popup1.opened = true;
        };

        $scope.$watchGroup(['filterOptions.selectedSection.id', 'filterOptions.selectedBenchmarkDate.id'], function (newValue, oldValue, scope) {
            if (angular.isDefined(newValue[0]) && angular.isDefined(newValue[1])) {
                if (newValue[0] != oldValue[0] || newValue[1] != oldValue[1]) {
                    LoadData();
                }
            }
        });
 
        $scope.sort = function (column) {
            $scope.SectionResults.doSort(column, $scope.staticColumnsObj, $scope.fields, $scope.headerClassArray, $scope.sortArray);
        };


        $scope.navigateToTdd = function (tddid) {
            $scope.filterOptions.selectedBenchmarkDate = nsFilterOptionsService.getBenchmarkDateById(tddid);
            nsFilterOptionsService.changeBenchmarkDate();
        }

        $scope.deleteAssessmentData = function (studentResult) {

            $bootbox.confirm('Are you sure you want to delete this record?', function (response) {
                if (response) {
                    $scope.SectionResults.deleteStudentTestResult($scope.assessment.Id, studentResult)
                    .then(
                        function (response) {
                            angular.extend(studentResult, response.data.StudentResult);
                            $scope.SectionResults.attachFieldsToResults($scope.studentResults, $scope.fields, nsLookupFieldService.LookupFieldsArray);
                            $scope.SectionResults.makeDatesPopupCompatible($scope.studentResults);
                            //LoadData();
                            nsPinesService.dataDeletedSuccessfully();
                        }
                    );
                }
            });

        };

        $scope.saveAssessmentData = function (studentResult) {
            $scope.SectionResults.saveAssessmentResult($scope.assessment.Id, studentResult, $scope.filterOptions.selectedBenchmarkDate.id)
                .then(
                    function (response) {
                        angular.extend(studentResult, response.data.StudentResult);
                        $scope.SectionResults.attachFieldsToResults($scope.studentResults, $scope.fields, $scope.lookupFieldsArray);
                        $scope.SectionResults.makeDatesPopupCompatible($scope.studentResults);
                        //LoadData();
                        nsPinesService.dataSavedSuccessfully();
                    }
                );
        };

        $scope.defaultEditAction = function (studentResult, rowform) {
            var dataEntryPage = $scope.assessment.DefaultDataEntryPage;
            // default editing
            if (dataEntryPage === null || dataEntryPage === '')
            {
                // set default recorder
                if (studentResult.Recorder == null || studentResult.Recorder.id == -1) {
                    // add current user
                    studentResult.Recorder = { id: NSUserInfoService.currentUser.Id, text: NSUserInfoService.currentUser.FullName };
                }
                rowform.$show();
            }
            else
            {
                $location.path(dataEntryPage + "/" + $routeParams.assessmentId + "/" + $scope.filterOptions.selectedSection.id + "/" + $scope.filterOptions.selectedBenchmarkDate.id + "/" + studentResult.StudentId + "/" + studentResult.ResultId);
            }
        };

        var LoadData = function()
        {
            console.time("Start load assessmentdata");
            console.time("Start load data");
  
            if ($scope.filterOptions.selectedBenchmarkDate != null && $scope.filterOptions.selectedSection != null) {
                $timeout(function () {
                    spinnerService.show('tableSpinner');
                });
                $scope.SectionResults.loadAssessmentResultData($routeParams.assessmentId, nsFilterOptionsService.options)
                    .then(
                        function (data) {
                            console.timeEnd("Start load data");
                            //$scope.lookupFieldsArray = data.data.Assessment.LookupFields;
                            $scope.fields = data.data.Assessment.Fields;
                            $scope.assessment = data.data.Assessment;
                            $scope.studentResults = data.data.StudentResults;
                            $scope.SectionResults.attachFieldsToResults($scope.studentResults, $scope.fields, $scope.lookupFieldsArray);
                            console.time("Start dates compatible");
                            $scope.SectionResults.makeDatesPopupCompatible($scope.studentResults);
                            console.timeEnd("Start dates compatible");

                            console.timeEnd("Start load assessmentdata");
                        }
                    )
                    .finally(function () {
                        $timeout(function () {
                            spinnerService.hide('tableSpinner');
                        });
                    });
            }
        }

        // initial load
        LoadData();
    }

    function SectionDataEntryKNTCController($httpParamSerializer, $scope, $q, $http, nsPinesService, $location, $filter, $routeParams, nsSectionDataEntryService, nsFilterOptionsService, NSSectionAssessmentDataEntryManager, nsLookupFieldService, nsSelect2RemoteOptions, $bootbox, $uibModal, progressLoader, NSUserInfoService, FileSaver, webApiBaseUrl, $timeout, authService, nsGlobalSettings, ckEditorSettings, spinnerService, NSObservationSummarySectionManager, NSStudentAttributeLookups) {
        $scope.sortArray = [];
        $scope.headerClassArray = [];
        $scope.staticColumnsObj = {};
        $scope.staticColumnsObj.studentNameHeaderClass = "fa";
        $scope.filterOptions = nsFilterOptionsService.options;
        $scope.copySettings = { copyAll: false, sourceTddId: 0, targetTddId: 0, studentId: 0, sectionId: 0 };
        $scope.uploadSettings = { LogItems: [] };
        $scope.allAttributes = new NSStudentAttributeLookups();
        $scope.sourceTddSettings = {};

        $scope.ckeditorOptions = {
            language: 'en',
            allowedContent: true,
            entities: false,
            width: 250,
            height: 600, 
            uploadUrl: ckEditorSettings.uploadurl + '?access_token=' + authService.token(),
            imageUploadUrl: ckEditorSettings.imageUploadUrl + '?access_token=' + authService.token(),
            filebrowserImageUploadUrl: ckEditorSettings.filebrowserImageUploadUrl + '?access_token=' + authService.token(),
            toolbarGroups: nsGlobalSettings.ckEditorDefaultConfig.toolbarGroups
        };

        $scope.settings = { printMode: false, printInProgress: false };
        $scope.observationSummaryManager = new NSObservationSummarySectionManager();

        var attachFieldsCallback = function () {
            for (var j = 0; j < $scope.observationSummaryManager.Scores.StudentResults.length; j++) {
                for (var k = 0; k < $scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults.length; k++) {
                    for (var i = 0; i < $scope.observationSummaryManager.Scores.Fields.length; i++) {
                        if ($scope.observationSummaryManager.Scores.Fields[i].DatabaseColumn == $scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].DbColumn) {
                            $scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].Field = angular.copy($scope.observationSummaryManager.Scores.Fields[i]);
                        }
                    }
                }
            }
        }

        $scope.guidedReadingColor = function (fieldValue) {
            switch (fieldValue) {
                case 1:
                    return 'grRed';
                case 2:
                    return 'grOrange';
                case 3:
                    return 'grYellow';
                case 4:
                    return 'grGreen';
                case 5:
                    return 'grBlue';
                case 6:
                    return 'grPurple';
                default:
                    return '';
            }
        }

        if ($location.absUrl().indexOf('printmode=') >= 0) {
            $scope.settings.printMode = true;
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
            if ($scope.filterOptions.selectedSection == null || $scope.filterOptions.selectedBenchmarkDate == null) {
                $bootbox.alert('You must select a section and benchmark date in order to generate a template.');
                return;
            }

            var paramObj = { AssessmentId: $routeParams.assessmentId, SectionId: $scope.filterOptions.selectedSection.id, BenchmarkDateId: $scope.filterOptions.selectedBenchmarkDate.id };

            var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/GetBenchmarkExporTemplateWithData', paramObj);
            promise.then(function (response) {
                var data = new Blob([response.data.Result], { type: 'text/plain;charset=ANSI' });
                FileSaver.saveAs(data, 'export.csv');
            });
        }

        //var b = $httpParamSerializer($scope.filterOptions);
        $scope.SectionResults = new NSSectionAssessmentDataEntryManager(nsLookupFieldService.LookupFieldsArray);
        $scope.getTeacherRemoteOptions = nsSelect2RemoteOptions.TeacherRemoteOptions;
        $scope.validateRecorder = function (studentResult) {
            if (angular.isDefined(studentResult) && studentResult.Recorder.id > 0) {
                return true;
            }

            return false;
        }

        $scope.popupHeader = function () {
            if ($scope.copySettings.copyAll) {
                return 'All Student ';
            } else {
                return $scope.copySettings.targetResult.StudentName + "'s ";
            }
        }

        $scope.openImportDialog = function () {

            var modalInstance = $uibModal.open({
                templateUrl: 'importBenchmarkData.html',
                scope: $scope,
                controller: function ($scope, $uibModalInstance) {
                    $scope.theFiles = [];

                    $scope.upload = function (theFiles) {

                        var formData = new FormData();
                        formData.append("AssessmentId", $routeParams.assessmentId);
                        formData.append("BenchmarkDateId", $scope.filterOptions.selectedBenchmarkDate.id);

                        angular.forEach(theFiles, function (file) {
                            formData.append(file.name, file);
                        });
                        var paramObj = {};
                        // start loader
                        progressLoader.start();
                        progressLoader.set(50);
                        var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/UploadBenchmarkDataCSV', formData, {
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

        $scope.openCopyDialog = function (studentResult, copyAll) {
            // set settings
            $scope.copySettings.copyAll = copyAll;
            $scope.copySettings.targetResult = studentResult;

            var modalInstance = $uibModal.open({
                templateUrl: 'copyFieldTddChooser.html',
                scope: $scope,
                controller: function ($scope, $uibModalInstance) {
                    $scope.copyData = function () {

                        // validate that a date is selected and it isn't the current date
                        if (!$scope.copySettings.targetCopyDate || $scope.copySettings.targetCopyDate.id == $scope.filterOptions.selectedBenchmarkDate.id) {
                            alert("Please select a valid target benchmark date.");
                            return;
                        }

                        $uibModalInstance.dismiss('cancel');

                        progressLoader.start();
                        progressLoader.set(50);
                        if (copyAll) {
                            $scope.SectionResults.copySectionAssessmentData($routeParams.assessmentId, $scope.filterOptions.selectedBenchmarkDate, $scope.copySettings.targetCopyDate, $scope.filterOptions.selectedSection).then(function (response) {
                                nsPinesService.dataSavedSuccessfully();
                                progressLoader.end()
                            });
                        } else {
                            $scope.SectionResults.copyStudentAssessmentData($routeParams.assessmentId, $scope.filterOptions.selectedBenchmarkDate, $scope.copySettings.targetCopyDate, $scope.filterOptions.selectedSection, studentResult.StudentId).then(function (response) {
                                nsPinesService.dataSavedSuccessfully();
                                progressLoader.end()
                            });
                        }

                    };
                    $scope.cancel = function () {
                        $uibModalInstance.dismiss('cancel');
                    };
                },
                size: 'md',
            });
        }

        $scope.before = function (rowform) {
            rowform.$setSubmitted();

            if (rowform.$valid) {
                return;
            } else return 'At least one required field is not filled out.';
        }

        $scope.formats = ['dd-MMM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
        $scope.format = $scope.formats[0];

        // move this to a simple directive
        $scope.$on('NSHTTPError', function (event, data) {
            $scope.errors.push({ type: "danger", msg: data });
            $('html, body').animate({ scrollTop: 0 }, 'fast');
        });


        $scope.popup1 = {
            opened: false
        };

        $scope.open1 = function () {
            $scope.popup1.opened = true;
        };

        $scope.$watchGroup(['filterOptions.selectedSection.id', 'filterOptions.selectedBenchmarkDate.id'], function (newValue, oldValue, scope) {
            if (angular.isDefined(newValue[0]) && angular.isDefined(newValue[1])) {
                if (newValue[0] != oldValue[0] || newValue[1] != oldValue[1]) {
                    LoadData();
                }
            }
        });

        $scope.sort = function (column) {
            $scope.SectionResults.doSort(column, $scope.staticColumnsObj, $scope.fields, $scope.headerClassArray, $scope.sortArray);
        };

        $scope.openSourceTddDialog = function () {
            // set settings
            //$scope.copySettings.copyAll = copyAll;
            //$scope.copySettings.targetResult = studentResult;

            var modalInstance = $uibModal.open({
                templateUrl: 'sourceTddChooser.html',
                scope: $scope,
                controller: function ($scope, $uibModalInstance) {
                    $scope.setSourceDate = function () {

                        // validate that a date is selected and it isn't the current date
                        if (!$scope.sourceTddSettings.newSourceDate) {
                            alert("Please select a valid source benchmark date.");
                            return;
                        }

                        $scope.sourceTddSettings.sourceDate = $scope.sourceTddSettings.newSourceDate;
                        $uibModalInstance.dismiss('cancel');
                        LoadData();
                    };
                    $scope.cancel = function () {
                        $uibModalInstance.dismiss('cancel');
                    };
                },
                size: 'md',
            });
        }

        $scope.navigateToTdd = function (tddid) {
            $scope.filterOptions.selectedBenchmarkDate = nsFilterOptionsService.getBenchmarkDateById(tddid);
            nsFilterOptionsService.changeBenchmarkDate();
        }

        $scope.deleteAssessmentData = function (studentResult) {

            $bootbox.confirm('Are you sure you want to delete this record?', function (response) {
                if (response) {
                    $scope.SectionResults.deleteStudentTestResult($scope.assessment.Id, studentResult)
                    .then(
                        function (response) {
                            angular.extend(studentResult, response.data.StudentResult);
                            $scope.SectionResults.attachFieldsToResults($scope.studentResults, $scope.fields, nsLookupFieldService.LookupFieldsArray);
                            $scope.SectionResults.makeDatesPopupCompatible($scope.studentResults);
                            //LoadData();
                            nsPinesService.dataDeletedSuccessfully();
                        }
                    );
                }
            });

        };

        $scope.saveAssessmentData = function (studentResult) {
            $scope.SectionResults.saveAssessmentResult($scope.assessment.Id, studentResult, $scope.filterOptions.selectedBenchmarkDate.id)
                .then(
                    function (response) {
                        angular.extend(studentResult, response.data.StudentResult);
                        $scope.SectionResults.attachFieldsToResults($scope.studentResults, $scope.fields, $scope.lookupFieldsArray);
                        $scope.SectionResults.makeDatesPopupCompatible($scope.studentResults);
                        //LoadData();
                        nsPinesService.dataSavedSuccessfully();
                    }
                );
        };

        $scope.defaultEditAction = function (studentResult, rowform) {
            var dataEntryPage = $scope.assessment.DefaultDataEntryPage;
            // default editing
            if (dataEntryPage === null || dataEntryPage === '') {
                // set default recorder
                if (studentResult.Recorder == null || studentResult.Recorder.id == -1) {
                    // add current user
                    studentResult.Recorder = { id: NSUserInfoService.currentUser.Id, text: NSUserInfoService.currentUser.FullName };
                }
                rowform.$show();
            }
            else {
                $location.path(dataEntryPage + "/" + $routeParams.assessmentId + "/" + $scope.filterOptions.selectedSection.id + "/" + $scope.filterOptions.selectedBenchmarkDate.id + "/" + studentResult.StudentId + "/" + studentResult.ResultId);
            }
        }; 

        $scope.$on('NSFieldsUpdated', function (event, data) {
            LoadData();
        });

        $scope.getAttributeLookupValue = function (studentId, attributeId) {
            var attributeIndex = -1;
            for (var p = 0; p < $scope.allAttributes.AllAttributes.length; p++) {
                if (attributeId == $scope.allAttributes.AllAttributes[p].Id) {
                    attributeIndex = p;
                    break;
                }
            }

            // if this attribute doesn't exist (fringe case)
            if (p == -1) {
                return '';
            }

            for (var i = 0; i < $scope.observationSummaryManager.StudentAttributes.length; i++) {
                if ($scope.observationSummaryManager.StudentAttributes[i].StudentId == studentId) {
                    // now have student selected
                    var currentAttribute = $scope.allAttributes.AllAttributes[attributeIndex];
                    // if student even has this attribute
                    if (typeof ($scope.observationSummaryManager.StudentAttributes[i][attributeId]) != 'undefined') {
                        var currentStudentLookupId = $scope.observationSummaryManager.StudentAttributes[i][attributeId];
                        return currentStudentLookupId;
                    }
                }
            }
            return '';
        }

        $scope.getAttributeValue = function (studentId, attributeId) {
            var attributeIndex = -1;
            for (var p = 0; p < $scope.allAttributes.AllAttributes.length; p++) {
                if (attributeId == $scope.allAttributes.AllAttributes[p].Id) {
                    attributeIndex = p;
                    break;
                }
            }

            // if this attribute doesn't exist (fringe case)
            if (p == -1) {
                return '';
            }

            for (var i = 0; i < $scope.observationSummaryManager.StudentAttributes.length; i++) {
                if ($scope.observationSummaryManager.StudentAttributes[i].StudentId == studentId) {
                    // now have student selected
                    var currentAttribute = $scope.allAttributes.AllAttributes[attributeIndex];
                    // if student even has this attribute
                    if (typeof ($scope.observationSummaryManager.StudentAttributes[i][attributeId]) != 'undefined') {
                        var currentStudentLookupId = $scope.observationSummaryManager.StudentAttributes[i][attributeId];

                        for (var m = 0; m < $scope.allAttributes.AllAttributes[attributeIndex].LookupValues.length; m++) {
                            if ($scope.allAttributes.AllAttributes[attributeIndex].LookupValues[m].LookupValueId == currentStudentLookupId) {
                                return $scope.allAttributes.AllAttributes[attributeIndex].LookupValues[m].LookupValue;
                            }
                        }
                    }
                }
            }
            return '';
        }

        var LoadData = function () {
            console.time("Start load assessmentdata");
            console.time("Start load data");

            if ($scope.filterOptions.selectedBenchmarkDate != null && $scope.filterOptions.selectedSection != null) {
                $timeout(function () {
                    spinnerService.show('tableSpinner');
                });

                // get sourcedate object from url if exists
                if ($location.$$search.SourceBenchmarkDate) {
                    $scope.sourceTddSettings.sourceDate = JSON.parse(decodeURIComponent($location.$$search.SourceBenchmarkDate));
                }

                if (!$scope.sourceTddSettings.sourceDate) {
                    $scope.sourceTddSettings.sourceDate = $scope.filterOptions.selectedBenchmarkDate; // set default source date
                }


                $scope.SectionResults.loadAssessmentResultData($routeParams.assessmentId, nsFilterOptionsService.options)
                    .then(
                        function (data) {
                            console.timeEnd("Start load data");
                            //$scope.lookupFieldsArray = data.data.Assessment.LookupFields;
                            $scope.fields = data.data.Assessment.Fields;
                            $scope.assessment = data.data.Assessment;
                            $scope.studentResults = data.data.StudentResults;
                            $scope.SectionResults.attachFieldsToResults($scope.studentResults, $scope.fields, $scope.lookupFieldsArray);
                            console.time("Start dates compatible");
                            $scope.SectionResults.makeDatesPopupCompatible($scope.studentResults);
                            console.timeEnd("Start dates compatible");

                            console.timeEnd("Start load assessmentdata");
                        }
                    )
                    .finally(function () {
                        $timeout(function () {
                            $scope.observationSummaryManager.LoadData($scope.filterOptions.selectedSection.id, $scope.sourceTddSettings.sourceDate.id).then(function (response) {
                                attachFieldsCallback();
                                spinnerService.hide('tableSpinner');
                            });
                        });
                    });
            }
        }

        // initial load
        LoadData();
    }

    function SectionStudentResultHFWDataEntryController($scope, $q, $http, pinesNotifications, $location, $filter, $routeParams, $bootbox, webApiBaseUrl, $timeout, spinnerService, FileSaver, nsFilterOptionsService) {

        $scope.settings = {
            defaultDate: new Date(),
            selectedWordRange: "1-100",
            selectedWordOrder: "Alphabetic",
            selectedSortOrder: "SortOrder"
        };

        $scope.shouldClose = true;

            $scope.sortArray = [];
            $scope.headerClassArray = [];
            $scope.firstNameHeaderClass = "fa";
            $scope.lastNameHeaderClass = "fa"; 
            $scope.wordRangeOptions = ["Kindergarten","1-100","101-200","201-300","301-400","401-500","501-600","601-700","701-800","801-900","901-1000"];            
            $scope.wordOrderOptions = [{ id: "alphabetic", text: "Alphabetic Order" }, { id: "teaching", text: "Teaching Order" }];
            $scope.totalFields = [];
            $scope.commentFields = [];
            $scope.LowerFieldGroups = [];
            $scope.UpperFieldGroups = [];
            $scope.start = 0;
            $scope.end = 0;
            $scope.isKdg = false;
            $scope.errors = [];
            $scope.shouldClose = true;

            $scope.formats = ['dd-MMM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
            $scope.format = $scope.formats[0];

            // move this to a simple directive
            $scope.$on('NSHTTPError', function (event, data) {
                $scope.errors.push({ type: "danger", msg: data });
                $('html, body').animate({ scrollTop: 0 }, 'fast');
            });

            $scope.popup1 = {
                opened: false
            };

            $scope.open1 = function () {
                $scope.popup1.opened = true;
            };

            function getSelectedItemFromCollection(id, collection) {
                for (var i = 0; i < collection.length; i++) {
                    if (collection[i].id == id) {
                        return collection[i];
                    }
                }
                return null;
            }

            $scope.changeSortOrder = function (order) {

                // save the setting back to DB
                //$scope.filterOptions.selectedWordOrder = order;
                var paramObj = { SettingName: 'hfwsortorder', SettingValue: order.id};
                $http.post(webApiBaseUrl + '/api/filteroptions/UpdateHfwSetting', paramObj);
                //$scope.filterOptions.selectedHfwSortOrder = $scope.settings.selectedWordOrder;
                //nsFilterOptionsService.changeHfwSortOrder();

                $scope.LowerFieldGroups = [];
                $scope.UpperFieldGroups = [];

                for (var n = 0; n < $scope.assessment.FieldGroups.length; n++) {
                    var currentFieldGroup = $scope.assessment.FieldGroups[n];
                    currentFieldGroup.Fields = [];

                    // get all the fields for this group
                    for (var g = 0; g < $scope.fields.length; g++) {
                        var currentField = $scope.fields[g];
                        if (currentFieldGroup.Id == currentField.GroupId) {
                            currentFieldGroup.Fields.push(currentField);
                        }
                    }

                    if ($scope.settings.selectedWordOrder.id == 'alphabetic') {
                        if (currentFieldGroup.SortOrder <= ($scope.start + 49) || $scope.isKdg) {
                            $scope.LowerFieldGroups.push(currentFieldGroup);
                        } else {
                            $scope.UpperFieldGroups.push(currentFieldGroup);
                        }
                    } else {
                        if (currentFieldGroup.AltOrder <= ($scope.start + 49) || $scope.isKdg) {
                            $scope.LowerFieldGroups.push(currentFieldGroup);
                        } else {
                            $scope.UpperFieldGroups.push(currentFieldGroup);
                        }
                    }
                }

                if ($scope.settings.selectedWordOrder.id == 'alphabetic') {
                    $scope.settings.selectedSortOrder = 'SortOrder';
                } else {
                    $scope.settings.selectedSortOrder = 'AltOrder';
                }
            }

            $scope.changeWordRange = function (range) {
                var paramObj = { SettingName: 'hfwsinglerange', SettingValue: range };
                $http.post(webApiBaseUrl + '/api/filteroptions/UpdateHfwSetting', paramObj);

                var anyModified = false;
                // check if any modified fields before switching
                for (var r = 0; r < $scope.studentResult.ReadFieldResults.length; r++) {
                    if ($scope.studentResult.ReadFieldResults[r].IsModified) {
                        anyModified = true;
                        break;
                    }
                }

                if (!anyModified) {
                    for (var r = 0; r < $scope.studentResult.WriteFieldResults.length; r++) {
                        if ($scope.studentResult.WriteFieldResults[r].IsModified) {
                            anyModified = true;
                            break;
                        }
                    }
                }

                if (anyModified) {
                    $bootbox.confirm('You have made changes to this Word Range.  Do you want to save them before continuing?', function (response) {
                        if (response) {
                            $bootbox.hideAll();
                            $scope.saveAssessmentData($scope.studentResult, true).then(function (response) {
                                loadDataCallBack($scope.settings.selectedWordRange);
                            })
                        } else {
                            loadDataCallBack($scope.settings.selectedWordRange);
                        }

                    });
                } else {
                    loadDataCallBack($scope.settings.selectedWordRange);
                }

                
            }


            var loadDataCallBack = function (range) {
                switch (range) {
                    case 'Kindergarten':
                        LoadData(1, 26, true);
                        break;
                    case '1-100':
                        LoadData(1, 100, false);
                        break;
                    case '101-200':
                        LoadData(101, 200, false);
                        break;
                    case '201-300':
                        LoadData(201, 300, false);
                        break;
                    case '301-400':
                        LoadData(301, 400, false);
                        break;
                    case '401-500':
                        LoadData(401, 500, false);
                        break;
                    case '501-600':
                        LoadData(501, 600, false);
                        break;
                    case '601-700':
                        LoadData(601, 700, false);
                        break;
                    case '701-800':
                        LoadData(701, 800, false);
                        break;
                    case '801-900':
                        LoadData(801, 900, false);
                        break;
                    case '901-1000':
                        LoadData(901, 1000, false);
                        break;
                }
            }

           $scope.cancel = function () {
               $location.path("section-assessment-resultlist/" + $routeParams.assessmentId);
           }

            $scope.saveAssessmentData = function (studentResult, inPlaceSave) {
                var assessmentId = $scope.assessment.Id;
                //var studentResult = {};
                var returnObject = {
                    StudentResult: studentResult,
                    AssessmentId: assessmentId
                }
                return $http.post(webApiBaseUrl + "/api/DataEntry/SaveHFWAssessmentResult", returnObject).then(function (data) {



                    $scope.dataSavedSuccessfully();
                    if (!$scope.shouldClose) {
                        for (var r = 0; r < $scope.studentResult.ReadFieldResults.length; r++) {
                            $scope.studentResult.ReadFieldResults[r].IsModified = false;
                        }

                        if (!$scope.anyModified) {
                            for (var r = 0; r < $scope.studentResult.WriteFieldResults.length; r++) {
                                $scope.studentResult.WriteFieldResults[r].IsModified = false;
                            }
                        }
                        return;
                    }
                    if (inPlaceSave) {
                        return;
                    }
                    $location.path("section-assessment-resultlist/" + $routeParams.assessmentId);
                    // TODO: update diplayvalue of any dropdownfromdb fields

                });

            };

            $scope.dataSavedSuccessfully = function () {
                pinesNotifications.notify({
                    title: 'Data Saved',
                    text: 'Your data was saved successfully.',
                    type: 'success'
                });
            };

            $scope.dataDeletedSuccessfully = function () {
                pinesNotifications.notify({
                    title: 'Data Deleted',
                    text: 'Your data was deleted successfully.',
                    type: 'success'
                });
            };

            $scope.toggleCategory = function (category, page, isChecked) {
                var resultSet = category.DisplayName == 'Read' ? $scope.studentResult.ReadFieldResults : $scope.studentResult.WriteFieldResults;
              
                if (!isChecked) {
                    //confirm and return if false
                    $bootbox.confirm('Un-checking this will clear all the ' + category.DisplayName + ' results.  This operation cannot be undone.  Do you want to proceed?', function(result) {
                        if(result) {
                            for (var i = 0; i < resultSet.length; i++) {
                                var currentFieldGroup = null;
                                // find the fieldGroup that matches the groupid of this result field and check its sortorder
                                for (var w = 0; w < $scope.assessment.FieldGroups.length; w++) {
                                    if ($scope.assessment.FieldGroups[w].Id === resultSet[i].Field.GroupId) {
                                        currentFieldGroup = $scope.assessment.FieldGroups[w];
                                    }
                                }

                                if (page == 1) {
                                    if ($scope.settings.selectedWordOrder.id == 'alphabetic' && (currentFieldGroup.SortOrder <= $scope.start + 49 || $scope.isKdg)) {
                                        resultSet[i].BoolValue = false;
                                        resultSet[i].DateValue = null;
                                        resultSet[i].IsModified = true;
                                    }
                                else if ($scope.settings.selectedWordOrder.id == 'teaching' && (currentFieldGroup.AltOrder <= $scope.start + 49 || $scope.isKdg)) {
                                        resultSet[i].BoolValue = false;
                                        resultSet[i].DateValue = null;
                                        resultSet[i].IsModified = true;
                                    }
                                } else if (page == 2) {
                                    if ($scope.settings.selectedWordOrder.id == 'alphabetic' && currentFieldGroup.SortOrder > $scope.start + 49) {
                                        resultSet[i].BoolValue = false;
                                        resultSet[i].DateValue = null;
                                        resultSet[i].IsModified = true;
                                    }
                                    else if ($scope.settings.selectedWordOrder.id == 'teaching' && currentFieldGroup.AltOrder > $scope.start + 49) {
                                        resultSet[i].BoolValue = false;
                                        resultSet[i].DateValue = null;
                                        resultSet[i].IsModified = true;
                                    }
                                }
                            }
                        } else {
                            // reset checked status
                            if (page == 1) {
                                category.checked1 = true;
                            } else {
                                category.checked2 = true;
                            }
                        }
                    });
                }
                else {
                    for (var i = 0; i < resultSet.length; i++) {
                        var currentFieldGroup = null;
                        // find the fieldGroup that matches the groupid of this result field and check its sortorder
                        for (var w = 0; w < $scope.assessment.FieldGroups.length; w++) {
                            if ($scope.assessment.FieldGroups[w].Id === resultSet[i].Field.GroupId) {
                                currentFieldGroup = $scope.assessment.FieldGroups[w];
                                break;
                            }
                        }

                        if (page == 1) {
                            if ($scope.settings.selectedWordOrder.id == 'alphabetic' && (currentFieldGroup.SortOrder <= $scope.start + 49 || $scope.isKdg)) {
                                resultSet[i].BoolValue = true;
                                if (resultSet[i].DateValue == null) {
                                    resultSet[i].DateValue = $scope.settings.defaultDate;
                                    resultSet[i].IsModified = true;
                                }
                            }
                            else if ($scope.settings.selectedWordOrder.id == 'teaching' && (currentFieldGroup.AltOrder <= $scope.start + 49 || $scope.isKdg)) {
                                resultSet[i].BoolValue = true;
                                if (resultSet[i].DateValue == null) {
                                    resultSet[i].DateValue = $scope.settings.defaultDate;
                                    resultSet[i].IsModified = true;
                                }
                            }
                        } else if (page == 2) {
                            if ($scope.settings.selectedWordOrder.id == 'alphabetic' && currentFieldGroup.SortOrder > $scope.start + 49) {
                                resultSet[i].BoolValue = true;
                                if (resultSet[i].DateValue == null) {
                                    resultSet[i].DateValue = $scope.settings.defaultDate;
                                    resultSet[i].IsModified = true;
                                }
                            }
                            else if ($scope.settings.selectedWordOrder.id == 'teaching' && currentFieldGroup.AltOrder > $scope.start + 49) {
                                resultSet[i].BoolValue = true;
                                if (resultSet[i].DateValue == null) {
                                    resultSet[i].DateValue = $scope.settings.defaultDate;
                                    resultSet[i].IsModified = true;
                                }
                            }
                        }
                    }
                } 
             
            }

        // get initial settings and load accordingly

            $http.get(webApiBaseUrl + '/api/filteroptions/LoadHfwSettings')
                .then(function (response) {
                    if (response.data.WordOrder != null) {
                        $scope.settings.selectedWordOrder = getSelectedItemFromCollection(response.data.WordOrder, $scope.wordOrderOptions);
                    } else {
                        $scope.settings.selectedWordOrder = $scope.wordOrderOptions[0];
                    }
                    if (response.data.WordRange != null) {
                        $scope.settings.selectedWordRange = response.data.WordRange;
                    }

                    // load URL overrides
                    if ($location.absUrl().indexOf('printmode=') >= 0) {
                        $scope.settings.selectedWordRange = $routeParams.wordRange;
                    }

                    loadDataCallBack($scope.settings.selectedWordRange);
                }
            )
            


            // get selected wordorder and range

            function LoadData(start, end, isKdg)
            {
                $timeout(function () {
                    spinnerService.show('tableSpinner');
                });

                
                $scope.start = start;
                $scope.end = end;
                $scope.isKdg = isKdg;

                $scope.totalFields = [];
                $scope.commentFields = [];
                $scope.LowerFieldGroups = [];
                $scope.UpperFieldGroups = [];

                var paramObj = {
                    AssessmentId: $routeParams.assessmentId,
                    SectionId: $routeParams.classId,
                    BenchmarkDateId: $routeParams.benchmarkDateId,
                    StudentResultId: $routeParams.studentResultId,
                    StudentId: $routeParams.studentId,
                    LowWordOrder: start,
                    HighWordOrder: end,
                    WordOrder: $scope.settings.selectedWordOrder.id,
                    IsKdg: isKdg
                }

                $http.post(webApiBaseUrl + '/api/dataentry/GetHFWSingleAssessmentResult', paramObj).then(function (response) {
                    $scope.assessment = response.data.Assessment;
                    $scope.fields = response.data.Assessment.Fields;
                    $scope.categories = response.data.Assessment.FieldCategories;
                    $scope.studentResult = response.data.StudentResult;
                    $scope.wordCount = response.data.WordCount;

                    if ($scope.settings.selectedWordOrder.id == 'alphabetic') {
                        $scope.settings.selectedSortOrder = 'SortOrder';
                    } else {
                        $scope.settings.selectedSortOrder = 'AltOrder';
                    }

                    // assign fields to proper categories
                    for (var k = 0; k < $scope.fields.length; k++) {
                        for (var i = 0; i < $scope.categories.length; i++) {
                            if ($scope.fields[k].CategoryId === $scope.categories[i].Id) {
                                $scope.fields[k].Category = $scope.categories[i];
                            }
                        }
                    }

                    for (var r = 0; r < $scope.fields.length; r++) {
                        $scope.headerClassArray[r] = 'fa';
                    }

                    // Add FieldOrder, FieldType
                    for (var k = 0; k < $scope.studentResult.ReadFieldResults.length; k++) {
                        for (var i = 0; i < $scope.fields.length; i++) {
                            if ($scope.fields[i].DatabaseColumn == $scope.studentResult.ReadFieldResults[k].DbColumn) {
                                $scope.studentResult.ReadFieldResults[k].Field = $scope.fields[i];
                            }
                        }
                    }

                    for (var k = 0; k < $scope.studentResult.WriteFieldResults.length; k++) {
                        for (var i = 0; i < $scope.fields.length; i++) {
                            if ($scope.fields[i].DatabaseColumn == $scope.studentResult.WriteFieldResults[k].DbColumn) {
                                $scope.studentResult.WriteFieldResults[k].Field = $scope.fields[i];
                            }
                        }
                    }

                    // do total fields as well?


                    // split the fieldgroups into two
                    for (var n = 0; n < $scope.assessment.FieldGroups.length; n++) {
                        var currentFieldGroup = $scope.assessment.FieldGroups[n];
                        currentFieldGroup.Fields = [];

                        // get all the fields for this group
                        for (var g = 0; g < $scope.fields.length; g++) {
                            var currentField = $scope.fields[g];
                            if (currentFieldGroup.Id == currentField.GroupId) {
                                currentFieldGroup.Fields.push(currentField);
                            }
                        }

                        if ($scope.settings.selectedWordOrder.id == 'alphabetic') {
                            if (currentFieldGroup.SortOrder <= ($scope.start + 49) || $scope.isKdg) {
                                $scope.LowerFieldGroups.push(currentFieldGroup);
                            } else {
                                $scope.UpperFieldGroups.push(currentFieldGroup);
                            }
                        } else {
                            if (currentFieldGroup.AltOrder <= ($scope.start + 49) || $scope.isKdg) {
                                $scope.LowerFieldGroups.push(currentFieldGroup);
                            } else {
                                $scope.UpperFieldGroups.push(currentFieldGroup);
                            }
                        }
                    }

                    // now that the fieldgroups are set up, add the fields for each group


                    for (var f = 0; f < $scope.fields.length; f++) {
                        if ($scope.fields[f].DatabaseColumn === 'readScore') {
                            $scope.totalFields.push($scope.fields[f]);
                        }
                        else if ($scope.fields[f].DatabaseColumn === 'writeScore') {
                            $scope.totalFields.push($scope.fields[f]);
                        }
                        else if ($scope.fields[f].DatabaseColumn === 'totalScore') {
                            $scope.totalFields.push($scope.fields[f]);
                        }
                        else if ($scope.fields[f].DatabaseColumn === 'comments') {
                            $scope.commentFields.push($scope.fields[f]);
                        }
                    }
                })
                .finally(function () {
                        spinnerService.hide('tableSpinner');
                });
            }
        //});
    }


})();