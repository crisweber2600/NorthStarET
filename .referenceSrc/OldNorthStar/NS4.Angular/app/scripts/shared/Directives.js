'use strict'

angular
	.module('theme.directives', [])
    .directive('staticInclude', ['$templateCache','$compile', function ($templateCache, $compile) {
        return {
            restrict: 'AE',
            link: function (scope, element, attrs) {
                var template = $templateCache.get(attrs.source);
                element.html(template);
                $compile(element.contents())(scope);
            }
        };
    }])
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
			require: 'ngModel',
			scope: {
				options: '='
			},
			replace: true,
			link: function (scope, element, attr, ngModel) {
			    $(element).knob({
			        change: function (value) {
			            scope.$apply(function () {
			                ngModel.$setViewValue(value);
			            });
			        }
			    });
			    ngModel.$render = function () {
			        $(element).val(ngModel.$viewValue).trigger("change");
			    };
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
		                scope.errors = [];
		                scope.errors.push({ type: "danger", msg: data });
		                $('html, body').animate({ scrollTop: 0 }, 'fast');
		                progressLoader.end();
		            });

		            scope.$on('NSHTTPClear', function (event, data) {
		                scope.errors = [];
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
    .directive('standardReportHeader', [
		'$routeParams', '$compile', '$templateCache', '$http', '$filter', function ($routeParams, $compile, $templateCache, $http, $filter) {
		    return {
		        scope: {
		            options: '=',
                    heading: '='
		        },
		        restrict: 'E',
		        templateUrl: 'templates/standard-report-header.html',
		        link: function (scope, element, attr) {
		            scope.headingHtml = function () {
		                return $filter('unsafe')('<a class="navbar-brand" href="#/">North Star <span class="navbar-brand-faded">Educational Tools</span></a>');
		            }

		            scope.getFieldDisplayTest = function(fieldValue, emptyMessage, quickSearchEquivalentProperty) {
		                if(scope.options.quickSearchStudent){
		                    return quickSearchEquivalentProperty;
		                } else {
		                    return fieldValue || emptyMessage; 
		                }
		            }
		        }
		    };
		}
    ])

        .directive('hfwStudentFooter', [
		'$routeParams', '$compile', '$templateCache', '$http', '$filter', function ($routeParams, $compile, $templateCache, $http, $filter) {
		    return {
		        scope: {
		            options: '=',
		            heading: '='
		        },
		        restrict: 'E',
		        templateUrl: 'templates/hfw-student-footer.html',
		        link: function (scope, element, attr) {
		            scope.getFieldDisplayTest = function (fieldValue, emptyMessage, quickSearchEquivalentProperty) {
		                if (scope.options.quickSearchStudent) {
		                    return quickSearchEquivalentProperty;
		                } else {
		                    return fieldValue || emptyMessage;
		                }
		            }
		        }
		    };
		}
        ])


             .directive('studentDashboardLink', [
			'$routeParams', '$compile', '$templateCache', '$uibModal', '$timeout', function ($routeParams, $compile, $templateCache, $uibModal, $timeout) {
			    return {
			        restrict: 'EA',
			        templateUrl: 'templates/student-dashboard-link.html',
			        scope: {
			            studentId: '=',
                        studentName: '='
			        },
			        link: function (scope, element, attr) {
			            scope.openStudentDashboardDialog = function () {

			                var modalInstance = $uibModal.open({
			                    templateUrl: 'studentDashboardViewer.html',
			                    scope: scope,
			                    controller: function ($scope, $uibModalInstance) {
			                        // use jquery to change the class of the modal content
			                        $timeout(function () {
			                            $('div.modal-dialog').addClass('modal-dialog-max');
			                            $('div.modal-content').addClass('modal-content-max');
			                        }, 250);


			                        $scope.settings = { selectedStudent: { id: $scope.studentId, text: $scope.studentName } };
			                        $scope.cancel = function () {
			                            $uibModalInstance.dismiss('cancel');
			                            // set it back
			                        };
			                    },
			                    size: 'md',
			                });
			            }
			        }
			    };
			}
             ])
            .directive('interventionDashboardLink', [
			'$routeParams', '$compile', '$templateCache', '$uibModal', '$timeout', function ($routeParams, $compile, $templateCache, $uibModal, $timeout) {
			    return {
			        restrict: 'EA',
			        templateUrl: 'templates/intervention-dashboard-link.html',
			        scope: {
			            intervention: '='
			        },
			        link: function (scope, element, attr) {
			            scope.openInterventionDashboardDialog = function () {

			                var modalInstance = $uibModal.open({
			                    templateUrl: 'interventionDashboardViewer.html',
			                    scope: scope,
			                    controller: function ($scope, $uibModalInstance) {
			                        // use jquery to change the class of the modal content
			                        $timeout(function () {
			                            $('div.modal-dialog').addClass('modal-dialog-max');
			                            $('div.modal-content').addClass('modal-content-max');
			                        }, 250);


			                        $scope.settings = { selectedInterventionStudent: { id: $scope.intervention.StudentID}, selectedStint: { id: $scope.intervention.Id}, selectedInterventionGroup: { id: $scope.intervention.InterventionGroupId, text: $scope.intervention.Description}, selectedSchoolYear: { id: $scope.intervention.SchoolYear } };
			                        $scope.cancel = function () {
			                            $uibModalInstance.dismiss('cancel');
			                            // set it back
			                        };
			                    },
			                    size: 'md',
			                });
			            }
			        }
			    };
			}
            ])
                .directive('activeInterventionDashboardLink', [
			'$routeParams', '$compile', '$templateCache', '$uibModal', '$timeout', function ($routeParams, $compile, $templateCache, $uibModal, $timeout) {
			    return {
			        restrict: 'EA',
			        templateUrl: 'templates/active-intervention-dashboard-link.html',
			        scope: {
			            studentId: '=',
			            interventionId: '=',
			            groupId: '=',
			            schoolYear: '=',
			            interventionType: '=',
			            schoolId: '=',
			            interventionistId: '=',
                        staffInitials: '='
			        },
			        link: function (scope, element, attr) {
			            scope.openInterventionDashboardDialog = function () {

			                var modalInstance = $uibModal.open({
			                    templateUrl: 'interventionDashboardViewer.html',
			                    scope: scope,
			                    controller: function ($scope, $uibModalInstance) {
			                        // use jquery to change the class of the modal content
			                        $timeout(function () {
			                            $('div.modal-dialog').addClass('modal-dialog-max');
			                            $('div.modal-content').addClass('modal-content-max');
			                        }, 250);


			                        $scope.settings = { selectedInterventionStudent: { id: scope.studentId }, selectedStint: { id: scope.interventionId }, selectedInterventionGroup: { id: scope.groupId, text: scope.interventionType }, selectedSchoolYear: { id: scope.schoolYear } };
			                        $scope.cancel = function () {
			                            $uibModalInstance.dismiss('cancel');
			                            // set it back
			                        };
			                    },
			                    size: 'md',
			                });
			            }
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
    .directive('onFinishRender', ['$timeout', '$parse', function ($timeout, $parse) {
        return {
            restrict: 'A',
            link: function (scope, element, attr) {
                if (scope.$last === true) {
                    $timeout(function () {
                        scope.$emit('ngRepeatFinished');
                        if (!!attr.onFinishRender) {
                            $parse(attr.onFinishRender)(scope);
                        }
                    });
                }
            }
        }
    }])
    .value('THROTTLE_MILLISECONDS', 250)
     .directive('infiniteScroll', [
    '$rootScope', '$window', '$interval', 'THROTTLE_MILLISECONDS',
    function($rootScope, $window, $interval, THROTTLE_MILLISECONDS) {
        return {
            scope: {
                infiniteScroll: '&',
                infiniteScrollContainer: '=',
                infiniteScrollDistance: '=',
                infiniteScrollDisabled: '=',
                infiniteScrollUseDocumentBottom: '=',
                infiniteScrollListenForEvent: '@'
            },

            link : function(scope, elem, attrs) {
                var windowElement = angular.element($window);

                var scrollDistance = null;
                var scrollEnabled = null;
                var checkWhenEnabled = null;
                var container = null;
                var immediateCheck = true;
                var useDocumentBottom = false;
                var unregisterEventListener = null;
                var checkInterval = false;

                function height(element) {
                    var el = element[0] || element;

                    if (isNaN(el.offsetHeight)) {
                        return el.document.documentElement.clientHeight;
                    }
                    return el.offsetHeight;
                }

                function pageYOffset(element) {
                    var el = element[0] || element;

                    if (isNaN(window.pageYOffset)) {
                        return el.document.documentElement.scrollTop;
                    }
                    return el.ownerDocument.defaultView.pageYOffset;
                }

                function offsetTop(element) {
                    if (!(!element[0].getBoundingClientRect || element.css('none'))) {
                        return element[0].getBoundingClientRect().top + pageYOffset(element);
                    }
                    return undefined;
                }

                // infinite-scroll specifies a function to call when the window,
                // or some other container specified by infinite-scroll-container,
                // is scrolled within a certain range from the bottom of the
                // document. It is recommended to use infinite-scroll-disabled
                // with a boolean that is set to true when the function is
                // called in order to throttle the function call.
                function defaultHandler() {
                    var containerBottom;
                    var elementBottom;
                    if (container === windowElement) {
                        containerBottom = height(container) + pageYOffset(container[0].document.documentElement);
                        elementBottom = offsetTop(elem) + height(elem);
                    } else {
                        containerBottom = height(container);
                        var containerTopOffset = 0;
                        if (offsetTop(container) !== undefined) {
                            containerTopOffset = offsetTop(container);
                        }
                        elementBottom = (offsetTop(elem) - containerTopOffset) + height(elem);
                    }

                    if (useDocumentBottom) {
                        elementBottom = height((elem[0].ownerDocument || elem[0].document).documentElement);
                    }

                    var remaining = elementBottom - containerBottom;
                    var shouldScroll = remaining <= (height(container) * scrollDistance) + 1;

                    if (shouldScroll) {
                        checkWhenEnabled = true;

                        if (scrollEnabled) {
                            if (scope.$$phase || $rootScope.$$phase) {
                                scope.infiniteScroll();
                            } else {
                                scope.$apply(scope.infiniteScroll);
                            }
                        }
                    } else {
                        if (checkInterval) { $interval.cancel(checkInterval); }
                        checkWhenEnabled = false;
                    }
                }

                // The optional THROTTLE_MILLISECONDS configuration value specifies
                // a minimum time that should elapse between each call to the
                // handler. N.b. the first call the handler will be run
                // immediately, and the final call will always result in the
                // handler being called after the `wait` period elapses.
                // A slimmed down version of underscore's implementation.
                function throttle(func, wait) {
                    var timeout = null;
                    var previous = 0;

                    function later() {
                        previous = new Date().getTime();
                        $interval.cancel(timeout);
                        timeout = null;
                        return func.call();
                    }

                    function throttled() {
                        var now = new Date().getTime();
                        var remaining = wait - (now - previous);
                        if (remaining <= 0) {
                            $interval.cancel(timeout);
                            timeout = null;
                            previous = now;
                            func.call();
                        } else if (!timeout) {
                            timeout = $interval(later, remaining, 1);
                        }
                    }

                    return throttled;
                }

                var handler = (THROTTLE_MILLISECONDS != null) ?
                  throttle(defaultHandler, THROTTLE_MILLISECONDS) :
                  defaultHandler;

                function handleDestroy() {
                    container.unbind('scroll', handler);
                    if (unregisterEventListener != null) {
                        unregisterEventListener();
                        unregisterEventListener = null;
                    }
                    if (checkInterval) {
                        $interval.cancel(checkInterval);
                    }
                }

                scope.$on('$destroy', handleDestroy);

                // infinite-scroll-distance specifies how close to the bottom of the page
                // the window is allowed to be before we trigger a new scroll. The value
                // provided is multiplied by the container height; for example, to load
                // more when the bottom of the page is less than 3 container heights away,
                // specify a value of 3. Defaults to 0.
                function handleInfiniteScrollDistance(v) {
                    scrollDistance = parseFloat(v) || 0;
                }

                scope.$watch('infiniteScrollDistance', handleInfiniteScrollDistance);
                // If I don't explicitly call the handler here, tests fail. Don't know why yet.
                handleInfiniteScrollDistance(scope.infiniteScrollDistance);

                // infinite-scroll-disabled specifies a boolean that will keep the
                // infnite scroll function from being called; this is useful for
                // debouncing or throttling the function call. If an infinite
                // scroll is triggered but this value evaluates to true, then
                // once it switches back to false the infinite scroll function
                // will be triggered again.
                function handleInfiniteScrollDisabled(v) {
                    scrollEnabled = !v;
                    if (scrollEnabled && checkWhenEnabled) {
                        checkWhenEnabled = false;
                        handler();
                    }
                }

                scope.$watch('infiniteScrollDisabled', handleInfiniteScrollDisabled);
                // If I don't explicitly call the handler here, tests fail. Don't know why yet.
                handleInfiniteScrollDisabled(scope.infiniteScrollDisabled);

                // use the bottom of the document instead of the element's bottom.
                // This useful when the element does not have a height due to its
                // children being absolute positioned.
                function handleInfiniteScrollUseDocumentBottom(v) {
                    useDocumentBottom = v;
                }

                scope.$watch('infiniteScrollUseDocumentBottom', handleInfiniteScrollUseDocumentBottom);
                handleInfiniteScrollUseDocumentBottom(scope.infiniteScrollUseDocumentBottom);

                // infinite-scroll-container sets the container which we want to be
                // infinte scrolled, instead of the whole window. Must be an
                // Angular or jQuery element, or, if jQuery is loaded,
                // a jQuery selector as a string.
                function changeContainer(newContainer) {
                    if (container != null) {
                        container.unbind('scroll', handler);
                    }

                    container = newContainer;
                    if (newContainer != null) {
                        container.bind('scroll', handler);
                    }
                }

                changeContainer(windowElement);

                if (scope.infiniteScrollListenForEvent) {
                    unregisterEventListener = $rootScope.$on(scope.infiniteScrollListenForEvent, handler);
                }

                function handleInfiniteScrollContainer(newContainer) {
                    // TODO: For some reason newContainer is sometimes null instead
                    // of the empty array, which Angular is supposed to pass when the
                    // element is not defined
                    // (https://github.com/sroze/ngInfiniteScroll/pull/7#commitcomment-5748431).
                    // So I leave both checks.
                    if ((!(newContainer != null)) || newContainer.length === 0) {
                        return;
                    }

                    var newerContainer;

                    if (newContainer.nodeType && newContainer.nodeType === 1) {
                        newerContainer = angular.element(newContainer);
                    } else if (typeof newContainer.append === 'function') {
                        newerContainer = angular.element(newContainer[newContainer.length - 1]);
                    } else if (typeof newContainer === 'string') {
                        newerContainer = angular.element(document.querySelector(newContainer));
                    } else {
                        newerContainer = newContainer;
                    }

                    if (newerContainer == null) {
                        throw new Error('invalid infinite-scroll-container attribute.');
                    }
                    changeContainer(newerContainer);
                }

                scope.$watch('infiniteScrollContainer', handleInfiniteScrollContainer);
                handleInfiniteScrollContainer(scope.infiniteScrollContainer || []);

                // infinite-scroll-parent establishes this element's parent as the
                // container infinitely scrolled instead of the whole window.
                if (attrs.infiniteScrollParent != null) {
                    changeContainer(angular.element(elem.parent()));
                }

                // infinte-scoll-immediate-check sets whether or not run the
                // expression passed on infinite-scroll for the first time when the
                // directive first loads, before any actual scroll.
                if (attrs.infiniteScrollImmediateCheck != null) {
                    immediateCheck = scope.$eval(attrs.infiniteScrollImmediateCheck);
                }

                function intervalCheck() {
                    if (immediateCheck) {
                        handler();
                    }
                    return $interval.cancel(checkInterval);
                }

                checkInterval = $interval(intervalCheck);
                return checkInterval;
            }
        }
    }

     ])//<a ng-click="print()" class="btn btn-orange hidden-xs"><i class="fa fa-print"></i> Print</a>
        .directive('printButton', [
			'$routeParams', '$compile', '$bootbox', 'nsPinesService', 'webApiBaseUrl', '$location', '$http', 'FileSaver', function ($routeParams, $compile, $bootbox, nsPinesService, webApiBaseUrl, $location, $http, FileSaver) {


			    return {
			        restrict: 'EA',
			        scope: {
			            printLandscape: '=',
			            printMultiPage: '=',
			            fitWidth: '=',
			            fitViewerHeight: '=',
			            stretchToFit: '=',
			            htmlViewerHeight: '=',
			            htmlViewerWidth: '=',
			            sortArray: '=',
			            groupsFactory: '=',
			            forcePortraitPageSize: '=',
			            groupsFactoryArray: '=',
			            reportByNumberOfStudents: '=',
			            graphSettings: '=',
			            sourceBenchmarkDate: '=',
                        url: '='
			        },
			        link: function (scope, element, attr) {
                         
			                var templateHtml = '<a ng-click="print()" class="btn btn-orange hidden-xs"><i class="fa fa-print"></i> Print</a>';

			                scope.settings = { printInProgress: false };

                            // added on 7/20/2018 to allow dynamic URLs
			                var destinationUrl = $location.absUrl();
			                if (scope.url) {
			                    destinationUrl = scope.url;
			                }
                        
			                scope.print = function () {
			                    var sortParam = '';
			                    var groupsParam = {};
			                    var encodedGroupsParam = '';
			                    var encodedGroupsArrayParam = '';
			                    var encodedSummaryDataParam = '';
			                    var groupsArrayParam = [];
			                    var summaryDataParam = {};
			                    var encodedSourceBenchmarkDate = '';

			                    if (scope.sourceBenchmarkDate) {
			                        encodedSourceBenchmarkDate = encodeURIComponent(JSON.stringify(scope.sourceBenchmarkDate));
			                    }

			                    if (scope.graphSettings) {
			                        summaryDataParam.summaryMode = scope.graphSettings.summaryMode;
			                        summaryDataParam.summaryView = scope.graphSettings.summaryView;
			                        summaryDataParam.summaryCategory = scope.graphSettings.summaryCategory;
			                        summaryDataParam.summaryScoreGrouping = scope.graphSettings.summaryScoreGrouping;
			                        summaryDataParam.stacking = scope.graphSettings.stacking;
			                        summaryDataParam.stackingDescription = scope.graphSettings.stackingDescription;

			                        encodedSummaryDataParam = encodeURIComponent(JSON.stringify(summaryDataParam));
			                    }

			                    if (scope.groupsFactory) {
			                        groupsParam.selectedAssessmentField = scope.groupsFactory.options.selectedAssessmentField;
			                        groupsParam.selectedEducationLabels = scope.groupsFactory.options.selectedEducationLabels;
			                        groupsParam.selectedSchoolYear = scope.groupsFactory.options.selectedSchoolYear;
			                        groupsParam.selectedSchools = scope.groupsFactory.options.selectedSchools;
			                        groupsParam.selectedTeachers = scope.groupsFactory.options.selectedTeachers;
			                        groupsParam.selectedSections = scope.groupsFactory.options.selectedSections;
			                        groupsParam.selectedStudents = scope.groupsFactory.options.selectedStudents;
			                        groupsParam.selectedInterventionTypes = scope.groupsFactory.options.selectedInterventionTypes;
			                        groupsParam.selectedGrades = scope.groupsFactory.options.selectedGrades;
			                        groupsParam.selectedTestDueDate = scope.groupsFactory.options.selectedTestDueDate;
			                        groupsParam.attributeTypes = scope.groupsFactory.options.attributeTypes;
			                        //groupsParam.Fields = scope.groupsFactory.Fields;
			                        
			                        encodedGroupsParam = encodeURIComponent(JSON.stringify(groupsParam));
			                        //var len = encodedGroupsParam.length;
			                        //var unecoded = JSON.parse(decodeURIComponent(encodedGroupsParam));
			                    }

			                    if (scope.groupsFactoryArray) {
			                        for (var i = 0; i < scope.groupsFactoryArray.length; i++) {
			                            var currentGroupParam = {};

			                            currentGroupParam.DisplayName = scope.groupsFactoryArray[i].DisplayName;
			                            currentGroupParam.AccordionHeaderDisplayName = scope.groupsFactoryArray[i].AccordionHeaderDisplayName;
			                            currentGroupParam.selectedAssessmentField = scope.groupsFactoryArray[i].options.selectedAssessmentField;
			                            currentGroupParam.selectedEducationLabels = scope.groupsFactoryArray[i].options.selectedEducationLabels;
			                            currentGroupParam.selectedSchoolYear = scope.groupsFactoryArray[i].options.selectedSchoolYear;
			                            currentGroupParam.selectedSchools = scope.groupsFactoryArray[i].options.selectedSchools;
			                            currentGroupParam.selectedTeachers = scope.groupsFactoryArray[i].options.selectedTeachers;
			                            currentGroupParam.selectedSections = scope.groupsFactoryArray[i].options.selectedSections;
			                            currentGroupParam.selectedStudents = scope.groupsFactoryArray[i].options.selectedStudents;
			                            currentGroupParam.selectedInterventionTypes = scope.groupsFactoryArray[i].options.selectedInterventionTypes;
			                            currentGroupParam.selectedGrades = scope.groupsFactoryArray[i].options.selectedGrades;
			                            currentGroupParam.selectedTestDueDate = scope.groupsFactoryArray[i].options.selectedTestDueDate;
			                            currentGroupParam.attributeTypes = scope.groupsFactoryArray[i].options.attributeTypes;
			                            //currentGroupParam.Fields = scope.groupsFactoryArray[i].Fields;
			                            groupsArrayParam.push(currentGroupParam);
			                        }
			                        encodedGroupsArrayParam = encodeURIComponent(JSON.stringify(groupsArrayParam));
			                    }

			                    // serialize sortArray
			                    if (scope.sortArray) {
			                        sortParam = scope.sortArray.join(',');
			                    }

			                    
			                    if (scope.settings.printInProgress) {
			                        $bootbox.alert("Please wait... another print job is already in progress.");
			                        return;
			                    }
                                 
			                    scope.settings.printInProgress = true;
			                    var notice = nsPinesService.startDynamic();

			                    var returnObj = {
			                        PrintLandscape: scope.printLandscape,
			                        PrintMultiPage: scope.printMultiPage,
			                        StretchToFit: scope.stretchToFit,
			                        FitHeight: scope.fitViewerHeight,
			                        FitWidth: scope.fitWidth,
			                        HtmlViewerHeight: scope.htmlViewerHeight,
			                        HtmlViewerWidth: scope.htmlViewerWidth,
			                        SortParam: sortParam,
			                        GroupsParam: encodedGroupsParam,
			                        ForcePortraitPageSize: scope.forcePortraitPageSize,
			                        ReportByNumberOfStudents: scope.reportByNumberOfStudents,
			                        SummaryDataParam: encodedSummaryDataParam,
			                        GroupsArrayParam: encodedGroupsArrayParam,
			                        SourceBenchmarkDate: encodedSourceBenchmarkDate,
			                        //SchoolId: $scope.filterOptions.selectedSchool.id,
			                        //GradeId: $scope.filterOptions.selectedGrade.id,
			                        //TeacherId: $scope.filterOptions.selectedTeacher.id,
			                        //SectionId: $scope.filterOptions.selectedSection.id,
			                        //StudentId: $scope.filterOptions.selectedSectionStudent.id,
			                        //SchoolYear: $scope.filterOptions.selectedSchoolYear.id,
			                        Url: destinationUrl
			                    };

			                    var printMethod = 'PrintPage';
			                    if (webApiBaseUrl.indexOf('localhost') > 0 || webApiBaseUrl.indexOf('192.168') > 0) {
			                        printMethod = 'PrintPageLocal';  
			                    }

			                    $http.post(webApiBaseUrl + '/api/Print/' + printMethod, returnObj, {
			                        responseType: 'arraybuffer', headers: {
			                            accept: 'application/pdf'
			                        },
			                    })
                                    .then(function (data) {
                                        var blob = new Blob([data.data], { type: 'application/pdf' });
                                        FileSaver.saveAs(blob, "NorthStarPrint.pdf");
                                    })
                                .finally(function () {
                                    scope.settings.printInProgress = false;
                                    nsPinesService.endDynamic(notice);
                                });
			                }

			                element.html(templateHtml);
			                $compile(element.contents())(scope);
			        }
			    };
			}
        ])
    .directive('printSplitButton', [
			'$routeParams', '$compile', '$bootbox', 'nsPinesService', 'webApiBaseUrl', '$location', '$http', 'FileSaver', function ($routeParams, $compile, $bootbox, nsPinesService, webApiBaseUrl, $location, $http, FileSaver) {


			    return {
			        restrict: 'EA',
			        scope: {
			            printLandscape: '=',
			            printMultiPage: '=',
			            fitWidth: '=',
			            fitViewerHeight: '=',
			            stretchToFit: '=',
			            htmlViewerHeight: '=',
			            htmlViewerWidth: '=',
			            forcePortraitPageSize: '=',
			            targetPages: '=',
                        student: '='
			        },
			        link: function (scope, element, attr) {
			            scope.$watch('student', function (newVal, oldVal) {
			                if (newVal) {
			                    var templateHtml = '';
			                    // need to watch student??
			                    if (scope.targetPages) {
			                        for (var i = 0; i < scope.targetPages.length; i++) {
			                            if (i == 0) {
			                                templateHtml += '<div class="btn-group" uib-dropdown><a ng-click="print(\'' + scope.targetPages[i].url + '/' + scope.student.id + '/' + scope.targetPages[i].tab + '\')" class="btn btn-orange"><i class="fa fa-print"></i> ' + scope.targetPages[i].label + '</a>';
			                                templateHtml += ' <a href="#" class="btn btn-orange-alt dropdown-toggle" uib-dropdown-toggle><span class="caret"></span></a>';
			                                templateHtml += '<ul class="dropdown-menu" uib-dropdown-menu role="menu">';
			                            } else {
			                                templateHtml += '<li><a ng-click="print(\'' + scope.targetPages[i].url + '/' + scope.student.id + '/' + scope.targetPages[i].tab + '\')"><i class="fa fa-print"></i> ' + scope.targetPages[i].label + '</a></li>';
			                            }
			                        }

			                        // close it up
			                        templateHtml += '</ul></div>';
			                    }

			                    scope.settings = { printInProgress: false };

			                    scope.print = function (url) {

			                        if (scope.settings.printInProgress) {
			                            $bootbox.alert("Please wait... another print job is already in progress.");
			                            return;
			                        }

			                        scope.settings.printInProgress = true;
			                        var notice = nsPinesService.startDynamic();

			                        var returnObj = {
			                            PrintLandscape: scope.printLandscape,
			                            PrintMultiPage: scope.printMultiPage,
			                            StretchToFit: scope.stretchToFit,
			                            FitHeight: scope.fitViewerHeight,
			                            FitWidth: scope.fitWidth,
			                            HtmlViewerHeight: scope.htmlViewerHeight,
			                            HtmlViewerWidth: scope.htmlViewerWidth,
			                            ForcePortraitPageSize: scope.forcePortraitPageSize,
			                            Url: url
			                        };

			                        var printMethod = 'PrintPage';
			                        if (webApiBaseUrl.indexOf('localhost') > 0 || webApiBaseUrl.indexOf('192.168.') > 0) {
			                            printMethod = 'PrintPageLocal';
			                        }

			                        $http.post(webApiBaseUrl + '/api/Print/' + printMethod, returnObj, {
			                            responseType: 'arraybuffer', headers: {
			                                accept: 'application/pdf'
			                            },
			                        })
                                        .then(function (data) {
                                            var blob = new Blob([data.data], { type: 'application/pdf' });
                                            FileSaver.saveAs(blob, "NorthStarPrint.pdf");
                                        })
                                    .finally(function () {
                                        scope.settings.printInProgress = false;
                                        nsPinesService.endDynamic(notice);
                                    });
			                    }

			                    element.html(templateHtml);
			                    $compile(element.contents())(scope);
			                } else {
			                    templateHtml = '';
			                    element.html(templateHtml);
			                    $compile(element.contents())(scope);
			                }
			            }, true);
			        }
			    };
			}
    ])
    .directive('printTargetButton', [
			'$routeParams', '$compile', '$bootbox', 'nsPinesService', 'webApiBaseUrl', '$location', '$http', 'FileSaver', function ($routeParams, $compile, $bootbox, nsPinesService, webApiBaseUrl, $location, $http, FileSaver) {


			    return {
			        restrict: 'EA',
			        scope: {
			            printLandscape: '=',
			            printMultiPage: '=',
			            fitWidth: '=',
			            fitViewerHeight: '=',
			            stretchToFit: '=',
			            htmlViewerHeight: '=',
			            htmlViewerWidth: '=',
			            forcePortraitPageSize: '=',
			            targetPages: '=',
			            student: '=',
			            year: '=',
			            school: '=',
			            teacher: '=',
			            interventionGroup: '=',
                        stint: '='
			        },
			        link: function (scope, element, attr) {
			            scope.$watch('stint', function (newVal, oldVal) {
			                if (newVal) {
			                    var templateHtml = ''; 
			                    // need to watch student??
			                    if (scope.targetPages) {
			                        for (var i = 0; i < scope.targetPages.length; i++) {
			                            if (i == 0) {
			                                templateHtml = '<a ng-click="print(\'' + scope.targetPages[i].url + '/' + scope.year.id + '/' + scope.school.id + '/' + scope.teacher.id + '/' + scope.interventionGroup.id + '/' + scope.student.id + '/' + scope.stint.id + '\')" class="btn btn-orange"><i class="fa fa-print"></i> ' + scope.targetPages[i].label + '</a>';
			                            } 
			                        }
			                    }

			                    scope.settings = { printInProgress: false };

			                    scope.print = function (url) {

			                        if (scope.settings.printInProgress) {
			                            $bootbox.alert("Please wait... another print job is already in progress.");
			                            return;
			                        }

			                        scope.settings.printInProgress = true;
			                        var notice = nsPinesService.startDynamic();

			                        var returnObj = {
			                            PrintLandscape: scope.printLandscape,
			                            PrintMultiPage: scope.printMultiPage,
			                            StretchToFit: scope.stretchToFit,
			                            FitHeight: scope.fitViewerHeight,
			                            FitWidth: scope.fitWidth,
			                            HtmlViewerHeight: scope.htmlViewerHeight,
			                            HtmlViewerWidth: scope.htmlViewerWidth,
			                            ForcePortraitPageSize: scope.forcePortraitPageSize,
			                            Url: url
			                        };

			                        var printMethod = 'PrintPage';
			                        if (webApiBaseUrl.indexOf('localhost') > 0 || webApiBaseUrl.indexOf('192.168.') > 0) {
			                            printMethod = 'PrintPageLocal';
			                        }

			                        $http.post(webApiBaseUrl + '/api/Print/' + printMethod, returnObj, {
			                            responseType: 'arraybuffer', headers: {
			                                accept: 'application/pdf'
			                            },
			                        })
                                        .then(function (data) {
                                            var blob = new Blob([data.data], { type: 'application/pdf' });
                                            FileSaver.saveAs(blob, "NorthStarPrint.pdf");
                                        })
                                    .finally(function () {
                                        scope.settings.printInProgress = false;
                                        nsPinesService.endDynamic(notice);
                                    });
			                    }

			                    element.html(templateHtml);
			                    $compile(element.contents())(scope);
			                } else {
			                    templateHtml = '';
			                    element.html(templateHtml);
			                    $compile(element.contents())(scope);
			                }
			            }, true);
			        }
			    };
			}
    ])
    //.directive('commentPopup', ['$compile', '$uibModal', function ($compile, $uibModal) {

    //    return {
    //        restrict: 'AE',
    //        scope: {
    //            text: '='
    //        },
    //        link: function (scope, element, attr) {
    //            var buttonTemplate = '<i class="fa fa-comments" style="margin-left:5px;cursor:pointer" ng-click="openCommentModal();"></i>';

    //            if (angular.isDefined(scope.text) && scope.text != '' && scope.text != null) {
    //                element.html(buttonTemplate);
    //                $compile(element.contents())(scope);
    //            }

    //            scope.openCommentModal = function () {
    //                var returnString = '';

    //                var modalInstance = $uibModal.open({
    //                    templateUrl: 'genericModal.html',
    //                    scope: scope,
    //                    controller: function ($scope, $uibModalInstance) {
    //                        $scope.heading = 'Comments';
    //                        $scope.body = $scope.text;
    //                        $scope.cancel = function () {
    //                            $uibModalInstance.dismiss('cancel');
    //                        };
    //                    },
    //                    size: 'lg',
    //                });

    //            }
    //        }
    //    }
    //}])
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

