var app = angular.module('app', ['ngFileUpload']);

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
    this.svc_TaskApprovalGetPendingTaskApproval = function (pageNo, rowlimit, SearchBy, Keywords) {
        var model = {
            PageIndex: pageNo,
            PageSize: rowlimit,
            SearchBy: SearchBy,
            Keywords: Keywords,
        }
        var param = {
            model: model
        }

        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/PendingTask.asmx/TaskApprovalByCurrentLogin",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }
});

app.controller('ctrl', function ($scope, svc) {
    $scope.pageNo = 1;
    $scope.rowlimit = 20;
    $scope.PendingTasks = [];
    $scope.Keywords = '';

    $scope.ddlSearchBy = [
        { Code: 'ItemTitle', Name: 'Nintex No' },
        { Code: 'RequestorName', Name: 'Requestor' },
    ];

    $scope.SearchBy = $scope.ddlSearchBy[0];

    $scope.GetMonthFormat = function () {
        const months = {
            mmmm: ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"],
            mmm: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
            mm: ["01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12"],
            m: ["1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12"]
        };
        return months;
    };

    $scope.GetFormatMapping = function (date, months, days, pad) {
        const map = {
            "{yyyy}": date.getFullYear(),
            "{mmmm}": months.mmmm[date.getMonth()],
            "{mmm}": months.mmm[date.getMonth()],
            "{mm}": months.mm[date.getMonth()],
            "{m}": months.m[date.getMonth()],
            "{dd}": pad(date.getDate()),
            "{d}": date.getDate(),
            "{HH}": pad(date.getHours()),
            "{H}": date.getHours(),
            "{MM}": pad(date.getMinutes()),
            "{M}": date.getMinutes(),
            "{SS}": pad(date.getSeconds()),
            "{S}": date.getSeconds(),
            "{day}": days[date.getDay()]
        };
        return map;
    };

    $scope.ConvertJSONDate = function (x, format = "{dd}-{mmm}-{yyyy}") {
        if (!x) return x;
        const match = x.match(/\/Date\(([0-9]*)\)\//);
        if (!match) return null;

        const date = new Date(Number.parseInt(match[1]));
        const pad = num => (num < 10 ? "0" + num : num);
        const months = $scope.GetMonthFormat();
        const days = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
        const map = $scope.GetFormatMapping(date, months, days, pad);
        Object.keys(map).forEach(key => {
            format = format.replace(key, map[key]);
        });
        return format;
    };

    $scope.TaskApprovalGetPendingTaskApproval = (pageNo) => {
        try {
            $scope.pageNo = pageNo;

            var proc = svc.svc_TaskApprovalGetPendingTaskApproval($scope.pageNo, $scope.rowlimit, $scope.SearchBy.Code, $scope.Keywords);
            proc.then(function (response) {
                var data = JSON.parse(response.data.d);
                if (data.ProcessSuccess) {
                    $scope.PendingTasks = data.data;

                    for (let x in $scope.PendingTasks) {
                        for (let y in $scope.PendingTasks[x]) {
                            if (y.endsWith('Date')) {
                                $scope.PendingTasks[x][y] = $scope.ConvertJSONDate($scope.PendingTasks[x][y]);
                            }
                        }
                    }

                    $(".Pager").ASPSnippets_Pager({
                        ActiveCssClass: "current",
                        PagerCssClass: "pager",
                        PageIndex: $scope.pageNo,
                        PageSize: $scope.rowlimit,
                        RecordCount: data.RecordCount
                    });
                } else {
                    alert(data.InfoMessage);
                }
            }, function (data, status) {
                console.log(data.statusText + ' - ' + data.data.Message);
            });

        } catch (e) {
            console.log(e.message);
        }
    }

    $scope.documentReady = function () {
        $scope.TaskApprovalGetPendingTaskApproval(1);
    }

    $("body").on("click", ".Pager .page", function () {
        $scope.TaskApprovalGetPendingTaskApproval(parseInt($(this).attr('page')));
    });

    $scope.SearchHelper = function (keyEvent) {
        if (keyEvent.which === 13) {
            $scope.documentReady();
        }
    };

    $scope.Search = function () {
        $scope.documentReady();
    };


    $scope.documentReady();
});