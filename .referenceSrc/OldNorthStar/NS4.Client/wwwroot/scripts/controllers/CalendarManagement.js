(function () {
    'use strict';

    angular
        .module('calendarModule', [])
        .controller('DistrictCalendarController', DistrictCalendarController)
        .controller('SchoolCalendarController', SchoolCalendarController)
        .directive('fullCalendarCustom', function () {
            return {
                restrict: 'A',
                scope: {
                    options: '=fullCalendarCustom'
                },
                link: function (scope, element, attr) {
                    var defaultOptions = {
                        eventClick: function(event, element) {

                            //event.title = "CLICKED!";
                            alert(event.title);
                            //$('#calendar').fullCalendar('updateEvent', event);

                        },
                        header: {
                            left: 'prev,next today',
                            center: 'title',
                            right: 'month,agendaWeek,agendaDay'
                        },
                        selectable: true,
                        selectHelper: true,
                        select: function (start, end, allDay) {
                            var title = prompt('Event Title:');
                            if (title) {
                                calendar.fullCalendar('renderEvent',
                                  {
                                      title: title,
                                      start: start,
                                      end: end,
                                      allDay: allDay
                                  },
                                  true // make the event "stick"
                                );
                            }
                            calendar.fullCalendar('unselect');
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
                                calendar = $(element).fullCalendar('getCalendar');
                                calendar.removeEvents();
                                calendar.addEventSource(scope.options.events);
                                calendar.refetchEvents();
                            }
                        }
                    });

                   
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
        })
        .factory('NSDistrictCalendarManager', [
          '$http', '$routeParams', function ($http, $routeParams) {
              var NSDistrictCalendarManager = function () {
                  var self = this;


                  self.initialize = function () {

                      var url = 'http://localhost:16726/api/calendar/GetDistrictCalendar/';
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

                  self.initialize();
              };



              return (NSDistrictCalendarManager);
          }
        ])
         .factory('NSSchoolCalendarManager', [
          '$http', '$routeParams', function ($http, $routeParams) {
              var NSSchoolCalendarManager = function () {
                  var self = this;


                  self.loadCalendar = function (school) {
                      if (school == null || angular.isUndefined(school)) {
                          return;
                      }

                      var paramObj = { Id: school.id };
                      var url = 'http://localhost:16726/api/calendar/GetSchoolCalendar/';
                      var promise = $http.post(url, paramObj);

                      return promise.then(function (response) {
                          self.CalendarItems = response.data.CalendarItems.map(function (item) {
                              return {
                                  title: item.Subject,
                                  start: item.Start,
                                  end: item.End,
                                  allDay: true,
                                  id: item.Id,
                                  editable: true
                              };
                          });
                      });
                  };

                  
                  //self.saveAvailability = function (availability) {
                  //    var paramObj = { AssessmentId: availability.AssessmentId, SchoolId: availability.SchoolId, AssessmentIsAvailable: availability.AssessmentIsAvailable };
                  //    var url = 'http://localhost:16726/api/AssessmentAvailability/UpdateSchoolAssessmentAvailability/';
                  //    var promise = $http.post(url, paramObj);

                  //    return promise.then(function (response) {
                  //        return response.data.isValid;
                  //    });
                  //};
              };



              return (NSSchoolCalendarManager);
          }
         ]);

    DistrictCalendarController.$inject = ['$scope', 'InterventionGroup', '$q', '$http', 'nsPinesService', '$location', '$routeParams', 'NSDistrictCalendarManager'];

    function DistrictCalendarController($scope, InterventionGroup, $q, $http, nsPinesService, $location, $routeParams, NSDistrictCalendarManager) {

        var vm = this;
        vm.dataManager = new NSDistrictCalendarManager();

        //vm.saveAvailability = function (availability) {
        //    vm.dataManager.saveAvailability(availability).then(function (response) {
        //        if (response) {
        //            nsPinesService.dataSavedSuccessfully();
        //        } else {
        //            nsPinesService.dataSaveError();
        //        }
        //    });
        //};
    }

    SchoolCalendarController.$inject = ['$scope', 'InterventionGroup', '$q', '$http', 'nsPinesService', '$location', '$routeParams', 'NSSchoolCalendarManager', 'nsFilterOptionsService'];

    function SchoolCalendarController($scope, InterventionGroup, $q, $http, nsPinesService, $location, $routeParams, NSSchoolCalendarManager, nsFilterOptionsService) {

        var vm = this;
        vm.filterOptions = nsFilterOptionsService.options;
        vm.dataManager = new NSSchoolCalendarManager();
        vm.dataManager.loadCalendar(vm.filterOptions.selectedSchool);

        //vm.saveAvailability = function (availability) {
        //    if (!availability.IsDisabled) {
        //        vm.dataManager.saveAvailability(availability).then(function (response) {
        //            if (response) {
        //                nsPinesService.dataSavedSuccessfully();
        //            } else {
        //                nsPinesService.dataSaveError();
        //            }
        //        });
        //    }
        //};

        $scope.$watch('vm.filterOptions.selectedSchool.id', function (newValue, oldValue) {
            if (!angular.equals(newValue, oldValue)) {
                vm.dataManager.loadCalendar(vm.filterOptions.selectedSchool);
            }
        });
    }

})();