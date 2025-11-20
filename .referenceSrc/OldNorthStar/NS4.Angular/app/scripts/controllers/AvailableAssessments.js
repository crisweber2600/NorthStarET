(function () {
    'use strict';
     
    angular
        .module('availableAssessmentsModule', [])
        .controller('DistrictAssessmentsController', DistrictAssessmentsController)
        .controller('SchoolAssessmentsController', SchoolAssessmentsController)
        .controller('PersonalAssessmentsController', PersonalAssessmentsController)
        .service('observationSummaryAssessmentFieldChooserSvc', ['$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
            var self = this;
            self.BenchmarkAssessments = [];
            self.StateTests = [];

            self.updateAssessments = function (assessments) {
                var paramObj = {
                    AssessmentsOrFields: assessments,
                    OSSchoolColumnVisible: self.OSSchoolVisible,
                    OSGradeColumnVisible: self.OSGradeVisible,
                    OSTeacherColumnVisible: self.OSTeacherVisible
                };
                var promise = $http.post(webApiBaseUrl + '/api/assessmentavailability/UpdateObservationSummaryAssessmentVisibility', paramObj);
                return promise;
            }

            self.hideField = function (field) {
                var fields = [];
                fields.push({ id: field.Id, text: '', Visible: false});
                var paramObj = { AssessmentsOrFields: fields };

                var promise = $http.post(webApiBaseUrl + '/api/assessmentavailability/UpdateObservationSummaryAssessmentFieldVisibility', paramObj);
                return promise;
            }

            self.hideAssessment = function (assessment) {
                var fields = [];
                fields.push({ id: assessment.AssessmentId, text: '', Visible: false });
                var paramObj = { AssessmentsOrFields: fields };

                return $http.post(webApiBaseUrl + '/api/assessmentavailability/UpdateObservationSummaryAssessmentVisibility', paramObj).then(function(response) {
                    self.initialize(); // reload fields
                });
            }

            self.hideOSColumn = function (columnName) {
                var paramObj = { value: columnName };

                return $http.post(webApiBaseUrl + '/api/assessmentavailability/UpdateObservationSummaryColumnVisibility', paramObj).then(function (response) {
                    self.initialize(); // reload fields
                });
                }

            self.updateFields = function (fields) {
                var paramObj = { AssessmentsOrFields: fields };
                var promise = $http.post(webApiBaseUrl + '/api/assessmentavailability/UpdateObservationSummaryAssessmentFieldVisibility', paramObj);
                return promise;
            }

            // get list of assessments
            self.initialize = function () {
                var promise = $http.get(webApiBaseUrl + '/api/assessmentavailability/GetObservationSummaryAssessmentList')

                return promise.then(function (response) {
                    angular.extend(self, response.data);
                });
            };

            self.getAssessmentFields = function (assessmentId) {
                var paramObj = {Id: assessmentId};
                var promise = $http.post(webApiBaseUrl + '/api/assessmentavailability/GetObservationSummaryAssessmentFieldList', paramObj);
                return promise;
            }

            self.selectedAssessments = function () {
                var selected = '';

                for (var i = 0; i < self.BenchmarkAssessments.length; i++) {
                    if (self.BenchmarkAssessments[i].Visible) {
                        selected += self.BenchmarkAssessments[i].id + ',';
                    }
                }

                for (var i = 0; i < self.StateTests.length; i++) {
                    if (self.StateTests[i].Visible) {
                        selected += self.StateTests[i].id + ',';
                    }
                }

                if(selected.length > 0) // TODO: which it always will be if we make sure at least one assessment is selected
                {
                    selected = selected.substring(0, selected.length - 1);
                }

                return selected;
            }

            self.initialize();
        }])
        .directive('observationSummaryFieldChooser', ['observationSummaryAssessmentFieldChooserSvc', '$uibModal', 'nsPinesService', 'progressLoader', '$rootScope',
            function (observationSummaryAssessmentFieldChooserSvc, $uibModal, nsPinesService, progressLoader, $rootScope) {

                return {
                    restrict: 'E',
                    templateUrl: 'templates/observation-summary-field-chooser.html',
                    link: function (scope, element, attr) {
                        scope.assessmentService = observationSummaryAssessmentFieldChooserSvc;
                        scope.modifiedAssessments = [];
                        scope.modifiedFields = [];
                        scope.modifiedColumns = [];
                        scope.settings = { menuOpen: false };

                        scope.updateSelectedColumn = function (column) {
                            scope.modifiedColumns.push(column);
                        }

                        scope.updateSelectedAssessment = function (assessment) {
                            scope.modifiedAssessments.push(assessment);
                        }
                        scope.updateSelectedField = function (field) {
                            scope.modifiedFields.push(field);
                        }

                        scope.openAssessmentChooser = function () {
                            var modalInstance = $uibModal.open({
                                templateUrl: 'assessmentChooser.html',
                                scope: scope,
                                controller: function ($scope, $uibModalInstance) {
                                    
                                    $scope.refreshAssessments = function () {
                                        progressLoader.start();
                                        progressLoader.set(50);
                                        $scope.assessmentService.updateAssessments($scope.modifiedAssessments).then(function (response) {
                                            $rootScope.$broadcast('NSFieldsUpdated', true);
                                            nsPinesService.dataSavedSuccessfully();
                                            progressLoader.end()
                                            $scope.modifiedAssessments = [];
                                            $scope.settings.menuOpen = false;
                                        });
                                        $uibModalInstance.dismiss('cancel');
                                    }

                                    $scope.openFieldsPopup = function (assessment) {

                                        $scope.selectedAssessment = assessment;

                                        // get fields
                                        scope.assessmentService.getAssessmentFields(assessment.id).then(function (response) {
                                            $scope.selectedFields = response.data.Fields;

                                            var modalInstance = $uibModal.open({
                                                templateUrl: 'assessmentFieldViewer.html',
                                                scope: $scope,
                                                controller: function ($scope, $uibModalInstance) {
                                                    $scope.saveFields = function () {
                                                        $uibModalInstance.dismiss('cancel');

                                                        progressLoader.start();
                                                        progressLoader.set(50);
                                                        $scope.assessmentService.updateFields($scope.modifiedFields).then(function (response) {
                                                            $rootScope.$broadcast('NSFieldsUpdated', true);
                                                            nsPinesService.dataSavedSuccessfully();
                                                            progressLoader.end()
                                                            $scope.modifiedFields = [];
                                                            $scope.settings.menuOpen = false;
                                                        });
                                                    };
                                                    $scope.cancel = function () {
                                                        $uibModalInstance.dismiss('cancel');
                                                    };
                                                },
                                                size: 'lg',
                                            });
                                        });
                                    }

                                    $scope.cancel = function () {
                                        $uibModalInstance.dismiss('cancel');
                                    };
                                },
                                size: 'md',
                            });
                        }

                       
                    }
                }
                
            }])
        .service('stackedBarGraphSummaryAttributeChooserSvc', ['$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
            var self = this;
            self.Attributes = [];

            self.updateAttributes = function (attributes) {
                var paramObj = {
                    Attributes: attributes
                };
                var promise = $http.post(webApiBaseUrl + '/api/assessmentavailability/UpdateStudentAttributeVisibility', paramObj);
                return promise;
            }

            self.hideAssessment = function (attribute) {
                var attributes = [];
                attributes.push({ Id: attribute.Id, Name: '', Visible: false });
                var paramObj = { Attributes: attributes };

                return $http.post(webApiBaseUrl + '/api/assessmentavailability/UpdateStudentAttributeVisibility', paramObj).then(function (response) {
                    self.initialize(); // reload fields
                });
            }

            // get list of assessments
            self.initialize = function () {
                var promise = $http.get(webApiBaseUrl + '/api/assessmentavailability/GetStudentAttributeList')

                return promise.then(function (response) {
                    angular.extend(self, response.data);
                });
            };

            self.initialize();
        }])
    .directive('stackedBarGraphSummaryAttributeChooser', ['stackedBarGraphSummaryAttributeChooserSvc', '$uibModal', 'nsPinesService', 'progressLoader', '$rootScope',
            function (stackedBarGraphSummaryAttributeChooserSvc, $uibModal, nsPinesService, progressLoader, $rootScope) {

                return {
                    restrict: 'E',
                    templateUrl: 'templates/student-attribute-chooser.html',
                    link: function (scope, element, attr) {
                        scope.attributeService = stackedBarGraphSummaryAttributeChooserSvc;
                        scope.modifiedAttributes = [];
                        scope.modifiedFields = [];
                        scope.modifiedColumns = [];

                        scope.updateSelectedColumn = function (column) {
                            scope.modifiedColumns.push(column);
                        }

                        scope.updateSelectedAttribute = function (attribute) {
                            // make sure it isnt already on the list first
                            for (var i = 0; i < scope.modifiedAttributes.length; i++){
                                if (scope.modifiedAttributes[i].id == attribute.id) {
                                    return;
                                }
                            }
                            scope.modifiedAttributes.push(attribute);
                        }
                        //scope.updateSelectedField = function (field) {
                        //    scope.modifiedFields.push(field);
                        //}

                        scope.openAttributeChooser = function () {
                            var modalInstance = $uibModal.open({
                                templateUrl: 'attributeChooser.html',
                                scope: scope,
                                controller: function ($scope, $uibModalInstance) {

                                    $scope.refreshAttributes = function () {
                                        progressLoader.start();
                                        progressLoader.set(50);
                                        $scope.attributeService.updateAttributes($scope.modifiedAttributes).then(function (response) {
                                            $rootScope.$broadcast('NSStudentAttributesUpdated', true);
                                            nsPinesService.dataSavedSuccessfully();
                                            progressLoader.end()
                                            scope.modifiedAttributes = [];
                                       
                                        });
                                        $uibModalInstance.dismiss('cancel');
                                    }

                                    $scope.cancel = function () {
                                        $uibModalInstance.dismiss('cancel');
                                    };
                                },
                                size: 'md',
                            });
                        }


                    }
                }

            }])
    ;

    DistrictAssessmentsController.$inject = ['$scope', 'InterventionGroup', '$q', '$http', 'nsPinesService', '$location', '$routeParams', 'NSDistrictAssessmentAvailabilityManager'];

    function DistrictAssessmentsController($scope, InterventionGroup, $q, $http, nsPinesService, $location, $routeParams, NSDistrictAssessmentAvailabilityManager) {

        var vm = this;
        vm.dataManager = new NSDistrictAssessmentAvailabilityManager();

        vm.saveAvailability = function (availability) {
            vm.dataManager.saveAvailability(availability).then(function (response) {
                if (response) {
                    nsPinesService.dataSavedSuccessfully();
                } else {
                    nsPinesService.dataSaveError();
                }
            });
        };
    }

    SchoolAssessmentsController.$inject = ['$scope', 'InterventionGroup', '$q', '$http', 'nsPinesService', '$location', '$routeParams', 'NSSchoolAssessmentAvailabilityManager', 'nsFilterOptionsService','spinnerService', '$timeout'];

    function SchoolAssessmentsController($scope, InterventionGroup, $q, $http, nsPinesService, $location, $routeParams, NSSchoolAssessmentAvailabilityManager, nsFilterOptionsService, spinnerService, $timeout) {

        $scope.filterOptions = nsFilterOptionsService.options;
        $scope.dataManager = new NSSchoolAssessmentAvailabilityManager();
        $scope.dataManager.initialize($scope.filterOptions.selectedSchool);

        $scope.saveAvailability = function (availability) {
            if (!availability.IsDisabled) {
                $scope.dataManager.saveAvailability(availability).then(function (response) {
                    if (response) {
                        nsPinesService.dataSavedSuccessfully();
                    } else {
                        nsPinesService.dataSaveError();
                    }
                });
            }
        };

        $scope.$watch('filterOptions.selectedSchool', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                    spinnerService.show('tableSpinner');
                $scope.dataManager.initialize($scope.filterOptions.selectedSchool).finally(function (response) {
                    spinnerService.hide('tableSpinner');
                });
            }
        }, true);
    }

    PersonalAssessmentsController.$inject = ['$scope', 'InterventionGroup', '$q', '$http', 'nsPinesService', '$location', '$routeParams', 'NSStaffAssessmentAvailabilityManager', '$global'];

    function PersonalAssessmentsController($scope, InterventionGroup, $q, $http, nsPinesService, $location, $routeParams, NSStaffAssessmentAvailabilityManager, $global) {

        var vm = this;
        vm.dataManager = new NSStaffAssessmentAvailabilityManager();

        vm.saveAvailability = function (availability) {
            if (!availability.IsDisabled) {
                vm.dataManager.saveAvailability(availability).then(function (response) {
                    if (response) {
                        $global.set('navrefreshneeded', true);
                        nsPinesService.dataSavedSuccessfully();
                    } else {
                        nsPinesService.dataSaveError();
                    }
                });
            }
        };
    }

})();