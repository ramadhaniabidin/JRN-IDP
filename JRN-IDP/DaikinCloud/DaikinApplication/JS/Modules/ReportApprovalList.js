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
    this.svc_GetOptions = function (ListName) {
        var param = {
            ListName: ListName
        };
        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/ClaimReimbursement.asmx/ModuleOptions",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }
    this.svc_ListData = function (model) {

        var param = {
            model: model
        }

        console.log('param svc_ListData', param);
        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NACWebService.asmx/ListData",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }
    this.svc_ListDataNonCommercials = function (model) {

        var param = {
            model: model
        }

        console.log('param svc_ListData', param);
        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NACWebService.asmx/ListDataNonCommercials",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }
    this.svc_ListDataCommercials = function (model) {

        var param = {
            model: model
        }

        console.log('param svc_ListData', param);
        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NACWebService.asmx/ListDataCommercials",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    };

    this.svc_SPDEV_ListDataApproval = function (model) {
        const param = {
            model: model
        };

        console.log('param svc_ListData', param);
        const response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NACWebService.asmx/SPDEV_ListDataApproval",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    };
    
});

app.controller('ctrl', function ($scope, svc) {
    $scope.Branch = {};


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
    $scope.ddlModuleCategory = [
        { Name: 'Claim Reimbursement' },
        { Name: 'Non Commercials' },
        { Name : 'Commercials' }
    ]
    $scope.ModuleCategory = $scope.ddlModuleCategory[0]
    $scope.onChangeModuleCategory = function () {
        console.log($scope.ModuleCategory);
        $scope.Items = [];
        $scope.GetModuleOptions();
    }

    $scope.Module = {};
    $scope.ddlModule = [];

    $scope.ddlProcDepartments = [
        { Code: '', Name: 'All' },
        { Code: '4', Name: 'Service' },
        { Code: '5', Name: 'HP Compensation and Benefit' },
        { Code: '6', Name: 'HR Recruitment' },
        { Code: '7', Name: 'Training' },
        { Code: '8', Name: 'SCM Sparepart' },
        { Code: '9', Name: 'SCM Finish Good' },
        { Code: '22', Name: 'Marketing Trade' },
        { Code: '23', Name: 'Marketing Digital' },
        { Code: '24', Name: 'Information Technology (IT)' },
        { Code: '25', Name: 'General Affair (GA)' },
    ];
    $scope.Department = $scope.ddlProcDepartments[0];
    $scope.GetModuleOptions = function (x) {
        var proc = svc.svc_GetOptions($scope.ModuleCategory.Name);
        proc.then(function (response) {
            var data = JSON.parse(response.data.d);
            console.log(data);
            if (data.ProcessSuccess) {
                if ($scope.ModuleCategory.Name == 'Claim Reimbursement') {
                    $scope.ddlModule = [{ Name: 'All', List_Name: 'All', }, ...data.Items];
                } else {
                    $scope.ddlModule = [...data.Items];
                }
                //$scope.Module = $scope.ddlModule[0];
                var module_code = GetQueryString()['module'];

                $scope.ddlBranch = data.listBranch;
                console.log('GetModuleOptions - Branch: ', data.Branch);
                var br = data.Branch;

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

                if (module_code == undefined) {
                    $scope.Module = $scope.ddlModule[0];
                } else {
                    $scope.Module = $scope.ddlModule.find(o => o.Code == module_code);
                }
                $scope.OnChangeModule()
                //if (!!x)
                //x();
            }
        }, function (data, status) {
            console.log(data.statusText + ' - ' + data.data.Message);
        });
    };

    $scope.tableHead = []
    $scope.OnChangeModule = function () {
        $scope.Items = [];
        if($scope.ModuleCategory.Name == 'Claim Reimbursement'){
            $scope.tableHead = [
                { Name: 'Direct Head' },
                { Name: 'Receiver Document' },
                { Name: 'Verifier Document' },
                { Name: 'Finance Verifier 2' },
                { Name: 'Pending SAP Post' },
            ]
        } else {
            if ($scope.Module.Code == 'M014') {
                $scope.tableHead = [
                    { Name: 'Direct Head/Head Of Department' },
                    { Name: 'PIC Contract/ Head of Departement MKT' },
                    { Name: 'PIC Contract MKT' },
                ]
            } else if ($scope.Module.Code == 'M016') {
                $scope.tableHead = [
                    { Name: 'Direct Head' },
                    { Name: 'Head of Departement' },
                    { Name: 'GM Planning' },
                ]
            } else if ($scope.Module.Code == 'M017') {
                $scope.tableHead = [
                    { Name: 'Head Of Branch' },
                    { Name: 'Direct Head' },
                    { Name: 'Head of Departement' },
                    { Name: 'AGM / GM' },
                    { Name: 'CFO' },
                    { Name: 'CEO' }
                ]
            } else if ($scope.Module.Code == 'M018') {
                $scope.tableHead = [
                    { Name: 'Accounting Manager' },
                    { Name: 'Receiver Document' },
                    { Name: 'Verifier Document' },
                    { Name: 'Finance Verifier 2' },
                    { Name: 'Tax Verification' },
                    { Name: 'Pending SAP Post' }
                ]
            } else if ($scope.Module.Code == 'M019' || $scope.Module.Code == 'M026') {
                $scope.tableHead = [
                    { Name: 'Head Of Service' },
                    { Name: 'Field Service Manager' },
                    { Name: 'Finance Verifier - Subcon 1' },
                    { Name: 'Tax Verifier - Subcon' },
                    { Name: 'Manager Service Operation' },
                    { Name: 'Assistant GM Service / Svc. Planning Manager' },
                    { Name: 'Finance Verifier - Subcon 2' },
                    { Name: 'Finance Receiver - Subcon' },
                    { Name: 'Waiting For MIRO' }
                ]
            } else if ($scope.Module.Code == 'M010') {
                $scope.tableHead = [
                    { Name: 'Direct Head' },
                    { Name: 'AGM / GM Planning / Head Of Branch' },
                    { Name: 'Receiver Document' },
                    { Name: 'Verifier Document' },
                    { Name: 'Verifier Document 2' },
                    { Name: 'Tax Verification' },
                    { Name: 'Pending SAP Post' }
                ]
            } else if ($scope.Module.Code == 'M011') {
                $scope.tableHead = [
                    { Name: 'Head of Budget Owner' },
                    { Name: 'AGM / GM Planning / Head Of Branch' },
                    { Name: 'Receiver Document' },
                    { Name: 'Finance Verification' },
                    { Name: 'Finance Verification 2' },
                    { Name: 'Tax Verification' },
                    { Name: 'Pending SAP Post' }
                ]
            } else if ($scope.Module.Code == 'M020') {
                $scope.tableHead = [
                    { Name: 'Direct Head' },
                    { Name: 'Head Of Marketing' }
                ]
            } else if ($scope.Module.Code == 'M025') {
                $scope.tableHead = [
                    { Name: 'Head of Budget Owner' },
                    { Name: 'Head of Department' },
                    { Name: 'GM Planning' },
                    { Name: 'Receiver Document' },
                    { Name: 'Finance Verification' },
                    { Name: 'Finance Senior Manager' },
                    { Name: 'Payment Confirmation' },
                    { Name: 'Waiting For MIRO' }
                ]
            }
        }
    }

    $scope.Date = {
        Start: DateFormat_ddMMMyyyy(new Date(new Date().setDate(1))),
        End: DateFormat_ddMMMyyyy(new Date())
    };
    
    $scope.ListData = function () {
        var ModuleCategory = $scope.ModuleCategory.Name;
        var Module = $scope.Module.Name;
        var StartDate = $scope.Date.Start;
        var EndDate = $scope.Date.End;
        var Branch = $scope.Branch == undefined ? '' : $scope.Branch.Name;
        if (Branch == 'All') {
            Branch = '';
        }
        if (Module == 'All') {
            Module = '';
        }
        var param = {
            ModuleCategory: ModuleCategory,
            Module: Module,
            Branch: Branch,
            StartDate: StartDate,
            EndDate: EndDate
        };


        if (ModuleCategory != undefined) {
            //var proc = svc.svc_ListData(param);
            const proc = svc.svc_SPDEV_ListDataApproval(param);
            proc.then(function (response) {
                var data = JSON.parse(response.data.d);
                console.log(data);
                if (data.ProcessSuccess) {
                    $scope.Items = data.Items;
                }
            }, function (data, status) {
                console.log(data);
                alert(data.statusText + ' - ' + data.data.Message);
            });
        }
    }
    $scope.ListDataNonCommercials = function () {
        var ModuleCategory = $scope.ModuleCategory.Name;
        var Module = $scope.Module.Name
        var StartDate = $scope.Date.Start;
        var EndDate = $scope.Date.End;
        var Branch = $scope.Branch == undefined ? '' : $scope.Branch.Name;
        var ProcDept = $scope.Department.Name
        if (Branch == 'All') {
            Branch = '';
        }
        if (Module == 'All') {
            Module = '';
        }        
        if (ProcDept == 'All') {
            ProcDept = '';
        }
        var param = {
            ModuleCategory: ModuleCategory,
            Module: Module,
            Branch: Branch,
            StartDate: StartDate,
            EndDate: EndDate,
            ProcDept: ProcDept
        };


        if (ModuleCategory != undefined) {
            //var proc = svc.svc_ListDataNonCommercials(param);
            const proc = svc.svc_SPDEV_ListDataApproval(param);
            proc.then(function (response) {
                const data = JSON.parse(response.data.d);
                console.log(data);
                if (data.ProcessSuccess) {
                    $scope.Items = data.Items;
                }
            }, function (data, status) {
                console.log(data);
                alert(data.statusText + ' - ' + data.data.Message);
            });
        }
    }
    $scope.ListDataCommercials = function () {
        var ModuleCategory = $scope.ModuleCategory.Name;
        var Module = $scope.Module.Name
        var StartDate = $scope.Date.Start;
        var EndDate = $scope.Date.End;
        var Branch = $scope.Branch == undefined ? '' : $scope.Branch.Name;
        if (Branch == 'All') {
            Branch = '';
        }
        if (Module == 'All') {
            Module = '';
        }
        var param = {
            ModuleCategory: ModuleCategory,
            Module: Module,
            Branch: Branch,
            StartDate: StartDate,
            EndDate: EndDate
        };


        if (ModuleCategory != undefined) {
            //var proc = svc.svc_ListDataCommercials(param);
            const proc = svc.svc_SPDEV_ListDataApproval(param);
            proc.then(function (response) {
                const data = JSON.parse(response.data.d);
                console.log(data);
                if (data.ProcessSuccess) {
                    $scope.Items = data.Items;
                }
            }, function (data, status) {
                console.log(data);
                alert(data.statusText + ' - ' + data.data.Message);
            });
        }
    }

    $scope.Search = () => {
        if ($scope.ModuleCategory.Name == 'Claim Reimbursement') {
            $scope.ListData()
        } else if ($scope.ModuleCategory.Name == 'Non Commercials') {
            $scope.ListDataNonCommercials()
        } else if ($scope.ModuleCategory.Name == 'Commercials') {
            $scope.ListDataCommercials()
        }
    };

    $scope.Export = () => {
        if($scope.ModuleCategory.Name == 'Claim Reimbursement' ){
            $("#tblReportsClaimReimbursement").table2excel({
                filename: "ReportTables.xls"
            });
        }
        else if (($scope.ModuleCategory.Name == 'Non Commercials' && $scope.Module.Code == 'M016') || ($scope.ModuleCategory.Name == 'Non Commercials' && $scope.Module.Code == 'M014')) {
            $("#tblReportsPROrContract").table2excel({
                filename: "ReportTables.xls"
            }); 
        }
        else if (($scope.ModuleCategory.Name == 'Non Commercials' && $scope.Module.Code == 'M017') || ($scope.ModuleCategory.Name == 'Commercials' && $scope.Module.Code == 'M011') || ($scope.ModuleCategory.Name == 'Commercials' && $scope.Module.Code == 'M010')) {
            $("#tblReportsQCForServistCostorFOB").table2excel({
                filename: "ReportTables.xls"
            }); 
        }
        else if ($scope.ModuleCategory.Name == 'Non Commercials' && $scope.Module.Code == 'M018') {
            $("#tblReportsPORelease").table2excel({
                filename: "ReportTables.xls"
            });
        }
        else if ($scope.ModuleCategory.Name == 'Non Commercials' && $scope.Module.Code == 'M020') {
            $("#tblReportsPOContract").table2excel({
                filename: "ReportTables.xls"
            });
        }
        else if ($scope.ModuleCategory.Name == 'Commercials' && $scope.Module.Code == 'M019') {
            $("#tblReportsPOSubcon").table2excel({
                filename: "ReportTables.xls"
            });
        }

        else if ($scope.ModuleCategory.Name == 'Commercials' && $scope.Module.Code == 'M025') {
            $("#tblReportsPIB").table2excel({
                filename: "ReportTables.xls"
            });
        } 
    }


    //$scope.onChangeDDLPostingStatus = function () {
    //    if ($scope.PostingStatus.Code != 4) {
    //        $scope.PendingApprovalRole = {};
    //        $scope.MasterRoleApproverCR = [];
    //    }

    //    var proc = svc.svc_GetPendingApprovalRole();
    //    proc.then(function (response) {
    //        var data = JSON.parse(response.data.d);
    //        console.log(data);
    //        if (data.ProcessSuccess) {
    //            $scope.ddlPendingApprovalRole = data.MasterRoleApproverCR;
    //            $scope.MasterRoleApproverCR = data.MasterRoleApproverCR[0];

    //        }
    //    }, function (data, status) {
    //        console.log(data.statusText + ' - ' + data.data.Message);
    //    });
    //}

    //$scope.ddlFilterBy = [
    //    //{ Code: "", Name: "Please Select" },
    //    { Code: 'Created_Date', Name: 'Created Date' },
    //    { Code: 'Approval_Date', Name: 'Approval Date' },
    //    { Code: 'MIRO_Date', Name: 'MIRO Date' },
    //    { Code: 'Scheduled_Payment_Date', Name: 'Scheduled Payment Date' },
    //    { Code: 'Actual_Payment_Date', Name: 'Actual Payment Date' }
    //];

    //$scope.FilterBy = $scope.ddlFilterBy[0];

    //$scope.ddlPaymentStatus = [
    //    //{ Code: "", Name: "Please Select" },
    //    { Code: '', Name: 'All' },
    //    { Code: '1', Name: 'Paid' },
    //    { Code: '0', Name: 'Unpaid' },
    //];

    //$scope.PaymentStatus = $scope.ddlPaymentStatus[0];

    //$scope.ddlPostingStatus = [
    //    //{ Code: "", Name: "Please Select" },
    //    { Code: '', Name: 'All' },
    //    { Code: '1', Name: 'Submitted' },
    //    { Code: '4', Name: 'Pending Approval' },
    //    { Code: '1', Name: 'Posted' },
    //    { Code: '0', Name: 'Pending SAP Post' },
    //    { Code: '5', Name: 'Revised' },
    //    { Code: '6', Name: 'Rejected' },
    //    { Code: '7', Name: 'Approved' },
    //    { Code: '8', Name: 'Draft' },
    //];

    //$scope.PostingStatus = $scope.ddlPostingStatus[0];

    //$scope.ddlSearchBy = [
    //    { Code: 'Form_No', Name: 'Nintex No' },
    //    { Code: 'Requester_Name', Name: 'Requester' },
    //];

    //$scope.SearchBy = $scope.ddlSearchBy[0];


    //$scope.Date = {
    //    Start: DateFormat_ddMMMyyyy(new Date(new Date().setDate(1))),
    //    End: DateFormat_ddMMMyyyy(new Date())
    //};

    //$scope.CreateNewForm = function () {
    //    if ($scope.Module.Code == 'M001')
    //        location.href = 'Memo.aspx?module=' + $scope.Module.Code;
    //    else
    //        location.href = $scope.Module.Module_Url;
    //}

    ///* Pagination */
    //$scope.ItemIDs = [];
    //$scope.TaskIDs = [];
    //$scope.Items = [];
    //$scope.Keywords = '';
    //$scope.GrandTotal = 0;
    //$scope.ListData = function (PageIndex) {
    //    var TableName = $scope.Module.Table_Name;
    //    var isReviseRebate = $scope.Module.Name == 'Revise Rebate' ? true : false
    //    var FilterBy = $scope.FilterBy.Code;
    //    var listName = $scope.Module.List_Name
    //    var StartDate = $scope.Date.Start;
    //    var EndDate = $scope.Date.End;
    //    var Branch = $scope.Branch == undefined ? '' : $scope.Branch.Name;
    //    if (Branch == 'All') {
    //        Branch = '';
    //    }
    //    console.log(isReviseRebate, $scope.Module)

    //    var param = {
    //        TableName: TableName,
    //        FilterBy: FilterBy,
    //        //StartDate: StartDate.toISOString().split('T')[0],
    //        //EndDate: EndDate.toISOString().split('T')[0],
    //        StartDate: StartDate,
    //        EndDate: EndDate,
    //        PageIndex: PageIndex,
    //        SearchBy: $scope.SearchBy.Code,
    //        Keywords: $scope.Keywords,
    //        PaymentStatus: $scope.PaymentStatus.Code,
    //        PostingStatus: $scope.PostingStatus.Code,
    //        BranchName: Branch,
    //        ListName: listName,
    //        PendingApproverRole: $scope.PendingApprovalRole.Code,
    //        //CurrentLogin: 'daikin\\urohman',
    //    };


    //    if (TableName != undefined) {

    //        var proc = svc.svc_ListData(param);
    //        proc.then(function (response) {
    //            var data = JSON.parse(response.data.d);
    //            console.log(data);
    //            if (data.ProcessSuccess) {
    //                $scope.GrandTotal = data.GrandTotal;
    //                $scope.Total = 0;
    //                $scope.Items = data.Items;

    //                for (x in $scope.Items) {
    //                    $scope.Total += $scope.Items[x].Grand_Total;
    //                    for (y in $scope.Items[x]) {
    //                        if (y.endsWith('Date')) {
    //                            $scope.Items[x][y] = $scope.ConvertJSONDate($scope.Items[x][y]);
    //                        }
    //                    }
    //                }
    //                $(".Pager").ASPSnippets_Pager({
    //                    ActiveCssClass: "current",
    //                    PagerCssClass: "pager",
    //                    PageIndex: data.PageIndex,
    //                    PageSize: data.PageSize,
    //                    RecordCount: data.RecordCount
    //                });
    //            }
    //        }, function (data, status) {
    //            console.log(data);
    //            alert(data.statusText + ' - ' + data.data.Message);
    //        });
    //    }
    //}
    //$scope.Search = function () {
    //    $scope.ListData(1);
    //}

    $scope.GetModuleOptions();
    ///*End Of Pagination*/

    //console.log($scope.FilterBy, 'Filter By');
    //$("body").on("click", ".Pager .page", function () {
    //    $scope.ListData(parseInt($(this).attr('page')));
    //});

    //$scope.SearchHelper = function (keyEvent) {
    //    if (keyEvent.which === 13) {
    //        $scope.Search();
    //    }
    //};


    //setTimeout(function(){ 
    //    $scope.Search();
    //}, 3000);


});