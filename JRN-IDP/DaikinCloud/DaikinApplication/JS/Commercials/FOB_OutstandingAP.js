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

        var param = {
            Form_No: Form_No,
            Module_Code: 'M010',
            Transaction_ID: Transaction_ID
        }

        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/Master.asmx/GetHistoryLog",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }

    this.svc_ListOutstandingAP = function (PageIndex, DueOn, TradingPartnerCode, Curr) {
        var param = {
            PageIndex: PageIndex,
            DueOn: DueOn,
            TradingPartnerCode: TradingPartnerCode,
            Curr: Curr,
        };

        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/Commercials.asmx/ListOutstandingAP",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }
    this.svc_SaveUpdate = function (h, listDetail, listRemarks) {
        var param = {
            h: h,
            listDetail: listDetail,
            listRemarks: listRemarks,
        };
        console.log(param);

        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/Commercials.asmx/SaveFOB",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }

    this.svc_GetData = function (ID, PageIndex) {
        var param = {
            Form_No: ID,
            PageIndex: PageIndex,
        };

        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/Commercials.asmx/GetDataFOB",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }

    this.svc_Approval = function (ntx, IsDocumentReceived, listRemarks) {
        var param = {
            ntx: ntx,
            IsDocumentReceived: IsDocumentReceived,
            listRemarks: listRemarks,
        };

        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/Commercials.asmx/FOBApproval",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }

});

app.controller('ctrl', function ($scope, svc, Upload, $timeout) {
    $scope.Items = [];
    $scope.IsRequestor = true;
    $scope.IsCurrentApprover = false;
    $scope.SelectedFile = '';
    $scope.uploadf = false;
    $scope.ItemSelected = [];
    $scope.Remarks = [];
    $scope.IsDocumentReceived = false;
    $scope.Outcome = 0;
    $scope.ddlCurrency = [];
    $scope.Curr = {};
    $scope.ddlPlant = [];
    $scope.Plant = {};
    $scope.colspan = 7;
    $scope.TotalCurrPerPage = 0;
    $scope.TotalLocalCurrPerPage = 0;
    var TotalChecked = 0;

    $("body").on("click", ".Pager .page", function () {
        var pg = parseInt($(this).attr('page'));
        $scope.GetData(pg);
    });

    $scope.Excel = function () {
        location.href = 'https://sp3.daikin.co.id:8443/_layouts/15/Daikin.WebApps/Report_SPDEV.aspx?SP=usp_FOBHeader_ExportToExcel&RDLC=FOB&DataSet=FOBDS' +
            '&export=EXCELOPENXML&download=true&ReportName=FOB&Form_No=' + $scope.Header.Form_No;
    };

    $scope.UpdateLogRemarks = function (idx) {
        var objIndex = $scope.Remarks.findIndex(o => o.ID == idx.ID);
        $scope.Remarks[objIndex].Outcome = idx.Outcome;
        $scope.Remarks[objIndex].Reason_Rejection = idx.Outcome == true ? '' : idx.Reason_Rejection;
    };


    $scope.ApproverLog = function () {
        var proc = svc.svc_ListLog($scope.Header.Form_No, $scope.Header.ID);
        proc.then(function (response) {
            var data = JSON.parse(response.data.d);

            if (data.ProcessSuccess) {
                $scope.Logs = data.Logs;
            } else {
                alert(data.InfoMessage);
            }
        }, function (data, status) {
            console.log(data.statusText + ' - ' + data.data.Message);
        });
    };


    $scope.Close = function () {
        location.href = 'FOB.aspx';
    }

    $scope.checkAll = false;
    $scope.Val_Message = '';
    $scope.Header = {
        Form_No: '',
        Requester_Name: '',
        Requester_Email: '',
        ID: 0,
        Factory: '',
        FactoryCode: '',
        Grand_Total: 0,
        Grand_Total_Curr: 0,
        Due_On: '',
        Pending_Approver_Role_ID: '',
        Currency: '',
        Plant_Code: '',
        Plant_Name: '',
        OA_Summary_Attachment: '',
        OA_Summary_FileName: '',
    }

    $scope.ntx = {
        FormNo: '',
        Comment: '',
        Outcome: 0,
        Module: 'FOB',
    }

    $scope.selectAll = function () {
        $scope.ItemSelected = [];
        TotalChecked = 0;

        for (let x in $scope.Items) {
            //$scope.Items[x].check = $scope.Items[x].check ? false : true;
            if (!$scope.checkAll) {
                $scope.Items[x].check = true;
                $scope.ItemSelected.push($scope.Items[x]);
                TotalChecked++;
            } else {
                $scope.Items[x].check = false;
                var objIndex = $scope.ItemSelected.findIndex(o => o.Document_No == $scope.Items[x].Document_No);
                $scope.ItemSelected.pop(objIndex, 1);
                TotalChecked--;
            }
        }
        $scope.Header.Grand_Total = $scope.sum($scope.ItemSelected, 'Amount');
        $scope.Grand_Total_Local = $scope.sum($scope.ItemSelected, 'Amount_In_Local_Curr');
    }

    $scope.DistinctItemSelected = [];

    $scope.updateSelection = function (o) {
        if (o.check == true) {
            o.check = false;
            TotalChecked--;
            var filteredArr = $scope.ItemSelected.filter(function (el) {
                return el.Document_No !== o.Document_No;
            });
            $scope.ItemSelected = filteredArr;

        } else {
            o.check = true;
            $scope.ItemSelected.push(o);
            $scope.DistinctItemSelected = [... new Set($scope.ItemSelected.map(x => x.Currency))];
            TotalChecked++;
        }
        $scope.Header.Grand_Total = $scope.sum($scope.ItemSelected, 'Amount');
        $scope.Grand_Total_Local = $scope.sum($scope.ItemSelected, 'Amount_In_Local_Curr');
    }

    $scope.isSelectedAll = function () {
        return $scope.Items.length === $scope.ItemSelected.length;
    }

    $scope.Submit = function (Approval_Status) {
        try {
            if ($scope.ItemSelected.length <= 0) {
                alert('Please select any data first!');
                return;
            }
            if ($scope.Plant.Code == '') {
                alert('Please Select the Plant');
                return;
            }
            if ($scope.Curr.Code == '') {
                alert('Please Select the Currency');
                return;
            }
            if ($scope.Header.Factory.length <= 0) {
                alert('Factory should be defined!');
                return;
            }

            var isValid = true;
            var strMessage = 'Please upload an attachment for this below document No. :\n';
            for (var c = 0; c < $scope.ItemSelected.length; c++) {
                if (IsEmpty($scope.ItemSelected[c].File_Name)) {
                    isValid = false;
                    strMessage += $scope.ItemSelected[c].Document_No + '\n';
                }
            }
            if (!isValid) {
                alert(strMessage);
                return;
            }

            if ($scope.Header.OA_Summary_FileName.length <= 0) {
                alert('Please upload OA Summary Attachments');
                return;
            }

            $scope.Header.Currency = $scope.Curr.Code;
            var conf = confirm('Submit ?');
            if (conf) {
                $scope.Header.Approval_Status = Approval_Status;
                var req = svc.svc_SaveUpdate($scope.Header, $scope.ItemSelected, $scope.Remarks);
                req.then(function (response) {
                    var data = JSON.parse(response.data.d);
                    if (data.ProcessSuccess) {
                        location.href = '/_layouts/15/Daikin.Application/Commercials/OutstandingAP.aspx?ID=' + data.ID;
                        alert("Submitted Successfully");
                    }
                    else {
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

    $scope.sumByCurrency = function (Curr) {
        var total = 0;
        for (let count = 0; count < $scope.ItemSelected.length; count++) {
            if ($scope.ItemSelected[count].Currency == Curr)
                total += $scope.ItemSelected[count].Amount;
        }
        return total;
    };

    $scope.SelectFile = function (file, obj) {
        obj.File_Name = file[0].name;
        $scope.SelectedFile = file[0];
        $scope.Upload();
        var objIndex = $scope.ItemSelected.findIndex((o => o.Document_No == obj.Document_No));
        $scope.ItemSelected[objIndex].File_Name = obj.File_Name;
    };

    $scope.RemoveAttachment = function (obj) {
        if ($scope.Header.Pending_Approver_Role_ID == '' || $scope.Header.Approval_Status == '5') {
            var dialogRemove = confirm('Remove Attachment ?');
            if (dialogRemove) {
                var objIndex = $scope.ItemSelected.findIndex((o => o.Document_No == obj.Document_No));
                obj.File_Name = '';
                $scope.ItemSelected[objIndex].File_Name = obj.File_Name;
                console.log($scope.ItemSelected)
            }
        }
    }

    $scope.SelectFileOA = function (file) {
        $scope.Header.OA_Summary_FileName = file[0].name;
        $scope.SelectedFile = file[0];
        $scope.Upload();
    }

    $scope.RemoveOA = function () {
        console.log('$scope.Header.Approval_Status: ', $scope.Header.Approval_Status);
        if ($scope.Header.OA_Summary_Attachment == '' || $scope.Header.Approval_Status !== '4') {
            var confirmMsg = confirm('Remove OA Summary Attachment ?');
            if (confirmMsg) {
                $scope.Header.OA_Summary_Attachment = '';
                $scope.Header.OA_Summary_FileName = '';
            }
        }
    }

    $scope.Upload = function () {
        $scope.uploadf = true;
        var files = $scope.SelectedFile;
        if (files.name.length > 0) {
            Upload.upload({
                url: '/_layouts/15/Daikin.Application/Handler/UploadHandler.ashx',
                data: { file: files },
                //data: {
                //    files: files
                //}
            }).then(function (response) {
                $timeout(function (response) {
                    //console.log('response upload: ', response);
                    $scope.SelectedDoc = files.name;
                    $scope.SelectedFile = '';

                });
            }, function (response) {
                if (response.status > 0) {
                    alert(response.status + ': ' + response);
                }
            });
        }
    };

    $scope.sum = function (items, prop) {
        return items.reduce(function (a, b) {
            return a + b[prop];
        }, 0);
    };

    //$scope.travelerTotal = $scope.sum($scope.traveler, 'Amount');

    $scope.ListOutstandingAP = function (page, DueOn, FactoryCode, Curr) {
        if (!page)
            page = 1;

        var proc = svc.svc_ListOutstandingAP(page, DueOn, FactoryCode, Curr);
        proc.then(function (response) {
            var data = JSON.parse(response.data.d);
            console.log(data);
            if (data.ProcessSuccess) {
                $scope.Items = data.items;
                $scope.Header.Requester_Name = data.CurrentLogin;
                $scope.Header.Requester_Email = data.CurrentLoginEmail;
                $scope.Val_Message = data.Val_Message;
                if ($scope.ddlCurrency.length <= 0) {
                    $scope.ddlCurrency = data.listCurrency;
                    $scope.Curr = data.listCurrency[0];
                }
                console.log("Header: ", $scope.Header);
                $scope.ddlPlant = data.listPlant;
                $scope.Plant = data.listPlant[0];

                for (var j = 0; j < $scope.ItemSelected.length; j++) {
                    for (var k = 0; k < $scope.Items.length; k++) {
                        if ($scope.ItemSelected[j].Document_No == $scope.Items[k].Document_No) {
                            $scope.Items[k].check = true;
                            $scope.Items[k].File_Name = $scope.ItemSelected[j].File_Name;
                            break;
                        }
                    }
                }

                $(".Pager").ASPSnippets_Pager({
                    ActiveCssClass: "current",
                    PagerCssClass: "pager",
                    PageIndex: page,
                    PageSize: 10,
                    RecordCount: data.RecordCount
                });
                //$scope.Total = $scope.sum(data.items, 'Amount_In_Local_Curr');
                //$scope.Header.Grand_Total = data.Grand_Total;
            } else {
                alert(data.InfoMessage);
            }

        }, function (data, status) {
            alert(data.data.Message);
        });
    }

    $scope.IsReceiverDocs = false;

    $scope.GetData =
        function (page) {
            var id = GetQueryString()['ID'];
            if (id !== undefined) {
                var proc = svc.svc_GetData(id, page);
                proc.then(function (response) {
                    var data = JSON.parse(response.data.d);
                    if (data.ProcessSuccess) {
                        console.log(data);
                        $scope.Header = data.Header;
                        $scope.Items = data.Details;
                        $scope.Remarks = data.Remarks;

                        for (var x = 0; x < data.Remarks.length; x++) {
                            if (data.Remarks[x].Outcome == 'True') {
                                $scope.Remarks[x].Outcome = true;
                            }
                        }

                        $scope.ItemSelected = $scope.Items;
                        $scope.DistinctItemSelected = [... new Set($scope.Items.map(x => x.Currency))];
                        //$scope.Header.Grand_Total = $scope.sum($scope.ItemSelected, 'Amount');
                        //$scope.Grand_Total_Local = $scope.sum($scope.ItemSelected, 'Amount_In_Local_Curr')

                        $scope.TotalCurrPerPage = data.TotalCurr;
                        $scope.TotalLocalCurrPerPage = data.TotalLocalCurr;

                        $scope.IsRequestor = data.IsRequestor;
                        $scope.IsCurrentApprover = data.IsCurrentApprover;
                        $scope.IsDocumentReceived = data.Header.Document_Received == '1' ? true : false;
                        $scope.IsReceiverDocs = data.Header.Pending_Approver_Role_ID == '23' ? true : false;
                        $(".Pager").ASPSnippets_Pager({
                            ActiveCssClass: "current",
                            PagerCssClass: "pager",
                            PageIndex: page,
                            PageSize: 10,
                            RecordCount: data.RecordCount
                        });

                        $scope.ApproverLog();
                    } else {
                        alert(data.InfoMessage);
                    }

                }, function (data, status) {
                    alert(data.Message);
                });
            } else {
                var Factory = decodeURIComponent(GetQueryString()['tp']);
                var FactoryCode = decodeURIComponent(GetQueryString()['tpc']);
                var DueOn = decodeURIComponent(GetQueryString()['don']);
                var DueOnCode = decodeURIComponent(GetQueryString()['doc']);
                var Curr = decodeURIComponent(GetQueryString()['curr']);

                $scope.Header.Factory = Factory;
                $scope.Header.FactoryCode = FactoryCode;
                $scope.Header.Due_On = DueOn;
                $scope.Header.Curr = Curr;
                $scope.Header.Plant_Code = $scope.Plant.Code;
                $scope.Header.Plant_Name = $scope.Plant.Name;


                $scope.ListOutstandingAP(page, DueOnCode, FactoryCode, Curr);
            }

        }

    $scope.OnChangeDDLCurr = function () {
        var Factory = decodeURIComponent(GetQueryString()['tp']);
        var FactoryCode = decodeURIComponent(GetQueryString()['tpc']);
        var DueOn = decodeURIComponent(GetQueryString()['don']);
        var DueOnCode = decodeURIComponent(GetQueryString()['doc']);
        var Curr = $scope.Curr.Code;
        var Plant_Code = $scope.Plant.Code;
        var Plant_Name = $scope.Plant.Name;

        $scope.Header.Factory = Factory;
        $scope.Header.FactoryCode = FactoryCode;
        $scope.Header.Due_On = DueOn;
        $scope.Header.Currency = Curr;
        $scope.Header.Plant_Code = Plant_Code;
        $scope.Header.Plant_Name = Plant_Name;


        $scope.ListOutstandingAP(1, DueOnCode, FactoryCode, Curr);
        $scope.colspan = 8;
    }

    $scope.onChangeDDLPlant = function () {
        const { Code, Name } = $scope.Plant;

        $scope.Header.Plant_Code = Code;
        $scope.Header.Plant_Name = Name;
    }

    $scope.Approval = function () {
        try {
            var st = $scope.Outcome;
            if (st == 0) {
                alert('Please select the outcomes');
                return;
            }
            if ($scope.ntx.Comment.length <= 0 && st == 2) {
                alert('Please specify your comments for rejecting this FOB');
                return;
            }
            var msg = '';
            var outcomeName = '';
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

            //var confirmApprove = confirm(msg);

            //if (confirmApprove) {
            $scope.ntx.FormNo = $scope.Header.Form_No;
            $scope.ntx.Outcome = st;
            $scope.ntx.Module = 'FOB';
            $scope.ntx.Transaction_ID = $scope.Header.ID;
            $scope.ntx.Position_ID = $scope.Header.Pending_Approver_Role_ID;
            $scope.ntx.Item_ID = $scope.Header.Item_ID;
            $scope.ntx.OutcomeName = outcomeName;

            var proc = svc.svc_Approval($scope.ntx, $scope.IsDocumentReceived, $scope.Remarks);
            proc.then(function (response) {
                var data = JSON.parse(response.data.d);
                if (data.ProcessSuccess) {
                    location.href = '/_layouts/15/Daikin.Application/Modules/PendingTask/PendingTaskList.aspx';
                } else {
                    console.log(data.InfoMessage);
                    alert(data.InfoMessage);
                }

            }, function (data, status) {
                alert(data.data.Message);
            });
            //}

        } catch (e) {
            alert(e.message);
        }

    }

    $scope.GetData(1);

});