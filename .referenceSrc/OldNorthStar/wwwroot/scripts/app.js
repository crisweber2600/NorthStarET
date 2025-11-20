(function () {


    'use strict';

    var mainModule = angular
      .module('themesApp', [
          'LocalStorageModule',
          'loginModule',
          'personalSettingsModule',
          'teamMeetingModule',
          'northstar.business',
          'remoteValidation2',
          'angularValidator',
          'districtSettingsModule',
        'highcharts-ng',
        'uiSwitch',
        'ngCookies',
        'ngStorage',
        'nsOAuth2',
        //'pikaday',
        'angular.datepicker',
        'lr.upload',
        'easypiechart',
        'ui.bootstrap',
        'ui.tree',
        'ui.select2',
        'ngGrid',
        'xeditable',
        'theme.services',
        'theme.directives',
        'highcharts-ng',
        'benchmarksModule',
        'staffModule',
        'assessmentModule',
        'hfwReportsModule',
        'videosModule',
        'filterOptionsModule',
        'assessmentFieldsModule',
        'sectionModule',
        'studentModule',
        'sectionReportsModule',
        'sectionDataEntryModule',
        'interventionGroupDataEntryModule',
        'studentDashboardModule',
        'interventionDashboardModule',
        'stateTestDataModule',
        'availableAssessmentsModule',
        'calendarModule',
        'interventionGroupModule',
        'interventionToolkitModule',
        'observationSummaryModule',
        'lineGraphModule',
        'stackedBarGraphModule',
        'theme.navigation-controller',
        'theme.notifications-controller',
        'theme.messages-controller',
        'theme.colorpicker-controller',
        'theme.layout-horizontal',
        'theme.layout-boxed',
        //'theme.vector_maps',
        //'theme.google_maps',
        'theme.calendars',
        'theme.gallery',
        'theme.tasks',
        'theme.ui-tables-basic',
        'theme.ui-panels',
        'theme.ui-ratings',
        'theme.ui-modals',
        'theme.ui-tiles',
        'theme.ui-alerts',
        'theme.ui-sliders',
        'theme.ui-progressbars',
        'theme.ui-paginations',
        'theme.ui-carousel',
        'theme.ui-tabs',
        'theme.ui-nestable',
        'theme.form-components',
        'theme.form-directives',
        'theme.form-validation',
        'theme.form-inline',
        'theme.form-image-crop',
        'theme.tables-ng-grid',
        'theme.tables-editable',
        'theme.charts-flot',
        'theme.charts-canvas',
        'theme.charts-svg',
        'theme.charts-inline',
        'theme.pages-controllers',
        'theme.dashboard',
        'theme.templates',
        'theme.template-overrides',
        'ngCookies',
        'ngResource',
        'ngSanitize',
        'ngRoute',
        'ngAnimate',
        'app.photo',
        'ngFileSaver',
        'districtDashboardsModule',
        'schoolDashboardsModule',
        'dataExportModule',
        'rolloverModule',
        'angularSpinners'
      ])
        .constant("nsGlobalSettings", {
            ckEditorDefaultConfig: {
                toolbarGroups:
                    [
                    { name: 'editing', groups: ['spellchecker'] },
                    { name: 'insert' },
                    { name: 'tools' },
                    { name: 'basicstyles', groups: ['basicstyles', 'cleanup'] }
                    ]
            },
            ckEditorFullConfig: {
                toolbarGroups:
                  [

                  { name: 'editing', groups: ['find', 'selection', 'spellchecker'] },
                  { name: 'links' },
                  { name: 'insert' },
                  { name: 'forms' },
                  { name: 'tools' },
                  { name: 'document', groups: ['mode', 'document', 'doctools'] },
                  { name: 'others' },
                  '/',
                  { name: 'basicstyles', groups: ['basicstyles', 'cleanup'] },
                  { name: 'paragraph', groups: ['list', 'indent', 'blocks', 'align', 'bidi'] },
                  { name: 'styles' },
                  { name: 'colors' },
                  { name: 'about' }
                  ]
            }
        })
      //.constant("webApiBaseUrl", "http://localhost:16726")
      //  .constant('ngAuthSettings', {
      //      apiServiceBaseUri: "http://localhost:16725/connect/",
      //      clientId: 'roclient'
      //  })
      //  .constant("ckEditorSettings", {
      //      imageUploadUrl : "http://localhost:16726/api/fileuploader/uploadimages",
      //      uploadUrl: "http://localhost:16726/api/fileuploader/uploadimages",
      //      filebrowserImageUploadUrl: "http://localhost:16726/api/fileuploader/uploadimagesjsResponse"
      //  })
      //  .constant("ckEditorSettingsAdmin", {
      //      imageUploadUrl: "http://localhost:16726/api/fileuploader/uploadimagesadmin",
      //      uploadUrl: "http://localhost:16726/api/fileuploader/uploadimagesadmin",
      //      filebrowserImageUploadUrl: "http://localhost:16726/api/fileuploader/uploadimagesjsResponseadmin"
      //  })
      //    .constant("webApiBaseUrl", "https://api.northstaret.net")
      //  .constant("ckEditorSettings", {
      //      imageUploadUrl : "https://api.northstaret.net/api/fileuploader/uploadimages",
      //      uploadUrl: "https://api.northstaret.net/api/fileuploader/uploadimages",
      //      filebrowserImageUploadUrl: "https://api.northstaret.net/api/fileuploader/uploadimagesjsResponse"
      //  })
      //      .constant("ckEditorSettingsAdmin", {
      //      imageUploadUrl: "https://api.northstaret.net/api/fileuploader/uploadimagesadmin",
      //      uploadUrl: "https://api.northstaret.net/api/fileuploader/uploadimagesadmin",
      //      filebrowserImageUploadUrl: "https://api.northstaret.net/api/fileuploader/uploadimagesjsResponseadmin"
      //  })
      //.constant('ngAuthSettings', {
      //    apiServiceBaseUri: "https://identity.northstaret.net/connect/",
      //    clientId: 'roclient'
      //})
      .filter('unsafe', ['$sce', function ($sce) {
          return function (val) {
              return $sce.trustAsHtml(val);
          };
      }])
        .filter('nsDateFormat', [function () {
            return function (val) {
                return val ? moment(val).format('DD-MMM-YYYY') : null;
            };
        }])
            .filter('nsLongDateFormat', [function () {
                return function (val) {
                    return val ? moment(val).format('dddd, MMMM DD, YYYY') : null;
                };
            }])
            .filter('nsDateFormatWithTime', [function () {
                return function (val) {
                    return val ? moment(val).format('MMM DD, YYYY h:mm a') : null;
                };
            }])
            .filter('nsLongDateFormatWithTime', [function () {
                return function (val) {
                    return val ? moment(val).format('dddd, MMMM DD, YYYY h:mm a') : null;
                };
            }])
            .directive('nsHelp', ['$templateCache', '$compile', 'NSHelpManager', 'NSUserInfoService', 'nsPinesService', '$uibModal', 'ckEditorSettingsAdmin', 'nsGlobalSettings',
                function ($templateCache, $compile, NSHelpManager, NSUserInfoService, nsPinesService, $uibModal, ckEditorSettingsAdmin, nsGlobalSettings) {

                    var getTemplate = function (type) {
                        var templateName = '';

                        if (type == 'html') {
                            return 'templates/nshelp-text.html';
                        } else {
                            return 'templates/nshelp-modal.html';
                        }
                    }

                    return {
                        restrict: 'AE',
                        scope: {
                            helpField: '=',
                            helpFormat: '='
                        },
                        link: function (scope, element, attrs) {
                            scope.helpManager = new NSHelpManager(scope.helpField);
                            scope.settings = { editMode: false };
                            scope.currentUser = NSUserInfoService.currentUser;

                            scope.ckeditorOptions = {
                                language: 'en',
                                allowedContent: true,
                                entities: false,
                                uploadUrl: ckEditorSettingsAdmin.uploadurl,
                                imageUploadUrl: ckEditorSettingsAdmin.imageUploadUrl,
                                filebrowserImageUploadUrl: ckEditorSettingsAdmin.filebrowserImageUploadUrl,
                                toolbarGroups: nsGlobalSettings.ckEditorFullConfig.toolbarGroups
                            };

                            scope.enableEdit = function () {
                                scope.settings.editMode = true;
                            }

                            scope.cancelEdit = function () {
                                scope.settings.editMode = false;
                            }

                            scope.openModal = function () {
                                var modalInstance = $uibModal.open({
                                    templateUrl: 'genericModal.html',
                                    scope: scope,
                                    controller: function ($scope, $uibModalInstance) {
                                        $scope.heading = 'Help';
                                        $scope.body = $scope.helpManager.fieldValue
                                        $scope.cancel = function () {
                                            $uibModalInstance.dismiss('cancel');
                                        };
                                    },
                                    size: 'md',
                                });
                            }

                            scope.saveData = function () {
                                scope.helpManager.saveData().then(function (response) {
                                    nsPinesService.dataSavedSuccessfully();
                                    scope.settings.editMode = false;
                                })
                            }

                            var template = $templateCache.get(getTemplate(scope.helpFormat));
                            element.html(template);
                            $compile(element.contents())(scope);
                        }
                    };
                }])
        .factory('NSHelpManager', ['$http', 'webApiBaseUrl', '$location', function ($http, webApiBaseUrl, $location) {

            var NSHelpManager = function (fieldName) {
                var self = this;


                self.path = $location.path();
                self.fieldName = fieldName;
                self.fieldValue = '';

                self.initialize = function () {
                    var paramObj = { path: self.path, field: self.fieldName };

                    var promise = $http.post(webApiBaseUrl + '/api/navigation/gethelp', paramObj);

                    promise.then(function (response) {
                        self.fieldValue = response.data.text;
                    });
                }

                self.saveData = function () {
                    var paramObj = { path: self.path, field: self.fieldName, text: self.fieldValue };

                    var promise = $http.post(webApiBaseUrl + '/api/navigation/savehelp', paramObj);

                    return promise;
                }

                self.initialize();
            }

            return (NSHelpManager);
        }])
      .controller('MainController', ['$scope', '$global', '$timeout', 'progressLoader', '$location', 'authService', '$window', '$http', 'webApiBaseUrl', '$bootbox', 'nsPinesService', '$rootScope',
          function ($scope, $global, $timeout, progressLoader, $location, authService, $window, $http, webApiBaseUrl, $bootbox, nsPinesService, $rootScope) {
          var mc = self;

          $scope.style_fixedHeader = $global.get('fixedHeader');
          $scope.style_headerBarHidden = $global.get('headerBarHidden');
          $scope.style_layoutBoxed = $global.get('layoutBoxed');
          $scope.style_fullscreen = $global.get('fullscreen');
          $scope.style_leftbarCollapsed = $global.get('leftbarCollapsed');
          $scope.style_leftbarShown = $global.get('leftbarShown');
          $scope.style_rightbarCollapsed = $global.get('rightbarCollapsed');
          $scope.style_isSmallScreen = false;
          $scope.style_showSearchCollapsed = $global.get('showSearchCollapsed');
          $scope.style_layoutHorizontal = $global.get('layoutHorizontal');
          $scope.currentYear = new Date().getFullYear();
          //$scope.currentUser = nsUserInfo.currentUser;


          $scope.hideSearchBar = function () {
              $global.set('showSearchCollapsed', false);
          };

          $scope.hideHeaderBar = function () {
              $global.set('headerBarHidden', true);
          };

          $scope.showHeaderBar = function ($event) {
              $event.stopPropagation();
              $global.set('headerBarHidden', false);
          };

          $scope.toggleLeftBar = function () {
              if ($scope.style_isSmallScreen) {
                  return $global.set('leftbarShown', !$scope.style_leftbarShown);
              }
              $global.set('leftbarCollapsed', !$scope.style_leftbarCollapsed);
          };

          $scope.toggleRightBar = function () {
              $global.set('rightbarCollapsed', !$scope.style_rightbarCollapsed);
          };

          $scope.$on('globalStyles:changed', function (event, newVal) {
              $scope['style_' + newVal.key] = newVal.value;
          });
          $scope.$on('globalStyles:maxWidth767', function (event, newVal) {
              $timeout(function () {
                  $scope.style_isSmallScreen = newVal;
                  if (!newVal) {
                      $global.set('leftbarShown', false);
                  } else {
                      $global.set('leftbarCollapsed', false);
                  }
              });
          });

          // there are better ways to do this, e.g. using a dedicated service
          // but for the purposes of this demo this will do :P
          $scope.isLoggedIn = true;
          $scope.logOut = function () {
              authService.logOut();
          };
          //$scope.logIn = function () {
          //  $scope.isLoggedIn = true;
          //};

          $scope.rightbarAccordionsShowOne = false;
          $scope.rightbarAccordions = [{ open: true }, { open: true }, { open: true }, { open: true }, { open: true }, { open: true }, { open: true }];

          $scope.$on('$routeChangeStart', function (e) {
              // console.log('start: ', $location.path());
              progressLoader.start();
              progressLoader.set(50);

              var resetLinkPath = '/reset-password-from-link';

              // don't check on pages where you're not logged in yet
              if (
                  $location.path() !== '/login' &&
                  $location.path() !== '/401' &&
                  $location.path() !== '/request-password-reset' &&
                  $location.path().slice(0, resetLinkPath.length) !== resetLinkPath) {
                  $http.get(webApiBaseUrl + '/api/PersonalSettings/GetUserCurrentVersion')
                      .then(function (response) {
                          // set current NS version global variable
                          $rootScope.NSVersion = response.data.CurrentNSVersion || 0;

                          if ((response.data.UserCurrentVersion == null || response.data.UserCurrentVersion < $rootScope.NSVersion) && $rootScope.NSVersion != 0) {
                              $bootbox.confirm('You must update your version of North Star to continue.  <br><br><b>Please click OK to update to the latest version.</b>', function (response) {
                                  if (response) {
                                      $http.post(webApiBaseUrl + '/api/PersonalSettings/StartVersionUpdate').then(function (reponse) {
                                          location.reload(true);
                                      });
                                  } else {
                                      e.preventDefault();
                                  }
                              });
                          } else if (response.data.VersionLastUpdated == null && $rootScope.NSVersion != 0) {
                              $http.post(webApiBaseUrl + '/api/PersonalSettings/FinalizeVersionUpdate').then(function (response) {
                                  nsPinesService.buildMessage('North Star Version Updated', 'You have been updated to the latest version of North Star.', 'success');
                              });
                          }
                      });
              }

          });
          $scope.$on('$routeChangeSuccess', function (e) {
              $window.ga('send', 'pageview', { page: $location.url() });
              // console.log('success: ', $location.path());
              progressLoader.end();
          });
      }])
      .config(['$provide', '$routeProvider', '$locationProvider', '$httpProvider', function ($provide, $routeProvider, $locationProvider, $httpProvider) {
          $httpProvider.defaults.useXDomain = true;
          // delete $httpProvider.defaults.headers.common['X-Requested-With'];
          //$httpProvider.defaults.headers.post['Content-Type'] = 'application/x-www-form-urlencoded;charset=utf-8';

          $httpProvider.interceptors.push('authInterceptorService');

          // dynamic URL for views
          $httpProvider.interceptors.push(['$rootScope', function ($rootScope) {
              return {
                  request: function (config) {
                      if (config.url.indexOf('views/') > -1
                          && config.url.indexOf('views/401') < 0
                          && config.url.indexOf('views/login') < 0) {
                          if (!$rootScope.NSVersion) {
                              $rootScope.NSVersion = 4;
                          }

                          var separator = config.url.indexOf('?') === -1 ? '?' : '&';
                          config.url = config.url + separator + 'v=' + $rootScope.NSVersion;
                      }

                      return config;
                  }
              };
          }]);
          //$httpProvider.interceptors.push('nsResponseInterceptor');

          $routeProvider
            .when('/', {
                templateUrl: 'views/index.html',
                resolve: {
                    userinfo: ['NSUserInfoService', function (NSUserInfoService) {
                        return NSUserInfoService.loadUserInfo();
                    }],
                    lazyLoad: ['lazyLoad', function (lazyLoad) {
                        return lazyLoad.load([
                          'assets/plugins/form-ckeditor/ckeditor.js',
                          'assets/plugins/form-ckeditor/lang/en.js'
                        ]);
                    }]
                }
            })
            .when('/calendar', {
                templateUrl: 'views/calendar.html',
                resolve: {
                    lazyLoad: ['lazyLoad', function (lazyLoad) {
                        return lazyLoad.load([
                          'assets/plugins/fullcalendar/fullcalendar.js'
                        ]);
                    }]
                }
            })
            .when('/form-ckeditor', {
                templateUrl: 'views/form-ckeditor.html',
                resolve: {
                    lazyLoad: ['lazyLoad', function (lazyLoad) {
                        return lazyLoad.load([
                          'assets/plugins/form-ckeditor/ckeditor.js',
                          'assets/plugins/form-ckeditor/lang/en.js'
                        ]);
                    }]
                }
            })
            .when('/form-imagecrop', {
                templateUrl: 'views/form-imagecrop.html',
                resolve: {
                    lazyLoad: ['lazyLoad', function (lazyLoad) {
                        return lazyLoad.load([
                          'assets/plugins/jcrop/js/jquery.Jcrop.js'
                        ]);
                    }]
                }
            })
            .when('/form-wizard', {
                templateUrl: 'views/form-wizard.html',
                resolve: {
                    lazyLoad: ['lazyLoad', function (lazyLoad) {
                        return lazyLoad.load([
                          'bower_components/jquery-validation/dist/jquery.validate.js',
                          'bower_components/stepy/lib/jquery.stepy.js'
                        ]);
                    }]
                }
            })
            .when('/form-masks', {
                templateUrl: 'views/form-masks.html',
                resolve: {
                    lazyLoad: ['lazyLoad', function (lazyLoad) {
                        return lazyLoad.load([
                          'bower_components/jquery.inputmask/dist/jquery.inputmask.bundle.js'
                        ]);
                    }]
                }
            })
            .when('/maps-vector', {
                templateUrl: 'views/maps-vector.html',
                resolve: {
                    lazyLoad: ['lazyLoad', function (lazyLoad) {
                        return lazyLoad.load([
                          'bower_components/jqvmap/jqvmap/maps/jquery.vmap.europe.js',
                          'bower_components/jqvmap/jqvmap/maps/jquery.vmap.usa.js'
                        ]);
                    }]
                }
            })
            .when('/charts-canvas', {
                templateUrl: 'views/charts-canvas.html',
                resolve: {
                    lazyLoad: ['lazyLoad', function (lazyLoad) {
                        return lazyLoad.load([
                          'bower_components/Chart.js/Chart.min.js'
                        ]);
                    }]
                }
            })
            .when('/charts-svg', {
                templateUrl: 'views/charts-svg.html',
                resolve: {
                    lazyLoad: ['lazyLoad', function (lazyLoad) {
                        return lazyLoad.load([
                          'bower_components/raphael/raphael.js',
                          'bower_components/morris.js/morris.js'
                        ]);
                    }]
                }
            })
          .when('/district-calendar', {
              templateUrl: 'views/district-calendar.html',
              resolve: {
                  lazyLoad: ['lazyLoad', function (lazyLoad) {
                      return lazyLoad.load([
                      'assets/plugins/fullcalendar/fullcalendar.js'
                      ]);
                  }]
              }
          })
          .when('/school-calendar', {
              templateUrl: 'views/school-calendar.html',
              resolve: {
                  lazyLoad: ['lazyLoad', function (lazyLoad) {
                      return lazyLoad.load([
                      'assets/plugins/fullcalendar/fullcalendar.js'
                      ]);
                  }]
              }
          })
                  .when('/user-contact-school-admin', {
                      templateUrl: 'views/user-contact-school-admin.html',
                      resolve: {
                          lazyLoad: ['lazyLoad', function (lazyLoad) {
                              return lazyLoad.load([
                                'assets/plugins/form-ckeditor/ckeditor.js',
                                'assets/plugins/form-ckeditor/lang/en.js'
                              ]);
                          }]
                      }
                  })
                          .when('/user-contact-district-admin', {
                              templateUrl: 'views/user-contact-district-admin.html',
                              resolve: {
                                  lazyLoad: ['lazyLoad', function (lazyLoad) {
                                      return lazyLoad.load([
                                        'assets/plugins/form-ckeditor/ckeditor.js',
                                        'assets/plugins/form-ckeditor/lang/en.js'
                                      ]);
                                  }]
                              }
                          })
                          .when('/user-contact-northstar-support', {
                              templateUrl: 'views/user-contact-northstar-support.html',
                              resolve: {
                                  lazyLoad: ['lazyLoad', function (lazyLoad) {
                                      return lazyLoad.load([
                                        'assets/plugins/form-ckeditor/ckeditor.js',
                                        'assets/plugins/form-ckeditor/lang/en.js'
                                      ]);
                                  }]
                              }
                          })
          .when('/callback/:token',
          {

              templateUrl: function (param) { return 'views/index.html' }
          })

              .when('/reset-password-from-link/:uid',
              {
                  templateUrl: function (param) { return 'views/reset-password-from-link.html' }, requireToken: false
              })
                      .when('/request-password-reset',
              {
                  templateUrl: function (param) { return 'views/request-password-reset.html' }, requireToken: false
              })
              .when('/observation-summary-class/:benchmarkDateId?',
              {
                  templateUrl: function (param) { return 'views/observation-summary-class.html' }, requireToken: true,
                  resolve: {
                      lookupFields: ['nsLookupFieldService', function (nsLookupFieldService) {
                          return nsLookupFieldService.LoadLookupFields();
                      }]
                  }
              })
                      .when('/observation-summary-filtered',
              {
                  templateUrl: function (param) { return 'views/observation-summary-filtered.html' }, requireToken: true,
                  resolve: {
                      lookupFields: ['nsLookupFieldService', function (nsLookupFieldService) {
                          return nsLookupFieldService.LoadLookupFields();
                      }]
                  }
              })
              .when('/tm-attend/:teamMeeting?',
              {
                  templateUrl: function (param) { return 'views/tm-attend.html' }, requireToken: true,
                  resolve: {
                      lookupFields: ['nsLookupFieldService', function (nsLookupFieldService) {
                          return nsLookupFieldService.LoadLookupFields();
                      }]
                  }
              })
              .when('/tm-attend-notes/:teamMeetingId/:studentId',
              {
                  templateUrl: function (param) { return 'views/tm-attend-notes.html' }, requireToken: true,
                  resolve: {
                      lazyLoad: ['lazyLoad', function (lazyLoad) {
                          return lazyLoad.load([
                            'assets/plugins/form-ckeditor/ckeditor.js',
                            'assets/plugins/form-ckeditor/lang/en.js'
                          ]);
                      }]
                  }
              })
              .when('/section-assessment-resultlist/:assessmentid',
              {
                  templateUrl: function (param) { return 'views/section-assessment-resultlist.html' }, requireToken: true,
                  resolve: {
                      lookupFields: ['nsLookupFieldService', function (nsLookupFieldService) {
                          return nsLookupFieldService.LoadLookupFields();
                      }],
                      lazyLoad: ['lazyLoad', function (lazyLoad) {
                          return lazyLoad.load([
                            'assets/plugins/form-ckeditor/ckeditor.js',
                            'assets/plugins/form-ckeditor/lang/en.js'
                          ]);
                      }]
                  }
              })
              .when('/student-dashboard/:id?',
              {
                  templateUrl: function (param) { return 'views/student-dashboard.html' }, requireToken: true,
                  resolve: {
                      lookupFields: ['nsLookupFieldService', function (nsLookupFieldService) {
                          return nsLookupFieldService.LoadLookupFields();
                      }],
                      lazyLoad: ['lazyLoad', function (lazyLoad) {
                          return lazyLoad.load([
                            'assets/plugins/form-ckeditor/ckeditor.js',
                            'assets/plugins/form-ckeditor/lang/en.js'
                          ]);
                      }]
                  }
              })
              .when('/ig-dashboard',
              {
                  templateUrl: function (param) { return 'views/ig-dashboard.html' }, requireToken: true,
                  resolve: {
                      lookupFields: ['nsLookupFieldService', function (nsLookupFieldService) {
                          return nsLookupFieldService.LoadLookupFields();
                      }],
                      lazyLoad: ['lazyLoad', function (lazyLoad) {
                          return lazyLoad.load([
                            'assets/plugins/form-ckeditor/ckeditor.js',
                            'assets/plugins/form-ckeditor/lang/en.js'
                          ]);
                      }]
                  }
              })
              .when('/intervention-detail/:id',
              {
                  templateUrl: function (param) { return 'views/intervention-detail.html' }, requireToken: true,
                  resolve: {
                      lookupFields: ['nsLookupFieldService', function (nsLookupFieldService) {
                          return nsLookupFieldService.LoadLookupFields();
                      }],
                      lazyLoad: ['lazyLoad', function (lazyLoad) {
                          return lazyLoad.load([
                            'assets/plugins/form-ckeditor/ckeditor.js',
                            'assets/plugins/form-ckeditor/lang/en.js'
                          ]);
                      }]
                  }
              })
              .when('/ig-dashboard/:schoolyear/:school/:interventionist/:interventiongroup/:student/:stint',
              {
                  templateUrl: function (param) { return 'views/ig-dashboard.html' }, requireToken: true,
                  resolve: {
                      lookupFields: ['nsLookupFieldService', function (nsLookupFieldService) {
                          return nsLookupFieldService.LoadLookupFields();
                      }],
                      lazyLoad: ['lazyLoad', function (lazyLoad) {
                          return lazyLoad.load([
                            'assets/plugins/form-ckeditor/ckeditor.js',
                            'assets/plugins/form-ckeditor/lang/en.js'
                          ]);
                      }]
                  }
              })
              .when('/ig-assessment-resultlist/:assessmentid',
              {
                  templateUrl: function (param) { return 'views/ig-assessment-resultlist.html' }, requireToken: true,
                  resolve: {
                      lookupFields: ['nsLookupFieldService', function (nsLookupFieldService) {
                          return nsLookupFieldService.LoadLookupFields();
                      }]
                  }
              })
              .when('/tm-email-invite/:teamMeetingId/:tddid/:staffId',
              {
                  templateUrl: function (param) { return 'views/tm-email-invite.html' }, requireToken: true
              })
              .when('/stackedbargraph-groups/:autoload?',
              {
                  templateUrl: function (param) { return 'views/stackedbargraph-groups.html' }, requireToken: true,
                  resolve: {
                      lookupFields: ['nsLookupFieldService', function (nsLookupFieldService) {
                          return nsLookupFieldService.LoadLookupFields();
                      }]
                  }
              })
              .when('/system-benchmarks',
              {
                  templateUrl: function (param) { return 'views/system-benchmarks.html' }, requireToken: true,
                  resolve: {
                      lookupFields: ['nsLookupFieldService', function (nsLookupFieldService) {
                          return nsLookupFieldService.LoadLookupFields();
                      }]
                  }
              })
              .when('/district-benchmarks',
              {
                  templateUrl: function (param) { return 'views/district-benchmarks.html' }, requireToken: true,
                  resolve: {
                      lookupFields: ['nsLookupFieldService', function (nsLookupFieldService) {
                          return nsLookupFieldService.LoadLookupFields();
                      }]
                  }
              })
              .when('/district-yearlyassessment-benchmarks',
              {
                  templateUrl: function (param) { return 'views/district-yearlyassessment-benchmarks.html' }, requireToken: true,
                  resolve: {
                      lookupFields: ['nsLookupFieldService', function (nsLookupFieldService) {
                          return nsLookupFieldService.LoadLookupFields();
                      }]
                  }
              })
              .when('/stackedbargraph-comparison/:autoload?',
              {
                  templateUrl: function (param) { return 'views/stackedbargraph-comparison.html' }, requireToken: true,
                  resolve: {
                      lookupFields: ['nsLookupFieldService', function (nsLookupFieldService) {
                          return nsLookupFieldService.LoadLookupFields();
                      }]
                  }
              })
              .when('/dashboard-school-mca',
              {
                  templateUrl: function (param) { return 'views/dashboard-school-mca.html' }, requireToken: true,
                  resolve: {
                      lookupFields: ['nsLookupFieldService', function (nsLookupFieldService) {
                          return nsLookupFieldService.LoadLookupFields();
                      }]
                  }
              })
              .when('/dashboard-school-mca-prelim',
              {
                  templateUrl: function (param) { return 'views/dashboard-school-mca-prelim.html' }, requireToken: true,
                  resolve: {
                      lookupFields: ['nsLookupFieldService', function (nsLookupFieldService) {
                          return nsLookupFieldService.LoadLookupFields();
                      }]
                  }
              })
              .when('/stacked-bar-graph-summary/:scoreGrouping/:testDueDate',
              {
                  templateUrl: function (param) { return 'views/stacked-bar-graph-summary.html' }, requireToken: true,
                  resolve: {
                      lookupFields: ['nsLookupFieldService', function (nsLookupFieldService) {
                          return nsLookupFieldService.LoadLookupFields();
                      }]
                      //,
                      //dataRetrieval: ['nsStackedBarGraphGroupsOptionsService', '$route', function (nsStackedBarGraphGroupsOptionsService, $route) {
                      //    return nsStackedBarGraphGroupsOptionsService.loadSummaryData($route.current.params.scoreGrouping, $route.current.params.testDueDate);
                      //}]
                  }
              })
              .when('/tm-email-invite/:teamMeetingId/:tddid/:staffId',
              {
                  templateUrl: function (param) { return 'views/tm-email-invite.html' }, requireToken: true
              })
                    .when('/login', {
                        templateUrl: function (param) { return 'views/login.html' }, requireToken: true

                    })
                            .when('/401', {
                                templateUrl: function (param) { return 'views/401.html' }, requireToken: true

                            })
                                    .when('/500', {
                                        templateUrl: function (param) { return 'views/500.html' }, requireToken: true

                                    })
            .when('/:templateFile', {
                templateUrl: function (param) { return 'views/' + param.templateFile + '.html' }, requireToken: true,
                resolve: {
                    userinfo: ['NSUserInfoService', function (NSUserInfoService) {
                        return NSUserInfoService.loadUserInfo();
                    }],
                    lookupFields: ['nsLookupFieldService', function (nsLookupFieldService) {
                        return nsLookupFieldService.LoadLookupFields();
                    }]
                }
            })

           .when('/:templateFile/:id', {
               templateUrl: function (param) { return 'views/' + param.templateFile + '.html' }, requireToken: true,
               resolve: {
                   userinfo: ['NSUserInfoService', function (NSUserInfoService) {
                       return NSUserInfoService.loadUserInfo();
                   }],
                   lookupFields: ['nsLookupFieldService', function (nsLookupFieldService) {
                       return nsLookupFieldService.LoadLookupFields();
                   }]
               }

           })
          .when('/:templateFile/:assessmentId/:classId', {
              templateUrl: function (param) { return 'views/' + param.templateFile + '.html' }, requireToken: true
          })
              .when('/:templateFile/:assessmentId/:classId/:benchmarkDateId', {
                  templateUrl: function (param) { return 'views/' + param.templateFile + '.html' }, requireToken: true
              })
                      .when('/:templateFile/:assessmentId/:classId/:benchmarkDateId/:studentId/:studentResultId', {
                          templateUrl: function (param) { return 'views/' + param.templateFile + '.html' }, requireToken: true,
                          resolve: {
                              lookupFields: ['nsLookupFieldService', function (nsLookupFieldService) {
                                  return nsLookupFieldService.LoadLookupFields();
                              }],
                              filterOptions: ['nsFilterOptionsService', function (nsFilterOptionsService) {
                                  return nsFilterOptionsService.loadOptions();
                              }]
                          }
                      })
            .otherwise({
                redirectTo: '/'
            });

          // $locationProvider.html5Mode(true);
      }])
    .run(['authService', function (authService) {
        authService.fillAuthData();
    }]);


    if (location.hostname == 'localhost') {
        document.domain = document.domain;
        mainModule
        .constant("webApiBaseUrl", "http://localhost:16726")
      .constant("ckEditorSettings", {
          imageUploadUrl: "http://localhost:16726/api/fileuploader/uploadimages",
          uploadUrl: "http://localhost:16726/api/fileuploader/uploadimages",
          filebrowserImageUploadUrl: "http://localhost:16726/api/fileuploader/uploadimagesjsResponse"
      })
      .constant("ckEditorSettingsAdmin", {
          imageUploadUrl: "http://localhost:16726/api/fileuploader/uploadimagesadmin",
          uploadUrl: "http://localhost:16726/api/fileuploader/uploadimagesadmin",
          filebrowserImageUploadUrl: "http://localhost:16726/api/fileuploader/uploadimagesjsResponseadmin"
      })
          .constant('ngAuthSettings', {
              apiServiceBaseUri: "http://localhost:16725/connect/",
              clientId: 'roclient'
          });
    }
    else {
        document.domain = 'northstaret.net';
        mainModule
          .constant("webApiBaseUrl", "https://api.northstaret.net")
          .constant("ckEditorSettings", {
              imageUploadUrl: "https://api.northstaret.net/api/fileuploader/uploadimages",
              uploadUrl: "https://api.northstaret.net/api/fileuploader/uploadimages",
              filebrowserImageUploadUrl: "https://api.northstaret.net/api/fileuploader/uploadimagesjsResponse"
          })
              .constant("ckEditorSettingsAdmin", {
                  imageUploadUrl: "https://api.northstaret.net/api/fileuploader/uploadimagesadmin",
                  uploadUrl: "https://api.northstaret.net/api/fileuploader/uploadimagesadmin",
                  filebrowserImageUploadUrl: "https://api.northstaret.net/api/fileuploader/uploadimagesjsResponseadmin"
              })
        .constant('ngAuthSettings', {
            apiServiceBaseUri: "https://identity.northstaret.net/connect/",
            clientId: 'roclient'
        })
    }
})();