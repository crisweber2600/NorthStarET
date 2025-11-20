(function () {
'use strict'

angular
  .module('theme.form-directives', [])
  .directive('autosize', function () {
    return {
      restrict: 'AC',
      link: function (scope, element, attr) {
        element.autosize({append: "\n"})
      }
    }
  })
  .directive('fullscreen', function () {
    return {
      restrict: 'AC',
      link: function (scope, element, attr) {
        element.fseditor({maxHeight: 500});
      }
    }
  })
  .directive('colorpicker', function () {
    return {
      restrict: 'AC',
      link: function (scope, element, attr) {
        element.colorpicker();
      }
    }
  })
  .directive('daterangepicker', function () {
    return {
      restrict: 'A',
      scope: {
        options: '=daterangepicker',
        start: '=dateBegin',
        end: '=dateEnd'
      },
      link: function (scope, element, attr) {
        element.daterangepicker(scope.options, function (start, end) {
          if (scope.start) scope.start = start.format('MMMM D, YYYY');
          if (scope.end) scope.end = end.format('MMMM D, YYYY');
          scope.$apply();
        });
        // THIS IS NEW... SH on 7/21/2015
        var makeMonday = function(theDate) {
            return moment(theDate).startOf('isoweek');
        };
        // SO IS THIS
        scope.$watch('start', function(newVal, oldVal) {
			if (newVal !== oldVal) {
                var newDate = makeMonday(scope.start);
                element.data('daterangepicker').setStartDate(moment(newDate).format('MM/DD/YYYY'));
                element.data('daterangepicker').setEndDate(moment(newDate).add('days', 4).format('MM/DD/YYYY'));
            };
		});
      }
    }
  })
  .directive('multiselect', ['$timeout', function ($t) {
    return {
      restrict: 'A',
      link: function (scope, element, attr) {
        $t( function () {
          element.multiSelect();
        });
      }
    }
  }])
  .directive('wizard', function () {
    return {
      restrict: 'A',
      scope: {
        options: '=wizard'
      },
      link: function (scope, element, attr) {
          if (scope.options) {        
            element.stepy(scope.options);

            //Make Validation Compability - see docs
            if (scope.options.validate == true)
              element.validate({
                  errorClass: "help-block",
                  validClass: "help-block",
                  highlight: function(element, errorClass,validClass) {
                     $(element).closest('.form-group').addClass("has-error");
                  },
                  unhighlight: function(element, errorClass,validClass) {
                      $(element).closest('.form-group').removeClass("has-error");
                  }
               });
          } else {
            element.stepy();
          }
          //Add Wizard Compability - see docs
          element.find('.stepy-navigator').wrapInner('<div class="pull-right"></div>');
      }
    }
  })
  .directive('maskinput', function () {
    return {
      restrict: 'A',
      link: function (scope, element, attr) {
        element.inputmask();
      }
    }
  })
      .directive('wysiwygCkeditor', ['$timeout', function ($timeout) {
          return {
              restrict: 'A',
              scope: {
                  options: '=wysiwygCkeditor'
              },
              link: function (scope, element, attr) {

                  $timeout(function (){
                  if (scope.options && scope.options.inline == true)
                      return CKEDITOR.inline(attr.name || attr.id, scope.options);

                  CKEDITOR.replace(attr.name || attr.id, scope.options);

              }); 
              }
          }
      }])
.directive('ckeditor', ['$parse', ckeditorDirective]);

// Polyfill setImmediate function.
var setImmediate = window && window.setImmediate ? window.setImmediate : function (fn) {
    setTimeout(fn, 0);
};

/**
 * CKEditor directive.
 *
 * @example
 * <div ckeditor="options" ng-model="content" ready="onReady()"></div>
 */

function ckeditorDirective($parse) {
    return {
        restrict: 'A',
        require: ['ckeditor', 'ngModel'],
        controller: [
          '$scope',
          '$element',
          '$attrs',
          '$parse',
          '$q',
          ckeditorController
        ],
        link: function (scope, element, attrs, ctrls) {
            // get needed controllers
            var controller = ctrls[0]; // our own, see below
            var ngModelController = ctrls[1];

            // Initialize the editor content when it is ready.
            controller.ready().then(function initialize() {
                // Sync view on specific events.
                ['dataReady', 'change', 'blur', 'saveSnapshot'].forEach(function (event) {
                    controller.onCKEvent(event, function syncView() {
                        ngModelController.$setViewValue(controller.instance.getData() || '');
                    });
                });

                controller.instance.setReadOnly(!!attrs.readonly);
                attrs.$observe('readonly', function (readonly) {
                    controller.instance.setReadOnly(!!readonly);
                });

                // Defer the ready handler calling to ensure that the editor is
                // completely ready and populated with data.
                setImmediate(function () {
                    $parse(attrs.ready)(scope);
                });
            });

            // Set editor data when view data change.
            ngModelController.$render = function syncEditor() {
                controller.ready().then(function () {
                    // "noSnapshot" prevent recording an undo snapshot
                    controller.instance.setData(ngModelController.$viewValue || '', {
                        noSnapshot: true,
                        callback: function () {
                            // Amends the top of the undo stack with the current DOM changes
                            // ie: merge snapshot with the first empty one
                            // http://docs.ckeditor.com/#!/api/CKEDITOR.editor-event-updateSnapshot
                            controller.instance.fire('updateSnapshot');
                        }
                    });
                });
            };
        }
    };
}

/**
 * CKEditor controller.
 */

function ckeditorController($scope, $element, $attrs, $parse, $q) {
    var config = $parse($attrs.ckeditor)($scope) || {};
    var editorElement = $element[0];
    var instance;
    var readyDeferred = $q.defer(); // a deferred to be resolved when the editor is ready
    var inlineEdit = false;
    var inlineEditDefined = false;

    if (editorElement.hasAttribute('inlineedit')) {
        inlineEditDefined = true;
        inlineEdit = $parse($attrs.inlineedit)($scope)
    }



    // Create editor instance.
    if ((editorElement.hasAttribute('contenteditable') &&
        editorElement.getAttribute('contenteditable').toLowerCase() == 'true') || inlineEdit == true || inlineEditDefined) {
        instance = this.instance = CKEDITOR.inline(editorElement, config);
    }
    else {
        instance = this.instance = CKEDITOR.replace(editorElement, config);
    }

    if (angular.isDefined($scope.note)) {
        $scope.$watch('note.IsEditing', function (newVal, oldVal) {
            if (newVal !== oldVal) {
                instance.setReadOnly(!newVal);
            }
        });
    }

    /**
     * Listen on events of a given type.
     * This make all event asynchronous and wrapped in $scope.$apply.
     *
     * @param {String} event
     * @param {Function} listener
     * @returns {Function} Deregistration function for this listener.
     */

    this.onCKEvent = function (event, listener) {
        instance.on(event, asyncListener);

        function asyncListener() {
            var args = arguments;
            setImmediate(function () {
                applyListener.apply(null, args);
            });
        }

        function applyListener() {
            var args = arguments;
            $scope.$apply(function () {
                listener.apply(null, args);
            });
        }

        // Return the deregistration function
        return function $off() {
            instance.removeListener(event, applyListener);
        };
    };

    this.onCKEvent('instanceReady', function () {
        readyDeferred.resolve(true);
    });

    /**
     * Check if the editor if ready.
     *
     * @returns {Promise}
     */
    this.ready = function ready() {
        return readyDeferred.promise;
    };

    // Destroy editor when the scope is destroyed.
    $scope.$on('$destroy', function onDestroy() {
        // do not delete too fast or pending events will throw errors
        readyDeferred.promise.then(function () {
            instance.destroy(false);
        });
    });
}
  //.directive('wysiwygCkeditor', ['$sce', '$timeout', function ($sce, $timeout) {
  //    return {
  //        restrict: 'A',
  //        scope: {
  //            value: "=ngBindHtml",
  //            options: '=wysiwygCkeditor'
  //        },
  //        link: function (scope, elm, attr, ngBindHtml) {


  //            $timeout(function()
  //            {
  //                var ck_inline = null;
  //                var ck_textarea = null;


  //                if (scope.options && scope.options.inline == true)
  //                    ck_inline = CKEDITOR.inline(attr.name || attr.id, scope.options);

  //                if (ck_inline == null) {
  //                    ck_textarea = CKEDITOR.replace(attr.name || attr.id, scope.options);
  //                   // elm.attr("contenteditable", "true");
  //                }
                  
  //                //CKEDITOR.disableAutoInline = true;
  //                //ck_inline = CKEDITOR.inline(elm[0]);


  //                //if (attr.ngBindHtml && ck_inline != null) {
  //                //    ck_inline.on('instanceReady', function () {
  //                //        ck_inline.setData(elm.html());
  //                //    });
                      
  //                //    function updateHtml() {
  //                //         scope.$apply(function () {
  //                //        scope.value = ck_inline.getData();
  //                //        scope.value = $sce.trustAsHtml(ck_inline.getData());
  //                //          });
  //                //    };

  //                //    ck_inline.on('blur', updateHtml);
  //                //    ck_inline.on('dataReady', updateHtml);
  //                //}
  //            }); 
  //            //if (scope.options && scope.options.inline == true)
  //            //    return CKEDITOR.inline(attr.name || attr.id, scope.options);

              
  //        }
  //    }
  //}])
;

})();