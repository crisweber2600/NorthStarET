(function () {
	'use strict';

    angular
        .module('interventionToolkitModule', [])
        .controller('InterventionTierController', InterventionTierController)
        .controller('InterventionViewTierController', InterventionViewTierController)
        .controller('InterventionDetailController', InterventionDetailController)
        .service('nsInterventionToolkitService', [
			    '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
			        //this.options = {};

			        this.getTiers = function () {
			            return $http.get(webApiBaseUrl + '/api/interventiontoolkit/getinterventiontiers');
			        };
			        //$routeParams.id, $scope.selectedGrade, $scope.selectedCategory, $scope.selectedGroupSize, $scope.selectedUnitOfStudy, $scope.selectedFramework, $scope.selectedWorkshop
			        this.getTierData = function (id, selectedGrade, selectedCategory, selectedWorkshop) {
			            var paramObj = {
			                GradeId: !selectedGrade ? -1 : selectedGrade,
			                CategoryId: !selectedCategory ? -1 : selectedCategory,
			                WorkshopId: !selectedWorkshop ? -1 : selectedWorkshop,
                            TierId: id
			            };
			        
			            return $http.post(webApiBaseUrl + '/api/interventiontoolkit/GetInterventionsByTier', paramObj);
			        };

			        this.getInterventionById = function (id) {
			            var paramObj = {
			                Id: id
			            };

			            return $http.post(webApiBaseUrl + '/api/interventiontoolkit/GetInterventionById', paramObj);
			        };

			        this.saveIntervention = function (intervention) {
			            var paramObj = {
			                Intervention: intervention
			            };

			            return $http.post(webApiBaseUrl + '/api/interventiontoolkit/SaveIntervention', paramObj);
			        };
			        //this.applyStatusNotes = function (attendanceDate, status, notes, staffId, sectionId, schoolStartYear) {
			        //    return $http.post(webApiBaseUrl + '/api/interventiongroup/applyStatusNotes', { Notes: notes, 'Date': attendanceDate, Status: status, StaffId: staffId, SectionId: sectionId, SchoolStartYear: schoolStartYear });
			        //};


			    }]
	    );

    InterventionTierController.$inject = ['$scope', 'InterventionGroup', '$q', '$http', 'pinesNotifications', '$location', 'nsInterventionToolkitService', 'nsPinesService'];


    function InterventionTierController($scope, InterventionGroup, $q, $http, pinesNotifications, $location, nsInterventionToolkitService, nsPinesService) {
        nsInterventionToolkitService.getTiers().then(function(response) {
            $scope.tiers = response.data.Tiers;

            // create data array
            $scope.tierData = [];
            for (var i = 0; i < $scope.tiers.length; i++) {
                var data = {
                    title: 'Tier ' + $scope.tiers[i].TierValue,
                    href: '#/intervention-view-tier/' + $scope.tiers[i].TierValue,
                    titleBarInfo: '<span class="badge">4</span> Interventions',
                    text: $scope.tiers[i].Description,
                    color: 'info',
                    classes: 'fa fa-eye'
                }
                $scope.tierData.push(data);
            }

        });
    }

    InterventionViewTierController.$inject = ['$scope', 'InterventionGroup', '$q', '$http', 'pinesNotifications', '$location', 'nsInterventionToolkitService', 'nsPinesService','$routeParams'];
    function InterventionViewTierController($scope, InterventionGroup, $q, $http, pinesNotifications, $location, nsInterventionToolkitService, nsPinesService, $routeParams) {

        $scope.selectedOptions = {};
        $scope.selectedOptions.selectedGrade = -1;
        $scope.selectedOptions.selectedCategory = -1;
        $scope.selectedOptions.selectedWorkshop = -1;

        var LoadTierData = function () {
            console.time("GetInterventions");
            nsInterventionToolkitService.getTierData($routeParams.id,
                $scope.selectedOptions.selectedGrade,
                $scope.selectedOptions.selectedCategory,
                $scope.selectedOptions.selectedWorkshop).then(function(response) {
                            $scope.interventions = response.data.Interventions;
                            $scope.grades = response.data.Grades;
                            $scope.categories = response.data.Categories;
                            $scope.workshops = response.data.Workshops;
                            $scope.tier = response.data.Tier;
                            console.timeEnd("GetInterventions");
            });
        }

        LoadTierData();

        $scope.$watchCollection('selectedOptions', function (newVal, oldVal) {
            if (newVal !== oldVal) {
                LoadTierData();
            }
        });
    }

    InterventionDetailController.$inject = ['$scope', 'InterventionGroup', '$q', '$http', 'pinesNotifications', '$location', 'nsInterventionToolkitService', 'nsPinesService', '$uibModal', '$routeParams', 'progressLoader', 'nsSelect2RemoteOptions'];
    function InterventionDetailController($scope, InterventionGroup, $q, $http, pinesNotifications, $location, nsInterventionToolkitService, nsPinesService, $uibModal, $routeParams, progressLoader, nsSelect2RemoteOptions) {

        $scope.gradeOptions = nsSelect2RemoteOptions.GradeQuickSearchRemoteOptions;
        $scope.workshopOptions = nsSelect2RemoteOptions.WorkshopRemoteOptions;

        var LoadData = function () {
            nsInterventionToolkitService.getInterventionById($routeParams.id)
                .then(function (response) {
                    $scope.intervention = response.data.Intervention;

                });
        }

        $scope.settings = {
            IsEditing: false,
            Mode: 'overview'
        };

        // go into edit mode
        $scope.edit = function () {
            $scope.settings.IsEditing = true;

            $scope.intervention.DetailedDescriptionTemp = $scope.intervention.DetailedDescription;
            $scope.intervention.LearnerNeedTemp = $scope.intervention.LearnerNeed;
            $scope.intervention.ExitCriteriaTemp = $scope.intervention.ExitCriteria;
            $scope.intervention.EntranceCriteriaTemp = $scope.intervention.EntranceCriteria;
            $scope.intervention.BriefDescriptionTemp = $scope.intervention.BriefDescription;
            $scope.intervention.TimeOfYearTemp = $scope.intervention.TimeOfYear;
            $scope.intervention.InterventionTierIDTemp = $scope.intervention.InterventionTierID + '';
        }

        $scope.cancel = function () {
            $scope.settings.IsEditing = false;
        }

        $scope.save = function () {
            progressLoader.start();
            progressLoader.set(50);

            // call save function, show message, then
            $scope.intervention.DetailedDescription = $scope.intervention.DetailedDescriptionTemp;
            $scope.intervention.LearnerNeed = $scope.intervention.LearnerNeedTemp;
            $scope.intervention.ExitCriteria = $scope.intervention.ExitCriteriaTemp;
            $scope.intervention.EntranceCriteria = $scope.intervention.EntranceCriteriaTemp;
            $scope.intervention.BriefDescription = $scope.intervention.BriefDescriptionTemp;
            $scope.intervention.TimeOfYear = $scope.intervention.TimeOfYearTemp;
            $scope.intervention.InterventionTierID = $scope.intervention.InterventionTierIDTemp;

            nsInterventionToolkitService.saveIntervention($scope.intervention)
                .then(function (response) {
                    progressLoader.end();
                    nsPinesService.dataSavedSuccessfully();
                    $scope.settings.IsEditing = false;
                    LoadData();
                });
        }

        LoadData();

        $scope.openModal = function (size, templateUrl) {
            var modalInstance = $uibModal.open({
                templateUrl: templateUrl,
                controller: function ($scope, $uibModalInstance) {
                    $scope.close = function () {
                        $uibModalInstance.dismiss('cancel');
                    };
                },
                size: size,
            });
        }
    }


})();