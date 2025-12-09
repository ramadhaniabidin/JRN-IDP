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

app.service("svc", function ($http) {
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
    };

    this.svc_GetBranches = function () {
        return $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/Master.asmx/GetListBranch",
            data: {},
            dataType: "json"
        });
    };

    this.svc_ListData = function (model) {
        return $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/Master.asmx/BusinessPartner_ListData",
            data: JSON.stringify({model: model}),
            dataType: "json"
        });
    };

    this.svc_ListLog = function (Trans_ID, Module_Code, Transaction_ID) {
        const param = {
            Form_No: Trans_ID,
            Module_Code: Module_Code,
            Transaction_ID: Transaction_ID
        };
        return $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/Master.asmx/GetHistoryLog",
            data: JSON.stringify(param),
            dataType: "json"
        });
    };

});

app.controller('ctrl', function ($scope, svc) {
    $scope.showModal = 'none';
    $scope.colspan = 7;
    $scope.ddlPendingApprover = [
        { Position_ID: 0, Position_Name: "All" },
        { Position_ID: 5, Position_Name: "Direct Head" },
        { Position_ID: 100, Position_Name: "IT Approver" },
        { Position_ID: 101, Position_Name: "Finance Approver" }
    ];
    $scope.PendingApproverRole = $scope.ddlPendingApprover[0];
    $scope.ddlModule = [
        { Code: 'M029', Name: 'PAL', New_Form_Url: '/Lists/PAL/NewForm.aspx', Table_Name: 'PALHeader' },
        { Code: 'M030', Name: 'BP', New_Form_Url: '/Lists/Business Partners/NewForm.aspx', Table_Name: 'BPHeader' },
    ];
    $scope.Module = $scope.ddlModule[0];
    $scope.ddlPaymentStatus = [
        { Code: '', Name: 'All' },
        { Code: '1', Name: 'Paid' },
        { Code: '0', Name: 'Unpaid' },
    ];
    $scope.PaymentStatus = $scope.ddlPaymentStatus[0];
    $scope.ddlFilterBy = [
        { Code: 'Created_Date', Name: 'Created Date' },
        { Code: 'Approval_Date', Name: 'Approval Date' },
    ];
    $scope.FilterBy = $scope.ddlFilterBy[0];
    $scope.ddlSearchBy = [
        { Code: 'Form_No', Name: 'Nintex No' },
        { Code: 'Requester_Name', Name: 'Requester' },
    ];
    $scope.SearchBy = $scope.ddlSearchBy[0];
    $scope.ddlBranch = [];
    $scope.Branch = {};
    $scope.ddlPostingStatus = [
        { Code: '', Name: 'All' },
        { Code: '8', Name: 'Draft' },
        { Code: '1', Name: 'Submitted' },
        { Code: '5', Name: 'Revised' },
        { Code: '6', Name: 'Rejected' },
        { Code: '7', Name: 'Approved' },
        { Code: '4', Name: 'Waiting for Approval' }
    ];
    $scope.PostingStatus = $scope.ddlPostingStatus[0];
    $scope.Pending_Approver_Name = '';
    $scope.Pending_Approver_Role = '';
    $scope.Approval_Status = null;
    $scope.Approval_Status_Name = null;
    $scope.Logs = [];

    const modal = document.getElementById("appModal");
    window.onclick = function (event) {
        if (event.target == modal) {
            $scope.showModal = 'none';
        }
    };

    $scope.GetModuleURL = function (Module_Code) {
        if (Module_Code === "M025") {
            return "/Lists/PIB/NewForm.aspx";
        }
        const ur1 = "/_layouts/15/daikin.application/Commercials";
        const ur2 = Module_Code === "M011" ? "/ServiceCost.aspx" : "/FOB.aspx";
        return ur1 + ur2;
    };

    $scope.CreateNewForm = function () {
        const url = $scope.Module.New_Form_Url;
        window.open(url, "_blank");
    };

    


    

    

    

    $scope.Date = {
        Start: DateFormat_ddMMMyyyy(new Date(new Date().setDate(1))),
        End: DateFormat_ddMMMyyyy(new Date())
    };
    $scope.selected = [];



    $scope.plant = {};
    $scope.dllPlants = [];
    var init = true;
    $scope.onChangeDDLModule = function () {
        console.log('Module: ', $scope.Module);
        $scope.Items = [];
    };

    function getParameterByName(name) {
        var url = window.location.href;
        name = name.replace(/[\[\]]/g, '\\$&');
        var regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)'),
            results = regex.exec(url);
        if (!results) return null;
        if (!results[2]) return '';
        return decodeURIComponent(results[2].replace(/\+/g, ' '));
    }

    $scope.getSelectedClass = function (entity) {
        return $scope.isSelected(entity.Item_ID) ? 'selected' : '';
    };

    $scope.selectAll = function ($event) {
        var checkbox = $event.target;
        var action = (checkbox.checked ? 'add' : 'remove');
        for (var i = 0; i < $scope.Items.length; i++) {
            var entity = $scope.Items[i];
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

    var updateSelected = function (action, id) {
        if (action === 'add' && $scope.selected.indexOf(id) === -1) {
            $scope.selected.push(id);
        }
        if (action === 'remove' && $scope.selected.indexOf(id) !== -1) {
            $scope.selected.splice($scope.selected.indexOf(id), 1);
        }
    };

    $scope.updateSelection = function ($event, id) {
        var checkbox = $event.target;
        var action = (checkbox.checked ? 'add' : 'remove');
        updateSelected(action, id);
    };


    $scope.closeModal = function () {
        $scope.showModal = 'none';
    };

    $scope.ApproverLog = function (obj) {
        $scope.showModal = 'block';
        $scope.HistoryLog_UpdateApprovalInfo(obj);
        const prom = svc.svc_ListLog(obj.Form_No, $scope.Module.Code, obj.ID);
        prom.then(function (response) {
            const jsonData = JSON.parse(response.data.d);
            console.log(jsonData);
            if (jsonData.ProcessSuccess) {
                $scope.Logs = jsonData.Logs;
            } else {
                console.log(jsonData.InfoMessage);
            }
        }).catch(function (err) {
            console.log(err);
        });
    };

    $scope.HistoryLog_UpdateApprovalInfo = function (obj) {
        $scope.Module_Name = $scope.Module.Name;
        $scope.Nintex_No = obj.Form_No;
        $scope.Pending_Approver_Name = obj.Pending_Approver_Name;
        $scope.Pending_Approver_Role = obj.Pending_Approver_Role;
        $scope.Approval_Status = obj.Approval_Status;
        $scope.Approval_Status_Name = obj.Approval_Status_Name;
    };

    $scope.FindIndexAll = function (arr) {
        return arr.findIndex(item => item.Name === 'All');
    };

    var arrBranch;

    $scope.GetListBranch = function () {
        try{
            const prom = svc.svc_GetBranches();
            prom.then(function (response) {
                const jsonData = JSON.parse(response.data.d);
                if (jsonData.ProcessSuccess) {
                    $scope.ddlBranch = jsonData.ListBranch;
                    $scope.Branch = $scope.ddlBranch[0];
                } else {
                    console.log(jsonData.InfoMessage);
                }
            }).catch(function (err) {
                console.error(err);
            });
        }
        catch (err) {
            console.error(err.message);
        }
    };

    $scope.GetModuleOptions = function (module_code) {
        try {
            const proc = svc.svc_GetOptions();
            proc.then(function (response) {
                const data = JSON.parse(response.data.d);
                console.log('Module Data: ', data);
                console.log('Module URL: ', $scope.Module.Module_Url);
                //if (data.ProcessSuccess) {
                //    $scope.ddlModule = [];
                //    $scope.ddlModule = [...$scope.ddlModule, ...data.Items];
                //    $scope.Module = $scope.ddlModule[0];
                //    const filteredAR = data.listApproverRole2.filter(function (o) { return o.Module_Code == module_code });
                //    $scope.ddlPendingApprover = filteredAR;
                //    $scope.PendingApproverRole = $scope.ddlPendingApprover[0];
                //    $scope.dllPlants = data.listPlant;
                //    $scope.plant = data.listPlant[0];
                //    arrBranch = data.listBranch;
                //    if ((module_code == 'M019') || (module_code == 'M026'))
                //        $scope.ddlBranch = data.listBranchSubcon;
                //    else
                //        $scope.ddlBranch = data.listBranch;
                //    if (data.Branch == '') {
                //        $scope.Branch = data.listBranch[$scope.FindIndexAll(data.listBranch)];
                //    } else {
                //        if ((module_code == 'M019') || (module_code == 'M026')) {
                //            $scope.Branch = data.listBranchSubcon[$scope.FindIndexAll(data.listBranchSubcon)];
                //        } else {
                //            $scope.Branch = data.listBranch[$scope.FindIndexAll(data.listBranch)];
                //        }
                //    }
                //    if ((module_code == 'M019') || (module_code == 'M026')) {
                //        $scope.Module = $scope.ddlModule[0];
                //    } else {
                //        $scope.Module = $scope.ddlModule.find(o => o.Code == module_code);
                //    }
                //}
                //else {
                //    console.log(`GetModuleOptions : ${data}`);
                //    alert(data.InfoMessage);
                //}
            });

        } catch (err) {
            console.error(err.message);
        }
    };


    $scope.GetPlantOptions = function () {
        const proc = svc.svc_PlantOptions();

        proc.then(function (response) {
            var data = JSON.parse(response.data.d);

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

        var re = /\/Date\(([0-9]*)\)\//;
        var m = x.match(re);
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
    $scope.RecordCount = 0;
    $scope.ListData_GenerateParam = function (PageIndex) {
        const param = {
            TableName: $scope.Module.Table_Name,
            FilterBy: $scope.FilterBy.Code,
            StartDate: $scope.Date.Start,
            EndDate: $scope.Date.End,
            PageIndex: PageIndex,
            PageSize: 10,
            SearchBy: $scope.SearchBy.Code,
            Keywords: $scope.Keywords,
            BranchName: $scope.Branch.Name === 'All' ? '' : $scope.Branch.Name,
            ModuleId: $scope.Module.Code,
            PendingApproverRoleID: $scope.PendingApproverRole.Position_ID,
            PostingStatus: $scope.PostingStatus.Code
        };
        return param;
    };
    $scope.Item_Pagination = function (data) {
        $(".Pager").ASPSnippets_Pager({
            ActiveCssClass: "current",
            PagerCssClass: "pager",
            PageIndex: data.PageIndex,
            PageSize: data.PageSize,
            RecordCount: data.RecordCount
        });
    };
    $scope.ListData = function (PageIndex) {
        const param = $scope.ListData_GenerateParam(PageIndex);
        const prom = svc.svc_ListData(param);
        prom.then(function (response) {
            const jsonData = JSON.parse(response.data.d);
            if (jsonData.ProcessSuccess) {
                $scope.Items = jsonData.Items;
                $scope.Item_Pagination(jsonData);
            } else {
                console.log(jsonData);
            }
        }).catch(function (err) {
            console.error(err);
        });
    };


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
            $scope.GetListBranch();
        } catch (err) {
            console.error(err);
        }
    }

    $scope.loadPage();
});