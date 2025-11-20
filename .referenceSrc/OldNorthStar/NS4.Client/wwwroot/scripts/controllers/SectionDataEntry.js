(function () {
    'use strict';

    angular
        .module('sectionDataEntryModule', [])
        .controller('NSStudentSectionDataEntryBaseController',
        ['$scope', '$q', '$http', 'nsPinesService', '$location', '$filter', '$routeParams', 'NSStudentSectionResultDataEntry', 'nsFilterOptionsService','nsLookupFieldService', 'nsSelect2RemoteOptions',
    function ($scope, $q, $http, nsPinesService, $location, $filter, $routeParams, NSStudentSectionResultDataEntry, nsFilterOptionsService, nsLookupFieldService, nsSelect2RemoteOptions) {
                $scope.StudentSectionResults = new NSStudentSectionResultDataEntry();
                $scope.filterOptions = nsFilterOptionsService.options;
                $scope.errors = [];

                $scope.saveAssessmentData = function (studentResult) {
                    $scope.StudentSectionResults.saveAssessmentResult($routeParams.assessmentId, studentResult, $routeParams.benchmarkDateId)
                        .then(
                            function (data) {
                                $location.path("section-assessment-resultlist/" + $routeParams.assessmentId);
                                nsPinesService.dataSavedSuccessfully();
                            }
                        );
                };
                $scope.cancel = function () {
                    $location.path("section-assessment-resultlist/" + $routeParams.assessmentId);
                }

                $scope.LoadData = function () {
                    $scope.filterOptions.selectedBenchmarkDate = (typeof $routeParams.benchmarkDateId !== 'undefined') ? nsFilterOptionsService.getBenchmarkDateById($routeParams.benchmarkDateId) : $scope.filterOptions.selectedBenchmarkDate;


                    $scope.StudentSectionResults.loadAssessmentStudentResultData($routeParams.assessmentId, $routeParams.classId, $routeParams.benchmarkDateId, $routeParams.studentId, $routeParams.studentResultId)
                            .then(
                                function (data) {
                                    //$scope.lookupFieldsArray = data.data.Assessment.LookupFields;
                                    $scope.fields = data.data.Assessment.Fields;
                                    $scope.assessment = data.data.Assessment;
                                    $scope.studentResult = data.data.StudentResult;
                                    $scope.StudentSectionResults.attachFieldsToResults($scope.studentResult, $scope.fields, nsLookupFieldService.LookupFieldsArray);


                                    if ($scope.studentResult.TestDate == null) {
                                        $scope.studentResult.TestDate = new Date();
                                    } else {
                                        var momentizedDate = moment($scope.studentResult.TestDate);
                                        $scope.studentResult.TestDate = momentizedDate.toDate();
                                    }
                                    $scope.setDisplayStructure();
                                },
                                function (msg) {
                                    alert('error loading results');
                                }
                            );
                }


                $scope.formats = ['dd-MMM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
                $scope.format = $scope.formats[0];

                // move this to a simple directive
                $scope.$on('NSHTTPError', function (event, data) {
                    $scope.errors.push({ type: "danger", msg: data });
                    $('html, body').animate({ scrollTop: 0 }, 'fast');
                });

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
        .controller('SectionStudentResultHFWDataEntryController', SectionStudentResultHFWDataEntryController)
        .controller('SectionStudentResultSpellingDataEntryController', SectionStudentResultSpellingDataEntryController)
    .controller('SectionStudentResultLetterIDDataEntryController', SectionStudentResultLetterIDDataEntryController)
        .controller('SectionStudentResultHRSIWDataEntryController', SectionStudentResultHRSIWDataEntryController)
    .controller('SectionStudentResultCAPDataEntryController', SectionStudentResultCAPDataEntryController);

    SectionDataEntryController.$inject = ['$httpParamSerializer','$scope', '$q', '$http', 'nsPinesService', '$location', '$filter', '$routeParams', 'nsSectionDataEntryService', 'nsFilterOptionsService', 'NSSectionAssessmentDataEntryManager', 'nsLookupFieldService', 'nsSelect2RemoteOptions','$bootbox'];
    SectionStudentResultHFWDataEntryController.$inject = ['$scope', '$q', '$http', 'pinesNotifications', '$location', '$filter', '$routeParams', '$bootbox'];
    SectionStudentResultSpellingDataEntryController.$inject = ['$scope', '$q', '$http', 'nsPinesService', '$location', '$filter', '$routeParams', 'NSStudentSectionResultDataEntry', 'nsFilterOptionsService','$controller', 'nsSelect2RemoteOptions'];
    SectionStudentResultLetterIDDataEntryController.$inject = ['$scope', '$q', '$http', 'nsPinesService', '$location', '$filter', '$routeParams', 'NSStudentSectionResultDataEntry', 'nsFilterOptionsService', '$controller', 'nsSelect2RemoteOptions'];
    SectionStudentResultHRSIWDataEntryController.$inject = ['$scope', '$q', '$http', 'nsPinesService', '$location', '$filter', '$routeParams', 'NSStudentSectionResultDataEntry', 'nsFilterOptionsService', '$controller', 'nsSelect2RemoteOptions'];
    SectionStudentResultCAPDataEntryController.$inject = ['$scope', '$q', '$http', 'nsPinesService', '$location', '$filter', '$routeParams', 'NSStudentSectionResultDataEntry', 'nsFilterOptionsService', '$controller', 'nsSelect2RemoteOptions'];

    function SectionStudentResultLetterIDDataEntryController($scope, $q, $http, nsPinesService, $location, $filter, $routeParams, NSStudentSectionResultDataEntry, nsFilterOptionsService, $controller, nsSelect2RemoteOptions) {

        $controller('NSStudentSectionDataEntryBaseController', { $scope: $scope });
        $scope.totalFields = [];
        $scope.commentFields = [];
        $scope.page1FieldGroups = [];
        $scope.page2FieldGroups = [];

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

        $scope.setDisplayStructure = function () {
            // create scope fields house specific totals


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
                else if ($scope.fields[f].DatabaseColumn === 'unknownLetters') {
                    $scope.commentFields.push($scope.fields[f]);
                }
                else if ($scope.fields[f].DatabaseColumn === 'comments') {
                    $scope.commentFields.push($scope.fields[f]);
                }
            }

            for (var i = 0; i < $scope.fields.length; i++) {
                var field = $scope.fields[i];
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
                        p1GroupIndex = $scope.page1FieldGroups.push({ Id: currentFieldGroupId }) - 1;
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
                        p2GroupIndex = $scope.page2FieldGroups.push({ Id: currentFieldGroupId }) - 1;
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
        $scope.rowSize = 26;
        $scope.formId = null;
        $scope.formFieldResult = {};
        $scope.isNew = $routeParams.studentResultId == "-1" ? true : false;
        $scope.checkAll = { checked: false };


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
                
            if(category.DisplayName === 'Words Spelled Correctly') {
                return 'whitebg';
            }
                
            for (var j = 0; j < $scope.studentResult.FieldResults.length; j++) {
			    if ($scope.studentResult.FieldResults[j].Field.Category && $scope.studentResult.FieldResults[j].Field.Page === 1 && $scope.studentResult.FieldResults[j].Field.Category.Id === category.Id) {
                    categoryFieldCount++;
                        
                    if($scope.studentResult.FieldResults[j].BoolValue === true) {
                            categoryFieldCheckedCount++;
                    }
			    }
			}

            if(categoryFieldCount - categoryFieldCheckedCount < 2) {
                return 'yellowbg';
            } else if(categoryFieldCount - categoryFieldCheckedCount === 2){
                return 'bluebg';
            } else { 
                return 'whitebg';
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

    function SectionDataEntryController($httpParamSerializer, $scope, $q, $http, nsPinesService, $location, $filter, $routeParams, nsSectionDataEntryService, nsFilterOptionsService, NSSectionAssessmentDataEntryManager, nsLookupFieldService, nsSelect2RemoteOptions, $bootbox) {
        $scope.sortArray = [];
        $scope.headerClassArray = [];
        $scope.staticColumnsObj = {};
        $scope.staticColumnsObj.studentNameHeaderClass = "fa";
        $scope.filterOptions = nsFilterOptionsService.options;
        //var b = $httpParamSerializer($scope.filterOptions);
        $scope.SectionResults = new NSSectionAssessmentDataEntryManager(nsLookupFieldService.LookupFieldsArray);
        $scope.getTeacherRemoteOptions = nsSelect2RemoteOptions.TeacherRemoteOptions;
        $scope.validateRecorder = function (studentResult) {
            if (angular.isDefined(studentResult) && studentResult.Recorder.id > 0) {
                return true;
            }

            return false;
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

        $scope.print = function () {
            var returnObj = {
                SchoolId: $scope.filterOptions.selectedSchool.Id,
                GradeId: $scope.filterOptions.selectedGrade.Id,
                TeacherId: $scope.filterOptions.selectedTeacher.Id,
                SectionId: $scope.filterOptions.selectedSection.Id,
                StudentId: $scope.filterOptions.selectedStudent.Id,
                SchoolYear: $scope.filterOptions.selectedSchoolYear.SchoolStartYear,
                Url: $location.absUrl()
            };

            $http.post('http://localhost:16726/api/Print/PrintPage', returnObj, {
                responseType: 'arraybuffer', headers: {
                    accept: 'application/pdf'
                },
            })
                .then(function (data) {
                    var blob = new Blob([data.data], { type: 'application/pdf' });
                    saveAs(blob, "NorthStarPrint.pdf");
                    //var fileName = "test.pdf";
                    //var a = document.createElement("a");
                    //document.body.appendChild(a);

                    
                    //var fileURL = window.URL.createObjectURL(file);
                    //a.href = fileURL;
                    //a.download = fileName;
                    //a.click();
                }, function (err) {
                    alert('error getting pdf');
                });
        }

        $scope.$watch('filterOptions.selectedSection.id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                //var b = $httpParamSerializer($scope.filterOptions);
                LoadData(); 
            }
        });
        $scope.$watch('filterOptions.selectedBenchmarkDate.id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                //var b = $httpParamSerializer($scope.filterOptions);
                LoadData();
            }
        });

        $scope.sort = function (column) {
            $scope.SectionResults.doSort(column, $scope.staticColumnsObj, $scope.fields, $scope.headerClassArray, $scope.sortArray);
        };

        //$scope.navigateToTdd = function (tddid) {
        //    if (tddid != $routeParams.benchmarkDateId) {
        //        $location.path('/section-assessment-resultlist/' + $routeParams.assessmentid + '/' + tddid);
        //    }
        //}

        $scope.navigateToTdd = function (tddid) {
            $scope.filterOptions.selectedBenchmarkDate = nsFilterOptionsService.getBenchmarkDateById(tddid);
        }

        $scope.deleteAssessmentData = function (studentResult) {

            $bootbox.confirm('Are you sure you want to delete this record?', function (response) {
                if (response) {
                    $scope.SectionResults.deleteStudentTestResult($scope.assessment.Id, studentResult)
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
            $scope.SectionResults.saveAssessmentResult($scope.assessment.Id, studentResult, $scope.filterOptions.selectedBenchmarkDate.id)
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
                $location.path(dataEntryPage + "/" + $routeParams.assessmentid + "/" + $scope.filterOptions.selectedSection.id + "/" + $scope.filterOptions.selectedBenchmarkDate.id + "/" + studentResult.StudentId + "/" + studentResult.ResultId);
            }
        };

        var LoadData = function()
        {
            // if trying to load data, but no routeparam defined, add benchmark date to URL first
            //if (typeof $routeParams.benchmarkDateId === 'undefined' && $scope.filterOptions.selectedBenchmarkDate !== null) {
            //    $scope.navigateToTdd($scope.filterOptions.selectedBenchmarkDate.id);
            //}

            //$scope.filterOptions.selectedBenchmarkDate = (typeof $routeParams.benchmarkDateId !== 'undefined') ? nsFilterOptionsService.getBenchmarkDateById($routeParams.benchmarkDateId) : $scope.filterOptions.selectedBenchmarkDate;

            if ($scope.filterOptions.selectedBenchmarkDate != null && $scope.filterOptions.selectedSection != null) {
                $scope.SectionResults.loadAssessmentResultData($routeParams.assessmentid, nsFilterOptionsService.options)
                    .then(
                        function (data) {
                            //$scope.lookupFieldsArray = data.data.Assessment.LookupFields;
                            $scope.fields = data.data.Assessment.Fields;
                            $scope.assessment = data.data.Assessment;
                            $scope.studentResults = data.data.StudentResults;
                            $scope.SectionResults.attachFieldsToResults($scope.studentResults, $scope.fields, $scope.lookupFieldsArray);
                            $scope.SectionResults.makeDatesPopupCompatible($scope.studentResults);
                        },
                        function (msg) {
                            alert('error loading results'); 
                        }
                    );
            }
        }

        // initial load
        LoadData();
        $scope.sort('StudentName');
    }

    function SectionStudentResultHFWDataEntryController($scope, $q, $http, pinesNotifications, $location, $filter, $routeParams, $bootbox) {

        $scope.settings = {
            defaultDate: new Date(),
            selectedWordRange: "1-100",
            selectedWordOrder: "Alphabetic",
            selectedSortOrder: "SortOrder"
        };
        // get lookup field values
        $http.get('http://localhost:16726/api/assessment/GetLookupFieldsForAssessment/' + $routeParams.assessmentId).success(function (lookupData) {

            $scope.lookupFieldsArray = lookupData;
            $scope.sortArray = [];
            $scope.headerClassArray = [];
            $scope.firstNameHeaderClass = "fa";
            $scope.lastNameHeaderClass = "fa";
            $scope.wordRangeOptions = ["1-100","101-200","201-300","301-400","401-500","501-600","601-700","701-800","801-900","910-1000"];            
            $scope.wordOrderOptions = ["Alphabetic", "Teaching"];
            $scope.totalFields = [];
            $scope.commentFields = [];
            $scope.LowerFieldGroups = [];
            $scope.UpperFieldGroups = [];
            $scope.start = 0;
            $scope.end = 0;
            $scope.errors = [];

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

            $scope.changeSortOrder = function (order) {

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

                    if ($scope.settings.selectedWordOrder == 'Alphabetic') {
                        if (currentFieldGroup.SortOrder <= ($scope.start + 49)) {
                            $scope.LowerFieldGroups.push(currentFieldGroup);
                        } else {
                            $scope.UpperFieldGroups.push(currentFieldGroup);
                        }
                    } else {
                        if (currentFieldGroup.AltOrder <= ($scope.start + 49)) {
                            $scope.LowerFieldGroups.push(currentFieldGroup);
                        } else {
                            $scope.UpperFieldGroups.push(currentFieldGroup);
                        }
                    }
                }

                if ($scope.settings.selectedWordOrder == 'Alphabetic') {
                    $scope.settings.selectedSortOrder = 'SortOrder';
                } else {
                    $scope.settings.selectedSortOrder = 'AltOrder';
                }
            }

            $scope.changeWordRange = function (range) {

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
                    case '1-100':
                        LoadData(1, 100);
                        break;
                    case '101-200':
                        LoadData(101, 200);
                        break;
                    case '201-300':
                        LoadData(201, 300);
                        break;
                    case '301-400':
                        LoadData(301, 400);
                        break;
                    case '401-500':
                        LoadData(401, 500);
                        break;
                    case '501-600':
                        LoadData(501, 600);
                        break;
                    case '601-700':
                        LoadData(601, 700);
                        break;
                    case '701-800':
                        LoadData(701, 800);
                        break;
                    case '801-900':
                        LoadData(801, 900);
                        break;
                    case '901-1000':
                        LoadData(901, 1000);
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
                return $http.post("http://localhost:16726/api/DataEntry/SaveHFWAssessmentResult", returnObject).then(function (data) {



                    $scope.dataSavedSuccessfully();
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
                                    if ($scope.settings.selectedWordOrder == 'Alphabetic' && currentFieldGroup.SortOrder <= $scope.start + 49) {
                                        resultSet[i].BoolValue = false;
                                        resultSet[i].DateValue = null;
                                        resultSet[i].IsModified = true;
                                    }
                                    else if ($scope.settings.selectedWordOrder == 'Teaching' && currentFieldGroup.AltOrder <= $scope.start + 49) {
                                        resultSet[i].BoolValue = false;
                                        resultSet[i].DateValue = null;
                                        resultSet[i].IsModified = true;
                                    }
                                } else if (page == 2) {
                                    if ($scope.settings.selectedWordOrder == 'Alphabetic' && currentFieldGroup.SortOrder > $scope.start + 49) {
                                        resultSet[i].BoolValue = false;
                                        resultSet[i].DateValue = null;
                                        resultSet[i].IsModified = true;
                                    }
                                    else if ($scope.settings.selectedWordOrder == 'Teaching' && currentFieldGroup.AltOrder > $scope.start + 49) {
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
                            if ($scope.settings.selectedWordOrder == 'Alphabetic' && currentFieldGroup.SortOrder <= $scope.start + 49) {
                                resultSet[i].BoolValue = true;
                                if (resultSet[i].DateValue == null) {
                                    resultSet[i].DateValue = $scope.settings.defaultDate;
                                    resultSet[i].IsModified = true;
                                }
                            }
                            else if ($scope.settings.selectedWordOrder == 'Teaching' && currentFieldGroup.AltOrder <= $scope.start + 49) {
                                resultSet[i].BoolValue = true;
                                if (resultSet[i].DateValue == null) {
                                    resultSet[i].DateValue = $scope.settings.defaultDate;
                                    resultSet[i].IsModified = true;
                                }
                            }
                        } else if (page == 2) {
                            if ($scope.settings.selectedWordOrder == 'Alphabetic' && currentFieldGroup.SortOrder > $scope.start + 49) {
                                resultSet[i].BoolValue = true;
                                if (resultSet[i].DateValue == null) {
                                    resultSet[i].DateValue = $scope.settings.defaultDate;
                                    resultSet[i].IsModified = true;
                                }
                            }
                            else if ($scope.settings.selectedWordOrder == 'Teaching' && currentFieldGroup.AltOrder > $scope.start + 49) {
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

            LoadData(1, 100);

            function LoadData(start, end)
            {
                $scope.start = start;
                $scope.end = end;

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
                    WordOrder: $scope.settings.selectedWordOrder
                }

                $http.post('http://localhost:16726/api/dataentry/GetHFWSingleAssessmentResult', paramObj).success(function (data) {
                    $scope.assessment = data.Assessment;
                    $scope.fields = data.Assessment.Fields;
                    $scope.categories = data.Assessment.FieldCategories;
                    $scope.studentResult = data.StudentResult;

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

                        if ($scope.settings.selectedWordOrder == 'Alphabetic') {
                            if (currentFieldGroup.SortOrder <= ($scope.start + 49)) {
                                $scope.LowerFieldGroups.push(currentFieldGroup);
                            } else {
                                $scope.UpperFieldGroups.push(currentFieldGroup);
                            }
                        } else {
                            if (currentFieldGroup.AltOrder <= ($scope.start + 49)) {
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
                });
            }
        });
    }


})();