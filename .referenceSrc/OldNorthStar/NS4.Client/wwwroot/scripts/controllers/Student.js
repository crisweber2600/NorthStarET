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

                this.save = function (student, successCallback, failureCallback) {
                    var saveResponse = $http.post(webApiBaseUrl + "/api/student/savestudent", student);
                    saveResponse.then(successCallback, failureCallback);
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
        '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
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
                        var momentizedDate = moment(self.DOB);
                        self.DOB = momentizedDate.toDate();
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
                this.addNewRegistration = function (school, schoolYear, studentId) {
                    var self = this;
                    if (typeof schoolYear === 'undefined' || schoolYear === null || typeof school === 'undefined' || school === null) {
                        alert('Please select a school year and school first.');
                        return false;
                    }
                    else {
                        for (var i = 0; i < self.StudentSchools.length; i++) {
                            if (self.StudentSchools[i].SchoolId == school.Id && self.StudentSchools[i].SchoolStartYear == schoolYear.SchoolStartYear) {
                                alert('Student is already enrolled at this school/school year combination.');
                                return false;
                            }
                        }

                        self.StudentSchools.unshift({ id: -1, SchoolId: school.Id, SchoolName: school.Name, SchoolStartYear: schoolYear.SchoolStartYear, SchoolYearLabel: schoolYear.YearVerbose, StudentId: studentId, isNew: true });
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
                            alert('You either do not have access to this school, or the student is already attending a section at this school.')
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
        }


    }])
	.controller('StudentListController', StudentListController)
    .controller('StudentEditController', StudentEditController);

    /* Movies List Controller  */
    StudentListController.$inject = ['$scope', 'NSStudentManager', 'nsFilterOptionsService','nsPinesService', '$location', '$bootbox'];

    function StudentListController($scope, NSStudentManager, nsFilterOptionsService, nsPinesService, $location, $bootbox) {

        $scope.filterOptions = nsFilterOptionsService.options;
        $scope.studentManager = new NSStudentManager();
        $scope.sortClasses = {};
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

        var LoadData = function () {
            //if($scope.filterOptions.selectedSchool.id !== null)
            $scope.studentManager.getStudentList(
                $scope.filterOptions.selectedSchoolYear,
                $scope.filterOptions.selectedSchool,
                $scope.filterOptions.selectedGrade,
                $scope.filterOptions.selectedTeacher,
                $scope.filterOptions.selectedSection,
                $scope.filterOptions.selectedStudent)
                .then(
                    function (data) {
                        $scope.students = data.data.Students;
                    },
                    function (msg) {
                        alert('error loading students');
                    }
                );
        };


        $scope.deleteStudent = function (id) {
            // TODO: ensure all fields are valid
            $scope.studentManager.delete(id,
                function () {
                    nsPinesService.dataDeletedSuccessfully();
                    LoadData();
                }, function (msg) {
                    $scope.errors.push({ msg: '<strong>An Error Has Occurred</strong> ' + msg.data, type: 'danger' });
                    $('html, body').animate({ scrollTop: 0 }, 'fast')
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
        $scope.$watch('filterOptions.selectedGrade', function (newVal) {
            if (newVal !== null) {
                LoadData();
            }
        });
        $scope.$watch('filterOptions.selectedTeacher', function (newVal) {
            if (newVal !== null) {
                LoadData();
            }
        });
        $scope.$watch('filterOptions.selectedSection', function (newVal) {
            if (newVal !== null) {
                LoadData();
            }
        });

        // initial load
        LoadData();
    }


    /* Movies Edit Controller */
    StudentEditController.$inject = ['$scope', '$routeParams', '$location', 'nsFilterOptionsService', 'nsPinesService', 'nsSelect2RemoteOptions', 'NSStudent', 'NSStudentManager', 'NSStudentSpedLookupValues', 'NSStudentAttributeLookups', 'NSSchoolYearsAndSchools'];

    function StudentEditController($scope, $routeParams, $location, nsFilterOptionsService, nsPinesService, nsSelect2RemoteOptions, NSStudent, NSStudentManager, NSStudentSpedLookupValues, NSStudentAttributeLookups, NSSchoolYearsAndSchools) {
        $scope.student = new NSStudent($routeParams.id);
        $scope.studentManager = new NSStudentManager();
        $scope.remoteStudentIdValidationPath = $scope.studentManager.remoteStudentIdValidationPath;
        $scope.student.SpecialEdLabels = [];
        $scope.student.LastValidationStatus = true;
        $scope.student.StudentAttributes = {};
        $scope.filterOptions = nsFilterOptionsService.options;
        $scope.allSpedLabels = new NSStudentSpedLookupValues();
        $scope.allAttributes = new NSStudentAttributeLookups();
        $scope.errors = [];
        $scope.studentSpedLabelRemoteOptions = nsSelect2RemoteOptions.StudentSpedLabelRemoteOptions;
        $scope.schoolsAndYears = new NSSchoolYearsAndSchools();
        $scope.remoteValidationError = {};

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
            $scope.studentManager.save($scope.student,
                function () {
                    nsPinesService.dataSavedSuccessfully();
                    $location.path('student-list');
                }, function (msg) {
                    $scope.errors.push({ msg: '<strong>An Error Has Occurred</strong> ' + msg.data, type: 'danger' });
                    $('html, body').animate({ scrollTop: 0 }, 'fast')
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
            if($scope.student.addNewRegistration($scope.AddSchool, $scope.AddYear, $scope.student.Id))
            {
                $scope.AddSchool = null;
                $scope.AddYear = null;
            }
        }

        $scope.closeAlert = function (index) {
            $scope.errors.splice(index, 1);
        };

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