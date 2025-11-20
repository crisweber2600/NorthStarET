
(function () {
    'use strict'

    angular
      .module('theme.navigation-controller', [])
      .controller('NavigationController', ['$scope', '$http', '$location', '$timeout', '$global', 'webApiBaseUrl', 'authService', function ($scope, $http, $location, $timeout, $global, webApiBaseUrl, authService) {
          $scope.menu = { items: [] };

          // initial load 
          if (authService.authentication.isAuth) {
              LoadNavigationNodes();
          }

          $scope.$watch(function () {
              return authService.authentication.isAuth;
          }, function (newVal, oldVal) {
              if (newVal !== oldVal && newVal === true) {
                  LoadNavigationNodes();
              }
          });

          function LoadNavigationNodes() {
              $scope.menu.items = [];
              $scope.openItems = [];
              $scope.selectedItems = [];
              $scope.selectedFromNavMenu = false;
              $http.get(webApiBaseUrl + "/api/Navigation/NavigationNodes").then(
        function (response) {
            //fore
            $scope.menu.items = response.data.Nodes;
            var item = $scope.findItemByUrl($scope.menu.items, $location.path());

            if (item)
                $timeout(function () { $scope.select(item); });
            $global.set('navrefreshneeded', false);
        });
          }

          // if refresh needed
          $scope.$watch(function () {
              return $global.get('navrefreshneeded');
          }, function (newVal, oldVal) {
              if (newVal !== oldVal && newVal === true) {
                  LoadNavigationNodes();
              }
          });

          $scope.$watch(function () {
              return $global.get('destroynavigation');
          }, function (newVal, oldVal) {
              if (newVal !== oldVal && newVal === true) {
                  $scope.menu.items = [];
                  $scope.openItems = [];
                  $scope.selectedItems = [];
                  $scope.selectedFromNavMenu = false;
              }
          });


          // if login status changes, refresh
          //$scope.$watch(function () {
          //    return authService.authentication.isAuth;
          //}, function (newVal, oldVal) {
          //    if (newVal !== oldVal && newVal === true) {
          //        LoadNavigationNodes();
          //    }
          //});

          /*
        $scope.menu = [
            {
                label: 'Dashboard',
                iconClasses: 'fa fa-home',
                url: '#/'
            },
            {
                label: 'Personal Settings',
                iconClasses: 'fa fa-cog',
                html: '<span class="badge badge-blue">6</span>',
                children: [
                    {
                        label: 'Customize Fields',
                        url: '#/personal-fields'
                    },
                    {
                        label: 'Change Password'
                    },
                    {
                        label: 'Change Profile'
                    }
                ]
            },
            {
                label: 'Assessment Builder',
                iconClasses: 'fa fa-briefcase',
                html: '<span class="badge badge-blue">2</span>',
                children: [
                    {
                        label: 'Manage Assessments',
                        url: '#/assessment-list' 
                    },
                    {
                        label: 'Observation Summary - Class',
                        url: '#/observation-summary-class'
                    },
                    {
                        label: 'F&P Data Entry',
                        url: '#/assessment-defaultentry/1/10877/374'
                    },
                    {
                        label: 'Writing Vocab Data Entry',
                        url: '#/assessment-defaultentry/3/10877/374'
                    }
                ]
            },
            {
                label: 'Admin',
                iconClasses: 'fa fa-cog',
                html: '<span class="badge badge-blue">6</span>',
                children: [
                    {
                        label: 'Staff',
                        children: [
                            {
                                label: 'Manage Staff',
                                url: '#/staff-list'
                            }
                        ]
                    },
                    {
                        label: 'Sections',
                        children: [
                            {
                                label: 'Manage Sections',
                                url: '#/section-list'
                            }
                        ]
                    },
                    {
                        label: 'Students',
                        children: [
                            {
                                label: 'Manage Students',
                                url: '#/student-list'
                            }
                        ]
                    },
                    {
                        label: 'Districts',
                        children: [
                            {
                                label: 'Manage Staff',
                                url: '#/staff-list'
                            }
                        ]
                    },
                    {
                        label: 'School'
                    }
                ]
                },
                {
                    label: 'Intervention Toolkit',
                    iconClasses: 'fa fa-cog',
                    html: '<span class="badge badge-blue">6</span>',
                    children: [
                        {
                            label: 'Browse Interventions',
                            url: '#/interventions-browse'
                        },
                        {
                            label: 'Manage Tools'
                        },
                        {
                            label: 'Manage Videos'
                        }
                        ,
                        {
                            label: 'Manage Tiers'
                        }
                        ,
                        {
                            label: 'Manage Categories'
                        }
                        ,
                        {
                            label: 'Manage Units of Study'
                        }
                        ,
                        {
                            label: 'Manage Frameworks'
                        }
                        ,
                        {
                            label: 'Manage Workshops'
                        }
                    ]
                },
            {
                label: "Intervention Groups",
                iconClasses: "fa fa-th-large",
                children: [
                    {
                        label: "Add/Edit",
                        iconClasses: "fa fa-tasks",
                        url: "#/ig-manage"
                    },
                    {
                        label: "Attendance",
                        iconClasses: "fa fa-calendar",
                        url: "#/ig-attendance"
                    },
                    {
                        label: "Reports",
                        iconClasses: "fa fa-bar-chart-o",
                        url: "#/ig-reports"
                    },
                    {
                        label: "Data-Entry",
                        iconClasses: "fa fa-pencil",
                        url: "#/ig-data-entry"
                    }
                ]
            },
                            {
                                label: "RTI",
                                iconClasses: "fa fa-th-large",
                                children: [
                                    {
                                        label: "Manage Team Meetings",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/tm-manage"
                                    },
                                    {
                                        label: "Attend Team Meeting",
                                        iconClasses: "fa fa-calendar",
                                        url: "#/tm-attend-list"
                                    }
                                ]
                            },
            {
                label: "Sections",
                iconClasses: "fa fa-th-large",
                children: [
                    {
                        label: "Add/Edit",
                        iconClasses: "fa fa-tasks",
                        url: "#/section-manage"
                    },
                    {
                        label: "Reports",
                        iconClasses: "fa fa-bar-chart-o",
                        children: [
                            {
                                label: "Fountas & Pinnell",
                                iconClasses: "fa fa-tasks",
                                children: [
                                    {
                                        label: "Line Graph",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/linegraph/1"
                                    },
                                    {
                                        label: "Section Report",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/fp-sectionreport"
                                    },
                                    {
                                        label: "Progress with Targets",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/fp-sectionreporttargets"
                                    },
                                ]
                            },
                        ]
                    },
                    {
                        label: "Data-Entry",
                        iconClasses: "fa fa-pencil",
                        url: "#/ig-data-entry"
                    }
                ]
            },
            { 
                label: "Data Entry",
                iconClasses: "fa fa-bar-chart-o",
                children: [
                    {
                        label: "Classrooms (Sections)",
                        iconClasses: "fa fa-tasks",
                        children: [
                            {
                                label: 'F&P Data Entry',
                                url: '#/section-assessment-resultlist/1'
                            },
                            {
                                label: 'Writing Vocab Data Entry',
                                url: '#/section-assessment-resultlist/3'
                            },
                            {
                                label: 'CAP Data Entry',
                                url: '#/section-assessment-resultlist/27'
                            },
                            {
                                label: 'HFW Data Entry',
                                url: '#/section-assessment-resultlist/36'
                            },
                            {
                                label: 'LID Data Entry',
                                url: '#/section-assessment-resultlist/28'
                            },
                            {
                                label: 'Spelling Inventory V4 Primary',
                                url: '#/section-assessment-resultlist/30'
                            },
                            {
                                label: 'Spelling Inventory V4 Elementary',
                                url: '#/section-assessment-resultlist/31'
                            },
                            {
                                label: 'Spelling Inventory V4 Intermediate',
                                url: '#/section-assessment-resultlist/32'
                            },
                            {
                                label: 'Spelling Inventory V3 Primary',
                                url: '#/section-assessment-resultlist/33'
                            },
                            {
                                label: 'Spelling Inventory V3 Elementary',
                                url: '#/section-assessment-resultlist/34'
                            },
                            {
                                label: 'Spelling Inventory V3 Intermediate',
                                url: '#/section-assessment-resultlist/35'
                            }
                        ]
                    },
                    {
                        label: "Intervention Groups",
                        iconClasses: "fa fa-tasks",
                        children: [
                            {
                                label: "LLI",
                                iconClasses: "fa fa-tasks",
                            }
                        ]
                    }
                ]
            },
            {
            label: "Reports",
                iconClasses: "fa fa-bar-chart-o",
                children: [
                    {
                        label: "Classrooms (Sections)",
                        iconClasses: "fa fa-tasks",
                        children: [
                            {
                                label: "Fountas & Pinnell",
                                iconClasses: "fa fa-tasks",
                                children: [
                                    {
                                        label: "Line Graph",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/linegraph/1"
                                    },
                                    {
                                        label: "Section Report",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/fp-sectionreport"
                                    },
                                    {
                                        label: "Progress with Targets",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/fp-sectionreporttargets"
                                    }
                                ]
                            },
                            {
                                label: "HFW",
                                iconClasses: "fa fa-tasks",
                                children: [
                                    {
                                        label: "Line Graph",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/linegraph/36"
                                    },
                                    {
                                        label: "Detailed Student Report",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/hfw-detailedstudentreport"
                                    },
                                    {
                                        label: "Missing Words",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/hfw-missingwords"
                                    } 
                                ]
                            },
                            {
                                label: "Spelling Inventory V4 Primary",
                                iconClasses: "fa fa-tasks",
                                children: [
                                    {
                                        label: "Section Report",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/spell-section-report/30/10877/374"
                                    },
                                    {
                                        label: "Line Graph",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/spelling-line-graph/30/10877"
                                    }
                                ]
                            }, 
                            {
                                label: "Spelling Inventory V4 Elementary",
                                iconClasses: "fa fa-tasks",
                                children: [
                                    {
                                        label: "Section Report",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/spell-section-report/31/11090/374"
                                    },
                                    {
                                        label: "Line Graph",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/spelling-line-graph/31/10877"
                                    }
                                ]
                            }, 
                            {
                                label: "Spelling Inventory V4 Intermediate",
                                iconClasses: "fa fa-tasks",
                                children: [
                                    {
                                        label: "Section Report",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/spell-section-report/32/10877/374"
                                    },
                                    {
                                        label: "Line Graph",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/spelling-line-graph/32/10877"
                                    }
                                ]
                            }, 
                            {
                                label: "Spelling Inventory V3 Primary",
                                iconClasses: "fa fa-tasks",
                                children: [
                                    {
                                        label: "Section Report",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/spell-section-report/33/10877/374"
                                    },
                                    {
                                        label: "Line Graph",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/spelling-line-graph/33/10877"
                                    }
                                ]
                            }, 
                            {
                                label: "Spelling Inventory V3 Elementary",
                                iconClasses: "fa fa-tasks",
                                children: [
                                    {
                                        label: "Section Report",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/spell-section-report/34/10877/374"
                                    },
                                    {
                                        label: "Line Graph",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/spelling-line-graph/34/10877"
                                    }
                                ]
                            }, 
                            {
                                label: "Spelling Inventory V3 Intermediate",
                                iconClasses: "fa fa-tasks",
                                children: [
                                    {
                                        label: "Section Report",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/spell-section-report/35/10877/374"
                                    },
                                    {
                                        label: "Line Graph",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/spelling-line-graph/35/10877"
                                    }
                                ]
                            }, 
                            {
                                label: "Concepts About Print",
                                iconClasses: "fa fa-tasks",
                                children: [
                                    {
                                        label: "Section Report",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/cap-section-report/27/10877"
                                    }
                                ]
                            }
                            ,
                            {
                                label: "Letter ID",
                                iconClasses: "fa fa-tasks",
                                children: [
                                    {
                                        label: "By Alphabet",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/lid-section-report/28/10877/Alphabet Response"
                                    },
                                    {
                                        label: "By Sound",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/lid-section-report/28/10877/Sound Response"
                                    },
                                    {
                                        label: "By Word",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/lid-section-report/28/10877/Word Response"
                                    },
                                    {
                                        label: "Overall",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/lid-section-report/28/10877/Overall Response"
                                    }
                                ]
                            }
                        ]
                    },
                    {
                        label: "Intervention Groups",
                        iconClasses: "fa fa-tasks",
                        children: [
                            {
                                label: "Fountas & Pinnell",
                                iconClasses: "fa fa-tasks",
                                children: [
                                    {
                                        label: "Line Graph",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/linegraph/1"
                                    },
                                    {
                                        label: "Section Report",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/fp-sectionreport"
                                    },
                                    {
                                        label: "Progress with Targets",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/fp-sectionreporttargets"
                                    },
                                ]
                            }
                        ]
                    },
                    {
                        label: "Stacked Bar Graphs",
                        iconClasses: "fa fa-tasks",
                        children: [
                            {
                                label: "State Test Data",
                                iconClasses: "fa fa-tasks",
                                children: [
                                    {
                                        label: "Line Graph",
                                        iconClasses: "fa fa-tasks",
                                        url: "#/linegraph/1"
                                    }
                                ]
                            },
                            {
                                label: "Compare Custom Groups",
                                iconClasses: "fa fa-tasks",
                                url: "#/stackedbargraph-groups"
                            },
                            {
                                label: "Compare Assessements",
                                iconClasses: "fa fa-tasks",
                                url: "#/stackedbargraph-assessments"
                            }
                        ]
                    },
                    {
                        label: "State Test Data",
                        iconClasses: "fa fa-tasks",
                        children: [
                            {
                                label: "Line Graphs",
                                iconClasses: "fa fa-tasks",
                            }
                        ]
                    }
                ]
            },
            {
                label:"System",
                iconClasses:"fa fa-th-large",
                children: [
                    {
                        label:"Import MN State Test Data",
                        iconClasses:"fa fa-tasks",
                        url:"#/import-state-test-data-mn"
                    },
                    {
                        label:"Import NY State Test Data",
                        iconClasses:"fa fa-comments-o",
                        url:"#/import-state-test-data-ny"
                    }
                ]
            }
        ];
        */
          var setParent = function (children, parent) {
              angular.forEach(children, function (child) {
                  child.parent = parent;
                  if (child.children !== undefined && child.children !== null && child.children.length > 0) {
                      setParent(child.children, child);
                  }
              });
          };

          $scope.findItemByUrl = function (children, url) {
              for (var i = 0, length = children.length; i < length; i++) {
                  if (children[i].url && children[i].url.replace('#', '') == url) return children[i];
                  if (children[i].children !== undefined && children[i].children !== null) {
                      var item = $scope.findItemByUrl(children[i].children, url);
                      if (item) return item;
                  }
              }
          };

          //setParent ($scope.menu, null);

          $scope.openItems = [];
          $scope.selectedItems = [];
          $scope.selectedFromNavMenu = false;

          $scope.select = function (item) {
              // close open nodes
              if (item.open) {
                  item.open = false;
                  return;
              }
              for (var i = $scope.openItems.length - 1; i >= 0; i--) {
                  $scope.openItems[i].open = false;
              };
              $scope.openItems = [];
              var parentRef = item;
              while (parentRef !== null && parentRef !== undefined) {
                  parentRef.open = true;
                  $scope.openItems.push(parentRef);
                  parentRef = parentRef.parent;
              }

              // handle leaf nodes
              if (!item.children || (item.children && item.children.length < 1)) {
                  $scope.selectedFromNavMenu = true;
                  for (var j = $scope.selectedItems.length - 1; j >= 0; j--) {
                      $scope.selectedItems[j].selected = false;
                  };
                  $scope.selectedItems = [];
                  var parentRef = item;
                  while (parentRef !== null && parentRef !== undefined) {
                      parentRef.selected = true;
                      $scope.selectedItems.push(parentRef);
                      parentRef = parentRef.parent;
                  }
              };
          };

          $scope.$watch(function () {
              return $location.path();
          }, function (newVal, oldVal) {
              if ($scope.selectedFromNavMenu == false) {
                  var item = $scope.findItemByUrl($scope.menu.items, newVal);
                  if (item)
                      $timeout(function () { $scope.select(item); });
              }
              $scope.selectedFromNavMenu = false;
          });
          $scope.$watch(function () {
              return $scope.menu.items;
          }, function (newVal, oldVal) {
              if ($scope.selectedFromNavMenu == false) {
                  setParent($scope.menu.items, null);
                  var item = $scope.findItemByUrl($scope.menu.items, newVal);
                  if (item)
                      $timeout(function () { $scope.select(item); });
              }
              $scope.selectedFromNavMenu = false;
          });

          // searchbar
          $scope.showSearchBar = function ($e) {
              $e.stopPropagation();
              $global.set('showSearchCollapsed', true);
          }
          $scope.$on('globalStyles:changed:showSearchCollapsed', function (event, newVal) {
              $scope.style_showSearchCollapsed = newVal;
          });
          $scope.goToSearch = function () {
              $location.path('/extras-search')
          };
      }])

})();









