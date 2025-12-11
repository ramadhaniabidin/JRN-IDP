
function HideAttachmentControl() {
    $(".nf-attachmentsLink").css("display", "none");
    $(".propertysheet").css("display", "none");
};

function ShowAttachmentControl() {
    $(".nf-attachmentsLink").css("display", "block");
    $(".propertysheet").css("display", "block");
};

function AttachmentOpenNewTab() {
    $(".nf-attachmentsRow a").attr("target", "_blank");
};

const statusRules = {
    "M017": ["Revise", "Start"],
    "M016": ["", "Revise", "Draft"],
    "M018": ["", "Revise", "Draft", "Start"]
};

NWF.FormFiller.Events.RegisterAfterReady(function () {
    const moduleCategory = NWF$("#" + cvModuleCategory).val();
    const moduleCode = NWF$("#" + cvModuleCode).val();
    const formStatus = NWF$("#" + cvFormStatus).val();
    GetWorkflowHistoryList();
    if (moduleCategory === "Non Commercial") {
        if (statusRules[moduleCode] && !statusRules[moduleCode].includes(formStatus)) {
            HideAttachmentControl();
            AttachmentOpenNewTab();
        }
    }
    else if (moduleCategory === "PO Subcon") {
        NWF$('#' + Comment).val("");
    } else {
        if (["", "Revise", "Draft"].includes(formStatus)) {
            ShowAttachmentControl();
        } else {
            HideAttachmentControl();
            AttachmentOpenNewTab();
        }
    }
});

NWF.FormFiller.Events.RegisterRepeaterRowAdded(function () {
    const moduleCategory = NWF$("#" + cvModuleCategory).val();
    const moduleCode = NWF$("#" + cvModuleCode).val();
    const formStatus = NWF$("#" + cvFormStatus).val();
    GetWorkflowHistoryList();
    if (moduleCategory === "Non Commercial") {
        if (statusRules[moduleCode] && !statusRules[moduleCode].includes(formStatus)) {
            HideAttachmentControl();
            AttachmentOpenNewTab();
        }
    }
    else if (moduleCategory === "PO Subcon") {
        NWF$('#' + Comment).val("");
    } else {
        if (["", "Revise", "Draft"].includes(formStatus)) {
            ShowAttachmentControl();
        } else {
            HideAttachmentControl();
            AttachmentOpenNewTab();
        }
    }
});


function GetWorkflowHistoryList() {
    try {
        var objek = new Object();

        objek.Form_No = NWF$('#' + textFormNo).val();
        objek.Transaction_ID = NWF$("#" + cvItemID).val();
        objek.Module_Code = NWF$("#" + cvModuleCode).val();
        console.log("First get form no: ", objek.Form_No);
        if (objek.Form_No == undefined || objek.Form_No == null || objek.Form_No == "") {
            objek.Form_No = NWF$(".form_no input").val();
        }

        console.log('Param', objek);
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


}


function onSuccessGetWorkflowHistoryLog(response) {
    var trHTML = '';
    var parsedData = JSON.parse(response.d);
    console.log("Parse Data = ", parsedData);
    var TotalRows = $("#tblHistory").find("tr:not(:first)").length;
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
    console.log(trHTML);
    $("#tblHistory > tbody").append(trHTML);
    console.log($('#tblHistory').html());
};

function PartnerBankCheck() {
    var ceklist = NWF$("#" + chkPartnerBank);
    var ceklistAccountNoInvalid = NWF$("#" + chkAccountNoInvalid);
    var ceklistAccountNameInvalid = NWF$("#" + chkAccountNameInvalid);
    var ceklistBankKeyInvalid = NWF$("#" + chkBankKeyInvalid);
    var ceklistBankNameInvalid = NWF$("#" + chkBankNameInvalid);
    ceklist.click(function () {
        var isChecked = ceklist.prop('checked');
        if (isChecked == true && ceklistAccountNoInvalid.prop("checked") == false) {
            NWF$("#" + chkAccountNoInvalid).click();
            ceklistAccountNoInvalid.prop("disabled", true);
        } else if (isChecked == false && ceklistAccountNoInvalid.prop("checked", true)) {
            ceklistAccountNoInvalid.click();
            ceklistAccountNoInvalid.prop("disabled", false);
        } else if (isChecked == true && ceklistAccountNoInvalid.prop("checked", true)) {
            ceklistAccountNoInvalid.prop("disabled", true);
        }

        if (isChecked == true && ceklistAccountNameInvalid.prop("checked") == false) {
            ceklistAccountNameInvalid.click();
            ceklistAccountNameInvalid.prop("disabled", true);
        } else if (isChecked == false && ceklistAccountNameInvalid.prop("checked") == true) {
            ceklistAccountNameInvalid.click();
            ceklistAccountNameInvalid.prop("disabled", false);
        } else if (isChecked == true && ceklistAccountNameInvalid.prop("checked") == true) {
            ceklistAccountNameInvalid.prop("disabled", true);
        }

        if (isChecked == true && ceklistBankKeyInvalid.prop("checked") == false) {
            ceklistBankKeyInvalid.click();
            ceklistBankKeyInvalid.prop("disabled", true);
        } else if (isChecked == false && ceklistBankKeyInvalid.prop("checked") == true) {
            ceklistBankKeyInvalid.click();
            ceklistBankKeyInvalid.prop("disabled", false);
        } else if (isChecked == true && ceklistBankKeyInvalid.prop("checked") == true) {
            ceklistBankKeyInvalid.prop("disabled", true);
        }

        if (isChecked == true && ceklistBankNameInvalid.prop("checked") == false) {
            ceklistBankNameInvalid.click();
            ceklistBankNameInvalid.prop("disabled", true);
        } else if (isChecked == false && ceklistBankNameInvalid.prop("checked") == true) {
            ceklistBankNameInvalid.click();
            ceklistBankNameInvalid.prop("disabled", false);
        } else if (isChecked == true && ceklistBankNameInvalid.prop("checked") == true) {
            ceklistBankNameInvalid.prop("disabled", true);
        }
    });
};