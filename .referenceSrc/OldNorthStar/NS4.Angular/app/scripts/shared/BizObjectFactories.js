'use strict'

angular
	.module('northstar.business', [])
 
    .factory('NSStudentSectionResultDataEntry', [
        '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
            var NSStudentSectionResultDataEntry = function () {

                this.loadAssessmentStudentResultData = function (assessmentId, selectedSectionId, selectedBenchmarkDateId, studentId, studentResultId) {
                    var postObject = { AssessmentId: assessmentId, SectionId: selectedSectionId, BenchmarkDateId: selectedBenchmarkDateId, StudentId: studentId, StudentResultId: studentResultId };
                    var url = webApiBaseUrl + '/api/sectiondataentry/GetStudentAssessmentResult';
                    return $http.post(url, postObject);
                }

                this.loadProgMonStudentResultData = function (assessmentId, interventionGroupId, studentId, studentResultId) {
                    var postObject = { AssessmentId: assessmentId, InterventionGroupId: interventionGroupId, StudentId: studentId, StudentResultId: studentResultId };
                    var url = webApiBaseUrl + '/api/sectiondataentry/GetStudentProgressMonResult';
                    return $http.post(url, postObject);
                }

                this.attachFieldsToResults = function (studentResult, fieldsArray, lookupFieldsArray) {
                    console.time("Start attach fields");
                        for (var k = 0; k < studentResult.FieldResults.length; k++) {
                            for (var r = 0; r < fieldsArray.length; r++) {
                                if (fieldsArray[r].DatabaseColumn == studentResult.FieldResults[k].DbColumn) {
                                    studentResult.FieldResults[k].Field = angular.copy(fieldsArray[r]);

                                    // set display value
                                    if (fieldsArray[r].FieldType === "DropdownFromDB") {
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
                  
                    console.timeEnd("Start attach fields");
                    // set initial display values
                };
                
                this.saveAssessmentResult = function (assessmentId, studentResult, benchmarkDateId) {

                    var returnObject = {
                        StudentResult: studentResult,
                        AssessmentId: assessmentId,
                        BenchmarkDateId: benchmarkDateId
                    }

                    return $http.post(webApiBaseUrl + "/api/dataentry/SaveAssessmentResult", returnObject);
                };

                this.saveProgMonResult = function (assessmentId, studentResult, interventionGroupId) {

                    var returnObject = {
                        StudentResult: studentResult,
                        AssessmentId: assessmentId,
                        InterventionGroupId: interventionGroupId
                    }

                    return $http.post(webApiBaseUrl + "/api/dataentry/SaveProgMonResult", returnObject);
                };

            };

            return (NSStudentSectionResultDataEntry);
        }
    ])
        .factory('NSSchoolYearsAndSchools', [
        '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
            var NSSchoolYearsAndSchools = function (studentId) {
                this.initialize = function () {
                    var url = webApiBaseUrl + '/api/assessment/GetUpdatedFilterOptions';
                    var schoolsAndYears = $http.post(url, { ChangeType: 'initial' });
                    var self = this;

                    self.Schools = [];
                    self.SchoolYears = {};

                    schoolsAndYears.then(function (response) {
                        angular.extend(self, response.data);
                        if (self.Schools === null) self.Schools = [];
                        if (self.SchoolYears === null) self.SchoolYears = [];
                    }, function (response) {
                        // error callback function

                    });
                }

                this.initialize();
            };

            return (NSSchoolYearsAndSchools);
        }
        ])



        .factory('NSStudentAttributeLookups', [
        '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
            var NSStudentAttributeLookups = function (studentId) {
                this.initialize = function () {
                    var url = webApiBaseUrl + '/api/student/GetStudentAttributeLookups';
                    var attributeLookups = $http.get(url);
                    var self = this;

                    self.AllAttributes = [];

                    attributeLookups.then(function (response) {
                        angular.extend(self, response.data);
                        if (self.AllAttributes === null) self.AllAttributes = [];
                    }, function (response) {
                        // error callback function

                    });
                }

                this.initialize();
            };

            return (NSStudentAttributeLookups);
        }
        ])
            .factory('NSStudentSpedLookupValues', [
        '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
            var NSStudentSpedLookupValues = function (studentId) {
                this.initialize = function () {
                    var url = webApiBaseUrl + '/api/student/GetStudentSpedLabelLookups';
                    var attributeLookups = $http.get(url);
                    var self = this;

                    self.AllSpedLabels = [];

                    attributeLookups.then(function (response) {
                        angular.extend(self, response.data);
                        if (self.AllSpedLabels === null) self.AllSpedLabels = [];
                    }, function (response) {
                        // error callback function

                    });
                }

                this.initialize();
            };

            return (NSStudentSpedLookupValues);
        }
            ])

   
 


    .factory('NSObservationSummaryTeamMeetingManager', [
        '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
            var NSObservationSummaryTeamMeetingManager = function () {
                this.initialize = function () {

                }

                this.LoadData = function (teamMeetingId, tddId, staffId) {
                    var paramObj = { TeamMeetingId: teamMeetingId, TestDueDateId: tddId, StaffId: staffId };
                    var url = webApiBaseUrl + '/api/assessment/GetTeamMeetingObservationSummary/';
                    //var paramObj = {}
                    var summaryData = $http.post(url, paramObj);
                    var self = this;

                    self.LookupLists = [];
                    self.Scores = [];
                    self.BenchmarksByGrade = [];

                    return summaryData.then(function (response) {
                        angular.extend(self, response.data);
                        if (self.LookupLists === null) self.LookupLists = [];
                        if (self.Scores === null) self.Scores = [];
                        if (self.BenchmarksByGrade === null) self.BenchmarksByGrade = [];
                  
                    }, function (response) {
                        // error callback function

                    });
                }
            };

            return (NSObservationSummaryTeamMeetingManager);
        }
    ])
        .factory('NSDistrictAssessmentAvailabilityManager', [
        '$http', 'webApiBaseUrl', function ($http, webApiBaseUrl) {
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
                    var paramObj = { Id: availability.Id, AssessmentIsAvailable: availability.AssessmentIsAvailable };
                    var url = webApiBaseUrl + '/api/AssessmentAvailability/UpdateAssessmentAvailability/';
                    var promise = $http.post(url, paramObj);

                    return promise.then(function (response) {
                        return response.data.isValid;
                    });
                };

                self.initialize();
            };

            return (NSDistrictAssessmentAvailabilityManager);
        }
        ])
            .factory('NSSchoolAssessmentAvailabilityManager', [
      '$http', '$routeParams', 'webApiBaseUrl', function ($http, $routeParams, webApiBaseUrl) {
          var NSSchoolAssessmentAvailabilityManager = function () {
              var self = this;


              self.initialize = function (school) {
                  if (school == null || angular.isUndefined(school))
                  {
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
                  var paramObj = { AssessmentId: availability.AssessmentId, SchoolId: availability.SchoolId, AssessmentIsAvailable: availability.AssessmentIsAvailable };
                  var url = webApiBaseUrl + '/api/AssessmentAvailability/UpdateSchoolAssessmentAvailability/';
                  var promise = $http.post(url, paramObj);

                  return promise.then(function (response) {
                      return response.data.isValid;
                  });
              };
          };

         

          return (NSSchoolAssessmentAvailabilityManager);
        }
            ])
        .factory('NSStaffAssessmentAvailabilityManager', [
      '$http', '$routeParams', 'webApiBaseUrl', function ($http, $routeParams, webApiBaseUrl) {
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
                  var paramObj = { AssessmentId: availability.AssessmentId, StaffId: availability.StaffId, AssessmentIsAvailable: availability.AssessmentIsAvailable };
                  var url = webApiBaseUrl + '/api/AssessmentAvailability/UpdateStaffAssessmentAvailability/';
                  var promise = $http.post(url, paramObj);

                  return promise.then(function (response) {
                      return response.data.isValid;
                  });
              };

              self.initialize();
          };



          return (NSStaffAssessmentAvailabilityManager);
      }
                ])
   
    .factory('NSSortManager', [
        '$http','$location', function ($http, $location) {
            var NSSortManager = function () {
                var self = this;
                self.manualSortHeaders = {};
                self.sortArray = [];
                self.headerClassArray = [];
                self.fieldResultName = '';
                self.fieldsParent = {};
                self.initializedOnce = false;

                self.initialize = function (manualSortHeaders, sortArray, headerClassArray, fieldResultName, fieldsParent) {
                    self.manualSortHeaders = manualSortHeaders;
                    self.sortArray = sortArray;
                    self.headerClassArray = headerClassArray;
                    self.fieldResultName = fieldResultName;
                    self.fieldsParent = fieldsParent;

                    // set sort settings if printing
                    if ($location.absUrl().indexOf('SortParam=') > 0 && self.initializedOnce == false) {
                        self.initializedOnce = true;
                        var sortParamArray = $location.search().SortParam.split(',');

                        for (var i = 0; i < sortParamArray.length; i++) {
                            var currentParam = decodeURIComponent(sortParamArray[i]);

                            var matches = currentParam.match(/\[(.*?)\]/);
                            var doubleUp = currentParam.indexOf('-') > -1;

                            // if we are able to get a number
                            if (matches) {
                                var index = matches[1];

                                if ($location.absUrl().indexOf('spell-section-report') > 0) {
                                    self.funkySort(parseInt(index) - 1, parseInt(index));
                                    if (doubleUp) {
                                        self.funkySort(parseInt(index) - 1, parseInt(index));
                                    }
                                } else {
                                    self.sort(index);
                                    if (doubleUp) {
                                        self.sort(index);
                                    }
                                }

                            } else {
                                // if we are on the spelling section report, check for '-' and then forcibly sort twice
                                //if ($location.absUrl().indexOf('spell-section-report') > 0) {
                                    if (doubleUp) {
                                        self.sort(currentParam.substr(1, 500));
                                        self.sort(currentParam.substr(1, 500));
                                    } else {
                                        self.sort(currentParam);
                                    }
                                //}
                                //else {
                                 //   self.sort(currentParam);
                                 //   if (doubleUp) {
                                 //       self.sort(currentParam);
                                 //   }
                                //}
                            }
                        }
                    }
                };

                self.funkySort = function (column, altIndex) {

                    var columnIndex = -1;
                    // if this is not a first or lastname column
                    if (!isNaN(parseInt(column))) {
                        if (column >= 0) { 
                            columnIndex = column;
                            switch (self.fieldsParent.Fields[column].FieldType) {
                                case 'Textfield':
                                    column = self.fieldResultName + '[' + altIndex + '].StringValue';
                                    break;
                                case 'DecimalRange':
                                    column = self.fieldResultName + '[' + altIndex + '].DecimalValue';
                                    break;
                                case 'DropdownRange':
                                    column = self.fieldResultName + '[' + altIndex + '].IntValue';
                                    break;
                                case 'DropdownFromDB':
                                    column = self.fieldResultName + '[' + altIndex + '].IntValue';
                                    break;
                                case 'CalculatedFieldDbOnly':
                                    column = self.fieldResultName + '[' + altIndex + '].StringValue';
                                    break;
                                case 'CalculatedFieldDbBacked':
                                    column = self.fieldResultName + '[' + altIndex + '].IntValue';
                                    break;
                                case 'CalculatedFieldDbBackedString':
                                    column = self.fieldResultName + '[' + altIndex + '].StringValue';
                                    break;
                                case 'CalculatedFieldClientOnly':
                                    column = self.fieldResultName + '[' + altIndex + '].StringValue'; //shouldnt even be used in sorting
                                    break;
                                default:
                                    column = self.fieldResultName + '[' + altIndex + '].IntValue';
                                    break;
                            }
                        } else {
                            column = 'FieldResults[0].IntValue';
                        }
                    }
                    var bFound = false;
                    for (var j = 0; j < self.sortArray.length; j++) {
                        // if it is already on the list, reverse the sort
                        if (self.sortArray[j].indexOf(column) >= 0) {
                            bFound = true;

                            // is it already negative? if so, remove it
                            if (self.sortArray[j].indexOf("-") === 0) {
                                if (columnIndex > -1) {
                                    self.headerClassArray[columnIndex] = "fa";
                                } else if (column === 'FirstName') {
                                    self.manualSortHeaders.firstNameHeaderClass = "fa";
                                } else if (column === 'LastName') {
                                    self.manualSortHeaders.lastNameHeaderClass = "fa";
                                } else if (column === 'StudentName') {
                                    self.manualSortHeaders.studentNameHeaderClass = "fa";
                                } else if (column === 'SchoolName') {
                                    self.manualSortHeaders.schoolNameHeaderClass = "fa";
                                } else if (column === 'GradeOrder') {
                                    self.manualSortHeaders.gradeNameHeaderClass = "fa";
                                } else if (column === 'DelimitedTeacherSections') {
                                    self.manualSortHeaders.teacherNameHeaderClass = "fa";
                                } else if (column === 'FPValueID') {
                                    self.manualSortHeaders.fpValueIDHeaderClass = "fa";
                                } else if (column === 'FieldResults[0].IntValue') {
                                    self.manualSortHeaders.totalScoreHeaderClass = "fa";
                                } else if (column === 'TestDate') {
                                    self.manualSortHeaders.tddHeaderClass = "fa";
                                } else if (column === 'DelimitedTeachers') {
                                    self.manualSortHeaders.teachersHeaderClass = "fa";
                                } else if (column === 'GuidedReadingGroupId') {
                                    self.manualSortHeaders.guidedReadingGroupClass = "fa";
                                }
                                self.sortArray.splice(j, 1);
                            } else {
                                if (columnIndex > -1) {
                                    self.headerClassArray[columnIndex] = "fa fa-chevron-down";
                                } else if (column === 'FirstName') {
                                    self.manualSortHeaders.firstNameHeaderClass = "fa fa-chevron-down";
                                } else if (column === 'LastName') {
                                    self.manualSortHeaders.lastNameHeaderClass = "fa fa-chevron-down";
                                } else if (column === 'StudentName') {
                                    self.manualSortHeaders.studentNameHeaderClass = "fa fa-chevron-down";
                                } else if (column === 'SchoolName') {
                                    self.manualSortHeaders.schoolNameHeaderClass = "fa fa-chevron-down";
                                } else if (column === 'GradeOrder') {
                                    self.manualSortHeaders.gradeNameHeaderClass = "fa fa-chevron-down";
                                } else if (column === 'DelimitedTeacherSections') {
                                    self.manualSortHeaders.teacherNameHeaderClass = "fa fa-chevron-down";
                                } else if (column === 'FPValueID') {
                                    self.manualSortHeaders.fpValueIDHeaderClass = "fa fa-chevron-down";
                                } else if (column === 'FieldResults[0].IntValue') {
                                    self.manualSortHeaders.totalScoreHeaderClass = "fa fa-chevron-down";
                                } else if (column === 'TestDate') {
                                    self.manualSortHeaders.tddHeaderClass = "fa fa-chevron-down";
                                } else if (column === 'DelimitedTeachers') {
                                    self.manualSortHeaders.teachersHeaderClass = "fa fa-chevron-down";
                                } else if (column === 'GuidedReadingGroupId') {
                                    self.manualSortHeaders.guidedReadingGroupClass = "fa fa-chevron-dwon";
                                }
                                self.sortArray[j] = "-" + self.sortArray[j];
                            }
                            break;
                        }
                    }
                    if (!bFound) {
                        self.sortArray.push(column);

                        if (columnIndex > -1) {
                            self.headerClassArray[columnIndex] = "fa fa-chevron-up";
                        } else if (column === 'FirstName') {
                            self.manualSortHeaders.firstNameHeaderClass = "fa fa-chevron-up";
                        } else if (column === 'LastName') {
                            self.manualSortHeaders.lastNameHeaderClass = "fa fa-chevron-up";
                        } else if (column === 'StudentName') {
                            self.manualSortHeaders.studentNameHeaderClass = "fa fa-chevron-up";
                        } else if (column === 'SchoolName') {
                            self.manualSortHeaders.schoolNameHeaderClass = "fa fa-chevron-up";
                        } else if (column === 'GradeOrder') {
                            self.manualSortHeaders.gradeNameHeaderClass = "fa fa-chevron-up";
                        } else if (column === 'DelimitedTeacherSections') {
                            self.manualSortHeaders.teacherNameHeaderClass = "fa fa-chevron-up";
                        } else if (column === 'FPValueID') {
                            self.manualSortHeaders.fpValueIDHeaderClass = "fa fa-chevron-up";
                        } else if (column === 'FieldResults[0].IntValue') {
                            self.manualSortHeaders.totalScoreHeaderClass = "fa fa-chevron-up";
                        } else if (column === 'TestDate') {
                            self.manualSortHeaders.tddHeaderClass = "fa fa-chevron-up";
                        } else if (column === 'DelimitedTeachers') {
                            self.manualSortHeaders.teachersHeaderClass = "fa fa-chevron-up";
                        }
                    }
                };

                self.sort = function (column) {

                    var columnIndex = -1;
                    // if this is not a first or lastname column
                    if (!isNaN(parseInt(column))) {
                        columnIndex = column;
                        switch (self.fieldsParent.Fields[column].FieldType) {
                            case 'checklist':
                                column = self.fieldResultName + '[' + column + '].DisplayValue';
                                break;
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
                                column = self.fieldResultName + '[' + column + '].StringValue'; //shouldnt even be used in sorting
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
                            if (self.sortArray[j].indexOf("-") === 0) {
                                if (columnIndex > -1) {
                                    self.headerClassArray[columnIndex] = "fa";
                                } else {
                                    self.manualSortHeaders[column] = "fa";
                                }

                                //else if (column === 'FirstName') {
                                //    self.manualSortHeaders.firstNameHeaderClass = "fa";
                                //} else if (column === 'LastName') {
                                //    self.manualSortHeaders.lastNameHeaderClass = "fa";
                                //} else if (column === 'StudentName') {
                                //    self.manualSortHeaders.studentNameHeaderClass = "fa";
                                //} else if (column === 'SchoolName') {
                                //    self.manualSortHeaders.schoolNameHeaderClass = "fa";
                                //} else if (column === 'GradeOrder') {
                                //    self.manualSortHeaders.gradeNameHeaderClass = "fa";
                                //} else if (column === 'DelimitedTeacherSections') {
                                //    self.manualSortHeaders.teacherNameHeaderClass = "fa";
                                //} else if (column === 'FPValueID') {
                                //    self.manualSortHeaders.fpValueIDHeaderClass = "fa";
                                //} else if (column === 'FieldResults[0].IntValue') {
                                //    self.manualSortHeaders.totalScoreHeaderClass = "fa";
                                //} else if (column === 'TestDate') {
                                //    self.manualSortHeaders.tddHeaderClass = "fa";
                                //} else if (column === 'DelimitedTeachers') {
                                //    self.manualSortHeaders.teachersHeaderClass = "fa";
                                //} else if (column === 'GuidedReadingGroupId') {
                                //    self.manualSortHeaders.guidedReadingGroupClass = "fa";
                                //}
                                self.sortArray.splice(j, 1);
                            } else {
                                if (columnIndex > -1) {
                                    self.headerClassArray[columnIndex] = "fa fa-chevron-down";
                                } else {
                                    self.manualSortHeaders[column] = "fa fa-chevron-down";
                                }
                                //} else if (column === 'FirstName') {
                                //    self.manualSortHeaders.firstNameHeaderClass = "fa fa-chevron-down";
                                //} else if (column === 'LastName') {
                                //    self.manualSortHeaders.lastNameHeaderClass = "fa fa-chevron-down";
                                //} else if (column === 'StudentName') {
                                //    self.manualSortHeaders.studentNameHeaderClass = "fa fa-chevron-down";
                                //} else if (column === 'SchoolName') {
                                //    self.manualSortHeaders.schoolNameHeaderClass = "fa fa-chevron-down";
                                //} else if (column === 'GradeOrder') {
                                //    self.manualSortHeaders.gradeNameHeaderClass = "fa fa-chevron-down";
                                //} else if (column === 'DelimitedTeacherSections') {
                                //    self.manualSortHeaders.teacherNameHeaderClass = "fa fa-chevron-down";
                                //} else if (column === 'FPValueID') {
                                //    self.manualSortHeaders.fpValueIDHeaderClass = "fa fa-chevron-down";
                                //} else if (column === 'FieldResults[0].IntValue') {
                                //    self.manualSortHeaders.totalScoreHeaderClass = "fa fa-chevron-down";
                                //} else if (column === 'TestDate') {
                                //    self.manualSortHeaders.tddHeaderClass = "fa fa-chevron-down";
                                //} else if (column === 'DelimitedTeachers') {
                                //    self.manualSortHeaders.teachersHeaderClass = "fa fa-chevron-down";
                                //} else if (column === 'GuidedReadingGroupId') {
                                //    self.manualSortHeaders.guidedReadingGroupClass = "fa fa-chevron-down";
                                //}
                                self.sortArray[j] = "-" + self.sortArray[j];
                            }
                            break;
                        }
                    }
                    if (!bFound) {
                        self.sortArray.push(column);

                        if (columnIndex > -1) {
                            self.headerClassArray[columnIndex] = "fa fa-chevron-up";
                        } else {
                            self.manualSortHeaders[column] = "fa fa-chevron-up";
                        }

                        //} else if (column === 'FirstName') {
                        //    self.manualSortHeaders.firstNameHeaderClass = "fa fa-chevron-up";
                        //} else if (column === 'LastName') {
                        //    self.manualSortHeaders.lastNameHeaderClass = "fa fa-chevron-up";
                        //} else if (column === 'StudentName') {
                        //    self.manualSortHeaders.studentNameHeaderClass = "fa fa-chevron-up";
                        //} else if (column === 'SchoolName') {
                        //    self.manualSortHeaders.schoolNameHeaderClass = "fa fa-chevron-up";
                        //} else if (column === 'GradeOrder') {
                        //    self.manualSortHeaders.gradeNameHeaderClass = "fa fa-chevron-up";
                        //} else if (column === 'DelimitedTeacherSections') {
                        //    self.manualSortHeaders.teacherNameHeaderClass = "fa fa-chevron-up";
                        //} else if (column === 'FPValueID') {
                        //    self.manualSortHeaders.fpValueIDHeaderClass = "fa fa-chevron-up";
                        //} else if (column === 'FieldResults[0].IntValue') {
                        //    self.manualSortHeaders.totalScoreHeaderClass = "fa fa-chevron-up";
                        //} else if (column === 'TestDate') {
                        //    self.manualSortHeaders.tddHeaderClass = "fa fa-chevron-up";
                        //} else if (column === 'DelimitedTeachers') {
                        //    self.manualSortHeaders.teachersHeaderClass = "fa fa-chevron-up";
                        //} else if (column === 'GuidedReadingGroupId') {
                        //    self.manualSortHeaders.guidedReadingGroupClass = "fa fa-chevron-up";
                        //}
                    }
                };
            };



            return NSSortManager;
        }])

    ;