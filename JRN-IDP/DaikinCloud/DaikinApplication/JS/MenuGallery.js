var app = angular.module('modApp', []);

app.service("svc", function ($http) {
   
    this.svc_GetModules = function (ListName) {
        var param = {
            //ListName: ListName
        }

        var response = $http({
            method: "post",
            url: "/_layouts/15/WebServices/Modules.asmx/getModules",
            data: {},
            dataType: "json"
        });
        return response;

    }

});

app.controller('modCtrl', function ($scope, svc) {
    $scope.Modules = [];

    $scope.getModules = function () {
        var proc = svc.svc_GetModules();
        proc.then(function (response) {
            var res = JSON.parse(response.data.d);
             console.log(res);
            if (res.ProcessSuccess) {
                $scope.Modules = res.Datas;
            }
            else {
                console.error(res.InfoMessage);
            }
        }, function (res, status) {
            console.log(res);
            alert(res.statusText + ' - ' + res.data.Message);
        });
    }

    $scope.redirectPage = function (i) {
        location.href = $scope.Modules[i].Module_Url;
    }
    $scope.getModules();

});


