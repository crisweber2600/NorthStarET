(function () {
    'use strict';

    angular
        .module('assessmentFieldsModule', [])
    .directive('nsAssessmentField', [
			'Assessment', '$routeParams', '$compile', '$templateCache', '$http', 'nsLookupFieldService', '$uibModal', '$global', function (Assessment, $routeParams, $compile, $templateCache, $http, nsLookupFieldService, $uibModal, $global) {

			    var getTemplate = function (field, mode) {
			        var type = field.FieldType;
			        var template = '';

			        if (type === '' || type === null) {
			            type = 'textfield';
			        }
			        var templateName = 'templates/assessment-' + type + '-' + mode + '.html';
			        template =  $templateCache.get(templateName.toLocaleLowerCase());
			        return template;
			    } 

			    return {
			        restrict: 'E',
			        //scope: {
			        //    result: '@',
			        //    allResults: '@',
			        //    eForm: '=',
			        //    mode: '@',
                    //    fieldRequired: '='
			        //},
                    scope: true,
			        link: function (scope, element, attr) {

			            scope.lookupValues = [];
			            scope.lookupFieldsArray = [];
			            scope.lookupFieldsArray = nsLookupFieldService.LookupFieldsArray;

			            var removeit = scope.$watch(function () {
			                if (attr.result && attr.allResults) {
			                    return 'loaded';
			                } else {
			                    return '';
			                }
			            }, function (newVal) {
			                if (newVal == 'loaded') {
			                    scope.ckeditorOptions = angular.isDefined(attr.ckeditorOptions) ? scope.$eval(attr.ckeditorOptions) : { language: 'en', allowedContent: true, entities: false };

			                    scope.mode = angular.isDefined(attr.mode) ? scope.$eval(attr.mode) : 'readonly';
			                    scope.result = scope.$eval(attr.result);
			                    scope.allResults = scope.$eval(attr.allResults);
			                    scope.displayTextAreas = angular.isDefined(attr.displayTextAreas) ? scope.$eval(attr.displayTextAreas) : false;

			                    // check for global textarea setting
			                    if (angular.isDefined($global.get('turnOffCommentBubbles'))) {
			                        scope.displayTextAreas = $global.get('turnOffCommentBubbles');
			                    }

			                    scope.eForm = angular.isDefined(attr.eForm) ? scope.$eval(attr.eForm) : null;
			                    scope.fieldRequired = angular.isDefined(attr.fieldRequired) ? scope.$eval(attr.fieldRequired) : false;

			                    for (var i = 0; i < scope.lookupFieldsArray.length; i++) {
			                        if (scope.result.Field) {
			                            if (scope.lookupFieldsArray[i].LookupColumnName === scope.result.Field.LookupFieldName) {
			                                scope.lookupValues = scope.lookupFieldsArray[i].LookupFields;

			                                if (scope.result.Field.FieldType == 'checklist') {
			                                    var aryResult = [];

			                                    // now find the specifc value that matches
			                                    for (var j = 0; j < scope.lookupFieldsArray[i].LookupFields.length; j++) {
			                                        if (scope.result.StringValue) {
			                                            var aryEachId = scope.result.StringValue.split(',');

			                                            for (var z = 0; z < aryEachId.length; z++) {
			                                                if (aryEachId[z] == scope.lookupFieldsArray[i].LookupFields[j].FieldSpecificId) {
			                                                    aryResult.push(scope.lookupFieldsArray[i].LookupFields[j].FieldValue);
			                                                }
			                                            }
			                                        }
			                                    }
			                                    scope.result.DisplayValue = aryResult.join(', ');
			                                } else {
			                                    for (var j = 0; j < scope.lookupFieldsArray[i].LookupFields.length; j++) {
			                                        if (scope.lookupFieldsArray[i].LookupFields[j].FieldSpecificId == scope.result.IntValue) {
			                                            scope.result.DisplayValue = scope.lookupFieldsArray[i].LookupFields[j].FieldValue;
			                                            break;
			                                        }
			                                    }
			                                }
			                            }
			                        }
			                    }

			                    var templateText = getTemplate(scope.result.Field, scope.mode);
			                    scope.calcFunction = getCalculationFunction(scope.result.Field);
			                    var dataToAppend = templateText;
			                    //element.html("<span>" + scope.result.DisplayValue + "</span>");
			                    element.html(dataToAppend);
			                    $compile(element.contents())(scope);
			                    removeit();
			                }
			            }, true);



			            scope.toolTipFunction = function () {
			                if (scope.result.StringValue) {
			                    return scope.result.StringValue;
			                }
			                else { return ''; }
			            }

			            scope.commentsModal = function () {
			                var modalInstance = $uibModal.open({
			                    templateUrl: 'commentsViewer.html',
			                    scope: scope,
			                    controller: function ($scope, $uibModalInstance) {
			                        $scope.cancel = function () {
			                            $uibModalInstance.dismiss('cancel');
			                        };
			                    },
			                    size: 'md',
			                });
			            }

			            scope.validateIfRequired = function (val) {
			                if (angular.isDefined(scope.fieldRequired) && scope.fieldRequired === true && !val) {
			                    return 'This field is required.';
			                }
			            }

			            // get our own lookupFields

			            scope.guidedReadingCheck = function (fieldValue, DbColumn) {
			                if (DbColumn == 'guidedreadinggroup') {
			                    switch (fieldValue) {
			                        case 1:
			                            return 'grRed';
			                        case 2:
			                            return 'grOrange';
			                        case 3:
			                            return 'grYellow';
			                        case 4:
			                            return 'grGreen';
			                        case 5:
			                            return 'grBlue';
			                        case 6:
			                            return 'grPurple';
			                        default:
			                            return '';
			                    }
			                } else return '';
			            }
			           

			            var SumFunction = function (args) {
			                var aryFields = args.split(",");
			                var sum = 0;

			                for (var i = 0; i < aryFields.length; i++) {
			                    var fieldDbColumn = aryFields[i];

			                    for (var j = 0; j < scope.allResults.length; j++) {
			                        if (scope.allResults[j].DbColumn == fieldDbColumn) {
			                            sum += scope.allResults[j].IntValue;
			                        }
			                    }
			                }
			                scope.result.StringValue = sum;
			                return sum;
			            };

			            var BenchmarkFunction = function (args) {
			                return 3;
			                var FPValueId = 0;
			                var Accuracy = 0;
			                var CompScore = 0;

			                // calculate compscore
			                for (var j = 0; j < scope.allResults.length; j++) {
			                    if (scope.allResults[j].DbColumn === "Beyond" ||
									scope.allResults[j].DbColumn === "About" ||
									scope.allResults[j].DbColumn === "Within" ||
									scope.allResults[j].DbColumn === "ExtraPt") {
			                        CompScore += scope.allResults[j].IntValue;
			                    }
			                }

			                // get accuracy
			                for (var j = 0; j < scope.allResults.length; j++) {
			                    if (scope.allResults[j].DbColumn === "Accuracy") {
			                        Accuracy = scope.allResults[j].IntValue;
			                        break;
			                    }
			                }

			                // FPScore
			                for (var j = 0; j < scope.allResults.length; j++) {
			                    if (scope.allResults[j].DbColumn === "FPValueID") {
			                        FPValueId = scope.allResults[j].IntValue;
			                        break;
			                    }
			                }
			                //scope.result.StringValue = 'test';
			                if (Accuracy == null) {
			                    Accuracy = 0;
			                }
			                if (CompScore == null) {
			                    CompScore = 0;
			                }
			                if (FPValueId == null) {
			                    FPValueId = 0;
			                }
			                //return 'test';
			                // get benchmarklevel
			                $http.get('/api/assessment/getbenchmarklevel/' + FPValueId + '/' + Accuracy + '/' + CompScore).success(function (data) {
			                    return data;
			                })
							.error(function (data, status, headers, config) {
							    return 'error';
							});
			            }

			            var ConcatFunction = function (args) {
			                var aryFields = args.split(",");
			                var sum = '';

			                for (var i = 0; i < aryFields.length; i++) {
			                    var fieldDbColumn = aryFields[i];

			                    for (var j = 0; j < scope.allResults.length; j++) {
			                        if (scope.allResults[j].DbColumn == fieldDbColumn) {
			                            sum += scope.allResults[j].IntValue;
			                            if (i < aryFields.length - 1) {
			                                sum += '/';
			                            }
			                        }
			                    }
			                }
			                scope.result.StringValue = sum;
			                return sum;
			            }

			            function getCalculationFunction(field) {
			                var calcFunctionName = field.CalculationFunction;
			                var calcFunction;

			                switch (calcFunctionName) {
			                    case 'Sum':
			                        calcFunction = SumFunction;
			                        break;
			                    case 'BenchmarkLevel':
			                        calcFunction = BenchmarkFunction;
			                        break;
			                    case 'Concatenate':
			                        calcFunction = ConcatFunction;
			                        break;
			                    default:
			                        calcFunction = null;
			                        break;
			                }

			                return calcFunction;
			            }



			            scope.Calculate = function (args) {
			                return scope.calcFunction(args);
			            }
			        }
			    };
			}
    ])
        .directive('observationSummaryViewField', [
			'$routeParams', '$compile', '$templateCache', '$http', 'nsLookupFieldService', function ($routeParams, $compile, $templateCache, $http, nsLookupFieldService) {

			    var getTemplate = function (field) {
			        var type = field.ColumnType;
			        var template = '';

			        switch (type) {
			            case 'Textfield':
			                template = $templateCache.get('templates/assessment-textfield-readonly.html');
			                break;
			            case 'DecimalRange':
			                template = $templateCache.get('templates/assessment-decimalrange-readonly.html');
			                break;
			            case 'DropdownRange':
			                template = $templateCache.get('templates/assessment-dropdownrange-readonly.html');
			                break;
			            case 'DropdownFromDB':
			                template = $templateCache.get('templates/assessment-dropdownfromdb-readonly.html');
			                break;
			            case 'CalculatedFieldClientOnly':
			                template = $templateCache.get('templates/assessment-calculatedfield-readonly.html');
			                break;
			            case 'CalculatedFieldDbBacked':
			                template = $templateCache.get('templates/assessment-calculatedfielddbbacked-readonly.html');
			                break;
			            case 'CalculatedFieldDbBackedString':
			                template = $templateCache.get('templates/assessment-calculatedfielddbbackedstring-readonly.html');
			                break;
			            case 'CalculatedFieldDbOnly':
			                template = $templateCache.get('templates/assessment-calculatedfielddbonly-readonly.html');
			                break;
			            default:
			                template = $templateCache.get('templates/assessment-textfield-readonly.html');
			                break;
			        }

			        return template;
			    }

			    return {
			        restrict: 'E',
			        template: '<div>{{result}}</div>',
			        scope: {
			            result: '=',
			            allResults: '=',
			            lookupFieldsArray: '='
			        },
			        link: function (scope, element, attr) {

			            // get our own lookupFields
			            scope.lookupValues = [];
			            scope.lookupFieldsArray = nsLookupFieldService.LookupFieldsArray;
			            //for (var i = 0; i < scope.lookupFieldsArray.length; i++)
			            //    if (scope.lookupFieldsArray[i].LookupColumnName === scope.result.LookupFieldName) {
			            //        scope.lookupValues = scope.lookupFieldsArray[i].LookupFields;
			            //        break;
			            //    }

			            var SumFunction = function (args) {
			                var aryFields = args.split(",");
			                var sum = 0;

			                for (var i = 0; i < aryFields.length; i++) {
			                    var fieldDbColumn = aryFields[i];

			                    for (var j = 0; j < scope.allResults.length; j++) {
			                        if (scope.allResults[j].DbColumn == fieldDbColumn) {
			                            sum += scope.allResults[j].IntValue;
			                        }
			                    }
			                }
			                scope.result.StringValue = sum;
			                return sum;
			            };


			            var ConcatFunction = function (args) {
			                var aryFields = args.split(",");
			                var sum = 0;

			                for (var i = 0; i < aryFields.length; i++) {
			                    var fieldDbColumn = aryFields[i];

			                    for (var j = 0; j < scope.allResults.length; j++) {
			                        if (scope.allResults[j].DbColumn == fieldDbColumn) {
			                            sum += scope.allResults[j].IntValue + '/';
			                        }
			                    }
			                }
			                scope.result.StringValue = sum;
			                return sum;
			            }

			            function getCalculationFunction(field) {
			                var calcFunctionName = field.CalculationFunction;
			                var calcFunction;

			                switch (calcFunctionName) {
			                    case 'Sum':
			                        calcFunction = SumFunction;
			                        break;
			                    case 'Concatenate':
			                        calcFunction = ConcatFunction;
			                        break;
			                    default:
			                        calcFunction = null;
			                        break;
			                }

			                return calcFunction;
			            }

			            var templateText = getTemplate(scope.result);
			            var calcFunction = getCalculationFunction(scope.result);
			            var dataToAppend = templateText;
			            element.html(dataToAppend);
			            $compile(element.contents())(scope);





			            scope.Calculate = function (args) {
			                return calcFunction(args);
			            }
			        }
			    };
			}
        ])
        .directive('assessmentEditFieldCustom', [
			'Assessment', '$routeParams', '$compile', '$templateCache', '$http', 'nsLookupFieldService', function (Assessment, $routeParams, $compile, $templateCache, $http, nsLookupFieldService) {

			    var getTemplate = function (field, orientation) {
			        var type = field.FieldType;
			        var template = '';

			        if (type === '' || type === null) {
			            type = 'textfield';
			        }
			        var templateName = 'templates/assessment-' + type + orientation + '-custom.html';
			        template = $templateCache.get(templateName.toLocaleLowerCase());
			        return template;
			    }

			    return {
			        restrict: 'E',
			        template: '<div>{{result}}</div>',
			        scope: {
			            currentField: '=',
			            studentResult: '=',
			            groups: '=',
			            fields: '=',
			            orientation: '=',
			            disabled: '=',
			            fieldRequired: '=',
                        fieldName: '='
			        },
			        link: function (scope, element, attr) {

			            if (!angular.isDefined(scope.orientation)) {
			                scope.orientation = '';
			            }

			            // get our own lookupFields
			            scope.lookupValues = [];
			            scope.lookupFieldsArray = nsLookupFieldService.LookupFieldsArray;
			            scope.fieldResult = {};

			            // determine 'which' fieldResult is our currentFieldResult
			            // if field doesn't have a 'databasecolumn', then it is a label of some sort
			            // just use a standard display template
			            if (scope.currentField.DatabaseColumn != '' || scope.currentField.DatabaseColumn != null) {
			                for (var j = 0; j < scope.studentResult.FieldResults.length; j++) {
			                    if (scope.studentResult.FieldResults[j].DbColumn === scope.currentField.DatabaseColumn) {
			                        scope.fieldResult = scope.studentResult.FieldResults[j];
			                        break;
			                    }
			                }
			            }

			            for (var i = 0; i < scope.lookupFieldsArray.length; i++)
			                if (scope.lookupFieldsArray[i].LookupColumnName === scope.currentField.LookupFieldName) {
			                    scope.lookupValues = scope.lookupFieldsArray[i].LookupFields;
			                    break;
			                }

			            var SumFunction = function (args) {
			                var aryFields = args.split(",");
			                var sum = 0;

			                for (var i = 0; i < aryFields.length; i++) {
			                    var fieldDbColumn = aryFields[i];

			                    for (var j = 0; j < scope.studentResult.FieldResults.length; j++) {
			                        if (scope.studentResult.FieldResults[j].DbColumn == fieldDbColumn) {
			                            sum += scope.studentResult.FieldResults[j].IntValue;
			                        }
			                    }
			                }
			                //scope.result.StringValue = sum;
			                return sum;
			            };

			            var SumBoolFunction = function (args) {
			                var aryFields = args.split(",");
			                var sum = 0;

			                for (var i = 0; i < aryFields.length; i++) {
			                    var fieldDbColumn = aryFields[i];

			                    for (var j = 0; j < scope.studentResult.FieldResults.length; j++) {
			                        if (scope.studentResult.FieldResults[j].DbColumn == fieldDbColumn) {
			                            sum += (scope.studentResult.FieldResults[j].BoolValue ? 1 : 0);
			                        }
			                    }
			                }
			                //scope.result.StringValue = sum;
			                return sum;
			            };

			            var SumBoolByGroupFunction = function () {

			                var sum = 0;

			                for (var i = 0; i < scope.groups.length; i++) {
			                    var groupId = scope.groups[i].Id;

			                    // i don't like having this reference to the field... need to figure out
			                    // if it makes more sense to pass the additional data for each field
			                    // or to just join them on the client
			                    for (var j = 0; j < scope.studentResult.FieldResults.length; j++) {
			                        if (scope.studentResult.FieldResults[j].Field.GroupId === groupId && scope.studentResult.FieldResults[j].DbColumn.substring(0, 3) === 'chk') {
			                            // only add each groupid once

			                            if (scope.studentResult.FieldResults[j].BoolValue) {
			                                sum++;
			                                break;
			                            }
			                        }
			                    }
			                }
			                //scope.result.StringValue = sum;
			                return sum;
			            };

			            var ConcatenatedMissingLetters = function () {

			                var unknownLetters = '';

			                for (var i = 0; i < scope.groups.length; i++) {
			                    var groupId = scope.groups[i].Id;
			                    var foundInGroup = false;
			                    var groupDbCol = '';

			                    for (var j = 0; j < scope.studentResult.FieldResults.length; j++) {
			                        if (scope.studentResult.FieldResults[j].Field.GroupId === groupId && scope.studentResult.FieldResults[j].DbColumn.substring(0, 3) === 'chk') {
			                            if (scope.studentResult.FieldResults[j].BoolValue) {
			                                groupDbCol = scope.studentResult.FieldResults[j].Field.DatabaseColumn;
			                                foundInGroup = true;
			                                break;
			                            }
			                        }
			                    }
			                    if (!foundInGroup) {
			                        // how to get the letter? find the field with the same groupid and print its DisplayLabel
			                        for (var r = 0; r < scope.fields.length; r++) {
			                            var field = scope.fields[r];

			                            if (field.GroupId === groupId && field.FieldType === 'Label') {
			      			                unknownLetters += field.DisplayLabel + ', ';
			                            }
			                        }
			                    }
			                }
			                //remove trailing comma
			                if (unknownLetters.length > 0) {
			                    unknownLetters = unknownLetters.substring(0, unknownLetters.length - 1);
			                }
			                else {
			                    unknownLetters = 'none';
			                }
			                return unknownLetters;
			            };

			            var BenchmarkFunction = function (args) {
			                return 3;
			                var FPValueId = 0;
			                var Accuracy = 0;
			                var CompScore = 0;

			                // calculate compscore
			                for (var j = 0; j < scope.allResults.length; j++) {
			                    if (scope.allResults[j].DbColumn === "Beyond" ||
									scope.allResults[j].DbColumn === "About" ||
									scope.allResults[j].DbColumn === "Within" ||
									scope.allResults[j].DbColumn === "ExtraPt") {
			                        CompScore += scope.allResults[j].IntValue;
			                    }
			                }

			                // get accuracy
			                for (var j = 0; j < scope.allResults.length; j++) {
			                    if (scope.allResults[j].DbColumn === "Accuracy") {
			                        Accuracy = scope.allResults[j].IntValue;
			                        break;
			                    }
			                }

			                // FPScore
			                for (var j = 0; j < scope.allResults.length; j++) {
			                    if (scope.allResults[j].DbColumn === "FPValueID") {
			                        FPValueId = scope.allResults[j].IntValue;
			                        break;
			                    }
			                }
			                //scope.result.StringValue = 'test';
			                if (Accuracy == null) {
			                    Accuracy = 0;
			                }
			                if (CompScore == null) {
			                    CompScore = 0;
			                }
			                if (FPValueId == null) {
			                    FPValueId = 0;
			                }
			                //return 'test';
			                // get benchmarklevel
			                $http.get('/api/assessment/getbenchmarklevel/' + FPValueId + '/' + Accuracy + '/' + CompScore).success(function (data) {
			                    return data;
			                })
							.error(function (data, status, headers, config) {
							    return 'error';
							});
			            }

			            function getCalculationFunction(field) {
			                var calcFunctionName = field.CalculationFunction;
			                var calcFunction;

			                switch (calcFunctionName) {
			                    case 'Sum':
			                        calcFunction = SumFunction;
			                        break;
			                    case 'SumBool':
			                        calcFunction = SumBoolFunction;
			                        break;
			                    case 'SumBoolByGroup':
			                        calcFunction = SumBoolByGroupFunction;
			                        break;
			                    case 'ConcatenatedMissingLetters':
			                        calcFunction = ConcatenatedMissingLetters;
			                        break;
			                    case 'BenchmarkLevel':
			                        calcFunction = BenchmarkFunction;
			                        break;
			                    default:
			                        calcFunction = null;
			                        break;
			                }

			                return calcFunction;
			            }

			            var templateText = getTemplate(scope.currentField, scope.orientation);
			            var calcFunction = getCalculationFunction(scope.currentField);
			            var dataToAppend = templateText;
			            element.html(dataToAppend);
			            $compile(element.contents())(scope);



			            scope.checkClick = function (boolValue, calcFields, fieldResult) {
			                // most of the time, we will not do anything
			                if (calcFields) {
			                    var aryFields = calcFields.split(","); 
			                    
			                    for (var i = 0; i < aryFields.length; i++) {
			                        var fieldDbColumn = aryFields[i];

			                        for (var j = 0; j < scope.studentResult.FieldResults.length; j++) {
			                            if (scope.studentResult.FieldResults[j].DbColumn == fieldDbColumn) {
			                                scope.studentResult.FieldResults[j].BoolValue = boolValue;
			                            }
			                        }
			                    }
			                } else {
			                    // if this is a normal field, let's see if we need to uncheck the word
			                    if (fieldResult.Field.DatabaseColumn.indexOf('wsc') < 0) {
			                        if (!boolValue) {
			                            for (var j = 0; j < scope.studentResult.FieldResults.length; j++) {
			                                if (scope.studentResult.FieldResults[j].DbColumn.indexOf('wsc') > 0 && scope.studentResult.FieldResults[j].Field.GroupId == fieldResult.Field.GroupId) {
			                                    scope.studentResult.FieldResults[j].BoolValue = false;
			                                }
			                            }
			                        }
			                    }
			                }
			            }


			            scope.Calculate = function (args) {
			                return calcFunction(args);
			            }
			        }
			    };
			}
        ])
.directive('assessmentHfwEditFieldCustom', [
			'Assessment', '$routeParams', '$compile', '$templateCache', '$http', function (Assessment, $routeParams, $compile, $templateCache, $http) {

			    var getTemplate = function (field) {
			        var type = field.FieldType;

			        var type = field.FieldType;
			        var template = '';

			        if (type === 'DateCheckbox') {
			            template = $templateCache.get('templates/assessment-datecheckbox.html')
			        } else if (type == 'CalculatedFieldDbBacked') {
			            template = $templateCache.get('templates/assessment-calculatedfielddbbacked-custom.html');
			        }
			        else if (type == 'Textarea') {
			            template = $templateCache.get('templates/assessment-textarea-custom.html');
			        }

			        return template;
			    }

			    return {
			        restrict: 'E',
			        template: '<div>{{result}}</div>',
			        scope: {
			            currentField: '=',
			            studentResult: '=',
			            lookupFieldsArray: '=',
			            groups: '=',
			            fields: '=',
			            defaultDate: '=',
                        fieldMode: '='
			        },
			        link: function (scope, element, attr) {

			            // get our own lookupFields
			            scope.lookupValues = [];
			            scope.fieldResult = {};

			            scope.formattedDate = function (date) {
			                if (date != null) {
			                    if (moment.isDate(date)) {
			                        return moment(date).format("DD-MMM-YYYY");
			                    } else {
			                        return moment(date, "YYYY-MM-DD").format("DD-MMM-YYYY");
			                    }
			                }
			                return '';
			            }

			            scope.checkClick = function ($event) {
			                //   $event.preventDefault();
			                //   $event.stopPropagation();
			                //     $event.stopImmediatePropagation();

			                if (scope.fieldResult.BoolValue) {
			                    if (moment(scope.fieldResult.DateValue).toString() == moment(scope.defaultDate).toString()) {
			                        scope.fieldResult.DateValue = null;//TODO: Get value from date box
			                        scope.fieldResult.BoolValue = false;
			                    } else {
			                            scope.fieldResult.DateValue = null;//TODO: Get value from date box
			                            scope.fieldResult.BoolValue = false;
			                    }
			                }
			                else {
			                    scope.fieldResult.DateValue = angular.isDefined(scope.defaultDate) ? scope.defaultDate : new Date();//TODO: Get value from date box
			                    scope.fieldResult.BoolValue = true;
			                }
			                scope.fieldResult.IsModified = true;
			            }

			            // determine 'which' fieldResult is our currentFieldResult
			            // if field doesn't have a 'databasecolumn', then it is a label of some sort
			            // just use a standard display template
			            if (scope.currentField.DatabaseColumn != '' || scope.currentField.DatabaseColumn != null) {
			                // this would be one of the total or the comment columns
			                if (scope.currentField.CategoryId === null) {
			                    for (var j = 0; j < scope.studentResult.TotalFieldResults.length; j++) {
			                        if (scope.studentResult.TotalFieldResults[j].DbColumn === scope.currentField.DatabaseColumn) {
			                            scope.fieldResult = scope.studentResult.TotalFieldResults[j];
			                            break;
			                        }
			                    }
			                }
			                else if (scope.currentField.Category.DisplayName === 'Read') {
			                    for (var j = 0; j < scope.studentResult.ReadFieldResults.length; j++) {
			                        if (scope.studentResult.ReadFieldResults[j].DbColumn === scope.currentField.DatabaseColumn) {
			                            scope.fieldResult = scope.studentResult.ReadFieldResults[j];
			                            break;
			                        }
			                    }
			                } else if(scope.currentField.Category.DisplayName == 'Write') {
			                    for (var j = 0; j < scope.studentResult.WriteFieldResults.length; j++) {
			                        if (scope.studentResult.WriteFieldResults[j].DbColumn === scope.currentField.DatabaseColumn) {
			                            scope.fieldResult = scope.studentResult.WriteFieldResults[j];
			                            break;
			                        }
			                    }
			                }

			            }




			            var SumBoolByCategory = function () {

			                var sum = 0;

			                if (scope.currentField.CalculationFields == 'read') {
			                    for (var j = 0; j < scope.studentResult.ReadFieldResults.length; j++) {
			                        if (scope.studentResult.ReadFieldResults[j].BoolValue) {
			                            sum++;
			                        }
			                    }
			                } else if (scope.currentField.CalculationFields == 'write') {
			                    for (var j = 0; j < scope.studentResult.WriteFieldResults.length; j++) {
			                        if (scope.studentResult.WriteFieldResults[j].BoolValue) {
			                            sum++;
			                        }
			                    }
			                } else {
			                    for (var j = 0; j < scope.studentResult.WriteFieldResults.length; j++) {
			                        if (scope.studentResult.WriteFieldResults[j].BoolValue && scope.studentResult.ReadFieldResults[j].BoolValue) {
			                            sum++;
			                        }
			                    }
			                }
			                return sum;
			            };

			            function getCalculationFunction(field) {
			                var calcFunctionName = field.CalculationFunction;
			                var calcFunction;

			                switch (calcFunctionName) {
			                    case 'SumBoolByCategory':
			                        calcFunction = SumBoolByCategory;
			                        break;
			                    default:
			                        calcFunction = null;
			                        break;
			                }

			                return calcFunction;
			            }

			            var templateText = getTemplate(scope.currentField);
			            //var calcFunction = getCalculationFunction(scope.currentField);
			            var dataToAppend = templateText;
			            element.html(dataToAppend);
			            $compile(element.contents())(scope);


			            scope.checkClick2 = function (boolValue, calcFields) {
			                // most of the time, we will not do anything
			                if (calcFields) {
			                    var aryFields = calcFields.split(",");
			                    var sum = 0;

			                    for (var i = 0; i < aryFields.length; i++) {
			                        var fieldDbColumn = aryFields[i];

			                        for (var j = 0; j < scope.studentResult.FieldResults.length; j++) {
			                            if (scope.studentResult.FieldResults[j].DbColumn == fieldDbColumn) {
			                                scope.studentResult.FieldResults[j].BoolValue = boolValue;
			                            }
			                        }
			                    }
			                }
			            }

			            var calcFunction = getCalculationFunction(scope.currentField);
			            scope.Calculate = function (args) {
			                return calcFunction(args);
			            }
			        }
			    };
			}
])
.service('nsLookupFieldService', ['$http', '$routeParams', 'webApiBaseUrl', function ($http, $routeParams, webApiBaseUrl) {
    var self = this;
    self.LookupFieldsArray = [];

    self.LoadLookupFields = function () {
        // only need to do this once
        if (self.LookupFieldsArray.length > 0) {
            return;
        }

        return $http.get(webApiBaseUrl + '/api/assessment/GetAllLookupFields').then(
            function (response) {
                self.LookupFieldsArray = response.data;;
        });
    };
}])
.factory('NSSectionAssessmentDataEntryManager', [
        '$http', 'webApiBaseUrl', 'nsLookupFieldService', function ($http, webApiBaseUrl, nsLookupFieldService) {
            var self = this;


            var NSSectionAssessmentDataEntryManager = function (lookupFieldsArray) {
                self.LookupFieldsArray = lookupFieldsArray;

                this.initialize = function () {

                }

                this.copyStudentAssessmentData = function (assessmentId, selectedBenchmarkDate, targetBenchmarkDate, section, studentId) {
                    var returnObject = {
                        StudentId: studentId,
                        SelectedBenchmarkDate: selectedBenchmarkDate,
                        TargetBenchmarkDate: targetBenchmarkDate,
                        AssessmentId: assessmentId,
                        Section: section
                    }

                    return $http.post(webApiBaseUrl + "/api/dataentry/CopyStudentAssessmentData", returnObject);
                }

                this.copySectionAssessmentData = function (assessmentId, selectedBenchmarkDate, targetBenchmarkDate, section, studentResults) {
                    var returnObject = {
                        SelectedBenchmarkDate: selectedBenchmarkDate,
                        TargetBenchmarkDate: targetBenchmarkDate,
                        AssessmentId: assessmentId,
                        Section: section
                    }

                    return $http.post(webApiBaseUrl + "/api/dataentry/CopySectionAssessmentData", returnObject);
                }

                this.copyFromStudentAssessmentData = function (assessmentId, selectedBenchmarkDate, sourceBenchmarkDate, section, studentId) {
                    var returnObject = {
                        StudentId: studentId,
                        SelectedBenchmarkDate: selectedBenchmarkDate,
                        SourceBenchmarkDate: sourceBenchmarkDate,
                        AssessmentId: assessmentId,
                        Section: section
                    }

                    return $http.post(webApiBaseUrl + "/api/dataentry/CopyFromStudentAssessmentData", returnObject);
                }

                this.copyFromSectionAssessmentData = function (assessmentId, selectedBenchmarkDate, sourceBenchmarkDate, section, studentResults) {
                    var returnObject = {
                        SelectedBenchmarkDate: selectedBenchmarkDate,
                        SourceBenchmarkDate: sourceBenchmarkDate,
                        AssessmentId: assessmentId,
                        Section: section
                    }

                    return $http.post(webApiBaseUrl + "/api/dataentry/CopyFromSectionAssessmentData", returnObject);
                }

                this.loadAssessmentResultData = function (assessmentId, nsFilterOptionsService) {
                    var postObject = { AssessmentId: assessmentId, SectionId: nsFilterOptionsService.selectedSection.id, BenchmarkDateId: nsFilterOptionsService.selectedBenchmarkDate.id };
                    var url = webApiBaseUrl + '/api/dataentry/GetAssessmentResults';
                    return $http.post(url, postObject);
                }

                this.makeDatesPopupCompatible = function (studentResultsArray) {
                    for (var j = 0; j < studentResultsArray.length; j++) {
                        var result = studentResultsArray[j];

                        if (result.TestDate == null || typeof result.TestDate == 'undefined' || result.TestDateDisplay == '' || result.TestDateDisplay == null) {
                            result.TestDate = moment().format('DD-MMM-YYYY');
                        } else {
                            var momentizedDate = moment(result.TestDateDisplay, 'DD-MMM-YYYY');  // TODO:  make sure all dates that are parsed are sent as TEXT!!!!
                            result.TestDate = momentizedDate.format('DD-MMM-YYYY');
                        }
                    }
                }

                this.attachFieldsToResults = function (studentResultsArray, fieldsArray) {
                    console.time("Start attach fields");
                    for (var j = 0; j < studentResultsArray.length; j++) {
                        for (var k = 0; k < studentResultsArray[j].FieldResults.length; k++) {
                            for (var r = 0; r < fieldsArray.length; r++) {
                                if (fieldsArray[r].DatabaseColumn == studentResultsArray[j].FieldResults[k].DbColumn) {
                                    studentResultsArray[j].FieldResults[k].Field = angular.copy(fieldsArray[r]);

                                    // set display value
                                    if (fieldsArray[r].FieldType === "DropdownFromDB") {
                                        for (var p = 0; p < self.LookupFieldsArray.length; p++) {
                                            if (self.LookupFieldsArray[p].LookupColumnName === fieldsArray[r].LookupFieldName) {
                                                // now find the specifc value that matches
                                                for (var y = 0; y < self.LookupFieldsArray[p].LookupFields.length; y++) {
                                                    if (studentResultsArray[j].FieldResults[k].IntValue === self.LookupFieldsArray[p].LookupFields[y].FieldSpecificId) {
                                                        studentResultsArray[j].FieldResults[k].DisplayValue = self.LookupFieldsArray[p].LookupFields[y].FieldValue;
                                                    }
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

                this.initializeHeaderClassArray = function (fields, headerClassArray) {
                    for (var r = 0; r < fields.length; r++) {
                        headerClassArray[r] = 'fa';
                    }
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
                                else if (column === 'StudentName') {
                                    staticColumnsObj.studentNameHeaderClass = "fa";
                                }
                                sortArray.splice(j, 1);
                            } else {
                                if (columnIndex > -1) {
                                    headerClassArray[columnIndex] = "fa fa-chevron-down";
                                }
                                else if (column === 'StudentName') {
                                    staticColumnsObj.studentNameHeaderClass = "fa fa-chevron-down";
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
                        else if (column === 'StudentName') {
                            staticColumnsObj.studentNameHeaderClass = "fa fa-chevron-up";
                        }
                    }
                };

                this.saveAssessmentResult = function (assessmentId, studentResult, benchmarkDateId) {

                    var returnObject = {
                        StudentResult: studentResult,
                        AssessmentId: assessmentId,
                        BenchmarkDateId: benchmarkDateId
                    }

                    return $http.post(webApiBaseUrl + "/api/dataentry/SaveAssessmentResult", returnObject);
                };

                this.deleteStudentTestResult = function (assessmentId, studentResult) {

                    var returnObject = {
                        StudentResult: studentResult,
                        AssessmentId: assessmentId
                    }

                    return $http.post(webApiBaseUrl + "/api/dataentry/DeleteAssessmentResult", returnObject);
                };

                this.cleanupAfterDelete = function () {
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
                };

                this.initialize();

            };

            return (NSSectionAssessmentDataEntryManager);
        }
])
})();