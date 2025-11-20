(function () {
    'use strict';

    angular
        .module('loginModule', [])
        .controller('LoginController', LoginController);

    LoginController.$inject = ['$scope', '$global', '$http', 'authService', 'ngAuthSettings','$location','progressLoader', '$cookies'];


    function LoginController($scope, $global, $http, authService, ngAuthSettings, $location, progressLoader, $cookies) {
        $global.set('fullscreen', true);

        var vm = this;

        vm.showAlternate = function () {
            if ($location.search().impersonate) {
                return true;
            }

            return false;
        }



        vm.message = "";
        vm.loginData = {
            userName: "",
            password: "",
            remember: false,
            useRefreshTokens: false
        };

        // read cookie... if set, store store it in vm.loginData.userName
        var favoriteCookie = $cookies.get('NSLogin');

        if (favoriteCookie) {
            vm.loginData.userName = favoriteCookie;
            vm.loginData.remember = true;
        }

        $scope.errors = [];
        vm.login = function () {
            progressLoader.start();
            progressLoader.set(50);
            authService.login(vm.loginData)
                .then(function (response) {
                    if (vm.loginData.remember) {
                        $cookies.put('NSLogin', vm.loginData.userName);
                    } else {
                        $cookies.remove('NSLogin');
                    }

                    $location.path('/');
                    progressLoader.end();
            });
        };

        $scope.$on('$destroy', function () {
            $global.set('fullscreen', false);
        });

        $scope.$on('NSHTTPError', function (event, data) {
            progressLoader.end();
            $scope.errors = [];
            if (data && data.error_description) {
                $scope.errors.push({ type: "danger", msg: data.error_description });
            } else {
                $scope.errors.push({ type: "danger", msg: 'Invalid username or password. Please try again.' });
            }
            $('html, body').animate({ scrollTop: 0 }, 'fast');
        });

        //vm.login = function () {
        //    var client_id = "roclient";
        //    var client_secret = "secret";

        //    var url = "http://localhost:16725/identity/connect/token";
        //    var data = {
        //        username: vm.email,
        //        password: vm.password,
        //        grant_type: "password",
        //        scope: "read write",
        //        client_id: client_id,
        //        client_secret: client_secret
        //    };
        //    var body = "";
        //    for (var key in data) {
        //        if (body.length) {
        //            body += "&";
        //        }
        //        body += key + "=";
        //        body += encodeURIComponent(data[key]);
        //    }

        //    var req = {
        //        headers: {
        //            'Content-Type': "application/x-www-form-urlencoded",
        //            "Authorization": "Basic " + btoa(client_id + ":" + client_secret)
        //        }
        //    }
        //    var responseData = $http.post(url, body, req)

        //    responseData.then(function(data) {
        //        var b = data;
        //    },
        //    function(error) {
        //        var msg = error;
        //    });
        //}

        vm.reset = function ()
        {
            vm.email = '';
            vm.password = '';
        }
    }


})();