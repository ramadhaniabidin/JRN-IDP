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

app.service("svc", function ($http) {
    this.svc_ListLog = function (Form_No, Transaction_ID) {
        const param = {
            Form_No: Form_No,
            Module_Code: 'M011',
            Transaction_ID: Transaction_ID
        };

        const response = $http({
            method: "post",
            url: "/_layouts/15/daikin.application/WebServices/Master.asmx/GetHistoryLog",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    };

    this.svc_LoadDDL = function () {
        const response = $http({
            method: "post",
            url: "/_layouts/15/daikin.application/WebServices/Commercials.asmx/LoadDDL",
            data: {},
            dataType: "json"
        });
        return response;
    };

    this.svc_GetData = function (ID) {
        const param = {
            Form_No: ID,
        };

        const response = $http({
            method: "post",
            url: "/_layouts/15/daikin.application/WebServices/Commercials.asmx/GetDataSC",
            data: JSON.stringify(param),
            dataType: "json"
        });

        return response;
    };

    this.svc_GetDataByType = function (ReferenceNo, ReferenceType, TradingPartnerCode, PPJKCode) {
        const param = {
            ReferenceNo: ReferenceNo,
            ReferenceType: ReferenceType,
            TradingPartnerCode: TradingPartnerCode,
            PPJKCode: PPJKCode,
        };

        const response = $http({
            method: "post",
            url: "/_layouts/15/daikin.application/WebServices/Commercials.asmx/GetDataByType",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    };

    this.svc_SaveUpdate = function (h, listDetail) {
        const param = {
            h: h,
            listDetail: listDetail,
        };
        const response = $http({
            method: "post",
            url: "/_layouts/15/daikin.application/WebServices/Commercials.asmx/SaveSC",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    };


    this.svc_PopListReference = function (Keywords, PageIndex, ReferenceType) {
        const param = {
            Keywords: Keywords,
            PageIndex: PageIndex,
            ReferenceType: ReferenceType,
        };

        const response = $http({
            method: "post",
            url: "/_layouts/15/daikin.application/WebServices/Commercials.asmx/PopListReference",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;

    };

    this.Approval = function (h, listDetail) {
        const param = {
            h: h,
            listDetail: listDetail,
        };

        const response = $http({
            method: "post",
            url: "/_layouts/15/daikin.application/WebServices/Commercials.asmx/SaveSC",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    };

    this.svc_LoadDDLVendorLC = function (expensetype) {
        const param = {
            expensetype: expensetype,
        };
        const response = $http({
            method: "post",
            url: "/_layouts/15/daikin.application/WebServices/Commercials.asmx/LoadDDLVendorLC",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;

    };

    this.svc_Approval = function (ntx, listRemarks, listDetail, IsDocumentReceived) {
        const param = {
            ntx: ntx,
            listRemarks: listRemarks,
            listDetail: listDetail,
            IsDocumentReceived: IsDocumentReceived,
        };

        const response = $http({
            method: "post",
            url: "/_layouts/15/daikin.application/WebServices/Commercials.asmx/SC_Approval",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    };
});

app.controller('ctrl', function ($scope, svc, Upload, $timeout) {
    $scope.ddlBussPlace = [];
    $scope.BL_No = '';
    $scope.FOB_No = '';
    $scope.bussPlace = {};
    $scope.ddlCondition = [];
    $scope.ddlExpenseType = [];
    $scope.expenseType = {};
    $scope.ddlVendor = [];
    $scope.ddlPPJK = [];
    $scope.ppjk = {};
    $scope.ddlPlant = [];
    $scope.plant = {};
    $scope.ddlWHT = [];
    $scope.ddlVAT = [];
    $scope.txt_ReffNo = '';
    $scope.Reference_Type = 'Reference No.';
    $scope.BtnAdd = '(+) Add';
    $scope.Keywords = '';
    $scope.showModal = 'none';
    $scope.IsRequestor = false;
    $scope.IsTaxVerifier = false;
    $scope.showWHT = false;
    $scope.Items = [];
    $scope.SelectedFile = '';
    $scope.uploadf = false;
    $scope.Remarks = [];
    $scope.IsDocumentReceived = false;
    $scope.CanEdit = true;
    $scope.DisableActivate = false;
    $scope.Outcome = 0;
    $scope.IsCurrentApprover = false;

    $scope.Header = {
        ID: 0,
        Form_No: '',
        Requester_Name: '',
        Requester_Email: '',
        Approval_Status: '',
        Approval_Status_Name: '',
        Document_Received: '0',
        Item_ID: 0,
        Grand_Total: 0,
        Pending_Approver_Role_ID: '',
        PPJK_Curr: '',
        PPJK_Category: '',
        OA_Summary_Attachment: '',
    };

    $scope.ntx = {
        FormNo: '',
        Comment: '',
        Outcome: 0,
        Module: 'SC',
    };

    $scope.closeModal = function () {
        $scope.showModal = 'none';
    };

    $("body").on("click", ".Pager .page", function () {
        $scope.PopListReference(parseInt($(this).attr('page')));
    });

    $scope.ApproverLog = function () {
        var proc = svc.svc_ListLog($scope.Header.Form_No, $scope.Header.ID);
        proc.then(function (response) {
            var data = JSON.parse(response.data.d);
            console.log("Get history log ", data);
            if (data.ProcessSuccess) {
                $scope.Logs = data.Logs;
            } else {
                alert(data.InfoMessage);
            }

        }, function (data, status) {
            console.log(data.statusText + ' - ' + data.data.Message);
        });
    };

    $scope.UpdateLogRemarks = function (idx) {
        var objIndex = $scope.Remarks.findIndex(o => o.ID == idx.ID);
        $scope.Remarks[objIndex].Outcome = idx.Outcome;
        $scope.Remarks[objIndex].Reason_Rejection = idx.Outcome == true ? '' : idx.Reason_Rejection;
        console.log($scope.Remarks, 'Remarks');
    };

    $scope.PopListReference = function (page) {
        try {
            if ($scope.txt_ReffNo.length <= 0) {
                $scope.showModal = 'block';
            }
            var req = svc.svc_PopListReference($scope.Keywords, page, $scope.Reference_Type);
            req.then(function (response) {
                var data = JSON.parse(response.data.d);
                console.log(data, 'PopListReference');
                if (data.ProcessSuccess) {
                    $scope.PopItems = data.Items;

                    $("#pagerPop").ASPSnippets_Pager({
                        ActiveCssClass: "current",
                        PagerCssClass: "pager",
                        PageIndex: data.PageIndex,
                        PageSize: data.PageSize,
                        RecordCount: data.RecordCount
                    });
                }
            }, function (data, status) {
                alert(data.statusText + ' - ' + data.data.Message);
            });
        } catch (e) {
            alert(e.message);
        }
    };

    $scope.SubmitGenerateHeader = function (st) {
        const header = {
            ID: $scope.Header.ID,
            Form_No: $scope.Header.Form_No,
            Requester_Name: $scope.Header.Requester_Name,
            Requester_Email: $scope.Header.Requester_Email,
            Trading_Partner_Code: $scope.tradingPartner.Code,
            Trading_Partner_Name: $scope.tradingPartner.Name,
            Plant_Code: $scope.plant.Code,
            Plant_Name: $scope.plant.Name,
            PPJK_Code: $scope.ppjk.Code,
            PPJK_Name: $scope.ppjk.Name,
            PPJK_Curr: $scope.ppjk.Curr,
            PPJK_Category: $scope.ppjk.Category,
            Bank_Key_ID: $scope.ppjk.Bank_Key,
            Bank_Account_No: $scope.ppjk.Bank_Account_No,
            Vendor_Code: $scope.ppjk.Code,
            Buss_Place_Code: $scope.bussPlace.Code,
            Buss_Place_Name: $scope.bussPlace.Name,
            Item_ID: $scope.Header.Item_ID,
            Expense_Type_Code: $scope.expenseType.Code,
            Expense_Type_Name: $scope.expenseType.Name,
            Approval_Status: st,
            OA_Summary_FileName: $scope.Header.OA_Summary_FileName,
            OA_Summary_Attachment: $scope.Header.OA_Summary_Attachment,
            DIID_Invoice: $scope.Header.DIID_Invoice
        };

        return header;
    };

    $scope.ValidateDetail = function () {
        let msg = "Please complete this below fields: ";
        let result = { anyError: false, message: "" };
        if ($scope.Items.length <= 0) {
            result = { anyError: true, message: "Please insert details" };
            return result;
        }
        $scope.Items.forEach(item => {
            if (item.Document_Date.length <= 0) {
                result = { anyError: true, message: msg + "\n - Document Date" };
                return;
            }

            if (item.Vendor_No.length <= 0) {
                result = { anyError: true, message: msg + "\n - Vendor No" };
                return;
            }

            if (item.Vendor_Invoice_No.length <= 0) {
                result = { anyError: true, message: msg + "\n - Vendor Invoice No" };
                return;
            }

            if (item.Condition_Code.length <= 0) {
                result = { anyError: true, message: msg + "\n - Condition" };
                return;
            } else {
                if (item.Condition_Code.includes("ZF") && item.Freight_Cost <= 0) {
                    result = { anyError: true, message: msg + "\n - Freight Cost" };
                    return;
                }
            }

            if (item.VAT_Type !== 'I0' && item.VAT_No.length <= 0) {
                result = { anyError: true, message: msg + "\n - VAT No." };
                return;
            }

            if (item.Tax_Base_Amount.length <= 0) {
                result = { anyError: true, message: msg + "\n - Tax Base Amount" };
                return;
            }

            if (item.File_Name.length <= 0) {
                result = { anyError: true, message: msg + "\n - Attachment" };
                return;
            }

        });

        return result;
    };

    $scope.SubmitGenerateDetail = function () {
        let detail = [];
        $scope.Items.forEach(it => {
            detail.push({
                No: it.No,
                ID: it.ID,
                Document_Date: it.Document_Date,
                Ref_No: it.Ref_No,
                Ref_Type: it.Ref_Type,
                BL_No: it.BL_No,
                FOB_No: it.FOB_No,
                Freight_Cost: it.Freight_Cost.replace(/,/g, ''),
                Vendor_No: it.Vendor_No,
                Vendor_Name: it.Vendor_Name,
                Vendor_Invoice_No: it.Vendor_Invoice_No,
                Condition_ID: it.Condition_ID,
                Condition_Code: it.Condition_Code,
                Condition_Name: it.Condition_Name,
                VAT_No: it.VAT_No,
                VAT_Percent: it.VAT_Percent,
                VAT_Amount: it.VAT_Amount.toString().replace(/,/g, ''),
                VAT_Type: it.VAT_Type,
                WHT_Type_Code: it.WHT_Type_Code,
                WHT_Type_Name: it.WHT_Type_Name,
                WHT_Amount: it.WHT_Amount.toString().replace(/,/g, ''),
                Tax_Base_Amount: it.Tax_Base_Amount.replace(/,/g, ''),
                Total_Amount: it.Total_Amount.toString(),
                Text: it.Text,
                File_Name: it.File_Name,
                Attachment_URL: it.Attachment_URL,
                Currency: $scope.Header.PPJK_Curr,
                Business_Place_Code: $scope.bussPlace.Code,
                Assignment: it.FOB_No,
            });
        });
        return detail;
    };

    $scope.Submit = function (st) {
        try {
            const validationHeaderResult = $scope.ValidationHeader();
            if (validationHeaderResult.anyError) {
                alert(validationHeaderResult.warningMsg);
                return;
            }
            const validationDetailResult = $scope.ValidateDetail();
            if (validationDetailResult.anyError) {
                alert(validationDetailResult.message);
                return;
            }
            const header = $scope.SubmitGenerateHeader(st);
            const detail = $scope.SubmitGenerateDetail();

            const confirmMsg = confirm('Submit ?');
            if (confirmMsg) {
                const proc = svc.svc_SaveUpdate(header, detail);
                proc.then(function (response) {
                    const data = JSON.parse(response.data.d);
                    if (data.ProcessSuccess) {
                        alert('Submitted Successfully!');
                        location.href = 'ServiceCost.aspx?ID=' + data.ID;
                    } else {
                        alert(data.InfoMessage);
                    }

                }, function (data, status) {
                    alert(data.data.Message);
                });
            }

        } catch (e) {
            alert(e.message);
        }
    };

    $scope.Approval = function () {
        try {
            console.log($scope.Outcome, 'Outcome');
            const st = $scope.Outcome;
            if (st == 0) {
                alert('Please select the outcomes');
                return;
            }
            if ($scope.ntx.Comment.length <= 0 && st == 2) {
                alert('Please specify your comments for rejecting this Service Cost');
                return;
            }
            let msg = '';
            let outcomeName = '';
            if (st == 1) {
                msg = 'Approve ?';
                outcomeName = 'Approve';
            } else if (st == 2) {
                msg = 'Reject ?';
                outcomeName = 'Reject';
            } else {
                msg = 'Revise ?';
                outcomeName = 'Revise';
            }
            $scope.ntx.FormNo = $scope.Header.Form_No;
            $scope.ntx.Outcome = st;
            $scope.ntx.Module = 'SC';
            $scope.ntx.Position_ID = $scope.Header.Pending_Approver_Role_ID;
            $scope.ntx.Transaction_ID = $scope.Header.ID;
            $scope.ntx.Item_ID = $scope.Header.Item_ID;
            $scope.ntx.OutcomeName = outcomeName;

            let detail = [];
            for (let j = 0; j < $scope.Items.length; j++) {
                const it = $scope.Items[j];
                detail.push({
                    No: it.No,
                    ID: it.ID,
                    Document_Date: it.Document_Date,
                    Ref_No: it.Ref_No,
                    Ref_Type: it.Ref_Type,
                    BL_No: it.BL_No,
                    FOB_No: it.FOB_No,
                    Freight_Cost: it.Freight_Cost.replace(/,/g, ''),
                    Vendor_No: it.Vendor_No,
                    Vendor_Name: it.Vendor_Name,
                    Vendor_Invoice_No: it.Vendor_Invoice_No,
                    Condition_ID: it.Condition_ID,
                    Condition_Code: it.Condition_Code,
                    Condition_Name: it.Condition_Name,
                    VAT_No: it.VAT_No,
                    VAT_Percent: it.VAT_Percent,
                    VAT_Amount: it.VAT_Amount.toString().replace(/,/g, ''),
                    WHT_Type_Code: it.WHT_Type_Code,
                    WHT_Type_Name: it.WHT_Type_Name,
                    WHT_Amount: it.WHT_Amount.toString().replace(/,/g, ''),
                    Tax_Base_Amount: it.Tax_Base_Amount.replace(/,/g, ''),
                    Total_Amount: it.Total_Amount,
                    Text: it.Text,
                    File_Name: it.File_Name,
                    Attachment_URL: it.Attachment_URL,
                });
            };
            const proc = svc.svc_Approval($scope.ntx, $scope.Remarks, detail, $scope.IsDocumentReceived);
            proc.then(function (response) {
                const data = JSON.parse(response.data.d);
                if (data.ProcessSuccess) {
                    location.href = '/_layouts/15/Daikin.Application/Modules/PendingTask/PendingTaskList.aspx';
                } else {
                    alert(data.InfoMessage);
                }

            }, function (data, status) {
                alert(data.data.Message);
            });
        } catch (e) {
            alert(e.message);
        }
    };

    $scope.ValidationHeader = function () {
        let anyError = false;
        let warningMsg = '';
        if ($scope.tradingPartner.Code == '') {
            anyError = true;
            warningMsg += '\n Select the Trading Partner';
        }

        if ($scope.plant.Code == '') {
            anyError = true;
            warningMsg += '\n Select the Plant';
        }

        if ($scope.expenseType.Code == '') {
            anyError = true;
            warningMsg += '\n Select the Expense Type';
        }

        if ($scope.ppjk.Code == '') {
            anyError = true;
            warningMsg += '\n Select the PPJK';
        }

        if ($scope.bussPlace.Code == '') {
            anyError = true;
            warningMsg += '\n Select the Business Place';
        }
        return { anyError, warningMsg };
    };

    $scope.GetDataByType = function (ReferenceNo) {
        $scope.showModal = 'none';

        if ($scope.ValidationHeader()) {
            return;
        }

        const proc = svc.svc_GetDataByType(ReferenceNo, $scope.Reference_Type, $scope.tradingPartner.Code, $scope.ppjk.Code);
        proc.then(function (response) {
            const data = JSON.parse(response.data.d);
            if (data.ProcessSuccess) {
                $scope.BL_No = data.Items.BL_No;
                $scope.FOB_No = data.Items.FOB_No;
                const Ref_Type = ($scope.expenseType.Name == 'INSPECTION COST') ? 'FOB' : 'BL';
                const No = $scope.Items.length + 1;
                $scope.Items.push({
                    No: No,
                    ID: 0,
                    Document_Date: '',
                    Ref_No: Ref_Type == 'FOB' ? $scope.FOB_No : $scope.BL_No,
                    Ref_Type: Ref_Type,
                    BL_No: $scope.BL_No,
                    FOB_No: $scope.FOB_No,
                    ddlVendor: $scope.ddlVendor,
                    ddlCondition: $scope.FilteredCondition,
                    Freight_Cost: '0',
                    ddlWHT: $scope.ddlWHT,
                    Vendor_No: '',
                    Vendor_Name: '',
                    Vendor_Invoice_No: '',
                    Condition_ID: '0',
                    Condition_Code: '',
                    Condition_Name: '',
                    ddlVAT: $scope.ddlVAT,
                    VAT_No: '',
                    VAT_Percent: '0',
                    VAT_Type: 'I0',
                    VAT_Amount: '0',
                    WHT_Type_Code: '',
                    WHT_Type_Name: '',
                    WHT_Amount: '0',
                    Tax_Base_Amount: '0',
                    Total_Amount: '0',
                    Text: '',
                    File_Name: '',
                    Attachment_URL: '#',
                });
            } else {
                alert(data.InfoMessage);
            }

        }, function (data, status) {
            alert(data.statusText + ' - ' + data.data.Message);
        });

    }

    $scope.AddEnter = function (keyEvent) {
        if (keyEvent.which === 13) {
            $scope.showModal = 'none';
            if ($scope.txt_ReffNo.length <= 0) {
                return;
            }
            $scope.GetDataByType($scope.txt_ReffNo);
        }

    };

    $scope.SelectReference = function (o) {
        $scope.showModal = 'none';
        $scope.Keywords = '';
        $scope.txt_ReffNo = '';
        try {
            console.log(o, 'o.Reference_No');
            $scope.GetDataByType(o.Reference_No);
        } catch (e) {
            alert(e.message);
        }
    };

    $scope.AddReference = function () {
        try {
            $scope.GetDataByType($scope.txt_ReffNo);
            $scope.txt_ReffNo = '';
        } catch (e) {
            alert(e.message);
        }
    };

    $scope.RemoveItems = function (idx) {
        const objIndex = $scope.Items.findIndex(o => o.No == idx.No);
        let confirmMsg = confirm('Remove this items ?');
        if (confirmMsg) {
            $scope.Items.splice(objIndex, 1);
            $scope.Header.Grand_Total = 0;
            for (let x = 0; x < $scope.Items.length; x++) {
                $scope.Items[x].No = x + 1;
                $scope.Header.Grand_Total += $scope.Items[x].Total_Amount;
            }
        }
    };

    $scope.OnBlurDateChange = function (o) {
        const objIndex = $scope.Items.findIndex(obj => obj.No == o.No);
        $scope.Items[objIndex].Vendor_Invoice_No = o.Vendor_Invoice_No;
        $scope.Items[objIndex].Document_Date = o.Document_Date;
    };

    $scope.OnChangeDDLExpenseType = function () {
        if ($scope.expenseType.Name == 'INSPECTION COST') {
            $scope.BtnAdd = '(+)';
            $scope.Reference_Type = 'FOB No.';
        } else {
            $scope.BtnAdd = '(+)';
            $scope.Reference_Type = 'BL No.';
        }
        $scope.LoadDDLCondition();
        $scope.LoadDDLVendorLC($scope.expenseType.Name);
    };

    $scope.LoadDDLVendorLC = function (expensetype) {
        try {
            const proc = svc.svc_LoadDDLVendorLC(expensetype);
            proc.then(function (response) {
                const data = JSON.parse(response.data.d);
                if (data.ProcessSuccess) {
                    $scope.ddlVendor = data.listMasterVendor;
                }
            }, function (data, status) {
                alert(data.data.Message);
            });

        } catch (e) {
            console.log(e.message);
        }
    };

    $scope.OnChangeDDLVendor = function (idx) {
        try {
            const objIndex = $scope.Items.findIndex(o => o.No == idx.No);
            $scope.Items[objIndex].Vendor_Name = $scope.ddlVendor.find(o => o.Code == idx.Vendor_No).Name;
        } catch (e) {
            console.log(e.message);
        }
    };

    $scope.sum = function (items, prop) {
        return items.reduce(function (a, b) {
            return a + b[prop];
        }, 0);
    };

    $scope.NormalizeFreighCost = function (item, input) {
        if (!input.Freight_Cost || input.Freight_Cost.length === 0) {
            item.Freight_Cost = "0";
            return;
        }
        input.Freight_Cost = input.Freight_Cost.replace(/,/g, '');
        item.Freight_Cost = addCommas(input.Freight_Cost);
    };

    $scope.NormalizeTaxBaseAmount = function (item, input) {
        if (!input.Tax_Base_Amount || input.Tax_Base_Amount.length === 0) {
            item.Tax_Base_Amount = '0';
            input.Tax_Base_Amount = '0';
            input.Total_Amount = '0';
            return;
        }

        input.Tax_Base_Amount = input.Tax_Base_Amount.replace(/,/g, '');
        input.Total_Amount = input.Tax_Base_Amount;
    };

    $scope.CalculateVAT = function (input, Calculate) {
        if (input.VAT_Type === "I0") {
            return {
                VAT_Amount: 0,
                VAT_Percent: 0,
                VAT_No: '',
                Total_Amount: addCommas(input.Tax_Base_Amount)
            };
        }
        const vatData = $scope.ddlVAT.find(o => o.Code == input.VAT_Type);
        const percent = vatData?.VAT_Percent || 0;
        const VAT_Amount = Calculate ? percent * Number.parseFloat(input.Tax_Base_Amount) :
            Number.parseFloat(input.VAT_Amount.replace(/,/g, ''));
        return {
            VAT_Amount,
            VAT_Percent: percent,
            VAT_No: input.VAT_No,
            Total_Amount: VAT_Amount + parseFloat(input.Tax_Base_Amount)
        };
    };

    $scope.ApplyVAT = function (item, vat) {
        item.VAT_Type = item.VAT_Type;
        item.VAT_Percent = vat.VAT_Percent;
        item.VAT_Amount = addCommas(vat.VAT_Amount);
    };

    $scope.CalculateWHT = function (input, Calculate) {
        if (input.WHT_Type_Code === '00') {
            return { WHT_Amount: 0, Name: '', Percent: 0 };
        }
        const whtData = $scope.ddlWHT.find(o => o.Code == input.WHT_Type_Code);
        if (!whtData) {
            return { WHT_Amount: 0, Name: '', Percent: 0 };
        }
        const WHT_Amount = Calculate
            ? parseFloat(input.Tax_Base_Amount) * whtData.Percentage
            : parseFloat(input.WHT_Amount.replace(/,/g, ''));
        return {
            WHT_Amount,
            Name: whtData.Name,
            Percent: whtData.Percentage
        };
    };

    $scope.ApplyWHT = function (item, wht) {
        item.WHT_Type_Code = item.WHT_Type_Code || '00';
        item.WHT_Amount = addCommas(wht.WHT_Amount);
        item.WHT_Type_Name = wht.Name;
    };

    $scope.UpdateItemTotals = function (item, vat, wht) {
        const total = vat.Total_Amount - parseFloat(wht.WHT_Amount);
        item.Total_Amount = total;
    };

    $scope.UpdateGrandTotal = function () {
        let total = 0;
        $scope.Items.forEach(x => total += parseFloat(x.Total_Amount));
        $scope.Header.Grand_Total = total;
    };

    $scope.OnChangeDDLVAT = function (i, Calculate) {
        try {
            const objIndex = $scope.items.findIndex(o => o.No == i.No);
            const item = $scope.items[objIndex];
            $scope.NormalizeFreighCost(item, i);
            $scope.NormalizeTaxBaseAmount(item, i);
            const vat = $scope.CalculateVAT(i, Calculate);
            $scope.ApplyVAT(item, vat);
            const wht = $scope.CalculateWHT(i, Calculate);
            $scope.ApplyWHT(item, wht);
            $scope.UpdateItemTotals(item, vat, wht);
            $scope.UpdateGrandTotal();
        } catch (e) {
            console.log(e.message);
        }
    };


    // $scope.OnChangeDDLVAT = function (i, Calculate) {
    //     try {
    //         const objIndex = $scope.Items.findIndex(o => o.No == i.No);
    //         if ($scope.Items[objIndex].Freight_Cost.length <= 0) {
    //             $scope.Items[objIndex].Freight_Cost = '0';
    //         } else {
    //             i.Freight_Cost = i.Freight_Cost.replace(/,/g, '');
    //             $scope.Items[objIndex].Freight_Cost = addCommas(i.Freight_Cost);
    //         }

    //         if ($scope.Items[objIndex].Tax_Base_Amount.length <= 0) {
    //             $scope.Items[objIndex].Tax_Base_Amount = '0';
    //             i.Tax_Base_Amount = '0';
    //         } else {
    //             i.Tax_Base_Amount = i.Tax_Base_Amount.replace(/,/g, '');
    //             i.Total_Amount = i.Tax_Base_Amount;
    //         }

    //         var VAT_Amount = 0;
    //         var Tax_Base_Amount = 0;
    //         var Total = i.Total_Amount.replace(/,/g, '');

    //         if (i.VAT_Type == 'I0') {
    //             $scope.Items[objIndex].VAT_Amount = '0';
    //             $scope.Items[objIndex].VAT_Percent = '0';
    //             $scope.Items[objIndex].VAT_No = '';

    //             $scope.Items[objIndex].Total_Amount = addCommas(i.Tax_Base_Amount);

    //         } else {
    //             var Percent = $scope.ddlVAT.find(o => o.Code == i.VAT_Type).VAT_Percent;
    //             if (Calculate) {
    //                 VAT_Amount = Percent * parseFloat(i.Tax_Base_Amount);
    //             } else {
    //                 VAT_Amount = i.VAT_Amount.replace(/,/g, '');
    //             }


    //             $scope.Items[objIndex].VAT_Percent = Percent;
    //             Tax_Base_Amount = parseFloat(i.Tax_Base_Amount);

    //             Total = parseFloat(VAT_Amount) + Tax_Base_Amount;
    //         }
    //         $scope.Items[objIndex].VAT_Type = i.VAT_Type;

    //         var WHT_Amount = 0;
    //         var WHT_Name = '';
    //         if (i.WHT_Type_Code !== '00') {
    //             var WHT_Percent = $scope.ddlWHT.find(o => o.Code == i.WHT_Type_Code);
    //             if (WHT_Percent !== undefined) {
    //                 if (Calculate) {
    //                     WHT_Amount = i.Tax_Base_Amount * WHT_Percent.Percentage;
    //                 } else {
    //                     WHT_Amount = i.WHT_Amount.replace(/,/g, '');
    //                 }
    //                 WHT_Name = WHT_Percent.Name;
    //             }
    //         }
    //         $scope.Items[objIndex].VAT_Amount = addCommas(VAT_Amount);
    //         $scope.Items[objIndex].Tax_Base_Amount = addCommas(i.Tax_Base_Amount);
    //         $scope.Items[objIndex].Total_Amount = Total - parseFloat(WHT_Amount);
    //         $scope.Items[objIndex].WHT_Type_Code = i.WHT_Type_Code.length <= 0 ? '00' : i.WHT_Type_Code;
    //         $scope.Items[objIndex].WHT_Amount = addCommas(WHT_Amount);
    //         $scope.Items[objIndex].WHT_Type_Name = WHT_Name;

    //         Total = 0;
    //         for (var x in $scope.Items) {
    //             Total += parseFloat($scope.Items[x].Total_Amount);
    //         }
    //         $scope.Header.Grand_Total = Total;
    //     } catch (e) {
    //         console.log(e.message);
    //     }

    // };

    $scope.OnChangeDDLPPJK = function () {
        try {
            $scope.Header.PPJK_Curr = $scope.ppjk.Curr;
            $scope.Header.PPJK_Category = $scope.ppjk.Category;
            $scope.Header.Bank_Account_No = $scope.ppjk.Bank_Account_No;
        } catch (e) {
            console.log(e.message);
        }
    };

    $scope.OnChangeDDLCondition = function (i) {
        try {
            console.log(i.Condition_Code, 'Condition_Code');
            console.log(i.Condition_ID, 'Condition_ID');
            console.log(i.Condition_Name, 'Condition_Name');
            var objIndex = $scope.Items.findIndex(o => o.No == i.No);
            $scope.Items[objIndex].Condition_Name = $scope.ddlCondition.find(o => o.Code == i.Condition_Code).Name;
            $scope.Items[objIndex].Condition_ID = $scope.ddlCondition.find(o => o.Code == i.Condition_Code).ID;
            $scope.Items[objIndex].Condition_Code = i.Condition_Code;
            $scope.Items[objIndex].Text = i.FOB_No + ' ' + $scope.Items[objIndex].Condition_Name;
            $scope.Items[objIndex].Freight_Cost = '0';
            console.log($scope.Items[objIndex], '$scope.Items[objIndex]');

        } catch (e) {
            console.log(e.message);
        }
    };

    $scope.LoadDDL = function () {
        try {
            var proc = svc.svc_LoadDDL();
            proc.then(function (response) {
                var data = JSON.parse(response.data.d);
                console.log(data, 'LoadDDL()');
                if (data.ProcessSuccess) {
                    $scope.ddlTradingPartner = data.listTradingPartner;
                    $scope.tradingPartner = data.listTradingPartner[0];
                    $scope.Val_Message = data.Val_Message;
                    $scope.ddlPlant = data.listPlant;
                    $scope.plant = $scope.ddlPlant[0];

                    $scope.ddlExpenseType = data.listExpenseType;
                    $scope.expenseType = data.listExpenseType[0];
                    $scope.ddlBussPlace = data.listBussPlace;
                    $scope.bussPlace = data.listBussPlace[0];
                    $scope.ddlPPJK = data.listPPJK;
                    $scope.ppjk = data.listPPJK[0];
                    $scope.ddlWHT = data.listWHT;
                    $scope.ddlCondition = data.listConditionSC;

                    $scope.ddlVAT = data.listVAT;
                    $scope.IsRequestor = true;
                    $scope.Header.Requester_Name = data.CurrentLoginName;
                    $scope.Header.Requester_Email = data.CurrentLoginEmail;

                } else {
                    alert(data.InfoMessage);
                }

            }, function (data, status) {
                alert(data.data.Message);
            });
        } catch (e) {
            alert(e.message);
        }

    };

    $scope.SelectFile = function (file, obj) {
        obj.File_Name = file[0].name;
        $scope.SelectedFile = file[0];
        $scope.Upload();
        var objIndex = $scope.Items.findIndex((o => o.No == obj.No));
        $scope.Items[objIndex].File_Name = obj.File_Name;
        console.log($scope.Items);
    };

    $scope.RemoveAttachment = function (obj) {
        if ($scope.Header.Pending_Approver_Role_ID == '' || $scope.Header.Approval_Status == '5') {
            var dialogRemove = confirm('Remove Attachment ?');
            if (dialogRemove) {
                var objIndex = $scope.Items.findIndex((o => o.No == obj.No));
                obj.File_Name = '';
                $scope.Items[objIndex].File_Name = obj.File_Name;
            }
        }
    };

    $scope.Upload = function () {
        $scope.uploadf = true;
        var files = $scope.SelectedFile;
        console.log(files);
        if (files.name.length > 0) {
            Upload.upload({
                url: '/_layouts/15/Daikin.Application/Handler/UploadHandler.ashx',
                data: { file: files },
            }).then(function (response) {
                $timeout(function (response) {
                    $scope.SelectedDoc = files.name;
                    $scope.SelectedFile = '';

                });
            }, function (response) {
                if (response.status > 0) {
                    alert(response.status + ': ' + response.data);
                }
            });
        }
    };

    $scope.FilteredCondition = [];

    $scope.LoadDDLCondition = function () {
        $scope.FilteredCondition = [];
        $scope.FilteredCondition.push({
            Code: '',
            Name: 'Please Select',
            Selected: false,
            ID: '0',
        });
        for (var x = 0; x < $scope.ddlCondition.length; x++) {
            var item = $scope.ddlCondition[x];
            if (item.Title !== '') {
                console.log(item.Title, 'Item.Title');
                if ($scope.expenseType.Name == 'INSPECTION COST') {
                    if (item.Title.includes('ZSG')) {
                        $scope.FilteredCondition.push(item);
                    }
                } else {
                    if (!item.Title.includes('ZSG')) {
                        $scope.FilteredCondition.push(item);
                    }

                }
            }
        }
        console.log($scope.FilteredCondition, 'filtered ddl condition');
    };

    $scope.ProcessRemarks = function (remarks) {
        $scope.Remarks = remarks || [];
        $scope.Remarks.forEach(r => {
            r.Outcome = (r.Outcome === "True");
        });
    };

    $scope.ProcessFlags = function (data) {
        $scope.IsCurrentApprover = data.IsCurrentApprover;
        $scope.IsTaxVerifier = data.IsTaxVerifier;
        $scope.IsReceiverDocs = data.IsReceiverDocs;
        const h = data.header;
        $scope.IsRequestor = (h.Approval_Status === "5") && data.IsRequestor;
        $scope.showWHT = data.IsTaxVerifier || h.Approval_Status === "7";

        const isRevise = (h.Approval_Status === "5");
        $scope.CanEdit = isRevise;
        $scope.DisableActivate = !isRevise;
        $scope.Val_Message = isRevise ? "" : undefined;

        $scope.ApproverLog();
    };

    $scope.ProcessHeader = function (h) {
        $scope.Header = {
            ...$scope.Header,
            ...h,
            OA_Summary_Attachment: h.OA_Summary_Attachment,
            OA_Summary_FileName: h.OA_Summary_FileName,
            Document_Received: h.Document_Received,
            Grand_Total: h.Grand_Total
        };
        $scope.IsDocumentReceived = (h.Document_Received !== "0");
    };

    $scope.ProcessDropDownValues = function (data) {
        const h = data.header;
        // Trading partner
        $scope.ddlTradingPartner = data.listTradingPartner;
        $scope.tradingPartner = $scope.ddlTradingPartner.find(o => o.Code == h.Trading_Partner_Code);
        // Plant
        $scope.ddlPlant = data.listPlant;
        $scope.plant = $scope.ddlPlant.find(o => o.Code == h.Plant_Code);
        // Expense type
        $scope.ddlExpenseType = data.listExpenseType;
        $scope.expenseType = $scope.ddlExpenseType.find(o => o.Name == h.Expense_Type_Name);
        // Business place
        $scope.ddlBussPlace = data.listBussPlace;
        $scope.bussPlace = $scope.ddlBussPlace.find(o => o.Code == h.Buss_Place_Code);
        // PPJK
        $scope.ddlPPJK = data.listPPJK;
        $scope.ppjk = $scope.ddlPPJK.find(o => o.Code == h.PPJK_Code);

        // detail dropdowns
        $scope.ddlVendor = data.listMasterVendor;
        $scope.ddlWHT = data.listWHT;
        $scope.ddlCondition = data.listConditionSC;
        $scope.ddlVAT = data.listVAT;
    };

    $scope.ProcessDetails = function (details) {
        if (!details) return;
        $scope.Items = details.map(d => ({
            No: d.No,
            ID: d.ID,
            Document_Date: d.Document_Date,
            Ref_No: d.Ref_No,
            Ref_Type: d.Ref_Type,
            BL_No: d.BL_No,
            FOB_No: d.FOB_No,

            // Dropdowns
            ddlVendor: $scope.ddlVendor,
            ddlCondition: $scope.ddlCondition,
            ddlWHT: $scope.ddlWHT,
            ddlVAT: $scope.ddlVAT,

            Freight_Cost: addCommas(d.Freight_Cost),

            Vendor_No: d.Vendor_No,
            Vendor_Name: d.Vendor_Name,
            Vendor_Invoice_No: d.Vendor_Invoice_No,

            Condition_ID: d.Condition_ID,
            Condition_Code: d.Condition_Code,
            Condition_Name: d.Condition_Name,

            VAT_No: d.VAT_No,
            VAT_Type: d.VAT_Type,
            VAT_Percent: d.VAT_Percent,
            VAT_Amount: d.VAT_Amount,

            WHT_Type_Code: d.WHT_Type_Code,
            WHT_Type_Name: d.WHT_Type_Name,
            WHT_Amount: d.WHT_Amount,

            Tax_Base_Amount: addCommas(d.Tax_Base_Amount),
            Total_Amount: d.Total_Amount,
            Text: d.Text,
            File_Name: d.File_Name,
            Attachment_URL: d.Attachment_URL
        }));
    };

    $scope.GetData = function () {
        try {
            const id = GetQueryString()["ID"];
            if (!id) {
                $scope.LoadDDL();
                return;
            }
            svc.svc_GetData(id).then(response => {
                const data = JSON.parse(response.data.d);
                if (!data.ProcessSuccess) return;
                $scope.ProcessRemarks(data.remarks);
                $scope.ProcessFlags(data);
                $scope.ProcessHeader(data.Header);
                $scope.ProcessDropDownValues(data);
                $scope.ProcessDetails(data.Details);
            });
        } catch (e) {
            alert(e.message);
        }
    };

    // $scope.GetData = function () {
    //     try {
    //         var id = GetQueryString()['ID']; //Nintex No
    //         if (id != undefined) {
    //             var proc = svc.svc_GetData(id);
    //             proc.then(function (response) {
    //                 var data = JSON.parse(response.data.d);
    //                 console.log('Get data', data);
    //                 if (data.ProcessSuccess) {
    //                     var h = data.Header;
    //                     var d = data.Details;
    //                     $scope.Remarks = data.Remarks;

    //                     for (var x = 0; x < data.Remarks.length; x++) {
    //                         if (data.Remarks[x].Outcome == 'True') {
    //                             $scope.Remarks[x].Outcome = true;
    //                         }
    //                     }
    //                     $scope.IsCurrentApprover = data.IsCurrentApprover;
    //                     console.log($scope.IsCurrentApprover, 'IsCurrentApprover');

    //                     $scope.Header.OA_Summary_Attachment = h.OA_Summary_Attachment;
    //                     $scope.Header.OA_Summary_FileName = h.OA_Summary_FileName;
    //                     $scope.Header.ID = h.ID;
    //                     $scope.Header.Form_No = h.Form_No;
    //                     $scope.Header.Requester_Email = h.Requester_Email;
    //                     $scope.Header.Requester_Name = h.Requester_Name;
    //                     $scope.Header.Approval_Status = h.Approval_Status;
    //                     $scope.Header.Approval_Status_Name = h.Approval_Status_Name;
    //                     $scope.Header.PPJK_Category = h.PPJK_Category;
    //                     $scope.Header.PPJK_Curr = h.PPJK_Curr;
    //                     $scope.Header.Grand_Total = h.Grand_Total;
    //                     $scope.Header.Item_ID = h.Item_ID;
    //                     $scope.Header.Bank_Account_No = h.Bank_Account_No;
    //                     $scope.IsDocumentReceived = h.Document_Received == '0' ? false : true;
    //                     $scope.Header.Pending_Approver_Role_ID = h.Pending_Approver_Role_ID;
    //                     $scope.Header.DIID_Invoice = h.DIID_Invoice;
    //                     if ($scope.Header.Approval_Status == '5' && data.IsRequestor) {
    //                         $scope.IsRequestor = true;
    //                     }
    //                     else {
    //                         $scope.IsRequestor = false;
    //                     }
    //                     if ($scope.Header.Approval_Status == '5') { //Revise
    //                         $scope.Val_Message = '';
    //                     }

    //                     console.log('isRequestor', data.IsRequestor);
    //                     console.log('Header', $scope.Header);

    //                     $scope.IsTaxVerifier = data.IsTaxVerifier;
    //                     $scope.IsReceiverDocs = data.IsReceiverDocs;
    //                     if (data.IsTaxVerifier || h.Approval_Status == '7') {
    //                         $scope.showWHT = true;
    //                     } else {
    //                         $scope.showWHT = false;
    //                     }

    //                     $scope.ApproverLog();

    //                     if (h.Approval_Status == '5') {
    //                         $scope.CanEdit = true;
    //                         $scope.DisableActivate = false;
    //                     } else {
    //                         $scope.CanEdit = false;
    //                         $scope.DisableActivate = true;
    //                     }


    //                     $scope.ddlTradingPartner = data.listTradingPartner;
    //                     $scope.tradingPartner = data.listTradingPartner.find(o => o.Code == h.Trading_Partner_Code);

    //                     $scope.ddlPlant = data.listPlant;
    //                     $scope.plant = data.listPlant.find(o => o.Code == h.Plant_Code);

    //                     $scope.ddlExpenseType = data.listExpenseType;
    //                     $scope.expenseType = data.listExpenseType.find(o => o.Name == h.Expense_Type_Name);
    //                     $scope.ddlBussPlace = data.listBussPlace;
    //                     $scope.bussPlace = data.listBussPlace.find(o => o.Code == h.Buss_Place_Code);
    //                     $scope.ddlPPJK = data.listPPJK;
    //                     $scope.ppjk = data.listPPJK.find(o => o.Code == h.PPJK_Code);


    //                     //Details
    //                     $scope.ddlVendor = data.listMasterVendor;
    //                     $scope.ddlWHT = data.listWHT;
    //                     $scope.ddlCondition = data.listConditionSC;
    //                     $scope.ddlVAT = data.listVAT;

    //                     for (x = 0; x < d.length; x++) {


    //                         $scope.Items.push({
    //                             No: d[x].No,
    //                             ID: d[x].ID,
    //                             Document_Date: d[x].Document_Date,
    //                             Ref_No: d[x].Ref_No,
    //                             Ref_Type: d[x].Ref_Type,
    //                             BL_No: d[x].BL_No,
    //                             FOB_No: d[x].FOB_No,
    //                             ddlVendor: $scope.ddlVendor,
    //                             ddlCondition: $scope.ddlCondition,
    //                             Freight_Cost: addCommas(d[x].Freight_Cost),
    //                             ddlWHT: $scope.ddlWHT,
    //                             Vendor_No: d[x].Vendor_No,
    //                             Vendor_Name: d[x].Vendor_Name,
    //                             Vendor_Invoice_No: d[x].Vendor_Invoice_No,
    //                             Condition_ID: d[x].Condition_ID,
    //                             Condition_Code: d[x].Condition_Code,
    //                             Condition_Name: d[x].Condition_Name,
    //                             ddlVAT: $scope.ddlVAT,
    //                             VAT_No: d[x].VAT_No,
    //                             VAT_Type: d[x].VAT_Type,
    //                             VAT_Percent: d[x].VAT_Percent,
    //                             VAT_Amount: d[x].VAT_Amount,
    //                             WHT_Type_Code: d[x].WHT_Type_Code,
    //                             WHT_Type_Name: d[x].WHT_Type_Name,
    //                             WHT_Amount: d[x].WHT_Amount,
    //                             Tax_Base_Amount: addCommas(d[x].Tax_Base_Amount),
    //                             Total_Amount: d[x].Total_Amount,
    //                             Text: d[x].Text,
    //                             File_Name: d[x].File_Name,
    //                             Attachment_URL: d[x].Attachment_URL,
    //                         });
    //                     }

    //                     console.log($scope.Items, 'Get Data Items');

    //                 }
    //             });

    //         } else {
    //             $scope.LoadDDL();
    //         }

    //     } catch (e) {
    //         alert(e.message);
    //     }
    // };

    $scope.Close = function () {
        location.href = 'List.aspx';
    }

    $scope.GetData();

});