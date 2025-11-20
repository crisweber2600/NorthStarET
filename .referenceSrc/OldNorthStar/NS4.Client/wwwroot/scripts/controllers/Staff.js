(function () {
'use strict'

angular
    .module('staffModule', [])
    .factory('NSStaff', [
        '$http', 'webApiBaseUrl', 'nsStaffService', function ($http, webApiBaseUrl, nsStaffService) {
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
                }

                self.initialize();
            };

            return (NSStaff);
        }
    ])
    .service('nsStaffService', [
			'$http', 'pinesNotifications', 'webApiBaseUrl', function ($http, pinesNotifications, webApiBaseUrl) {
			    //this.options = {};
			    var self = this;

			    self.requestPasswordReset = function (email) {
			        return $http.post(webApiBaseUrl + '/api/passwordreset/requestpasswordreset', { UserName: email });
			    }

			    self.validateUid = function (uid) {
			        return $http.post(webApiBaseUrl + '/api/passwordreset/ValidateUID', { UID: uid });
			    }

			    self.deleteStaff = function (id) {
			        return $http.post(webApiBaseUrl + '/api/staff/deletestaff', { Id: id });
			    }

			    self.resetPasswordFromEmail = function (uid, pwd) {
			        return $http.post(webApiBaseUrl + '/api/passwordreset/ResetPasswordFromEmail', { UID: uid, Password: pwd });
			    }

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
    .controller('StaffListController', StaffListController)
    .controller('StaffEditController', StaffEditController)
    .controller('StaffPasswordResetController', StaffPasswordResetController)
    .controller('StaffPasswordResetRequestController', ['$http', '$scope', '$global', 'nsStaffService', function ($http, $scope, $global, nsStaffService) {
        var self = this;
        $global.set('fullscreen', true);
        $scope.errors = [];

        $scope.$on('NSHTTPError', function (event, data) {
            $scope.errors.push({ type: "danger", msg: data });
            $('html, body').animate({ scrollTop: 0 }, 'fast');
        });
        $scope.requestReset = function () {
            nsStaffService.requestPasswordReset($scope.userInfo.email).then(function () {
                $scope.requestSent = true; // show the thanks message
            });
        }

        $scope.userInfo = {};
    }])
    .controller('StaffPasswordResetFromLinkController', ['$http', '$scope', '$global', 'nsStaffService', '$routeParams', function ($http, $scope, $global, nsStaffService, $routeParams) {
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
            $scope.errors.push({ type: "danger", msg: data });
            $('html, body').animate({ scrollTop: 0 }, 'fast');
        });
        $scope.resetPwd = function () {
            nsStaffService.resetPasswordFromEmail($routeParams.uid, $scope.userInfo.pwd).then(function () {
                $scope.success = true; // show the thanks message
            });
        }

        $scope.userInfo = {};
        validateUID();
    }])
    .controller('StaffInfoController', [
        '$http', '$location', '$q', '$routeParams', 'webApiBaseUrl', 'authService', '$global', '$scope', 'NSUserInfoService', function ($http, $location, $q, $routeParams, webApiBaseUrl, authService, $global, $scope, NSUserInfoService) {
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

            
        }]);

	/* Movies List Controller  */
StaffListController.$inject = ['$scope', 'nsStaffService', 'nsFilterOptionsService', 'nsSelect2RemoteOptions', '$location', '$bootbox', 'nsPinesService'];

function StaffListController($scope, nsStaffService, nsFilterOptionsService, nsSelect2RemoteOptions, $location, $bootbox, nsPinesService) {

	$scope.filterOptions = nsFilterOptionsService.options;
	$scope.staffs = [];
	$scope.sortClasses = { };
	$scope.sortColumns = { column: null };
	$scope.errors = [];

	$scope.$on('NSHTTPError', function (event, data) {
	    $scope.errors = [];
	    $scope.errors.push({ type: "danger", msg: data });
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
	}

	$scope.deleteStaff = function (id) {
	    $bootbox.confirm("Are you sure you want to delete this staff member? <br><br><b>Note:</b> If the user is in use, you won't be able to delete the user and should 'disable' the account instead.",
            function (result) {
                if (result) {
                    nsStaffService.deleteStaff(id).then(function (response) {
                        $scope.errors = [];
                        nsPinesService.dataDeletedSuccessfully();
                        LoadData($scope.filterOptions.selectedSchool.id)
                    })
                }
            })
	}

	$scope.processQuickSearchStaff = function () {
	    if (angular.isDefined($scope.filterOptions.quickSearchStaff)) {
	        $location.path('staff-edit/' + $scope.filterOptions.quickSearchStaff.id);
	    } else {
	        $bootbox.alert('Please select a Staff member first.');
	    }
	}

	var LoadData = function (schoolId) {
	    if (schoolId > 0) {
	        nsStaffService.getStaffBySchool($scope.filterOptions.selectedSchool.id).success(function (data) {
	            $scope.staffs = data;
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

	StaffPasswordResetController.$inject = ['$scope', 'nsStaffService', 'nsFilterOptionsService', '$http', 'webApiBaseUrl'];

	function StaffPasswordResetController($scope, nsStaffService, nsFilterOptionsService, $http, webApiBaseUrl) {
	    $scope.userInfo = {};
	    $scope.saveData = function () {
	        $http.post(webApiBaseUrl + "/api/staff/ResetUsersPassword", { Password: $scope.userInfo.pwd, UserName: $scope.userInfo.email }).then(function (response) {
	            var result = response.data;
	        });
	    }
	}

	/* Movies Edit Controller */
	StaffEditController.$inject = ['$scope', '$routeParams', '$location', 'nsStaffService', 'nsSectionService', 'NSUserInfoService', 'NSStaff', 'authService', 'nsPinesService'];

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
	    }

	    $scope.teacherKeySetArgs = function (val, el, attrs, ngModel) {
	        return { value: val, UserId: $scope.staffid };
	    }

	    $scope.usernameSetArgs = function (val, el, attrs, ngModel) {
	        return { value: val, UserId: $scope.staffid };
	    }

	    var LoadGrades = function () {
	        nsSectionService.loadGrades().then(function (data) {
	            $scope.gradeList.push.apply($scope.gradeList, data.data);

	        });
	    }

	    $scope.select2GradeOptions = {
	        minimumInputLength: 0,
	        data: $scope.gradeList,
	        multiple: true,
	        width: 'resolve',
	    };

	    LoadGrades();

	    // initial load
	    //LoadData($routeParams.id);
	    $scope.saveData = function () {
	        nsStaffService.saveStaff($scope.staff).then(function (response) {
	            nsPinesService.dataSavedSuccessfully();

	            if ($scope.staffid == $scope.user.Id && $scope.staff.OriginalUserName != $scope.staff.Email) {
	                nsPinesService.buildMessage('You have been logged out', 'Your email address has changed and you have been logged out', 'info')
	                authService.logOut();
	            } else {
	                // TODO: notice service
	                $location.path('staff-list');
	            }
	        });
	    };

	    //nsStaffService.saveStaff($scope.staff)

	}


})();
