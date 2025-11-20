'use strict';

angular
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
    'theme.vector_maps',
    'theme.google_maps',
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
    'ngAnimate'
  ])
  .constant("webApiBaseUrl", "http://localhost:16726")
    //.constant("webApiBaseUrl", "http://northstar4webapi.azurewebsites.net")
  .constant('ngAuthSettings', {
    //  apiServiceBaseUri: "http://northstaridentity.azurewebsites.net/connect/",
      apiServiceBaseUri: "http://localhost:16725/connect/",
    clientId: 'roclient'
   })  
  .filter('unsafe', ['$sce', function ($sce) {
    return function (val) {   
        return $sce.trustAsHtml(val); 
    }; 
  }])
    .filter('nsDateFormat', [function () {
          return function (val) {
              return moment(val).format('DD-MMM-YYYY');
          };
    }])
        .filter('nsLongDateFormat', [function () {
            return function (val) {
                return moment(val).format('dddd, MMMM DD, YYYY');
            };
        }])
        .filter('nsLongDateFormatWithTime', [function () {
            return function (val) {
                return moment(val).format('dddd, MMMM DD, YYYY h:mm a');
            };
        }])
  .controller('MainController', ['$scope', '$global', '$timeout', 'progressLoader', '$location', 'authService', function ($scope, $global, $timeout, progressLoader, $location, authService) {
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
      $scope['style_'+newVal.key] = newVal.value;
    });
    $scope.$on('globalStyles:maxWidth767', function (event, newVal) {
      $timeout( function () {      
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
    $scope.rightbarAccordions = [{open:true},{open:true},{open:true},{open:true},{open:true},{open:true},{open:true}];

    $scope.$on('$routeChangeStart', function (e) {
      // console.log('start: ', $location.path());
      progressLoader.start();
      progressLoader.set(50);
    });
    $scope.$on('$routeChangeSuccess', function (e) {
      // console.log('success: ', $location.path());
      progressLoader.end();
    });
  }])
  .config(['$provide', '$routeProvider', '$locationProvider', '$httpProvider', function ($provide, $routeProvider, $locationProvider, $httpProvider) {
    $httpProvider.defaults.useXDomain = true;
   // delete $httpProvider.defaults.headers.common['X-Requested-With'];
    //$httpProvider.defaults.headers.post['Content-Type'] = 'application/x-www-form-urlencoded;charset=utf-8';

    $httpProvider.interceptors.push('authInterceptorService');
    //$httpProvider.interceptors.push('nsResponseInterceptor');

    $routeProvider
      .when('/', {
          templateUrl: 'views/index.html',
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

        templateUrl: function(param) { return 'views/index.html' }
    })

        .when('/reset-password-from-link/:uid',
        {
            templateUrl: function (param) { return 'views/reset-password-from-link.html' }, requireToken: true
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
                }]
            }
        })
        .when('/student-dashboard',
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
        .when('/stackedbargraph-comparison/:autoload?',
        {
            templateUrl: function (param) { return 'views/stackedbargraph-comparison.html' }, requireToken: true,
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
              }]
          }
      })

	 .when('/:templateFile/:id', {
	     templateUrl: function (param) { return 'views/' + param.templateFile + '.html' }, requireToken: true,
	     resolve: {
	         userinfo: ['NSUserInfoService', function (NSUserInfoService) {
	             return NSUserInfoService.loadUserInfo();
	         }]
	     }

	 })
    .when('/:templateFile/:assessmentId/:classId', {
        templateUrl: function (param) { return 'views/' + param.templateFile + '.html' }, requireToken: true
    })
		.when('/:templateFile/:assessmentId/:classId/:benchmarkDateId', {
			templateUrl: function (param) { return 'views/' + param.templateFile + '.html'}, requireToken: true 
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
