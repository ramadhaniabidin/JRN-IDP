
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
    if (moduleCategory === "Non Commercial" && !statusRules[moduleCode].includes(formStatus)) {
        HideAttachmentControl();
        AttachmentOpenNewTab();
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
    if (moduleCategory === "Non Commercial" && !statusRules[moduleCode].includes(formStatus)) {
        HideAttachmentControl();
        AttachmentOpenNewTab();
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
        let objek = new Object();

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


};


function onSuccessGetWorkflowHistoryLog(response) {
    let trHTML = '';
    const parsedData = JSON.parse(response.d);
    console.log("Parse Data = ", parsedData);
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
    console.log(trHTML);
    $("#tblHistory > tbody").append(trHTML);
    console.log($('#tblHistory').html());
};

function PartnerBankCheck() {
    const main = NWF$("#" + chkPartnerBank);
    const children = [
        NWF$("#" + chkAccountNoInvalid),
        NWF$("#" + chkAccountNameInvalid),
        NWF$("#" + chkBankKeyInvalid),
        NWF$("#" + chkBankNameInvalid)
    ];
    main.click(() => {
        const isChecked = main.prop("checked");
        children.forEach(cb => {
            cb.prop("checked", isChecked);
            cb.prop("disabled", isChecked);
        });
    });
};