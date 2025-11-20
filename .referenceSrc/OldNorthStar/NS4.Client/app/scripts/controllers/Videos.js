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


			}]
	)
	.controller('ManageVideosController', [
        '$bootbox', 'nsPinesService', 'NSVideosService', '$scope', function ($bootbox, nsPinesService, NSVideosService, $scope) {
            NSVideosService.getVideosByPage(1).then(function (response) {
                $scope.videos = response.data.Videos;
            })
        }        
	])
    
})();