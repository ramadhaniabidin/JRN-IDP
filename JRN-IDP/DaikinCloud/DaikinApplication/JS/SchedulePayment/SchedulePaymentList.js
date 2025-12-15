var app = angular.module('app', []);
$(".table").click(
    function (event) {
        event.preventDefault();
        alert('Picked:');
    }
);

app.directive("datepicker", function () {
    return {
        restrict: "A",
        require: "ngModel",
        link: function (scope, elem, attrs, ngModelCtrl) {
            var updateModel = function (dateText) {
                scope.$apply(function () {
                    ngModelCtrl.$setViewValue(dateText);
                });
            };
            var options = {
                dateFormat: "d M yy",
                onSelect: function (dateText) {
                    updateModel(dateText);
                }
            };
            elem.datepicker(options);
        }
    }
});


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

app.filter("FormatDate", function () {
    var re = /\/Date\(([0-9]*)\)\//;
    return function (x) {
        var m = x.match(re);
        if (m) return new Date(parseInt(m[1]));
        else return null;
    };
});

app.service("svc", function ($http) {
    this.svc_ListData = function (SearchBy, Keyword, PageIndex, PageSize, Start_Date, End_Date, BankName) {
        var param = {
            model: {
                SearchBy: SearchBy,
                Keywords: Keyword,
                PageIndex: PageIndex,
                PageSize: PageSize,
                Payment_Date_Start: Start_Date,
                Payment_Date_End: End_Date,
                BankName: BankName
            }
        };
        console.log("Param for ListData ", param);
        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/ScheduledPayment.asmx/ListData",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }

    this.svc_HistoryLogList = function (form_no, module_code, transaction_id) {
        var param = {
            Form_No: form_no,
            Module_Code: module_code,
            Transaction_ID: transaction_id
        };
        console.log("Param for approval log ", param);

        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application//WebServices/Master.asmx/GetHistoryLog",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    };
});

app.controller("ctrl", function ($scope, svc) {
    $scope.ddlBankName = [
        { Code: 'BCA', Name: 'BCA' },
        { Code: 'MUFG-SMBC', Name: 'MUFG-SMBC' },
    ];
    $scope.Items = [];
    $scope.Logs = [];
    $scope.bankName = $scope.ddlBankName[0];
    $scope.showModal = 'none';

    $scope.ddlSearchBy = [
        { Code: 'Form_No', Name: 'Title' },
    ];

    $scope.SearchBy = $scope.ddlSearchBy[0];
    $scope.Keywords = "";
    $scope.currPageIndex = 1;
    $scope.pageSize = 10;
    $scope.paymentDateFrom = DateFormat_ddMMMyyyy(new Date(new Date().setDate(1)));
    $scope.paymentDateEnd = DateFormat_ddMMMyyyy(new Date());


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

    $scope.ConvertJSONDate = function (x, format) {
        if (!format) format = "{dd}-{mmm}-{yyyy}";
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

    $scope.List_Data = (pageIndex) => {
        var Payment_Date_Start = $scope.paymentDateFrom == null ? "" : $scope.paymentDateFrom;
        var Payment_Date_End = $scope.paymentDateEnd == null ? "" : $scope.paymentDateEnd;
        var bankName = $scope.bankName.Code;
        var proc = svc.svc_ListData($scope.SearchBy.Code, $scope.Keywords, pageIndex, $scope.pageSize, Payment_Date_Start, Payment_Date_End, bankName);
        proc.then(function (resp) {
            var jsonData = JSON.parse(resp.data.d);
            console.log('JSON data ', jsonData);
            var Items = jsonData.Items;
            $scope.Total = 0;
            $scope.GrandTotal = jsonData.GrandTotal;
            for (let x in Items) {
                $scope.Total += Items[x].Amount;
                for (let y in Items[x]) {
                    if (y.endsWith('Date')) {
                        Items[x][y] = $scope.ConvertJSONDate(Items[x][y]);
                    }
                }
            }

            $scope.Items = Items;
            $(".Pager").ASPSnippets_Pager({
                ActiveCssClass: "current",
                PagerCssClass: "pager",
                PageIndex: jsonData.PageIndex,
                PageSize: jsonData.PageSize,
                RecordCount: jsonData.RecordCount
            });

        }).catch(function (err) {
            console.log(err);
        });
    };

    $("body").on("click", ".Pager .page", function () {
        $scope.List_Data(parseInt($(this).attr('page')));
    });

    $scope.closeModal = function () {
        $scope.showModal = 'none';
    };

    $scope.ApprovalLog = (form_no) => {
        $scope.showModal = 'block';
        var proc = svc.svc_HistoryLogList(form_no, 'M008', 0);
        proc.then(function (response) {
            var jsonData = JSON.parse(response.data.d);
            console.log(jsonData);
            if (jsonData.ProcessSuccess) {
                $scope.Logs = jsonData.Logs;
            }
        }).catch(function (err) {
            console.log(err);
        });
    };

    $scope.List_Data($scope.currPageIndex);
});