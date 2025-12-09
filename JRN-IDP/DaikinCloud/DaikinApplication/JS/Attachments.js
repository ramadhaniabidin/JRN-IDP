var app = angular.module('app', []);

app.service("svc", function ($http) {
    this.svc_AttachmentList = function (FormNo, ModuleId) {
        var param = {
            FormNo: FormNo,
            ModuleId: ModuleId,
        };
        var response = $http({
            method: "post",
            url: "/_layouts/15/WebServices/SharePointFunctionality.asmx/GetAttachmentList",
            data: JSON.stringify(param),
            dataType: "json"
        });
        return response;
    }
});

app.controller('ctrl', function ($scope, svc) {
    $scope.Items = [];
    $scope.ListData = function () {
        var FormNo = GetQueryString()['FormNo'];
        var ModuleId = GetQueryString()['ModuleId'];
        var req = svc.svc_AttachmentList(FormNo, ModuleId);
        req.then(function (response) {
            var data = JSON.parse(response.data.d);
            console.log(data);
            if (data.ProcessSuccess) {
                $scope.Items = data.Items;
            }

        }, function (data, status) {

        });
    }
});