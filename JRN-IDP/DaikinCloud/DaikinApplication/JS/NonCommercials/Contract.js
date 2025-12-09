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
    }
});

app.directive("monthyears", function () {
    return {
        restrict: "A",
        require: "ngModel",
        link: function (scope, elem, attrs, ctrl) {
            var updateModel = function (dateText) {
                scope.$apply(function () {
                    ctrl.$setViewValue(dateText);
                });
            };
            var options = {
                changeMonth: true,
                changeYear: true,
                dateInput: false,
                dateFormat: 'M yy',
                onClose: function(dateText, inst) { 
                    $(this).datepicker('setDate', new Date(inst.selectedYear, inst.selectedMonth, 1));
                },
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

app.directive('format', ['$filter', function ($filter) {
    return {
        require: '?ngModel',
        link: function (scope, elem, attrs, ctrl) {
            if (!ctrl) return;

            ctrl.$formatters.unshift(function (a) {
                return $filter(attrs.format)(ctrl.$modelValue)
            });

            elem.bind('focus', (event) => {
                // return elem.val('')
            });

            ctrl.$parsers.unshift( function (viewValue) {
                var plainNumber = viewValue.replace(/[^\d|\-+|\.+]/g,'')
                elem.val($filter(attrs.format)(plainNumber))
                return plainNumber
            })
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
    this.svc_ContractGetApproverLogByID = function (id) {

        var param = {
            Form_No: id,
            Module_Code: 'M014',
            Transaction_ID: 0
        }

        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/Master.asmx/GetHistoryLog",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }

    this.svc_GetBranches = function () {
        var param = {}

        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NonCommercials.asmx/GetBranches",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }

    this.svc_GetVendors = function () {
        var param = {}

        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NonCommercials.asmx/GetVendors",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }

    this.svc_GetDepartments = function () {
        var param = {}

        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NonCommercials.asmx/GetDepartments",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }

    this.svc_ContractGetContractDatas = function () {
        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NonCommercials.asmx/ContractGetContractDatas",
            data: {},
            dataType: "json"
        });
        return response;
    }

    this.svc_ContractSubmit = function (ch, cd, ca, Deleted) {
        var param = {
            ch: ch,
            cd: cd,
            ca: ca,
            dh: Deleted.chd.join("','"),
            dd: Deleted.cdd.join("','"),
            da: Deleted.cad.join("','"),
        };
        //console.log(param);
        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NonCommercials.asmx/ContractSubmit",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }

    this.svc_ContractGetContractByID = function (ID) {
        var param = {
            Form_No: ID,
        };
        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NonCommercials.asmx/ContractGetContractByID",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }

    this.svc_GetMaterialAnaplans = function(){
        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NonCommercials.asmx/GetMaterialAnaplans",
            data: {},
            dataType: "json"
        });
        return response;
    };

    this.svc_GetMaterialAnaplansByID = function(ID){
        var param = {
            Form_No: ID
        };
        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/NonCommercials.asmx/GetMaterialAnaplanByID",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    };

    this.svc_ContractApprovalSubmit = function(approvalValue, ListName, ListItemID, HeaderID, Comment){
        var param = {
            approvalValue: approvalValue,
            ListName: ListName,
            ListItemID: ListItemID,
            HeaderID: HeaderID,
            comments: Comment,
        };

        var response = $http({
            method: "POST",
            url: "/_layouts/15/Daikin.Application/WebServices/NACWebService.asmx/ApproveRequestNonCom",
            data: JSON.stringify(param),
            dataType: "json"
        });

        return response;
    };
    
    this.svc_PopUpVendor_GetData = function(SearchTable, PageIndex, SearchBy, Keywords){
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

        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/PopList.asmx/PopUpListData",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    };

    this.svc_PopUpList = function(SearchTable, PageIndex, SearchBy, Keywords){
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
        var response = $http({
            method: "post",
            url: "/_layouts/15/Daikin.Application/WebServices/PopList.asmx/PopUpListData",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    };
});


app.controller('ctrl', function ($scope, svc, Upload, $timeout) {
    const now = new Date();
    var options = {
        year: "numeric",
        month: "short",
    };

    let DateNow = now.toLocaleDateString(undefined, options);

    $scope.ContractHeader = {
        ID: 0,
        Form_No: '',
        Request_Date: DateFormat_ddMMMyyyy(new Date()),
        Requester_Name: '',
        Requester_Email: '',
        Requester_Department: '',
        Procurement_Department: '',
        Procurement_Department_Code: '',
        Procurement_Department_Code_PO: '',
        Internal_Order_Code : '',
        Internal_Order_Name : '',
        PO_Number: '',
        Contract_Status_ID: 1,
        Contract_Status_Name: '',
        Document_Received : false,
        Branch: '',
        Cost_Center: '',
        Vendor_Code: '',
        Vendor_Name: '',
        Contract_No: '',
        Contract_Type_ID: 0,
        Contract_Type_Name: '',
        Period_Start: DateFormat_ddMMMyyyy(new Date()),
        Period_End: DateFormat_ddMMMyyyy(new Date(now.getFullYear(), now.getMonth()+1, 0)),
        Remarks: '',
        Grand_Total: 0,
        Item_ID: 0,
        Approval_Status: 1,
        IsShow : true,
        IsDisabled : false,
        IsEdited : true,
    }

    $scope.ddlContractStatuses = [
        { Code: '1', Name: 'Submitted' },
        { Code: '2', Name: 'Generating' },
        { Code: '3', Name: 'Generated' },
        { Code: '4', Name: 'Pending for Approval' },
        { Code: '5', Name: 'Revised' },
        { Code: '6', Name: 'Rejected' },
        { Code: '7', Name: 'Approved' },
        { Code: '8', Name: 'Draft' }
    ];

    $scope.ContractHeader.Contract_Status_ID = $scope.ddlContractStatuses[0].Code;
    
    $scope.selectedMaterial = [];

    $scope.ddlVendorNonCommercials = [];
    $scope.VendorNonCommercials = {};

    $scope.ddlMasterContractTypes = [];
    $scope.MasterContractType = {};

    $scope.ddlBranches = [];
    $scope.Branch = {};

    $scope.MaterialAnaplans = [];
    $scope.ddlMaterialAnaplansTemp = [];
    $scope.MaterialAnaplan = {};

    $scope.ProcDeptTypes = [];
    $scope.ProcDeptType = {};

    $scope.Outcome = 0;
    $scope.IsCurrentApprover = false;
    $scope.IsReceiverDocs =  false;
    $scope.IsRequestor = false;
    $scope.IsTaxVerifier = false;
    $scope.IsDocumentReceived = false;

    $scope.ntx = {
        FormNo: '',
        Comment: '',
        Outcome: 0,
        Module: 'PC',
    }
    $scope.Logs = [];
    $scope.IsDepartment = false;
    $scope.IsBranch = false;
    $scope.IsContractTypes = false;
    $scope.Deleted = {chd:[],cdd:[],cad:[]};

    $scope.ContractDetails = [];
    $scope.ContractDetails.Material = {
        Name: '',
        Code: ''
    }
    let No = 1;

    $scope.ContractUploaded = [];
    $scope.ContractAttachments = [];
    $scope.InternalOrders = [];
    $scope.InternalOrder = {};

    $scope.showModal = "none";
    $scope.popUpModule = "";
    $scope.popUpRowIndex = 0;
    $scope.popUpSearchTable = "";
    $scope.popUpSearchBy = "";
    $scope.popUpSearchKeyword = "";
    $scope.popUpTableHeader = [];
    $scope.popUpSearchOptions = [];
    $scope.popUpCurrPageIndex = 1;
    $scope.popUpTotalRecords = 0;
    $scope.popUpTotalPageCount = 0;
    $scope.PopUpData = [];

    $scope.PopUp_SearchBy_OnChange = () =>{
        //console.log($scope.popUpSearchBy);
    };

    $scope.PopUpDialog = (module, rowIndex) => {
        //console.log(rowIndex);
        $scope.showModal = "block";
        $scope.popUpModule = module;
        $scope.popUpRowIndex = rowIndex;
        if(module == "Vendor"){
            $scope.popUpSearchTable = "dbo.MasterVendorNonCommercials";
            $scope.popUpTableHeader = ["Code", "Name"];
            $scope.popUpSearchOptions = [
                {'Text': 'Code', 'DB_Col': 'Vendor_Number'},
                {'Text': 'Name', 'DB_Col': 'Title'},
            ];
            $scope.popUpSearchBy = $scope.popUpSearchOptions[0].Text;
        }
        else if(module == "Material Anaplan"){
            $scope.popUpSearchTable = "dbo.MasterMaterialAnaplan";
            $scope.popUpSearchOptions = [
                {'Text': 'Kode', 'DB_Col': 'Material_Code'},
                {'Text': 'Deskripsi', 'DB_Col': 'Material_Description'},
                {'Text': 'GL', 'DB_Col': 'GL'},
                {'Text': 'Deskripsi GL', 'DB_Col': 'GL_Description'},
            ];
            $scope.popUpSearchBy = $scope.popUpSearchOptions[0].Text;
        }
        $scope.PopUp_Search();
        //$scope.PopUp_List($scope.popUpSearchTable, $scope.popUpCurrPageIndex, 'Title', '');
    };

    $scope.PopUp_SearchHelper = (keyEvent) => {
        if (keyEvent.which === 13) {
            $scope.PopUp_Search();
        }
    };

    $scope.PopUp_Search = () => {
        $scope.popUpCurrPageIndex = 1;
        var searchByItem = $scope.popUpSearchOptions.find(function(opt){
            return opt.Text == $scope.popUpSearchBy;
        });
        var searchBy = searchByItem.DB_Col;
        $scope.PopUp_List($scope.popUpSearchTable, $scope.popUpCurrPageIndex, searchBy, $scope.popUpSearchKeyword);
    };

    $scope.PopUp_Prev = () => {
        if($scope.popUpCurrPageIndex > 1){
            $scope.popUpCurrPageIndex -= 1;
            var searchByItem = $scope.popUpSearchOptions.find(function(opt){
                return opt.Text == $scope.popUpSearchBy;
            });
            var searchBy = searchByItem.DB_Col;
            $scope.PopUp_List($scope.popUpSearchTable, $scope.popUpCurrPageIndex, searchBy, $scope.popUpSearchKeyword);
        }
    };

    $scope.PopUp_Next = () => {
        if($scope.popUpCurrPageIndex < $scope.popUpTotalPageCount){
            $scope.popUpCurrPageIndex += 1;
            var searchByItem = $scope.popUpSearchOptions.find(function(opt){
                return opt.Text == $scope.popUpSearchBy;
            });
            var searchBy = searchByItem.DB_Col;
            $scope.PopUp_List($scope.popUpSearchTable, $scope.popUpCurrPageIndex, searchBy, $scope.popUpSearchKeyword);
        }
    };

    $scope.PopUp_List = (tableName, pageIndex, searchBy, keyWord) => {
        if($scope.popUpModule == "Material Anaplan"){
            searchBy += ";Procurement_Department_Title";
            keyWord += ";" + $scope.ContractHeader.Procurement_Department;
        }
        var proc = svc.svc_PopUpList(tableName, pageIndex, searchBy, keyWord);
        proc.then(function(response){
            var jsonData = JSON.parse(response.data.d);
            //console.log(jsonData);
            if($scope.popUpModule == "Vendor"){
                var vendorData = jsonData.Logs;
                $scope.popUpTotalPageCount = jsonData.TotalPages;
                $scope.popUpTotalRecords = jsonData.TotalRecords;
                $scope.PopUpData = [];
                for(let i of vendorData){
                    var newObj = {
                        'ID': i.filter(x => x.Key == 'ID')[0].Value,
                        'Name': i.filter(x => x.Key == 'Title')[0].Value,
                        'Code': i.filter(x => x.Key == 'Vendor_Number')[0].Value,
                        //'Type': i.filter(x => x.Key == 'Type')[0].Value,
                    };
                    $scope.PopUpData.push(newObj);
                }
            }
            else if($scope.popUpModule == "Material Anaplan"){
                $scope.popUpTotalPageCount = jsonData.TotalPages;
                $scope.popUpTotalRecords = jsonData.TotalRecords;
                $scope.PopUpData = [];
                for(let i of jsonData.Logs){
                    var newObj = {
                        'ID': i.filter(x => x.Key == 'ID')[0].Value,
                        'Kode': i.filter(x => x.Key == 'Material_Code')[0].Value,
                        'Deskripsi': i.filter(x => x.Key == 'Material_Description')[0].Value,
                        'GL': i.filter(x => x.Key == 'GL')[0].Value,
                        'Deskripsi GL': i.filter(x => x.Key == 'GL_Description')[0].Value,
                    };
                    $scope.PopUpData.push(newObj);
                }
            }

        }).catch(function(err){
            //console.log(err);
        });
    };

    $scope.CloseDialog = () => {
        $scope.showModal = "none";
        $scope.PopUpData = [];
        $scope.popUpSearchBy = "";
        $scope.popUpSearchKeyword = "";
    };

    $scope.PopUp_SelectItem = (id) => {
        var selectedItem = $scope.PopUpData.find(function(item){
            return item.ID == id;
        });
        if($scope.popUpModule == "Vendor"){
            $scope.VendorNonCommercials.Name = selectedItem.Name;
            $scope.VendorNonCommercials.Code = selectedItem.Code;
            $scope.ContractHeader.Vendor_Code = $scope.VendorNonCommercials.Code;
            $scope.ContractHeader.Vendor_Name = $scope.VendorNonCommercials.Name;
        }
        else if($scope.popUpModule == "Material Anaplan"){
            $scope.ContractDetails[$scope.popUpRowIndex].Material_Number = selectedItem.Kode;
            $scope.ContractDetails[$scope.popUpRowIndex].Material_Name = selectedItem.Kode + " - " + selectedItem.Deskripsi;
            $scope.ContractDetails[$scope.popUpRowIndex].Material = selectedItem;
            $scope.ContractDetails[$scope.popUpRowIndex].Material_Description = selectedItem.Deskripsi;
            $scope.ContractDetails[$scope.popUpRowIndex].Material.Name = selectedItem.Kode + " - " + selectedItem.Deskripsi;
            $scope.selectedMaterial.push(selectedItem);
        }
        $scope.CloseDialog();
    };
    
    $scope.GetBranches = function () {
        try {
            var proc = svc.svc_GetBranches();
            proc.then(function (response) {
                var data = JSON.parse(response.data.d);
                if (data.ProcessSuccess) {
                    $scope.ddlBranches = data.Branches;
                    $scope.Branch = data.Branches[0];

                } else {
                    alert(data.InfoMessage);
                }
            }, function (data) {
                //console.log(data);
                //console.log(status);
                //console.log(data.statusText + ' - ' + data.data.Message);
            });
        } catch (e) {
            alert(e.message);
        }
    }

    $scope.GetVendors = function () {
        try {
            var proc = svc.svc_GetVendors();
            proc.then(function (response) {
                var data = JSON.parse(response.data.d);
                if (data.ProcessSuccess) {
                    //console.log(data);
                    $scope.ddlVendorNonCommercials = data.Vendors;
                    $scope.VendorNonCommercials = data.Vendors[0];

                } else {
                    alert(data.InfoMessage);
                }
            }, function (data) {
                //console.log(data);
                //console.log(status);
                //console.log(data.statusText + ' - ' + data.data.Message);
            });
        } catch (e) {
            alert(e.message);
        }
    }

    $scope.GetDepartments = function () {
        try {
            var proc = svc.svc_GetDepartments();
            proc.then(function (response) {
                var data = JSON.parse(response.data.d);
                if (data.ProcessSuccess) {
                    //console.log(data);
                    $scope.IsDepartment = false;
                    if (data.UserDepartment.length > 0) {
                        ////console.log(data.UserDepartment.length);
                        $scope.ProcDeptType = data.UserDepartment[0];
                        $scope.ProcDeptTypes = data.UserDepartment;
                        $scope.IsDepartment = true;
                        $scope.contractProcurementDepartmentOnChange();

                        ////console.log($scope.ProcDeptType.Procurement_Department_Code);
                    }
                    else {
                        alert("You do not have the authority to make a contract");
                        $scope.ContractHeader.IsShow = false;
                        $scope.ContractHeader.IsDisabled = true;
                        $scope.ContractHeader.IsEdited = false;
                    }
                } else {
                    alert(data.InfoMessage);
                }
            }, function (data) {
                //console.log(data);
                //console.log(status);
                //console.log(data.statusText + ' - ' + data.data.Message);
            });
        } catch (e) {
            alert(e.message);
        }
    }

    $scope.contractGetContractDatas = function () {
        $scope.GetBranches();
        $scope.GetVendors();
        $scope.GetDepartments();

        var proc = svc.svc_ContractGetContractDatas();
        proc.then(function (response) {
            var data = JSON.parse(response.data.d);
            if (data.ProcessSuccess) {
                //console.log(data);

                $scope.IsContractTypes = true;
                $scope.IsBranch = true;
                $scope.ddlMasterContractTypes = data.ContractTypes;
                $scope.MasterContractType = data.ContractTypes[0];

                //angular.forEach(data.MaterialAnaplans, (val,ind) =>{
                //    $scope.ddlMaterialAnaplansTemp.push({
                //        Code : val.Code,
                //        Name : val.Code+" - "+val.Name,
                //        Short_x0020_Name : val.Short_x0020_Name
                //    })
                //});

                $scope.ContractHeader.Requester_Name = data.CurrentLoginName;
                $scope.ContractHeader.Requester_Email = data.CurrentLoginEmail;
                //$scope.ContractHeader.Form_No = data.FormNo;

                $scope.InternalOrders = data.InternalOrders;
                $scope.InternalOrder = data.InternalOrders[0];

                $scope.ContractHeader.Created_By = data.CurrentLoginName;
                $scope.ContractHeader.Modified_By = data.CurrentLoginName;
            } else {
                alert(data.InfoMessage);
            }
        }, function (data) {
            //console.log(data);
            //console.log(status);
            //console.log(data.statusText + ' - ' + data.data.Message);
        });
    }

    $scope.GetMaterialAnaplans = () => {
        var promise = svc.svc_GetMaterialAnaplans();
        promise.then(function(response){
            var data = JSON.parse(response.data.d);
            angular.forEach(data.MaterialAnaplans, (val, ind) => {
                $scope.ddlMaterialAnaplansTemp.push({
                    Code : val.Code,
                    Name : val.Code+" - "+val.Name,
                    Short_x0020_Name : val.Short_x0020_Name
                })
            })
        }).catch(function(err){
            //console.log(err)
        });
    };

    $scope.GetMaterialAnaplansByID = (ID) => {
        var promise = svc.svc_GetMaterialAnaplansByID(ID);
        promise.then(function(response){
            var data = JSON.parse(response.data.d);
            //console.log(data);
            //data.MaterialAnaplans.forEach((o) => {
            //    let code = o.Code;
            //    let name = o.Code +" - "+ o.Name;
    
            //    $scope.MaterialAnaplans.push({
            //        Code : code,
            //        Name : name,
            //        Short_x0020_Name : o.Short_x0020_Name
            //    });

            //    $scope.ddlMaterialAnaplansTemp.push({
            //        Code : code,
            //        Name : name,
            //        Short_x0020_Name : o.Short_x0020_Name
            //    });
    
            //    const indexMaterialName = data.ContractDetail.map(function(e) {return e.Material_Number}).indexOf(o.Code);
            //    if(indexMaterialName != -1) data.ContractDetail[indexMaterialName].Material_Name = name;
            //});

            $scope.contractGetContracMaterialName_New();
        }).catch(function(err){
            //console.log(err)
        });
    };

    $scope.contractProcurementDepartmentOnChange = () => {
        //console.log($scope.ContractDetails);
        $scope.ContractDetails.map(function(e) { 
            if(e.ID > 0) {
                $scope.Deleted.cdd.push(e.ID);
                return e.ID;
            }
        });

        $scope.ContractHeader.Procurement_Department = ''
        if($scope.ProcDeptType.Code != '' || $scope.ProcDeptType.Code == null)
        {
            $scope.ContractHeader.Procurement_Department = $scope.ProcDeptType.Name;
        }
        if($scope.ContractHeader.Procurement_Department != 'Marketing Digital' || $scope.ContractHeader.Procurement_Department != 'Marketing Trade')
        {
            $scope.ContractHeader.Internal_Order_Code = "";
            $scope.ContractHeader.Internal_Order_Name = "";
            $scope.InternalOrder = $scope.InternalOrders[0];
        }

        $scope.ContractDetails = [];
        $scope.MaterialAnaplans = [];

        //console.log("ddl Material Anaplans Temp ", $scope.ddlMaterialAnaplansTemp);
        
        angular.forEach($scope.ddlMaterialAnaplansTemp, (val) =>{
            let Code = val.Code;
            var str = $scope.ProcDeptType.Procurement_Department_Code;
            //var str = $scope.ProcDeptType.Name;

            ////console.log("Code : ", Code);
            ////console.log("str : ", str);   

            if(Code.includes(str)){
                $scope.MaterialAnaplans.push({
                    Code : val.Code,
                    Name : val.Name,
                    Short_x0020_Name : val.Short_x0020_Name
                })
            }

            //$scope.MaterialAnaplans.push({
            //    Code : val.Code,
            //    Name : val.Name,
            //    Short_x0020_Name : val.Short_x0020_Name
            //})
        });
        ////console.log("Material Anaplans : ", $scope.MaterialAnaplans);
        
        $scope.MaterialAnaplans.sort((a, b) => a.Code.localeCompare(b.Name));

        $scope.contractAddContractDetail();
    }

    $scope.ContractInternalOrderOnChange = () => {
        const {Code,Name} = $scope.InternalOrder;

        if(Name !== "Please Select")
        {
            $scope.ContractHeader.Internal_Order_Code = Code;
            $scope.ContractHeader.Internal_Order_Name = Name;
        }
    }

    $scope.contractVendorOnChange = function () {
        $scope.ContractHeader.Vendor_Code = $scope.VendorNonCommercials.Code;
        $scope.ContractHeader.Vendor_Name = $scope.VendorNonCommercials.Name;

        //console.log($scope.ContractHeader.Vendor_Code);
        //console.log($scope.ContractHeader.Vendor_Name);
    }

    $scope.contractContractTypeOnChange = function () {
        $scope.ContractHeader.Contract_Type_ID = $scope.MasterContractType.Code;
        $scope.ContractHeader.Contract_Type_Name = $scope.MasterContractType.Name;
    }

    $scope.contractBranchOnChange = function () {
        $scope.ContractHeader.Branch = $scope.Branch.Name;
    }
    
    $scope.isUploadFile = false;
    $scope.contractUploadingFile = function () {
        var msg = 'The attachments below already exist: ';
        
        const IsUpload = () => {
            var warningMsg = '';
            var anyError = false;

            angular.forEach($scope.uploadFiles, function (file, key) {
                //console.log("File : ", file);
                // let indexObj = $scope.ContractUploaded.indexOf(file)
                var indexObj = $scope.ContractUploaded.map(function(e) { return e.name; }).indexOf(file.name);
                if(indexObj == -1 || $scope.ContractUploaded.length == 0) {
                    ////console.log(file);
    
                    var input = file;
                    var formdata = false;
                    if (window.FormData) {
                        formdata = new FormData();
                    }
                    else{
                        //alert("Not support for ajax uploading technology (FormData). Try using latest firefox / chrome browser.");
                        //console.log("Not support for ajax uploading technology (FormData). Try using latest firefox / chrome browser.");
                        $scope.isUploadFile = false;
                    }

                    if (window.FileReader) {
                        let reader = new FileReader();
                        reader.onloadend = function (e) {
                            //showUploadedItem(e.target.result);
                        };
                        reader.readAsDataURL(input);
                    }
                    else{
                        //alert("Not support for ajax uploading technology (FileReader). Try using latest firefox / chrome browser.");
                        //console.log("Not support for ajax uploading technology (FileReader). Try using latest firefox / chrome browser.");
                        $scope.isUploadFile = false;
                    }

                    if (formdata) {
                        formdata.append("file", input);
                        //console.log("Formdata", formdata);
                        //console.log("input", input);
                        $.ajax({
                            url: "/_layouts/15/Daikin.Application/Handler/PostUploadHandler.ashx",
                            type: "POST",
                            data: formdata,
                            processData: false,
                            contentType: false,
                            async: false,
                            dataType: "JSON",
                            success: function (result) {
                                //console.log(result);

                                const {PostedFile} = result;

                                const myFile = new File([input], PostedFile.name);

                                $scope.isUploadFile = false;
                     

                                $scope.ContractUploaded.push(myFile);

                                $scope.ContractAttachments.push({
                                    Id: 0,
                                    Attachment_FileName: PostedFile.name,
                                    Size: PostedFile.size,
                                    Header_ID:$scope.ContractHeader.ID?$scope.ContractHeader.ID:0,
                                });
                            },
                            error : function (err) { 
                                //console.log(err.statusText);
                                //alert(err.statusText); 
                            },
                        });
                    }
                    else{
                        //alert("Not support for ajax uploading technology (FormData). Try using firefox browser.");
                        //console.log("Not support for ajax uploading technology (FormData). Try using firefox browser.");
                        $scope.isUploadFile = false;
                    }
                }
                else{
                    warningMsg += `\n----- ${file.name} -----`;
                    anyError = true;
                    return
                }
            });

            return {
                anyError: anyError,
                warningMsg: warningMsg,
            };
        }
        
        
        const {anyError, warningMsg} = IsUpload();

        if (anyError) {
            alert(msg + warningMsg);
            var inputs = document.querySelectorAll("input[type=file]")
            inputs[0].value = null;
            return;
        }
    };

    $scope.contractDeletingFile = function (index) {
        if($scope.ContractAttachments[index].ID){
            $scope.Deleted.cad.push($scope.ContractAttachments[index].ID);
        }

        $scope.ContractAttachments.splice(index, 1);
        $scope.ContractUploaded.splice(index, 1);
    };

    $scope.contractAddContractDetail = function () {
        const NewDetail = {
            ID:0,
            Material: "",
            No: No++,
            Material_Number: "",
            Material_Name:"",
            Material_Description: "",
            Contract_Amount: 0,
            Variable_Amount: false,
        };

        $scope.ContractDetails.push(NewDetail);

        $scope.contractContractDetailMaterialAmountOnChangeCalculateGrandTotal();
    };
  
    $scope.contractContractDetailVariableAmountOnCheck = function (index, value) {
        let check = true;
        if (!value.Variable_Amount) {
            check = false;
        }
        $scope.ContractDetails[index].Variable_Amount = check;
    };

    $scope.contractContractDetailMaterialNoOnChange = (index, value) => {
        $scope.ContractDetails[index].Material_Number = value.Material.Code;
        $scope.ContractDetails[index].Material_Name = value.Material.Name;

        $scope.selectedMaterial.push(value.Material);
        //console.log($scope.ContractDetails);
    };

    $scope.contractContractDetailMaterialDescriptionOnBlur = function (index, value) {
        $scope.ContractDetails[index].Material_Description = value.Material_Description;
    };

    $scope.contractContractDetailMaterialAmountOnBlur = function (index, value) {
        if(!value.Contract_Amount || value.Contract_Amount == undefined) 
            value.Contract_Amount = 0
        $scope.ContractDetails[index].Contract_Amount = value.Contract_Amount;
        
        $scope.contractContractDetailMaterialAmountOnChangeCalculateGrandTotal();
    };

    $scope.contractContractDetailMaterialAmountOnChangeCalculateGrandTotal = function () {
        $scope.ContractHeader.Grand_Total = 0;
        for (let i = 0; i < $scope.ContractDetails.length; i++) {
            $scope.ContractDetails[i].No = i + 1;
            $scope.ContractHeader.Grand_Total += parseFloat($scope.ContractDetails[i].Contract_Amount);
        }
    };

    $scope.contractSubmitCheck = function () {
        try {
            var anyError = false;
            //console.log($scope.ContractHeader.Period_Start, $scope.ContractHeader.Period_End);


            var date_Start = new Date($scope.ContractHeader.Period_Start);
            var date_End = new Date($scope.ContractHeader.Period_End);

            //console.log(date_Start, date_End);
            //console.log("Header", $scope.ContractHeader);

            //if ($scope.ContractHeader.Period_Start > $scope.ContractHeader.Period_End) {
            //    alert('Start Period not allowed to be more larger than End Period');
            //    return;
            //}

            if ($scope.ContractHeader.Procurement_Department.length <= 0) {
                alert('Please choose procurement department');
                return;
            }
            if ($scope.ContractHeader.Contract_No.length <= 0) {
                alert('Please insert contract no');
                return;
            }
            if ($scope.ContractHeader.Vendor_Name.length <= 0) {
                alert('Please choose vendor name');
                return;
            }
            if ($scope.ContractHeader.Contract_Type_Name.length <= 0) {
                alert('Please choose contract type');
                return;
            }
            if ($scope.ContractHeader.Period_Start.length <= 0) {
                alert('Please insert periode start');
                return;
            }
            if ($scope.ContractHeader.Period_End.length <= 0) {
                alert('Please insert periode end');
                return;
            }
            if ($scope.ContractHeader.Branch.length <= 0) {
                alert('Please choose branch');
                return;
            }
            if (date_Start > date_End){
                alert('Start Period not allowed to be more larger than End Period');
                return;
            }


            if ($scope.ContractHeader.Remarks.length <= 0) {
                alert('Please insert Remarks');
                return;
            }
            if ($scope.ContractAttachments.length <= 0) {
                alert('Please add attachment');
                return;
            }
            if (['Marketing Trade','Marketing Digital'].indexOf($scope.ContractHeader.Procurement_Department) >= 0 && !$scope.ContractHeader.Internal_Order_Code) {
                alert('Please choose internal order');
                return;
            }

            var msg = 'Please complete the column in the table';
            for (var j = 0; j < $scope.ContractDetails.length; j++) {
                var cd = $scope.ContractDetails[j];

                if (cd.Material_Number.length <= 0) {
                    anyError = true;
                }

                // if (cd.Material_Description.length <= 0) {
                //     anyError = true;
                // }

                if (cd.Contract_Amount.length <= 0 || cd.Contract_Amount <= 0) {
                    anyError = true;
                }
            }

            if (anyError) {
                alert(msg);
                return;
            }

            var confirmMsg = confirm('Submit ?');
            if (confirmMsg) {
                var proc = svc.svc_ContractSubmit($scope.ContractHeader, $scope.ContractDetails, $scope.ContractAttachments,$scope.Deleted);
                proc.then(function (response) {
                    var data = JSON.parse(response.data.d);
                    if (data.ProcessSuccess) {
                        alert('Submitted Successfully!');
                        // location.href = 'Contract.aspx?ID=' + data.ContractHeader.Form_No;
                        //console.log(data);

                        location.href = 'List.aspx';
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
    }

    $scope.ContractContractDetailOnDelete = function (index) {
        if($scope.ContractDetails[index].ID){
            $scope.Deleted.cdd.push($scope.ContractDetails[index].ID);
        }
        $scope.ContractDetails.splice(index, 1);

        $scope.contractContractDetailMaterialAmountOnChangeCalculateGrandTotal();
    }

    $scope.contractGetContracMaterialName = () => {
        angular.forEach($scope.ContractDetails, function (v, i) {
            angular.forEach($scope.MaterialAnaplans, function (val, ind) {
                if (v.Material_Number == val.Code) $scope.ContractDetails[i].Material = val;
            });
        });
    };

    $scope.contractGetContracMaterialName_New = () => {
        angular.forEach($scope.ContractDetails, function(v, i){
            var material = {
                Name: v.Material_Name,
                Code: v.Material_Number
            }
            $scope.ContractDetails[i].Material = material;

        });
        ////console.log($scope.ContractDetails);
    };

    $scope.ContractGetContractByID = function () {
        try {
            var id = GetQueryString()['ID']; //Nintex No

            if (id != undefined) {
                $scope.showModal = "none";
                $scope.GetBranches();
                $scope.GetVendors();

                $scope.ContractAttachments = [];
                $scope.ContractUploaded = [];
                var proc = svc.svc_ContractGetContractByID(id);
                proc.then(function (response) {
                    var data = JSON.parse(response.data.d);
                    if (data.ProcessSuccess) {
                        ////console.log(data);

                        for (let i in data.ContractHeader){
                            if(i.startsWith('Period')) data.ContractHeader[i] = $scope.ConvertJSONDate(data.ContractHeader[i]);
                            else if (i.endsWith('Date')) data.ContractHeader[i] = $scope.ConvertJSONDate(data.ContractHeader[i]);
                        } 

                        for (let i in data.ContractDetail) {
                            for (let j in data.ContractDetail[i]) {
                                if (j.endsWith('Date')) {
                                    data.ContractDetail[i][j] = $scope.ConvertJSONDate(data.ContractDetail[i][j]);
                                }
                            }
                        }

                        for (let i in data.ContractAttachment) {
                            for (let j in data.ContractAttachment[i]) {
                                if (j.endsWith('Date')) {
                                    data.ContractAttachment[i][j] = $scope.ConvertJSONDate(data.ContractAttachment[i][j]);
                                }
                            }
                        }

                        const indexVendor = $scope.ddlVendorNonCommercials.map(function(e) {return e.Code}).indexOf(data.ContractHeader.Vendor_Code);
                        $scope.VendorNonCommercials = $scope.ddlVendorNonCommercials[indexVendor];

                        $scope.IsBranch = false;
                        const indexBranch = $scope.ddlBranches.map(function(e) {return e.Name}).indexOf(data.ContractHeader.Branch);
                        $scope.Branch = $scope.ddlBranches[indexBranch];
                        if(indexBranch != -1) $scope.IsBranch = true;

                        $scope.IsContractTypes = false;
                        $scope.ddlMasterContractTypes = data.ContractTypes;
                        const indexContractType = data.ContractTypes.map(function(e) {return e.Name}).indexOf(data.ContractHeader.Contract_Type_Name);
                        $scope.MasterContractType = data.ContractTypes[indexContractType];
                        if(indexContractType != -1) $scope.IsContractTypes = true;
                        
                        $scope.InternalOrders = data.InternalOrders;
                        const indexIntOrd = $scope.InternalOrders.map((o) => { return o.Code }).indexOf( data.ContractHeader.Internal_Order_Code);
                        $scope.InternalOrder = $scope.InternalOrders[indexIntOrd];

                        $scope.GetMaterialAnaplansByID(id);

                        $scope.MaterialAnaplans.sort((a, b) => a.Code.localeCompare(b.Name));

                        $scope.IsDepartment = false;
                        $scope.ProcDeptTypes = data.UserDepartment;
                        const indexProcDept = $scope.ProcDeptTypes.map(function(e) { return e.Name; }).indexOf( data.ContractHeader.Procurement_Department);
                        $scope.ProcDeptType = $scope.ProcDeptTypes[indexProcDept];
                        if(indexProcDept != -1) $scope.IsDepartment = true;

                        data.ContractAttachment.forEach(o => {
                            $scope.ContractUploaded.push({
                                name: o.Attachment_FileName,
                                type: "application/xml"
                            });
                        });

                        if(['4','5','6','7','8'].indexOf(data.ContractHeader.Approval_Status) >= 0){
                            data.ContractHeader.IsShow = true; 
                            data.ContractHeader.IsDisabled = true;
                            data.ContractHeader.IsEdited = false; 
                        }
                        else{
                            data.ContractHeader.IsShow = true; 
                            data.ContractHeader.IsDisabled = false;
                            data.ContractHeader.IsEdited = true;
                            if(indexProcDept == -1){
                                data.ContractHeader.IsShow = false;
                                data.ContractHeader.IsDisabled = true;
                                data.ContractHeader.IsEdited = false;
                            }
                        }

                        data.ContractHeader.Document_Received = (data.ContractHeader.Document_Received == '0' || !data.ContractHeader.Document_Received) ? false : true;
                        const Contract = $scope.ContractHeader;
                        $scope.ContractHeader = {...Contract,...data.ContractHeader};
                        $scope.ContractDetails = data.ContractDetail;   
                        $scope.ContractAttachments = data.ContractAttachment;

                        $scope.IsCurrentApprover = data.IsCurrentApprover;
                        $scope.IsReceiverDocs = data.IsReceiverDocs;
                        $scope.IsRequestor = data.IsRequestor;
                        $scope.IsTaxVerifier = data.IsTaxVerifier;                        
                        $scope.IsDocumentReceived = data.ContractHeader.Document_Received;
                    }
                    else {
                        //console.log(data);
                    }
                }, function (data, status) {
                    //console.log(data);

                    //console.log(data.statusText + ' - ' + data.data.Message);
                });
            } else {
                No = 1;
                $scope.contractGetContractDatas();
                $scope.contractAddContractDetail();
            }

        } catch (e) {
            //console.log(e.message);
        }
    }

    $scope.ContractGetApproverLogByID = function () {
        try {
            var id = GetQueryString()['ID']; //Nintex No

            if (id != undefined) {
                var proc = svc.svc_ContractGetApproverLogByID(id);
                
                proc.then(function (response) {
                    var data = JSON.parse(response.data.d);
                    if (data.ProcessSuccess) {
                        //console.log(data);

                        $scope.Logs = data.Logs;
                    } else {
                        alert(data.InfoMessage);
                    }
                }, function (data, status) {
                    //console.log(data.statusText + ' - ' + data.data.Message);
                });
            }
        } catch (e) {
            alert(e.message);
        }
    }

    $scope.ContractApprovalSubmit = function () {
        try {
            var st = $scope.Outcome;
            var id = $scope.ContractHeader.Form_No;
            var approvalValue = "";
            approvalValue = st == 1 ? "Approve" : "Reject";

            if (st == 0) {
                alert('Please select the outcomes');
                return;
            }

            if ($scope.ntx.Comment.length <= 0 && st == 2) {
                alert('Please specify your comments for rejecting this Contract');
                return;
            }

            //var msg = '';
            //if (st == 1) {
            //    msg = 'Are you sure want to approve ?';
            //} else if (st == 2) {
            //    msg = 'Are you sure want to reject ?';
            //} else {
            //    msg = 'Are you sure want to revise ?';
            //}
            //var confirmApprove = confirm(msg);

            var confirmApprove = true;
            if (confirmApprove) {
                $scope.ntx.FormNo = $scope.ContractHeader.Form_No;
                $scope.ntx.Outcome = st;
                $scope.ntx.Module = 'PC';
                $scope.ntx.Position_ID = $scope.ContractHeader.Pending_Approver_Role_ID;
                $scope.ntx.Transaction_ID = $scope.ContractHeader.ID;

                var proc = svc.svc_ContractApprovalSubmit(approvalValue, "Contract", $scope.ContractHeader.Item_ID, $scope.ContractHeader.ID, $scope.ntx.Comment);
                proc.then(function (response) {
                    //console.log(response);
                    var data = JSON.parse(response.data.d);
                    if (data.ProcessSuccess) {
                        //if (st == 1) {
                        //    alert('Approved Successfully!');
                        //} else if (st == 2) {
                        //    alert('Rejected Successfully!');
                        //}
                        location.href = '/_layouts/15/Daikin.Application/Modules/PendingTask/PendingTaskList.aspx';
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
    }

    $scope.contractClose = function () {
        location.href = 'List.aspx';
    }

    $scope.ConvertJSONDate = function (x,format) {
        if(format == undefined) format = '{dd}-{mmm}-{yyyy}';
        if (x == null)
            return x;
        var re = /\/Date\(([0-9]*)\)\//;
        var m = x.match(re);
        var jsondate = "";
        if (m)
        {
            jsondate = new Date(parseInt(m[1]));
            var date = new Date(jsondate);
            let year = date.getFullYear();
    
            let month = date.getMonth();
            let months = {
                mmmm: new Array("January","February","March","April","May","June","July","August","September","October","November","December"),
                mmm: new Array("Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"),
                mm: new Array('01', '02', '03', '04', '05', '06', '07', '08', '09', '10', '11', '12'),
                m: new Array('1', '2', '3', '4', '5', '6', '7', '8', '9', '10', '11', '12')
            };
    
            let d = date.getDate();
            let dd;
            if (d < 10) dd = "0" + d;
            else dd = d;
    
            let day = date.getDay();
            let days = new Array('Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday');
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
        }
        else
            return null;
        
    }

    $scope.ContractGetContractByID();
    $scope.ContractGetApproverLogByID();
    //$scope.GetMaterialAnaplans();
});