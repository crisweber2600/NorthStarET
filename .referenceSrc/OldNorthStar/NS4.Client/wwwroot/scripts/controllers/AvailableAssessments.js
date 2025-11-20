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
                var paramObj = {AssessmentsOrFields : assessments};
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

            self.updateFields = function (fields) {
                var paramObj = { AssessmentsOrFields: fields };
                var promise = $http.post(webApiBaseUrl + '/api/assessmentavailability/UpdateObservationSummaryAssessmentFieldVisibility', paramObj);
                return promise;
            }

            // get list of assessments
            self.initialize = function () {
                var promise = $http.get(webApiBaseUrl + '/api/assessmentavailability/GetObservationSummaryAssessmentList').then(function (response) {
                    angular.extend(self, response.data);
                })
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
                        scope.settings = { menuOpen: false };

                        scope.updateSelectedAssessment = function (assessment) {
                            scope.modifiedAssessments.push(assessment);
                        }
                        scope.updateSelectedField = function (field) {
                            scope.modifiedFields.push(field);
                        }

                        scope.refreshAssessments = function () {
                            progressLoader.start();
                            progressLoader.set(50);
                            scope.assessmentService.updateAssessments(scope.modifiedAssessments).then(function (response) {
                                $rootScope.$broadcast('NSFieldsUpdated', true);
                                nsPinesService.dataSavedSuccessfully();
                                progressLoader.end()
                                scope.modifiedAssessments = [];
                                scope.settings.menuOpen = false;
                            });
                        }

                        scope.openFieldsPopup = function (assessment) {

                            scope.selectedAssessment = assessment;

                            // get fields
                            scope.assessmentService.getAssessmentFields(assessment.id).then(function (response) {
                                scope.selectedFields = response.data.Fields;

                                var modalInstance = $uibModal.open({
                                    templateUrl: 'assessmentFieldViewer.html',
                                    scope: scope,
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

    SchoolAssessmentsController.$inject = ['$scope', 'InterventionGroup', '$q', '$http', 'nsPinesService', '$location', '$routeParams', 'NSSchoolAssessmentAvailabilityManager', 'nsFilterOptionsService'];

    function SchoolAssessmentsController($scope, InterventionGroup, $q, $http, nsPinesService, $location, $routeParams, NSSchoolAssessmentAvailabilityManager, nsFilterOptionsService) {

        var vm = this;
        vm.filterOptions = nsFilterOptionsService.options;
        vm.dataManager = new NSSchoolAssessmentAvailabilityManager();
        vm.dataManager.initialize(vm.filterOptions.selectedSchool);

        vm.saveAvailability = function (availability) {
            if (!availability.IsDisabled) {
                vm.dataManager.saveAvailability(availability).then(function (response) {
                    if (response) {
                        nsPinesService.dataSavedSuccessfully();
                    } else {
                        nsPinesService.dataSaveError();
                    }
                });
            }
        };

        $scope.$watch('vm.filterOptions.selectedSchool.Id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                vm.dataManager.initialize(vm.filterOptions.selectedSchool);
            }
        });
    }

    PersonalAssessmentsController.$inject = ['$scope', 'InterventionGroup', '$q', '$http', 'nsPinesService', '$location', '$routeParams', 'NSStaffAssessmentAvailabilityManager'];

    function PersonalAssessmentsController($scope, InterventionGroup, $q, $http, nsPinesService, $location, $routeParams, NSStaffAssessmentAvailabilityManager) {

        var vm = this;
        vm.dataManager = new NSStaffAssessmentAvailabilityManager();

        vm.saveAvailability = function (availability) {
            if (!availability.IsDisabled) {
                vm.dataManager.saveAvailability(availability).then(function (response) {
                    if (response) {
                        nsPinesService.dataSavedSuccessfully();
                    } else {
                        nsPinesService.dataSaveError();
                    }
                });
            }
        };
    }

})();