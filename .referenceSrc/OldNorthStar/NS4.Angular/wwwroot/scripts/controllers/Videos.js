(function () {
    'use strict'
     
    angular
	.module('videosModule', [])
    .service('NSVideosService', [
			'$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
			    //this.options = {};
			    var self = this;

			    self.getVideosByPage = function (pageNumber) {
			        var paramObj = { Id: pageNumber };
			        var url = webApiBaseUrl + '/api/video/GetVzaarVideoList/';
			        var promise = $http.post(url, paramObj);

			        return promise;
			    }
			    //
			    self.loadVideosForDistrict = function (gradeId) {
			        var paramObj = { Id: gradeId };
			        var url = webApiBaseUrl + '/api/video/GetDistrictVideos';
			        var promise = $http.post(url, paramObj);

			        return promise;
			    }

			    self.loadAllVideos = function () {
			        var url = webApiBaseUrl + '/api/video/GetAllVideos';
			        var promise = $http.get(url);

			        return promise;
			    }


			    self.saveVideo = function (video) {
			        var paramObj = { Video: video };
			        var url = webApiBaseUrl + '/api/interventiontoolkit/SaveVideo';
			        var promise = $http.post(url, paramObj);

			        return promise;
			    }

			    self.removeVideo = function (video) {
			        var paramObj = { Id: video.Id };
			        var url = webApiBaseUrl + '/api/video/RemoveVideo';
			        var promise = $http.post(url, paramObj);

			        return promise;
			    }
			}]
	)
	.controller('ManageVideosController', [
        '$bootbox', 'nsPinesService', 'NSVideosService', '$scope', 'nsSelect2RemoteOptions', '$uibModal', 'spinnerService', '$timeout', function ($bootbox, nsPinesService, NSVideosService, $scope, nsSelect2RemoteOptions, $uibModal, spinnerService, $timeout) {
            
            $scope.nsSelect2RemoteOptions = nsSelect2RemoteOptions;
            $scope.unsavedVideo = null;

            $scope.newVideo = function () {
                $scope.unsavedVideo = { Id: -1, UploadedVideoFile: {}, VideoName: 'New Video' };
            }

            $scope.removeVideo = function (video) {
                $bootbox.confirm("Are you sure you want to delete this video?", function(response){
                 if(response){
                     NSVideosService.removeVideo(video).finally(function (response) {
                         nsPinesService.dataDeletedSuccessfully();
                         LoadVideos();
                     })
                    }   
                });
            }

            $scope.cancelNewRowEdit = function (rowform) {
                rowform.$cancel();
                $scope.unsavedVideo = null;
            }

            $scope.displayVideoDialog = function (video, $event) {
                $event.preventDefault();
                $event.stopPropagation();
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

            var LoadVideos = function () {
                $timeout(function () {
                    spinnerService.show('tableSpinner');
                });
                NSVideosService.loadAllVideos()
                    .then(function (response) {
                        $scope.videos = response.data.Videos;
                    })
                    .finally(function () {
                        spinnerService.hide('tableSpinner');
                    });
            }

            $scope.saveVideo = function (video) {
                NSVideosService.saveVideo(video)
                .then(function (response) {
                    $scope.unsavedVideo = null;
                    nsPinesService.dataSavedSuccessfully();
                    LoadVideos();
                });
            };


            LoadVideos();
        }        
	])
    	.controller('ViewVideosController', [
        '$bootbox', 'nsPinesService', 'NSVideosService', '$scope', '$uibModal', 'nsSelect2RemoteOptions', function ($bootbox, nsPinesService, NSVideosService, $scope, $uibModal, nsSelect2RemoteOptions) {
            $scope.settings = { selectedGradeId: null, currentPage: 1 };
            $scope.galleryFilter = 'all';
            $scope.remoteOptions = nsSelect2RemoteOptions;

            $scope.pageChanged = function (page) {

            }

            $scope.displayVideoDialog = function (video, $event) {
                $event.preventDefault();
                $event.stopPropagation();
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

            NSVideosService.loadVideosForDistrict($scope.settings.selectedGradeId).then(function (response) {
                $scope.videos = response.data.Videos;

                for (var i = 0; i < $scope.videos.length; i++) {
                    $scope.videos[i].GradeGroup = [];
                    for (var j = 0; j < $scope.videos[i].Grades.length; j++) {
                        $scope.videos[i].GradeGroup.push($scope.videos[i].Grades[j].id + '');
                    }
                }
            });
        }
    	])
    
})();