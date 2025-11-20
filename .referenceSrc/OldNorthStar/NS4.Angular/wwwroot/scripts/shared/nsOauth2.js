(function () {
    'use strict';
    angular.module('nsOAuth2', [])
    .factory('authInterceptorService', ['$rootScope', '$cookies', '$q', '$injector', '$location', 'localStorageService', '$templateCache', function ($rootScope, $cookies, $q, $injector, $location, localStorageService, $templateCache) {

        var authInterceptorServiceFactory = {};
        var expired = function (token) {
            return (token && token.expires_at && new Date(token.expires_at) < new Date());
        };

        var _request = function (config) {

            config.headers = config.headers || {};
            var cookie = $cookies.get("Authorization");

            // TODO: get rid of this... SH on 1/7
            if (angular.isDefined(cookie)) {
                $rootScope.cookieDefined = 'yes';
            }
            else {
                $rootScope.cookieDefined = 'no';
            }

            // for print batching, see if token is passed in
            if (angular.isDefined(cookie) && cookie.indexOf('Bearer ') == 0) {
                var access_token = cookie.split('Bearer ')[1];
                //setExpiresAt()
                localStorageService.set('authorizationData', { token: access_token, expires_at: new Date().setYear(2100), userName: 'printer', refreshToken: "", useRefreshTokens: false });
            }

            var authData = localStorageService.get('authorizationData');
            if (authData) {
                // edge case fix when expiration date isn't set
                if (!angular.isDefined(authData.expires_at)) {
                    localStorageService.remove('authorizationData');
                    $location.path('/login');
                }

                // if token is expired, don't try to use it... send you to the login screen
                if (expired(authData) && $location.path() !== '/login') // no infinite loops
                {
                    $location.path('/login');
                }

                // don't try to use this on the login page
                if ($location.path() !== '/login') {
                    config.headers.Authorization = 'Bearer ' + authData.token;
                }
            } else { // if no login token yet, go get one
                // these are the only pages we can get to w/o a token
                var resetLinkPath = '/reset-password-from-link';
                if ($location.path() !== '/login' && $location.path() !== '/request-password-reset' && $location.path().slice(0, resetLinkPath.length) !== resetLinkPath) {
                    $location.path('/login');
                }
            }

            return config;
        }

        var _responseError = function (rejection) {
            if (rejection.status === 401) {
                var authService = $injector.get('authService');
                var authData = localStorageService.get('authorizationData');

                if (authData) {
                    //if (authData.useRefreshTokens) {
                    //    $location.path('/refresh');
                    //    return $q.reject(rejection);
                    //}
                }

                // if we are logged in with a good token and get a 401, this came from the application
                if (!expired(authData)) {
                    $location.path('/401');
                }
                else {
                    authService.logOut();
                    $location.path('/login');
                }
            } else if (rejection.status === 500 || rejection.status === 0 && (rejection.data === "" || rejection.data == null)) {
                var authData = localStorageService.get('authorizationData');

                // if we are logged in with a good token and get a 401, this came from the application
                if (!expired(authData)) {
                    // let's try a popup instead
                    var bootbox = $injector.get('$bootbox');
                    //var modalInstance = uibModal.open({
                    //    templateUrl: 'templates/error.html',
                    //    size: 'lg',
                    //});
                    var message = $templateCache.get('templates/error.html');


                    bootbox.alert(message);
                    //$location.path('/extras-500');
                }
                else {
                    authService.logOut();
                    $location.path('/login');
                }
            } else if (rejection.status === 400) { // user displayble error, broadcast it
                $rootScope.$broadcast("NSHTTPError", rejection.data);
            }
            return $q.reject(rejection);
        }

        authInterceptorServiceFactory.request = _request;
        authInterceptorServiceFactory.responseError = _responseError;

        return authInterceptorServiceFactory;
    }])
    .factory('authService', ['$http', '$q', 'localStorageService', 'ngAuthSettings', '$location', '$global', function ($http, $q, localStorageService, ngAuthSettings, $location, $global) {

        var serviceBase = ngAuthSettings.apiServiceBaseUri;
        var authServiceFactory = {};

        var _authentication = {
            isAuth: false,
            userName: "",
            useRefreshTokens: false
        };

        function setExpiresAt(token) {
            if (token) {
                var expires_at = new Date();
                expires_at.setSeconds(expires_at.getSeconds() + parseInt(token.expires_in) - 60); // 60 seconds less to secure browser and response latency
                token.expires_at = expires_at;
            }
        }

        var _externalAuthData = {
            provider: "",
            userName: "",
            externalAccessToken: ""
        };

        var _token = function () {
            var authData = localStorageService.get('authorizationData');
            if (authData) {
                return authData.token;
            }
            return null;
        }

        var _saveRegistration = function (registration) {

            _logOut();

            return $http.post(serviceBase + 'api/account/register', registration).then(function (response) {
                return response;
            });

        };

        var _login = function (loginData) {

            var data = "scope=idmgr&grant_type=password&username=" + loginData.userName + "&password=" + loginData.password + "&client_id=" + ngAuthSettings.clientId + "&client_secret=secret&acr_values=" + loginData.alternate;

            //if (loginData.useRefreshTokens) {
            //    data = data + "&client_id=" + ngAuthSettings.clientId;
            //}


            var deferred = $q.defer();

            $http.post(serviceBase + 'token', data, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }).success(function (response) {

                setExpiresAt(response);

                if (loginData.useRefreshTokens) {
                    localStorageService.set('authorizationData', { token: response.access_token, userName: loginData.userName, refreshToken: response.refresh_token, useRefreshTokens: true });
                }
                else {
                    localStorageService.set('authorizationData', { token: response.access_token, expires_at: response.expires_at, userName: loginData.userName, refreshToken: "", useRefreshTokens: false });
                }
                _authentication.isAuth = true;
                _authentication.userName = loginData.userName;
                _authentication.useRefreshTokens = loginData.useRefreshTokens;

                deferred.resolve(response);

            }).error(function (err, status) {
                _logOut();
                deferred.reject(err);
            });

            return deferred.promise;

        };

        var _logOut = function () {
            $global.set('destroynavigation', true);
            localStorageService.remove('authorizationData');
            _authentication.isAuth = false;
            _authentication.userName = "";
            _authentication.useRefreshTokens = false;
            $location.path('/login');
        };

        var _fillAuthData = function () {

            var authData = localStorageService.get('authorizationData');
            if (authData) {
                _authentication.isAuth = true;
                _authentication.userName = authData.userName;
                _authentication.useRefreshTokens = authData.useRefreshTokens;
            }

        };

        var _refreshToken = function () {
            var deferred = $q.defer();

            var authData = localStorageService.get('authorizationData');

            if (authData) {

                if (authData.useRefreshTokens) {

                    var data = "grant_type=refresh_token&refresh_token=" + authData.refreshToken + "&client_id=" + ngAuthSettings.clientId;

                    localStorageService.remove('authorizationData');

                    $http.post(serviceBase + 'token', data, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }).success(function (response) {

                        localStorageService.set('authorizationData', { token: response.access_token, userName: response.userName, refreshToken: response.refresh_token, useRefreshTokens: true });

                        deferred.resolve(response);

                    }).error(function (err, status) {
                        _logOut();
                        deferred.reject(err);
                    });
                }
            }

            return deferred.promise;
        };

        var _obtainAccessToken = function (externalData) {

            var deferred = $q.defer();

            $http.get(serviceBase + 'api/account/ObtainLocalAccessToken', { params: { provider: externalData.provider, externalAccessToken: externalData.externalAccessToken } }).success(function (response) {

                localStorageService.set('authorizationData', { token: response.access_token, userName: response.userName, refreshToken: "", useRefreshTokens: false });

                _authentication.isAuth = true;
                _authentication.userName = response.userName;
                _authentication.useRefreshTokens = false;

                deferred.resolve(response);

            }).error(function (err, status) {
                _logOut();
                deferred.reject(err);
            });

            return deferred.promise;

        };

        var _registerExternal = function (registerExternalData) {

            var deferred = $q.defer();

            $http.post(serviceBase + 'api/account/registerexternal', registerExternalData).success(function (response) {

                localStorageService.set('authorizationData', { token: response.access_token, userName: response.userName, refreshToken: "", useRefreshTokens: false });

                _authentication.isAuth = true;
                _authentication.userName = response.userName;
                _authentication.useRefreshTokens = false;

                deferred.resolve(response);

            }).error(function (err, status) {
                _logOut();
                deferred.reject(err);
            });

            return deferred.promise;

        };

        authServiceFactory.saveRegistration = _saveRegistration;
        authServiceFactory.login = _login;
        authServiceFactory.logOut = _logOut;
        authServiceFactory.fillAuthData = _fillAuthData;
        authServiceFactory.authentication = _authentication;
        authServiceFactory.refreshToken = _refreshToken;

        authServiceFactory.obtainAccessToken = _obtainAccessToken;
        authServiceFactory.externalAuthData = _externalAuthData;
        authServiceFactory.registerExternal = _registerExternal;
        authServiceFactory.token = _token;

        return authServiceFactory;
    }])
    .factory('tokensManagerService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {

        var serviceBase = ngAuthSettings.apiServiceBaseUri;

        var tokenManagerServiceFactory = {};

        var _getRefreshTokens = function () {

            return $http.get(serviceBase + 'api/refreshtokens').then(function (results) {
                return results;
            });
        };

        var _deleteRefreshTokens = function (tokenid) {

            return $http.delete(serviceBase + 'api/refreshtokens/?tokenid=' + tokenid).then(function (results) {
                return results;
            });
        };

        tokenManagerServiceFactory.deleteRefreshTokens = _deleteRefreshTokens;
        tokenManagerServiceFactory.getRefreshTokens = _getRefreshTokens;

        return tokenManagerServiceFactory;

    }]);
})();