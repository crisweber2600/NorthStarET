(function () {
    'use strict';

    angular
        .module('personalSettingsModule', [])
        .controller('PersonalFieldsController', ['$scope', '$routeParams', '$location', 'nsSectionService', 'nsFilterOptionsService', 'nsPinesService', 'nsSelect2RemoteOptions', 'NSPersonalFieldsManager',
            function PersonalFieldsController($scope, $routeParams, $location, nsSectionService, nsFilterOptionsService, nsPinesService, nsSelect2RemoteOptions, NSPersonalFieldsManager) {
                $scope.fieldsManager = new NSPersonalFieldsManager();
                $scope.assessments = $scope.fieldsManager.Assessments;
                $scope.errors = [];
                $scope.oneAtATime = true;

                $scope.changeFieldStatus = function (assessmentId, fieldId, status, hideFieldFrom) {
                    $scope.fieldsManager.changeFieldStatus(assessmentId, fieldId, status, hideFieldFrom,
                       function () {
                           nsPinesService.dataSavedSuccessfully();
                       }, function (msg) {
                           $scope.errors.push({ msg: '<strong>An Error Has Occurred</strong> ' + msg.data, type: 'danger' });
                           $('html, body').animate({ scrollTop: 0 }, 'fast')
                       });
                }

                $scope.removeStudentFromSection = function (id) {
                    $scope.section.removeStudent(id);
                }

                $scope.closeAlert = function (index) {
                    $scope.errors.splice(index, 1);
                };

                // initial load
                //$scope.getCoTeacherRemoteOptions = nsSelect2RemoteOptions.CoTeacherRemoteOptions;
                //$scope.getStaffGroupRemoteOptions = nsSelect2RemoteOptions.StaffGroupRemoteOptions;
            }
        ])
        .controller('PersonalPasswordChangeController', ['$scope', '$routeParams', '$location', 'nsSectionService', 'nsFilterOptionsService', 'nsPinesService', 'nsSelect2RemoteOptions', 'NSUserInfoService',
            function PersonalFieldsController($scope, $routeParams, $location, nsSectionService, nsFilterOptionsService, nsPinesService, nsSelect2RemoteOptions, NSUserInfoService) {
                $scope.user = NSUserInfoService.currentUser;
                $scope.userInfo = {};
                $scope.settings = {};
                $scope.errors = [];

                $scope.$on('NSHTTPError', function (event, data) {
                    $scope.errors.push({ type: "danger", msg: data });
                    $('html, body').animate({ scrollTop: 0 }, 'fast');
                });

                $scope.passwordValidator = function (password) {

                    if (!password) { return; }

                    if (password.length < 6) {
                        return "Password must be at least " + 6 + " characters long";
                    }

                    return true;
                };

                $scope.changePassword = function () {
                    NSUserInfoService.changePassword($scope.user.Email, $scope.userInfo.pwd).then(function (response) {
                        $scope.settings.pageStatus = 'success';
                    });
                }
            }
        ])
        .controller('PersonalUsernameChangeController', ['$scope', '$routeParams', '$location', 'nsSectionService', 'nsFilterOptionsService', 'nsPinesService', 'nsSelect2RemoteOptions', 'NSUserInfoService','authService','$parse',
            function PersonalUsernameChangeController($scope, $routeParams, $location, nsSectionService, nsFilterOptionsService, nsPinesService, nsSelect2RemoteOptions, NSUserInfoService, authService, $parse) {
                $scope.user = NSUserInfoService.currentUser;
                $scope.remoteUsernameValidationPath = NSUserInfoService.remoteUsernameValidationPath;
                $scope.remoteEmailFormatValidationPath = NSUserInfoService.remoteEmailFormatValidationPath;
                $scope.userInfo = {};
                $scope.settings = { };
                $scope.errors = [];
                $scope.validationObject = function () {
                    var usernamePath = $scope.remoteUsernameValidationPath;
                    var emailPath = $scope.remoteEmailFormatValidationPath;
                    return {
                        usernamePath : 'unique',
                        emailPath : 'emailformat'
                        };
                }
                
                $scope.$on('NSHTTPError', function (event, data) {
                    $scope.errors.push({ type: "danger", msg: data });
                    $('html, body').animate({ scrollTop: 0 }, 'fast');
                });

                $scope.logOut = function () {
                    authService.logOut();
                }

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
                }
            }
         ])
        .controller('PersonalProfileChangeController', ['$scope', '$routeParams', '$location', 'nsSectionService', 'nsFilterOptionsService', 'nsPinesService', 'nsSelect2RemoteOptions', 'NSUserInfoService','$global',
            function PersonalFieldsController($scope, $routeParams, $location, nsSectionService, nsFilterOptionsService, nsPinesService, nsSelect2RemoteOptions, NSUserInfoService, $global) {
                $scope.user = NSUserInfoService.currentUser;
                $scope.userInfo = {};
                $scope.settings = { teachKeyValid: true, teachKeyInvalidMessage: null };
                $scope.errors = [];
                $scope.remoteValidationPath = NSUserInfoService.remoteTeacherKeyValidationPath;

                $scope.checkTeacherKeyValidationStatus = function () {
                    return $scope.settings.teachKeyValid;
                }

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
                    })
                }

                $scope.$on('NSHTTPError', function (event, data) {
                    $scope.errors.push({ type: "danger", msg: data });
                    $('html, body').animate({ scrollTop: 0 }, 'fast');
                });

                $scope.updateProfile = function () {
                    NSUserInfoService.updateProfile($scope.userInfo.firstName, $scope.userInfo.middleName, $scope.userInfo.lastName, $scope.userInfo.isInterventionist, $scope.userInfo.role, $scope.userInfo.teacherKey).then(function (response) {
                        $scope.settings.pageStatus = 'success';

                        // now force update of userinfservice
                        $global.set('userprofileupdated', true);
                    });
                }
            }
        ])
        .controller('PersonalContactNorthStarSupportController', ['$scope', '$routeParams', '$location', 'nsSectionService', 'nsFilterOptionsService', 'nsPinesService', 'nsSelect2RemoteOptions', 'NSUserInfoService', '$global',
            function PersonalContactNorthStarSupportController($scope, $routeParams, $location, nsSectionService, nsFilterOptionsService, nsPinesService, nsSelect2RemoteOptions, NSUserInfoService, $global) {
                $scope.user = NSUserInfoService.currentUser;
                $scope.info = {};
                $scope.settings = {};
                $scope.errors = [];

                $scope.resetForm = function () {
                    $scope.settings.pageStatus = null;
                    $scope.info = {};
                }

                $scope.$on('NSHTTPError', function (event, data) {
                    $scope.errors.push({ type: "danger", msg: data });
                    $('html, body').animate({ scrollTop: 0 }, 'fast');
                });

                $scope.sendMail = function (subject, message) {
                    NSUserInfoService.sendSupportMail(subject, message).then(function (response) {
                        $scope.settings.pageStatus = 'success';
                    });
                }

                $scope.getAdminsRemoteOptions = nsSelect2RemoteOptions.SchoolAdminRemoteOptions;
            }
                ])
        .controller('PersonalContactSchoolAdminController', ['$scope', '$routeParams', '$location', 'nsSectionService', 'nsFilterOptionsService', 'nsPinesService', 'nsSelect2RemoteOptions', 'NSUserInfoService', '$global',
            function PersonalFieldsController($scope, $routeParams, $location, nsSectionService, nsFilterOptionsService, nsPinesService, nsSelect2RemoteOptions, NSUserInfoService, $global) {
                $scope.user = NSUserInfoService.currentUser;
                $scope.info = {};
                $scope.settings = {};
                $scope.errors = [];
    
                $scope.resetForm = function () {
                    $scope.settings.pageStatus = null;
                    $scope.info = {};
                }

                $scope.$on('NSHTTPError', function (event, data) {
                    $scope.errors.push({ type: "danger", msg: data });
                    $('html, body').animate({ scrollTop: 0 }, 'fast');
                });

                $scope.sendMail = function (to, subject, message) {
                    NSUserInfoService.sendMail(to, subject, message).then(function (response) {
                        $scope.settings.pageStatus = 'success';
                    });
                }

                $scope.getAdminsRemoteOptions = nsSelect2RemoteOptions.SchoolAdminRemoteOptions;
            }
        ])
        .controller('PersonalContactDistrictAdminController', ['$scope', '$routeParams', '$location', 'nsSectionService', 'nsFilterOptionsService', 'nsPinesService', 'nsSelect2RemoteOptions', 'NSUserInfoService', '$global',
            function PersonalContactDistrictAdminController($scope, $routeParams, $location, nsSectionService, nsFilterOptionsService, nsPinesService, nsSelect2RemoteOptions, NSUserInfoService, $global) {
                $scope.user = NSUserInfoService.currentUser;
                $scope.info = {};
                $scope.settings = {};
                $scope.errors = [];

                $scope.resetForm = function () {
                    $scope.settings.pageStatus = null;
                    $scope.info = {};
                }

                $scope.$on('NSHTTPError', function (event, data) {
                    $scope.errors.push({ type: "danger", msg: data });
                    $('html, body').animate({ scrollTop: 0 }, 'fast');
                });

                $scope.sendMail = function (to, subject, message) {
                    NSUserInfoService.sendMail(to, subject, message).then(function (response) {
                        $scope.settings.pageStatus = 'success';
                    });
                }

                $scope.getAdminsRemoteOptions = nsSelect2RemoteOptions.DistrictAdminRemoteOptions;
            }
                ])
        .service('NSUserInfoService', [
			'$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
			    var self = this;
			    self.currentUser = {};

			    self.remoteTeacherKeyValidationPath = webApiBaseUrl + "/api/staff/ValidateTeacherKeyChange";
			    self.remoteUsernameValidationPath = webApiBaseUrl + "/api/staff/ValidateUsernameChange";
			    self.remoteTeacherKeyOtherUserValidationPath = webApiBaseUrl + "/api/staff/ValidateTeacherKeyChangeOtherUser";
			    self.remoteUsernameOtherUserValidationPath = webApiBaseUrl + "/api/staff/ValidateUsernameChangeOtherUser";
			    self.remoteEmailFormatValidationPath = webApiBaseUrl + "/api/staff/ValidateEmailFormat";

			    //self.validateUniqueTeacherKey = function (teacherKey) {
			    //    var paramObj = { text: teacherKey };
			    //    return $http.post(webApiBaseUrl + "/api/staff/ValidateTeacherKeyChange", paramObj);
			    //};

			    //self.validateUniqueUsername = function (username) {
			    //    var paramObj = { text: username };
			    //    return $http.post(webApiBaseUrl + "/api/staff/ValidateUsernameChange", paramObj);
			    //};

			    self.sendMail = function (to, subject, message) {
			        var paramObj = { ToId: to, Subject: subject, Message: message };
			        return $http.post(webApiBaseUrl + "/api/staff/sendmail", paramObj);
			    }

			    self.sendSupportMail = function (subject, message) {
			        var paramObj = { ToId: -1, Subject: subject, Message: message };
			        return $http.post(webApiBaseUrl + "/api/staff/sendsupportmail", paramObj);
			    }

			    self.loadUserInfo = function () {
			        return $http.get(webApiBaseUrl + "/api/staff/myinfo").then(function (response) {
			            angular.extend(self.currentUser, response.data);
			        });
			    };
			    self.changePassword = function (email, pwd) {
                    // TODO: use one that doesn't specify a username, or check that username matches current user
			        return $http.post(webApiBaseUrl + "/api/PasswordReset/ResetUsersPassword", { Password: pwd, UserName: email }).then(function (response) {
			            var result = response.data;                        
			        });
			    };
			    self.changeUsername = function (username) {
			        // TODO: use one that doesn't specify a username, or check that username matches current user
			        return $http.post(webApiBaseUrl + "/api/PasswordReset/ChangeUsername", { value: username }).then(function (response) {
			            var result = response.data;
			        });
			    };


			    self.updateProfile = function (firstName, middleName, lastName, isInterventionist, role, teacherKey) {
			        // TODO: use one that doesn't specify a username, or check that username matches current user
			        return $http.post(webApiBaseUrl + "/api/staff/UpdateUserProfile", { FirstName: firstName, MiddleName: middleName, LastName: lastName, IsInterventionSpecialist: isInterventionist, RoleID: role, TeacherIdentifier: teacherKey }).then(function (response) {
			            var result = response.data;
			        });
			    };
			}])
        .factory('NSPersonalFieldsManager', [
            '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
                var NSPersonalFieldsManager = function () {
                    this.initialize = function () {
                        var url = webApiBaseUrl + '/api/personalsettings/GetAssessmentsAndFieldsForUser';
                        var personalData = $http.get(url);
                        var self = this;

                        self.Assessments = [];

                        personalData.then(function (response) {
                            angular.extend(self, response.data);
                            if (self.Assessments === null) self.Assessments = [];
                        }, function (response) {
                            // error callback function

                        });
                    }

                    this.changeFieldStatus = function (assessmentId, fieldId, status, hideFieldFrom, successCallback, failureCallback) {
                        var returnObject = { AssessmentId: assessmentId, FieldId: fieldId, HiddenStatus: status, HideFieldFrom: hideFieldFrom };
                        var saveResponse = $http.post(webApiBaseUrl + "/api/personalsettings/UpdateFieldForUser", returnObject);
                        saveResponse.then(successCallback, failureCallback);
                    };

                    this.initialize();
                };

                return (NSPersonalFieldsManager);
            }
        ]);

})();