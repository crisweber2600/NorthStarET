(function () {
	'use strict';

	angular
		.module('studentDashboardModule', [])
		.directive('studentDashboard', [
			'$compile', '$templateCache', '$http', 'NSStudent', 'NSStudentAttributeLookups','nsFilterOptionsService', 'NSAssesssmentLineGraphManager', 'NSStudentInterventionManager', 'NSStudentTMNotesManager', 'NSStudentNotesManager', 'NSStudentAssessmentFieldManager', 'nsPinesService', 'webApiBaseUrl', 'ckEditorSettings', '$bootbox', '$routeParams', 'authService', 'spinnerService', '$q','$timeout', 'NSUserInfoService',
			function ($compile, $templateCache, $http, NSStudent, NSStudentAttributeLookups, nsFilterOptionsService, NSAssesssmentLineGraphManager, NSStudentInterventionManager, NSStudentTMNotesManager, NSStudentNotesManager, NSStudentAssessmentFieldManager, nsPinesService, webApiBaseUrl, ckEditorSettings, $bootbox, $routeParams, authService, spinnerService, $q, $timeout, NSUserInfoService) {
				return {
					restrict: 'EA',
					templateUrl: 'templates/student-dashboard-directive.html',
					scope: {
						student: '='
					},
					link: function (scope, element, attr) {
						scope.currentUser = NSUserInfoService.currentUser;
						scope.filterOptions = nsFilterOptionsService.options;
						scope.settings = { studentLoaded: false, selectedStudent: null };
						scope.studentAssessmentFieldsManager = new NSStudentAssessmentFieldManager();
						scope.studentTMNotesManager = new NSStudentTMNotesManager();
						scope.studentNotesManager = new NSStudentNotesManager();
						scope.studentDataManager = new NSStudentInterventionManager();
						scope.ClassLineGraphDataManagers = [];
						scope.fullStudent = {};
						scope.allAttributes = new NSStudentAttributeLookups();

						scope.getAttributeValue = function (selectedValue, attribute) {
						    for (var i = 0; i < attribute.LookupValues.length; i++) {
						        if (selectedValue == attribute.LookupValues[i].LookupValueId) {
						            return attribute.LookupValues[i].LookupValue;
						        }
						    }
						}

                        // stupid highcharts won't work right unless they are the first goddam tab
						//scope.lineGraphsSelected = function () {
						//    $timeout(function () {
						//        $(Highcharts.charts).each(function (i, chart) {
						//            if (chart) {
						//                chart.reflow();
						//            }
						//        });
						//    }, 1000);
						//}

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
							    // added on 9/27/2020
							    scope.fullStudent = new NSStudent(scope.student.id);
								LoadLineGraphs(scope.settings.selectedStudent);
								scope.studentTMNotesManager.LoadData(scope.settings.selectedStudent.id);
							}
						});

						scope.saveNote = function (noteId, noteHtml, teamMeetingId, studentId, meeting) {

							if (noteHtml === '' || noteHtml == null) {
								$bootbox.alert('Please enter a note');
								return;
							}
							scope.studentTMNotesManager.saveNote(noteId, noteHtml, teamMeetingId, studentId)
								.then(function (response) {
									nsPinesService.dataSavedSuccessfully();
									meeting.NewNoteHtml = '';
									meeting.newNoteEditMode = false;
									// reload the list
									scope.studentTMNotesManager.LoadData(scope.settings.selectedStudent.id);
								});
						}

						scope.saveStudentNote = function (noteId, noteHtml, studentId) {

							if (noteHtml === '' || noteHtml == null) {
								$bootbox.alert('Please enter a note');
								return;
							}
							scope.studentNotesManager.saveNote(noteId, noteHtml, studentId)
								.then(function (response) {
									nsPinesService.dataSavedSuccessfully();
									// reload the list
									scope.settings.selectedStudent.newNoteEditMode = false;
									scope.settings.selectedStudent.NewNoteHtml = '';
									scope.studentNotesManager.LoadData(scope.settings.selectedStudent.id);
								});
						}

						scope.deleteNote = function (noteId) {
							$bootbox.confirm('Are you sure you want to delete this note?', function (response) {
								if (response) {

									scope.studentTMNotesManager.deleteNote(noteId)
										.then(function (response) {
											nsPinesService.dataDeletedSuccessfully();
											scope.studentTMNotesManager.LoadData(scope.settings.selectedStudent.id);
										});
								}
							});
						};

						scope.deleteStudentNote = function (noteId) {
							$bootbox.confirm('Are you sure you want to delete this note?', function (response) {
								if (response) {

									scope.studentNotesManager.deleteNote(noteId)
										.then(function (response) {
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
								})
							});
						}

					}
				};
			}
		])
		.directive('studentDashboardPrint', [
			'$compile', '$templateCache', '$http', 'NSStudent', 'NSStudentAttributeLookups', 'nsFilterOptionsService', 'NSAssesssmentLineGraphManager', 'NSStudentInterventionManager', 'NSStudentTMNotesManager', 'NSStudentNotesManager', 'NSStudentAssessmentFieldManager', 'nsPinesService', 'webApiBaseUrl', 'ckEditorSettings', '$bootbox', '$routeParams', 'authService', 'spinnerService', '$q', '$timeout', '$location',
			function ($compile, $templateCache, $http, NSStudent, NSStudentAttributeLookups, nsFilterOptionsService, NSAssesssmentLineGraphManager, NSStudentInterventionManager, NSStudentTMNotesManager, NSStudentNotesManager, NSStudentAssessmentFieldManager, nsPinesService, webApiBaseUrl, ckEditorSettings, $bootbox, $routeParams, authService, spinnerService, $q, $timeout, $location) {
				return {
					restrict: 'EA',
					templateUrl: 'templates/student-dashboard-print-directive.html',
					scope: {
						student: '=',
						tab: '='
					},
					link: function (scope, element, attr) {
						
						scope.settings = { studentLoaded: false, selectedStudent: null };
						scope.studentAssessmentFieldsManager = new NSStudentAssessmentFieldManager();
						scope.studentTMNotesManager = new NSStudentTMNotesManager();
						scope.studentNotesManager = new NSStudentNotesManager();
						scope.studentDataManager = new NSStudentInterventionManager();
						scope.ClassLineGraphDataManagers = [];

						scope.fullStudent = {};
						scope.allAttributes = new NSStudentAttributeLookups();

						scope.getAttributeValue = function (selectedValue, attribute) {
						    for (var i = 0; i < attribute.LookupValues.length; i++) {
						        if (selectedValue == attribute.LookupValues[i].LookupValueId) {
						            return attribute.LookupValues[i].LookupValue;
						        }
						    }
						}

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
								scope.settings.tab = scope.tab;
							    // added on 9/27/2020
								scope.fullStudent = new NSStudent(scope.student.id);
								LoadLineGraphs(scope.settings.selectedStudent);
								scope.studentTMNotesManager.LoadData(scope.settings.selectedStudent.id);
							}
						});

					
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

                                    // dont do this in printmode
									if ($location.absUrl().indexOf('printmode=') < 0) {
									    $timeout(function () {
									        $(Highcharts.charts).each(function (i, chart) {
									            if (chart) {
									                chart.reflow();
									            }
									        });
									    }, 1000);
									}
								})
							});
						}

					}
				};
			}
		])
		.controller('StudentDashboardController', [
			'$http', 'nsFilterOptionsService', '$scope', 'NSAssesssmentLineGraphManager', 'NSStudentInterventionManager', 'NSStudentTMNotesManager', 'NSStudentNotesManager', 'NSStudentAssessmentFieldManager', 'nsPinesService', 'webApiBaseUrl', 'ckEditorSettings', '$bootbox', '$routeParams', 'authService','spinnerService', '$q', function (
				$http, nsFilterOptionsService, $scope, NSAssesssmentLineGraphManager, NSStudentInterventionManager, NSStudentTMNotesManager, NSStudentNotesManager, NSStudentAssessmentFieldManager, nsPinesService, webApiBaseUrl, ckEditorSettings, $bootbox, $routeParams, authService, spinnerService, $q) {
			$scope.filterOptions = nsFilterOptionsService.options;
			$scope.settings = { studentLoaded: false, selectedStudent: null };
			$scope.settings.alreadyLetUrlStudentLoad = false;

			var origin = location.protocol + '//' + location.host + '/#/';
			$scope.targetPages = [{ url: origin + 'student-dashboard-printall', label: 'Print Full Dashboard', tab: '' },
                                   { url: origin + 'student-dashboard-printall', label: 'Student Details', tab: 'dt' },
								  { url: origin + 'student-dashboard-printall', label: 'Class Line Graphs', tab: 'lg' },
								  { url: origin + 'student-dashboard-printall', label: 'Student Cumulative Folder', tab: 'os' },
								  { url: origin + 'student-dashboard-printall', label: 'Interventions', tab: 'intv' },
								  { url: origin + 'student-dashboard-printall', label: 'Student Notes', tab: 'sn' },
								  { url: origin + 'student-dashboard-printall', label: 'Team Meeting Notes', tab: 'tmn' }];

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
					$scope.settings.selectedStudent.text = response.data.LastName + ", " + response.data.FirstName;
				})
			}

			$scope.processQuickSearchStudent = function () {
				if (angular.isDefined($scope.filterOptions.quickSearchStudent)) {
					$scope.settings.studentLoaded = true;
					$scope.settings.selectedStudent = $scope.filterOptions.quickSearchStudent;
					$scope.settings.selectedStudent.text = $scope.filterOptions.quickSearchStudent.LastName + ", " + $scope.filterOptions.quickSearchStudent.FirstName;
				} else {
					$bootbox.alert('Please select a Student first.');
				}
			}    
			}])
		.controller('StudentDashboardPrintController', [
			'$http', 'nsFilterOptionsService', '$scope', 'NSAssesssmentLineGraphManager', 'NSStudentInterventionManager', 'NSStudentTMNotesManager', 'NSStudentNotesManager', 'NSStudentAssessmentFieldManager', 'nsPinesService', 'webApiBaseUrl', 'ckEditorSettings', '$bootbox', '$routeParams', 'authService', 'spinnerService', '$q', function (
				$http, nsFilterOptionsService, $scope, NSAssesssmentLineGraphManager, NSStudentInterventionManager, NSStudentTMNotesManager, NSStudentNotesManager, NSStudentAssessmentFieldManager, nsPinesService, webApiBaseUrl, ckEditorSettings, $bootbox, $routeParams, authService, spinnerService, $q) {
				$scope.filterOptions = nsFilterOptionsService.options;
				$scope.settings = { studentLoaded: false, selectedStudent: null, tab: null };

				$scope.settings.tab = $routeParams.tab;

				if (angular.isDefined($routeParams.id)) {
					var paramObj = { id: $routeParams.id };
					$http.post(webApiBaseUrl + '/api/student/getstudentbyid', paramObj).then(function (response) {
						$scope.settings.studentLoaded = true;
						$scope.settings.selectedStudent = response.data;
						$scope.settings.selectedStudent.text = response.data.LastName + ", " + response.data.FirstName;
					})
				}
			}])
		.factory('NSStudentDashboardManager', ['$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {

			var NSStudentDashboardManager = function () {
				this.LoadData = function (studentId) {
					var url = webApiBaseUrl + '/api/studentdashboard/GetStudentObservationSummary/';
					var paramObj = { StudentId: studentId }
					var promise = $http.post(url, paramObj);
					var self = this;

					self.LookupLists = [];
					self.Scores = [];
					self.BenchmarksByGrade = [];
					

					return promise.then(function (response) {
						angular.extend(self, response.data);
						if (self.LookupLists === null) self.LookupLists = [];
						if (self.Scores === null) self.Scores = [];
						if (self.BenchmarksByGrade === null) self.BenchmarksByGrade = [];

					});
				}
			}
			return NSStudentDashboardManager;
		}])
		.factory('NSStudentNotesManager', ['$http', 'webApiBaseUrl', '$filter', function ($http, webApiBaseUrl, $filter) {

			var NSStudentNotesManager = function () {

				this.saveNote = function (noteId, noteHtml, studentId) {
					var url = webApiBaseUrl + '/api/student/savenote';
					var paramObj = { NoteId: noteId, NoteHtml: noteHtml, StudentId: studentId };
					var promise = $http.post(url, paramObj);

					return promise;
				}

				this.deleteNote = function (noteId) {
					var url = webApiBaseUrl + '/api/student/deletenote';
					var paramObj = { Id: noteId };
					var promise = $http.post(url, paramObj);

					return promise;
				}

				this.LoadData = function (studentId) {
					var url = webApiBaseUrl + '/api/student/getnotesforstudent/';
					var paramObj = { id: studentId }
					var promise = $http.post(url, paramObj);
					var self = this;
					self.Notes = [];

					return promise.then(function (response) {
						self.Notes = response.data.Notes;
					});
				}
			}
			return NSStudentNotesManager;
		}])
		.factory('NSStudentTMNotesManager', ['$http', 'webApiBaseUrl','$filter', function ($http, webApiBaseUrl, $filter) {

			var NSStudentTMNotesManager = function () {

				this.saveNote = function (noteId, noteHtml, teamMeetingId, studentId) {
					var url = webApiBaseUrl + '/api/teammeeting/savenote';
					var paramObj = { TeamMeetingId: teamMeetingId, NoteId: noteId, NoteHtml: noteHtml, StudentId: studentId };
					var promise = $http.post(url, paramObj);

					return promise;
				}

				this.deleteNote = function (noteId) {
					var url = webApiBaseUrl + '/api/teammeeting/deletenote';
					var paramObj = { Id: noteId };
					var promise = $http.post(url, paramObj);

					return promise;
				}

				this.LoadData = function (studentId) {
					var url = webApiBaseUrl + '/api/teammeeting/getnotesforstudentteammeetings/';
					var paramObj = { id: studentId }
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
								var newGroupedNote = { SchoolYear: meeting.SchoolYear, Meetings: [] };
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
				}
			}
			return NSStudentTMNotesManager;
		}])
		.directive('nsObservationSummaryStudent', [
			'$routeParams', '$compile', '$templateCache', '$http', 'nsFilterOptionsService', '$filter', 'NSStudentDashboardManager', 'NSSortManager', 'nsLookupFieldService', 'observationSummaryAssessmentFieldChooserSvc', '$location',
			function ($routeParams, $compile, $templateCache, $http, nsFilterOptionsService, $filter, NSStudentDashboardManager, NSSortManager, nsLookupFieldService, observationSummaryAssessmentFieldChooserSvc, $location) {

				return {
					restrict: 'E',
					template: '<div ng-include="templateUrl"></div>',
					//templateUrl: 'templates/observation-summary-student.html',
					scope: {
						selectedStudentId: '=',
						selectedAssessmentIds: '='
					},
					link: function (scope, element, attr) {
						scope.observationSummaryManager = new NSStudentDashboardManager();
						scope.filterOptions = nsFilterOptionsService.options;
						scope.manualSortHeaders = {};
						scope.manualSortHeaders.firstNameHeaderClass = "fa";
						scope.manualSortHeaders.gradeNameHeaderClass = "fa";
						scope.manualSortHeaders.teachersHeaderClass = "fa";
						scope.manualSortHeaders.tddHeaderClass = "fa";
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
						}

						scope.hideAssessment = function (assessment) {
							observationSummaryAssessmentFieldChooserSvc.hideAssessment(assessment).then(function (response) {
								scope.observationSummaryManager.LoadData(scope.selectedStudentId).then(function (response) {
									attachFieldsCallback();
								});
							});
						}

						scope.$on('NSFieldsUpdated', function (event, data) {
							scope.observationSummaryManager.LoadData(scope.selectedStudentId).then(function (response) {
								attachFieldsCallback();
							});
						});

						scope.$watch('selectedStudentId', function (newVal, oldVal) {
							if (newVal !== oldVal) {
								scope.observationSummaryManager.LoadData(scope.selectedStudentId).then(function (response) { attachFieldsCallback(); });
							}
						});
														
						scope.printPages = [];
						// printing setup
						if ($location.absUrl().indexOf('printmode=') >= 0) {
							scope.templateUrl = 'templates/observation-summary-student-print.html';
						} else {
							scope.templateUrl = 'templates/observation-summary-student.html';
						}

						var attachFieldsCallback = function () {
							// initialize the sort manager now that the data has been loaded
							scope.sortMgr.initialize(scope.manualSortHeaders, scope.sortArray, scope.headerClassArray, 'OSFieldResults', scope.observationSummaryManager.Scores);

							if ($location.absUrl().indexOf('printmode=') >= 0) {

								// add curent page to array
								var currentPageNo = 1;
								var currentHeaderCount = 0;
								//scope.printPages.push(currentPage);

								// set page for headers
								var currentPage = { page: currentPageNo, assessments: [], currentHeaderCount: 0 };
								scope.printPages.push(currentPage);
								for (var p = 0; p < scope.observationSummaryManager.Scores.HeaderGroups.length; p++) {
									currentHeaderCount += scope.observationSummaryManager.Scores.HeaderGroups[p].FieldCount;


									// if less than 15, add to current page
									if (currentHeaderCount <= 15) {
										scope.observationSummaryManager.Scores.HeaderGroups[p].page = currentPageNo;
										currentPage.assessments.push(scope.observationSummaryManager.Scores.HeaderGroups[p].AssessmentId);
										currentPage.currentHeaderCount = currentHeaderCount;
									} else {
										// the header groups create the pages in the page array
										currentPageNo++;
										currentPage = { page: currentPageNo, assessments: [], currentHeaderCount: scope.observationSummaryManager.Scores.HeaderGroups[p].FieldCount };
										currentPage.assessments.push(scope.observationSummaryManager.Scores.HeaderGroups[p].AssessmentId);
										scope.printPages.push(currentPage);
										scope.observationSummaryManager.Scores.HeaderGroups[p].page = currentPageNo;
										currentHeaderCount = scope.observationSummaryManager.Scores.HeaderGroups[p].FieldCount;
									}
								}

								// set page for fields
								for (var p = 0; p < scope.observationSummaryManager.Scores.Fields.length; p++) {
									for (var x = 0; x < scope.printPages.length; x++) {
										for (var y = 0; y < scope.printPages[x].assessments.length; y++) {
											if (scope.printPages[x].assessments[y] == scope.observationSummaryManager.Scores.Fields[p].AssessmentId) {
												scope.observationSummaryManager.Scores.Fields[p].page = scope.printPages[x].page;
											}
										}
									}
								}

								// set page for OSFieldREsults
								for (var j = 0; j < scope.observationSummaryManager.Scores.StudentResults.length; j++) {
									for (var k = 0; k < scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults.length; k++) {
										for (var x = 0; x < scope.printPages.length; x++) {
											for (var y = 0; y < scope.printPages[x].assessments.length; y++) {
												if (scope.printPages[x].assessments[y] == scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].AssessmentId) {
													scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].page = scope.printPages[x].page;
												}
											}
										}
									}
								}
							}

							for (var j = 0; j < scope.observationSummaryManager.Scores.StudentResults.length; j++) {
								for (var k = 0; k < scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults.length; k++) {
									for (var i = 0; i < scope.observationSummaryManager.Scores.Fields.length; i++) {
										if (scope.observationSummaryManager.Scores.Fields[i].DatabaseColumn == scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].DbColumn) {
											scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].Field = angular.copy(scope.observationSummaryManager.Scores.Fields[i]);

											// set display value
											//if (scope.observationSummaryManager.Scores.Fields[i].FieldType === "DropdownFromDB") {
											//    for (var p = 0; p < scope.lookupFieldsArray.length; p++) {
											//        if (scope.lookupFieldsArray[p].LookupColumnName === scope.observationSummaryManager.Scores.Fields[i].LookupFieldName) {
											//            // now find the specifc value that matches
											//            for (var y = 0; y < scope.lookupFieldsArray[p].LookupFields.length; y++) {
											//                if (scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].IntValue === scope.lookupFieldsArray[p].LookupFields[y].FieldSpecificId) {
											//                    scope.observationSummaryManager.Scores.StudentResults[j].OSFieldResults[k].DisplayValue = scope.lookupFieldsArray[p].LookupFields[y].FieldValue;
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


						}
						scope.observationSummaryManager.LoadData(scope.selectedStudentId).then(function (response) { attachFieldsCallback(); });

						// delegate sorting to the sort manager
						scope.sort = function (column) {
							scope.sortMgr.sort(column);
						};

						function getIntColor(gradeId, testLevelPeriodId, studentFieldScore, fieldValue) {
							var benchmarkArray = null;
							for (var i = 0; i < scope.observationSummaryManager.BenchmarksByGrade.length; i++) {

								if (scope.observationSummaryManager.BenchmarksByGrade[i].GradeId == gradeId) {
									benchmarkArray = scope.observationSummaryManager.BenchmarksByGrade[i];
								}

								if (benchmarkArray != null) {
									for (var j = 0; j < benchmarkArray.Benchmarks.length; j++) {
										if (benchmarkArray.Benchmarks[j].DbColumn === studentFieldScore.DbColumn
											&& benchmarkArray.Benchmarks[j].AssessmentId === studentFieldScore.AssessmentId
											&& benchmarkArray.Benchmarks[j].TestLevelPeriodId == testLevelPeriodId) { 
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

						scope.getBackgroundClass = function (gradeId, testLevelPeriodId, studentFieldScore) {
							switch (studentFieldScore.ColumnType) {
								case 'Textfield':
									return '';
									break;
								case 'DecimalRange':
									return getIntColor(gradeId, testLevelPeriodId, studentFieldScore, studentFieldScore.DecimalValue);
									break;
								case 'DropdownRange':
									return getIntColor(gradeId, testLevelPeriodId, studentFieldScore, studentFieldScore.IntValue);
									break;
								case 'DropdownFromDB':
									return getIntColor(gradeId, testLevelPeriodId, studentFieldScore, studentFieldScore.IntValue);
									break;
								case 'CalculatedFieldClientOnly':
									return '';
									break;
								case 'CalculatedFieldDbBacked':
									return getIntColor(gradeId, testLevelPeriodId, studentFieldScore, studentFieldScore.IntValue);
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
})();