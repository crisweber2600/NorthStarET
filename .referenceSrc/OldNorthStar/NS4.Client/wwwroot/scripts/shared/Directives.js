'use strict'

angular
	.module('theme.directives', [])
	.directive('disableAnimation', [
		'$animate', function($animate) {
			return {
				restrict: 'A',
				link: function($scope, $element, $attrs) {
					$attrs.$observe('disableAnimation', function(value) {
						$animate.enabled(!value, $element);
					});
				}
			}
		}
	])
	.directive('slideOut', function() {
		return {
			restrict: 'A',
			scope: {
				show: '=slideOut'
			},
			link: function(scope, element, attr) {
				element.hide();
				scope.$watch('show', function(newVal, oldVal) {
					if (newVal !== oldVal) {
						element.slideToggle({
							complete: function() { scope.$apply(); }
						});
					}
				});
			}
		}
	})
    .directive('ngEnter', function () {
        return function (scope, element, attrs) {
            element.bind("keydown keypress", function (event) {
                if (event.which === 13) {
                    scope.$apply(function () {
                        scope.$eval(attrs.ngEnter);
                    });

                    event.preventDefault();
                }
            });
        };
    })
	.directive('slideOutNav', [
		'$timeout', function($t) {
			return {
				restrict: 'A',
				scope: {
					show: '=slideOutNav'
				},
				link: function(scope, element, attr) {
					scope.$watch('show', function(newVal, oldVal) {
						if ($('body').hasClass('collapse-leftbar')) {
							if (newVal == true)
								element.css('display', 'block');
							else
								element.css('display', 'none');
							return;
						}
						if (newVal == true) {
							element.slideDown({
								complete: function() {
									$t(function() { scope.$apply() })
								}
							});
						} else if (newVal == false) {
							element.slideUp({
								complete: function() {
									$t(function() { scope.$apply() })
								}
							});
						}
					});
				}
			}
		}
	])
	.directive('panel', function() {
		return {
			restrict: 'E',
			transclude: true,
			scope: {
				panelClass: '@',
				heading: '@',
				panelIcon: '@'
			},
			templateUrl: 'templates/panel.html',
		}
	})
	.directive('pulsate', function() {
		return {
			scope: {
				pulsate: '='
			},
			link: function(scope, element, attr) {
				// stupid hack to prevent FF from throwing error
				if (element.css('background-color') == "transparent") {
					element.css('background-color', "rgba(0,0,0,0.01)");
				}
				$(element).pulsate(scope.pulsate);
			}
		}
	})
	.directive('prettyprint', function() {
		return {
			restrict: 'C',
			link: function postLink(scope, element, attrs) {
				element.html(prettyPrintOne(element.html(), '', true));
			}
		};
	})
	.directive("passwordVerify", function() {
		return {
			require: "ngModel",
			scope: {
				passwordVerify: '='
			},
			link: function(scope, element, attrs, ctrl) {
				scope.$watch(function() {
					var combined;

					if (scope.passwordVerify || ctrl.$viewValue) {
						combined = scope.passwordVerify + '_' + ctrl.$viewValue;
					}
					return combined;
				}, function(value) {
					if (value) {
						ctrl.$parsers.unshift(function(viewValue) {
							var origin = scope.passwordVerify;
							if (origin !== viewValue) {
								ctrl.$setValidity("passwordVerify", false);
								return undefined;
							} else {
								ctrl.$setValidity("passwordVerify", true);
								return viewValue;
							}
						});
					}
				});
			}
		};
	})
	.directive('backgroundSwitcher', function() {
		return {
			restrict: 'EA',
			link: function(scope, element, attr) {
				$(element).click(function() {
					$('body').css('background', $(element).css('background'));
				});
			}
		};
	})
	.directive('panelControls', [
		function() {
			return {
				restrict: 'E',
				require: '?^tabset',
				link: function(scope, element, attrs, tabsetCtrl) {
					var panel = $(element).closest('.panel');
					if (panel.hasClass('.ng-isolate-scope') == false) {
						$(element).appendTo(panel.find('.options'));
					}
				}
			};
		}
	])
	.directive('panelControlCollapse', function() {
		return {
			restrict: 'EAC',
			link: function(scope, element, attr) {
				element.bind('click', function() {
					$(element).toggleClass("fa-chevron-down fa-chevron-up");
					$(element).closest(".panel").find('.panel-body').slideToggle({ duration: 200 });
					$(element).closest(".panel-heading").toggleClass('rounded-bottom');
				})
				return false;
			}
		};
	})
	.directive('icheck', function($timeout, $parse) {
		return {
			require: '?ngModel',
			link: function($scope, element, $attrs, ngModel) {
				return $timeout(function() {
					var parentLabel = element.parent('label');
					if (parentLabel.length)
						parentLabel.addClass('icheck-label');
					var value;
					value = $attrs['value'];

					$scope.$watch($attrs['ngModel'], function(newValue) {
						$(element).iCheck('update');
					})

					return $(element).iCheck({
						checkboxClass: 'icheckbox_minimal-blue',
						radioClass: 'iradio_minimal-blue'

					}).on('ifChanged', function(event) {
						if ($(element).attr('type') === 'checkbox' && $attrs['ngModel']) {
							$scope.$apply(function() {
								return ngModel.$setViewValue(event.target.checked);
							});
						}
						if ($(element).attr('type') === 'radio' && $attrs['ngModel']) {
							return $scope.$apply(function() {
								return ngModel.$setViewValue(value);
							});
						}
					});
				});
			}
		};
	})
	.directive('knob', function() {
		return {
			restrict: 'EA',
			template: '<input class="dial" type="text"/>',
			scope: {
				options: '='
			},
			replace: true,
			link: function(scope, element, attr) {
				$(element).knob(scope.options);
			}
		}
	})
	.directive('uiBsSlider', [
		'$timeout', function($timeout) {
			return {
				link: function(scope, element, attr) {
					// $timeout is needed because certain wrapper directives don't
					// allow for a correct calculaiton of width
					$timeout(function() {
						element.slider();
					});
				}
			};
		}
	])
	.directive('tileLarge', function() {
		return {
			restrict: 'E',
			scope: {
				item: '=data'
			},
			templateUrl: 'templates/tile-large.html',
			replace: true,
			transclude: true
		}
	})
	.directive('tileMini', function() {
		return {
			restrict: 'E',
			scope: {
				item: '=data'
			},
			replace: true,
			templateUrl: 'templates/tile-mini.html'
		}
	})
	.directive('tile', function() {
		return {
			restrict: 'E',
			scope: {
				heading: '@',
				type: '@'
			},
			transclude: true,
			templateUrl: 'templates/tile-generic.html',
			link: function(scope, element, attr) {
				var heading = element.find('tile-heading');
				if (heading.length) {
					heading.appendTo(element.find('.tiles-heading'));
				}
			},
			replace: true
		}
	})
	.directive('jscrollpane', [
		'$timeout', function($timeout) {
			return {
				restrict: 'A',
				scope: {
					options: '=jscrollpane'
				},
				link: function(scope, element, attr) {
					$timeout(function() {
						if (navigator.appVersion.indexOf("Win") != -1)
							element.jScrollPane($.extend({ mouseWheelSpeed: 20 }, scope.options))
						else
							element.jScrollPane(scope.options);
						element.on('click', '.jspVerticalBar', function(event) {
							event.preventDefault();
							event.stopPropagation();
						});
						element.bind('mousewheel', function(e) {
							e.preventDefault();
						});
					});
				}
			};
		}
	])
	// specific to app
	.directive('stickyScroll', function() {
		return {
			restrict: 'A',
			link: function(scope, element, attr) {
				function stickyTop() {
					var topMax = parseInt(attr.stickyScroll);
					var headerHeight = $('header').height();
					if (headerHeight > topMax) topMax = headerHeight;
					if ($('body').hasClass('static-header') == false)
						return element.css('top', topMax + 'px');
					var window_top = $(window).scrollTop();
					var div_top = element.offset().top;
					if (window_top < topMax) {
						element.css('top', (topMax - window_top) + 'px');
					} else {
						element.css('top', 0 + 'px');
					}
				}

				$(function() {
					$(window).scroll(stickyTop);
					stickyTop();
				});
			}
		}
	})
	.directive('rightbarRightPosition', function() {
		return {
			restrict: 'A',
			scope: {
				isFixedLayout: '=rightbarRightPosition'
			},
			link: function(scope, element, attr) {
				scope.$watch('isFixedLayout', function(newVal, oldVal) {
					if (newVal != oldVal) {
						setTimeout(function() {
							var $pc = $('#page-content');
							var ending_right = ($(window).width() - ($pc.offset().left + $pc.outerWidth()));
							if (ending_right < 0) ending_right = 0;
							$('#page-rightbar').css('right', ending_right);
						}, 100);
					}
				});
			}
		};
	})
	.directive('fitHeight', [
		'$window', '$timeout', '$location', function($window, $timeout, $location) {
			return {
				restrict: 'A',
				scope: true,
				link: function(scope, element, attr) {
					scope.docHeight = $(document).height();
					var setHeight = function(newVal) {
						var diff = $('header').height();
						if ($('body').hasClass('layout-horizontal')) diff += 112;
						if ((newVal - diff) > element.outerHeight()) {
							element.css('min-height', (newVal - diff) + 'px');
						} else {
							element.css('min-height', $(window).height() - diff);
						}
					};
					scope.$watch('docHeight', function(newVal, oldVal) {
						setHeight(newVal);
					});
					$(window).on('resize', function() {
						setHeight($(document).height());
					});
					var resetHeight = function() {
						scope.docHeight = $(document).height();
						$timeout(resetHeight, 1000);
					}
					$timeout(resetHeight, 1000);
				}
			};
		}
	])
	.directive('jscrollpaneOn', [
		'$timeout', function($timeout) {
			return {
				restrict: 'A',
				scope: {
					applyon: '=jscrollpaneOn'
				},
				link: function(scope, element, attr) {
					scope.$watch('applyon', function(newVal) {
						if (newVal == false) {
							var api = element.data('jsp');
							if (api) api.destroy();
							return;
						}
						$timeout(function() {
							element.jScrollPane({ autoReinitialise: true });
						});
					});
				}
			};
		}
	])
	.directive('backToTop', function() {
		return {
			restrict: 'AE',
			link: function(scope, element, attr) {
				element.click(function(e) {
					$('body').scrollTop(0);
				});
			}
		}
	})
	.directive('nsErrorDisplay', [
		'progressLoader', function (progressLoader) {
		    return {
		        restrict: 'E',
		        templateUrl: 'templates/ns-errors.html',
		        link: function (scope, element, attr) {
		            scope.errors = [];
		            scope.$on('NSHTTPError', function (event, data) {
		                scope.errors.push({ type: "danger", msg: data });
		                $('html, body').animate({ scrollTop: 0 }, 'fast');
		                progressLoader.end();
		            });
		        }
		    };
		}
	])
	.directive('assessmentPreview', [
		'Assessment', '$routeParams', function(Assessment, $routeParams) {
			return {
				restrict: 'E',
				templateUrl: 'templates/assessment-preview.html',
				link: function(scope, element, attr) {
					scope.assessment = Assessment.get({ id: $routeParams.id });
					scope.allFields = scope.assessment.Fields;
				}
			};
		}
	])
	.directive('assessmentField', [
			'Assessment', '$routeParams', '$compile', '$templateCache', '$http', function (Assessment, $routeParams, $compile, $templateCache, $http) {

				var getTemplate = function(field) {
					var type = field.FieldType;
					var template = '';

					switch (type) {
                    case 'DateCheckbox':
							template = $templateCache.get('templates/assessment-datecheckbox.html');
							break;
					case 'Textfield':
						template = $templateCache.get('templates/assessment-textfield.html');
						break;
					case 'DecimalRange':
						template = $templateCache.get('templates/assessment-decimal.html');
						break;
					case 'DropdownRange':
						template = $templateCache.get('templates/assessment-dropdownrange.html');
						break;
					case 'DropdownFromDB':
						template = $templateCache.get('templates/assessment-dropdownfromdb.html');
						break;
					case 'CalculatedField':
						template = $templateCache.get('templates/assessment-calculatedlabel.html');
						break;
					default:
						template = $templateCache.get('templates/assessment-textfield.html');
						break;
					}

					return template;
				}

			return {
					restrict: 'E',
					template: '<div>{{field}}</div>',
					scope: {
						field: '=',
						allFields: '='
					},
					link: function (scope, element, attr) {

						var SumFunction = function (args) {
							var aryFields = args.split(",");
							var sum = 0;

							for (var i = 0; i < aryFields.length; i++) {
								var fieldId = parseInt(aryFields[i]);

								for (var j = 0; j < scope.allFields.length; j++) {
									if (scope.allFields[j].Id == fieldId) {
										sum += scope.allFields[j].FieldOrder;
									}
								}
							}
							return sum;
						};

						var BenchmarkFunction = function (args) {
							return 334;
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
								default:
									calcFunction = null;
									break;
							}

							return calcFunction;
						}

						var templateText = getTemplate(scope.field);
						var calcFunction = getCalculationFunction(scope.field);
						var dataToAppend = templateText;
						element.html(dataToAppend);
						$compile(element.contents())(scope);

						



						scope.Calculate = function(args) {
							return calcFunction(args);
						}

						scope.lookupValues = [];
						scope.loadLookupValues =  function(lookupFieldName) {
							return scope.lookupValues.length ? null : $http.get('/api/assessment/getlookupfield/' + lookupFieldName).success(function (data) {
								scope.lookupValues = data;
							});
						}
					}
				};
			}
	])
.directive('assessmentEditField', [
			'Assessment', '$routeParams', '$compile', '$templateCache', '$http', function (Assessment, $routeParams, $compile, $templateCache, $http) {

				var getTemplate = function (field) {
					var type = field.FieldType;
					var template = '';

					switch (type) {
                        case 'DateCheckbox':
							template = $templateCache.get('templates/assessment-datecheckbox.html');
							break;
						case 'Textfield':
							template = $templateCache.get('templates/assessment-textfield.html');
							break;
						case 'DecimalRange':
							template = $templateCache.get('templates/assessment-decimal.html');
							break;
						case 'DropdownRange':
							template = $templateCache.get('templates/assessment-dropdownrange.html');
							break;
						case 'DropdownFromDB':
							template = $templateCache.get('templates/assessment-dropdownfromdb.html');
							break;
						case 'CalculatedFieldClientOnly':
							template = $templateCache.get('templates/assessment-calculatedlabel.html');
							break;
						case 'CalculatedFieldDbBacked':
							template = $templateCache.get('templates/assessment-calculatedlabeldbbacked.html');
							break;
					    case 'CalculatedFieldDbBackedString':
					        template = $templateCache.get('templates/assessment-calculatedlabeldbbackedstring.html');
					        break;
						case 'CalculatedFieldDbOnly':
							template = $templateCache.get('templates/assessment-calculatedlabeldbonly.html');
							break;
						default:
							template = $templateCache.get('templates/assessment-textfield.html');
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
						eForm: '=',
						lookupFieldsArray: '='
					},
					link: function (scope, element, attr) {

						// get our own lookupFields
						scope.lookupValues = [];

						for(var i = 0; i <scope.lookupFieldsArray.length;i++ )
							if (scope.lookupFieldsArray[i].LookupColumnName === scope.result.Field.LookupFieldName) {
								scope.lookupValues = scope.lookupFieldsArray[i].LookupFields;
								break;
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
							$http.get('/api/assessment/getbenchmarklevel/' + FPValueId + '/' + Accuracy + '/' + CompScore).success(function(data) {
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
								case 'BenchmarkLevel':
									calcFunction = BenchmarkFunction;
									break;
								default:
									calcFunction = null;
									break;
							}

							return calcFunction;
						}

						var templateText = getTemplate(scope.result.Field);
						var calcFunction = getCalculationFunction(scope.result.Field);
						var dataToAppend = templateText;
						element.html(dataToAppend);
						$compile(element.contents())(scope);

						scope.Calculate = function (args) {
							return calcFunction(args);
						}

						//scope.lookupValues = [];
						//scope.loadLookupValues = function (lookupFieldName) {
						//	//return [];
						//		return scope.lookupValues.length ? null : $http.get('/api/assessment/getlookupfield/' + lookupFieldName).success(function (data) {
						//			scope.lookupValues = data;
						//		});
						//}
					}
				};
			}
])    
.directive('manualswitch', function () {
  return {
    restrict: 'AE',
    replace: true,
    transclude: true,
    template: function (element, attrs) {
      var html = '';
      html += '<span';
      html += ' class="switch' + (attrs.class ? ' ' + attrs.class : '') + '"';
      //html += attrs.ngModel ? ' ng-click="' (attrs.ngChange ? '; ' + attrs.ngChange + '()"' : '"') : '';
      html += ' ng-class="{ checked:' + attrs.ngModel + ' }"';
      html += '>';
      html += '<small></small>';
      html += '<input type="checkbox"';
      html += attrs.id ? ' id="' + attrs.id + '"' : '';
      html += attrs.name ? ' name="' + attrs.name + '"' : '';
      html += attrs.ngModel ? ' ng-model="' + attrs.ngModel + '"' : '';
      html += ' style="display:none" />';
      html += '<span class="switch-text">';
      /*adding new container for switch text*/
      html += attrs.on ? '<span class="on">' + attrs.on + '</span>' : '';
      /*switch text on value set by user in directive html markup*/
      html += attrs.off ? '<span class="off">' + attrs.off + '</span>' : ' ';
      /*switch text off value set by user in directive html markup*/
      html += '</span>';
      return html;
    }
  };
})
.directive('spellingInventoryTotalColumn', [
			'Assessment', '$routeParams', '$compile', '$templateCache', '$http', function (Assessment, $routeParams, $compile, $templateCache, $http) {

			    var getTemplate = function (field) {
			        var template = $templateCache.get('templates/assessment-calculatedfielddbbacked-custom.html');
			        return template;
			    }

			    return {
			        restrict: 'E',
			        scope: {
			            studentResult: '=',
			            groups: '=',
                        fields: '=',
                        currentCategory: '='
			        },
			        link: function (scope, element, attr) {

                        scope.currentField = {};
                        // get currentField for current Category
                        for (var i = 0; i < scope.fields.length; i++) {
			                if(scope.currentCategory.Id === scope.fields[i].CategoryId && scope.fields[i].Page === 2) {
                                scope.currentField = scope.fields[i];
                                break;
                            }
			            }

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

			            function getCalculationFunction(field) {
			                var calcFunctionName = field.CalculationFunction;
			                var calcFunction = SumBoolFunction;

			                return calcFunction;
			            }

			            var templateText = getTemplate(scope.currentField);
			            var calcFunction = getCalculationFunction(scope.currentField);
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
.directive('genericReadOnlyField', [
			'$routeParams', '$compile', '$templateCache', '$http', function ($routeParams, $compile, $templateCache, $http) {

				var getTemplate = function (field) {
					var type = field.FieldType;
					var template = '';

					switch (type) {
                        case 'DateCheckbox':
							template = $templateCache.get('templates/assessment-datecheckbox-readonly.html');
							break;
						case 'Textfield':
							template = $templateCache.get('templates/assessment-textfield-readonly.html');
							break;
						case 'DecimalRange':
							template = $templateCache.get('templates/assessment-decimal-readonly.html');
							break;
						case 'DropdownRange':
							template = $templateCache.get('templates/assessment-dropdownrange-readonly.html');
							break;
						case 'DropdownFromDB':
							template = $templateCache.get('templates/assessment-dropdownfromdb-readonly.html');
							break;
						case 'CalculatedFieldClientOnly':
							template = $templateCache.get('templates/assessment-calculatedlabel-readonly.html');
							break;
						case 'CalculatedFieldDbBacked':
							template = $templateCache.get('templates/assessment-calculatedlabeldbbacked-readonly.html');
							break;
					    case 'CalculatedFieldDbBackedString':
					        template = $templateCache.get('templates/assessment-calculatedlabeldbbackedstring-readonly.html');
					        break;
						case 'CalculatedFieldDbOnly':
							template = $templateCache.get('templates/assessment-calculatedlabeldbonly-readonly.html');
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

						for (var i = 0; i < scope.lookupFieldsArray.length; i++)
							if (scope.lookupFieldsArray[i].LookupColumnName === scope.result.LookupFieldName) {
								scope.lookupValues = scope.lookupFieldsArray[i].LookupFields;
								break;
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

						
						

						function getCalculationFunction(field) {
							var calcFunctionName = field.CalculationFunction;
							var calcFunction;

							switch (calcFunctionName) {
								case 'Sum':
									calcFunction = SumFunction;
									break;
								default:
									calcFunction = null;
									break;
							}

							return calcFunction;
						}

						var templateText = getTemplate(scope.result.Field);
						var calcFunction = getCalculationFunction(scope.result.Field);
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

   
     .directive('testDueDateFooter', [
			'$routeParams', '$compile', '$templateCache', '$http', function ($routeParams, $compile, $templateCache, $http) {


			    return {
			        restrict: 'EA',
			        scope: {
			            tdds: '='
			        },
			        link: function (scope, element, attr) {

			            scope.$watch('tdds', function () {
			                var outputHtml = '';

			                outputHtml += '<table><tbody><tr>';

			                for (var i = 0; i < scope.tdds.length; i++) {
			                    outputHtml += '<td style="padding-right:5px;"><span style="border:1px solid black;display:inline-block;height:20px;width:25px;Background-color:' + scope.tdds[i].Hex + ';"></span></td><td style="padding-right:10px"><strong>' + scope.tdds[i].DisplayDate + '</strong></td>';
			                }

			                outputHtml += "</tr></tbody></table>";
			                outputHtml += "<div><span class='badge' style='background-color:black;color:white;'>X</span> = Score Lower Than Previous Test</div>";

			                element.html(outputHtml);
			                $compile(element.contents())(scope);
			            });
			        }
			    };
			}
     ])
         .directive('attendanceBadge', [
			'$routeParams', '$compile', '$templateCache', function ($routeParams, $compile, $templateCache) {


			    return {
			        restrict: 'EA',
			        scope: {
			            dayStatus: '='
			        },
			        link: function (scope, element, attr) {
			            scope.$watch('dayStatus', function() {
			                var outputHtml = '';

			                if (scope.dayStatus === "Scheduled To Meet") {
			                    outputHtml = "<span class='badge badge-primary'><i class='fa fa-calendar'></i> " + scope.dayStatus + "</span>";
			                }
			                else if (scope.dayStatus === "None") {
			                    outputHtml = "<span class='badge badge-default'><i class='fa fa-square'></i> " + scope.dayStatus + "</span>";
			                }
			                else if (scope.dayStatus === "No School") {
			                    // this is for district and school holidays
			                    outputHtml = "<span class='badge badge-default'><i class='fa fa-lock'></i> " + scope.dayStatus + "</span>";
			                }
			                else if (scope.dayStatus === "None") {
			                    outputHtml = "<span class='badge badge-default'><i class='fa fa-square'></i> " + scope.dayStatus + "</span>";
			                }
			                else if (scope.dayStatus === "Teacher Absent" || scope.dayStatus === "Teacher Unavailable" || scope.dayStatus === "Child Absent" || scope.dayStatus === "Child Unavailable") {
			                    outputHtml = "<span class='badge badge-danger'><i class='fa fa-times'></i> " + scope.dayStatus + "</span>";
			                }
			                else if (scope.dayStatus === "Non-Cycle Day") {
			                    outputHtml = "<span class='badge badge-info'><i class='fa fa-info'></i> " + scope.dayStatus + "</span>";
			                }
			                else if (scope.dayStatus === "Make-Up Lesson" || scope.dayStatus === "Intervention Delivered") {
			                    outputHtml = "<span class='badge badge-success'><i class='fa fa-check'></i> " + scope.dayStatus + "</span>";
			                }
			                else {
			                    outputHtml = scope.dayStatus;
			                    // TODO: log unexpected result
			                }
			                element.html(outputHtml);
			                $compile(element.contents())(scope);
			            });
			        }
			    };
			}
         ])
    
    .directive('nsRepeatComplete', [
        '$rootScope', '$timeout', function ($rootScope, $timeout) {
            return {
                restrict: 'A',
                link: function (scope, element, attr) {
                    if (scope.$last === true) {
                        $timeout(function () {
                            scope.$emit('ngRepeatFinished');
                        });
                    }
                }
            }
        }

    ])
   
    


.directive('convertToNumber', function () {
    return {
        require: 'ngModel',
        link: function (scope, element, attrs, ngModel) {
            ngModel.$parsers.push(function (val) {
                return parseInt(val, 10);
            });
            ngModel.$formatters.push(function (val) {
                return '' + val;
            });
        }
    };
});

;

