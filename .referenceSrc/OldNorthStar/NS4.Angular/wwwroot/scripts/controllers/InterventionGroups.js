(function () {
	'use strict';

	angular
		.module('interventionGroupModule', [])
		.controller('InterventionGroupListController', InterventionGroupListController)
		.controller('InterventionGroupEditController', InterventionGroupEditController)
		.controller('InterventionGroupAttendanceController', InterventionGroupAttendanceController)
		 .service('InterventionGroupService', [
			'$http', 'pinesNotifications', 'webApiBaseUrl', function ($http, pinesNotifications, webApiBaseUrl) {
				//this.options = {};
				var self = this;

				self.deleteGroup = function (groupId) {
					var paramObj = { Id: groupId };
					var url = webApiBaseUrl + '/api/interventiongroup/DeleteIntervention/';
					var promise = $http.post(url, paramObj);

					return promise;
				}

				self.getbyyearschoolstaff = function (year, schoolid, staffid) {
					var paramObj = { SchoolYear: year, SchoolId: schoolid, StaffId: staffid };
					return $http.post(webApiBaseUrl + '/api/interventiongroup/getbyyearschoolstaff/', paramObj);
				};
			}]
	)
		.factory('InterventionGroup', [
	'$http', 'webApiBaseUrl', '$filter', 'nsFilterOptionsService', function InterventionGroup($http, webApiBaseUrl, $filter, nsFilterOptionsService) {
		var InterventionGroup = function(groupId) {

			var self = this;
			self.StintsByStudent = [];

			self.initialize = function () {
				var paramObj = { Id: groupId };
				var url = webApiBaseUrl + '/api/interventiongroup/GetGroup/';
				var promise = $http.post(url, paramObj);

				function getSelectedItemFromCollection(id, collection) {
					for (var i = 0; i < collection.length; i++) {
						if (collection[i].id == id) {
							return collection[i];
						}
					}
					return null;
				}

				return promise.then(function (response) {
					angular.extend(self, response.data);

					// don't allow school or year to be changed for existing groups
					if (self.Id == 0 || self.Id == -1) {
						self.SchoolID = nsFilterOptionsService.normalizeParameter(nsFilterOptionsService.options.selectedSchool);
						self.SchoolStartYear = nsFilterOptionsService.normalizeParameter(nsFilterOptionsService.options.selectedSchoolYear);
						
					} else {
						self.selectedTier = getSelectedItemFromCollection(self.Tier, self.tiers);
					}

					// fix all start and end dates to only use date

					self.postLoadProcessing();
				});
			}

			self.tiers = [{ id: 1, text: 'Tier 1' }, {id: 2, text: 'Tier 2' }, {id:3, text :'Tier 3'}];

			self.validate = function () {
				var msg = null;

				if (moment(self.StartTime).toDate() >= moment(self.EndTime).toDate()) {
					msg = 'Intervention Group start time must be BEFORE the end time.';
					return msg;
				}

				return msg;
			}

			self.canDeleteStint = function (stintId) {
				var paramObj = { Id: stintId };
				var url = webApiBaseUrl + '/api/interventiongroup/CanStintBeDeleted/';
				var promise = $http.post(url, paramObj);

				return promise;
			}

			self.postLoadProcessing = function () {
				for (var i = 0; i < self.StudentInterventionGroups.length; i++) {
					var sg = self.StudentInterventionGroups[i];
					// see if we've added this student already or not
					var recordsForStudentId = $filter('filter')(self.StintsByStudent, { StudentId: sg.StudentId });

					// if we've already created a group, just add it
					if (recordsForStudentId.length > 0) {
						var currentStintGroup = recordsForStudentId[0];
						currentStintGroup.Stints.push(sg);
					} else { // create a new stint group
						var newStintGroup = { StudentId: sg.StudentId, StudentName: sg.StudentName, Stints: [] };
						newStintGroup.Stints.push(sg);
						self.StintsByStudent.push(newStintGroup);
					}

				}
			}

			self.save = function () {
				var url = webApiBaseUrl + '/api/interventiongroup/SaveInterventionGroup';

				self.Tier = self.selectedTier.id;
				var promise = $http.post(url, self);

				return promise;
			}

			self.initialize();
		}

		return InterventionGroup;
	}
		])
	.service('nsInterventionGroupService', [
			'$http', 'pinesNotifications', 'webApiBaseUrl', 'nsFilterOptionsService', function ($http, pinesNotifications, webApiBaseUrl, nsFilterOptionsService) {
				//this.options = {};

				this.getAttendanceDataForWeek = function (sectionId, staffId, schoolYear, mondayDate, schoolId) {
					var paramObj = { InterventionGroupId: nsFilterOptionsService.normalizeParameter(sectionId), StaffId: nsFilterOptionsService.normalizeParameter(staffId), SchoolStartYear: nsFilterOptionsService.normalizeParameter(schoolYear), MondayDate: mondayDate, SchoolId: nsFilterOptionsService.normalizeParameter(schoolId) }
					return $http.post(webApiBaseUrl + '/api/interventiongroup/getweeklyattendance/', paramObj);
				};

				this.applyStatusNotes = function (attendanceDate, status, notes, staffId, sectionId, schoolStartYear) {
					return $http.post(webApiBaseUrl + '/api/interventiongroup/applyStatusNotes', { Notes: notes, 'Date': attendanceDate, Status: status, StaffId: nsFilterOptionsService.normalizeParameter(staffId), SectionId: nsFilterOptionsService.normalizeParameter(sectionId), SchoolStartYear: nsFilterOptionsService.normalizeParameter(schoolStartYear) });
				};

				this.saveSingleAttendance = function (startEndDateId, status, notes, date, studentId, sectionId) {
					return $http.post(webApiBaseUrl + '/api/interventiongroup/saveSingleAttendance', { Notes: notes, StartEndDateId: startEndDateId, 'Date': date, Status: status, StudentId: studentId, SectionId: sectionId });
				};
			}]
	);

	/* Movies List Controller  */
	InterventionGroupListController.$inject = ['$scope', 'InterventionGroup', '$q', '$http', 'nsPinesService', '$location', 'InterventionGroupService', '$bootbox', 'nsFilterOptionsService'];


	function InterventionGroupListController($scope, InterventionGroup, $q, $http, nsPinesService, $location, InterventionGroupService, $bootbox, nsFilterOptionsService) {
		$scope.groups = [];
		$scope.dropdownGroups = [];
		$scope.filterOptions = nsFilterOptionsService.options;

		$scope.errors = [];
		$scope.$on('NSHTTPError', function (event, data) {
			$scope.errors.push({ type: "danger", msg: data });
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
			$bootbox.confirm("Are you sure you want to delete this Intervention Group?<br><br><b>Note:</b> Any associated test or attendance data for this intervention group will be lost.", function (response) {
				if (response) {
					InterventionGroupService.deleteGroup(id).then(function (response) {
						nsPinesService.dataDeletedSuccessfully();
						LoadData();		                
					});
				}
			});

		}

		var LoadData = function () {
			InterventionGroupService.getbyyearschoolstaff(nsFilterOptionsService.normalizeParameter($scope.filterOptions.selectedSchoolYear),
				nsFilterOptionsService.normalizeParameter($scope.filterOptions.selectedSchool),
				nsFilterOptionsService.normalizeParameter($scope.filterOptions.selectedInterventionist)).then(function (response) {
				$scope.groups = response.data.Groups;
			});
		}

		LoadData();
	}


	InterventionGroupEditController.$inject = ['$scope', 'InterventionGroup', '$q', '$http', 'nsPinesService', '$location', '$routeParams', 'nsSelect2RemoteOptions', '$bootbox', '$filter', 'nsFilterOptionsService'];

	function InterventionGroupEditController($scope, InterventionGroup, $q, $http, nsPinesService, $location, $routeParams, nsSelect2RemoteOptions, $bootbox, $filter, nsFilterOptionsService) {
		$scope.group = new InterventionGroup($routeParams.id);
		$scope.StudentQuickSearchRemoteOptions = nsSelect2RemoteOptions.StudentQuickSearchRemoteOptions;
		$scope.PrimaryInterventionistRemoteOptions = nsSelect2RemoteOptions.PrimaryInterventionistRemoteOptions;
		$scope.InterventionTypeRemoteOptions = nsSelect2RemoteOptions.InterventionTypeRemoteOptions;
		$scope.CoInterventionistsRemoteOptions = nsSelect2RemoteOptions.CoInterventionistsRemoteOptions;
		$scope.filterOptions = nsFilterOptionsService.options;

		$scope.errors = [];
		$scope.$on('NSHTTPError', function (event, data) {
			$scope.errors.push({ type: "danger", msg: data });
			$('html, body').animate({ scrollTop: 0 }, 'fast');
		});
		$scope.newStint = {};
		
		$scope.datePopupStatus = {
			opened: false
		};


		// we don't care about these changes unless this is a new intervention group
		//$scope.$watch('filterOptions', function () {
		//    if ($routeParams.id === "-1") {
		//        $scope.group.SchoolID = nsFilterOptionsService.normalizeParameter($scope.filterOptions.selectedSchool);
		//        $scope.group.SchoolStartYear = nsFilterOptionsService.normalizeParameter($scope.filterOptions.selectedSchoolYear);
		//    }
		//}, true);

		$scope.saveGroup = function () {
			var validationMessage = $scope.group.validate();
			if (validationMessage != null) {
				$scope.errors.push({ type: "danger", msg: validationMessage });
				$('html, body').animate({ scrollTop: 0 }, 'fast');
				return;
			} else {
				$scope.errors = [];
			}

			if ($scope.group.Id <= 0) {
				$scope.group.SchoolID = $scope.filterOptions.selectedSchool.id;
				$scope.group.SchoolStartYear = $scope.filterOptions.selectedSchoolYear.id;
			}

			$scope.group.save().then(function (response) {
				nsPinesService.dataSavedSuccessfully();
				$location.path('ig-manage');
			});
		}

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
			if(!angular.isDefined(student)) {
				$bootbox.alert('Please select a student');
				return;
			}

			// make sure start date is before end date
			if ($scope.newStint.endDate != null) {
				if (moment($scope.newStint.endDate) <= moment($scope.newStint.startDate)) {
					$bootbox.alert('Start date must be before End date');
					return;
				}
			}

			var recordsForStudentId = $filter('filter')($scope.group.StintsByStudent, { StudentId: student.id });
			var newStint = {
				StudentId: student.id,
				StartDate: $scope.newStint.startDate,
				StartDateText: $scope.newStint.startDate,
				EndDate: $scope.newStint.endDate,
				EndDateText: ($scope.newStint.endDate == null || $scope.newStint.endDate == '') ? 'no end date' : $scope.newStint.endDate,
				Id: -1,
				InterventionGroupId: $scope.group.Id
			};



			// add new record as long as start
			if (recordsForStudentId.length == 0) {
				// create new stintholder
				var newStintGroup = { StudentId: student.id, StudentName: student.LastName + ', ' + student.FirstName + ' ' + student.MiddleName, Stints: [] };
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
		}

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
					if (moment(newStint.StartDate) < moment(existingStudentStints[i].EndDate)) {
						validationMessage = 'Start date of an open ended stint must be after the end date of all other stints.';
					}
				}
			} else {
				// both start and end are filled in, make sure start is either before any ohter start date and ends before or is after any other end date and ends after
				for (var i = 0; i < existingStudentStints.length; i++) {
					if (moment(newStint.StartDate) <= moment(existingStudentStints[i].StartDate) && moment(newStint.EndDate) >= moment(existingStudentStints[i].StartDate)) {
						validationMessage = 'New stint must end before the start of any other stints.';
						break;
					} else if (moment(newStint.StartDate) <= moment(existingStudentStints[i].EndDate) && moment(newStint.EndDate) >= moment(existingStudentStints[i].EndDate)) {
						validationMessage = 'Cannot start a new stint during another stint.';
						break;
					} else if (moment(newStint.StartDate) >= moment(existingStudentStints[i].StartDate) && moment(newStint.EndDate) <= moment(existingStudentStints[i].EndDate)) {
						validationMessage = 'A new stint cannot take place during an existing stint.';
						break;
					} else if (moment(newStint.StartDate) <= moment(existingStudentStints[i].StartDate) && moment(newStint.EndDate) >= moment(existingStudentStints[i].EndDate)) {
						validationMessage = 'A new sting cannot overlap an existing stint.';
						break;
					}
				}
			}

			return validationMessage;
		}

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
					if (moment(newStint.newStartDate) < moment(existingStudentStints[i].EndDate) && existingStudentStints[i] != newStint) {
						validationMessage = 'Start date of an open ended stint must be after the end date of all other stints.'
					}
				}
			} else {
				// both start and end are filled in, make sure start is either before any ohter start date and ends before or is after any other end date and ends after
				for (var i = 0; i < existingStudentStints.length; i++) {
					if (moment(newStint.newStartDate) <= moment(existingStudentStints[i].StartDate) && moment(newStint.newEndDate) >= moment(existingStudentStints[i].StartDate) && existingStudentStints[i] != newStint) {
						validationMessage = 'New stint must end before the start of any other stints.';
						break;
				} else if (moment(newStint.newStartDate) <= moment(existingStudentStints[i].EndDate) && moment(newStint.newEndDate) >= moment(existingStudentStints[i].EndDate) && existingStudentStints[i] != newStint) {
						validationMessage = 'Cannot start a new stint during another stint.';
						break;
				} else if (moment(newStint.newStartDate) >= moment(existingStudentStints[i].StartDate) && moment(newStint.newEndDate) <= moment(existingStudentStints[i].EndDate) && existingStudentStints[i] != newStint) {
						validationMessage = 'A new stint cannot take place during an existing stint.';
						break;
				} else if (moment(newStint.newStartDate) <= moment(existingStudentStints[i].StartDate) && moment(newStint.newEndDate) >= moment(existingStudentStints[i].EndDate) && existingStudentStints[i] != newStint) {
						validationMessage = 'A new stint cannot overlap an existing stint.';
						break;
					}
				}
			}

			return validationMessage;
		}


		$scope.editStint = function (stint) {
			stint.editMode = true;
			stint.newStartDate = moment(stint.StartDateText, 'DD-MMM-YYYY').format('DD-MMM-YYYY');
			stint.newEndDate = stint.EndDate == null ? stint.EndDate :  moment(stint.EndDateText, 'DD-MMM-YYYY').format('DD-MMM-YYYY');
		}

		$scope.saveStint = function (stint, newStart, newEnd, stints, studentId) {
			// validation checks
			if (angular.isUndefined(newStart) || newStart == null || newStart == '') {
				$bootbox.alert('Please enter a valid start date');
				return;
			}
			if (angular.isUndefined(newEnd)) {
			    newEnd = null;
			}

			// make sure start date is before end date
			if (newEnd != null && newEnd != '') {
				if (moment(newEnd) <= moment(newStart)) {
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
			stint.StartDateText = newStart;
			stint.EndDate = (newEnd == '' || newEnd == null) ? null : newEnd;
			stint.EndDateText = (newEnd == '' || newEnd == null) ? 'no end date' : newEnd;
			stint.StudentId = studentId;
			stint.editMode = false;
		}

		$scope.removeStint = function (stint) {
			// call webservice to see if we can remove this stint (is there any attendance or data tied to it)
			$scope.group.canDeleteStint(stint.Id).then(function (response) {
				if (!response.data.isValid) {
				    $bootbox.confirm('If you remove this stint, any attendance recorded for this student during this stint will be lost.  Are you sure you want to remove this stint?', function (confirmResponse) {
				        if (confirmResponse) {
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
				} else {
				    // if no problem deleting stint
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

		}

		$scope.endDate = function (date) {
			if (date == null) {
				return 'no end date';
			} else {
				return $filter('nsDateFormat')(date);
			}
		}

		var resetAddStint = function () {
			$scope.newStint = {};
			$scope.igform.newStintForm.$setPristine();
			$scope.igform.newStintForm.$setUntouched();
		}

	}

	InterventionGroupAttendanceController.$inject = ['$scope', 'InterventionGroup', '$q', '$http', 'nsFilterOptionsService', '$location', '$routeParams', 'nsInterventionGroupService', 'nsPinesService', 'progressLoader', '$bootbox', 'spinnerService', '$timeout'];

	function InterventionGroupAttendanceController($scope, InterventionGroup, $q, $http, nsFilterOptionsService, $location, $routeParams, nsInterventionGroupService, nsPinesService, progressLoader, $bootbox, spinnerService, $timeout) {
		
		var makeMonday = function (theDate) {
			return moment(theDate).startOf('isoweek');
		};

		$scope.filterOptions = nsFilterOptionsService.options;
		$scope.computedMonday = makeMonday(moment()).format('MMMM D, YYYY');
		$scope.computedFriday = moment($scope.computedMonday, 'MMMM D, YYYY').add(4, 'days').format('MMMM D, YYYY');

		$scope.showResults = function () {
			if ($scope.filterOptions.selectedInterventionist != null) {
				return true;
			} else return false;
		}

		 $scope.drp_start = $scope.computedMonday;
			//$scope.drp_end = moment().add('days', 31).format('MMMM D, YYYY');
			$scope.drp_options = {
			  singleDatePicker: true,
			  opens: 'left'
		};

		$scope.changeWeek = function(count)
		{
			$scope.drp_start = moment($scope.computedMonday, 'MMMM D, YYYY').add(count, 'weeks').format('MMMM D, YYYY');
		}

		$scope.$watch('drp_start', function(newVal, oldVal) {
							if (newVal !== oldVal) {
							// compute new monday and friday
							$scope.computedMonday = makeMonday($scope.drp_start).format('MMMM D, YYYY');
							$scope.computedFriday = moment($scope.computedMonday, 'MMMM D, YYYY').add(4, 'days').format('MMMM D, YYYY');
							LoadData()
							}
						});

		$scope.$on('NSInterventionistOptionsUpdated', function (event, data) {
			LoadData();
		});
		
		$scope.$on('NSInterventionGroupOptionsUpdated', function (event, data) {
			LoadData();
		});

		$scope.$on('NSInitialLoadComplete', function (event, data) {
			LoadData();
		});

		var loaderStart = function () {
			  progressLoader.start();
			  setTimeout(function(){
				  progressLoader.set(50);
			  }, 1000);

			};

		var LoadData = function (sectionId, staffId, schoolYear, mondayDate) {
			if ($scope.showResults()) {
				// TODO: Make this more robust
				progressLoader.start();
				progressLoader.set(50);
				$timeout(function () {
					spinnerService.show('tableSpinner');
				});
				nsInterventionGroupService.getAttendanceDataForWeek($scope.filterOptions.selectedInterventionGroup, $scope.filterOptions.selectedInterventionist, $scope.filterOptions.selectedSchoolYear, $scope.computedMonday, $scope.filterOptions.selectedSchool).then(function (response) {
					progressLoader.end();
					spinnerService.hide('tableSpinner');
					$scope.weekdays = response.data.WeekDays;
					$scope.attendanceData = response.data.AttendanceData;
				});
			}
		};

		//LoadData();

		$scope.applyStatusNotes = function (date) {
			if (date.Status == null) {
				$bootbox.alert('Please select a status');
				return;
			}

			$bootbox.confirm("Are you sure you want to apply this status to all listed students?", function (response) {
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
			})
		};

		// Disable weekend selection
		$scope.disabled = function (date, mode) {
			return (mode === 'day' && (date.getDay() === 0 || date.getDay() === 6));
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
		}

		$scope.attendanceReasons = ["Teacher Absent", "Teacher Unavailable", "Child Absent", "Child Unavailable", "No School", "Intervention Delivered", "Make-Up Lesson", "Non-Cycle Day","None"]

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
		$scope.saveSingleAttendanceDelivered = function (studentResult, realStatusField, startEndDateId, status, notes, date, studentId, sectionId) {


			// update real value with temp
			studentResult[realStatusField] = "Intervention Delivered";

			nsInterventionGroupService.saveSingleAttendance(startEndDateId, "Intervention Delivered", '', date, studentId, sectionId).then(function (data) {
				nsPinesService.dataSavedSuccessfully();
				return true;
			});
		};
	}

})();