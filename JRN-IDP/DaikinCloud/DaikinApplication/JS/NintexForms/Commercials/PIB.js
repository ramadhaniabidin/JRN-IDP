let dialog;
let CurPage = 1;
let TotalPage = 1;

let objRS;

NWF$().ready(function () {
    //$("#fieldName").prop("readonly", true);
    NWF$('#' + txt1).prop('readonly', true);
    NWF$('#' + txt2).prop('readonly', true);
    NWF$('#' + txtRemarks).prop('readonly', true);
    NWF$("#s4-ribbonrow").css("display", "none");

    //$("p").css("background-color", "yellow");
    NWF$('#' + txt1).css('background-color', '#DCDCDC');
    NWF$('#' + txt2).css('background-color', '#DCDCDC');
    NWF$('#' + txtRemarks).css('background-color', '#DCDCDC');

    NWF$('.number').blur(function () {

        if (NWF$(this).val() == 0) {
            NWF$(this).val(0);
        } else {
            if (NWF$(this).val().length > 0) {
                NWF$(this).val(NWF$(this).val().replace(/,/g, ''));
                let amount = NWF$(this).val();
                NWF$(this).val(addCommas(amount));
            }
        }
    });

    NWF$('.number').on('focus', function () {
        if (NWF$(this).val() == 0) {
            NWF$(this).val(' ');
        } else {
            NWF$(this).val(NWF$(this).val().replace(/,/g, ''));
        }
    });

    NWF$('.number').keypress(function (evt) {
        OnlyNumberAlsoDecimals(evt)
    });

    NWF$('#' + Comment).val("");
    $('.btnApproval input[type="radio"]').prop('checked', false);
    GetWorkflowHistoryList();

    //$title = NWF$('#' + title).val();
    //console.log(NWF$(('.title').val()));
    //console.log($title)
    //if ($title !== null) {
    //   // NWF$('#' + popUp).prop("hidden", true);
    //}
});



/**************************REPEATING SECTION********************************/
function selectItems_ForRS(el) {
    console.log(objRS);
    objRS.val(el.parentNode.parentNode.cells[1].innerHTML);
    dialog.dialog("close");

}

NWF.FormFiller.Events.RegisterRepeaterRowAdded(function () {
    NWF$(".repeatedSection .nf-repeater-row:not('.nf-repeater-row-hidden')").each(function () {
        let row = NWF$(this);
        let inp_target = row.find('.target input');
        row.find('.img-lookup').click(function (e) {
            e.preventDefault();
            showDialog(inp_target);
            Search();
        });
    });

    NWF$('#' + Comment).val("");
    $('.btnApproval input[type="radio"]').prop('checked', false);
    GetWorkflowHistoryList();

});
/**************************REPEATING SECTION********************************/

function showDialog(o) {
    dialog = $("#dialog").dialog({
        height: 500,
        width: 500,
        //modal: true,
    });

    if (o !== undefined) {
        objRS = o;
    }

}

$('.img-lookup').click(function (e) {
    e.preventDefault();
    showDialog();
    Search();
});

function PopUp_Dialog() {
    showDialog();
    Search();
};

NWF$('#' + pibNumber).blur(function (e) {
    e.preventDefault();

    if (NWF$('#' + OtherInvoice).val().trim() == "") {
        NWF$('#' + txtRemarks).val(NWF$('#' + pibNumber).val() + " INV " + NWF$('#' + txt1).val());
    } else {
        NWF$('#' + txtRemarks).val(NWF$('#' + pibNumber).val() + " INV " + NWF$('#' + txt1).val() + "," + NWF$('#' + OtherInvoice).val());
    }

    let control3 = NWF$("#" + txtRemarks);
    NWF.FormFiller.Functions.ProcessOnChange(control3);

});

NWF$('#' + OtherInvoice).blur(function (e) {
    e.preventDefault();
    if (NWF$('#' + OtherInvoice).val().trim() == "") {
        NWF$('#' + txtRemarks).val(NWF$('#' + pibNumber).val() + " INV " + NWF$('#' + txt1).val());
    } else {
        NWF$('#' + txtRemarks).val(NWF$('#' + pibNumber).val() + " INV " + NWF$('#' + txt1).val() + "," + NWF$('#' + OtherInvoice).val());
    }

    let control3 = NWF$("#" + txtRemarks);
    NWF.FormFiller.Functions.ProcessOnChange(control3);

});

function selectItems(el) {
    try {
        let Inv_No = el.parentNode.parentNode.cells[1].innerHTML;
        let BL_No = el.parentNode.parentNode.cells[2].innerHTML;
        NWF$('#' + txt2).val(BL_No);

        let param = {
            Keywords: BL_No == undefined ? '' : BL_No
        };
        const InvoiceCommercial = [];
        console.log(param);
        $.ajax({
            type: "POST",
            contentType: "application/json; charset=utf-8",
            url: "/_layouts/15/Daikin.application/webservices/PopList.asmx/ListData_InvoiceCommercialNumberOnly",
            data: JSON.stringify(param),
            dataType: "json",
            async: false,
            success: function (data) {
                console.log(data.d);
                let jsonData = JSON.parse(data.d);

                $.each(jsonData.Items, function () {
                    InvoiceCommercial.push(this.Ref_No);
                });
                console.log(InvoiceCommercial);

                let listInvNoSpace = InvoiceCommercial.join(' ');
                let listInvNoComma = InvoiceCommercial.join(',');

                if (NWF$('#' + OtherInvoice).val().trim() == "") {
                    NWF$('#' + txtRemarks).val(NWF$('#' + pibNumber).val() + " INV " + NWF$('#' + txt1).val());
                } else {
                    NWF$('#' + txtRemarks).val(NWF$('#' + pibNumber).val() + " INV " + NWF$('#' + txt1).val() + "," + NWF$('#' + OtherInvoice).val());
                }

                NWF$('#' + txt1).val(listInvNoComma);


            },
            error: function (xhr) {
                alert(xhr.responseText);
            }
        });


        console.log(NWF$('#' + pibNumber).val());

        let control = NWF$("#" + txt1);
        NWF.FormFiller.Functions.ProcessOnChange(control);

        let control2 = NWF$("#" + txt2);
        NWF.FormFiller.Functions.ProcessOnChange(control2);

        let control3 = NWF$("#" + txtRemarks);
        NWF.FormFiller.Functions.ProcessOnChange(control3);

        dialog.dialog("close");
    } catch (e) {
        console.log(e.message);
    }

}

/**********************PAGINATION DATA***********************************/

function Prev() {
    if (CurPage > 1) {
        CurPage = CurPage - 1;
        ListData_InvoiceNumber(CurPage, $('#Keywords').val());
    } else {
        CurPage = 1;
        ListData_InvoiceNumber(CurPage, $('#Keywords').val());
    }
}

function Search() {
    CurPage = 1;
    ListData_InvoiceNumber(CurPage, $('#Keywords').val());
}

function Next() {
    CurPage++;
    ListData_InvoiceNumber(CurPage, $('#Keywords').val());
}

function GetWorkflowHistoryList() {
    try {
        let objek = new Object();
        objek.Form_No = NWF$('#' + textFormNo).val();
        objek.Transaction_ID = NWF$('.trans_id input').val();
        objek.Module_Code = "M025";

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
    let trHTML = '';
    let parsedData = JSON.parse(response.d);
    console.log("Parse Data = ", parsedData);
    let TotalRows = $("#tblHistory").find("tr:not(:first)").length;
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


function ListData_InvoiceNumber(PageIndex, Keywords) {
    let param = {
        PageIndex: PageIndex,
        Keywords: Keywords == undefined ? '' : Keywords,
    };
    console.log(param);
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/_layouts/15/Daikin.application/webservices/PopList.asmx/ListData_InvoiceCommercialNumber",
        data: JSON.stringify(param),
        dataType: "json",
        async: false,
        success: function (data) {
            let trHTML = '';
            let r = 0;
            console.log(data.d, 'data.d');
            let jsonData = JSON.parse(data.d);

            $('#RecordCount').val(jsonData.RecordCount);
            let TotalRecords = jsonData.RecordCount;

            // old logic for TotalPage
            // TotalPage = TotalRecords / 5;
            // TotalPage = Math.ceil(TotalPage.toFixed(1));

            TotalPage = Math.ceil(TotalRecords / 5);

            let TotalRows = $("#tblPop tbody").find("tr").length;
            if (TotalRows > 0) {
                $("#tblPop tbody").find("tr").remove();
            }

            trHTML = '<tbody>';
            const InvoiceCommercial = [];
            $.each(jsonData.Items, function () {
                InvoiceCommercial.push(this.Ref_No);


            });
            $.each(jsonData.Items, function () {
                trHTML += '<tr>';
                trHTML += '<td>' + this.No + '</td>';
                trHTML += '<td>' + this.Ref_No + '</td>';
                trHTML += '<td>' + this.BL_No + '</td>';

                trHTML += '<td align="center"><a style="cursor:pointer" onclick="selectItems(this);">Select</a></td>';

                trHTML += '</tr>';
            });
            console.log(InvoiceCommercial);
            trHTML += '</tbody>';
            $('#tblPop').append(trHTML);

            $('#info-paging').html(CurPage.toString() + ' to ' + TotalPage.toString() + ' of ' + TotalRecords.toString() + ' entries');
        },
        error: function (xhr) {
            alert(xhr.responseText);
        }
    });
}
/**********************PAGINATION DATA***********************************/
