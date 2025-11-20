(function () {
    'use strict';

    angular
        .module('stateTestDataModule', [])
        .controller('MNStateTestDataImportController', MNStateTestDataImportController);

    /* Movies List Controller  */
    MNStateTestDataImportController.$inject = ['$scope', '$q', '$http', 'pinesNotifications', '$location', '$filter'];


    function MNStateTestDataImportController($scope, $q, $http, pinesNotifications, $location, $filter) {
        $scope.studentResults = {};
        //$scope.benchmarkDates = {};
        //$scope.chartConfig = {};

        var highchartsNgConfig = {};
//$scope.filters = authorizationFactory.filters;
        
        function loadBarGraph()
        {

            $http.post('/api/StackedBarGraph/GetStudentStackedBarGraph', { schoolStartYear: 2012, sectionID: 2384, schoolID: 73, gradeID: null, groupingType: 1, groupingValue: null, assessmentID: 1, fieldToRetrieve: 'FPValueID', isDecimalField: false }).success(function (data) {

                // might need this
                //angular.copy(data.results, $scope.studentResults);
                $scope.studentResults = data.Results;

                //return;
                var seriesArray = [];
                var categoriesArray = [];

                // set up series
                for (var i = 0; i < $scope.studentResults.length; i++)
                {
                    var currentResult = $scope.studentResults[i];

                    var foundCategory = $filter('filter')(categoriesArray, { name: currentResult.GroupingValue }, true);
                    // see if category already exists, if not, add it
                    if(foundCategory.length)
                    {
                        var found = $filter('filter')(foundCategory[0].categories, currentResult.DisplayDate, true);
                        // if you don't find this TestDueDateID already in the subcategories
                        if (!found.length)
                        {
                            foundCategory[0].categories.push(currentResult.DisplayDate);
                        }
                    }
                    else
                    {
                        categoriesArray[categoriesArray.length] = { name: currentResult.GroupingValue, categories: [currentResult.DisplayDate] };
                    }
                    //labels: {rotation: -90}
                    // create a data array for each scoregrouping
                    // FIX THIS... need to be able to create an array of arrays with the index being the scoregrouping

                    var groupingName = "";
                    var groupingColor = "";

                    if (currentResult.ScoreGrouping == 1) {
                        groupingName = "Exceeds Expectations";
                        groupingColor = "#0000FF";
                    }
                    if (currentResult.ScoreGrouping == 2) {
                        groupingName = "Meets Expectations";
                        groupingColor = "#00FF00";
                    }
                    if (currentResult.ScoreGrouping == 3) {
                        groupingName = "Approaches Expectations";
                        groupingColor = "#CCFF00";
                    }
                    if (currentResult.ScoreGrouping == 4) {
                        groupingName = "Does Not Meet Expectations";
                        groupingColor = "#FF0000";
                    }

                    if(seriesArray[currentResult.ScoreGrouping - 1] == null)
                    {
                        seriesArray[currentResult.ScoreGrouping - 1] = { name: groupingName, color: groupingColor, data: [currentResult.NumberOfResults] }
                    }
                    else
                    {                    
                        seriesArray[currentResult.ScoreGrouping - 1].data.push(currentResult.NumberOfResults);
                    }
                }




                highchartsNgConfig = {
                    //This is not a highcharts object. It just looks a little like one!
                    options: {
                        //This is the Main Highcharts chart config. Any Highchart options are valid here.
                        //will be ovverriden by values specified below.
                        chart: {
                            type: 'column'
                        },
                        tooltip: {
                            style: {
                                padding: 10,
                                fontWeight: 'bold'
                            }
                        },
                        plotOptions: {
                            column: {
                                stacking: 'normal'
                            }
                        }
                    },

                    yAxis: {
                        allowDecimals: false,
                        min: 0,
                        title: {
                            text: 'Number of Students'
                        }
                    },

                    //The below properties are watched separately for changes.

                    //Series object (optional) - a list of series using normal highcharts series options.
                    series: seriesArray,
                    //Title configuration (optional)
                    title: {
                        text: 'Stacked and Grouped'
                    },
                    //Boolean to control showng loading status on chart (optional)
                    //Could be a string if you want to show specific loading text.
                    loading: false,
                    //Configuration for the xAxis (optional). Currently only one x axis can be dynamically controlled.
                    //properties currentMin and currentMax provied 2-way binding to the chart's maximimum and minimum
                    xAxis: {
                        categories: categoriesArray
                        
                    },
                    //Whether to use HighStocks instead of HighCharts (optional). Defaults to false.
                    useHighStocks: false,
                    //size (optional) if left out the chart will default to size of the div or something sensible.
                    //                size: {
                    //                  width: 1000,
                    //                height: 600
                    //           },
                    //function (optional)
                    func: function (chart) {
                        //setup some logic for the chart
                    }

                };

                $scope.chartConfig = highchartsNgConfig;
            }).error(function (error) {
                alert('there was an error loading the data');
            });
        }

        // TODO:  This may need to be a watch
        loadBarGraph();
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