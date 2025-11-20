(function () {
	'use strict';

	angular
		.module('interventionToolkitModule', [])
		.controller('InterventionTierController', InterventionTierController)
		.controller('InterventionViewTierController', InterventionViewTierController)
		.controller('InterventionViewPagesController', InterventionViewPagesController)
		.controller('InterventionDetailController', InterventionDetailController)
		.controller('MTSSPageController', MTSSPageController)
		   .service('nsFileDownloadService', [
				'$http', 'webApiBaseUrl', 'FileSaver', function ($http, webApiBaseUrl, FileSaver) {

					this.getFile = function (filename) {
						return $http.get(webApiBaseUrl + '/api/azuredownload/downloadnorthstarfile?filename=' + filename, {
							responseType: 'arraybuffer'
							}).then(function (response) {
							var data = new Blob([response.data]);
							FileSaver.saveAs(data, filename);
						});
					};
					this.loadVideos = function (filename) {
						return $http.post(webApiBaseUrl + '/api/video/GetPagedVideoList');
					};
					this.getZippedTools = function (arrFileNames) {
						var paramObj = { FileNames: arrFileNames };

						return $http.post(webApiBaseUrl + '/api/azuredownload/downloadzippedtools', paramObj, {
							responseType: 'arraybuffer'
						}).then(function (response) {
							var data = new Blob([response.data]);
							FileSaver.saveAs(data, "NorthStarTools.zip");
						});
					};
				}]
		)
		.service('nsInterventionToolkitService', [
				'$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
					//this.options = {};

					this.getTiers = function () {
						return $http.get(webApiBaseUrl + '/api/interventiontoolkit/getinterventiontiers');
					};

					this.getPages = function () {
						return $http.get(webApiBaseUrl + '/api/interventiontoolkit/getpages');
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

					this.getPageById = function (id) {
						var paramObj = {
							Id: id
						};

						return $http.post(webApiBaseUrl + '/api/interventiontoolkit/GetPageById', paramObj);
					};

					this.deleteIntervention = function (id) {
						var paramObj = {
							Id: id
						};

						return $http.post(webApiBaseUrl + '/api/interventiontoolkit/DeleteIntervention', paramObj);
					};

					this.deletePage = function (id) {
					    var paramObj = {
					        Id: id
					    };

					    return $http.post(webApiBaseUrl + '/api/interventiontoolkit/DeletePage', paramObj);
					};

					this.saveIntervention = function (intervention) {
						var paramObj = {
							Intervention: intervention
						};

						return $http.post(webApiBaseUrl + '/api/interventiontoolkit/SaveIntervention', paramObj);
					};

					this.savePage = function (page) {
						var paramObj = {
							NSPage: page
						};

						return $http.post(webApiBaseUrl + '/api/interventiontoolkit/SavePage', paramObj);
					};

					this.saveTool = function (tool) {
						var paramObj = {
							Tool: tool
						};

						return $http.post(webApiBaseUrl + '/api/interventiontoolkit/SaveTool', paramObj);
					};

					this.savePageTool = function (tool) {
						var paramObj = {
							Tool: tool
						};

						return $http.post(webApiBaseUrl + '/api/interventiontoolkit/savePageTool', paramObj);
					};

					this.savePagePresentation = function (presentation) {
						var paramObj = {
							Presentation: presentation
						};

						return $http.post(webApiBaseUrl + '/api/interventiontoolkit/savePagePresentation', paramObj); 
					};

					this.saveVideo = function (video) {
						var paramObj = {
							Video: video
						};

						return $http.post(webApiBaseUrl + '/api/interventiontoolkit/SaveVideo', paramObj);
					};

					this.savePageVideo = function (video) {
						var paramObj = {
							Video: video
						};

						return $http.post(webApiBaseUrl + '/api/interventiontoolkit/savePageVideo', paramObj);
					};


					this.removeTool = function (interventionId, toolId) {
						var paramObj = {
							InterventionToolId: toolId,
							InterventionId: interventionId
						};

						return $http.post(webApiBaseUrl + '/api/interventiontoolkit/RemoveTool', paramObj);
					};

					this.removePageTool = function (pageId, toolId) {
						var paramObj = {
							ToolId: toolId,
							PageId: pageId
						};

						return $http.post(webApiBaseUrl + '/api/interventiontoolkit/RemovePageTool', paramObj);
					};

					this.removePresentation = function (pageId, presentationId) {
						var paramObj = {
							PresentationId: presentationId,
							PageId: pageId
						};

						return $http.post(webApiBaseUrl + '/api/interventiontoolkit/RemovePresentation', paramObj);
					};

					this.removeVideo = function (interventionId, videoId) {
						var paramObj = {
							VideoId: videoId,
							InterventionId: interventionId
						};

						return $http.post(webApiBaseUrl + '/api/interventiontoolkit/RemoveVideo', paramObj);
					};

					this.removePageVideo = function (pageId, videoId) {
						var paramObj = {
							VideoId: videoId,
							PageId: pageId
						};

						return $http.post(webApiBaseUrl + '/api/interventiontoolkit/RemovePageVideo', paramObj);
					};

					this.associateTool = function (interventionId, toolId) {
						var paramObj = {
							InterventionToolId: toolId,
							InterventionId: interventionId
						};

						return $http.post(webApiBaseUrl + '/api/interventiontoolkit/AssociateTool', paramObj);
					};

					this.associatePageTool = function (pageId, toolId) {
						var paramObj = {
							ToolId: toolId,
							PageId: pageId
						};

						return $http.post(webApiBaseUrl + '/api/interventiontoolkit/AssociatePageTool', paramObj);
					};
		
					this.associatePagePresentation = function (pageId, presentationId) {
						var paramObj = {
							PresentationId: presentationId,
							PageId: pageId
						};

						return $http.post(webApiBaseUrl + '/api/interventiontoolkit/AssociatePagePresentation', paramObj);
					};
				}]
		);

	InterventionTierController.$inject = ['$scope', 'InterventionGroup', '$q', '$http', 'pinesNotifications', '$location', 'nsInterventionToolkitService', 'nsPinesService','NSUserInfoService'];


	function InterventionTierController($scope, InterventionGroup, $q, $http, pinesNotifications, $location, nsInterventionToolkitService, nsPinesService, NSUserInfoService) {
		nsInterventionToolkitService.getTiers().then(function(response) {
			$scope.tiers = response.data.Tiers;
			$scope.currentUser = NSUserInfoService.currentUser;

			// create data array
			$scope.tierData = [];
			//for (var i = 0; i < $scope.tiers.length; i++) {
			//    var data = {
			//        title: 'Tier ' + $scope.tiers[i].TierValue,
			//        href: '#/intervention-view-tier/' + $scope.tiers[i].TierValue,
			//        titleBarInfo: '<span class="badge">4</span> Interventions',
			//        text: $scope.tiers[i].Description,
			//        color: 'info',
			//        classes: 'fa fa-eye'
			//    }
			//    $scope.tierData.push(data);
			//}

		});
	}

	InterventionViewTierController.$inject = ['$scope', 'InterventionGroup', '$q', '$http', 'pinesNotifications', '$location', 'nsInterventionToolkitService', 'nsPinesService','$routeParams', 'progressLoader', '$bootbox', 'NSUserInfoService'];
	function InterventionViewTierController($scope, InterventionGroup, $q, $http, pinesNotifications, $location, nsInterventionToolkitService, nsPinesService, $routeParams, progressLoader, $bootbox, NSUserInfoService) {
		$scope.currentUser = NSUserInfoService.currentUser;
		$scope.selectedOptions = {};
		$scope.selectedOptions.selectedGrade = "-1";
		$scope.selectedOptions.selectedCategory = "-1";
		$scope.selectedOptions.selectedWorkshop = "-1";

		var LoadTierData = function () {
			progressLoader.start();
			progressLoader.set(50)
			nsInterventionToolkitService.getTierData($routeParams.id,
				$scope.selectedOptions.selectedGrade,
				$scope.selectedOptions.selectedCategory,
				$scope.selectedOptions.selectedWorkshop).then(function(response) {
							$scope.interventions = response.data.Interventions;
							$scope.grades = response.data.Grades;
							$scope.categories = response.data.Categories;
							$scope.workshops = response.data.Workshops;
							$scope.tier = response.data.Tier;
							progressLoader.end();
			});
		}

		LoadTierData();

		$scope.delete = function (id, $event) {
			$event.preventDefault();
			$event.stopPropagation();

			$bootbox.confirm('Are you ABSOLUTELY SURE you want to delete this intervention?  Once deleted, it is GONE.', function (response) {
				if (response) {
					nsInterventionToolkitService.deleteIntervention(id).then(function (response) {
						LoadTierData();
					})
				}
			});
		}

		$scope.$watchCollection('selectedOptions', function (newVal, oldVal) {
			if (newVal !== oldVal) {
				LoadTierData();
			}
		});
	}

	InterventionViewPagesController.$inject = ['$scope', 'InterventionGroup', '$q', '$http', 'pinesNotifications', '$location', 'nsInterventionToolkitService', 'nsPinesService', '$routeParams', 'progressLoader', '$bootbox', 'NSUserInfoService'];
	function InterventionViewPagesController($scope, InterventionGroup, $q, $http, pinesNotifications, $location, nsInterventionToolkitService, nsPinesService, $routeParams, progressLoader, $bootbox, NSUserInfoService) {
		$scope.currentUser = NSUserInfoService.currentUser;
		$scope.selectedOptions = {};
	  

		var LoadTierData = function () {
			progressLoader.start();
			progressLoader.set(50)
			nsInterventionToolkitService.getPages().then(function (response) {
					$scope.pages = response.data.Pages;
					progressLoader.end();
				});
		}

		$scope.delete = function (id, $event) {
		    $event.preventDefault();
		    $event.stopPropagation();

		    $bootbox.confirm('Are you ABSOLUTELY SURE you want to delete this page?  Once deleted, it is GONE.', function (response) {
		        if (response) {
		            nsInterventionToolkitService.deletePage(id).then(function (response) {
		                LoadTierData();
		            })
		        }
		    });
		}

		LoadTierData();
	
	}

	InterventionDetailController.$inject = ['$scope', 'InterventionGroup', '$q', '$http', 'pinesNotifications', '$location', 'nsInterventionToolkitService', 'nsPinesService', '$uibModal', '$routeParams', 'progressLoader', 'nsSelect2RemoteOptions', 'webApiBaseUrl', '$timeout', '$bootbox', 'nsFileDownloadService', 'NSUserInfoService'];
	function InterventionDetailController($scope, InterventionGroup, $q, $http, pinesNotifications, $location, nsInterventionToolkitService, nsPinesService, $uibModal, $routeParams, progressLoader, nsSelect2RemoteOptions, webApiBaseUrl, $timeout, $bootbox, nsFileDownloadService, NSUserInfoService) {
		$scope.currentUser = NSUserInfoService.currentUser;
		$scope.uploadSettings = {};
		$scope.Intervention = { ready: false };
		$scope.gradeOptions = nsSelect2RemoteOptions.GradeQuickSearchRemoteOptions;
		$scope.workshopOptions = nsSelect2RemoteOptions.WorkshopRemoteOptions;
		$scope.nsSelect2RemoteOptions = nsSelect2RemoteOptions;
		$scope.nsFileDownloadService = nsFileDownloadService;
		$scope.SelectedAssessmentTools = [];
		$scope.SelectedInterventionTools = [];

		var LoadData = function () {
			nsInterventionToolkitService.getInterventionById($routeParams.id)
				.then(function (response) {
					angular.extend($scope, response.data);
					$scope.Intervention.ready = true;
					//$scope.Intervention = response.data.Intervention;
					// edit mode by default for new items
					if ($routeParams.id == -1) {
						$scope.Intervention.InterventionTierID = '';
						$scope.edit();
					}
				});
		}

		LoadData();

		$scope.callit = function () {
			nsFileDownloadService.loadVideos();
		}

		$scope.downloadSelectedAssessmentTools = function () {
			nsFileDownloadService.getZippedTools($scope.SelectedAssessmentTools);
		}

		$scope.downloadSelectedInterventionTools = function () {
			nsFileDownloadService.getZippedTools($scope.SelectedInterventionTools);
		}

		$scope.saveTool = function (tool) {
			nsInterventionToolkitService.saveTool(tool)
				.then(function (response) {
					progressLoader.end();
					nsPinesService.dataSavedSuccessfully();
				});
		}

		$scope.saveVideo = function (video) {
			nsInterventionToolkitService.saveVideo(video)
				.then(function (response) {
					progressLoader.end();
					nsPinesService.dataSavedSuccessfully();
					LoadData();
				});
		}

		$scope.settings = {
			IsEditing: false,
			Mode: 'overview',
			SelectedAssessmentTool: {},
			SelectedInterventionTool: {},
			SelectedAssociatedVideo: {},
			SelectAllAssessmentTools: false,
			SelectAllInterventionTools: false
		};

		$scope.toggleSelectedAssessmentTools = function () {
			for (var i = 0; i < $scope.Intervention.AssessmentTools.length; i++) {
				var tool = $scope.Intervention.AssessmentTools[i];
				tool.toolIsSelected = $scope.settings.SelectAllAssessmentTools;
				$scope.toggleAssessmentToolSelection(tool);
			}
		}

		$scope.toggleSelectedInterventionTools = function () {
			for (var i = 0; i < $scope.Intervention.InterventionTools.length; i++) {
				var tool = $scope.Intervention.InterventionTools[i];
				tool.toolIsSelected = $scope.settings.SelectAllInterventionTools;
				$scope.toggleInterventionToolSelection(tool);
			}
		}

		$scope.toggleAssessmentToolSelection = function (tool) {
			if (tool.toolIsSelected) {
				var isFound = false;
				for (var i = 0; i < $scope.SelectedAssessmentTools.length; i++) {
					if ($scope.SelectedAssessmentTools[i] == tool.ToolFileName) {
						isFound = true;
						break;
					}
				}
				// only allow to be added once
				if (!isFound) {
					$scope.SelectedAssessmentTools.push(tool.ToolFileName);
				}
			} else {
				// remove tool
				for (var i = 0; i < $scope.SelectedAssessmentTools.length; i++) {
					if ($scope.SelectedAssessmentTools[i] == tool.ToolFileName) {
						$scope.SelectedAssessmentTools.splice(i, 1);
						break;
					}
				}
			}
		}

		$scope.toggleInterventionToolSelection = function (tool) {
			if (tool.toolIsSelected) {
				var isFound = false;
				for (var i = 0; i < $scope.SelectedInterventionTools.length; i++) {
					if ($scope.SelectedInterventionTools[i] == tool.ToolFileName) {
						isFound = true;
						break;
					}
				}
				// only allow to be added once
				if (!isFound) {
					$scope.SelectedInterventionTools.push(tool.ToolFileName);
				}
			} else {
				// remove tool
				for (var i = 0; i < $scope.SelectedInterventionTools.length; i++) {
					if ($scope.SelectedInterventionTools[i] == tool.ToolFileName) {
						$scope.SelectedInterventionTools.splice(i, 1);
						break;
					}
				}
			}
		}

		// go into edit mode
		$scope.edit = function () {
			$scope.settings.IsEditing = true;

			$scope.Intervention.DetailedDescriptionTemp = $scope.Intervention.DetailedDescription;
			$scope.Intervention.LearnerNeedTemp = $scope.Intervention.LearnerNeed;
			$scope.Intervention.ExitCriteriaTemp = $scope.Intervention.ExitCriteria;
			$scope.Intervention.EntranceCriteriaTemp = $scope.Intervention.EntranceCriteria;
			$scope.Intervention.BriefDescriptionTemp = $scope.Intervention.BriefDescription;
			$scope.Intervention.TimeOfYearTemp = $scope.Intervention.TimeOfYear;
			$scope.Intervention.InterventionTierIDTemp = $scope.Intervention.InterventionTierID + '';
		}

		$scope.cancel = function () {
			$scope.settings.IsEditing = false;
		}

		$scope.removeTool = function (toolId) {
			$bootbox.confirm("Are you sure you want to remove this tool?  Note: The tool file is not deleted.", function (response) {
				if (response) {
					nsInterventionToolkitService.removeTool($scope.Intervention.Id, toolId)
					   .then(function (response) {
						   progressLoader.end();
						   nsPinesService.dataSavedSuccessfully();
						   LoadData();
					   });
				}
			})
		}

		$scope.removeVideo = function (videoId) {
			$bootbox.confirm("Are you sure you want to remove this video from this intervention?  Note: The video is not deleted.", function (response) {
				if (response) {
					nsInterventionToolkitService.removeVideo($scope.Intervention.Id, videoId)
					   .then(function (response) {
						   progressLoader.end();
						   nsPinesService.dataSavedSuccessfully();
						   LoadData();
					   });
				}
			})
		}

		$scope.save = function () {
			progressLoader.start();
			progressLoader.set(50);

			// call save function, show message, then
			$scope.Intervention.DetailedDescription = $scope.Intervention.DetailedDescriptionTemp;
			$scope.Intervention.LearnerNeed = $scope.Intervention.LearnerNeedTemp;
			$scope.Intervention.ExitCriteria = $scope.Intervention.ExitCriteriaTemp;
			$scope.Intervention.EntranceCriteria = $scope.Intervention.EntranceCriteriaTemp;
			$scope.Intervention.BriefDescription = $scope.Intervention.BriefDescriptionTemp;
			$scope.Intervention.TimeOfYear = $scope.Intervention.TimeOfYearTemp;
			$scope.Intervention.InterventionTierID = $scope.Intervention.InterventionTierIDTemp;

			nsInterventionToolkitService.saveIntervention($scope.Intervention)
				.then(function (response) {
					progressLoader.end();
					nsPinesService.dataSavedSuccessfully();

					// if we just saved a new one, redirect to new id
					if ($routeParams.id == -1) {
						$location.path("intervention-detail/" + response.data.id);
					} else {
						$scope.settings.IsEditing = false;
						LoadData();
					}
				});
		}



		$scope.associateAssessmentToolDialog = function () {
			var modalInstance = $uibModal.open({
				templateUrl: 'associateAssessmentTool.html',
				scope: $scope,
				controller: function ($scope, $uibModalInstance) {

					$scope.associateTool = function () {
						var paramObj = { InterventionId: $scope.Intervention.Id, InterventionToolId: $scope.settings.SelectedAssessmentTool.id };
						// start loader
						progressLoader.start();
						progressLoader.set(50);
						var promise = $http.post(webApiBaseUrl + '/api/interventiontoolkit/AssociateTool', paramObj).then(function (response) {
							// end loader
							progressLoader.end();
							$scope.errors = [];
							// show success
							$timeout(function () {
								$('#formReset').click();
							}, 100);

							LoadData();
							$scope.settings.SelectedAssessmentTool = {};
							nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
						});

						$uibModalInstance.dismiss('cancel');
					};
					$scope.cancel = function () {
						$scope.settings.SelectedAssessmentTool = {};
						$uibModalInstance.dismiss('cancel');
					};
				},
				size: 'md',
			});
		}

		$scope.displayVideoDialog = function (video) {
			$scope.selectedVideo = video;
			var modalInstance = $uibModal.open({
				templateUrl: 'playVideo.html',
				scope: $scope,
				controller: function ($scope, $uibModalInstance, $sce) {

					$scope.getSrc = function () {
						if ($scope.selectedVideo) {
							return $sce.trustAsResourceUrl("https://view.vzaar.com/" + $scope.selectedVideo.VideoStreamId + "/player?apiOn=true");
						}
						return $sce.trustAsResourceUrl("about:blank");
  
					};
					$scope.cancel = function () {
						$scope.selectedVideo = null;
						$uibModalInstance.dismiss('cancel');
					};
				},
				size: 'lg',
			});
		}

		$scope.associateVideoDialog = function () {
			var modalInstance = $uibModal.open({
				templateUrl: 'associateVideo.html',
				scope: $scope,
				controller: function ($scope, $uibModalInstance) {

					$scope.associateVideo = function () {

						var paramObj = { InterventionId: $scope.Intervention.Id, Video: $scope.settings.SelectedAssociatedVideo };
						// start loader
						progressLoader.start();
						progressLoader.set(50);
						var promise = $http.post(webApiBaseUrl + '/api/interventiontoolkit/associatevideo', paramObj).then(function (response) {
							// end loader
							progressLoader.end();
							$scope.errors = [];
							// show success
							$timeout(function () {
								$('#formReset').click();
							}, 100);

							$scope.settings.SelectedAssociatedVideo = {};
							LoadData();
							nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
						});

						$uibModalInstance.dismiss('cancel');
					};
					$scope.cancel = function () {
						$scope.settings.SelectedAssociatedVideo = {};
						$uibModalInstance.dismiss('cancel');
					};
				},
				size: 'md',
			});
		}

		$scope.associateInterventionToolDialog = function () {
			var modalInstance = $uibModal.open({
				templateUrl: 'associateInterventionTool.html',
				scope: $scope,
				controller: function ($scope, $uibModalInstance) {

					$scope.associateTool = function () {

						var paramObj = { InterventionId: $scope.Intervention.Id, InterventionToolId: $scope.settings.SelectedInterventionTool.id };
						// start loader
						progressLoader.start();
						progressLoader.set(50);
						var promise = $http.post(webApiBaseUrl + '/api/interventiontoolkit/AssociateTool', paramObj).then(function (response) {
							// end loader
							progressLoader.end();
							$scope.errors = [];
							// show success
							$timeout(function () {
								$('#formReset').click();
							}, 100);

							$scope.settings.SelectedInterventionTool = {};
							LoadData();
							nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
						});

						$uibModalInstance.dismiss('cancel');
					};
					$scope.cancel = function () {
						$scope.settings.SelectedInterventionTool = {};
						$uibModalInstance.dismiss('cancel');
					};
				},
				size: 'md',
			});
		}

		$scope.openUploadITDialog = function () {

			var modalInstance = $uibModal.open({
				templateUrl: 'uploadTool.html',
				scope: $scope,
				controller: function ($scope, $uibModalInstance) {
					$scope.theFiles = [];

					$scope.upload = function (theFiles) {

						var formData = new FormData();
						formData.append("InterventionId", $routeParams.id);

						angular.forEach(theFiles, function (file) {
							formData.append(file.name, file);
						});
						var paramObj = {};
						// start loader
						progressLoader.start();
						progressLoader.set(50);
						var promise = $http.post(webApiBaseUrl + '/api/interventiontoolkit/uploadinterventiontool', formData, {
							transformRequest: angular.identity,
							headers: { 'Content-Type': undefined }
						}).then(function (response) {
							// end loader
							progressLoader.end();
							$scope.errors = [];
							// show success
							$timeout(function () {
								$('#formReset').click();
							}, 100);
							//$scope.theFiles.length = 0;
							//$scope.settings.hasFiles = false;
							$scope.uploadSettings.uploadComplete = true;

							LoadData();
							nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
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

		$scope.openUploadATDialog = function () {

			var modalInstance = $uibModal.open({
				templateUrl: 'uploadTool.html',
				scope: $scope,
				controller: function ($scope, $uibModalInstance) {
					$scope.theFiles = [];

					$scope.upload = function (theFiles) {

						var formData = new FormData();
						formData.append("InterventionId", $routeParams.id);

						angular.forEach(theFiles, function (file) {
							formData.append(file.name, file);
						});
						var paramObj = {};
						// start loader
						progressLoader.start();
						progressLoader.set(50);
						var promise = $http.post(webApiBaseUrl + '/api/interventiontoolkit/uploadassessmenttool', formData, {
							transformRequest: angular.identity,
							headers: { 'Content-Type': undefined }
						}).then(function (response) {
							// end loader
							progressLoader.end();
							$scope.errors = [];
							// show success
							$timeout(function () {
								$('#formReset').click();
							}, 100);
							//$scope.theFiles.length = 0;
							//$scope.settings.hasFiles = false;
							$scope.uploadSettings.uploadComplete = true;

							LoadData();
							nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
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


	MTSSPageController.$inject = ['$scope', 'InterventionGroup', '$q', '$http', 'pinesNotifications', '$location', 'nsInterventionToolkitService', 'nsPinesService', '$uibModal', '$routeParams', 'progressLoader', 'nsSelect2RemoteOptions', 'webApiBaseUrl', '$timeout', '$bootbox', 'nsFileDownloadService', 'NSUserInfoService'];
	function MTSSPageController($scope, InterventionGroup, $q, $http, pinesNotifications, $location, nsInterventionToolkitService, nsPinesService, $uibModal, $routeParams, progressLoader, nsSelect2RemoteOptions, webApiBaseUrl, $timeout, $bootbox, nsFileDownloadService, NSUserInfoService) {
		$scope.currentUser = NSUserInfoService.currentUser;
		$scope.uploadSettings = {};
		$scope.page = { ready: false };
		$scope.gradeOptions = nsSelect2RemoteOptions.GradeQuickSearchRemoteOptions;
		$scope.workshopOptions = nsSelect2RemoteOptions.WorkshopRemoteOptions;
		$scope.nsSelect2RemoteOptions = nsSelect2RemoteOptions;
		$scope.nsFileDownloadService = nsFileDownloadService;
		$scope.SelectedTools = [];
		$scope.SelectedPresentations = [];

		var LoadData = function () {
			nsInterventionToolkitService.getPageById($routeParams.id)
				.then(function (response) {
					angular.extend($scope.page, response.data.NSPage);
					$scope.page.ready = true;
					//$scope.Intervention = response.data.Intervention;
					// edit mode by default for new items
					if ($routeParams.id == -1) {
						//$scope.Intervention.InterventionTierID = '';
						$scope.edit();
					}
				});
		}

		LoadData();

		$scope.callit = function () {
			nsFileDownloadService.loadVideos();
		}

		$scope.downloadSelectedTools = function () {
			nsFileDownloadService.getZippedTools($scope.SelectedTools);
		}

		$scope.downloadSelectedPresentations = function () {
			nsFileDownloadService.getZippedTools($scope.SelectedPresentations);
		}

		$scope.saveTool = function (tool) {
		    nsInterventionToolkitService.savePageTool(tool)
				.then(function (response) {
					progressLoader.end();
					nsPinesService.dataSavedSuccessfully();
				});
		}

		$scope.savePresentation = function (tool) {
		    nsInterventionToolkitService.savePagePresentation(tool)
				.then(function (response) {
				    progressLoader.end();
				    nsPinesService.dataSavedSuccessfully();
				});
		}

		$scope.saveVideo = function (video) {
			nsInterventionToolkitService.savePageVideo(video)
				.then(function (response) {
					progressLoader.end();
					nsPinesService.dataSavedSuccessfully();
					LoadData();
				});
		}

		$scope.deletePage = function (page) {

		}

		$scope.settings = {
			IsEditing: false,
			Mode: 'overview',
			SelectedTool: {},
			SelectedPresentation: {},
			SelectedAssociatedVideo: {},
			SelectAllTools: false,
			SelectAllPresentations: false
		};

		$scope.toggleSelectedTools = function () {
		    for (var i = 0; i < $scope.page.Tools.length; i++) {
			    var tool = $scope.page.Tools[i];
				tool.toolIsSelected = $scope.settings.SelectAllTools;
				$scope.toggleToolSelection(tool);
			}
		}

		$scope.toggleSelectedPresentations = function () {
			for (var i = 0; i < $scope.page.Presentations.length; i++) {
			    var tool = $scope.page.Presentations[i];
				tool.toolIsSelected = $scope.settings.SelectAllPresentations;
				$scope.togglePresentationSelection(tool);
			}
		}

		$scope.toggleToolSelection = function (tool) {
			if (tool.toolIsSelected) {
				var isFound = false;
				for (var i = 0; i < $scope.SelectedTools.length; i++) {
				    if ($scope.SelectedTools[i] == tool.ToolFileName) {
						isFound = true;
						break;
					}
				}
				// only allow to be added once
				if (!isFound) {
				    $scope.SelectedTools.push(tool.ToolFileName);
				}
			} else {
				// remove tool
			    for (var i = 0; i < $scope.SelectedTools.length; i++) {
				    if ($scope.SelectedTools[i] == tool.ToolFileName) {
				        $scope.SelectedTools.splice(i, 1);
						break;
					}
				}
			}
		}

		$scope.togglePresentationSelection = function (tool) {
			if (tool.toolIsSelected) {
				var isFound = false;
				for (var i = 0; i < $scope.SelectedPresentations.length; i++) {
				    if ($scope.SelectedPresentations[i] == tool.ToolFileName) {
						isFound = true;
						break;
					}
				}
				// only allow to be added once
				if (!isFound) {
				    $scope.SelectedPresentations.push(tool.ToolFileName);
				}
			} else {
				// remove tool
			    for (var i = 0; i < $scope.SelectedPresentations.length; i++) {
			        if ($scope.SelectedPresentations[i] == tool.ToolFileName) {
			            $scope.SelectedPresentations.splice(i, 1);
						break;
					}
				}
			}
		}

		// go into edit mode
		$scope.edit = function () {
			$scope.settings.IsEditing = true;

			$scope.page.BriefDescriptionTemp = $scope.page.BriefDescription;
			$scope.page.TitleTemp = $scope.page.Title + '';
		}

		$scope.cancel = function () {
			$scope.settings.IsEditing = false;
		}

		$scope.removeTool = function (toolId) {
			$bootbox.confirm("Are you sure you want to remove this tool?  Note: The tool file is not deleted.", function (response) {
				if (response) {
					nsInterventionToolkitService.removePageTool($scope.page.Id, toolId)
					   .then(function (response) {
						   progressLoader.end();
						   nsPinesService.dataSavedSuccessfully();
						   LoadData();
					   });
				}
			})
		}

		$scope.removePresentation = function (toolId) {
		    $bootbox.confirm("Are you sure you want to remove this presentation?  Note: The presentation file is not deleted.", function (response) {
				if (response) {
					nsInterventionToolkitService.removePresentation($scope.page.Id, toolId)
					   .then(function (response) {
						   progressLoader.end();
						   nsPinesService.dataSavedSuccessfully();
						   LoadData();
					   });
				}
			})
		}

		$scope.removeVideo = function (videoId) {
			$bootbox.confirm("Are you sure you want to remove this video from this page?  Note: The video is not deleted.", function (response) {
				if (response) {
					nsInterventionToolkitService.removePageVideo($scope.page.Id, videoId)
					   .then(function (response) {
						   progressLoader.end();
						   nsPinesService.dataSavedSuccessfully();
						   LoadData();
					   });
				}
			})
		}

		$scope.save = function () {
			progressLoader.start();
			progressLoader.set(50);

			// call save function, show message, then
			$scope.page.BriefDescription = $scope.page.BriefDescriptionTemp; 

			nsInterventionToolkitService.savePage($scope.page)
				.then(function (response) {
					progressLoader.end();
					nsPinesService.dataSavedSuccessfully();

					// if we just saved a new one, redirect to new id
					if ($routeParams.id == -1) {
					    $location.path("view-mtsspage-cbm/" + response.data.id);
					} else {
						$scope.settings.IsEditing = false;
						LoadData();
					}
				}, function (error) {
					progressLoader.end();
				});
		}



		$scope.associateToolDialog = function () {
			var modalInstance = $uibModal.open({
				templateUrl: 'associatePageTool.html',
				scope: $scope,
				controller: function ($scope, $uibModalInstance) {

					$scope.associateTool = function () {
						var paramObj = { PageId: $scope.page.Id, ToolId: $scope.settings.SelectedTool.id };
						// start loader
						progressLoader.start();
						progressLoader.set(50);
						var promise = $http.post(webApiBaseUrl + '/api/interventiontoolkit/AssociatePageTool', paramObj).then(function (response) {
							// end loader
							progressLoader.end();
							$scope.errors = [];
							// show success
							$timeout(function () {
								$('#formReset').click();
							}, 100);

							LoadData();
							$scope.settings.SelectedTool = {};
							nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
						});

						$uibModalInstance.dismiss('cancel');
					};
					$scope.cancel = function () {
					    $scope.settings.SelectedTool = {};
						$uibModalInstance.dismiss('cancel');
					};
				},
				size: 'md',
			});
		}

		$scope.associatePresentationDialog = function () {
		    var modalInstance = $uibModal.open({
		        templateUrl: 'associatePresentation.html',
		        scope: $scope,
		        controller: function ($scope, $uibModalInstance) {

		            $scope.associatePresentation = function () {
		                var paramObj = { PageId: $scope.page.Id, PresentationId: $scope.settings.SelectedPresentation.id };
		                // start loader
		                progressLoader.start();
		                progressLoader.set(50);
		                var promise = $http.post(webApiBaseUrl + '/api/interventiontoolkit/AssociatePresentation', paramObj).then(function (response) {
		                    // end loader
		                    progressLoader.end();
		                    $scope.errors = [];
		                    // show success
		                    $timeout(function () {
		                        $('#formReset').click();
		                    }, 100);

		                    LoadData();
		                    $scope.settings.SelectedPresentation = {};
		                    nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
		                });

		                $uibModalInstance.dismiss('cancel');
		            };
		            $scope.cancel = function () {
		                $scope.settings.SelectedPresentation = {};
		                $uibModalInstance.dismiss('cancel');
		            };
		        },
		        size: 'md',
		    });
		}

		$scope.displayVideoDialog = function (video) {
			$scope.selectedVideo = video;
			var modalInstance = $uibModal.open({
				templateUrl: 'playVideo.html',
				scope: $scope,
				controller: function ($scope, $uibModalInstance, $sce) {

					$scope.getSrc = function () {
						if ($scope.selectedVideo) {
							return $sce.trustAsResourceUrl("https://view.vzaar.com/" + $scope.selectedVideo.VideoStreamId + "/player?apiOn=true");
						}
						return $sce.trustAsResourceUrl("about:blank");

					};
					$scope.cancel = function () {
						$scope.selectedVideo = null;
						$uibModalInstance.dismiss('cancel');
					};
				},
				size: 'lg',
			});
		}

		$scope.associateVideoDialog = function () {
			var modalInstance = $uibModal.open({
				templateUrl: 'associateVideo.html',
				scope: $scope,
				controller: function ($scope, $uibModalInstance) {

					$scope.associateVideo = function () {

						var paramObj = { PageId: $scope.page.Id, Video: $scope.settings.SelectedAssociatedVideo };
						// start loader
						progressLoader.start();
						progressLoader.set(50);
						var promise = $http.post(webApiBaseUrl + '/api/interventiontoolkit/associatePagevideo', paramObj).then(function (response) {
							// end loader
							progressLoader.end();
							$scope.errors = [];
							// show success
							$timeout(function () {
								$('#formReset').click();
							}, 100);

							$scope.settings.SelectedAssociatedVideo = {};
							LoadData();
							nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
						});

						$uibModalInstance.dismiss('cancel');
					};
					$scope.cancel = function () {
						$scope.settings.SelectedAssociatedVideo = {};
						$uibModalInstance.dismiss('cancel');
					};
				},
				size: 'md',
			});
		}

		$scope.associateInterventionToolDialog = function () {
			var modalInstance = $uibModal.open({
				templateUrl: 'associateInterventionTool.html',
				scope: $scope,
				controller: function ($scope, $uibModalInstance) {

					$scope.associateTool = function () {

						var paramObj = { InterventionId: $scope.Intervention.Id, InterventionToolId: $scope.settings.SelectedInterventionTool.id };
						// start loader
						progressLoader.start();
						progressLoader.set(50);
						var promise = $http.post(webApiBaseUrl + '/api/interventiontoolkit/AssociateTool', paramObj).then(function (response) {
							// end loader
							progressLoader.end();
							$scope.errors = [];
							// show success
							$timeout(function () {
								$('#formReset').click();
							}, 100);

							$scope.settings.SelectedInterventionTool = {};
							LoadData();
							nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
						});

						$uibModalInstance.dismiss('cancel');
					};
					$scope.cancel = function () {
						$scope.settings.SelectedInterventionTool = {};
						$uibModalInstance.dismiss('cancel');
					};
				},
				size: 'md',
			});
		}

		$scope.openUploadToolDialog = function () {

			var modalInstance = $uibModal.open({
				templateUrl: 'uploadTool.html',
				scope: $scope,
				controller: function ($scope, $uibModalInstance) {
					$scope.theFiles = [];

					$scope.upload = function (theFiles) {

						var formData = new FormData();
						formData.append("PageId", $routeParams.id);

						angular.forEach(theFiles, function (file) {
							formData.append(file.name, file);
						});
						var paramObj = {};
						// start loader
						progressLoader.start();
						progressLoader.set(50);
						var promise = $http.post(webApiBaseUrl + '/api/interventiontoolkit/uploadpagetool', formData, {
							transformRequest: angular.identity,
							headers: { 'Content-Type': undefined }
						}).then(function (response) {
							// end loader
							progressLoader.end();
							$scope.errors = [];
							// show success
							$timeout(function () {
								$('#formReset').click();
							}, 100);
							//$scope.theFiles.length = 0;
							//$scope.settings.hasFiles = false;
							$scope.uploadSettings.uploadComplete = true;

							LoadData();
							nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
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

		$scope.openUploadPresentationDialog = function () {

			var modalInstance = $uibModal.open({
				templateUrl: 'uploadTool.html',
				scope: $scope,
				controller: function ($scope, $uibModalInstance) {
					$scope.theFiles = [];

					$scope.upload = function (theFiles) {

						var formData = new FormData();
						formData.append("PageId", $routeParams.id);

						angular.forEach(theFiles, function (file) {
							formData.append(file.name, file);
						});
						var paramObj = {};
						// start loader
						progressLoader.start();
						progressLoader.set(50);
						var promise = $http.post(webApiBaseUrl + '/api/interventiontoolkit/uploadpresentation', formData, {
							transformRequest: angular.identity,
							headers: { 'Content-Type': undefined }
						}).then(function (response) {
							// end loader
							progressLoader.end();
							$scope.errors = [];
							// show success
							$timeout(function () {
								$('#formReset').click();
							}, 100);
							//$scope.theFiles.length = 0;
							//$scope.settings.hasFiles = false;
							$scope.uploadSettings.uploadComplete = true;

							LoadData();
							nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
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