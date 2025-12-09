NWF.FormFiller.Events.RegisterAfterReady(function () {
    defaultOnloadForm();
    SetAttachmentLink();
    GetWorkflowHistoryLog();
});


NWF.FormFiller.Events.RegisterRepeaterRowAdded(function () {
    defaultOnloadForm();
    SetAttachmentLink();
    GetWorkflowHistoryLog();
});

function GetWorkflowHistoryLog() {
    try {
        const objek = {
            Form_No: NWF$('#' + textFormNo).val(),
            Transaction_ID: NWF$("#" + cvItemID).val(),
            Module_Code: NWF$("#" + cvModuleCode).val()
        };
        LoadHistoryLog(objek);
    }
    catch (err) {
        console.log('GetWorkflowHistoryList : ' + err.message);
    }
};

async function LoadHistoryLog(param) {
    try {
        const response = await fetch("/_layouts/15/Daikin.Application/WebServices/NACWebService.asmx/GetHistoryLog2", {
            method: "POST",
            headers: { "Content-Type": "application/json; charset=utf-8" },
            body: JSON.stringify(param)
        });
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        const jsonData = JSON.parse(data.d);
        HandleHistoryLogSuccess(jsonData);
    }
    catch (err) {
        console.log(err);
    }
};

function HandleHistoryLogSuccess(jsonData) {
    let trHTML = "";
    const tblHistory = document.getElementById("tblHistory");
    const tbody = tblHistory.querySelector("tbody");
    const rows = tblHistory.querySelectorAll("tr:not(:first-child)");
    rows.forEach(row => row.remove());
    jsonData.forEach(item => {
        trHTML += `<tr>
            <td>${item.Personal_Name}</td>
            <td>${item.Position}</td>
            <td>${item.Comments}</td>
            <td>${item.Action_Name}</td>
            <td>${item.Action_Date}</td>
        </tr>`
    });
    tbody.insertAdjacentHTML('beforeend', trHTML);
};


function defaultOnloadForm() {
    $("#" + Comment).val("");
    $('.btnApproval input[type="radio"]').prop('checked', false);
    NWF$("#s4-ribbonrow").hide();
};

function deleteRepeaterRow(rowNo) {
    const row = NWF$(".details .nf-repeater-row:not('.nf-repeater-row-hidden')").eq(rowNo);
    row.find(".nf-repeater-deleterow-image").css("display", "none");
};

function attachmentOpenNewTab() {
    NWF$('.atcNewTab').click(function () {
        NWF$('tbody tr td a').attr("target", "_blank");
    });
};

function SetAttachmentLink() {
    if (NWF$('#' + cvFormStatus).val() == '' || NWF$('#' + cvFormStatus).val() == 'Revise' || NWF$('#' + cvFormStatus).val() == 'Draft') {
        $(".nf-attachmentsLink").css("display", "block");
        $(".propertysheet").css("display", "block");
    } else {
        $(".nf-attachmentsLink").css("display", "none");
        $(".propertysheet").css("display", "none");
        $(".nf-attachmentsRow a").attr("target", "_blank");
    }
};

