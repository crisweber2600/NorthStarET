'use strict';
angular.module('theme.services', []).factory('Admin', [
  '$resource',
  'webApiBaseUrl',
  function Admin($resource, webApiBaseUrl) {
    return $resource(webApiBaseUrl + '/api/staff/:id');
  }
]).factory('Assessment', [
  '$resource',
  'webApiBaseUrl',
  function Assessment($resource, webApiBaseUrl) {
    return $resource(webApiBaseUrl + '/api/assessment/:id');
  }
]).service('$global', [
  '$rootScope',
  'EnquireService',
  '$document',
  function ($rootScope, EnquireService, $document) {
    this.settings = {
      fixedHeader: true,
      headerBarHidden: true,
      leftbarCollapsed: false,
      leftbarShown: false,
      rightbarCollapsed: false,
      fullscreen: false,
      layoutHorizontal: false,
      layoutHorizontalLargeIcons: false,
      layoutBoxed: false,
      showSearchCollapsed: false
    };
    var brandColors = {
        'default': '#ecf0f1',
        'inverse': '#95a5a6',
        'primary': '#3498db',
        'success': '#2ecc71',
        'warning': '#f1c40f',
        'danger': '#e74c3c',
        'info': '#1abcaf',
        'brown': '#c0392b',
        'indigo': '#9b59b6',
        'orange': '#e67e22',
        'midnightblue': '#34495e',
        'sky': '#82c4e6',
        'magenta': '#e73c68',
        'purple': '#e044ab',
        'green': '#16a085',
        'grape': '#7a869c',
        'toyo': '#556b8d',
        'alizarin': '#e74c3c'
      };
    this.getBrandColor = function (name) {
      if (brandColors[name]) {
        return brandColors[name];
      } else {
        return brandColors['default'];
      }
    };
    $document.ready(function () {
      EnquireService.register('screen and (max-width: 767px)', {
        match: function () {
          $rootScope.$broadcast('globalStyles:maxWidth767', true);
        },
        unmatch: function () {
          $rootScope.$broadcast('globalStyles:maxWidth767', false);
        }
      });
    });
    this.get = function (key) {
      return this.settings[key];
    };
    this.set = function (key, value) {
      this.settings[key] = value;
      $rootScope.$broadcast('globalStyles:changed', {
        key: key,
        value: this.settings[key]
      });
      $rootScope.$broadcast('globalStyles:changed:' + key, this.settings[key]);
    };
    this.values = function () {
      return this.settings;
    };
  }
]).factory('pinesNotifications', function () {
  return {
    notify: function (args) {
      var notification = new PNotify(args);
      notification.notify = notification.update;
      return notification;
    }
  };
}).factory('progressLoader', function () {
  return {
    start: function () {
      $(document).skylo('start');
    },
    set: function (position) {
      $(document).skylo('set', position);
    },
    end: function () {
      $(document).skylo('end');
    },
    get: function () {
      return $(document).skylo('get');
    },
    inch: function (amount) {
      $(document).skylo('show', function () {
        $(document).skylo('inch', amount);
      });
    }
  };
}).factory('EnquireService', [
  '$window',
  function ($window) {
    return $window.enquire;
  }
]).factory('$bootbox', [
  '$uibModal',
  function ($uibModal) {
    // NOTE: this is a workaround to make BootboxJS somewhat compatible with
    // Angular UI Bootstrap in the absence of regular bootstrap.js
    if ($.fn.modal == undefined) {
      $.fn.modal = function (directive) {
        var that = this;
        if (directive == 'hide') {
          if (this.data('bs.modal')) {
            this.data('bs.modal').close();
            $(that).remove();
          }
          return;
        } else if (directive == 'show') {
          return;
        }
        var modalInstance = $uibModal.open({ template: $(this).find('.modal-content').html() });
        this.data('bs.modal', modalInstance);
        setTimeout(function () {
          $('.modal.ng-isolate-scope').remove();
          $(that).css({
            opacity: 1,
            display: 'block'
          }).addClass('in');
        }, 100);
      };
    }
    return bootbox;
  }
]).service('lazyLoad', [
  '$q',
  '$timeout',
  function ($q, $t) {
    var deferred = $q.defer();
    var promise = deferred.promise;
    this.load = function (files) {
      angular.forEach(files, function (file) {
        if (file.indexOf('.js') > -1) {
          // script
          (function (d, script) {
            var fDeferred = $q.defer();
            script = d.createElement('script');
            script.type = 'text/javascript';
            script.async = true;
            script.onload = function () {
              $t(function () {
                fDeferred.resolve();
              });
            };
            script.onerror = function () {
              $t(function () {
                fDeferred.reject();
              });
            };
            promise = promise.then(function () {
              script.src = file;
              d.getElementsByTagName('head')[0].appendChild(script);
              return fDeferred.promise;
            });
          }(document));
        }
      });
      deferred.resolve();
      return promise;
    };
  }
]).filter('safe_html', [
  '$sce',
  function ($sce) {
    return function (val) {
      return $sce.trustAsHtml(val);
    };
  }
]).filter('range', function () {
  return function (input, min, max) {
    min = parseInt(min);
    //Make string input int
    max = parseInt(max);
    for (var i = min; i <= max; i++)
      input.push(i);
    return input;
  };
}).service('formService', function formService() {
  return {
    fields: [
      {
        name: 'Label',
        value: 'Label'
      },
      {
        name: 'DateCheckbox',
        value: 'Date Checkbox'
      },
      {
        name: 'DropdownRange',
        value: 'Dropdown Range'
      },
      {
        name: 'DecimalRange',
        value: 'Decimal Range'
      },
      {
        name: 'CalculatedFieldClientOnly',
        value: 'Calculated Field - Client Side Only'
      },
      {
        name: 'CalculatedFieldDbBacked',
        value: 'Calculated Field - DB Backed'
      },
      {
        name: 'CalculatedFieldDbBackedString',
        value: 'Calculated Field - DB Backed String'
      },
      {
        name: 'CalculatedFieldDbOnly',
        value: 'Calculated Field - DB Only'
      },
      {
        name: 'DropdownFromDB',
        value: 'Dropdown From Database'
      },
      {
        name: 'Textfield',
        value: 'Textfield'
      },
      {
        name: 'Email',
        value: 'E-mail'
      },
      {
        name: 'Password',
        value: 'Password'
      },
      {
        name: 'Radio',
        value: 'Radio Buttons'
      },
      {
        name: 'Dropdown',
        value: 'Dropdown List'
      },
      {
        name: 'Date',
        value: 'Date'
      },
      {
        name: 'Textarea',
        value: 'Text Area'
      },
      {
        name: 'Checkbox',
        value: 'Checkbox'
      },
      {
        name: 'Hidden',
        value: 'Hidden'
      }
    ]
  };
}).service('nsSectionDataEntryService', [
  '$http',
  'pinesNotifications',
  'webApiBaseUrl',
  function ($http, pinesNotifications, webApiBaseUrl) {
    //this.options = {};
    this.getSectionResults = function (assessmentId, sectionId, benchmarkDateId) {
      return $http.get(webApiBaseUrl + '/api/sectiondataentry/getassessmentresults/' + assessmentId + '/' + sectionId + '/' + benchmarkDateId);
    };
    this.loadLookupFields = function (assessmentId) {
      return $http.get(webApiBaseUrl + '/api/assessment/GetLookupFieldsForAssessment/' + assessmentId);
    };
    this.attachFieldsToResults = function (studentResultsArray, fieldsArray) {
      for (var j = 0; j < studentResultsArray.length; j++) {
        for (var k = 0; k < studentResultsArray[j].FieldResults.length; k++) {
          studentResultsArray[j].FieldResults[k].Field = fieldsArray[studentResultsArray[j].FieldResults[k].FieldIndex];
        }
      }
    };
    this.initializeHeaderClassArray = function (fields, headerClassArray) {
      for (var r = 0; r < fields.length; r++) {
        headerClassArray[r] = 'fa';
      }
    };
    this.doSort = function (column, staticColumnsObj, fields, headerClassArray, sortArray) {
      var columnIndex = -1;
      // if this is not a first or lastname column
      if (!isNaN(parseInt(column))) {
        columnIndex = column;
        switch (fields[column].FieldType) {
        case 'DateCheckbox':
          column = 'FieldResults[' + column + '].DateValue';
          break;
        case 'Textfield':
          column = 'FieldResults[' + column + '].StringValue';
          break;
        case 'DecimalRange':
          column = 'FieldResults[' + column + '].DecimalValue';
          break;
        case 'DropdownRange':
          column = 'FieldResults[' + column + '].IntValue';
          break;
        case 'DropdownFromDB':
          column = 'FieldResults[' + column + '].IntValue';
          break;
        case 'CalculatedFieldDbOnly':
          column = 'FieldResults[' + column + '].StringValue';
          break;
        case 'CalculatedFieldDbBacked':
          column = 'FieldResults[' + column + '].IntValue';
          break;
        case 'CalculatedFieldDbBackedString':
          column = 'FieldResults[' + column + '].StringValue';
          break;
        case 'CalculatedFieldClientOnly':
          column = 'FieldResults[' + column + '].StringValue';
          //shouldnt even be used in sorting
          break;
        default:
          column = 'FieldResults[' + column + '].IntValue';
          break;
        }
      }
      var bFound = false;
      for (var j = 0; j < sortArray.length; j++) {
        // if it is already on the list, reverse the sort
        if (sortArray[j].indexOf(column) >= 0) {
          bFound = true;
          // is it already negative? if so, remove it
          if (sortArray[j].indexOf('-') === 0) {
            if (columnIndex > -1) {
              headerClassArray[columnIndex] = 'fa';
            } else if (column === 'FirstName') {
              staticColumnsObj.firstNameHeaderClass = 'fa';
            } else if (column === 'LastName') {
              staticColumnsObj.lastNameHeaderClass = 'fa';
            }
            sortArray.splice(j, 1);
          } else {
            if (columnIndex > -1) {
              headerClassArray[columnIndex] = 'fa fa-chevron-down';
            } else if (column === 'FirstName') {
              staticColumnsObj.firstNameHeaderClass = 'fa fa-chevron-down';
            } else if (column === 'LastName') {
              staticColumnsObj.lastNameHeaderClass = 'fa fa-chevron-down';
            }
            sortArray[j] = '-' + sortArray[j];
          }
          break;
        }
      }
      if (!bFound) {
        sortArray.push(column);
        if (columnIndex > -1) {
          headerClassArray[columnIndex] = 'fa fa-chevron-up';
        } else if (column === 'FirstName') {
          staticColumnsObj.firstNameHeaderClass = 'fa fa-chevron-up';
        } else if (column === 'LastName') {
          staticColumnsObj.lastNameHeaderClass = 'fa fa-chevron-up';
        }
      }
    };
    this.deleteStudentTestResult = function (assessmentId, studentResult) {
      var returnObject = {
          StudentResult: studentResult,
          AssessmentId: assessmentId
        };
      // TODO: update local model, delete all field values
      // loop over each field and delete the values... also delete anything else relevant
      for (var k = 0; k < studentResult.FieldResults.length; k++) {
        studentResult.FieldResults[k].IntValue = null;
        studentResult.FieldResults[k].DecimalValue = null;
        studentResult.FieldResults[k].DateValue = null;
        studentResult.FieldResults[k].StringValue = null;
        studentResult.FieldResults[k].DisplayValue = null;
        studentResult.FieldResults[k].BoolValue = null;
      }
      $http.post(webApiBaseUrl + '/api/assessment/DeleteAssessmentResult', returnObject).success(function (data) {
        dataDeletedSuccessfully();
      }).error(function (data, status, headers, config) {
        alert('error deleting');
      });
    };
    var dataSavedSuccessfully = function () {
      pinesNotifications.notify({
        title: 'Data Saved',
        text: 'Your data was saved successfully.',
        type: 'success'
      });
    };
    var dataDeletedSuccessfully = function () {
      pinesNotifications.notify({
        title: 'Data Deleted',
        text: 'Your data was deleted successfully.',
        type: 'success'
      });
    };
  }
]).service('nsSelect2RemoteOptions', [
  'authService',
  'nsFilterOptionsService',
  'webApiBaseUrl',
  '$routeParams',
  function (authService, nsFilterOptionsService, webApiBaseUrl, $routeParams) {
    this.chooseSectionRemoteOptions = {
      placeholder: 'Search for a section',
      minimumInputLength: 3,
      width: 'resolve',
      multiple: false,
      id: function (section) {
        return section.id;
      },
      ajax: {
        url: webApiBaseUrl + '/api/teammeeting/getsectionsfordropdown',
        dataType: 'json',
        quietMillis: 250,
        transport: function (params) {
          params.beforeSend = function (request) {
            request.setRequestHeader('Authorization', 'Bearer ' + authService.token());
          };
          return $.ajax(params);
        },
        data: function (term, page) {
          // page is the one-based page number tracked by Select2
          return {
            pageNo: page,
            schoolYear: nsFilterOptionsService.options.selectedSchoolYear.SchoolStartYear,
            searchString: term
          };
        },
        results: function (data, page) {
          //var more = (page * 10) < data.total; // whether or not there are more results available
          // notice we return the value of more so Select2 knows if more results can be loaded
          return { results: data };
        }
      },
      dropdownCssClass: 'bigdrop',
      escapeMarkup: function (m) {
        return m;
      }  // we do not want to escape markup since we are displaying html in results
    };
    this.quickSearchSectionsRemoteOptions = {
      placeholder: 'Search for a section',
      minimumInputLength: 3,
      width: 'resolve',
      multiple: false,
      id: function (section) {
        return section.id;
      },
      ajax: {
        url: webApiBaseUrl + '/api/section/QuickSearchSections',
        dataType: 'json',
        quietMillis: 250,
        transport: function (params) {
          params.beforeSend = function (request) {
            request.setRequestHeader('Authorization', 'Bearer ' + authService.token());
          };
          return $.ajax(params);
        },
        data: function (term, page) {
          // page is the one-based page number tracked by Select2
          return {
            schoolYear: nsFilterOptionsService.options.selectedSchoolYear.id,
            searchString: term
          };
        },
        results: function (data) {
          return { results: data };
        }
      },
      dropdownCssClass: 'bigdrop',
      escapeMarkup: function (m) {
        return m;
      }  // we do not want to escape markup since we are displaying html in results
    };
    this.WorkshopRemoteOptions = {
      placeholder: 'Search for a workshop',
      minimumInputLength: 0,
      width: 'resolve',
      multiple: false,
      id: function (item) {
        return item.id;
      },
      ajax: {
        url: webApiBaseUrl + '/api/interventiontoolkit/Workshops',
        dataType: 'json',
        quietMillis: 250,
        transport: function (params) {
          params.beforeSend = function (request) {
            request.setRequestHeader('Authorization', 'Bearer ' + authService.token());
          };
          return $.ajax(params);
        },
        data: function (term) {
          // page is the one-based page number tracked by Select2
          return { searchString: term };
        },
        results: function (data) {
          return { results: data };
        }
      },
      dropdownCssClass: 'bigdrop',
      escapeMarkup: function (m) {
        return m;
      }  // we do not want to escape markup since we are displaying html in results
    };
    this.MultiWorkshopRemoteOptions = {
      placeholder: 'Search for a workshop',
      minimumInputLength: 0,
      width: 'resolve',
      multiple: true,
      id: function (item) {
        return item.id;
      },
      ajax: {
        url: webApiBaseUrl + '/api/interventiontoolkit/Workshops',
        dataType: 'json',
        quietMillis: 250,
        transport: function (params) {
          params.beforeSend = function (request) {
            request.setRequestHeader('Authorization', 'Bearer ' + authService.token());
          };
          return $.ajax(params);
        },
        data: function (term) {
          // page is the one-based page number tracked by Select2
          return { searchString: term };
        },
        results: function (data) {
          return { results: data };
        }
      },
      dropdownCssClass: 'bigdrop',
      escapeMarkup: function (m) {
        return m;
      }  // we do not want to escape markup since we are displaying html in results
    };
    this.quickSearchSectionsAllSchoolYearsRemoteOptions = {
      placeholder: 'Search for a section',
      minimumInputLength: 3,
      width: 'resolve',
      multiple: false,
      id: function (section) {
        return section.id;
      },
      ajax: {
        url: webApiBaseUrl + '/api/section/QuickSearchSectionsAllSchoolYears',
        dataType: 'json',
        quietMillis: 250,
        transport: function (params) {
          params.beforeSend = function (request) {
            request.setRequestHeader('Authorization', 'Bearer ' + authService.token());
          };
          return $.ajax(params);
        },
        data: function (term) {
          // page is the one-based page number tracked by Select2
          return { searchString: term };
        },
        results: function (data) {
          return { results: data };
        }
      },
      dropdownCssClass: 'bigdrop',
      escapeMarkup: function (m) {
        return m;
      }  // we do not want to escape markup since we are displaying html in results
    };
    this.quickSearchVideos = {
      placeholder: 'Search for an uploaded video',
      minimumInputLength: 1,
      width: 'resolve',
      multiple: false,
      initSelection: function (element, callback) {
        var initialSelection = $(element).data('$ngModelController').$modelValue;
        if (angular.isObject(initialSelection)) {
          callback(initialSelection);
        }
      },
      ajax: {
        url: webApiBaseUrl + '/api/interventiontoolkit/quicksearchvideos',
        dataType: 'json',
        quietMillis: 250,
        transport: function (params) {
          params.beforeSend = function (request) {
            request.setRequestHeader('Authorization', 'Bearer ' + authService.token());
          };
          return $.ajax(params);
        },
        data: function (term) {
          // page is the one-based page number tracked by Select2
          return { searchString: term };
        },
        results: function (data) {
          return { results: data };
        }
      },
      dropdownCssClass: 'bigdrop',
      escapeMarkup: function (m) {
        return m;
      }  // we do not want to escape markup since we are displaying html in results
    };
    this.quickSearchAssessmentToolsRemoteOptions = {
      placeholder: 'Search for an assessment tool',
      minimumInputLength: 1,
      width: 'resolve',
      multiple: false,
      initSelection: function (element, callback) {
        var initialSelection = $(element).data('$ngModelController').$modelValue;
        if (angular.isObject(initialSelection)) {
          callback(initialSelection);
        }
      },
      ajax: {
        url: webApiBaseUrl + '/api/interventiontoolkit/quicksearchassessmenttools',
        dataType: 'json',
        quietMillis: 250,
        transport: function (params) {
          params.beforeSend = function (request) {
            request.setRequestHeader('Authorization', 'Bearer ' + authService.token());
          };
          return $.ajax(params);
        },
        data: function (term) {
          // page is the one-based page number tracked by Select2
          return { searchString: term };
        },
        results: function (data) {
          return { results: data };
        }
      },
      dropdownCssClass: 'bigdrop',
      escapeMarkup: function (m) {
        return m;
      }  // we do not want to escape markup since we are displaying html in results
    };
    this.quickSearchInterventionToolsRemoteOptions = {
      placeholder: 'Search for an intervention tool',
      minimumInputLength: 1,
      width: 'resolve',
      multiple: false,
      initSelection: function (element, callback) {
        var initialSelection = $(element).data('$ngModelController').$modelValue;
        if (angular.isObject(initialSelection)) {
          callback(initialSelection);
        }
      },
      ajax: {
        url: webApiBaseUrl + '/api/interventiontoolkit/quicksearchinterventiontools',
        dataType: 'json',
        quietMillis: 250,
        transport: function (params) {
          params.beforeSend = function (request) {
            request.setRequestHeader('Authorization', 'Bearer ' + authService.token());
          };
          return $.ajax(params);
        },
        data: function (term) {
          // page is the one-based page number tracked by Select2
          return { searchString: term };
        },
        results: function (data) {
          return { results: data };
        }
      },
      dropdownCssClass: 'bigdrop',
      escapeMarkup: function (m) {
        return m;
      }  // we do not want to escape markup since we are displaying html in results
    };
    this.CoTeacherRemoteOptions = {
      placeholder: 'Search for a teacher',
      minimumInputLength: 3,
      width: 'resolve',
      multiple: true,
      id: function (staff) {
        return staff.id;
      },
      ajax: {
        url: webApiBaseUrl + '/api/section/getcoteachersfordropdown',
        dataType: 'json',
        quietMillis: 250,
        transport: function (params) {
          params.beforeSend = function (request) {
            request.setRequestHeader('Authorization', 'Bearer ' + authService.token());
          };
          return $.ajax(params);
        },
        data: function (term, page) {
          // page is the one-based page number tracked by Select2
          return {
            pageNo: page,
            searchString: term
          };
        },
        results: function (data, page) {
          //var more = (page * 10) < data.total; // whether or not there are more results available
          // notice we return the value of more so Select2 knows if more results can be loaded
          return { results: data };
        }
      },
      dropdownCssClass: 'bigdrop',
      escapeMarkup: function (m) {
        return m;
      }  // we do not want to escape markup since we are displaying html in results
    };
    this.CoInterventionistsRemoteOptions = {
      placeholder: 'Search for an interventionist',
      minimumInputLength: 3,
      width: 'resolve',
      multiple: true,
      id: function (staff) {
        return staff.id;
      },
      ajax: {
        url: webApiBaseUrl + '/api/InterventionGroup/getinterventionistsfordropdown',
        dataType: 'json',
        quietMillis: 250,
        transport: function (params) {
          params.beforeSend = function (request) {
            request.setRequestHeader('Authorization', 'Bearer ' + authService.token());
          };
          return $.ajax(params);
        },
        data: function (term, page) {
          // page is the one-based page number tracked by Select2
          return {
            pageNo: page,
            searchString: term
          };
        },
        results: function (data, page) {
          //var more = (page * 10) < data.total; // whether or not there are more results available
          // notice we return the value of more so Select2 knows if more results can be loaded
          return { results: data };
        }
      },
      dropdownCssClass: 'bigdrop',
      escapeMarkup: function (m) {
        return m;
      }  // we do not want to escape markup since we are displaying html in results
    };
    this.StaffGroupRemoteOptions = {
      placeholder: 'Search for a staff group',
      minimumInputLength: 3,
      width: 'resolve',
      multiple: true,
      id: function (staff) {
        return staff.id;
      },
      ajax: {
        url: webApiBaseUrl + '/api/teammeeting/getstaffgroupsfordropdown',
        dataType: 'json',
        quietMillis: 250,
        transport: function (params) {
          params.beforeSend = function (request) {
            request.setRequestHeader('Authorization', 'Bearer ' + authService.token());
          };
          return $.ajax(params);
        },
        data: function (term, page) {
          // page is the one-based page number tracked by Select2
          return {
            pageNo: page,
            searchString: term
          };
        },
        results: function (data, page) {
          //var more = (page * 10) < data.total; // whether or not there are more results available
          // notice we return the value of more so Select2 knows if more results can be loaded
          return { results: data };
        }
      },
      dropdownCssClass: 'bigdrop',
      escapeMarkup: function (m) {
        return m;
      }  // we do not want to escape markup since we are displaying html in results
    };
    this.StudentSpedLabelRemoteOptions = {
      placeholder: 'Search for a Special Education Label',
      minimumInputLength: 0,
      width: 'resolve',
      multiple: true,
      id: function (label) {
        return label.id;
      },
      ajax: {
        url: webApiBaseUrl + '/api/student/GetStudentSpedLabelLookups',
        dataType: 'json',
        quietMillis: 250,
        transport: function (params) {
          params.beforeSend = function (request) {
            request.setRequestHeader('Authorization', 'Bearer ' + authService.token());
          };
          return $.ajax(params);
        },
        data: function (term, page) {
          // page is the one-based page number tracked by Select2
          return {
            pageNo: page,
            searchString: term
          };
        },
        results: function (data, page) {
          //var more = (page * 10) < data.total; // whether or not there are more results available
          // notice we return the value of more so Select2 knows if more results can be loaded
          return { results: data };
        }
      },
      dropdownCssClass: 'bigdrop',
      escapeMarkup: function (m) {
        return m;
      }  // we do not want to escape markup since we are displaying html in results
    };
    this.InterventionTypeRemoteOptions = {
      placeholder: 'Search for an intervention type',
      minimumInputLength: 0,
      width: 'resolve',
      initSelection: function (element, callback) {
        var initialSelection = $(element).data('$ngModelController').$modelValue;
        if (angular.isObject(initialSelection)) {
          callback(initialSelection);
        }
      },
      ajax: {
        url: webApiBaseUrl + '/api/InterventionGroup/getinterventionsfordropdown',
        transport: function (params) {
          params.beforeSend = function (request) {
            request.setRequestHeader('Authorization', 'Bearer ' + authService.token());
          };
          return $.ajax(params);
        },
        dataType: 'json',
        quietMillis: 250,
        data: function (term, page) {
          // page is the one-based page number tracked by Select2
          return {
            pageNo: page,
            searchString: term
          };
        },
        results: function (data, page) {
          //var more = (page * 10) < data.total; // whether or not there are more results available
          // notice we return the value of more so Select2 knows if more results can be loaded
          return { results: data };
        }
      },
      dropdownCssClass: 'bigdrop',
      escapeMarkup: function (m) {
        return m;
      }  // we do not want to escape markup since we are displaying html in results
    };
    this.MultiInterventionTypeRemoteOptions = {
      placeholder: 'Search for an intervention type',
      minimumInputLength: 0,
      width: 'resolve',
      multiple: true,
      initSelection: function (element, callback) {
        var initialSelection = $(element).data('$ngModelController').$modelValue;
        if (angular.isObject(initialSelection)) {
          callback(initialSelection);
        }
      },
      ajax: {
        url: webApiBaseUrl + '/api/InterventionToolkit/getinterventions',
        transport: function (params) {
          params.beforeSend = function (request) {
            request.setRequestHeader('Authorization', 'Bearer ' + authService.token());
          };
          return $.ajax(params);
        },
        dataType: 'json',
        quietMillis: 250,
        data: function (term, page) {
          // page is the one-based page number tracked by Select2
          return {
            pageNo: page,
            searchString: term
          };
        },
        results: function (data, page) {
          //var more = (page * 10) < data.total; // whether or not there are more results available
          // notice we return the value of more so Select2 knows if more results can be loaded
          return { results: data };
        }
      },
      dropdownCssClass: 'bigdrop',
      escapeMarkup: function (m) {
        return m;
      }  // we do not want to escape markup since we are displaying html in results
    };
    this.PrimaryInterventionistRemoteOptions = {
      placeholder: 'Search for an interventionist',
      minimumInputLength: 3,
      width: 'resolve',
      initSelection: function (element, callback) {
        var initialSelection = $(element).data('$ngModelController').$modelValue;
        if (angular.isObject(initialSelection)) {
          callback(initialSelection);
        }
      },
      ajax: {
        url: webApiBaseUrl + '/api/InterventionGroup/GetInterventionistsForDropdown',
        transport: function (params) {
          params.beforeSend = function (request) {
            request.setRequestHeader('Authorization', 'Bearer ' + authService.token());
          };
          return $.ajax(params);
        },
        dataType: 'json',
        quietMillis: 250,
        data: function (term, page) {
          // page is the one-based page number tracked by Select2
          return {
            pageNo: page,
            searchString: term
          };
        },
        results: function (data, page) {
          //var more = (page * 10) < data.total; // whether or not there are more results available
          // notice we return the value of more so Select2 knows if more results can be loaded
          return { results: data };
        }
      },
      dropdownCssClass: 'bigdrop',
      escapeMarkup: function (m) {
        return m;
      }  // we do not want to escape markup since we are displaying html in results
    };
    this.TeacherRemoteOptions = {
      placeholder: 'Search for a teacher',
      minimumInputLength: 3,
      width: 'resolve',
      initSelection: function (element, callback) {
        var initialSelection = $(element).data('$ngModelController');
        if (angular.isDefined(initialSelection)) {
          initialSelection = initialSelection.$modelValue;
          if (angular.isObject(initialSelection)) {
            callback(initialSelection);
          }
        }
      },
      ajax: {
        url: webApiBaseUrl + '/api/section/getcoteachersfordropdown',
        transport: function (params) {
          params.beforeSend = function (request) {
            request.setRequestHeader('Authorization', 'Bearer ' + authService.token());
          };
          return $.ajax(params);
        },
        dataType: 'json',
        quietMillis: 250,
        data: function (term, page) {
          // page is the one-based page number tracked by Select2
          return {
            pageNo: page,
            searchString: term
          };
        },
        results: function (data, page) {
          //var more = (page * 10) < data.total; // whether or not there are more results available
          // notice we return the value of more so Select2 knows if more results can be loaded
          return { results: data };
        }
      },
      dropdownCssClass: 'bigdrop',
      escapeMarkup: function (m) {
        return m;
      }  // we do not want to escape markup since we are displaying html in results
    };
    this.StaffQuickSearchRemoteOptions = {
      placeholder: 'Search for staff',
      minimumInputLength: 2,
      width: 'resolve',
      initSelection: function (element, callback) {
        var initialSelection = $(element).data('$ngModelController').$modelValue;
        if (angular.isObject(initialSelection)) {
          callback(initialSelection);
        }
      },
      ajax: {
        url: webApiBaseUrl + '/api/staff/quicksearchstaff',
        transport: function (params) {
          params.beforeSend = function (request) {
            request.setRequestHeader('Authorization', 'Bearer ' + authService.token());
          };
          return $.ajax(params);
        },
        dataType: 'json',
        quietMillis: 250,
        data: function (term) {
          // page is the one-based page number tracked by Select2
          return { searchString: term };
        },
        results: function (data, page) {
          //var more = (page * 10) < data.total; // whether or not there are more results available
          // notice we return the value of more so Select2 knows if more results can be loaded
          return { results: data };
        }
      },
      formatResult: function (staff) {
        var labelClass = 'label-success';
        var inactiveLabel = '<span class=\'badge badge-default\'>Inactive</span>';
        var interventionistLabel = '<span class=\'badge badge-primary\'>Interventionist</span>';
        var disabledLabel = '<span class=\'badge badge-danger\'>No Access</span>';
        if (staff.IsActive) {
          inactiveLabel = '';
        }
        if (!staff.disabled) {
          disabledLabel = '';
        }
        if (!staff.IsInterventionist) {
          interventionistLabel = '';
        }
        var markup = '<div>';
        markup += '<b>' + staff.LastName + ', ' + staff.FirstName + '</b> (' + staff.Email + ') | Key: <b>' + staff.TeacherKey + '</b>  ' + inactiveLabel + ' ' + interventionistLabel + ' ' + disabledLabel;
        markup += '</div>';
        return markup;
      },
      formatSelection: function (staff) {
        return '<b>' + staff.FirstName + ' ' + staff.LastName + '</b> (' + staff.Email + ') |  Key: <b>' + staff.TeacherKey + '</b> ';
      },
      dropdownCssClass: 'bigdrop',
      escapeMarkup: function (m) {
        return m;
      }  // we do not want to escape markup since we are displaying html in results
    };
    this.StudentQuickSearchRemoteOptions = {
      placeholder: 'Search for a student',
      minimumInputLength: 3,
      width: 'resolve',
      initSelection: function (element, callback) {
        var initialSelection = $(element).data('$ngModelController').$modelValue;
        if (angular.isObject(initialSelection)) {
          callback(initialSelection);
        }
      },
      ajax: {
        url: webApiBaseUrl + '/api/student/quicksearchstudent',
        transport: function (params) {
          params.beforeSend = function (request) {
            request.setRequestHeader('Authorization', 'Bearer ' + authService.token());
          };
          return $.ajax(params);
        },
        dataType: 'json',
        quietMillis: 250,
        data: function (term) {
          // page is the one-based page number tracked by Select2
          return {
            searchString: term,
            disableInactiveStudents: false
          };
        },
        results: function (data, page) {
          //var more = (page * 10) < data.total; // whether or not there are more results available
          // notice we return the value of more so Select2 knows if more results can be loaded
          return { results: data };
        }
      },
      formatResult: function (student) {
        var inactiveLabel = '<span class=\'badge badge-default\'>Inactive</span>';
        if (student.IsActive) {
          inactiveLabel = '';
        }
        var markup = '<div>';
        markup += '<b>' + student.LastName + ', ' + student.FirstName + ' ' + student.MiddleName + '</b> | Key: <b>' + student.StudentIdentifier + '</b>  ' + inactiveLabel;
        markup += '</div>';
        return markup;
      },
      formatSelection: function (student) {
        return '<b>' + student.LastName + ', ' + student.FirstName + ' ' + student.MiddleName + '</b> | Key: <b>' + student.StudentIdentifier + '</b>';
      },
      dropdownCssClass: 'bigdrop',
      escapeMarkup: function (m) {
        return m;
      }  // we do not want to escape markup since we are displaying html in results
    };
    this.StudentDetailedQuickSearchRemoteOptions = {
      placeholder: 'Search for a student',
      minimumInputLength: 2,
      width: 'resolve',
      initSelection: function (element, callback) {
        var initialSelection = $(element).data('$ngModelController').$modelValue;
        if (angular.isObject(initialSelection)) {
          callback(initialSelection);
        }
      },
      ajax: {
        url: webApiBaseUrl + '/api/student/quicksearchstudentdetailed',
        transport: function (params) {
          params.beforeSend = function (request) {
            request.setRequestHeader('Authorization', 'Bearer ' + authService.token());
          };
          return $.ajax(params);
        },
        dataType: 'json',
        quietMillis: 250,
        data: function (term) {
          // page is the one-based page number tracked by Select2
          return {
            searchString: term,
            disableInactiveStudents: false
          };
        },
        results: function (data, page) {
          //var more = (page * 10) < data.total; // whether or not there are more results available
          // notice we return the value of more so Select2 knows if more results can be loaded
          return { results: data };
        }
      },
      formatResult: function (student) {
        var inactiveLabel = '<span class=\'badge badge-default\'>Inactive</span>';
        if (student.IsActive) {
          inactiveLabel = '';
        }
        var markup = '<div>';
        markup += '<b>' + student.LastName + ', ' + student.FirstName + ' ' + student.MiddleName + '</b> | Key: <b>' + student.StudentIdentifier + '</b>  ' + inactiveLabel;
        markup += '<br>' + student.SchoolYearVerbose + ' | ' + student.SchoolName + ' | Grade: <b>' + student.GradeName + '</b>';
        markup += '<br>' + student.StaffName + ' | Section: <b>' + student.SectionName + '</b>';
        markup += '</div>';
        return markup;
      },
      formatSelection: function (student) {
        return '<b>' + student.LastName + ', ' + student.FirstName + ' ' + student.MiddleName + '</b> | Key: <b>' + student.StudentIdentifier + '</b> ' + (student.GradeName ? ' | Grade: <b> ' + student.GradeName + '</b>' : '');
      },
      dropdownCssClass: 'bigdrop',
      escapeMarkup: function (m) {
        return m;
      }  // we do not want to escape markup since we are displaying html in results
    };
    this.HfwMultiRangeRemoteOptions = {
      placeholder: '-Choose HFW Ranges-',
      minimumInputLength: 0,
      width: 'resolve',
      multiple: true,
      initSelection: function (element, callback) {
        var initialSelection = $(element).data('$ngModelController').$modelValue;
        if (angular.isObject(initialSelection)) {
          callback(initialSelection);
        }
      },
      ajax: {
        url: webApiBaseUrl + '/api/sectionreport/QuickSearchHFWRanges',
        transport: function (params) {
          params.beforeSend = function (request) {
            request.setRequestHeader('Authorization', 'Bearer ' + authService.token());
          };
          return $.ajax(params);
        },
        dataType: 'json',
        quietMillis: 250,
        data: function (term) {
          // page is the one-based page number tracked by Select2
          return { searchString: term };
        },
        results: function (data, page) {
          //var more = (page * 10) < data.total; // whether or not there are more results available
          // notice we return the value of more so Select2 knows if more results can be loaded
          return { results: data };
        }
      },
      formatResult: function (item) {
        var markup = '<div>' + item.text + '</div>';
        return markup;
      },
      formatSelection: function (item) {
        return item.text;
      },
      dropdownCssClass: 'bigdrop',
      escapeMarkup: function (m) {
        return m;
      }  // we do not want to escape markup since we are displaying html in results
    };
    this.SchoolsForUserRemoteOptions = {
      placeholder: '-school-',
      minimumInputLength: 0,
      width: 'resolve',
      multiple: false,
      initSelection: function (element, callback) {
        var initialSelection = $(element).data('$ngModelController').$modelValue;
        if (angular.isObject(initialSelection)) {
          callback(initialSelection);
        }
      },
      ajax: {
        url: webApiBaseUrl + '/api/filteroptions/LoadAllSchoolsForUser',
        transport: function (params) {
          params.beforeSend = function (request) {
            request.setRequestHeader('Authorization', 'Bearer ' + authService.token());
          };
          return $.ajax(params);
        },
        dataType: 'json',
        quietMillis: 250,
        data: function (term) {
          // page is the one-based page number tracked by Select2
          return { searchString: term };
        },
        results: function (data, page) {
          //var more = (page * 10) < data.total; // whether or not there are more results available
          // notice we return the value of more so Select2 knows if more results can be loaded
          return { results: data };
        }
      },
      formatResult: function (grade) {
        var markup = '<div>' + grade.text + '</div>';
        return markup;
      },
      formatSelection: function (grade) {
        return grade.text;
      },
      dropdownCssClass: 'bigdrop',
      escapeMarkup: function (m) {
        return m;
      }  // we do not want to escape markup since we are displaying html in results
    };
    this.AllSchoolYearsRemoteOptions = {
      placeholder: '-school year-',
      minimumInputLength: 0,
      width: 'resolve',
      multiple: false,
      initSelection: function (element, callback) {
        var initialSelection = $(element).data('$ngModelController').$modelValue;
        if (angular.isObject(initialSelection)) {
          callback(initialSelection);
        }
      },
      ajax: {
        url: webApiBaseUrl + '/api/filteroptions/LoadAllSchoolYears',
        transport: function (params) {
          params.beforeSend = function (request) {
            request.setRequestHeader('Authorization', 'Bearer ' + authService.token());
          };
          return $.ajax(params);
        },
        dataType: 'json',
        quietMillis: 250,
        data: function (term) {
          // page is the one-based page number tracked by Select2
          return { searchString: term };
        },
        results: function (data, page) {
          //var more = (page * 10) < data.total; // whether or not there are more results available
          // notice we return the value of more so Select2 knows if more results can be loaded
          return { results: data };
        }
      },
      formatResult: function (grade) {
        var markup = '<div>' + grade.text + '</div>';
        return markup;
      },
      formatSelection: function (grade) {
        return grade.text;
      },
      dropdownCssClass: 'bigdrop',
      escapeMarkup: function (m) {
        return m;
      }  // we do not want to escape markup since we are displaying html in results
    };
    this.AllGradesRemoteOptions = {
      placeholder: '-grade-',
      minimumInputLength: 0,
      width: 'resolve',
      multiple: false,
      initSelection: function (element, callback) {
        var initialSelection = $(element).data('$ngModelController').$modelValue;
        if (angular.isObject(initialSelection)) {
          callback(initialSelection);
        }
      },
      ajax: {
        url: webApiBaseUrl + '/api/filteroptions/LoadAllGrades',
        transport: function (params) {
          params.beforeSend = function (request) {
            request.setRequestHeader('Authorization', 'Bearer ' + authService.token());
          };
          return $.ajax(params);
        },
        dataType: 'json',
        quietMillis: 250,
        data: function (term) {
          // page is the one-based page number tracked by Select2
          return { searchString: term };
        },
        results: function (data, page) {
          //var more = (page * 10) < data.total; // whether or not there are more results available
          // notice we return the value of more so Select2 knows if more results can be loaded
          return { results: data };
        }
      },
      formatResult: function (grade) {
        var markup = '<div>' + grade.text + '</div>';
        return markup;
      },
      formatSelection: function (grade) {
        return grade.text;
      },
      dropdownCssClass: 'bigdrop',
      escapeMarkup: function (m) {
        return m;
      }  // we do not want to escape markup since we are displaying html in results
    };
    this.GradeQuickSearchRemoteOptions = {
      placeholder: 'Search for a Grade',
      minimumInputLength: 0,
      width: 'resolve',
      multiple: true,
      initSelection: function (element, callback) {
        var initialSelection = $(element).data('$ngModelController').$modelValue;
        if (angular.isObject(initialSelection)) {
          callback(initialSelection);
        }
      },
      ajax: {
        url: webApiBaseUrl + '/api/interventiontoolkit/QuickSearchGrades',
        transport: function (params) {
          params.beforeSend = function (request) {
            request.setRequestHeader('Authorization', 'Bearer ' + authService.token());
          };
          return $.ajax(params);
        },
        dataType: 'json',
        quietMillis: 250,
        data: function (term) {
          // page is the one-based page number tracked by Select2
          return { searchString: term };
        },
        results: function (data, page) {
          //var more = (page * 10) < data.total; // whether or not there are more results available
          // notice we return the value of more so Select2 knows if more results can be loaded
          return { results: data };
        }
      },
      formatResult: function (grade) {
        var markup = '<div>' + grade.text + '</div>';
        return markup;
      },
      formatSelection: function (grade) {
        return grade.text;
      },
      dropdownCssClass: 'bigdrop',
      escapeMarkup: function (m) {
        return m;
      }  // we do not want to escape markup since we are displaying html in results
    };
    this.MultiDistrictRemoteOptions = {
      placeholder: 'Choose Districts',
      minimumInputLength: 0,
      width: 'resolve',
      multiple: true,
      initSelection: function (element, callback) {
        var initialSelection = $(element).data('$ngModelController').$modelValue;
        if (angular.isObject(initialSelection)) {
          callback(initialSelection);
        }
      },
      ajax: {
        url: webApiBaseUrl + '/api/interventiontoolkit/QuickSearchDistricts',
        transport: function (params) {
          params.beforeSend = function (request) {
            request.setRequestHeader('Authorization', 'Bearer ' + authService.token());
          };
          return $.ajax(params);
        },
        dataType: 'json',
        quietMillis: 250,
        data: function (term) {
          // page is the one-based page number tracked by Select2
          return { searchString: term };
        },
        results: function (data, page) {
          //var more = (page * 10) < data.total; // whether or not there are more results available
          // notice we return the value of more so Select2 knows if more results can be loaded
          return { results: data };
        }
      },
      formatResult: function (grade) {
        var markup = '<div>' + grade.text + '</div>';
        return markup;
      },
      formatSelection: function (grade) {
        return grade.text;
      },
      dropdownCssClass: 'bigdrop',
      escapeMarkup: function (m) {
        return m;
      }  // we do not want to escape markup since we are displaying html in results
    };
    this.SchoolAdminRemoteOptions = {
      placeholder: 'Search for a School Administrator',
      minimumInputLength: 0,
      width: 'resolve',
      initSelection: function (element, callback) {
        var initialSelection = $(element).data('$ngModelController').$modelValue;
        if (angular.isObject(initialSelection)) {
          callback(initialSelection);
        }
      },
      ajax: {
        url: webApiBaseUrl + '/api/staff/GetSchoolAdminstrators',
        transport: function (params) {
          params.beforeSend = function (request) {
            request.setRequestHeader('Authorization', 'Bearer ' + authService.token());
          };
          return $.ajax(params);
        },
        dataType: 'json',
        quietMillis: 250,
        data: function (term, page) {
          // page is the one-based page number tracked by Select2
          return {
            pageNo: page,
            searchString: term
          };
        },
        results: function (data, page) {
          //var more = (page * 10) < data.total; // whether or not there are more results available
          // notice we return the value of more so Select2 knows if more results can be loaded
          return { results: data };
        }
      },
      dropdownCssClass: 'bigdrop',
      escapeMarkup: function (m) {
        return m;
      }  // we do not want to escape markup since we are displaying html in results
    };
    this.DistrictAdminRemoteOptions = {
      placeholder: 'Search for a District Administrator',
      minimumInputLength: 0,
      width: 'resolve',
      initSelection: function (element, callback) {
        var initialSelection = $(element).data('$ngModelController').$modelValue;
        if (angular.isObject(initialSelection)) {
          callback(initialSelection);
        }
      },
      ajax: {
        url: webApiBaseUrl + '/api/staff/GetDistrictAdminstrators',
        transport: function (params) {
          params.beforeSend = function (request) {
            request.setRequestHeader('Authorization', 'Bearer ' + authService.token());
          };
          return $.ajax(params);
        },
        dataType: 'json',
        quietMillis: 250,
        data: function (term, page) {
          // page is the one-based page number tracked by Select2
          return {
            pageNo: page,
            searchString: term
          };
        },
        results: function (data, page) {
          //var more = (page * 10) < data.total; // whether or not there are more results available
          // notice we return the value of more so Select2 knows if more results can be loaded
          return { results: data };
        }
      },
      dropdownCssClass: 'bigdrop',
      escapeMarkup: function (m) {
        return m;
      }  // we do not want to escape markup since we are displaying html in results
    };
    this.StudentToSectionRemoteOptions = {
      placeholder: 'Search for a student',
      minimumInputLength: 3,
      width: 'resolve',
      initSelection: function (element, callback) {
        var id;
        id = $(element).val();
        if (id !== '') {
          return $.ajax({
            headers: {
              'Authorization': 'Bearer ' + authService.token(),
              'Content-Type': 'application/json'
            },
            url: webApiBaseUrl + '/api/InterventionGroup/getstudentfordropdown',
            type: 'GET',
            dataType: 'json',
            data: { id: id }
          }).done(function (data) {
            var results;
            results = [];
            results.push(data);
            callback(results[0]);
          });
        }
      },
      id: function (student) {
        return student.StudentId;
      },
      ajax: {
        url: webApiBaseUrl + '/api/section/getstudentquicksearch',
        transport: function (params) {
          params.beforeSend = function (request) {
            request.setRequestHeader('Authorization', 'Bearer ' + authService.token());
          };
          return $.ajax(params);
        },
        dataType: 'json',
        quietMillis: 250,
        data: function (term, page) {
          // page is the one-based page number tracked by Select2
          return {
            strSearch: term,
            expandSearch: false,
            page: page,
            schoolYear: 2014
          };
        },
        results: function (data, page) {
          //var more = (page * 10) < data.total; // whether or not there are more results available
          // notice we return the value of more so Select2 knows if more results can be loaded
          return { results: data };
        }
      },
      formatResult: function (student) {
        var markup = '<table class=\'movie-result\'><tr>';
        //if (movie.posters !== undefined && movie.posters.thumbnail !== undefined) {
        //	markup += "<td class='movie-image'><img src='" + movie.posters.thumbnail + "'/></td>";
        //}
        markup += '<td class=\'movie-info\'><div class=\'movie-title\'>' + student.FirstName + ' ' + student.LastName + '</div>';
        //if (movie.critics_consensus !== undefined) {
        //	markup += "<div class='movie-synopsis'>" + movie.critics_consensus + "</div>";
        //}
        //else if (movie.synopsis !== undefined) {
        //	markup += "<div class='movie-synopsis'>" + movie.synopsis + "</div>";
        //}
        markup += '</td></tr></table>';
        return markup;
      },
      formatSelection: function (student) {
        return student.FirstName + ' ' + student.LastName;
      },
      dropdownCssClass: 'bigdrop',
      escapeMarkup: function (m) {
        return m;
      }  // we do not want to escape markup since we are displaying html in results
    };
  }
]).service('nsPinesService', [
  'pinesNotifications',
  '$rootScope',
  function (pinesNotifications, $rootScope) {
    this.dataSavedSuccessfully = function () {
      $rootScope.$broadcast('NSHTTPClear', 'Saved Successfully');
      pinesNotifications.notify({
        title: 'Data Saved',
        text: 'Your data was saved successfully.',
        type: 'success'
      });
    };
    var self = this;
    self.currentStep = 0;
    self.interval = 0;
    self.setIntervalMessage = function (interval) {
      self.interval = interval;
      self.currentStep = 0;
      self.intervalMessage = pinesNotifications.notify({
        title: 'Processed item',
        text: '0 of ' + interval,
        type: 'info',
        icon: 'fa fa-spin fa-refresh',
        hide: false,
        closer: false,
        sticker: false,
        opacity: 1,
        shadow: true,
        width: '200px'
      });
    };
    self.incrementInterval = function () {
      if (self.intervalMessage) {
        self.currentStep++;
        var options = { text: self.currentStep + ' of ' + self.interval };
        // finalize if so
        if (self.currentStep >= self.interval) {
          options.title = 'All complete!';
          options.type = 'success';
          options.hide = true;
          options.closer = true;
          options.sticker = true;
          options.icon = 'fa  fa-check-square-o';
          options.opacity = 1;
          options.shadow = true;
        }
        self.intervalMessage.notify(options);
      }
    };
    this.buildMessage = function (title, text, type) {
      pinesNotifications.notify({
        title: title,
        text: text,
        type: type
      });
    };
    self.startDynamic = function () {
      var notice = pinesNotifications.notify({
          title: 'Please Wait',
          type: 'info',
          icon: 'fa fa-spin fa-refresh',
          hide: false,
          closer: false,
          sticker: false,
          opacity: 0.75,
          shadow: false,
          width: '200px'
        });
      return notice;
    };
    self.endDynamic = function (notice) {
      var options = { text: 'Generating PDF...' };
      options.title = 'Done!';
      options.type = 'success';
      options.hide = true;
      options.closer = true;
      options.sticker = true;
      options.icon = 'fa fa-check';
      options.opacity = 1;
      options.shadow = true;
      options.text = 'File is ready.';
      notice.notify(options);
    };
    this.emailSentSuccessfully = function () {
      pinesNotifications.notify({
        title: 'Email Sent',
        text: 'Your email was sent successfully.',
        type: 'success'
      });
    };
    this.dataDeletedSuccessfully = function () {
      pinesNotifications.notify({
        title: 'Data Deleted',
        text: 'Your data was deleted successfully.',
        type: 'success'
      });
    };
    this.cancelled = function () {
      pinesNotifications.notify({
        title: 'Cancelled',
        text: 'The operation has been cancelled.',
        type: 'info'
      });
    };
    this.generalLoadingError = function () {
      pinesNotifications.notify({
        title: 'Error Loading Data',
        text: 'There was an error processing your request.  Please try again later.',
        type: 'error'
      });
    };
    this.dataError = function () {
      pinesNotifications.notify({
        title: 'Error Saving Data',
        text: 'There was an error saving your changes.  Please try again later.',
        type: 'error'
      });
    };
  }
]);
//.factory('nsResponseInterceptor', ['$rootScope', '$q', '$location', '$window',  function ($rootScope, $q, $location, $window) {
//    return {
//        response: function (response) {
//            if (response.status === 401) {
//                $location.path('#/401');
//            }
//            return response;
//        }
//    };
//}])
;
'use strict';
angular.module('northstar.business', []).factory('NSStudentSectionResultDataEntry', [
  '$http',
  'webApiBaseUrl',
  function ($http, webApiBaseUrl) {
    var NSStudentSectionResultDataEntry = function () {
      this.loadAssessmentStudentResultData = function (assessmentId, selectedSectionId, selectedBenchmarkDateId, studentId, studentResultId) {
        var postObject = {
            AssessmentId: assessmentId,
            SectionId: selectedSectionId,
            BenchmarkDateId: selectedBenchmarkDateId,
            StudentId: studentId,
            StudentResultId: studentResultId
          };
        var url = webApiBaseUrl + '/api/sectiondataentry/GetStudentAssessmentResult';
        return $http.post(url, postObject);
      };
      this.attachFieldsToResults = function (studentResult, fieldsArray, lookupFieldsArray) {
        console.time('Start attach fields');
        for (var k = 0; k < studentResult.FieldResults.length; k++) {
          for (var r = 0; r < fieldsArray.length; r++) {
            if (fieldsArray[r].DatabaseColumn == studentResult.FieldResults[k].DbColumn) {
              studentResult.FieldResults[k].Field = angular.copy(fieldsArray[r]);
              // set display value
              if (fieldsArray[r].FieldType === 'DropdownFromDB') {
                for (var p = 0; p < lookupFieldsArray.length; p++) {
                  if (lookupFieldsArray[p].LookupColumnName === fieldsArray[r].LookupFieldName) {
                    // now find the specifc value that matches
                    for (var y = 0; y < lookupFieldsArray[p].LookupFields.length; y++) {
                      if (studentResult.FieldResults[k].IntValue === lookupFieldsArray[p].LookupFields[y].FieldSpecificId) {
                        studentResult.FieldResults[k].DisplayValue = lookupFieldsArray[p].LookupFields[y].FieldValue;
                      }
                    }
                  }
                }
              }
            }
          }
        }
        console.timeEnd('Start attach fields');  // set initial display values
      };
      this.saveAssessmentResult = function (assessmentId, studentResult, benchmarkDateId) {
        var returnObject = {
            StudentResult: studentResult,
            AssessmentId: assessmentId,
            BenchmarkDateId: benchmarkDateId
          };
        return $http.post(webApiBaseUrl + '/api/dataentry/SaveAssessmentResult', returnObject);
      };
    };
    return NSStudentSectionResultDataEntry;
  }
]).factory('NSSchoolYearsAndSchools', [
  '$http',
  'webApiBaseUrl',
  function ($http, webApiBaseUrl) {
    var NSSchoolYearsAndSchools = function (studentId) {
      this.initialize = function () {
        var url = webApiBaseUrl + '/api/assessment/GetUpdatedFilterOptions';
        var schoolsAndYears = $http.post(url, { ChangeType: 'initial' });
        var self = this;
        self.Schools = [];
        self.SchoolYears = {};
        schoolsAndYears.then(function (response) {
          angular.extend(self, response.data);
          if (self.Schools === null)
            self.Schools = [];
          if (self.SchoolYears === null)
            self.SchoolYears = [];
        }, function (response) {
        });
      };
      this.initialize();
    };
    return NSSchoolYearsAndSchools;
  }
]).factory('NSStudentAttributeLookups', [
  '$http',
  'webApiBaseUrl',
  function ($http, webApiBaseUrl) {
    var NSStudentAttributeLookups = function (studentId) {
      this.initialize = function () {
        var url = webApiBaseUrl + '/api/student/GetStudentAttributeLookups';
        var attributeLookups = $http.get(url);
        var self = this;
        self.AllAttributes = [];
        attributeLookups.then(function (response) {
          angular.extend(self, response.data);
          if (self.AllAttributes === null)
            self.AllAttributes = [];
        }, function (response) {
        });
      };
      this.initialize();
    };
    return NSStudentAttributeLookups;
  }
]).factory('NSStudentSpedLookupValues', [
  '$http',
  'webApiBaseUrl',
  function ($http, webApiBaseUrl) {
    var NSStudentSpedLookupValues = function (studentId) {
      this.initialize = function () {
        var url = webApiBaseUrl + '/api/student/GetStudentSpedLabelLookups';
        var attributeLookups = $http.get(url);
        var self = this;
        self.AllSpedLabels = [];
        attributeLookups.then(function (response) {
          angular.extend(self, response.data);
          if (self.AllSpedLabels === null)
            self.AllSpedLabels = [];
        }, function (response) {
        });
      };
      this.initialize();
    };
    return NSStudentSpedLookupValues;
  }
]).factory('NSObservationSummaryTeamMeetingManager', [
  '$http',
  'webApiBaseUrl',
  function ($http, webApiBaseUrl) {
    var NSObservationSummaryTeamMeetingManager = function () {
      this.initialize = function () {
      };
      this.LoadData = function (teamMeetingId, tddId, staffId) {
        var paramObj = {
            TeamMeetingId: teamMeetingId,
            TestDueDateId: tddId,
            StaffId: staffId
          };
        var url = webApiBaseUrl + '/api/assessment/GetTeamMeetingObservationSummary/';
        //var paramObj = {}
        var summaryData = $http.post(url, paramObj);
        var self = this;
        self.LookupLists = [];
        self.Scores = [];
        self.BenchmarksByGrade = [];
        return summaryData.then(function (response) {
          angular.extend(self, response.data);
          if (self.LookupLists === null)
            self.LookupLists = [];
          if (self.Scores === null)
            self.Scores = [];
          if (self.BenchmarksByGrade === null)
            self.BenchmarksByGrade = [];
        }, function (response) {
        });
      };
    };
    return NSObservationSummaryTeamMeetingManager;
  }
]).factory('NSDistrictAssessmentAvailabilityManager', [
  '$http',
  'webApiBaseUrl',
  function ($http, webApiBaseUrl) {
    var NSDistrictAssessmentAvailabilityManager = function () {
      var self = this;
      self.initialize = function () {
        var url = webApiBaseUrl + '/api/AssessmentAvailability/GetAssessmentList/';
        var promise = $http.get(url);
        return promise.then(function (response) {
          self.Assessments = response.data.Assessments;
        });
      };
      self.saveAvailability = function (availability) {
        var paramObj = {
            Id: availability.Id,
            AssessmentIsAvailable: availability.AssessmentIsAvailable
          };
        var url = webApiBaseUrl + '/api/AssessmentAvailability/UpdateAssessmentAvailability/';
        var promise = $http.post(url, paramObj);
        return promise.then(function (response) {
          return response.data.isValid;
        });
      };
      self.initialize();
    };
    return NSDistrictAssessmentAvailabilityManager;
  }
]).factory('NSSchoolAssessmentAvailabilityManager', [
  '$http',
  '$routeParams',
  'webApiBaseUrl',
  function ($http, $routeParams, webApiBaseUrl) {
    var NSSchoolAssessmentAvailabilityManager = function () {
      var self = this;
      self.initialize = function (school) {
        if (school == null || angular.isUndefined(school)) {
          return;
        }
        var paramObj = { Id: school.id };
        var url = webApiBaseUrl + '/api/AssessmentAvailability/GetSchoolAssessments/';
        var promise = $http.post(url, paramObj);
        return promise.then(function (response) {
          self.SchoolAssessments = response.data.SchoolAssessments;
        });
      };
      self.saveAvailability = function (availability) {
        var paramObj = {
            AssessmentId: availability.AssessmentId,
            SchoolId: availability.SchoolId,
            AssessmentIsAvailable: availability.AssessmentIsAvailable
          };
        var url = webApiBaseUrl + '/api/AssessmentAvailability/UpdateSchoolAssessmentAvailability/';
        var promise = $http.post(url, paramObj);
        return promise.then(function (response) {
          return response.data.isValid;
        });
      };
    };
    return NSSchoolAssessmentAvailabilityManager;
  }
]).factory('NSStaffAssessmentAvailabilityManager', [
  '$http',
  '$routeParams',
  'webApiBaseUrl',
  function ($http, $routeParams, webApiBaseUrl) {
    var NSStaffAssessmentAvailabilityManager = function () {
      var self = this;
      self.initialize = function () {
        var url = webApiBaseUrl + '/api/AssessmentAvailability/GetStaffAssessments/';
        var promise = $http.post(url);
        return promise.then(function (response) {
          self.StaffAssessments = response.data.StaffAssessments;
        });
      };
      self.saveAvailability = function (availability) {
        var paramObj = {
            AssessmentId: availability.AssessmentId,
            StaffId: availability.StaffId,
            AssessmentIsAvailable: availability.AssessmentIsAvailable
          };
        var url = webApiBaseUrl + '/api/AssessmentAvailability/UpdateStaffAssessmentAvailability/';
        var promise = $http.post(url, paramObj);
        return promise.then(function (response) {
          return response.data.isValid;
        });
      };
      self.initialize();
    };
    return NSStaffAssessmentAvailabilityManager;
  }
]).factory('NSSortManager', [
  '$http',
  function ($http) {
    var NSSortManager = function () {
      var self = this;
      self.manualSortHeaders = {};
      self.sortArray = [];
      self.headerClassArray = [];
      self.fieldResultName = '';
      self.fieldsParent = {};
      self.initialize = function (manualSortHeaders, sortArray, headerClassArray, fieldResultName, fieldsParent) {
        self.manualSortHeaders = manualSortHeaders;
        self.sortArray = sortArray;
        self.headerClassArray = headerClassArray;
        self.fieldResultName = fieldResultName;
        self.fieldsParent = fieldsParent;
      };
      self.sort = function (column) {
        var columnIndex = -1;
        // if this is not a first or lastname column
        if (!isNaN(parseInt(column))) {
          columnIndex = column;
          switch (self.fieldsParent.Fields[column].FieldType) {
          case 'Textfield':
            column = self.fieldResultName + '[' + column + '].StringValue';
            break;
          case 'DecimalRange':
            column = self.fieldResultName + '[' + column + '].DecimalValue';
            break;
          case 'DropdownRange':
            column = self.fieldResultName + '[' + column + '].IntValue';
            break;
          case 'DropdownFromDB':
            column = self.fieldResultName + '[' + column + '].IntValue';
            break;
          case 'CalculatedFieldDbOnly':
            column = self.fieldResultName + '[' + column + '].StringValue';
            break;
          case 'CalculatedFieldDbBacked':
            column = self.fieldResultName + '[' + column + '].IntValue';
            break;
          case 'CalculatedFieldDbBackedString':
            column = self.fieldResultName + '[' + column + '].StringValue';
            break;
          case 'CalculatedFieldClientOnly':
            column = self.fieldResultName + '[' + column + '].StringValue';
            //shouldnt even be used in sorting
            break;
          default:
            column = self.fieldResultName + '[' + column + '].IntValue';
            break;
          }
        }
        var bFound = false;
        for (var j = 0; j < self.sortArray.length; j++) {
          // if it is already on the list, reverse the sort
          if (self.sortArray[j].indexOf(column) >= 0) {
            bFound = true;
            // is it already negative? if so, remove it
            if (self.sortArray[j].indexOf('-') === 0) {
              if (columnIndex > -1) {
                self.headerClassArray[columnIndex] = 'fa';
              } else if (column === 'FirstName') {
                self.manualSortHeaders.firstNameHeaderClass = 'fa';
              } else if (column === 'LastName') {
                self.manualSortHeaders.lastNameHeaderClass = 'fa';
              } else if (column === 'StudentName') {
                self.manualSortHeaders.studentNameHeaderClass = 'fa';
              } else if (column === 'SchoolName') {
                self.manualSortHeaders.schoolNameHeaderClass = 'fa';
              } else if (column === 'GradeOrder') {
                self.manualSortHeaders.gradeNameHeaderClass = 'fa';
              } else if (column === 'DelimitedTeacherSections') {
                self.manualSortHeaders.teacherNameHeaderClass = 'fa';
              } else if (column === 'FPValueID') {
                self.manualSortHeaders.fpValueIDHeaderClass = 'fa';
              } else if (column === 'FieldResults[0].IntValue') {
                self.manualSortHeaders.totalScoreHeaderClass = 'fa';
              }
              self.sortArray.splice(j, 1);
            } else {
              if (columnIndex > -1) {
                self.headerClassArray[columnIndex] = 'fa fa-chevron-down';
              } else if (column === 'FirstName') {
                self.manualSortHeaders.firstNameHeaderClass = 'fa fa-chevron-down';
              } else if (column === 'LastName') {
                self.manualSortHeaders.lastNameHeaderClass = 'fa fa-chevron-down';
              } else if (column === 'StudentName') {
                self.manualSortHeaders.studentNameHeaderClass = 'fa fa-chevron-down';
              } else if (column === 'SchoolName') {
                self.manualSortHeaders.schoolNameHeaderClass = 'fa fa-chevron-down';
              } else if (column === 'GradeOrder') {
                self.manualSortHeaders.gradeNameHeaderClass = 'fa fa-chevron-down';
              } else if (column === 'DelimitedTeacherSections') {
                self.manualSortHeaders.teacherNameHeaderClass = 'fa fa-chevron-down';
              } else if (column === 'FPValueID') {
                self.manualSortHeaders.fpValueIDHeaderClass = 'fa fa-chevron-down';
              } else if (column === 'FieldResults[0].IntValue') {
                self.manualSortHeaders.totalScoreHeaderClass = 'fa fa-chevron-down';
              }
              self.sortArray[j] = '-' + self.sortArray[j];
            }
            break;
          }
        }
        if (!bFound) {
          self.sortArray.push(column);
          if (columnIndex > -1) {
            self.headerClassArray[columnIndex] = 'fa fa-chevron-up';
          } else if (column === 'FirstName') {
            self.manualSortHeaders.firstNameHeaderClass = 'fa fa-chevron-up';
          } else if (column === 'LastName') {
            self.manualSortHeaders.lastNameHeaderClass = 'fa fa-chevron-up';
          } else if (column === 'StudentName') {
            self.manualSortHeaders.studentNameHeaderClass = 'fa fa-chevron-up';
          } else if (column === 'SchoolName') {
            self.manualSortHeaders.schoolNameHeaderClass = 'fa fa-chevron-up';
          } else if (column === 'GradeOrder') {
            self.manualSortHeaders.gradeNameHeaderClass = 'fa fa-chevron-up';
          } else if (column === 'DelimitedTeacherSections') {
            self.manualSortHeaders.teacherNameHeaderClass = 'fa fa-chevron-up';
          } else if (column === 'FPValueID') {
            self.manualSortHeaders.fpValueIDHeaderClass = 'fa fa-chevron-up';
          } else if (column === 'FieldResults[0].IntValue') {
            self.manualSortHeaders.totalScoreHeaderClass = 'fa fa-chevron-up';
          }
        }
      };
    };
    return NSSortManager;
  }
]);
;
'use strict';
angular.module('theme.directives', []).directive('staticInclude', [
  '$templateCache',
  '$compile',
  function ($templateCache, $compile) {
    return {
      restrict: 'AE',
      link: function (scope, element, attrs) {
        var template = $templateCache.get(attrs.source);
        element.html(template);
        $compile(element.contents())(scope);
      }
    };
  }
]).directive('disableAnimation', [
  '$animate',
  function ($animate) {
    return {
      restrict: 'A',
      link: function ($scope, $element, $attrs) {
        $attrs.$observe('disableAnimation', function (value) {
          $animate.enabled(!value, $element);
        });
      }
    };
  }
]).directive('slideOut', function () {
  return {
    restrict: 'A',
    scope: { show: '=slideOut' },
    link: function (scope, element, attr) {
      element.hide();
      scope.$watch('show', function (newVal, oldVal) {
        if (newVal !== oldVal) {
          element.slideToggle({
            complete: function () {
              scope.$apply();
            }
          });
        }
      });
    }
  };
}).directive('ngEnter', function () {
  return function (scope, element, attrs) {
    element.bind('keydown keypress', function (event) {
      if (event.which === 13) {
        scope.$apply(function () {
          scope.$eval(attrs.ngEnter);
        });
        event.preventDefault();
      }
    });
  };
}).directive('slideOutNav', [
  '$timeout',
  function ($t) {
    return {
      restrict: 'A',
      scope: { show: '=slideOutNav' },
      link: function (scope, element, attr) {
        scope.$watch('show', function (newVal, oldVal) {
          if ($('body').hasClass('collapse-leftbar')) {
            if (newVal == true)
              element.css('display', 'block');
            else
              element.css('display', 'none');
            return;
          }
          if (newVal == true) {
            element.slideDown({
              complete: function () {
                $t(function () {
                  scope.$apply();
                });
              }
            });
          } else if (newVal == false) {
            element.slideUp({
              complete: function () {
                $t(function () {
                  scope.$apply();
                });
              }
            });
          }
        });
      }
    };
  }
]).directive('panel', function () {
  return {
    restrict: 'E',
    transclude: true,
    scope: {
      panelClass: '@',
      heading: '@',
      panelIcon: '@'
    },
    templateUrl: 'templates/panel.html'
  };
}).directive('pulsate', function () {
  return {
    scope: { pulsate: '=' },
    link: function (scope, element, attr) {
      // stupid hack to prevent FF from throwing error
      if (element.css('background-color') == 'transparent') {
        element.css('background-color', 'rgba(0,0,0,0.01)');
      }
      $(element).pulsate(scope.pulsate);
    }
  };
}).directive('prettyprint', function () {
  return {
    restrict: 'C',
    link: function postLink(scope, element, attrs) {
      element.html(prettyPrintOne(element.html(), '', true));
    }
  };
}).directive('passwordVerify', function () {
  return {
    require: 'ngModel',
    scope: { passwordVerify: '=' },
    link: function (scope, element, attrs, ctrl) {
      scope.$watch(function () {
        var combined;
        if (scope.passwordVerify || ctrl.$viewValue) {
          combined = scope.passwordVerify + '_' + ctrl.$viewValue;
        }
        return combined;
      }, function (value) {
        if (value) {
          ctrl.$parsers.unshift(function (viewValue) {
            var origin = scope.passwordVerify;
            if (origin !== viewValue) {
              ctrl.$setValidity('passwordVerify', false);
              return undefined;
            } else {
              ctrl.$setValidity('passwordVerify', true);
              return viewValue;
            }
          });
        }
      });
    }
  };
}).directive('backgroundSwitcher', function () {
  return {
    restrict: 'EA',
    link: function (scope, element, attr) {
      $(element).click(function () {
        $('body').css('background', $(element).css('background'));
      });
    }
  };
}).directive('panelControls', [function () {
    return {
      restrict: 'E',
      require: '?^tabset',
      link: function (scope, element, attrs, tabsetCtrl) {
        var panel = $(element).closest('.panel');
        if (panel.hasClass('.ng-isolate-scope') == false) {
          $(element).appendTo(panel.find('.options'));
        }
      }
    };
  }]).directive('panelControlCollapse', function () {
  return {
    restrict: 'EAC',
    link: function (scope, element, attr) {
      element.bind('click', function () {
        $(element).toggleClass('fa-chevron-down fa-chevron-up');
        $(element).closest('.panel').find('.panel-body').slideToggle({ duration: 200 });
        $(element).closest('.panel-heading').toggleClass('rounded-bottom');
      });
      return false;
    }
  };
}).directive('icheck', function ($timeout, $parse) {
  return {
    require: '?ngModel',
    link: function ($scope, element, $attrs, ngModel) {
      return $timeout(function () {
        var parentLabel = element.parent('label');
        if (parentLabel.length)
          parentLabel.addClass('icheck-label');
        var value;
        value = $attrs['value'];
        $scope.$watch($attrs['ngModel'], function (newValue) {
          $(element).iCheck('update');
        });
        return $(element).iCheck({
          checkboxClass: 'icheckbox_minimal-blue',
          radioClass: 'iradio_minimal-blue'
        }).on('ifChanged', function (event) {
          if ($(element).attr('type') === 'checkbox' && $attrs['ngModel']) {
            $scope.$apply(function () {
              return ngModel.$setViewValue(event.target.checked);
            });
          }
          if ($(element).attr('type') === 'radio' && $attrs['ngModel']) {
            return $scope.$apply(function () {
              return ngModel.$setViewValue(value);
            });
          }
        });
      });
    }
  };
}).directive('knob', function () {
  return {
    restrict: 'EA',
    template: '<input class="dial" type="text"/>',
    require: 'ngModel',
    scope: { options: '=' },
    replace: true,
    link: function (scope, element, attr, ngModel) {
      $(element).knob({
        change: function (value) {
          scope.$apply(function () {
            ngModel.$setViewValue(value);
          });
        }
      });
      ngModel.$render = function () {
        $(element).val(ngModel.$viewValue).trigger('change');
      };
    }
  };
}).directive('uiBsSlider', [
  '$timeout',
  function ($timeout) {
    return {
      link: function (scope, element, attr) {
        // $timeout is needed because certain wrapper directives don't
        // allow for a correct calculaiton of width
        $timeout(function () {
          element.slider();
        });
      }
    };
  }
]).directive('tileLarge', function () {
  return {
    restrict: 'E',
    scope: { item: '=data' },
    templateUrl: 'templates/tile-large.html',
    replace: true,
    transclude: true
  };
}).directive('tileMini', function () {
  return {
    restrict: 'E',
    scope: { item: '=data' },
    replace: true,
    templateUrl: 'templates/tile-mini.html'
  };
}).directive('tile', function () {
  return {
    restrict: 'E',
    scope: {
      heading: '@',
      type: '@'
    },
    transclude: true,
    templateUrl: 'templates/tile-generic.html',
    link: function (scope, element, attr) {
      var heading = element.find('tile-heading');
      if (heading.length) {
        heading.appendTo(element.find('.tiles-heading'));
      }
    },
    replace: true
  };
}).directive('jscrollpane', [
  '$timeout',
  function ($timeout) {
    return {
      restrict: 'A',
      scope: { options: '=jscrollpane' },
      link: function (scope, element, attr) {
        $timeout(function () {
          if (navigator.appVersion.indexOf('Win') != -1)
            element.jScrollPane($.extend({ mouseWheelSpeed: 20 }, scope.options));
          else
            element.jScrollPane(scope.options);
          element.on('click', '.jspVerticalBar', function (event) {
            event.preventDefault();
            event.stopPropagation();
          });
          element.bind('mousewheel', function (e) {
            e.preventDefault();
          });
        });
      }
    };
  }
]).directive('stickyScroll', function () {
  return {
    restrict: 'A',
    link: function (scope, element, attr) {
      function stickyTop() {
        var topMax = parseInt(attr.stickyScroll);
        var headerHeight = $('header').height();
        if (headerHeight > topMax)
          topMax = headerHeight;
        if ($('body').hasClass('static-header') == false)
          return element.css('top', topMax + 'px');
        var window_top = $(window).scrollTop();
        var div_top = element.offset().top;
        if (window_top < topMax) {
          element.css('top', topMax - window_top + 'px');
        } else {
          element.css('top', 0 + 'px');
        }
      }
      $(function () {
        $(window).scroll(stickyTop);
        stickyTop();
      });
    }
  };
}).directive('rightbarRightPosition', function () {
  return {
    restrict: 'A',
    scope: { isFixedLayout: '=rightbarRightPosition' },
    link: function (scope, element, attr) {
      scope.$watch('isFixedLayout', function (newVal, oldVal) {
        if (newVal != oldVal) {
          setTimeout(function () {
            var $pc = $('#page-content');
            var ending_right = $(window).width() - ($pc.offset().left + $pc.outerWidth());
            if (ending_right < 0)
              ending_right = 0;
            $('#page-rightbar').css('right', ending_right);
          }, 100);
        }
      });
    }
  };
}).directive('fitHeight', [
  '$window',
  '$timeout',
  '$location',
  function ($window, $timeout, $location) {
    return {
      restrict: 'A',
      scope: true,
      link: function (scope, element, attr) {
        scope.docHeight = $(document).height();
        var setHeight = function (newVal) {
          var diff = $('header').height();
          if ($('body').hasClass('layout-horizontal'))
            diff += 112;
          if (newVal - diff > element.outerHeight()) {
            element.css('min-height', newVal - diff + 'px');
          } else {
            element.css('min-height', $(window).height() - diff);
          }
        };
        scope.$watch('docHeight', function (newVal, oldVal) {
          setHeight(newVal);
        });
        $(window).on('resize', function () {
          setHeight($(document).height());
        });
        var resetHeight = function () {
          scope.docHeight = $(document).height();
          $timeout(resetHeight, 1000);
        };
        $timeout(resetHeight, 1000);
      }
    };
  }
]).directive('jscrollpaneOn', [
  '$timeout',
  function ($timeout) {
    return {
      restrict: 'A',
      scope: { applyon: '=jscrollpaneOn' },
      link: function (scope, element, attr) {
        scope.$watch('applyon', function (newVal) {
          if (newVal == false) {
            var api = element.data('jsp');
            if (api)
              api.destroy();
            return;
          }
          $timeout(function () {
            element.jScrollPane({ autoReinitialise: true });
          });
        });
      }
    };
  }
]).directive('backToTop', function () {
  return {
    restrict: 'AE',
    link: function (scope, element, attr) {
      element.click(function (e) {
        $('body').scrollTop(0);
      });
    }
  };
}).directive('nsErrorDisplay', [
  'progressLoader',
  function (progressLoader) {
    return {
      restrict: 'E',
      templateUrl: 'templates/ns-errors.html',
      link: function (scope, element, attr) {
        scope.errors = [];
        scope.$on('NSHTTPError', function (event, data) {
          scope.errors = [];
          scope.errors.push({
            type: 'danger',
            msg: data
          });
          $('html, body').animate({ scrollTop: 0 }, 'fast');
          progressLoader.end();
        });
        scope.$on('NSHTTPClear', function (event, data) {
          scope.errors = [];
        });
      }
    };
  }
]).directive('assessmentPreview', [
  'Assessment',
  '$routeParams',
  function (Assessment, $routeParams) {
    return {
      restrict: 'E',
      templateUrl: 'templates/assessment-preview.html',
      link: function (scope, element, attr) {
        scope.assessment = Assessment.get({ id: $routeParams.id });
        scope.allFields = scope.assessment.Fields;
      }
    };
  }
]).directive('assessmentField', [
  'Assessment',
  '$routeParams',
  '$compile',
  '$templateCache',
  '$http',
  function (Assessment, $routeParams, $compile, $templateCache, $http) {
    var getTemplate = function (field) {
      var type = field.FieldType;
      var template = '';
      switch (type) {
      case 'DateCheckbox':
        template = $templateCache.get('templates/assessment-datecheckbox.html');
        break;
      case 'Textfield':
        template = $templateCache.get('templates/assessment-textfield.html');
        break;
      case 'DecimalRange':
        template = $templateCache.get('templates/assessment-decimal.html');
        break;
      case 'DropdownRange':
        template = $templateCache.get('templates/assessment-dropdownrange.html');
        break;
      case 'DropdownFromDB':
        template = $templateCache.get('templates/assessment-dropdownfromdb.html');
        break;
      case 'CalculatedField':
        template = $templateCache.get('templates/assessment-calculatedlabel.html');
        break;
      default:
        template = $templateCache.get('templates/assessment-textfield.html');
        break;
      }
      return template;
    };
    return {
      restrict: 'E',
      template: '<div>{{field}}</div>',
      scope: {
        field: '=',
        allFields: '='
      },
      link: function (scope, element, attr) {
        var SumFunction = function (args) {
          var aryFields = args.split(',');
          var sum = 0;
          for (var i = 0; i < aryFields.length; i++) {
            var fieldId = parseInt(aryFields[i]);
            for (var j = 0; j < scope.allFields.length; j++) {
              if (scope.allFields[j].Id == fieldId) {
                sum += scope.allFields[j].FieldOrder;
              }
            }
          }
          return sum;
        };
        var BenchmarkFunction = function (args) {
          return 334;
        };
        function getCalculationFunction(field) {
          var calcFunctionName = field.CalculationFunction;
          var calcFunction;
          switch (calcFunctionName) {
          case 'Sum':
            calcFunction = SumFunction;
            break;
          case 'BenchmarkLevel':
            calcFunction = BenchmarkFunction;
            break;
          default:
            calcFunction = null;
            break;
          }
          return calcFunction;
        }
        var templateText = getTemplate(scope.field);
        var calcFunction = getCalculationFunction(scope.field);
        var dataToAppend = templateText;
        element.html(dataToAppend);
        $compile(element.contents())(scope);
        scope.Calculate = function (args) {
          return calcFunction(args);
        };
        scope.lookupValues = [];
        scope.loadLookupValues = function (lookupFieldName) {
          return scope.lookupValues.length ? null : $http.get('/api/assessment/getlookupfield/' + lookupFieldName).success(function (data) {
            scope.lookupValues = data;
          });
        };
      }
    };
  }
]).directive('assessmentEditField', [
  'Assessment',
  '$routeParams',
  '$compile',
  '$templateCache',
  '$http',
  function (Assessment, $routeParams, $compile, $templateCache, $http) {
    var getTemplate = function (field) {
      var type = field.FieldType;
      var template = '';
      switch (type) {
      case 'DateCheckbox':
        template = $templateCache.get('templates/assessment-datecheckbox.html');
        break;
      case 'Textfield':
        template = $templateCache.get('templates/assessment-textfield.html');
        break;
      case 'DecimalRange':
        template = $templateCache.get('templates/assessment-decimal.html');
        break;
      case 'DropdownRange':
        template = $templateCache.get('templates/assessment-dropdownrange.html');
        break;
      case 'DropdownFromDB':
        template = $templateCache.get('templates/assessment-dropdownfromdb.html');
        break;
      case 'CalculatedFieldClientOnly':
        template = $templateCache.get('templates/assessment-calculatedlabel.html');
        break;
      case 'CalculatedFieldDbBacked':
        template = $templateCache.get('templates/assessment-calculatedlabeldbbacked.html');
        break;
      case 'CalculatedFieldDbBackedString':
        template = $templateCache.get('templates/assessment-calculatedlabeldbbackedstring.html');
        break;
      case 'CalculatedFieldDbOnly':
        template = $templateCache.get('templates/assessment-calculatedlabeldbonly.html');
        break;
      default:
        template = $templateCache.get('templates/assessment-textfield.html');
        break;
      }
      return template;
    };
    return {
      restrict: 'E',
      template: '<div>{{result}}</div>',
      scope: {
        result: '=',
        allResults: '=',
        eForm: '=',
        lookupFieldsArray: '='
      },
      link: function (scope, element, attr) {
        // get our own lookupFields
        scope.lookupValues = [];
        for (var i = 0; i < scope.lookupFieldsArray.length; i++)
          if (scope.lookupFieldsArray[i].LookupColumnName === scope.result.Field.LookupFieldName) {
            scope.lookupValues = scope.lookupFieldsArray[i].LookupFields;
            break;
          }
        var SumFunction = function (args) {
          var aryFields = args.split(',');
          var sum = 0;
          for (var i = 0; i < aryFields.length; i++) {
            var fieldDbColumn = aryFields[i];
            for (var j = 0; j < scope.allResults.length; j++) {
              if (scope.allResults[j].DbColumn == fieldDbColumn) {
                sum += scope.allResults[j].IntValue;
              }
            }
          }
          scope.result.StringValue = sum;
          return sum;
        };
        var BenchmarkFunction = function (args) {
          return 3;
          var FPValueId = 0;
          var Accuracy = 0;
          var CompScore = 0;
          // calculate compscore
          for (var j = 0; j < scope.allResults.length; j++) {
            if (scope.allResults[j].DbColumn === 'Beyond' || scope.allResults[j].DbColumn === 'About' || scope.allResults[j].DbColumn === 'Within' || scope.allResults[j].DbColumn === 'ExtraPt') {
              CompScore += scope.allResults[j].IntValue;
            }
          }
          // get accuracy
          for (var j = 0; j < scope.allResults.length; j++) {
            if (scope.allResults[j].DbColumn === 'Accuracy') {
              Accuracy = scope.allResults[j].IntValue;
              break;
            }
          }
          // FPScore
          for (var j = 0; j < scope.allResults.length; j++) {
            if (scope.allResults[j].DbColumn === 'FPValueID') {
              FPValueId = scope.allResults[j].IntValue;
              break;
            }
          }
          //scope.result.StringValue = 'test';
          if (Accuracy == null) {
            Accuracy = 0;
          }
          if (CompScore == null) {
            CompScore = 0;
          }
          if (FPValueId == null) {
            FPValueId = 0;
          }
          //return 'test';
          // get benchmarklevel
          $http.get('/api/assessment/getbenchmarklevel/' + FPValueId + '/' + Accuracy + '/' + CompScore).success(function (data) {
            return data;
          }).error(function (data, status, headers, config) {
            return 'error';
          });
        };
        function getCalculationFunction(field) {
          var calcFunctionName = field.CalculationFunction;
          var calcFunction;
          switch (calcFunctionName) {
          case 'Sum':
            calcFunction = SumFunction;
            break;
          case 'BenchmarkLevel':
            calcFunction = BenchmarkFunction;
            break;
          default:
            calcFunction = null;
            break;
          }
          return calcFunction;
        }
        var templateText = getTemplate(scope.result.Field);
        var calcFunction = getCalculationFunction(scope.result.Field);
        var dataToAppend = templateText;
        element.html(dataToAppend);
        $compile(element.contents())(scope);
        scope.Calculate = function (args) {
          return calcFunction(args);
        }  //scope.lookupValues = [];
           //scope.loadLookupValues = function (lookupFieldName) {
           //	//return [];
           //		return scope.lookupValues.length ? null : $http.get('/api/assessment/getlookupfield/' + lookupFieldName).success(function (data) {
           //			scope.lookupValues = data;
           //		});
           //}
;
      }
    };
  }
]).directive('manualswitch', function () {
  return {
    restrict: 'AE',
    replace: true,
    transclude: true,
    template: function (element, attrs) {
      var html = '';
      html += '<span';
      html += ' class="switch' + (attrs.class ? ' ' + attrs.class : '') + '"';
      //html += attrs.ngModel ? ' ng-click="' (attrs.ngChange ? '; ' + attrs.ngChange + '()"' : '"') : '';
      html += ' ng-class="{ checked:' + attrs.ngModel + ' }"';
      html += '>';
      html += '<small></small>';
      html += '<input type="checkbox"';
      html += attrs.id ? ' id="' + attrs.id + '"' : '';
      html += attrs.name ? ' name="' + attrs.name + '"' : '';
      html += attrs.ngModel ? ' ng-model="' + attrs.ngModel + '"' : '';
      html += ' style="display:none" />';
      html += '<span class="switch-text">';
      /*adding new container for switch text*/
      html += attrs.on ? '<span class="on">' + attrs.on + '</span>' : '';
      /*switch text on value set by user in directive html markup*/
      html += attrs.off ? '<span class="off">' + attrs.off + '</span>' : ' ';
      /*switch text off value set by user in directive html markup*/
      html += '</span>';
      return html;
    }
  };
}).directive('spellingInventoryTotalColumn', [
  'Assessment',
  '$routeParams',
  '$compile',
  '$templateCache',
  '$http',
  function (Assessment, $routeParams, $compile, $templateCache, $http) {
    var getTemplate = function (field) {
      var template = $templateCache.get('templates/assessment-calculatedfielddbbacked-custom.html');
      return template;
    };
    return {
      restrict: 'E',
      scope: {
        studentResult: '=',
        groups: '=',
        fields: '=',
        currentCategory: '='
      },
      link: function (scope, element, attr) {
        scope.currentField = {};
        // get currentField for current Category
        for (var i = 0; i < scope.fields.length; i++) {
          if (scope.currentCategory.Id === scope.fields[i].CategoryId && scope.fields[i].Page === 2) {
            scope.currentField = scope.fields[i];
            break;
          }
        }
        var SumBoolFunction = function (args) {
          var aryFields = args.split(',');
          var sum = 0;
          for (var i = 0; i < aryFields.length; i++) {
            var fieldDbColumn = aryFields[i];
            for (var j = 0; j < scope.studentResult.FieldResults.length; j++) {
              if (scope.studentResult.FieldResults[j].DbColumn == fieldDbColumn) {
                sum += scope.studentResult.FieldResults[j].BoolValue ? 1 : 0;
              }
            }
          }
          //scope.result.StringValue = sum;
          return sum;
        };
        function getCalculationFunction(field) {
          var calcFunctionName = field.CalculationFunction;
          var calcFunction = SumBoolFunction;
          return calcFunction;
        }
        var templateText = getTemplate(scope.currentField);
        var calcFunction = getCalculationFunction(scope.currentField);
        var dataToAppend = templateText;
        element.html(dataToAppend);
        $compile(element.contents())(scope);
        scope.Calculate = function (args) {
          return calcFunction(args);
        };
      }
    };
  }
]).directive('genericReadOnlyField', [
  '$routeParams',
  '$compile',
  '$templateCache',
  '$http',
  function ($routeParams, $compile, $templateCache, $http) {
    var getTemplate = function (field) {
      var type = field.FieldType;
      var template = '';
      switch (type) {
      case 'DateCheckbox':
        template = $templateCache.get('templates/assessment-datecheckbox-readonly.html');
        break;
      case 'Textfield':
        template = $templateCache.get('templates/assessment-textfield-readonly.html');
        break;
      case 'DecimalRange':
        template = $templateCache.get('templates/assessment-decimal-readonly.html');
        break;
      case 'DropdownRange':
        template = $templateCache.get('templates/assessment-dropdownrange-readonly.html');
        break;
      case 'DropdownFromDB':
        template = $templateCache.get('templates/assessment-dropdownfromdb-readonly.html');
        break;
      case 'CalculatedFieldClientOnly':
        template = $templateCache.get('templates/assessment-calculatedlabel-readonly.html');
        break;
      case 'CalculatedFieldDbBacked':
        template = $templateCache.get('templates/assessment-calculatedlabeldbbacked-readonly.html');
        break;
      case 'CalculatedFieldDbBackedString':
        template = $templateCache.get('templates/assessment-calculatedlabeldbbackedstring-readonly.html');
        break;
      case 'CalculatedFieldDbOnly':
        template = $templateCache.get('templates/assessment-calculatedlabeldbonly-readonly.html');
        break;
      default:
        template = $templateCache.get('templates/assessment-textfield-readonly.html');
        break;
      }
      return template;
    };
    return {
      restrict: 'E',
      template: '<div>{{result}}</div>',
      scope: {
        result: '=',
        allResults: '=',
        lookupFieldsArray: '='
      },
      link: function (scope, element, attr) {
        // get our own lookupFields
        scope.lookupValues = [];
        for (var i = 0; i < scope.lookupFieldsArray.length; i++)
          if (scope.lookupFieldsArray[i].LookupColumnName === scope.result.LookupFieldName) {
            scope.lookupValues = scope.lookupFieldsArray[i].LookupFields;
            break;
          }
        var SumFunction = function (args) {
          var aryFields = args.split(',');
          var sum = 0;
          for (var i = 0; i < aryFields.length; i++) {
            var fieldDbColumn = aryFields[i];
            for (var j = 0; j < scope.allResults.length; j++) {
              if (scope.allResults[j].DbColumn == fieldDbColumn) {
                sum += scope.allResults[j].IntValue;
              }
            }
          }
          scope.result.StringValue = sum;
          return sum;
        };
        function getCalculationFunction(field) {
          var calcFunctionName = field.CalculationFunction;
          var calcFunction;
          switch (calcFunctionName) {
          case 'Sum':
            calcFunction = SumFunction;
            break;
          default:
            calcFunction = null;
            break;
          }
          return calcFunction;
        }
        var templateText = getTemplate(scope.result.Field);
        var calcFunction = getCalculationFunction(scope.result.Field);
        var dataToAppend = templateText;
        element.html(dataToAppend);
        $compile(element.contents())(scope);
        scope.Calculate = function (args) {
          return calcFunction(args);
        };
      }
    };
  }
]).directive('testDueDateFooter', [
  '$routeParams',
  '$compile',
  '$templateCache',
  '$http',
  function ($routeParams, $compile, $templateCache, $http) {
    return {
      restrict: 'EA',
      scope: { tdds: '=' },
      link: function (scope, element, attr) {
        scope.$watch('tdds', function () {
          var outputHtml = '';
          outputHtml += '<table><tbody><tr>';
          for (var i = 0; i < scope.tdds.length; i++) {
            outputHtml += '<td style="padding-right:5px;"><span style="border:1px solid black;display:inline-block;height:20px;width:25px;Background-color:' + scope.tdds[i].Hex + ';"></span></td><td style="padding-right:10px"><strong>' + scope.tdds[i].DisplayDate + '</strong></td>';
          }
          outputHtml += '</tr></tbody></table>';
          outputHtml += '<div><span class=\'badge\' style=\'background-color:black;color:white;\'>X</span> = Score Lower Than Previous Test</div>';
          element.html(outputHtml);
          $compile(element.contents())(scope);
        });
      }
    };
  }
]).directive('standardReportHeader', [
  '$routeParams',
  '$compile',
  '$templateCache',
  '$http',
  '$filter',
  function ($routeParams, $compile, $templateCache, $http, $filter) {
    return {
      scope: {
        options: '=',
        heading: '='
      },
      restrict: 'E',
      templateUrl: 'templates/standard-report-header.html',
      link: function (scope, element, attr) {
        scope.headingHtml = function () {
          return $filter('unsafe')('<a class="navbar-brand" href="#/">North Star <span class="navbar-brand-faded">Educational Tools</span></a>');
        };
      }
    };
  }
]).directive('studentDashboardLink', [
  '$routeParams',
  '$compile',
  '$templateCache',
  '$uibModal',
  '$timeout',
  function ($routeParams, $compile, $templateCache, $uibModal, $timeout) {
    return {
      restrict: 'EA',
      templateUrl: 'templates/student-dashboard-link.html',
      scope: {
        studentId: '=',
        studentName: '='
      },
      link: function (scope, element, attr) {
        scope.openStudentDashboardDialog = function () {
          var modalInstance = $uibModal.open({
              templateUrl: 'studentDashboardViewer.html',
              scope: scope,
              controller: function ($scope, $uibModalInstance) {
                // use jquery to change the class of the modal content
                $timeout(function () {
                  $('div.modal-dialog').addClass('modal-dialog-max');
                  $('div.modal-content').addClass('modal-content-max');
                }, 250);
                $scope.settings = {
                  selectedStudent: {
                    id: $scope.studentId,
                    text: $scope.studentName
                  }
                };
                $scope.cancel = function () {
                  $uibModalInstance.dismiss('cancel');  // set it back
                };
              },
              size: 'md'
            });
        };
      }
    };
  }
]).directive('attendanceBadge', [
  '$routeParams',
  '$compile',
  '$templateCache',
  function ($routeParams, $compile, $templateCache) {
    return {
      restrict: 'EA',
      scope: { dayStatus: '=' },
      link: function (scope, element, attr) {
        scope.$watch('dayStatus', function () {
          var outputHtml = '';
          if (scope.dayStatus === 'Scheduled To Meet') {
            outputHtml = '<span class=\'badge badge-primary\'><i class=\'fa fa-calendar\'></i> ' + scope.dayStatus + '</span>';
          } else if (scope.dayStatus === 'None') {
            outputHtml = '<span class=\'badge badge-default\'><i class=\'fa fa-square\'></i> ' + scope.dayStatus + '</span>';
          } else if (scope.dayStatus === 'No School') {
            // this is for district and school holidays
            outputHtml = '<span class=\'badge badge-default\'><i class=\'fa fa-lock\'></i> ' + scope.dayStatus + '</span>';
          } else if (scope.dayStatus === 'None') {
            outputHtml = '<span class=\'badge badge-default\'><i class=\'fa fa-square\'></i> ' + scope.dayStatus + '</span>';
          } else if (scope.dayStatus === 'Teacher Absent' || scope.dayStatus === 'Teacher Unavailable' || scope.dayStatus === 'Child Absent' || scope.dayStatus === 'Child Unavailable') {
            outputHtml = '<span class=\'badge badge-danger\'><i class=\'fa fa-times\'></i> ' + scope.dayStatus + '</span>';
          } else if (scope.dayStatus === 'Non-Cycle Day') {
            outputHtml = '<span class=\'badge badge-info\'><i class=\'fa fa-info\'></i> ' + scope.dayStatus + '</span>';
          } else if (scope.dayStatus === 'Make-Up Lesson' || scope.dayStatus === 'Intervention Delivered') {
            outputHtml = '<span class=\'badge badge-success\'><i class=\'fa fa-check\'></i> ' + scope.dayStatus + '</span>';
          } else {
            outputHtml = scope.dayStatus;  // TODO: log unexpected result
          }
          element.html(outputHtml);
          $compile(element.contents())(scope);
        });
      }
    };
  }
]).directive('nsRepeatComplete', [
  '$rootScope',
  '$timeout',
  function ($rootScope, $timeout) {
    return {
      restrict: 'A',
      link: function (scope, element, attr) {
        if (scope.$last === true) {
          $timeout(function () {
            scope.$emit('ngRepeatFinished');
          });
        }
      }
    };
  }
]).directive('onFinishRender', [
  '$timeout',
  '$parse',
  function ($timeout, $parse) {
    return {
      restrict: 'A',
      link: function (scope, element, attr) {
        if (scope.$last === true) {
          $timeout(function () {
            scope.$emit('ngRepeatFinished');
            if (!!attr.onFinishRender) {
              $parse(attr.onFinishRender)(scope);
            }
          });
        }
      }
    };
  }
]).value('THROTTLE_MILLISECONDS', 250).directive('infiniteScroll', [
  '$rootScope',
  '$window',
  '$interval',
  'THROTTLE_MILLISECONDS',
  function ($rootScope, $window, $interval, THROTTLE_MILLISECONDS) {
    return {
      scope: {
        infiniteScroll: '&',
        infiniteScrollContainer: '=',
        infiniteScrollDistance: '=',
        infiniteScrollDisabled: '=',
        infiniteScrollUseDocumentBottom: '=',
        infiniteScrollListenForEvent: '@'
      },
      link: function (scope, elem, attrs) {
        var windowElement = angular.element($window);
        var scrollDistance = null;
        var scrollEnabled = null;
        var checkWhenEnabled = null;
        var container = null;
        var immediateCheck = true;
        var useDocumentBottom = false;
        var unregisterEventListener = null;
        var checkInterval = false;
        function height(element) {
          var el = element[0] || element;
          if (isNaN(el.offsetHeight)) {
            return el.document.documentElement.clientHeight;
          }
          return el.offsetHeight;
        }
        function pageYOffset(element) {
          var el = element[0] || element;
          if (isNaN(window.pageYOffset)) {
            return el.document.documentElement.scrollTop;
          }
          return el.ownerDocument.defaultView.pageYOffset;
        }
        function offsetTop(element) {
          if (!(!element[0].getBoundingClientRect || element.css('none'))) {
            return element[0].getBoundingClientRect().top + pageYOffset(element);
          }
          return undefined;
        }
        // infinite-scroll specifies a function to call when the window,
        // or some other container specified by infinite-scroll-container,
        // is scrolled within a certain range from the bottom of the
        // document. It is recommended to use infinite-scroll-disabled
        // with a boolean that is set to true when the function is
        // called in order to throttle the function call.
        function defaultHandler() {
          var containerBottom;
          var elementBottom;
          if (container === windowElement) {
            containerBottom = height(container) + pageYOffset(container[0].document.documentElement);
            elementBottom = offsetTop(elem) + height(elem);
          } else {
            containerBottom = height(container);
            var containerTopOffset = 0;
            if (offsetTop(container) !== undefined) {
              containerTopOffset = offsetTop(container);
            }
            elementBottom = offsetTop(elem) - containerTopOffset + height(elem);
          }
          if (useDocumentBottom) {
            elementBottom = height((elem[0].ownerDocument || elem[0].document).documentElement);
          }
          var remaining = elementBottom - containerBottom;
          var shouldScroll = remaining <= height(container) * scrollDistance + 1;
          if (shouldScroll) {
            checkWhenEnabled = true;
            if (scrollEnabled) {
              if (scope.$$phase || $rootScope.$$phase) {
                scope.infiniteScroll();
              } else {
                scope.$apply(scope.infiniteScroll);
              }
            }
          } else {
            if (checkInterval) {
              $interval.cancel(checkInterval);
            }
            checkWhenEnabled = false;
          }
        }
        // The optional THROTTLE_MILLISECONDS configuration value specifies
        // a minimum time that should elapse between each call to the
        // handler. N.b. the first call the handler will be run
        // immediately, and the final call will always result in the
        // handler being called after the `wait` period elapses.
        // A slimmed down version of underscore's implementation.
        function throttle(func, wait) {
          var timeout = null;
          var previous = 0;
          function later() {
            previous = new Date().getTime();
            $interval.cancel(timeout);
            timeout = null;
            return func.call();
          }
          function throttled() {
            var now = new Date().getTime();
            var remaining = wait - (now - previous);
            if (remaining <= 0) {
              $interval.cancel(timeout);
              timeout = null;
              previous = now;
              func.call();
            } else if (!timeout) {
              timeout = $interval(later, remaining, 1);
            }
          }
          return throttled;
        }
        var handler = THROTTLE_MILLISECONDS != null ? throttle(defaultHandler, THROTTLE_MILLISECONDS) : defaultHandler;
        function handleDestroy() {
          container.unbind('scroll', handler);
          if (unregisterEventListener != null) {
            unregisterEventListener();
            unregisterEventListener = null;
          }
          if (checkInterval) {
            $interval.cancel(checkInterval);
          }
        }
        scope.$on('$destroy', handleDestroy);
        // infinite-scroll-distance specifies how close to the bottom of the page
        // the window is allowed to be before we trigger a new scroll. The value
        // provided is multiplied by the container height; for example, to load
        // more when the bottom of the page is less than 3 container heights away,
        // specify a value of 3. Defaults to 0.
        function handleInfiniteScrollDistance(v) {
          scrollDistance = parseFloat(v) || 0;
        }
        scope.$watch('infiniteScrollDistance', handleInfiniteScrollDistance);
        // If I don't explicitly call the handler here, tests fail. Don't know why yet.
        handleInfiniteScrollDistance(scope.infiniteScrollDistance);
        // infinite-scroll-disabled specifies a boolean that will keep the
        // infnite scroll function from being called; this is useful for
        // debouncing or throttling the function call. If an infinite
        // scroll is triggered but this value evaluates to true, then
        // once it switches back to false the infinite scroll function
        // will be triggered again.
        function handleInfiniteScrollDisabled(v) {
          scrollEnabled = !v;
          if (scrollEnabled && checkWhenEnabled) {
            checkWhenEnabled = false;
            handler();
          }
        }
        scope.$watch('infiniteScrollDisabled', handleInfiniteScrollDisabled);
        // If I don't explicitly call the handler here, tests fail. Don't know why yet.
        handleInfiniteScrollDisabled(scope.infiniteScrollDisabled);
        // use the bottom of the document instead of the element's bottom.
        // This useful when the element does not have a height due to its
        // children being absolute positioned.
        function handleInfiniteScrollUseDocumentBottom(v) {
          useDocumentBottom = v;
        }
        scope.$watch('infiniteScrollUseDocumentBottom', handleInfiniteScrollUseDocumentBottom);
        handleInfiniteScrollUseDocumentBottom(scope.infiniteScrollUseDocumentBottom);
        // infinite-scroll-container sets the container which we want to be
        // infinte scrolled, instead of the whole window. Must be an
        // Angular or jQuery element, or, if jQuery is loaded,
        // a jQuery selector as a string.
        function changeContainer(newContainer) {
          if (container != null) {
            container.unbind('scroll', handler);
          }
          container = newContainer;
          if (newContainer != null) {
            container.bind('scroll', handler);
          }
        }
        changeContainer(windowElement);
        if (scope.infiniteScrollListenForEvent) {
          unregisterEventListener = $rootScope.$on(scope.infiniteScrollListenForEvent, handler);
        }
        function handleInfiniteScrollContainer(newContainer) {
          // TODO: For some reason newContainer is sometimes null instead
          // of the empty array, which Angular is supposed to pass when the
          // element is not defined
          // (https://github.com/sroze/ngInfiniteScroll/pull/7#commitcomment-5748431).
          // So I leave both checks.
          if (!(newContainer != null) || newContainer.length === 0) {
            return;
          }
          var newerContainer;
          if (newContainer.nodeType && newContainer.nodeType === 1) {
            newerContainer = angular.element(newContainer);
          } else if (typeof newContainer.append === 'function') {
            newerContainer = angular.element(newContainer[newContainer.length - 1]);
          } else if (typeof newContainer === 'string') {
            newerContainer = angular.element(document.querySelector(newContainer));
          } else {
            newerContainer = newContainer;
          }
          if (newerContainer == null) {
            throw new Error('invalid infinite-scroll-container attribute.');
          }
          changeContainer(newerContainer);
        }
        scope.$watch('infiniteScrollContainer', handleInfiniteScrollContainer);
        handleInfiniteScrollContainer(scope.infiniteScrollContainer || []);
        // infinite-scroll-parent establishes this element's parent as the
        // container infinitely scrolled instead of the whole window.
        if (attrs.infiniteScrollParent != null) {
          changeContainer(angular.element(elem.parent()));
        }
        // infinte-scoll-immediate-check sets whether or not run the
        // expression passed on infinite-scroll for the first time when the
        // directive first loads, before any actual scroll.
        if (attrs.infiniteScrollImmediateCheck != null) {
          immediateCheck = scope.$eval(attrs.infiniteScrollImmediateCheck);
        }
        function intervalCheck() {
          if (immediateCheck) {
            handler();
          }
          return $interval.cancel(checkInterval);
        }
        checkInterval = $interval(intervalCheck);
        return checkInterval;
      }
    };
  }
]).directive('print-button', [
  '$routeParams',
  '$compile',
  '$bootbox',
  'nsPinesService',
  'webApiBaseUrl',
  '$location',
  function ($routeParams, $compile, $bootbox, nsPinesService, webApiBaseUrl, $location) {
    return {
      restrict: 'EA',
      scope: {
        printLandscape: '=',
        printMultiPage: '=',
        fitWidth: '=',
        fitHeight: '=',
        stretchToFit: '=',
        htmlViewerHeight: '=',
        htmlViewerWidth: '='
      },
      link: function (scope, element, attr) {
        var templateHtml = '<a ng-click="print()" class="btn btn-orange hidden-xs"><i class="fa fa-print"></i> Print</a>';
        scope.setttings = { printInProgress: false };
        scope.print = function () {
          if (scope.settings.printInProgress) {
            $bootbox.alert('Please wait... another print job is already in progress.');
            return;
          }
          $scope.settings.printInProgress = true;
          var notice = nsPinesService.startDynamic();
          var returnObj = {
              PrintLandscape: scope.printLandscape,
              PrintMultiPage: scope.printMultiPage,
              StretchToFit: scope.stretchToFit,
              FitHeight: scope.fitHeight,
              FitWidth: scope.fitWidth,
              HtmlViewerHeight: scope.htmlViewerHeight,
              HtmlViewerWidth: scope.htmlViewerWidth,
              Url: $location.absUrl()
            };
          var printMethod = 'PrintPage';
          if (webApiBaseUrl.indexOf('localhost') > 0) {
            printMethod = 'PrintPageLocal';
          }
          $http.post(webApiBaseUrl + '/api/Print/' + printMethod, returnObj, {
            responseType: 'arraybuffer',
            headers: { accept: 'application/pdf' }
          }).then(function (data) {
            var blob = new Blob([data.data], { type: 'application/pdf' });
            FileSaver.saveAs(blob, 'NorthStarPrint.pdf');
          }).finally(function () {
            $scope.settings.printInProgress = false;
            nsPinesService.endDynamic(notice);
          });
        };
        element.html(outputHtml);
        $compile(element.contents())(scope);
      }
    };
  }
]).directive('convertToNumber', function () {
  return {
    require: 'ngModel',
    link: function (scope, element, attrs, ngModel) {
      ngModel.$parsers.push(function (val) {
        return parseInt(val, 10);
      });
      ngModel.$formatters.push(function (val) {
        return '' + val;
      });
    }
  };
});
;
(function () {
  'use strict';
  angular.module('assessmentFieldsModule', []).directive('nsAssessmentField', [
    'Assessment',
    '$routeParams',
    '$compile',
    '$templateCache',
    '$http',
    'nsLookupFieldService',
    '$uibModal',
    function (Assessment, $routeParams, $compile, $templateCache, $http, nsLookupFieldService, $uibModal) {
      var getTemplate = function (field, mode) {
        var type = field.FieldType;
        var template = '';
        if (type === '' || type === null) {
          type = 'textfield';
        }
        var templateName = 'templates/assessment-' + type + '-' + mode + '.html';
        template = $templateCache.get(templateName.toLocaleLowerCase());
        return template;
      };
      return {
        restrict: 'E',
        scope: true,
        link: function (scope, element, attr) {
          scope.lookupValues = [];
          scope.lookupFieldsArray = [];
          scope.lookupFieldsArray = nsLookupFieldService.LookupFieldsArray;
          var removeit = scope.$watch(function () {
              if (attr.result && attr.allResults) {
                return 'loaded';
              } else {
                return '';
              }
            }, function (newVal) {
              if (newVal == 'loaded') {
                scope.ckeditorOptions = angular.isDefined(attr.ckeditorOptions) ? scope.$eval(attr.ckeditorOptions) : {
                  language: 'en',
                  allowedContent: true,
                  entities: false
                };
                scope.mode = angular.isDefined(attr.mode) ? scope.$eval(attr.mode) : 'readonly';
                scope.result = scope.$eval(attr.result);
                scope.allResults = scope.$eval(attr.allResults);
                scope.eForm = angular.isDefined(attr.eForm) ? scope.$eval(attr.eForm) : null;
                scope.fieldRequired = angular.isDefined(attr.fieldRequired) ? scope.$eval(attr.fieldRequired) : false;
                for (var i = 0; i < scope.lookupFieldsArray.length; i++) {
                  if (scope.lookupFieldsArray[i].LookupColumnName === scope.result.Field.LookupFieldName) {
                    scope.lookupValues = scope.lookupFieldsArray[i].LookupFields;
                    for (var j = 0; j < scope.lookupFieldsArray[i].LookupFields.length; j++) {
                      if (scope.lookupFieldsArray[i].LookupFields[j].FieldSpecificId == scope.result.IntValue) {
                        scope.result.DisplayValue = scope.lookupFieldsArray[i].LookupFields[j].FieldValue;
                        break;
                      }
                    }
                  }
                }
                var templateText = getTemplate(scope.result.Field, scope.mode);
                var calcFunction = getCalculationFunction(scope.result.Field);
                var dataToAppend = templateText;
                //element.html("<span>" + scope.result.DisplayValue + "</span>");
                element.html(dataToAppend);
                $compile(element.contents())(scope);
                removeit();
              }
            }, true);
          scope.toolTipFunction = function () {
            if (scope.result.StringValue) {
              return scope.result.StringValue;
            } else {
              return '';
            }
          };
          scope.commentsModal = function () {
            var modalInstance = $uibModal.open({
                templateUrl: 'commentsViewer.html',
                scope: scope,
                controller: [
                  '$scope',
                  '$uibModalInstance',
                  function ($scope, $uibModalInstance) {
                    $scope.cancel = function () {
                      $uibModalInstance.dismiss('cancel');
                    };
                  }
                ],
                size: 'md'
              });
          };
          scope.validateIfRequired = function (val) {
            if (angular.isDefined(scope.fieldRequired) && scope.fieldRequired === true && !val) {
              return 'This field is required.';
            }
          };
          // get our own lookupFields
          var SumFunction = function (args) {
            var aryFields = args.split(',');
            var sum = 0;
            for (var i = 0; i < aryFields.length; i++) {
              var fieldDbColumn = aryFields[i];
              for (var j = 0; j < scope.allResults.length; j++) {
                if (scope.allResults[j].DbColumn == fieldDbColumn) {
                  sum += scope.allResults[j].IntValue;
                }
              }
            }
            scope.result.StringValue = sum;
            return sum;
          };
          var BenchmarkFunction = function (args) {
            return 3;
            var FPValueId = 0;
            var Accuracy = 0;
            var CompScore = 0;
            // calculate compscore
            for (var j = 0; j < scope.allResults.length; j++) {
              if (scope.allResults[j].DbColumn === 'Beyond' || scope.allResults[j].DbColumn === 'About' || scope.allResults[j].DbColumn === 'Within' || scope.allResults[j].DbColumn === 'ExtraPt') {
                CompScore += scope.allResults[j].IntValue;
              }
            }
            // get accuracy
            for (var j = 0; j < scope.allResults.length; j++) {
              if (scope.allResults[j].DbColumn === 'Accuracy') {
                Accuracy = scope.allResults[j].IntValue;
                break;
              }
            }
            // FPScore
            for (var j = 0; j < scope.allResults.length; j++) {
              if (scope.allResults[j].DbColumn === 'FPValueID') {
                FPValueId = scope.allResults[j].IntValue;
                break;
              }
            }
            //scope.result.StringValue = 'test';
            if (Accuracy == null) {
              Accuracy = 0;
            }
            if (CompScore == null) {
              CompScore = 0;
            }
            if (FPValueId == null) {
              FPValueId = 0;
            }
            //return 'test';
            // get benchmarklevel
            $http.get('/api/assessment/getbenchmarklevel/' + FPValueId + '/' + Accuracy + '/' + CompScore).success(function (data) {
              return data;
            }).error(function (data, status, headers, config) {
              return 'error';
            });
          };
          function getCalculationFunction(field) {
            var calcFunctionName = field.CalculationFunction;
            var calcFunction;
            switch (calcFunctionName) {
            case 'Sum':
              calcFunction = SumFunction;
              break;
            case 'BenchmarkLevel':
              calcFunction = BenchmarkFunction;
              break;
            default:
              calcFunction = null;
              break;
            }
            return calcFunction;
          }
          scope.Calculate = function (args) {
            return calcFunction(args);
          };
        }
      };
    }
  ]).directive('observationSummaryViewField', [
    '$routeParams',
    '$compile',
    '$templateCache',
    '$http',
    'nsLookupFieldService',
    function ($routeParams, $compile, $templateCache, $http, nsLookupFieldService) {
      var getTemplate = function (field) {
        var type = field.ColumnType;
        var template = '';
        switch (type) {
        case 'Textfield':
          template = $templateCache.get('templates/assessment-textfield-readonly.html');
          break;
        case 'DecimalRange':
          template = $templateCache.get('templates/assessment-decimalrange-readonly.html');
          break;
        case 'DropdownRange':
          template = $templateCache.get('templates/assessment-dropdownrange-readonly.html');
          break;
        case 'DropdownFromDB':
          template = $templateCache.get('templates/assessment-dropdownfromdb-readonly.html');
          break;
        case 'CalculatedFieldClientOnly':
          template = $templateCache.get('templates/assessment-calculatedfield-readonly.html');
          break;
        case 'CalculatedFieldDbBacked':
          template = $templateCache.get('templates/assessment-calculatedfielddbbacked-readonly.html');
          break;
        case 'CalculatedFieldDbBackedString':
          template = $templateCache.get('templates/assessment-calculatedfielddbbackedstring-readonly.html');
          break;
        case 'CalculatedFieldDbOnly':
          template = $templateCache.get('templates/assessment-calculatedfielddbonly-readonly.html');
          break;
        default:
          template = $templateCache.get('templates/assessment-textfield-readonly.html');
          break;
        }
        return template;
      };
      return {
        restrict: 'E',
        template: '<div>{{result}}</div>',
        scope: {
          result: '=',
          allResults: '=',
          lookupFieldsArray: '='
        },
        link: function (scope, element, attr) {
          // get our own lookupFields
          scope.lookupValues = [];
          scope.lookupFieldsArray = nsLookupFieldService.LookupFieldsArray;
          //for (var i = 0; i < scope.lookupFieldsArray.length; i++)
          //    if (scope.lookupFieldsArray[i].LookupColumnName === scope.result.LookupFieldName) {
          //        scope.lookupValues = scope.lookupFieldsArray[i].LookupFields;
          //        break;
          //    }
          var SumFunction = function (args) {
            var aryFields = args.split(',');
            var sum = 0;
            for (var i = 0; i < aryFields.length; i++) {
              var fieldDbColumn = aryFields[i];
              for (var j = 0; j < scope.allResults.length; j++) {
                if (scope.allResults[j].DbColumn == fieldDbColumn) {
                  sum += scope.allResults[j].IntValue;
                }
              }
            }
            scope.result.StringValue = sum;
            return sum;
          };
          function getCalculationFunction(field) {
            var calcFunctionName = field.CalculationFunction;
            var calcFunction;
            switch (calcFunctionName) {
            case 'Sum':
              calcFunction = SumFunction;
              break;
            default:
              calcFunction = null;
              break;
            }
            return calcFunction;
          }
          var templateText = getTemplate(scope.result);
          var calcFunction = getCalculationFunction(scope.result);
          var dataToAppend = templateText;
          element.html(dataToAppend);
          $compile(element.contents())(scope);
          scope.Calculate = function (args) {
            return calcFunction(args);
          };
        }
      };
    }
  ]).directive('assessmentEditFieldCustom', [
    'Assessment',
    '$routeParams',
    '$compile',
    '$templateCache',
    '$http',
    'nsLookupFieldService',
    function (Assessment, $routeParams, $compile, $templateCache, $http, nsLookupFieldService) {
      var getTemplate = function (field, orientation) {
        var type = field.FieldType;
        var template = '';
        if (type === '' || type === null) {
          type = 'textfield';
        }
        var templateName = 'templates/assessment-' + type + orientation + '-custom.html';
        template = $templateCache.get(templateName.toLocaleLowerCase());
        return template;
      };
      return {
        restrict: 'E',
        template: '<div>{{result}}</div>',
        scope: {
          currentField: '=',
          studentResult: '=',
          groups: '=',
          fields: '=',
          orientation: '=',
          disabled: '=',
          fieldRequired: '=',
          fieldName: '='
        },
        link: function (scope, element, attr) {
          if (!angular.isDefined(scope.orientation)) {
            scope.orientation = '';
          }
          // get our own lookupFields
          scope.lookupValues = [];
          scope.lookupFieldsArray = nsLookupFieldService.LookupFieldsArray;
          scope.fieldResult = {};
          // determine 'which' fieldResult is our currentFieldResult
          // if field doesn't have a 'databasecolumn', then it is a label of some sort
          // just use a standard display template
          if (scope.currentField.DatabaseColumn != '' || scope.currentField.DatabaseColumn != null) {
            for (var j = 0; j < scope.studentResult.FieldResults.length; j++) {
              if (scope.studentResult.FieldResults[j].DbColumn === scope.currentField.DatabaseColumn) {
                scope.fieldResult = scope.studentResult.FieldResults[j];
                break;
              }
            }
          }
          for (var i = 0; i < scope.lookupFieldsArray.length; i++)
            if (scope.lookupFieldsArray[i].LookupColumnName === scope.currentField.LookupFieldName) {
              scope.lookupValues = scope.lookupFieldsArray[i].LookupFields;
              break;
            }
          var SumFunction = function (args) {
            var aryFields = args.split(',');
            var sum = 0;
            for (var i = 0; i < aryFields.length; i++) {
              var fieldDbColumn = aryFields[i];
              for (var j = 0; j < scope.studentResult.FieldResults.length; j++) {
                if (scope.studentResult.FieldResults[j].DbColumn == fieldDbColumn) {
                  sum += scope.studentResult.FieldResults[j].IntValue;
                }
              }
            }
            //scope.result.StringValue = sum;
            return sum;
          };
          var SumBoolFunction = function (args) {
            var aryFields = args.split(',');
            var sum = 0;
            for (var i = 0; i < aryFields.length; i++) {
              var fieldDbColumn = aryFields[i];
              for (var j = 0; j < scope.studentResult.FieldResults.length; j++) {
                if (scope.studentResult.FieldResults[j].DbColumn == fieldDbColumn) {
                  sum += scope.studentResult.FieldResults[j].BoolValue ? 1 : 0;
                }
              }
            }
            //scope.result.StringValue = sum;
            return sum;
          };
          var SumBoolByGroupFunction = function () {
            var sum = 0;
            for (var i = 0; i < scope.groups.length; i++) {
              var groupId = scope.groups[i].Id;
              // i don't like having this reference to the field... need to figure out
              // if it makes more sense to pass the additional data for each field
              // or to just join them on the client
              for (var j = 0; j < scope.studentResult.FieldResults.length; j++) {
                if (scope.studentResult.FieldResults[j].Field.GroupId === groupId && scope.studentResult.FieldResults[j].DbColumn.substring(0, 3) === 'chk') {
                  // only add each groupid once
                  if (scope.studentResult.FieldResults[j].BoolValue) {
                    sum++;
                    break;
                  }
                }
              }
            }
            //scope.result.StringValue = sum;
            return sum;
          };
          var ConcatenatedMissingLetters = function () {
            var unknownLetters = '';
            for (var i = 0; i < scope.groups.length; i++) {
              var groupId = scope.groups[i].Id;
              var foundInGroup = false;
              var groupDbCol = '';
              for (var j = 0; j < scope.studentResult.FieldResults.length; j++) {
                if (scope.studentResult.FieldResults[j].Field.GroupId === groupId && scope.studentResult.FieldResults[j].DbColumn.substring(0, 3) === 'chk') {
                  if (scope.studentResult.FieldResults[j].BoolValue) {
                    groupDbCol = scope.studentResult.FieldResults[j].Field.DatabaseColumn;
                    foundInGroup = true;
                    break;
                  }
                }
              }
              if (!foundInGroup) {
                // how to get the letter? find the field with the same groupid and print its DisplayLabel
                for (var r = 0; r < scope.fields.length; r++) {
                  var field = scope.fields[r];
                  if (field.GroupId === groupId && field.FieldType === 'Label') {
                    unknownLetters += field.DisplayLabel + ', ';
                  }
                }
              }
            }
            //remove trailing comma
            if (unknownLetters.length > 0) {
              unknownLetters = unknownLetters.substring(0, unknownLetters.length - 1);
            } else {
              unknownLetters = 'none';
            }
            return unknownLetters;
          };
          var BenchmarkFunction = function (args) {
            return 3;
            var FPValueId = 0;
            var Accuracy = 0;
            var CompScore = 0;
            // calculate compscore
            for (var j = 0; j < scope.allResults.length; j++) {
              if (scope.allResults[j].DbColumn === 'Beyond' || scope.allResults[j].DbColumn === 'About' || scope.allResults[j].DbColumn === 'Within' || scope.allResults[j].DbColumn === 'ExtraPt') {
                CompScore += scope.allResults[j].IntValue;
              }
            }
            // get accuracy
            for (var j = 0; j < scope.allResults.length; j++) {
              if (scope.allResults[j].DbColumn === 'Accuracy') {
                Accuracy = scope.allResults[j].IntValue;
                break;
              }
            }
            // FPScore
            for (var j = 0; j < scope.allResults.length; j++) {
              if (scope.allResults[j].DbColumn === 'FPValueID') {
                FPValueId = scope.allResults[j].IntValue;
                break;
              }
            }
            //scope.result.StringValue = 'test';
            if (Accuracy == null) {
              Accuracy = 0;
            }
            if (CompScore == null) {
              CompScore = 0;
            }
            if (FPValueId == null) {
              FPValueId = 0;
            }
            //return 'test';
            // get benchmarklevel
            $http.get('/api/assessment/getbenchmarklevel/' + FPValueId + '/' + Accuracy + '/' + CompScore).success(function (data) {
              return data;
            }).error(function (data, status, headers, config) {
              return 'error';
            });
          };
          function getCalculationFunction(field) {
            var calcFunctionName = field.CalculationFunction;
            var calcFunction;
            switch (calcFunctionName) {
            case 'Sum':
              calcFunction = SumFunction;
              break;
            case 'SumBool':
              calcFunction = SumBoolFunction;
              break;
            case 'SumBoolByGroup':
              calcFunction = SumBoolByGroupFunction;
              break;
            case 'ConcatenatedMissingLetters':
              calcFunction = ConcatenatedMissingLetters;
              break;
            case 'BenchmarkLevel':
              calcFunction = BenchmarkFunction;
              break;
            default:
              calcFunction = null;
              break;
            }
            return calcFunction;
          }
          var templateText = getTemplate(scope.currentField, scope.orientation);
          var calcFunction = getCalculationFunction(scope.currentField);
          var dataToAppend = templateText;
          element.html(dataToAppend);
          $compile(element.contents())(scope);
          scope.checkClick = function (boolValue, calcFields) {
            // most of the time, we will not do anything
            if (calcFields) {
              var aryFields = calcFields.split(',');
              var sum = 0;
              for (var i = 0; i < aryFields.length; i++) {
                var fieldDbColumn = aryFields[i];
                for (var j = 0; j < scope.studentResult.FieldResults.length; j++) {
                  if (scope.studentResult.FieldResults[j].DbColumn == fieldDbColumn) {
                    scope.studentResult.FieldResults[j].BoolValue = boolValue;
                  }
                }
              }
            }
          };
          scope.Calculate = function (args) {
            return calcFunction(args);
          };
        }
      };
    }
  ]).directive('assessmentHfwEditFieldCustom', [
    'Assessment',
    '$routeParams',
    '$compile',
    '$templateCache',
    '$http',
    function (Assessment, $routeParams, $compile, $templateCache, $http) {
      var getTemplate = function (field) {
        var type = field.FieldType;
        var type = field.FieldType;
        var template = '';
        if (type === 'DateCheckbox') {
          template = $templateCache.get('templates/assessment-datecheckbox.html');
        } else if (type == 'CalculatedFieldDbBacked') {
          template = $templateCache.get('templates/assessment-calculatedfielddbbacked-custom.html');
        } else if (type == 'Textarea') {
          template = $templateCache.get('templates/assessment-textarea-custom.html');
        }
        return template;
      };
      return {
        restrict: 'E',
        template: '<div>{{result}}</div>',
        scope: {
          currentField: '=',
          studentResult: '=',
          lookupFieldsArray: '=',
          groups: '=',
          fields: '=',
          defaultDate: '=',
          fieldMode: '='
        },
        link: function (scope, element, attr) {
          // get our own lookupFields
          scope.lookupValues = [];
          scope.fieldResult = {};
          scope.formattedDate = function (date) {
            if (date != null) {
              return moment(date).format('DD-MMM-YYYY');
            }
            return '';
          };
          scope.checkClick = function ($event) {
            //   $event.preventDefault();
            //   $event.stopPropagation();
            //     $event.stopImmediatePropagation();
            if (scope.fieldResult.BoolValue) {
              if (moment(scope.fieldResult.DateValue).toString() == moment(scope.defaultDate).toString()) {
                scope.fieldResult.DateValue = null;
                //TODO: Get value from date box
                scope.fieldResult.BoolValue = false;
              } else {
                if (confirm('Are you sure you want to change this date?')) {
                  scope.fieldResult.DateValue = null;
                  //TODO: Get value from date box
                  scope.fieldResult.BoolValue = false;
                }
              }
            } else {
              scope.fieldResult.DateValue = angular.isDefined(scope.defaultDate) ? scope.defaultDate : new Date();
              //TODO: Get value from date box
              scope.fieldResult.BoolValue = true;
            }
            scope.fieldResult.IsModified = true;
          };
          // determine 'which' fieldResult is our currentFieldResult
          // if field doesn't have a 'databasecolumn', then it is a label of some sort
          // just use a standard display template
          if (scope.currentField.DatabaseColumn != '' || scope.currentField.DatabaseColumn != null) {
            // this would be one of the total or the comment columns
            if (scope.currentField.CategoryId === null) {
              for (var j = 0; j < scope.studentResult.TotalFieldResults.length; j++) {
                if (scope.studentResult.TotalFieldResults[j].DbColumn === scope.currentField.DatabaseColumn) {
                  scope.fieldResult = scope.studentResult.TotalFieldResults[j];
                  break;
                }
              }
            } else if (scope.currentField.Category.DisplayName === 'Read') {
              for (var j = 0; j < scope.studentResult.ReadFieldResults.length; j++) {
                if (scope.studentResult.ReadFieldResults[j].DbColumn === scope.currentField.DatabaseColumn) {
                  scope.fieldResult = scope.studentResult.ReadFieldResults[j];
                  break;
                }
              }
            } else if (scope.currentField.Category.DisplayName == 'Write') {
              for (var j = 0; j < scope.studentResult.WriteFieldResults.length; j++) {
                if (scope.studentResult.WriteFieldResults[j].DbColumn === scope.currentField.DatabaseColumn) {
                  scope.fieldResult = scope.studentResult.WriteFieldResults[j];
                  break;
                }
              }
            }
          }
          var SumBoolByCategory = function () {
            var sum = 0;
            if (scope.currentField.CalculationFields == 'read') {
              for (var j = 0; j < scope.studentResult.ReadFieldResults.length; j++) {
                if (scope.studentResult.ReadFieldResults[j].BoolValue) {
                  sum++;
                }
              }
            } else if (scope.currentField.CalculationFields == 'write') {
              for (var j = 0; j < scope.studentResult.WriteFieldResults.length; j++) {
                if (scope.studentResult.WriteFieldResults[j].BoolValue) {
                  sum++;
                }
              }
            } else {
              for (var j = 0; j < scope.studentResult.WriteFieldResults.length; j++) {
                if (scope.studentResult.WriteFieldResults[j].BoolValue && scope.studentResult.ReadFieldResults[j].BoolValue) {
                  sum++;
                }
              }
            }
            return sum;
          };
          function getCalculationFunction(field) {
            var calcFunctionName = field.CalculationFunction;
            var calcFunction;
            switch (calcFunctionName) {
            case 'SumBoolByCategory':
              calcFunction = SumBoolByCategory;
              break;
            default:
              calcFunction = null;
              break;
            }
            return calcFunction;
          }
          var templateText = getTemplate(scope.currentField);
          //var calcFunction = getCalculationFunction(scope.currentField);
          var dataToAppend = templateText;
          element.html(dataToAppend);
          $compile(element.contents())(scope);
          scope.checkClick2 = function (boolValue, calcFields) {
            // most of the time, we will not do anything
            if (calcFields) {
              var aryFields = calcFields.split(',');
              var sum = 0;
              for (var i = 0; i < aryFields.length; i++) {
                var fieldDbColumn = aryFields[i];
                for (var j = 0; j < scope.studentResult.FieldResults.length; j++) {
                  if (scope.studentResult.FieldResults[j].DbColumn == fieldDbColumn) {
                    scope.studentResult.FieldResults[j].BoolValue = boolValue;
                  }
                }
              }
            }
          };
          var calcFunction = getCalculationFunction(scope.currentField);
          scope.Calculate = function (args) {
            return calcFunction(args);
          };
        }
      };
    }
  ]).service('nsLookupFieldService', [
    '$http',
    '$routeParams',
    'webApiBaseUrl',
    function ($http, $routeParams, webApiBaseUrl) {
      var self = this;
      self.LookupFieldsArray = [];
      self.LoadLookupFields = function () {
        // only need to do this once
        if (self.LookupFieldsArray.length > 0) {
          return;
        }
        return $http.get(webApiBaseUrl + '/api/assessment/GetAllLookupFields').then(function (response) {
          self.LookupFieldsArray = response.data;
          ;
        });
      };
    }
  ]).factory('NSSectionAssessmentDataEntryManager', [
    '$http',
    'webApiBaseUrl',
    'nsLookupFieldService',
    function ($http, webApiBaseUrl, nsLookupFieldService) {
      var self = this;
      var NSSectionAssessmentDataEntryManager = function (lookupFieldsArray) {
        self.LookupFieldsArray = lookupFieldsArray;
        this.initialize = function () {
        };
        this.copyStudentAssessmentData = function (assessmentId, selectedBenchmarkDate, targetBenchmarkDate, section, studentId) {
          var returnObject = {
              StudentId: studentId,
              SelectedBenchmarkDate: selectedBenchmarkDate,
              TargetBenchmarkDate: targetBenchmarkDate,
              AssessmentId: assessmentId,
              Section: section
            };
          return $http.post(webApiBaseUrl + '/api/dataentry/CopyStudentAssessmentData', returnObject);
        };
        this.copySectionAssessmentData = function (assessmentId, selectedBenchmarkDate, targetBenchmarkDate, section, studentResults) {
          var returnObject = {
              SelectedBenchmarkDate: selectedBenchmarkDate,
              TargetBenchmarkDate: targetBenchmarkDate,
              AssessmentId: assessmentId,
              Section: section
            };
          return $http.post(webApiBaseUrl + '/api/dataentry/CopySectionAssessmentData', returnObject);
        };
        this.loadAssessmentResultData = function (assessmentId, nsFilterOptionsService) {
          var postObject = {
              AssessmentId: assessmentId,
              SectionId: nsFilterOptionsService.selectedSection.id,
              BenchmarkDateId: nsFilterOptionsService.selectedBenchmarkDate.id
            };
          var url = webApiBaseUrl + '/api/dataentry/GetAssessmentResults';
          return $http.post(url, postObject);
        };
        this.makeDatesPopupCompatible = function (studentResultsArray) {
          for (var j = 0; j < studentResultsArray.length; j++) {
            var result = studentResultsArray[j];
            if (result.TestDate == null) {
              result.TestDate = moment().format('DD-MMM-YYYY');
            } else {
              var momentizedDate = moment(result.TestDate);
              result.TestDate = momentizedDate.format('DD-MMM-YYYY');
            }
          }
        };
        this.attachFieldsToResults = function (studentResultsArray, fieldsArray) {
          console.time('Start attach fields');
          for (var j = 0; j < studentResultsArray.length; j++) {
            for (var k = 0; k < studentResultsArray[j].FieldResults.length; k++) {
              for (var r = 0; r < fieldsArray.length; r++) {
                if (fieldsArray[r].DatabaseColumn == studentResultsArray[j].FieldResults[k].DbColumn) {
                  studentResultsArray[j].FieldResults[k].Field = angular.copy(fieldsArray[r]);
                  // set display value
                  if (fieldsArray[r].FieldType === 'DropdownFromDB') {
                    for (var p = 0; p < self.LookupFieldsArray.length; p++) {
                      if (self.LookupFieldsArray[p].LookupColumnName === fieldsArray[r].LookupFieldName) {
                        // now find the specifc value that matches
                        for (var y = 0; y < self.LookupFieldsArray[p].LookupFields.length; y++) {
                          if (studentResultsArray[j].FieldResults[k].IntValue === self.LookupFieldsArray[p].LookupFields[y].FieldSpecificId) {
                            studentResultsArray[j].FieldResults[k].DisplayValue = self.LookupFieldsArray[p].LookupFields[y].FieldValue;
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
          }
          console.timeEnd('Start attach fields');  // set initial display values
        };
        this.initializeHeaderClassArray = function (fields, headerClassArray) {
          for (var r = 0; r < fields.length; r++) {
            headerClassArray[r] = 'fa';
          }
        };
        this.doSort = function (column, staticColumnsObj, fields, headerClassArray, sortArray) {
          var columnIndex = -1;
          // if this is not a first or lastname column
          if (!isNaN(parseInt(column))) {
            columnIndex = column;
            switch (fields[column].FieldType) {
            case 'DateCheckbox':
              column = 'FieldResults[' + column + '].DateValue';
              break;
            case 'Textfield':
              column = 'FieldResults[' + column + '].StringValue';
              break;
            case 'DecimalRange':
              column = 'FieldResults[' + column + '].DecimalValue';
              break;
            case 'DropdownRange':
              column = 'FieldResults[' + column + '].IntValue';
              break;
            case 'DropdownFromDB':
              column = 'FieldResults[' + column + '].IntValue';
              break;
            case 'CalculatedFieldDbOnly':
              column = 'FieldResults[' + column + '].StringValue';
              break;
            case 'CalculatedFieldDbBacked':
              column = 'FieldResults[' + column + '].IntValue';
              break;
            case 'CalculatedFieldDbBackedString':
              column = 'FieldResults[' + column + '].StringValue';
              break;
            case 'CalculatedFieldClientOnly':
              column = 'FieldResults[' + column + '].StringValue';
              //shouldnt even be used in sorting
              break;
            default:
              column = 'FieldResults[' + column + '].IntValue';
              break;
            }
          }
          var bFound = false;
          for (var j = 0; j < sortArray.length; j++) {
            // if it is already on the list, reverse the sort
            if (sortArray[j].indexOf(column) >= 0) {
              bFound = true;
              // is it already negative? if so, remove it
              if (sortArray[j].indexOf('-') === 0) {
                if (columnIndex > -1) {
                  headerClassArray[columnIndex] = 'fa';
                } else if (column === 'StudentName') {
                  staticColumnsObj.studentNameHeaderClass = 'fa';
                }
                sortArray.splice(j, 1);
              } else {
                if (columnIndex > -1) {
                  headerClassArray[columnIndex] = 'fa fa-chevron-down';
                } else if (column === 'StudentName') {
                  staticColumnsObj.studentNameHeaderClass = 'fa fa-chevron-down';
                }
                sortArray[j] = '-' + sortArray[j];
              }
              break;
            }
          }
          if (!bFound) {
            sortArray.push(column);
            if (columnIndex > -1) {
              headerClassArray[columnIndex] = 'fa fa-chevron-up';
            } else if (column === 'StudentName') {
              staticColumnsObj.studentNameHeaderClass = 'fa fa-chevron-up';
            }
          }
        };
        this.saveAssessmentResult = function (assessmentId, studentResult, benchmarkDateId) {
          var returnObject = {
              StudentResult: studentResult,
              AssessmentId: assessmentId,
              BenchmarkDateId: benchmarkDateId
            };
          return $http.post(webApiBaseUrl + '/api/dataentry/SaveAssessmentResult', returnObject);
        };
        this.deleteStudentTestResult = function (assessmentId, studentResult) {
          var returnObject = {
              StudentResult: studentResult,
              AssessmentId: assessmentId
            };
          return $http.post(webApiBaseUrl + '/api/dataentry/DeleteAssessmentResult', returnObject);
        };
        this.cleanupAfterDelete = function () {
          // TODO: update local model, delete all field values
          // loop over each field and delete the values... also delete anything else relevant
          for (var k = 0; k < studentResult.FieldResults.length; k++) {
            studentResult.FieldResults[k].IntValue = null;
            studentResult.FieldResults[k].DecimalValue = null;
            studentResult.FieldResults[k].DateValue = null;
            studentResult.FieldResults[k].StringValue = null;
            studentResult.FieldResults[k].DisplayValue = null;
            studentResult.FieldResults[k].BoolValue = null;
          }
        };
        this.initialize();
      };
      return NSSectionAssessmentDataEntryManager;
    }
  ]);
}());
(function () {
  'use strict';
  angular.module('nsOAuth2', []).factory('authInterceptorService', [
    '$rootScope',
    '$cookies',
    '$q',
    '$injector',
    '$location',
    'localStorageService',
    function ($rootScope, $cookies, $q, $injector, $location, localStorageService) {
      var authInterceptorServiceFactory = {};
      var expired = function (token) {
        return token && token.expires_at && new Date(token.expires_at) < new Date();
      };
      var _request = function (config) {
        config.headers = config.headers || {};
        var cookie = $cookies.get('Authorization');
        // TODO: get rid of this... SH on 1/7
        if (angular.isDefined(cookie)) {
          $rootScope.cookieDefined = 'yes';
        } else {
          $rootScope.cookieDefined = 'no';
        }
        // for print batching, see if token is passed in
        if (angular.isDefined(cookie) && cookie.indexOf('Bearer ') == 0) {
          var access_token = cookie.split('Bearer ')[1];
          //setExpiresAt()
          localStorageService.set('authorizationData', {
            token: access_token,
            expires_at: new Date().setYear(2100),
            userName: 'printer',
            refreshToken: '',
            useRefreshTokens: false
          });
        }
        var authData = localStorageService.get('authorizationData');
        if (authData) {
          // edge case fix when expiration date isn't set
          if (!angular.isDefined(authData.expires_at)) {
            localStorageService.remove('authorizationData');
            $location.path('/login');
          }
          // if token is expired, don't try to use it... send you to the login screen
          if (expired(authData) && $location.path() !== '/login')
            // no infinite loops
            {
              $location.path('/login');
            }
          // don't try to use this on the login page
          if ($location.path() !== '/login') {
            config.headers.Authorization = 'Bearer ' + authData.token;
          }
        } else {
          // if no login token yet, go get one
          // these are the only pages we can get to w/o a token
          var resetLinkPath = '/reset-password-from-link';
          if ($location.path() !== '/login' && $location.path() !== '/request-password-reset' && $location.path().slice(0, resetLinkPath.length) !== resetLinkPath) {
            $location.path('/login');
          }
        }
        return config;
      };
      var _responseError = function (rejection) {
        if (rejection.status === 401) {
          var authService = $injector.get('authService');
          var authData = localStorageService.get('authorizationData');
          if (authData) {
          }
          // if we are logged in with a good token and get a 401, this came from the application
          if (!expired(authData)) {
            $location.path('/401');
          } else {
            authService.logOut();
            $location.path('/login');
          }
        } else if (rejection.status === 500 || rejection.status === 0 && (rejection.data === '' || rejection.data == null)) {
          var authData = localStorageService.get('authorizationData');
          // if we are logged in with a good token and get a 401, this came from the application
          if (!expired(authData)) {
            $location.path('/extras-500');
          } else {
            authService.logOut();
            $location.path('/login');
          }
        } else if (rejection.status === 400) {
          // user displayble error, broadcast it
          $rootScope.$broadcast('NSHTTPError', rejection.data.Message);
        }
        return $q.reject(rejection);
      };
      authInterceptorServiceFactory.request = _request;
      authInterceptorServiceFactory.responseError = _responseError;
      return authInterceptorServiceFactory;
    }
  ]).factory('authService', [
    '$http',
    '$q',
    'localStorageService',
    'ngAuthSettings',
    '$location',
    '$global',
    function ($http, $q, localStorageService, ngAuthSettings, $location, $global) {
      var serviceBase = ngAuthSettings.apiServiceBaseUri;
      var authServiceFactory = {};
      var _authentication = {
          isAuth: false,
          userName: '',
          useRefreshTokens: false
        };
      function setExpiresAt(token) {
        if (token) {
          var expires_at = new Date();
          expires_at.setSeconds(expires_at.getSeconds() + parseInt(token.expires_in) - 60);
          // 60 seconds less to secure browser and response latency
          token.expires_at = expires_at;
        }
      }
      var _externalAuthData = {
          provider: '',
          userName: '',
          externalAccessToken: ''
        };
      var _token = function () {
        var authData = localStorageService.get('authorizationData');
        if (authData) {
          return authData.token;
        }
        return null;
      };
      var _saveRegistration = function (registration) {
        _logOut();
        return $http.post(serviceBase + 'api/account/register', registration).then(function (response) {
          return response;
        });
      };
      var _login = function (loginData) {
        var data = 'scope=idmgr&grant_type=password&username=' + loginData.userName + '&password=' + loginData.password + '&client_id=' + ngAuthSettings.clientId + '&client_secret=secret&acr_values=' + loginData.alternate;
        //if (loginData.useRefreshTokens) {
        //    data = data + "&client_id=" + ngAuthSettings.clientId;
        //}
        var deferred = $q.defer();
        $http.post(serviceBase + 'token', data, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }).success(function (response) {
          setExpiresAt(response);
          if (loginData.useRefreshTokens) {
            localStorageService.set('authorizationData', {
              token: response.access_token,
              userName: loginData.userName,
              refreshToken: response.refresh_token,
              useRefreshTokens: true
            });
          } else {
            localStorageService.set('authorizationData', {
              token: response.access_token,
              expires_at: response.expires_at,
              userName: loginData.userName,
              refreshToken: '',
              useRefreshTokens: false
            });
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
        _authentication.userName = '';
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
            var data = 'grant_type=refresh_token&refresh_token=' + authData.refreshToken + '&client_id=' + ngAuthSettings.clientId;
            localStorageService.remove('authorizationData');
            $http.post(serviceBase + 'token', data, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }).success(function (response) {
              localStorageService.set('authorizationData', {
                token: response.access_token,
                userName: response.userName,
                refreshToken: response.refresh_token,
                useRefreshTokens: true
              });
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
        $http.get(serviceBase + 'api/account/ObtainLocalAccessToken', {
          params: {
            provider: externalData.provider,
            externalAccessToken: externalData.externalAccessToken
          }
        }).success(function (response) {
          localStorageService.set('authorizationData', {
            token: response.access_token,
            userName: response.userName,
            refreshToken: '',
            useRefreshTokens: false
          });
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
          localStorageService.set('authorizationData', {
            token: response.access_token,
            userName: response.userName,
            refreshToken: '',
            useRefreshTokens: false
          });
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
    }
  ]).factory('tokensManagerService', [
    '$http',
    'ngAuthSettings',
    function ($http, ngAuthSettings) {
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
    }
  ]);
}());
(function () {
  'use strict';
  angular.module('theme.template-overrides', []).config([
    '$provide',
    function ($provide) {
      $provide.decorator('uibTabsetDirective', function ($delegate) {
        $delegate[0].templateUrl = function (element, attr) {
          if (attr.tabPosition || attr.tabTheme) {
            if (attr.tabPosition && (attr.tabPosition == '\'bottom\'' || attr.tabPosition == 'bottom'))
              return 'templates/themed-tabs-bottom.html';
            return 'templates/themed-tabs.html';
          } else if (attr.panelTabs && attr.uibTabheading !== undefined) {
            return 'templates/panel-tabs.html';
          } else if (attr.panelTabs && attr.uibTabheading == undefined) {
            return 'templates/panel-tabs-without-heading.html';
          } else {
            return 'templates/themed-tabs.html';
          }
        };
        var directive = $delegate[0];
        directive.$$isolateBindings.heading = {
          attrName: 'heading',
          mode: '@',
          optional: true
        };
        directive.$$isolateBindings.panelClass = {
          attrName: 'panelClass',
          mode: '@',
          optional: true
        };
        directive.$$isolateBindings.panelIcon = {
          attrName: 'panelIcon',
          mode: '@',
          optional: true
        };
        directive.$$isolateBindings.theme = {
          attrName: 'tabTheme',
          mode: '@',
          optional: true
        };
        directive.$$isolateBindings.position = {
          attrName: 'tabPosition',
          mode: '@',
          optional: true
        };
        angular.extend($delegate[0].scope, {
          heading: '@',
          panelClass: '@',
          panelIcon: '@',
          theme: '@tabTheme',
          position: '@tabPosition'
        });
        return $delegate;
      });
      $provide.decorator('uibProgressbarDirective', function ($delegate) {
        $delegate[0].templateUrl = function (element, attr) {
          if (attr.contextual && attr.contextual == 'true') {
            return 'templates/contextual-progressbar.html';
          }
          return 'template/progressbar/progressbar.html';
        };
        angular.extend($delegate[0].scope, { heading: '@' });
        return $delegate;
      });
    }
  ]).run([
    '$templateCache',
    function ($templateCache) {
      $templateCache.put('footerTemplate.html', '<div ng-show="showFooter" class="ng-grid-footer" ng-style="footerStyle()">\r' + '\n' + '    <div class="col-md-4" >\r' + '\n' + '        <div class="ngFooterTotalItems" ng-class="{\'ngNoMultiSelect\': !multiSelect}" >\r' + '\n' + '            <span class="ngLabel">{{i18n.ngTotalItemsLabel}} {{maxRows()}}</span><span ng-show="filterText.length > 0" class="ngLabel">({{i18n.ngShowingItemsLabel}} {{totalFilteredItemsLength()}})</span>\r' + '\n' + '        </div>\r' + '\n' + '        <div class="ngFooterSelectedItems" ng-show="multiSelect">\r' + '\n' + '            <span class="ngLabel">{{i18n.ngSelectedItemsLabel}} {{selectedItems.length}}</span>\r' + '\n' + '        </div>\r' + '\n' + '    </div>\r' + '\n' + '    <div class="col-md-4" ng-show="enablePaging" ng-class="{\'ngNoMultiSelect\': !multiSelect}">\r' + '\n' + '            <label class="control-label ng-grid-pages center-block">{{i18n.ngPageSizeLabel}}\r' + '\n' + '               <select class="form-control input-sm" ng-model="pagingOptions.pageSize" >\r' + '\n' + '                      <option ng-repeat="size in pagingOptions.pageSizes">{{size}}</option>\r' + '\n' + '                </select>\r' + '\n' + '        </label>\r' + '\n' + '</div>\r' + '\n' + '     <div class="col-md-4">\r' + '\n' + '        <div class="pull-right ng-grid-pagination">\r' + '\n' + '            <button type="button" class="btn btn-default btn-sm" ng-click="pageToFirst()" ng-disabled="cantPageBackward()" title="{{i18n.ngPagerFirstTitle}}"><i class="fa fa-angle-double-left"></i></button>\r' + '\n' + '            <button type="button" class="btn btn-default btn-sm" ng-click="pageBackward()" ng-disabled="cantPageBackward()" title="{{i18n.ngPagerPrevTitle}}"><i class="fa fa-angle-left"></i></button>\r' + '\n' + '            <label class="control-label">\r' + '\n' + '                   <input class="form-control input-sm" min="1" max="{{currentMaxPages}}" type="number" style="width:50px; height: 24px; margin-top: 1px; padding: 0 4px;" ng-model="pagingOptions.currentPage"/>\r' + '\n' + '            </label>\r' + '\n' + '            <span class="ngGridMaxPagesNumber" ng-show="maxPages() > 0">/ {{maxPages()}}</span>\r' + '\n' + '            <button type="button" class="btn btn-default btn-sm" ng-click="pageForward()" ng-disabled="cantPageForward()" title="{{i18n.ngPagerNextTitle}}"><i class="fa fa-angle-right"></i></button>\r' + '\n' + '            <button type="button" class="btn btn-default btn-sm" ng-click="pageToLast()" ng-disabled="cantPageToLast()" title="{{i18n.ngPagerLastTitle}}"><i class="fa fa-angle-double-right"></i></button>\r' + '\n' + '        </div>\r' + '\n' + '     </div>\r' + '\n' + '</div>\r' + '\n');
      $templateCache.put('template/rating/rating.html', '<span ng-mouseleave="reset()" ng-keydown="onKeydown($event)" tabindex="0" role="slider" aria-valuemin="0" aria-valuemax="{{range.length}}" aria-valuenow="{{value}}">\n' + '    <i ng-repeat="r in range track by $index" ng-mouseenter="enter($index + 1)" ng-click="rate($index + 1)" class="fa" ng-class="$index < value && (r.stateOn || \'fa-star\') || (r.stateOff || \'fa-star-o\')">\n' + '        <span class="sr-only">({{ $index < value ? \'*\' : \' \' }})</span>\n' + '    </i>\n' + '</span>');
    }
  ]);
}());
(function () {
  'use strict';
  angular.module('theme.navigation-controller', []).controller('NavigationController', [
    '$scope',
    '$http',
    '$location',
    '$timeout',
    '$global',
    'webApiBaseUrl',
    'authService',
    function ($scope, $http, $location, $timeout, $global, webApiBaseUrl, authService) {
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
        $http.get(webApiBaseUrl + '/api/Navigation/NavigationNodes').then(function (response) {
          //fore
          $scope.menu.items = response.data.Nodes;
          var item = $scope.findItemByUrl($scope.menu.items, $location.path());
          if (item)
            $timeout(function () {
              $scope.select(item);
            });
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
          if (children[i].url && children[i].url.replace('#', '') == url)
            return children[i];
          if (children[i].children !== undefined && children[i].children !== null) {
            var item = $scope.findItemByUrl(children[i].children, url);
            if (item)
              return item;
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
        }
        ;
        $scope.openItems = [];
        var parentRef = item;
        while (parentRef !== null && parentRef !== undefined) {
          parentRef.open = true;
          $scope.openItems.push(parentRef);
          parentRef = parentRef.parent;
        }
        // handle leaf nodes
        if (!item.children || item.children && item.children.length < 1) {
          $scope.selectedFromNavMenu = true;
          for (var j = $scope.selectedItems.length - 1; j >= 0; j--) {
            $scope.selectedItems[j].selected = false;
          }
          ;
          $scope.selectedItems = [];
          var parentRef = item;
          while (parentRef !== null && parentRef !== undefined) {
            parentRef.selected = true;
            $scope.selectedItems.push(parentRef);
            parentRef = parentRef.parent;
          }
        }
        ;
      };
      $scope.$watch(function () {
        return $location.path();
      }, function (newVal, oldVal) {
        if ($scope.selectedFromNavMenu == false) {
          var item = $scope.findItemByUrl($scope.menu.items, newVal);
          if (item)
            $timeout(function () {
              $scope.select(item);
            });
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
            $timeout(function () {
              $scope.select(item);
            });
        }
        $scope.selectedFromNavMenu = false;
      });
      // searchbar
      $scope.showSearchBar = function ($e) {
        $e.stopPropagation();
        $global.set('showSearchCollapsed', true);
      };
      $scope.$on('globalStyles:changed:showSearchCollapsed', function (event, newVal) {
        $scope.style_showSearchCollapsed = newVal;
      });
      $scope.goToSearch = function () {
        $location.path('/extras-search');
      };
    }
  ]);
}());
(function () {
  'use strict';
  angular.module('staffModule', []).factory('NSStaff', [
    '$http',
    'webApiBaseUrl',
    'nsStaffService',
    function ($http, webApiBaseUrl, nsStaffService) {
      var NSStaff = function (staffId) {
        var self = this;
        self.initialize = function () {
          self.SpecialEdLabels = [];
          self.StudentSchools = [];
          self.StudentAttributes = {};
          nsStaffService.getStaffById(staffId).then(function (response) {
            angular.extend(self, response.data);
            if (self.RoleID != 0) {
              self.role = self.RoleID + '';
            } else {
              self.role = null;
            }
          });
        };
        self.initialize();
      };
      return NSStaff;
    }
  ]).service('nsStaffService', [
    '$http',
    'pinesNotifications',
    'webApiBaseUrl',
    function ($http, pinesNotifications, webApiBaseUrl) {
      //this.options = {};
      var self = this;
      self.requestPasswordReset = function (email) {
        return $http.post(webApiBaseUrl + '/api/passwordreset/requestpasswordreset', { UserName: email });
      };
      self.validateUid = function (uid) {
        return $http.post(webApiBaseUrl + '/api/passwordreset/ValidateUID', { UID: uid });
      };
      self.deleteStaff = function (id) {
        return $http.post(webApiBaseUrl + '/api/staff/deletestaff', { Id: id });
      };
      self.resetPasswordFromEmail = function (uid, pwd) {
        return $http.post(webApiBaseUrl + '/api/passwordreset/ResetPasswordFromEmail', {
          UID: uid,
          Password: pwd
        });
      };
      this.getStaffBySchool = function (schoolId) {
        return $http.get(webApiBaseUrl + '/api/staff/getstaffbyschool/' + schoolId);
      };
      this.getStaffById = function (staffId) {
        return $http.get(webApiBaseUrl + '/api/staff/getstaffbyid/' + staffId);
      };
      this.saveStaff = function (staff) {
        // hack fix
        staff.RoleID = staff.role;
        return $http.post(webApiBaseUrl + '/api/staff/savestaff', staff);
      };
      this.doSort = function (column, staticColumnsObj, fields, headerClassArray, sortArray) {
        var columnIndex = -1;
        // if this is not a first or lastname column
        if (!isNaN(parseInt(column))) {
          columnIndex = column;
          switch (fields[column].FieldType) {
          case 'DateCheckbox':
            column = 'FieldResults[' + column + '].DateValue';
            break;
          case 'Textfield':
            column = 'FieldResults[' + column + '].StringValue';
            break;
          case 'DecimalRange':
            column = 'FieldResults[' + column + '].DecimalValue';
            break;
          case 'DropdownRange':
            column = 'FieldResults[' + column + '].IntValue';
            break;
          case 'DropdownFromDB':
            column = 'FieldResults[' + column + '].IntValue';
            break;
          case 'CalculatedFieldDbOnly':
            column = 'FieldResults[' + column + '].StringValue';
            break;
          case 'CalculatedFieldDbBacked':
            column = 'FieldResults[' + column + '].IntValue';
            break;
          case 'CalculatedFieldDbBackedString':
            column = 'FieldResults[' + column + '].StringValue';
            break;
          case 'CalculatedFieldClientOnly':
            column = 'FieldResults[' + column + '].StringValue';
            //shouldnt even be used in sorting
            break;
          default:
            column = 'FieldResults[' + column + '].IntValue';
            break;
          }
        }
        var bFound = false;
        for (var j = 0; j < sortArray.length; j++) {
          // if it is already on the list, reverse the sort
          if (sortArray[j].indexOf(column) >= 0) {
            bFound = true;
            // is it already negative? if so, remove it
            if (sortArray[j].indexOf('-') === 0) {
              if (columnIndex > -1) {
                headerClassArray[columnIndex] = 'fa';
              } else if (column === 'FirstName') {
                staticColumnsObj.firstNameHeaderClass = 'fa';
              } else if (column === 'LastName') {
                staticColumnsObj.lastNameHeaderClass = 'fa';
              }
              sortArray.splice(j, 1);
            } else {
              if (columnIndex > -1) {
                headerClassArray[columnIndex] = 'fa fa-chevron-down';
              } else if (column === 'FirstName') {
                staticColumnsObj.firstNameHeaderClass = 'fa fa-chevron-down';
              } else if (column === 'LastName') {
                staticColumnsObj.lastNameHeaderClass = 'fa fa-chevron-down';
              }
              sortArray[j] = '-' + sortArray[j];
            }
            break;
          }
        }
        if (!bFound) {
          sortArray.push(column);
          if (columnIndex > -1) {
            headerClassArray[columnIndex] = 'fa fa-chevron-up';
          } else if (column === 'FirstName') {
            staticColumnsObj.firstNameHeaderClass = 'fa fa-chevron-up';
          } else if (column === 'LastName') {
            staticColumnsObj.lastNameHeaderClass = 'fa fa-chevron-up';
          }
        }
      };
      this.deleteStudentTestResult = function (assessmentId, studentResult) {
        var returnObject = {
            StudentResult: studentResult,
            AssessmentId: assessmentId
          };
        // TODO: update local model, delete all field values
        // loop over each field and delete the values... also delete anything else relevant
        for (var k = 0; k < studentResult.FieldResults.length; k++) {
          studentResult.FieldResults[k].IntValue = null;
          studentResult.FieldResults[k].DecimalValue = null;
          studentResult.FieldResults[k].DateValue = null;
          studentResult.FieldResults[k].StringValue = null;
          studentResult.FieldResults[k].DisplayValue = null;
          studentResult.FieldResults[k].BoolValue = null;
        }
        $http.post(webApiBaseUrl + '/api/assessment/DeleteAssessmentResult', returnObject).success(function (data) {
          dataDeletedSuccessfully();
        }).error(function (data, status, headers, config) {
          alert('error deleting');
        });
      };
      var dataSavedSuccessfully = function () {
        pinesNotifications.notify({
          title: 'Data Saved',
          text: 'Your data was saved successfully.',
          type: 'success'
        });
      };
      var dataDeletedSuccessfully = function () {
        pinesNotifications.notify({
          title: 'Data Deleted',
          text: 'Your data was deleted successfully.',
          type: 'success'
        });
      };
    }
  ]).controller('StaffListController', StaffListController).controller('StaffEditController', StaffEditController).controller('StaffPasswordResetController', StaffPasswordResetController).controller('StaffPasswordResetRequestController', [
    '$http',
    '$scope',
    '$global',
    'nsStaffService',
    function ($http, $scope, $global, nsStaffService) {
      var self = this;
      $global.set('fullscreen', true);
      $scope.errors = [];
      $scope.$on('NSHTTPError', function (event, data) {
        $scope.errors.push({
          type: 'danger',
          msg: data
        });
        $('html, body').animate({ scrollTop: 0 }, 'fast');
      });
      $scope.requestReset = function () {
        nsStaffService.requestPasswordReset($scope.userInfo.email).then(function () {
          $scope.requestSent = true;  // show the thanks message
        });
      };
      $scope.userInfo = {};
    }
  ]).controller('StaffPasswordResetFromLinkController', [
    '$http',
    '$scope',
    '$global',
    'nsStaffService',
    '$routeParams',
    function ($http, $scope, $global, nsStaffService, $routeParams) {
      var self = this;
      $global.set('fullscreen', true);
      $scope.errors = [];
      // validate UID
      function validateUID() {
        nsStaffService.validateUid($routeParams.uid).then(function (response) {
          // we don't need to check any return value, if we got here w/o getting a 400 or 500, which would have been caught elsewhere, we are good
          $scope.validated = true;
        });
      }
      $scope.$on('NSHTTPError', function (event, data) {
        $scope.errors.push({
          type: 'danger',
          msg: data
        });
        $('html, body').animate({ scrollTop: 0 }, 'fast');
      });
      $scope.resetPwd = function () {
        nsStaffService.resetPasswordFromEmail($routeParams.uid, $scope.userInfo.pwd).then(function () {
          $scope.success = true;  // show the thanks message
        });
      };
      $scope.userInfo = {};
      validateUID();
    }
  ]).controller('StaffInfoController', [
    '$http',
    '$location',
    '$q',
    '$routeParams',
    'webApiBaseUrl',
    'authService',
    '$global',
    '$scope',
    'NSUserInfoService',
    function ($http, $location, $q, $routeParams, webApiBaseUrl, authService, $global, $scope, NSUserInfoService) {
      var uvm = this;
      uvm.userInfo = NSUserInfoService;
      if (authService.authentication.isAuth) {
        uvm.userInfo.loadUserInfo();
      }
      // if refresh needed
      $scope.$watch(function () {
        return $global.get('userprofileupdated');
      }, function (newVal, oldVal) {
        if (newVal !== oldVal && newVal === true) {
          $global.set('userprofileupdated', false);
          uvm.userInfo.loadUserInfo();
        }
      });
      // if login status changes, refresh
      $scope.$watch(function () {
        return authService.authentication.isAuth;
      }, function (newVal, oldVal) {
        if (newVal !== oldVal && newVal === true) {
          uvm.userInfo.loadUserInfo();
        }
      });
    }
  ]);
  /* Movies List Controller  */
  StaffListController.$inject = [
    '$scope',
    'nsStaffService',
    'nsFilterOptionsService',
    'nsSelect2RemoteOptions',
    '$location',
    '$bootbox',
    'nsPinesService',
    'spinnerService',
    '$timeout'
  ];
  function StaffListController($scope, nsStaffService, nsFilterOptionsService, nsSelect2RemoteOptions, $location, $bootbox, nsPinesService, spinnerService, $timeout) {
    $scope.filterOptions = nsFilterOptionsService.options;
    $scope.staffs = [];
    $scope.sortClasses = {};
    $scope.sortColumns = { column: null };
    $scope.errors = [];
    $scope.$on('NSHTTPError', function (event, data) {
      $scope.errors = [];
      $scope.errors.push({
        type: 'danger',
        msg: data
      });
      $('html, body').animate({ scrollTop: 0 }, 'fast');
    });
    $scope.sort = function (sortField) {
      if ($scope.sortColumns.column == null) {
        // first sort
        $scope.sortColumns.column = sortField;
        $scope.sortClasses[sortField] = 'fa fa-chevron-up';
      } else {
        if ($scope.sortColumns.column.indexOf(sortField) < 0) {
          // if this field isn't currently being sorted by
          $scope.sortColumns.column = sortField;
          $scope.sortClasses = {};
          $scope.sortClasses[sortField] = 'fa fa-chevron-up';
        } else {
          // we are sorting by this field already
          if ($scope.sortColumns.column.indexOf('-') < 0) {
            // if this field isn't currently being sorted by descending
            $scope.sortColumns.column = '-' + sortField;
            $scope.sortClasses = {};
            $scope.sortClasses[sortField] = 'fa fa-chevron-down';
          } else {
            // remove sort
            $scope.sortColumns.column = null;
            $scope.sortClasses = {};
          }
        }
      }
    };
    $scope.deleteStaff = function (id) {
      $bootbox.confirm('Are you sure you want to delete this staff member? <br><br><b>Note:</b> If the user is in use, you won\'t be able to delete the user and should \'disable\' the account instead.', function (result) {
        if (result) {
          nsStaffService.deleteStaff(id).then(function (response) {
            $scope.errors = [];
            nsPinesService.dataDeletedSuccessfully();
            LoadData($scope.filterOptions.selectedSchool.id);
          });
        }
      });
    };
    $scope.processQuickSearchStaff = function () {
      if (angular.isDefined($scope.filterOptions.quickSearchStaff)) {
        $location.path('staff-edit/' + $scope.filterOptions.quickSearchStaff.id);
      } else {
        $bootbox.alert('Please select a Staff member first.');
      }
    };
    var LoadData = function (schoolId) {
      $timeout(function () {
        spinnerService.show('tableSpinner');
      });
      if (schoolId > 0) {
        nsStaffService.getStaffBySchool($scope.filterOptions.selectedSchool.id).then(function (response) {
          $scope.staffs = response.data;
        }).finally(function () {
          spinnerService.hide('tableSpinner');
        });
      }
    };
    $scope.$watch('filterOptions.selectedSchool', function (newVal) {
      if (newVal !== null) {
        LoadData($scope.filterOptions.selectedSchool.id);
      }
    });
    $scope.StaffQuickSearchRemoteOptions = nsSelect2RemoteOptions.StaffQuickSearchRemoteOptions;
  }
  StaffPasswordResetController.$inject = [
    '$scope',
    'nsStaffService',
    'nsFilterOptionsService',
    '$http',
    'webApiBaseUrl'
  ];
  function StaffPasswordResetController($scope, nsStaffService, nsFilterOptionsService, $http, webApiBaseUrl) {
    $scope.userInfo = {};
    $scope.settings = {};
    $scope.errors = [];
    $scope.passwordValidator = function (password) {
      if (!password) {
        return;
      }
      if (password.length < 6) {
        return 'Password must be at least ' + 6 + ' characters long';
      }
      return true;
    };
    $scope.changePassword = function () {
      $http.post(webApiBaseUrl + '/api/staff/ResetUsersPassword', {
        Password: $scope.userInfo.pwd,
        UserName: $scope.userInfo.email
      }).then(function (response) {
        $scope.settings.pageStatus = 'success';
      });
    };
  }
  /* Movies Edit Controller */
  StaffEditController.$inject = [
    '$scope',
    '$routeParams',
    '$location',
    'nsStaffService',
    'nsSectionService',
    'NSUserInfoService',
    'NSStaff',
    'authService',
    'nsPinesService'
  ];
  function StaffEditController($scope, $routeParams, $location, nsStaffService, nsSectionService, NSUserInfoService, NSStaff, authService, nsPinesService) {
    $scope.gradeList = [];
    $scope.staffid = $routeParams.id;
    $scope.remoteUsernameValidationPath = NSUserInfoService.remoteUsernameOtherUserValidationPath;
    $scope.remoteEmailFormatValidationPath = NSUserInfoService.remoteEmailFormatValidationPath;
    $scope.remoteTeacherKeyValidationPath = NSUserInfoService.remoteTeacherKeyOtherUserValidationPath;
    $scope.staff = new NSStaff($routeParams.id);
    $scope.user = NSUserInfoService.currentUser;
    $scope.cancel = function () {
      $location.path('staff-list');
    };
    $scope.teacherKeySetArgs = function (val, el, attrs, ngModel) {
      return {
        value: val,
        UserId: $scope.staffid
      };
    };
    $scope.usernameSetArgs = function (val, el, attrs, ngModel) {
      return {
        value: val,
        UserId: $scope.staffid
      };
    };
    var LoadGrades = function () {
      nsSectionService.loadGrades().then(function (data) {
        $scope.gradeList.push.apply($scope.gradeList, data.data);
      });
    };
    $scope.select2GradeOptions = {
      minimumInputLength: 0,
      data: $scope.gradeList,
      multiple: true,
      width: 'resolve'
    };
    LoadGrades();
    // initial load
    //LoadData($routeParams.id);
    $scope.saveData = function () {
      nsStaffService.saveStaff($scope.staff).then(function (response) {
        nsPinesService.dataSavedSuccessfully();
        if ($scope.staffid == $scope.user.Id && $scope.staff.OriginalUserName != $scope.staff.Email) {
          nsPinesService.buildMessage('You have been logged out', 'Your email address has changed and you have been logged out', 'info');
          authService.logOut();
        } else {
          // TODO: notice service
          $location.path('staff-list');
        }
      });
    };  //nsStaffService.saveStaff($scope.staff)
  }
}());
(function () {
  'use strict';
  angular.module('assessmentModule', []).controller('AssessmentPreviewController', AssessmentPreviewController).controller('AssessmentListController', AssessmentListController).controller('AssessmentAddController', AssessmentAddController).controller('AssessmentEditController', AssessmentEditController).controller('AssessmentDefaultEntryController', AssessmentDefaultEntryController).controller('AssessmentDeleteController', AssessmentDeleteController);
  /* Movies List Controller  */
  AssessmentListController.$inject = [
    '$scope',
    '$http',
    'Assessment',
    '$location',
    'nsPinesService',
    '$bootbox',
    'webApiBaseUrl'
  ];
  function AssessmentListController($scope, $http, Assessment, $location, nsPinesService, $bootbox, webApiBaseUrl) {
    //$scope.assessments = Assessment.query();
    var LoadData = function () {
      $http.get(webApiBaseUrl + '/api/assessmentavailability/GetAssessmentList').then(function (response) {
        $scope.assessments = response.data.Assessments;
      });
    };
    LoadData();
    $scope.copyAsInterventionTest = function (assessmentId) {
      $http.post(webApiBaseUrl + '/api/assessment/copyasinterventiontest', { Id: assessmentId }).then(function () {
        nsPinesService.dataSavedSuccessfully();
      });
    };
    $scope.simpleCopy = function (assessmentId) {
      $http.post(webApiBaseUrl + '/api/assessment/simplecopy', { Id: assessmentId }).then(function () {
        nsPinesService.dataSavedSuccessfully();
      });
    };
    $scope.remove = function (assessmentId) {
      $bootbox.confirm('Are you sure you want to delete this assessment?', function (result) {
        if (result === true) {
          $http.post(webApiBaseUrl + '/api/assessment/delete', { Id: assessmentId }).then(function () {
            nsPinesService.dataDeletedSuccessfully();
            LoadData();
          });
        }
      });
    };
    $scope.dataSavedSuccessfully = function () {
      pinesNotifications.notify({
        title: 'Data Saved',
        text: 'The assessment was deleted successfully.',
        type: 'success'
      });
    };
    $scope.dataError = function () {
      pinesNotifications.notify({
        title: 'Data Not Saved',
        text: 'The assessment was not successfully.  It is likely in use.',
        type: 'error'
      });
    };
  }
  /* Assessment Preview Controller */
  AssessmentPreviewController.$inject = [
    '$scope',
    '$location',
    'Assessment'
  ];
  function AssessmentPreviewController($scope, $location, Assessment) {
  }
  /* Movies Create Controller */
  AssessmentAddController.$inject = [
    '$scope',
    '$location',
    'Assessment'
  ];
  function AssessmentAddController($scope, $location, Assessment) {
    $scope.assessment = new Assessment();
    $scope.add = function () {
      $scope.assessment.$save(function () {
        $location.path('/');
      }, function (error) {
        _showValidationErrors($scope, error);
      });
    };
  }
  /* Movies Edit Controller */
  AssessmentEditController.$inject = [
    '$scope',
    '$routeParams',
    '$location',
    'Assessment',
    'formService',
    '$http',
    'webApiBaseUrl'
  ];
  function AssessmentEditController($scope, $routeParams, $location, Assessment, formService, $http, webApiBaseUrl) {
    var assessmentId = $routeParams.id;
    if ($routeParams.id === undefined) {
      assessmentId = -1;
    }
    $scope.loadFields = function () {
      var groupId = $scope.assessment.SelectedGroup === null ? 0 : $scope.assessment.SelectedGroup;
      var subCatId = $scope.assessment.SelectedSubcategory === null ? 0 : $scope.assessment.SelectedSubcategory;
      var categoryId = $scope.assessment.SelectedCategory === null ? 0 : $scope.assessment.SelectedCategory;
      var dbTable = $scope.assessment.SelectedDBTable ? $scope.assessment.SelectedDBTable : 'primary';
      var paramObj = {
          groupId: groupId,
          subCategoryId: subCatId,
          categoryId: categoryId,
          page: $scope.assessment.SelectedPage,
          assessmentId: $routeParams.id,
          dbTable: dbTable
        };
      $http.post(webApiBaseUrl + '/api/assessment/GetFieldsForAssessment', paramObj).then(function (response) {
        angular.copy(response.data.Fields, $scope.assessment.Fields);
      });
    };
    $scope.loadGroups = function () {
      var startOrder = $scope.assessment.StartSortOrder === null ? 1 : $scope.assessment.StartSortOrder;
      var endOrder = $scope.assessment.EndSortOrder === null ? 100 : $scope.assessment.EndSortOrder;
      var paramObj = {
          assessmentId: $routeParams.id,
          startOrder: startOrder,
          endOrder: endOrder
        };
      $http.post(webApiBaseUrl + '/api/assessment/GetGroupsForAssessment', paramObj).then(function (response) {
        angular.copy(response.data.Groups, $scope.assessment.FieldGroups);
      });
    };
    Assessment.get({ id: assessmentId }).$promise.then(function (data) {
      $scope.assessment = data;
      $scope.assessment.TestType = $scope.assessment.TestType + '';
      $scope.assessment.BaseType = $scope.assessment.BaseType + '';
      // TODO: this is temporary until we get DTOs built to do this automatically
      $scope.assessment.SelectedCategory = 0;
      $scope.assessment.SelectedGroup = 0;
      $scope.assessment.SelectedPage = 0;
      $scope.assessment.SelectedSubcategory = 0;
      $scope.assessment.Fields = [];
      $scope.assessment.FieldGroups = [];  //$scope.assessment.FieldCategories = [];
                                           //$scope.assessment.FieldSubCategories = [];
    });
    $scope.FieldTypes = formService.fields;
    $scope.edit = function () {
      $scope.assessment.$save(function () {
        $location.path('assessment-list');
      }, function (error) {
        _showValidationErrors($scope, error);
      });
    };
    // Start Field Functions
    $scope.addField = {};
    $scope.addField.lastAddedID = 500000;
    $scope.deleteField = function (name) {
      for (var i = 0; i < $scope.assessment.Fields.length; i++) {
        if ($scope.assessment.Fields[i].DisplayLabel == name) {
          //$scope.assessment.Fields.splice(i, 1);
          $scope.assessment.Fields[i].IsFlaggedForDelete = true;
          break;
        }
      }
    };
    $scope.addNewField = function () {
      // new approach... cycle through all fields and get the greatest ID and add one
      if ($scope.assessment.Fields != null) {
        for (var i = 0; i < $scope.assessment.Fields.length; i++) {
          if ($scope.assessment.Fields[i].Id >= $scope.addField.lastAddedID) {
            $scope.addField.lastAddedID = $scope.assessment.Fields[i].Id;
            $scope.addField.lastAddedID++;
          }
        }
      } else {
        $scope.assessment.Fields = [];
      }
      var newField = {
          'Id': $scope.addField.lastAddedID,
          'DisplayLabel': 'Field - ' + $scope.addField.lastAddedID,
          'SortOrder': 1,
          'AssessmentId': $scope.assessment.Id
        };
      // put newField into fields array
      $scope.assessment.Fields.push(newField);
    };
    // End Field Functions
    // Start Group Functions
    $scope.addGroup = {};
    $scope.addGroup.lastAddedID = 500000;
    $scope.deleteGroup = function (id) {
      for (var i = 0; i < $scope.assessment.FieldGroups.length; i++) {
        if ($scope.assessment.FieldGroups[i].Id == id) {
          $scope.assessment.FieldGroups[i].IsFlaggedForDelete = true;
          //$scope.assessment.FieldGroups.splice(i, 1);
          break;
        }
      }
    };
    $scope.getSubGroup = function (id) {
      var groupName = '';
      if (id) {
        for (var i = 0; i < $scope.assessment.FieldGroups.length; i++) {
          if ($scope.assessment.FieldGroups[i].Id == id) {
            groupName = ' / Grp:' + $scope.assessment.FieldGroups[i].DisplayName;
            break;
          }
        }
      }
      return groupName;
    };
    $scope.addNewGroup = function () {
      // new approach... cycle through all fields and get the greatest ID and add one
      if ($scope.assessment.FieldGroups != null) {
        for (var i = 0; i < $scope.assessment.FieldGroups.length; i++) {
          if ($scope.assessment.FieldGroups[i].Id >= $scope.addGroup.lastAddedID) {
            $scope.addGroup.lastAddedID = $scope.assessment.FieldGroups[i].Id;
            $scope.addGroup.lastAddedID++;
          }
        }
      } else {
        $scope.assessment.FieldGroups = [];
      }
      var newGroup = {
          'Id': $scope.addGroup.lastAddedID,
          'DisplayName': 'Group - ' + $scope.addGroup.lastAddedID,
          'SortOrder': 1,
          'AssessmentId': $scope.assessment.Id
        };
      // put newField into fields array
      $scope.assessment.FieldGroups.push(newGroup);
    };
    // End Group Functions
    // Start Category Functions
    $scope.addCategory = {};
    $scope.addCategory.lastAddedID = 500000;
    $scope.deleteCategory = function (id) {
      for (var i = 0; i < $scope.assessment.FieldCategories.length; i++) {
        if ($scope.assessment.FieldCategories[i].Id == id) {
          $scope.assessment.FieldCategories.splice(i, 1);
          break;
        }
      }
    };
    $scope.getCategory = function (id) {
      var groupName = '';
      if (id) {
        for (var i = 0; i < $scope.assessment.FieldCategories.length; i++) {
          if ($scope.assessment.FieldCategories[i].Id == id) {
            groupName = ' / Cat:' + $scope.assessment.FieldCategories[i].DisplayName;
            break;
          }
        }
      }
      return groupName;
    };
    $scope.addNewCategory = function () {
      // new approach... cycle through all fields and get the greatest ID and add one
      if ($scope.assessment.FieldCategories != null) {
        for (var i = 0; i < $scope.assessment.FieldCategories.length; i++) {
          if ($scope.assessment.FieldCategories[i].Id >= $scope.addCategory.lastAddedID) {
            $scope.addCategory.lastAddedID = $scope.assessment.FieldCategories[i].Id;
            $scope.addCategory.lastAddedID++;
          }
        }
      } else {
        $scope.assessment.FieldCategories = [];
      }
      var newCategory = {
          'Id': $scope.addCategory.lastAddedID,
          'DisplayName': 'Category - ' + $scope.addCategory.lastAddedID,
          'SortOrder': 1,
          'AssessmentId': $scope.assessment.Id
        };
      // put newField into fields array
      $scope.assessment.FieldCategories.push(newCategory);
    };
    // End Category Functions
    // Start Sub-Category Functions
    $scope.addSubCategory = {};
    $scope.addSubCategory.lastAddedID = 500000;
    $scope.deleteSubCategory = function (id) {
      for (var i = 0; i < $scope.assessment.FieldSubCategories.length; i++) {
        if ($scope.assessment.FieldSubCategories[i].Id == id) {
          $scope.assessment.FieldSubCategories.splice(i, 1);
          break;
        }
      }
    };
    $scope.getSubCategory = function (id) {
      var groupName = '';
      if (id) {
        for (var i = 0; i < $scope.assessment.FieldSubCategories.length; i++) {
          if ($scope.assessment.FieldSubCategories[i].Id == id) {
            groupName = ' / SC:' + $scope.assessment.FieldSubCategories[i].DisplayName;
            break;
          }
        }
      }
      return groupName;
    };
    $scope.addNewSubCategory = function () {
      // new approach... cycle through all fields and get the greatest ID and add one
      if ($scope.assessment.FieldSubCategories != null) {
        for (var i = 0; i < $scope.assessment.FieldSubCategories.length; i++) {
          if ($scope.assessment.FieldSubCategories[i].Id >= $scope.addSubCategory.lastAddedID) {
            $scope.addSubCategory.lastAddedID = $scope.assessment.FieldCategories[i].Id;
            $scope.addSubCategory.lastAddedID++;
          }
        }
      } else {
        $scope.assessment.FieldSubCategories = [];
      }
      var newSubCategory = {
          'Id': $scope.addSubCategory.lastAddedID,
          'DisplayName': 'Sub-Category - ' + $scope.addSubCategory.lastAddedID,
          'SortOrder': 1,
          'AssessmentId': $scope.assessment.Id
        };
      // put newField into fields array
      $scope.assessment.FieldSubCategories.push(newSubCategory);
    }  // End Sub-Category Functions
;
  }
  AssessmentDefaultEntryController.$inject = [
    '$scope',
    '$routeParams',
    '$location',
    'Assessment',
    'formService',
    '$http',
    'pinesNotifications',
    'webApiBaseUrl'
  ];
  function AssessmentDefaultEntryController($scope, $routeParams, $location, Assessment, formService, $http, pinesNotifications, webApiBaseUrl) {
    // get lookup field values
    $http.get(webApiBaseUrl + '/api/assessment/GetLookupFieldsForAssessment/' + $routeParams.assessmentId).success(function (lookupData) {
      $scope.lookupFieldsArray = lookupData;
      $scope.sortArray = [];
      $scope.headerClassArray = [];
      $scope.firstNameHeaderClass = 'fa';
      $scope.lastNameHeaderClass = 'fa';
      $scope.sort = function (column) {
        var columnIndex = -1;
        // if this is not a first or lastname column
        if (!isNaN(parseInt(column))) {
          columnIndex = column;
          switch ($scope.fields[column].FieldType) {
          case 'Textfield':
            column = 'FieldResults[' + column + '].StringValue';
            break;
          case 'DateCheckbox':
            column = 'FieldResults[' + column + '].DateValue';
            break;
          case 'DecimalRange':
            column = 'FieldResults[' + column + '].DecimalValue';
            break;
          case 'DropdownRange':
            column = 'FieldResults[' + column + '].IntValue';
            break;
          case 'DropdownFromDB':
            column = 'FieldResults[' + column + '].IntValue';
            break;
          case 'CalculatedFieldDbOnly':
            column = 'FieldResults[' + column + '].StringValue';
            break;
          case 'CalculatedFieldDbBacked':
            column = 'FieldResults[' + column + '].IntValue';
            break;
          case 'CalculatedFieldDbBackedString':
            column = 'FieldResults[' + column + '].StringValue';
            break;
          case 'CalculatedFieldClientOnly':
            column = 'FieldResults[' + column + '].StringValue';
            //shouldnt even be used in sorting
            break;
          default:
            column = 'FieldResults[' + column + '].IntValue';
            break;
          }
        }
        var bFound = false;
        for (var j = 0; j < $scope.sortArray.length; j++) {
          // if it is already on the list, reverse the sort
          if ($scope.sortArray[j].indexOf(column) >= 0) {
            bFound = true;
            // is it already negative? if so, remove it
            if ($scope.sortArray[j].indexOf('-') === 0) {
              if (columnIndex > -1) {
                $scope.headerClassArray[columnIndex] = 'fa';
              } else if (column === 'FirstName') {
                $scope.firstNameHeaderClass = 'fa';
              } else if (column === 'LastName') {
                $scope.lastNameHeaderClass = 'fa';
              }
              $scope.sortArray.splice(j, 1);
            } else {
              if (columnIndex > -1) {
                $scope.headerClassArray[columnIndex] = 'fa fa-chevron-down';
              } else if (column === 'FirstName') {
                $scope.firstNameHeaderClass = 'fa fa-chevron-down';
              } else if (column === 'LastName') {
                $scope.lastNameHeaderClass = 'fa fa-chevron-down';
              }
              $scope.sortArray[j] = '-' + $scope.sortArray[j];
            }
            break;
          }
        }
        if (!bFound) {
          $scope.sortArray.push(column);
          if (columnIndex > -1) {
            $scope.headerClassArray[columnIndex] = 'fa fa-chevron-up';
          } else if (column === 'FirstName') {
            $scope.firstNameHeaderClass = 'fa fa-chevron-up';
          } else if (column === 'LastName') {
            $scope.lastNameHeaderClass = 'fa fa-chevron-up';
          }
        }  //if ($scope.orderProp === column) {
           //	$scope.direction = !$scope.direction;
           //} else {
           //	$scope.orderProp = column;
           //	$scope.direction = true;
           //}
      };
      // strip unnecessary stuff off of studentResult to increase speed
      $scope.deleteAssessmentData = function (studentResult) {
        var assessmentId = $scope.assessment.Id;
        //var studentResult = {};
        var returnObject = {
            StudentResult: studentResult,
            AssessmentId: assessmentId
          };
        // TODO: update local model, delete all field values
        // loop over each field and delete the values... also delete anything else relevant
        for (var k = 0; k < studentResult.FieldResults.length; k++) {
          studentResult.FieldResults[k].IntValue = null;
          studentResult.FieldResults[k].DecimalValue = null;
          studentResult.FieldResults[k].DateValue = null;
          studentResult.FieldResults[k].StringValue = null;
          studentResult.FieldResults[k].DisplayValue = null;
        }
        $http.post(webApiBaseUrl + '/api/assessment/DeleteAssessmentResult', returnObject).success(function (data) {
          $scope.dataDeletedSuccessfully();
        }).error(function (data, status, headers, config) {
          alert('error deleting');
        });
      };
      $scope.saveAssessmentData = function (studentResult) {
        var assessmentId = $scope.assessment.Id;
        //var studentResult = {};
        var returnObject = {
            StudentResult: studentResult,
            AssessmentId: assessmentId
          };
        $http.post(webApiBaseUrl + '/api/assessment/SaveAssessmentResult', returnObject).success(function (data) {
          // set values for the lookup fileds
          for (var k = 0; k < studentResult.FieldResults.length; k++) {
            for (var i = 0; i < $scope.fields.length; i++) {
              if ($scope.fields[i].DatabaseColumn == studentResult.FieldResults[k].DbColumn) {
                // set display value
                if ($scope.fields[i].FieldType === 'DropdownFromDB') {
                  for (var p = 0; p < $scope.lookupFieldsArray.length; p++) {
                    if ($scope.lookupFieldsArray[p].LookupColumnName === $scope.fields[i].LookupFieldName) {
                      // now find the specifc value that matches
                      for (var y = 0; y < $scope.lookupFieldsArray[p].LookupFields.length; y++) {
                        if (studentResult.FieldResults[k].IntValue === $scope.lookupFieldsArray[p].LookupFields[y].FieldSpecificId) {
                          studentResult.FieldResults[k].DisplayValue = $scope.lookupFieldsArray[p].LookupFields[y].FieldValue;
                        }
                      }
                    }
                  }
                }  // set the values passed back from the save method to the calculated fields
                else if ($scope.fields[i].FieldType === 'CalculatedFieldDbOnly' || $scope.fields[i].FieldType === 'CalculatedFieldDbBacked' || $scope.fields[i].FieldType === 'CalculatedFieldDbBackedString') {
                  for (var p = 0; p < data.length; p++) {
                    if (data[p].DbColumn === studentResult.FieldResults[k].DbColumn) {
                      studentResult.FieldResults[k].IntValue = data[p].IntValue;
                      studentResult.FieldResults[k].StringValue = data[p].StringValue;
                    }
                  }
                }
              }
            }
          }
          $scope.dataSavedSuccessfully();  // TODO: update diplayvalue of any dropdownfromdb fields
        }).error(function (data, status, headers, config) {
          alert('error saving');
        });
      };
      $scope.dataSavedSuccessfully = function () {
        pinesNotifications.notify({
          title: 'Data Saved',
          text: 'Your data was saved successfully.',
          type: 'success'
        });
      };
      $scope.dataDeletedSuccessfully = function () {
        pinesNotifications.notify({
          title: 'Data Deleted',
          text: 'Your data was deleted successfully.',
          type: 'success'
        });
      };
      $http.get(webApiBaseUrl + '/api/assessment/getassessmentresults/' + $routeParams.assessmentId + '/' + $routeParams.classId + '/' + $routeParams.benchmarkDateId).success(function (data) {
        $scope.assessment = data.Assessment;
        $scope.fields = data.Assessment.Fields;
        $scope.studentResults = data.StudentResults;
        for (var r = 0; r < $scope.fields.length; r++) {
          $scope.headerClassArray[r] = 'fa';
        }
        // Add FieldOrder, FieldType
        for (var j = 0; j < $scope.studentResults.length; j++) {
          for (var k = 0; k < $scope.studentResults[j].FieldResults.length; k++) {
            for (var i = 0; i < $scope.fields.length; i++) {
              if ($scope.fields[i].DatabaseColumn == $scope.studentResults[j].FieldResults[k].DbColumn) {
                $scope.studentResults[j].FieldResults[k].Field = $scope.fields[i];
                // set display value
                if ($scope.fields[i].FieldType === 'DropdownFromDB') {
                  for (var p = 0; p < $scope.lookupFieldsArray.length; p++) {
                    if ($scope.lookupFieldsArray[p].LookupColumnName === $scope.fields[i].LookupFieldName) {
                      // now find the specifc value that matches
                      for (var y = 0; y < $scope.lookupFieldsArray[p].LookupFields.length; y++) {
                        if ($scope.studentResults[j].FieldResults[k].IntValue === $scope.lookupFieldsArray[p].LookupFields[y].FieldSpecificId) {
                          $scope.studentResults[j].FieldResults[k].DisplayValue = $scope.lookupFieldsArray[p].LookupFields[y].FieldValue;
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }  //alert(data);
      });
    });
  }
  /* Movies Delete Controller  */
  AssessmentDeleteController.$inject = [
    '$scope',
    '$routeParams',
    '$location',
    'Assessment'
  ];
  function AssessmentDeleteController($scope, $routeParams, $location, Assessment) {
    $scope.assessment = Assessment.get({ id: $routeParams.id });
    $scope.remove = function () {
      $scope.assessment.$remove({ id: $scope.sassessmenttaff.Id }, function () {
        $location.path('/');
      });
    };
  }
  /* Utility Functions */
  function _showValidationErrors($scope, error) {
    $scope.validationErrors = [];
    if (error.data && angular.isObject(error.data)) {
      for (var key in error.data) {
        $scope.validationErrors.push(error.data[key][0]);
      }
    } else {
      $scope.validationErrors.push('Could not add assessment.');
    }
    ;
  }
}());
(function () {
  'use strict';
  angular.module('lineGraphModule', []).directive('lineGraphDetail', [
    '$http',
    function ($http) {
      return {
        restrict: 'E',
        templateUrl: 'templates/linegraph-detail.html',
        scope: { dataManager: '=' }
      };
    }
  ]).directive('lineGraphDetailIg', [
    '$http',
    function ($http) {
      return {
        restrict: 'E',
        templateUrl: 'templates/linegraph-detail-ig.html',
        scope: { dataManager: '=' }
      };
    }
  ]).directive('studentInterventions', [
    '$http',
    '$location',
    function ($http, $location) {
      return {
        restrict: 'E',
        templateUrl: 'templates/student-interventions.html',
        scope: { dataManager: '=' },
        link: function (scope, element, attr) {
          scope.goToDashboard = function (schoolYear, school, interventionist, interventionGroup, studentId, stint) {
            $location.path('ig-dashboard/' + schoolYear + '/' + school + '/' + interventionist + '/' + interventionGroup + '/' + studentId + '/' + stint);
          };
        }
      };
    }
  ]).controller('LineGraphController', LineGraphController).controller('LineGraphIGController', LineGraphIGController).factory('NSStudentInterventionManager', [
    '$http',
    'webApiBaseUrl',
    function ($http, webApiBaseUrl) {
      var NSStudentInterventionManager = function () {
        var self = this;
        self.formatDate2 = function (inDate) {
          if (inDate != null) {
            return moment(inDate).format('DD-MMM-YYYY');
          } else {
            return 'N/A';
          }
        };
        self.LoadData = function (studentId) {
          var returnObject = { id: studentId };
          var url = webApiBaseUrl + '/api/student/GetStudentInterventions';
          var promise = $http.post(url, returnObject);
          return promise.then(function (response) {
            angular.extend(self, response.data);
            if (self.Interventions === null)
              self.Interventions = [];
          });
        };
      };
      return NSStudentInterventionManager;
    }
  ]).factory('NSStudentAssessmentFieldManager', [
    '$http',
    'webApiBaseUrl',
    function ($http, webApiBaseUrl) {
      var NSStudentAssessmentFieldManager = function () {
        var self = this;
        self.Fields = [];
        self.LoadData = function (assessmentTypeId, studentId, interventionGroupId) {
          var returnObject = {
              StudentId: studentId,
              AssessmentTypeId: assessmentTypeId,
              InterventionGroupId: interventionGroupId
            };
          var url = webApiBaseUrl + '/api/linegraph/GetStudentLineGraphFields';
          var promise = $http.post(url, returnObject);
          return promise.then(function (response) {
            angular.extend(self, response.data);
            if (self.Fields === null)
              self.Fields = [];
          });
        };
      };
      return NSStudentAssessmentFieldManager;
    }
  ]).factory('NSAssesssmentLineGraphManager', [
    '$http',
    'webApiBaseUrl',
    function ($http, webApiBaseUrl) {
      var NSAssesssmentLineGraphManager = function () {
        var self = this;
        self.LoadData = function (assessmentId, fieldToRetrieve, lookupFieldName, fieldType, studentId, fieldDisplayName, sectionId, assessmentName, interventions) {
          self.bUseExceeds = false;
          self.bUseMeets = false;
          self.bUseApproaches = false;
          self.bUseDoesNotMeet = false;
          self.assessmentName = assessmentName;
          self.fieldToRetrieve = fieldDisplayName;
          self.status = {};
          self.status.isLookup = lookupFieldName != null && lookupFieldName != '';
          self.status.isDecimalField = false;
          // TODO: check field type
          var returnObject = {
              AssessmentId: assessmentId,
              FieldToRetrieve: fieldToRetrieve,
              LookupFieldName: lookupFieldName,
              IsLookupColumn: self.status.isLookup,
              StudentId: studentId
            };
          var url = webApiBaseUrl + '/api/lineGraph/GetStudentLineGraph';
          var returnData = $http.post(url, returnObject);
          self.Results = [];
          self.Fields = [];
          self.BenchmarkDates = [];
          self.Interventions = interventions == null ? [] : interventions;
          self.VScale = [];
          // self.Fields = {};
          self.chartConfig = {};
          return returnData.then(function (response) {
            angular.extend(self, response.data);
            if (self.Results === null)
              self.Results = [];
            if (self.Fields === null)
              self.Fields = [];
            if (self.BenchmarkDates === null)
              self.BenchmarkDates = [];
            //if (self.Interventions === null) self.Interventions = [];
            if (self.VScale === null)
              self.VScale = [];
            //if (self.Fields === null) {
            //    self.Fields = {};
            //}
            //else {
            //    self.Fields = self.Fields.Assessments[0].Fields;
            //}
            self.postDataLoadSetup();
          });
        };
        self.LoadAssessmentFields = function () {
          var url = webApiBaseUrl + '/api/benchmark/GetAssessmentsAndFields';
          var promise = $http.get(url);
          return promise.then(function (response) {
            self.assessments = self.flatten(response.data.Assessments);
          });
        };
        self.flatten = function (data) {
          var out = [];
          angular.forEach(data, function (d) {
            angular.forEach(d.Fields, function (v) {
              out.push({
                AssessmentName: d.AssessmentName,
                DisplayLabel: v.DisplayLabel,
                FieldName: v.DatabaseColumn,
                LookupFieldName: v.LookupFieldName,
                AssessmentId: d.Id,
                FieldType: v.FieldType,
                RangeHigh: v.RangeHigh,
                RangeLow: v.RangeLow,
                DisplayInLineGraphs: v.DisplayInLineGraphs
              });
            });
          });
          return out;
        };
        self.postDataLoadSetup = function () {
          var self = this;
          var currentMax = 1;
          var data = [];
          var doesnotmeetData = [];
          var approachesData = [];
          var meetsData = [];
          var exceedsData = [];
          var plotlines = [];
          var loopCounter = 0;
          var plotLinesLoopCounter = 0;
          var highchartsNgConfig = {};
          // determine which benchmarks we want to use
          for (var d = 0; d < self.BenchmarkDates.length; d++) {
            if (self.BenchmarkDates[d].Exceeds !== null || self.bUseExceeds == true) {
              self.bUseExceeds = true;
            }
            if (self.BenchmarkDates[d].Meets !== null || self.bUseMeets == true) {
              self.bUseMeets = true;
            }
            if (self.BenchmarkDates[d].Approaches !== null || self.bUseApproaches == true) {
              self.bUseApproaches = true;
            }
            if (self.BenchmarkDates[d].DoesNotMeet !== null || self.bUseDoesNotMeet == true) {
              self.bUseDoesNotMeet = true;
            }
          }
          // set up fields
          for (var r = 0; r < self.Fields.length; r++) {
            self.Fields[r].BenchmarkDates = angular.copy(self.BenchmarkDates);
            for (var i = 0; i < self.Fields[r].BenchmarkDates.length; i++) {
              self.Fields[r].BenchmarkDates[i].Result = {};
              self.Fields[r].BenchmarkDates[i].Result.StringValue = 'N/A';
              // magic number
              for (var j = 0; j < self.Results.length; j++) {
                if (self.Fields[r].BenchmarkDates[i].TestDueDateID == self.Results[j].TestDueDateID) {
                  // attach result to the proper benchmark date
                  //self.Fields[r].BenchmarkDates[i].Result = self.Results[j];
                  // now get the proper result for this field
                  for (var p = 0; p < self.Results[j].FieldResults.length; p++) {
                    if (self.Results[j].FieldResults[p].DbColumn === self.Fields[r].DatabaseColumn) {
                      self.Fields[r].BenchmarkDates[i].Result.StringValue = self.Results[j].FieldResults[p].StringValue;
                      break;
                    }
                  }
                }
              }
            }
          }
          // set up series
          for (var i = 0; i < self.BenchmarkDates.length; i++) {
            self.BenchmarkDates[i].Result = {};
            self.BenchmarkDates[i].Result.FieldValueID = null;
            // magic number
            self.BenchmarkDates[i].Result.FieldDisplayValue = 'N/A';
            // magic number
            currentMax = self.BenchmarkDates[i].TestNumber;
            // set benchmark data series
            doesnotmeetData[i] = [
              i + 1,
              self.BenchmarkDates[i].DoesNotMeet
            ];
            approachesData[i] = [
              i + 1,
              self.BenchmarkDates[i].Approaches
            ];
            meetsData[i] = [
              i + 1,
              self.BenchmarkDates[i].Meets
            ];
            exceedsData[i] = [
              i + 1,
              self.BenchmarkDates[i].Exceeds
            ];
            for (var j = 0; j < self.Results.length; j++) {
              if (self.BenchmarkDates[i].TestDueDateID == self.Results[j].TestDueDateID) {
                // attach result to the proper benchmark date
                self.BenchmarkDates[i].Result = self.Results[j];
                // process the result, set colors, etc
                if (self.Results[j].FieldDisplayValue != null && self.Results[j].FieldDisplayValue != '') {
                  self.Results[j].x = i + 1;
                  var fillColor = self.Results[j].IsCopied ? '#FFFFFF' : '#2244BB';
                  data[loopCounter] = {
                    marker: { fillColor: fillColor },
                    x: i + 1,
                    y: self.Results[j].FieldValueID,
                    name: self.Results[j].FieldDisplayValue,
                    dataLabels: {
                      enabled: true,
                      crop: false,
                      style: { fontWeight: 'bold' },
                      formatter: function () {
                        return this.point.name;
                      }
                    }
                  };
                } else {
                  self.Results[j].x = i + 1;
                  data[loopCounter] = [
                    i + 1,
                    self.Results[j].FieldValueID
                  ];
                }
                loopCounter++;
              }
            }
            // set up interventions
            // TODO: create a formula for the Y value.  something like +30 for every time through
            for (var j = 0; j < self.Interventions.length; j++) {
              if (self.BenchmarkDates[i].TestDueDateID == self.Interventions[j].StartTDDID) {
                plotlines[plotLinesLoopCounter] = {
                  color: '#2244BB',
                  dashStyle: 'solid',
                  value: currentMax,
                  width: '2',
                  label: {
                    text: 'Start ' + self.Interventions[j].InterventionType,
                    align: 'left',
                    y: 20,
                    x: 3,
                    rotation: 0,
                    style: { fontFamily: 'Arial' }
                  },
                  zIndex: 0
                };
                plotLinesLoopCounter++;
              }
              if (self.BenchmarkDates[i].TestDueDateID == self.Interventions[j].EndTDDID) {
                plotlines[plotLinesLoopCounter] = {
                  color: '#2244BB',
                  dashStyle: 'solid',
                  value: currentMax,
                  width: '2',
                  label: {
                    text: 'End ' + self.Interventions[j].InterventionType,
                    align: 'left',
                    y: 45,
                    x: 3,
                    rotation: 0,
                    style: { fontFamily: 'Arial' }
                  },
                  zIndex: 0
                };
                plotLinesLoopCounter++;
              }
            }
          }
          var yAxis = null;
          if (self.status.isLookup) {
            yAxis = {
              allowDecimals: false,
              min: 0,
              title: { text: self.fieldToRetrieve },
              labels: {
                formatter: function () {
                  var value = null;
                  for (var i = 0; i < self.VScale.length; i++) {
                    if (self.VScale[i].FieldSpecificId == this.value) {
                      return self.VScale[i].FieldValue;
                    }
                  }
                  return null;
                }
              }
            };
          } else {
            yAxis = {
              allowDecimals: false,
              min: 0,
              title: { text: self.fieldToRetrieve }
            };
          }
          // TODO:  dynamically push series depending on if they have values or not
          var series = [];
          series.push({
            name: self.fieldToRetrieve,
            color: '#2244BB',
            data: data,
            lineWidth: 3,
            shadow: {
              offsetX: 3,
              offsetY: 2,
              opacity: 0.12,
              width: 6
            },
            zIndex: 100,
            marker: {
              lineWidth: 2,
              lineColor: null
            },
            tooltip: {
              headerFormat: '',
              pointFormatter: function (point) {
                //getDisplayValue(date.Result.FieldValueID)
                var responseString = '';
                // header score ROW
                for (var j = 0; j < self.BenchmarkDates.length; j++) {
                  var currentBenchmarkDate = self.BenchmarkDates[j];
                  // if we are on the right benchmark date
                  if (j + 1 == this.x) {
                    responseString = '<span style="font-weight:bold;color:' + this.color + '">' + self.formatDate(currentBenchmarkDate.DueDate) + '</span><br/>';
                    responseString += '<span style="color:' + this.color + '">\u25cf</span> ' + 'Score: ';
                    responseString += '<b>' + self.getDisplayValue(currentBenchmarkDate.Result.FieldValueID) + '</b><br/>';
                    break;
                  }
                }
                for (var i = 0; i < self.Fields.length; i++) {
                  var field = self.Fields[i];
                  // don't display text fields
                  if (field.FieldType == 'Textarea') {
                    continue;
                  }
                  responseString += field.DisplayLabel + ' : ';
                  for (var j = 0; j < field.BenchmarkDates.length; j++) {
                    var currentBenchmarkDate = field.BenchmarkDates[j];
                    // if we are on the right benchmark date
                    if (j + 1 == this.x) {
                      if (currentBenchmarkDate.Result.StringValue == null) {
                        responseString += '<br/>';
                      } else {
                        responseString += '<b>' + (field.AltDisplayLabel ? field.AltDisplayLabel : '') + currentBenchmarkDate.Result.StringValue + '</b><br/>';
                      }
                      break;
                    }
                  }
                }
                return responseString;
              }
            }
          });
          if (self.bUseExceeds) {
            series.push({
              name: 'Exceeds',
              color: '#4697ce',
              data: exceedsData,
              lineWidth: 3,
              shadow: false,
              zIndex: 2,
              dashStyle: 'ShortDash',
              marker: { enabled: false },
              tooltip: {
                headerFormat: '<span style="font-size: 10px">Test {point.key}</span><br/>',
                pointFormatter: function () {
                  for (var i = 0; i < self.VScale.length; i++) {
                    if (self.VScale[i].FieldSpecificId == this.y) {
                      return '<span style="color:' + this.color + '">\u25cf</span> ' + this.series.name + ': <b>' + self.VScale[i].FieldValue + '</b><br/>';
                    }
                  }
                  return '<span style="color:' + this.color + '">\u25cf</span> ' + this.series.name + ': <b>' + this.y + '</b><br/>';
                }
              }
            });
          }
          if (self.bUseMeets) {
            series.push({
              name: 'Meets',
              color: '#ABF09C',
              data: meetsData,
              lineWidth: 3,
              shadow: false,
              zIndex: 2,
              dashStyle: 'ShortDash',
              marker: { enabled: false },
              tooltip: {
                headerFormat: '<span style="font-size: 10px">Test {point.key}</span><br/>',
                pointFormatter: function () {
                  for (var i = 0; i < self.VScale.length; i++) {
                    if (self.VScale[i].FieldSpecificId == this.y) {
                      return '<span style="color:' + this.color + '">\u25cf</span> ' + this.series.name + ': <b>' + self.VScale[i].FieldValue + '</b><br/>';
                    }
                  }
                  return '<span style="color:' + this.color + '">\u25cf</span> ' + this.series.name + ': <b>' + this.y + '</b><br/>';
                }
              }
            });
          }
          if (self.bUseApproaches) {
            series.push({
              name: 'Approaches',
              color: '#E4D354',
              data: approachesData,
              lineWidth: 3,
              shadow: false,
              zIndex: 3,
              dashStyle: 'ShortDash',
              marker: { enabled: false },
              tooltip: {
                headerFormat: '<span style="font-size: 10px">Test {point.key}</span><br/>',
                pointFormatter: function () {
                  for (var i = 0; i < self.VScale.length; i++) {
                    if (self.VScale[i].FieldSpecificId == this.y) {
                      return '<span style="color:' + this.color + '">\u25cf</span> ' + this.series.name + ': <b>' + self.VScale[i].FieldValue + '</b><br/>';
                    }
                  }
                  return '<span style="color:' + this.color + '">\u25cf</span> ' + this.series.name + ': <b>' + this.y + '</b><br/>';
                }
              }
            });
          }
          if (self.bUseDoesNotMeet) {
            series.push({
              name: 'Does Not Meet',
              color: '#BF453D',
              data: doesnotmeetData,
              lineWidth: 3,
              shadow: false,
              zIndex: 1,
              dashStyle: 'ShortDash',
              marker: { enabled: false },
              tooltip: {
                headerFormat: '<span style="font-size: 10px">Test {point.key}</span><br/>',
                pointFormatter: function () {
                  for (var i = 0; i < self.VScale.length; i++) {
                    if (self.VScale[i].FieldSpecificId == this.y) {
                      return '<span style="color:' + this.color + '">\u25cf</span> ' + this.series.name + ': <b>' + self.VScale[i].FieldValue + '</b><br/>';
                    }
                  }
                  return '<span style="color:' + this.color + '">\u25cf</span> ' + this.series.name + ': <b>' + this.y + '</b><br/>';
                }
              }
            });
          }
          highchartsNgConfig = {
            options: {
              credits: { enabled: false },
              chart: { type: 'line' }
            },
            series: series,
            title: { text: self.assessmentName + ' ' + self.fieldToRetrieve },
            loading: false,
            xAxis: {
              currentMin: 1,
              currentMax: currentMax,
              title: { text: 'Test Number' },
              plotLines: plotlines,
              allowDecimals: false
            },
            yAxis: yAxis,
            useHighStocks: false,
            size: { height: 600 },
            func: function (chart) {
            }
          };
          self.chartConfig = highchartsNgConfig;
        };
        self.getBackgroundClass = function (studentFieldScore, tddId) {
          var self = this;
          var bgClass = '';
          bgClass = self.getIntColor(studentFieldScore, tddId);
          // see if this is an intervention date
          bgClass += ' ' + self.getTddClass(tddId);
          return bgClass;
        };
        self.formatDate = function (inDate) {
          if (inDate != null) {
            return moment(inDate).format('MMM YYYY');
          } else {
            return 'N/A';
          }
        };
        self.formatDate2 = function (inDate) {
          if (inDate != null) {
            return moment(inDate).format('DD-MMM-YYYY');
          } else {
            return 'N/A';
          }
        };
        self.getDisplayValue = function (id) {
          var self = this;
          var lookupList = self.VScale;
          if (!self.status.isLookup) {
            return id;
          }
          for (var i = 0; i < lookupList.length; i++) {
            if (lookupList[i].FieldSpecificId == id) {
              return lookupList[i].FieldValue;
            }
          }
          return 'N/A';
        };
        /* Private Functions */
        self.getTddClass = function (tddId) {
          var self = this;
          for (var i = 0; i < self.Interventions.length; i++) {
            if (tddId === self.Interventions[i].StartTDDID || tddId === self.Interventions[i].EndTDDID) {
              return 'interventionCell';
            }
          }
          return '';
        };
        self.getIntColor = function (studentFieldScore, tddId) {
          var self = this;
          // TODO: dont forget to add a check for NULL Benchmarks, what happens?
          for (var i = 0; i < self.BenchmarkDates.length; i++) {
            if (self.BenchmarkDates[i].TestDueDateID === tddId) {
              if (studentFieldScore.FieldValueID != null) {
                // not defined yet
                //if (studentFieldScore.IntValue === $scope.Benchmarks[i].MaxScore) {
                //	return 'obsGreen';
                //}
                if (studentFieldScore.FieldValueID >= self.BenchmarkDates[i].Exceeds && self.BenchmarkDates[i].Exceeds != null) {
                  return 'obsBlue';
                } else if (studentFieldScore.FieldValueID >= self.BenchmarkDates[i].Meets && self.BenchmarkDates[i].Meets != null) {
                  return '';
                } else if (studentFieldScore.FieldValueID >= self.BenchmarkDates[i].Approaches && self.BenchmarkDates[i].Approaches != null) {
                  return 'obsYellow';
                } else if (studentFieldScore.FieldValueID < self.BenchmarkDates[i].Approaches && self.BenchmarkDates[i].Approaches != null) {
                  return 'obsRed';
                }
              }
            }
          }
          return '';
        }  /* End Private Functions */;
      };
      return NSAssesssmentLineGraphManager;
    }
  ]).factory('NSAssesssmentIGLineGraphManager', [
    '$http',
    'webApiBaseUrl',
    function ($http, webApiBaseUrl) {
      var NSAssesssmentIGLineGraphManager = function () {
        var self = this;
        self.LoadData = function (assessmentId, fieldToRetrieve, lookupFieldName, fieldType, studentId, fieldDisplayName, interventionGroupId, assessmentName, interventions, schoolStartYear) {
          self.bUseExceeds = false;
          self.bUseMeets = false;
          self.bUseApproaches = false;
          self.bUseDoesNotMeet = false;
          self.assessmentName = assessmentName;
          self.fieldToRetrieve = fieldDisplayName;
          self.status = {};
          self.status.isLookup = lookupFieldName != null && lookupFieldName != '';
          self.status.isDecimalField = false;
          // TODO: check field type
          var returnObject = {
              AssessmentId: assessmentId,
              FieldToRetrieve: fieldToRetrieve,
              LookupFieldName: lookupFieldName,
              IsLookupColumn: self.status.isLookup,
              StudentId: studentId,
              InterventionGroupId: interventionGroupId,
              SchoolStartYear: schoolStartYear
            };
          var url = webApiBaseUrl + '/api/lineGraph/GetStudentIGLineGraph';
          var returnData = $http.post(url, returnObject);
          self.Results = [];
          self.BenchmarkDates = [];
          self.Interventions = interventions == null ? [] : interventions;
          self.VScale = [];
          //self.Fields = {};
          self.chartConfig = {};
          return returnData.then(function (response) {
            angular.extend(self, response.data);
            if (self.Results === null)
              self.Results = [];
            if (self.Fields === null)
              self.Fields = [];
            if (self.BenchmarkDates === null)
              self.BenchmarkDates = [];
            //if (self.Interventions === null) self.Interventions = [];
            if (self.VScale === null)
              self.VScale = [];
            //if (self.Fields === null) {
            //    self.Fields = {};
            //}
            //else {
            //    self.Fields = self.Fields.Assessments[0].Fields;
            //}
            self.postDataLoadSetup();
          });
        };
        self.LoadAssessmentFields = function () {
          var url = webApiBaseUrl + '/api/benchmark/GetInterventionAssessmentsAndFields';
          var promise = $http.get(url);
          return promise.then(function (response) {
            self.assessments = self.flatten(response.data.Assessments);
          });
        };
        self.flatten = function (data) {
          var out = [];
          angular.forEach(data, function (d) {
            angular.forEach(d.Fields, function (v) {
              out.push({
                AssessmentName: d.AssessmentName,
                DisplayLabel: v.DisplayLabel,
                FieldName: v.DatabaseColumn,
                LookupFieldName: v.LookupFieldName,
                AssessmentId: d.Id,
                FieldType: v.FieldType,
                RangeHigh: v.RangeHigh,
                RangeLow: v.RangeLow,
                DisplayInLineGraphs: v.DisplayInLineGraphs
              });
            });
          });
          return out;
        };
        self.postDataLoadSetup = function () {
          var self = this;
          var currentMax = 1;
          var data = [];
          var doesnotmeetData = [];
          var approachesData = [];
          var meetsData = [];
          var exceedsData = [];
          var plotlines = [];
          var loopCounter = 0;
          var plotLinesLoopCounter = 0;
          var highchartsNgConfig = {};
          // determine which benchmarks we want to use
          for (var d = 0; d < self.BenchmarkDates.length; d++) {
            if (self.BenchmarkDates[d].Exceeds !== null || self.bUseExceeds == true) {
              self.bUseExceeds = true;
            }
            if (self.BenchmarkDates[d].Meets !== null || self.bUseMeets == true) {
              self.bUseMeets = true;
            }
            if (self.BenchmarkDates[d].Approaches !== null || self.bUseApproaches == true) {
              self.bUseApproaches = true;
            }
            if (self.BenchmarkDates[d].DoesNotMeet !== null || self.bUseDoesNotMeet == true) {
              self.bUseDoesNotMeet = true;
            }
          }
          // set up fields
          for (var r = 0; r < self.Fields.length; r++) {
            self.Fields[r].BenchmarkDates = angular.copy(self.BenchmarkDates);
            for (var i = 0; i < self.Fields[r].BenchmarkDates.length; i++) {
              self.Fields[r].BenchmarkDates[i].Result = {};
              self.Fields[r].BenchmarkDates[i].Result.StringValue = 'N/A';
              // magic number
              for (var j = 0; j < self.Results.length; j++) {
                if (self.Fields[r].BenchmarkDates[i].TestNumber == self.Results[j].TestNumber) {
                  // attach result to the proper benchmark date
                  //self.Fields[r].BenchmarkDates[i].Result = self.Results[j];
                  // now get the proper result for this field
                  for (var p = 0; p < self.Results[j].FieldResults.length; p++) {
                    if (self.Results[j].FieldResults[p].DbColumn === self.Fields[r].DatabaseColumn) {
                      self.Fields[r].BenchmarkDates[i].Result.StringValue = self.Results[j].FieldResults[p].StringValue;
                      break;
                    }
                  }
                }
              }
            }
          }
          // set up series
          for (var i = 0; i < self.BenchmarkDates.length; i++) {
            self.BenchmarkDates[i].Result = {};
            self.BenchmarkDates[i].Result.FieldValueID = null;
            // magic number
            self.BenchmarkDates[i].Result.FieldDisplayValue = 'N/A';
            // magic number
            currentMax = self.BenchmarkDates[i].TestNumber;
            // set benchmark data series
            doesnotmeetData[i] = [
              i + 1,
              self.BenchmarkDates[i].DoesNotMeet
            ];
            approachesData[i] = [
              i + 1,
              self.BenchmarkDates[i].Approaches
            ];
            meetsData[i] = [
              i + 1,
              self.BenchmarkDates[i].Meets
            ];
            exceedsData[i] = [
              i + 1,
              self.BenchmarkDates[i].Exceeds
            ];
            for (var j = 0; j < self.Results.length; j++) {
              if (self.BenchmarkDates[i].TestNumber == self.Results[j].TestNumber) {
                // attach result to the proper benchmark date
                self.BenchmarkDates[i].Result = self.Results[j];
                // process the result, set colors, etc
                if (self.Results[j].FieldDisplayValue != null && self.Results[j].FieldDisplayValue != '') {
                  self.Results[j].x = i + 1;
                  data[loopCounter] = {
                    x: i + 1,
                    y: self.Results[j].FieldValueID,
                    name: self.Results[j].FieldDisplayValue,
                    dataLabels: {
                      enabled: true,
                      crop: false,
                      style: { fontWeight: 'bold' },
                      formatter: function () {
                        return this.point.name;
                      }
                    }
                  };
                } else {
                  self.Results[j].x = i + 1;
                  data[loopCounter] = [
                    i + 1,
                    self.Results[j].FieldValueID
                  ];
                }
                loopCounter++;
              }
            }
          }
          var yAxis = null;
          if (self.status.isLookup) {
            yAxis = {
              allowDecimals: false,
              min: 0,
              title: { text: self.fieldToRetrieve },
              labels: {
                formatter: function () {
                  var value = null;
                  for (var i = 0; i < self.VScale.length; i++) {
                    if (self.VScale[i].FieldSpecificId == this.value) {
                      return self.VScale[i].FieldValue;
                    }
                  }
                  return null;
                }
              }
            };
          } else {
            yAxis = {
              allowDecimals: false,
              min: 0,
              title: { text: self.fieldToRetrieve }
            };
          }
          // TODO:  dynamically push series depending on if they have values or not
          var series = [];
          series.push({
            name: self.fieldToRetrieve,
            color: '#2244BB',
            data: data,
            lineWidth: 3,
            shadow: {
              offsetX: 3,
              offsetY: 2,
              opacity: 0.12,
              width: 6
            },
            zIndex: 100,
            marker: {
              lineWidth: 2,
              lineColor: null
            },
            tooltip: {
              headerFormat: '',
              pointFormatter: function (point) {
                //getDisplayValue(date.Result.FieldValueID)
                var responseString = '';
                // header score ROW
                for (var j = 0; j < self.BenchmarkDates.length; j++) {
                  var currentBenchmarkDate = self.BenchmarkDates[j];
                  // if we are on the right benchmark date
                  if (j + 1 == this.x) {
                    responseString = '<span style="font-weight:bold;color:' + this.color + '">' + self.formatDate(currentBenchmarkDate.Result.TestDueDate) + '</span><br/>';
                    ;
                    responseString += '<span style="color:' + this.color + '">\u25cf</span> ' + 'Score: ';
                    responseString += '<b>' + self.getDisplayValue(currentBenchmarkDate.Result.FieldValueID) + '</b><br/>';
                    break;
                  }
                }
                for (var i = 0; i < self.Fields.length; i++) {
                  var field = self.Fields[i];
                  // don't display text fields
                  if (field.FieldType == 'Textarea') {
                    continue;
                  }
                  responseString += field.DisplayLabel + ' : ';
                  for (var j = 0; j < field.BenchmarkDates.length; j++) {
                    var currentBenchmarkDate = field.BenchmarkDates[j];
                    // if we are on the right benchmark date
                    if (j + 1 == this.x) {
                      if (currentBenchmarkDate.Result.StringValue == null) {
                        responseString += '<br/>';
                      } else {
                        responseString += '<b>' + (field.AltDisplayLabel ? field.AltDisplayLabel : '') + currentBenchmarkDate.Result.StringValue + '</b><br/>';
                      }
                      break;
                    }
                  }
                }
                return responseString;
              }
            }
          });
          if (self.bUseExceeds) {
            series.push({
              name: 'Exceeds',
              color: '#4697ce',
              data: exceedsData,
              lineWidth: 3,
              shadow: false,
              zIndex: 2,
              dashStyle: 'ShortDash',
              marker: { enabled: false },
              tooltip: {
                headerFormat: '<span style="font-size: 10px">Test {point.key}</span><br/>',
                pointFormatter: function () {
                  for (var i = 0; i < self.VScale.length; i++) {
                    if (self.VScale[i].FieldSpecificId == this.y) {
                      return '<span style="color:' + this.color + '">\u25cf</span> ' + this.series.name + ': <b>' + self.VScale[i].FieldValue + '</b><br/>';
                    }
                  }
                  return '<span style="color:' + this.color + '">\u25cf</span> ' + this.series.name + ': <b>' + this.y + '</b><br/>';
                }
              }
            });
          }
          if (self.bUseMeets) {
            series.push({
              name: 'Meets',
              color: '#ABF09C',
              data: meetsData,
              lineWidth: 3,
              shadow: false,
              zIndex: 2,
              dashStyle: 'ShortDash',
              marker: { enabled: false },
              tooltip: {
                headerFormat: '<span style="font-size: 10px">Test {point.key}</span><br/>',
                pointFormatter: function () {
                  for (var i = 0; i < self.VScale.length; i++) {
                    if (self.VScale[i].FieldSpecificId == this.y) {
                      return '<span style="color:' + this.color + '">\u25cf</span> ' + this.series.name + ': <b>' + self.VScale[i].FieldValue + '</b><br/>';
                    }
                  }
                  return '<span style="color:' + this.color + '">\u25cf</span> ' + this.series.name + ': <b>' + this.y + '</b><br/>';
                }
              }
            });
          }
          if (self.bUseApproaches) {
            series.push({
              name: 'Approaches',
              color: '#E4D354',
              data: approachesData,
              lineWidth: 3,
              shadow: false,
              zIndex: 3,
              dashStyle: 'ShortDash',
              marker: { enabled: false },
              tooltip: {
                headerFormat: '<span style="font-size: 10px">Test {point.key}</span><br/>',
                pointFormatter: function () {
                  for (var i = 0; i < self.VScale.length; i++) {
                    if (self.VScale[i].FieldSpecificId == this.y) {
                      return '<span style="color:' + this.color + '">\u25cf</span> ' + this.series.name + ': <b>' + self.VScale[i].FieldValue + '</b><br/>';
                    }
                  }
                  return '<span style="color:' + this.color + '">\u25cf</span> ' + this.series.name + ': <b>' + this.y + '</b><br/>';
                }
              }
            });
          }
          if (self.bUseDoesNotMeet) {
            series.push({
              name: 'Does Not Meet',
              color: '#BF453D',
              data: doesnotmeetData,
              lineWidth: 3,
              shadow: false,
              zIndex: 1,
              dashStyle: 'ShortDash',
              marker: { enabled: false },
              tooltip: {
                headerFormat: '<span style="font-size: 10px">Test {point.key}</span><br/>',
                pointFormatter: function () {
                  for (var i = 0; i < self.VScale.length; i++) {
                    if (self.VScale[i].FieldSpecificId == this.y) {
                      return '<span style="color:' + this.color + '">\u25cf</span> ' + this.series.name + ': <b>' + self.VScale[i].FieldValue + '</b><br/>';
                    }
                  }
                  return '<span style="color:' + this.color + '">\u25cf</span> ' + this.series.name + ': <b>' + this.y + '</b><br/>';
                }
              }
            });
          }
          highchartsNgConfig = {
            options: {
              credits: { enabled: false },
              chart: { type: 'line' }
            },
            series: series,
            title: { text: self.assessmentName + ' ' + self.fieldToRetrieve },
            loading: false,
            xAxis: {
              currentMin: 1,
              currentMax: currentMax,
              title: { text: 'Test Number' },
              allowDecimals: false
            },
            yAxis: yAxis,
            useHighStocks: false,
            size: { height: 600 },
            func: function (chart) {
            }
          };
          self.chartConfig = highchartsNgConfig;
        };
        self.getBackgroundClass = function (studentFieldScore, testNumber) {
          var self = this;
          var bgClass = '';
          bgClass = self.getIntColor(studentFieldScore, testNumber);
          // see if this is an intervention date
          //bgClass += ' ' + self.getTddClass(tddId);
          return bgClass;
        };
        self.formatDate = function (inDate) {
          if (inDate != null) {
            return moment(inDate).format('DD-MMM-YY');
          } else {
            return 'N/A';
          }
        };
        self.formatDate2 = function (inDate) {
          if (inDate != null) {
            return moment(inDate).format('DD-MMM-YYYY');
          } else {
            return 'N/A';
          }
        };
        self.getDisplayValue = function (id) {
          var self = this;
          var lookupList = self.VScale;
          if (!self.status.isLookup) {
            return id;
          }
          for (var i = 0; i < lookupList.length; i++) {
            if (lookupList[i].FieldSpecificId == id) {
              return lookupList[i].FieldValue;
            }
          }
          return 'N/A';
        };
        /* Private Functions */
        //this.getTddClass = function (tddId) {
        //    var self = this;
        //    for (var i = 0; i < self.Interventions.length; i++) {
        //        if (tddId === self.Interventions[i].StartTDDID ||
        //            tddId === self.Interventions[i].EndTDDID) {
        //            return 'interventionCell';
        //        }
        //    }
        //    return '';
        //}
        self.getIntColor = function (studentFieldScore, tddId) {
          var self = this;
          // TODO: dont forget to add a check for NULL Benchmarks, what happens?
          for (var i = 0; i < self.BenchmarkDates.length; i++) {
            if (self.BenchmarkDates[i].TestNumber === tddId) {
              if (studentFieldScore.FieldValueID != null) {
                // not defined yet
                //if (studentFieldScore.IntValue === $scope.Benchmarks[i].MaxScore) {
                //	return 'obsGreen';
                //}
                if (studentFieldScore.FieldValueID >= self.BenchmarkDates[i].Exceeds && self.BenchmarkDates[i].Exceeds != null) {
                  return 'obsBlue';
                } else if (studentFieldScore.FieldValueID >= self.BenchmarkDates[i].Meets && self.BenchmarkDates[i].Meets != null) {
                  return '';
                } else if (studentFieldScore.FieldValueID >= self.BenchmarkDates[i].Approaches && self.BenchmarkDates[i].Approaches != null) {
                  return 'obsYellow';
                } else if (studentFieldScore.FieldValueID < self.BenchmarkDates[i].Approaches && self.BenchmarkDates[i].Approaches != null) {
                  return 'obsRed';
                }
              }
            }
          }
          return '';
        }  /* End Private Functions */;
      };
      return NSAssesssmentIGLineGraphManager;
    }
  ]);
  /* Movies List Controller  */
  LineGraphController.$inject = [
    '$scope',
    'InterventionGroup',
    '$q',
    '$http',
    'pinesNotifications',
    '$location',
    '$routeParams',
    'NSAssesssmentLineGraphManager',
    'nsFilterOptionsService',
    'NSStudentInterventionManager',
    'spinnerService'
  ];
  function LineGraphController($scope, InterventionGroup, $q, $http, pinesNotifications, $location, $routeParams, NSAssesssmentLineGraphManager, nsFilterOptionsService, NSStudentInterventionManager, spinnerService) {
    var vm = this;
    vm.dataManager = new NSAssesssmentLineGraphManager();
    vm.studentDataManager = new NSStudentInterventionManager();
    vm.filterOptions = nsFilterOptionsService.options;
    vm.settings = { selectedAssessmentField: null };
    vm.dataManager.LoadAssessmentFields();
    // watch selectedField
    $scope.$watch('vm.settings.selectedAssessmentField', function (newVal, oldVal) {
      if (newVal !== oldVal && newVal != null) {
        if (vm.filterOptions.selectedSectionStudent != null && angular.isDefined(vm.settings.selectedAssessmentField.FieldName)) {
          spinnerService.show('tableSpinner');
          vm.dataManager.LoadData(vm.settings.selectedAssessmentField.AssessmentId, vm.settings.selectedAssessmentField.FieldName, vm.settings.selectedAssessmentField.LookupFieldName, vm.settings.selectedAssessmentField.FieldType, vm.filterOptions.selectedSectionStudent.id, vm.settings.selectedAssessmentField.DisplayLabel, vm.filterOptions.selectedSection.id, vm.settings.selectedAssessmentField.AssessmentName, vm.studentDataManager.Interventions).finally(function (response) {
            spinnerService.hide('tableSpinner');
          });
        }
      }
    }, true);
    $scope.$watch('vm.filterOptions.selectedSectionStudent', function (newVal, oldVal) {
      if (newVal !== oldVal && newVal != null) {
        if (vm.filterOptions.selectedSectionStudent != null) {
          vm.studentDataManager.LoadData(vm.filterOptions.selectedSectionStudent.id).then(function (response) {
            if (vm.settings.selectedAssessmentField != null && angular.isDefined(vm.settings.selectedAssessmentField.FieldName)) {
              spinnerService.show('tableSpinner');
              vm.dataManager.LoadData(vm.settings.selectedAssessmentField.AssessmentId, vm.settings.selectedAssessmentField.FieldName, vm.settings.selectedAssessmentField.LookupFieldName, vm.settings.selectedAssessmentField.FieldType, vm.filterOptions.selectedSectionStudent.id, vm.settings.selectedAssessmentField.DisplayLabel, vm.filterOptions.selectedSection.id, vm.settings.selectedAssessmentField.AssessmentName, vm.studentDataManager.Interventions).finally(function (response) {
                spinnerService.hide('tableSpinner');
              });
            }
          });
        }
      }
    }, true);
  }
  LineGraphIGController.$inject = [
    '$scope',
    'InterventionGroup',
    '$q',
    '$http',
    'pinesNotifications',
    '$location',
    '$routeParams',
    'NSAssesssmentIGLineGraphManager',
    'nsFilterOptionsService',
    'NSStudentInterventionManager'
  ];
  function LineGraphIGController($scope, InterventionGroup, $q, $http, pinesNotifications, $location, $routeParams, NSAssesssmentIGLineGraphManager, nsFilterOptionsService, NSStudentInterventionManager) {
    var vm = this;
    vm.dataManager = new NSAssesssmentIGLineGraphManager();
    vm.studentDataManager = new NSStudentInterventionManager();
    vm.filterOptions = nsFilterOptionsService.options;
    vm.settings = { selectedAssessmentField: {} };
    vm.dataManager.LoadAssessmentFields();
    // watch selectedField
    $scope.$watch('vm.settings.selectedAssessmentField', function (newVal, oldVal) {
      if (newVal !== oldVal) {
        if (vm.filterOptions.selectedInterventionStudent != null && angular.isDefined(vm.settings.selectedAssessmentField.FieldName)) {
          vm.dataManager.LoadData(vm.settings.selectedAssessmentField.AssessmentId, vm.settings.selectedAssessmentField.FieldName, vm.settings.selectedAssessmentField.LookupFieldName, vm.settings.selectedAssessmentField.FieldType, vm.filterOptions.selectedInterventionStudent.id, vm.settings.selectedAssessmentField.DisplayLabel, vm.filterOptions.selectedInterventionGroup.id, vm.settings.selectedAssessmentField.AssessmentName, vm.studentDataManager.Interventions, vm.filterOptions.selectedSchoolYear.id);
        }
      }
    }, true);
    $scope.$watch('vm.filterOptions.selectedInterventionStudent', function (newVal, oldVal) {
      if (newVal !== oldVal) {
        if (vm.filterOptions.selectedInterventionStudent != null) {
          vm.studentDataManager.LoadData(vm.filterOptions.selectedInterventionStudent.id).then(function (response) {
            if (angular.isDefined(vm.settings.selectedAssessmentField.FieldName)) {
              vm.dataManager.LoadData(vm.settings.selectedAssessmentField.AssessmentId, vm.settings.selectedAssessmentField.FieldName, vm.settings.selectedAssessmentField.LookupFieldName, vm.settings.selectedAssessmentField.FieldType, vm.filterOptions.selectedInterventionStudent.id, vm.settings.selectedAssessmentField.DisplayLabel, vm.filterOptions.selectedInterventionGroup.id, vm.settings.selectedAssessmentField.AssessmentName, vm.studentDataManager.Interventions, vm.filterOptions.selectedSchoolYear.id);
            }
          });
        }
      }
    }, true);
  }
}());
(function () {
  'use strict';
  angular.module('benchmarksModule', []).controller('SystemBenchmarksController', SystemBenchmarksController).factory('NSBenchmarksManager', [
    '$http',
    'webApiBaseUrl',
    function ($http, webApiBaseUrl) {
      var NSBenchmarksManager = function () {
        var self = this;
        self.LoadData = function (assessmentId, fieldName, lookupFieldName) {
          var paramObj = {
              AssessmentId: assessmentId,
              FieldName: fieldName,
              LookupFieldName: lookupFieldName
            };
          var url = webApiBaseUrl + '/api/benchmark/GetSystemBenchmarks';
          var benchmarksPromise = $http.post(url, paramObj);
          return benchmarksPromise.then(function (response) {
            self.benchmarks = response.data.Benchmarks;
          });
        };
        self.LoadAssessmentFields = function () {
          var url = webApiBaseUrl + '/api/benchmark/GetAssessmentsAndFields';
          var promise = $http.get(url);
          return promise.then(function (response) {
            self.assessments = self.flatten(response.data.Assessments);
          });
        };
        self.flatten = function (data) {
          var out = [];
          angular.forEach(data, function (d) {
            angular.forEach(d.Fields, function (v) {
              out.push({
                AssessmentName: d.AssessmentName,
                DisplayLabel: v.DisplayLabel,
                FieldName: v.DatabaseColumn,
                LookupFieldName: v.LookupFieldName,
                AssessmentId: d.Id,
                FieldType: v.FieldType,
                RangeHigh: v.RangeHigh,
                RangeLow: v.RangeLow
              });
            });
          });
          return out;
        };
        self.SaveBenchmark = function (benchmarkRecord) {
          var paramObj = { Benchmark: benchmarkRecord };
          var url = webApiBaseUrl + '/api/benchmark/SaveSystemBenchmark';
          var promise = $http.post(url, paramObj);
          // temporary
          return promise.then(function (response) {
          });
        };
        self.DeleteBenchmark = function (benchmarkRecord) {
          var paramObj = { Benchmark: benchmarkRecord };
          var url = webApiBaseUrl + '/api/benchmark/DeleteSystemBenchmark';
          var promise = $http.post(url, paramObj);
          // temporary
          return promise.then(function (response) {
          });
        }  //this.changeFieldStatus = function (assessmentId, fieldId, status, hideFieldFrom, successCallback, failureCallback) {
           //    var returnObject = { AssessmentId: assessmentId, FieldId: fieldId, HiddenStatus: status, HideFieldFrom: hideFieldFrom };
           //    var saveResponse = $http.post(webApiBaseUrl + "/api/personalsettings/UpdateFieldForUser", returnObject);
           //    saveResponse.then(successCallback, failureCallback);
           //};
;
      };
      return NSBenchmarksManager;
    }
  ]).directive('nsBenchmarkField', [
    'Assessment',
    '$routeParams',
    '$compile',
    '$templateCache',
    '$http',
    'nsLookupFieldService',
    function (Assessment, $routeParams, $compile, $templateCache, $http, nsLookupFieldService) {
      var getTemplate = function (field, mode) {
        var type = field.FieldType;
        var template = '';
        if (type === '' || type === null) {
          type = 'textfield';
        }
        var templateName = 'templates/benchmark-' + type + '.html';
        template = $templateCache.get(templateName.toLocaleLowerCase());
        return template;
      };
      return {
        restrict: 'E',
        scope: {
          result: '=',
          eForm: '=',
          field: '='
        },
        link: function (scope, element, attr) {
          // get our own lookupFields
          scope.lookupValues = [];
          scope.lookupFieldsArray = nsLookupFieldService.LookupFieldsArray;
          for (var i = 0; i < scope.lookupFieldsArray.length; i++) {
            if (scope.lookupFieldsArray[i].LookupColumnName === scope.field.LookupFieldName) {
              scope.lookupValues = scope.lookupFieldsArray[i].LookupFields;
              break;
            }
          }
          var templateText = getTemplate(scope.field, scope.mode);
          var dataToAppend = templateText;
          element.html(dataToAppend);
          $compile(element.contents())(scope);
        }
      };
    }
  ]);
  SystemBenchmarksController.$inject = [
    '$scope',
    '$routeParams',
    '$location',
    'nsSectionService',
    'nsFilterOptionsService',
    'nsPinesService',
    'nsSelect2RemoteOptions',
    'NSBenchmarksManager',
    '$bootbox'
  ];
  function SystemBenchmarksController($scope, $routeParams, $location, nsSectionService, nsFilterOptionsService, nsPinesService, nsSelect2RemoteOptions, NSBenchmarksManager, $bootbox) {
    $scope.dataMgr = new NSBenchmarksManager();
    //$scope.assessments = $scope.fieldsManager.Assessments;
    $scope.errors = [];
    $scope.$on('NSHTTPError', function (event, data) {
      $scope.errors.push({
        type: 'danger',
        msg: data
      });
      $('html, body').animate({ scrollTop: 0 }, 'fast');
    });
    $scope.settings = {};
    $scope.saveAssessmentData = function (result) {
      $scope.dataMgr.SaveBenchmark(result).then(function (response) {
        nsPinesService.dataSavedSuccessfully();
        //$scope.dataMgr.LoadData(1, 'FPValueId', 'FPScale');
        $scope.dataMgr.LoadData($scope.settings.selectedAssessmentField.AssessmentId, $scope.settings.selectedAssessmentField.FieldName, $scope.settings.selectedAssessmentField.LookupFieldName);
      });
    };
    $scope.deleteAssessmentData = function (result) {
      $bootbox.confirm('Are you sure you want to delete this benchmark record?', function (response) {
        if (response) {
          $scope.dataMgr.DeleteBenchmark(result).then(function (response) {
            nsPinesService.dataDeletedSuccessfully();
            $scope.dataMgr.LoadData($scope.settings.selectedAssessmentField.AssessmentId, $scope.settings.selectedAssessmentField.FieldName, $scope.settings.selectedAssessmentField.LookupFieldName);
          });
        }
      });
    };
    $scope.$watch('settings.selectedAssessmentField', function (newValue, oldValue) {
      if (!angular.equals(newValue, oldValue)) {
        $scope.dataMgr.LoadData($scope.settings.selectedAssessmentField.AssessmentId, $scope.settings.selectedAssessmentField.FieldName, $scope.settings.selectedAssessmentField.LookupFieldName);
      }
    }, true);
    $scope.dataMgr.LoadAssessmentFields();  //$scope.dataMgr.LoadData(1, 'FPValueId', 'FPScale');
                                            // initial load
                                            //$scope.getCoTeacherRemoteOptions = nsSelect2RemoteOptions.CoTeacherRemoteOptions;
                                            //$scope.getStaffGroupRemoteOptions = nsSelect2RemoteOptions.StaffGroupRemoteOptions;
  }
}());
(function () {
  'use strict';
  angular.module('stackedBarGraphModule', []).controller('StackedBarGraphComparisonController', StackedBarGraphComparisonController).controller('StackedBarGraphGroupsController', StackedBarGraphGroupsController).directive('nsSummaryFieldHeaders', [
    '$compile',
    'NSSummarySortManager',
    function ($compile, NSSummarySortManager) {
      function Controller($scope, $element) {
        var outputHtml = '';
        var sortText = '';
        $scope.manualSortHeaders = {};
        $scope.manualSortHeaders.firstNameHeaderClass = 'fa';
        $scope.manualSortHeaders.lastNameHeaderClass = 'fa';
        $scope.headerClassArray = [];
        for (var i = 0; i < $scope.tdds.length; i++) {
          $scope.headerClassArray[i] = [];
        }
        $scope.sortMgr = new NSSummarySortManager();
        $scope.sort = function (tddIndex, fieldIndex) {
          $scope.sortMgr.sort(tddIndex, fieldIndex);
        };
        // display the headers once for each TDD
        for (var i = 0; i < $scope.tdds.length; i++) {
          for (var j = 0; j < $scope.fields.length; j++) {
            if ($scope.sortable) {
              sortText = '<div style="cursor: pointer;" ng-click="sort(' + i + ',' + j + ')">' + $scope.fields[j].DisplayLabel + ' <i class="{{headerClassArray[' + i + '][' + j + ']}}"></i></div>';
            } else {
              sortText = $scope.fields[j].DisplayLabel;
            }
            if (j == 0) {
              outputHtml += '<th class="text-center" style="border-left:3px solid #e9ecf0;border-right:1px solid #e9ecf0;">' + sortText + '</th>';
            } else if (j == $scope.fields.length - 1) {
              outputHtml += '<th class="text-center" style="border-right:3px solid #e9ecf0;border-left:1px solid #e9ecf0;">' + sortText + '</th>';
            } else {
              outputHtml += '<th class="text-center" style="border-left:1px solid #e9ecf0;border-right:1px solid #e9ecf0;">' + sortText + '</th>';
            }
          }
        }
        $element.html(outputHtml);
        $compile($element.contents())($scope);
        $scope.sortMgr.initialize($scope.manualSortHeaders, $scope.sortArray, $scope.headerClassArray, $scope.fields);
      }
      return {
        restrict: 'AE',
        scope: {
          tdds: '=',
          fields: '=',
          dataRows: '=',
          sortable: '=',
          sortArray: '='
        },
        controller: Controller
      };
    }
  ]).factory('NSSummarySortManager', [
    '$http',
    function ($http) {
      var NSSummarySortManager = function () {
        var self = this;
        self.manualSortHeaders = {};
        self.sortArray = [];
        self.headerClassArray = [];
        self.fieldResultName = '';
        self.fields = [];
        self.initialize = function (manualSortHeaders, sortArray, headerClassArray, fields) {
          self.manualSortHeaders = manualSortHeaders;
          self.sortArray = sortArray;
          self.headerClassArray = headerClassArray;
          self.fields = fields;
        };
        self.sort = function (tddIndex, fieldIndex) {
          var column = '';
          var columnIndex = -1;
          // if this is not a first or lastname column
          if (!isNaN(parseInt(fieldIndex))) {
            switch (self.fields[fieldIndex].FieldType) {
            case 'Textfield':
              column = 'ResultsByTDD[' + tddIndex + '].FieldResults[' + fieldIndex + '].StringValue';
              break;
            case 'DecimalRange':
              column = 'ResultsByTDD[' + tddIndex + '].FieldResults[' + fieldIndex + '].DecimalValue';
              break;
            case 'DropdownRange':
              column = 'ResultsByTDD[' + tddIndex + '].FieldResults[' + fieldIndex + '].IntValue';
              break;
            case 'DropdownFromDB':
              column = 'ResultsByTDD[' + tddIndex + '].FieldResults[' + fieldIndex + '].IntValue';
              break;
            case 'CalculatedFieldDbOnly':
              column = 'ResultsByTDD[' + tddIndex + '].FieldResults[' + fieldIndex + '].StringValue';
              break;
            case 'CalculatedFieldDbBacked':
              column = 'ResultsByTDD[' + tddIndex + '].FieldResults[' + fieldIndex + '].IntValue';
              break;
            case 'CalculatedFieldDbBackedString':
              column = 'ResultsByTDD[' + tddIndex + '].FieldResults[' + fieldIndex + '].StringValue';
              break;
            case 'CalculatedFieldClientOnly':
              column = 'ResultsByTDD[' + tddIndex + '].FieldResults[' + fieldIndex + '].StringValue';
              //shouldnt even be used in sorting
              break;
            default:
              column = 'ResultsByTDD[' + tddIndex + '].FieldResults[' + fieldIndex + '].IntValue';
              break;
            }
          }
          var bFound = false;
          for (var j = 0; j < self.sortArray.length; j++) {
            // if it is already on the list, reverse the sort
            if (self.sortArray[j].indexOf(column) >= 0) {
              bFound = true;
              // is it already negative? if so, remove it
              if (self.sortArray[j].indexOf('-') === 0) {
                if (tddIndex > -1) {
                  self.headerClassArray[tddIndex][fieldIndex] = 'fa';
                } else if (column === 'FirstName') {
                  self.manualSortHeaders.firstNameHeaderClass = 'fa';
                } else if (column === 'LastName') {
                  self.manualSortHeaders.lastNameHeaderClass = 'fa';
                }
                self.sortArray.splice(j, 1);
              } else {
                if (tddIndex > -1) {
                  self.headerClassArray[tddIndex][fieldIndex] = 'fa fa-chevron-down';
                } else if (column === 'FirstName') {
                  self.manualSortHeaders.firstNameHeaderClass = 'fa fa-chevron-down';
                } else if (column === 'LastName') {
                  self.manualSortHeaders.lastNameHeaderClass = 'fa fa-chevron-down';
                }
                self.sortArray[j] = '-' + self.sortArray[j];
              }
              break;
            }
          }
          if (!bFound) {
            self.sortArray.push(column);
            if (tddIndex > -1) {
              self.headerClassArray[tddIndex][fieldIndex] = 'fa fa-chevron-up';
            } else if (column === 'FirstName') {
              self.manualSortHeaders.firstNameHeaderClass = 'fa fa-chevron-up';
            } else if (column === 'LastName') {
              self.manualSortHeaders.lastNameHeaderClass = 'fa fa-chevron-up';
            }
          }
        };
      };
      return NSSummarySortManager;
    }
  ]).directive('nsSummaryField', [
    '$compile',
    '$filter',
    function ($compile, $filter) {
      return {
        restrict: 'AE',
        scope: true,
        link: function (scope, element, attr) {
          var outputHtml = '';
          var removeit = scope.$watch(function () {
              if (attr.tdds && attr.fields && attr.studentResult) {
                return 'loaded';
              } else {
                return '';
              }
            }, function (newVal) {
              if (newVal == 'loaded') {
                scope.tdds = scope.$eval(attr.tdds);
                scope.fields = scope.$eval(attr.fields);
                scope.studentResult = scope.$eval(attr.studentResult);
                // display the headers once for each TDD
                for (var i = 0; i < scope.tdds.length; i++) {
                  var currentTdd = scope.tdds[i];
                  for (var j = 0; j < scope.fields.length; j++) {
                    var currentField = scope.fields[j];
                    var foundTdd = false;
                    for (var k = 0; k < scope.studentResult.ResultsByTDD.length; k++) {
                      var currentResultByTDD = scope.studentResult.ResultsByTDD[k];
                      // make sure we have the right test due date
                      if (currentResultByTDD.PeriodId == currentTdd.TestLevelPeriodID) {
                        for (var v = 0; v < currentResultByTDD.FieldResults.length; v++) {
                          //var resultField = $filter('filter')(currentResultByTDD.FieldResults, { DbColumn: currentField.DatabaseColumn }, true);
                          var resultField = currentResultByTDD.FieldResults[v];
                          if (resultField.DbColumn === currentField.DatabaseColumn) {
                            foundTdd = true;
                            // now output the value for this specific field, directive ideally
                            if (j == 0) {
                              outputHtml += '<td class=\'text-center\' style=\'border-left:3px solid #e9ecf0;border-right:1px solid #e9ecf0;\'><ns-assessment-field result=\'studentResult.ResultsByTDD[' + k + '].FieldResults[' + v + ']\' all-results=\'studentResult.ResultsByTDD[' + k + '].FieldResults\' /></td>';
                            } else if (j == scope.fields.length - 1) {
                              outputHtml += '<td class=\'text-center\' style=\'border-right:3px solid #e9ecf0;border-left:1px solid #e9ecf0;\'><ns-assessment-field result=\'studentResult.ResultsByTDD[' + k + '].FieldResults[' + v + ']\' all-results=\'studentResult.ResultsByTDD[' + k + '].FieldResults\' /></td>';
                            } else {
                              outputHtml += '<td class=\'text-center\' style=\'border-left:1px solid #e9ecf0;border-right:1px solid #e9ecf0;\'><ns-assessment-field result=\'studentResult.ResultsByTDD[' + k + '].FieldResults[' + v + ']\' all-results=\'studentResult.ResultsByTDD[' + k + '].FieldResults\' /></td>';
                            }
                          }
                        }
                      }
                    }
                    // if there's no data for this tdd
                    if (!foundTdd) {
                      if (j == 0) {
                        outputHtml += '<td style=\'border-left:3px solid #e9ecf0;border-right:1px solid #e9ecf0;\'></td>';
                      } else if (j == scope.fields.length - 1) {
                        outputHtml += '<td style=\'border-right:3px solid #e9ecf0;border-left:1px solid #e9ecf0;\'></td>';
                      } else {
                        outputHtml += '<td style=\'border-left:1px solid #e9ecf0;border-right:1px solid #e9ecf0;\'></td>';
                      }  //outputHtml += "<td></td>";
                    }
                  }
                }
                var linkFn = $compile(outputHtml);
                var content = linkFn(scope);
                element.append(content);
                removeit();
              }
            }, true);  //var linkFn = $compile(outputHtml);
                       //var content = linkFn(scope);
                       //element.append(content);
                       //element.append(outputHtml);
                       //$compile(element.contents())(scope);
                       //init();
        }
      };
    }
  ]).directive('nsStackedBarGraphLegend', [
    '$routeParams',
    '$compile',
    '$templateCache',
    '$http',
    '$filter',
    function ($routeParams, $compile, $templateCache, $http, $filter) {
      return {
        scope: {
          options: '=',
          zone: '=',
          category: '=',
          summaryMode: '=',
          title: '='
        },
        restrict: 'E',
        templateUrl: 'templates/stacked-bar-graph-legend.html',
        link: function (scope, element, attr) {
          scope.schoolYear = function () {
            return scope.options.selectedSchoolYear.text;
          };
          scope.whichStack = function () {
            return scope.category;
          };
          scope.scoreGrouping = function () {
            switch (scope.zone) {
            case 1:
              return 'Exceeds Expectations';
            case 2:
              return 'Meets Expectations';
            case 3:
              return 'Approaches Expectations';
            case 4:
              return 'Does Not Meet Expectations';
            }
          };
          scope.schools = function () {
            var result = '';
            if (scope.options.selectedSchools.length > 0) {
              for (var i = 0; i < scope.options.selectedSchools.length; i++) {
                result += scope.options.selectedSchools[i].text + ', ';
              }
              result = result.substring(0, result.length - 2);
            } else {
              result = 'All Schools';
            }
            return result;
          };
          scope.grades = function () {
            var result = '';
            if (scope.options.selectedGrades.length > 0) {
              for (var i = 0; i < scope.options.selectedGrades.length; i++) {
                result += scope.options.selectedGrades[i].text + ', ';
              }
              result = result.substring(0, result.length - 2);
            } else {
              result = 'All Grades';
            }
            return result;
          };
          scope.teachers = function () {
            var result = '';
            if (scope.options.selectedTeachers.length > 0) {
              for (var i = 0; i < scope.options.selectedTeachers.length; i++) {
                result += scope.options.selectedTeachers[i].text + ', ';
              }
              result = result.substring(0, result.length - 2);
            } else {
              result = 'All Teachers';
            }
            return result;
          };
          scope.sections = function () {
            var result = '';
            if (scope.options.selectedSections.length > 0) {
              for (var i = 0; i < scope.options.selectedSections.length; i++) {
                result += scope.options.selectedSections[i].text + ', ';
              }
              result = result.substring(0, result.length - 2);
            } else {
              result = 'All Sections';
            }
            return result;
          };
          scope.interventionTypes = function () {
            var result = '';
            if (scope.options.selectedInterventionTypes.length > 0) {
              for (var i = 0; i < scope.options.selectedInterventionTypes.length; i++) {
                result += scope.options.selectedInterventionTypes[i].text + ', ';
              }
              result = result.substring(0, result.length - 2);
            } else {
              result = 'All Intervention Types';
            }
            return result;
          };
          scope.attributeValue = function (attributeName, selectedData) {
            var result = '';
            if (selectedData.length > 0) {
              for (var i = 0; i < selectedData.length; i++) {
                result += selectedData[i].text + ', ';
              }
              result = result.substring(0, result.length - 2);
            } else {
              result = 'All ' + attributeName;
            }
            return result;
          };
          scope.specialEd = function () {
            if (scope.options.selectedSpedTypes == null) {
              return 'All Students';
            } else {
              return scope.options.selectedSpedTypes.text;
            }
          };
          scope.assessmentField = function () {
            if (!angular.isDefined(scope.options.selectedAssessmentField.AssessmentName)) {
              return 'Field not yet chosen';
            } else {
              return scope.options.selectedAssessmentField.AssessmentName + ' / ' + scope.options.selectedAssessmentField.DisplayLabel;
            }
          };
        }
      };
    }
  ]).directive('nsStackedBarGraphCustomGroupOptions', [
    '$routeParams',
    '$compile',
    '$templateCache',
    '$http',
    '$filter',
    function ($routeParams, $compile, $templateCache, $http, $filter) {
      function Controller($scope) {
        $scope.filterOptions = $scope.groupFactory.options;
        $scope.changeSchools = function (newVals, oldVals) {
          $scope.groupFactory.loadSchoolChange(newVals, oldVals);
        };
        $scope.loadSchools = function (grades, oldVals) {
          $scope.groupFactory.loadSchoolsByGrade(grades, oldVals);
        };
        $scope.clearSchools = function (oldVals) {
          $scope.groupFactory.clearSchools(oldVals);
        };
        $scope.changeSchoolYears = function (newVals, oldVals) {
          $scope.groupFactory.loadSchoolYearChange(newVals, oldVals);
        };
        $scope.changeGrades = function (newVals, oldVals) {
          $scope.groupFactory.loadGradeChange(newVals, oldVals);
        };
        $scope.changeTeachers = function (newVals, oldVals) {
          $scope.groupFactory.loadTeacherChange(newVals, oldVals);
        };
        $scope.changeSections = function () {
          $scope.groupFactory.loadSectionChange();
        };
        $scope.select2SchoolOptions = {
          minimumInputLength: 0,
          data: $scope.filterOptions.schools,
          multiple: true,
          width: 'resolve'
        };
        $scope.select2SchoolYearOptions = {
          minimumInputLength: 0,
          data: $scope.filterOptions.schoolYears,
          multiple: false,
          width: 'resolve'
        };
        $scope.select2BenchmarkDateOptions = {
          minimumInputLength: 0,
          data: $scope.filterOptions.testDueDates,
          multiple: false,
          width: 'resolve'
        };
        $scope.select2GradeOptions = {
          minimumInputLength: 0,
          data: $scope.filterOptions.grades,
          multiple: true,
          width: 'resolve'
        };
        $scope.select2TeacherOptions = {
          minimumInputLength: 0,
          data: $scope.filterOptions.teachers,
          multiple: true,
          width: 'resolve'
        };
        $scope.select2SectionOptions = {
          minimumInputLength: 0,
          data: $scope.filterOptions.sections,
          multiple: true,
          width: 'resolve'
        };
        $scope.select2InterventionTypeOptions = {
          minimumInputLength: 0,
          data: $scope.filterOptions.interventionTypes,
          multiple: true,
          width: 'resolve'
        };
        $scope.select2SpedOptions = {
          minimumInputLength: 0,
          data: [
            {
              id: 1,
              text: 'Special Ed (Special Education Student)'
            },
            {
              id: 0,
              text: 'Non-Special Ed (General Population Student)'
            }
          ],
          multiple: false,
          width: 'resolve'
        };
      }
      return {
        scope: {
          groupFactory: '=',
          tddEnabled: '=',
          assessmentFieldEnabled: '=',
          groupName: '='
        },
        restrict: 'E',
        templateUrl: 'templates/stacked-bar-graph-group-options.html',
        controller: Controller
      };
    }
  ]).factory('nsStackedBarGraphOptionsFactory', [
    '$http',
    '$routeParams',
    '$filter',
    'nsLookupFieldService',
    'webApiBaseUrl',
    'spinnerService',
    'progressLoader',
    function ($http, $routeParams, $filter, nsLookupFieldService, webApiBaseUrl, spinnerService, progressLoader) {
      var nsStackedBarGraphOptionsFactory = function (name, tddEnabled, skipAssessmentFieldLoad, explicitReturnLoadPromise) {
        var self = this;
        self.tddEnabled = tddEnabled;
        self.name = name;
        self.LookupFieldsArray = nsLookupFieldService.LookupFieldsArray;
        self.options = {};
        self.groupResults = {};
        self.Scores = [];
        self.BenchmarksByGrade = [];
        self.TestDueDates = [];
        self.StudentRecords = [];
        self.options.selectedAssessmentField = {};
        self.options.attributeTypes = [];
        self.options.teachers = [];
        self.options.grades = [];
        self.options.sections = [];
        self.options.students = [];
        self.options.schools = [];
        self.options.schoolYears = [];
        self.options.ethnicities = [];
        self.options.genders = [];
        self.options.interventionTypes = [];
        self.options.titleOneTypes = [];
        self.options.educationLabels = [];
        self.options.testDueDates = [];
        self.options.selectedGrades = [];
        self.options.selectedTeachers = [];
        self.options.selectedSections = [];
        self.options.selectedSchools = [];
        self.options.selectedEthnicities = [];
        self.options.selectedInterventionTypes = [];
        self.options.selectedSpedTypes = null;
        self.options.selectedTitleOneTypes = [];
        self.options.selectedEducationLabels = [];
        self.options.selectedSchoolYear = {};
        // TODO: GetDefaultYear
        self.options.selectedTestDueDate = {};
        self.options.selectedADSIS = false;
        self.options.selectedELL = false;
        self.options.selectedGifted = false;
        self.options.selectedGender = {};
        self.PageSize = 20;
        //this.options.selected
        self.LoadAssessmentFields = function () {
          var url = webApiBaseUrl + '/api/benchmark/GetAssessmentsAndFields';
          var promise = $http.get(url);
          return promise.then(function (response) {
            self.options.assessments = self.flatten(response.data.Assessments);
          });
        };
        self.flatten = function (data) {
          var out = [];
          angular.forEach(data, function (d) {
            angular.forEach(d.Fields, function (v) {
              out.push({
                AssessmentName: d.AssessmentName,
                TestType: d.TestType,
                DisplayLabel: v.DisplayLabel,
                FieldName: v.DatabaseColumn,
                LookupFieldName: v.LookupFieldName,
                AssessmentId: d.Id,
                FieldType: v.FieldType,
                RangeHigh: v.RangeHigh,
                RangeLow: v.RangeLow,
                DisplayInLineGraphs: v.DisplayInLineGraphs,
                DatabaseColumn: v.DatabaseColumn
              });
            });
          });
          return out;
        };
        self.loadOSData = function () {
          spinnerService.show('tableSpinner');
          var selectedAttributes = [];
          for (var i = 0; i < self.options.attributeTypes.length; i++) {
            var currentType = self.options.attributeTypes[i];
            selectedAttributes.push({
              AttributeTypeId: currentType.AttributeTypeId,
              Name: currentType.Name,
              DropDownData: currentType.selectedData
            });
          }
          var returnObject = {
              Schools: self.options.selectedSchools,
              Grades: self.options.selectedGrades,
              Teachers: self.options.selectedTeachers,
              Sections: self.options.selectedSections,
              InterventionTypes: self.options.selectedInterventionTypes,
              SpecialEd: self.options.selectedSpedTypes,
              SchoolStartYear: self.options.selectedSchoolYear.id,
              DropdownDataList: selectedAttributes,
              AssessmentField: self.options.selectedAssessmentField,
              TestDueDateID: self.options.selectedTestDueDate.id,
              GroupName: self.name
            };
          return $http.post(webApiBaseUrl + '/api/assessment/GetFilteredObservationSummary', returnObject).then(function (response) {
            angular.extend(self, response.data);
            self.LookupLists = nsLookupFieldService.LookupFieldsArray;
            //if (response.data.Scores != null) {
            //    self.AllStudentResults = response.data.Scores.StudentResults;
            //    self.Scores.StudentResults = self.AllStudentResults.slice(0, self.PageSize);
            //    self.Scores.Fields = response.data.Scores.Fields;
            //    self.Scores.HeaderGroups = response.data.Scores.HeaderGroups;
            //}
            if (self.Scores === null)
              self.Scores = { StudentResults: [] };
            // benchmarks by grade
            //self.BenchmarksByGrade = response.data.BenchmarksByGrade;
            if (response.data.BenchmarksByGrade === null)
              self.BenchmarksByGrade = [];
            // reset infinity scroll
            self.timesScrolled = 1;
            self.maxRecords = self.PageSize;
            return;
          }).finally(function (response) {
            spinnerService.hide('tableSpinner');
          });
        };
        self.loadOSExportData = function () {
          var selectedAttributes = [];
          for (var i = 0; i < self.options.attributeTypes.length; i++) {
            var currentType = self.options.attributeTypes[i];
            selectedAttributes.push({
              AttributeTypeId: currentType.AttributeTypeId,
              Name: currentType.Name,
              DropDownData: currentType.selectedData
            });
          }
          var returnObject = {
              Schools: self.options.selectedSchools,
              Grades: self.options.selectedGrades,
              Teachers: self.options.selectedTeachers,
              Sections: self.options.selectedSections,
              InterventionTypes: self.options.selectedInterventionTypes,
              SpecialEd: self.options.selectedSpedTypes,
              SchoolStartYear: self.options.selectedSchoolYear.id,
              DropdownDataList: selectedAttributes,
              AssessmentField: self.options.selectedAssessmentField,
              TestDueDateID: self.options.selectedTestDueDate.id,
              GroupName: self.name
            };
          // doesn't actually return the data, just creates a job
          return $http.post(webApiBaseUrl + '/api/exportdata/CreateAssessmentDataExportJob', returnObject);
        };
        self.timesScrolled = 1;
        self.maxRecords = self.PageSize;
        // every time we scroll, add more records
        self.loadOSInfinityRecords = function () {
          if (self.Scores.StudentResults) {
            var scrollStart = self.timesScrolled * self.PageSize;
            // don't keep calling this forever
            if (self.Scores.StudentResults.length > scrollStart) {
              progressLoader.start();
              progressLoader.set(50);
              //self.Scores.StudentResults.push.apply(self.Scores.StudentResults, self.AllStudentResults.slice(scrollStart, scrollStart + self.PageSize));
              self.timesScrolled++;
              self.maxRecords = scrollStart + self.PageSize;
              progressLoader.end();
            }
          }
        };
        self.loadGroupData = function (comparison) {
          var selectedAttributes = [];
          for (var i = 0; i < self.options.attributeTypes.length; i++) {
            var currentType = self.options.attributeTypes[i];
            selectedAttributes.push({
              AttributeTypeId: currentType.AttributeTypeId,
              Name: currentType.Name,
              DropDownData: currentType.selectedData
            });
          }
          var returnObject = {
              Schools: self.options.selectedSchools,
              Grades: self.options.selectedGrades,
              Teachers: self.options.selectedTeachers,
              Sections: self.options.selectedSections,
              InterventionTypes: self.options.selectedInterventionTypes,
              SpecialEd: self.options.selectedSpedTypes,
              SchoolStartYear: self.options.selectedSchoolYear.id,
              DropdownDataList: selectedAttributes,
              AssessmentField: self.options.selectedAssessmentField,
              TestDueDateID: self.tddEnabled ? self.options.selectedTestDueDate.id : null,
              GroupName: self.name
            };
          if (comparison) {
            return $http.post(webApiBaseUrl + '/api/stackedbargraph/GetStackedBarGraphComparisonData', returnObject);
          } else {
            return $http.post(webApiBaseUrl + '/api/stackedbargraph/GetStackedBarGraphGroupData', returnObject);
          }  //    .then(function (response) {
             //    self.groupResults.data = response.data;
             //});
        };
        self.loadSummaryData = function (scoreGrouping, tdd, comparisionTdd, isHistorical) {
          var selectedAttributes = [];
          for (var i = 0; i < self.options.attributeTypes.length; i++) {
            var currentType = self.options.attributeTypes[i];
            selectedAttributes.push({
              AttributeTypeId: currentType.AttributeTypeId,
              Name: currentType.Name,
              DropDownData: currentType.selectedData
            });
          }
          var returnObject = {
              Schools: self.options.selectedSchools,
              Grades: self.options.selectedGrades,
              Teachers: self.options.selectedTeachers,
              Sections: self.options.selectedSections,
              InterventionTypes: self.options.selectedInterventionTypes,
              SpecialEd: self.options.selectedSpedTypes,
              SchoolStartYear: self.options.selectedSchoolYear.id,
              ScoreGrouping: angular.isDefined(scoreGrouping) ? scoreGrouping : $routeParams.scoreGrouping,
              TestDueDate: angular.isDefined(comparisionTdd) && comparisionTdd != null ? moment(comparisionTdd).format('MM/DD/YYYY') : tdd,
              AssessmentField: self.options.selectedAssessmentField,
              DropdownDataList: selectedAttributes
            };
          if (isHistorical) {
            return $http.post(webApiBaseUrl + '/api/stackedbargraph/GetStackedBarGraphGroupHistoricalSummary', returnObject).then(function (response) {
              self.TestDueDates = response.data.TestDueDates;
              self.HistoricalSummaryRecords = response.data.SummaryRecords;
              self.Fields = response.data.Fields;
              self.HistoricalSummaryDataPostProcess();
            });
          } else {
            return $http.post(webApiBaseUrl + '/api/stackedbargraph/GetStackedBarGraphGroupSummary', returnObject).then(function (response) {
              self.TestDueDates = response.data.TestDueDates;
              self.SummaryRecords = response.data.SummaryRecords;
              self.Fields = response.data.Fields;
              self.SummaryDataPostProcess();
            });
          }
        };
        self.SummaryDataPostProcess = function () {
          for (var j = 0; j < self.SummaryRecords.length; j++) {
            for (var i = 0; i < self.SummaryRecords[j].ResultsByTDD.length; i++) {
              for (var k = 0; k < self.SummaryRecords[j].ResultsByTDD[i].FieldResults.length; k++) {
                for (var r = 0; r < self.Fields.length; r++) {
                  if (self.Fields[r].DatabaseColumn == self.SummaryRecords[j].ResultsByTDD[i].FieldResults[k].DbColumn) {
                    self.SummaryRecords[j].ResultsByTDD[i].FieldResults[k].Field = angular.copy(self.Fields[r]);  //if (self.Fields[r].FieldType === "DropdownFromDB") {
                                                                                                                  //    for (var p = 0; p < self.LookupFieldsArray.length; p++) {
                                                                                                                  //        if (self.LookupFieldsArray[p].LookupColumnName === self.Fields[r].LookupFieldName) {
                                                                                                                  //            // now find the specifc value that matches
                                                                                                                  //            for (var y = 0; y < self.LookupFieldsArray[p].LookupFields.length; y++) {
                                                                                                                  //                if (self.SummaryRecords[j].ResultsByTDD[i].FieldResults[k].IntValue === self.LookupFieldsArray[p].LookupFields[y].FieldSpecificId) {
                                                                                                                  //                    self.SummaryRecords[j].ResultsByTDD[i].FieldResults[k].DisplayValue = self.LookupFieldsArray[p].LookupFields[y].FieldValue;
                                                                                                                  //                }
                                                                                                                  //            }
                                                                                                                  //        }
                                                                                                                  //    }
                                                                                                                  //}
                  }
                }
              }
            }
          }
        };
        function uniqueClassCount(summaryRecords, studentId) {
          var uniqueCounter = 0;
          var currentSection = '';
          var recordsForStudentId = $filter('filter')(summaryRecords, { StudentID: studentId });
          for (var i = 0; i < recordsForStudentId.length; i++) {
            if (currentSection !== recordsForStudentId[i].Section) {
              currentSection = recordsForStudentId[i].Section;
              uniqueCounter++;
            }
          }
          return uniqueCounter;
        }
        function uniqueSchoolForClassCount(summaryRecords, studentId, schoolName) {
          var recordsForStudentIdSchool = $filter('filter')(summaryRecords, {
              StudentID: studentId,
              SchoolName: schoolName
            });
          return recordsForStudentIdSchool.length;
        }
        self.HistoricalSummaryDataPostProcess = function () {
          var currentStudentId = -1;
          var currentSchool = '';
          for (var j = 0; j < self.HistoricalSummaryRecords.length; j++) {
            if (currentStudentId != self.HistoricalSummaryRecords[j].StudentID) {
              currentStudentId = self.HistoricalSummaryRecords[j].StudentID;
              self.HistoricalSummaryRecords[j].FirstRecord = true;
              self.HistoricalSummaryRecords[j].RowSpan = uniqueClassCount(self.HistoricalSummaryRecords, currentStudentId);
              currentSchool = self.HistoricalSummaryRecords[j].SchoolName;
              self.HistoricalSummaryRecords[j].SchoolRowSpan = uniqueSchoolForClassCount(self.HistoricalSummaryRecords, currentStudentId, currentSchool);
              self.HistoricalSummaryRecords[j].FirstSchoolRecord = true;
            } else {
              self.HistoricalSummaryRecords[j].FirstRecord = false;
              self.HistoricalSummarcurrentSchool = self.HistoricalSummaryRecords[j].SchoolName;
              if (currentSchool != self.HistoricalSummaryRecords[j].SchoolName) {
                currentSchool = self.HistoricalSummaryRecords[j].SchoolName;
                self.HistoricalSummaryRecords[j].SchoolRowSpan = uniqueSchoolForClassCount(self.HistoricalSummaryRecords, currentStudentId, currentSchool);
                self.HistoricalSummaryRecords[j].FirstSchoolRecord = true;
              } else {
                self.HistoricalSummaryRecords[j].SchoolRowSpan = 0;
                self.HistoricalSummaryRecords[j].FirstSchoolRecord = false;
              }
            }
            // get unique class count, which will give me the rowspan for 
            for (var i = 0; i < self.HistoricalSummaryRecords[j].ResultsByTDD.length; i++) {
              for (var k = 0; k < self.HistoricalSummaryRecords[j].ResultsByTDD[i].FieldResults.length; k++) {
                for (var r = 0; r < self.Fields.length; r++) {
                  if (self.Fields[r].DatabaseColumn == self.HistoricalSummaryRecords[j].ResultsByTDD[i].FieldResults[k].DbColumn) {
                    self.HistoricalSummaryRecords[j].ResultsByTDD[i].FieldResults[k].Field = angular.copy(self.Fields[r]);  //if (self.Fields[r].FieldType === "DropdownFromDB") {
                                                                                                                            //    for (var p = 0; p < self.LookupFieldsArray.length; p++) {
                                                                                                                            //        if (self.LookupFieldsArray[p].LookupColumnName === self.Fields[r].LookupFieldName) {
                                                                                                                            //            // now find the specifc value that matches
                                                                                                                            //            for (var y = 0; y < self.LookupFieldsArray[p].LookupFields.length; y++) {
                                                                                                                            //                if (self.HistoricalSummaryRecords[j].ResultsByTDD[i].FieldResults[k].IntValue === self.LookupFieldsArray[p].LookupFields[y].FieldSpecificId) {
                                                                                                                            //                    self.HistoricalSummaryRecords[j].ResultsByTDD[i].FieldResults[k].DisplayValue = self.LookupFieldsArray[p].LookupFields[y].FieldValue;
                                                                                                                            //                }
                                                                                                                            //            }
                                                                                                                            //        }
                                                                                                                            //    }
                                                                                                                            //}
                  }
                }
              }
            }
          }
        };
        self.loadOptions = function () {
          var returnObject = { ChangeType: 'initial' };
          return $http.post(webApiBaseUrl + '/api/stackedbargraph/GetStackedBarGraphGroupingUpdatedOptions', returnObject).then(function (response) {
            //self.options.schools.splice(1, 0);
            self.options.schools.push.apply(self.options.schools, response.data.Schools);
            self.options.grades.push.apply(self.options.grades, response.data.Grades);
            self.options.teachers.push.apply(self.options.teachers, response.data.Teachers);
            self.options.sections.push.apply(self.options.sections, response.data.Sections);
            self.options.schoolYears.push.apply(self.options.schoolYears, response.data.SchoolYears);
            self.options.educationLabels.push.apply(self.options.educationLabels, response.data.EducationLabels);
            self.options.titleOneTypes.push.apply(self.options.titleOneTypes, response.data.TitleOneTypes);
            self.options.interventionTypes.push.apply(self.options.interventionTypes, response.data.InterventionTypes);
            self.options.ethnicities.push.apply(self.options.ethnicities, response.data.Ethnicities);
            self.options.genders.push.apply(self.options.genders, response.data.Genders);
            self.options.attributeTypes.push.apply(self.options.attributeTypes, response.data.DropdownDataList);
            self.options.testDueDates.push.apply(self.options.testDueDates, response.data.TestDueDates);
            // set defaults
            if (response.data.SelectedSchoolYear != null) {
              for (var i = 0; i < self.options.schoolYears.length; i++) {
                if (self.options.schoolYears[i].id == response.data.SelectedSchoolYear) {
                  self.options.selectedSchoolYear = self.options.schoolYears[i];
                }
              }
            }
            if (response.data.SelectedSchool != null) {
              for (var i = 0; i < self.options.schools.length; i++) {
                if (self.options.schools[i].id == response.data.SelectedSchool) {
                  self.options.selectedSchools.splice(0, 0, self.options.schools[i]);
                }
              }
            }
            if (response.data.SelectedGrade != null) {
              for (var i = 0; i < self.options.grades.length; i++) {
                if (self.options.grades[i].id == response.data.SelectedGrade) {
                  self.options.selectedGrades.splice(0, 0, self.options.grades[i]);
                }
              }
            }
            if (response.data.SelectedTeacher != null) {
              for (var i = 0; i < self.options.teachers.length; i++) {
                if (self.options.teachers[i].id == response.data.SelectedTeacher) {
                  self.options.selectedTeachers.splice(0, 0, self.options.teachers[i]);
                }
              }
            }
            if (response.data.SelectedSection != null) {
              for (var i = 0; i < self.options.sections.length; i++) {
                if (self.options.sections[i].id == response.data.SelectedSection) {
                  self.options.selectedSections.splice(0, 0, self.options.sections[i]);
                }
              }
            }
            // TODO: set a reasonable default TDD (should come from response.data.SelectedTDD!!!)
            //if (self.options.testDueDates.length > 0 && self.tddEnabled) {
            //    self.options.selectedTestDueDate = self.options.testDueDates[0];
            //}
            if (response.data.SelectedTestDueDateId != null) {
              for (var i = 0; i < self.options.testDueDates.length; i++) {
                if (self.options.testDueDates[i].id == response.data.SelectedTestDueDateId) {
                  self.options.selectedTestDueDate = self.options.testDueDates[i];
                }
              }
            }  //options.schoolYears.splice(1, 0, data.SchoolYears);
               //options.schoolYears.push.apply(options.schoolYears, data.SchoolYears);
               //options.benchmarkDates.splice(1, 0, data.TestDueDates);
               //tions.benchmarkDates.push.apply(options.benchmarkDates, data.TestDueDates);
          });
        };
        if (!explicitReturnLoadPromise) {
          self.loadOptions();
        }
        if (!skipAssessmentFieldLoad) {
          self.LoadAssessmentFields();
        }
        //this.initialLoad = function()
        //{
        //    this.loadOptions(this.options);
        //}
        self.loadSchoolChange = function (newSelections, oldSelections, forceLoad) {
          if (angular.equals(newSelections, oldSelections) && !angular.isDefined(forceLoad)) {
            return;
          }
          var returnObject = {
              ChangeType: 'school',
              Schools: self.options.selectedSchools,
              SchoolStartYear: self.options.selectedSchoolYear.id
            };
          $http.post(webApiBaseUrl + '/api/stackedbargraph/GetStackedBarGraphGroupingUpdatedOptions', returnObject).success(function (data) {
            self.options.grades.length = 0;
            self.options.grades.push.apply(self.options.grades, data.Grades);
            self.options.teachers.length = 0;
            ;
            self.options.teachers.push.apply(self.options.teachers, data.Teachers);
            self.options.sections.length = 0;
            self.options.students.length = 0;
            self.options.selectedGrades = [];
            self.options.selectedSections = [];
            self.options.selectedStudents = [];
            self.options.selectedTeachers = [];
          });
        };
        self.loadSchoolsByGrade = function (grades, oldSelections) {
          // get schools, then call loadschoolchange with the schools
          var paramObj = { Grades: grades };
          $http.post(webApiBaseUrl + '/api/stackedbargraph/getschoolsbygrade', paramObj).then(function (response) {
            self.options.selectedSchools.length = 0;
            self.options.selectedSchools.push.apply(self.options.selectedSchools, response.data.Schools);
            self.loadSchoolChange(self.options.selectedSchools, oldSelections, true);
          });
        };
        self.clearSchools = function (oldSelections) {
          self.options.selectedSchools.length = 0;
          self.loadSchoolChange(self.options.selectedSchools, oldSelections, true);
        };
        self.loadGradeChange = function (newSelections, oldSelections) {
          if (angular.equals(newSelections, oldSelections)) {
            return;
          }
          var returnObject = {
              ChangeType: 'grade',
              Schools: self.options.selectedSchools,
              Grades: self.options.selectedGrades,
              SchoolStartYear: self.options.selectedSchoolYear.id
            };
          $http.post(webApiBaseUrl + '/api/stackedbargraph/GetStackedBarGraphGroupingUpdatedOptions', returnObject).success(function (data) {
            self.options.teachers.length = 0;
            self.options.teachers.push.apply(self.options.teachers, data.Teachers);
            self.options.sections.length = 0;
            self.options.students.length = 0;
            self.options.selectedSections = [];
            self.options.selectedStudents = [];
            self.options.selectedTeachers = [];
          });
        };
        self.loadTeacherChange = function (newSelections, oldSelections) {
          if (angular.equals(newSelections, oldSelections)) {
            return;
          }
          var returnObject = {
              ChangeType: 'teacher',
              Schools: self.options.selectedSchools,
              Grades: self.options.selectedGrades,
              Teachers: self.options.selectedTeachers,
              SchoolStartYear: self.options.selectedSchoolYear.id
            };
          $http.post(webApiBaseUrl + '/api/stackedbargraph/GetStackedBarGraphGroupingUpdatedOptions', returnObject).success(function (data) {
            //self.options.sections = data.Sections;
            //self.options.sections.splice(1, self.options.sections.length)
            self.options.sections.length = 0;
            ;
            self.options.sections.push.apply(self.options.sections, data.Sections);  //if (data.Sections.length === 1) {
                                                                                     //    self.options.selectedSections = self.options.sections[1];
                                                                                     //}
                                                                                     //else {
                                                                                     //    self.options.selectedSections = self.options.sections[0];
                                                                                     //}
                                                                                     // load students
          });
        };
        self.loadSchoolYearChange = function () {
          var returnObject = {
              ChangeType: 'schoolyear',
              SchoolStartYear: self.options.selectedSchoolYear.id,
              Schools: self.options.selectedSchools,
              Grades: self.options.selectedGrades,
              Teachers: self.options.selectedTeachers
            };
          $http.post(webApiBaseUrl + '/api/stackedbargraph/GetStackedBarGraphGroupingUpdatedOptions', returnObject).then(function (response) {
            self.options.testDueDates.length = 0;
            self.options.testDueDates.push.apply(self.options.testDueDates, response.data.TestDueDates);
            // TODO: find "current" TDD and select it from array
            if (self.options.testDueDates.length > 0) {
              self.options.selectedTestDueDate = self.options.testDueDates[0];
            }
            self.options.interventionTypes.length = 0;
            self.options.selectedInterventionTypes = [];
            self.options.interventionTypes.push.apply(self.options.interventionTypes, response.data.InterventionTypes);
            self.options.grades.length = 0;
            self.options.grades.push.apply(self.options.grades, response.data.Grades);
            self.options.selectedGrades = [];
            self.options.sections.length = 0;
            self.options.sections.push.apply(self.options.sections, response.data.Sections);
            self.options.selectedSections = [];
            self.options.teachers.length = 0;
            self.options.teachers.push.apply(self.options.teachers, response.data.Teachers);
            self.options.selectedTeachers = [];
            if (response.data.SelectedGrade != null) {
              for (var i = 0; i < self.options.grades.length; i++) {
                if (self.options.grades[i].id == response.data.SelectedGrade) {
                  self.options.selectedGrades.splice(0, 0, self.options.grades[i]);
                }
              }
            }
            if (response.data.SelectedTeacher != null) {
              for (var i = 0; i < self.options.teachers.length; i++) {
                if (self.options.teachers[i].id == response.data.SelectedTeacher) {
                  self.options.selectedTeachers.splice(0, 0, self.options.teachers[i]);
                }
              }
            }
            if (response.data.SelectedSection != null) {
              for (var i = 0; i < self.options.sections.length; i++) {
                if (self.options.sections[i].id == response.data.SelectedSection) {
                  self.options.selectedSections.splice(0, 0, self.options.sections[i]);
                }
              }
            }
          });
        };
      };
      return nsStackedBarGraphOptionsFactory;
    }
  ]);
  ;
  StackedBarGraphComparisonController.$inject = [
    '$scope',
    '$q',
    '$http',
    'pinesNotifications',
    '$location',
    '$filter',
    'nsStackedBarGraphOptionsFactory',
    '$routeParams',
    'spinnerService'
  ];
  function StackedBarGraphComparisonController($scope, $q, $http, pinesNotifications, $location, $filter, nsStackedBarGraphOptionsFactory, $routeParams, spinnerService) {
    $scope.settings = { chartHeader: '' };
    $scope.settings.summaryMode = false;
    $scope.settings.summaryView = 'Current Year Only (Sortable)';
    $scope.settings.summaryCategory = '';
    $scope.settings.summaryScoreGrouping = 1;
    $scope.settings.stacking = 'normal';
    $scope.settings.stackingDescription = 'Number of Students';
    $scope.settings.graphGenerated = false;
    $scope.groupCounter = 1;
    $scope.sortArray = [];
    $scope.schoolYearSame = true;
    $scope.datesSame = true;
    $scope.schoolsSame = true;
    $scope.gradesSame = true;
    $scope.teachersSame = true;
    $scope.sectionsSame = true;
    $scope.interventionTypesSame = true;
    $scope.specialEdSame = true;
    $scope.assessmentFieldSame = true;
    $scope.attributesSameArrary = [];
    var highchartsNgConfig = {};
    $scope.groupsFactory = {};
    $scope.comparisonGroups = [];
    var firstGroup = new nsStackedBarGraphOptionsFactory('Group 1', true, false, true);
    $scope.attributeTypesArray = [];
    // load attributesSameArray
    firstGroup.loadOptions().then(function (response) {
      $scope.attributeTypesArray = firstGroup.options.attributeTypes;
      for (var i = 0; i < $scope.attributeTypesArray.length; i++) {
        $scope.attributesSameArrary[$scope.attributeTypesArray[i].Name] = true;
      }
    });
    firstGroup.DisplayName = 'Group 1';
    firstGroup.AccordionHeaderDisplayName = 'Group 1';
    $scope.comparisonGroups.push(firstGroup);
    $scope.filterOptions = $scope.comparisonGroups[0].options;
    //$scope.groupResults = nsStackedBarGraphGroupsOptionsService.groupResults;
    $scope.getSummaryHeader = function (displayDate) {
      if ($scope.groupsFactory.TestDueDates.length == 1) {
        return $scope.groupsFactory.options.selectedSchoolYear.text;
      } else {
        return displayDate;
      }
    };
    $scope.generateChartHeader = function () {
      $scope.schoolYearSame = true;
      $scope.datesSame = true;
      $scope.schoolsSame = true;
      $scope.gradesSame = true;
      $scope.teachersSame = true;
      $scope.sectionsSame = true;
      $scope.interventionTypesSame = true;
      $scope.specialEdSame = true;
      $scope.assessmentFieldSame = true;
      for (var i = 0; i < $scope.attributeTypesArray.length; i++) {
        $scope.attributesSameArrary[$scope.attributeTypesArray[i].Name] = true;
      }
      // no special header for 1 or fewere groups
      if ($scope.comparisonGroups.length <= 1) {
        $scope.settings.chartHeader = '';
        return '';
      }
      var headerText = ' - ';
      // loop over all the comparison groups and compare them to eachother
      for (var i = 0; i < $scope.comparisonGroups.length; i++) {
        // don't process the last one
        if (i !== $scope.comparisonGroups.length - 1) {
          if ($scope.comparisonGroups[i].options.selectedSchoolYear.text != $scope.comparisonGroups[i + 1].options.selectedSchoolYear.text) {
            $scope.schoolYearSame = false;
          }
          if ($scope.comparisonGroups[i].options.selectedTestDueDate.text != $scope.comparisonGroups[i + 1].options.selectedTestDueDate.text) {
            $scope.datesSame = false;
          }
          if (!angular.equals($scope.comparisonGroups[i].options.selectedSchools, $scope.comparisonGroups[i + 1].options.selectedSchools)) {
            $scope.schoolsSame = false;
          }
          if (!angular.equals($scope.comparisonGroups[i].options.selectedGrades, $scope.comparisonGroups[i + 1].options.selectedGrades)) {
            $scope.gradesSame = false;
          }
          if (!angular.equals($scope.comparisonGroups[i].options.selectedTeachers, $scope.comparisonGroups[i + 1].options.selectedTeachers)) {
            $scope.teachersSame = false;
          }
          if (!angular.equals($scope.comparisonGroups[i].options.selectedSections, $scope.comparisonGroups[i + 1].options.selectedSections)) {
            $scope.sectionsSame = false;
          }
          if (!angular.equals($scope.comparisonGroups[i].options.selectedInterventionTypes, $scope.comparisonGroups[i + 1].options.selectedInterventionTypes)) {
            $scope.interventionTypesSame = false;
          }
          if ($scope.comparisonGroups[i].options.selectedSpedTypes != null && $scope.comparisonGroups[i + 1].options.selectedSpedTypes != null && $scope.comparisonGroups[i].options.selectedSpedTypes.text != $scope.comparisonGroups[i + 1].options.selectedSpedTypes.text || $scope.comparisonGroups[i].options.selectedSpedTypes != null && $scope.comparisonGroups[i + 1].options.selectedSpedTypes == null || $scope.comparisonGroups[i].options.selectedSpedTypes == null && $scope.comparisonGroups[i + 1].options.selectedSpedTypes != null) {
            $scope.specialEdSame = false;
          }
          if ($scope.comparisonGroups[i].options.selectedAssessmentField != null && $scope.comparisonGroups[i + 1].options.selectedAssessmentField != null && $scope.comparisonGroups[i].options.selectedAssessmentField.AssessmentName + '/' + $scope.comparisonGroups[i].options.selectedAssessmentField.DisplayLabel != $scope.comparisonGroups[i + 1].options.selectedAssessmentField.AssessmentName + '/' + $scope.comparisonGroups[i + 1].options.selectedAssessmentField.DisplayLabel || $scope.comparisonGroups[i].options.selectedAssessmentField == null && $scope.comparisonGroups[i + 1].options.selectedAssessmentField != null || $scope.comparisonGroups[i].options.selectedAssessmentField != null && $scope.comparisonGroups[i + 1].options.selectedAssessmentField == null) {
            $scope.assessmentFieldSame = false;
          }
          // Attributes
          for (var j = 0; j < $scope.comparisonGroups[i].options.attributeTypes.length; j++) {
            var att = $scope.comparisonGroups[i].options.attributeTypes[j];
            var att2 = $scope.comparisonGroups[i + 1].options.attributeTypes[j];
            if (!angular.equals(att.selectedData, att2.selectedData)) {
              $scope.attributesSameArrary[att.Name] = false;
            }
          }
        }
      }
      var delimiter = ' | ';
      // now add the ones that are the same across the board
      if ($scope.schoolYearSame) {
        headerText += $scope.comparisonGroups[0].options.selectedSchoolYear.text + delimiter;
      }
      if ($scope.datesSame) {
        headerText += $scope.comparisonGroups[0].options.selectedTestDueDate.text + delimiter;
      }
      if ($scope.schoolsSame) {
        headerText += getMultiDropdownString($scope.comparisonGroups[0].options.selectedSchools, delimiter);
      }
      if ($scope.gradesSame) {
        headerText += getMultiDropdownString($scope.comparisonGroups[0].options.selectedGrades, delimiter);
      }
      if ($scope.teachersSame) {
        headerText += getMultiDropdownString($scope.comparisonGroups[0].options.selectedTeachers, delimiter);
      }
      if ($scope.sectionsSame) {
        headerText += getMultiDropdownString($scope.comparisonGroups[0].options.selectedSections, delimiter);
      }
      if ($scope.interventionTypesSame) {
        headerText += getMultiDropdownString($scope.comparisonGroups[0].options.selectedInterventionTypes, delimiter);
      }
      if ($scope.specialEdSame && $scope.comparisonGroups[0].options.selectedSpedTypes != null) {
        headerText += $scope.comparisonGroups[0].options.selectedSpedTypes.text + delimiter;
      }
      if ($scope.assessmentFieldSame && $scope.comparisonGroups[0].options.selectedAssessmentField != null) {
        headerText += $scope.comparisonGroups[0].options.selectedAssessmentField.AssessmentName + '/' + $scope.comparisonGroups[0].options.selectedAssessmentField.DisplayLabel + delimiter;
      }
      for (var j = 0; j < $scope.comparisonGroups[0].options.attributeTypes.length; j++) {
        if ($scope.attributesSameArrary[$scope.comparisonGroups[0].options.attributeTypes[j].Name]) {
          headerText += getMultiDropdownString($scope.comparisonGroups[0].options.attributeTypes[j].selectedData, delimiter);
        }
      }
      // if every field is differnt, make it a part of the group names and not the header
      $scope.settings.chartHeader = headerText == ' - ' ? '' : headerText.substr(0, headerText.length - delimiter.length);
    };
    $scope.generateGroupName = function (groupName) {
      var group = null;
      for (var i = 0; i < $scope.comparisonGroups.length; i++) {
        if ($scope.comparisonGroups[i].name == groupName) {
          group = $scope.comparisonGroups[i];
        }
      }
      var delimiter = ' <br/> ';
      if (group == null) {
        return groupName;
      }
      var originalName = group.name;
      var customName = '';
      if (!$scope.schoolYearSame) {
        customName += group.options.selectedSchoolYear.text + delimiter;
      }
      if (!$scope.datesSame) {
        customName += group.options.selectedTestDueDate.text + delimiter;
      }
      if (!$scope.schoolsSame && group.options.selectedSchools.length > 0) {
        customName += getMultiDropdownString(group.options.selectedSchools, delimiter);
      }
      if (!$scope.gradesSame && group.options.selectedGrades.length > 0) {
        customName += getMultiDropdownString(group.options.selectedGrades, delimiter);
      }
      if (!$scope.teachersSame && group.options.selectedTeachers.length > 0) {
        customName += getMultiDropdownString(group.options.selectedTeachers, delimiter);
      }
      if (!$scope.sectionsSame && group.options.selectedSections.length > 0) {
        customName += getMultiDropdownString(group.options.selectedSections, delimiter);
      }
      if (!$scope.interventionTypesSame && group.options.selectedInterventionTypes.length > 0) {
        customName += getMultiDropdownString(group.options.selectedInterventionTypes, delimiter);
      }
      if (!$scope.specialEdSame && group.options.selectedSpedTypes != null) {
        customName += group.options.selectedSpedTypes.text + delimiter;
      }
      if (!$scope.assessmentFieldSame && group.options.selectedAssessmentField != null) {
        customName += group.options.selectedAssessmentField.AssessmentName + '/' + group.options.selectedAssessmentField.DisplayLabel + delimiter;
      }
      for (var j = 0; j < group.options.attributeTypes.length; j++) {
        if (!$scope.attributesSameArrary[group.options.attributeTypes[j].Name] && group.options.attributeTypes[j].selectedData.length > 0) {
          customName += getMultiDropdownString(group.options.attributeTypes[j].selectedData, delimiter);
        }
      }
      // return default name unless name has changed
      return customName == '' ? originalName : customName.substr(0, customName.length - delimiter.length);
    };
    function getMultiDropdownString(aryOptions, delimiter) {
      var result = '';
      if (aryOptions.length > 0) {
        for (var i = 0; i < aryOptions.length; i++) {
          result += aryOptions[i].text + ', ';
        }
        result = result.substring(0, result.length - 2) + delimiter;
      } else {
        result = '';
      }
      return result;
    }
    $scope.addNewGroup = function () {
      $scope.groupCounter++;
      var newGroup = new nsStackedBarGraphOptionsFactory('Group ' + $scope.groupCounter, true);
      newGroup.DisplayName = 'Group ' + $scope.groupCounter;
      newGroup.AccordionHeaderDisplayName = 'Group ' + $scope.groupCounter;
      $scope.comparisonGroups.push(newGroup);
    };
    $scope.removeGroup = function (name, $event) {
      $event.preventDefault();
      $event.stopPropagation();
      if ($scope.comparisonGroups.length == 1) {
        alert('You must keep at least one group.');
        return;
      }
      for (var i = 0; i < $scope.comparisonGroups.length; i++) {
        if ($scope.comparisonGroups[i].name === name) {
          $scope.comparisonGroups.splice(i, 1);
          return;
        }
      }
    };
    $scope.generateGraph = function () {
      $scope.settings.anyResults = false;
      spinnerService.show('tableSpinner');
      var promiseCollection = [];
      // loop over each group and get data... build graph once they've all returned
      for (var i = 0; i < $scope.comparisonGroups.length; i++) {
        promiseCollection.push($scope.comparisonGroups[i].loadGroupData(true));
      }
      var studentResultsCollection = [];
      //var groupNameCollection = [];
      $q.all(promiseCollection).then(function (response) {
        for (var j = 0; j < response.length; j++) {
          $scope.comparisonGroups[j].graphGenerated = true;
          $scope.settings.anyResults = true;
          studentResultsCollection.push(response[j].data);  //groupNameCollection.push()
        }
        updateDataFromServiceChange(studentResultsCollection);
      }).finally(function () {
        spinnerService.hide('tableSpinner');
      });
      $scope.settings.graphGenerated = true;
    };
    $scope.changeSummaryMode = function () {
      changeToSummaryMode($scope.settings.summaryCategory, $scope.settings.summaryScoreGrouping);
    };
    $scope.$watch('settings.stacking', function (newValue, oldValue) {
      if (!angular.equals(newValue, oldValue)) {
        if (newValue == 'normal') {
          $scope.settings.stackingDescription = 'Number of Students';
        } else {
          $scope.settings.stackingDescription = 'Percentage of Students';
        }
        $scope.generateGraph();
      }
    });
    var changeToSummaryMode = function (category, scoreGrouping) {
      spinnerService.show('tableSpinner');
      $scope.settings.summaryCategory = category;
      $scope.settings.summaryScoreGrouping = scoreGrouping;
      // find proper factory based on category (groupName)
      for (var i = 0; i < $scope.comparisonGroups.length; i++) {
        if ($scope.comparisonGroups[i].DisplayName == category) {
          $scope.groupsFactory = $scope.comparisonGroups[i];
          $scope.groupsFactory.loadSummaryData(scoreGrouping, category, $scope.groupsFactory.options.selectedTestDueDate.text, $scope.settings.summaryView !== 'Current Year Only (Sortable)').then(function (response) {
            $scope.settings.summaryMode = true;
          }).finally(function () {
            spinnerService.hide('tableSpinner');
          });
          break;
        }
      }
    };
    $scope.changeToChartMode = function () {
      $scope.settings.summaryView = 'Current Year Only (Sortable)';
      $scope.settings.summaryMode = false;
      $scope.settings.summaryCategory = '';
    };
    function getTitle() {
      var title = '';
      title += '<b>Schools: </b>';
      if ($scope.filterOptions.selectedSchools.length > 0) {
        for (var i = 0; i < $scope.filterOptions.selectedSchools.length; i++) {
          title += $scope.filterOptions.selectedSchools[i].text + ',';
        }
      } else {
        title += ' All ';
      }
      title += '<b>Grades: </b>';
      if ($scope.filterOptions.selectedGrades.length > 0) {
        for (var i = 0; i < $scope.filterOptions.selectedGrades.length; i++) {
          title += $scope.filterOptions.selectedGrades[i].text + ',';
        }
      } else {
        title += ' All ';
      }
      return title;
    }
    function updateDataFromServiceChange(studentResultsCollection) {
      // might need this
      //angular.copy(data.results, $scope.studentResults);
      //$scope.studentResults = nsStackedBarGraphGroupsOptionsService.groupResults.data;
      var re = /<br\/>/g;
      //return;
      var seriesArray = [];
      var categoriesArray = [];
      $scope.generateChartHeader();
      // set up series
      for (var i = 0; i < studentResultsCollection.length; i++) {
        var currentResult = studentResultsCollection[i];
        var groupName = $scope.generateGroupName(currentResult.GroupName);
        // update group display names
        for (var n = 0; n < $scope.comparisonGroups.length; n++) {
          if (currentResult.GroupName == $scope.comparisonGroups[n].name) {
            $scope.comparisonGroups[n].DisplayName = groupName;
            $scope.comparisonGroups[n].AccordionHeaderDisplayName = groupName.replace(re, ' | ');
          }
        }
        var foundCategory = $filter('filter')(categoriesArray, groupName, true);
        // see if category already exists, if not, add it
        if (!foundCategory.length) {
          categoriesArray.push(groupName);  //[categoriesArray.length] = { name: currentResult.DueDate, categories: [currentResult.DueDate] };
        }
        for (var j = 0; j < currentResult.Results.length; j++) {
          var currentScore = currentResult.Results[j];
          //labels: {rotation: -90}
          // create a data array for each scoregrouping
          // FIX THIS... need to be able to create an array of arrays with the index being the scoregrouping
          var groupingName = '';
          var groupingColor = '';
          if (currentScore.ScoreGrouping == 1) {
            groupingName = 'Exceeds Expectations';
            groupingColor = '#4697ce';
          }
          if (currentScore.ScoreGrouping == 2) {
            groupingName = 'Meets Expectations';
            groupingColor = '#90ED7D';
          }
          if (currentScore.ScoreGrouping == 3) {
            groupingName = 'Approaches Expectations';
            groupingColor = '#E4D354';
          }
          if (currentScore.ScoreGrouping == 4) {
            groupingName = 'Does Not Meet Expectations';
            groupingColor = '#BF453D';
          }
          if (seriesArray[currentScore.ScoreGrouping] == null) {
            seriesArray[currentScore.ScoreGrouping] = {
              name: groupingName,
              color: groupingColor,
              data: [currentScore.NumberOfResults],
              id: currentScore.ScoreGrouping
            };
          } else {
            seriesArray[currentScore.ScoreGrouping].data.push(currentScore.NumberOfResults);
          }
        }
      }
      highchartsNgConfig = {
        options: {
          chart: { type: 'column' },
          tooltip: {
            pointFormat: '<span style="color:{series.color}">\u25cf</span>  <span style="color:#666666">{series.name}</span>: <b>{point.y} Students</b> ({point.percentage:.0f}%)<br/>',
            style: {
              padding: 10,
              fontWeight: 'bold'
            },
            useHTML: true
          },
          plotOptions: {
            series: {
              cursor: 'pointer',
              point: {
                events: {
                  click: function (event) {
                    var category = this.category;
                    var scoreGrouping = this.series.userOptions.id;
                    changeToSummaryMode(category, scoreGrouping);
                  }
                }
              }
            },
            column: {
              stacking: $scope.settings.stacking,
              dataLabels: {
                enabled: true,
                color: Highcharts.theme && Highcharts.theme.dataLabelsColor || 'white',
                style: { textShadow: '0 0 3px black' },
                formatter: function () {
                  if (this.y > 0 && $scope.settings.stacking === 'normal')
                    return this.y;
                  if (this.y > 0 && $scope.settings.stacking === 'percent')
                    return this.percentage.toFixed(0) + '%';
                }
              }
            }
          }
        },
        yAxis: {
          allowDecimals: false,
          min: 0,
          title: { text: $scope.settings.stackingDescription },
          stackLabels: {
            enabled: true,
            style: {
              fontWeight: 'bold',
              color: Highcharts.theme && Highcharts.theme.textColor || 'gray'
            }
          }
        },
        series: seriesArray,
        title: { text: 'Compare Student Groups ' + $scope.settings.chartHeader },
        loading: false,
        xAxis: { categories: categoriesArray },
        useHighStocks: false,
        func: function (chart) {
        }
      };
      $scope.chartConfig = highchartsNgConfig;
    }
  }
  StackedBarGraphGroupsController.$inject = [
    '$scope',
    '$q',
    '$http',
    'pinesNotifications',
    '$location',
    '$filter',
    'nsStackedBarGraphOptionsFactory',
    '$routeParams',
    'spinnerService'
  ];
  function StackedBarGraphGroupsController($scope, $q, $http, pinesNotifications, $location, $filter, nsStackedBarGraphOptionsFactory, $routeParams, spinnerService) {
    $scope.settings = {};
    $scope.settings.summaryMode = false;
    $scope.settings.summaryView = 'Current Year Only (Sortable)';
    $scope.settings.summaryCategory = '';
    $scope.settings.summaryScoreGrouping = 1;
    $scope.settings.stacking = 'normal';
    $scope.settings.stackingDescription = 'Number of Students';
    $scope.settings.graphGenerated = false;
    $scope.sortArray = [];
    var highchartsNgConfig = {};
    $scope.groupsFactory = new nsStackedBarGraphOptionsFactory('Compare Group Across Benchmark Dates', false);
    $scope.filterOptions = $scope.groupsFactory.options;
    $scope.groupResults = $scope.groupsFactory.groupResults;
    $scope.generateGraph = function () {
      spinnerService.show('tableSpinner');
      $scope.groupsFactory.loadGroupData().then(function (response) {
        $scope.groupsFactory.groupResults.data = response.data;
        updateDataFromServiceChange();
      }).finally(function () {
        spinnerService.hide('tableSpinner');
      });
      $scope.settings.graphGenerated = true;
    };
    $scope.getSummaryHeader = function (displayDate) {
      if ($scope.groupsFactory.TestDueDates.length == 1) {
        return $scope.groupsFactory.options.selectedSchoolYear.text;
      } else {
        return displayDate;
      }
    };
    $scope.changeSummaryMode = function () {
      changeToSummaryMode($scope.settings.summaryCategory, $scope.settings.summaryScoreGrouping);
    };
    $scope.$watch('settings.stacking', function (newValue, oldValue) {
      if (!angular.equals(newValue, oldValue)) {
        if (newValue == 'normal') {
          $scope.settings.stackingDescription = 'Number of Students';
        } else {
          $scope.settings.stackingDescription = 'Percentage of Students';
        }
        updateDataFromServiceChange();
      }
    });
    var changeToSummaryMode = function (category, scoreGrouping) {
      spinnerService.show('tableSpinner');
      $scope.settings.summaryCategory = category;
      $scope.settings.summaryScoreGrouping = scoreGrouping;
      $scope.groupsFactory.loadSummaryData(scoreGrouping, category, null, $scope.settings.summaryView !== 'Current Year Only (Sortable)').then(function (response) {
        $scope.settings.summaryMode = true;
      }).finally(function () {
        spinnerService.hide('tableSpinner');
      });
    };
    $scope.changeToChartMode = function () {
      $scope.settings.summaryView = 'Current Year Only (Sortable)';
      $scope.settings.summaryMode = false;
    };
    function getTitle() {
      var title = '';
      title += '<b>Schools: </b>';
      if ($scope.filterOptions.selectedSchools.length > 0) {
        for (var i = 0; i < $scope.filterOptions.selectedSchools.length; i++) {
          title += $scope.filterOptions.selectedSchools[i].text + ',';
        }
      } else {
        title += ' All ';
      }
      title += '<b>Grades: </b>';
      if ($scope.filterOptions.selectedGrades.length > 0) {
        for (var i = 0; i < $scope.filterOptions.selectedGrades.length; i++) {
          title += $scope.filterOptions.selectedGrades[i].text + ',';
        }
      } else {
        title += ' All ';
      }
      return title;
    }
    function updateDataFromServiceChange() {
      $scope.settings.anyResults = false;
      // might need this
      $scope.studentResults = $scope.groupsFactory.groupResults.data;
      //return;
      var seriesArray = [];
      var categoriesArray = [];
      // set up series
      for (var i = 0; i < $scope.studentResults.length; i++) {
        $scope.settings.anyResults = true;
        var currentResult = $scope.studentResults[i];
        var formattedDate = currentResult.DueDate == null ? $scope.groupsFactory.options.selectedSchoolYear.text : moment(currentResult.DueDate).format('DD-MMM-YYYY');
        var foundCategory = $filter('filter')(categoriesArray, formattedDate, true);
        // see if category already exists, if not, add it
        if (!foundCategory.length) {
          categoriesArray.push(formattedDate);  //[categoriesArray.length] = { name: currentResult.DueDate, categories: [currentResult.DueDate] };
        }
        //labels: {rotation: -90}
        // create a data array for each scoregrouping
        // FIX THIS... need to be able to create an array of arrays with the index being the scoregrouping
        var groupingName = '';
        var groupingColor = '';
        if (currentResult.ScoreGrouping == 1) {
          groupingName = 'Exceeds Expectations';
          groupingColor = '#4697ce';
        }
        if (currentResult.ScoreGrouping == 2) {
          groupingName = 'Meets Expectations';
          groupingColor = '#90ED7D';
        }
        if (currentResult.ScoreGrouping == 3) {
          groupingName = 'Approaches Expectations';
          groupingColor = '#E4D354';
        }
        if (currentResult.ScoreGrouping == 4) {
          groupingName = 'Does Not Meet Expectations';
          groupingColor = '#BF453D';
        }
        if (seriesArray[currentResult.ScoreGrouping] == null) {
          seriesArray[currentResult.ScoreGrouping] = {
            name: groupingName,
            color: groupingColor,
            data: [currentResult.NumberOfResults],
            id: currentResult.ScoreGrouping
          };
        } else {
          seriesArray[currentResult.ScoreGrouping].data.push(currentResult.NumberOfResults);
        }
      }
      highchartsNgConfig = {
        options: {
          chart: { type: 'column' },
          tooltip: {
            pointFormat: '<span style="color:{series.color}">\u25cf</span>  <span style="color:#666666">{series.name}</span>: <b>{point.y} Students</b> ({point.percentage:.0f}%)<br/>',
            style: {
              padding: 10,
              fontWeight: 'bold'
            },
            useHTML: true
          },
          plotOptions: {
            series: {
              cursor: 'pointer',
              point: {
                events: {
                  click: function (event) {
                    var category = this.category;
                    var scoreGrouping = this.series.userOptions.id;
                    changeToSummaryMode(category, scoreGrouping);
                  }
                }
              }
            },
            column: {
              stacking: $scope.settings.stacking,
              dataLabels: {
                enabled: true,
                color: Highcharts.theme && Highcharts.theme.dataLabelsColor || 'white',
                style: { textShadow: '0 0 3px black' },
                formatter: function () {
                  if (this.y > 0 && $scope.settings.stacking === 'normal')
                    return this.y;
                  if (this.y > 0 && $scope.settings.stacking === 'percent')
                    return this.percentage.toFixed(0) + '%';
                }
              }
            }
          }
        },
        yAxis: {
          allowDecimals: false,
          min: 0,
          title: { text: $scope.settings.stackingDescription },
          stackLabels: {
            enabled: true,
            style: {
              fontWeight: 'bold',
              color: Highcharts.theme && Highcharts.theme.textColor || 'gray'
            }
          }
        },
        series: seriesArray,
        title: { text: 'Single Group Of Students Across Multiple Benchmark Dates' },
        loading: false,
        xAxis: { categories: categoriesArray },
        useHighStocks: false,
        func: function (chart) {
        }
      };
      $scope.chartConfig = highchartsNgConfig;
    }
  }
}());
(function () {
  'use strict';
  angular.module('stateTestDataModule', []).factory('StateTestDataImportManager', [
    '$http',
    'webApiBaseUrl',
    'FileSaver',
    function ($http, webApiBaseUrl, FileSaver) {
      var StateTestDataImportManager = function () {
        var self = this;
        self.LoadAssessmentTemplate = function (id) {
          var url = webApiBaseUrl + '/api/importstatetestdata/GetStateTestDataImportTemplate';
          var paramObj = { id: id };
          var promise = $http.post(url, paramObj);
          return promise.then(function (response) {
            self.Fields = response.data.Fields;
          });
        };
        self.LoadImportHistory = function () {
          var url = webApiBaseUrl + '/api/importstatetestdata/LoadStateTestImportHistory';
          var promise = $http.get(url);
          return promise.then(function (response) {
            self.HistoryItems = response.data.HistoryItems;
          });
        };
        self.deleteHistoryItem = function (item) {
          var url = webApiBaseUrl + '/api/importstatetestdata/DeleteHistoryItem';
          var paramObj = { id: item.Id };
          var promise = $http.post(url, paramObj);
          return promise;
        };
        self.downloadImportFile = function (item) {
          var url = webApiBaseUrl + '/api/importstatetestdata/GetImportFile';
          var paramObj = { value: item.UploadedFileName };
          var promise = $http.post(url, paramObj, { responseType: 'arraybuffer' }).then(function (response) {
              var data = new Blob([response.data]);
              FileSaver.saveAs(data, 'originalimportfile.csv');
            });
        };
        self.downloadImportLog = function (item) {
          var paramObj = { id: item.Id };
          var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/GetHistoryLog', paramObj);
          promise.then(function (response) {
            var data = new Blob([response.data.Result], { type: 'text/csv;charset=ANSI' });
            FileSaver.saveAs(data, 'importlog.txt');
          });
        };
      };
      return StateTestDataImportManager;
    }
  ]).controller('StateTestDataImportController', [
    '$scope',
    '$http',
    'webApiBaseUrl',
    'progressLoader',
    'StateTestDataImportManager',
    'nsFilterOptionsService',
    'FileSaver',
    '$timeout',
    'nsPinesService',
    '$interval',
    '$bootbox',
    function ($scope, $http, webApiBaseUrl, progressLoader, StateTestDataImportManager, nsFilterOptionsService, FileSaver, $timeout, nsPinesService, $interval, $bootbox) {
      $scope.settings = {
        uploadComplete: false,
        hasFiles: false
      };
      $scope.dataMgr = new StateTestDataImportManager();
      $scope.filterOptions = nsFilterOptionsService.options;
      $scope.theFiles = [];
      // watch assessmentfield chooser and reload template when it changes
      // load template
      $scope.$watch('filterOptions.selectedStateTest', function (newValue, oldValue) {
        if (!angular.equals(newValue, oldValue) && newValue != null) {
          progressLoader.start();
          progressLoader.set(50);
          $scope.dataMgr.LoadAssessmentTemplate($scope.filterOptions.selectedStateTest.id).then(function (response) {
            progressLoader.end();
          });
        }
      }, true);
      $scope.downloadImportFile = function (item) {
        $scope.dataMgr.downloadImportFile(item);
      };
      $scope.deleteHistoryItem = function (item) {
        $bootbox.confirm('Are you sure you want to delete this job?  <b>Note:</b> If it has not yet been processed, it will be cancelled.', function (response) {
          if (response) {
            progressLoader.start();
            progressLoader.set(50);
            $scope.dataMgr.deleteHistoryItem(item).then(function (response) {
              nsPinesService.dataDeletedSuccessfully();
              reloadHistoryTable();
              progressLoader.end();
            });
          }
        });
      };
      $scope.downloadImportLog = function (item) {
        $scope.dataMgr.downloadImportLog(item);
      };
      $scope.getStatusClass = function (item) {
        if (item.Status == 'Awaiting processing') {
          return 'badge-default';
        } else if (item.Status == 'Complete') {
          return 'badge-success';
        } else if (item.Status == 'Processing') {
          return 'badge-primary';
        } else if (item.Status == 'Error') {
          return 'badge-danger';
        } else {
          return 'badge-warning';
        }
      };
      var reloadHistoryTable = function () {
        $scope.dataMgr.LoadImportHistory();
      };
      reloadHistoryTable();
      // reload table every 5 seconds
      var reloadInterval = $interval(reloadHistoryTable, 5000);
      // here is where the cleanup happens
      $scope.$on('$destroy', function () {
        $interval.cancel(reloadInterval);
      });
      $scope.getTemplate = function () {
        var paramObj = { id: $scope.filterOptions.selectedStateTest.id };
        var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/getstatetestexporttemplate', paramObj);
        promise.then(function (response) {
          var data = new Blob([response.data.Result], { type: 'text/csv;charset=ANSI' });
          FileSaver.saveAs(data, 'template.csv');
        });
      };
      $scope.upload = function (theFiles) {
        var formData = new FormData();
        formData.append('AssessmentId', $scope.filterOptions.selectedStateTest.id);
        formData.append('SchoolYear', $scope.filterOptions.selectedSchoolYear.id);
        angular.forEach(theFiles, function (file) {
          formData.append(file.name, file);
        });
        var paramObj = {};
        // start loader
        progressLoader.start();
        progressLoader.set(50);
        var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/uploadstatetestcsv', formData, {
            transformRequest: angular.identity,
            headers: { 'Content-Type': undefined }
          }).then(function (response) {
            // end loader
            progressLoader.end();
            //$scope.errors = [];
            $scope.settings.LogItems = response.data.LogItems;
            // show success
            $timeout(function () {
              $('#formReset').click();
            }, 100);
            //$scope.theFiles.length = 0;
            //$scope.settings.hasFiles = false;
            $scope.settings.uploadComplete = true;
            if ($scope.settings.LogItems.length > 0) {
              nsPinesService.buildMessage('Data Import Error', 'There were one or more errors in your import file.', 'error');
            } else {
              nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
            }
          }, function (err) {
            progressLoader.end();
            $scope.settings.uploadComplete = true;
          });
      };
    }
  ]).factory('BenchmarkDataImportManager', [
    '$http',
    'webApiBaseUrl',
    'FileSaver',
    function ($http, webApiBaseUrl, FileSaver) {
      var BenchmarkDataImportManager = function () {
        var self = this;
        self.LoadAssessmentTemplate = function (id) {
          var url = webApiBaseUrl + '/api/importstatetestdata/GetBenchmarkTestDataImportTemplate';
          var paramObj = { id: id };
          var promise = $http.post(url, paramObj);
          return promise.then(function (response) {
            self.Fields = response.data.Fields;
          });
        };
        self.LoadImportHistory = function () {
          var url = webApiBaseUrl + '/api/importstatetestdata/LoadBenchmarkTestImportHistory';
          var promise = $http.get(url);
          return promise.then(function (response) {
            self.HistoryItems = response.data.HistoryItems;
          });
        };
        self.deleteHistoryItem = function (item) {
          var url = webApiBaseUrl + '/api/importstatetestdata/DeleteBMHistoryItem';
          var paramObj = { id: item.Id };
          var promise = $http.post(url, paramObj);
          return promise;
        };
        self.downloadImportFile = function (item) {
          var url = webApiBaseUrl + '/api/importstatetestdata/GetImportFile';
          var paramObj = { value: item.UploadedFileName };
          var promise = $http.post(url, paramObj, { responseType: 'arraybuffer' }).then(function (response) {
              var data = new Blob([response.data]);
              FileSaver.saveAs(data, 'originalimportfile.csv');
            });
        };
        self.downloadImportLog = function (item) {
          var paramObj = { id: item.Id };
          var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/GetBMHistoryLog', paramObj);
          promise.then(function (response) {
            var data = new Blob([response.data.Result], { type: 'text/csv;charset=ANSI' });
            FileSaver.saveAs(data, 'importlog.txt');
          });
        };
      };
      return BenchmarkDataImportManager;
    }
  ]).controller('BenchmarkDataImportController', [
    '$scope',
    '$http',
    'webApiBaseUrl',
    'progressLoader',
    'BenchmarkDataImportManager',
    'nsFilterOptionsService',
    'FileSaver',
    '$timeout',
    'nsPinesService',
    '$interval',
    '$bootbox',
    function ($scope, $http, webApiBaseUrl, progressLoader, BenchmarkDataImportManager, nsFilterOptionsService, FileSaver, $timeout, nsPinesService, $interval, $bootbox) {
      $scope.settings = {
        uploadComplete: false,
        hasFiles: false
      };
      $scope.dataMgr = new BenchmarkDataImportManager();
      $scope.filterOptions = nsFilterOptionsService.options;
      $scope.theFiles = [];
      // watch assessmentfield chooser and reload template when it changes
      // load template
      $scope.$watch('filterOptions.selectedBenchmarkTest', function (newValue, oldValue) {
        if (!angular.equals(newValue, oldValue) && newValue != null) {
          progressLoader.start();
          progressLoader.set(50);
          $scope.dataMgr.LoadAssessmentTemplate($scope.filterOptions.selectedBenchmarkTest.id).then(function (response) {
            progressLoader.end();
          });
        }
      }, true);
      $scope.downloadImportFile = function (item) {
        $scope.dataMgr.downloadImportFile(item);
      };
      $scope.deleteHistoryItem = function (item) {
        $bootbox.confirm('Are you sure you want to delete this job?  <b>Note:</b> If it has not yet been processed, it will be cancelled.', function (response) {
          if (response) {
            progressLoader.start();
            progressLoader.set(50);
            $scope.dataMgr.deleteHistoryItem(item).then(function (response) {
              nsPinesService.dataDeletedSuccessfully();
              reloadHistoryTable();
              progressLoader.end();
            });
          }
        });
      };
      $scope.downloadImportLog = function (item) {
        $scope.dataMgr.downloadImportLog(item);
      };
      $scope.getStatusClass = function (item) {
        if (item.Status == 'Awaiting processing') {
          return 'badge-default';
        } else if (item.Status == 'Complete') {
          return 'badge-success';
        } else if (item.Status == 'Processing') {
          return 'badge-primary';
        } else if (item.Status == 'Error') {
          return 'badge-danger';
        } else {
          return 'badge-warning';
        }
      };
      var reloadHistoryTable = function () {
        $scope.dataMgr.LoadImportHistory();
      };
      reloadHistoryTable();
      // reload table every 5 seconds
      var reloadInterval = $interval(reloadHistoryTable, 5000);
      // here is where the cleanup happens
      $scope.$on('$destroy', function () {
        $interval.cancel(reloadInterval);
      });
      $scope.getTemplate = function () {
        var paramObj = { id: $scope.filterOptions.selectedBenchmarkTest.id };
        var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/getbenchmarktestexporttemplate', paramObj);
        promise.then(function (response) {
          var data = new Blob([response.data.Result], { type: 'text/csv;charset=ANSI' });
          FileSaver.saveAs(data, 'template.csv');
        });
      };
      $scope.upload = function (theFiles) {
        var formData = new FormData();
        formData.append('AssessmentId', $scope.filterOptions.selectedBenchmarkTest.id);
        formData.append('SchoolYear', $scope.filterOptions.selectedSchoolYear.id);
        formData.append('BenchmarkDateId', $scope.filterOptions.selectedBenchmarkDate.id);
        formData.append('RecorderId', $scope.filterOptions.quickSearchStaff.id);
        angular.forEach(theFiles, function (file) {
          formData.append(file.name, file);
        });
        var paramObj = {};
        // start loader
        progressLoader.start();
        progressLoader.set(50);
        var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/uploadbenchmarktestcsv', formData, {
            transformRequest: angular.identity,
            headers: { 'Content-Type': undefined }
          }).then(function (response) {
            // end loader
            progressLoader.end();
            //$scope.errors = [];
            $scope.settings.LogItems = response.data.LogItems;
            // show success
            $timeout(function () {
              $('#formReset').click();
            }, 100);
            //$scope.theFiles.length = 0;
            //$scope.settings.hasFiles = false;
            $scope.settings.uploadComplete = true;
            if ($scope.settings.LogItems.length > 0) {
              nsPinesService.buildMessage('Data Import Error', 'There were one or more errors in your import file.', 'error');
            } else {
              nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
            }
          }, function (err) {
            progressLoader.end();
            $scope.settings.uploadComplete = true;
          });
      };
    }
  ]).factory('InterventionDataImportManager', [
    '$http',
    'webApiBaseUrl',
    'FileSaver',
    function ($http, webApiBaseUrl, FileSaver) {
      var InterventionDataImportManager = function () {
        var self = this;
        self.LoadAssessmentTemplate = function (id) {
          var url = webApiBaseUrl + '/api/importstatetestdata/GetInterventionTestDataImportTemplate';
          var paramObj = { id: id };
          var promise = $http.post(url, paramObj);
          return promise.then(function (response) {
            self.Fields = response.data.Fields;
          });
        };
        self.LoadImportHistory = function () {
          var url = webApiBaseUrl + '/api/importstatetestdata/LoadInterventionTestImportHistory';
          var promise = $http.get(url);
          return promise.then(function (response) {
            self.HistoryItems = response.data.HistoryItems;
          });
        };
        self.deleteHistoryItem = function (item) {
          var url = webApiBaseUrl + '/api/importstatetestdata/DeleteIntvHistoryItem';
          var paramObj = { id: item.Id };
          var promise = $http.post(url, paramObj);
          return promise;
        };
        self.downloadImportFile = function (item) {
          var url = webApiBaseUrl + '/api/importstatetestdata/GetImportFile';
          var paramObj = { value: item.UploadedFileName };
          var promise = $http.post(url, paramObj, { responseType: 'arraybuffer' }).then(function (response) {
              var data = new Blob([response.data]);
              FileSaver.saveAs(data, 'originalimportfile.csv');
            });
        };
        self.downloadImportLog = function (item) {
          var paramObj = { id: item.Id };
          var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/GetIntvHistoryLog', paramObj);
          promise.then(function (response) {
            var data = new Blob([response.data.Result], { type: 'text/csv;charset=ANSI' });
            FileSaver.saveAs(data, 'importlog.txt');
          });
        };
      };
      return InterventionDataImportManager;
    }
  ]).controller('InterventionDataImportController', [
    '$scope',
    '$http',
    'webApiBaseUrl',
    'progressLoader',
    'InterventionDataImportManager',
    'nsFilterOptionsService',
    'FileSaver',
    '$timeout',
    'nsPinesService',
    '$interval',
    '$bootbox',
    function ($scope, $http, webApiBaseUrl, progressLoader, InterventionDataImportManager, nsFilterOptionsService, FileSaver, $timeout, nsPinesService, $interval, $bootbox) {
      $scope.settings = {
        uploadComplete: false,
        hasFiles: false
      };
      $scope.dataMgr = new InterventionDataImportManager();
      $scope.filterOptions = nsFilterOptionsService.options;
      $scope.theFiles = [];
      // watch assessmentfield chooser and reload template when it changes
      // load template
      $scope.$watch('filterOptions.selectedInterventionTest', function (newValue, oldValue) {
        if (!angular.equals(newValue, oldValue) && newValue != null) {
          progressLoader.start();
          progressLoader.set(50);
          $scope.dataMgr.LoadAssessmentTemplate($scope.filterOptions.selectedInterventionTest.id).then(function (response) {
            progressLoader.end();
          });
        }
      }, true);
      $scope.downloadImportFile = function (item) {
        $scope.dataMgr.downloadImportFile(item);
      };
      $scope.deleteHistoryItem = function (item) {
        $bootbox.confirm('Are you sure you want to delete this job?  <b>Note:</b> If it has not yet been processed, it will be cancelled.', function (response) {
          if (response) {
            progressLoader.start();
            progressLoader.set(50);
            $scope.dataMgr.deleteHistoryItem(item).then(function (response) {
              nsPinesService.dataDeletedSuccessfully();
              reloadHistoryTable();
              progressLoader.end();
            });
          }
        });
      };
      $scope.downloadImportLog = function (item) {
        $scope.dataMgr.downloadImportLog(item);
      };
      $scope.getStatusClass = function (item) {
        if (item.Status == 'Awaiting processing') {
          return 'badge-default';
        } else if (item.Status == 'Complete') {
          return 'badge-success';
        } else if (item.Status == 'Processing') {
          return 'badge-primary';
        } else if (item.Status == 'Error') {
          return 'badge-danger';
        } else {
          return 'badge-warning';
        }
      };
      var reloadHistoryTable = function () {
        $scope.dataMgr.LoadImportHistory();
      };
      reloadHistoryTable();
      // reload table every 5 seconds
      var reloadInterval = $interval(reloadHistoryTable, 5000);
      // here is where the cleanup happens
      $scope.$on('$destroy', function () {
        $interval.cancel(reloadInterval);
      });
      $scope.getTemplate = function () {
        var paramObj = { id: $scope.filterOptions.selectedInterventionTest.id };
        var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/getbenchmarktestexporttemplate', paramObj);
        promise.then(function (response) {
          var data = new Blob([response.data.Result], { type: 'text/csv;charset=ANSI' });
          FileSaver.saveAs(data, 'template.csv');
        });
      };
      $scope.upload = function (theFiles) {
        var formData = new FormData();
        formData.append('SchoolYear', $scope.filterOptions.selectedSchoolYear.id);
        formData.append('AssessmentId', $scope.filterOptions.selectedInterventionTest.id);
        formData.append('InterventionGroupId', $scope.filterOptions.selectedInterventionGroup.id);
        formData.append('RecorderId', $scope.filterOptions.quickSearchStaff.id);
        angular.forEach(theFiles, function (file) {
          formData.append(file.name, file);
        });
        var paramObj = {};
        // start loader
        progressLoader.start();
        progressLoader.set(50);
        var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/uploadinterventiontestcsv', formData, {
            transformRequest: angular.identity,
            headers: { 'Content-Type': undefined }
          }).then(function (response) {
            // end loader
            progressLoader.end();
            //$scope.errors = [];
            $scope.settings.LogItems = response.data.LogItems;
            // show success
            $timeout(function () {
              $('#formReset').click();
            }, 100);
            //$scope.theFiles.length = 0;
            //$scope.settings.hasFiles = false;
            $scope.settings.uploadComplete = true;
            if ($scope.settings.LogItems.length > 0) {
              nsPinesService.buildMessage('Data Import Error', 'There were one or more errors in your import file.', 'error');
            } else {
              nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
            }
          }, function (err) {
            progressLoader.end();
            $scope.settings.uploadComplete = true;
          });
      };
    }
  ]).controller('UpdateCalculatedFieldsController', [
    '$scope',
    '$http',
    'webApiBaseUrl',
    'progressLoader',
    function ($scope, $http, webApiBaseUrl, progressLoader) {
      $scope.settings = { uploadComplete: false };
      $scope.run = function () {
        progressLoader.start();
        progressLoader.set(50);
        $http.post(webApiBaseUrl + '/api/importstatetestdata/updatecalculatedfields').then(function (response) {
          $scope.settings.uploadComplete = true;
          progressLoader.end();
        });
      };
    }
  ]);
  ;  //MNStateTestDataImportFinalController.$inject = ['$scope', '$q', '$http', 'nsPinesService', '$location', '$filter', 'webApiBaseUrl', 'nsFilterOptionsService', 'nsSelect2RemoteOptions', 'progressLoader', 'FileSaver', 'Blob', '$timeout'];
     //function MNStateTestDataImportFinalController($scope, $q, $http, nsPinesService, $location, $filter, webApiBaseUrl, nsFilterOptionsService, nsSelect2RemoteOptions, progressLoader, FileSaver, Blob, $timeout) {
     //    $scope.filterOptions = nsFilterOptionsService.options;
     //    $scope.getTeacherRemoteOptions = nsSelect2RemoteOptions.TeacherRemoteOptions;
     //    $scope.theFiles = [];
     //    $scope.settings = { hasFiles: false, uploadComplete: false };
     //    $scope.LogItems = [];
     //    // get scoped warnings and download
     //    $scope.downloadResult = function () {
     //        var text = '';
     //        for (var i = 0; i < $scope.LogItems.length; i++) {
     //            text += $scope.LogItems[i] + '\r\n';
     //        }
     //        var data = new Blob([text], { type: 'text/plain;charset=ANSI' });
     //        FileSaver.saveAs(data, 'results.txt');
     //        $scope.settings.uploadComplete = false;
     //    }
     //    $scope.upload = function (theFiles) {
     //        var formData = new FormData();
     //        angular.forEach(theFiles, function (file) {
     //            formData.append(file.name, file);
     //        });
     //        var paramObj = {};
     //        // start loader
     //        progressLoader.start();
     //        progressLoader.set(50);
     //        var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/uploadmnfinalcsv', formData, {
     //            transformRequest: angular.identity,
     //            headers: { 'Content-Type': undefined }
     //        }).then(function (response) {
     //            // end loader
     //            progressLoader.end();
     //            $scope.errors = [];
     //            $scope.LogItems = response.data.LogItems;
     //            // show success
     //            $timeout(function () {
     //                $('#formReset').click();
     //            }, 100);
     //            //$scope.theFiles.length = 0;
     //            //$scope.settings.hasFiles = false;
     //            $scope.settings.uploadComplete = true;
     //            nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
     //        });
     //    }
     //}
     //MNStateTestDataImportController.$inject = ['$scope', '$q', '$http', 'nsPinesService', '$location', '$filter', 'webApiBaseUrl', 'nsFilterOptionsService', 'nsSelect2RemoteOptions', 'progressLoader', 'FileSaver', 'Blob', '$timeout'];
     //function MNStateTestDataImportController($scope, $q, $http, nsPinesService, $location, $filter, webApiBaseUrl, nsFilterOptionsService, nsSelect2RemoteOptions, progressLoader, FileSaver, Blob, $timeout) {
     //    $scope.filterOptions = nsFilterOptionsService.options;
     //    $scope.getTeacherRemoteOptions = nsSelect2RemoteOptions.TeacherRemoteOptions;
     //    $scope.theFiles = [];
     //    $scope.settings = { hasFiles: false, uploadComplete: false };
     //    $scope.LogItems = [];
     //    // get scoped warnings and download
     //    $scope.downloadResult = function () {
     //        var text = '';
     //        for (var i = 0; i < $scope.LogItems.length; i++) {
     //            text += $scope.LogItems[i] + '\r\n';
     //        }
     //        var data = new Blob([text], { type: 'text/plain;charset=ANSI' });
     //        FileSaver.saveAs(data, 'results.txt');
     //        $scope.settings.uploadComplete = false;
     //    }
     //    $scope.upload = function (theFiles) {
     //        var formData = new FormData();
     //        angular.forEach(theFiles, function (file) {
     //            formData.append(file.name, file);
     //        });
     //        var paramObj = {};
     //        // start loader
     //        progressLoader.start();
     //        progressLoader.set(50);
     //        var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/uploadmncsv', formData, {
     //            transformRequest: angular.identity,
     //            headers: { 'Content-Type': undefined }
     //        }).then(function (response) {
     //            // end loader
     //            progressLoader.end();
     //            $scope.errors = [];
     //            $scope.LogItems = response.data.LogItems;
     //            // show success
     //            $timeout(function () {
     //                $('#formReset').click();
     //            }, 100);
     //            //$scope.theFiles.length = 0;
     //            //$scope.settings.hasFiles = false;
     //            $scope.settings.uploadComplete = true;
     //            nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
     //        });
     //    }
     //}
     //BenchmarkDataImportController.$inject = ['$scope', '$q', '$http', 'nsPinesService', '$location', '$filter', 'webApiBaseUrl', 'nsFilterOptionsService', 'nsSelect2RemoteOptions', 'progressLoader', 'FileSaver', 'Blob', '$timeout'];
     //function BenchmarkDataImportController($scope, $q, $http, nsPinesService, $location, $filter, webApiBaseUrl, nsFilterOptionsService, nsSelect2RemoteOptions, progressLoader, FileSaver, Blob, $timeout) {
     //    $scope.filterOptions = nsFilterOptionsService.options;
     //    $scope.getTeacherRemoteOptions = nsSelect2RemoteOptions.TeacherRemoteOptions;
     //    $scope.theFiles = [];
     //    $scope.settings = { hasFiles: false, uploadComplete: false };
     //    $scope.LogItems = [];
     //    // get scoped warnings and download
     //    $scope.downloadResult = function () {
     //        var text = '';
     //        for (var i = 0; i < $scope.LogItems.length; i++) {
     //            text += $scope.LogItems[i] + '\r\n';
     //        }
     //        var data = new Blob([text], { type: 'text/plain;charset=ANSI' });
     //        FileSaver.saveAs(data, 'results.txt');
     //        $scope.settings.uploadComplete = false;
     //    }
     //    $scope.upload = function (theFiles) {
     //        var formData = new FormData();
     //        formData.append("AssessmentId", 63);
     //        formData.append("BenchmarkDateId", $scope.filterOptions.selectedBenchmarkDate.id);
     //        formData.append("RecorderId", $scope.settings.Recorder.id);
     //        formData.append("SchoolYear", $scope.filterOptions.selectedSchoolYear.id);
     //        angular.forEach(theFiles, function (file) {
     //            formData.append(file.name, file);
     //        });
     //        var paramObj = {};
     //        // start loader
     //        progressLoader.start();
     //        progressLoader.set(50);
     //        var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/uploadcsv', formData, {
     //            transformRequest: angular.identity,
     //            headers: { 'Content-Type': undefined }
     //        }).then(function (response) {
     //            // end loader
     //            progressLoader.end();
     //            $scope.errors = [];
     //            $scope.LogItems = response.data.LogItems;
     //            // show success
     //            $timeout(function () {
     //                $('#formReset').click();
     //            }, 100);
     //            //$scope.theFiles.length = 0;
     //            //$scope.settings.hasFiles = false;
     //            $scope.settings.uploadComplete = true;
     //            nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
     //        });
     //    }
     //}
}());
(function () {
  'use strict';
  angular.module('observationSummaryModule', []).controller('ObservationSummaryClassController', ObservationSummaryClassController).controller('ObservationSummaryFilteredController', ObservationSummaryFilteredController).factory('NSObservationSummarySectionManager', [
    '$http',
    'webApiBaseUrl',
    'nsLookupFieldService',
    'spinnerService',
    function ($http, webApiBaseUrl, nsLookupFieldService, spinnerService) {
      var NSObservationSummarySectionManager = function () {
        var self = this;
        self.initialize = function () {
        };
        self.LoadData = function (sectionId, tddId) {
          spinnerService.show('tableSpinner');
          var url = webApiBaseUrl + '/api/assessment/GetClassObservationSummary/';
          //var paramObj = {}
          var summaryData = $http.get(url + sectionId + '/' + tddId);
          self.LookupLists = [];
          self.Scores = [];
          self.BenchmarksByGrade = [];
          return summaryData.then(function (response) {
            angular.extend(self, response.data);
            self.LookupLists = nsLookupFieldService.LookupFieldsArray;
            //if (self.LookupLists === null) self.LookupLists = [];
            if (self.Scores === null)
              self.Scores = [];
            if (self.BenchmarksByGrade === null)
              self.BenchmarksByGrade = [];
            return;
          }).finally(function (response) {
            spinnerService.hide('tableSpinner');
          });
        };
      };
      return NSObservationSummarySectionManager;
    }
  ]).directive('nsObservationSummaryTm', [
    '$routeParams',
    '$compile',
    '$templateCache',
    '$http',
    'nsFilterOptionsService',
    '$filter',
    'NSObservationSummaryTeamMeetingManager',
    'NSSortManager',
    '$timeout',
    function ($routeParams, $compile, $templateCache, $http, nsFilterOptionsService, $filter, NSObservationSummaryTeamMeetingManager, NSSortManager, $timeout) {
      return {
        restrict: 'E',
        templateUrl: 'templates/observation-summary-tm.html',
        scope: {
          selectedTeamMeetingId: '=',
          selectedStaffId: '=',
          selectedBenchmarkDateId: '='
        },
        link: function (scope, element, attr) {
          scope.observationSummaryManager = new NSObservationSummaryTeamMeetingManager();
          scope.filterOptions = nsFilterOptionsService.options;
          scope.manualSortHeaders = {};
          scope.manualSortHeaders.firstNameHeaderClass = 'fa';
          scope.manualSortHeaders.lastNameHeaderClass = 'fa';
          scope.sortArray = [];
          scope.headerClassArray = [];
          scope.allSelected = false;
          scope.sortMgr = new NSSortManager();
          scope.$watch('selectedStaffId', function (newVal, oldVal) {
            if (newVal !== oldVal) {
              scope.observationSummaryManager.LoadData(scope.selectedTeamMeetingId, scope.selectedBenchmarkDateId, scope.selectedStaffId).then(function () {
                attachFieldsCallback();
              });
            }
          });
          scope.$on('NSFieldsUpdated', function (event, data) {
            scope.observationSummaryManager.LoadData(scope.selectedTeamMeetingId, scope.selectedBenchmarkDateId, scope.selectedStaffId).then(function () {
              attachFieldsCallback();
            });
          });
          var attachFieldsCallback = function () {
            // initialize the sort manager now that the data has been loaded
            scope.sortMgr.initialize(scope.manualSortHeaders, scope.sortArray, scope.headerClassArray, 'OSFieldResults', scope.observationSummaryManager.Scores);
            for (var j = 0; j < scope.observationSummaryManager.Scores.StudentResults.length; j++) {
              for (var k = 0; k < scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults.length; k++) {
                for (var i = 0; i < scope.observationSummaryManager.Scores.Fields.length; i++) {
                  if (scope.observationSummaryManager.Scores.Fields[i].DatabaseColumn == scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].DbColumn) {
                    //scope.observationSummaryManager.Scores[j].FieldResults[k].Field = $scope.fields[i];
                    // set display value
                    if (scope.observationSummaryManager.Scores.Fields[i].FieldType === 'DropdownFromDB') {
                      for (var p = 0; p < scope.observationSummaryManager.LookupLists.length; p++) {
                        if (scope.observationSummaryManager.LookupLists[p].LookupColumnName === scope.observationSummaryManager.Scores.Fields[i].LookupFieldName) {
                          // now find the specifc value that matches
                          for (var y = 0; y < scope.observationSummaryManager.LookupLists[p].LookupFields.length; y++) {
                            if (scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].IntValue === scope.observationSummaryManager.LookupLists[p].LookupFields[y].FieldSpecificId) {
                              scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].DisplayValue = scope.observationSummaryManager.LookupLists[p].LookupFields[y].FieldValue;
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
            // if there are no records, broadcast it
            if (!angular.isDefined(scope.observationSummaryManager.Scores.StudentResults) || scope.observationSummaryManager.Scores.StudentResults.length === 0) {
              $timeout(function () {
                scope.$emit('nsNoRecords');
              }, 750);
            }
          };
          scope.observationSummaryManager.LoadData(scope.selectedTeamMeetingId, scope.selectedBenchmarkDateId, scope.selectedStaffId).then(function () {
            attachFieldsCallback();
          });
          // delegate sorting to the sort manager
          scope.sort = function (column) {
            scope.sortMgr.sort(column);
          };
          function getIntColor(gradeId, studentFieldScore, fieldValue) {
            var benchmarkArray = null;
            for (var i = 0; i < scope.observationSummaryManager.BenchmarksByGrade.length; i++) {
              if (scope.observationSummaryManager.BenchmarksByGrade[i].GradeId == gradeId) {
                benchmarkArray = scope.observationSummaryManager.BenchmarksByGrade[i];
              }
              if (benchmarkArray != null) {
                for (var j = 0; j < benchmarkArray.Benchmarks.length; j++) {
                  if (benchmarkArray.Benchmarks[j].DbColumn === studentFieldScore.DbColumn && benchmarkArray.Benchmarks[j].AssessmentId === studentFieldScore.AssessmentId) {
                    if (fieldValue != null) {
                      // not defined yet
                      //if (studentFieldScore.DecimalValue === $scope.Benchmarks[i].MaxScore) {
                      //	return 'obsGreen';
                      //}
                      if (fieldValue >= benchmarkArray.Benchmarks[j].Exceeds && benchmarkArray.Benchmarks[j].Exceeds != null) {
                        return 'obsBlue';
                      }
                      if (fieldValue >= benchmarkArray.Benchmarks[j].Meets && benchmarkArray.Benchmarks[j].Meets != null) {
                        return 'obsGreen';
                      }
                      if (fieldValue >= benchmarkArray.Benchmarks[j].Approaches && benchmarkArray.Benchmarks[j].Approaches != null) {
                        return 'obsYellow';
                      }
                      if (fieldValue < benchmarkArray.Benchmarks[j].Approaches && benchmarkArray.Benchmarks[j].Approaches != null) {
                        return 'obsRed';
                      }
                    }
                  }
                }
              }
            }
            return '';
          }
          scope.getBackgroundClass = function (gradeId, studentFieldScore) {
            switch (studentFieldScore.ColumnType) {
            case 'Textfield':
              return '';
              break;
            case 'DecimalRange':
              return getDecimalColor(gradeId, studentFieldScore, studentFieldScore.DecimalValue);
              break;
            case 'DropdownRange':
              return getIntColor(gradeId, studentFieldScore, studentFieldScore.IntValue);
              break;
            case 'DropdownFromDB':
              return getIntColor(gradeId, studentFieldScore, studentFieldScore.IntValue);
              break;
            case 'CalculatedFieldClientOnly':
              return '';
              break;
            case 'CalculatedFieldDbBacked':
              return getIntColor(gradeId, studentFieldScore, studentFieldScore.IntValue);
              break;
            case 'CalculatedFieldDbOnly':
              return '';
              break;
            default:
              return '';
              break;
            }
            return '';
          };
        }
      };
    }
  ]).directive('nsObservationSummaryFiltered', [
    '$routeParams',
    '$compile',
    '$templateCache',
    '$http',
    'nsFilterOptionsService',
    '$filter',
    'NSObservationSummarySectionManager',
    'NSSortManager',
    'observationSummaryAssessmentFieldChooserSvc',
    'nsPinesService',
    '$timeout',
    function ($routeParams, $compile, $templateCache, $http, nsFilterOptionsService, $filter, NSObservationSummarySectionManager, NSSortManager, observationSummaryAssessmentFieldChooserSvc, nsPinesService, $timeout) {
      return {
        restrict: 'E',
        templateUrl: 'templates/observation-summary-filtered-button.html',
        scope: { groupFactory: '=' },
        link: function (scope, element, attr) {
          scope.observationSummaryManager = scope.groupFactory;
          scope.filterOptions = scope.groupFactory.options;
          scope.manualSortHeaders = {};
          scope.manualSortHeaders.studentNameHeaderClass = 'fa';
          scope.manualSortHeaders.gradeNameHeaderClass = 'fa';
          scope.manualSortHeaders.schoolNameHeaderClass = 'fa';
          scope.manualSortHeaders.teacherNameHeaderClass = 'fa';
          scope.sortArray = [];
          scope.headerClassArray = [];
          scope.allSelected = false;
          scope.sortMgr = new NSSortManager();
          scope.fieldChooser = observationSummaryAssessmentFieldChooserSvc;
          scope.settings = { graphGenerated: false };
          // infinite scroll function call
          scope.loadMoreRecords = function () {
            $timeout(function () {
              scope.observationSummaryManager.busy = true;
              scope.observationSummaryManager.loadOSInfinityRecords();
              // attachFieldsCallback();
              scope.observationSummaryManager.busy = false;
            }, 100);
          };
          scope.generateGraph = function () {
            scope.observationSummaryManager.loadOSData().then(function (response) {
              attachFieldsCallback();
              scope.settings.graphGenerated = true;
            });
          };
          scope.hideField = function (field) {
            observationSummaryAssessmentFieldChooserSvc.hideField(field).then(function (response) {
              scope.observationSummaryManager.loadOSData().then(function (response) {
                attachFieldsCallback();
              });
            });
          };
          scope.hideAssessment = function (assessment) {
            observationSummaryAssessmentFieldChooserSvc.hideAssessment(assessment).then(function (response) {
              scope.observationSummaryManager.loadOSData().then(function (response) {
                attachFieldsCallback();
              });
            });
          };
          scope.$on('NSFieldsUpdated', function (event, data) {
            scope.observationSummaryManager.loadOSData().then(function (response) {
              attachFieldsCallback();
            });
          });
          scope.changeTeamMeetingStudentSelection = function (studentResult) {
            if (studentResult.selected) {
              // add student to the collection
              scope.teamMeetingStudents.push({
                ID: -1,
                TeamMeetingID: -1,
                SchoolID: -1,
                StaffID: scope.selectedStaffId,
                SectionID: scope.selectedSectionId,
                StudentID: studentResult.StudentId,
                Notes: null
              });
              for (var i = 0; i < scope.teamMeetingSections.length; i++) {
                if (scope.teamMeetingSections[i].Id == scope.selectedSectionId) {
                  scope.teamMeetingSections[i].NumStudents++;
                }
              }
            } else {
              // remove student from the collection
              // TODO: Note, if a student is in more than one class, he will be removed from ALL classes with this logic.  Its probably fine, 
              // but we can do something else if need be
              for (var n = 0; n < scope.teamMeetingStudents.length; n++) {
                if (scope.teamMeetingStudents[n].StudentID === studentResult.StudentId) {
                  scope.teamMeetingStudents.splice(n, 1);
                }
              }
              for (var i = 0; i < scope.teamMeetingSections.length; i++) {
                if (scope.teamMeetingSections[i].Id == scope.selectedSectionId) {
                  scope.teamMeetingSections[i].NumStudents--;
                }
              }
            }
          };
          var attachFieldsCallback = function () {
            // initialize the sort manager now that the data has been loaded
            scope.sortMgr.initialize(scope.manualSortHeaders, scope.sortArray, scope.headerClassArray, 'OSFieldResults', scope.observationSummaryManager.Scores);
            if (!angular.isDefined(scope.observationSummaryManager.Scores.StudentResults)) {
              return;
            }
            for (var j = 0; j < scope.observationSummaryManager.Scores.StudentResults.length; j++) {
              for (var k = 0; k < scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults.length; k++) {
                for (var i = 0; i < scope.observationSummaryManager.Scores.Fields.length; i++) {
                  if (scope.observationSummaryManager.Scores.Fields[i].DatabaseColumn == scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].DbColumn) {
                    scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].Field = angular.copy(scope.observationSummaryManager.Scores.Fields[i]);
                  }
                }
              }
              // now select the students that should be selected from the team meeeting
              if (scope.teamMeetingStudents) {
                for (var n = 0; n < scope.teamMeetingStudents.length; n++) {
                  if (scope.teamMeetingStudents[n].StudentID === scope.observationSummaryManager.Scores.StudentResults[j].StudentId) {
                    // increment numstudents too
                    scope.observationSummaryManager.Scores.StudentResults[j].selected = true;
                  }
                }
              }
            }
          };
          scope.observationSummaryManager.loadOSData().then(function (response) {
            attachFieldsCallback();
          });
          // delegate sorting to the sort manager
          scope.sort = function (column) {
            scope.sortMgr.sort(column);
          };
          function getIntColor(gradeId, studentFieldScore, fieldValue) {
            var benchmarkArray = null;
            for (var i = 0; i < scope.observationSummaryManager.BenchmarksByGrade.length; i++) {
              // if this is a state test, use the statetestgrade instead of the one for the overall result
              if (studentFieldScore.TestTypeId == 3) {
                gradeId = studentFieldScore.StateGradeId;
              }
              if (scope.observationSummaryManager.BenchmarksByGrade[i].GradeId == gradeId) {
                benchmarkArray = scope.observationSummaryManager.BenchmarksByGrade[i];
              }
              if (benchmarkArray != null) {
                for (var j = 0; j < benchmarkArray.Benchmarks.length; j++) {
                  if (benchmarkArray.Benchmarks[j].DbColumn === studentFieldScore.DbColumn && benchmarkArray.Benchmarks[j].AssessmentId === studentFieldScore.AssessmentId) {
                    if (fieldValue != null) {
                      // not defined yet
                      //if (studentFieldScore.DecimalValue === $scope.Benchmarks[i].MaxScore) {
                      //	return 'obsPerfect';
                      //}
                      if (fieldValue >= benchmarkArray.Benchmarks[j].Exceeds && benchmarkArray.Benchmarks[j].Exceeds != null) {
                        return 'obsBlue';
                      }
                      if (fieldValue >= benchmarkArray.Benchmarks[j].Meets && benchmarkArray.Benchmarks[j].Meets != null) {
                        return 'obsGreen';
                      }
                      if (fieldValue >= benchmarkArray.Benchmarks[j].Approaches && benchmarkArray.Benchmarks[j].Approaches != null) {
                        return 'obsYellow';
                      }
                      if (fieldValue < benchmarkArray.Benchmarks[j].Approaches && benchmarkArray.Benchmarks[j].Approaches != null) {
                        return 'obsRed';
                      }
                    }
                  }
                }
              }
            }
            return '';
          }
          scope.getBackgroundClass = function (gradeId, studentFieldScore) {
            switch (studentFieldScore.ColumnType) {
            case 'Textfield':
              return '';
              break;
            case 'DecimalRange':
              return getIntColor(gradeId, studentFieldScore, studentFieldScore.DecimalValue);
              break;
            case 'DropdownRange':
              return getIntColor(gradeId, studentFieldScore, studentFieldScore.IntValue);
              break;
            case 'DropdownFromDB':
              return getIntColor(gradeId, studentFieldScore, studentFieldScore.IntValue);
              break;
            case 'CalculatedFieldClientOnly':
              return '';
              break;
            case 'CalculatedFieldDbBacked':
              return getIntColor(gradeId, studentFieldScore, studentFieldScore.IntValue);
              break;
            case 'CalculatedFieldDbOnly':
              return '';
              break;
            default:
              return '';
              break;
            }
            return '';
          };
        }
      };
    }
  ]).directive('nsObservationSummarySection', [
    '$routeParams',
    '$compile',
    '$templateCache',
    '$http',
    'nsFilterOptionsService',
    '$filter',
    'NSObservationSummarySectionManager',
    'NSSortManager',
    'observationSummaryAssessmentFieldChooserSvc',
    function ($routeParams, $compile, $templateCache, $http, nsFilterOptionsService, $filter, NSObservationSummarySectionManager, NSSortManager, observationSummaryAssessmentFieldChooserSvc) {
      return {
        restrict: 'E',
        templateUrl: 'templates/observation-summary-section.html',
        scope: {
          selectedSectionId: '=',
          selectedStaffId: '=',
          selectedBenchmarkDateId: '=',
          selectedAssessmentIds: '=',
          showCheckboxes: '=',
          teamMeetingStudents: '=',
          teamMeetingSections: '='
        },
        link: function (scope, element, attr) {
          scope.observationSummaryManager = new NSObservationSummarySectionManager();
          scope.filterOptions = nsFilterOptionsService.options;
          scope.manualSortHeaders = {};
          scope.manualSortHeaders.studentNameHeaderClass = 'fa';
          scope.sortArray = [];
          scope.headerClassArray = [];
          scope.allSelected = false;
          scope.sortMgr = new NSSortManager();
          scope.fieldChooser = observationSummaryAssessmentFieldChooserSvc;
          scope.hideField = function (field) {
            observationSummaryAssessmentFieldChooserSvc.hideField(field).then(function (response) {
              scope.observationSummaryManager.LoadData(scope.selectedSectionId, scope.selectedBenchmarkDateId).then(function (response) {
                attachFieldsCallback();
              });
            });
          };
          scope.hideAssessment = function (assessment) {
            observationSummaryAssessmentFieldChooserSvc.hideAssessment(assessment).then(function (response) {
              scope.observationSummaryManager.LoadData(scope.selectedSectionId, scope.selectedBenchmarkDateId).then(function (response) {
                attachFieldsCallback();
              });
            });
          };
          scope.$on('NSFieldsUpdated', function (event, data) {
            scope.observationSummaryManager.LoadData(scope.selectedSectionId, scope.selectedBenchmarkDateId).then(function (response) {
              attachFieldsCallback();
            });
          });
          scope.$watch('selectedSectionId', function (newValue, oldValue) {
            if (newValue != oldValue && newValue != null) {
              scope.observationSummaryManager.LoadData(scope.selectedSectionId, scope.selectedBenchmarkDateId).then(function (response) {
                attachFieldsCallback();
              });
            }
          });
          scope.changeTeamMeetingStudentSelection = function (studentResult) {
            if (studentResult.selected) {
              // add student to the collection
              scope.teamMeetingStudents.push({
                ID: -1,
                TeamMeetingID: -1,
                SchoolID: -1,
                StaffID: scope.selectedStaffId,
                SectionID: scope.selectedSectionId,
                StudentID: studentResult.StudentId,
                Notes: null
              });
              for (var i = 0; i < scope.teamMeetingSections.length; i++) {
                if (scope.teamMeetingSections[i].Id == scope.selectedSectionId) {
                  scope.teamMeetingSections[i].NumStudents++;
                }
              }
            } else {
              // remove student from the collection
              // TODO: Note, if a student is in more than one class, he will be removed from ALL classes with this logic.  Its probably fine, 
              // but we can do something else if need be
              for (var n = 0; n < scope.teamMeetingStudents.length; n++) {
                if (scope.teamMeetingStudents[n].StudentID === studentResult.StudentId) {
                  scope.teamMeetingStudents.splice(n, 1);
                }
              }
              for (var i = 0; i < scope.teamMeetingSections.length; i++) {
                if (scope.teamMeetingSections[i].Id == scope.selectedSectionId) {
                  scope.teamMeetingSections[i].NumStudents--;
                }
              }
            }
          };
          var attachFieldsCallback = function () {
            // initialize the sort manager now that the data has been loaded
            scope.sortMgr.initialize(scope.manualSortHeaders, scope.sortArray, scope.headerClassArray, 'OSFieldResults', scope.observationSummaryManager.Scores);
            for (var j = 0; j < scope.observationSummaryManager.Scores.StudentResults.length; j++) {
              for (var k = 0; k < scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults.length; k++) {
                for (var i = 0; i < scope.observationSummaryManager.Scores.Fields.length; i++) {
                  if (scope.observationSummaryManager.Scores.Fields[i].DatabaseColumn == scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].DbColumn) {
                    scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].Field = angular.copy(scope.observationSummaryManager.Scores.Fields[i]);  // set display value
                                                                                                                                                                        //if (scope.observationSummaryManager.Scores.Fields[i].FieldType === "DropdownFromDB") {
                                                                                                                                                                        //    for (var p = 0; p < scope.observationSummaryManager.LookupLists.length; p++) {
                                                                                                                                                                        //        if (scope.observationSummaryManager.LookupLists[p].LookupColumnName === scope.observationSummaryManager.Scores.Fields[i].LookupFieldName) {
                                                                                                                                                                        //            // now find the specifc value that matches
                                                                                                                                                                        //            for (var y = 0; y < scope.observationSummaryManager.LookupLists[p].LookupFields.length; y++) {
                                                                                                                                                                        //                if (scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].IntValue === scope.observationSummaryManager.LookupLists[p].LookupFields[y].FieldSpecificId) {
                                                                                                                                                                        //                    scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].DisplayValue = scope.observationSummaryManager.LookupLists[p].LookupFields[y].FieldValue;
                                                                                                                                                                        //                }
                                                                                                                                                                        //            }
                                                                                                                                                                        //        }
                                                                                                                                                                        //    }
                                                                                                                                                                        //}
                  }
                }
              }
              // now select the students that should be selected from the team meeeting
              if (scope.teamMeetingStudents) {
                for (var n = 0; n < scope.teamMeetingStudents.length; n++) {
                  if (scope.teamMeetingStudents[n].StudentID === scope.observationSummaryManager.Scores.StudentResults[j].StudentId) {
                    // increment numstudents too
                    scope.observationSummaryManager.Scores.StudentResults[j].selected = true;
                  }
                }
              }
            }
          };
          scope.observationSummaryManager.LoadData(scope.selectedSectionId, scope.selectedBenchmarkDateId).then(function (response) {
            attachFieldsCallback();
          });
          // delegate sorting to the sort manager
          scope.sort = function (column) {
            scope.sortMgr.sort(column);
          };
          //function getDecimalColor(gradeId, studentFieldScore) {
          //    var benchmarkArray = null;
          //    for (var i = 0; i < scope.observationSummaryManager.BenchmarksByGrade.length; i++) {
          //        if (scope.observationSummaryManager.BenchmarksByGrade[i].GradeId == gradeId) {
          //            benchmarkArray = scope.observationSummaryManager.BenchmarksByGrade[i];
          //        }
          //        if (benchmarkArray != null) {
          //            for (var j = 0; j < benchmarkArray.Benchmarks.length; j++) {
          //                if (benchmarkArray.Benchmarks[j].DbColumn === studentFieldScore.DbColumn && benchmarkArray.Benchmarks[j].AssessmentId === studentFieldScore.AssessmentId) {
          //                    if (studentFieldScore.DecimalValue != null) {
          //                        // not defined yet
          //                        //if (studentFieldScore.DecimalValue === $scope.Benchmarks[i].MaxScore) {
          //                        //	return 'obsGreen';
          //                        //}
          //                        if (studentFieldScore.DecimalValue >= benchmarkArray.Benchmarks[j].Decimal80) {
          //                            return 'obsBlue';
          //                        }
          //                        if (studentFieldScore.DecimalValue >= benchmarkArray.Benchmarks[j].DecimalMean) {
          //                            return '';
          //                        }
          //                        if (studentFieldScore.DecimalValue >= benchmarkArray.Benchmarks[j].Decimal20) {
          //                            return 'obsYellow';
          //                        }
          //                        if (studentFieldScore.DecimalValue <= benchmarkArray.Benchmarks[j].Decimal20) {
          //                            return 'obsRed';
          //                        }
          //                    }
          //                }
          //            }
          //        }
          //    }
          //    return '';
          //}
          function getIntColor(gradeId, studentFieldScore, fieldValue) {
            var benchmarkArray = null;
            for (var i = 0; i < scope.observationSummaryManager.BenchmarksByGrade.length; i++) {
              // if this is a state test, use the statetestgrade instead of the one for the overall result
              if (studentFieldScore.TestTypeId == 3) {
                gradeId = studentFieldScore.StateGradeId;
              }
              if (scope.observationSummaryManager.BenchmarksByGrade[i].GradeId == gradeId) {
                benchmarkArray = scope.observationSummaryManager.BenchmarksByGrade[i];
              }
              if (benchmarkArray != null) {
                for (var j = 0; j < benchmarkArray.Benchmarks.length; j++) {
                  if (benchmarkArray.Benchmarks[j].DbColumn === studentFieldScore.DbColumn && benchmarkArray.Benchmarks[j].AssessmentId === studentFieldScore.AssessmentId) {
                    if (fieldValue != null) {
                      // not defined yet
                      //if (studentFieldScore.DecimalValue === $scope.Benchmarks[i].MaxScore) {
                      //	return 'obsPerfect';
                      //}
                      if (fieldValue >= benchmarkArray.Benchmarks[j].Exceeds && benchmarkArray.Benchmarks[j].Exceeds != null) {
                        return 'obsBlue';
                      }
                      if (fieldValue >= benchmarkArray.Benchmarks[j].Meets && benchmarkArray.Benchmarks[j].Meets != null) {
                        return 'obsGreen';
                      }
                      if (fieldValue >= benchmarkArray.Benchmarks[j].Approaches && benchmarkArray.Benchmarks[j].Approaches != null) {
                        return 'obsYellow';
                      }
                      if (fieldValue < benchmarkArray.Benchmarks[j].Approaches && benchmarkArray.Benchmarks[j].Approaches != null) {
                        return 'obsRed';
                      }
                    }
                  }
                }
              }
            }
            return '';
          }
          scope.getBackgroundClass = function (gradeId, studentFieldScore) {
            switch (studentFieldScore.ColumnType) {
            case 'Textfield':
              return '';
              break;
            case 'DecimalRange':
              return getIntColor(gradeId, studentFieldScore, studentFieldScore.DecimalValue);
              break;
            case 'DropdownRange':
              return getIntColor(gradeId, studentFieldScore, studentFieldScore.IntValue);
              break;
            case 'DropdownFromDB':
              return getIntColor(gradeId, studentFieldScore, studentFieldScore.IntValue);
              break;
            case 'CalculatedFieldClientOnly':
              return '';
              break;
            case 'CalculatedFieldDbBacked':
              return getIntColor(gradeId, studentFieldScore, studentFieldScore.IntValue);
              break;
            case 'CalculatedFieldDbOnly':
              return '';
              break;
            default:
              return '';
              break;
            }
            return '';
          };
        }
      };
    }
  ]);
  /* Movies List Controller  */
  ObservationSummaryClassController.$inject = [
    '$scope',
    'Admin',
    '$http',
    'nsFilterOptionsService',
    '$routeParams',
    '$location'
  ];
  function ObservationSummaryClassController($scope, Admin, $http, nsFilterOptionsService, $routeParams, $location) {
    $scope.selectedOptions = {};
    //$scope.lookupFieldsArray = [];
    //$scope.studentResults = [];
    //$scope.benchmarks = [];
    //$scope.sortArray = [];
    //$scope.headerClassArray = [];
    //$scope.firstNameHeaderClass = "fa";
    //$scope.lastNameHeaderClass = "fa";
    //$scope.options = {};
    $scope.filterOptions = nsFilterOptionsService.options;
    //$scope.filterOptions.selectedBenchmarkDate = (typeof $routeParams.benchmarkDateId !== 'undefined') ? nsFilterOptionsService.getBenchmarkDateById($routeParams.benchmarkDateId) : $scope.filterOptions.selectedBenchmarkDate;
    //$scope.selectedOptions.benchmarkDateId = $scope.filterOptions.selectedBenchmarkDate.id;
    //$scope.selectedOptions.selectedSectionId = $scope.filterOptions.selectedSection.id;
    //$scope.$watch('filterOptions.selectedSection', function () {
    //    $scope.selectedOptions.selectedSectionId = $scope.filterOptions.selectedSection.id;
    //});
    //$scope.$watch('filterOptions.selectedBenchmarkDate', function () {
    //    $scope.selectedOptions.selectedBenchmarkDateId = $scope.filterOptions.selectedBenchmarkDate.id;
    //});
    $scope.navigateToTdd = function (tddid) {
      $scope.filterOptions.selectedBenchmarkDate = nsFilterOptionsService.getBenchmarkDateById(tddid);
      nsFilterOptionsService.changeBenchmarkDate();
    };
  }
  /* Movies Create Controller */
  ObservationSummaryFilteredController.$inject = [
    '$scope',
    'Admin',
    '$http',
    'nsFilterOptionsService',
    '$routeParams',
    '$location',
    'nsStackedBarGraphOptionsFactory'
  ];
  function ObservationSummaryFilteredController($scope, Admin, $http, nsFilterOptionsService, $routeParams, $location, nsStackedBarGraphOptionsFactory) {
    $scope.selectedOptions = {};
    //$scope.filterOptions = nsFilterOptionsService.options;
    $scope.groupsFactory = new nsStackedBarGraphOptionsFactory('Compare Group Across Benchmark Dates', false);
    $scope.filterOptions = $scope.groupsFactory.options;  //$scope.navigateToTdd = function (tddid) {
                                                          //    $scope.filterOptions.selectedBenchmarkDate = nsFilterOptionsService.getBenchmarkDateById(tddid);
                                                          //    nsFilterOptionsService.changeBenchmarkDate();
                                                          //}
  }
}());
(function () {
  'use strict';
  angular.module('interventionGroupModule', []).controller('InterventionGroupListController', InterventionGroupListController).controller('InterventionGroupEditController', InterventionGroupEditController).controller('InterventionGroupAttendanceController', InterventionGroupAttendanceController).service('InterventionGroupService', [
    '$http',
    'pinesNotifications',
    'webApiBaseUrl',
    function ($http, pinesNotifications, webApiBaseUrl) {
      //this.options = {};
      var self = this;
      self.deleteGroup = function (groupId) {
        var paramObj = { Id: groupId };
        var url = webApiBaseUrl + '/api/interventiongroup/DeleteIntervention/';
        var promise = $http.post(url, paramObj);
        return promise;
      };
      self.getbyyearschoolstaff = function (year, schoolid, staffid) {
        var paramObj = {
            SchoolYear: year,
            SchoolId: schoolid,
            StaffId: staffid
          };
        return $http.post(webApiBaseUrl + '/api/interventiongroup/getbyyearschoolstaff/', paramObj);
      };
    }
  ]).factory('InterventionGroup', [
    '$http',
    'webApiBaseUrl',
    '$filter',
    'nsFilterOptionsService',
    function InterventionGroup($http, webApiBaseUrl, $filter, nsFilterOptionsService) {
      var InterventionGroup = function (groupId) {
        var self = this;
        self.StintsByStudent = [];
        self.initialize = function () {
          var paramObj = { Id: groupId };
          var url = webApiBaseUrl + '/api/interventiongroup/GetGroup/';
          var promise = $http.post(url, paramObj);
          return promise.then(function (response) {
            angular.extend(self, response.data);
            // don't allow school or year to be changed for existing groups
            if (self.Id == 0 || self.Id == -1) {
              self.SchoolID = nsFilterOptionsService.normalizeParameter(nsFilterOptionsService.options.selectedSchool);
              self.SchoolStartYear = nsFilterOptionsService.normalizeParameter(nsFilterOptionsService.options.selectedSchoolYear);
            }
            self.postLoadProcessing();
          });
        };
        self.validate = function () {
          var msg = null;
          if (moment(self.StartTime).toDate() >= moment(self.EndTime).toDate()) {
            msg = 'Intervention Group start time must be BEFORE the end time.';
            return msg;
          }
          return msg;
        };
        self.canDeleteStint = function (stintId) {
          var paramObj = { Id: stintId };
          var url = webApiBaseUrl + '/api/interventiongroup/CanStintBeDeleted/';
          var promise = $http.post(url, paramObj);
          return promise;
        };
        self.postLoadProcessing = function () {
          for (var i = 0; i < self.StudentInterventionGroups.length; i++) {
            var sg = self.StudentInterventionGroups[i];
            // see if we've added this student already or not
            var recordsForStudentId = $filter('filter')(self.StintsByStudent, { StudentId: sg.StudentId });
            // if we've already created a group, just add it
            if (recordsForStudentId.length > 0) {
              var currentStintGroup = recordsForStudentId[0];
              currentStintGroup.Stints.push(sg);
            } else {
              // create a new stint group
              var newStintGroup = {
                  StudentId: sg.StudentId,
                  StudentName: sg.StudentName,
                  Stints: []
                };
              newStintGroup.Stints.push(sg);
              self.StintsByStudent.push(newStintGroup);
            }
          }
        };
        self.save = function () {
          var url = webApiBaseUrl + '/api/interventiongroup/SaveInterventionGroup';
          var promise = $http.post(url, self);
          return promise;
        };
        self.initialize();
      };
      return InterventionGroup;
    }
  ]).service('nsInterventionGroupService', [
    '$http',
    'pinesNotifications',
    'webApiBaseUrl',
    'nsFilterOptionsService',
    function ($http, pinesNotifications, webApiBaseUrl, nsFilterOptionsService) {
      //this.options = {};
      this.getAttendanceDataForWeek = function (sectionId, staffId, schoolYear, mondayDate) {
        var paramObj = {
            InterventionGroupId: nsFilterOptionsService.normalizeParameter(sectionId),
            StaffId: nsFilterOptionsService.normalizeParameter(staffId),
            SchoolStartYear: nsFilterOptionsService.normalizeParameter(schoolYear),
            MondayDate: mondayDate
          };
        return $http.post(webApiBaseUrl + '/api/interventiongroup/getweeklyattendance/', paramObj);
      };
      this.applyStatusNotes = function (attendanceDate, status, notes, staffId, sectionId, schoolStartYear) {
        return $http.post(webApiBaseUrl + '/api/interventiongroup/applyStatusNotes', {
          Notes: notes,
          'Date': attendanceDate,
          Status: status,
          StaffId: nsFilterOptionsService.normalizeParameter(staffId),
          SectionId: nsFilterOptionsService.normalizeParameter(sectionId),
          SchoolStartYear: nsFilterOptionsService.normalizeParameter(schoolStartYear)
        });
      };
      this.saveSingleAttendance = function (startEndDateId, status, notes, date, studentId, sectionId) {
        return $http.post(webApiBaseUrl + '/api/interventiongroup/saveSingleAttendance', {
          Notes: notes,
          StartEndDateId: startEndDateId,
          'Date': date,
          Status: status,
          StudentId: studentId,
          SectionId: sectionId
        });
      };
    }
  ]);
  /* Movies List Controller  */
  InterventionGroupListController.$inject = [
    '$scope',
    'InterventionGroup',
    '$q',
    '$http',
    'nsPinesService',
    '$location',
    'InterventionGroupService',
    '$bootbox',
    'nsFilterOptionsService'
  ];
  function InterventionGroupListController($scope, InterventionGroup, $q, $http, nsPinesService, $location, InterventionGroupService, $bootbox, nsFilterOptionsService) {
    $scope.groups = [];
    $scope.dropdownGroups = [];
    $scope.filterOptions = nsFilterOptionsService.options;
    $scope.errors = [];
    $scope.$on('NSHTTPError', function (event, data) {
      $scope.errors.push({
        type: 'danger',
        msg: data
      });
      $('html, body').animate({ scrollTop: 0 }, 'fast');
    });
    $scope.$watch('filterOptions.selectedSchool', function (newVal) {
      if (newVal !== null) {
        LoadData();
      }
    });
    $scope.$watch('filterOptions.selectedSchoolYear', function (newVal) {
      if (newVal !== null) {
        LoadData();
      }
    });
    $scope.$watch('filterOptions.selectedInterventionist', function (newVal) {
      if (newVal !== null) {
        LoadData();
      }
    });
    $scope.deleteGroup = function (id) {
      $bootbox.confirm('Are you sure you want to delete this Intervention Group?<br><br><b>Note:</b> You will not be able to delete this group if you have recorded attendance data for it or saved student assessment data.', function (response) {
        if (response) {
          InterventionGroupService.deleteGroup(id).then(function (response) {
            nsPinesService.dataDeletedSuccessfully();
            LoadData();
          });
        }
      });
    };
    var LoadData = function () {
      InterventionGroupService.getbyyearschoolstaff(nsFilterOptionsService.normalizeParameter($scope.filterOptions.selectedSchoolYear), nsFilterOptionsService.normalizeParameter($scope.filterOptions.selectedSchool), nsFilterOptionsService.normalizeParameter($scope.filterOptions.selectedInterventionist)).then(function (response) {
        $scope.groups = response.data.Groups;
      });
    };
    LoadData();
  }
  InterventionGroupEditController.$inject = [
    '$scope',
    'InterventionGroup',
    '$q',
    '$http',
    'nsPinesService',
    '$location',
    '$routeParams',
    'nsSelect2RemoteOptions',
    '$bootbox',
    '$filter',
    'nsFilterOptionsService'
  ];
  function InterventionGroupEditController($scope, InterventionGroup, $q, $http, nsPinesService, $location, $routeParams, nsSelect2RemoteOptions, $bootbox, $filter, nsFilterOptionsService) {
    $scope.group = new InterventionGroup($routeParams.id);
    $scope.StudentQuickSearchRemoteOptions = nsSelect2RemoteOptions.StudentQuickSearchRemoteOptions;
    $scope.PrimaryInterventionistRemoteOptions = nsSelect2RemoteOptions.PrimaryInterventionistRemoteOptions;
    $scope.InterventionTypeRemoteOptions = nsSelect2RemoteOptions.InterventionTypeRemoteOptions;
    $scope.CoInterventionistsRemoteOptions = nsSelect2RemoteOptions.CoInterventionistsRemoteOptions;
    $scope.filterOptions = nsFilterOptionsService.options;
    $scope.errors = [];
    $scope.$on('NSHTTPError', function (event, data) {
      $scope.errors.push({
        type: 'danger',
        msg: data
      });
      $('html, body').animate({ scrollTop: 0 }, 'fast');
    });
    $scope.newStint = {};
    $scope.datePopupStatus = { opened: false };
    // we don't care about these changes unless this is a new intervention group
    $scope.$watch('filterOptions', function () {
      if ($routeParams.id === '-1') {
        $scope.group.SchoolID = nsFilterOptionsService.normalizeParameter($scope.filterOptions.selectedSchool);
        $scope.group.SchoolStartYear = nsFilterOptionsService.normalizeParameter($scope.filterOptions.selectedSchoolYear);
      }
    }, true);
    $scope.saveGroup = function () {
      var validationMessage = $scope.group.validate();
      if (validationMessage != null) {
        $scope.errors.push({
          type: 'danger',
          msg: validationMessage
        });
        $('html, body').animate({ scrollTop: 0 }, 'fast');
        return;
      } else {
        $scope.errors = [];
      }
      $scope.group.save().then(function (response) {
        nsPinesService.dataSavedSuccessfully();
        $location.path('ig-manage');
      });
    };
    $scope.addNewStint = function () {
      // validate start date
      if (angular.isUndefined($scope.igform.newStintForm.newStintStart.$modelValue) || $scope.newStint.startDate == null) {
        $scope.igform.newStintForm.$pending = true;
        return;
      }
      // validate end date
      if ($scope.igform.newStintForm.newStintEnd.$invalid) {
        $scope.igform.newStintForm.$pending = true;
        return;
      }
      var student = $scope.newStint.quickSearchStudent;
      // make sure a student is selected
      if (!angular.isDefined(student)) {
        $bootbox.alert('Please select a student');
        return;
      }
      // make sure start date is before end date
      if ($scope.newStint.endDate != null) {
        if ($scope.newStint.endDate <= $scope.newStint.startDate) {
          $bootbox.alert('Start date must be before End date');
          return;
        }
      }
      var recordsForStudentId = $filter('filter')($scope.group.StintsByStudent, { StudentId: student.id });
      var newStint = {
          StudentId: student.id,
          StartDate: $scope.newStint.startDate,
          EndDate: $scope.newStint.endDate,
          Id: -1,
          InterventionGroupId: $scope.group.Id
        };
      // add new record as long as start
      if (recordsForStudentId.length == 0) {
        // create new stintholder
        var newStintGroup = {
            StudentId: student.id,
            StudentName: student.LastName + ', ' + student.FirstName + ' ' + student.MiddleName,
            Stints: []
          };
        newStintGroup.Stints.push(newStint);
        $scope.group.StintsByStudent.push(newStintGroup);
        $scope.group.StudentInterventionGroups.push(newStint);
      } else {
        var stintValidationMessage = validateNewStint(recordsForStudentId[0].Stints, newStint);
        if (stintValidationMessage != null) {
          $bootbox.alert(stintValidationMessage);
          return;
        }
        // additional validation against existing stints
        $scope.group.StudentInterventionGroups.push(newStint);
        recordsForStudentId[0].Stints.push(newStint);
      }
      nsPinesService.buildMessage('Stint Added Successfully', 'The stint has been successfully added to this intervention group', 'info');
      resetAddStint();
      $scope.igform.newStintForm.$pending = false;
    };
    // move to igManager
    var validateNewStint = function (existingStudentStints, newStint) {
      var validationMessage = null;
      // make sure only one open ended stint
      if (newStint.EndDate == null) {
        for (var i = 0; i < existingStudentStints.length; i++) {
          if (existingStudentStints[i].EndDate == null) {
            validationMessage = 'Only one stint can be open ended.';
            break;
          }
          // make sure start date is later than last end date
          if (newStint.StartDate < existingStudentStints[i].EndDate) {
            validationMessage = 'Start date of an open ended stint must be after the end date of all other stints.';
          }
        }
      } else {
        // both start and end are filled in, make sure start is either before any ohter start date and ends before or is after any other end date and ends after
        for (var i = 0; i < existingStudentStints.length; i++) {
          if (newStint.StartDate <= existingStudentStints[i].StartDate && newStint.EndDate >= existingStudentStints[i].StartDate) {
            validationMessage = 'New stint must end before the start of any other stints.';
            break;
          } else if (newStint.StartDate <= existingStudentStints[i].EndDate && newStint.EndDate >= existingStudentStints[i].EndDate) {
            validationMessage = 'Cannot start a new stint during another stint.';
            break;
          } else if (newStint.StartDate >= existingStudentStints[i].StartDate && newStint.EndDate <= existingStudentStints[i].EndDate) {
            validationMessage = 'A new stint cannot take place during an existing stint.';
            break;
          } else if (newStint.StartDate <= existingStudentStints[i].StartDate && newStint.EndDate >= existingStudentStints[i].EndDate) {
            validationMessage = 'A new sting cannot overlap an existing stint.';
            break;
          }
        }
      }
      return validationMessage;
    };
    var validateUpdatedStint = function (existingStudentStints, newStint) {
      var validationMessage = null;
      // make sure only one open ended stint
      if (newStint.newEndDate == null) {
        for (var i = 0; i < existingStudentStints.length; i++) {
          if (existingStudentStints[i].EndDate == null && existingStudentStints[i] != newStint) {
            validationMessage = 'Only one stint can be open ended.';
            break;
          }
          // make sure start date is later than last end date
          if (newStint.newStartDate < existingStudentStints[i].EndDate && existingStudentStints[i] != newStint) {
            validationMessage = 'Start date of an open ended stint must be after the end date of all other stints.';
          }
        }
      } else {
        // both start and end are filled in, make sure start is either before any ohter start date and ends before or is after any other end date and ends after
        for (var i = 0; i < existingStudentStints.length; i++) {
          if (newStint.newStartDate <= existingStudentStints[i].StartDate && newStint.newEndDate >= existingStudentStints[i].StartDate && existingStudentStints[i] != newStint) {
            validationMessage = 'New stint must end before the start of any other stints.';
            break;
          } else if (newStint.newStartDate <= existingStudentStints[i].EndDate && newStint.newEndDate >= existingStudentStints[i].EndDate && existingStudentStints[i] != newStint) {
            validationMessage = 'Cannot start a new stint during another stint.';
            break;
          } else if (newStint.newStartDate >= existingStudentStints[i].StartDate && newStint.newEndDate <= existingStudentStints[i].EndDate && existingStudentStints[i] != newStint) {
            validationMessage = 'A new stint cannot take place during an existing stint.';
            break;
          } else if (newStint.newStartDate <= existingStudentStints[i].StartDate && newStint.newEndDate >= existingStudentStints[i].EndDate && existingStudentStints[i] != newStint) {
            validationMessage = 'A new stint cannot overlap an existing stint.';
            break;
          }
        }
      }
      return validationMessage;
    };
    $scope.editStint = function (stint) {
      stint.editMode = true;
      stint.newStartDate = moment(stint.StartDate).format('DD-MMM-YYYY');
      stint.newEndDate = stint.EndDate == null ? stint.EndDate : moment(stint.EndDate).format('DD-MMM-YYYY');
    };
    $scope.saveStint = function (stint, newStart, newEnd, stints, studentId) {
      // validation checks
      if (angular.isUndefined(newStart) || newStart == null) {
        $bootbox.alert('Please enter a valid start date');
        return;
      }
      if (angular.isUndefined(newEnd)) {
        $bootbox.alert('Please enter a valid end date');
        return;
      }
      // make sure start date is before end date
      if (newEnd != null) {
        if (newEnd <= newStart) {
          $bootbox.alert('Start date must be before End date');
          return;
        }
      }
      // compare to other start end dates
      var stintValidationMessage = validateUpdatedStint(stints, stint);
      if (stintValidationMessage != null) {
        $bootbox.alert(stintValidationMessage);
        return;
      }
      stint.StartDate = newStart;
      stint.EndDate = newEnd;
      stint.StudentId = studentId;
      stint.editMode = false;
    };
    $scope.removeStint = function (stint) {
      // call webservice to see if we can remove this stint (is there any attendance or data tied to it)
      $scope.group.canDeleteStint(stint.Id).then(function (response) {
        if (!response.data.isValid) {
          $bootbox.alert('Cannot remove this stint because it has data that references it.');
          return;
        } else {
          // StudentInterventionGroups
          for (var i = 0; i < $scope.group.StintsByStudent.length; i++) {
            // find the right group
            if ($scope.group.StintsByStudent[i].StudentId == stint.StudentId) {
              for (var j = 0; j < $scope.group.StintsByStudent[i].Stints.length; j++) {
                // find the right group
                if ($scope.group.StintsByStudent[i].Stints[j] == stint) {
                  $scope.group.StintsByStudent[i].Stints.splice(j, 1);
                  // if empty get rid of parent
                  if ($scope.group.StintsByStudent[i].Stints.length == 0) {
                    $scope.group.StintsByStudent.splice(i, 1);
                  }
                  break;
                }
              }
            }
          }
          for (var i = 0; i < $scope.group.StudentInterventionGroups.length; i++) {
            if ($scope.group.StudentInterventionGroups[i] == stint) {
              $scope.group.StudentInterventionGroups.splice(i, 1);
              break;
            }
          }
        }
      });
    };
    $scope.endDate = function (date) {
      if (date == null) {
        return 'no end date';
      } else {
        return $filter('nsDateFormat')(date);
      }
    };
    var resetAddStint = function () {
      $scope.newStint = {};
      $scope.igform.newStintForm.$setPristine();
      $scope.igform.newStintForm.$setUntouched();
    };
  }
  InterventionGroupAttendanceController.$inject = [
    '$scope',
    'InterventionGroup',
    '$q',
    '$http',
    'nsFilterOptionsService',
    '$location',
    '$routeParams',
    'nsInterventionGroupService',
    'nsPinesService',
    'progressLoader',
    '$bootbox'
  ];
  function InterventionGroupAttendanceController($scope, InterventionGroup, $q, $http, nsFilterOptionsService, $location, $routeParams, nsInterventionGroupService, nsPinesService, progressLoader, $bootbox) {
    var makeMonday = function (theDate) {
      return moment(theDate).startOf('isoweek');
    };
    $scope.filterOptions = nsFilterOptionsService.options;
    $scope.computedMonday = makeMonday(moment()).format('MMMM D, YYYY');
    $scope.computedFriday = moment($scope.computedMonday, 'MMMM D, YYYY').add(4, 'days').format('MMMM D, YYYY');
    $scope.showResults = function () {
      if ($scope.filterOptions.selectedInterventionist != null) {
        return true;
      } else
        return false;
    };
    $scope.drp_start = $scope.computedMonday;
    //$scope.drp_end = moment().add('days', 31).format('MMMM D, YYYY');
    $scope.drp_options = {
      singleDatePicker: true,
      opens: 'left'
    };
    $scope.changeWeek = function (count) {
      $scope.drp_start = moment($scope.computedMonday, 'MMMM D, YYYY').add(count, 'weeks').format('MMMM D, YYYY');
    };
    $scope.$watch('drp_start', function (newVal, oldVal) {
      if (newVal !== oldVal) {
        // compute new monday and friday
        $scope.computedMonday = makeMonday($scope.drp_start).format('MMMM D, YYYY');
        $scope.computedFriday = moment($scope.computedMonday, 'MMMM D, YYYY').add(4, 'days').format('MMMM D, YYYY');
        LoadData();
      }
    });
    $scope.$on('NSInterventionistOptionsUpdated', function (event, data) {
      LoadData();
    });
    $scope.$on('NSInterventionGroupOptionsUpdated', function (event, data) {
      LoadData();
    });
    var loaderStart = function () {
      progressLoader.start();
      setTimeout(function () {
        progressLoader.set(50);
      }, 1000);
    };
    var LoadData = function (sectionId, staffId, schoolYear, mondayDate) {
      if ($scope.showResults()) {
        // TODO: Make this more robust
        loaderStart();
        nsInterventionGroupService.getAttendanceDataForWeek($scope.filterOptions.selectedInterventionGroup, $scope.filterOptions.selectedInterventionist, $scope.filterOptions.selectedSchoolYear, $scope.computedMonday).then(function (response) {
          progressLoader.end();
          $scope.weekdays = response.data.WeekDays;
          $scope.attendanceData = response.data.AttendanceData;
        });
      }
    };
    LoadData();
    $scope.applyStatusNotes = function (date) {
      if (date.Status == null) {
        $bootbox.alert('Please select a status');
        return;
      }
      $bootbox.confirm('Are you sure you want to apply this status to all listed students?', function (response) {
        if (response) {
          loaderStart();
          nsInterventionGroupService.applyStatusNotes(date.Date, date.Status, date.Notes, $scope.filterOptions.selectedInterventionist, $scope.filterOptions.selectedInterventionGroup, $scope.filterOptions.selectedSchoolYear).then(function (response) {
            // reload the data
            LoadData();
            nsPinesService.dataSavedSuccessfully();
            progressLoader.end();
            return true;
          });
        }
      });
    };
    // Disable weekend selection
    $scope.disabled = function (date, mode) {
      return mode === 'day' && (date.getDay() === 0 || date.getDay() === 6);
    };
    $scope.open = function ($event) {
      $event.preventDefault();
      $event.stopPropagation();
      $scope.opened = true;
    };
    $scope.today = function () {
      $scope.dt = new Date();
    };
    $scope.today();
    $scope.cellEnterEditMode = function (form, studentResult, realStatusField) {
      studentResult[realStatusField + 'Temp'] = studentResult[realStatusField];
      form.$show();
    };
    $scope.attendanceReasons = [
      'Teacher Absent',
      'Teacher Unavailable',
      'Child Absent',
      'Child Unavailable',
      'No School',
      'Intervention Delivered',
      'Make-Up Lesson',
      'Non-Cycle Day',
      'None'
    ];
    $scope.saveSingleAttendance = function (studentResult, realStatusField, startEndDateId, status, notes, date, studentId, sectionId) {
      if (studentResult[realStatusField + 'Temp'] == null) {
        $bootbox.alert('Please select a status');
        return;
      }
      // update real value with temp
      studentResult[realStatusField] = studentResult[realStatusField + 'Temp'];
      nsInterventionGroupService.saveSingleAttendance(startEndDateId, studentResult[realStatusField], notes, date, studentId, sectionId).then(function (data) {
        nsPinesService.dataSavedSuccessfully();
        return true;
      });
    };
  }
}());
(function () {
  'use strict';
  angular.module('interventionToolkitModule', []).controller('InterventionTierController', InterventionTierController).controller('InterventionViewTierController', InterventionViewTierController).controller('InterventionDetailController', InterventionDetailController).service('nsFileDownloadService', [
    '$http',
    'webApiBaseUrl',
    'FileSaver',
    function ($http, webApiBaseUrl, FileSaver) {
      this.getFile = function (filename) {
        return $http.get(webApiBaseUrl + '/api/azuredownload/downloadnorthstarfile?filename=' + filename, { responseType: 'arraybuffer' }).then(function (response) {
          var data = new Blob([response.data]);
          FileSaver.saveAs(data, filename);
        });
      };
      this.loadVideos = function (filename) {
        return $http.post(webApiBaseUrl + '/api/video/GetPagedVideoList');
      };
      this.getZippedTools = function (arrFileNames) {
        var paramObj = { FileNames: arrFileNames };
        return $http.post(webApiBaseUrl + '/api/azuredownload/downloadzippedtools', paramObj, { responseType: 'arraybuffer' }).then(function (response) {
          var data = new Blob([response.data]);
          FileSaver.saveAs(data, 'NorthStarTools.zip');
        });
      };
    }
  ]).service('nsInterventionToolkitService', [
    '$http',
    'webApiBaseUrl',
    function ($http, webApiBaseUrl) {
      //this.options = {};
      this.getTiers = function () {
        return $http.get(webApiBaseUrl + '/api/interventiontoolkit/getinterventiontiers');
      };
      //$routeParams.id, $scope.selectedGrade, $scope.selectedCategory, $scope.selectedGroupSize, $scope.selectedUnitOfStudy, $scope.selectedFramework, $scope.selectedWorkshop
      this.getTierData = function (id, selectedGrade, selectedCategory, selectedWorkshop) {
        var paramObj = {
            GradeId: !selectedGrade ? -1 : selectedGrade,
            CategoryId: !selectedCategory ? -1 : selectedCategory,
            WorkshopId: !selectedWorkshop ? -1 : selectedWorkshop,
            TierId: id
          };
        return $http.post(webApiBaseUrl + '/api/interventiontoolkit/GetInterventionsByTier', paramObj);
      };
      this.getInterventionById = function (id) {
        var paramObj = { Id: id };
        return $http.post(webApiBaseUrl + '/api/interventiontoolkit/GetInterventionById', paramObj);
      };
      this.deleteIntervention = function (id) {
        var paramObj = { Id: id };
        return $http.post(webApiBaseUrl + '/api/interventiontoolkit/DeleteIntervention', paramObj);
      };
      this.saveIntervention = function (intervention) {
        var paramObj = { Intervention: intervention };
        return $http.post(webApiBaseUrl + '/api/interventiontoolkit/SaveIntervention', paramObj);
      };
      this.saveTool = function (tool) {
        var paramObj = { Tool: tool };
        return $http.post(webApiBaseUrl + '/api/interventiontoolkit/SaveTool', paramObj);
      };
      this.saveVideo = function (video) {
        var paramObj = { Video: video };
        return $http.post(webApiBaseUrl + '/api/interventiontoolkit/SaveVideo', paramObj);
      };
      this.removeTool = function (interventionId, toolId) {
        var paramObj = {
            InterventionToolId: toolId,
            InterventionId: interventionId
          };
        return $http.post(webApiBaseUrl + '/api/interventiontoolkit/RemoveTool', paramObj);
      };
      this.removeVideo = function (interventionId, videoId) {
        var paramObj = {
            VideoId: videoId,
            InterventionId: interventionId
          };
        return $http.post(webApiBaseUrl + '/api/interventiontoolkit/RemoveVideo', paramObj);
      };
      this.associateTool = function (interventionId, toolId) {
        var paramObj = {
            InterventionToolId: toolId,
            InterventionId: interventionId
          };
        return $http.post(webApiBaseUrl + '/api/interventiontoolkit/AssociateTool', paramObj);
      };
    }
  ]);
  InterventionTierController.$inject = [
    '$scope',
    'InterventionGroup',
    '$q',
    '$http',
    'pinesNotifications',
    '$location',
    'nsInterventionToolkitService',
    'nsPinesService',
    'NSUserInfoService'
  ];
  function InterventionTierController($scope, InterventionGroup, $q, $http, pinesNotifications, $location, nsInterventionToolkitService, nsPinesService, NSUserInfoService) {
    nsInterventionToolkitService.getTiers().then(function (response) {
      $scope.tiers = response.data.Tiers;
      $scope.currentUser = NSUserInfoService.currentUser;
      // create data array
      $scope.tierData = [];  //for (var i = 0; i < $scope.tiers.length; i++) {
                             //    var data = {
                             //        title: 'Tier ' + $scope.tiers[i].TierValue,
                             //        href: '#/intervention-view-tier/' + $scope.tiers[i].TierValue,
                             //        titleBarInfo: '<span class="badge">4</span> Interventions',
                             //        text: $scope.tiers[i].Description,
                             //        color: 'info',
                             //        classes: 'fa fa-eye'
                             //    }
                             //    $scope.tierData.push(data);
                             //}
    });
  }
  InterventionViewTierController.$inject = [
    '$scope',
    'InterventionGroup',
    '$q',
    '$http',
    'pinesNotifications',
    '$location',
    'nsInterventionToolkitService',
    'nsPinesService',
    '$routeParams',
    'progressLoader',
    '$bootbox',
    'NSUserInfoService'
  ];
  function InterventionViewTierController($scope, InterventionGroup, $q, $http, pinesNotifications, $location, nsInterventionToolkitService, nsPinesService, $routeParams, progressLoader, $bootbox, NSUserInfoService) {
    $scope.currentUser = NSUserInfoService.currentUser;
    $scope.selectedOptions = {};
    $scope.selectedOptions.selectedGrade = '-1';
    $scope.selectedOptions.selectedCategory = '-1';
    $scope.selectedOptions.selectedWorkshop = '-1';
    var LoadTierData = function () {
      progressLoader.start();
      progressLoader.set(50);
      nsInterventionToolkitService.getTierData($routeParams.id, $scope.selectedOptions.selectedGrade, $scope.selectedOptions.selectedCategory, $scope.selectedOptions.selectedWorkshop).then(function (response) {
        $scope.interventions = response.data.Interventions;
        $scope.grades = response.data.Grades;
        $scope.categories = response.data.Categories;
        $scope.workshops = response.data.Workshops;
        $scope.tier = response.data.Tier;
        progressLoader.end();
      });
    };
    LoadTierData();
    $scope.delete = function (id, $event) {
      $event.preventDefault();
      $event.stopPropagation();
      $bootbox.confirm('Are you ABSOLUTELY SURE you want to delete this intervention?  Once deleted, it is GONE.', function (response) {
        if (response) {
          nsInterventionToolkitService.deleteIntervention(id).then(function (response) {
            LoadTierData();
          });
        }
      });
    };
    $scope.$watchCollection('selectedOptions', function (newVal, oldVal) {
      if (newVal !== oldVal) {
        LoadTierData();
      }
    });
  }
  InterventionDetailController.$inject = [
    '$scope',
    'InterventionGroup',
    '$q',
    '$http',
    'pinesNotifications',
    '$location',
    'nsInterventionToolkitService',
    'nsPinesService',
    '$uibModal',
    '$routeParams',
    'progressLoader',
    'nsSelect2RemoteOptions',
    'webApiBaseUrl',
    '$timeout',
    '$bootbox',
    'nsFileDownloadService',
    'NSUserInfoService'
  ];
  function InterventionDetailController($scope, InterventionGroup, $q, $http, pinesNotifications, $location, nsInterventionToolkitService, nsPinesService, $uibModal, $routeParams, progressLoader, nsSelect2RemoteOptions, webApiBaseUrl, $timeout, $bootbox, nsFileDownloadService, NSUserInfoService) {
    $scope.currentUser = NSUserInfoService.currentUser;
    $scope.uploadSettings = {};
    $scope.Intervention = { ready: false };
    $scope.gradeOptions = nsSelect2RemoteOptions.GradeQuickSearchRemoteOptions;
    $scope.workshopOptions = nsSelect2RemoteOptions.WorkshopRemoteOptions;
    $scope.nsSelect2RemoteOptions = nsSelect2RemoteOptions;
    $scope.nsFileDownloadService = nsFileDownloadService;
    $scope.SelectedAssessmentTools = [];
    $scope.SelectedInterventionTools = [];
    var LoadData = function () {
      nsInterventionToolkitService.getInterventionById($routeParams.id).then(function (response) {
        angular.extend($scope, response.data);
        $scope.Intervention.ready = true;
        //$scope.Intervention = response.data.Intervention;
        // edit mode by default for new items
        if ($routeParams.id == -1) {
          $scope.Intervention.InterventionTierID = '';
          $scope.edit();
        }
      });
    };
    LoadData();
    $scope.callit = function () {
      nsFileDownloadService.loadVideos();
    };
    $scope.downloadSelectedAssessmentTools = function () {
      nsFileDownloadService.getZippedTools($scope.SelectedAssessmentTools);
    };
    $scope.downloadSelectedInterventionTools = function () {
      nsFileDownloadService.getZippedTools($scope.SelectedInterventionTools);
    };
    $scope.saveTool = function (tool) {
      nsInterventionToolkitService.saveTool(tool).then(function (response) {
        progressLoader.end();
        nsPinesService.dataSavedSuccessfully();
      });
    };
    $scope.saveVideo = function (video) {
      nsInterventionToolkitService.saveVideo(video).then(function (response) {
        progressLoader.end();
        nsPinesService.dataSavedSuccessfully();
        LoadData();
      });
    };
    $scope.settings = {
      IsEditing: false,
      Mode: 'overview',
      SelectedAssessmentTool: {},
      SelectedInterventionTool: {},
      SelectedAssociatedVideo: {},
      SelectAllAssessmentTools: false,
      SelectAllInterventionTools: false
    };
    $scope.toggleSelectedAssessmentTools = function () {
      for (var i = 0; i < $scope.Intervention.AssessmentTools.length; i++) {
        var tool = $scope.Intervention.AssessmentTools[i];
        tool.toolIsSelected = $scope.settings.SelectAllAssessmentTools;
        $scope.toggleAssessmentToolSelection(tool);
      }
    };
    $scope.toggleSelectedInterventionTools = function () {
      for (var i = 0; i < $scope.Intervention.InterventionTools.length; i++) {
        var tool = $scope.Intervention.InterventionTools[i];
        tool.toolIsSelected = $scope.settings.SelectAllInterventionTools;
        $scope.toggleInterventionToolSelection(tool);
      }
    };
    $scope.toggleAssessmentToolSelection = function (tool) {
      if (tool.toolIsSelected) {
        var isFound = false;
        for (var i = 0; i < $scope.SelectedAssessmentTools.length; i++) {
          if ($scope.SelectedAssessmentTools[i] == tool.ToolFileName) {
            isFound = true;
            break;
          }
        }
        // only allow to be added once
        if (!isFound) {
          $scope.SelectedAssessmentTools.push(tool.ToolFileName);
        }
      } else {
        // remove tool
        for (var i = 0; i < $scope.SelectedAssessmentTools.length; i++) {
          if ($scope.SelectedAssessmentTools[i] == tool.ToolFileName) {
            $scope.SelectedAssessmentTools.splice(i, 1);
            break;
          }
        }
      }
    };
    $scope.toggleInterventionToolSelection = function (tool) {
      if (tool.toolIsSelected) {
        var isFound = false;
        for (var i = 0; i < $scope.SelectedInterventionTools.length; i++) {
          if ($scope.SelectedInterventionTools[i] == tool.ToolFileName) {
            isFound = true;
            break;
          }
        }
        // only allow to be added once
        if (!isFound) {
          $scope.SelectedInterventionTools.push(tool.ToolFileName);
        }
      } else {
        // remove tool
        for (var i = 0; i < $scope.SelectedInterventionTools.length; i++) {
          if ($scope.SelectedInterventionTools[i] == tool.ToolFileName) {
            $scope.SelectedInterventionTools.splice(i, 1);
            break;
          }
        }
      }
    };
    // go into edit mode
    $scope.edit = function () {
      $scope.settings.IsEditing = true;
      $scope.Intervention.DetailedDescriptionTemp = $scope.Intervention.DetailedDescription;
      $scope.Intervention.LearnerNeedTemp = $scope.Intervention.LearnerNeed;
      $scope.Intervention.ExitCriteriaTemp = $scope.Intervention.ExitCriteria;
      $scope.Intervention.EntranceCriteriaTemp = $scope.Intervention.EntranceCriteria;
      $scope.Intervention.BriefDescriptionTemp = $scope.Intervention.BriefDescription;
      $scope.Intervention.TimeOfYearTemp = $scope.Intervention.TimeOfYear;
      $scope.Intervention.InterventionTierIDTemp = $scope.Intervention.InterventionTierID + '';
    };
    $scope.cancel = function () {
      $scope.settings.IsEditing = false;
    };
    $scope.removeTool = function (toolId) {
      $bootbox.confirm('Are you sure you want to remove this tool?  Note: The tool file is not deleted.', function (response) {
        if (response) {
          nsInterventionToolkitService.removeTool($scope.Intervention.Id, toolId).then(function (response) {
            progressLoader.end();
            nsPinesService.dataSavedSuccessfully();
            LoadData();
          });
        }
      });
    };
    $scope.removeVideo = function (videoId) {
      $bootbox.confirm('Are you sure you want to remove this video from this intervention?  Note: The video is not deleted.', function (response) {
        if (response) {
          nsInterventionToolkitService.removeVideo($scope.Intervention.Id, videoId).then(function (response) {
            progressLoader.end();
            nsPinesService.dataSavedSuccessfully();
            LoadData();
          });
        }
      });
    };
    $scope.save = function () {
      progressLoader.start();
      progressLoader.set(50);
      // call save function, show message, then
      $scope.Intervention.DetailedDescription = $scope.Intervention.DetailedDescriptionTemp;
      $scope.Intervention.LearnerNeed = $scope.Intervention.LearnerNeedTemp;
      $scope.Intervention.ExitCriteria = $scope.Intervention.ExitCriteriaTemp;
      $scope.Intervention.EntranceCriteria = $scope.Intervention.EntranceCriteriaTemp;
      $scope.Intervention.BriefDescription = $scope.Intervention.BriefDescriptionTemp;
      $scope.Intervention.TimeOfYear = $scope.Intervention.TimeOfYearTemp;
      $scope.Intervention.InterventionTierID = $scope.Intervention.InterventionTierIDTemp;
      nsInterventionToolkitService.saveIntervention($scope.Intervention).then(function (response) {
        progressLoader.end();
        nsPinesService.dataSavedSuccessfully();
        // if we just saved a new one, redirect to new id
        if ($routeParams.id == -1) {
          $location.path('intervention-detail/' + response.data.id);
        } else {
          $scope.settings.IsEditing = false;
          LoadData();
        }
      });
    };
    $scope.associateAssessmentToolDialog = function () {
      var modalInstance = $uibModal.open({
          templateUrl: 'associateAssessmentTool.html',
          scope: $scope,
          controller: function ($scope, $uibModalInstance) {
            $scope.associateTool = function () {
              var paramObj = {
                  InterventionId: $scope.Intervention.Id,
                  InterventionToolId: $scope.settings.SelectedAssessmentTool.id
                };
              // start loader
              progressLoader.start();
              progressLoader.set(50);
              var promise = $http.post(webApiBaseUrl + '/api/interventiontoolkit/AssociateTool', paramObj).then(function (response) {
                  // end loader
                  progressLoader.end();
                  $scope.errors = [];
                  // show success
                  $timeout(function () {
                    $('#formReset').click();
                  }, 100);
                  LoadData();
                  $scope.settings.SelectedAssessmentTool = {};
                  nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
                });
              $uibModalInstance.dismiss('cancel');
            };
            $scope.cancel = function () {
              $scope.settings.SelectedAssessmentTool = {};
              $uibModalInstance.dismiss('cancel');
            };
          },
          size: 'md'
        });
    };
    $scope.displayVideoDialog = function (video) {
      $scope.selectedVideo = video;
      var modalInstance = $uibModal.open({
          templateUrl: 'playVideo.html',
          scope: $scope,
          controller: function ($scope, $uibModalInstance, $sce) {
            $scope.getSrc = function () {
              if ($scope.selectedVideo) {
                return $sce.trustAsResourceUrl('https://view.vzaar.com/' + $scope.selectedVideo.VideoStreamId + '/player?apiOn=true');
              }
              return $sce.trustAsResourceUrl('about:blank');
            };
            $scope.cancel = function () {
              $scope.selectedVideo = null;
              $uibModalInstance.dismiss('cancel');
            };
          },
          size: 'lg'
        });
    };
    $scope.associateVideoDialog = function () {
      var modalInstance = $uibModal.open({
          templateUrl: 'associateVideo.html',
          scope: $scope,
          controller: function ($scope, $uibModalInstance) {
            $scope.associateVideo = function () {
              var paramObj = {
                  InterventionId: $scope.Intervention.Id,
                  Video: $scope.settings.SelectedAssociatedVideo
                };
              // start loader
              progressLoader.start();
              progressLoader.set(50);
              var promise = $http.post(webApiBaseUrl + '/api/interventiontoolkit/associatevideo', paramObj).then(function (response) {
                  // end loader
                  progressLoader.end();
                  $scope.errors = [];
                  // show success
                  $timeout(function () {
                    $('#formReset').click();
                  }, 100);
                  $scope.settings.SelectedAssociatedVideo = {};
                  LoadData();
                  nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
                });
              $uibModalInstance.dismiss('cancel');
            };
            $scope.cancel = function () {
              $scope.settings.SelectedAssociatedVideo = {};
              $uibModalInstance.dismiss('cancel');
            };
          },
          size: 'md'
        });
    };
    $scope.associateInterventionToolDialog = function () {
      var modalInstance = $uibModal.open({
          templateUrl: 'associateInterventionTool.html',
          scope: $scope,
          controller: function ($scope, $uibModalInstance) {
            $scope.associateTool = function () {
              var paramObj = {
                  InterventionId: $scope.Intervention.Id,
                  InterventionToolId: $scope.settings.SelectedInterventionTool.id
                };
              // start loader
              progressLoader.start();
              progressLoader.set(50);
              var promise = $http.post(webApiBaseUrl + '/api/interventiontoolkit/AssociateTool', paramObj).then(function (response) {
                  // end loader
                  progressLoader.end();
                  $scope.errors = [];
                  // show success
                  $timeout(function () {
                    $('#formReset').click();
                  }, 100);
                  $scope.settings.SelectedInterventionTool = {};
                  LoadData();
                  nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
                });
              $uibModalInstance.dismiss('cancel');
            };
            $scope.cancel = function () {
              $scope.settings.SelectedInterventionTool = {};
              $uibModalInstance.dismiss('cancel');
            };
          },
          size: 'md'
        });
    };
    $scope.openUploadITDialog = function () {
      var modalInstance = $uibModal.open({
          templateUrl: 'uploadTool.html',
          scope: $scope,
          controller: function ($scope, $uibModalInstance) {
            $scope.theFiles = [];
            $scope.upload = function (theFiles) {
              var formData = new FormData();
              formData.append('InterventionId', $routeParams.id);
              angular.forEach(theFiles, function (file) {
                formData.append(file.name, file);
              });
              var paramObj = {};
              // start loader
              progressLoader.start();
              progressLoader.set(50);
              var promise = $http.post(webApiBaseUrl + '/api/interventiontoolkit/uploadinterventiontool', formData, {
                  transformRequest: angular.identity,
                  headers: { 'Content-Type': undefined }
                }).then(function (response) {
                  // end loader
                  progressLoader.end();
                  $scope.errors = [];
                  // show success
                  $timeout(function () {
                    $('#formReset').click();
                  }, 100);
                  //$scope.theFiles.length = 0;
                  //$scope.settings.hasFiles = false;
                  $scope.uploadSettings.uploadComplete = true;
                  LoadData();
                  nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
                });
              $uibModalInstance.dismiss('cancel');
            };
            $scope.cancel = function () {
              $uibModalInstance.dismiss('cancel');
            };
          },
          size: 'md'
        });
    };
    $scope.openUploadATDialog = function () {
      var modalInstance = $uibModal.open({
          templateUrl: 'uploadTool.html',
          scope: $scope,
          controller: function ($scope, $uibModalInstance) {
            $scope.theFiles = [];
            $scope.upload = function (theFiles) {
              var formData = new FormData();
              formData.append('InterventionId', $routeParams.id);
              angular.forEach(theFiles, function (file) {
                formData.append(file.name, file);
              });
              var paramObj = {};
              // start loader
              progressLoader.start();
              progressLoader.set(50);
              var promise = $http.post(webApiBaseUrl + '/api/interventiontoolkit/uploadassessmenttool', formData, {
                  transformRequest: angular.identity,
                  headers: { 'Content-Type': undefined }
                }).then(function (response) {
                  // end loader
                  progressLoader.end();
                  $scope.errors = [];
                  // show success
                  $timeout(function () {
                    $('#formReset').click();
                  }, 100);
                  //$scope.theFiles.length = 0;
                  //$scope.settings.hasFiles = false;
                  $scope.uploadSettings.uploadComplete = true;
                  LoadData();
                  nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
                });
              $uibModalInstance.dismiss('cancel');
            };
            $scope.cancel = function () {
              $uibModalInstance.dismiss('cancel');
            };
          },
          size: 'md'
        });
    };
    $scope.openModal = function (size, templateUrl) {
      var modalInstance = $uibModal.open({
          templateUrl: templateUrl,
          controller: function ($scope, $uibModalInstance) {
            $scope.close = function () {
              $uibModalInstance.dismiss('cancel');
            };
          },
          size: size
        });
    };
  }
}());
(function () {
  'use strict';
  angular.module('availableAssessmentsModule', []).controller('DistrictAssessmentsController', DistrictAssessmentsController).controller('SchoolAssessmentsController', SchoolAssessmentsController).controller('PersonalAssessmentsController', PersonalAssessmentsController).service('observationSummaryAssessmentFieldChooserSvc', [
    '$http',
    'webApiBaseUrl',
    function ($http, webApiBaseUrl) {
      var self = this;
      self.BenchmarkAssessments = [];
      self.StateTests = [];
      self.updateAssessments = function (assessments) {
        var paramObj = { AssessmentsOrFields: assessments };
        var promise = $http.post(webApiBaseUrl + '/api/assessmentavailability/UpdateObservationSummaryAssessmentVisibility', paramObj);
        return promise;
      };
      self.hideField = function (field) {
        var fields = [];
        fields.push({
          id: field.Id,
          text: '',
          Visible: false
        });
        var paramObj = { AssessmentsOrFields: fields };
        var promise = $http.post(webApiBaseUrl + '/api/assessmentavailability/UpdateObservationSummaryAssessmentFieldVisibility', paramObj);
        return promise;
      };
      self.hideAssessment = function (assessment) {
        var fields = [];
        fields.push({
          id: assessment.AssessmentId,
          text: '',
          Visible: false
        });
        var paramObj = { AssessmentsOrFields: fields };
        return $http.post(webApiBaseUrl + '/api/assessmentavailability/UpdateObservationSummaryAssessmentVisibility', paramObj).then(function (response) {
          self.initialize();  // reload fields
        });
      };
      self.updateFields = function (fields) {
        var paramObj = { AssessmentsOrFields: fields };
        var promise = $http.post(webApiBaseUrl + '/api/assessmentavailability/UpdateObservationSummaryAssessmentFieldVisibility', paramObj);
        return promise;
      };
      // get list of assessments
      self.initialize = function () {
        var promise = $http.get(webApiBaseUrl + '/api/assessmentavailability/GetObservationSummaryAssessmentList').then(function (response) {
            angular.extend(self, response.data);
          });
      };
      self.getAssessmentFields = function (assessmentId) {
        var paramObj = { Id: assessmentId };
        var promise = $http.post(webApiBaseUrl + '/api/assessmentavailability/GetObservationSummaryAssessmentFieldList', paramObj);
        return promise;
      };
      self.selectedAssessments = function () {
        var selected = '';
        for (var i = 0; i < self.BenchmarkAssessments.length; i++) {
          if (self.BenchmarkAssessments[i].Visible) {
            selected += self.BenchmarkAssessments[i].id + ',';
          }
        }
        for (var i = 0; i < self.StateTests.length; i++) {
          if (self.StateTests[i].Visible) {
            selected += self.StateTests[i].id + ',';
          }
        }
        if (selected.length > 0)
          // TODO: which it always will be if we make sure at least one assessment is selected
          {
            selected = selected.substring(0, selected.length - 1);
          }
        return selected;
      };
      self.initialize();
    }
  ]).directive('observationSummaryFieldChooser', [
    'observationSummaryAssessmentFieldChooserSvc',
    '$uibModal',
    'nsPinesService',
    'progressLoader',
    '$rootScope',
    function (observationSummaryAssessmentFieldChooserSvc, $uibModal, nsPinesService, progressLoader, $rootScope) {
      return {
        restrict: 'E',
        templateUrl: 'templates/observation-summary-field-chooser.html',
        link: function (scope, element, attr) {
          scope.assessmentService = observationSummaryAssessmentFieldChooserSvc;
          scope.modifiedAssessments = [];
          scope.modifiedFields = [];
          scope.settings = { menuOpen: false };
          scope.updateSelectedAssessment = function (assessment) {
            scope.modifiedAssessments.push(assessment);
          };
          scope.updateSelectedField = function (field) {
            scope.modifiedFields.push(field);
          };
          scope.openAssessmentChooser = function () {
            var modalInstance = $uibModal.open({
                templateUrl: 'assessmentChooser.html',
                scope: scope,
                controller: function ($scope, $uibModalInstance) {
                  $scope.refreshAssessments = function () {
                    progressLoader.start();
                    progressLoader.set(50);
                    $scope.assessmentService.updateAssessments($scope.modifiedAssessments).then(function (response) {
                      $rootScope.$broadcast('NSFieldsUpdated', true);
                      nsPinesService.dataSavedSuccessfully();
                      progressLoader.end();
                      $scope.modifiedAssessments = [];
                      $scope.settings.menuOpen = false;
                    });
                    $uibModalInstance.dismiss('cancel');
                  };
                  $scope.openFieldsPopup = function (assessment) {
                    $scope.selectedAssessment = assessment;
                    // get fields
                    scope.assessmentService.getAssessmentFields(assessment.id).then(function (response) {
                      $scope.selectedFields = response.data.Fields;
                      var modalInstance = $uibModal.open({
                          templateUrl: 'assessmentFieldViewer.html',
                          scope: $scope,
                          controller: function ($scope, $uibModalInstance) {
                            $scope.saveFields = function () {
                              $uibModalInstance.dismiss('cancel');
                              progressLoader.start();
                              progressLoader.set(50);
                              $scope.assessmentService.updateFields($scope.modifiedFields).then(function (response) {
                                $rootScope.$broadcast('NSFieldsUpdated', true);
                                nsPinesService.dataSavedSuccessfully();
                                progressLoader.end();
                                $scope.modifiedFields = [];
                                $scope.settings.menuOpen = false;
                              });
                            };
                            $scope.cancel = function () {
                              $uibModalInstance.dismiss('cancel');
                            };
                          },
                          size: 'lg'
                        });
                    });
                  };
                  $scope.cancel = function () {
                    $uibModalInstance.dismiss('cancel');
                  };
                },
                size: 'md'
              });
          };
        }
      };
    }
  ]);
  ;
  DistrictAssessmentsController.$inject = [
    '$scope',
    'InterventionGroup',
    '$q',
    '$http',
    'nsPinesService',
    '$location',
    '$routeParams',
    'NSDistrictAssessmentAvailabilityManager'
  ];
  function DistrictAssessmentsController($scope, InterventionGroup, $q, $http, nsPinesService, $location, $routeParams, NSDistrictAssessmentAvailabilityManager) {
    var vm = this;
    vm.dataManager = new NSDistrictAssessmentAvailabilityManager();
    vm.saveAvailability = function (availability) {
      vm.dataManager.saveAvailability(availability).then(function (response) {
        if (response) {
          nsPinesService.dataSavedSuccessfully();
        } else {
          nsPinesService.dataSaveError();
        }
      });
    };
  }
  SchoolAssessmentsController.$inject = [
    '$scope',
    'InterventionGroup',
    '$q',
    '$http',
    'nsPinesService',
    '$location',
    '$routeParams',
    'NSSchoolAssessmentAvailabilityManager',
    'nsFilterOptionsService',
    'spinnerService',
    '$timeout'
  ];
  function SchoolAssessmentsController($scope, InterventionGroup, $q, $http, nsPinesService, $location, $routeParams, NSSchoolAssessmentAvailabilityManager, nsFilterOptionsService, spinnerService, $timeout) {
    $scope.filterOptions = nsFilterOptionsService.options;
    $scope.dataManager = new NSSchoolAssessmentAvailabilityManager();
    $scope.dataManager.initialize($scope.filterOptions.selectedSchool);
    $scope.saveAvailability = function (availability) {
      if (!availability.IsDisabled) {
        $scope.dataManager.saveAvailability(availability).then(function (response) {
          if (response) {
            nsPinesService.dataSavedSuccessfully();
          } else {
            nsPinesService.dataSaveError();
          }
        });
      }
    };
    $scope.$watch('filterOptions.selectedSchool', function (newValue, oldValue) {
      if (!angular.equals(newValue, oldValue)) {
        spinnerService.show('tableSpinner');
        $scope.dataManager.initialize($scope.filterOptions.selectedSchool).finally(function (response) {
          spinnerService.hide('tableSpinner');
        });
      }
    }, true);
  }
  PersonalAssessmentsController.$inject = [
    '$scope',
    'InterventionGroup',
    '$q',
    '$http',
    'nsPinesService',
    '$location',
    '$routeParams',
    'NSStaffAssessmentAvailabilityManager',
    '$global'
  ];
  function PersonalAssessmentsController($scope, InterventionGroup, $q, $http, nsPinesService, $location, $routeParams, NSStaffAssessmentAvailabilityManager, $global) {
    var vm = this;
    vm.dataManager = new NSStaffAssessmentAvailabilityManager();
    vm.saveAvailability = function (availability) {
      if (!availability.IsDisabled) {
        vm.dataManager.saveAvailability(availability).then(function (response) {
          if (response) {
            $global.set('navrefreshneeded', true);
            nsPinesService.dataSavedSuccessfully();
          } else {
            nsPinesService.dataSaveError();
          }
        });
      }
    };
  }
}());
(function () {
  'use strict';
  angular.module('calendarModule', []).controller('DistrictCalendarController', DistrictCalendarController).controller('SchoolCalendarController', SchoolCalendarController).directive('fullCalendarCustom', [
    '$uibModal',
    'progressLoader',
    '$bootbox',
    function ($uibModal, progressLoader, $bootbox) {
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
                  };
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
                  };
                  $scope.cancel = function () {
                    $uibModalInstance.dismiss('cancel');
                  };
                },
                size: 'md'
              });
          };
          scope.newEventDialog = function (start, end, allDay) {
            var modalInstance = $uibModal.open({
                templateUrl: 'addevent.html',
                scope: scope,
                controller: function ($scope, $uibModalInstance) {
                  scope.selectedEvent = {
                    title: '',
                    start: start,
                    end: end,
                    allDay: allDay
                  };
                  scope.saveEvent = function (event) {
                    if (scope.selectedEvent.title) {
                      progressLoader.start();
                      progressLoader.set(50);
                      scope.mgr.saveEvent(event).then(function (response) {
                        calendar.fullCalendar('renderEvent', {
                          id: response.data.id,
                          title: scope.selectedEvent.title,
                          start: scope.selectedEvent.start,
                          end: scope.selectedEvent.end,
                          allDay: true
                        }, true);
                        // make the event "stick"
                        calendar.fullCalendar('unselect');
                        progressLoader.end();
                        $uibModalInstance.dismiss('cancel');
                      });
                    }
                  };
                  $scope.cancel = function () {
                    $uibModalInstance.dismiss('cancel');
                  };
                },
                size: 'md'
              });
          };
          var defaultOptions = {
              eventClick: function (event, element) {
                scope.editEventDialog(event);  //alert(event.title);
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
                prevYear: '<< previous year',
                nextYear: 'next year >> ',
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
                calendar = $(element).fullCalendar(defaultOptions);
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
            };
          }
          var calendar = {};
        }
      };
    }
  ]).factory('NSDistrictCalendarManager', [
    '$http',
    '$routeParams',
    'webApiBaseUrl',
    function ($http, $routeParams, webApiBaseUrl) {
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
          var paramObj = {
              Id: event.id,
              Subject: event.title,
              Start: event.start.toDate(),
              End: event.end.toDate()
            };
          var promise = $http.post(url, paramObj);
          return promise;
        };
        self.deleteEvent = function (event) {
          var url = webApiBaseUrl + '/api/calendar/DeleteDistrictCalendarEvent/';
          var paramObj = {
              Id: event.id,
              Subject: event.title,
              Start: event.start.toDate(),
              End: event.end ? event.end.toDate() : event.start.toDate()
            };
          var promise = $http.post(url, paramObj);
          return promise;
        };
        self.initialize();
      };
      return NSDistrictCalendarManager;
    }
  ]).factory('NSSchoolCalendarManager', [
    '$http',
    '$routeParams',
    'webApiBaseUrl',
    function ($http, $routeParams, webApiBaseUrl) {
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
          var paramObj = {
              Id: event.id,
              Subject: event.title,
              Start: event.start.toDate(),
              End: event.end.toDate(),
              SchoolId: event.SchoolId ? event.SchoolId : self.SchoolId
            };
          var promise = $http.post(url, paramObj);
          return promise;
        };
        self.deleteEvent = function (event) {
          var url = webApiBaseUrl + '/api/calendar/DeleteSchoolCalendarEvent/';
          var paramObj = {
              Id: event.id,
              Subject: event.title,
              Start: event.start.toDate(),
              End: event.end ? event.end.toDate() : event.start.toDate(),
              SchoolId: event.schoolId
            };
          var promise = $http.post(url, paramObj);
          return promise;
        };
      };
      return NSSchoolCalendarManager;
    }
  ]);
  DistrictCalendarController.$inject = [
    '$scope',
    'InterventionGroup',
    '$q',
    '$http',
    'nsPinesService',
    '$location',
    '$routeParams',
    'NSDistrictCalendarManager'
  ];
  function DistrictCalendarController($scope, InterventionGroup, $q, $http, nsPinesService, $location, $routeParams, NSDistrictCalendarManager) {
    $scope.dataManager = new NSDistrictCalendarManager();
  }
  SchoolCalendarController.$inject = [
    '$scope',
    'InterventionGroup',
    '$q',
    '$http',
    'nsPinesService',
    '$location',
    '$routeParams',
    'NSSchoolCalendarManager',
    'nsFilterOptionsService',
    'progressLoader'
  ];
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
}());
(function () {
  'use strict';
  angular.module('sectionReportsModule', []).factory('LIDReportManager', [
    '$http',
    'webApiBaseUrl',
    function ($http, webApiBaseUrl) {
      var LIDReportManager = function () {
        var self = this;
        self.assessment = {};
        self.studentSectionReportResults = [];
        self.tdds = [];
        self.HeaderFields = [];
        self.headerClassArray = [];
        self.sortArray = [];
        self.LoadData = function (assessmentId, sectionId, reportType, schoolYear) {
          var paramObj = {
              AssessmentId: assessmentId,
              SectionId: sectionId,
              ReportType: reportType,
              SchoolYear: schoolYear
            };
          return $http.post(webApiBaseUrl + '/api/sectionreport/GetLIDSectionReport', paramObj).then(function (response) {
            self.assessment = response.data.Assessment;
            self.studentSectionReportResults = response.data.StudentSectionReportResults;
            self.tdds = response.data.TestDueDates;
            self.HeaderFields = response.data.HeaderFields;
            var currentCssClass = '';
            var currentSubCatId = 0;
            for (var i = 0; i < self.HeaderFields.length; i++) {
              if (self.HeaderFields[i].SubcategoryId != currentSubCatId) {
                self.HeaderFields[i].cssClass = 'leftDoubleBorder';
              } else if (i === self.HeaderFields.length - 1) {
                self.HeaderFields[i].cssClass = 'rightDoubleBorder';
              }
              currentSubCatId = self.HeaderFields[i].SubcategoryId;
            }
            for (var r = 0; r < self.HeaderFields.length; r++) {
              self.headerClassArray[r] = 'fa';
            }
          });
        };
        self.sort = function (column) {
          var columnIndex = -1;
          // if this is not a first or lastname column
          if (!isNaN(parseInt(column))) {
            columnIndex = column;
            column = 'SummaryFieldResults[' + column + '].CellColorDate';
          }
          var bFound = false;
          for (var j = 0; j < self.sortArray.length; j++) {
            // if it is already on the list, reverse the sort
            if (self.sortArray[j].indexOf(column) >= 0) {
              bFound = true;
              // is it already negative? if so, remove it
              if (self.sortArray[j].indexOf('-') === 0) {
                if (columnIndex > -1) {
                  self.headerClassArray[columnIndex] = 'fa';
                } else if (column === 'LastName') {
                  self.nameHeaderClass = 'fa';
                }
                self.sortArray.splice(j, 1);
              } else {
                if (columnIndex > -1) {
                  self.headerClassArray[columnIndex] = 'fa fa-chevron-down';
                } else if (column === 'LastName') {
                  self.nameHeaderClass = 'fa fa-chevron-down';
                }
                self.sortArray[j] = '-' + self.sortArray[j];
              }
              break;
            }
          }
          if (!bFound) {
            self.sortArray.push(column);
            if (columnIndex > -1) {
              self.headerClassArray[columnIndex] = 'fa fa-chevron-up';
            } else if (column === 'LastName') {
              self.nameHeaderClass = 'fa fa-chevron-up';
            }
          }
        };
        self.subCategoryColSpan = function (subCategoryId) {
          var colSpan = 0;
          for (var i = 0; i < self.HeaderFields.length; i++) {
            if (self.HeaderFields[i].SubcategoryId == subCategoryId) {
              colSpan++;
            }
          }
          return colSpan;
        };
      };
      return LIDReportManager;
    }
  ]).factory('FPReportManager', [
    '$http',
    'webApiBaseUrl',
    function ($http, webApiBaseUrl) {
      var FPReportManager = function () {
        var self = this;
        self.assessment = {};
        self.studentSectionReportResults = [];
        self.tdds = [];
        self.scale = [];
        self.LoadData = function (assessmentId, sectionId, schoolYear) {
          var paramObj = {
              AssessmentId: assessmentId,
              SectionId: sectionId,
              SchoolYear: schoolYear
            };
          return $http.post(webApiBaseUrl + '/api/sectionreport/GetFPSectionReport', paramObj).then(function (response) {
            self.assessment = response.data.Assessment;
            self.studentSectionReportResults = response.data.StudentSectionReportResults;
            self.tdds = response.data.TestDueDates;
            self.scale = response.data.Scale;
            self.eoyBenchmark = response.data.EndOfYearBenchmark;
            self.soyBenchmark = response.data.StartOfYearBenchmark;
            self.targetZone = response.data.TargetZone;
            self.interventionRecords = response.data.InterventionRecords;
            self.previousGradeScores = response.data.PreviousGradeScores;
            self.studentServices = response.data.StudentServices;
            self.benchmarksByGrade = response.data.BenchmarksByGrade;
          });
        };
      };
      return FPReportManager;
    }
  ]).factory('SpellReportManager', [
    '$http',
    'webApiBaseUrl',
    function ($http, webApiBaseUrl) {
      var SpellReportManager = function () {
        var self = this;
        self.LoadData = function (assessmentId, sectionId, benchmarkDateId) {
          var paramObj = {
              AssessmentId: assessmentId,
              SectionId: sectionId,
              BenchmarkDateId: benchmarkDateId
            };
          return $http.post(webApiBaseUrl + '/api/sectionreport/GetSpellingInventorySectionReport', paramObj).then(function (response) {
            self.assessment = response.data.Assessment;
            self.studentResults = response.data.StudentResults;
            self.HeaderFields = response.data.HeaderFields;
            // assign fields to fieldResults... for the love of god, optimize this
            for (var j = 0; j < self.studentResults.length; j++) {
              for (var k = 0; k < self.studentResults[j].FieldResults.length; k++) {
                for (var i = 0; i < self.HeaderFields.length; i++) {
                  if (self.HeaderFields[i].DatabaseColumn === self.studentResults[j].FieldResults[k].DbColumn) {
                    self.studentResults[j].FieldResults[k].Field = self.HeaderFields[i];
                    self.studentResults[j].FieldResults[k].Field.OutOfHowMany = self.HeaderFields[i].OutOfHowMany;
                  }
                }
              }
            }
          });
        };
      };
      return SpellReportManager;
    }
  ]).factory('HRSIWReportManager', [
    '$http',
    'webApiBaseUrl',
    function ($http, webApiBaseUrl) {
      var HRSIWReportManager = function () {
        var self = this;
        self.assessment = {};
        self.studentSectionReportResults = [];
        self.tdds = [];
        self.HeaderFields = [];
        self.headerClassArray = [];
        self.sortArray = [];
        self.LoadData = function (assessmentId, sectionId, reportType, schoolYear, hrsFormId) {
          var paramObj = {
              AssessmentId: assessmentId,
              SectionId: sectionId,
              ReportType: reportType,
              SchoolYear: schoolYear,
              HRSFormId: hrsFormId
            };
          return $http.post(webApiBaseUrl + '/api/sectionreport/GetHRSIWSectionReport', paramObj).then(function (response) {
            self.assessment = response.data.Assessment;
            self.studentSectionReportResults = response.data.StudentSectionReportResults;
            self.tdds = response.data.TestDueDates;
            self.HeaderFields = response.data.HeaderFields;
            var currentCssClass = '';
            var currentSubCatId = 0;
            //for (var i = 0; i < self.HeaderFields.length; i++) {
            //    if (self.HeaderFields[i].SubcategoryId != currentSubCatId) {
            //        self.HeaderFields[i].cssClass = 'leftDoubleBorder';
            //    }
            //    else if (i === self.HeaderFields.length - 1) {
            //        self.HeaderFields[i].cssClass = 'rightDoubleBorder';
            //    }
            //    currentSubCatId = self.HeaderFields[i].SubcategoryId;
            //}
            for (var r = 0; r < self.HeaderFields.length; r++) {
              self.headerClassArray[r] = 'fa';
            }
          });
        };
        self.sort = function (column) {
          var columnIndex = -1;
          // if this is not a first or lastname column
          if (!isNaN(parseInt(column))) {
            columnIndex = column;
            column = 'SummaryFieldResults[' + column + '].CellColorDate';
          }
          var bFound = false;
          for (var j = 0; j < self.sortArray.length; j++) {
            // if it is already on the list, reverse the sort
            if (self.sortArray[j].indexOf(column) >= 0) {
              bFound = true;
              // is it already negative? if so, remove it
              if (self.sortArray[j].indexOf('-') === 0) {
                if (columnIndex > -1) {
                  self.headerClassArray[columnIndex] = 'fa';
                } else if (column === 'LastName') {
                  self.nameHeaderClass = 'fa';
                }
                self.sortArray.splice(j, 1);
              } else {
                if (columnIndex > -1) {
                  self.headerClassArray[columnIndex] = 'fa fa-chevron-down';
                } else if (column === 'LastName') {
                  self.nameHeaderClass = 'fa fa-chevron-down';
                }
                self.sortArray[j] = '-' + self.sortArray[j];
              }
              break;
            }
          }
          if (!bFound) {
            self.sortArray.push(column);
            if (columnIndex > -1) {
              self.headerClassArray[columnIndex] = 'fa fa-chevron-up';
            } else if (column === 'LastName') {
              self.nameHeaderClass = 'fa fa-chevron-up';
            }
          }
        };
        self.subCategoryColSpan = function (subCategoryId) {
          var colSpan = 0;
          for (var i = 0; i < self.HeaderFields.length; i++) {
            if (self.HeaderFields[i].SubcategoryId == subCategoryId) {
              colSpan++;
            }
          }
          return colSpan;
        };
      };
      return HRSIWReportManager;
    }
  ]).factory('WVReportManager', [
    '$http',
    'webApiBaseUrl',
    function ($http, webApiBaseUrl) {
      var WVReportManager = function () {
        var self = this;
        self.assessment = {};
        self.studentSectionReportResults = [];
        self.tdds = [];
        self.scale = [];
        self.LoadData = function (assessmentId, sectionId, schoolYear) {
          var paramObj = {
              AssessmentId: assessmentId,
              SectionId: sectionId,
              SchoolYear: schoolYear
            };
          return $http.post(webApiBaseUrl + '/api/sectionreport/GetWVSectionReport', paramObj).then(function (response) {
            self.assessment = response.data.Assessment;
            self.studentSectionReportResults = response.data.StudentSectionReportResults;
            self.tdds = response.data.TestDueDates;
            self.scale = response.data.Scale;
          });
        };
      };
      return WVReportManager;
    }
  ]).directive('interventionString', [
    '$routeParams',
    '$compile',
    '$templateCache',
    '$http',
    function ($routeParams, $compile, $templateCache, $http) {
      return {
        restrict: 'A',
        scope: {
          studentId: '=',
          tier: '=',
          interventionRecords: '='
        },
        link: function (scope, element, attr) {
          var recordHtml = '';
          var interventionString = '';
          for (var i = 0; i < scope.interventionRecords.length; i++) {
            if (scope.interventionRecords[i].Tier == scope.tier && scope.interventionRecords[i].StudentId == scope.studentId) {
              interventionString += '<a class=\'badge badge-danger\' href=\'#/ig-dashboard/' + scope.interventionRecords[i].SchoolStartYear + '/' + scope.interventionRecords[i].SchoolId + '/' + scope.interventionRecords[i].InterventionistId + '/' + scope.interventionRecords[i].InterventionGroupId + '/' + scope.interventionRecords[i].StudentId + '/' + scope.interventionRecords[i].StintId + '\'>' + scope.interventionRecords[i].InterventionType + '/' + scope.interventionRecords[i].StaffInitials + '-' + scope.interventionRecords[i].NumberOfLessons + '<a/><br/>';
            }
          }
          element.html(interventionString);
          $compile(element.contents())(scope);
        }
      };
    }
  ]).directive('wvScoreByTdd', [
    '$routeParams',
    '$compile',
    '$templateCache',
    '$http',
    function ($routeParams, $compile, $templateCache, $http) {
      return {
        restrict: 'A',
        scope: {
          studentResult: '=',
          tdd: '='
        },
        link: function (scope, element, attr) {
          var score = '';
          for (var i = 0; i < scope.studentResult.FieldResultsByTestDueDate.length; i++) {
            if (scope.studentResult.FieldResultsByTestDueDate[i].TDDID === scope.tdd.Id) {
              for (var k = 0; k < scope.studentResult.FieldResultsByTestDueDate[i].FieldResults.length; k++) {
                // find the right field, then check to see if it should be colored in
                if (scope.studentResult.FieldResultsByTestDueDate[i].FieldResults[k].WordsCorrect != null) {
                  score = scope.studentResult.FieldResultsByTestDueDate[i].FieldResults[k].WordsCorrect;
                  break;
                }
              }
            }
          }
          element.html(score);
          $compile(element.contents())(scope);
        }
      };
    }
  ]).directive('studentServices', [
    '$routeParams',
    '$compile',
    '$templateCache',
    '$http',
    function ($routeParams, $compile, $templateCache, $http) {
      return {
        restrict: 'A',
        scope: {
          studentId: '=',
          allStudentServices: '='
        },
        link: function (scope, element, attr) {
          var recordHtml = '';
          var studentServiceString = '';
          for (var i = 0; i < scope.allStudentServices.length; i++) {
            if (scope.allStudentServices[i].StudentId == scope.studentId) {
              studentServiceString += '<span title="' + scope.allStudentServices[i].Description + '">' + scope.allStudentServices[i].Label + '</span><br />';
            }
          }
          element.html(studentServiceString);
          $compile(element.contents())(scope);
        }
      };
    }
  ]).directive('fpTestScore', [
    '$routeParams',
    '$compile',
    '$templateCache',
    '$http',
    function ($routeParams, $compile, $templateCache, $http) {
      return {
        restrict: 'A',
        scope: {
          scale: '=',
          studentResult: '=',
          tdd: '='
        },
        link: function (scope, element, attr) {
          var cellHtml = '';
          var currentTDDID = scope.tdd.Id;
          for (var i = 0; i < scope.studentResult.FieldResultsByTestDueDate.length; i++) {
            if (scope.studentResult.FieldResultsByTestDueDate[i].TDDID === currentTDDID) {
              if (scope.studentResult.FieldResultsByTestDueDate[i].FieldResults.length > 0) {
                // if there is a value
                for (var p = 0; p < scope.scale.length; p++) {
                  if (scope.studentResult.FieldResultsByTestDueDate[i].FieldResults[0].FPValueId == scope.scale[p].FPID) {
                    cellHtml = scope.scale[p].FPs;
                    break;
                  }
                }
              }
            }
          }
          element.html(cellHtml);
          $compile(element.contents())(scope);
        }
      };
    }
  ]).directive('fpCommentsCell', [
    '$routeParams',
    '$compile',
    '$templateCache',
    '$http',
    '$uibModal',
    function ($routeParams, $compile, $templateCache, $http, $uibModal) {
      return {
        restrict: 'A',
        scope: {
          scaleRow: '=',
          studentresult: '=',
          tdds: '=',
          targetZone: '=',
          eoyBenchmark: '=',
          soyBenchmark: '=',
          scale: '='
        },
        link: function (scope, element, attr) {
          var currentColor = '';
          //var previousColor = '';
          var currentCellLeftHtml = '';
          var currentCellRightHtml = '</td>';
          var rowHtml = '';
          var currentInnerText = '';
          var currentNoteText = '';
          var noteLeftTemplate = '';
          var noteRightTemplate = '';
          var currentCategory = 0;
          var cssBorderClass = '';
          scope.settings = { comments: 'empty' };
          scope.toolTipFunction = function (tddId, date, duedateIndex, resultIndex, tddId2, date2, duedateIndex2, resultIndex2, tddId3, date3, duedateIndex3, resultIndex3, tddId4, date4, duedateIndex4, resultIndex4) {
            var returnString = '';
            returnString = '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex].FieldResults[resultIndex].Comment + '</span></div>';
            if (angular.isDefined(tddId2)) {
              returnString += '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId2 + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date2 + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex2].FieldResults[resultIndex2].Comment + '</span></div>';
            }
            if (angular.isDefined(tddId3)) {
              returnString += '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId3 + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date3 + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex3].FieldResults[resultIndex3].Comment + '</span></div>';
            }
            if (angular.isDefined(tddId4)) {
              returnString += '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId4 + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date4 + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex4].FieldResults[resultIndex4].Comment + '</span></div>';
            }
            var modalInstance = $uibModal.open({
                templateUrl: 'commentsModal.html',
                scope: scope,
                controller: function ($scope, $uibModalInstance) {
                  $scope.settings.comments = returnString;
                  $scope.cancel = function () {
                    $uibModalInstance.dismiss('cancel');
                  };
                },
                size: 'lg'
              });  //return returnString;
          };
          var studentresult = scope.studentresult;
          currentCellLeftHtml = '';
          currentInnerText = '';
          currentNoteText = '';
          noteLeftTemplate = '';
          noteRightTemplate = '';
          cssBorderClass = '';
          for (var j = 0; j < scope.tdds.length; j++) {
            var currentTDDID = scope.tdds[j].Id;
            //TODO, need to do a foreach of the headerfields and output them
            for (var i = 0; i < studentresult.FieldResultsByTestDueDate.length; i++) {
              if (studentresult.FieldResultsByTestDueDate[i].TDDID === currentTDDID) {
                // we've found the color for this one, do we color the cell or not?
                for (var k = 0; k < studentresult.FieldResultsByTestDueDate[i].FieldResults.length; k++) {
                  // do the comment here for each tdd, only add one icon
                  if (studentresult.FieldResultsByTestDueDate[i].FieldResults[k].Comment != null && studentresult.FieldResultsByTestDueDate[i].FieldResults[k].Comment != '') {
                    if (currentNoteText === '') {
                      noteLeftTemplate = '<i class="fa fa-comments" style="margin-left:5px;cursor:pointer" ng-click="toolTipFunction(';
                      noteRightTemplate = ');"></i>';
                    }
                    var d = new Date(Date.parse(scope.tdds[j].DueDate));
                    currentNoteText += '\'' + scope.tdds[j].Id + '\',\'' + (d.getMonth() + 1) + '/' + d.getFullYear() + '\',' + i + ',' + k + ',';
                  }
                }
              }
            }
          }
          // remove trailing comma on currentNotText
          if (currentNoteText.length > 0) {
            currentNoteText = currentNoteText.substring(0, currentNoteText.length - 1);
          }
          //currentCategory = scope.headerfields[p].SubcategoryId;
          // add an empty cell if none of the test due dates are checked
          if (currentCellLeftHtml === '') {
            if (noteLeftTemplate === '') {
              rowHtml += '<td></td>';
            } else {
              rowHtml += '<td>' + noteLeftTemplate + currentNoteText + noteRightTemplate + '</td>';
            }
          } else {
            if (noteLeftTemplate === '') {
              rowHtml += currentCellLeftHtml + currentInnerText + currentCellRightHtml;
            } else {
              rowHtml += currentCellLeftHtml + currentInnerText + noteLeftTemplate + currentNoteText + noteRightTemplate + currentCellRightHtml;
            }
          }
          //element.html("cheese");
          element.html(rowHtml);
          $compile(element.contents())(scope);
        }
      };
    }
  ]).directive('fpSectionReportTableRow', [
    '$routeParams',
    '$compile',
    '$templateCache',
    '$http',
    function ($routeParams, $compile, $templateCache, $http) {
      return {
        restrict: 'A',
        scope: {
          scaleRow: '=',
          studentresults: '=',
          tdds: '=',
          targetZone: '=',
          eoyBenchmark: '=',
          soyBenchmark: '=',
          benchmarksByGrade: '=',
          previousGradeScores: '='
        },
        link: function (scope, element, attr) {
          var currentColor = '';
          //var previousColor = '';
          var currentCellLeftHtml = '';
          var currentCellRightHtml = '</td>';
          var rowHtml = '';
          var currentInnerText = '';
          var currentNoteText = '';
          var noteLeftTemplate = '';
          var noteRightTemplate = '';
          var currentCategory = 0;
          var cssBorderClass = '';
          var scaleRowFPValueID = scope.scaleRow.FPID;
          function getCellStyle() {
            var cellStyle = '';
            // use the grade level for the section for the leftmost columns
            // target zone
            if (scope.scaleRow.FPID >= scope.targetZone.Meets && scope.scaleRow.FPID < scope.targetZone.Exceeds) {
              cellStyle = 'targetZone';
            }
            // eoy
            if (scope.scaleRow.FPID == scope.eoyBenchmark.Exceeds - 1) {
              cellStyle += ' eoyBenchmark';
            }
            // soy
            if (scope.scaleRow.FPID == scope.soyBenchmark.Meets || scope.soyBenchmark.Meets == 1 && scope.scaleRow.FPID == 1) {
              cellStyle += ' soyBenchmark';
            }
            return cellStyle;
          }
          function getCellStyleForStudentResult(studentResult) {
            var cellStyle = '';
            // use the benchmarks from student's current grade
            var currentTargetZone = scope.targetZone;
            var currentEoyBenchmark = scope.eoyBenchmark;
            var currentSoyBenchmark = scope.soyBenchmark;
            for (var i = 0; i < scope.benchmarksByGrade.length; i++) {
              if (scope.benchmarksByGrade[i].GradeId == studentResult.GradeId) {
                currentTargetZone = scope.benchmarksByGrade[i].TargetZone;
                currentEoyBenchmark = scope.benchmarksByGrade[i].EndOfYearBenchmark;
                currentSoyBenchmark = scope.benchmarksByGrade[i].StartOfYearBenchmark;
                // target zone
                if (scope.scaleRow.FPID >= currentTargetZone.Meets && scope.scaleRow.FPID < currentTargetZone.Exceeds) {
                  cellStyle = 'targetZone';
                }
                // eoy
                if (scope.scaleRow.FPID == currentEoyBenchmark.Exceeds - 1) {
                  cellStyle += ' eoyBenchmark';
                }
                // soy
                if (scope.scaleRow.FPID == currentSoyBenchmark.Meets || currentSoyBenchmark.Meets == 1 && scope.scaleRow.FPID == 1) {
                  cellStyle += ' soyBenchmark';
                }
              }
            }
            return cellStyle;
          }
          // left side of grid, TODO: css class for upper and lower and benchmark gray
          rowHtml += '<td class=\'' + getCellStyle() + '\'>' + scope.scaleRow.Grades + '</td>';
          rowHtml += '<td class=\'' + getCellStyle() + '\'>' + scope.scaleRow.DRAs + '</td>';
          rowHtml += '<td class=\'' + getCellStyle() + '\'>' + (scope.scaleRow.RR === null ? '' : scope.scaleRow.RR) + '</td>';
          rowHtml += '<td class=\'' + getCellStyle() + '\'>' + scope.scaleRow.FPs + '</td>';
          rowHtml += '<td class=\'' + getCellStyle() + '\'>' + scope.scaleRow.Lexiles + '</td>';
          // loop over the tdds for each studentresult and find the latest one, then get the hex for it?
          for (var p = 0; p < scope.studentresults.length; p++) {
            var studentresult = scope.studentresults[p];
            currentCellLeftHtml = '';
            currentInnerText = '';
            currentNoteText = '';
            noteLeftTemplate = '';
            noteRightTemplate = '';
            cssBorderClass = '';
            // previous grade score
            for (var n = 0; n < scope.previousGradeScores.length; n++) {
              if (scope.previousGradeScores[n].Id === studentresult.StudentId && scope.previousGradeScores[n].PreviousFPID === scope.scaleRow.FPID) {
                currentInnerText += '<span class=\'badge\' style=\'background-color:black\'>' + scope.previousGradeScores[n].PreviousGradeLabel + '</span>';
                break;
              }
            }
            for (var n = 0; n < studentresult.SummaryFieldResults.length; n++) {
              // find the right field, then check to see if it should be colored in
              if (studentresult.SummaryFieldResults[n].FPValueId === scaleRowFPValueID) {
                // NEW
                if (studentresult.SummaryFieldResults[n].XColorDates.length > 0) {
                  var hexColor = '';
                  for (var r = 0; r < studentresult.SummaryFieldResults[n].XColorDates.length; r++) {
                    for (var t = 0; t < scope.tdds.length; t++) {
                      if (scope.tdds[t].DueDate == studentresult.SummaryFieldResults[n].XColorDates[r]) {
                        hexColor = scope.tdds[t].Hex;
                        currentInnerText += '<span class=\'badge\' style=\'background-color:black;color:' + hexColor + '\'>X</span>';
                        break;
                      }
                    }
                  }
                }
                if (studentresult.SummaryFieldResults[n].CellColorDate != null) {
                  var hexColor = '';
                  for (var t = 0; t < scope.tdds.length; t++) {
                    if (scope.tdds[t].DueDate == studentresult.SummaryFieldResults[n].CellColorDate) {
                      hexColor = scope.tdds[t].Hex;
                    }
                  }
                  currentCellLeftHtml = '<td class=\'' + getCellStyleForStudentResult(studentresult) + '\' style=\'font-weight:bold;text-align:center;background-color:' + hexColor + '\'>';
                }
              }
            }
            //currentCategory = scope.headerfields[p].SubcategoryId;
            // add an empty cell if none of the test due dates are checked
            if (currentCellLeftHtml === '') {
              if (noteLeftTemplate === '') {
                if (currentInnerText == '') {
                  rowHtml += '<td class=\'' + getCellStyleForStudentResult(studentresult) + '\' ></td>';
                } else {
                  rowHtml += '<td class=\'' + getCellStyleForStudentResult(studentresult) + '\' style=\'font-weight:bold;text-align:center\'>' + currentInnerText + '</td>';
                }
              } else {
                rowHtml += '<td class=\'' + getCellStyleForStudentResult(studentresult) + '\' >' + noteLeftTemplate + currentNoteText + noteRightTemplate + '</td>';
              }
            } else {
              if (noteLeftTemplate === '') {
                rowHtml += currentCellLeftHtml + currentInnerText + currentCellRightHtml;
              } else {
                rowHtml += currentCellLeftHtml + currentInnerText + noteLeftTemplate + currentNoteText + noteRightTemplate + currentCellRightHtml;
              }
            }
          }
          //element.html("cheese");
          element.html(rowHtml);
          $compile(element.contents())(scope);
        }
      };
    }
  ]).directive('wvSectionReportTableRow', [
    '$routeParams',
    '$compile',
    '$templateCache',
    '$http',
    function ($routeParams, $compile, $templateCache, $http) {
      return {
        restrict: 'A',
        scope: {
          scaleRow: '=',
          studentresults: '=',
          tdds: '='
        },
        link: function (scope, element, attr) {
          var currentColor = '';
          //var previousColor = '';
          var currentCellLeftHtml = '';
          var currentCellRightHtml = '</td>';
          var rowHtml = '';
          var currentInnerText = '';
          var currentNoteText = '';
          var noteLeftTemplate = '';
          var noteRightTemplate = '';
          var currentCategory = 0;
          var cssBorderClass = '';
          var scaleRowScore = scope.scaleRow.id;
          // left side of grid, TODO: css class for upper and lower and benchmark gray
          rowHtml += '<td>' + scope.scaleRow.text + '</td>';
          // loop over the tdds for each studentresult and find the latest one, then get the hex for it?
          for (var p = 0; p < scope.studentresults.length; p++) {
            var studentresult = scope.studentresults[p];
            currentCellLeftHtml = '';
            currentInnerText = '';
            currentNoteText = '';
            noteLeftTemplate = '';
            noteRightTemplate = '';
            cssBorderClass = '';
            //for (var j = 0; j < scope.tdds.length; j++) {
            //    var currentTDDID = scope.tdds[j].Id;
            //    //TODO, need to do a foreach of the headerfields and output them
            //    for (var i = 0; i < studentresult.FieldResultsByTestDueDate.length; i++) {
            //        if (studentresult.FieldResultsByTestDueDate[i].TDDID === currentTDDID) {
            //            // we've found the color for this one, do we color the cell or not?
            //            for (var k = 0; k < studentresult.FieldResultsByTestDueDate[i].FieldResults.length; k++) {
            //                // find the right field, then check to see if it should be colored in
            //                if (studentresult.FieldResultsByTestDueDate[i].FieldResults[k].WordsCorrect === scaleRowScore || (studentresult.FieldResultsByTestDueDate[i].FieldResults[k].WordsCorrect >= 56 && scaleRowScore == 56)) {
            //                    // do the comment here for each tdd, only add one icon
            //                    if (studentresult.FieldResultsByTestDueDate[i].FieldResults[k].Comment != null && studentresult.FieldResultsByTestDueDate[i].FieldResults[k].Comment != '') {
            //                        if (currentNoteText === '') {
            //                            //currentNoteText += '<i class="fa fa-paperclip" style="cursor:pointer" popover-template="&apos;commentCell_' + scope.studentresult.StudentId + '_' + field.Id + '&apos;" popover-title="Comments"></i>';
            //                            noteLeftTemplate = '<i popover-trigger="mouseenter" class="fa fa-comments" style="margin-left:5px;cursor:pointer" popover="';
            //                            noteRightTemplate = '" popover-title="Comments"></i>';
            //                        }
            //                        var d = new Date(Date.parse(scope.tdds[j].DueDate));
            //                        currentNoteText += "<div><span class='orange' style='border:1px solid #666666;display:inline-block;height:10px;width:25px;Background-color:" + scope.tdds[j].Hex + ";'></span><span>&nbsp;" + (d.getMonth() + 1) + "/" + d.getFullYear() + "</span>&nbsp;&nbsp;--&nbsp;&nbsp;<span style='font-weight:bold'>" + studentresult.FieldResultsByTestDueDate[i].FieldResults[k].Comment + "</span></div>";
            //                        //noteLeftTemplate += "busted";
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            for (var n = 0; n < studentresult.SummaryFieldResults.length; n++) {
              // find the right field, then check to see if it should be colored in
              if (studentresult.SummaryFieldResults[n].WordsCorrect === scaleRowScore) {
                // NEW
                if (studentresult.SummaryFieldResults[n].XColorDates.length > 0) {
                  var hexColor = '';
                  for (var r = 0; r < studentresult.SummaryFieldResults[n].XColorDates.length; r++) {
                    for (var t = 0; t < scope.tdds.length; t++) {
                      if (scope.tdds[t].DueDate == studentresult.SummaryFieldResults[n].XColorDates[r]) {
                        hexColor = scope.tdds[t].Hex;
                        currentInnerText += '<span class=\'badge\' style=\'background-color:black;color:' + hexColor + '\'>X</span>';
                        break;
                      }
                    }
                  }
                }
                if (studentresult.SummaryFieldResults[n].CellColorDate != null) {
                  var hexColor = '';
                  for (var t = 0; t < scope.tdds.length; t++) {
                    if (scope.tdds[t].DueDate == studentresult.SummaryFieldResults[n].CellColorDate) {
                      hexColor = scope.tdds[t].Hex;
                    }
                  }
                  currentCellLeftHtml = '<td style=\'font-weight:bold;text-align:center;background-color:' + hexColor + '\'>';
                }
              }
            }
            //currentCategory = scope.headerfields[p].SubcategoryId;
            // add an empty cell if none of the test due dates are checked
            if (currentCellLeftHtml === '') {
              if (noteLeftTemplate === '') {
                rowHtml += '<td></td>';
              } else {
                rowHtml += '<td>' + noteLeftTemplate + currentNoteText + noteRightTemplate + '</td>';
              }
            } else {
              if (noteLeftTemplate === '') {
                rowHtml += currentCellLeftHtml + currentInnerText + currentCellRightHtml;
              } else {
                rowHtml += currentCellLeftHtml + currentInnerText + noteLeftTemplate + currentNoteText + noteRightTemplate + currentCellRightHtml;
              }
            }
          }
          //element.html("cheese");
          element.html(rowHtml);
          $compile(element.contents())(scope);
        }
      };
    }
  ]).directive('capSectionReportTableRow', [
    '$routeParams',
    '$compile',
    '$templateCache',
    '$http',
    function ($routeParams, $compile, $templateCache, $http) {
      return {
        restrict: 'A',
        scope: {
          headerfields: '=',
          studentresult: '=',
          tdds: '='
        },
        link: function (scope, element, attr) {
          var currentColor = '';
          //var previousColor = '';
          var currentCellLeftHtml = '';
          var currentCellRightHtml = '</td>';
          var rowHtml = '';
          var currentInnerText = '';
          var currentNoteText = '';
          var noteLeftTemplate = '';
          var noteRightTemplate = '';
          var currentCategory = 0;
          var cssBorderClass = '';
          scope.toolTipFunction = function (tddId, date, duedateIndex, resultIndex, tddId2, date2, duedateIndex2, resultIndex2, tddId3, date3, duedateIndex3, resultIndex3, tddId4, date4, duedateIndex4, resultIndex4) {
            var returnString = '';
            returnString = '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex].FieldResults[resultIndex].Comment + '</span></div>';
            if (angular.isDefined(tddId2)) {
              returnString += '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId2 + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date2 + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex2].FieldResults[resultIndex2].Comment + '</span></div>';
            }
            if (angular.isDefined(tddId3)) {
              returnString += '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId3 + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date3 + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex3].FieldResults[resultIndex3].Comment + '</span></div>';
            }
            if (angular.isDefined(tddId4)) {
              returnString += '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId4 + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date4 + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex4].FieldResults[resultIndex4].Comment + '</span></div>';
            }
            return returnString;
          };
          rowHtml += '<td><span student-dashboard-link student-id="' + scope.studentresult.StudentId + '" student-name="\'' + scope.studentresult.LastName.replace(/'/g, '\\\'') + ', ' + scope.studentresult.FirstName.replace(/'/g, '\\\'') + '\'"></span></td>';
          // loop over the tdds for each studentresult and find the latest one, then get the hex for it?
          for (var p = 0; p < scope.headerfields.length; p++) {
            var field = scope.headerfields[p];
            currentCellLeftHtml = '';
            currentInnerText = '';
            currentNoteText = '';
            noteLeftTemplate = '';
            noteRightTemplate = '';
            cssBorderClass = '';
            if (currentCategory != scope.headerfields[p].SubcategoryId) {
              cssBorderClass = 'leftDoubleBorder';
            } else if (p == scope.headerfields.length - 1) {
              cssBorderClass = 'rightDoubleBorder';
            }
            for (var j = 0; j < scope.tdds.length; j++) {
              currentColor = scope.tdds[j].Hex;
              var currentTDDID = scope.tdds[j].Id;
              //TODO, need to do a foreach of the headerfields and output them
              for (var i = 0; i < scope.studentresult.FieldResultsByTestDueDate.length; i++) {
                if (scope.studentresult.FieldResultsByTestDueDate[i].TDDID === currentTDDID) {
                  // we've found the color for this one, do we color the cell or not?
                  for (var k = 0; k < scope.studentresult.FieldResultsByTestDueDate[i].FieldResults.length; k++) {
                    // find the right field, then check to see if it should be colored in
                    if (scope.studentresult.FieldResultsByTestDueDate[i].FieldResults[k].GroupId === field.GroupId) {
                      // do the comment here for each tdd, only add one icon
                      if (scope.studentresult.FieldResultsByTestDueDate[i].FieldResults[k].Comment != null && scope.studentresult.FieldResultsByTestDueDate[i].FieldResults[k].Comment != '') {
                        if (currentNoteText === '') {
                          noteLeftTemplate = '<i popover-trigger="mouseenter" class="fa fa-comments" style="margin-left:5px;cursor:pointer" uib-popover-html="toolTipFunction(';
                          noteRightTemplate = ');" popover-title="Comments"></i>';
                        }
                        var d = new Date(Date.parse(scope.tdds[j].DueDate));
                        currentNoteText += '\'' + scope.tdds[j].Id + '\',\'' + (d.getMonth() + 1) + '/' + d.getFullYear() + '\',' + i + ',' + k + ',';
                      }
                    }
                  }
                }
              }
              for (var n = 0; n < scope.studentresult.SummaryFieldResults.length; n++) {
                // find the right field, then check to see if it should be colored in
                if (scope.studentresult.SummaryFieldResults[n].GroupId === field.GroupId) {
                  // NEW
                  if (scope.studentresult.SummaryFieldResults[n].XColorDate != null) {
                    var hexColor = '';
                    for (var t = 0; t < scope.tdds.length; t++) {
                      if (scope.tdds[t].DueDate == scope.studentresult.SummaryFieldResults[n].XColorDate) {
                        hexColor = scope.tdds[t].Hex;
                      }
                    }
                    currentInnerText = '<span style=\'color:' + hexColor + '\'>X</span>';
                  }
                  if (scope.studentresult.SummaryFieldResults[n].CellColorDate != null) {
                    var hexColor = '';
                    for (var t = 0; t < scope.tdds.length; t++) {
                      if (scope.tdds[t].DueDate == scope.studentresult.SummaryFieldResults[n].CellColorDate) {
                        hexColor = scope.tdds[t].Hex;
                      }
                    }
                    currentCellLeftHtml = '<td class=\'' + cssBorderClass + '\' style=\'font-weight:bold;text-align:center;background-color:' + hexColor + '\'>';
                  }
                }
              }
            }
            // remove trailing comma on currentNotText
            if (currentNoteText.length > 0) {
              currentNoteText = currentNoteText.substring(0, currentNoteText.length - 1);
            }
            currentCategory = scope.headerfields[p].SubcategoryId;
            // add an empty cell if none of the test due dates are checked
            if (currentCellLeftHtml === '') {
              if (noteLeftTemplate === '') {
                rowHtml += '<td class=\'' + cssBorderClass + '\' ></td>';
              } else {
                rowHtml += '<td class=\'' + cssBorderClass + '\' >' + noteLeftTemplate + currentNoteText + noteRightTemplate + '</td>';
              }
            } else {
              if (noteLeftTemplate === '') {
                rowHtml += currentCellLeftHtml + currentInnerText + currentCellRightHtml;
              } else {
                rowHtml += currentCellLeftHtml + currentInnerText + noteLeftTemplate + currentNoteText + noteRightTemplate + currentCellRightHtml;
              }
            }
          }
          //element.html("cheese");
          element.html(rowHtml);
          $compile(element.contents())(scope);
        }
      };
    }
  ]).directive('hrsiwSectionReportTableRow', [
    '$routeParams',
    '$compile',
    '$templateCache',
    '$http',
    function ($routeParams, $compile, $templateCache, $http) {
      return {
        restrict: 'A',
        scope: {
          headerfields: '=',
          studentresult: '=',
          tdds: '='
        },
        link: function (scope, element, attr) {
          var currentColor = '';
          //var previousColor = '';
          var currentCellLeftHtml = '';
          var currentCellRightHtml = '</td>';
          var rowHtml = '';
          var currentInnerText = '';
          var currentNoteText = '';
          var noteLeftTemplate = '';
          var noteRightTemplate = '';
          var currentCategory = 0;
          var cssBorderClass = '';
          scope.toolTipFunction = function (tddId, date, duedateIndex, tddId2, date2, duedateIndex2, tddId3, date3, duedateIndex3, tddId4, date4, duedateIndex4) {
            var returnString = '';
            returnString = '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex].Comments + '</span></div>';
            if (angular.isDefined(tddId2)) {
              returnString += '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId2 + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date2 + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex2].Comments + '</span></div>';
            }
            if (angular.isDefined(tddId3)) {
              returnString += '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId3 + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date3 + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex3].Comments + '</span></div>';
            }
            if (angular.isDefined(tddId4)) {
              returnString += '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId4 + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date4 + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex4].Comments + '</span></div>';
            }
            return returnString;
          };
          rowHtml += '<td>' + scope.studentresult.LastName + ',' + scope.studentresult.FirstName + '</td>';
          // add the comments column
          for (var j = 0; j < scope.tdds.length; j++) {
            currentColor = scope.tdds[j].Hex;
            var currentTDDID = scope.tdds[j].Id;
            for (var i = 0; i < scope.studentresult.FieldResultsByTestDueDate.length; i++) {
              if (scope.studentresult.FieldResultsByTestDueDate[i].TDDID === currentTDDID) {
                // do the comment here for each tdd, only add one icon
                if (scope.studentresult.FieldResultsByTestDueDate[i].Comments != null && scope.studentresult.FieldResultsByTestDueDate[i].Comments != '') {
                  if (currentNoteText === '') {
                    //currentNoteText += '<i class="fa fa-paperclip" style="cursor:pointer" popover-template="&apos;commentCell_' + scope.studentresult.StudentId + '_' + field.Id + '&apos;" popover-title="Comments"></i>';
                    noteLeftTemplate = '<i popover-trigger="mouseenter" class="fa fa-comments" style="margin-left:5px;cursor:pointer" uib-popover-html="toolTipFunction(';
                    noteRightTemplate = ');" popover-title="Comments"></i>';
                  }
                  var d = new Date(Date.parse(scope.tdds[j].DueDate));
                  currentNoteText += '\'' + scope.tdds[j].Id + '\',\'' + (d.getMonth() + 1) + '/' + d.getFullYear() + '\',' + i + ',';  //"<div><span class='orange' style='border:1px solid #666666;display:inline-block;height:10px;width:25px;Background-color:" + scope.tdds[j].Hex + ";'></span><span>&nbsp;" + (d.getMonth() + 1) + "/" + d.getFullYear() + "</span>&nbsp;&nbsp;--&nbsp;&nbsp;<span style='font-weight:bold'>" + scope.studentresult.FieldResultsByTestDueDate[i].FieldResults[k].Comment + "</span></div>";
                                                                                                                                        //noteLeftTemplate += "busted"; 
                }
              }
            }
          }
          // remove trailing comma on currentNotText
          if (currentNoteText.length > 0) {
            currentNoteText = currentNoteText.substring(0, currentNoteText.length - 1);
          }
          rowHtml += '<td class=\'nsCenterAlignedText\'>' + noteLeftTemplate + currentNoteText + noteRightTemplate + '</td>';
          // loop over the tdds for each studentresult and find the latest one, then get the hex for it?
          for (var p = 0; p < scope.headerfields.length; p++) {
            var field = scope.headerfields[p];
            currentCellLeftHtml = '';
            currentInnerText = '';
            currentNoteText = '';
            noteLeftTemplate = '';
            noteRightTemplate = '';
            for (var j = 0; j < scope.tdds.length; j++) {
              currentColor = scope.tdds[j].Hex;
              var currentTDDID = scope.tdds[j].Id;
              //TODO, need to do a foreach of the headerfields and output them
              for (var n = 0; n < scope.studentresult.SummaryFieldResults.length; n++) {
                // find the right field, then check to see if it should be colored in
                if (scope.studentresult.SummaryFieldResults[n].DbColumn === field.DatabaseColumn) {
                  // NEW
                  if (scope.studentresult.SummaryFieldResults[n].XColorDate != null) {
                    var hexColor = '';
                    for (var t = 0; t < scope.tdds.length; t++) {
                      if (scope.tdds[t].DueDate == scope.studentresult.SummaryFieldResults[n].XColorDate) {
                        hexColor = scope.tdds[t].Hex;
                      }
                    }
                    currentInnerText = '<span class=\'badge nsBadgeBackground\' style=\'color:' + hexColor + '\'>X</span>';
                  }
                  if (scope.studentresult.SummaryFieldResults[n].CellColorDate != null) {
                    var hexColor = '';
                    for (var t = 0; t < scope.tdds.length; t++) {
                      if (scope.tdds[t].DueDate == scope.studentresult.SummaryFieldResults[n].CellColorDate) {
                        hexColor = scope.tdds[t].Hex;
                      }
                    }
                    currentCellLeftHtml = '<td class=\'' + cssBorderClass + '\' style=\'font-weight:bold;text-align:center;background-color:' + hexColor + '\'>';
                  }
                }
              }
            }
            currentCategory = scope.headerfields[p].SubcategoryId;
            // add an empty cell if none of the test due dates are checked
            if (currentCellLeftHtml === '') {
              rowHtml += '<td></td>';
            } else {
              rowHtml += currentCellLeftHtml + currentInnerText + currentCellRightHtml;
            }
          }
          //element.html("cheese");
          element.html(rowHtml);
          $compile(element.contents())(scope);
        }
      };
    }
  ]).directive('lidSectionReportTableRow', [
    '$routeParams',
    '$compile',
    '$templateCache',
    '$http',
    function ($routeParams, $compile, $templateCache, $http) {
      return {
        restrict: 'A',
        scope: {
          headerfields: '=',
          studentresult: '=',
          tdds: '='
        },
        link: function (scope, element, attr) {
          var currentColor = '';
          //var previousColor = '';
          var currentCellLeftHtml = '';
          var currentCellRightHtml = '</td>';
          var rowHtml = '';
          var currentInnerText = '';
          var currentNoteText = '';
          var noteLeftTemplate = '';
          var noteRightTemplate = '';
          var currentCategory = 0;
          var cssBorderClass = '';
          scope.toolTipFunction = function (tddId, date, duedateIndex, resultIndex, tddId2, date2, duedateIndex2, resultIndex2, tddId3, date3, duedateIndex3, resultIndex3, tddId4, date4, duedateIndex4, resultIndex4) {
            var returnString = '';
            returnString = '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex].FieldResults[resultIndex].Comment + '</span></div>';
            if (angular.isDefined(tddId2)) {
              returnString += '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId2 + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date2 + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex2].FieldResults[resultIndex2].Comment + '</span></div>';
            }
            if (angular.isDefined(tddId3)) {
              returnString += '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId3 + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date3 + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex3].FieldResults[resultIndex3].Comment + '</span></div>';
            }
            if (angular.isDefined(tddId4)) {
              returnString += '<div class="commentSpacing"><span class="commentCellBox tddBg' + tddId4 + '">&nbsp;</span><span class="boldCommentDate">&nbsp;' + date4 + '</span>&nbsp;&nbsp;:&nbsp;&nbsp;<span class="normalCommentText">' + scope.studentresult.FieldResultsByTestDueDate[duedateIndex4].FieldResults[resultIndex4].Comment + '</span></div>';
            }
            return returnString;
          };
          rowHtml += '<td><span student-dashboard-link student-id="' + scope.studentresult.StudentId + '" student-name="\'' + scope.studentresult.LastName.replace(/'/g, '\\\'') + ', ' + scope.studentresult.FirstName.replace(/'/g, '\\\'') + '\'"></span></td>';
          //rowHtml += "<td>" + scope.studentresult.LastName + "," + scope.studentresult.FirstName + "</td>";
          // loop over the tdds for each studentresult and find the latest one, then get the hex for it?
          for (var p = 0; p < scope.headerfields.length; p++) {
            var field = scope.headerfields[p];
            currentCellLeftHtml = '';
            currentInnerText = '';
            currentNoteText = '';
            noteLeftTemplate = '';
            noteRightTemplate = '';
            cssBorderClass = '';
            if (currentCategory != scope.headerfields[p].SubcategoryId) {
              cssBorderClass = 'leftDoubleBorder';
            } else if (p == scope.headerfields.length - 1) {
              cssBorderClass = 'rightDoubleBorder';
            }
            for (var j = 0; j < scope.tdds.length; j++) {
              currentColor = scope.tdds[j].Hex;
              var currentTDDID = scope.tdds[j].Id;
              //TODO, need to do a foreach of the headerfields and output them
              for (var i = 0; i < scope.studentresult.FieldResultsByTestDueDate.length; i++) {
                if (scope.studentresult.FieldResultsByTestDueDate[i].TDDID === currentTDDID) {
                  // we've found the color for this one, do we color the cell or not?
                  for (var k = 0; k < scope.studentresult.FieldResultsByTestDueDate[i].FieldResults.length; k++) {
                    // find the right field, then check to see if it should be colored in
                    if (scope.studentresult.FieldResultsByTestDueDate[i].FieldResults[k].GroupId === field.GroupId) {
                      // do the comment here for each tdd, only add one icon
                      if (scope.studentresult.FieldResultsByTestDueDate[i].FieldResults[k].Comment != null && scope.studentresult.FieldResultsByTestDueDate[i].FieldResults[k].Comment != '') {
                        if (currentNoteText === '') {
                          noteLeftTemplate = '<i popover-trigger="mouseenter" class="fa fa-comments" style="margin-left:5px;cursor:pointer" uib-popover-html="toolTipFunction(';
                          noteRightTemplate = ');" popover-title="Comments"></i>';
                        }
                        var d = new Date(Date.parse(scope.tdds[j].DueDate));
                        currentNoteText += '\'' + scope.tdds[j].Id + '\',\'' + (d.getMonth() + 1) + '/' + d.getFullYear() + '\',' + i + ',' + k + ',';  //if (currentNoteText === '') {
                                                                                                                                                        //    //currentNoteText += '<i class="fa fa-paperclip" style="cursor:pointer" popover-template="&apos;commentCell_' + scope.studentresult.StudentId + '_' + field.Id + '&apos;" popover-title="Comments"></i>';
                                                                                                                                                        //    noteLeftTemplate = '<i popover-trigger="mouseenter" class="fa fa-comments" style="margin-left:5px;cursor:pointer" popover="';
                                                                                                                                                        //    noteRightTemplate = '" popover-title="Comments"></i>';
                                                                                                                                                        //}
                                                                                                                                                        //var d = new Date(Date.parse(scope.tdds[j].DueDate));
                                                                                                                                                        //currentNoteText += "<div><span class='orange' style='border:1px solid #666666;display:inline-block;height:10px;width:25px;Background-color:" + scope.tdds[j].Hex + ";'></span><span>&nbsp;" + (d.getMonth() + 1) + "/" + d.getFullYear() + "</span>&nbsp;&nbsp;--&nbsp;&nbsp;<span style='font-weight:bold'>" + scope.studentresult.FieldResultsByTestDueDate[i].FieldResults[k].Comment + "</span></div>";
                                                                                                                                                        ////noteLeftTemplate += "busted";
                      }
                    }
                  }
                }
              }
              for (var n = 0; n < scope.studentresult.SummaryFieldResults.length; n++) {
                // find the right field, then check to see if it should be colored in
                if (scope.studentresult.SummaryFieldResults[n].GroupId === field.GroupId) {
                  // NEW
                  if (scope.studentresult.SummaryFieldResults[n].XColorDate != null) {
                    var hexColor = '';
                    for (var t = 0; t < scope.tdds.length; t++) {
                      if (scope.tdds[t].DueDate == scope.studentresult.SummaryFieldResults[n].XColorDate) {
                        hexColor = scope.tdds[t].Hex;
                      }
                    }
                    currentInnerText = '<span style=\'color:' + hexColor + '\'>X</span>';
                  }
                  if (scope.studentresult.SummaryFieldResults[n].CellColorDate != null) {
                    var hexColor = '';
                    for (var t = 0; t < scope.tdds.length; t++) {
                      if (scope.tdds[t].DueDate == scope.studentresult.SummaryFieldResults[n].CellColorDate) {
                        hexColor = scope.tdds[t].Hex;
                      }
                    }
                    currentCellLeftHtml = '<td class=\'' + cssBorderClass + '\' style=\'font-weight:bold;text-align:center;background-color:' + hexColor + '\'>';
                  }
                }
              }
            }
            // remove trailing comma on currentNotText
            if (currentNoteText.length > 0) {
              currentNoteText = currentNoteText.substring(0, currentNoteText.length - 1);
            }
            currentCategory = scope.headerfields[p].SubcategoryId;
            // add an empty cell if none of the test due dates are checked
            if (currentCellLeftHtml === '') {
              if (noteLeftTemplate === '') {
                rowHtml += '<td class=\'' + cssBorderClass + '\' ></td>';
              } else {
                rowHtml += '<td class=\'' + cssBorderClass + '\' >' + noteLeftTemplate + currentNoteText + noteRightTemplate + '</td>';
              }
            } else {
              if (noteLeftTemplate === '') {
                rowHtml += currentCellLeftHtml + currentInnerText + currentCellRightHtml;
              } else {
                rowHtml += currentCellLeftHtml + currentInnerText + noteLeftTemplate + currentNoteText + noteRightTemplate + currentCellRightHtml;
              }
            }
          }
          //element.html("cheese");
          element.html(rowHtml);
          $compile(element.contents())(scope);
        }
      };
    }
  ]).controller('WVSectionReportController', WVSectionReportController).controller('FPSectionReportController', FPSectionReportController).controller('CAPSectionReportController', CAPSectionReportController).controller('HRSIWSectionReportController', HRSIWSectionReportController).controller('LIDSectionAlphaReportController', LIDSectionAlphaReportController).controller('LIDSectionSoundReportController', LIDSectionSoundReportController).controller('LIDSectionOverallReportController', LIDSectionOverallReportController).controller('SpellingInventorySectionReportController', SpellingInventorySectionReportController);
  WVSectionReportController.$inject = [
    '$scope',
    '$q',
    '$http',
    'nsPinesService',
    '$location',
    '$filter',
    '$routeParams',
    'nsFilterOptionsService',
    'WVReportManager',
    'webApiBaseUrl',
    'spinnerService',
    '$timeout',
    'FileSaver',
    '$bootbox'
  ];
  FPSectionReportController.$inject = [
    '$scope',
    '$q',
    '$http',
    'nsPinesService',
    '$location',
    '$filter',
    '$routeParams',
    'nsFilterOptionsService',
    'FPReportManager',
    'webApiBaseUrl',
    'spinnerService',
    '$timeout',
    'FileSaver',
    '$bootbox'
  ];
  HRSIWSectionReportController.$inject = [
    '$scope',
    '$q',
    '$http',
    'pinesNotifications',
    '$location',
    '$filter',
    '$routeParams',
    'nsFilterOptionsService',
    'HRSIWReportManager'
  ];
  CAPSectionReportController.$inject = [
    '$scope',
    '$q',
    '$http',
    'pinesNotifications',
    '$location',
    '$filter',
    '$routeParams',
    'nsFilterOptionsService',
    'webApiBaseUrl'
  ];
  LIDSectionAlphaReportController.$inject = [
    '$scope',
    '$q',
    '$http',
    'pinesNotifications',
    '$location',
    '$filter',
    '$routeParams',
    'nsFilterOptionsService',
    'LIDReportManager'
  ];
  LIDSectionSoundReportController.$inject = [
    '$scope',
    '$q',
    '$http',
    'pinesNotifications',
    '$location',
    '$filter',
    '$routeParams',
    'nsFilterOptionsService',
    'LIDReportManager'
  ];
  LIDSectionOverallReportController.$inject = [
    '$scope',
    '$q',
    '$http',
    'pinesNotifications',
    '$location',
    '$filter',
    '$routeParams',
    'nsFilterOptionsService',
    'LIDReportManager'
  ];
  SpellingInventorySectionReportController.$inject = [
    '$scope',
    '$q',
    '$http',
    'pinesNotifications',
    '$location',
    '$filter',
    '$routeParams',
    'nsFilterOptionsService',
    'webApiBaseUrl',
    'SpellReportManager',
    'NSSortManager'
  ];
  function WVSectionReportController($scope, $q, $http, nsPinesService, $location, $filter, $routeParams, nsFilterOptionsService, WVReportManager, webApiBaseUrl, spinnerService, $timeout, FileSaver, $bootbox) {
    $scope.filterOptions = nsFilterOptionsService.options;
    $scope.dataMgr = new WVReportManager();
    $scope.settings = { printInProgress: false };
    $scope.print = function () {
      if ($scope.settings.printInProgress) {
        $bootbox.alert('Please wait... another print job is already in progress.');
        return;
      }
      var returnObj = {
          PrintLandscape: false,
          PrintMultiPage: false,
          Url: $location.absUrl()
        };
      $scope.settings.printInProgress = true;
      var notice = nsPinesService.startDynamic();
      var printMethod = 'PrintPage';
      if (webApiBaseUrl.indexOf('localhost') > 0) {
        printMethod = 'PrintPageLocal';
      }
      $http.post(webApiBaseUrl + '/api/Print/' + printMethod, returnObj, {
        responseType: 'arraybuffer',
        headers: { accept: 'application/pdf' }
      }).then(function (data) {
        var blob = new Blob([data.data], { type: 'application/pdf' });
        FileSaver.saveAs(blob, 'NorthStarPrint.pdf');
      }).finally(function () {
        $scope.settings.printInProgress = false;
        nsPinesService.endDynamic(notice);
      });
    };
    $scope.$watch('filterOptions.selectedSection.id', function (newValue, oldValue) {
      if (!angular.equals(newValue, oldValue)) {
        LoadData();
      }
    });
    var LoadData = function () {
      if ($scope.filterOptions.selectedSection != null) {
        $scope.dataMgr.LoadData($routeParams.id, $scope.filterOptions.selectedSection.id, $scope.filterOptions.selectedSchoolYear.id);
      }
    };
    // intial load if section already selected
    LoadData();
  }
  function FPSectionReportController($scope, $q, $http, nsPinesService, $location, $filter, $routeParams, nsFilterOptionsService, FPReportManager, webApiBaseUrl, spinnerService, $timeout, FileSaver, $bootbox) {
    $scope.filterOptions = nsFilterOptionsService.options;
    $scope.dataMgr = new FPReportManager();
    $scope.settings = { printInProgress: false };
    $scope.print = function () {
      if ($scope.settings.printInProgress) {
        $bootbox.alert('Please wait... another print job is already in progress.');
        return;
      }
      var returnObj = {
          PrintLandscape: false,
          PrintMultiPage: false,
          Url: $location.absUrl()
        };
      $scope.settings.printInProgress = true;
      var notice = nsPinesService.startDynamic();
      var printMethod = 'PrintPage';
      if (webApiBaseUrl.indexOf('localhost') > 0) {
        printMethod = 'PrintPageLocal';
      }
      $http.post(webApiBaseUrl + '/api/Print/' + printMethod, returnObj, {
        responseType: 'arraybuffer',
        headers: { accept: 'application/pdf' }
      }).then(function (data) {
        var blob = new Blob([data.data], { type: 'application/pdf' });
        FileSaver.saveAs(blob, 'NorthStarPrint.pdf');
      }).finally(function () {
        $scope.settings.printInProgress = false;
        nsPinesService.endDynamic(notice);
      });
    };
    $scope.$watch('filterOptions.selectedSection.id', function (newValue, oldValue) {
      if (!angular.equals(newValue, oldValue)) {
        LoadData();
      }
    });
    $scope.lowBenchmark = function () {
      for (var p = 0; p < $scope.dataMgr.scale.length; p++) {
        if ($scope.dataMgr.soyBenchmark.Meets == $scope.dataMgr.scale[p].FPID) {
          return $scope.dataMgr.scale[p].FPs;
        }
      }
    };
    $scope.highBenchmark = function () {
      for (var p = 0; p < $scope.dataMgr.scale.length; p++) {
        if ($scope.dataMgr.eoyBenchmark.Meets == $scope.dataMgr.scale[p].FPID) {
          return $scope.dataMgr.scale[p].FPs;
        }
      }
    };
    $scope.targetZone = function () {
      var start = '', end = '';
      for (var p = 0; p < $scope.dataMgr.scale.length; p++) {
        if ($scope.dataMgr.targetZone.Meets == $scope.dataMgr.scale[p].FPID) {
          start = $scope.dataMgr.scale[p].FPs;
          break;
        }
      }
      for (var p = 0; p < $scope.dataMgr.scale.length; p++) {
        if ($scope.dataMgr.targetZone.Exceeds == $scope.dataMgr.scale[p].FPID) {
          end = $scope.dataMgr.scale[p].FPs;
          break;
        }
      }
      return start + '-' + end;  //scope.scaleRow.FPID >= scope.targetZone.Meets && scope.scaleRow.FPID < scope.targetZone.Exceeds
    };
    var LoadData = function () {
      if ($scope.filterOptions.selectedSection != null) {
        $timeout(function () {
          spinnerService.show('tableSpinner');
        });
        $scope.dataMgr.LoadData($routeParams.id, $scope.filterOptions.selectedSection.id, $scope.filterOptions.selectedSchoolYear.id).finally(function () {
          spinnerService.hide('tableSpinner');
        });
      }
    };
    // intial load if section already selected
    LoadData();
  }
  function HRSIWSectionReportController($scope, $q, $http, pinesNotifications, $location, $filter, $routeParams, nsFilterOptionsService, HRSIWReportManager) {
    $scope.reportType = 'Alphabet Response';
    $scope.filterOptions = nsFilterOptionsService.options;
    $scope.dataMgr = new HRSIWReportManager();
    $scope.nameHeaderClass = 'fa';
    $scope.showForm = function () {
      return $scope.filterOptions.selectedSection != null && $scope.filterOptions.selectedHrsForm != null;
    };
    $scope.$watch('filterOptions.selectedSection.id', function (newValue, oldValue) {
      if (!angular.equals(newValue, oldValue)) {
        LoadData();
      }
    });
    $scope.$watch('filterOptions.selectedHrsForm.id', function (newValue, oldValue) {
      if (!angular.equals(newValue, oldValue)) {
        LoadData();
      }
    });
    var LoadData = function () {
      if ($scope.filterOptions.selectedSection != null && $scope.filterOptions.selectedHrsForm != null) {
        //Todo: CHECK FORMID(assessmentId, sectionId, reportType, schoolYear)
        $scope.dataMgr.LoadData($routeParams.id, $scope.filterOptions.selectedSection.id, $scope.reportType, $scope.filterOptions.selectedSchoolYear.id, $scope.filterOptions.selectedHrsForm.id);
      }
    };
    // intial load if section already selected
    LoadData();
  }
  function LIDSectionAlphaReportController($scope, $q, $http, pinesNotifications, $location, $filter, $routeParams, nsFilterOptionsService, LIDReportManager) {
    $scope.reportType = 'Alphabet Response';
    $scope.filterOptions = nsFilterOptionsService.options;
    $scope.dataMgr = new LIDReportManager();
    $scope.nameHeaderClass = 'fa';
    $scope.$watch('filterOptions.selectedSection.id', function (newValue, oldValue) {
      if (!angular.equals(newValue, oldValue)) {
        //var b = $httpParamSerializer($scope.filterOptions);
        LoadData();
      }
    });
    var LoadData = function () {
      if ($scope.filterOptions.selectedSection != null) {
        //(assessmentId, sectionId, reportType, schoolYear)
        $scope.dataMgr.LoadData($routeParams.id, $scope.filterOptions.selectedSection.id, $scope.reportType, $scope.filterOptions.selectedSchoolYear.id);
      }
    };
    // intial load if section already selected
    LoadData();
  }
  function LIDSectionSoundReportController($scope, $q, $http, pinesNotifications, $location, $filter, $routeParams, nsFilterOptionsService, LIDReportManager) {
    $scope.reportType = 'Sound Response';
    $scope.filterOptions = nsFilterOptionsService.options;
    $scope.dataMgr = new LIDReportManager();
    $scope.nameHeaderClass = 'fa';
    $scope.$watch('filterOptions.selectedSection.id', function (newValue, oldValue) {
      if (!angular.equals(newValue, oldValue)) {
        //var b = $httpParamSerializer($scope.filterOptions);
        LoadData();
      }
    });
    var LoadData = function () {
      if ($scope.filterOptions.selectedSection != null) {
        //(assessmentId, sectionId, reportType, schoolYear)
        $scope.dataMgr.LoadData($routeParams.id, $scope.filterOptions.selectedSection.id, $scope.reportType, $scope.filterOptions.selectedSchoolYear.id);
      }
    };
    // intial load if section already selected
    LoadData();
  }
  function LIDSectionOverallReportController($scope, $q, $http, pinesNotifications, $location, $filter, $routeParams, nsFilterOptionsService, LIDReportManager) {
    $scope.reportType = 'Overall Response';
    $scope.filterOptions = nsFilterOptionsService.options;
    $scope.dataMgr = new LIDReportManager();
    $scope.nameHeaderClass = 'fa';
    $scope.$watch('filterOptions.selectedSection.id', function (newValue, oldValue) {
      if (!angular.equals(newValue, oldValue)) {
        //var b = $httpParamSerializer($scope.filterOptions);
        LoadData();
      }
    });
    var LoadData = function () {
      if ($scope.filterOptions.selectedSection != null) {
        //(assessmentId, sectionId, reportType, schoolYear)
        $scope.dataMgr.LoadData($routeParams.id, $scope.filterOptions.selectedSection.id, $scope.reportType, $scope.filterOptions.selectedSchoolYear.id);
      }
    };
    // intial load if section already selected
    LoadData();
  }
  function CAPSectionReportController($scope, $q, $http, pinesNotifications, $location, $filter, $routeParams, nsFilterOptionsService, webApiBaseUrl) {
    $scope.assessment = {};
    $scope.studentSectionReportResults = [];
    $scope.tdds = [];
    $scope.HeaderFields = [];
    $scope.filterOptions = nsFilterOptionsService.options;
    $scope.subCategoryColSpan = function (subCategoryId) {
      var colSpan = 0;
      for (var i = 0; i < $scope.HeaderFields.length; i++) {
        if ($scope.HeaderFields[i].SubcategoryId == subCategoryId) {
          colSpan++;
        }
      }
      return colSpan;
    };
    var LoadData = function () {
      if ($scope.filterOptions.selectedSection != null) {
        var paramObj = {
            AssessmentId: $routeParams.id,
            SchoolYear: $scope.filterOptions.selectedSchoolYear.id,
            SectionId: $scope.filterOptions.selectedSection.id
          };
        $http.post(webApiBaseUrl + '/api/sectionreport/GetCAPSectionReport', paramObj).success(function (data) {
          $scope.assessment = data.Assessment;
          $scope.studentSectionReportResults = data.StudentSectionReportResults;
          $scope.tdds = data.TestDueDates;
          $scope.HeaderFields = data.HeaderFields;
          var currentCssClass = '';
          var currentSubCatId = 0;
          for (var i = 0; i < $scope.HeaderFields.length; i++) {
            if ($scope.HeaderFields[i].SubcategoryId != currentSubCatId) {
              $scope.HeaderFields[i].cssClass = 'leftDoubleBorder';
            } else if (i === $scope.HeaderFields.length - 1) {
              $scope.HeaderFields[i].cssClass = 'rightDoubleBorder';
            }
            currentSubCatId = $scope.HeaderFields[i].SubcategoryId;
          }
        });
      }
    };
    $scope.$watch('filterOptions.selectedSection.id', function (newValue, oldValue) {
      if (!angular.equals(newValue, oldValue)) {
        //var b = $httpParamSerializer($scope.filterOptions);
        LoadData();
      }
    });
    $scope.$watch('filterOptions.selectedBenchmarkDate.id', function (newValue, oldValue) {
      if (!angular.equals(newValue, oldValue)) {
        //var b = $httpParamSerializer($scope.filterOptions);
        LoadData();
      }
    });
    $scope.sortArray = [];
    $scope.headerClassArray = [];
    $scope.nameHeaderClass = 'fa';
    $scope.sort = function (column) {
      var columnIndex = -1;
      // if this is not a first or lastname column
      if (!isNaN(parseInt(column))) {
        columnIndex = column;
        column = 'SummaryFieldResults[' + column + '].CellColorDate';
      }
      var bFound = false;
      for (var j = 0; j < $scope.sortArray.length; j++) {
        // if it is already on the list, reverse the sort
        if ($scope.sortArray[j].indexOf(column) >= 0) {
          bFound = true;
          // is it already negative? if so, remove it
          if ($scope.sortArray[j].indexOf('-') === 0) {
            if (columnIndex > -1) {
              $scope.headerClassArray[columnIndex] = 'fa';
            } else if (column === 'LastName') {
              $scope.nameHeaderClass = 'fa';
            }
            $scope.sortArray.splice(j, 1);
          } else {
            if (columnIndex > -1) {
              $scope.headerClassArray[columnIndex] = 'fa fa-chevron-down';
            } else if (column === 'LastName') {
              $scope.nameHeaderClass = 'fa fa-chevron-down';
            }
            $scope.sortArray[j] = '-' + $scope.sortArray[j];
          }
          break;
        }
      }
      if (!bFound) {
        $scope.sortArray.push(column);
        if (columnIndex > -1) {
          $scope.headerClassArray[columnIndex] = 'fa fa-chevron-up';
        } else if (column === 'LastName') {
          $scope.nameHeaderClass = 'fa fa-chevron-up';
        }
      }
    };
    for (var r = 0; r < $scope.HeaderFields.length; r++) {
      $scope.headerClassArray[r] = 'fa';
    }
    LoadData();
  }
  function SpellingInventorySectionReportController($scope, $q, $http, pinesNotifications, $location, $filter, $routeParams, nsFilterOptionsService, webApiBaseUrl, SpellReportManager, NSSortManager) {
    $scope.filterOptions = nsFilterOptionsService.options;
    $scope.dataMgr = new SpellReportManager();
    $scope.manualSortHeaders = {};
    $scope.manualSortHeaders.studentNameHeaderClass = 'fa';
    $scope.manualSortHeaders.fpValueIDHeaderClass = 'fa';
    $scope.manualSortHeaders.totalScoreHeaderClass = 'fa';
    $scope.sortArray = [];
    $scope.headerClassArray = [];
    $scope.allSelected = false;
    $scope.sortMgr = new NSSortManager();
    $scope.$watch('filterOptions.selectedSection.id', function (newValue, oldValue) {
      if (!angular.equals(newValue, oldValue)) {
        LoadData();
      }
    });
    $scope.spellingCellBgColor = function (fieldResult) {
      if (fieldResult.IntValue != null) {
        if (fieldResult.Field.OutOfHowMany - fieldResult.IntValue <= 1) {
          return 'obsGreen';
        } else if (fieldResult.Field.OutOfHowMany - fieldResult.IntValue <= 2) {
          return 'obsYellow';
        } else {
          return '';
        }
      }
    };
    $scope.$watch('filterOptions.selectedBenchmarkDate.id', function (newValue, oldValue) {
      if (!angular.equals(newValue, oldValue)) {
        LoadData();
      }
    });
    $scope.navigateToTdd = function (tddid) {
      $scope.filterOptions.selectedBenchmarkDate = nsFilterOptionsService.getBenchmarkDateById(tddid);
      nsFilterOptionsService.changeBenchmarkDate();
    };
    var LoadData = function () {
      if ($scope.filterOptions.selectedSection != null && $scope.filterOptions.selectedBenchmarkDate != null) {
        $scope.dataMgr.LoadData($routeParams.id, $scope.filterOptions.selectedSection.id, $scope.filterOptions.selectedBenchmarkDate.id).then(function (response) {
          $scope.sortMgr.initialize($scope.manualSortHeaders, $scope.sortArray, $scope.headerClassArray, 'FieldResults', $scope.dataMgr.assessment);
        });
      }
    };
    $scope.totalFPAndWords = function (studentResult) {
      var totalScore = '0';
      var totalWSC = '0';
      var totalFP = '0';
      // do some error checking, etc to make sure IntValue is populated, etc
      for (var i = 0; i < studentResult.FieldResults.length; i++) {
        if (studentResult.FieldResults[i].DbColumn === 'totalScore') {
          totalScore = studentResult.FieldResults[i].IntValue === null ? '0' : studentResult.FieldResults[i].IntValue;
        } else if (studentResult.FieldResults[i].DbColumn === 'totalWSC') {
          totalWSC = studentResult.FieldResults[i].IntValue === null ? '0' : studentResult.FieldResults[i].IntValue;
        } else if (studentResult.FieldResults[i].DbColumn === 'totalFP') {
          totalFP = studentResult.FieldResults[i].IntValue === null ? '0' : studentResult.FieldResults[i].IntValue;
        }
      }
      return totalScore + ' (' + totalFP + '/' + totalWSC + ')';
    };
    // intial load if section already selected
    LoadData();
    $scope.sort = function (column) {
      $scope.sortMgr.sort(column);
    };  // default sort
        //$scope.sortArray.push("LastName");
        //$scope.sortArray.push("FirstName");
        //$scope.hasBeenSorted = false;
        //$scope.sort = function(column) {
        //    if(!$scope.hasBeenSorted) {
        //        $scope.sortArray = [];
        //        $scope.hasBeenSorted = true;
        //    }
        //	var columnIndex = -1;
        //	// if this is not a first or lastname column
        //	if (!isNaN(parseInt(column))) {
        //		columnIndex = column;
        //		switch ($scope.HeaderFields[column].FieldType) {
        //        case 'DateCheckbox':
        //			column = 'FieldResults[' + column + '].DateValue';
        //			break;
        //		case 'Textfield':
        //			column = 'FieldResults[' + column + '].StringValue';
        //			break;
        //		case 'DecimalRange':
        //			column = 'FieldResults[' + column + '].DecimalValue';
        //			break;
        //		case 'DropdownRange':
        //			column = 'FieldResults[' + column + '].IntValue';
        //			break;
        //		case 'DropdownFromDB':
        //			column = 'FieldResults[' + column + '].IntValue';
        //			break;
        //		case 'CalculatedFieldDbOnly':
        //			column = 'FieldResults[' + column + '].StringValue';
        //			break;
        //		case 'CalculatedFieldDbBacked':
        //			column = 'FieldResults[' + column + '].IntValue';
        //			break;
        //		case 'CalculatedFieldDbBackedString':
        //			column = 'FieldResults[' + column + '].StringValue';
        //			break;
        //		case 'CalculatedFieldClientOnly':
        //			column = 'FieldResults[' + column + '].StringValue'; //shouldnt even be used in sorting
        //			break;
        //		default:
        //			column = 'FieldResults[' + column + '].IntValue';
        //			break;
        //		}
        //	}
        //	var bFound = false;
        //	for (var j = 0; j < $scope.sortArray.length; j++) {
        //		// if it is already on the list, reverse the sort
        //		if ($scope.sortArray[j].indexOf(column) >= 0) {
        //			bFound = true;
        //			// is it already negative? if so, remove it
        //			if ($scope.sortArray[j].indexOf("-") === 0) {
        //				if (columnIndex > -1) {
        //					$scope.headerClassArray[columnIndex] = "fa";
        //				} else if (column === 'FirstName') {
        //					$scope.firstNameHeaderClass = "fa";
        //				} else if (column === 'LastName') {
        //					$scope.lastNameHeaderClass = "fa";
        //				} else if (column === 'FPValueID') {
        //					$scope.fpValueIDHeaderClass = "fa";
        //				} else if (column === 'TotalScore') {
        //					$scope.totalScoreHeaderClass = "fa";
        //				}
        //				$scope.sortArray.splice(j, 1);
        //                if($scope.sortArray.length === 0) {
        //                     $scope.sortArray.push("LastName");
        //                     $scope.sortArray.push("FirstName");
        //                     $scope.hasBeenSorted = false;
        //                }
        //			} else {
        //				if (columnIndex > -1) {
        //					$scope.headerClassArray[columnIndex] = "fa fa-chevron-down";
        //				} else if (column === 'FirstName') {
        //					$scope.firstNameHeaderClass = "fa fa-chevron-down";
        //				} else if (column === 'LastName') {
        //					$scope.lastNameHeaderClass = "fa fa-chevron-down";
        //				} else if (column === 'FPValueID') {
        //					$scope.fpValueIDHeaderClass = "fa fa-chevron-down";
        //				} else if (column === 'TotalScore') {
        //					$scope.totalScoreHeaderClass = "fa fa-chevron-down";
        //				}
        //				$scope.sortArray[j] = "-" + $scope.sortArray[j];
        //			}
        //			break;
        //		}
        //	}
        //	if (!bFound) {
        //		$scope.sortArray.push(column);
        //		if (columnIndex > -1) {
        //			$scope.headerClassArray[columnIndex] = "fa fa-chevron-up";
        //		} else if (column === 'FirstName') {
        //			$scope.firstNameHeaderClass = "fa fa-chevron-up";
        //		} else if (column === 'LastName') {
        //			$scope.lastNameHeaderClass = "fa fa-chevron-up";
        //		} else if (column === 'FPValueID') {
        //		    $scope.fpValueIDHeaderClass = "fa fa-chevron-up";
        //	    } else if (column === 'TotalScore') {
        //		    $scope.totalScoreHeaderClass = "fa fa-chevron-up";
        //	    }
        //	}
        //};
        //for (var r = 0; r < $scope.HeaderFields.length; r++) {
        //    $scope.headerClassArray[r] = 'fa';
        //}
  }  /* Utility Functions */
}());
(function () {
  'use strict';
  angular.module('sectionDataEntryModule', []).controller('NSStudentSectionDataEntryBaseController', [
    '$scope',
    '$q',
    '$http',
    'nsPinesService',
    '$location',
    '$filter',
    '$routeParams',
    'NSStudentSectionResultDataEntry',
    'nsFilterOptionsService',
    'nsLookupFieldService',
    'nsSelect2RemoteOptions',
    'NSUserInfoService',
    function ($scope, $q, $http, nsPinesService, $location, $filter, $routeParams, NSStudentSectionResultDataEntry, nsFilterOptionsService, nsLookupFieldService, nsSelect2RemoteOptions, NSUserInfoService) {
      $scope.StudentSectionResults = new NSStudentSectionResultDataEntry();
      $scope.filterOptions = nsFilterOptionsService.options;
      //$scope.errors = [];
      $scope.saveAssessmentData = function (studentResult) {
        $scope.StudentSectionResults.saveAssessmentResult($routeParams.assessmentId, studentResult, $routeParams.benchmarkDateId).then(function (data) {
          $location.path('section-assessment-resultlist/' + $routeParams.assessmentId);
          nsPinesService.dataSavedSuccessfully();
        });
      };
      $scope.cancel = function () {
        $location.path('section-assessment-resultlist/' + $routeParams.assessmentId);
      };
      $scope.LoadData = function () {
        console.time('Start load assessmentdata');
        $scope.filterOptions.selectedBenchmarkDate = typeof $routeParams.benchmarkDateId !== 'undefined' ? nsFilterOptionsService.getBenchmarkDateById($routeParams.benchmarkDateId) : $scope.filterOptions.selectedBenchmarkDate;
        $scope.StudentSectionResults.loadAssessmentStudentResultData($routeParams.assessmentId, $routeParams.classId, $routeParams.benchmarkDateId, $routeParams.studentId, $routeParams.studentResultId).then(function (data) {
          //$scope.lookupFieldsArray = data.data.Assessment.LookupFields;
          $scope.fields = data.data.Assessment.Fields;
          $scope.assessment = data.data.Assessment;
          $scope.studentResult = data.data.StudentResult;
          $scope.StudentSectionResults.attachFieldsToResults($scope.studentResult, $scope.fields, nsLookupFieldService.LookupFieldsArray);
          // set default recorder
          if ($scope.studentResult.Recorder == null || $scope.studentResult.Recorder.id == -1) {
            // add current user
            $scope.studentResult.Recorder = {
              id: NSUserInfoService.currentUser.Id,
              text: NSUserInfoService.currentUser.FullName
            };
          }
          if ($scope.studentResult.TestDate == null) {
            $scope.studentResult.TestDate = moment().format('DD-MMM-YYYY');
          } else {
            var momentizedDate = moment($scope.studentResult.TestDate);
            $scope.studentResult.TestDate = momentizedDate.format('DD-MMM-YYYY');
          }
          $scope.setDisplayStructure();
          console.timeEnd('Start load assessmentdata');
        });
      };
      $scope.formats = [
        'dd-MMM-yyyy',
        'yyyy/MM/dd',
        'dd.MM.yyyy',
        'shortDate'
      ];
      $scope.format = $scope.formats[0];
      // move this to a simple directive
      //$scope.$on('NSHTTPError', function (event, data) {
      //    $scope.errors.push({ type: "danger", msg: data });
      //    $('html, body').animate({ scrollTop: 0 }, 'fast');
      //});
      $scope.validateRecorder = function () {
        if (angular.isDefined($scope.studentResult) && $scope.studentResult.Recorder.id > 0) {
          return true;
        }
        return false;
      };
      $scope.popup1 = { opened: false };
      $scope.open1 = function () {
        $scope.popup1.opened = true;
      };
      $scope.getTeacherRemoteOptions = nsSelect2RemoteOptions.TeacherRemoteOptions;
    }
  ]).controller('SectionDataEntryController', SectionDataEntryController).controller('SectionStudentResultHFWDataEntryController', SectionStudentResultHFWDataEntryController).controller('SectionStudentResultSpellingDataEntryController', SectionStudentResultSpellingDataEntryController).controller('SectionStudentResultLetterIDDataEntryController', SectionStudentResultLetterIDDataEntryController).controller('SectionStudentResultHRSIWDataEntryController', SectionStudentResultHRSIWDataEntryController).controller('SectionStudentResultCAPDataEntryController', SectionStudentResultCAPDataEntryController);
  SectionDataEntryController.$inject = [
    '$httpParamSerializer',
    '$scope',
    '$q',
    '$http',
    'nsPinesService',
    '$location',
    '$filter',
    '$routeParams',
    'nsSectionDataEntryService',
    'nsFilterOptionsService',
    'NSSectionAssessmentDataEntryManager',
    'nsLookupFieldService',
    'nsSelect2RemoteOptions',
    '$bootbox',
    '$uibModal',
    'progressLoader',
    'NSUserInfoService',
    'FileSaver',
    'webApiBaseUrl',
    '$timeout',
    'authService',
    'nsGlobalSettings',
    'ckEditorSettings',
    'spinnerService'
  ];
  SectionStudentResultHFWDataEntryController.$inject = [
    '$scope',
    '$q',
    '$http',
    'pinesNotifications',
    '$location',
    '$filter',
    '$routeParams',
    '$bootbox',
    'webApiBaseUrl',
    '$timeout',
    'spinnerService',
    'FileSaver',
    'nsFilterOptionsService'
  ];
  SectionStudentResultSpellingDataEntryController.$inject = [
    '$scope',
    '$q',
    '$http',
    'nsPinesService',
    '$location',
    '$filter',
    '$routeParams',
    'NSStudentSectionResultDataEntry',
    'nsFilterOptionsService',
    '$controller',
    'nsSelect2RemoteOptions'
  ];
  SectionStudentResultLetterIDDataEntryController.$inject = [
    '$scope',
    '$q',
    '$http',
    'nsPinesService',
    '$location',
    '$filter',
    '$routeParams',
    'NSStudentSectionResultDataEntry',
    'nsFilterOptionsService',
    '$controller',
    'nsSelect2RemoteOptions'
  ];
  SectionStudentResultHRSIWDataEntryController.$inject = [
    '$scope',
    '$q',
    '$http',
    'nsPinesService',
    '$location',
    '$filter',
    '$routeParams',
    'NSStudentSectionResultDataEntry',
    'nsFilterOptionsService',
    '$controller',
    'nsSelect2RemoteOptions'
  ];
  SectionStudentResultCAPDataEntryController.$inject = [
    '$scope',
    '$q',
    '$http',
    'nsPinesService',
    '$location',
    '$filter',
    '$routeParams',
    'NSStudentSectionResultDataEntry',
    'nsFilterOptionsService',
    '$controller',
    'nsSelect2RemoteOptions'
  ];
  function SectionStudentResultLetterIDDataEntryController($scope, $q, $http, nsPinesService, $location, $filter, $routeParams, NSStudentSectionResultDataEntry, nsFilterOptionsService, $controller, nsSelect2RemoteOptions) {
    $controller('NSStudentSectionDataEntryBaseController', { $scope: $scope });
    $scope.totalFields = [];
    $scope.commentFields = [];
    $scope.page1FieldGroups = [];
    $scope.page2FieldGroups = [];
    $scope.headerColspan = function (index) {
      if (index == 0) {
        return '1';
      } else {
        return '2';
      }
    };
    $scope.toggleCategory = function (category, page, isChecked) {
      if (isChecked) {
        for (var i = 0; i < $scope.studentResult.FieldResults.length; i++) {
          if ($scope.studentResult.FieldResults[i].Field.CategoryId == category.Id && $scope.studentResult.FieldResults[i].Field.Page == page) {
            $scope.studentResult.FieldResults[i].BoolValue = true;
          }
        }
      } else {
        for (var i = 0; i < $scope.studentResult.FieldResults.length; i++) {
          if ($scope.studentResult.FieldResults[i].Field.CategoryId == category.Id && $scope.studentResult.FieldResults[i].Field.Page == page) {
            $scope.studentResult.FieldResults[i].BoolValue = false;
          }
        }
      }
    };
    $scope.setDisplayStructure = function () {
      // create scope fields house specific totals
      // set total fields
      for (var f = 0; f < $scope.fields.length; f++) {
        if ($scope.fields[f].DatabaseColumn === 'totalAlphabetResponse') {
          $scope.totalFields.push($scope.fields[f]);
        } else if ($scope.fields[f].DatabaseColumn === 'totalSoundResponse') {
          $scope.totalFields.push($scope.fields[f]);
        } else if ($scope.fields[f].DatabaseColumn === 'totalWordResponse') {
          $scope.totalFields.push($scope.fields[f]);
        } else if ($scope.fields[f].DatabaseColumn === 'totalOverallResponse') {
          $scope.totalFields.push($scope.fields[f]);
        } else if ($scope.fields[f].DatabaseColumn === 'unknownLetters') {
          $scope.commentFields.push($scope.fields[f]);
        } else if ($scope.fields[f].DatabaseColumn === 'comments') {
          $scope.commentFields.push($scope.fields[f]);
        }
      }
      for (var i = 0; i < $scope.fields.length; i++) {
        var field = $scope.fields[i];
        var currentFieldGroupId = field.GroupId;
        var currentPage = field.Page;
        // check if this groupid is already added, if not add it
        var p1GroupIndex = 0;
        var p2GroupIndex = 0;
        if (currentPage == 1 && !isNaN(currentFieldGroupId)) {
          var boolFoundOnP1 = false;
          for (var j = 0; j < $scope.page1FieldGroups.length; j++) {
            if ($scope.page1FieldGroups[j].Id === currentFieldGroupId) {
              boolFoundOnP1 = true;
              p1GroupIndex = j;
              break;
            }
          }
          // if we didn't find it, add this group
          if (!boolFoundOnP1) {
            p1GroupIndex = $scope.page1FieldGroups.push({ Id: currentFieldGroupId }) - 1;
            $scope.page1FieldGroups[p1GroupIndex].Fields = [];
          }
          // add the field to the fields array
          $scope.page1FieldGroups[p1GroupIndex].Fields.push(angular.copy(field));
        } else if (currentPage == 2 && !isNaN(currentFieldGroupId)) {
          var boolFoundOnP2 = false;
          for (var j = 0; j < $scope.page2FieldGroups.length; j++) {
            if ($scope.page2FieldGroups[j].Id === currentFieldGroupId) {
              boolFoundOnP2 = true;
              p2GroupIndex = j;
              break;
            }
          }
          // if we didn't find it, add this group
          if (!boolFoundOnP2) {
            p2GroupIndex = $scope.page2FieldGroups.push({ Id: currentFieldGroupId }) - 1;
            $scope.page2FieldGroups[p2GroupIndex].Fields = [];
          }
          // add the field to the fields array
          $scope.page2FieldGroups[p2GroupIndex].Fields.push(angular.copy(field));
        }
      }
    };
    // end spelling controller specific
    // initial load
    $scope.LoadData();
  }
  function SectionStudentResultHRSIWDataEntryController($scope, $q, $http, nsPinesService, $location, $filter, $routeParams, NSStudentSectionResultDataEntry, nsFilterOptionsService, $controller, nsSelect2RemoteOptions) {
    $controller('NSStudentSectionDataEntryBaseController', { $scope: $scope });
    $scope.totalFields = [];
    $scope.commentFields = [];
    $scope.formFields = [];
    $scope.row1Fields = [];
    $scope.row2Fields = [];
    $scope.rowSize = 30;
    $scope.formId = null;
    $scope.formFieldResult = {};
    $scope.isNew = $routeParams.studentResultId == '-1' ? true : false;
    $scope.checkAll = { checked: false };
    $scope.formSettings = {};
    $scope.toggleAll = function () {
      if ($scope.checkAll.checked) {
        for (var i = 0; i < $scope.studentResult.FieldResults.length; i++) {
          if ($scope.studentResult.FieldResults[i].Field.Page == $scope.formId) {
            $scope.studentResult.FieldResults[i].BoolValue = true;
          }
        }
      } else {
        for (var i = 0; i < $scope.studentResult.FieldResults.length; i++) {
          if ($scope.studentResult.FieldResults[i].Field.Page == $scope.formId) {
            $scope.studentResult.FieldResults[i].BoolValue = false;
          }
        }
      }
    };
    var setFormId = function () {
      for (var i = 0; i < $scope.studentResult.FieldResults.length; i++) {
        if ($scope.studentResult.FieldResults[i].DbColumn == 'FormId') {
          $scope.formId = $scope.studentResult.FieldResults[i].IntValue;
          $scope.formFieldResult = $scope.studentResult.FieldResults[i];
        }
      }
    };
    $scope.headerColspan = function (index) {
      if (index == 0) {
        return '1';
      } else {
        return '2';
      }
    };
    $scope.checkDisabledForm = function () {
      var firstCheck = $scope.formFieldResult.IntValue != null && !$scope.isNew;
      // simple check
      if (firstCheck) {
        return firstCheck;
      } else {
        // otherwise loop through all checkbox fields to see if any are checked
        for (var i = 0; i < $scope.studentResult.FieldResults.length; i++) {
          if ($scope.studentResult.FieldResults[i].Field.FieldType == 'Checkbox') {
            if ($scope.studentResult.FieldResults[i].BoolValue == true) {
              return true;
            }
          }
        }
        return false;
      }
    };
    $scope.setDisplayStructure = function (reload) {
      setFormId();
      // create scope fields house specific totals
      // set total fields
      $scope.row1Fields = [];
      $scope.row2Fields = [];
      for (var f = 0; f < $scope.fields.length; f++) {
        if ($scope.fields[f].DatabaseColumn === 'totalScore') {
          if ($scope.totalFields.length == 0) {
            $scope.totalFields.push($scope.fields[f]);
          }
        } else if ($scope.fields[f].DatabaseColumn === 'comments') {
          if ($scope.commentFields.length == 0) {
            $scope.commentFields.push($scope.fields[f]);
          }
        } else if ($scope.fields[f].DatabaseColumn === 'FormId') {
          if ($scope.formFields.length == 0) {
            $scope.formFields.push($scope.fields[f]);
          }
        }
      }
      var fieldsAdded = 0;
      for (var i = 0; i < $scope.fields.length; i++) {
        var field = $scope.fields[i];
        if (fieldsAdded < $scope.rowSize && field.Page == $scope.formId) {
          $scope.row1Fields.push(angular.copy(field));
          fieldsAdded++;
        } else if (field.Page == $scope.formId) {
          $scope.row2Fields.push(angular.copy(field));
          fieldsAdded++;
        }
      }
    };
    // end spelling controller specific
    // initial load
    $scope.LoadData();
    $scope.$watch('formFieldResult.IntValue', function (newValue, oldValue) {
      if (!angular.equals(newValue, oldValue)) {
        $scope.setDisplayStructure(true);
      }
    });
  }
  function SectionStudentResultCAPDataEntryController($scope, $q, $http, nsPinesService, $location, $filter, $routeParams, NSStudentSectionResultDataEntry, nsFilterOptionsService, $controller, nsSelect2RemoteOptions) {
    $controller('NSStudentSectionDataEntryBaseController', { $scope: $scope });
    $scope.setDisplayStructure = function () {
      for (var i = 0; i < $scope.assessment.FieldGroups.length; i++) {
        $scope.assessment.FieldGroups[i].Fields = [];
        for (var k = 0; k < $scope.fields.length; k++) {
          if ($scope.fields[k].GroupId == $scope.assessment.FieldGroups[i].Id) {
            $scope.assessment.FieldGroups[i].Fields.push(angular.copy($scope.fields[k]));
          }
        }
      }
    };
    $scope.checkAll = { checked: false };
    $scope.toggleAll = function () {
      if ($scope.checkAll.checked) {
        for (var i = 0; i < $scope.studentResult.FieldResults.length; i++) {
          if ($scope.studentResult.FieldResults[i].Field.FieldType == 'Checkbox') {
            $scope.studentResult.FieldResults[i].BoolValue = true;
          }
        }
      } else {
        for (var i = 0; i < $scope.studentResult.FieldResults.length; i++) {
          if ($scope.studentResult.FieldResults[i].Field.FieldType == 'Checkbox') {
            $scope.studentResult.FieldResults[i].BoolValue = false;
          }
        }
      }
    };
    // initial load
    $scope.LoadData();
  }
  function SectionStudentResultSpellingDataEntryController($scope, $q, $http, nsPinesService, $location, $filter, $routeParams, NSStudentSectionResultDataEntry, nsFilterOptionsService, $controller, nsSelect2RemoteOptions) {
    $scope.wscToggle = {};
    $scope.wscToggle.toggleWordsSpelledCorrectly = false;
    $controller('NSStudentSectionDataEntryBaseController', { $scope: $scope });
    // spelling controller specific
    $scope.switchAllWordsSpelledCorrectly = function () {
      for (var j = 0; j < $scope.studentResult.FieldResults.length; j++) {
        if ($scope.studentResult.FieldResults[j].Field.FieldType === 'Checkbox') {
          $scope.studentResult.FieldResults[j].BoolValue = $scope.wscToggle.toggleWordsSpelledCorrectly;
        }
      }
    };
    $scope.footerCellBgColor = function (category) {
      var categoryFieldCount = 0;
      var categoryFieldCheckedCount = 0;
      if (category.DisplayName === 'Words Spelled Correctly' || category.DisplayName === 'Feature Points') {
        return '';
      }
      for (var j = 0; j < $scope.studentResult.FieldResults.length; j++) {
        if ($scope.studentResult.FieldResults[j].Field.CategoryId && $scope.studentResult.FieldResults[j].Field.SubcategoryId === null && $scope.studentResult.FieldResults[j].Field.CategoryId === category.Id) {
          categoryFieldCount++;
          if ($scope.studentResult.FieldResults[j].BoolValue === true) {
            categoryFieldCheckedCount++;
          }
        }
      }
      if (categoryFieldCount - categoryFieldCheckedCount <= 1) {
        return 'obsGreen';
      } else if (categoryFieldCount - categoryFieldCheckedCount == 2) {
        return 'obsYellow';
      } else {
        return 'obsRed';
      }
    };
    $scope.setDisplayStructure = function () {
      for (var i = 0; i < $scope.assessment.FieldGroups.length; i++) {
        $scope.assessment.FieldGroups[i].Categories = [];
        for (var j = 0; j < $scope.assessment.FieldCategories.length; j++) {
          $scope.assessment.FieldGroups[i].Categories.push(angular.copy($scope.assessment.FieldCategories[j]));
          $scope.assessment.FieldGroups[i].Categories[j].Fields = [];
          for (var k = 0; k < $scope.fields.length; k++) {
            if ($scope.fields[k].GroupId == $scope.assessment.FieldGroups[i].Id && $scope.fields[k].CategoryId == $scope.assessment.FieldCategories[j].Id) {
              $scope.assessment.FieldGroups[i].Categories[j].Fields.push(angular.copy($scope.fields[k]));
            }
          }
        }
      }
    };
    // end spelling controller specific
    // initial load
    $scope.LoadData();
  }
  function SectionDataEntryController($httpParamSerializer, $scope, $q, $http, nsPinesService, $location, $filter, $routeParams, nsSectionDataEntryService, nsFilterOptionsService, NSSectionAssessmentDataEntryManager, nsLookupFieldService, nsSelect2RemoteOptions, $bootbox, $uibModal, progressLoader, NSUserInfoService, FileSaver, webApiBaseUrl, $timeout, authService, nsGlobalSettings, ckEditorSettings, spinnerService) {
    $scope.sortArray = [];
    $scope.headerClassArray = [];
    $scope.staticColumnsObj = {};
    $scope.staticColumnsObj.studentNameHeaderClass = 'fa';
    $scope.filterOptions = nsFilterOptionsService.options;
    $scope.copySettings = {
      copyAll: false,
      sourceTddId: 0,
      targetTddId: 0,
      studentId: 0,
      sectionId: 0
    };
    $scope.uploadSettings = { LogItems: [] };
    $scope.ckeditorOptions = {
      language: 'en',
      allowedContent: true,
      entities: false,
      width: 250,
      height: 90,
      uploadUrl: ckEditorSettings.uploadurl + '?access_token=' + authService.token(),
      imageUploadUrl: ckEditorSettings.imageUploadUrl + '?access_token=' + authService.token(),
      filebrowserImageUploadUrl: ckEditorSettings.filebrowserImageUploadUrl + '?access_token=' + authService.token(),
      toolbarGroups: nsGlobalSettings.ckEditorDefaultConfig.toolbarGroups
    };
    $scope.settings = {
      printMode: false,
      printInProgress: false
    };
    if ($location.absUrl().indexOf('printmode=') >= 0) {
      $scope.settings.printMode = true;
    }
    $scope.downloadResult = function () {
      var text = '';
      for (var i = 0; i < $scope.uploadSettings.LogItems.length; i++) {
        text += $scope.uploadSettings.LogItems[i] + '\r\n';
      }
      var data = new Blob([text], { type: 'text/plain;charset=ANSI' });
      FileSaver.saveAs(data, 'results.txt');
      $scope.uploadSettings.uploadComplete = false;
    };
    $scope.getTemplate = function () {
      if ($scope.filterOptions.selectedSection == null || $scope.filterOptions.selectedBenchmarkDate == null) {
        $bootbox.alert('You must select a section and benchmark date in order to generate a template.');
        return;
      }
      var paramObj = {
          AssessmentId: $routeParams.assessmentid,
          SectionId: $scope.filterOptions.selectedSection.id,
          BenchmarkDateId: $scope.filterOptions.selectedBenchmarkDate.id
        };
      var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/GetBenchmarkExporTemplateWithData', paramObj);
      promise.then(function (response) {
        var data = new Blob([response.data.Result], { type: 'text/plain;charset=ANSI' });
        FileSaver.saveAs(data, 'export.csv');
      });
    };
    //var b = $httpParamSerializer($scope.filterOptions);
    $scope.SectionResults = new NSSectionAssessmentDataEntryManager(nsLookupFieldService.LookupFieldsArray);
    $scope.getTeacherRemoteOptions = nsSelect2RemoteOptions.TeacherRemoteOptions;
    $scope.validateRecorder = function (studentResult) {
      if (angular.isDefined(studentResult) && studentResult.Recorder.id > 0) {
        return true;
      }
      return false;
    };
    $scope.popupHeader = function () {
      if ($scope.copySettings.copyAll) {
        return 'All Student ';
      } else {
        return $scope.copySettings.targetResult.StudentName + '\'s ';
      }
    };
    $scope.openImportDialog = function () {
      var modalInstance = $uibModal.open({
          templateUrl: 'importBenchmarkData.html',
          scope: $scope,
          controller: function ($scope, $uibModalInstance) {
            $scope.theFiles = [];
            $scope.upload = function (theFiles) {
              var formData = new FormData();
              formData.append('AssessmentId', $routeParams.assessmentid);
              formData.append('BenchmarkDateId', $scope.filterOptions.selectedBenchmarkDate.id);
              angular.forEach(theFiles, function (file) {
                formData.append(file.name, file);
              });
              var paramObj = {};
              // start loader
              progressLoader.start();
              progressLoader.set(50);
              var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/UploadBenchmarkDataCSV', formData, {
                  transformRequest: angular.identity,
                  headers: { 'Content-Type': undefined }
                }).then(function (response) {
                  // end loader
                  progressLoader.end();
                  $scope.errors = [];
                  $scope.uploadSettings.LogItems = response.data.LogItems;
                  // show success
                  $('#formReset').click();
                  $scope.uploadSettings.hasFiles = false;
                  //$scope.theFiles.length = 0;
                  //$scope.settings.hasFiles = false;
                  $scope.uploadSettings.uploadComplete = true;
                  LoadData();
                  nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
                }, function (err) {
                  progressLoader.end();
                  $('#formReset').click();
                  $scope.uploadSettings.hasFiles = false;
                });
              $uibModalInstance.dismiss('cancel');
            };
            $scope.cancel = function () {
              $uibModalInstance.dismiss('cancel');
            };
          },
          size: 'md'
        });
    };
    $scope.openCopyDialog = function (studentResult, copyAll) {
      // set settings
      $scope.copySettings.copyAll = copyAll;
      $scope.copySettings.targetResult = studentResult;
      var modalInstance = $uibModal.open({
          templateUrl: 'copyFieldTddChooser.html',
          scope: $scope,
          controller: function ($scope, $uibModalInstance) {
            $scope.copyData = function () {
              // validate that a date is selected and it isn't the current date
              if (!$scope.copySettings.targetCopyDate || $scope.copySettings.targetCopyDate.id == $scope.filterOptions.selectedBenchmarkDate.id) {
                alert('Please select a valid target benchmark date.');
                return;
              }
              $uibModalInstance.dismiss('cancel');
              progressLoader.start();
              progressLoader.set(50);
              if (copyAll) {
                $scope.SectionResults.copySectionAssessmentData($routeParams.assessmentid, $scope.filterOptions.selectedBenchmarkDate, $scope.copySettings.targetCopyDate, $scope.filterOptions.selectedSection).then(function (response) {
                  nsPinesService.dataSavedSuccessfully();
                  progressLoader.end();
                });
              } else {
                $scope.SectionResults.copyStudentAssessmentData($routeParams.assessmentid, $scope.filterOptions.selectedBenchmarkDate, $scope.copySettings.targetCopyDate, $scope.filterOptions.selectedSection, studentResult.StudentId).then(function (response) {
                  nsPinesService.dataSavedSuccessfully();
                  progressLoader.end();
                });
              }
            };
            $scope.cancel = function () {
              $uibModalInstance.dismiss('cancel');
            };
          },
          size: 'md'
        });
    };
    $scope.before = function (rowform) {
      rowform.$setSubmitted();
      if (rowform.$valid) {
        return;
      } else
        return 'At least one required field is not filled out.';
    };
    $scope.formats = [
      'dd-MMM-yyyy',
      'yyyy/MM/dd',
      'dd.MM.yyyy',
      'shortDate'
    ];
    $scope.format = $scope.formats[0];
    // move this to a simple directive
    $scope.$on('NSHTTPError', function (event, data) {
      $scope.errors.push({
        type: 'danger',
        msg: data
      });
      $('html, body').animate({ scrollTop: 0 }, 'fast');
    });
    $scope.popup1 = { opened: false };
    $scope.open1 = function () {
      $scope.popup1.opened = true;
    };
    $scope.print = function () {
      if ($scope.settings.printInProgress) {
        $bootbox.alert('Please wait... another print job is already in progress.');
        return;
      }
      $scope.settings.printInProgress = true;
      var notice = nsPinesService.startDynamic();
      var returnObj = {
          PrintLandscape: true,
          PrintMultiPage: false,
          Url: $location.absUrl()
        };
      var printMethod = 'PrintPage';
      if (webApiBaseUrl.indexOf('localhost') > 0) {
        printMethod = 'PrintPageLocal';
      }
      $http.post(webApiBaseUrl + '/api/Print/' + printMethod, returnObj, {
        responseType: 'arraybuffer',
        headers: { accept: 'application/pdf' }
      }).then(function (data) {
        var blob = new Blob([data.data], { type: 'application/pdf' });
        FileSaver.saveAs(blob, 'NorthStarPrint.pdf');
      }).finally(function () {
        $scope.settings.printInProgress = false;
        nsPinesService.endDynamic(notice);
      });
    };
    $scope.$watchGroup([
      'filterOptions.selectedSection.id',
      'filterOptions.selectedBenchmarkDate.id'
    ], function (newValue, oldValue, scope) {
      if (angular.isDefined(newValue[0]) && angular.isDefined(newValue[1])) {
        if (newValue[0] != oldValue[0] || newValue[1] != oldValue[1]) {
          LoadData();
        }
      }
    });
    //$scope.$watch('filterOptions.selectedBenchmarkDate.id', function (newValue, oldValue) {
    //    if (!angular.equals(newValue, oldValue)) {
    //        //var b = $httpParamSerializer($scope.filterOptions);
    //        LoadData();
    //    }
    //});
    $scope.sort = function (column) {
      $scope.SectionResults.doSort(column, $scope.staticColumnsObj, $scope.fields, $scope.headerClassArray, $scope.sortArray);
    };
    //$scope.navigateToTdd = function (tddid) {
    //    if (tddid != $routeParams.benchmarkDateId) {
    //        $location.path('/section-assessment-resultlist/' + $routeParams.assessmentid + '/' + tddid);
    //    }
    //}
    $scope.navigateToTdd = function (tddid) {
      $scope.filterOptions.selectedBenchmarkDate = nsFilterOptionsService.getBenchmarkDateById(tddid);
      nsFilterOptionsService.changeBenchmarkDate();
    };
    $scope.deleteAssessmentData = function (studentResult) {
      $bootbox.confirm('Are you sure you want to delete this record?', function (response) {
        if (response) {
          $scope.SectionResults.deleteStudentTestResult($scope.assessment.Id, studentResult).then(function (response) {
            angular.extend(studentResult, response.data.StudentResult);
            $scope.SectionResults.attachFieldsToResults($scope.studentResults, $scope.fields, $scope.lookupFieldsArray);
            $scope.SectionResults.makeDatesPopupCompatible($scope.studentResults);
            //LoadData();
            nsPinesService.dataDeletedSuccessfully();
          });
        }
      });
    };
    $scope.saveAssessmentData = function (studentResult) {
      $scope.SectionResults.saveAssessmentResult($scope.assessment.Id, studentResult, $scope.filterOptions.selectedBenchmarkDate.id).then(function (response) {
        angular.extend(studentResult, response.data.StudentResult);
        $scope.SectionResults.attachFieldsToResults($scope.studentResults, $scope.fields, $scope.lookupFieldsArray);
        $scope.SectionResults.makeDatesPopupCompatible($scope.studentResults);
        //LoadData();
        nsPinesService.dataSavedSuccessfully();
      });
    };
    $scope.defaultEditAction = function (studentResult, rowform) {
      var dataEntryPage = $scope.assessment.DefaultDataEntryPage;
      // default editing
      if (dataEntryPage === null || dataEntryPage === '') {
        // set default recorder
        if (studentResult.Recorder == null || studentResult.Recorder.id == -1) {
          // add current user
          studentResult.Recorder = {
            id: NSUserInfoService.currentUser.Id,
            text: NSUserInfoService.currentUser.FullName
          };
        }
        rowform.$show();
      } else {
        $location.path(dataEntryPage + '/' + $routeParams.assessmentid + '/' + $scope.filterOptions.selectedSection.id + '/' + $scope.filterOptions.selectedBenchmarkDate.id + '/' + studentResult.StudentId + '/' + studentResult.ResultId);
      }
    };
    var LoadData = function () {
      console.time('Start load assessmentdata');
      console.time('Start load data');
      // if trying to load data, but no routeparam defined, add benchmark date to URL first
      //if (typeof $routeParams.benchmarkDateId === 'undefined' && $scope.filterOptions.selectedBenchmarkDate !== null) {
      //    $scope.navigateToTdd($scope.filterOptions.selectedBenchmarkDate.id);
      //}
      //$scope.filterOptions.selectedBenchmarkDate = (typeof $routeParams.benchmarkDateId !== 'undefined') ? nsFilterOptionsService.getBenchmarkDateById($routeParams.benchmarkDateId) : $scope.filterOptions.selectedBenchmarkDate;
      if ($scope.filterOptions.selectedBenchmarkDate != null && $scope.filterOptions.selectedSection != null) {
        $timeout(function () {
          spinnerService.show('tableSpinner');
        });
        $scope.SectionResults.loadAssessmentResultData($routeParams.assessmentid, nsFilterOptionsService.options).then(function (data) {
          console.timeEnd('Start load data');
          //$scope.lookupFieldsArray = data.data.Assessment.LookupFields;
          $scope.fields = data.data.Assessment.Fields;
          $scope.assessment = data.data.Assessment;
          $scope.studentResults = data.data.StudentResults;
          $scope.SectionResults.attachFieldsToResults($scope.studentResults, $scope.fields, $scope.lookupFieldsArray);
          console.time('Start dates compatible');
          $scope.SectionResults.makeDatesPopupCompatible($scope.studentResults);
          console.timeEnd('Start dates compatible');
          console.timeEnd('Start load assessmentdata');
        }).finally(function () {
          $timeout(function () {
            spinnerService.hide('tableSpinner');
          });
        });
      }
    };
    // initial load
    LoadData();
  }
  function SectionStudentResultHFWDataEntryController($scope, $q, $http, pinesNotifications, $location, $filter, $routeParams, $bootbox, webApiBaseUrl, $timeout, spinnerService, FileSaver, nsFilterOptionsService) {
    $scope.settings = {
      defaultDate: new Date(),
      selectedWordRange: '1-100',
      selectedWordOrder: 'Alphabetic',
      selectedSortOrder: 'SortOrder'
    };
    // get lookup field values
    //$http.get(webApiBaseUrl + '/api/assessment/GetLookupFieldsForAssessment/' + $routeParams.assessmentId).success(function (lookupData) {
    //$scope.filterOptions = nsFilterOptionsService.options;
    // load selected word range and sort order
    //$scope.filterOptions.selectedWordOrder || 'Alphabetic';
    //$scope.lookupFieldsArray = lookupData;
    $scope.sortArray = [];
    $scope.headerClassArray = [];
    $scope.firstNameHeaderClass = 'fa';
    $scope.lastNameHeaderClass = 'fa';
    $scope.wordRangeOptions = [
      'Kindergarten',
      '1-100',
      '101-200',
      '201-300',
      '301-400',
      '401-500',
      '501-600',
      '601-700',
      '701-800',
      '801-900',
      '901-1000'
    ];
    $scope.wordOrderOptions = [
      'Alphabetic',
      'Teaching'
    ];
    $scope.totalFields = [];
    $scope.commentFields = [];
    $scope.LowerFieldGroups = [];
    $scope.UpperFieldGroups = [];
    $scope.start = 0;
    $scope.end = 0;
    $scope.isKdg = false;
    $scope.errors = [];
    $scope.formats = [
      'dd-MMM-yyyy',
      'yyyy/MM/dd',
      'dd.MM.yyyy',
      'shortDate'
    ];
    $scope.format = $scope.formats[0];
    // move this to a simple directive
    $scope.$on('NSHTTPError', function (event, data) {
      $scope.errors.push({
        type: 'danger',
        msg: data
      });
      $('html, body').animate({ scrollTop: 0 }, 'fast');
    });
    $scope.popup1 = { opened: false };
    $scope.open1 = function () {
      $scope.popup1.opened = true;
    };
    // initial load only
    //$scope.$watch('filterOptions.selectedHfwSortOrder', function (newValue, oldValue) {
    //    if (!angular.equals(newValue, oldValue) && oldValue == null) {
    //        $scope.settings.selectedWordOrder = $scope.filterOptions.selectedHfwSortOrder;
    //    }
    //});
    $scope.changeSortOrder = function (order) {
      // save the setting back to DB
      //$scope.filterOptions.selectedWordOrder = order;
      var paramObj = {
          SettingName: 'hfwsortorder',
          SettingValue: order
        };
      $http.post(webApiBaseUrl + '/api/filteroptions/UpdateHfwSetting', paramObj);
      //$scope.filterOptions.selectedHfwSortOrder = $scope.settings.selectedWordOrder;
      //nsFilterOptionsService.changeHfwSortOrder();
      $scope.LowerFieldGroups = [];
      $scope.UpperFieldGroups = [];
      for (var n = 0; n < $scope.assessment.FieldGroups.length; n++) {
        var currentFieldGroup = $scope.assessment.FieldGroups[n];
        currentFieldGroup.Fields = [];
        // get all the fields for this group
        for (var g = 0; g < $scope.fields.length; g++) {
          var currentField = $scope.fields[g];
          if (currentFieldGroup.Id == currentField.GroupId) {
            currentFieldGroup.Fields.push(currentField);
          }
        }
        if ($scope.settings.selectedWordOrder == 'Alphabetic') {
          if (currentFieldGroup.SortOrder <= $scope.start + 49 || $scope.isKdg) {
            $scope.LowerFieldGroups.push(currentFieldGroup);
          } else {
            $scope.UpperFieldGroups.push(currentFieldGroup);
          }
        } else {
          if (currentFieldGroup.AltOrder <= $scope.start + 49 || $scope.isKdg) {
            $scope.LowerFieldGroups.push(currentFieldGroup);
          } else {
            $scope.UpperFieldGroups.push(currentFieldGroup);
          }
        }
      }
      if ($scope.settings.selectedWordOrder == 'Alphabetic') {
        $scope.settings.selectedSortOrder = 'SortOrder';
      } else {
        $scope.settings.selectedSortOrder = 'AltOrder';
      }
    };
    $scope.changeWordRange = function (range) {
      var paramObj = {
          SettingName: 'hfwsinglerange',
          SettingValue: range
        };
      $http.post(webApiBaseUrl + '/api/filteroptions/UpdateHfwSetting', paramObj);
      var anyModified = false;
      // check if any modified fields before switching
      for (var r = 0; r < $scope.studentResult.ReadFieldResults.length; r++) {
        if ($scope.studentResult.ReadFieldResults[r].IsModified) {
          anyModified = true;
          break;
        }
      }
      if (!anyModified) {
        for (var r = 0; r < $scope.studentResult.WriteFieldResults.length; r++) {
          if ($scope.studentResult.WriteFieldResults[r].IsModified) {
            anyModified = true;
            break;
          }
        }
      }
      if (anyModified) {
        $bootbox.confirm('You have made changes to this Word Range.  Do you want to save them before continuing?', function (response) {
          if (response) {
            $bootbox.hideAll();
            $scope.saveAssessmentData($scope.studentResult, true).then(function (response) {
              loadDataCallBack($scope.settings.selectedWordRange);
            });
          } else {
            loadDataCallBack($scope.settings.selectedWordRange);
          }
        });
      } else {
        loadDataCallBack($scope.settings.selectedWordRange);
      }
    };
    var loadDataCallBack = function (range) {
      switch (range) {
      case 'Kindergarten':
        LoadData(1, 26, true);
        break;
      case '1-100':
        LoadData(1, 100, false);
        break;
      case '101-200':
        LoadData(101, 200, false);
        break;
      case '201-300':
        LoadData(201, 300, false);
        break;
      case '301-400':
        LoadData(301, 400, false);
        break;
      case '401-500':
        LoadData(401, 500, false);
        break;
      case '501-600':
        LoadData(501, 600, false);
        break;
      case '601-700':
        LoadData(601, 700, false);
        break;
      case '701-800':
        LoadData(701, 800, false);
        break;
      case '801-900':
        LoadData(801, 900, false);
        break;
      case '901-1000':
        LoadData(901, 1000, false);
        break;
      }
    };
    $scope.cancel = function () {
      $location.path('section-assessment-resultlist/' + $routeParams.assessmentId);
    };
    $scope.saveAssessmentData = function (studentResult, inPlaceSave) {
      var assessmentId = $scope.assessment.Id;
      //var studentResult = {};
      var returnObject = {
          StudentResult: studentResult,
          AssessmentId: assessmentId
        };
      return $http.post(webApiBaseUrl + '/api/DataEntry/SaveHFWAssessmentResult', returnObject).then(function (data) {
        $scope.dataSavedSuccessfully();
        if (inPlaceSave) {
          return;
        }
        $location.path('section-assessment-resultlist/' + $routeParams.assessmentId);  // TODO: update diplayvalue of any dropdownfromdb fields
      });
    };
    $scope.dataSavedSuccessfully = function () {
      pinesNotifications.notify({
        title: 'Data Saved',
        text: 'Your data was saved successfully.',
        type: 'success'
      });
    };
    $scope.dataDeletedSuccessfully = function () {
      pinesNotifications.notify({
        title: 'Data Deleted',
        text: 'Your data was deleted successfully.',
        type: 'success'
      });
    };
    $scope.toggleCategory = function (category, page, isChecked) {
      var resultSet = category.DisplayName == 'Read' ? $scope.studentResult.ReadFieldResults : $scope.studentResult.WriteFieldResults;
      if (!isChecked) {
        //confirm and return if false
        $bootbox.confirm('Un-checking this will clear all the ' + category.DisplayName + ' results.  This operation cannot be undone.  Do you want to proceed?', function (result) {
          if (result) {
            for (var i = 0; i < resultSet.length; i++) {
              var currentFieldGroup = null;
              // find the fieldGroup that matches the groupid of this result field and check its sortorder
              for (var w = 0; w < $scope.assessment.FieldGroups.length; w++) {
                if ($scope.assessment.FieldGroups[w].Id === resultSet[i].Field.GroupId) {
                  currentFieldGroup = $scope.assessment.FieldGroups[w];
                }
              }
              if (page == 1) {
                if ($scope.settings.selectedWordOrder == 'Alphabetic' && currentFieldGroup.SortOrder <= $scope.start + 49) {
                  resultSet[i].BoolValue = false;
                  resultSet[i].DateValue = null;
                  resultSet[i].IsModified = true;
                } else if ($scope.settings.selectedWordOrder == 'Teaching' && currentFieldGroup.AltOrder <= $scope.start + 49) {
                  resultSet[i].BoolValue = false;
                  resultSet[i].DateValue = null;
                  resultSet[i].IsModified = true;
                }
              } else if (page == 2) {
                if ($scope.settings.selectedWordOrder == 'Alphabetic' && currentFieldGroup.SortOrder > $scope.start + 49) {
                  resultSet[i].BoolValue = false;
                  resultSet[i].DateValue = null;
                  resultSet[i].IsModified = true;
                } else if ($scope.settings.selectedWordOrder == 'Teaching' && currentFieldGroup.AltOrder > $scope.start + 49) {
                  resultSet[i].BoolValue = false;
                  resultSet[i].DateValue = null;
                  resultSet[i].IsModified = true;
                }
              }
            }
          } else {
            // reset checked status
            if (page == 1) {
              category.checked1 = true;
            } else {
              category.checked2 = true;
            }
          }
        });
      } else {
        for (var i = 0; i < resultSet.length; i++) {
          var currentFieldGroup = null;
          // find the fieldGroup that matches the groupid of this result field and check its sortorder
          for (var w = 0; w < $scope.assessment.FieldGroups.length; w++) {
            if ($scope.assessment.FieldGroups[w].Id === resultSet[i].Field.GroupId) {
              currentFieldGroup = $scope.assessment.FieldGroups[w];
              break;
            }
          }
          if (page == 1) {
            if ($scope.settings.selectedWordOrder == 'Alphabetic' && currentFieldGroup.SortOrder <= $scope.start + 49) {
              resultSet[i].BoolValue = true;
              if (resultSet[i].DateValue == null) {
                resultSet[i].DateValue = $scope.settings.defaultDate;
                resultSet[i].IsModified = true;
              }
            } else if ($scope.settings.selectedWordOrder == 'Teaching' && currentFieldGroup.AltOrder <= $scope.start + 49) {
              resultSet[i].BoolValue = true;
              if (resultSet[i].DateValue == null) {
                resultSet[i].DateValue = $scope.settings.defaultDate;
                resultSet[i].IsModified = true;
              }
            }
          } else if (page == 2) {
            if ($scope.settings.selectedWordOrder == 'Alphabetic' && currentFieldGroup.SortOrder > $scope.start + 49) {
              resultSet[i].BoolValue = true;
              if (resultSet[i].DateValue == null) {
                resultSet[i].DateValue = $scope.settings.defaultDate;
                resultSet[i].IsModified = true;
              }
            } else if ($scope.settings.selectedWordOrder == 'Teaching' && currentFieldGroup.AltOrder > $scope.start + 49) {
              resultSet[i].BoolValue = true;
              if (resultSet[i].DateValue == null) {
                resultSet[i].DateValue = $scope.settings.defaultDate;
                resultSet[i].IsModified = true;
              }
            }
          }
        }
      }
    };
    // get initial settings and load accordingly
    $http.get(webApiBaseUrl + '/api/filteroptions/LoadHfwSettings').then(function (response) {
      if (response.data.WordOrder != null) {
        $scope.settings.selectedWordOrder = response.data.WordOrder;
      }
      if (response.data.WordRange != null) {
        $scope.settings.selectedWordRange = response.data.WordRange;
      }
      loadDataCallBack($scope.settings.selectedWordRange);
    });
    // get selected wordorder and range
    function LoadData(start, end, isKdg) {
      $timeout(function () {
        spinnerService.show('tableSpinner');
      });
      $scope.start = start;
      $scope.end = end;
      $scope.isKdg = isKdg;
      $scope.totalFields = [];
      $scope.commentFields = [];
      $scope.LowerFieldGroups = [];
      $scope.UpperFieldGroups = [];
      var paramObj = {
          AssessmentId: $routeParams.assessmentId,
          SectionId: $routeParams.classId,
          BenchmarkDateId: $routeParams.benchmarkDateId,
          StudentResultId: $routeParams.studentResultId,
          StudentId: $routeParams.studentId,
          LowWordOrder: start,
          HighWordOrder: end,
          WordOrder: $scope.settings.selectedWordOrder,
          IsKdg: isKdg
        };
      $http.post(webApiBaseUrl + '/api/dataentry/GetHFWSingleAssessmentResult', paramObj).then(function (response) {
        $scope.assessment = response.data.Assessment;
        $scope.fields = response.data.Assessment.Fields;
        $scope.categories = response.data.Assessment.FieldCategories;
        $scope.studentResult = response.data.StudentResult;
        // assign fields to proper categories
        for (var k = 0; k < $scope.fields.length; k++) {
          for (var i = 0; i < $scope.categories.length; i++) {
            if ($scope.fields[k].CategoryId === $scope.categories[i].Id) {
              $scope.fields[k].Category = $scope.categories[i];
            }
          }
        }
        for (var r = 0; r < $scope.fields.length; r++) {
          $scope.headerClassArray[r] = 'fa';
        }
        // Add FieldOrder, FieldType
        for (var k = 0; k < $scope.studentResult.ReadFieldResults.length; k++) {
          for (var i = 0; i < $scope.fields.length; i++) {
            if ($scope.fields[i].DatabaseColumn == $scope.studentResult.ReadFieldResults[k].DbColumn) {
              $scope.studentResult.ReadFieldResults[k].Field = $scope.fields[i];
            }
          }
        }
        for (var k = 0; k < $scope.studentResult.WriteFieldResults.length; k++) {
          for (var i = 0; i < $scope.fields.length; i++) {
            if ($scope.fields[i].DatabaseColumn == $scope.studentResult.WriteFieldResults[k].DbColumn) {
              $scope.studentResult.WriteFieldResults[k].Field = $scope.fields[i];
            }
          }
        }
        // do total fields as well?
        // split the fieldgroups into two
        for (var n = 0; n < $scope.assessment.FieldGroups.length; n++) {
          var currentFieldGroup = $scope.assessment.FieldGroups[n];
          currentFieldGroup.Fields = [];
          // get all the fields for this group
          for (var g = 0; g < $scope.fields.length; g++) {
            var currentField = $scope.fields[g];
            if (currentFieldGroup.Id == currentField.GroupId) {
              currentFieldGroup.Fields.push(currentField);
            }
          }
          if ($scope.settings.selectedWordOrder == 'Alphabetic') {
            if (currentFieldGroup.SortOrder <= $scope.start + 49 || $scope.isKdg) {
              $scope.LowerFieldGroups.push(currentFieldGroup);
            } else {
              $scope.UpperFieldGroups.push(currentFieldGroup);
            }
          } else {
            if (currentFieldGroup.AltOrder <= $scope.start + 49 || $scope.isKdg) {
              $scope.LowerFieldGroups.push(currentFieldGroup);
            } else {
              $scope.UpperFieldGroups.push(currentFieldGroup);
            }
          }
        }
        // now that the fieldgroups are set up, add the fields for each group
        for (var f = 0; f < $scope.fields.length; f++) {
          if ($scope.fields[f].DatabaseColumn === 'readScore') {
            $scope.totalFields.push($scope.fields[f]);
          } else if ($scope.fields[f].DatabaseColumn === 'writeScore') {
            $scope.totalFields.push($scope.fields[f]);
          } else if ($scope.fields[f].DatabaseColumn === 'totalScore') {
            $scope.totalFields.push($scope.fields[f]);
          } else if ($scope.fields[f].DatabaseColumn === 'comments') {
            $scope.commentFields.push($scope.fields[f]);
          }
        }
      }).finally(function () {
        spinnerService.hide('tableSpinner');
      });
    }  //});
  }
}());
(function () {
  'use strict';
  angular.module('interventionGroupDataEntryModule', []).controller('IGDataEntryController', IGDataEntryController).factory('NSInterventionGroupAssessmentDataEntryManager', [
    '$http',
    'webApiBaseUrl',
    'nsLookupFieldService',
    function ($http, webApiBaseUrl, nsLookupFieldService) {
      var self = this;
      var NSInterventionGroupAssessmentDataEntryManager = function (lookupFieldsArray) {
        self.LookupFieldsArray = lookupFieldsArray;
        this.initialize = function () {
        };
        this.loadAssessmentResultData = function (assessmentId, nsFilterOptionsService) {
          var postObject = {
              AssessmentId: assessmentId,
              InterventionGroupId: nsFilterOptionsService.selectedInterventionGroup.id,
              StudentId: nsFilterOptionsService.selectedInterventionStudent.id
            };
          var url = webApiBaseUrl + '/api/interventiongroupdataentry/GetAssessmentResults';
          return $http.post(url, postObject);
        };
        this.makeDatesPopupCompatible = function (studentResultsArray) {
          for (var j = 0; j < studentResultsArray.length; j++) {
            var result = studentResultsArray[j];
            if (result.TestDate == null) {
              result.TestDate = new Date();
            } else {
              var momentizedDate = moment(result.TestDate);
              result.TestDate = momentizedDate.toDate();
            }
          }
        };
        this.attachFieldsToResults = function (studentResultsArray, fieldsArray) {
          console.time('Start attach fields');
          for (var j = 0; j < studentResultsArray.length; j++) {
            for (var k = 0; k < studentResultsArray[j].FieldResults.length; k++) {
              for (var r = 0; r < fieldsArray.length; r++) {
                if (fieldsArray[r].DatabaseColumn == studentResultsArray[j].FieldResults[k].DbColumn) {
                  studentResultsArray[j].FieldResults[k].Field = angular.copy(fieldsArray[r]);
                  // set display value
                  if (fieldsArray[r].FieldType === 'DropdownFromDB') {
                    for (var p = 0; p < self.LookupFieldsArray.length; p++) {
                      if (self.LookupFieldsArray[p].LookupColumnName === fieldsArray[r].LookupFieldName) {
                        // now find the specifc value that matches
                        for (var y = 0; y < self.LookupFieldsArray[p].LookupFields.length; y++) {
                          if (studentResultsArray[j].FieldResults[k].IntValue === self.LookupFieldsArray[p].LookupFields[y].FieldSpecificId) {
                            studentResultsArray[j].FieldResults[k].DisplayValue = self.LookupFieldsArray[p].LookupFields[y].FieldValue;
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
          }
          console.timeEnd('Start attach fields');  // set initial display values
        };
        this.initializeHeaderClassArray = function (fields, headerClassArray) {
          for (var r = 0; r < fields.length; r++) {
            headerClassArray[r] = 'fa';
          }
        };
        this.doSort = function (column, staticColumnsObj, fields, headerClassArray, sortArray) {
          var columnIndex = -1;
          // if this is not a first or lastname column
          if (!isNaN(parseInt(column))) {
            columnIndex = column;
            switch (fields[column].FieldType) {
            case 'DateCheckbox':
              column = 'FieldResults[' + column + '].DateValue';
              break;
            case 'Textfield':
              column = 'FieldResults[' + column + '].StringValue';
              break;
            case 'DecimalRange':
              column = 'FieldResults[' + column + '].DecimalValue';
              break;
            case 'DropdownRange':
              column = 'FieldResults[' + column + '].IntValue';
              break;
            case 'DropdownFromDB':
              column = 'FieldResults[' + column + '].IntValue';
              break;
            case 'CalculatedFieldDbOnly':
              column = 'FieldResults[' + column + '].StringValue';
              break;
            case 'CalculatedFieldDbBacked':
              column = 'FieldResults[' + column + '].IntValue';
              break;
            case 'CalculatedFieldDbBackedString':
              column = 'FieldResults[' + column + '].StringValue';
              break;
            case 'CalculatedFieldClientOnly':
              column = 'FieldResults[' + column + '].StringValue';
              //shouldnt even be used in sorting
              break;
            default:
              column = 'FieldResults[' + column + '].IntValue';
              break;
            }
          }
          var bFound = false;
          for (var j = 0; j < sortArray.length; j++) {
            // if it is already on the list, reverse the sort
            if (sortArray[j].indexOf(column) >= 0) {
              bFound = true;
              // is it already negative? if so, remove it
              if (sortArray[j].indexOf('-') === 0) {
                if (columnIndex > -1) {
                  headerClassArray[columnIndex] = 'fa';
                } else if (column === 'StudentName') {
                  staticColumnsObj.studentNameHeaderClass = 'fa';
                }
                sortArray.splice(j, 1);
              } else {
                if (columnIndex > -1) {
                  headerClassArray[columnIndex] = 'fa fa-chevron-down';
                } else if (column === 'StudentName') {
                  staticColumnsObj.studentNameHeaderClass = 'fa fa-chevron-down';
                }
                sortArray[j] = '-' + sortArray[j];
              }
              break;
            }
          }
          if (!bFound) {
            sortArray.push(column);
            if (columnIndex > -1) {
              headerClassArray[columnIndex] = 'fa fa-chevron-up';
            } else if (column === 'StudentName') {
              staticColumnsObj.studentNameHeaderClass = 'fa fa-chevron-up';
            }
          }
        };
        this.saveAssessmentResult = function (assessmentId, studentResult) {
          var returnObject = {
              StudentResult: studentResult,
              AssessmentId: assessmentId
            };
          return $http.post(webApiBaseUrl + '/api/interventiongroupdataentry/SaveAssessmentResult', returnObject);
        };
        this.deleteStudentTestResult = function (assessmentId, studentResult) {
          var returnObject = {
              StudentResult: studentResult,
              AssessmentId: assessmentId
            };
          return $http.post(webApiBaseUrl + '/api/interventiongroupdataentry/DeleteAssessmentResult', returnObject);
        };
        this.cleanupAfterDelete = function () {
          // TODO: update local model, delete all field values
          // loop over each field and delete the values... also delete anything else relevant
          for (var k = 0; k < studentResult.FieldResults.length; k++) {
            studentResult.FieldResults[k].IntValue = null;
            studentResult.FieldResults[k].DecimalValue = null;
            studentResult.FieldResults[k].DateValue = null;
            studentResult.FieldResults[k].StringValue = null;
            studentResult.FieldResults[k].DisplayValue = null;
            studentResult.FieldResults[k].BoolValue = null;
          }
        };
        this.initialize();
      };
      return NSInterventionGroupAssessmentDataEntryManager;
    }
  ]);
  IGDataEntryController.$inject = [
    '$httpParamSerializer',
    '$scope',
    '$q',
    '$http',
    'nsPinesService',
    '$location',
    '$filter',
    '$routeParams',
    'nsSectionDataEntryService',
    'nsFilterOptionsService',
    'NSInterventionGroupAssessmentDataEntryManager',
    'nsLookupFieldService',
    'nsSelect2RemoteOptions',
    '$bootbox',
    '$uibModal',
    'progressLoader',
    'spinnerService',
    'FileSaver',
    'webApiBaseUrl',
    '$timeout'
  ];
  function IGDataEntryController($httpParamSerializer, $scope, $q, $http, nsPinesService, $location, $filter, $routeParams, nsSectionDataEntryService, nsFilterOptionsService, NSInterventionGroupAssessmentDataEntryManager, nsLookupFieldService, nsSelect2RemoteOptions, $bootbox, $uibModal, progressLoader, spinnerService, FileSaver, webApiBaseUrl, $timeout) {
    $scope.sortArray = [];
    $scope.errors = [];
    $scope.headerClassArray = [];
    $scope.staticColumnsObj = {};
    $scope.staticColumnsObj.studentNameHeaderClass = 'fa';
    $scope.filterOptions = nsFilterOptionsService.options;
    $scope.uploadSettings = { LogItems: [] };
    $scope.GroupResults = new NSInterventionGroupAssessmentDataEntryManager(nsLookupFieldService.LookupFieldsArray);
    $scope.getTeacherRemoteOptions = nsSelect2RemoteOptions.TeacherRemoteOptions;
    $scope.validateRecorder = function (studentResult) {
      if (angular.isDefined(studentResult) && studentResult.Recorder.id > 0) {
        return true;
      }
      return false;
    };
    $scope.downloadResult = function () {
      var text = '';
      for (var i = 0; i < $scope.uploadSettings.LogItems.length; i++) {
        text += $scope.uploadSettings.LogItems[i] + '\r\n';
      }
      var data = new Blob([text], { type: 'text/plain;charset=ANSI' });
      FileSaver.saveAs(data, 'results.txt');
      $scope.uploadSettings.uploadComplete = false;
    };
    $scope.getTemplate = function () {
      if ($scope.filterOptions.selectedInterventionStudent == null) {
        $bootbox.alert('You must select a student in order to generate a template.');
        return;
      }
      var paramObj = {
          AssessmentId: $routeParams.assessmentid,
          InterventionGroupId: $scope.filterOptions.selectedInterventionGroup.id,
          StudentId: $scope.filterOptions.selectedInterventionStudent.id
        };
      var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/GetInterventionExporTemplateWithData', paramObj);
      promise.then(function (response) {
        var data = new Blob([response.data.Result], { type: 'text/plain;charset=ANSI' });
        FileSaver.saveAs(data, 'export.csv');
      });
    };
    //$scope.openStudentDashboardDialog = function (studentId, studentName) {
    //    var modalInstance = $uibModal.open({
    //        templateUrl: 'studentDashboardViewer.html',
    //        scope: $scope,
    //        controller: function ($scope, $uibModalInstance) {
    //            $scope.settings = { selectedStudent: { id: studentId, text: studentName } };
    //            $scope.cancel = function () {
    //                $uibModalInstance.dismiss('cancel');
    //            };
    //        },
    //        size: 'md',
    //    });
    //}
    $scope.openImportDialog = function () {
      var modalInstance = $uibModal.open({
          templateUrl: 'importBenchmarkData.html',
          scope: $scope,
          controller: function ($scope, $uibModalInstance) {
            $scope.theFiles = [];
            $scope.upload = function (theFiles) {
              var formData = new FormData();
              formData.append('AssessmentId', $routeParams.assessmentid);
              formData.append('InterventionGroupId', $scope.filterOptions.selectedInterventionGroup.id);
              formData.append('StudentId', $scope.filterOptions.selectedInterventionStudent.id);
              angular.forEach(theFiles, function (file) {
                formData.append(file.name, file);
              });
              var paramObj = {};
              // start loader
              progressLoader.start();
              progressLoader.set(50);
              var promise = $http.post(webApiBaseUrl + '/api/importstatetestdata/UploadInterventionDataCSV', formData, {
                  transformRequest: angular.identity,
                  headers: { 'Content-Type': undefined }
                }).then(function (response) {
                  // end loader
                  progressLoader.end();
                  $scope.errors = [];
                  $scope.uploadSettings.LogItems = response.data.LogItems;
                  // show success
                  $('#formReset').click();
                  $scope.uploadSettings.hasFiles = false;
                  //$scope.theFiles.length = 0;
                  //$scope.settings.hasFiles = false;
                  $scope.uploadSettings.uploadComplete = true;
                  LoadData();
                  nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
                }, function (err) {
                  progressLoader.end();
                  $('#formReset').click();
                  $scope.uploadSettings.hasFiles = false;
                });
              $uibModalInstance.dismiss('cancel');
            };
            $scope.cancel = function () {
              $uibModalInstance.dismiss('cancel');
            };
          },
          size: 'md'
        });
    };
    $scope.addNewRow = function () {
      for (var i = 0; i < $scope.studentResults.length; i++) {
        if ($scope.studentResults[i].ResultId == -1) {
          alert('Only one new record can be added a time.');
          return;
        }
      }
      var newResult = {};
      newResult.StudentId = $scope.filterOptions.selectedInterventionStudent.id;
      newResult.StudentName = $scope.filterOptions.selectedInterventionStudent.text;
      newResult.ResultId = -1;
      newResult.TestDate = new Date();
      newResult.Recorder = {
        id: $scope.filterOptions.selectedInterventionist.id,
        text: $scope.filterOptions.selectedInterventionist.text
      };
      // TODO: eventually switch this to use the CURRENTUSER service
      newResult.ClassId = $scope.filterOptions.selectedInterventionGroup.id;
      newResult.StaffId = $scope.filterOptions.selectedInterventionist.id;
      // don't think we even need this
      newResult.FieldResults = [];
      // now turn fields into fieldResults
      for (var i = 0; i < $scope.assessment.Fields.length; i++) {
        var field = $scope.assessment.Fields[i];
        if (field.DatabaseColumn == null || field.DatabaseColumn == '') {
          continue;
        } else {
          newResult.FieldResults.push({
            StringValue: null,
            IntValue: null,
            DecimalValue: null,
            BoolValue: null,
            DbColumn: field.DatabaseColumn,
            IsModified: true,
            FieldIndex: 0,
            Field: angular.copy(field)
          });
        }
      }
      // now need some way to put form in edit mode
      $scope.addNewResult = newResult;
      // $scope.addNewForm.$visible = true;
      $scope.studentResults.push($scope.addNewResult);
    };
    $scope.before = function (rowform) {
      rowform.$setSubmitted();
      if (rowform.$valid) {
        return;
      } else
        return 'At least one required field is not filled out.';
    };
    $scope.formats = [
      'dd-MMM-yyyy',
      'yyyy/MM/dd',
      'dd.MM.yyyy',
      'shortDate'
    ];
    $scope.format = $scope.formats[0];
    $scope.cancelEdit = function (studentResult) {
      if (studentResult.ResultId == -1) {
        // delete record
        for (var i = 0; i < $scope.studentResults.length; i++) {
          if ($scope.studentResults[i].ResultId == -1) {
            $scope.studentResults.splice(i, 1);
            break;
          }
        }
      } else {
        studentResult.rowform.$cancel();
      }
    };
    $scope.popup1 = { opened: false };
    $scope.open1 = function () {
      $scope.popup1.opened = true;
    };
    //studentResults
    $scope.$watch('filterOptions.selectedInterventionStudent.id', function (newValue, oldValue) {
      if (!angular.equals(newValue, oldValue)) {
        LoadData();
      }
    });
    //$scope.$watch('addNewResult', function (newValue, oldValue) {
    //    if (angular.isDefined($scope.addNewForm) && $scope.addNewForm != null) {
    //        $scope.addNewForm.$visible = true;
    //    }
    //});
    $scope.sort = function (column) {
      $scope.GroupResults.doSort(column, $scope.staticColumnsObj, $scope.fields, $scope.headerClassArray, $scope.sortArray);
    };
    $scope.deleteAssessmentData = function (studentResult) {
      $bootbox.confirm('Are you sure you want to delete this record?', function (response) {
        if (response) {
          $scope.GroupResults.deleteStudentTestResult($scope.assessment.Id, studentResult).then(function (data) {
            LoadData();
            nsPinesService.dataDeletedSuccessfully();
          });
        }
      });
    };
    $scope.saveAssessmentData = function (studentResult) {
      $scope.GroupResults.saveAssessmentResult($scope.assessment.Id, studentResult).then(function (data) {
        LoadData();
        nsPinesService.dataSavedSuccessfully();
      });
    };
    $scope.defaultEditAction = function (studentResult, rowform) {
      var dataEntryPage = $scope.assessment.DefaultDataEntryPage;
      // default editing
      if (dataEntryPage === null || dataEntryPage === '') {
        rowform.$show();
      }  //else
         //{
         //    $location.path(dataEntryPage + "/" + $routeParams.assessmentid + "/" + $scope.filterOptions.selectedSection.id + "/" + $scope.filterOptions.selectedBenchmarkDate.id + "/" + studentResult.StudentId + "/" + studentResult.ResultId);
         //}
    };
    var LoadData = function () {
      if ($scope.filterOptions.selectedInterventionStudent != null) {
        $timeout(function () {
          spinnerService.show('tableSpinner');
        });
        $scope.GroupResults.loadAssessmentResultData($routeParams.assessmentid, nsFilterOptionsService.options).then(function (data) {
          //$scope.lookupFieldsArray = data.data.Assessment.LookupFields;
          $scope.fields = data.data.Assessment.Fields;
          $scope.assessment = data.data.Assessment;
          $scope.studentResults = data.data.StudentResults;
          $scope.GroupResults.attachFieldsToResults($scope.studentResults, $scope.fields, $scope.lookupFieldsArray);
          $scope.GroupResults.makeDatesPopupCompatible($scope.studentResults);
        }).finally(function () {
          spinnerService.hide('tableSpinner');
        });
        ;
      }
    };
    // initial load
    LoadData();
    $scope.sort('-TestDate');
  }
}());
(function () {
  'use strict';
  angular.module('sectionModule', []).controller('SectionListController', SectionListController).controller('SectionEditController', SectionEditController).service('nsSectionService', [
    '$http',
    'webApiBaseUrl',
    function ($http, webApiBaseUrl) {
      this.getSectionList = function (schoolYear, schoolId, gradeId, teacherId) {
        var returnObject = {
            SchoolYear: schoolYear,
            SchoolId: schoolId,
            GradeId: gradeId,
            TeacherId: teacherId
          };
        return $http.post(webApiBaseUrl + '/api/section/GetSectionList', returnObject);
      };
      this.getSection = function (sectionId) {
        return $http.get(webApiBaseUrl + '/api/section/GetSection/' + sectionId);
      };
      this.loadGrades = function () {
        return $http.get(webApiBaseUrl + '/api/filteroptions/LoadAllGrades');
      };
      this.deleteSection = function (sectionId) {
        var returnObject = { Id: sectionId };
        return $http.post(webApiBaseUrl + '/api/section/deletesection', returnObject);
      };
      this.saveSection = function (section) {
        return $http.post(webApiBaseUrl + '/api/section/savesection', section);
      };
    }
  ]).factory('NSSection', [
    '$http',
    'webApiBaseUrl',
    function ($http, webApiBaseUrl) {
      var NSSection = function (sectionId) {
        this.initialize = function () {
          var url = webApiBaseUrl + '/api/section/GetSection/' + sectionId;
          var sectionData = $http.get(url);
          var self = this;
          self.Students = [];
          self.CoTeachers = [];
          return sectionData.then(function (response) {
            angular.extend(self, response.data);
            if (self.Students === null)
              self.Students = [];
            if (self.CoTeachers === null)
              self.CoTeachers = [];
          }, function (response) {
          });
        };
        this.addStudentToSection = function (selectedStudent) {
          var self = this;
          if (selectedStudent !== undefined && selectedStudent !== null) {
            self.Students.unshift({
              id: selectedStudent.id,
              text: selectedStudent.LastName + ', ' + selectedStudent.FirstName,
              isNew: true
            });
            return true;
          } else {
            alert('Please select a student first.');
            return false;
          }
        };
        this.removeStudent = function (id) {
          var self = this;
          for (var i = 0; i < self.Students.length; i++) {
            if (self.Students[i].id === id) {
              self.Students.splice(i, 1);
            }
          }
        };
        this.initialize();
      };
      return NSSection;
    }
  ]).factory('NSSectionManager', [
    '$http',
    '$bootbox',
    'nsFilterOptionsService',
    'webApiBaseUrl',
    function ($http, $bootbox, nsFilterOptionsService, webApiBaseUrl) {
      var NSSectionManager = function () {
        this.initialize = function () {
        };
        this.getSectionList = function (schoolYear, schoolId, gradeId, teacherId) {
          var returnObject = {
              SchoolYear: nsFilterOptionsService.normalizeParameter(schoolYear),
              SchoolId: nsFilterOptionsService.normalizeParameter(schoolId),
              GradeId: nsFilterOptionsService.normalizeParameter(gradeId),
              TeacherId: nsFilterOptionsService.normalizeParameter(teacherId)
            };
          return $http.post(webApiBaseUrl + '/api/section/GetSectionList', returnObject);
        };
        this.save = function (section, successCallback, failureCallback) {
          var saveResponse = $http.post(webApiBaseUrl + '/api/section/savesection', section);
          saveResponse.then(successCallback, failureCallback);
        };
        this.delete = function (id, successCallback, failureCallback) {
          $bootbox.confirm('Are you sure you want to delete this section?', function (result) {
            if (result === true) {
              var returnObject = { Id: id };
              var deleteResponse = $http.post(webApiBaseUrl + '/api/section/deletesection', returnObject);
              deleteResponse.then(successCallback, failureCallback);
            }
          });
        };
        this.initialize();
      };
      return NSSectionManager;
    }
  ]);
  /* Movies List Controller  */
  SectionListController.$inject = [
    '$scope',
    'nsSectionService',
    'nsFilterOptionsService',
    'nsPinesService',
    '$bootbox',
    'NSSectionManager',
    'spinnerService',
    '$timeout'
  ];
  function SectionListController($scope, nsSectionService, nsFilterOptionsService, nsPinesService, $bootbox, NSSectionManager, spinnerService, $timeout) {
    $scope.filterOptions = nsFilterOptionsService.options;
    $scope.sectionManager = new NSSectionManager();
    var LoadData = function () {
      $timeout(function () {
        spinnerService.show('tableSpinner');
      });
      $scope.sectionManager.getSectionList($scope.filterOptions.selectedSchoolYear, $scope.filterOptions.selectedSchool, $scope.filterOptions.selectedGrade, $scope.filterOptions.selectedTeacher).then(function (data) {
        $scope.sections = data.data.Sections;
      }).finally(function () {
        spinnerService.hide('tableSpinner');
      });
    };
    $scope.deleteSection = function (id) {
      // TODO: ensure all fields are valid
      $scope.sectionManager.delete(id, function () {
        nsPinesService.dataDeletedSuccessfully();
        LoadData();
      }, function (msg) {
        $scope.errors.push({
          msg: '<strong>An Error Has Occurred</strong> ' + msg.data,
          type: 'danger'
        });
        $('html, body').animate({ scrollTop: 0 }, 'fast');
      });
    };
    $scope.$watch('filterOptions', function () {
      LoadData();
    }, true);
  }
  /* Movies Edit Controller */
  SectionEditController.$inject = [
    '$scope',
    '$routeParams',
    '$location',
    'nsSectionService',
    'nsFilterOptionsService',
    'nsPinesService',
    'nsSelect2RemoteOptions',
    'NSSection',
    'NSSectionManager'
  ];
  function SectionEditController($scope, $routeParams, $location, nsSectionService, nsFilterOptionsService, nsPinesService, nsSelect2RemoteOptions, NSSection, NSSectionManager) {
    $scope.section = new NSSection($routeParams.id);
    // move this to a simple directive
    $scope.$on('NSHTTPError', function (event, data) {
      $scope.errors.push({
        type: 'danger',
        msg: data
      });
      $('html, body').animate({ scrollTop: 0 }, 'fast');
    });
    $scope.sectionManager = new NSSectionManager();
    $scope.section.Students = [];
    $scope.section.CoTeachers = [];
    $scope.gradeList = [];
    $scope.filterOptions = nsFilterOptionsService.options;
    $scope.errors = [];
    // if doesn't pass security check 
    // we don't care about these changes unless this is a new section
    $scope.$watch('filterOptions', function () {
      if ($routeParams.id === '-1') {
        $scope.section.SchoolId = nsFilterOptionsService.normalizeParameter($scope.filterOptions.selectedSchool);
        $scope.section.SchoolYear = nsFilterOptionsService.normalizeParameter($scope.filterOptions.selectedSchoolYear);
      }
    }, true);
    $scope.addStudentToSection = function () {
      if ($scope.section.addStudentToSection($scope.section.addStudent)) {
        $scope.section.addStudent = null;
      }
    };
    $scope.saveSection = function () {
      // TODO: ensure all fields are valid
      $scope.sectionManager.save($scope.section, function () {
        nsPinesService.dataSavedSuccessfully();
        $location.path('section-list');
      });
    };
    $scope.removeStudentFromSection = function (id) {
      $scope.section.removeStudent(id);
    };
    $scope.closeAlert = function (index) {
      $scope.errors.splice(index, 1);
    };
    // don't catch any exception.  if there's an issue loading grades, it should bubble up to the http interceptor
    var LoadGrades = function () {
      nsSectionService.loadGrades().then(function (data) {
        $scope.gradeList.push.apply($scope.gradeList, data.data);
      });
    };
    // initial load
    LoadGrades();
    $scope.getCoTeacherRemoteOptions = nsSelect2RemoteOptions.CoTeacherRemoteOptions;
    $scope.getTeacherRemoteOptions = nsSelect2RemoteOptions.TeacherRemoteOptions;
    $scope.addStudentToSectionRemoteOptions = nsSelect2RemoteOptions.StudentToSectionRemoteOptions;
    $scope.StudentQuickSearchRemoteOptions = nsSelect2RemoteOptions.StudentQuickSearchRemoteOptions;
    $scope.StudentDetailedQuickSearchRemoteOptions = nsSelect2RemoteOptions.StudentDetailedQuickSearchRemoteOptions;
  }
}());
(function () {
  'use strict';
  angular.module('studentModule', []).factory('NSStudentManager', [
    '$http',
    '$bootbox',
    'nsFilterOptionsService',
    'webApiBaseUrl',
    function ($http, $bootbox, nsFilterOptionsService, webApiBaseUrl) {
      var NSStudentManager = function () {
        this.initialize = function () {
        };
        this.remoteStudentIdValidationPath = webApiBaseUrl + '/api/student/IsStudentIDUnique';
        this.getStudentList = function (schoolYear, schoolId, gradeId, teacherId, sectionId, studentId) {
          var returnObject = {
              SchoolYear: nsFilterOptionsService.normalizeParameter(schoolYear),
              SchoolId: nsFilterOptionsService.normalizeParameter(schoolId),
              GradeId: nsFilterOptionsService.normalizeParameter(gradeId),
              TeacherId: nsFilterOptionsService.normalizeParameter(teacherId),
              SectionId: nsFilterOptionsService.normalizeParameter(sectionId),
              StudentId: nsFilterOptionsService.normalizeParameter(studentId)
            };
          return $http.post(webApiBaseUrl + '/api/student/GetStudentList', returnObject);
        };
        this.save = function (student, successCallback, failureCallback) {
          var saveResponse = $http.post(webApiBaseUrl + '/api/student/savestudent', student);
          return saveResponse;
        };
        this.moveStudent = function (student, targetSection) {
          var paramObj = {
              Student: student,
              Section: targetSection
            };
          return $http.post(webApiBaseUrl + '/api/student/movestudent', paramObj);
        };
        this.delete = function (id, successCallback, failureCallback) {
          $bootbox.confirm('Are you sure you want to delete this student?', function (result) {
            if (result === true) {
              var returnObject = { Id: id };
              var deleteResponse = $http.post(webApiBaseUrl + '/api/student/deletestudent', returnObject);
              deleteResponse.then(successCallback, failureCallback);
            }
          });
        };
        this.initialize();
      };
      return NSStudentManager;
    }
  ]).factory('NSStudent', [
    '$http',
    'webApiBaseUrl',
    '$bootbox',
    function ($http, webApiBaseUrl, $bootbox) {
      var NSStudent = function (studentId) {
        this.initialize = function () {
          var url = webApiBaseUrl + '/api/student/GetStudent/' + studentId;
          var studentData = $http.get(url);
          var self = this;
          self.SpecialEdLabels = [];
          self.StudentSchools = [];
          self.StudentAttributes = {};
          studentData.then(function (response) {
            angular.extend(self, response.data);
            if (self.SpecialEdLabels === null)
              self.SpecialEdLabels = [];
            if (self.StudentSchools === null)
              self.StudentSchools = [];
            if (self.StudentAttributes === null)
              self.StudentAttributes = {};
            // hack DOB into format for stupid UIB-datepopup control
            if (self.DOB == null) {
              self.DOB = moment().format('DD-MMM-YYYY');
            } else {
              var momentizedDate = moment(self.DOB);
              self.DOB = momentizedDate.format('DD-MMM-YYYY');
            }
          });
        };
        this.initialize();
        this.validateStudentIdentifer = function (studentIdentifier) {
          var self = this;
          var postObject = {
              StudentId: self.Id,
              StudentIdentifier: studentIdentifier
            };
          var url = webApiBaseUrl + '/api/student/IsStudentIDUnique';
          return $http.post(url, postObject);
        };
        // don't let them add the same registration twice
        this.addNewRegistration = function (school, schoolYear, grade, studentId) {
          var self = this;
          if (schoolYear === null || school === null || grade == null) {
            $bootbox.alert('Please select a school year, school and grade first.');
            return false;
          } else {
            for (var i = 0; i < self.StudentSchools.length; i++) {
              // can't be at same school twice in the same year
              if (self.StudentSchools[i].SchoolId == school.id && self.StudentSchools[i].SchoolStartYear == schoolYear.id) {
                $bootbox.alert('Student is already enrolled at this school/school year combination.');
                return false;
              }
              // if at another school, must be same grade
              if (self.StudentSchools[i].SchoolStartYear == schoolYear.id && self.StudentSchools[i].GradeId != grade.id) {
                $bootbox.alert('If student is at more than one school, he or she must be in the same grade at both schools.');
                return false;
              }
            }
            self.StudentSchools.unshift({
              id: -1,
              SchoolId: school.id,
              SchoolName: school.text,
              SchoolStartYear: schoolYear.id,
              SchoolYearLabel: schoolYear.text,
              GradeId: grade.id,
              GradeName: grade.text,
              StudentId: studentId,
              isNew: true
            });
            return true;
          }
        };
        this.validateUpdatedRegistration = function (school, schoolYear, grade, registrationId) {
          var self = this;
          if (schoolYear === null || school === null || grade == null) {
            $bootbox.alert('Please select a school year, school and grade first.');
            return false;
          } else {
            for (var i = 0; i < self.StudentSchools.length; i++) {
              // can't be at same school twice in the same year
              if (self.StudentSchools[i].SchoolId == school.id && self.StudentSchools[i].SchoolStartYear == schoolYear.id && registrationId != self.StudentSchools[i].Id) {
                $bootbox.alert('Student is already enrolled at this school/school year combination.');
                return false;
              }
              // if at another school, must be same grade
              if (self.StudentSchools[i].SchoolStartYear == schoolYear.id && self.StudentSchools[i].GradeId != grade.id && registrationId != self.StudentSchools[i].Id) {
                $bootbox.alert('If student is at more than one school, he or she must be in the same grade at both schools.');
                return false;
              }
            }
            return true;
          }
        };
        this.removeRegistration = function (studentSchool) {
          var self = this;
          var url = webApiBaseUrl + '/api/student/CanRemoveStudentSchool';
          var studentData = $http.post(url, studentSchool);
          studentData.then(function (response) {
            if (response.data.Success === true) {
              for (var i = 0; i < self.StudentSchools.length; i++) {
                if (self.StudentSchools[i].SchoolId === studentSchool.SchoolId && studentSchool.SchoolStartYear === self.StudentSchools[i].SchoolStartYear) {
                  self.StudentSchools.splice(i, 1);
                }
              }
            } else {
              $bootbox.alert('You either do not have access to this school, or the student is already attending a section at this school.');
            }
          }, function (response) {
          });
        };
      };
      return NSStudent;
    }
  ]).controller('StudentMoveController', [
    '$scope',
    'nsPinesService',
    'nsSelect2RemoteOptions',
    '$bootbox',
    'progressLoader',
    'NSStudentManager',
    function ($scope, nsPinesService, nsSelect2RemoteOptions, $bootbox, progressLoader, NSStudentManager) {
      var mgr = new NSStudentManager();
      $scope.resetForm = function () {
        $scope.moveSettings = {
          studentToMove: null,
          targetSection: null,
          moveComplete: false
        };
        $scope.warnings = [];
        $scope.errors = [];
      };
      $scope.resetForm();
      $scope.checkDifferentYears = function () {
        if ($scope.moveSettings.studentToMove === null || $scope.moveSettings.targetSection === null) {
          return false;
        }
        return $scope.moveSettings.studentToMove.SchoolStartYear != $scope.moveSettings.targetSection.SchoolStartYear;
      };
      $scope.$on('NSHTTPError', function (event, data) {
        $scope.errors.push({
          type: 'danger',
          msg: data
        });
        $('html, body').animate({ scrollTop: 0 }, 'fast');
      });
      $scope.quickSearchStudent = nsSelect2RemoteOptions.StudentDetailedQuickSearchRemoteOptions;
      $scope.quickSearchSection = nsSelect2RemoteOptions.quickSearchSectionsAllSchoolYearsRemoteOptions;
      // watch targetSection, show warning if not same year
      $scope.processMove = function () {
        if ($scope.moveSettings.studentToMove === null || $scope.moveSettings.targetSection === null) {
          $bootbox.alert('You must select a student to move and a target section before continuing.');
          return;
        }
        $bootbox.confirm('Are you sure you want to move this student to a new class?', function (response) {
          if (response) {
            // loader
            progressLoader.start();
            progressLoader.set(50);
            // do move
            mgr.moveStudent($scope.moveSettings.studentToMove, $scope.moveSettings.targetSection).then(function (response) {
              progressLoader.end();
              // pinesnotifcation
              nsPinesService.dataSavedSuccessfully();
              // reset form
              $scope.moveSettings.moveComplete = true;
            }, function (err) {
              progressLoader.end();
            });
          }
        });
      };
    }
  ]).controller('StudentListController', StudentListController).controller('StudentEditController', StudentEditController);
  /* Movies List Controller  */
  StudentListController.$inject = [
    '$scope',
    'NSStudentManager',
    'nsFilterOptionsService',
    'nsPinesService',
    '$location',
    '$bootbox',
    '$timeout',
    'spinnerService'
  ];
  function StudentListController($scope, NSStudentManager, nsFilterOptionsService, nsPinesService, $location, $bootbox, $timeout, spinnerService) {
    $scope.filterOptions = nsFilterOptionsService.options;
    $scope.studentManager = new NSStudentManager();
    $scope.sortClasses = {};
    $scope.sortColumns = { column: null };
    $scope.sort = function (sortField) {
      if ($scope.sortColumns.column == null) {
        // first sort
        $scope.sortColumns.column = sortField;
        $scope.sortClasses[sortField] = 'fa fa-chevron-up';
      } else {
        if ($scope.sortColumns.column.indexOf(sortField) < 0) {
          // if this field isn't currently being sorted by
          $scope.sortColumns.column = sortField;
          $scope.sortClasses = {};
          $scope.sortClasses[sortField] = 'fa fa-chevron-up';
        } else {
          // we are sorting by this field already
          if ($scope.sortColumns.column.indexOf('-') < 0) {
            // if this field isn't currently being sorted by descending
            $scope.sortColumns.column = '-' + sortField;
            $scope.sortClasses = {};
            $scope.sortClasses[sortField] = 'fa fa-chevron-down';
          } else {
            // remove sort
            $scope.sortColumns.column = null;
            $scope.sortClasses = {};
          }
        }
      }
    };
    var LoadData = function () {
      if ($scope.filterOptions.selectedGrade == null) {
        return;
      }
      $timeout(function () {
        spinnerService.show('tableSpinner');
      });
      $scope.studentManager.getStudentList($scope.filterOptions.selectedSchoolYear, $scope.filterOptions.selectedSchool, $scope.filterOptions.selectedGrade, $scope.filterOptions.selectedTeacher, $scope.filterOptions.selectedSection, $scope.filterOptions.selectedSectionStudent).then(function (response) {
        $scope.students = response.data.Students;
      }).finally(function () {
        spinnerService.hide('tableSpinner');
      });
      ;
    };
    $scope.deleteStudent = function (id) {
      // TODO: ensure all fields are valid
      $scope.studentManager.delete(id, function () {
        nsPinesService.dataDeletedSuccessfully();
        LoadData();
      });
    };
    $scope.processQuickSearchStudent = function () {
      if (angular.isDefined($scope.filterOptions.quickSearchStudent)) {
        $location.path('student-addedit/' + $scope.filterOptions.quickSearchStudent.id);
      } else {
        $bootbox.alert('Please select a Student first.');
      }
    };
    //$scope.$watch('filterOptions', function () {
    //    LoadData();
    //}, true);
    $scope.$on('NSSchoolYearOptionsUpdated', function (event, data) {
      LoadData();
    });
    $scope.$on('NSSchoolOptionsUpdated', function (event, data) {
      LoadData();
    });
    $scope.$on('NSGradeOptionsUpdated', function (event, data) {
      LoadData();
    });
    $scope.$on('NSTeacherOptionsUpdated', function (event, data) {
      LoadData();
    });
    $scope.$on('NSSectionOptionsUpdated', function (event, data) {
      LoadData();
    });
    $scope.$on('NSInitialLoadComplete', function (event, data) {
      LoadData();
    });  //$scope.$watchGroup(['filterOptions.selectedSchoolYear', 'filterOptions.selectedGrade', 'filterOptions.selectedTeacher', 'filterOptions.selectedSection'], function (newValues, oldValues) {
         //        LoadData();
         //}, true);
         //$scope.$watch('filterOptions.selectedSchoolYear', function (newVal) {
         //    if (newVal !== null) {
         //        LoadData();
         //    }
         //});
         //$scope.$watch('filterOptions.selectedGrade', function (newVal) {
         //    if (newVal !== null) {
         //        LoadData();
         //    }
         //});
         //$scope.$watch('filterOptions.selectedTeacher', function (newVal) {
         //    if (newVal !== null) {
         //        LoadData();
         //    }
         //});
         //$scope.$watch('filterOptions.selectedSection', function (newVal) {
         //    if (newVal !== null) {
         //        LoadData();
         //    }
         //});
         // initial load
         //LoadData();
  }
  /* Movies Edit Controller */
  StudentEditController.$inject = [
    '$scope',
    '$routeParams',
    '$location',
    'nsFilterOptionsService',
    'nsPinesService',
    'nsSelect2RemoteOptions',
    'NSStudent',
    'NSStudentManager',
    'NSStudentSpedLookupValues',
    'NSStudentAttributeLookups',
    'NSSchoolYearsAndSchools',
    '$bootbox'
  ];
  function StudentEditController($scope, $routeParams, $location, nsFilterOptionsService, nsPinesService, nsSelect2RemoteOptions, NSStudent, NSStudentManager, NSStudentSpedLookupValues, NSStudentAttributeLookups, NSSchoolYearsAndSchools, $bootbox) {
    $scope.student = new NSStudent($routeParams.id);
    $scope.studentManager = new NSStudentManager();
    $scope.settings = {
      AddYear: null,
      AddGrade: null,
      AddSchool: null
    };
    $scope.remoteStudentIdValidationPath = $scope.studentManager.remoteStudentIdValidationPath;
    $scope.student.SpecialEdLabels = [];
    $scope.student.LastValidationStatus = true;
    $scope.student.StudentAttributes = {};
    $scope.filterOptions = nsFilterOptionsService.options;
    $scope.allSpedLabels = new NSStudentSpedLookupValues();
    $scope.allAttributes = new NSStudentAttributeLookups();
    $scope.errors = [];
    $scope.s2Options = nsSelect2RemoteOptions;
    $scope.schoolsAndYears = new NSSchoolYearsAndSchools();
    $scope.remoteValidationError = {};
    $scope.edit = function (registration) {
      registration.editMode = true;
      registration.newGrade = {
        id: registration.GradeId,
        text: registration.GradeName
      };
      registration.newSchool = {
        id: registration.SchoolId,
        text: registration.SchoolName
      };
      registration.newSchoolYear = {
        id: registration.SchoolStartYear,
        text: registration.SchoolYearLabel
      };
    };
    $scope.saveRegistration = function (reg, newSchoolYear, newSchool, newGrade, registrations, studentId) {
      // validation checks
      if (angular.isUndefined(newSchoolYear) || newSchoolYear == null) {
        $bootbox.alert('Please enter a valid school year');
        return;
      }
      if (angular.isUndefined(newSchool) || newSchool == null) {
        $bootbox.alert('Please select a school');
        return;
      }
      if (angular.isUndefined(newGrade) || newGrade == null) {
        $bootbox.alert('Please select a grade');
        return;
      }
      // this.validateUpdatedRegistration = function (school, schoolYear, grade, registrationId) {
      var registrationValidationMessage = $scope.student.validateUpdatedRegistration(newSchool, newSchoolYear, newGrade, reg.Id);
      if (!registrationValidationMessage) {
        return;
      }
      reg.GradeId = newGrade.id;
      reg.GradeName = newGrade.text;
      reg.SchoolId = newSchool.id;
      reg.SchoolName = newSchool.text;
      reg.SchoolStartYear = newSchoolYear.id;
      reg.SchoolYearLabel = newSchoolYear.text;
      reg.editMode = false;
    };
    $scope.studentidSetArgs = function (val, el, attrs, ngModel) {
      return {
        StudentIdentifier: val,
        StudentId: $scope.student.Id
      };
    };
    $scope.checkRemoteValidationStatus = function (field) {
      if ($scope.remoteValidationError === null) {
        return true;
      } else {
        return false;
      }
    };
    $scope.datePopupStatus = { opened: false };
    $scope.getValidationMessage = function (field) {
      return $scope.remoteValidationError;
    };
    $scope.savestudent = function () {
      // TODO: ensure all fields are valid
      $scope.studentManager.save($scope.student).then(function (response) {
        nsPinesService.dataSavedSuccessfully();
        $location.path('student-list');
      });
    };
    $scope.studentidSetArgs = function (val, el, attrs, ngModel) {
      return {
        StudentID: $scope.student.Id,
        studentIdentifier: $scope.student.StudentIdentifier
      };
    };
    $scope.checkUnique = function (studentIdentifier) {
      // if we haven't already validated this value, keep going
      if (typeof $scope.student.validatedValue === 'undefined' || $scope.student.validatedValue !== studentIdentifier.$modelValue) {
        if (typeof $scope.student.validating === 'undefined' || $scope.student.validating === false) {
          if (typeof $scope.student.Id != 'undefined') {
            $scope.student.validating = true;
            $scope.student.validateStudentIdentifer(studentIdentifier.$modelValue).then(function (result) {
              if (result.data.Success) {
                $scope.student.validating = false;
                $scope.student.validatedValue = studentIdentifier.$modelValue;
                $scope.student.LastValidationStatus = true;
                return true;
              } else {
                $scope.student.validating = false;
                $scope.student.validatedValue = studentIdentifier.$modelValue;
                $scope.student.LastValidationStatus = false;
                return result.data.Status;
              }
            }, function (err) {
              $scope.errors.push({
                msg: '<strong>An Error Has Occurred</strong> Unable to validate the uniqueness of the Student ID.',
                type: 'danger'
              });
              $scope.student.validating = false;
              $scope.student.validatedValue = studentIdentifier.$modelValue;
            });
          }
        }
        $scope.student.validating = false;
      }
      return $scope.student.LastValidationStatus;
    };
    $scope.removeStudentSchool = function (studentSchool) {
      $scope.student.removeRegistration(studentSchool);
    };
    $scope.addStudentToSchoolAndYear = function () {
      if ($scope.student.addNewRegistration($scope.settings.AddSchool, $scope.settings.AddYear, $scope.settings.AddGrade, $scope.student.Id)) {
        $scope.settings.AddSchool = null;
        $scope.settings.AddYear = null;
        $scope.settings.AddGrade = null;
      }
    }  //var LoadGrades = function () {
       //    nsstudentService.loadGrades().then(function (data) {
       //        $scope.gradeList.push.apply($scope.gradeList, data.data);
       //    }, function (msg) {
       //        $scope.errors.push({ msg: '<strong>An Error Has Occurred while loading the student. </strong>  Please try again later. ', type: 'danger' });
       //        $('html, body').animate({ scrollTop: 0 }, 'fast')
       //        //alert('error loading grades');
       //    });
       //}
       // initial load
       //LoadGrades();
       //$scope.getCoTeacherRemoteOptions = nsSelect2RemoteOptions.CoTeacherRemoteOptions;
       //$scope.getTeacherRemoteOptions = nsSelect2RemoteOptions.TeacherRemoteOptions;
       //$scope.addStudentTostudentRemoteOptions = nsSelect2RemoteOptions.StudentTostudentRemoteOptions;
;
  }
}());
(function () {
  'use strict';
  angular.module('teamMeetingModule', []).controller('TeamMeetingListController', TeamMeetingListController).controller('TeamMeetingEditController', TeamMeetingEditController).controller('TeamMeetingInvitationController', TeamMeetingInvitationController).controller('TeamMeetingAttendController', TeamMeetingAttendController).controller('TeamMeetingAttendListController', TeamMeetingAttendListController).controller('TeamMeetingNotesController', TeamMeetingNotesController).controller('TeamMeetingEmailInviteController', TeamMeetingEmailInviteController).service('nsTMAssignStudentToIG', [
    '$http',
    function ($http) {
      var self = this;
      self.display = false;
      self.meetingName = '';
      self.studentName = '';
      self.meetingId = '';
      self.initialize = function (meetingId, meetingName, studentName) {
        self.meetingId = meetingId;
        self.meetingName = meetingName;
        self.studentName = studentName;
        self.display = true;
      };
    }
  ]).directive('nsDisplayAssigningStudent', [
    'nsTMAssignStudentToIG',
    '$location',
    function (nsTMAssignStudentToIG, $location) {
      return {
        restrict: 'E',
        templateUrl: 'templates/teammeeting-assign-student-to-ig.html',
        link: function (scope, element, attr) {
          scope.display = nsTMAssignStudentToIG.display;
          scope.teamMeetingName = nsTMAssignStudentToIG.meetingName;
          scope.teamMeetingId = nsTMAssignStudentToIG.meetingId;
          scope.studentName = nsTMAssignStudentToIG.studentName;
          scope.backToTeamMeeting = function () {
            nsTMAssignStudentToIG.display = false;
            $location.path('tm-attend/' + scope.teamMeetingId);
          };
        }
      };
    }
  ]).directive('nsObservationSummaryTmAttend', [
    '$routeParams',
    '$compile',
    '$templateCache',
    '$http',
    'nsFilterOptionsService',
    'NSObservationSummaryTeamMeetingAttendManager',
    'NSSortManager',
    '$uibModal',
    'nsTMAssignStudentToIG',
    '$location',
    'spinnerService',
    'nsLookupFieldService',
    function ($routeParams, $compile, $templateCache, $http, nsFilterOptionsService, NSObservationSummaryTeamMeetingAttendManager, NSSortManager, $uibModal, nsTMAssignStudentToIG, $location, spinnerService, nsLookupFieldService) {
      return {
        restrict: 'E',
        templateUrl: 'templates/observation-summary-tm-attend.html',
        scope: {
          selectedTeamMeetingId: '=',
          selectedStaffId: '=',
          selectedBenchmarkDateId: '=',
          selectedMeetingName: '='
        },
        link: function (scope, element, attr) {
          scope.observationSummaryManager = new NSObservationSummaryTeamMeetingAttendManager();
          scope.filterOptions = nsFilterOptionsService.options;
          scope.manualSortHeaders = {};
          scope.manualSortHeaders.studentNameHeaderClass = 'fa';
          scope.sortArray = [];
          scope.headerClassArray = [];
          scope.allSelected = false;
          scope.sortMgr = new NSSortManager();
          scope.selectedStudentResult = {};
          scope.goToDashboard = function (schoolYear, school, interventionist, interventionGroup, studentId, stint) {
            $location.path('ig-dashboard/' + schoolYear + '/' + school + '/' + interventionist + '/' + interventionGroup + '/' + studentId + '/' + stint);
          };
          scope.$on('NSFieldsUpdated', function (event, data) {
            LoadData();
          });
          scope.openInterventionPopup = function (studentResult) {
            scope.selectedStudentResult = studentResult;
            var modalInstance = $uibModal.open({
                templateUrl: 'interventionList.html',
                scope: scope,
                controller: function ($scope, $uibModalInstance) {
                  $scope.cancel = function () {
                    $uibModalInstance.dismiss('cancel');
                  };
                },
                size: 'lg'
              });
          };
          scope.assignStudentToIntervention = function (studentResult) {
            nsTMAssignStudentToIG.initialize(scope.selectedTeamMeetingId, scope.selectedMeetingName, studentResult.StudentName);
            $location.path('ig-manage');
          };
          scope.openNotesModal = function (studentId, teamMeetingId) {
            // show modal after data is loaded
            scope.observationSummaryManager.LoadNotes(studentId, teamMeetingId).then(function () {
              var modalInstance = $uibModal.open({
                  templateUrl: 'studentTMNotes.html',
                  scope: scope,
                  size: 'lg'
                });
            });
          };
          //scope.$watch('selectedTeamMeetingId', function (newVal, oldVal) {
          //    if (newVal !== oldVal) {
          //        if(scope.selectedTeamMeetingId != null)
          //        scope.observationSummaryManager.LoadData(scope.selectedTeamMeetingId, scope.selectedBenchmarkDateId, scope.selectedStaffId).then(function () { attachFieldsCallback() });
          //    }
          //});
          function LoadData() {
            spinnerService.show('tableSpinner');
            scope.observationSummaryManager.LoadData(scope.selectedTeamMeetingId, scope.selectedBenchmarkDateId, scope.selectedStaffId).then(function () {
              attachFieldsCallback();
            }).finally(function () {
              spinnerService.hide('tableSpinner');
            });
          }
          scope.$watch('selectedStaffId', function (newVal, oldVal) {
            if (newVal !== oldVal) {
              LoadData();
            }
          });
          var attachFieldsCallback = function () {
            // initialize the sort manager now that the data has been loaded
            scope.sortMgr.initialize(scope.manualSortHeaders, scope.sortArray, scope.headerClassArray, 'OSFieldResults', scope.observationSummaryManager.Scores);
            for (var j = 0; j < scope.observationSummaryManager.Scores.StudentResults.length; j++) {
              for (var k = 0; k < scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults.length; k++) {
                for (var i = 0; i < scope.observationSummaryManager.Scores.Fields.length; i++) {
                  if (scope.observationSummaryManager.Scores.Fields[i].DatabaseColumn == scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].DbColumn) {
                    //scope.observationSummaryManager.Scores[j].FieldResults[k].Field = $scope.fields[i];
                    // set display value
                    if (scope.observationSummaryManager.Scores.Fields[i].FieldType === 'DropdownFromDB') {
                      for (var p = 0; p < nsLookupFieldService.LookupFieldsArray.length; p++) {
                        if (nsLookupFieldService.LookupFieldsArray[p].LookupColumnName === scope.observationSummaryManager.Scores.Fields[i].LookupFieldName) {
                          // now find the specifc value that matches
                          for (var y = 0; y < nsLookupFieldService.LookupFieldsArray[p].LookupFields.length; y++) {
                            if (scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].IntValue === nsLookupFieldService.LookupFieldsArray[p].LookupFields[y].FieldSpecificId) {
                              scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].DisplayValue = nsLookupFieldService.LookupFieldsArray[p].LookupFields[y].FieldValue;
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
          };
          //scope.observationSummaryManager.LoadData(scope.selectedTeamMeetingId, scope.selectedBenchmarkDateId, scope.selectedStaffId).then(function () { attachFieldsCallback() });
          // delegate sorting to the sort manager
          scope.sort = function (column) {
            scope.sortMgr.sort(column);
          };
          function getDecimalColor(gradeId, studentFieldScore) {
            var benchmarkArray = null;
            for (var i = 0; i < scope.observationSummaryManager.BenchmarksByGrade.length; i++) {
              if (scope.observationSummaryManager.BenchmarksByGrade[i].GradeId == gradeId) {
                benchmarkArray = scope.observationSummaryManager.BenchmarksByGrade[i];
              }
              if (benchmarkArray != null) {
                for (var j = 0; j < benchmarkArray.Benchmarks.length; j++) {
                  if (benchmarkArray.Benchmarks[j].DbColumn === studentFieldScore.DbColumn && benchmarkArray.Benchmarks[j].AssessmentId === studentFieldScore.AssessmentId) {
                    if (studentFieldScore.DecimalValue != null) {
                      // not defined yet
                      //if (studentFieldScore.DecimalValue === $scope.Benchmarks[i].MaxScore) {
                      //	return 'obsGreen';
                      //}
                      if (studentFieldScore.DecimalValue >= benchmarkArray.Benchmarks[j].Decimal80) {
                        return 'obsBlue';
                      }
                      if (studentFieldScore.DecimalValue >= benchmarkArray.Benchmarks[j].DecimalMean) {
                        return '';
                      }
                      if (studentFieldScore.DecimalValue >= benchmarkArray.Benchmarks[j].Decimal20) {
                        return 'obsYellow';
                      }
                      if (studentFieldScore.DecimalValue <= benchmarkArray.Benchmarks[j].Decimal20) {
                        return 'obsRed';
                      }
                    }
                  }
                }
              }
            }
            return '';
          }
          function getIntColor(gradeId, studentFieldScore) {
            var benchmarkArray = null;
            for (var i = 0; i < scope.observationSummaryManager.BenchmarksByGrade.length; i++) {
              if (scope.observationSummaryManager.BenchmarksByGrade[i].GradeId == gradeId) {
                benchmarkArray = scope.observationSummaryManager.BenchmarksByGrade[i];
              }
              if (benchmarkArray != null) {
                for (var j = 0; j < benchmarkArray.Benchmarks.length; j++) {
                  if (benchmarkArray.Benchmarks[j].DbColumn === studentFieldScore.DbColumn && benchmarkArray.Benchmarks[j].AssessmentId === studentFieldScore.AssessmentId) {
                    if (studentFieldScore.IntValue != null) {
                      // not defined yet
                      //if (studentFieldScore.DecimalValue === $scope.Benchmarks[i].MaxScore) {
                      //	return 'obsGreen';
                      //}
                      if (studentFieldScore.IntValue >= benchmarkArray.Benchmarks[j].Exceeds && benchmarkArray.Benchmarks[j].Exceeds != null) {
                        return 'obsBlue';
                      }
                      if (studentFieldScore.IntValue >= benchmarkArray.Benchmarks[j].Meets && benchmarkArray.Benchmarks[j].Meets != null) {
                        return 'obsGreen';
                      }
                      if (studentFieldScore.IntValue >= benchmarkArray.Benchmarks[j].Approaches && benchmarkArray.Benchmarks[j].Approaches != null) {
                        return 'obsYellow';
                      }
                      if (studentFieldScore.IntValue < benchmarkArray.Benchmarks[j].Approaches && benchmarkArray.Benchmarks[j].Approaches != null) {
                        return 'obsRed';
                      }
                    }
                  }
                }
              }
            }
            return '';
          }
          scope.getBackgroundClass = function (gradeId, studentFieldScore) {
            switch (studentFieldScore.ColumnType) {
            case 'Textfield':
              return '';
              break;
            case 'DecimalRange':
              return getDecimalColor(gradeId, studentFieldScore);
              break;
            case 'DropdownRange':
              return getIntColor(gradeId, studentFieldScore);
              break;
            case 'DropdownFromDB':
              return getIntColor(gradeId, studentFieldScore);
              break;
            case 'CalculatedFieldClientOnly':
              return '';
              break;
            case 'CalculatedFieldDbBacked':
              return getIntColor(gradeId, studentFieldScore);
              break;
            case 'CalculatedFieldDbOnly':
              return '';
              break;
            default:
              return '';
              break;
            }
            return '';
          };
          // initial load
          LoadData();
        }
      };
    }
  ]).factory('NSObservationSummaryTeamMeetingAttendManager', [
    '$http',
    'webApiBaseUrl',
    function ($http, webApiBaseUrl) {
      var NSObservationSummaryTeamMeetingAttendManager = function () {
        this.initialize = function () {
        };
        this.LoadData = function (teamMeetingId, tddId, staffId) {
          var paramObj = {
              TeamMeetingId: teamMeetingId,
              TestDueDateId: 0,
              StaffId: staffId
            };
          var url = webApiBaseUrl + '/api/assessment/GetTeamMeetingAttendObservationSummary/';
          //var paramObj = {}
          var summaryData = $http.post(url, paramObj);
          var self = this;
          self.LookupLists = [];
          self.Scores = [];
          self.BenchmarksByGrade = [];
          self.InterventionsByStudent = [];
          // add a "NoteCount" to each studentresultdata
          return summaryData.then(function (response) {
            angular.extend(self, response.data);
            if (self.LookupLists === null)
              self.LookupLists = [];
            if (self.Scores === null)
              self.Scores = [];
            if (self.BenchmarksByGrade === null)
              self.BenchmarksByGrade = [];
            if (self.InterventionsByStudent === null)
              self.InterventionsByStudent = [];
            // hook up interventions to studentresults
            for (var i = 0; i < self.Scores.StudentResults.length; i++) {
              var currentStudent = self.Scores.StudentResults[i];
              for (var j = 0; j < self.InterventionsByStudent.length; j++) {
                if (self.InterventionsByStudent[j].StudentId === currentStudent.StudentId) {
                  currentStudent.Interventions = self.InterventionsByStudent[j];
                }
              }
            }
          }, function (response) {
          });
        };
      };
      return NSObservationSummaryTeamMeetingAttendManager;
    }
  ]).factory('NSTeamMeetingManager', [
    '$http',
    '$bootbox',
    'nsFilterOptionsService',
    'webApiBaseUrl',
    function ($http, $bootbox, nsFilterOptionsService, webApiBaseUrl) {
      var NSTeamMeetingManager = function () {
        this.initialize = function () {
        };
        this.getTeamMeetingList = function (schoolYear) {
          var returnObject = { SchoolYear: nsFilterOptionsService.normalizeParameter(schoolYear) };
          return $http.post(webApiBaseUrl + '/api/teammeeting/GetTeamMeetingList', returnObject);
        };
        this.createNewAttendeeGroup = function (attendees, groupName) {
          var returnObject = {
              GroupName: groupName,
              Attendees: attendees
            };
          var promise = $http.post(webApiBaseUrl + '/api/teammeeting/saveattendeegroup', returnObject);
          return promise;
        };
        this.loadStudentsForSection = function (sectionId, successCallback, failureCallback) {
          var loadResponse = $http.get(webApiBaseUrl + '/api/teammeeting/getsectiondetails/' + sectionId);
          loadResponse.then(successCallback, failureCallback);
        };
        this.save = function (teammeeting, successCallback, failureCallback) {
          var saveResponse = $http.post(webApiBaseUrl + '/api/teammeeting/saveteammeeting', teammeeting);
          saveResponse.then(successCallback, failureCallback);
        };
        this.delete = function (id, successCallback, failureCallback) {
          $bootbox.confirm('Are you sure you want to delete this team meeting?  You will not be able to delete a meeting that has Notes stored.', function (result) {
            if (result === true) {
              var returnObject = { Id: id };
              var deleteResponse = $http.post(webApiBaseUrl + '/api/teammeeting/deleteteammeeting', returnObject);
              deleteResponse.then(successCallback, failureCallback);
            }
          });
        };
        this.initialize();
      };
      return NSTeamMeetingManager;
    }
  ]).factory('NSTeamMeeting', [
    'nsPinesService',
    '$http',
    'webApiBaseUrl',
    function (nsPinesService, $http, webApiBaseUrl) {
      var NSTeamMeeting = function (id, flag) {
        var self = this;
        self.initialize = function () {
          var paramObj = {
              Id: id,
              flag: flag
            };
          var url = webApiBaseUrl + '/api/teammeeting/getteammeeting';
          var meetingData = $http.post(url, paramObj);
          self.Sections = [];
          self.Attendees = [];
          self.StaffGroups = [];
          self.TeamMeetingAttendances = [];
          self.TeamMeetingStudents = [];
          self.TeamMeetingStudentNotes = [];
          meetingData.then(function (response) {
            angular.extend(self, response.data);
            if (self.Sections === null)
              self.Sections = [];
            if (self.Attendees === null)
              self.Attendees = [];
            if (self.StaffGroups === null)
              self.StaffGroups = [];
            if (self.TeamMeetingAttendances === null)
              self.TeamMeetingAttendances = [];
            if (self.TeamMeetingStudents === null)
              self.TeamMeetingStudents = [];
            if (self.TeamMeetingStudentNotes === null)
              self.TeamMeetingStudentNotes = [];
            self.MeetingTime = moment(self.MeetingTime).toDate();
          }, function (response) {
          });
        };
        self.LoadNotes = function (studentId, teamMeetingId) {
          var paramObj = {
              StudentId: studentId,
              TeamMeetingId: teamMeetingId
            };
          var url = webApiBaseUrl + '/api/teammeeting/getnotesforstudentteammeeting';
          //var paramObj = {}
          var promise = $http.post(url, paramObj);
          return promise.then(function (response) {
            self.CurrentNotes = response.data.Notes;
            self.CurrentStudent = response.data.Student;
          });
        };
        self.saveSingleTeamMeetingAttendance = function (attendance) {
          var paramObj = { TeamMeetingAttendance: attendance };
          var url = webApiBaseUrl + '/api/teammeeting/savesingleteammeetingattendance';
          var promise = $http.post(url, paramObj);
          return promise;
        };
        self.saveAllTeamMeetingAttendances = function (attendances) {
          var paramObj = { TeamMeetingAttendances: attendances };
          var url = webApiBaseUrl + '/api/teammeeting/saveallteammeetingattendances';
          var promise = $http.post(url, paramObj);
          return promise;
        };
        self.getInvitationPreview = function (staffId) {
          var paramObj = {
              TeamMeetingId: self.ID,
              StaffId: staffId
            };
          var url = webApiBaseUrl + '/api/teammeeting/gettminvitationemailpreview';
          var promise = $http.post(url, paramObj);
          return promise;
        };
        self.SaveNote = function (note) {
          var paramObj = {
              StudentId: note.StudentID,
              TeamMeetingId: note.TeamMeetingID,
              NoteId: note.ID,
              NoteHtml: note.Note
            };
          var url = webApiBaseUrl + '/api/teammeeting/SaveNote';
          var promise = $http.post(url, paramObj);
          return promise;
        };
        self.DeleteNote = function (note) {
          var paramObj = { Id: note.ID };
          var url = webApiBaseUrl + '/api/teammeeting/DeleteNote';
          var promise = $http.post(url, paramObj);
          return promise;
        };
        self.removeGroup = function (groupId) {
          var self = this;
          var response = $http.get(webApiBaseUrl + '/api/teammeeting/getstaffforattendeegroup/' + groupId);
          response.then(function (data) {
            for (var i = self.Attendees.length - 1; i >= 0; i--) {
              for (var j = 0; j < data.data.length; j++) {
                if (self.Attendees[i].id === data.data[j].id) {
                  // if this is one of the ones we need to remove, split it
                  self.Attendees.splice(i, 1);
                  break;
                }
              }
            }
          }, genericFailure());
        };
        self.deleteGroup = function (groupId) {
          var responseObject = { Id: groupId };
          var response = $http.post(webApiBaseUrl + '/api/teammeeting/deleteattendeegroup/', responseObject);
          response.then(function (data) {
            for (var i = self.AttendeeGroups.length - 1; i >= 0; i--) {
              if (self.AttendeeGroups[i].Id === groupId) {
                // if this is one of the ones we need to remove, split it
                nsPinesService.dataDeletedSuccessfully();
                self.AttendeeGroups.splice(i, 1);
                break;
              }
            }
          }, genericFailure());
        };
        self.addGroup = function (groupId) {
          var response = $http.get(webApiBaseUrl + '/api/teammeeting/getstaffforattendeegroup/' + groupId);
          response.then(function (data) {
            for (var i = 0; i < data.data.length; i++) {
              var found = false;
              for (var j = 0; j < self.Attendees.length; j++) {
                if (self.Attendees[j].id === data.data[i].id) {
                  // if this is one of the ones we need to remove, split it
                  found = true;
                  break;
                }
              }
              if (!found) {
                self.Attendees.push(data.data[i]);
              }
            }
          }, genericFailure());
        };
        var genericFailure = function (error) {
        };
        self.initialize();
      };
      return NSTeamMeeting;
    }
  ]);
  TeamMeetingEditController.$inject = [
    '$scope',
    '$routeParams',
    '$location',
    'nsSectionService',
    'nsFilterOptionsService',
    'nsPinesService',
    'nsSelect2RemoteOptions',
    'NSTeamMeeting',
    'NSTeamMeetingManager',
    '$bootbox'
  ];
  function TeamMeetingEditController($scope, $routeParams, $location, nsSectionService, nsFilterOptionsService, nsPinesService, nsSelect2RemoteOptions, NSTeamMeeting, NSTeamMeetingManager, $bootbox) {
    $scope.meeting = new NSTeamMeeting($routeParams.id, false);
    $scope.meeting.Sections = [];
    $scope.meeting.Attendees = [];
    $scope.meeting.StaffGroups = [];
    $scope.meeting.TeamMeetingAttendances = [];
    $scope.meeting.TeamMeetingStudents = [];
    $scope.meeting.TeamMeetingStudentNotes = [];
    $scope.meetingManager = new NSTeamMeetingManager();
    $scope.filterOptions = nsFilterOptionsService.options;
    $scope.errors = [];
    $scope.selectedSection = null;
    $scope.selectedStudentSections = [];
    $scope.loadSection = function (section) {
      $scope.selectedSection = section;
    };
    // we don't care about these changes unless this is a new section
    //$scope.$watch('filterOptions', function () {
    //    if ($routeParams.id === "-1") {
    //        $scope.section.SchoolId = nsFilterOptionsService.normalizeParameter($scope.filterOptions.selectedSchool);
    //        $scope.section.SchoolYear = nsFilterOptionsService.tempNormalizeParameter($scope.filterOptions.selectedSchoolYear);
    //    }
    //}, true);
    $scope.removeGoup = function (groupId) {
      $scope.meeting.removeGroup(groupId);
    };
    $scope.deleteGroup = function (groupId) {
      $bootbox.confirm('Are You sure you want to delete this Attendee Group?', function (result) {
        if (result) {
          $scope.meeting.deleteGroup(groupId);
        }
      });
    };
    $scope.addGroup = function (groupId) {
      $scope.meeting.addGroup(groupId);
    };
    $scope.loadStudentsForSection = function (selectedSection) {
      if (selectedSection == null) {
        alert('Please select a section first.');
        return;
      }
      // go get section details and add to Sections[]
      $scope.meetingManager.loadStudentsForSection(selectedSection.id, loadStudentsCallback);
    };
    $scope.createNewAttendeeGroup = function (attendees) {
      $bootbox.prompt('What do you want to call this group?', function (result) {
        if (result === null) {
          nsPinesService.cancelled();
        } else {
          // call meeting manager to create new group and then add to the attendee group list
          $scope.meetingManager.createNewAttendeeGroup(attendees, result).then(function (response) {
            nsPinesService.buildMessage('Group Created', 'Your group was saved successfully', 'success');
            $scope.meeting.AttendeeGroups.push(response.data);
          });
        }
      });
    };
    var loadStudentsCallback = function (data) {
      // set the dynamic section to null to reset
      $scope.meeting.DynamicSection = null;
      $scope.meeting.Sections.push(data.data);
      $scope.loadSection(data.data);
    };
    // TODO: make this generic
    //var failureCallback = function (error) {
    //    alert('remove me and use generic');
    //}
    $scope.addStudentToSection = function () {
      if ($scope.section.addStudentToSection($scope.section.addStudent)) {
        $scope.section.addStudent = null;
      }
    };
    $scope.saveTeamMeeting = function () {
      // TODO: ensure all fields are valid
      $scope.meetingManager.save($scope.meeting, function () {
        nsPinesService.dataSavedSuccessfully();
        $location.path('tm-manage');
      }, function (msg) {
        $scope.errors.push({
          msg: '<strong>An Error Has Occurred</strong> ' + msg.data,
          type: 'danger'
        });
        $('html, body').animate({ scrollTop: 0 }, 'fast');
      });
    };
    $scope.closeAlert = function (index) {
      $scope.errors.splice(index, 1);
    };
    // initial load
    $scope.getCoTeacherRemoteOptions = nsSelect2RemoteOptions.CoTeacherRemoteOptions;
    $scope.getStaffGroupRemoteOptions = nsSelect2RemoteOptions.StaffGroupRemoteOptions;
    $scope.chooseSectionRemoteOptions = nsSelect2RemoteOptions.quickSearchSectionsRemoteOptions;
  }
  TeamMeetingInvitationController.$inject = [
    '$scope',
    '$q',
    '$http',
    '$uibModal',
    'nsPinesService',
    '$location',
    '$filter',
    '$routeParams',
    'NSTeamMeeting',
    'progressLoader',
    'webApiBaseUrl'
  ];
  function TeamMeetingInvitationController($scope, $q, $http, $uibModal, nsPinesService, $location, $filter, $routeParams, NSTeamMeeting, progressLoader, webApiBaseUrl) {
    //var self = this;
    $scope.status = {};
    $scope.status.sendEmailMode = false;
    $scope.meeting = new NSTeamMeeting($routeParams.id, true);
    $scope.meeting.selectedStaffId = null;
    $scope.saveSingleTeamMeetingAttendance = function (attendance) {
      progressLoader.start();
      progressLoader.set(50);
      $scope.meeting.saveSingleTeamMeetingAttendance(attendance).then(function (response) {
        progressLoader.end();
      });
    };
    $scope.toggleIncludeAllStudents = function () {
      for (var i = 0; i < $scope.meeting.TeamMeetingAttendances.length; i++) {
        var tma = $scope.meeting.TeamMeetingAttendances[i];
        tma.IncludeAllStudents = $scope.meeting.AllInclude;
      }
      progressLoader.start();
      progressLoader.set(50);
      $scope.meeting.saveAllTeamMeetingAttendances($scope.meeting.TeamMeetingAttendances).then(function (response) {
        progressLoader.end();
      });
    };
    $scope.toggleAllSelected = function () {
      for (var i = 0; i < $scope.meeting.TeamMeetingAttendances.length; i++) {
        var tma = $scope.meeting.TeamMeetingAttendances[i];
        tma.Selected = $scope.meeting.SelectAll;
      }
    };
    $scope.toggleAllAttended = function () {
      for (var i = 0; i < $scope.meeting.TeamMeetingAttendances.length; i++) {
        var tma = $scope.meeting.TeamMeetingAttendances[i];
        tma.Attended = $scope.meeting.AllAttended;
      }
      progressLoader.start();
      progressLoader.set(50);
      $scope.meeting.saveAllTeamMeetingAttendances($scope.meeting.TeamMeetingAttendances).then(function (response) {
        progressLoader.end();
      });
    };
    $scope.sendSelectedInvites = function () {
      var noticesToSend = [];
      // get the selected ones
      for (var i = 0; i < $scope.meeting.TeamMeetingAttendances.length; i++) {
        var tma = $scope.meeting.TeamMeetingAttendances[i];
        if (tma.Selected) {
          noticesToSend.push(tma);
        }
      }
      if (noticesToSend.length == 0) {
        nsPinesService.buildMessage('Nothing to send', 'You have not selected any staff to send an invitation to.', 'info');
        return;
      }
      nsPinesService.setIntervalMessage(noticesToSend.length);
      // only process the selected ones
      for (var i = 0; i < noticesToSend.length; i++) {
        var tma = noticesToSend[i];
        var postObject = {
            TeamMeetingId: $scope.meeting.ID,
            StaffId: tma.StaffID
          };
        $http.post(webApiBaseUrl + '/api/teammeeting/sendtminvitation', postObject).then(function () {
          nsPinesService.incrementInterval();
        });
      }
    };
    $scope.sendSelectedConcluded = function () {
      var noticesToSend = [];
      // get the selected ones
      for (var i = 0; i < $scope.meeting.TeamMeetingAttendances.length; i++) {
        var tma = $scope.meeting.TeamMeetingAttendances[i];
        if (tma.Selected) {
          noticesToSend.push(tma);
        }
      }
      if (noticesToSend.length == 0) {
        nsPinesService.buildMessage('Nothing to send', 'You have not selected any staff to send a concluded notice to.', 'info');
        return;
      }
      nsPinesService.setIntervalMessage(noticesToSend.length);
      // only process the selected ones
      for (var i = 0; i < noticesToSend.length; i++) {
        var tma = noticesToSend[i];
        var postObject = {
            TeamMeetingId: $scope.meeting.ID,
            StaffId: tma.StaffID
          };
        $http.post(webApiBaseUrl + '/api/teammeeting/sendtmconcluded', postObject).then(function () {
          nsPinesService.incrementInterval();
        });
      }
    };
    $scope.sendInvite = function (staffId) {
      var postObject = {
          TeamMeetingId: $scope.meeting.ID,
          StaffId: staffId
        };
      $http.post(webApiBaseUrl + '/api/teammeeting/sendtminvitation', postObject).then(function () {
        nsPinesService.emailSentSuccessfully();
      });
    };
    $scope.sendConcluded = function (staffId) {
      var postObject = {
          TeamMeetingId: $scope.meeting.ID,
          StaffId: staffId
        };
      $http.post(webApiBaseUrl + '/api/teammeeting/sendtmconcluded', postObject).then(function () {
        nsPinesService.emailSentSuccessfully();
      });
    };
    $scope.openDemoModal = function (staffId) {
      var paramStaffId = staffId;
      $scope.meeting.selectedStaffId = paramStaffId;
      $scope.meeting.getInvitationPreview(paramStaffId).then(function (response) {
        $scope.meeting.previewHtml = response.data;
        var modalInstance = $uibModal.open({
            templateUrl: 'demoModalContent.html',
            scope: $scope,
            controller: function ($scope, $uibModalInstance) {
              $scope.cancel = function () {
                $uibModalInstance.dismiss('cancel');
              };
            },
            size: 'lg'
          });
      });
    };
  }
  TeamMeetingAttendController.$inject = [
    '$scope',
    '$q',
    '$http',
    '$uibModal',
    'nsPinesService',
    '$location',
    '$filter',
    '$routeParams',
    'NSTeamMeeting',
    'nsFilterOptionsService'
  ];
  function TeamMeetingAttendController($scope, $q, $http, $uibModal, nsPinesService, $location, $filter, $routeParams, NSTeamMeeting, nsFilterOptionsService) {
    $scope.filterOptions = nsFilterOptionsService.options;
    $scope.status = {};
    $scope.meeting = new NSTeamMeeting($routeParams.teamMeeting, true);
    $scope.meeting.selectedStaffId = null;
  }
  TeamMeetingNotesController.$inject = [
    '$scope',
    '$q',
    '$http',
    '$uibModal',
    'nsPinesService',
    '$location',
    '$filter',
    '$routeParams',
    'NSTeamMeeting',
    '$bootbox'
  ];
  function TeamMeetingNotesController($scope, $q, $http, $uibModal, nsPinesService, $location, $filter, $routeParams, NSTeamMeeting, $bootbox) {
    // TODO: add current staff ID to a common service
    //var self = this;
    $scope.status = {};
    $scope.meeting = new NSTeamMeeting($routeParams.teamMeetingId, true);
    $scope.meeting.LoadNotes($routeParams.studentId, $routeParams.teamMeetingId);
    $scope.NewNote = {};
    $scope.NewNote.ID = -1;
    $scope.NewNote.TeamMeetingID = $routeParams.teamMeetingId;
    $scope.NewNote.StudentID = $routeParams.studentId;
    $scope.meeting.selectedStaffId = null;
    $scope.teamMeetingId = $routeParams.teamMeetingId;
    $scope.editingId = -1;
    $scope.getNoteClass = function (staffId, index) {
      var classArray = [
          'chat-success',
          'chat-midnightblue',
          'chat-primary',
          'chat-indigo',
          'chat-danger',
          'chat-orange'
        ];
      var randomIndex = Math.floor(index / classArray.length * classArray.length);
      var randomClass = classArray[randomIndex];
      if (staffId == 1191) {
        return 'me';
      } else {
        return randomClass;
      }
    };
    $scope.cancel = function () {
      $scope.editingId = -1;
    };
    $scope.cancelAndReturn = function () {
      $scope.editingId = -1;
      $location.path('tm-attend/' + $routeParams.teamMeetingId);
    };
    $scope.editNote = function (noteId) {
      $scope.editingId = noteId;
    };
    $scope.saveExistingNote = function (note) {
      // get note
      var editor = CKEDITOR.instances.editableNote;
      var udpatedText = editor.getData();
      note.Note = udpatedText;
      $scope.meeting.SaveNote(note).then(function () {
        nsPinesService.dataSavedSuccessfully();
        $scope.editingId = -1;
      });
    };
    $scope.deleteNote = function (note) {
      $bootbox.confirm('Are You sure you want to delete this Note?', function (result) {
        if (result) {
          $scope.meeting.DeleteNote(note).then(function () {
            nsPinesService.dataDeletedSuccessfully();
            // reload notes
            $scope.meeting.LoadNotes($routeParams.studentId, $routeParams.teamMeetingId);
          });
        }
      });
    };
    $scope.saveNewNote = function () {
      var editor = CKEDITOR.instances.mainEditor;
      var udpatedText = editor.getData();
      $scope.NewNote.Note = udpatedText;
      $scope.meeting.SaveNote($scope.NewNote).then(function () {
        nsPinesService.dataSavedSuccessfully();
        editor.setData('');
        $scope.NewNote.Note = '';
        // reload notes
        $scope.meeting.LoadNotes($routeParams.studentId, $routeParams.teamMeetingId);
      });
    };
    $scope.isEditing = function (noteId) {
      if ($scope.editingId === noteId) {
        return true;
      } else {
        return false;
      }
    };
    $scope.canEditNote = function (noteStaffId) {
      if (noteStaffId == 1191) {
        return true;
      } else {
        return false;
      }
    };
  }
  TeamMeetingEmailInviteController.$inject = [
    '$scope',
    '$q',
    '$http',
    '$uibModal',
    'nsPinesService',
    '$location',
    '$filter',
    '$routeParams',
    'NSTeamMeeting'
  ];
  function TeamMeetingEmailInviteController($scope, $q, $http, $uibModal, nsPinesService, $location, $filter, $routeParams, NSTeamMeeting) {
    //var self = this;
    $scope.invite = {};
    $scope.invite.testDueDateId = $routeParams.tddid;
    $scope.invite.meetingId = $routeParams.teamMeetingId;
    $scope.invite.staffId = $routeParams.staffId;
  }
  TeamMeetingAttendListController.$inject = [
    '$scope',
    '$q',
    '$http',
    'nsPinesService',
    '$location',
    '$filter',
    '$routeParams',
    'nsSectionDataEntryService',
    'nsFilterOptionsService',
    'NSTeamMeetingManager'
  ];
  function TeamMeetingAttendListController($scope, $q, $http, nsPinesService, $location, $filter, $routeParams, nsSectionDataEntryService, nsFilterOptionsService, NSTeamMeetingManager) {
    $scope.sortArray = [];
    $scope.headerClassArray = [];
    $scope.staticColumnsObj = {};
    $scope.staticColumnsObj.studentNameHeaderClass = 'fa';
    $scope.filterOptions = nsFilterOptionsService.options;
    $scope.TeamMeetingManager = new NSTeamMeetingManager();
    $scope.$watch('filterOptions.selectedSchoolYear', function (newValue, oldValue) {
      if (!angular.equals(newValue, oldValue)) {
        LoadData();
      }
    }, true);
    $scope.sort = function (column) {
      $scope.TeamMeetingManager.doSort(column, $scope.staticColumnsObj, $scope.fields, $scope.headerClassArray, $scope.sortArray);
    };
    $scope.defaultEditAction = function (studentResult, rowform) {
      var dataEntryPage = $scope.assessment.DefaultDataEntryPage;
      // default editing
      if (dataEntryPage === null || dataEntryPage === '') {
        rowform.$show();
      } else {
        $location.path(dataEntryPage + '/' + $routeParams.assessmentid + '/' + $scope.filterOptions.selectedSection.Id + '/' + $scope.filterOptions.selectedBenchmarkDate.Id + '/' + studentResult.StudentId + '/' + studentResult.ResultId);
      }
    };
    var LoadData = function () {
      if ($scope.filterOptions.selectedSchoolYear != null) {
        $scope.TeamMeetingManager.getTeamMeetingList($scope.filterOptions.selectedSchoolYear).then(function (data) {
          $scope.teamMeetings = data.data.TeamMeetings;
        }, function (msg) {
          alert('error loading results');
        });
      }
    };
    // initial load
    LoadData();  // $scope.sort('StudentName');
  }
  TeamMeetingListController.$inject = [
    '$scope',
    '$q',
    '$http',
    'nsPinesService',
    '$location',
    '$filter',
    '$routeParams',
    'nsSectionDataEntryService',
    'nsFilterOptionsService',
    'NSTeamMeetingManager'
  ];
  function TeamMeetingListController($scope, $q, $http, nsPinesService, $location, $filter, $routeParams, nsSectionDataEntryService, nsFilterOptionsService, NSTeamMeetingManager) {
    $scope.sortArray = [];
    $scope.headerClassArray = [];
    $scope.staticColumnsObj = {};
    $scope.staticColumnsObj.studentNameHeaderClass = 'fa';
    $scope.filterOptions = nsFilterOptionsService.options;
    $scope.TeamMeetingManager = new NSTeamMeetingManager();
    $scope.$watch('filterOptions.selectedSchoolYear', function (newValue, oldValue) {
      if (!angular.equals(newValue, oldValue)) {
        LoadData();
      }
    }, true);
    $scope.sort = function (column) {
      $scope.TeamMeetingManager.doSort(column, $scope.staticColumnsObj, $scope.fields, $scope.headerClassArray, $scope.sortArray);
    };
    $scope.defaultEditAction = function (studentResult, rowform) {
      var dataEntryPage = $scope.assessment.DefaultDataEntryPage;
      // default editing
      if (dataEntryPage === null || dataEntryPage === '') {
        rowform.$show();
      } else {
        $location.path(dataEntryPage + '/' + $routeParams.assessmentid + '/' + $scope.filterOptions.selectedSection.Id + '/' + $scope.filterOptions.selectedBenchmarkDate.Id + '/' + studentResult.StudentId + '/' + studentResult.ResultId);
      }
    };
    var LoadData = function () {
      if ($scope.filterOptions.selectedSchoolYear != null) {
        $scope.TeamMeetingManager.getTeamMeetingList($scope.filterOptions.selectedSchoolYear).then(function (data) {
          $scope.teamMeetings = data.data.TeamMeetings;
        }, function (msg) {
          alert('error loading results');
        });
      }
    };
    // initial load
    LoadData();  // $scope.sort('StudentName');
  }
}());
(function () {
  'use strict';
  angular.module('loginModule', []).controller('LoginController', LoginController);
  LoginController.$inject = [
    '$scope',
    '$global',
    '$http',
    'authService',
    'ngAuthSettings',
    '$location',
    'progressLoader',
    '$cookies'
  ];
  function LoginController($scope, $global, $http, authService, ngAuthSettings, $location, progressLoader, $cookies) {
    $global.set('fullscreen', true);
    var vm = this;
    vm.showAlternate = function () {
      if ($location.search().impersonate) {
        return true;
      }
      return false;
    };
    vm.message = '';
    vm.loginData = {
      userName: '',
      password: '',
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
      authService.login(vm.loginData).then(function (response) {
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
      $scope.errors.push({
        type: 'danger',
        msg: 'Invalid username or password. Please try again.'
      });
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
    vm.reset = function () {
      vm.email = '';
      vm.password = '';
    };
  }
}());
(function () {
  'use strict';
  angular.module('studentDashboardModule', []).directive('studentDashboard', [
    '$compile',
    '$templateCache',
    '$http',
    'nsFilterOptionsService',
    'NSAssesssmentLineGraphManager',
    'NSStudentInterventionManager',
    'NSStudentTMNotesManager',
    'NSStudentNotesManager',
    'NSStudentAssessmentFieldManager',
    'nsPinesService',
    'webApiBaseUrl',
    'ckEditorSettings',
    '$bootbox',
    '$routeParams',
    'authService',
    'spinnerService',
    '$q',
    '$timeout',
    function ($compile, $templateCache, $http, nsFilterOptionsService, NSAssesssmentLineGraphManager, NSStudentInterventionManager, NSStudentTMNotesManager, NSStudentNotesManager, NSStudentAssessmentFieldManager, nsPinesService, webApiBaseUrl, ckEditorSettings, $bootbox, $routeParams, authService, spinnerService, $q, $timeout) {
      return {
        restrict: 'EA',
        templateUrl: 'templates/student-dashboard-directive.html',
        scope: { student: '=' },
        link: function (scope, element, attr) {
          scope.filterOptions = nsFilterOptionsService.options;
          scope.settings = {
            studentLoaded: false,
            selectedStudent: null
          };
          scope.studentAssessmentFieldsManager = new NSStudentAssessmentFieldManager();
          scope.studentTMNotesManager = new NSStudentTMNotesManager();
          scope.studentNotesManager = new NSStudentNotesManager();
          scope.studentDataManager = new NSStudentInterventionManager();
          scope.ClassLineGraphDataManagers = [];
          scope.options = {
            language: 'en',
            allowedContent: true,
            entities: false,
            uploadUrl: ckEditorSettings.uploadurl + '?access_token=' + authService.token(),
            imageUploadUrl: ckEditorSettings.imageUploadUrl + '?access_token=' + authService.token(),
            filebrowserImageUploadUrl: ckEditorSettings.filebrowserImageUploadUrl + '?access_token=' + authService.token()
          };
          scope.$watch('student.id', function (newVal, oldVal) {
            if (newVal != null) {
              scope.settings.selectedStudent = scope.student;
              LoadLineGraphs(scope.settings.selectedStudent);
              scope.studentTMNotesManager.LoadData(scope.settings.selectedStudent.id);
            }
          });
          scope.saveNote = function (noteId, noteHtml, teamMeetingId, studentId, meeting) {
            if (noteHtml === '' || noteHtml == null) {
              $bootbox.alert('Please enter a note');
              return;
            }
            scope.studentTMNotesManager.saveNote(noteId, noteHtml, teamMeetingId, studentId).then(function (response) {
              nsPinesService.dataSavedSuccessfully();
              meeting.NewNoteHtml = '';
              meeting.newNoteEditMode = false;
              // reload the list
              scope.studentTMNotesManager.LoadData(scope.settings.selectedStudent.id);
            });
          };
          scope.saveStudentNote = function (noteId, noteHtml, studentId) {
            if (noteHtml === '' || noteHtml == null) {
              $bootbox.alert('Please enter a note');
              return;
            }
            scope.studentNotesManager.saveNote(noteId, noteHtml, studentId).then(function (response) {
              nsPinesService.dataSavedSuccessfully();
              // reload the list
              scope.settings.selectedStudent.newNoteEditMode = false;
              scope.settings.selectedStudent.NewNoteHtml = '';
              scope.studentNotesManager.LoadData(scope.settings.selectedStudent.id);
            });
          };
          scope.deleteNote = function (noteId) {
            $bootbox.confirm('Are you sure you want to delete this note?', function (response) {
              if (response) {
                scope.studentTMNotesManager.deleteNote(noteId).then(function (response) {
                  nsPinesService.dataDeletedSuccessfully();
                  scope.studentTMNotesManager.LoadData(scope.settings.selectedStudent.id);
                });
              }
            });
          };
          scope.deleteStudentNote = function (noteId) {
            $bootbox.confirm('Are you sure you want to delete this note?', function (response) {
              if (response) {
                scope.studentNotesManager.deleteNote(noteId).then(function (response) {
                  nsPinesService.dataDeletedSuccessfully();
                  scope.studentNotesManager.LoadData(scope.settings.selectedStudent.id);
                });
              }
            });
          };
          // scope.watch assessment and call function to load
          function LoadLineGraphs(student) {
            spinnerService.show('tableSpinner');
            scope.ClassLineGraphDataManagers = [];
            // load notes
            var promiseCollection = [];
            promiseCollection.push(scope.studentNotesManager.LoadData(student.id));
            promiseCollection.push(scope.studentDataManager.LoadData(student.id));
            promiseCollection.push(scope.studentAssessmentFieldsManager.LoadData(1, student.id));
            var innerPromiseCollection = [];
            $q.all(promiseCollection).then(function (response) {
              angular.forEach(scope.studentAssessmentFieldsManager.Fields, function (f) {
                var dataMgr = new NSAssesssmentLineGraphManager();
                innerPromiseCollection.push(dataMgr.LoadData(f.AssessmentId, f.DatabaseColumn, f.LookupFieldName, f.FieldType, student.id, f.DisplayLabel, -1, f.AssessmentName, scope.studentDataManager.Interventions));
                scope.ClassLineGraphDataManagers.push(dataMgr);
              });
              $q.all(innerPromiseCollection).then(function (response) {
                scope.settings.studentLoaded = true;
                spinnerService.hide('tableSpinner');
                $timeout(function () {
                  $(Highcharts.charts).each(function (i, chart) {
                    if (chart) {
                      chart.reflow();
                    }
                  });
                }, 1000);
              });
            });
          }
        }
      };
    }
  ]).controller('StudentDashboardController', [
    '$http',
    'nsFilterOptionsService',
    '$scope',
    'NSAssesssmentLineGraphManager',
    'NSStudentInterventionManager',
    'NSStudentTMNotesManager',
    'NSStudentNotesManager',
    'NSStudentAssessmentFieldManager',
    'nsPinesService',
    'webApiBaseUrl',
    'ckEditorSettings',
    '$bootbox',
    '$routeParams',
    'authService',
    'spinnerService',
    '$q',
    function ($http, nsFilterOptionsService, $scope, NSAssesssmentLineGraphManager, NSStudentInterventionManager, NSStudentTMNotesManager, NSStudentNotesManager, NSStudentAssessmentFieldManager, nsPinesService, webApiBaseUrl, ckEditorSettings, $bootbox, $routeParams, authService, spinnerService, $q) {
      $scope.filterOptions = nsFilterOptionsService.options;
      $scope.settings = {
        studentLoaded: false,
        selectedStudent: null
      };
      $scope.settings.alreadyLetUrlStudentLoad = false;
      $scope.$watch('filterOptions.selectedSectionStudent', function (newVal, oldVal) {
        if (newVal !== oldVal && newVal != null) {
          if (angular.isDefined($routeParams.id) && $scope.settings.alreadyLetUrlStudentLoad == false) {
            $scope.settings.alreadyLetUrlStudentLoad = true;
            return;
          }
          $scope.settings.selectedStudent = $scope.filterOptions.selectedSectionStudent;
        }
      });
      $scope.$watch('settings.selectedStudent', function (newVal, oldVal) {
        if (newVal !== oldVal && newVal != null) {
          $scope.settings.studentLoaded = true;
        }
      });
      if (angular.isDefined($routeParams.id)) {
        var paramObj = { id: $routeParams.id };
        $http.post(webApiBaseUrl + '/api/student/getstudentbyid', paramObj).then(function (response) {
          $scope.settings.studentLoaded = true;
          $scope.filterOptions.quickSearchStudent = response.data;
          $scope.settings.selectedStudent = response.data;
          $scope.settings.selectedStudent.text = response.data.LastName + ', ' + response.data.FirstName;
        });
      }
      $scope.processQuickSearchStudent = function () {
        if (angular.isDefined($scope.filterOptions.quickSearchStudent)) {
          $scope.settings.studentLoaded = true;
          $scope.settings.selectedStudent = $scope.filterOptions.quickSearchStudent;
          $scope.settings.selectedStudent.text = $scope.filterOptions.quickSearchStudent.LastName + ', ' + $scope.filterOptions.quickSearchStudent.FirstName;
        } else {
          $bootbox.alert('Please select a Student first.');
        }
      };
    }
  ]).factory('NSStudentDashboardManager', [
    '$http',
    'webApiBaseUrl',
    function ($http, webApiBaseUrl) {
      var NSStudentDashboardManager = function () {
        this.LoadData = function (studentId) {
          var url = webApiBaseUrl + '/api/studentdashboard/GetStudentObservationSummary/';
          var paramObj = { StudentId: studentId };
          var promise = $http.post(url, paramObj);
          var self = this;
          self.LookupLists = [];
          self.Scores = [];
          self.BenchmarksByGrade = [];
          return promise.then(function (response) {
            angular.extend(self, response.data);
            if (self.LookupLists === null)
              self.LookupLists = [];
            if (self.Scores === null)
              self.Scores = [];
            if (self.BenchmarksByGrade === null)
              self.BenchmarksByGrade = [];
          });
        };
      };
      return NSStudentDashboardManager;
    }
  ]).factory('NSStudentNotesManager', [
    '$http',
    'webApiBaseUrl',
    '$filter',
    function ($http, webApiBaseUrl, $filter) {
      var NSStudentNotesManager = function () {
        this.saveNote = function (noteId, noteHtml, studentId) {
          var url = webApiBaseUrl + '/api/student/savenote';
          var paramObj = {
              NoteId: noteId,
              NoteHtml: noteHtml,
              StudentId: studentId
            };
          var promise = $http.post(url, paramObj);
          return promise;
        };
        this.deleteNote = function (noteId) {
          var url = webApiBaseUrl + '/api/student/deletenote';
          var paramObj = { Id: noteId };
          var promise = $http.post(url, paramObj);
          return promise;
        };
        this.LoadData = function (studentId) {
          var url = webApiBaseUrl + '/api/student/getnotesforstudent/';
          var paramObj = { id: studentId };
          var promise = $http.post(url, paramObj);
          var self = this;
          self.Notes = [];
          return promise.then(function (response) {
            self.Notes = response.data.Notes;
          });
        };
      };
      return NSStudentNotesManager;
    }
  ]).factory('NSStudentTMNotesManager', [
    '$http',
    'webApiBaseUrl',
    '$filter',
    function ($http, webApiBaseUrl, $filter) {
      var NSStudentTMNotesManager = function () {
        this.saveNote = function (noteId, noteHtml, teamMeetingId, studentId) {
          var url = webApiBaseUrl + '/api/teammeeting/savenote';
          var paramObj = {
              TeamMeetingId: teamMeetingId,
              NoteId: noteId,
              NoteHtml: noteHtml,
              StudentId: studentId
            };
          var promise = $http.post(url, paramObj);
          return promise;
        };
        this.deleteNote = function (noteId) {
          var url = webApiBaseUrl + '/api/teammeeting/deletenote';
          var paramObj = { Id: noteId };
          var promise = $http.post(url, paramObj);
          return promise;
        };
        this.LoadData = function (studentId) {
          var url = webApiBaseUrl + '/api/teammeeting/getnotesforstudentteammeetings/';
          var paramObj = { id: studentId };
          var promise = $http.post(url, paramObj);
          var self = this;
          self.Meetings = [];
          // SAVE THIS LOGIC for Regular student notes
          function groupByYear(data) {
            if (data === null) {
              return [];
            }
            var groupedNotes = [];
            angular.forEach(data.Meetings, function (meeting) {
              var foundYear = $filter('filter')(groupedNotes, { SchoolYear: meeting.SchoolYear });
              // see if category already exists, if not, add it
              if (!foundYear.length) {
                var newGroupedNote = {
                    SchoolYear: meeting.SchoolYear,
                    Meetings: []
                  };
                newGroupedNote.Meetings.push(meeting);
                groupedNotes.push(newGroupedNote);
              } else {
                foundYear[0].Meetings.push(meeting);
              }
            });
            return groupedNotes;
          }
          return promise.then(function (response) {
            self.Meetings = response.data.Meetings;
          });
        };
      };
      return NSStudentTMNotesManager;
    }
  ]).directive('nsObservationSummaryStudent', [
    '$routeParams',
    '$compile',
    '$templateCache',
    '$http',
    'nsFilterOptionsService',
    '$filter',
    'NSStudentDashboardManager',
    'NSSortManager',
    'nsLookupFieldService',
    'observationSummaryAssessmentFieldChooserSvc',
    function ($routeParams, $compile, $templateCache, $http, nsFilterOptionsService, $filter, NSStudentDashboardManager, NSSortManager, nsLookupFieldService, observationSummaryAssessmentFieldChooserSvc) {
      return {
        restrict: 'E',
        templateUrl: 'templates/observation-summary-student.html',
        scope: {
          selectedStudentId: '=',
          selectedAssessmentIds: '='
        },
        link: function (scope, element, attr) {
          scope.observationSummaryManager = new NSStudentDashboardManager();
          scope.filterOptions = nsFilterOptionsService.options;
          scope.manualSortHeaders = {};
          scope.manualSortHeaders.firstNameHeaderClass = 'fa';
          scope.manualSortHeaders.lastNameHeaderClass = 'fa';
          scope.sortArray = [];
          scope.headerClassArray = [];
          scope.allSelected = false;
          scope.sortMgr = new NSSortManager();
          scope.lookupFieldsArray = nsLookupFieldService.LookupFieldsArray;
          scope.hideField = function (field) {
            observationSummaryAssessmentFieldChooserSvc.hideField(field).then(function (response) {
              scope.observationSummaryManager.LoadData(scope.selectedStudentId).then(function (response) {
                attachFieldsCallback();
              });
            });
          };
          scope.hideAssessment = function (assessment) {
            observationSummaryAssessmentFieldChooserSvc.hideAssessment(assessment).then(function (response) {
              scope.observationSummaryManager.LoadData(scope.selectedStudentId).then(function (response) {
                attachFieldsCallback();
              });
            });
          };
          scope.$on('NSFieldsUpdated', function (event, data) {
            scope.observationSummaryManager.LoadData(scope.selectedStudentId).then(function (response) {
              attachFieldsCallback();
            });
          });
          scope.$watch('selectedStudentId', function (newVal, oldVal) {
            if (newVal !== oldVal) {
              scope.observationSummaryManager.LoadData(scope.selectedStudentId).then(function (response) {
                attachFieldsCallback();
              });
            }
          });
          var attachFieldsCallback = function () {
            // initialize the sort manager now that the data has been loaded
            scope.sortMgr.initialize(scope.manualSortHeaders, scope.sortArray, scope.headerClassArray, 'OSFieldResults', scope.observationSummaryManager.Scores);
            for (var j = 0; j < scope.observationSummaryManager.Scores.StudentResults.length; j++) {
              for (var k = 0; k < scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults.length; k++) {
                for (var i = 0; i < scope.observationSummaryManager.Scores.Fields.length; i++) {
                  if (scope.observationSummaryManager.Scores.Fields[i].DatabaseColumn == scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].DbColumn) {
                    scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].Field = angular.copy(scope.observationSummaryManager.Scores.Fields[i]);
                    // set display value
                    if (scope.observationSummaryManager.Scores.Fields[i].FieldType === 'DropdownFromDB') {
                      for (var p = 0; p < scope.lookupFieldsArray.length; p++) {
                        if (scope.lookupFieldsArray[p].LookupColumnName === scope.observationSummaryManager.Scores.Fields[i].LookupFieldName) {
                          // now find the specifc value that matches
                          for (var y = 0; y < scope.lookupFieldsArray[p].LookupFields.length; y++) {
                            if (scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].IntValue === scope.lookupFieldsArray[p].LookupFields[y].FieldSpecificId) {
                              scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].DisplayValue = scope.lookupFieldsArray[p].LookupFields[y].FieldValue;
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
              // now select the students that should be selected from the team meeeting
              if (scope.teamMeetingStudents) {
                for (var n = 0; n < scope.teamMeetingStudents.length; n++) {
                  if (scope.teamMeetingStudents[n].StudentID === scope.observationSummaryManager.Scores.StudentResults[j].StudentId) {
                    // increment numstudents too
                    scope.observationSummaryManager.Scores.StudentResults[j].selected = true;
                  }
                }
              }
            }
          };
          scope.observationSummaryManager.LoadData(scope.selectedStudentId).then(function (response) {
            attachFieldsCallback();
          });
          // delegate sorting to the sort manager
          scope.sort = function (column) {
            scope.sortMgr.sort(column);
          };
          function getIntColor(gradeId, studentFieldScore, fieldValue) {
            var benchmarkArray = null;
            for (var i = 0; i < scope.observationSummaryManager.BenchmarksByGrade.length; i++) {
              if (scope.observationSummaryManager.BenchmarksByGrade[i].GradeId == gradeId) {
                benchmarkArray = scope.observationSummaryManager.BenchmarksByGrade[i];
              }
              if (benchmarkArray != null) {
                for (var j = 0; j < benchmarkArray.Benchmarks.length; j++) {
                  if (benchmarkArray.Benchmarks[j].DbColumn === studentFieldScore.DbColumn && benchmarkArray.Benchmarks[j].AssessmentId === studentFieldScore.AssessmentId) {
                    if (fieldValue != null) {
                      // not defined yet
                      //if (studentFieldScore.DecimalValue === $scope.Benchmarks[i].MaxScore) {
                      //	return 'obsPerfect';
                      //}
                      if (fieldValue >= benchmarkArray.Benchmarks[j].Exceeds && benchmarkArray.Benchmarks[j].Exceeds != null) {
                        return 'obsBlue';
                      }
                      if (fieldValue >= benchmarkArray.Benchmarks[j].Meets && benchmarkArray.Benchmarks[j].Meets != null) {
                        return 'obsGreen';
                      }
                      if (fieldValue >= benchmarkArray.Benchmarks[j].Approaches && benchmarkArray.Benchmarks[j].Approaches != null) {
                        return 'obsYellow';
                      }
                      if (fieldValue < benchmarkArray.Benchmarks[j].Approaches && benchmarkArray.Benchmarks[j].Approaches != null) {
                        return 'obsRed';
                      }
                    }
                  }
                }
              }
            }
            return '';
          }
          scope.getBackgroundClass = function (gradeId, studentFieldScore) {
            switch (studentFieldScore.ColumnType) {
            case 'Textfield':
              return '';
              break;
            case 'DecimalRange':
              return getIntColor(gradeId, studentFieldScore, studentFieldScore.DecimalValue);
              break;
            case 'DropdownRange':
              return getIntColor(gradeId, studentFieldScore, studentFieldScore.IntValue);
              break;
            case 'DropdownFromDB':
              return getIntColor(gradeId, studentFieldScore, studentFieldScore.IntValue);
              break;
            case 'CalculatedFieldClientOnly':
              return '';
              break;
            case 'CalculatedFieldDbBacked':
              return getIntColor(gradeId, studentFieldScore, studentFieldScore.IntValue);
              break;
            case 'CalculatedFieldDbOnly':
              return '';
              break;
            default:
              return '';
              break;
            }
            return '';
          };
        }
      };
    }
  ]);
}());
(function () {
  'use strict';
  angular.module('interventionDashboardModule', []).controller('InterventionDashboardController', [
    'nsFilterOptionsService',
    'NSInterventionDashboardManager',
    '$scope',
    'NSPieChartManager',
    'NSStudentAssessmentFieldManager',
    'NSAssesssmentIGLineGraphManager',
    'spinnerService',
    '$q',
    function (nsFilterOptionsService, NSInterventionDashboardManager, $scope, NSPieChartManager, NSStudentAssessmentFieldManager, NSAssesssmentIGLineGraphManager, spinnerService, $q) {
      $scope.filterOptions = nsFilterOptionsService.options;
      $scope.studentAssessmentFieldsManager = new NSStudentAssessmentFieldManager();
      $scope.dashMgr = new NSInterventionDashboardManager();
      $scope.pieMgr = new NSPieChartManager();
      $scope.settings = {};
      $scope.ClassLineGraphDataManagers = [];
      $scope.attendanceNoteColor = function (type) {
        switch (type.AttendanceStatus) {
        case 'None':
          return 'timeline-inverse';
        case 'No School':
          return 'timeline-inverse';
        case 'Intervention Delivered':
          return 'timeline-green';
        case 'Make-Up Lesson':
          return 'timeline-warning';
        default:
          return 'timeline-danger';
        }
      };
      $scope.selectPieSlice = function (status) {
        $scope.pieMgr.selectPieSlice(status);
      };
      $scope.attendanceBadgeClass = function (AttendanceStatus) {
        switch (AttendanceStatus) {
        case 'None':
          return 'badge-inverse';
        case 'No School':
          return 'badge-inverse';
        case 'Intervention Delivered':
          return 'badge-success';
        case 'Make-Up Lesson':
          return 'badge-warning';
        default:
          return 'badge-danger';
        }
      };
      $scope.$watch('filterOptions.selectedStint.id', function (newVal, oldVal) {
        if (newVal !== oldVal) {
          if (angular.isDefined(newVal) && newVal !== null)
            $scope.dashMgr.LoadAttendanceSummary($scope.filterOptions.selectedInterventionStudent.id, $scope.filterOptions.selectedStint.id).then(function (response) {
              $scope.pieMgr.SetData($scope.dashMgr.AttendanceSummary);
            });
        }
      });
      $scope.$watch('filterOptions.selectedInterventionStudent.id', function (newVal, oldVal) {
        if (newVal !== oldVal) {
          if (angular.isDefined(newVal) && newVal !== null) {
            LoadLineGraphs();
          }
        }
      });
      function LoadLineGraphs() {
        spinnerService.show('tableSpinner');
        $scope.ClassLineGraphDataManagers = [];
        $scope.studentAssessmentFieldsManager.LoadData(2, $scope.filterOptions.selectedInterventionStudent.id, $scope.filterOptions.selectedInterventionGroup.id).then(function (response) {
          var promiseCollection = [];
          angular.forEach($scope.studentAssessmentFieldsManager.Fields, function (f) {
            var dataMgr = new NSAssesssmentIGLineGraphManager();
            promiseCollection.push(dataMgr.LoadData(f.AssessmentId, f.DatabaseColumn, f.LookupFieldName, f.FieldType, $scope.filterOptions.selectedInterventionStudent.id, f.DisplayLabel, $scope.filterOptions.selectedInterventionGroup.id, f.AssessmentName, null, $scope.filterOptions.selectedSchoolYear.id));
            $scope.ClassLineGraphDataManagers.push(dataMgr);
          });
          $q.all(promiseCollection).then(function (response) {
            spinnerService.hide('tableSpinner');
          });
        });
      }
    }
  ]).factory('NSInterventionDashboardManager', [
    '$http',
    'webApiBaseUrl',
    function ($http, webApiBaseUrl) {
      var NSInterventionDashboardManager = function () {
        var self = this;
        //self.LoadStints = function (studentId, interventionGroupId) {
        //    var url = webApiBaseUrl + '/api/interventiongroup/GetInterventionGroupStints';
        //    var paramObj = { StudentId: studentId, InterventionGroupId: interventionGroupId }
        //    var promise = $http.post(url, paramObj);
        //    return promise.then(function (response) {
        //        angular.extend(self, response.data);
        //        if (self.Stints === null) self.Stints = [];
        //    });
        //}
        self.LoadAttendanceSummary = function (studentId, stintId) {
          var url = webApiBaseUrl + '/api/interventiondashboard/GetStintAttendanceSummary';
          var paramObj = {
              StudentId: studentId,
              StintId: stintId
            };
          var promise = $http.post(url, paramObj);
          return promise.then(function (response) {
            angular.extend(self, response.data);
            if (self.Notes === null)
              self.Notes = [];
            if (self.AttendanceSummary === null)
              self.AttendanceSummary = [];
            self.TotalDays = 0;
            angular.forEach(self.AttendanceSummary, function (item) {
              self.TotalDays += item.Count;
            });  //if (self.NotesGroupedByType === null) self.NotesGroupedByType = [];
          });
        };
      };
      return NSInterventionDashboardManager;
    }
  ]).directive('nsInterventionDashboardRow', [
    '$http',
    function ($http) {
    }
  ]).factory('NSPieChartManager', [
    '$http',
    'webApiBaseUrl',
    function ($http, webApiBaseUrl) {
      var NSPieChartManager = function () {
        var self = this;
        self.chartConfig = {};
        self.settings = { selectedStatus: undefined };
        self.SetData = function (attendanceSummary) {
          if (attendanceSummary != null) {
            self.AttendanceSummary = attendanceSummary;
            self.postDataLoadSetup();
          }
        };
        self.selectPieSlice = function (slice) {
          if (self.settings.selectedStatus == slice || slice == null) {
            self.settings.selectedStatus = undefined;
          } else {
            self.settings.selectedStatus = slice;
          }
        };
        self.postDataLoadSetup = function () {
          var self = this;
          var currentMax = 1;
          var data = [];
          var attendanceColor = {
              'Teacher Absent': '#870000',
              'Teacher Unavailable': '#D03737',
              'Child Absent': '#AC1313',
              'Child Unavailable': '#F45B5B',
              'No School': '#434348',
              'Intervention Delivered': '#90ED7D',
              'Make-Up Lesson': '#E4D354 ',
              'Non-Cycle Day': '#4697ce',
              'None': '#cccccc'
            };
          var highchartsNgConfig = {};
          // set up series
          //color: attendanceColor[self.AttendanceSummary[i].StatusLabel]
          for (var i = 0; i < self.AttendanceSummary.length; i++) {
            data.push({
              name: self.AttendanceSummary[i].StatusLabel,
              y: self.AttendanceSummary[i].Count,
              color: attendanceColor[self.AttendanceSummary[i].StatusLabel]
            });
          }
          highchartsNgConfig = {
            options: {
              chart: {
                type: 'pie',
                plotShadow: false,
                plotBorderWidth: null,
                plotBackgroundColor: null
              },
              credits: { enabled: false },
              tooltip: {
                formatter: function () {
                  if (this.y > 0)
                    return this.y + ' (' + this.percentage.toFixed(0) + '%)';
                }
              },
              plotOptions: {
                pie: {
                  allowPointSelect: true,
                  cursor: 'pointer',
                  point: {
                    events: {
                      click: function (event) {
                        self.selectPieSlice(this.name);
                      }
                    }
                  },
                  dataLabels: {
                    enabled: true,
                    format: '{point.name} ({point.percentage:.1f}%)',
                    style: { color: Highcharts.theme && Highcharts.theme.contrastTextColor || 'black' }
                  },
                  showInLegend: true
                }
              }
            },
            series: [{
                name: 'Attendance',
                colorByPoint: true,
                data: data
              }],
            title: { text: '' },
            loading: false,
            func: function (chart) {
            }
          };
          self.chartConfig = highchartsNgConfig;
        };
      };
      return NSPieChartManager;
    }
  ]);
}());
(function () {
  'use strict';
  angular.module('videosModule', []).service('NSVideosService', [
    '$http',
    'webApiBaseUrl',
    function ($http, webApiBaseUrl) {
      //this.options = {};
      var self = this;
      self.getVideosByPage = function (pageNumber) {
        var paramObj = { Id: pageNumber };
        var url = webApiBaseUrl + '/api/video/GetVzaarVideoList/';
        var promise = $http.post(url, paramObj);
        return promise;
      };
      //
      self.loadVideosForDistrict = function (gradeId) {
        var paramObj = { Id: gradeId };
        var url = webApiBaseUrl + '/api/video/GetDistrictVideos';
        var promise = $http.post(url, paramObj);
        return promise;
      };
      self.loadAllVideos = function () {
        var url = webApiBaseUrl + '/api/video/GetAllVideos';
        var promise = $http.get(url);
        return promise;
      };
      self.saveVideo = function (video) {
        var paramObj = { Video: video };
        var url = webApiBaseUrl + '/api/interventiontoolkit/SaveVideo';
        var promise = $http.post(url, paramObj);
        return promise;
      };
      self.removeVideo = function (video) {
        var paramObj = { Id: video.Id };
        var url = webApiBaseUrl + '/api/video/RemoveVideo';
        var promise = $http.post(url, paramObj);
        return promise;
      };
    }
  ]).controller('ManageVideosController', [
    '$bootbox',
    'nsPinesService',
    'NSVideosService',
    '$scope',
    'nsSelect2RemoteOptions',
    '$uibModal',
    'spinnerService',
    '$timeout',
    function ($bootbox, nsPinesService, NSVideosService, $scope, nsSelect2RemoteOptions, $uibModal, spinnerService, $timeout) {
      $scope.nsSelect2RemoteOptions = nsSelect2RemoteOptions;
      $scope.unsavedVideo = null;
      $scope.newVideo = function () {
        $scope.unsavedVideo = {
          Id: -1,
          UploadedVideoFile: {},
          VideoName: 'New Video'
        };
      };
      $scope.removeVideo = function (video) {
        $bootbox.confirm('Are you sure you want to delete this video?', function (response) {
          if (response) {
            NSVideosService.removeVideo(video).finally(function (response) {
              nsPinesService.dataDeletedSuccessfully();
              LoadVideos();
            });
          }
        });
      };
      $scope.cancelNewRowEdit = function (rowform) {
        rowform.$cancel();
        $scope.unsavedVideo = null;
      };
      $scope.displayVideoDialog = function (video, $event) {
        $event.preventDefault();
        $event.stopPropagation();
        $scope.selectedVideo = video;
        var modalInstance = $uibModal.open({
            templateUrl: 'playVideo.html',
            scope: $scope,
            controller: function ($scope, $uibModalInstance, $sce) {
              $scope.getSrc = function () {
                if ($scope.selectedVideo) {
                  return $sce.trustAsResourceUrl('https://view.vzaar.com/' + $scope.selectedVideo.VideoStreamId + '/player?apiOn=true');
                }
                return $sce.trustAsResourceUrl('about:blank');
              };
              $scope.cancel = function () {
                $scope.selectedVideo = null;
                $uibModalInstance.dismiss('cancel');
              };
            },
            size: 'lg'
          });
      };
      var LoadVideos = function () {
        $timeout(function () {
          spinnerService.show('tableSpinner');
        });
        NSVideosService.loadAllVideos().then(function (response) {
          $scope.videos = response.data.Videos;
        }).finally(function () {
          spinnerService.hide('tableSpinner');
        });
      };
      $scope.saveVideo = function (video) {
        NSVideosService.saveVideo(video).then(function (response) {
          $scope.unsavedVideo = null;
          nsPinesService.dataSavedSuccessfully();
          LoadVideos();
        });
      };
      LoadVideos();
    }
  ]).controller('ViewVideosController', [
    '$bootbox',
    'nsPinesService',
    'NSVideosService',
    '$scope',
    '$uibModal',
    function ($bootbox, nsPinesService, NSVideosService, $scope, $uibModal) {
      $scope.settings = { selectedGradeId: null };
      $scope.galleryFilter = 'all';
      $scope.displayVideoDialog = function (video, $event) {
        $event.preventDefault();
        $event.stopPropagation();
        $scope.selectedVideo = video;
        var modalInstance = $uibModal.open({
            templateUrl: 'playVideo.html',
            scope: $scope,
            controller: function ($scope, $uibModalInstance, $sce) {
              $scope.getSrc = function () {
                if ($scope.selectedVideo) {
                  return $sce.trustAsResourceUrl('https://view.vzaar.com/' + $scope.selectedVideo.VideoStreamId + '/player?apiOn=true');
                }
                return $sce.trustAsResourceUrl('about:blank');
              };
              $scope.cancel = function () {
                $scope.selectedVideo = null;
                $uibModalInstance.dismiss('cancel');
              };
            },
            size: 'lg'
          });
      };
      NSVideosService.loadVideosForDistrict($scope.settings.selectedGradeId).then(function (response) {
        $scope.videos = response.data.Videos;
        for (var i = 0; i < $scope.videos.length; i++) {
          $scope.videos[i].GradeGroup = [];
          for (var j = 0; j < $scope.videos[i].Grades.length; j++) {
            $scope.videos[i].GradeGroup.push($scope.videos[i].Grades[j].id + '');
          }
        }
      });
    }
  ]);
}());
(function () {
  'use strict';
  angular.module('districtSettingsModule', []).controller('DistrictBenchmarksController', [
    '$scope',
    '$routeParams',
    '$location',
    'nsSectionService',
    'nsFilterOptionsService',
    'nsPinesService',
    'nsSelect2RemoteOptions',
    'DistrictBenchmarksManager',
    '$bootbox',
    'progressLoader',
    function ($scope, $routeParams, $location, nsSectionService, nsFilterOptionsService, nsPinesService, nsSelect2RemoteOptions, DistrictBenchmarksManager, $bootbox, progressLoader) {
      $scope.dataMgr = new DistrictBenchmarksManager();
      //$scope.assessments = $scope.fieldsManager.Assessments;
      $scope.errors = [];
      $scope.$on('NSHTTPError', function (event, data) {
        $scope.errors.push({
          type: 'danger',
          msg: data
        });
        $('html, body').animate({ scrollTop: 0 }, 'fast');
      });
      $scope.settings = {};
      $scope.notImplemented = function () {
        $bootbox.alert('Note: This feature is not implemented yet.');
      };
      $scope.saveAssessmentData = function (result) {
        $scope.dataMgr.SaveBenchmark(result).then(function (response) {
          nsPinesService.dataSavedSuccessfully();
          //$scope.dataMgr.LoadData(1, 'FPValueId', 'FPScale');
          $scope.dataMgr.LoadData($scope.settings.selectedAssessmentField.AssessmentId, $scope.settings.selectedAssessmentField.FieldName, $scope.settings.selectedAssessmentField.LookupFieldName);
        });
      };
      $scope.deleteAssessmentData = function (result) {
        $bootbox.confirm('Are you sure you want to delete this benchmark record?', function (response) {
          if (response) {
            progressLoader.start();
            progressLoader.set(50);
            $scope.dataMgr.DeleteBenchmark(result).then(function (response) {
              nsPinesService.dataDeletedSuccessfully();
              $scope.dataMgr.LoadData($scope.settings.selectedAssessmentField.AssessmentId, $scope.settings.selectedAssessmentField.FieldName, $scope.settings.selectedAssessmentField.LookupFieldName).then(function (response) {
                progressLoader.end();
              });
            });
          }
        });
      };
      $scope.$watch('settings.selectedAssessmentField', function (newValue, oldValue) {
        if (!angular.equals(newValue, oldValue) && newValue !== null) {
          progressLoader.start();
          progressLoader.set(50);
          $scope.dataMgr.LoadData($scope.settings.selectedAssessmentField.AssessmentId, $scope.settings.selectedAssessmentField.FieldName, $scope.settings.selectedAssessmentField.LookupFieldName).then(function (response) {
            progressLoader.end();
          });
        }
      }, true);
      $scope.dataMgr.LoadAssessmentFields();
    }
  ]).controller('StudentAttributeController', [
    '$scope',
    '$routeParams',
    '$location',
    'nsSectionService',
    'nsFilterOptionsService',
    'nsPinesService',
    'nsSelect2RemoteOptions',
    'DistrictStudentAttributesManager',
    '$bootbox',
    'progressLoader',
    '$uibModal',
    function ($scope, $routeParams, $location, nsSectionService, nsFilterOptionsService, nsPinesService, nsSelect2RemoteOptions, DistrictStudentAttributesManager, $bootbox, progressLoader, $uibModal) {
      $scope.dataMgr = new DistrictStudentAttributesManager();
      $scope.settings = { newAttribute: {} };
      $scope.showNewAttributeModal = function () {
        var modalInstance = $uibModal.open({
            templateUrl: 'newAttribute.html',
            scope: $scope,
            controller: function ($scope, $uibModalInstance) {
              $scope.saveNewAttribute = function (att) {
                if (!att.AttributeName || att.AttributeName == '') {
                  alert('The Attribute must have a name.');
                  return;
                }
                $scope.saveAttribute(att);
                $uibModalInstance.dismiss('cancel');
              };
              $scope.cancel = function () {
                $uibModalInstance.dismiss('cancel');
              };
            },
            size: 'md'
          });
      };
      $scope.saveAttribute = function (att) {
        $scope.dataMgr.SaveAttribute(att).then(function (response) {
          nsPinesService.dataSavedSuccessfully();
          $scope.dataMgr.LoadStudentAttributes();
          $scope.settings.newAttribute = {};
        }, function (err) {
          $scope.dataMgr.LoadStudentAttributes();
        });
      };
      $scope.saveAttributeValue = function (att) {
        $scope.dataMgr.SaveAttributeValue(att).then(function (response) {
          nsPinesService.dataSavedSuccessfully();
          $scope.dataMgr.LoadStudentAttributes();
        }, function (err) {
          $scope.dataMgr.LoadStudentAttributes();
        });
      };
      $scope.saveNewAttributeValue = function (attribute) {
        if (attribute.addAttributeLookupValue == '' || !attribute.addAttributeLookupValue) {
          $bootbox.alert('New Attribute Value must have a name.');
          return;
        }
        var addAttributeValue = {
            AttributeId: attribute.Id,
            LookupValue: attribute.addAttributeLookupValue,
            Description: attribute.addAttributeDescription
          };
        $scope.dataMgr.SaveAttributeValue(addAttributeValue).then(function (response) {
          nsPinesService.dataSavedSuccessfully();
          attribute.addAttributeLookupValue = '';
          attribute.addAttributeDescription = '';
          $scope.dataMgr.LoadStudentAttributes();
        }, function (err) {
          $scope.dataMgr.LoadStudentAttributes();
        });
      };
      $scope.startEdit = function (rowform) {
        rowform.$show();
      };
      $scope.before = function (rowform) {
        rowform.$setSubmitted();
        if (rowform.$valid) {
          return;
        } else
          return 'At least one required field is not filled out.';
      };
      $scope.deleteAttribute = function (attribute) {
        $bootbox.confirm('Are you sure you want to delete the Attribute "' + attribute.AttributeName + '"?<br><br><span style="color:red;font-weight:bold">NOTE: This will delete any attribute data associated with students and is IRREVERSIBLE!</span>', function (response) {
          if (response) {
            progressLoader.start();
            progressLoader.set(50);
            $scope.dataMgr.DeleteAttribute(attribute).then(function (response) {
              nsPinesService.dataDeletedSuccessfully();
              $scope.dataMgr.LoadStudentAttributes().then(function (response) {
                progressLoader.end();
              });
            });
          }
        });
      };
      $scope.deleteAttributeValue = function (attribute) {
        $bootbox.confirm('Are you sure you want to delete this Attribute Value "' + attribute.LookupValue + '"?<br><br><span style="color:red;font-weight:bold">NOTE: This will delete any attribute value data associated with students and is IRREVERSIBLE!</span>', function (response) {
          if (response) {
            progressLoader.start();
            progressLoader.set(50);
            $scope.dataMgr.DeleteAttributeValue(attribute).then(function (response) {
              nsPinesService.dataDeletedSuccessfully();
              $scope.dataMgr.LoadStudentAttributes().then(function (response) {
                progressLoader.end();
              });
            });
          }
        });
      };
      $scope.dataMgr.LoadStudentAttributes();
    }
  ]).controller('DistrictYearlyAssessmentBenchmarksController', [
    '$scope',
    '$routeParams',
    '$location',
    'nsSectionService',
    'nsFilterOptionsService',
    'nsPinesService',
    'nsSelect2RemoteOptions',
    'DistrictYearlyAssessmentBenchmarksManager',
    '$bootbox',
    'progressLoader',
    function ($scope, $routeParams, $location, nsSectionService, nsFilterOptionsService, nsPinesService, nsSelect2RemoteOptions, DistrictYearlyAssessmentBenchmarksManager, $bootbox, progressLoader) {
      $scope.dataMgr = new DistrictYearlyAssessmentBenchmarksManager();
      //$scope.assessments = $scope.fieldsManager.Assessments;
      $scope.errors = [];
      $scope.$on('NSHTTPError', function (event, data) {
        $scope.errors.push({
          type: 'danger',
          msg: data
        });
        $('html, body').animate({ scrollTop: 0 }, 'fast');
      });
      $scope.settings = {};
      $scope.saveAssessmentData = function (result) {
        $scope.dataMgr.SaveBenchmark(result).then(function (response) {
          nsPinesService.dataSavedSuccessfully();
          $scope.dataMgr.LoadData($scope.settings.selectedAssessmentField.AssessmentId, $scope.settings.selectedAssessmentField.FieldName, $scope.settings.selectedAssessmentField.LookupFieldName);
        });
      };
      $scope.deleteAssessmentData = function (result) {
        $bootbox.confirm('Are you sure you want to delete this benchmark record?', function (response) {
          if (response) {
            progressLoader.start();
            progressLoader.set(50);
            $scope.dataMgr.DeleteBenchmark(result).then(function (response) {
              nsPinesService.dataDeletedSuccessfully();
              $scope.dataMgr.LoadData($scope.settings.selectedAssessmentField.AssessmentId, $scope.settings.selectedAssessmentField.FieldName, $scope.settings.selectedAssessmentField.LookupFieldName).then(function (response) {
                progressLoader.end();
              });
            });
          }
        });
      };
      $scope.$watch('settings.selectedAssessmentField', function (newValue, oldValue) {
        if (!angular.equals(newValue, oldValue)) {
          progressLoader.start();
          progressLoader.set(50);
          $scope.dataMgr.LoadData($scope.settings.selectedAssessmentField.AssessmentId, $scope.settings.selectedAssessmentField.FieldName, $scope.settings.selectedAssessmentField.LookupFieldName).then(function (response) {
            progressLoader.end();
          });
        }
      }, true);
      $scope.dataMgr.LoadAssessmentFields();
    }
  ]).factory('DistrictBenchmarksManager', [
    '$http',
    'webApiBaseUrl',
    function ($http, webApiBaseUrl) {
      var NSBenchmarksManager = function () {
        var self = this;
        self.LoadData = function (assessmentId, fieldName, lookupFieldName) {
          var paramObj = {
              AssessmentId: assessmentId,
              FieldName: fieldName,
              LookupFieldName: lookupFieldName
            };
          var url = webApiBaseUrl + '/api/benchmark/GetDistrictBenchmarks';
          var benchmarksPromise = $http.post(url, paramObj);
          return benchmarksPromise.then(function (response) {
            self.benchmarks = response.data.Benchmarks;
          });
        };
        self.LoadAssessmentFields = function () {
          var url = webApiBaseUrl + '/api/benchmark/GetDistrictAssessmentsAndFields';
          var promise = $http.get(url);
          return promise.then(function (response) {
            self.assessments = self.flatten(response.data.Assessments);
          });
        };
        self.flatten = function (data) {
          var out = [];
          angular.forEach(data, function (d) {
            angular.forEach(d.Fields, function (v) {
              out.push({
                AssessmentName: d.AssessmentName,
                DisplayLabel: v.DisplayLabel,
                FieldName: v.DatabaseColumn,
                LookupFieldName: v.LookupFieldName,
                AssessmentId: d.Id,
                FieldType: v.FieldType,
                RangeHigh: v.RangeHigh,
                RangeLow: v.RangeLow
              });
            });
          });
          return out;
        };
        self.SaveBenchmark = function (benchmarkRecord) {
          var paramObj = { Benchmark: benchmarkRecord };
          var url = webApiBaseUrl + '/api/benchmark/SaveDistrictBenchmark';
          var promise = $http.post(url, paramObj);
          // temporary
          return promise.then(function (response) {
          });
        };
        self.DeleteBenchmark = function (benchmarkRecord) {
          var paramObj = { Benchmark: benchmarkRecord };
          var url = webApiBaseUrl + '/api/benchmark/DeleteDistrictBenchmark';
          var promise = $http.post(url, paramObj);
          // temporary
          return promise.then(function (response) {
          });
        };
      };
      return NSBenchmarksManager;
    }
  ]).factory('DistrictStudentAttributesManager', [
    '$http',
    'webApiBaseUrl',
    function ($http, webApiBaseUrl) {
      var DistrictStudentAttributesManager = function () {
        var self = this;
        self.LoadStudentAttributes = function () {
          var url = webApiBaseUrl + '/api/districtsettings/GetStudentAttributes';
          var promise = $http.get(url);
          return promise.then(function (response) {
            self.attributes = response.data.Attributes;
          });
        };
        self.SaveAttribute = function (attribute) {
          var paramObj = { Attribute: attribute };
          var url = webApiBaseUrl + '/api/districtsettings/SaveAttribute';
          var promise = $http.post(url, paramObj);
          return promise;
        };
        self.SaveAttributeValue = function (val) {
          var paramObj = { AttributeValue: val };
          var url = webApiBaseUrl + '/api/districtsettings/SaveAttributeValue';
          var promise = $http.post(url, paramObj);
          return promise;
        };
        self.DeleteAttribute = function (attribute) {
          var paramObj = { Attribute: attribute };
          var url = webApiBaseUrl + '/api/districtsettings/DeleteAttribute';
          var promise = $http.post(url, paramObj);
          return promise;
        };
        self.DeleteAttributeValue = function (val) {
          var paramObj = { AttributeValue: val };
          var url = webApiBaseUrl + '/api/districtsettings/DeleteAttributeValue';
          var promise = $http.post(url, paramObj);
          return promise;
        };
      };
      return DistrictStudentAttributesManager;
    }
  ]).factory('DistrictYearlyAssessmentBenchmarksManager', [
    '$http',
    'webApiBaseUrl',
    function ($http, webApiBaseUrl) {
      var NSYearlyAssessmentBenchmarksManager = function () {
        var self = this;
        self.LoadData = function (assessmentId, fieldName, lookupFieldName) {
          var paramObj = {
              AssessmentId: assessmentId,
              FieldName: fieldName,
              LookupFieldName: lookupFieldName
            };
          var url = webApiBaseUrl + '/api/benchmark/GetDistrictYearlyAssessmentBenchmarks';
          var benchmarksPromise = $http.post(url, paramObj);
          return benchmarksPromise.then(function (response) {
            self.benchmarks = response.data.Benchmarks;
          });
        };
        self.LoadAssessmentFields = function () {
          var url = webApiBaseUrl + '/api/benchmark/GetDistrictYearlyAssessmentsAndFields';
          var promise = $http.get(url);
          return promise.then(function (response) {
            self.assessments = self.flatten(response.data.Assessments);
          });
        };
        self.flatten = function (data) {
          var out = [];
          angular.forEach(data, function (d) {
            angular.forEach(d.Fields, function (v) {
              out.push({
                AssessmentName: d.AssessmentName,
                DisplayLabel: v.DisplayLabel,
                FieldName: v.DatabaseColumn,
                LookupFieldName: v.LookupFieldName,
                AssessmentId: d.Id,
                FieldType: v.FieldType,
                RangeHigh: v.RangeHigh,
                RangeLow: v.RangeLow
              });
            });
          });
          return out;
        };
        self.SaveBenchmark = function (benchmarkRecord) {
          var paramObj = { Benchmark: benchmarkRecord };
          var url = webApiBaseUrl + '/api/benchmark/SaveDistrictYearlyAssessmentBenchmark';
          var promise = $http.post(url, paramObj);
          // temporary
          return promise.then(function (response) {
          });
        };
        self.DeleteBenchmark = function (benchmarkRecord) {
          var paramObj = { Benchmark: benchmarkRecord };
          var url = webApiBaseUrl + '/api/benchmark/DeleteDistrictYearlyAssessmentBenchmark';
          var promise = $http.post(url, paramObj);
          // temporary
          return promise.then(function (response) {
          });
        };
      };
      return NSYearlyAssessmentBenchmarksManager;
    }
  ]).factory('NSHFWListManager', [
    '$http',
    'webApiBaseUrl',
    function ($http, webApiBaseUrl) {
      var NSHFWListManager = function () {
        var self = this;
        self.LoadData = function (wordList, isAlphaOrder) {
          var paramObj = {
              WordList: wordList,
              IsAlphaOrder: isAlphaOrder
            };
          var url = webApiBaseUrl + '/api/districtsettings/GetHFWList';
          var promise = $http.post(url, paramObj);
          return promise;
        };
        self.saveHfw = function (word) {
          var paramObj = { Word: word };
          var url = webApiBaseUrl + '/api/districtsettings/SaveHFW';
          var promise = $http.post(url, paramObj);
          return promise;
        };
      };
      return NSHFWListManager;
    }
  ]).factory('NSInterventionListManager', [
    '$http',
    'webApiBaseUrl',
    function ($http, webApiBaseUrl) {
      var NSInterventionListManager = function () {
        var self = this;
        self.LoadData = function () {
          var url = webApiBaseUrl + '/api/districtsettings/GetInterventionList';
          var promise = $http.get(url);
          return promise;
        };
        self.saveIntervention = function (intervention) {
          var paramObj = { Intervention: intervention };
          var url = webApiBaseUrl + '/api/districtsettings/SaveIntervention';
          var promise = $http.post(url, paramObj);
          return promise;
        };
        self.deleteIntervention = function (intervention) {
          var paramObj = { Intervention: intervention };
          var url = webApiBaseUrl + '/api/districtsettings/DeleteIntervention';
          var promise = $http.post(url, paramObj);
          return promise;
        };
      };
      return NSInterventionListManager;
    }
  ]).controller('ManageInterventionTypesController', [
    'NSInterventionListManager',
    '$scope',
    'nsFilterOptionsService',
    'progressLoader',
    'nsPinesService',
    '$rootScope',
    '$bootbox',
    '$uibModal',
    function (NSInterventionListManager, $scope, nsFilterOptionsService, progressLoader, nsPinesService, $rootScope, $bootbox, $uibModal) {
      $scope.mgr = new NSInterventionListManager();
      $scope.settings = { newIntervention: {} };
      $scope.startEdit = function (rowform) {
        rowform.$show();
      };
      $scope.before = function (rowform) {
        rowform.$setSubmitted();
        if (rowform.$valid) {
          return;
        } else
          return 'At least one required field is not filled out.';
      };
      $scope.deleteIntervention = function (intervention) {
        $bootbox.confirm('Are you sure you want to delete this Intervention?  <br><br><b>NOTE:</b> You will not be able to delete it if it is in use.', function (response) {
          if (response) {
            progressLoader.start();
            progressLoader.set(50);
            $scope.mgr.deleteIntervention(intervention).then(function (response) {
              progressLoader.end();
              nsPinesService.dataDeletedSuccessfully();
              $rootScope.$broadcast('NSHTTPClear', 'Saved Successfully');
              LoadData();
            }, function (error) {
              nsPinesService.generalLoadingError();
            });
          }
        });
      };
      $scope.saveIntervention = function (intervention) {
        progressLoader.start();
        progressLoader.set(50);
        $scope.mgr.saveIntervention(intervention).then(function (response) {
          $scope.settings.newIntervention = {};
          progressLoader.end();
          nsPinesService.dataSavedSuccessfully();
          $rootScope.$broadcast('NSHTTPClear', 'Saved Successfully');
          LoadData();
        }, function (error) {
          nsPinesService.dataError();
          LoadData();
        });
      };
      var LoadData = function () {
        progressLoader.start();
        progressLoader.set(50);
        $scope.mgr.LoadData().then(function (response) {
          progressLoader.end();
          $scope.interventions = response.data.Interventions;
        });
      };
      $scope.showNewInterventionModal = function () {
        var modalInstance = $uibModal.open({
            templateUrl: 'newIntervention.html',
            scope: $scope,
            controller: function ($scope, $uibModalInstance) {
              $scope.saveNewIntervention = function (intervention) {
                if (!intervention.InterventionType || intervention.InterventionType == '') {
                  alert('The Intervention must have a name.');
                  return;
                }
                $scope.saveIntervention(intervention);
                $uibModalInstance.dismiss('cancel');
              };
              $scope.cancel = function () {
                $uibModalInstance.dismiss('cancel');
              };
            },
            size: 'md'
          });
      };
      LoadData();
    }
  ]).controller('HFWListController', [
    'NSHFWListManager',
    '$scope',
    'nsFilterOptionsService',
    'progressLoader',
    'nsPinesService',
    '$rootScope',
    '$bootbox',
    function (NSHFWListManager, $scope, nsFilterOptionsService, progressLoader, nsPinesService, $rootScope, $bootbox) {
      $scope.mgr = new NSHFWListManager();
      $scope.filterOptions = nsFilterOptionsService.options;
      $scope.startEdit = function (rowform, word) {
        word.DisplayNameTemp = word.DisplayName;
        word.IsKdgTemp = word.IsKdg;
        word.SortOrderTemp = word.SortOrder;
        word.AltOrderTemp = word.AltOrder;
        rowform.$show();
      };
      $scope.before = function (rowform) {
        rowform.$setSubmitted();
        if (rowform.$valid) {
          return;
        } else
          return 'At least one required field is not filled out.';
      };
      $scope.saveWord = function (word) {
        word.DisplayName = word.DisplayNameTemp;
        word.IsKdg = word.IsKdgTemp;
        word.SortOrder = word.SortOrderTemp;
        word.AltOrder = word.AltOrderTemp;
        progressLoader.start();
        progressLoader.set(50);
        $scope.mgr.saveHfw(word).then(function (response) {
          progressLoader.end();
          nsPinesService.dataSavedSuccessfully();
          $rootScope.$broadcast('NSHTTPClear', 'Saved Successfully');
          LoadData();
        }, function (error) {
          nsPinesService.dataError();
          LoadData();
        });
      };
      $scope.$watch('filterOptions.selectedHfwRange', function (newValue, oldValue) {
        if (newValue != null && newValue != '') {
          if (!angular.equals(newValue, oldValue)) {
            LoadData();
          }
        } else {
          $scope.words = [];
        }
      });
      $scope.$watch('filterOptions.selectedHfwSortOrder', function (newValue, oldValue) {
        if (!angular.equals(newValue, oldValue)) {
          LoadData();
        }
      });
      var LoadData = function () {
        progressLoader.start();
        progressLoader.set(50);
        if ($scope.filterOptions.selectedHfwRange != null) {
          $scope.mgr.LoadData($scope.filterOptions.selectedHfwRange, $scope.filterOptions.selectedHfwSortOrder).then(function (response) {
            progressLoader.end();
            $scope.words = response.data.Words;
          });
        }
      };
    }
  ]).controller('BenchmarkDatesController', [
    'NSBenchmarkDatesManager',
    '$scope',
    'nsFilterOptionsService',
    'progressLoader',
    'nsPinesService',
    '$rootScope',
    '$bootbox',
    function (NSBenchmarkDatesManager, $scope, nsFilterOptionsService, progressLoader, nsPinesService, $rootScope, $bootbox) {
      $scope.mgr = new NSBenchmarkDatesManager();
      $scope.filterOptions = nsFilterOptionsService.options;
      $scope.benchmarkdates = [];
      $scope.settings = { newItem: null };
      $scope.addNew = function () {
        $scope.settings.newItem = {
          TestLevelPeriodIDTemp: null,
          DueDateTemp: moment().toDate(),
          StartDateTemp: moment().toDate(),
          SchoolStartYear: $scope.filterOptions.selectedSchoolYear.id
        };
      };
      $scope.startEdit = function (rowform, period) {
        period.TestLevelPeriodIDTemp = period.TestLevelPeriodID;
        period.StartDateTemp = moment(period.StartDate).toDate();
        period.DueDateTemp = moment(period.DueDate).toDate();
        period.HexTemp = period.Hex;
        period.NotesTemp = period.Notes;
        period.IsSupplementalTemp = period.IsSupplemental;
        rowform.$show();
      };
      $scope.before = function (rowform) {
        rowform.$setSubmitted();
        if (rowform.$valid) {
          return;
        } else
          return 'At least one required field is not filled out.';
      };
      $scope.deleteBenchmark = function (benchmark) {
        $bootbox.confirm('Are you sure you want to delete this benchmark date?  You will not be able to delete it if it is in use.', function (response) {
          if (response) {
            progressLoader.start();
            progressLoader.set(50);
            $scope.mgr.deleteBenchmark(benchmark).then(function (response) {
              progressLoader.end();
              nsPinesService.dataDeletedSuccessfully();
              $rootScope.$broadcast('NSHTTPClear', 'Saved Successfully');
              LoadData();
            }, function (error) {
              nsPinesService.generalLoadingError();
            });
          }
        });
      };
      $scope.saveBenchmarkPeriod = function (period) {
        period.TestLevelPeriodID = period.TestLevelPeriodIDTemp;
        period.StartDate = moment(period.StartDateTemp).toDate();
        period.DueDate = moment(period.DueDateTemp).toDate();
        period.Hex = period.HexTemp;
        period.Notes = period.NotesTemp;
        period.IsSupplemental = period.IsSupplementalTemp;
        progressLoader.start();
        progressLoader.set(50);
        $scope.mgr.saveBenchmarkPeriod(period).then(function (response) {
          progressLoader.end();
          nsPinesService.dataSavedSuccessfully();
          $scope.settings.newItem = null;
          $rootScope.$broadcast('NSHTTPClear', 'Saved Successfully');
          LoadData();
        }, function (error) {
          nsPinesService.dataError();
          LoadData();
        });
      };
      $scope.$watch('filterOptions.selectedSchoolYear.id', function (newValue, oldValue) {
        if (!angular.equals(newValue, oldValue)) {
          LoadData();
        }
      });
      var LoadData = function () {
        if ($scope.filterOptions.selectedSchoolYear != null) {
          $scope.mgr.LoadData($scope.filterOptions.selectedSchoolYear.id).then(function (response) {
            $scope.benchmarkdates = response.data.DueDates;
            for (var i = 0; i < $scope.benchmarkdates.length; i++) {
              $scope.benchmarkdates[i].TestLevelPeriodID = $scope.benchmarkdates[i].TestLevelPeriodID + '';
              $scope.benchmarkdates[i].StartDate = moment($scope.benchmarkdates[i].StartDate).toDate();
              $scope.benchmarkdates[i].DueDate = moment($scope.benchmarkdates[i].DueDate).toDate();
            }
          });
        }
      };
    }
  ]).factory('NSBenchmarkDatesManager', [
    '$http',
    'webApiBaseUrl',
    function ($http, webApiBaseUrl) {
      var NSBenchmarkDatesManager = function () {
        var self = this;
        self.LoadData = function (schoolYear) {
          var paramObj = { Id: schoolYear };
          var url = webApiBaseUrl + '/api/districtsettings/GetBenchmarkDatesForSchoolYear';
          var benchmarksPromise = $http.post(url, paramObj);
          return benchmarksPromise;
        };
        self.saveBenchmarkPeriod = function (period) {
          var paramObj = { Tdd: period };
          var url = webApiBaseUrl + '/api/districtsettings/SaveTestDueDate';
          var promise = $http.post(url, paramObj);
          return promise;
        };
        self.deleteBenchmark = function (period) {
          var paramObj = { Tdd: period };
          var url = webApiBaseUrl + '/api/districtsettings/DeleteBenchmarkDate';
          var promise = $http.post(url, paramObj);
          return promise;
        };
      };
      return NSBenchmarkDatesManager;
    }
  ]);
}());
(function () {
  'use strict';
  angular.module('hfwReportsModule', []).controller('HfwStudentDetailReportController', [
    '$scope',
    'InterventionGroup',
    '$q',
    '$http',
    'nsPinesService',
    '$location',
    '$routeParams',
    'hfwReportFactory',
    'nsFilterOptionsService',
    '$timeout',
    '$bootbox',
    'webApiBaseUrl',
    'FileSaver',
    'spinnerService',
    function ($scope, InterventionGroup, $q, $http, nsPinesService, $location, $routeParams, hfwReportFactory, nsFilterOptionsService, $timeout, $bootbox, webApiBaseUrl, FileSaver, spinnerService) {
      var LoadData = function () {
        $timeout(function () {
          spinnerService.show('tableSpinner');
        });
        $scope.factory.LoadReportDetailData($scope.filterOptions.selectedSectionStudent.id, $scope.filterOptions.selectedHfwMultiRange, $scope.filterOptions.selectedHfwSortOrder).then(function () {
        }).finally(function () {
          spinnerService.hide('tableSpinner');
        });
      };
      $scope.factory = new hfwReportFactory();
      $scope.filterOptions = nsFilterOptionsService.options;
      $scope.settings = {
        printMode: false,
        printInProgress: false
      };
      if ($location.absUrl().indexOf('printmode=') >= 0) {
        $scope.settings.printMode = true;
      }
      $scope.$watch('filterOptions.selectedSectionStudent.id', function (newVal, oldVal) {
        if (newVal !== oldVal) {
          LoadData();
        }
      });
      $scope.setHfwBackgroundColor = function (wordRow) {
        if (wordRow.Read && wordRow.Write) {
          return 'yellowSelectedHfwRow';  // TODO: filterOptions.hfwBG
        }
      };
      $scope.$watch('filterOptions.selectedHfwSortOrder', function (newVal, oldVal) {
        if (newVal !== oldVal) {
          LoadData();
        }
      });
      $scope.$watch('filterOptions.selectedHfwMultiRange', function (newVal, oldVal) {
        if (newVal !== oldVal) {
          LoadData();
        }
      });
      $scope.print = function () {
        if ($scope.settings.printInProgress) {
          $bootbox.alert('Please wait... another print job is already in progress.');
          return;
        }
        $scope.settings.printInProgress = true;
        var notice = nsPinesService.startDynamic();
        var returnObj = {
            PrintLandscape: false,
            PrintMultiPage: true,
            Url: $location.absUrl()
          };
        var printMethod = 'PrintPage';
        if (webApiBaseUrl.indexOf('localhost') > 0) {
          printMethod = 'PrintPageLocal';
        }
        $http.post(webApiBaseUrl + '/api/Print/' + printMethod, returnObj, {
          responseType: 'arraybuffer',
          headers: { accept: 'application/pdf' }
        }).then(function (data) {
          var blob = new Blob([data.data], { type: 'application/pdf' });
          FileSaver.saveAs(blob, 'NorthStarPrint.pdf');
        }).finally(function () {
          $scope.settings.printInProgress = false;
          nsPinesService.endDynamic(notice);
        });
      };
    }
  ]).controller('HfwStudentMissingWordsReportController', [
    '$scope',
    'InterventionGroup',
    '$q',
    '$http',
    'nsPinesService',
    '$location',
    '$routeParams',
    'hfwReportFactory',
    'nsFilterOptionsService',
    function ($scope, InterventionGroup, $q, $http, nsPinesService, $location, $routeParams, hfwReportFactory, nsFilterOptionsService) {
      $scope.factory = new hfwReportFactory();
      $scope.filterOptions = nsFilterOptionsService.options;
      $scope.$watch('filterOptions.selectedSectionStudent.id', function (newVal, oldVal) {
        if (newVal !== oldVal) {
          $scope.factory.LoadMissingWordsReportData($scope.filterOptions.selectedSectionStudent.id, $scope.filterOptions.selectedHfwMultiRange, $scope.filterOptions.selectedHfwSortOrder);
        }
      });
      $scope.$watch('filterOptions.selectedHfwSortOrder', function (newVal, oldVal) {
        if (newVal !== oldVal) {
          $scope.factory.LoadMissingWordsReportData($scope.filterOptions.selectedSectionStudent.id, $scope.filterOptions.selectedHfwMultiRange, $scope.filterOptions.selectedHfwSortOrder);
        }
      });
      $scope.$watch('filterOptions.selectedHfwMultiRange', function (newVal, oldVal) {
        if (newVal !== oldVal) {
          $scope.factory.LoadMissingWordsReportData($scope.filterOptions.selectedSectionStudent.id, $scope.filterOptions.selectedHfwMultiRange, $scope.filterOptions.selectedHfwSortOrder);
        }
      });
    }
  ]).factory('hfwReportFactory', [
    '$http',
    'webApiBaseUrl',
    function ($http, webApiBaseUrl) {
      var hfwReportFactory = function () {
        this.LoadReportDetailData = function (studentId, selectedRanges, sortOrder) {
          // set default
          if (selectedRanges == null) {
            selectedRanges = [{
                id: 1,
                text: '1-100'
              }];
          }
          var url = webApiBaseUrl + '/api/sectionreport/GetHFWDetailReport/';
          var paramObj = {
              StudentId: studentId,
              SelectedRanges: selectedRanges,
              HfwSortOrder: sortOrder
            };
          var promise = $http.post(url, paramObj);
          var self = this;
          return promise.then(function (response) {
            angular.extend(self, response.data);
          });
        };
        this.LoadMissingWordsReportData = function (studentId, selectedRanges, sortOrder) {
          // set default
          if (selectedRanges == null) {
            selectedRanges = [{
                id: 1,
                text: '1-100'
              }];
          }
          var url = webApiBaseUrl + '/api/sectionreport/GetHFWMissingWordsReport/';
          var paramObj = {
              StudentId: studentId,
              SelectedRanges: selectedRanges,
              HfwSortOrder: sortOrder
            };
          var promise = $http.post(url, paramObj);
          var self = this;
          return promise.then(function (response) {
            angular.extend(self, response.data);
          });
        };
      };
      return hfwReportFactory;
    }
  ]);
  ;
}());
(function () {
  'use strict';
  angular.module('filterOptionsModule', []).directive('nsFilterOptionsContainerDirective', [
    '$routeParams',
    '$compile',
    '$templateCache',
    '$http',
    'nsFilterOptionsService',
    '$filter',
    'nsSelect2RemoteOptions',
    function ($routeParams, $compile, $templateCache, $http, nsFilterOptionsService, $filter, nsSelect2RemoteOptions) {
      return {
        restrict: 'E',
        templateUrl: 'templates/global-filter-options.html',
        scope: {
          schoolYearEnabled: '=',
          schoolEnabled: '=',
          gradeEnabled: '=',
          teacherEnabled: '=',
          interventionistEnabled: '=',
          sectionEnabled: '=',
          interventionGroupEnabled: '=',
          sectionStudentEnabled: '=',
          interventionStudentEnabled: '=',
          benchmarkDateEnabled: '=',
          verticalMode: '=',
          schoolYearRequired: '=',
          benchmarkDateRequired: '=',
          schoolRequired: '=',
          hrsFormEnabled: '=',
          staffQuickSearchEnabled: '=',
          showHr: '=',
          studentQuickSearchEnabled: '=',
          studentDetailedQuickSearchEnabled: '=',
          quickSearchCallback: '=',
          stintEnabled: '=',
          teamMeetingEnabled: '=',
          teamMeetingStaffEnabled: '=',
          hfwRangeEnabled: '=',
          hfwSortOrderEnabled: '=',
          hfwMultiRangeEnabled: '=',
          stateTestEnabled: '=',
          benchmarkTestEnabled: '=',
          interventionTestEnabled: '=',
          staffSearchCustomLabel: '='
        },
        link: function (scope, element, attr) {
          scope.filterOptions = nsFilterOptionsService.options;
          nsFilterOptionsService.options.schoolYearEnabled = scope.schoolYearEnabled;
          nsFilterOptionsService.options.benchmarkDateEnabled = scope.benchmarkDateEnabled;
          nsFilterOptionsService.options.schoolEnabled = scope.schoolEnabled;
          nsFilterOptionsService.options.gradeEnabled = scope.gradeEnabled;
          nsFilterOptionsService.options.teacherEnabled = scope.teacherEnabled;
          nsFilterOptionsService.options.interventionistEnabled = scope.interventionistEnabled;
          nsFilterOptionsService.options.sectionEnabled = scope.sectionEnabled;
          nsFilterOptionsService.options.interventionGroupEnabled = scope.interventionGroupEnabled;
          nsFilterOptionsService.options.sectionStudentEnabled = scope.sectionStudentEnabled;
          nsFilterOptionsService.options.interventionStudentEnabled = scope.interventionStudentEnabled;
          nsFilterOptionsService.options.hrsFormEnabled = scope.hrsFormEnabled;
          nsFilterOptionsService.options.staffQuickSearchEnabled = scope.staffQuickSearchEnabled;
          nsFilterOptionsService.options.showHr = scope.showHr;
          nsFilterOptionsService.options.studentQuickSearchEnabled = scope.studentQuickSearchEnabled;
          nsFilterOptionsService.options.studentDetailedQuickSearchEnabled = scope.studentDetailedQuickSearchEnabled;
          nsFilterOptionsService.options.stintEnabled = scope.stintEnabled;
          nsFilterOptionsService.options.teamMeetingEnabled = scope.teamMeetingEnabled;
          nsFilterOptionsService.options.teamMeetingStaffEnabled = scope.teamMeetingStaffEnabled;
          nsFilterOptionsService.options.hfwRangeEnabled = scope.hfwRangeEnabled;
          nsFilterOptionsService.options.hfwSortOrderEnabled = scope.hfwSortOrderEnabled;
          nsFilterOptionsService.options.hfwMultiRangeEnabled = scope.hfwMultiRangeEnabled;
          nsFilterOptionsService.options.stateTestEnabled = scope.stateTestEnabled;
          nsFilterOptionsService.options.benchmarkTestEnabled = scope.benchmarkTestEnabled;
          nsFilterOptionsService.options.interventionTestEnabled = scope.interventionTestEnabled;
          // determine which method to call
          if (angular.isDefined($routeParams.stint)) {
            nsFilterOptionsService.stintOverride();
          } else if (angular.isDefined($routeParams.teamMeeting)) {
            nsFilterOptionsService.teamMeetingOverride();
          } else {
            nsFilterOptionsService.loadOptions();
          }
          scope.HfwMultiRangeRemoteOptions = nsSelect2RemoteOptions.HfwMultiRangeRemoteOptions;
          scope.StaffQuickSearchRemoteOptions = nsSelect2RemoteOptions.StaffQuickSearchRemoteOptions;
          scope.StudentQuickSearchRemoteOptions = nsSelect2RemoteOptions.StudentQuickSearchRemoteOptions;
          scope.StudentDetailedQuickSearchRemoteOptions = nsSelect2RemoteOptions.StudentDetailedQuickSearchRemoteOptions;
          scope.qsCallBack = function () {
            if (angular.isDefined(scope.quickSearchCallback)) {
              scope.quickSearchCallback();
            } else {
              return;
            }
          };
          scope.changeSchool = function () {
            nsFilterOptionsService.changeSchool();
          };
          scope.changeSchoolYear = function () {
            nsFilterOptionsService.changeSchoolYear();
          };
          scope.changeGrade = function () {
            nsFilterOptionsService.changeGrade();
          };
          scope.changeTeacher = function () {
            nsFilterOptionsService.changeTeacher();
          };
          scope.changeInterventionist = function () {
            nsFilterOptionsService.changeInterventionist();
          };
          scope.changeTeacher = function () {
            nsFilterOptionsService.changeTeacher();
          };
          scope.changeSection = function () {
            nsFilterOptionsService.changeSection();
          };
          scope.changeSectionStudent = function () {
            nsFilterOptionsService.changeSectionStudent();
          };
          scope.changeInterventionStudent = function () {
            nsFilterOptionsService.changeInterventionStudent();
          };
          scope.changeHfwMultiRange = function () {
            nsFilterOptionsService.changeHfwMultiRange();
          };
          scope.changeHfwSortOrder = function () {
            nsFilterOptionsService.changeHfwSortOrder();
          };
          scope.changeTeamMeeting = function () {
            nsFilterOptionsService.changeTeamMeeting();
          };
          scope.changeInterventionGroup = function () {
            nsFilterOptionsService.changeInterventionGroup();
          };
        }
      };
    }
  ]).service('nsFilterOptionsService', [
    '$http',
    '$location',
    '$q',
    '$routeParams',
    'webApiBaseUrl',
    '$rootScope',
    function ($http, $location, $q, $routeParams, webApiBaseUrl, $rootScope) {
      var self = this;
      self.initialLoadComplete = false;
      self.normalizeParameter = function (parameter) {
        if (parameter !== null && angular.isDefined(parameter)) {
          return parameter.id;
        } else {
          return -1;
        }
      };
      self.createReturnObject = function (options) {
        var returnObject = {
            SchoolId: self.normalizeParameter(options.selectedSchool),
            SchoolEnabled: options.schoolEnabled,
            GradeId: self.normalizeParameter(options.selectedGrade),
            GradeEnabled: options.gradeEnabled,
            TeacherId: self.normalizeParameter(options.selectedTeacher),
            TeacherEnabled: options.teacherEnabled,
            InterventionistId: self.normalizeParameter(options.selectedInterventionist),
            InterventionistEnabled: options.interventionistEnabled,
            SectionId: self.normalizeParameter(options.selectedSection),
            SectionEnabled: options.sectionEnabled,
            InterventionGroupId: self.normalizeParameter(options.selectedInterventionGroup),
            InterventionGroupEnabled: options.interventionGroupEnabled,
            SectionStudentId: self.normalizeParameter(options.selectedSectionStudent),
            SectionStudentEnabled: options.sectionStudentEnabled,
            InterventionStudentId: self.normalizeParameter(options.selectedInterventionStudent),
            InterventionStudentEnabled: options.interventionStudentEnabled,
            SchoolYear: self.normalizeParameter(options.selectedSchoolYear),
            SchoolYearEnabled: options.schoolYearEnabled,
            BenchmarkDateId: self.normalizeParameter(options.selectedBenchmarkDate),
            BenchmarkDateEnabled: options.benchmarkDateEnabled,
            HRSFormEnabled: options.hrsFormEnabled,
            StintId: self.normalizeParameter(options.selectedStint),
            StintEnabled: options.stintEnabled,
            TeamMeetingId: self.normalizeParameter(options.selectedTeamMeeting),
            TeamMeetingEnabled: options.teamMeetingEnabled,
            TeamMeetingStaffId: self.normalizeParameter(options.selectedTeamMeetingStaff),
            TeamMeetingStaffEnabled: options.teamMeetingStaffEnabled,
            HFWRangeEnabled: options.hfwRangeEnabled,
            HFWRange: options.selectedHfwRange,
            HFWSortOrderEnabled: options.hfwSortOrderEnabled,
            HFWMultiRangeEnabled: options.hfwMultiRangeEnabled,
            HFWSortOrder: options.selectedHfwSortOrder,
            HFWMultiRange: options.selectedHfwMultiRange,
            StateTestEnabled: options.stateTestEnabled,
            BenchmarkTestEnabled: options.benchmarkTestEnabled,
            InterventionTestEnabled: options.interventionTestEnabled
          };
        return returnObject;
      };
      self.options = {};
      self.options.teacherEnabled = false;
      self.options.teachers = [];
      self.options.gradeEnabled = false;
      self.options.grades = [];
      self.options.sectionEnabled = false;
      self.options.sections = [];
      self.options.studentEnabled = false;
      self.options.sectionStudents = [];
      self.options.interventionStudents = [];
      self.options.schoolEnabled = false;
      self.options.schools = [];
      self.options.schoolYearEnabled = false;
      self.options.schoolYears = [];
      self.options.benchmarkDateEnabled = false;
      self.options.benchmarkDates = [];
      self.options.interventionGroupEnabled = false;
      self.options.interventionGroups = [];
      self.options.interventionistEnabled = false;
      self.options.interventionists = [];
      self.options.hrsFormEnabled = false;
      self.options.hrsForms = [];
      self.options.hfwOrders = [
        'Alphabetic',
        'Teaching'
      ];
      self.options.stints = [];
      self.options.stateTests = [];
      self.options.benchmarkTests = [];
      self.options.interventionTests = [];
      self.options.stintEnabled = false;
      self.options.teamMeetings = [];
      self.options.teamMeetingEnabled = false;
      self.options.teamMeetingStaffs = [];
      self.options.teamMeetingStaffEnabled = false;
      self.options.hfwRangeEnabled = false;
      self.options.hfwSortOrderEnabled = false;
      self.options.stateTestEnabled = false;
      self.options.benchmarkTestEnabled = false;
      self.options.interventionTestEnabled = false;
      self.options.selectedGrade = null;
      self.options.selectedTeacher = null;
      self.options.selectedInterventionist = null;
      self.options.selectedSection = null;
      self.options.selectedInterventionGroup = null;
      self.options.selectedSectionStudent = null;
      self.options.selectedInterventionStudent = null;
      self.options.selectedSchool = null;
      self.options.selectedSchoolYear = null;
      self.options.selectedBenchmarkDate = null;
      self.options.selectedHrsForm = null;
      self.options.selectedStint = null;
      self.options.selectedTeamMeeting = null;
      self.options.selectedTeamMeetingStaff = null;
      self.options.selectedHfwRange = null;
      self.options.selectedHfwMultiRange = null;
      self.options.selectedHfwSortOrder = null;
      self.options.selectedStateTest = null;
      self.options.selectedBenchmarkTest = null;
      self.options.selectedInterventionTest = null;
      self.getBenchmarkDateById = function (id) {
        var theBenchmarkDate = null;
        for (var i = 0; i < self.options.benchmarkDates.length; i++) {
          if (self.options.benchmarkDates[i].id == id) {
            self.options.benchmarkDates[i].active = true;
            theBenchmarkDate = self.options.benchmarkDates[i];
          } else {
            self.options.benchmarkDates[i].active = false;
          }
        }
        return theBenchmarkDate;
      };
      self.loadOptions = function () {
        var returnObject = self.createReturnObject(self.options);
        returnObject.ChangeType = 'initial';
        returnObject.SchoolEnabled = true;
        returnObject.SchoolYearEnabled = true;
        returnObject.BenchmarkDateEnabled = true;
        return $http.post(webApiBaseUrl + '/api/assessment/GetUpdatedFilterOptions', returnObject).then(function (response) {
          reloadSchoolYears(response);
          reloadSchools(response);
          reloadGrades(response);
          reloadTeachers(response);
          reloadSections(response);
          reloadSectionStudents(response);
          reloadInterventionists(response);
          reloadInterventionGroups(response);
          reloadInterventionStudents(response);
          reloadStints(response);
          reloadBenchmarkDates(response);
          reloadTeamMeetings(response);
          reloadTeamMeetingStaffs(response);
          loadHrsForms(response);
          loadHfwRanges(response);
          loadHfwSortOrder(response);
          loadStateTests(response);
          loadInterventionTests(response);
          loadBenchmarkTests(response);
          // TODO: reload all them
          //self.initialLoadComplete = true;
          $rootScope.$broadcast('NSInitialLoadComplete', true);  // Read any passed in options from RouteParams and set them
                                                                 //self.setUrlOverrides();
        });
      };
      var loadHrsForms = function (response) {
        if (self.options.hrsFormEnabled) {
          self.options.hrsForms.splice(0, self.options.hrsForms.length);
          self.options.hrsForms.push.apply(self.options.hrsForms, response.data.HRSForms);
        }
      };
      var loadStateTests = function (response) {
        if (self.options.stateTestEnabled) {
          self.options.stateTests.splice(0, self.options.stateTests.length);
          self.options.stateTests.push.apply(self.options.stateTests, response.data.StateTests);
        }
      };
      var loadBenchmarkTests = function (response) {
        if (self.options.benchmarkTestEnabled) {
          self.options.benchmarkTests.splice(0, self.options.benchmarkTests.length);
          self.options.benchmarkTests.push.apply(self.options.benchmarkTests, response.data.BenchmarkTests);
        }
      };
      var loadInterventionTests = function (response) {
        if (self.options.interventionTestEnabled) {
          self.options.interventionTests.splice(0, self.options.interventionTests.length);
          self.options.interventionTests.push.apply(self.options.interventionTests, response.data.InterventionTests);
        }
      };
      var loadHfwRanges = function (response) {
        if (self.options.hfwMultiRangeEnabled) {
          self.options.selectedHfwMultiRange = response.data.SelectedHFWMultiRange;
        }
      };
      var loadHfwSortOrder = function (response) {
        if (self.options.hfwSortOrderEnabled) {
          self.options.selectedHfwSortOrder = getSelectedStringFromCollection(response.data.SelectedHFWSortOrder, options.hfwOrders);
        }
      };
      self.studentOverride = function () {
      };
      self.stintOverride = function () {
        // if this is a stint override, create returnobject based on URL params
        var paramObj = {
            SchoolId: $routeParams.school,
            SchoolEnabled: true,
            GradeId: -1,
            GradeEnabled: false,
            TeacherId: -1,
            TeacherEnabled: false,
            InterventionistId: $routeParams.interventionist,
            InterventionistEnabled: true,
            SectionId: -1,
            SectionEnabled: false,
            InterventionGroupId: $routeParams.interventiongroup,
            InterventionGroupEnabled: true,
            InterventionStudentId: $routeParams.student,
            InterventionStudentEnabled: true,
            SectionStudentId: -1,
            SectionStudentEnabled: false,
            SchoolYear: $routeParams.schoolyear,
            SchoolYearEnabled: true,
            BenchmarkDateId: -1,
            BenchmarkDateEnabled: false,
            HRSFormEnabled: false,
            StintId: $routeParams.stint,
            StintEnabled: true,
            TeamMeetingId: -1,
            TeamMeetingEnabled: false,
            TeamMeetingStaffId: -1,
            TeamMeetingStaffEnabled: false
          };
        paramObj.ChangeType = 'stintOverride';
        return $http.post(webApiBaseUrl + '/api/assessment/GetUpdatedFilterOptions', paramObj).then(function (response) {
          reloadSchoolYears(response);
          reloadSchools(response);
          reloadInterventionists(response);
          reloadInterventionGroups(response);
          reloadInterventionStudents(response);
          reloadStints(response);
          // TODO: reload all them
          self.initialLoadComplete = true;
        });
      };
      self.teamMeetingOverride = function () {
        // if this is a teammeeting override, create returnobject based on URL params
        var paramObj = {
            SchoolId: -1,
            SchoolEnabled: true,
            GradeId: -1,
            GradeEnabled: false,
            TeacherId: -1,
            TeacherEnabled: false,
            InterventionistId: -1,
            InterventionistEnabled: true,
            SectionId: -1,
            SectionEnabled: false,
            InterventionGroupId: -1,
            InterventionGroupEnabled: true,
            InterventionStudentId: -1,
            InterventionStudentEnabled: true,
            SectionStudentId: -1,
            SectionStudentEnabled: false,
            SchoolYear: $routeParams.schoolyear,
            SchoolYearEnabled: true,
            BenchmarkDateId: -1,
            BenchmarkDateEnabled: false,
            HRSFormEnabled: false,
            StintId: $routeParams.stint,
            StintEnabled: true,
            TeamMeetingId: $routeParams.teamMeeting,
            TeamMeetingEnabled: true,
            TeamMeetingStaffId: -1,
            TeamMeetingStaffEnabled: true
          };
        paramObj.ChangeType = 'teammeeting';
        return $http.post(webApiBaseUrl + '/api/assessment/GetUpdatedFilterOptions', paramObj).then(function (response) {
          reloadTeamMeetingStaffs(response);
          // TODO: reload all them
          self.initialLoadComplete = true;
        });
      };
      //self.schoolYearOverride = function () {
      //    //schoolyear
      //    if (angular.isDefined($location.$$search.SchoolYear) && !isNaN($location.$$search.SchoolYear)) {
      //        for (var i = 0; i < self.options.schoolYears.length; i++) {
      //            if (self.options.schoolYears[i].SchoolStartYear == $location.$$search.SchoolYear) {
      //                self.options.selectedSchoolYear = self.options.schoolYears[i];
      //                return self.changeSchoolYear().then(function (response) { self.schoolOverride() });
      //            }
      //        }
      //    }
      //    return $q.resolve(1);
      //};
      //self.schoolOverride = function () {
      //    //school
      //    if (angular.isDefined($location.$$search.SchoolId) && !isNaN($location.$$search.SchoolId)) {
      //        for (var i = 0; i < self.options.schools.length; i++) {
      //            if (self.options.schools[i].id == $location.$$search.SchoolId) {
      //                self.options.selectedSchool = self.options.schools[i];
      //                return self.changeSchool().then(function (response) { self.gradeOverride() });
      //            }
      //        }
      //    }
      //    return $q.resolve(1);
      //};
      //self.gradeOverride = function () {
      //    //grade
      //    if (angular.isDefined($location.$$search.GradeId) && !isNaN($location.$$search.GradeId)) {
      //        for (var i = 0; i < self.options.grades.length; i++) {
      //            if (self.options.grades[i].id == $location.$$search.GradeId) {
      //                self.options.selectedGrade = self.options.grades[i];
      //                return self.changeGrade().then(function (response) { self.teacherOverride() });
      //            }
      //        }
      //    }
      //    return $q.resolve(1);
      //};
      //self.teacherOverride = function () {
      //    //teacher
      //    if (angular.isDefined($location.$$search.TeacherId) && !isNaN($location.$$search.TeacherId)) {
      //        for (var i = 0; i < self.options.teachers.length; i++) {
      //            if (self.options.teachers[i].id == $location.$$search.TeacherId) {
      //                self.options.selectedTeacher = self.options.teachers[i];
      //                return self.changeTeacher().then(function (response) { self.sectionOverride() });
      //            }
      //        }
      //    }
      //    return $q.resolve(1);
      //};
      //self.sectionOverride = function () {
      //    //section
      //    if (angular.isDefined($location.$$search.SectionId) && !isNaN($location.$$search.SectionId)) {
      //        for (var i = 0; i < self.options.sections.length; i++) {
      //            if (self.options.sections[i].id == $location.$$search.SectionId) {
      //                self.options.selectedSection = self.options.sections[i];
      //                return self.changeSection().then(function (response) { self.studentOverride() });
      //            }
      //        }
      //    }
      //    return $q.resolve(1);
      //};
      //self.studentOverride = function () {
      //    //student
      //    if (angular.isDefined($location.$$search.StudentId) && !isNaN($location.$$search.StudentId)) {
      //        for (var i = 0; i < self.options.students.length; i++) {
      //            if (self.options.students[i].id == $location.$$search.StudentId) {
      //                self.options.selectedStudent = self.options.students[i];
      //                return $q.resolve();
      //            }
      //        }
      //    }
      //    return $q.resolve(1);
      //};
      //self.setUrlOverrides = function () {
      //    // start with the chained ones
      //    self.schoolYearOverride().then(
      //        function () { },
      //        function (error) { alert('something bad'); });
      //    // now do the unchained ones
      //    // list of tests, etc
      //}
      //self.loadOptions(self.options);
      self.loadSchoolChange = function (options) {
        var returnObject = self.createReturnObject(options);
        returnObject.ChangeType = 'school';
        return $http.post(webApiBaseUrl + '/api/assessment/GetUpdatedFilterOptions', returnObject).then(function (response) {
          reloadGrades(response);
          reloadTeachers(response);
          reloadInterventionists(response);
          reloadSections(response);
          reloadInterventionGroups(response);
          reloadSectionStudents(response);
          reloadInterventionStudents(response);
          $rootScope.$broadcast('NSSchoolOptionsUpdated', true);
          return response.data;
        });
      };
      self.loadGradeChange = function (options) {
        var returnObject = self.createReturnObject(options);
        returnObject.ChangeType = 'grade';
        return $http.post(webApiBaseUrl + '/api/assessment/GetUpdatedFilterOptions', returnObject).then(function (response) {
          reloadTeachers(response);
          reloadSections(response);
          reloadSectionStudents(response);
          $rootScope.$broadcast('NSGradeOptionsUpdated', true);
          return response.data;
        });
      };
      self.loadTeacherChange = function (options) {
        var returnObject = self.createReturnObject(options);
        returnObject.ChangeType = 'teacher';
        return $http.post(webApiBaseUrl + '/api/assessment/GetUpdatedFilterOptions', returnObject).then(function (response) {
          reloadSections(response);
          reloadSectionStudents(response);
          // TODO: determine if this is necessary, and correct
          options.selectedGrade = getSelectedItemFromCollection(response.data.SelectedGrade, options.grades);
          $rootScope.$broadcast('NSTeacherOptionsUpdated', true);
          return response.data;
        });
      };
      self.loadInterventionistChange = function (options) {
        var returnObject = self.createReturnObject(options);
        returnObject.ChangeType = 'interventionist';
        return $http.post(webApiBaseUrl + '/api/assessment/GetUpdatedFilterOptions', returnObject).then(function (response) {
          reloadInterventionGroups(response);
          reloadInterventionStudents(response);
          $rootScope.$broadcast('NSInterventionistOptionsUpdated', true);
          return response.data;
        });
      };
      self.loadSectionChange = function (options) {
        var returnObject = self.createReturnObject(options);
        returnObject.ChangeType = 'section';
        return $http.post(webApiBaseUrl + '/api/assessment/GetUpdatedFilterOptions', returnObject).then(function (response) {
          reloadSectionStudents(response);
          $rootScope.$broadcast('NSSectionOptionsUpdated', true);
          return response.data;
        });
      };
      self.loadSectionStudentChange = function (options) {
        var returnObject = self.createReturnObject(options);
        returnObject.ChangeType = 'sectionstudent';
        return $http.post(webApiBaseUrl + '/api/assessment/GetUpdatedFilterOptions', returnObject).then(function (response) {
          $rootScope.$broadcast('NSSectionStudentOptionsUpdated', true);
          return response.data;
        });
      };
      self.loadChangeHfwMultiRange = function (options) {
        var returnObject = self.createReturnObject(options);
        returnObject.ChangeType = 'hfwmultirange';
        return $http.post(webApiBaseUrl + '/api/assessment/GetUpdatedFilterOptions', returnObject).then(function (response) {
          $rootScope.$broadcast('NSHfwMultiRangeOptionsUpdated', true);
          return response.data;
        });
      };
      self.loadChangeHfwSortOrder = function (options) {
        var returnObject = self.createReturnObject(options);
        returnObject.ChangeType = 'hfwsortorder';
        return $http.post(webApiBaseUrl + '/api/assessment/GetUpdatedFilterOptions', returnObject).then(function (response) {
          $rootScope.$broadcast('NSHfwSortOrderOptionsUpdated', true);
          return response.data;
        });
      };
      self.loadInterventionStudentChange = function (options) {
        // only do anything for studentchange if loading stints
        if (options.stintEnabled) {
          var returnObject = self.createReturnObject(options);
          returnObject.ChangeType = 'interventionstudent';
          return $http.post(webApiBaseUrl + '/api/assessment/GetUpdatedFilterOptions', returnObject).then(function (response) {
            reloadStints(response);
            $rootScope.$broadcast('NSInterventionStudentOptionsUpdated', true);
            return response.data;
          });
        }
      };
      self.loadTeamMeetingChange = function (options) {
        // only do anything for studentchange if loading stints
        var returnObject = self.createReturnObject(options);
        returnObject.ChangeType = 'teammeeting';
        return $http.post(webApiBaseUrl + '/api/assessment/GetUpdatedFilterOptions', returnObject).then(function (response) {
          reloadTeamMeetingStaffs(response);
          $rootScope.$broadcast('NSTeamMeetingOptionsUpdated', true);
          return response.data;
        });
      };
      self.loadBenchmarkDateChange = function (options) {
        var returnObject = self.createReturnObject(options);
        returnObject.ChangeType = 'benchmarkdate';
        // nothing to update, this is just to save the setting
        return $http.post(webApiBaseUrl + '/api/assessment/GetUpdatedFilterOptions', returnObject).then(function (response) {
          $rootScope.$broadcast('NSBenchmarkDateOptionsUpdated', true);
          return response.data;
        });
      };
      self.loadInterventionGroupChange = function (options) {
        var returnObject = self.createReturnObject(options);
        returnObject.ChangeType = 'interventiongroup';
        return $http.post(webApiBaseUrl + '/api/assessment/GetUpdatedFilterOptions', returnObject).then(function (response) {
          reloadInterventionStudents(response);
          reloadStints(response);
          $rootScope.$broadcast('NSInterventionGroupOptionsUpdated', true);
          return response.data;
        });
      };
      self.loadSchoolYearChange = function (options) {
        var returnObject = self.createReturnObject(options);
        returnObject.ChangeType = 'schoolyear';
        return $http.post(webApiBaseUrl + '/api/assessment/GetUpdatedFilterOptions', returnObject).then(function (response) {
          reloadGrades(response);
          reloadBenchmarkDates(response);
          reloadTeachers(response);
          reloadInterventionists(response);
          reloadInterventionGroups(response);
          reloadInterventionStudents(response);
          reloadStints(response);
          reloadSections(response);
          reloadSectionStudents(response);
          reloadTeamMeetings(response);
          reloadTeamMeetingStaffs(response);
          $rootScope.$broadcast('NSSchoolYearOptionsUpdated', true);
          // check for routeparam with benchmarkdateid
          if (typeof $routeParams.benchmarkDateId !== 'undefined' && self.options.selectedBenchmarkDate.id !== null && self.options.selectedBenchmarkDate.id !== -1) {
            var index = $location.path().lastIndexOf('/');
            $location.path($location.path().substring(0, index) + '/' + self.options.selectedBenchmarkDate.id);
          }
          return response.data;
        });
      };
      function reloadBenchmarkDates(response) {
        self.options.benchmarkDates.splice(0, self.options.benchmarkDates.length);
        self.options.benchmarkDates.push.apply(self.options.benchmarkDates, response.data.TestDueDates);
        self.options.selectedBenchmarkDate = getSelectedItemFromCollection(response.data.SelectedTDD, self.options.benchmarkDates);
        if (self.options.selectedBenchmarkDate != null && angular.isDefined(self.options.selectedBenchmarkDate)) {
          self.options.selectedBenchmarkDate.active = true;
        }
      }
      function reloadSchoolYears(response) {
        self.options.schoolYears.splice(0, self.options.schoolYears.length);
        self.options.schoolYears.push.apply(self.options.schoolYears, response.data.SchoolYears);
        self.options.selectedSchoolYear = getSelectedItemFromCollection(response.data.SelectedSchoolStartYear, self.options.schoolYears);
      }
      function reloadSchools(response) {
        self.options.schools.splice(0, self.options.schools.length);
        self.options.schools.push.apply(self.options.schools, response.data.Schools);
        self.options.selectedSchool = getSelectedItemFromCollection(response.data.SelectedSchool, self.options.schools);
      }
      function reloadGrades(response) {
        self.options.grades.splice(0, self.options.grades.length);
        self.options.grades.push.apply(self.options.grades, response.data.Grades);
        self.options.selectedGrade = getSelectedItemFromCollection(response.data.SelectedGrade, self.options.grades);
      }
      function reloadTeachers(response) {
        self.options.teachers.splice(0, self.options.teachers.length);
        self.options.teachers.push.apply(self.options.teachers, response.data.Teachers);
        self.options.selectedTeacher = getSelectedItemFromCollection(response.data.SelectedTeacher, self.options.teachers);
      }
      function reloadSections(response) {
        self.options.sections.splice(0, self.options.sections.length);
        self.options.sections.push.apply(self.options.sections, response.data.Sections);
        self.options.selectedSection = getSelectedItemFromCollection(response.data.SelectedSection, self.options.sections);
      }
      function reloadInterventionGroups(response) {
        self.options.interventionGroups.splice(0, self.options.interventionGroups.length);
        self.options.interventionGroups.push.apply(self.options.interventionGroups, response.data.InterventionGroups);
        self.options.selectedInterventionGroup = getSelectedItemFromCollection(response.data.SelectedInterventionGroup, self.options.interventionGroups);
      }
      function reloadInterventionists(response) {
        self.options.interventionists.splice(0, self.options.interventionists.length);
        self.options.interventionists.push.apply(self.options.interventionists, response.data.Interventionists);
        self.options.selectedInterventionist = getSelectedItemFromCollection(response.data.SelectedInterventionist, self.options.interventionists);
      }
      function reloadInterventionStudents(response) {
        self.options.interventionStudents.splice(0, self.options.interventionStudents.length);
        self.options.interventionStudents.push.apply(self.options.interventionStudents, response.data.InterventionStudents);
        self.options.selectedInterventionStudent = getSelectedItemFromCollection(response.data.SelectedInterventionStudent, self.options.interventionStudents);
      }
      function reloadSectionStudents(response) {
        self.options.sectionStudents.splice(0, self.options.sectionStudents.length);
        self.options.sectionStudents.push.apply(self.options.sectionStudents, response.data.SectionStudents);
        self.options.selectedSectionStudent = getSelectedItemFromCollection(response.data.SelectedSectionStudent, self.options.sectionStudents);
      }
      function reloadStints(response) {
        self.options.stints.splice(0, self.options.stints.length);
        self.options.stints.push.apply(self.options.stints, response.data.Stints);
        self.options.selectedStint = getSelectedItemFromCollection(response.data.SelectedStint, self.options.stints);
      }
      function reloadTeamMeetings(response) {
        self.options.teamMeetings.splice(0, self.options.teamMeetings.length);
        self.options.teamMeetings.push.apply(self.options.teamMeetings, response.data.TeamMeetings);
        self.options.selectedTeamMeeting = getSelectedItemFromCollection(response.data.SelectedTeamMeeting, self.options.teamMeetings);
      }
      function reloadTeamMeetingStaffs(response) {
        self.options.teamMeetingStaffs.splice(0, self.options.teamMeetingStaffs.length);
        self.options.teamMeetingStaffs.push.apply(self.options.teamMeetingStaffs, response.data.TeamMeetingStaffs);
        self.options.selectedTeamMeetingStaff = getSelectedItemFromCollection(response.data.SelectedTeamMeetingStaff, self.options.teamMeetingStaffs);
      }
      function getSelectedItemFromCollection(id, collection) {
        for (var i = 0; i < collection.length; i++) {
          if (collection[i].id == id) {
            return collection[i];
          }
        }
        return null;
      }
      function getSelectedStringFromCollection(item, collection) {
        for (var i = 0; i < collection.length; i++) {
          if (collection[i] == item) {
            return collection[i];
          }
        }
        return null;
      }
      self.changeSchool = function () {
        return self.loadSchoolChange(self.options);
      };
      self.changeGrade = function () {
        return self.loadGradeChange(self.options);
      };
      self.changeTeacher = function () {
        return self.loadTeacherChange(self.options);
      };
      self.changeInterventionist = function () {
        return self.loadInterventionistChange(self.options);
      };
      self.changeSection = function () {
        return self.loadSectionChange(self.options);
      };
      self.changeSectionStudent = function () {
        return self.loadSectionStudentChange(self.options);
      };
      self.changeInterventionStudent = function () {
        return self.loadInterventionStudentChange(self.options);
      };
      self.changeHfwMultiRange = function () {
        return self.loadChangeHfwMultiRange(self.options);
      };
      self.changeHfwSortOrder = function () {
        return self.loadChangeHfwSortOrder(self.options);
      };
      self.changeTeamMeeting = function () {
        return self.loadTeamMeetingChange(self.options);
      };
      self.changeInterventionGroup = function () {
        return self.loadInterventionGroupChange(self.options);
      };
      self.changeSchoolYear = function () {
        return self.loadSchoolYearChange(self.options);
      };
      self.changeBenchmarkDate = function () {
        return self.loadBenchmarkDateChange(self.options);
      };
    }
  ]);
}());
(function () {
  'use strict';
  angular.module('personalSettingsModule', []).controller('PersonalFieldsController', [
    '$scope',
    '$routeParams',
    '$location',
    'nsSectionService',
    'nsFilterOptionsService',
    'nsPinesService',
    'nsSelect2RemoteOptions',
    'NSPersonalFieldsManager',
    function PersonalFieldsController($scope, $routeParams, $location, nsSectionService, nsFilterOptionsService, nsPinesService, nsSelect2RemoteOptions, NSPersonalFieldsManager) {
      $scope.fieldsManager = new NSPersonalFieldsManager();
      $scope.assessments = $scope.fieldsManager.Assessments;
      $scope.errors = [];
      $scope.oneAtATime = true;
      $scope.changeFieldStatus = function (assessmentId, fieldId, status, hideFieldFrom) {
        $scope.fieldsManager.changeFieldStatus(assessmentId, fieldId, status, hideFieldFrom, function () {
          nsPinesService.dataSavedSuccessfully();
        }, function (msg) {
          $scope.errors.push({
            msg: '<strong>An Error Has Occurred</strong> ' + msg.data,
            type: 'danger'
          });
          $('html, body').animate({ scrollTop: 0 }, 'fast');
        });
      };
      $scope.removeStudentFromSection = function (id) {
        $scope.section.removeStudent(id);
      };
      $scope.closeAlert = function (index) {
        $scope.errors.splice(index, 1);
      };  // initial load
          //$scope.getCoTeacherRemoteOptions = nsSelect2RemoteOptions.CoTeacherRemoteOptions;
          //$scope.getStaffGroupRemoteOptions = nsSelect2RemoteOptions.StaffGroupRemoteOptions;
    }
  ]).controller('PersonalPasswordChangeController', [
    '$scope',
    '$routeParams',
    '$location',
    'nsSectionService',
    'nsFilterOptionsService',
    'nsPinesService',
    'nsSelect2RemoteOptions',
    'NSUserInfoService',
    function PersonalFieldsController($scope, $routeParams, $location, nsSectionService, nsFilterOptionsService, nsPinesService, nsSelect2RemoteOptions, NSUserInfoService) {
      $scope.user = NSUserInfoService.currentUser;
      $scope.userInfo = {};
      $scope.settings = {};
      $scope.errors = [];
      $scope.$on('NSHTTPError', function (event, data) {
        $scope.errors.push({
          type: 'danger',
          msg: data
        });
        $('html, body').animate({ scrollTop: 0 }, 'fast');
      });
      $scope.passwordValidator = function (password) {
        if (!password) {
          return;
        }
        if (password.length < 6) {
          return 'Password must be at least ' + 6 + ' characters long';
        }
        return true;
      };
      $scope.changePassword = function () {
        NSUserInfoService.changePassword($scope.user.Email, $scope.userInfo.pwd).then(function (response) {
          $scope.settings.pageStatus = 'success';
        });
      };
    }
  ]).controller('PersonalUsernameChangeController', [
    '$scope',
    '$routeParams',
    '$location',
    'nsSectionService',
    'nsFilterOptionsService',
    'nsPinesService',
    'nsSelect2RemoteOptions',
    'NSUserInfoService',
    'authService',
    '$parse',
    function PersonalUsernameChangeController($scope, $routeParams, $location, nsSectionService, nsFilterOptionsService, nsPinesService, nsSelect2RemoteOptions, NSUserInfoService, authService, $parse) {
      $scope.user = NSUserInfoService.currentUser;
      $scope.remoteUsernameValidationPath = NSUserInfoService.remoteUsernameValidationPath;
      $scope.remoteEmailFormatValidationPath = NSUserInfoService.remoteEmailFormatValidationPath;
      $scope.userInfo = {};
      $scope.settings = {};
      $scope.errors = [];
      $scope.validationObject = function () {
        var usernamePath = $scope.remoteUsernameValidationPath;
        var emailPath = $scope.remoteEmailFormatValidationPath;
        return {
          usernamePath: 'unique',
          emailPath: 'emailformat'
        };
      };
      $scope.$on('NSHTTPError', function (event, data) {
        $scope.errors.push({
          type: 'danger',
          msg: data
        });
        $('html, body').animate({ scrollTop: 0 }, 'fast');
      });
      $scope.logOut = function () {
        authService.logOut();
      };
      //$scope.checkUsernameValidationStatus = function (error) {
      //    if (error) {
      //        if (error.email == true) {
      //            // now check for valid format
      //            if (validateEmail($scope.userInfo.username)) {
      //                return true;
      //            } else {
      //                return 'Please enter a valid email address';
      //            }
      //        }
      //        else {
      //            return 'This Username is already assigned to another user';
      //        }
      //    } else {
      //        return true;
      //    }
      //}
      //function validateEmail(elementValue) {
      //    var emailPattern = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/;
      //    return emailPattern.test(elementValue);
      //}
      $scope.changeUsername = function () {
        NSUserInfoService.changeUsername($scope.userInfo.username).then(function (response) {
          $scope.settings.pageStatus = 'success';
        });
      };
    }
  ]).controller('PersonalProfileChangeController', [
    '$scope',
    '$routeParams',
    '$location',
    'nsSectionService',
    'nsFilterOptionsService',
    'nsPinesService',
    'nsSelect2RemoteOptions',
    'NSUserInfoService',
    '$global',
    function PersonalFieldsController($scope, $routeParams, $location, nsSectionService, nsFilterOptionsService, nsPinesService, nsSelect2RemoteOptions, NSUserInfoService, $global) {
      $scope.user = NSUserInfoService.currentUser;
      $scope.userInfo = {};
      $scope.settings = {
        teachKeyValid: true,
        teachKeyInvalidMessage: null
      };
      $scope.errors = [];
      $scope.remoteValidationPath = NSUserInfoService.remoteTeacherKeyValidationPath;
      $scope.checkTeacherKeyValidationStatus = function () {
        return $scope.settings.teachKeyValid;
      };
      // set initial profile settings from userinfo service
      $scope.userInfo.firstName = $scope.user.FirstName;
      $scope.userInfo.lastName = $scope.user.LastName;
      $scope.userInfo.middleName = $scope.user.MiddleName;
      $scope.userInfo.role = $scope.user.RoleID + '';
      $scope.userInfo.isInterventionist = $scope.user.IsInterventionSpecialist;
      $scope.userInfo.teacherKey = $scope.user.TeacherIdentifier;
      $scope.validateUniqueTeacherKey = function validateUniqueTeacherKey(teacherKey) {
        NSUserInfoService.validateUniqueTeacherKey(teacherKey).then(function (response) {
          return response.data.Success;
        });
      };
      $scope.$on('NSHTTPError', function (event, data) {
        $scope.errors.push({
          type: 'danger',
          msg: data
        });
        $('html, body').animate({ scrollTop: 0 }, 'fast');
      });
      $scope.updateProfile = function () {
        NSUserInfoService.updateProfile($scope.userInfo.firstName, $scope.userInfo.middleName, $scope.userInfo.lastName, $scope.userInfo.isInterventionist, $scope.userInfo.role, $scope.userInfo.teacherKey).then(function (response) {
          $scope.settings.pageStatus = 'success';
          // now force update of userinfservice
          $global.set('userprofileupdated', true);
        });
      };
    }
  ]).controller('PersonalContactNorthStarSupportController', [
    '$scope',
    '$routeParams',
    '$location',
    'nsSectionService',
    'nsFilterOptionsService',
    'nsPinesService',
    'nsSelect2RemoteOptions',
    'NSUserInfoService',
    '$global',
    function PersonalContactNorthStarSupportController($scope, $routeParams, $location, nsSectionService, nsFilterOptionsService, nsPinesService, nsSelect2RemoteOptions, NSUserInfoService, $global) {
      $scope.user = NSUserInfoService.currentUser;
      $scope.info = {};
      $scope.settings = {};
      $scope.errors = [];
      $scope.resetForm = function () {
        $scope.settings.pageStatus = null;
        $scope.info = {};
      };
      $scope.$on('NSHTTPError', function (event, data) {
        $scope.errors.push({
          type: 'danger',
          msg: data
        });
        $('html, body').animate({ scrollTop: 0 }, 'fast');
      });
      $scope.sendMail = function (subject, message) {
        NSUserInfoService.sendSupportMail(subject, message).then(function (response) {
          $scope.settings.pageStatus = 'success';
        });
      };
      $scope.getAdminsRemoteOptions = nsSelect2RemoteOptions.SchoolAdminRemoteOptions;
    }
  ]).controller('PersonalContactSchoolAdminController', [
    '$scope',
    '$routeParams',
    '$location',
    'nsSectionService',
    'nsFilterOptionsService',
    'nsPinesService',
    'nsSelect2RemoteOptions',
    'NSUserInfoService',
    '$global',
    function PersonalFieldsController($scope, $routeParams, $location, nsSectionService, nsFilterOptionsService, nsPinesService, nsSelect2RemoteOptions, NSUserInfoService, $global) {
      $scope.user = NSUserInfoService.currentUser;
      $scope.info = {};
      $scope.settings = {};
      $scope.errors = [];
      $scope.resetForm = function () {
        $scope.settings.pageStatus = null;
        $scope.info = {};
      };
      $scope.$on('NSHTTPError', function (event, data) {
        $scope.errors.push({
          type: 'danger',
          msg: data
        });
        $('html, body').animate({ scrollTop: 0 }, 'fast');
      });
      $scope.sendMail = function (to, subject, message) {
        NSUserInfoService.sendMail(to, subject, message).then(function (response) {
          $scope.settings.pageStatus = 'success';
        });
      };
      $scope.getAdminsRemoteOptions = nsSelect2RemoteOptions.SchoolAdminRemoteOptions;
    }
  ]).controller('PersonalContactDistrictAdminController', [
    '$scope',
    '$routeParams',
    '$location',
    'nsSectionService',
    'nsFilterOptionsService',
    'nsPinesService',
    'nsSelect2RemoteOptions',
    'NSUserInfoService',
    '$global',
    function PersonalContactDistrictAdminController($scope, $routeParams, $location, nsSectionService, nsFilterOptionsService, nsPinesService, nsSelect2RemoteOptions, NSUserInfoService, $global) {
      $scope.user = NSUserInfoService.currentUser;
      $scope.info = {};
      $scope.settings = {};
      $scope.errors = [];
      $scope.resetForm = function () {
        $scope.settings.pageStatus = null;
        $scope.info = {};
      };
      $scope.$on('NSHTTPError', function (event, data) {
        $scope.errors.push({
          type: 'danger',
          msg: data
        });
        $('html, body').animate({ scrollTop: 0 }, 'fast');
      });
      $scope.sendMail = function (to, subject, message) {
        NSUserInfoService.sendMail(to, subject, message).then(function (response) {
          $scope.settings.pageStatus = 'success';
        });
      };
      $scope.getAdminsRemoteOptions = nsSelect2RemoteOptions.DistrictAdminRemoteOptions;
    }
  ]).service('NSUserInfoService', [
    '$http',
    'webApiBaseUrl',
    function ($http, webApiBaseUrl) {
      var self = this;
      self.currentUser = {};
      self.remoteTeacherKeyValidationPath = webApiBaseUrl + '/api/staff/ValidateTeacherKeyChange';
      self.remoteUsernameValidationPath = webApiBaseUrl + '/api/staff/ValidateUsernameChange';
      self.remoteTeacherKeyOtherUserValidationPath = webApiBaseUrl + '/api/staff/ValidateTeacherKeyChangeOtherUser';
      self.remoteUsernameOtherUserValidationPath = webApiBaseUrl + '/api/staff/ValidateUsernameChangeOtherUser';
      self.remoteEmailFormatValidationPath = webApiBaseUrl + '/api/staff/ValidateEmailFormat';
      //self.validateUniqueTeacherKey = function (teacherKey) {
      //    var paramObj = { text: teacherKey };
      //    return $http.post(webApiBaseUrl + "/api/staff/ValidateTeacherKeyChange", paramObj);
      //};
      //self.validateUniqueUsername = function (username) {
      //    var paramObj = { text: username };
      //    return $http.post(webApiBaseUrl + "/api/staff/ValidateUsernameChange", paramObj);
      //};
      self.sendMail = function (to, subject, message) {
        var paramObj = {
            ToId: to,
            Subject: subject,
            Message: message
          };
        return $http.post(webApiBaseUrl + '/api/staff/sendmail', paramObj);
      };
      self.sendSupportMail = function (subject, message) {
        var paramObj = {
            ToId: -1,
            Subject: subject,
            Message: message
          };
        return $http.post(webApiBaseUrl + '/api/staff/sendsupportmail', paramObj);
      };
      self.loadUserInfo = function () {
        return $http.get(webApiBaseUrl + '/api/staff/myinfo').then(function (response) {
          angular.extend(self.currentUser, response.data);
        });
      };
      self.changePassword = function (email, pwd) {
        // TODO: use one that doesn't specify a username, or check that username matches current user
        return $http.post(webApiBaseUrl + '/api/PasswordReset/ResetUsersPassword', {
          Password: pwd,
          UserName: email
        }).then(function (response) {
          var result = response.data;
        });
      };
      self.changeUsername = function (username) {
        // TODO: use one that doesn't specify a username, or check that username matches current user
        return $http.post(webApiBaseUrl + '/api/PasswordReset/ChangeUsername', { value: username }).then(function (response) {
          var result = response.data;
        });
      };
      self.updateProfile = function (firstName, middleName, lastName, isInterventionist, role, teacherKey) {
        // TODO: use one that doesn't specify a username, or check that username matches current user
        return $http.post(webApiBaseUrl + '/api/staff/UpdateUserProfile', {
          FirstName: firstName,
          MiddleName: middleName,
          LastName: lastName,
          IsInterventionSpecialist: isInterventionist,
          RoleID: role,
          TeacherIdentifier: teacherKey
        }).then(function (response) {
          var result = response.data;
        });
      };
    }
  ]).factory('NSPersonalFieldsManager', [
    '$http',
    'webApiBaseUrl',
    function ($http, webApiBaseUrl) {
      var NSPersonalFieldsManager = function () {
        this.initialize = function () {
          var url = webApiBaseUrl + '/api/personalsettings/GetAssessmentsAndFieldsForUser';
          var personalData = $http.get(url);
          var self = this;
          self.Assessments = [];
          personalData.then(function (response) {
            angular.extend(self, response.data);
            if (self.Assessments === null)
              self.Assessments = [];
          }, function (response) {
          });
        };
        this.changeFieldStatus = function (assessmentId, fieldId, status, hideFieldFrom, successCallback, failureCallback) {
          var returnObject = {
              AssessmentId: assessmentId,
              FieldId: fieldId,
              HiddenStatus: status,
              HideFieldFrom: hideFieldFrom
            };
          var saveResponse = $http.post(webApiBaseUrl + '/api/personalsettings/UpdateFieldForUser', returnObject);
          saveResponse.then(successCallback, failureCallback);
        };
        this.initialize();
      };
      return NSPersonalFieldsManager;
    }
  ]);
}());
'use strict';
angular.module('lr.upload', [
  'lr.upload.formdata',
  'lr.upload.iframe',
  'lr.upload.directives'
]);
angular.module('lr.upload.directives', []);
'use strict';
angular.module('lr.upload.directives').directive('uploadButton', [
  'upload',
  function (upload) {
    return {
      restrict: 'EA',
      scope: {
        data: '=?data',
        url: '@',
        param: '@',
        method: '@',
        onUpload: '&',
        onSuccess: '&',
        onError: '&',
        onComplete: '&'
      },
      link: function (scope, element, attr) {
        var el = angular.element(element);
        var fileInput = angular.element('<input type="file" />');
        el.append(fileInput);
        fileInput.on('change', function uploadButtonFileInputChange() {
          if (fileInput[0].files && fileInput[0].files.length === 0) {
            return;
          }
          var options = {
              url: scope.url,
              method: scope.method || 'POST',
              forceIFrameUpload: scope.$eval(attr.forceIframeUpload) || false,
              data: scope.data || {}
            };
          options.data[scope.param || 'file'] = fileInput;
          scope.$apply(function () {
            scope.onUpload({ files: fileInput[0].files });
          });
          upload(options).then(function (response) {
            scope.onSuccess({ response: response });
            scope.onComplete({ response: response });
          }, function (response) {
            scope.onError({ response: response });
            scope.onComplete({ response: response });
          });
        });
        if ('required' in attr) {
          attr.$observe('required', function uploadButtonRequiredObserve(value) {
            var required = value === '' ? true : scope.$eval(value);
            fileInput.attr('required', required);
            element.toggleClass('ng-valid', !required);
            element.toggleClass('ng-invalid ng-invalid-required', required);
          });
        }
        if ('accept' in attr) {
          attr.$observe('accept', function uploadButtonAcceptObserve(value) {
            fileInput.attr('accept', value);
          });
        }
        if (upload.support.formData) {
          var uploadButtonMultipleObserve = function () {
            fileInput.attr('multiple', !!(scope.$eval(attr.multiple) && !scope.$eval(attr.forceIframeUpload)));
          };
          attr.$observe('multiple', uploadButtonMultipleObserve);
          attr.$observe('forceIframeUpload', uploadButtonMultipleObserve);
        }
      }
    };
  }
]);
'use strict';
angular.module('lr.upload.formdata', []).factory('formDataTransform', function () {
  return function formDataTransform(data) {
    var formData = new FormData();
    angular.forEach(data, function (value, key) {
      if (angular.isElement(value)) {
        var files = [];
        angular.forEach(value, function (el) {
          angular.forEach(el.files, function (file) {
            files.push(file);
          });
        });
        if (files.length !== 0) {
          if (files.length > 1) {
            angular.forEach(files, function (file, index) {
              formData.append(key + '[' + index + ']', file);
            });
          } else {
            formData.append(key, files[0]);
          }
        }
      } else {
        formData.append(key, value);
      }
    });
    return formData;
  };
}).factory('formDataUpload', [
  '$http',
  'formDataTransform',
  function ($http, formDataTransform) {
    return function formDataUpload(config) {
      config.transformRequest = formDataTransform;
      config.headers = angular.extend(config.headers || {}, { 'Content-Type': undefined });
      return $http(config);
    };
  }
]);
'use strict';
angular.module('lr.upload.iframe', []).factory('iFrameUpload', [
  '$q',
  '$http',
  '$document',
  '$rootScope',
  function ($q, $http, $document, $rootScope) {
    function indexOf(array, obj) {
      if (array.indexOf) {
        return array.indexOf(obj);
      }
      for (var i = 0; i < array.length; i++) {
        if (obj === array[i]) {
          return i;
        }
      }
      return -1;
    }
    function iFrameUpload(config) {
      var files = [];
      var deferred = $q.defer(), promise = deferred.promise;
      angular.forEach(config.data || {}, function (value, key) {
        if (angular.isElement(value)) {
          delete config.data[key];
          value.attr('name', key);
          files.push(value);
        }
      });
      var addParamChar = /\?/.test(config.url) ? '&' : '?';
      if (config.method === 'DELETE') {
        config.url = config.url + addParamChar + '_method=DELETE';
        config.method = 'POST';
      } else if (config.method === 'PUT') {
        config.url = config.url + addParamChar + '_method=PUT';
        config.method = 'POST';
      } else if (config.method === 'PATCH') {
        config.url = config.url + addParamChar + '_method=PATCH';
        config.method = 'POST';
      }
      var body = angular.element($document[0].body);
      var uniqueScope = $rootScope.$new();
      var uniqueName = 'iframe-transport-' + uniqueScope.$id;
      uniqueScope.$destroy();
      var form = angular.element('<form></form>');
      form.attr('target', uniqueName);
      form.attr('action', config.url);
      form.attr('method', config.method || 'POST');
      form.css('display', 'none');
      if (files.length) {
        form.attr('enctype', 'multipart/form-data');
        form.attr('encoding', 'multipart/form-data');
      }
      var iframe = angular.element('<iframe name="' + uniqueName + '" src="javascript:false;"></iframe>');
      iframe.on('load', function () {
        iframe.off('load').on('load', function () {
          var response;
          try {
            var doc = this.contentWindow ? this.contentWindow.document : this.contentDocument;
            response = angular.element(doc.body).text();
            if (!response.length) {
              throw new Error();
            }
          } catch (e) {
          }
          form.append(angular.element('<iframe src="javascript:false;"></iframe>'));
          try {
            response = transformData(response, $http.defaults.transformResponse);
          } catch (e) {
          }
          deferred.resolve({
            data: response,
            status: 200,
            headers: [],
            config: config
          });
        });
        angular.forEach(files, function (input) {
          var clone = input.clone(true);
          input.after(clone);
          form.append(input);
        });
        angular.forEach(config.data, function (value, name) {
          var input = angular.element('<input type="hidden" />');
          input.attr('name', name);
          input.val(value);
          form.append(input);
        });
        config.$iframeTransportForm = form;
        $http.pendingRequests.push(config);
        function transformData(data, fns) {
          var headers = [];
          if (angular.isFunction(fns)) {
            return fns(data, headers);
          }
          angular.forEach(fns, function (fn) {
            data = fn(data, headers);
          });
          return data;
        }
        function removePendingReq() {
          var idx = indexOf($http.pendingRequests, config);
          if (idx !== -1) {
            $http.pendingRequests.splice(idx, 1);
            config.$iframeTransportForm.remove();
            delete config.$iframeTransportForm;
          }
        }
        form[0].submit();
        promise.then(removePendingReq, removePendingReq);
      });
      form.append(iframe);
      body.append(form);
      return promise;
    }
    return iFrameUpload;
  }
]);
'use strict';
angular.module('lr.upload').factory('upload', [
  '$window',
  'formDataUpload',
  'iFrameUpload',
  function ($window, formDataUpload, iFrameUpload) {
    var support = {
        fileInput: !(new RegExp('(Android (1\\.[0156]|2\\.[01]))' + '|(Windows Phone (OS 7|8\\.0))|(XBLWP)|(ZuneWP)|(WPDesktop)' + '|(w(eb)?OSBrowser)|(webOS)' + '|(Kindle/(1\\.0|2\\.[05]|3\\.0))').test($window.navigator.userAgent) || angular.element('<input type="file">').prop('disabled')),
        fileUpload: !!($window.XMLHttpRequestUpload && $window.FileReader),
        formData: !!$window.FormData
      };
    function upload(config) {
      if (support.formData && !config.forceIFrameUpload) {
        return formDataUpload(config);
      }
      return iFrameUpload(config);
    }
    upload.support = support;
    return upload;
  }
]);
(function () {
  'use strict';
  angular.module('app.photo', []);
}());
(function () {
  'use strict';
  angular.module('app.photo').directive('egFiles', egFiles);
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
        angular.element(element[0].form).bind('reset', function () {
          scope.$apply(function () {
            scope.files.length = 0;
            scope.hasFiles = false;
          });
        });
      }
    }
  }
}());
(function () {
  'use strict';
  angular.module('app.photo').directive('egPhotoUploader', egPhotoUploader);
  egPhotoUploader.$inject = [
    'appInfo',
    'photoManager',
    '$templateCache'
  ];
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
}());
(function () {
  'use strict';
  angular.module('app.photo').directive('egUpload', egUpload);
  egUpload.$inject = ['$timeout'];
  function egUpload($timeout) {
    var directive = {
        link: link,
        restrict: 'A',
        scope: { upload: '&egUpload' }
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
}());
(function () {
  'use strict';
  angular.module('app.photo').factory('photoManager', photoManager);
  photoManager.$inject = [
    '$q',
    'photoManagerClient',
    'appInfo'
  ];
  function photoManager($q, photoManagerClient, appInfo) {
    var service = {
        photos: [],
        load: load,
        upload: upload,
        remove: remove,
        photoExists: photoExists,
        status: { uploading: false }
      };
    return service;
    function load() {
      appInfo.setInfo({
        busy: true,
        message: 'loading photos'
      });
      service.photos.length = 0;
      return photoManagerClient.query().$promise.then(function (result) {
        result.photos.forEach(function (photo) {
          service.photos.push(photo);
        });
        appInfo.setInfo({ message: 'photos loaded successfully' });
        return result.$promise;
      }, function (result) {
        appInfo.setInfo({ message: 'something went wrong: ' + result.data.message });
        return $q.reject(result);
      })['finally'](function () {
        appInfo.setInfo({ busy: false });
      });
    }
    function upload(photos) {
      service.status.uploading = true;
      appInfo.setInfo({
        busy: true,
        message: 'uploading photos'
      });
      var formData = new FormData();
      angular.forEach(photos, function (photo) {
        formData.append(photo.name, photo);
      });
      return photoManagerClient.save(formData).$promise.then(function (result) {
        if (result && result.photos) {
          result.photos.forEach(function (photo) {
            if (!photoExists(photo.name)) {
              service.photos.push(photo);
            }
          });
        }
        appInfo.setInfo({ message: 'photos uploaded successfully' });
        return result.$promise;
      }, function (result) {
        appInfo.setInfo({ message: 'something went wrong: ' + result.data.message });
        return $q.reject(result);
      })['finally'](function () {
        appInfo.setInfo({ busy: false });
        service.status.uploading = false;
      });
    }
    function remove(photo) {
      appInfo.setInfo({
        busy: true,
        message: 'deleting photo ' + photo.name
      });
      return photoManagerClient.remove({ fileName: photo.name }).$promise.then(function (result) {
        //if the photo was deleted successfully remove it from the photos array
        var i = service.photos.indexOf(photo);
        service.photos.splice(i, 1);
        appInfo.setInfo({ message: 'photos deleted' });
        return result.$promise;
      }, function (result) {
        appInfo.setInfo({ message: 'something went wrong: ' + result.data.message });
        return $q.reject(result);
      })['finally'](function () {
        appInfo.setInfo({ busy: false });
      });
    }
    function photoExists(photoName) {
      var res = false;
      service.photos.forEach(function (photo) {
        if (photo.name === photoName) {
          res = true;
        }
      });
      return res;
    }
  }
}());
(function () {
  'use strict';
  angular.module('app.photo').factory('photoManagerClient', photoManagerClient);
  photoManagerClient.$inject = [
    '$resource',
    'webApiBaseUrl'
  ];
  function photoManagerClient($resource, webApiBaseUrl) {
    return $resource(webApiBaseUrl + '/api/fileuploader/:fileName', { id: '@fileName' }, {
      'query': { method: 'GET' },
      'save': {
        method: 'POST',
        transformRequest: angular.identity,
        headers: { 'Content-Type': undefined }
      },
      'remove': {
        method: 'DELETE',
        url: webApiBaseUrl + '/api/fileuploader/:fileName',
        params: { name: '@fileName' }
      }
    });
  }
}());
(function () {
  'use strict';
  angular.module('app.photo').controller('photos', photos);
  photos.$inject = [
    '$scope',
    'photoManager'
  ];
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
      vm.previewPhoto = photo;
    }
    function remove(photo) {
      photoManager.remove(photo).then(function () {
        setPreviewPhoto();
      });
    }
  }
}());
(function () {
  'use strict';
  angular.module('app.photo').factory('appInfo', appInfo);
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
}());
(function () {
  'use strict';
  angular.module('app.photo').directive('egAppStatus', egAppStatus);
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
}());
(function () {
  'use strict';
  angular.module('districtDashboardsModule', []).service('NSDistrictDashboardMCAService', [
    '$http',
    'webApiBaseUrl',
    function ($http, webApiBaseUrl) {
      //this.options = {};
      var self = this;
      self.getVideosByPage = function (pageNumber) {
        var paramObj = { Id: pageNumber };
        var url = webApiBaseUrl + '/api/video/GetVzaarVideoList/';
        var promise = $http.post(url, paramObj);
        return promise;
      };
    }
  ]).controller('DistrictDashboardMCAController', [
    '$bootbox',
    'nsPinesService',
    'NSDistrictDashboardMCAService',
    '$scope',
    function ($bootbox, nsPinesService, NSDistrictDashboardMCAService, $scope) {
    }
  ]);
}());
(function () {
  'use strict';
  angular.module('schoolDashboardsModule', []).controller('SchoolDashboardMCAPrelimController', [
    '$bootbox',
    'nsPinesService',
    'nsStackedBarGraphOptionsFactory',
    '$scope',
    'nsFilterOptionsService',
    '$q',
    '$filter',
    function ($bootbox, nsPinesService, nsStackedBarGraphOptionsFactory, $scope, nsFilterOptionsService, $q, $filter) {
      $scope.settings = {};
      $scope.settings.summaryMode = false;
      $scope.settings.summaryView = 'Current Year Only (Sortable)';
      $scope.settings.summaryCategory = '';
      $scope.settings.summaryScoreGrouping = 1;
      $scope.settings.stacking = 'normal';
      $scope.settings.stackingDescription = 'Number of Students';
      $scope.settings.graphGenerated = false;
      $scope.settings.firstLoadStarted = false;
      $scope.sortArray = [];
      var highchartsNgConfig = {};
      $scope.groupsFactory = {};
      $scope.comparisonGroups = [];
      $scope.comparisonGroups.push(new nsStackedBarGraphOptionsFactory('MCA Reading - Prelim', false, true));
      $scope.comparisonGroups.push(new nsStackedBarGraphOptionsFactory('MCA Math - Prelim', false, true));
      $scope.comparisonGroups.push(new nsStackedBarGraphOptionsFactory('MCA Science - Prelim', false, true));
      $scope.filterOptions = nsFilterOptionsService.options;
      // TODO: fix hack, actually go and get assessmentfield.assessment.testtype
      $scope.getSummaryHeader = function (displayDate) {
        if ($scope.groupsFactory.TestDueDates.length == 1) {
          return $scope.groupsFactory.options.selectedSchoolYear.text;
        } else {
          return displayDate;
        }
      };
      // add initial groups (might be prelim or historical, based on checkbox)
      // TODO: FYI, need a benchmark date from THIS YEAR
      var loadInitialGroups = function () {
        $scope.comparisonGroups[0].options.selectedSchools = new Array($scope.filterOptions.selectedSchool);
        $scope.comparisonGroups[0].options.selectedGrades = $scope.filterOptions.selectedGrade == null ? [] : new Array($scope.filterOptions.selectedGrade);
        $scope.comparisonGroups[0].options.selectedSchoolYear = $scope.filterOptions.selectedSchoolYear;
        //    $scope.comparisonGroups[0].options.selectedTestDueDate = { id: 401, text: "crap" };
        // todo - get from db
        var readingField = {
            DisplayLabel: 'Achievement Level',
            DatabaseColumn: 'AchievementLevelId',
            FieldType: 'DropdownFromDB',
            LookupFieldName: 'AchievementLevel',
            FieldName: 'AchievementLevelId',
            AssessmentId: 55
          };
        $scope.comparisonGroups[0].options.selectedAssessmentField = readingField;
        $scope.comparisonGroups[1].options.selectedSchools = new Array($scope.filterOptions.selectedSchool);
        $scope.comparisonGroups[1].options.selectedGrades = $scope.filterOptions.selectedGrade == null ? [] : new Array($scope.filterOptions.selectedGrade);
        $scope.comparisonGroups[1].options.selectedSchoolYear = $scope.filterOptions.selectedSchoolYear;
        //  $scope.comparisonGroups[1].options.selectedTestDueDate = { id: 401, text: "crap" };
        // todo - get from db
        var mathField = {
            DisplayLabel: 'Achievement Level',
            DatabaseColumn: 'AchievementLevelId',
            FieldType: 'DropdownFromDB',
            LookupFieldName: 'AchievementLevel',
            FieldName: 'AchievementLevelId',
            AssessmentId: 57
          };
        $scope.comparisonGroups[1].options.selectedAssessmentField = mathField;
        $scope.comparisonGroups[2].options.selectedSchools = new Array($scope.filterOptions.selectedSchool);
        $scope.comparisonGroups[2].options.selectedGrades = $scope.filterOptions.selectedGrade == null ? [] : new Array($scope.filterOptions.selectedGrade);
        $scope.comparisonGroups[2].options.selectedSchoolYear = $scope.filterOptions.selectedSchoolYear;
        // $scope.comparisonGroups[2].options.selectedTestDueDate = { id: 401, text: "crap" };
        // todo - get from db
        var scienceField = {
            DisplayLabel: 'Achievement Level',
            DatabaseColumn: 'AchievementLevelId',
            FieldType: 'DropdownFromDB',
            LookupFieldName: 'AchievementLevel',
            FieldName: 'AchievementLevelId',
            AssessmentId: 56
          };
        $scope.comparisonGroups[2].options.selectedAssessmentField = scienceField;
      };
      var isReadyToLoad = function () {
        if ($scope.filterOptions.selectedSchool == null || $scope.filterOptions.selectedSchoolYear == null) {
          return false;
        } else {
          return true;
        }
      };
      $scope.generateGraph = function () {
        if (!isReadyToLoad()) {
          return;
        }
        $scope.settings.firstLoadStarted = true;
        loadInitialGroups();
        var promiseCollection = [];
        // loop over each group and get data... build graph once they've all returned
        for (var i = 0; i < $scope.comparisonGroups.length; i++) {
          promiseCollection.push($scope.comparisonGroups[i].loadGroupData(true));
        }
        var studentResultsCollection = [];
        //var groupNameCollection = [];
        $q.all(promiseCollection).then(function (response) {
          for (var j = 0; j < response.length; j++) {
            $scope.comparisonGroups[j].graphGenerated = true;
            studentResultsCollection.push(response[j].data);  //groupNameCollection.push()
          }
          updateDataFromServiceChange(studentResultsCollection);
        });
        $scope.settings.graphGenerated = true;
        $scope.settings.firstLoadStarted = false;
      };
      $scope.changeSummaryMode = function () {
        changeToSummaryMode($scope.settings.summaryCategory, $scope.settings.summaryScoreGrouping);
      };
      $scope.$watch('settings.stacking', function (newValue, oldValue) {
        if (!angular.equals(newValue, oldValue)) {
          if (newValue == 'normal') {
            $scope.settings.stackingDescription = 'Number of Students';
          } else {
            $scope.settings.stackingDescription = 'Percentage of Students';
          }
          $scope.generateGraph();
          $scope.settings.firstLoadStarted = false;
        }
      });
      // TODO: WATCH SCHOOL
      $scope.$watch('filterOptions.selectedSchool', function (newValue, oldValue) {
        if (isReadyToLoad() && $scope.settings.firstLoadStarted == false) {
          $scope.generateGraph();
          $scope.settings.firstLoadStarted = false;
        }
      });
      $scope.getChartHeader = function () {
        if (isReadyToLoad()) {
          return 'Preliminary MCA data for ' + $scope.filterOptions.selectedSchool.text + ' for the ' + $scope.filterOptions.selectedSchoolYear.text + ' school year for ' + ($scope.filterOptions.selectedGrade == null ? ' All grades' : $scope.filterOptions.selectedGrade.text + ' grade');
        } else {
          return 'Please select a year and school';
        }
      };
      $scope.$watch('filterOptions.selectedSchoolYear', function (newValue, oldValue) {
        if (isReadyToLoad() && $scope.settings.firstLoadStarted == false) {
          $scope.generateGraph();
          $scope.settings.firstLoadStarted = false;
        }
      });
      $scope.$watch('filterOptions.selectedGrade', function (newValue, oldValue) {
        if (isReadyToLoad() && $scope.settings.firstLoadStarted == false) {
          $scope.generateGraph();
          $scope.settings.firstLoadStarted = false;
        }
      });
      var changeToSummaryMode = function (category, scoreGrouping) {
        $scope.settings.summaryCategory = category;
        $scope.settings.summaryScoreGrouping = scoreGrouping;
        // find proper factory based on category (groupName)
        for (var i = 0; i < $scope.comparisonGroups.length; i++) {
          if ($scope.comparisonGroups[i].name == category) {
            $scope.groupsFactory = $scope.comparisonGroups[i];
            $scope.groupsFactory.loadSummaryData(scoreGrouping, category, $scope.groupsFactory.options.selectedTestDueDate.text, $scope.settings.summaryView !== 'Current Year Only (Sortable)').then(function (response) {
              $scope.settings.summaryMode = true;
            });
            break;
          }
        }
      };
      $scope.changeToChartMode = function () {
        $scope.settings.summaryView = 'Current Year Only (Sortable)';
        $scope.settings.summaryMode = false;
        $scope.settings.summaryCategory = '';
      };
      // initial auto-load
      $scope.generateGraph();
      function updateDataFromServiceChange(studentResultsCollection) {
        $scope.settings.anyResults = false;
        // might need this
        //angular.copy(data.results, $scope.studentResults);
        //$scope.studentResults = nsStackedBarGraphGroupsOptionsService.groupResults.data;
        //return;
        var seriesArray = [];
        var categoriesArray = [];
        // set up series
        for (var i = 0; i < studentResultsCollection.length; i++) {
          var currentResult = studentResultsCollection[i];
          var foundCategory = $filter('filter')(categoriesArray, currentResult.GroupName, true);
          // see if category already exists, if not, add it
          if (!foundCategory.length) {
            categoriesArray.push(currentResult.GroupName);  //[categoriesArray.length] = { name: currentResult.DueDate, categories: [currentResult.DueDate] };
          }
          for (var j = 0; j < currentResult.Results.length; j++) {
            $scope.settings.anyResults = true;
            var currentScore = currentResult.Results[j];
            //labels: {rotation: -90}
            // create a data array for each scoregrouping
            // FIX THIS... need to be able to create an array of arrays with the index being the scoregrouping
            var groupingName = '';
            var groupingColor = '';
            if (currentScore.ScoreGrouping == 1) {
              groupingName = 'Exceeds Expectations';
              groupingColor = '#4697ce';
            }
            if (currentScore.ScoreGrouping == 2) {
              groupingName = 'Meets Expectations';
              groupingColor = '#90ED7D';
            }
            if (currentScore.ScoreGrouping == 3) {
              groupingName = 'Approaches Expectations';
              groupingColor = '#E4D354';
            }
            if (currentScore.ScoreGrouping == 4) {
              groupingName = 'Does Not Meet Expectations';
              groupingColor = '#BF453D';
            }
            if (seriesArray[currentScore.ScoreGrouping] == null) {
              seriesArray[currentScore.ScoreGrouping] = {
                name: groupingName,
                color: groupingColor,
                data: [currentScore.NumberOfResults],
                id: currentScore.ScoreGrouping
              };
            } else {
              seriesArray[currentScore.ScoreGrouping].data.push(currentScore.NumberOfResults);
            }
          }
        }
        highchartsNgConfig = {
          options: {
            chart: { type: 'column' },
            tooltip: {
              pointFormat: '<span style="color:{series.color}">\u25cf</span>  <span style="color:#666666">{series.name}</span>: <b>{point.y} Students</b> ({point.percentage:.0f}%)<br/>',
              style: {
                padding: 10,
                fontWeight: 'bold'
              },
              useHTML: true
            },
            plotOptions: {
              series: {
                cursor: 'pointer',
                point: {
                  events: {
                    click: function (event) {
                      var category = this.category;
                      var scoreGrouping = this.series.userOptions.id;
                      changeToSummaryMode(category, scoreGrouping);
                    }
                  }
                }
              },
              column: {
                stacking: $scope.settings.stacking,
                dataLabels: {
                  enabled: true,
                  color: Highcharts.theme && Highcharts.theme.dataLabelsColor || 'white',
                  style: { textShadow: '0 0 3px black' },
                  formatter: function () {
                    if (this.y > 0 && $scope.settings.stacking === 'normal')
                      return this.y;
                    if (this.y > 0 && $scope.settings.stacking === 'percent')
                      return this.percentage.toFixed(0) + '%';
                  }
                }
              }
            }
          },
          yAxis: {
            allowDecimals: false,
            min: 0,
            title: { text: $scope.settings.stackingDescription },
            stackLabels: {
              enabled: true,
              style: {
                fontWeight: 'bold',
                color: Highcharts.theme && Highcharts.theme.textColor || 'gray'
              }
            }
          },
          series: seriesArray,
          title: { text: $scope.getChartHeader() },
          loading: false,
          xAxis: { categories: categoriesArray },
          useHighStocks: false,
          func: function (chart) {
          }
        };
        $scope.chartConfig = highchartsNgConfig;
      }
    }
  ]).controller('SchoolDashboardMCAController', [
    '$bootbox',
    'nsPinesService',
    'nsStackedBarGraphOptionsFactory',
    '$scope',
    'nsFilterOptionsService',
    '$q',
    '$filter',
    function ($bootbox, nsPinesService, nsStackedBarGraphOptionsFactory, $scope, nsFilterOptionsService, $q, $filter) {
      $scope.settings = {};
      $scope.settings.summaryMode = false;
      $scope.settings.summaryView = 'Current Year Only (Sortable)';
      $scope.settings.summaryCategory = '';
      $scope.settings.summaryScoreGrouping = 1;
      $scope.settings.stacking = 'normal';
      $scope.settings.stackingDescription = 'Number of Students';
      $scope.settings.graphGenerated = false;
      $scope.settings.firstLoadStarted = false;
      $scope.sortArray = [];
      var highchartsNgConfig = {};
      $scope.groupsFactory = {};
      $scope.comparisonGroups = [];
      $scope.comparisonGroups.push(new nsStackedBarGraphOptionsFactory('MCA Reading', false, true));
      $scope.comparisonGroups.push(new nsStackedBarGraphOptionsFactory('MCA Math', false, true));
      $scope.comparisonGroups.push(new nsStackedBarGraphOptionsFactory('MCA Science', false, true));
      $scope.filterOptions = nsFilterOptionsService.options;
      $scope.getSummaryHeader = function (displayDate) {
        if ($scope.groupsFactory.TestDueDates.length == 1) {
          return $scope.groupsFactory.options.selectedSchoolYear.text;
        } else {
          return displayDate;
        }
      };
      // add initial groups (might be prelim or historical, based on checkbox)
      var loadInitialGroups = function () {
        $scope.comparisonGroups[0].options.selectedSchools = new Array($scope.filterOptions.selectedSchool);
        $scope.comparisonGroups[0].options.selectedGrades = $scope.filterOptions.selectedGrade == null ? [] : new Array($scope.filterOptions.selectedGrade);
        $scope.comparisonGroups[0].options.selectedSchoolYear = $scope.filterOptions.selectedSchoolYear;
        $scope.comparisonGroups[0].options.selectedTestDueDate = {
          id: 364,
          text: 'test'
        };
        // todo - get from db
        var readingField = {
            DisplayLabel: 'Achievement Level',
            DatabaseColumn: 'AchievementLevelId',
            FieldType: 'DropdownFromDB',
            LookupFieldName: 'AchievementLevel',
            FieldName: 'AchievementLevelId',
            AssessmentId: 59
          };
        $scope.comparisonGroups[0].options.selectedAssessmentField = readingField;
        $scope.comparisonGroups[1].options.selectedSchools = new Array($scope.filterOptions.selectedSchool);
        $scope.comparisonGroups[1].options.selectedGrades = $scope.filterOptions.selectedGrade == null ? [] : new Array($scope.filterOptions.selectedGrade);
        $scope.comparisonGroups[1].options.selectedSchoolYear = $scope.filterOptions.selectedSchoolYear;
        $scope.comparisonGroups[1].options.selectedTestDueDate = {
          id: 364,
          text: 'test'
        };
        // todo - get from db
        var mathField = {
            DisplayLabel: 'Achievement Level',
            DatabaseColumn: 'AchievementLevelId',
            FieldType: 'DropdownFromDB',
            LookupFieldName: 'AchievementLevel',
            FieldName: 'AchievementLevelId',
            AssessmentId: 60
          };
        $scope.comparisonGroups[1].options.selectedAssessmentField = mathField;
        $scope.comparisonGroups[2].options.selectedSchools = new Array($scope.filterOptions.selectedSchool);
        $scope.comparisonGroups[2].options.selectedGrades = $scope.filterOptions.selectedGrade == null ? [] : new Array($scope.filterOptions.selectedGrade);
        $scope.comparisonGroups[2].options.selectedSchoolYear = $scope.filterOptions.selectedSchoolYear;
        $scope.comparisonGroups[2].options.selectedTestDueDate = {
          id: 364,
          text: 'test'
        };
        // todo - get from db
        var scienceField = {
            DisplayLabel: 'Achievement Level',
            DatabaseColumn: 'AchievementLevelId',
            FieldType: 'DropdownFromDB',
            LookupFieldName: 'AchievementLevel',
            FieldName: 'AchievementLevelId',
            AssessmentId: 61
          };
        $scope.comparisonGroups[2].options.selectedAssessmentField = scienceField;
      };
      var isReadyToLoad = function () {
        if ($scope.filterOptions.selectedSchool == null || $scope.filterOptions.selectedSchoolYear == null) {
          return false;
        } else {
          return true;
        }
      };
      $scope.generateGraph = function () {
        if (!isReadyToLoad()) {
          return;
        }
        $scope.settings.firstLoadStarted = true;
        loadInitialGroups();
        var promiseCollection = [];
        // loop over each group and get data... build graph once they've all returned
        for (var i = 0; i < $scope.comparisonGroups.length; i++) {
          promiseCollection.push($scope.comparisonGroups[i].loadGroupData(true));
        }
        var studentResultsCollection = [];
        //var groupNameCollection = [];
        $q.all(promiseCollection).then(function (response) {
          for (var j = 0; j < response.length; j++) {
            $scope.comparisonGroups[j].graphGenerated = true;
            studentResultsCollection.push(response[j].data);  //groupNameCollection.push()
          }
          updateDataFromServiceChange(studentResultsCollection);
        });
        $scope.settings.graphGenerated = true;
        $scope.settings.firstLoadStarted = false;
      };
      $scope.changeSummaryMode = function () {
        changeToSummaryMode($scope.settings.summaryCategory, $scope.settings.summaryScoreGrouping);
      };
      $scope.$watch('settings.stacking', function (newValue, oldValue) {
        if (!angular.equals(newValue, oldValue)) {
          if (newValue == 'normal') {
            $scope.settings.stackingDescription = 'Number of Students';
          } else {
            $scope.settings.stackingDescription = 'Percentage of Students';
          }
          $scope.generateGraph();
          $scope.settings.firstLoadStarted = false;
        }
      });
      // TODO: WATCH SCHOOL
      $scope.$watch('filterOptions.selectedSchool', function (newValue, oldValue) {
        if (isReadyToLoad() && $scope.settings.firstLoadStarted == false) {
          $scope.generateGraph();
          $scope.settings.firstLoadStarted = false;
        }
      });
      $scope.getChartHeader = function () {
        if (isReadyToLoad()) {
          return 'MCA data for ' + $scope.filterOptions.selectedSchool.text + ' for the ' + $scope.filterOptions.selectedSchoolYear.text + ' school year for ' + ($scope.filterOptions.selectedGrade == null ? ' All grades' : $scope.filterOptions.selectedGrade.text + ' grade');
        } else {
          return 'Please select a year and school';
        }
      };
      $scope.$watch('filterOptions.selectedSchoolYear', function (newValue, oldValue) {
        if (isReadyToLoad() && $scope.settings.firstLoadStarted == false) {
          $scope.generateGraph();
          $scope.settings.firstLoadStarted = false;
        }
      });
      $scope.$watch('filterOptions.selectedGrade', function (newValue, oldValue) {
        if (isReadyToLoad() && $scope.settings.firstLoadStarted == false) {
          $scope.generateGraph();
          $scope.settings.firstLoadStarted = false;
        }
      });
      var changeToSummaryMode = function (category, scoreGrouping) {
        $scope.settings.summaryCategory = category;
        $scope.settings.summaryScoreGrouping = scoreGrouping;
        // find proper factory based on category (groupName)
        for (var i = 0; i < $scope.comparisonGroups.length; i++) {
          if ($scope.comparisonGroups[i].name == category) {
            $scope.groupsFactory = $scope.comparisonGroups[i];
            $scope.groupsFactory.loadSummaryData(scoreGrouping, category, $scope.groupsFactory.options.selectedTestDueDate.text, $scope.settings.summaryView !== 'Current Year Only (Sortable)').then(function (response) {
              $scope.settings.summaryMode = true;
            });
            break;
          }
        }
      };
      $scope.changeToChartMode = function () {
        $scope.settings.summaryView = 'Current Year Only (Sortable)';
        $scope.settings.summaryMode = false;
        $scope.settings.summaryCategory = '';
      };
      // initial auto-load
      $scope.generateGraph();
      function updateDataFromServiceChange(studentResultsCollection) {
        $scope.settings.anyResults = false;
        // might need this
        //angular.copy(data.results, $scope.studentResults);
        //$scope.studentResults = nsStackedBarGraphGroupsOptionsService.groupResults.data;
        //return;
        var seriesArray = [];
        var categoriesArray = [];
        // set up series
        for (var i = 0; i < studentResultsCollection.length; i++) {
          var currentResult = studentResultsCollection[i];
          var foundCategory = $filter('filter')(categoriesArray, currentResult.GroupName, true);
          // see if category already exists, if not, add it
          if (!foundCategory.length) {
            categoriesArray.push(currentResult.GroupName);  //[categoriesArray.length] = { name: currentResult.DueDate, categories: [currentResult.DueDate] };
          }
          for (var j = 0; j < currentResult.Results.length; j++) {
            $scope.settings.anyResults = true;
            var currentScore = currentResult.Results[j];
            //labels: {rotation: -90}
            // create a data array for each scoregrouping
            // FIX THIS... need to be able to create an array of arrays with the index being the scoregrouping
            var groupingName = '';
            var groupingColor = '';
            if (currentScore.ScoreGrouping == 1) {
              groupingName = 'Exceeds Expectations';
              groupingColor = '#4697ce';
            }
            if (currentScore.ScoreGrouping == 2) {
              groupingName = 'Meets Expectations';
              groupingColor = '#90ED7D';
            }
            if (currentScore.ScoreGrouping == 3) {
              groupingName = 'Approaches Expectations';
              groupingColor = '#E4D354';
            }
            if (currentScore.ScoreGrouping == 4) {
              groupingName = 'Does Not Meet Expectations';
              groupingColor = '#BF453D';
            }
            if (seriesArray[currentScore.ScoreGrouping] == null) {
              seriesArray[currentScore.ScoreGrouping] = {
                name: groupingName,
                color: groupingColor,
                data: [currentScore.NumberOfResults],
                id: currentScore.ScoreGrouping
              };
            } else {
              seriesArray[currentScore.ScoreGrouping].data.push(currentScore.NumberOfResults);
            }
          }
        }
        highchartsNgConfig = {
          options: {
            chart: { type: 'column' },
            tooltip: {
              pointFormat: '<span style="color:{series.color}">\u25cf</span>  <span style="color:#666666">{series.name}</span>: <b>{point.y} Students</b> ({point.percentage:.0f}%)<br/>',
              style: {
                padding: 10,
                fontWeight: 'bold'
              },
              useHTML: true
            },
            plotOptions: {
              series: {
                cursor: 'pointer',
                point: {
                  events: {
                    click: function (event) {
                      var category = this.category;
                      var scoreGrouping = this.series.userOptions.id;
                      changeToSummaryMode(category, scoreGrouping);
                    }
                  }
                }
              },
              column: {
                stacking: $scope.settings.stacking,
                dataLabels: {
                  enabled: true,
                  color: Highcharts.theme && Highcharts.theme.dataLabelsColor || 'white',
                  style: { textShadow: '0 0 3px black' },
                  formatter: function () {
                    if (this.y > 0 && $scope.settings.stacking === 'normal')
                      return this.y;
                    if (this.y > 0 && $scope.settings.stacking === 'percent')
                      return this.percentage.toFixed(0) + '%';
                  }
                }
              }
            }
          },
          yAxis: {
            allowDecimals: false,
            min: 0,
            title: { text: $scope.settings.stackingDescription },
            stackLabels: {
              enabled: true,
              style: {
                fontWeight: 'bold',
                color: Highcharts.theme && Highcharts.theme.textColor || 'gray'
              }
            }
          },
          series: seriesArray,
          title: { text: $scope.getChartHeader() },
          loading: false,
          xAxis: { categories: categoriesArray },
          useHighStocks: false,
          func: function (chart) {
          }
        };
        $scope.chartConfig = highchartsNgConfig;
      }
    }
  ]);
}());
(function () {
  'use strict';
  angular.module('rolloverModule', []).factory('FullRolloverManager', [
    '$http',
    'webApiBaseUrl',
    'FileSaver',
    function ($http, webApiBaseUrl, FileSaver) {
      var InterventionDataImportManager = function () {
        var self = this;
        self.LoadTemplate = function () {
          var url = webApiBaseUrl + '/api/RosterRollover/GetFullRolloverImportTemplate';
          var promise = $http.get(url);
          return promise.then(function (response) {
            self.Fields = response.data.Fields;
          });
        };
        self.LoadImportHistory = function () {
          var url = webApiBaseUrl + '/api/RosterRollover/LoadFullRolloverImportHistory';
          var promise = $http.get(url);
          return promise.then(function (response) {
            angular.extend(self, response.data);  //self.RolloverInProgress = response.data.RolloverInProgress;
                                                  //if (self.HistoryItems) {
                                                  //    while(self.HistoryItems.length)
                                                  //        self.HistoryItems.pop();
                                                  //}
                                                  //self.HistoryItems = response.data.HistoryItems;
          });
        };
        self.FullRolloverReset = function (item) {
          var url = webApiBaseUrl + '/api/RosterRollover/FullRolloverReset';
          var promise = $http.post(url);
          return promise;
        };
        self.deleteHistoryItem = function (item) {
          var url = webApiBaseUrl + '/api/RosterRollover/DeleteFullRolloverHistoryItem';
          var paramObj = { id: item.Id };
          var promise = $http.post(url, paramObj);
          return promise;
        };
        self.CancelRollover = function (item) {
          var url = webApiBaseUrl + '/api/RosterRollover/CancelRollover';
          var paramObj = { id: item.Id };
          var promise = $http.post(url, paramObj);
          return promise;
        };
        self.validateRollover = function (item) {
          var url = webApiBaseUrl + '/api/RosterRollover/validateRollover';
          var paramObj = { id: item.Id };
          var promise = $http.post(url, paramObj);
          return promise;
        };
        self.downloadImportFile = function (item) {
          var url = webApiBaseUrl + '/api/RosterRollover/GetImportFile';
          var paramObj = { value: item.UploadedFileName };
          var promise = $http.post(url, paramObj, { responseType: 'arraybuffer' }).then(function (response) {
              var data = new Blob([response.data]);
              FileSaver.saveAs(data, 'originalimportfile.csv');
            });
        };
        self.downloadImportLog = function (item) {
          var paramObj = { id: item.Id };
          var promise = $http.post(webApiBaseUrl + '/api/RosterRollover/GetFullRolloverHistoryLog', paramObj);
          promise.then(function (response) {
            var data = new Blob([response.data.Result], { type: 'text/csv;charset=ANSI' });
            FileSaver.saveAs(data, 'importlog.txt');
          });
        };
      };
      return InterventionDataImportManager;
    }
  ]).controller('FullRolloverController', [
    '$scope',
    '$http',
    'webApiBaseUrl',
    'progressLoader',
    'FullRolloverManager',
    'nsFilterOptionsService',
    'FileSaver',
    '$timeout',
    'nsPinesService',
    '$interval',
    '$bootbox',
    '$uibModal',
    function ($scope, $http, webApiBaseUrl, progressLoader, FullRolloverManager, nsFilterOptionsService, FileSaver, $timeout, nsPinesService, $interval, $bootbox, $uibModal) {
      $scope.settings = {
        uploadComplete: false,
        hasFiles: false
      };
      $scope.dataMgr = new FullRolloverManager();
      $scope.filterOptions = nsFilterOptionsService.options;
      $scope.theFiles = [];
      $scope.downloadImportFile = function (item) {
        $scope.dataMgr.downloadImportFile(item);
      };
      $scope.verificationDialog = function (job) {
        var modalInstance = $uibModal.open({
            templateUrl: 'rolloverIssueValidation.html',
            scope: $scope,
            controller: function ($scope, $uibModalInstance) {
              $scope.selectedJob = job;
              $scope.issues = job.RolloverLogMessages;
              $scope.ensureAllSelected = function () {
                for (var i = 0; i < $scope.issues.length; i++) {
                  if ($scope.issues[i].Validate != true) {
                    return true;
                  }
                }
                return false;
              };
              $scope.validateRollover = function (job) {
                $bootbox.confirm('Are you sure you want to ignore all potential issues listed for this rollover? <BR><BR> <b>Note:</b> You may end up with duplicate students and/or teachers if the issues are not investigated thoroughly.', function (response) {
                  if (response) {
                    progressLoader.start();
                    progressLoader.set(50);
                    $scope.dataMgr.validateRollover(job).then(function (response) {
                      progressLoader.end();
                      $uibModalInstance.dismiss('cancel');
                    });
                  }
                });
              };
              $scope.cancel = function () {
                $uibModalInstance.dismiss('cancel');
              };
            },
            size: 'md'
          });
      };
      $scope.deleteHistoryItem = function (item) {
        $bootbox.confirm('Are you sure you want to delete this job?  <b>Note:</b> If it has not yet been processed, it will be cancelled.', function (response) {
          if (response) {
            progressLoader.start();
            progressLoader.set(50);
            $scope.dataMgr.deleteHistoryItem(item).then(function (response) {
              nsPinesService.dataDeletedSuccessfully();
              reloadHistoryTable();
              progressLoader.end();
            });
          }
        });
      };
      $scope.downloadImportLog = function (item) {
        $scope.dataMgr.downloadImportLog(item);
      };
      $scope.getStatusClass = function (item) {
        if (item.Status == 'Awaiting processing') {
          return 'badge-default';
        } else if (item.Status == 'Complete') {
          return 'badge-success';
        } else if (item.Status == 'Processing') {
          return 'badge-primary';
        } else if (item.Status == 'Error') {
          return 'badge-danger';
        } else {
          return 'badge-warning';
        }
      };
      var reloadHistoryTable = function () {
        $scope.dataMgr.LoadImportHistory();
      };
      $scope.cancelRollover = function (job) {
        $bootbox.confirm('Are you sure you want to cancel this rollover? <BR><BR> <b>Note:</b> You can resolve any issues and resubmit the rollover file to try again.', function (response) {
          if (response) {
            progressLoader.start();
            progressLoader.set(50);
            $scope.dataMgr.CancelRollover(job).then(function (response) {
              progressLoader.end();
            });
          }
        });
      };
      $scope.FullRolloverReset = function () {
        $bootbox.confirm('Are you sure you want to cancel any rollover that may be in progress? <BR><BR> <b>Note:</b> You typically only need to do this if a rollover is in a "hung" state or cannot be completed for some reason.', function (response) {
          if (response) {
            progressLoader.start();
            progressLoader.set(50);
            $scope.dataMgr.FullRolloverReset().then(function (response) {
              progressLoader.end();
            });
          }
        });
      };
      reloadHistoryTable();
      // reload table every 5 seconds
      var reloadInterval = $interval(reloadHistoryTable, 5000);
      // here is where the cleanup happens
      $scope.$on('$destroy', function () {
        $interval.cancel(reloadInterval);
      });
      $scope.getTemplate = function () {
        var promise = $http.get(webApiBaseUrl + '/api/RosterRollover/GetFullRolloverTemplateCSV');
        promise.then(function (response) {
          var data = new Blob([response.data.Result], { type: 'text/csv;charset=ANSI' });
          FileSaver.saveAs(data, 'template.csv');
        });
      };
      $scope.upload = function (theFiles) {
        var formData = new FormData();
        formData.append('SchoolYear', $scope.filterOptions.selectedSchoolYear.id);
        angular.forEach(theFiles, function (file) {
          formData.append(file.name, file);
        });
        var paramObj = {};
        // start loader
        progressLoader.start();
        progressLoader.set(50);
        var promise = $http.post(webApiBaseUrl + '/api/RosterRollover/uploadfullrollovercsv', formData, {
            transformRequest: angular.identity,
            headers: { 'Content-Type': undefined }
          }).then(function (response) {
            // end loader
            progressLoader.end();
            //$scope.errors = [];
            $scope.settings.LogItems = response.data.LogItems;
            // show success
            $timeout(function () {
              $('#formReset').click();
            }, 100);
            //$scope.theFiles.length = 0;
            //$scope.settings.hasFiles = false;
            $scope.settings.uploadComplete = true;
            if ($scope.settings.LogItems.length > 0) {
              nsPinesService.buildMessage('Data Import Error', 'There were one or more errors in your import file.', 'error');
            } else {
              nsPinesService.buildMessage('Data Upload Success', 'Your data was successfully uploaded.', 'success');
            }
          }, function (err) {
            progressLoader.end();
            $scope.settings.uploadComplete = true;
          });
      };
      progressLoader.start();
      progressLoader.set(50);
      $scope.dataMgr.LoadTemplate().then(function (response) {
        progressLoader.end();
      });
    }
  ]);
}());
(function () {
  'use strict';
  angular.module('dataExportModule', []).factory('AssessmentDataExportManager', [
    '$http',
    'webApiBaseUrl',
    'FileSaver',
    function ($http, webApiBaseUrl, FileSaver) {
      var InterventionDataImportManager = function () {
        var self = this;
        self.LoadAssessmentDataExportHistory = function () {
          var url = webApiBaseUrl + '/api/exportdata/LoadAssessmentDataExportHistory';
          var promise = $http.get(url);
          return promise.then(function (response) {
            self.HistoryItems = response.data.HistoryItems;
          });
        };
        self.deleteHistoryItem = function (item) {
          var url = webApiBaseUrl + '/api/exportdata/DeleteHistoryItem';
          var paramObj = { id: item.Id };
          var promise = $http.post(url, paramObj);
          return promise;
        };
        self.downloadExportFile = function (item) {
          var url = webApiBaseUrl + '/api/exportdata/DownloadExportFile';
          var paramObj = { value: item.UploadedFileName };
          var promise = $http.post(url, paramObj, { responseType: 'arraybuffer' }).then(function (response) {
              var data = new Blob([response.data]);
              FileSaver.saveAs(data, 'assessmentdataexport.csv');
            });
        };
      };
      return InterventionDataImportManager;
    }
  ]).controller('AssessmentDataExportController', [
    '$scope',
    '$http',
    'webApiBaseUrl',
    'progressLoader',
    'nsFilterOptionsService',
    'FileSaver',
    '$timeout',
    'nsPinesService',
    '$interval',
    '$bootbox',
    'nsStackedBarGraphOptionsFactory',
    'AssessmentDataExportManager',
    function ($scope, $http, webApiBaseUrl, progressLoader, nsFilterOptionsService, FileSaver, $timeout, nsPinesService, $interval, $bootbox, nsStackedBarGraphOptionsFactory, AssessmentDataExportManager) {
      $scope.settings = {
        uploadComplete: false,
        hasFiles: false
      };
      $scope.dataMgr = new AssessmentDataExportManager();
      $scope.groupsFactory = new nsStackedBarGraphOptionsFactory('Get Assessment Export Data', false);
      $scope.filterOptions = $scope.groupsFactory.options;
      $scope.deleteHistoryItem = function (item) {
        $bootbox.confirm('Are you sure you want to delete this job?  <b>Note:</b> If it has not yet been processed, it will be cancelled.', function (response) {
          if (response) {
            progressLoader.start();
            progressLoader.set(50);
            $scope.dataMgr.deleteHistoryItem(item).then(function (response) {
              nsPinesService.dataDeletedSuccessfully();
              reloadHistoryTable();
              progressLoader.end();
            });
          }
        });
      };
      $scope.downloadExportFile = function (item) {
        $scope.dataMgr.downloadExportFile(item);
      };
      $scope.getStatusClass = function (item) {
        if (item.Status == 'Awaiting processing') {
          return 'badge-default';
        } else if (item.Status == 'Complete') {
          return 'badge-success';
        } else if (item.Status == 'Processing') {
          return 'badge-primary';
        } else if (item.Status == 'Error') {
          return 'badge-danger';
        } else {
          return 'badge-warning';
        }
      };
      var reloadHistoryTable = function () {
        $scope.dataMgr.LoadAssessmentDataExportHistory();
      };
      reloadHistoryTable();
      //reload table every 5 seconds
      var reloadInterval = $interval(reloadHistoryTable, 5000);
      //here is where the cleanup happens
      $scope.$on('$destroy', function () {
        $interval.cancel(reloadInterval);
      });
      $scope.getExportData = function () {
        $scope.groupsFactory.loadOSExportData();
        nsPinesService.buildMessage('Export Requested', 'Your data export has been requested', 'info');
      };
    }
  ]).factory('AttendanceDataExportManager', [
    '$http',
    'webApiBaseUrl',
    'FileSaver',
    function ($http, webApiBaseUrl, FileSaver) {
      var InterventionDataImportManager = function () {
        var self = this;
        self.LoadHistory = function () {
          var url = webApiBaseUrl + '/api/exportdata/LoadAttendanceDataExportHistory';
          var promise = $http.get(url);
          return promise.then(function (response) {
            self.HistoryItems = response.data.HistoryItems;
          });
        };
        self.deleteHistoryItem = function (item) {
          var url = webApiBaseUrl + '/api/exportdata/DeleteAttendanceHistoryItem';
          var paramObj = { Id: item.Id };
          var promise = $http.post(url, paramObj);
          return promise;
        };
        self.getExportData = function (year) {
          var returnObject = { id: year };
          // doesn't actually return the data, just creates a job
          return $http.post(webApiBaseUrl + '/api/exportdata/CreateAttendanceDataExportJob', returnObject);
        };
        self.downloadExportFile = function (item) {
          var url = webApiBaseUrl + '/api/exportdata/DownloadAttendanceExportFile';
          var paramObj = { value: item.UploadedFileName };
          var promise = $http.post(url, paramObj, { responseType: 'arraybuffer' }).then(function (response) {
              var data = new Blob([response.data]);
              FileSaver.saveAs(data, 'attendancedataexport.csv');
            });
        };
      };
      return InterventionDataImportManager;
    }
  ]).controller('AttendanceDataExportController', [
    '$scope',
    '$http',
    'webApiBaseUrl',
    'progressLoader',
    'nsFilterOptionsService',
    'FileSaver',
    '$timeout',
    'nsPinesService',
    '$interval',
    '$bootbox',
    'AttendanceDataExportManager',
    function ($scope, $http, webApiBaseUrl, progressLoader, nsFilterOptionsService, FileSaver, $timeout, nsPinesService, $interval, $bootbox, AttendanceDataExportManager) {
      $scope.settings = {
        uploadComplete: false,
        hasFiles: false
      };
      $scope.dataMgr = new AttendanceDataExportManager();
      $scope.filterOptions = nsFilterOptionsService.options;
      $scope.deleteHistoryItem = function (item) {
        $bootbox.confirm('Are you sure you want to delete this job?  <b>Note:</b> If it has not yet been processed, it will be cancelled.', function (response) {
          if (response) {
            progressLoader.start();
            progressLoader.set(50);
            $scope.dataMgr.deleteHistoryItem(item).then(function (response) {
              nsPinesService.dataDeletedSuccessfully();
              reloadHistoryTable();
              progressLoader.end();
            });
          }
        });
      };
      $scope.downloadExportFile = function (item) {
        $scope.dataMgr.downloadExportFile(item);
      };
      $scope.getStatusClass = function (item) {
        if (item.Status == 'Awaiting processing') {
          return 'badge-default';
        } else if (item.Status == 'Complete') {
          return 'badge-success';
        } else if (item.Status == 'Processing') {
          return 'badge-primary';
        } else if (item.Status == 'Error') {
          return 'badge-danger';
        } else {
          return 'badge-warning';
        }
      };
      var reloadHistoryTable = function () {
        $scope.dataMgr.LoadHistory();
      };
      reloadHistoryTable();
      //reload table every 5 seconds
      var reloadInterval = $interval(reloadHistoryTable, 5000);
      //here is where the cleanup happens
      $scope.$on('$destroy', function () {
        $interval.cancel(reloadInterval);
      });
      $scope.getExportData = function () {
        if ($scope.filterOptions.selectedSchoolYear != null) {
          $scope.dataMgr.getExportData($scope.filterOptions.selectedSchoolYear.id);
          nsPinesService.buildMessage('Export Requested', 'Your data export has been requested', 'info');
        }
      };
    }
  ]);
}());
(function (angular) {
  'use strict';
  if (!angular) {
    throw 'Missing something? Please add angular.js to your project or move this script below the angular.js reference';
  }
  var directiveId = 'ngRemoteValidate2', remoteValidate2 = function ($http, $timeout, $q) {
      return {
        restrict: 'A',
        require: [
          '^form',
          'ngModel'
        ],
        scope: { ngRemoteInterceptors: '=?' },
        link: function (scope, el, attrs, ctrls) {
          var cache = {}, handleChange, setValidation, addToCache, request, shouldProcess, ngForm = ctrls[0], ngModel = ctrls[1], options = {
              ngRemoteThrottle: 400,
              ngRemoteMethod: 'POST'
            };
          angular.extend(options, attrs);
          //TODO: Use Cain of Responsibility to reduce complexity.
          if (options.ngRemoteValidate2.charAt(0) === '[') {
            options.urls = eval(options.ngRemoteValidate2);
          } else if (options.ngRemoteValidate2.charAt(0) === '{') {
            options.keys = eval('(' + options.ngRemoteValidate2 + ')');
            options.urls = Object.keys(options.keys);
          } else {
            options.urls = [options.ngRemoteValidate2];
          }
          addToCache = function (response) {
            var value = response[0].data.value;
            if (cache[value])
              return cache[value];
            cache[value] = response;
          };
          shouldProcess = function (value) {
            var otherRulesInValid = false;
            for (var p in ngModel.$error) {
              var checkedKey = !options.hasOwnProperty('keys') || !Object.keys(options.keys).filter(function (k) {
                  return options.keys[k] === p;
                })[0];
              if (ngModel.$error[p] && p != directiveId && checkedKey) {
                otherRulesInValid = true;
                break;
              }
            }
            return !(ngModel.$pristine || otherRulesInValid);
          };
          setValidation = function (response, skipCache) {
            var i = 0, l = response.length, useKeys = options.hasOwnProperty('keys'), isValid = true;
            for (; i < l; i++) {
              if (scope.ngRemoteInterceptors && scope.ngRemoteInterceptors.response) {
                response[i] = scope.ngRemoteInterceptors.response(response[i]);
              }
              if (!response[i].data.isValid) {
                isValid = false;
                if (!useKeys) {
                  break;
                }
              }
              var canSetKey = useKeys && response[i].hasOwnProperty('config') && options.keys[response[i].config.url];
              if (canSetKey) {
                var key = options.keys[response[i].config.url];
                ngModel.$setValidity(key, response[i].data.isValid);
              }
              if (response[i].data.formattedValue) {
                ngModel.$setViewValue(response[i].data.formattedValue);
                ngModel.$render();
              }
            }
            if (!skipCache) {
              addToCache(response);
            }
            ngModel.$setValidity(directiveId, isValid);
            ngModel.$processing = ngModel.$pending = ngForm.$pending = false;
          };
          handleChange = function (value) {
            if (typeof value === 'undefined' || value === '') {
              if (request) {
                $timeout.cancel(request);
              }
              ngModel.$setPristine();
              ngModel.$setValidity(directiveId, true);
              return;
            }
            if (!shouldProcess(value)) {
              return setValidation([{
                  data: {
                    isValid: true,
                    value: value
                  }
                }], true);
            }
            if (cache[value]) {
              return setValidation(cache[value], true);
            }
            //Set processing now, before the delay.
            //Check first to reduce DOM updates
            if (!ngModel.$pending) {
              ngModel.$processing = ngModel.$pending = ngForm.$pending = true;
            }
            if (request) {
              $timeout.cancel(request);
            }
            request = $timeout(function () {
              var calls = [], i = 0, l = options.urls.length, toValidate = { value: value }, httpOpts = { method: options.ngRemoteMethod };
              if (scope.$parent[el[0].name + 'SetArgs']) {
                toValidate = scope.$parent[el[0].name + 'SetArgs'](value, el, attrs, ngModel);
              }
              if (options.ngRemoteMethod == 'POST') {
                httpOpts.data = toValidate;
              } else {
                httpOpts.params = toValidate;
              }
              for (; i < l; i++) {
                httpOpts.url = options.urls[i];
                if (scope.ngRemoteInterceptors && scope.ngRemoteInterceptors.request) {
                  httpOpts = scope.ngRemoteInterceptors.request(httpOpts);
                }
                calls.push($http(httpOpts));
              }
              $q.all(calls).then(setValidation);
            }, options.ngRemoteThrottle);
            return true;
          };
          //ngModel.$parsers.unshift( handleChange );
          scope.$watch(function () {
            return ngModel.$viewValue;
          }, handleChange);
        }
      };
    };
  angular.module('remoteValidation2', []).constant('MODULE_VERSION', '##_version_##').directive(directiveId, [
    '$http',
    '$timeout',
    '$q',
    remoteValidate2
  ]);
}(this.angular));
/*!
 * jquery.fixedHeaderTable. The jQuery fixedHeaderTable plugin
 *
 * Copyright (c) 2013 Mark Malek
 * http://fixedheadertable.com
 *
 * Licensed under MIT
 * http://www.opensource.org/licenses/mit-license.php
 *
 * http://docs.jquery.com/Plugins/Authoring
 * jQuery authoring guidelines
 *
 * Launch  : October 2009
 * Version : 1.3
 * Released: May 9th, 2011
 *
 *
 * all CSS sizing (width,height) is done in pixels (px)
 */
(function ($) {
  $.fn.fixedHeaderTable = function (method) {
    // plugin's default options
    var defaults = {
        width: '100%',
        height: '100%',
        themeClass: 'fht-default',
        borderCollapse: true,
        fixedColumns: 0,
        fixedColumn: false,
        sortable: false,
        autoShow: true,
        footer: false,
        cloneHeadToFoot: false,
        autoResize: false,
        create: null
      };
    var settings = {};
    // public methods
    var methods = {
        init: function (options) {
          settings = $.extend({}, defaults, options);
          // iterate through all the DOM elements we are attaching the plugin to
          return this.each(function () {
            var $self = $(this);
            // reference the jQuery version of the current DOM element
            if (helpers._isTable($self)) {
              methods.setup.apply(this, Array.prototype.slice.call(arguments, 1));
              $.isFunction(settings.create) && settings.create.call(this);
            } else {
              $.error('Invalid table mark-up');
            }
          });
        },
        setup: function () {
          var $self = $(this), self = this, $thead = $self.find('thead'), $tfoot = $self.find('tfoot'), tfootHeight = 0, $wrapper, $divHead, $divBody, $fixedBody, widthMinusScrollbar;
          settings.originalTable = $(this).clone();
          settings.includePadding = helpers._isPaddingIncludedWithWidth();
          settings.scrollbarOffset = helpers._getScrollbarWidth();
          settings.themeClassName = settings.themeClass;
          if (settings.width.search('%') > -1) {
            widthMinusScrollbar = $self.parent().width() - settings.scrollbarOffset;
          } else {
            widthMinusScrollbar = settings.width - settings.scrollbarOffset;
          }
          $self.css({ width: widthMinusScrollbar });
          if (!$self.closest('.fht-table-wrapper').length) {
            $self.addClass('fht-table');
            $self.wrap('<div class="fht-table-wrapper"></div>');
          }
          $wrapper = $self.closest('.fht-table-wrapper');
          if (settings.fixedColumn == true && settings.fixedColumns <= 0) {
            settings.fixedColumns = 1;
          }
          if (settings.fixedColumns > 0 && $wrapper.find('.fht-fixed-column').length == 0) {
            $self.wrap('<div class="fht-fixed-body"></div>');
            $('<div class="fht-fixed-column"></div>').prependTo($wrapper);
            $fixedBody = $wrapper.find('.fht-fixed-body');
          }
          $wrapper.css({
            width: settings.width,
            height: settings.height
          }).addClass(settings.themeClassName);
          if (!$self.hasClass('fht-table-init')) {
            $self.wrap('<div class="fht-tbody"></div>');
          }
          $divBody = $self.closest('.fht-tbody');
          var tableProps = helpers._getTableProps($self);
          helpers._setupClone($divBody, tableProps.tbody);
          if (!$self.hasClass('fht-table-init')) {
            if (settings.fixedColumns > 0) {
              $divHead = $('<div class="fht-thead"><table class="fht-table"></table></div>').prependTo($fixedBody);
            } else {
              $divHead = $('<div class="fht-thead"><table class="fht-table"></table></div>').prependTo($wrapper);
            }
            $divHead.find('table.fht-table').addClass(settings.originalTable.attr('class')).attr('style', settings.originalTable.attr('style'));
            $thead.clone().appendTo($divHead.find('table'));
          } else {
            $divHead = $wrapper.find('div.fht-thead');
          }
          helpers._setupClone($divHead, tableProps.thead);
          $self.css({ 'margin-top': -$divHead.outerHeight(true) });
          /*
                 * Check for footer
                 * Setup footer if present
                 */
          if (settings.footer == true) {
            helpers._setupTableFooter($self, self, tableProps);
            if (!$tfoot.length) {
              $tfoot = $wrapper.find('div.fht-tfoot table');
            }
            tfootHeight = $tfoot.outerHeight(true);
          }
          var tbodyHeight = $wrapper.height() - $thead.outerHeight(true) - tfootHeight - tableProps.border;
          $divBody.css({ 'height': tbodyHeight });
          $self.addClass('fht-table-init');
          if (typeof settings.altClass !== 'undefined') {
            methods.altRows.apply(self);
          }
          if (settings.fixedColumns > 0) {
            helpers._setupFixedColumn($self, self, tableProps);
          }
          if (!settings.autoShow) {
            $wrapper.hide();
          }
          helpers._bindScroll($divBody, tableProps);
          return self;
        },
        resize: function () {
          var self = this;
          return self;
        },
        altRows: function (arg1) {
          var $self = $(this), altClass = typeof arg1 !== 'undefined' ? arg1 : settings.altClass;
          $self.closest('.fht-table-wrapper').find('tbody tr:odd:not(:hidden)').addClass(altClass);
        },
        show: function (arg1, arg2, arg3) {
          var $self = $(this), self = this, $wrapper = $self.closest('.fht-table-wrapper');
          // User provided show duration without a specific effect
          if (typeof arg1 !== 'undefined' && typeof arg1 === 'number') {
            $wrapper.show(arg1, function () {
              $.isFunction(arg2) && arg2.call(this);
            });
            return self;
          } else if (typeof arg1 !== 'undefined' && typeof arg1 === 'string' && typeof arg2 !== 'undefined' && typeof arg2 === 'number') {
            // User provided show duration with an effect
            $wrapper.show(arg1, arg2, function () {
              $.isFunction(arg3) && arg3.call(this);
            });
            return self;
          }
          $self.closest('.fht-table-wrapper').show();
          $.isFunction(arg1) && arg1.call(this);
          return self;
        },
        hide: function (arg1, arg2, arg3) {
          var $self = $(this), self = this, $wrapper = $self.closest('.fht-table-wrapper');
          // User provided show duration without a specific effect
          if (typeof arg1 !== 'undefined' && typeof arg1 === 'number') {
            $wrapper.hide(arg1, function () {
              $.isFunction(arg3) && arg3.call(this);
            });
            return self;
          } else if (typeof arg1 !== 'undefined' && typeof arg1 === 'string' && typeof arg2 !== 'undefined' && typeof arg2 === 'number') {
            $wrapper.hide(arg1, arg2, function () {
              $.isFunction(arg3) && arg3.call(this);
            });
            return self;
          }
          $self.closest('.fht-table-wrapper').hide();
          $.isFunction(arg3) && arg3.call(this);
          return self;
        },
        destroy: function () {
          var $self = $(this), self = this, $wrapper = $self.closest('.fht-table-wrapper');
          $self.insertBefore($wrapper).removeAttr('style').append($wrapper.find('tfoot')).removeClass('fht-table fht-table-init').find('.fht-cell').remove();
          $wrapper.remove();
          return self;
        }
      };
    // private methods
    var helpers = {
        _isTable: function ($obj) {
          var $self = $obj, hasTable = $self.is('table'), hasThead = $self.find('thead').length > 0, hasTbody = $self.find('tbody').length > 0;
          if (hasTable && hasThead && hasTbody) {
            return true;
          }
          return false;
        },
        _bindScroll: function ($obj) {
          var $self = $obj, $wrapper = $self.closest('.fht-table-wrapper'), $thead = $self.siblings('.fht-thead'), $tfoot = $self.siblings('.fht-tfoot');
          $self.bind('scroll', function () {
            if (settings.fixedColumns > 0) {
              var $fixedColumns = $wrapper.find('.fht-fixed-column');
              $fixedColumns.find('.fht-tbody table').css({ 'margin-top': -$self.scrollTop() });
            }
            $thead.find('table').css({ 'margin-left': -this.scrollLeft });
            if (settings.footer || settings.cloneHeadToFoot) {
              $tfoot.find('table').css({ 'margin-left': -this.scrollLeft });
            }
          });
        },
        _fixHeightWithCss: function ($obj, tableProps) {
          if (settings.includePadding) {
            $obj.css({ 'height': $obj.height() + tableProps.border });
          } else {
            $obj.css({ 'height': $obj.parent().height() + tableProps.border });
          }
        },
        _fixWidthWithCss: function ($obj, tableProps, width) {
          if (settings.includePadding) {
            $obj.each(function () {
              $(this).css({ 'width': width == undefined ? $(this).width() + tableProps.border : width + tableProps.border });
            });
          } else {
            $obj.each(function () {
              $(this).css({ 'width': width == undefined ? $(this).parent().width() + tableProps.border : width + tableProps.border });
            });
          }
        },
        _setupFixedColumn: function ($obj, obj, tableProps) {
          var $self = $obj, $wrapper = $self.closest('.fht-table-wrapper'), $fixedBody = $wrapper.find('.fht-fixed-body'), $fixedColumn = $wrapper.find('.fht-fixed-column'), $thead = $('<div class="fht-thead"><table class="fht-table"><thead><tr></tr></thead></table></div>'), $tbody = $('<div class="fht-tbody"><table class="fht-table"><tbody></tbody></table></div>'), $tfoot = $('<div class="fht-tfoot"><table class="fht-table"><tfoot><tr></tr></tfoot></table></div>'), fixedBodyWidth = $wrapper.width(), fixedBodyHeight = $fixedBody.find('.fht-tbody').height() - settings.scrollbarOffset, $firstThChildren, $firstTdChildren, fixedColumnWidth, $newRow, firstTdChildrenSelector;
          $thead.find('table.fht-table').addClass(settings.originalTable.attr('class'));
          $tbody.find('table.fht-table').addClass(settings.originalTable.attr('class'));
          $tfoot.find('table.fht-table').addClass(settings.originalTable.attr('class'));
          $firstThChildren = $fixedBody.find('.fht-thead thead tr > *:lt(' + settings.fixedColumns + ')');
          fixedColumnWidth = settings.fixedColumns * tableProps.border;
          $firstThChildren.each(function () {
            fixedColumnWidth += $(this).outerWidth(true);
          });
          // Fix cell heights
          helpers._fixHeightWithCss($firstThChildren, tableProps);
          helpers._fixWidthWithCss($firstThChildren, tableProps);
          var tdWidths = [];
          $firstThChildren.each(function () {
            tdWidths.push($(this).width());
          });
          firstTdChildrenSelector = 'tbody tr > *:not(:nth-child(n+' + (settings.fixedColumns + 1) + '))';
          $firstTdChildren = $fixedBody.find(firstTdChildrenSelector).each(function (index) {
            helpers._fixHeightWithCss($(this), tableProps);
            helpers._fixWidthWithCss($(this), tableProps, tdWidths[index % settings.fixedColumns]);
          });
          // clone header
          $thead.appendTo($fixedColumn).find('tr').append($firstThChildren.clone());
          $tbody.appendTo($fixedColumn).css({
            'margin-top': -1,
            'height': fixedBodyHeight + tableProps.border
          });
          $firstTdChildren.each(function (index) {
            if (index % settings.fixedColumns == 0) {
              $newRow = $('<tr></tr>').appendTo($tbody.find('tbody'));
              if (settings.altClass && $(this).parent().hasClass(settings.altClass)) {
                $newRow.addClass(settings.altClass);
              }
            }
            $(this).clone().appendTo($newRow);
          });
          // set width of fixed column wrapper
          $fixedColumn.css({
            'height': 0,
            'width': fixedColumnWidth
          });
          // bind mousewheel events
          var maxTop = $fixedColumn.find('.fht-tbody .fht-table').height() - $fixedColumn.find('.fht-tbody').height();
          $fixedColumn.find('.fht-tbody .fht-table').bind('mousewheel', function (event, delta, deltaX, deltaY) {
            if (deltaY == 0) {
              return;
            }
            var top = parseInt($(this).css('marginTop'), 10) + (deltaY > 0 ? 120 : -120);
            if (top > 0) {
              top = 0;
            }
            if (top < -maxTop) {
              top = -maxTop;
            }
            $(this).css('marginTop', top);
            $fixedBody.find('.fht-tbody').scrollTop(-top).scroll();
            return false;
          });
          // set width of body table wrapper
          $fixedBody.css({ 'width': fixedBodyWidth });
          // setup clone footer with fixed column
          if (settings.footer == true || settings.cloneHeadToFoot == true) {
            var $firstTdFootChild = $fixedBody.find('.fht-tfoot tr > *:lt(' + settings.fixedColumns + ')'), footwidth;
            helpers._fixHeightWithCss($firstTdFootChild, tableProps);
            $tfoot.appendTo($fixedColumn).find('tr').append($firstTdFootChild.clone());
            // Set (view width) of $tfoot div to width of table (this accounts for footers with a colspan)
            footwidth = $tfoot.find('table').innerWidth();
            $tfoot.css({
              'top': settings.scrollbarOffset,
              'width': footwidth
            });
          }
        },
        _setupTableFooter: function ($obj, obj, tableProps) {
          var $self = $obj, $wrapper = $self.closest('.fht-table-wrapper'), $tfoot = $self.find('tfoot'), $divFoot = $wrapper.find('div.fht-tfoot');
          if (!$divFoot.length) {
            if (settings.fixedColumns > 0) {
              $divFoot = $('<div class="fht-tfoot"><table class="fht-table"></table></div>').appendTo($wrapper.find('.fht-fixed-body'));
            } else {
              $divFoot = $('<div class="fht-tfoot"><table class="fht-table"></table></div>').appendTo($wrapper);
            }
          }
          $divFoot.find('table.fht-table').addClass(settings.originalTable.attr('class'));
          switch (true) {
          case !$tfoot.length && settings.cloneHeadToFoot == true && settings.footer == true:
            var $divHead = $wrapper.find('div.fht-thead');
            $divFoot.empty();
            $divHead.find('table').clone().appendTo($divFoot);
            break;
          case $tfoot.length && settings.cloneHeadToFoot == false && settings.footer == true:
            $divFoot.find('table').append($tfoot).css({ 'margin-top': -tableProps.border });
            helpers._setupClone($divFoot, tableProps.tfoot);
            break;
          }
        },
        _getTableProps: function ($obj) {
          var tableProp = {
              thead: {},
              tbody: {},
              tfoot: {},
              border: 0
            }, borderCollapse = 1;
          if (settings.borderCollapse == true) {
            borderCollapse = 2;
          }
          tableProp.border = ($obj.find('th:first-child').outerWidth() - $obj.find('th:first-child').innerWidth()) / borderCollapse;
          $obj.find('thead tr:first-child > *').each(function (index) {
            tableProp.thead[index] = $(this).width() + tableProp.border;
          });
          $obj.find('tfoot tr:first-child > *').each(function (index) {
            tableProp.tfoot[index] = $(this).width() + tableProp.border;
          });
          $obj.find('tbody tr:first-child > *').each(function (index) {
            tableProp.tbody[index] = $(this).width() + tableProp.border;
          });
          return tableProp;
        },
        _setupClone: function ($obj, cellArray) {
          var $self = $obj, selector = $self.find('thead').length ? 'thead tr:first-child > *' : $self.find('tfoot').length ? 'tfoot tr:first-child > *' : 'tbody tr:first-child > *', $cell;
          $self.find(selector).each(function (index) {
            $cell = $(this).find('div.fht-cell').length ? $(this).find('div.fht-cell') : $('<div class="fht-cell"></div>').appendTo($(this));
            $cell.css({ 'width': parseInt(cellArray[index], 10) });
            /*
                     * Fixed Header and Footer should extend the full width
                     * to align with the scrollbar of the body
                     */
            if (!$(this).closest('.fht-tbody').length && $(this).is(':last-child') && !$(this).closest('.fht-fixed-column').length) {
              var padding = Math.max(($(this).innerWidth() - $(this).width()) / 2, settings.scrollbarOffset);
              $(this).css({ 'padding-right': parseInt($(this).css('padding-right')) + padding + 'px' });
            }
          });
        },
        _isPaddingIncludedWithWidth: function () {
          var $obj = $('<table class="fht-table"><tr><td style="padding: 10px; font-size: 10px;">test</td></tr></table>'), defaultHeight, newHeight;
          $obj.addClass(settings.originalTable.attr('class'));
          $obj.appendTo('body');
          defaultHeight = $obj.find('td').height();
          $obj.find('td').css('height', $obj.find('tr').height());
          newHeight = $obj.find('td').height();
          $obj.remove();
          if (defaultHeight != newHeight) {
            return true;
          } else {
            return false;
          }
        },
        _getScrollbarWidth: function () {
          var scrollbarWidth = 0;
          if (!scrollbarWidth) {
            if (/msie/.test(navigator.userAgent.toLowerCase())) {
              var $textarea1 = $('<textarea cols="10" rows="2"></textarea>').css({
                  position: 'absolute',
                  top: -1000,
                  left: -1000
                }).appendTo('body'), $textarea2 = $('<textarea cols="10" rows="2" style="overflow: hidden;"></textarea>').css({
                  position: 'absolute',
                  top: -1000,
                  left: -1000
                }).appendTo('body');
              scrollbarWidth = $textarea1.width() - $textarea2.width() + 2;
              // + 2 for border offset
              $textarea1.add($textarea2).remove();
            } else {
              var $div = $('<div />').css({
                  width: 100,
                  height: 100,
                  overflow: 'auto',
                  position: 'absolute',
                  top: -1000,
                  left: -1000
                }).prependTo('body').append('<div />').find('div').css({
                  width: '100%',
                  height: 200
                });
              scrollbarWidth = 100 - $div.width();
              $div.parent().remove();
            }
          }
          return scrollbarWidth;
        }
      };
    // if a method as the given argument exists
    if (methods[method]) {
      // call the respective method
      return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));  // if an object is given as method OR nothing is given as argument
    } else if (typeof method === 'object' || !method) {
      // call the initialization method
      return methods.init.apply(this, arguments);  // otherwise
    } else {
      // trigger an error
      $.error('Method "' + method + '" does not exist in fixedHeaderTable plugin!');
    }
  };
}(jQuery));
if (window.matchMedia) {
  // chrome & safari (ff supports it but doesn't implement it the way we need)
  var mediaQueryList = window.matchMedia('print');
  mediaQueryList.addListener(function (mql) {
    if (mql.matches) {
      reflowForPrinting();
    } else {
      reflowAfterPrinting();
    }
  });
}
window.addEventListener('beforeprint', function (ev) {
  reflowForPrinting();
});
window.addEventListener('afterprint', function (ev) {
  reflowAfterPrinting();
});
function reflowForPrinting() {
  if (typeof Highcharts.charts !== 'undefined') {
    console.log('Resizing charts ready for printing', new Date());
    reflowTheseCharts(Highcharts.charts);
  }
}
function reflowAfterPrinting() {
  if (typeof Highcharts.charts !== 'undefined') {
    console.log('Resizing charts back to screen size after printing', new Date());
    reflowTheseCharts(Highcharts.charts);
  }
}
function reflowTheseCharts(charts) {
  charts.forEach(function (chart) {
    // I'm assuming this check is quicker to execute than a chart 
    // reflow so this check is actually saving time...
    if (typeof chart !== 'undefined') {
      console.log('reflowing chart ');
      //chart.setSize(800, 600);
      chart.reflow();
    }
  });
}
if (location.href.indexOf('printmode=') >= 0) {
  var styles = document.createElement('link');
  styles.rel = 'stylesheet';
  styles.type = 'text/css';
  styles.media = 'all';
  styles.href = 'styles/nspdfstyles.css';
  document.getElementsByTagName('head')[0].appendChild(styles);
}