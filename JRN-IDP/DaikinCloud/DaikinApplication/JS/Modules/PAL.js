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

});

app.controller("ctrl", function ($scope, svc, Upload, $timeout) {
    $scope.ID = GetQueryString()['ID'];
    $scope.ddlCompanyCode = ["Please Select", "1400"];
    $scope.ddlTitle = ["Please Select", "Mr.", "Mrs."];
    $scope.ddlDepartment = ["Please Select", "IT SAP", "IT Infrastructure", "Sales Development"];
    $scope.ddlJobTitle = ["Please Select", "Sales Engineer", "Engineer", "Junior Engineer"];
    $scope.ddlBranch = ["Please Select", "Head Office", "Bekasi", "Sunter"];
    $scope.ddlRegion = ["Please Select", "DKI Jakarta", "West Java", "East Java"];
    $scope.ddlEmployeeGroup = ["Please Select", "Sales", "After Sales Service", "Training"];
    $scope.ddlEmployeeSubGroup = ["Please Select", "Engineer", "Technician", "Helper"];
    $scope.ddlSubArea = ["Please Select", "Jakarta", "Bandung", "Bekasi"];
    $scope.ddlSalesGroup = ["Please Select", "101 Sales", "102 Service", "103 Sparepart"];
    $scope.ddlBankKey = ["Please Select", "V014 Bank Central Asia", "001 Bank CIMB", "002 Bank Sumitomo"];
    $scope.ddlCurrency = ["IDR", "JPY", "USD"];
    $scope.ddlPartnerBank = ["ID01", "ID02", "ID03"];


    $scope.Header = {
        Form_No: "GENERATED ON SUBMIT",
        Company_Code: $scope.ddlCompanyCode[0],
        Title: $scope.ddlTitle[0],
        Full_Name: '',
        Personnel_Np: '',
        Join_Date: '',
        Birth_Date_Year: '',
        Department: $scope.ddlDepartment[0],
        Job_Title: $scope.ddlJobTitle[0],
        Email: '',
        Branch: $scope.ddlBranch[0],
        Region: $scope.ddlRegion[0],
        City: '',
        Postal_Code: '',
        Employee_Group: $scope.ddlEmployeeGroup[0],
        Employee_SubGroup: $scope.ddlEmployeeSubGroup[0],
        Payroll_Area: 99,
        Sub_Area: $scope.ddlSubArea[0],
        Bank_Key: $scope.ddlBankKey[0],
        Currency: $scope.ddlCurrency[0],
        Partner_Bank_ID: $scope.ddlPartnerBank[0],
        Bank_Account_No: '',
        Bank_Account_Name: ''
    };
});
