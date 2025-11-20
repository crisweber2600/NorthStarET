(function () {
    'use strict'

    angular
	.module('sectionModule', [])
	.controller('SectionListController', SectionListController)
    .controller('SectionEditController', SectionEditController)
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
                        self.Students.unshift({ id: selectedStudent.StudentId, text: selectedStudent.LastName + ", " + selectedStudent.FirstName, isNew: true });
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
    SectionListController.$inject = ['$scope', 'nsSectionService', 'nsFilterOptionsService', 'nsPinesService', '$bootbox', 'NSSectionManager'];

    function SectionListController($scope, nsSectionService, nsFilterOptionsService, nsPinesService, $bootbox, NSSectionManager) {

        $scope.filterOptions = nsFilterOptionsService.options;
        $scope.sectionManager = new NSSectionManager();

        var LoadData = function () {
            $scope.sectionManager.getSectionList(
                $scope.filterOptions.selectedSchoolYear,
                $scope.filterOptions.selectedSchool, 
                $scope.filterOptions.selectedGrade,
                $scope.filterOptions.selectedTeacher)
                .then(
                    function (data) {
                        $scope.sections = data.data.Sections;
                    },
                    function (msg) {
                        alert('error loading sections');
                    }
                );
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
        $scope.$watch('filterOptions', function () {
            if ($routeParams.id === "-1") {
                $scope.section.SchoolId = nsFilterOptionsService.normalizeParameter($scope.filterOptions.selectedSchool);
                $scope.section.SchoolYear = nsFilterOptionsService.normalizeParameter($scope.filterOptions.selectedSchoolYear);
            }
        }, true);

        $scope.addStudentToSection = function () {
            if ($scope.section.addStudentToSection($scope.section.addStudent))
            {
                $scope.section.addStudent = null;
            }
        }

        $scope.saveSection = function () {
            // TODO: ensure all fields are valid
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
    }
})();