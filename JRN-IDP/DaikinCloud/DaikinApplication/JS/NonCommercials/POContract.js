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
        var param = {
            Form_No: Form_No,
            Module_Code: "M020",
            Transaction_ID: 0
        };

        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/Master.asmx/GetHistoryLog",
            data: JSON.stringify(param),
            dataType: "json",
        });
        return response;
    };

    this.svc_POWithContractGetDataById = function (ID) {
        var param = {
            Form_No: ID,
        };
        console.log(param);
        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NonCommercials.asmx/POWithContractGetDataById",
            data: JSON.stringify(param),
            dataType: "json",
        });
        return response;
    };

    this.svc_POWithContractGetData = () => {
        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NonCommercials.asmx/POWithContractGetData",
            data: {},
            dataType: "json",
        });
        return response;
    };

    this.svc_POWithContractGetVendor = (ProcurementDepartment) => {
        var param = {
            ProcurementDepartment: ProcurementDepartment,
        };
        console.log('Param for get Vendor ', param);
        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NonCommercials.asmx/POWithContractGetVendor",
            data: JSON.stringify(param),
            dataType: "json",
        });
        return response;
    };

    this.svc_POWithContractGetMarketingCategory = () => {
        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NonCommercials.asmx/POWithContractGetMarketingCategory",
            data: {},
            dataType: "json",
        });
        return response;
    };

    this.svc_POWithContractGetBranches = function (
        VendorCode,
        ProcurementDepartment
    ) {
        var param = {
            VendorCode: VendorCode,
            ProcurementDepartment: ProcurementDepartment,
        };
        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NonCommercials.asmx/POWithContractGetBranches",
            data: JSON.stringify(param),
            dataType: "json",
        });
        return response;
    };

    this.svc_POWithContractGetRemarks = function (
        Vendor_Code,
        Branch,
        Procurement_Department
    ) {
        console.log(`Vendor Code : ${Vendor_Code}; Branch : ${Branch}; Procurement Dept : ${Procurement_Department}.`)
        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NonCommercials.asmx/POWithContractGetRemarks",
            data: {
                Vendor_Code: Vendor_Code,
                Branch: Branch,
                Procurement_Department: Procurement_Department,
            },
            dataType: "json",
        });
        console.log("Response :", response);
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
        var param = {
            h: header,
            d: detail,
            Form_Status: Form_Status,
            Notes: notes
        };

        console.log(param);
        var response = $http({
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
                for (i of jsonData.Logs) {
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

            d = date.getDate();
            if (d < 10) dd = "0" + d;
            else dd = d;

            day = date.getDay();
            days = new Array(
                "Sunday",
                "Monday",
                "Tuesday",
                "Wednesday",
                "Thursday",
                "Friday",
                "Saturday"
            );
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
    };

    $scope.getDateTime = (format, jsondate) => {
        var date = new Date(jsondate);
        year = date.getFullYear();

        month = date.getMonth();
        months = {
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

        d = date.getDate();
        if (d < 10) dd = "0" + d;
        else dd = d;

        day = date.getDay();
        days = new Array(
            "Sunday",
            "Monday",
            "Tuesday",
            "Wednesday",
            "Thursday",
            "Friday",
            "Saturday"
        );
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
        console.log("Header", $scope.Header);
        try {
            var anyError = false;
            if ($scope.Header.Procurement_Department.length == 0 || $scope.Header.Procurement_Department == "Please Select") {
                alert("Please select Procurement Department");
                return;
            }

            if ($scope.Header.Vendor_Code == null || $scope.Header.Vendor_Code == undefined || $scope.Header.Vendor_Code.length <= 0) {
                alert("Please choose vendor name");
                return;
            }

            if ($scope.Header.Branch == null || $scope.Header.Branch == undefined || $scope.Header.Branch.length <= 0) {
                alert("Please choose branch");
                return;
            }

            if (
                $scope.Header.Marketing_Category_ID <= 0 && ['Marketing Trade', 'Marketing Digital'].indexOf($scope.Header.Procurement_Department) >= 0
            ) {
                alert("Please choose marketing category");
                return;
            }

            var msg = "Please complete the column in the table";

            //console.log("Call Details Here :", $scope.Header.Detail);

            if ($scope.Header.Detail.length > 0) {
                for (var j = 0; j < $scope.Header.Detail.length; j++) {
                    var ch = $scope.Header.Detail[j];

                    if (ch.Create_PO_To_Period.length <= 0) {
                        anyError = true;
                    }

                    if (ch.Create_PO_From_Period.length <= 0) {
                        anyError = true;
                    }

                    if (ch.Materials.length > 0) {
                        for (var i = 0; i < ch.Materials.length; i++) {
                            var cm = ch.Materials[i];
                            if (cm.Contract_Amount.length <= 0 || cm.Contract_Amount <= 0) {
                                anyError = true;
                            }

                            if (!cm.Cost_Center || cm.Cost_Center == undefined) {
                                anyError = true;
                            }

                            if (!cm.Text || cm.Text == undefined) {
                                anyError = true;
                            }

                            if (cm.Qty <= 0) {
                                anyError = true;
                            }
                        }
                    } else {
                        alert("Please choose contract remarks");
                        return;
                    }
                }
            } else {
                alert("Please add contract");
                return;
            }

            if (anyError) {
                alert(msg);
                return;
            }

            var submitMessage = "";
            var responseSubmitMessage = "";
            if (statusWF == 1 || statusWF == 19) {
                submitMessage = "Submit ?";
                responseSubmitMessage = "Submit Successfully!";
            } else if (statusWF == 0) {
                submitMessage = "Save as Draft ?";
                responseSubmitMessage = "Success Save as Draft!";
            }

            var confirmMsg = confirm(submitMessage);
            if (!confirmMsg) {
                return;
            }
            // console.log($scope.Header);
            const header = {
                Actual_Payment_Date: angular.copy($scope.Header.Actual_Payment_Date),
                Approval_Date: angular.copy($scope.Header.Approval_Date),
                Approval_Status: angular.copy($scope.Header.Approval_Status),
                Branch: angular.copy($scope.Header.Branch),
                Cost_Center: angular.copy($scope.Header.Cost_Center),
                Created_Date: angular.copy($scope.Header.Created_Date),
                Current_Index_Approver: angular.copy(
                    $scope.Header.Current_Index_Approver
                ),
                Document_Received: angular.copy($scope.Header.Document_Received),
                Form_No: angular.copy($scope.Header.Form_No),
                Grand_Total: angular.copy($scope.Header.Grand_Total),
                ID: angular.copy($scope.Header.ID),
                Item_ID: angular.copy($scope.Header.Item_ID),
                Last_Action_By: angular.copy($scope.Header.Last_Action_By),
                Last_Action_Date: angular.copy($scope.Header.Last_Action_Date),
                Last_Action_Name: angular.copy($scope.Header.Last_Action_Name),
                Marketing_Category_ID: angular.copy(
                    $scope.Header.Marketing_Category_ID
                ),
                Marketing_Category_Name: angular.copy(
                    $scope.Header.Marketing_Category_Name
                ),
                PIC_Team: angular.copy($scope.Header.PIC_Team),
                Pending_Approver_Name: angular.copy(
                    $scope.Header.Pending_Approver_Name
                ),
                Pending_Approver_Role: angular.copy(
                    $scope.Header.Pending_Approver_Role
                ),
                Pending_Approver_Role_ID: angular.copy(
                    $scope.Header.Pending_Approver_Role_ID
                ),
                Posting_Date: angular.copy($scope.Header.Posting_Date),
                Procurement_Department: angular.copy(
                    $scope.Header.Procurement_Department
                ),
                Received_Date: angular.copy($scope.Header.Received_Date),
                Requester_Department: angular.copy($scope.Header.Requester_Department),
                Requester_Email: angular.copy($scope.Header.Requester_Email),
                Requester_Name: angular.copy($scope.Header.Requester_Name),
                Scheduled_Payment_Date: angular.copy(
                    $scope.Header.Scheduled_Payment_Date
                ),
                Vendor_Code: angular.copy($scope.Header.Vendor_Code),
                Vendor_Name: angular.copy($scope.Header.Vendor_Name),
            };

            // console.log(header);

            let detail = [];
            $scope.Header.Detail.forEach(function (value, index) {
                const materials = value.Materials.map((o) => {
                    return {
                        Contract_Amount: angular.copy(o.Contract_Amount),
                        Contract_Detail_Id: angular.copy(o.Contract_Detail_Id),
                        Contract_ID: angular.copy(o.Contract_ID),
                        Contract_No: angular.copy(o.Contract_No),
                        Cost_Center: angular.copy(o.Cost_Center),
                        Form_No: angular.copy(o.Form_No),
                        GL: angular.copy(o.GL),
                        GL_Description: angular.copy(o.GL_Description),
                        Header_ID: angular.copy(o.Header_ID),
                        ID: angular.copy(o.ID),
                        Material_Description: angular.copy(o.Material_Description),
                        Material_Name: angular.copy(o.Material_Name),
                        Material_Number: angular.copy(o.Material_Number),
                        No: angular.copy(o.No),
                        Qty: angular.copy(o.Qty),
                        Remarks_Contract: angular.copy(o.Remarks_Contract),
                        Text: angular.copy(o.Text),
                        Variable_Amount: angular.copy(o.Variable_Amount),
                        WHT: angular.copy(o.WHT),
                    };
                });

                detail[index] = {
                    Contract_ID: angular.copy(value.Contract_ID),
                    Contract_No: angular.copy(value.Contract_No),
                    Create_PO_From_Period: angular.copy(value.Create_PO_From_Period),
                    Create_PO_To_Period: angular.copy(value.Create_PO_To_Period),
                    Created_Date: angular.copy(value.Created_Date),
                    Form_No: angular.copy(value.Form_No),
                    Grand_Total: angular.copy(value.Grand_Total),
                    Header_ID: angular.copy(value.Header_ID),
                    ID: angular.copy(value.ID),
                    Internal_Order_Code: angular.copy(value.Internal_Order_Code),
                    Internal_Order_Name: angular.copy(value.Internal_Order_Name),
                    No: angular.copy(value.No),
                    Period_End: angular.copy(value.Period_End),
                    Period_Start: angular.copy(value.Period_Start),
                    Remarks_Contract: angular.copy(value.Remarks_Contract),
                    Materials: materials,
                };
            });
            console.log("Details :", detail);

            // const TempAttachment = $scope.Header.Detail.map(v => {if(v.Attachments) return v.Attachments;});
            //let Form_Status = statusWF ? "1": "-";//trigger wf
            let Form_Status = "";
            if (statusWF == "1") {
                Form_Status = "1";
            } else if (statusWF == "19") {
                Form_Status = "19";
            } else {
                Form_Status = "-";
            }

            var proc = svc.svc_POWithContractSubmit(header, detail, Form_Status, $scope.Revise_Notes);
            proc.then(
                function (response) {
                    var data = JSON.parse(response.data.d);
                    if (data.ProcessSuccess) {
                        console.log(data);
                        alert(responseSubmitMessage);
                        location.href = "List.aspx";
                        //location.href = 'POContract.aspx?ID=' + data.Header.Form_No;
                    } else {
                        alert(data.InfoMessage);
                        // console.log(data);
                    }
                },
                function (err) {
                    alert(err.statusText + " - " + err.data.Message);
                    console.log(err.statusText + " - " + err.data.Message);
                }
            );
        } catch (e) {
            alert(e.message);
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

    //$scope.POWithContractCopyingMaterial = (i,j) => {
    //    return new Promise((resolve,reject) => {
    //        const newMaterial = angular.copy($scope.Header.Detail[i].Materials[j]);
    //        newMaterial.CostCenter.Branches = "";
    //        newMaterial.CostCenter.Business_Area = "";
    //        newMaterial.CostCenter.Code = "";
    //        newMaterial.CostCenter.Cost_Center = "";
    //        newMaterial.CostCenter.Description = "";
    //        newMaterial.CostCenter.Name = "";
    //        newMaterial.Qty = 1;
    //        newMaterial.Text = "";
    //        newMaterial.WHT = false;
    //        newMaterial.Cost_Center = "";

    //        $scope.Header.Detail[i].Materials.push(newMaterial);
    //        resolve("Copying material..");
    //    })
    //}

    $scope.POWithContractCopyMaterial = (i, j) => {
        //async function checkRequestdata() {
        //    const newData = await $scope.POWithContractCopyingMaterial(i,j);
        //    console.log(newData)
        //}
        //checkRequestdata();
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

                            // data.Detail.Create_PO_From_Period = DateFormat_ddMMMyyyy(new Date(new Date().setDate(1)));
                            // data.Detail.Create_PO_To_Period = DateFormat_ddMMMyyyy(new Date());
                            // data.Detail.Period_Start = $scope.ConvertJSONDate(data.Detail.Period_Start);
                            // data.Detail.Period_End = $scope.ConvertJSONDate(data.Detail.Period_End);
                            data.Detail.Remark = valDetail;

                            //$scope.CostCenters = [];
                            //const diffCostCenter = data.CostCenter.filter(
                            //    ({ Code: val1 }) =>
                            //        !$scope.CostCenters.some(({ Code: val2 }) => val1 == val2)
                            //);

                            //diffCostCenter.forEach((o) => {
                            //    $scope.CostCenters.push(o);
                            //});

                            const checkData = (datas) => {
                                if (Array.isArray(datas)) {
                                    datas.forEach((obj) => {
                                        for (objProp in obj) {

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
                                    for (objProp in datas) {
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
                        // console.log(data);
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

        //$scope.

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
                    console.log(data);
                    //const UserProceDept = $scope.FilterArrayName(
                    //    data.UserProcDepts,
                    //    data.ContractUserProcDepts
                    //);
                    const UserProceDept = data.UserProcDepts;
                    console.log("User Procurement Departments : ", UserProceDept);
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

    //$scope.POWithContractGetDataById = () => {
    //    try {
    //        var id = GetQueryString()["ID"]; //Nintex No

    //        if (id != undefined) {
    //            const proc = svc.svc_POWithContractGetDataById(id);
    //            let data = JSON.parse(proc.data.d);
    //            if (data.ProcessSuccess) {
    //                console.log(data);

    //                const formattingDate = (vals) => {
    //                    if (Array.isArray(vals)) {
    //                        vals.map((val) => {
    //                            for (let header in val) {
    //                                if (header.match("Create_PO_From_Period")) {
    //                                    val[header] = $scope.ConvertJSONDate(val[header]);
    //                                } else if (header.match("Create_PO_To_Period")) {
    //                                    val[header] = $scope.ConvertJSONDate(val[header]);
    //                                } else if (header.match("Period_Start")) {
    //                                    val[header] = $scope.ConvertJSONDate(val[header]);
    //                                } else if (header.match("Period_End")) {
    //                                    val[header] = $scope.ConvertJSONDate(val[header]);
    //                                } else if (header.endsWith("Date")) {
    //                                    val[header] = $scope.ConvertJSONDate(val[header]);
    //                                }

    //                                if (Array.isArray(val[header])) formattingDate(val[header]);
    //                            }
    //                        });
    //                    } else {
    //                        for (let header in vals) {
    //                            if (header.match("Create_PO_From_Period")) {
    //                                vals[header] = $scope.ConvertJSONDate(vals[header]);
    //                            } else if (header.match("Create_PO_To_Period")) {
    //                                vals[header] = $scope.ConvertJSONDate(vals[header]);
    //                            } else if (header.match("Period_Start")) {
    //                                vals[header] = $scope.ConvertJSONDate(vals[header]);
    //                            } else if (header.match("Period_End")) {
    //                                vals[header] = $scope.ConvertJSONDate(vals[header]);
    //                            } else if (header.endsWith("Date")) {
    //                                vals[header] = $scope.ConvertJSONDate(vals[header]);
    //                            }

    //                            if (Array.isArray(vals[header])) formattingDate(vals[header]);
    //                        }
    //                    }
    //                };
    //                formattingDate(data.Header);
    //                $scope.Header = data.Header;

    //                $scope.Vendors = data.Vendors;
    //                data.Vendors.map((i) => {
    //                    if (i.Code == data.Header.Vendor_Code) $scope.Vendor = i;
    //                });

    //                $scope.Branches = data.Branches;
    //                data.Branches.map((i) => {
    //                    if (i.Name == data.Header.Branch) $scope.Branch = i;
    //                });

    //                $scope.Remarks = data.Remarks;
    //                data.Remarks.map((i) => {
    //                    $scope.Header.Detail.map((j, k) => {
    //                        if (i.Name == j.Remarks_Contract)
    //                            $scope.Header.Detail[k].Remark = i;
    //                    });
    //                });

    //                $scope.MarketingCategories = data.MarketingCategories;
    //                data.MarketingCategories.map((i) => {
    //                    if (i.Name == data.Header.Marketing_Category_Name)
    //                        $scope.MarketingCategory = i;
    //                });

    //                $scope.ProcurementDepartments = data.UserDepartment;
    //                $scope.IsDepartment = false;
    //                data.UserDepartment.map((i) => {
    //                    if (i.Name == data.Header.Procurement_Department) {
    //                        $scope.ProcurementDepartment = i;
    //                        $scope.IsDepartment = true;
    //                    }
    //                });

    //                const diffCostCenter = data.CostCenter.filter(
    //                    ({ Name: val1 }) =>
    //                        !$scope.CostCenters.some(({ Name: val2 }) => val1 == val2)
    //                );

    //                diffCostCenter.forEach((o) => {
    //                    $scope.CostCenters.push(o);
    //                });

    //                // //data.forEach
    //                data.CostCenter.map((i) => {
    //                    $scope.Header.Detail.map((j, k) => {
    //                        j.Materials.map((l, m) => {
    //                            if (i.Name == l.Cost_Center)
    //                                $scope.Header.Detail[k].Materials[m].CostCenter = i;
    //                            // console.log(i);
    //                        });
    //                    });
    //                });

    //                $scope.IsCurrentApprover = data.IsCurrentApprover;
    //                $scope.IsReceiverDocs = data.IsReceiverDocs;
    //                $scope.IsRequestor = data.IsRequestor;
    //                $scope.IsTaxVerifier = data.IsTaxVerifier;

    //                $scope.IsDocumentReceived =
    //                    data.Header.Document_Received == "0" ? false : true;

    //                $scope.ApproverLog();
    //                $scope.POWithContractPOContractMaterialContractAmountChangeCalculate();

    //                $scope.POWithContractCheckStyle();
    //                if (['Marketing Trade','Marketing Digital'].indexOf($scope.ProcurementDepartment.Name) >= 0) {
    //                    $scope.InternalOrderIsShow = true;
    //                }
    //            } else {
    //                console.error(data.statusText);
    //            }
    //        } else {
    //            $scope.POWithContractGetData();
    //        }
    //    } catch (err) {
    //        alert(err);
    //    }
    //};

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

                    //const diffCostCenter = data.CostCenter.filter(
                    //               ({ Name: val1 }) =>
                    //                   !$scope.CostCenters.some(({ Name: val2 }) => val1 == val2)
                    //           );

                    //diffCostCenter.forEach((o) => {
                    //    $scope.CostCenters.push(o);
                    //});


                    //// //data.forEach
                    //data.CostCenter.map((i) => {
                    //    $scope.Header.Detail.map((j, k) => {
                    //        j.Materials.map((l, m) => {
                    //            if (i.Name == l.Cost_Center)
                    //                $scope.Header.Detail[k].Materials[m].CostCenter = i;
                    //            // console.log(i);
                    //        });
                    //    });
                    //});

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

    $scope.POWithContractGetDataById();

    // Count group
    // const countSelected = $scope.RemarkSelected.reduce((cur,val) => {
    //     let code = val.Code.replace(" ", "_");
    //     if (!cur.hasOwnProperty(code)) {
    //         cur[code] = 0;
    //     }
    //     cur[code]++;
    //     return cur;
    // }, {});

    // var countsExtended = Object.keys(countSelected).map(k => {
    //     return {name: k.replace("_", " "), count: countSelected[k]};
    // });

    // console.log(countsExtended);
});
