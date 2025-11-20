angular.module('theme.templates', []).run(['$templateCache', function ($templateCache) {
  'use strict';

  $templateCache.put('templates/active-intervention-dashboard-link.html',
    "<a class=\"badge badge-danger\" href=\"javascript:;\" ng-click=\"openInterventionDashboardDialog(schoolYear, schoolId, interventionistId, groupId, studentId, interventionId)\">{{interventionType}} - {{staffInitials}}</a>"
  );


  $templateCache.put('templates/assessment-calculatedfield-grideditable.html',
    "<span ng-bind=\"Calculate(result.Field.CalculationFields)\"></span>"
  );


  $templateCache.put('templates/assessment-calculatedfield-readonly.html',
    "<span ng-bind=\"::Calculate(result.Field.CalculationFields)\"></span> <span ng-if=\"result.IsCopied\"> *</span>"
  );


  $templateCache.put('templates/assessment-calculatedfield.html',
    "<span ng-bind=\"Calculate(result.Field.CalculationFields)\"></span>"
  );


  $templateCache.put('templates/assessment-calculatedfieldclientonly-readonly.html',
    "<span ng-bind-html=\"Calculate(result.Field.CalculationFields) | safe_html\"></span> <span ng-if=\"result.IsCopied\"> *</span>"
  );


  $templateCache.put('templates/assessment-calculatedfielddbbacked-custom.html',
    "<span ng-bind=\"Calculate(currentField.CalculationFields)\"></span>"
  );


  $templateCache.put('templates/assessment-calculatedfielddbbacked-grideditable.html',
    "<span>{{::result.IntValue}}</span>"
  );


  $templateCache.put('templates/assessment-calculatedfielddbbacked-readonly.html',
    "<span ng-bind=\"::result.IntValue\"></span> <span ng-if=\"result.IsCopied\"> *</span>"
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
    "<span ng-bind=\"::result.StringValue\"></span> <span ng-if=\"result.IsCopied\"> *</span>"
  );


  $templateCache.put('templates/assessment-calculatedfielddbbackedstring.html',
    "<span ng-bind=\"result.StringValue\"></span>"
  );


  $templateCache.put('templates/assessment-calculatedfielddbonly-grideditable.html',
    "<span ng-bind=\"result.StringValue\"></span>"
  );


  $templateCache.put('templates/assessment-calculatedfielddbonly-readonly.html',
    "<span ng-bind=\"::result.StringValue\"></span> <span ng-if=\"result.IsCopied\"> *</span>"
  );


  $templateCache.put('templates/assessment-calculatedfielddbonly.html',
    "<span ng-bind=\"result.StringValue\"></span>"
  );


  $templateCache.put('templates/assessment-checkbox-custom.html',
    "<table>\r" +
    "\n" +
    "    <tr>\r" +
    "\n" +
    "        <td><switch class=\"green\" ng-change=\"checkClick(fieldResult.BoolValue, currentField.CalculationFields, fieldResult)\" ng-model=\"fieldResult.BoolValue\"></switch></td>\r" +
    "\n" +
    "        <td class=\"switchLabel\">{{::currentField.DisplayLabel}}</td>\r" +
    "\n" +
    "    </tr>\r" +
    "\n" +
    "</table>\r" +
    "\n"
  );


  $templateCache.put('templates/assessment-checkbox-grideditable.html',
    "<span e-form=\"eForm\" e-title=\"Yes?\" editable-checkbox=\"result.BoolValue\">{{ result.BoolValue != null ? (result.BoolValue && 'Yes' || 'No') : '' }}</span>"
  );


  $templateCache.put('templates/assessment-checkbox-readonly.html',
    "<div ng-if=\"fieldResult.BoolValue == true\"><i class=\"fa fa-check\" style=\"color:green\"></i></div>\r" +
    "\n" +
    "<div ng-if=\"fieldResult.BoolValue != true\"><i class=\"fa fa-times\" style=\"color:red\"></i></div>"
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


  $templateCache.put('templates/assessment-checklist-grideditable.html',
    "<div class=\"checklist-vertical\">\r" +
    "\n" +
    "    <span editable-checklist=\"result.ChecklistValues\" e-ng-required=\"false\" e-form=\"eForm\" e-ng-options=\"g.FieldSpecificId as g.FieldValue for g in lookupValues\">{{ result.DisplayValue || '' }}</span>\r" +
    "\n" +
    "</div>\r" +
    "\n"
  );


  $templateCache.put('templates/assessment-checklist-readonly.html',
    "<span>{{ ::result.DisplayValue || '' }}</span>"
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


  $templateCache.put('templates/assessment-decimalrange-custom.html',
    "<input type=\"number\" class=\"form-control\" min=\"{{currentField.RangeLow}}\" max=\"{{currentField.RangeHigh}}\" ng-model=\"fieldResult.DecimalValue\" />\r" +
    "\n" +
    "\r" +
    "\n"
  );


  $templateCache.put('templates/assessment-decimalrange-grideditable.html',
    "<span ng-show=\"result.DecimalValue != null && !eForm.$visible\">{{result.Field.AltDisplayLabel}}</span><span e-step=\"any\" e-style=\"min-width:65px\" e-form=\"eForm\" editable-number=\"result.DecimalValue\"  e-min=\"{{result.Field.RangeLow}}\" e-max=\"{{result.Field.RangeHigh}}\" >{{ result.DecimalValue != null ? result.DecimalValue : '' }}</span>"
  );


  $templateCache.put('templates/assessment-decimalrange-readonly.html',
    "<span>{{ ::result.DecimalValue != null ? result.DecimalValue : '' }}</span> <span ng-if=\"result.IsCopied\"> *</span>"
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
    "<span ng-class=\"::guidedReadingCheck(result.IntValue, result.DbColumn)\">{{:: result.DisplayValue || '' }}</span> <span ng-if=\"result.IsCopied\"> *</span>"
  );


  $templateCache.put('templates/assessment-dropdownfromdb.html',
    "<span editable-select=\"result.IntValue\" e-form=\"eForm\" e-ng-options=\"g.FieldSpecificId as g.FieldValue for g in lookupValues\">{{ result.DisplayValue || '' }}</span>"
  );


  $templateCache.put('templates/assessment-dropdownrange-grideditable.html',
    "<span editable-select=\"result.IntValue\" e-form=\"eForm\" e-ng-options=\"n for n in [] | range:result.Field.RangeLow:result.Field.RangeHigh\">{{ result.IntValue != null ? result.IntValue : '' }}</span>"
  );


  $templateCache.put('templates/assessment-dropdownrange-readonly.html',
    "<span>{{ ::result.IntValue != null ? result.IntValue : '' }}</span> <span ng-if=\"result.IsCopied\"> *</span>"
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
    "<span>{{:: result.IntValue || '' }}</span>"
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
    "<span ng-if=\"eForm.$visible\">\r" +
    "\n" +
    "    <textarea name=\"ckeditor_studentNote\" id=\"studentNote_NewNote\" cols=\"80\" rows=\"20\" class=\"ckeditor\" ng-model=\"result.StringValue\" ckeditor=\"ckeditorOptions\"></textarea>\r" +
    "\n" +
    "</span>\r" +
    "\n" +
    "<i ng-if=\"result.StringValue && !eForm.$visible && !displayTextAreas\" class=\"fa fa-comments\" style=\"margin-left:5px;cursor:pointer\" ng-click=\"commentsModal(result.StringValue)\"></i>\r" +
    "\n" +
    "<span ng-if=\"displayTextAreas && !eForm.$visible\" ng-bind-html=\"result.StringValue | safe_html\"></span>\r" +
    "\n" +
    "<!--<span editable-textarea=\"result.StringValue\" e-form=\"eForm\" e-rows=\"5\" e-cols=\"80\"><i ng-if=\"result.StringValue\" popover-trigger=\"mouseenter\" class=\"fa fa-comments\" style=\"margin-left:5px;cursor:pointer\" uib-popover-html=\"toolTipFunction()\" popover-title=\"Comments\"></i></span>-->"
  );


  $templateCache.put('templates/assessment-textarea-readonly.html',
    "<i ng-if=\"result.StringValue && result.StringValue != '' && !displayTextAreas\" class=\"fa fa-comments\" style=\"margin-left:5px;cursor:pointer\" ng-click=\"commentsModal(result.StringValue)\"></i>\r" +
    "\n" +
    "<span ng-if=\"displayTextAreas\" ng-bind-html=\"result.StringValue | safe_html\"></span>"
  );


  $templateCache.put('templates/assessment-textfield-custom.html',
    "<input type=\"text\" class=\"form-control\" ng-model=\"fieldResult.StringValue\" />"
  );


  $templateCache.put('templates/assessment-textfield-grideditable.html',
    "<span editable-text=\"result.StringValue\" e-form=\"eForm\" >{{ result.StringValue || '' }}</span>"
  );


  $templateCache.put('templates/assessment-textfield-readonly.html',
    "<span>{{ ::result.StringValue || '' }}</span>"
  );


  $templateCache.put('templates/assessment-textfield.html',
    "<span editable-text=\"result.StringValue\" e-form=\"eForm\" >{{ result.StringValue || '' }}</span>"
  );


  $templateCache.put('templates/avmr-detail.html',
    "<div class=\"row\">\r" +
    "\n" +
    "    <table class=\"table table-striped table-bordered\">\r" +
    "\n" +
    "        <thead>\r" +
    "\n" +
    "            <tr>\r" +
    "\n" +
    "                <th>Children's Names</th>\r" +
    "\n" +
    "                <th ng-repeat=\"category in detailReportMgr.categories | orderBy : 'SortOrder'\" class=\"nsCenterAlignedText\">\r" +
    "\n" +
    "                    {{::category.DisplayName}}\r" +
    "\n" +
    "                </th>\r" +
    "\n" +
    "            </tr>\r" +
    "\n" +
    "        </thead>\r" +
    "\n" +
    "        <tbody>\r" +
    "\n" +
    "            <tr ng-repeat=\"studentResult in detailReportMgr.studentDetailResults | orderBy : 'StudentName'\">\r" +
    "\n" +
    "                <td>{{::studentResult.StudentName}}</td>\r" +
    "\n" +
    "                <td ng-repeat=\"fieldResult in studentResult.GroupFieldResults\" class=\"nsCenterAlignedText\">\r" +
    "\n" +
    "                    <ns-assessment-field mode=\"'readonly'\" display-text-areas=\"displayTextAreas\" lookup-fields-array=\"lookupFieldsArray\" result=\"fieldResult\" all-results=\"studentResult.FieldResults\"></ns-assessment-field>\r" +
    "\n" +
    "                </td>\r" +
    "\n" +
    "            </tr>\r" +
    "\n" +
    "        </tbody>\r" +
    "\n" +
    "    </table>\r" +
    "\n" +
    "</div>"
  );


  $templateCache.put('templates/benchmark-calculatedfielddbbacked.html',
    "<span e-step=\"1\" e-form=\"eForm\" editable-number=\"result.Score\">{{ result.Score || '' }}</span>"
  );


  $templateCache.put('templates/benchmark-decimalrange.html',
    "<span e-step=\"any\" e-form=\"eForm\" editable-number=\"result.Score\" e-min=\"{{field.RangeLow}}\" e-max=\"{{field.RangeHigh}}\">{{ result.Score || '' }}</span>"
  );


  $templateCache.put('templates/benchmark-dropdownfromdb.html',
    "<span editable-select=\"result.Score\" e-form=\"eForm\" e-ng-options=\"g.FieldSpecificId as g.FieldValue for g in lookupValues\">{{ result.ScoreLabel || '' }}</span>"
  );


  $templateCache.put('templates/benchmark-dropdownrange.html',
    "<span e-step=\"1\" e-form=\"eForm\" editable-number=\"result.Score\" e-min=\"{{field.RangeLow}}\" e-max=\"{{field.RangeHigh}}\">{{ result.Score || '' }}</span>\r" +
    "\n" +
    "\r" +
    "\n" +
    "<!--<span editable-select=\"result.Score\" e-form=\"eForm\" e-ng-options=\"n for n in [] | range:field.RangeLow:field.RangeHigh\">{{ result.Score || '' }}</span>-->"
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


  $templateCache.put('templates/dirPagination.tpl.html',
    "<ul class=\"pagination\" ng-if=\"1 < pages.length || !autoHide\">\r" +
    "\n" +
    "    <li ng-if=\"boundaryLinks\" ng-class=\"{ disabled : pagination.current == 1 }\">\r" +
    "\n" +
    "        <a href=\"\" ng-click=\"setCurrent(1)\">&laquo;</a>\r" +
    "\n" +
    "    </li>\r" +
    "\n" +
    "    <li ng-if=\"directionLinks\" ng-class=\"{ disabled : pagination.current == 1 }\">\r" +
    "\n" +
    "        <a href=\"\" ng-click=\"setCurrent(pagination.current - 1)\">&lsaquo;</a>\r" +
    "\n" +
    "    </li>\r" +
    "\n" +
    "    <li ng-repeat=\"pageNumber in pages track by tracker(pageNumber, $index)\" ng-class=\"{ active : pagination.current == pageNumber, disabled : pageNumber == '...' }\">\r" +
    "\n" +
    "        <a href=\"\" ng-click=\"setCurrent(pageNumber)\">{{ pageNumber }}</a>\r" +
    "\n" +
    "    </li>\r" +
    "\n" +
    "\r" +
    "\n" +
    "    <li ng-if=\"directionLinks\" ng-class=\"{ disabled : pagination.current == pagination.last }\">\r" +
    "\n" +
    "        <a href=\"\" ng-click=\"setCurrent(pagination.current + 1)\">&rsaquo;</a>\r" +
    "\n" +
    "    </li>\r" +
    "\n" +
    "    <li ng-if=\"boundaryLinks\" ng-class=\"{ disabled : pagination.current == pagination.last }\">\r" +
    "\n" +
    "        <a href=\"\" ng-click=\"setCurrent(pagination.last)\">&raquo;</a>\r" +
    "\n" +
    "    </li>\r" +
    "\n" +
    "</ul>"
  );


  $templateCache.put('templates/egAppStatus.html',
    "<div class=\"alert\" style=\"height: 50px;\" ng-class=\"{'alert-warning': status.busy, 'alert-info': !status.busy}\">\r" +
    "\n" +
    "    {{status.message}}\r" +
    "\n" +
    "</div>\r" +
    "\n"
  );


  $templateCache.put('templates/egphotouploader.html',
    "<form name=\"newPhotosForm\" role=\"form\" enctype=\"multipart/form-data\" ng-disabled=\"appStatus.busy || photoManagerStatus.uploading\">\r" +
    "\n" +
    "    <div class=\"form-group\" ng-hide=\"hasFiles\">\r" +
    "\n" +
    "        <label for=\"newPhotos\">select and upload new photos</label>\r" +
    "\n" +
    "        <input type=\"file\" id=\"newPhotos\" class=\"uploadFile\" accept=\"image/*\" eg-files=\"photos\" has-files=\"hasFiles\" multiple>\r" +
    "\n" +
    "    </div>\r" +
    "\n" +
    "    <div class=\"form-group\" ng-show=\"hasFiles && !photoManagerStatus.uploading\">\r" +
    "\n" +
    "        <ul class=\"list-inline\">\r" +
    "\n" +
    "            <li><strong>files:</strong></li>\r" +
    "\n" +
    "            <li ng-repeat=\"photo in photos\"> {{photo.name}}</li>\r" +
    "\n" +
    "        </ul>\r" +
    "\n" +
    "        <input class=\"btn btn-primary\" type=\"button\" eg-upload=\"upload(photos)\" value=\"upload\">\r" +
    "\n" +
    "        <input class=\"btn btn-warning\" type=\"reset\" value=\"cancel\" />\r" +
    "\n" +
    "    </div>\r" +
    "\n" +
    "    <div class=\"form-group\" ng-show=\"photoManagerStatus.uploading\">\r" +
    "\n" +
    "        <p class=\"help-block\">uploading</p>\r" +
    "\n" +
    "    </div>\r" +
    "\n" +
    "</form>"
  );


  $templateCache.put('templates/error.html',
    "<div class=\"container-fluid\">\r" +
    "\n" +
    "\r" +
    "\n" +
    "    <div class=\"row\">\r" +
    "\n" +
    "        <div class=\"col-md-12\">\r" +
    "\n" +
    "            <p class=\"text-center\">\r" +
    "\n" +
    "                <span class=\"text-danger\" style=\"font-size:4em;\">Sorry</span>\r" +
    "\n" +
    "            </p>\r" +
    "\n" +
    "            <p class=\"text-center\">An error has occurred while processing your request.</p>\r" +
    "\n" +
    "            <p class=\"text-center\">We are looking into the issue. Please try again later.</p>\r" +
    "\n" +
    "        </div>\r" +
    "\n" +
    "    </div>\r" +
    "\n" +
    "\r" +
    "\n" +
    "</div> "
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
    "                <div class=\"col-md-3\" ng-if=\"filterOptions.select2MultiBenchmarkDateOptions && filterOptions.multiBenchmarkDateEnabled\">\r" +
    "\n" +
    "                    <div ui-select2=\"filterOptions.select2MultiBenchmarkDateOptions\" ng-change=\"changeMultiBenchmarkDates(filterOptions.selectedBenchmarkDates, {{filterOptions.selectedBenchmarkDates}})\" ng-model=\"filterOptions.selectedBenchmarkDates\" data-placeholder=\"- Select Benchmark Dates -\" style=\"width:100%\"></div>\r" +
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
    "                <!--<div class=\"col-md-4\" ng-show=\"teamMeetingEnabled\">\r" +
    "\n" +
    "                    <select class=\"form-control\" ng-options=\"item.text for item in filterOptions.teamMeetings\" ng-model=\"filterOptions.selectedTeamMeeting\" ng-change=\"changeTeamMeeting()\">\r" +
    "\n" +
    "                        <option value=\"\">-team meeting-</option>\r" +
    "\n" +
    "                    </select>\r" +
    "\n" +
    "                </div>-->\r" +
    "\n" +
    "                <div class=\"col-md-2\" ng-show=\"teamMeetingStaffEnabled\">\r" +
    "\n" +
    "                    <select class=\"form-control\" ng-options=\"item.text for item in filterOptions.teamMeetingStaffs\" ng-model=\"filterOptions.selectedTeamMeetingStaff\">\r" +
    "\n" +
    "                        <option value=\"\">-all attendees-</option>\r" +
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
    "                <div class=\"col-md-2\" ng-show=\"sectionStudentEnabled\">\r" +
    "\n" +
    "                    <select class=\"form-control\" ng-model=\"filterOptions.selectedSectionStudent\" ng-options=\"item.text for item in filterOptions.sectionStudents\" ng-change=\"changeSectionStudent()\">\r" +
    "\n" +
    "                        <option value=\"\">-student-</option>\r" +
    "\n" +
    "                    </select>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"col-md-2\" ng-show=\"interventionStudentEnabled\">\r" +
    "\n" +
    "                    <select class=\"form-control\" ng-model=\"filterOptions.selectedInterventionStudent\" ng-options=\"item.text for item in filterOptions.interventionStudents\" ng-change=\"changeInterventionStudent()\">\r" +
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
    "                    <select class=\"form-control\" ng-model=\"filterOptions.selectedHrsForm\" ng-options=\"item.text for item in filterOptions.hrsForms\" ng-change=\"changeHrsForm()\">\r" +
    "\n" +
    "                        <option value=\"\">-form-</option>\r" +
    "\n" +
    "                    </select>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"col-md-1\" ng-show=\"hrsForm2Enabled\">\r" +
    "\n" +
    "                    <select class=\"form-control\" ng-model=\"filterOptions.selectedHrsForm2\" ng-options=\"item.text for item in filterOptions.hrsForms2\" ng-change=\"changeHrsForm2()\">\r" +
    "\n" +
    "                        <option value=\"\">-form-</option>\r" +
    "\n" +
    "                    </select>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"col-md-1\" ng-show=\"hrsForm3Enabled\">\r" +
    "\n" +
    "                    <select class=\"form-control\" ng-model=\"filterOptions.selectedHrsForm3\" ng-options=\"item.text for item in filterOptions.hrsForms3\" ng-change=\"changeHrsForm3()\">\r" +
    "\n" +
    "                        <option value=\"\">-form-</option>\r" +
    "\n" +
    "                    </select>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"col-md-1\" ng-show=\"stateTestEnabled\">\r" +
    "\n" +
    "                    <select class=\"form-control\" ng-model=\"filterOptions.selectedStateTest\" ng-options=\"item.text for item in filterOptions.stateTests\">\r" +
    "\n" +
    "                        <option value=\"\">-state test-</option>\r" +
    "\n" +
    "                    </select>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"col-md-1\" ng-show=\"benchmarkTestEnabled\">\r" +
    "\n" +
    "                    <select class=\"form-control\" ng-model=\"filterOptions.selectedBenchmarkTest\" ng-options=\"item.text for item in filterOptions.benchmarkTests\">\r" +
    "\n" +
    "                        <option value=\"\">-benchmark test-</option>\r" +
    "\n" +
    "                    </select>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"col-md-1\" ng-show=\"interventionTestEnabled\">\r" +
    "\n" +
    "                    <select class=\"form-control\" ng-model=\"filterOptions.selectedInterventionTest\" ng-options=\"item.text for item in filterOptions.interventionTests\">\r" +
    "\n" +
    "                        <option value=\"\">-intervention test-</option>\r" +
    "\n" +
    "                    </select>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "            <hr ng-if=\"showHr\" />\r" +
    "\n" +
    "            <div>\r" +
    "\n" +
    "                <div class=\"col-md-3\" ng-show=\"hfwRangeEnabled\">\r" +
    "\n" +
    "                    <select class=\"form-control\" ng-model=\"filterOptions.selectedHfwRange\">\r" +
    "\n" +
    "                        <option value=\"\">-HFW Range-</option>\r" +
    "\n" +
    "                        <option value=\"kdg\">Kindergarten</option>\r" +
    "\n" +
    "                        <option value=\"1-100\">1-100</option>\r" +
    "\n" +
    "                        <option value=\"101-200\">101-200</option>\r" +
    "\n" +
    "                        <option value=\"201-300\">201-300</option>\r" +
    "\n" +
    "                        <option value=\"301-400\">301-400</option>\r" +
    "\n" +
    "                        <option value=\"401-500\">401-500</option>\r" +
    "\n" +
    "                        <option value=\"501-600\">501-600</option>\r" +
    "\n" +
    "                        <option value=\"601-700\">601-700</option>\r" +
    "\n" +
    "                        <option value=\"701-800\">701-800</option>\r" +
    "\n" +
    "                        <option value=\"801-900\">801-900</option>\r" +
    "\n" +
    "                        <option value=\"901-1000\">901-1000</option>\r" +
    "\n" +
    "                    </select>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"col-md-3\" ng-show=\"hfwMultiRangeEnabled\">\r" +
    "\n" +
    "                    <div ui-select2=\"HfwMultiRangeRemoteOptions\" ng-model=\"filterOptions.selectedHfwMultiRange\" ng-change=\"changeHfwMultiRange()\" />\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"col-md-3\" ng-show=\"hfwSortOrderEnabled\">\r" +
    "\n" +
    "                    <select class=\"form-control\" ng-options=\"item.text for item in filterOptions.hfwOrders\" ng-model=\"filterOptions.selectedHfwSortOrder\" ng-change=\"changeHfwSortOrder()\">\r" +
    "\n" +
    "                    </select>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <hr ng-if=\"staffQuickSearchEnabled || studentQuickSearchEnabled || studentDetailedQuickSearchEnabled\" style=\"clear:both\" />\r" +
    "\n" +
    "                <div class=\"col-md-12\" ng-if=\"staffQuickSearchEnabled\">\r" +
    "\n" +
    "                    <div class=\"col-md-2\">\r" +
    "\n" +
    "                        <span style=\"text-transform: Uppercase; font-weight: bold; margin-right: 10px;\">{{staffSearchCustomLabel || 'Staff Quick Search'}}:</span>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                    <div class=\"col-md-10\">\r" +
    "\n" +
    "                        <table style=\"width:700px;\">\r" +
    "\n" +
    "                            <tr>\r" +
    "\n" +
    "                                <td><div ui-select2=\"StaffQuickSearchRemoteOptions\" ng-model=\"filterOptions.quickSearchStaff\" /></td>\r" +
    "\n" +
    "                                <td style=\"width:90px;padding-left:10px;\"><button class=\"btn btn-primary\" ng-click=\"qsCallBack()\"><i class=\"fa fa-check-square-o\"></i> Select</button></td>\r" +
    "\n" +
    "                                <td style=\"width:90px;padding-left:10px;\"><button class=\"btn btn-danger\" ng-click=\"clearQsStaff()\"><i class=\"fa fa-refresh\"></i> Reset</button></td>\r" +
    "\n" +
    "                            </tr>\r" +
    "\n" +
    "                        </table>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                 </div>\r" +
    "\n" +
    "                <div class=\"col-md-12\" ng-if=\"studentQuickSearchEnabled\">\r" +
    "\n" +
    "                    <div class=\"col-md-2\">\r" +
    "\n" +
    "                        <span style=\"text-transform: Uppercase; font-weight: bold; margin-right: 10px;\">Student Quick Search:</span>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                    <div class=\"col-md-10\">\r" +
    "\n" +
    "                        <table style=\"width:700px;\">\r" +
    "\n" +
    "                            <tr>\r" +
    "\n" +
    "                                <td><div ui-select2=\"StudentQuickSearchRemoteOptions\" ng-model=\"filterOptions.quickSearchStudent\" /></td>\r" +
    "\n" +
    "                                <td style=\"width:90px;padding-left:10px;\"><button class=\"btn btn-primary\" ng-click=\"qsCallBack()\"><i class=\"fa fa-check-square-o\"></i> Select</button></td>\r" +
    "\n" +
    "                                <td style=\"width:90px;padding-left:10px;\"><button class=\"btn btn-danger\" ng-click=\"clearQsStudent()\"><i class=\"fa fa-refresh\"></i> Reset</button></td>\r" +
    "\n" +
    "                            </tr>\r" +
    "\n" +
    "                        </table>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"col-md-12\" ng-if=\"studentDetailedQuickSearchEnabled\">\r" +
    "\n" +
    "                    <div class=\"col-md-2\">\r" +
    "\n" +
    "                        <span style=\"text-transform: Uppercase; font-weight: bold; margin-right: 10px;\">Student Quick Search:</span>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                    <div class=\"col-md-10\">\r" +
    "\n" +
    "                        <table style=\"width:700px;\">\r" +
    "\n" +
    "                            <tr>\r" +
    "\n" +
    "                                <td><div ui-select2=\"StudentDetailedQuickSearchRemoteOptions\" ng-model=\"filterOptions.quickSearchStudent\" /></td>\r" +
    "\n" +
    "                                <td style=\"width:90px;padding-left:10px;\"><button class=\"btn btn-primary\" ng-click=\"qsCallBack()\"><i class=\"fa fa-check-square-o\"></i> Select</button></td>\r" +
    "\n" +
    "                                <td style=\"width:90px;padding-left:10px;\"><button class=\"btn btn-danger\" ng-click=\"clearQsStudent()\"><i class=\"fa fa-refresh\"></i> Reset</button></td>\r" +
    "\n" +
    "                            </tr>\r" +
    "\n" +
    "                        </table>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"col-md-12\" ng-if=\"classroomAssessmentFieldEnabled\">\r" +
    "\n" +
    "                    <div class=\"col-md-2\">\r" +
    "\n" +
    "                        <span style=\"text-transform: Uppercase; font-weight: bold; margin-right: 10px;\">Assessment Field:</span>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                    <div class=\"col-md-10\">\r" +
    "\n" +
    "                        <select ng-change=\"changeClassroomAssessmentField()\" class=\"form-control\" ng-model=\"filterOptions.selectedClassroomAssessmentField\" ng-options=\"field.DisplayLabel group by field.AssessmentName for field in filterOptions.classroomAssessmentFields | filter: {DisplayInLineGraphs : true}\">\r" +
    "\n" +
    "                            <option value=\"\"> -- Choose a Field -- </option>\r" +
    "\n" +
    "                        </select>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"col-md-12\" ng-if=\"interventionGroupAssessmentFieldEnabled\">\r" +
    "\n" +
    "                    <div class=\"col-md-2\">\r" +
    "\n" +
    "                        <span style=\"text-transform: Uppercase; font-weight: bold; margin-right: 10px;\">Assessment Field:</span>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                    <div class=\"col-md-10\">\r" +
    "\n" +
    "                        <select ng-change=\"changeInterventionGroupAssessmentField()\" class=\"form-control\" ng-model=\"filterOptions.selectedInterventionGroupAssessmentField\" ng-options=\"field.DisplayLabel group by field.AssessmentName for field in filterOptions.interventionGroupAssessmentFields | filter: {DisplayInLineGraphs : true}\">\r" +
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
    "    <div class=\"form-group\" ng-show=\"sectionStudentEnabled\">\r" +
    "\n" +
    "        <label for=\"fieldname\" class=\"col-md-3 control-label\">Student</label>\r" +
    "\n" +
    "        <div class=\"col-md-6\">\r" +
    "\n" +
    "            <select name=\"verticalStudent\" class=\"form-control\" ng-model=\"filterOptions.selectedSectionStudent\" ng-options=\"item.text for item in filterOptions.sectionStudents\"></select>\r" +
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


  $templateCache.put('templates/hfw-student-footer.html',
    "\r" +
    "\n" +
    "                  {{getFieldDisplayTest(options.selectedSchoolYear.text, '-not selected-', options.quickSearchStudent.SchoolYearVerbose)}} / \r" +
    "\n" +
    "                   {{getFieldDisplayTest(options.selectedSchool.text, '-not selected-', options.quickSearchStudent.SchoolName)}} / \r" +
    "\n" +
    "                    {{getFieldDisplayTest(options.selectedGrade.text, '-not selected-', options.quickSearchStudent.GradeName)}} /\r" +
    "\n" +
    "                   {{getFieldDisplayTest(options.selectedTeacher.text, '-not selected-', options.quickSearchStudent.StaffName)}} /\r" +
    "\n" +
    "                 {{getFieldDisplayTest(options.selectedSection.text, '-not selected-', options.quickSearchStudent.SectionName)}} /\r" +
    "\n" +
    "               {{getFieldDisplayTest(options.selectedSectionStudent.text, '-not selected-', options.quickSearchStudent.LastName + ', ' + options.quickSearchStudent.FirstName)}}\r" +
    "\n"
  );


  $templateCache.put('templates/intervention-dashboard-directive.html',
    "<div style=\"min-height:350px;\">\r" +
    "\n" +
    "    <spinner name=\"tableSpinner\">\r" +
    "\n" +
    "        <div style=\"position:absolute;width:100%;height:100%;background-color:rgba(100, 100, 100, 0.7);z-index:100;\">\r" +
    "\n" +
    "            <div class=\"sk-cube-grid\">\r" +
    "\n" +
    "                <div class=\"sk-cube sk-cube1\"></div>\r" +
    "\n" +
    "                <div class=\"sk-cube sk-cube2\"></div>\r" +
    "\n" +
    "                <div class=\"sk-cube sk-cube3\"></div>\r" +
    "\n" +
    "                <div class=\"sk-cube sk-cube4\"></div>\r" +
    "\n" +
    "                <div class=\"sk-cube sk-cube5\"></div>\r" +
    "\n" +
    "                <div class=\"sk-cube sk-cube6\"></div>\r" +
    "\n" +
    "                <div class=\"sk-cube sk-cube7\"></div>\r" +
    "\n" +
    "                <div class=\"sk-cube sk-cube8\"></div>\r" +
    "\n" +
    "                <div class=\"sk-cube sk-cube9\"></div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "        </div>\r" +
    "\n" +
    "    </spinner>\r" +
    "\n" +
    "    <div class=\"col-md-12\">\r" +
    "\n" +
    "        <uib-tabset>\r" +
    "\n" +
    "            <uib-tab>\r" +
    "\n" +
    "                <uib-tab-heading>\r" +
    "\n" +
    "                    <i class=\"fa fa-line-chart\"></i> Progress Monitoring Line Graphs\r" +
    "\n" +
    "                </uib-tab-heading>\r" +
    "\n" +
    "                <div>\r" +
    "\n" +
    "                    <div ng-repeat=\"fieldMgr in ClassLineGraphDataManagers\" style=\"margin-bottom:50px;\">\r" +
    "\n" +
    "                        <div class=\"col-md-12\">\r" +
    "\n" +
    "                            <div>\r" +
    "\n" +
    "                                <highchart id=\"chart2\" config=\"fieldMgr.chartConfig\"></highchart>\r" +
    "\n" +
    "                            </div>\r" +
    "\n" +
    "                            <div>\r" +
    "\n" +
    "                                <line-graph-detail-ig data-data-manager=\"fieldMgr\"></line-graph-detail-ig>\r" +
    "\n" +
    "                            </div>\r" +
    "\n" +
    "                        </div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                    <div ng-show=\"ClassLineGraphDataManagers.length == 0\">\r" +
    "\n" +
    "                        <uib-alert type=\"info\"><b>Note:</b> No assessment data recorded during this intervention.</uib-alert>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                    <div style=\"clear:both;\" />\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </uib-tab>\r" +
    "\n" +
    "            <uib-tab>\r" +
    "\n" +
    "                <uib-tab-heading>\r" +
    "\n" +
    "                    <i class=\"fa fa-pie-chart\"></i> Attendance and Notes\r" +
    "\n" +
    "                </uib-tab-heading>\r" +
    "\n" +
    "                <div class=\"col-md-12\">\r" +
    "\n" +
    "                    <div class=\"col-md-6\">\r" +
    "\n" +
    "                        <div class=\"col-md-12\">\r" +
    "\n" +
    "                            <h2>Graph</h2>\r" +
    "\n" +
    "                        </div>\r" +
    "\n" +
    "                        <div class=\"col-md-12\">\r" +
    "\n" +
    "                            <highchart id=\"chart1\" config=\"pieMgr.chartConfig\"></highchart>\r" +
    "\n" +
    "                        </div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                    <div class=\"col-md-6\">\r" +
    "\n" +
    "                        <div class=\"col-md-12\">\r" +
    "\n" +
    "                            <h2>Attendance Summary</h2>\r" +
    "\n" +
    "                        </div>\r" +
    "\n" +
    "                        <div class=\"col-md-12\">\r" +
    "\n" +
    "                            <div class=\"panel\">\r" +
    "\n" +
    "                                <div class=\"list-group\">\r" +
    "\n" +
    "                                    <a ng-repeat=\"status in dashMgr.AttendanceSummary\" href=\"\" ng-click=\"selectPieSlice(status.StatusLabel)\" class=\"list-group-item\"><span class=\"badge\" ng-class=\"attendanceBadgeClass(status.StatusLabel)\">{{status.Count}}</span>{{status.StatusLabel}}</a>\r" +
    "\n" +
    "                                    <a href=\"\" ng-click=\"selectPieSlice(null)\" class=\"list-group-item\"><span class=\"badge badge-info\">{{dashMgr.TotalDays}}</span><b>Total Days (Delivered + Make-Up)</b></a>\r" +
    "\n" +
    "                                </div>\r" +
    "\n" +
    "                            </div>\r" +
    "\n" +
    "                        </div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"col-md-12\">\r" +
    "\n" +
    "                    <div class=\"col-md-12\">\r" +
    "\n" +
    "                        <h2>Note Detail</h2>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                    <div ng-show=\"filteredValues.length\" class=\"table-responsive col-md-12\">\r" +
    "\n" +
    "                        <div class=\"container-fluid ns-table\">\r" +
    "\n" +
    "                            <div class=\"ns-table-row ns-table-header\">\r" +
    "\n" +
    "                                <div class=\"ns-table-cell text-center\">Status</div>\r" +
    "\n" +
    "                                <div class=\"ns-table-cell text-center\">Recorder</div>\r" +
    "\n" +
    "                                <div class=\"ns-table-cell\">Date</div>\r" +
    "\n" +
    "                                <div class=\"ns-table-cell\">Note</div>\r" +
    "\n" +
    "                            </div>\r" +
    "\n" +
    "                            <form class=\"ns-table-row\" ng-repeat=\"note in filteredValues = (dashMgr.Notes  | filter: { AttendanceStatus : pieMgr.settings.selectedStatus || undefined } )\">\r" +
    "\n" +
    "                                <!-- | filter: { AttendanceStatus : pieMgr.settings.selectedStatus || undefined } -->\r" +
    "\n" +
    "                                <div class=\"ns-table-cell text-center\" data-title=\"Status\">\r" +
    "\n" +
    "                                    <span class=\"badge\" ng-class=\"attendanceBadgeClass(note.AttendanceStatus)\">{{::note.AttendanceStatus}}</span>\r" +
    "\n" +
    "                                </div>\r" +
    "\n" +
    "                                <div class=\"ns-table-cell text-center\" data-title=\"Recorder\">\r" +
    "\n" +
    "                                    {{::note.Recorder.LastName}}, {{::note.Recorder.FirstName}}\r" +
    "\n" +
    "                                </div>\r" +
    "\n" +
    "                                <div class=\"ns-table-cell\" data-title=\"Date\">\r" +
    "\n" +
    "                                    {{note.AttendanceDateString}}\r" +
    "\n" +
    "                                </div>\r" +
    "\n" +
    "                                <div class=\"ns-table-cell\" data-title=\"Note\">\r" +
    "\n" +
    "                                    <div ng-bind-html=\"note.Notes | safe_html\"></div>\r" +
    "\n" +
    "                                </div>\r" +
    "\n" +
    "                            </form>\r" +
    "\n" +
    "                        </div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"col-md-12\" ng-show=\"!filteredValues.length\">\r" +
    "\n" +
    "                    <uib-alert type=\"info\"><b>Note:</b> No notes of this type recorded.</uib-alert>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div style=\"clear:both\"></div>\r" +
    "\n" +
    "            </uib-tab>\r" +
    "\n" +
    "\r" +
    "\n" +
    "        </uib-tabset>\r" +
    "\n" +
    "    </div>\r" +
    "\n" +
    "</div>\r" +
    "\n" +
    "<!--  -->"
  );


  $templateCache.put('templates/intervention-dashboard-link.html',
    "<button class=\"btn btn-xs btn-primary\" ng-click=\"openInterventionDashboardDialog(intervention.SchoolYear, intervention.SchoolId, intervention.InterventionistId, intervention.InterventionGroupId, intervention.StudentID, intervention.Id)\"><i class=\"fa fa-dashboard\"></i> Go</button>\r" +
    "\n"
  );


  $templateCache.put('templates/linegraph-detail-ig.html',
    "<table class=\"table table-striped table-lowPadding table-bordered\">\r" +
    "\n" +
    "    <thead>\r" +
    "\n" +
    "        <tr>\r" +
    "\n" +
    "            <th class=\"\">Dates</th>\r" +
    "\n" +
    "            <th ng-repeat=\"date in dataManager.Results\" class=\"verticalTdDate\">\r" +
    "\n" +
    "                <div class=\"verticalDate\">{{::dataManager.formatDate(date.TestDueDate)}}</div>\r" +
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
    "            <th>{{ dataManager.fieldName }}</th>\r" +
    "\n" +
    "            <td ng-repeat=\"date in dataManager.BenchmarkDates\" ng-class=\"dataManager.getBackgroundClass(date.Result, date.TestNumber)\">\r" +
    "\n" +
    "                {{ ::dataManager.getDisplayValue(date.Result.FieldValueID)}}\r" +
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
    "        <tr ng-repeat=\"field in dataManager.Fields\">\r" +
    "\n" +
    "            <th>{{field.DisplayLabel}}</th>\r" +
    "\n" +
    "            <td ng-repeat=\"date in field.BenchmarkDates\">\r" +
    "\n" +
    "                <span ng-if=\"field.FieldType == 'Textarea'\">\r" +
    "\n" +
    "                    <line-graph-comments-cell comments=\"date.Result.StringValue\"></line-graph-comments-cell>\r" +
    "\n" +
    "                </span>\r" +
    "\n" +
    "                <span ng-if=\"field.FieldType != 'Textarea'\">\r" +
    "\n" +
    "                    <span ng-show=\"::date.Result.StringValue\">{{::field.AltDisplayLabel}}</span>{{::date.Result.StringValue}}\r" +
    "\n" +
    "                </span>\r" +
    "\n" +
    "                <!--<span ng-show=\"::date.Result.StringValue\">{{::field.AltDisplayLabel}}</span><span ng-if=\"field.DisplayLabel == 'Comments' && date.Result.StringValue != null && date.Result.StringValue != ''\"><i class=\"fa fa-comments\" style=\"margin-left:5px;cursor:pointer\" ng-click=\"openCommentModal(date.Result.StringValue);\"></i></span><span ng-if=\"field.DisplayLabel != 'Comments'\">{{::date.Result.StringValue}}</span>-->\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "        <tr ng-if=\"dataManager.bUseExceeds\">\r" +
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
    "        <tr ng-if=\"dataManager.bUseMeets\">\r" +
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
    "        <tr ng-if=\"dataManager.bUseApproaches\">\r" +
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
    "        <tr ng-if=\"dataManager.bUseDoesNotMeet\">\r" +
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
    "    </tbody>\r" +
    "\n" +
    "</table>"
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
    "            <th>Score</th>\r" +
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
    "        <tr ng-repeat=\"field in dataManager.Fields\">\r" +
    "\n" +
    "            <th>{{field.DisplayLabel}}</th>\r" +
    "\n" +
    "            <td ng-repeat=\"date in field.BenchmarkDates\">\r" +
    "\n" +
    "                <span ng-if=\"field.FieldType == 'Textarea'\">\r" +
    "\n" +
    "                    <line-graph-comments-cell comments=\"date.Result.StringValue\"></line-graph-comments-cell>\r" +
    "\n" +
    "                </span>\r" +
    "\n" +
    "                <span ng-if=\"field.FieldType != 'Textarea'\">\r" +
    "\n" +
    "                    <span ng-show=\"::date.Result.StringValue\">{{::field.AltDisplayLabel}}</span>{{::date.Result.StringValue}}\r" +
    "\n" +
    "                </span>\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "        <tr ng-if=\"dataManager.bUseExceeds\">\r" +
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
    "        <tr ng-if=\"dataManager.bUseMeets\">\r" +
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
    "        <tr ng-if=\"dataManager.bUseApproaches\">\r" +
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
    "        <tr ng-if=\"dataManager.bUseDoesNotMeet\">\r" +
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
    "    </tbody>\r" +
    "\n" +
    "</table>"
  );


  $templateCache.put('templates/nav_renderer.html',
    "<a ng-click=\"select(item)\" ng-href=\"{{::item.url}}\">\n" +
    "\t<i ng-if=\"::item.iconClasses\" class=\"{{::item.iconClasses}}\"></i><span>{{::item.label}}</span>\n" +
    "\t<span ng-bind-html=\"item.html\"></span>\n" +
    "</a>\n" +
    "<ul ng-if=\"::item.children.length\" data-slide-out-nav=\"item.open\">\n" +
    "    <li ng-repeat=\"item in item.children\"\n" +
    "\t    ng-class=\"{ hasChild: (item.children!==undefined && item.children.length > 0),\n" +
    "                      active: item.selected,\n" +
    "                        open: (item.children!==undefined && item.children.length > 0) && item.open }\"\n" +
    "    \tstatic-include data-source=\"templates/nav_renderer.html\"\n" +
    "    ></li>\n" +
    "</ul>\n"
  );


  $templateCache.put('templates/nav_renderer_horizontal.html',
    "<a ng-click=\"select(item)\" ng-href=\"{{::item.url}}\">\n" +
    "  <i ng-if=\"::item.iconClasses\" class=\"{{::item.iconClasses}}\"></i><span>{{::item.label}}</span>\n" +
    "  <span ng-bind-html=\"item.html\"></span>\n" +
    "</a>\n" +
    "<ul ng-if=\"::item.children.length\">\n" +
    "    <li ng-repeat=\"item in item.children\"\n" +
    "      ng-class=\"{ hasChild: (item.children!==undefined),\n" +
    "                      active: item.selected,\n" +
    "                        open: (item.children!==undefined) && item.open }\"\n" +
    "      static-include data-source=\"templates/nav_renderer_horizontal.html\"\n" +
    "    ></li>\n" +
    "</ul>\n"
  );


  $templateCache.put('templates/ns-errors.html',
    "<div class=\"col-md-12\">\r" +
    "\n" +
    "    <uib-alert ng-repeat=\"alert in errors\" type=\"{{alert.type}}\"><i class=\"fa fa-times-circle\"></i> <b>Warning</b> {{::alert.msg}}</uib-alert>\r" +
    "\n" +
    "</div>"
  );


  $templateCache.put('templates/nshelp-modal.html',
    "<span ng-if=\"settings.editMode\">\r" +
    "\n" +
    "    <textarea name=\"ckeditor_studentNote\" id=\"studentNote_NewNote\" cols=\"80\" rows=\"20\" class=\"ckeditor\" ng-model=\"helpManager.fieldValue\" ckeditor=\"{ language: 'en', allowedContent: true, entities: false }\"></textarea>\r" +
    "\n" +
    "    <span style=\"float:right\"><a class=\"btn btn-success btn-xs\" href=\"\" ng-click=\"saveData()\"><i class=\"fa fa-save\"></i> Save</a> <a class=\"btn btn-danger btn-xs\" href=\"\" ng-click=\"cancelEdit()\"><i class=\"fa fa-times-circle\"></i> Cancel</a></span>\r" +
    "\n" +
    "</span>\r" +
    "\n" +
    "<span ng-if=\"!settings.editMode\">\r" +
    "\n" +
    "    <span style=\"float:right\" ng-if=\"currentUser.IsSA || currentUser.IsPowerUser\"><a href=\"\" ng-click=\"enableEdit()\">Edit</a></span>\r" +
    "\n" +
    "    <a href=\"\" ng-click=\"openModal()\"><i style=\"font-size:20px;\" class=\"fa fa-question-circle\"></i></a>\r" +
    "\n" +
    "</span>"
  );


  $templateCache.put('templates/nshelp-text.html',
    "<span ng-if=\"settings.editMode\">\r" +
    "\n" +
    "    <textarea name=\"ckeditor_studentNote\" id=\"studentNote_NewNote\" cols=\"80\" rows=\"20\" class=\"ckeditor\" ng-model=\"helpManager.fieldValue\" ckeditor=\"ckeditorOptions\"></textarea>\r" +
    "\n" +
    "    <span style=\"float:right\"><a class=\"btn btn-success btn-xs\" href=\"\" ng-click=\"saveData()\"><i class=\"fa fa-save\"></i> Save</a> <a class=\"btn btn-danger btn-xs\" href=\"\" ng-click=\"cancelEdit()\"><i class=\"fa fa-times-circle\"></i> Cancel</a></span>\r" +
    "\n" +
    "</span>\r" +
    "\n" +
    "<span ng-if=\"!settings.editMode\">\r" +
    "\n" +
    "    <span ng-bind-html=\"helpManager.fieldValue | safe_html\"></span> <span style=\"float:right\" ng-if=\"currentUser.IsSA || currentUser.IsPowerUser\"><a href=\"\" ng-click=\"enableEdit()\">Edit</a></span>\r" +
    "\n" +
    "\r" +
    "\n" +
    "</span>"
  );


  $templateCache.put('templates/observation-summary-field-chooser.html',
    "<div class=\"btn-toolbar\">\r" +
    "\n" +
    "        <a href=\"\" class=\"dropdown-toggle btn btn-sky\" ng-click=\"openAssessmentChooser()\">Change Selected Assessments/Fields <i class=\"fa fa-bars\"></i></a>\r" +
    "\n" +
    "        <!--<div style=\"min-width:450px;\" class=\"dropdown-menu userinfo arrow\" uib-dropdown-menu>\r" +
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
    "        </div>-->\r" +
    "\n" +
    "</div>\r" +
    "\n" +
    "<script type=\"text/ng-template\" id=\"assessmentChooser.html\">\r" +
    "\n" +
    "    <div class=\"modal-header\">\r" +
    "\n" +
    "        <h3 class=\"modal-title\">Change Assessment Visibility</h3>\r" +
    "\n" +
    "    </div>\r" +
    "\n" +
    "    <div class=\"modal-body\" style=\"overflow-y:auto;height:600px;\">\r" +
    "\n" +
    "        <h3>Benchmarked Assessments</h3>\r" +
    "\n" +
    "        <table class=\"table\">\r" +
    "\n" +
    "            <thead>\r" +
    "\n" +
    "                <tr>\r" +
    "\n" +
    "                    <th>\r" +
    "\n" +
    "                        Change Available Fields\r" +
    "\n" +
    "                    </th>\r" +
    "\n" +
    "                    <th>\r" +
    "\n" +
    "                        Assessment Name\r" +
    "\n" +
    "                    </th>\r" +
    "\n" +
    "                    <th>\r" +
    "\n" +
    "                        Visible?\r" +
    "\n" +
    "                    </th>\r" +
    "\n" +
    "                </tr>\r" +
    "\n" +
    "            </thead>\r" +
    "\n" +
    "            <tbody>\r" +
    "\n" +
    "                <tr ng-repeat=\"assessment in assessmentService.BenchmarkAssessments\">\r" +
    "\n" +
    "                    <td>\r" +
    "\n" +
    "                        <button class=\"btn btn-primary btn-xs\" ng-click=\"openFieldsPopup(assessment)\"><i class=\"fa fa-bars\"></i> Fields</button>\r" +
    "\n" +
    "                    </td>\r" +
    "\n" +
    "                    <td>{{::assessment.text}}</td>\r" +
    "\n" +
    "                    <td><switch class=\"green\" ng-model=\"assessment.Visible\" ng-change=\"updateSelectedAssessment(assessment)\"></td>\r" +
    "\n" +
    "                </tr>\r" +
    "\n" +
    "            </tbody>\r" +
    "\n" +
    "        </table>\r" +
    "\n" +
    "\r" +
    "\n" +
    "        <h3>State Assessments</h3>\r" +
    "\n" +
    "        <table class=\"table\">\r" +
    "\n" +
    "            <thead>\r" +
    "\n" +
    "                <tr>\r" +
    "\n" +
    "                    <th>\r" +
    "\n" +
    "                        Change Available Fields\r" +
    "\n" +
    "                    </th>\r" +
    "\n" +
    "                    <th>\r" +
    "\n" +
    "                        Assessment Name\r" +
    "\n" +
    "                    </th>\r" +
    "\n" +
    "                    <th>\r" +
    "\n" +
    "                        Visible?\r" +
    "\n" +
    "                    </th>\r" +
    "\n" +
    "                </tr>\r" +
    "\n" +
    "            </thead>\r" +
    "\n" +
    "            <tbody>\r" +
    "\n" +
    "                <tr ng-repeat=\"assessment in assessmentService.StateTests\">\r" +
    "\n" +
    "                    <td>\r" +
    "\n" +
    "                        <button class=\"btn btn-primary btn-xs\" ng-click=\"openFieldsPopup(assessment)\"><i class=\"fa fa-bars\"></i> Fields</button>\r" +
    "\n" +
    "                    </td>\r" +
    "\n" +
    "                    <td>{{::assessment.text}}</td>\r" +
    "\n" +
    "                    <td><switch class=\"green\" ng-model=\"assessment.Visible\" ng-change=\"updateSelectedAssessment(assessment)\"></td>\r" +
    "\n" +
    "                </tr>\r" +
    "\n" +
    "            </tbody>\r" +
    "\n" +
    "        </table>\r" +
    "\n" +
    "        <h3>Filtered Classroom Dashboard Columns</h3>\r" +
    "\n" +
    "        <table class=\"table\">\r" +
    "\n" +
    "            <thead>\r" +
    "\n" +
    "                <tr>\r" +
    "\n" +
    "                    <th>\r" +
    "\n" +
    "                        Column Name\r" +
    "\n" +
    "                    </th>\r" +
    "\n" +
    "                    <th>\r" +
    "\n" +
    "                        Visible?\r" +
    "\n" +
    "                    </th>\r" +
    "\n" +
    "                </tr>\r" +
    "\n" +
    "            </thead>\r" +
    "\n" +
    "            <tbody>\r" +
    "\n" +
    "                <tr>\r" +
    "\n" +
    "                    <td>School</td>\r" +
    "\n" +
    "                    <td><switch class=\"green\" ng-model=\"assessmentService.OSSchoolVisible\" ng-change=\"updateSelectedColumn(assessmentService.School)\"></td>\r" +
    "\n" +
    "                </tr>\r" +
    "\n" +
    "                <tr>\r" +
    "\n" +
    "                    <td>Grade</td>\r" +
    "\n" +
    "                    <td><switch class=\"green\" ng-model=\"assessmentService.OSGradeVisible\" ng-change=\"updateSelectedColumn(assessmentService.Grade)\"></td>\r" +
    "\n" +
    "                </tr>\r" +
    "\n" +
    "                <tr>\r" +
    "\n" +
    "                    <td>Teacher</td>\r" +
    "\n" +
    "                    <td><switch class=\"green\" ng-model=\"assessmentService.OSTeacherVisible\" ng-change=\"updateSelectedColumn(assessmentService.Teacher)\"></td>\r" +
    "\n" +
    "                </tr>\r" +
    "\n" +
    "            </tbody>\r" +
    "\n" +
    "        </table>\r" +
    "\n" +
    "    </div>\r" +
    "\n" +
    "    <div class=\"modal-footer\">\r" +
    "\n" +
    "        <button ng-click=\"refreshAssessments()\" class=\"btn btn-success\"><i class=\"fa-save fa\"></i> Save and Update</button> <button class=\"btn btn-default\" ng-click=\"cancel()\"><i class=\"fa fa-times-circle\"></i> Close</button>\r" +
    "\n" +
    "    </div>\r" +
    "\n" +
    "</script>"
  );


  $templateCache.put('templates/observation-summary-filtered-button-print.html',
    "\r" +
    "\n" +
    "<div class=\"col-md-12 hidden-print\" style=\"margin-bottom:15px;\">\r" +
    "\n" +
    "    <button ng-click=\"generateGraph()\" class=\"btn btn-orange\"><i class=\"fa fa-bar-chart-o\"></i> Generate Report</button>\r" +
    "\n" +
    "</div>\r" +
    "\n" +
    "<div style=\"clear:both\"></div>\r" +
    "\n" +
    "<div ng-if=\"!observationSummaryManager.Scores.StudentResults || observationSummaryManager.Scores.StudentResults.length == 0\">\r" +
    "\n" +
    "    <uib-alert type=\"info\"><b>Note:</b> Either you have not yet generated a report or the selections you've made do not return any results.</uib-alert>\r" +
    "\n" +
    "</div>\r" +
    "\n" +
    "<div class=\"col-md-12\" style=\"margin-top:5px;margin-bottom:10px;\" ng-if=\"observationSummaryManager.Scores.StudentResults && observationSummaryManager.Scores.StudentResults.length > 0\">\r" +
    "\n" +
    "    <ns-stacked-bar-graph-legend title=\"'Filtered Observation Summary'\" options=\"groupFactory.options\"></ns-stacked-bar-graph-legend>\r" +
    "\n" +
    "</div>\r" +
    "\n" +
    "<div style=\"clear:both\"></div>\r" +
    "\n" +
    "<div style=\"margin:10px;\" ng-repeat=\"currentPage in printPages\" ng-if=\"observationSummaryManager.Scores.StudentResults && observationSummaryManager.Scores.StudentResults.length > 0\">\r" +
    "\n" +
    "    <div class=\"table-responsive\">\r" +
    "\n" +
    "        <table class=\"table table-striped\">\r" +
    "\n" +
    "            <thead>\r" +
    "\n" +
    "                <tr>\r" +
    "\n" +
    "                    <th ng-if=\"showCheckboxes === true\" rowspan=\"2\">\r" +
    "\n" +
    "                        Select All<br />\r" +
    "\n" +
    "                        <switch ng-model=\"allSelected\" ng-change=\"selectAllStudents()\"></switch>\r" +
    "\n" +
    "                    </th>\r" +
    "\n" +
    "                    <th rowspan=\"2\"><div style=\"cursor: pointer\" ng-click=\"sort('StudentName')\">Full Name <i class=\"{{manualSortHeaders.studentNameHeaderClass}}\"></i></div></th>\r" +
    "\n" +
    "                    <th ng-if=\"fieldChooser.OSSchoolVisible\" rowspan=\"2\"><div style=\"cursor: pointer\" ng-click=\"sort('SchoolName')\">School <i class=\"{{manualSortHeaders.schoolNameHeaderClass}}\"></i></div></th>\r" +
    "\n" +
    "                    <th ng-if=\"fieldChooser.OSGradeVisible\" rowspan=\"2\"><div style=\"cursor: pointer\" ng-click=\"sort('GradeOrder')\">Grade <i class=\"{{manualSortHeaders.gradeNameHeaderClass}}\"></i></div></th>\r" +
    "\n" +
    "                    <th ng-if=\"fieldChooser.OSTeacherVisible\" rowspan=\"2\"><div style=\"cursor: pointer\" ng-click=\"sort('DelimitedTeacherSections')\">Teacher (Section) <i class=\"{{manualSortHeaders.teacherNameHeaderClass}}\"></i></div></th>\r" +
    "\n" +
    "                    <th ng-if=\"observationSummaryManager.Scores.Att1Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att1')\">{{::observationSummaryManager.Scores.Att1Header}} <i class=\"{{manualSortHeaders['Att1']}}\"></i></div></th>\r" +
    "\n" +
    "                    <th ng-if=\"observationSummaryManager.Scores.Att2Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att2')\">{{::observationSummaryManager.Scores.Att2Header}} <i class=\"{{manualSortHeaders['Att2']}}\"></i></div></th>\r" +
    "\n" +
    "                    <th ng-if=\"observationSummaryManager.Scores.Att3Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att3')\">{{::observationSummaryManager.Scores.Att3Header}} <i class=\"{{manualSortHeaders['Att3']}}\"></i></div></th>\r" +
    "\n" +
    "                    <th ng-if=\"observationSummaryManager.Scores.Att4Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att4')\">{{::observationSummaryManager.Scores.Att4Header}} <i class=\"{{manualSortHeaders['Att4']}}\"></i></div></th>\r" +
    "\n" +
    "                    <th ng-if=\"observationSummaryManager.Scores.Att5Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att5')\">{{::observationSummaryManager.Scores.Att5Header}} <i class=\"{{manualSortHeaders['Att5']}}\"></i></div></th>\r" +
    "\n" +
    "                    <th ng-if=\"observationSummaryManager.Scores.Att6Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att6')\">{{::observationSummaryManager.Scores.Att6Header}} <i class=\"{{manualSortHeaders['Att6']}}\"></i></div></th>\r" +
    "\n" +
    "                    <th ng-if=\"observationSummaryManager.Scores.Att7Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att7')\">{{::observationSummaryManager.Scores.Att7Header}} <i class=\"{{manualSortHeaders['Att7']}}\"></i></div></th>\r" +
    "\n" +
    "                    <th ng-if=\"observationSummaryManager.Scores.Att8Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att8')\">{{::observationSummaryManager.Scores.Att8Header}} <i class=\"{{manualSortHeaders['Att8']}}\"></i></div></th>\r" +
    "\n" +
    "                    <th ng-if=\"observationSummaryManager.Scores.Att9Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att9')\">{{::observationSummaryManager.Scores.Att9Header}} <i class=\"{{manualSortHeaders['Att9']}}\"></i></div></th>\r" +
    "\n" +
    "                    <th style=\"text-align: center\" scope=\"col\" ng-repeat=\"headerGroup in observationSummaryManager.Scores.HeaderGroups  | filter: { page : currentPage.page}\" colspan=\"{{headerGroup.FieldCount}}\">\r" +
    "\n" +
    "                        <a href=\"\" ng-click=\"hideAssessment(headerGroup)\" class=\"pull-right\"><i class=\"fa fa-minus\"></i></a> {{::headerGroup.AssessmentName}}\r" +
    "\n" +
    "                    </th>\r" +
    "\n" +
    "                </tr>\r" +
    "\n" +
    "                <tr>\r" +
    "\n" +
    "                    <th scope=\"col\" data-tablesaw-sortable-col data-tablesaw-priority=\"3\" ng-repeat=\"field in observationSummaryManager.Scores.Fields | filter: { page : currentPage.page}\">\r" +
    "\n" +
    "                        <a href=\"\" ng-click=\"hideField(field)\" class=\"pull-right\"><i class=\"fa fa-minus\"></i></a>\r" +
    "\n" +
    "\r" +
    "\n" +
    "                        <div style=\"cursor: pointer\" ng-click=\"sort(observationSummaryManager.Scores.Fields.indexOf(field))\"><span ng-bind-html=\"field.FieldName | safe_html\"> </span><i class=\"{{headerClassArray[observationSummaryManager.Scores.Fields.indexOf(field)]}}\"></i></div>\r" +
    "\n" +
    "                    </th>\r" +
    "\n" +
    "                </tr>\r" +
    "\n" +
    "            </thead>\r" +
    "\n" +
    "            <tbody>\r" +
    "\n" +
    "                <tr on-finish-render=\"hideLoading()\" ng-class=\"{'nsObservationSummarySelected' : studentResult.selected}\" ng-repeat=\"studentResult in observationSummaryManager.Scores.StudentResults  | orderBy:sortArray \">\r" +
    "\n" +
    "                    <td ng-if=\"showCheckboxes === true\">\r" +
    "\n" +
    "                        <switch ng-model=\"studentResult.selected\" ng-change=\"changeTeamMeetingStudentSelection(studentResult)\"></switch>\r" +
    "\n" +
    "                    </td>\r" +
    "\n" +
    "                    <td align=\"left\" data-title=\"StudentName\"><span student-dashboard-link student-id=\"studentResult.StudentId\" student-name=\"studentResult.StudentName\"></span></td>\r" +
    "\n" +
    "                    <td ng-if=\"observationSummaryManager.Scores.Att1Visible\">{{::studentResult.Att1}}</td>\r" +
    "\n" +
    "                    <td ng-if=\"observationSummaryManager.Scores.Att2Visible\">{{::studentResult.Att2}}</td>\r" +
    "\n" +
    "                    <td ng-if=\"observationSummaryManager.Scores.Att3Visible\">{{::studentResult.Att3}}</td>\r" +
    "\n" +
    "                    <td ng-if=\"observationSummaryManager.Scores.Att4Visible\">{{::studentResult.Att4}}</td>\r" +
    "\n" +
    "                    <td ng-if=\"observationSummaryManager.Scores.Att5Visible\">{{::studentResult.Att5}}</td>\r" +
    "\n" +
    "                    <td ng-if=\"observationSummaryManager.Scores.Att6Visible\">{{::studentResult.Att6}}</td>\r" +
    "\n" +
    "                    <td ng-if=\"observationSummaryManager.Scores.Att7Visible\">{{::studentResult.Att7}}</td>\r" +
    "\n" +
    "                    <td ng-if=\"observationSummaryManager.Scores.Att8Visible\">{{::studentResult.Att8}}</td>\r" +
    "\n" +
    "                    <td ng-if=\"observationSummaryManager.Scores.Att9Visible\">{{::studentResult.Att9}}</td>\r" +
    "\n" +
    "                    <td ng-if=\"fieldChooser.OSSchoolVisible\" align=\"left\" data-title=\"SchoolName\">{{::studentResult.SchoolName}}</td>\r" +
    "\n" +
    "                    <td ng-if=\"fieldChooser.OSGradeVisible\" align=\"left\" data-title=\"GradeName\">{{::studentResult.GradeName}}</td>\r" +
    "\n" +
    "                    <td ng-if=\"fieldChooser.OSTeacherVisible\" align=\"left\" data-title=\"TeacherName\">{{::studentResult.DelimitedTeacherSections}}</td>\r" +
    "\n" +
    "                    <td ng-class=\"::getBackgroundClass(studentResult.GradeId, fieldResult)\" ng-repeat=\"fieldResult in studentResult.OSFieldResults | filter: { page : currentPage.page}\">\r" +
    "\n" +
    "                        <ns-assessment-field mode=\"'readonly'\" result=\"fieldResult\" all-results=\"studentResult.OSFieldResults\"></ns-assessment-field>\r" +
    "\n" +
    "                    </td>\r" +
    "\n" +
    "                </tr>\r" +
    "\n" +
    "            </tbody>\r" +
    "\n" +
    "        </table>\r" +
    "\n" +
    "    </div>\r" +
    "\n" +
    "    <div style=\"clear:both\"></div>\r" +
    "\n" +
    "    <div class=\"col-md-12\">\r" +
    "\n" +
    "        <div>\r" +
    "\n" +
    "            <table>\r" +
    "\n" +
    "                <tbody>\r" +
    "\n" +
    "                    <tr>\r" +
    "\n" +
    "                        <td style=\"padding-right:5px;\">\r" +
    "\n" +
    "                            <span class=\"obsPerfect\" style=\"border:1px solid black;display:inline-block;height:20px;width:25px;\"></span>\r" +
    "\n" +
    "                        </td>\r" +
    "\n" +
    "                        <td style=\"padding-right:10px\">\r" +
    "\n" +
    "                            <strong>Perfect Score</strong>\r" +
    "\n" +
    "                        </td>\r" +
    "\n" +
    "                        <td style=\"padding-right:5px;\">\r" +
    "\n" +
    "                            <span class=\"obsBlue\" style=\"border:1px solid black;display:inline-block;height:20px;width:25px;\"></span>\r" +
    "\n" +
    "                        </td>\r" +
    "\n" +
    "                        <td style=\"padding-right:10px\">\r" +
    "\n" +
    "                            <strong>Exceeds Expectations</strong>\r" +
    "\n" +
    "                        </td>\r" +
    "\n" +
    "                        <td style=\"padding-right:5px;\">\r" +
    "\n" +
    "                            <span class=\"obsGreen\" style=\"border:1px solid black;display:inline-block;height:20px;width:25px;\"></span>\r" +
    "\n" +
    "                        </td>\r" +
    "\n" +
    "                        <td style=\"padding-right:10px\">\r" +
    "\n" +
    "                            <strong>Meets Expectations</strong>\r" +
    "\n" +
    "                        </td>\r" +
    "\n" +
    "                        <td style=\"padding-right:5px;\">\r" +
    "\n" +
    "                            <span class=\"obsYellow\" style=\"border:1px solid black;display:inline-block;height:20px;width:25px;\"></span>\r" +
    "\n" +
    "                        </td>\r" +
    "\n" +
    "                        <td style=\"padding-right:10px\">\r" +
    "\n" +
    "                            <strong>Approaches Expectations</strong>\r" +
    "\n" +
    "                        </td>\r" +
    "\n" +
    "                        <td style=\"padding-right:5px;\">\r" +
    "\n" +
    "                            <span class=\"obsRed\" style=\"border:1px solid black;display:inline-block;height:20px;width:25px;\"></span>\r" +
    "\n" +
    "                        </td>\r" +
    "\n" +
    "                        <td style=\"padding-right:10px\">\r" +
    "\n" +
    "                            <strong>Does Not Meet Expectations</strong>\r" +
    "\n" +
    "                        </td>\r" +
    "\n" +
    "                    </tr>\r" +
    "\n" +
    "                    <tr>\r" +
    "\n" +
    "                        <td colspan=\"10\">\r" +
    "\n" +
    "                            <strong>* = Copied from a previous benchmark date</strong>\r" +
    "\n" +
    "                        </td>\r" +
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
    "    <div class=\"col-md-12 breaker\" ng-if=\"$index != printPages.length - 1\" style=\"height:50px;\">&nbsp;</div>\r" +
    "\n" +
    "    <div style=\"clear:both\"></div>\r" +
    "\n" +
    "</div>\r" +
    "\n" +
    "\r" +
    "\n" +
    "<style>\r" +
    "\n" +
    "    .breaker {\r" +
    "\n" +
    "        page-break-after: always;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "    .nsObservationSummarySelected {\r" +
    "\n" +
    "        border-left: 4px solid green;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "\r" +
    "\n" +
    "    a i.fa-minus {\r" +
    "\n" +
    "        color: #cccccc;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "\r" +
    "\n" +
    "    tr.ng-enter {\r" +
    "\n" +
    "        -webkit-transition: 1s;\r" +
    "\n" +
    "        transition: 1s;\r" +
    "\n" +
    "        opacity: 0;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "\r" +
    "\n" +
    "    tr.ng-enter-active {\r" +
    "\n" +
    "        opacity: 1;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "\r" +
    "\n" +
    "</style>\r" +
    "\n"
  );


  $templateCache.put('templates/observation-summary-filtered-button.html',
    "\r" +
    "\n" +
    "<div class=\"col-md-12\" style=\"margin-bottom:15px;\"  >\r" +
    "\n" +
    "    <button ng-click=\"generateGraph()\" class=\"btn btn-orange\"><i class=\"fa fa-bar-chart-o\"></i> Generate Report</button>\r" +
    "\n" +
    "</div>\r" +
    "\n" +
    "<div style=\"clear:both\"></div>\r" +
    "\n" +
    "<div ng-if=\"!observationSummaryManager.Scores.StudentResults || observationSummaryManager.Scores.StudentResults.length == 0\">\r" +
    "\n" +
    "    <uib-alert type=\"info\"><b>Note:</b> Either you have not yet generated a report or the selections you've made do not return any results.</uib-alert>\r" +
    "\n" +
    "</div>\r" +
    "\n" +
    "<div class=\"col-md-12\" style=\"margin-top:5px;margin-bottom:10px;\" ng-if=\"observationSummaryManager.Scores.StudentResults && observationSummaryManager.Scores.StudentResults.length > 0\">\r" +
    "\n" +
    "    <ns-stacked-bar-graph-legend title=\"'Filtered Observation Summary'\" options=\"groupFactory.options\"></ns-stacked-bar-graph-legend>\r" +
    "\n" +
    "</div>\r" +
    "\n" +
    "<div style=\"clear:both\"></div>\r" +
    "\n" +
    "<div ng-if=\"observationSummaryManager.Scores.StudentResults && observationSummaryManager.Scores.StudentResults.length > 0\" style=\"margin:10px;\">\r" +
    "\n" +
    "    <div class=\"table-responsive\" infinite-scroll=\"loadMoreRecords()\" infinite-scroll-disabled=\"observationSummaryManager.busy\">\r" +
    "\n" +
    "        <table class=\"table table-striped\">\r" +
    "\n" +
    "            <thead>\r" +
    "\n" +
    "                <tr>\r" +
    "\n" +
    "                    <th ng-if=\"showCheckboxes === true\" rowspan=\"2\">\r" +
    "\n" +
    "                        Select All<br />\r" +
    "\n" +
    "                        <switch ng-model=\"allSelected\" ng-change=\"selectAllStudents()\"></switch>\r" +
    "\n" +
    "                    </th>\r" +
    "\n" +
    "                    <th rowspan=\"2\"><div style=\"cursor: pointer\" ng-click=\"sort('StudentName')\">Student Name <i class=\"{{manualSortHeaders.studentNameHeaderClass}}\"></i></div></th>\r" +
    "\n" +
    "                    <th ng-if=\"observationSummaryManager.Scores.Att1Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att1')\">{{::observationSummaryManager.Scores.Att1Header}} <i class=\"{{manualSortHeaders['Att1']}}\"></i></div></th>\r" +
    "\n" +
    "                    <th ng-if=\"observationSummaryManager.Scores.Att2Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att2')\">{{::observationSummaryManager.Scores.Att2Header}} <i class=\"{{manualSortHeaders['Att2']}}\"></i></div></th>\r" +
    "\n" +
    "                    <th ng-if=\"observationSummaryManager.Scores.Att3Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att3')\">{{::observationSummaryManager.Scores.Att3Header}} <i class=\"{{manualSortHeaders['Att3']}}\"></i></div></th>\r" +
    "\n" +
    "                    <th ng-if=\"observationSummaryManager.Scores.Att4Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att4')\">{{::observationSummaryManager.Scores.Att4Header}} <i class=\"{{manualSortHeaders['Att4']}}\"></i></div></th>\r" +
    "\n" +
    "                    <th ng-if=\"observationSummaryManager.Scores.Att5Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att5')\">{{::observationSummaryManager.Scores.Att5Header}} <i class=\"{{manualSortHeaders['Att5']}}\"></i></div></th>\r" +
    "\n" +
    "                    <th ng-if=\"observationSummaryManager.Scores.Att6Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att6')\">{{::observationSummaryManager.Scores.Att6Header}} <i class=\"{{manualSortHeaders['Att6']}}\"></i></div></th>\r" +
    "\n" +
    "                    <th ng-if=\"observationSummaryManager.Scores.Att7Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att7')\">{{::observationSummaryManager.Scores.Att7Header}} <i class=\"{{manualSortHeaders['Att7']}}\"></i></div></th>\r" +
    "\n" +
    "                    <th ng-if=\"observationSummaryManager.Scores.Att8Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att8')\">{{::observationSummaryManager.Scores.Att8Header}} <i class=\"{{manualSortHeaders['Att8']}}\"></i></div></th>\r" +
    "\n" +
    "                    <th ng-if=\"observationSummaryManager.Scores.Att9Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att9')\">{{::observationSummaryManager.Scores.Att9Header}} <i class=\"{{manualSortHeaders['Att9']}}\"></i></div></th>\r" +
    "\n" +
    "                    <th ng-if=\"fieldChooser.OSSchoolVisible\" rowspan=\"2\">\r" +
    "\n" +
    "                        <a href=\"\" ng-click=\"hideOSColumn('school')\" class=\"pull-right\"><i class=\"fa fa-minus\"></i></a>\r" +
    "\n" +
    "                        <div style=\"cursor: pointer\" ng-click=\"sort('SchoolName')\">School <i class=\"{{manualSortHeaders.schoolNameHeaderClass}}\"></i></div>\r" +
    "\n" +
    "                    </th>\r" +
    "\n" +
    "                    <th  ng-if=\"fieldChooser.OSGradeVisible\" rowspan=\"2\">\r" +
    "\n" +
    "                        <a href=\"\" ng-click=\"hideOSColumn('grade')\" class=\"pull-right\"><i class=\"fa fa-minus\"></i></a>\r" +
    "\n" +
    "                        <div style=\"cursor: pointer\" ng-click=\"sort('GradeOrder')\"> Grade <i class=\"{{manualSortHeaders.gradeNameHeaderClass}}\"></i></div>\r" +
    "\n" +
    "                    </th>\r" +
    "\n" +
    "                    <th  ng-if=\"fieldChooser.OSTeacherVisible\" rowspan=\"2\">\r" +
    "\n" +
    "                        <a href=\"\" ng-click=\"hideOSColumn('teacher')\" class=\"pull-right\"><i class=\"fa fa-minus\"></i></a>\r" +
    "\n" +
    "                        <div style=\"cursor: pointer\" ng-click=\"sort('DelimitedTeacherSections')\"> Teacher (Section) <i class=\"{{manualSortHeaders.teacherNameHeaderClass}}\"></i></div>\r" +
    "\n" +
    "                    </th>\r" +
    "\n" +
    "                    <th style=\"text-align: center\" scope=\"col\" data-tablesaw-sortable-col data-tablesaw-priority=\"3\" ng-repeat=\"headerGroup in observationSummaryManager.Scores.HeaderGroups\" colspan=\"{{headerGroup.FieldCount}}\">\r" +
    "\n" +
    "                        <a href=\"\" ng-click=\"hideAssessment(headerGroup)\" class=\"pull-right\"><i class=\"fa fa-minus\"></i></a> {{::headerGroup.AssessmentName}}\r" +
    "\n" +
    "                    </th>\r" +
    "\n" +
    "                </tr>\r" +
    "\n" +
    "                <tr>\r" +
    "\n" +
    "                    <th scope=\"col\" data-tablesaw-sortable-col data-tablesaw-priority=\"3\" ng-repeat=\"field in observationSummaryManager.Scores.Fields\">\r" +
    "\n" +
    "                        <a href=\"\" ng-click=\"hideField(field)\" class=\"pull-right\"><i class=\"fa fa-minus\"></i></a>\r" +
    "\n" +
    "\r" +
    "\n" +
    "                        <div style=\"cursor: pointer\" ng-click=\"sort($index)\"><span ng-bind-html=\"field.FieldName | safe_html\"> </span><i class=\"{{headerClassArray[$index]}}\"></i></div>\r" +
    "\n" +
    "                    </th>\r" +
    "\n" +
    "                </tr>\r" +
    "\n" +
    "            </thead>\r" +
    "\n" +
    "            <tbody>\r" +
    "\n" +
    "                <tr on-finish-render=\"hideLoading()\" ng-class=\"{'nsObservationSummarySelected' : studentResult.selected}\" ng-repeat=\"studentResult in observationSummaryManager.Scores.StudentResults  | orderBy:sortArray | limitTo:observationSummaryManager.maxRecords\">\r" +
    "\n" +
    "                    <td ng-if=\"showCheckboxes === true\">\r" +
    "\n" +
    "                        <switch ng-model=\"studentResult.selected\" ng-change=\"changeTeamMeetingStudentSelection(studentResult)\"></switch>\r" +
    "\n" +
    "                    </td>\r" +
    "\n" +
    "                    <td align=\"left\" data-title=\"StudentName\"><span student-dashboard-link student-id=\"studentResult.StudentId\" student-name=\"studentResult.StudentName\"></span></td>\r" +
    "\n" +
    "                    <td ng-if=\"observationSummaryManager.Scores.Att1Visible\">{{::studentResult.Att1}}</td>\r" +
    "\n" +
    "                    <td ng-if=\"observationSummaryManager.Scores.Att2Visible\">{{::studentResult.Att2}}</td>\r" +
    "\n" +
    "                    <td ng-if=\"observationSummaryManager.Scores.Att3Visible\">{{::studentResult.Att3}}</td>\r" +
    "\n" +
    "                    <td ng-if=\"observationSummaryManager.Scores.Att4Visible\">{{::studentResult.Att4}}</td>\r" +
    "\n" +
    "                    <td ng-if=\"observationSummaryManager.Scores.Att5Visible\">{{::studentResult.Att5}}</td>\r" +
    "\n" +
    "                    <td ng-if=\"observationSummaryManager.Scores.Att6Visible\">{{::studentResult.Att6}}</td>\r" +
    "\n" +
    "                    <td ng-if=\"observationSummaryManager.Scores.Att7Visible\">{{::studentResult.Att7}}</td>\r" +
    "\n" +
    "                    <td ng-if=\"observationSummaryManager.Scores.Att8Visible\">{{::studentResult.Att8}}</td>\r" +
    "\n" +
    "                    <td ng-if=\"observationSummaryManager.Scores.Att9Visible\">{{::studentResult.Att9}}</td>\r" +
    "\n" +
    "                    <td ng-if=\"fieldChooser.OSSchoolVisible\" align=\"left\" data-title=\"SchoolName\">{{::studentResult.SchoolName}}</td>\r" +
    "\n" +
    "                    <td ng-if=\"fieldChooser.OSGradeVisible\"  align=\"left\" data-title=\"GradeName\">{{::studentResult.GradeName}}</td>\r" +
    "\n" +
    "                    <td ng-if=\"fieldChooser.OSTeacherVisible\" align=\"left\" data-title=\"TeacherName\">{{::studentResult.DelimitedTeacherSections}}</td>\r" +
    "\n" +
    "                    <td ng-class=\"::getBackgroundClass(studentResult.GradeId, fieldResult)\" ng-repeat=\"fieldResult in studentResult.OSFieldResults\">\r" +
    "\n" +
    "                        <ns-assessment-field mode=\"'readonly'\" result=\"fieldResult\" all-results=\"studentResult.OSFieldResults\"></ns-assessment-field>\r" +
    "\n" +
    "                    </td>\r" +
    "\n" +
    "                </tr>\r" +
    "\n" +
    "            </tbody>\r" +
    "\n" +
    "        </table>\r" +
    "\n" +
    "    </div>\r" +
    "\n" +
    "    <div style=\"clear:both\"></div>\r" +
    "\n" +
    "    <div class=\"col-md-12\">\r" +
    "\n" +
    "        <div>\r" +
    "\n" +
    "            <table>\r" +
    "\n" +
    "                <tbody>\r" +
    "\n" +
    "                    <tr>\r" +
    "\n" +
    "                        <td style=\"padding-right:5px;\">\r" +
    "\n" +
    "                            <span class=\"obsPerfect\" style=\"border:1px solid black;display:inline-block;height:20px;width:25px;\"></span>\r" +
    "\n" +
    "                        </td>\r" +
    "\n" +
    "                        <td style=\"padding-right:10px\">\r" +
    "\n" +
    "                            <strong>Perfect Score</strong>\r" +
    "\n" +
    "                        </td>\r" +
    "\n" +
    "                        <td style=\"padding-right:5px;\">\r" +
    "\n" +
    "                            <span class=\"obsBlue\" style=\"border:1px solid black;display:inline-block;height:20px;width:25px;\"></span>\r" +
    "\n" +
    "                        </td>\r" +
    "\n" +
    "                        <td style=\"padding-right:10px\">\r" +
    "\n" +
    "                            <strong>Exceeds Expectations</strong>\r" +
    "\n" +
    "                        </td>\r" +
    "\n" +
    "                        <td style=\"padding-right:5px;\">\r" +
    "\n" +
    "                            <span class=\"obsGreen\" style=\"border:1px solid black;display:inline-block;height:20px;width:25px;\"></span>\r" +
    "\n" +
    "                        </td>\r" +
    "\n" +
    "                        <td style=\"padding-right:10px\">\r" +
    "\n" +
    "                            <strong>Meets Expectations</strong>\r" +
    "\n" +
    "                        </td>\r" +
    "\n" +
    "                        <td style=\"padding-right:5px;\">\r" +
    "\n" +
    "                            <span class=\"obsYellow\" style=\"border:1px solid black;display:inline-block;height:20px;width:25px;\"></span>\r" +
    "\n" +
    "                        </td>\r" +
    "\n" +
    "                        <td style=\"padding-right:10px\">\r" +
    "\n" +
    "                            <strong>Approaches Expectations</strong>\r" +
    "\n" +
    "                        </td>\r" +
    "\n" +
    "                        <td style=\"padding-right:5px;\">\r" +
    "\n" +
    "                            <span class=\"obsRed\" style=\"border:1px solid black;display:inline-block;height:20px;width:25px;\"></span>\r" +
    "\n" +
    "                        </td>\r" +
    "\n" +
    "                        <td style=\"padding-right:10px\">\r" +
    "\n" +
    "                            <strong>Does Not Meet Expectations</strong>\r" +
    "\n" +
    "                        </td>\r" +
    "\n" +
    "                    </tr>\r" +
    "\n" +
    "                    <tr>\r" +
    "\n" +
    "                        <td colspan=\"10\">\r" +
    "\n" +
    "                            <strong>* = Copied from a previous benchmark date</strong>\r" +
    "\n" +
    "                        </td>\r" +
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
    "</div>\r" +
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
    "\r" +
    "\n" +
    "    a i.fa-minus {\r" +
    "\n" +
    "        color: #cccccc;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "    tr.ng-enter {\r" +
    "\n" +
    "    -webkit-transition: 1s;\r" +
    "\n" +
    "    transition: 1s;\r" +
    "\n" +
    "    opacity: 0;\r" +
    "\n" +
    "}\r" +
    "\n" +
    "tr.ng-enter-active {\r" +
    "\n" +
    "    opacity: 1;\r" +
    "\n" +
    "}\r" +
    "\n" +
    "</style>\r" +
    "\n"
  );


  $templateCache.put('templates/observation-summary-section-multiple-columns.html',
    "<table class=\"table table-striped\" id=\"obsSummary\">\r" +
    "\n" +
    "    <thead>\r" +
    "\n" +
    "        <tr>\r" +
    "\n" +
    "            <th rowspan=\"3\"><div>Student Name <i class=\"{{manualSortHeaders.studentNameHeaderClass}}\"></i></div></th>\r" +
    "\n" +
    "            <th ng-if=\"observationSummaryManager.Scores.Att1Visible\" rowspan=\"3\">{{::observationSummaryManager.Scores.Att1Header}}</th>\r" +
    "\n" +
    "            <th ng-if=\"observationSummaryManager.Scores.Att2Visible\" rowspan=\"3\">{{::observationSummaryManager.Scores.Att2Header}}</th>\r" +
    "\n" +
    "            <th ng-if=\"observationSummaryManager.Scores.Att3Visible\" rowspan=\"3\">{{::observationSummaryManager.Scores.Att3Header}}</th>\r" +
    "\n" +
    "            <th ng-if=\"observationSummaryManager.Scores.Att4Visible\" rowspan=\"3\">{{::observationSummaryManager.Scores.Att4Header}}</th>\r" +
    "\n" +
    "            <th ng-if=\"observationSummaryManager.Scores.Att5Visible\" rowspan=\"3\">{{::observationSummaryManager.Scores.Att5Header}}</th>\r" +
    "\n" +
    "            <th ng-if=\"observationSummaryManager.Scores.Att6Visible\" rowspan=\"3\">{{::observationSummaryManager.Scores.Att6Header}}</th>\r" +
    "\n" +
    "            <th ng-if=\"observationSummaryManager.Scores.Att7Visible\" rowspan=\"3\">{{::observationSummaryManager.Scores.Att7Header}}</th>\r" +
    "\n" +
    "            <th ng-if=\"observationSummaryManager.Scores.Att8Visible\" rowspan=\"3\">{{::observationSummaryManager.Scores.Att8Header}}</th>\r" +
    "\n" +
    "            <th ng-if=\"observationSummaryManager.Scores.Att9Visible\" rowspan=\"3\">{{::observationSummaryManager.Scores.Att9Header}}</th>\r" +
    "\n" +
    "            <th style=\"text-align: center;border-top:3px solid black !important;border-left:3px solid black;border-right:3px solid black;\" scope=\"col\" ng-repeat=\"headerGroup in observationSummaryManager.Scores.HeaderGroups\" colspan=\"{{::filterOptions.selectedBenchmarkDates.length * headerGroup.FieldCount}}\">\r" +
    "\n" +
    "                <a href=\"\" ng-click=\"hideAssessment(headerGroup)\" class=\"pull-right\"><i class=\"fa fa-minus\"></i></a> {{::headerGroup.AssessmentName}}\r" +
    "\n" +
    "            </th>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "        <tr>\r" +
    "\n" +
    "            <th class=\"text-center\" ng-style=\"assessmentDivider($index, field.IsAssessmentBorder)\" style=\"background-color:#fbfbfb;\" scope=\"col\" data-tablesaw-sortable-col data-tablesaw-priority=\"3\" colspan=\"{{::filterOptions.selectedBenchmarkDates.length}}\" ng-repeat=\"field in observationSummaryManager.Scores.Fields\">\r" +
    "\n" +
    "                <span ng-bind-html=\"field.FieldName | safe_html\"></span> \r" +
    "\n" +
    "            </th>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "        <tr>\r" +
    "\n" +
    "            <th ng-style=\"::dateDivider($index, dateField.field.IsAssessmentBorder, dateField.IsBorderCell)\" style=\"background-color:#efefef;\" scope=\"col\" data-tablesaw-sortable-col data-tablesaw-priority=\"3\" ng-repeat=\"dateField in observationSummaryManager.FieldsWithDates\">\r" +
    "\n" +
    "                <span>{{::dateField.benchmarkDate.text}}</span> \r" +
    "\n" +
    "            </th>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "    </thead>\r" +
    "\n" +
    "    <tbody>\r" +
    "\n" +
    "        <tr ng-repeat=\"studentResult in observationSummaryManager.StudentResultsByDates\">\r" +
    "\n" +
    "            <td align=\"left\" data-title=\"FullName\"><span student-dashboard-link student-id=\"studentResult.StudentId\" student-name=\"studentResult.StudentName\"></span></td>\r" +
    "\n" +
    "            <td ng-if=\"observationSummaryManager.Scores.Att1Visible\">{{::studentResult.Att1}}</td>\r" +
    "\n" +
    "            <td ng-if=\"observationSummaryManager.Scores.Att2Visible\">{{::studentResult.Att2}}</td>\r" +
    "\n" +
    "            <td ng-if=\"observationSummaryManager.Scores.Att3Visible\">{{::studentResult.Att3}}</td>\r" +
    "\n" +
    "            <td ng-if=\"observationSummaryManager.Scores.Att4Visible\">{{::studentResult.Att4}}</td>\r" +
    "\n" +
    "            <td ng-if=\"observationSummaryManager.Scores.Att5Visible\">{{::studentResult.Att5}}</td>\r" +
    "\n" +
    "            <td ng-if=\"observationSummaryManager.Scores.Att6Visible\">{{::studentResult.Att6}}</td>\r" +
    "\n" +
    "            <td ng-if=\"observationSummaryManager.Scores.Att7Visible\">{{::studentResult.Att7}}</td>\r" +
    "\n" +
    "            <td ng-if=\"observationSummaryManager.Scores.Att8Visible\">{{::studentResult.Att8}}</td>\r" +
    "\n" +
    "            <td ng-if=\"observationSummaryManager.Scores.Att9Visible\">{{::studentResult.Att9}}</td>\r" +
    "\n" +
    "            <td ng-style=\"::bodyDivider($index, $parent.$index, fieldResult.IsLastDateForField, fieldResult.IsAssessmentBorder)\" ng-class=\"::getBackgroundClass(fieldResult.ResultGradeId, fieldResult, fieldResult.TestLevelPeriodId)\" ng-repeat=\"fieldResult in studentResult.OSFieldResults\">\r" +
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
    "<style>\r" +
    "\n" +
    "    .nsObservationSummarySelected {\r" +
    "\n" +
    "        border-left: 4px solid green;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "\r" +
    "\n" +
    "    a i.fa-minus {\r" +
    "\n" +
    "        color: #cccccc;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "</style>\r" +
    "\n"
  );


  $templateCache.put('templates/observation-summary-section-multiple.html',
    "<table class=\"table table-striped\" id=\"obsSummary\">\r" +
    "\n" +
    "    <thead>\r" +
    "\n" +
    "        <tr>\r" +
    "\n" +
    "            <th ng-if=\"showCheckboxes === true\" rowspan=\"2\">\r" +
    "\n" +
    "                Select All<br />\r" +
    "\n" +
    "                <switch ng-model=\"allSelected\" ng-change=\"selectAllStudents(allSelected)\"></switch>\r" +
    "\n" +
    "            </th>\r" +
    "\n" +
    "            <th rowspan=\"2\"><div>Student Name <i class=\"{{manualSortHeaders.studentNameHeaderClass}}\"></i></div></th>\r" +
    "\n" +
    "            <th ng-if=\"observationSummaryManager.Scores.Att1Visible\" rowspan=\"2\">{{::observationSummaryManager.Scores.Att1Header}}</th>\r" +
    "\n" +
    "            <th ng-if=\"observationSummaryManager.Scores.Att2Visible\" rowspan=\"2\">{{::observationSummaryManager.Scores.Att2Header}}</th>\r" +
    "\n" +
    "            <th ng-if=\"observationSummaryManager.Scores.Att3Visible\" rowspan=\"2\">{{::observationSummaryManager.Scores.Att3Header}}</th>\r" +
    "\n" +
    "            <th ng-if=\"observationSummaryManager.Scores.Att4Visible\" rowspan=\"2\">{{::observationSummaryManager.Scores.Att4Header}}</th>\r" +
    "\n" +
    "            <th ng-if=\"observationSummaryManager.Scores.Att5Visible\" rowspan=\"2\">{{::observationSummaryManager.Scores.Att5Header}}</th>\r" +
    "\n" +
    "            <th ng-if=\"observationSummaryManager.Scores.Att6Visible\" rowspan=\"2\">{{::observationSummaryManager.Scores.Att6Header}}</th>\r" +
    "\n" +
    "            <th ng-if=\"observationSummaryManager.Scores.Att7Visible\" rowspan=\"2\">{{::observationSummaryManager.Scores.Att7Header}}</th>\r" +
    "\n" +
    "            <th ng-if=\"observationSummaryManager.Scores.Att8Visible\" rowspan=\"2\">{{::observationSummaryManager.Scores.Att8Header}}</th>\r" +
    "\n" +
    "            <th ng-if=\"observationSummaryManager.Scores.Att9Visible\" rowspan=\"2\">{{::observationSummaryManager.Scores.Att9Header}}</th>\r" +
    "\n" +
    "            <th style=\"text-align: center;border-top:3px solid black !important;border-left:3px solid black;border-right:3px solid black;\" scope=\"col\" ng-repeat=\"headerGroup in observationSummaryManager.Scores.HeaderGroups\" colspan=\"{{filterOptions.selectedBenchmarkDates.length}}\">\r" +
    "\n" +
    "                <a href=\"\" ng-click=\"hideAssessment(headerGroup)\" class=\"pull-right\"><i class=\"fa fa-minus\"></i></a> {{::headerGroup.AssessmentName}}\r" +
    "\n" +
    "            </th>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "        <tr>\r" +
    "\n" +
    "            <th ng-style=\"::divider($index)\" style=\"background-color:#efefef;\" scope=\"col\" data-tablesaw-sortable-col data-tablesaw-priority=\"3\" ng-repeat=\"dateField in observationSummaryManager.FieldsWithDates\">\r" +
    "\n" +
    "                <span>{{::dateField.benchmarkDate.text}}</span> \r" +
    "\n" +
    "            </th>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "    </thead>\r" +
    "\n" +
    "    <tbody>\r" +
    "\n" +
    "        <tr ng-repeat=\"studentResult in observationSummaryManager.StudentResultsByDates\">\r" +
    "\n" +
    "            <td align=\"left\" data-title=\"FullName\"><span student-dashboard-link student-id=\"studentResult.StudentId\" student-name=\"studentResult.StudentName\"></span></td>\r" +
    "\n" +
    "            <td ng-if=\"observationSummaryManager.Scores.Att1Visible\">{{::studentResult.Att1}}</td>\r" +
    "\n" +
    "            <td ng-if=\"observationSummaryManager.Scores.Att2Visible\">{{::studentResult.Att2}}</td>\r" +
    "\n" +
    "            <td ng-if=\"observationSummaryManager.Scores.Att3Visible\">{{::studentResult.Att3}}</td>\r" +
    "\n" +
    "            <td ng-if=\"observationSummaryManager.Scores.Att4Visible\">{{::studentResult.Att4}}</td>\r" +
    "\n" +
    "            <td ng-if=\"observationSummaryManager.Scores.Att5Visible\">{{::studentResult.Att5}}</td>\r" +
    "\n" +
    "            <td ng-if=\"observationSummaryManager.Scores.Att6Visible\">{{::studentResult.Att6}}</td>\r" +
    "\n" +
    "            <td ng-if=\"observationSummaryManager.Scores.Att7Visible\">{{::studentResult.Att7}}</td>\r" +
    "\n" +
    "            <td ng-if=\"observationSummaryManager.Scores.Att8Visible\">{{::studentResult.Att8}}</td>\r" +
    "\n" +
    "            <td ng-if=\"observationSummaryManager.Scores.Att9Visible\">{{::studentResult.Att9}}</td>\r" +
    "\n" +
    "            <td ng-style=\"::divider($index, $parent.$index, observationSummaryManager.StudentResultsByDates.length)\" ng-class=\"::getBackgroundClass(fieldResult.ResultGradeId, fieldResult, fieldResult.TestLevelPeriodId)\" ng-repeat=\"fieldResult in studentResult.OSFieldResults\">\r" +
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
    "<style>\r" +
    "\n" +
    "    .nsObservationSummarySelected {\r" +
    "\n" +
    "        border-left: 4px solid green;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "\r" +
    "\n" +
    "    a i.fa-minus {\r" +
    "\n" +
    "        color: #cccccc;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "</style>\r" +
    "\n"
  );


  $templateCache.put('templates/observation-summary-section-print.html',
    "<div ng-repeat=\"currentPage in printPages\">\r" +
    "\n" +
    "    <table class=\"table table-striped\" id=\"obsSummary\">\r" +
    "\n" +
    "        <thead>\r" +
    "\n" +
    "            <tr>\r" +
    "\n" +
    "                <th ng-if=\"showCheckboxes === true\" rowspan=\"2\">\r" +
    "\n" +
    "                    Select All<br />\r" +
    "\n" +
    "                    <switch ng-model=\"allSelected\" ng-change=\"selectAllStudents()\"></switch>\r" +
    "\n" +
    "                </th>\r" +
    "\n" +
    "                <th rowspan=\"2\"><div style=\"cursor: pointer\" ng-click=\"sort('StudentName')\">Student Name <i class=\"{{manualSortHeaders.studentNameHeaderClass}}\"></i></div></th>\r" +
    "\n" +
    "                <th ng-if=\"observationSummaryManager.Scores.Att1Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att1')\">{{::observationSummaryManager.Scores.Att1Header}} <i class=\"{{manualSortHeaders['Att1']}}\"></i></div></th>\r" +
    "\n" +
    "                <th ng-if=\"observationSummaryManager.Scores.Att2Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att2')\">{{::observationSummaryManager.Scores.Att2Header}} <i class=\"{{manualSortHeaders['Att2']}}\"></i></div></th>\r" +
    "\n" +
    "                <th ng-if=\"observationSummaryManager.Scores.Att3Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att3')\">{{::observationSummaryManager.Scores.Att3Header}} <i class=\"{{manualSortHeaders['Att3']}}\"></i></div></th>\r" +
    "\n" +
    "                <th ng-if=\"observationSummaryManager.Scores.Att4Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att4')\">{{::observationSummaryManager.Scores.Att4Header}} <i class=\"{{manualSortHeaders['Att4']}}\"></i></div></th>\r" +
    "\n" +
    "                <th ng-if=\"observationSummaryManager.Scores.Att5Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att5')\">{{::observationSummaryManager.Scores.Att5Header}} <i class=\"{{manualSortHeaders['Att5']}}\"></i></div></th>\r" +
    "\n" +
    "                <th ng-if=\"observationSummaryManager.Scores.Att6Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att6')\">{{::observationSummaryManager.Scores.Att6Header}} <i class=\"{{manualSortHeaders['Att6']}}\"></i></div></th>\r" +
    "\n" +
    "                <th ng-if=\"observationSummaryManager.Scores.Att7Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att7')\">{{::observationSummaryManager.Scores.Att7Header}} <i class=\"{{manualSortHeaders['Att7']}}\"></i></div></th>\r" +
    "\n" +
    "                <th ng-if=\"observationSummaryManager.Scores.Att8Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att8')\">{{::observationSummaryManager.Scores.Att8Header}} <i class=\"{{manualSortHeaders['Att8']}}\"></i></div></th>\r" +
    "\n" +
    "                <th ng-if=\"observationSummaryManager.Scores.Att9Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att9')\">{{::observationSummaryManager.Scores.Att9Header}} <i class=\"{{manualSortHeaders['Att9']}}\"></i></div></th>\r" +
    "\n" +
    "                <th style=\"text-align: center\" scope=\"col\" ng-repeat=\"headerGroup in observationSummaryManager.Scores.HeaderGroups | filter: { page : currentPage.page}\" colspan=\"{{headerGroup.FieldCount}}\">\r" +
    "\n" +
    "                    <a href=\"\" ng-click=\"hideAssessment(headerGroup)\" class=\"pull-right\"><i class=\"fa fa-minus\"></i></a> {{::headerGroup.AssessmentName}}\r" +
    "\n" +
    "                </th>\r" +
    "\n" +
    "            </tr>\r" +
    "\n" +
    "            <tr>\r" +
    "\n" +
    "                <th scope=\"col\" ng-repeat=\"field in observationSummaryManager.Scores.Fields | filter: { page : currentPage.page}\">\r" +
    "\n" +
    "                    <a href=\"\" ng-click=\"hideField(field)\" class=\"pull-right\"><i class=\"fa fa-minus\"></i></a>\r" +
    "\n" +
    "\r" +
    "\n" +
    "                    <div style=\"cursor: pointer\" ng-click=\"sort(observationSummaryManager.Scores.Fields.indexOf(field))\"><span ng-bind-html=\"::field.FieldName | safe_html\"></span> <i class=\"{{headerClassArray[observationSummaryManager.Scores.Fields.indexOf(field)]}}\"></i></div>\r" +
    "\n" +
    "                </th>\r" +
    "\n" +
    "            </tr>\r" +
    "\n" +
    "        </thead>\r" +
    "\n" +
    "        <tbody>\r" +
    "\n" +
    "            <tr ng-class=\"{'nsObservationSummarySelected' : studentResult.selected}\" ng-repeat=\"studentResult in observationSummaryManager.Scores.StudentResults | orderBy:sortArray\">\r" +
    "\n" +
    "                <td ng-if=\"::showCheckboxes === true\">\r" +
    "\n" +
    "                    <switch ng-model=\"studentResult.selected\" ng-change=\"changeTeamMeetingStudentSelection(studentResult)\"></switch>\r" +
    "\n" +
    "                </td>\r" +
    "\n" +
    "                <td align=\"left\" data-title=\"FullName\"><span student-dashboard-link student-id=\"studentResult.StudentId\" student-name=\"studentResult.StudentName\"></span></td>\r" +
    "\n" +
    "                <td ng-if=\"observationSummaryManager.Scores.Att1Visible\">{{::studentResult.Att1}}</td>\r" +
    "\n" +
    "                <td ng-if=\"observationSummaryManager.Scores.Att2Visible\">{{::studentResult.Att2}}</td>\r" +
    "\n" +
    "                <td ng-if=\"observationSummaryManager.Scores.Att3Visible\">{{::studentResult.Att3}}</td>\r" +
    "\n" +
    "                <td ng-if=\"observationSummaryManager.Scores.Att4Visible\">{{::studentResult.Att4}}</td>\r" +
    "\n" +
    "                <td ng-if=\"observationSummaryManager.Scores.Att5Visible\">{{::studentResult.Att5}}</td>\r" +
    "\n" +
    "                <td ng-if=\"observationSummaryManager.Scores.Att6Visible\">{{::studentResult.Att6}}</td>\r" +
    "\n" +
    "                <td ng-if=\"observationSummaryManager.Scores.Att7Visible\">{{::studentResult.Att7}}</td>\r" +
    "\n" +
    "                <td ng-if=\"observationSummaryManager.Scores.Att8Visible\">{{::studentResult.Att8}}</td>\r" +
    "\n" +
    "                <td ng-if=\"observationSummaryManager.Scores.Att9Visible\">{{::studentResult.Att9}}</td>\r" +
    "\n" +
    "                <td ng-class=\"::getBackgroundClass(studentResult.GradeId, fieldResult)\" ng-repeat=\"fieldResult in studentResult.OSFieldResults | filter: { page : currentPage.page}\">\r" +
    "\n" +
    "                    <ns-assessment-field mode=\"'readonly'\" result=\"fieldResult\" all-results=\"studentResult.OSFieldResults\"></ns-assessment-field>\r" +
    "\n" +
    "                </td>\r" +
    "\n" +
    "            </tr>\r" +
    "\n" +
    "        </tbody>\r" +
    "\n" +
    "    </table>\r" +
    "\n" +
    "    <div class=\"col-md-12 breaker\" ng-if=\"$index != printPages.length - 1\" style=\"height:50px;\">&nbsp;</div>\r" +
    "\n" +
    "\r" +
    "\n" +
    "</div>\r" +
    "\n" +
    "<style>\r" +
    "\n" +
    "    .breaker {\r" +
    "\n" +
    "        page-break-after: always;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "    .nsObservationSummarySelected {\r" +
    "\n" +
    "        border-left: 4px solid green;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "\r" +
    "\n" +
    "    a i.fa-minus {\r" +
    "\n" +
    "        color: #cccccc;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "</style>\r" +
    "\n"
  );


  $templateCache.put('templates/observation-summary-section.html',
    "<table class=\"table table-striped\" id=\"obsSummary\">\r" +
    "\n" +
    "    <thead>\r" +
    "\n" +
    "        <tr>\r" +
    "\n" +
    "            <th ng-if=\"showCheckboxes === true\" rowspan=\"2\">\r" +
    "\n" +
    "                Select All<br />\r" +
    "\n" +
    "                <switch ng-model=\"allSelected\" ng-change=\"selectAllStudents(allSelected)\"></switch>\r" +
    "\n" +
    "            </th>\r" +
    "\n" +
    "            <th rowspan=\"2\"><div style=\"cursor: pointer\" ng-click=\"sort('StudentName')\">Student Name <i class=\"{{manualSortHeaders.studentNameHeaderClass}}\"></i></div></th>\r" +
    "\n" +
    "            <th ng-if=\"observationSummaryManager.Scores.Att1Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att1')\">{{::observationSummaryManager.Scores.Att1Header}} <i class=\"{{manualSortHeaders['Att1']}}\"></i></div></th>\r" +
    "\n" +
    "            <th ng-if=\"observationSummaryManager.Scores.Att2Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att2')\">{{::observationSummaryManager.Scores.Att2Header}} <i class=\"{{manualSortHeaders['Att2']}}\"></i></div></th>\r" +
    "\n" +
    "            <th ng-if=\"observationSummaryManager.Scores.Att3Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att3')\">{{::observationSummaryManager.Scores.Att3Header}} <i class=\"{{manualSortHeaders['Att3']}}\"></i></div></th>\r" +
    "\n" +
    "            <th ng-if=\"observationSummaryManager.Scores.Att4Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att4')\">{{::observationSummaryManager.Scores.Att4Header}} <i class=\"{{manualSortHeaders['Att4']}}\"></i></div></th>\r" +
    "\n" +
    "            <th ng-if=\"observationSummaryManager.Scores.Att5Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att5')\">{{::observationSummaryManager.Scores.Att5Header}} <i class=\"{{manualSortHeaders['Att5']}}\"></i></div></th>\r" +
    "\n" +
    "            <th ng-if=\"observationSummaryManager.Scores.Att6Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att6')\">{{::observationSummaryManager.Scores.Att6Header}} <i class=\"{{manualSortHeaders['Att6']}}\"></i></div></th>\r" +
    "\n" +
    "            <th ng-if=\"observationSummaryManager.Scores.Att7Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att7')\">{{::observationSummaryManager.Scores.Att7Header}} <i class=\"{{manualSortHeaders['Att7']}}\"></i></div></th>\r" +
    "\n" +
    "            <th ng-if=\"observationSummaryManager.Scores.Att8Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att8')\">{{::observationSummaryManager.Scores.Att8Header}} <i class=\"{{manualSortHeaders['Att8']}}\"></i></div></th>\r" +
    "\n" +
    "            <th ng-if=\"observationSummaryManager.Scores.Att9Visible\" rowspan=\"2\"><div style=\"cursor: pointer;\" ng-click=\"sort('Att9')\">{{::observationSummaryManager.Scores.Att9Header}} <i class=\"{{manualSortHeaders['Att9']}}\"></i></div></th>\r" +
    "\n" +
    "            <th style=\"text-align: center\" scope=\"col\" ng-repeat=\"headerGroup in observationSummaryManager.Scores.HeaderGroups\" colspan=\"{{headerGroup.FieldCount}}\">\r" +
    "\n" +
    "                <a href=\"\" ng-click=\"hideAssessment(headerGroup)\" class=\"pull-right\"><i class=\"fa fa-minus\"></i></a> {{::headerGroup.AssessmentName}}\r" +
    "\n" +
    "            </th>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "        <tr>\r" +
    "\n" +
    "            <th scope=\"col\" data-tablesaw-sortable-col data-tablesaw-priority=\"3\" ng-repeat=\"field in observationSummaryManager.Scores.Fields\">\r" +
    "\n" +
    "                <a href=\"\" ng-click=\"hideField(field)\" class=\"pull-right\"><i class=\"fa fa-minus\"></i></a>\r" +
    "\n" +
    "\r" +
    "\n" +
    "                <div style=\"cursor: pointer\" ng-click=\"sort($index)\"><span ng-bind-html=\"::field.FieldName | safe_html\"></span> <i class=\"{{headerClassArray[$index]}}\"></i></div>\r" +
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
    "            <td ng-if=\"::showCheckboxes === true\">\r" +
    "\n" +
    "                <switch ng-model=\"studentResult.selected\" ng-change=\"changeTeamMeetingStudentSelection(studentResult)\"></switch>\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "            <td align=\"left\" data-title=\"FullName\"><span student-dashboard-link student-id=\"studentResult.StudentId\" student-name=\"studentResult.StudentName\"></span></td>\r" +
    "\n" +
    "            <td ng-if=\"observationSummaryManager.Scores.Att1Visible\">{{::studentResult.Att1}}</td>\r" +
    "\n" +
    "            <td ng-if=\"observationSummaryManager.Scores.Att2Visible\">{{::studentResult.Att2}}</td>\r" +
    "\n" +
    "            <td ng-if=\"observationSummaryManager.Scores.Att3Visible\">{{::studentResult.Att3}}</td>\r" +
    "\n" +
    "            <td ng-if=\"observationSummaryManager.Scores.Att4Visible\">{{::studentResult.Att4}}</td>\r" +
    "\n" +
    "            <td ng-if=\"observationSummaryManager.Scores.Att5Visible\">{{::studentResult.Att5}}</td>\r" +
    "\n" +
    "            <td ng-if=\"observationSummaryManager.Scores.Att6Visible\">{{::studentResult.Att6}}</td>\r" +
    "\n" +
    "            <td ng-if=\"observationSummaryManager.Scores.Att7Visible\">{{::studentResult.Att7}}</td>\r" +
    "\n" +
    "            <td ng-if=\"observationSummaryManager.Scores.Att8Visible\">{{::studentResult.Att8}}</td>\r" +
    "\n" +
    "            <td ng-if=\"observationSummaryManager.Scores.Att9Visible\">{{::studentResult.Att9}}</td>\r" +
    "\n" +
    "            <td ng-class=\"::getBackgroundClass(studentResult.GradeId, fieldResult)\" ng-repeat=\"fieldResult in studentResult.OSFieldResults\">\r" +
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
    "<style>\r" +
    "\n" +
    "    .nsObservationSummarySelected {\r" +
    "\n" +
    "        border-left: 4px solid green;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "\r" +
    "\n" +
    "    a i.fa-minus {\r" +
    "\n" +
    "        color: #cccccc;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "</style>\r" +
    "\n"
  );


  $templateCache.put('templates/observation-summary-student-print.html',
    "<div ng-repeat=\"currentPage in printPages\">\r" +
    "\n" +
    "    <table class=\"table table-striped\">\r" +
    "\n" +
    "        <thead>\r" +
    "\n" +
    "            <tr>\r" +
    "\n" +
    "                <th rowspan=\"2\"><div style=\"cursor: pointer\" ng-click=\"sort('TestDate')\">Benchmark Date <i class=\"{{manualSortHeaders.tddHeaderClass}}\"></i></div></th>\r" +
    "\n" +
    "                <th rowspan=\"2\"><div style=\"cursor: pointer\" ng-click=\"sort('GradeOrder')\">Grade <i class=\"{{manualSortHeaders.gradeNameHeaderClass}}\"></i></div></th>\r" +
    "\n" +
    "                <th rowspan=\"2\"><div style=\"cursor: pointer\" ng-click=\"sort('DelimitedTeachers')\">Teachers <i class=\"{{manualSortHeaders.teachersHeaderClass}}\"></i></div></th>\r" +
    "\n" +
    "                <th style=\"text-align: center\" scope=\"col\" ng-repeat=\"headerGroup in observationSummaryManager.Scores.HeaderGroups | filter: { page : currentPage.page}\" colspan=\"{{headerGroup.FieldCount}}\">\r" +
    "\n" +
    "                    <a href=\"\" ng-click=\"hideAssessment(headerGroup)\" class=\"pull-right\"><i class=\"fa fa-minus\"></i></a> {{headerGroup.AssessmentName}}\r" +
    "\n" +
    "                </th>\r" +
    "\n" +
    "            </tr>\r" +
    "\n" +
    "            <tr>\r" +
    "\n" +
    "                <th scope=\"col\" ng-repeat=\"field in observationSummaryManager.Scores.Fields | filter: { page : currentPage.page}\">\r" +
    "\n" +
    "                    <a href=\"\" ng-click=\"hideField(field)\" class=\"pull-right\"><i class=\"fa fa-minus\"></i></a>\r" +
    "\n" +
    "\r" +
    "\n" +
    "                    <div style=\"cursor: pointer\" ng-click=\"sort($index)\"><span ng-bind-html=\"field.FieldName | safe_html\"></span> <i class=\"{{headerClassArray[$index]}}\"></i></div>\r" +
    "\n" +
    "                </th>\r" +
    "\n" +
    "            </tr>\r" +
    "\n" +
    "        </thead>\r" +
    "\n" +
    "        <tbody>\r" +
    "\n" +
    "            <tr ng-repeat=\"studentResult in observationSummaryManager.Scores.StudentResults | orderBy:sortArray\">\r" +
    "\n" +
    "                <td align=\"left\" data-title=\"FirstName\">\r" +
    "\n" +
    "                    {{studentResult.TestDate | nsDateFormat }}\r" +
    "\n" +
    "                </td>\r" +
    "\n" +
    "                <td align=\"left\">\r" +
    "\n" +
    "                    {{studentResult.GradeName }}\r" +
    "\n" +
    "                </td>\r" +
    "\n" +
    "                <td align=\"left\">\r" +
    "\n" +
    "                    {{studentResult.DelimitedTeachers }}\r" +
    "\n" +
    "                </td>\r" +
    "\n" +
    "                <td ng-class=\"getBackgroundClass(studentResult.GradeId, studentResult.TestLevelPeriodId, fieldResult)\" ng-repeat=\"fieldResult in studentResult.OSFieldResults  | filter: { page : currentPage.page}\">\r" +
    "\n" +
    "                    <ns-assessment-field mode=\"'readonly'\" result=\"fieldResult\" all-results=\"studentResult.OSFieldResults\"></ns-assessment-field>\r" +
    "\n" +
    "                </td>\r" +
    "\n" +
    "            </tr>\r" +
    "\n" +
    "        </tbody>\r" +
    "\n" +
    "    </table>\r" +
    "\n" +
    "    <div class=\"col-md-12 breaker\" ng-if=\"$index != printPages.length - 1\" style=\"height:50px;\">&nbsp;</div>\r" +
    "\n" +
    "</div>\r" +
    "\n" +
    "<style>\r" +
    "\n" +
    "\r" +
    "\n" +
    "    a i.fa-minus {\r" +
    "\n" +
    "        color: #cccccc;\r" +
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
    "            <th rowspan=\"2\"><div style=\"cursor: pointer\" ng-click=\"sort('TestDate')\">Benchmark Date <i class=\"{{manualSortHeaders.tddHeaderClass}}\"></i></div></th>\r" +
    "\n" +
    "            <th rowspan=\"2\"><div style=\"cursor: pointer\" ng-click=\"sort('GradeOrder')\">Grade <i class=\"{{manualSortHeaders.gradeNameHeaderClass}}\"></i></div></th>\r" +
    "\n" +
    "            <th rowspan=\"2\"><div style=\"cursor: pointer\" ng-click=\"sort('DelimitedTeachers')\">Teachers <i class=\"{{manualSortHeaders.teachersHeaderClass}}\"></i></div></th>\r" +
    "\n" +
    "            <th style=\"text-align: center\" scope=\"col\" data-tablesaw-sortable-col data-tablesaw-priority=\"3\" ng-repeat=\"headerGroup in observationSummaryManager.Scores.HeaderGroups\" colspan=\"{{headerGroup.FieldCount}}\">\r" +
    "\n" +
    "                <a href=\"\" ng-click=\"hideAssessment(headerGroup)\" class=\"pull-right\"><i class=\"fa fa-minus\"></i></a> {{headerGroup.AssessmentName}}\r" +
    "\n" +
    "            </th>\r" +
    "\n" +
    "        </tr>\r" +
    "\n" +
    "        <tr>\r" +
    "\n" +
    "            <th scope=\"col\" data-tablesaw-sortable-col data-tablesaw-priority=\"3\" ng-repeat=\"field in observationSummaryManager.Scores.Fields\">\r" +
    "\n" +
    "                <a href=\"\" ng-click=\"hideField(field)\" class=\"pull-right\"><i class=\"fa fa-minus\"></i></a>\r" +
    "\n" +
    "\r" +
    "\n" +
    "                <div style=\"cursor: pointer\" ng-click=\"sort($index)\"><span ng-bind-html=\"field.FieldName | safe_html\"></span> <i class=\"{{headerClassArray[$index]}}\"></i></div>\r" +
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
    "            <td align=\"left\" data-title=\"FirstName\">\r" +
    "\n" +
    "                {{studentResult.TestDate | nsDateFormat }}\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "            <td align=\"left\">\r" +
    "\n" +
    "                {{studentResult.GradeName }}\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "            <td align=\"left\">\r" +
    "\n" +
    "                {{studentResult.DelimitedTeachers }}\r" +
    "\n" +
    "            </td>\r" +
    "\n" +
    "            <td ng-class=\"getBackgroundClass(studentResult.GradeId, studentResult.TestLevelPeriodId, fieldResult)\" ng-repeat=\"fieldResult in studentResult.OSFieldResults\">\r" +
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
    "<style>\r" +
    "\n" +
    "    a i.fa-minus {\r" +
    "\n" +
    "        color: #cccccc;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "</style>"
  );


  $templateCache.put('templates/observation-summary-tm-attend.html',
    "<div style=\"overflow-x:auto\">\r" +
    "\n" +
    "    <table class=\"table table-striped\">\r" +
    "\n" +
    "        <thead>\r" +
    "\n" +
    "            <tr>\r" +
    "\n" +
    "                <th rowspan=\"2\"><div style=\"cursor: pointer\" ng-click=\"sort('StudentName')\">Student Name <i class=\"{{manualSortHeaders.studentNameHeaderClass}}\"></i></div></th>\r" +
    "\n" +
    "                <th rowspan=\"2\"><div style=\"cursor: pointer\" ng-click=\"sort('GradeOrder')\">Grade <i class=\"{{manualSortHeaders.gradeNameHeaderClass}}\"></i></div></th>\r" +
    "\n" +
    "                <th rowspan=\"2\"><div style=\"cursor: pointer\" ng-click=\"sort('DelimitedTeachers')\">Teacher <i class=\"{{manualSortHeaders.teachersHeaderClass}}\"></i></div></th>\r" +
    "\n" +
    "                <th rowspan=\"2\">Modify Notes</th>\r" +
    "\n" +
    "                <th rowspan=\"2\">\r" +
    "\n" +
    "                    Assign Intervention\r" +
    "\n" +
    "                </th>\r" +
    "\n" +
    "                <th rowspan=\"2\">Current Interventions</th>\r" +
    "\n" +
    "                <th style=\"text-align: center\" scope=\"col\" data-tablesaw-sortable-col data-tablesaw-priority=\"3\" ng-repeat=\"headerGroup in observationSummaryManager.Scores.HeaderGroups\" colspan=\"{{headerGroup.FieldCount}}\">\r" +
    "\n" +
    "                    {{headerGroup.AssessmentName}}\r" +
    "\n" +
    "                </th>\r" +
    "\n" +
    "            </tr>\r" +
    "\n" +
    "            <tr>\r" +
    "\n" +
    "                <th scope=\"col\" data-tablesaw-sortable-col data-tablesaw-priority=\"3\" ng-repeat=\"field in observationSummaryManager.Scores.Fields\">\r" +
    "\n" +
    "                    <div style=\"cursor: pointer\" ng-click=\"sort($index)\"><span ng-bind-html=\"field.FieldName | safe_html\"></span> <i class=\"{{headerClassArray[$index]}}\"></i></div>\r" +
    "\n" +
    "                </th>\r" +
    "\n" +
    "            </tr>\r" +
    "\n" +
    "        </thead>\r" +
    "\n" +
    "        <tbody>\r" +
    "\n" +
    "            <tr ng-if=\"observationSummaryManager.Scores.StudentResults.length == 0\">\r" +
    "\n" +
    "                <td colspan=\"100\">\r" +
    "\n" +
    "                    No students available for this teacher.\r" +
    "\n" +
    "                </td>\r" +
    "\n" +
    "            </tr>\r" +
    "\n" +
    "            <tr ns-repeat-complete ng-repeat=\"studentResult in observationSummaryManager.Scores.StudentResults | orderBy:sortArray\">\r" +
    "\n" +
    "                <td align=\"left\" data-title=\"StudentName\"><span student-dashboard-link student-id=\"studentResult.StudentId\" student-name=\"studentResult.StudentName\"></span></td>\r" +
    "\n" +
    "                <td align=\"left\" data-title=\"Grade\">{{::studentResult.GradeName}}</td>\r" +
    "\n" +
    "                <td align=\"left\" data-title=\"TeacherName\">{{::studentResult.DelimitedTeachers}}</td>\r" +
    "\n" +
    "                <td>\r" +
    "\n" +
    "                    <a class=\"btn btn-xs btn-midnightblue\" href=\"#/tm-attend-notes/{{::selectedTeamMeetingId}}/{{::studentResult.StudentId}}\"><i class=\"fa fa-file-text-o\"></i> {{::studentResult.NoteCount}} Notes</a>\r" +
    "\n" +
    "                </td>\r" +
    "\n" +
    "                <td>\r" +
    "\n" +
    "                    <a href=\"\" ng-click=\"assignStudentToIntervention(studentResult)\" class=\"btn btn-xs btn-primary\"><i class=\"fa fa-bolt\"></i> Assign</a>\r" +
    "\n" +
    "                </td>\r" +
    "\n" +
    "                <td class=\"smallAccordion\">\r" +
    "\n" +
    "                    <span class=\"badge badge-success\" ng-if=\"!studentResult.Interventions\">no interventions</span>\r" +
    "\n" +
    "                    <button class=\"btn-xs btn btn-sky\" ng-if=\"studentResult.Interventions.InterventionsBySchoolYear.length > 0\" ng-click=\"openInterventionPopup(studentResult)\"><i class=\"fa fa-eye\"></i> View</button>\r" +
    "\n" +
    "                </td>\r" +
    "\n" +
    "                <td ng-class=\"getBackgroundClass(studentResult.GradeId, fieldResult)\" ng-repeat=\"fieldResult in studentResult.OSFieldResults\">\r" +
    "\n" +
    "                    <observation-summary-view-field lookup-fields-array=\"observationSummaryManager.LookupLists\" result=\"fieldResult\" all-results=\"studentResult.OSFieldResults\"></observation-summary-view-field>\r" +
    "\n" +
    "                </td>\r" +
    "\n" +
    "            </tr>\r" +
    "\n" +
    "        </tbody>\r" +
    "\n" +
    "    </table>\r" +
    "\n" +
    "</div>\r" +
    "\n" +
    "<script type=\"text/ng-template\" id=\"interventionList.html\">\r" +
    "\n" +
    "    <div class=\"modal-header\">\r" +
    "\n" +
    "        <h3 class=\"modal-title\">Intervention History for {{::selectedStudentResult.StudentName}}</h3>\r" +
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
    "                <div style=\"cursor: pointer\" ng-click=\"sort($index)\"><span ng-bind-html=\"field.FieldName | safe_html\"></span> <i class=\"{{headerClassArray[$index]}}\"></i></div>\r" +
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
    "                        <div ui-select2=\"select2SchoolOptions\" ng-model=\"filterOptions.selectedSchools\" ng-change=\"changeSchools(filterOptions.selectedSchools, {{filterOptions.selectedSchools}})\" data-placeholder=\"- All schools you can access -\" style=\"width:100%\"></div>\r" +
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
    "                        <div ui-select2=\"select2SchoolOptions\" ng-model=\"filterOptions.selectedSchools\" ng-change=\"changeSchools(filterOptions.selectedSchools, {{filterOptions.selectedSchools}})\" data-placeholder=\"- All schools you can access -\" style=\"width:100%\"></div>\r" +
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
    "                <div  ng-show=\"tddEnabled && !interventionMode\" class=\"row filterRowMargin\">\r" +
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
    "                    <label class=\"col-sm-4 control-label\">Schools </label>\r" +
    "\n" +
    "                    <div class=\"col-sm-8\">\r" +
    "\n" +
    "                        <div ui-select2=\"select2SchoolOptions\" ng-model=\"filterOptions.selectedSchools\" ng-change=\"changeSchools(filterOptions.selectedSchools, {{filterOptions.selectedSchools}})\" data-placeholder=\"- All schools you can access -\" style=\"width:100%\"></div>\r" +
    "\n" +
    "                        <br />\r" +
    "\n" +
    "                        <div style=\"margin-top:3px\">\r" +
    "\n" +
    "                            <span title=\"Pre-Schools\" style=\"cursor:pointer\" ng-click=\"loadSchools('prek', filterOptions.selectedSchools,'Pre-K')\" class=\"badge badge-primary\">Pre-K</span>\r" +
    "\n" +
    "                            <span title=\"Primary Schools (K-2)\" style=\"cursor:pointer\" ng-click=\"loadSchools('k2', filterOptions.selectedSchools,'Primary Schools')\" class=\"badge badge-primary\">Prim.</span>\r" +
    "\n" +
    "                            <span title=\"Elementary Schools (3-5)\" style=\"cursor:pointer\" ng-click=\"loadSchools('elem', filterOptions.selectedSchools, 'Elementary Schools')\" class=\"badge badge-primary\">Elem.</span>\r" +
    "\n" +
    "                            <span title=\"Elementary Schools (K-5)\" style=\"cursor:pointer\" ng-click=\"loadSchools('k5', filterOptions.selectedSchools, 'K-5')\" class=\"badge badge-primary\">K-5</span>\r" +
    "\n" +
    "                            <span title=\"Elementary Schools (K-8)\" style=\"cursor:pointer\" ng-click=\"loadSchools('k8', filterOptions.selectedSchools, 'K-8')\" class=\"badge badge-primary\">K-8</span>\r" +
    "\n" +
    "                            <span title=\"Middle Schools (6-8)\" style=\"cursor:pointer\" ng-click=\"loadSchools('ms', filterOptions.selectedSchools, 'Middle Schools')\" class=\"badge badge-primary\">MS</span>\r" +
    "\n" +
    "                            <span title=\"High Schools (9-12)\" style=\"cursor:pointer\" ng-click=\"loadSchools('hs', filterOptions.selectedSchools, 'High Schools')\" class=\"badge badge-primary\">HS</span>\r" +
    "\n" +
    "                            <span title=\"Special Education Services School (K-12)\" style=\"cursor:pointer\" ng-click=\"loadSchools('ss', filterOptions.selectedSchools, 'SS')\" class=\"badge badge-warning\">SS</span>\r" +
    "\n" +
    "                            <span title=\"All Schools (K-12)\" style=\"cursor:pointer\" ng-click=\"loadSchools('all', filterOptions.selectedSchools, 'All Schools')\" class=\"badge badge-success\">All</span>\r" +
    "\n" +
    "                            <span title=\"Clear All Schools\" style=\"cursor:pointer\" ng-click=\"clearSchools(filterOptions.selectedSchools)\" class=\"badge badge-danger\">Clear</span>\r" +
    "\n" +
    "                        </div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"row filterRowMargin\" ng-hide=\"interventionMode\">\r" +
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
    "                <div class=\"row filterRowMargin\" ng-hide=\"interventionMode\">\r" +
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
    "                <div class=\"row filterRowMargin\" ng-hide=\"interventionMode\">\r" +
    "\n" +
    "                    <label class=\"col-sm-4 control-label\">Sections</label>\r" +
    "\n" +
    "                    <div class=\"col-sm-8\">\r" +
    "\n" +
    "                        <div ui-select2=\"select2SectionOptions\" ng-model=\"filterOptions.selectedSections\"  ng-change=\"changeSections(filterOptions.selectedSections, {{filterOptions.selectedSections}})\"  data-placeholder=\"- All sections -\" style=\"width:100%\"></div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"row filterRowMargin\" ng-if=\"interventionMode\">\r" +
    "\n" +
    "                    <label class=\"col-sm-4 control-label\">Interventionists</label>\r" +
    "\n" +
    "                    <div class=\"col-sm-8\">\r" +
    "\n" +
    "                        <div ui-select2=\"select2InterventionistOptions\" ng-model=\"filterOptions.selectedInterventionists\" ng-change=\"changeInterventionists(filterOptions.selectedInterventionists, {{filterOptions.selectedInterventionists}})\" data-placeholder=\"- All Interventionists -\" style=\"width:100%\"></div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"row filterRowMargin\" ng-if=\"interventionMode\">\r" +
    "\n" +
    "                    <label class=\"col-sm-4 control-label\">Intervention Groups</label>\r" +
    "\n" +
    "                    <div class=\"col-sm-8\">\r" +
    "\n" +
    "                        <div ui-select2=\"select2InterventionGroupOptions\" ng-model=\"filterOptions.selectedInterventionGroups\" ng-change=\"changeInterventionGroups(filterOptions.selectedInterventionGroups, {{filterOptions.selectedInterventionGroups}})\" data-placeholder=\"- All Intervention Groups -\" style=\"width:100%\"></div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"row filterRowMargin\" ng-if=\"interventionMode\">\r" +
    "\n" +
    "                    <label class=\"col-sm-4 control-label\">Students</label>\r" +
    "\n" +
    "                    <div class=\"col-sm-8\">\r" +
    "\n" +
    "                        <div ui-select2=\"select2InterventionStudentOptions\" ng-model=\"filterOptions.selectedInterventionStudents\" ng-change=\"changeSections(filterOptions.selectedInterventionStudents, {{filterOptions.selectedInterventionStudents}})\" data-placeholder=\"- All Students -\" style=\"width:100%\"></div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"row filterRowMargin\" ng-if=\"interventionMode\">\r" +
    "\n" +
    "                    <label class=\"col-sm-4 control-label\">Assessment Type</label>\r" +
    "\n" +
    "                    <div class=\"col-sm-8\">\r" +
    "\n" +
    "                        <div ui-select2=\"select2InterventionAssessmentOptions\" ng-model=\"filterOptions.selectedInterventionAssessment\" data-placeholder=\"- Select Assessment -\" style=\"width:100%\"></div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"row filterRowMargin\" ng-if=\"studentsEnabled\" ng-hide=\"interventionMode\">\r" +
    "\n" +
    "                    <label class=\"col-sm-4 control-label\">Students</label>\r" +
    "\n" +
    "                    <div class=\"col-sm-8\">\r" +
    "\n" +
    "                        <div ui-select2=\"select2SectionStudentOptions\" ng-model=\"filterOptions.selectedStudents\" data-placeholder=\"- All Students -\" style=\"width:100%\"></div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"row filterRowMargin\" ng-show=\"assessmentFieldEnabled\">\r" +
    "\n" +
    "                    <label class=\"col-sm-4 control-label\">Assessment Field</label>\r" +
    "\n" +
    "                    <div class=\"col-sm-8\">\r" +
    "\n" +
    "                        <select class=\"form-control\" ng-model=\"filterOptions.selectedAssessmentField\" ng-options=\"field.DisplayLabel group by field.AssessmentName for field in filterOptions.assessments\">\r" +
    "\n" +
    "                            <option value=\"\"> -- Choose a Field -- </option>\r" +
    "\n" +
    "                        </select>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div ng-show=\"assessmentExportEnabled\">\r" +
    "\n" +
    "                    <div class=\"row\">\r" +
    "\n" +
    "                        <h3 class=\"dotted-underline\">Data Export Options</h3>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                    <div class=\"row filterRowMargin\">\r" +
    "\n" +
    "                        <label class=\"col-sm-4 control-label\">Assessments</label>\r" +
    "\n" +
    "                        <div class=\"col-sm-8\">\r" +
    "\n" +
    "                            <div ui-select2=\"select2MultiExportableAssessmentOptions\" ng-model=\"filterOptions.selectedExportableAssessments\" data-placeholder=\"- Select Assessments -\" style=\"width:100%\"></div>\r" +
    "\n" +
    "                        </div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div ng-show=\"batchPrintFieldsEnabled\">\r" +
    "\n" +
    "                    <div class=\"row\">\r" +
    "\n" +
    "                        <h3 class=\"dotted-underline\">Batch Print Options</h3>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                    <div class=\"row filterRowMargin\">\r" +
    "\n" +
    "                        <label class=\"col-sm-4 control-label\">Assessments</label>\r" +
    "\n" +
    "                        <div class=\"col-sm-8\">\r" +
    "\n" +
    "                            <div ui-select2=\"select2MultiAssessmentOptions\" ng-model=\"filterOptions.selectedAssessments\" data-placeholder=\"- Select Assessments -\" style=\"width:100%\"></div>\r" +
    "\n" +
    "                        </div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                    <div class=\"row filterRowMargin\">\r" +
    "\n" +
    "                        <label class=\"col-sm-4 control-label\">HFW Ranges</label>\r" +
    "\n" +
    "                        <div class=\"col-sm-8\">\r" +
    "\n" +
    "                            <div ui-select2=\"HfwMultiRangeRemoteOptions\" ng-model=\"filterOptions.selectedHfwPages\" data-placeholder=\"- Select HFW Pages -\" style=\"width:100%\"></div>\r" +
    "\n" +
    "                        </div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                    <!--<div class=\"row filterRowMargin\">\r" +
    "\n" +
    "                        <label class=\"col-sm-4 control-label\">HFW Student Report Pages</label>\r" +
    "\n" +
    "                        <div class=\"col-sm-8\">\r" +
    "\n" +
    "                            <div ui-select2=\"HfwMultiStudentReportRemoteOptions\" ng-model=\"filterOptions.selectedHfwStudentReportPages\" data-placeholder=\"- Select HFW Student Report Pages -\" style=\"width:100%\"></div>\r" +
    "\n" +
    "                        </div>\r" +
    "\n" +
    "                    </div>-->\r" +
    "\n" +
    "                    <div class=\"row filterRowMargin\">\r" +
    "\n" +
    "                        <label class=\"col-sm-4 control-label\">Page Types to Print</label>\r" +
    "\n" +
    "                        <div class=\"col-sm-8\">\r" +
    "\n" +
    "                            <div ui-select2=\"QuickSearchPageTypesToPrint\" ng-model=\"filterOptions.selectedPageTypes\" data-placeholder=\"- Select Page Types -\" style=\"width:100%\"></div>\r" +
    "\n" +
    "                        </div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                    <div class=\"row filterRowMargin\">\r" +
    "\n" +
    "                        <label class=\"col-sm-4 control-label\">Text Level Zone</label>\r" +
    "\n" +
    "                        <div class=\"col-sm-8\">\r" +
    "\n" +
    "                            <div ui-select2=\"QuickSearchTextLevelZones\" ng-model=\"filterOptions.selectedTextLevelZones\" data-placeholder=\"- All Students -\" style=\"width:100%\"></div>\r" +
    "\n" +
    "                        </div>\r" +
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
    "                        <div ui-select2=\"{minimumInputLength: 0, data: attributeType.DropDownData, multiple: true, width: 'resolve'}\" ng-model=\"attributeType.selectedData\" data-placeholder=\"- All Students -\" style=\"width:100%\"></div>\r" +
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
    "                    <label class=\"col-sm-4 control-label\">Special Education</label>\r" +
    "\n" +
    "                    <div class=\"col-sm-8\">\r" +
    "\n" +
    "                        <select class=\"form-control\" ng-model=\"filterOptions.selectedSpedTypes\" ng-options=\"item.text for item in select2SpedOptions.data\" style=\"width:100%\">\r" +
    "\n" +
    "                            <option value=\"\"> -- All Students -- </option>\r" +
    "\n" +
    "                        </select>\r" +
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
    "<panel panel-class=\"panel-primary\" heading=\"{{title}} Selected Options\">\r" +
    "\n" +
    "    <div class=\"col-md-12\">\r" +
    "\n" +
    "        <div class=\"col-md-6\">\r" +
    "\n" +
    "            <div class=\"row\">\r" +
    "\n" +
    "                <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-4 control-label\">School Year</label>\r" +
    "\n" +
    "                <div class=\"col-sm-8\">\r" +
    "\n" +
    "                    {{schoolYear()}}\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "            <div ng-show=\"options.selectedSchools.length > 0\" class=\"row\">\r" +
    "\n" +
    "                <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-4 control-label\">School</label>\r" +
    "\n" +
    "                <div class=\"col-sm-8\">\r" +
    "\n" +
    "                    {{schools()}}\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "            <div ng-show=\"options.selectedGrades.length > 0\" class=\"row\">\r" +
    "\n" +
    "                <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-4 control-label\">Grade</label>\r" +
    "\n" +
    "                <div class=\"col-sm-8\">\r" +
    "\n" +
    "                    {{grades()}}\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "            <div ng-show=\"options.selectedSections.length > 0\" class=\"row\">\r" +
    "\n" +
    "                <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-4 control-label\">Sections</label>\r" +
    "\n" +
    "                <div class=\"col-sm-8\">\r" +
    "\n" +
    "                    {{sections()}}\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "            <div ng-show=\"options.selectedInterventionTypes.length > 0\" class=\"row\">\r" +
    "\n" +
    "                <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-4 control-label\">Intervention Types</label>\r" +
    "\n" +
    "                <div class=\"col-sm-8\">\r" +
    "\n" +
    "                    {{interventionTypes()}}\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "            <div ng-show=\"options.selectedSpedTypes != null\" class=\"row\">\r" +
    "\n" +
    "                <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-4 control-label\">Special Ed</label>\r" +
    "\n" +
    "                <div class=\"col-sm-8\">\r" +
    "\n" +
    "                    {{specialEd()}}\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "            <div ng-show=\"options.selectedAssessmentField != null\" class=\"row\">\r" +
    "\n" +
    "                <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-4 control-label\">Assessment Field</label>\r" +
    "\n" +
    "                <div class=\"col-sm-8\">\r" +
    "\n" +
    "                    {{assessmentField()}}\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "            <div class=\"row\" ng-if=\"summaryMode\">\r" +
    "\n" +
    "                <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-4 control-label\">Zone</label>\r" +
    "\n" +
    "                <div class=\"col-sm-8\">\r" +
    "\n" +
    "                    {{scoreGrouping()}}\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "            <div class=\"row\" ng-if=\"summaryMode\">\r" +
    "\n" +
    "                <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-4 control-label\">Stack</label>\r" +
    "\n" +
    "                <div class=\"col-sm-8\">\r" +
    "\n" +
    "                    <span ng-bind-html=\"whichStack()\"></span>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "        </div>\r" +
    "\n" +
    "        <div class=\"col-md-6\" style=\"padding-left:25px;\">\r" +
    "\n" +
    "            <div  ng-repeat=\"attributeType in options.attributeTypes\"  ng-show=\"attributeType.selectedData.length > 0\" class=\"row\">\r" +
    "\n" +
    "                <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-4 control-label\">{{attributeType.Name}}</label>\r" +
    "\n" +
    "                <div class=\"col-sm-8\">\r" +
    "\n" +
    "                    {{attributeValue(attributeType.Name, attributeType.selectedData)}}\r" +
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
    "    .panel-primary{\r" +
    "\n" +
    "        margin-bottom: 5px !important;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "</style>"
  );


  $templateCache.put('templates/standard-report-header.html',
    "<panel panel-class=\"panel-primary\" heading=\"{{heading}}\">\r" +
    "\n" +
    "    <div class=\"row\">\r" +
    "\n" +
    "        <div class=\"col-md-2\">\r" +
    "\n" +
    "            <img style=\"height:50px\" src=\"/assets/img/northstar-logo-grayscale.png\">\r" +
    "\n" +
    "        </div>\r" +
    "\n" +
    "        <div class=\"col-md-5\" style=\"padding-left:25px;border-left:1px solid #cccccc;border-right:1px solid #cccccc\">\r" +
    "\n" +
    "            <div ng-show=\"options.schoolYearEnabled\" class=\"row\">\r" +
    "\n" +
    "                <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-4 control-label\">School Year</label>\r" +
    "\n" +
    "                <div class=\"col-sm-8\">\r" +
    "\n" +
    "                    {{getFieldDisplayTest(options.selectedSchoolYear.text, '-not selected-', options.quickSearchStudent.SchoolYearVerbose)}}\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "            <div ng-show=\"options.schoolEnabled\" class=\"row\">\r" +
    "\n" +
    "                <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-4 control-label\">School</label>\r" +
    "\n" +
    "                <div class=\"col-sm-8\">\r" +
    "\n" +
    "                    {{getFieldDisplayTest(options.selectedSchool.text, '-not selected-', options.quickSearchStudent.SchoolName)}}\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "            <div ng-show=\"options.gradeEnabled\" class=\"row\">\r" +
    "\n" +
    "                <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-4 control-label\">Grade</label>\r" +
    "\n" +
    "                <div class=\"col-sm-8\">\r" +
    "\n" +
    "                    {{getFieldDisplayTest(options.selectedGrade.text, '-not selected-', options.quickSearchStudent.GradeName)}}\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "        </div>\r" +
    "\n" +
    "        <div class=\"col-md-5\" style=\"padding-left:25px;\">\r" +
    "\n" +
    "            <div ng-show=\"options.teacherEnabled\" class=\"row\">\r" +
    "\n" +
    "                <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-4 control-label\">Teacher</label>\r" +
    "\n" +
    "                <div class=\"col-sm-8\">\r" +
    "\n" +
    "                    {{getFieldDisplayTest(options.selectedTeacher.text, '-not selected-', options.quickSearchStudent.StaffName)}}\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "            <div ng-show=\"options.interventionistEnabled\" class=\"row\">\r" +
    "\n" +
    "                <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-4 control-label\">Interventionist</label>\r" +
    "\n" +
    "                <div class=\"col-sm-8\">\r" +
    "\n" +
    "                    {{options.selectedInterventionist.text || '-not selected-'}}\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "            <div ng-show=\"options.sectionEnabled\" class=\"row\">\r" +
    "\n" +
    "                <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-4 control-label\">Section</label>\r" +
    "\n" +
    "                <div class=\"col-sm-8\">\r" +
    "\n" +
    "                    {{getFieldDisplayTest(options.selectedSection.text, '-not selected-', options.quickSearchStudent.SectionName)}}\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "            <div ng-show=\"options.interventionGroupEnabled\" class=\"row\">\r" +
    "\n" +
    "                <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-4 control-label\">Intervention Group</label>\r" +
    "\n" +
    "                <div class=\"col-sm-8\">\r" +
    "\n" +
    "                    {{options.selectedInterventionGroup.text || '-not selected-'}}\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "            <div ng-show=\"options.interventionStudentEnabled\" class=\"row\">\r" +
    "\n" +
    "                <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-4 control-label\">Student</label>\r" +
    "\n" +
    "                <div class=\"col-sm-8\">\r" +
    "\n" +
    "                    {{options.selectedInterventionStudent.text || '-not selected-'}}\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "            <div ng-show=\"options.sectionStudentEnabled\" class=\"row\">\r" +
    "\n" +
    "                <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-4 control-label\">Student</label>\r" +
    "\n" +
    "                <div class=\"col-sm-8\">\r" +
    "\n" +
    "                    {{getFieldDisplayTest(options.selectedSectionStudent.text, '-not selected-', options.quickSearchStudent.LastName + ', ' + options.quickSearchStudent.FirstName)}}\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "            <div ng-show=\"options.benchmarkDateEnabled\" class=\"row\">\r" +
    "\n" +
    "                <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-4 control-label\">Benchmark Date</label>\r" +
    "\n" +
    "                <div class=\"col-sm-8\">\r" +
    "\n" +
    "                    {{options.selectedBenchmarkDate.text || '-not selected-'}}\r" +
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
    "   \r" +
    "\n"
  );


  $templateCache.put('templates/student-attribute-chooser.html',
    "<div class=\"btn-toolbar\">\r" +
    "\n" +
    "    <a href=\"\" class=\"dropdown-toggle btn btn-danger\" ng-click=\"openAttributeChooser()\">Change Selected Student Attributes <i class=\"fa fa-bars\"></i></a>\r" +
    "\n" +
    "</div>\r" +
    "\n" +
    "<script type=\"text/ng-template\" id=\"attributeChooser.html\">\r" +
    "\n" +
    "    <div class=\"modal-header\">\r" +
    "\n" +
    "        <h3 class=\"modal-title\">Change Student Attribute Visibility</h3>\r" +
    "\n" +
    "    </div>\r" +
    "\n" +
    "    <div class=\"modal-body\" style=\"overflow-y:auto;height:600px;\">\r" +
    "\n" +
    "        <h3>Student Attributes</h3>\r" +
    "\n" +
    "        <table class=\"table\">\r" +
    "\n" +
    "            <thead>\r" +
    "\n" +
    "                <tr>\r" +
    "\n" +
    "                    <th>\r" +
    "\n" +
    "                        Attribute Name\r" +
    "\n" +
    "                    </th>\r" +
    "\n" +
    "                    <th>\r" +
    "\n" +
    "                        Visible?\r" +
    "\n" +
    "                    </th>\r" +
    "\n" +
    "                </tr>\r" +
    "\n" +
    "            </thead>\r" +
    "\n" +
    "            <tbody>\r" +
    "\n" +
    "                <tr ng-repeat=\"attribute in attributeService.Attributes\">\r" +
    "\n" +
    "                    <td>{{::attribute.Name}}</td>\r" +
    "\n" +
    "                    <td><switch class=\"green\" ng-model=\"attribute.Visible\" ng-change=\"updateSelectedAttribute(attribute)\"></td>\r" +
    "\n" +
    "                </tr>\r" +
    "\n" +
    "            </tbody>\r" +
    "\n" +
    "        </table>\r" +
    "\n" +
    "    </div>\r" +
    "\n" +
    "    <div class=\"modal-footer\">\r" +
    "\n" +
    "        <button ng-click=\"refreshAttributes()\" class=\"btn btn-success\"><i class=\"fa-save fa\"></i> Save and Update</button> <button class=\"btn btn-default\" ng-click=\"cancel()\"><i class=\"fa fa-times-circle\"></i> Close</button>\r" +
    "\n" +
    "    </div>\r" +
    "\n" +
    "</script>"
  );


  $templateCache.put('templates/student-dashboard-directive.html',
    "\r" +
    "\n" +
    "<div style=\"min-height:350px;\">\r" +
    "\n" +
    "    <spinner name=\"tableSpinner\">\r" +
    "\n" +
    "        <div style=\"position:absolute;width:100%;height:100%;background-color:rgba(100, 100, 100, 0.7);z-index:100;\">\r" +
    "\n" +
    "            <div class=\"sk-cube-grid\">\r" +
    "\n" +
    "                <div class=\"sk-cube sk-cube1\"></div>\r" +
    "\n" +
    "                <div class=\"sk-cube sk-cube2\"></div>\r" +
    "\n" +
    "                <div class=\"sk-cube sk-cube3\"></div>\r" +
    "\n" +
    "                <div class=\"sk-cube sk-cube4\"></div>\r" +
    "\n" +
    "                <div class=\"sk-cube sk-cube5\"></div>\r" +
    "\n" +
    "                <div class=\"sk-cube sk-cube6\"></div>\r" +
    "\n" +
    "                <div class=\"sk-cube sk-cube7\"></div>\r" +
    "\n" +
    "                <div class=\"sk-cube sk-cube8\"></div>\r" +
    "\n" +
    "                <div class=\"sk-cube sk-cube9\"></div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "        </div>\r" +
    "\n" +
    "    </spinner>\r" +
    "\n" +
    "    <div class=\"col-md-12\" ng-if=\"settings.studentLoaded\">\r" +
    "\n" +
    "        <uib-tabset justified=\"false\">\r" +
    "\n" +
    "            <uib-tab>\r" +
    "\n" +
    "                <uib-tab-heading>\r" +
    "\n" +
    "                    <i class=\"fa fa-line-chart\"></i> Class Line Graphs\r" +
    "\n" +
    "                </uib-tab-heading>\r" +
    "\n" +
    "                <div>\r" +
    "\n" +
    "                    <div ng-if=\"ClassLineGraphDataManagers.length == 0\">\r" +
    "\n" +
    "                        <uib-alert type=\"info\"><b>Note:</b> Student has not taken any benchmarked assessments</uib-alert>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                    <div ng-repeat=\"fieldMgr in ClassLineGraphDataManagers\" style=\"margin-bottom:50px;\">\r" +
    "\n" +
    "                        <div class=\"col-md-12\">\r" +
    "\n" +
    "                            <div>\r" +
    "\n" +
    "                                <highchart id=\"chart1\" config=\"fieldMgr.chartConfig\"></highchart>\r" +
    "\n" +
    "                            </div>\r" +
    "\n" +
    "                            <div>\r" +
    "\n" +
    "                                <line-graph-detail data-data-manager=\"fieldMgr\"></line-graph-detail>\r" +
    "\n" +
    "                            </div>\r" +
    "\n" +
    "                        </div>\r" +
    "\n" +
    "                        <div style=\"clear:both;\" />\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </uib-tab>\r" +
    "\n" +
    "            <uib-tab>\r" +
    "\n" +
    "                <uib-tab-heading>\r" +
    "\n" +
    "                    <i class=\"fa fa-user\"></i> Student Details\r" +
    "\n" +
    "                </uib-tab-heading>\r" +
    "\n" +
    "                <div>\r" +
    "\n" +
    "                    <!-- Photo -->\r" +
    "\n" +
    "                    <div ng-show=\"fullStudent.ImageUrl != '' && fullStudent.ImageUrl != null\">\r" +
    "\n" +
    "                        <img class=\"studentPhoto\" ng-src=\"{{fullStudent.ImageUrl}}\" alt=\"{{studentResult.StudentName}}\" />\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                    <div ng-if=\"fullStudent.ImageUrl == '' || fullStudent.ImageUrl == null\">\r" +
    "\n" +
    "                        <img class=\"studentPhoto\" src=\"/assets/img/placeholder_person.jpg\"  />\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                    <fieldset>\r" +
    "\n" +
    "                        <legend>Details</legend>\r" +
    "\n" +
    "                        <div class=\"row\">\r" +
    "\n" +
    "                            <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-6 control-label studentLabel\">First Name</label>\r" +
    "\n" +
    "                            <div class=\"col-md-4 studentText\">\r" +
    "\n" +
    "                                {{fullStudent.FirstName}}\r" +
    "\n" +
    "                            </div>\r" +
    "\n" +
    "                        </div>\r" +
    "\n" +
    "                        <div class=\"row\">\r" +
    "\n" +
    "                            <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-6 control-label studentLabel\">Middle Name</label>\r" +
    "\n" +
    "                            <div class=\"col-md-6 studentText\">\r" +
    "\n" +
    "                                {{fullStudent.MiddleName}}\r" +
    "\n" +
    "                            </div>\r" +
    "\n" +
    "                        </div>\r" +
    "\n" +
    "                        <div class=\"row\">\r" +
    "\n" +
    "                            <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-6 control-label studentLabel\">Last Name</label>\r" +
    "\n" +
    "                            <div class=\"col-md-6 studentText\">\r" +
    "\n" +
    "                                {{fullStudent.LastName}}\r" +
    "\n" +
    "                            </div>\r" +
    "\n" +
    "                        </div>\r" +
    "\n" +
    "                        <div class=\"row\">\r" +
    "\n" +
    "                            <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-6 control-label studentLabel\">Student ID</label>\r" +
    "\n" +
    "                            <div class=\"col-md-6 studentText\">\r" +
    "\n" +
    "                                {{fullStudent.StudentIdentifier}}\r" +
    "\n" +
    "                            </div>\r" +
    "\n" +
    "                        </div>\r" +
    "\n" +
    "                        <div class=\"row\">\r" +
    "\n" +
    "                            <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-6 control-label studentLabel\">Date of Birth</label>\r" +
    "\n" +
    "                            <div class=\"col-md-6 studentText\">\r" +
    "\n" +
    "                                {{fullStudent.DOB}}\r" +
    "\n" +
    "                            </div>\r" +
    "\n" +
    "                        </div>\r" +
    "\n" +
    "                        <div class=\"row\">\r" +
    "\n" +
    "                            <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-6 control-label studentLabel\">Graduation Year</label>\r" +
    "\n" +
    "                            <div class=\"col-md-6 studentText\">\r" +
    "\n" +
    "                                {{fullStudent.GraduationYear}}\r" +
    "\n" +
    "                            </div>\r" +
    "\n" +
    "                        </div>\r" +
    "\n" +
    "                        <div class=\"row\">\r" +
    "\n" +
    "                            <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-6 control-label studentLabel\">Enrollment Year</label>\r" +
    "\n" +
    "                            <div class=\"col-md-6 studentText\">{{fullStudent.EnrollmentYear}}</div>\r" +
    "\n" +
    "                        </div>\r" +
    "\n" +
    "\r" +
    "\n" +
    "                    </fieldset>\r" +
    "\n" +
    "                    <fieldset>\r" +
    "\n" +
    "                        <legend>Student Attributes</legend>\r" +
    "\n" +
    "                        <div class=\"row\" ng-repeat=\"attribute in allAttributes.AllAttributes\">\r" +
    "\n" +
    "                            <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-6 control-label studentLabel\">{{attribute.AttributeName}}</label>\r" +
    "\n" +
    "                            <div class=\"col-md-6 studentText\">\r" +
    "\n" +
    "                                {{getAttributeValue(fullStudent.StudentAttributes[attribute.Id], attribute)}}\r" +
    "\n" +
    "                                <!--<select class=\"form-control\" ng-model=\"fullStudent.StudentAttributes[attribute.Id]\" ng-options=\"v.LookupValueId as v.LookupValue for v in attribute.LookupValues\">\r" +
    "\n" +
    "                                    <option value=\"\">-select-</option>\r" +
    "\n" +
    "                                </select>-->\r" +
    "\n" +
    "                            </div>\r" +
    "\n" +
    "                        </div>\r" +
    "\n" +
    "                    </fieldset>\r" +
    "\n" +
    "                    <fieldset>\r" +
    "\n" +
    "                        <legend>Education Services</legend>\r" +
    "\n" +
    "                        <div class=\"row\">\r" +
    "\n" +
    "                            <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-6 control-label studentLabel\">Services</label>\r" +
    "\n" +
    "                            <div class=\"col-md-6 studentText\">\r" +
    "\n" +
    "                                <div ng-repeat=\"label in fullStudent.SpecialEdLabels\">\r" +
    "\n" +
    "                                    {{label.text}}\r" +
    "\n" +
    "                                </div>\r" +
    "\n" +
    "                            </div>\r" +
    "\n" +
    "                        </div>\r" +
    "\n" +
    "                    </fieldset>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </uib-tab>\r" +
    "\n" +
    "            <uib-tab>\r" +
    "\n" +
    "                <uib-tab-heading>\r" +
    "\n" +
    "                    <i class=\"fa fa-folder-open\"></i> Student Cumulative Folder\r" +
    "\n" +
    "                </uib-tab-heading>\r" +
    "\n" +
    "                <h2><b>{{settings.selectedStudent.text}}</b> Assessment History</h2>\r" +
    "\n" +
    "                <div class=\"table-responsive\">\r" +
    "\n" +
    "                    <ns-observation-summary-student selected-student-id=\"settings.selectedStudent.id\"></ns-observation-summary-student>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div>\r" +
    "\n" +
    "                    <div>\r" +
    "\n" +
    "                        <table>\r" +
    "\n" +
    "                            <tbody>\r" +
    "\n" +
    "                                <tr>\r" +
    "\n" +
    "                                    <td style=\"padding-right:5px;\">\r" +
    "\n" +
    "                                        <span class=\"obsPerfect\" style=\"border:1px solid black;display:inline-block;height:20px;width:25px;\"></span>\r" +
    "\n" +
    "                                    </td>\r" +
    "\n" +
    "                                    <td style=\"padding-right:10px\">\r" +
    "\n" +
    "                                        <strong>Perfect Score</strong>\r" +
    "\n" +
    "                                    </td>\r" +
    "\n" +
    "                                    <td style=\"padding-right:5px;\">\r" +
    "\n" +
    "                                        <span class=\"obsBlue\" style=\"border:1px solid black;display:inline-block;height:20px;width:25px;\"></span>\r" +
    "\n" +
    "                                    </td>\r" +
    "\n" +
    "                                    <td style=\"padding-right:10px\">\r" +
    "\n" +
    "                                        <strong>Exceeds Expectations</strong>\r" +
    "\n" +
    "                                    </td>\r" +
    "\n" +
    "                                    <td style=\"padding-right:5px;\">\r" +
    "\n" +
    "                                        <span class=\"obsGreen\" style=\"border:1px solid black;display:inline-block;height:20px;width:25px;\"></span>\r" +
    "\n" +
    "                                    </td>\r" +
    "\n" +
    "                                    <td style=\"padding-right:10px\">\r" +
    "\n" +
    "                                        <strong>Meets Expectations</strong>\r" +
    "\n" +
    "                                    </td>\r" +
    "\n" +
    "                                    <td style=\"padding-right:5px;\">\r" +
    "\n" +
    "                                        <span class=\"obsYellow\" style=\"border:1px solid black;display:inline-block;height:20px;width:25px;\"></span>\r" +
    "\n" +
    "                                    </td>\r" +
    "\n" +
    "                                    <td style=\"padding-right:10px\">\r" +
    "\n" +
    "                                        <strong>Approaches Expectations</strong>\r" +
    "\n" +
    "                                    </td>\r" +
    "\n" +
    "                                    <td style=\"padding-right:5px;\">\r" +
    "\n" +
    "                                        <span class=\"obsRed\" style=\"border:1px solid black;display:inline-block;height:20px;width:25px;\"></span>\r" +
    "\n" +
    "                                    </td>\r" +
    "\n" +
    "                                    <td style=\"padding-right:10px\">\r" +
    "\n" +
    "                                        <strong>Does Not Meet Expectations</strong>\r" +
    "\n" +
    "                                    </td>\r" +
    "\n" +
    "                                </tr>\r" +
    "\n" +
    "                            </tbody>\r" +
    "\n" +
    "                        </table>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </uib-tab>\r" +
    "\n" +
    "\r" +
    "\n" +
    "            <uib-tab>\r" +
    "\n" +
    "                <uib-tab-heading>\r" +
    "\n" +
    "                    <i class=\"fa fa-bolt\"></i> Interventions\r" +
    "\n" +
    "                </uib-tab-heading>\r" +
    "\n" +
    "                <div ng-if=\"studentDataManager.Interventions.length  == 0\">\r" +
    "\n" +
    "                    <uib-alert type=\"info\"><b>Note:</b> Student has not received any interventions</uib-alert>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div ng-if=\"studentDataManager.Interventions.length > 0\">\r" +
    "\n" +
    "                    <student-interventions data-data-manager=\"studentDataManager\"></student-interventions>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </uib-tab>\r" +
    "\n" +
    "            <uib-tab>\r" +
    "\n" +
    "                <uib-tab-heading>\r" +
    "\n" +
    "                    <i class=\"fa fa-paperclip\"></i> Student Notes\r" +
    "\n" +
    "                </uib-tab-heading>\r" +
    "\n" +
    "                <div ng-if=\"studentNotesManager.Notes.length == 0\">\r" +
    "\n" +
    "                    <uib-alert type=\"info\"><b>Note:</b> Student does not have any notes</uib-alert>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div>\r" +
    "\n" +
    "                    <ul class=\"timeline\">\r" +
    "\n" +
    "                        <li class=\"timeline-midnightblue\">\r" +
    "\n" +
    "                            <div class=\"timeline-icon\"><i class=\"fa fa-plus\"></i></div>\r" +
    "\n" +
    "                            <div class=\"timeline-body\" ng-show=\"!settings.selectedStudent.newNoteEditMode\">\r" +
    "\n" +
    "                                <button class=\"btn btn-success\" ng-click=\"settings.selectedStudent.newNoteEditMode = true\"><i class=\"fa fa-plus\"></i> Add New Note</button>\r" +
    "\n" +
    "                            </div>\r" +
    "\n" +
    "                            <div ng-show=\"settings.selectedStudent.newNoteEditMode == true\" class=\"timeline-body\">\r" +
    "\n" +
    "                                <div class=\"timeline-header\">\r" +
    "\n" +
    "                                    <span class=\"date\">{{::moment() | nsLongDateFormatWithTime}}</span>\r" +
    "\n" +
    "                                </div>\r" +
    "\n" +
    "                                <div class=\"timeline-content\">\r" +
    "\n" +
    "                                    <h3>You say:</h3>\r" +
    "\n" +
    "                                    <p>\r" +
    "\n" +
    "                                        <textarea name=\"ckeditor_studentNote\" ready=\"onReady()\" id=\"studentNote_NewNote\" cols=\"80\" rows=\"20\" class=\"ckeditor\" ng-model=\"settings.selectedStudent.NewNoteHtml\" ckeditor=\"options\"></textarea>\r" +
    "\n" +
    "                                    </p>\r" +
    "\n" +
    "                                </div>\r" +
    "\n" +
    "                                <div class=\"timeline-footer\">\r" +
    "\n" +
    "                                    <div style=\"margin-top:10px;\">\r" +
    "\n" +
    "                                        <a href=\"\" ng-click=\"saveStudentNote(-1, settings.selectedStudent.NewNoteHtml, settings.selectedStudent.id)\" class=\"btn btn-primary btn-sm pull-left\"><i class=\"fa fa-save\"></i>  Save New Note</a>\r" +
    "\n" +
    "                                        <a style=\"margin-left:5px;\" href=\"\" ng-click=\"settings.selectedStudent.newNoteEditMode = false;\" class=\"btn btn-default btn-sm pull-left\"><i class=\"fa fa-times-circle\"></i>  Cancel</a>\r" +
    "\n" +
    "                                    </div>\r" +
    "\n" +
    "                                </div>\r" +
    "\n" +
    "                            </div>\r" +
    "\n" +
    "                        </li>\r" +
    "\n" +
    "                        <li class=\"timeline-primary\" ng-repeat=\"note in studentNotesManager.Notes\">\r" +
    "\n" +
    "                            <div class=\"timeline-icon\"><i class=\"fa fa-user\"></i></div>\r" +
    "\n" +
    "                            <div class=\"timeline-body\">\r" +
    "\n" +
    "                                <div class=\"timeline-header\">\r" +
    "\n" +
    "                                    <span class=\"date\">{{::note.NoteDate | nsLongDateFormatWithTime}}</span>\r" +
    "\n" +
    "                                    <span class=\"author\" ng-if=\"note.StaffID == currentUser.Id\"><a ng-if=\"note.IsEditing != true\" style=\"margin-right:10px;\" class=\"btn btn-xs btn-default\" href=\"\" ng-click=\"note.IsEditing = true\"><i class=\"fa fa-pencil-square-o\"></i> edit</a>  <a style=\"margin-right:10px;\" class=\"btn btn-xs btn-danger\" href=\"\" ng-if=\"note.IsEditing != true\" ng-click=\"deleteStudentNote(note.Id)\"><i class=\"fa fa-times\"></i> delete</a> <a ng-if=\"note.IsEditing == true\" class=\"btn btn-xs btn-success\" href=\"\" ng-click=\"saveStudentNote(note.Id, note.Note, settings.selectedStudent.id)\"><i class=\"fa fa-save\"></i> Save Note</a> <a ng-if=\"note.IsEditing == true\" class=\"btn btn-xs btn-default\" href=\"\" ng-click=\"note.IsEditing = false\"><i class=\"fa fa-times-circle\"></i> cancel</a></span>\r" +
    "\n" +
    "\r" +
    "\n" +
    "                                </div>\r" +
    "\n" +
    "                                <div class=\"timeline-content\">\r" +
    "\n" +
    "                                    <h3>{{::note.Staff.FirstName}} {{::note.Staff.LastName}} said:</h3>\r" +
    "\n" +
    "                                    <p>\r" +
    "\n" +
    "                                        <div inlineedit=\"note.IsEditing == true\" readonly=\"{{note.StaffID != currentUser.Id && note.IsEditing != true}}\" ckeditor=\"options\" ng-model=\"note.Note\"></div>\r" +
    "\n" +
    "                                    </p>\r" +
    "\n" +
    "                                </div>\r" +
    "\n" +
    "                            </div>\r" +
    "\n" +
    "                        </li>\r" +
    "\n" +
    "                    </ul>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </uib-tab>\r" +
    "\n" +
    "            <uib-tab>\r" +
    "\n" +
    "                <uib-tab-heading>\r" +
    "\n" +
    "                    <i class=\"fa fa-file-text-o\"></i> Team Meeting Notes\r" +
    "\n" +
    "                </uib-tab-heading>\r" +
    "\n" +
    "                <div ng-if=\"studentTMNotesManager.Meetings.length == 0\">\r" +
    "\n" +
    "                    <uib-alert type=\"info\"><b>Note:</b> Student has not been involved in any team meetings</uib-alert>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div ng-repeat=\"meeting in studentTMNotesManager.Meetings\">\r" +
    "\n" +
    "                    <h4 class=\"timeline-month\"><span>{{::meeting.Title}}</span>  <small>({{::meeting.MeetingTime | nsLongDateFormatWithTime}})</small></h4>\r" +
    "\n" +
    "                    <ul class=\"timeline\">\r" +
    "\n" +
    "                        <li class=\"timeline-midnightblue\">\r" +
    "\n" +
    "                            <div class=\"timeline-icon\"><i class=\"fa fa-plus\"></i></div>\r" +
    "\n" +
    "                            <div class=\"timeline-body\" ng-show=\"!meeting.newNoteEditMode\">\r" +
    "\n" +
    "                                <button class=\"btn btn-success\" ng-click=\"meeting.newNoteEditMode = true\"><i class=\"fa fa-plus\"></i> Add New Note to This Meeting</button>\r" +
    "\n" +
    "                            </div>\r" +
    "\n" +
    "                            <div ng-show=\"meeting.newNoteEditMode == true\" class=\"timeline-body\">\r" +
    "\n" +
    "                                <div class=\"timeline-header\">\r" +
    "\n" +
    "                                    <span class=\"date\">{{::moment() | nsLongDateFormatWithTime}}</span>\r" +
    "\n" +
    "                                </div>\r" +
    "\n" +
    "                                <div class=\"timeline-content\">\r" +
    "\n" +
    "                                    <h3>You say:</h3>\r" +
    "\n" +
    "                                    <p>\r" +
    "\n" +
    "                                        <textarea name=\"ckeditor_{{::meeting.Id}}\" ready=\"onReady()\" id=\"{{::meeting.Id}}_NewNote\" cols=\"80\" rows=\"20\" class=\"ckeditor\" ng-model=\"meeting.NewNoteHtml\" ckeditor=\"options\"></textarea>\r" +
    "\n" +
    "                                    </p>\r" +
    "\n" +
    "                                </div>\r" +
    "\n" +
    "                                <div class=\"timeline-footer\">\r" +
    "\n" +
    "                                    <div style=\"margin-top:10px;\">\r" +
    "\n" +
    "                                        <a href=\"\" ng-click=\"saveNote(-1, meeting.NewNoteHtml, meeting.Id, settings.selectedStudent.id, meeting)\" class=\"btn btn-primary btn-sm pull-left\"><i class=\"fa fa-save\"></i>  Save New Note</a>\r" +
    "\n" +
    "                                        <a style=\"margin-left:5px;\" href=\"\" ng-click=\"meeting.newNoteEditMode = false;\" class=\"btn btn-default btn-sm pull-left\"><i class=\"fa fa-times-circle\"></i>  Cancel</a>\r" +
    "\n" +
    "                                    </div>\r" +
    "\n" +
    "                                </div>\r" +
    "\n" +
    "                            </div>\r" +
    "\n" +
    "                        </li>\r" +
    "\n" +
    "                        <li class=\"timeline-primary\" ng-repeat=\"note in meeting.TeamMeetingStudentNotes\">\r" +
    "\n" +
    "                            <div class=\"timeline-icon\"><i class=\"fa fa-user\"></i></div>\r" +
    "\n" +
    "                            <div class=\"timeline-body\">\r" +
    "\n" +
    "                                <div class=\"timeline-header\">\r" +
    "\n" +
    "                                    <span class=\"date\">{{::note.NoteDate | nsLongDateFormatWithTime}}</span>\r" +
    "\n" +
    "                                    <span class=\"author\" ng-if=\"note.StaffID == currentUser.Id\"><a ng-if=\"note.IsEditing != true\" style=\"margin-right:10px;\" class=\"btn btn-xs btn-default\" href=\"\" ng-click=\"note.IsEditing = true\"><i class=\"fa fa-pencil-square-o\"></i> edit</a>  <a style=\"margin-right:10px;\" class=\"btn btn-xs btn-danger\" href=\"\" ng-if=\"note.IsEditing != true\" ng-click=\"deleteNote(note.ID)\"><i class=\"fa fa-times\"></i> delete</a> <a ng-if=\"note.IsEditing == true\" class=\"btn btn-xs btn-success\" href=\"\" ng-click=\"saveNote(note.ID, note.Note, meeting.Id, settings.selectedStudent.id)\"><i class=\"fa fa-save\"></i> Save Note</a> <a ng-if=\"note.IsEditing == true\" class=\"btn btn-xs btn-default\" href=\"\" ng-click=\"note.IsEditing = false\"><i class=\"fa fa-times-circle\"></i> cancel</a></span>\r" +
    "\n" +
    "\r" +
    "\n" +
    "                                </div>\r" +
    "\n" +
    "                                <div class=\"timeline-content\">\r" +
    "\n" +
    "                                    <h3>{{::note.Staff.FirstName}} {{::note.Staff.LastName}} said:</h3>\r" +
    "\n" +
    "                                    <p>\r" +
    "\n" +
    "                                        <div inlineedit=\"note.IsEditing == true\" readonly=\"{{note.StaffID != currentUser.Id && note.IsEditing != true}}\" ckeditor=\"options\" ng-model=\"note.Note\"></div>\r" +
    "\n" +
    "                                    </p>\r" +
    "\n" +
    "                                </div>\r" +
    "\n" +
    "                            </div>\r" +
    "\n" +
    "                        </li>\r" +
    "\n" +
    "                    </ul>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </uib-tab>\r" +
    "\n" +
    "        </uib-tabset>\r" +
    "\n" +
    "    </div>\r" +
    "\n" +
    "</div>\r" +
    "\n" +
    "\r" +
    "\n" +
    "<style>\r" +
    "\n" +
    "    .studentLabel{\r" +
    "\n" +
    "        text-align:right;\r" +
    "\n" +
    "        font-size:14px;\r" +
    "\n" +
    "        padding: 3px 10px 3px 3px;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "    .studentText{\r" +
    "\n" +
    "        font-size: 14px;\r" +
    "\n" +
    "        padding:3px;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "    .studentPhoto {\r" +
    "\n" +
    "        max-width: 200px;\r" +
    "\n" +
    "        border-radius: 20%;\r" +
    "\n" +
    "        margin:auto;\r" +
    "\n" +
    "        display:block;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "</style>"
  );


  $templateCache.put('templates/student-dashboard-link.html',
    "<!--<a style=\"font-weight:bold;color:black\" href=\"#/student-dashboard/{{studentId}}\">{{::studentName}}</a>-->\r" +
    "\n" +
    "\r" +
    "\n" +
    "<a style=\"font-weight:bold;color:black\" href=\"\" ng-click=\"openStudentDashboardDialog()\">{{studentName}}</a>"
  );


  $templateCache.put('templates/student-dashboard-print-directive.html',
    "\r" +
    "\n" +
    "<div style=\"min-height:350px;\">\r" +
    "\n" +
    "    <spinner name=\"tableSpinner\">\r" +
    "\n" +
    "        <div style=\"position:absolute;width:100%;height:100%;background-color:rgba(100, 100, 100, 0.7);z-index:100;\" class=\"hidden-print\">\r" +
    "\n" +
    "            <div class=\"sk-cube-grid\">\r" +
    "\n" +
    "                <div class=\"sk-cube sk-cube1\"></div>\r" +
    "\n" +
    "                <div class=\"sk-cube sk-cube2\"></div>\r" +
    "\n" +
    "                <div class=\"sk-cube sk-cube3\"></div>\r" +
    "\n" +
    "                <div class=\"sk-cube sk-cube4\"></div>\r" +
    "\n" +
    "                <div class=\"sk-cube sk-cube5\"></div>\r" +
    "\n" +
    "                <div class=\"sk-cube sk-cube6\"></div>\r" +
    "\n" +
    "                <div class=\"sk-cube sk-cube7\"></div>\r" +
    "\n" +
    "                <div class=\"sk-cube sk-cube8\"></div>\r" +
    "\n" +
    "                <div class=\"sk-cube sk-cube9\"></div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "        </div>\r" +
    "\n" +
    "    </spinner>\r" +
    "\n" +
    "    <div class=\"col-md-12\" ng-if=\"settings.studentLoaded\">\r" +
    "\n" +
    "        \r" +
    "\n" +
    "        <h1>Student Dashboard for {{::settings.selectedStudent.text}}</h1>\r" +
    "\n" +
    "\r" +
    "\n" +
    "        <div ng-if=\"settings.tab == 'dt' || !settings.tab\">\r" +
    "\n" +
    "            <h2><i class=\"fa fa-user\"></i> Student Details</h2>\r" +
    "\n" +
    "            <!-- Photo -->\r" +
    "\n" +
    "            <div ng-show=\"fullStudent.ImageUrl != '' && fullStudent.ImageUrl != null\">\r" +
    "\n" +
    "                <img class=\"studentPhoto\" ng-src=\"{{::fullStudent.ImageUrl}}\" alt=\"{{::studentResult.StudentName}}\" />\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "            <div ng-if=\"fullStudent.ImageUrl == '' || fullStudent.ImageUrl == null\">\r" +
    "\n" +
    "                <img class=\"studentPhoto\" src=\"/assets/img/placeholder_person.jpg\" />\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "            <fieldset>\r" +
    "\n" +
    "                <legend>Details</legend>\r" +
    "\n" +
    "                <div class=\"row\">\r" +
    "\n" +
    "                    <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-6 control-label studentLabel\">First Name</label>\r" +
    "\n" +
    "                    <div class=\"col-md-4 studentText\">\r" +
    "\n" +
    "                        {{::fullStudent.FirstName}}\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"row\">\r" +
    "\n" +
    "                    <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-6 control-label studentLabel\">Middle Name</label>\r" +
    "\n" +
    "                    <div class=\"col-md-6 studentText\">\r" +
    "\n" +
    "                        {{::fullStudent.MiddleName}}\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"row\">\r" +
    "\n" +
    "                    <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-6 control-label studentLabel\">Last Name</label>\r" +
    "\n" +
    "                    <div class=\"col-md-6 studentText\">\r" +
    "\n" +
    "                        {{::fullStudent.LastName}}\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"row\">\r" +
    "\n" +
    "                    <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-6 control-label studentLabel\">Student ID</label>\r" +
    "\n" +
    "                    <div class=\"col-md-6 studentText\">\r" +
    "\n" +
    "                        {{::fullStudent.StudentIdentifier}}\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"row\">\r" +
    "\n" +
    "                    <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-6 control-label studentLabel\">Date of Birth</label>\r" +
    "\n" +
    "                    <div class=\"col-md-6 studentText\">\r" +
    "\n" +
    "                        {{::fullStudent.DOB}}\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"row\">\r" +
    "\n" +
    "                    <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-6 control-label studentLabel\">Graduation Year</label>\r" +
    "\n" +
    "                    <div class=\"col-md-6 studentText\">\r" +
    "\n" +
    "                        {{::fullStudent.GraduationYear}}\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div class=\"row\">\r" +
    "\n" +
    "                    <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-6 control-label studentLabel\">Enrollment Year</label>\r" +
    "\n" +
    "                    <div class=\"col-md-6 studentText\">{{::fullStudent.EnrollmentYear}}</div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "\r" +
    "\n" +
    "            </fieldset>\r" +
    "\n" +
    "            <fieldset>\r" +
    "\n" +
    "                <legend>Student Attributes</legend>\r" +
    "\n" +
    "                <div class=\"row\" ng-repeat=\"attribute in allAttributes.AllAttributes\">\r" +
    "\n" +
    "                    <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-6 control-label studentLabel\">{{::attribute.AttributeName}}</label>\r" +
    "\n" +
    "                    <div class=\"col-md-6 studentText\">\r" +
    "\n" +
    "                        {{::getAttributeValue(fullStudent.StudentAttributes[attribute.Id], attribute)}}\r" +
    "\n" +
    "                        <!--<select class=\"form-control\" ng-model=\"fullStudent.StudentAttributes[attribute.Id]\" ng-options=\"v.LookupValueId as v.LookupValue for v in attribute.LookupValues\">\r" +
    "\n" +
    "                        <option value=\"\">-select-</option>\r" +
    "\n" +
    "                    </select>-->\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </fieldset>\r" +
    "\n" +
    "            <fieldset>\r" +
    "\n" +
    "                <legend>Education Services</legend>\r" +
    "\n" +
    "                <div class=\"row\">\r" +
    "\n" +
    "                    <label style=\"font-weight:bold;margin-bottom:0px\" class=\"col-sm-6 control-label studentLabel\">Services</label>\r" +
    "\n" +
    "                    <div class=\"col-md-6 studentText\">\r" +
    "\n" +
    "                        <div ng-repeat=\"label in fullStudent.SpecialEdLabels\">\r" +
    "\n" +
    "                            {{::label.text}}\r" +
    "\n" +
    "                        </div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </fieldset>\r" +
    "\n" +
    "            <div class=\"breaker\"></div>\r" +
    "\n" +
    "        </div>\r" +
    "\n" +
    "        <div ng-if=\"settings.tab == 'lg' || !settings.tab\">\r" +
    "\n" +
    "        <h2><i class=\"fa fa-line-chart\"></i> Class Line Graphs</h2>\r" +
    "\n" +
    "        \r" +
    "\n" +
    "                <div>\r" +
    "\n" +
    "                    <div ng-if=\"ClassLineGraphDataManagers.length == 0\">\r" +
    "\n" +
    "                        <uib-alert type=\"info\"><b>Note:</b> Student has not taken any benchmarked assessments</uib-alert>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                    <div ng-repeat=\"fieldMgr in ClassLineGraphDataManagers\" style=\"margin-bottom:50px;\">\r" +
    "\n" +
    "                        <h4>{{::settings.selectedStudent.text}}</h4>\r" +
    "\n" +
    "                        <div class=\"col-md-12\">\r" +
    "\n" +
    "                            <div>\r" +
    "\n" +
    "                                <highchart id=\"chart1\" config=\"fieldMgr.chartConfig\"></highchart>\r" +
    "\n" +
    "                            </div>\r" +
    "\n" +
    "                            <div>\r" +
    "\n" +
    "                                <line-graph-detail data-data-manager=\"fieldMgr\"></line-graph-detail>\r" +
    "\n" +
    "                            </div>\r" +
    "\n" +
    "                        </div>\r" +
    "\n" +
    "                        <div style=\"clear:both;\" />\r" +
    "\n" +
    "                        <div class=\"col-md-12 breaker\" ng-if=\"$index != ClassLineGraphDataManagers.length - 1 || !settings.tab\">&nbsp;</div>\r" +
    "\n" +
    "                    </div>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "        </div>\r" +
    "\n" +
    "        \r" +
    "\n" +
    "        <div ng-if=\"settings.tab == 'os' || !settings.tab\">\r" +
    "\n" +
    "            <h2><i class=\"fa fa-folder-open\"></i> Student Cumulative Folder</h2>\r" +
    "\n" +
    "            <h4>{{::settings.selectedStudent.text}}</h4>\r" +
    "\n" +
    "            <div class=\"table-responsive\">\r" +
    "\n" +
    "                <ns-observation-summary-student selected-student-id=\"settings.selectedStudent.id\"></ns-observation-summary-student>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "            <div style=\"margin-bottom:25px;\">\r" +
    "\n" +
    "                <div>\r" +
    "\n" +
    "                    <table>\r" +
    "\n" +
    "                        <tbody>\r" +
    "\n" +
    "                            <tr>\r" +
    "\n" +
    "                                <td style=\"padding-right:5px;\">\r" +
    "\n" +
    "                                    <span class=\"obsPerfect\" style=\"border:1px solid black;display:inline-block;height:20px;width:25px;\"></span>\r" +
    "\n" +
    "                                </td>\r" +
    "\n" +
    "                                <td style=\"padding-right:10px\">\r" +
    "\n" +
    "                                    <strong>Perfect Score</strong>\r" +
    "\n" +
    "                                </td>\r" +
    "\n" +
    "                                <td style=\"padding-right:5px;\">\r" +
    "\n" +
    "                                    <span class=\"obsBlue\" style=\"border:1px solid black;display:inline-block;height:20px;width:25px;\"></span>\r" +
    "\n" +
    "                                </td>\r" +
    "\n" +
    "                                <td style=\"padding-right:10px\">\r" +
    "\n" +
    "                                    <strong>Exceeds Expectations</strong>\r" +
    "\n" +
    "                                </td>\r" +
    "\n" +
    "                                <td style=\"padding-right:5px;\">\r" +
    "\n" +
    "                                    <span class=\"obsGreen\" style=\"border:1px solid black;display:inline-block;height:20px;width:25px;\"></span>\r" +
    "\n" +
    "                                </td>\r" +
    "\n" +
    "                                <td style=\"padding-right:10px\">\r" +
    "\n" +
    "                                    <strong>Meets Expectations</strong>\r" +
    "\n" +
    "                                </td>\r" +
    "\n" +
    "                                <td style=\"padding-right:5px;\">\r" +
    "\n" +
    "                                    <span class=\"obsYellow\" style=\"border:1px solid black;display:inline-block;height:20px;width:25px;\"></span>\r" +
    "\n" +
    "                                </td>\r" +
    "\n" +
    "                                <td style=\"padding-right:10px\">\r" +
    "\n" +
    "                                    <strong>Approaches Expectations</strong>\r" +
    "\n" +
    "                                </td>\r" +
    "\n" +
    "                                <td style=\"padding-right:5px;\">\r" +
    "\n" +
    "                                    <span class=\"obsRed\" style=\"border:1px solid black;display:inline-block;height:20px;width:25px;\"></span>\r" +
    "\n" +
    "                                </td>\r" +
    "\n" +
    "                                <td style=\"padding-right:10px\">\r" +
    "\n" +
    "                                    <strong>Does Not Meet Expectations</strong>\r" +
    "\n" +
    "                                </td>\r" +
    "\n" +
    "                            </tr>\r" +
    "\n" +
    "                        </tbody>\r" +
    "\n" +
    "                    </table>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "        </div>\r" +
    "\n" +
    "\r" +
    "\n" +
    "        <div ng-if=\"settings.tab == 'intv' || !settings.tab\">\r" +
    "\n" +
    "            <h2><i class=\"fa fa-bolt\"></i> Interventions</h2>\r" +
    "\n" +
    "            <h4>{{::settings.selectedStudent.text}}</h4>\r" +
    "\n" +
    "                <div ng-if=\"studentDataManager.Interventions.length  == 0\">\r" +
    "\n" +
    "                    <uib-alert type=\"info\"><b>Note:</b> Student has not received any interventions</uib-alert>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "                <div ng-if=\"studentDataManager.Interventions.length > 0\">\r" +
    "\n" +
    "                    <student-interventions data-data-manager=\"studentDataManager\"></student-interventions>\r" +
    "\n" +
    "                </div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "        \r" +
    "\n" +
    "        <div ng-if=\"settings.tab == 'sn' || !settings.tab\">\r" +
    "\n" +
    "            <h2><i class=\"fa fa-paperclip\"></i> Student Notes</h2>\r" +
    "\n" +
    "            <h4>{{::settings.selectedStudent.text}}</h4>\r" +
    "\n" +
    "            <div ng-if=\"studentNotesManager.Notes.length == 0\">\r" +
    "\n" +
    "                <uib-alert type=\"info\"><b>Note:</b> Student does not have any notes</uib-alert>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "            <div>\r" +
    "\n" +
    "                <ul class=\"timeline\">\r" +
    "\n" +
    "                    <li class=\"timeline-midnightblue\">\r" +
    "\n" +
    "                        <div ng-show=\"settings.selectedStudent.newNoteEditMode == true\" class=\"timeline-body\">\r" +
    "\n" +
    "                            <div class=\"timeline-header\">\r" +
    "\n" +
    "                                <span class=\"date\">{{::moment() | nsLongDateFormatWithTime}}</span>\r" +
    "\n" +
    "                            </div>\r" +
    "\n" +
    "                            <div class=\"timeline-footer\">\r" +
    "\n" +
    "                                <div style=\"margin-top:10px;\">\r" +
    "\n" +
    "                                    <a href=\"\" ng-click=\"saveStudentNote(-1, settings.selectedStudent.NewNoteHtml, settings.selectedStudent.id)\" class=\"btn btn-primary btn-sm pull-left\"><i class=\"fa fa-save\"></i>  Save New Note</a>\r" +
    "\n" +
    "                                    <a style=\"margin-left:5px;\" href=\"\" ng-click=\"settings.selectedStudent.newNoteEditMode = false;\" class=\"btn btn-default btn-sm pull-left\"><i class=\"fa fa-times-circle\"></i>  Cancel</a>\r" +
    "\n" +
    "                                </div>\r" +
    "\n" +
    "                            </div>\r" +
    "\n" +
    "                        </div>\r" +
    "\n" +
    "                    </li>\r" +
    "\n" +
    "                    <li class=\"timeline-primary\" ng-repeat=\"note in studentNotesManager.Notes\">\r" +
    "\n" +
    "                        <div class=\"timeline-icon\"><i class=\"fa fa-user\"></i></div>\r" +
    "\n" +
    "                        <div class=\"timeline-body\">\r" +
    "\n" +
    "                            <div class=\"timeline-header\">\r" +
    "\n" +
    "                                <span class=\"date\">{{::note.NoteDate | nsLongDateFormatWithTime}}</span>\r" +
    "\n" +
    "                                <span class=\"author\" ng-if=\"note.StaffID == currentUser.Id\"><a ng-if=\"note.IsEditing != true\" style=\"margin-right:10px;\" class=\"btn btn-xs btn-default\" href=\"\" ng-click=\"note.IsEditing = true\"><i class=\"fa fa-pencil-square-o\"></i> edit</a>  <a style=\"margin-right:10px;\" class=\"btn btn-xs btn-danger\" href=\"\" ng-if=\"note.IsEditing != true\" ng-click=\"deleteStudentNote(note.Id)\"><i class=\"fa fa-times\"></i> delete</a> <a ng-if=\"note.IsEditing == true\" class=\"btn btn-xs btn-success\" href=\"\" ng-click=\"saveStudentNote(note.Id, note.Note, settings.selectedStudent.id)\"><i class=\"fa fa-save\"></i> Save Note</a> <a ng-if=\"note.IsEditing == true\" class=\"btn btn-xs btn-default\" href=\"\" ng-click=\"note.IsEditing = false\"><i class=\"fa fa-times-circle\"></i> cancel</a></span>\r" +
    "\n" +
    "\r" +
    "\n" +
    "                            </div>\r" +
    "\n" +
    "                            <div class=\"timeline-content\">\r" +
    "\n" +
    "                                <h3>{{::note.Staff.FirstName}} {{::note.Staff.LastName}} said:</h3>\r" +
    "\n" +
    "                                <p>\r" +
    "\n" +
    "                                    <div inlineedit=\"note.IsEditing == true\" readonly=\"{{note.StaffID != currentUser.Id && note.IsEditing != true}}\" ckeditor=\"options\" ng-model=\"note.Note\"></div>\r" +
    "\n" +
    "                                </p>\r" +
    "\n" +
    "                            </div>\r" +
    "\n" +
    "                        </div>\r" +
    "\n" +
    "                    </li>\r" +
    "\n" +
    "                </ul>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "\r" +
    "\n" +
    "        <div ng-if=\"settings.tab == 'tmn' || !settings.tab\">\r" +
    "\n" +
    "            <h2><i class=\"fa fa-file-text-o\"></i> Team Meeting Notes</h2>\r" +
    "\n" +
    "            <h4>{{::settings.selectedStudent.text}}</h4>\r" +
    "\n" +
    "            <div ng-if=\"studentTMNotesManager.Meetings.length == 0\">\r" +
    "\n" +
    "                <uib-alert type=\"info\"><b>Note:</b> Student has not been involved in any team meetings</uib-alert>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "            <div ng-repeat=\"meeting in studentTMNotesManager.Meetings\">\r" +
    "\n" +
    "                <h4 class=\"timeline-month\"><span>{{::meeting.Title}}</span>  <small>({{::meeting.MeetingTime | nsLongDateFormatWithTime}})</small></h4>\r" +
    "\n" +
    "                <ul class=\"timeline\">\r" +
    "\n" +
    "                    <li class=\"timeline-midnightblue\">\r" +
    "\n" +
    "                        <div ng-show=\"meeting.newNoteEditMode == true\" class=\"timeline-body\">\r" +
    "\n" +
    "                            <div class=\"timeline-header\">\r" +
    "\n" +
    "                                <span class=\"date\">{{::moment() | nsLongDateFormatWithTime}}</span>\r" +
    "\n" +
    "                            </div>\r" +
    "\n" +
    "                            <div class=\"timeline-footer\">\r" +
    "\n" +
    "                                <div style=\"margin-top:10px;\">\r" +
    "\n" +
    "                                    <a href=\"\" ng-click=\"saveNote(-1, meeting.NewNoteHtml, meeting.Id, settings.selectedStudent.id, meeting)\" class=\"btn btn-primary btn-sm pull-left\"><i class=\"fa fa-save\"></i>  Save New Note</a>\r" +
    "\n" +
    "                                    <a style=\"margin-left:5px;\" href=\"\" ng-click=\"meeting.newNoteEditMode = false;\" class=\"btn btn-default btn-sm pull-left\"><i class=\"fa fa-times-circle\"></i>  Cancel</a>\r" +
    "\n" +
    "                                </div>\r" +
    "\n" +
    "                            </div>\r" +
    "\n" +
    "                        </div>\r" +
    "\n" +
    "                    </li>\r" +
    "\n" +
    "                    <li class=\"timeline-primary\" ng-repeat=\"note in meeting.TeamMeetingStudentNotes\">\r" +
    "\n" +
    "                        <div class=\"timeline-icon\"><i class=\"fa fa-user\"></i></div>\r" +
    "\n" +
    "                        <div class=\"timeline-body\">\r" +
    "\n" +
    "                            <div class=\"timeline-header\">\r" +
    "\n" +
    "                                <span class=\"date\">{{::note.NoteDate | nsLongDateFormatWithTime}}</span>\r" +
    "\n" +
    "                                <span class=\"author\" ng-if=\"note.StaffID == currentUser.Id\"><a ng-if=\"note.IsEditing != true\" style=\"margin-right:10px;\" class=\"btn btn-xs btn-default\" href=\"\" ng-click=\"note.IsEditing = true\"><i class=\"fa fa-pencil-square-o\"></i> edit</a>  <a style=\"margin-right:10px;\" class=\"btn btn-xs btn-danger\" href=\"\" ng-if=\"note.IsEditing != true\" ng-click=\"deleteNote(note.ID)\"><i class=\"fa fa-times\"></i> delete</a> <a ng-if=\"note.IsEditing == true\" class=\"btn btn-xs btn-success\" href=\"\" ng-click=\"saveNote(note.ID, note.Note, meeting.Id, settings.selectedStudent.id)\"><i class=\"fa fa-save\"></i> Save Note</a> <a ng-if=\"note.IsEditing == true\" class=\"btn btn-xs btn-default\" href=\"\" ng-click=\"note.IsEditing = false\"><i class=\"fa fa-times-circle\"></i> cancel</a></span>\r" +
    "\n" +
    "\r" +
    "\n" +
    "                            </div>\r" +
    "\n" +
    "                            <div class=\"timeline-content\">\r" +
    "\n" +
    "                                <h3>{{::note.Staff.FirstName}} {{::note.Staff.LastName}} said:</h3>\r" +
    "\n" +
    "                                <p>\r" +
    "\n" +
    "                                    <div inlineedit=\"note.IsEditing == true\" readonly=\"{{note.StaffID != currentUser.Id && note.IsEditing != true}}\" ckeditor=\"options\" ng-model=\"note.Note\"></div>\r" +
    "\n" +
    "                                </p>\r" +
    "\n" +
    "                            </div>\r" +
    "\n" +
    "                        </div>\r" +
    "\n" +
    "                    </li>\r" +
    "\n" +
    "                </ul>\r" +
    "\n" +
    "            </div>\r" +
    "\n" +
    "        </div>\r" +
    "\n" +
    "    </div>\r" +
    "\n" +
    "</div>\r" +
    "\n" +
    "\r" +
    "\n" +
    "<style>\r" +
    "\n" +
    "    .interventionCell {\r" +
    "\n" +
    "        border: 3px dotted blue !important;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "\r" +
    "\n" +
    "    .verticalTdDate {\r" +
    "\n" +
    "            text-align:center;\r" +
    "\n" +
    "    white-space:nowrap;\r" +
    "\n" +
    "    transform: rotate(90deg);\r" +
    "\n" +
    "        /*-webkit-transform: rotate(90deg);\r" +
    "\n" +
    "        -moz-transform: rotate(90deg);\r" +
    "\n" +
    "        -o-transform: rotate(90deg);\r" +
    "\n" +
    "        transform-origin:bottom left;\r" +
    "\n" +
    "        transform: rotate(90deg);\r" +
    "\n" +
    "        white-space: nowrap;\r" +
    "\n" +
    "        display: block;\r" +
    "\n" +
    "        bottom: 0;\r" +
    "\n" +
    "        width: 10px;\r" +
    "\n" +
    "        height: 20px;\r" +
    "\n" +
    "        font-size: 12px;\r" +
    "\n" +
    "        font-weight: bold;*/\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "\r" +
    "\n" +
    "    .verticalTdDate div {\r" +
    "\n" +
    "         margin:0 -999px;/* virtually reduce space needed on width to very little */\r" +
    "\n" +
    "    display:inline-block;\r" +
    "\n" +
    "        /*height: 80px;\r" +
    "\n" +
    "        padding-top: 8px;\r" +
    "\n" +
    "        padding-left: 8px;\r" +
    "\n" +
    "        vertical-align: top;\r" +
    "\n" +
    "        position:relative;\r" +
    "\n" +
    "        top: -35px;\r" +
    "\n" +
    "        left: 3px;*/\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "        .verticalTdDate div:before {\r" +
    "\n" +
    "            content:'';\r" +
    "\n" +
    "    width:0;\r" +
    "\n" +
    "    padding-top:110%;\r" +
    "\n" +
    "    /* takes width as reference, + 10% for faking some extra padding */\r" +
    "\n" +
    "    display:inline-block;\r" +
    "\n" +
    "    vertical-align:middle;\r" +
    "\n" +
    "        }\r" +
    "\n" +
    "\r" +
    "\n" +
    "    .table-lowPadding > thead > tr > th, .table-lowPadding > tbody > tr > td {\r" +
    "\n" +
    "        padding: 3px 3px !important;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "\r" +
    "\n" +
    "    .leftDoubleBorder {\r" +
    "\n" +
    "        border-left: 3px solid black !important;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "\r" +
    "\n" +
    "    .rightDoubleBorder {\r" +
    "\n" +
    "        border-right: 3px solid black !important;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "\r" +
    "\n" +
    "    .nsWhiteCell {\r" +
    "\n" +
    "        background-color: white !important;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "\r" +
    "\n" +
    "    .table-bordered > thead > tr > th, .table-bordered > tbody > tr > th, .table-bordered > tfoot > tr > th, .table-bordered > thead > tr > td, .table-bordered > tbody > tr > td, .table-bordered > tfoot > tr > td {\r" +
    "\n" +
    "        border: 1px solid black;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "\r" +
    "\n" +
    "    .table-bordered {\r" +
    "\n" +
    "        border: 1px solid black;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "</style>\r" +
    "\n" +
    "<!--<style>\r" +
    "\n" +
    "    .timeline-primary div.timeline-body{\r" +
    "\n" +
    "        background-color:none  !important;\r" +
    "\n" +
    "        border-color: none  !important;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "    .timeline > li div.timeline-body {\r" +
    "\n" +
    "        color:black !important;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "    .timeline > li div.timeline-header {\r" +
    "\n" +
    "    color: black !important;\r" +
    "\n" +
    "}\r" +
    "\n" +
    "</style>-->\r" +
    "\n" +
    "<style>\r" +
    "\n" +
    "    .studentLabel {\r" +
    "\n" +
    "        text-align: right;\r" +
    "\n" +
    "        font-size: 14px;\r" +
    "\n" +
    "        padding: 3px 10px 3px 3px;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "\r" +
    "\n" +
    "    .studentText {\r" +
    "\n" +
    "        font-size: 14px;\r" +
    "\n" +
    "        padding: 3px;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "\r" +
    "\n" +
    "    .studentPhoto {\r" +
    "\n" +
    "        max-width: 200px;\r" +
    "\n" +
    "        border-radius: 20%;\r" +
    "\n" +
    "        margin: auto;\r" +
    "\n" +
    "        display: block;\r" +
    "\n" +
    "    }\r" +
    "\n" +
    "</style>"
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
    "            <th>\r" +
    "\n" +
    "                Dashboard\r" +
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
    "            <td>{{::intervention.StartOfInterventionString}}</td>\r" +
    "\n" +
    "            <td>{{::intervention.EndOfInterventionString}}</td>\r" +
    "\n" +
    "            <td>{{::intervention.StaffInitials}}</td>\r" +
    "\n" +
    "            <td>\r" +
    "\n" +
    "                <intervention-dashboard-link intervention=\"intervention\"></intervention-dashboard-link>\r" +
    "\n" +
    "                <!--<button class=\"btn btn-xs btn-primary\" ng-click=\"goToDashboard(intervention.SchoolYear, intervention.SchoolId, intervention.InterventionistId, intervention.InterventionGroupId, intervention.StudentID, intervention.Id)\"><i class=\"fa fa-dashboard\"></i> Go</button>-->\r" +
    "\n" +
    "            </td>\r" +
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