(function () {
	'use strict'

	angular
	.module('studentModule', [])
	.factory('NSStudentManager', [
		'$http', '$bootbox', 'nsFilterOptionsService', 'webApiBaseUrl', function ($http, $bootbox, nsFilterOptionsService, webApiBaseUrl) {
			var NSStudentManager = function () {
				this.initialize = function () {
				}

				this.remoteStudentIdValidationPath = webApiBaseUrl + '/api/student/IsStudentIDUnique';

				this.getStudentList = function (schoolYear, schoolId, gradeId, teacherId, sectionId, studentId) {
					var returnObject = {
						SchoolYear: nsFilterOptionsService.normalizeParameter(schoolYear),
						SchoolId: nsFilterOptionsService.normalizeParameter(schoolId),
						GradeId: nsFilterOptionsService.normalizeParameter(gradeId),
						TeacherId: nsFilterOptionsService.normalizeParameter(teacherId),
						SectionId: nsFilterOptionsService.normalizeParameter(sectionId),
						StudentId: nsFilterOptionsService.normalizeParameter(studentId),
					};

					return $http.post(webApiBaseUrl + "/api/student/GetStudentList", returnObject);
				};

				this.getSectionsForYear = function (studentSchool) {
				    return $http.post(webApiBaseUrl + '/api/student/GetSectionsForYear', { StudentSchool: studentSchool });
				}

				this.consolidateStudent = function (primaryStudent, secondaryStudent) {
				    return $http.post(webApiBaseUrl + '/api/student/ConsolidateStudent', { PrimaryStudent: primaryStudent, SecondaryStudent: secondaryStudent });
				}

				this.consolidateStudentServices = function (primaryServices, secondaryServices) {
				    return $http.post(webApiBaseUrl + '/api/student/ConsolidateStudentServices', { PrimaryService: primaryServices, SecondaryServices: secondaryServices });
				}

				this.save = function (student, successCallback, failureCallback) {
					var saveResponse = $http.post(webApiBaseUrl + "/api/student/savestudent", student)

					return saveResponse;
				}

				this.moveStudent = function (student, targetSection) {
					var paramObj = {Student: student, Section: targetSection};
					return $http.post(webApiBaseUrl + "/api/student/movestudent", paramObj);
				}

				this.delete = function (id, successCallback, failureCallback) {
					$bootbox.confirm("Are you sure you want to delete this student?", function (result) {
						if (result === true) {
							var returnObject = { Id: id };
							var deleteResponse = $http.post(webApiBaseUrl + "/api/student/deletestudent", returnObject);

							deleteResponse.then(successCallback, failureCallback);
						}
					});
				};

				this.initialize();
			};

			return (NSStudentManager);
		}
		])
	.factory('NSStudent', [
		'$http', 'webApiBaseUrl','$bootbox', function ($http, webApiBaseUrl, $bootbox) {
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
						if (self.SpecialEdLabels === null) self.SpecialEdLabels = [];
						if (self.StudentSchools === null) self.StudentSchools = [];
						if (self.StudentAttributes === null) self.StudentAttributes = {};

						// hack DOB into format for stupid UIB-datepopup control
						if (self.DOB == null || self.Id <= 0) {
							self.DOB = moment().format('DD-MMM-YYYY');
						} else {
							var momentizedDate = moment(self.DOBText, 'DD-MMM-YYYY');
							self.DOB = momentizedDate.format('DD-MMM-YYYY');
						}
					});
				}
				
				this.initialize();

				this.validateStudentIdentifer = function (studentIdentifier) {
					var self = this;
					var postObject = { StudentId: self.Id, StudentIdentifier: studentIdentifier };
					var url = webApiBaseUrl + '/api/student/IsStudentIDUnique';
					return $http.post(url, postObject);
				}
				// don't let them add the same registration twice
				this.addNewRegistration = function (school, schoolYear, grade, studentId) {
					var self = this;
					if (schoolYear === null || school === null || grade == null) {
						$bootbox.alert('Please select a school year, school and grade first.');
						return false;
					}
					else {
						for (var i = 0; i < self.StudentSchools.length; i++) {
							// can't be at same school twice in the same year
							if (self.StudentSchools[i].SchoolId == school.id && self.StudentSchools[i].SchoolStartYear == schoolYear.id) {
								$bootbox.alert('Student is already enrolled at this school/school year combination.');
								return false;
							}
							// if at another school, must be same grade
							if (self.StudentSchools[i].SchoolStartYear == schoolYear.id && self.StudentSchools[i].GradeId !=  grade.id) {
								$bootbox.alert('If student is at more than one school, he or she must be in the same grade at both schools.');
								return false;
							}
						}

						self.StudentSchools.unshift({ id: -1, SchoolId: school.id, SchoolName: school.text, SchoolStartYear: schoolYear.id, SchoolYearLabel: schoolYear.text, GradeId: grade.id, GradeName: grade.text, StudentId: studentId, isNew: true });
						return true;
					}
				}

				this.validateUpdatedRegistration = function (school, schoolYear, grade, registrationId) {
					var self = this;
					if (schoolYear === null || school === null || grade == null) {
						$bootbox.alert('Please select a school year, school and grade first.');
						return false;
					}
					else {
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
				}

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
						}
						else {
							$bootbox.alert('You either do not have access to this school, or the student is already attending a section at this school.')
						}
					}, function (response) {
						// error callback function

					});
				}

			};

			return (NSStudent);
		}
	])
	.controller('StudentMoveController', ['$scope', 'nsPinesService', 'nsSelect2RemoteOptions', '$bootbox', 'progressLoader', 'NSStudentManager',
		function ($scope, nsPinesService, nsSelect2RemoteOptions, $bootbox, progressLoader, NSStudentManager) {

		var mgr = new NSStudentManager();

		$scope.resetForm = function () {
			$scope.moveSettings = { studentToMove: null, targetSection: null, moveComplete: false };
			$scope.warnings = [];
			$scope.errors = [];
		}

		$scope.resetForm();

		$scope.checkDifferentYears = function () {
			if ($scope.moveSettings.studentToMove === null || $scope.moveSettings.targetSection === null) {
				return false;
			}

			return ($scope.moveSettings.studentToMove.SchoolStartYear != $scope.moveSettings.targetSection.SchoolStartYear);
		}

		$scope.$on('NSHTTPError', function (event, data) {
			$scope.errors.push({ type: "danger", msg: data });
			$('html, body').animate({ scrollTop: 0 }, 'fast');
		});

		$scope.quickSearchStudent = nsSelect2RemoteOptions.StudentDetailedQuickSearchCurrentYearRemoteOptions;
		$scope.quickSearchSection = nsSelect2RemoteOptions.quickSearchSectionsCurrentYearRemoteOptions;

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
		}


		}])
	.controller('StudentConsolidationController', ['$scope', 'nsPinesService', 'nsSelect2RemoteOptions', '$bootbox', 'progressLoader', 'NSStudentManager',
		function ($scope, nsPinesService, nsSelect2RemoteOptions, $bootbox, progressLoader, NSStudentManager) {
			var mgr = new NSStudentManager();

			$scope.resetForm = function () {
				$scope.moveSettings = { studentPrimary: null, studentSecondary: null, moveComplete: false };
				$scope.warnings = [];
				$scope.errors = [];
			}

			$scope.resetForm();

			$scope.checkSameUser = function () {
				if ($scope.moveSettings.studentPrimary === null || $scope.moveSettings.studentSecondary === null) {
					return false;
				}

				return ($scope.moveSettings.studentPrimary.LastName != $scope.moveSettings.studentSecondary.LastName || $scope.moveSettings.studentPrimary.FirstName != $scope.moveSettings.studentSecondary.FirstName);
			}

			$scope.$on('NSHTTPError', function (event, data) {
				$scope.errors.push({ type: "danger", msg: data });
				$('html, body').animate({ scrollTop: 0 }, 'fast');
			});

			$scope.quickSearchStudent = nsSelect2RemoteOptions.StudentQuickSearchRemoteOptions;

			// watch targetSection, show warning if not same year
			$scope.processMove = function () {
				if ($scope.moveSettings.studentPrimary === null || $scope.moveSettings.studentSecondary === null) {
					$bootbox.alert('You must select two students to consolidate before continuing.');
					return;
				}

				$bootbox.confirm('Are you sure you want to consolidate these two students? <br> <b>NOTE:</b> THIS IS IRREVERSIBLE.', function (response) {
					if (response) {
						// loader
						progressLoader.start();
						progressLoader.set(50);

						// do move
						mgr.consolidateStudent($scope.moveSettings.studentPrimary, $scope.moveSettings.studentSecondary).then(function (response) {
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
			}


		}])
        .controller('StudentServicesConsolidationController', ['$scope', 'nsPinesService', 'nsSelect2RemoteOptions', '$bootbox', 'progressLoader', 'NSStudentManager',
        function ($scope, nsPinesService, nsSelect2RemoteOptions, $bootbox, progressLoader, NSStudentManager) {

            var mgr = new NSStudentManager();

            $scope.resetForm = function () {
                $scope.moveSettings = { servicesPrimary: null, servicesSecondary: [], moveComplete: false };
                $scope.warnings = [];
                $scope.errors = [];
            }

            $scope.resetForm();

            $scope.canProcess = function () {
                if ($scope.moveSettings.servicesPrimary === null || $scope.moveSettings.servicesSecondary.length == 0) {
                    return false;
                }

                if ($scope.checkCircularReference() == false) {
                    return true;
                }

                return false;
            }

            $scope.checkCircularReference = function () {
                if ($scope.moveSettings.servicesPrimary === null || $scope.moveSettings.servicesSecondary.length == 0) {
                    return false;
                }

                for (var i = 0; i < $scope.moveSettings.servicesSecondary.length; i++) {
                    if ($scope.moveSettings.servicesSecondary[i].id == $scope.moveSettings.servicesPrimary.id) {
                        return true;
                    }
                }

                return false;
            }

            $scope.$on('NSHTTPError', function (event, data) {
                $scope.errors.push({ type: "danger", msg: data });
                $('html, body').animate({ scrollTop: 0 }, 'fast');
            });

            $scope.quickSearchStudentServiceSingle = nsSelect2RemoteOptions.StudentServicesSingleRemoteOptions;
            $scope.quickSearchStudentServiceMultiple = nsSelect2RemoteOptions.StudentServicesMultipleRemoteOptions;

            // watch targetSection, show warning if not same year
            $scope.processMove = function () {
                if ($scope.moveSettings.servicesPrimary === null || $scope.moveSettings.servicesSecondary.length == 0) {
                    $bootbox.alert('You must select a target and source services to consolidate before continuing.');
                    return;
                }

                $bootbox.confirm('Are you sure you want to consolidate these Student Services? <br><br> <b>NOTE:</b> THIS IS IRREVERSIBLE.', function (response) {
                    if (response) {
                        // loader
                        progressLoader.start();
                        progressLoader.set(50);

                        // do move
                        mgr.consolidateStudentServices($scope.moveSettings.servicesPrimary, $scope.moveSettings.servicesSecondary).then(function (response) {
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
            }


        }])
	.controller('StudentListController', StudentListController)
	.controller('StudentEditController', StudentEditController);

	/* Movies List Controller  */
	StudentListController.$inject = ['$scope', 'NSStudentManager', 'nsFilterOptionsService','nsPinesService', '$location', '$bootbox', '$timeout', 'spinnerService'];

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
		}

		var LoadData = function () {
			if ($scope.filterOptions.selectedGrade == null) {
				return;
			}

			$timeout(function () {
				spinnerService.show('tableSpinner');
			});

			$scope.studentManager.getStudentList(
				$scope.filterOptions.selectedSchoolYear,
				$scope.filterOptions.selectedSchool,
				$scope.filterOptions.selectedGrade,
				$scope.filterOptions.selectedTeacher,
				$scope.filterOptions.selectedSection,
				$scope.filterOptions.selectedSectionStudent)
				.then(
					function (response) {
						$scope.students = response.data.Students;
					}
				).finally(function () {
					spinnerService.hide('tableSpinner');
				});;
		};


		$scope.deleteStudent = function (id) {
			// TODO: ensure all fields are valid
			$scope.studentManager.delete(id,
				function () {
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
		}

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
		});
		//$scope.$watchGroup(['filterOptions.selectedSchoolYear', 'filterOptions.selectedGrade', 'filterOptions.selectedTeacher', 'filterOptions.selectedSection'], function (newValues, oldValues) {
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
	StudentEditController.$inject = ['$scope', '$routeParams', '$location', 'nsFilterOptionsService', 'nsPinesService', 'nsSelect2RemoteOptions', 'NSStudent', 'NSStudentManager', 'NSStudentSpedLookupValues', 'NSStudentAttributeLookups', 'NSSchoolYearsAndSchools', '$bootbox', '$uibModal'];

	function StudentEditController($scope, $routeParams, $location, nsFilterOptionsService, nsPinesService, nsSelect2RemoteOptions, NSStudent, NSStudentManager, NSStudentSpedLookupValues, NSStudentAttributeLookups, NSSchoolYearsAndSchools, $bootbox, $uibModal) {
		$scope.student = new NSStudent($routeParams.id);
		$scope.studentManager = new NSStudentManager();
		$scope.settings = { AddYear: null, AddGrade: null, AddSchool: null };
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
		
		$scope.showSectionsModal = function (registration) {
		    var modalInstance = $uibModal.open({
		        templateUrl: 'studentSectionsModal.html',
		        scope: $scope,
		        controller: function ($scope, $uibModalInstance) {
		            $scope.studentManager.getSectionsForYear(registration).then(function (response) {
		                $scope.sections = response.data.Sections;
		                $scope.studentSchool = registration;
		            })
		            $scope.cancel = function () {
		                $uibModalInstance.dismiss('cancel');
		            };
		        },
		        size: 'md',
		    });
		}

		$scope.deleteStudent = function () {
			// TODO: ensure all fields are valid
			$scope.studentManager.delete($routeParams.id,
				function () {
					nsPinesService.dataDeletedSuccessfully();
					$location.path('student-list');
				});
		};

		$scope.edit = function (registration) {
			registration.editMode = true;
			registration.newGrade = { id: registration.GradeId, text: registration.GradeName };
			registration.newSchool = { id: registration.SchoolId, text: registration.SchoolName };
			registration.newSchoolYear = { id: registration.SchoolStartYear, text: registration.SchoolYearLabel };
		}

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
		}

		$scope.studentidSetArgs = function (val, el, attrs, ngModel) {
			return { StudentIdentifier: val, StudentId: $scope.student.Id };
		}

		$scope.checkRemoteValidationStatus = function (field) {
			if ($scope.remoteValidationError === null)
			{
				return true;
			}
			else
			{
				return false;
			}
		};

		$scope.datePopupStatus = {
			opened: false
		};

		$scope.getValidationMessage = function (field) {
			return $scope.remoteValidationError;
		};

		$scope.savestudent = function () {
			// TODO: ensure all fields are valid
			$scope.studentManager.save($scope.student).then(
				function (response) {
					nsPinesService.dataSavedSuccessfully();
					$location.path('student-list');
				});
		}

		$scope.studentidSetArgs = function (val, el, attrs, ngModel) {
			return { StudentID: $scope.student.Id, studentIdentifier: $scope.student.StudentIdentifier };
		};

		$scope.checkUnique = function (studentIdentifier) {
			// if we haven't already validated this value, keep going
			if (typeof $scope.student.validatedValue === 'undefined' || $scope.student.validatedValue !== studentIdentifier.$modelValue)
			{
				if (typeof $scope.student.validating === 'undefined' || $scope.student.validating === false)
				{
					if (typeof $scope.student.Id != 'undefined') {
						$scope.student.validating = true;
						$scope.student.validateStudentIdentifer(studentIdentifier.$modelValue).then(function (result) {
							if (result.data.Success) {
								$scope.student.validating = false;
								$scope.student.validatedValue = studentIdentifier.$modelValue;
								$scope.student.LastValidationStatus = true;
								return true;
							}
							else {
								$scope.student.validating = false;
								$scope.student.validatedValue = studentIdentifier.$modelValue;
								$scope.student.LastValidationStatus = false;
								return result.data.Status;
							}

						}, function (err) {
							$scope.errors.push({ msg: '<strong>An Error Has Occurred</strong> Unable to validate the uniqueness of the Student ID.', type: 'danger' });
							$scope.student.validating = false;
							$scope.student.validatedValue = studentIdentifier.$modelValue;
						});
					}
				}
				$scope.student.validating = false;
			}
			
			return $scope.student.LastValidationStatus;
		}

		
		$scope.removeStudentSchool = function (studentSchool) {
			$scope.student.removeRegistration(studentSchool);
		}

		$scope.addStudentToSchoolAndYear = function () {
			if ($scope.student.addNewRegistration($scope.settings.AddSchool, $scope.settings.AddYear, $scope.settings.AddGrade, $scope.student.Id))
			{
				$scope.settings.AddSchool = null;
				$scope.settings.AddYear = null;
				$scope.settings.AddGrade = null;
			}
		}


		//var LoadGrades = function () {
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
	}

})();