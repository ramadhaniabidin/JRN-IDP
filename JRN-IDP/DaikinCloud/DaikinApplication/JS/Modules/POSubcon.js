let popUpDialog;


NWF.FormFiller.Events.RegisterAfterReady(function () {
    $('.btnApproval input[type="radio"]').prop('checked', false);
    NWF$('#' + Comment).val("");
    getApprovalLog();
});

NWF.FormFiller.Events.RegisterRepeaterRowAdded(function () {
    NWF$('#' + Comment).val("");
    $('.btnApproval input[type="radio"]').prop('checked', false);
    getApprovalLog();
});


function PopUp_ShowDetail() {
    popUpDialog = $("#PopUp_Dialog").dialog({
        height: 500,
        width: 1000,
        title: "Select : "
    });
};


function getApprovalLog() {
    try {
        var objek = new Object();
        objek.Form_No = NWF$('#' + textFormNo).val();
        objek.Transaction_ID = NWF$("#" + cvItemID).val();
        objek.Module_Code = NWF$("#" + cvModuleCode).val();
        if (objek.Form_No == undefined || objek.Form_No == null || objek.Form_No == "") {
            objek.Form_No = NWF$(".form_no input").val();
        }
        let props = getAjaxProps(objek);
        NWF$.ajax(props);
    } catch (e) {
        console.log('GetWorkflowHistoryList : ' + e.message);
    }
};

function getAjaxProps(objek) {
    let props = {
        type: "POST",
        url: "/_layouts/15/Daikin.Application/WebServices/Master.asmx/GetHistoryLog",
        async: false,
        cache: false,
        data: JSON.stringify(objek),
        contentType: "application/json; charset=utf-8",
        success: poSubconSuccess,
        error: function (xhr, ajaxOptions, thrownError) {
            console.log('Status : ', xhr.status, 'responseText: ', xhr.responseText, '-', thrownError);
        }
    };
    return props;
};

function poSubconSuccess(response) {
    var trHTML = '';
    var parsedData = JSON.parse(response.d);
    var items = parsedData.Logs;
    var TotalRows = $("#tblHistory").find("tr:not(:first)").length;
    if (TotalRows > 0) {
        $("#tblHistory").find("tr:not(:first)").remove();
    }
    $.each(items, function () {
        trHTML += '<tr><td>' + this.Personal_Name + '</td>';
        trHTML += '<td>' + this.Position + '</td>';
        trHTML += '<td>' + this.Comments + '</td>';
        trHTML += '<td>' + this.Action_Name + '</td>';
        trHTML += '<td>' + this.Action_Date + '</td></tr>';
    });
    $("#tblHistory > tbody").append(trHTML);
};