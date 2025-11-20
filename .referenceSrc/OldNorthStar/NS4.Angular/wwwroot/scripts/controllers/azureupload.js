(function () {
    'use strict';

    angular.module('app.photo', []);
})();
(function () {
    'use strict';

    angular
        .module('app.photo')
        .directive('egFiles', egFiles);

    function egFiles() {

        var directive = {
            link: link,
            restrict: 'A',
            scope: {
                files: '=egFiles',
                hasFiles: '='
            }
        };
        return directive;

        function link(scope, element, attrs) {
            element.bind('change', function () {
                scope.$apply(function () {
                    if (element[0].files) {
                        scope.files.length = 0;

                        angular.forEach(element[0].files, function (f) {
                            scope.files.push(f);
                        });

                        scope.hasFiles = true;
                    }
                });
            });

            if (element[0].form) {
                angular.element(element[0].form)
                        .bind('reset', function () {
                            scope.$apply(function () {
                                scope.files.length = 0;
                                scope.hasFiles = false;
                            });
                        });
            }
        }
    }
})();
(function () {
    'use strict';

    angular
        .module('app.photo')
        .directive('egPhotoUploader', egPhotoUploader);

    egPhotoUploader.$inject = ['appInfo', 'photoManager', '$templateCache'];

    function egPhotoUploader(appInfo, photoManager, $templateCache) {

        var directive = {
            link: link,
            restrict: 'E',
            template: $templateCache.get('templates/egphotouploader.html'),
            scope: true
        };
        return directive;

        function link(scope, element, attrs) {
            scope.hasFiles = false;
            scope.photos = [];
            scope.upload = photoManager.upload;
            scope.appStatus = appInfo.status;
            scope.photoManagerStatus = photoManager.status;
        }
    }

})();
(function () {
    'use strict';

    angular
        .module('app.photo')
        .directive('egUpload', egUpload);

    egUpload.$inject = ['$timeout'];

    function egUpload($timeout) {

        var directive = {
            link: link,
            restrict: 'A',
            scope: {
                upload: '&egUpload'
            }
        };
        return directive;

        function link(scope, element, attrs) {
            var parentForm = element[0].form;
            if (parentForm) {
                element.on('click', function (event) {
                    return scope.upload().then(function () {
                        //see:https://docs.angularjs.org/error/$rootScope/inprog?p0=$digest for why there is a need to use timeout to avoid conflict
                        $timeout(function () {
                            parentForm.reset();
                        });
                    });
                });
            }
        }
    }
})();
(function () {
    'use strict';

    angular
        .module('app.photo')
        .factory('photoManager', photoManager);

    photoManager.$inject = ['$q', 'photoManagerClient', 'appInfo'];

    function photoManager($q, photoManagerClient, appInfo) {
        var service = {
            photos: [],
            load: load,
            upload: upload,
            remove: remove,
            photoExists: photoExists,
            status: {
                uploading: false
            }
        };

        return service;

        function load() {
            appInfo.setInfo({ busy: true, message: "loading photos" })

            service.photos.length = 0;

            return photoManagerClient.query()
                                .$promise
                                .then(function (result) {
                                    result.photos
                                            .forEach(function (photo) {
                                                service.photos.push(photo);
                                            });

                                    appInfo.setInfo({ message: "photos loaded successfully" });

                                    return result.$promise;
                                },
                                function (result) {
                                    appInfo.setInfo({ message: "something went wrong: " + result.data.message });
                                    return $q.reject(result);
                                })
                                ['finally'](
                                function () {
                                    appInfo.setInfo({ busy: false });
                                });
        }

        function upload(photos) {
            service.status.uploading = true;
            appInfo.setInfo({ busy: true, message: "uploading photos" });

            var formData = new FormData();

            angular.forEach(photos, function (photo) {
                formData.append(photo.name, photo);
            });

            return photoManagerClient.save(formData)
                                        .$promise
                                        .then(function (result) {
                                            if (result && result.photos) {
                                                result.photos.forEach(function (photo) {
                                                    if (!photoExists(photo.name)) {
                                                        service.photos.push(photo);
                                                    }
                                                });
                                            }

                                            appInfo.setInfo({ message: "photos uploaded successfully" });

                                            return result.$promise;
                                        },
                                        function (result) {
                                            appInfo.setInfo({ message: "something went wrong: " + result.data.message });
                                            return $q.reject(result);
                                        })
                                        ['finally'](
                                        function () {
                                            appInfo.setInfo({ busy: false });
                                            service.status.uploading = false;
                                        });
        }

        function remove(photo) {
            appInfo.setInfo({ busy: true, message: "deleting photo " + photo.name });

            return photoManagerClient.remove({ fileName: photo.name })
                                        .$promise
                                        .then(function (result) {
                                            //if the photo was deleted successfully remove it from the photos array
                                            var i = service.photos.indexOf(photo);
                                            service.photos.splice(i, 1);

                                            appInfo.setInfo({ message: "photos deleted" });

                                            return result.$promise;
                                        },
                                        function (result) {
                                            appInfo.setInfo({ message: "something went wrong: " + result.data.message });
                                            return $q.reject(result);
                                        })
                                        ['finally'](
                                        function () {
                                            appInfo.setInfo({ busy: false });
                                        });
        }

        function photoExists(photoName) {
            var res = false
            service.photos.forEach(function (photo) {
                if (photo.name === photoName) {
                    res = true;
                }
            });

            return res;
        }
    }
})();
(function () {
    'use strict';

    angular
        .module('app.photo')
        .factory('photoManagerClient', photoManagerClient);

    photoManagerClient.$inject = ['$resource','webApiBaseUrl'];

    function photoManagerClient($resource, webApiBaseUrl) {
        return $resource(webApiBaseUrl + "/api/fileuploader/:fileName",
                { id: "@fileName" },
                {
                    'query': { method: 'GET' },
                    'save': { method: 'POST', transformRequest: angular.identity, headers: { 'Content-Type': undefined } },
                    'remove': { method: 'DELETE', url: webApiBaseUrl + '/api/fileuploader/:fileName', params: { name: '@fileName' } }
                });
    }
})();
(function () {
    'use strict';

    angular
        .module('app.photo')
        .controller('photos', photos);

    photos.$inject = ['$scope', 'photoManager'];

    function photos($scope, photoManager) {
        /* jshint validthis:true */
        var vm = this;
        vm.title = 'photo manager';
        vm.photos = photoManager.photos;
        vm.uploading = false;
        vm.previewPhoto;
        vm.remove = photoManager.remove;
        vm.setPreviewPhoto = setPreviewPhoto;

        activate();

        function activate() {
            photoManager.load();
        }

        function setPreviewPhoto(photo) {
            vm.previewPhoto = photo
        }

        function remove(photo) {
            photoManager.remove(photo).then(function () {
                setPreviewPhoto();
            });
        }
    }
})();
(function () {
    'use strict';

    angular
        .module('app.photo')
        .factory('appInfo', appInfo);

    function appInfo() {
        var service = {
            status: {
                busy: false,
                message: ''
            },
            setInfo: setInfo
        };

        return service;

        function setInfo(args) {
            if (args) {
                if (args.hasOwnProperty('busy')) {
                    service.status.busy = args.busy;
                }
                if (args.hasOwnProperty('message')) {
                    service.status.message = args.message;
                }
            } else {
                service.status.busy = false;
                service.status.message = '';
            }
        }
    }
})();
(function () {
    'use strict';

    angular
        .module('app.photo')
        .directive('egAppStatus', egAppStatus);

    egAppStatus.$inject = ['appInfo'];

    function egAppStatus(appInfo) {
        var directive = {
            link: link,
            restrict: 'E',
            templateUrl: 'templates/egAppStatus.html'
        };
        return directive;

        function link(scope, element, attrs) {
            scope.status = appInfo.status;
        }
    }

})();