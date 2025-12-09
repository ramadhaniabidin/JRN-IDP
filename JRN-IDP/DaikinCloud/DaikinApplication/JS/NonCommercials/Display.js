var app = angular.module('app', []);
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
    //this.svc_GetOptions = function (Table, Code, Name, FilterBy, FilterValue, Extra) {
    //    var param = {
    //        Table: Table,
    //        Code: Code,
    //        Name: Name,
    //        FilterBy: FilterBy,
    //        FilterValue: FilterValue,
    //        Extra: Extra
    //    };
    //    var response = $http({
    //        method: "post",
    //        url: "/_layouts/15/WebServices/Master.asmx/GetOptions",
    //        data: JSON.stringify(param),
    //        dataType: "json"
    //    });
    //    return response;
    //}
});

app.controller('ctrl', function ($scope, svc) {
    $scope.ConvertJSONDate = function (x) {
        if (x == null)
            return x;

        var re = /\/Date\(([0-9]*)\)\//;
        var m = x.match(re);
        if (m)
            return new Date(parseInt(m[1]));
        else
            return null;
    }

    $scope.Display = {};
    $scope.ddlDisplay = [];
    $scope.ddlDisplay = [{ Code: "", Name: "Please Select", Extra: '' }];
    //$scope.GetModuleOptions = function () {
    //    var urlParams = new URLSearchParams(location.search);
    //    var Module = urlParams.get('module');
    //    if (!Module)
    //        return;

    //    var Table = 'uv_ContentType';
    //    var Code = 'Content_Type';
    //    var Name = 'Form_Type_Name';
    //    var FilterBy = 'Module_Code'
    //    var FilterValue = Module;
    //    var Extra = 'List_Name';

    //    var proc = svc.svc_GetOptions(Table, Code, Name, FilterBy, FilterValue, Extra);
    //    proc.then(function (response) {
    //        var data = JSON.parse(response.data.d);
    //        console.log(data);
    //        if (data.ProcessSuccess) {
    //            $scope.ddlDisplay = [...$scope.ddlDisplay, ...data.Items];
    //            $scope.Display = $scope.ddlDisplay[0];
    //        }
    //    }, function (data, status) {
    //        console.log(status);
    //        console.log(data);
    //    });
    //};

    $scope.GetModuleOptions = () => {
        var urlParams = new URLSearchParams(location.search);
        var Module = urlParams.get('module');
        if (Module == 'M016') {
            $scope.ddlDisplay = [
                { Code: "", Name: "Please Select", Extra: '' },
                { Code: "", Name: "Marketing", Extra: 'Purchase Request MKT' },
                { Code: "", Name: "Non Marketing", Extra: 'Purchase Request GA' },
            ]
            $scope.Display = $scope.ddlDisplay[0];
        }
    }
    $scope.GetModuleOptions()

    $scope.NewItem = function () {
        //if (!!$scope.Display.Code)
        location.href = '/Lists/' + $scope.Display.Extra + '/NewForm.aspx'
    }
});