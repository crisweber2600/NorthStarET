(function () {
    'use strict'

    angular
	.module('sectionModule', [])
	.controller('SectionListController', SectionListController)
    .controller('SectionEditController', SectionEditController)
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
			return $http.get(webApiBaseUrl + "/api/filteroptions/LoadAllGrades");
		}
		this.deleteSection = function (sectionId) {
			var returnObject = { Id: sectionId };

			return $http.post(webApiBaseUrl + "/api/section/deletesection", returnObject);
		}

		this.saveSection = function (section) {
			return $http.post(webApiBaseUrl + "/api/section/savesection", section);
		}
	}])
    .factory('NSSection', [
        '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
            var NSSection = function (sectionId) {
                this.initialize = function () {
                    var url = webApiBaseUrl + '/api/section/GetSection/' + sectionId;
                    var sectionData = $http.get(url);
                    var self = this;

                    self.Students = [];
                    self.CoTeachers = [];

                    return sectionData.then(function (response) {
                        angular.extend(self, response.data);
                        if (self.Students === null) self.Students = [];
                        if (self.CoTeachers === null) self.CoTeachers = [];
                    }, function (response) {
                        // error callback function

                    });
                }

                this.addStudentToSection = function (selectedStudent) {
                    var self = this;
                    if (selectedStudent !== undefined && selectedStudent !== null) {
                        self.Students.unshift({ id: selectedStudent.id, text: selectedStudent.LastName + ", " + selectedStudent.FirstName, isNew: true });
                        return true;
                    }
                    else {
                        alert('Please select a student first.');
                        return false;
                    }
                }

                this.removeStudent = function (id) {
                    var self = this;
                    for (var i = 0; i < self.Students.length; i++) {
                        if (self.Students[i].id === id) {
                            self.Students.splice(i, 1);
                        }
                    }
                }

                this.initialize();
            };

            return (NSSection);
        }
    ])
        .factory('NSSectionManager', [
        '$http', '$bootbox', 'nsFilterOptionsService', 'webApiBaseUrl', function ($http, $bootbox, nsFilterOptionsService, webApiBaseUrl) {
            var NSSectionManager = function () {
                this.initialize = function () {
                }

                this.getSectionList = function (schoolYear, schoolId, gradeId, teacherId) {
                    var returnObject = {
                        SchoolYear: nsFilterOptionsService.normalizeParameter(schoolYear),
                        SchoolId: nsFilterOptionsService.normalizeParameter(schoolId),
                        GradeId: nsFilterOptionsService.normalizeParameter(gradeId),
                        TeacherId: nsFilterOptionsService.normalizeParameter(teacherId)
                    };

                    return $http.post(webApiBaseUrl + "/api/section/GetSectionList", returnObject);
                };

                this.save = function (section, successCallback, failureCallback) {
                    var saveResponse = $http.post(webApiBaseUrl + "/api/section/savesection", section);
                    saveResponse.then(successCallback, failureCallback);
                };

                this.delete = function (id, successCallback, failureCallback) {
                    $bootbox.confirm("Are you sure you want to delete this section?", function (result) {
                        if (result === true) {
                            var returnObject = { Id: id };
                            var deleteResponse = $http.post(webApiBaseUrl + "/api/section/deletesection", returnObject);

                            deleteResponse.then(successCallback, failureCallback);
                        }
                    });
                };

                this.initialize();
            };

            return (NSSectionManager);
        }
        ]);

    /* Movies List Controller  */
    SectionListController.$inject = ['$scope', 'nsSectionService', 'nsFilterOptionsService', 'nsPinesService', '$bootbox', 'NSSectionManager', 'spinnerService', '$timeout'];

    function SectionListController($scope, nsSectionService, nsFilterOptionsService, nsPinesService, $bootbox, NSSectionManager, spinnerService, $timeout) {

        $scope.filterOptions = nsFilterOptionsService.options;
        $scope.sectionManager = new NSSectionManager();

        var LoadData = function () {
            $timeout(function () {
                spinnerService.show('tableSpinner');
            });
            $scope.sectionManager.getSectionList(
                $scope.filterOptions.selectedSchoolYear,
                $scope.filterOptions.selectedSchool, 
                $scope.filterOptions.selectedGrade,
                $scope.filterOptions.selectedTeacher)
                .then(
                    function (data) {
                        $scope.sections = data.data.Sections;
                        angular.forEach($scope.sections, function (sec) {
                            if (sec.CoTeachers.length > 0)
                                sec.CoTeachers = sec.CoTeachers.join(' / ');
                            else
                                sec.CoTeachers = '';
                        });
                    }
                ).finally(function () {
                    spinnerService.hide('tableSpinner');
                });
        };


        $scope.deleteSection = function (id) {
            // TODO: ensure all fields are valid
            $scope.sectionManager.delete(id,
                function () {
                    nsPinesService.dataDeletedSuccessfully();
                    LoadData();
                }, function (msg) {
                    $scope.errors.push({ msg: '<strong>An Error Has Occurred</strong> ' + msg.data, type: 'danger' });
                    $('html, body').animate({ scrollTop: 0 }, 'fast')
                });
        };

        $scope.$watch('filterOptions', function () {
            LoadData();
        }, true);
    }


    /* Movies Edit Controller */
    SectionEditController.$inject = ['$scope', '$routeParams', '$location', 'nsSectionService', 'nsFilterOptionsService', 'nsPinesService','nsSelect2RemoteOptions','NSSection','NSSectionManager'];

    function SectionEditController($scope, $routeParams, $location, nsSectionService, nsFilterOptionsService, nsPinesService, nsSelect2RemoteOptions, NSSection, NSSectionManager) {
        $scope.section = new NSSection($routeParams.id);

        // move this to a simple directive
        $scope.$on('NSHTTPError', function (event, data) {
            $scope.errors.push({ type: "danger", msg: data });
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
        //$scope.$watch('filterOptions', function () {
        //    if ($routeParams.id === "-1") {
        //        $scope.section.SchoolId = nsFilterOptionsService.normalizeParameter($scope.filterOptions.selectedSchool);
        //        $scope.section.SchoolYear = nsFilterOptionsService.normalizeParameter($scope.filterOptions.selectedSchoolYear);
        //    }
        //}, true);

        $scope.addStudentToSection = function () {
            if ($scope.section.addStudentToSection($scope.section.addStudent))
            {
                $scope.section.addStudent = null;
            }
        }

        $scope.saveSection = function () {
            // TODO: ensure all fields are valid
            $scope.section.SchoolId = $scope.filterOptions.selectedSchool.id;
            $scope.section.SchoolYear = $scope.filterOptions.selectedSchoolYear.id;

            $scope.sectionManager.save($scope.section,
                function () {
                    nsPinesService.dataSavedSuccessfully();
                    $location.path('section-list');
                }
            );
        }        

        $scope.removeStudentFromSection = function (id) {
            $scope.section.removeStudent(id);
        }

        $scope.closeAlert = function (index) {
            $scope.errors.splice(index, 1);
        };

        // don't catch any exception.  if there's an issue loading grades, it should bubble up to the http interceptor
        var LoadGrades = function () {
            nsSectionService.loadGrades().then(function (data) {
                $scope.gradeList.push.apply($scope.gradeList, data.data);
                
            });
        }


        // initial load
        LoadGrades();
        $scope.getCoTeacherRemoteOptions = nsSelect2RemoteOptions.CoTeacherRemoteOptions;
        $scope.getTeacherRemoteOptions = nsSelect2RemoteOptions.TeacherRemoteOptions;
        $scope.addStudentToSectionRemoteOptions = nsSelect2RemoteOptions.StudentToSectionRemoteOptions;
        $scope.StudentQuickSearchRemoteOptions = nsSelect2RemoteOptions.StudentQuickSearchRemoteOptions;
        $scope.StudentDetailedQuickSearchRemoteOptions = nsSelect2RemoteOptions.StudentDetailedQuickSearchRemoteOptions;
    }
})();