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
    this.svc_GetOptions = function (ListName) {
        const param = {
            ListName: ListName
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
        const response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NACWebService.asmx/ListData",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }
    this.svc_ListDataNonCommercials = function (model) {
        const param = {
            model: model
        };
        const response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NACWebService.asmx/ListDataNonCommercials",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }
    this.svc_ListDataCommercials = function (model) {
        const param = {
            model: model
        };
        const response = $http({
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
    $scope.ddlModuleCategory = [{ Name: 'Claim Reimbursement' }, { Name: 'Non Commercials' }, { Name: 'Commercials' }];
    $scope.ModuleCategory = $scope.ddlModuleCategory[0];
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
    $scope.tableHead = [];



    $scope.ConvertJSONDate = function (x) {
        if (x == null) return x;
        const re = /\/Date\(([0-9]*)\)\//;
        const m = x.match(re);
        return m ? new Date(Number.parseInt(m[1])) : null;
    };
    $scope.onChangeModuleCategory = function () {
        $scope.Items = [];
        $scope.GetModuleOptions();
    };
    $scope.GetModuleOptions = function (x) {
        const proc = svc.svc_GetOptions($scope.ModuleCategory.Name);
        proc.then(function (response) {
            const data = JSON.parse(response.data.d);
            if (!data.ProcessSuccess) return;
            const module_code = GetQueryString()['module'];
            const newModule = { Name: 'All', List_Name: 'All' };
            $scope.ddlModule = $scope.ModuleCategory.Name === "Claim Reimbursement" ? [newModule, ...data.Items] : [...data.Items];
            $scope.ddlBranch = data.listBranch;
            $scope.Branch = (!data.Branch) ? data.listBranch[0] : data.listBranch.find(o => o.Name === data.Branch);
            $scope.Module = (!module_code) ? $scope.ddlModule[0] : $scope.ddlModule.find(o => o.Code === module_code);
        }, function (data, status) {
            console.log(data.statusText + ' - ' + data.data.Message);
        });
    };


    const tableHeadMapping = {
        "M014": [
            { Name: 'Direct Head/Head Of Department' },
            { Name: 'PIC Contract/ Head of Departement MKT' },
            { Name: 'PIC Contract MKT' },
        ],
        "M016": [
            { Name: 'Direct Head' },
            { Name: 'Head of Departement' },
            { Name: 'GM Planning' },
        ],
        "M017": [
            { Name: 'Head Of Branch' },
            { Name: 'Direct Head' },
            { Name: 'Head of Departement' },
            { Name: 'AGM / GM' },
            { Name: 'CFO' },
            { Name: 'CEO' }
        ],
        "M018": [
            { Name: 'Accounting Manager' },
            { Name: 'Receiver Document' },
            { Name: 'Verifier Document' },
            { Name: 'Finance Verifier 2' },
            { Name: 'Tax Verification' },
            { Name: 'Pending SAP Post' }
        ],
        "M019": [
            { Name: 'Head Of Service' },
            { Name: 'Field Service Manager' },
            { Name: 'Finance Verifier - Subcon 1' },
            { Name: 'Tax Verifier - Subcon' },
            { Name: 'Manager Service Operation' },
            { Name: 'Assistant GM Service / Svc. Planning Manager' },
            { Name: 'Finance Verifier - Subcon 2' },
            { Name: 'Finance Receiver - Subcon' },
            { Name: 'Waiting For MIRO' }
        ],
        "M010": [
            { Name: 'Direct Head' },
            { Name: 'AGM / GM Planning / Head Of Branch' },
            { Name: 'Receiver Document' },
            { Name: 'Verifier Document' },
            { Name: 'Verifier Document 2' },
            { Name: 'Tax Verification' },
            { Name: 'Pending SAP Post' }
        ],
        "M011": [
            { Name: 'Head of Budget Owner' },
            { Name: 'AGM / GM Planning / Head Of Branch' },
            { Name: 'Receiver Document' },
            { Name: 'Finance Verification' },
            { Name: 'Finance Verification 2' },
            { Name: 'Tax Verification' },
            { Name: 'Pending SAP Post' }
        ],
        "M020": [
            { Name: 'Direct Head' },
            { Name: 'Head Of Marketing' }
        ],
        "M025": [
            { Name: 'Head of Budget Owner' },
            { Name: 'Head of Department' },
            { Name: 'GM Planning' },
            { Name: 'Receiver Document' },
            { Name: 'Finance Verification' },
            { Name: 'Finance Senior Manager' },
            { Name: 'Payment Confirmation' },
            { Name: 'Waiting For MIRO' }
        ]
    };


    $scope.OnChangeModule = function () {
        $scope.Items = [];
        if ($scope.ModuleCategory.Name == 'Claim Reimbursement') {
            $scope.tableHead = [
                { Name: 'Direct Head' },
                { Name: 'Receiver Document' },
                { Name: 'Verifier Document' },
                { Name: 'Finance Verifier 2' },
                { Name: 'Pending SAP Post' },
            ]
        } else {
            const key = $scope.Module.Code === "M026" ? "M019" : $scope.Module.Code;
            $scope.tableHead = tableHeadMapping[key];
            // if ($scope.Module.Code == 'M014') {
            //     $scope.tableHead = [
            //         { Name: 'Direct Head/Head Of Department' },
            //         { Name: 'PIC Contract/ Head of Departement MKT' },
            //         { Name: 'PIC Contract MKT' },
            //     ]
            // } else if ($scope.Module.Code == 'M016') {
            //     $scope.tableHead = [
            //         { Name: 'Direct Head' },
            //         { Name: 'Head of Departement' },
            //         { Name: 'GM Planning' },
            //     ]
            // } else if ($scope.Module.Code == 'M017') {
            //     $scope.tableHead = [
            //         { Name: 'Head Of Branch' },
            //         { Name: 'Direct Head' },
            //         { Name: 'Head of Departement' },
            //         { Name: 'AGM / GM' },
            //         { Name: 'CFO' },
            //         { Name: 'CEO' }
            //     ]
            // } else if ($scope.Module.Code == 'M018') {
            //     $scope.tableHead = [
            //         { Name: 'Accounting Manager' },
            //         { Name: 'Receiver Document' },
            //         { Name: 'Verifier Document' },
            //         { Name: 'Finance Verifier 2' },
            //         { Name: 'Tax Verification' },
            //         { Name: 'Pending SAP Post' }
            //     ]
            // } else if ($scope.Module.Code == 'M019' || $scope.Module.Code == 'M026') {
            //     $scope.tableHead = [
            //         { Name: 'Head Of Service' },
            //         { Name: 'Field Service Manager' },
            //         { Name: 'Finance Verifier - Subcon 1' },
            //         { Name: 'Tax Verifier - Subcon' },
            //         { Name: 'Manager Service Operation' },
            //         { Name: 'Assistant GM Service / Svc. Planning Manager' },
            //         { Name: 'Finance Verifier - Subcon 2' },
            //         { Name: 'Finance Receiver - Subcon' },
            //         { Name: 'Waiting For MIRO' }
            //     ]
            // } else if ($scope.Module.Code == 'M010') {
            //     $scope.tableHead = [
            //         { Name: 'Direct Head' },
            //         { Name: 'AGM / GM Planning / Head Of Branch' },
            //         { Name: 'Receiver Document' },
            //         { Name: 'Verifier Document' },
            //         { Name: 'Verifier Document 2' },
            //         { Name: 'Tax Verification' },
            //         { Name: 'Pending SAP Post' }
            //     ]
            // } else if ($scope.Module.Code == 'M011') {
            //     $scope.tableHead = [
            //         { Name: 'Head of Budget Owner' },
            //         { Name: 'AGM / GM Planning / Head Of Branch' },
            //         { Name: 'Receiver Document' },
            //         { Name: 'Finance Verification' },
            //         { Name: 'Finance Verification 2' },
            //         { Name: 'Tax Verification' },
            //         { Name: 'Pending SAP Post' }
            //     ]
            // } else if ($scope.Module.Code == 'M020') {
            //     $scope.tableHead = [
            //         { Name: 'Direct Head' },
            //         { Name: 'Head Of Marketing' }
            //     ]
            // } else if ($scope.Module.Code == 'M025') {
            //     $scope.tableHead = [
            //         { Name: 'Head of Budget Owner' },
            //         { Name: 'Head of Department' },
            //         { Name: 'GM Planning' },
            //         { Name: 'Receiver Document' },
            //         { Name: 'Finance Verification' },
            //         { Name: 'Finance Senior Manager' },
            //         { Name: 'Payment Confirmation' },
            //         { Name: 'Waiting For MIRO' }
            //     ]
            // }
        }
    }

    $scope.Date = {
        Start: DateFormat_ddMMMyyyy(new Date(new Date().setDate(1))),
        End: DateFormat_ddMMMyyyy(new Date())
    };

    $scope.ListData = function () {
        const ModuleCategory = $scope.ModuleCategory.Name;
        let Module = $scope.Module.Name;
        const StartDate = $scope.Date.Start;
        const EndDate = $scope.Date.End;
        let Branch = $scope.Branch == undefined ? '' : $scope.Branch.Name;
        if (Branch == 'All') {
            Branch = '';
        }
        if (Module == 'All') {
            Module = '';
        }
        const param = {
            ModuleCategory: ModuleCategory,
            Module: Module,
            Branch: Branch,
            StartDate: StartDate,
            EndDate: EndDate
        };


        if (ModuleCategory != undefined) {
            //const proc = svc.svc_ListData(param);
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
    $scope.ListDataNonCommercials = function () {
        const ModuleCategory = $scope.ModuleCategory.Name;
        let Module = $scope.Module.Name
        const StartDate = $scope.Date.Start;
        const EndDate = $scope.Date.End;
        let Branch = $scope.Branch == undefined ? '' : $scope.Branch.Name;
        let ProcDept = $scope.Department.Name
        if (Branch == 'All') {
            Branch = '';
        }
        if (Module == 'All') {
            Module = '';
        }
        if (ProcDept == 'All') {
            ProcDept = '';
        }
        const param = {
            ModuleCategory: ModuleCategory,
            Module: Module,
            Branch: Branch,
            StartDate: StartDate,
            EndDate: EndDate,
            ProcDept: ProcDept
        };


        if (ModuleCategory != undefined) {
            //const proc = svc.svc_ListDataNonCommercials(param);
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
        const ModuleCategory = $scope.ModuleCategory.Name;
        let Module = $scope.Module.Name
        const StartDate = $scope.Date.Start;
        const EndDate = $scope.Date.End;
        let Branch = $scope.Branch == undefined ? '' : $scope.Branch.Name;
        if (Branch == 'All') {
            Branch = '';
        }
        if (Module == 'All') {
            Module = '';
        }
        const param = {
            ModuleCategory: ModuleCategory,
            Module: Module,
            Branch: Branch,
            StartDate: StartDate,
            EndDate: EndDate
        };


        if (ModuleCategory != undefined) {
            //const proc = svc.svc_ListDataCommercials(param);
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

    $scope.GetTableMap = function () {
        const tableMap = {
            "Claim Reimbursement": {
                "*": "tblReportsClaimReimbursement"
            },

            "Non Commercials": {
                "M016": "tblReportsPROrContract",
                "M014": "tblReportsPROrContract",
                "M017": "tblReportsQCForServistCostorFOB",
                "M018": "tblReportsPORelease",
                "M020": "tblReportsPOContract"
            },

            "Commercials": {
                "M011": "tblReportsQCForServistCostorFOB",
                "M010": "tblReportsQCForServistCostorFOB",
                "M019": "tblReportsPOSubcon",
                "M025": "tblReportsPIB"
            }
        };
        return tableMap;
    };

    $scope.Export = () => {
        const category = $scope.ModuleCategory.Name;
        const code = $scope.Module.Code;
        const tableMap = $scope.GetTableMap();

        const mapping = tableMap[category];
        if (!mapping) return;

        const tableID = mapping[code] || mapping["*"];
        if (!tableID) return;

        $("#" + tableID).table2excel({ filename: "ReportTables.xls" });
    };

    $scope.GetModuleOptions();
    ///*End Of Pagination*/
});