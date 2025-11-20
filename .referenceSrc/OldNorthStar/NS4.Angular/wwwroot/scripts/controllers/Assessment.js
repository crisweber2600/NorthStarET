(function () {
	'use strict';

	angular
        .module('assessmentModule', [])
		.controller('AssessmentPreviewController', AssessmentPreviewController)
        .controller('AssessmentListController', AssessmentListController)
        .controller('AssessmentSyndicationController', AssessmentSyndicationController)
        .controller('AssessmentAddController', AssessmentAddController)
        .controller('AssessmentEditController', AssessmentEditController)
		.controller('AssessmentDefaultEntryController', AssessmentDefaultEntryController)
        .controller('AssessmentDeleteController', AssessmentDeleteController);

	AssessmentListController.$inject = ['$scope', '$http', 'Assessment', '$location', 'nsPinesService', '$bootbox', 'webApiBaseUrl'];

	function AssessmentListController($scope, $http, Assessment, $location, nsPinesService, $bootbox, webApiBaseUrl) {
		//$scope.assessments = Assessment.query();


	    var LoadData = function () {
	        $http.get(webApiBaseUrl + "/api/assessmentavailability/GetAssessmentList").then(function (response) {
	            $scope.assessments = response.data.Assessments;
	        });
	    }

	    LoadData();

		$scope.copyAsInterventionTest = function (assessmentId) {
		    $http.post(webApiBaseUrl + "/api/assessment/copyasinterventiontest", { Id: assessmentId }).then(function() {
		        nsPinesService.dataSavedSuccessfully();
		    }) 
		};

		$scope.simpleCopy = function (assessmentId) {
		    $http.post(webApiBaseUrl + "/api/assessment/simplecopy", { Id: assessmentId }).then(function () {
		        nsPinesService.dataSavedSuccessfully();
		    })
		}

		$scope.remove = function (assessmentId) {
            $bootbox.confirm("Are you sure you want to delete this assessment?", function(result) {
                if(result === true){
                    $http.post(webApiBaseUrl + "/api/assessment/delete", { Id: assessmentId }).then(function () {
                        nsPinesService.dataDeletedSuccessfully();
                        LoadData();
			        });
                }   
            });
        }  


		$scope.dataSavedSuccessfully = function () {
			pinesNotifications.notify({
				title: 'Data Saved',
				text: 'The assessment was deleted successfully.',
				type: 'success'
			});
		};

		$scope.dataError = function () {
			pinesNotifications.notify({
				title: 'Data Not Saved',
				text: 'The assessment was not successfully.  It is likely in use.',
				type: 'error'
			});
		};
	}

	AssessmentSyndicationController.$inject = ['$scope', '$http', 'Assessment', '$location', 'nsPinesService', '$bootbox', 'webApiBaseUrl','$timeout','$uibModal'];

	function AssessmentSyndicationController($scope, $http, Assessment, $location, nsPinesService, $bootbox, webApiBaseUrl, $timeout, $uibModal) {
	    //$scope.assessments = Assessment.query();

	    $scope.openDistrictSelectionDialog = function (assessment) {
	        // set settings
	        //$scope.copySettings.copyAll = copyAll;
	        $scope.selectedAssessment = assessment;

	        var modalInstance = $uibModal.open({
	            templateUrl: 'targetDistrictChooser.html',
	            scope: $scope,
	            controller: function ($scope, $uibModalInstance) {

	                var GetDistricts = function () {
	                    $http.get(webApiBaseUrl + "/api/Assessment/GetDistrictList").then(function (response) {
	                        $scope.allDistricts = response.data.Districts;
	                    });
	                }

	                GetDistricts();

	                $scope.copyAssessment = function (district) {

	                    $bootbox.confirm("Are you sure you want to copy this assessment?", function (result) {
	                        if (result === true) {

	                            district.status = 'label-primary';
	                            district.message = 'In progress...';
	                            $http.post(webApiBaseUrl + '/api/Assessment/CopyAssessmentToNewDistrict', { AssessmentId: $scope.selectedAssessment.Id, DistrictId: district.Id }).then(function (response) {
	                                if (response.data.isValid) {
	                                    district.message = "Copy complete";
	                                    district.status = 'label-success';
	                                } else {
	                                    district.status = 'label-danger';
	                                    district.message = 'Error while copying';
	                                }
	                            });
	                        }
	                    });
	                };
	                $scope.cancel = function () {
	                    $uibModalInstance.dismiss('cancel');
	                };
	            },
	            size: 'md',
	        });
	    }

	    var LoadData = function () {
	        $http.get(webApiBaseUrl + "/api/assessmentavailability/GetAssessmentList").then(function (response) {
	            $scope.assessments = response.data.Assessments;
	        });
	    }

	    LoadData();


	    $scope.dataSavedSuccessfully = function () {
	        pinesNotifications.notify({
	            title: 'Data Saved',
	            text: 'The assessment was deleted successfully.',
	            type: 'success'
	        });
	    };

	    $scope.dataError = function () {
	        pinesNotifications.notify({
	            title: 'Data Not Saved',
	            text: 'The assessment was not successfully.  It is likely in use.',
	            type: 'error'
	        });
	    };
	}

	/* Assessment Preview Controller */
	AssessmentPreviewController.$inject = ['$scope', '$location', 'Assessment'];

	function AssessmentPreviewController($scope, $location, Assessment) {

	}

	/* Movies Create Controller */
	AssessmentAddController.$inject = ['$scope', '$location', 'Assessment'];

	function AssessmentAddController($scope, $location, Assessment) {
		$scope.assessment = new Assessment();
		$scope.add = function () {
			$scope.assessment.$save(
                // success
                function () {
                	$location.path('/');
                },
                // error
                function (error) {
                	_showValidationErrors($scope, error);
                }
            );
		};

	}

	/* Movies Edit Controller */
	AssessmentEditController.$inject = ['$scope', '$routeParams', '$location', 'Assessment', 'formService', '$http', 'webApiBaseUrl', '$uibModal','$bootbox'];

	function AssessmentEditController($scope, $routeParams, $location, Assessment, formService, $http, webApiBaseUrl, $uibModal, $bootbox) {
		
		var assessmentId = $routeParams.id;
		if ($routeParams.id === undefined) {
			assessmentId = -1;
		}

		$scope.openGroupRepeater = function () {
		    var modalInstance = $uibModal.open({
		        templateUrl: 'groupRepeater.html',
		        scope: $scope,
		        controller: function ($scope, $uibModalInstance) {

		            $scope.processCopy = function (job) {
                                //get all of the fields from the currently selected group  
                                  var currentFields = [];
                                  var previousLabel = '';
                                  for (var i = 0; i < $scope.assessment.Fields.length; i++) {
                                      if ($scope.assessment.Fields[i].GroupId == $scope.assessment.SourceGroup) {
                                          if ($scope.assessment.Fields[i].FieldType == 'Label' && previousLabel == '') { // only set it once
                                              previousLabel = $scope.assessment.Fields[i].DisplayLabel;
                                          }
                                          currentFields.push($scope.assessment.Fields[i]);
                                      }
                                  }
                                  
                                  // now we have all of the fields that we want to duplicate, get started with the labels
                                  var aryLabels = [];
                                  var currentLabel = $scope.assessment.NewLabel;


                                    for (var j = 0; j < currentFields.length; j++) {
                                        var currentField = currentFields[j];

                                        if ($scope.assessment.Fields != null) {
                                            for (var i = 0; i < $scope.assessment.Fields.length; i++) {
                                                if ($scope.assessment.Fields[i].Id >= $scope.addField.lastAddedID) {
                                                    $scope.addField.lastAddedID = $scope.assessment.Fields[i].Id;
                                                    $scope.addField.lastAddedID++;
                                                }
                                            }
                                        } else {
                                            $scope.assessment.Fields = [];
                                        }

                                        // TODO: make sure the names of the fields are correct and the dbcolumn replace is correect
                                        var newField = {
                                            "Id": $scope.addField.lastAddedID,
                                            "DisplayLabel": currentField.DisplayLabel,
                                            "FieldType" : currentField.FieldType,
                                            "FieldOrder": currentField.FieldOrder,
                                            "GroupId": $scope.assessment.TargetGroup,
                                            "CategoryId": currentField.CategoryId,
                                            "SubcategoryId": currentField.SubcategoryId,
                                            "DatabaseColumn": currentField.DatabaseColumn ? currentField.DatabaseColumn.replace(previousLabel, currentLabel) : null,
                                            "AssessmentId": $scope.assessment.Id,
                                            "ObsSummaryLabel": currentField.ObsSummaryLabel,
                                            "DisplayInEditResultList": currentField.DisplayInEditResultList,
                                            "DisplayInLineGraphSummaryTable": currentField.DisplayInLineGraphSummaryTable,
                                            "DisplayInLineGraphs": currentField.DisplayInLineGraphs,
                                            "DisplayInObsSummary": currentField.DisplayInObsSummary,
                                            "DisplayInStackedBarGraphSummary": currentField.DisplayInStackedBarGraphSummary,
                                            "RangeHigh": currentField.RangeHigh,
                                            "RangeLow": currentField.RangeLow,
                                            "Page": currentField.Page,
                                            "UniqueImportColumnName": currentField.UniqueImportColumnName
                                        };

                                        // change the label for fields with the new label type
                                        if (newField.FieldType == 'Label') {
                                            newField.DisplayLabel = currentLabel;
                                        }

                                        // put newField into fields array
                                        $scope.assessment.Fields.push(newField);
                                    } 

		            }

		            $scope.cancel = function () {
		                $uibModalInstance.dismiss('cancel');
		            };
		        },
		        size: 'md',
		    });
		}
         
		$scope.openHRCreator = function () {
		    var modalInstance = $uibModal.open({
		        templateUrl: 'hrsiwSentence.html',
		        scope: $scope,
		        controller: function ($scope, $uibModalInstance) {

		            $scope.processHRSentence = function (job) {
		                //get all of the fields from the currently selected group  
		                var currentFields = [];

                        // string format:  letter | SPACE,
		                var fieldList = $scope.assessment.Sentence.split(",");
		                var fieldOrder = 1;
		                var page = $scope.assessment.newHRISPage;

		                for (var j = 0; j < fieldList.length; j++) {
		                    var currentField = fieldList[j];
		                    var isLabel = currentField == 'SPACE' ? true : false;

		                    if ($scope.assessment.Fields != null) {
		                        for (var i = 0; i < $scope.assessment.Fields.length; i++) {
		                            if ($scope.assessment.Fields[i].Id >= $scope.addField.lastAddedID) {
		                                $scope.addField.lastAddedID = $scope.assessment.Fields[i].Id;
		                                $scope.addField.lastAddedID++;
		                            }
		                        }
		                    } else {
		                        $scope.assessment.Fields = [];
		                    }

		                    // TODO: make sure the names of the fields are correct and the dbcolumn replace is correect
		                    var newField = {
		                        "Id": $scope.addField.lastAddedID,
		                        "DisplayLabel": isLabel ? '' : currentField,
		                        "FieldType": isLabel ? 'Label' : 'Checkbox',
		                        "FieldOrder": fieldOrder,
		                        "GroupId": $scope.assessment.HRSIWGroup,
		                        "CategoryId": null,
		                        "SubcategoryId": null,
		                        "DatabaseColumn": isLabel ? null : 'column_' + ($scope.assessment.Fields.length + 1),
		                        "AssessmentId": $scope.assessment.Id,
		                        "ObsSummaryLabel": isLabel ? '' : currentField,
		                        "DisplayInEditResultList": false,
		                        "DisplayInLineGraphSummaryTable": currentField.DisplayInLineGraphSummaryTable,
		                        "DisplayInLineGraphs": currentField.DisplayInLineGraphs,
		                        "DisplayInObsSummary": currentField.DisplayInObsSummary,
		                        "DisplayInStackedBarGraphSummary": isLabel ? false : true,
		                        "RangeHigh": 0,
		                        "RangeLow": 0,
		                        "Page": page,
		                        "UniqueImportColumnName": isLabel ? null : 'hrsiw2_' + 'column_' + ($scope.assessment.Fields.length + 1),
		                    };

		                  
		                    // put newField into fields array
		                    $scope.assessment.Fields.push(newField);
		                    fieldOrder++;
		                }

		            }

		            $scope.cancel = function () {
		                $uibModalInstance.dismiss('cancel');
		            };
		        },
		        size: 'md',
		    });
		}

		$scope.loadFields = function () {

		    var groupId = $scope.assessment.SelectedGroup === null ? 0 : $scope.assessment.SelectedGroup;
		    var subCatId = $scope.assessment.SelectedSubcategory === null ? 0 : $scope.assessment.SelectedSubcategory;
		    var categoryId = $scope.assessment.SelectedCategory === null ? 0 : $scope.assessment.SelectedCategory;
		    var dbTable = $scope.assessment.SelectedDBTable ? $scope.assessment.SelectedDBTable : "primary";

		    var paramObj = { groupId: groupId, subCategoryId: subCatId, categoryId: categoryId, page: $scope.assessment.SelectedPage, assessmentId: $routeParams.id, dbTable: dbTable };

		    $http.post(webApiBaseUrl + "/api/assessment/GetFieldsForAssessment", paramObj).then(function (response) {
		        angular.copy(response.data.Fields, $scope.assessment.Fields);
		    });
		}

		$scope.loadGroups = function () {

		    var startOrder = $scope.assessment.StartSortOrder === null ? 1 : $scope.assessment.StartSortOrder;
		    var endOrder = $scope.assessment.EndSortOrder === null ? 100 : $scope.assessment.EndSortOrder;

		    var paramObj = { assessmentId: $routeParams.id, startOrder: startOrder, endOrder: endOrder };

		    $http.post(webApiBaseUrl + "/api/assessment/GetGroupsForAssessment", paramObj).then(function (response) {
		        angular.copy(response.data.Groups, $scope.assessment.FieldGroups);
		    });
		}

		Assessment.get({ id: assessmentId }).$promise.then(function (data) {
		    $scope.assessment = data;
		    $scope.assessment.TestType = $scope.assessment.TestType + '';
		    $scope.assessment.BaseType = $scope.assessment.BaseType + '';
		    // TODO: this is temporary until we get DTOs built to do this automatically
		    $scope.assessment.SelectedCategory = 0;
		    $scope.assessment.SelectedGroup = 0;
		    $scope.assessment.SelectedPage = 0;
		    $scope.assessment.SelectedSubcategory = 0;
		    $scope.assessment.Fields = [];
		    $scope.assessment.FieldGroups = [];
		    $scope.assessment.StartSortOrder = 1;
		    $scope.assessment.EndSortOrder = 100;
		    //$scope.assessment.FieldCategories = [];
		    //$scope.assessment.FieldSubCategories = [];

		});



		$scope.FieldTypes = formService.fields;

		$scope.edit = function () {
			$scope.assessment.$save(
                // success
                function () {
                	$location.path('assessment-list');
                },
                // error
                function (error) {
                	_showValidationErrors($scope, error);
                }
            );
		};

		// Start Field Functions
		$scope.addField = {};
		$scope.addField.lastAddedID = 500000;
		$scope.deleteField = function (id) {
			for (var i = 0; i < $scope.assessment.Fields.length; i++) {
				if ($scope.assessment.Fields[i].Id == id) {
				    //$scope.assessment.Fields.splice(i, 1);
				    $scope.assessment.Fields[i].IsFlaggedForDelete = true;
					break;
				}
			}
		}
		$scope.addNewField = function () {
			// new approach... cycle through all fields and get the greatest ID and add one
			if ($scope.assessment.Fields != null) {
				for (var i = 0; i < $scope.assessment.Fields.length; i++) {
					if ($scope.assessment.Fields[i].Id >= $scope.addField.lastAddedID) {
						$scope.addField.lastAddedID = $scope.assessment.Fields[i].Id;
						$scope.addField.lastAddedID++;
					}
				}
			} else {
				$scope.assessment.Fields = [];
			}

			var newField = {
				"Id": $scope.addField.lastAddedID,
				"DisplayLabel": "Field - " + ($scope.addField.lastAddedID),
				"SortOrder": 1,
				"AssessmentId": $scope.assessment.Id
			};

			// put newField into fields array
			$scope.assessment.Fields.push(newField);
		}
		// End Field Functions

		// Start Group Functions
		$scope.addGroup = {};
		$scope.addGroup.lastAddedID = 500000;
		$scope.deleteGroup = function (id) {
			for (var i = 0; i < $scope.assessment.FieldGroups.length; i++) {
				if ($scope.assessment.FieldGroups[i].Id == id) {
                    $scope.assessment.FieldGroups[i].IsFlaggedForDelete = true;
					//$scope.assessment.FieldGroups.splice(i, 1);
					break;
				}
			}
		}
		$scope.addGroupContainer = {};
		$scope.addGroupContainer.lastAddedID = 500000;
		$scope.deleteGroupContainer = function (id) {
		    for (var i = 0; i < $scope.assessment.FieldGroupContainers.length; i++) {
		        if ($scope.assessment.FieldGroupContainers[i].Id == id) {
		            $scope.assessment.FieldGroupContainers[i].IsFlaggedForDelete = true;
		            break;
		        }
		    }
		}
        $scope.getSubGroup = function(id) {
            var groupName = '';
            
            if(id)
            {
                for (var i = 0; i < $scope.assessment.FieldGroups.length; i++) {
				    if ($scope.assessment.FieldGroups[i].Id == id) {
					    groupName = ' / Grp:' + $scope.assessment.FieldGroups[i].DisplayName;
					    break;
				    }
			    }
            }
            return groupName;
        };
		$scope.addNewGroup = function () {
			// new approach... cycle through all fields and get the greatest ID and add one
			if ($scope.assessment.FieldGroups != null) {
				for (var i = 0; i < $scope.assessment.FieldGroups.length; i++) {
					if ($scope.assessment.FieldGroups[i].Id >= $scope.addGroup.lastAddedID) {
						$scope.addGroup.lastAddedID = $scope.assessment.FieldGroups[i].Id;
						$scope.addGroup.lastAddedID++;
					}
				}
			} else {
				$scope.assessment.FieldGroups = [];
			}

			var newGroup = {
				"Id": $scope.addGroup.lastAddedID,
				"DisplayName": "Group - " + ($scope.addGroup.lastAddedID),
				"SortOrder": 1,
				"AssessmentId": $scope.assessment.Id
			};

			// put newField into fields array
			$scope.assessment.FieldGroups.push(newGroup);
		}
		$scope.addNewGroupContainer = function () {
		    // new approach... cycle through all fields and get the greatest ID and add one
		    if ($scope.assessment.FieldGroupContainers != null) {
		        for (var i = 0; i < $scope.assessment.FieldGroupContainers.length; i++) {
		            if ($scope.assessment.FieldGroupContainers[i].Id >= $scope.addGroupContainer.lastAddedID) {
		                $scope.addGroupContainer.lastAddedID = $scope.assessment.FieldGroupContainers[i].Id;
		                $scope.addGroupContainer.lastAddedID++;
		            }
		        }
		    } else {
		        $scope.assessment.FieldGroupContainers = [];
		    }

		    var newGroup = {
		        "Id": $scope.addGroupContainer.lastAddedID,
		        "DisplayName": "Group Container - " + ($scope.addGroupContainer.lastAddedID),
		        "SortOrder": 1,
		        "AssessmentId": $scope.assessment.Id
		    };

		    // put newField into fields array
		    $scope.assessment.FieldGroupContainers.push(newGroup);
		}
		// End Group Functions

        // Start Category Functions
		$scope.addCategory = {};
		$scope.addCategory.lastAddedID = 500000;
		$scope.deleteCategory = function (id) {
			for (var i = 0; i < $scope.assessment.FieldCategories.length; i++) {
				if ($scope.assessment.FieldCategories[i].Id == id) {
					$scope.assessment.FieldCategories.splice(i, 1);
					break;
				}
			}
		}
        $scope.getCategory = function(id) {
            var groupName = '';
            
            if(id)
            {
                for (var i = 0; i < $scope.assessment.FieldCategories.length; i++) {
				    if ($scope.assessment.FieldCategories[i].Id == id) {
					    groupName = ' / Cat:' + $scope.assessment.FieldCategories[i].DisplayName;
					    break;
				    }
			    }
            }
            return groupName;
        };
		$scope.addNewCategory = function () {
			// new approach... cycle through all fields and get the greatest ID and add one
			if ($scope.assessment.FieldCategories != null) {
				for (var i = 0; i < $scope.assessment.FieldCategories.length; i++) {
					if ($scope.assessment.FieldCategories[i].Id >= $scope.addCategory.lastAddedID) {
						$scope.addCategory.lastAddedID = $scope.assessment.FieldCategories[i].Id;
						$scope.addCategory.lastAddedID++;
					}
				}
			} else {
				$scope.assessment.FieldCategories = [];
			}

			var newCategory = {
				"Id": $scope.addCategory.lastAddedID,
				"DisplayName": "Category - " + ($scope.addCategory.lastAddedID),
				"SortOrder": 1,
				"AssessmentId": $scope.assessment.Id
			};

			// put newField into fields array
			$scope.assessment.FieldCategories.push(newCategory);
		}
		// End Category Functions

        // Start Sub-Category Functions
		$scope.addSubCategory = {};
		$scope.addSubCategory.lastAddedID = 500000;
		$scope.deleteSubCategory = function (id) {
			for (var i = 0; i < $scope.assessment.FieldSubCategories.length; i++) {
				if ($scope.assessment.FieldSubCategories[i].Id == id) {
					$scope.assessment.FieldSubCategories.splice(i, 1);
					break;
				}
			}
		}
        $scope.getSubCategory = function(id) {
            var groupName = '';
            
            if(id)
            {
                for (var i = 0; i < $scope.assessment.FieldSubCategories.length; i++) {
				    if ($scope.assessment.FieldSubCategories[i].Id == id) {
					    groupName = ' / SC:' + $scope.assessment.FieldSubCategories[i].DisplayName;
					    break;
				    }
			    }
            }
            return groupName;
        };
		$scope.addNewSubCategory = function () {
			// new approach... cycle through all fields and get the greatest ID and add one
			if ($scope.assessment.FieldSubCategories != null) {
				for (var i = 0; i < $scope.assessment.FieldSubCategories.length; i++) {
					if ($scope.assessment.FieldSubCategories[i].Id >= $scope.addSubCategory.lastAddedID) {
					    $scope.addSubCategory.lastAddedID = $scope.assessment.FieldSubCategories[i].Id;
						$scope.addSubCategory.lastAddedID++;
					}
				}
			} else {
				$scope.assessment.FieldSubCategories = [];
			}

			var newSubCategory = {
				"Id": $scope.addSubCategory.lastAddedID,
				"DisplayName": "Sub-Category - " + ($scope.addSubCategory.lastAddedID),
				"SortOrder": 1,
				"AssessmentId": $scope.assessment.Id
			};

			// put newField into fields array
			$scope.assessment.FieldSubCategories.push(newSubCategory);
		}
		// End Sub-Category Functions
	}

	AssessmentDefaultEntryController.$inject = ['$scope', '$routeParams', '$location', 'Assessment', 'formService', '$http', 'pinesNotifications', 'webApiBaseUrl'];

	function AssessmentDefaultEntryController($scope, $routeParams, $location, Assessment, formService, $http, pinesNotifications, webApiBaseUrl) {
		// get lookup field values
	    $http.get(webApiBaseUrl + '/api/assessment/GetLookupFieldsForAssessment/' + $routeParams.assessmentId).success(function (lookupData) {

			$scope.lookupFieldsArray = lookupData;
			$scope.sortArray = [];
			$scope.headerClassArray = [];
			$scope.firstNameHeaderClass = "fa";
			$scope.lastNameHeaderClass = "fa";
		
			$scope.sort = function (column) {

				var columnIndex = -1;
				// if this is not a first or lastname column
				if (!isNaN(parseInt(column))) {
					columnIndex = column;
					switch ($scope.fields[column].FieldType) {
						case 'Textfield':
							column = 'FieldResults[' + column + '].StringValue';
							break;
                        case 'DateCheckbox':
							column = 'FieldResults[' + column + '].DateValue';
							break;
						case 'DecimalRange':
							column = 'FieldResults[' + column + '].DecimalValue';
							break;
						case 'DropdownRange':
							column = 'FieldResults[' + column + '].IntValue';
							break;
					    case 'checklist':
					        column = 'FieldResults[' + column + '].StringValue';
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
				for (var j = 0; j < $scope.sortArray.length; j++) {
					// if it is already on the list, reverse the sort
					if ($scope.sortArray[j].indexOf(column) >= 0) {
						bFound = true;

						// is it already negative? if so, remove it
						if ($scope.sortArray[j].indexOf("-") === 0) {
							if (columnIndex > -1) {
								$scope.headerClassArray[columnIndex] = "fa";
							}
							else if (column === 'FirstName') {
								$scope.firstNameHeaderClass = "fa";
							}
							else if (column === 'LastName') {
								$scope.lastNameHeaderClass = "fa";
							}
							$scope.sortArray.splice(j, 1);
						} else {
							if (columnIndex > -1) {
								$scope.headerClassArray[columnIndex] = "fa fa-chevron-down";
							}
							else if (column === 'FirstName') {
								$scope.firstNameHeaderClass = "fa fa-chevron-down";
							}
							else if (column === 'LastName') {
								$scope.lastNameHeaderClass = "fa fa-chevron-down";
							}
							$scope.sortArray[j] = "-" + $scope.sortArray[j];
						}
						break;
					}
				}
				if (!bFound) {
					$scope.sortArray.push(column);

					if (columnIndex > -1) {
						$scope.headerClassArray[columnIndex] = "fa fa-chevron-up";
					}
					else if (column === 'FirstName') {
						$scope.firstNameHeaderClass = "fa fa-chevron-up";
					}
					else if (column === 'LastName') {
						$scope.lastNameHeaderClass = "fa fa-chevron-up";
					}
				}

				//if ($scope.orderProp === column) {
				//	$scope.direction = !$scope.direction;
				//} else {
				//	$scope.orderProp = column;
				//	$scope.direction = true;
				//}
			};

			// strip unnecessary stuff off of studentResult to increase speed
			$scope.deleteAssessmentData = function(studentResult) {
				var assessmentId = $scope.assessment.Id;
				//var studentResult = {};
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
				}


				$http.post(webApiBaseUrl + "/api/assessment/DeleteAssessmentResult", returnObject).success(function (data) {
					$scope.dataDeletedSuccessfully();

				}).error(function(data, status, headers, config) {
					alert('error deleting');
				});

			};

			$scope.saveAssessmentData = function (studentResult) {
				var assessmentId = $scope.assessment.Id;
				//var studentResult = {};
				var returnObject = {
					StudentResult: studentResult,
					AssessmentId: assessmentId
				}
				$http.post(webApiBaseUrl + "/api/assessment/SaveAssessmentResult", returnObject).success(function (data) {

					// set values for the lookup fileds
					for (var k = 0; k < studentResult.FieldResults.length; k++) {
						for (var i = 0; i < $scope.fields.length; i++) {
							if ($scope.fields[i].DatabaseColumn == studentResult.FieldResults[k].DbColumn) {
								// set display value
								if ($scope.fields[i].FieldType === "DropdownFromDB") {
									for (var p = 0; p < $scope.lookupFieldsArray.length; p++) {
										if ($scope.lookupFieldsArray[p].LookupColumnName === $scope.fields[i].LookupFieldName) {
											// now find the specifc value that matches
											for (var y = 0; y < $scope.lookupFieldsArray[p].LookupFields.length; y++) {
												if (studentResult.FieldResults[k].IntValue === $scope.lookupFieldsArray[p].LookupFields[y].FieldSpecificId) { 
													studentResult.FieldResults[k].DisplayValue = $scope.lookupFieldsArray[p].LookupFields[y].FieldValue;
												}
											}
										}
									}
								}
								// set the values passed back from the save method to the calculated fields
								else if ($scope.fields[i].FieldType === "CalculatedFieldDbOnly" || $scope.fields[i].FieldType === "CalculatedFieldDbBacked" || $scope.fields[i].FieldType === "CalculatedFieldDbBackedString") {
									for (var p = 0; p < data.length; p++) {
										if (data[p].DbColumn === studentResult.FieldResults[k].DbColumn) {
											studentResult.FieldResults[k].IntValue = data[p].IntValue;
											studentResult.FieldResults[k].StringValue = data[p].StringValue;
										}
									}
								}
							}
						}
					}


					$scope.dataSavedSuccessfully();

					// TODO: update diplayvalue of any dropdownfromdb fields

				}).error(function (data, status, headers, config) {
					alert('error saving');
				});

			};

			$scope.dataSavedSuccessfully = function () {
				pinesNotifications.notify({
					title: 'Data Saved',
					text: 'Your data was saved successfully.',
					type: 'success'
				});
			};

			$scope.dataDeletedSuccessfully = function () {
				pinesNotifications.notify({
					title: 'Data Deleted',
					text: 'Your data was deleted successfully.',
					type: 'success'
				});
			};

			$http.get(webApiBaseUrl + '/api/assessment/getassessmentresults/' + $routeParams.assessmentId + "/" + $routeParams.classId + "/" + $routeParams.benchmarkDateId).success(function (data) {
				$scope.assessment = data.Assessment;
				$scope.fields = data.Assessment.Fields;
				$scope.studentResults = data.StudentResults;

				for (var r = 0; r < $scope.fields.length; r++) {
					$scope.headerClassArray[r] = 'fa';
				}

				// Add FieldOrder, FieldType

				for (var j = 0; j < $scope.studentResults.length; j++) {
					for (var k = 0; k < $scope.studentResults[j].FieldResults.length; k++) {
						for (var i = 0; i < $scope.fields.length; i++) {
							if ($scope.fields[i].DatabaseColumn == $scope.studentResults[j].FieldResults[k].DbColumn) {
								$scope.studentResults[j].FieldResults[k].Field = $scope.fields[i];

								// set display value
								if ($scope.fields[i].FieldType === "DropdownFromDB") {
									for (var p = 0; p < $scope.lookupFieldsArray.length; p++) {
										if ($scope.lookupFieldsArray[p].LookupColumnName === $scope.fields[i].LookupFieldName) {
											// now find the specifc value that matches
											for (var y = 0; y < $scope.lookupFieldsArray[p].LookupFields.length; y++) {
												if ($scope.studentResults[j].FieldResults[k].IntValue === $scope.lookupFieldsArray[p].LookupFields[y].FieldSpecificId) { 
													$scope.studentResults[j].FieldResults[k].DisplayValue = $scope.lookupFieldsArray[p].LookupFields[y].FieldValue;
												}
											}
										}
									}
								}
							}
						}
					}
				}

				//alert(data);
			});
		});

	}

	/* Movies Delete Controller  */
	AssessmentDeleteController.$inject = ['$scope', '$routeParams', '$location', 'Assessment'];

	function AssessmentDeleteController($scope, $routeParams, $location, Assessment) {
		$scope.assessment = Assessment.get({ id: $routeParams.id });
		$scope.remove = function () {
			$scope.assessment.$remove({ id: $scope.sassessmenttaff.Id }, function () {
				$location.path('/');
			});
		};
	}


	/* Utility Functions */

	function _showValidationErrors($scope, error) {
		$scope.validationErrors = [];
		if (error.data && angular.isObject(error.data)) {
			for (var key in error.data) {
				$scope.validationErrors.push(error.data[key][0]);
			}
		} else {
			$scope.validationErrors.push('Could not add assessment.');
		};
	}

})();