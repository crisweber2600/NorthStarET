'use strict'

angular
	.module('theme.services', [])
	.factory('Admin', [
		'$resource', 'webApiBaseUrl', function Admin($resource, webApiBaseUrl) {
		    return $resource(webApiBaseUrl + '/api/staff/:id');
		}
	])
	.factory('Assessment', [
		'$resource', 'webApiBaseUrl', function Assessment($resource, webApiBaseUrl) {
		    return $resource(webApiBaseUrl + '/api/assessment/:id');
		}
	])

	.service('$global', [
		'$rootScope', 'EnquireService', '$document', function ($rootScope, EnquireService, $document) {
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
		        EnquireService.register("screen and (max-width: 767px)", {
		            match: function () {
		                $rootScope.$broadcast('globalStyles:maxWidth767', true);
		            },
		            unmatch: function () {
		                $rootScope.$broadcast('globalStyles:maxWidth767', false);
		            }
		        });
		    });

		    this.get = function (key) { return this.settings[key]; };
		    this.set = function (key, value) {
		        this.settings[key] = value;
		        $rootScope.$broadcast('globalStyles:changed', { key: key, value: this.settings[key] });
		        $rootScope.$broadcast('globalStyles:changed:' + key, this.settings[key]);
		    };
		    this.values = function () { return this.settings; };
		}
	])
	.factory('pinesNotifications', function () {
	    return {
	        notify: function (args) {
	            var notification = new PNotify(args);
	            notification.notify = notification.update;
	            return notification;
	        },
	    }
	})
	.factory('progressLoader', function () {
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
	    }
	})
	.factory('EnquireService', [
		'$window', function ($window) {
		    return $window.enquire;
		}
	])
	.factory('$bootbox', [
		'$uibModal', function ($uibModal) {
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

		            var modalInstance = $uibModal.open({
		                template: $(this).find('.modal-content').html()
		            });
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
	])
	.service('lazyLoad', [
		'$q', '$timeout', function ($q, $t) {
		    var deferred = $q.defer();
		    var promise = deferred.promise;
		    this.load = function (files) {
		        angular.forEach(files, function (file) {
		            if (file.indexOf('.js') > -1) { // script
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
	])
	.filter('safe_html', [
		'$sce', function ($sce) {
		    return function (val) {
		        return $sce.trustAsHtml(val);
		    };


		}
	])
	.filter('range', function () {
	    return function (input, min, max) {
	        min = parseInt(min); //Make string input int
	        max = parseInt(max);
	        for (var i = min; i <= max; i++)
	            input.push(i);
	        return input;
	    };
	})
	.service('formService', function formService() {

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
	}
	)
	.service('nsSectionDataEntryService', [
			'$http', 'pinesNotifications', 'webApiBaseUrl', function ($http, pinesNotifications, webApiBaseUrl) {
			    //this.options = {};

			    this.getSectionResults = function (assessmentId, sectionId, benchmarkDateId) {
			        return $http.get(webApiBaseUrl + '/api/sectiondataentry/getassessmentresults/' + assessmentId + "/" + sectionId + "/" + benchmarkDateId);
			    };

			    this.loadLookupFields = function (assessmentId) {
			        return $http.get(webApiBaseUrl + '/api/assessment/GetLookupFieldsForAssessment/' + assessmentId);
			    }

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
			    }

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
			                    column = 'FieldResults[' + column + '].StringValue';//shouldnt even be used in sorting
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
			                if (sortArray[j].indexOf("-") === 0) {
			                    if (columnIndex > -1) {
			                        headerClassArray[columnIndex] = "fa";
			                    }
			                    else if (column === 'FirstName') {
			                        staticColumnsObj.firstNameHeaderClass = "fa";
			                    }
			                    else if (column === 'LastName') {
			                        staticColumnsObj.lastNameHeaderClass = "fa";
			                    }
			                    sortArray.splice(j, 1);
			                } else {
			                    if (columnIndex > -1) {
			                        headerClassArray[columnIndex] = "fa fa-chevron-down";
			                    }
			                    else if (column === 'FirstName') {
			                        staticColumnsObj.firstNameHeaderClass = "fa fa-chevron-down";
			                    }
			                    else if (column === 'LastName') {
			                        staticColumnsObj.lastNameHeaderClass = "fa fa-chevron-down";
			                    }
			                    sortArray[j] = "-" + sortArray[j];
			                }
			                break;
			            }
			        }
			        if (!bFound) {
			            sortArray.push(column);

			            if (columnIndex > -1) {
			                headerClassArray[columnIndex] = "fa fa-chevron-up";
			            }
			            else if (column === 'FirstName') {
			                staticColumnsObj.firstNameHeaderClass = "fa fa-chevron-up";
			            }
			            else if (column === 'LastName') {
			                staticColumnsObj.lastNameHeaderClass = "fa fa-chevron-up";
			            }
			        }
			    };

			    this.deleteStudentTestResult = function (assessmentId, studentResult) {

			        var returnObject = {
			            StudentResult: studentResult,
			            AssessmentId: assessmentId
			        }

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


			        $http.post(webApiBaseUrl + "/api/assessment/DeleteAssessmentResult", returnObject).success(function (data) {
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
			}]
	)

    
    
    .service('nsSelect2RemoteOptions', [
        'authService', 'nsFilterOptionsService', 'webApiBaseUrl', '$routeParams', function (authService, nsFilterOptionsService, webApiBaseUrl, $routeParams) {
            this.chooseSectionRemoteOptions = {
                placeholder: "Search for a section",
                minimumInputLength: 3,
                width: 'resolve',
                multiple: false,
                id: function (section) { return section.id; },
                ajax: {
                    url: webApiBaseUrl + "/api/teammeeting/getsectionsfordropdown",
                    dataType: 'json',
                    quietMillis: 250,
                    //headers: {
                    //    "Authorization": 'Bearer ' + AccessToken.token.access_token,
                    //    "Content-Type": "application/json",
                    //},
                    transport: function (params) {
                        params.beforeSend = function (request) {
                            request.setRequestHeader("Authorization", 'Bearer ' + authService.token());
                        };
                        return $.ajax(params);
                    },
                    data: function (term, page) { // page is the one-based page number tracked by Select2
                        return {
                            pageNo: page, //search term
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
                dropdownCssClass: "bigdrop", // apply css that makes the dropdown taller
                escapeMarkup: function (m) { return m; } // we do not want to escape markup since we are displaying html in results
            };
            this.quickSearchSectionsRemoteOptions = {
                placeholder: "Search for a section",
                minimumInputLength: 3,
                width: 'resolve',
                multiple: false,
                id: function (section) { return section.id; },
                ajax: {
                    url: webApiBaseUrl + "/api/section/QuickSearchSections",
                    dataType: 'json',
                    quietMillis: 250,
                    transport: function (params) {
                        params.beforeSend = function (request) {
                            request.setRequestHeader("Authorization", 'Bearer ' + authService.token());
                        };
                        return $.ajax(params);
                    },
                    data: function (term, page) { // page is the one-based page number tracked by Select2
                        return {
                            schoolYear: nsFilterOptionsService.options.selectedSchoolYear.id,
                            searchString: term
                        };
                    },
                    results: function (data) {
                        return { results: data };
                    }
                },
                dropdownCssClass: "bigdrop", // apply css that makes the dropdown taller
                escapeMarkup: function (m) { return m; } // we do not want to escape markup since we are displaying html in results
            };
            this.WorkshopRemoteOptions = {
                placeholder: "Search for a workshop",
                minimumInputLength: 0,
                width: 'resolve',
                multiple: false,
                id: function (item) { return item.id; },
                ajax: {
                    url: webApiBaseUrl + "/api/interventiontoolkit/Workshops",
                    dataType: 'json',
                    quietMillis: 250,
                    transport: function (params) {
                        params.beforeSend = function (request) {
                            request.setRequestHeader("Authorization", 'Bearer ' + authService.token());
                        };
                        return $.ajax(params);
                    },
                    data: function (term) { // page is the one-based page number tracked by Select2
                        return {
                            searchString: term
                        };
                    },
                    results: function (data) {
                        return { results: data };
                    }
                },
                dropdownCssClass: "bigdrop", // apply css that makes the dropdown taller
                escapeMarkup: function (m) { return m; } // we do not want to escape markup since we are displaying html in results
            };
            this.quickSearchSectionsAllSchoolYearsRemoteOptions = {
                placeholder: "Search for a section",
                minimumInputLength: 3,
                width: 'resolve',
                multiple: false,
                id: function (section) { return section.id; },
                ajax: {
                    url: webApiBaseUrl + "/api/section/QuickSearchSectionsAllSchoolYears",
                    dataType: 'json',
                    quietMillis: 250,
                    transport: function (params) {
                        params.beforeSend = function (request) {
                            request.setRequestHeader("Authorization", 'Bearer ' + authService.token());
                        };
                        return $.ajax(params);
                    },
                    data: function (term) { // page is the one-based page number tracked by Select2
                        return {
                            searchString: term
                        };
                    },
                    results: function (data) {
                        return { results: data };
                    }
                },
                dropdownCssClass: "bigdrop", // apply css that makes the dropdown taller
                escapeMarkup: function (m) { return m; } // we do not want to escape markup since we are displaying html in results
            };
            this.CoTeacherRemoteOptions = {
                placeholder: "Search for a teacher",
                minimumInputLength: 3,
                width: 'resolve',
                multiple: true,
                id: function (staff) { return staff.id; },
                ajax: {
                    url: webApiBaseUrl + "/api/section/getcoteachersfordropdown",
                    dataType: 'json',
                    quietMillis: 250,
                    //headers: {
                    //    "Authorization": 'Bearer ' + AccessToken.token.access_token,
                    //    "Content-Type": "application/json",
                    //},
                    transport: function (params) {
                        params.beforeSend = function (request) {
                            request.setRequestHeader("Authorization", 'Bearer ' + authService.token());
                        };
                        return $.ajax(params);
                    },
                    data: function (term, page) { // page is the one-based page number tracked by Select2
                        return {
                            pageNo: page, //search term
                            searchString: term
                        };
                    },
                    results: function (data, page) {
                        //var more = (page * 10) < data.total; // whether or not there are more results available

                        // notice we return the value of more so Select2 knows if more results can be loaded
                        return { results: data };
                    }
                },
                dropdownCssClass: "bigdrop", // apply css that makes the dropdown taller
                escapeMarkup: function (m) { return m; } // we do not want to escape markup since we are displaying html in results
            };
            this.CoInterventionistsRemoteOptions = {
                placeholder: "Search for an interventionist",
                minimumInputLength: 3,
                width: 'resolve',
                multiple: true,
                id: function (staff) { return staff.id; },
                ajax: {
                    url: webApiBaseUrl + "/api/InterventionGroup/getinterventionistsfordropdown",
                    dataType: 'json',
                    quietMillis: 250,
                    //headers: {
                    //    "Authorization": 'Bearer ' + AccessToken.token.access_token,
                    //    "Content-Type": "application/json",
                    //},
                    transport: function (params) {
                        params.beforeSend = function (request) {
                            request.setRequestHeader("Authorization", 'Bearer ' + authService.token());
                        };
                        return $.ajax(params);
                    },
                    data: function (term, page) { // page is the one-based page number tracked by Select2
                        return {
                            pageNo: page, //search term
                            searchString: term
                        };
                    },
                    results: function (data, page) {
                        //var more = (page * 10) < data.total; // whether or not there are more results available

                        // notice we return the value of more so Select2 knows if more results can be loaded
                        return { results: data };
                    }
                },
                dropdownCssClass: "bigdrop", // apply css that makes the dropdown taller
                escapeMarkup: function (m) { return m; } // we do not want to escape markup since we are displaying html in results
            };
            this.StaffGroupRemoteOptions = {
                placeholder: "Search for a staff group",
                minimumInputLength: 3,
                width: 'resolve',
                multiple: true,
                id: function (staff) { return staff.id; },
                ajax: {
                    url: webApiBaseUrl + "/api/teammeeting/getstaffgroupsfordropdown",
                    dataType: 'json',
                    quietMillis: 250,
                    //headers: {
                    //    "Authorization": 'Bearer ' + AccessToken.token.access_token,
                    //    "Content-Type": "application/json",
                    //},
                    transport: function (params) {
                        params.beforeSend = function (request) {
                            request.setRequestHeader("Authorization", 'Bearer ' + authService.token());
                        };
                        return $.ajax(params);
                    },
                    data: function (term, page) { // page is the one-based page number tracked by Select2
                        return {
                            pageNo: page, //search term
                            searchString: term
                        };
                    },
                    results: function (data, page) {
                        //var more = (page * 10) < data.total; // whether or not there are more results available

                        // notice we return the value of more so Select2 knows if more results can be loaded
                        return { results: data };
                    }
                },
                dropdownCssClass: "bigdrop", // apply css that makes the dropdown taller
                escapeMarkup: function (m) { return m; } // we do not want to escape markup since we are displaying html in results
            };
            this.StudentSpedLabelRemoteOptions = {
                placeholder: "Search for a Special Education Label",
                minimumInputLength: 1,
                width: 'resolve',
                multiple: true,
                id: function (label) { return label.id; },
                ajax: {
                    url: webApiBaseUrl + "/api/student/GetStudentSpedLabelLookups",
                    dataType: 'json',
                    quietMillis: 250,
                    //headers: {
                    //    "Authorization": 'Bearer ' + AccessToken.token.access_token,
                    //    "Content-Type": "application/json",
                    //},
                    transport: function (params) {
                        params.beforeSend = function (request) {
                            request.setRequestHeader("Authorization", 'Bearer ' + authService.token());
                        };
                        return $.ajax(params);
                    },
                    data: function (term, page) { // page is the one-based page number tracked by Select2
                        return {
                            pageNo: page, //search term
                            searchString: term
                        };
                    },
                    results: function (data, page) {
                        //var more = (page * 10) < data.total; // whether or not there are more results available

                        // notice we return the value of more so Select2 knows if more results can be loaded
                        return { results: data };
                    }
                },
                dropdownCssClass: "bigdrop", // apply css that makes the dropdown taller
                escapeMarkup: function (m) { return m; } // we do not want to escape markup since we are displaying html in results
            };
            this.InterventionTypeRemoteOptions = {
                placeholder: "Search for an intervention type",
                minimumInputLength: 0,
                width: 'resolve',
                initSelection: function (element, callback) {
                    var initialSelection = $(element).data('$ngModelController').$modelValue;
                    if (angular.isObject(initialSelection)) {
                        callback(initialSelection)
                    }
                },
                //id: function (staff) { return staff.id; },
                ajax: {
                    url: webApiBaseUrl + "/api/InterventionGroup/getinterventionsfordropdown",
                    transport: function (params) {
                        params.beforeSend = function (request) {
                            request.setRequestHeader("Authorization", 'Bearer ' + authService.token());
                        };
                        return $.ajax(params);
                    },
                    dataType: 'json',
                    quietMillis: 250,
                    data: function (term, page) { // page is the one-based page number tracked by Select2
                        return {
                            pageNo: page, //search term
                            searchString: term
                        };
                    },
                    results: function (data, page) {
                        //var more = (page * 10) < data.total; // whether or not there are more results available

                        // notice we return the value of more so Select2 knows if more results can be loaded
                        return { results: data };
                    }
                },
                dropdownCssClass: "bigdrop", // apply css that makes the dropdown taller
                escapeMarkup: function (m) { return m; } // we do not want to escape markup since we are displaying html in results
            };
            this.PrimaryInterventionistRemoteOptions = {
                placeholder: "Search for an interventionist",
                minimumInputLength: 3,
                width: 'resolve',
                initSelection: function (element, callback) {
                    var initialSelection = $(element).data('$ngModelController').$modelValue;
                    if (angular.isObject(initialSelection)) {
                        callback(initialSelection)
                    }
                },
                //id: function (staff) { return staff.id; },
                ajax: {
                    url: webApiBaseUrl + "/api/InterventionGroup/GetInterventionistsForDropdown",
                    transport: function (params) {
                        params.beforeSend = function (request) {
                            request.setRequestHeader("Authorization", 'Bearer ' + authService.token());
                        };
                        return $.ajax(params);
                    },
                    dataType: 'json',
                    quietMillis: 250,
                    data: function (term, page) { // page is the one-based page number tracked by Select2
                        return {
                            pageNo: page, //search term
                            searchString: term
                        };
                    },
                    results: function (data, page) {
                        //var more = (page * 10) < data.total; // whether or not there are more results available

                        // notice we return the value of more so Select2 knows if more results can be loaded
                        return { results: data };
                    }
                },
                dropdownCssClass: "bigdrop", // apply css that makes the dropdown taller
                escapeMarkup: function (m) { return m; } // we do not want to escape markup since we are displaying html in results
            };
            this.TeacherRemoteOptions = {
                placeholder: "Search for a teacher",
                minimumInputLength: 3,
                width: 'resolve',
                initSelection: function (element, callback) {
                    var initialSelection = $(element).data('$ngModelController').$modelValue;
                    if (angular.isObject(initialSelection)) {
                        callback(initialSelection)
                    }
                },
                //id: function (staff) { return staff.id; },
                ajax: {
                    url: webApiBaseUrl + "/api/section/getcoteachersfordropdown",
                    transport: function (params) {
                        params.beforeSend = function (request) {
                            request.setRequestHeader("Authorization", 'Bearer ' + authService.token());
                        };
                        return $.ajax(params);
                    },
                    dataType: 'json',
                    quietMillis: 250,
                    data: function (term, page) { // page is the one-based page number tracked by Select2
                        return {
                            pageNo: page, //search term
                            searchString: term
                        };
                    },
                    results: function (data, page) {
                        //var more = (page * 10) < data.total; // whether or not there are more results available

                        // notice we return the value of more so Select2 knows if more results can be loaded
                        return { results: data };
                    }
                },
                dropdownCssClass: "bigdrop", // apply css that makes the dropdown taller
                escapeMarkup: function (m) { return m; } // we do not want to escape markup since we are displaying html in results
            };
            this.StaffQuickSearchRemoteOptions = {
                placeholder: "Search for staff",
                minimumInputLength: 2,
                width: 'resolve',
                initSelection: function (element, callback) {
                    var initialSelection = $(element).data('$ngModelController').$modelValue;
                    if (angular.isObject(initialSelection)) {
                        callback(initialSelection)
                    }
                },
                //id: function (staff) { return staff.id; },
                ajax: {
                    url: webApiBaseUrl + "/api/staff/quicksearchstaff",
                    transport: function (params) {
                        params.beforeSend = function (request) {
                            request.setRequestHeader("Authorization", 'Bearer ' + authService.token());
                        };
                        return $.ajax(params);
                    },
                    dataType: 'json',
                    quietMillis: 250,
                    data: function (term) { // page is the one-based page number tracked by Select2
                        return {
                            searchString: term
                        };
                    },
                    results: function (data, page) {
                        //var more = (page * 10) < data.total; // whether or not there are more results available

                        // notice we return the value of more so Select2 knows if more results can be loaded
                        return { results: data };
                    }
                },
                formatResult: function (staff) {
                    var labelClass = 'label-success';
                    var inactiveLabel = "<span class='badge badge-default'>Inactive</span>";
                    var interventionistLabel = "<span class='badge badge-primary'>Interventionist</span>";
                    var disabledLabel = "<span class='badge badge-danger'>No Access</span>";

                    if(staff.IsActive) {
                        inactiveLabel = '';
                    }

                    if (!staff.disabled) {
                        disabledLabel = '';
                    }

                    if(!staff.IsInterventionist) {
                        interventionistLabel = '';
                    }

                    var markup = "<div>";
                    markup += "<b>" + staff.LastName + ", " + staff.FirstName + "</b> (" + staff.Email + ") | Key: <b>" + staff.TeacherKey + "</b>  " + inactiveLabel + " " + interventionistLabel + " " + disabledLabel;
                    markup += "</div>";
                    return markup;
                },
                formatSelection: function (staff) { return "<b>" + staff.FirstName + " " + staff.LastName + "</b> (" + staff.Email + ") |  Key: <b>" + staff.TeacherKey + "</b> "; },
                dropdownCssClass: "bigdrop", // apply css that makes the dropdown taller
                escapeMarkup: function (m) { return m; } // we do not want to escape markup since we are displaying html in results
            };
            this.StudentQuickSearchRemoteOptions = {
                placeholder: "Search for a student",
                minimumInputLength: 3,
                width: 'resolve',
                initSelection: function (element, callback) {
                    var initialSelection = $(element).data('$ngModelController').$modelValue;
                    if (angular.isObject(initialSelection)) {
                        callback(initialSelection)
                    }
                },
                //id: function (staff) { return staff.id; },
                ajax: {
                    url: webApiBaseUrl + "/api/student/quicksearchstudent",
                    transport: function (params) {
                        params.beforeSend = function (request) {
                            request.setRequestHeader("Authorization", 'Bearer ' + authService.token());
                        };
                        return $.ajax(params);
                    },
                    dataType: 'json',
                    quietMillis: 250,
                    data: function (term) { // page is the one-based page number tracked by Select2
                        return {
                            searchString: term,
                            disableInactiveStudents: false // TODO: just create another one for section??
                        };
                    },
                    results: function (data, page) {
                        //var more = (page * 10) < data.total; // whether or not there are more results available

                        // notice we return the value of more so Select2 knows if more results can be loaded
                        return { results: data };
                    }
                },
                formatResult: function (student) {
                    var inactiveLabel = "<span class='badge badge-default'>Inactive</span>";

                    if (student.IsActive) {
                        inactiveLabel = '';
                    }

                    var markup = "<div>";
                    markup += "<b>" + student.LastName + ", " + student.FirstName + " " + student.MiddleName + "</b> | Key: <b>" + student.StudentIdentifier + "</b>  " + inactiveLabel;
                    markup += "</div>";
                    return markup;
                },
                formatSelection: function (student) { return "<b>" + student.LastName + ", " + student.FirstName + " " + student.MiddleName + "</b> | Key: <b>" + student.StudentIdentifier + "</b>"; },
                dropdownCssClass: "bigdrop", // apply css that makes the dropdown taller
                escapeMarkup: function (m) { return m; } // we do not want to escape markup since we are displaying html in results
            };
            this.StudentDetailedQuickSearchRemoteOptions = {
                placeholder: "Search for a student",
                minimumInputLength: 2,
                width: 'resolve',
                initSelection: function (element, callback) {
                    var initialSelection = $(element).data('$ngModelController').$modelValue;
                    if (angular.isObject(initialSelection)) {
                        callback(initialSelection)
                    }
                },
                //id: function (staff) { return staff.id; },
                ajax: {
                    url: webApiBaseUrl + "/api/student/quicksearchstudentdetailed",
                    transport: function (params) {
                        params.beforeSend = function (request) {
                            request.setRequestHeader("Authorization", 'Bearer ' + authService.token());
                        };
                        return $.ajax(params);
                    },
                    dataType: 'json',
                    quietMillis: 250,
                    data: function (term) { // page is the one-based page number tracked by Select2
                        return {
                            searchString: term,
                            disableInactiveStudents: false // TODO: just create another one for section??
                        };
                    },
                    results: function (data, page) {
                        //var more = (page * 10) < data.total; // whether or not there are more results available

                        // notice we return the value of more so Select2 knows if more results can be loaded
                        return { results: data };
                    }
                },
                formatResult: function (student) {
                    var inactiveLabel = "<span class='badge badge-default'>Inactive</span>";

                    if (student.IsActive) {
                        inactiveLabel = '';
                    }

                    var markup = "<div>";
                    markup += "<b>" + student.LastName + ", " + student.FirstName + " " + student.MiddleName + "</b> | Key: <b>" + student.StudentIdentifier + "</b>  " + inactiveLabel;
                    markup += "<br>" + student.SchoolYearVerbose + " | " + student.SchoolName + " | Grade: <b>" + student.GradeName + "</b>";
                    markup += "<br>" + student.StaffName + " | Section: <b>" + student.SectionName + "</b>";
                    markup += "</div>";
                    return markup;
                },
                formatSelection: function (student) { return "<b>" + student.LastName + ", " + student.FirstName + " " + student.MiddleName + "</b> | Key: <b>" + student.StudentIdentifier + "</b> | Grade: <b>" + student.GradeName + "</b>"; },
                dropdownCssClass: "bigdrop", // apply css that makes the dropdown taller
                escapeMarkup: function (m) { return m; } // we do not want to escape markup since we are displaying html in results
            };
            this.GradeQuickSearchRemoteOptions = {
                placeholder: "Search for a Grade",
                minimumInputLength: 0,
                width: 'resolve',
                multiple: true,
                initSelection: function (element, callback) {
                    var initialSelection = $(element).data('$ngModelController').$modelValue;
                    if (angular.isObject(initialSelection)) {
                        callback(initialSelection)
                    }
                },
                //id: function (staff) { return staff.id; },
                ajax: {
                    url: webApiBaseUrl + "/api/interventiontoolkit/QuickSearchGrades",
                    transport: function (params) {
                        params.beforeSend = function (request) {
                            request.setRequestHeader("Authorization", 'Bearer ' + authService.token());
                        };
                        return $.ajax(params);
                    },
                    dataType: 'json',
                    quietMillis: 250,
                    data: function (term) { // page is the one-based page number tracked by Select2
                        return {
                            searchString: term
                        };
                    },
                    results: function (data, page) {
                        //var more = (page * 10) < data.total; // whether or not there are more results available

                        // notice we return the value of more so Select2 knows if more results can be loaded
                        return { results: data };
                    }
                },
                formatResult: function (grade) {
                    var markup = "<div>" + grade.text + "</div>";
                    return markup;
                },
                formatSelection: function (grade) { return grade.text; },
                dropdownCssClass: "bigdrop", // apply css that makes the dropdown taller
                escapeMarkup: function (m) { return m; } // we do not want to escape markup since we are displaying html in results
            };
            this.SchoolAdminRemoteOptions = {
                placeholder: "Search for a School Administrator",
                minimumInputLength: 0,
                width: 'resolve',
                initSelection: function (element, callback) {
                    var initialSelection = $(element).data('$ngModelController').$modelValue;
                    if (angular.isObject(initialSelection)) {
                        callback(initialSelection)
                    }
                },
                //id: function (staff) { return staff.id; },
                ajax: {
                    url: webApiBaseUrl + "/api/staff/GetSchoolAdminstrators",
                    transport: function (params) {
                        params.beforeSend = function (request) {
                            request.setRequestHeader("Authorization", 'Bearer ' + authService.token());
                        };
                        return $.ajax(params);
                    },
                    dataType: 'json',
                    quietMillis: 250,
                    data: function (term, page) { // page is the one-based page number tracked by Select2
                        return {
                            pageNo: page, //search term
                            searchString: term
                        };
                    },
                    results: function (data, page) {
                        //var more = (page * 10) < data.total; // whether or not there are more results available

                        // notice we return the value of more so Select2 knows if more results can be loaded
                        return { results: data };
                    }
                },
                dropdownCssClass: "bigdrop", // apply css that makes the dropdown taller
                escapeMarkup: function (m) { return m; } // we do not want to escape markup since we are displaying html in results
            };
            this.DistrictAdminRemoteOptions = {
                placeholder: "Search for a District Administrator",
                minimumInputLength: 0,
                width: 'resolve',
                initSelection: function (element, callback) {
                    var initialSelection = $(element).data('$ngModelController').$modelValue;
                    if (angular.isObject(initialSelection)) {
                        callback(initialSelection)
                    }
                },
                //id: function (staff) { return staff.id; },
                ajax: {
                    url: webApiBaseUrl + "/api/staff/GetDistrictAdminstrators",
                    transport: function (params) {
                        params.beforeSend = function (request) {
                            request.setRequestHeader("Authorization", 'Bearer ' + authService.token());
                        };
                        return $.ajax(params);
                    },
                    dataType: 'json',
                    quietMillis: 250,
                    data: function (term, page) { // page is the one-based page number tracked by Select2
                        return {
                            pageNo: page, //search term
                            searchString: term
                        };
                    },
                    results: function (data, page) {
                        //var more = (page * 10) < data.total; // whether or not there are more results available

                        // notice we return the value of more so Select2 knows if more results can be loaded
                        return { results: data };
                    }
                },
                dropdownCssClass: "bigdrop", // apply css that makes the dropdown taller
                escapeMarkup: function (m) { return m; } // we do not want to escape markup since we are displaying html in results
            };
            this.StudentToSectionRemoteOptions = {
                placeholder: "Search for a student",
                minimumInputLength: 3,
                width: 'resolve',
                initSelection: function (element, callback) {
                    var id;
                    id = $(element).val();
                    if (id !== "") {
                        return $.ajax({
                            headers: {
                                "Authorization": 'Bearer ' + authService.token(),
                                "Content-Type": "application/json",
                            },
                            url: webApiBaseUrl + "/api/InterventionGroup/getstudentfordropdown",
                            type: "GET",
                            dataType: "json",
                            data: {
                                id: id
                            }
                        }).done(function (data) {
                            var results;
                            results = [];
                            results.push(
                                data
                            );
                            callback(results[0]);
                        });
                    }
                },
                id: function (student) { return student.StudentId; },
                ajax: {
                    url: webApiBaseUrl + "/api/section/getstudentquicksearch",
                    transport: function (params) {
                        params.beforeSend = function (request) {
                            request.setRequestHeader("Authorization", 'Bearer ' + authService.token());
                        };
                        return $.ajax(params);
                    },
                    dataType: 'json',
                    quietMillis: 250,
                    data: function (term, page) { // page is the one-based page number tracked by Select2
                        return {
                            strSearch: term, //search term
                            expandSearch: false, // page size
                            page: page, // page number
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
                    var markup = "<table class='movie-result'><tr>";
                    //if (movie.posters !== undefined && movie.posters.thumbnail !== undefined) {
                    //	markup += "<td class='movie-image'><img src='" + movie.posters.thumbnail + "'/></td>";
                    //}
                    markup += "<td class='movie-info'><div class='movie-title'>" + student.FirstName + " " + student.LastName + "</div>";
                    //if (movie.critics_consensus !== undefined) {
                    //	markup += "<div class='movie-synopsis'>" + movie.critics_consensus + "</div>";
                    //}
                    //else if (movie.synopsis !== undefined) {
                    //	markup += "<div class='movie-synopsis'>" + movie.synopsis + "</div>";
                    //}
                    markup += "</td></tr></table>";
                    return markup;
                },
                formatSelection: function (student) { return student.FirstName + " " + student.LastName; },
                dropdownCssClass: "bigdrop", // apply css that makes the dropdown taller
                escapeMarkup: function (m) { return m; } // we do not want to escape markup since we are displaying html in results
            };
        }
    ])
        .service('nsSectionService', [
			'$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
			    this.getSectionList = function (schoolYear, schoolId, gradeId, teacherId) {
			        var returnObject = { SchoolYear: schoolYear, SchoolId: schoolId, GradeId: gradeId, TeacherId: teacherId };

			        return $http.post(webApiBaseUrl + "/api/section/GetSectionList", returnObject);
			    }
			    this.getSection = function (sectionId) {

			        return $http.get(webApiBaseUrl + "/api/section/GetSection/" + sectionId);
			    }
			    this.loadGrades = function () {
			        return $http.get(webApiBaseUrl + "/api/section/LoadGrades");
			    }
			    this.deleteSection = function (sectionId) {
			        var returnObject = { Id: sectionId };

			        return $http.post(webApiBaseUrl + "/api/section/deletesection", returnObject);
			    }

			    this.saveSection = function (section) {
			        return $http.post(webApiBaseUrl + "/api/section/savesection", section);
			    }
			}])

    

.service('nsPinesService', ['pinesNotifications', function(pinesNotifications) {
    this.dataSavedSuccessfully = function () {
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
            title: "Processed item",
            text: "0 of " + interval,
            type: 'info',
            icon: 'fa fa-spin fa-refresh',
            hide: false,
            closer: false,
            sticker: false,
            opacity: 1,
            shadow: true,
            width: "200px"
        });
    }

    self.incrementInterval = function () {
        if (self.intervalMessage) {
            self.currentStep++;
            var options = { text: self.currentStep + " of " + self.interval};
            // finalize if so
            if (self.currentStep >= self.interval) {
                options.title = "All complete!";
                options.type = "success";
                options.hide = true;
                options.closer = true;
                options.sticker = true;
                options.icon = 'fa  fa-check-square-o';
                options.opacity = 1;
                options.shadow = true;
            } 

            self.intervalMessage.notify(options);
       }
    }

    this.buildMessage = function (title, text, type) {
        pinesNotifications.notify({
            title: title,
            text: text,
            type: type
        });
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
            this.generalLoadingError = function() {
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
}])
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
