"use strict";

if (!window.location.pathname.match(/\/$/)) {
    window.location = window.location + "/";
}

angular.module("bh", ['ui.bootstrap']);


angular.module("bh").controller("bhQuery", function ($scope, $http, $timeout) {
    $scope.D = { execute: execute, query: 'select * where {$s $p $o}'};

    $scope.$watch("D.query", function () {
        $http.post("/blackhole/QuerySyntax/", { query: $scope.D.query })
            .success(function (data) {
                $scope.R = data;
            })
            .error(function (data, status, headers, config) {
                $scope.R = { errormessage: "Error " + status + ": " + data.statusMessage };
            })
        ;
    });

    $timeout(function () {
        var s = document.querySelector('#start');
        s && s.focus();
    }, 100);

    function execute() {
        $http.post("", { query: $scope.D.query })
            .success(function (data) {
                $scope.R = data;
            })
            .error(function (data, status, headers, config) {
                $scope.R = { errormessage: "Error " + status + ": " + data.statusMessage };
            })
        ;
    }

});


angular.module("bh").controller("bhSyntax", function ($scope, $http, $timeout) {
    $scope.D = {};
    $scope.D.ref = 0;
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
        var s = document.querySelector('#start');
        s && s.focus();
    }, 100);
});


angular.module("bh").controller("bhHome", function ($scope, $http, $timeout) {
    $scope.D = { stores: [], newstore: "", addstore: addstore, deletestore: deletestore, errormessage: "" };
    refresh();

    function refresh () {
        $http.get("stores/").success(function (data) {
            $scope.D.stores = data;
        });
    }

    function addstore() {
        $http.put("stores/", { store: $scope.D.newstore })
            .success(function (data) {
                $scope.D.errormessage = data;
                $scope.D.newstore = "";
                refresh();
            })
            .error(function (data, status, headers, config) {
                $scope.D.errormessage = "Error " + status + ": " + data.statusMessage;
            })
        ;
    };

    function deletestore(st) {
        // see https://docs.angularjs.org/api/ng/service/$http delete has no body
        $http.delete("stores/", { params: { store: st } })
            .success(function (data) {
                $scope.D.errormessage = data;
                refresh();
            })
            .error(function (data, status, headers, config) {
                $scope.D.errormessage = "Error " + status + ": " + data.statusMessage;
            })
        ;
    };
});


/**
 * A generic confirmation for risky actions.
 * Usage: Add attributes: ng-really-message="Are you sure?" ng-really-click="takeAction()" function
 * see: https://gist.github.com/asafge/7430497
 */
angular.module('bh').directive('ngReallyClick', [function () {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {
            element.bind('click', function () {
                var message = attrs.ngReallyMessage;
                if (message && confirm(message)) {
                    scope.$apply(attrs.ngReallyClick);
                }
            });
        }
    }
}]);