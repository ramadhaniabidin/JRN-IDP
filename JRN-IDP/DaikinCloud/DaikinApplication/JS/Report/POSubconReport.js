const app = angular.module('app', ['ngFileUpload']);

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
app.directive("datepicker", function () {
    return {
        restrict: "A",
        require: "ngModel",
        link: function (scope, elem, attrs, ngModelCtrl) {
            const updateModel = function (dateText) {
                scope.$apply(function () {
                    ngModelCtrl.$setViewValue(dateText);
                });
            };
            const options = {
                dateFormat: "d M yy",
                onSelect: function (dateText) {
                    updateModel(dateText);
                }
            };
            elem.datepicker(options);
        }
    }
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
app.filter("FormatDate", function () {
    const re = /\/Date\(([0-9]*)\)\//;
    return function (x) {
        const m = x.match(re);
        if (m) return new Date(parseInt(m[1]));
        else return null;
    };
});


app.service("svc", function ($http) {
    this.svc_GetOptions = function () {
        const param = {
            ListName: 'Commercials'
        };
        const response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/ClaimReimbursement.asmx/ModuleOptions",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }
    this.svc_ListData = function (model) {
        const param = {
            model: model
        };

        console.log('param svc_ListData', param);
        const response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NACWebService.asmx/ListDataPOSubcon",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }
});


app.controller('ctrl', function ($scope, svc) {
    $scope.Branch = {}
    $scope.Status = { Name: 'Completed' }
    $scope.ddlStatus = [
        { Name: 'Completed' },
        { Name: 'On Going' }
    ]
    $scope.Date = {
        Start: DateFormat_ddMMMyyyy(new Date(new Date().setDate(1))),
        End: DateFormat_ddMMMyyyy(new Date())
    };

    $scope.ddlSearchBy = [
    { Code: 'Form_No', Name: 'Nintex No' },
    { Code: 'Requester_Name', Name: 'Requester' },
    ];
    $scope.SearchBy = $scope.ddlSearchBy[0];
    $scope.Keywords = '';
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
    $scope.GetModuleOptions = function (x) {
        const proc = svc.svc_GetOptions();
        proc.then(function (response) {
            const data = JSON.parse(response.data.d);
            console.log(data);
            if (data.ProcessSuccess) {
                $scope.ddlBranch = data.listBranch;
                console.log('GetModuleOptions - Branch: ', data.Branch);
                let br = data.Branch;

                if (data.Branch == undefined) {
                    br = '';
                } else if (data.Branch == '') {
                    br = '';
                }

                if (br == '') {
                    $scope.Branch = data.listBranch[0];
                } else {
                    $scope.Branch = data.listBranch.find(o => o.Name == data.Branch);
                }
                console.log($scope.Branch, '$scope.Branch');

                //if (!!x)
                //x();
            }
        }, function (data, status) {
            console.log(data.statusText + ' - ' + data.data.Message);
        });
    };
    $scope.ListData = function () {
        const SearchBy = $scope.SearchBy.Code;
        const Keywords = $scope.Keywords
        const StartDate = $scope.Date.Start;
        const EndDate = $scope.Date.End;
        let Branch = $scope.Branch == undefined ? '' : $scope.Branch.Name;
        const Status = $scope.Status.Name
        if (Branch == 'All') {
            Branch = '';
        }
        const param = {
            SearchBy: SearchBy,
            Keywords: Keywords,
            Branch: Branch,
            StartDate: StartDate,
            EndDate: EndDate,
            Status: Status
        };
        const proc = svc.svc_ListData(param);
        proc.then(function (response) {
            const data = JSON.parse(response.data.d);
            console.log(data);
            if (data.ProcessSuccess) {
                $scope.Items = data.Items;
                for (let x in $scope.Items) {
                    for (let y in $scope.Items[x]) {
                        if (y.endsWith('Date')) {
                            $scope.Items[x][y] = $scope.ConvertJSONDate($scope.Items[x][y]);
                        }
                    }
                }
            }
        }, function (data, status) {
            console.log(data);
            alert(data.statusText + ' - ' + data.data.Message);
        });
    }

    $scope.Search = () => {
        $scope.ListData()
    }
    $scope.GetModuleOptions()

});