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
    this.svc_ListLog = function (Form_No, Module_Code, Trans_ID) {
        const param = {
            Form_No: Form_No,
            Module_Code: Module_Code,
            Transaction_ID: Trans_ID
        };
        console.log("Param history log: ", param);

        const response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/Master.asmx/GetHistoryLog",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }

    this.svc_POSubconListData = function (model) {
        const param = {
            model: model
        }
        console.log('subcon', param);
        const response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/Commercials.asmx/POSubconListData",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;

    }

    this.svc_PIBListData = function (model) {
        const param = {
            model: model
        }
        console.log('pib', param);
        const response = $http({
            method: "post",
            url: "/_layouts/15/daikin.application/WebServices/Commercials.asmx/PIBListData",
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
            url: "/_layouts/15/Daikin.application/WebServices/Master.asmx/ListData",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }

    this.svc_LoadApproverRoles = function (ListName) {
        const param = {
            ListName: ListName
        }

        const response = $http({
            method: "post",
            url: "/_layouts/15/daikin.application/WebServices/Commercials.asmx/LoadApproverRoles",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;

    }

    this.svc_GetOptions = function () {
        const param = {
            ListName: 'Commercials'
        };
        const response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/Master.asmx/ModuleOptions",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }

    this.svc_PlantOptions = function () {
        const param = {};
        const response = $http({
            method: "post",
            url: "/_layouts/15/daikin.application/WebServices/Master.asmx/PlantOptions",
            data: {},
            dataType: "json"
        });
        return response;
    }


});

app.controller('ctrl', function ($scope, svc) {
    $scope.showModal = 'none';
    $scope.colspan = 7;

    const modal = document.getElementById("appModal");
    window.onclick = function (event) {
        if (event.target == modal) {
            $scope.showModal = 'none';
        }
    }

    $scope.GetModuleURL = function (Module_Code) {
        if (Module_Code === "M025") {
            return "/Lists/PIB/NewForm.aspx";
        }
        const ur1 = "/_layouts/15/daikin.application/Commercials";
        const ur2 = Module_Code === "M011" ? "/ServiceCost.aspx" : "/FOB.aspx";
        return ur1 + ur2;
    };

    $scope.CreateNewForm = function () {
        const url = $scope.GetModuleURL($scope.Module.Code);
        window.open(url, "_blank");
    };

    $scope.ddlFilterBy = [
        //{ Code: "", Name: "Please Select" },
        { Code: 'Created_Date', Name: 'Created Date' },
        { Code: 'Approval_Date', Name: 'Approval Date' },
        { Code: 'MIRO_Date', Name: 'MIRO Date' },
        { Code: 'Scheduled_Payment_Date', Name: 'Scheduled Payment Date' },
        { Code: 'Actual_Payment_Date', Name: 'Actual Payment Date' },
        { Code: 'Document_Date', Name: 'Document Date PO' }
    ];
    $scope.FilterBy = $scope.ddlFilterBy[0];


    $scope.ddlPaymentStatus = [
        //{ Code: "", Name: "Please Select" },
        { Code: '', Name: 'All' },
        { Code: '1', Name: 'Paid' },
        { Code: '0', Name: 'Unpaid' },
    ];

    $scope.PaymentStatus = $scope.ddlPaymentStatus[0];

    $scope.ddlPostingStatus = [];

    $scope.ddlSearchBy = [
        { Code: 'Form_No', Name: 'Nintex No' },
        { Code: 'Requester_Name', Name: 'Requester' },
        { Code: 'Vendor_Name', Name: 'Vendor' },
    ];
    $scope.SearchBy = $scope.ddlSearchBy[0];

    $scope.Date = {
        Start: DateFormat_ddMMMyyyy(new Date(new Date().setDate(1))),
        End: DateFormat_ddMMMyyyy(new Date())
    };
    $scope.selected = [];

    $scope.ddlPendingApprover = [];
    $scope.PendingApproverRole = {};

    $scope.plant = {};
    $scope.dllPlants = [];
    let init = true;
    $scope.onChangeDDLModule = function () {
        $scope.ddlPendingApprover = [];
        $scope.PendingApproverRole = {};

        try {
            if ($scope.Module.Code == 'M010') { //FOB
                $scope.colspan = 7;
            } else {
                $scope.colspan = 5;
            }

            if ($scope.Module.Code == 'M025') {
                $scope.ddlPostingStatus = [
                     //{ Code: "", Name: "Please Select" },
                     { Code: '', Name: 'All' },
                     { Code: '8', Name: 'Draft' },
                     { Code: '1', Name: 'Submitted' },
                     { Code: '5', Name: 'Revised' },
                     { Code: '6', Name: 'Rejected' },
                     { Code: '7', Name: 'Approved' },
                     { Code: '4', Name: 'Waiting for Approval' },
                     { Code: '14', Name: 'Waiting for MIRO' },
                     { Code: '17', Name: 'Waiting for PEN' },
                     { Code: '18', Name: 'Completed' }

                ];

                $scope.ddlSearchBy = [
                    { Code: 'Form_No', Name: 'Nintex No' },
                    { Code: 'Requester_Name', Name: 'Requester' },
                    { Code: 'PIB_Number', Name: 'PIB Number' },
                    { Code: 'PEN_Number', Name: 'PEN Number' },
                    { Code: 'Remarks', Name: 'Remarks' },
                ];
                $scope.SearchBy = $scope.ddlSearchBy[0];

                $scope.ddlFilterBy = [
                //{ Code: "", Name: "Please Select" },
                { Code: 'Created_Time', Name: 'Created Date' },
                ];
                $scope.FilterBy = $scope.ddlFilterBy[0];

            }
            else if ($scope.Module.Code !== 'M019') {
                $scope.ddlPostingStatus = [
                    //{ Code: "", Name: "Please Select" },
                    { Code: '', Name: 'All' },
                    { Code: '1', Name: 'Posted' },
                    { Code: '0', Name: 'Pending SAP Post' },
                    { Code: '5', Name: 'Revised' },
                    { Code: '6', Name: 'Rejected' },
                    { Code: '7', Name: 'Approved' },
                    { Code: '8', Name: 'Draft' },
                ];
            } else {
                $scope.ddlPostingStatus = [
                    { Code: '', Name: 'All' },
                    { Code: '3', Name: 'Generated' },
                    { Code: '4', Name: 'Pending for Approval' },
                    { Code: '11', Name: 'Pending for Submit Document' },
                    { Code: '12', Name: 'Waiting for Feedback Release' },
                    { Code: '13', Name: 'Waiting for Feedback MIGO' },
                    { Code: '14', Name: 'Waiting for Feedback MIRO' },
                    { Code: '5', Name: 'Revised' },
                    { Code: '6', Name: 'Rejected' },
                    { Code: '7', Name: 'Approved' },
                    { Code: '15', Name: 'Pending for Closing' },
                ];

            }
            $scope.PostingStatus = $scope.ddlPostingStatus[0];


            if (!init) $scope.GetModuleOptions($scope.Module.Code)

        } catch (e) {
            console.log(e.message);
        }
    }

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
        const item_id = obj.Module_Code == "M026" ? obj.Item_ID : obj.ID;
        const proc = svc.svc_ListLog(obj.Form_No, $scope.Module.Code, item_id);
        console.log(obj);
        proc.then(function (response) {
            const data = JSON.parse(response.data.d);

            if (data.ProcessSuccess) {
                $scope.Logs = data.Logs;
                $scope.Module_Name = $scope.Module.Name;
                $scope.Nintex_No = obj.Form_No;
                $scope.Pending_Approver_Name = obj.Pending_Approver_Name;
                $scope.Pending_Approver_Role = obj.Pending_Approver_Role;
                $scope.Approval_Status = obj.Approval_Status;
                $scope.Approval_Status_Name = obj.Approval_Status_Name;
            } else {
                console.log(`ApproverLog : ${data}`);
                alert(data.InfoMessage);
            }
        }, function (data, status) {
            console.log(data.statusText + ' - ' + data.data.Message);
        });

    }

    $scope.FindIndexAll = function (arr) {
        return arr.findIndex(item => item.Name === 'All');
    };

    let arrBranch;

    function bindBranchField(arr){
        return arr[$scope.FindIndexAll(arr)];
    };

    $scope.Module = {};
    $scope.ddlModule = [];
    $scope.GetModuleOptions = function (module_code) {
        try {
            const isSubcon = (module_code === "M019" || module_code === "M026");
            const proc = svc.svc_GetOptions();
            proc.then(function (response) {
                const data = JSON.parse(response.data.d);
                console.log('Module Data: ', data);
                console.log('Module URL: ', $scope.Module.Module_Url);
                if (data.ProcessSuccess) {
                    $scope.ddlModule = [];
                    $scope.ddlModule = [...$scope.ddlModule, ...data.Items];
                    $scope.Module = $scope.ddlModule[0];
                    const filteredAR = data.listApproverRole2.filter(function (o) { return o.Module_Code == module_code });
                    $scope.ddlPendingApprover = filteredAR;
                    $scope.PendingApproverRole = $scope.ddlPendingApprover[0];
                    $scope.dllPlants = data.listPlant;
                    $scope.plant = data.listPlant[0];
                    arrBranch = data.listBranch;
                    $scope.ddlBranch = isSubcon ? data.listBranchSubcon : data.listBranch;
                    $scope.Module = isSubcon ? $scope.ddlModule[0] : $scope.ddlModule.find(md => md.Code === module_code);
                    $scope.Branch = isSubcon ? bindBranchField(data.listBranchSubcon) : bindBranchField(data.listBranch);

                    // if ((module_code == 'M019') || (module_code == 'M026'))
                    //     $scope.ddlBranch = data.listBranchSubcon;
                    // else
                    //     $scope.ddlBranch = data.listBranch;
                    // if (data.Branch == '') {
                    //     $scope.Branch = data.listBranch[$scope.FindIndexAll(data.listBranch)];
                    // } else {
                    //     if ((module_code == 'M019') || (module_code == 'M026')) {
                    //         $scope.Branch = data.listBranchSubcon[$scope.FindIndexAll(data.listBranchSubcon)];
                    //     } else {
                    //         $scope.Branch = data.listBranch[$scope.FindIndexAll(data.listBranch)];
                    //     }
                    // }
                    // if ((module_code == 'M019') || (module_code == 'M026')) {
                    //     $scope.Module = $scope.ddlModule[0];
                    // } else {
                    //     $scope.Module = $scope.ddlModule.find(o => o.Code == module_code);
                    // }
                }
                else {
                    console.log(`GetModuleOptions : ${data}`);
                    alert(data.InfoMessage);
                }
            });

        } catch (err) {
            console.error(err.message);
        }
    };


    $scope.GetPlantOptions = function () {
        const proc = svc.svc_PlantOptions();

        proc.then(function (response) {
            const data = JSON.parse(response.data.d);

            if (data.ProcessSuccess) {
                $scope.dllPlants = data.listPlant;
                $scope.plant = data.listPlant[0];

            } else {
                console.log(`GetPlantOptions : ${data}`);
                alert(data.InfoMessage);
            }
        }, function (data) {
            console.error(data.statusText + ' - ' + data.data.Message);
        });
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

    /* Pagination */
    $scope.ItemIDs = [];
    $scope.TaskIDs = [];
    $scope.Items = [];
    $scope.Keywords = '';
    $scope.GrandTotal = 0;
    $scope.ListData = function (PageIndex) {
        const TableName = $scope.Module.Table_Name;
        const FilterBy = $scope.FilterBy.Code;
        const StartDate = $scope.Date.Start;
        const EndDate = $scope.Date.End;
        let Branch = $scope.Branch.Name;
        if (Branch == 'All') {
            Branch = '';
        }
        const param = {
            TableName: TableName,
            FilterBy: FilterBy,
            Plant_Code: $scope.plant.Code,
            StartDate: StartDate,
            EndDate: EndDate,
            PageIndex: PageIndex,
            SearchBy: $scope.SearchBy.Code,
            Keywords: $scope.Keywords,
            PaymentStatus: $scope.PaymentStatus.Code,
            PostingStatus: $scope.PostingStatus.Code,
            BranchName: Branch,
            ModuleId: $scope.Module.Code,
            PendingApproverRoleID: $scope.PendingApproverRole.Position_ID == undefined ? 999 : $scope.PendingApproverRole.Position_ID,
        };
        // console.log(param);
        if ($scope.Module.Code == 'M010' || $scope.Module.Code == 'M011') { //FOB or LC
            if (TableName != undefined) {
                const proc = svc.svc_ListData(param);
                proc.then(function (response) {
                    const data = JSON.parse(response.data.d);
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
                    else {
                        console.log(`ListData : ${data}`);
                        alert(data.InfoMessage);
                    }
                }, function (data, status) {
                    console.log(data);
                    alert(data.statusText + ' - ' + data.data.Message);
                });

            }
        }
        else if ($scope.Module.Code == 'M025') { //PIB
            param.PendingApproverRoleID = $scope.PendingApproverRole.Position_ID;

            const proc = svc.svc_PIBListData(param);
            proc.then(function (response) {
                const data = JSON.parse(response.data.d);
                console.log(data);

                if (data.ProcessSuccess) {
                    $scope.GrandTotal = data.Total;
                    $scope.Total = 0;
                    $scope.ItemsPIB = data.Items;

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
        else {
            param.PendingApproverRoleID = $scope.PendingApproverRole.Position_ID;

            const proc = svc.svc_POSubconListData(param);
            proc.then(function (response) {
                const data = JSON.parse(response.data.d);
                console.log(data);
                if (data.ProcessSuccess) {
                    $scope.GrandTotal = data.GrandTotal;
                    $scope.Total = 0;
                    $scope.ItemsSubcon = data.Items;

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


    $("body").on("click", ".Pager .page", function () {
        $scope.ListData(parseInt($(this).attr('page')));
    });

    $scope.SearchHelper = function (keyEvent) {
        if (keyEvent.which === 13) {
            $scope.Search();
        }
    };


    $scope.loadPage = function () {
        try {
            $scope.GetModuleOptions('M026');
            $scope.onChangeDDLModule();
            if (init) {
                $scope.ddlPostingStatus = [
                    { Code: '', Name: 'All' },
                    { Code: '3', Name: 'Generated' },
                    { Code: '4', Name: 'Pending for Approval' },
                    { Code: '11', Name: 'Pending for Submit Document' },
                    { Code: '12', Name: 'Waiting for Feedback Release' },
                    { Code: '13', Name: 'Waiting for Feedback MIGO' },
                    { Code: '14', Name: 'Waiting for Feedback MIRO' },
                    { Code: '5', Name: 'Revised' },
                    { Code: '6', Name: 'Rejected' },
                    { Code: '7', Name: 'Approved' },
                    { Code: '15', Name: 'Pending for Closing' },
                ];
                $scope.PostingStatus = $scope.ddlPostingStatus[0];
            }
            init = false;

        } catch (err) {
            console.error(err);
        }
    }

    $scope.loadPage();
});