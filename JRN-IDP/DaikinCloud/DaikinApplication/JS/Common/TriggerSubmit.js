var popUpDialog;
var page_CurrIdx = 1;
var page_Count = 1;
var dataResult = [];
var dataResult_Details = [];
var inputSelectors = [];
var transNumber = '';
var selectedItem = [];
var sumTotalValue = 0;
var vendorName;
var itemsRS_currSelected;

var options = [];
var tblHeaders = [];
var tblName = "";
var moduleName = "";
var trgtID = "";
var trgtCol = [];
var page_count = 0;
var detailTable = "";
var ColumnToFilter = "";
var FilterValue = "";
var targetID_Detail = [];
var targetCol_Detail = [];
let totalItemCnt;

NWF.FormFiller.Events.RegisterAfterReady(function () {
    console.log("Masuk Condition Business Relation")
    var control = NWF$("#" + Approvers);
    control.val('Approver1; Approver2;');
    control.trigger('change')
    NWF.FormFiller.Functions.ProcessOnChange(control);
    $('#' + Comment).val("")
    $('.btnApproval input[type="radio"]').prop('checked', false);
    //NWF$("#RibbonContainer").hide();
    NWF$("#s4-ribbonrow").hide();
    //NWF$('.atcNewTab').click(function () {
    //    NWF$('tbody tr td a').attr("target", "_blank");
    //});
    if ($('#' + ListName).val() == "Affiliate Not Claim") {
        deleteRepeaterRow(0)
        deleteRepeaterRow(1)
        deleteRepeaterRow(2)
        deleteRepeaterRow(3)
        NWF$('#' + PIB).change(function () {
            var PIB = NWF$('.PIB label').html()
            console.log(PIB)
            if (PIB == "No") {
                deleteAmountAffiliate(0)
                deleteAmountAffiliate(1)
                deleteAmountAffiliate(2)
                deleteAmountAffiliate(3)
            }
        });
        NWF$('#' + BPNPaidBy).change(function () {
            var BPNPaid = NWF$('#' + BPNPaidBy).val()
            var PIB = NWF$('#' + PIB).val()
            if (BPNPaid.includes("Vendor")) {
                deleteAmountAffiliate(1)
                deleteAmountAffiliate(2)
            } else if (BPNPaid.includes("Daikin")) {
                deleteAmountAffiliate(3)
            }
        });

    }
    if ($('#' + ListName).val() == "Business Relation" && $('#' + ItemID).val() != null && $('#' + ItemID).val() != "") {
        console.log("Masuk Condition Business Relation")
        //NWF$('.atcNewTab').click(function () {
        //    NWF$('tbody tr td a').attr("target", "_blank");
        //});
        setAttachmentSelfie($('#' + ItemID).val())
    }
    if ($('#' + ItemID).val() != null && $('#' + ItemID).val() != "") {
        //console.log('Masuk Sini')
        var control = NWF$("#" + Approvers);
        control.val('Approver1; Approver2;');
        control.trigger('change')
        NWF.FormFiller.Functions.ProcessOnChange(control);
        //getApprover($('#' + ListName).val(), $('#' + ItemID).val())
        //NWF$('.atcNewTab').click(function () {
        //    NWF$('tbody tr td a').attr("target", "_blank");
        //});
    }
    if ($('#' + ItemID).val() != null && $('#' + ItemID).val() != "" && NWF$('#' + cvFormStatus).val() != 'Revise') {
        NWF$('.atcNewTab').click(function () {
            NWF$('tbody tr td a').attr("target", "_blank");
        });
    }

    var repeatingSection = NWF$(".repeatingSection .nf-repeater-row:not('.nf-repeater-row-hidden')");
    repeatingSection.each(function () {
        var row = NWF$(this);
        NWF$(this).find('.pop-up').on('click', function () {
            PopUp_ShowDialog(null, "Material Anaplan", row);
        });
    });

    var poRS = NWF$(".poRS .nf-repeater-row:not('.nf-repeater-row-hidden')");
    poRS.each(function () {
        var poRSRow = NWF$(this);
        NWF$(this).find(".pop-up").on("click", function () {
            PopUp_ShowDialog(null, "Cost Center", poRSRow);
        });
    });

    var ANCRS = NWF$(".ANCRS .nf-repeater-row:not('.nf-repeater-row-hidden')");
    ANCRS.each(function () {
        var ANCRSRow = NWF$(this);
        NWF$(this).find(".pop-up").on("click", function () {
            PopUp_ShowDialog(null, "ANC Vendor Bank RS", ANCRSRow);
        });
    });

    //GetUserProfile();
});

NWF.FormFiller.Events.RegisterRepeaterRowAdded(function () {
    $('.btnApproval input[type="radio"]').prop('checked', false);
    var repeatingSection = NWF$(".repeatingSection .nf-repeater-row:not('.nf-repeater-row-hidden')");
    repeatingSection.each(function () {
        var row = NWF$(this);
        NWF$(this).find('.pop-up').on('click', function () {
            PopUp_ShowDialog(null, "Material Anaplan", row);
        });
    });

    var poRS = NWF$(".poRS .nf-repeater-row:not('.nf-repeater-row-hidden')");
    poRS.each(function () {
        var poRSRow = NWF$(this);
        NWF$(this).find(".pop-up").on("click", function () {
            PopUp_ShowDialog(null, "Cost Center", poRSRow);
        });
    });

    var ANCRS = NWF$(".ANCRS .nf-repeater-row:not('.nf-repeater-row-hidden')");
    ANCRS.each(function () {
        var ANCRSRow = NWF$(this);
        NWF$(this).find(".pop-up").on("click", function () {
            PopUp_ShowDialog(null, "ANC Vendor Bank RS", ANCRSRow);
        });
    });
});

function GetUserProfile() {
    var param = {
        'logon': NWF$("#" + current_user).val()
        //logon: 'test1'
    };

    console.log(param);

    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "_layouts/15/Daikin.Application/WebServices/NACWebService.asmx/GetLoginAttributes",
        data: JSON.stringify(param),
        dataType: "json",
        async: true,
        success: function (data) {
            var jsonData = JSON.parse(data.d);
            console.log(jsonData);
            if (jsonData.Success) {
                $(".requester_name input").val(jsonData.CurrLoginName);
                $(".requester_name input").trigger("click");
                $(".requester_name input").trigger("blur");

                $(".requester_dept input").val(jsonData.Department);
                $(".requester_dept input").trigger("click");
                $(".requester_dept input").trigger("blur");
            }
        }
    });
};

function deleteAmountAffiliate(NoRow) {
    var row = NWF$(".details .nf-repeater-row:not('.nf-repeater-row-hidden')").eq(NoRow)
    row.find(".taxBase input").val('')
    row.find(".amount input").val('')
    row.find(".amount label").html('')
}

function deleteRepeaterRow(NoRow) {
    var row = NWF$(".details .nf-repeater-row:not('.nf-repeater-row-hidden')").eq(NoRow)
    row.find(".nf-repeater-deleterow-image").css("display", "none");
}

function setAttachmentSelfie(Item_ID) {
    const param = {
        Item_ID: Item_ID,
    };
    console.log(param)
    try {
        jQuery.ajax({
            type: "POST",
            contentType: "application/json; charset=utf-8",
            url: "/_layouts/15/Daikin.Application/WebServices/NACWebService.asmx/GetAttachmentSelfie",
            data: JSON.stringify(param),
            dataType: "json",
            async: true,
            success: function (data) {
                var JSONData = JSON.parse(data.d)
                var atc = JSONData.dataATC
                console.log('AtcSelfie Data: ', atc)
                atc.forEach((dataAtc) => {
                    console.log(dataAtc, "dataperATC")
                    var url = `<a href="https://spdev:3473/Lists/Business%20Relation/Attachments/` + Item_ID + `/` + dataAtc.Attachment_Selfie + `" target="_blank"><img width="200px" src="http://spdev:3473/Lists/Business%20Relation/Attachments/` + Item_ID + `/` + dataAtc.Attachment_Selfie + `"></a>"`
                    console.log(url)
                    var row = NWF$(".details .nf-repeater-row:not('.nf-repeater-row-hidden')").eq(dataAtc.No - 1);
                    row.find(".atcselfietesting").html(url)
                });
            },
            error: function (xhr) {
                console.log('Error: ' + xhr.responsename)
                alert(xhr.responsename);
            }
        })
    }
    catch (ex) {
        alert(ex.message)
    }
}

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
                console.log('Approver: ' + data.d)
                var control = NWF$("#" + Approvers);
                control.val(data.d);
                control.trigger('change')
                NWF.FormFiller.Functions.ProcessOnChange(control);
            },
            error: function (xhr) {
                console.log('Error: ' + xhr.responsename)
                alert(xhr.responsename);
            }
        })
    }
    catch (ex) {
        alert(ex.message)
    }

}

function SaveApproval(approvalValue, ListName, ListItemID) {
    const param = {
        approvalValue: approvalValue,
        ListName: ListName,
        ListItemID: ListItemID,
        comments: NWF$('#' + Comment).val(),
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
                console.log('Error: ' + xhr.responsename)
                alert(xhr.responsename);
            }
        })
    }
    catch (ex) {
        alert(ex.message)
    }
}
function SaveApprovalNonCom(approvalValue, ListName, ListItemID, HeaderID) {
    const param = {
        approvalValue: approvalValue,
        ListName: ListName,
        ListItemID: ListItemID,
        HeaderID: HeaderID,
        comments: NWF$('#' + Comment).val(),
    };

    console.log('Save & Submit Triggered.');

    console.log('PARAM', param)
    try {
        jQuery.ajax({
            type: "POST",
            contentType: "application/json; charset=utf-8",
            url: "/_layouts/15/Daikin.Application/WebServices/NACWebService.asmx/ApproveRequestNonCom",
            data: JSON.stringify(param),
            dataType: "json",
            async: true,
            success: function () {
                console.log('Your Response have been recorded!');
            },
            error: function (xhr) {
                console.log('Error: ' + xhr.responsename)
                alert(xhr.responsename);
            }
        })
    }
    catch (ex) {
        alert(ex.message)
    }

}

function PopUp_ShowDialog(shownPopup, module, currentRow) {
    popUpDialog = $("#PopUp_Dialog").dialog({
        height: 650,
        width: 1000,
        title: "Select : " + module
    });

    if (module == "Material Anaplan") {
        tblName = "dbo.MasterMaterialAnaplan"
        moduleName = module;
        options = [
            { value: "Material_Code", name: "Kode Material" },
            { value: "Concatenate_GA", name: "Material" },
            { value: "Procurement_Department_Title", name: "Procurement Department" },
        ];
        tblHeaders = [
            { db_col: "Material_Code", name: "Kode Material" },
            { db_col: "Concatenate_GA", name: "Material" },
            { db_col: "Procurement_Department_Title", name: "Procurement Department" },
        ];

        trgtID = ["mat_anaplan"];
        trgtCol = ["Concatenate_GA"];

    }
    else if (module == "Cost Center") {
        tblName = "dbo.MasterMappingCostCenter"
        moduleName = module;

        options = [
            { value: "Cost_Center", name: "Cost Center" },
            { value: "Description", name: "Description" },
            { value: "Branch", name: "Branch" },
            { value: "Combine", name: "Combine" },
        ];
        tblHeaders = [
            { db_col: "Cost_Center", name: "Cost Center" },
            { db_col: "Description", name: "Description" },
            { db_col: "Branch", name: "Branch" },
            { db_col: "Combine", name: "Combine" },
        ];

        trgtID = ["cost-center", "cc_code"];
        trgtCol = ["Combine", "Cost_Center"];
    }
    else if (module == "ANC Vendor Bank") {
        tblName = "dbo.MasterVendorBankAffiliate"
        moduleName = module;

        options = [
            { value: "Vendor_Name", name: "Vendor Name" },
            { value: "Vendor_Number", name: "Vendor Number" },
            { value: "Bank_Key", name: "Bank Key" },
            { value: "Bank_Account_No", name: "Account No" },
            { value: "Bank_Account_Name", name: "Account Name" },
            { value: "Bank_Name", name: "Bank Name" },
            { value: "Partner_Bank", name: "Partner Bank" },
        ];
        tblHeaders = [
            { db_col: "Vendor_Name", name: "Vendor Name" },
            { db_col: "Vendor_Number", name: "Vendor Number" },
            { db_col: "Bank_Key", name: "Bank Key" },
            { db_col: "Bank_Account_No", name: "Account No" },
            { db_col: "Bank_Account_Name", name: "Account Name" },
            { db_col: "Bank_Name", name: "Bank Name" },
            { db_col: "Partner_Bank", name: "Partner Bank" },
        ];

        trgtID = [VendorName, VendorNumber, BankKey, BankAccountNo, BankAccountName, BankName, PartnerBank];
        trgtCol = ["Vendor_Name", "Vendor_Number", "Bank_Key", "Bank_Account_No", "Bank_Account_Name", "Bank_Name", "Partner_Bank"];
    }
    else if (module == 'ANC Vendor Bank RS') {
        tblName = "dbo.MasterVendorBankAffiliate"
        moduleName = module;

        options = [
            { value: "Vendor_Name", name: "Vendor Name" },
            { value: "Vendor_Number", name: "Vendor Number" },
            { value: "Bank_Key", name: "Bank Key" },
            { value: "Bank_Account_No", name: "Account No" },
            { value: "Bank_Account_Name", name: "Account Name" },
            { value: "Bank_Name", name: "Bank Name" },
            { value: "Partner_Bank", name: "Partner Bank" },
        ];
        tblHeaders = [
            { db_col: "Vendor_Name", name: "Vendor Name" },
            { db_col: "Vendor_Number", name: "Vendor Number" },
            { db_col: "Bank_Key", name: "Bank Key" },
            { db_col: "Bank_Account_No", name: "Account No" },
            { db_col: "Bank_Account_Name", name: "Account Name" },
            { db_col: "Bank_Name", name: "Bank Name" },
            { db_col: "Partner_Bank", name: "Partner Bank" },
        ];

        trgtID = ["VendorName"];
        trgtCol = ["Vendor_Name"];
    }


    console.log("Pop up Module: " + module);
    $('#PopUp_Dropdown').val('');
    $('#PopUp_Keyword').val('');

    if (currentRow != null) {
        itemsRS_currSelected = currentRow;
    }
    console.log("Current row: ", itemsRS_currSelected);

    $('#PopUp_Dropdown').html('');
    $.each(options, function (i, option) {
        $('#PopUp_Dropdown').append($('<option>', {
            text: option.name,
            value: option.value
        }));
    });

    $('#PopUp_TblHeader').html('');
    $.each(tblHeaders, function (i, header) {
        $('#PopUp_TblHeader').append($('<th>', {
            scope: "col",
            style: "text-align: center; color: white; background-color: #0072c6; padding: 10px;",
            text: header.name,
            colspan: 3
        }));
    });
    $('#PopUp_TblHeader').append($('<th>', {
        scope: "col",
        style: "text-align: center; color: white; background-color: #0072c6; padding: 10px;",
        name: "",
        colspan: 2
    }));

    PopUp_Search();
};

function SearchHelper(event) {
    if (event.key === 'Enter') {
        PopUp_Search();
    }
};

function PopUp_Search() {
    page_CurrIdx = 1;
    PopUp_List(page_CurrIdx, $('#PopUp_Dropdown').val(), $('#PopUp_Keyword').val(), selectedItem);
};

function PopUp_Prev() {
    if (page_CurrIdx > 1) {
        page_CurrIdx = page_CurrIdx - 1;
        PopUp_List(page_CurrIdx, $('#PopUp_Dropdown').val(), $('#PopUp_Keyword').val(), selectedItem);
    }
};

function PopUp_Next() {
    if (page_CurrIdx < page_count) {
        page_CurrIdx++;
        PopUp_List(page_CurrIdx, $('#PopUp_Dropdown').val(), $('#PopUp_Keyword').val(), selectedItem);
    }
};

function PopUp_List(PageIndex, SearchBy, Keywords, SelectedItem) {
    try {
        console.log('Selected item : ', SelectedItem);
        $('#PopUp_TableBody').html('');
        if ((Keywords != undefined) || (Keywords != '')) {
            Keywords = $('#PopUp_Keyword').val();
        }
        else {
            Keywords = '';
        }

        if ((SearchBy != undefined) || (SearchBy != '')) {
            SearchBy = $('#PopUp_Dropdown').val();
        }
        else {
            SearchBy = '';
        }

        if (moduleName == "Material Anaplan") {
            SearchBy += ";Procurement_Department_Title";
            Keywords += ";" + $(".proc_dept").find('input').val();
        }

        else if (moduleName == "Cost Center") {
            SearchBy += ";Branch";
            Keywords += ";" + $(".branch").find("label").html();
        }

        else if(moduleName === "ANC Vendor Bank")
        {
            SearchBy += ";Vendor_Name;Active";
            Keywords += ";bea cukai;1";
        }

        else if(moduleName === "ANC Vendor Bank RS")
        {
            SearchBy += ";Vendor_Name;Active";
            Keywords += ";bea cukai;1";
        }
    }
    catch (err) {
        console.log("PopUp Error: PopUp_List() \n" + err);
    }

    var param = {
        input: {
            searchTabl: tblName,
            searchCol: SearchBy,
            searchVal: Keywords,
            searchLike: 1,
            pageIndx: PageIndex,
            pageSize: 10,
        },

        output: {
            RecordCount: 0,
        }
    };
    console.log('Parameters: ', param);

    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "_layouts/15/Daikin.Application/WebServices/PopList.asmx/PopUpListData",
        data: JSON.stringify(param),
        dataType: "json",
        async: true,
        success: function (data) {
            const tBodyHTML = document.getElementById("PopUp_TableBody");
            $('#PopUp_TableBody').html('');

            var jsonData = JSON.parse(data.d);
            dataResult = jsonData.Logs;
            console.log(dataResult, 'log tesar')

            var i = 0;
            $.each(jsonData.Logs, function (key, values) {
                var dataRow = document.createElement("tr");
                var dataCol = document.createElement("td");

                $.each(tblHeaders, function (HeaderID, data) {
                    dataCol = document.createElement("td");
                    dataCol.setAttribute("colspan", 3);
                    dataCol.setAttribute("style", "text-align:center");

                    var value = values.filter(e => e.Key == data.db_col)[0].Value;
                    if (typeof value === 'string' && value.indexOf("/Date(") >= 0) {
                        value = value.substring(6, 19);
                    } else {
                        dataCol.innerHTML = value;
                    }

                    dataRow.appendChild(dataCol);
                });

                var colFour = document.createElement("td");
                colFour.setAttribute("colspan", 2);
                colFour.setAttribute("style", "text-align:center;cursor:pointer;");
                colFour.innerHTML = "<a style=\"color:blue;\" class=\"action-name\" onclick=\"PopUp_SelectItem(" + i + ")\">SELECT<\/a>";

                dataRow.appendChild(colFour);
                tBodyHTML.appendChild(dataRow);
                i++;
            });
            if (parseInt(jsonData.TotalRecords) >= 0) {
                totalItemCnt = parseInt(jsonData.TotalRecords);
            }
            else {
                totalItemCnt = 0;
            }

            page_count = Math.ceil(totalItemCnt / param.input.pageSize);

            $('#info-paging_items').html('Page : ' + PageIndex.toString() + ' of ' + page_count.toString() + ' | ' + totalItemCnt.toString() + ' Results');
            i++
        },
        error: function (xhr) {
            alert(xhr.responsename);
        }
    });


};

function PopUp_SelectItem(id) {
    console.log(dataResult[id]);
    if (moduleName == "Material Anaplan") {
        if ((itemsRS_currSelected !== null) && (itemsRS_currSelected !== undefined)) {
            $.each(trgtCol, function (index, value) {
                itemsRS_currSelected.find("." + trgtID[index] + " input").val(dataResult[id].filter(x => x.Key == value)[0].Value);
                itemsRS_currSelected.find("." + trgtID[index] + " input").trigger("click");
                itemsRS_currSelected.find("." + trgtID[index] + " input").trigger("blur");
            });
            itemsRS_currSelected = null;
        }
    }
    else if (moduleName == "Cost Center") {
        if ((itemsRS_currSelected !== null) && (itemsRS_currSelected !== undefined)) {
            $.each(trgtCol, function (index, value) {
                itemsRS_currSelected.find("." + trgtID[index] + " input").val(dataResult[id].filter(x => x.Key == value)[0].Value);
                itemsRS_currSelected.find("." + trgtID[index] + " input").trigger("click");
                itemsRS_currSelected.find("." + trgtID[index] + " input").trigger("blur");
            });
            itemsRS_currSelected = null;
        }
    }
    else if (moduleName == "ANC Vendor Bank") {
        $.each(trgtCol, function (index, values) {
            var resultValue;
            if ((dataResult[id].filter(e => e.Key == values)[0] !== null) && (dataResult[id].filter(e => e.Key == values)[0] !== undefined)) {
                resultValue = dataResult[id].filter(e => e.Key == values)[0].Value;
            }

            else if ((dataResult[id].filter(e => e.Key == values)[0] == null) || (dataResult[id].filter(e => e.Key == values)[0] == undefined)) {
                resultValue = "";
            }
            console.log('606', resultValue, trgtID[index], moduleName)
            if (typeof resultValue === 'string' && resultValue.startsWith('/Date(') && resultValue.endsWith(')/')) {
                var timestamp = parseInt(resultValue.match(/\/Date\((\d+)\)\//)[1], 10);
                var formattedDate = new Date(timestamp).toLocaleDateString('en-US', { year: 'numeric', month: '2-digit', day: '2-digit' });
                NWF$('#' + trgtID[index]).val(formattedDate);
            } else {
                NWF$('#' + trgtID[index]).val(resultValue);
            }
        });
    }
    else if (moduleName == "ANC Vendor Bank RS") {
        console.log('22222')
        if ((itemsRS_currSelected !== null) && (itemsRS_currSelected !== undefined)) {
            console.log('1111')
            $.each(trgtCol, function (index, value) {
                itemsRS_currSelected.find("." + trgtID[index] + " input").val(dataResult[id].filter(x => x.Key == value)[0].Value);
                itemsRS_currSelected.find("." + trgtID[index] + " input").trigger("click");
                itemsRS_currSelected.find("." + trgtID[index] + " input").trigger("blur");
            });
            itemsRS_currSelected = null;
        }
    }

    popUpDialog.dialog("close");

    options = [];
    tblHeaders = [];
    tblName = "";
    trgtID = '';
    trgtCol = "";
};