(function () {
    'use strict'

    angular
	.module('districtDashboardsModule', [])
    .service('NSDistrictDashboardMCAService', [
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
	.controller('DistrictDashboardMCAController', [
        '$bootbox', 'nsPinesService', 'NSDistrictDashboardMCAService', '$scope', function ($bootbox, nsPinesService, NSDistrictDashboardMCAService, $scope) {

        }
	])

})();