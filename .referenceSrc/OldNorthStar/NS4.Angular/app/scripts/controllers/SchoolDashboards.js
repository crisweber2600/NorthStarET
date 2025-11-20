(function () {
    'use strict'

    angular
	.module('schoolDashboardsModule', [])
        	.controller('SchoolDashboardMCAPrelimController', [
        '$bootbox', 'nsPinesService', 'nsStackedBarGraphOptionsFactory', '$scope', 'nsFilterOptionsService', '$q', '$filter', 'NSSummarySortManager', 'spinnerService', function ($bootbox, nsPinesService, nsStackedBarGraphOptionsFactory, $scope, nsFilterOptionsService, $q, $filter, NSSummarySortManager, spinnerService) {
            $scope.settings = {};
            $scope.settings.summaryMode = false;
            $scope.settings.summaryView = 'Current Year Only (Sortable)';
            $scope.settings.summaryCategory = '';
            $scope.settings.summaryScoreGrouping = 1;
            $scope.settings.stacking = 'normal';
            $scope.settings.stackingDescription = 'Number of Students';
            $scope.settings.graphGenerated = false;
            $scope.settings.firstLoadStarted = false;

            $scope.sortArray = [];
            $scope.attributeHeaders = {};
            $scope.manualSortHeaders = {};
            $scope.manualSortHeaders['Student'] = "fa";
            $scope.manualSortHeaders['StudentIdentifier'] = "fa";
            $scope.manualSortHeaders['SpecialED'] = "fa";
            $scope.manualSortHeaders['Services'] = "fa";
            $scope.manualSortHeaders['School'] = "fa";
            $scope.manualSortHeaders['Grade'] = "fa";
            $scope.manualSortHeaders['Teacher'] = "fa";
            $scope.manualSortHeaders['HomeLanguage'] = "fa";
            $scope.headerClassArray = [];
            $scope.sortMgr = new NSSummarySortManager();

            $scope.sort = function (fieldName) {
                $scope.sortMgr.sort(fieldName);
            };


            var highchartsNgConfig = {};
            $scope.groupsFactory = {};

            $scope.comparisonGroups = [];
            $scope.comparisonGroups.push(new nsStackedBarGraphOptionsFactory("MCA Reading - Prelim", false, true));
            $scope.comparisonGroups.push(new nsStackedBarGraphOptionsFactory("MCA Math - Prelim", false, true));
            $scope.comparisonGroups.push(new nsStackedBarGraphOptionsFactory("MCA Science - Prelim", false, true));

            $scope.filterOptions = nsFilterOptionsService.options;

            // TODO: fix hack, actually go and get assessmentfield.assessment.testtype
            $scope.getSummaryHeader = function(displayDate){
                if ($scope.groupsFactory.TestDueDates.length == 1) {
                    return $scope.groupsFactory.options.selectedSchoolYear.text;
                } else {
                    return displayDate;
                }
            }

            // add initial groups (might be prelim or historical, based on checkbox)
            // TODO: FYI, need a benchmark date from THIS YEAR
            var loadInitialGroups = function () {
                $scope.comparisonGroups[0].options.selectedSchools = new Array($scope.filterOptions.selectedSchool);
                $scope.comparisonGroups[0].options.selectedGrades = $scope.filterOptions.selectedGrade == null ? [] : new Array($scope.filterOptions.selectedGrade);
                $scope.comparisonGroups[0].options.selectedSchoolYear = $scope.filterOptions.selectedSchoolYear;
            //    $scope.comparisonGroups[0].options.selectedTestDueDate = { id: 401, text: "crap" };
                // todo - get from db
                var readingField = { DisplayLabel: 'Achievement Level', DatabaseColumn: 'AchievementLevelId', FieldType: 'DropdownFromDB', LookupFieldName: 'AchievementLevel', FieldName: 'AchievementLevelId', AssessmentId: 55 };
                $scope.comparisonGroups[0].options.selectedAssessmentField = readingField;

                $scope.comparisonGroups[1].options.selectedSchools = new Array($scope.filterOptions.selectedSchool);
                $scope.comparisonGroups[1].options.selectedGrades = $scope.filterOptions.selectedGrade == null ? [] : new Array($scope.filterOptions.selectedGrade);
                $scope.comparisonGroups[1].options.selectedSchoolYear = $scope.filterOptions.selectedSchoolYear;
              //  $scope.comparisonGroups[1].options.selectedTestDueDate = { id: 401, text: "crap" };
                // todo - get from db
                var mathField = { DisplayLabel: 'Achievement Level', DatabaseColumn: 'AchievementLevelId', FieldType: 'DropdownFromDB', LookupFieldName: 'AchievementLevel', FieldName: 'AchievementLevelId', AssessmentId: 57 };
                $scope.comparisonGroups[1].options.selectedAssessmentField = mathField;

                $scope.comparisonGroups[2].options.selectedSchools = new Array($scope.filterOptions.selectedSchool);
                $scope.comparisonGroups[2].options.selectedGrades = $scope.filterOptions.selectedGrade == null ? [] : new Array($scope.filterOptions.selectedGrade);
                $scope.comparisonGroups[2].options.selectedSchoolYear = $scope.filterOptions.selectedSchoolYear;
               // $scope.comparisonGroups[2].options.selectedTestDueDate = { id: 401, text: "crap" };
                // todo - get from db
                var scienceField = { DisplayLabel: 'Achievement Level', DatabaseColumn: 'AchievementLevelId', FieldType: 'DropdownFromDB', LookupFieldName: 'AchievementLevel', FieldName: 'AchievementLevelId', AssessmentId: 56 };
                $scope.comparisonGroups[2].options.selectedAssessmentField = scienceField;
            }

            var isReadyToLoad = function () {
                if ($scope.filterOptions.selectedSchool == null || $scope.filterOptions.selectedSchoolYear == null) {
                    return false;
                } else {
                    return true;
                }
            }

            $scope.generateGraph = function () {
                if (!isReadyToLoad()) {
                    return;
                }

                $scope.settings.firstLoadStarted = true;

                loadInitialGroups();

                var promiseCollection = [];
                // loop over each group and get data... build graph once they've all returned
                for (var i = 0; i < $scope.comparisonGroups.length; i++) {
                    promiseCollection.push($scope.comparisonGroups[i].loadGroupData(true));
                }

                var studentResultsCollection = [];
                //var groupNameCollection = [];
                $q.all(promiseCollection).then(function (response) {
                    for (var j = 0; j < response.length; j++) {
                        $scope.comparisonGroups[j].graphGenerated = true;
                        studentResultsCollection.push(response[j].data);
                        //groupNameCollection.push()
                    }


                    updateDataFromServiceChange(studentResultsCollection);
                });

                $scope.settings.graphGenerated = true;
                $scope.settings.firstLoadStarted = false;
            };

            $scope.changeSummaryMode = function () {
                changeToSummaryMode($scope.settings.summaryCategory, $scope.settings.summaryScoreGrouping);
            };

            $scope.$watch('settings.stacking', function (newValue, oldValue) {
                if (!angular.equals(newValue, oldValue)) {
                    if (newValue == 'normal') {
                        $scope.settings.stackingDescription = 'Number of Students';
                    } else {
                        $scope.settings.stackingDescription = 'Percentage of Students';
                    }

                    $scope.generateGraph();
                    $scope.settings.firstLoadStarted = false;
                }
            });

            // TODO: WATCH SCHOOL
            $scope.$watch('filterOptions.selectedSchool', function (newValue, oldValue) {
                if (isReadyToLoad() && $scope.settings.firstLoadStarted == false) {
                    $scope.generateGraph();
                    $scope.settings.firstLoadStarted = false;
                }
            });

            $scope.getChartHeader = function () {
                if (isReadyToLoad()) {
                    return 'Preliminary MCA data for ' + $scope.filterOptions.selectedSchool.text + ' for the ' + $scope.filterOptions.selectedSchoolYear.text + ' school year for ' + ($scope.filterOptions.selectedGrade == null ? ' All grades' : $scope.filterOptions.selectedGrade.text + ' grade');
                } else {
                    return 'Please select a year and school';
                }
            }

            $scope.$watch('filterOptions.selectedSchoolYear', function (newValue, oldValue) {
                if (isReadyToLoad() && $scope.settings.firstLoadStarted == false) {
                    $scope.generateGraph();
                    $scope.settings.firstLoadStarted = false;
                }
            });

            $scope.$watch('filterOptions.selectedGrade', function (newValue, oldValue) {
                if (isReadyToLoad() && $scope.settings.firstLoadStarted == false) {
                    $scope.generateGraph();
                    $scope.settings.firstLoadStarted = false;
                }
            });

            var changeToSummaryMode = function (category, scoreGrouping, skipSpinner) {
                if (!skipSpinner)
                    spinnerService.show('tableSpinner');
                $scope.settings.summaryCategory = category;
                $scope.settings.summaryScoreGrouping = scoreGrouping;

                // find proper factory based on category (groupName)
                for (var i = 0; i < $scope.comparisonGroups.length; i++) {
                    if ($scope.comparisonGroups[i].name == category) {
                        $scope.groupsFactory = $scope.comparisonGroups[i];
                        $scope.groupsFactory.loadSummaryData(scoreGrouping, category, $scope.groupsFactory.options.selectedTestDueDate.text, ($scope.settings.summaryView !== 'Current Year Only (Sortable)')).then(function (response) {
                            $scope.settings.summaryMode = true;
                            $scope.settings.graphGenerated = true;

                            // set attribute headers (hidden ones here too)
                            $scope.attributeHeaders['Att1'] = $scope.groupsFactory.Att1Header;
                            $scope.attributeHeaders['Att2'] = $scope.groupsFactory.Att2Header;
                            $scope.attributeHeaders['Att3'] = $scope.groupsFactory.Att3Header;
                            $scope.attributeHeaders['Att4'] = $scope.groupsFactory.Att4Header;
                            $scope.attributeHeaders['Att5'] = $scope.groupsFactory.Att5Header;
                            $scope.attributeHeaders['Att6'] = $scope.groupsFactory.Att6Header;
                            $scope.attributeHeaders['Att7'] = $scope.groupsFactory.Att7Header;
                            $scope.attributeHeaders['Att8'] = $scope.groupsFactory.Att8Header;
                            $scope.attributeHeaders['Att9'] = $scope.groupsFactory.Att9Header;

                            // initialize sorting
                            for (var i = 0; i < $scope.groupsFactory.TestDueDates.length; i++) {
                                $scope.headerClassArray[i] = [];
                            }
                            $scope.sortMgr.initialize($scope.manualSortHeaders, $scope.sortArray, $scope.headerClassArray, $scope.groupsFactory.Fields);
                        }).finally(function () {
                            if (!skipSpinner)
                                spinnerService.hide('tableSpinner');
                        });
                        break;
                    }
                }
            }

            $scope.$on('NSStudentAttributesUpdated', function (event, data) {
                if ($scope.settings.summaryMode == true) {
                    $scope.changeSummaryMode();
                }
            });

            $scope.changeToChartMode = function () {
                $scope.settings.summaryView = 'Current Year Only (Sortable)';
                $scope.settings.summaryMode = false;
                $scope.settings.summaryCategory = '';
            }

            // initial auto-load
            $scope.generateGraph();

            function updateDataFromServiceChange(studentResultsCollection) {
                $scope.settings.anyResults = false;
                // might need this
                //angular.copy(data.results, $scope.studentResults);
                //$scope.studentResults = nsStackedBarGraphGroupsOptionsService.groupResults.data;

                //return;
                var seriesArray = [];
                var categoriesArray = [];

                // set up series
                for (var i = 0; i < studentResultsCollection.length; i++) {
                    var currentResult = studentResultsCollection[i];

                    var foundCategory = $filter('filter')(categoriesArray, currentResult.GroupName, true);
                    // see if category already exists, if not, add it
                    if (!foundCategory.length) {
                        categoriesArray.push(currentResult.GroupName);//[categoriesArray.length] = { name: currentResult.DueDate, categories: [currentResult.DueDate] };
                    }

                    for (var j = 0; j < currentResult.Results.length; j++) {
                        $scope.settings.anyResults = true;
                        var currentScore = currentResult.Results[j];
                        //labels: {rotation: -90}
                        // create a data array for each scoregrouping
                        // FIX THIS... need to be able to create an array of arrays with the index being the scoregrouping

                        var groupingName = "";
                        var groupingColor = "";

                        if (currentScore.ScoreGrouping == 1) {
                            groupingName = "Exceeds Expectations";
                            groupingColor = "#4697ce";
                        }
                        if (currentScore.ScoreGrouping == 2) {
                            groupingName = "Meets Expectations";
                            groupingColor = "#90ED7D";
                        }
                        if (currentScore.ScoreGrouping == 3) {
                            groupingName = "Approaches Expectations";
                            groupingColor = "#E4D354";
                        }
                        if (currentScore.ScoreGrouping == 4) {
                            groupingName = "Does Not Meet Expectations";
                            groupingColor = "#BF453D";
                        }

                        if (seriesArray[currentScore.ScoreGrouping] == null) {
                            seriesArray[currentScore.ScoreGrouping] = { name: groupingName, color: groupingColor, data: [currentScore.NumberOfResults], id: currentScore.ScoreGrouping }
                        }
                        else {
                            seriesArray[currentScore.ScoreGrouping].data.push(currentScore.NumberOfResults);
                        }
                    }
                }

                highchartsNgConfig = {
                    //This is not a highcharts object. It just looks a little like one!
                    options: {
                        chart: {
                            type: 'column',
                        },
                        tooltip: {
                            pointFormat: '<span style="color:{series.color}">\u25CF</span>  <span style="color:#666666">{series.name}</span>: <b>{point.y} Students</b> ({point.percentage:.0f}%)<br/>',
                            style: {
                                padding: 10,
                                fontWeight: 'bold'
                            }, useHTML: true
                        },
                        plotOptions: {
                            series: {
                                cursor: 'pointer',
                                point: {
                                    events: {
                                        click: function (event) {
                                            var category = this.category;
                                            var scoreGrouping = this.series.userOptions.id;

                                            changeToSummaryMode(category, scoreGrouping);
                                        }
                                    }
                                }
                            },
                            column: {
                                stacking: $scope.settings.stacking,
                                dataLabels: {
                                    enabled: true,
                                    color: (Highcharts.theme && Highcharts.theme.dataLabelsColor) || 'white',
                                    style: {
                                        textShadow: '0 0 3px black'
                                    },
                                    formatter: function () {
                                        if (this.y > 0 && $scope.settings.stacking === 'normal')
                                            return this.y;
                                        if (this.y > 0 && $scope.settings.stacking === 'percent')
                                            return this.percentage.toFixed(0) + '%';
                                    }
                                }
                            }
                        }
                    },

                    yAxis: {
                        allowDecimals: false,
                        min: 0,
                        title: {
                            text: $scope.settings.stackingDescription
                        },
                        stackLabels: {
                            enabled: true,
                            style: {
                                fontWeight: 'bold',
                                color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                            }
                        }
                    },
                    series: seriesArray,
                    title: {
                        text: $scope.getChartHeader()
                    },
                    loading: false,
                    xAxis: {
                        categories: categoriesArray

                    },
                    useHighStocks: false,
                    func: function (chart) {
                    }

                };

                $scope.chartConfig = highchartsNgConfig;
            }
        }
        	])
	.controller('SchoolDashboardMCAController', [
        '$bootbox', 'nsPinesService', 'nsStackedBarGraphOptionsFactory', '$scope', 'nsFilterOptionsService', '$q', '$filter', 'NSSummarySortManager', 'spinnerService', function ($bootbox, nsPinesService, nsStackedBarGraphOptionsFactory, $scope, nsFilterOptionsService, $q, $filter, NSSummarySortManager, spinnerService) {
            $scope.settings = {};
            $scope.settings.summaryMode = false;
            $scope.settings.summaryView = 'Current Year Only (Sortable)';
            $scope.settings.summaryCategory = '';
            $scope.settings.summaryScoreGrouping = 1;
            $scope.settings.stacking = 'normal';
            $scope.settings.stackingDescription = 'Number of Students';
            $scope.settings.graphGenerated = false;
            $scope.settings.firstLoadStarted = false;

            $scope.sortArray = [];
            $scope.attributeHeaders = {};
            $scope.manualSortHeaders = {};
            $scope.manualSortHeaders['Student'] = "fa";
            $scope.manualSortHeaders['StudentIdentifier'] = "fa";
            $scope.manualSortHeaders['SpecialED'] = "fa";
            $scope.manualSortHeaders['Services'] = "fa";
            $scope.manualSortHeaders['School'] = "fa";
            $scope.manualSortHeaders['Grade'] = "fa";
            $scope.manualSortHeaders['Teacher'] = "fa";
            $scope.manualSortHeaders['HomeLanguage'] = "fa";
            $scope.headerClassArray = [];
            $scope.sortMgr = new NSSummarySortManager();

            $scope.sort = function (fieldName) {
                $scope.sortMgr.sort(fieldName);
            };

            var highchartsNgConfig = {};
            $scope.groupsFactory = {};

            $scope.comparisonGroups = [];
            $scope.comparisonGroups.push(new nsStackedBarGraphOptionsFactory("MCA Reading", false, true));
            $scope.comparisonGroups.push(new nsStackedBarGraphOptionsFactory("MCA Math", false, true));
            $scope.comparisonGroups.push(new nsStackedBarGraphOptionsFactory("MCA Science", false, true));

            $scope.filterOptions = nsFilterOptionsService.options;

            $scope.getSummaryHeader = function (displayDate) {
                if ($scope.groupsFactory.TestDueDates.length == 1) {
                    return $scope.groupsFactory.options.selectedSchoolYear.text;
                } else {
                    return displayDate;
                }
            }
            
            // add initial groups (might be prelim or historical, based on checkbox)
            var loadInitialGroups = function () {
                $scope.comparisonGroups[0].options.selectedSchools = new Array($scope.filterOptions.selectedSchool);
                $scope.comparisonGroups[0].options.selectedGrades = $scope.filterOptions.selectedGrade == null ? [] : new Array($scope.filterOptions.selectedGrade);
                $scope.comparisonGroups[0].options.selectedSchoolYear = $scope.filterOptions.selectedSchoolYear;
                $scope.comparisonGroups[0].options.selectedTestDueDate = { id: 364, text: "test" };
                // todo - get from db
                var readingField = { DisplayLabel: 'Achievement Level', DatabaseColumn: 'AchievementLevelId', FieldType: 'DropdownFromDB', LookupFieldName: 'AchievementLevel', FieldName: 'AchievementLevelId', AssessmentId: 59 };
                $scope.comparisonGroups[0].options.selectedAssessmentField = readingField;

                $scope.comparisonGroups[1].options.selectedSchools = new Array($scope.filterOptions.selectedSchool);
                $scope.comparisonGroups[1].options.selectedGrades = $scope.filterOptions.selectedGrade == null ? [] : new Array($scope.filterOptions.selectedGrade);
                $scope.comparisonGroups[1].options.selectedSchoolYear = $scope.filterOptions.selectedSchoolYear;
                $scope.comparisonGroups[1].options.selectedTestDueDate = { id: 364, text: "test" };
                // todo - get from db
                var mathField = { DisplayLabel: 'Achievement Level', DatabaseColumn: 'AchievementLevelId', FieldType: 'DropdownFromDB', LookupFieldName: 'AchievementLevel', FieldName: 'AchievementLevelId', AssessmentId: 60 };
                $scope.comparisonGroups[1].options.selectedAssessmentField = mathField;

                $scope.comparisonGroups[2].options.selectedSchools = new Array($scope.filterOptions.selectedSchool);
                $scope.comparisonGroups[2].options.selectedGrades = $scope.filterOptions.selectedGrade == null ? [] : new Array($scope.filterOptions.selectedGrade);
                $scope.comparisonGroups[2].options.selectedSchoolYear = $scope.filterOptions.selectedSchoolYear;
                $scope.comparisonGroups[2].options.selectedTestDueDate = { id: 364, text: "test" };
                // todo - get from db
                var scienceField = { DisplayLabel: 'Achievement Level', DatabaseColumn: 'AchievementLevelId', FieldType: 'DropdownFromDB', LookupFieldName: 'AchievementLevel', FieldName: 'AchievementLevelId', AssessmentId: 61 };
                $scope.comparisonGroups[2].options.selectedAssessmentField = scienceField;
            }

            var isReadyToLoad = function () {
                if ($scope.filterOptions.selectedSchool == null || $scope.filterOptions.selectedSchoolYear == null) {
                    return false;
                } else {
                    return true;
                }
            }

            $scope.generateGraph = function () {
                if (!isReadyToLoad()) {
                    return;
                }

                $scope.settings.firstLoadStarted = true;

                loadInitialGroups();

                var promiseCollection = [];
                // loop over each group and get data... build graph once they've all returned
                for (var i = 0; i < $scope.comparisonGroups.length; i++) {
                    promiseCollection.push($scope.comparisonGroups[i].loadGroupData(true));
                }

                var studentResultsCollection = [];
                //var groupNameCollection = [];
                $q.all(promiseCollection).then(function (response) {
                    for (var j = 0; j < response.length; j++) {
                        $scope.comparisonGroups[j].graphGenerated = true;
                        studentResultsCollection.push(response[j].data);
                        //groupNameCollection.push()
                    }

                   
                    updateDataFromServiceChange(studentResultsCollection);
                });

                $scope.settings.graphGenerated = true;
                $scope.settings.firstLoadStarted = false;
            };

            $scope.changeSummaryMode = function () {
                changeToSummaryMode($scope.settings.summaryCategory, $scope.settings.summaryScoreGrouping);
            };

            $scope.$watch('settings.stacking', function (newValue, oldValue) {
                if (!angular.equals(newValue, oldValue)) {
                    if (newValue == 'normal') {
                        $scope.settings.stackingDescription = 'Number of Students';
                    } else {
                        $scope.settings.stackingDescription = 'Percentage of Students';
                    }

                    $scope.generateGraph();
                    $scope.settings.firstLoadStarted = false;
                }
            });

            // TODO: WATCH SCHOOL
            $scope.$watch('filterOptions.selectedSchool', function (newValue, oldValue) {
                if (isReadyToLoad() && $scope.settings.firstLoadStarted == false) {
                    $scope.generateGraph();
                    $scope.settings.firstLoadStarted = false;
                }
            });

            $scope.getChartHeader = function () {
                if (isReadyToLoad()) {
                    return 'MCA data for ' + $scope.filterOptions.selectedSchool.text + ' for the ' + $scope.filterOptions.selectedSchoolYear.text + ' school year for ' + ($scope.filterOptions.selectedGrade == null ? ' All grades' : $scope.filterOptions.selectedGrade.text + ' grade');
                } else {
                    return 'Please select a year and school';
                }
            }

            $scope.$watch('filterOptions.selectedSchoolYear', function (newValue, oldValue) {
                if (isReadyToLoad() && $scope.settings.firstLoadStarted == false) {
                    $scope.generateGraph();
                    $scope.settings.firstLoadStarted = false;
                }
            });

            $scope.$watch('filterOptions.selectedGrade', function (newValue, oldValue) {
                if (isReadyToLoad() && $scope.settings.firstLoadStarted == false) {
                    $scope.generateGraph();
                    $scope.settings.firstLoadStarted = false;
                }
            });

            var changeToSummaryMode = function (category, scoreGrouping, skipSpinner) {
                if (!skipSpinner)
                    spinnerService.show('tableSpinner');
                $scope.settings.summaryCategory = category;
                $scope.settings.summaryScoreGrouping = scoreGrouping;

                // find proper factory based on category (groupName)
                for (var i = 0; i < $scope.comparisonGroups.length; i++) {
                    if ($scope.comparisonGroups[i].name == category) {
                        $scope.groupsFactory = $scope.comparisonGroups[i];
                        $scope.groupsFactory.loadSummaryData(scoreGrouping, category, $scope.groupsFactory.options.selectedTestDueDate.text, ($scope.settings.summaryView !== 'Current Year Only (Sortable)')).then(function (response) {
                            $scope.settings.summaryMode = true;

                            $scope.settings.graphGenerated = true;

                            // set attribute headers (hidden ones here too)
                            $scope.attributeHeaders['Att1'] = $scope.groupsFactory.Att1Header;
                            $scope.attributeHeaders['Att2'] = $scope.groupsFactory.Att2Header;
                            $scope.attributeHeaders['Att3'] = $scope.groupsFactory.Att3Header;
                            $scope.attributeHeaders['Att4'] = $scope.groupsFactory.Att4Header;
                            $scope.attributeHeaders['Att5'] = $scope.groupsFactory.Att5Header;
                            $scope.attributeHeaders['Att6'] = $scope.groupsFactory.Att6Header;
                            $scope.attributeHeaders['Att7'] = $scope.groupsFactory.Att7Header;
                            $scope.attributeHeaders['Att8'] = $scope.groupsFactory.Att8Header;
                            $scope.attributeHeaders['Att9'] = $scope.groupsFactory.Att9Header;

                            // initialize sorting
                            for (var i = 0; i < $scope.groupsFactory.TestDueDates.length; i++) {
                                $scope.headerClassArray[i] = [];
                            }
                            $scope.sortMgr.initialize($scope.manualSortHeaders, $scope.sortArray, $scope.headerClassArray, $scope.groupsFactory.Fields);
                        }).finally(function () {
                            if (!skipSpinner)
                                spinnerService.hide('tableSpinner');
                        });
                        break;
                    }
                }
            }

            $scope.$on('NSStudentAttributesUpdated', function (event, data) {
                if ($scope.settings.summaryMode == true) {
                    $scope.changeSummaryMode();
                }
            });

            $scope.changeToChartMode = function () {
                $scope.settings.summaryView = 'Current Year Only (Sortable)';
                $scope.settings.summaryMode = false;
                $scope.settings.summaryCategory = '';
            }

            // initial auto-load
            $scope.generateGraph();

            function updateDataFromServiceChange(studentResultsCollection) {
                $scope.settings.anyResults = false;
                // might need this
                //angular.copy(data.results, $scope.studentResults);
                //$scope.studentResults = nsStackedBarGraphGroupsOptionsService.groupResults.data;

                //return;
                var seriesArray = [];
                var categoriesArray = [];

                // set up series
                for (var i = 0; i < studentResultsCollection.length; i++) {
                    var currentResult = studentResultsCollection[i];

                    var foundCategory = $filter('filter')(categoriesArray, currentResult.GroupName, true);
                    // see if category already exists, if not, add it
                    if (!foundCategory.length) {
                        categoriesArray.push(currentResult.GroupName);//[categoriesArray.length] = { name: currentResult.DueDate, categories: [currentResult.DueDate] };
                    }

                    for (var j = 0; j < currentResult.Results.length; j++) {
                        $scope.settings.anyResults = true;
                        var currentScore = currentResult.Results[j];
                        //labels: {rotation: -90}
                        // create a data array for each scoregrouping
                        // FIX THIS... need to be able to create an array of arrays with the index being the scoregrouping

                        var groupingName = "";
                        var groupingColor = "";

                        if (currentScore.ScoreGrouping == 1) {
                            groupingName = "Exceeds Expectations";
                            groupingColor = "#4697ce";
                        }
                        if (currentScore.ScoreGrouping == 2) {
                            groupingName = "Meets Expectations";
                            groupingColor = "#90ED7D";
                        }
                        if (currentScore.ScoreGrouping == 3) {
                            groupingName = "Approaches Expectations";
                            groupingColor = "#E4D354";
                        }
                        if (currentScore.ScoreGrouping == 4) {
                            groupingName = "Does Not Meet Expectations";
                            groupingColor = "#BF453D";
                        }

                        if (seriesArray[currentScore.ScoreGrouping] == null) {
                            seriesArray[currentScore.ScoreGrouping] = { name: groupingName, color: groupingColor, data: [currentScore.NumberOfResults], id: currentScore.ScoreGrouping }
                        }
                        else {
                            seriesArray[currentScore.ScoreGrouping].data.push(currentScore.NumberOfResults);
                        }
                    }
                }

                highchartsNgConfig = {
                    //This is not a highcharts object. It just looks a little like one!
                    options: {
                        chart: {
                            type: 'column',
                        },
                        tooltip: {
                            pointFormat: '<span style="color:{series.color}">\u25CF</span>  <span style="color:#666666">{series.name}</span>: <b>{point.y} Students</b> ({point.percentage:.0f}%)<br/>',
                            style: {
                                padding: 10,
                                fontWeight: 'bold'
                            }, useHTML: true
                        },
                        plotOptions: {
                            series: {
                                cursor: 'pointer',
                                point: {
                                    events: {
                                        click: function (event) {
                                            var category = this.category;
                                            var scoreGrouping = this.series.userOptions.id;

                                            changeToSummaryMode(category, scoreGrouping);
                                        }
                                    }
                                }
                            },
                            column: {
                                stacking: $scope.settings.stacking,
                                dataLabels: {
                                    enabled: true,
                                    color: (Highcharts.theme && Highcharts.theme.dataLabelsColor) || 'white',
                                    style: {
                                        textShadow: '0 0 3px black'
                                    },
                                    formatter: function () {
                                        if (this.y > 0 && $scope.settings.stacking === 'normal')
                                            return this.y;
                                        if (this.y > 0 && $scope.settings.stacking === 'percent')
                                            return this.percentage.toFixed(0) + '%';
                                    }
                                }
                            }
                        }
                    },

                    yAxis: {
                        allowDecimals: false,
                        min: 0,
                        title: {
                            text: $scope.settings.stackingDescription
                        },
                        stackLabels: {
                            enabled: true,
                            style: {
                                fontWeight: 'bold',
                                color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                            }
                        }
                    },
                    series: seriesArray,
                    title: {
                        text: $scope.getChartHeader()
                    },
                    loading: false,
                    xAxis: {
                        categories: categoriesArray

                    },
                    useHighStocks: false,
                    func: function (chart) {
                    }

                };

                $scope.chartConfig = highchartsNgConfig;
            }
        }
	])

})();