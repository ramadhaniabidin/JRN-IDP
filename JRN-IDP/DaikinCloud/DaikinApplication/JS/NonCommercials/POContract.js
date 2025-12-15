var app = angular.module("app", []);

app.directive("button", () => {
    return {
        restrict: "E",
        link: function (scope, elem, attrs) {
            if (attrs.ngClick || attrs.href === "" || attrs.href === "#") {
                elem.on("click", function (e) {
                    e.preventDefault();
                });
            }
        },
    };
});

app.directive("datepicker", () => {
    return {
        restrict: "A",
        require: "ngModel",
        link: function (scope, elem, attrs, ngModelCtrl) {
            var updateModel = function (dateText) {
                scope.$apply(() => {
                    ngModelCtrl.$setViewValue(dateText);
                });
            };
            var options = {
                showButtonPanel: true,
                changeMonth: true,
                changeYear: true,
                dateFormat: "d-M-yy",
                showOtherMonths: true,
                selectOtherMonths: true,
                onSelect: function (dateText) {
                    updateModel(dateText);
                },
            };
            elem.datepicker(options);
        },
    };
});

app.directive("monthyears", () => {
    return {
        restrict: "A",
        require: "ngModel",
        link: function (scope, elem, attrs, ngModelCtrl) {
            var updateModel = function (dateText) {
                scope.$apply(() => {
                    ngModelCtrl.$setViewValue(dateText);
                });
            };
            var options = {
                changeMonth: true,
                changeYear: true,
                //showButtonPanel: true,
                dateFormat: "M yy",
                onClose: function (dateText, inst) {
                    $(this).datepicker(
                        "setDate",
                        new Date(inst.selectedYear, inst.selectedMonth, 1)
                    );
                },
                onSelect: function (dateText) {
                    updateModel(dateText);
                },
            };
            elem.datepicker(options);
        },
    };
});

app.directive("loading", [
    "$http",
    function ($http) {
        return {
            restrict: "A",
            link: function (scope, elm, attrs) {
                scope.isLoading = () => {
                    return $http.pendingRequests.length > 0;
                };

                scope.$watch(scope.isLoading, function (v) {
                    if (v) {
                        elm.show();
                    } else {
                        elm.hide();
                    }
                });
            },
        };
    },
]);

app.directive("format", [
    "$filter",
    function ($filter) {
        return {
            require: "?ngModel",
            link: function (scope, elem, attrs, ctrl) {
                if (!ctrl) return;

                ctrl.$formatters.unshift(function (a) {
                    return $filter(attrs.format)(ctrl.$modelValue);
                });

                elem.bind("focus", (event) => {
                    // return elem.val('')
                });

                ctrl.$parsers.unshift(function (viewValue) {
                    var plainNumber = viewValue.replace(/[^\d|\-+|\.+]/g, "");
                    elem.val($filter(attrs.format)(plainNumber));
                    return plainNumber;
                });
            },
        };
    },
]);

app.filter("FormatDate", () => {
    var re = /\/Date\(([0-9]*)\)\//;
    return function (x) {
        var m = x.match(re);
        if (m) return new Date(parseInt(m[1]));
        else return null;
    };
});

app.service("svc", function ($http) {
    this.svc_ListLog = function (Form_No) {
        const param = {
            Form_No: Form_No,
            Module_Code: "M020",
            Transaction_ID: 0
        };

        const response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/Master.asmx/GetHistoryLog",
            data: JSON.stringify(param),
            dataType: "json",
        });
        return response;
    };

    this.svc_POWithContractGetDataById = function (ID) {
        const param = {
            Form_No: ID,
        };
        const response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NonCommercials.asmx/POWithContractGetDataById",
            data: JSON.stringify(param),
            dataType: "json",
        });
        return response;
    };

    this.svc_POWithContractGetData = () => {
        const response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NonCommercials.asmx/POWithContractGetData",
            data: {},
            dataType: "json",
        });
        return response;
    };

    this.svc_POWithContractGetVendor = (ProcurementDepartment) => {
        const param = {
            ProcurementDepartment: ProcurementDepartment,
        };
        const response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NonCommercials.asmx/POWithContractGetVendor",
            data: JSON.stringify(param),
            dataType: "json",
        });
        return response;
    };

    this.svc_POWithContractGetMarketingCategory = () => {
        const response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NonCommercials.asmx/POWithContractGetMarketingCategory",
            data: {},
            dataType: "json",
        });
        return response;
    };

    this.svc_POWithContractGetBranches = function (VendorCode, ProcurementDepartment) {
        const param = {
            VendorCode: VendorCode,
            ProcurementDepartment: ProcurementDepartment,
        };
        const response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NonCommercials.asmx/POWithContractGetBranches",
            data: JSON.stringify(param),
            dataType: "json",
        });
        return response;
    };

    this.svc_POWithContractGetRemarks = function (Vendor_Code, Branch, Procurement_Department) {
        const response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NonCommercials.asmx/POWithContractGetRemarks",
            data: {
                Vendor_Code: Vendor_Code,
                Branch: Branch,
                Procurement_Department: Procurement_Department,
            },
            dataType: "json",
        });
        return response;
    };

    this.svc_POWithContractGetContract = function (
        Vendor_Code,
        Branch,
        Contract_No,
        Remarks_Contract,
        ProcurementDepartment
    ) {
        console.log('Branch', Branch);
        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NonCommercials.asmx/POWithContractGetContract",
            data: {
                Vendor_Code: Vendor_Code,
                Branch: Branch,
                Contract_No: Contract_No,
                Remarks_Contract: Remarks_Contract,
                Procurement_Department: ProcurementDepartment
            },
            dataType: "json",
        });
        return response;
    };

    this.svc_POWithContractSubmit = function (header, detail, Form_Status, notes) {
        const param = {
            h: header,
            d: detail,
            Form_Status: Form_Status,
            Notes: notes
        };
        const response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NonCommercials.asmx/POWithContractSubmit",
            data: JSON.stringify(param),
            dataType: "json",
        });
        return response;
    };

    this.svc_POWithContractGetApproval = function (approvalValue, ListName, ListItemID, HeaderID, Comment) {
        var param = {
            approvalValue: approvalValue,
            ListName: ListName,
            ListItemID: ListItemID,
            HeaderID: HeaderID,
            comments: Comment,
        }; /* function (ntx, IsDocumentReceived) {
        var param = {
            ntx: ntx,
            IsDocumentReceived: IsDocumentReceived,
        }; */

        console.log("Approval Param :", param);



        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NACWebService.asmx/ApproveRequestNonCom",
            //url: "/_layouts/15/Daikin.Application/WebServices/NonCommercials.asmx/POWithContractGetApproval",
            data: JSON.stringify(param),
            dataType: "json",
        });
        return response;
    };

    this.svc_PopUpList = function (SearchTable, PageIndex, SearchBy, Keywords) {
        var param = {
            input: {
                searchTabl: SearchTable,
                searchCol: SearchBy,
                searchVal: Keywords,
                searchLike: 1,
                pageIndx: PageIndex,
                pageSize: 5,
            },

            output: {
                RecordCount: 0,
            }
        };
        console.log("Param for pop up", param);
        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/PopList.asmx/PopUpListData",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    };
});

app.controller("ctrl", function ($scope, svc) {
    $scope.Header = {
        ID: 0,
        Form_No: "",
        Requester_Name: "",
        Requester_Email: "",
        Requester_Department: "",
        Procurement_Department: "",
        Marketing_Category_ID: 0,
        Marketing_Category_Name: "",
        Branch: "",
        Cost_Center: "",
        Grand_Total: 0,
        Vendor_Code: "",
        Vendor_Name: "",
        Item_ID: "",
        Document_Received: false,
        Received_Date: "",
        Approval_Date: "",
        Posting_Date: "",
        Scheduled_Payment_Date: "",
        Actual_Payment_Date: "",
        Current_Index_Approver: "",
        Pending_Approver_Name: "",
        Pending_Approver_Role: "",
        Pending_Approver_Role_ID: 0,
        Last_Action_Date: "",
        Last_Action_Name: "",
        Last_Action_By: "",
        PIC_Team: "",
        Created_Date: DateFormat_ddMMMyyyy(new Date()),
        Detail: [],
    };

    $scope.User = {
        Name: "",
        Email: "",
    };

    $scope.Vendors = [];
    $scope.Vendor = {};

    $scope.Branches = [];
    $scope.Branch = {};

    $scope.MarketingCategories = [];
    $scope.MarketingCategory = {};

    $scope.ProcurementDepartments = [];
    $scope.ProcurementDepartment = {};

    $scope.Remarks = [];
    $scope.RemarksSelected = [];

    $scope.Outcome = 0;
    $scope.IsCurrentApprover = false;
    $scope.IsReceiverDocs = false;
    $scope.IsRequestor = false;
    $scope.IsTaxVerifier = false;
    $scope.IsDocumentReceived = false;

    $scope.ntx = {
        FormNo: "",
        Comment: "",
        Outcome: 0,
        Module: "PC",
    };

    $scope.Revise_Notes = "";
    $scope.Logs = [];
    $scope.RemarkSelected = [];

    $scope.IsUserPOCreated = true;
    $scope.VendorIsDisabled = true;
    $scope.BranchIsDisabled = true;
    let counter = 1;
    $scope.IsDepartment = true;
    $scope.InternalOrderIsShow = false;

    $scope.CostCenters = [];
    $scope.MaxContent = "100%;";
    $scope.Colspan = 9;

    $scope.showModal = "none";
    $scope.popUpModule = "";
    $scope.popUpRowIndex = null;
    $scope.popUpIndexDetail = null;
    $scope.popUpIndexMaterial = null;
    $scope.popUpSearchTable = "";
    $scope.popUpSearchBy = "";
    $scope.popUpSearchKeyword = "";
    $scope.popUpTableHeader = [];
    $scope.popUpSearchOptions = [];
    $scope.popUpCurrPageIndex = 1;
    $scope.popUpTotalRecords = 0;
    $scope.popUpTotalPageCount = 0;
    $scope.PopUpData = [];

    $scope.PopUpDialog = (module, indexDetail, indexMaterial) => {
        $scope.showModal = "block";
        $scope.popUpModule = module;

        if (module == "Cost Center") {
            $scope.popUpIndexDetail = indexDetail;
            $scope.popUpIndexMaterial = indexMaterial;
            $scope.popUpSearchTable = "dbo.MasterMappingCostCenter";
            $scope.popUpSearchOptions = [
                { 'Text': 'Cost Center', 'DB_Col': 'Cost_Center' },
                { 'Text': 'Description', 'DB_Col': 'Description' },
                { 'Text': 'Branch', 'DB_Col': 'Branch' },
                { 'Text': 'Combine', 'DB_Col': 'Combine' },
            ];
            $scope.popUpSearchBy = $scope.popUpSearchOptions[0].Text;
        }
        $scope.PopUp_Search();
    };

    $scope.PopUp_SearchHelper = (keyEvent) => {
        if (keyEvent.which === 13) {
            $scope.PopUp_Search();
        }
    };

    $scope.PopUp_Search = () => {
        $scope.popUpCurrPageIndex = 1;
        var searchByItem = $scope.popUpSearchOptions.find(function (opt) {
            return opt.Text == $scope.popUpSearchBy;
        });
        var searchBy = searchByItem.DB_Col;
        $scope.PopUp_List($scope.popUpSearchTable, $scope.popUpCurrPageIndex, searchBy, $scope.popUpSearchKeyword);
    };

    $scope.PopUp_Prev = () => {
        if ($scope.popUpCurrPageIndex > 1) {
            $scope.popUpCurrPageIndex -= 1;
            var searchByItem = $scope.popUpSearchOptions.find(function (opt) {
                return opt.Text == $scope.popUpSearchBy;
            });
            var searchBy = searchByItem.DB_Col;
            $scope.PopUp_List($scope.popUpSearchTable, $scope.popUpCurrPageIndex, searchBy, $scope.popUpSearchKeyword);
        }
    };

    $scope.PopUp_Next = () => {
        if ($scope.popUpCurrPageIndex < $scope.popUpTotalPageCount) {
            $scope.popUpCurrPageIndex += 1;
            var searchByItem = $scope.popUpSearchOptions.find(function (opt) {
                return opt.Text == $scope.popUpSearchBy;
            });
            var searchBy = searchByItem.DB_Col;
            $scope.PopUp_List($scope.popUpSearchTable, $scope.popUpCurrPageIndex, searchBy, $scope.popUpSearchKeyword);
        }
    };

    $scope.PopUp_List = (tableName, pageIndex, searchBy, keyWord) => {
        if ($scope.popUpModule == "Cost Center") {
            if (!$scope.Header.Procurement_Department.includes('Marketing') && $scope.Header.Branch == 'Head Office') {
                searchBy += '';
                keyWord += '';
            }
            else {
                searchBy += ";Branch";
                keyWord += ";" + $scope.Header.Branch;
            }
        }
        console.log($scope.Header);
        var proc = svc.svc_PopUpList(tableName, pageIndex, searchBy, keyWord);
        proc.then(function (response) {
            var jsonData = JSON.parse(response.data.d);
            if ($scope.popUpModule == "Cost Center") {
                $scope.popUpTotalPageCount = jsonData.TotalPages;
                $scope.popUpTotalRecords = jsonData.TotalRecords;
                $scope.PopUpData = [];
                for (let i of jsonData.Logs) {
                    var newObj = {
                        'ID': i.filter(x => x.Key == 'ID')[0].Value,
                        'Cost Center': i.filter(x => x.Key == 'Cost_Center')[0].Value,
                        'Description': i.filter(x => x.Key == 'Description')[0].Value,
                        'Branch': i.filter(x => x.Key == 'Branch')[0].Value,
                        'Combine': i.filter(x => x.Key == 'Combine')[0].Value,
                    };
                    $scope.PopUpData.push(newObj);
                }
            }
        }).catch(function (err) {
            console.error(err);
        });
    };

    $scope.CloseDialog = () => {
        $scope.showModal = "none";
        $scope.PopUpData = [];
        $scope.popUpSearchBy = "";
        $scope.popUpSearchKeyword = "";
    };

    $scope.PopUp_SelectItem = (id) => {
        var selectedItem = $scope.PopUpData.find(function (item) {
            return item.ID == id;
        });
        $scope.Header.Detail[$scope.popUpIndexDetail].Materials[$scope.popUpIndexMaterial].Cost_Center = selectedItem.Combine;
        $scope.CloseDialog();
    };

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

    $scope.getDateTime = (format, jsondate) => {
        var date = new Date(jsondate);
        let year = date.getFullYear();

        let month = date.getMonth();
        let months = {
            mmmm: new Array(
                "January",
                "February",
                "March",
                "April",
                "May",
                "June",
                "Jully",
                "August",
                "September",
                "October",
                "November",
                "December"
            ),
            mmm: new Array(
                "Jan",
                "Feb",
                "Mar",
                "Apr",
                "Mei",
                "Jun",
                "Jul",
                "Agu",
                "Sep",
                "Okt",
                "Nov",
                "Des"
            ),
            mm: new Array(
                "01",
                "02",
                "03",
                "04",
                "05",
                "06",
                "07",
                "08",
                "09",
                "10",
                "11",
                "12"
            ),
            m: new Array(
                "1",
                "2",
                "3",
                "4",
                "5",
                "6",
                "7",
                "8",
                "9",
                "10",
                "11",
                "12"
            ),
        };

        let d = date.getDate();
        let dd;
        if (d < 10) dd = "0" + d;
        else dd = d;

        let day = date.getDay();
        let days = new Array(
            "Sunday",
            "Monday",
            "Tuesday",
            "Wednesday",
            "Thursday",
            "Friday",
            "Saturday"
        );
        //days = new Array('Minggu', 'Senin', 'Selasa', 'Rabu', 'Kamis', 'Jumat', 'Sabtu');

        let H = date.getHours();
        let HH;
        if (H < 10) HH = "0" + H;
        else HH = H;

        let M = date.getMinutes();
        let MM;
        if (M < 10) MM = "0" + M;
        else MM = M;

        let S = date.getSeconds();
        let SS;
        if (S < 10) SS = "0" + S;
        else SS = S;

        var format = format.replace("{yyyy}", year);
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
    };

    $scope.Close = () => {
        location.href = "List.aspx";
    };

    $scope.CloseApproval = () => {
        location.href = "/_layouts/15/Daikin.Application/Modules/pendingtask/pendingtasklist.aspx";
    };

    $scope.Approval = function () {
        try {
            var st = $scope.Outcome;

            if (st == 0) {
                alert("Please select the outcomes");
                return;
            }

            if ($scope.ntx.Comment.length <= 0 && st == 2) {
                alert(
                    "Please specify your comments for rejecting this PO with Contract"
                );
                return;
            }

            //var approvalValue = st == 1 ? "Approve" : "Reject";
            var approvalValue = "";
            if (st == 1) {
                approvalValue = "Approve";
            }
            else if (st == 2) {
                approvalValue = "Reject";
            }
            else {
                approvalValue = "Revise";
            }

            //var msg = "";
            //if (st == 1) {
            //    msg = "Approve ?";
            //} else if (st == 2) {
            //    msg = "Reject ?";
            //} else {
            //    msg = "Revise ?";
            //}
            //var confirmApprove = confirm(msg);
            var confirmApprove = true;

            if (confirmApprove) {
                $scope.ntx.FormNo = $scope.Header.Form_No;
                $scope.ntx.Outcome = st;
                $scope.ntx.Module = "PC";
                $scope.ntx.Position_ID = $scope.Header.Pending_Approver_Role_ID;
                $scope.ntx.Transaction_ID = $scope.Header.ID;

                var proc = svc.svc_POWithContractGetApproval(
                    approvalValue, "PO Contract", $scope.Header.Item_ID, $scope.Header.ID, $scope.ntx.Comment
                );
                proc.then(
                    function (response) {
                        var data = JSON.parse(response.data.d);
                        if (data.ProcessSuccess) {
                            console.log(data);
                            //if (st == 1) {
                            //    alert("Approved Successfully!");
                            //} else {
                            //    alert("Rejected Successfully!");
                            //}
                            location.href = "/_layouts/15/Daikin.Application/Modules/PendingTask/PendingTaskList.aspx";
                        } else {
                            alert(data.InfoMessage);
                        }
                    },
                    function (data, status) {
                        alert(data.data.Message);
                    }
                );
            }
        } catch (e) {
            alert(e.message);
        }
    };

    $scope.FilterArrayName = (arrays1, arrays2) => {
        const arrays = arrays1.filter((i) => {
            return arrays2.find((o) => {
                return i.Name == o.Name;
            });
        });

        return arrays;
    };

    $scope.FilterArrayCode = (arrays1, arrays2) => {
        const arrays = arrays1.filter((i) => {
            return arrays2.find((o) => {
                return i.Code == o.Code;
            });
        });

        return arrays;
    };

    $scope.ResetHeader = () => {
        $scope.Header = {
            ID: 0,
            Form_No: "",
            Requester_Name: "",
            Requester_Email: "",
            Requester_Department: "",
            Procurement_Department_ID: 0,
            Procurement_Department: "",
            Marketing_Category_ID: 0,
            Marketing_Category_Name: "",
            Branch: "",
            Cost_Center: "",
            Grand_Total: 0,
            Vendor_Code: "",
            Vendor_Name: "",
            Item_ID: 0,
            Document_Received: false,
            Received_Date: "",
            Approval_Date: "",
            Posting_Date: "",
            Scheduled_Payment_Date: "",
            Actual_Payment_Date: "",
            Current_Index_Approver: "",
            Pending_Approver_Name: "",
            Pending_Approver_Role: "",
            Pending_Approver_Role_ID: 0,
            Last_Action_Date: "",
            Last_Action_Name: "",
            Last_Action_By: "",
            PIC_Team: "",
            Created_Date: DateFormat_ddMMMyyyy(new Date()),
            Detail: [],
        };

        $scope.Header.Requester_Name = angular.copy($scope.User.Name);
        $scope.Header.Requester_Email = angular.copy($scope.User.Email);
        $scope.Header.Procurement_Department = angular.copy(
            $scope.ProcurementDepartment.Name
        );
        $scope.Header.Marketing_Category_ID = angular.copy(
            $scope.MarketingCategory.Code
        );
        $scope.Header.Marketing_Category_Name = angular.copy(
            $scope.MarketingCategory.Name
        );
        $scope.Header.Vendor_Code = angular.copy($scope.Vendor.Code);
        $scope.Header.Vendor_Name = angular.copy($scope.Vendor.Name);
        $scope.Header.Branch = angular.copy($scope.Branch.Code);

        $scope.MaxContent = "max-content;";
        $scope.Colspan = 9;
    };

    $scope.POWithContractSubmit = (statusWF) => {
        try {
            if (!validateHeader($scope.Header)) return;
            if (!validateDetails($scope.Header.Detail)) return;
            const submitInfo = getSubmitMessage(statusWF);
            if (!confirm(submitInfo.confirm)) return;

            const header = buildHeaderPayload($scope.Header);
            const detail = buildDetailPayload($scope.Header.Detail);
            const formStatus = getFormStatus(statusWF);

            $scope.SubmitPO(header, detail, formStatus, submitInfo.success);
        } catch (e) {
            console.log("Message :", e.message);
        }
    };

    $scope.POWithContractPOContractMaterialContractAmountChangeCalculate = () => {
        $scope.Header.Grand_Total = 0;

        angular.forEach($scope.Header.Detail, (value, index) => {
            $scope.Header.Detail[index].Show = true;
            $scope.Header.Detail[index].No = index + 1;
            $scope.Header.Detail[index].Grand_Total = 0;

            angular.forEach(value.Materials, (v, i) => {
                $scope.Header.Detail[index].Materials[i].No = i + 1;
                $scope.Header.Detail[index].Grand_Total += parseFloat(
                    //v.Contract_Amount * v.Qty request pa rangga 2022-07-07
                    v.Contract_Amount * 1
                );
            });
            $scope.Header.Grand_Total += parseFloat(
                $scope.Header.Detail[index].Grand_Total
            );
        });
    };

    $scope.POWithContractCheckMaterialWHT = (i, j) => {
        let Check = false;
        if (!$scope.Header.Detail[i].Materials[j].WHT) Check = true;
        $scope.Header.Detail[i].Materials[j].WHT = Check;
    };

    $scope.POWithContractGetCostCenter = (i, j) => {
        const data = $scope.Header.Detail[i].Materials[j].CostCenter.Name;

        $scope.Header.Detail[i].Materials[j].Cost_Center = data;
    };

    $scope.POWithContractCopyMaterial = (i, j) => {
        const newMaterial = angular.copy($scope.Header.Detail[i].Materials[j]);
        newMaterial.CostCenter.Branches = "";
        newMaterial.CostCenter.Business_Area = "";
        newMaterial.CostCenter.Code = "";
        newMaterial.CostCenter.Cost_Center = "";
        newMaterial.CostCenter.Description = "";
        newMaterial.CostCenter.Name = "";
        newMaterial.Qty = 1;
        newMaterial.Text = "";
        newMaterial.WHT = false;
        newMaterial.Cost_Center = "";

        $scope.Header.Detail[i].Materials.push(newMaterial);

        $scope.POWithContractPOContractMaterialContractAmountChangeCalculate();
    };

    $scope.POWithContractDeleteMaterial = (i, j) => {
        $scope.Header.Detail[i].Materials.splice(j, 1);
        if ($scope.Header.Detail[i].Materials.length == 0) {
            const Remark_Contract = $scope.Header.Detail[i].Remarks_Contract;

            angular.forEach($scope.RemarkSelected, function (val, ind) {
                if (val.Name === Remark_Contract) $scope.RemarkSelected.splice(ind, 1);
            });

            $scope.Header.Detail.splice(i, 1);
        }

        $scope.POWithContractPOContractMaterialContractAmountChangeCalculate();
    };

    $scope.POWithContractShowHideMaterial = (index) => {
        if ($scope.Header.Detail[index].Show) {
            $scope.Header.Detail[index].Show = false;
        } else {
            $scope.Header.Detail[index].Show = true;
        }

        $scope.POWithContractCheckStyle();
    };

    $scope.POWithContractDeletePOContractDetail = function (index) {
        const Remark_Contract = $scope.Header.Detail[index].Remarks_Contract;

        $scope.RemarkSelected.forEach(function (v, i) {
            if (v.Name === Remark_Contract) $scope.RemarkSelected.splice(i, 1);
        });

        $scope.Header.Detail.splice(index, 1);

        $scope.POWithContractPOContractMaterialContractAmountChangeCalculate();
    };

    $scope.POWithContractGetContract = (valDetail, indDetail) => {
        console.log('POWithContractGetContract');
        if (!$scope.Vendor.Code) {
            alert("Please Choose Vendor");
            return;
        } else if (!$scope.Branch.Code) {
            alert("Please Choose Branch");
            return;
        } else {
            var indexRemark = $scope.RemarkSelected.findIndex(
                (o) => o.Name == valDetail.Name
            );
            if (indexRemark == -1) {
                console.log('$scope.Header.Detail[0].Remark: ', $scope.Header.Detail[0].Remark);
                var proc = svc.svc_POWithContractGetContract(
                    $scope.Vendor.Code,
                    $scope.Branch.Code,
                    valDetail.Code,
                    $scope.Header.Detail[0].Remark.Name,
                    $scope.ProcurementDepartment.Name,
                    $scope.ProcurementDepartment.ID
                );
                proc.then(
                    function (response) {
                        var data = JSON.parse(response.data.d);
                        console.log(data);
                        if (data.ProcessSuccess) {
                            data.Detail.Remark = valDetail;
                            const checkData = (datas) => {
                                if (Array.isArray(datas)) {
                                    datas.forEach((obj) => {
                                        for (let objProp in obj) {

                                            if (objProp.match("Create_PO_From_Period"))
                                                obj[objProp] = DateFormat_ddMMMyyyy(
                                                    new Date(new Date().setDate(1))
                                                );

                                            else if (objProp.match("Create_PO_To_Period"))
                                                obj[objProp] = DateFormat_ddMMMyyyy(new Date());

                                            else if (objProp.includes("Period"))
                                                obj[objProp] = $scope.ConvertJSONDate(obj[objProp]);

                                            else if (objProp.includes("Date"))
                                                obj[objProp] = $scope.ConvertJSONDate(obj[objProp]);

                                            if (Array.isArray(obj[objProp]))
                                                checkData(obj[objProp]);
                                        }
                                    });
                                } else {
                                    for (let objProp in datas) {
                                        if (objProp.match("Create_PO_From_Period"))
                                            datas[objProp] = DateFormat_ddMMMyyyy(
                                                new Date(new Date().setDate(1))
                                            );
                                        else if (objProp.match("Create_PO_To_Period"))
                                            datas[objProp] = DateFormat_ddMMMyyyy(new Date());
                                        else if (objProp.includes("Period"))
                                            datas[objProp] = $scope.ConvertJSONDate(datas[objProp]);
                                        else if (objProp.includes("Date"))
                                            datas[objProp] = $scope.ConvertJSONDate(datas[objProp]);

                                        if (Array.isArray(datas[objProp]))
                                            checkData(datas[objProp]);
                                    }
                                }
                                return datas;
                            };

                            const Detail = checkData(data.Detail);

                            $scope.Header.Detail[indDetail] = Detail;

                            $scope.RemarkSelected = $scope.Header.Detail.map((o) => o.Remark); //reset remark selected

                            $scope.POWithContractPOContractMaterialContractAmountChangeCalculate();
                            $scope.POWithContractCheckStyle();
                        } else {
                            alert(data.InfoMessage);
                        }
                    },
                    function (data, status) {
                        console.log(data.statusText + " - " + data.data.Message);
                    }
                );
            } else {
                alert("This remark already selected");
                $scope.Header.Detail.splice(indDetail, 1);
                $scope.RemarkSelected = $scope.Header.Detail.map((o) => o.Remark); //reset remark selected
                $scope.POWithContractPOContractMaterialContractAmountChangeCalculate();
            }
        }
    };

    $scope.POWithContractAddContractHeader = () => {
        $scope.Header.Detail.push({
            ID: 0,
            No: counter,
            Contract_No: "",
            Period_Start: "",
            Period_End: "",
            Create_PO_From_Period: "",
            Create_PO_To_Period: "",
            Remark: {},
            Remarks_Contract: "",
            Materials: [],
            Attachments: [],
            Show: false,
        });
        counter++;
    };

    $scope.POWithContractCheckStyle = () => {
        const expanded = $scope.Header.Detail.some((o) => o.Show == true);
        if (expanded) {
            $scope.Colspan = 11;
            $scope.MaxContent = "max-content";
        } else {
            $scope.Colspan = 9;
            $scope.MaxContent = "max-content;";
        }
    };

    $scope.POWithContractGetRemarks = () => {
        if (!$scope.Header.Procurement_Department) {
            alert("Please Choose Procurement Department");
            return;
        } else if (!$scope.Vendor.Code) {
            alert("Please Choose Vendor");
            return;
        } else if (!$scope.Branch.Code) {
            alert("Please Choose Branch");
            return;
        } else {
            var proc = svc.svc_POWithContractGetRemarks(
                $scope.Vendor.Code,
                $scope.Branch.Code,
                $scope.Header.Procurement_Department
            );
            proc.then(
                function (response) {
                    var data = JSON.parse(response.data.d);
                    if (data.ProcessSuccess) {
                        console.log(data);
                        if ($scope.RemarkSelected.length < data.Remarks.length) {
                            data.Remarks.forEach(function (value, index) {
                                var index = $scope.Remarks.findIndex(
                                    (x) => x.Name == value.Name
                                );

                                if (index === -1) {
                                    $scope.Remarks.push(value);
                                }
                            });
                            $scope.POWithContractAddContractHeader();
                            $scope.POWithContractCheckStyle();
                        } else {
                            alert("All contract data has been selected");
                            $scope.POWithContractCheckStyle();
                            return;
                        }
                    } else {
                        alert(data.InfoMessage);
                    }
                },
                function (data, status) {
                    console.log(data.statusText + " - " + data.data.Message);
                }
            );
        }
    };

    $scope.POWithContractBranchChangeResetTable = () => {
        $scope.ResetHeader();
        $scope.RemarkSelected = [];
        $scope.Remarks = [];

        console.log('Branch.Code : ', $scope.Branch.Code);

        $scope.POWithContractGetRemarks();
    };

    $scope.POWithContractGetBranches = () => {
        $scope.ResetHeader();

        $scope.Branches = [];
        $scope.Branch = {};

        $scope.BranchIsDisabled = true;

        $scope.RemarkSelected = [];
        $scope.Remarks = [];

        if ($scope.Vendor.Code.length > 0) {
            var proc = svc.svc_POWithContractGetBranches(
                $scope.Vendor.Code,
                $scope.Header.Procurement_Department
            );
            proc.then(
                function (response) {
                    var data = JSON.parse(response.data.d);
                    if (data.ProcessSuccess) {
                        $scope.Branches = data.Branches;
                        $scope.Branch = data.Branches[0];

                        $scope.BranchIsDisabled = false;
                    } else {
                        alert(data.InfoMessage);
                    }
                },
                function (data, status) {
                    console.log(data.statusText + " - " + data.data.Message);
                }
            );
        }
    };

    $scope.POWithContractGetMarketingCategoryChange = () => {
        $scope.Header.Marketing_Category_ID = angular.copy(
            $scope.MarketingCategory.Code
        );
        $scope.Header.Marketing_Category_Name = angular.copy(
            $scope.MarketingCategory.Name
        );
    };

    $scope.POWithContractGetVendor = () => {
        $scope.ResetHeader();

        $scope.Branches = [];
        $scope.Branch = {};

        $scope.BranchIsDisabled = true;

        $scope.RemarkSelected = [];
        $scope.Remarks = [];

        var proc = svc.svc_POWithContractGetVendor(
            $scope.Header.Procurement_Department
        );
        proc.then(
            function (response) {
                var data = JSON.parse(response.data.d);
                if (data.ProcessSuccess) {
                    console.log(data);
                    $scope.Vendors = data.Vendors;
                    $scope.Vendor = data.Vendors[0];

                    $scope.VendorIsDisabled = false;
                } else {
                    alert(data.InfoMessage);
                }
            },
            function (data, status) {
                console.log(data.statusText + " - " + data.data.Message);
                console.log(status);
            }
        );
    };

    $scope.POWithContractGetMarketingCategory = () => {
        $scope.MarketingCategories = [];
        $scope.MarketingCategory = {};

        var proc = svc.svc_POWithContractGetMarketingCategory();
        proc.then(
            function (response) {
                var data = JSON.parse(response.data.d);
                if (data.ProcessSuccess) {
                    console.log(data);
                    $scope.MarketingCategories = data.MarketingCategories;
                    $scope.MarketingCategory = $scope.MarketingCategories[0];
                } else {
                    alert(data.InfoMessage);
                }
            },
            function (data) {
                console.log(data.statusText + " - " + data.data.Message);
            }
        );
    };

    $scope.POWithContractProcurementDepartmentOnChange = () => {
        $scope.ResetHeader();

        $scope.Vendors = [];
        $scope.Vendor = {};

        $scope.Branches = [];
        $scope.Branch = {};

        $scope.VendorIsDisabled = true;
        $scope.BranchIsDisabled = true;

        $scope.RemarkSelected = [];
        $scope.Remarks = [];
        $scope.POWithContractGetVendor();

        $scope.POWithContractGetMarketingCategory();
        if (['Marketing Trade', 'Marketing Digital'].indexOf($scope.ProcurementDepartment.Name) >= 0) {
            $scope.InternalOrderIsShow = true;
        }
    };

    $scope.POWithContractGetData = () => {
        var proc = svc.svc_POWithContractGetData();
        proc.then(
            function (response) {
                var data = JSON.parse(response.data.d);
                if (data.ProcessSuccess) {
                    const UserProceDept = data.UserProcDepts;
                    $scope.User.Name = data.CurrentLoginName;
                    $scope.User.Email = data.CurrentLoginEmail;
                    $scope.ProcurementDepartments = UserProceDept;
                    $scope.ProcurementDepartment = UserProceDept[0];
                    $scope.IsUserPOCreated = false;

                    $scope.ResetHeader();
                } else {
                    alert(data.InfoMessage);
                }
            },
            function (data, status) {
                console.log(data.statusText + " - " + data.data.Message);
            }
        );
    };

    $scope.ApproverLog = function () {
        try {
            var id = GetQueryString()["ID"]; //Nintex No

            if (id != undefined) {
                var proc = svc.svc_ListLog(id);
                proc.then(
                    function (response) {
                        var data = JSON.parse(response.data.d);
                        if (data.ProcessSuccess) {
                            if (data.Logs.length > 0) {
                                $scope.Logs = data.Logs;
                            }
                        } else {
                            alert(data.InfoMessage);
                        }
                    },
                    function (data, status) {
                        console.log(data.statusText + " - " + data.data.Message);
                    }
                );
            } else {
                console.log("No param");
            }
        } catch (e) {
            alert(e.message);
        }
    };

    $scope.POWithContractGetDataById = () => {
        var id = GetQueryString()["ID"]; //Nintex No
        if (id != undefined && id != null) {
            var proc = svc.svc_POWithContractGetDataById(id);
            $scope.showModal = "none";
            proc.then(response => {
                let data = JSON.parse(response.data.d);
                console.log(data);
                if (data.ProcessSuccess) {
                    const formattingDate = (vals) => {
                        if (Array.isArray(vals)) {
                            vals.forEach((obj) => {
                                for (let header in val) {
                                    if (header.match("Create_PO_From_Period")) {
                                        val[header] = $scope.ConvertJSONDate(val[header]);
                                    } else if (header.match("Create_PO_To_Period")) {
                                        val[header] = $scope.ConvertJSONDate(val[header]);
                                    } else if (header.match("Period_Start")) {
                                        val[header] = $scope.ConvertJSONDate(val[header]);
                                    } else if (header.match("Period_End")) {
                                        val[header] = $scope.ConvertJSONDate(val[header]);
                                    } else if (header.endsWith("Date")) {
                                        val[header] = $scope.ConvertJSONDate(val[header]);
                                    }

                                    if (Array.isArray(val[header])) formattingDate(val[header]);
                                }
                            });
                        } else {
                            for (let header in vals) {
                                if (header.match("Create_PO_From_Period")) {
                                    vals[header] = $scope.ConvertJSONDate(vals[header]);
                                } else if (header.match("Create_PO_To_Period")) {
                                    vals[header] = $scope.ConvertJSONDate(vals[header]);
                                } else if (header.match("Period_Start")) {
                                    vals[header] = $scope.ConvertJSONDate(vals[header]);
                                } else if (header.match("Period_End")) {
                                    vals[header] = $scope.ConvertJSONDate(vals[header]);
                                } else if (header.endsWith("Date")) {
                                    vals[header] = $scope.ConvertJSONDate(vals[header]);
                                }

                                if (Array.isArray(vals[header])) formattingDate(vals[header]);
                            }
                        }
                    };

                    formattingDate(data.Header);
                    $scope.Header = data.Header;
                    if ($scope.Header.ID > 0) {
                        console.log("Header ID > 0");
                        var lookup_elements = document.getElementsByClassName("fa");
                        for (var i = 0; i < lookup_elements.length; i++) {
                            lookup_elements[i].style.pointerEvents = "none";
                        }
                    }

                    $scope.Vendors = data.Vendors;
                    data.Vendors.map((i) => {
                        if (i.Code == data.Header.Vendor_Code) $scope.Vendor = i;
                    });

                    $scope.Branches = data.Branches;
                    data.Branches.map((i) => {
                        if (i.Name == data.Header.Branch) $scope.Branch = i;
                    });

                    $scope.Remarks = data.Remarks;
                    data.Remarks.map((i) => {
                        $scope.Header.Detail.map((j, k) => {
                            if (i.Name == j.Remarks_Contract)
                                $scope.Header.Detail[k].Remark = i;
                        });
                    });

                    $scope.MarketingCategories = data.MarketingCategories;
                    data.MarketingCategories.map((i) => {
                        if (i.Name == data.Header.Marketing_Category_Name)
                            $scope.MarketingCategory = i;
                    });

                    $scope.ProcurementDepartments = data.UserDepartment;
                    $scope.IsDepartment = false;
                    data.UserDepartment.map((i) => {
                        if (i.Name == data.Header.Procurement_Department) {
                            $scope.ProcurementDepartment = i;
                            $scope.IsDepartment = true;
                        }
                    });

                    $scope.IsCurrentApprover = data.IsCurrentApprover;
                    $scope.IsReceiverDocs = data.IsReceiverDocs;
                    $scope.IsRequestor = data.IsRequestor;
                    $scope.IsTaxVerifier = data.IsTaxVerifier;

                    $scope.IsDocumentReceived =
                        data.Header.Document_Received == "0" ? false : true;

                    $scope.ApproverLog();
                    $scope.POWithContractPOContractMaterialContractAmountChangeCalculate();

                    $scope.POWithContractCheckStyle();

                    if (['Marketing Trade', 'Marketing Digital'].indexOf($scope.ProcurementDepartment.Name) >= 0) {
                        $scope.InternalOrderIsShow = true;
                    }
                } else {
                    console.log(data);
                }
            }).catch(err => {
                alert(err);
            });
        } else {
            $scope.POWithContractGetData();
        }
    };

    $scope.SubmitPO = function (header, detail, formStatus, successMessage) {
        const proc = svc.svc_POWithContractSubmit(header, detail, formStatus, successMessage);
        proc.then(function (response) {
            const data = JSON.parse(response.data.d);
            if (!data.ProcessSuccess) {
                alert(data.InfoMessage);
                return;
            }
            alert(successMessage);
            location.href = "List.aspx";
        }).catch(function (err) {
            console.log(err.statusText + " - " + err.data.Message);
        });
    };

    $scope.POWithContractGetDataById();
});


function validateHeader(header) {
    const procDeptMarketing = ["Marketing Trade", "Marketing Digital"];
    if (!header.Procurement_Department || header.Procurement_Department === "Please Select") {
        alert("Please select Procurement Department");
        return false;
    }
    if (!header.Vendor_Code) {
        alert("Please choose vendor name");
        return false;
    }
    if (!header.Branch) {
        alert("Please choos branch");
        return false;
    }
    if (header.Marketing_Category_ID <= 0 && procDeptMarketing.indexOf(header.Procurement_Department) >= 0) {
        alert("Please choose marketing category");
        return false;
    }
    return true;
};

function validateDetails(details) {
    if (isEmpty(details)) {
        alert("Please add contract");
        return false;
    }
    for (const detail of details) {
        if (!isValidContract(detail)) return false;
    }
    return true;
};

function alertAndFail() {
    alert("Please complete the column in the table");
    return false;
};

function getFormStatus(statusWF) {
    if (statusWF == "1") return "1";
    if (statusWF == "19") return "19";
    return "-";
}

function buildDetailPayload(details) {
    const detail = [];
    details.forEach(function (value, index) {
        const materials = value.Materials.map(function (o) {
            return {
                Contract_Amount: o.Contract_Amount,
                Contract_Detail_Id: o.Contract_Detail_Id,
                Contract_ID: o.Contract_ID,
                Contract_No: o.Contract_No,
                Cost_Center: o.Cost_Center,
                Form_No: o.Form_No,
                GL: o.GL,
                GL_Description: o.GL_Description,
                Header_ID: o.Header_ID,
                ID: o.ID,
                Material_Description: o.Material_Description,
                Material_Name: o.Material_Name,
                Material_Number: o.Material_Number,
                No: o.No,
                Qty: o.Qty,
                Remarks_Contract: o.Remarks_Contract,
                Text: o.Text,
                Variable_Amount: o.Variable_Amount,
                WHT: o.WHT
            };
        });

        detail[index] = {
            Contract_ID: value.Contract_ID,
            Contract_No: value.Contract_No,
            Create_PO_From_Period: value.Create_PO_From_Period,
            Create_PO_To_Period: value.Create_PO_To_Period,
            Created_Date: value.Created_Date,
            Form_No: value.Form_No,
            Grand_Total: value.Grand_Total,
            Header_ID: value.Header_ID,
            ID: value.ID,
            Internal_Order_Code: value.Internal_Order_Code,
            Internal_Order_Name: value.Internal_Order_Name,
            No: value.No,
            Period_End: value.Period_End,
            Period_Start: value.Period_Start,
            Remarks_Contract: value.Remarks_Contract,
            Materials: materials
        };
    });
    return detail;
};

function buildHeaderPayload(header) {
    const h = angular.copy(header);

    return {
        Actual_Payment_Date: h.Actual_Payment_Date,
        Approval_Date: h.Approval_Date,
        Approval_Status: h.Approval_Status,
        Branch: h.Branch,
        Cost_Center: h.Cost_Center,
        Created_Date: h.Created_Date,
        Current_Index_Approver: h.Current_Index_Approver,
        Document_Received: h.Document_Received,
        Form_No: h.Form_No,
        Grand_Total: h.Grand_Total,
        ID: h.ID,
        Item_ID: h.Item_ID,
        Last_Action_By: h.Last_Action_By,
        Last_Action_Date: h.Last_Action_Date,
        Last_Action_Name: h.Last_Action_Name,
        Marketing_Category_ID: h.Marketing_Category_ID,
        Marketing_Category_Name: h.Marketing_Category_Name,
        PIC_Team: h.PIC_Team,
        Pending_Approver_Name: h.Pending_Approver_Name,
        Pending_Approver_Role: h.Pending_Approver_Role,
        Pending_Approver_Role_ID: h.Pending_Approver_Role_ID,
        Posting_Date: h.Posting_Date,
        Procurement_Department: h.Procurement_Department,
        Received_Date: h.Received_Date,
        Requester_Department: h.Requester_Department,
        Requester_Email: h.Requester_Email,
        Requester_Name: h.Requester_Name,
        Scheduled_Payment_Date: h.Scheduled_Payment_Date,
        Vendor_Code: h.Vendor_Code,
        Vendor_Name: h.Vendor_Name
    };
};

function getSubmitMessage(statusWF) {
    if (statusWF == 1 || statusWF == 19) {
        return {
            confirm: "Submit ?",
            success: "Submit Successfully!"
        };
    }

    return {
        confirm: "Save as Draft ?",
        success: "Success Save as Draft!"
    };
};

function isValidContract(detail) {
    if (!hasValidPeriod(detail)) {
        alert("Please complete the column in the table");
        return false;
    }

    if (!hasMaterials(detail)) {
        alert("Please choose contract remarks");
        return false;
    }

    return areMaterialsValid(detail.Materials);
};

function areMaterialsValid(materials) {
    for (const cm of materials) {
        if (!isValidMaterial(cm)) {
            alert("Please complete the column in the table");
            return false;
        }
    }
    return true;
};

function isValidMaterial(cm) {
    return (
        isPositive(cm.Contract_Amount) &&
        isPresent(cm.Cost_Center) &&
        isPresent(cm.Text) &&
        isPositive(cm.Qty)
    );
};

function hasValidPeriod(detail) {
    return detail.Create_PO_From_Period && detail.Create_PO_To_Period;
};

function hasMaterials(detail) {
    return detail.Materials && detail.Materials.length > 0;
};

function isPositive(value) {
    return value > 0;
};

function isPresent(value) {
    return !!value;
};

function isEmpty(arr) {
    return !arr || arr.length === 0;
};

