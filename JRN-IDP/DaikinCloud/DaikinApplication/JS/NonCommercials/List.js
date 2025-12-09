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

app.directive('format', ['$filter', function ($filter) {
    return {
        require: '?ngModel',
        link: function (scope, elem, attrs, ctrl) {
            if (!ctrl) return;

            ctrl.$formatters.unshift(function (a) {
                return $filter(attrs.format)(ctrl.$modelValue)
            });

            elem.bind('blur', function (event) {
                var plainNumber = elem.val().replace(/[^\d|\-+|\.+]/g, '');
                elem.val($filter(attrs.format)(plainNumber));
            });
        }
    };
}]);

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

app.directive('ngFile', ['$parse', function ($parse) {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {
            element.bind('change', function () {

                $parse(attrs.ngFile).assign(scope, element[0].files)
                scope.$apply();
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
    this.svc_ListsGetListById = function (ID) {
        var param = {
            Form_No: ID,
        };
        var response = $http({
            method: "post",
            url: "/_layouts/15/WebServices/NonCommercials.asmx/ListsGetListById",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }

    this.svc_listGetListData = function () {
        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NonCommercials.asmx/ListGetListData",
            data: {},
            dataType: "json"
        });
        return response;
    }

    this.svc_ListsGetLists = function (model) {
        var param = { model }
        console.log(param, 'List NC');
        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NonCommercials.asmx/ListData",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }

    this.svc_LoadApproverRoles = function (Module_ID) {
        var param = { Module_ID: Module_ID }

        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NonCommercials.asmx/LoadApproverRoles",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;

    }

    this.svc_ListLog = function (Trans_ID, Module_Code, Transaction_ID) {

        var param = {
            Form_No: Trans_ID,
            Module_Code: Module_Code,
            Transaction_ID: Transaction_ID
        }
        console.log("Param Get History Log, ", param);
        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/Master.asmx/GetHistoryLog",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }

    this.svc_listsGetListsCreateNewForm = function (params) {
        var param = {
            Procurement_Department_ID: params.Procurement_Department_ID,
            Code: params.Code
        }

        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NonCommercials.asmx/ListsGetListsCreateNewForm",
            data: JSON.stringify(param),
            dataType: "json"
        });

        return response;
    }
});


app.controller('ctrl', function ($scope, svc, Upload, $timeout) {
    const now = new Date();
    var options = {
        year: "numeric",
        month: "short",
    };

    let DateNow = now.toLocaleDateString(undefined, options);

    $scope.showModal = 'none';

    $scope.now = DateNow;
    $scope.Param = {
        Module: {},
        FilterBy: {},
        Branch: {},
        Status: {},
        Department: {},
        SearchBy: {},
        Keywords: '',
        MIGO: {},
        Start: DateFormat_ddMMMyyyy(new Date(new Date().setDate(1))),
        End: DateFormat_ddMMMyyyy(new Date())
    };
    $scope.Modules = [];
    $scope.Branches = [];

    $scope.FiltersBy = [
        { Code: 'Created_Date', Name: 'Created Date' },
        { Code: 'Approval_Date', Name: 'Approval Date' },
        { Code: 'Scheduled_Payment_Date', Name: 'Scheduled Payment Date' },
        { Code: 'Actual_Payment_Date', Name: 'Actual Payment Date' },
    ];


    $scope.MIGOs = [
        {
            Code: '', Name: 'All'
        },
        {
            Code: '0', Name: 'Pending'
        },
        {
            Code: '1', Name: 'Completed'
        }
    ];

    $scope.Statuses = [
        { Code: '', Name: 'All' },
        { Code: '10', Name: 'Posted' },
        { Code: '3', Name: 'Pending for Submit' },
        { Code: '5', Name: 'Revised' },
        { Code: '6', Name: 'Rejected' },
        { Code: '7', Name: 'Approved' },
        { Code: '8', Name: 'Draft' },
    ];

    $scope.SearchsBy = [
        { Code: 'Form_No', Name: 'Nintex No' },
        { Code: 'Requester_Name', Name: 'Requester' },
        { Code: 'Vendor_Name', Name: 'Vendor' },
    ];

    $scope.colspan = 4;
    $scope.Departments = [];

    $scope.IsCreatedNewForm = true;

    $scope.ListsGetListById = () => {
        try {
            var id = GetQueryString()['ID'];
            if (id != undefined) {
                $scope.Param.Keywords = id;

                $scope.listGetListData();
                $scope.ListsGetLists(1);

            } else {
                $scope.listGetListData();
            }

        } catch (e) {
            console.log(e.message);
            $scope.listGetListData();
        }
    }

    $scope.listGetListData = () => {
        try {
            $scope.onChangeDDLModule();
            var proc = svc.svc_listGetListData();
            proc.then(function (response) {
                var data = JSON.parse(response.data.d);
                if (data.ProcessSuccess) {
                    console.log(data);
                    $scope.Modules = data.Modules;
                    $scope.Param.Module = data.Modules[0];

                    $scope.Branches = data.Branches;
                    $scope.Param.Branch = data.Branches[0];

                    $scope.Param.FilterBy = $scope.FiltersBy[0];
                    $scope.Param.Status = $scope.Statuses[0];

                    $scope.Param.MIGO = $scope.MIGOs[0];

                    $scope.Departments = data.Departments;
                    $scope.Param.Department = $scope.Departments[0];

                    $scope.Param.SearchBy = $scope.SearchsBy[0];

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


    $scope.ddlPendingApprover = [];
    $scope.PendingApproverRole = {};

    $scope.onChangeDDLModule = function () {
        $scope.ddlPendingApprover = [];
        $scope.PendingApproverRole = {};
        $scope.GrandTotal = 0;
        $scope.Total = 0;
        try {
            const MasterApprover = 'Master Approver Non Commercials';
            if ($scope.Param.Module.Code == undefined) {
                $scope.Param.Module.Code = "M016";
            }
            console.log($scope.Param.Module.Code);
            console.log($scope.Param);
            $scope.SearchsBy = [
                { Code: 'Form_No', Name: 'Nintex No' },
                { Code: 'Requester_Name', Name: 'Requester' },
            ];

            if ($scope.Param.Module.Code == 'M014') {
                $scope.SearchsBy.push(
                    { Code: 'Remarks', Name: 'Remarks' },
                    { Code: 'Vendor_Name', Name: 'Vendor Name' },
                    { Code: 'Material', Name: 'Material Anaplan' }
                );
            }
            if ($scope.Param.Module.Code == 'M016') {
                $scope.SearchsBy.push(
                    { Code: 'Dealer', Name: 'Dealer Name' }
                );
            }
            if ($scope.Param.Module.Code == 'M015' || $scope.Param.Module.Code == 'M017' || $scope.Param.Module.Code == 'M018' || $scope.Param.Module.Code == 'M020') {
                $scope.SearchsBy.push(
                    { Code: 'Material', Name: 'Material Anaplan' },
                    { Code: 'Vendor_Name', Name: 'Vendor Name' }
                );
            }
            if ($scope.Param.Module.Code == 'M016' || $scope.Param.Module.Code == 'M017') {
                $scope.SearchsBy.push(
                    { Code: 'Dealer_Name', Name: 'Dealer Name' }
                );
            }
            if ($scope.Param.Module.Code == 'M020') {
                $scope.SearchsBy.push(
                    { Code: 'Contract', Name: 'Contract' }
                );
            }
            $scope.Param.SearchBy = $scope.SearchsBy[0];

            var module_code = $scope.Param.Module.Code == undefined ? '' : $scope.Param.Module.Code;
            var proc = svc.svc_LoadApproverRoles(module_code);
            proc.then(function (response) {
                var data = JSON.parse(response.data.d);
                if (data.ProcessSuccess) {
                    // console.log(data);
                    $scope.Items = [];
                    $scope.ddlPendingApprover = data.listApproverRole;
                    $scope.PendingApproverRole = data.listApproverRole[0];

                    if ($scope.Param.Module.Code == "M018") {
                        $scope.PendingApproverRole = data.listApproverRole[0];
                    }


                    $scope.colspan = 4;
                    if (['M016', 'M017', 'M018'].indexOf($scope.Param.Module.Code) >= 0) $scope.colspan = 6;
                    else if (['M020'].indexOf($scope.Param.Module.Code) >= 0) $scope.colspan = 5;
                    else if (['M015'].indexOf($scope.Param.Module.Code) >= 0) $scope.colspan = 7;

                    $scope.IsCreatedNewForm = true;
                    if (['M018', 'M017', 'M015'].indexOf($scope.Param.Module.Code) >= 0) $scope.IsCreatedNewForm = false;
                } else {
                    console.log(data.InfoMessage, 'onChangeDDLModule');
                }
            }, function (data, status) {
                console.log(data.statusText + ' - ' + data.data.Message);
            });

        } catch (e) {
            console.log(e.message);
        }
    }

    $scope.selected = [];
    $scope.ItemIDs = [];
    $scope.TaskIDs = [];
    $scope.Items = [];
    $scope.Keywords = '';
    $scope.GrandTotal = 0;
    $scope.Form = false;
    $scope.ListsGetLists = (PageIndex) => {
        try {
            var param = {
                TableName: $scope.Param.Module.Table_Name,
                FilterBy: $scope.Param.FilterBy.Code,
                StartDate: $scope.Param.Start,
                EndDate: $scope.Param.End,
                PageIndex: PageIndex,
                SearchBy: $scope.Param.SearchBy.Code,
                Keywords: $scope.Param.Keywords,
                PageSize: 10,
                MIGO: $scope.Param.MIGO.Code,
                // RecordCount:0,
                // GrandTotal:0,
                Procurement_Department: $scope.Param.Department.Name,
                PaymentStatus: 0,
                PostingStatus: $scope.Param.Status.Code,
                BranchName: $scope.Param.Branch.Name,
                ModuleId: $scope.Param.Module.Code,
                PendingApproverRole: $scope.PendingApproverRole.Name == undefined ? '' : $scope.PendingApproverRole.Name,
            }

            // console.log($scope.PendingApproverRole);
            var proc = svc.svc_ListsGetLists(param);
            proc.then(function (response) {
                var data = JSON.parse(response.data.d);
                if (data.ProcessSuccess) {
                    console.log(data);
                    $scope.GrandTotal = data.GrandTotal;
                    $scope.Total = 0;
                    if ($scope.Param.Module.Code == "M018") {
                        data.Items.forEach(e => {
                            let SubStringFormCode = e.PO_No.substring(2, 0);

                            if (SubStringFormCode == "PC") e.check = true;
                        });
                    }
                    $scope.Items = data.Items;

                    for (x in $scope.Items) {
                        $scope.Total += $scope.Items[x].Grand_Total;
                        for (y in $scope.Items[x]) {
                            if (y.endsWith('Date')) {
                                $scope.Items[x][y] = $scope.ConvertJSONDate($scope.Items[x][y]);
                            }
                        }
                    }
                    $(".Pager").ASPSnippets_Pager({
                        ActiveCssClass: "current",
                        PagerCssClass: "pager",
                        PageIndex: data.PageIndex,
                        PageSize: data.PageSize,
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

    $("body").on("click", ".Pager .page", function () {
        $scope.ListsGetLists(parseInt($(this).attr('page')));
    });

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

    $scope.closeModal = function () {
        $scope.showModal = 'none';
    }

    $scope.ApproverLog = function (obj) {
        $scope.showModal = 'block';
        var proc = svc.svc_ListLog(obj.Form_No, $scope.Param.Module.Code, obj.ID);

        proc.then(function (response) {
            var data = JSON.parse(response.data.d);
            //console.log('Approval Log Data:', data);
            if (data.ProcessSuccess) {
                $scope.Logs = data.Logs;
                $scope.Module_Name = $scope.Param.Module.Name;
                $scope.Nintex_No = obj.Form_No;
                $scope.Pending_Approver_Name = obj.Pending_Approver_Name;
                $scope.Pending_Approver_Role = obj.Pending_Approver_Role;
            } else {
                alert(data.InfoMessage);
            }
        }, function (data, status) {
            console.log(data.statusText + ' - ' + data.data.Message);
        });

    }

    $scope.listsGetListsCreateNewForm = () => {
        try {
            let param = {
                Procurement_Department_ID: $scope.Param.Department.Code ? $scope.Param.Department.Code : 2,
                Code: $scope.Param.Module.Code
            }
            // console.log($scope.Param.Module.Code)

            if ($scope.Param.Module.Code == "M016") {
                //location.href = "/_layouts/15/Daikin.WebApps/Master/Display.aspx?module=M016";
                var targetURL = "/_Layouts/15/Daikin.Application/NonCommercials/Display.aspx?module=M016";
                location.href = targetURL;

            }
            else if ($scope.Param.Module.Code == "M017") {
                var targetURL = "/Lists/QCF " + (($scope.Param.Department.Name.toUpperCase().includes("MARKETING")) ? "MKT/NewForm.aspx" : "GA IT/NewForm.aspx");
                location.href = targetURL;
            }
            else {
                var proc = svc.svc_listsGetListsCreateNewForm(param);
                proc.then(function (response) {
                    var data = JSON.parse(response.data.d);
                    if (data.ProcessSuccess) {
                        console.log(data);

                        var objIndex = data.ProcDept.findIndex(o => o.Code == $scope.Param.Module.Code);
                        if (objIndex !== -1) {
                            location.href = data.ProcDept[objIndex].Module_Url;
                        }
                        console.log(data.ProcDept[objIndex].Module_Url);
                    } else {
                        alert(data.InfoMessage);
                    }
                }, function (data, status) {
                    console.log(data.statusText + ' - ' + data.data.Message);
                });
            }
        } catch (e) {
            console.log(e.message);
        }
    }

    $scope.listGetListData();
});