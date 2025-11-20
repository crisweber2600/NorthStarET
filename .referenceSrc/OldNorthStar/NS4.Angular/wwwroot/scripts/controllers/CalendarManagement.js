(function () {
    'use strict';

    angular
        .module('calendarModule', [])
        .controller('DistrictCalendarController', DistrictCalendarController)
        .controller('SchoolCalendarController', SchoolCalendarController)
        .directive('fullCalendarCustom', ['$uibModal', 'progressLoader', '$bootbox', function ($uibModal, progressLoader, $bootbox) {
            return {
                restrict: 'A',
                scope: {
                    options: '=fullCalendarCustom',
                    mgr: '=',
                    selectedSchool: '='
                },
                link: function (scope, element, attr) {

                    scope.editEventDialog = function (event) {

                        var modalInstance = $uibModal.open({
                            templateUrl: 'editevent.html',
                            scope: scope,
                            controller: function ($scope, $uibModalInstance) {
                                scope.selectedEvent = event;

                                scope.saveEvent = function (event) {
                                    event.title = scope.selectedEvent.title;
                                    calendar.fullCalendar('updateEvent', event);
                                              progressLoader.start();
                                              progressLoader.set(50);
                                              scope.mgr.saveEvent(event).then(function (response) {
                                                  progressLoader.end();
                                                  $uibModalInstance.dismiss('cancel');
                                              }); 
                                }

                                scope.deleteEvent = function (event) {
                                    $bootbox.confirm('Are you sure you want to delete this holiday?', function (response) {
                                        if (response) {
                                            calendar.fullCalendar('removeEvents', event.id);
                                            progressLoader.start();
                                            progressLoader.set(50);
                                            scope.mgr.deleteEvent(event).then(function (response) {
                                                progressLoader.end();
                                                $uibModalInstance.dismiss('cancel');
                                            });
                                        }
                                    });

                                }

                                $scope.cancel = function () {
                                    $uibModalInstance.dismiss('cancel');
                                };
                            },
                            size: 'md',
                        });
                    }

                    scope.newEventDialog = function (start, end, allDay) {

                        var modalInstance = $uibModal.open({
                            templateUrl: 'addevent.html',
                            scope: scope,
                            controller: function ($scope, $uibModalInstance) {
                                scope.selectedEvent = { title: '', start: start, end: end, allDay: allDay };

                                scope.saveEvent = function (event) {

                                    if (scope.selectedEvent.title) {
                                        progressLoader.start();
                                        progressLoader.set(50);
                                        scope.mgr.saveEvent(event).then(function (response) {
                                                calendar.fullCalendar('renderEvent',
                                                  {
                                                      id: response.data.id,
                                                      title: scope.selectedEvent.title,
                                                      start: scope.selectedEvent.start,
                                                      end: scope.selectedEvent.end,
                                                      allDay: true
                                                  },
                                                  true); // make the event "stick"
                                            calendar.fullCalendar('unselect');
                                            progressLoader.end();
                                            $uibModalInstance.dismiss('cancel');
                                        });
                                    }
                                }

                                $scope.cancel = function () {
                                    $uibModalInstance.dismiss('cancel');
                                };
                            },
                            size: 'md',
                        });
                    };

                    var defaultOptions = {
                        eventClick: function(event, element) {

                            scope.editEventDialog(event);
                            //alert(event.title);
                            //$('#calendar').fullCalendar('updateEvent', event);

                        },
                        header: {
                            left: 'prev,next today',
                            center: 'title',
                            right: 'month'
                        },
                        selectable: true,
                        selectHelper: true,
                        select: function (start, end, allDay) {

                            scope.newEventDialog(start, end, allDay); 

                        },
                        editable: true,
                        events: [],
                        buttonText: {
                            prev: '< previous',
                            next: 'next >',
                            prevYear: '<< previous year',  // <<
                            nextYear: 'next year >> ',  // >>
                            today: 'Today',
                            month: 'Month',
                            week: 'Week',
                            day: 'Day'
                        }
                    };

                    // don't load events till the promise gets done loading
                    scope.$watch('options.events', function (newVal, oldVal) {
                        if (newVal !== oldVal) {
                            $.extend(true, defaultOptions, scope.options);

                            // if this is a new calendar, like for district just use options
                            if ($(element).children().length === 0) {
                                calendar = $(element).fullCalendar(defaultOptions)
                            } else {
                                // if this is an old calendar, remove events, then re-add
                                var cal = $(element).fullCalendar('getCalendar');
                                cal.removeEvents();
                                cal.addEventSource(scope.options.events);
                                cal.refetchEvents();
                            }
                        }
                    }, true);

                   
                    if (defaultOptions.droppable == true) {
                        defaultOptions.drop = function (date, allDay) {
                            var originalEventObject = $(this).data('eventObject');
                            var copiedEventObject = $.extend({}, originalEventObject);
                            copiedEventObject.start = date;
                            copiedEventObject.allDay = allDay;
                            calendar.fullCalendar('renderEvent', copiedEventObject, true);
                            if (defaultOptions.removeDroppedEvent == true)
                                $(this).remove();
                        }
                    }
                    var calendar = {};
                }
            };
        }])
        .factory('NSDistrictCalendarManager', [
          '$http', '$routeParams', 'webApiBaseUrl', function ($http, $routeParams, webApiBaseUrl) {
              var NSDistrictCalendarManager = function () {
                  var self = this;


                  self.initialize = function () {

                      var url = webApiBaseUrl + '/api/calendar/GetDistrictCalendar/';
                      var promise = $http.get(url);

                      return promise.then(function (response) {
                          self.CalendarItems = response.data.CalendarItems.map(function (item) {
                              return {
                                  title: item.Subject,
                                  start: item.Start,
                                  end: item.End,
                                  allDay: true,
                                  id: item.Id
                              };
                          });
                      });
                  };


                  self.saveEvent = function (event) {

                      var url = webApiBaseUrl + '/api/calendar/SaveDistrictCalendarEvent/';
                      var paramObj = { Id: event.id, Subject: event.title, Start: event.start.toDate(), End: event.end.toDate() };
                      var promise = $http.post(url, paramObj);

                      return promise;
                  };

                  self.deleteEvent = function (event) {

                      var url = webApiBaseUrl + '/api/calendar/DeleteDistrictCalendarEvent/';
                      var paramObj = { Id: event.id, Subject: event.title, Start: event.start.toDate(), End: event.end ? event.end.toDate() : event.start.toDate() };
                      var promise = $http.post(url, paramObj);

                      return promise;
                  };

                  self.initialize();
              };



              return (NSDistrictCalendarManager);
          }
        ])
         .factory('NSSchoolCalendarManager', [
          '$http', '$routeParams', 'webApiBaseUrl', function ($http, $routeParams, webApiBaseUrl) {
              var NSSchoolCalendarManager = function () {
                  var self = this;
                  

                  self.loadCalendar = function (school) {
                      if (school == null || angular.isUndefined(school)) {
                          return;
                      }
                      self.SchoolId = school.id;
                      var paramObj = { Id: school.id };
                      var url = webApiBaseUrl + '/api/calendar/GetSchoolCalendar/';
                      var promise = $http.post(url, paramObj);

                      return promise.then(function (response) {
                          self.CalendarItems = response.data.CalendarItems.map(function (item) {
                              return {
                                  title: item.Subject,
                                  start: item.Start,
                                  end: item.End,
                                  allDay: true,
                                  id: item.Id,
                                  SchoolId: item.SchoolID
                              };
                          });
                      });
                  };

                  self.saveEvent = function (event) {

                      var url = webApiBaseUrl + '/api/calendar/SaveSchoolCalendarEvent/';
                      var paramObj = { Id: event.id, Subject: event.title, Start: event.start.toDate(), End: event.end.toDate(), SchoolId: event.SchoolId ? event.SchoolId : self.SchoolId };
                      var promise = $http.post(url, paramObj);

                      return promise;
                  };

                  self.deleteEvent = function (event) {

                      var url = webApiBaseUrl + '/api/calendar/DeleteSchoolCalendarEvent/';
                      var paramObj = { Id: event.id, Subject: event.title, Start: event.start.toDate(), End: event.end ? event.end.toDate() : event.start.toDate(), SchoolId : event.schoolId };
                      var promise = $http.post(url, paramObj);

                      return promise;
                  };


              };



              return (NSSchoolCalendarManager);
          }
         ]);

    DistrictCalendarController.$inject = ['$scope', 'InterventionGroup', '$q', '$http', 'nsPinesService', '$location', '$routeParams', 'NSDistrictCalendarManager'];

    function DistrictCalendarController($scope, InterventionGroup, $q, $http, nsPinesService, $location, $routeParams, NSDistrictCalendarManager) {

        $scope.dataManager = new NSDistrictCalendarManager();
    }

    SchoolCalendarController.$inject = ['$scope', 'InterventionGroup', '$q', '$http', 'nsPinesService', '$location', '$routeParams', 'NSSchoolCalendarManager', 'nsFilterOptionsService', 'progressLoader'];

    function SchoolCalendarController($scope, InterventionGroup, $q, $http, nsPinesService, $location, $routeParams, NSSchoolCalendarManager, nsFilterOptionsService, progressLoader) {

        $scope.filterOptions = nsFilterOptionsService.options;
        $scope.dataManager = new NSSchoolCalendarManager();
        $scope.dataManager.loadCalendar($scope.filterOptions.selectedSchool);

        $scope.$watch('filterOptions.selectedSchool.id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                progressLoader.start();
                progressLoader.set(50);
                $scope.dataManager.loadCalendar($scope.filterOptions.selectedSchool).then(function () {
                    progressLoader.end();
                });
            }
        });
    }

})();