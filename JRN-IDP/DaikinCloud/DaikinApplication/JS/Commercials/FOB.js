const app = angular.module('app', []);

app.directive('button', function () {
    return {
        restrict: 'E',
        link: function (scope, elem, attrs) {
            if (attrs.ngClick || attrs.href === '' || attrs.href === '#') {
                elem.on('click', function (e) {
                    e.preventDefault();
                });
            }
        }
    };
});
app.directive('loading', ['$http', function ($http) {
    return {
        restrict: 'A',
        link: function (scope, elm, attrs) {
            scope.isLoading = function () {
                return $http.pendingRequests.length > 0;
            };

            scope.$watch(scope.isLoading, function (v) {
                if (v) {
                    elm.show();
                } else {
                    elm.hide();
                }
            });
        }
    };

}]);

app.service("svc", function ($http) {
    this.svc_LoadDDL = function () {
        const response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/Commercials.asmx/LoadDDL",
            data: {},
            dataType: "json"
        });
        return response;
    }
});

app.controller('ctrl', function ($scope, svc) {
    $scope.Generate = function () {
        location.href = '/_layouts/15/Daikin.Application/Commercials/OutstandingAP.aspx?tp=' + $scope.tradingPartner.Name +
                        '&doc=' + $scope.dueOn.Code + '&don=' + $scope.dueOn.Name + '&tpc=' + $scope.tradingPartner.Code +
                        '&curr=';
    };


    $scope.LoadDDL = function () {
        const proc = svc.svc_LoadDDL();
        proc.then(function (response) {
            const data = JSON.parse(response.data.d);
            if (data.ProcessSuccess) {
                $scope.ddlTradingPartner = data.listTradingPartner;
                const index = $scope.ddlTradingPartner.findIndex(x => x.Code === '');
                $scope.tradingPartner = data.listTradingPartner[index];
                $scope.ddlDueOn = data.listDueOn;
                $scope.dueOn = $scope.ddlDueOn[0];

            } else {
                alert(data.InfoMessage);
            }

        }, function (data, status) {
            alert(data.Message);
        });
    }

    $scope.LoadDDL();
});
