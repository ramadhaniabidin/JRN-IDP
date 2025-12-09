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
            model:model
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


    $scope.ConvertJSONDate = function (x, format) {
        if (format == undefined) format = "{dd}-{mmm}-{yyyy}";
        if (x == null || !x) return x;
        var re = /\/Date\(([0-9]*)\)\//;
        var m = x.match(re);
        var jsondate = "";
        if (m) {
            jsondate = new Date(parseInt(m[1]));
            var date = new Date(jsondate);
            year = date.getFullYear();

            month = date.getMonth();
            months = {
                mmmm: new Array("January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"),
                mmm: new Array("Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"),
                mm: new Array("01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12"),
                m: new Array("1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12"),
            };

            d = date.getDate();
            if (d < 10) dd = "0" + d;
            else dd = d;

            day = date.getDay();
            days = new Array("Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday");
            //days = new Array('Minggu', 'Senin', 'Selasa', 'Rabu', 'Kamis', 'Jumat', 'Sabtu');

            H = date.getHours();
            if (H < 10) HH = "0" + H;
            else HH = H;

            M = date.getMinutes();
            if (M < 10) MM = "0" + M;
            else MM = M;

            S = date.getSeconds();
            if (S < 10) SS = "0" + S;
            else SS = S;

            format = format.replace("{yyyy}", year);
            format = format.replace("{mmmm}", months["mmmm"][month]);
            format = format.replace("{mmm}", months["mmm"][month]);
            format = format.replace("{mm}", months["mm"][month]);
            format = format.replace("{m}", months["m"][month]);
            format = format.replace("{dd}", dd);
            format = format.replace("{d}", d);

            format = format.replace("{HH}", HH);
            format = format.replace("{H}", H);
            format = format.replace("{MM}", MM);
            format = format.replace("{M}", M);
            format = format.replace("{SS}", SS);
            format = format.replace("{S}", S);

            format = format.replace("{day}", days[day]);

            return format;
        } else return null;
    }

    $scope.TaskApprovalGetPendingTaskApproval = (pageNo) => {
        try {
            $scope.pageNo = pageNo;

            var proc = svc.svc_TaskApprovalGetPendingTaskApproval($scope.pageNo, $scope.rowlimit, $scope.SearchBy.Code, $scope.Keywords);
            proc.then(function (response) {
                var data = JSON.parse(response.data.d);
                if (data.ProcessSuccess) {
                    // console.log(data);
                    console.log(data);

                    $scope.PendingTasks = data.data;

                    for (x in $scope.PendingTasks) {
                        for (y in $scope.PendingTasks[x]) {
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