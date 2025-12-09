NWF.FormFiller.Events.RegisterAfterReady(function () {
    if ($('#' + ItemID).val() != null && $('#' + ItemID).val() != "")
        getApprover($('#' + ListName).val(), $('#' + ItemID).val())
});

function getApprover(ListName, ListItemID) {
    const param = {
        ListName: ListName,
        ListItemID: ListItemID,
    };

    console.log('PARAM', param)
    try {
        jQuery.ajax({
            type: "POST",
            contentType: "application/json; charset=utf-8",
            url: "/_layouts/15/Daikin.Application/WebServices/NACWebService.asmx/GetApprover",
            data: JSON.stringify(param),
            dataType: "json",
            async: true,
            success: function (data) {
                console.log('Approver: '+ data.d)
                var control = NWF$("#" + Approvers);
                control.val(data.d);
                control.trigger('change')
                NWF.FormFiller.Functions.ProcessOnChange(control);
            },
            error: function (xhr) {
                console.log('Error: ' + xhr.responseText)
                alert(xhr.responseText);
            }
        })
    }
    catch (ex) {
        alert(ex.message)
    }

}

function SaveApproval(approvalValue, ListName, ListItemID) {
    param = {
        approvalValue: approvalValue,
        ListName: ListName,
        ListItemID: ListItemID,
        comments: NWF$("#" + Comment).val(),
    };

    console.log('Save & Submit Triggered.');

    console.log('PARAM', param)
    try {
        jQuery.ajax({
            type: "POST",
            contentType: "application/json; charset=utf-8",
            url: "/_layouts/15/Daikin.Application/WebServices/NACWebService.asmx/ApproveRequest",
            data: JSON.stringify(param),
            dataType: "json",
            async: true,
            success: function () {
                console.log('Your Response have been recorded!');
            },
            error: function (xhr) {
                console.log('Error: ' + xhr.responseText)
                alert(xhr.responseText);
            }
        })
    }
    catch (ex)
    {
        alert(ex.message)
    }
}