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
    this.svc_GetOptions2 = function (Table, Code, Name, Extra) {
        const param = {
            Table: Table,
            Code: Code,
            Name: Name,
            FilterBy: '',
            FilterValue: '',
            Extra: Extra
        };
        const response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/ClaimReimbursement.asmx/GetOptions",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }

    this.svc_UpdateDocReceived = function (List_Name, ItemIDs) {
        const param = {
            List_Name: List_Name,
            ItemIDs: ItemIDs,
        };
        const response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/ClaimReimbursement.asmx/UpdateDocumentReceived",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;

    }

    this.svc_GetCurrentLogin = function () {
        const param = {
            WithPattern: true
        };
        const response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NonCommercials.asmx/GetCurrentLogin",
            data: JSON.stringify(param),
            dataType: "json"
        });

        return response;
    };

    this.svc_GetOptions = function () {
        const param = {
            ListName: 'Claim Reimbursement'
        };
        const response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/ClaimReimbursement.asmx/ModuleOptions",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }

    this.svc_GetPendingApprovalRole = function () {
        const response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/ClaimReimbursement.asmx/GetPendingApprovalRole",
            data: {},
            dataType: "json"
        });
        return response;
    }

    this.svc_ListData = function (model) {
        const param = {
            model: model
        };

         console.log('param svc_ListData',param);
        const response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/ClaimReimbursement.asmx/ListData",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }

    this.svc_ListLog = function (Trans_ID, Module_Code) {

        const param = {
            Form_No: Trans_ID,
            Module_Code: Module_Code,
        };

        const response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/ClaimReimbursement.asmx/GetHistoryLog",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }

    this.svc_ListLogByTransID = function (Transaction_ID, Module_Code) {
        const param = { Transaction_ID, Module_Code };
        const response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/ClaimReimbursement.asmx/GetHistoryLogByTransactionID",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }

    this.svc_GetRuleExcelButton = function () {
        const response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/ClaimReimbursement.asmx/GetRuleButtonExcel",
            data: {},
            dataType: "json"
        });
        return response;
    };
});

app.controller('ctrl', function ($scope, svc) {
    $scope.showModal = 'none';
    $scope.Pending_Approver_Name = '';
    $scope.Pending_Approver_Role = '';
    $scope.Branch = {};
    $scope.PendingApprovalRole = {};
    $scope.ddlPendingApprovalRole = [];
    $scope.RequestorDepartment = "";
    $scope.ShowExcelButton = false;

    const modal = document.getElementById("appModal");
    window.onclick = function (event) {
        if (event.target == modal) {
            $scope.showModal = 'none';
        }
    }

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

    $scope.Modules = {
        'M001': {ListName: 'Business Relation', RDLC: 'EntertainmentReport', ReportName: 'BusinessRelationReport'},
        'M002': {ListName: 'Travel Expense', RDLC: 'TravelExpenseReport', ReportName: 'TravelExpenseReport'},
        'M003': {ListName: 'Unloading Fee', RDLC: 'UnloadingFeeReport', ReportName: 'UnloadingFeeReport'},
        'M004': {ListName: 'Modern Channel', RDLC: 'ModernChannelReport', ReportName: 'ModernChannelReport'},
        'M012': {ListName: 'General Rebate', RDLC: 'GeneralRebateReport', ReportName: 'GeneralRebateReport'},
        'M013': {ListName: 'Revise Rebate', RDLC: 'ReviseRebateReport', ReportName: 'ReviseRebateReport'},
        'M021': {ListName: 'Claim Rebate', RDLC: 'ClaimRebateReport', ReportName: 'ClaimRebateReport'}
    };

    $scope.Received = function () {
        try {
            const confirmDialog = confirm('Confirm received ?');
            if (confirmDialog) {
                const proc = svc.svc_UpdateDocReceived('Claim Reimbursement', $scope.selected);
                proc.then(function (response) {
                    const data = JSON.parse(response.data.d);
                    if (data.ProcessSuccess) {
                        alert('Physical Document has been confirmed successfully!');
                    } else {
                        alert(data.InfoMessage);
                    }
                }, function (data, status) {
                    alert(data.statusText + ' - ' + data.data.Message);
                });
            }

        } catch (e) {
            alert(e.message);
        }
    }

    $scope.selected = [];

    function getParameterByName(name) {
        const url = window.location.href;
        name = name.replace(/[[\]]/g, '\\$&');
        const regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)'),
            results = regex.exec(url);
        if (!results) return null;
        if (!results[2]) return '';
        return decodeURIComponent(results[2].replace(/\+/g, ' '));
    }

    $scope.getSelectedClass = function (entity) {
        return $scope.isSelected(entity.Item_ID) ? 'selected' : '';
    };

    $scope.selectAll = function ($event) {
        const checkbox = $event.target;
        const action = (checkbox.checked ? 'add' : 'remove');
        for (let i = 0; i < $scope.Items.length; i++) {
            const entity = $scope.Items[i];
            if (entity.Current_Index_Approver == 2) {
                console.log('entity:', entity);
                updateSelected(action, entity.Item_ID);
            }
        }
    };
    $scope.isSelected = function (id) {
        return $scope.selected.indexOf(id) >= 0;
    };

    $scope.isSelectedAll = function () {
        return $scope.selected.length === $scope.Items.length;
    };

    const updateSelected = function (action, id) {
        if (action === 'add' && $scope.selected.indexOf(id) === -1) {
            $scope.selected.push(id);
            console.log(id, 'selected push id');
        }
        if (action === 'remove' && $scope.selected.indexOf(id) !== -1) {
            $scope.selected.splice($scope.selected.indexOf(id), 1);
        }
    };

    $scope.updateSelection = function ($event, id) {
        const checkbox = $event.target;
        const action = (checkbox.checked ? 'add' : 'remove');
        updateSelected(action, id);
    };


    $scope.closeModal = function () {
        $scope.showModal = 'none';
    }
    $scope.ApproverLog = function (obj) {
        $scope.showModal = 'block';
        let proc = "";
        console.log(obj.ID, 'obj ID');
        proc = svc.svc_ListLogByTransID(obj.ID, $scope.Module.Code);

        proc.then(function (response) {
            const data = JSON.parse(response.data.d);

            if (data.ProcessSuccess) {
                console.log(data);
                $scope.Logs = data.Logs;
                $scope.Module_Name = $scope.Module.Name;
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

    $scope.IsFC = false;


    $scope.Module = {};
    $scope.ddlModule = [];
    $scope.GetModuleOptions = function (x) {
        const proc = svc.svc_GetOptions();
        proc.then(function (response) {
            const data = JSON.parse(response.data.d);
            console.log(data);
            if (data.ProcessSuccess) {
                $scope.ddlModule = [...$scope.ddlModule, ...data.Items];
                const module_code = GetQueryString()['module'];
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

                if (module_code == undefined) {
                    $scope.Module = $scope.ddlModule[0];
                } else {
                    $scope.Module = $scope.ddlModule.find(o => o.Code == module_code);
                }
            }
        }, function (data, status) {
            console.log(data.statusText + ' - ' + data.data.Message);
        });
    };

    $scope.onChangeDDLPostingStatus = function () {
        if ($scope.PostingStatus.Code != 4) {
            $scope.PendingApprovalRole = {};
            $scope.MasterRoleApproverCR = [];
        }

        const proc = svc.svc_GetPendingApprovalRole();
        proc.then(function (response) {
            const data = JSON.parse(response.data.d);
            console.log(data);
            if (data.ProcessSuccess) {
                $scope.ddlPendingApprovalRole = data.MasterRoleApproverCR;
                $scope.MasterRoleApproverCR = data.MasterRoleApproverCR[0];

            }
        }, function (data, status) {
            console.log(data.statusText + ' - ' + data.data.Message);
        });
    }

    $scope.ddlFilterBy = [
        { Code: 'Created_Date', Name: 'Created Date' },
        { Code: 'Approval_Date', Name: 'Approval Date' },
        { Code: 'MIRO_Date', Name: 'MIRO Date' },
        { Code: 'Scheduled_Payment_Date', Name: 'Scheduled Payment Date' },
        { Code: 'Actual_Payment_Date', Name: 'Actual Payment Date' }
    ];

    $scope.FilterBy = $scope.ddlFilterBy[0];

    $scope.ddlPaymentStatus = [
        { Code: '', Name: 'All' },
        { Code: '1', Name: 'Paid' },
        { Code: '0', Name: 'Unpaid' },
    ];

    $scope.PaymentStatus = $scope.ddlPaymentStatus[0];

    $scope.ddlPostingStatus = [
        { Code: '', Name: 'All' },
        { Code: '1', Name: 'Submitted' },
        { Code: '4', Name: 'Pending Approval' },
        { Code: '1', Name: 'Posted' },
        { Code: '0', Name: 'Pending SAP Post' },
        { Code: '5', Name: 'Revised' },
        { Code: '6', Name: 'Rejected' },
        { Code: '7', Name: 'Approved' },
        { Code: '8', Name: 'Draft' },
    ];

    $scope.PostingStatus = $scope.ddlPostingStatus[0];

    $scope.ddlSearchBy = [
        { Code: 'Form_No', Name: 'Nintex No' },
        { Code: 'Requester_Name', Name: 'Requester' },
        { Code: 'Department', Name: 'Department' },
    ];

    $scope.SearchBy = $scope.ddlSearchBy[0];


    $scope.Date = {
        Start: DateFormat_ddMMMyyyy(new Date(new Date().setDate(1))),
        End: DateFormat_ddMMMyyyy(new Date())
    };

    $scope.CreateNewForm = function () {
        if ($scope.Module.Code == 'M001')
            location.href = 'Memo.aspx?module=' + $scope.Module.Code;
        else 
            location.href = $scope.Module.Module_Url;
    }

    $scope.GenerateExcelParam = function(currLogin){
        const Branch = ($scope.Branch.Name === undefined || $scope.Branch.Name === 'All') ? '' : $scope.Branch.Name;
        const param = {
            TableName: $scope.Module.Table_Name,
            FilterBy: $scope.FilterBy.Code,
            StartDate: $scope.Date.Start,
            EndDate: $scope.Date.End,
            PageIndex: 1,
            PageSize: 1000,
            SearchBy: $scope.SearchBy.Code,
            Keywords: $scope.Keywords,
            PaymentStatus: $scope.PaymentStatus.Code,
            PostingStatus: $scope.PostingStatus.Code,
            BranchName: Branch,
            ListName: $scope.Modules[$scope.Module.Code]["ListName"],
            PendingApproverRole: '',
            CurrentLogin: (currLogin === undefined || currLogin === null) ? '' : currLogin
        };
        const entriesString = Object.entries(param)
          .map(([key, value]) => `${key}=${value}`)
          .join('&');
        return entriesString;
    };

    $scope.ExportListToExcel = function () {
        const rdlc = $scope.Modules[$scope.Module.Code]["RDLC"];
        const reportName = $scope.Modules[$scope.Module.Code]["ReportName"]
        const dataSet = 'UNDataset';        
        const prom = svc.svc_GetCurrentLogin();
        prom.then(function (response) {
            const data = JSON.parse(response.data.d);
            if (data.Success) {
                const currLogin = data.CurreLogin;
                const entriesString = $scope.GenerateExcelParam(currLogin);                
                const url = "https://sp3.daikin.co.id:8443/_layouts/15/daikin.webapps/Report_SPDEV.aspx"
                    + "?SP=usp_ClaimReimbursement_ListData"
                    +"&RDLC=" + rdlc
                    +"&DataSet=" + dataSet
                    +"&export=EXCELOPENXML"
                    +"&download=true&ReportName=" + reportName + "&" + entriesString;
                console.log(url);
                location.href = url;
            } else {
                console.log(data);
            }
        }).catch(function (err) {

        });


    };

    /* Pagination */
    $scope.ItemIDs = [];
    $scope.TaskIDs = [];
    $scope.Items = [];
    $scope.Keywords = '';
    $scope.GrandTotal = 0;
    $scope.ListData = function (PageIndex) {
        let TableName = $scope.Module.Table_Name;
        let isReviseRebate = $scope.Module.Name == 'Revise Rebate' ? true : false
        let FilterBy = $scope.FilterBy.Code;
        let listName = $scope.Module.List_Name
        let StartDate = $scope.Date.Start;
        let EndDate = $scope.Date.End;
        let Branch = $scope.Branch == undefined ? '' : $scope.Branch.Name;
        let RequestorDepartment = $scope.RequestorDepartment == undefined ? '' : $scope.RequestorDepartment;
        if (Branch == 'All') {
            Branch = '';
        }
        console.log(isReviseRebate, $scope.Module)

        let param = {
            TableName: TableName,
            FilterBy: FilterBy,
            StartDate: StartDate,
            EndDate: EndDate,
            PageIndex: PageIndex,
            SearchBy: $scope.SearchBy.Code,
            Keywords: $scope.Keywords,
            PaymentStatus: $scope.PaymentStatus.Code,
            PostingStatus: $scope.PostingStatus.Code,
            BranchName: Branch,
            ListName: listName,
            PendingApproverRole: $scope.PendingApprovalRole.Code,
        }; 
         

        if (TableName != undefined) {

            let proc = svc.svc_ListData(param);
            proc.then(function (response) {
                let data = JSON.parse(response.data.d);
                console.log(data);
                if (data.ProcessSuccess) {
                    $scope.GrandTotal = data.GrandTotal;
                    $scope.Total = 0;
                    $scope.Items = data.Items;

                    for (let x in $scope.Items) {
                        $scope.Total += $scope.Items[x].Grand_Total;
                        for (let y in $scope.Items[x]) {
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
                }
            }, function (data, status) {
                console.log(data);
                alert(data.statusText + ' - ' + data.data.Message);
            });
        }
    }
    $scope.Search = function () {
        $scope.ListData(1);
    }

    /*End Of Pagination*/

    $scope.GetRuleExcelButton = function () {
        const prom = svc.svc_GetRuleExcelButton();
        prom.then(function (response) {
            const data = JSON.parse(response.data.d);
            if (data.Success) {
                $scope.ShowExcelButton = data.Show;
            } else {
                console.log(data);
            }
        }).catch(function (err) {
            console.log(err);
        });
    };

    $scope.GetModuleOptions($scope.ListData(1));
    $scope.GetRuleExcelButton();

    console.log($scope.FilterBy, 'Filter By');
    $("body").on("click", ".Pager .page", function () {
        $scope.ListData(parseInt($(this).attr('page')));
    });

    $scope.SearchHelper = function (keyEvent) {
        if (keyEvent.which === 13) {
            $scope.Search();
        }
    };

});