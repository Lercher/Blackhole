"use strict";

if (!window.location.pathname.match(/\/$/)) {
    window.location = window.location + "/";
}

angular.module("bh", ['ui.bootstrap']);


angular.module("bh").controller("bhMonitor", function ($scope, $http, $timeout) {
    $scope.D = { execute: execute, query: 'ask {$s $p $o}', ws: null };
    $scope.R = { errormessage: null};
    $scope.log = [];
    $scope.store = window.location.pathname.split('/').slice(-2)[0];

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

    execute();

    // end

    function execute() {
        if ($scope.D.ws) {
            var s = $scope.D.query;
            log(">> " + s);
            $scope.D.ws.send(s);
        } else {
            var wsImpl = window.WebSocket || window.MozWebSocket;
            $scope.D.ws = new wsImpl('ws://localhost:8091/' + $scope.store);
            $scope.D.ws.onopen = onopen;
            $scope.D.ws.onclose = onclose;
            $scope.D.ws.onmessage = onmsg;
        }
    }
    function log(s) { $scope.log.push(s);  }
    function onopen() { log("[connection open]"); $scope.$digest(); }
    function onclose() { log("[connection close]"); $scope.D.ws = null; $scope.$digest(); }
    function onmsg(e) { log(e.data); $scope.$digest(); }
});

angular.module("bh").controller("bhUpdate", function ($scope, $http, $timeout) {
    $scope.D = { execute: execute, update: 'delete data { <http://www.example.org> a "URI" };\ninsert data { <http://www.example.org> a "URI" }' };
    $scope.store = window.location.pathname.split('/').slice(-2)[0];

    $scope.$watch("D.update", function () {
        $http.post("/blackhole/UpdateSyntax/", { query: $scope.D.update })
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
        $http.post("", { update: $scope.D.update })
            .success(function (data) {
                $scope.R = data;
            })
            .error(function (data, status, headers, config) {
                $scope.R = { errormessage: "Error " + status + ": " + data.statusMessage };
            })
        ;
    }

});

angular.module("bh").controller("bhQuery", function ($scope, $http, $timeout) {
    $scope.D = { execute: execute, query: 'select * where {$s $p $o}'};
    $scope.store = window.location.pathname.split('/').slice(-2)[0];

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
    $scope.D = {ref: 0, query: ''};
    $scope.R = {ref: -1, errormessage: null, normalized: null}

    $scope.typ = window.location.pathname.split('/').slice(-2)[0].replace("Syntax", "");

    $scope.$watch("D.query", function () {
        $scope.D.ref++;
        $http.post("", $scope.D)
            .success(function (data) {
                if ($scope.R.ref > data.ref)
                    return;
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