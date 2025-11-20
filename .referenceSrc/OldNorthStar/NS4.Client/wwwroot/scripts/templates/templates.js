angular.module('theme.templates', []).run(['$templateCache', function ($templateCache) {
  'use strict';

  $templateCache.put('templates/assessment-calculatedfield-grideditable.html',
    "<span ng-bind=\"Calculate(result.Field.CalculationFields)\"></span>"
  );


  $templateCache.put('templates/assessment-calculatedfield-readonly.html',
    "<span ng-bind=\"Calculate(result.Field.CalculationFields)\"></span>"
  );


  $templateCache.put('templates/assessment-calculatedfield.html',
    "<span ng-bind=\"Calculate(result.Field.CalculationFields)\"></span>"
  );


  $templateCache.put('templates/assessment-calculatedfielddbbacked-custom.html',
    "<span ng-bind=\"Calculate(currentField.CalculationFields)\"></span>"
  );


  $templateCache.put('templates/assessment-calculatedfielddbbacked-grideditable.html',
    "<span>{{::result.IntValue}}</span>"
  );


  $templateCache.put('templates/assessment-calculatedfielddbbacked-readonly.html',
    "<span ng-bind=\"result.IntValue\"></span>"
  );


  $templateCache.put('templates/assessment-calculatedfielddbbacked.html',
    "<span>{{::result.IntValue}}</span>"
  );


  $templateCache.put('templates/assessment-calculatedfielddbbackedstring-custom.html',
    "<span ng-bind-html=\"Calculate(currentField.CalculationFields) | safe_html\"></span>"
  );


  $templateCache.put('templates/assessment-calculatedfielddbbackedstring-grideditable.html',
    "<span ng-bind-html=\"result.StringValue | safe_html\"></span>"
  );


  $templateCache.put('templates/assessment-calculatedfielddbbackedstring-readonly.html',
    "<span ng-bind=\"result.StringValue\"></span>"
  );


  $templateCache.put('templates/assessment-calculatedfielddbbackedstring.html',
    "<span ng-bind=\"result.StringValue\"></span>"
  );


  $templateCache.put('templates/assessment-calculatedfielddbonly-grideditable.html',
    "<span ng-bind=\"result.StringValue\"></span>"
  );


  $templateCache.put('templates/assessment-calculatedfielddbonly-readonly.html',
    "<span ng-bind=\"result.StringValue\"></span>"
  );


  $templateCache.put('templates/assessment-calculatedfielddbonly.html',
    "<span ng-bind=\"result.StringValue\"></span>"
  );


  $templateCache.put('templates/assessment-checkbox-custom.html',
    "<table>\r" +
    "\n" +
    "    <tr>\r" +
    "\n" +
    "        <td><switch class=\"green\" ng-change=\"checkClick(fieldResult.BoolValue, currentField.CalculationFields)\" ng-model=\"fieldResult.BoolValue\"></switch></td>\r" +
    "\n" +
    "        <td class=\"switchLabel\">{{::currentField.DisplayLabel}}</td>\r" +
    "\n" +
    "    </tr>\r" +
    "\n" +
    "</table>\r" +
    "\n"
  );


  $templateCache.put('templates/assessment-checkboxvertical-custom.html',
    "<table>\r" +
    "\n" +
    "    <tr>\r" +
    "\n" +
    "        <td class=\"verticalSwitchLabel\">{{::currentField.DisplayLabel}}</td>\r" +
    "\n" +
    "    </tr>\r" +
    "\n" +
    "    <tr>\r" +
    "\n" +
    "        <td><switch class=\"green\" ng-change=\"checkClick(fieldResult.BoolValue, currentField.CalculationFields)\" ng-model=\"fieldResult.BoolValue\"></switch></td>\r" +
    "\n" +
    "    </tr>\r" +
    "\n" +
    "</table>\r" +
    "\n"
  );


  $templateCache.put('templates/assessment-datecheckbox-grideditable.html',
    "<manualswitch class=\"green\" ng-mousedown=\"checkClick($event);\" ng-model=\"fieldResult.BoolValue\"></manualswitch><span ng-bind=\"fieldResult.DateValue\"></span>"
  );


  $templateCache.put('templates/assessment-datecheckbox-readonly.html',
    "<!DOCTYPE html>\r" +
    "\n" +
    "<html>\r" +
    "\n" +
    "<head>\r" +
    "\n" +
    "    <meta charset=\"utf-8\" />\r" +
    "\n" +
    "    <title></title>\r" +
    "\n" +
    "</head>\r" +
    "\n" +
    "<body>\r" +
    "\n" +
    "\r" +
    "\n" +
    "</body>\r" +
    "\n" +
    "</html>"
  );


  $templateCache.put('templates/assessment-datecheckbox.html',
    "<table>\r" +
    "\n" +
    "    <tr>\r" +
    "\n" +
    "        <td class=\"dateCheckboxSwitchCell\">\r" +
    "\n" +
    "            <manualswitch class=\"green\" ng-mousedown=\"checkClick($event);\" ng-model=\"fieldResult.BoolValue\"></manualswitch>\r" +
    "\n" +
    "        </td>\r" +
    "\n" +
    "        <td class=\"dateCheckboxDateCell\">\r" +
    "\n" +
    "            <span>{{formattedDate(fieldResult.DateValue)}}</span>\r" +
    "\n" +
    "        </td>\r" +
    "\n" +
    "    </tr>\r" +
    "\n" +
    "</table>\r" +
    "\n"
  );


  $templateCache.put('templates/assessment-decimalrange-grideditable.html',
    "<span ng-show=\"result.DecimalValue\">{{result.Field.AltDisplayLabel}}</span><span e-step=\"any\" e-form=\"eForm\" editable-number=\"result.DecimalValue\"  e-min=\"{{result.Field.RangeLow}}\" e-max=\"{{result.Field.RangeHigh}}\" >{{ result.DecimalValue || '' }}</span>"
  );


  $templateCache.put('templates/assessment-decimalrange-readonly.html',
    "<span>{{ result.DecimalValue || '' }}</span>"
  );


  $templateCache.put('templates/assessment-decimalrange.html',
    "<span e-step=\"any\" e-form=\"eForm\" editable-number=\"result.DecimalValue\"  e-min=\"{{result.Field.RangeLow}}\" e-max=\"{{result.Field.RangeHigh}}\" >{{ result.DecimalValue || '' }}</span>"
  );


  $templateCache.put('templates/assessment-dropdownfromdb-custom.html',
    "<select name=\"{{fieldName}}\" ng-required=\"fieldRequired\" class=\"form-control\" ng-disabled=\"disabled\" ng-options=\"g.FieldSpecificId as g.FieldValue for g in lookupValues\" ng-model=\"fieldResult.IntValue\"></select>"
  );


  $templateCache.put('templates/assessment-dropdownfromdb-grideditable.html',
    "<span editable-select=\"result.IntValue\" onbeforesave=\"validateIfRequired($data)\" e-ng-model=\"result.IntValue\" e-ng-required=\"fieldRequired\" e-form=\"eForm\" e-ng-options=\"g.FieldSpecificId as g.FieldValue for g in lookupValues\">{{ result.DisplayValue || '' }}</span>"
  );


  $templateCache.put('templates/assessment-dropdownfromdb-readonly.html',
    "<span>{{ result.DisplayValue || '' }}</span>"
  );


  $templateCache.put('templates/assessment-dropdownfromdb.html',
    "<span editable-select=\"result.IntValue\" e-form=\"eForm\" e-ng-options=\"g.FieldSpecificId as g.FieldValue for g in lookupValues\">{{ result.DisplayValue || '' }}</span>"
  );


  $templateCache.put('templates/assessment-dropdownrange-grideditable.html',
    "<span editable-select=\"result.IntValue\" e-form=\"eForm\" e-ng-options=\"n for n in [] | range:result.Field.RangeLow:result.Field.RangeHigh\">{{ result.IntValue || '' }}</span>"
  );


  $templateCache.put('templates/assessment-dropdownrange-readonly.html',
    "<span>{{ result.IntValue || '' }}</span>"
  );


  $templateCache.put('templates/assessment-dropdownrange.html',
    "<span editable-select=\"result.Score\" e-form=\"eForm\" e-ng-options=\"n for n in [] | range:field.RangeLow:field.RangeHigh\">{{ result.Score || '' }}</span>"
  );


  $templateCache.put('templates/assessment-field.html',
    "<!DOCTYPE html>\r" +
    "\n" +
    "\r" +
    "\n" +
    "<html>\r" +
    "\n" +
    "<head>\r" +
    "\n" +
    "    <meta charset=\"utf-8\" />\r" +
    "\n" +
    "    <title></title>\r" +
    "\n" +
    "</head>\r" +
    "\n" +
    "<body>\r" +
    "\n" +
    "\r" +
    "\n" +
    "</body>\r" +
    "\n" +
    "</html>"
  );


  $templateCache.put('templates/assessment-integer-readonly.html',
    "<span>{{ result.IntValue || '' }}</span>"
  );


  $templateCache.put('templates/assessment-integer.html',
    "<span editable-number=\"result.IntValue\" e-form=\"eForm\" e-min=\"{{result.Field.RangeLow}}\" e-max=\"{{result.Field.RangeHigh}}\">{{ result.IntValue || '' }}</span>"
  );


  $templateCache.put('templates/assessment-label-custom.html',
    "<span ng-bind-html=\"currentField.DisplayLabel | safe_html\"></span>"
  );


  $templateCache.put('templates/assessment-label-grideditable.html',
    "<span>{{::currentField.DisplayLabel}}</span>"
  );


  $templateCache.put('templates/assessment-label-readonly.html',
    "<span>{{::currentField.DisplayLabel}}</span>"
  );


  $templateCache.put('templates/assessment-preview.html',
    "<h2>{{ assessment.AssessmentName }}</h2>\r" +
    "\n" +
    "\r" +
    "\n" +
    "<div >\r" +
    "\n" +
    "\t<div class=\"field row\">\r" +
    "\n" +
    "\t\t<div class=\"span2\">Assessment ID:</div>\r" +
    "\n" +
    "\t\t<div class=\"span4\">{{ assessment.Id }}</div>\r" +
    "\n" +
    "\t</div>\r" +
    "\n" +
    "\t\r" +
    "\n" +
    "\t\t<div ng-repeat=\"field in assessment.Fields\">\r" +
    "\n" +
    "\t\t\t<assessment-field field=\"field\" all-fields=\"assessment.Fields\"></assessment-field>\r" +
    "\n" +
    "\t\t</div>\r" +
    "\n" +
    "\t\r" +
    "\n" +
    "\r" +
    "\n" +
    "\t<div class=\"form-actions\">\r" +
    "\n" +
    "\t\t<p class=\"text-center\">\r" +
    "\n" +
    "\t\t\t<button class=\"btn btn-success right\" type=\"button\" ng-disabled=\"!myForm.$valid\" ng-click=\"submit()\"><i class=\"icon-edit icon-white\"></i> Submit Form</button>\r" +
    "\n" +
    "\t\t\t<button class=\"btn btn-danger right\" type=\"button\" ng-click=\"cancel()\"><i class=\"icon-remove icon-white\"></i> Cancel</button>\r" +
    "\n" +
    "\t\t</p>\r" +
    "\n" +
    "\t</div>\r" +
    "\n" +
    "</div>\r" +
    "\n" +
    "<div >\r" +
    "\n" +
    "\t<h3>Submitted Data</h3>\r" +
    "\n" +
    "\t<div b=\"field in form.form_fields\">\r" +
    "\n" +
    "\t\tField Title:  field.field_title  <br>\r" +
    "\n" +
    "\t\tField Value:  field.field_value  <br><br>\r" +
    "\n" +
    "\t</div>\r" +
    "\n" +
    "</div>\r" +
    "\n"
  );


  $templateCache.put('templates/assessment-textarea-custom.html',
    "<textarea name=\"{{fieldName}}\" ng-required=\"fieldRequired\" class=\"form-control\" ng-model=\"fieldResult.StringValue\"></textarea>"
  );


  $templateCache.put('templates/assessment-textarea-grideditable.html',
    "<span editable-textarea=\"result.StringValue\" e-form=\"eForm\" e-rows=\"5\" e-cols=\"80\"><i ng-if=\"result.StringValue\" popover-trigger=\"mouseenter\" class=\"fa fa-comments\" style=\"margin-left:5px;cursor:pointer\" uib-popover-html=\"toolTipFunction()\" popover-title=\"Comments\"></i></span>"
  );


  $templateCache.put('templates/assessment-textfield-custom.html',
    "<input type=\"text\" class=\"form-control\" ng-model=\"fieldResult.StringValue\" />"
  );


  $templateCache.put('templates/assessment-textfield-grideditable.html',
    "<span editable-text=\"result.StringValue\" e-form=\"eForm\" >{{ result.StringValue || '' }}</span>"
  );


  $templateCache.put('templates/assessment-textfield-readonly.html',
    "<span>{{ result.StringValue || '' }}</span>"
  );


  $templateCache.put('templates/assessment-textfield.html',
    "<span editable-text=\"result.StringValue\" e-form=\"eForm\" >{{ result.StringValue || '' }}</span>"
  );


  $templateCache.put('templates/benchmark-decimalrange.html',
    "<span e-step=\"any\" e-form=\"eForm\" editable-number=\"result.Score\" e-min=\"{{field.RangeLow}}\" e-max=\"{{field.RangeHigh}}\">{{ result.Score || '' }}</span>"
  );


  $templateCache.put('templates/benchmark-dropdownfromdb.html',
    "<span editable-select=\"result.Score\" e-form=\"eForm\" e-ng-options=\"g.FieldSpecificId as g.FieldValue for g in lookupValues\">{{ result.ScoreLabel || '' }}</span>"
  );


  $templateCache.put('templates/benchmark-dropdownrange.html',
    "<span editable-select=\"result.Score\" e-form=\"eForm\" e-ng-options=\"n for n in [] | range:field.RangeLow:field.RangeHigh\">{{ result.Score || '' }}</span>"
  );


  $templateCache.put('templates/bs-modal.html',
    "<div class=\"modal-header\">\n" +
    "    <h3 class=\"modal-title\">I'm a modal!</h3>\n" +
    "</div>\n" +
    "<div class=\"modal-body\">\n" +
    "    <ul>\n" +
    "        <li ng-repeat=\"item in items\">\n" +
    "            <a ng-click=\"selected.item = item\">{{ item }}</a>\n" +
    "        </li>\n" +
    "    </ul>\n" +
    "    Selected: <b>{{ selected.item }}</b>\n" +
    "</div>\n" +
    "<div class=\"modal-footer\">\n" +
    "    <button class=\"btn btn-primary\" ng-click=\"ok()\">OK</button>\n" +
    "    <button class=\"btn btn-warning\" ng-click=\"cancel()\">Cancel</button>\n" +
    "</div>\n"
  );


  $templateCache.put('templates/contextual-progressbar.html',
    "<div class=\"contextual-progress\">\n" +
    "\t<div class=\"clearfix\">\n" +
    "\t\t<div class=\"progress-title\">{{heading}}</div>\n" +
    "\t\t<div class=\"progress-percentage\">{{percent | number:0}}%</div>\n" +
    "\t</div>\n" +
    "\t<div class=\"progress\">\n" +
    "\t\t<div class=\"progress-bar\" ng-class=\"type && 'progress-bar-' + type\" role=\"progressbar\" aria-valuenow=\"{{value}}\" aria-valuemin=\"0\" aria-valuemax=\"{{max}}\" ng-style=\"{width: percent + '%'}\" aria-valuetext=\"{{percent | number:0}}%\" ng-transclude></div>\n" +
    "\t</div>\n" +
    "</div>\n"
  );


  $templateCache.put('templates/global-filter-options.html',
    "<span ng-if=\"!verticalMode\">\r" +
    "\n" +
    "    <panel panel-class=\"panel-midnightblue\" heading=\"Filters / Options\" panel-icon=\"fa fa-filter\">\r" +
    "\n" +
    "        <div class=\"tiles-heading\">\r" +
    "\n" +
    "            <div class=\"row\">\r" +
    "\n" +
    "                <!--<div class=\"col-md-1\">\r" +
    "\n" +
    "                    <span style=\"text-transform: Uppercase; font-weight: bold; margin-right: 10px;\">Filters:</span>\r" +
    "\n" +
    "                </div>-->\r" +
    "\n" +
    "                <div class=\"col-md-1\" ng-show=\"schoolYearEnabled\">\r" +
    "\n" +
    "                    <select class=\"form-control\" ng-options=\"item.text for item in filterOptions.schoolYears\" ng-model=\"filterOptions.selectedSchoolYear\" ng-change=\"changeSchoolYear()\">\r" +
    "\n" +
    "                        <option value=\"\">-school year-</option>\r" +
    "\n" +
    "                    </select>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"col-md-1\" ng-show=\"benchmarkDateEnabled\">\r" +
    "\n" +
    "                    <select class=\"form-control\" ng-options=\"item.text for item in filterOptions.benchmarkDates\" ng-model=\"filterOptions.selectedBenchmarkDate\">\r" +
    "\n" +
    "                        <option value=\"\">-benchmark date-</option>\r" +
    "\n" +
    "                    </select>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"col-md-2\" ng-show=\"schoolEnabled\">\r" +
    "\n" +
    "                    <select class=\"form-control\" ng-options=\"item.text for item in filterOptions.schools\" ng-model=\"filterOptions.selectedSchool\" ng-change=\"changeSchool()\">\r" +
    "\n" +
    "                        <option value=\"\">-school-</option>\r" +
    "\n" +
    "                    </select>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"col-md-4\" ng-show=\"teamMeetingEnabled\">\r" +
    "\n" +
    "                    <select class=\"form-control\" ng-options=\"item.text for item in filterOptions.teamMeetings\" ng-model=\"filterOptions.selectedTeamMeeting\" ng-change=\"changeTeamMeeting()\">\r" +
    "\n" +
    "                        <option value=\"\">-team meeting-</option>\r" +
    "\n" +
    "                    </select>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"col-md-2\" ng-show=\"teamMeetingStaffEnabled\">\r" +
    "\n" +
    "                    <select class=\"form-control\" ng-options=\"item.text for item in filterOptions.teamMeetingStaffs\" ng-model=\"filterOptions.selectedTeamMeetingStaff\">\r" +
    "\n" +
    "                        <option value=\"\">-attendee-</option>\r" +
    "\n" +
    "                    </select>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"col-md-1\" ng-show=\"gradeEnabled\">\r" +
    "\n" +
    "                    <select class=\"form-control\" ng-model=\"filterOptions.selectedGrade\" ng-options=\"item.text for item in filterOptions.grades\" ng-change=\"changeGrade()\">\r" +
    "\n" +
    "                        <option value=\"\">-grade-</option>\r" +
    "\n" +
    "                    </select>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"col-md-1\" ng-show=\"teacherEnabled\">\r" +
    "\n" +
    "                    <select class=\"form-control\" ng-options=\"item.text for item in filterOptions.teachers\" ng-model=\"filterOptions.selectedTeacher\" ng-change=\"changeTeacher()\">\r" +
    "\n" +
    "                        <option value=\"\">-teacher-</option>\r" +
    "\n" +
    "                    </select>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"col-md-1\" ng-show=\"interventionistEnabled\">\r" +
    "\n" +
    "                    <select class=\"form-control\" ng-options=\"item.text for item in filterOptions.interventionists\" ng-model=\"filterOptions.selectedInterventionist\" ng-change=\"changeInterventionist()\">\r" +
    "\n" +
    "                        <option value=\"\">-interventionist-</option>\r" +
    "\n" +
    "                    </select>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"col-md-1\" ng-show=\"sectionEnabled\">\r" +
    "\n" +
    "                    <select class=\"form-control\" ng-options=\"item.text for item in filterOptions.sections\" ng-model=\"filterOptions.selectedSection\" ng-change=\"changeSection()\">\r" +
    "\n" +
    "                        <option value=\"\">-section-</option>\r" +
    "\n" +
    "                    </select>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"col-md-1\" ng-show=\"interventionGroupEnabled\">\r" +
    "\n" +
    "                    <select class=\"form-control\" ng-options=\"item.text for item in filterOptions.interventionGroups\" ng-model=\"filterOptions.selectedInterventionGroup\" ng-change=\"changeInterventionGroup()\">\r" +
    "\n" +
    "                        <option value=\"\">-intv. group-</option>\r" +
    "\n" +
    "                    </select>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"col-md-2\" ng-show=\"studentEnabled\">\r" +
    "\n" +
    "                    <select class=\"form-control\" ng-model=\"filterOptions.selectedStudent\" ng-options=\"item.text for item in filterOptions.students\" ng-change=\"changeStudent()\">\r" +
    "\n" +
    "                        <option value=\"\">-student-</option>\r" +
    "\n" +
    "                    </select>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"col-md-3\" ng-show=\"stintEnabled\">\r" +
    "\n" +
    "                    <select class=\"form-control\" ng-model=\"filterOptions.selectedStint\" ng-options=\"item.text for item in filterOptions.stints\">\r" +
    "\n" +
    "                        <option value=\"\">-all stints-</option>\r" +
    "\n" +
    "                    </select>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"col-md-1\" ng-show=\"hrsFormEnabled\">\r" +
    "\n" +
    "                    <select class=\"form-control\" ng-model=\"filterOptions.selectedHrsForm\" ng-options=\"item.text for item in filterOptions.hrsForms\">\r" +
    "\n" +
    "                        <option value=\"\">-form-</option>\r" +
    "\n" +
    "                    </select>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "            <hr ng-if=\"staffQuickSearchEnabled || studentQuickSearchEnabled || studentDetailedQuickSearchEnabled\" />\r" +
    "\n" +
    "            <div>\r" +
    "\n" +
    "                <div class=\"row\" ng-if=\"staffQuickSearchEnabled\">\r" +
    "\n" +
    "                    <div class=\"col-md-2\">\r" +
    "\n" +
    "                        <span style=\"text-transform: Uppercase; font-weight: bold; margin-right: 10px;\">Staff Quick Search:</span>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                    <div class=\"col-md-10\">\r" +
    "\n" +
    "                        <table style=\"width:600px;\">\r" +
    "\n" +
    "                            <tr>\r" +
    "\n" +
    "                                <td><div ui-select2=\"StaffQuickSearchRemoteOptions\" ng-model=\"filterOptions.quickSearchStaff\" /></td>\r" +
    "\n" +
    "                                <td style=\"width:90px;padding-left:10px;\"><button class=\"btn btn-primary\" ng-click=\"qsCallBack()\"><i class=\"fa fa-check-square-o\"></i> Select</button></td>\r" +
    "\n" +
    "                            </tr>\r" +
    "\n" +
    "                        </table>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                 </div>\r" +
    "\n" +
    "                <div class=\"row\" ng-if=\"studentQuickSearchEnabled\">\r" +
    "\n" +
    "                    <div class=\"col-md-2\">\r" +
    "\n" +
    "                        <span style=\"text-transform: Uppercase; font-weight: bold; margin-right: 10px;\">Student Quick Search:</span>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                    <div class=\"col-md-10\">\r" +
    "\n" +
    "                        <table style=\"width:600px;\">\r" +
    "\n" +
    "                            <tr>\r" +
    "\n" +
    "                                <td><div ui-select2=\"StudentQuickSearchRemoteOptions\" ng-model=\"filterOptions.quickSearchStudent\" /></td>\r" +
    "\n" +
    "                                <td style=\"width:90px;padding-left:10px;\"><button class=\"btn btn-primary\" ng-click=\"qsCallBack()\"><i class=\"fa fa-check-square-o\"></i> Select</button></td>\r" +
    "\n" +
    "                            </tr>\r" +
    "\n" +
    "                        </table>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"row\" ng-if=\"studentDetailedQuickSearchEnabled\">\r" +
    "\n" +
    "                    <div class=\"col-md-2\">\r" +
    "\n" +
    "                        <span style=\"text-transform: Uppercase; font-weight: bold; margin-right: 10px;\">Student Quick Search:</span>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                    <div class=\"col-md-10\">\r" +
    "\n" +
    "                        <table style=\"width:600px;\">\r" +
    "\n" +
    "                            <tr>\r" +
    "\n" +
    "                                <td><div ui-select2=\"StudentDetailedQuickSearchRemoteOptions\" ng-model=\"filterOptions.quickSearchStudent\" /></td>\r" +
    "\n" +
    "                                <td style=\"width:90px;padding-left:10px;\"><button class=\"btn btn-primary\" ng-click=\"qsCallBack()\"><i class=\"fa fa-check-square-o\"></i> Select</button></td>\r" +
    "\n" +
    "                            </tr>\r" +
    "\n" +
    "                        </table>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "        </div>\r" +
    "\n" +
    "    </panel>\r" +
    "\n" +
    "</span>\r" +
    "\n" +
    "<span ng-if=\"verticalMode\">\r" +
    "\n" +
    "    <div class=\"form-group\" ng-show=\"schoolYearEnabled\">\r" +
    "\n" +
    "        <label for=\"fieldname\" class=\"col-md-3 control-label\">School Year</label>\r" +
    "\n" +
    "        <div class=\"col-md-6\">\r" +
    "\n" +
    "            <select name=\"verticalSchoolYear\"  class=\"form-control\" ng-options=\"item.text for item in filterOptions.schoolYears\" ng-model=\"filterOptions.selectedSchoolYear\" ng-change=\"changeSchoolYear()\" ng-required=\"schoolYearRequired\">\r" +
    "\n" +
    "                <option value=\"\">-school year-</option>\r" +
    "\n" +
    "            </select>\r" +
    "\n" +
    "        </div>\r" +
    "\n" +
    "    </div>\r" +
    "\n" +
    "    <div class=\"form-group\" ng-show=\"benchmarkDateEnabled\">\r" +
    "\n" +
    "        <label for=\"fieldname\" class=\"col-md-3 control-label\">Benchmark Date</label>\r" +
    "\n" +
    "        <div class=\"col-md-6\">\r" +
    "\n" +
    "            <select class=\"form-control\" ng-options=\"item.text for item in filterOptions.benchmarkDates\" ng-model=\"filterOptions.selectedBenchmarkDate\" name=\"benchmarkDate\" ng-required=\"benchmarkDateRequired\">\r" +
    "\n" +
    "                <option value=\"\">-benchmark date-</option>\r" +
    "\n" +
    "            </select>\r" +
    "\n" +
    "        </div>\r" +
    "\n" +
    "    </div>\r" +
    "\n" +
    "    <div class=\"form-group\" ng-show=\"schoolEnabled\">\r" +
    "\n" +
    "        <label for=\"fieldname\" class=\"col-md-3 control-label\">School</label>\r" +
    "\n" +
    "        <div class=\"col-md-6\">\r" +
    "\n" +
    "            <select name=\"verticalSchool\" class=\"form-control\" ng-options=\"item.text for item in filterOptions.schools\" ng-model=\"filterOptions.selectedSchool\"   ng-required=\"schoolRequired\"  ng-change=\"changeSchool()\">\r" +
    "\n" +
    "                <option value=\"\">-school-</option>\r" +
    "\n" +
    "            </select>\r" +
    "\n" +
    "        </div>\r" +
    "\n" +
    "    </div>\r" +
    "\n" +
    "    <div class=\"form-group\" ng-show=\"gradeEnabled\">\r" +
    "\n" +
    "        <label for=\"fieldname\" class=\"col-md-3 control-label\">Grade</label>\r" +
    "\n" +
    "        <div class=\"col-md-6\">\r" +
    "\n" +
    "            <select name=\"verticalGrade\" class=\"form-control\" ng-model=\"filterOptions.selectedGrade\" ng-options=\"item.ShortName for item in filterOptions.grades\" ng-change=\"changeGrade()\"></select>\r" +
    "\n" +
    "        </div>\r" +
    "\n" +
    "    </div>\r" +
    "\n" +
    "    <div class=\"form-group\" ng-show=\"teacherEnabled\">\r" +
    "\n" +
    "        <label for=\"fieldname\" class=\"col-md-3 control-label\">Teacher</label>\r" +
    "\n" +
    "        <div class=\"col-md-6\">\r" +
    "\n" +
    "            <select class=\"form-control\" ng-options=\"item.text for item in filterOptions.teachers\" ng-model=\"filterOptions.selectedTeacher\" ng-change=\"changeTeacher()\"></select>\r" +
    "\n" +
    "        </div>\r" +
    "\n" +
    "    </div>\r" +
    "\n" +
    "    <div class=\"form-group\" ng-show=\"interventionistEnabled\">\r" +
    "\n" +
    "        <label for=\"fieldname\" class=\"col-md-3 control-label\">Teacher</label>\r" +
    "\n" +
    "        <div class=\"col-md-6\">\r" +
    "\n" +
    "            <select class=\"form-control\" ng-options=\"item.text for item in filterOptions.interventionists\" ng-model=\"filterOptions.selectedInterventionist\" ng-change=\"changeInterventionist()\"></select>\r" +
    "\n" +
    "        </div>\r" +
    "\n" +
    "    </div>\r" +
    "\n" +
    "    <div class=\"form-group\" ng-show=\"sectionEnabled\">\r" +
    "\n" +
    "        <label for=\"fieldname\" class=\"col-md-3 control-label\">Section</label>\r" +
    "\n" +
    "        <div class=\"col-md-6\">\r" +
    "\n" +
    "            <select name=\"verticalSection\" class=\"form-control\" ng-options=\"item.text for item in filterOptions.sections\" ng-model=\"filterOptions.selectedSection\" ng-change=\"changeSection()\"></select>\r" +
    "\n" +
    "        </div>\r" +
    "\n" +
    "    </div>\r" +
    "\n" +
    "    <div class=\"form-group\" ng-show=\"interventionGroupEnabled\">\r" +
    "\n" +
    "        <label for=\"fieldname\" class=\"col-md-3 control-label\">Intervention Group</label>\r" +
    "\n" +
    "        <div class=\"col-md-6\">\r" +
    "\n" +
    "            <select name=\"verticalInterventionGroup\" class=\"form-control\" ng-options=\"item.text for item in filterOptions.interventionGroups\" ng-model=\"filterOptions.selectedInterventionGroup\" ng-change=\"changeInterventionGroup()\"></select>\r" +
    "\n" +
    "        </div>\r" +
    "\n" +
    "    </div>\r" +
    "\n" +
    "    <div class=\"form-group\" ng-show=\"studentEnabled\">\r" +
    "\n" +
    "        <label for=\"fieldname\" class=\"col-md-3 control-label\">Student</label>\r" +
    "\n" +
    "        <div class=\"col-md-6\">\r" +
    "\n" +
    "            <select name=\"verticalStudent\" class=\"form-control\" ng-model=\"filterOptions.selectedStudent\" ng-options=\"item.text for item in filterOptions.students\"></select>\r" +
    "\n" +
    "        </div>\r" +
    "\n" +
    "    </div>\r" +
    "\n" +
    "</span>\r" +
    "\n" +
    "\r" +
    "\n" +
    "<style>\r" +
    "\n" +
    "    /* TODO: this is a hack for the multiselect to stop scrolling*/\r" +
    "\n" +
    "    .panel-body{\r" +
    "\n" +
    "        overflow-x: visible !important;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "</style>"
  );


  $templateCache.put('templates/hfw-calculatedlabel.html',
    "<span ng-bind=\"Calculate(currentField.CalculationFields)\"></span>"
  );


  $templateCache.put('templates/linegraph-detail.html',
    "<table class=\"table table-striped table-lowPadding table-bordered\">\r" +
    "\n" +
    "    <thead>\r" +
    "\n" +
    "        <tr>\r" +
    "\n" +
    "            <th class=\"\">Dates</th>\r" +
    "\n" +
    "            <th ng-repeat=\"date in dataManager.BenchmarkDates\" class=\"verticalTdDate\">\r" +
    "\n" +
    "                <div class=\"verticalDate\">{{::dataManager.formatDate(date.DueDate)}}</div>\r" +
    "\n" +
    "            </th>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "    </thead>\r" +
    "\n" +
    "    <tbody>\r" +
    "\n" +
    "        <tr>\r" +
    "\n" +
    "            <th>Student Name</th>\r" +
    "\n" +
    "            <td ng-repeat=\"date in dataManager.BenchmarkDates\" ng-class=\"dataManager.getBackgroundClass(date.Result, date.TestDueDateID)\">\r" +
    "\n" +
    "                {{ ::dataManager.getDisplayValue(date.Result.FieldValueID)}}\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "        <tr>\r" +
    "\n" +
    "            <th>Exceeds Expectations</th>\r" +
    "\n" +
    "            <td ng-repeat=\"date in dataManager.BenchmarkDates\">\r" +
    "\n" +
    "                {{::dataManager.getDisplayValue(date.Exceeds)}}\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "        <tr>\r" +
    "\n" +
    "            <th>\r" +
    "\n" +
    "                Meets Expectations\r" +
    "\n" +
    "            </th>\r" +
    "\n" +
    "            <td ng-repeat=\"date in dataManager.BenchmarkDates\">\r" +
    "\n" +
    "                {{::dataManager.getDisplayValue(date.Meets) }}\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "        <tr>\r" +
    "\n" +
    "            <th>Approaches Expectations</th>\r" +
    "\n" +
    "            <td ng-repeat=\"date in dataManager.BenchmarkDates\">\r" +
    "\n" +
    "                {{ ::dataManager.getDisplayValue(date.Approaches) }}\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "        <tr>\r" +
    "\n" +
    "            <th>Does Not Meet Expectations</th>\r" +
    "\n" +
    "            <td ng-repeat=\"date in dataManager.BenchmarkDates\">\r" +
    "\n" +
    "                {{ ::dataManager.getDisplayValue(date.DoesNotMeet) }}\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "        <tr>\r" +
    "\n" +
    "            <th>Grade</th>\r" +
    "\n" +
    "            <td ng-repeat=\"date in dataManager.BenchmarkDates\">\r" +
    "\n" +
    "                {{ ::date.GradeShortName}}\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "        <tr>\r" +
    "\n" +
    "            <th>Test No.</th>\r" +
    "\n" +
    "            <td ng-repeat=\"date in dataManager.BenchmarkDates\">\r" +
    "\n" +
    "                {{ ::date.TestNumber }}\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "    </tbody>\r" +
    "\n" +
    "</table>"
  );


  $templateCache.put('templates/nav_renderer.html',
    "<a ng-click=\"select(item)\" ng-href=\"{{::item.url}}\">\n" +
    "\t<i ng-if=\"item.iconClasses\" class=\"{{::item.iconClasses}}\"></i><span>{{::item.label}}</span>\n" +
    "\t<span ng-bind-html=\"item.html\"></span>\n" +
    "</a>\n" +
    "<ul ng-if=\"item.children.length\" data-slide-out-nav=\"item.open\">\n" +
    "    <li ng-repeat=\"item in item.children\"\n" +
    "\t    ng-class=\"{ hasChild: (item.children!==undefined && item.children.length > 0),\n" +
    "                      active: item.selected,\n" +
    "                        open: (item.children!==undefined && item.children.length > 0) && item.open }\"\n" +
    "    \tng-include=\"'templates/nav_renderer.html'\"\n" +
    "    ></li>\n" +
    "</ul>\n"
  );


  $templateCache.put('templates/nav_renderer_horizontal.html',
    "<a ng-click=\"select(item)\" ng-href=\"{{::item.url}}\">\n" +
    "  <i ng-if=\"item.iconClasses\" class=\"{{::item.iconClasses}}\"></i><span>{{::item.label}}</span>\n" +
    "  <span ng-bind-html=\"item.html\"></span>\n" +
    "</a>\n" +
    "<ul ng-if=\"item.children.length\">\n" +
    "    <li ng-repeat=\"item in item.children\"\n" +
    "      ng-class=\"{ hasChild: (item.children!==undefined),\n" +
    "                      active: item.selected,\n" +
    "                        open: (item.children!==undefined) && item.open }\"\n" +
    "      ng-include=\"'templates/nav_renderer_horizontal.html'\"\n" +
    "    ></li>\n" +
    "</ul>\n"
  );


  $templateCache.put('templates/ns-errors.html',
    "<div class=\"col-md-12\">\r" +
    "\n" +
    "    <alert ng-repeat=\"alert in errors\" type=\"{{alert.type}}\"><span ng-bind-html=\"alert.msg | safe_html\"></span></alert>\r" +
    "\n" +
    "</div>"
  );


  $templateCache.put('templates/observation-summary-field-chooser.html',
    "<div class=\"btn-toolbar\">\r" +
    "\n" +
    "    <div class=\"dropdown\" uib-dropdown is-open=\"settings.menuOpen\" auto-close=\"outsideClick\">\r" +
    "\n" +
    "        <a href=\"\" class=\"dropdown-toggle btn btn-sky\" uib-dropdown-toggle>Change Selected Assessments/Fields <i class=\"fa fa-bars\"></i></a>\r" +
    "\n" +
    "        <div style=\"min-width:450px;\" class=\"dropdown-menu userinfo arrow\" uib-dropdown-menu>\r" +
    "\n" +
    "            <div class=\"col-md-12 nsRowBorderBottom\" style=\"padding-top:2px;padding-bottom:2px;\" ng-repeat=\"assessment in assessmentService.BenchmarkAssessments\">\r" +
    "\n" +
    "                <div class=\"col-md-2\">\r" +
    "\n" +
    "                    <button class=\"btn btn-primary btn-xs\" ng-click=\"openFieldsPopup(assessment)\"><i class=\"fa fa-bars\"></i> Fields</button>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"col-md-8\" style=\"color:#656b79\">\r" +
    "\n" +
    "                    {{::assessment.text}}\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"col-md-2\">\r" +
    "\n" +
    "                    <switch class=\"green\" ng-model=\"assessment.Visible\" ng-change=\"updateSelectedAssessment(assessment)\">\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "            <div ng-if=\"assessmentService.StateTests.length > 0\" class=\"divider col-md-12\"></div>\r" +
    "\n" +
    "            <div class=\"col-md-12 nsRowBorderBottom\" style=\"padding-top:2px;padding-bottom:2px;\" ng-repeat=\"assessment in assessmentService.StateTests\">\r" +
    "\n" +
    "                <div class=\"col-md-2\">\r" +
    "\n" +
    "                    <button class=\"btn btn-primary btn-xs\" ng-click=\"openFieldsPopup(assessment)\"><i class=\"fa fa-bars\"></i> Fields</button>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"col-md-8\" style=\"color:#656b79\">\r" +
    "\n" +
    "                    {{::assessment.text}}\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"col-md-2\">\r" +
    "\n" +
    "                    <switch class=\"green\" ng-model=\"assessment.Visible\" ng-change=\"updateSelectedAssessment(assessment)\">\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "            <div style=\"margin-top:15px; margin-bottom:15px;\" class=\"col-md-12\">\r" +
    "\n" +
    "                <button ng-click=\"refreshAssessments()\" class=\"btn btn-success\"><i class=\"fa-save fa\"></i> Save and Update Report</button>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "        </div>\r" +
    "\n" +
    "    </div>\r" +
    "\n" +
    "</div>"
  );


  $templateCache.put('templates/observation-summary-section.html',
    "<table class=\"table table-striped\">\r" +
    "\n" +
    "    <thead>\r" +
    "\n" +
    "        <tr>\r" +
    "\n" +
    "            <th ng-if=\"showCheckboxes === true\" rowspan=\"2\">\r" +
    "\n" +
    "                Select All<br />\r" +
    "\n" +
    "                <switch ng-model=\"allSelected\" ng-change=\"selectAllStudents()\"></switch>\r" +
    "\n" +
    "            </th>\r" +
    "\n" +
    "            <th rowspan=\"2\"><div style=\"cursor: pointer\" ng-click=\"sort('LastName')\">Last Name <i class=\"{{manualSortHeaders.lastNameHeaderClass}}\"></i></div></th>\r" +
    "\n" +
    "            <th rowspan=\"2\"><div style=\"cursor: pointer\" ng-click=\"sort('FirstName')\">First Name <i class=\"{{manualSortHeaders.firstNameHeaderClass}}\"></i></div></th>\r" +
    "\n" +
    "            <th style=\"text-align: center\" scope=\"col\" data-tablesaw-sortable-col data-tablesaw-priority=\"3\" ng-repeat=\"headerGroup in observationSummaryManager.Scores.HeaderGroups\" colspan=\"{{headerGroup.FieldCount}}\">\r" +
    "\n" +
    "                <a href=\"\" ng-click=\"hideAssessment(headerGroup)\" class=\"pull-right\"><i class=\"fa fa-minus-square\"></i></a> {{headerGroup.AssessmentName}}\r" +
    "\n" +
    "            </th>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "        <tr>\r" +
    "\n" +
    "            <th scope=\"col\" data-tablesaw-sortable-col data-tablesaw-priority=\"3\" ng-repeat=\"field in observationSummaryManager.Scores.Fields\">\r" +
    "\n" +
    "                <a href=\"\" ng-click=\"hideField(field)\" class=\"pull-right\"><i class=\"fa fa-minus-square\"></i></a>\r" +
    "\n" +
    "                \r" +
    "\n" +
    "                <div style=\"cursor: pointer\" ng-click=\"sort($index)\">{{field.FieldName}} <i class=\"{{headerClassArray[$index]}}\"></i></div>\r" +
    "\n" +
    "            </th>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "    </thead>\r" +
    "\n" +
    "    <tbody>\r" +
    "\n" +
    "        <tr ng-class=\"{'nsObservationSummarySelected' : studentResult.selected}\" ng-repeat=\"studentResult in observationSummaryManager.Scores.StudentResults | orderBy:sortArray\">\r" +
    "\n" +
    "            <td ng-if=\"showCheckboxes === true\">\r" +
    "\n" +
    "                <switch ng-model=\"studentResult.selected\" ng-change=\"changeTeamMeetingStudentSelection(studentResult)\"></switch>\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "            <td align=\"left\" data-title=\"LastName\">{{studentResult.LastName}}</td>\r" +
    "\n" +
    "            <td align=\"left\" data-title=\"FirstName\">\r" +
    "\n" +
    "                {{studentResult.FirstName}}\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "            <td ng-class=\"getBackgroundClass(studentResult.GradeId, fieldResult)\" ng-repeat=\"fieldResult in studentResult.OSFieldResults\">\r" +
    "\n" +
    "                <ns-assessment-field mode=\"'readonly'\" result=\"fieldResult\" all-results=\"studentResult.OSFieldResults\"></ns-assessment-field>\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "    </tbody>\r" +
    "\n" +
    "</table>\r" +
    "\n" +
    "\r" +
    "\n" +
    "<style>\r" +
    "\n" +
    "    .nsObservationSummarySelected {\r" +
    "\n" +
    "        border-left: 4px solid green;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "</style>"
  );


  $templateCache.put('templates/observation-summary-student.html',
    "<table class=\"table table-striped\">\r" +
    "\n" +
    "    <thead>\r" +
    "\n" +
    "        <tr>\r" +
    "\n" +
    "            <th ng-if=\"showCheckboxes === true\" rowspan=\"2\">\r" +
    "\n" +
    "                Select All<br />\r" +
    "\n" +
    "                <switch ng-model=\"allSelected\" ng-change=\"selectAllStudents()\"></switch>\r" +
    "\n" +
    "            </th>\r" +
    "\n" +
    "            <th rowspan=\"2\"><div style=\"cursor: pointer\" ng-click=\"sort('LastName')\">Last Name <i class=\"{{manualSortHeaders.lastNameHeaderClass}}\"></i></div></th>\r" +
    "\n" +
    "            <th rowspan=\"2\"><div style=\"cursor: pointer\" ng-click=\"sort('FirstName')\">First Name <i class=\"{{manualSortHeaders.firstNameHeaderClass}}\"></i></div></th>\r" +
    "\n" +
    "            <th rowspan=\"2\"><div style=\"cursor: pointer\" ng-click=\"sort('TestDate')\">Benchmark Date <i class=\"{{manualSortHeaders.firstNameHeaderClass}}\"></i></div></th>\r" +
    "\n" +
    "            <th style=\"text-align: center\" scope=\"col\" data-tablesaw-sortable-col data-tablesaw-priority=\"3\" ng-repeat=\"headerGroup in observationSummaryManager.Scores.HeaderGroups\" colspan=\"{{headerGroup.FieldCount}}\">\r" +
    "\n" +
    "                {{headerGroup.AssessmentName}}\r" +
    "\n" +
    "            </th>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "        <tr>\r" +
    "\n" +
    "            <th scope=\"col\" data-tablesaw-sortable-col data-tablesaw-priority=\"3\" ng-repeat=\"field in observationSummaryManager.Scores.Fields\">\r" +
    "\n" +
    "                <div style=\"cursor: pointer\" ng-click=\"sort($index)\">{{field.FieldName}} <i class=\"{{headerClassArray[$index]}}\"></i></div>\r" +
    "\n" +
    "            </th>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "    </thead>\r" +
    "\n" +
    "    <tbody>\r" +
    "\n" +
    "        <tr ng-repeat=\"studentResult in observationSummaryManager.Scores.StudentResults | orderBy:sortArray\">\r" +
    "\n" +
    "            <td ng-if=\"showCheckboxes === true\">\r" +
    "\n" +
    "                <switch ng-model=\"studentResult.selected\" ng-change=\"changeTeamMeetingStudentSelection(studentResult)\"></switch>\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "            <td align=\"left\" data-title=\"LastName\">{{studentResult.LastName}}</td>\r" +
    "\n" +
    "            <td align=\"left\" data-title=\"FirstName\">\r" +
    "\n" +
    "                {{studentResult.FirstName}}\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "            <td align=\"left\" data-title=\"FirstName\">\r" +
    "\n" +
    "                {{studentResult.TestDate | nsDateFormat }}\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "            <td ng-class=\"getBackgroundClass(studentResult.GradeId, fieldResult)\" ng-repeat=\"fieldResult in studentResult.OSFieldResults\">\r" +
    "\n" +
    "                <ns-assessment-field mode=\"'readonly'\" result=\"fieldResult\" all-results=\"studentResult.OSFieldResults\"></ns-assessment-field>\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "    </tbody>\r" +
    "\n" +
    "</table>"
  );


  $templateCache.put('templates/observation-summary-tm-attend.html',
    "<table class=\"table table-striped\">\r" +
    "\n" +
    "    <thead>\r" +
    "\n" +
    "        <tr>\r" +
    "\n" +
    "            <th rowspan=\"2\"><div style=\"cursor: pointer\" ng-click=\"sort('LastName')\">Last Name <i class=\"{{manualSortHeaders.lastNameHeaderClass}}\"></i></div></th>\r" +
    "\n" +
    "            <th rowspan=\"2\"><div style=\"cursor: pointer\" ng-click=\"sort('FirstName')\">First Name <i class=\"{{manualSortHeaders.firstNameHeaderClass}}\"></i></div></th>\r" +
    "\n" +
    "            <th rowspan=\"2\">Modify Notes</th>\r" +
    "\n" +
    "            <th rowspan=\"2\">\r" +
    "\n" +
    "                Assign Intervention\r" +
    "\n" +
    "            </th>\r" +
    "\n" +
    "            <th rowspan=\"2\">Current Interventions</th>\r" +
    "\n" +
    "            <th style=\"text-align: center\" scope=\"col\" data-tablesaw-sortable-col data-tablesaw-priority=\"3\" ng-repeat=\"headerGroup in observationSummaryManager.Scores.HeaderGroups\" colspan=\"{{headerGroup.FieldCount}}\">\r" +
    "\n" +
    "                {{headerGroup.AssessmentName}}\r" +
    "\n" +
    "            </th>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "        <tr>\r" +
    "\n" +
    "            <th scope=\"col\" data-tablesaw-sortable-col data-tablesaw-priority=\"3\" ng-repeat=\"field in observationSummaryManager.Scores.Fields\">\r" +
    "\n" +
    "                <div style=\"cursor: pointer\" ng-click=\"sort($index)\">{{field.FieldName}} <i class=\"{{headerClassArray[$index]}}\"></i></div>\r" +
    "\n" +
    "            </th>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "    </thead>\r" +
    "\n" +
    "    <tbody>\r" +
    "\n" +
    "        <tr ng-if=\"observationSummaryManager.Scores.StudentResults.length == 0\">\r" +
    "\n" +
    "            <td colspan=\"100\">\r" +
    "\n" +
    "                No students selected\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "        <tr ns-repeat-complete ng-repeat=\"studentResult in observationSummaryManager.Scores.StudentResults | orderBy:sortArray\">\r" +
    "\n" +
    "            <td align=\"left\" data-title=\"LastName\">{{studentResult.LastName}}</td>\r" +
    "\n" +
    "            <td align=\"left\" data-title=\"FirstName\">\r" +
    "\n" +
    "                {{studentResult.FirstName}}\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "            <td>\r" +
    "\n" +
    "                 <a class=\"btn btn-xs btn-midnightblue\" href=\"#/tm-attend-notes/{{::selectedTeamMeetingId}}/{{::studentResult.StudentId}}\"><i class=\"fa fa-file-text-o\"></i> {{::studentResult.NoteCount}} Notes</a>\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "            <td>\r" +
    "\n" +
    "                <a href=\"\" ng-click=\"assignStudentToIntervention(studentResult)\" class=\"btn btn-xs btn-primary\"><i class=\"fa fa-bolt\"></i> Assign</a>\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "            <td class=\"smallAccordion\">\r" +
    "\n" +
    "                <span class=\"badge badge-success\" ng-if=\"!studentResult.Interventions\">no interventions</span>\r" +
    "\n" +
    "                <button class=\"btn-xs btn btn-sky\" ng-if=\"studentResult.Interventions.InterventionsBySchoolYear.length > 0\" ng-click=\"openInterventionPopup(studentResult)\"><i class=\"fa fa-eye\"></i> View</button>\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "            <td ng-class=\"getBackgroundClass(studentResult.GradeId, fieldResult)\" ng-repeat=\"fieldResult in studentResult.OSFieldResults\">\r" +
    "\n" +
    "                <observation-summary-view-field lookup-fields-array=\"observationSummaryManager.LookupLists\" result=\"fieldResult\" all-results=\"studentResult.OSFieldResults\"></observation-summary-view-field>\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "    </tbody>\r" +
    "\n" +
    "</table>\r" +
    "\n" +
    "<script type=\"text/ng-template\" id=\"interventionList.html\">\r" +
    "\n" +
    "    <div class=\"modal-header\">\r" +
    "\n" +
    "        <h3 class=\"modal-title\">Intervention History for {{::selectedStudentResult.LastName}}, {{::selectedStudentResult.FirstName}}</h3>\r" +
    "\n" +
    "    </div>\r" +
    "\n" +
    "    <div class=\"modal-body\">\r" +
    "\n" +
    "        <div ng-repeat=\"interventionYear in selectedStudentResult.Interventions.InterventionsBySchoolYear\">\r" +
    "\n" +
    "            <h2 style=\"font-weight:bold\">{{::interventionYear.SchoolYear}}</h2>\r" +
    "\n" +
    "            <table class=\"table\">\r" +
    "\n" +
    "                <thead>\r" +
    "\n" +
    "                    <tr>\r" +
    "\n" +
    "                        <th width=\"10%\">\r" +
    "\n" +
    "                            Dashboard\r" +
    "\n" +
    "                        </th>\r" +
    "\n" +
    "                        <th width=\"20%\">\r" +
    "\n" +
    "                            Intervention Group Name\r" +
    "\n" +
    "                        </th>\r" +
    "\n" +
    "                        <th width=\"20%\">\r" +
    "\n" +
    "                            Type (Description)\r" +
    "\n" +
    "                        </th>\r" +
    "\n" +
    "                        <th width=\"25%\">\r" +
    "\n" +
    "                            Start Date\r" +
    "\n" +
    "                        </th>\r" +
    "\n" +
    "                        <th width=\"25%\">\r" +
    "\n" +
    "                            End Date\r" +
    "\n" +
    "                        </th>\r" +
    "\n" +
    "                    </tr>\r" +
    "\n" +
    "                </thead>\r" +
    "\n" +
    "                <tbody>\r" +
    "\n" +
    "                    <tr ng-repeat=\"intervention in interventionYear.InterventionList\">\r" +
    "\n" +
    "                        <td>\r" +
    "\n" +
    "                            <button class=\"btn btn-xs btn-primary\" ng-click=\"goToDashboard(intervention.SchoolYear, intervention.SchoolId, intervention.InterventionistId, intervention.InterventionGroupId, intervention.StudentId, intervention.Id)\"><i class=\"fa fa-dashboard\"></i> Go</button>\r" +
    "\n" +
    "                        </td>\r" +
    "\n" +
    "                        <td>\r" +
    "\n" +
    "                            {{::intervention.InterventionGroupName}}\r" +
    "\n" +
    "                        </td>\r" +
    "\n" +
    "                        <td><span class=\"badge badge-danger\">{{::intervention.InterventionType}}</span> ({{::intervention.InterventionTypeLong}})</td>\r" +
    "\n" +
    "                        <td>{{::intervention.StartDate | nsLongDateFormat}}</td>\r" +
    "\n" +
    "                        <td>{{::intervention.EndDate == null ? 'no end date' : intervention.EndDate | nsLongDateFormat}}</td>\r" +
    "\n" +
    "                    </tr>\r" +
    "\n" +
    "                </tbody>\r" +
    "\n" +
    "            </table>\r" +
    "\n" +
    "        </div>\r" +
    "\n" +
    "    </div>\r" +
    "\n" +
    "    <div class=\"modal-footer\">\r" +
    "\n" +
    "        <button class=\"btn btn-primary\" ng-click=\"cancel()\"><i class=\"fa fa-times\"></i> Close</button>\r" +
    "\n" +
    "    </div>\r" +
    "\n" +
    "</script>\r" +
    "\n" +
    "\r" +
    "\n" +
    "<style>\r" +
    "\n" +
    "    .smallAccordion .panel-heading{\r" +
    "\n" +
    "        height: 20px;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "    .smallAccordion accordion h4.panel-title a{\r" +
    "\n" +
    "        line-height: 20px;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "</style>"
  );


  $templateCache.put('templates/observation-summary-tm.html',
    "<table class=\"table table-striped\">\r" +
    "\n" +
    "    <thead>\r" +
    "\n" +
    "        <tr>\r" +
    "\n" +
    "            <th rowspan=\"2\"><div style=\"cursor: pointer\" ng-click=\"sort('LastName')\">Last Name <i class=\"{{manualSortHeaders.lastNameHeaderClass}}\"></i></div></th>\r" +
    "\n" +
    "            <th rowspan=\"2\"><div style=\"cursor: pointer\" ng-click=\"sort('FirstName')\">First Name <i class=\"{{manualSortHeaders.firstNameHeaderClass}}\"></i></div></th>\r" +
    "\n" +
    "            <th style=\"text-align: center\" scope=\"col\" data-tablesaw-sortable-col data-tablesaw-priority=\"3\" ng-repeat=\"headerGroup in observationSummaryManager.Scores.HeaderGroups\" colspan=\"{{headerGroup.FieldCount}}\">\r" +
    "\n" +
    "                {{headerGroup.AssessmentName}}\r" +
    "\n" +
    "            </th>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "        <tr>\r" +
    "\n" +
    "            <th scope=\"col\" data-tablesaw-sortable-col data-tablesaw-priority=\"3\" ng-repeat=\"field in observationSummaryManager.Scores.Fields\">\r" +
    "\n" +
    "                <div style=\"cursor: pointer\" ng-click=\"sort($index)\">{{field.FieldName}} <i class=\"{{headerClassArray[$index]}}\"></i></div>\r" +
    "\n" +
    "            </th>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "    </thead>\r" +
    "\n" +
    "    <tbody>\r" +
    "\n" +
    "        <tr ng-if=\"observationSummaryManager.Scores.StudentResults.length == 0\">\r" +
    "\n" +
    "            <td colspan=\"100\">\r" +
    "\n" +
    "                No students selected\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "        <tr ns-repeat-complete ng-repeat=\"studentResult in observationSummaryManager.Scores.StudentResults | orderBy:sortArray\">\r" +
    "\n" +
    "            <td align=\"left\" data-title=\"LastName\">{{studentResult.LastName}}</td>\r" +
    "\n" +
    "            <td align=\"left\" data-title=\"FirstName\">\r" +
    "\n" +
    "                {{studentResult.FirstName}}\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "            <td ng-class=\"getBackgroundClass(studentResult.GradeId, fieldResult)\" ng-repeat=\"fieldResult in studentResult.OSFieldResults\">\r" +
    "\n" +
    "                <observation-summary-view-field lookup-fields-array=\"observationSummaryManager.LookupLists\" result=\"fieldResult\" all-results=\"studentResult.OSFieldResults\"></observation-summary-view-field>\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "    </tbody>\r" +
    "\n" +
    "</table>"
  );


  $templateCache.put('templates/panel-tabs-without-heading.html',
    "<div class=\"panel {{panelClass}}\">\n" +
    "  <div class=\"panel-heading\">\n" +
    "        <h4>\n" +
    "            <ul class=\"nav nav-{{type || 'tabs'}}\" ng-class=\"{'nav-stacked': vertical, 'nav-justified': justified}\" ng-transclude></ul>\n" +
    "        </h4>\n" +
    "  </div>\n" +
    "  <div class=\"panel-body\">\n" +
    "    <div class=\"tab-content\">\n" +
    "        <div class=\"tab-pane\"\n" +
    "            ng-repeat=\"tab in tabs\"\n" +
    "            ng-class=\"{active: tab.active}\"\n" +
    "            tab-content-transclude=\"tab\">\n" +
    "        </div>\n" +
    "    </div>\n" +
    "  </div>\n" +
    "</div>\n"
  );


  $templateCache.put('templates/panel-tabs.html',
    "<div class=\"panel {{panelClass}}\">\n" +
    "  <div class=\"panel-heading\">\n" +
    "        <h4><i ng-if=\"panelIcon\" class=\"{{panelIcon}}\"></i>{{(panelIcon? \" \":\"\")+heading}}</h4>\n" +
    "        <div class=\"options\">\n" +
    "            <ul class=\"nav nav-{{type || 'tabs'}}\" ng-class=\"{'nav-stacked': vertical, 'nav-justified': justified}\" ng-transclude></ul>\n" +
    "        </div>\n" +
    "  </div>\n" +
    "  <div class=\"panel-body\">\n" +
    "    <div class=\"tab-content\">\n" +
    "        <div class=\"tab-pane\"\n" +
    "            ng-repeat=\"tab in tabs\"\n" +
    "            ng-class=\"{active: tab.active}\"\n" +
    "            tab-content-transclude=\"tab\">\n" +
    "        </div>\n" +
    "    </div>\n" +
    "  </div>\n" +
    "</div>\n"
  );


  $templateCache.put('templates/panel.html',
    "<div class=\"panel {{panelClass}}\">\n" +
    "  <div class=\"panel-heading\">\n" +
    "        <h4><i ng-if=\"panelIcon\" class=\"{{panelIcon}}\"></i>{{(panelIcon? \" \":\"\")+heading}}</h4>\n" +
    "        <div class=\"options\">\n" +
    "        </div>\n" +
    "  </div>\n" +
    "  <div class=\"panel-body\" ng-transclude>\n" +
    "  </div>\n" +
    "</div>\n"
  );


  $templateCache.put('templates/stacked-bar-graph-comparison-options.html',
    "\r" +
    "\n" +
    "<panel panel-class=\"panel-midnightblue\" heading=\"Group\" panel-icon=\"fa fa-group\">\r" +
    "\n" +
    "    <div class=\"tiles-heading\">\r" +
    "\n" +
    "        <div class=\"row\">\r" +
    "\n" +
    "            <div class=\"col-md-6 vertical-separator\">\r" +
    "\n" +
    "                <div class=\"row\">\r" +
    "\n" +
    "                    <h3 class=\"dotted-underline\">Options</h3>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"row filterRowMargin\">\r" +
    "\n" +
    "                    <label class=\"col-sm-4 control-label\">School Year</label>\r" +
    "\n" +
    "                    <div class=\"col-sm-8\">\r" +
    "\n" +
    "                        <div ui-select2=\"select2SchoolYearOptions\" ng-model=\"filterOptions.selectedSchoolYear\" ng-change=\"changeSchoolYears(filterOptions.selectedSchoolYear, {{filterOptions.selectedSchoolYear}})\" data-placeholder=\"Select school year\" style=\"width:100%\"></div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"row filterRowMargin\">\r" +
    "\n" +
    "                    <label class=\"col-sm-4 control-label\">Benchmark Date</label>\r" +
    "\n" +
    "                    <div class=\"col-sm-8\">\r" +
    "\n" +
    "                        <div ui-select2=\"select2BenchmarkDateOptions\" ng-model=\"filterOptions.selectedTestDueDate\" data-placeholder=\"- Select Benchmark Date -\" style=\"width:100%\"></div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"row filterRowMargin\">\r" +
    "\n" +
    "                    <label class=\"col-sm-4 control-label\">Schools</label>\r" +
    "\n" +
    "                    <div class=\"col-sm-8\">\r" +
    "\n" +
    "                        <div ui-select2=\"select2SchoolOptions\" ng-model=\"filterOptions.selectedSchools\" ng-change=\"changeSchools(filterOptions.selectedSchools, {{filterOptions.selectedSchools}})\" data-placeholder=\"- All schools in district -\" style=\"width:100%\"></div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"row filterRowMargin\">\r" +
    "\n" +
    "                    <label class=\"col-sm-4 control-label\">Grades</label>\r" +
    "\n" +
    "                    <div class=\"col-sm-8\">\r" +
    "\n" +
    "                        <div ui-select2=\"select2GradeOptions\" ng-model=\"filterOptions.selectedGrades\" ng-change=\"changeGrades(filterOptions.selectedGrades, {{filterOptions.selectedGrades}})\" data-placeholder=\"- All grades -\" style=\"width:100%\"></div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"row filterRowMargin\">\r" +
    "\n" +
    "                    <label class=\"col-sm-4 control-label\">Teachers</label>\r" +
    "\n" +
    "                    <div class=\"col-sm-8\">\r" +
    "\n" +
    "                        <div ui-select2=\"select2TeacherOptions\" ng-model=\"filterOptions.selectedTeachers\" ng-change=\"changeTeachers(filterOptions.selectedTeachers, {{filterOptions.selectedTeachers}})\" data-placeholder=\"- All teachers -\" style=\"width:100%\"></div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"row filterRowMargin\">\r" +
    "\n" +
    "                    <label class=\"col-sm-4 control-label\">Sections</label>\r" +
    "\n" +
    "                    <div class=\"col-sm-8\">\r" +
    "\n" +
    "                        <div ui-select2=\"select2SectionOptions\" ng-model=\"filterOptions.selectedSections\" data-placeholder=\"- All sections -\" style=\"width:100%\"></div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"row filterRowMargin\">\r" +
    "\n" +
    "                    <label class=\"col-sm-4 control-label\">Assessment Field</label>\r" +
    "\n" +
    "                    <div class=\"col-sm-8\">\r" +
    "\n" +
    "                        <div ui-select2=\"select2SchoolOptions\" ng-model=\"filterOptions.selectedSchools\" ng-change=\"changeSchools(filterOptions.selectedSchools, {{filterOptions.selectedSchools}})\" data-placeholder=\"- All schools in district -\" style=\"width:100%\"></div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "            <div class=\"col-md-6\" style=\"padding-left:25px\">\r" +
    "\n" +
    "                <div class=\"row\">\r" +
    "\n" +
    "                    <h3 class=\"dotted-underline\">Student Attributes</h3>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"row filterRowMargin\" ng-repeat=\"attributeType in filterOptions.attributeTypes\">\r" +
    "\n" +
    "                    <label class=\"col-sm-4 control-label\">{{attributeType.Name}}</label>\r" +
    "\n" +
    "                    <div class=\"col-sm-8\">\r" +
    "\n" +
    "                        <div ui-select2=\"{minimumInputLength: 0, data: attributeType.DropDownData, multiple: true, width: 'resolve'}\" ng-model=\"attributeType.selectedData\" data-placeholder=\"- Select Value -\" style=\"width:100%\"></div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "        </div>\r" +
    "\n" +
    "    </div>\r" +
    "\n" +
    "</panel>\r" +
    "\n" +
    "\r" +
    "\n" +
    "<style>\r" +
    "\n" +
    "    .tab-content {\r" +
    "\n" +
    "        height: 310px;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "</style>"
  );


  $templateCache.put('templates/stacked-bar-graph-group-options.html',
    "\r" +
    "\n" +
    "    <div class=\"tiles-heading\">\r" +
    "\n" +
    "        <div class=\"row\">\r" +
    "\n" +
    "            <div class=\"col-md-6 vertical-separator\">\r" +
    "\n" +
    "                <div class=\"row\">\r" +
    "\n" +
    "                    <h3 class=\"dotted-underline\">Options</h3>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"row filterRowMargin\">\r" +
    "\n" +
    "                    <label class=\"col-sm-4 control-label\">School Year</label>\r" +
    "\n" +
    "                    <div class=\"col-sm-8\">\r" +
    "\n" +
    "                        <div ui-select2=\"select2SchoolYearOptions\" ng-model=\"filterOptions.selectedSchoolYear\" ng-change=\"changeSchoolYears(filterOptions.selectedSchoolYear, {{filterOptions.selectedSchoolYear}})\" data-placeholder=\"Select school year\" style=\"width:100%\"></div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div ng-show=\"tddEnabled\" class=\"row filterRowMargin\">\r" +
    "\n" +
    "                    <label class=\"col-sm-4 control-label\">Benchmark Date</label>\r" +
    "\n" +
    "                    <div class=\"col-sm-8\">\r" +
    "\n" +
    "                        <div ui-select2=\"select2BenchmarkDateOptions\" ng-model=\"filterOptions.selectedTestDueDate\" data-placeholder=\"- Select Benchmark Date -\" style=\"width:100%\"></div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"row filterRowMargin\">\r" +
    "\n" +
    "                    <label class=\"col-sm-4 control-label\">Schools</label>\r" +
    "\n" +
    "                    <div class=\"col-sm-8\">\r" +
    "\n" +
    "                        <div ui-select2=\"select2SchoolOptions\" ng-model=\"filterOptions.selectedSchools\" ng-change=\"changeSchools(filterOptions.selectedSchools, {{filterOptions.selectedSchools}})\" data-placeholder=\"- All schools in district -\" style=\"width:100%\"></div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"row filterRowMargin\">\r" +
    "\n" +
    "                    <label class=\"col-sm-4 control-label\">Grades</label>\r" +
    "\n" +
    "                    <div class=\"col-sm-8\">\r" +
    "\n" +
    "                        <div ui-select2=\"select2GradeOptions\" ng-model=\"filterOptions.selectedGrades\" ng-change=\"changeGrades(filterOptions.selectedGrades, {{filterOptions.selectedGrades}})\" data-placeholder=\"- All grades -\" style=\"width:100%\"></div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"row filterRowMargin\">\r" +
    "\n" +
    "                    <label class=\"col-sm-4 control-label\">Teachers</label>\r" +
    "\n" +
    "                    <div class=\"col-sm-8\">\r" +
    "\n" +
    "                        <div ui-select2=\"select2TeacherOptions\" ng-model=\"filterOptions.selectedTeachers\" ng-change=\"changeTeachers(filterOptions.selectedTeachers, {{filterOptions.selectedTeachers}})\" data-placeholder=\"- All teachers -\" style=\"width:100%\"></div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"row filterRowMargin\">\r" +
    "\n" +
    "                    <label class=\"col-sm-4 control-label\">Sections</label>\r" +
    "\n" +
    "                    <div class=\"col-sm-8\">\r" +
    "\n" +
    "                        <div ui-select2=\"select2SectionOptions\" ng-model=\"filterOptions.selectedSections\" data-placeholder=\"- All sections -\" style=\"width:100%\"></div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"row filterRowMargin\">\r" +
    "\n" +
    "                    <label class=\"col-sm-4 control-label\">Intervention Types</label>\r" +
    "\n" +
    "                    <div class=\"col-sm-8\">\r" +
    "\n" +
    "                        <div ui-select2=\"select2InterventionTypeOptions\" ng-model=\"filterOptions.selectedInterventionTypes\" data-placeholder=\"- All Intervention Types -\" style=\"width:100%\"></div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"row filterRowMargin\">\r" +
    "\n" +
    "                    <label class=\"col-sm-4 control-label\">Assessment Field</label>\r" +
    "\n" +
    "                    <div class=\"col-sm-8\">\r" +
    "\n" +
    "                        <select class=\"form-control\" ng-model=\"filterOptions.selectedAssessmentField\" ng-options=\"field.DisplayLabel group by field.AssessmentName for field in filterOptions.assessments | filter: {DisplayInLineGraphs : true}\">\r" +
    "\n" +
    "                            <option value=\"\"> -- Choose a Field -- </option>\r" +
    "\n" +
    "                        </select>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "            <div class=\"col-md-6\" style=\"padding-left:25px\">\r" +
    "\n" +
    "                <div class=\"row\">\r" +
    "\n" +
    "                    <h3 class=\"dotted-underline\">Student Attributes</h3>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"row filterRowMargin\" ng-repeat=\"attributeType in filterOptions.attributeTypes\">\r" +
    "\n" +
    "                    <label class=\"col-sm-4 control-label\">{{attributeType.Name}}</label>\r" +
    "\n" +
    "                    <div class=\"col-sm-8\">\r" +
    "\n" +
    "                        <div ui-select2=\"{minimumInputLength: 0, data: attributeType.DropDownData, multiple: true, width: 'resolve'}\" ng-model=\"attributeType.selectedData\" data-placeholder=\"- All {{attributeType.Name}} -\" style=\"width:100%\"></div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "        </div>\r" +
    "\n" +
    "    </div>\r" +
    "\n" +
    "\r" +
    "\n" +
    "<style>\r" +
    "\n" +
    "    /*.tab-content {\r" +
    "\n" +
    "        height: 310px;\r" +
    "\n" +
    "    }*/\r" +
    "\n" +
    "</style>"
  );


  $templateCache.put('templates/stacked-bar-graph-legend.html',
    "<div class=\"nsLegend\">\r" +
    "\n" +
    "    <h3 style=\"text-align:center\">{{title}} Selected Options</h3>\r" +
    "\n" +
    "    <table style=\"margin:auto\">\r" +
    "\n" +
    "        <tr>\r" +
    "\n" +
    "            <td style=\"padding-right:20px;\">\r" +
    "\n" +
    "                <table>\r" +
    "\n" +
    "                    <tr>\r" +
    "\n" +
    "                        <td class=\"legendHeading\">School Year</td>\r" +
    "\n" +
    "                        <td class=\"legendText\">{{schoolYear()}}</td>\r" +
    "\n" +
    "                    </tr>\r" +
    "\n" +
    "                    <tr>\r" +
    "\n" +
    "                        <td class=\"legendHeading\">Schools</td>\r" +
    "\n" +
    "                        <td class=\"legendText\">{{schools()}}</td>\r" +
    "\n" +
    "                    </tr>\r" +
    "\n" +
    "                    <tr>\r" +
    "\n" +
    "                        <td class=\"legendHeading\">Grades</td>\r" +
    "\n" +
    "                        <td class=\"legendText\">{{grades()}}</td>\r" +
    "\n" +
    "                    </tr>\r" +
    "\n" +
    "                    <tr>\r" +
    "\n" +
    "                        <td class=\"legendHeading\">Teachers</td>\r" +
    "\n" +
    "                        <td class=\"legendText\">{{teachers()}}</td>\r" +
    "\n" +
    "                    </tr>\r" +
    "\n" +
    "                    <tr>\r" +
    "\n" +
    "                        <td class=\"legendHeading\">Sections</td>\r" +
    "\n" +
    "                        <td class=\"legendText\">{{sections()}}</td>\r" +
    "\n" +
    "                    </tr>\r" +
    "\n" +
    "                    <tr>\r" +
    "\n" +
    "                        <td class=\"legendHeading\">Intervention Types</td>\r" +
    "\n" +
    "                        <td class=\"legendText\">{{interventionTypes()}}</td>\r" +
    "\n" +
    "                    </tr>\r" +
    "\n" +
    "                    <tr>\r" +
    "\n" +
    "                        <td class=\"legendHeading\">Assessment Field</td>\r" +
    "\n" +
    "                        <td class=\"legendText\">{{assessmentField()}}</td>\r" +
    "\n" +
    "                    </tr>\r" +
    "\n" +
    "                    <tr ng-if=\"summaryMode\">\r" +
    "\n" +
    "                        <td class=\"legendHeading\">Zone</td>\r" +
    "\n" +
    "                        <td class=\"legendText\">{{scoreGrouping()}}</td>\r" +
    "\n" +
    "                    </tr>\r" +
    "\n" +
    "                    <tr ng-if=\"summaryMode\">\r" +
    "\n" +
    "                        <td class=\"legendHeading\">Stack</td>\r" +
    "\n" +
    "                        <td class=\"legendText\">{{whichStack()}}</td>\r" +
    "\n" +
    "                    </tr>\r" +
    "\n" +
    "                </table>\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "            <td>\r" +
    "\n" +
    "                <table>\r" +
    "\n" +
    "                    <tr ng-repeat=\"attributeType in options.attributeTypes\">\r" +
    "\n" +
    "                        <td class=\"legendHeading\">{{attributeType.Name}}</td>\r" +
    "\n" +
    "                        <td class=\"legendText\">{{attributeValue(attributeType.Name, attributeType.selectedData)}}</td>\r" +
    "\n" +
    "                    </tr>\r" +
    "\n" +
    "                </table>\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "    </table>\r" +
    "\n" +
    "</div>"
  );


  $templateCache.put('templates/student-interventions.html',
    "<table class=\"table table-striped\">\r" +
    "\n" +
    "    <thead>\r" +
    "\n" +
    "        <tr>\r" +
    "\n" +
    "            <th>\r" +
    "\n" +
    "                Tier\r" +
    "\n" +
    "            </th>\r" +
    "\n" +
    "            <th>\r" +
    "\n" +
    "                Type\r" +
    "\n" +
    "            </th>\r" +
    "\n" +
    "            <th>\r" +
    "\n" +
    "                Description\r" +
    "\n" +
    "            </th>\r" +
    "\n" +
    "            <th>\r" +
    "\n" +
    "                # of Lessons\r" +
    "\n" +
    "            </th>\r" +
    "\n" +
    "            <th>\r" +
    "\n" +
    "                Start Date\r" +
    "\n" +
    "            </th>\r" +
    "\n" +
    "            <th>\r" +
    "\n" +
    "                End Date\r" +
    "\n" +
    "            </th>\r" +
    "\n" +
    "            <th>\r" +
    "\n" +
    "                Staff Initials\r" +
    "\n" +
    "            </th>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "    </thead>\r" +
    "\n" +
    "    <tbody>\r" +
    "\n" +
    "        <tr ng-repeat=\"intervention in dataManager.Interventions\">\r" +
    "\n" +
    "            <td>{{::intervention.Tier}}</td>\r" +
    "\n" +
    "            <td>{{::intervention.InterventionType}}</td>\r" +
    "\n" +
    "            <td>{{::intervention.Description}}</td>\r" +
    "\n" +
    "            <td>{{::intervention.NumLessons}}</td>\r" +
    "\n" +
    "            <td>{{::dataManager.formatDate2(intervention.StartOfIntervention)}}</td>\r" +
    "\n" +
    "            <td>{{::dataManager.formatDate2(intervention.EndOfIntervention)}}</td>\r" +
    "\n" +
    "            <td>{{::intervention.StaffInitials}}</td>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "    </tbody>\r" +
    "\n" +
    "</table>"
  );


  $templateCache.put('templates/teammeeting-assign-student-to-ig.html',
    "<div class=\"col-md-12\" ng-if=\"display\">\r" +
    "\n" +
    "    <panel panel-class=\"panel-primary\" heading=\"Team Meeting: {{::teamMeetingName}}\">\r" +
    "\n" +
    "        <panel-controls>\r" +
    "\n" +
    "            <a href=\"\" ng-click=\"backToTeamMeeting(teamMeetingId)\"><i class=\"fa fa-chevron-left\"></i><i class=\"fa fa-chevron-left\"></i> Back to Team Meeting</a>\r" +
    "\n" +
    "        </panel-controls>\r" +
    "\n" +
    "        <h2 style=\"text-align:center;color:white;\">Assigning Student: <b>{{::studentName}}</b></h2>\r" +
    "\n" +
    "        <small style=\"display:block;text-align:center;color:white\">to Intervention Group from Team Meeting</small>\r" +
    "\n" +
    "    </panel>\r" +
    "\n" +
    "</div>\r" +
    "\n" +
    "<style>\r" +
    "\n" +
    "    .panel-primary .panel-body{\r" +
    "\n" +
    "        background-color: #4697ce !important;\r" +
    "\n" +
    "        border-left: none !important;\r" +
    "\n" +
    "        border-right: none !important;\r" +
    "\n" +
    "        border-bottom: none !important;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "</style>"
  );


  $templateCache.put('templates/themed-tabs-bottom.html',
    "<div class=\"tab-container tab-{{theme || 'primary'}} tab-{{position || 'normal'}}\">\n" +
    "  <div class=\"tab-content\">\n" +
    "    <div class=\"tab-pane\"\n" +
    "        ng-repeat=\"tab in tabs\"\n" +
    "        ng-class=\"{active: tab.active}\"\n" +
    "        tab-content-transclude=\"tab\">\n" +
    "    </div>\n" +
    "  </div>\n" +
    "  <ul class=\"nav nav-{{type || 'tabs'}}\" ng-class=\"{'nav-stacked': vertical, 'nav-justified': justified}\" ng-transclude></ul>\n" +
    "</div>\n"
  );


  $templateCache.put('templates/themed-tabs.html',
    "<div class=\"tab-container tab-{{theme || 'primary'}} tab-{{position || 'normal'}}\">\n" +
    "  <ul class=\"nav nav-{{type || 'tabs'}}\" ng-class=\"{'nav-stacked': vertical, 'nav-justified': justified}\" ng-transclude></ul>\n" +
    "  <div class=\"tab-content\">\n" +
    "    <div class=\"tab-pane\"\n" +
    "        ng-repeat=\"tab in tabs\"\n" +
    "        ng-class=\"{active: tab.active}\"\n" +
    "        uib-tab-content-transclude=\"tab\">\n" +
    "    </div>\n" +
    "  </div>\n" +
    "</div>\n"
  );


  $templateCache.put('templates/tile-generic.html',
    "<div class=\"info-tiles tiles-{{type}}\">\n" +
    "\t<div class=\"tiles-heading\">\n" +
    "\t\t{{heading}}\n" +
    "\t</div>\n" +
    "\t<div class=\"tiles-body\" ng-transclude>\n" +
    "\t</div>\n" +
    "</div>\n"
  );


  $templateCache.put('templates/tile-large.html',
    "<a class=\"info-tiles tiles-{{item.color}}\" ng-href=\"{{item.href}}\">\n" +
    "    <div class=\"tiles-heading\">\n" +
    "        <div class=\"pull-left\">{{item.title}}</div>\n" +
    "        <div class=\"pull-right\">{{item.titleBarInfo}}</div>\n" +
    "    </div>\n" +
    "    <div class=\"tiles-body\">\n" +
    "        <div class=\"pull-left\"><i class=\"{{item.classes}}\"></i></div>\n" +
    "        <div class=\"pull-right\" ng-show=\"item.text\">{{item.text}}</div>\n" +
    "        <div class=\"pull-right\" ng-show=\"!item.text\" ng-transclude></div>\n" +
    "    </div>\n" +
    "</a>\n"
  );


  $templateCache.put('templates/tile-mini.html',
    "<a class=\"shortcut-tiles tiles-{{item.color}}\" ng-href=\"{{item.href}}\">\n" +
    "\t<div class=\"tiles-body\">\n" +
    "\t\t<div class=\"pull-left\"><i class=\"{{item.classes}}\"></i></div>\n" +
    "\t\t<div class=\"pull-right\"><span class=\"badge\">{{item.titleBarInfo}}</span></div>\n" +
    "\t</div>\n" +
    "\t<div class=\"tiles-footer\">\n" +
    "\t\t{{item.text}}\n" +
    "\t</div>\n" +
    "</a>\n"
  );
}])