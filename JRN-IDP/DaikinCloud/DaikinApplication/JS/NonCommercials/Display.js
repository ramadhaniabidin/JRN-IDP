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
 
});

app.controller('ctrl', function ($scope, svc) {
    $scope.ConvertJSONDate = function (x) {
        if (x == null)
            return x;

        const re = /\/Date\(([0-9]*)\)\//;
        const m = x.match(re);
        if (m)
            return new Date(parseInt(m[1]));
        else
            return null;
    }

    $scope.Display = {};
    $scope.ddlDisplay = [];
    $scope.ddlDisplay = [{ Code: "", Name: "Please Select", Extra: '' }];

    $scope.GetModuleOptions = () => {
        const urlParams = new URLSearchParams(location.search);
        const Module = urlParams.get('module');
        if (Module == 'M016') {
            $scope.ddlDisplay = [
                { Code: "", Name: "Please Select", Extra: '' },
                { Code: "", Name: "Marketing", Extra: 'Purchase Request MKT' },
                { Code: "", Name: "Non Marketing", Extra: 'Purchase Request GA' },
            ]
            $scope.Display = $scope.ddlDisplay[0];
        }
    }
    $scope.GetModuleOptions();

    $scope.NewItem = function () {
        location.href = '/Lists/' + $scope.Display.Extra + '/NewForm.aspx'
    };
});