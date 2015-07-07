"use strict";

angular.module("bh", ['ui.bootstrap']);

angular.module("bh").controller("bhc", function ($scope, $http, $timeout) {
    $scope.D = {};
    $scope.D.ref = 1;
    $scope.D.query = '';

    $scope.$watch("D.query", function () {
        $scope.D.ref++;
        $http.post("", $scope.D)
            .success(function (data) {
                $scope.R = data;
            })
            .error(function (data, status, headers, config) {
                $scope.R = { errormessage: "Error " + status + ": " + data.statusMessage };
            })
        ;
    });

    $timeout(function () {
        document.querySelector('#start').focus();
    }, 100);
});
