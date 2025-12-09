NWF.FormFiller.Events.RegisterAfterReady(function () {
    GetWorkflowHistoryList();
    NWF$('#' + Comment).val("");
});

NWF.FormFiller.Events.RegisterRepeaterRowAdded(function () {
    GetWorkflowHistoryList();
    NWF$('#' + Comment).val("");
});


function GetWorkflowHistoryList() {
    try {
        const objek = {
            Form_No: NWF$("#" + cvFormNo).val(),
            Transaction_ID: NWF$("#" + cvTransID).val(),
            Module_Code: "M029"
        };
        NWF$.ajax({
            type: "POST",
            url: '/_layouts/15/Daikin.Application/WebServices/NACWebService.asmx/GetHistoryLog2',
            async: false,
            cache: false,
            data: JSON.stringify(objek),
            contentType: "application/json; charset=utf-8",
            success: onSuccessGetWorkflowHistoryLog,
            error: function (xhr, ajaxOptions, thrownError) {
                console.log('Status : ', xhr.status, 'responseText: ', xhr.responseText, '-', thrownError);
            }
        });
    } catch (e) {
        console.log('GetWorkflowHistoryList : ' + e.message);
    }
};

function onSuccessGetWorkflowHistoryLog(response) {
    let trHTML = '';
    const parsedData = JSON.parse(response.d);
    const TotalRows = $("#tblHistory").find("tr:not(:first)").length;
    if (TotalRows > 0) {
        $("#tblHistory").find("tr:not(:first)").remove();
    }

    $.each(parsedData, function () {
        trHTML += '<tr><td>' + this.Personal_Name + '</td>';
        trHTML += '<td>' + this.Position + '</td>';
        trHTML += '<td>' + this.Comments + '</td>';
        trHTML += '<td>' + this.Action_Name + '</td>';
        trHTML += '<td>' + this.Action_Date + '</td></tr>';
    });
    $("#tblHistory > tbody").append(trHTML);
};